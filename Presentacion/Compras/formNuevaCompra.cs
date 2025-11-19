using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Caja;
using Presentacion.Cortes;
using Utilidades;


namespace Presentacion
{
    public partial class formNuevaCompra : Form, InterfacePersona, InterfaceCorte, InterfaceUsuario
    {
        DataTable dtSucursales;
        DataTable dtCortes;
        Negocio.Compra oCompraN = new Negocio.Compra();
        Negocio.Sucursal oSucursalN;
        Negocio.Corte oCorteN = new Negocio.Corte();
        public Entidades.Usuario oUsuario;
        public bool esEgresoCaja = false;
        public int idCompra = 0;
        public Entidades.EgresoCaja oEgresoCajaE;
        Entidades.Compra oCompraE = new Entidades.Compra();
        Entidades.Persona oProvNuevaCompra;
        Entidades.Corte oCorteNuevaCompra;
        Entidades.MediaRes oMediaRes;
        Entidades.CortePorCompra oCortePorCompra;
        //Entidades.CortePorCompra listaCortePorCompra;
        Entidades.Sucursal oSucursalE;
        CortesPorCompra cortesPorCompra;
        MediasPorCompra mediasPorCompra;
        float totalKgs = 0;
        float totalPesos = 0;

        string tipoCompra = "Media Res";

        List<CortesPorCompra> listaCortesEnGrilla = new List<CortesPorCompra>();//Lista que se carga en la grilla
        List<MediasPorCompra> listaMediasEnGrilla = new List<MediasPorCompra>();

        List<Entidades.MediaRes> listaMediaRes = new List<Entidades.MediaRes>();
        List<Entidades.CortePorCompra> listaCortePorCompra = new List<Entidades.CortePorCompra>();

        formCompras oFrmCompra;
        Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        bool ultimaValidacion = true;
        bool mostrarCartelCierre = true;

        public formNuevaCompra()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;

            cambiarGrupo();
            cargarComboSucursal();
        }

        #region eventos

        #region RadioButtons
        private void radioMediaRes_CheckedChanged(object sender, EventArgs e)
        {
            cambiarGrupo();
        }

        private void radioCorte_CheckedChanged(object sender, EventArgs e)
        {
            cambiarGrupo();
        }

        private void radioIngresoStock_CheckedChanged_1(object sender, EventArgs e)
        {
            cambiarGrupo();
        }


        private void radioMediaRes_Click(object sender, EventArgs e)
        {
            cambiarGrupo();
        }

        private void radioCorte_Click(object sender, EventArgs e)
        {
            cambiarGrupo();
        }

        #endregion


        private void btnBuscarProv_Click(object sender, EventArgs e)
        {
            buscarPersona();
        }

        private void buscarPersona()
        {
            Personas.formBuscarPersona frmBuscarPersona = new Personas.formBuscarPersona();
            frmBuscarPersona.ShowDialog(this);

            if (radioMediaRes.Checked)
                txtKgMedia.Select();
            else
                txtCodigo.Select();
        }

        //comunicación con interface
        public void EnviarPersona(Entidades.Persona proveedor)
        {
            oProvNuevaCompra = proveedor;
            checkCtaCte.Checked = oProvNuevaCompra.CtaCte;
            this.txtProveedor.Text = oProvNuevaCompra.razonSocial;
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
            this.txtUsuario.Text = oUsuario.Nombre;
        }

        //comunicación con interface
        public void EnviarCorte(Entidades.Corte corte)
        {
            oCorteNuevaCompra = corte;
            this.txtCorteNuevaCompra.Text = oCorteNuevaCompra.corte;
            this.txtCodigo.Text = oCorteNuevaCompra.codigo.ToString();
            this.txtCodigo.Focus();
        }

        private void btnBuscaCorte_Click(object sender, EventArgs e)
        {
            buscarCorte();
        }

        private void buscarCorte()
        {
            formBuscarCorte frmBuscarCorte = new formBuscarCorte();
            frmBuscarCorte.Show(this);
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            quitarLinea();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            agregarLinea();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            agregarCompra();
        }

        #endregion

        #region Métodos

        //se asigna el form compra para recargar la grilla del formCompras
        public void asignarFormCompra(formCompras frmCompra)
        {
            oFrmCompra = frmCompra;
        }

        private void agregarCompra()
        {
            try
            {
                //si NO es Egreso de caja y NO tiene permiso
                if (!esEgresoCaja && !(oUsuarioN.tienePermiso(oUsuario, this.Name, txtFechaCompra.Value,
                    oCompraE.IdCompra > 0 ? oCompraE.CreadoPor.Id : oUsuario.Id)))
                {
                    Utilidades.Mensajes.ErrorPermisoEdicion();
                    return;
                }

                if (listaCortePorCompra.Count > 0 || listaMediaRes.Count > 0)
                {
                    if (validaciónFinal())
                    {
                        Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
                        cargarCompra();//se cargan datos de la compra

                        if (esEgresoCaja)
                        {
                            if (!oCierreN.validarCajaAbiertaVendedor(oCompraE.FechaCompra, oCompraE.Sucursal, oUsuario))
                            {
                                MessageBox.Show("La compra seleccionada es de una fecha anterior a la apertura de su caja. No puede registrarla.","",MessageBoxButtons.OK,MessageBoxIcon.Error);
                                return;
                            }
                        }

                        mostrarCartelCierre = false;

                        oCompraE.IdCompra = oCompraN.AddOrEditCompra(oCompraE, tipoCompra, listaMediaRes, listaCortePorCompra, esEgresoCaja, oEgresoCajaE);

                        MessageBox.Show("La Compra se registró correctamente.");
                        //if (oFrmCompra != null) oFrmCompra.cargarGrilla();
                        this.Close();
                        //limpiarListas();
                        //oCompraE.IdCompra = 0;
                        //txtFechaCompra.Focus();
                        //mostrarCartelCierre = true;
                    }
                }
                else
                {
                    MessageBox.Show("No hay cargada ninguna linea de compra.", "No hay lineas cargadas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void imprimirTicket(Entidades.EgresoCaja oEgresoCajaE)
        {
            try
            {
                Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
                oEgresoCajaE = oCierreN.getEgresoCajaById(oEgresoCajaE.Id);
                //imprimir ticket
                Ticket.CreaTicket ticket = new Ticket.CreaTicket();
                ticket.imprimir = true;//checkTicket.Checked;
                ticket.TextoCentro("Egreso Caja");
                ticket.LineasEnBlanco(1);
                //ticket.TextoIzquierda("123456789*123456789*123456789*123456789*123456789*");
                ticket.TextoIzquierda("Sucursal: " + oEgresoCajaE.Sucursal.sucursal);
                ticket.TextoIzquierda("Vendedor: " + oEgresoCajaE.CreadoPorUser.Nombre);
                ticket.TextoIzquierda("Id: " + oEgresoCajaE.Id.ToString());
                ticket.TextoIzquierda("Fecha: " + Utilidades.Util_Form.fechaFormato24Horas(oEgresoCajaE.Fecha));
                ticket.LineasGuion();
                ticket.TextoIzquierda("Tipo: " + oEgresoCajaE.TipoEgresoCaja);
                ticket.TextoMuchasLineas("Descripción: " + oEgresoCajaE.Descripcion);
                ticket.TextoIzquierda("Monto: " + oEgresoCajaE.Monto);
                ticket.TextoMuchasLineas("Detalle: " + oEgresoCajaE.Detalle);
                DateTime? creado = oEgresoCajaE.Id.Equals(0) ? DateTime.Now : oEgresoCajaE.Creado;
                ticket.TextoIzquierda("Creado: " + Utilidades.Util_Form.fechaFormato24Horas(creado));
                if (oEgresoCajaE.Actualizado != null) ticket.TextoIzquierda("Modif.: " + Utilidades.Util_Form.fechaFormato24Horas(oEgresoCajaE.Actualizado));
                ticket.LineasEnBlanco(5);
                ticket.realizarImpresion();
            }
            catch (Exception)
            {
                MessageBox.Show("Error al imprimir el Ticket");
                return;
            }
        }

        private void limpiarListas()
        {
            //limpio campos
            txtPrecioKg.Text = "";
            txtCantMedias.Text = "";
            txtNroRemito.Text = "";
            txtObservaciones.Text = "";

            txtFechaCompra.Select();

            listaCortesEnGrilla = new List<CortesPorCompra>();//Lista que se carga en la grilla
            listaMediasEnGrilla = new List<MediasPorCompra>();

            listaMediaRes = new List<Entidades.MediaRes>();
            listaCortePorCompra = new List<Entidades.CortePorCompra>();

            grillaMediaRes.DataSource = null;
            grillaCortePorCompra.DataSource = null;

        }


        private void cargarCompra()
        {
            oCompraE.NroRemito = txtNroRemito.Text.Trim();
            oCompraE.Proveedor = oProvNuevaCompra;
            oCompraE.FechaCompra = txtFechaCompra.Value;
            oCompraE.Estado = "";
            oCompraE.CantMedias = string.IsNullOrEmpty(txtCantMedias.Text) ? null : (int?)Convert.ToInt32(txtCantMedias.Text);
            oCompraE.KgsMedias = string.IsNullOrEmpty(txtTotalKg.Text) || tipoCompra == "Cortes" ? null :
                (float?)Utilidades.Util_Form.convertFloat(txtTotalKg.Text, false, false); //(int?)Convert.ToInt32(txtTotalKg.Text);
            oCompraE.Observaciones = txtObservaciones.Text.Trim();
            oCompraE.TipoCompra = tipoCompra;// FormPrincipal.soyYo ? tipoCompra : (oCompraE.CantMedias == null ? "Cortes" : "Media Res");
            oCompraE.Sucursal = oSucursalE;
            oCompraE.EnCtaCte = checkCtaCte.Checked;
            switch (oCompraE.IdCompra)
            {
                case 0:
                    oCompraE.CreadoPor = oUsuario;
                    break;
                default:
                    oCompraE.ActualizadoPor = oUsuario;
                    break;
            }
        }

        private void quitarLinea()
        {
            if (tipoCompra == "Media Res")
            {
                quitarMedia();
            }
            if (tipoCompra == "Cortes" || tipoCompra == "Ingreso Stock")
            {
                quitarCorte();
            }

            cargarGrilla();
            //si ambas listas no contienen objetos se habilitan los radioButtons
            if (listaCortePorCompra.Count() == 0 && listaMediaRes.Count() == 0)
            {
                radioMediaRes.Enabled = true;
                radioCorte.Enabled = true;
                comboSucursal.Enabled = true;
            }
        }

        private void quitarCorte()
        {
            try
            {
                if (grillaCortePorCompra.SelectedRows.Count > 0 || grillaCortePorCompra.CurrentRow != null)
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
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void quitarMedia()
        {
            try
            {
                if (grillaMediaRes.SelectedRows.Count > 0 || grillaMediaRes.CurrentRow != null)
                {
                    int nroFila = grillaMediaRes.Rows.GetFirstRow(DataGridViewElementStates.Selected);//obtiene nro de fila de la grilla
                    listaMediasEnGrilla.RemoveAt(nroFila);//elimina objetos de las listas
                    listaMediaRes.RemoveAt(nroFila);
                }
                else
                {
                    MessageBox.Show("No hay ninguna fila seleccionada.", "Seleccione un fila", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void agregarLinea()
        {
            if (validarCampos())
            {
                if (tipoCompra == "Media Res")
                {
                    agregarMediaRes();
                    txtKgMedia.Focus();
                }
                if (tipoCompra == "Cortes" || tipoCompra == "Ingreso Stock")
                {
                    agregarCorte();
                    txtCodigo.Focus();
                }

                limpiarCampos();

            }
        }

        private void agregarCorte()
        {
            cargarCortesPorCompra();
            cargarGrilla();
        }

        //carga textBox de totales
        private void cargarTotales()
        {
            totalKgs = 0;
            totalPesos = 0;

            if (tipoCompra == "Cortes" || tipoCompra == "Ingreso Stock")
            {
                foreach (CortesPorCompra fila in listaCortesEnGrilla)
                {
                    //sumo totales
                    totalKgs = totalKgs + fila.cantKgs;
                    totalPesos = totalPesos + fila.totalS;

                    //cargo Totales
                    txtTotalKg.Text = Convert.ToString(totalKgs);
                    txtTotal.Text = Convert.ToString(totalPesos);
                    txtCantItems.Text = grillaCortePorCompra.Rows.Count.ToString();

                }
            }

            if (tipoCompra == "Media Res")
            {
                foreach (MediasPorCompra fila in listaMediasEnGrilla)
                {
                    totalKgs = totalKgs + fila.kgMedia;
                    totalPesos = totalPesos + fila.totalS;

                    //cargo Totales
                    txtTotalKg.Text = Convert.ToString(totalKgs);
                    txtTotal.Text = Convert.ToString(totalPesos);
                    txtCantItems.Text = grillaMediaRes.Rows.Count.ToString();

                }
            }

        }

        private int validarCorteEnGrilla()
        {
            int nroFila = -1;//si Producto no está cargado
            foreach (Entidades.CortePorCompra corte in listaCortePorCompra)
            {

                if (corte.corte.idCorte == oCorteNuevaCompra.idCorte && corte.sucursal.IdSucursal == Convert.ToInt32(comboSucursal.SelectedValue.ToString()))
                {
                    DialogResult resp = MessageBox.Show("Ya se ha cargado el Producto en la sucursal seleccionada.\nDesea sumarlo al Producto ya ingresado?.", "Producto ya cargado en la sucursal", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                    if (resp == DialogResult.Yes)
                    {
                        nroFila = listaCortePorCompra.IndexOf(corte);//se envía el index de la lista para sumar los kg al corte ya ingresado
                    }
                    else
                    {
                        nroFila = -2;//si está cargado y no se quiere volver a cargar
                    }
                }
            }

            return nroFila; ;
        }


        //cargar Cortes y Grilla
        private void cargarCortesPorCompra()
        {
            try
            {
                ultimaValidacion = true;
                //creo y Cargar la Entidad CortePorCompra
                oCortePorCompra = new Entidades.CortePorCompra();

                oCortePorCompra.corte = oCorteNuevaCompra;
                cargarCompra();//cargo datos en oCompraE
                oCortePorCompra.compra = oCompraE;
                try
                {
                    oCortePorCompra.cantKgs = Util_Form.convertFloat(txtCantKgs.Text, false,true); //float.Parse(txtCantKgs.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                    oCortePorCompra.precioKg = Util_Form.convertFloat(txtPrecioKg.Text, false, false); //float.Parse(txtPrecioKg.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));

                }
                catch (Exception)
                {
                    try
                    {
                        oCortePorCompra.Desc_recargo = float.Parse(txtBoxDescuento.Text.Trim());
                        oCortePorCompra.Iva_compra = float.Parse(txtIva.Text.Trim());
                        oCortePorCompra.cantKgs = float.Parse(txtCantKgs.Text.Trim());
                        oCortePorCompra.precioKg = float.Parse(txtPrecioKg.Text.Trim());
                    }
                    catch (Exception)
                    {

                        ultimaValidacion = false;
                    }
                }

                if (ultimaValidacion)
                {
                    oSucursalE = new Entidades.Sucursal(); //creo objeto sucursal
                    oSucursalE.IdSucursal = Convert.ToInt32(comboSucursal.SelectedValue.ToString());

                    if (oSucursalE.IdSucursal == 1)
                    {
                        oSucursalE.SucursalNombre = "San Lorenzo";
                    }
                    else
                    {
                        oSucursalE.SucursalNombre = "San Martín";
                    }

                    oCortePorCompra.sucursal = oSucursalE;
                    oCortePorCompra.Creado = DateTime.Now;
                    oCortePorCompra.CreadoPor = oUsuario;
                    oCortePorCompra.Margen = string.IsNullOrEmpty(txtMargenGan.Text) ? 0f : float.Parse(txtMargenGan.Text.Trim());
                    oCortePorCompra.PrecioVenta = string.IsNullOrEmpty(txtPrecioFinalVenta.Text) ? 0f : float.Parse(txtPrecioFinalVenta.Text.Trim());

                    listaCortePorCompra.Add(oCortePorCompra);

                    //creo CortesPorCompra y cargo la lista de la grilla
                    cortesPorCompra = new CortesPorCompra();

                    cortesPorCompra.codigo = oCortePorCompra.corte.codigo;
                    cortesPorCompra.corte = oCortePorCompra.corte.corte;
                    cortesPorCompra.Desc_recargo = oCortePorCompra.Desc_recargo;
                    cortesPorCompra.Iva_compra = oCortePorCompra.Iva_compra;
                    cortesPorCompra.cantKgs = oCortePorCompra.cantKgs;
                    cortesPorCompra.precioKg = oCortePorCompra.precioKg;
                    cortesPorCompra.totalS = oCortePorCompra.precioKg * cortesPorCompra.cantKgs;
                    cortesPorCompra.sucursal = oCortePorCompra.sucursal.SucursalNombre;
                    cortesPorCompra.margen = oCortePorCompra.Margen;
                    cortesPorCompra.precioVenta = oCortePorCompra.PrecioVenta;

                    listaCortesEnGrilla.Add(cortesPorCompra);

                    oCortePorCompra = null;
                    cortesPorCompra = null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void agregarMediaRes()
        {
            cargarMediasPorCompra();

            cargarGrilla();
        }


        private void cargarMediasPorCompra()
        {
            ultimaValidacion = true;
            //creo y Cargar la Entidad MediaRes
            oMediaRes = new Entidades.MediaRes();

            cargarCompra();//cargo datos en oCompraE
            oMediaRes.compra = oCompraE;
            oMediaRes.nroTropa = txtNroTropa.Text.Trim();

            try
            {
                oMediaRes.kgMedia = Util_Form.convertFloat(txtKgMedia.Text, false, true); // float.Parse(txtKgMedia.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                oMediaRes.precioMedia = Util_Form.convertFloat(txtPrecioKg.Text, false, false);// float.Parse(txtPrecioKg.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
            }
            catch (Exception)
            {
                try
                {
                    oMediaRes.kgMedia = float.Parse(txtKgMedia.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                    oMediaRes.precioMedia = float.Parse(txtPrecioKg.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));

                }
                catch (Exception)
                {
                    MessageBox.Show("Verifique que ha ingresado números en los campos 'Kg Media' y 'Precio'.", "Error de ingreso de datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ultimaValidacion = false;
                }
            }

            if (ultimaValidacion)
            {
                oSucursalE = new Entidades.Sucursal(); //creo objeto sucursal
                oSucursalE.IdSucursal = Convert.ToInt32(comboSucursal.SelectedValue.ToString());
                if (oSucursalE.IdSucursal == 1)
                {
                    oSucursalE.SucursalNombre = "San Lorenzo";
                }
                else
                {
                    oSucursalE.SucursalNombre = "San Martín";
                }

                oMediaRes.sucursal = oSucursalE;
                listaMediaRes.Add(oMediaRes);

                //creo MediasPorCompra y cargo la lista de la grilla
                mediasPorCompra = new MediasPorCompra();

                mediasPorCompra.nroTropa = oMediaRes.nroTropa;
                mediasPorCompra.kgMedia = oMediaRes.kgMedia;
                mediasPorCompra.precioMedia = oMediaRes.precioMedia;
                mediasPorCompra.totalS = oMediaRes.kgMedia * oMediaRes.precioMedia;
                mediasPorCompra.sucursal = oMediaRes.sucursal.SucursalNombre;

                listaMediasEnGrilla.Add(mediasPorCompra);

                oMediaRes = null;
                mediasPorCompra = null;

            }

        }

        private bool validarCampos()
        {
            if (comboSucursal.SelectedIndex.Equals(-1))
            {
                MessageBox.Show("Ingrese la sucursal.", "Ingresar sucursal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            else
            {
                //validacion para que sólo salte el cartel al presionar el boton -
                if (radioCorte.Checked && (oCorteNuevaCompra == null || oCorteNuevaCompra.idCorte == 0))
                {
                    MessageBox.Show("No existe el Producto", "No existe el Producto", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtCodigo.Focus();
                    return false;
                }

                if (txtPrecioKg.Text.Equals("") || ((txtKgMedia.Text.Equals("") || txtKgMedia.Text.Equals("")) &&
                 (txtCorteNuevaCompra.Text.Equals("") || txtCantKgs.Text.Equals(""))))
                {
                    MessageBox.Show("Debe Completar todos los campos.", "Complete los campos vacíos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (txtPrecioKg.Text.Equals("") && !checkCalcularPrecio.Checked)
                        txtPrecioKg.Focus();
                    else
                        txtBoxPrecioNeto.Focus();

                    if (txtCantKgs.Text.Equals("") && tipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes))) txtCantKgs.Focus();
                    if (txtKgMedia.Text.Equals("") && tipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes))) txtKgMedia.Focus();

                    return false;
                }
                else
                {
                    //si precio venta es menor a compra
                    if (txtMargenGan.Text.Contains("-"))
                    {
                        MessageBox.Show("El Precio Venta no puede ser menor a Precio Compra (el margen de ganancia es negativo)", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (checkManualPrecio.Checked)
                            txtPrecioFinalVenta.Focus();
                        else
                            txtMargenGan.Focus();
                        return false;
                    }

                    return true;
                }
            }
        }

        private bool validaciónFinal()
        {
            if (tipoCompra == "Media Res" && string.IsNullOrEmpty(txtCantMedias.Text))
            {
                MessageBox.Show("Ingrese la cantidad de medias.", "Complete el campo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCantMedias.Focus();
                return false;
            }

            if (txtProveedor.Text.Equals(""))
            {
                MessageBox.Show("Debe ingresar un Proveedor.", "Complete el campo Proveedor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            else
            {
                DialogResult respuesta;
                respuesta = MessageBox.Show("Vefique que la Fecha, Sucursal y demás datos ingresados esté correctos.\n\n¿Están correctos?.", "Verificar datos ingresados", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (respuesta == DialogResult.Yes)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                // return true;
            }
        }

        private void limpiarCampos()
        {
            if (checkCodigo.Checked)
            {
                txtCantKgs.Text = "";
                txtCantKgs.Focus();
                return;
            }
            //txtNroTropa.Text="";
            txtKgMedia.Text = "";
            oCorteNuevaCompra = null;
            txtCodigo.Text = "";
            txtCorteNuevaCompra.Text = "";
            txtBoxPrecioNeto.Text = "";
            txtPrecioKg.Text = radioCorte.Checked ? "" : txtPrecioKg.Text;
            txtCantKgs.Text = "";
            txtPrecioFinalVenta.Text = "";
        }

        private void cargarGrilla()
        {
            try
            {
                if (tipoCompra == "Media Res")
                {
                    grillaMediaRes.AutoGenerateColumns = false;
                    grillaMediaRes.DataSource = null;
                    grillaMediaRes.DataSource = listaMediasEnGrilla;

                    if (listaMediasEnGrilla.Count > 0)
                    {
                        grillaMediaRes.Rows[listaMediasEnGrilla.Count - 1].Selected = true;
                        grillaMediaRes.FirstDisplayedScrollingRowIndex = listaMediasEnGrilla.Count - 1;
                    }
                }

                if (tipoCompra == "Cortes" || tipoCompra == "Ingreso Stock")
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
                validarListas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void validarListas()
        {
            if (listaCortePorCompra.Count() > 0)
            {
                radioMediaRes.Enabled = false;
                radioCorte.Enabled = true;
                comboSucursal.Enabled = false;
            }

            if (listaMediaRes.Count() > 0)
            {
                radioMediaRes.Enabled = true;
                radioCorte.Enabled = false;
                comboSucursal.Enabled = false;
            }

            //si ambas listas no contienen objetos se habilitan los radioButtons
            if (listaCortePorCompra.Count() == 0 && listaMediaRes.Count() == 0)
            {
                radioMediaRes.Enabled = true;
                radioCorte.Enabled = true;
                comboSucursal.Enabled = true;
            }
        }

        private void cambiarGrupo()
        {
            limpiarCampos();


            if (radioMediaRes.Checked == true)
            {
                panelCorte.Visible = false;

                grupoMediaRes.Text = "Media Res";

                tipoCompra = "Media Res";
                grillaMediaRes.Visible = true;
                grillaCortePorCompra.Visible = false;

                txtNroTropa.TabStop = true;
                txtKgMedia.TabStop = true;

                txtCantKgs.TabStop = false;
                txtPrecioKg.TabStop = false;
                groupCantMedias.Visible = true;
            }

            if (radioCorte.Checked == true)
            {
                panelCorte.Visible = true;
                grupoMediaRes.Text = "Producto ";

                tipoCompra = "Cortes";

                grillaMediaRes.Visible = false;
                grillaCortePorCompra.Visible = true;

                txtNroTropa.TabStop = false;
                txtKgMedia.TabStop = false;

                txtCantKgs.TabStop = true;
                txtPrecioKg.TabStop = true;
                groupCantMedias.Visible = false;
            }
        }

        private void cargarCorte()
        {
            try
            {
                txtCorteNuevaCompra.Text = "";
                oCorteNuevaCompra = null;
                oCorteNuevaCompra = new Entidades.Corte();

                if (txtCodigo.Text.Trim() != "")
                {

                    DataTable dtCorte = new DataTable();

                    dtCorte = oCorteN.buscarCodigoCorte(Convert.ToInt64(txtCodigo.Text.Trim()));

                    if (dtCorte.Rows.Count > 0)
                    {
                        foreach (DataRow fila in dtCorte.Rows)
                        {
                            oCorteNuevaCompra.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                            oCorteNuevaCompra.codigo = Convert.ToInt64(fila["codigo"].ToString());
                            oCorteNuevaCompra.corte = fila["corte"].ToString();

                            txtPrecioActualCorte.Text = float.TryParse(fila["precioKg"].ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result) ? result.ToString("N2") : fila["precioKg"].ToString();
                        }

                        //se cargan los datos del corte
                        txtCorteNuevaCompra.Text = oCorteNuevaCompra.corte;
                    }
                    else
                    {
                        //MessageBox.Show("El código no existe");
                        txtCodigo.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarComboSucursal()
        {
            dtSucursales = new DataTable();
            oSucursalN = new Negocio.Sucursal();
            dtSucursales = oSucursalN.obtenerSucursales();
            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";

            //comboSucursal.SelectedIndex = -1;//San Martín
        }

        #endregion

        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
                {
                if (txtPrecioKg.Focused)
                {
                    if (checkMargen.Checked)
                    {
                        txtMargenGan.Focus();
                        return;
                    }
                    if (checkManualPrecio.Checked)
                    {
                        txtPrecioFinalVenta.Focus();
                        return;
                    }
                }

                if (txtMargenGan.Focused || txtPrecioFinalVenta.Focused)
                {
                    btnAgregar.Focus();
                    return;
                }

                if (radioCorte.TabStop)
                {
                    radioCorte.TabStop = false;
                }
                //si txtBoxPrecio es vacio se mueve el foco a éste
                if ((txtKgMedia.Focused || txtCantKgs.Focused) && txtPrecioKg.Text.Equals("") && 
                    ((checkCalcularPrecio.Checked && !string.IsNullOrEmpty(txtBoxPrecioNeto.Text)) ||
                    (!checkCalcularPrecio.Checked)) )
                {

                    txtPrecioKg.Focus();
                    return;
                }
                e.Handled = true;

                SendKeys.Send("{TAB}");
            }
        }

        /// <summary>
        /// Actualizar precio Venta
        /// </summary>
        /// <returns></returns>
        private void actualizarPrecioVenta()
        {

        }


        //Métodos autocompletar
        public AutoCompleteStringCollection LoadAutoComplete()
        {
            dtCortes = oCorteN.buscarCorteSinMaestro(txtCorteNuevaCompra.Text.Trim());

            AutoCompleteStringCollection cortes = new AutoCompleteStringCollection();

            foreach (DataRow fila in dtCortes.Rows)
            {
                //cortes.Add(fila["codigo"].ToString());
                cortes.Add(fila["corte"].ToString());

                //crear un corte nuevo y lo envia al metodo EnviarCorte                
                if (txtCorteNuevaCompra.Text.Trim() == fila["corte"].ToString())
                {
                    Entidades.Corte oCorteNuevoE = new Entidades.Corte();
                    oCorteNuevoE.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                    oCorteNuevoE.codigo = Convert.ToInt64(fila["codigo"].ToString());
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

        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            cargarCorte();
        }

        private void btnAceptar_Enter(object sender, EventArgs e)
        {
            btnAceptar.BackColor = System.Drawing.Color.FromName("DarkSeaGreen");
        }

        private void btnAceptar_Leave(object sender, EventArgs e)
        {
            btnAceptar.BackColor = System.Drawing.Color.FromName("SeaGreen");
        }

        private void formNuevaCompra_Load(object sender, EventArgs e)
        {
            if (oUsuario == null)
            {
                FormLoginVendedor frmLogin = new FormLoginVendedor();
                frmLogin.soloActivos = true;
                frmLogin.ShowDialog(this);
            }

            if (oUsuario == null)
            {
                this.Close();
                return;
            }

            if (!oUsuarioN.tienePermiso(oUsuario, this.Name, txtFechaCompra.Value,
                        oCompraE != null && oCompraE.IdCompra > 0 ? oCompraE.CreadoPor.Id : oUsuario.Id))
            {
                Utilidades.Mensajes.ErrorPermisoEdicion();
                this.Close();
                return;
            }

            this.Text += Utilidades.Conexion.getSucursalConexion();
            txtUsuario.Text = oUsuario.Nombre;
            cargarSucursal();            

            checkCtaCte.Checked = false;
            checkCtaCte.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkCtaCte.Checked);
            radioCorte.Checked = esEgresoCaja;
            radioMediaRes.Enabled = !esEgresoCaja;

            ///si no soy yo -> NO se muestra el 
            ///
            if (!FormPrincipal.soyYo)
            {
                radioCorte.Checked = true;
                //panelCorte.Visible = true;
                groupBoxTipoCompra.Visible = false;
                groupCantMedias.Visible = true;
            }

            if (idCompra > 0)
            {
                oCompraE = oCompraN.findById_convertToCompra(idCompra);
                listaCortePorCompra = oCompraN.convertCortesPorCompraToList(idCompra);

                oSucursalE = oCompraE.Sucursal;
                oProvNuevaCompra = oCompraE.Proveedor;
                comboSucursal.SelectedValue = oSucursalE.idSucursal;
                txtFechaCompra.Value = oCompraE.FechaCompra;
                txtProveedor.Text = oCompraE.Proveedor.razonSocial;
                txtNroRemito.Text = oCompraE.NroRemito;
                checkCtaCte.Checked = oCompraE.EnCtaCte;
                txtObservaciones.Text = oCompraE.Observaciones;
                txtCreado.Text = Util_Form.fechaFormato24Horas(oCompraE.Creado);
                txtCreadoPor.Text = oCompraE.CreadoPor != null ? oCompraE.CreadoPor.Nombre : "-";
                txtActualizado.Text = oCompraE.Actualizado != null ? Util_Form.fechaFormato24Horas(oCompraE.Actualizado) : "-";
                txtActualizadoPor.Text = oCompraE.ActualizadoPor != null ? oCompraE.ActualizadoPor.Nombre : "-";

                foreach (Entidades.CortePorCompra corte in listaCortePorCompra)
                {
                    cargarCorteEnGrilla(corte);
                }
                cargarGrilla();
            }


            //se valida que sea Admin para cambiar de sucursal
            comboSucursal.Visible = ((oUsuario != null && oUsuario.Admin) || FormPrincipal.logueado);
            txtSucursal.Visible = !comboSucursal.Visible;

            btnBuscarProv.Select();
        }

        private void cargarCorteEnGrilla(Entidades.CortePorCompra oCortePorCompra)
        {
            cortesPorCompra = new CortesPorCompra();

            cortesPorCompra.Index = oCortePorCompra.IdCortePorCompra;
            cortesPorCompra.codigo = oCortePorCompra.corte.codigo;
            cortesPorCompra.corte = oCortePorCompra.corte.corte;
            cortesPorCompra.cantKgs = oCortePorCompra.cantKgs;
            cortesPorCompra.precioKg = oCortePorCompra.precioKg;
            cortesPorCompra.totalS = oCortePorCompra.precioKg * cortesPorCompra.cantKgs;
            cortesPorCompra.sucursal = oCortePorCompra.sucursal.SucursalNombre;
            cortesPorCompra.Creado = oCortePorCompra.Creado;

            listaCortesEnGrilla.Add(cortesPorCompra);

            oCortePorCompra = null;
            cortesPorCompra = null;
        }

        private void cargarSucursal()
        {
            int idSucursal = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());
            oSucursalE = oSucursalN.findById(idSucursal);
            oCompraE.Sucursal = oSucursalE;
            txtSucursal.Text = oCompraE.Sucursal.sucursal;

            comboSucursal.DataSource = oSucursalN.obtenerSucursales();
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedValue = idSucursal;
        }

        private void comboSucursal_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!comboSucursal.ValueMember.Equals("") && comboSucursal.SelectedValue != null)
            {
                int idSucursal = (int)comboSucursal.SelectedValue;
                oSucursalE = oSucursalN.findById(idSucursal);
                oCompraE.Sucursal = oSucursalE;
                comboSucursal.SelectedValue = oCompraE.Sucursal.idSucursal;
                txtSucursal.Text = oCompraE.Sucursal.sucursal;
            }
        }

        private void txtNumerico_TextChanged(object sender, EventArgs e)
        {
            //permitir valor negativo en margen ganancia
            TextBox txt = sender as TextBox;
            bool aceptarNegativo = txt != null && txt.Name == "txtMargenGan";

            //validarImporte(sender, aceptarNegativo);
            bool esImporte = txt.Name != "txtCantKgs";

            if (esImporte)
                validarImporte(sender, aceptarNegativo);

            if (sender is TextBox)
            {
                TextBox txtNumerico = (TextBox)sender;
                if (!validarCampoNumerico(txtNumerico.Text, txtNumerico.Name)) txtNumerico.Text = "";
                CalcularPrecioFinal();
                return;
            }

            if (sender is MaskedTextBox)
            {
                MaskedTextBox txtNumerico = (MaskedTextBox)sender;
                if (!validarCampoNumerico(txtNumerico.Text, txtNumerico.Name)) txtNumerico.Text = "";
                CalcularPrecioFinal();
                return;
            }
        }


        private bool validarCampoNumerico(string valor, string nombreTextBox)
        {
            return string.IsNullOrEmpty(valor) ? true : Utilidades.Util_Form.validarCampoNumerico(valor, "El valor");
        }

        private void txtCantMedias_TextChanged(object sender, EventArgs e)
        {
            if (!Utilidades.Util_Form.validarCampoNumeroEntero(txtCantKgs.Text, "Cant. Medias"))
                txtCantMedias.Text = "";
        }

        private void formNuevaCompra_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = salir();
        }

        private bool salir()
        {
            //Si no se ingresaron datos en los sig. campos no se muestra cartel de cierre
            if (txtProveedor.Text.Equals("") && txtNroRemito.Text.Equals("") && txtObservaciones.Text.Equals("")
                && ((radioCorte.Checked && grillaCortePorCompra.Rows.Count == 0) ||
                (radioMediaRes.Checked && grillaMediaRes.Rows.Count == 0))) mostrarCartelCierre = false;

            if (!mostrarCartelCierre)
            {
                //oFrmCompra.EnviarUsuario(null);
                oUsuario = null;
                return false;
            }

            DialogResult respuesta = MessageBox.Show("Si cierra el formulario se perderan las modificaciones realizadas.\n¿Está seguro que desea salir?. ", "Compras", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if ((respuesta == System.Windows.Forms.DialogResult.Yes))
            {
                //oFrmCompra.EnviarUsuario(null);
                oUsuario = null;
                return false;
            }
            else
            {
                return true;
            }
        }

        private void checkCtaCte_CheckedChanged(object sender, EventArgs e)
        {
            checkCtaCte.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkCtaCte.Checked);
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
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
                case Keys.F9:
                    buscarPersona();
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

        private void checkCodigo_CheckedChanged(object sender, EventArgs e)
        {
            if (checkCodigo.Checked)
            {
                MessageBox.Show("Ha activado la función para que el Producto seleccionado quede fijo");
            }
        }

        private void checkAplicarDescIva_CheckedChanged(object sender, EventArgs e)
        {
            panelCalcularPrecio.Enabled = checkCalcularPrecio.Checked;
            txtPrecioKg.ReadOnly = checkCalcularPrecio.Checked;
            if (checkCalcularPrecio.Checked)
            {
                txtBoxPrecioNeto.Focus();
                txtBoxPrecioNeto.SelectAll();
            }
            else
            {
                txtBoxPrecioNeto.Text = "";
                txtBoxDescuento.Text = txtIva.Text = "0";
                txtPrecioKg.Focus();
                txtPrecioKg.SelectAll();
            }
        }

        private void calcularPrecioFinal()
        {
            if (validarCampoNumerico(txtBoxPrecioNeto.Text, "$") && validarCampoNumerico(txtBoxDescuento.Text,"%") &&
                validarCampoNumerico(txtIva.Text,"Iva") )
            {
                txtPrecioKg.Text = ((Util_Form.convertFloat(txtBoxPrecioNeto.Text, false, false) *
                    ((100 + Util_Form.convertFloat(txtBoxDescuento.Text, false, false)) / 100)) * (1 + (Util_Form.convertFloat(txtIva.Text, false, false) / 100))).ToString("F2");
            }
        }

        private void txtBoxPrecioNeto_TextChanged(object sender, EventArgs e)
        {
            validarImporte(sender, true);

            if (string.IsNullOrEmpty(txtBoxPrecioNeto.Text) ||
                (txtBoxDescuento.Text.Length == 1 && txtBoxDescuento.Text.Equals("-")))
                return;
            if (string.IsNullOrEmpty(txtBoxDescuento.Text) || string.IsNullOrEmpty(txtBoxDescuento.Text))
            {
                txtPrecioKg.Text = "";
                return;
            }

            calcularPrecioFinal();
        }

        private void checkManualPrecio_CheckedChanged(object sender, EventArgs e)
        {
            checkMargen.Checked =  false;
            panelPrecioVenta.Enabled = checkManualPrecio.Checked || checkMargen.Checked;
            txtPrecioFinalVenta.TabStop = checkManualPrecio.Checked;
            txtPrecioFinalVenta.ReadOnly = !checkManualPrecio.Checked;
            txtPrecioFinalVenta.Text = txtMargenGan.Text = "";
            txtPrecioFinalVenta.Focus();
            txtPrecioFinalVenta.SelectAll();
        }

        private void checkMargen_CheckedChanged(object sender, EventArgs e)
        {
            checkManualPrecio.Checked = false;
            panelPrecioVenta.Enabled = checkManualPrecio.Checked || checkMargen.Checked;
            txtMargenGan.TabStop = checkMargen.Checked;
            txtMargenGan.ReadOnly = !checkMargen.Checked;
            txtPrecioFinalVenta.Text = txtMargenGan.Text = "";
            txtMargenGan.Focus();
            txtMargenGan.SelectAll();  
        }

        private void txtPrecioFinalVenta_TextChanged(object sender, EventArgs e)
        {
            validarImporte(sender);
            CalcularMargen();
        }

        private void CalcularMargen()
        {
            if (!(checkManualPrecio.Checked))
                return;

            // Validar que ambos TextBox contengan valores numéricos
            //if (float.TryParse(txtPrecioKg.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float precioKg) &&
            //    float.TryParse(txtPrecioFinalVenta.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float precioFinalVenta))
            float precioKg = string.IsNullOrEmpty(txtPrecioKg.Text) ? 0f : Util_Form.convertFloat(txtPrecioKg.Text, false, false);
            float precioFinalVenta = string.IsNullOrEmpty(txtPrecioFinalVenta.Text) ? 0f : Util_Form.convertFloat(txtPrecioFinalVenta.Text, false, false);
            if (precioKg > 0 && precioFinalVenta > 0)
            {
                // Realizar la multiplicación
                float margenGan = ((precioFinalVenta / precioKg) - 1) * 100;

                txtMargenGan.Text = margenGan.ToString("0.00"); // Formato con 2 decimales
            }
            else
            {
                // Mostrar mensaje de error o limpiar el TextBox de resultado
                if (string.IsNullOrEmpty(txtPrecioFinalVenta.Text))
                {
                    txtMargenGan.Text = "";
                    return;
                }

                MessageBox.Show("Ingrese valores numéricos válidos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPrecioFinalVenta.Text = string.Empty;
                txtMargenGan.Text = string.Empty;
            }
        }
        private void CalcularPrecioFinal()
        {
            //Calcular el total del producto Cant * precioFinal
            try
            {
                float cant = Util_Form.convertFloat(txtCantKgs.Text, false, true); // float.TryParse(txtCantKgs.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float cant);
                float precioKgIncluido = Util_Form.convertFloat(txtPrecioKg.Text, false, false);
                txtTotalProd.Text = (precioKgIncluido * cant).ToString("0.00");

                if (FormPrincipal.soyYo && radioMediaRes.Checked &&
                    float.TryParse(txtKgMedia.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float cantKgMedia))
                {
                    txtTotalProd.Text = (precioKgIncluido * cantKgMedia).ToString("0.00");
                }

                if (!(checkMargen.Checked && (txtPrecioKg.Focused || txtMargenGan.Focused)))
                    return;

                // Validar que ambos TextBox contengan valores numéricos
                //if (float.TryParse(txtPrecioKg.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float precioKg) &&
                //    float.TryParse(txtMargenGan.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float margenGan))
                float precioKg = string.IsNullOrEmpty(txtPrecioKg.Text) ? 0f : Util_Form.convertFloat(txtPrecioKg.Text, false, false);
                float margenGan = string.IsNullOrEmpty(txtMargenGan.Text) ? 0f : Util_Form.convertFloat(txtMargenGan.Text, false, false);
                if (precioKg > 0 && margenGan > 0)
                {
                    // Realizar la multiplicación
                    float precioFinalVenta = precioKg * (1 + (margenGan / 100));

                    // Mostrar el resultado en txtPrecioFinalVenta
                    txtPrecioFinalVenta.Text = precioFinalVenta.ToString("0.00"); // Formato con 2 decimales
                    //txtTotalProd.Text = (precioFinalVenta * cant).ToString("0.00");                
                }
                else
                {
                    // Mostrar mensaje de error o limpiar el TextBox de resultado
                    //MessageBox.Show("Ingrese valores numéricos válidos en ambos campos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPrecioFinalVenta.Text = string.Empty;
                }

            }
            catch (Exception)
            {
                txtTotalProd.Text = "-";
            }
        }

        private void txtBox_Enter(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }
        private static void validarImporte(object sender, bool aceptarNegativo = false)
        {
            TextBox txt = sender as TextBox;
            Util_Form.ValidarImporte((TextBox)sender, aceptarNegativo);
        }

        private void txtTotalProd_TextChanged(object sender, EventArgs e)
        {
            validarImporte(sender);
        }
    }
}
