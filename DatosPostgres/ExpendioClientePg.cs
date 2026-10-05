using System;
using System.Data;

namespace DatosPostgres
{
    // Expendios de un cliente real (expendios.idpersona), de SOLO LECTURA, para el filtro
    // "Persona" del modal de expendios del POS de Ventas (2026-10-04, ver docs/DECISIONS.md).
    // El filtro va en SQL (no en memoria) porque con el rango de fechas ampliado la consulta sin
    // filtrar traeria una fila por cada producto de todos los expendios de varios meses.
    // Ver Contratos.IExpendioClienteRepository.
    public class ExpendioClientePg : Contratos.IExpendioClienteRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public ExpendioClientePg(string connectionString, int idEmpresa)
        {
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        // Mismo SELECT, JOINs y columnas que VentaPg.obtenerExpendiosAvanzado (el controller mapea
        // las filas igual para las dos consultas): solo se suma e.idpersona = @idPersona. RLS
        // filtra por empresa (DbPg abre la conexion con el tenant).
        public DataTable obtenerExpendiosPorPersona(int idPersona, DateTime fechaDesde, DateTime? fechaHasta, int idSucursal)
        {
            return DbPg.DataTable(_connectionString, _idEmpresa, @"
                SELECT e.fechaexpendio,
                       e.idexpendio,
                       identificacionexpendio,
                       sector,
                       c.codigo,
                       c.corte,
                       le.cantkg,
                       le.preciokg,
                       (le.cantkg * le.preciokg) AS total,
                       idventa,
                       u.nombre AS vendedor,
                       e.observaciones,
                       e.nroremito,
                       e.idsucursal,
                       s.sucursal AS sucursalnombre
                FROM expendios e
                INNER JOIN lineaexpendio le ON e.idexpendio = le.idexpendio
                INNER JOIN corte c ON le.idcorte = c.idcorte
                INNER JOIN usuarios u ON e.idvendedor = u.id
                INNER JOIN sucursal s ON e.idsucursal = s.idsucursal
                WHERE e.idpersona = @idPersona
                  AND e.fechaexpendio >= @fechaDesde
                  AND (@fechaHasta::timestamp IS NULL OR e.fechaexpendio <= @fechaHasta)
                  AND (@idSucursal = 0 OR e.idsucursal = @idSucursal)
                ORDER BY e.fechaexpendio;",
                p =>
                {
                    p.AddWithValue("idPersona", idPersona);
                    p.AddWithValue("fechaDesde", fechaDesde);
                    p.AddWithValue("fechaHasta", (object)fechaHasta ?? DBNull.Value);
                    p.AddWithValue("idSucursal", idSucursal);
                });
        }
    }
}
