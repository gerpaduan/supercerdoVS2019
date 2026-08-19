using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.ICatalogoGlobalProductoRepository (4/4 metodos
    // publicos). "catalogoglobalproducto" es catalogo global puro: sin idempresa, sin RLS
    // (mismo criterio que formularios/alicuotasiva). idcorte NO es identity (valores fijos
    // preasignados desde el origen, igual que en SQL Server).
    //
    // ObtenerCatalogoGlobalPorIds: el original en SQL Server batchea de a 2000 ids (limite de
    // parametros de SqlCommand). En Postgres se usa "= ANY(@ids)" con un array nativo -- sin el
    // limite de SQL Server, no hace falta el batching. Mismo resultado, una sola consulta.
    public class CatalogoGlobalProductoPg : Contratos.ICatalogoGlobalProductoRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;
        private readonly Contratos.IPersonaRepository _personaRepo;

        public CatalogoGlobalProductoPg(string connectionString, int idEmpresa, Contratos.IPersonaRepository personaRepo)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
            _personaRepo = personaRepo ?? throw new ArgumentNullException(nameof(personaRepo));
        }

        private const string ColumnasBase = @"
                c.idcorte, c.codigo, c.corte, c.tipo, c.promedio, c.puntostock, c.nivel,
                c.idcortemaestro, cm.corte AS cortemaestro, c.porcentaje, c.preciokg,
                c.ingresorapidoembutido, c.habilitado, c.encierrestock, c.porcentajehueso,
                c.independiente, c.desvioestandar, c.creado, c.actualizado, c.idalicuotaiva,
                c.alicuotaiva, c.pesable";

        private static int GetInt32Safe(NpgsqlDataReader dr, string columna, int defaultValue = 0) =>
            dr[columna] == DBNull.Value ? defaultValue : Convert.ToInt32(dr[columna]);

        private static long GetInt64Safe(NpgsqlDataReader dr, string columna, long defaultValue = 0) =>
            dr[columna] == DBNull.Value ? defaultValue : Convert.ToInt64(dr[columna]);

        private static float GetFloatSafe(NpgsqlDataReader dr, string columna, float defaultValue = 0f) =>
            dr[columna] == DBNull.Value ? defaultValue : Convert.ToSingle(dr[columna]);

        private static bool GetBoolSafe(NpgsqlDataReader dr, string columna, bool defaultValue = false) =>
            dr[columna] == DBNull.Value ? defaultValue : Convert.ToBoolean(dr[columna]);

        private static string GetStringSafe(NpgsqlDataReader dr, string columna, string defaultValue = "") =>
            dr[columna] == DBNull.Value ? defaultValue : Convert.ToString(dr[columna]);

        private static DateTime GetDateTimeSafe(NpgsqlDataReader dr, string columna, DateTime defaultValue) =>
            dr[columna] == DBNull.Value ? defaultValue : Convert.ToDateTime(dr[columna]);

        public Entidades.CatalogoGlobalProducto findCorteGlobalByCodigo(long codigo, bool buscarMaestro)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM catalogoglobalproducto WHERE codigo = @codigo;",
                dr => MapCorteGlobalCompleto(dr, buscarMaestro),
                p => p.AddWithValue("codigo", codigo));

            return lista.Count > 0 ? lista[0] : null;
        }

        private Entidades.CatalogoGlobalProducto findCorteGlobalById(int idCorte, bool buscarMaestro)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM catalogoglobalproducto WHERE idcorte = @idCorte;",
                dr => MapCorteGlobalCompleto(dr, buscarMaestro),
                p => p.AddWithValue("idCorte", idCorte));

            return lista.Count > 0 ? lista[0] : null;
        }

        private Entidades.CatalogoGlobalProducto MapCorteGlobalCompleto(NpgsqlDataReader dr, bool cargarMaestro)
        {
            var producto = new Entidades.CatalogoGlobalProducto
            {
                IdCorte = GetInt32Safe(dr, "idcorte"),
                Codigo = GetInt64Safe(dr, "codigo"),
                CorteDesc = GetStringSafe(dr, "corte"),
                Tipo = GetStringSafe(dr, "tipo"),
                Promedio = GetFloatSafe(dr, "promedio"),
                PuntoStock = GetInt32Safe(dr, "puntostock"),
                Nivel = GetInt32Safe(dr, "nivel"),
                Porcentaje = GetFloatSafe(dr, "porcentaje"),
                PrecioKg = GetFloatSafe(dr, "preciokg"),
                PrecioKgReferencia = GetFloatSafe(dr, "preciokg"),
                IngresoRapidoEmbutido = GetBoolSafe(dr, "ingresorapidoembutido"),
                Habilitado = GetBoolSafe(dr, "habilitado"),
                EnCierreStock = GetBoolSafe(dr, "encierrestock", true),
                PorcentajeHueso = GetFloatSafe(dr, "porcentajehueso"),
                Independiente = GetInt32Safe(dr, "independiente"),
                DesvioEstandar = GetFloatSafe(dr, "desvioestandar"),
                Creado = GetDateTimeSafe(dr, "creado", DateTime.MinValue),
                Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"]),
                IdAlicuotaIva = GetInt32Safe(dr, "idalicuotaiva"),
                AlicuotaIva = GetFloatSafe(dr, "alicuotaiva"),
                Pesable = GetBoolSafe(dr, "pesable")
            };

            if (dr["idmarca"] != DBNull.Value)
            {
                producto.Marca = _personaRepo.findById(GetInt32Safe(dr, "idmarca"));
            }

            if (cargarMaestro)
            {
                int idMaestro = GetInt32Safe(dr, "idcortemaestro");
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

            string texto = (busqueda ?? "").Trim();
            string buscar = "%" + texto + "%";
            string tipoFiltro = (tipo ?? "").Trim();

            string sql = $@"
                WITH catalogoglobal AS
                (
                    SELECT
                        {ColumnasBase},
                        ROW_NUMBER() OVER (ORDER BY c.codigo ASC, c.idcorte ASC) AS fila
                    FROM catalogoglobalproducto c
                    LEFT JOIN catalogoglobalproducto cm ON cm.idcorte = c.idcortemaestro
                    WHERE (@texto = '' OR c.corte ILIKE @buscar OR CAST(c.codigo AS text) LIKE @buscar)
                      AND (@tipo = '' OR c.tipo = @tipo)
                )
                SELECT *
                FROM catalogoglobal
                WHERE fila BETWEEN @desde AND @hasta
                ORDER BY fila ASC;";

            return DbPg.Reader(_connectionString, _idEmpresa, sql,
                dr => MapCatalogoGlobal(dr),
                p =>
                {
                    p.AddWithValue("texto", texto);
                    p.AddWithValue("buscar", buscar);
                    p.AddWithValue("tipo", tipoFiltro);
                    p.AddWithValue("desde", desde);
                    p.AddWithValue("hasta", hasta);
                });
        }

        public List<string> ObtenerTiposCatalogoGlobal()
        {
            return DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT DISTINCT TRIM(tipo) AS tipo
                FROM catalogoglobalproducto
                WHERE TRIM(COALESCE(tipo, '')) <> ''
                ORDER BY TRIM(tipo);",
                dr => Convert.ToString(dr["tipo"]));
        }

        public List<Entidades.CatalogoGlobalProducto> ObtenerCatalogoGlobalPorIds(IEnumerable<int> idsCortes)
        {
            var ids = (idsCortes ?? Enumerable.Empty<int>()).Distinct().Where(x => x > 0).ToList();
            if (ids.Count == 0) return new List<Entidades.CatalogoGlobalProducto>();

            string sql = $@"
                SELECT {ColumnasBase}
                FROM catalogoglobalproducto c
                LEFT JOIN catalogoglobalproducto cm ON cm.idcorte = c.idcortemaestro
                WHERE c.idcorte = ANY(@ids)
                ORDER BY c.codigo ASC, c.idcorte ASC;";

            return DbPg.Reader(_connectionString, _idEmpresa, sql,
                dr => MapCatalogoGlobal(dr),
                p => p.AddWithValue("ids", ids));
        }

        private static Entidades.CatalogoGlobalProducto MapCatalogoGlobal(NpgsqlDataReader dr)
        {
            var producto = new Entidades.CatalogoGlobalProducto
            {
                IdCorte = Convert.ToInt32(dr["idcorte"]),
                Codigo = Convert.ToInt64(dr["codigo"]),
                CorteDesc = Convert.ToString(dr["corte"]),
                Tipo = Convert.ToString(dr["tipo"]),
                Promedio = dr["promedio"] == DBNull.Value ? 0f : Convert.ToSingle(dr["promedio"]),
                PuntoStock = dr["puntostock"] == DBNull.Value ? 0 : Convert.ToInt32(dr["puntostock"]),
                Nivel = dr["nivel"] == DBNull.Value ? 0 : Convert.ToInt32(dr["nivel"]),
                Porcentaje = dr["porcentaje"] == DBNull.Value ? 0f : Convert.ToSingle(dr["porcentaje"]),
                PrecioKg = dr["preciokg"] == DBNull.Value ? 0f : Convert.ToSingle(dr["preciokg"]),
                PrecioKgReferencia = dr["preciokg"] == DBNull.Value ? 0f : Convert.ToSingle(dr["preciokg"]),
                IngresoRapidoEmbutido = dr["ingresorapidoembutido"] != DBNull.Value && Convert.ToBoolean(dr["ingresorapidoembutido"]),
                Habilitado = dr["habilitado"] != DBNull.Value && Convert.ToBoolean(dr["habilitado"]),
                EnCierreStock = dr["encierrestock"] != DBNull.Value && Convert.ToBoolean(dr["encierrestock"]),
                PorcentajeHueso = dr["porcentajehueso"] == DBNull.Value ? 0f : Convert.ToSingle(dr["porcentajehueso"]),
                Independiente = dr["independiente"] == DBNull.Value ? 0 : Convert.ToInt32(dr["independiente"]),
                DesvioEstandar = dr["desvioestandar"] == DBNull.Value ? 0f : Convert.ToSingle(dr["desvioestandar"]),
                Creado = dr["creado"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["creado"]),
                Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"]),
                IdAlicuotaIva = dr["idalicuotaiva"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idalicuotaiva"]),
                AlicuotaIva = dr["alicuotaiva"] == DBNull.Value ? 0f : Convert.ToSingle(dr["alicuotaiva"]),
                Pesable = dr["pesable"] != DBNull.Value && Convert.ToBoolean(dr["pesable"])
            };

            int idMaestro = dr["idcortemaestro"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idcortemaestro"]);
            if (idMaestro > 0)
            {
                producto.CorteMaestro = new Entidades.CatalogoGlobalProducto
                {
                    IdCorte = idMaestro,
                    CorteDesc = dr["cortemaestro"] == DBNull.Value ? "" : Convert.ToString(dr["cortemaestro"])
                };
            }

            producto.Presentacion = producto.EsPresentacion(producto.PorcentajeHueso);
            if (producto.Presentacion)
                producto.Porcentaje = producto.getCantPresentacion(producto.PorcentajeHueso);

            return producto;
        }
    }
}
