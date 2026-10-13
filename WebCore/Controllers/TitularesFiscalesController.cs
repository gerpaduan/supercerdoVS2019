// Configuracion > Titulares de facturacion: alta y edicion de los titulares fiscales de la empresa (personas con su CUIT,
// condicion IVA, inicio de actividades, IIBB, topes informativos y puntos de venta de ARCA), habilitar / deshabilitar,
// y ver lo facturado frente a los topes por mes y por anio. Titulares fiscales por empresa y por sucursal (2026-10-09,
// ver docs/DECISIONS.md y docs/06-datos-e-integraciones/afip-y-facturacion.md).
// Permisos (2026-10-10): el alta y la edicion de titulares son SOLO del super-admin de la plataforma (sobre la empresa que elige con ?idEmpresa=);
// el administrador de la empresa solo ve el listado y habilita / deshabilita titulares. Solo Postgres (NegocioFactory.CrearTitularFiscal devuelve null en SQL Server).
// La empresa SIEMPRE sale de la sesion; el CUIT de un titular sale de la persona elegida y despues no se puede cambiar
// (es la carpeta de su certificado). El certificado se carga en Certificado ARCA; el titular de cada sucursal, en Mis Sucursales.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using AFIP;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;
using WebCore.Services;

namespace WebCore.Controllers
{
    public class TitularesFiscalesController : Controller
    {
        // "Traer de ARCA" pega contra el WSFE del titular: tope por empresa y hora para no abusar del servicio.
        private static readonly AfipLimitador LimitadorPuntosVenta = new AfipLimitador(10, TimeSpan.FromHours(1));

        private readonly IUsuarioSesionService _sesion;
        private readonly IWebHostEnvironment _env;
        private readonly IAfipConfigProvider _afip;
        private readonly ICertificadoArcaEstado _estado;
        private readonly ILogger<TitularesFiscalesController> _log;

        // Se arman en OnActionExecuting, cuando ya se sabe sobre que empresa se trabaja (la de la sesion o, para el
        // superadmin, la que eligio con ?idEmpresa=).
        private IEmpresaContext _empresa = null!;
        private IParametrosContext _param = null!;
        private Negocio.TitularFiscal? _titulares;
        private Negocio.Persona _personas = null!;
        private Negocio.Sucursal _sucursales = null!;
        private Entidades.Empresa? _empresaObjetivo;
        private int _idEmpresaObjetivo;

        public TitularesFiscalesController(IUsuarioSesionService sesion, IWebHostEnvironment env, IAfipConfigProvider afip,
            ICertificadoArcaEstado estado, ILogger<TitularesFiscalesController> log)
        {
            _sesion = sesion;
            _env = env;
            _afip = afip;
            _estado = estado;
            _log = log;
        }

        // Administrador de la empresa de la sesion: ve el listado y habilita / deshabilita titulares.
        private bool EsAdmin => _sesion.UsuarioActual != null && _sesion.UsuarioActual.Admin;

        // Super-admin de la plataforma (columna usuarios.superadmin): el unico que da de alta y edita titulares, en cualquier empresa.
        private bool? _esSuperAdmin;
        private bool EsSuperAdmin => _esSuperAdmin ??= _sesion.UsuarioActual != null
            && Infrastructure.NegocioFactory.CrearSystemAdministrationRepository().EsSuperAdmin(_sesion.UsuarioActual.Id);

        // El superadmin trabaja sobre otra empresa que la de su sesion: certificados y claves (que se leen con la sesion) no estan disponibles.
        private bool EsOtraEmpresa => _idEmpresaObjetivo != _sesion.Empresa.IdEmpresa;

        // idEmpresa para los links y redirects: null cuando es la propia (la URL queda como siempre).
        private int? RutaEmpresa => EsOtraEmpresa ? _idEmpresaObjetivo : (int?)null;

        // Define la empresa objetivo y arma los repositorios sobre ella. Pedir otra empresa que la propia exige ser super-admin.
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext contexto)
        {
            _idEmpresaObjetivo = _sesion.Empresa.IdEmpresa;
            string? pedida = Request.Query["idEmpresa"];
            if (string.IsNullOrEmpty(pedida) && Request.HasFormContentType) pedida = Request.Form["idEmpresa"];
            if (int.TryParse(pedida, NumberStyles.None, CultureInfo.InvariantCulture, out int idPedida) && idPedida > 0 && idPedida != _idEmpresaObjetivo)
            {
                if (!EsSuperAdmin)
                {
                    contexto.Result = AccesoDenegado();
                    return;
                }
                _idEmpresaObjetivo = idPedida;
            }

            _empresa = EsOtraEmpresa ? new EmpresaObjetivoContext(_idEmpresaObjetivo) : _sesion.Empresa;
            _param = Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();
            _titulares = Infrastructure.NegocioFactory.CrearTitularFiscal(_empresa);
            _personas = Infrastructure.NegocioFactory.CrearPersona(_empresa, _param);
            _sucursales = Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
            base.OnActionExecuting(contexto);
        }

        // Empresa sobre la que se trabaja: la de la sesion o la elegida por el superadmin (null si no existe).
        private Entidades.Empresa? EmpresaObjetivo()
        {
            if (!EsOtraEmpresa) return _estado.EmpresaActual();
            return _empresaObjetivo ??= _sucursales.findEmpresaById(_idEmpresaObjetivo);
        }

        // Contexto de empresa fijo para el superadmin que trabaja sobre otra empresa (RLS filtra por este id).
        private sealed class EmpresaObjetivoContext : IEmpresaContext
        {
            public EmpresaObjetivoContext(int idEmpresa) { IdEmpresa = idEmpresa; }
            public int IdEmpresa { get; }
        }

        // Redirect que conserva la empresa elegida por el superadmin.
        private IActionResult Volver(string accion, int? id = null)
        {
            var ruta = new Microsoft.AspNetCore.Routing.RouteValueDictionary();
            if (id.HasValue) ruta["id"] = id.Value;
            if (RutaEmpresa.HasValue) ruta["idEmpresa"] = RutaEmpresa.Value;
            return RedirectToAction(accion, ruta);
        }

        // ------------------------------------------------------------------ listado

        // Titulares con lo facturado del mes y del periodo anual de cada uno, en el mes/anio elegido (por defecto el actual).
        [HttpGet]
        public IActionResult Index(int? mes = null, int? anio = null)
        {
            if (!(EsAdmin || EsSuperAdmin)) return AccesoDenegado();

            DateTime hoy = DateTime.Today;
            int mesElegido = mes.HasValue && mes.Value >= 1 && mes.Value <= 12 ? mes.Value : hoy.Month;
            int anioElegido = anio.HasValue && anio.Value >= 2000 && anio.Value <= hoy.Year + 1 ? anio.Value : hoy.Year;

            var vm = new TitularesIndexVm
            {
                Soportado = _titulares != null, Mes = mesElegido, Anio = anioElegido,
                PuedeGestionar = EsSuperAdmin, IdEmpresaRuta = RutaEmpresa, CertificadoDisponible = !EsOtraEmpresa
            };
            vm.Anios = Enumerable.Range(hoy.Year - 5, 7).Reverse().ToList();
            if (!vm.Anios.Contains(anioElegido)) vm.Anios.Add(anioElegido);

            var empresa = EmpresaObjetivo();
            if (_titulares != null && empresa != null)
            {
                bool homologacion = AfipEntorno.EsHomologacion(empresa.Entorno_HOMO_PROD);
                vm.EntornoEnUso = homologacion ? "Pruebas (homologación)" : "Producción";
                vm.EmpresaNombre = empresa.RazonSocialAfip ?? empresa.NombreFantasia ?? "";

                foreach (var consumo in _titulares.ConsumoDeTitulares(empresa.IdEmpresa, anioElegido, mesElegido))
                {
                    var info = LeerCertificado(empresa, consumo.Titular, homologacion);
                    // Titular compartido: otras empresas con el mismo CUIT (los nombres solo los ve el super-admin).
                    var otras = consumo.Titular.Cuit > 0 && !consumo.Titular.Revocado
                        ? _titulares.OtrasEmpresasConCuit(consumo.Titular.Cuit, empresa.IdEmpresa)
                        : new List<Entidades.EmpresaConCuit>();
                    vm.Filas.Add(new TitularFilaVm
                    {
                        Consumo = consumo,
                        EstadoCertificado = info.Estado,
                        VenceCertificado = info.Vence,
                        DiasRestantes = info.DiasRestantes,
                        OtrasEmpresas = otras.Count,
                        NombresOtrasEmpresas = EsSuperAdmin ? string.Join(", ", otras.Select(o => o.Nombre)) : ""
                    });
                }

                vm.Sucursales = ArmarSucursales(empresa);
            }

            ViewBag.Title = "Titulares de facturación";
            ViewBag.Seccion = "Titulares de facturación";
            return View(vm);
        }

        // ------------------------------------------------------------------ alta y edicion

        [HttpGet]
        public IActionResult Editar(int id = 0)
        {
            if (!EsSuperAdmin) return AccesoDenegado();
            if (_titulares == null) return Volver(nameof(Index));

            var empresa = EmpresaObjetivo();
            if (empresa == null) return Volver(nameof(Index));

            var vm = new TitularEditVm
            {
                EntornoEnUso = AfipEntorno.EsHomologacion(empresa.Entorno_HOMO_PROD) ? "Pruebas (homologación)" : "Producción",
                IdEmpresaRuta = RutaEmpresa,
                EmpresaNombre = empresa.RazonSocialAfip ?? empresa.NombreFantasia ?? "",
                CertificadoDisponible = !EsOtraEmpresa
            };

            if (id > 0)
            {
                var titular = _titulares.Obtener(id);
                if (titular == null || titular.IdEmpresa != empresa.IdEmpresa)
                {
                    TempData["TitularError"] = "El titular no existe.";
                    return Volver(nameof(Index));
                }
                CargarVm(vm, titular, empresa);
            }

            ViewBag.Title = vm.EsAlta ? "Nuevo titular de facturación" : "Titular de facturación";
            ViewBag.Seccion = "Titulares de facturación";
            return View(vm);
        }

        // Alta (idTitular = 0, con idPersona) o edicion. Los datos de la persona (razon social, CUIT, condicion IVA) se
        // copian al titular solo en el alta; despues se refrescan con "Actualizar desde la persona".
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(int idTitular, int idPersona, string? inicioActividad, string? iibb,
            string? limiteMensual, string? limiteAnual, string? ventanaAnual, bool confirmaCompartido = false)
        {
            if (!EsSuperAdmin) return AccesoDenegado();
            if (_titulares == null) return Volver(nameof(Index));

            var empresa = EmpresaObjetivo();
            if (empresa == null) return Volver(nameof(Index));

            string? error = null;
            bool mensualOk = TryParseMonto(limiteMensual, out decimal? mensual);
            bool anualOk = TryParseMonto(limiteAnual, out decimal? anual);
            if (!mensualOk) error = "El tope mensual no es un número válido.";
            else if (!anualOk) error = "El tope anual no es un número válido.";
            else
            {
                var titular = ArmarTitular(empresa, idTitular, idPersona, inicioActividad, iibb, mensual, anual, ventanaAnual, out error);
                if (titular != null && error == null)
                {
                    try
                    {
                        // Titular compartido (2026-10-12): si el CUIT ya es titular en otras empresas, el alta exige confirmar de forma
                        // explicita que esta empresa usara el mismo certificado (se valida aca, no solo en el navegador).
                        if (idTitular <= 0)
                            error = _titulares.ErrorSiFaltaConfirmarCompartido(titular.Cuit, empresa.IdEmpresa, confirmaCompartido);

                        int idGuardado = 0;
                        if (error == null)
                            error = _titulares.Guardar(titular, out idGuardado);

                        if (error == null)
                        {
                            int otras = _titulares.OtrasEmpresasConCuit(titular.Cuit, empresa.IdEmpresa).Count;
                            _log.LogInformation("Titulares fiscales: usuario {IdUsuario} guardo el titular {IdTitular} de la empresa {IdEmpresa} (CUIT compartido con {Otras} empresa(s) mas).",
                                _sesion.UsuarioActual.Id, idGuardado, empresa.IdEmpresa, otras);
                            TempData["TitularMensaje"] = idTitular > 0
                                ? "Titular guardado."
                                : "Titular creado." + (otras > 0
                                    ? " Usa el mismo certificado de ARCA del CUIT (lo administra otra empresa): no hace falta cargarlo ni pedirlo de nuevo. Habilitá sus puntos de venta para poder asignarlo a una sucursal."
                                    : " Cargá su certificado de ARCA en Certificado ARCA y habilitá sus puntos de venta para poder asignarlo a una sucursal.");
                            return Volver(nameof(Editar), idGuardado);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "Titulares fiscales: error guardando un titular de la empresa {IdEmpresa}.", empresa.IdEmpresa);
                        error = "No se pudo guardar el titular. Revisá el log del servidor.";
                    }
                }
            }

            TempData["TitularError"] = error ?? "No se pudo guardar el titular.";
            return idTitular > 0
                ? Volver(nameof(Editar), idTitular)
                : Volver(nameof(Editar));
        }

        // Arma el titular a guardar. Alta: datos copiados de la persona. Edicion: lo ya guardado + los campos editables.
        private Entidades.TitularFiscal? ArmarTitular(Entidades.Empresa empresa, int idTitular, int idPersona, string? inicioActividad,
            string? iibb, decimal? mensual, decimal? anual, string? ventanaAnual, out string? error)
        {
            error = null;
            string ventana = ventanaAnual == Entidades.TitularFiscal.VentanaCalendario
                ? Entidades.TitularFiscal.VentanaCalendario
                : Entidades.TitularFiscal.VentanaMovil12Meses;

            Entidades.TitularFiscal titular;
            if (idTitular > 0)
            {
                var existente = _titulares!.Obtener(idTitular);
                if (existente == null || existente.IdEmpresa != empresa.IdEmpresa)
                {
                    error = "El titular no existe.";
                    return null;
                }
                titular = existente;
            }
            else
            {
                var persona = PersonaDeLaEmpresa(idPersona, empresa);
                if (persona == null) { error = "Elegí una persona de la lista."; return null; }

                long cuit = CuitDePersona(persona);
                if (cuit <= 0) { error = "La persona no tiene un CUIT válido cargado (11 dígitos con dígito verificador). Corregilo en Personas."; return null; }

                if (_titulares!.Listar(empresa.IdEmpresa, false).Any(t => t.IdPersona == persona.IdPersona))
                { error = "Esa persona ya es titular de la empresa."; return null; }

                titular = new Entidades.TitularFiscal
                {
                    IdEmpresa = empresa.IdEmpresa,
                    IdPersona = persona.IdPersona,
                    Habilitado = true,
                    Cuit = cuit,
                    RazonSocial = persona.razonSocial,
                    IdIva = persona.IdIva
                };
            }

            titular.LimiteMensual = mensual;
            titular.LimiteAnual = anual;
            titular.VentanaAnual = ventana;

            // El principal solo guarda topes y ventana: sus datos fiscales son los de la empresa.
            if (titular.EsPredeterminado) return titular;

            if (!string.IsNullOrWhiteSpace(inicioActividad))
            {
                if (!DateTime.TryParseExact(inicioActividad.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                { error = "La fecha de inicio de actividades no es válida."; return null; }
                titular.InicioActividad = fecha;
            }
            else titular.InicioActividad = null;

            string soloDigitos = Entidades.CuitHelper.Normalizar(iibb);
            titular.Iibb = long.TryParse(soloDigitos, NumberStyles.None, CultureInfo.InvariantCulture, out long numeroIibb) ? numeroIibb : 0;
            return titular;
        }

        // Vuelve a leer razon social y condicion IVA de la persona (la persona pudo cambiar despues del alta).
        // El CUIT no se toca: si ya no coincide, se rechaza.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarDesdePersona(int idTitular)
        {
            if (!EsSuperAdmin) return AccesoDenegado();
            if (_titulares == null) return Volver(nameof(Index));
            var empresa = EmpresaObjetivo();
            if (empresa == null) return Volver(nameof(Index));

            var titular = _titulares.Obtener(idTitular);
            string? error = null;
            if (titular == null || titular.IdEmpresa != empresa.IdEmpresa) error = "El titular no existe.";
            else if (titular.EsPredeterminado || !titular.IdPersona.HasValue) error = "El titular principal toma sus datos de la empresa (Administración del sistema).";
            else
            {
                var persona = PersonaDeLaEmpresa(titular.IdPersona.Value, empresa);
                if (persona == null) error = "La persona del titular ya no existe.";
                else if (CuitDePersona(persona) != titular.Cuit)
                    error = "El CUIT de la persona ya no coincide con el del titular. El CUIT de un titular no se puede cambiar: dalo de baja y creá otro.";
                else
                {
                    titular.RazonSocial = persona.razonSocial;
                    titular.IdIva = persona.IdIva;
                    try { error = _titulares.Guardar(titular, out _); }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "Titulares fiscales: error actualizando el titular {IdTitular} desde la persona.", idTitular);
                        error = "No se pudo actualizar el titular. Revisá el log del servidor.";
                    }
                }
            }

            if (error == null) TempData["TitularMensaje"] = "Datos actualizados desde la persona.";
            else TempData["TitularError"] = error;
            return Volver(nameof(Editar), idTitular);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetHabilitado(int idTitular, bool habilitado)
        {
            if (!(EsAdmin || EsSuperAdmin)) return AccesoDenegado();
            if (_titulares == null) return Volver(nameof(Index));
            var empresa = EmpresaObjetivo();
            if (empresa == null) return Volver(nameof(Index));

            // El principal es el respaldo de toda sucursal sin asignacion: no se deshabilita.
            var titular = _titulares.Obtener(idTitular);
            string? error = titular != null && titular.IdEmpresa == empresa.IdEmpresa && titular.EsPredeterminado && !habilitado
                ? "El titular principal (la empresa) no se puede deshabilitar."
                : _titulares.SetHabilitado(empresa.IdEmpresa, idTitular, habilitado);
            if (error == null) TempData["TitularMensaje"] = habilitado ? "Titular habilitado." : "Titular deshabilitado.";
            else TempData["TitularError"] = error;
            return Volver(nameof(Index));
        }

        // ------------------------------------------------------------------ titular compartido entre empresas (2026-10-12)

        // Cancela la titularidad (solo super-admin). Si el titular nunca facturo en la empresa se borra (la empresa no ve
        // nada: puede ser una asignacion equivocada); si ya facturo queda "cancelada" de solo lectura. En ambos casos sus
        // sucursales vuelven al principal y la empresa no puede emitir factura ni nota de credito con ese certificado.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Revocar(int idTitular)
        {
            if (!EsSuperAdmin) return AccesoDenegado();
            if (_titulares == null) return Volver(nameof(Index));
            var empresa = EmpresaObjetivo();
            if (empresa == null) return Volver(nameof(Index));

            try
            {
                var resultado = _titulares.Revocar(empresa.IdEmpresa, idTitular, _sesion.UsuarioActual.Id);
                if (resultado.Ok)
                {
                    // Solo ids: sin datos del titular ni de la persona.
                    _log.LogWarning("Titulares fiscales: el superadmin {IdUsuario} cancelo la titularidad {IdTitular} de la empresa {IdEmpresa} ({Resultado}).",
                        _sesion.UsuarioActual.Id, idTitular, empresa.IdEmpresa, resultado.Borrado ? "eliminado, nunca habia facturado" : "marcado cancelado, ya habia facturado");
                    TempData["TitularMensaje"] = resultado.Mensaje;
                }
                else TempData["TitularError"] = resultado.Mensaje;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Titulares fiscales: error cancelando la titularidad {IdTitular} de la empresa {IdEmpresa}.", idTitular, empresa.IdEmpresa);
                TempData["TitularError"] = "No se pudo cancelar la titularidad. Revisá el log del servidor.";
            }
            return Volver(nameof(Index));
        }

        // Repone una titularidad cancelada que ya tenia facturas (queda deshabilitada: la empresa decide cuando habilitarla).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reponer(int idTitular)
        {
            if (!EsSuperAdmin) return AccesoDenegado();
            if (_titulares == null) return Volver(nameof(Index));
            var empresa = EmpresaObjetivo();
            if (empresa == null) return Volver(nameof(Index));

            string? error = _titulares.Reponer(empresa.IdEmpresa, idTitular);
            if (error == null)
            {
                _log.LogWarning("Titulares fiscales: el superadmin {IdUsuario} repuso la titularidad {IdTitular} de la empresa {IdEmpresa}.",
                    _sesion.UsuarioActual.Id, idTitular, empresa.IdEmpresa);
                TempData["TitularMensaje"] = "Titularidad repuesta. Queda deshabilitada: la empresa la habilita cuando quiera usarla.";
            }
            else TempData["TitularError"] = error;
            return Volver(nameof(Index));
        }

        // Empresas (ademas de la elegida) que ya tienen ese CUIT como titular: el alta lo muestra para pedir la confirmacion.
        // Solo super-admin (devuelve nombres de otras empresas).
        [HttpGet]
        public IActionResult CuitCompartido(string? cuit)
        {
            if (!EsSuperAdmin) return StatusCode(403, new { ok = false, msg = "Solo el administrador del sistema." });
            var empresa = EmpresaObjetivo();
            if (_titulares == null || empresa == null) return Json(new { empresas = new string[0] });
            long.TryParse(Entidades.CuitHelper.Normalizar(cuit), NumberStyles.None, CultureInfo.InvariantCulture, out long cuitNumerico);
            if (cuitNumerico <= 0) return Json(new { empresas = new string[0] });
            var otras = _titulares.OtrasEmpresasConCuit(cuitNumerico, empresa.IdEmpresa);
            return Json(new { empresas = otras.Select(o => o.Nombre).ToArray() });
        }

        // ------------------------------------------------------------------ personas

        // Personas que se pueden elegir como titular: de la empresa (o compartidas), con CUIT valido y que no sean ya titulares.
        [HttpGet]
        public IActionResult BuscarPersonas(string? filtro)
        {
            if (!EsSuperAdmin) return StatusCode(403, new { ok = false, msg = "Solo el administrador del sistema." });
            var empresa = EmpresaObjetivo();
            if (_titulares == null || empresa == null) return Json(new object[0]);

            var yaTitulares = new HashSet<int>(_titulares.Listar(empresa.IdEmpresa, false)
                .Where(t => t.IdPersona.HasValue).Select(t => t.IdPersona!.Value));

            DataTable tabla = _personas.buscarPersona(filtro ?? "", false) ?? new DataTable();
            var personas = tabla.AsEnumerable()
                .Where(fila => Convert.ToInt32(fila["idEmpresa"]) == empresa.IdEmpresa || Convert.ToInt32(fila["idEmpresa"]) == 0)
                .Select(fila => new
                {
                    idPersona = Convert.ToInt32(fila["idPersona"]),
                    razonSocial = fila["razonSocial"] == DBNull.Value ? "" : fila["razonSocial"].ToString() ?? "",
                    cuit = fila["cuit"] == DBNull.Value ? "" : fila["cuit"].ToString() ?? ""
                })
                .Where(p => !yaTitulares.Contains(p.idPersona) && Entidades.CuitHelper.EsValido(p.cuit))
                .OrderBy(p => p.razonSocial)
                .Take(60)
                .ToList();

            // La condicion IVA no viene en la busqueda: se lee de cada persona y solo sirven RI y monotributo.
            var titulares = personas
                .Select(p => new { p.idPersona, p.razonSocial, p.cuit, idIva = _personas.findById(p.idPersona)?.IdIva ?? 0 })
                .Where(p => p.idIva == Entidades.Persona.codIvaRRII_Afip || p.idIva == Entidades.Persona.codIvaMonotributo_Afip)
                .Take(30)
                .ToList();
            return Json(titulares);
        }

        // ------------------------------------------------------------------ puntos de venta

        // Trae de ARCA los puntos de venta habilitados del titular y los suma a su lista (los manuales no se tocan).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TraerPuntosVentaArca(int idTitular)
        {
            if (!EsSuperAdmin) return StatusCode(403, new { ok = false, mensaje = "Solo el administrador del sistema." });
            // La clave del certificado del titular se lee con la sesion de su empresa: el super-admin no puede usarla desde otra.
            if (EsOtraEmpresa) return Json(new { ok = false, mensaje = "Traer de ARCA usa el certificado del titular y solo anda en la empresa de tu sesion. Cargá los puntos de venta a mano." });
            var empresa = EmpresaObjetivo();
            var titular = TitularNoPrincipal(empresa, idTitular, out string? motivo);
            if (titular == null) return Json(new { ok = false, mensaje = motivo });

            if (!LimitadorPuntosVenta.IntentarUsar(empresa!.IdEmpresa.ToString(CultureInfo.InvariantCulture)))
                return Json(new { ok = false, mensaje = "Se hicieron demasiadas consultas a ARCA en la última hora. Esperá un rato y reintentá." });

            try
            {
                bool homologacion = AfipEntorno.EsHomologacion(empresa.Entorno_HOMO_PROD);
                var emisor = new Entidades.EmisorFiscal { IdTitular = titular.IdTitular, Cuit = titular.Cuit, RazonSocial = titular.RazonSocial, EsPredeterminado = false };
                var config = _afip.Crear(emisor, homologacion);
                var resultado = ConsultarPuntosVentaService.Consultar(titular.Cuit, homologacion, _env.ContentRootPath, config, titular.NombreCertificadoPfx);
                if (!resultado.Ok) return Json(new { ok = false, mensaje = resultado.Mensaje });

                var existentes = _titulares!.ListarPuntosVenta(titular.IdTitular).ToDictionary(p => p.CodPuntoVentaAfip);
                int nuevos = 0;
                foreach (var pv in resultado.PuntosVenta)
                {
                    if (existentes.ContainsKey(pv.Codigo)) continue;
                    string? error = _titulares.GuardarPuntoVenta(empresa.IdEmpresa, new Entidades.TitularPuntoVenta
                    {
                        IdTitular = titular.IdTitular,
                        CodPuntoVentaAfip = pv.Codigo,
                        Descripcion = string.IsNullOrWhiteSpace(pv.TipoEmision) ? "" : pv.TipoEmision,
                        Habilitado = true,
                        Origen = "ARCA"
                    });
                    if (error != null) return Json(new { ok = false, mensaje = error });
                    nuevos++;
                }
                return Json(new { ok = true, mensaje = resultado.Mensaje + (nuevos > 0 ? " Se agregaron " + nuevos + "." : " No había puntos de venta nuevos.") });
            }
            catch (InvalidOperationException ex)
            {
                // Clave guardada que no se puede descifrar (AfipConfigProvider): mensaje ya mostrable.
                return Json(new { ok = false, mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Titulares fiscales: error trayendo los puntos de venta del titular {IdTitular}.", idTitular);
                return Json(new { ok = false, mensaje = "No se pudo consultar a ARCA. Revisá el log del servidor." });
            }
        }

        // Alta manual o cambio (descripcion / habilitado) de un punto de venta del titular.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarPuntoVenta(int idTitular, int codigo, string? descripcion, bool habilitado = true)
        {
            if (!EsSuperAdmin) return StatusCode(403, new { ok = false, mensaje = "Solo el administrador del sistema." });
            var empresa = EmpresaObjetivo();
            var titular = TitularNoPrincipal(empresa, idTitular, out string? motivo);
            if (titular == null) return Json(new { ok = false, mensaje = motivo });

            // Conserva el origen si el punto de venta ya existia (uno traido de ARCA sigue siendo "ARCA" al editarlo).
            var previo = _titulares!.ListarPuntosVenta(idTitular).FirstOrDefault(p => p.CodPuntoVentaAfip == codigo);
            string? error = _titulares.GuardarPuntoVenta(empresa!.IdEmpresa, new Entidades.TitularPuntoVenta
            {
                IdTitular = idTitular,
                CodPuntoVentaAfip = codigo,
                Descripcion = (descripcion ?? "").Trim(),
                Habilitado = habilitado,
                Origen = previo?.Origen ?? "MANUAL"
            });
            if (error != null) return Json(new { ok = false, mensaje = error });

            // Titular compartido: el numero de comprobante es por CUIT + punto de venta + tipo; si otra empresa usa el mismo punto
            // de venta de este CUIT las dos compiten por el mismo numero. Es un aviso, no bloquea.
            string aviso = "";
            if (habilitado)
            {
                var otras = _titulares.OtrasEmpresasConPuntoVenta(titular.Cuit, codigo, empresa.IdEmpresa);
                if (otras.Count > 0)
                    aviso = " Aviso: el punto de venta " + codigo + " de este CUIT también lo usa " + string.Join(", ", otras.Select(o => o.Nombre))
                        + ". Si las dos facturan a la vez compiten por el mismo número y ARCA rechaza una; conviene un punto de venta distinto por empresa.";
            }
            return Json(new { ok = true, mensaje = "Punto de venta guardado." + aviso });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarPuntoVenta(int idTitular, int codigo)
        {
            if (!EsSuperAdmin) return StatusCode(403, new { ok = false, mensaje = "Solo el administrador del sistema." });
            var empresa = EmpresaObjetivo();
            var titular = TitularNoPrincipal(empresa, idTitular, out string? motivo);
            if (titular == null) return Json(new { ok = false, mensaje = motivo });

            string? error = _titulares!.EliminarPuntoVenta(empresa!.IdEmpresa, idTitular, codigo);
            return Json(new { ok = error == null, mensaje = error ?? "Punto de venta eliminado." });
        }

        // Punto de venta que el titular usa en una sucursal donde NO es el asignado (para facturar una sola factura con el).
        // codigo vacio / 0 lo borra.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarPuntoVentaSucursal(int idTitular, int idSucursal, int? codigo)
        {
            if (!EsSuperAdmin) return StatusCode(403, new { ok = false, mensaje = "Solo el administrador del sistema." });
            var empresa = EmpresaObjetivo();
            var titular = TitularNoPrincipal(empresa, idTitular, out string? motivo);
            if (titular == null) return Json(new { ok = false, mensaje = motivo });

            // La sucursal tiene que ser de la empresa de la sesion (nunca confiar en el id del request).
            var sucursal = _sucursales.findById(idSucursal);
            if (sucursal == null || sucursal.IdEmpresa != empresa!.IdEmpresa)
                return Json(new { ok = false, mensaje = "La sucursal no existe." });

            string? error = _titulares!.GuardarPuntoVentaSucursal(empresa.IdEmpresa, idTitular, idSucursal, codigo);
            return Json(new { ok = error == null, mensaje = error ?? "Punto de venta de la sucursal guardado." });
        }

        // ------------------------------------------------------------------ helpers

        // El titular debe existir, ser de la empresa de la sesion y no ser el principal (este usa el punto de venta de la sucursal).
        private Entidades.TitularFiscal? TitularNoPrincipal(Entidades.Empresa? empresa, int idTitular, out string? motivo)
        {
            motivo = null;
            if (_titulares == null || empresa == null) { motivo = "Los titulares de facturación requieren la base Postgres."; return null; }
            var titular = _titulares.Obtener(idTitular);
            if (titular == null || titular.IdEmpresa != empresa.IdEmpresa) { motivo = "El titular no existe."; return null; }
            if (titular.EsPredeterminado) { motivo = "El titular principal usa el punto de venta de cada sucursal (se edita en Administración del sistema)."; return null; }
            return titular;
        }

        // La persona debe ser de la empresa o compartida (idEmpresa 0).
        private Entidades.Persona? PersonaDeLaEmpresa(int idPersona, Entidades.Empresa empresa)
        {
            if (idPersona <= 0) return null;
            var persona = _personas.findById(idPersona);
            if (persona == null) return null;
            if (persona.IdEmpresa != 0 && persona.IdEmpresa != empresa.IdEmpresa) return null;
            return persona;
        }

        // CUIT de la persona (campo cuit; si esta vacio, la identificacion) o 0 si no es valido.
        private static long CuitDePersona(Entidades.Persona persona)
        {
            long cuit = Entidades.CuitHelper.ParsearValido(persona.Cuit);
            return cuit > 0 ? cuit : Entidades.CuitHelper.ParsearValido(persona.Identificacion);
        }

        private InfoCertificado LeerCertificado(Entidades.Empresa empresa, Entidades.TitularFiscal titular, bool homologacion)
        {
            // Certificados y claves se leen con la sesion: de otra empresa (super-admin) no hay estado para mostrar.
            if (EsOtraEmpresa) return new InfoCertificado { Estado = EstadoCertificado.SinCertificado, Mensaje = "No disponible desde otra empresa." };
            try
            {
                return titular.EsPredeterminado
                    ? _estado.Leer(empresa, homologacion)
                    : _estado.LeerTitular(empresa, titular, homologacion);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Titulares fiscales: no se pudo leer el certificado del titular {IdTitular}.", titular.IdTitular);
                return new InfoCertificado { Estado = EstadoCertificado.Ilegible, Mensaje = "No se pudo leer el certificado." };
            }
        }

        // Titular de cada sucursal con su punto de venta (solo lectura; se cambia en Mis Sucursales).
        private List<SucursalTitularVm> ArmarSucursales(Entidades.Empresa empresa)
        {
            var lista = new List<SucursalTitularVm>();
            var titulares = _titulares!.Listar(empresa.IdEmpresa, false);
            var principal = titulares.FirstOrDefault(t => t.EsPredeterminado);
            foreach (var sucursal in _sucursales.findAll().Where(s => s.IdEmpresa == empresa.IdEmpresa).OrderBy(s => s.sucursal))
            {
                int? idAsignado = _titulares.ObtenerIdTitularDeSucursal(sucursal.IdSucursal);
                var titular = titulares.FirstOrDefault(t => t.IdTitular == (idAsignado ?? principal?.IdTitular ?? 0)) ?? principal;
                int puntoVenta = titular == null ? 0 : _titulares.ResolverPuntoVenta(titular, sucursal, out _);
                lista.Add(new SucursalTitularVm
                {
                    IdSucursal = sucursal.IdSucursal,
                    Nombre = sucursal.sucursal ?? "",
                    TitularNombre = titular?.RazonSocial ?? "",
                    PuntoVenta = puntoVenta
                });
            }
            return lista;
        }

        private void CargarVm(TitularEditVm vm, Entidades.TitularFiscal titular, Entidades.Empresa empresa)
        {
            vm.IdTitular = titular.IdTitular;
            vm.EsPrincipal = titular.EsPredeterminado;
            vm.Habilitado = titular.Habilitado;
            vm.IdPersona = titular.IdPersona ?? 0;
            vm.Cuit = titular.Cuit.ToString(CultureInfo.InvariantCulture);
            vm.RazonSocial = titular.RazonSocial ?? "";
            vm.IdIva = titular.IdIva;
            vm.CondicionIva = titular.EsRRII ? "Responsable Inscripto" : (titular.IdIva == Entidades.Persona.codIvaMonotributo_Afip ? "Monotributo" : (titular.CondicionIVA ?? ""));
            vm.PersonaNombre = titular.RazonSocial ?? "";
            vm.InicioActividad = titular.InicioActividad?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "";
            vm.Iibb = titular.Iibb > 0 ? titular.Iibb.ToString(CultureInfo.InvariantCulture) : "";
            vm.LimiteMensual = titular.LimiteMensual?.ToString("0.##", CultureInfo.InvariantCulture) ?? "";
            vm.LimiteAnual = titular.LimiteAnual?.ToString("0.##", CultureInfo.InvariantCulture) ?? "";
            vm.VentanaAnual = titular.VentanaAnual;
            vm.TieneCertificado = titular.TieneCertificado;
            vm.Revocado = titular.Revocado;
            // Titular compartido: el aviso de topes solo aparece si el CUIT ya es titular en otras empresas.
            if (titular.Cuit > 0)
                vm.OtrasEmpresasConCuit = _titulares!.OtrasEmpresasConCuit(titular.Cuit, empresa.IdEmpresa).Select(o => o.Nombre).ToList();

            if (titular.EsPredeterminado) return;

            vm.PuntosVenta = _titulares!.ListarPuntosVenta(titular.IdTitular)
                .OrderBy(p => p.CodPuntoVentaAfip)
                .Select(p => new TitularPuntoVentaVm { Codigo = p.CodPuntoVentaAfip, Descripcion = p.Descripcion ?? "", Habilitado = p.Habilitado, Origen = p.Origen ?? "MANUAL" })
                .ToList();

            var asignados = _titulares.ListarPuntosVentaAsignados(titular.IdTitular);
            foreach (var sucursal in _sucursales.findAll().Where(s => s.IdEmpresa == empresa.IdEmpresa).OrderBy(s => s.sucursal))
            {
                vm.Sucursales.Add(new TitularSucursalPvVm
                {
                    IdSucursal = sucursal.IdSucursal,
                    Nombre = sucursal.sucursal ?? "",
                    PuntoVentaAsignado = asignados.Where(a => a.IdSucursal == sucursal.IdSucursal).Select(a => (int?)a.CodPuntoVentaAfip).FirstOrDefault(),
                    EsTitularDeLaSucursal = _titulares.ObtenerIdTitularDeSucursal(sucursal.IdSucursal) == titular.IdTitular
                });
            }
        }

        // Importe opcional ("1.234,56" o "1234.56"): vacio = sin tope. false si no es un numero.
        private static bool TryParseMonto(string? texto, out decimal? monto)
        {
            monto = null;
            if (string.IsNullOrWhiteSpace(texto)) return true;
            string limpio = texto.Trim().Replace(" ", "");
            // Con coma decimal (es-AR) los puntos son separadores de miles.
            if (limpio.Contains(',')) limpio = limpio.Replace(".", "").Replace(',', '.');
            if (!decimal.TryParse(limpio, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal valor)) return false;
            monto = valor;
            return true;
        }

        private IActionResult AccesoDenegado()
        {
            ViewBag.Seccion = "Titulares de facturación";
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }
    }
}
