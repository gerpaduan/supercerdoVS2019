using System;
using System.Collections.Generic;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.ITerminalMercadoPagoRepository. Tabla nueva sin
    // precedente en SQL Server -- no existia ningun concepto de "caja/terminal fisica"
    // persistente en el modelo (ver docs/DECISIONS.md), asi que nace directo en Postgres.
    public class TerminalMercadoPagoPg : Contratos.ITerminalMercadoPagoRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public TerminalMercadoPagoPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        public List<Entidades.TerminalMercadoPago> Listar(int idEmpresa)
        {
            return DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT id, idempresa, idsucursal, terminalidmp, posid, alias, activo, fechaaltautc
                FROM terminales_mercadopago
                WHERE idempresa = @idEmpresa
                ORDER BY idsucursal, alias;",
                MapTerminal,
                p => p.AddWithValue("idEmpresa", idEmpresa));
        }

        // Solo terminales activas (vinculadas y verificadas) -- pensado para Fase 5 (elegir a
        // que terminal mandar un cobro). El listado completo para administracion (incluye las
        // que todavia no se vincularon) es Listar(idEmpresa), filtrado por sucursal del lado
        // del llamador.
        public List<Entidades.TerminalMercadoPago> ListarPorSucursal(int idSucursal)
        {
            return DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT id, idempresa, idsucursal, terminalidmp, posid, alias, activo, fechaaltautc
                FROM terminales_mercadopago
                WHERE idsucursal = @idSucursal AND activo = true
                ORDER BY alias;",
                MapTerminal,
                p => p.AddWithValue("idSucursal", idSucursal));
        }

        public void Agregar(Entidades.TerminalMercadoPago terminal)
        {
            if (terminal == null) throw new ArgumentNullException(nameof(terminal));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO terminales_mercadopago (idempresa, idsucursal, terminalidmp, posid, alias, activo, fechaaltautc)
                VALUES (@idEmpresa, @idSucursal, @terminalIdMp, @posId, @alias, @activo, @fechaAlta);",
                p =>
                {
                    p.AddWithValue("idEmpresa", terminal.IdEmpresa);
                    p.AddWithValue("idSucursal", terminal.IdSucursal);
                    p.AddWithValue("terminalIdMp", (object)terminal.TerminalIdMp ?? DBNull.Value);
                    p.AddWithValue("posId", (object)terminal.PosId ?? DBNull.Value);
                    p.AddWithValue("alias", (object)terminal.Alias ?? DBNull.Value);
                    p.AddWithValue("activo", terminal.Activo);
                    p.AddWithValue("fechaAlta", terminal.FechaAltaUtc);
                });
        }

        public void Actualizar(Entidades.TerminalMercadoPago terminal)
        {
            if (terminal == null) throw new ArgumentNullException(nameof(terminal));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE terminales_mercadopago
                SET alias = @alias, activo = @activo, terminalidmp = @terminalIdMp
                WHERE id = @id AND idempresa = @idEmpresa;",
                p =>
                {
                    p.AddWithValue("id", terminal.Id);
                    p.AddWithValue("idEmpresa", terminal.IdEmpresa);
                    p.AddWithValue("alias", (object)terminal.Alias ?? DBNull.Value);
                    p.AddWithValue("activo", terminal.Activo);
                    p.AddWithValue("terminalIdMp", (object)terminal.TerminalIdMp ?? DBNull.Value);
                });
        }

        public void Eliminar(int id, int idEmpresa)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM terminales_mercadopago WHERE id = @id AND idempresa = @idEmpresa;",
                p =>
                {
                    p.AddWithValue("id", id);
                    p.AddWithValue("idEmpresa", idEmpresa);
                });
        }

        private static Entidades.TerminalMercadoPago MapTerminal(Npgsql.NpgsqlDataReader dr)
        {
            return new Entidades.TerminalMercadoPago
            {
                Id = Convert.ToInt32(dr["id"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                IdSucursal = Convert.ToInt32(dr["idsucursal"]),
                TerminalIdMp = dr["terminalidmp"] == DBNull.Value ? "" : Convert.ToString(dr["terminalidmp"]),
                PosId = dr["posid"] == DBNull.Value ? "" : Convert.ToString(dr["posid"]),
                Alias = dr["alias"] == DBNull.Value ? "" : Convert.ToString(dr["alias"]),
                Activo = dr["activo"] != DBNull.Value && Convert.ToBoolean(dr["activo"]),
                FechaAltaUtc = Convert.ToDateTime(dr["fechaaltautc"])
            };
        }
    }
}
