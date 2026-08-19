using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Transactions;
using Utilidades;
using Entidades;

namespace Negocio
{
    public class Venta
    {
        // oVentaD: bloque Ventas/LineaVenta/TemporalLineaVenta migrado a la interfaz -- puede
        // ser SQL Server o Postgres. oVentaDSqlServer: el resto de la clase (Expendios,
        // Sectores, FacturaElectronica) que todavia no esta migrado -- siempre SQL Server.
        // Ver docs/DECISIONS.md, Etapa 7.
        private readonly Contratos.IVentaRepository oVentaD;
        private readonly Datos.Venta oVentaDSqlServer;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;

        // Constructor existente: SIN CAMBIOS de comportamiento.
        public Venta(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa;
            _param = param;
            var datosVenta = new Datos.Venta(empresa, param);
            oVentaD = datosVenta;
            oVentaDSqlServer = datosVenta;
        }

        // Constructor nuevo, aditivo: inyecta cualquier implementacion de IVentaRepository
        // (ej. DatosPostgres.VentaPg) para el bloque migrado. Solo lo usa el controller de
        // comparacion. El resto de la clase sigue yendo por SQL Server (oVentaDSqlServer).
        public Venta(Contratos.IVentaRepository repositorio, IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa;
            _param = param;
            oVentaD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
            oVentaDSqlServer = new Datos.Venta(empresa, param);
        }


        public Entidades.Venta getVentaById(int idVenta)
        {
            Entidades.Venta oVentaE = oVentaD.getVentaById(idVenta);
            return oVentaE;
        }
        public List<Entidades.Venta> getAllVentas(
                                            DateTime fechaDesde,
                                            DateTime fechaHasta,
                                            string texto,
                                            int? idVendedor,
                                            int? idCliente,
                                            int? idSucursal,
                                            bool soloAnulados,
                                            bool cargarLineas)
        {
            return oVentaD.getAllVentas(fechaDesde, fechaHasta, texto, idVendedor, idCliente, idSucursal, soloAnulados, cargarLineas);
        }

        public List<Entidades.Venta> getVentasBalancePeriodo(
                                            DateTime fechaDesde,
                                            DateTime fechaHasta,
                                            int? idSucursal)
        {
            return oVentaD.getVentasBalancePeriodo(fechaDesde, fechaHasta, idSucursal);
        }

        public decimal getTotalKgsPesablesBalancePeriodo(
                                            DateTime fechaDesde,
                                            DateTime fechaHasta,
                                            int? idSucursal,
                                            bool incluirVentasCuentaCorriente)
        {
            return oVentaD.getTotalKgsPesablesBalancePeriodo(fechaDesde, fechaHasta, idSucursal, incluirVentasCuentaCorriente);
        }

        public int agregarVenta(Entidades.Venta oVentaE, bool esNotaCredito = false)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    //Se carga la comision segun el tipo de tarjeta (Este valor se obtiene desde tabla parametros)
                    switch (oVentaE.FormaPago.ToString())
                    {
                        case "Efectivo":
                            oVentaE.ComisionTarjeta = 0;
                            break;
                        case "Debito":
                            oVentaE.ComisionTarjeta = _param.GetFloat(ParamKeys.ComisionDebito, 0f);
                            break;
                        case "Credito":
                            oVentaE.ComisionTarjeta = _param.GetFloat(ParamKeys.ComisionCredito, 0f);
                            break;
                        default:
                            oVentaE.ComisionTarjeta = 0;
                            break;
                    }
                    oVentaE.IdVenta = oVentaD.agregarVenta(oVentaE);

                    ///llama al metodo para asinar el idVenta a la tabla Expendios
                    ///
                    if (oVentaE.ListaExpendios != null)
                    {
                        foreach (int item in oVentaE.ListaExpendios)
                            oVentaD.asignarVentaEnExpendio(oVentaE.IdVenta, item);
                    }

                    /////para no repetir lineas, se multiplica por el multiplicador 
                    /////esto cuando es NC, lo hace el signo inverso
                    /////
                    //int multiplicador = esNotaCredito ? -1 : 1;

                    //if (esNotaCredito)
                    //{
                    //    for (int i = 0; i < oVentaE.LineasVenta.Count; i++)
                    //    {
                    //        oVentaE.LineasVenta[i].Venta = oVentaE;
                    //        oVentaE.LineasVenta[i].CantKg *= -1;
                    //        oVentaE.LineasVenta[i].KgsTotalCalculado *= -1;
                    //        oVentaE.LineasVenta[i] = agregarLineaVenta(oVentaE.LineasVenta[i]);
                    //    }
                    //}
                    //else
                    //{

                    //    for (int index = 0; index < oVentaE.LineasVenta.Count; index++)
                    //    {
                    //        Entidades.LineaVenta linea = oVentaE.LineasVenta[index];
                    //        //setear por cada linea cantKg <- KgsTotalCalculado
                    //        linea.CantKg = linea.KgsTotalCalculado;

                    //        //si está anulada la linea se asigna el IdLineaVenta del corte anulado
                    //        linea.IndexAnulado = Entidades.LineaVenta.esAnulado(linea.Estado) ? oVentaE.LineasVenta[linea.IndexAnulado].IdLineaVenta :
                    //            Entidades.LineaVenta.getIdEstado(Entidades.LineaVenta.estados.NoAnulado);

                    //        oVentaE.LineasVenta[index] = agregarLineaVenta(linea);
                    //    }
                    //}

                    for (int i = 0; i < oVentaE.LineasVenta.Count; i++)
                    {
                        var linea = oVentaE.LineasVenta[i];
                        linea.Venta = oVentaE;

                        if (esNotaCredito)
                        {
                            linea.CantKg *= -1;
                            linea.KgsTotalCalculado *= -1;
                        }
                        else
                        {
                            // setear por cada línea cantKg <- KgsTotalCalculado
                            linea.CantKg = linea.KgsTotalCalculado;

                            // si está anulada la línea se asigna el IdLineaVenta del corte anulado
                            linea.IndexAnulado = Entidades.LineaVenta.esAnulado(linea.Estado)
                                ? oVentaE.LineasVenta[linea.IndexAnulado].IdLineaVenta
                                : Entidades.LineaVenta.getIdEstado(Entidades.LineaVenta.estados.NoAnulado);
                        }

                        oVentaE.LineasVenta[i] = agregarLineaVenta(linea);
                    }


                    egresoCajaPagoTarjeta(oVentaE);//(oVentaE.IdVenta, oVentaE.Vendedor, oVentaE.PagoMixtoEfectivo);

                    crearMovCtaCteVenta(oVentaE);

                    // si todo salió bien, confirmamos
                    scope.Complete();

                    return oVentaE.IdVenta;
                }
                catch (Exception ex)
                {
                    // si algo falla NO llamamos a scope.Complete()
                    // y automáticamente se hace rollback
                    throw new Exception("Error en registrar la venta: \n" + ex.Message, ex);
                }
            }
        }

        // Corrige la alicuota de una linea ya insertada (agregarLineaVenta la hardcodea desde el Corte usado).
        public void actualizarAlicuotaLineaVenta(int idLineaVenta, int idAlicuotaIva, float alicuotaIva)
        {
            oVentaD.actualizarAlicuotaLineaVenta(idLineaVenta, idAlicuotaIva, alicuotaIva);
        }

        // Borrado minimo de lineas, sin los efectos colaterales de modificarVenta(eliminarLineas:true).
        public void eliminarLineasVenta(int idVenta)
        {
            oVentaD.eliminarLineasVenta(idVenta);
        }

        public void modificarVenta(Entidades.Venta oVentaE, int SucAnterior, bool eliminarLineas, List<Entidades.LineaVenta> lineaNuevosAnulados)
        {
            ////Se carga la comision segun el tipo de tarjeta (Este valor se obtiene desde tabla parametros)
            //switch (oVentaE.FormaPago.ToString())
            //{
            //    case "Efectivo":
            //        oVentaE.ComisionTarjeta = 0;
            //        break;
            //    case "Debito":
            //        oVentaE.ComisionTarjeta = Entidades.ParamKeys.comisionDebito;
            //        break;
            //    case "Credito":
            //        oVentaE.ComisionTarjeta = Entidades.ParamKeys.comisionCredito;
            //        break;
            //    default:
            //        oVentaE.ComisionTarjeta = 0;
            //        break;
            //}
            //oVentaD.modificarVenta(oVentaE, SucAnterior, eliminarLineas);

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    //Se carga la comision segun el tipo de tarjeta (Este valor se obtiene desde tabla parametros)
                    switch (oVentaE.FormaPago.ToString())
                    {
                        case "Efectivo":
                            oVentaE.ComisionTarjeta = 0;
                            break;
                        case "Debito":
                            oVentaE.ComisionTarjeta = _param.GetFloat(ParamKeys.ComisionDebito, 0f);
                            break;
                        case "Credito":
                            oVentaE.ComisionTarjeta = _param.GetFloat(ParamKeys.ComisionCredito, 0f);
                            break;
                        default:
                            oVentaE.ComisionTarjeta = 0;
                            break;
                    }
                    oVentaD.modificarVenta(oVentaE, SucAnterior, eliminarLineas);

                    if (lineaNuevosAnulados != null)
                    {
                        foreach (Entidades.LineaVenta lineaNuevoAnulado in lineaNuevosAnulados)
                        {
                            agregarLineaVenta(lineaNuevoAnulado);
                        }
                    }
                    ///Si se llema a eliminar lineas - Desde FormUltimaVenta no se eliminan pero sí en formModificarVenta
                    if (eliminarLineas)
                    {
                        for (int i = 0; i < oVentaE.LineasVenta.Count; i++)
                        {
                            var linea = oVentaE.LineasVenta[i];
                            linea.Venta = oVentaE;
                            linea.CantKg = linea.KgsTotalCalculado;
                            linea.IndexAnulado = Entidades.LineaVenta.esAnulado(linea.Estado)
                                ? oVentaE.LineasVenta[linea.IndexAnulado].IdLineaVenta
                                : Entidades.LineaVenta.getIdEstado(Entidades.LineaVenta.estados.NoAnulado);

                            oVentaE.LineasVenta[i] = agregarLineaVenta(linea);
                        }
                    }

                    egresoCajaPagoTarjeta(oVentaE);//(oVentaE.IdVenta, oVentaE.Vendedor, oVentaE.PagoMixtoEfectivo);

                    crearMovCtaCteVenta(oVentaE);

                    // si todo salió bien, confirmamos
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    // si algo falla NO llamamos a scope.Complete()
                    // y automáticamente se hace rollback
                    throw new Exception("Error en registrar la venta: \n" + ex.Message, ex);
                }
            }
        }
        public void crearMovCtaCteVenta(Entidades.Venta oVentaE)
        {
            //oVentaE = oVentaD.getVentaById(oVentaE.IdVenta);
            Negocio.CuentaCorriente oCtaCteN = new Negocio.CuentaCorriente(_empresa);
            oCtaCteN.crearMovCtaCte(oVentaE.Persona, oVentaE.FechaVenta, Entidades.MovCtaCte.tablas.Ventas, oVentaE.IdVenta, oVentaE.NroRemito,
                //"", Entidades.MovCtaCte.tipoMov.Debito, oVentaE.LineasVenta.Count == 0 ? 0 : Entidades.Venta. oVentaD.getTotalVenta(oVentaE.IdVenta), oVentaE.Sucursal,
                "", Entidades.MovCtaCte.tipoMov.Debito, oVentaE.LineasVenta.Count == 0 ? 0 : oVentaE.getImporteVenta(oVentaE), oVentaE.Sucursal,
                oVentaE.Creado, oVentaE.Vendedor, oVentaE.Actualizado, null, oVentaE.EnCtaCte, null, null, null);      
        }

        public float getTotalVenta(int idVenta)
        {
            return oVentaD.getTotalVenta(idVenta);
        }

        public float getTotalKgsVenta(int idVenta)
        {
            return oVentaD.getTotalKgsVenta(idVenta);
        }

        public DataTable obtenerVentas(int idSucursal, int idCliente, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool soloAnulados)
        {
            return oVentaD.obtenerVentas(idSucursal, idCliente, idVendedor, fechaDesde, fechaHasta, texto, soloAnulados);
        }


        public DataTable getVentasVendedorCierreCaja(Entidades.CierreCaja oCierreE, bool soloAnulados)
        {
            return oVentaD.getVentasVendedorCierreCaja(oCierreE, soloAnulados);
        }

        public float obtenerTotalVentas(int idVendedor, int idSucursal, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            return oVentaD.obtenerTotalVentas(idVendedor, idSucursal, fechaDesde, fechaHasta);
        } 

        public Entidades.LineaVenta agregarLineaVenta(Entidades.LineaVenta oLineaE)
        {
            oLineaE.AjustePrecio = oLineaE.PrecioKg - oLineaE.Corte.precioKgReferencia;
            return oVentaD.agregarLineaVenta(oLineaE);
        }

        public  List<Entidades.LineaVenta> obtenerLineasVenta(int idVenta)
        {
            return oVentaD.obtenerLineasVenta(idVenta);
        }

        public DataTable obtenerUltimosPreciosPorCliente(int idPersona, int topVentas = 10)
        {
            return oVentaD.obtenerUltimosPreciosPorCliente(idPersona, topVentas);
        }

        public void agregarStockVenta(Entidades.Venta oVentaE)
        {
            oVentaD.agregarStockVenta(oVentaE);
        }

        public Entidades.Venta getUltimaVentaVendedor(Entidades.CierreCaja oCierreE)
        {
            return oVentaD.getUltimaVentaVendedor(oCierreE);
        }

        public void agregarTemporalLineaVenta(Entidades.TemporalLineaVenta oTemporalLV)
        {
            oVentaD.agregarTemporalLineaVenta(oTemporalLV);
        }

        public DataTable obtenerTemporalLineaVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool conVentas)
        {
            return oVentaD.obtenerTemporalLineaVenta(idSucursal, idVendedor, fechaDesde, fechaHasta, texto, conVentas);
        }

        public DataTable getAllLineasVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            return oVentaD.getAllLineasVenta(idSucursal, idVendedor, fechaDesde, fechaHasta, texto);
        }

        public DataTable ultimasVentasCliente(int idSucursal, int idPersona)
        {
            return oVentaD.ultimasVentasCliente(idSucursal, idPersona);
        }

        public void egresoCajaPagoTarjeta(Entidades.Venta oVentaConEgresoCaja)//(int idVenta, Entidades.Usuario oUsuario, float pagoMixtoEfectivo)
        {
            //Entidades.Venta oVentaConEgresoCaja = getVentaById(idVenta);

            bool esEfectivo = oVentaConEgresoCaja.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString());                   
            //se genera el egreso de caja si no es Efectivo
            if (!esEfectivo)
            {
                float totalS = 0, totalKgs = 0;// getTotalVenta(oVentaConEgresoCaja.IdVenta);
                foreach (Entidades.LineaVenta linea in oVentaConEgresoCaja.LineasVenta)
                {
                    totalKgs += linea.CantKg;
                    totalS += (linea.CantKg * linea.PrecioKg); 
                }

                Entidades.EgresoCaja oEgresoCajaE = new Entidades.EgresoCaja();

                oEgresoCajaE.Fecha = oVentaConEgresoCaja.FechaVenta;
                string formaPagoDetalle = "";
                if ((oVentaConEgresoCaja.FormaPago.Equals(Entidades.Venta.formaPagoEnum.CtaCte.ToString())))
                {
                    oEgresoCajaE.IdTipoEgresoCaja = Entidades.EgresoCaja.idCtaCte;
                    oEgresoCajaE.Descripcion = "Venta a " + oVentaConEgresoCaja.Persona.razonSocial + " - ID:" + oVentaConEgresoCaja.IdVenta.ToString();
                }
                else
                {
                    formaPagoDetalle = oVentaConEgresoCaja.PagoMixtoEfectivo > 0 ? 
                        "Mixta - T$"+ totalS.ToString("N2") + " -> " + oVentaConEgresoCaja.FormaPago.ToString()+" $"+(totalS - oVentaConEgresoCaja.PagoMixtoEfectivo).ToString("N2")+" | "+"Efvo $"+oVentaConEgresoCaja.PagoMixtoEfectivo.ToString("N2") : 
                        oVentaConEgresoCaja.FormaPago.ToString();
                    oEgresoCajaE.IdTipoEgresoCaja = Entidades.EgresoCaja.idPagoTarjeta;
                    oEgresoCajaE.Descripcion = "Venta " + formaPagoDetalle + " - ID:" + oVentaConEgresoCaja.IdVenta.ToString();
                }

                oEgresoCajaE.Monto = totalS - oVentaConEgresoCaja.PagoMixtoEfectivo;// oVentaN.getTotalVenta(oVentaConEgresoCaja.IdVenta);
                oEgresoCajaE.Detalle = " | Kgs: " + totalKgs.ToString("N3") +
                    " | Precio: " + (totalS / totalKgs).ToString("N3") +
                    " | TOT: " + totalS.ToString("N3") + "\n\n" + oVentaConEgresoCaja.Observaciones;
                oEgresoCajaE.Sucursal = oVentaConEgresoCaja.Sucursal;
                oEgresoCajaE.IdCompra = 0;
                oEgresoCajaE.Tabla = Entidades.EgresoCaja.tablas.Ventas.ToString();
                oEgresoCajaE.IdTabla = oVentaConEgresoCaja.IdVenta;
                oEgresoCajaE.CreadoPor = oVentaConEgresoCaja.Vendedor.Id;
                oEgresoCajaE.ActualizadoPor = oEgresoCajaE.Id > 0 ? (oVentaConEgresoCaja.Vendedor != null ? oVentaConEgresoCaja.Vendedor.Id : -1) : -1;

                Negocio.CierreCaja oCierreN = new Negocio.CierreCaja(_empresa);
                oEgresoCajaE = oCierreN.addOrEditEgresoCaja(oEgresoCajaE);
            }
        }

        public void actualizarCliente(int idVenta, int idPersona)
        {
            oVentaD.actualizarCliente(idVenta, idPersona);
        }

        #region CARGA EXHAUSTIVA
        public void cargaExhaustiva(Entidades.Venta oVentaE, List<Entidades.LineaVenta> listaLineaVenta)
        {

            DateTime dateTime = DateTime.Now.AddDays(-1000);
            for (int i = 0; i < 365; i++)
            {
                dateTime = dateTime.AddDays(i);
                for (int j = 0; j < 300; j++)
                {
                    if (dateTime.Year > 2025 && dateTime.Month > 2)
                    {
                        string d = "2";
                        dateTime = dateTime.AddYears(2025 - dateTime.Year);
                        dateTime = dateTime.AddMonths(3 - dateTime.Month);
                        //return;
                    }
                    oVentaE.FechaVenta = dateTime;
                    oVentaE.Sucursal.idSucursal = 2;
                    oVentaE.IdVenta = agregarVenta(oVentaE);

                    Random random = new Random();
                    int min = 6594; // Límite inferior
                    int max = 6829; // Límite superior

                    int idCorte_Random = random.Next(min, max + 1); // Incluye el límite superior

                    for (int index = 0; index < listaLineaVenta.Count; index++)
                    {
                        Entidades.LineaVenta linea = listaLineaVenta[index];
                        //setear por cada linea cantKg <- KgsTotalCalculado
                        linea.CantKg = linea.KgsTotalCalculado;
                        linea.Venta.IdVenta = oVentaE.IdVenta;
                        linea.Corte.idCorte = random.Next(min, max + 1);
                        try
                        {
                            agregarLineaVenta(linea);
                        }
                        catch (Exception)
                        {
                            int d = linea.Corte.idCorte;
                        }
                    }
                }

            }
        }
        #endregion


        #region EXPENDIO
        public int agregarExpendio(Entidades.Venta oVentaE)
        {
           return oVentaD.agregarExpendio(oVentaE);
        }
        public Entidades.LineaVenta agregarLineaExprendio(Entidades.LineaVenta oLineaE)
        {
            return oVentaD.agregarLineaExprendio(oLineaE);
        }
        public DataTable obtenerUltimosExpendios(int ultimosMinutos, int idSucursal)
        {
            return oVentaD.obtenerUltimosExpendios(ultimosMinutos, idSucursal);
        }
        public DataTable obtenerExpendiosPorUsuario(int idSucursal, int idVendedor, int top = 100, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            return oVentaD.obtenerExpendiosPorUsuario(idSucursal, idVendedor, top, fechaDesde, fechaHasta);
        }
        public DataTable obtenerExpendiosEmpresa(int top = 300, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            return oVentaD.obtenerExpendiosEmpresa(top, fechaDesde, fechaHasta);
        }
        public DataTable obtenerSectores()
        {
            return oVentaD.obtenerSectores();
        }

        public bool existeSector(string sector, string sectorActual = "")
        {
            return oVentaD.existeSector(sector, sectorActual);
        }

        public void agregarSector(string sector)
        {
            oVentaD.agregarSector(sector);
        }

        public void modificarSector(string sectorActual, string sectorNuevo)
        {
            oVentaD.modificarSector(sectorActual, sectorNuevo);
        }

        public bool sectorEstaEnUso(string sector)
        {
            return oVentaD.sectorEstaEnUso(sector);
        }

        public void eliminarSector(string sector)
        {
            oVentaD.eliminarSector(sector);
        }

        public DataTable obtenerSectoresConTodos()
        {
            DataTable dtSectores = obtenerSectores();
            DataRow drSector = dtSectores.NewRow();
            drSector["Sector"] = "TODOS";
            dtSectores.Rows.Add(drSector);

            // Mover 'TODOS' al principio
            DataRow filaTodos = dtSectores.Rows[dtSectores.Rows.Count - 1];
            DataRow filaNueva = dtSectores.NewRow();
            filaNueva.ItemArray = filaTodos.ItemArray;

            dtSectores.Rows.Remove(filaTodos);
            dtSectores.Rows.InsertAt(filaNueva, 0);

            return dtSectores;
        }

        public string getUltimoSectorSelect(string serialCPU)
        {
            return oVentaD.getUltimoSectorSelect(serialCPU);
        }

        public Entidades.Venta getExpedioById(int idExpendio)
        {
            return oVentaD.getExpedioById((int)idExpendio);
        }
        #endregion


        #region FACTURA ELECTRONICA

        /// <summary>
        /// Retorna cero si está pendiente de facturacion (CAE es vacio)
        /// </summary>
        /// <param name="idVenta"></param>
        /// <returns></returns>
            public int esVentaSinFacturar(int idVenta, bool esNotaCredito)
        {
            return oVentaDSqlServer.esVentaSinFacturar(idVenta, esNotaCredito);
        }
        
        /// <summary>
        /// Retorna ID factura electronica para el idVenta. Cero si no existe.
        /// </summary>
        /// <param name="idVenta"></param>
        /// <returns></returns>
        public int existeFactuElectParaVenta(int idVenta)
        {
            int idFactuElec = oVentaDSqlServer.existeFacturaElect(idVenta);
            return idFactuElec;
        }

        public int existeNotaCreditoParaVenta(int idVenta)
        {
            return oVentaDSqlServer.existeNotaCreditoElect(idVenta);
        }

        public void addOrEditFactuElec(Entidades.FacturaElectronica oFacturaElectronicaE)
        {
            oVentaDSqlServer.addOrEditFactuElec(oFacturaElectronicaE);

            char letraId_TipoCbte = oFacturaElectronicaE.getLetraId_TipoCbte(oFacturaElectronicaE.CodTipoCbteAfip);

            //actualizar el tipo cbte de la tabla Ventas
            if (!(oFacturaElectronicaE.Venta != null &&
                oFacturaElectronicaE.Venta.TipoComprobante.Equals(letraId_TipoCbte)))
            {
                oVentaD.actualizarLetraId_TipoCbte(oFacturaElectronicaE.IdVenta, letraId_TipoCbte);
            }
            
            ///si el cuit se lo ingresa al facturar y no en la venta 
            ///se actualiza el cliente en caso de existir en BD
            if (oFacturaElectronicaE.TipoDocAfip == FacturaElectronica.codTipoDoc_CUIT.ToString())
            {
                oFacturaElectronicaE.Venta = oVentaD.getVentaById(oFacturaElectronicaE.IdVenta);
                if (oFacturaElectronicaE.Venta != null &&
                    oFacturaElectronicaE.NroDocAfip != oFacturaElectronicaE.Venta.Persona.Cuit)
                {
                    Negocio.Persona personaN = new Negocio.Persona(_empresa, _param);
                    int idPersona = personaN.existeCuit(oFacturaElectronicaE.NroDocAfip.ToString());
                    if (idPersona > 0)
                        oVentaD.actualizarCliente(oFacturaElectronicaE.Venta.IdVenta, idPersona);
                }
            }
        }

        public Entidades.FacturaElectronica getFactuElecById(int idFactuElec)
        {
            return oVentaDSqlServer.getFactuElecById(idFactuElec);
        }

        public List<Entidades.FacturaElectronica> BuscarFacturasPagina(
            DateTime fechaDesde, DateTime fechaHasta, int idSucursal,
            string cliente, string vendedor, List<string> formasPago, List<int> codigosComprobante,
            int pagina, int cantidad, int cantidadExtra)
        {
            return oVentaDSqlServer.BuscarFacturasPagina(fechaDesde, fechaHasta, idSucursal, cliente, vendedor, formasPago, codigosComprobante, pagina, cantidad, cantidadExtra);
        }

        public (int Cantidad, decimal Total) ObtenerFacturasResumen(
            DateTime fechaDesde, DateTime fechaHasta, int idSucursal,
            string cliente, string vendedor, List<string> formasPago, List<int> codigosComprobante)
        {
            return oVentaDSqlServer.ObtenerFacturasResumen(fechaDesde, fechaHasta, idSucursal, cliente, vendedor, formasPago, codigosComprobante);
        }

        #endregion

    }
}
