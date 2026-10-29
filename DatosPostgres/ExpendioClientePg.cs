using System;
using System.Data;

namespace DatosPostgres
{
    // Expendios de una persona, de SOLO LECTURA, para el filtro "Persona" del modal de expendios
    // del POS de Ventas (2026-10-04, ver docs/DECISIONS.md; coincidencia ampliada el 2026-10-29).
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
        // las filas igual para las dos consultas): solo cambia el WHERE. Un expendio coincide con la
        // persona si tiene guardado su id (e.idpersona) O si su "Identificacion cliente"
        // (e.identificacionexpendio) es IGUAL a la identificacion de la persona (sin distinguir
        // mayusculas, acentos ni espacios de los bordes). Se compara con la identificacion y no con
        // la razon social porque hay personas distintas que comparten razon social (ej. dos cocinas
        // del mismo ministerio). Motivo: los expendios cargados con texto libre tienen idpersona
        // NULL y no aparecian (2026-10-29, ver docs/DECISIONS.md). Limite conocido: la comparacion
        // por texto no usa indice; el rango de fechas acota la cantidad de filas a recorrer.
        // RLS filtra por empresa (DbPg abre la conexion con el tenant).
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
                WHERE (e.idpersona = @idPersona
                       OR EXISTS (SELECT 1 FROM personas p
                                  WHERE p.idpersona = @idPersona
                                    AND btrim(coalesce(p.identificacion, '')) <> ''
                                    AND upper(unaccent(btrim(p.identificacion)))
                                      = upper(unaccent(btrim(coalesce(e.identificacionexpendio, ''))))))
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
