using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Cortes;
using Presentacion.Reportes;

namespace Presentacion.Movimientos
{
    public partial class formInfoMovimiento : Form
    {
        formMovimientos frmMovimiento;

        Entidades.Movimiento oMovimientoE = new Entidades.Movimiento();

        List<Entidades.CortePorMovimiento> listaCortesPorMovimiento = new List<Entidades.CortePorMovimiento>();

        CortesPorMovimiento cortePorMovimiento;
        List<CortesPorMovimiento> listaEnGrilla;

        Negocio.Corte oCorteN = new Negocio.Corte();

        public formInfoMovimiento()
        {
            InitializeComponent();

        }

        public void obtenerParametros(formMovimientos frmMovimientosParam, Entidades.Movimiento movimientoParam)
        {
            frmMovimiento = frmMovimientosParam;
            oMovimientoE = movimientoParam;

            cargarCampos();
        }

        private void cargarCampos()
        {
            lblIdOrigen.Text = oMovimientoE.IdMovOrigen != null && oMovimientoE.IdMovOrigen > 0 ? 
                oMovimientoE.IdMovOrigen.ToString() : oMovimientoE.IdMovimiento.ToString();
            lblIdDestino.Text = oMovimientoE.IdMovOrigen != null && oMovimientoE.IdMovOrigen > 0 ?
                oMovimientoE.IdMovimiento.ToString() : "-";

            txtSucOrigen.Text = oMovimientoE.SucursalOrigen.sucursal;
            txtSucDestino.Text = oMovimientoE.SucursalDestino.sucursal;
            txtFechaMovimiento.Value = oMovimientoE.FechaMovimiento;
            txtHoraMovimiento.Text = oMovimientoE.FechaMovimiento.TimeOfDay.ToString();
            txtObservaciones.Text = oMovimientoE.Observaciones;
            string datosCreado = "Creado: " + oMovimientoE.Creado.ToString() + "\tModificado: " +
                (oMovimientoE.Actualizado > DateTime.Today.AddYears(-20) ? oMovimientoE.Actualizado.ToString() : "-");
            txtCreado.Text = datosCreado;

            cargarListaCortesPorMovimiento();
        }

        private void cargarListaCortesPorMovimiento()
        {
            listaCortesPorMovimiento= oCorteN.cargarCortesPorMovimiento(oMovimientoE.IdMovimiento);
            cargarGrilla();        
        }

        private void cargarListaEnGrilla()
        {
            listaEnGrilla = new List<CortesPorMovimiento>();

            foreach (Entidades.CortePorMovimiento lineaCorte in listaCortesPorMovimiento)
            {
                cortePorMovimiento = new CortesPorMovimiento();

                cortePorMovimiento.IdCortePorMovimiento = lineaCorte.IdCorteMovimiento;
                cortePorMovimiento.IdCorte = lineaCorte.Corte.idCorte;
                cortePorMovimiento.Codigo = lineaCorte.Corte.codigo;
                cortePorMovimiento.Corte = lineaCorte.Corte.corte;
                cortePorMovimiento.CantUnidad = lineaCorte.CantUnidad;
                cortePorMovimiento.CantKg = lineaCorte.CantKg;
                cortePorMovimiento.PesoBalanza = lineaCorte.PesoBalanza;

                listaEnGrilla.Add(cortePorMovimiento);
            }

        }

        public void cargarGrilla()
        {
            cargarListaEnGrilla();

            grillaCortesPorMovimiento.DataSource = null;
            grillaCortesPorMovimiento.AutoGenerateColumns = false;

            grillaCortesPorMovimiento.DataSource = listaEnGrilla;

            cargarTotales();
        }

        private void cargarTotales()
        {
            float totalKg = 0;
            foreach (Entidades.CortePorMovimiento filaCorte in listaCortesPorMovimiento)
            {
                totalKg += filaCorte.CantKg;
            }

            txtCantItems.Text = Convert.ToString(grillaCortesPorMovimiento.Rows.Count);
            txtTotalKg.Text = Convert.ToString(totalKg);

        }



        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["formNuevoMovimiento"] != null)
            {
                Application.OpenForms["formNuevoMovimiento"].Activate();
            }
            else
            {

                if (Utilidades.Util_Form.validarSucursal(FormPrincipal.logueado, oMovimientoE.SucursalOrigen.idSucursal) &&
                    Utilidades.Util_Form.validarPermisoModif(Presentacion.FormPrincipal.logueado, oMovimientoE.FechaMovimiento))
                {
                    formNuevoMovimiento frmNuevoMovimiento = new formNuevoMovimiento();
                    frmNuevoMovimiento.obtenerParametros(frmMovimiento, oMovimientoE, listaCortesPorMovimiento);
                    this.Close();
                    frmNuevoMovimiento.Show();
                }
            }
        }

        private void cargarReporte()
        {
            int tipoReporte = 5;//nro perteneciente al reporte de los movimientos
            formReporteStock frmReporte = new formReporteStock();
            frmReporte.obtenerParametros(oMovimientoE.SucursalDestino.idSucursal, oMovimientoE.FechaMovimiento, oMovimientoE.FechaMovimiento, tipoReporte, oMovimientoE.IdMovimiento.ToString());
            frmReporte.Show();        
        }

        private void Reporte_Click(object sender, EventArgs e)
        {
            cargarReporte();
        }

        private void formInfoMovimiento_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();

        }

        private void Imprimir_Click(object sender, EventArgs e)
        {
            try
            {
                string titulo = "Movimiento";
                FormReportes frmReportes;

                DialogResult resp = MessageBox.Show("¿Emitir Reporte con el Total Acumulado por cada Corte?","",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question,MessageBoxDefaultButton.Button3);

                if (resp != DialogResult.Cancel)
                {
                    if (resp == DialogResult.Yes)
                    {
                        titulo = "Movimiento Acum";
                        Reportes.ReporteMovimientoAcum reporte = new Reportes.ReporteMovimientoAcum();
                        frmReportes = new FormReportes(reporte, titulo, null, oMovimientoE.FechaMovimiento, oMovimientoE.FechaMovimiento);

                    }
                    else
                    {
                        Reportes.ReporteMovimiento reporte = new Reportes.ReporteMovimiento();
                        frmReportes = new FormReportes(reporte, titulo, null, oMovimientoE.FechaMovimiento, oMovimientoE.FechaMovimiento);

                    }
                    frmReportes.ListaCortesPorMov = listaEnGrilla;
                    frmReportes.Objetos = true;
                    frmReportes.ReporteMovimiento = true;
                    frmReportes.Origen = oMovimientoE.SucursalOrigen.SucursalNombre;
                    frmReportes.Destino = oMovimientoE.SucursalDestino.SucursalNombre;

                    frmReportes.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void eliminar_Click(object sender, EventArgs e)
        {
            if (Utilidades.Util_Form.validarSucursal(FormPrincipal.logueado, oMovimientoE.SucursalOrigen.idSucursal) &&
                    Utilidades.Util_Form.validarPermisoModif(Presentacion.FormPrincipal.logueado, oMovimientoE.FechaMovimiento) &&
                    MessageBox.Show("¿Está seguro que desea eliminar el movimiento?", "Eliminar Movimiento", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2).Equals(DialogResult.Yes))
            {
                oCorteN.eliminarMovimiento(oMovimientoE.IdMovimiento);
                pnlEliminado.Visible = true;
                pnlEliminado.BringToFront();
                frmMovimiento.cargarGrilla();
                MessageBox.Show("El Movimiento se eliminó correctamente!");
            }
        }        
    }
}
