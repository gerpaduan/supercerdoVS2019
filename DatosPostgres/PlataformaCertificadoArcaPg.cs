// Certificado de la plataforma para el padron compartido: CUIT dueno del alias, nombre del .pfx y su clave
// YA CIFRADA (Data Protection, lo hace WebCore.Services.PadronPlataformaCredencial). La tabla esta cerrada por RLS
// para las sesiones de empresa: se accede con SET LOCAL ROLE carnisys_sysadmin_bypass (mismo mecanismo que
// SystemAdministrationPg). Una sola fila (id = 1).
using System;
using Npgsql;

namespace DatosPostgres
{
    public class PlataformaCertificadoArcaRegistro
    {
        public long Cuit { get; set; }
        public string NombreArchivo { get; set; }
        public string ClaveProtegida { get; set; }
        public DateTime ActualizadoEn { get; set; }
    }

    public class PlataformaCertificadoArcaPg
    {
        private readonly string _connectionString;

        public PlataformaCertificadoArcaPg(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
        }

        // El registro configurado, o null si la plataforma todavia no tiene certificado de padron.
        public PlataformaCertificadoArcaRegistro Obtener()
        {
            return EjecutarComoAdmin((cn, tx) =>
            {
                using (var cmd = new NpgsqlCommand(
                    "SELECT cuit, nombre_archivo, clave_protegida, actualizado_en FROM plataforma_certificado_arca WHERE id = 1;", cn, tx))
                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return null;
                    return new PlataformaCertificadoArcaRegistro
                    {
                        Cuit = Convert.ToInt64(dr["cuit"]),
                        NombreArchivo = Convert.ToString(dr["nombre_archivo"]),
                        ClaveProtegida = Convert.ToString(dr["clave_protegida"]),
                        ActualizadoEn = Convert.ToDateTime(dr["actualizado_en"])
                    };
                }
            });
        }

        // Inserta o reemplaza el registro (idempotente).
        public void Guardar(long cuit, string nombreArchivo, string claveProtegida, int idUsuario)
        {
            if (cuit <= 0) throw new ArgumentException("CUIT inválido.", nameof(cuit));
            if (string.IsNullOrWhiteSpace(nombreArchivo)) throw new ArgumentException("Falta el nombre del archivo.", nameof(nombreArchivo));
            if (string.IsNullOrEmpty(claveProtegida)) throw new ArgumentException("Falta la clave protegida.", nameof(claveProtegida));

            EjecutarComoAdmin<object>((cn, tx) =>
            {
                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO plataforma_certificado_arca (id, cuit, nombre_archivo, clave_protegida, actualizado_en, actualizado_por)
                    VALUES (1, @cuit, @nombre, @clave, now(), @idUsuario)
                    ON CONFLICT (id) DO UPDATE
                        SET cuit = EXCLUDED.cuit,
                            nombre_archivo = EXCLUDED.nombre_archivo,
                            clave_protegida = EXCLUDED.clave_protegida,
                            actualizado_en = now(),
                            actualizado_por = EXCLUDED.actualizado_por;", cn, tx))
                {
                    cmd.Parameters.AddWithValue("cuit", cuit);
                    cmd.Parameters.AddWithValue("nombre", nombreArchivo);
                    cmd.Parameters.AddWithValue("clave", claveProtegida);
                    cmd.Parameters.AddWithValue("idUsuario", idUsuario);
                    cmd.ExecuteNonQuery();
                }
                return null;
            });
        }

        // Abre una transaccion con el rol de administracion de plataforma (BYPASSRLS) y la confirma al terminar.
        private T EjecutarComoAdmin<T>(Func<NpgsqlConnection, NpgsqlTransaction, T> accion)
        {
            using (var cn = new NpgsqlConnection(_connectionString))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        using (var setRole = new NpgsqlCommand("SET LOCAL ROLE carnisys_sysadmin_bypass;", cn, tx))
                            setRole.ExecuteNonQuery();
                        T resultado = accion(cn, tx);
                        tx.Commit();
                        return resultado;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
