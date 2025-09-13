using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Entidades;
using System.Data.SqlClient;

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
            Entidades.Usuario actualizadoPor, bool crearMovCtaCte)
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
                ///TODO: cargar el egreso de caja por pago. lo pongo acá para tambien registar el archivo duplicado
                ///


                ///--si tiene mov cta cte y tiene el mismo TipoMov se actualiza                
                ///--si tiene mov cta cte y es distinto tipo se crea un registro opuesto
                ///-----
                ///Si no coincide importe y tipo de mov. se crea un opuesto y luego el nuevo registro
                if (!(oMovCtaCte.Tipo.Equals(tipoMov.ToString()) && 
                    oMovCtaCte.Importe.Equals(oMovCtaCte.getImporte(importe,tipoMov))))
                {
                    oMovCtaCte.Id = 0;
                    oMovCtaCte.Detalle = "";
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
                            CargarEgresoCajaPorPago(oMovCtaCte);
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

        public int getUltimoIdPago()
        {
            return oCtaCteD.getUltimoIdPago();
        }

        public Entidades.Pago getPagoById(int idPago)
        {
            return oCtaCteD.getPagoById(idPago);
        }

        public Entidades.Pago addOrEditPago(Entidades.Pago oPagoE)
        {
            ///obtener los cheques del pago antes de modificar y comparo si se eliminar cheques del pago, los receteo
            ///
            List<Cheque> listaCheques = oCtaCteD.getChequesPorPago(oPagoE.Id);
            foreach (Cheque cheque in listaCheques)
            {
                bool yaExiste = oPagoE.Cheques.Any(c => c.Id == cheque.Id);

                if (!yaExiste)
                    oCtaCteD.resetearChequesAsignados(oPagoE.Id);
            }

            return oCtaCteD.addOrEditPago(oPagoE);
        }

        public void eliminarPago(Entidades.Pago oPagoE)
        {
            oCtaCteD.eliminarPago(oPagoE);
        }

        public DataTable obtenerPagos(string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            return oCtaCteD.obtenerPagos(texto, fechaDesde, fechaHasta);
        }

        public void crearMovCtaCtePago(Entidades.Pago oPagoE, Entidades.CierreCaja oCierreCajaE)
        {
            oPagoE = oCtaCteD.getPagoById(oPagoE.Id);
            Negocio.CuentaCorriente oCtaCteN = new Negocio.CuentaCorriente();
            oCtaCteN.crearMovCtaCte(oPagoE.Persona, oPagoE.Fecha, Entidades.MovCtaCte.tablas.Pagos, oPagoE.Id, oPagoE.NroRecibo,
                 oPagoE.FormaPago, oPagoE.AProveedor ? Entidades.MovCtaCte.tipoMov.Debito : Entidades.MovCtaCte.tipoMov.Credito, oPagoE.Importe, oPagoE.Sucursal,
                oPagoE.Creado, oPagoE.CreadoPor, oPagoE.Actualizado, null, true);
        }

        private static void CargarEgresoCajaPorPago(Entidades.MovCtaCte oMovCtaCte)//Pago oPagoE, Entidades.CierreCaja oCierreCajaE)
        {
            ///Si se llama desde POS, generar el egreso de caja de pago/cobro
            ///
            Entidades.EgresoCaja oEgresoCajaE = new Entidades.EgresoCaja();

            string descripcionEgreso = oPagoE.AProveedor ? "Pago a " : "Cobro a ";
            string detalleEgreso = string.Empty;
            float montoEgreso = oPagoE.Importe;

            switch (oPagoE.FormaPago.ToUpper())
            {
                case "EFECTIVO":
                    montoEgreso = oPagoE.AProveedor ? oPagoE.Importe : (-1 * oPagoE.Importe);//se multiplica *-1 para que sume a la caja
                    detalleEgreso = " | " + oPagoE.FormaPago + " $" + oPagoE.Importe.ToString("F2");
                    break;

                case "EFTVO+CHEQUE":
                    montoEgreso = oPagoE.AProveedor ? oPagoE.Efectivo : (-1 * oPagoE.Efectivo);//se multiplica *-1 para que sume a la caja
                    detalleEgreso = " | Cheques $" + (oPagoE.Importe - oPagoE.Efectivo).ToString("F2") + " | EF $" + oPagoE.Efectivo;
                    break;

                default:
                    montoEgreso = 0;
                    detalleEgreso = " | " + oPagoE.FormaPago + " $" + montoEgreso.ToString("F2");
                    break;
            }

            descripcionEgreso += oPagoE.Persona.razonSocial + " - ID:" + oPagoE.Id.ToString() + detalleEgreso;

            oEgresoCajaE.Fecha = oPagoE.Fecha;
            oEgresoCajaE.IdTipoEgresoCaja = Entidades.Parametros.idPagoCobroEgresoCaja;
            oEgresoCajaE.Descripcion = descripcionEgreso;
            oEgresoCajaE.Monto = montoEgreso;
            oEgresoCajaE.Detalle = oPagoE.Observaciones;
            oEgresoCajaE.Sucursal = oPagoE.Sucursal;
            oEgresoCajaE.IdCompra = 0;
            oEgresoCajaE.CreadoPor = oEgresoCajaE.Id > 0 ? oPagoE.CreadoPor.Id : oCierreCajaE.UsuarioInicio.Id;
            oEgresoCajaE.ActualizadoPor = oEgresoCajaE.Id > 0 ? oCierreCajaE.UsuarioInicio.Id : 0;
            Negocio.CierreCaja oCierreN = new CierreCaja();
            oCierreN.addOrEditEgresoCaja(oEgresoCajaE);
        }

        #endregion
    }
}
