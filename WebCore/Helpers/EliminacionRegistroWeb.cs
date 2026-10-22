// Piezas comunes de los endpoints de eliminacion de Compras, Stock y Movimientos (WebCore): armado del modelo del boton, y
// proyeccion a JSON de la vista previa y de las respuestas de error de la revalidacion. La logica de negocio vive en
// Negocio.Compra.eliminarCompra / Negocio.Corte.eliminarMovimientoConMotivo. Ver docs/DECISIONS.md "Eliminar compras, stock y movimientos".
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebCore.Models;

namespace WebCore.Helpers
{
    public static class EliminacionRegistroWeb
    {
        private static readonly CultureInfo CulturaAr = new CultureInfo("es-AR");

        // Modelo del boton. En la cuenta de produccion se ofrece la lista de usuarios activos (la persona real elige su nombre y
        // pone su clave); en una sesion normal se confirma al propio usuario.
        public static EliminarRegistroVm CrearVm(string modulo, int idRegistro, string sustantivo, string? urlPrevisualizar, string? urlEliminar,
            string? urlRedireccion, Entidades.Usuario usuarioSesion, Negocio.Usuario oUsuarioN, string? motivoDeshabilitado = null)
        {
            var vm = new EliminarRegistroVm
            {
                Modulo = modulo,
                IdRegistro = idRegistro,
                Sustantivo = sustantivo,
                UrlPrevisualizar = urlPrevisualizar ?? "",
                UrlEliminar = urlEliminar ?? "",
                UrlRedireccion = urlRedireccion ?? "",
                EsCuentaProduccion = usuarioSesion.EsUsuarioProduccion,
                IdUsuarioSesion = usuarioSesion.Id,
                NombreUsuarioSesion = usuarioSesion.Nombre ?? "",
                MotivoDeshabilitado = motivoDeshabilitado
            };

            if (usuarioSesion.EsUsuarioProduccion)
            {
                var dt = oUsuarioN.obtenerUsuarios(true);
                if (dt != null && dt.Columns.Contains("id") && dt.Columns.Contains("nombre"))
                {
                    vm.Usuarios = dt.AsEnumerable()
                        .Select(r => new EliminarRegistroVm.UsuarioOpcion { Id = System.Convert.ToInt32(r["id"]), Nombre = System.Convert.ToString(r["nombre"]) ?? "" })
                        .Where(u => u.Id > 0 && !string.IsNullOrWhiteSpace(u.Nombre))
                        .OrderBy(u => u.Nombre, System.StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
            }

            return vm;
        }

        // Quien puede ver los registros eliminados de un modulo (Registros eliminados, boton "Ver eliminados" de cada Index): el admin, o
        // quien tiene permiso de MODIFICAR ese modulo (el mismo con el que se elimina). Ve TODOS los eliminados del modulo, no solo los
        // suyos. La cuenta compartida de produccion no ve nada si no es admin: no hay una persona identificada.
        public static bool PuedeVerEliminados(Entidades.Usuario? usuario, Negocio.Usuario oUsuarioN, string modulo)
        {
            if (usuario == null) return false;
            if (usuario.Admin) return true;
            if (usuario.EsUsuarioProduccion) return false;

            string? permiso = modulo == Entidades.AuditoriaEliminacion.ModuloCompra ? Entidades.Permisos.Compra.ModificarCompra
                : modulo == Entidades.AuditoriaEliminacion.ModuloStock ? Entidades.Permisos.Stock.AddOrEditStock
                : modulo == Entidades.AuditoriaEliminacion.ModuloMovimiento ? Entidades.Permisos.Movimiento.NuevoMovimiento
                : null;
            if (permiso == null) return false;

            return oUsuarioN.tienePermiso(usuario, permiso, System.DateTime.Today, usuario.Id);
        }

        // Modulos cuyos eliminados puede ver el usuario (vacia = ninguno).
        public static System.Collections.Generic.List<string> ModulosConEliminadosVisibles(Entidades.Usuario? usuario, Negocio.Usuario oUsuarioN)
        {
            return new[]
            {
                Entidades.AuditoriaEliminacion.ModuloCompra,
                Entidades.AuditoriaEliminacion.ModuloStock,
                Entidades.AuditoriaEliminacion.ModuloMovimiento
            }.Where(m => PuedeVerEliminados(usuario, oUsuarioN, m)).ToList();
        }

        // ===== Recuperar un registro eliminado (2026-10-22) =====
        // "Recuperar" NO restaura el registro original: abre la pantalla de alta del modulo precargada con la cabecera y las lineas
        // que quedaron en la auditoria, y la persona decide si lo guarda de nuevo (se guarda como un registro NUEVO, con otro numero).

        // null si se puede recuperar; si no, el motivo para mostrarle al usuario.
        public static string? MotivoNoRecuperable(Entidades.AuditoriaEliminacion auditoria)
        {
            if (auditoria == null) return "El registro eliminado no existe.";
            if (string.Equals(auditoria.TipoRegistro, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.AjusteStock), System.StringComparison.OrdinalIgnoreCase))
                return "Un ajuste de stock no se recupera: se vuelve a generar desde el pesaje que ajustaba (Generar ajuste).";
            return null;
        }

        // Quien puede abrir el alta del modulo para recuperar: admin o quien tiene el permiso de alta (mismo que el de la pantalla de alta).
        public static bool PuedeCrearComoRecuperacion(Entidades.Usuario? usuario, Negocio.Usuario oUsuarioN, string modulo)
        {
            if (usuario == null) return false;
            if (usuario.Admin) return true;
            if (usuario.EsUsuarioProduccion) return false;

            string? permiso = modulo == Entidades.AuditoriaEliminacion.ModuloCompra ? Entidades.Permisos.Compra.NuevaCompra
                : modulo == Entidades.AuditoriaEliminacion.ModuloStock ? Entidades.Permisos.Stock.AddOrEditStock
                : modulo == Entidades.AuditoriaEliminacion.ModuloMovimiento ? Entidades.Permisos.Movimiento.NuevoMovimiento
                : null;
            return permiso != null && oUsuarioN.tienePermiso(usuario, permiso, System.DateTime.Today, usuario.Id);
        }

        // Lee la auditoria para precargar un alta. null + error si no existe, es de otro modulo, no se puede recuperar o el usuario
        // no puede ver los eliminados de ese modulo.
        public static Entidades.AuditoriaEliminacion? CargarParaRecuperar(Negocio.AuditoriaEliminaciones auditoriaN, Negocio.Usuario oUsuarioN,
            Entidades.Usuario usuario, int idAuditoria, string modulo, out string error)
        {
            error = "";
            Entidades.AuditoriaEliminacion? auditoria = null;
            try
            {
                auditoria = auditoriaN.obtener(idAuditoria);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Trace.TraceError("No se pudo leer la auditoria " + idAuditoria + " para recuperar: " + ex);
                error = "No se pudo leer el registro eliminado.";
                return null;
            }

            if (auditoria == null || auditoria.Modulo != modulo) { error = "El registro eliminado no existe."; return null; }
            if (!PuedeVerEliminados(usuario, oUsuarioN, modulo)) { error = "No tiene permiso para ver los registros eliminados."; return null; }

            string? noRecuperable = MotivoNoRecuperable(auditoria);
            if (noRecuperable != null) { error = noRecuperable; return null; }
            return auditoria;
        }

        // Cada linea guarda su "detalle" como texto ("Balanza", "No contado", "Tropa T-5", separados por " | ").
        public static bool DetalleTiene(string? detalle, string marca) =>
            !string.IsNullOrEmpty(detalle) && detalle.IndexOf(marca, System.StringComparison.OrdinalIgnoreCase) >= 0;

        public static string TropaDe(string? detalle)
        {
            if (string.IsNullOrEmpty(detalle)) return "";
            int i = detalle.IndexOf("Tropa ", System.StringComparison.OrdinalIgnoreCase);
            return i < 0 ? "" : detalle.Substring(i + 6).Split('|')[0].Trim();
        }

        // Cartel de la pantalla de alta precargada.
        public static string TextoRecuperando(Entidades.AuditoriaEliminacion a, string sustantivo, IEnumerable<string>? avisos = null)
        {
            string texto = "Estás recuperando " + sustantivo + " #" + a.IdRegistro + ", eliminad" + (sustantivo.StartsWith("la ") ? "a" : "o") + " el "
                + a.FechaEliminacion.ToString("dd/MM/yyyy HH:mm", CulturaAr) + " por " + (a.UsuarioEliminoNombre ?? "-")
                + ". Revisá los datos y guardá para cargarlo de nuevo: se guarda como un registro NUEVO (con otro número).";
            if (avisos != null)
            {
                foreach (string aviso in avisos) texto += " " + aviso;
            }
            return texto;
        }

        // Se abrio por URL (un link guardado, el extracto de cta cte, un borrador) un registro que ya fue eliminado: en vez de un 404
        // mudo se explica quien lo elimino, cuando y por que. Quien puede ver los eliminados de ese modulo (PuedeVerEliminados) va al
        // detalle completo; el resto vuelve al listado con el aviso.
        // Devuelve null si el registro no figura como eliminado (queda el 404 de siempre) o si la auditoria no se pudo leer.
        public static IActionResult? ResolverRegistroEliminado(Controller controller, Entidades.Usuario usuario, Negocio.Usuario oUsuarioN,
            Negocio.AuditoriaEliminaciones auditoriaN, string modulo, int idRegistro, string controllerListado)
        {
            Entidades.AuditoriaEliminacion auditoria;
            try
            {
                auditoria = auditoriaN.obtenerPorRegistro(modulo, idRegistro);
            }
            catch (System.Exception ex)
            {
                // Sin la tabla de auditoria (migracion sin aplicar) el comportamiento es el de antes: 404. No se oculta: queda en el log.
                System.Diagnostics.Trace.TraceError("No se pudo leer la auditoria de eliminaciones (" + modulo + " " + idRegistro + "): " + ex);
                return null;
            }

            if (auditoria == null) return null;

            if (PuedeVerEliminados(usuario, oUsuarioN, modulo))
                return controller.RedirectToAction("Detalle", "RegistrosEliminados", new { id = auditoria.Id });

            controller.TempData["AlertType"] = "warning";
            controller.TempData["AlertTitle"] = "Registro eliminado";
            controller.TempData["AlertMsg"] = "Este registro fue eliminado el " + auditoria.FechaEliminacion.ToString("dd/MM/yyyy HH:mm", CulturaAr)
                + " por " + (auditoria.UsuarioEliminoNombre ?? "-") + ". Motivo: " + auditoria.Motivo;
            return controller.RedirectToAction("Index", controllerListado);
        }

        public static object VistaPreviaJson(Negocio.VistaPreviaEliminacion v)
        {
            return new
            {
                ok = true,
                modulo = v.Modulo,
                idRegistro = v.IdRegistro,
                tipo = v.TipoRegistro,
                fecha = v.FechaRegistro.ToString("dd/MM/yyyy HH:mm", CulturaAr),
                sucursal = v.SucursalNombre,
                sucursalDestino = v.SucursalDestinoNombre,
                persona = v.PersonaNombre,
                nroRemito = v.NroRemito,
                enCtaCte = v.EnCtaCte,
                importe = v.Importe,
                importeTexto = "$ " + v.Importe.ToString("N2", CulturaAr),
                totalKg = v.TotalKg,
                totalKgTexto = v.TotalKg.ToString("N2", CulturaAr),
                cantLineas = v.CantLineas,
                requiereAdmin = v.RequiereAdmin,
                efectoCtaCte = v.EfectoCtaCte,
                avisos = v.Avisos,
                bloqueos = v.Bloqueos,
                lineas = v.Lineas.Select(l => new
                {
                    codigo = l.Codigo.HasValue ? l.Codigo.Value.ToString(CulturaAr) : "",
                    producto = l.Producto ?? "",
                    cantKg = l.CantKg.HasValue ? l.CantKg.Value.ToString("N3", CulturaAr) : "",
                    cantUnidad = l.CantUnidad.HasValue ? l.CantUnidad.Value.ToString(CulturaAr) : "",
                    precioKg = l.PrecioKg.HasValue && l.PrecioKg.Value != 0 ? l.PrecioKg.Value.ToString("N2", CulturaAr) : "",
                    detalle = l.Detalle ?? ""
                }).ToList()
            };
        }

        // Respuesta cuando la revalidacion de usuario y contraseña falla: mismo contrato que los otros step-up (bloqueado +
        // segundosRestantes para la cuenta regresiva; si no, mensaje).
        public static object RevalidacionFallidaJson(ReautenticacionEliminacion.Resultado r)
        {
            if (r.Bloqueado)
                return new { ok = false, bloqueado = true, segundosRestantes = r.SegundosBloqueo, mensaje = r.Mensaje };

            return new { ok = false, mensaje = r.Mensaje };
        }
    }
}
