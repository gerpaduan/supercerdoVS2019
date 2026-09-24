// Clave del .pfx del certificado ARCA de una empresa, guardada YA CIFRADA (Data Protection de ASP.NET,
// lo hace WebCore.Services.AfipConfigProvider). Esta clase solo lee/escribe el texto protegido; nunca ve
// la clave en claro. Una fila por empresa (tabla certificado_arca_clave, con RLS por idempresa).
using System;

namespace DatosPostgres
{
    public class CertificadoArcaClavePg
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public CertificadoArcaClavePg(string connectionString, int idEmpresa)
        {
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        // Texto cifrado de la clave, o null si la empresa no tiene (pfx historico sin clave).
        public string ObtenerProtegida()
        {
            object valor = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT clave_protegida FROM certificado_arca_clave WHERE idempresa = @idEmpresa;",
                p => p.AddWithValue("idEmpresa", _idEmpresa));
            return (valor == null || valor == DBNull.Value) ? null : Convert.ToString(valor);
        }

        // Guarda el nombre del archivo .pfx en la empresa (empresas.nombrecertificado_pfx). Solo se usa
        // cuando la empresa todavia no tenia uno y se instala el primer certificado desde la pantalla:
        // sin ese campo el sistema no ofrece facturacion electronica. No toca ningun otro dato de la empresa.
        public void ActualizarNombreCertificado(string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo)) throw new ArgumentException("Falta el nombre del archivo.", nameof(nombreArchivo));

            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE empresas SET nombrecertificado_pfx = @nombre WHERE idempresa = @idEmpresa;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("nombre", nombreArchivo);
                });
        }

        // Inserta o reemplaza la clave cifrada de la empresa.
        public void GuardarProtegida(string claveProtegida, int idUsuario)
        {
            if (string.IsNullOrEmpty(claveProtegida)) throw new ArgumentException("Falta la clave protegida.", nameof(claveProtegida));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO certificado_arca_clave (idempresa, clave_protegida, actualizado_en, actualizado_por)
                VALUES (@idEmpresa, @clave, now(), @idUsuario)
                ON CONFLICT (idempresa) DO UPDATE
                    SET clave_protegida = EXCLUDED.clave_protegida,
                        actualizado_en = now(),
                        actualizado_por = EXCLUDED.actualizado_por;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("clave", claveProtegida);
                    p.AddWithValue("idUsuario", idUsuario);
                });
        }
    }
}
