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


namespace Presentacion
{
    public partial class formIngresoEmbutido : formBaseColor, InterfaceCorte, InterfaceEmbutido
    {
        bool checkAnterior = false;
        Utilidades.SingletonLeerPeso Leer_Peso;

        formEmbutidos frmEmbutidos=new formEmbutidos();
        DataTable dtSucursales;
        Negocio.Sucursal oSucursalN;
        Negocio.Corte oCorteN=new Negocio.Corte();

        Entidades.Corte oCorteEmbutidoE;
        Entidades.Corte oCorteE;
        Entidades.CortePorEmbutido oCortePorEmbutidoE;
        Entidades.Embutido oEmbutidoE=new Entidades.Embutido();

        CortePorEmbutido cortePorEmbutido;
        List<CortePorEmbutido> listaCortesEnGrilla = new List<CortePorEmbutido>();

        List<Entidades.CortePorEmbutido> listaCortePorEmbutido = new List<Entidades.CortePorEmbutido>();

        public formIngresoEmbutido()
        {
            InitializeComponent();
            timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["timerForm"].ToString());
            cargarComboSucursal();
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

            cargarTotalKg();
            
        }

        public void obtenerParametros(formEmbutidos formEmbutidoParam)
        {
            frmEmbutidos = formEmbutidoParam;
        }

        private void agregarEmbutido()
        {

            if (validacionFinal())
            {
                cargarEmbutido();
                oEmbutidoE.idEmbutido = oCorteN.agregarEmbutido(oEmbutidoE);

                foreach (Entidades.CortePorEmbutido cortePorEmbutido in listaCortePorEmbutido)
                {
                    oCorteN.agregarCortePorEmbutido(cortePorEmbutido);
                }

                frmEmbutidos.cargarGrilla();
                this.Close();
            }

        }

        private bool validacionFinal()
        {
            if (grillaCortesPorEmbutido.SelectedRows.Count > 0)
            {
                
            
                if (comboSucursal.SelectedItem==null)
                {
                    MessageBox.Show("Complete el campo Sucursal.", "Completar la Sucursal", MessageBoxButtons.OK,MessageBoxIcon.Information);
                    return false;
                }

                else
                {
                    DialogResult respuesta = MessageBox.Show("Verifique si la Fecha, Sucursal y los demás los datos ingresados están correctos.\n ¿Están correctos?. ", "Verificar Datos", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                    if (respuesta == System.Windows.Forms.DialogResult.Yes)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }


            }
            else
            {
                MessageBox.Show("No ingresó ningún corte correspondiente al embutido.", "No existe cortes en la grilla", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return false;
            }
        }

        private void cargarEmbutido()
        {
            oEmbutidoE.fechaEmbutido = txtFechaEmbutido.Value;
            oEmbutidoE.corte = oCorteEmbutidoE;

            //creo y asigno la sucursal seleccionada
            Entidades.Sucursal oSucursalE=new Entidades.Sucursal();
            oSucursalE.IdSucursal = Convert.ToInt32(comboSucursal.SelectedValue.ToString());
            oEmbutidoE.sucursal = oSucursalE;

            oEmbutidoE.observaciones = txtObservaciones.Text.Trim();

        }

        private bool validarCantKgs()
        { 
            bool resp=true;

            try
            {
                decimal peso = Convert.ToDecimal(txtCantKgs.Text);
                if (peso>0)
                {
                    resp = true;
                }
                else
                {
                    DialogResult result = MessageBox.Show("La Cantidad de Kgs. ingresado es igual o menor a 0(Cero).\n¿Desea ingresar esa Cant. de Kgs. igualmente?.","",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2);
                    if (result==DialogResult.Yes)
                    {
                        resp = true;
                    }
                    else
                    {
                        resp = false;
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

        private void agregarCorteEnEmbutido()
        {
            if (validar() && validarCantKgs())
            {
                cargarCortePorEmbutido();
                //cargarCorteEnLista();
                listaCortesEnGrilla.Add(cortePorEmbutido);
                listaCortePorEmbutido.Add(oCortePorEmbutidoE);

                cargarGrilla();

                oCorteE = null;//libero el objeto
                oCortePorEmbutidoE = null;//libero el objeto
                limpiarCampos();
                
            }
        }

        private void quitarCortePorEmbutido()
        {
            if (grillaCortesPorEmbutido.SelectedRows.Count > 0)
            {
                int nroFila = grillaCortesPorEmbutido.Rows.GetFirstRow(DataGridViewElementStates.Selected);//obtiene nro de fila de la grilla
                listaCortePorEmbutido.RemoveAt(nroFila);//elimina objetos de las listas
                listaCortesEnGrilla.RemoveAt(nroFila);
            }
            else
            {
                MessageBox.Show("No hay ninguna fila seleccionada.", "Seleccione un fila", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            cargarGrilla();
        }

        private void cargarCortePorEmbutido()
        {
            oCortePorEmbutidoE = new Entidades.CortePorEmbutido();

            oCortePorEmbutidoE.embutido = oEmbutidoE;
            oCortePorEmbutidoE.corte = oCorteE;
            try 
	        {
                oCortePorEmbutidoE.kgUtilizado = float.Parse(txtCantKgs.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));

             }
	        catch (Exception)
	        {
                oCortePorEmbutidoE.kgUtilizado = float.Parse(txtCantKgs.Text.Trim());
            }

            oCortePorEmbutidoE.PesoBalanza = checkLeerPeso.Checked;
           
            //Cargar CortePorEmbutido para grilla
            cortePorEmbutido = new CortePorEmbutido();

            cortePorEmbutido.idCorte = oCortePorEmbutidoE.corte.idCorte;
            cortePorEmbutido.codigo = oCortePorEmbutidoE.corte.codigo;
            cortePorEmbutido.corte = oCortePorEmbutidoE.corte.corte;
            cortePorEmbutido.kgUtilizado = oCortePorEmbutidoE.kgUtilizado;
            
 
        }

        private void limpiarCampos()
        {
            txtCodCorteEnEmbutido.Text = "";
            txtCorteEnEmbutido.Text = "";
            txtCantKgs.Text = "";

        }

        private void cargarTotalKg()
        {
            float totalKg = 0;

            foreach (Entidades.CortePorEmbutido corte in listaCortePorEmbutido)
            {
                totalKg = totalKg + corte.kgUtilizado;                
            }

            txtTotalKg.Text = Convert.ToString(totalKg);
        }

        private void cargarCorteEnLista()
        {
            

        }

        private bool validar()
        {
            string mensaje="Complete los siguientes campos:";
            if (oCorteEmbutidoE == null || oCorteE == null || txtCantKgs.Text.Trim()=="")
            {
                
                if (oCorteEmbutidoE == null)
                {
                    mensaje += "\n" + "- Embutido";
                }
                if (oCorteE==null)
                {
                    mensaje += "\n" + "- Corte en Embutido";
                }
                if (txtCantKgs.Text.Trim()=="")
                {
                    mensaje += "\n" + "- Cant. Kgs";
                }
                

                //MessageBox.Show("Complete todos los Campos. (Codigo, Corte en Codigo y Cant. Kgs)", "Complete todos los campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MessageBox.Show(mensaje, "Complete todos los campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;                
            }


            else
            {
                return true;
            }
        }

        private void cargarComboSucursal()
        {
            dtSucursales = new DataTable();
            oSucursalN = new Negocio.Sucursal();
            dtSucursales = oSucursalN.obtenerSucursalSanMartin();
            dtSucursales = oSucursalN.obtenerSucursales();
            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedIndex = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString()) - 1;//-1;//No muestra ninguna sucursal
        }


        #endregion

        private void btnBuscarEmbutido_Click(object sender, EventArgs e)
        {
            formBuscarEmbutido frmBuscarEmbutido = new formBuscarEmbutido();
            if (frmEmbutidos.EsVentaClientes)
            {
                frmBuscarEmbutido.Text = "Buscar Cliente";
            }
            frmBuscarEmbutido.Show(this);
        }

        public void EnviarEmbutido(Entidades.Corte corte)
        {
            oCorteEmbutidoE = corte;
            txtCodigoEmbutido.Text = Convert.ToString(oCorteEmbutidoE.codigo);
            txtEmbutido.Text = oCorteEmbutidoE.corte;
        }

        private void btnBuscarCorte_Click(object sender, EventArgs e)
        {
            formBuscarCorte frmBuscarCorte = new formBuscarCorte();
            frmBuscarCorte.Show(this);
        }

        public void EnviarCorte(Entidades.Corte corte)
        {
            oCorteE = corte;
            txtCodCorteEnEmbutido.Text = Convert.ToString(oCorteE.codigo);
            txtCorteEnEmbutido.Text = oCorteE.corte;

            txtCantKgs.Focus();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            agregarCorteEnEmbutido();
            txtCodCorteEnEmbutido.Focus();
        }        

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            agregarEmbutido();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult respuesta = MessageBox.Show("Si cierra el formulario se perderan los datos ingresados.\n¿Está seguro que desea salir?. ", "Cerrar Formulario", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if ((respuesta == DialogResult.Yes))
            {
                this.Close();
            }            
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            quitarCortePorEmbutido();
        }

        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                e.Handled = true;
                SendKeys.Send("{TAB}");
            }
        }

        private void cargarCorteEnEmbutido()
        {
            try
            {
                if (txtCodCorteEnEmbutido.Text.Trim() != "")
                {
                    oCorteE = new Entidades.Corte();

                    DataTable dtCorte = new DataTable();

                    dtCorte = oCorteN.buscarCodigoCorte(Convert.ToInt32(txtCodCorteEnEmbutido.Text.Trim()));

                    if (dtCorte.Rows.Count > 0)
                    {
                        foreach (DataRow fila in dtCorte.Rows)
                        {
                            oCorteE.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                            oCorteE.codigo = Convert.ToInt32(fila["codigo"].ToString());
                            oCorteE.corte = fila["corte"].ToString();
                        }
                        //se cargan los datos del corte
                        txtCorteEnEmbutido.Text = oCorteE.corte;
                    }
                    else
                    {
                        txtCodCorteEnEmbutido.Text = "";
                        MessageBox.Show("El código no existe");
                        txtCodCorteEnEmbutido.Focus();
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void txtCodCorteEnEmbutido_TextChanged(object sender, EventArgs e)
        {
            cargarCorteEnEmbutido();
        }

        const int WM_SYSCOMMAND = 0x0112;
        const int SC_CLOSE = 0xF060;

        protected override void WndProc(ref Message m)
        {
            if ((m.Msg == WM_SYSCOMMAND) && (m.WParam == (IntPtr)SC_CLOSE) )
            {
                DialogResult respuesta = MessageBox.Show("Si cierra el formulario se perderan los datos ingresados.\n¿Está seguro que desea salir?. ", "Cerrar Formulario", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if ((respuesta == System.Windows.Forms.DialogResult.No))
                {
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void formIngresoEmbutido_Load(object sender, EventArgs e)
        {
            if (frmEmbutidos.EsVentaClientes)
            {
                this.Text = "Nueva Venta Cliente";
                groupBox1.Text = "Cliente ";
                groupBox2.Text = "Cortes ";
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
    }
}
