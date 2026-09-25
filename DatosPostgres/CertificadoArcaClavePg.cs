// Clave del .pfx del certificado ARCA de una empresa, guardada YA CIFRADA (Data Protection de ASP.NET,
// lo hace WebCore.Services.AfipConfigProvider). Esta clase solo lee/escribe el texto protegido; nunca ve
// la clave en claro. Una fila por empresa y ENTORNO ('PROD'/'HOMO'; tabla certificado_arca_clave, con RLS
// por idempresa): el certificado de homologacion nunca comparte ni pisa la clave del de produccion.
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

        // 'HOMO' o 'PROD' (cualquier otro valor se toma como PROD, igual que AfipEntorno).
        private static string Entorno(bool homologacion)
        {
            return homologacion ? "HOMO" : "PROD";
        }

        // Texto cifrado de la clave del entorno, o null si no hay (pfx historico sin clave).
        public string ObtenerProtegida(bool homologacion)
        {
            object valor = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT clave_protegida FROM certificado_arca_clave WHERE idempresa = @idEmpresa AND entorno = @entorno;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("entorno", Entorno(homologacion));
                });
            return (valor == null || valor == DBNull.Value) ? null : Convert.ToString(valor);
        }

        // Marca a la empresa como "con certificado" (empresas.nombrecertificado_pfx): sin ese campo el sistema no
        // ofrece facturacion electronica. Solo se escribe si esta VACIO (el primer certificado que se instala);
        // no pisa el nombre historico (p. ej. certif-prod.pfx) ni toca ningun otro dato de la empresa.
        public void MarcarConCertificado(string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo)) throw new ArgumentException("Falta el nombre del archivo.", nameof(nombreArchivo));

            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE empresas SET nombrecertificado_pfx = @nombre WHERE idempresa = @idEmpresa AND COALESCE(nombrecertificado_pfx, '') = '';",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("nombre", nombreArchivo);
                });
        }

        // Inserta o reemplaza la clave cifrada de la empresa para ese entorno.
        public void GuardarProtegida(bool homologacion, string claveProtegida, int idUsuario)
        {
            if (string.IsNullOrEmpty(claveProtegida)) throw new ArgumentException("Falta la clave protegida.", nameof(claveProtegida));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO certificado_arca_clave (idempresa, entorno, clave_protegida, actualizado_en, actualizado_por)
                VALUES (@idEmpresa, @entorno, @clave, now(), @idUsuario)
                ON CONFLICT (idempresa, entorno) DO UPDATE
                    SET clave_protegida = EXCLUDED.clave_protegida,
                        actualizado_en = now(),
                        actualizado_por = EXCLUDED.actualizado_por;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("entorno", Entorno(homologacion));
                    p.AddWithValue("clave", claveProtegida);
                    p.AddWithValue("idUsuario", idUsuario);
                });
        }
    }
}
