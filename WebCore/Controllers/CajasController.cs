// Port PARCIAL de Web/Controllers/CajasController.cs (ver docs/DECISIONS.md, migracion ASP.NET
// Core, Modulo 7 -- Caja y tesoreria). El original tiene 1631 lineas y ~20 acciones. Primer slice
// (confirmado con el usuario): la pantalla "Cajas Abiertas" completa -- listado + historial +
// egresos de caja (Nuevo/Guardar/Actividades) + cierre de caja + cambio de sucursal. Portado:
// CajasAbiertas, HistorialCierresCaja, ObtenerDatosCierre, CerrarCaja, PreviewCambioSucursalCaja,
// CambiarSucursalCaja, ActividadesCaja, NuevoEgresoCaja, GuardarEgresoCaja, AbrirCaja. NO portados
// en este slice: EgresosCaja/TiposEgresoCaja/GuardarTipoEgresoCaja/EliminarTipoEgresoCaja/
// CalcularComisionesElectronicas/GuardarComisionesElectronicas/TiposEgresoCajaOpciones (pantalla
// administrativa separada, "Egresos de Caja", segundo slice de este modulo).
//
// Mismo criterio de stub que el resto de la migracion: IEmpresaContext + IParametrosContext
// reales, Entidades.Usuario stub (Id=2, Admin=true, IdEmpresa=1, IdSucursal=2, Nombre="ger").
//
// Autenticacion de step-up de Cierre de Caja (Web/Controllers/CajasController.cs:
// AutorizarAccionCierre/RevocarAutorizacionCierre, con CierreCajaStepUpRateLimiter) NO se porta:
// es un mecanismo para que un usuario SIN el permiso directo de cerrar caja pueda autorizar
// temporalmente tipeando la clave de otro usuario que si lo tiene. PermisosHelper.
// ObtenerUsuarioAutorizadoCierre(Session) resuelve primero TienePermisoVer(Cajas.CerrarCaja), que
// con Admin=true (bypass ya usado en toda la migracion) siempre da true -- el stub SIEMPRE tiene
// el permiso directo, asi que la rama de step-up nunca se ejecuta (mismo criterio que
// AutorizarModuloCompras en Compras, o SeleccionUsuarioController: codigo inalcanzable bajo este
// stub, no se reproduce con infraestructura de Session que WebCore no tiene). El front-end sigue
// intacto (window.CajasStepUpTienePermisoDirecto=true evita que el modal de autorizacion se abra
// nunca) y las URLs de esas 2 acciones quedan armadas en el JS aunque no exista el endpoint en el
// servidor -- no se disparan bajo ningun flujo de esta pantalla.
//
// El boton "Ventas" de cada fila abre Ventas/MisVentas (Modulo 8, POS, no portado) -- queda
// wireado igual que el original pero da 404 al clickear, gap ya aceptado en este mismo patron
// para toda dependencia de POS (ver Compras.desdePos).
//
// AbrirCaja se porta por fidelidad (accion simple) pero es codigo inalcanzable en este slice: su
// unico punto de entrada real es Views/Ventas/POS.cshtml (_AbrirCajaModal.cshtml), Modulo 8.
using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class CajasController : Controller
    {
        private sealed class StubEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private sealed class GuardarEgresoCajaResultado
        {
            public bool Ok { get; set; }
            public int Id { get; set; }
            public string Mensaje { get; set; } = "";
        }

        public sealed class CambioSucursalCajaPostVm
        {
            public int IdCierre { get; set; }
            public int IdSucursalNueva { get; set; }
        }

        private readonly IEmpresaContext _empresa = new StubEmpresaContext();
        private readonly IParametrosContext _param;
        private readonly Negocio.CierreCaja _oCierreN;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.Usuario _oUsuarioN;
        private readonly Negocio.Venta _oVentaN;

        private readonly Entidades.Usuario _usuarioActual = new Entidades.Usuario
        {
            Id = 2,
            Admin = true,
            IdEmpresa = 1,
            IdSucursal = 2,
            Nombre = "ger"
        };

        public CajasController()
        {
            _param = new Negocio.Parametros(_empresa);
            _param.Reload();

            _oCierreN = new Negocio.CierreCaja(_empresa, _param);
            _oSucursalN = new Negocio.Sucursal(_empresa, _param);
            _oUsuarioN = new Negocio.Usuario(_empresa, _param);
            _oVentaN = new Negocio.Venta(_empresa, _param);
        }

        [HttpGet]
        public IActionResult CajasAbiertas(int? idSucursal, string buscar = "", DateTime? fechaDesde = null, bool ajax = false)
        {
            var user = _usuarioActual;
            bool tienePermisoCerrarCaja = true;

            var sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            int? idSucursalSeleccionada = ResolverSucursalSeleccionada(sucursales, idSucursal);
            DateTime desde = fechaDesde ?? DateTime.Today.AddDays(-7);

            ViewBag.Sucursales = sucursales;
            ViewBag.HayVariasSucursales = sucursales.Count > 1;
            ViewBag.IdSucursal = idSucursalSeleccionada;
            ViewBag.Buscar = buscar ?? "";
            ViewBag.FechaDesde = desde;
            ViewBag.PuedeModificarCierres = true;
            ViewBag.TienePermisoCerrarCaja = tienePermisoCerrarCaja;
            ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo();
            ViewBag.HistorialCierres = tienePermisoCerrarCaja
                ? ObtenerHistorialCierresCaja(idSucursalSeleccionada, buscar ?? "", desde, sucursales)
                : new DataTable();

            var dt = ObtenerCajasAbiertas(idSucursalSeleccionada, buscar ?? "");

            if (ajax)
                return PartialView("~/Views/Cajas/_TablaCajasAbiertas.cshtml", dt);

            ViewBag.Title = "Cajas Abiertas";
            return View("~/Views/Cajas/CajasAbiertas.cshtml", dt);
        }

        [HttpGet]
        public IActionResult HistorialCierresCaja(int? idSucursal, string buscar = "", DateTime? fechaDesde = null)
        {
            var sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            int? idSucursalSeleccionada = ResolverSucursalSeleccionada(sucursales, idSucursal);
            DateTime desde = fechaDesde ?? DateTime.Today.AddDays(-7);

            ViewBag.PuedeModificarCierres = true;
            var dt = ObtenerHistorialCierresCaja(idSucursalSeleccionada, buscar ?? "", desde, sucursales);
            return PartialView("~/Views/Cajas/_TablaCierresDeCaja.cshtml", dt);
        }

        [HttpGet]
        public IActionResult ObtenerDatosCierre(int id, bool modoModificacion = false)
        {
            var usuarioAutorizado = _usuarioActual;

            Entidades.CierreCaja oCierreE = new CierreCaja();
            oCierreE.Id = id;
            var caja = oCierreE = _oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindById, "");

            if (caja == null || caja.Id == 0)
                return NotFound("No se encontró la caja seleccionada.");

            DateTime fechaHastaVentas = modoModificacion && caja.FechaHoraCierre.HasValue
                ? caja.FechaHoraCierre.Value
                : DateTime.Now;

            caja.EgresosCaja = _oCierreN.getMontoEgresosCajaVendedor(oCierreE);

            return Json(new
            {
                id = caja.Id,
                suc = caja.Sucursal.SucursalNombre,
                vendedor = caja.UsuarioInicio.Nombre,
                cajaInicial = caja.CajaInicio,
                fechaApertura = caja.FechaHoraInicio.Value.ToString("dd/MM/yyyy HH:mm"),
                fechaCierre = caja.FechaHoraCierre.HasValue ? caja.FechaHoraCierre.Value.ToString("dd/MM/yyyy HH:mm") : "",
                usuario = (caja.UsuarioCierre != null && caja.UsuarioCierre.Id > 0)
                    ? caja.UsuarioCierre.Nombre
                    : usuarioAutorizado.Nombre,
                ventas = _oCierreN.obtenerTotalVentas(oCierreE.UsuarioInicio.Id, oCierreE.Sucursal.idSucursal,
                        oCierreE.FechaHoraInicio, fechaHastaVentas).ToString(),
                egresosCaja = caja.EgresosCaja,
                cajaCierre = caja.CajaCierre,
                diferencia = caja.Diferencia,
                importeRetirado = caja.ImporteRetirado,
                cajaInicioSiguiente = caja.CajaInicioSiguiente,
                modoModificacion = modoModificacion
            });
        }

        public IActionResult CerrarCaja(
                int Id,
                string CajaCierre,
                string Diferencia,
                string ImporteRetirado,
                string CajaInicioSiguiente,
                bool modoModificacion = false
            )
        {
            var usuarioAutorizado = _usuarioActual;

            Entidades.CierreCaja model = _oCierreN.findByIdOrLast(
                new CierreCaja { Id = Id },
                Entidades.CierreCaja.tipoBusqueda.FindById,
                ""
            );

            if (model == null || model.Id == 0)
                return Json(new { ok = false, error = "No se encontró la caja seleccionada." });

            if (modoModificacion && !model.FechaHoraCierre.HasValue)
                return Json(new { ok = false, error = "La caja seleccionada todavía no tiene cierre para modificar." });

            model.CajaCierre = ParseFloat(CajaCierre);
            model.Diferencia = ParseFloat(Diferencia);
            model.ImporteRetirado = ParseFloat(ImporteRetirado);
            model.CajaInicioSiguiente = ParseFloat(CajaInicioSiguiente);
            model.UsuarioCierre = usuarioAutorizado;
            model.FechaHoraCierre = modoModificacion
                ? model.FechaHoraCierre
                : (model.FechaHoraCierre != null ? model.FechaHoraCierre : DateTime.Now);
            model.Ventas = _oCierreN.obtenerTotalVentas(model.UsuarioInicio.Id, model.Sucursal.idSucursal,
                    model.FechaHoraInicio, modoModificacion ? model.FechaHoraCierre : DateTime.Now);
            model.EgresosCaja = _oCierreN.getMontoEgresosCajaVendedor(model);

            var result = _oCierreN.addOrEditCierreCaja_Result(model);

            if (!result.Ok)
                return Json(new { ok = false, error = result.Mensaje });

            return Json(new
            {
                ok = true,
                mensaje = modoModificacion ? "El cierre de caja se actualizó correctamente." : result.Mensaje
            });
        }

        [HttpGet]
        public IActionResult PreviewCambioSucursalCaja(int idCierre, int idSucursalNueva)
        {
            var sucursales = _oSucursalN.findAll();
            if (!PuedeCambiarSucursalCaja(sucursales))
                return Json(new { ok = false, mensaje = "No tiene permisos para cambiar la sucursal de una caja." });

            var cierre = _oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "");
            if (cierre == null || cierre.Id == 0)
                return Json(new { ok = false, mensaje = "No se encontro la caja seleccionada." });
            if (!CajaSigueAbierta(cierre))
                return Json(new { ok = false, mensaje = "La caja seleccionada ya no se encuentra abierta." });

            var preview = _oCierreN.obtenerPreviewCambioSucursalCaja(cierre, idSucursalNueva);
            return Json(new
            {
                ok = true,
                puedeEjecutar = preview.PuedeEjecutar,
                mensaje = preview.Mensaje,
                tieneCajaAbiertaEnDestino = preview.TieneCajaAbiertaEnDestino,
                idCierreCaja = preview.IdCierreCaja,
                sucursalActual = preview.SucursalActual,
                sucursalNueva = preview.SucursalNueva,
                usuarioCaja = preview.UsuarioCaja,
                fechaDesde = preview.FechaDesde.ToString("dd/MM/yyyy HH:mm"),
                fechaHasta = preview.FechaHasta.ToString("dd/MM/yyyy HH:mm"),
                tablas = preview.Tablas.Select(t => new { tabla = t.Tabla, cantidad = t.Cantidad }).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarSucursalCaja(CambioSucursalCajaPostVm model)
        {
            var user = _usuarioActual;
            var sucursales = _oSucursalN.findAll();
            if (!PuedeCambiarSucursalCaja(sucursales))
                return Json(new { ok = false, mensaje = "No tiene permisos para cambiar la sucursal de una caja." });

            if (model == null || model.IdCierre <= 0 || model.IdSucursalNueva <= 0)
                return Json(new { ok = false, mensaje = "Datos invalidos para cambiar la sucursal." });

            var cierre = _oCierreN.findByIdOrLast(new CierreCaja { Id = model.IdCierre }, CierreCaja.tipoBusqueda.FindById, "");
            if (cierre == null || cierre.Id == 0)
                return Json(new { ok = false, mensaje = "No se encontro la caja seleccionada." });
            if (!CajaSigueAbierta(cierre))
                return Json(new { ok = false, mensaje = "La caja seleccionada ya no se encuentra abierta." });

            var resultado = _oCierreN.cambiarSucursalCaja(cierre, model.IdSucursalNueva, user.Id, user.Nombre);
            return Json(new
            {
                ok = resultado.Ok,
                mensaje = resultado.Mensaje,
                tablas = resultado.Tablas.Select(t => new { tabla = t.Tabla, cantidad = t.Cantidad }).ToList()
            });
        }

        [HttpGet]
        public IActionResult ActividadesCaja(int idCierre, string filtroActividad = "todos")
        {
            var user = _usuarioActual;

            if (idCierre <= 0)
                return BadRequest("Caja inválida.");

            var cierre = _oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "");
            if (cierre == null || cierre.Id == 0)
                return NotFound("No se encontró la caja seleccionada.");

            DataTable dt = _oCierreN.getEgresosCajaVendedor(cierre);
            dt = FiltrarActividadesCaja(dt, filtroActividad);

            string nombreVendedor = cierre.UsuarioInicio != null ? cierre.UsuarioInicio.Nombre : "cajero";
            string nombreSucursal = cierre.Sucursal != null ? cierre.Sucursal.SucursalNombre : "";

            ViewBag.DesdePOS = false;
            ViewBag.SoloEgresos = false;
            ViewBag.FiltroActividad = filtroActividad ?? "todos";
            ViewBag.CierreCaja = cierre;
            ViewBag.TiposEgresoCaja = _oCierreN.obtenerTiposEgresoCaja("", 0);
            ViewBag.TotalVisible = CalcularTotalGastosCaja(dt);
            ViewBag.MostrarResumenMisActividades = true;
            ViewBag.ModoActividades = true;
            ViewBag.PermitirNuevo = CajaSigueAbierta(cierre);
            ViewBag.IdCierreActividad = idCierre;
            ViewBag.SucursalActividad = nombreSucursal;
            ViewBag.TituloActividades = "Actividades";
            ViewBag.SubtituloActividades = string.IsNullOrWhiteSpace(nombreSucursal)
                ? nombreVendedor
                : nombreVendedor + " | " + nombreSucursal;
            CargarPermisosEdicionEgresos(dt, false, cierre);

            return PartialView("~/Views/Cajas/_MisEgresosCaja.cshtml", dt);
        }

        [HttpGet]
        public IActionResult NuevoEgresoCaja(int id = 0, bool desdePos = false, int idCierre = 0)
        {
            var user = _usuarioActual;

            CierreCaja? cierreContexto = idCierre > 0
                ? _oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "")
                : null;

            if (idCierre > 0)
            {
                if (cierreContexto == null || cierreContexto.Id == 0)
                    return NotFound("No se encontró la caja seleccionada.");

                if (!CajaSigueAbierta(cierreContexto))
                    return BadRequest("La caja seleccionada ya no se encuentra abierta.");
            }

            EgresoCaja egreso;
            int idSucursal = cierreContexto != null && cierreContexto.Sucursal != null
                ? cierreContexto.Sucursal.idSucursal
                : user.IdSucursal;

            if (id > 0)
            {
                egreso = _oCierreN.getEgresoCajaById(id);
                if (egreso == null || egreso.Id == 0)
                    return NotFound("Egreso de caja no encontrado.");

                var validacion = EgresosCajaPolicy.EvaluarModificacion(user, egreso, desdePos, _empresa, _oCierreN.validarCajaAbiertaVendedor);
                if (!validacion.PuedeModificar)
                    return BadRequest(validacion.MensajeBloqueo);

                if (cierreContexto != null && !FechaDentroDeCaja(egreso.Fecha, cierreContexto))
                    return BadRequest("El egreso seleccionado no corresponde al rango horario de la caja abierta.");

                idSucursal = egreso.Sucursal != null && egreso.Sucursal.idSucursal > 0 ? egreso.Sucursal.idSucursal : idSucursal;
            }
            else
            {
                egreso = new EgresoCaja
                {
                    Fecha = DateTime.Now,
                    Sucursal = _oSucursalN.findById(idSucursal),
                    CreadoPor = cierreContexto != null && cierreContexto.UsuarioInicio != null ? cierreContexto.UsuarioInicio.Id : user.Id,
                    CreadoPorUser = cierreContexto != null ? cierreContexto.UsuarioInicio : user
                };
            }

            CargarViewBagsFormularioEgreso(desdePos, idSucursal, cierreContexto);
            ViewBag.EsEdicion = id > 0;
            ViewBag.IdCierreActividad = idCierre;

            if (egreso.Sucursal == null || egreso.Sucursal.idSucursal == 0)
                egreso.Sucursal = _oSucursalN.findById(idSucursal);

            return PartialView("~/Views/Cajas/_AddOrEditEgresoCaja.cshtml", egreso);
        }

        [HttpPost]
        public IActionResult GuardarEgresoCaja(int id, DateTime fecha, int idTipoEgresoCaja, string descripcion, string monto, string detalle, int idSucursal, bool desdePos = false, int idCierre = 0)
        {
            try
            {
                var resultado = GuardarEgresoCajaCore(id, fecha, idTipoEgresoCaja, descripcion, monto, detalle, idSucursal, desdePos, idCierre);
                return Json(new { ok = resultado.Ok, id = resultado.Id, mensaje = resultado.Mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = "Error al guardar egreso de caja. " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AbrirCaja(string cajaInicio, string fechaHora, string posInstanceId = null)
        {
            try
            {
                var user = _usuarioActual;
                var operador = user;

                float cajaInicio_ = ParseFloat(cajaInicio);
                if (cajaInicio_ <= 0)
                    return Json(new { ok = false, mensaje = "Importe inválido" });

                if (!DateTime.TryParseExact(
                        fechaHora,
                        "dd/MM/yyyy HH:mm",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime fechaApertura))
                {
                    return Json(new { ok = false, mensaje = "Fecha inválida" });
                }

                var nuevoCierre = new Entidades.CierreCaja
                {
                    Sucursal = _oSucursalN.findById(user.IdSucursal),
                    UsuarioInicio = operador,
                    FechaHoraInicio = fechaApertura,
                    CajaInicio = cajaInicio_
                };

                Entidades.CierreCaja cierre = _oCierreN.findByIdOrLast(
                    nuevoCierre,
                    Entidades.CierreCaja.tipoBusqueda.FindLast,
                    ""
                );

                if (cierre != null && cierre.UsuarioCierre == null)
                    return Json(new { ok = false, mensaje = "Ya existe una caja abierta" });

                _oCierreN.addOrEditCierreCaja(nuevoCierre);

                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        // ---- Slice 2: pantalla administrativa "Egresos de Caja" (EgresosCaja/TiposEgresoCaja) ----
        // Mismo criterio de stub que el resto del controller. PermisosHelper.TienePermisoVer/
        // TienePermisoEditar se omiten (siempre true bajo Admin=true) salvo donde el original
        // ya usaba un chequeo real de negocio (ej. "!user.Admin" en EliminarTipoEgresoCaja, que
        // se deja tal cual aunque nunca se dispare con este stub).

        [HttpGet]
        public IActionResult EgresosCaja(int? idSucursal, int idUsuario = -1, int idTipoEgresoCaja = 0, string descripcion = "", DateTime? fechaDesde = null, DateTime? fechaHasta = null, string filtroGasto = "todos", bool ajax = false)
        {
            DateTime desde = fechaDesde ?? DateTime.Today;
            DateTime hasta = fechaHasta ?? DateTime.Today.AddDays(1).AddSeconds(-1);
            if (hasta.TimeOfDay == TimeSpan.Zero)
                hasta = hasta.AddDays(1).AddSeconds(-1);

            int sucursalSeleccionada = idSucursal ?? 0;

            CargarViewBagsEgresos(sucursalSeleccionada, idUsuario, idTipoEgresoCaja, descripcion, desde, hasta, filtroGasto, false);

            DataTable dt = _oCierreN.obtenerEgresosCaja(sucursalSeleccionada, idUsuario, idTipoEgresoCaja, descripcion ?? "", desde, hasta);
            dt = ExcluirTiposReservadosEgresosCaja(dt);
            CargarPermisosEdicionEgresos(dt, false);

            if (ajax)
                return PartialView("~/Views/Cajas/_EgresosCajaTabla.cshtml", dt);

            ViewBag.Title = "Egresos de Caja";
            return View("~/Views/Cajas/EgresosCaja.cshtml", dt);
        }

        [HttpGet]
        public IActionResult TiposEgresoCaja(string buscar = "")
        {
            var user = _usuarioActual;

            ViewBag.BuscarTipoEgreso = buscar ?? "";
            ViewBag.PuedeEditarTipos = true;
            ViewBag.UsuarioAdmin = user.Admin;

            DataTable dt = _oCierreN.obtenerTiposEgresoCaja(buscar ?? "", 0);
            return PartialView("~/Views/Cajas/_TiposEgresoCajaModal.cshtml", dt);
        }

        [HttpGet]
        public IActionResult AddOrEditTipoEgresoCaja(int id = 0)
        {
            var model = new TipoEgresoCajaEditVm();
            if (id > 0)
            {
                DataTable dt = _oCierreN.obtenerTiposEgresoCaja("", id);
                if (dt == null || dt.Rows.Count == 0)
                    return NotFound("No se encontró el tipo de egreso seleccionado.");

                DataRow row = dt.Rows[0];
                bool reservado = row.Table.Columns.Contains("Reservado") && row["Reservado"] != DBNull.Value && Convert.ToBoolean(row["Reservado"]);
                if (reservado)
                    return BadRequest("El tipo de egreso seleccionado es reservado por el sistema y no puede modificarse.");

                model.Id = id;
                model.TipoEgresoCaja = Convert.ToString(row["tipoEgresoCaja"]) ?? "";
                model.EsGasto = row.Table.Columns.Contains("Es_Gasto") && row["Es_Gasto"] != DBNull.Value && Convert.ToBoolean(row["Es_Gasto"]);
                model.Reservado = reservado;
            }

            return PartialView("~/Views/Cajas/_AddOrEditTipoEgresoCaja.cshtml", model);
        }

        [HttpGet]
        public IActionResult TiposEgresoCajaOpciones()
        {
            DataTable dt = _oCierreN.obtenerTiposEgresoCaja("", 0);
            var items = dt.AsEnumerable()
                .Where(r => Convert.ToInt32(r["id"]) > 0)
                .Select(r => new
                {
                    id = Convert.ToInt32(r["id"]),
                    nombre = Convert.ToString(r["tipoEgresoCaja"])
                })
                .ToList();

            return Json(new { ok = true, items = items });
        }

        [HttpGet]
        public IActionResult CalcularComisionesElectronicas(DateTime? fechaDesde = null, DateTime? fechaHasta = null, int idSucursal = 0, bool desdePos = false, int idCierre = 0)
        {
            var user = _usuarioActual;

            CierreCaja? cierreContexto = idCierre > 0
                ? _oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "")
                : null;

            if (idCierre > 0)
            {
                if (cierreContexto == null || cierreContexto.Id == 0)
                    return NotFound("No se encontró la caja seleccionada.");

                if (!CajaSigueAbierta(cierreContexto))
                    return BadRequest("La caja seleccionada ya no se encuentra abierta.");
            }

            DateTime desde = (fechaDesde ?? DateTime.Today).Date;
            DateTime hasta = (fechaHasta ?? DateTime.Today).Date;
            if (desde > hasta)
                return BadRequest("La fecha desde no puede ser mayor que la fecha hasta.");

            bool sucursalFija = desdePos || (cierreContexto != null && cierreContexto.Sucursal != null);
            int sucursalSeleccionada = desdePos
                ? user.IdSucursal
                : (cierreContexto != null && cierreContexto.Sucursal != null
                    ? cierreContexto.Sucursal.idSucursal
                    : idSucursal);

            var sucursal = sucursalSeleccionada > 0 ? _oSucursalN.findById(sucursalSeleccionada) : null;
            if (sucursalSeleccionada > 0 && sucursal == null)
                return BadRequest("Sucursal inválida.");

            if (desdePos && !_oCierreN.validarCajaAbiertaVendedor(DateTime.Now, sucursal, user))
                return BadRequest("Debe tener una caja abierta en la sucursal activa para registrar egresos desde POS.");

            var model = CrearModelComisionesElectronicas(desde, hasta, DateTime.Now, sucursalSeleccionada, desdePos, idCierre);
            ViewBag.TiposEgresoCaja = _oCierreN.obtenerTiposEgresoCaja("", 0);
            ViewBag.Sucursales = _oSucursalN.findAll();
            ViewBag.MostrarSelectorSucursal = !sucursalFija;
            return PartialView("~/Views/Cajas/_CalcularComisionesElectronicas.cshtml", model);
        }

        [HttpGet]
        public IActionResult ObtenerResumenComisionesElectronicas(DateTime fechaDesde, DateTime fechaHasta, int idSucursal)
        {
            try
            {
                if (fechaDesde.Date > fechaHasta.Date)
                    return Json(new { ok = false, mensaje = "La fecha desde no puede ser mayor que la fecha hasta." });

                int? sucursalConsulta = idSucursal > 0 ? (int?)idSucursal : null;
                if (sucursalConsulta.HasValue && _oSucursalN.findById(sucursalConsulta.Value) == null)
                    return Json(new { ok = false, mensaje = "Sucursal inválida." });

                var formas = ObtenerFormasPagoElectronicas(fechaDesde.Date, fechaHasta.Date, sucursalConsulta, null);
                return Json(new
                {
                    ok = true,
                    items = formas.Select(f => new
                    {
                        codigo = f.Codigo,
                        nombre = f.Nombre,
                        totalCobrado = f.TotalCobrado
                    }).ToList(),
                    total = formas.Sum(f => f.TotalCobrado)
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = "No se pudieron calcular las comisiones. " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarTipoEgresoCaja(TipoEgresoCajaEditVm model)
        {
            try
            {
                string nombre = model != null ? (model.TipoEgresoCaja ?? "").Trim() : "";
                if (string.IsNullOrWhiteSpace(nombre))
                    return Json(new { ok = false, mensaje = "El campo Tipo no puede ser vacío." });

                int id = model != null ? model.Id : 0;
                if (id > 0)
                {
                    DataTable dt = _oCierreN.obtenerTiposEgresoCaja("", id);
                    if (dt == null || dt.Rows.Count == 0)
                        return Json(new { ok = false, mensaje = "No se encontró el tipo de egreso seleccionado." });

                    bool reservado = dt.Rows[0].Table.Columns.Contains("Reservado") &&
                                     dt.Rows[0]["Reservado"] != DBNull.Value &&
                                     Convert.ToBoolean(dt.Rows[0]["Reservado"]);
                    if (reservado)
                        return Json(new { ok = false, mensaje = "El tipo de egreso seleccionado es reservado por el sistema y no puede modificarse." });
                }

                _oCierreN.addOrEditTipoEgreso(id > 0 ? id : -1, nombre, model != null && model.EsGasto);
                return Json(new { ok = true, mensaje = "El Tipo Egreso se registró correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarTipoEgresoCaja(int id)
        {
            try
            {
                var user = _usuarioActual;

                if (!user.Admin)
                    return Json(new { ok = false, mensaje = "Debe tener permiso de Administrador para eliminar un Tipo Egreso." });

                DataTable dt = _oCierreN.obtenerTiposEgresoCaja("", id);
                if (dt == null || dt.Rows.Count == 0)
                    return Json(new { ok = false, mensaje = "No se encontró el tipo de egreso seleccionado." });

                bool reservado = dt.Rows[0].Table.Columns.Contains("Reservado") &&
                                 dt.Rows[0]["Reservado"] != DBNull.Value &&
                                 Convert.ToBoolean(dt.Rows[0]["Reservado"]);
                if (reservado)
                    return Json(new { ok = false, mensaje = "El Tipo Egreso seleccionado es reservado por el sistema y no puede eliminarse." });

                _oCierreN.eliminarTipoEgreso(id);
                return Json(new { ok = true, mensaje = "El Tipo Egreso se eliminó correctamente." });
            }
            catch (Exception ex)
            {
                string msg = ex.Message != null && ex.Message.IndexOf("FK", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "No se puede eliminar porque existen egresos de caja con el Tipo Egreso seleccionado."
                    : ex.Message;
                return Json(new { ok = false, mensaje = msg });
            }
        }

        [HttpPost]
        public IActionResult GuardarComisionesElectronicas(DateTime fechaDesde, DateTime fechaHasta, DateTime fechaEgreso, int idTipoEgresoCaja, int idSucursal, string porcentajeDebito, string porcentajeCredito, string porcentajeQr, string porcentajeTransferencia, bool desdePos = false, int idCierre = 0)
        {
            try
            {
                var user = _usuarioActual;

                DateTime desde = fechaDesde.Date;
                DateTime hasta = fechaHasta.Date;
                if (desde > hasta)
                    return Json(new { ok = false, mensaje = "La fecha desde no puede ser mayor que la fecha hasta." });

                var porcentajes = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Debito", ParseDecimalFlexible(porcentajeDebito) },
                    { "Credito", ParseDecimalFlexible(porcentajeCredito) },
                    { "Qr", ParseDecimalFlexible(porcentajeQr) },
                    { "Transferencia", ParseDecimalFlexible(porcentajeTransferencia) }
                };

                int? sucursalFiltro = idSucursal > 0 ? (int?)idSucursal : null;
                if (sucursalFiltro.HasValue && _oSucursalN.findById(sucursalFiltro.Value) == null)
                    return Json(new { ok = false, mensaje = "Sucursal inválida." });

                var formas = ObtenerFormasPagoElectronicas(desde, hasta, sucursalFiltro, porcentajes);
                decimal totalComisiones = formas.Sum(f => f.ImporteComision);
                if (totalComisiones <= 0)
                    return Json(new { ok = false, mensaje = "Debe existir al menos una comisión mayor a cero para guardar el egreso." });

                string sucursalDescripcion = ObtenerDescripcionSucursal(idSucursal);
                string descripcion = string.Format(
                    "Periodo {0} - {1} | Sucursal: {2}",
                    desde.ToString("dd/MM/yyyy"),
                    hasta.ToString("dd/MM/yyyy"),
                    sucursalDescripcion);

                StringBuilder detalleBuilder = new StringBuilder();
                detalleBuilder.AppendFormat(
                    "Cálculo automático de comisiones por pagos electrónicos entre {0} y {1} | Sucursal: {2}.",
                    desde.ToString("dd/MM/yyyy"),
                    hasta.ToString("dd/MM/yyyy"),
                    sucursalDescripcion);

                foreach (var forma in formas)
                {
                    detalleBuilder.AppendLine();
                    detalleBuilder.AppendFormat(
                        "{0}: total cobrado ${1} - comisión {2}% = ${3}",
                        forma.Nombre,
                        FormatearImporte(forma.TotalCobrado),
                        FormatearPorcentaje(forma.Porcentaje),
                        FormatearImporte(forma.ImporteComision));
                }

                int idSucursalGuardar = idSucursal > 0 ? idSucursal : user.IdSucursal;
                var resultado = GuardarEgresoCajaCore(
                    0,
                    fechaEgreso,
                    idTipoEgresoCaja,
                    descripcion,
                    totalComisiones.ToString(CultureInfo.InvariantCulture),
                    detalleBuilder.ToString(),
                    idSucursalGuardar,
                    desdePos,
                    idCierre);

                return Json(new { ok = resultado.Ok, id = resultado.Id, mensaje = resultado.Mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = "Error al guardar comisiones electrónicas. " + ex.Message });
            }
        }

        private float ParseFloat(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            value = value.Replace(",", ".");

            float result;
            if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;

            return 0;
        }

        private void CargarViewBagsFormularioEgreso(bool desdePos, int idSucursal, CierreCaja? cierreContexto = null)
        {
            var user = _usuarioActual;

            ViewBag.DesdePOS = desdePos;
            ViewBag.UsuarioActual = cierreContexto != null && cierreContexto.UsuarioInicio != null
                ? cierreContexto.UsuarioInicio
                : user;
            ViewBag.UsuarioOperando = user;
            ViewBag.UsuarioAdmin = user.Admin;
            ViewBag.Sucursales = _oSucursalN.findAll();
            ViewBag.TiposEgresoCaja = _oCierreN.obtenerTiposEgresoCaja("", 0);
            ViewBag.IdSucursal = idSucursal;
            ViewBag.IdCierreActividad = cierreContexto != null ? cierreContexto.Id : 0;
        }

        private List<object> ObtenerUsuariosActivosEmpresaParaCombo()
        {
            var dt = _oUsuarioN.obtenerUsuarios(true);
            if (dt == null || !dt.Columns.Contains("id") || !dt.Columns.Contains("nombre"))
                return new List<object>();

            return dt.AsEnumerable()
                .Select(row => new { id = ValorInt(row, "id"), nombre = ValorString(row, "nombre") })
                .Where(u => u.id > 0 && !string.IsNullOrWhiteSpace(u.nombre))
                .OrderBy(u => u.nombre, StringComparer.OrdinalIgnoreCase)
                .Cast<object>()
                .ToList();
        }

        private int? ResolverSucursalSeleccionada(List<Entidades.Sucursal> sucursales, int? idSucursal)
        {
            if (idSucursal.HasValue)
                return idSucursal.Value > 0 ? idSucursal : (int?)null;

            if (sucursales != null && sucursales.Count == 1)
                return sucursales[0].IdSucursal;

            return null;
        }

        private DataTable ObtenerCajasAbiertas(int? idSucursal, string buscar)
        {
            CierreCaja filtro = new CierreCaja();

            if (idSucursal.HasValue)
            {
                var sucursalActual = _oSucursalN.findById(idSucursal.Value);
                if (sucursalActual == null)
                    return new DataTable();

                filtro.Sucursal = sucursalActual;
            }

            return _oCierreN.findCierreCaja(filtro, CierreCaja.tipoBusqueda.FindOpen, buscar, null);
        }

        private DataTable ObtenerHistorialCierresCaja(int? idSucursal, string buscar, DateTime fechaDesde, List<Entidades.Sucursal> sucursales)
        {
            if (idSucursal.HasValue)
            {
                var sucursal = _oSucursalN.findById(idSucursal.Value);
                return sucursal == null ? new DataTable() : ObtenerHistorialCierresCajaSucursal(sucursal, buscar, fechaDesde);
            }

            var listaSucursales = sucursales ?? new List<Entidades.Sucursal>();
            DataTable? acumulado = null;

            foreach (var sucursal in listaSucursales)
            {
                if (sucursal == null || sucursal.IdSucursal <= 0)
                    continue;

                var dtSucursal = ObtenerHistorialCierresCajaSucursal(sucursal, buscar, fechaDesde);
                if (dtSucursal == null)
                    continue;

                if (acumulado == null)
                    acumulado = dtSucursal.Clone();

                foreach (DataRow row in dtSucursal.Rows)
                    acumulado.ImportRow(row);
            }

            if (acumulado == null)
                return new DataTable();

            var vista = acumulado.DefaultView;
            vista.Sort = "id DESC";
            return vista.ToTable();
        }

        private DataTable ObtenerHistorialCierresCajaSucursal(Entidades.Sucursal sucursal, string buscar, DateTime fechaDesde)
        {
            if (sucursal == null)
                return new DataTable();

            var filtro = new CierreCaja
            {
                Sucursal = sucursal
            };

            return _oCierreN.findCierreCaja(filtro, CierreCaja.tipoBusqueda.FindAll, buscar ?? "", fechaDesde);
        }

        private CierreCaja? ObtenerCajaAbiertaUsuario(Entidades.Usuario user)
        {
            if (user == null || user.IdSucursal == 0)
                return null;

            if (user.Sucursal == null)
                user.Sucursal = _oSucursalN.findById(user.IdSucursal);

            var cierre = new CierreCaja
            {
                Sucursal = user.Sucursal,
                UsuarioInicio = user
            };

            cierre = _oCierreN.findByIdOrLast(cierre, CierreCaja.tipoBusqueda.FindLast, "");

            bool abierta = cierre != null && cierre.UsuarioCierre != null && cierre.UsuarioCierre.Id == 0;
            return abierta ? cierre : null;
        }

        private DataTable FiltrarActividadesCaja(DataTable dt, string filtroActividad)
        {
            if (dt == null)
                return dt;

            string filtro = string.IsNullOrWhiteSpace(filtroActividad) ? "todos" : filtroActividad;
            if (string.Equals(filtro, "todos", StringComparison.OrdinalIgnoreCase))
                return dt;

            DataTable filtrado = dt.Clone();
            foreach (DataRow row in dt.Rows)
            {
                bool incluir = false;

                if (string.Equals(filtro, "gastos", StringComparison.OrdinalIgnoreCase))
                    incluir = EsGastoCaja(row) || (EsPagoCobro(row) && TieneMovimientoCaja(row));
                else if (string.Equals(filtro, "pagoElectronico", StringComparison.OrdinalIgnoreCase))
                    incluir = EsPagoElectronico(row);
                else if (string.Equals(filtro, "ctaCte", StringComparison.OrdinalIgnoreCase))
                    incluir = EsCtaCte(row) || EsPagoCobro(row);

                if (incluir)
                    filtrado.ImportRow(row);
            }

            return filtrado;
        }

        private bool EsPagoElectronico(DataRow row)
        {
            string tipo = ValorString(row, "TipoEgresoCaja");
            return string.Equals(tipo, "Pago Electronico", StringComparison.OrdinalIgnoreCase);
        }

        private bool EsCtaCte(DataRow row)
        {
            string tipo = ValorString(row, "TipoEgresoCaja");
            return string.Equals(tipo, "Cta Cte", StringComparison.OrdinalIgnoreCase);
        }

        private bool EsPagoCobro(DataRow row)
        {
            string tipo = ValorString(row, "TipoEgresoCaja");
            if (string.IsNullOrWhiteSpace(tipo))
                return false;

            string tipoNormalizado = tipo.Replace(" ", "");
            return tipoNormalizado.IndexOf("Pago", StringComparison.OrdinalIgnoreCase) >= 0 &&
                   tipoNormalizado.IndexOf("Cobro", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool EsGastoCaja(DataRow row)
        {
            if (row == null || row.Table == null)
                return false;

            if (EsPagoCobro(row))
                return false;

            return !EsPagoElectronico(row) && !EsCtaCte(row);
        }

        private bool TieneMovimientoCaja(DataRow row)
        {
            decimal monto;
            string valor = ValorString(row, "Monto");
            if (decimal.TryParse(valor, NumberStyles.Any, CultureInfo.CurrentCulture, out monto) ||
                decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out monto))
            {
                return monto != 0;
            }

            return false;
        }

        private decimal CalcularTotalGastosCaja(DataTable dt)
        {
            if (dt == null || !dt.Columns.Contains("Monto"))
                return 0;

            decimal total = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (!EsGastoCaja(row) && !(EsPagoCobro(row) && TieneMovimientoCaja(row)))
                    continue;

                decimal monto;
                if (decimal.TryParse(Convert.ToString(row["Monto"]), NumberStyles.Any, CultureInfo.CurrentCulture, out monto) ||
                    decimal.TryParse(Convert.ToString(row["Monto"]), NumberStyles.Any, CultureInfo.InvariantCulture, out monto))
                {
                    total += monto;
                }
            }

            return total;
        }

        private bool PuedeCambiarSucursalCaja(List<Entidades.Sucursal> sucursales = null)
        {
            int cantidadSucursales = sucursales != null ? sucursales.Count : _oSucursalN.findAll().Count;
            return cantidadSucursales > 1;
        }

        private void CargarPermisosEdicionEgresos(DataTable dt, bool desdePos, CierreCaja? cierreContexto = null)
        {
            var user = _usuarioActual;
            var idsModificables = new HashSet<int>();
            var pagosModificables = new Dictionary<int, int>();
            var comprasModificables = new Dictionary<int, int>();

            if (dt == null || !dt.Columns.Contains("id"))
            {
                ViewBag.IdsEgresosModificables = idsModificables;
                ViewBag.PagosModificables = pagosModificables;
                ViewBag.ComprasModificables = comprasModificables;
                return;
            }

            var ids = new List<int>();
            foreach (DataRow row in dt.Rows)
            {
                int id = ValorInt(row, "id");
                if (id > 0)
                    ids.Add(id);
            }

            var egresosPorId = _oCierreN.getEgresosCajaByIds(ids)
                .Where(x => x != null && x.Id > 0)
                .GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g => g.First());

            bool puedeEditarEnPos = false;
            if (desdePos)
            {
                var cierrePos = cierreContexto ?? ObtenerCajaAbiertaUsuario(user);
                puedeEditarEnPos = cierrePos != null &&
                                   cierrePos.Id > 0 &&
                                   cierrePos.Sucursal != null &&
                                   cierrePos.Sucursal.idSucursal > 0 &&
                                   CajaSigueAbierta(cierrePos) &&
                                   user.IdSucursal > 0 &&
                                   user.IdSucursal == cierrePos.Sucursal.idSucursal;
            }

            foreach (DataRow row in dt.Rows)
            {
                int id = ValorInt(row, "id");
                if (id <= 0)
                    continue;

                Entidades.EgresoCaja egreso;
                if (!egresosPorId.TryGetValue(id, out egreso))
                    continue;

                bool esPagoCobro = egreso != null &&
                                   egreso.Id > 0 &&
                                   string.Equals(egreso.Tabla, Entidades.EgresoCaja.tablas.Pagos.ToString(), StringComparison.OrdinalIgnoreCase) &&
                                   egreso.IdTabla.HasValue &&
                                   egreso.IdTabla.Value > 0;

                if (esPagoCobro)
                    pagosModificables[id] = egreso.IdTabla.Value;

                if (desdePos)
                {
                    if (!puedeEditarEnPos)
                        continue;

                    if (EgresosCajaPolicy.EsCompra(egreso))
                    {
                        int idCompraRelacionado = egreso.IdCompra.HasValue && egreso.IdCompra.Value > 0
                            ? egreso.IdCompra.Value
                            : (egreso.IdTabla.HasValue ? egreso.IdTabla.Value : 0);

                        if (idCompraRelacionado > 0 &&
                            (cierreContexto == null || FechaDentroDeCaja(egreso.Fecha, cierreContexto)) &&
                            egreso.Sucursal != null &&
                            egreso.Sucursal.idSucursal == user.IdSucursal)
                        {
                            comprasModificables[id] = idCompraRelacionado;
                        }

                        continue;
                    }

                    if (EgresosCajaPolicy.EsPagoElectronico(egreso) ||
                        EgresosCajaPolicy.EsCuentaCorriente(egreso))
                    {
                        continue;
                    }

                    if ((cierreContexto == null || FechaDentroDeCaja(egreso.Fecha, cierreContexto)) &&
                        egreso.Sucursal != null &&
                        egreso.Sucursal.idSucursal == user.IdSucursal)
                    {
                        idsModificables.Add(id);
                    }

                    continue;
                }

                var validacion = EgresosCajaPolicy.EvaluarModificacion(user, egreso, false, _empresa, _oCierreN.validarCajaAbiertaVendedor);
                if (validacion.PuedeModificar && (cierreContexto == null || FechaDentroDeCaja(egreso.Fecha, cierreContexto)))
                    idsModificables.Add(id);
            }

            ViewBag.IdsEgresosModificables = idsModificables;
            ViewBag.PagosModificables = pagosModificables;
            ViewBag.ComprasModificables = comprasModificables;
        }

        private bool CajaSigueAbierta(CierreCaja cierre)
        {
            return cierre != null &&
                   cierre.Id > 0 &&
                   (cierre.UsuarioCierre == null || cierre.UsuarioCierre.Id == 0);
        }

        private bool FechaDentroDeCaja(DateTime fecha, CierreCaja cierre)
        {
            if (cierre == null || cierre.FechaHoraInicio == null)
                return false;

            DateTime inicio = cierre.FechaHoraInicio.Value;
            DateTime fin = cierre.FechaHoraCierre ?? DateTime.Now;
            return fecha >= inicio && fecha <= fin;
        }

        private GuardarEgresoCajaResultado GuardarEgresoCajaCore(int id, DateTime fecha, int idTipoEgresoCaja, string descripcion, string monto, string detalle, int idSucursal, bool desdePos, int idCierre)
        {
            var user = _usuarioActual;

            if (idTipoEgresoCaja <= 0)
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "Seleccione un tipo de egreso." };

            if (string.IsNullOrWhiteSpace(descripcion))
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "Ingrese una descripción." };

            float importe = ParseFloat(monto);
            if (importe <= 0)
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "Ingrese un monto válido." };

            CierreCaja? cierreContexto = idCierre > 0
                ? _oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "")
                : null;

            if (idCierre > 0)
            {
                if (cierreContexto == null || cierreContexto.Id == 0)
                    return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "No se encontró la caja seleccionada." };

                if (!CajaSigueAbierta(cierreContexto))
                    return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "La caja seleccionada ya no se encuentra abierta." };
            }

            if (desdePos)
                idSucursal = user.IdSucursal;
            else if (cierreContexto != null && cierreContexto.Sucursal != null)
                idSucursal = cierreContexto.Sucursal.idSucursal;

            EgresoCaja egresoAnterior = id > 0 ? _oCierreN.getEgresoCajaById(id) : null;
            if (id > 0 && (egresoAnterior == null || egresoAnterior.Id == 0))
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "No se encontró el egreso de caja a modificar." };

            var sucursal = _oSucursalN.findById(idSucursal);
            if (sucursal == null)
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "Sucursal inválida." };

            if (desdePos)
            {
                bool cajaAbierta = _oCierreN.validarCajaAbiertaVendedor(fecha, sucursal, user);
                if (!cajaAbierta)
                    return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "La fecha y hora del egreso debe corresponder a una caja abierta del vendedor." };
            }
            else if (cierreContexto != null && !FechaDentroDeCaja(fecha, cierreContexto))
            {
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "La fecha y hora del egreso debe corresponder a la caja abierta seleccionada." };
            }

            if (egresoAnterior != null)
            {
                var validacion = EgresosCajaPolicy.EvaluarModificacion(user, egresoAnterior, desdePos, _empresa, _oCierreN.validarCajaAbiertaVendedor);
                if (!validacion.PuedeModificar)
                    return new GuardarEgresoCajaResultado { Ok = false, Mensaje = validacion.MensajeBloqueo };
            }

            var egreso = new EgresoCaja
            {
                Id = id,
                Fecha = fecha,
                IdTipoEgresoCaja = idTipoEgresoCaja,
                Descripcion = descripcion ?? "",
                Detalle = detalle ?? "",
                Monto = importe,
                Sucursal = sucursal,
                IdCompra = egresoAnterior != null ? egresoAnterior.IdCompra : null,
                Tabla = egresoAnterior != null ? egresoAnterior.Tabla : null,
                IdTabla = egresoAnterior != null ? egresoAnterior.IdTabla : null,
                CreadoPor = egresoAnterior != null
                    ? egresoAnterior.CreadoPor
                    : (cierreContexto != null && cierreContexto.UsuarioInicio != null ? cierreContexto.UsuarioInicio.Id : user.Id),
                ActualizadoPor = id > 0 ? user.Id : 0
            };

            egreso = _oCierreN.addOrEditEgresoCaja(egreso);

            return new GuardarEgresoCajaResultado
            {
                Ok = true,
                Id = egreso.Id,
                Mensaje = id > 0 ? "El egreso de caja se modificó correctamente." : "El egreso de caja se guardó correctamente."
            };
        }

        private void CargarViewBagsEgresos(int idSucursal, int idUsuario, int idTipoEgresoCaja, string descripcion, DateTime fechaDesde, DateTime fechaHasta, string filtroGasto, bool desdePos)
        {
            var user = _usuarioActual;
            ViewBag.Sucursales = _oSucursalN.findAll();
            ViewBag.Usuarios = ObtenerUsuariosFiltroEgresos();
            ViewBag.TiposEgresoCaja = _oCierreN.obtenerTiposEgresoCaja("", 0);
            ViewBag.IdSucursal = idSucursal;
            ViewBag.IdUsuario = idUsuario;
            ViewBag.IdTipoEgresoCaja = idTipoEgresoCaja;
            ViewBag.Descripcion = descripcion ?? "";
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.FiltroGasto = filtroGasto;
            ViewBag.DesdePOS = desdePos;
            ViewBag.UsuarioAdmin = user.Admin;
            ViewBag.PuedeVerTiposEgreso = true;
            ViewBag.PuedeEditarTiposEgreso = true;
        }

        private DataTable ObtenerUsuariosFiltroEgresos()
        {
            var dtUsuarios = _oUsuarioN.obtenerUsuarios(true);

            if (dtUsuarios != null && dtUsuarios.Columns.Contains("id") && dtUsuarios.Columns.Contains("nombre"))
            {
                DataRow drTodos = dtUsuarios.NewRow();
                drTodos["id"] = -1;
                drTodos["nombre"] = "Todos";
                dtUsuarios.Rows.Add(drTodos);
                dtUsuarios.DefaultView.Sort = "id";
            }

            return dtUsuarios;
        }

        // Tipos reservados que el sistema inserta solo (reflejo de una venta con forma de pago no
        // efectivo o de un movimiento de cuenta corriente) -- no son egresos de caja reales, nunca
        // se muestran en este listado.
        private static readonly string[] TiposExcluidosDeEgresosCaja = { "Cta Cte", "Pago Electronico" };

        private DataTable ExcluirTiposReservadosEgresosCaja(DataTable dt)
        {
            if (dt == null || !dt.Columns.Contains("TipoEgresoCaja"))
                return dt;

            DataTable resultado = dt.Clone();
            foreach (DataRow row in dt.Rows)
            {
                string tipo = Convert.ToString(row["TipoEgresoCaja"]);
                bool esReservadoExcluido = TiposExcluidosDeEgresosCaja.Any(t => string.Equals(t, tipo, StringComparison.OrdinalIgnoreCase));
                if (!esReservadoExcluido)
                    resultado.ImportRow(row);
            }

            return resultado;
        }

        private CalcularComisionesElectronicasVm CrearModelComisionesElectronicas(DateTime fechaDesde, DateTime fechaHasta, DateTime fechaEgreso, int idSucursal, bool desdePos, int idCierre)
        {
            var sucursal = idSucursal > 0 ? _oSucursalN.findById(idSucursal) : null;
            var porcentajesDefault = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                { "Debito", Convert.ToDecimal(_param.GetFloat(ParamKeys.ComisionDebito, 0f)) },
                { "Credito", Convert.ToDecimal(_param.GetFloat(ParamKeys.ComisionCredito, 0f)) },
                { "Qr", 0m },
                { "Transferencia", 0m }
            };

            var formas = ObtenerFormasPagoElectronicas(fechaDesde, fechaHasta, idSucursal, porcentajesDefault);
            return new CalcularComisionesElectronicasVm
            {
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                FechaEgreso = fechaEgreso,
                IdSucursal = idSucursal,
                SucursalNombre = sucursal != null ? sucursal.SucursalNombre : "Todas",
                IdTipoEgresoCaja = 0,
                DesdePos = desdePos,
                IdCierre = idCierre,
                FormasPago = formas,
                TotalEgreso = formas.Sum(f => f.ImporteComision)
            };
        }

        private List<ComisionElectronicaFormaVm> ObtenerFormasPagoElectronicas(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal, IDictionary<string, decimal> porcentajes)
        {
            var items = new List<ComisionElectronicaFormaVm>
            {
                new ComisionElectronicaFormaVm { Codigo = "Debito", Nombre = "Débito", Porcentaje = ObtenerPorcentaje(porcentajes, "Debito") },
                new ComisionElectronicaFormaVm { Codigo = "Credito", Nombre = "Crédito", Porcentaje = ObtenerPorcentaje(porcentajes, "Credito") },
                new ComisionElectronicaFormaVm { Codigo = "Qr", Nombre = "QR", Porcentaje = ObtenerPorcentaje(porcentajes, "Qr") },
                new ComisionElectronicaFormaVm { Codigo = "Transferencia", Nombre = "Transferencia", Porcentaje = ObtenerPorcentaje(porcentajes, "Transferencia") }
            };

            var ventas = _oVentaN.getAllVentas(fechaDesde.Date, fechaHasta.Date, "", -1, -1, idSucursal, false, false) ?? new List<Entidades.Venta>();
            foreach (var venta in ventas)
            {
                if (venta == null || string.Equals(venta.Estado ?? "", "ANULADO", StringComparison.OrdinalIgnoreCase))
                    continue;

                var item = items.FirstOrDefault(i => string.Equals(i.Codigo, venta.FormaPago ?? "", StringComparison.OrdinalIgnoreCase));
                if (item == null)
                    continue;

                decimal totalVenta = Convert.ToDecimal(venta.TotalImporte);
                decimal pagoMixtoEfectivo = Convert.ToDecimal(venta.PagoMixtoEfectivo);
                decimal totalElectronico = pagoMixtoEfectivo > 0m ? totalVenta - pagoMixtoEfectivo : totalVenta;
                if (totalElectronico <= 0m)
                    continue;

                item.TotalCobrado += totalElectronico;
            }

            foreach (var item in items)
            {
                item.ImporteComision = decimal.Round(item.TotalCobrado * item.Porcentaje / 100m, 2, MidpointRounding.AwayFromZero);
            }

            return items;
        }

        private decimal ObtenerPorcentaje(IDictionary<string, decimal> porcentajes, string codigo)
        {
            if (porcentajes == null || string.IsNullOrWhiteSpace(codigo))
                return 0m;

            decimal valor;
            return porcentajes.TryGetValue(codigo, out valor) ? valor : 0m;
        }

        private string FormatearImporte(decimal valor)
        {
            return valor.ToString("N2", new CultureInfo("es-AR"));
        }

        private string FormatearPorcentaje(decimal valor)
        {
            return valor.ToString("0.##", new CultureInfo("es-AR"));
        }

        private string ObtenerDescripcionSucursal(int idSucursal)
        {
            if (idSucursal <= 0)
                return "Todas";

            var sucursal = _oSucursalN.findById(idSucursal);
            return sucursal != null && !string.IsNullOrWhiteSpace(sucursal.SucursalNombre)
                ? sucursal.SucursalNombre
                : "Todas";
        }

        private decimal ParseDecimalFlexible(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0m;

            string limpio = value.Trim()
                .Replace("$", "")
                .Replace(" ", "");

            decimal numero;
            if (decimal.TryParse(limpio, NumberStyles.Any, new CultureInfo("es-AR"), out numero))
                return numero;

            if (decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out numero))
                return numero;

            int ultimaComa = limpio.LastIndexOf(',');
            int ultimoPunto = limpio.LastIndexOf('.');
            if (ultimaComa >= 0 && ultimoPunto >= 0)
            {
                char separadorDecimal = ultimaComa > ultimoPunto ? ',' : '.';
                limpio = separadorDecimal == ','
                    ? limpio.Replace(".", "").Replace(',', '.')
                    : limpio.Replace(",", "");
            }
            else
            {
                limpio = limpio.Replace(',', '.');
            }

            return decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out numero)
                ? numero
                : 0m;
        }

        private int ValorInt(DataRow row, string columna)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return 0;

            int valor;
            return int.TryParse(Convert.ToString(row[columna]), out valor) ? valor : 0;
        }

        private string ValorString(DataRow row, string columna)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return "";

            return Convert.ToString(row[columna]) ?? "";
        }
    }

    // Port de Web/Helpers/EgresosCajaPolicy.cs. La unica diferencia real respecto al original: la
    // rama "no desde POS" llamaba a PermisosHelper.TienePermiso(usuario, empresa,
    // EgresosCaja.AltaEdicion, fecha, creadoPor) (chequeo de permiso vs Session) -- se reemplaza
    // por el mismo bypass de Admin ya usado en toda esta migracion (usuario.Admin), ya que
    // PermisosHelper.TienePermiso hace exactamente eso mismo internamente para un usuario Admin.
    // El resto de las reglas de negocio reales (que registro es una compra/pago electronico/
    // cuenta corriente, que sucursal/caja abierta aplica desde POS) se portan tal cual.
    public sealed class EgresoCajaPermisoResultado
    {
        public bool PuedeModificar { get; set; }
        public string MensajeBloqueo { get; set; } = "";
    }

    public static class EgresosCajaPolicy
    {
        public static EgresoCajaPermisoResultado EvaluarModificacion(
            Entidades.Usuario usuario,
            EgresoCaja egreso,
            bool desdePos,
            IEmpresaContext empresa,
            Func<DateTime, Sucursal, Entidades.Usuario, bool> validarCajaAbierta)
        {
            if (usuario == null)
                return Bloqueado("Sesion invalida.");

            if (egreso == null || egreso.Id == 0)
                return Bloqueado("No se encontro el egreso de caja.");

            if (EsCompra(egreso))
                return Bloqueado("Este registro corresponde a una compra y debe modificarse desde Compras.");

            if (EsPagoElectronico(egreso))
                return Bloqueado("Los pagos electronicos se modifican desde Ventas.");

            if (EsCuentaCorriente(egreso))
                return Bloqueado("Los movimientos de cuenta corriente se modifican desde su modulo original.");

            if (!desdePos)
            {
                return usuario.Admin
                    ? Permitido()
                    : Bloqueado("No tiene permisos para modificar este egreso de caja.");
            }

            if (egreso.Sucursal == null || egreso.Sucursal.idSucursal <= 0)
                return Bloqueado("No se pudo determinar la sucursal del egreso.");

            if (usuario.IdSucursal <= 0 || usuario.IdSucursal != egreso.Sucursal.idSucursal)
                return Bloqueado("Solo puede modificar egresos de la sucursal activa en la sesion.");

            if (validarCajaAbierta == null || !validarCajaAbierta(egreso.Fecha, egreso.Sucursal, usuario))
                return Bloqueado("La fecha y hora del egreso debe corresponder a una caja abierta del vendedor.");

            return Permitido();
        }

        public static bool EsPagoElectronico(EgresoCaja egreso)
        {
            if (egreso == null)
                return false;

            return egreso.IdTipoEgresoCaja == EgresoCaja.idPagoTarjeta ||
                   string.Equals(egreso.Tabla, EgresoCaja.tablas.Ventas.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsCuentaCorriente(EgresoCaja egreso)
        {
            if (egreso == null)
                return false;

            return egreso.esEgresoCtaCte(egreso.IdTipoEgresoCaja) ||
                   string.Equals(egreso.Tabla, EgresoCaja.tablas.Pagos.ToString(), StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(egreso.Tabla, EgresoCaja.tablas.MovCtaCte.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsCompra(EgresoCaja egreso)
        {
            if (egreso == null)
                return false;

            return (egreso.IdCompra.HasValue && egreso.IdCompra.Value > 0) ||
                   string.Equals(egreso.Tabla, EgresoCaja.tablas.Compras.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static EgresoCajaPermisoResultado Permitido()
        {
            return new EgresoCajaPermisoResultado
            {
                PuedeModificar = true,
                MensajeBloqueo = string.Empty
            };
        }

        private static EgresoCajaPermisoResultado Bloqueado(string mensaje)
        {
            return new EgresoCajaPermisoResultado
            {
                PuedeModificar = false,
                MensajeBloqueo = mensaje ?? "No se puede modificar este egreso."
            };
        }
    }
}
