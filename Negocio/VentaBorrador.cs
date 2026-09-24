using System;
using System.Collections.Generic;
using System.Globalization;

namespace Negocio
{
    // Reglas de las ventas en curso del POS guardadas en el servidor, de las advertencias de "producto
    // pesado sin agregar" y de las notificaciones al admin (ver docs/DECISIONS.md "Ventas en curso").
    // Solo Postgres: no hay constructor contra SQL Server (mismo criterio que UsuarioPasskey). Los
    // umbrales (minutos sin latido, segundos de producto en pantalla) llegan por parametro desde la
    // configuracion de WebCore; aca no hay valores hardcodeados.
    // Esta clase NO parsea el JSON del carrito: el controller ya calculo cantidad de lineas y total y
    // pasa el payload como texto opaco (Negocio compila tambien para net472, sin System.Text.Json).
    public class VentaBorrador
    {
        // Largos maximos de textos libres que llegan del navegador o del admin.
        public const int LargoMaximoMotivo = 300;
        public const int LargoMaximoComentario = 500;

        // Topes de sanidad de las duraciones que informa el navegador (no confiar en el cliente).
        public const int MaximoSegundosEnPantalla = 6 * 60 * 60;
        public const int MaximoSegundosDesdeSalida = 60 * 60;

        private readonly Contratos.IVentaBorradorRepository oBorradorD;

        public VentaBorrador(Contratos.IVentaBorradorRepository repositorio)
        {
            oBorradorD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        // ============================================================================================
        // Venta en curso (POS)
        // ============================================================================================

        // Guarda/actualiza el carrito. Valida lo minimo; el resultado indica si el navegador debe
        // abandonar ese carrito (YaCerrado) o si el clientId es de otro operador (Ajeno).
        public Entidades.ResultadoGuardarBorrador Guardar(Entidades.VentaBorrador borrador)
        {
            if (borrador == null) throw new ArgumentNullException(nameof(borrador));
            if (borrador.ClientId == Guid.Empty) throw new ArgumentException("Falta el identificador del carrito.", nameof(borrador));
            if (borrador.IdOperador <= 0) throw new ArgumentException("Falta el operador.", nameof(borrador));
            if (borrador.IdSucursal <= 0) throw new ArgumentException("Falta la sucursal.", nameof(borrador));
            if (borrador.CantLineas < 0) borrador.CantLineas = 0;
            if (borrador.Total < 0) borrador.Total = 0;

            return oBorradorD.Guardar(borrador);
        }

        public bool RegistrarLatido(Guid clientId, int idOperador, int idSucursal)
        {
            if (clientId == Guid.Empty) return false;
            return oBorradorD.RegistrarLatido(clientId, idOperador, idSucursal);
        }

        // Cierre de pestana informado por el navegador (pagehide). Solo si el carrito es del operador.
        public bool RegistrarCierrePestana(Guid clientId, int idOperador, int idSucursal, int idUsuarioActor)
        {
            var borrador = oBorradorD.ObtenerPorClientId(clientId);
            if (borrador == null || borrador.Estado != Entidades.VentaBorrador.EstadoActiva) return false;
            if (borrador.IdOperador != idOperador || borrador.IdSucursal != idSucursal) return false;

            oBorradorD.AgregarEvento(new Entidades.VentaBorradorEvento
            {
                IdBorrador = borrador.Id,
                Tipo = Entidades.VentaBorradorEvento.TipoCierrePestana,
                IdUsuario = idUsuarioActor,
                Detalle = "El navegador se cerró o abandonó el POS con la venta sin finalizar."
            });

            // Un pagehide real es una señal mas confiable que "no hubo latido": en vez de esperar el
            // umbral configurado (5 min por defecto), la venta queda interrumpida (recuperable) de inmediato.
            oBorradorD.EnvejecerLatidoPorCierre(borrador.Id);
            return true;
        }

        // Idempotencia de Finalizar: si este carrito ya se convirtio en una venta real, devuelve su id
        // (un reintento tras un timeout no debe duplicar la venta). null si no.
        public int? ObtenerIdVentaSiYaFinalizada(Guid clientId)
        {
            if (clientId == Guid.Empty) return null;
            var borrador = oBorradorD.ObtenerPorClientId(clientId);
            if (borrador != null && borrador.Estado == Entidades.VentaBorrador.EstadoFinalizada && borrador.IdVenta.HasValue)
                return borrador.IdVenta;
            return null;
        }

        public bool MarcarFinalizada(Guid clientId, int idVenta)
        {
            if (clientId == Guid.Empty) return false;
            return oBorradorD.MarcarFinalizada(clientId, idVenta);
        }

        // Ventas sin cerrar de toda la sucursal (con payload) para el listado del POS. Aprovecha para
        // purgar las FINALIZADAS viejas (solo servian para idempotencia).
        public List<Entidades.VentaBorrador> ListarSucursal(int idSucursal, int diasRetencionFinalizadas)
        {
            if (diasRetencionFinalizadas > 0) oBorradorD.PurgarFinalizadasAntiguas(diasRetencionFinalizadas);
            return oBorradorD.ListarActivasPorSucursal(idSucursal);
        }

        // Una venta esta "en uso" si su ultimo latido es reciente: hay un carrito vivo en alguna terminal.
        public static bool EstaEnUso(Entidades.VentaBorrador borrador, int minutosSinLatido)
        {
            return borrador != null
                && borrador.Estado == Entidades.VentaBorrador.EstadoActiva
                && !borrador.EstaInterrumpida(minutosSinLatido);
        }

        // Recupera una venta sin cerrar para cargarla al carrito del operador actual. Reglas:
        //  - debe existir, estar ACTIVA y estar interrumpida (no "en uso" en otra terminal);
        //  - si es de otro operador, hace falta autorizacion de un supervisor/admin.
        // Al recuperarla pasa al operador actual y queda "en uso" (su carrito sigue latiendo con el mismo
        // clientId). Devuelve null y el motivo en "error" si no se puede.
        // nombreSupervisor: nombre de quien autorizo con su clave (null/vacio = sin autorizacion).
        public Entidades.VentaBorrador Recuperar(int idBorrador, int idOperadorActual, int idUsuarioSesionActual,
            int idUsuarioActor, string nombreSupervisor, int minutosSinLatido, out string error)
        {
            error = null;
            bool autorizadoPorSupervisor = !string.IsNullOrWhiteSpace(nombreSupervisor);
            var borrador = oBorradorD.ObtenerPorId(idBorrador);
            if (borrador == null || borrador.Estado != Entidades.VentaBorrador.EstadoActiva)
            {
                error = "Esa venta ya no está disponible (se finalizó o se descartó).";
                return null;
            }

            if (EstaEnUso(borrador, minutosSinLatido))
            {
                error = "Esa venta está en uso en otra terminal. Solo se pueden cargar las que quedaron interrumpidas.";
                return null;
            }

            bool esDelOperador = borrador.IdOperador == idOperadorActual;
            if (!esDelOperador && !autorizadoPorSupervisor)
            {
                error = "Esa venta es de otro usuario: hace falta la autorización de un supervisor.";
                return null;
            }

            int duenoOriginal = borrador.IdOperador;
            string nombreDueno = borrador.NombreOperador;

            if (!oBorradorD.TomarBorrador(idBorrador, idOperadorActual, idUsuarioSesionActual))
            {
                error = "Esa venta ya no está disponible.";
                return null;
            }

            oBorradorD.AgregarEvento(new Entidades.VentaBorradorEvento
            {
                IdBorrador = idBorrador,
                Tipo = Entidades.VentaBorradorEvento.TipoRecuperada,
                IdUsuario = idUsuarioActor,
                Detalle = esDelOperador
                    ? "Recuperada por su dueño."
                    : "Tomada por otro usuario con autorización de " + nombreSupervisor + ". Dueño original: " + (nombreDueno ?? ("#" + duenoOriginal.ToString(CultureInfo.InvariantCulture))) + "."
            });

            borrador.IdOperador = idOperadorActual;
            borrador.IdUsuarioSesion = idUsuarioSesionActual;
            return borrador;
        }

        // Descarta una venta sin cerrar. Mismo criterio de propiedad que Recuperar y motivo obligatorio.
        // SIEMPRE avisa al admin (notificacion VENTA_DESCARTADA).
        // nombreSupervisor: nombre de quien autorizo con su clave (null/vacio = sin autorizacion).
        public bool Descartar(int idBorrador, string motivo, int idOperadorActual, int idUsuarioActor, string nombreActor,
            string nombreSupervisor, int minutosSinLatido, out string error)
        {
            error = null;
            bool autorizadoPorSupervisor = !string.IsNullOrWhiteSpace(nombreSupervisor);
            motivo = (motivo ?? "").Trim();
            if (motivo.Length == 0)
            {
                error = "Indicá el motivo por el que se descarta la venta.";
                return false;
            }
            if (motivo.Length > LargoMaximoMotivo) motivo = motivo.Substring(0, LargoMaximoMotivo);

            var borrador = oBorradorD.ObtenerPorId(idBorrador);
            if (borrador == null || borrador.Estado != Entidades.VentaBorrador.EstadoActiva)
            {
                error = "Esa venta ya no está disponible (se finalizó o se descartó).";
                return false;
            }

            if (EstaEnUso(borrador, minutosSinLatido))
            {
                error = "Esa venta está en uso en otra terminal: no se puede descartar mientras se está armando.";
                return false;
            }

            if (borrador.IdOperador != idOperadorActual && !autorizadoPorSupervisor)
            {
                error = "Esa venta es de otro usuario: hace falta la autorización de un supervisor.";
                return false;
            }

            if (!oBorradorD.MarcarDescartada(idBorrador))
            {
                error = "Esa venta ya no está disponible.";
                return false;
            }

            oBorradorD.AgregarEvento(new Entidades.VentaBorradorEvento
            {
                IdBorrador = idBorrador,
                Tipo = Entidades.VentaBorradorEvento.TipoDescartada,
                IdUsuario = idUsuarioActor,
                Detalle = autorizadoPorSupervisor ? motivo + " (autorizó: " + nombreSupervisor + ")" : motivo
            });

            oBorradorD.UpsertNotificacion(new Entidades.Notificacion
            {
                IdSucursal = borrador.IdSucursal,
                Tipo = Entidades.Notificacion.TipoVentaDescartada,
                Severidad = Entidades.Notificacion.SeveridadAdvertencia,
                Titulo = "Venta en curso descartada",
                Mensaje = (nombreActor ?? "Un usuario") + " descartó una venta sin cerrar de " + (borrador.NombreOperador ?? "un cajero")
                    + " por $ " + borrador.Total.ToString("N2", new CultureInfo("es-AR")) + " (" + borrador.CantLineas.ToString(CultureInfo.InvariantCulture)
                    + " ítem(s)). Motivo: " + motivo,
                RefId = idBorrador
            }, true);

            return true;
        }

        // Cierre de caja: ventas ACTIVAS del dueno de la caja (sin payload) para avisar/confirmar.
        public List<Entidades.VentaBorrador> ListarEnCursoDelOperador(int idOperador, int idSucursal)
        {
            return oBorradorD.ListarActivasPorOperador(idOperador, idSucursal);
        }

        // Registra que se cerro la caja con ventas en curso: evento + notificacion por cada una. Las
        // ventas quedan ACTIVAS (el cajero podra recuperarlas al abrir una caja nueva).
        public int RegistrarCierreCajaConVentas(int idOperadorDuenoCaja, int idSucursal, int idUsuarioAutorizado, string nombreAutorizado)
        {
            var enCurso = oBorradorD.ListarActivasPorOperador(idOperadorDuenoCaja, idSucursal);
            foreach (var borrador in enCurso)
            {
                oBorradorD.AgregarEvento(new Entidades.VentaBorradorEvento
                {
                    IdBorrador = borrador.Id,
                    Tipo = Entidades.VentaBorradorEvento.TipoCierreCaja,
                    IdUsuario = idUsuarioAutorizado,
                    Detalle = "Se cerró la caja con esta venta en curso. Cerró: " + (nombreAutorizado ?? "un supervisor") + "."
                });

                oBorradorD.UpsertNotificacion(new Entidades.Notificacion
                {
                    IdSucursal = borrador.IdSucursal,
                    Tipo = Entidades.Notificacion.TipoCierreCajaConVenta,
                    Severidad = Entidades.Notificacion.SeveridadAdvertencia,
                    Titulo = "Caja cerrada con una venta en curso",
                    Mensaje = (nombreAutorizado ?? "Un supervisor") + " cerró la caja de " + (borrador.NombreOperador ?? "un cajero")
                        + " con una venta sin finalizar de $ " + borrador.Total.ToString("N2", new CultureInfo("es-AR"))
                        + " (" + borrador.CantLineas.ToString(CultureInfo.InvariantCulture) + " ítem(s)).",
                    RefId = borrador.Id
                }, true);
            }
            return enCurso.Count;
        }

        // Logout con ventas en curso de esa cuenta de sesion: deja constancia en cada una.
        public int RegistrarLogout(int idUsuarioSesion)
        {
            var enCurso = oBorradorD.ListarActivasPorUsuarioSesion(idUsuarioSesion);
            foreach (var borrador in enCurso)
            {
                oBorradorD.AgregarEvento(new Entidades.VentaBorradorEvento
                {
                    IdBorrador = borrador.Id,
                    Tipo = Entidades.VentaBorradorEvento.TipoLogout,
                    IdUsuario = idUsuarioSesion,
                    Detalle = "Se cerró la sesión con la venta sin finalizar."
                });
            }
            return enCurso.Count;
        }

        // ============================================================================================
        // Producto pesado sin agregar (advertencia)
        // ============================================================================================

        // Registra la advertencia. Devuelve el id o 0 si no correspondia registrarla (datos invalidos o
        // por debajo del umbral); el navegador nunca debe fallar por esto. Actualiza la notificacion
        // unica del operador y dia con el acumulado.
        public int RegistrarProductoSinAgregar(Entidades.ProductoSinAgregar producto, int segundosUmbral, string nombreOperador)
        {
            if (producto == null) throw new ArgumentNullException(nameof(producto));

            if (string.IsNullOrWhiteSpace(producto.Codigo)) return 0;
            if (producto.CantidadKg <= 0 || producto.PrecioKg <= 0) return 0;
            if (producto.SegundosEstables < segundosUmbral) return 0;
            if (producto.SegundosEnPantalla < producto.SegundosEstables) producto.SegundosEnPantalla = producto.SegundosEstables;
            if (producto.SegundosEnPantalla > MaximoSegundosEnPantalla) return 0;
            if (producto.SegundosDesdeSalida < 0) producto.SegundosDesdeSalida = 0;
            if (producto.SegundosDesdeSalida > MaximoSegundosDesdeSalida) return 0;
            if (!EsUno(producto.Origen, Entidades.ProductoSinAgregar.OrigenBalanza, Entidades.ProductoSinAgregar.OrigenManual)) return 0;
            if (!EsUno(producto.Motivo,
                    Entidades.ProductoSinAgregar.MotivoCodigoBorrado, Entidades.ProductoSinAgregar.MotivoCodigoCambiado,
                    Entidades.ProductoSinAgregar.MotivoCantidadCero, Entidades.ProductoSinAgregar.MotivoFinalizar,
                    Entidades.ProductoSinAgregar.MotivoCierrePestana)) return 0;

            producto.Importe = Math.Round(producto.CantidadKg * producto.PrecioKg, 2, MidpointRounding.AwayFromZero);

            int id = oBorradorD.AgregarProductoSinAgregar(producto);
            var guardado = oBorradorD.ObtenerProductoSinAgregar(id);
            DateTime dia = guardado != null ? guardado.Fin.Date : DateTime.Now.Date;

            // Acumulado del operador en ese dia (incluye el recien insertado).
            var resumen = oBorradorD.ResumirProductoSinAgregar(producto.IdOperador, dia, dia.AddDays(1));
            var es = new CultureInfo("es-AR");
            oBorradorD.UpsertNotificacion(new Entidades.Notificacion
            {
                IdSucursal = producto.IdSucursal,
                Tipo = Entidades.Notificacion.TipoProductoSinAgregar,
                Severidad = Entidades.Notificacion.SeveridadAdvertencia,
                Titulo = "Producto pesado sin agregar al carrito",
                Mensaje = resumen.Cantidad.ToString(CultureInfo.InvariantCulture)
                    + (resumen.Cantidad == 1 ? " producto pesado quedó en pantalla y no se agregó" : " productos pesados quedaron en pantalla y no se agregaron")
                    + " el " + dia.ToString("dd/MM/yyyy", es) + " — $ " + resumen.Importe.ToString("N2", es)
                    + " — " + (string.IsNullOrWhiteSpace(nombreOperador) ? "cajero #" + producto.IdOperador.ToString(CultureInfo.InvariantCulture) : nombreOperador),
                RefId = Entidades.Notificacion.RefIdOperadorDia(producto.IdOperador, dia)
            }, true);

            return id;
        }

        private static bool EsUno(string valor, params string[] permitidos)
        {
            foreach (string permitido in permitidos)
            {
                if (string.Equals(valor, permitido, StringComparison.Ordinal)) return true;
            }
            return false;
        }

        // ============================================================================================
        // Admin: notificaciones y revision
        // ============================================================================================

        // Deteccion perezosa de ventas interrumpidas y listado para la campana. Devuelve el total de
        // pendientes y llena "ultimas" con las mas recientes (pendientes primero por fecha).
        public int ResumenParaCampana(int minutosSinLatido, int max, out List<Entidades.Notificacion> ultimas)
        {
            oBorradorD.CrearNotificacionesVentasInterrumpidas(minutosSinLatido);
            ultimas = oBorradorD.ListarNotificaciones(true, max);
            return oBorradorD.ContarNotificacionesPendientes();
        }

        public List<Entidades.Notificacion> ListarNotificaciones(bool soloPendientes, int max)
        {
            return oBorradorD.ListarNotificaciones(soloPendientes, max);
        }

        // Registra (o actualiza, idempotente por empresa+tipo+refid) una notificacion al admin que no nace
        // de un borrador: p. ej. el aviso de certificado ARCA por vencer. reabrir=true la vuelve a dejar
        // pendiente aunque ya la hubiera atendido un admin.
        public void RegistrarNotificacion(Entidades.Notificacion notificacion, bool reabrir)
        {
            oBorradorD.UpsertNotificacion(notificacion, reabrir);
        }

        public Entidades.Notificacion ObtenerNotificacion(int id)
        {
            return oBorradorD.ObtenerNotificacion(id);
        }

        public bool AtenderNotificacion(int id, int idUsuario)
        {
            return oBorradorD.AtenderNotificacion(id, idUsuario);
        }

        // Revision de una advertencia por el admin: solo JUSTIFICADA o SOSPECHOSA.
        public bool RevisarProductoSinAgregar(int id, string revision, string comentario, int idUsuario, out string error)
        {
            error = null;
            if (!EsUno(revision, Entidades.ProductoSinAgregar.RevisionJustificada, Entidades.ProductoSinAgregar.RevisionSospechosa))
            {
                error = "Elegí si la advertencia fue justificada o sospechosa.";
                return false;
            }

            comentario = (comentario ?? "").Trim();
            if (comentario.Length > LargoMaximoComentario) comentario = comentario.Substring(0, LargoMaximoComentario);

            if (!oBorradorD.RevisarProductoSinAgregar(id, revision, comentario.Length == 0 ? null : comentario, idUsuario))
            {
                error = "No se encontró la advertencia.";
                return false;
            }
            return true;
        }

        public Entidades.ProductoSinAgregar ObtenerProductoSinAgregar(int id)
        {
            return oBorradorD.ObtenerProductoSinAgregar(id);
        }

        public List<Entidades.ProductoSinAgregar> ListarProductoSinAgregarDelDia(int idOperador, DateTime dia)
        {
            return oBorradorD.ListarProductoSinAgregarDelDia(idOperador, dia);
        }

        public Entidades.ResumenProductoSinAgregar ResumirProductoSinAgregar(int idOperador, DateTime desde, DateTime hasta)
        {
            return oBorradorD.ResumirProductoSinAgregar(idOperador, desde, hasta);
        }

        // ============================================================================================
        // Admin: Actividades y detalle
        // ============================================================================================

        public List<Entidades.VentaBorrador> ListarInterrumpidasPorRango(DateTime desde, DateTime hasta, int minutosSinLatido)
        {
            return oBorradorD.ListarInterrumpidasPorRango(desde, hasta, minutosSinLatido);
        }

        public List<Entidades.VentaBorradorEvento> ListarEventosPorRango(DateTime desde, DateTime hasta)
        {
            return oBorradorD.ListarEventosPorRango(desde, hasta);
        }

        public List<Entidades.VentaBorradorEvento> ListarEventosPorBorrador(int idBorrador)
        {
            return oBorradorD.ListarEventosPorBorrador(idBorrador);
        }

        public List<Entidades.ProductoSinAgregar> ListarProductoSinAgregarPorRango(DateTime desde, DateTime hasta)
        {
            return oBorradorD.ListarProductoSinAgregarPorRango(desde, hasta);
        }

        // Con payload; el controller lo usa para armar el detalle (items) de una venta en curso.
        public Entidades.VentaBorrador ObtenerPorId(int id)
        {
            return oBorradorD.ObtenerPorId(id);
        }

        public Entidades.VentaBorrador ObtenerPorClientId(Guid clientId)
        {
            return oBorradorD.ObtenerPorClientId(clientId);
        }
    }
}
