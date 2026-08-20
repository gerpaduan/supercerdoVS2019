using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Transactions;
using Utilidades;

namespace Negocio
{
    public class Compra
    {
        // oCompraD: bloque real (Compras/CortePorCompra/MediaRes) migrado a la interfaz -- puede
        // ser SQL Server o Postgres. oCompraDSqlServer: backup/restaurarBD, sin equivalente en
        // Postgres, siempre SQL Server. Ver docs/DECISIONS.md, Etapa 9.
        private readonly Contratos.ICompraRepository oCompraD;
        private readonly Datos.Compra oCompraDSqlServer;
        Negocio.Corte oCorteN;
        Negocio.Sucursal oSucN;
        Negocio.Usuario oUsuarioN;
        Negocio.CierreCaja oCierreN;
        Negocio.Persona oPersonaN;
        Negocio.CuentaCorriente oCtaCteN;

        IEmpresaContext _empresa;private readonly IParametrosContext _param;

        // Constructor existente: SIN CAMBIOS de comportamiento.
        public Compra(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa;_param = param;
            var datosCompra = new Datos.Compra(empresa, param);
            oCompraD = datosCompra;
            oCompraDSqlServer = datosCompra;
            oCorteN = new Corte(empresa, param);
            oSucN = new Negocio.Sucursal(empresa, param);
            oUsuarioN = new Usuario(empresa, param);
            oCierreN = new Negocio.CierreCaja(empresa, param);
            oPersonaN = new Persona(empresa, param);
            oCtaCteN = new Negocio.CuentaCorriente(empresa, param);
        }

        // Constructor nuevo, aditivo: inyecta cualquier implementacion de ICompraRepository
        // (ej. DatosPostgres.CompraPg) para el bloque migrado. backup/restaurarBD siguen yendo
        // por SQL Server (oCompraDSqlServer).
        //
        // Las 6 dependencias internas (corteN..ctaCteN) son opcionales, default null -- si no
        // se pasan, caen al constructor SQL-Server-only de siempre (mismo patron ya usado en
        // Negocio.Corte para CortePuntoStockSucursal y en Negocio.Usuario para Sucursal). Sin
        // esto, una operacion de Compra en modo Postgres solo escribia Compras/CortePorCompra/
        // MediaRes en Postgres, pero crearMovCtaCteCompra (siempre se llama) seguia escribiendo
        // en SQL Server -- dos motores mezclados en la misma operacion de negocio, y la causa
        // real del choque de TransactionScope contra Npgsql (Etapa de testeo profundo,
        // 2026-08-20, ver docs/DECISIONS.md). NegocioFactory.CrearCompra pasa las 6 ya
        // cableadas a Postgres cuando corresponde.
        public Compra(
            Contratos.ICompraRepository repositorio,
            IEmpresaContext empresa,
            IParametrosContext param = null,
            Negocio.Corte corteN = null,
            Negocio.Sucursal sucursalN = null,
            Negocio.Usuario usuarioN = null,
            Negocio.CierreCaja cierreCajaN = null,
            Negocio.Persona personaN = null,
            Negocio.CuentaCorriente ctaCteN = null)
        {
            _empresa = empresa; _param = param;
            oCompraD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
            oCompraDSqlServer = new Datos.Compra(empresa, param);
            oCorteN = corteN ?? new Corte(empresa, param);
            oSucN = sucursalN ?? new Negocio.Sucursal(empresa, param);
            oUsuarioN = usuarioN ?? new Usuario(empresa, param);
            oCierreN = cierreCajaN ?? new Negocio.CierreCaja(empresa, param);
            oPersonaN = personaN ?? new Persona(empresa, param);
            oCtaCteN = ctaCteN ?? new Negocio.CuentaCorriente(empresa, param);
        }

        public int AddOrEditCompra(Entidades.Compra oCompraE, string tipoCompra,List<Entidades.MediaRes> listaMediaRes,
            List<Entidades.CortePorCompra> listaCortePorCompra, bool esEgresoCaja, Entidades.EgresoCaja oEgresoCajaE)
        {
            // Mismo mecanismo que Negocio.Venta.agregarVenta: TransactionScope para SQL Server
            // (sin cambios), IUnitOfWork explicito para Postgres (ver docs/DECISIONS.md,
            // 2026-08-20). oCorteN.editPrecioCorte y actualizarEstadoPesaje (mas abajo) NO
            // participan de la unidad de trabajo compartida todavia -- caminos condicionales,
            // no exercitados por el testeo real hecho hasta ahora (ActualizarPrecioVenta y
            // PesajeCortes). Si alguna vez fallan en modo Postgres, aplicar el mismo patron.
            var unitOfWork = oCompraD.IniciarUnitOfWork();
            if (unitOfWork == null)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        int id = EjecutarAddOrEditCompra(oCompraE, tipoCompra, listaMediaRes, listaCortePorCompra, esEgresoCaja, oEgresoCajaE, null);
                        scope.Complete();
                        return id;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error en registrar la compra: \n" + ex.Message, ex);
                    }
                }
            }
            else
            {
                using (unitOfWork)
                {
                    try
                    {
                        int id = EjecutarAddOrEditCompra(oCompraE, tipoCompra, listaMediaRes, listaCortePorCompra, esEgresoCaja, oEgresoCajaE, unitOfWork);
                        unitOfWork.Completar();
                        return id;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error en registrar la compra: \n" + ex.Message, ex);
                    }
                }
            }
        }

        private int EjecutarAddOrEditCompra(Entidades.Compra oCompraE, string tipoCompra, List<Entidades.MediaRes> listaMediaRes,
            List<Entidades.CortePorCompra> listaCortePorCompra, bool esEgresoCaja, Entidades.EgresoCaja oEgresoCajaE, Contratos.IUnitOfWork unitOfWork)
        {
            oCompraE.LineasMediasReses = listaMediaRes;
            oCompraE.LineasCortes = listaCortePorCompra;

            oCompraE.IdCompra = oCompraD.addOrEditCompra(oCompraE, unitOfWork);

            if (tipoCompra == Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes))
            {
                foreach (Entidades.MediaRes mediaRes in listaMediaRes)
                {
                    mediaRes.sucursal = oCompraE.Sucursal;
                    agregarMedias(mediaRes, unitOfWork);
                }
            }

            foreach (Entidades.CortePorCompra cortePorCompra in listaCortePorCompra)
            {
                cortePorCompra.Sucursal = oCompraE.Sucursal;
                agregarCortePorCompra(cortePorCompra, unitOfWork);

                //se actualiza el precio del corte
                if (cortePorCompra.ActualizarPrecioVenta && cortePorCompra.PrecioVenta > 0)
                {
                    cortePorCompra.corte.precioKg = cortePorCompra.PrecioVenta;
                    oCorteN.editPrecioCorte(cortePorCompra.Corte);
                }
            }

            //Se actualiza el estado del Pesaje
            if (oCompraE.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.PesajeCortes)))
            {
                //se verifica que el pesaje sea de medias
                if ((!oCompraE.KgsMedias.Equals(null) && oCompraE.KgsMedias > 0) &&
                    (!oCompraE.CantMedias.Equals(null) && oCompraE.CantMedias > 0))
                {
                    actualizarEstadoPesaje(oCompraE.IdCompra, estadoAjusteStock(oCompraE.IdCompra, 0));
                }
            }

            //Cuenta Corriente
            crearMovCtaCteCompra(oCompraE, unitOfWork);

            if (esEgresoCaja)
            {
                if (oEgresoCajaE == null)
                    oEgresoCajaE = new Entidades.EgresoCaja();

                ///si la compra es en CTA CTE,
                ///se informa en egresos pero el monto será 0 porque no salió el dinero de la caja
                string descripcionEgreso = "Compra a ";
                string detalleEgreso = string.Empty;
                float montoEgreso = oCompraE.getImporteCompra(oCompraE, listaMediaRes, listaCortePorCompra);
                if (oCompraE.EnCtaCte)
                {
                    descripcionEgreso = "Compra CTA CTE a ";
                    detalleEgreso = " | $" + montoEgreso.ToString("F2");
                    montoEgreso = 0;
                }
                descripcionEgreso += oCompraE.Proveedor.razonSocial + " - ID:" + oCompraE.IdCompra.ToString() + detalleEgreso;

                oEgresoCajaE.Fecha = oCompraE.FechaCompra;
                oEgresoCajaE.IdTipoEgresoCaja = Entidades.EgresoCaja.idCompraEgresoCaja;// oCierreN.getIdEgresoCajaPorCompra();
                oEgresoCajaE.Descripcion = descripcionEgreso;
                oEgresoCajaE.Monto = montoEgreso;
                oEgresoCajaE.Detalle = oCompraE.Observaciones;
                oEgresoCajaE.Sucursal = oCompraE.Sucursal;
                oEgresoCajaE.IdCompra = oCompraE.IdCompra;
                oEgresoCajaE.CreadoPor = oCompraE.CreadoPor.Id;
                oEgresoCajaE.ActualizadoPor = oEgresoCajaE.Id > 0 ? oCompraE.ActualizadoPor.Id : 0;

                oEgresoCajaE = oCierreN.addOrEditEgresoCaja(oEgresoCajaE, unitOfWork);
            }

            return oCompraE.IdCompra;
        }
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
            //TODO: compra el using como en Ventas
            Entidades.Compra oCompra = new Entidades.Compra();
            DataTable dtCompra = this.findById(idCompra);
            foreach (DataRow row in dtCompra.Rows)
            {
                oCompra.IdCompra = Convert.ToInt32(row["idCompra"].ToString());
                oCompra.NroRemito = row["nroRemito"].ToString();
                oCompra.FechaCompra = Convert.ToDateTime(row["fechaCompra"].ToString());
                
                oCompra.Proveedor = oPersonaN.findById(Convert.ToInt32(row["idProveedor"].ToString()));
                oCompra.TipoCompra = row["tipoCompra"].ToString();
                oCompra.CantMedias = row["cantMedias"].Equals(DBNull.Value) ? null : (int?)(row["cantMedias"]);
                oCompra.KgsMedias = row["kgsMedias"].Equals(DBNull.Value) ? null : (float?) Convert.ToSingle(row["kgsMedias"]);
                oCompra.IdPesajeAjustado = row.Table.Columns.Contains("idPesajeAjustado") && !row["idPesajeAjustado"].Equals(DBNull.Value)
                    ? (int?)Convert.ToInt32(row["idPesajeAjustado"])
                    : null;
                oCompra.EnCtaCte = Convert.ToBoolean(row["enCtaCte"]);
                //agrego sucursal
                
                oCompra.Sucursal = oSucN.findById(Convert.ToInt32(row["idSucursal"].ToString()));
                oCompra.Estado = row["estado"].ToString();
                oCompra.Observaciones = row["observaciones"].ToString();
                oCompra.Creado = row["creado"].Equals(null) ? (DateTime?)null : (DateTime?)Convert.ToDateTime(row["creado"].ToString());
                oCompra.Actualizado = row["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(row["actualizado"]);
                
                oCompra.CreadoPor = row["creadoPor"].Equals(DBNull.Value) ? null : oUsuarioN.getUserById(Convert.ToInt32(row["creadoPor"].ToString()));
                oCompra.ActualizadoPor = row["actualizadoPor"].Equals(DBNull.Value) ? null : oUsuarioN.getUserById(Convert.ToInt32(row["actualizadoPor"].ToString()));
            }
            return oCompra;
        }

        public void modificarCompra(Entidades.Compra oCompraE)
        {
            oCompraD.ModificarCompra(oCompraE);
        }

        public void actualizarObservacionesCompra(int idCompra, string observaciones, Entidades.Usuario actualizadoPor)
        {
            int idUsuario = actualizadoPor != null ? actualizadoPor.Id : 0;
            oCompraD.actualizarObservacionesCompra(idCompra, observaciones, idUsuario);
        }

        public List<int> obtenerPesajesVinculadosPorDestino(int idPesajeDestino)
        {
            return oCompraD.obtenerPesajesVinculadosPorDestino(idPesajeDestino);
        }

        public Dictionary<int, List<int>> obtenerPesajesVinculadosPorDestinos(IEnumerable<int> idsDestino)
        {
            return oCompraD.obtenerPesajesVinculadosPorDestinos(idsDestino);
        }

        public void actualizarIdPesajeAjustado(int idCompra, int? idPesajeAjustado, Entidades.Usuario actualizadoPor)
        {
            int idUsuario = actualizadoPor != null ? actualizadoPor.Id : 0;
            oCompraD.actualizarIdPesajeAjustado(idCompra, idPesajeAjustado, idUsuario);
        }
        
        public void crearMovCtaCteCompra(Entidades.Compra oCompraE, Contratos.IUnitOfWork unitOfWork = null)
        {
            //oCompraE = findById_convertToCompra(oCompraE.IdCompra);

            string detalle = "\'" + oCompraE.TipoCompra + "\'" + " | Kgs: " + oCompraE.KgsMedias.ToString()
                + " | " + "Cant: " + oCompraE.CantMedias.ToString();
            oCtaCteN.crearMovCtaCte(oCompraE.Proveedor, oCompraE.FechaCompra, Entidades.MovCtaCte.tablas.Compras, oCompraE.IdCompra, oCompraE.NroRemito,
                //detalle, Entidades.MovCtaCte.tipoMov.Credito, oCompraD.getTotalCompra(oCompraE.IdCompra, oCompraE.TipoCompra), oCompraE.Sucursal,
                detalle, Entidades.MovCtaCte.tipoMov.Credito, oCompraE.getImporteCompra(oCompraE, oCompraE.LineasMediasReses, oCompraE.LineasCortes), oCompraE.Sucursal,
                oCompraE.Creado, oCompraE.CreadoPor, oCompraE.Actualizado, null, oCompraE.EnCtaCte, null, null, null, unitOfWork);
        }

        public void modificarPrecioMedia(int idCompra, float precioKg)
        {
            oCompraD.modificarPrecioMedia(idCompra, precioKg);
        }

        public void agregarMedias(Entidades.MediaRes oMediaResE, Contratos.IUnitOfWork unitOfWork = null)
        {
            oCompraD.agregarMediaRes(oMediaResE, unitOfWork);

        }

        public void agregarCortePorCompra(Entidades.CortePorCompra oCortePorCompraE, Contratos.IUnitOfWork unitOfWork = null)
        {
            oCompraD.agregarCortePorCompra(oCortePorCompraE, unitOfWork);
        }

        public void limpiarCortesPorCompra(int idCompra)
        {
            oCompraD.limpiarCortesPorCompra(idCompra);
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
                
                List<Entidades.Usuario> listaUsuario = oUsuarioN.listaUsuario();

                
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
                    corte.Balanza = row["balanza"] != DBNull.Value ? Convert.ToBoolean(row["balanza"]) : false;
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
                            oCorte.codigo = Convert.ToInt64(rowCorte["codigo"]);
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

        public Dictionary<int, int> getIdsAjustePorPesajes(IEnumerable<int> idsPesaje)
        {
            return oCompraD.getIdsAjustePorPesajes(idsPesaje);
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
            oCompraDSqlServer.backup(destino);
        }

        public void restaurarBD(string dataSource ,string bdAuxiliar, string rutaOrigen)
        {
            oCompraDSqlServer.restaurarBD(dataSource,bdAuxiliar, rutaOrigen);
        }
    }
}
