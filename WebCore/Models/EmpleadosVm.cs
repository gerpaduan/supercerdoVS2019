using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCore.Models
{
    public class EmpleadoIndexVm
    {
        public bool PuedeAdministrar { get; set; }
        public string Texto { get; set; } = "";
        public List<EmpleadoResumenVm> Items { get; set; } = new List<EmpleadoResumenVm>();
    }

    public class EmpleadoResumenVm
    {
        public int Id { get; set; }
        public string Legajo { get; set; } = "";
        public string RazonSocial { get; set; } = "";
        public string UsuarioLogin { get; set; } = "";
        public string FormaLiquidacion { get; set; } = "";
        public bool Activo { get; set; }
    }

    // Alta/edicion unificada de Empleado: crea o vincula una Persona y un Usuario en un solo paso
    // (ver Negocio/Empleado.cs). En edicion, los campos de Persona/Usuario editan los datos YA
    // vinculados -- no se puede re-vincular a otra Persona/Usuario existente desde esta pantalla.
    public class EmpleadoEditVm
    {
        public int Id { get; set; }
        public bool EsEdicion { get; set; }
        public int IdEmpresa { get; set; }

        // Solo se usa en alta (Id==0): con valor vincula una Persona existente tal cual (se
        // ignoran los demás campos de Persona); null/vacío crea una nueva con los campos de abajo.
        // Nullable a proposito: el hidden input queda vacío cuando no hay selección, y bindear un
        // string vacío a un int no-nullable tira "The value '' is invalid." (ModelState invalido).
        public int? IdPersonaExistente { get; set; }

        // Sin [Required]: solo son obligatorios cuando IdPersonaExistente<=0 (se crea una persona
        // nueva) -- eso se valida a mano en EmpleadosController.Guardar, porque [Required] no
        // puede expresar "obligatorio solo si X". Ver docs/DECISIONS.md 2026-09-30.
        public string PersonaRazonSocial { get; set; } = "";
        public string PersonaIdentificacion { get; set; } = "";
        public string PersonaTelefono { get; set; } = "";
        public string PersonaDomicilio { get; set; } = "";

        // Idem IdPersonaExistente (nullable por el mismo motivo: hidden input vacío).
        public int? IdUsuarioExistente { get; set; }

        // Idem PersonaRazonSocial: obligatorios solo cuando IdUsuarioExistente<=0, validado a mano.
        public string UsuarioNombre { get; set; } = "";
        public string UsuarioLogin { get; set; } = "";
        public string UsuarioClave { get; set; } = "";

        [Required(ErrorMessage = "El legajo es obligatorio.")]
        public string Legajo { get; set; } = "";

        public Entidades.Empleado.formaLiquidacion FormaLiquidacion { get; set; }
        public DateTime FechaIngreso { get; set; } = DateTime.Today;
        public bool Activo { get; set; } = true;

        // Tarifas cargadas en el mismo formulario -- se agregan como filas nuevas (append-only,
        // ver Entidades/EmpleadoTarifa.cs), nunca reemplazan una tarifa existente.
        public List<EmpleadoTarifaItemVm> TarifasActuales { get; set; } = new List<EmpleadoTarifaItemVm>();
    }

    public class EmpleadoTarifaItemVm
    {
        public string Turno { get; set; } = ""; // "" = general (null)
        public string TurnoTexto { get; set; } = "";
        public string DiaSemana { get; set; } = ""; // "" = general (null)
        public decimal Valor { get; set; }
        public DateTime VigenteDesde { get; set; }
    }

    public class EmpleadoVacacionesVm
    {
        public int IdEmpleado { get; set; }
        public string EmpleadoNombre { get; set; } = "";
        public List<Entidades.EmpleadoVacacion> Items { get; set; } = new List<Entidades.EmpleadoVacacion>();
        public DateTime NuevaFechaDesde { get; set; } = DateTime.Today;
        public DateTime NuevaFechaHasta { get; set; } = DateTime.Today;
        public string NuevaObservaciones { get; set; } = "";
    }

    public class EmpleadoHistorialVm
    {
        public int IdEmpleado { get; set; }
        public string EmpleadoNombre { get; set; } = "";
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public List<Entidades.EmpleadoTarifa> Tarifas { get; set; } = new List<Entidades.EmpleadoTarifa>();
        public List<Entidades.LiquidacionSueldo> Liquidaciones { get; set; } = new List<Entidades.LiquidacionSueldo>();
    }

    public class TarifaGeneralIndexVm
    {
        public bool PuedeAdministrar { get; set; }
        public List<Entidades.TarifaGeneral> Items { get; set; } = new List<Entidades.TarifaGeneral>();
        public List<SelectListItem> FormasLiquidacion { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Turnos { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DiasSemana { get; set; } = new List<SelectListItem>();
    }
}
