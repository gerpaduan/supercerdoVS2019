using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Entidades;
using System.Data.SqlClient;
using System.Transactions;
using Datos;

namespace Negocio
{
    public class CuentaCorriente
    {
        Datos.CuentaCorriente oCtaCteD = new Datos.CuentaCorriente();

        public DataTable obtenerCtasCtes(string txtBusqueda, int? idPersona)
        {
            return oCtaCteD.obtenerCtasCtes(txtBusqueda, idPersona);
        }

        public DataTable getCtaCteByIdPersona(int idPersona, DateTime fechaDesde)
        {
            DataTable dtMovCtaCte = oCtaCteD.getCtaCteByIdPersona(idPersona, fechaDesde);

            for (int fila = 0; fila < dtMovCtaCte.Rows.Count; fila++)
            {
                dtMovCtaCte.Rows[fila]["Saldo"] = fila.Equals(0) ? dtMovCtaCte.Rows[fila]["importe"] : float.Parse(dtMovCtaCte.Rows[fila - 1]["Saldo"].ToString()) + float.Parse(dtMovCtaCte.Rows[fila]["importe"].ToString());
            }

            return dtMovCtaCte;
        }

        public void crearMovCtaCte(Entidades.Persona oPersonaE, DateTime fecha,
            Entidades.MovCtaCte.tablas tabla, int idTabla, string nroDoc, string detalle, Entidades.MovCtaCte.tipoMov tipoMov, float importe,
            Entidades.Sucursal oSucursalE, DateTime? creado, Entidades.Usuario creadoPor, DateTime? actualizado,
            Entidades.Usuario actualizadoPor, bool crearMovCtaCte, Entidades.CierreCaja oCierreCajaE, Entidades.Pago oPagoE, Entidades.Pago oPagoAnterior)
        {
            Datos.CuentaCorriente oCtaCteN = new Datos.CuentaCorriente();
            Entidades.MovCtaCte oMovCtaCte = oCtaCteN.getMovCtaCteBy(0, tabla, idTabla, Entidades.MovCtaCte.getBy.TablaAndId);

            ///si no tiene oMovCtaCte o Tiene y fue quitado de la cta se la crea 
            if ((oMovCtaCte == null || oMovCtaCte.Id.Equals(0)) || oMovCtaCte.QuitadoCtaCta)
            {
                oMovCtaCte = new Entidades.MovCtaCte();
            }
            else
            {
                ///--si tiene mov cta cte y tiene el mismo TipoMov se actualiza                
                ///--si tiene mov cta cte y es distinto tipo se crea un registro opuesto
                ///-----
                ///Si no coincide importe y tipo de mov. se crea un opuesto y luego el nuevo registro
                if (!(oMovCtaCte.Tipo.Equals(tipoMov.ToString()) && 
                    oMovCtaCte.Importe.Equals(oMovCtaCte.getImporte(importe,tipoMov))))
                {
                    oMovCtaCte.Id = 0;
                    oMovCtaCte.Detalle = "ANULACION" + " - " +oMovCtaCte.Detalle;
                    oMovCtaCte.Tipo = oMovCtaCte.getTipoMovOpuesto(oMovCtaCte.getTipoMovEnum(oMovCtaCte.Tipo));
                    oMovCtaCte.Importe = oMovCtaCte.getImporte(oMovCtaCte.Importe, oMovCtaCte.getTipoMovEnum(oMovCtaCte.Tipo));
                    //se registra el registro opuesto
                    oCtaCteD.addOrEditMovCtaCte(oMovCtaCte);

                    ///se crea la nueva instancia para el nuevo registro
                    ///**Solo si el nuevo registro tiene distinto tipoMov (p/que no se registre 2 veces el mov cta cte)**
                    switch (oMovCtaCte.getTablaEnum(oMovCtaCte.Tabla))
                    {
                        case Entidades.MovCtaCte.tablas.Compras:
                            break;
                        case Entidades.MovCtaCte.tablas.Ventas:
                            if(oMovCtaCte.getTipoMovEnum(oMovCtaCte.Tipo).Equals(Entidades.MovCtaCte.tipoMov.Debito))
                                return;
                            break;
                        case Entidades.MovCtaCte.tablas.Pagos:
                            CargarEgresoCajaPorPago(oMovCtaCte, oCierreCajaE, oPagoAnterior, true);
                            break;
                        case Entidades.MovCtaCte.tablas.MovCtaCte:
                            break;
                        default:
                            break;
                    }
                    oMovCtaCte = new Entidades.MovCtaCte();                    
                }

                ///-Si coincide Importe y tipo Mov y EnCtaCte es Falso Siginifica que se sacó la venta de Cta Cte
                ///
                if (!crearMovCtaCte && oMovCtaCte.Tipo.Equals(tipoMov.ToString()) &&
                    oMovCtaCte.Importe.Equals(oMovCtaCte.getImporte(importe, tipoMov)))
                {
                    oMovCtaCte.Id = 0;
                    oMovCtaCte.Detalle = !crearMovCtaCte ? "Quitado de Cta.Cte." : "";
                    oMovCtaCte.QuitadoCtaCta = !crearMovCtaCte; 
                    oMovCtaCte.Tipo = oMovCtaCte.getTipoMovOpuesto(oMovCtaCte.getTipoMovEnum(oMovCtaCte.Tipo));
                    oMovCtaCte.Importe = oMovCtaCte.getImporte(oMovCtaCte.Importe, oMovCtaCte.getTipoMovEnum(oMovCtaCte.Tipo));
                    //se registra el registro opuesto
                    oCtaCteD.addOrEditMovCtaCte(oMovCtaCte);
                    CargarEgresoCajaPorPago(oMovCtaCte, oCierreCajaE, oPagoE, false);

                    //se crea la nueva instancia para el nuevo registro
                    oMovCtaCte = new Entidades.MovCtaCte();
                }
                
            }
            //si crearMovCtaCte es falso se aborta el proceso
            if (!crearMovCtaCte) return;

            oMovCtaCte.Persona = oPersonaE;
            oMovCtaCte.Fecha = fecha;
            oMovCtaCte.Tabla = tabla.ToString();
            oMovCtaCte.IdTabla = idTabla;
            oMovCtaCte.NroDoc = nroDoc;
            oMovCtaCte.Detalle = detalle;
            oMovCtaCte.Tipo = tipoMov.ToString();
            oMovCtaCte.Importe = oMovCtaCte.getImporte(importe, tipoMov);
            oMovCtaCte.Sucursal = oSucursalE;
            oMovCtaCte.Creado = creado;
            oMovCtaCte.CreadoPor = creadoPor;
            oMovCtaCte.Actualizado = actualizado;
            oMovCtaCte.ActualizadoPor = actualizadoPor;

            oCtaCteD.addOrEditMovCtaCte(oMovCtaCte);
            CargarEgresoCajaPorPago(oMovCtaCte, oCierreCajaE, oPagoE, false);
        }

        #region Cheques

        public DataTable obtenerCheques(string texto, DateTime fechaDesde, DateTime fechaHasta, bool soloPropios, string estado)
        {
            return oCtaCteD.obtenerCheques(texto, fechaDesde, fechaHasta, soloPropios, estado);
        }

        public Cheque getChequePorIDorNro(int id, string nroCheque)
        {
            return oCtaCteD.getChequePorIDorNro(id, nroCheque);
        }

        public bool AddOrEditCheque(Cheque oCheque)
        {
            return oCtaCteD.AddOrEditCheque(oCheque);
        }
        public bool EliminarCheque(int id)
        {
            return oCtaCteD.EliminarCheque(id);
        }

        public List<string> getBancos()
        {            
            return oCtaCteD.getBancos();
        }

        #endregion

        #region Pagos

        public string getNroReciboAutomatico(int idSucursal)
        {
            return idSucursal.ToString("D3") + "-" + (getUltimoIdPago() + 1).ToString("D8");
        }

        public int getUltimoIdPago()
        {
            return oCtaCteD.getUltimoIdPago();
        }

        public Entidades.Pago getPagoById(int idPago)
        {
            Pago oPagoE = oCtaCteD.getPagoById(idPago);
            if (oPagoE != null)
            {
                oPagoE.FormaPago_ = (Pago.formasPago)Enum.Parse(typeof(Pago.formasPago), oPagoE.FormaPago);
                oPagoE.ImporteDec = (decimal)oPagoE.Importe;
                oPagoE.EfectivoDec = (decimal)oPagoE.Efectivo;
            }
            return oPagoE;
        }

        public Entidades.Pago addOrEditPago(Entidades.Pago oPagoE, Entidades.CierreCaja oCierreCajaE, Entidades.Pago oPagoSinMod)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    // obtener los cheques del pago antes de modificar
                    List<Cheque> listaCheques = oCtaCteD.getChequesPorPago(oPagoE.Id);
                    foreach (Cheque cheque in listaCheques)
                    {
                        bool yaExiste = oPagoE.Cheques.Any(c => c.Id == cheque.Id);

                        if (!yaExiste)
                            oCtaCteD.resetearChequesAsignados(oPagoE.Id);
                    }

                    // guardar o editar el pago
                    oPagoE = oCtaCteD.addOrEditPago(oPagoE);

                    // crear movimiento y egreso de caja
                    crearMovCtaCtePago(oPagoE, oCierreCajaE, oPagoSinMod);

                    // si todo salió bien, confirmamos
                    scope.Complete();

                    return oPagoE;
                }
                catch (Exception ex)
                {
                    // si algo falla NO llamamos a scope.Complete()
                    // y automáticamente se hace rollback
                    throw new Exception("Error en addOrEditPago: " + ex.Message, ex);
                }
            }
        }

        public void eliminarPago(Entidades.Pago oPagoE)
        {
            oCtaCteD.eliminarPago(oPagoE);
        }

        public DataTable obtenerPagos(string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            return oCtaCteD.obtenerPagos(texto, fechaDesde, fechaHasta);
        }

        public void crearMovCtaCtePago(Entidades.Pago oPagoE, Entidades.CierreCaja oCierreCajaE, Entidades.Pago oPagoAnterior)
        {
            //oPagoE = oCtaCteD.getPagoById(oPagoE.Id); COMENTADO XQ YA ENVIO EL OBJETO POR PARAMETRO
            Negocio.CuentaCorriente oCtaCteN = new Negocio.CuentaCorriente();
            oCtaCteN.crearMovCtaCte(oPagoE.Persona, oPagoE.Fecha, Entidades.MovCtaCte.tablas.Pagos, oPagoE.Id, oPagoE.NroRecibo,
                 oPagoE.FormaPago, oPagoE.AProveedor ? Entidades.MovCtaCte.tipoMov.Debito : Entidades.MovCtaCte.tipoMov.Credito, oPagoE.Importe, oPagoE.Sucursal,
                oPagoE.Creado, oPagoE.CreadoPor, oPagoE.Actualizado, null, true, oCierreCajaE, oPagoE, oPagoAnterior);
        }

        private static void CargarEgresoCajaPorPago(Entidades.MovCtaCte oMovCtaCte, Entidades.CierreCaja oCierreCajaE, 
            Entidades.Pago oPagoE, bool esRegistroAnulacion)//Pago oPagoE, Entidades.CierreCaja oCierreCajaE)
        {
            ///Si se llama desde POS, generar el egreso de caja de pago/cobro
            ///
            if ((oCierreCajaE == null || oCierreCajaE.Id == 0))
                return;

            ///si es ANULACION recupera se carga el pago del importe en efectivo correspondiente al pago de esta caja abierta
            ///
            Negocio.CierreCaja oCierreN = new CierreCaja();
            ///consulto si existe un egreso de caja para la tabla y id
            ///si no existe, creo nuevo objeto
            ///
            Entidades.EgresoCaja oEgresoCajaE = oCierreN.findEgresoCajaByTablaYId(Entidades.EgresoCaja.tablas.Pagos.ToString(), oPagoE.Id);
            ///(Si es anulación ó no existe un registro anterior ó existe registro anterior y son 
            if (oEgresoCajaE == null || oEgresoCajaE.Id == 0)
                oEgresoCajaE = new Entidades.EgresoCaja();

            //si es anulacion al registro recuperado se lo setea con ID = 0 y se genera el opuesto
            if (esRegistroAnulacion)
            {
                oEgresoCajaE.Id = 0;
                oEgresoCajaE.Descripcion = "ANULACION: " + oEgresoCajaE.Descripcion;
                oEgresoCajaE.Monto = oEgresoCajaE.Monto * -1;
            }
            else
            {
                string descripcionEgreso = (oPagoE.AProveedor ? "Pago a " : "Cobro a ");
                string detalleEgreso = string.Empty;
                float montoEgreso;

                switch (oPagoE.FormaPago.ToUpper())
                {
                    case "EFECTIVO":
                        montoEgreso = oPagoE.AProveedor ? oPagoE.Importe : (-1 * oPagoE.Importe);//se multiplica *-1 para que sume a la caja
                        detalleEgreso = " | " + oPagoE.FormaPago + " $" + oPagoE.Importe.ToString("N2");
                        break;

                    case "EFTVO+CHEQUE":
                        montoEgreso = oPagoE.AProveedor ? oPagoE.Efectivo : (-1 * oPagoE.Efectivo);//se multiplica *-1 para que sume a la caja
                        detalleEgreso = " | Cheques $" + (oPagoE.Importe - oPagoE.Efectivo).ToString("N2") + " | EF $" + oPagoE.Efectivo;
                        break;

                    default:
                        montoEgreso = 0;
                        detalleEgreso = " | " + oPagoE.FormaPago + " $" + oMovCtaCte.Importe.ToString("N2");
                        break;
                }

                descripcionEgreso += oPagoE.Persona.razonSocial + " - ID:" + oPagoE.Id.ToString() + detalleEgreso;

                ///si el monto del pago es diferente al monto del Egreso de caja -> crear nuevo registro
                ///
                if (montoEgreso != oEgresoCajaE.Monto)
                    oEgresoCajaE.Id = 0;

                oEgresoCajaE.Fecha = oPagoE.Fecha;
                oEgresoCajaE.IdTipoEgresoCaja = Entidades.Parametros.idPagoCobroEgresoCaja;
                oEgresoCajaE.Descripcion = descripcionEgreso;
                oEgresoCajaE.Monto = montoEgreso;
                oEgresoCajaE.Detalle = oPagoE.Observaciones;
                oEgresoCajaE.Sucursal = oPagoE.Sucursal;
                oEgresoCajaE.IdCompra = 0;
                oEgresoCajaE.Tabla = Entidades.EgresoCaja.tablas.Pagos.ToString();
                oEgresoCajaE.IdTabla = oPagoE.Id;
                oEgresoCajaE.CreadoPor = oEgresoCajaE.Id > 0 ? oPagoE.CreadoPor.Id : oCierreCajaE.UsuarioInicio.Id;
                oEgresoCajaE.ActualizadoPor = oEgresoCajaE.Id > 0 ? oCierreCajaE.UsuarioInicio.Id : 0;
            }
            
            oCierreN.addOrEditEgresoCaja(oEgresoCajaE);
        }

        #endregion

        #region VALIDACION_CHEQUES
        public (bool ok, string mensaje, Cheque cheque) ValidarChequeParaPago(
                                                                        string nroCheque,
                                                                        Pago pagoActual,
                                                                        bool esAProveedor)
        {
            if (pagoActual == null)
                pagoActual = new Pago();

            if (pagoActual.Cheques == null)
                pagoActual.Cheques = new List<Cheque>();


            var oCheque = getChequePorIDorNro(0, nroCheque);

            //NO CAMBIAR MENSAJE PORQUE ROMPE MODAL ALTA CHEQUE EN WEB
            if (oCheque == null)
                return (false, "No existe un cheque con ese número.", null);
            //NO CAMBIAR MENSAJE PORQUE ROMPE MODAL ALTA CHEQUE EN WEB

            //infoCheque
            string infoCheque = "\nInfo Cheque:" +
                "\nNúmero: " + oCheque.NroCheque +
                "\nBanco: " + oCheque.Banco +
                "\nFecha Pago: " + oCheque.FechaPago +
                "\nImporte: " + oCheque.Importe +
                "\nTitular: " + oCheque.Titular +
                "\nRecibido de: " + (oCheque.Propio ? "Propio" : (oCheque.PagoDe != null ? oCheque.PagoDe.Persona.RazonSocial : "-")) +
                "\nEntregado a: " + (oCheque.PagoA != null ? oCheque.PagoA.Persona.RazonSocial : "-");

            // Duplicado dentro del mismo pago
            if (pagoActual.Cheques.Any(c => c.NroCheque == oCheque.NroCheque))
                return (false, "El cheque ya está asignado a este pago.", null);

            // Pagar proveedor → debe ser propio o recibido
            if (esAProveedor && !(oCheque.Propio || (oCheque.PagoDe?.Id > 0)))
                return (false, "El cheque debe ser propio o provenir de un cobro.", null);

            // Ya asignado a otro pago destino
            if (esAProveedor && oCheque.PagoA?.Id > 0 && oCheque.PagoA.Id != pagoActual.Id)
                return (false, $"El cheque ya fue asignado al pago ID {oCheque.PagoA.Id}."+ infoCheque, null);

            // Ya asignado a otro pago origen (cliente)
            if (!esAProveedor && oCheque.PagoDe?.Id > 0 && oCheque.PagoDe.Id != pagoActual.Id)
                return (false, $"El cheque ya está asignado al pago ID {oCheque.PagoDe.Id}."+ infoCheque, null);

            // Vencido
            if (oCheque.FechaPago.AddDays(30) < DateTime.Today)
                return (false, $"El cheque está vencido (Fecha Pago: {oCheque.FechaPago:dd/MM/yyyy})."+ infoCheque, null);

            return (true, "", oCheque);
        }


        #endregion
    }
}
