using Entidades;
using Presentacion.Caja;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utilidades;

namespace Presentacion.Cortes
{
    public partial class formMarcaAddOrEdit : Form, InterfaceUsuario, InterfacePersona
    {
        public  formMarcas frmMarcas;
        Entidades.Persona oMarcaE = new Entidades.Persona();
        Entidades.Persona oPropietarioE = new Entidades.Persona();
        Entidades.Persona oMarcasSinMod = new Entidades.Persona();
        Negocio.Persona oPersonaN = new Negocio.Persona(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
        Negocio.Usuario oUsuarioN = new Negocio.Usuario(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
        public Entidades.Usuario oUsuario;
        public int idPersona = 0;
        bool huboModif = true;
        bool modificar = false;
        bool readOnly = false;

        DataTable dtIva;
        public bool modifPersonaCajaVenta = false;//se setea en TRUE para poder modificar la persona desde Caja Venta

        public formMarcaAddOrEdit()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }
        
        private void formNuevaPersona_Load(object sender, EventArgs e)
        {
            try
            {
                if (idPersona > 0)
                {
                    oMarcaE = oPersonaN.findById(idPersona);
                    oMarcasSinMod = oPersonaN.findById(idPersona);

                    cargarCampos();
                    readOnly = !modifPersonaCajaVenta; //ver descripcion en la declaracion de la var.
                    setearPropiedadesForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la carga del formulario.\n\n" + ex.Message);
            }
        }


        private void setearPropiedadesForm()
        {
            this.Text = readOnly ? "Info Marca" : "Modificar Marca";
            this.btnGuardar.Text = readOnly ? "&Modificar" : "&Guardar";

            //valido que es un nueva persona
            bool permitirModificarCampos = oMarcaE == null || oMarcaE.idPersona == 0;
            bool personaTieneCompras_Ventas = true;

            if (!permitirModificarCampos && oUsuario != null)
            {
                if (oUsuario.Admin)
                {
                    personaTieneCompras_Ventas = false;//establezco false para permitir la edicion en los campos
                    MessageBox.Show(
                        "“Al cambiar el nombre de la Marca puede afectar los registros históricos.”",
                        "Info",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                        );
                }
                else
                {
                    MessageBox.Show(
                        "“Para cambiar el nombre de la marca debe comunicarse con un administrador," +
                        " ya que puede afectar los registros históricos.”",
                        "Info",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                        );
                }
            }

            txtRazonSocial.ReadOnly = personaTieneCompras_Ventas; //readOnly;
            btnPropietario.Enabled = !readOnly;
            btnQuitarPropietario.Visible = string.IsNullOrEmpty(txtPropietario.Text) ? false : true;
            txtOtrosDatos.ReadOnly = readOnly;
        }

        private void cargarCampos()
        {
            txtRazonSocial.Text = oMarcaE.razonSocial;
            txtPropietario.Text = oMarcaE.Propietario != null ? oMarcaE.Propietario.RazonSocial : "";
            txtOtrosDatos.Text = oMarcaE.otrosDatos;
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void addOrEditPersona()
        {
            if (oMarcaE.idPersona > 0 && readOnly)
            {
                ///Se valida que la persona no sea un ID reservado por el sistema
                ///
                if (oMarcaE.idPersona.Equals(Entidades.Persona.idConsumidorFinal) || oMarcaE.idPersona.Equals(Entidades.Persona.idIndefinido))
                {
                    MessageBox.Show("La persona seleccionada es reservada por el sistema y no se puede modificar");
                    return;
                }

                if (oUsuario == null)
                {
                    FormLoginVendedor frmLogin = new FormLoginVendedor();
                    frmLogin.soloActivos = true;
                    frmLogin.ShowDialog(this);
                }

                if (oUsuario == null) return;

                readOnly = false;
                setearPropiedadesForm();
                return;
            }

            if (validar())
            {
                try
                {
                    cargarMarca();
                    
                    oPersonaN.addOrEditPersona(oMarcaE);

                    MessageBox.Show("La Marca se ha guardado correctamente.");

                    if (frmMarcas != null)
                        frmMarcas.cargarGrilla();

                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }                    
            }
        }

        private void cargarMarca()
        {
            oMarcaE.razonSocial = txtRazonSocial.Text.Trim();
            oMarcaE.Marca = true;
            oMarcaE.otrosDatos = txtOtrosDatos.Text.Trim();
        }

        private bool huboModificaciones()
        { 
            bool huboModif = true;
            if (oMarcasSinMod.idPersona == 0 || ((oMarcaE.razonSocial.Equals(oMarcasSinMod.razonSocial)) &&
                (oMarcaE.IdPropietario.Equals(oMarcasSinMod.IdPropietario)) &&
                (oMarcaE.otrosDatos.Equals(oMarcasSinMod.otrosDatos))))
                huboModif = false;

            return huboModif;
        }

        public Boolean validar()
        {
            // Palabras que NO queremos tener en cuenta
            HashSet<string> articulos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "el", "la", "los", "las", "un", "una", "unos", "unas",
                "de", "del", "en", "y", "por", "para", "con"
            };

            // DataTable final donde se acumulan los resultados
            DataTable dtMarcasParecidas = new DataTable();

            // Dividir la razón social en palabras
            string[] palabras = txtRazonSocial.Text.Split(' ');

            // Recorro cada palabra
            foreach (string palabra in palabras)
            {
                // Normalizar (quitar espacios y bajar a minúscula para comparar)
                string palabraLimpia = palabra.Trim().ToLower();

                if (!articulos.Contains(palabraLimpia))
                {
                    DataTable dtTemp = oPersonaN.existenMarcasParecidas(palabra, oMarcaE.idPersona);

                    if (dtTemp != null && dtTemp.Rows.Count > 0)
                    {
                        if (dtMarcasParecidas.Columns.Count == 0)
                            dtMarcasParecidas = dtTemp.Clone();

                        foreach (DataRow row in dtTemp.Rows)
                        {
                            string marca = row["Marca"].ToString();
                            string propietario = row["Propietario"].ToString();// == "" ? "-" : row["Propietario"].ToString();

                            bool existe = dtMarcasParecidas.AsEnumerable().Any(r =>
                                r["Marca"].ToString() == marca &&
                                r["Propietario"].ToString() == propietario);

                            if (!existe)
                                dtMarcasParecidas.ImportRow(row);
                        }
                    }
                }
            }

            if (dtMarcasParecidas.Rows.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                // Encabezados
                sb.AppendLine("Marca".PadRight(30) + "Propietario");
                sb.AppendLine(new string('-', 60));

                foreach (DataRow row in dtMarcasParecidas.Rows)
                {
                    string marca = row["Marca"].ToString();
                    string propietario = row["Propietario"].ToString();

                    sb.AppendLine(marca.PadRight(30) + propietario);
                }


                DialogResult resp = MessageBox.Show(sb.ToString() + "\n\n¿Desea guardar la Marca igualmente? ", "Marcas Parecidas",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (resp.Equals(DialogResult.No))
                    return false;
            }
            else
            {
                //MessageBox.Show("No se encontraron marcas parecidas.", "Marcas Parecidas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            bool retornar = false;

            if (Util_Form.validarCampoVacio(txtRazonSocial.Text, "Nombre Marca"))
                retornar = true;

            return retornar;            
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            //VALIDAR QUE ES ADD OR EDIT
            DialogResult resp = DialogResult.Yes;

            if ((oMarcaE == null || oMarcaE.idPersona == 0) || (oMarcaE.idPersona > 0 && !readOnly))
            {
                cargarMarca();
                if (huboModificaciones())
                    resp = MessageBox.Show("¿Está seguro de salir sin guardar las modificaciones?", "Se perderán las modificaciones",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            }

            if (resp.Equals(DialogResult.Yes))
                this.Close();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            addOrEditPersona();
        }



        private void btnInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                   "⚠️ Aviso:\n\n" +
                   "• Cualquier usuario puede crear nuevas marcas.\n" +
                   "• Solo los administradores pueden modificar el nombre" +
                   " de marcas que ya tengan ventas o compras registradas.",
                   "Restricción de permisos",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Information
               );
        }

        private void btnPropietario_Click(object sender, EventArgs e)
        {
            buscarPersona();
        }
        private void buscarPersona()
        {
            Personas.formBuscarPersona frmBuscarPersona = new Personas.formBuscarPersona();
            frmBuscarPersona.ShowDialog(this);
        }

        //comunicación con interface
        public void EnviarPersona(Entidades.Persona proveedor)
        {
            oPropietarioE = proveedor;
            cargarPropietario();
        }

        private void btnQuitarPropietario_Click(object sender, EventArgs e)
        {
            DialogResult resp = MessageBox.Show("¿Eliminar Marca?"
                    , "Atención", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (resp.Equals(DialogResult.No))
                return;

            oPropietarioE = null;
            cargarPropietario();
        }

        private void cargarPropietario()
        {
            oMarcaE.Propietario = oPropietarioE;
            this.txtPropietario.Text = oPropietarioE != null ? oPropietarioE.RazonSocial : "";
            btnQuitarPropietario.Visible = string.IsNullOrEmpty(txtPropietario.Text) ? false : true;
        }

        private void btnInfo_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show(
                   "⚠️ Aviso:\n\n" +
                   "• Cualquier usuario puede crear nuevas marcas.\n" +
                   "• Solo los administradores pueden modificar el nombre de la marca.\n\n" +
                   "• El Sistema informará cuando haya marcas parecidas al agregar una nueva.(se obvian los articulos por ej.:'el','la','los/as' entre otros)",
                   "Restricción de permisos",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Information
               );
        }
    }
}
