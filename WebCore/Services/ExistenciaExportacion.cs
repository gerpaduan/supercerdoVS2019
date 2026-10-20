// Datos ya listos para exportar Existencia por sucursales a Excel (ExcelExistenciaSucursales) y a PDF (PdfExistenciaSucursales).
// Se arma a partir del mismo modelo que muestra la pantalla (ExistenciaPorSucursalesVm), en el ORDEN de los idCorte que manda
// el navegador: lo que se ve es lo que se exporta (filtros y orden de la pantalla). El estado de cada celda sale de
// Negocio.ExistenciaEstado, igual que en pantalla. Sin acceso a base de datos ni efectos secundarios.
using System;
using System.Collections.Generic;
using System.Linq;
using Entidades;
using Negocio;
using WebCore.Helpers;

namespace WebCore.Services
{
    public sealed class ExistenciaExportCelda
    {
        public double Stock;
        // Equivalente en unidades ("276,3 un."), solo pesables con promedio; null si no aplica.
        public string Unidades;
        public double Punto;
        public string EstadoTexto = "";
        public string EstadoCodigo = "";
    }

    public sealed class ExistenciaExportFila
    {
        public long Codigo;
        public string Producto = "";
        public List<ExistenciaExportCelda> Celdas = new List<ExistenciaExportCelda>();
        // Solo con 2 o mas sucursales.
        public ExistenciaExportCelda Total;
    }

    public sealed class ExistenciaExportVm
    {
        public string Titulo = "Existencia por sucursales";
        // Lineas informativas bajo el titulo (sucursales, fecha del stock, filtros aplicados).
        public List<string> Encabezado = new List<string>();
        public List<string> Sucursales = new List<string>();
        public bool MostrarTotal;
        public bool MostrarEstado;
        public List<ExistenciaExportFila> Filas = new List<ExistenciaExportFila>();
    }

    public static class ExistenciaExportacion
    {
        // Arma el modelo de exportacion. idsOrden: idCorte en el orden de la pantalla; los que ya no estan en el resultado se omiten.
        public static ExistenciaExportVm Construir(ExistenciaPorSucursalesVm model, IList<int> idsOrden, bool mostrarEstado, DateTime generado)
        {
            var vm = new ExistenciaExportVm
            {
                MostrarEstado = mostrarEstado,
                MostrarTotal = model.Columnas.Count > 1
            };
            vm.Sucursales = model.Columnas.Select(c => c.Sucursal).ToList();

            vm.Encabezado.Add("Sucursales: " + string.Join(", ", vm.Sucursales));
            vm.Encabezado.Add("Stock al " + generado.ToString("dd/MM/yyyy HH:mm"));
            var filtros = TextoFiltros(model.Filtro);
            if (filtros.Length > 0) vm.Encabezado.Add("Filtros: " + filtros);

            var porId = model.Productos.GroupBy(p => p.IdCorte).ToDictionary(g => g.Key, g => g.First());
            foreach (int id in idsOrden.Distinct())
            {
                ProductoStockPorSucursalVm producto;
                if (!porId.TryGetValue(id, out producto)) continue;

                var fila = new ExistenciaExportFila { Codigo = producto.Codigo, Producto = producto.Corte ?? "" };
                double totalStock = 0, totalPunto = 0;

                foreach (var columna in model.Columnas)
                {
                    var celda = producto.Celdas.FirstOrDefault(x => x.IdSucursal == columna.IdSucursal);
                    double stock = celda != null ? celda.StockActual : 0;
                    double punto = celda != null ? celda.PuntoStock : 0;
                    totalStock += stock;
                    totalPunto += punto;

                    fila.Celdas.Add(Celda(stock, punto, celda != null
                        ? StockUnidadesHelper.FormatearEquivalenteUnidades((decimal)celda.StockActual, (decimal)celda.Promedio, celda.Pesable)
                        : null));
                }

                if (vm.MostrarTotal) fila.Total = Celda(totalStock, totalPunto, null);
                vm.Filas.Add(fila);
            }

            return vm;
        }

        private static ExistenciaExportCelda Celda(double stock, double punto, string unidades)
        {
            var estado = ExistenciaEstado.Calcular(stock, punto);
            return new ExistenciaExportCelda
            {
                Stock = stock,
                Unidades = unidades,
                Punto = punto,
                EstadoTexto = ExistenciaEstadoTextos.TextoConPunto(estado),
                EstadoCodigo = estado.Codigo
            };
        }

        // Resumen de los filtros de producto aplicados (los de la barra que no viajan al servidor, como el de Estado, no estan aca).
        private static string TextoFiltros(ExistenciaStockPorSucursalFiltroVm f)
        {
            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(f.Texto)) partes.Add("texto \"" + f.Texto + "\"");
            if (f.Tipos.Count > 0) partes.Add("tipos: " + string.Join(", ", f.Tipos));
            if (f.IdsProveedores.Count > 0)
                partes.Add("proveedores: " + string.Join(", ", f.ProveedoresDisponibles.Where(p => f.IdsProveedores.Contains(p.IdPersona)).Select(p => p.RazonSocial)));
            if (f.IdsMarcas.Count > 0)
                partes.Add("marcas: " + string.Join(", ", f.MarcasDisponibles.Where(m => f.IdsMarcas.Contains(m.IdPersona)).Select(m => m.RazonSocial)));
            return string.Join(" · ", partes);
        }
    }
}
