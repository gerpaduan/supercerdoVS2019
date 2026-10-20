// Un pedido "en cruz": una fila por producto y una columna por sucursal (lo que se ve en el detalle y lo que imprimen el PDF, el Excel y el
// ticket). Las lineas guardadas son un renglon por producto y sucursal; aca se juntan. Sin acceso a base de datos ni efectos secundarios.
using System;
using System.Collections.Generic;
using System.Linq;
using Entidades;

namespace WebCore.Services
{
    public sealed class PedidoPivotSucursal
    {
        public int IdSucursal;
        public string Nombre = "";
    }

    public sealed class PedidoPivotFila
    {
        public int IdCorte;
        public long Codigo;
        public string Producto = "";
        public bool Pesable;
        // Kilos por unidad (solo pesables), para el equivalente en unidades.
        public decimal? Promedio;
        // Cantidad por sucursal, en el MISMO orden que PedidoPivotVm.Sucursales; null = esa sucursal no pidio este producto.
        public List<decimal?> Cantidades = new List<decimal?>();
        public decimal Total;
        // Ultimo precio de compra guardado (el de la primera linea que lo tiene); null si ninguna linea tenia precio.
        public decimal? Precio;
        public string Proveedor;
        // Fecha de la compra que dio ese precio (null si no se guardo) y cuantos dias antes del pedido fue.
        public DateTime? FechaPrecio;
        public int? DiasDelPrecio;
        // true si esa compra es de hace mas dias que el limite configurado (Pedidos:DiasPrecioViejo): el precio puede estar desactualizado.
        public bool PrecioViejo;
        // Suma de los importes de las lineas con precio; null si ninguna lo tenia.
        public decimal? Importe;
        // true si alguna linea de este producto no tenia precio (su cantidad no suma al importe).
        public bool AlgunaSinPrecio;
    }

    public sealed class PedidoPivotVm
    {
        public List<PedidoPivotSucursal> Sucursales = new List<PedidoPivotSucursal>();
        public List<PedidoPivotFila> Filas = new List<PedidoPivotFila>();
        public decimal TotalEstimado;
        public int LineasSinPrecio;
        public int Lineas;
        // Limite (dias) con el que se marco PrecioViejo, y si alguna fila quedo marcada (para mostrar la leyenda solo cuando hace falta).
        public int DiasPrecioViejo;
        public bool HayPreciosViejos;
    }

    public static class PedidoPivot
    {
        // diasPrecioViejo: limite de antiguedad del precio; null = el configurado (Pedidos:DiasPrecioViejo). La antiguedad se mide contra la
        // FECHA DEL PEDIDO (no contra hoy): lo que importa es cuan viejo era el precio cuando se hizo el pedido.
        public static PedidoPivotVm Construir(Pedido pedido, int? diasPrecioViejo = null)
        {
            var vm = new PedidoPivotVm { DiasPrecioViejo = diasPrecioViejo ?? WebCore.Helpers.PedidoPrecioSettings.DiasPrecioViejo };
            if (pedido == null) return vm;

            var lineas = pedido.Lineas ?? new List<PedidoLinea>();
            vm.Lineas = lineas.Count;
            vm.TotalEstimado = pedido.TotalEstimado;
            vm.LineasSinPrecio = pedido.LineasSinPrecio;

            vm.Sucursales = lineas
                .GroupBy(l => l.IdSucursal)
                .Select(g => new PedidoPivotSucursal { IdSucursal = g.Key, Nombre = g.First().Sucursal ?? "" })
                .OrderBy(s => s.Nombre, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(s => s.IdSucursal)
                .ToList();

            foreach (var grupo in lineas.GroupBy(l => l.IdCorte).OrderBy(g => g.First().Codigo).ThenBy(g => g.First().Producto, StringComparer.CurrentCultureIgnoreCase))
            {
                var primera = grupo.First();
                var fila = new PedidoPivotFila
                {
                    IdCorte = grupo.Key,
                    Codigo = primera.Codigo,
                    Producto = primera.Producto ?? "",
                    Pesable = primera.Pesable,
                    Promedio = primera.Promedio
                };

                foreach (var sucursal in vm.Sucursales)
                {
                    var linea = grupo.FirstOrDefault(l => l.IdSucursal == sucursal.IdSucursal);
                    fila.Cantidades.Add(linea != null ? (decimal?)linea.Cantidad : null);
                }

                fila.Total = grupo.Sum(l => l.Cantidad);
                var conPrecio = grupo.FirstOrDefault(l => l.PrecioUnitario.HasValue);
                if (conPrecio != null)
                {
                    fila.Precio = conPrecio.PrecioUnitario;
                    fila.Proveedor = conPrecio.Proveedor;
                    fila.FechaPrecio = conPrecio.FechaPrecio;
                    fila.DiasDelPrecio = Negocio.PedidoTextos.DiasDelPrecio(conPrecio.FechaPrecio, pedido.Fecha);
                    fila.PrecioViejo = Negocio.PedidoTextos.PrecioViejo(conPrecio.FechaPrecio, pedido.Fecha, vm.DiasPrecioViejo);
                    if (fila.PrecioViejo) vm.HayPreciosViejos = true;
                }
                fila.Importe = grupo.Any(l => l.Importe.HasValue) ? (decimal?)grupo.Where(l => l.Importe.HasValue).Sum(l => l.Importe.Value) : null;
                fila.AlgunaSinPrecio = grupo.Any(l => !l.Importe.HasValue);
                vm.Filas.Add(fila);
            }

            return vm;
        }
    }
}
