using System;
using System.Data;
using Npgsql;

namespace DatosPostgres
{
    // Fuentes de datos para el modulo "Actividades" (solo admin, POS-only Postgres -- 2026-09-07,
    // pedido explicito del usuario, ver docs/DECISIONS.md). Reune eventos heterogeneos (cambios de
    // precio, ventas con lineas anuladas o bonificacion manual, movimientos, compras, formulas de
    // elaborados) en queries de SOLO LECTURA -- no pasa por Negocio/ porque no hay logica de
    // negocio transaccional aca, es puro reporting cross-dominio (decision documentada en
    // docs/DECISIONS.md). Cada metodo devuelve un DataTable crudo; el mapeo a texto legible
    // ("se modifico el precio de X...") lo arma WebCore.Controllers.ActividadesController.
    // 2026-09-10 (Batch 9 de la quinta ronda, ver docs/DECISIONS.md): las 4 queries con fecha de
    // negocio propia (ventas anuladas/bonificadas, movimientos, compras) devuelven fecha_negocio/
    // creado/actualizado por separado en vez de un unico COALESCE ya resuelto -- el WHERE/ORDER BY
    // sigue filtrando y ordenando por el COALESCE (mismo comportamiento de rango que antes), pero
    // el SELECT ya no lo colapsa, para que el service pueda calcular el origen ("creación"/
    // "modificación") y detectar fechas anomalas (>1 dia de diferencia con la fecha de negocio).
    public class ActividadPg
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public ActividadPg(string connectionString, int idEmpresa)
        {
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        // Cambios de precio de Corte, con precio anterior calculado via LAG() sobre TODO el
        // historial (no solo el rango filtrado, para que la primera fila del rango tenga bien su
        // "anterior" real) -- filtra recien despues filas sin cambio real de precio. LIMITACION
        // CONOCIDA (ver docs/GAPS.md): actualizacioncorte solo se llena desde la edicion COMPLETA
        // del producto (addOrEditCorte), no desde la edicion rapida de precio (editPrecioCorte) --
        // esta ultima no deja rastro hoy, asi que esos cambios no van a aparecer aca.
        public DataTable ObtenerCambiosPrecio(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                WITH historial AS (
                    SELECT idcorte, corte, codigo, preciokg,
                           LAG(preciokg) OVER (PARTITION BY idcorte ORDER BY actualizado) AS preciokg_anterior,
                           actualizado AS fecha
                    FROM actualizacioncorte
                )
                SELECT corte, codigo, preciokg AS precio_nuevo, preciokg_anterior AS precio_anterior, fecha
                FROM historial
                WHERE fecha BETWEEN @desde AND @hasta
                  AND preciokg_anterior IS NOT NULL
                  AND preciokg_anterior <> preciokg
                ORDER BY fecha DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("desde", desde);
                p.AddWithValue("hasta", hasta);
            });
        }

        // Ventas que tienen al menos una linea anulada (cantkg<0, mismo criterio que el resto del
        // sistema -- ver docs/DECISIONS.md 2026-09-07, historial de precios). Fecha del evento =
        // COALESCE(actualizado,creado,fechaventa): lineaventa no tiene timestamp propio, asi que
        // se aproxima con el de la venta (LIMITACION CONOCIDA: si la anulacion ocurrio en una
        // edicion posterior, "actualizado" de la venta si la refleja; si la venta nunca se
        // reescribio via "modificar venta", cae en fechaventa/creado). Las 3 columnas (fecha de
        // negocio, creado, actualizado) se devuelven CRUDAS por separado (2026-09-10, Batch 9 de
        // la quinta ronda, ver docs/DECISIONS.md) -- el calculo de cual mostrar y la deteccion de
        // fecha anomala (>1 dia de diferencia) se movieron de SQL a ActividadesFeedService.cs.
        public DataTable ObtenerVentasConLineasAnuladas(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT DISTINCT v.idventa, v.fechaventa AS fecha_negocio, v.creado AS creado,
                       v.actualizado AS actualizado, u.nombre AS vendedor,
                       COALESCE(v.actualizado, v.creado, v.fechaventa) AS orden_fecha
                FROM lineaventa lv
                INNER JOIN ventas v ON v.idventa = lv.idventa
                LEFT JOIN usuarios u ON u.id = v.idvendedor
                WHERE lv.cantkg < 0
                  AND COALESCE(v.actualizado, v.creado, v.fechaventa) BETWEEN @desde AND @hasta
                ORDER BY orden_fecha DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("desde", desde);
                p.AddWithValue("hasta", hasta);
            });
        }

        // Ventas con al menos una linea bonificada/recargada manualmente por el cajero (precio
        // modificado a mano o % aplicado -- ambos casos quedan en lineaventa.bonificacion<>0, no
        // se puede distinguir el origen exacto desde el dato guardado). Excluye lineas de
        // anulacion (cantkg<0) para no duplicar con el reporte anterior.
        public DataTable ObtenerVentasConBonificacionManual(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT DISTINCT v.idventa, v.fechaventa AS fecha_negocio, v.creado AS creado,
                       v.actualizado AS actualizado, u.nombre AS vendedor,
                       COALESCE(v.actualizado, v.creado, v.fechaventa) AS orden_fecha
                FROM lineaventa lv
                INNER JOIN ventas v ON v.idventa = lv.idventa
                LEFT JOIN usuarios u ON u.id = v.idvendedor
                WHERE lv.cantkg > 0 AND lv.bonificacion <> 0
                  AND COALESCE(v.actualizado, v.creado, v.fechaventa) BETWEEN @desde AND @hasta
                ORDER BY orden_fecha DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("desde", desde);
                p.AddWithValue("hasta", hasta);
            });
        }

        // Movimientos de stock entre sucursales (alta o edicion -- se ordena por lo que haya
        // ocurrido ultimo, igual criterio que el resto de este archivo).
        public DataTable ObtenerMovimientos(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT m.idmovimiento, m.fechamovimiento AS fecha_negocio, m.creado AS creado,
                       m.actualizado AS actualizado,
                       so.sucursal AS sucursal_origen, sd.sucursal AS sucursal_destino,
                       COALESCE(ap.nombre, cp.nombre) AS usuario
                FROM movimiento m
                LEFT JOIN sucursal so ON so.idsucursal = m.sucursalorigen
                LEFT JOIN sucursal sd ON sd.idsucursal = m.sucursaldestino
                LEFT JOIN usuarios cp ON cp.id = m.creadopor
                LEFT JOIN usuarios ap ON ap.id = m.actualizadopor
                WHERE COALESCE(m.actualizado, m.creado, m.fechamovimiento) BETWEEN @desde AND @hasta
                ORDER BY COALESCE(m.actualizado, m.creado, m.fechamovimiento) DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("desde", desde);
                p.AddWithValue("hasta", hasta);
            });
        }

        // Compras/registros de stock (ingreso, egreso, ajuste, cierre -- todo lo que pasa por la
        // tabla "compras" segun Entidades.Compra.tipoCompraEnum).
        public DataTable ObtenerCompras(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT c.idcompra, c.fechacompra AS fecha_negocio, c.creado AS creado,
                       c.actualizado AS actualizado,
                       c.tipocompra, COALESCE(ap.nombre, cp.nombre) AS usuario
                FROM compras c
                LEFT JOIN usuarios cp ON cp.id = c.creadopor
                LEFT JOIN usuarios ap ON ap.id = c.actualizadopor
                WHERE COALESCE(c.actualizado, c.creado, c.fechacompra) BETWEEN @desde AND @hasta
                ORDER BY COALESCE(c.actualizado, c.creado, c.fechacompra) DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("desde", desde);
                p.AddWithValue("hasta", hasta);
            });
        }

        // Formulas de elaborados/embutidos (alta o edicion de receta).
        public DataTable ObtenerFormulas(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT f.idformula, COALESCE(f.actualizado, f.creado) AS fecha,
                       c.corte AS producto, COALESCE(ap.nombre, cp.nombre) AS usuario
                FROM formulas f
                LEFT JOIN corte c ON c.idcorte = f.idembutido
                LEFT JOIN usuarios cp ON cp.id = f.creadopor
                LEFT JOIN usuarios ap ON ap.id = f.actualizadopor
                WHERE COALESCE(f.actualizado, f.creado) BETWEEN @desde AND @hasta
                ORDER BY fecha DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("desde", desde);
                p.AddWithValue("hasta", hasta);
            });
        }
    }
}
