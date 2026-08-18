using System;
using System.Collections.Generic;
using System.Data;

namespace Contratos
{
    // Espeja Datos.CuentaCorriente (MovCtaCte, Pagos, Cheques, Bancos). El overload privado
    // AddOrEditCheque(SqlConnection, SqlTransaction, Cheque) es detalle de implementacion de
    // SQL Server, no va en la interfaz. Ver docs/DECISIONS.md, Etapa 5.
    public interface ICuentaCorrienteRepository
    {
        DataTable obtenerCtasCtes(string txtBusqueda, int? idPersona, string ordenSaldo = "DESC");
        DataTable obtenerResumenDashboard();
        DataTable getCtaCteByIdPersona(int idPersona, DateTime fechaDesde);
        Entidades.MovCtaCte getMovCtaCteBy(int id, Entidades.MovCtaCte.tablas tabla, int idTabla, Entidades.MovCtaCte.getBy getBy);
        Entidades.MovCtaCte addOrEditMovCtaCte(Entidades.MovCtaCte oMovCtaCteE);

        DataTable obtenerCheques(string texto, DateTime fechaDesde, DateTime fechaHasta, bool soloPropios, string estado);
        Entidades.Cheque getChequePorIDorNro(int id, string nroCheque);
        List<Entidades.Cheque> getChequesPorPago(int idPago, bool conPagos = true);
        bool AddOrEditCheque(Entidades.Cheque oCheque);
        bool EliminarCheque(int id);
        bool resetearChequesAsignados(int idPago);
        List<string> getBancos();

        int getUltimoIdPago();
        Entidades.Pago addOrEditPago(Entidades.Pago oPagoE);
        // Bug real preexistente en SQL Server: el SP "eliminarPago" no existe en la base
        // (confirmado contra sys.procedures), solo alcanzable desde Presentacion/WinForms.
        // Fuera de alcance de esta migracion -- ver docs/DECISIONS.md, Etapa 5.
        void eliminarPago(Entidades.Pago oPagoE);
        DataTable obtenerPagos(string texto, DateTime fechaDesde, DateTime fechaHasta);
        DataTable obtenerTotalesPagosBalance(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal);
        DataTable obtenerUltimosPagosDashboard(int cantidad);
        DataTable obtenerChequesPendientesDashboard(int cantidad, DateTime fechaActual);
        Entidades.Pago getPagoById(int idPago, bool conCheques = true);
    }
}
