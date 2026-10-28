-- Volcado de SuperCerdo unificada (ya copiada a carnisys.stg por tools/FusionSuperCerdoPg) a la empresa 1 de carnisys.
-- Una sola transaccion como superusuario (bypasea RLS) con las FKs diferidas. Uso (psql, conectado a carnisys):
--   psql -v finish=ROLLBACK -f 05-volcar-a-carnisys.sql   (ensayo)  |  psql -v finish=COMMIT -f ...  (real)
-- Reglas decididas con el usuario el 2026-10-27 (docs/DECISIONS.md "Unificacion SuperCerdo SM + SL"):
--   * Se borra TODO lo de la empresa 1 salvo los certificados (titulares fiscales y claves de certificado ARCA),
--     y se carga SuperCerdo con sus IDs intactos.
--   * Las filas de otras empresas / globales cuyo ID choca con SuperCerdo se renumeran, con todas sus referencias.
--   * Catalogos globales (alicuotasiva, formularios, iva, parametros, catalogoglobalproducto) quedan los de carnisys;
--     empresaparametros se mapea por nombre de parametro. Filas globales de sistema (usuario admin id 0, tipos de
--     egreso de empresa 0) se conservan y se omite su duplicado de SuperCerdo.
--   * sucursal.idtitularfacturacion (vinculo con el titular fiscal) se conserva de la empresa 1 anterior.
--   * german queda superadmin (pedido del usuario del 2026-09-21).
\set ON_ERROR_STOP on
\pset pager off
begin;
set local session_replication_role = replica;

-- tablas de certificados de la empresa 1 que NO se borran
create temp table keep_tablas(tabla text primary key);
insert into keep_tablas values ('titulares_fiscales'), ('titulares_fiscales_clave'), ('titulares_fiscales_ptovta'),
    ('titulares_fiscales_sucursal'), ('certificado_arca_clave');

-- 0) precondiciones: stg solo trae la empresa 1
do $$
declare r record; n bigint;
begin
  for r in select table_name from information_schema.columns where table_schema = 'stg' and column_name = 'idempresa' loop
    execute format('select count(*) from stg.%I where idempresa <> 1', r.table_name) into n;
    if n > 0 then raise exception 'stg.% trae % filas de otra empresa', r.table_name, n; end if;
  end loop;
end $$;

-- Mapa de referencias de carnisys: espacio de IDs (tabla padre) -> columnas que lo referencian.
-- cond: condicion sobre la fila hija (alias t) para columnas polimorficas.
create temp table space_pk(space text primary key, pk text not null);
insert into space_pk values ('usuarios','id'), ('personas','idpersona'), ('ventas','idventa'), ('compras','idcompra'), ('pagos','id'),
  ('corte','idcorte'), ('cierrecaja','id'), ('movimiento','idmovimiento'), ('embutidos','idembutido'), ('formulas','idformula'),
  ('expendios','idexpendio'), ('facturaelectronica','id'), ('lineaventa','idlineaventa'), ('dispositivosseguros','id'),
  ('auditoriaeliminaciones','id'), ('borradorgenerico','id'), ('ventaborrador','id'), ('tiposegresocaja','id'), ('pedido','idpedido'),
  ('sucursal','idsucursal'), ('empleado','idempleado'), ('registrojornada','idregistrojornada'), ('liquidacionsueldo','idliquidacionsueldo');
create temp table ref_map(space text, ref_table text, ref_col text, cond text);
insert into ref_map values
 -- usuarios
 ('usuarios','ventas','idvendedor',null), ('usuarios','expendios','idvendedor',null), ('usuarios','temporallineaventa','idvendedor',null),
 ('usuarios','permisosusuarios','idusuario',null), ('usuarios','usuariopasswordresettokens','idusuario',null), ('usuarios','loginubicacionlog','idusuario',null),
 ('usuarios','dispositivosseguros','idusuariocreador',null), ('usuarios','dispositivosseguros_solicitudes','idusuario',null),
 ('usuarios','dispositivosseguros_solicitudes','resueltapor',null), ('usuarios','formatoscodigobarras','idusuariocreador',null),
 ('usuarios','formatoscodigobarras','idusuariomodificador',null), ('usuarios','catalogoglobalimportacionproductos','idusuarioalta',null),
 ('usuarios','auditoriacambiosucursalcaja','idusuarioejecutor',null), ('usuarios','auditoriacambiosucursalcaja','idusuariocaja',null),
 ('usuarios','auditoriacierrecaja','idusuario',null), ('usuarios','auditoriaeliminaciones','idusuariocreador',null),
 ('usuarios','auditoriaeliminaciones','idusuarioelimino',null), ('usuarios','auditoriaeliminaciones','idusuariosesion',null),
 ('usuarios','auditoriaformulas','idusuario',null), ('usuarios','auditoriaformulas','idusuariosesion',null), ('usuarios','auditoriapagos','idusuario',null),
 ('usuarios','borradorgenerico','idoperador',null), ('usuarios','borradorgenerico','idusuariosesion',null), ('usuarios','borradorgenericoevento','idusuario',null),
 ('usuarios','ventaborrador','idoperador',null), ('usuarios','ventaborrador','idusuariosesion',null), ('usuarios','ventaborradorevento','idusuario',null),
 ('usuarios','ventaproductosinagregar','idoperador',null), ('usuarios','ventaproductosinagregar','idusuariosesion',null),
 ('usuarios','ventaproductosinagregar','revisadapor',null), ('usuarios','empleado','idusuario',null), ('usuarios','pedido','idusuario',null),
 ('usuarios','pedido','modificadopor',null), ('usuarios','pedidoevento','idusuario',null), ('usuarios','pedidoevento','idautorizador',null),
 ('usuarios','notificaciones','atendidapor',null), ('usuarios','registrojornada','registradopor',null),
 ('usuarios','registrojornadahistorial','modificadopor',null), ('usuarios','usuariopasskeys','idusuario',null),
 ('usuarios','titulares_fiscales','revocado_por',null), ('usuarios','certificado_arca_clave','actualizado_por',null),
 ('usuarios','plataforma_certificado_arca','actualizado_por',null), ('usuarios','pagos','eliminadopor',null),
 -- personas
 ('personas','ventas','idpersona',null), ('personas','compras','idproveedor',null), ('personas','corteproveedor','idproveedor',null),
 ('personas','movctacte','idpersona',null), ('personas','pagos','idpersona',null), ('personas','personas','idpropietario',null),
 ('personas','cheques','recibidode',null), ('personas','cheques','entregadoa',null), ('personas','expendios','idpersona',null),
 ('personas','auditoriaeliminaciones','idpersona',null), ('personas','auditoriapagos','idpersonaanterior',null),
 ('personas','auditoriapagos','idpersonanueva',null), ('personas','ventaborrador','idpersona',null), ('personas','empleado','idpersona',null),
 ('personas','titulares_fiscales','idpersona',null),
 -- ventas
 ('ventas','lineaventa','idventa',null), ('ventas','facturaelectronica','idventa',null), ('ventas','expendios','idventa',null),
 ('ventas','ventaborrador','idventa',null), ('ventas','movctacte','idtabla','t.tabla = ''Ventas'''), ('ventas','egresoscaja','idtabla','t.tabla = ''Ventas'''),
 -- compras (los ingresos de stock tambien son compras)
 ('compras','corteporcompra','idcompra',null), ('compras','mediares','idcompra',null), ('compras','egresoscaja','idcompra',null),
 ('compras','movctacte','idtabla','t.tabla = ''Compras'''), ('compras','egresoscaja','idtabla','t.tabla = ''Compras'''), ('compras','compras','idpesajeajustado',null),
 ('compras','borradorgenerico','idregistro','t.modulo in (''COMPRA'',''STOCK'')'), ('compras','borradorgenerico','idresultado','t.modulo in (''COMPRA'',''STOCK'')'),
 ('compras','auditoriaeliminaciones','idregistro','t.modulo in (''COMPRA'',''STOCK'')'),
 -- pagos
 ('pagos','movctacte','idtabla','t.tabla = ''Pagos'''), ('pagos','egresoscaja','idtabla','t.tabla = ''Pagos'''), ('pagos','auditoriapagos','idpago',null),
 -- productos
 ('corte','lineaventa','idcorte',null), ('corte','corteporcompra','idcorte',null), ('corte','corteproveedor','idcorte',null),
 ('corte','cortepuntostocksucursal','idcorte',null), ('corte','cortejerarquiasucursal','idcorte',null), ('corte','embutidos','idcorte',null),
 ('corte','corteporembutido','idcorte',null), ('corte','corteporformula','idcorte',null), ('corte','cortepormovimiento','idcorte',null),
 ('corte','lineaexpendio','idcorte',null), ('corte','temporallineaventa','idcorte',null), ('corte','movimientohistorial','idcorte',null),
 ('corte','corte','idcortemaestro',null), ('corte','actualizacioncorte','idcorte',null), ('corte','actualizacioncorte','idcortemaestro',null),
 ('corte','catalogoglobalimportacionproductos','idproductoempresa',null), ('corte','auditoriaeliminacionlineas','idcorte',null),
 ('corte','auditoriaformulas','idcorte',null), ('corte','pedidolinea','idcorte',null),
 -- resto
 ('cierrecaja','auditoriacambiosucursalcaja','idcierrecaja',null), ('cierrecaja','auditoriacierrecaja','idcierrecaja',null), ('cierrecaja','ventaborrador','idcierrecaja',null),
 ('movimiento','cortepormovimiento','idmovimientos',null), ('movimiento','movimientohistorial','idmovimiento',null), ('movimiento','movimiento','idmovorigen',null),
 ('movimiento','borradorgenerico','idregistro','t.modulo = ''MOVIMIENTO'''), ('movimiento','borradorgenerico','idresultado','t.modulo = ''MOVIMIENTO'''),
 ('movimiento','auditoriaeliminaciones','idregistro','t.modulo = ''MOVIMIENTO'''),
 ('embutidos','corteporembutido','idembutido',null), ('embutidos','formulas','idembutido',null),
 ('embutidos','borradorgenerico','idregistro','t.modulo like ''EMBUTIDO%'''), ('embutidos','borradorgenerico','idresultado','t.modulo like ''EMBUTIDO%'''),
 ('pedido','borradorgenerico','idregistro','t.modulo = ''PEDIDO_STOCK'''), ('pedido','borradorgenerico','idresultado','t.modulo = ''PEDIDO_STOCK'''),
 ('pedido','pedidoevento','idpedido',null), ('pedido','pedidolinea','idpedido',null),
 ('formulas','corteporformula','idformula',null), ('expendios','lineaexpendio','idexpendio',null),
 ('facturaelectronica','alicuotaivaporfactura','idfacturaelectronica',null), ('lineaventa','lineaventa','idlineaventaanulado',null),
 ('dispositivosseguros','loginubicacionlog','iddispositivoseguro',null), ('dispositivosseguros','registrojornada','iddispositivoseguro',null),
 ('auditoriaeliminaciones','auditoriaeliminacionlineas','idauditoria',null), ('borradorgenerico','borradorgenericoevento','idborrador',null),
 ('ventaborrador','ventaborradorevento','idborrador',null), ('tiposegresocaja','egresoscaja','idtipoegresocaja',null),
 ('empleado','empleadotarifa','idempleado',null), ('empleado','empleadovacacion','idempleado',null), ('empleado','liquidacionsueldo','idempleado',null),
 ('empleado','registrojornada','idempleado',null), ('empleado','registrojornadahistorial','idempleado',null),
 ('registrojornada','registrojornadahistorial','idregistrojornada',null), ('liquidacionsueldo','liquidacionsueldodetalle','idliquidacionsueldo',null);
-- creadopor / actualizadopor de cualquier tabla son IDs de usuario
insert into ref_map select 'usuarios', table_name, column_name, null from information_schema.columns c
 where table_schema = 'public' and column_name in ('creadopor', 'actualizadopor') and data_type = 'integer'
   and exists (select 1 from information_schema.tables t where t.table_schema = 'public' and t.table_name = c.table_name and t.table_type = 'BASE TABLE');
-- sucursales: tablas con idsucursal (y variantes) apuntan a sucursal
insert into ref_map select 'sucursal', table_name, column_name, null from information_schema.columns c
 where table_schema = 'public' and data_type = 'integer' and table_name <> 'sucursal'
   and column_name in ('idsucursal', 'idsucursaluser', 'sucursalorigen', 'sucursaldestino', 'idsucorigen', 'idsucdestino', 'idsucursalanterior', 'idsucursalnueva', 'idsucursaldestino')
   and exists (select 1 from information_schema.tables t where t.table_schema = 'public' and t.table_name = c.table_name and t.table_type = 'BASE TABLE');

-- Tablas cuya PK nadie referencia: si chocan se renumeran solo ellas. Una colision en una tabla que no este aca ni
-- en space_pk aborta (faltaria definir sus referencias).
create temp table sin_refs(tabla text primary key);
insert into sin_refs values ('alicuotaivaporfactura'), ('corteporcompra'), ('corteporembutido'), ('corteporformula'), ('cortepormovimiento'),
  ('corteproveedor'), ('cortepuntostocksucursal'), ('cortejerarquiasucursal'), ('egresoscaja'), ('movctacte'), ('mediares'), ('lineaexpendio'),
  ('temporallineaventa'), ('cheques'), ('usuariopasswordresettokens'), ('catalogoglobalimportacionproductos'), ('dispositivosseguros_solicitudes'),
  ('auditoriapagos'), ('auditoriaformulas'), ('auditoriaeliminacionlineas'), ('borradorgenericoevento'), ('ventaborradorevento'), ('formatoscodigobarras');

-- 1) metricas de las OTRAS empresas antes (filas, sumas y referencias resueltas) para comparar al final
create temp table metrics(phase text, k text, n numeric);
create function pg_temp.collect(p_phase text) returns void language plpgsql as $f$
declare rf record; t text; c text; hasemp boolean; sp text; parentemp boolean;
begin
  for rf in select * from ref_map loop
    select pk into sp from space_pk where space = rf.space;
    select exists(select 1 from information_schema.columns where table_schema = 'public' and table_name = rf.ref_table and column_name = 'idempresa') into hasemp;
    select exists(select 1 from information_schema.columns where table_schema = 'public' and table_name = rf.space and column_name = 'idempresa') into parentemp;
    if not hasemp then continue; end if;  -- tablas globales (loginubicacionlog, certificados de plataforma): sin empresa
    execute format('insert into metrics select %L, %L, count(*) from public.%I t where t.idempresa <> 1%s and exists (select 1 from public.%I p where p.%I = t.%I%s)',
      p_phase, 'ref ' || rf.space || ' <- ' || rf.ref_table || '.' || rf.ref_col || coalesce(' [' || rf.cond || ']', ''), rf.ref_table,
      coalesce(' and (' || rf.cond || ')', ''), rf.space, sp, rf.ref_col, case when parentemp then ' and p.idempresa = t.idempresa' else '' end);
  end loop;
  for t in select table_name from information_schema.columns where table_schema = 'public' and column_name = 'idempresa'
             and table_name in (select table_name from information_schema.tables where table_schema = 'public' and table_type = 'BASE TABLE') loop
    execute format('insert into metrics select %L, %L, count(*) from public.%I where idempresa <> 1', p_phase, 'rows ' || t, t);
    for c in select column_name from information_schema.columns where table_schema = 'public' and table_name = t and data_type in ('double precision', 'numeric') loop
      execute format('insert into metrics select %L, %L, coalesce(round(sum(%I)::numeric, 4), 0) from public.%I where idempresa <> 1', p_phase, 'sum ' || t || '.' || c, c, t);
    end loop;
  end loop;
end $f$;
create temp table emp1_sucs as select idsucursal from public.sucursal where idempresa = 1;
create temp table emp1_users as select id from public.usuarios where idempresa = 1;
-- personas que referencian los titulares fiscales de la empresa 1 (certificados): se conservan si SuperCerdo no
-- tiene ese ID; si lo tiene con otro CUIT, se aborta (el titular quedaria apuntando a otra persona).
create temp table keep_personas as select p.idpersona from public.personas p
  where p.idempresa = 1 and p.idpersona in (select idpersona from public.titulares_fiscales where idempresa = 1 and idpersona is not null)
    and not exists (select 1 from stg.personas s where s.idpersona = p.idpersona);
do $$ begin
  if exists (select 1 from public.titulares_fiscales t join stg.personas s on s.idpersona = t.idpersona
             join public.personas p on p.idpersona = t.idpersona and p.idempresa = 1
             where t.idempresa = 1 and regexp_replace(coalesce(s.cuit, ''), '[^0-9]', '', 'g') <> regexp_replace(coalesce(p.cuit, ''), '[^0-9]', '', 'g')) then
    raise exception 'un titular fiscal de la empresa 1 apunta a un ID de persona que en SuperCerdo es otra persona (otro CUIT)';
  end if; end $$;
select pg_temp.collect('pre');
insert into metrics select 'pre', 'rows loginubicacionlog (otras empresas)', count(*) from public.loginubicacionlog where idsucursal not in (select idsucursal from emp1_sucs) and idusuario not in (select id from emp1_users);
do $$ declare k record; n bigint; begin
  for k in select tabla from keep_tablas loop
    execute format('select count(*) from public.%I where idempresa = 1', k.tabla) into n;
    insert into metrics values ('pre', 'keep ' || k.tabla, n);
  end loop; end $$;

-- vinculo sucursal -> titular fiscal de la empresa 1 (es configuracion de certificados: se conserva)
create temp table keep_suc_titular as select idsucursal, idtitularfacturacion from public.sucursal where idempresa = 1 and idtitularfacturacion is not null;

-- 2) exclusiones del snapshot: filas globales de sistema que carnisys ya tiene
delete from stg.permisosusuarios where idusuario = 0;
delete from stg.usuarios where id = 0;
delete from stg.tiposegresocaja where id in (select id from public.tiposegresocaja where idempresa = 0);
update stg.usuarios set superadmin = true where lower(usuario) = 'german';

-- 3) borrar la empresa 1 (todo menos los certificados)
-- logins de la empresa 1: por sucursal o por usuario (hay logins con sucursal 0)
delete from public.loginubicacionlog where idsucursal in (select idsucursal from emp1_sucs) or idusuario in (select id from emp1_users);
do $$
declare r record; n bigint;
begin
  for r in select c.table_name from information_schema.columns c join information_schema.tables t on t.table_schema = c.table_schema and t.table_name = c.table_name and t.table_type = 'BASE TABLE'
           where c.table_schema = 'public' and c.column_name = 'idempresa' and c.table_name not in (select tabla from keep_tablas) order by 1 loop
    execute format('delete from public.%I where idempresa = 1%s', r.table_name,
                   case when r.table_name = 'personas' then ' and idpersona not in (select idpersona from keep_personas)' else '' end);
    get diagnostics n = row_count;
    if n > 0 then raise notice 'borrado empresa 1: % = % filas', r.table_name, n; end if;
  end loop;
end $$;

-- 4) renumerar las filas que quedan (otras empresas / globales) cuyo ID choca con SuperCerdo
do $$
declare r record; rf record; base bigint; n bigint; nref bigint; hasemp boolean; filtro text;
begin
  for r in select (select relname from pg_class where oid = c.conrelid) tbl, a.attname pk
           from pg_constraint c join pg_attribute a on a.attrelid = c.conrelid and a.attnum = c.conkey[1]
           where c.contype = 'p' and c.connamespace = 'public'::regnamespace and array_length(c.conkey, 1) = 1 and a.atttypid = 'int4'::regtype
             and (select relname from pg_class where oid = c.conrelid) not in ('loginubicacionlog', 'parametros', 'formularios', 'iva', 'alicuotasiva', 'catalogoglobalproducto')
             and exists (select 1 from information_schema.tables where table_schema = 'stg' and table_name = (select relname from pg_class where oid = c.conrelid))
           order by 1 loop
    execute format('select count(*) from public.%I p join stg.%I s on s.%I = p.%I', r.tbl, r.tbl, r.pk, r.pk) into n;
    if n = 0 then continue; end if;
    if r.tbl not in (select space from space_pk) and r.tbl not in (select tabla from sin_refs) then
      raise exception 'colision de IDs en % (% ids) y no hay referencias definidas para esa tabla', r.tbl, n;
    end if;
    execute format('select greatest(coalesce((select max(%I) from public.%I), 0), coalesce((select max(%I) from stg.%I), 0)) + 1', r.pk, r.tbl, r.pk, r.tbl) into base;
    execute format('create temp table map_%s on commit drop as select p.%I as old_id, (%s + row_number() over (order by p.%I))::int as new_id from public.%I p join stg.%I s on s.%I = p.%I',
      r.tbl, r.pk, base, r.pk, r.tbl, r.tbl, r.pk, r.pk);
    execute format('update public.%I t set %I = m.new_id from map_%s m where t.%I = m.old_id', r.tbl, r.pk, r.tbl, r.pk);
    raise notice 'renumerado %.%: % ids (nuevos desde %)', r.tbl, r.pk, n, base;
    -- referencias de las filas que no son de la empresa 1 (las de la empresa 1 que quedan -certificados- apuntan a SuperCerdo)
    for rf in select * from ref_map where space = r.tbl loop
      if not exists (select 1 from information_schema.tables where table_schema = 'public' and table_name = rf.ref_table) then continue; end if;
      select exists(select 1 from information_schema.columns where table_schema = 'public' and table_name = rf.ref_table and column_name = 'idempresa') into hasemp;
      filtro := case when hasemp then ' and t.idempresa <> 1'
                     when rf.ref_table = 'loginubicacionlog' then ' and t.idsucursal not in (select idsucursal from emp1_sucs) and t.idusuario not in (select id from emp1_users)' else '' end;
      execute format('update public.%I t set %I = m.new_id from map_%s m where t.%I = m.old_id%s%s',
        rf.ref_table, rf.ref_col, r.tbl, rf.ref_col, filtro, coalesce(' and (' || rf.cond || ')', ''));
      get diagnostics nref = row_count;
      if nref > 0 then raise notice '   ref %.%: % filas', rf.ref_table, rf.ref_col, nref; end if;
    end loop;
    if r.tbl = 'personas' then
      update public.empresaparametros ep set valor = m.new_id::text from map_personas m, public.parametros p
       where p.idparametro = ep.idparametro and p.nombre in ('idConsumidorFinal', 'idIndefinido') and ep.valor = m.old_id::text and ep.idempresa <> 1;
      get diagnostics nref = row_count; raise notice '   ref empresaparametros (idConsumidorFinal/idIndefinido): % filas', nref;
    end if;
  end loop;
end $$;

-- 5) cargar SuperCerdo en la empresa 1
do $$
declare r record; cols text; n bigint;
begin
  for r in select c.table_name from information_schema.columns c
           where c.table_schema = 'stg' and c.column_name = 'idempresa' and c.table_name <> 'empresaparametros'
             and exists (select 1 from information_schema.columns pc where pc.table_schema = 'public' and pc.table_name = c.table_name and pc.column_name = 'idempresa')
             and c.table_name not in (select tabla from keep_tablas) order by 1 loop
    select string_agg(quote_ident(column_name), ', ' order by ordinal_position) into cols
      from information_schema.columns where table_schema = 'public' and table_name = r.table_name;
    execute format('insert into public.%I (%s) overriding system value select %s from stg.%I', r.table_name, cols, cols, r.table_name);
    get diagnostics n = row_count;
    if n > 0 then raise notice 'cargado: % = % filas', r.table_name, n; end if;
  end loop;
end $$;
-- empresaparametros: por NOMBRE de parametro (los IDs del catalogo de parametros difieren entre los motores)
insert into public.empresaparametros (idempresa, idparametro, valor)
  select 1, p.idparametro, e.valor from stg.empresaparametros e
  join stg.parametros sp on sp.idparametro = e.idparametro join public.parametros p on p.nombre = sp.nombre;
\echo '== Parametros de SuperCerdo sin equivalente en carnisys (no se cargan) =='
select sp.nombre, e.valor from stg.empresaparametros e join stg.parametros sp on sp.idparametro = e.idparametro
 where not exists (select 1 from public.parametros p where p.nombre = sp.nombre);
-- loginubicacionlog: sin idempresa; el id se regenera (nada lo referencia)
insert into public.loginubicacionlog (idusuario, idsucursal, fechahora, latitud, longitud, precisionmetros, distanciametros, permitido, motivo, ip, iddispositivoseguro, dispositivo)
  select idusuario, idsucursal, fechahora, latitud, longitud, precisionmetros, distanciametros, permitido, motivo, ip, iddispositivoseguro, dispositivo from stg.loginubicacionlog;
-- vinculo sucursal -> titular fiscal
update public.sucursal s set idtitularfacturacion = k.idtitularfacturacion from keep_suc_titular k
 where s.idempresa = 1 and s.idsucursal = k.idsucursal and s.idtitularfacturacion is null;

-- 6) secuencias al maximo de cada tabla
do $$
declare r record;
begin
  for r in select table_name, column_name, pg_get_serial_sequence('public.' || quote_ident(table_name), column_name) seq
           from information_schema.columns where table_schema = 'public' and (is_identity = 'YES' or column_default like 'nextval%') loop
    if r.seq is not null then
      execute format('select setval(%L, coalesce((select max(%I) from public.%I), 1), (select max(%I) is not null from public.%I))',
        r.seq, r.column_name, r.table_name, r.column_name, r.table_name);
    end if;
  end loop;
end $$;

-- 7) verificaciones
select pg_temp.collect('post');
insert into metrics select 'post', 'rows loginubicacionlog (otras empresas)', count(*) from public.loginubicacionlog where idsucursal not in (select idsucursal from public.sucursal where idempresa = 1) and idusuario not in (select id from public.usuarios where idempresa = 1);
do $$ declare k record; n bigint; begin
  for k in select tabla from keep_tablas loop
    execute format('select count(*) from public.%I where idempresa = 1', k.tabla) into n;
    insert into metrics values ('post', 'keep ' || k.tabla, n);
  end loop; end $$;
\echo '== Otras empresas y certificados: metricas que cambiaron (debe estar vacio) =='
select coalesce(a.k, b.k) metrica, a.n pre, b.n post
  from (select * from metrics where phase = 'pre') a full join (select * from metrics where phase = 'post') b on a.k = b.k
 where a.n is distinct from b.n order by 1;

-- contenido de la empresa 1 = stg (con las exclusiones de sistema ya aplicadas a stg)
create temp table content_check(tbl text, stg_rows bigint, pub_rows bigint, same_hash boolean);
do $$
declare r record; cols text; s bigint; p bigint; hs numeric; hp numeric;
begin
  for r in select c.table_name from information_schema.columns c where c.table_schema = 'stg' and c.column_name = 'idempresa'
             and c.table_name not in ('empresaparametros', 'sucursal') order by 1 loop
    select string_agg(quote_ident(column_name), ', ' order by ordinal_position) into cols from information_schema.columns where table_schema = 'public' and table_name = r.table_name;
    execute format('select count(*), coalesce(sum(hashtextextended(x::text, 0)), 0) from (select %s from stg.%I) x', cols, r.table_name) into s, hs;
    execute format('select count(*), coalesce(sum(hashtextextended(x::text, 0)), 0) from (select %s from public.%I where idempresa = 1%s) x', cols, r.table_name,
      case when r.table_name = 'personas' then ' and idpersona not in (select idpersona from keep_personas)' else '' end) into p, hp;
    insert into content_check values (r.table_name, s, p, hs = hp);
  end loop;
end $$;
\echo '== Empresa 1: tablas que NO coinciden con SuperCerdo (debe estar vacio) =='
select * from content_check where stg_rows <> pub_rows or not same_hash;
\echo '== Empresa 1: referencias que no resuelven dentro de la empresa 1 (huerfanas) =='
do $$
declare rf record; sp text; n bigint; ns bigint; parentemp boolean; hasemp boolean;
begin
  create temp table orphan_report(ref text, en_carnisys bigint, en_supercerdo bigint);
  for rf in select * from ref_map loop
    select pk into sp from space_pk where space = rf.space;
    select exists(select 1 from information_schema.columns where table_schema = 'public' and table_name = rf.ref_table and column_name = 'idempresa') into hasemp;
    select exists(select 1 from information_schema.columns where table_schema = 'public' and table_name = rf.space and column_name = 'idempresa') into parentemp;
    if not hasemp or not exists (select 1 from information_schema.tables where table_schema = 'public' and table_name = rf.ref_table) then continue; end if;
    execute format('select count(*) from public.%I t where t.idempresa = 1 and t.%I > 0%s and not exists (select 1 from public.%I p where p.%I = t.%I%s)',
      rf.ref_table, rf.ref_col, coalesce(' and (' || rf.cond || ')', ''), rf.space, sp, rf.ref_col,
      case when parentemp then ' and p.idempresa in (0, 1)' else '' end) into n;
    ns := 0;
    if exists (select 1 from information_schema.tables where table_schema = 'stg' and table_name = rf.ref_table)
       and exists (select 1 from information_schema.tables where table_schema = 'stg' and table_name = rf.space) then
      execute format('select count(*) from stg.%I t where t.%I > 0%s and not exists (select 1 from stg.%I p where p.%I = t.%I) and not exists (select 1 from public.%I g where g.%I = t.%I%s)',
        rf.ref_table, rf.ref_col, coalesce(' and (' || rf.cond || ')', ''), rf.space, sp, rf.ref_col, rf.space, sp, rf.ref_col,
        case when parentemp then ' and g.idempresa = 0' else '' end) into ns;
    end if;
    if n > 0 or ns > 0 then insert into orphan_report values (rf.ref_table || '.' || rf.ref_col || ' -> ' || rf.space, n, ns); end if;
  end loop;
end $$;
select ref, en_carnisys, en_supercerdo, case when en_carnisys > en_supercerdo then 'NUEVAS' else 'ya venian de origen' end estado from orphan_report order by 1;
\echo '== Resumen =='
select (select count(*) from public.ventas where idempresa = 1) ventas_emp1, (select count(*) from public.lineaventa where idempresa = 1) lineas_emp1,
       (select count(*) from public.personas where idempresa = 1) personas_emp1, (select count(*) from public.usuarios where idempresa = 1) usuarios_emp1,
       (select string_agg(usuario, ', ') from public.usuarios where superadmin) superadmins,
       (select count(*) from public.titulares_fiscales where idempresa = 1) titulares_emp1,
       (select count(*) from public.titulares_fiscales t where t.idempresa = 1 and t.idpersona is not null and not exists (select 1 from public.personas p where p.idpersona = t.idpersona and p.idempresa = 1)) titulares_sin_persona,
       (select count(*) from public.sucursal where idempresa = 1 and idtitularfacturacion is not null) sucursales_con_titular;

:finish;
