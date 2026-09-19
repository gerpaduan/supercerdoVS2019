using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    // Acceso a dbo.CorteJerarquiaSucursal: excepcion de independiente/enCierreStock por
    // combinacion Producto (Corte) x Sucursal. Sin fila para una sucursal => se sigue usando
    // el valor global de dbo.Corte. Mismo patron que Datos/CortePuntoStockSucursal.cs.
    public class CorteJerarquiaSucursal : Contratos.ICorteJerarquiaSucursalRepository
    {
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;

        public CorteJerarquiaSucursal(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
            _param = param;
        }

        // Upsert de la excepcion: guarda los 2 valores actuales de los switches para esa
        // sucursal puntual.
        public void GuardarExcepcion(int idEmpresa, int idCorte, int idSucursal, bool independiente, bool enCierreStock)
        {
            const string sqlMerge = @"
                MERGE dbo.CorteJerarquiaSucursal AS target
                USING (SELECT @idEmpresa AS idEmpresa, @idCorte AS idCorte, @idSucursal AS idSucursal) AS source
                    ON target.idEmpresa = source.idEmpresa
                   AND target.idCorte = source.idCorte
                   AND target.idSucursal = source.idSucursal
                WHEN MATCHED THEN
                    UPDATE SET independiente = @independiente, enCierreStock = @enCierreStock, actualizado = GETDATE()
                WHEN NOT MATCHED THEN
                    INSERT (idEmpresa, idCorte, idSucursal, independiente, enCierreStock)
                    VALUES (@idEmpresa, @idCorte, @idSucursal, @independiente, @enCierreStock);";

            Db.NonQuery(
                _empresa,
                sqlMerge,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                    p.Add("@idCorte", SqlDbType.Int).Value = idCorte;
                    p.Add("@idSucursal", SqlDbType.Int).Value = idSucursal;
                    p.Add("@independiente", SqlDbType.Bit).Value = independiente;
                    p.Add("@enCierreStock", SqlDbType.Bit).Value = enCierreStock;
                }
            );
        }

        // Quitar la excepcion: la sucursal vuelve a usar el valor global de dbo.Corte.
        public void QuitarExcepcion(int idEmpresa, int idCorte, int idSucursal)
        {
            const string sql = @"
                DELETE FROM dbo.CorteJerarquiaSucursal
                WHERE idEmpresa = @idEmpresa AND idCorte = @idCorte AND idSucursal = @idSucursal;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                    p.Add("@idCorte", SqlDbType.Int).Value = idCorte;
                    p.Add("@idSucursal", SqlDbType.Int).Value = idSucursal;
                }
            );
        }

        // Excepciones existentes de UN corte (para pintar la seccion "Jerarquia por sucursal"
        // de AddOrEdit.cshtml). Sucursales que no aparecen en el diccionario usan el valor
        // global.
        public Dictionary<int, (bool independiente, bool enCierreStock)> ListarExcepcionesPorCorte(int idEmpresa, int idCorte)
        {
            const string sql = @"
                SELECT idSucursal, independiente, enCierreStock
                FROM dbo.CorteJerarquiaSucursal
                WHERE idEmpresa = @idEmpresa AND idCorte = @idCorte;";

            var resultado = new Dictionary<int, (bool independiente, bool enCierreStock)>();

            using (var con = Db.Open(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandTimeout = Conexion.timeOut;
                cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                cmd.Parameters.Add("@idCorte", SqlDbType.Int).Value = idCorte;

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        int idSucursal = Convert.ToInt32(dr["idSucursal"]);
                        bool independiente = Convert.ToBoolean(dr["independiente"]);
                        bool enCierreStock = Convert.ToBoolean(dr["enCierreStock"]);
                        resultado[idSucursal] = (independiente, enCierreStock);
                    }
                }
            }

            return resultado;
        }
    }
}
