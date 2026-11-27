using System;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IConteoVentasRepository: dos COUNT baratos (sin traer filas) para decidir si
    // hay que advertir antes de cargar un listado grande. El aislamiento por empresa lo pone DbPg (RLS).
    public class ConteoVentasPg : Contratos.IConteoVentasRepository
    {
        // Filtro comun de las dos consultas (los valores viajan siempre como parametros).
        private const string Filtro = @"
                WHERE v.fechaventa >= @fechaDesde AND v.fechaventa <= @fechaHasta
                  AND (@idSucursal = -1 OR v.idsucursal = @idSucursal)
                  AND NOT (v.idventa = ANY(@idsExcluidos))";

        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public ConteoVentasPg(string connectionString, int idEmpresa)
        {
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        public long ContarVentas(DateTime fechaDesde, DateTime fechaHasta, int idSucursal, int[] idsExcluidos)
        {
            return Contar("SELECT COUNT(*) FROM ventas v" + Filtro, fechaDesde, fechaHasta, idSucursal, idsExcluidos);
        }

        public long ContarLineas(DateTime fechaDesde, DateTime fechaHasta, int idSucursal, int[] idsExcluidos)
        {
            return Contar("SELECT COUNT(*) FROM ventas v INNER JOIN lineaventa l ON l.idventa = v.idventa" + Filtro,
                fechaDesde, fechaHasta, idSucursal, idsExcluidos);
        }

        private long Contar(string sql, DateTime fechaDesde, DateTime fechaHasta, int idSucursal, int[] idsExcluidos)
        {
            object cantidad = DbPg.Scalar(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("idsExcluidos", idsExcluidos ?? new int[0]);
            });
            return Convert.ToInt64(cantidad);
        }
    }
}
