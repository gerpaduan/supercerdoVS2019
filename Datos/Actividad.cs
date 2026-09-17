using System;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    // Implementacion SQL Server del modulo "Actividades" (solo admin, ver docs/DECISIONS.md).
    // Espeja DatosPostgres/ActividadPg.cs -- mismo criterio (SQL crudo parametrizado, sin stored
    // procedures, no pasa por Negocio/ porque es reporting de solo lectura sin logica de negocio)
    // y MISMOS alias de columna en cada SELECT, para que WebCore/Services/ActividadesFeedService.cs
    // funcione identico contra los dos motores sin ningun cambio.
    //
    // Filtrado multi-tenant: a diferencia de Postgres (RLS via ConexionPg.AbrirConTenant), SQL
    // Server no tiene una politica de RLS equivalente en estas tablas (solo dbo.Personas la tiene).
    // Se filtra por idEmpresa donde hay un join barato ya disponible (Corte, que tiene idEmpresa
    // directo, para Formulas; Sucursal, que tambien lo tiene, para Movimientos) -- para
    // Ventas/Compras no se fuerza un join nuevo solo para filtrar, mismo criterio que la mayoria de
    // los procs legacy existentes (topologia historica de una empresa por servidor SQL Server).
    //
    // GAP CONOCIDO (ver docs/GAPS.md): SQL Server no tiene historial de precios -- Corte solo
    // guarda el precio vigente y editPrecioCorte no loguea nada (a diferencia de Postgres, que
    // tiene actualizacioncorte). ObtenerCambiosPrecio devuelve un DataTable vacio con las columnas
    // esperadas en vez de lanzar o ser omitido, para que ActividadesFeedService.cs no necesite
    // ninguna rama especial por motor.
    public class Actividad : Contratos.IActividadRepository
    {
        private readonly IEmpresaContext _empresa;

        public Actividad(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
        }

        public DataTable ObtenerCambiosPrecio(DateTime desde, DateTime hasta)
        {
            var dt = new DataTable();
            dt.Columns.Add("corte", typeof(string));
            dt.Columns.Add("codigo", typeof(string));
            dt.Columns.Add("precio_nuevo", typeof(decimal));
            dt.Columns.Add("precio_anterior", typeof(decimal));
            dt.Columns.Add("fecha", typeof(DateTime));
            return dt;
        }

        public DataTable ObtenerVentasConLineasAnuladas(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT DISTINCT v.idVenta, v.fechaVenta AS fecha_negocio, v.creado AS creado,
                       v.actualizado AS actualizado, u.nombre AS vendedor,
                       COALESCE(v.actualizado, v.creado, v.fechaVenta) AS orden_fecha
                FROM dbo.LineaVenta lv
                INNER JOIN dbo.Ventas v ON v.idVenta = lv.idVenta
                LEFT JOIN dbo.Usuarios u ON u.id = v.idVendedor
                WHERE lv.cantKg < 0
                  AND COALESCE(v.actualizado, v.creado, v.fechaVenta) BETWEEN @desde AND @hasta
                ORDER BY orden_fecha DESC;";

            return Db.DataTable(_empresa, sql, CommandType.Text, p =>
            {
                p.Add("@desde", SqlDbType.DateTime).Value = desde;
                p.Add("@hasta", SqlDbType.DateTime).Value = hasta;
            });
        }

        public DataTable ObtenerVentasConBonificacionManual(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT DISTINCT v.idVenta, v.fechaVenta AS fecha_negocio, v.creado AS creado,
                       v.actualizado AS actualizado, u.nombre AS vendedor,
                       COALESCE(v.actualizado, v.creado, v.fechaVenta) AS orden_fecha
                FROM dbo.LineaVenta lv
                INNER JOIN dbo.Ventas v ON v.idVenta = lv.idVenta
                LEFT JOIN dbo.Usuarios u ON u.id = v.idVendedor
                WHERE lv.cantKg > 0 AND lv.bonificacion <> 0
                  AND COALESCE(v.actualizado, v.creado, v.fechaVenta) BETWEEN @desde AND @hasta
                ORDER BY orden_fecha DESC;";

            return Db.DataTable(_empresa, sql, CommandType.Text, p =>
            {
                p.Add("@desde", SqlDbType.DateTime).Value = desde;
                p.Add("@hasta", SqlDbType.DateTime).Value = hasta;
            });
        }

        public DataTable ObtenerMovimientos(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT m.idMovimiento, m.fechaMovimiento AS fecha_negocio, m.creado AS creado,
                       m.actualizado AS actualizado,
                       so.sucursal AS sucursal_origen, sd.sucursal AS sucursal_destino,
                       COALESCE(ap.nombre, cp.nombre) AS usuario
                FROM dbo.Movimiento m
                LEFT JOIN dbo.Sucursal so ON so.idSucursal = m.sucursalOrigen
                LEFT JOIN dbo.Sucursal sd ON sd.idSucursal = m.sucursalDestino
                LEFT JOIN dbo.Usuarios cp ON cp.id = m.creadoPor
                LEFT JOIN dbo.Usuarios ap ON ap.id = m.actualizadoPor
                WHERE (so.idEmpresa = @idEmpresa OR sd.idEmpresa = @idEmpresa)
                  AND COALESCE(m.actualizado, m.creado, m.fechaMovimiento) BETWEEN @desde AND @hasta
                ORDER BY COALESCE(m.actualizado, m.creado, m.fechaMovimiento) DESC;";

            return Db.DataTable(_empresa, sql, CommandType.Text, p =>
            {
                p.Add("@desde", SqlDbType.DateTime).Value = desde;
                p.Add("@hasta", SqlDbType.DateTime).Value = hasta;
                p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
            });
        }

        public DataTable ObtenerCompras(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT c.idCompra, c.fechaCompra AS fecha_negocio, c.creado AS creado,
                       c.actualizado AS actualizado,
                       c.tipoCompra, COALESCE(ap.nombre, cp.nombre) AS usuario
                FROM dbo.Compras c
                LEFT JOIN dbo.Usuarios cp ON cp.id = c.creadoPor
                LEFT JOIN dbo.Usuarios ap ON ap.id = c.actualizadoPor
                WHERE COALESCE(c.actualizado, c.creado, c.fechaCompra) BETWEEN @desde AND @hasta
                ORDER BY COALESCE(c.actualizado, c.creado, c.fechaCompra) DESC;";

            return Db.DataTable(_empresa, sql, CommandType.Text, p =>
            {
                p.Add("@desde", SqlDbType.DateTime).Value = desde;
                p.Add("@hasta", SqlDbType.DateTime).Value = hasta;
            });
        }

        public DataTable ObtenerFormulas(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT f.idFormula, COALESCE(f.actualizado, f.creado) AS fecha,
                       c.corte AS producto, COALESCE(ap.nombre, cp.nombre) AS usuario
                FROM dbo.Formulas f
                LEFT JOIN dbo.Corte c ON c.idCorte = f.idEmbutido
                LEFT JOIN dbo.Usuarios cp ON cp.id = f.creadoPor
                LEFT JOIN dbo.Usuarios ap ON ap.id = f.actualizadoPor
                WHERE c.idEmpresa = @idEmpresa
                  AND COALESCE(f.actualizado, f.creado) BETWEEN @desde AND @hasta
                ORDER BY fecha DESC;";

            return Db.DataTable(_empresa, sql, CommandType.Text, p =>
            {
                p.Add("@desde", SqlDbType.DateTime).Value = desde;
                p.Add("@hasta", SqlDbType.DateTime).Value = hasta;
                p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
            });
        }
    }
}
