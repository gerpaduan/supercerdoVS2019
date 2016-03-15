using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Cortes;

namespace Presentacion.Compras
{
    public partial class formModificarCompra : Form, InterfaceProveedor, InterfaceCorte
    {
        Negocio.Compra oCompraN = new Negocio.Compra();
        Negocio.Corte oCorteN = new Negocio.Corte();
        string estadoModificar;
        bool modificado = false;
        DateTime fechaModificar;//borrar
        formCompras frmCompras;
        Entidades.Compra oCompraModificada = new Entidades.Compra();
        Entidades.Persona oProvNuevaCompra=new Entidades.Persona();
        Entidades.Corte oCorteNuevaCompra;
        Entidades.Sucursal oSucursal = new Entidades.Sucursal();
        DataTable dtSucursales;
        DataTable dtCortes;
        DataTable dtLineasCompra=new DataTable();
        Negocio.Sucursal oSucursalN;
        CortesPorCompra cortePorCompra;
        MediasPorCompra mediaPorCompra;

        List<CortesPorCompra> listaCortesEnGrilla = new List<CortesPorCompra>();//Lista que se carga en la grilla
        List<MediasPorCompra> listaMediasEnGrilla = new List<MediasPorCompra>();

        Entidades.CortePorCompra oCortePorCompraE;
        Entidades.MediaRes oMediaResE;
        List<Entidades.MediaRes> listaMediaRes = new List<Entidades.MediaRes>();
        List<Entidades.CortePorCompra> listaCortePorCompra = new List<Entidades.CortePorCompra>();

        //listas que se obtienen antes de modificar la compra
        List<Entidades.MediaRes> listaMediaResAnterior = new List<Entidades.MediaRes>();
        List<Entidades.CortePorCompra> listaCortePorCompraAnterior = new List<Entidades.CortePorCompra>();

        public formModificarCompra()
        {
            InitializeComponent();
            cargarComboSucursal();
        }

        private void formModificarCompra_Load(object sender, EventArgs e)
        {
            //cargarLista();
            //cargarGrilla();
        }

        //Se carga la lista inicial desde la base de datos
        public void cargarLista()
        {
            if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes)))
            {                
                dtLineasCompra = oCompraN.obtenerMediasPorCompra(oCompraModificada.IdCompra);
                foreach (DataRow  fila in dtLineasCompra.Rows)
                {
                    mediaPorCompra = new MediasPorCompra();
                    mediaPorCompra.idMedia=Convert.ToInt32(fila["idMedia"].ToString());
                    mediaPorCompra.nroTropa = fila["nroTropa"].ToString();
                    mediaPorCompra.kgMedia =float.Parse( fila["kgMedia"].ToString());
                    mediaPorCompra.precioMedia = float.Parse(fila["precioMedia"].ToString());
                    mediaPorCompra.totalS = float.Parse(fila["totalS"].ToString());
                    mediaPorCompra.idSucursal = Convert.ToInt32(fila["idSucursal"].ToString());
                    mediaPorCompra.sucursal = fila["sucursal"].ToString();

                    cargarMediaRes(mediaPorCompra);
                    listaMediasEnGrilla.Add(mediaPorCompra);
                    mediaPorCompra = null;
                }
            }

            if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes))||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.IngresoStock)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.EgresoStock)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock)))
            {
                dtLineasCompra = oCompraN.obtenerCortesPorCompra(oCompraModificada.IdCompra);

                foreach (DataRow fila in dtLineasCompra.Rows)
                {
                    cortePorCompra = new CortesPorCompra();
                    cortePorCompra.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                    cortePorCompra.codigo = Convert.ToInt32(fila["codigo"].ToString());
                    cortePorCompra.corte = fila["corte"].ToString();
                    cortePorCompra.cantKgs = float.Parse(fila["cantKg"].ToString());
                    cortePorCompra.precioKg = float.Parse(fila["precioKg"].ToString());
                    cortePorCompra.totalS = float.Parse(fila["totalS"].ToString());
                    cortePorCompra.idSucursal = Convert.ToInt32(fila["idSucursal"].ToString());
                    cortePorCompra.sucursal = fila["sucursal"].ToString();

                    int nroFila = -1;
                    cargarCortePorCompra(nroFila,cortePorCompra);

                    listaCortesEnGrilla.Add(cortePorCompra);
                    cortePorCompra = null;                    
                }
            }        
        }

        private void cargarCortePorCompra(int nroFila, CortesPorCompra cortePorCompra)
        {
            oCortePorCompraE = new Entidades.CortePorCompra();

            //creo objeto compra y lo asigno
            Entidades.Compra oCompraE = new Entidades.Compra();
            oCortePorCompraE.compra = oCompraE;
            oCortePorCompraE.compra.IdCompra = oCompraModificada.IdCompra;
            
            //creo objeto corte y lo asigno
            Entidades.Corte oCorteE= new Entidades.Corte();
            oCortePorCompraE.corte = oCorteE;
            oCortePorCompraE.corte.idCorte = cortePorCompra.idCorte;            
            oCortePorCompraE.cantKgs = cortePorCompra.cantKgs;
            oCortePorCompraE.precioKg = cortePorCompra.precioKg;

            Entidades.Sucursal sucursalCorte = new Entidades.Sucursal();
            oCortePorCompraE.sucursal = sucursalCorte;
            oCortePorCompraE.sucursal.IdSucursal = cortePorCompra.idSucursal;

            if (nroFila==-1)
            {
                listaCortePorCompra.Add(oCortePorCompraE);

                //Si no se establece la modificación 
                if (modificado == false)
                {
                    listaCortePorCompraAnterior.Add(oCortePorCompraE);
                }
   
            }
            if (nroFila>-1)
            {
                listaCortePorCompra.RemoveAt(nroFila);
                listaCortePorCompra.Insert(nroFila, oCortePorCompraE);                
            }
            oCortePorCompraE = null;
 
        }

        private void cargarMediaRes(MediasPorCompra mediaResCompra)
        {
            oMediaResE = new Entidades.MediaRes();

            //creo objeto compra y lo asigno
            Entidades.Compra oCompraE = new Entidades.Compra();
            oMediaResE.compra = oCompraE;
            oMediaResE.compra.IdCompra = oCompraModificada.IdCompra;
            oMediaResE.idMedia = mediaResCompra.idMedia;
            oMediaResE.nroTropa = mediaResCompra.nroTropa;
            oMediaResE.kgMedia = mediaResCompra.kgMedia;
            oMediaResE.precioMedia = mediaResCompra.precioMedia;

            Entidades.Sucursal sucursalMedia = new Entidades.Sucursal();
            oMediaResE.sucursal = sucursalMedia;
            oMediaResE.sucursal.IdSucursal = mediaResCompra.idSucursal;

            listaMediaRes.Add(oMediaResE);
            listaMediaResAnterior.Add(oMediaResE);

            oMediaResE = null;
        
        }

        private void cargarGrilla()
        {
            try
            {
                if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes)))
                {
                    grillaMediaRes.AutoGenerateColumns = false;
                    grillaMediaRes.DataSource = null;
                    grillaMediaRes.DataSource = listaMediasEnGrilla;
                    if (listaCortesEnGrilla.Count > 0)
                    {
                        grillaMediaRes.Rows[listaMediasEnGrilla.Count - 1].Selected = true;
                        grillaMediaRes.FirstDisplayedScrollingRowIndex = listaMediasEnGrilla.Count - 1;
                    }
                }

                if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.IngresoStock)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.EgresoStock)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock)))
                {
                    grillaCortePorCompra.AutoGenerateColumns = false;
                    grillaCortePorCompra.DataSource = null;
                    grillaCortePorCompra.DataSource = listaCortesEnGrilla;
                    if (listaCortesEnGrilla.Count > 0)
                    {
                        grillaCortePorCompra.Rows[listaCortesEnGrilla.Count - 1].Selected = true;
                        grillaCortePorCompra.FirstDisplayedScrollingRowIndex = listaCortesEnGrilla.Count - 1;                        
                    }
                }

                cargarTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarTotales()
        {
            float totalKgs = 0, totalPesos = 0;
            if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.IngresoStock)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.EgresoStock)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock)))
            {
                foreach (CortesPorCompra fila in listaCortesEnGrilla)
                {
                    //sumo totales
                    totalKgs = totalKgs + fila.cantKgs;
                    totalPesos = totalPesos + fila.totalS;
                    txtCantItems.Text = grillaCortePorCompra.Rows.Count.ToString();
                }
            }

            if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes)))
            {
                foreach (MediasPorCompra fila in listaMediasEnGrilla)
                {
                    totalKgs = totalKgs + fila.kgMedia;
                    totalPesos = totalPesos + fila.totalS;
                    txtCantItems.Text = grillaMediaRes.Rows.Count.ToString();
                }
            }            
            txtTotalKgs.Text = Convert.ToString(totalKgs);
            txtTotalS.Text = Convert.ToString(totalPesos);
        }

        public void cargarParametros(formCompras frmComprasParam, int idCompra)
        {
            frmCompras=frmComprasParam;
            oCompraModificada = oCompraN.findById_convertToCompra(idCompra);
            cargarCampos();
        }

        private void cargarComboSucursal()
        {
            dtSucursales = new DataTable();
            oSucursalN = new Negocio.Sucursal();
            dtSucursales = oSucursalN.obtenerSucursales();
            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
        }

        public void cargarCampos()
        {
            oSucursal = oCompraModificada.Sucursal;
            oProvNuevaCompra = oCompraModificada.Proveedor;

            comboSucursal.SelectedValue = oSucursal.idSucursal;
            txtNroRemito.Text = oCompraModificada.NroRemito;
            txtProveedor.Text = oCompraModificada.Proveedor.razonSocial;
            txtFechaCompra.Value = oCompraModificada.FechaCompra;
            txtObservaciones.Text = oCompraModificada.Observaciones;
            string datosCreado = "Creado: " + oCompraModificada.Creado.ToString() + "\nModificado: " +
                (oCompraModificada.Actualizado > DateTime.Today.AddYears(-20) ? oCompraModificada.Actualizado.ToString() : "-");
            txtCreado.Text = datosCreado;
            establecerTipo(oCompraModificada.TipoCompra);

            validarEstado();
            cargarLista();
            cargarGrilla();
        }

        private void validarEstado()
        {
            if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes))||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.IngresoStock)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.EgresoStock)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock)))
            {
                quitarStock.Enabled = false;
                PorcentajesCorte.Enabled = false;
            }
            else
            {
                quitarStock.Enabled = true;
            }
            if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes)))
            {
                PorcentajesCorte.Enabled = true;

                if (!modificado)
                {
                    cambiarPrecio.Enabled = true;
                }                
            }
        }

        private void habilitarModificacion()
        {
            this.Text = "Modificar Compra";
            btnAgregar.Visible = true;
            btnQuitar.Visible = true;
            btnBuscarProv.Visible = true;
            btnBuscaCorte.Visible = true;
            txtNroRemito.Enabled = true;
            txtNroRemito.ReadOnly = false;
            modificar.Enabled = false;
            cambiarPrecio.Enabled = false;
            txtFechaCompra.Enabled = true;
            comboSucursal.Enabled = true;
            btnAceptar.Visible = true;
            txtFechaCompra.Value = oCompraModificada.FechaCompra;
            txtObservaciones.ReadOnly = false;

            if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes)))
            {
                
            }
            else
            {
                radioCorte.Enabled = true;
                radioIngresoStock.Enabled = true;
            }

        }

        private void establecerTipo(string tipoCompra)
        {
            if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes)))//(tipoCompra=="Media Res")
            {
                radioMediaRes.Checked = true;
                panelCorte.Visible = false;
                panelCorte.Enabled = false;

                grupoMediaRes.Text = oCompraModificada.TipoCompra;//"Media Res";

                //tipoCompra = "media";
                grillaMediaRes.Visible = true;
                grillaCortePorCompra.Visible = false;

                radioMediaRes.Enabled = true;
                radioCorte.Enabled = false;
                radioIngresoStock.Enabled = false;
                radioMediaRes.TabStop = false;
            }
            if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes)))//(tipoCompra=="Cortes")
            {
                radioCorte.Checked = true;
                panelCorte.Visible = true;
                grupoMediaRes.Text = "Corte ";

                //tipoCompra = "Cortes";

                grillaMediaRes.Visible = false;
                grillaCortePorCompra.Visible = true;

                radioMediaRes.Enabled = false;
                radioCorte.Enabled = true;
                radioIngresoStock.Enabled = false;
                radioCorte.TabStop = false;

                txtNroTropa.TabStop = false;
                txtKgMedia.TabStop = false;
            }

            if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.IngresoStock)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.EgresoStock)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock)))
            {
                radioIngresoStock.Checked = true;
                panelCorte.Visible = true;
                grupoMediaRes.Text = "Corte ";

                tipoCompra = oCompraModificada.TipoCompra;

                grillaMediaRes.Visible = false;
                grillaCortePorCompra.Visible = true;

                radioIngresoStock.TabStop = false;

                radioMediaRes.Enabled = false;
                radioCorte.Enabled = false;
                radioCorte.TabStop = false;

                txtNroTropa.TabStop = false;
                txtKgMedia.TabStop = false;
            }
        }


        private void cargarDatosEnCampos()
        {
            if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes)) && grillaMediaRes!=null)
            {
                txtNroTropa.Text =  grillaMediaRes.CurrentRow.Cells[1].Value.ToString();
                txtKgMedia.Text = grillaMediaRes.CurrentRow.Cells["kgMedia"].Value.ToString();
                txtPrecioKg.Text = grillaMediaRes.CurrentRow.Cells["precioMedia"].Value.ToString();
                comboSucursal.SelectedIndex = Convert.ToInt32(grillaMediaRes.CurrentRow.Cells["idSucursalMedia"].Value.ToString())-1;
              
            }
            if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes)))
            {

                
            }
        }

        private void agregarLinea()
        {
            if (validarCampos())
            {
                if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes)))
                {
                    agregarMediaRes();
                    txtCantKgs.Focus();
                }
                if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.IngresoStock)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.EgresoStock)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock)))
                {
                    agregarCorte();
                    txtCodigo.Focus();
                }
                modificado = true;
                limpiarCampos();
            }
        }

        private void limpiarCampos()
        {
            //txtNroTropa.Text="";
            txtKgMedia.Text = "";
            oCorteNuevaCompra = null;
            txtCodigo.Text = "";
            txtCorteNuevaCompra.Text = "";
            //txtPrecioKg.Text = "";
            txtCantKgs.Text = "";
            //comboSucursal.SelectedText = "";


        }

     //cargar Cortes y Grilla
        private void cargarCortesPorCompra()
        {
            try
            {
                //creo y Cargar la Entidad CortePorCompra

                cortePorCompra = new CortesPorCompra();

                cortePorCompra.idCorte = oCorteNuevaCompra.idCorte;
                cortePorCompra.codigo = oCorteNuevaCompra.codigo;
                cortePorCompra.corte = oCorteNuevaCompra.corte;

                try
                {
                    cortePorCompra.cantKgs = float.Parse(txtCantKgs.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                }
                catch (Exception)
                {

                    cortePorCompra.cantKgs = float.Parse(txtCantKgs.Text.Trim());
                }
                try
                {
                    cortePorCompra.precioKg = float.Parse(txtPrecioKg.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                }
                catch (Exception)
                {

                    cortePorCompra.precioKg = float.Parse(txtPrecioKg.Text.Trim());
                }

                cortePorCompra.totalS = cortePorCompra.cantKgs * cortePorCompra.precioKg;


                Entidades.Sucursal oSucursalE = new Entidades.Sucursal(); //creo objeto sucursal
                oSucursalE.IdSucursal = Convert.ToInt32(comboSucursal.SelectedValue.ToString());

                if (oSucursalE.IdSucursal == 1)
                {
                    oSucursalE.SucursalNombre = "San Lorenzo";
                }
                else
                {
                    oSucursalE.SucursalNombre = "San Martín";
                }

                cortePorCompra.idSucursal = oSucursalE.IdSucursal;
                cortePorCompra.sucursal = oSucursalE.SucursalNombre;

                int nroFila = validarCorteEnGrilla();
                //si no está cargado
                if (nroFila== -1)
                {
                    listaCortesEnGrilla.Add(cortePorCompra);
                    cargarCortePorCompra(nroFila, cortePorCompra);
                    cortePorCompra = null;
                }

                //si no se desea sumar kg al corte ya ingresado
                if (nroFila == -2)
                {
                    cortePorCompra = null;
                }

                //sumar los kg al corte ya ingresado
                if (nroFila > -1)
                {
                    listaCortesEnGrilla[nroFila].cantKgs = listaCortePorCompra[nroFila].cantKgs + cortePorCompra.cantKgs;
                    listaCortesEnGrilla[nroFila].totalS = listaCortesEnGrilla[nroFila].totalS + (cortePorCompra.cantKgs * cortePorCompra.precioKg);

                    cortePorCompra.cantKgs = listaCortesEnGrilla[nroFila].cantKgs;
                    cortePorCompra.totalS = listaCortesEnGrilla[nroFila].totalS;

                    cargarCortePorCompra(nroFila, cortePorCompra);
                    cortePorCompra = null;
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private void agregarCorte()
        {
            cargarCortesPorCompra();
            cargarGrilla();
        }

        private int validarCorteEnGrilla()
        {
            int nroFila=-1;//si corte no está cargado
            foreach (Entidades.CortePorCompra corte in listaCortePorCompra)
            {

                if (corte.corte.idCorte == oCorteNuevaCompra.idCorte && corte.sucursal.IdSucursal == Convert.ToInt32(comboSucursal.SelectedValue.ToString()))
                {
                    DialogResult resp= MessageBox.Show("Ya se ha cargado el corte en la sucursal seleccionada.\nDesea sumarlo al Corte ya ingresado?.", "Corte ya cargado en la sucursal", MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2);

                    if (resp==DialogResult.Yes)
                    {
                        nroFila=listaCortePorCompra.IndexOf(corte);//se envía el index de la lista para sumar los kg al corte ya ingresado
                    }
                    else
                    {
                        nroFila = -2;//si está cargado y no se quiere volver a cargar
                    }
                }
            }
            return nroFila; ;
        }

   

        private void agregarMediaRes()
        {
            cargarMediasPorCompra();
            cargarGrilla();
        }

        private void cargarMediasPorCompra()
        {
            //creo y Cargar la Entidad MediaRes            
            mediaPorCompra = new MediasPorCompra();

            mediaPorCompra.nroTropa = txtNroTropa.Text.Trim();

            try
            {
                mediaPorCompra.kgMedia = float.Parse(txtKgMedia.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                mediaPorCompra.precioMedia = float.Parse(txtPrecioKg.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));

            }
            catch (Exception)
            {
                try
                {
                    mediaPorCompra.kgMedia = float.Parse(txtKgMedia.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                    mediaPorCompra.precioMedia = float.Parse(txtPrecioKg.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                }
                catch (Exception)
                {
                    MessageBox.Show("Verifique que ha ingresado datos correctos en los campos 'Kg Media' y 'Precio'.", "Error de ingreso de datos", MessageBoxButtons.OK, MessageBoxIcon.Information);              
                }
            }
            
            
            mediaPorCompra.totalS = mediaPorCompra.kgMedia * mediaPorCompra.precioMedia;

            Entidades.Sucursal oSucursalE = new Entidades.Sucursal(); //creo objeto sucursal
            oSucursalE.IdSucursal = Convert.ToInt32(comboSucursal.SelectedValue.ToString());
            if (oSucursalE.IdSucursal == 1)
            {
                oSucursalE.SucursalNombre = "San Lorenzo";
            }
            else
            {
                oSucursalE.SucursalNombre = "San Martín";
            }

            mediaPorCompra.idSucursal = oSucursalE.IdSucursal;
            mediaPorCompra.sucursal = oSucursalE.SucursalNombre;

            listaMediasEnGrilla.Add(mediaPorCompra);

            cargarMediaRes(mediaPorCompra);
            
            mediaPorCompra = null;

        }

        private bool validarCampos()
        {
            if (txtProveedor.Text.Equals("") || comboSucursal.SelectedIndex.Equals(-1) || txtPrecioKg.Text.Equals("") || ((txtKgMedia.Text.Equals("") || txtKgMedia.Text.Equals("")) &&
                 (txtCorteNuevaCompra.Text.Equals("") || txtCantKgs.Text.Equals(""))))
            {
                MessageBox.Show("Debe Completar todos los campos.", "Complete los campos vacíos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            else
            {
                return true;
            }
        }

        private void cargarDatosCompraModificada()
        {
            oCompraModificada.NroRemito = txtNroRemito.Text.Trim();
            oCompraModificada.FechaCompra = txtFechaCompra.Value;
            
            oCompraModificada.Sucursal = oSucursal;
            oCompraModificada.Proveedor = oProvNuevaCompra;
            oCompraModificada.Estado = "";//Vuelvo a cargar el stock- Estado=" "
            oCompraModificada.Observaciones = txtObservaciones.Text.Trim();

            if (radioIngresoStock.Checked==true)
            {
                //oCompraModificada.TipoCompra = "Ingreso Stock";
            }
            if (radioCorte.Checked==true)
            {
                oCompraModificada.TipoCompra = Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes);//"Cortes";
            }
            oCompraModificada.TipoCompra = oCompraModificada.TipoCompra;
        }

        private void modificarCompra()
        {        
            if (modificado == true && Utilidades.Util_Form.validarFecha(txtFechaCompra.Value, "Fecha"))
            {
                 DialogResult respuesta = MessageBox.Show("¿Está seguro que desea guardar los cambios realizados?. ", "Modificar Compras", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                 if (respuesta == DialogResult.Yes)
                 {  
                    cargarDatosCompraModificada();
                    oCompraN.modificarCompra(oCompraModificada);

                    if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes)))
                    {
                        foreach (Entidades.MediaRes mediaRes in listaMediaResAnterior)
                        {
                            if (estadoModificar == "Stock Borrado")
                            {
                                oCompraN.quitarStockTeoricoMedia(mediaRes, oCompraModificada.IdCompra);
                            }
                            if (estadoModificar == "")
                            {
                                oCompraN.quitarStockMedia(mediaRes, oCompraModificada.IdCompra);
                                oCompraN.quitarStockTeoricoMedia(mediaRes, oCompraModificada.IdCompra);
                            }
                        }
                        foreach (Entidades.MediaRes mediaRes in listaMediaRes)
                        {
                            mediaRes.sucursal = oSucursal;
                            oCompraN.agregarMedias(mediaRes);
                        }
                    }

                    if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes)) ||
                        oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.IngresoStock)) ||
                        oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.EgresoStock)) ||
                        oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock)))
                    {

                        foreach (Entidades.CortePorCompra cortePorCompra in listaCortePorCompraAnterior)
                        {
                            oCompraN.quitarStockCorte(cortePorCompra, oCompraModificada.IdCompra);
                        }
                        foreach (Entidades.CortePorCompra cortePorCompra in listaCortePorCompra)
                        {
                            cortePorCompra.sucursal = oSucursal;
                            oCompraN.agregarCortePorCompra(cortePorCompra);
                        }
                    }
                     //se establece el estado a vacío
                    estadoModificar = "";
                    validarEstado();
                    frmCompras.cargarGrilla();
                    modificado = false;
                    this.Close();
                 }
            }
            else
            {
                if (!modificado)
                {
                    this.Close();                    
                }
            }        
        }

        private void quitarStockMedia()
        {
            DialogResult respuesta = MessageBox.Show("¿Está seguro que desea quitar el stock correspondiente a la compra?", "Quitar Stock", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (respuesta == System.Windows.Forms.DialogResult.Yes)
            {
                cargarDatosCompraModificada();                
                estadoModificar = "Stock Borrado";

                oCompraModificada.Estado = estadoModificar;
                oCompraN.modificarCompra(oCompraModificada);

                if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes)))
                {
                    foreach (Entidades.MediaRes mediaRes in listaMediaResAnterior)
                    {                       
                       oCompraN.quitarStockMedia(mediaRes, oCompraModificada.IdCompra);
                    }
                }

                validarEstado();
                frmCompras.cargarGrilla();
            }
        }

        private void quitarLinea()
        {
            try
            {
                if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes)))
                {
                    quitarMedia();
                }
                if (oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.IngresoStock)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.EgresoStock)) ||
                oCompraModificada.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock)))
                {
                    quitarCorte();
                }
                modificado = true;
                cargarGrilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }

        private void quitarMedia()
        {
            if (grillaMediaRes.SelectedRows.Count > 0)
            {
                //int nroFila = grillaMediaRes.CurrentRow.Index;//obtiene nro de fila de la grilla
                int nroFila = grillaMediaRes.Rows.GetFirstRow(DataGridViewElementStates.Selected);
                listaMediasEnGrilla.RemoveAt(nroFila);//elimina objetos de las listas
                listaMediaRes.RemoveAt(nroFila);
            }
            else
            {
                MessageBox.Show("No hay ninguna fila seleccionada.", "Seleccione un fila", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void quitarCorte()
        {
            if (grillaCortePorCompra.SelectedRows.Count > 0)
            {
                int nroFila = grillaCortePorCompra.Rows.GetFirstRow(DataGridViewElementStates.Selected);//obtiene nro de fila de la grilla
                listaCortePorCompra.RemoveAt(nroFila);//elimina objetos de las listas
                listaCortesEnGrilla.RemoveAt(nroFila);
            }
            else
            {
                MessageBox.Show("No hay ninguna fila seleccionada.", "Seleccione un fila", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        public void verPorcentajesPorCompra()
        {
            formPorcentajeCortesCompra frmPorcentajesPorCompra = new formPorcentajeCortesCompra(oCompraModificada.IdCompra);
            frmPorcentajesPorCompra.Show();
        
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            agregarLinea();
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            quitarLinea();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            modificarCompra();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void grillaMediaRes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
           cargarDatosEnCampos();
        }

        private void btnBuscarProv_Click(object sender, EventArgs e)
        {
            formBuscarProveedor frmBuscarProv = new formBuscarProveedor();
            frmBuscarProv.Show(this);
        }

        //comunicación con interface
        public void EnviarProveedor(Entidades.Persona proveedor)
        {
            oProvNuevaCompra = proveedor;
            txtProveedor.Text = oProvNuevaCompra.razonSocial;
            modificado = oCompraModificada.Proveedor.idPersona.Equals(oProvNuevaCompra.idPersona) ? false : true;
        }

        //comunicación con interface
        public void EnviarCorte(Entidades.Corte corte)
        {
            //oCorteNuevaCompra = new Entidades.Corte();
            oCorteNuevaCompra = corte;
            this.txtCorteNuevaCompra.Text = oCorteNuevaCompra.corte;
        }
        
        private void btnBuscaCorte_Click(object sender, EventArgs e)
        {
            formBuscarCorte frmBuscarCorte = new formBuscarCorte();
            frmBuscarCorte.Show(this);
            
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            habilitarModificacion();
        }


        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                e.Handled = true;
                SendKeys.Send("{TAB}");
            }
        }

        private void quitarStock_Click(object sender, EventArgs e)
        {
            quitarStockMedia();
        }

        private void cargarCorte()
        {
            try
            {
                if (txtCodigo.Text.Trim() != "")
                {
                    oCorteNuevaCompra = null;
                    oCorteNuevaCompra = new Entidades.Corte();

                    dtCortes = null;
                    DataTable dtCorte = new DataTable();

                    dtCorte = oCorteN.buscarCodigoCorte(Convert.ToInt32(txtCodigo.Text.Trim()));

                    if (dtCorte.Rows.Count > 0)
                    {
                        foreach (DataRow fila in dtCorte.Rows)
                        {
                            oCorteNuevaCompra.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                            oCorteNuevaCompra.codigo = Convert.ToInt32(fila["codigo"].ToString());
                            oCorteNuevaCompra.corte = fila["corte"].ToString();
                        }
                        //se cargan los datos del corte
                        txtCorteNuevaCompra.Text = oCorteNuevaCompra.corte;
                    }
                    else
                    {
                        txtCorteNuevaCompra.Text = "";
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

        //Métodos autocompletar
        public AutoCompleteStringCollection LoadAutoComplete()
        {
            dtCortes = oCorteN.buscarCorteSinMaestro(txtCorteNuevaCompra.Text.Trim());

            AutoCompleteStringCollection cortes = new AutoCompleteStringCollection();

            foreach (DataRow fila in dtCortes.Rows)
            {
                cortes.Add(fila["corte"].ToString());

                //crear un corte nuevo y lo envia al metodo EnviarCorte
                if (txtCorteNuevaCompra.Text.Trim() == fila["corte"].ToString())
                {
                    Entidades.Corte oCorteNuevoE = new Entidades.Corte();
                    oCorteNuevoE.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                    oCorteNuevoE.codigo = Convert.ToInt32(fila["codigo"].ToString());
                    oCorteNuevoE.corte = fila["corte"].ToString();

                    EnviarCorte(oCorteNuevoE);

                }
            }
            return cortes;
        }

        private void txtCorteNuevaCompra_TextChanged(object sender, EventArgs e)
        {
            txtCorteNuevaCompra.AutoCompleteCustomSource = LoadAutoComplete();
        }

        private void cargarReporte()
        {
            int tipoReporte=4, idSucursal=2;

            formReporteStock frmReporte = new formReporteStock();
            frmReporte.obtenerParametros(idSucursal, oCompraModificada.FechaCompra, fechaModificar, tipoReporte, oCompraModificada.NroRemito);
            frmReporte.Show();
        }

        private void PorcentajesCorte_Click(object sender, EventArgs e)
        {
            verPorcentajesPorCompra();
        }

        private void Reporte_Click(object sender, EventArgs e)
        {
            cargarReporte();
        }

        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            cargarCorte();
        }

        private void cambiarPrecio_Click(object sender, EventArgs e)
        {
            formCambiarPrecioKg frmCambiarPrecioKg = new formCambiarPrecioKg(this, oCompraModificada.IdCompra);
            frmCambiarPrecioKg.ShowDialog();

        }

        public void actualizarListas()//llamado desde modificar Precio Media
        {
           listaMediasEnGrilla = null;
           listaMediasEnGrilla = new List<MediasPorCompra>();

           listaMediaRes = null;
           listaMediaRes = new List<Entidades.MediaRes>();

           listaMediaResAnterior = null;
           listaMediaResAnterior = new List<Entidades.MediaRes>();

           cargarLista();
           cargarGrilla();
           frmCompras.cargarGrilla();
                        
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboSucursal.ValueMember.Equals(""))
            {
                oSucursal = new Entidades.Sucursal();
                oSucursal.idSucursal = (int)comboSucursal.SelectedValue;
                if (oSucursal.idSucursal.Equals(oCompraModificada.Sucursal.idSucursal))
                {
                    modificado = false;
                }
                else
                {
                    modificado = true;
                }
            }
        }

        private void txtFechaCompra_ValueChanged(object sender, EventArgs e)
        {
            if (txtFechaCompra.Value.Equals(oCompraModificada.FechaCompra))
            {
                modificado = false;
            }
            else
            {
                modificado = true;
            }
        }

        private void txtObservaciones_TextChanged(object sender, EventArgs e)
        {
            if (txtObservaciones.Text.Equals(oCompraModificada.Observaciones))
            {
                modificado = false;
            }
            else
            {
                modificado = true;
            }
        }

        private void formModificarCompra_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = salir();
        }

        private bool salir()
        {
            if (modificado)
            {
                DialogResult respuesta = MessageBox.Show("Si cierra el formulario se perderan las modificaciones realizadas.\n¿Está seguro que desea salir?. ", "Compras", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

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
    }
}
