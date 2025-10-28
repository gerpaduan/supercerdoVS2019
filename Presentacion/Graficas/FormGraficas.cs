using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Globalization;

namespace Presentacion.Graficas
{
    public partial class FormGraficas : Form
    {
        public DataTable dtVentasDiarias;
        public DateTime fechaDesde, fechaHasta;
        public string sucursal, vendedor, cliente, descripcion, seleccionadosFormaPago, seleccionadosTipoComprobante, SeleccionadosCondVenta;
        bool formCargado = false;
        public FormGraficas()
        {
            InitializeComponent();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {

        }

        private void FormGraficas_Load(object sender, EventArgs e)
        {
            CargarComboDias();
            txtFecha.Text = fechaDesde.ToString() + " - " + fechaHasta.ToString();
            txtSucursal.Text = sucursal;
            txtCliente.Text = cliente;
            txtVendedor.Text = vendedor;
            txtDescripcion.Text = descripcion;
            txtFormaPago.Text = seleccionadosFormaPago;
            txtTipoComprobantes.Text = seleccionadosTipoComprobante;
            txtCondVenta.Text = SeleccionadosCondVenta;

            formCargado = true;
        }

        // Método para cargar el ComboBox
        private void CargarComboDias()
        {
            if (!formCargado)
                return;

            comboBoxDias.Items.Clear();

            // agregamos 'Todos' primero
            comboBoxDias.Items.Add("Todos los días");

            // obtenemos los días de la semana localizados
            string[] dias = CultureInfo.CurrentCulture.DateTimeFormat.DayNames; // empieza domingo
                                                                                // mover domingo al final para que inicie lunes
            string[] diasOrdenados = dias.Skip(1).Concat(dias.Take(1)).ToArray();

            comboBoxDias.Items.AddRange(diasOrdenados);

            comboBoxDias.SelectedIndex = 0; // seleccionamos 'Todos' por defecto
        }

        // Evento SelectedIndexChanged para convertir a inglés
        private void comboBoxDias_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxDias.SelectedItem == null) return;

            string seleccionado = comboBoxDias.SelectedItem.ToString();

            if (seleccionado == "Todos los días")
            {
                CargarVentasPorHora(null);
                return;
            }

            // buscamos el índice del día en español
            string[] diasEsp = CultureInfo.CurrentCulture.DateTimeFormat.DayNames;
            string[] diasOrdenados = diasEsp.Skip(1).Concat(diasEsp.Take(1)).ToArray();

            int index = Array.IndexOf(diasOrdenados, seleccionado);

            if (index >= 0)
            {
                // obtenemos el nombre en inglés
                string diaEnIngles = CultureInfo.InvariantCulture.DateTimeFormat.DayNames[index].Substring(0); // placeholder
                                                                                                               // mejor: usamos DayOfWeek
                DayOfWeek diaSemana = (DayOfWeek)((index + 1) % 7); // domingo = 0
                CargarVentasPorHora(diaSemana);
            }
        }

        public void CargarFormaPago(string tipo)
        {
            //por cantidad 
            var FormasPagoPorCantidad = dtVentasDiarias.AsEnumerable()
                .GroupBy(r => r["formaPago"].ToString())
                .Select(g => new
                {
                    Forma = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            //por monto
            var FormasPagoPorMonto = dtVentasDiarias.AsEnumerable()
                .GroupBy(r => r["formaPago"].ToString())
                .Select(g => new
                {
                    Forma = g.Key,
                    Monto = g.Sum(x => Convert.ToDecimal(x["totalS"]))
                })
                .OrderByDescending(x => x.Monto)
                .ToList();


            chartVentasDiarias.Series.Clear();
            var serie = new Series();

            if (tipo == "cantidad")
            {
                serie.Name = "Cantidad por forma de pago";
                serie.ChartType = SeriesChartType.Pie;
                foreach (var item in FormasPagoPorCantidad)
                    serie.Points.AddXY(item.Forma, item.Cantidad);
            }
            else if (tipo == "monto")
            {
                serie.Name = "Monto por forma de pago";
                serie.ChartType = SeriesChartType.Pie;
                foreach (var item in FormasPagoPorMonto)
                    serie.Points.AddXY(item.Forma, item.Monto);
            }

            serie.IsValueShownAsLabel = true;
            chartVentasDiarias.Series.Add(serie);
        }

        public void CargarVentasPorHora(DayOfWeek? diaDeSemana)
        {
            // Filtrar solo sábados
            var diaSemanaSelected = dtVentasDiarias.AsEnumerable();
            //.Where(r => ((DateTime)r["fechaVenta"]).DayOfWeek == DayOfWeek.Saturday);

            //validar si se eligio un dia de la semana

            if (diaDeSemana.HasValue)
            {
                // Filtrar solo sábados
                diaSemanaSelected = dtVentasDiarias.AsEnumerable()
                    .Where(r => ((DateTime)r["fechaVenta"]).DayOfWeek == diaDeSemana);
            }

            // Agrupar en rangos de 15 minutos
            var grupos = diaSemanaSelected
                .GroupBy(r =>
                {
                    DateTime fecha = (DateTime)r["fechaVenta"];
                    int minutos = (fecha.Hour * 60) + fecha.Minute;
                    int bloque = minutos / 15; // cada bloque representa 15 min
                    int hora = bloque / 4;
                    int minInicio = (bloque % 4) * 15;
                    return new TimeSpan(hora, minInicio, 0);
                })
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Hora = g.Key,
                    //$"{g.Key:hh\\:mm}",
                    Cantidad = g.Count()
                })
                .ToList();

            //// --- Crear todos los intervalos posibles del día (00:00 a 23:45)
            //var todosLosBloques = Enumerable.Range(0, 24 * 4) // 24h * 4 bloques/hora = 96 intervalos
            //    .Select(i => new TimeSpan(0, i * 15, 0))
            //    .ToList();

            //// --- Unir los intervalos reales con los posibles (rellenar con 0 si no hay datos)
            //var gruposCompletos = todosLosBloques
            //    .Select(b =>
            //    {
            //        var existente = grupos.FirstOrDefault(g => g.Hora == b);
            //        return new
            //        {
            //            Hora = $"{b:hh\\:mm}",
            //            Cantidad = existente?.Cantidad ?? 0
            //        };
            //    })
            //    .ToList();
            // --- Crear todos los intervalos posibles del día (00:00 a 23:45)
            var todosLosBloques = Enumerable.Range(0, 24 * 4)
                .Select(i => new TimeSpan(0, i * 15, 0))
                .ToList();

            // --- Unir los intervalos reales con los posibles (rellenar con 0 si no hay datos)
            var gruposCompletos = todosLosBloques
                .Select(b =>
                {
                    var existente = grupos.FirstOrDefault(g => g.Hora == b);
                    return new
                    {
                        HoraTS = b,
                        Hora = $"{b:hh\\:mm}",
                        Cantidad = existente?.Cantidad ?? 0
                    };
                })
                .ToList();


            // Configurar gráfico
            chartVentasDiarias.Series.Clear();
            chartVentasDiarias.ChartAreas.Clear();
            chartVentasDiarias.ChartAreas.Add("Area1");


            var serie = new Series("Clientes por horario (" + comboBoxDias.Text + ")");
            serie.ChartType = SeriesChartType.Line;
            serie.IsValueShownAsLabel = true;

            //foreach (var item in grupos) 
            //foreach (var item in gruposCompletos)
            //{
            //    serie.Points.AddXY(item.Hora, item.Cantidad);
            //}

            // --- Detectar tramos largos (más de 12 bloques sin clientes)
            int contadorCeros = 0;
            bool enBloqueCeroLargo = false;

            foreach (var item in gruposCompletos)
            {
                if (item.Cantidad == 0)
                {
                    contadorCeros++;
                    if (contadorCeros > 4 && !enBloqueCeroLargo)
                    {
                        // Insertar un punto vacío (corte visual)
                        serie.Points.AddXY(item.Hora, double.NaN);
                        enBloqueCeroLargo = true;
                    }

                    // Mientras esté dentro del bloque largo, no agregamos puntos
                    if (!enBloqueCeroLargo)
                        serie.Points.AddXY(item.Hora, 0);
                }
                else
                {
                    // Si veníamos de un tramo largo de ceros, insertamos otro punto vacío para cortar
                    if (enBloqueCeroLargo)
                    {
                        serie.Points.AddXY(item.Hora, double.NaN);
                        enBloqueCeroLargo = false;
                    }

                    serie.Points.AddXY(item.Hora, item.Cantidad);
                    contadorCeros = 0;
                }
            }


            chartVentasDiarias.Series.Add(serie);
            chartVentasDiarias.ChartAreas[0].AxisX.Title = "Horario (rangos de 15 min)";
            chartVentasDiarias.ChartAreas[0].AxisY.Title = "Cantidad de clientes";
            chartVentasDiarias.ChartAreas[0].AxisX.Interval = 2; // mostrar cada 1h aprox
            chartVentasDiarias.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            chartVentasDiarias.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.WhiteSmoke;
            chartVentasDiarias.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.WhiteSmoke;

            return;
            //chartVentasDiarias.Series.Clear();
            //chartVentasDiarias.ChartAreas.Clear();

            //chartVentasDiarias.ChartAreas.Add("Area1");

            //var serie = new Series("Ventas Diarias");
            //serie.ChartType = SeriesChartType.Line;
            //serie.BorderWidth = 3;
            //serie.IsValueShownAsLabel = true;

            ////        var dt = BD.Consultar(@"
            ////    SELECT CONVERT(varchar(5), fecha, 103) AS Dia,
            ////           SUM(importe) AS Total
            ////    FROM Ventas
            ////    WHERE fecha >= DATEADD(day, -7, GETDATE())
            ////    GROUP BY CONVERT(varchar(5), fecha, 103)
            ////    ORDER BY MIN(fecha)
            ////");

            //dtVentasDiarias.Columns.Add("horaVenta", typeof(string));
            //foreach (DataRow r in dtVentasDiarias.Rows)
            //{
            //    DateTime fecha = Convert.ToDateTime(r["fechaVenta"]);
            //    r["horaVenta"] = fecha.ToString("HH:mm");
            //    serie.Points.AddXY(r["horaVenta"], r["totalKg"]);
            //}

            //chartVentasDiarias.Series.Add(serie);
        }
    }
}
