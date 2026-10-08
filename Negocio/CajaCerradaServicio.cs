using System;
using System.Collections.Generic;

namespace Negocio
{
    // Un registro (venta, compra, pago, egreso) ubicado por su dueno + sucursal + fecha: la caja a la que pertenece se
    // deduce por rango (no hay FK registro->caja).
    public sealed class CajaCerradaConsulta
    {
        public int IdDueno { get; private set; }
        public int IdSucursal { get; private set; }
        public DateTime Fecha { get; private set; }

        public CajaCerradaConsulta(int idDueno, int idSucursal, DateTime fecha)
        {
            IdDueno = idDueno;
            IdSucursal = idSucursal;
            Fecha = fecha;
        }
    }

    // Cambios en cajas YA CERRADAS (2026-10-06, ver docs/DECISIONS.md "Cambios en cajas cerradas y reapertura"):
    // Evaluar -> que cajas cerradas afecta un cambio; Registrar -> deja el aviso en cada una (auditoriacierrecaja).
    // Lo usan los controllers de Ventas, Compras, Finanzas (pagos/cobros) y Cajas (egresos). La decision de pedir
    // confirmacion es de CambioCajaCerrada.HuboCambioQueAfectaCierre (regla pura). Solo Postgres: con SQL Server
    // (Soporta = false) Evaluar devuelve vacio y Registrar no hace nada.
    public class CajaCerradaServicio
    {
        private readonly Negocio.CierreCaja _cierreN;

        public CajaCerradaServicio(Negocio.CierreCaja cierreN)
        {
            _cierreN = cierreN ?? throw new ArgumentNullException(nameof(cierreN));
        }

        public bool Soporta { get { return _cierreN.SoportaAuditoriaCierre; } }

        // Cajas cerradas distintas (sin duplicar por Id) que contienen alguna de las consultas. Se evaluan siempre la
        // fecha ORIGINAL y la NUEVA de un registro: cambiar la fecha puede moverlo entre cajas.
        public List<Entidades.CierreCaja> Evaluar(params CajaCerradaConsulta[] consultas)
        {
            var resultado = new List<Entidades.CierreCaja>();
            if (!Soporta || consultas == null) return resultado;

            var vistos = new HashSet<int>();
            foreach (var consulta in consultas)
            {
                if (consulta == null) continue;

                var caja = _cierreN.BuscarCajaCerradaQueContiene(consulta.IdDueno, consulta.IdSucursal, consulta.Fecha);
                if (caja != null && caja.Id > 0 && vistos.Add(caja.Id))
                    resultado.Add(caja);
            }
            return resultado;
        }

        // Deja un registro de auditoria por cada caja afectada. Se llama DESPUES de guardar el origen (fuera de su
        // transaccion): un fallo aqui NO deshace el cambio ya hecho, solo se loguea (deuda aceptada, ver DECISIONS).
        public void Registrar(string origen, int idOrigen, string accion, IEnumerable<Entidades.CierreCaja> cajas,
            int idUsuario, string usuario, double? importeAnterior, double? importeNuevo,
            string formaPagoAnterior, string formaPagoNueva, string detalle)
        {
            if (!Soporta || cajas == null) return;

            foreach (var caja in cajas)
            {
                try
                {
                    _cierreN.registrarAuditoriaCierre(new Entidades.AuditoriaCierreCaja
                    {
                        IdCierreCaja = caja.Id,
                        Tipo = Entidades.AuditoriaCierreCaja.TipoModificacion,
                        Origen = origen,
                        IdOrigen = idOrigen > 0 ? idOrigen : (int?)null,
                        Accion = accion,
                        IdUsuario = idUsuario,
                        Usuario = usuario,
                        ImporteAnterior = importeAnterior,
                        ImporteNuevo = importeNuevo,
                        FormaPagoAnterior = formaPagoAnterior,
                        FormaPagoNueva = formaPagoNueva,
                        Detalle = detalle
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("No se pudo registrar el aviso en el cierre de caja " + caja.Id +
                        " (" + origen + " " + idOrigen + "): " + ex.Message);
                }
            }
        }
    }
}
