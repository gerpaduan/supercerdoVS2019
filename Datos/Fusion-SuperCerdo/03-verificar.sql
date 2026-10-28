-- Fusion SuperCerdo SM + SL -- paso 3: el juez. Compara la base fusionada [$(DST)] contra sus origenes [$(SM)] y [$(SL)].
-- Chequeos (cada uno OK / FALLA; los "info" no hacen fallar):
--   filas     : por tabla, filas(DST) = filas(SM) + filas de SL que la regla carga - copias de SM descartadas.
--   sumas     : por tabla transaccional, la suma de cada columna numerica que no es un ID es SM + SL.
--   ctacte    : el saldo por persona y tipo (Credito/Debito) de MovCtaCte es la suma de las personas de origen unidas.
--   huerfanos : referencias a IDs inexistentes en DST = las que ya venian huerfanas en SM + en SL (no se crean nuevas).
--   duplicados: en modo corregido no queda ningun envio entre sucursales registrado dos veces.
--   sucursal  : en modo corregido la configuracion de la sucursal 1 (San Lorenzo) coincide con la base de SL.
-- Al final aborta (error) si hubo alguna FALLA. Detalle: docs/DECISIONS.md "Unificacion SuperCerdo SM + SL".
:on error exit
SET NOCOUNT ON;
USE [$(DST)];
GO
IF OBJECT_ID('fusion.Verif') IS NOT NULL DROP TABLE fusion.Verif;
CREATE TABLE fusion.Verif (id int IDENTITY PRIMARY KEY, chequeo varchar(20) NOT NULL, tabla nvarchar(300) NOT NULL,
    esperado float NULL, obtenido float NULL, ok bit NULL, nota nvarchar(400) NULL);
GO
------------------------------------------------------------------------------------------------
-- filas y sumas por tabla
------------------------------------------------------------------------------------------------
DECLARE @off int = (SELECT CAST(valor AS int) FROM fusion.Param WHERE clave = 'OFFSET');
DECLARE @t sysname, @est varchar(12), @sql nvarchar(max), @sm float, @sl float, @dst float, @c sysname;
DECLARE @fSM nvarchar(400), @fSL nvarchar(max);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT r.tabla, r.estrategia FROM fusion.ReglaTabla r
    WHERE EXISTS (SELECT 1 FROM sys.tables t WHERE t.name = r.tabla AND t.schema_id = SCHEMA_ID('dbo')) ORDER BY r.orden, r.tabla;
OPEN cur; FETCH NEXT FROM cur INTO @t, @est;
WHILE @@FETCH_STATUS = 0
BEGIN
    -- que filas de cada origen deberian estar en DST
    SET @fSM = N'1 = 1'; SET @fSL = NULL;
    IF @est IN ('offset', 'copiar', 'sinpk') SET @fSL = N'1 = 1';
    IF @est = 'mapa' AND @t <> 'PermisosUsuarios'
        SET @fSL = N'EXISTS (SELECT 1 FROM fusion.Map m JOIN fusion.ReglaTabla rt ON rt.tabla = ''' + @t + N''' AND m.espacio = rt.espacioPk WHERE m.inserta = 1 AND m.idSL = s.'
                 + QUOTENAME((SELECT TOP 1 pc.name FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                              JOIN sys.columns pc ON pc.object_id = ic.object_id AND pc.column_id = ic.column_id
                              WHERE i.object_id = OBJECT_ID(N'dbo.' + QUOTENAME(@t)) AND i.is_primary_key = 1)) + N')';
    IF @t = 'PermisosUsuarios' SET @fSL = N'EXISTS (SELECT 1 FROM fusion.Map m WHERE m.espacio = ''usuario'' AND m.inserta = 1 AND m.idSL = s.idUsuario)';
    IF @t = 'Movimiento'
    BEGIN SET @fSM = N'NOT EXISTS (SELECT 1 FROM fusion.MovDescartado d WHERE d.base = ''SM'' AND d.idMovimiento = s.idMovimiento)';
          SET @fSL = N'NOT EXISTS (SELECT 1 FROM fusion.MovDescartado d WHERE d.base = ''SL'' AND d.idMovimiento = s.idMovimiento)'; END
    IF @t = 'CortePorMovimiento'
    BEGIN SET @fSM = N'NOT EXISTS (SELECT 1 FROM fusion.MovDescartado d WHERE d.base = ''SM'' AND d.idMovimiento = s.idMovimientos)';
          SET @fSL = N'NOT EXISTS (SELECT 1 FROM fusion.MovDescartado d WHERE d.base = ''SL'' AND d.idMovimiento = s.idMovimientos)'; END
    IF @t = 'MovimientoHistorial'
    BEGIN SET @fSM = N'NOT EXISTS (SELECT 1 FROM fusion.MovDescartado d WHERE d.base = ''SM'' AND d.idMovimiento = s.idMovimiento)';
          SET @fSL = N'NOT EXISTS (SELECT 1 FROM fusion.MovDescartado d WHERE d.base = ''SL'' AND d.idMovimiento = s.idMovimiento)'; END
    -- modo replica: el 17/9 se descartaron las lineas de SL cuyo movimiento no existe en SL (ver 02-fusion.sql)
    IF @t IN ('CortePorMovimiento', 'MovimientoHistorial') AND (SELECT valor FROM fusion.Param WHERE clave = 'MODO') = 'replica'
        SET @fSL += N' AND EXISTS (SELECT 1 FROM [$(SL)].dbo.Movimiento lm WHERE lm.idMovimiento = s.'
                  + CASE WHEN @t = 'CortePorMovimiento' THEN N'idMovimientos' ELSE N'idMovimiento' END + N')';

    IF @est <> 'porsucursal'
    BEGIN
        SET @sql = N'SELECT @sm = COUNT(*) FROM [$(SM)].dbo.' + QUOTENAME(@t) + N' s WHERE ' + @fSM + N'; SELECT @dst = COUNT(*) FROM dbo.' + QUOTENAME(@t) + N';'
                 + CASE WHEN @fSL IS NULL OR NOT EXISTS (SELECT 1 FROM [$(SL)].sys.tables st WHERE st.name = @t) THEN N' SET @sl = 0;'
                        ELSE N' SELECT @sl = COUNT(*) FROM [$(SL)].dbo.' + QUOTENAME(@t) + N' s WHERE ' + @fSL + N';' END;
        EXEC sp_executesql @sql, N'@sm float OUTPUT, @sl float OUTPUT, @dst float OUTPUT', @sm OUTPUT, @sl OUTPUT, @dst OUTPUT;
        INSERT INTO fusion.Verif (chequeo, tabla, esperado, obtenido, ok, nota) VALUES ('filas', @t, @sm + @sl, @dst, CASE WHEN @sm + @sl = @dst THEN 1 ELSE 0 END, @est);
    END

    -- sumas de columnas numericas que no son IDs (solo tablas que suman SM + SL tal cual)
    IF @est IN ('offset', 'copiar', 'sinpk', 'movimiento') AND EXISTS (SELECT 1 FROM [$(SL)].sys.tables st WHERE st.name = @t)
    BEGIN
        DECLARE cc CURSOR LOCAL FAST_FORWARD FOR
            SELECT c.name FROM sys.columns c
            WHERE c.object_id = OBJECT_ID(N'dbo.' + QUOTENAME(@t)) AND TYPE_NAME(c.system_type_id) IN ('float', 'real', 'decimal', 'numeric', 'money', 'int', 'bigint', 'smallint', 'tinyint', 'bit')
              AND c.is_identity = 0 AND c.name <> 'idEmpresa'
              AND NOT EXISTS (SELECT 1 FROM fusion.ReglaRef rr JOIN fusion.Espacio e ON e.espacio = rr.espacio
                              WHERE rr.tabla = @t AND rr.columna = c.name AND e.tipo <> 'identidad')
              AND NOT EXISTS (SELECT 1 FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                              WHERE i.object_id = c.object_id AND i.is_primary_key = 1 AND ic.column_id = c.column_id)
              AND EXISTS (SELECT 1 FROM [$(SL)].sys.tables st JOIN [$(SL)].sys.columns sc ON sc.object_id = st.object_id WHERE st.name = @t AND sc.name = c.name);
        OPEN cc; FETCH NEXT FROM cc INTO @c;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @sql = N'SELECT @sm = ISNULL(SUM(CAST(s.' + QUOTENAME(@c) + N' AS float)), 0) FROM [$(SM)].dbo.' + QUOTENAME(@t) + N' s WHERE ' + @fSM
                     + N'; SELECT @sl = ISNULL(SUM(CAST(s.' + QUOTENAME(@c) + N' AS float)), 0) FROM [$(SL)].dbo.' + QUOTENAME(@t) + N' s WHERE ' + @fSL
                     + N'; SELECT @dst = ISNULL(SUM(CAST(s.' + QUOTENAME(@c) + N' AS float)), 0) FROM dbo.' + QUOTENAME(@t) + N' s;';
            EXEC sp_executesql @sql, N'@sm float OUTPUT, @sl float OUTPUT, @dst float OUTPUT', @sm OUTPUT, @sl OUTPUT, @dst OUTPUT;
            INSERT INTO fusion.Verif (chequeo, tabla, esperado, obtenido, ok)
            VALUES ('sumas', @t + N'.' + @c, @sm + @sl, @dst, CASE WHEN ABS(@sm + @sl - @dst) <= 1e-6 * (1 + ABS(@dst)) THEN 1 ELSE 0 END);
            FETCH NEXT FROM cc INTO @c;
        END
        CLOSE cc; DEALLOCATE cc;
    END
    FETCH NEXT FROM cur INTO @t, @est;
END
CLOSE cur; DEALLOCATE cur;
GO
------------------------------------------------------------------------------------------------
-- cuenta corriente: saldo por persona final y tipo
------------------------------------------------------------------------------------------------
;WITH origen AS (
    SELECT s.idPersona, s.tipo, s.importe FROM [$(SM)].dbo.MovCtaCte s
    UNION ALL
    SELECT COALESCE(m.idFinal, s.idPersona), s.tipo, s.importe FROM [$(SL)].dbo.MovCtaCte s
    LEFT JOIN fusion.Map m ON m.espacio = 'persona' AND m.idSL = s.idPersona),
o AS (SELECT idPersona, tipo, SUM(importe) total FROM origen GROUP BY idPersona, tipo),
d AS (SELECT idPersona, tipo, SUM(importe) total FROM dbo.MovCtaCte GROUP BY idPersona, tipo)
INSERT INTO fusion.Verif (chequeo, tabla, esperado, obtenido, ok, nota)
SELECT 'ctacte', 'MovCtaCte saldo por persona y tipo', COUNT(*),
       SUM(CASE WHEN ABS(ISNULL(o.total, 0) - ISNULL(d.total, 0)) <= 0.01 THEN 1 ELSE 0 END),
       CASE WHEN SUM(CASE WHEN ABS(ISNULL(o.total, 0) - ISNULL(d.total, 0)) <= 0.01 THEN 0 ELSE 1 END) = 0 THEN 1 ELSE 0 END,
       'esperado = personas x tipo; obtenido = las que coinciden'
FROM o FULL JOIN d ON d.idPersona = o.idPersona AND d.tipo = o.tipo;
GO
------------------------------------------------------------------------------------------------
-- referencias huerfanas: DST no debe tener mas de las que ya traian los origenes
------------------------------------------------------------------------------------------------
DECLARE @off int = (SELECT CAST(valor AS int) FROM fusion.Param WHERE clave = 'OFFSET');
DECLARE @t sysname, @col sysname, @tdest sysname, @pk sysname, @esp varchar(30), @sql nvarchar(max), @sm float, @sl float, @dst float;
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT rr.tabla, rr.columna, e.tablaDestino, e.pkDestino, e.espacio FROM fusion.ReglaRef rr JOIN fusion.Espacio e ON e.espacio = rr.espacio
    WHERE e.tablaDestino IS NOT NULL AND COL_LENGTH(N'dbo.' + QUOTENAME(rr.tabla), rr.columna) IS NOT NULL
      AND OBJECT_ID(N'dbo.' + QUOTENAME(e.tablaDestino)) IS NOT NULL
    ORDER BY rr.tabla, rr.columna;
OPEN cur; FETCH NEXT FROM cur INTO @t, @col, @tdest, @pk, @esp;
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @q nvarchar(max) = N'SELECT @n = COUNT(*) FROM {db}dbo.' + QUOTENAME(@t) + N' x WHERE x.' + QUOTENAME(@col) + N' > 0 AND NOT EXISTS (SELECT 1 FROM {db}dbo.'
        + QUOTENAME(@tdest) + N' y WHERE y.' + QUOTENAME(@pk) + N' = x.' + QUOTENAME(@col) + N')';
    SET @sql = REPLACE(@q, '{db}', '[$(SM)].'); EXEC sp_executesql @sql, N'@n float OUTPUT', @sm OUTPUT;
    IF EXISTS (SELECT 1 FROM [$(SL)].sys.tables st JOIN [$(SL)].sys.columns sc ON sc.object_id = st.object_id WHERE st.name = @t AND sc.name = @col)
       AND EXISTS (SELECT 1 FROM [$(SL)].sys.tables st WHERE st.name = @tdest)
       AND (SELECT estrategia FROM fusion.ReglaTabla WHERE tabla = @t) NOT IN ('sm', 'vaciaensl', 'ignorar')
    BEGIN SET @sql = REPLACE(@q, '{db}', '[$(SL)].'); EXEC sp_executesql @sql, N'@n float OUTPUT', @sl OUTPUT; END
    ELSE SET @sl = 0;
    SET @sql = REPLACE(@q, '{db}', ''); EXEC sp_executesql @sql, N'@n float OUTPUT', @dst OUTPUT;
    IF @sm + @sl + @dst > 0
        INSERT INTO fusion.Verif (chequeo, tabla, esperado, obtenido, ok, nota)
        VALUES ('huerfanos', @t + N'.' + @col + N' -> ' + @tdest, @sm + @sl, @dst, CASE WHEN @dst <= @sm + @sl THEN 1 ELSE 0 END,
                N'esperado = huerfanos que ya traian SM + SL');
    FETCH NEXT FROM cur INTO @t, @col, @tdest, @pk, @esp;
END
CLOSE cur; DEALLOCATE cur;
GO
------------------------------------------------------------------------------------------------
-- duplicados de envios entre sucursales y configuracion de la sucursal 1 (solo modo corregido)
------------------------------------------------------------------------------------------------
IF (SELECT valor FROM fusion.Param WHERE clave = 'MODO') = 'corregido'
BEGIN
    INSERT INTO fusion.Verif (chequeo, tabla, esperado, obtenido, ok, nota)
    SELECT 'duplicados', 'Movimiento: original y copia del mismo envio', 0, COUNT(*), CASE WHEN COUNT(*) = 0 THEN 1 ELSE 0 END, NULL
    FROM dbo.Movimiento c JOIN dbo.Movimiento o ON o.idMovimiento = c.idMovOrigen AND o.sucursalOrigen = c.sucursalOrigen AND o.sucursalDestino = c.sucursalDestino
    WHERE ISNULL(c.idMovOrigen, 0) > 0 AND ABS(DATEDIFF(minute, c.fechaMovimiento, o.fechaMovimiento)) <= 1440;

    INSERT INTO fusion.Verif (chequeo, tabla, esperado, obtenido, ok, nota)
    SELECT 'sucursal', 'CortePuntoStockSucursal suc 1 = SL', COUNT(*), SUM(CASE WHEN d.puntoStock = l.puntoStock THEN 1 ELSE 0 END),
           CASE WHEN COUNT(*) = SUM(CASE WHEN d.puntoStock = l.puntoStock THEN 1 ELSE 0 END) THEN 1 ELSE 0 END, NULL
    FROM [$(SL)].dbo.CortePuntoStockSucursal l
    LEFT JOIN fusion.Map m ON m.espacio = 'corte' AND m.idSL = l.idCorte
    LEFT JOIN dbo.CortePuntoStockSucursal d ON d.idSucursal = 1 AND d.idCorte = COALESCE(m.idFinal, l.idCorte)
    WHERE l.idSucursal = 1 AND EXISTS (SELECT 1 FROM [$(SL)].dbo.Corte cx WHERE cx.idCorte = l.idCorte);

    INSERT INTO fusion.Verif (chequeo, tabla, esperado, obtenido, ok, nota)
    SELECT 'sucursal', 'StockCorteSucursal suc 1 = SL', COUNT(*), SUM(CASE WHEN ABS(ISNULL(d.stock, 0) - ISNULL(l.stock, 0)) < 1e-6 THEN 1 ELSE 0 END),
           CASE WHEN COUNT(*) = SUM(CASE WHEN ABS(ISNULL(d.stock, 0) - ISNULL(l.stock, 0)) < 1e-6 THEN 1 ELSE 0 END) THEN 1 ELSE 0 END, NULL
    FROM [$(SL)].dbo.StockCorteSucursal l
    LEFT JOIN fusion.Map m ON m.espacio = 'corte' AND m.idSL = l.idCorte
    LEFT JOIN dbo.StockCorteSucursal d ON d.idSucursal = 1 AND d.idCorte = COALESCE(m.idFinal, l.idCorte)
    WHERE l.idSucursal = 1 AND EXISTS (SELECT 1 FROM [$(SL)].dbo.Corte cx WHERE cx.idCorte = l.idCorte);

    -- informativo: configuracion por sucursal de SL de productos que no existen en SL (no se cargan)
    INSERT INTO fusion.Verif (chequeo, tabla, esperado, obtenido, ok, nota)
    SELECT 'info huerfana SL', 'StockCorteSucursal', NULL, COUNT(*), NULL, N'filas de SL de productos inexistentes en SL (no se cargan)'
    FROM [$(SL)].dbo.StockCorteSucursal l WHERE NOT EXISTS (SELECT 1 FROM [$(SL)].dbo.Corte cx WHERE cx.idCorte = l.idCorte) HAVING COUNT(*) > 0;
    INSERT INTO fusion.Verif (chequeo, tabla, esperado, obtenido, ok, nota)
    SELECT 'info huerfana SL', 'CortePuntoStockSucursal', NULL, COUNT(*), NULL, N'filas de SL de productos inexistentes en SL (no se cargan)'
    FROM [$(SL)].dbo.CortePuntoStockSucursal l WHERE NOT EXISTS (SELECT 1 FROM [$(SL)].dbo.Corte cx WHERE cx.idCorte = l.idCorte) HAVING COUNT(*) > 0;
END

-- informativo: filas de catalogos (estrategia sm) que en SL son distintas o no estan en SM
DECLARE @t sysname, @sql nvarchar(max), @n float, @cols nvarchar(max), @pkc nvarchar(max);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT r.tabla FROM fusion.ReglaTabla r WHERE r.estrategia = 'sm' AND EXISTS (SELECT 1 FROM [$(SL)].sys.tables st WHERE st.name = r.tabla)
      AND r.tabla <> 'sysdiagrams';
OPEN cur; FETCH NEXT FROM cur INTO @t;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @cols = STUFF((SELECT N', ' + QUOTENAME(c.name) FROM sys.columns c
                          WHERE c.object_id = OBJECT_ID(N'dbo.' + QUOTENAME(@t)) AND c.name NOT IN ('idEmpresa')
                            AND TYPE_NAME(c.system_type_id) NOT IN ('text', 'ntext', 'image', 'xml', 'timestamp')
                            AND EXISTS (SELECT 1 FROM [$(SL)].sys.tables st JOIN [$(SL)].sys.columns sc ON sc.object_id = st.object_id WHERE st.name = @t AND sc.name = c.name)
                          ORDER BY c.column_id FOR XML PATH('')), 1, 2, '');
    SET @sql = N'SELECT @n = COUNT(*) FROM (SELECT ' + @cols + N' FROM [$(SL)].dbo.' + QUOTENAME(@t) + N' EXCEPT SELECT ' + @cols + N' FROM dbo.' + QUOTENAME(@t) + N') x';
    EXEC sp_executesql @sql, N'@n float OUTPUT', @n OUTPUT;
    IF @n > 0 INSERT INTO fusion.Verif (chequeo, tabla, esperado, obtenido, ok, nota) VALUES ('info catalogo', @t, 0, @n, NULL, N'filas de SL distintas o ausentes en SM (no se cargan)');
    FETCH NEXT FROM cur INTO @t;
END
CLOSE cur; DEALLOCATE cur;
INSERT INTO fusion.Verif (chequeo, tabla, esperado, obtenido, ok, nota)
SELECT 'info ignorada', r.tabla, NULL, NULL, NULL, r.notas FROM fusion.ReglaTabla r WHERE r.estrategia = 'ignorar';
INSERT INTO fusion.Verif (chequeo, tabla, esperado, obtenido, ok, nota)
SELECT 'info fk', tabla, NULL, NULL, NULL, LEFT(detalle, 400) FROM fusion.Log WHERE paso = 'FK sin validar';
GO
------------------------------------------------------------------------------------------------
-- resultado
------------------------------------------------------------------------------------------------
SELECT chequeo, COUNT(*) total, SUM(CASE WHEN ok = 1 THEN 1 ELSE 0 END) ok, SUM(CASE WHEN ok = 0 THEN 1 ELSE 0 END) falla
FROM fusion.Verif GROUP BY chequeo ORDER BY chequeo;
SELECT 'FALLA' estado, chequeo, tabla, esperado, obtenido, nota FROM fusion.Verif WHERE ok = 0 ORDER BY id;
SELECT 'INFO' estado, chequeo, tabla, obtenido, nota FROM fusion.Verif WHERE ok IS NULL ORDER BY id;
IF EXISTS (SELECT 1 FROM fusion.Verif WHERE ok = 0) RAISERROR('Verificacion: hay chequeos en FALLA (ver arriba)', 16, 1);
ELSE SELECT 'Verificacion: todos los chequeos OK' resultado;
GO
