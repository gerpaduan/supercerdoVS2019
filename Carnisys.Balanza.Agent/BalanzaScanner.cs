using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;

namespace Carnisys.Balanza.Agent
{
    // Un puerto COM de la PC con el nombre que le pone Windows (ej. "USB-SERIAL CH340 (COM3)").
    internal sealed class PuertoInfo
    {
        public string Puerto { get; set; }
        public string Descripcion { get; set; }

        public string Titulo
        {
            get { return string.IsNullOrWhiteSpace(Descripcion) ? Puerto : Descripcion; }
        }
    }

    // Resultado de probar un puerto con las marcas soportadas.
    internal sealed class ResultadoPuerto
    {
        public string Puerto { get; set; }
        public bool Responde { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string PesoDisplay { get; set; }
        public string Detalle { get; set; }
    }

    // Lista los puertos COM y detecta en cual esta la balanza (probando cada puerto con cada marca).
    // Es estatico y no depende del lector: lo usan el instalador (antes de que exista el agente) y la ventana de configuracion
    // (que antes pausa el lector para que el puerto este libre, ver BalanzaReaderService.PausarLector).
    internal static class BalanzaScanner
    {
        // Espera tras pedir el peso. Las balanzas responden en < 100 ms; se deja margen para adaptadores USB-serie lentos.
        private const int EsperaRespuestaMs = 350;

        private static readonly Regex PatronCom = new Regex(@"\((COM\d+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Puertos presentes con su descripcion de Windows. Si WMI falla, quedan solo los nombres (COM3...).
        public static List<PuertoInfo> ListarPuertos()
        {
            var descripciones = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var busqueda = new ManagementObjectSearcher("SELECT Name FROM Win32_PnPEntity WHERE Name LIKE '%(COM%)'"))
                using (var resultados = busqueda.Get())
                {
                    foreach (ManagementBaseObject item in resultados)
                    {
                        string nombre = item["Name"] as string;
                        Match m = nombre != null ? PatronCom.Match(nombre) : null;
                        if (m != null && m.Success)
                        {
                            descripciones[m.Groups[1].Value.ToUpperInvariant()] = nombre.Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AgentLog.Warn("No se pudo leer la descripcion de los puertos (WMI): " + ex.Message);
            }

            string[] nombres;
            try
            {
                nombres = SerialPort.GetPortNames();
            }
            catch (Exception ex)
            {
                AgentLog.Warn("No se pudo listar los puertos COM: " + ex.Message);
                nombres = new string[0];
            }

            return nombres
                .Select(n => n.Trim().ToUpperInvariant())
                .Distinct()
                .OrderBy(n => NumeroCom(n))
                .Select(n => new PuertoInfo
                {
                    Puerto = n,
                    Descripcion = descripciones.ContainsKey(n) ? descripciones[n] : n
                })
                .ToList();
        }

        // Prueba un puerto con una marca: abre, pide el peso, lee y parsea. Nunca lanza: los errores vuelven en Detalle.
        public static ResultadoPuerto ProbarPuerto(string puerto, IBalanzaDriver driver, BalanzaConfig plantilla)
        {
            var resultado = new ResultadoPuerto { Puerto = puerto, Marca = driver.Marca, Modelo = driver.Modelo };
            var config = ConfigStore.NormalizeBalanza(new BalanzaConfig
            {
                Marca = driver.Marca,
                Modelo = driver.Modelo,
                Puerto = puerto,
                BaudRate = plantilla != null ? plantilla.BaudRate : 0,
                DataBits = plantilla != null ? plantilla.DataBits : 0,
                Parity = plantilla != null ? plantilla.Parity : null,
                // Cada marca tiene su bit de parada (Kretz 2, Systel 1): se deja el de la marca, no el de la plantilla.
                StopBits = null,
                IntervaloLecturaMs = plantilla != null ? plantilla.IntervaloLecturaMs : 0
            });

            try
            {
                using (var port = BalanzaReaderService.CreatePort(config, driver))
                {
                    port.Open();
                    try { port.DiscardInBuffer(); } catch (Exception) { /* buffer ya vacio o puerto sin soporte: no impide la prueba */ }
                    driver.SolicitarPeso(port);
                    Thread.Sleep(EsperaRespuestaMs);
                    string raw = port.ReadExisting();
                    LecturaPeso lectura = driver.ParsearRespuesta(raw, config);
                    resultado.Responde = lectura.Ok;
                    resultado.PesoDisplay = lectura.PesoDisplay;
                    resultado.Detalle = lectura.Ok ? "Responde" : (lectura.Error ?? "Sin respuesta");
                }
            }
            catch (UnauthorizedAccessException)
            {
                resultado.Detalle = "Ocupado por otro programa";
            }
            catch (Exception ex)
            {
                resultado.Detalle = ex.Message;
            }

            return resultado;
        }

        // Prueba todos los puertos con todas las marcas. Devuelve un resultado por puerto (el de la primera marca que responde,
        // o el ultimo intento si ninguna). avance recibe el puerto/marca que se esta probando (puede ser null).
        public static List<ResultadoPuerto> Buscar(BalanzaDriverRegistry registro, BalanzaConfig plantilla, Action<string> avance)
        {
            var salida = new List<ResultadoPuerto>();
            foreach (PuertoInfo puerto in ListarPuertos())
            {
                ResultadoPuerto mejor = null;
                foreach (IBalanzaDriver driver in registro.GetAll())
                {
                    if (avance != null)
                    {
                        avance("Probando " + puerto.Puerto + " con " + driver.Marca + "…");
                    }

                    ResultadoPuerto r = ProbarPuerto(puerto.Puerto, driver, plantilla);
                    mejor = r;
                    if (r.Responde)
                    {
                        break;
                    }

                    // Un puerto ocupado lo esta para todas las marcas: no se repite la prueba.
                    if (r.Detalle == "Ocupado por otro programa")
                    {
                        break;
                    }
                }

                salida.Add(mejor);
                AgentLog.Info("Busqueda: " + mejor.Puerto + " -> " + (mejor.Responde ? "responde (" + mejor.Marca + ", " + mejor.PesoDisplay + ")" : mejor.Detalle));
            }

            return salida;
        }

        private static int NumeroCom(string nombre)
        {
            int numero;
            return nombre.Length > 3 && int.TryParse(nombre.Substring(3), out numero) ? numero : int.MaxValue;
        }
    }
}
