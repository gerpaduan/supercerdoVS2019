using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Compras
{
    public partial class formPorcentajeCortesCompra : Form
    {
        Negocio.Compra oCompraN = new Negocio.Compra();
        public formPorcentajeCortesCompra(int idCompra)
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
            cargarGrilla(idCompra);
        }

        private void cargarGrilla(int idCompra)
        {
            grillaPorcentajePorCorte.DataSource = null;

            DataTable tabla = oCompraN.porcentajeCortesPorCompra(idCompra);
            // ⚙️ Agregamos la fila de totales
            DataRow filaTotal = tabla.NewRow();
            filaTotal["Corte"] = "TOTAL GENERAL";
            filaTotal["Sucursal"] = tabla.Rows.Count > 0 ? tabla.Rows[0]["Sucursal"] : "";

            // Sumar todas las columnas numéricas
            filaTotal["Cantidad Kgs"] = tabla.AsEnumerable()
                .Sum(r => Convert.ToDecimal(r["Cantidad Kgs"] == DBNull.Value ? 0 : r["Cantidad Kgs"]));

            filaTotal["Stock Min"] = tabla.AsEnumerable()
                .Sum(r => Convert.ToDecimal(r["Stock Min"] == DBNull.Value ? 0 : r["Stock Min"]));

            filaTotal["Stock Max"] = tabla.AsEnumerable()
                .Sum(r => Convert.ToDecimal(r["Stock Max"] == DBNull.Value ? 0 : r["Stock Max"]));

            filaTotal["MontoVtaProy"] = tabla.AsEnumerable()
                .Sum(r => Convert.ToDecimal(r["MontoVtaProy"] == DBNull.Value ? 0 : r["MontoVtaProy"]));

            // Las columnas no sumables (precio, remarque) las dejamos vacías o nulas
            filaTotal["PrecioPromedio"] = DBNull.Value;
            filaTotal["precioKg"] = DBNull.Value;
            filaTotal["% Remarque"] = DBNull.Value;

            tabla.Rows.Add(filaTotal);


            grillaPorcentajePorCorte.DataSource = tabla;

        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        
    }
}
