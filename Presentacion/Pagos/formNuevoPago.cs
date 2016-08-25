using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Personas;

namespace Presentacion.Pagos
{
    public partial class formNuevoPago : Form, InterfaceProveedor
    {
        formPagos frmPagos;
        Negocio.CuentaCorriente oCtaCteN = new Negocio.CuentaCorriente();
        Entidades.Persona oProveedorE=new Entidades.Persona();
        Entidades.Pago oPagoE=new Entidades.Pago();


        bool modificar=false;
        bool ultimaValidacion = true;//valida que los ingresos estén correctos antes de ingresar datos al DB


        public formNuevoPago()
        {
            InitializeComponent();
        }

        private void btnBuscarProv_Click(object sender, EventArgs e)
        {
            formBuscarProveedor frmBuscarProv = new formBuscarProveedor();
            frmBuscarProv.Show(this);
        }

        //comunicación con interface
        public void EnviarProveedor(Entidades.Persona proveedor)
        {
            oProveedorE = proveedor;
            this.txtProveedor.Text = oProveedorE.razonSocial;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            agregarPago();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Métodos

        public void asignarForm(formPagos frmPagosParam)
        {
            frmPagos = frmPagosParam;
        }

        public void obtenerParametros(Entidades.Pago oPagoParam, formPagos frmPagosParam)
        {
            modificar = true;
            this.Text = "Modificar Pago";

            frmPagos = frmPagosParam;
            oPagoE = oPagoParam;
            oProveedorE = oPagoE.Persona;

            cargarCampos();
        }

        private void cargarCampos()
        {
            txtProveedor.Text = oPagoE.Persona.razonSocial;
            txtNroRecibo.Text = oPagoE.NroRecibo;
            txtFechaPago.Value = oPagoE.Fecha;
            comboTipoPago.Text = oPagoE.FormaPago;
            txtImporte.Text = oPagoE.Importe.ToString();
            txtObservaciones.Text = oPagoE.Observaciones;

        }

        private void agregarPago()
        {
            if (validar())
            {
                
                try
                {
                    cargarPago();

                    if (ultimaValidacion)
                    {

                        oPagoE = oCtaCteN.addOrEditPago(oPagoE);

                        frmPagos.cargarGrilla();

                        DialogResult resp= MessageBox.Show("¿Ingresar otro pago?.", "Pagos", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        if (resp == DialogResult.Yes)
                        {
                            txtNroRecibo.Text = "";
                            txtImporte.Text = "";
                            txtObservaciones.Text = "";
                        }
                        else
                        {
                            this.Close();
                        }
                        
                    }
                    
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);

                }
            }
            
        }

        private void cargarPago()
        {
            try
            {
                ultimaValidacion = true;

                oPagoE.NroRecibo = txtNroRecibo.Text.Trim();
                oPagoE.Persona = oProveedorE;
                oPagoE.FormaPago = comboTipoPago.Text.Trim();
                oPagoE.Fecha = txtFechaPago.Value;

                try
                {
                    //oPagoE.Importe = float.Parse(txtImporte.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                    float importe = float.Parse(txtImporte.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                    oPagoE.Importe = importe;
                }
                catch (Exception)
                {
                    try
                    {
                        oPagoE.Importe = float.Parse(txtImporte.Text.Trim());
                    }
                    catch (Exception ex1)
                    {
                        MessageBox.Show(ex1.Message);
                        ultimaValidacion = false;
                    }
                }
                oPagoE.Observaciones = txtObservaciones.Text.Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool validar()
        {
            bool respuesta = true;

            if (txtNroRecibo.Text=="" || txtProveedor.Text ==""  || comboTipoPago.Text==""
                || txtImporte.Text=="")
            {
                respuesta = false;

                string mensaje = "Complete los siguientes campos: ";

                if (txtNroRecibo.Text == "" )
                {
                    mensaje += "\n" + "-Número de Recibo";
                }

                if (txtProveedor.Text == "")
                {
                    mensaje += "\n" + "-Proveedor";
                }
                if (comboTipoPago.Text == "")
                {
                    mensaje += "\n" + "-Forma de Pago";
                }
                if (txtImporte.Text == "")
                {
                    mensaje += "\n" + "-Importe";
                }
                
                MessageBox.Show(mensaje, "Completar campos", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            return respuesta;        
        }

        #endregion

    }
}
