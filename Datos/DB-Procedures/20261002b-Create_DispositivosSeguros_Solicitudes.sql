-- Solicitud de autorizacion de dispositivo al administrador (2026-10-02, ver docs/DECISIONS.md
-- "Login por CUIT, clave rapida (PIN) y politica de clave", Fase 1c). Espeja
-- DatosPostgres/DB-Migrations/20261002b-Create_dispositivosseguros_solicitudes.sql.
--
-- SIN linea USE: ejecutar sobre la base correcta (dev local: CarniSys; SM y San Lorenzo:
-- SuperCerdo, SQL Server 2008 RTM -- sin sintaxis posterior a 2008, salvo DATETIME2 que ya usan
-- otras tablas de esas bases). Verificar en vivo que la tabla no existe antes de ejecutar y pedir
-- aprobacion explicita por servidor. Tabla nueva y aditiva: WinForms/clasico no la conocen.

IF OBJECT_ID('dbo.DispositivosSeguros_Solicitudes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DispositivosSeguros_Solicitudes (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DispositivosSeguros_Solicitudes PRIMARY KEY,
        IdEmpresa INT NOT NULL,
        IdUsuario INT NOT NULL,
        Serie NVARCHAR(200) NOT NULL,
        Nombre NVARCHAR(100) NOT NULL,
        Mensaje NVARCHAR(500) NULL,
        Ip NVARCHAR(100) NULL,
        Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_DispSolicitudes_Estado DEFAULT ('pendiente'),
        CreadaUtc DATETIME2 NOT NULL,
        ResueltaPor INT NULL,
        ResueltaUtc DATETIME2 NULL,
        CONSTRAINT CK_DispSolicitudes_Estado CHECK (Estado IN ('pendiente', 'aprobada', 'rechazada'))
    );

    CREATE INDEX IX_DispSolicitudes_IdEmpresa_Estado ON dbo.DispositivosSeguros_Solicitudes (IdEmpresa, Estado);
    CREATE INDEX IX_DispSolicitudes_Serie ON dbo.DispositivosSeguros_Solicitudes (Serie);
END
GO
