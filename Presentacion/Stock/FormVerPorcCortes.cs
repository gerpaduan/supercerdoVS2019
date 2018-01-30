using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Stock
{
    public partial class FormVerPorcCortes : Form
    {
        public int idPesaje = 0;
        int idAjuste = 0;

        Negocio.Compra oCompraN = new Negocio.Compra();
        Entidades.Compra oPesajeE;
        Entidades.Compra oAjusteE;

        Entidades.Compra.estadoAjusteStock estadoAjuste;
        public formAddOrEditStock frmPesaje;

        public FormVerPorcCortes()
        {
            InitializeComponent();
        }

        private void FormVerPorcCortes_Load(object sender, EventArgs e)
        {
            try
            {
                setearEstado();

                grillaPromMedias.DataSource = oCompraN.getPromMedias(idPesaje);
                grillaPorcCortes.DataSource = oCompraN.getPorcCortesEnMedias(idPesaje);

                for (int colum = 2; colum < grillaPorcCortes.Columns.Count; colum++)
                {
                    grillaPorcCortes.Columns[colum].DefaultCellStyle.Format = "F3";
                }
                grillaPorcCortes.Columns["idCorte"].Visible = false;

                //Formateando Fila de Totales
                float gan = 0;
                for (int fila = 0; fila < grillaPorcCortes.Rows.Count; fila++)
                {
                    if (fila == grillaPorcCortes.Rows.Count - 1)
                    {
                        for (int colum = 0; colum < grillaPorcCortes.Columns.Count; colum++)
                        {
                            if (grillaPorcCortes.Columns[colum].Name == "Gan.")
                            {
                                grillaPorcCortes[colum, (grillaPorcCortes.Rows.Count - 1)].Value = gan.ToString("F3");
                            }
                            if (grillaPorcCortes.Columns[colum].Name == "Codigo")
                            {
                                grillaPorcCortes[colum, (grillaPorcCortes.Rows.Count - 1)].Value = DBNull.Value;
                            }
                        }
                    }
                    else
                    {
                        gan += float.Parse(grillaPorcCortes["Gan.", fila].Value.ToString());
                    }
                }
                //Fin Formateando filas Totales

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnGenerarAj_Click(object sender, EventArgs e)
        {
            ///en el Load del form fijarse si el Pesaje ya tiene ajuste e informarlo

            try
            {
                cargarCompra();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarCompra()
        {
            try
            {                
                //se estable el IdPesaje al NroRemito del Ajuste para su identificacion
                oAjusteE.NroRemito = oPesajeE.IdCompra.ToString();
                oAjusteE.Proveedor = oPesajeE.Proveedor;
                oAjusteE.FechaCompra = oPesajeE.FechaCompra;
                oAjusteE.Estado = "";
                oAjusteE.Observaciones = "";
                oAjusteE.TipoCompra = Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.AjusteStock);
                oAjusteE.CantMedias = oPesajeE.CantMedias;
                oAjusteE.KgsMedias = oPesajeE.KgsMedias;
                oAjusteE.Sucursal = oPesajeE.Sucursal;

                switch (oAjusteE.IdCompra)
                {
                    case 0:
                        oAjusteE.CreadoPor = oPesajeE.CreadoPor;
                        oAjusteE.IdCompra = oCompraN.agregarCompra(oAjusteE);
                        break;
                    default:
                        oAjusteE.ActualizadoPor = oPesajeE.ActualizadoPor;
                        oCompraN.modificarCompra(oAjusteE);
                        break;
                }

                cargarCortes();
                oCompraN.actualizarEstadoPesaje(oPesajeE.IdCompra, Entidades.Compra.estadoAjusteStock.Actualizado);
                MessageBox.Show("El Ajuste de Stock se realizó correctamente!.");
                setearEstado();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarCortes()
        {
            try
            {
                Entidades.CortePorCompra oCortePorCompra;
                
                for (int i = 0; i < grillaPorcCortes.Rows.Count; i++)
                {
                    //Se valida que no sea la fila de total verificando q el valor sea nulo
                    if (grillaPorcCortes.Rows[i].Cells["idCorte"].Value != DBNull.Value)
                    {

                        //creo y Cargar la Entidad CortePorCompra
                        oCortePorCompra = new Entidades.CortePorCompra();

                        Entidades.Corte oCorteE = new Entidades.Corte();
                        oCorteE.idCorte = Convert.ToInt32(grillaPorcCortes.Rows[i].Cells["idCorte"].Value);

                        oCortePorCompra.Corte = oCorteE;
                        oCortePorCompra.Compra = oAjusteE;
                        oCortePorCompra.cantKgs = Utilidades.Util_Form.convertFloat(grillaPorcCortes.Rows[i].Cells["Dif."].Value.ToString(), true);
                        oCortePorCompra.precioKg = float.Parse("0.00");
                        oCortePorCompra.Creado = DateTime.Now;
                        oCortePorCompra.CreadoPor = oAjusteE.CreadoPor;
                        oCortePorCompra.sucursal = oAjusteE.Sucursal;

                        oCompraN.agregarCortePorCompra(oCortePorCompra);
                        oCortePorCompra = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error al cargar el corte.\n\nMensaje de exception: " + ex.Message);
            }
        }

        private void setearEstado()
        {
            try
            {
                oPesajeE = oCompraN.findById_convertToCompra(idPesaje);
                idAjuste = oCompraN.getIdAjusteDelPesaje(idPesaje);
                oAjusteE = idAjuste > 0 ? oCompraN.findById_convertToCompra(idAjuste) : new Entidades.Compra();

                estadoAjuste = oCompraN.estadoAjusteStock(idPesaje, idAjuste);

                btnGenerarAj.Enabled = true;
                lblEstadoAjuste.Text = Entidades.Compra.estadoAjStockToString(estadoAjuste);
                //Seteo del Label del Estado en el form
                switch (estadoAjuste)
                {
                    case Entidades.Compra.estadoAjusteStock.Actualizado:
                        lblEstadoAjuste.ForeColor = Color.Green;
                        btnGenerarAj.Enabled = false;
                        break;
                    case Entidades.Compra.estadoAjusteStock.NoActualizado:
                        lblEstadoAjuste.ForeColor = Color.Red;
                        break;
                    case Entidades.Compra.estadoAjusteStock.NoRealizado:
                        lblEstadoAjuste.ForeColor = Color.Red;
                        break;
                }    

                //si esta abierto form de Pesaje se actualiza el estado
                if (frmPesaje != null)
                {
                    frmPesaje.cargarEstadoAjuste(Entidades.Compra.estadoAjStockToString(estadoAjuste));
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.Source);
            }    
        }
    }
}
