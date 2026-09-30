using System;
using System.Collections.Generic;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IRegistroJornadaRepository.
    public class RegistroJornadaPg : Contratos.IRegistroJornadaRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public RegistroJornadaPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        private const string SelectBase = @"
                SELECT idregistrojornada, idempresa, idempleado, fecha, turno, horaentrada, horasalida,
                       cantidad, registradopor, iddispositivoseguro, motivocorreccion, creado, actualizado, actualizadopor
                FROM registrojornada ";

        private static Entidades.RegistroJornada Mapear(System.Data.IDataRecord dr)
        {
            return new Entidades.RegistroJornada
            {
                Id = Convert.ToInt32(dr["idregistrojornada"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                IdEmpleado = Convert.ToInt32(dr["idempleado"]),
                Fecha = Convert.ToDateTime(dr["fecha"]),
                Turno = dr["turno"] == DBNull.Value ? (Entidades.Turno?)null : (Entidades.Turno)Enum.Parse(typeof(Entidades.Turno), Convert.ToString(dr["turno"])),
                HoraEntrada = dr["horaentrada"] == DBNull.Value ? (TimeSpan?)null : (TimeSpan)dr["horaentrada"],
                HoraSalida = dr["horasalida"] == DBNull.Value ? (TimeSpan?)null : (TimeSpan)dr["horasalida"],
                Cantidad = dr["cantidad"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["cantidad"]),
                RegistradoPor = Convert.ToInt32(dr["registradopor"]),
                IdDispositivoSeguro = dr["iddispositivoseguro"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["iddispositivoseguro"]),
                MotivoCorreccion = dr["motivocorreccion"] == DBNull.Value ? "" : Convert.ToString(dr["motivocorreccion"]),
                Creado = dr["creado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["creado"]),
                Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"]),
                ActualizadoPor = dr["actualizadopor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["actualizadopor"])
            };
        }

        public List<Entidades.RegistroJornada> ListarPorEmpleadoYRango(int idEmpleado, DateTime desde, DateTime hasta)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                SelectBase + "WHERE idempleado = @idEmpleado AND fecha BETWEEN @desde AND @hasta ORDER BY fecha, horaentrada;",
                Mapear,
                p => { p.AddWithValue("idEmpleado", idEmpleado); p.AddWithValue("desde", desde.Date); p.AddWithValue("hasta", hasta.Date); });
        }

        public Entidades.RegistroJornada ObtenerPorId(int id, int idEmpresa)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                SelectBase + "WHERE idregistrojornada = @id AND idempresa = @idEmpresa;",
                Mapear,
                p => { p.AddWithValue("id", id); p.AddWithValue("idEmpresa", idEmpresa); });
            return lista.Count > 0 ? lista[0] : null;
        }

        public Entidades.RegistroJornada ObtenerAbiertoHoy(int idEmpleado, DateTime fecha)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                SelectBase + @"WHERE idempleado = @idEmpleado AND fecha = @fecha
                               AND horaentrada IS NOT NULL AND horasalida IS NULL
                               ORDER BY horaentrada DESC LIMIT 1;",
                Mapear,
                p => { p.AddWithValue("idEmpleado", idEmpleado); p.AddWithValue("fecha", fecha.Date); });
            return lista.Count > 0 ? lista[0] : null;
        }

        public int Agregar(Entidades.RegistroJornada registro)
        {
            const string sql = @"
                INSERT INTO registrojornada
                    (idempresa, idempleado, fecha, turno, horaentrada, horasalida, cantidad, registradopor,
                     iddispositivoseguro, motivocorreccion, creado)
                VALUES
                    (@idEmpresa, @idEmpleado, @fecha, @turno, @horaEntrada, @horaSalida, @cantidad, @registradoPor,
                     @idDispositivoSeguro, @motivoCorreccion, now())
                RETURNING idregistrojornada;";

            object nuevoId = DbPg.Scalar(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idEmpresa", registro.IdEmpresa);
                p.AddWithValue("idEmpleado", registro.IdEmpleado);
                p.AddWithValue("fecha", registro.Fecha.Date);
                p.AddWithValue("turno", registro.Turno.HasValue ? (object)registro.Turno.Value.ToString() : DBNull.Value);
                p.AddWithValue("horaEntrada", (object)registro.HoraEntrada ?? DBNull.Value);
                p.AddWithValue("horaSalida", (object)registro.HoraSalida ?? DBNull.Value);
                p.AddWithValue("cantidad", (object)registro.Cantidad ?? DBNull.Value);
                p.AddWithValue("registradoPor", registro.RegistradoPor);
                p.AddWithValue("idDispositivoSeguro", (object)registro.IdDispositivoSeguro ?? DBNull.Value);
                p.AddWithValue("motivoCorreccion", string.IsNullOrWhiteSpace(registro.MotivoCorreccion) ? (object)DBNull.Value : registro.MotivoCorreccion);
            });

            registro.Id = Convert.ToInt32(nuevoId);
            return registro.Id;
        }

        // Completa la salida de un registro abierto -- flujo normal del fichaje, no marca
        // Actualizado (no es una correccion).
        public void CompletarSalida(int id, int idEmpresa, TimeSpan horaSalida)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE registrojornada SET horasalida = @horaSalida WHERE idregistrojornada = @id AND idempresa = @idEmpresa;",
                p =>
                {
                    p.AddWithValue("id", id);
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("horaSalida", horaSalida);
                });
        }

        // Corrige un registro ya cargado: siempre marca Actualizado/ActualizadoPor (la carga
        // inicial pasa por Agregar, nunca por aca) -- ver Entidades/RegistroJornada.FueCorregido.
        public void Editar(Entidades.RegistroJornada registro)
        {
            const string sql = @"
                UPDATE registrojornada SET
                    horaentrada = @horaEntrada, horasalida = @horaSalida, cantidad = @cantidad,
                    motivocorreccion = @motivoCorreccion, actualizado = now(), actualizadopor = @actualizadoPor
                WHERE idregistrojornada = @id AND idempresa = @idEmpresa;";

            DbPg.NonQuery(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("id", registro.Id);
                p.AddWithValue("idEmpresa", registro.IdEmpresa);
                p.AddWithValue("horaEntrada", (object)registro.HoraEntrada ?? DBNull.Value);
                p.AddWithValue("horaSalida", (object)registro.HoraSalida ?? DBNull.Value);
                p.AddWithValue("cantidad", (object)registro.Cantidad ?? DBNull.Value);
                p.AddWithValue("motivoCorreccion", string.IsNullOrWhiteSpace(registro.MotivoCorreccion) ? (object)DBNull.Value : registro.MotivoCorreccion);
                p.AddWithValue("actualizadoPor", (object)registro.ActualizadoPor ?? DBNull.Value);
            });
        }

        public List<Entidades.RegistroJornada> ListarCorreccionesPendientes(int idEmpresa, int? idEmpleado)
        {
            var where = new List<string> { "idempresa = @idEmpresa", "(actualizado IS NOT NULL OR motivocorreccion IS NOT NULL)" };
            if (idEmpleado.HasValue) where.Add("idempleado = @idEmpleado");

            return DbPg.Reader(_connectionString, _idEmpresa,
                SelectBase + "WHERE " + string.Join(" AND ", where) + " ORDER BY actualizado DESC NULLS LAST, creado DESC;",
                Mapear,
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    if (idEmpleado.HasValue) p.AddWithValue("idEmpleado", idEmpleado.Value);
                });
        }
    }
}
