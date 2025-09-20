using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Transactions;

namespace Negocio
{
    public class Venta
    {
        Datos.Venta oVentaD = new Datos.Venta();

        public int agregarVenta(Entidades.Venta oVentaE)
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
                            oVentaE.ComisionTarjeta = Entidades.Parametros.comisionDebito;
                            break;
                        case "Credito":
                            oVentaE.ComisionTarjeta = Entidades.Parametros.comisionCredito;
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

        public void modificarVenta(Entidades.Venta oVentaE, int SucAnterior, bool eliminarLineas, List<Entidades.LineaVenta> lineaNuevosAnulados)
        {
            ////Se carga la comision segun el tipo de tarjeta (Este valor se obtiene desde tabla parametros)
            //switch (oVentaE.FormaPago.ToString())
            //{
            //    case "Efectivo":
            //        oVentaE.ComisionTarjeta = 0;
            //        break;
            //    case "Debito":
            //        oVentaE.ComisionTarjeta = Entidades.Parametros.comisionDebito;
            //        break;
            //    case "Credito":
            //        oVentaE.ComisionTarjeta = Entidades.Parametros.comisionCredito;
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
                            oVentaE.ComisionTarjeta = Entidades.Parametros.comisionDebito;
                            break;
                        case "Credito":
                            oVentaE.ComisionTarjeta = Entidades.Parametros.comisionCredito;
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
            oVentaE = oVentaD.getVentaById(oVentaE.IdVenta);
            Negocio.CuentaCorriente oCtaCteN = new Negocio.CuentaCorriente();
            oCtaCteN.crearMovCtaCte(oVentaE.Persona, oVentaE.FechaVenta, Entidades.MovCtaCte.tablas.Ventas, oVentaE.IdVenta, oVentaE.NroRemito,
                "", Entidades.MovCtaCte.tipoMov.Debito, oVentaE.LineasVenta.Count == 0 ? 0 : oVentaD.getTotalVenta(oVentaE.IdVenta), oVentaE.Sucursal,
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

        public Entidades.Venta getVentaById(int idVenta)
        {
            return oVentaD.getVentaById(idVenta);
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

                Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
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
        public DataTable obtenerSectores()
        {
            return oVentaD.obtenerSectores();
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
            return oVentaD.esVentaSinFacturar(idVenta, esNotaCredito);
        }
        
        /// <summary>
        /// Retorna ID factura electronica para el idVenta. Cero si no existe.
        /// </summary>
        /// <param name="idVenta"></param>
        /// <returns></returns>
        public int existeFactuElectParaVenta(int idVenta)
        {
            int idFactuElec = oVentaD.existeFacturaElect(idVenta);
            return idFactuElec;
        }

        public void addOrEditFactuElec(Entidades.FacturaElectronica oFacturaElectronicaE)
        {
            oVentaD.addOrEditFactuElec(oFacturaElectronicaE);

            char letraId_TipoCbte = oFacturaElectronicaE.getLetraId_TipoCbte(oFacturaElectronicaE.CodTipoCbteAfip);

            //actualizar el tipo cbte de la tabla Ventas
            if (!(oFacturaElectronicaE.Venta != null &&
                oFacturaElectronicaE.Venta.TipoComprobante.Equals(letraId_TipoCbte)))
            {
                oVentaD.actualizarLetraId_TipoCbte(oFacturaElectronicaE.IdVenta, letraId_TipoCbte);
            }
        }

        public Entidades.FacturaElectronica getFactuElecById(int idFactuElec)
        {
            return oVentaD.getFactuElecById(idFactuElec);
        }

        #endregion
    }
}
