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
using System.Configuration;
using System.IO;

namespace Presentacion
{
    public partial class formNuevoMovimiento : formBaseColor, InterfaceCorte, InterfaceUsuario
    {
        Utilidades.SingletonLeerPeso Leer_Peso;
        Util_Form Util_Form = new Util_Form();

        formMovimientos frmMovimiento = new formMovimientos();

        DataTable dtCorte = new DataTable();
        DataTable dtSucursalOrigen = new DataTable();
        DataTable dtSucursalDestino = new DataTable();

        public Entidades.Usuario oUsuario;
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
        Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        bool modificacion = false, huboModificaciones = false, eliminacion = false, dejarDeLeerPeso = false;

        bool loginRapidoMovimiento = Entidades.Parametros.loginRapidoMovimiento;// Convert.ToBoolean(ConfigurationManager.AppSettings["loginRapidoMovimiento"].ToString());

        bool fijarPeso = Convert.ToBoolean(ConfigurationManager.AppSettings["fijarPeso"].ToString());
        bool cantSuc2 = false;

        Color enableColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["enableColor"].ToString()); //SystemColors.Window;
        Color readOnlyColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString());//SystemColors.ScrollBar;
        Color focusColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["focusColor"].ToString());//Color.Orange;//Color.NavajoWhite;//Color.MediumAquamarine;
        Color ultimoColor = Color.Green;

        public formNuevoMovimiento()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
            idMovimientoLabel.Text = "0";
            timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["timerForm"].ToString());
            checkTicket.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["ticketForms"].ToString());
            checkLeerPeso.Visible = FormPrincipal.logueado || Convert.ToBoolean(ConfigurationManager.AppSettings["leerPeso"].ToString());
            cargarSucursales();
            dtCorte = oCorteN.obtenerCortes();
        }

        private void formNuevoMovimiento_Load(object sender, EventArgs e)
        {
            //TODO: Loguear rapido
            if (loginRapidoMovimiento && oMovimiento.IdMovimiento == 0)
                logueoUsuario();
            else
            {
                Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
                frmLogin.soloActivos = true;
                frmLogin.ShowDialog(this);
            }

            if (oUsuario != null)
            {
                if (!oUsuarioN.tienePermiso(oUsuario, this.Name, 
                    oMovimiento.IdMovimiento > 0 ? oMovimiento.FechaMovimiento : DateTime.Today,
                    oMovimiento.IdMovimiento > 0 ? oMovimiento.CreadoPor.Id : oUsuario.Id))
                {
                    Utilidades.Mensajes.ErrorPermisoEdicion();
                    this.Close();
                    return;
                }
            }

            if (oUsuario == null)
            {
                this.Close();
                return;
            }

            txtUsuario.Text = oUsuario.Nombre;
            this.Text += Utilidades.Conexion.getSucursalConexion();
            //if (modificacion && !Util_Form.validarPermisoModif(Presentacion.FormPrincipal.logueado, oMovimiento.FechaMovimiento))
            //{
            //    this.Close();
            //}

            if (dtCorte.Rows.Count == 0)
            {
                MessageBox.Show("No se pudieron cargar los Productos.");
            }
        }

        private void logueoUsuario()
        {
            //Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
            //frmLogin.ShowDialog(this);
            this.BringToFront();
            Usuarios.formSelectUser frmSelectUser = new Presentacion.Usuarios.formSelectUser();
            frmSelectUser.ShowDialog(this);
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
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

            idMovimientoLabel.Text = oMovimiento.IdMovimiento.ToString();

            lblIdDestino.Visible = true;
            lblIdOrigen.Visible = true;

            lblIdOrigen.Text = oMovimiento.IdMovOrigen != null && oMovimiento.IdMovOrigen > 0 ?
                oMovimiento.IdMovOrigen.ToString() : oMovimiento.IdMovimiento.ToString();
            lblIdDestino.Text = oMovimiento.IdMovOrigen != null && oMovimiento.IdMovOrigen > 0 ?
                oMovimiento.IdMovimiento.ToString() : "-";

            comboSucOrigen.SelectedValue = Convert.ToInt32(oMovimiento.SucursalOrigen.idSucursal);
            comboSucDestino.SelectedValue = Convert.ToInt32(oMovimiento.SucursalDestino.idSucursal);
            txtFechaMovimiento.Value = oMovimiento.FechaMovimiento;
            txtObservaciones.Text = oMovimiento.Observaciones;

            txtCreado.Text = Util_Form.fechaFormato24Horas(oMovimiento.Creado);
            txtCreadoPor.Text = oMovimiento.CreadoPor != null ? oMovimiento.CreadoPor.Nombre : "-";
            txtActualizado.Text = oMovimiento.Actualizado != null ? Util_Form.fechaFormato24Horas(oMovimiento.Actualizado) : "-";
            txtActualizadoPor.Text = oMovimiento.ActualizadoPor != null ? oMovimiento.ActualizadoPor.Nombre : "-";

            cargarGrilla();
        }

        public void agregarMovimiento()
        {
            if (!oUsuarioN.tienePermiso(oUsuario, this.Name, txtFechaMovimiento.Value,
                        oMovimiento.IdMovimiento > 0 ? oMovimiento.CreadoPor.Id : oUsuario.Id))
            {
                Utilidades.Mensajes.ErrorPermisoEdicion();
                return;
            }

            if (comboSucOrigen.SelectedValue.Equals(comboSucDestino.SelectedValue) || 
                comboSucOrigen.SelectedValue.Equals(0) || comboSucDestino.SelectedValue.Equals(0))
            {
                MessageBox.Show("Debe seleccionar la Sucursal Origen y Sucursal Destino; y ser diferentes entre ellas",
                    "Mensaje",MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtCodigo.Focus();
                return;
            }

            if (oCorteE != null && oCorteE.idCorte > 0)
            {
                MessageBox.Show("No se puede guardar el movimiento porque hay un Producto seleccionado");
                txtCodigo.Focus();
                return;
            }

            if (Util_Form.validarSucursal(Presentacion.FormPrincipal.logueado, Convert.ToInt32(comboSucOrigen.SelectedValue.ToString()))
                 && Util_Form.validarFecha(txtFechaMovimiento.Value, "Fecha") && validacionFinal())
            {
                cargarMovimiento();
                try
                {
                    if (eliminacion)
                    {
                        oCorteN.eliminarMovimiento(oMovimiento.IdMovimiento, oUsuario);
                    }
                    else
                    {
                        oMovimiento.IdMovimiento = oCorteN.addOrEditMovimiento(oMovimiento);
                    }

                    foreach (Entidades.CortePorMovimiento corteEnLista in listaCortesPorMovimiento)
                    {
                        corteEnLista.Movimientos = oMovimiento;
                        oCorteN.agregarCortePorMovimiento(corteEnLista);
                    }

                    if (checkTicket.Checked)
                    {
                        Ticket.formTipoTicket tipoTicket = new Presentacion.Ticket.formTipoTicket();
                        tipoTicket.movimientoAcumulado(oMovimiento.IdMovimiento);
                    }

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

        //private void imprimir()
        //{
        //    try
        //    {
        //        Entidades.Movimiento oMovimientoE = oCorteN.cargarMovimiento(oMovimiento.IdMovimiento, true);
        //        FormReportes frmReportes;

        //        string titulo = "Movimiento Acum.";
        //        Reportes.ReporteMovimientoAcum reporte = new Reportes.ReporteMovimientoAcum();
        //        frmReportes = new FormReportes(reporte, titulo, null, oMovimientoE.FechaMovimiento, oMovimientoE.FechaMovimiento);

        //        frmReportes.ListaCortesPorMov = listaEnGrilla;
        //        frmReportes.Objetos = true;
        //        frmReportes.ReporteMovimiento = true;
        //        frmReportes.Origen = oMovimientoE.SucursalOrigen.SucursalNombre;
        //        frmReportes.Destino = oMovimientoE.SucursalDestino.SucursalNombre;

        //        frmReportes.Show();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}

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
                if (true || huboModificaciones)
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
            switch (oMovimiento.IdMovimiento)
            {
                case 0:
                    oMovimiento.CreadoPor = oUsuario;
                    break;
                default:
                    oMovimiento.ActualizadoPor = oUsuario;
                    break;
            }
        }

        private bool validar()
        {
            bool resp = true;

            if (txtCorte.Text.Trim() == "")
            {
                txtCodigo.Focus();
                MessageBox.Show("No se hay ingresado ningún Producto.", "Completar Campos", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
                                oCorteE.codigo = Convert.ToInt64(fila["codigo"].ToString());
                                oCorteE.corte = fila["corte"].ToString();
                                oCorteE.tipo = fila["tipo"].ToString();
                                oCorteE.Promedio = Utilidades.Util_Form.convertFloat(fila["promedio"].ToString(), false);
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
                float peso = Utilidades.Util_Form.convertFloat(txtCantKgs.Text, false);
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

                ///se valida que la Cant. Unidad ingresada se corresponda con la Cant.Kgs del corte
                ///Nota: Si es Cant.Unidad = 0 ('Cero') no se valida
                ///
                int cantUni = Convert.ToInt32(txtCantUnidad.Text);
                if (resp && cantUni > 0 && !checkPermitirIngreso.Checked)
                {
                    float limitInferior = oCorteE.Promedio * (cantUni - 1);
                    float limitSuperior = oCorteE.Promedio * (cantUni + 1);
                    if (!(limitInferior < peso && peso < limitSuperior))
                    {
                        checkPermitirIngreso.Visible = true;
                        MessageBox.Show("La Cant.Unidad ingresada no se corresponde con la Cant.Kgs del Producto\n\n"+
                            "Corrobore la Cant.Unidades ingresada y tilde 'Permitir ingreso' si está correcto.","No hay consistencia",
                             MessageBoxButtons.OK,MessageBoxIcon.Error);
                        resp = false;
                        txtCantUnidad.Select();
                    }
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
                    oCortePorMovimientoE.CantKg = Util_Form.convertFloat(txtCantKgs.Text, true);
                    oCortePorMovimientoE.PesoBalanza = checkLeerPeso.Checked;
                    oCortePorMovimientoE.PermitirIngreso = checkPermitirIngreso.Checked;

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
            txtTotalKg.Text = totalKg.ToString("F3");
            txtCantTotUni.Text = Convert.ToString(totalCantUn);

        }

        private void cargarSucursales()
        {
            dtSucursalOrigen = oSucursalN.obtenerSucursales();

            cantSuc2 = dtSucursalOrigen.Rows.Count == 2;

            comboSucOrigen.DataSource = dtSucursalOrigen;
            comboSucOrigen.DisplayMember = "sucursal";
            comboSucOrigen.ValueMember = "idSucursal";

            dtSucursalDestino = oSucursalN.obtenerSucursales();
            comboSucDestino.DataSource = dtSucursalDestino;
            comboSucDestino.DisplayMember = "sucursal";
            comboSucDestino.ValueMember = "idSucursal";
            comboSucDestino.SelectedValue = 0;

            comboSucOrigen.SelectedValue = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());//-1;//No muestra ninguna sucursal
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
            //si la cantidad de Sucursales es mayor a 2 no hace el intercambio de valores automaticamente
            if (!cantSuc2)
                return;

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
            //si la cantidad de Sucursales distinta a 2 no hace el intercambio de valores automaticamente
            if (!cantSuc2 || comboSucDestino.SelectedValue == null)
                return;

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
            buscarCorte();
        }

        private void buscarCorte()
        {
            try
            {
                dtCorte = oCorteN.obtenerCortes();

                formBuscarCorte frmBuscarCorte = new formBuscarCorte();
                frmBuscarCorte.ShowDialog(this);
                txtCantUnidad.Focus();
                txtCantUnidad.Select();
            }
            catch (Exception)
            {
                MessageBox.Show("Hubo un error al cargar los Productos.");
            }
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
            checkPermitirIngreso.Visible = false;
            checkPermitirIngreso.Checked = false;
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            agregarCorteEnMovimiento();
            capturarPantalla();
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            quitarCorteEnMovimiento();
            capturarPantalla();
        }

        private void capturarPantalla()
        {
            //se refresca para que se muestren los datos
            this.Refresh();
            Util_Form.capturarPantalla("Movimiento", txtFechaMovimiento.Value);
        }

        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '*')// (char)(Keys.Multiply))
            {
                e.Handled = true;
                return;
            }

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

        private void checkLeerPeso_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkLeerPeso.Checked)
                {
                    dejarDeLeerPeso = false;
                    txtCodigo.Focus();
                    txtCantKgs.ReadOnly = true;
                    txtCantKgs.TabStop = false;
                    timer1.Enabled = true;
                }
                else
                {
                    txtCantKgs.Text = "";
                    txtCantKgs.ReadOnly = false;
                    txtCantKgs.TabStop = true;
                    txtCantKgs.Select();
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
                if (!FormPrincipal.leerBalanza) return;

                if (checkLeerPeso.Checked)
                {
                    if (fijarPeso)
                    {
                        txtCantKgs.Text = "1.500";
                    }
                    else
                    {
                        if (Convert.ToBoolean(ConfigurationManager.AppSettings["singleton"].ToString()))
                        {
                            Leer_Peso = Utilidades.SingletonLeerPeso.CrearLeerPeso();
                            txtCantKgs.Text = Leer_Peso.ObtenerPeso();
                        }
                        else
                        {
                            txtCantKgs.Text = Utilidades.Util_Form.leerPesoBalanza();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                timer1.Enabled = false;
                if (Utilidades.Util_Form.errorBalanza(ex.Message) == DialogResult.Yes)
                {
                    dejarDeLeerPeso = true;
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
            if (!Util_Form.validarCampoNumeroEntero(txtCantUnidad.Text, "Cant. Un"))
                txtCantUnidad.Text = "";
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
            if (e.KeyChar == '+')// (char)(Keys.Multiply))
            {
                e.Handled = true;
                //btnAgregar.Focus();
                //checkPermitirIngreso.Checked = !checkPermitirIngreso.Checked;
                return;
            }

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
            huboModificaciones = oMovimiento != null && oMovimiento.IdMovimiento > 0 && !oMovimiento.FechaMovimiento.Equals(txtFechaMovimiento.Value);
        }

        private void btnVerAcum_Click(object sender, EventArgs e)
        {
            cargarListaEnGrilla();
            Movimientos.formVerAcumulados formVerAcum = new Presentacion.Movimientos.formVerAcumulados();
            formVerAcum.verAcumulados(listaEnGrilla, null, Presentacion.Movimientos.formVerAcumulados.tipoAcum.movimiento);// (listaCortesPorMovimiento);
            formVerAcum.ShowDialog();
        }

        private void control_Enter(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox objectToChangeColor = (TextBox)sender;
                if (!objectToChangeColor.BackColor.Equals(focusColor)) ultimoColor = objectToChangeColor.BackColor;
                objectToChangeColor.BackColor = focusColor;
                return;
            }

            if (sender is MaskedTextBox)
            {
                MaskedTextBox objectToChangeColor = (MaskedTextBox)sender;
                if (!objectToChangeColor.BackColor.Equals(focusColor)) ultimoColor = objectToChangeColor.BackColor;
                objectToChangeColor.BackColor = focusColor;
                return;
            }

            if (sender is Button)
            {
                Button objectToChangeColor = (Button)sender;
                objectToChangeColor.UseVisualStyleBackColor = false;
                objectToChangeColor.BackColor = focusColor;
                return;
            }
        }

        private void control_Leave(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox objectToChangeColor = (TextBox)sender;
                objectToChangeColor.BackColor = ultimoColor;
                if (objectToChangeColor.Name.Equals("txtCantUnidad")) tipoDeCorte();
                return;
            }

            if (sender is MaskedTextBox)
            {
                MaskedTextBox objectToChangeColor = (MaskedTextBox)sender;
                objectToChangeColor.BackColor = ultimoColor;
                return;
            }

            if (sender is Button)
            {
                Button objectToChangeColor = (Button)sender;
                objectToChangeColor.UseVisualStyleBackColor = true;
                return;
            }
        }

        private void tipoDeCorte()
        {
            try
            {
                if ((oCorteE != null && oCorteE.idCorte > 0 && oCorteE.tipo.Equals("Unidad") && checkLeerPeso.Checked))
                {
                    checkLeerPeso.Checked = false;
                    txtCantKgs.Focus();
                }
                else
                {
                    if (!dejarDeLeerPeso && oCorteE != null && oCorteE.idCorte > 0 && !oCorteE.tipo.Equals("Unidad") && !checkLeerPeso.Checked)
                    {
                        checkLeerPeso.Checked = true;
                        txtCantKgs.BackColor = readOnlyColor;
                        btnAgregar.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error al verificar el tipo del Producto.\n\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Add:
                    btnAgregar.Focus();
                    checkPermitirIngreso.Checked = !checkPermitirIngreso.Checked;
                break;
                case Keys.Multiply:
                    dejarDeLeerPeso = checkLeerPeso.Checked;
                    checkLeerPeso.Checked = FormPrincipal.leerBalanza ? !checkLeerPeso.Checked : checkLeerPeso.Checked;
                    break;
                case Keys.Home:
                    txtCodigo.Focus();
                    break;
                case Keys.PageUp:
                    txtCodigo.Focus();
                    break;
                case Keys.F2:
                    foreach (Form frm in Application.OpenForms)
                    {
                        if (frm.GetType() == typeof(FormPrincipal))
                        {
                            frm.BringToFront();
                            break;
                        }
                    }
                    break;
                case Keys.F10:
                    buscarCorte();
                    break;
                case Keys.F11:
                    txtObservaciones.Focus();
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
