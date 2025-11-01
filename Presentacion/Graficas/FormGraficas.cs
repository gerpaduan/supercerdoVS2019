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

        public enum TipoGrafico
        {
            VentasPorHora,
            CantidadDeVentas,
            MontoDeVentas
        }

        public TipoGrafico tipoGrafico;

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
            if (comboBoxDias.SelectedItem == null || !formCargado) return;

            string seleccionado = comboBoxDias.SelectedItem.ToString();

            DayOfWeek? diaSemana = null;

            // buscamos el índice del día en español
            string[] diasEsp = CultureInfo.CurrentCulture.DateTimeFormat.DayNames;
            string[] diasOrdenados = diasEsp.Skip(1).Concat(diasEsp.Take(1)).ToArray();

            int index = Array.IndexOf(diasOrdenados, seleccionado);

            if (index >= 0)
            {
                // obtenemos el nombre en inglés
                string diaEnIngles = CultureInfo.InvariantCulture.DateTimeFormat.DayNames[index].Substring(0); // placeholder
                                                                                                               // mejor: usamos DayOfWeek
                diaSemana = (DayOfWeek)((index + 1) % 7); // domingo = 0  
            }

            if (tipoGrafico == TipoGrafico.VentasPorHora)
            {
                CargarVentasPorHora(diaSemana);
            }
            else
            {
                CargarFormaPago(diaSemana);
            }
        }

        public void CargarFormaPago(DayOfWeek? diaDeSemana)
        {
            // Filtrar solo sábados
            var diaSemanaSelected = dtVentasDiarias.AsEnumerable();

            //validar si se eligio un dia de la semana
            if (diaDeSemana.HasValue)
            {
                // Filtrar solo sábados
                diaSemanaSelected = dtVentasDiarias.AsEnumerable()
                    .Where(r => ((DateTime)r["fechaVenta"]).DayOfWeek == diaDeSemana);
            }

            //por cantidad 
            var FormasPagoPorCantidad = diaSemanaSelected
                .GroupBy(r => r["formaPago"].ToString())
                .Select(g => new
                {
                    Forma = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            //por monto
            var FormasPagoPorMonto = diaSemanaSelected
                .GroupBy(r => r["formaPago"].ToString())
                .Select(g => new
                {
                    Forma = g.Key,
                    Monto = g.Sum(x => Convert.ToDecimal(x["totalS"]))
                })
                .OrderByDescending(x => x.Monto)
                .ToList();

            chartVentasDiarias.Series.Clear();
            chartVentasDiarias.Titles.Clear(); // Limpia títulos anteriores
            var serie = new Series();

            if (tipoGrafico == TipoGrafico.CantidadDeVentas)// "cantidad")
            {
                serie.Name = "Cantidad ventas según forma de pago";
                serie.ChartType = SeriesChartType.Pie;
                foreach (var item in FormasPagoPorCantidad)
                    serie.Points.AddXY(item.Forma, item.Cantidad);

            }
            else if (tipoGrafico == TipoGrafico.MontoDeVentas)// "monto")
            {
                serie.Name = "Monto ventas según forma de pago";
                serie.ChartType = SeriesChartType.Pie;
                //serie.LabelFormat = "N2"; // <-- formato numérico
                serie.LabelForeColor = Color.Black;
                serie.Font = new Font("Segoe UI", 9, FontStyle.Regular);
                foreach (var item in FormasPagoPorMonto)
                //serie.Points.AddXY(item.Forma, item.Monto);
                {
                    int idx = serie.Points.AddXY(item.Forma, item.Monto);
                    var punto = serie.Points[idx];
                    // Muestra: "Efectivo: $12.345,67 (40%)"
                    punto.Label = $"{item.Forma}: ${item.Monto:N2} (#PERCENT{{P0}})";
                }
            }

            // 👇 Mostrar cantidad y porcentaje
            //serie.Label = "#VALX\n#VAL (#PERCENT{P0})";
            serie.Label = (tipoGrafico == TipoGrafico.CantidadDeVentas) ? "#VALX: #PERCENT{P1} (#VALY{N0})" : "#VALX: #PERCENT{P1} (#VALY{N2})";
            serie.LegendText = "#VALX (#PERCENT{P0})";
            serie["PieLabelStyle"] = "Outside";
            serie["PieLineColor"] = "Gray";

            serie.IsValueShownAsLabel = true;
            chartVentasDiarias.Series.Add(serie);
            
            // 🔹 Agregar título visible arriba del gráfico
            var titulo = new Title
            {
                Text = serie.Name,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Black,
                Alignment = ContentAlignment.TopCenter
            };
            chartVentasDiarias.Titles.Add(titulo);
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
        }
    }
}
