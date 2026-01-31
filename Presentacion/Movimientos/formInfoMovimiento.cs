using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Cortes;

using Utilidades;

namespace Presentacion.Movimientos
{
    public partial class formInfoMovimiento : Form, InterfaceUsuario
    {
        public int idMovimiento;
        public formMovimientos frmMovimiento;

        Entidades.Movimiento oMovimientoE = new Entidades.Movimiento();
        public Entidades.Usuario oUsuario;

        List<Entidades.CortePorMovimiento> listaCortesPorMovimiento = new List<Entidades.CortePorMovimiento>();

        CortesPorMovimiento cortePorMovimiento;
        List<CortesPorMovimiento> listaEnGrilla;

        Negocio.Corte oCorteN = new Negocio.Corte(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
        Negocio.Usuario oUsuarioN = new Negocio.Usuario(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);

        public formInfoMovimiento()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;

        }

        private void formInfoMovimiento_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text += Utilidades.Conexion.getSucursalConexion();
                oMovimientoE = oCorteN.cargarMovimiento(idMovimiento, false);
                cargarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el movimiento.\n\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void cargarCampos()
        {
            idMovimientoLabel.Text = oMovimientoE.IdMovimiento.ToString();

            lblIdOrigen.Text = oMovimientoE.IdMovOrigen != null && oMovimientoE.IdMovOrigen > 0 ? 
                oMovimientoE.IdMovOrigen.ToString() : oMovimientoE.IdMovimiento.ToString();
            lblIdDestino.Text = oMovimientoE.IdMovOrigen != null && oMovimientoE.IdMovOrigen > 0 ?
                oMovimientoE.IdMovimiento.ToString() : "-";

            txtSucOrigen.Text = oMovimientoE.SucursalOrigen.sucursal;
            txtSucDestino.Text = oMovimientoE.SucursalDestino.sucursal;
            txtFechaMovimiento.Text = Utilidades.Util_Form.fechaFormato24Horas(oMovimientoE.FechaMovimiento);
            txtObservaciones.Text = oMovimientoE.Observaciones;

            txtCreado.Text = Util_Form.fechaFormato24Horas(oMovimientoE.Creado);
            txtCreadoPor.Text = oMovimientoE.CreadoPor != null ? oMovimientoE.CreadoPor.Nombre : "-";
            txtActualizado.Text = oMovimientoE.Actualizado != null ? Util_Form.fechaFormato24Horas(oMovimientoE.Actualizado) : "-";
            txtActualizadoPor.Text = oMovimientoE.ActualizadoPor != null ? oMovimientoE.ActualizadoPor.Nombre : "-";

            cargarListaCortesPorMovimiento();
        }

        private void cargarListaCortesPorMovimiento()
        {
            listaCortesPorMovimiento= oCorteN.cargarCortesPorMovimiento(oMovimientoE.IdMovimiento, false);
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
                cortePorMovimiento.PermitirIngreso = lineaCorte.PermitirIngreso;

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
            txtTotalKg.Text = totalKg.ToString("F3");

        }



        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            try
            {
                int idMovimiento = oMovimientoE.IdMovimiento;
                bool formAbierto = false;
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.GetType() == typeof(formNuevoMovimiento))
                    {
                        foreach (Control ctrl in frm.Controls)
                        {
                            if (ctrl.Name.Equals("idMovimientoLabel") && ctrl.Text.Equals(idMovimiento.ToString()))
                            {
                                frm.BringToFront();
                                formAbierto = true;
                                this.Close();
                                break;
                            }
                        }
                    }
                    if (formAbierto) break;
                }
                if (!formAbierto)
                {
                    formNuevoMovimiento frmNuevoMovimiento = new formNuevoMovimiento();
                    frmNuevoMovimiento.obtenerParametros(frmMovimiento, oMovimientoE, listaCortesPorMovimiento);
                    this.Close();
                    frmNuevoMovimiento.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

        private void Imprimir_Click(object sender, EventArgs e)
        {
            Ticket.formTipoTicket tipoTicket = new Presentacion.Ticket.formTipoTicket();
            tipoTicket.movimientoAcumulado(oMovimientoE.IdMovimiento);

            #region imprimir desde reportes
            //string titulo = "Movimiento";
            //FormReportes frmReportes;

            //DialogResult resp = MessageBox.Show("¿Emitir Reporte con el Total Acumulado por cada Producto?","",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question,MessageBoxDefaultButton.Button3);

            //if (resp != DialogResult.Cancel)
            //{
            //    if (resp == DialogResult.Yes)
            //    {
            //        titulo = "Movimiento Acum";
            //        Reportes.ReporteMovimientoAcum reporte = new Reportes.ReporteMovimientoAcum();
            //        frmReportes = new FormReportes(reporte, titulo, null, oMovimientoE.FechaMovimiento, oMovimientoE.FechaMovimiento);

            //    }
            //    else
            //    {
            //        Reportes.ReporteMovimiento reporte = new Reportes.ReporteMovimiento();
            //        frmReportes = new FormReportes(reporte, titulo, null, oMovimientoE.FechaMovimiento, oMovimientoE.FechaMovimiento);

            //    }
            //    frmReportes.ListaCortesPorMov = listaEnGrilla;
            //    frmReportes.Objetos = true;
            //    frmReportes.ReporteMovimiento = true;
            //    frmReportes.Origen = oMovimientoE.SucursalOrigen.SucursalNombre;
            //    frmReportes.Destino = oMovimientoE.SucursalDestino.SucursalNombre;

            //    frmReportes.Show();
            //}
            #endregion
        }

        private void eliminar_Click(object sender, EventArgs e)
        {
            if (Utilidades.Util_Form.validarSucursal(FormPrincipal.logueado, oMovimientoE.SucursalOrigen.idSucursal) &&
                    Utilidades.Util_Form.validarPermisoModif(Presentacion.FormPrincipal.logueado, oMovimientoE.FechaMovimiento) &&
                    MessageBox.Show("¿Está seguro que desea eliminar el movimiento?\n\nNota: Para eliminar deberá ingresar su usuario y contraseña", 
                    "Eliminar Movimiento", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2).Equals(DialogResult.Yes) && logueoUsuario())
            {
                oCorteN.eliminarMovimiento(oMovimientoE.IdMovimiento, oUsuario);
                pnlEliminado.Visible = true;
                pnlEliminado.BringToFront();
                frmMovimiento.cargarGrilla();
                MessageBox.Show("El Movimiento se eliminó correctamente!");
            }
            oUsuario = null;
        }

        private bool logueoUsuario()
        {
            this.BringToFront();
            Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
            frmLogin.soloActivos = true;
            frmLogin.usuarioConPermiso = true;
            frmLogin.soloAdmin = false;
            frmLogin.ShowDialog(this);
            return (oUsuario != null);
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void verAcum_Click(object sender, EventArgs e)
        {
            cargarListaEnGrilla();
            Movimientos.formVerAcumulados formVerAcum = new Presentacion.Movimientos.formVerAcumulados();
            formVerAcum.verAcumulados(listaEnGrilla, null, formVerAcumulados.tipoAcum.movimiento);// (listaCortesPorMovimiento);
            formVerAcum.ShowDialog();
        }         
    }
}
