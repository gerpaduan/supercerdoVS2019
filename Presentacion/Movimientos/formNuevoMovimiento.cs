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
using Utilidades;
using System.Configuration;

namespace Presentacion
{
    public partial class formNuevoMovimiento : formBaseColor, InterfaceCorte
    {
        Utilidades.SingletonLeerPeso Leer_Peso;
        Util_Form Util_Form = new Util_Form();

        formMovimientos frmMovimiento = new formMovimientos();

        DataTable dtCorte = new DataTable();
        DataTable dtSucursalOrigen = new DataTable();
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

        bool modificacion = false, huboModificaciones = false, eliminacion = false;

        public formNuevoMovimiento()
        {
            InitializeComponent();
            timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["timerForm"].ToString());
            checkLeerPeso.Visible = Convert.ToBoolean(ConfigurationManager.AppSettings["leerPeso"].ToString());
            cargarSucursales();
            dtCorte = oCorteN.obtenerCortes();
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

            lblIdDestino.Visible = true;
            lblIdOrigen.Visible = true;

            lblIdOrigen.Text = oMovimiento.IdMovOrigen != null && oMovimiento.IdMovOrigen > 0 ?
                oMovimiento.IdMovOrigen.ToString() : oMovimiento.IdMovimiento.ToString();
            lblIdDestino.Text = oMovimiento.IdMovOrigen != null && oMovimiento.IdMovOrigen > 0 ?
                oMovimiento.IdMovimiento.ToString() : "-";

            comboSucOrigen.SelectedValue = Convert.ToInt32(oMovimiento.SucursalOrigen.idSucursal);

            txtFechaMovimiento.Value = oMovimiento.FechaMovimiento;
            txtHora.Text = oMovimiento.FechaMovimiento.TimeOfDay.ToString();
            txtObservaciones.Text = oMovimiento.Observaciones;

            string datosCreado = "Creado: " + oMovimiento.Creado.ToString() + "\n\nModificado: " +
                (oMovimiento.Actualizado > DateTime.Today.AddYears(-20) ? oMovimiento.Actualizado.ToString() : "-");
            txtCreado.Text = datosCreado;
            txtCreado.Visible = true;

            cargarGrilla();

        }

        public void agregarMovimiento()
        {
            if (Util_Form.validarFechaConAdmin(Presentacion.FormPrincipal.logueado, txtFechaMovimiento.Value, "Fecha") && 
                Util_Form.validarSucursal(Presentacion.FormPrincipal.logueado, Convert.ToInt32(comboSucOrigen.SelectedValue.ToString()))
                 && Util_Form.validarFecha(txtFechaMovimiento.Value, "Fecha") && validacionFinal())
            {
                cargarMovimiento();
                try
                {
                    if (modificacion)
                    {
                        if (eliminacion)
                        {
                            oCorteN.eliminarMovimiento(oMovimiento.IdMovimiento);
                        }
                        else
                        {
                            oCorteN.quitarCortesPorMovimiento(oMovimiento);
                            oCorteN.modificarMovimiento(oMovimiento);
                        }
                    }
                    else
                    {
                        oMovimiento.IdMovimiento = oCorteN.agregarMovimiento(oMovimiento);
                    }

                    foreach (Entidades.CortePorMovimiento corteEnLista in listaCortesPorMovimiento)
                    {
                        corteEnLista.Movimientos = oMovimiento;
                        oCorteN.agregarCortePorMovimiento(corteEnLista);
                    }

                    //DialogResult resp = MessageBox.Show("¿Emitir Reporte con el Total Acumulado por cada Corte?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    //if (resp == DialogResult.Yes)
                    //{
                    //    imprimir();
                    //}

                    frmMovimiento.cargarGrilla();
                    huboModificaciones = false;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void imprimir()
        {
            try
            {
                Entidades.Movimiento oMovimientoE = oCorteN.cargarMovimiento(oMovimiento.IdMovimiento);
                FormReportes frmReportes;

                string titulo = "Movimiento Acum.";
                Reportes.ReporteMovimientoAcum reporte = new Reportes.ReporteMovimientoAcum();
                frmReportes = new FormReportes(reporte, titulo, null, oMovimientoE.FechaMovimiento, oMovimientoE.FechaMovimiento);

                frmReportes.ListaCortesPorMov = listaEnGrilla;
                frmReportes.Objetos = true;
                frmReportes.ReporteMovimiento = true;
                frmReportes.Origen = oMovimientoE.SucursalOrigen.SucursalNombre;
                frmReportes.Destino = oMovimientoE.SucursalDestino.SucursalNombre;

                frmReportes.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool validacionFinal()
        {
            if (modificacion && huboModificaciones)
            {
                eliminacion = grillaCortesPorMovimiento.Rows.Count.Equals(0) ? true : false;
                DialogResult resp = eliminacion ?
                    MessageBox.Show("Si guarda los cambios se eliminará el movimiento.\n ¿Eliminar el Movimiento?", "Eliminar Movimiento", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
                    :
                    MessageBox.Show("¿Está seguro que desea modificar los datos del Movimiento?", "Modificar Movimiento", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
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
                if (huboModificaciones)
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
                else
                {
                    MessageBox.Show("No ha realizado ninguna modificación.\nPresione cancelar si desea salir");
                    return false;
                }
            }
        }

        private void cargarMovimiento()
        {
            oMovimiento.FechaMovimiento = txtFechaMovimiento.Value;

            //Se cargan sucursales y se asignan al movimiento

            oSucursalOrigen.idSucursal = Convert.ToInt32(comboSucOrigen.SelectedValue.ToString());
            oSucursalDestino.idSucursal = Convert.ToInt32(comboSucDestino.SelectedValue.ToString());

            oMovimiento.SucursalOrigen = oSucursalOrigen;
            oMovimiento.SucursalDestino = oSucursalDestino;

            oMovimiento.Observaciones = txtObservaciones.Text.Trim();

        }

        private bool validar()
        {
            bool resp = true;

            if (txtCorte.Text.Trim() == "")
            {
                txtCodigo.Focus();
                MessageBox.Show("No se hay ingresado ningún corte.", "Completar Campos", MessageBoxButtons.OK, MessageBoxIcon.Information);

                resp = false;
            }
            else
            {
                if (txtCantUnidad.Text.Trim() == "")
                {
                    MessageBox.Show("Ingrese la cantidad de Unidades", "Completar Campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtCantUnidad.Focus();
                    resp = false;
                }
                else
                {
                    if (txtCantKgs.Text.Trim() == "")
                    {
                        txtCantKgs.Focus();
                        MessageBox.Show("Ingrese la cantidad de Kgs.", "Completar Campos", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        resp = false;
                    }
                }

            }
            return resp;
        }

        private void cargarCorte()
        {
            try
            {
                oCorteE = null;
                txtCorte.Text = "";
                if (txtCodigo.Text.Trim() != "")
                {
                    oCorteE = new Entidades.Corte();

                    if (dtCorte.Rows.Count > 0)
                    {
                        foreach (DataRow fila in dtCorte.Rows)
                        {
                            if (fila["codigo"].ToString().Equals(txtCodigo.Text))
                            {
                                oCorteE.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                                oCorteE.codigo = Convert.ToInt32(fila["codigo"].ToString());
                                oCorteE.corte = fila["corte"].ToString();

                                break;
                            }
                        }
                        //se cargan los datos del corte
                        txtCorte.Text = oCorteE.corte;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool validarCantKgs()
        {
            bool resp = true;

            try
            {
                decimal peso = Convert.ToDecimal(txtCantKgs.Text);
                if (peso > 0)
                {
                    resp = true;
                }
                else
                {
                    MessageBox.Show("Cant. Kgs debe ser mayor a 0 (cero)");
                    resp = false;
                    txtCantKgs.Select();
                }
            }
            catch (Exception)
            {
                resp = false;
                MessageBox.Show("Cant. Kgs debe ser un número represente un peso en Kg.");
            }
            return resp;
        }
        private void cargarCortePorMovimiento()
        {
            if (validar() && validarCantKgs())
            {
                try
                {
                    oCortePorMovimientoE = new Entidades.CortePorMovimiento();
                    oCortePorMovimientoE.Corte = oCorteE;
                    oCortePorMovimientoE.CantUnidad = Convert.ToInt32(txtCantUnidad.Text);
                    oCortePorMovimientoE.CantKg = Util_Form.convertFloat(txtCantKgs.Text);
                    oCortePorMovimientoE.PesoBalanza = checkLeerPeso.Checked;

                    listaCortesPorMovimiento.Add(oCortePorMovimientoE);
                    cargarGrilla();
                    huboModificaciones = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                limpiarCampos();
                if (checkLeerPeso.Checked.Equals(true))
                {
                    txtCodigo.Focus();
                }
                else
                {
                    txtCodigo.Focus();
                }
            }
        }

        private void limpiarCampos()
        {
            txtCodigo.Text = "";
            txtCorte.Text = "";
            txtCantUnidad.Text = "";
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

            if (grillaCortesPorMovimiento.Rows.Count > 0)
            {
                grillaCortesPorMovimiento.Rows[listaCortesPorMovimiento.Count() - 1].Selected = true;
                grillaCortesPorMovimiento.FirstDisplayedScrollingRowIndex = listaCortesPorMovimiento.Count() - 1;
            }

            cargarTotales();
        }

        private void cargarTotales()
        {
            float totalKg = 0, totalCantUn = 0;
            foreach (Entidades.CortePorMovimiento filaCorte in listaCortesPorMovimiento)
            {
                totalKg += filaCorte.CantKg;
                totalCantUn += filaCorte.CantUnidad;
            }

            txtCantItems.Text = Convert.ToString(grillaCortesPorMovimiento.Rows.Count);
            txtTotalKg.Text = Convert.ToString(totalKg);
            txtCantTotUni.Text = Convert.ToString(totalCantUn);

        }

        private void cargarSucursales()
        {
            dtSucursalOrigen = oSucursalN.obtenerSucursales();

            comboSucOrigen.DataSource = dtSucursalOrigen;
            comboSucOrigen.DisplayMember = "sucursal";
            comboSucOrigen.ValueMember = "idSucursal";

            dtSucursalDestino = oSucursalN.obtenerSucursales();
            comboSucDestino.DataSource = dtSucursalDestino;
            comboSucDestino.DisplayMember = "sucursal";
            comboSucDestino.ValueMember = "idSucursal";

            comboSucOrigen.SelectedValue = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());//-1;//No muestra ninguna sucursal
            cambiarSucursalDestino();
        }

        private void quitarCorteEnMovimiento()
        {
            try
            {
                if (grillaCortesPorMovimiento.Rows.Count > 0)
                {
                    int nroFila = grillaCortesPorMovimiento.Rows.GetFirstRow(DataGridViewElementStates.Selected);
                    DialogResult eliminarCorte = MessageBox.Show(
                        "- "+ listaCortesPorMovimiento[nroFila].Corte.CorteDesc + "  " +
                        listaCortesPorMovimiento[nroFila].CantKg + "Kgs.\n¿Quitar el corte del movimiento?",
                        "Quitar Corte", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                    if (eliminarCorte.Equals(DialogResult.Yes))
                    {
                        listaCortesPorMovimiento.RemoveAt(nroFila);
                        cargarGrilla();
                        huboModificaciones = true;                        
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void agregarCorteEnMovimiento()
        {
            cargarCortePorMovimiento();
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
                comboSucDestino.SelectedValue = 2;
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
            txtCodigo.Text = Convert.ToString(oCorteE.codigo);
            txtCorte.Text = oCorteE.corte;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            agregarMovimiento();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            //cerrarFormulario();
            this.Close();
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
                if (oCorteE != null && oCorteE.idCorte > 0)
                {
                    e.Handled = true;
                    SendKeys.Send("{TAB}");
                }
                else
                {
                    txtCorte.Text = "";
                    MessageBox.Show("El código no existe");
                    txtCodigo.Focus();
                }
            }
        }

        private void cambiarMantenerCodigo()
        {
            if (checkLeerPeso.Checked.Equals(true))
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

        private void formNuevoMovimiento_Load(object sender, EventArgs e)
        {
            if (modificacion && !Util_Form.validarPermisoModif(Presentacion.FormPrincipal.logueado, oMovimiento.FechaMovimiento))
            {
                this.Close();
            }

            if (dtCorte.Rows.Count == 0)
            {
                MessageBox.Show("No se pudieron cargar los cortes.");
            }
        }

        private void checkLeerPeso_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkLeerPeso.Checked)
                {
                    txtCantKgs.ReadOnly = true;
                    txtCantKgs.TabStop = false;
                    timer1.Enabled = true;
                }
                else
                {
                    txtCantKgs.Text = "";
                    txtCantKgs.ReadOnly = false;
                    txtCantKgs.TabStop = true;
                    timer1.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (checkLeerPeso.Checked)
                {
                    Leer_Peso = Utilidades.SingletonLeerPeso.CrearLeerPeso();
                    txtCantKgs.Text = Leer_Peso.ObtenerPeso();
                }
            }
            catch (Exception ex)
            {
                timer1.Enabled = false;
                if (Utilidades.Util_Form.errorBalanza(ex.Message) == DialogResult.Yes)
                {
                    checkLeerPeso.Checked = false;
                }
                else
                {
                    timer1.Enabled = true;
                }
            }
        }

        private void txtCantUnidad_TextChanged(object sender, EventArgs e)
        {
            Util_Form.validarCampoNumeroEntero(txtCantUnidad.Text, "Cant. Un");
        }

        private void btnGuardar_Enter(object sender, EventArgs e)
        {
            btnGuardar.BackColor = System.Drawing.Color.FromName("LimeGreen");
        }

        private void btnGuardar_Leave(object sender, EventArgs e)
        {
            btnGuardar.BackColor = System.Drawing.Color.FromName("SeaGreen");
        }

        private void txtCantUnidad_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                if (Util_Form.validarCampoNumeroEntero(txtCantUnidad.Text, "Cant. Un."))
                {
                    e.Handled = true;
                    SendKeys.Send("{TAB}");
                }
                else
                {
                    txtCantUnidad.Text = "";
                    txtCantUnidad.Focus();
                }
            }
        }

        private void comboSucOrigen_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (modificacion && oMovimiento != null)
            {
                if (!comboSucOrigen.SelectedValue.Equals("") && !oMovimiento.SucursalOrigen.idSucursal.Equals(comboSucOrigen.SelectedValue))
                {
                    huboModificaciones = true;
                }                
            }
        }

        private void formNuevoMovimiento_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = salir();
        }

        private bool salir()
        {
            if (huboModificaciones)
            {
                DialogResult respuesta = MessageBox.Show("Si cierra el formulario se perderan las modificaciones realizadas.\n¿Está seguro que desea salir?. ", "Movimiento", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if ((respuesta == System.Windows.Forms.DialogResult.Yes))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private void txtFechaMovimiento_ValueChanged(object sender, EventArgs e)
        {
            huboModificaciones = true;
        }
    }
}
