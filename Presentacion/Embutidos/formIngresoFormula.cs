using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Embutidos;
using Presentacion.Cortes;
using System.Configuration;
using Utilidades;
using Entidades;


namespace Presentacion
{
    public partial class formIngresoFormula : formBaseColor, InterfaceCorte, InterfaceEmbutido, InterfaceUsuario
    {
        Utilidades.SingletonLeerPeso Leer_Peso;

        public formFormulas frmFormulas = new formFormulas();
        DataTable dtSucursales;
        Negocio.Sucursal oSucursalN;
        Negocio.Corte oCorteN=new Negocio.Corte();

        //Entidades.Corte oCorteFormulaE;
        Entidades.Corte oCorteE;
        Entidades.CortePorFormula oCortePorFormulaE;
        Entidades.Formula oFormulaE = new Entidades.Formula();
        public Entidades.Usuario oUsuario;
        public int idFormula = 0;
        bool cargandoDatos = false;//para evitar validaciones cuando se están cargando datos de una Formula seleccionada

        Entidades.Usuario oUsuarioNuevoFormula;

        CortePorFormula cortePorFormula;
        List<CortePorFormula> listaCortesEnGrilla = new List<CortePorFormula>();

        List<Entidades.CortePorFormula> listaCortePorFormula = new List<Entidades.CortePorFormula>();

        bool esDuplicado = false;
        bool saveChanges = false;
        bool dejarDeLeerPeso = false;
        bool fijarPeso = Convert.ToBoolean(ConfigurationManager.AppSettings["fijarPeso"].ToString());

        Color enableColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["enableColor"].ToString()); //SystemColors.Window;
        Color readOnlyColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString());//SystemColors.ScrollBar;
        Color focusColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["focusColor"].ToString());//Color.Orange;//Color.NavajoWhite;//Color.MediumAquamarine;
        Color ultimoColor = Color.Green;

        float totalKg = 0;//totalPesoFormulas

        public formIngresoFormula()
        {
            InitializeComponent();
        }

        #region Métodos

        private void cargarGrilla()
        {
            grillaCortesPorEmbutido.AutoGenerateColumns = false;
            grillaCortesPorEmbutido.DataSource = null;
            grillaCortesPorEmbutido.DataSource = listaCortesEnGrilla;

            if (listaCortesEnGrilla.Count>0)
            {
                grillaCortesPorEmbutido.Rows[listaCortesEnGrilla.Count - 1].Selected = true;
                grillaCortesPorEmbutido.FirstDisplayedScrollingRowIndex = listaCortesEnGrilla.Count - 1;
            }                      
        }

        private void agregarFormula()
        {
            try
            {
                if (validacionFinal())
                {
                    Entidades.Usuario oUser = new Entidades.Usuario();
                    if (oFormulaE.IdFormula.Equals(0))
                    {
                        oFormulaE.CreadoPor = oUsuario;
                    }
                    else
                    {
                        oFormulaE.ActualizadoPor = oUsuario;
                    }

                    cargarCorteEnFormula();
                    oFormulaE.IdFormula = oCorteN.addOrEditFormula(oFormulaE, listaCortePorFormula);

                    MessageBox.Show("La Fórmula se registró correctamente.\n");
                    saveChanges = true;
                    frmFormulas.cargarGrilla();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error al registrar la Fórmula.\n" + ex.Message);
            }
        }

        private bool validacionFinal()
        {
            if (oFormulaE.Embutido == null || oFormulaE.Embutido.idCorte == 0)
            {
                MessageBox.Show("Seleccione el Embutido / Elaborado para la formula.", "Ingresar Formula", MessageBoxButtons.OK,MessageBoxIcon.Information);
                btnBuscarEmbutido.Select();
                return false;
            }

            if (lblError.Visible)
            {
                MessageBox.Show("El Embutido / Elaborado ya posee una formula. Modifique la existente", "Ingresar Formula", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnBuscarEmbutido.Select();
                return false;
            }

            if (grillaCortesPorEmbutido.SelectedRows.Count > 0)
            {
                DialogResult respuesta = MessageBox.Show("Verifique si los datos ingresados están correctos.\n ¿Están correctos?. ", "Verificar Datos", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (respuesta == System.Windows.Forms.DialogResult.Yes)
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
                MessageBox.Show("No ingresó ningún corte a la formula.", "No existe cortes en la grilla", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
        }

        private bool validarPorcentaje()
        { 
            bool resp=true;

            try
            {
                if (!Utilidades.Util_Form.validarCampoNumerico(txtPorcentaje.Text, "Porcentaje (%)"))
                {
                    resp = false;
                    txtPorcentaje.Focus();
                }
            }
            catch (Exception ex)
            {
                resp = false;
                MessageBox.Show("Error en método validarPorcentaje()\n\n"+ex.Message);
            }
            return resp;
        }

        private void agregarCorteEnFormula(Entidades.CortePorFormula item)
        {
            if (cargandoDatos || (validar() && validarPorcentaje()))
            {
                cargarCortePorFormula(item);
                //cargarCorteEnLista();
                listaCortesEnGrilla.Add(cortePorFormula);
                listaCortePorFormula.Add(oCortePorFormulaE);

                cargarGrilla();

                oCorteE = null;//libero el objeto
                oCortePorFormulaE = null;//libero el objeto
                limpiarCampos();

                txtCodCorteEnFormula.Focus();                
            }
        }

        private void quitarCortePorFormula()
        {
            if (grillaCortesPorEmbutido.SelectedRows.Count > 0)
            {
                int nroFila = grillaCortesPorEmbutido.Rows.GetFirstRow(DataGridViewElementStates.Selected);//obtiene nro de fila de la grilla
                listaCortePorFormula.RemoveAt(nroFila);//elimina objetos de las listas
                listaCortesEnGrilla.RemoveAt(nroFila);
            }
            else
            {
                MessageBox.Show("No hay ninguna fila seleccionada.", "Seleccione un fila", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            cargarGrilla();
        }

        private void cargarCortePorFormula(Entidades.CortePorFormula item)
        {
            if (cargandoDatos)
            {
                oCortePorFormulaE = item;
            }
            else
            {
                oCortePorFormulaE = new Entidades.CortePorFormula();
                oCortePorFormulaE.Formula = oFormulaE;
                oCortePorFormulaE.CorteEnFormula = oCorteE;
                oCortePorFormulaE.Porcentaje = Util_Form.convertFloat(txtPorcentaje.Text, false);
                oCortePorFormulaE.AgregarAuto = checkAgregarAuto.Checked;
            }     

            //Cargar CortePorFormula para grilla
            cortePorFormula = new CortePorFormula();
            cortePorFormula.IdCorte = oCortePorFormulaE.CorteEnFormula.idCorte;
            cortePorFormula.Codigo = oCortePorFormulaE.CorteEnFormula.codigo;
            cortePorFormula.Corte = oCortePorFormulaE.CorteEnFormula.corte;
            cortePorFormula.Porcentaje = oCortePorFormulaE.Porcentaje;
            cortePorFormula.AgregarAuto = oCortePorFormulaE.AgregarAuto;
        }

        private void limpiarCampos()
        {
            txtCodCorteEnFormula.Text = "";
            txtCorteEnFormula.Text = "";
            txtPorcentaje.Text = "";
        }


        private bool validar()
        {
            string mensaje="Complete los siguientes campos:";
            if (oFormulaE == null || oCorteE == null || txtPorcentaje.Text.Trim()=="")
            {                
                if (oFormulaE == null)
                {
                    mensaje += "\n" + "- Formula";
                    btnAgregar.Focus();
                }
                if (oCorteE==null)
                {
                    mensaje += "\n" + "- Corte en Formula";
                    txtCodCorteEnFormula.Focus();
                }
                if (txtPorcentaje.Text.Trim()=="")
                {
                    mensaje += "\n" + "- Porcentaje (%)";
                    txtPorcentaje.Focus();
                }
                MessageBox.Show(mensaje, "Complete todos los campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;                
            }
            else
            {
                return true;
            }
        }


        #endregion

        private void buscarEmbutido()
        {
            formBuscarEmbutido frmBuscarEmbutido = new formBuscarEmbutido();
            if (frmFormulas.EsVentaClientes)
            {
                frmBuscarEmbutido.Text = "Buscar Cliente";
            }
            frmBuscarEmbutido.Show(this);
        }

        public void EnviarEmbutido(Entidades.Corte corte)
        {
            oFormulaE.Embutido = corte;
            txtCodigoEmbutido.Text = Convert.ToString(oFormulaE.Embutido.codigo);
            txtEmbutido.Text = oFormulaE.Embutido.corte;
            txtCodCorteEnFormula.Focus();

            //validar que no exista formula para el corte seleccionado
            lblError.Visible = oCorteN.existeFormula(oFormulaE.Embutido.idCorte);
        }


        private void btnBuscarCorte_Click(object sender, EventArgs e)
        {
            buscarCorte();
        }

        private void buscarCorte()
        {
            formBuscarCorte frmBuscarCorte = new formBuscarCorte();
            frmBuscarCorte.Show(this);
        }

        public void EnviarCorte(Entidades.Corte corte)
        {
            oCorteE = corte;
            txtCodCorteEnFormula.Text = Convert.ToString(oCorteE.codigo);
            txtCorteEnFormula.Text = oCorteE.corte;
            txtPorcentaje.Focus();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            agregarCorteEnFormula(null);            

            capturarPantalla();
        }        

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            //Si está en modificación y se quiere habilitar la modificación
            if (!groupBoxFormula.Enabled)
            {
                this.Text = "Modificar fórmula";
                btnGuardar.Text = "&Guardar";
                groupBoxFormula.Enabled = groupBoxCortesFormula.Enabled = true;
                return;
            }
            agregarFormula();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();           
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            quitarCortePorFormula();
            capturarPantalla();
        }

        private void capturarPantalla()
        {
            //se refresca para que se muestren los datos
            this.Refresh();
            Util_Form.capturarPantalla("Formula", DateTime.Today);
        }

        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                e.Handled = true;
                SendKeys.Send("{TAB}");
            }
        }

        private void cargarCorteEnFormula()
        {
            try
            {
                if (txtCodCorteEnFormula.Text.Trim() != "")
                {
                    oCorteE = new Entidades.Corte();
                    DataTable dtCorte = new DataTable();
                    dtCorte = oCorteN.buscarCodigoCorte(Convert.ToInt32(txtCodCorteEnFormula.Text.Trim()));

                    if (dtCorte.Rows.Count > 0)
                    {
                        foreach (DataRow fila in dtCorte.Rows)
                        {
                            oCorteE.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                            oCorteE.codigo = Convert.ToInt32(fila["codigo"].ToString());
                            oCorteE.corte = fila["corte"].ToString();
                            oCorteE.tipo = fila["tipo"].ToString();
                        }
                        //se cargan los datos del corte
                        txtCorteEnFormula.Text = oCorteE.corte;
                    }
                    else
                    {
                        txtCorteEnFormula.Text = "";
                        oCorteE = null;
                    }
                }
                else
                {
                    txtCorteEnFormula.Text = "";
                    oCorteE = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en método cargarCorteEnFormula().\n\n"+ex.Message);
            }
        }

        private void txtCodCorteEnFormula_TextChanged(object sender, EventArgs e)
        {
            cargarCorteEnFormula();
        }

        private void formIngresoFormula_Load(object sender, EventArgs e)
        {
            
            
            this.Text += Utilidades.Conexion.getSucursalConexion();
            if (oUsuario == null)
            {
                this.Close();
            }
            else
            {
                //es modificacion
                if (idFormula > 0)
                {
                    this.Text = "Info fórmula";
                    btnGuardar.Text = "&Modificar";
                    groupBoxFormula.Enabled = groupBoxCortesFormula.Enabled = false;

                    cargandoDatos = true;
                    oFormulaE = oCorteN.findFormulaByID(idFormula);

                    txtCodigoEmbutido.Text = oFormulaE.Embutido.codigo.ToString();
                    txtEmbutido.Text = oFormulaE.Embutido.corte;

                    txtCreado.Text = oFormulaE.Creado.ToString();
                    txtCreadoPor.Text = oFormulaE.CreadoPor.User;
                    txtActualizado.Text = oFormulaE.Actualizado.ToString();
                    txtActualizadoPor.Text = oFormulaE.ActualizadoPor != null ? oFormulaE.ActualizadoPor.User : "";

                    foreach (Entidades.CortePorFormula item in oFormulaE.ListaCortesEnFormula)
                    {
                        agregarCorteEnFormula(item);
                    }

                    cargandoDatos = false;
                }

                txtUsuario.Text = oUsuario.Nombre;
                btnBuscarEmbutido.Select();
            }
        }

        private void txtCodCorteEnFormula_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                if (oCorteE == null || oCorteE.idCorte.Equals(0))
                {
                    MessageBox.Show("El código no existe");
                    txtCodCorteEnFormula.Focus();
                }
            }
        }

        private void formIngresoFormula_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = salir();
        }

        private bool salir()
        {
            bool ret = false;
            if (!saveChanges && (grillaCortesPorEmbutido.SelectedRows.Count > 0 || (oFormulaE != null && oFormulaE.IdFormula > 0)))
            {
                DialogResult respuesta = MessageBox.Show("Si cierra el formulario se perderan los datos ingresados.\n¿Está seguro que desea salir?. ", "Cerrar Formulario", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if ((respuesta == DialogResult.No))
                {
                    ret = true;
                } 
            }
            saveChanges = false;//setea en false(si esta TRUE porque se presionó btnGuardar)
            return ret;
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


        private void btnNuevo_Click(object sender, EventArgs e)
        {
            this.BringToFront(); 
            Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
            frmLogin.ShowDialog(this);
            formIngresoFormula frmIngresoFormula = new formIngresoFormula();
            frmIngresoFormula.oUsuario = oUsuarioNuevoFormula;
            frmIngresoFormula.frmFormulas = frmFormulas;
            frmIngresoFormula.Show();
            this.Left -= 200;
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuarioNuevoFormula = usuario;
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Home:
                    txtCodCorteEnFormula.Focus();
                    break;
                case Keys.PageUp:
                    txtCodCorteEnFormula.Focus();
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
                case Keys.F9:
                    buscarEmbutido();
                    break;
                case Keys.F10:
                    buscarCorte();
                    break;
                case Keys.Escape:
                    this.Close();
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnBuscarEmbutido_Click(object sender, EventArgs e)
        {
            buscarEmbutido();
        }
    }
}
