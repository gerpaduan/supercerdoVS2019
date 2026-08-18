using System;
using System.Data;

namespace Contratos
{
    // Espeja Datos.OtrasClases (SQL Server), salvo Login(string) -- toca la tabla Claves,
    // confirmado sin ningun caller en todo el repo (docs/DECISIONS.md, 2026-08-18), no se migra.
    public interface IOtrasClasesRepository
    {
        bool existeLicencia(string nroLicencia);
        void agregarLicencia(string nroLicencia, string identificacion);
        DataTable obtenerVencimientoLicencia(DateTime fechaDesde);
        DateTime fechaVencimientoLicencia();
        bool existePagoLicenciaHoy();
        void agregaVencimientosLicencia(DateTime fechaDesde);
        void agregarPagoCuota(DateTime fechaVencimiento);
    }
}
