using System;
using System.Data;

namespace DatosPostgres
{
    // Cargas de un operador desde cierta fecha y hora (Stock, Movimientos, Elaborados), solo lectura, para la lista "Cargado desde
    // que abriste la caja" del menu Cargas del POS (2026-11-18, docs/DECISIONS.md "Cargas desde el POS").
    // Filtra por creadopor + creado (fecha y hora de alta, now() al insertar), NO por fechacompra / fechamovimiento / fechaembutido:
    // esas las tipea el usuario (y la de un movimiento nuevo es la llegada estimada, a futuro). Excluye las anuladas (estado Anulado).
    public class CargasPosPg : Contratos.ICargasPosRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public CargasPosPg(string connectionString, int idEmpresa)
        {
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        public DataTable ListarCargasDesde(int idOperador, DateTime desde, int limitePorTipo)
        {
            if (idOperador <= 0) return new DataTable();
            if (limitePorTipo <= 0) limitePorTipo = 30;

            // Tres consultas con el mismo formato de columnas unidas con UNION ALL; cada una trae sus propias N mas nuevas.
            // Los LEFT JOIN a las lineas hacen que un registro sin lineas igual aparezca (con 0 lineas y 0 kg).
            const string sql = @"
                (SELECT 'stock' AS origen, c.tipocompra AS subtipo, c.idcompra AS id, c.creado AS creado,
                        COUNT(cpc.idcorte) AS lineas, COALESCE(SUM(ABS(cpc.cantkg)), 0) AS kg, '' AS detalle
                 FROM compras c
                 LEFT JOIN corteporcompra cpc ON cpc.idcompra = c.idcompra
                 WHERE c.creadopor = @idOperador
                   AND c.creado >= @desde
                   AND c.tipocompra IN ('Ingreso Stock', 'Egreso Stock', 'Pesaje Cortes', 'Ajuste Stock', 'Cierre Stock')
                   AND COALESCE(c.estado, '') <> 'Anulado'
                 GROUP BY c.idcompra, c.tipocompra, c.creado
                 ORDER BY c.creado DESC
                 LIMIT @limite)

                UNION ALL

                (SELECT 'movimiento' AS origen, '' AS subtipo, m.idmovimiento AS id, m.creado AS creado,
                        COUNT(cpm.idcorte) AS lineas, COALESCE(SUM(ABS(cpm.cantkg)), 0) AS kg, sd.sucursal AS detalle
                 FROM movimiento m
                 INNER JOIN sucursal sd ON sd.idsucursal = m.sucursaldestino
                 LEFT JOIN cortepormovimiento cpm ON cpm.idmovimientos = m.idmovimiento
                 WHERE m.creadopor = @idOperador
                   AND m.creado >= @desde
                 GROUP BY m.idmovimiento, m.creado, sd.sucursal
                 ORDER BY m.creado DESC
                 LIMIT @limite)

                UNION ALL

                (SELECT 'elaborado' AS origen,
                        (CASE WHEN COALESCE(e.observaciones, '') = 'Desarme' THEN 'desarme'
                              WHEN COALESCE(ce.ingresorapidoembutido, false) THEN 'rapido'
                              ELSE 'elaborado' END) AS subtipo,
                        e.idembutido AS id, e.creado AS creado,
                        COUNT(cpe.idcorte) AS lineas, COALESCE(SUM(ABS(cpe.kgutilizados)), 0) AS kg, ce.corte AS detalle
                 FROM embutidos e
                 INNER JOIN corte ce ON ce.idcorte = e.idcorte
                 LEFT JOIN corteporembutido cpe ON cpe.idembutido = e.idembutido
                 WHERE e.creadopor = @idOperador
                   AND e.creado >= @desde
                   AND COALESCE(e.estado, '') <> 'Anulado'
                 GROUP BY e.idembutido, e.creado, ce.corte, ce.ingresorapidoembutido, e.observaciones
                 ORDER BY e.creado DESC
                 LIMIT @limite);";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idOperador", idOperador);
                p.AddWithValue("desde", desde);
                p.AddWithValue("limite", limitePorTipo);
            });
        }
    }
}
