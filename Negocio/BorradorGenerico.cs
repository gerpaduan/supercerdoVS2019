using System;
using System.Collections.Generic;
using System.Globalization;

namespace Negocio
{
    // Reglas de los borradores en servidor de Compras/Stock/Movimientos/Embutidos y de las
    // notificaciones al admin que generan (ver docs/DECISIONS.md "Borradores de
    // Compras/Stock/Movimientos/Embutidos"). Solo Postgres: no hay constructor contra SQL Server
    // (mismo criterio que Negocio.VentaBorrador). Los umbrales (minutos sin latido, dias de retencion)
    // llegan por parametro desde la configuracion de WebCore; aca no hay valores hardcodeados.
    // Esta clase NO parsea el JSON del formulario: el controller ya calculo resumen y cantidad de
    // lineas y pasa el payload como texto opaco (Negocio compila tambien para net472, sin
    // System.Text.Json).
    // A diferencia de Negocio.VentaBorrador: sin idempotencia contra "caja" (estos modulos no tienen),
    // sin RegistrarCierreCajaConVentas (no hay caja) y sin la seccion de "producto sin agregar"
    // (exclusiva de POS).
    public class BorradorGenerico
    {
        public const int LargoMaximoMotivo = 300;

        private readonly Contratos.IBorradorGenericoRepository oBorradorD;

        public BorradorGenerico(Contratos.IBorradorGenericoRepository repositorio)
        {
            oBorradorD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        private static void ValidarModulo(string modulo)
        {
            foreach (string valido in Entidades.BorradorGenerico.ModulosValidos)
            {
                if (string.Equals(modulo, valido, StringComparison.Ordinal)) return;
            }
            throw new ArgumentException("Módulo de borrador desconocido: " + modulo, nameof(modulo));
        }

        // Guarda/actualiza el formulario en curso. Valida lo minimo; el resultado indica si el
        // navegador debe abandonar ese borrador (YaCerrado) o si el clientId es de otro operador (Ajeno).
        public Entidades.ResultadoGuardarBorradorGenerico Guardar(Entidades.BorradorGenerico borrador)
        {
            if (borrador == null) throw new ArgumentNullException(nameof(borrador));
            ValidarModulo(borrador.Modulo);
            if (borrador.ClientId == Guid.Empty) throw new ArgumentException("Falta el identificador del formulario.", nameof(borrador));
            if (borrador.IdOperador <= 0) throw new ArgumentException("Falta el operador.", nameof(borrador));
            if (borrador.IdSucursal <= 0) throw new ArgumentException("Falta la sucursal.", nameof(borrador));
            if (borrador.CantLineas < 0) borrador.CantLineas = 0;

            return oBorradorD.Guardar(borrador);
        }

        public bool RegistrarLatido(Guid clientId, int idOperador, int idSucursal, string modulo)
        {
            ValidarModulo(modulo);
            if (clientId == Guid.Empty) return false;
            return oBorradorD.RegistrarLatido(clientId, idOperador, idSucursal, modulo);
        }

        // Cierre de pestana informado por el navegador (pagehide). Solo si el formulario es del operador.
        public bool RegistrarCierrePestana(Guid clientId, int idOperador, int idSucursal, string modulo, int idUsuarioActor)
        {
            ValidarModulo(modulo);
            var borrador = oBorradorD.ObtenerPorClientId(clientId, modulo);
            if (borrador == null || borrador.Estado != Entidades.BorradorGenerico.EstadoActiva) return false;
            if (borrador.IdOperador != idOperador || borrador.IdSucursal != idSucursal) return false;

            oBorradorD.AgregarEvento(new Entidades.BorradorGenericoEvento
            {
                IdBorrador = borrador.Id,
                Tipo = Entidades.BorradorGenericoEvento.TipoCierrePestana,
                IdUsuario = idUsuarioActor,
                Detalle = "El navegador se cerró o abandonó la pantalla con el formulario sin guardar."
            });

            // Un pagehide real es una señal mas confiable que "no hubo latido": en vez de esperar el
            // umbral configurado, queda interrumpido (recuperable y notificable) de inmediato.
            oBorradorD.EnvejecerLatidoPorCierre(borrador.Id);
            return true;
        }

        // Idempotencia de "Guardar" en el modulo destino: si este borrador ya se convirtio en el
        // registro real, devuelve su id (un reintento tras un timeout no debe duplicarlo). null si no.
        public int? ObtenerIdResultadoSiYaFinalizada(Guid clientId, string modulo)
        {
            ValidarModulo(modulo);
            if (clientId == Guid.Empty) return null;
            var borrador = oBorradorD.ObtenerPorClientId(clientId, modulo);
            if (borrador != null && borrador.Estado == Entidades.BorradorGenerico.EstadoFinalizada && borrador.IdResultado.HasValue)
                return borrador.IdResultado;
            return null;
        }

        public bool MarcarFinalizada(Guid clientId, string modulo, int idResultado)
        {
            ValidarModulo(modulo);
            if (clientId == Guid.Empty) return false;
            return oBorradorD.MarcarFinalizada(clientId, modulo, idResultado);
        }

        // Borradores sin cerrar de toda la sucursal (con payload) para un modulo, para el listado "ver
        // borradores sin cerrar". Aprovecha para purgar los FINALIZADA viejos de ese modulo (solo
        // servian para idempotencia).
        public List<Entidades.BorradorGenerico> ListarSucursal(int idSucursal, string modulo, int diasRetencionFinalizadas)
        {
            ValidarModulo(modulo);
            if (diasRetencionFinalizadas > 0) oBorradorD.PurgarFinalizadasAntiguas(modulo, diasRetencionFinalizadas);
            return oBorradorD.ListarActivasPorSucursal(idSucursal, modulo);
        }

        // Un borrador esta "en uso" si su ultimo latido es reciente: hay un formulario vivo en alguna terminal.
        public static bool EstaEnUso(Entidades.BorradorGenerico borrador, int minutosSinLatido)
        {
            return borrador != null
                && borrador.Estado == Entidades.BorradorGenerico.EstadoActiva
                && !borrador.EstaInterrumpida(minutosSinLatido);
        }

        // Recupera un borrador sin cerrar para cargarlo al formulario del operador actual. Reglas:
        //  - debe existir, estar ACTIVO y estar interrumpido (no "en uso" en otra terminal);
        //  - si es de otro operador, hace falta autorizacion de un supervisor/admin.
        // Al recuperarlo pasa al operador actual y queda "en uso" (sigue latiendo con el mismo clientId).
        // Devuelve null y el motivo en "error" si no se puede.
        // nombreSupervisor: nombre de quien autorizo con su clave (null/vacio = sin autorizacion).
        public Entidades.BorradorGenerico Recuperar(int idBorrador, string modulo, int idOperadorActual, int idUsuarioSesionActual,
            int idUsuarioActor, string nombreSupervisor, int minutosSinLatido, out string error)
        {
            ValidarModulo(modulo);
            error = null;
            bool autorizadoPorSupervisor = !string.IsNullOrWhiteSpace(nombreSupervisor);
            var borrador = oBorradorD.ObtenerPorId(idBorrador);
            if (borrador == null || borrador.Modulo != modulo || borrador.Estado != Entidades.BorradorGenerico.EstadoActiva)
            {
                error = "Ese borrador ya no está disponible (se guardó o se descartó).";
                return null;
            }

            if (EstaEnUso(borrador, minutosSinLatido))
            {
                error = "Ese borrador está en uso en otra terminal. Solo se pueden cargar los que quedaron interrumpidos.";
                return null;
            }

            bool esDelOperador = borrador.IdOperador == idOperadorActual;
            if (!esDelOperador && !autorizadoPorSupervisor)
            {
                error = "Ese borrador es de otro usuario: hace falta la autorización de un supervisor.";
                return null;
            }

            int duenoOriginal = borrador.IdOperador;
            string nombreDueno = borrador.NombreOperador;

            if (!oBorradorD.TomarBorrador(idBorrador, idOperadorActual, idUsuarioSesionActual))
            {
                error = "Ese borrador ya no está disponible.";
                return null;
            }

            oBorradorD.AgregarEvento(new Entidades.BorradorGenericoEvento
            {
                IdBorrador = idBorrador,
                Tipo = Entidades.BorradorGenericoEvento.TipoRecuperada,
                IdUsuario = idUsuarioActor,
                Detalle = esDelOperador
                    ? "Recuperado por su dueño."
                    : "Tomado por otro usuario con autorización de " + nombreSupervisor + ". Dueño original: " + (nombreDueno ?? ("#" + duenoOriginal.ToString(CultureInfo.InvariantCulture))) + "."
            });

            borrador.IdOperador = idOperadorActual;
            borrador.IdUsuarioSesion = idUsuarioSesionActual;
            return borrador;
        }

        // Descarta un borrador sin cerrar. Mismo criterio de propiedad que Recuperar y motivo obligatorio.
        // SIEMPRE avisa al admin (notificacion BORRADOR_DESCARTADO_<MODULO>).
        // nombreSupervisor: nombre de quien autorizo con su clave (null/vacio = sin autorizacion).
        public bool Descartar(int idBorrador, string modulo, string motivo, int idOperadorActual, int idUsuarioActor, string nombreActor,
            string nombreSupervisor, int minutosSinLatido, out string error)
        {
            ValidarModulo(modulo);
            error = null;
            bool autorizadoPorSupervisor = !string.IsNullOrWhiteSpace(nombreSupervisor);
            motivo = (motivo ?? "").Trim();
            if (motivo.Length == 0)
            {
                error = "Indicá el motivo por el que se descarta el borrador.";
                return false;
            }
            if (motivo.Length > LargoMaximoMotivo) motivo = motivo.Substring(0, LargoMaximoMotivo);

            var borrador = oBorradorD.ObtenerPorId(idBorrador);
            if (borrador == null || borrador.Modulo != modulo || borrador.Estado != Entidades.BorradorGenerico.EstadoActiva)
            {
                error = "Ese borrador ya no está disponible (se guardó o se descartó).";
                return false;
            }

            if (EstaEnUso(borrador, minutosSinLatido))
            {
                error = "Ese borrador está en uso en otra terminal: no se puede descartar mientras se está armando.";
                return false;
            }

            if (borrador.IdOperador != idOperadorActual && !autorizadoPorSupervisor)
            {
                error = "Ese borrador es de otro usuario: hace falta la autorización de un supervisor.";
                return false;
            }

            if (!oBorradorD.MarcarDescartada(idBorrador))
            {
                error = "Ese borrador ya no está disponible.";
                return false;
            }

            oBorradorD.AgregarEvento(new Entidades.BorradorGenericoEvento
            {
                IdBorrador = idBorrador,
                Tipo = Entidades.BorradorGenericoEvento.TipoDescartada,
                IdUsuario = idUsuarioActor,
                Detalle = autorizadoPorSupervisor ? motivo + " (autorizó: " + nombreSupervisor + ")" : motivo
            });

            oBorradorD.UpsertNotificacion(new Entidades.Notificacion
            {
                IdSucursal = borrador.IdSucursal,
                Tipo = TipoNotificacionDescartado(modulo),
                Severidad = Entidades.Notificacion.SeveridadAdvertencia,
                Titulo = "Borrador descartado (" + TituloModulo(modulo) + ")",
                Mensaje = (nombreActor ?? "Un usuario") + " descartó un borrador sin guardar de " + (borrador.NombreOperador ?? "un operador")
                    + (string.IsNullOrWhiteSpace(borrador.Resumen) ? "" : " (" + borrador.Resumen + ")")
                    + ". Motivo: " + motivo,
                RefId = idBorrador
            }, true);

            return true;
        }

        // Logout con borradores en curso de esa cuenta de sesion (de cualquier modulo): deja constancia
        // en cada uno.
        public int RegistrarLogout(int idUsuarioSesion)
        {
            var enCurso = oBorradorD.ListarActivasPorUsuarioSesion(idUsuarioSesion);
            foreach (var borrador in enCurso)
            {
                oBorradorD.AgregarEvento(new Entidades.BorradorGenericoEvento
                {
                    IdBorrador = borrador.Id,
                    Tipo = Entidades.BorradorGenericoEvento.TipoLogout,
                    IdUsuario = idUsuarioSesion,
                    Detalle = "Se cerró la sesión con el formulario sin guardar."
                });
            }
            return enCurso.Count;
        }

        private static string TipoNotificacionDescartado(string modulo)
        {
            switch (modulo)
            {
                case Entidades.BorradorGenerico.ModuloCompra: return Entidades.Notificacion.TipoBorradorDescartadoCompra;
                case Entidades.BorradorGenerico.ModuloStock: return Entidades.Notificacion.TipoBorradorDescartadoStock;
                case Entidades.BorradorGenerico.ModuloMovimiento: return Entidades.Notificacion.TipoBorradorDescartadoMovimiento;
                case Entidades.BorradorGenerico.ModuloEmbutidoCarga: return Entidades.Notificacion.TipoBorradorDescartadoEmbutidoCarga;
                case Entidades.BorradorGenerico.ModuloEmbutidoRapido: return Entidades.Notificacion.TipoBorradorDescartadoEmbutidoRapido;
                default: throw new ArgumentException("Módulo de borrador desconocido: " + modulo, nameof(modulo));
            }
        }

        private static string TituloModulo(string modulo)
        {
            switch (modulo)
            {
                case Entidades.BorradorGenerico.ModuloCompra: return "Compras";
                case Entidades.BorradorGenerico.ModuloStock: return "Stock";
                case Entidades.BorradorGenerico.ModuloMovimiento: return "Movimientos";
                case Entidades.BorradorGenerico.ModuloEmbutidoCarga: return "Embutidos";
                case Entidades.BorradorGenerico.ModuloEmbutidoRapido: return "Embutidos - Ingreso rápido";
                default: return modulo;
            }
        }

        // ============================================================================================
        // Admin: deteccion de interrumpidos (perezosa, se calcula cuando el admin consulta la campana)
        // ============================================================================================

        // Crea BORRADOR_INTERRUMPIDO_<modulo> para los 5 modulos. Se llama junto a
        // Negocio.VentaBorrador.ResumenParaCampana (misma tabla de notificaciones) desde
        // NotificacionesController.Resumen. Devuelve cuantas creo en total.
        public int CrearNotificacionesInterrumpidas(int minutosSinLatido)
        {
            int total = 0;
            foreach (string modulo in Entidades.BorradorGenerico.ModulosValidos)
            {
                total += oBorradorD.CrearNotificacionesInterrumpidas(modulo, minutosSinLatido);
            }
            return total;
        }

        // ============================================================================================
        // Admin: detalle
        // ============================================================================================

        public List<Entidades.BorradorGenericoEvento> ListarEventosPorBorrador(int idBorrador)
        {
            return oBorradorD.ListarEventosPorBorrador(idBorrador);
        }

        // Con payload; el controller lo usa para armar el detalle de un borrador (admin).
        public Entidades.BorradorGenerico ObtenerPorId(int id)
        {
            return oBorradorD.ObtenerPorId(id);
        }

        public Entidades.BorradorGenerico ObtenerPorClientId(Guid clientId, string modulo)
        {
            ValidarModulo(modulo);
            return oBorradorD.ObtenerPorClientId(clientId, modulo);
        }
    }
}
