using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Negocio
{
    public class Compra
    {
        Datos.Compra oCompraD = new Datos.Compra();
        
        public int agregarCompra(Entidades.Compra oCompraE)
        {
           return oCompraD.agregarCompra(oCompraE);            
        }
        
        public DataTable findById(int idCompra)
        {
            DataTable dtCompra = oCompraD.findById(idCompra);
            return dtCompra;
        }

        public Entidades.Compra findById_convertToCompra(int idCompra)
        {
            Entidades.Compra oCompra = new Entidades.Compra();
            DataTable dtCompra = this.findById(idCompra);
            foreach (DataRow row in dtCompra.Rows)
            {
                oCompra.IdCompra = Convert.ToInt32(row["idCompra"].ToString());
                oCompra.NroRemito = row["nroRemito"].ToString();
                oCompra.FechaCompra = Convert.ToDateTime(row["fechaCompra"].ToString());
                Negocio.Persona oPersonaN = new Persona();
                oCompra.Proveedor = oPersonaN.findById(Convert.ToInt32(row["idProveedor"].ToString()));
                oCompra.TipoCompra = row["tipoCompra"].ToString();
                oCompra.CantMedias = row["cantMedias"].Equals(DBNull.Value) ? null : (int?)(row["cantMedias"]);
                oCompra.KgsMedias = row["kgsMedias"].Equals(DBNull.Value) ? null : (float?) Convert.ToSingle(row["kgsMedias"]);
                oCompra.EnCtaCte = Convert.ToBoolean(row["enCtaCte"]);
                //agrego sucursal
                Negocio.Sucursal oSucN = new Negocio.Sucursal();
                oCompra.Sucursal = oSucN.findById(Convert.ToInt32(row["idSucursal"].ToString()));
                oCompra.Estado = row["estado"].ToString();
                oCompra.Observaciones = row["observaciones"].ToString();
                oCompra.Creado = row["creado"].Equals(null) ? (DateTime?)null : (DateTime?)Convert.ToDateTime(row["creado"].ToString());
                oCompra.Actualizado = row["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(row["actualizado"]);
                Usuario oUsuarioN = new Usuario();
                oCompra.CreadoPor = row["creadoPor"].Equals(DBNull.Value) ? null : oUsuarioN.getUserById(Convert.ToInt32(row["creadoPor"].ToString()));
                oCompra.ActualizadoPor = row["actualizadoPor"].Equals(DBNull.Value) ? null : oUsuarioN.getUserById(Convert.ToInt32(row["actualizadoPor"].ToString()));
            }
            return oCompra;
        }

        public void modificarCompra(Entidades.Compra oCompraE)
        {
            oCompraD.ModificarCompra(oCompraE);
        }
        
        public void crearMovCtaCteCompra(Entidades.Compra oCompraE)
        {
            oCompraE = findById_convertToCompra(oCompraE.IdCompra);
            Negocio.CuentaCorriente oCtaCteN = new Negocio.CuentaCorriente();
            string detalle = "\'" + oCompraE.TipoCompra + "\'" + " | Kgs: " + oCompraE.KgsMedias.ToString()
                + " | " + "Cant: " + oCompraE.CantMedias.ToString();
            oCtaCteN.crearMovCtaCte(oCompraE.Proveedor, oCompraE.FechaCompra, Entidades.MovCtaCte.tablas.Compras, oCompraE.IdCompra, oCompraE.NroRemito,
                detalle, Entidades.MovCtaCte.tipoMov.Credito, oCompraD.getTotalCompra(oCompraE.IdCompra, oCompraE.TipoCompra), oCompraE.Sucursal,
                oCompraE.Creado, oCompraE.CreadoPor, oCompraE.Actualizado, null, oCompraE.EnCtaCte);
        }

        public void modificarPrecioMedia(int idCompra, float precioKg)
        {
            oCompraD.modificarPrecioMedia(idCompra, precioKg);
        }
        
        public void agregarMedias(Entidades.MediaRes oMediaResE)
        {
            oCompraD.agregarMediaRes(oMediaResE);

        }

        public void agregarCortePorCompra(Entidades.CortePorCompra oCortePorCompraE)
        {
            oCompraD.agregarCortePorCompra(oCortePorCompraE);
        }

        public int obtenerUltimaCompra()
        {
            return oCompraD.obtenerIdUltimaCompra();
        }

        public DataTable obtenerCompras(int idSucursal, string tipoCompra, string texto, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal)
        {
            return oCompraD.obtenerCompras(idSucursal,tipoCompra, texto,fechaDesde,fechaHasta, conexionSucursal);
        }

        public DataTable getLineasCompras(int idSucursal, string tipoCompra, string texto, string codigo, string corte, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal)
        {
            return oCompraD.getLineasCompras(idSucursal, tipoCompra, texto, codigo, corte, fechaDesde, fechaHasta, conexionSucursal);
        }

        public DataTable obtenerCortesPorCompra(int idCompra)
        {
            return oCompraD.obtenerCortesPorCompra(idCompra);
        }

        public List<Entidades.CortePorCompra> convertCortesPorCompraToList(int idCompra)
        {
            List<Entidades.CortePorCompra> listCortesPorCompra = new List<Entidades.CortePorCompra>();

            DataTable dtCortesPorCompra = obtenerCortesPorCompra(idCompra);
            if (dtCortesPorCompra.Rows.Count > 0)
            {
                Negocio.Usuario oUsuarioN = new Usuario();
                List<Entidades.Usuario> listaUsuario = oUsuarioN.listaUsuario();

                Negocio.Corte oCorteN = new Corte();
                DataTable dtCortes = oCorteN.obtenerCortes();
                Entidades.Corte oCorte;

                Entidades.Compra oCompra = findById_convertToCompra(idCompra);
                Entidades.CortePorCompra corte;
                foreach (DataRow row in dtCortesPorCompra.Rows)
                {
                    corte = new Entidades.CortePorCompra();

                    corte.Compra = oCompra;

                    corte.IdCortePorCompra = row["idCortePorCompra"] != null ? Convert.ToInt32(row["idCortePorCompra"].ToString()) : 0;
                    corte.precioKgs = float.Parse(row["precioKg"].ToString());
                    corte.CantKgs = float.Parse(row["cantKg"].ToString());
                    corte.Creado = row["creado"] != DBNull.Value ? (DateTime?)(row["creado"]) : oCompra.Creado;
                    int idUser = row["creadoPor"] != null ? Convert.ToInt32(row["creadoPor"].ToString()) : 0;
                    foreach (Entidades.Usuario  user in listaUsuario)
                    {
                        if (user.Id.Equals(idUser))
                        {
                            corte.CreadoPor = user;
                            break;
                        }
                    }

                    oCorte = new Entidades.Corte();

                    foreach (DataRow rowCorte in dtCortes.Rows)
                    {
                        if (row["idCorte"].Equals(rowCorte["idCorte"]))
                        {
                            oCorte.idCorte = Convert.ToInt32(rowCorte["idCorte"]);
                            oCorte.codigo = Convert.ToInt32(rowCorte["codigo"]);
                            oCorte.corte = rowCorte["corte"].ToString();
                            oCorte.precioKg = float.Parse(rowCorte["precioKg"].ToString());
                            break;
                        }
                    }
                    corte.Corte = oCorte;
                    oCorte = null;
                    Entidades.Sucursal oSuc = new Entidades.Sucursal();
                    oSuc.idSucursal = Convert.ToInt32(row["idSucursal"]);
                    corte.Sucursal = oSuc;

                    listCortesPorCompra.Add(corte);
                }
            }
            
            return listCortesPorCompra;
        }

        public DataTable obtenerMediasPorCompra(int idCompra)
        {
            return oCompraD.obtenerMediasPorCompra(idCompra);
        }

        public void anularCompra(int idCompra)
        {
            oCompraD.anularCompra(idCompra);
        }

        public void modificarMediaPorCompra(Entidades.MediaRes oMediaResE, int idCompra)
        {
            oCompraD.modificarMediaPorCompra(oMediaResE, idCompra);
        }

        public void modificarCortePorCompra(Entidades.CortePorCompra oCortePorCompraE, int idCompra)
        {
            oCompraD.modificarCortePorCompra(oCortePorCompraE,idCompra);
        }

        public void quitarStockMedia(Entidades.MediaRes oMediaResE, int idCompra)
        {
            oCompraD.quitarStockMedia(oMediaResE, idCompra);
        }

        public void quitarStockTeoricoMedia(Entidades.MediaRes oMediaResE, int idCompra)
        {
            oCompraD.quitarStockTeoricoMedia(oMediaResE, idCompra);
        }

        public void quitarStockCorte(Entidades.CortePorCompra oCorteE, int idCompra)
        {
            oCompraD.quitarStockCorte(oCorteE, idCompra);
        }

        public DataTable porcentajeCortesPorCompra(int idCompra)
        {
            return oCompraD.porcentajeCortesPorCompra(idCompra);

        }

        public DataTable getPromMedias(int idCompra)
        {
            return oCompraD.getPromMedias(idCompra);

        }

        public DataTable getPorcCortesEnMedias(int idCompra)
        {
            return oCompraD.getPorcCortesEnMedias(idCompra);

        }

        //Se comprueba que el Pesaje tenga el ajuste realizado. Retorna ID <> 0 si tiene
        public int getIdAjusteDelPesaje(int idPesaje)
        {
            return oCompraD.getIdAjusteDelPesaje(idPesaje);
        }

        //retorna el Estado actual del AjusteStock
        public Entidades.Compra.estadoAjusteStock estadoAjusteStock(int idPesaje, int idAjuste)
        {
            Entidades.Compra.estadoAjusteStock estadoAjuste;

            Entidades.Compra oPesajeE;
            Entidades.Compra oAjusteE;

            oPesajeE = findById_convertToCompra(idPesaje);
            idAjuste = getIdAjusteDelPesaje(idPesaje);
            oAjusteE = idAjuste > 0 ? findById_convertToCompra(idAjuste) : null;

            if (oAjusteE == null)
                estadoAjuste = Entidades.Compra.estadoAjusteStock.NoRealizado;
            else
            {
                if (oPesajeE.Actualizado == null)
                {
                    estadoAjuste = Entidades.Compra.estadoAjusteStock.Actualizado;
                }
                else
                {
                    estadoAjuste = (oAjusteE.Actualizado == null && oAjusteE.Creado > oPesajeE.Actualizado) ? Entidades.Compra.estadoAjusteStock.Actualizado :
                        (((oAjusteE.Actualizado == null && oAjusteE.Creado < oPesajeE.Actualizado) || (oAjusteE.Actualizado != null && oPesajeE.Actualizado > oAjusteE.Actualizado)) ? Entidades.Compra.estadoAjusteStock.NoActualizado :
                        Entidades.Compra.estadoAjusteStock.Actualizado);
                }
            }

            return estadoAjuste;    
        }

        public void actualizarEstadoPesaje(int idPesaje, Entidades.Compra.estadoAjusteStock estadoAjStock)
        {
            oCompraD.actualizarEstadoPesaje(idPesaje, estadoAjStock);
        }

        public void backup(string destino)
        {
            oCompraD.backup(destino);
        }

        public void restaurarBD(string dataSource ,string bdAuxiliar, string rutaOrigen)
        {
            oCompraD.restaurarBD(dataSource,bdAuxiliar, rutaOrigen);
        }
    }
}
