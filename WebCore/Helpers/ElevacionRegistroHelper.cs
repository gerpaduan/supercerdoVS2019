using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebCore.Helpers
{
    // Clave del usuario para modificar registros ANTERIORES A HOY desde la cuenta compartida de produccion
    // (Movimientos, Stock y Elaborados). Ver docs/DECISIONS.md (2026-10-15, "Clave para modificar registros
    // anteriores").
    //
    // Problema: la cuenta de produccion elige "quien esta haciendo esto" SIN clave (SeleccionUsuario) y los
    // permisos con limite de fecha (Usuario.tienePermiso: DiasPermitidosVer/Editar) se evaluaban siempre contra
    // la cuenta compartida, no contra la persona elegida -> "no tiene permisos" aunque esa persona si pudiera.
    //
    // Regla: si la cuenta de produccion no alcanza para un registro anterior a hoy, se pide la clave de la
    // persona elegida (SeleccionUsuarioController.ClaveParaModificar). Con la clave validada, queda una
    // "elevacion" en Session (por modulo, 15 min) y los controllers evaluan el permiso tambien con ESA persona.
    // La elevacion solo AGREGA permisos (union con los de la cuenta de produccion): nunca quita ninguno.
    // La elevacion guarda al usuario con sus Permisos (los que carga ValidarUsuarioWeb); getUsuarioById no los trae.
    public static class ElevacionRegistroHelper
    {
        public const string ModuloMovimientos = "Movimientos";
        public const string ModuloStock = "Stock";
        public const string ModuloElaborados = "Elaborados";

        // Cubre abrir el registro y guardarlo sin pedir la clave dos veces; vence sola.
        public static readonly TimeSpan Duracion = TimeSpan.FromMinutes(15);

        private const string PrefijoClaveSession = "ElevacionRegistro_";

        private sealed class Elevacion
        {
            public Entidades.Usuario Usuario { get; set; }
            public DateTime ExpiraUtc { get; set; }
        }

        public static bool EsModuloValido(string modulo)
        {
            return modulo == ModuloMovimientos || modulo == ModuloStock || modulo == ModuloElaborados;
        }

        // Guarda la persona que acaba de validar su clave para este modulo (pisa una elevacion anterior).
        public static void Registrar(ISession session, string modulo, Entidades.Usuario operador)
        {
            if (session == null || operador == null || !EsModuloValido(modulo)) return;

            var elevacion = new Elevacion { Usuario = operador, ExpiraUtc = DateTime.UtcNow.Add(Duracion) };
            session.SetString(PrefijoClaveSession + modulo, JsonSerializer.Serialize(elevacion));
        }

        // Persona elevada vigente para este modulo, SOLO si es la que se eligio en pantalla (idOperador).
        // Null si no hay elevacion, vencio o es de otra persona.
        public static Entidades.Usuario Obtener(ISession session, string modulo, int idOperador)
        {
            if (session == null || idOperador <= 0 || !EsModuloValido(modulo)) return null;

            string json = session.GetString(PrefijoClaveSession + modulo);
            if (string.IsNullOrEmpty(json)) return null;

            Elevacion elevacion = JsonSerializer.Deserialize<Elevacion>(json);
            if (elevacion == null || elevacion.Usuario == null || elevacion.ExpiraUtc <= DateTime.UtcNow)
            {
                session.Remove(PrefijoClaveSession + modulo);
                return null;
            }

            return elevacion.Usuario.Id == idOperador ? elevacion.Usuario : null;
        }

        // true si el registro es anterior a hoy y la sesion es la cuenta de produccion con una persona ya elegida:
        // unico caso en que la clave de esa persona puede cambiar el resultado del permiso.
        public static bool AplicaElevacion(Entidades.Usuario sesion, int idUsuarioCreador, DateTime fechaRegistro)
        {
            return sesion != null && sesion.EsUsuarioProduccion && idUsuarioCreador > 0 && fechaRegistro.Date < DateTime.Today;
        }

        // Persona elevada que hay que sumar al permiso de la sesion para este registro (null si no aplica o si
        // todavia no puso su clave).
        public static Entidades.Usuario ObtenerElevado(ISession session, string modulo, Entidades.Usuario sesion, int idUsuarioCreador, DateTime fechaRegistro)
        {
            return AplicaElevacion(sesion, idUsuarioCreador, fechaRegistro)
                ? Obtener(session, modulo, idUsuarioCreador)
                : null;
        }

        // true si hay que pedir la clave antes de mostrar el registro: aplica la elevacion, todavia no hay clave
        // vigente y la cuenta de produccion no puede modificar -- y ademas no puede ni verlo, o la persona pidio
        // explicitamente modificar con su clave (conClave, boton "Modificar con mi clave").
        public static bool NecesitaClave(Entidades.Usuario sesion, int idUsuarioCreador, DateTime fechaRegistro,
            bool sesionPuedeModificar, bool sesionPuedeVer, bool conClave, Entidades.Usuario elevado)
        {
            if (elevado != null || !AplicaElevacion(sesion, idUsuarioCreador, fechaRegistro)) return false;
            return !sesionPuedeModificar && (!sesionPuedeVer || conClave);
        }

        // Redirige a la pantalla informativa que pide la clave de la persona elegida y despues vuelve a la URL actual.
        public static RedirectToActionResult RedirigirAClave(Controller controller, string modulo, int idUsuarioCreador)
        {
            return controller.RedirectToAction("ClaveParaModificar", "SeleccionUsuario", new
            {
                modulo,
                idUsuario = idUsuarioCreador,
                returnUrl = controller.Request.PathBase + controller.Request.Path + controller.Request.QueryString,
                cancelUrl = controller.Url.Action("Index", controller.ControllerContext.ActionDescriptor.ControllerName)
            });
        }

        // Mensaje cuando la persona ya puso su clave y igual no tiene permiso para esa fecha. `permiso` es el
        // formulario de edicion evaluado (Permisos.*); `soloVer` = se evaluo el permiso de consulta, no el de edicion.
        public static string MensajeSinPermisoFecha(Entidades.Usuario operador, string permiso, bool soloVer)
        {
            string nombre = operador != null && !string.IsNullOrWhiteSpace(operador.Nombre) ? operador.Nombre : "El usuario";
            string accion = soloVer ? "consultar" : "modificar";
            string basico = nombre + " no tiene permiso para " + accion + " registros con una fecha anterior a la de su permiso.";

            // Detalle: desde que fecha SI puede (solo si su permiso tiene un limite de dias configurado).
            DateTime? minima = soloVer
                ? PermisosHelper.ObtenerFechaMinimaPermitida(operador, permiso)
                : PermisosHelper.ObtenerFechaMinimaPermitida(operador, permiso, 0);
            return minima.HasValue
                ? basico + " Puede " + accion + " registros desde el " + minima.Value.ToString("dd/MM/yyyy") + "."
                : basico;
        }
    }
}
