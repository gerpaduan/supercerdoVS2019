using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Negocio
{
    public class CuentaCorriente
    {
        Datos.CuentaCorriente oCtaCteD = new Datos.CuentaCorriente();


        #region Pagos

        public void agregarPago(Entidades.Pagos oPagoE)
        {
            oCtaCteD.agregarPago(oPagoE);
        }

        public void modificarPago(Entidades.Pagos oPagoE)
        {
            oCtaCteD.modificarPago(oPagoE);
        }

        public void eliminarPago(Entidades.Pagos oPagoE)
        {
            oCtaCteD.eliminarPago(oPagoE);
        }

        public DataTable obtenerPagos(string tipoTramite, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            return oCtaCteD.obtenerPagos(tipoTramite, texto, fechaDesde, fechaHasta);
        }

        public Entidades.Pagos buscarPago(Entidades.Pagos oPagoE)
        {
            return oCtaCteD.buscarPago(oPagoE);
        }

        #endregion
    }
}
