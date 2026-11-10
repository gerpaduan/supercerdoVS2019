// Datos de titular propio para la cuenta corriente de un proveedor (pantalla, PDF y CSV): selector, nombres y resumen por titular.
// Compras: titular propio y comprobantes del proveedor (2026-11-09, ver docs/DECISIONS.md). Solo Postgres y solo con 2 o mas titulares.
using System;
using System.Collections.Generic;

namespace WebCore.Models
{
    public class CtaCteTitularesVm
    {
        // La empresa tiene 2 o mas titulares y la cuenta no esta oculta: se ven el selector, la columna y el resumen.
        public bool Mostrar { get; set; }
        // 0 = todos los titulares.
        public int IdTitularFiltro { get; set; }
        public List<TitularCompraOpcionVm> Titulares { get; set; } = new List<TitularCompraOpcionVm>();
        public Dictionary<int, string> Nombres { get; set; } = new Dictionary<int, string>();
        // Resumen por titular. null = la persona no tiene compras (no es proveedor): no se muestra el bloque.
        public Entidades.ResumenProveedor Resumen { get; set; }
        // Bloque "Control fiscal" (facturado vs pagado bancarizado/efectivo): solo para quien puede editar Pagos del menu (ver
        // FinanzasController.ArmarTitularesCtaCte). Sin permiso, esos numeros ni siquiera se calculan.
        public bool PuedeVerControlFiscal { get; set; }
        // Fecha de la primera compra de la persona: atajo "Desde el inicio" del filtro.
        public DateTime? PrimeraCompra { get; set; }
    }
}
