using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Utilidades;

namespace Datos
{
    /// <summary>
    /// Acceso a datos de dbo.CatalogoGlobalProducto (catalogo global de productos,
    /// separado de dbo.Corte -- ver 20260804-Create_CatalogoGlobalProducto.sql). Mismas
    /// firmas que los metodos equivalentes que existian en Datos.Corte para el catalogo
    /// global (findCorteGlobalByCodigo, ObtenerCatalogoGlobalPagina,
    /// ObtenerCatalogoGlobalPorIds, ObtenerTiposCatalogoGlobal), para minimizar el cambio
    /// en los call sites de ProductosController.
    /// </summary>
    public class CatalogoGlobalProducto : Contratos.ICatalogoGlobalProductoRepository
    {
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;

        public CatalogoGlobalProducto(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
            _param = param;
        }

        private const string ColumnasBase = @"
                c.idCorte, c.codigo, c.corte, c.tipo, c.promedio, c.puntoStock, c.nivel,
                c.idCorteMaestro, cm.corte AS corteMaestro, c.porcentaje, c.precioKg,
                c.ingresoRapidoEmbutido, c.habilitado, c.enCierreStock, c.porcentajeHueso,
                c.independiente, c.desvioEstandar, c.creado, c.actualizado, c.idAlicuotaIva,
                c.alicuotaIva, c.pesable";

        private static int GetInt32Safe(SqlDataReader dr, string columnName, int defaultValue = 0)
            => dr[columnName] == DBNull.Value ? defaultValue : Convert.ToInt32(dr[columnName]);

        private static long GetInt64Safe(SqlDataReader dr, string columnName, long defaultValue = 0)
            => dr[columnName] == DBNull.Value ? defaultValue : Convert.ToInt64(dr[columnName]);

        private static float GetFloatSafe(SqlDataReader dr, string columnName, float defaultValue = 0f)
            => dr[columnName] == DBNull.Value ? defaultValue : float.Parse(dr[columnName].ToString());

        private static bool GetBoolSafe(SqlDataReader dr, string columnName, bool defaultValue = false)
            => dr[columnName] == DBNull.Value ? defaultValue : Convert.ToBoolean(dr[columnName]);

        private static string GetStringSafe(SqlDataReader dr, string columnName, string defaultValue = "")
            => dr[columnName] == DBNull.Value ? defaultValue : Convert.ToString(dr[columnName]);

        private static DateTime GetDateTimeSafe(SqlDataReader dr, string columnName, DateTime defaultValue)
            => dr[columnName] == DBNull.Value ? defaultValue : Convert.ToDateTime(dr[columnName]);

        /// <summary>
        /// Mismo criterio que Datos.Corte.findCorteGlobalByCodigo original: SELECT * (una
        /// sola fila) resolviendo Marca con una consulta aparte a Personas y, si
        /// buscarMaestro, el corte maestro completo de forma recursiva (un solo nivel,
        /// igual que MapCorte). Se usa en el alta manual/rapida por codigo de barra, donde
        /// el N+1 de estas dos consultas extra es aceptable (una sola fila).
        /// </summary>
        public Entidades.CatalogoGlobalProducto findCorteGlobalByCodigo(long codigo, bool buscarMaestro)
        {
            var lista = Db.Reader(
                _empresa,
                "SELECT * FROM dbo.CatalogoGlobalProducto WHERE codigo = @codigo",
                CommandType.Text,
                dr => MapCorteGlobalCompleto(dr, buscarMaestro),
                p => p.Add("@codigo", SqlDbType.BigInt).Value = codigo
            );

            return lista.Count > 0 ? lista[0] : null;
        }

        private Entidades.CatalogoGlobalProducto findCorteGlobalById(int idCorte, bool buscarMaestro)
        {
            var lista = Db.Reader(
                _empresa,
                "SELECT * FROM dbo.CatalogoGlobalProducto WHERE idCorte = @idCorte",
                CommandType.Text,
                dr => MapCorteGlobalCompleto(dr, buscarMaestro),
                p => p.Add("@idCorte", SqlDbType.Int).Value = idCorte
            );

            return lista.Count > 0 ? lista[0] : null;
        }

        private Entidades.CatalogoGlobalProducto MapCorteGlobalCompleto(SqlDataReader dr, bool cargarMaestro)
        {
            var producto = new Entidades.CatalogoGlobalProducto
            {
                IdCorte = GetInt32Safe(dr, "idCorte"),
                Codigo = GetInt64Safe(dr, "codigo"),
                CorteDesc = GetStringSafe(dr, "corte"),
                Tipo = GetStringSafe(dr, "tipo"),
                Promedio = GetFloatSafe(dr, "promedio"),
                PuntoStock = GetInt32Safe(dr, "puntoStock"),
                Nivel = GetInt32Safe(dr, "nivel"),
                Porcentaje = GetFloatSafe(dr, "porcentaje"),
                PrecioKg = GetFloatSafe(dr, "precioKg"),
                PrecioKgReferencia = GetFloatSafe(dr, "precioKg"),
                IngresoRapidoEmbutido = GetBoolSafe(dr, "ingresoRapidoEmbutido"),
                Habilitado = GetBoolSafe(dr, "habilitado"),
                EnCierreStock = GetBoolSafe(dr, "enCierreStock", true),
                PorcentajeHueso = GetFloatSafe(dr, "porcentajeHueso"),
                Independiente = GetInt32Safe(dr, "independiente"),
                DesvioEstandar = GetFloatSafe(dr, "desvioEstandar"),
                Creado = GetDateTimeSafe(dr, "creado", DateTime.MinValue),
                Actualizado = dr["actualizado"] == DBNull.Value ? null : (DateTime?)dr["actualizado"],
                IdAlicuotaIva = GetInt32Safe(dr, "idAlicuotaIva"),
                AlicuotaIva = GetFloatSafe(dr, "alicuotaIva"),
                Pesable = GetBoolSafe(dr, "pesable")
            };

            if (dr["idMarca"] != DBNull.Value)
            {
                var oPersonaD = new Datos.Persona(_empresa, _param);
                producto.Marca = oPersonaD.findById(GetInt32Safe(dr, "idMarca"));
            }

            if (cargarMaestro)
            {
                int idMaestro = GetInt32Safe(dr, "idCorteMaestro");
                producto.CorteMaestro = idMaestro > 0 ? findCorteGlobalById(idMaestro, false) : null;
            }

            producto.Presentacion = producto.EsPresentacion(producto.PorcentajeHueso);
            if (producto.Presentacion)
                producto.Porcentaje = producto.getCantPresentacion(producto.PorcentajeHueso);

            return producto;
        }

        public List<Entidades.CatalogoGlobalProducto> ObtenerCatalogoGlobalPagina(string busqueda, string tipo, int pagina, int cantidad, int cantidadExtra)
        {
            pagina = pagina < 1 ? 1 : pagina;
            cantidad = cantidad < 1 ? 1 : cantidad;
            cantidadExtra = cantidadExtra < 0 ? 0 : cantidadExtra;
            int desde = (int)Math.Min(((long)(pagina - 1) * cantidad) + 1, int.MaxValue);
            int hasta = (int)Math.Min((long)desde + cantidad + cantidadExtra - 1, int.MaxValue);

            string sql = $@"
                ;WITH CatalogoGlobal AS
                (
                    SELECT
                        {ColumnasBase},
                        ROW_NUMBER() OVER (ORDER BY c.codigo ASC, c.idCorte ASC) AS fila
                    FROM dbo.CatalogoGlobalProducto c
                    LEFT JOIN dbo.CatalogoGlobalProducto cm ON cm.idCorte = c.idCorteMaestro
                    WHERE (@texto = '' OR c.corte LIKE @buscar OR CAST(c.codigo AS NVARCHAR(50)) LIKE @buscar)
                      AND (@tipo = '' OR c.tipo = @tipo)
                )
                SELECT *
                FROM CatalogoGlobal
                WHERE fila BETWEEN @desde AND @hasta
                ORDER BY fila ASC;";

            string texto = (busqueda ?? "").Trim();
            string buscar = "%" + texto + "%";
            string tipoFiltro = (tipo ?? "").Trim();

            return Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                dr => MapCatalogoGlobal(dr),
                p =>
                {
                    p.Add("@texto", SqlDbType.NVarChar, 100).Value = texto;
                    p.Add("@buscar", SqlDbType.NVarChar, 110).Value = buscar;
                    p.Add("@tipo", SqlDbType.NVarChar, 100).Value = tipoFiltro;
                    p.Add("@desde", SqlDbType.Int).Value = desde;
                    p.Add("@hasta", SqlDbType.Int).Value = hasta;
                });
        }

        public List<string> ObtenerTiposCatalogoGlobal()
        {
            const string sql = @"
                SELECT DISTINCT LTRIM(RTRIM(tipo)) AS tipo
                FROM dbo.CatalogoGlobalProducto
                WHERE LTRIM(RTRIM(ISNULL(tipo, ''))) <> ''
                ORDER BY LTRIM(RTRIM(tipo));";

            return Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                dr => Convert.ToString(dr["tipo"]));
        }

        public List<Entidades.CatalogoGlobalProducto> ObtenerCatalogoGlobalPorIds(IEnumerable<int> idsCortes)
        {
            var ids = (idsCortes ?? Enumerable.Empty<int>()).Distinct().Where(x => x > 0).ToList();
            var resultado = new List<Entidades.CatalogoGlobalProducto>();

            foreach (var lote in ids.Select((id, indice) => new { id, indice }).GroupBy(x => x.indice / 2000).Select(x => x.Select(y => y.id).ToList()))
            {
                resultado.AddRange(ObtenerCatalogoGlobalPaginaPorIds(lote));
            }

            return resultado;
        }

        private List<Entidades.CatalogoGlobalProducto> ObtenerCatalogoGlobalPaginaPorIds(List<int> ids)
        {
            var sql = new StringBuilder($@"
                SELECT {ColumnasBase}
                FROM dbo.CatalogoGlobalProducto c
                LEFT JOIN dbo.CatalogoGlobalProducto cm ON cm.idCorte = c.idCorteMaestro
                WHERE c.idCorte IN (");

            for (int i = 0; i < ids.Count; i++)
            {
                if (i > 0) sql.Append(", ");
                sql.Append("@id").Append(i);
            }
            sql.Append(") ORDER BY c.codigo ASC, c.idCorte ASC;");

            return Db.Reader(
                _empresa,
                sql.ToString(),
                CommandType.Text,
                dr => MapCatalogoGlobal(dr),
                p =>
                {
                    for (int i = 0; i < ids.Count; i++)
                        p.Add("@id" + i, SqlDbType.Int).Value = ids[i];
                });
        }

        private static Entidades.CatalogoGlobalProducto MapCatalogoGlobal(SqlDataReader dr)
        {
            var producto = new Entidades.CatalogoGlobalProducto
            {
                IdCorte = Convert.ToInt32(dr["idCorte"]),
                Codigo = Convert.ToInt64(dr["codigo"]),
                CorteDesc = Convert.ToString(dr["corte"]),
                Tipo = Convert.ToString(dr["tipo"]),
                Promedio = dr["promedio"] == DBNull.Value ? 0f : Convert.ToSingle(dr["promedio"]),
                PuntoStock = dr["puntoStock"] == DBNull.Value ? 0 : Convert.ToInt32(dr["puntoStock"]),
                Nivel = dr["nivel"] == DBNull.Value ? 0 : Convert.ToInt32(dr["nivel"]),
                Porcentaje = dr["porcentaje"] == DBNull.Value ? 0f : Convert.ToSingle(dr["porcentaje"]),
                PrecioKg = dr["precioKg"] == DBNull.Value ? 0f : Convert.ToSingle(dr["precioKg"]),
                PrecioKgReferencia = dr["precioKg"] == DBNull.Value ? 0f : Convert.ToSingle(dr["precioKg"]),
                IngresoRapidoEmbutido = dr["ingresoRapidoEmbutido"] != DBNull.Value && Convert.ToBoolean(dr["ingresoRapidoEmbutido"]),
                Habilitado = dr["habilitado"] != DBNull.Value && Convert.ToBoolean(dr["habilitado"]),
                EnCierreStock = dr["enCierreStock"] != DBNull.Value && Convert.ToBoolean(dr["enCierreStock"]),
                PorcentajeHueso = dr["porcentajeHueso"] == DBNull.Value ? 0f : Convert.ToSingle(dr["porcentajeHueso"]),
                Independiente = dr["independiente"] == DBNull.Value ? 0 : Convert.ToInt32(dr["independiente"]),
                DesvioEstandar = dr["desvioEstandar"] == DBNull.Value ? 0f : Convert.ToSingle(dr["desvioEstandar"]),
                Creado = dr["creado"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["creado"]),
                Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"]),
                IdAlicuotaIva = dr["idAlicuotaIva"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idAlicuotaIva"]),
                AlicuotaIva = dr["alicuotaIva"] == DBNull.Value ? 0f : Convert.ToSingle(dr["alicuotaIva"]),
                Pesable = dr["pesable"] != DBNull.Value && Convert.ToBoolean(dr["pesable"])
            };

            int idMaestro = dr["idCorteMaestro"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idCorteMaestro"]);
            if (idMaestro > 0)
            {
                producto.CorteMaestro = new Entidades.CatalogoGlobalProducto
                {
                    IdCorte = idMaestro,
                    CorteDesc = dr["corteMaestro"] == DBNull.Value ? "" : Convert.ToString(dr["corteMaestro"])
                };
            }

            producto.Presentacion = producto.EsPresentacion(producto.PorcentajeHueso);
            if (producto.Presentacion)
                producto.Porcentaje = producto.getCantPresentacion(producto.PorcentajeHueso);

            return producto;
        }
    }
}
