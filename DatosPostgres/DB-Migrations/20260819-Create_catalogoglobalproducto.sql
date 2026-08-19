-- Etapa: CatalogoGlobalProducto.cs (ultimo par de modulos chicos 0%-migrados, junto con
-- WhatsApp.cs). Catalogo global de productos, separado de dbo.Corte (ver
-- 20260804-Create_CatalogoGlobalProducto.sql en SQL Server). 4 metodos publicos:
-- findCorteGlobalByCodigo, ObtenerCatalogoGlobalPagina, ObtenerTiposCatalogoGlobal,
-- ObtenerCatalogoGlobalPorIds.
--
-- Catalogo global puro: SIN idempresa, SIN RLS -- mismo criterio que formularios/alicuotasiva.
-- idcorte NO es identity en SQL Server (valores fijos preasignados, verificado con
-- sys.identity_columns) -- se replica igual, sin identity en Postgres tampoco (a diferencia
-- del patron identity-por-defecto de esta migracion, acá no aplica: los ids ya vienen fijos
-- desde el catalogo origen y no hay alta nueva vía la app).
-- 101.943 filas reales -- la tabla mas grande migrada en esta ronda de modulos chicos.
CREATE TABLE catalogoglobalproducto (
    idcorte integer PRIMARY KEY,
    codigo bigint,
    corte text,
    tipo text,
    idcortemaestro integer,
    porcentaje double precision,
    preciokg double precision,
    porcentajehueso double precision,
    independiente integer,
    desvioestandar double precision,
    creado timestamp,
    actualizado timestamp,
    mayorista boolean,
    encierrestock boolean,
    promedio double precision,
    habilitado boolean,
    idalicuotaiva integer,
    alicuotaiva double precision,
    ingresorapidoembutido boolean,
    nivel integer,
    pesable boolean,
    puntostock integer,
    idmarca integer
);

CREATE INDEX ix_catalogoglobalproducto_codigo ON catalogoglobalproducto(codigo);
CREATE INDEX ix_catalogoglobalproducto_idcortemaestro ON catalogoglobalproducto(idcortemaestro);

GRANT SELECT ON catalogoglobalproducto TO carnisys_user, cs_admin_pg;
