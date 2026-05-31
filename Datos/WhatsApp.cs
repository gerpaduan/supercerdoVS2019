using System;
using System.Data;
using System.Data.SqlClient;
using Entidades;
using Utilidades;

namespace Datos
{
    public class WhatsApp
    {
        private const string VersionApiDefault = "v22.0";
        private readonly IEmpresaContext _empresa;

        public WhatsApp(IEmpresaContext empresaContext)
        {
            _empresa = empresaContext ?? throw new ArgumentNullException(nameof(empresaContext));
        }

        public ConfiguracionWhatsApp ObtenerOCrearConfiguracion(int? idUsuario)
        {
            AsegurarTablas();

            const string sql = @"
                MERGE dbo.ConfiguracionWhatsApp AS T
                USING (
                    SELECT
                        @idEmpresa AS IdEmpresa,
                        CAST(0 AS bit) AS Activo,
                        @metaApiVersion AS MetaApiVersion,
                        CAST(NULL AS nvarchar(100)) AS PhoneNumberId,
                        CAST(NULL AS nvarchar(100)) AS BusinessAccountId,
                        CAST(NULL AS nvarchar(500)) AS AccessToken,
                        @idUsuario AS IdUsuarioModificacion
                ) AS S
                ON T.IdEmpresa = S.IdEmpresa
                WHEN NOT MATCHED THEN
                    INSERT (IdEmpresa, Activo, MetaApiVersion, PhoneNumberId, BusinessAccountId, AccessToken, FechaAlta, FechaModificacion, IdUsuarioModificacion)
                    VALUES (S.IdEmpresa, S.Activo, S.MetaApiVersion, S.PhoneNumberId, S.BusinessAccountId, S.AccessToken, GETDATE(), GETDATE(), S.IdUsuarioModificacion);";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@metaApiVersion", SqlDbType.NVarChar, 20).Value = VersionApiDefault;
                    p.Add("@idUsuario", SqlDbType.Int).Value = (object)idUsuario ?? DBNull.Value;
                });

            return ObtenerConfiguracion();
        }

        public ConfiguracionWhatsApp ObtenerConfiguracion()
        {
            AsegurarTablas();

            const string sql = @"
                SELECT TOP 1
                    IdConfiguracionWhatsApp,
                    IdEmpresa,
                    Activo,
                    MetaApiVersion,
                    PhoneNumberId,
                    BusinessAccountId,
                    AccessToken,
                    FechaAlta,
                    FechaModificacion,
                    IdUsuarioModificacion
                FROM dbo.ConfiguracionWhatsApp
                WHERE IdEmpresa = @idEmpresa;";

            var items = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                map: dr => new ConfiguracionWhatsApp
                {
                    IdConfiguracionWhatsApp = dr["IdConfiguracionWhatsApp"] == DBNull.Value ? 0 : Convert.ToInt32(dr["IdConfiguracionWhatsApp"]),
                    IdEmpresa = dr["IdEmpresa"] == DBNull.Value ? 0 : Convert.ToInt32(dr["IdEmpresa"]),
                    Activo = dr["Activo"] != DBNull.Value && Convert.ToBoolean(dr["Activo"]),
                    MetaApiVersion = dr["MetaApiVersion"] == DBNull.Value ? VersionApiDefault : Convert.ToString(dr["MetaApiVersion"]),
                    PhoneNumberId = dr["PhoneNumberId"] == DBNull.Value ? "" : Convert.ToString(dr["PhoneNumberId"]),
                    BusinessAccountId = dr["BusinessAccountId"] == DBNull.Value ? "" : Convert.ToString(dr["BusinessAccountId"]),
                    AccessToken = dr["AccessToken"] == DBNull.Value ? "" : Convert.ToString(dr["AccessToken"]),
                    FechaAlta = dr["FechaAlta"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["FechaAlta"]),
                    FechaModificacion = dr["FechaModificacion"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["FechaModificacion"]),
                    IdUsuarioModificacion = dr["IdUsuarioModificacion"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["IdUsuarioModificacion"])
                },
                setParams: p => p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa
            );

            return items.Count > 0 ? items[0] : null;
        }

        public void GuardarConfiguracion(ConfiguracionWhatsApp configuracion)
        {
            if (configuracion == null) throw new ArgumentNullException(nameof(configuracion));

            AsegurarTablas();

            const string sql = @"
                MERGE dbo.ConfiguracionWhatsApp AS T
                USING (
                    SELECT
                        @idEmpresa AS IdEmpresa,
                        @activo AS Activo,
                        @metaApiVersion AS MetaApiVersion,
                        @phoneNumberId AS PhoneNumberId,
                        @businessAccountId AS BusinessAccountId,
                        @accessToken AS AccessToken,
                        @idUsuarioModificacion AS IdUsuarioModificacion
                ) AS S
                ON T.IdEmpresa = S.IdEmpresa
                WHEN MATCHED THEN
                    UPDATE SET
                        Activo = S.Activo,
                        MetaApiVersion = S.MetaApiVersion,
                        PhoneNumberId = S.PhoneNumberId,
                        BusinessAccountId = S.BusinessAccountId,
                        AccessToken = S.AccessToken,
                        FechaModificacion = GETDATE(),
                        IdUsuarioModificacion = S.IdUsuarioModificacion
                WHEN NOT MATCHED THEN
                    INSERT (IdEmpresa, Activo, MetaApiVersion, PhoneNumberId, BusinessAccountId, AccessToken, FechaAlta, FechaModificacion, IdUsuarioModificacion)
                    VALUES (S.IdEmpresa, S.Activo, S.MetaApiVersion, S.PhoneNumberId, S.BusinessAccountId, S.AccessToken, GETDATE(), GETDATE(), S.IdUsuarioModificacion);";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@activo", SqlDbType.Bit).Value = configuracion.Activo;
                    p.Add("@metaApiVersion", SqlDbType.NVarChar, 20).Value = (object)(configuracion.MetaApiVersion ?? VersionApiDefault) ?? DBNull.Value;
                    p.Add("@phoneNumberId", SqlDbType.NVarChar, 100).Value = (object)(configuracion.PhoneNumberId ?? "") ?? DBNull.Value;
                    p.Add("@businessAccountId", SqlDbType.NVarChar, 100).Value = (object)(configuracion.BusinessAccountId ?? "") ?? DBNull.Value;
                    p.Add("@accessToken", SqlDbType.NVarChar, 500).Value = (object)(configuracion.AccessToken ?? "") ?? DBNull.Value;
                    p.Add("@idUsuarioModificacion", SqlDbType.Int).Value = (object)configuracion.IdUsuarioModificacion ?? DBNull.Value;
                });
        }

        public void AsegurarTablas()
        {
            const string sql = @"
                IF OBJECT_ID('dbo.ConfiguracionWhatsApp', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.ConfiguracionWhatsApp
                    (
                        IdConfiguracionWhatsApp INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        IdEmpresa INT NOT NULL,
                        Activo BIT NOT NULL CONSTRAINT DF_ConfiguracionWhatsApp_Activo DEFAULT (0),
                        MetaApiVersion NVARCHAR(20) NOT NULL CONSTRAINT DF_ConfiguracionWhatsApp_MetaApiVersion DEFAULT ('v22.0'),
                        PhoneNumberId NVARCHAR(100) NULL,
                        BusinessAccountId NVARCHAR(100) NULL,
                        AccessToken NVARCHAR(500) NULL,
                        FechaAlta DATETIME NOT NULL CONSTRAINT DF_ConfiguracionWhatsApp_FechaAlta DEFAULT (GETDATE()),
                        FechaModificacion DATETIME NOT NULL CONSTRAINT DF_ConfiguracionWhatsApp_FechaModificacion DEFAULT (GETDATE()),
                        IdUsuarioModificacion INT NULL
                    );

                    CREATE UNIQUE INDEX UX_ConfiguracionWhatsApp_IdEmpresa
                        ON dbo.ConfiguracionWhatsApp(IdEmpresa);
                END;

                IF OBJECT_ID('dbo.WhatsAppEnvios', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.WhatsAppEnvios
                    (
                        IdWhatsAppEnvio INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        IdEmpresa INT NOT NULL,
                        IdVenta INT NULL,
                        IdPersona INT NULL,
                        TelefonoOriginal NVARCHAR(50) NULL,
                        TelefonoFormateado NVARCHAR(50) NULL,
                        NombreArchivo NVARCHAR(255) NULL,
                        MediaId NVARCHAR(100) NULL,
                        Estado NVARCHAR(50) NULL,
                        Exito BIT NOT NULL CONSTRAINT DF_WhatsAppEnvios_Exito DEFAULT (0),
                        MensajeError NVARCHAR(1000) NULL,
                        RespuestaApi NVARCHAR(MAX) NULL,
                        FechaAlta DATETIME NOT NULL CONSTRAINT DF_WhatsAppEnvios_FechaAlta DEFAULT (GETDATE()),
                        IdUsuarioAlta INT NULL
                    );

                    CREATE INDEX IX_WhatsAppEnvios_IdEmpresa_FechaAlta
                        ON dbo.WhatsAppEnvios(IdEmpresa, FechaAlta DESC);
                END;";

            Db.NonQuery(_empresa, sql, CommandType.Text);
        }
    }
}
