using System.Data;

namespace DatosPostgres
{
    // Presupuestos (expendios del sector PRESUPUESTO) de un cliente, de SOLO LECTURA, para el
    // historial de precios de Ventas y de Expendio (2026-10-04, ver docs/DECISIONS.md). El estado
    // de cada producto (vigente / caducado / reemplazado) NO se calcula aca: lo decide
    // Negocio.PresupuestosCliente con la hora del servidor. Ver Contratos.IPresupuestoClienteRepository.
    public class PresupuestoClientePg : Contratos.IPresupuestoClienteRepository
    {
        // Mismo valor que Negocio.SectorPuntoExpendio.Presupuesto (DatosPostgres no referencia Negocio).
        private const string SectorPresupuesto = "PRESUPUESTO";

        // Tope de cordura de presupuestos por cliente (los mas recientes). Un cliente normal tiene
        // unos pocos por ano; el tope solo evita una respuesta desmedida ante datos anomalos.
        private const int MaximoPresupuestosPorCliente = 200;

        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public PresupuestoClientePg(string connectionString, int idEmpresa)
        {
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        // LEFT JOIN a lineas y producto: un presupuesto sin lineas (o con un producto ya borrado)
        // igual aparece, con idcorte NULL en esa fila. RLS filtra por empresa (DbPg abre la
        // conexion con el tenant).
        public DataTable obtenerPresupuestosPorCliente(int idPersona)
        {
            const string sql = @"
                WITH presupuestos AS (
                    SELECT e.idexpendio, e.fechaexpendio, e.fechacaducidad, e.importe
                    FROM expendios e
                    WHERE e.idpersona = @idPersona
                      AND upper(btrim(e.sector)) = @sector
                    ORDER BY e.fechaexpendio DESC, e.idexpendio DESC
                    LIMIT @tope
                )
                SELECT p.idexpendio, p.fechaexpendio, p.fechacaducidad, p.importe,
                       le.idcorte, c.codigo, c.corte AS producto, le.preciokg, le.cantkg
                FROM presupuestos p
                LEFT JOIN lineaexpendio le ON le.idexpendio = p.idexpendio
                LEFT JOIN corte c ON c.idcorte = le.idcorte
                ORDER BY p.fechaexpendio DESC, p.idexpendio DESC, le.idlineaexpendio;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idPersona", idPersona);
                p.AddWithValue("sector", SectorPresupuesto);
                p.AddWithValue("tope", MaximoPresupuestosPorCliente);
            });
        }
    }
}
