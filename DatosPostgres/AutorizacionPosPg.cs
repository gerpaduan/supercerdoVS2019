using System;
using System.Data;

namespace DatosPostgres
{
    // Auditoria de autorizaciones provisorias del POS (tabla auditoriaautorizacionespos, ver la migracion
    // 20261103-Create_auditoriaautorizacionespos.sql y docs/DECISIONS.md). Solo INSERT y el cierre (fechafin/motivofin).
    public class AutorizacionPosPg : Contratos.IAutorizacionPosRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public AutorizacionPosPg(string connectionString, int idEmpresa)
        {
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        public int Registrar(int idOperador, int idAutorizador, int idUsuarioSesion, string posInstanceId, string accion, int idRegistro)
        {
            object id = DbPg.Scalar(_connectionString, _idEmpresa, @"
                INSERT INTO auditoriaautorizacionespos
                    (idempresa, posinstanceid, accion, idregistro, idoperador, idautorizador, idusuariosesion, fechaautorizacion)
                VALUES (@idEmpresa, @posInstanceId, @accion, @idRegistro, @idOperador, @idAutorizador, @idUsuarioSesion, now())
                RETURNING id;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("posInstanceId", posInstanceId ?? "");
                    p.AddWithValue("accion", accion);
                    p.AddWithValue("idRegistro", idRegistro);
                    p.AddWithValue("idOperador", idOperador);
                    p.AddWithValue("idAutorizador", idAutorizador);
                    p.AddWithValue("idUsuarioSesion", idUsuarioSesion);
                });

            return Convert.ToInt32(id);
        }

        public void Cerrar(int idAuditoria, string motivoFin)
        {
            if (idAuditoria <= 0) return;

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE auditoriaautorizacionespos
                   SET fechafin = now(), motivofin = @motivoFin
                 WHERE id = @id AND fechafin IS NULL;",
                p =>
                {
                    p.AddWithValue("id", idAuditoria);
                    p.AddWithValue("motivoFin", motivoFin ?? "");
                });
        }

        public DataTable ObtenerAutorizaciones(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT a.id, a.fechaautorizacion AS fecha, a.accion, a.idregistro,
                       uo.nombre AS operador, ua.nombre AS autorizador, us.nombre AS cuenta,
                       a.fechafin, a.motivofin
                FROM auditoriaautorizacionespos a
                LEFT JOIN usuarios uo ON uo.id = a.idoperador
                LEFT JOIN usuarios ua ON ua.id = a.idautorizador
                LEFT JOIN usuarios us ON us.id = a.idusuariosesion
                WHERE a.fechaautorizacion BETWEEN @desde AND @hasta
                ORDER BY a.fechaautorizacion DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("desde", desde);
                p.AddWithValue("hasta", hasta);
            });
        }
    }
}
