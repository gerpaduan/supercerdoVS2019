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

namespace Presentacion.Embutidos
{
    public partial class formElegirEmbutido : Form, InterfaceUsuario
    {

        public formEmbutidos frmEmbutidos = new formEmbutidos();
        public bool esDesarmeElaborado = false;
        Negocio.Sucursal oSucursalN;
        Negocio.Corte oCorteN = new Negocio.Corte(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);

        Entidades.Embutido oEmbutidoE = new Entidades.Embutido();
        public Entidades.Usuario oUsuario;

        Entidades.Usuario oUsuarioNuevoEmbutido;


        public formElegirEmbutido()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formElegirEmbutido_Load(object sender, EventArgs e)
        {
            try
            {
                if (oUsuario == null)
                {
                    this.Close();
                }
                else
                {
                    txtUsuario.Text = oUsuario.Nombre;
                    this.Text += Utilidades.Conexion.getSucursalConexion();

                    grillaEmbutidos.DataSource = oCorteN.getListaElegirEmbutido();

                    for (int i = 0; i < grillaEmbutidos.Columns.Count; i++)
                    {
                        grillaEmbutidos.Columns[i].Visible = false;
                    }
                    grillaEmbutidos.Columns["corteEmbutido"].Visible = true;
                    grillaEmbutidos.Columns["corteEmbutido"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    grillaEmbutidos.Columns["corteEmbutido"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuarioNuevoEmbutido = usuario;
        }

        private void grillaEmbutidos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int idCorteEmbutido = Convert.ToInt32(grillaEmbutidos.CurrentRow.Cells["idCorteEmbutido"].Value.ToString());
                //int idCorteEn = Convert.ToInt32(grillaEmbutidos.CurrentRow.Cells["idCorteEn"].Value.ToString());
                //int idCorteEn2 = Convert.ToInt32(grillaEmbutidos.CurrentRow.Cells["idCorteEn2"].Value.ToString());

                formIngresoEmbutidoRapido frmIngresarEmbutidoRapido = new formIngresoEmbutidoRapido();
                frmIngresarEmbutidoRapido.oUsuario = oUsuario;
                frmIngresarEmbutidoRapido.oCorteEmbutidoE = oCorteN.findCorteById(idCorteEmbutido, false);
                frmIngresarEmbutidoRapido.esDesarmeElaborado = esDesarmeElaborado;
                //frmIngresarEmbutidoRapido.oCorteE = oCorteN.findCorteById(idCorteEn, false);//corte en embutido
                //frmIngresarEmbutidoRapido.oCorteE2 = oCorteN.findCorteById(idCorteEn2, false);//corte en embutido
                frmIngresarEmbutidoRapido.frmEmbutidos = this.frmEmbutidos;
                this.Close();
                frmIngresarEmbutidoRapido.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

    }
}
