// Modelos de la pantalla Registros eliminados (RegistrosEliminadosController). Ver docs/DECISIONS.md "Eliminar compras, stock y movimientos".
using System;
using System.Collections.Generic;

namespace WebCore.Models
{
    public class RegistrosEliminadosIndexVm
    {
        // Modulos cuyos eliminados puede ver el usuario (el admin: los tres).
        public List<string> ModulosPermitidos { get; set; } = new List<string>();
        public string Modulo { get; set; } = "";
        public string FechaDesde { get; set; } = "";
        public string FechaHasta { get; set; } = "";
        public string Texto { get; set; } = "";
        public List<RegistrosEliminadosItemVm> Items { get; set; } = new List<RegistrosEliminadosItemVm>();
    }

    public class RegistrosEliminadosItemVm
    {
        public int Id { get; set; }
        public string Modulo { get; set; } = "";
        public int IdRegistro { get; set; }
        public string TipoRegistro { get; set; } = "";
        public DateTime? FechaRegistro { get; set; }
        public string Sucursal { get; set; } = "";
        public string Persona { get; set; } = "";
        public double? Importe { get; set; }
        public int CantLineas { get; set; }
        public string UsuarioElimino { get; set; } = "";
        public string UsuarioSesion { get; set; } = "";
        public DateTime FechaEliminacion { get; set; }
        public string Motivo { get; set; } = "";
    }
}
