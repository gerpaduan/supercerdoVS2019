using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilidades;
using Web.Models;

namespace Web.Helpers
{
    // Adaptador Postgres del modulo de administracion de plataforma: implementa
    // ISystemAdministrationRepository traduciendo Web.Models.* <-> Entidades.* en memoria (sin
    // SQL propio, todo el acceso a datos vive en DatosPostgres.SystemAdministrationPg) y
    // resolviendo los mismos defaults de negocio que el repo SQL Server resuelve en
    // SetEmpresaParams/SetSucursalParams/SetUsuarioParams (pais->"Argentina", nombreFantasia->
    // razonSocialAfip, tenantSlug/basePath autogenerados por slug, colorForm fijo "SteelBlue").
    // Ver docs/DECISIONS.md 2026-08-25.
    public class SystemAdministrationRepositoryPg : ISystemAdministrationRepository
    {
        private readonly DatosPostgres.SystemAdministrationPg _repo;

        public SystemAdministrationRepositoryPg(DatosPostgres.SystemAdministrationPg repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public bool EsSuperAdmin(int idUsuario) => _repo.EsSuperAdmin(idUsuario);

        // "telefono"/"activa" de Sucursal nunca existieron en Postgres (confirmado en vivo que
        // tampoco existen hoy en SQL Server real -- ver migracion 20260825), asi que este modulo
        // asume ambas columnas siempre ausentes: mismo fallback que el original usa cuando la
        // columna no existe (Activa por defecto true, Telefono vacio, ver ObtenerSucursales/
        // ObtenerSucursal de SystemAdministrationRepository.cs).
        public bool TablaSucursalTieneTelefono() => false;
        public bool TablaSucursalTieneActiva() => false;

        public List<SystemAdministrationEmpresaResumenVm> ObtenerEmpresas()
        {
            return _repo.ObtenerEmpresas()
                .Select(e => new SystemAdministrationEmpresaResumenVm
                {
                    IdEmpresa = e.IdEmpresa,
                    RazonSocialAfip = e.RazonSocialAfip ?? "",
                    NombreFantasia = e.NombreFantasia ?? "",
                    Cuit = e.Cuit,
                    CondicionIVA = e.CondicionIVA ?? "",
                    Telefono = e.Telefono ?? "",
                    Email = e.Email ?? "",
                    Activa = e.Activa == 1
                })
                .ToList();
        }

        public SystemAdministrationEmpresaEditVm ObtenerEmpresa(int idEmpresa)
        {
            var e = _repo.ObtenerEmpresa(idEmpresa);
            if (e == null) return null;

            return new SystemAdministrationEmpresaEditVm
            {
                EsEdicion = true,
                IdEmpresa = e.IdEmpresa,
                RazonSocialAfip = e.RazonSocialAfip ?? "",
                Cuit = e.Cuit > 0 ? e.Cuit.ToString() : "",
                NombreFantasia = e.NombreFantasia ?? "",
                Slogan1 = e.Slogan1 ?? "",
                Slogan2 = e.Slogan2 ?? "",
                Slogan3 = e.Slogan3 ?? "",
                Iibb = e.Iibb > 0 ? e.Iibb.ToString() : "",
                CondicionIVA = e.CondicionIVA ?? "",
                InicioActividad = e.InicioActividad == default(DateTime) ? (DateTime?)null : e.InicioActividad,
                TenantSlug = e.TenantSlug ?? "",
                Domicilio = e.Domicilio ?? "",
                Ciudad = e.Ciudad ?? "",
                Pais = e.Pais ?? "",
                Telefono = e.Telefono ?? "",
                Email = e.Email ?? "",
                BasePath = e.BasePath ?? "",
                EsRRII = e.EsRRII,
                NombreCertificadoPfx = e.NombreCertificado_pfx ?? "",
                EntornoHomoProd = e.Entorno_HOMO_PROD ?? "",
                BaseDatosNombre = e.BaseDatosNombre ?? "",
                Activa = e.Activa == 1,
                Observaciones = e.Observaciones ?? ""
            };
        }

        public int CrearEmpresa(SystemAdministrationEmpresaEditVm model)
        {
            var empresa = MapEmpresaEntidad(model);
            long codigo = model.CodigoGenericoCodigo > 0 ? model.CodigoGenericoCodigo : 999999;
            string nombre = !string.IsNullOrWhiteSpace(model.CodigoGenericoNombre) ? model.CodigoGenericoNombre : "Codigo Generico";
            int idAlicuotaIva = model.CodigoGenericoIdAlicuotaIva > 0 ? model.CodigoGenericoIdAlicuotaIva : 4;

            return _repo.CrearEmpresa(empresa, codigo, nombre, idAlicuotaIva);
        }

        public void ActualizarEmpresa(SystemAdministrationEmpresaEditVm model)
        {
            _repo.ActualizarEmpresa(MapEmpresaEntidad(model));
        }

        private static Entidades.Empresa MapEmpresaEntidad(SystemAdministrationEmpresaEditVm model)
        {
            return new Entidades.Empresa
            {
                IdEmpresa = model.IdEmpresa,
                RazonSocialAfip = (model.RazonSocialAfip ?? "").Trim(),
                Cuit = ParseLong(model.Cuit),
                NombreFantasia = NullToFallback(model.NombreFantasia, (model.RazonSocialAfip ?? "").Trim()),
                Slogan1 = model.Slogan1,
                Slogan2 = model.Slogan2,
                Slogan3 = model.Slogan3,
                Iibb = ParseLong(model.Iibb),
                CondicionIVA = model.CondicionIVA,
                InicioActividad = model.InicioActividad ?? default(DateTime),
                TenantSlug = NullToFallback(model.TenantSlug, BuildSlug(model.RazonSocialAfip)),
                Domicilio = model.Domicilio,
                Ciudad = model.Ciudad,
                Pais = NullToFallback(model.Pais, "Argentina"),
                Telefono = model.Telefono,
                Email = model.Email,
                BasePath = NullToFallback(model.BasePath, BuildBasePath(model.TenantSlug, model.RazonSocialAfip)),
                EsRRII = model.EsRRII,
                NombreCertificado_pfx = model.NombreCertificadoPfx,
                Entorno_HOMO_PROD = model.EntornoHomoProd,
                BaseDatosNombre = model.BaseDatosNombre,
                Activa = (byte)(model.Activa ? 1 : 0),
                Observaciones = model.Observaciones
            };
        }

        public List<SystemAdministrationSucursalResumenVm> ObtenerSucursales(int idEmpresa = 0)
        {
            return _repo.ObtenerSucursales(idEmpresa)
                .Select(s => new SystemAdministrationSucursalResumenVm
                {
                    IdSucursal = s.IdSucursal,
                    IdEmpresa = s.IdEmpresa,
                    EmpresaNombre = s.Empresa?.RazonSocialAfip ?? "",
                    Sucursal = s.SucursalNombre ?? "",
                    Direccion = s.Direccion ?? "",
                    Localidad = s.Localidad ?? "",
                    CodPuntoVentaAfip = s.CodPuntoVentaAfip,
                    Telefono = "",
                    Activa = true
                })
                .ToList();
        }

        public SystemAdministrationSucursalEditVm ObtenerSucursal(int idSucursal)
        {
            var s = _repo.ObtenerSucursal(idSucursal);
            if (s == null) return null;

            return new SystemAdministrationSucursalEditVm
            {
                EsEdicion = true,
                IdSucursal = s.IdSucursal,
                IdEmpresa = s.IdEmpresa,
                Sucursal = s.SucursalNombre ?? "",
                Direccion = s.Direccion ?? "",
                Localidad = s.Localidad ?? "",
                Provincia = s.Provincia ?? "",
                Pais = s.Pais ?? "",
                CodPuntoVentaAfip = s.CodPuntoVentaAfip > 0 ? (int?)s.CodPuntoVentaAfip : null,
                Telefono = "",
                Activa = true,
                Observaciones = s.Observaciones ?? "",
                TieneTelefono = false,
                TieneActiva = false
            };
        }

        public int CrearSucursal(SystemAdministrationSucursalEditVm model)
        {
            return _repo.CrearSucursal(MapSucursalEntidad(model), model.IdSucursalOrigenPuntoStock);
        }

        public void ActualizarSucursal(SystemAdministrationSucursalEditVm model)
        {
            _repo.ActualizarSucursal(MapSucursalEntidad(model));
        }

        private static Entidades.Sucursal MapSucursalEntidad(SystemAdministrationSucursalEditVm model)
        {
            return new Entidades.Sucursal
            {
                IdSucursal = model.IdSucursal,
                IdEmpresa = model.IdEmpresa,
                SucursalNombre = model.Sucursal,
                Direccion = model.Direccion,
                Localidad = model.Localidad,
                Provincia = model.Provincia,
                Pais = NullToFallback(model.Pais, "Argentina"),
                CodPuntoVentaAfip = model.CodPuntoVentaAfip ?? 0,
                Observaciones = model.Observaciones
            };
        }

        public List<SystemAdministrationUsuarioResumenVm> ObtenerUsuarios(int idEmpresa = 0)
        {
            return _repo.ObtenerUsuarios(idEmpresa)
                .Select(u => new SystemAdministrationUsuarioResumenVm
                {
                    Id = u.Id,
                    IdEmpresa = u.IdEmpresa,
                    EmpresaNombre = u.Empresa?.RazonSocialAfip ?? "",
                    IdSucursalUser = u.IdSucursal,
                    SucursalNombre = u.SucursalNombre ?? "",
                    Nombre = u.Nombre ?? "",
                    Usuario = u.User ?? "",
                    Email = u.Email ?? "",
                    Admin = u.Admin,
                    Activo = u.Activo
                })
                .ToList();
        }

        public SystemAdministrationUsuarioEditVm ObtenerUsuario(int idUsuario)
        {
            var u = _repo.ObtenerUsuario(idUsuario);
            if (u == null) return null;

            return new SystemAdministrationUsuarioEditVm
            {
                EsEdicion = true,
                Id = u.Id,
                Nombre = u.Nombre ?? "",
                Usuario = u.User ?? "",
                Email = u.Email ?? "",
                IdEmpresa = u.IdEmpresa,
                IdSucursalUser = u.IdSucursal,
                Admin = u.Admin,
                Activo = u.Activo,
                PermitirLoginFueraSucursal = u.PermitirLoginFueraSucursal
            };
        }

        public int CrearUsuario(SystemAdministrationUsuarioEditVm model)
        {
            var usuario = MapUsuarioEntidad(model);
            var hash = string.IsNullOrWhiteSpace(model.Clave) ? null : PasswordSecurity.HashPassword(model.Clave.Trim());

            return _repo.CrearUsuario(usuario, hash?.Hash, hash?.Salt, hash?.Iterations ?? 0);
        }

        public void ActualizarUsuario(SystemAdministrationUsuarioEditVm model)
        {
            var usuario = MapUsuarioEntidad(model);
            var hash = string.IsNullOrWhiteSpace(model.Clave) ? null : PasswordSecurity.HashPassword(model.Clave.Trim());

            _repo.ActualizarUsuario(usuario, model.Clave, hash?.Hash, hash?.Salt, hash?.Iterations ?? 0);
        }

        private static Entidades.Usuario MapUsuarioEntidad(SystemAdministrationUsuarioEditVm model)
        {
            return new Entidades.Usuario
            {
                Id = model.Id,
                Nombre = model.Nombre,
                User = model.Usuario,
                Clave = model.Clave,
                Email = model.Email,
                Admin = model.Admin,
                Activo = model.Activo,
                IdEmpresa = model.IdEmpresa,
                IdSucursal = model.IdSucursalUser,
                PermitirLoginFueraSucursal = model.PermitirLoginFueraSucursal
            };
        }

        public int CrearAltaRapida(SystemAdministrationAltaRapidaVm model)
        {
            var empresa = MapEmpresaEntidad(model.Empresa);
            long codigo = model.Empresa.CodigoGenericoCodigo > 0 ? model.Empresa.CodigoGenericoCodigo : 999999;
            string nombre = !string.IsNullOrWhiteSpace(model.Empresa.CodigoGenericoNombre) ? model.Empresa.CodigoGenericoNombre : "Codigo Generico";
            int idAlicuotaIva = model.Empresa.CodigoGenericoIdAlicuotaIva > 0 ? model.Empresa.CodigoGenericoIdAlicuotaIva : 4;

            var sucursal = MapSucursalEntidad(model.Sucursal);
            var usuario = MapUsuarioEntidad(model.Usuario);
            var hash = string.IsNullOrWhiteSpace(model.Usuario.Clave) ? null : PasswordSecurity.HashPassword(model.Usuario.Clave.Trim());

            return _repo.CrearAltaRapida(empresa, codigo, nombre, idAlicuotaIva, sucursal, usuario, hash?.Hash, hash?.Salt, hash?.Iterations ?? 0);
        }

        public bool ExisteCuit(long cuit, int idEmpresaExcluir) => _repo.ExisteCuit(cuit, idEmpresaExcluir);
        public bool ExisteUsuario(string usuario, int idUsuarioExcluir) => _repo.ExisteUsuario(usuario, idUsuarioExcluir);
        public bool ExisteEmail(string email, int idUsuarioExcluir) => _repo.ExisteEmail(email, idUsuarioExcluir);

        public List<SelectListItem> ObtenerEmpresasSelectList(int idSeleccionado = 0, bool incluirTodas = false)
        {
            var items = ObtenerEmpresas()
                .Select(x => new SelectListItem
                {
                    Value = x.IdEmpresa.ToString(),
                    Text = string.IsNullOrWhiteSpace(x.NombreFantasia) ? x.RazonSocialAfip : (x.NombreFantasia + " (" + x.RazonSocialAfip + ")"),
                    Selected = x.IdEmpresa == idSeleccionado
                })
                .ToList();

            if (incluirTodas)
            {
                items.Insert(0, new SelectListItem
                {
                    Value = "0",
                    Text = "Todas",
                    Selected = idSeleccionado <= 0
                });
            }

            return items;
        }

        public List<SelectListItem> ObtenerSucursalesSelectList(int idEmpresa, int idSeleccionado = 0)
        {
            return ObtenerSucursales(idEmpresa)
                .Where(x => idEmpresa <= 0 || x.IdEmpresa == idEmpresa)
                .Select(x => new SelectListItem
                {
                    Value = x.IdSucursal.ToString(),
                    Text = x.Sucursal,
                    Selected = x.IdSucursal == idSeleccionado
                })
                .ToList();
        }

        public List<SystemAdministrationAlicuotaIvaVm> ObtenerAlicuotasIva()
        {
            return _repo.ObtenerAlicuotasIva()
                .Select(a => new SystemAdministrationAlicuotaIvaVm
                {
                    IdAlicuotaIva = a.IdIva,
                    Alicuota = a.Iva
                })
                .ToList();
        }

        public List<SystemAdministrationCondicionIvaVm> ObtenerCondicionesIva()
        {
            return _repo.ObtenerCondicionesIva()
                .Select(c => new SystemAdministrationCondicionIvaVm
                {
                    Id = c.Id,
                    Descripcion = c.Descripcion
                })
                .ToList();
        }

        private static string NullToFallback(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static long ParseLong(string value)
        {
            long parsed;
            return long.TryParse((value ?? "").Trim(), out parsed) && parsed > 0 ? parsed : 0;
        }

        private static string BuildSlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var chars = value.Trim().ToLowerInvariant().Select(c =>
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                    return c;
                return '-';
            }).ToArray();

            string slug = new string(chars);
            while (slug.Contains("--"))
                slug = slug.Replace("--", "-");

            return slug.Trim('-');
        }

        private static string BuildBasePath(string tenantSlug, string razonSocial)
        {
            string slug = BuildSlug(tenantSlug);
            if (string.IsNullOrWhiteSpace(slug))
                slug = BuildSlug(razonSocial);

            return string.IsNullOrWhiteSpace(slug) ? null : "/" + slug;
        }
    }
}
