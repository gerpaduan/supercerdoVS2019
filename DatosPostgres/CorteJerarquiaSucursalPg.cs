using System;
using System.Collections.Generic;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.ICorteJerarquiaSucursalRepository (3/3 metodos).
    // Mismo patron que CortePuntoStockSucursalPg.cs. cortejerarquiasucursal tiene RLS (mejora
    // deliberada, igual criterio que cortepuntostocksucursal -- el original en SQL Server no
    // la tiene). Ver docs/DECISIONS.md (2026-09-17).
    public class CorteJerarquiaSucursalPg : Contratos.ICorteJerarquiaSucursalRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public CorteJerarquiaSucursalPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        public void GuardarExcepcion(int idEmpresa, int idCorte, int idSucursal, bool independiente, bool enCierreStock)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO cortejerarquiasucursal (idempresa, idcorte, idsucursal, independiente, encierrestock)
                VALUES (@idEmpresa, @idCorte, @idSucursal, @independiente, @enCierreStock)
                ON CONFLICT (idempresa, idcorte, idsucursal) DO UPDATE SET
                    independiente = EXCLUDED.independiente,
                    encierrestock = EXCLUDED.encierrestock,
                    actualizado = now();",
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("idCorte", idCorte);
                    p.AddWithValue("idSucursal", idSucursal);
                    p.AddWithValue("independiente", independiente);
                    p.AddWithValue("enCierreStock", enCierreStock);
                });
        }

        public void QuitarExcepcion(int idEmpresa, int idCorte, int idSucursal)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                DELETE FROM cortejerarquiasucursal
                WHERE idempresa = @idEmpresa AND idcorte = @idCorte AND idsucursal = @idSucursal;",
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("idCorte", idCorte);
                    p.AddWithValue("idSucursal", idSucursal);
                });
        }

        public Dictionary<int, (bool independiente, bool enCierreStock)> ListarExcepcionesPorCorte(int idEmpresa, int idCorte)
        {
            var resultado = new Dictionary<int, (bool independiente, bool enCierreStock)>();

            DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT idsucursal, independiente, encierrestock FROM cortejerarquiasucursal WHERE idempresa = @idEmpresa AND idcorte = @idCorte;",
                dr =>
                {
                    int idSucursal = Convert.ToInt32(dr["idsucursal"]);
                    bool independiente = Convert.ToBoolean(dr["independiente"]);
                    bool enCierreStock = Convert.ToBoolean(dr["encierrestock"]);
                    resultado[idSucursal] = (independiente, enCierreStock);
                    return (object)null;
                },
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("idCorte", idCorte);
                });

            return resultado;
        }
    }
}
