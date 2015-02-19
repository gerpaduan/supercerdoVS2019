using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Cortes;

namespace Presentacion
{
    public partial class formNuevoMovimiento : formBaseColor, InterfaceCorte
    {
        formMovimientos frmMovimiento = new formMovimientos();

        DataTable dtSucursalOrigen=new DataTable();
        DataTable dtSucursalDestino = new DataTable();

        Entidades.Corte oCorteE = new Entidades.Corte();
        Entidades.Movimiento oMovimiento = new Entidades.Movimiento();
        Entidades.Sucursal oSucursalOrigen = new Entidades.Sucursal();
        Entidades.Sucursal oSucursalDestino = new Entidades.Sucursal();

        Entidades.CortePorMovimiento oCortePorMovimientoE;

        List<Entidades.CortePorMovimiento> listaCortesPorMovimiento = new List<Entidades.CortePorMovimiento>();

        CortesPorMovimiento cortePorMovimiento;
        List<CortesPorMovimiento> listaEnGrilla;

        Negocio.Corte oCorteN = new Negocio.Corte();
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        bool modificacion = false;

        public formNuevoMovimiento()
        {
            InitializeComponent();
            cargarSucursales();
        }

        public void obtenerParametros(formMovimientos frmMovimientoParam, Entidades.Movimiento movimientoParam, List<Entidades.CortePorMovimiento> listaCortesPorMovimientoParam)
        {
            modificacion = true;
            frmMovimiento = frmMovimientoParam;
            oMovimiento = movimientoParam;
            listaCortesPorMovimiento = listaCortesPorMovimientoParam;

            cargarCampos();
        }

        public void obtenerForm(formMovimientos frmMovimientoParam)
        {
            frmMovimiento = frmMovimientoParam;
        }

        public void cargarCampos()
        {
            this.Text = "Modificar Movimiento";

            comboSucOrigen.SelectedIndex = oMovimiento.SucursalOrigen.idSucursal-1;

            txtFechaMovimiento.Value = oMovimiento.FechaMovimiento;
            txtObservaciones.Text = oMovimiento.Observaciones;

            cargarGrilla();
        
        }


        public void agregarMovimiento()
        {
            if (validacionFinal())
            {
                cargarMovimiento();

                try
                {
                    if (modificacion)
                    {
                        oCorteN.quitarCortesPorMovimiento(oMovimiento);

                        oCorteN.modificarMovimiento(oMovimiento);
                    }

                    else
                    {
                        oMovimiento.IdMovimiento = oCorteN.agregarMovimiento(oMovimiento);
                    }

                    foreach (Entidades.CortePorMovimiento  corteEnLista in listaCortesPorMovimiento)
                    {
                        corteEnLista.Movimientos = oMovimiento;

                        oCorteN.agregarCortePorMovimiento(corteEnLista);
                    }


                    frmMovimiento.cargarGrilla();
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private bool validacionFinal()
        {
            if (modificacion)
            {
                DialogResult resp = MessageBox.Show("¿Está seguro que desea modificar los datos del Movimiento?", "Modificar Movimiento", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (resp == DialogResult.Yes)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                DialogResult resp = MessageBox.Show("Verifique si la Sucursal Origen - Destino y la fecha ingresada son correctas.\n¿Están correctas?", "Verificar Datos ingresados", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (resp == DialogResult.Yes)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        
        }

        private void cargarMovimiento()
        {
            oMovimiento.FechaMovimiento = txtFechaMovimiento.Value;

            //Se cargan sucursales y se asignan al movimiento
   
            oSucursalOrigen.idSucursal=Convert.ToInt32(comboSucOrigen.SelectedValue.ToString());
            oSucursalDestino.idSucursal=Convert.ToInt32(comboSucDestino.SelectedValue.ToString());

            oMovimiento.SucursalOrigen = oSucursalOrigen;
            oMovimiento.SucursalDestino = oSucursalDestino;

            oMovimiento.Observaciones = txtObservaciones.Text.Trim();

        }

        private bool validar()
        {
            bool resp = true;

            if (txtCorte.Text.Trim()=="")
            {
                txtCodigo.Focus();
                MessageBox.Show("No se hay ingresado ningún corte.", "Completar Campos", MessageBoxButtons.OK, MessageBoxIcon.Information);

                resp= false;
            }

            if (txtCantKgs.Text.Trim() == "")
            {
                txtCantKgs.Focus();
                MessageBox.Show("Ingrese la cantidad de Kgs.", "Completar Campos", MessageBoxButtons.OK, MessageBoxIcon.Information);

                resp = false;
            }


            return resp;
        }

        private void cargarCorte()
        {
            try
            {
                if (txtCodigo.Text.Trim() !="")
                {
                    oCorteE = null;
                    oCorteE = new Entidades.Corte();

                    DataTable dtCorte = new DataTable();

                    dtCorte = oCorteN.buscarCodigoCorte(Convert.ToInt32(txtCodigo.Text.Trim()));

                    if (dtCorte.Rows.Count > 0)
                    {
                        foreach (DataRow fila in dtCorte.Rows)
                        {
                            oCorteE.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                            oCorteE.codigo = Convert.ToInt32(fila["codigo"].ToString());
                            oCorteE.corte = fila["corte"].ToString();
                        }

                        //se cargan los datos del corte
                        txtCorte.Text = oCorteE.corte;
                    }
                    else
                    {
                        txtCorte.Text = "";
                        MessageBox.Show("El código no existe");
                        txtCodigo.Focus();
                    }
                    
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        
        }

        private void cargarCortePorMovimiento()
        {
            if (validar())
            {
                try
                {
                    oCortePorMovimientoE = new Entidades.CortePorMovimiento();
                    oCortePorMovimientoE.Corte = oCorteE;

                    try
                    {
                        oCortePorMovimientoE.CantKg = float.Parse(txtCantKgs.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                    }
                    catch (Exception)
                    {

                        oCortePorMovimientoE.CantKg = float.Parse(txtCantKgs.Text.Trim());
                    }

                    listaCortesPorMovimiento.Add(oCortePorMovimientoE);
                    cargarGrilla();
                                       
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                limpiarCampos();
  
            }
  
        }

        private void limpiarCampos()
        {
            if (checkMantenerCodigo.Checked.Equals(false))
            {
                txtCodigo.Text = "";
                txtCorte.Text = "";
            }

            txtCantKgs.Text = "";
        
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
                cortePorMovimiento.CantKg = lineaCorte.CantKg;

                listaEnGrilla.Add(cortePorMovimiento);
            }
        
        }

        public void cargarGrilla()
        {
            cargarListaEnGrilla();

            grillaCortesPorMovimiento.DataSource = null;
            grillaCortesPorMovimiento.AutoGenerateColumns = false;

            grillaCortesPorMovimiento.DataSource = listaEnGrilla;

            grillaCortesPorMovimiento.Rows[listaCortesPorMovimiento.Count() - 1].Selected = true;
            grillaCortesPorMovimiento.FirstDisplayedScrollingRowIndex = listaCortesPorMovimiento.Count() - 1;


            cargarTotales();        
        }

        private void cargarTotales()
        {
            float totalKg = 0;
            foreach (Entidades.CortePorMovimiento  filaCorte in listaCortesPorMovimiento)
            {
                totalKg += filaCorte.CantKg;
            }

            txtCantItems.Text = Convert.ToString(grillaCortesPorMovimiento.Rows.Count);
            txtTotalKg.Text = Convert.ToString(totalKg);
        
        }

        private void cargarSucursales()
        {
            dtSucursalOrigen= oSucursalN.obtenerSucursales();

            comboSucOrigen.DataSource = dtSucursalOrigen;
            comboSucOrigen.DisplayMember = "sucursal";
            comboSucOrigen.ValueMember = "idSucursal";

            dtSucursalDestino= oSucursalN.obtenerSucursales();
            comboSucDestino.DataSource = dtSucursalDestino;
            comboSucDestino.DisplayMember = "sucursal";
            comboSucDestino.ValueMember = "idSucursal";

            cambiarSucursalDestino();
        }

        private void quitarCorteEnMovimiento()
        {
            try
            {
                int nroFila = grillaCortesPorMovimiento.Rows.GetFirstRow(DataGridViewElementStates.Selected);
                listaCortesPorMovimiento.RemoveAt(nroFila);

                cargarGrilla();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        
        }

        private void agregarCorteEnMovimiento()
        {
            cargarCortePorMovimiento();

            if (checkMantenerCodigo.Checked.Equals(true))
            {
                 txtCantKgs.Focus();
            }
            else
            {
                 txtCodigo.Focus();
            }
           
        }

        private void cerrarFormulario()
        {
            DialogResult respuesta;
            if (modificacion)
            {
                respuesta = MessageBox.Show("Si cierra el formulario se perderán los datos ingresados para realizar la modificación.\n¿Está seguro que desea salir?. ", "Modificar movimiento", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            }
            else
            {
                respuesta = MessageBox.Show("Si cierra el formulario se perderán los datos ingresados.\n¿Está seguro que desea salir?. ", "Nuevo movimiento", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            }

            if ((respuesta == System.Windows.Forms.DialogResult.Yes))
            {
                this.Close();
            }

        }

        private void cambiarSucursalDestino()
        { 
        
            if (comboSucOrigen.SelectedValue.Equals(1))
            {
                comboSucDestino.SelectedValue=2;
            }
            else
            {
                comboSucDestino.SelectedValue = 1;
            }
        }

        private void cambiarSucursalOrigen()
        {

            if (comboSucDestino.SelectedValue.Equals(1))
            {
                comboSucOrigen.SelectedValue = 2;
            }
            else
            {
                comboSucOrigen.SelectedValue = 1;
            }
        }

        private void comboSucOrigen_SelectedValueChanged(object sender, EventArgs e)
        {
            cambiarSucursalDestino();
        }

        private void comboSucDestino_SelectedValueChanged(object sender, EventArgs e)
        {
            cambiarSucursalOrigen();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            formBuscarCorte frmBuscarCorte = new formBuscarCorte();
            frmBuscarCorte.ShowDialog(this);
        }

        public void EnviarCorte(Entidades.Corte corte)
        {
            oCorteE = corte;
            txtCodigo.Text = Convert.ToString(oCorteE.codigo) ;
            txtCorte.Text = oCorteE.corte;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            agregarMovimiento();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            cerrarFormulario();
        }

        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            cargarCorte();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            agregarCorteEnMovimiento();
            
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            quitarCorteEnMovimiento();
        }

        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (e.KeyChar == (char)(Keys.Enter))
            {

                e.Handled = true;

                SendKeys.Send("{TAB}");

            }

        }

        private void cambiarMantenerCodigo()
        {
            if (checkMantenerCodigo.Checked.Equals(true))
            {
                txtCodigo.TabStop = false;
            }
            else
            {
                txtCodigo.TabStop = true;
            }
        }

        private void checkMantenerCodigo_CheckedChanged(object sender, EventArgs e)
        {
            cambiarMantenerCodigo();
        }


        const int WM_SYSCOMMAND = 0x0112;
        const int SC_CLOSE = 0xF060;

        protected override void WndProc(ref Message m)
        {
            if ((m.Msg == WM_SYSCOMMAND) && (m.WParam == (IntPtr)SC_CLOSE))
            {
                DialogResult respuesta;
                if (modificacion)
                {
                    respuesta = MessageBox.Show("Si cierra el formulario se perderán los datos ingresados para realizar la modificación.\n¿Está seguro que desea salir?. ", "Modificar movimiento", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                }
                else
                {
                    respuesta = MessageBox.Show("Si cierra el formulario se perderán los datos ingresados.\n¿Está seguro que desea salir?. ", "Nuevo movimiento", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                }

                if ((respuesta == System.Windows.Forms.DialogResult.No))
                {
                    return;
                }

            }

            base.WndProc(ref m);
        }


        
    }
}
