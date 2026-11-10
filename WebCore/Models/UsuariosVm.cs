using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCore.Models
{
    public class UsuarioIndexVm
    {
        public bool PuedeAdministrar { get; set; }
        public List<UsuarioResumenVm> Items { get; set; } = new List<UsuarioResumenVm>();
    }

    public class UsuarioResumenVm
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Usuario { get; set; } = "";
        public bool Admin { get; set; }
        public bool Activo { get; set; }
        public string Email { get; set; } = "";
        public int IdSucursalUser { get; set; }
        public string SucursalNombre { get; set; } = "";
        public int IdEmpresa { get; set; }
        public bool Bloqueado { get; set; }
        // Bloqueado solo para ingresos desde dispositivos no seguros (ver Entidades.Usuario).
        public bool BloqueadoNoSeguro { get; set; }
        public bool TienePin { get; set; }
        // Para las etiquetas del listado (2026-11-10, rediseno): tipo de cuenta, dispositivo seguro y si entra al POS.
        public bool EsProduccion { get; set; }
        public bool RequiereDispositivoSeguro { get; set; }
        public bool EntraAlPos { get; set; }
    }

    public class UsuarioEditVm
    {
        public int Id { get; set; }
        public bool EsEdicion { get; set; }
        public bool SoloLectura { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string Usuario { get; set; } = "";

        public string Clave { get; set; } = "";
        public bool Admin { get; set; }
        public bool Activo { get; set; } = true;
        public string Email { get; set; } = "";
        public int IdSucursalUser { get; set; }
        public bool PermitirLoginFueraSucursal { get; set; }
        public bool EsUsuarioProduccion { get; set; }
        public bool RequiereDispositivoSeguro { get; set; }
        // Bloqueo por inactividad (0 = nunca; el usuario de produccion queda exento igual). Tope 24 h.
        [Range(0, 1440, ErrorMessage = "Los minutos de inactividad deben estar entre 0 y 1440.")]
        public int MinutosInactividad { get; set; } = Entidades.Usuario.MinutosInactividadPorDefecto;
        public bool PuedeOperarPOS { get; set; } = true;
        public int IdEmpresa { get; set; }
        public List<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();
    }

    public class UsuarioPermisosVm
    {
        public int IdUsuario { get; set; }
        public string UsuarioNombre { get; set; } = "";
        public string UsuarioLogin { get; set; } = "";
        public bool SoloLectura { get; set; }
        public List<UsuarioPermisoItemVm> Items { get; set; } = new List<UsuarioPermisoItemVm>();
    }

    public class UsuarioPermisoItemVm
    {
        public int IdForm { get; set; }
        public string NombrePermiso { get; set; } = "";
        public string Detalle { get; set; } = "";
        public bool PuedeVer { get; set; }
        public int DiasVer { get; set; }
        public bool PuedeEditar { get; set; }
        public int DiasEditar { get; set; }
        public bool SoloRegistrosPropios { get; set; }
        // != "" = hoy ningun chequeo del codigo mira este permiso (CatalogoPermisos.SinEfectoHoy): la grilla avanzada lo marca.
        public string SinEfectoHoy { get; set; } = "";
    }

    // ===== Asistente de usuarios y permisos (Usuarios/Asistente, 2026-11-10) =====
    // Datos del usuario + estado de cada permiso. Plano (sin anidar) para que el formulario lo bindee con nombres simples.
    public class UsuarioAsistenteVm
    {
        public int Id { get; set; }
        public bool EsEdicion { get; set; }
        public string Nombre { get; set; } = "";
        public string Usuario { get; set; } = "";
        public string Clave { get; set; } = "";
        public string Email { get; set; } = "";
        public int IdSucursalUser { get; set; }
        public bool Activo { get; set; } = true;
        // "empleado" (permisos a medida), "admin" o "produccion" (cuenta compartida de un sector).
        public string TipoUsuario { get; set; } = TipoUsuarioEmpleado;
        public bool PermitirLoginFueraSucursal { get; set; }
        public bool RequiereDispositivoSeguro { get; set; }
        public int MinutosInactividad { get; set; } = Entidades.Usuario.MinutosInactividadPorDefecto;
        public int IdEmpresa { get; set; }

        // ---- Solo para dibujar la cabecera de la ficha de un usuario existente (no se postea) ----
        public string SucursalNombre { get; set; } = "";
        public bool TienePin { get; set; }
        public DateTime? PinActualizadoUtc { get; set; }
        public bool Bloqueado { get; set; }
        public bool BloqueadoNoSeguro { get; set; }

        // Un permiso por fila de `formularios` que el asistente muestra (lo que se postea al guardar).
        public List<WebCore.Helpers.EstadoPermisoAsistente> Permisos { get; set; } = new List<WebCore.Helpers.EstadoPermisoAsistente>();

        // ---- Solo para dibujar la pagina (no se postea) ----
        public int PasoInicial { get; set; } = 1;
        public List<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();
        public List<AsistenteModuloVm> Modulos { get; set; } = new List<AsistenteModuloVm>();
        public List<AsistentePerfilVm> Perfiles { get; set; } = new List<AsistentePerfilVm>();
        public List<SelectListItem> UsuariosParaCopiar { get; set; } = new List<SelectListItem>();

        public const string TipoUsuarioEmpleado = "empleado";
        public const string TipoUsuarioAdmin = "admin";
        public const string TipoUsuarioProduccion = "produccion";
    }

    public class AsistenteModuloVm
    {
        public string Nombre { get; set; } = "";
        public List<AsistentePermisoItemVm> Items { get; set; } = new List<AsistentePermisoItemVm>();
    }

    // Un permiso tal como lo dibuja el asistente: definicion del catalogo + estado actual del usuario.
    public class AsistentePermisoItemVm
    {
        public int IdForm { get; set; }
        public string Titulo { get; set; } = "";
        public string Ayuda { get; set; } = "";
        // "SoloVer" | "SoloModificar" | "VerYModificar"
        public string Tipo { get; set; } = "";
        public string TextoVer { get; set; } = "";
        public string TextoModificar { get; set; } = "";
        public string Aviso { get; set; } = "";
        public bool UsaDiasVer { get; set; }
        public bool UsaDiasEditar { get; set; }
        public bool UsaAlcance { get; set; }
        public bool PosConVer { get; set; }
        public bool PosConModificar { get; set; }
        // != "" = efecto colateral serio de dar "Ver y modificar": el resumen de la derecha lo lista en "Atencion".
        public string RiesgoAlModificar { get; set; } = "";
        // Una cuenta de produccion nunca puede tener este permiso.
        public bool BloqueadoProduccion { get; set; }
        public WebCore.Helpers.EstadoPermisoAsistente Estado { get; set; } = new WebCore.Helpers.EstadoPermisoAsistente();
    }

    public class AsistentePerfilVm
    {
        public string Clave { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public List<WebCore.Helpers.EstadoPermisoAsistente> Estados { get; set; } = new List<WebCore.Helpers.EstadoPermisoAsistente>();
    }
}
