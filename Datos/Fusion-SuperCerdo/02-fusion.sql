-- Fusion SuperCerdo SM + SL (SQL Server local) -- paso 2: guardia, mapas y carga de San Lorenzo.
-- Precondicion: [$(DST)] es una copia recien restaurada de la base de San Martin y 01-reglas.sql ya corrio.
-- Inserta en [$(DST)] las filas de [$(SL)] segun fusion.ReglaTabla / fusion.ReglaRef. No toca [$(SL)] ni [$(MAPDB)].
-- Detalle de las reglas: docs/DECISIONS.md "Unificacion SuperCerdo SM + SL" (2026-10-27).
:on error exit
SET NOCOUNT ON;
SET XACT_ABORT ON;
USE [$(DST)];
GO

------------------------------------------------------------------------------------------------
-- 1) Guardia: aborta si el esquema trae algo que las reglas no contemplan (tabla o columna de ID nueva),
--    asi un cambio de esquema futuro no se fusiona "a ciegas".
------------------------------------------------------------------------------------------------
DECLARE @faltan nvarchar(max) = N'';

-- tablas de SL o de SM sin regla
SELECT @faltan += N' tabla:' + t.name
FROM (SELECT name FROM [$(SL)].sys.tables WHERE is_ms_shipped = 0 UNION SELECT name FROM sys.tables WHERE is_ms_shipped = 0 AND schema_id = SCHEMA_ID('dbo')) t
WHERE NOT EXISTS (SELECT 1 FROM fusion.ReglaTabla r WHERE r.tabla = t.name);

-- columnas que parecen guardar un ID y no tienen regla (en tablas que se cargan desde SL)
SELECT @faltan += N' columna:' + t.name + N'.' + c.name
FROM sys.tables t JOIN sys.columns c ON c.object_id = t.object_id
JOIN fusion.ReglaTabla r ON r.tabla = t.name AND r.estrategia NOT IN ('sm', 'vaciaensl', 'ignorar')
WHERE t.schema_id = SCHEMA_ID('dbo') AND c.is_identity = 0
  AND (c.name LIKE 'id%' OR c.name LIKE '%Por' OR c.name IN ('recibidoDe', 'entregadoA', 'sucursalOrigen', 'sucursalDestino'))
  AND c.name NOT IN ('idEmpresa')
  AND NOT (c.name = (SELECT TOP 1 pc.name FROM sys.indexes i   -- la PK la resuelve la estrategia de la tabla
            JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
            JOIN sys.columns pc ON pc.object_id = ic.object_id AND pc.column_id = ic.column_id
            WHERE i.object_id = t.object_id AND i.is_primary_key = 1))
  AND NOT EXISTS (SELECT 1 FROM fusion.ReglaRef rr WHERE rr.tabla = t.name AND rr.columna = c.name);

-- columnas NOT NULL sin default que SL no tiene (la insercion fallaria)
SELECT @faltan += N' notnull:' + t.name + N'.' + c.name
FROM sys.tables t JOIN sys.columns c ON c.object_id = t.object_id
JOIN fusion.ReglaTabla r ON r.tabla = t.name AND r.estrategia NOT IN ('sm', 'vaciaensl', 'ignorar')
WHERE t.schema_id = SCHEMA_ID('dbo') AND c.is_nullable = 0 AND c.is_identity = 0 AND c.is_computed = 0
  AND c.default_object_id = 0 AND c.name <> 'idEmpresa'
  AND NOT EXISTS (SELECT 1 FROM [$(SL)].sys.tables st JOIN [$(SL)].sys.columns sc ON sc.object_id = st.object_id
                  WHERE st.name = t.name AND sc.name = c.name);

IF @faltan <> N'' BEGIN RAISERROR(N'Guardia: falta definir reglas para:%s', 16, 1, @faltan); RETURN; END

-- tablas que deben venir vacias de SL
DECLARE @t sysname, @sql nvarchar(max), @n int;
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT r.tabla FROM fusion.ReglaTabla r WHERE r.estrategia = 'vaciaensl'
      AND EXISTS (SELECT 1 FROM [$(SL)].sys.tables st WHERE st.name = r.tabla);
OPEN cur; FETCH NEXT FROM cur INTO @t;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'SELECT @n = COUNT(*) FROM [$(SL)].dbo.' + QUOTENAME(@t);
    EXEC sp_executesql @sql, N'@n int OUTPUT', @n OUTPUT;
    IF @n > 0 BEGIN RAISERROR(N'Guardia: %s trae %d filas en SL y su regla es "vaciaensl": hay que definir como se fusiona', 16, 1, @t, @n); RETURN; END
    FETCH NEXT FROM cur INTO @t;
END
CLOSE cur; DEALLOCATE cur;

-- rangos: los IDs de SM de las tablas que reciben SL corrido no pueden llegar al OFFSET (se pisarian con los de SL)
DECLARE @off int = (SELECT CAST(valor AS int) FROM fusion.Param WHERE clave = 'OFFSET'), @pkc sysname, @mx bigint;
DECLARE cur2 CURSOR LOCAL FAST_FORWARD FOR
    SELECT r.tabla, pc.name FROM fusion.ReglaTabla r
    JOIN sys.indexes i ON i.object_id = OBJECT_ID(N'dbo.' + QUOTENAME(r.tabla)) AND i.is_primary_key = 1
    JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    JOIN sys.columns pc ON pc.object_id = ic.object_id AND pc.column_id = ic.column_id AND pc.is_identity = 1
    WHERE r.estrategia IN ('offset', 'movimiento', 'porsucursal', 'mapa');
OPEN cur2; FETCH NEXT FROM cur2 INTO @t, @pkc;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'SELECT @mx = MAX(' + QUOTENAME(@pkc) + N') FROM dbo.' + QUOTENAME(@t);
    EXEC sp_executesql @sql, N'@mx bigint OUTPUT', @mx OUTPUT;
    IF @mx >= @off BEGIN RAISERROR(N'Guardia: %s.%s de SM ya llega a %I64d (>= OFFSET): los IDs de SL se pisarian', 16, 1, @t, @pkc, @mx); RETURN; END
    SET @sql = N'SELECT @mx = MAX(' + QUOTENAME(@pkc) + N') FROM [$(SL)].dbo.' + QUOTENAME(@t);
    EXEC sp_executesql @sql, N'@mx bigint OUTPUT', @mx OUTPUT;
    IF @mx >= @off BEGIN RAISERROR(N'Guardia: %s.%s de SL ya llega a %I64d (>= OFFSET)', 16, 1, @t, @pkc, @mx); RETURN; END
    FETCH NEXT FROM cur2 INTO @t, @pkc;
END
CLOSE cur2; DEALLOCATE cur2;
INSERT INTO fusion.Log (paso, detalle) VALUES ('guardia', 'ok');
GO

------------------------------------------------------------------------------------------------
-- 2) Mapas de entidades unidas (fusion.Map)
------------------------------------------------------------------------------------------------
DECLARE @off int = (SELECT CAST(valor AS int) FROM fusion.Param WHERE clave = 'OFFSET');
DECLARE @modo varchar(10) = (SELECT valor FROM fusion.Param WHERE clave = 'MODO');

-- Personas: las decisiones de la fusion del 17/9 (_MergeMapPersonas: CUIT, nombre exacto y 7 manuales) se respetan;
-- las personas de SL creadas despues se unen por CUIT (solo digitos, unico en SM), si no por razon social exacta
-- (unica en SM), si no entran nuevas con ID + OFFSET.
INSERT INTO fusion.Map (espacio, idSL, idFinal, motivo, inserta)
SELECT 'persona', mm.idPersona_SL, mm.idPersona_Final, mm.Motivo, CASE WHEN mm.Motivo = 'SIN_MATCH_OFFSET' THEN 1 ELSE 0 END
FROM [$(MAPDB)].dbo._MergeMapPersonas mm
WHERE EXISTS (SELECT 1 FROM [$(SL)].dbo.Personas p WHERE p.idPersona = mm.idPersona_SL);

;WITH nuevas AS (
    SELECT p.idPersona, p.razonSocial,
           REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(ISNULL(p.cuit, ''))), '-', ''), '.', ''), ' ', '') cuitNorm
    FROM [$(SL)].dbo.Personas p
    WHERE NOT EXISTS (SELECT 1 FROM fusion.Map m WHERE m.espacio = 'persona' AND m.idSL = p.idPersona)),
smp AS (
    SELECT s.idPersona, UPPER(LTRIM(RTRIM(s.razonSocial))) nombre,
           REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(ISNULL(s.cuit, ''))), '-', ''), '.', ''), ' ', '') cuitNorm
    FROM dbo.Personas s)
INSERT INTO fusion.Map (espacio, idSL, idFinal, motivo, inserta)
SELECT 'persona', n.idPersona,
       COALESCE(c.idPersona, nm.idPersona, n.idPersona + @off),
       CASE WHEN c.idPersona IS NOT NULL THEN 'CUIT_EXACTO_NUEVA' WHEN nm.idPersona IS NOT NULL THEN 'NOMBRE_EXACTO_NUEVA' ELSE 'SIN_MATCH_OFFSET' END,
       CASE WHEN c.idPersona IS NULL AND nm.idPersona IS NULL THEN 1 ELSE 0 END
FROM nuevas n
OUTER APPLY (SELECT MIN(s.idPersona) idPersona FROM smp s WHERE LEN(n.cuitNorm) >= 8 AND s.cuitNorm = n.cuitNorm
             HAVING COUNT(*) = 1) c
OUTER APPLY (SELECT MIN(s.idPersona) idPersona FROM smp s WHERE s.nombre = UPPER(LTRIM(RTRIM(n.razonSocial)))
             HAVING COUNT(*) = 1) nm;

-- Productos: decisiones del 17/9 (_MergeMapCorte) + productos nuevos de SL unidos por codigo exacto (unico en SM).
INSERT INTO fusion.Map (espacio, idSL, idFinal, motivo, inserta)
SELECT 'corte', mm.idCorte_SL, mm.idCorte_Final, mm.Motivo, CASE WHEN mm.Motivo = 'SOLO_EN_SL_OFFSET' THEN 1 ELSE 0 END
FROM [$(MAPDB)].dbo._MergeMapCorte mm
WHERE EXISTS (SELECT 1 FROM [$(SL)].dbo.Corte c WHERE c.idCorte = mm.idCorte_SL);

INSERT INTO fusion.Map (espacio, idSL, idFinal, motivo, inserta)
SELECT 'corte', l.idCorte, COALESCE(s.idCorte, l.idCorte + @off),
       CASE WHEN s.idCorte IS NOT NULL THEN 'CODIGO_EXACTO_NUEVO' ELSE 'SOLO_EN_SL_OFFSET' END,
       CASE WHEN s.idCorte IS NULL THEN 1 ELSE 0 END
FROM [$(SL)].dbo.Corte l
OUTER APPLY (SELECT MIN(x.idCorte) idCorte FROM dbo.Corte x WHERE x.codigo = l.codigo HAVING COUNT(*) = 1) s
WHERE NOT EXISTS (SELECT 1 FROM fusion.Map m WHERE m.espacio = 'corte' AND m.idSL = l.idCorte);

-- Usuarios: se unen por nombre de usuario. Los de SL que no estan en SM conservan su ID (si esta libre):
-- en modo corregido se cargan; en modo replica no (asi quedo la fusion del 17/9).
INSERT INTO fusion.Map (espacio, idSL, idFinal, motivo, inserta)
SELECT 'usuario', l.id, COALESCE(s.id, l.id),
       CASE WHEN s.id IS NOT NULL THEN 'NOMBRE_USUARIO' ELSE 'SOLO_EN_SL' END,
       CASE WHEN s.id IS NULL AND @modo = 'corregido' THEN 1 ELSE 0 END
FROM [$(SL)].dbo.Usuarios l
OUTER APPLY (SELECT MIN(x.id) id FROM dbo.Usuarios x WHERE LOWER(x.usuario) = LOWER(l.usuario) HAVING COUNT(*) = 1) s;
IF EXISTS (SELECT 1 FROM fusion.Map m WHERE m.espacio = 'usuario' AND m.inserta = 1 AND EXISTS (SELECT 1 FROM dbo.Usuarios u WHERE u.id = m.idFinal))
    RAISERROR('Un usuario de SL sin pareja en SM tiene un ID que SM ya usa: hay que definir la regla', 16, 1);

-- Tipos de egreso: modo replica = mismo ID (17/9); modo corregido = por nombre (el 17 es "Compra" en SL y "Pan" en SM).
IF @modo = 'replica'
    INSERT INTO fusion.Map (espacio, idSL, idFinal, motivo, inserta)
    SELECT 'tipoegreso', l.id, l.id, 'MISMO_ID', CASE WHEN EXISTS (SELECT 1 FROM dbo.TiposEgresoCaja s WHERE s.id = l.id) THEN 0 ELSE 1 END
    FROM [$(SL)].dbo.TiposEgresoCaja l;
ELSE
BEGIN
    DECLARE @maxTipo int = (SELECT MAX(id) FROM dbo.TiposEgresoCaja);
    INSERT INTO fusion.Map (espacio, idSL, idFinal, motivo, inserta)
    SELECT 'tipoegreso', l.id, COALESCE(s.id, l.id), CASE WHEN s.id IS NOT NULL THEN 'NOMBRE' ELSE 'SOLO_EN_SL' END,
           CASE WHEN s.id IS NULL THEN 1 ELSE 0 END
    FROM [$(SL)].dbo.TiposEgresoCaja l
    OUTER APPLY (SELECT MIN(x.id) id FROM dbo.TiposEgresoCaja x WHERE UPPER(LTRIM(RTRIM(x.tipoEgresoCaja))) = UPPER(LTRIM(RTRIM(l.tipoEgresoCaja))) HAVING COUNT(*) = 1) s;
    -- los que no tienen pareja conservan su ID si esta libre; si no, toman uno nuevo al final
    ;WITH choca AS (SELECT m.idSL, ROW_NUMBER() OVER (ORDER BY m.idSL) rn FROM fusion.Map m
                    WHERE m.espacio = 'tipoegreso' AND m.inserta = 1 AND EXISTS (SELECT 1 FROM dbo.TiposEgresoCaja s WHERE s.id = m.idFinal))
    UPDATE m SET idFinal = @maxTipo + c.rn FROM fusion.Map m JOIN choca c ON c.idSL = m.idSL WHERE m.espacio = 'tipoegreso';
END

INSERT INTO fusion.Log (paso, tabla, filas, detalle)
SELECT 'mapa', espacio, COUNT(*), STUFF((SELECT ', ' + x.motivo + '=' + CAST(COUNT(*) AS varchar) FROM fusion.Map x
       WHERE x.espacio = m.espacio GROUP BY x.motivo FOR XML PATH('')), 1, 2, '')
FROM fusion.Map m GROUP BY espacio;
GO

------------------------------------------------------------------------------------------------
-- 3) Movimientos entre sucursales: cada envio quedo registrado en las dos bases (el que envia lo crea; el que
--    recibe guarda una copia con idMovOrigen = ID del original en la otra base). Se conserva el de quien envia.
------------------------------------------------------------------------------------------------
-- copias en SL de envios de SM: se descartan en los dos modos (asi lo hizo el 17/9)
INSERT INTO fusion.MovDescartado (base, idMovimiento, idMovOrigen)
SELECT 'SL', l.idMovimiento, l.idMovOrigen FROM [$(SL)].dbo.Movimiento l
WHERE ISNULL(l.idMovOrigen, 0) > 0 AND EXISTS (SELECT 1 FROM dbo.Movimiento s WHERE s.idMovimiento = l.idMovOrigen);

-- copias en SM de movimientos registrados originalmente en SL (casi siempre envios de SL; algunos son envios de SM
-- que se cargaron por error desde SL): el 17/9 quedaron duplicadas; en modo corregido se descarta la copia de SM.
-- Copia = idMovOrigen apunta a un movimiento de SL sin origen, misma direccion y misma fecha (+-1 dia), para no
-- confundirla con una referencia interna de SM a otro movimiento propio con el mismo numero.
IF (SELECT valor FROM fusion.Param WHERE clave = 'MODO') = 'corregido'
    INSERT INTO fusion.MovDescartado (base, idMovimiento, idMovOrigen)
    SELECT 'SM', s.idMovimiento, s.idMovOrigen FROM dbo.Movimiento s
    WHERE ISNULL(s.idMovOrigen, 0) > 0
      AND EXISTS (SELECT 1 FROM [$(SL)].dbo.Movimiento l WHERE l.idMovimiento = s.idMovOrigen AND ISNULL(l.idMovOrigen, 0) = 0
                  AND l.sucursalOrigen = s.sucursalOrigen AND l.sucursalDestino = s.sucursalDestino
                  AND ABS(DATEDIFF(minute, l.fechaMovimiento, s.fechaMovimiento)) <= 1440);

DELETE h FROM dbo.MovimientoHistorial h JOIN fusion.MovDescartado d ON d.base = 'SM' AND d.idMovimiento = h.idMovimiento;
INSERT INTO fusion.Log (paso, tabla, filas) VALUES ('descartar SM', 'MovimientoHistorial', @@ROWCOUNT);
DELETE c FROM dbo.CortePorMovimiento c JOIN fusion.MovDescartado d ON d.base = 'SM' AND d.idMovimiento = c.idMovimientos;
INSERT INTO fusion.Log (paso, tabla, filas) VALUES ('descartar SM', 'CortePorMovimiento', @@ROWCOUNT);
DELETE m FROM dbo.Movimiento m JOIN fusion.MovDescartado d ON d.base = 'SM' AND d.idMovimiento = m.idMovimiento;
INSERT INTO fusion.Log (paso, tabla, filas) VALUES ('descartar SM', 'Movimiento', @@ROWCOUNT);

-- los movimientos de SM cuyo idMovOrigen es un movimiento de SL pasan a apuntar a su ID corrido (criterio del 17/9:
-- se mira si el ID existe en SL, no la direccion; asi quedan bien tambien las anulaciones de SM sobre envios de SL)
UPDATE s SET idMovOrigen = s.idMovOrigen + (SELECT CAST(valor AS int) FROM fusion.Param WHERE clave = 'OFFSET')
FROM dbo.Movimiento s
WHERE ISNULL(s.idMovOrigen, 0) > 0 AND EXISTS (SELECT 1 FROM [$(SL)].dbo.Movimiento l WHERE l.idMovimiento = s.idMovOrigen);
INSERT INTO fusion.Log (paso, tabla, filas, detalle) VALUES ('idMovOrigen SM', 'Movimiento', @@ROWCOUNT, 'apuntan a envios de SL: + OFFSET');
INSERT INTO fusion.Log (paso, tabla, filas)
SELECT 'mov descartados ' + base, 'Movimiento', COUNT(*) FROM fusion.MovDescartado GROUP BY base;
GO

------------------------------------------------------------------------------------------------
-- 4) Carga generica de una tabla de SL en el destino
------------------------------------------------------------------------------------------------
IF OBJECT_ID('fusion.p_Cargar') IS NOT NULL DROP PROCEDURE fusion.p_Cargar;
GO
-- Arma y ejecuta INSERT INTO dbo.<tabla> SELECT ... FROM [SL].dbo.<tabla> aplicando a cada columna su regla:
--   PK identity -> segun @pkModo ('offset' +OFFSET, 'mapa' idFinal de fusion.Map, 'igual' sin cambio);
--   columna en fusion.ReglaRef -> offset / mapa / identidad; columna que SL no tiene -> idEmpresa=1 o el default.
-- @filtro es un predicado extra sobre el alias s (fila de SL). Devuelve en fusion.Log cuantas filas inserto.
CREATE PROCEDURE fusion.p_Cargar @tabla sysname, @pkModo varchar(10), @filtro nvarchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @off int = (SELECT CAST(valor AS int) FROM fusion.Param WHERE clave = 'OFFSET');
    DECLARE @espPk varchar(30) = (SELECT espacioPk FROM fusion.ReglaTabla WHERE tabla = @tabla);
    DECLARE @cols nvarchar(max) = N'', @exprs nvarchar(max) = N'', @joins nvarchar(max) = N'', @sql nvarchar(max);
    DECLARE @col sysname, @ident bit, @esPk bit, @espacio varchar(30), @tipo varchar(10), @enSL bit, @k int = 0;

    DECLARE c CURSOR LOCAL FAST_FORWARD FOR
        SELECT c.name, c.is_identity,
               CASE WHEN EXISTS (SELECT 1 FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                                 WHERE i.object_id = c.object_id AND i.is_primary_key = 1 AND ic.column_id = c.column_id
                                   AND (SELECT COUNT(*) FROM sys.index_columns x WHERE x.object_id = i.object_id AND x.index_id = i.index_id) = 1)
                    THEN 1 ELSE 0 END,
               rr.espacio, e.tipo,
               CASE WHEN EXISTS (SELECT 1 FROM [$(SL)].sys.tables st JOIN [$(SL)].sys.columns sc ON sc.object_id = st.object_id
                                 WHERE st.name = @tabla AND sc.name = c.name) THEN 1 ELSE 0 END
        FROM sys.columns c
        LEFT JOIN fusion.ReglaRef rr ON rr.tabla = @tabla AND rr.columna = c.name
        LEFT JOIN fusion.Espacio e ON e.espacio = rr.espacio
        WHERE c.object_id = OBJECT_ID(N'dbo.' + QUOTENAME(@tabla)) AND c.is_computed = 0 AND TYPE_NAME(c.system_type_id) <> 'timestamp'
        ORDER BY c.column_id;
    OPEN c; FETCH NEXT FROM c INTO @col, @ident, @esPk, @espacio, @tipo, @enSL;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @e nvarchar(max) = NULL;
        IF @esPk = 1 AND @pkModo = 'offset' AND @enSL = 1 SET @e = N's.' + QUOTENAME(@col) + N' + ' + CAST(@off AS nvarchar);
        ELSE IF @esPk = 1 AND @pkModo = 'mapa'
        BEGIN
            SET @e = N'pkm.idFinal';
            SET @joins += N' JOIN fusion.Map pkm ON pkm.espacio = ''' + @espPk + N''' AND pkm.idSL = s.' + QUOTENAME(@col) + N' AND pkm.inserta = 1';
        END
        ELSE IF @enSL = 1 AND @tipo = 'offset' AND @espacio = 'idtabla'
            SET @e = N'CASE WHEN s.tabla IN (''Ventas'',''Pagos'',''Compras'') AND s.' + QUOTENAME(@col) + N' > 0 THEN s.' + QUOTENAME(@col) + N' + ' + CAST(@off AS nvarchar) + N' ELSE s.' + QUOTENAME(@col) + N' END';
        ELSE IF @enSL = 1 AND @tipo = 'offset'
            SET @e = N'CASE WHEN s.' + QUOTENAME(@col) + N' > 0 THEN s.' + QUOTENAME(@col) + N' + ' + CAST(@off AS nvarchar) + N' ELSE s.' + QUOTENAME(@col) + N' END';
        ELSE IF @enSL = 1 AND @tipo = 'mapa'
        BEGIN
            SET @k += 1;
            SET @e = N'COALESCE(m' + CAST(@k AS nvarchar) + N'.idFinal, s.' + QUOTENAME(@col) + N')';
            SET @joins += N' LEFT JOIN fusion.Map m' + CAST(@k AS nvarchar) + N' ON m' + CAST(@k AS nvarchar) + N'.espacio = ''' + @espacio
                        + N''' AND m' + CAST(@k AS nvarchar) + N'.idSL = s.' + QUOTENAME(@col);
        END
        ELSE IF @enSL = 1 SET @e = N's.' + QUOTENAME(@col);
        ELSE IF @col = 'idEmpresa' SET @e = N'1';
        IF @e IS NOT NULL
        BEGIN
            SET @cols += CASE WHEN @cols = N'' THEN N'' ELSE N', ' END + QUOTENAME(@col);
            SET @exprs += CASE WHEN @exprs = N'' THEN N'' ELSE N', ' END + @e;
        END
        FETCH NEXT FROM c INTO @col, @ident, @esPk, @espacio, @tipo, @enSL;
    END
    CLOSE c; DEALLOCATE c;

    DECLARE @hasIdent bit = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.' + QUOTENAME(@tabla)) AND is_identity = 1) THEN 1 ELSE 0 END;
    SET @sql = CASE WHEN @hasIdent = 1 THEN N'SET IDENTITY_INSERT dbo.' + QUOTENAME(@tabla) + N' ON; ' ELSE N'' END
             + N'INSERT INTO dbo.' + QUOTENAME(@tabla) + N' (' + @cols + N') SELECT ' + @exprs
             + N' FROM [$(SL)].dbo.' + QUOTENAME(@tabla) + N' s' + @joins
             + CASE WHEN @filtro IS NULL THEN N'' ELSE N' WHERE ' + @filtro END + N'; '
             + N'INSERT INTO fusion.Log (paso, tabla, filas) VALUES (''cargar SL'', ''' + @tabla + N''', @@ROWCOUNT); '
             + CASE WHEN @hasIdent = 1 THEN N'SET IDENTITY_INSERT dbo.' + QUOTENAME(@tabla) + N' OFF;' ELSE N'' END;
    EXEC (@sql);
END
GO

------------------------------------------------------------------------------------------------
-- 5) Carga, con las FKs apagadas (se revalidan al final)
------------------------------------------------------------------------------------------------
-- solo las tablas de dbo (las de fusion no tienen restricciones que apagar)
DECLARE @nc nvarchar(max) = N'';
SELECT @nc += N'ALTER TABLE dbo.' + QUOTENAME(t.name) + N' NOCHECK CONSTRAINT ALL; '
FROM sys.tables t WHERE t.schema_id = SCHEMA_ID('dbo')
  AND EXISTS (SELECT 1 FROM sys.objects o WHERE o.parent_object_id = t.object_id AND o.type IN ('F', 'C'));
EXEC (@nc);

DECLARE @modo varchar(10) = (SELECT valor FROM fusion.Param WHERE clave = 'MODO');
DECLARE @t sysname, @est varchar(12);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT tabla, estrategia FROM fusion.ReglaTabla
    WHERE estrategia IN ('mapa', 'offset', 'copiar', 'sinpk', 'movimiento')
      AND EXISTS (SELECT 1 FROM [$(SL)].sys.tables st WHERE st.name = tabla)
    ORDER BY orden, tabla;
OPEN cur; FETCH NEXT FROM cur INTO @t, @est;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF @t = 'PermisosUsuarios'
        EXEC fusion.p_Cargar @t, 'igual', N'EXISTS (SELECT 1 FROM fusion.Map x WHERE x.espacio = ''usuario'' AND x.idSL = s.idUsuario AND x.inserta = 1)';
    ELSE IF @est = 'mapa'
        EXEC fusion.p_Cargar @t, 'mapa';
    ELSE IF @t = 'Movimiento'
        EXEC fusion.p_Cargar @t, 'offset', N'NOT EXISTS (SELECT 1 FROM fusion.MovDescartado d WHERE d.base = ''SL'' AND d.idMovimiento = s.idMovimiento)';
    ELSE IF @t IN ('CortePorMovimiento', 'MovimientoHistorial')
    BEGIN
        -- hijos de un movimiento de SL: nunca los de una copia descartada. Las lineas huerfanas (su movimiento no existe
        -- en SL) el 17/9 se descartaron (modo replica); en modo corregido se conservan para no perder nada de SL.
        DECLARE @colMov sysname = CASE WHEN @t = 'CortePorMovimiento' THEN 'idMovimientos' ELSE 'idMovimiento' END;
        DECLARE @fm nvarchar(max) = N'NOT EXISTS (SELECT 1 FROM fusion.MovDescartado d WHERE d.base = ''SL'' AND d.idMovimiento = s.' + @colMov + N')'
            + CASE WHEN @modo = 'replica' THEN N' AND EXISTS (SELECT 1 FROM [$(SL)].dbo.Movimiento lm WHERE lm.idMovimiento = s.' + @colMov + N')' ELSE N'' END;
        EXEC fusion.p_Cargar @t, 'offset', @fm;
    END
    ELSE IF @est = 'copiar'
        EXEC fusion.p_Cargar @t, 'igual';
    ELSE
        EXEC fusion.p_Cargar @t, 'offset';
    FETCH NEXT FROM cur INTO @t, @est;
END
CLOSE cur; DEALLOCATE cur;
GO

------------------------------------------------------------------------------------------------
-- 6) Configuracion por sucursal (punto de stock, stock, jerarquia).
--    corregido: para San Lorenzo (sucursal 1) vale lo que dice la base de SL (reemplaza lo que SM tenia anotado de SL,
--               solo para los productos que SL tiene configurados); San Martin (2) sale de la base de SM y de SL
--               solo entran productos que SM no tenia configurados en la sucursal 2.
--    replica  : vale todo lo de SM; de SL solo entran los productos que no existian en SM (asi quedo el 17/9).
------------------------------------------------------------------------------------------------
DECLARE @modo varchar(10) = (SELECT valor FROM fusion.Param WHERE clave = 'MODO');
-- ID de producto de una fila de SL ya traducido (alias s)
DECLARE @corteFinal nvarchar(400) = N'COALESCE((SELECT mm.idFinal FROM fusion.Map mm WHERE mm.espacio = ''corte'' AND mm.idSL = s.idCorte), s.idCorte)';
-- filas de SL cuyo producto no existe en SL (huerfanas de origen): no se cargan; si no, chocan con un producto de SL
-- unido a uno de SM (ej. SL tiene stock del producto 196 que no existe en SL, y su 197 se une al 196 de SM).
DECLARE @existeEnSL nvarchar(400) = N'EXISTS (SELECT 1 FROM [$(SL)].dbo.Corte cx WHERE cx.idCorte = s.idCorte)';
DECLARE @t sysname, @pk varchar(10), @sql nvarchar(max), @filtro nvarchar(max);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT tabla FROM fusion.ReglaTabla WHERE estrategia = 'porsucursal'
      AND EXISTS (SELECT 1 FROM [$(SL)].sys.tables st WHERE st.name = tabla) ORDER BY orden;
OPEN cur; FETCH NEXT FROM cur INTO @t;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @pk = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.' + QUOTENAME(@t)) AND is_identity = 1) THEN 'offset' ELSE 'igual' END;
    IF @modo = 'corregido'
    BEGIN
        SET @sql = N'DELETE x FROM dbo.' + QUOTENAME(@t) + N' x WHERE x.idSucursal = 1 AND EXISTS (SELECT 1 FROM [$(SL)].dbo.'
                 + QUOTENAME(@t) + N' s WHERE s.idSucursal = 1 AND ' + @existeEnSL + N' AND ' + @corteFinal + N' = x.idCorte); '
                 + N'INSERT INTO fusion.Log (paso, tabla, filas) VALUES (''reemplazar suc 1 de SM'', ''' + @t + N''', @@ROWCOUNT);';
        EXEC (@sql);
        SET @filtro = @existeEnSL + N' AND (s.idSucursal = 1 OR (s.idSucursal <> 1 AND NOT EXISTS (SELECT 1 FROM dbo.' + QUOTENAME(@t)
                    + N' x WHERE x.idSucursal = s.idSucursal AND x.idCorte = ' + @corteFinal + N')))';
    END
    ELSE
        SET @filtro = @existeEnSL + N' AND EXISTS (SELECT 1 FROM fusion.Map mm WHERE mm.espacio = ''corte'' AND mm.idSL = s.idCorte AND mm.inserta = 1)';
    EXEC fusion.p_Cargar @t, @pk, @filtro;
    FETCH NEXT FROM cur INTO @t;
END
CLOSE cur; DEALLOCATE cur;
GO

------------------------------------------------------------------------------------------------
-- 7) Revalidar FKs y CHECKs (WITH CHECK). Si alguna no valida (referencias huerfanas que ya venian en los origenes) queda
--    habilitada sin validar y se anota en fusion.Log; 03-verificar.sql lista los huerfanos.
------------------------------------------------------------------------------------------------
DECLARE @tb sysname, @fk sysname, @sql nvarchar(max);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT OBJECT_NAME(parent_object_id), name FROM sys.objects WHERE type IN ('F', 'C') AND OBJECTPROPERTY(parent_object_id, 'IsUserTable') = 1 AND OBJECT_SCHEMA_NAME(parent_object_id) = 'dbo';
OPEN cur; FETCH NEXT FROM cur INTO @tb, @fk;
WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
        SET @sql = N'ALTER TABLE dbo.' + QUOTENAME(@tb) + N' WITH CHECK CHECK CONSTRAINT ' + QUOTENAME(@fk);
        EXEC (@sql);
    END TRY
    BEGIN CATCH
        INSERT INTO fusion.Log (paso, tabla, detalle) VALUES ('FK sin validar', @tb, @fk + ': ' + ERROR_MESSAGE());
        SET @sql = N'ALTER TABLE dbo.' + QUOTENAME(@tb) + N' CHECK CONSTRAINT ' + QUOTENAME(@fk);
        EXEC (@sql);
    END CATCH
    FETCH NEXT FROM cur INTO @tb, @fk;
END
CLOSE cur; DEALLOCATE cur;
INSERT INTO fusion.Log (paso, detalle) VALUES ('fin', 'fusion completa');
GO
