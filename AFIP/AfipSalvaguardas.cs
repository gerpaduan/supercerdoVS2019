// Piezas de proteccion para usar ARCA sin exponerse a bloqueos ni condiciones de carrera. Sin dependencias
// de web ni de base: se prueban en AFIP.Tests con un reloj inyectable.
//   - AfipTicketCache: un solo pedido de ticket WSAA a la vez por archivo (ARCA no entrega otro ticket mientras
//     hay uno vigente), escritura atomica y reuso del ticket entre todos los que lo necesiten.
//   - AfipLimitador: tope de consultas por clave (p. ej. por empresa) en una ventana de tiempo.
//   - AfipCorteAutomatico: tras N fallos seguidos "corta" un recurso un rato para no insistir contra un servicio caido.
//   - AfipCacheTemporal: cache con vencimiento.
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;

namespace AFIP
{
    public static class AfipTicketCache
    {
        private static readonly ConcurrentDictionary<string, object> Candados =
            new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        // Devuelve el ticket vigente guardado en rutaTicket o, si no hay (o esta vencido), lo pide UNA sola vez.
        // Si varios hilos llegan a la vez con el ticket vencido, uno lo pide y los demas esperan y reutilizan el
        // nuevo: un ticket vigente lo puede usar cualquiera al mismo tiempo, lo unico que ARCA impide es pedir
        // otro mientras hay uno valido. El candado es por archivo, asi que no bloquea a otras empresas.
        public static string ObtenerOPedir(string rutaTicket, Func<string, bool> estaActivo, Func<string> pedir)
        {
            if (string.IsNullOrWhiteSpace(rutaTicket)) throw new ArgumentException("Falta la ruta del ticket.", nameof(rutaTicket));
            string clave = Path.GetFullPath(rutaTicket);

            lock (Candados.GetOrAdd(clave, _ => new object()))
            {
                if (File.Exists(rutaTicket))
                {
                    try
                    {
                        string existente = File.ReadAllText(rutaTicket);
                        if (estaActivo(existente)) return existente;
                    }
                    catch (IOException)
                    {
                        // Archivo tomado por otro proceso o a medio escribir: se pide uno nuevo.
                    }
                }

                string nuevo = pedir();
                EscribirAtomico(rutaTicket, nuevo);
                return nuevo;
            }
        }

        // Escribe a un temporal y lo mueve/reemplaza, para que nadie lea un ticket a medias.
        public static void EscribirAtomico(string ruta, string contenido)
        {
            string carpeta = Path.GetDirectoryName(ruta);
            if (!string.IsNullOrEmpty(carpeta)) Directory.CreateDirectory(carpeta);

            string temporal = ruta + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temporal, contenido);
            try
            {
                if (File.Exists(ruta)) File.Replace(temporal, ruta, null);
                else File.Move(temporal, ruta);
            }
            catch
            {
                if (File.Exists(temporal)) File.Delete(temporal);
                throw;
            }
        }
    }

    public class AfipLimitador
    {
        private readonly int _maximo;
        private readonly TimeSpan _ventana;
        private readonly Func<DateTime> _ahora;
        private readonly ConcurrentDictionary<string, Queue<DateTime>> _usos = new ConcurrentDictionary<string, Queue<DateTime>>();

        public AfipLimitador(int maximoPorVentana, TimeSpan ventana, Func<DateTime> ahora = null)
        {
            if (maximoPorVentana <= 0) throw new ArgumentOutOfRangeException(nameof(maximoPorVentana));
            _maximo = maximoPorVentana;
            _ventana = ventana;
            _ahora = ahora ?? (() => DateTime.UtcNow);
        }

        // true y registra el uso si la clave todavia no llego al tope dentro de la ventana; false si ya lo alcanzo.
        public bool IntentarUsar(string clave)
        {
            var cola = _usos.GetOrAdd(clave ?? "", _ => new Queue<DateTime>());
            lock (cola)
            {
                DateTime ahora = _ahora();
                while (cola.Count > 0 && ahora - cola.Peek() >= _ventana)
                    cola.Dequeue();

                if (cola.Count >= _maximo) return false;
                cola.Enqueue(ahora);
                return true;
            }
        }

        // Usos registrados dentro de la ventana actual (para mostrar en pantalla / auditoria).
        public int UsosEnVentana(string clave)
        {
            if (!_usos.TryGetValue(clave ?? "", out var cola)) return 0;
            lock (cola)
            {
                DateTime ahora = _ahora();
                int n = 0;
                foreach (DateTime uso in cola) if (ahora - uso < _ventana) n++;
                return n;
            }
        }
    }

    public class AfipCorteAutomatico
    {
        private readonly int _fallosParaCortar;
        private readonly TimeSpan _duracionCorte;
        private readonly Func<DateTime> _ahora;
        private readonly object _candado = new object();
        private int _fallosSeguidos;
        private DateTime _cortadoHasta = DateTime.MinValue;

        public AfipCorteAutomatico(int fallosParaCortar, TimeSpan duracionCorte, Func<DateTime> ahora = null)
        {
            if (fallosParaCortar <= 0) throw new ArgumentOutOfRangeException(nameof(fallosParaCortar));
            _fallosParaCortar = fallosParaCortar;
            _duracionCorte = duracionCorte;
            _ahora = ahora ?? (() => DateTime.UtcNow);
        }

        // true mientras dure el corte: no hay que usar el recurso.
        public bool Cortado
        {
            get { lock (_candado) { return _ahora() < _cortadoHasta; } }
        }

        public int FallosSeguidos
        {
            get { lock (_candado) { return _fallosSeguidos; } }
        }

        public DateTime? CortadoHasta
        {
            get { lock (_candado) { return _ahora() < _cortadoHasta ? (DateTime?)_cortadoHasta : null; } }
        }

        public void RegistrarExito()
        {
            lock (_candado)
            {
                _fallosSeguidos = 0;
                _cortadoHasta = DateTime.MinValue;
            }
        }

        // Al llegar al tope de fallos seguidos corta por _duracionCorte; pasado ese tiempo se vuelve a probar y,
        // si falla otra vez, corta de nuevo (el contador no se reinicia hasta un exito).
        public void RegistrarFallo()
        {
            lock (_candado)
            {
                _fallosSeguidos++;
                if (_fallosSeguidos >= _fallosParaCortar)
                    _cortadoHasta = _ahora() + _duracionCorte;
            }
        }
    }

    public class AfipCacheTemporal<T>
    {
        private readonly TimeSpan _vigencia;
        private readonly Func<DateTime> _ahora;
        private readonly ConcurrentDictionary<string, KeyValuePair<DateTime, T>> _items =
            new ConcurrentDictionary<string, KeyValuePair<DateTime, T>>();

        public AfipCacheTemporal(TimeSpan vigencia, Func<DateTime> ahora = null)
        {
            _vigencia = vigencia;
            _ahora = ahora ?? (() => DateTime.UtcNow);
        }

        public bool TryGet(string clave, out T valor)
        {
            if (_items.TryGetValue(clave ?? "", out var item) && _ahora() - item.Key < _vigencia)
            {
                valor = item.Value;
                return true;
            }
            valor = default(T);
            return false;
        }

        public void Set(string clave, T valor)
        {
            _items[clave ?? ""] = new KeyValuePair<DateTime, T>(_ahora(), valor);
        }
    }
}
