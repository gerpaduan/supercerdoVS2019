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
            // 2026-08-20). oCorteN.editPrecioCorte (mas abajo) NO participa de la unidad de
            // trabajo compartida todavia -- camino condicional (ActualizarPrecioVenta) no
            // exercitado por el testeo real hecho hasta ahora; si alguna vez falla en modo
            // Postgres, aplicar el mismo patron. El estado del Pesaje (actualizarEstadoPesaje) en
            // Postgres se actualiza DESPUES del commit, no dentro de la unidad de trabajo: usa otra
            // conexion y, al editar un pesaje existente, quedaba esperando el lock de la fila que
            // la propia transaccion ya habia modificado hasta el timeout de 30 s (ver docs/DECISIONS.md
            // 2026-10-16, "Guardar un Pesaje existente en Postgres").
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
                    int id;
                    try
                    {
                        id = EjecutarAddOrEditCompra(oCompraE, tipoCompra, listaMediaRes, listaCortePorCompra, esEgresoCaja, oEgresoCajaE, unitOfWork);
                        unitOfWork.Completar();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error en registrar la compra: \n" + ex.Message, ex);
                    }

                    // Ya con la compra confirmada (commit hecho), el estado del Pesaje se actualiza por
                    // su propia conexion sin pelear el lock de la fila. Si fallara, la compra YA quedo
                    // guardada: el mensaje lo dice para que no se la vuelva a cargar (duplicaria).
                    try
                    {
                        ActualizarEstadoPesajeSiCorresponde(oCompraE);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("La compra " + id + " se guardó, pero no se pudo actualizar el estado del ajuste del pesaje: \n" + ex.Message, ex);
                    }

                    return id;
                }
            }
        }

        // Para un Pesaje de medias (con cantidad y kilos de medias) recalcula el estado de su ajuste
        // (No realizado / Actualizado / No actualizado) y lo guarda en la compra. No hace nada para
        // otros tipos ni para un pesaje sin medias.
        private void ActualizarEstadoPesajeSiCorresponde(Entidades.Compra oCompraE)
        {
            if (!oCompraE.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.PesajeCortes)))
                return;

            //se verifica que el pesaje sea de medias
            if ((!oCompraE.KgsMedias.Equals(null) && oCompraE.KgsMedias > 0) &&
                (!oCompraE.CantMedias.Equals(null) && oCompraE.CantMedias > 0))
            {
                actualizarEstadoPesaje(oCompraE.IdCompra, estadoAjusteStock(oCompraE.IdCompra, 0));
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

            //Se actualiza el estado del Pesaje. Con unidad de trabajo (Postgres) se difiere hasta
            //despues del commit (ver AddOrEditCompra); con TransactionScope (SQL Server) queda aca.
            if (unitOfWork == null)
                ActualizarEstadoPesajeSiCorresponde(oCompraE);

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
                    corte.NoContado = row.Table.Columns.Contains("noContado") && row["noContado"] != DBNull.Value && Convert.ToBoolean(row["noContado"]);
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

        #region Eliminacion con motivo (2026-10-22, ver docs/DECISIONS.md "Eliminar compras, stock y movimientos")

        private sealed class FotoCompra
        {
            public Entidades.Compra Compra;
            public List<Entidades.AuditoriaEliminacionLinea> Lineas = new List<Entidades.AuditoriaEliminacionLinea>();
            public double Importe;
            public double TotalKg;
        }

        // Lee la compra y sus lineas tal como estan ahora (lo que se va a borrar). null si la compra no existe.
        private FotoCompra cargarFotoParaEliminar(int idCompra)
        {
            Entidades.Compra compra = findById_convertToCompra(idCompra);
            if (compra == null || compra.IdCompra == 0) return null;

            var foto = new FotoCompra { Compra = compra };
            int idSucursalCompra = compra.Sucursal != null ? compra.Sucursal.IdSucursal : 0;

            DataTable cortes = oCompraD.obtenerCortesPorCompra(idCompra);
            if (cortes != null && cortes.Rows.Count > 0)
            {
                // Codigo y nombre del producto: se copian a la auditoria porque el producto puede cambiar o borrarse despues.
                var productos = new Dictionary<int, KeyValuePair<long, string>>();
                DataTable catalogo = oCorteN.obtenerCortes();
                if (catalogo != null)
                {
                    foreach (DataRow rowCorte in catalogo.Rows)
                        productos[Convert.ToInt32(rowCorte["idCorte"])] =
                            new KeyValuePair<long, string>(Convert.ToInt64(rowCorte["codigo"]), Convert.ToString(rowCorte["corte"]));
                }

                foreach (DataRow row in cortes.Rows)
                {
                    int idCorte = Convert.ToInt32(row["idCorte"]);
                    double cantKg = row["cantKg"] == DBNull.Value ? 0 : Convert.ToDouble(row["cantKg"]);
                    double precioKg = row["precioKg"] == DBNull.Value ? 0 : Convert.ToDouble(row["precioKg"]);
                    bool balanza = row["balanza"] != DBNull.Value && Convert.ToBoolean(row["balanza"]);
                    bool noContado = row.Table.Columns.Contains("noContado") && row["noContado"] != DBNull.Value && Convert.ToBoolean(row["noContado"]);
                    var detalle = new List<string>();
                    if (balanza) detalle.Add("Balanza");
                    if (noContado) detalle.Add("No contado");

                    KeyValuePair<long, string> producto;
                    bool conocido = productos.TryGetValue(idCorte, out producto);
                    foto.Lineas.Add(new Entidades.AuditoriaEliminacionLinea
                    {
                        IdCorte = idCorte,
                        Codigo = conocido ? (long?)producto.Key : null,
                        Producto = conocido ? producto.Value : "(producto " + idCorte + ")",
                        IdSucursal = row["idSucursal"] == DBNull.Value ? idSucursalCompra : Convert.ToInt32(row["idSucursal"]),
                        CantKg = cantKg,
                        PrecioKg = precioKg,
                        Detalle = detalle.Count > 0 ? string.Join(" | ", detalle) : null
                    });
                    foto.TotalKg += cantKg;
                    foto.Importe += cantKg * precioKg;
                }
            }

            DataTable medias = oCompraD.obtenerMediasPorCompra(idCompra);
            if (medias != null)
            {
                foreach (DataRow row in medias.Rows)
                {
                    double kg = row["kgMedia"] == DBNull.Value ? 0 : Convert.ToDouble(row["kgMedia"]);
                    double precio = row["precioMedia"] == DBNull.Value ? 0 : Convert.ToDouble(row["precioMedia"]);
                    string tropa = row["nroTropa"] == DBNull.Value ? "" : Convert.ToString(row["nroTropa"]);
                    foto.Lineas.Add(new Entidades.AuditoriaEliminacionLinea
                    {
                        Producto = "Media res",
                        IdSucursal = idSucursalCompra,
                        CantKg = kg,
                        PrecioKg = precio,
                        Detalle = string.IsNullOrEmpty(tropa) ? null : "Tropa " + tropa
                    });
                    foto.TotalKg += kg;
                    foto.Importe += kg * precio;
                }
            }

            return foto;
        }

        // Registros que dependen de este y que quedarian apuntando a algo que ya no existe: no se puede eliminar hasta resolverlos.
        private List<string> calcularBloqueosEliminacion(Entidades.Compra compra)
        {
            var bloqueos = new List<string>();

            List<int> vinculados = oCompraD.obtenerPesajesVinculadosPorDestino(compra.IdCompra) ?? new List<int>();
            if (vinculados.Count > 0)
                bloqueos.Add("Tiene pesajes vinculados (" + EliminacionRegistro.ListarIds(vinculados) + "). Desvinculalos o eliminalos primero.");

            if (EliminacionRegistro.EsPesaje(compra.TipoCompra))
            {
                int idAjuste = oCompraD.getIdAjusteDelPesaje(compra.IdCompra);
                if (idAjuste > 0)
                    bloqueos.Add("Tiene un ajuste de stock generado (#" + idAjuste + "). Eliminá primero el ajuste.");
            }

            return bloqueos;
        }

        private static string FormatearImporte(double importe)
        {
            return "$ " + importe.ToString("N2", new System.Globalization.CultureInfo("es-AR"));
        }

        // Saldo de cta cte: positivo = a favor de la persona, negativo = debe.
        private static string FormatearSaldo(decimal saldo)
        {
            return saldo < 0 ? FormatearImporte((double)-saldo) + " (debe)" : FormatearImporte((double)saldo) + " (a favor)";
        }

        // Todo lo que el usuario necesita ver antes de confirmar. null si la compra no existe. No escribe nada.
        public VistaPreviaEliminacion obtenerVistaPreviaEliminacion(int idCompra)
        {
            FotoCompra foto = cargarFotoParaEliminar(idCompra);
            if (foto == null) return null;

            Entidades.Compra compra = foto.Compra;
            var vista = new VistaPreviaEliminacion
            {
                Modulo = EliminacionRegistro.ModuloDeTipoCompra(compra.TipoCompra),
                IdRegistro = compra.IdCompra,
                TipoRegistro = compra.TipoCompra,
                FechaRegistro = compra.FechaCompra,
                IdSucursal = compra.Sucursal != null ? compra.Sucursal.IdSucursal : 0,
                SucursalNombre = compra.Sucursal != null ? compra.Sucursal.SucursalNombre : "",
                PersonaNombre = compra.Proveedor != null ? compra.Proveedor.RazonSocial : "",
                NroRemito = compra.NroRemito ?? "",
                IdUsuarioCreador = compra.CreadoPor != null ? compra.CreadoPor.Id : 0,
                EnCtaCte = compra.EnCtaCte,
                Importe = foto.Importe,
                TotalKg = foto.TotalKg,
                CantLineas = foto.Lineas.Count,
                RequiereAdmin = EliminacionRegistro.RequiereAdmin(compra.TipoCompra),
                Lineas = foto.Lineas,
                Bloqueos = calcularBloqueosEliminacion(compra)
            };

            Entidades.MovCtaCte movCtaCte = oCtaCteN.getMovCtaCteVigenteDeRegistro(Entidades.MovCtaCte.tablas.Compras, compra.IdCompra);
            if (movCtaCte != null && compra.Proveedor != null)
            {
                decimal saldoActual = oCtaCteN.obtenerSaldoPersona(compra.Proveedor.IdPersona) ?? 0m;
                decimal saldoDespues = saldoActual - (decimal)movCtaCte.Importe;
                vista.EfectoCtaCte = "Se genera el asiento contrario en la cuenta corriente de " + compra.Proveedor.RazonSocial
                    + ": el saldo pasa de " + FormatearSaldo(saldoActual) + " a " + FormatearSaldo(saldoDespues) + ". El asiento original queda en el extracto.";
            }

            Entidades.EgresoCaja egreso = oCierreN.findEgresoCajaPorCompra(compra.IdCompra);
            if (egreso != null && egreso.Id > 0)
            {
                vista.Egreso = egreso;
                if (egreso.Monto != 0)
                    vista.Avisos.Add("La compra tiene un egreso de caja de " + FormatearImporte(egreso.Monto)
                        + ": se registra un egreso opuesto con la fecha de hoy.");
            }

            if (string.Equals(compra.TipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes), StringComparison.OrdinalIgnoreCase))
                vista.Avisos.Add("Si esta compra actualizó el precio de venta de algún producto, ese precio no se revierte.");
            if (string.Equals(compra.TipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock), StringComparison.OrdinalIgnoreCase))
                vista.Avisos.Add("Un cierre de stock es el punto de partida del cálculo: al eliminarlo cambia el stock de la sucursal desde el cierre anterior.");
            if (vista.Modulo == Entidades.AuditoriaEliminacion.ModuloStock)
                vista.Avisos.Add("El stock de " + vista.SucursalNombre + " se recalcula sin este registro.");

            return vista;
        }

        // Elimina DE VERDAD una compra o una operacion de stock. Valida, arma la foto, y en una sola transaccion: borra el registro
        // y sus lineas, guarda la auditoria, genera el asiento contrario en la cta cte del proveedor (si tenia uno vigente) y el
        // egreso de caja opuesto (si tenia monto). No chequea permisos ni la clave: eso es del controller (quien llama garantiza
        // que usuarioElimino ya valido su usuario y clave). moduloEsperado evita borrar stock desde Compras y al reves.
        public ResultadoEliminacion eliminarCompra(int idCompra, string moduloEsperado, string motivo, Entidades.Usuario usuarioElimino,
            Entidades.Usuario usuarioSesion, string ip)
        {
            if (usuarioElimino == null || usuarioElimino.Id <= 0)
                return ResultadoEliminacion.Fallo("No se pudo identificar al usuario que elimina el registro.");

            string errorMotivo = EliminacionRegistro.ValidarMotivo(motivo, out string motivoNormalizado);
            if (errorMotivo != null) return ResultadoEliminacion.Fallo(errorMotivo);

            FotoCompra foto = cargarFotoParaEliminar(idCompra);
            if (foto == null) return ResultadoEliminacion.Fallo("El registro no existe o ya fue eliminado.");

            Entidades.Compra compra = foto.Compra;
            if (!string.Equals(EliminacionRegistro.ModuloDeTipoCompra(compra.TipoCompra), moduloEsperado, StringComparison.Ordinal))
                return ResultadoEliminacion.Fallo("Este registro es de tipo «" + compra.TipoCompra + "» y no se elimina desde esta pantalla.");

            List<string> bloqueos = calcularBloqueosEliminacion(compra);
            if (bloqueos.Count > 0) return ResultadoEliminacion.Fallo(string.Join(" ", bloqueos));

            // Efectos que se van a producir, escritos en la auditoria (se calculan antes: la auditoria se inserta junto con el borrado).
            Entidades.MovCtaCte movVigente = oCtaCteN.getMovCtaCteVigenteDeRegistro(Entidades.MovCtaCte.tablas.Compras, compra.IdCompra);
            Entidades.EgresoCaja egreso = oCierreN.findEgresoCajaPorCompra(compra.IdCompra);
            bool hayEgresoConMonto = egreso != null && egreso.Id > 0 && egreso.Monto != 0;

            var efectos = new List<string>();
            if (movVigente != null) efectos.Add("Asiento contrario en la cuenta corriente del proveedor por " + FormatearImporte(Math.Abs(movVigente.Importe)) + ".");
            if (hayEgresoConMonto) efectos.Add("Egreso de caja opuesto por " + FormatearImporte(egreso.Monto) + " con fecha " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + ".");

            var auditoria = new Entidades.AuditoriaEliminacion
            {
                Modulo = EliminacionRegistro.ModuloDeTipoCompra(compra.TipoCompra),
                IdRegistro = compra.IdCompra,
                TipoRegistro = compra.TipoCompra,
                FechaRegistro = compra.FechaCompra,
                IdSucursal = compra.Sucursal != null ? (int?)compra.Sucursal.IdSucursal : null,
                IdPersona = compra.Proveedor != null ? (int?)compra.Proveedor.IdPersona : null,
                NroRemito = compra.NroRemito,
                EnCtaCte = compra.EnCtaCte,
                CantMedias = compra.CantMedias,
                KgsMedias = compra.KgsMedias,
                Importe = foto.Importe,
                TotalKg = foto.TotalKg,
                CantLineas = foto.Lineas.Count,
                Observaciones = compra.Observaciones,
                IdUsuarioCreador = compra.CreadoPor != null ? (int?)compra.CreadoPor.Id : null,
                Creado = compra.Creado,
                IdUsuarioElimino = usuarioElimino.Id,
                IdUsuarioSesion = (usuarioSesion ?? usuarioElimino).Id,
                Motivo = motivoNormalizado,
                Ip = ip,
                Efectos = efectos.Count > 0 ? string.Join("\n", efectos) : null
            };

            int idAuditoria;
            try
            {
                var unitOfWork = oCompraD.IniciarUnitOfWork();
                if (unitOfWork == null)
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        idAuditoria = EjecutarEliminarCompra(compra, auditoria, movVigente != null, hayEgresoConMonto ? egreso : null, usuarioElimino, null);
                        scope.Complete();
                    }
                }
                else
                {
                    using (unitOfWork)
                    {
                        idAuditoria = EjecutarEliminarCompra(compra, auditoria, movVigente != null, hayEgresoConMonto ? egreso : null, usuarioElimino, unitOfWork);
                        unitOfWork.Completar();
                    }
                }
            }
            catch (RegistroYaEliminadoException)
            {
                return ResultadoEliminacion.Fallo("El registro ya fue eliminado por otro usuario.");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el registro: \n" + ex.Message, ex);
            }

            auditoria.Id = idAuditoria;
            var resultado = new ResultadoEliminacion { Ok = true, IdAuditoria = idAuditoria, Auditoria = auditoria };

            // Ya con el borrado confirmado: lo que sigue es secundario y su falla no deshace nada.
            try
            {
                RecalcularEstadoDelPesajeDelAjuste(compra);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("No se pudo recalcular el estado del pesaje tras eliminar el ajuste " + compra.IdCompra + ": " + ex);
                resultado.Advertencia = "El registro se eliminó, pero no se pudo actualizar el estado del ajuste del pesaje. Abrí el pesaje y guardalo para recalcularlo.";
            }

            oCtaCteN.notificarRegistroEliminado(auditoria, usuarioElimino.Nombre,
                (compra.TipoCompra ?? "registro") + " #" + compra.IdCompra + (compra.Proveedor != null ? " de " + compra.Proveedor.RazonSocial : "")
                + " por " + FormatearImporte(foto.Importe));

            return resultado;
        }

        private int EjecutarEliminarCompra(Entidades.Compra compra, Entidades.AuditoriaEliminacion auditoria, bool revertirCtaCte,
            Entidades.EgresoCaja egresoAReversar, Entidades.Usuario usuarioElimino, Contratos.IUnitOfWork unitOfWork)
        {
            // 1) Borrado + auditoria. Si devuelve 0 nadie escribio nada: otro usuario se adelanto.
            int idAuditoria = oCompraD.eliminarCompraConAuditoria(compra.IdCompra, auditoria, unitOfWork);
            if (idAuditoria == 0) throw new RegistroYaEliminadoException();

            // 2) Cuenta corriente del proveedor: asiento contrario (queda el original + el ELIMINADO).
            if (revertirCtaCte)
                oCtaCteN.registrarContraasientoPorEliminacion(Entidades.MovCtaCte.tablas.Compras, compra.IdCompra, usuarioElimino, unitOfWork);

            // 3) Caja: egreso opuesto fechado HOY (la caja original puede estar cerrada y no se reescribe). Va ultimo: si falla,
            // se deshace todo lo anterior.
            if (egresoAReversar != null)
            {
                egresoAReversar.Id = 0;
                egresoAReversar.Descripcion = "ELIMINADO: " + egresoAReversar.Descripcion;
                egresoAReversar.Monto = egresoAReversar.Monto * -1;
                egresoAReversar.Fecha = DateTime.Now;
                egresoAReversar.CreadoPor = usuarioElimino.Id;
                egresoAReversar.ActualizadoPor = 0;
                oCierreN.addOrEditEgresoCaja(egresoAReversar, unitOfWork);
            }

            return idAuditoria;
        }

        // Al eliminar un Ajuste de stock, el Pesaje que ajustaba vuelve a "No realizado". Mismo criterio que
        // ActualizarEstadoPesajeSiCorresponde: solo para pesajes de medias.
        private void RecalcularEstadoDelPesajeDelAjuste(Entidades.Compra ajusteEliminado)
        {
            if (!EliminacionRegistro.EsAjuste(ajusteEliminado.TipoCompra)) return;

            int idPesaje = ajusteEliminado.IdPesajeAjustado ?? 0;
            if (idPesaje <= 0) int.TryParse(ajusteEliminado.NroRemito, out idPesaje);
            if (idPesaje <= 0) return;

            Entidades.Compra pesaje = findById_convertToCompra(idPesaje);
            if (pesaje == null || pesaje.IdCompra == 0 || !EliminacionRegistro.EsPesaje(pesaje.TipoCompra)) return;
            if (!(pesaje.KgsMedias.HasValue && pesaje.KgsMedias.Value > 0 && pesaje.CantMedias.HasValue && pesaje.CantMedias.Value > 0)) return;

            actualizarEstadoPesaje(idPesaje, estadoAjusteStock(idPesaje, 0));
        }

        #endregion

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
