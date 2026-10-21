-- Ocultar los datos de la empresa en el ticket de venta no fiscal (2026-10-19, ver docs/DECISIONS.md
-- "Parametro para ocultar los datos de la empresa en el ticket de venta no fiscal"): mismo alta que
-- DatosPostgres/DB-Migrations/20261019d-Insert_parametro_ocultarencabezadoticketnofiscal.sql, para
-- poder editar el parametro en la pantalla Parametros de Servidor SM y San Lorenzo. No inserta valores
-- por empresa: sin valor rige No (el ticket sale como siempre).
--
-- Esquema real verificado el 2026-10-21 en SM y San Lorenzo (dbo.Parametros):
--   - SM:  id INT NOT NULL (PK, sin identity ni default), idParametro INT identity, sin columna tipo.
--   - SL:  id INT NOT NULL (PK, sin identity ni default), idParametro INT NULL (sin identity), sin columna tipo.
-- Por eso el INSERT se arma en dinamico (una columna inexistente nombrada en el lote no compila) y:
--   - id (PK): si la columna existe y no es identity, SIEMPRE se completa con MAX(id) + 1. Una version anterior
--     de este script lo completaba con un UPDATE posterior suponiendo id NULL: fallaba en SM (id es NOT NULL).
--   - idParametro: se omite si es identity; si no, MAX(idParametro) + 1 (igual que en Postgres).
--   - tipo: solo si la columna existe (Datos/Parametros.cs ya contempla bases sin ella).
-- Idempotente (UX_Parametros_Nombre es unico). Compatible con SQL Server 2008 (San Lorenzo).
IF NOT EXISTS (SELECT 1 FROM dbo.Parametros WHERE nombre = 'ocultarEncabezadoTicketNoFiscal')
BEGIN
    DECLARE @nombre NVARCHAR(100);
    DECLARE @descripcion NVARCHAR(100);
    DECLARE @columnas NVARCHAR(400);
    DECLARE @valores NVARCHAR(400);
    DECLARE @sql NVARCHAR(MAX);
    DECLARE @siguiente INT;
    DECLARE @siguienteId INT;
    DECLARE @idEsIdentity BIT;
    DECLARE @incluirId BIT;

    SET @nombre = N'ocultarEncabezadoTicketNoFiscal';
    SET @descripcion = N'Si = el ticket de venta X (no fiscal) no imprime nombre ni slogans de la empresa';
    SELECT @siguiente = ISNULL(MAX(idParametro), 0) + 1 FROM dbo.Parametros;

    SET @idEsIdentity = CASE WHEN COLUMNPROPERTY(OBJECT_ID('dbo.Parametros'), 'idParametro', 'IsIdentity') = 1 THEN 1 ELSE 0 END;
    SET @incluirId = CASE WHEN COL_LENGTH('dbo.Parametros', 'id') IS NOT NULL
                           AND ISNULL(COLUMNPROPERTY(OBJECT_ID('dbo.Parametros'), 'id', 'IsIdentity'), 0) = 0
                          THEN 1 ELSE 0 END;

    -- MAX(id) en dinamico: si la columna id no existiera, nombrarla aca no compilaria.
    SET @siguienteId = @siguiente;
    IF @incluirId = 1
        EXEC sp_executesql N'SELECT @s = ISNULL(MAX(id), 0) + 1 FROM dbo.Parametros', N'@s INT OUTPUT', @siguienteId OUTPUT;

    SET @columnas = N'nombre, descripcion';
    SET @valores = N'@nombre, @descripcion';

    IF @idEsIdentity = 0
    BEGIN
        SET @columnas = N'idParametro, ' + @columnas;
        SET @valores = N'@siguiente, ' + @valores;
    END

    IF @incluirId = 1
    BEGIN
        SET @columnas = N'id, ' + @columnas;
        SET @valores = N'@siguienteId, ' + @valores;
    END

    IF COL_LENGTH('dbo.Parametros', 'tipo') IS NOT NULL
    BEGIN
        SET @columnas = @columnas + N', tipo';
        SET @valores = @valores + N', 2';
    END

    SET @sql = N'INSERT INTO dbo.Parametros (' + @columnas + N') VALUES (' + @valores + N');';
    EXEC sp_executesql @sql,
        N'@siguiente INT, @siguienteId INT, @nombre NVARCHAR(100), @descripcion NVARCHAR(100)',
        @siguiente, @siguienteId, @nombre, @descripcion;
END
