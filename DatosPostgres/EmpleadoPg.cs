using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IEmpleadoRepository. Modulo Empleados y Liquidacion de
    // Sueldos (2026-09-29), solo Postgres -- ver docs/DECISIONS.md.
    public class EmpleadoPg : Contratos.IEmpleadoRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public EmpleadoPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        // Trae solo lo necesario de Persona/Usuario para listar y para resolver la ficha (nombre,
        // login, activo) -- si hace falta el detalle completo de Persona/Usuario para editar, el
        // caller (Negocio.Empleado) los resuelve aparte con PersonaPg/UsuarioPg.
        private const string SelectBase = @"
                SELECT e.idempleado, e.idempresa, e.idpersona, e.idusuario, e.formaliquidacion,
                       e.fechaingreso, e.fechabaja, e.activo, e.creado, e.creadopor, e.actualizado, e.actualizadopor,
                       p.razonsocial AS personarazonsocial, p.identificacion AS personaidentificacion,
                       u.nombre AS usuarionombre, u.usuario AS usuariologin, u.activo AS usuarioactivo
                FROM empleado e
                JOIN personas p ON p.idpersona = e.idpersona
                JOIN usuarios u ON u.id = e.idusuario ";

        private static Entidades.Empleado Mapear(System.Data.IDataRecord dr)
        {
            return new Entidades.Empleado
            {
                Id = Convert.ToInt32(dr["idempleado"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                Persona = new Entidades.Persona
                {
                    idPersona = Convert.ToInt32(dr["idpersona"]),
                    razonSocial = Convert.ToString(dr["personarazonsocial"]),
                    Identificacion = dr["personaidentificacion"] == DBNull.Value ? "" : Convert.ToString(dr["personaidentificacion"])
                },
                Usuario = new Entidades.Usuario
                {
                    Id = Convert.ToInt32(dr["idusuario"]),
                    Nombre = Convert.ToString(dr["usuarionombre"]),
                    User = Convert.ToString(dr["usuariologin"]),
                    Activo = dr["usuarioactivo"] != DBNull.Value && Convert.ToBoolean(dr["usuarioactivo"])
                },
                FormaLiquidacion = (Entidades.Empleado.formaLiquidacion)Enum.Parse(typeof(Entidades.Empleado.formaLiquidacion), Convert.ToString(dr["formaliquidacion"])),
                FechaIngreso = Convert.ToDateTime(dr["fechaingreso"]),
                FechaBaja = dr["fechabaja"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["fechabaja"]),
                Activo = Convert.ToBoolean(dr["activo"]),
                Creado = dr["creado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["creado"]),
                CreadoPor = dr["creadopor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["creadopor"]),
                Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"]),
                ActualizadoPor = dr["actualizadopor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["actualizadopor"])
            };
        }

        public List<Entidades.Empleado> Listar(int idEmpresa, string texto, Entidades.Empleado.formaLiquidacion? forma, bool? soloActivos)
        {
            var where = new List<string> { "e.idempresa = @idEmpresa" };
            if (!string.IsNullOrWhiteSpace(texto)) where.Add("(p.razonsocial ILIKE @texto OR p.identificacion ILIKE @texto OR p.cuit ILIKE @texto)");
            if (forma.HasValue) where.Add("e.formaliquidacion = @forma");
            if (soloActivos.HasValue) where.Add("e.activo = @activo");

            return DbPg.Reader(_connectionString, _idEmpresa,
                SelectBase + "WHERE " + string.Join(" AND ", where) + " ORDER BY p.razonsocial;",
                Mapear,
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    if (!string.IsNullOrWhiteSpace(texto)) p.AddWithValue("texto", "%" + texto.Trim() + "%");
                    if (forma.HasValue) p.AddWithValue("forma", forma.Value.ToString());
                    if (soloActivos.HasValue) p.AddWithValue("activo", soloActivos.Value);
                });
        }

        public Entidades.Empleado ObtenerPorId(int id, int idEmpresa)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                SelectBase + "WHERE e.idempleado = @id AND e.idempresa = @idEmpresa;",
                Mapear,
                p => { p.AddWithValue("id", id); p.AddWithValue("idEmpresa", idEmpresa); });
            return lista.Count > 0 ? lista[0] : null;
        }

        public Entidades.Empleado ObtenerPorIdUsuario(int idUsuario, int idEmpresa)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                SelectBase + "WHERE e.idusuario = @idUsuario AND e.idempresa = @idEmpresa;",
                Mapear,
                p => { p.AddWithValue("idUsuario", idUsuario); p.AddWithValue("idEmpresa", idEmpresa); });
            return lista.Count > 0 ? lista[0] : null;
        }

        public int Agregar(Entidades.Empleado empleado)
        {
            const string sql = @"
                INSERT INTO empleado (idempresa, idpersona, idusuario, formaliquidacion, fechaingreso, activo, creado, creadopor)
                VALUES (@idEmpresa, @idPersona, @idUsuario, @forma, @fechaIngreso, @activo, now(), @creadoPor)
                RETURNING idempleado;";

            object nuevoId = DbPg.Scalar(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idEmpresa", empleado.IdEmpresa);
                p.AddWithValue("idPersona", empleado.Persona.idPersona);
                p.AddWithValue("idUsuario", empleado.Usuario.Id);
                p.AddWithValue("forma", empleado.FormaLiquidacion.ToString());
                p.AddWithValue("fechaIngreso", empleado.FechaIngreso);
                p.AddWithValue("activo", empleado.Activo);
                p.AddWithValue("creadoPor", (object)empleado.CreadoPor ?? DBNull.Value);
            });

            empleado.Id = Convert.ToInt32(nuevoId);
            return empleado.Id;
        }

        public void Editar(Entidades.Empleado empleado)
        {
            const string sql = @"
                UPDATE empleado SET
                    formaliquidacion = @forma, fechaingreso = @fechaIngreso,
                    fechabaja = @fechaBaja, activo = @activo, actualizado = now(), actualizadopor = @actualizadoPor
                WHERE idempleado = @id AND idempresa = @idEmpresa;";

            DbPg.NonQuery(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("id", empleado.Id);
                p.AddWithValue("idEmpresa", empleado.IdEmpresa);
                p.AddWithValue("forma", empleado.FormaLiquidacion.ToString());
                p.AddWithValue("fechaIngreso", empleado.FechaIngreso);
                p.AddWithValue("fechaBaja", (object)empleado.FechaBaja ?? DBNull.Value);
                p.AddWithValue("activo", empleado.Activo);
                p.AddWithValue("actualizadoPor", (object)empleado.ActualizadoPor ?? DBNull.Value);
            });
        }

        public void SetActivo(int idEmpleado, int idEmpresa, bool activo)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                @"UPDATE empleado SET activo = @activo, fechabaja = @fechaBaja, actualizado = now()
                  WHERE idempleado = @id AND idempresa = @idEmpresa;",
                p =>
                {
                    p.AddWithValue("id", idEmpleado);
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("activo", activo);
                    p.AddWithValue("fechaBaja", activo ? (object)DBNull.Value : DateTime.Today);
                });
        }

        public bool ExistePersonaVinculada(int idPersona, int idEmpresa, int idExcluir)
        {
            object resultado = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT 1 FROM empleado WHERE idempresa = @idEmpresa AND idpersona = @idPersona AND idempleado <> @idExcluir LIMIT 1;",
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("idPersona", idPersona);
                    p.AddWithValue("idExcluir", idExcluir);
                });
            return resultado != null;
        }

        public bool ExisteUsuarioVinculado(int idUsuario, int idEmpresa, int idExcluir)
        {
            object resultado = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT 1 FROM empleado WHERE idempresa = @idEmpresa AND idusuario = @idUsuario AND idempleado <> @idExcluir LIMIT 1;",
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("idUsuario", idUsuario);
                    p.AddWithValue("idExcluir", idExcluir);
                });
            return resultado != null;
        }

        private static Entidades.EmpleadoTarifa MapearTarifa(System.Data.IDataRecord dr)
        {
            return new Entidades.EmpleadoTarifa
            {
                Id = Convert.ToInt32(dr["idempleadotarifa"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                IdEmpleado = Convert.ToInt32(dr["idempleado"]),
                Turno = dr["turno"] == DBNull.Value ? (Entidades.Turno?)null : (Entidades.Turno)Enum.Parse(typeof(Entidades.Turno), Convert.ToString(dr["turno"])),
                DiaSemana = dr["diasemana"] == DBNull.Value ? (Entidades.DiaSemana?)null : (Entidades.DiaSemana)Enum.Parse(typeof(Entidades.DiaSemana), Convert.ToString(dr["diasemana"])),
                Valor = Convert.ToDecimal(dr["valor"]),
                VigenteDesde = Convert.ToDateTime(dr["vigentedesde"]),
                Creado = dr["creado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["creado"]),
                CreadoPor = dr["creadopor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["creadopor"])
            };
        }

        // Todo el historial (append-only) -- usado tanto para resolver la vigente al calcular una
        // liquidacion como para Empleados/Historial.cshtml.
        public List<Entidades.EmpleadoTarifa> ListarTarifas(int idEmpleado)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                @"SELECT idempleadotarifa, idempresa, idempleado, turno, diasemana, valor, vigentedesde, creado, creadopor
                  FROM empleadotarifa WHERE idempleado = @idEmpleado ORDER BY vigentedesde DESC;",
                MapearTarifa,
                p => p.AddWithValue("idEmpleado", idEmpleado));
        }

        // Nunca UPDATE/DELETE: siempre INSERT de una fila nueva (regla de historial append-only).
        public void AgregarTarifa(Entidades.EmpleadoTarifa tarifa)
        {
            const string sql = @"
                INSERT INTO empleadotarifa (idempresa, idempleado, turno, diasemana, valor, vigentedesde, creado, creadopor)
                VALUES (@idEmpresa, @idEmpleado, @turno, @diaSemana, @valor, @vigenteDesde, now(), @creadoPor);";

            DbPg.NonQuery(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idEmpresa", tarifa.IdEmpresa);
                p.AddWithValue("idEmpleado", tarifa.IdEmpleado);
                p.AddWithValue("turno", tarifa.Turno.HasValue ? (object)tarifa.Turno.Value.ToString() : DBNull.Value);
                p.AddWithValue("diaSemana", tarifa.DiaSemana.HasValue ? (object)tarifa.DiaSemana.Value.ToString() : DBNull.Value);
                p.AddWithValue("valor", tarifa.Valor);
                p.AddWithValue("vigenteDesde", tarifa.VigenteDesde);
                p.AddWithValue("creadoPor", (object)tarifa.CreadoPor ?? DBNull.Value);
            });
        }

        private static Entidades.EmpleadoVacacion MapearVacacion(System.Data.IDataRecord dr)
        {
            return new Entidades.EmpleadoVacacion
            {
                Id = Convert.ToInt32(dr["idempleadovacacion"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                IdEmpleado = Convert.ToInt32(dr["idempleado"]),
                FechaDesde = Convert.ToDateTime(dr["fechadesde"]),
                FechaHasta = Convert.ToDateTime(dr["fechahasta"]),
                Observaciones = dr["observaciones"] == DBNull.Value ? "" : Convert.ToString(dr["observaciones"]),
                Creado = dr["creado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["creado"]),
                CreadoPor = dr["creadopor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["creadopor"])
            };
        }

        public List<Entidades.EmpleadoVacacion> ListarVacaciones(int idEmpleado)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                @"SELECT idempleadovacacion, idempresa, idempleado, fechadesde, fechahasta, observaciones, creado, creadopor
                  FROM empleadovacacion WHERE idempleado = @idEmpleado ORDER BY fechadesde DESC;",
                MapearVacacion,
                p => p.AddWithValue("idEmpleado", idEmpleado));
        }

        public void AgregarVacacion(Entidades.EmpleadoVacacion vacacion)
        {
            const string sql = @"
                INSERT INTO empleadovacacion (idempresa, idempleado, fechadesde, fechahasta, observaciones, creado, creadopor)
                VALUES (@idEmpresa, @idEmpleado, @fechaDesde, @fechaHasta, @observaciones, now(), @creadoPor);";

            DbPg.NonQuery(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idEmpresa", vacacion.IdEmpresa);
                p.AddWithValue("idEmpleado", vacacion.IdEmpleado);
                p.AddWithValue("fechaDesde", vacacion.FechaDesde);
                p.AddWithValue("fechaHasta", vacacion.FechaHasta);
                p.AddWithValue("observaciones", (object)vacacion.Observaciones ?? DBNull.Value);
                p.AddWithValue("creadoPor", (object)vacacion.CreadoPor ?? DBNull.Value);
            });
        }

        public Entidades.EmpleadoVacacion ObtenerVacacionVigente(int idEmpleado, DateTime fecha)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                @"SELECT idempleadovacacion, idempresa, idempleado, fechadesde, fechahasta, observaciones, creado, creadopor
                  FROM empleadovacacion
                  WHERE idempleado = @idEmpleado AND @fecha::date BETWEEN fechadesde AND fechahasta
                  ORDER BY fechadesde DESC LIMIT 1;",
                MapearVacacion,
                p => { p.AddWithValue("idEmpleado", idEmpleado); p.AddWithValue("fecha", fecha.Date); });
            return lista.Count > 0 ? lista[0] : null;
        }
    }
}
