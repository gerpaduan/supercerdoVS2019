using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Entidades;
using System.Data.SqlClient;
using System.Transactions;
using Datos;
using Utilidades;

namespace Negocio
{
    public class CuentaCorriente
    {
        private readonly Contratos.ICuentaCorrienteRepository oCtaCteD;

        IEmpresaContext _empresa;private readonly IParametrosContext _param;

        // Constructor existente: SIN CAMBIOS. Los 11 puntos de instanciacion actuales siguen igual.
        public CuentaCorriente(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa;_param = param;
            oCtaCteD = new Datos.CuentaCorriente(empresa, param);
        }

        // Constructor nuevo, aditivo: inyecta cualquier implementacion de ICuentaCorrienteRepository
        // (ej. DatosPostgres.CuentaCorrientePg). Solo lo usa el controller de comparacion.
        public CuentaCorriente(Contratos.ICuentaCorrienteRepository repositorio, IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa; _param = param;
            oCtaCteD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        // CierreCaja consciente del motor de datos (Postgres/SQL Server). Lo cablea NegocioFactory; si
        // no se inyecta cae a new CierreCaja(_empresa) (SQL Server), igual que CargarEgresoCajaPorPago.
        // Lo usa el codigo de Pagos agregado el 2026-09-24 (cambio de persona / eliminacion).
        public Func<Negocio.CierreCaja> CierreCajaFactory { get; set; }

        private Negocio.CierreCaja ObtenerCierreCajaN()
        {
            return CierreCajaFactory != null ? CierreCajaFactory() : new CierreCaja(_empresa);
        }

        public DataTable obtenerCtasCtes(string txtBusqueda, int? idPersona, string ordenSaldo = "DESC")
        {
            return oCtaCteD.obtenerCtasCtes(txtBusqueda, idPersona, ordenSaldo);
        }

        public DataTable obtenerResumenDashboard()
        {
            return oCtaCteD.obtenerResumenDashboard();
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

        // unitOfWork opcional: ver Contratos/IUnitOfWork.cs. Solo se propaga a los llamados a
        // oCtaCteD -- CargarEgresoCajaPorPago (mas abajo) sigue sin compartir la transaccion,
        // pero para el camino de Venta es un no-op real (oCierreCajaE siempre llega null desde
        // crearMovCtaCteVenta), asi que no bloquea el fix de Venta. Pendiente si algun dia se
        // necesita atomicidad completa tambien para el flujo de Pagos/Cheques.
        public void crearMovCtaCte(Entidades.Persona oPersonaE, DateTime fecha,
            Entidades.MovCtaCte.tablas tabla, int idTabla, string nroDoc, string detalle, Entidades.MovCtaCte.tipoMov tipoMov, float importe,
            Entidades.Sucursal oSucursalE, DateTime? creado, Entidades.Usuario creadoPor, DateTime? actualizado,
            Entidades.Usuario actualizadoPor, bool crearMovCtaCte, Entidades.CierreCaja oCierreCajaE, Entidades.Pago oPagoE, Entidades.Pago oPagoAnterior,
            Contratos.IUnitOfWork unitOfWork = null)
        {
            Entidades.MovCtaCte oMovCtaCte = oCtaCteD.getMovCtaCteBy(0, tabla, idTabla, Entidades.MovCtaCte.getBy.TablaAndId, unitOfWork);

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
                    oCtaCteD.addOrEditMovCtaCte(oMovCtaCte, unitOfWork);

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
                /// oMovCtaCte.Tipo puede ser null aca: si el bloque de arriba (tipo/importe
                /// distintos) ya corrio, reseteo oMovCtaCte a una instancia en blanco (linea
                /// de arriba) -- en ese caso el registro viejo ya quedo reversado por ese
                /// bloque y este chequeo no aplica (nada que "sacar de cta cte", ya se sacó).
                if (!crearMovCtaCte && oMovCtaCte.Tipo != null && oMovCtaCte.Tipo.Equals(tipoMov.ToString()) &&
                    oMovCtaCte.Importe.Equals(oMovCtaCte.getImporte(importe, tipoMov)))
                {
                    oMovCtaCte.Id = 0;
                    oMovCtaCte.Detalle = !crearMovCtaCte ? "Quitado de Cta.Cte." : "";
                    oMovCtaCte.QuitadoCtaCta = !crearMovCtaCte; 
                    oMovCtaCte.Tipo = oMovCtaCte.getTipoMovOpuesto(oMovCtaCte.getTipoMovEnum(oMovCtaCte.Tipo));
                    oMovCtaCte.Importe = oMovCtaCte.getImporte(oMovCtaCte.Importe, oMovCtaCte.getTipoMovEnum(oMovCtaCte.Tipo));
                    //se registra el registro opuesto
                    oCtaCteD.addOrEditMovCtaCte(oMovCtaCte, unitOfWork);
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

            oCtaCteD.addOrEditMovCtaCte(oMovCtaCte, unitOfWork);
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

        // unitOfWork: ver Contratos/IUnitOfWork.cs. En SQL Server oCtaCteD.IniciarUnitOfWork()
        // devuelve null y el metodo sigue con TransactionScope de siempre. En Postgres arma un
        // UnitOfWorkPg real y lo threadea a getChequesPorPago/resetearChequesAsignados/addOrEditPago/
        // crearMovCtaCtePago para que Pagos+Cheques+MovCtaCte queden en una sola transaccion
        // (mismo bug que Venta/Compra: el auto-enlistment ambiente de Npgsql con TransactionScope
        // no preserva el tenant de RLS entre las distintas conexiones que abre cada llamado).
        public Entidades.Pago addOrEditPago(Entidades.Pago oPagoE, Entidades.CierreCaja oCierreCajaE, Entidades.Pago oPagoSinMod)
        {
            var unitOfWork = oCtaCteD.IniciarUnitOfWork();
            if (unitOfWork == null)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        var resultado = EjecutarAddOrEditPago(oPagoE, oCierreCajaE, oPagoSinMod, null);
                        scope.Complete();
                        RegistrarEfectosPostGuardadoPago(resultado, oPagoSinMod);
                        return resultado;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error en addOrEditPago: " + ex.Message, ex);
                    }
                }
            }
            else
            {
                using (unitOfWork)
                {
                    try
                    {
                        var resultado = EjecutarAddOrEditPago(oPagoE, oCierreCajaE, oPagoSinMod, unitOfWork);
                        unitOfWork.Completar();
                        RegistrarEfectosPostGuardadoPago(resultado, oPagoSinMod);
                        return resultado;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error en addOrEditPago: " + ex.Message, ex);
                    }
                }
            }
        }

        private Entidades.Pago EjecutarAddOrEditPago(Entidades.Pago oPagoE, Entidades.CierreCaja oCierreCajaE,
            Entidades.Pago oPagoSinMod, Contratos.IUnitOfWork unitOfWork)
        {
            // un pago eliminado es historia contable (ya tiene su asiento opuesto): no se puede editar
            if (oPagoSinMod != null && oPagoSinMod.Eliminado)
                throw new InvalidOperationException("El pago fue eliminado y no puede modificarse.");

            // obtener los cheques del pago antes de modificar
            List<Cheque> listaCheques = oCtaCteD.getChequesPorPago(oPagoE.Id, true, unitOfWork);
            foreach (Cheque cheque in listaCheques)
            {
                bool yaExiste = oPagoE.Cheques.Any(c => c.Id == cheque.Id);

                if (!yaExiste)
                    oCtaCteD.resetearChequesAsignados(oPagoE.Id, unitOfWork);
            }

            // guardar o editar el pago
            oPagoE = oCtaCteD.addOrEditPago(oPagoE, unitOfWork);

            // crear movimiento y egreso de caja
            crearMovCtaCtePago(oPagoE, oCierreCajaE, oPagoSinMod, unitOfWork);

            // si cambio la persona: auditoria (misma transaccion) y descripcion del egreso de caja
            RegistrarCambioDePersonaDelPago(oPagoE, oPagoSinMod, unitOfWork);

            return oPagoE;
        }

        // El pago se cargo en una fecha distinta a la del dia en que se creo el registro (ej. cobro de
        // ayer cargado hoy, o fecha editada despues): el UI lo muestra como advertencia y el admin
        // recibe una notificacion en la campana (RegistrarEfectosPostGuardadoPago).
        public static bool TieneFechaDistintaALaCarga(Entidades.Pago oPagoE)
        {
            return oPagoE != null && oPagoE.Id > 0 && oPagoE.Creado.HasValue
                && oPagoE.Fecha.Date != oPagoE.Creado.Value.Date;
        }

        // Cambio de persona de un pago existente. La cta cte NO se anula/recrea: crearMovCtaCte muta el
        // MovCtaCte en el lugar cuando solo cambia la persona (decision del usuario 2026-09-24, ver
        // docs/DECISIONS.md), por eso la trazabilidad vive en auditoriapagos, no en el libro mayor.
        private void RegistrarCambioDePersonaDelPago(Entidades.Pago oPagoNuevo, Entidades.Pago oPagoAnterior,
            Contratos.IUnitOfWork unitOfWork)
        {
            if (oPagoAnterior == null || oPagoAnterior.Id <= 0 || oPagoAnterior.Persona == null || oPagoNuevo.Persona == null)
                return;
            if (oPagoAnterior.Persona.idPersona == oPagoNuevo.Persona.idPersona)
                return;

            int idUsuario = oPagoNuevo.ActualizadoPor != null ? oPagoNuevo.ActualizadoPor.Id
                : (oPagoNuevo.CreadoPor != null ? oPagoNuevo.CreadoPor.Id : 0);

            oCtaCteD.registrarAuditoriaPago(new Entidades.AuditoriaPago
            {
                IdPago = oPagoNuevo.Id,
                Tipo = Entidades.AuditoriaPago.TipoCambioPersona,
                IdPersonaAnterior = oPagoAnterior.Persona.idPersona,
                IdPersonaNueva = oPagoNuevo.Persona.idPersona,
                IdUsuario = idUsuario,
                Detalle = "De " + oPagoAnterior.Persona.razonSocial + " (ID " + oPagoAnterior.Persona.idPersona + ") a "
                    + oPagoNuevo.Persona.razonSocial + " (ID " + oPagoNuevo.Persona.idPersona + ")"
            }, unitOfWork);

            // El egreso de caja lleva el nombre de la persona en su descripcion: se actualiza el texto
            // (monto y fecha no cambian, asi que la caja no se toca; se permite aunque la caja este cerrada).
            // TODO: no participa del UnitOfWork (limitacion preexistente de los egresos de caja de pagos).
            if (string.IsNullOrWhiteSpace(oPagoAnterior.Persona.razonSocial))
                return;

            var oCierreN = ObtenerCierreCajaN();
            Entidades.EgresoCaja oEgresoCajaE = oCierreN.findEgresoCajaByTablaYId(Entidades.EgresoCaja.tablas.Pagos.ToString(), oPagoNuevo.Id);
            if (oEgresoCajaE != null && oEgresoCajaE.Id > 0 && !string.IsNullOrEmpty(oEgresoCajaE.Descripcion)
                && oEgresoCajaE.Descripcion.Contains(oPagoAnterior.Persona.razonSocial))
            {
                oEgresoCajaE.Descripcion = oEgresoCajaE.Descripcion.Replace(oPagoAnterior.Persona.razonSocial, oPagoNuevo.Persona.razonSocial);
                oEgresoCajaE.ActualizadoPor = idUsuario;
                oCierreN.addOrEditEgresoCaja(oEgresoCajaE);
            }
        }

        // Despues del commit: avisa al admin si la fecha del pago difiere del dia de carga. Es una
        // advertencia, no parte de la operacion contable: si falla no deshace el pago, se registra el error.
        private void RegistrarEfectosPostGuardadoPago(Entidades.Pago oPagoGuardado, Entidades.Pago oPagoAnterior)
        {
            if (oPagoGuardado == null || oPagoGuardado.Id <= 0) return;

            DateTime creado = (oPagoAnterior != null && oPagoAnterior.Creado.HasValue) ? oPagoAnterior.Creado.Value
                : (oPagoGuardado.Creado ?? DateTime.Now);
            if (oPagoGuardado.Fecha.Date == creado.Date) return;

            // reabre la notificacion (vuelve a "sin atender") solo si es un pago nuevo o si cambio la fecha
            bool reabrir = oPagoAnterior == null || oPagoAnterior.Id <= 0 || oPagoAnterior.Fecha != oPagoGuardado.Fecha;
            try
            {
                oCtaCteD.upsertNotificacion(new Entidades.Notificacion
                {
                    IdSucursal = oPagoGuardado.Sucursal != null ? (int?)oPagoGuardado.Sucursal.idSucursal : null,
                    Tipo = Entidades.Notificacion.TipoPagoFechaDistinta,
                    Severidad = Entidades.Notificacion.SeveridadAdvertencia,
                    Titulo = "Pago con fecha distinta a la de carga",
                    Mensaje = (oPagoGuardado.AProveedor ? "Pago" : "Cobro") + " " + oPagoGuardado.NroRecibo + " de "
                        + (oPagoGuardado.Persona != null ? oPagoGuardado.Persona.razonSocial : "-")
                        + " por $" + oPagoGuardado.Importe.ToString("N2") + ": fecha del pago "
                        + oPagoGuardado.Fecha.ToString("dd/MM/yyyy") + ", cargado el " + creado.ToString("dd/MM/yyyy") + ".",
                    RefId = oPagoGuardado.Id
                }, reabrir);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("No se pudo crear la notificacion PAGO_FECHA_DISTINTA del pago " + oPagoGuardado.Id + ": " + ex);
            }
        }

        public void eliminarPago(Entidades.Pago oPagoE)
        {
            oCtaCteD.eliminarPago(oPagoE);
        }

        // Eliminacion logica de un pago/cobro: NO borra nada. Marca el pago como eliminado, genera el
        // asiento opuesto en la cta cte ("ELIMINADO - ..."), el egreso de caja opuesto (si el pago tenia
        // uno, fechado HOY para no tocar una caja ya cerrada), libera los cheques asignados, deja
        // auditoria y avisa al admin (campana). Todo lo contable en una sola transaccion.
        // Devuelve (false, mensaje) si no corresponde (no existe, ya eliminado, sin motivo, cheque recibido
        // ya entregado a otro pago). Ver docs/DECISIONS.md "Pagos: listado, cambio de persona y eliminacion".
        public (bool ok, string mensaje) eliminarPagoConContraasiento(int idPago, string motivo, Entidades.Usuario oUsuarioE)
        {
            if (oUsuarioE == null || oUsuarioE.Id <= 0)
                return (false, "No se pudo identificar al usuario que elimina el pago.");
            if (string.IsNullOrWhiteSpace(motivo))
                return (false, "Ingrese el motivo de la eliminación.");
            motivo = motivo.Trim();

            Entidades.Pago oPagoE = oCtaCteD.getPagoById(idPago);
            if (oPagoE == null)
                return (false, "El pago no existe.");
            if (oPagoE.Eliminado)
                return (false, "El pago ya fue eliminado.");

            // Un cobro con cheques recibidos que ya se entregaron en otro pago no se puede eliminar: el
            // cheque ya circulo y liberarlo dejaria el otro pago apuntando a un cheque sin origen.
            if (!oPagoE.AProveedor && oPagoE.Cheques != null)
            {
                foreach (Cheque chequeDelPago in oPagoE.Cheques)
                {
                    Cheque cheque = oCtaCteD.getChequePorIDorNro(chequeDelPago.Id, "");
                    if (cheque != null && cheque.PagoA != null && cheque.PagoA.Id > 0 && cheque.PagoA.Id != idPago)
                        return (false, "El cheque " + cheque.NroCheque + " de este cobro ya fue entregado en el pago ID "
                            + cheque.PagoA.Id + ". Elimine primero ese pago.");
                }
            }

            var unitOfWork = oCtaCteD.IniciarUnitOfWork();
            try
            {
                bool ok;
                if (unitOfWork == null)
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        ok = EjecutarEliminarPago(oPagoE, motivo, oUsuarioE, null);
                        if (ok) scope.Complete();
                    }
                }
                else
                {
                    using (unitOfWork)
                    {
                        ok = EjecutarEliminarPago(oPagoE, motivo, oUsuarioE, unitOfWork);
                        if (ok) unitOfWork.Completar();
                    }
                }

                if (!ok)
                    return (false, "El pago ya fue eliminado.");
            }
            catch (Exception ex)
            {
                throw new Exception("Error en eliminarPagoConContraasiento: " + ex.Message, ex);
            }

            NotificarPagoEliminado(oPagoE, motivo, oUsuarioE);
            return (true, string.Empty);
        }

        private bool EjecutarEliminarPago(Entidades.Pago oPagoE, string motivo, Entidades.Usuario oUsuarioE,
            Contratos.IUnitOfWork unitOfWork)
        {
            // 1) marca (idempotente: false = otro usuario lo elimino en el medio)
            if (!oCtaCteD.marcarPagoEliminado(oPagoE.Id, oUsuarioE.Id, motivo, unitOfWork))
                return false;

            // 2) asiento opuesto en la cta cte, mismo criterio que la ANULACION de crearMovCtaCte. Se toma el
            // ultimo movimiento del pago; si ya estaba "Quitado de Cta.Cte." no hay nada vigente que revertir.
            Entidades.MovCtaCte oMovCtaCte = oCtaCteD.getMovCtaCteBy(0, Entidades.MovCtaCte.tablas.Pagos, oPagoE.Id,
                Entidades.MovCtaCte.getBy.TablaAndId, unitOfWork);
            if (oMovCtaCte != null && oMovCtaCte.Id > 0 && !oMovCtaCte.QuitadoCtaCta)
            {
                oMovCtaCte.Id = 0;
                oMovCtaCte.Detalle = "ELIMINADO - " + oMovCtaCte.Detalle;
                oMovCtaCte.Tipo = oMovCtaCte.getTipoMovOpuesto(oMovCtaCte.getTipoMovEnum(oMovCtaCte.Tipo));
                oMovCtaCte.Importe = oMovCtaCte.getImporte(oMovCtaCte.Importe, oMovCtaCte.getTipoMovEnum(oMovCtaCte.Tipo));
                oMovCtaCte.Creado = DateTime.Now;
                oMovCtaCte.CreadoPor = oUsuarioE;
                oCtaCteD.addOrEditMovCtaCte(oMovCtaCte, unitOfWork);
            }

            // 3) cheques: se liberan (mismo reset que usa la edicion del pago)
            oCtaCteD.resetearChequesAsignados(oPagoE.Id, unitOfWork);

            // 4) egreso de caja opuesto, si el pago tenia uno (solo pagos hechos desde POS con caja abierta).
            // Va fechado HOY: la caja donde se cobro/pago puede estar cerrada y no se reescribe.
            // TODO: no participa del UnitOfWork (limitacion preexistente de los egresos de caja de pagos).
            var oCierreN = ObtenerCierreCajaN();
            Entidades.EgresoCaja oEgresoCajaE = oCierreN.findEgresoCajaByTablaYId(Entidades.EgresoCaja.tablas.Pagos.ToString(), oPagoE.Id);
            if (oEgresoCajaE != null && oEgresoCajaE.Id > 0 && oEgresoCajaE.Monto != 0)
            {
                oEgresoCajaE.Id = 0;
                oEgresoCajaE.Descripcion = "ELIMINADO: " + oEgresoCajaE.Descripcion;
                oEgresoCajaE.Monto = oEgresoCajaE.Monto * -1;
                oEgresoCajaE.Fecha = DateTime.Now;
                oEgresoCajaE.CreadoPor = oUsuarioE.Id;
                oEgresoCajaE.ActualizadoPor = 0;
                oCierreN.addOrEditEgresoCaja(oEgresoCajaE);
            }

            // 5) auditoria (misma transaccion)
            oCtaCteD.registrarAuditoriaPago(new Entidades.AuditoriaPago
            {
                IdPago = oPagoE.Id,
                Tipo = Entidades.AuditoriaPago.TipoEliminacion,
                IdPersonaAnterior = oPagoE.Persona != null ? (int?)oPagoE.Persona.idPersona : null,
                IdUsuario = oUsuarioE.Id,
                Detalle = motivo
            }, unitOfWork);

            return true;
        }

        // Despues del commit. Igual que RegistrarEfectosPostGuardadoPago: advertencia, no parte de la
        // operacion contable; si falla se registra y no se deshace la eliminacion.
        private void NotificarPagoEliminado(Entidades.Pago oPagoE, string motivo, Entidades.Usuario oUsuarioE)
        {
            try
            {
                oCtaCteD.upsertNotificacion(new Entidades.Notificacion
                {
                    IdSucursal = oPagoE.Sucursal != null ? (int?)oPagoE.Sucursal.idSucursal : null,
                    Tipo = Entidades.Notificacion.TipoPagoEliminado,
                    Severidad = Entidades.Notificacion.SeveridadAdvertencia,
                    Titulo = "Pago eliminado",
                    Mensaje = oUsuarioE.Nombre + " eliminó el " + (oPagoE.AProveedor ? "pago " : "cobro ") + oPagoE.NroRecibo + " de "
                        + (oPagoE.Persona != null ? oPagoE.Persona.razonSocial : "-") + " por $" + oPagoE.Importe.ToString("N2")
                        + ". Motivo: " + motivo,
                    RefId = oPagoE.Id
                }, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("No se pudo crear la notificacion PAGO_ELIMINADO del pago " + oPagoE.Id + ": " + ex);
            }
        }

        // Observaciones del registro origen de cada movimiento de la cta cte de la persona, indexadas por
        // "tabla|idTabla" (ej. "Pagos|123"). Solo trae los que tienen texto.
        public Dictionary<string, string> obtenerObservacionesCtaCte(int idPersona)
        {
            var observaciones = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            DataTable dt = oCtaCteD.obtenerObservacionesCtaCte(idPersona);
            foreach (DataRow fila in dt.Rows)
                observaciones[Convert.ToString(fila["tabla"]) + "|" + Convert.ToString(fila["idTabla"])] = Convert.ToString(fila["observaciones"]);
            return observaciones;
        }

        public DataTable obtenerPagos(string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            return oCtaCteD.obtenerPagos(texto, fechaDesde, fechaHasta);
        }

        public DataTable obtenerTotalesPagosBalance(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal)
        {
            return oCtaCteD.obtenerTotalesPagosBalance(fechaDesde, fechaHasta, idSucursal);
        }

        public DataTable obtenerUltimosPagosDashboard(int cantidad)
        {
            return oCtaCteD.obtenerUltimosPagosDashboard(cantidad);
        }

        public DataTable obtenerChequesPendientesDashboard(int cantidad, DateTime fechaActual)
        {
            return oCtaCteD.obtenerChequesPendientesDashboard(cantidad, fechaActual);
        }

        public void crearMovCtaCtePago(Entidades.Pago oPagoE, Entidades.CierreCaja oCierreCajaE, Entidades.Pago oPagoAnterior,
            Contratos.IUnitOfWork unitOfWork = null)
        {
            //oPagoE = oCtaCteD.getPagoById(oPagoE.Id); COMENTADO XQ YA ENVIO EL OBJETO POR PARAMETRO

            crearMovCtaCte(oPagoE.Persona, oPagoE.Fecha, Entidades.MovCtaCte.tablas.Pagos, oPagoE.Id, oPagoE.NroRecibo,
                 oPagoE.FormaPago, oPagoE.AProveedor ? Entidades.MovCtaCte.tipoMov.Debito : Entidades.MovCtaCte.tipoMov.Credito, oPagoE.Importe, oPagoE.Sucursal,
                oPagoE.Creado, oPagoE.CreadoPor, oPagoE.Actualizado, null, true, oCierreCajaE, oPagoE, oPagoAnterior, unitOfWork);
        }

        private void CargarEgresoCajaPorPago(Entidades.MovCtaCte oMovCtaCte, Entidades.CierreCaja oCierreCajaE, 
            Entidades.Pago oPagoE, bool esRegistroAnulacion)//Pago oPagoE, Entidades.CierreCaja oCierreCajaE)
        {
            ///Si se llama desde POS, generar el egreso de caja de pago/cobro
            ///
            if ((oCierreCajaE == null || oCierreCajaE.Id == 0))
                return;

            ///si es ANULACION recupera se carga el pago del importe en efectivo correspondiente al pago de esta caja abierta
            ///
            // ObtenerCierreCajaN (y no new CierreCaja(_empresa), que siempre es el repositorio SQL Server): con
            // DataEngine=Postgres el egreso de caja de un pago hecho desde POS se buscaba/guardaba contra
            // SQL Server y el guardado fallaba (2026-09-24). Sin factory inyectada (tests) cae al de antes.
            Negocio.CierreCaja oCierreN = ObtenerCierreCajaN();
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

                    case "EFTVOCHEQUE":
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
                oEgresoCajaE.IdTipoEgresoCaja = Entidades.EgresoCaja.idPagoCobroEgresoCaja;
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


        public (bool ok, string mensaje) ValidarPago(Entidades.Pago oPagoE)
        {
            if (oPagoE == null)
                return (false, "No se recibió información del pago.");

            bool formaIncluyeCheque =
                !string.IsNullOrWhiteSpace(oPagoE.FormaPago) &&
                oPagoE.FormaPago.IndexOf(Pago.formasPago.Cheque.ToString(), StringComparison.OrdinalIgnoreCase) >= 0;

            // ===============================
            // PERSONA
            // ===============================
            if (oPagoE.Persona == null ||
                oPagoE.Persona.idPersona == _param.GetInt(ParamKeys.IdConsumidorFinal, 0))
            {
                return (false,
                    "Debe seleccionar una persona válida y diferente a Consumidor Final.\n" +
                    "No pueden asignarse pagos/cobros a CF.");
            }

            // ===============================
            // FECHA
            // ===============================
            if (oPagoE.Fecha == DateTime.MinValue)
                return (false, "La fecha del pago no es válida.");

            if (oPagoE.Fecha > DateTime.Now)
                return (false, "La fecha del pago debe ser menor a la fecha y hora actual.");

            // ===============================
            // SUCURSAL
            // ===============================
            if (oPagoE.Sucursal == null)
            {
                return (false,
                    "Debe seleccionar una sucursal.");
            }

            // ===============================
            // FORMA DE PAGO
            // ===============================
            if (string.IsNullOrWhiteSpace(oPagoE.FormaPago))
                return (false, "Debe seleccionar una forma de pago.");

            // ===============================
            // EFTVO + CHEQUE
            // ===============================
            if (formaIncluyeCheque)
            {
                if (oPagoE.Cheques == null || !oPagoE.Cheques.Any())
                {
                    return (false,
                        "Debe ingresar el/los cheques que forman parte del pago.");
                }

                if (oPagoE.FormaPago == Pago.formasPago.EftvoCheque.ToString() && oPagoE.Efectivo <= 0)
                {
                    return (false,
                        "Ingrese un importe en efectivo mayor a 0.");
                }
            }
            else
            {
                // ===============================
                // IMPORTE
                // ===============================
                if (oPagoE.FormaPago != Pago.formasPago.Otro.ToString() &&
                    oPagoE.Importe <= 0)
                {
                    return (false, "Ingrese un importe mayor a 0.");
                }
            }

            // ===============================
            // CAMPOS OBLIGATORIOS
            // ===============================
            var faltantes = new List<string>();

            if (oPagoE.Persona == null)
                faltantes.Add("Persona");

            if (string.IsNullOrWhiteSpace(oPagoE.FormaPago))
                faltantes.Add("Forma de Pago");

            if (oPagoE.Importe <= 0 && oPagoE.FormaPago != "EftvoCheque")
                faltantes.Add("Importe");

            if (faltantes.Any())
            {
                return (false,
                    "Complete los siguientes campos:\n- " +
                    string.Join("\n- ", faltantes));
            }

            // ===============================
            // VALIDACION DE CHEQUES
            // ===============================
            if (formaIncluyeCheque)
            {
                var numerosCheque = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var chequeActual in oPagoE.Cheques)
                {
                    Cheque cheque = chequeActual;

                    if (cheque == null)
                        return (false, "Hay cheques inválidos en el pago.");

                    if ((cheque.Id <= 0) && string.IsNullOrWhiteSpace(cheque.NroCheque))
                        return (false, "Hay cheques inválidos en el pago.");

                    if (cheque.Id > 0)
                    {
                        cheque = getChequePorIDorNro(cheque.Id, "");
                    }
                    else if (!string.IsNullOrWhiteSpace(cheque.NroCheque))
                    {
                        cheque = getChequePorIDorNro(0, cheque.NroCheque.Trim());
                    }

                    if (cheque == null || string.IsNullOrWhiteSpace(cheque.NroCheque))
                        return (false, "Hay cheques inválidos en el pago.");

                    if (!numerosCheque.Add(cheque.NroCheque.Trim()))
                        return (false, "El mismo cheque no puede asignarse más de una vez al pago actual.");

                    var validacionCheque = ValidarChequeParaPago(cheque.NroCheque, oPagoE, oPagoE.AProveedor, true);
                    if (!validacionCheque.ok)
                        return (false, validacionCheque.mensaje);
                }
            }

            return (true, string.Empty);
        }


        #endregion

        #region VALIDACION_CHEQUES
        public (bool ok, string mensaje, Cheque cheque) ValidarChequeParaPago(
                                                                        string nroCheque,
                                                                        Pago pagoActual,
                                                                        bool esAProveedor,
                                                                        bool permitirChequeActualEnValidacion = false)
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

            bool yaAsignadoEnPagoActual = pagoActual.Cheques.Any(c =>
            {
                if (c == null)
                    return false;

                bool mismoCheque =
                    (c.Id > 0 && c.Id == oCheque.Id) ||
                    (!string.IsNullOrWhiteSpace(c.NroCheque) &&
                     c.NroCheque.Trim().Equals(oCheque.NroCheque, StringComparison.OrdinalIgnoreCase));

                if (!mismoCheque)
                    return false;

                if (!permitirChequeActualEnValidacion)
                    return true;

                bool esElMismoChequeEvaluado =
                    (c.Id > 0 && oCheque.Id > 0 && c.Id == oCheque.Id) ||
                    (!string.IsNullOrWhiteSpace(c.NroCheque) &&
                     c.NroCheque.Trim().Equals(oCheque.NroCheque, StringComparison.OrdinalIgnoreCase));

                return !esElMismoChequeEvaluado;
            });

            if (yaAsignadoEnPagoActual)
                return (false, "El cheque ya ha sido asignado al pago actual." + infoCheque, null);

            // Pagar proveedor → debe ser propio o recibido
            if (esAProveedor && !(oCheque.Propio || (oCheque.PagoDe?.Id > 0)))
                return (false, "El cheque debe ser propio o provenir de un cobro para ser asignado a un Pago.", null);

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
