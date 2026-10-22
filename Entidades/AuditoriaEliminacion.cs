using System;
using System.Collections.Generic;

namespace Entidades
{
    // Foto append-only de un registro ELIMINADO (compra, movimiento de stock o movimiento entre sucursales):
    // quien lo elimino, cuando, el motivo y lo que tenia (cabecera + lineas). El registro original se borra de su
    // tabla; esto es lo unico que queda. Tablas auditoriaeliminaciones / auditoriaeliminacionlineas (Postgres) y
    // AuditoriaEliminaciones / AuditoriaEliminacionLineas (SQL Server). Ver docs/DECISIONS.md "Eliminar compras, stock y movimientos".
    public class AuditoriaEliminacion
    {
        public const string ModuloCompra = "COMPRA";
        public const string ModuloStock = "STOCK";
        public const string ModuloMovimiento = "MOVIMIENTO";

        public const int LargoMinimoMotivo = 10;
        public const int LargoMaximoMotivo = 300;

        public int Id { get; set; }
        public string Modulo { get; set; }
        public int IdRegistro { get; set; }
        public string TipoRegistro { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public int? IdSucursal { get; set; }
        public string SucursalNombre { get; set; }
        public int? IdSucursalDestino { get; set; }
        public string SucursalDestinoNombre { get; set; }
        public int? IdPersona { get; set; }
        public string PersonaNombre { get; set; }
        public string NroRemito { get; set; }
        public bool? EnCtaCte { get; set; }
        // Solo pesajes de medias (compras.cantmedias / kgsmedias): para poder recuperar el registro.
        public int? CantMedias { get; set; }
        public double? KgsMedias { get; set; }
        public double? Importe { get; set; }
        public double? TotalKg { get; set; }
        public int CantLineas { get; set; }
        public string Observaciones { get; set; }
        public int? IdUsuarioCreador { get; set; }
        public string UsuarioCreadorNombre { get; set; }
        public DateTime? Creado { get; set; }

        // Quien puso la clave y elimino (la persona real) y la cuenta con la que estaba abierta la sesion
        // (distinta solo con la cuenta compartida de produccion).
        public int IdUsuarioElimino { get; set; }
        public string UsuarioEliminoNombre { get; set; }
        public int IdUsuarioSesion { get; set; }
        public string UsuarioSesionNombre { get; set; }
        public DateTime FechaEliminacion { get; set; }
        public string Motivo { get; set; }
        public string Ip { get; set; }

        // Texto libre: asiento contrario en la cta cte, egreso de caja opuesto, etc.
        public string Efectos { get; set; }

        public List<AuditoriaEliminacionLinea> Lineas { get; set; } = new List<AuditoriaEliminacionLinea>();
    }

    public class AuditoriaEliminacionLinea
    {
        public int Id { get; set; }
        public int? IdCorte { get; set; }
        public long? Codigo { get; set; }
        public string Producto { get; set; }
        public int? IdSucursal { get; set; }
        public double? CantKg { get; set; }
        public int? CantUnidad { get; set; }
        public double? PrecioKg { get; set; }
        public string Detalle { get; set; }
    }
}
