-- Ocultar los datos de la empresa en el ticket de venta no fiscal (2026-10-19, ver docs/DECISIONS.md
-- "Parametro para ocultar los datos de la empresa en el ticket de venta no fiscal"): mismo alta que
-- DatosPostgres/DB-Migrations/20261019d-Insert_parametro_ocultarencabezadoticketnofiscal.sql, para
-- poder editar el parametro en la pantalla Parametros de Servidor SM y San Lorenzo. No inserta valores
-- por empresa: sin valor rige No (el ticket sale como siempre).
--
-- NO CORRIDO TODAVIA. El esquema de dbo.Parametros no esta verificado en estas bases: antes de correr,
-- mirar sys.columns de dbo.Parametros. Por eso el INSERT se arma en dinamico (una columna inexistente
-- nombrada en el lote no compila):
--   - tipo: solo si la columna existe (Datos/Parametros.cs ya contempla bases sin ella). Sin "tipo" el
--     parametro aparece como texto y se carga 1 / 0 a mano (GetBool01 acepta ambos).
--   - idParametro: se omite si es identity; si no, MAX + 1 (igual que en Postgres).
--   - id: si existe y no es identity, lleva el mismo valor que idParametro (en el INSERT, o con un
--     UPDATE posterior si idParametro es identity).
-- Idempotente. Compatible con SQL Server 2008 (San Lorenzo).
IF NOT EXISTS (SELECT 1 FROM dbo.Parametros WHERE nombre = 'ocultarEncabezadoTicketNoFiscal')
BEGIN
    DECLARE @nombre NVARCHAR(100);
    DECLARE @descripcion NVARCHAR(100);
    DECLARE @columnas NVARCHAR(400);
    DECLARE @valores NVARCHAR(400);
    DECLARE @sql NVARCHAR(MAX);
    DECLARE @siguiente INT;
    DECLARE @idEsIdentity BIT;
    DECLARE @completarId BIT;

    SET @nombre = N'ocultarEncabezadoTicketNoFiscal';
    SET @descripcion = N'Si = el ticket de venta X (no fiscal) no imprime nombre ni slogans de la empresa';
    SELECT @siguiente = ISNULL(MAX(idParametro), 0) + 1 FROM dbo.Parametros;

    SET @idEsIdentity = CASE WHEN COLUMNPROPERTY(OBJECT_ID('dbo.Parametros'), 'idParametro', 'IsIdentity') = 1 THEN 1 ELSE 0 END;
    SET @completarId = CASE WHEN COL_LENGTH('dbo.Parametros', 'id') IS NOT NULL
                             AND ISNULL(COLUMNPROPERTY(OBJECT_ID('dbo.Parametros'), 'id', 'IsIdentity'), 0) = 0
                            THEN 1 ELSE 0 END;

    SET @columnas = N'nombre, descripcion';
    SET @valores = N'@nombre, @descripcion';

    -- idParametro (y id, que lo duplica) con MAX + 1 cuando no es identity.
    IF @idEsIdentity = 0
    BEGIN
        SET @columnas = N'idParametro, ' + @columnas;
        SET @valores = N'@siguiente, ' + @valores;
        IF @completarId = 1
        BEGIN
            SET @columnas = @columnas + N', id';
            SET @valores = @valores + N', @siguiente';
        END
    END

    IF COL_LENGTH('dbo.Parametros', 'tipo') IS NOT NULL
    BEGIN
        SET @columnas = @columnas + N', tipo';
        SET @valores = @valores + N', 2';
    END

    SET @sql = N'INSERT INTO dbo.Parametros (' + @columnas + N') VALUES (' + @valores + N');';
    EXEC sp_executesql @sql,
        N'@siguiente INT, @nombre NVARCHAR(100), @descripcion NVARCHAR(100)',
        @siguiente, @nombre, @descripcion;

    -- idParametro identity: el valor recien se conoce despues del INSERT.
    IF @idEsIdentity = 1 AND @completarId = 1
    BEGIN
        EXEC sp_executesql
            N'UPDATE dbo.Parametros SET id = idParametro WHERE nombre = @nombre AND id IS NULL;',
            N'@nombre NVARCHAR(100)',
            @nombre;
    END
END
