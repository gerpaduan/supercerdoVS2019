using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    // Acceso a dbo.CortePuntoStockSucursal: punto de stock propio por combinacion
    // Producto (Corte) x Sucursal. Reemplaza la lectura del campo unico Corte.puntoStock
    // para el calculo de faltantes por sucursal.
    public class CortePuntoStockSucursal : Contratos.ICortePuntoStockSucursalRepository
    {
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;

        public CortePuntoStockSucursal(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
            _param = param;
        }

        // Alta de producto: crea un registro por cada sucursal de la empresa con el mismo
        // valor inicial (el que el usuario cargo como "Punto Stock" en el formulario de alta).
        // Idempotente (WHERE NOT EXISTS): tambien se reutiliza para "asegurar que existan los
        // registros" cuando un producto pasa a EnCierreStock=true en edicion (ver
        // ProductosController.Guardar) — no duplica filas si ya existen para alguna sucursal.
        public void CrearParaTodasLasSucursales(int idEmpresa, int idCorte, int puntoStockInicial)
        {
            const string sql = @"
                INSERT INTO dbo.CortePuntoStockSucursal (idEmpresa, idCorte, idSucursal, puntoStock)
                SELECT @idEmpresa, @idCorte, s.idSucursal, @puntoStockInicial
                FROM dbo.Sucursal s
                WHERE s.idEmpresa = @idEmpresa
                  AND NOT EXISTS (
                      SELECT 1 FROM dbo.CortePuntoStockSucursal existente
                      WHERE existente.idEmpresa = @idEmpresa
                        AND existente.idCorte = @idCorte
                        AND existente.idSucursal = s.idSucursal
                  );";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                    p.Add("@idCorte", SqlDbType.Int).Value = idCorte;
                    p.Add("@puntoStockInicial", SqlDbType.Int).Value = puntoStockInicial;
                }
            );
        }

        // Guardado en lote: todas las sucursales de un producto se actualizan dentro de la
        // misma transaccion. Si una sola falla, se hace rollback de todas — nunca queda un
        // producto con algunas sucursales guardadas y otras no.
        public void GuardarPuntosStockLote(int idEmpresa, int idCorte, List<(int idSucursal, int puntoStock)> valores)
        {
            const string sqlMerge = @"
                MERGE dbo.CortePuntoStockSucursal AS target
                USING (SELECT @idEmpresa AS idEmpresa, @idCorte AS idCorte, @idSucursal AS idSucursal) AS source
                    ON target.idEmpresa = source.idEmpresa
                   AND target.idCorte = source.idCorte
                   AND target.idSucursal = source.idSucursal
                WHEN MATCHED THEN
                    UPDATE SET puntoStock = @puntoStock, actualizado = GETDATE()
                WHEN NOT MATCHED THEN
                    INSERT (idEmpresa, idCorte, idSucursal, puntoStock)
                    VALUES (@idEmpresa, @idCorte, @idSucursal, @puntoStock);";

            using (var con = Db.Open(_empresa))
            using (var tx = con.BeginTransaction())
            {
                try
                {
                    foreach (var valor in valores)
                    {
                        using (var cmd = new SqlCommand(sqlMerge, con, tx))
                        {
                            cmd.CommandTimeout = Conexion.timeOut;
                            cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                            cmd.Parameters.Add("@idCorte", SqlDbType.Int).Value = idCorte;
                            cmd.Parameters.Add("@idSucursal", SqlDbType.Int).Value = valor.idSucursal;
                            cmd.Parameters.Add("@puntoStock", SqlDbType.Int).Value = valor.puntoStock;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        // Punto de stock de todos los productos para UNA sucursal puntual (idCorte -> puntoStock).
        // Usado para corregir en C# el faltante que calcula Negocio.Corte.CierreStock sin tener
        // que tocar el SP a_CierreStock.
        public Dictionary<int, int> FindPorSucursal(int idSucursal)
        {
            const string sql = "SELECT idCorte, puntoStock FROM dbo.CortePuntoStockSucursal WHERE idSucursal = @idSucursal";

            var resultado = new Dictionary<int, int>();

            using (var con = Db.Open(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandTimeout = Conexion.timeOut;
                cmd.Parameters.Add("@idSucursal", SqlDbType.Int).Value = idSucursal;

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        resultado[Convert.ToInt32(dr["idCorte"])] = Convert.ToInt32(dr["puntoStock"]);
                    }
                }
            }

            return resultado;
        }
    }
}
