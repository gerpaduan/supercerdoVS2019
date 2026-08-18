using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Entidades;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres del bloque CRUD/referencia de Contratos.ICorteRepository
    // (Corte, ActualizacionCorte, CatalogoGlobalImportacionProductos, Formulas/CortePorFormula,
    // AlicuotasIva, TiposProducto). El resto de Datos.Corte (Embutido, Movimiento, cascade de
    // StockCorteSucursal, reportes) no esta cubierto por ICorteRepository todavia -- se agrega
    // en una etapa futura. Ver docs/DECISIONS.md, Etapa 6.
    //
    // StockCorteSucursal: NUNCA se porta a Postgres (tabla obsoleta, 0 filas reales, confirmado
    // sin lector/escritor real en C#) -- eliminarCorte omite ese paso a proposito, no es un gap.
    public class CortePg : Contratos.ICorteRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;
        private readonly Contratos.IPersonaRepository _personaRepo;

        public CortePg(string connectionString, int idEmpresa, Contratos.IPersonaRepository personaRepo)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
            _personaRepo = personaRepo ?? throw new ArgumentNullException(nameof(personaRepo));
        }

        #region Helpers

        private static bool ColumnaExiste(NpgsqlDataReader dr, string columna)
        {
            try { return dr.GetOrdinal(columna) >= 0; } catch { return false; }
        }

        private static string GetString(NpgsqlDataReader dr, string columna) =>
            ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value ? Convert.ToString(dr[columna]) : "";

        private static int GetInt(NpgsqlDataReader dr, string columna) =>
            ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value ? Convert.ToInt32(dr[columna]) : 0;

        private static float GetFloat(NpgsqlDataReader dr, string columna) =>
            ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value ? Convert.ToSingle(dr[columna]) : 0f;

        private static bool GetBool(NpgsqlDataReader dr, string columna) =>
            ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value && Convert.ToBoolean(dr[columna]);

        private static long GetLong(NpgsqlDataReader dr, string columna) =>
            ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value ? Convert.ToInt64(dr[columna]) : 0L;

        private Usuario GetUsuarioLiviano(int id)
        {
            if (id <= 0) return null;

            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT id, nombre, usuario AS user, email, idsucursaluser AS idsucursal, idempresa FROM usuarios WHERE id = @id;",
                dr => new Usuario
                {
                    Id = Convert.ToInt32(dr["id"]),
                    Nombre = dr["nombre"] as string,
                    User = dr["user"] as string,
                    Email = dr["email"] as string,
                    IdSucursal = dr["idsucursal"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idsucursal"]),
                    IdEmpresa = Convert.ToInt32(dr["idempresa"])
                },
                p => p.AddWithValue("id", id));

            return lista.Count > 0 ? lista[0] : null;
        }

        // Replica exacto el calculo de "Nivel" de las SPs reales addOrEditCorte/obtenerNivelCorte
        // (verificadas con sp_helptext contra la base viva, no inventado): busca hasta 4 niveles
        // de profundidad en la cadena idCorteMaestro. Tope de 4 niveles deliberado del original,
        // no una limitacion nuestra -- no se "mejora" a una recursion infinita.
        private int CalcularNivel(int idCorteMaestro)
        {
            if (idCorteMaestro <= 0) return 0;

            const string sql = @"
                SELECT
                    CASE
                        WHEN EXISTS (
                            SELECT 1 WHERE @idCorteMaestro IN (
                                SELECT idcorte FROM corte AS corte_n2
                                WHERE idcortemaestro IN (
                                    SELECT idcorte FROM corte AS corte_n1
                                    WHERE idcortemaestro IN (
                                        SELECT idcorte FROM corte AS corte_n0
                                        WHERE idcortemaestro IN (SELECT idcorte FROM corte AS corte_n)
                                    )
                                )
                            )
                        ) THEN 4
                        WHEN EXISTS (
                            SELECT 1 WHERE @idCorteMaestro IN (
                                SELECT corte_n2.idcorte FROM corte AS corte_n2
                                WHERE corte_n2.idcortemaestro IN (
                                    SELECT corte_n1.idcorte FROM corte AS corte_n1
                                    WHERE corte_n1.idcortemaestro IN (SELECT corte_n0.idcorte FROM corte AS corte_n0)
                                )
                            )
                        ) THEN 3
                        WHEN EXISTS (
                            SELECT 1 WHERE @idCorteMaestro IN (
                                SELECT corte_n1.idcorte FROM corte AS corte_n1
                                WHERE corte_n1.idcortemaestro IN (SELECT corte_n0.idcorte FROM corte AS corte_n0)
                            )
                        ) THEN 2
                        WHEN EXISTS (
                            SELECT 1 WHERE @idCorteMaestro IN (SELECT corte_n0.idcorte FROM corte AS corte_n0)
                        ) THEN 1
                        ELSE 0
                    END AS nivel;";

            object result = DbPg.Scalar(_connectionString, _idEmpresa, sql,
                p => p.AddWithValue("idCorteMaestro", idCorteMaestro));

            return (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
        }

        #endregion

        #region Mapeo Corte

        private Corte MapCorte(NpgsqlDataReader dr, bool cargarMaestro)
        {
            var oCorteE = new Corte
            {
                IdCorte = GetInt(dr, "idcorte"),
                Codigo = GetLong(dr, "codigo"),
                CorteDesc = GetString(dr, "corte"),
                Tipo = GetString(dr, "tipo"),
                Promedio = GetFloat(dr, "promedio"),
                PuntoStock = GetInt(dr, "puntostock"),
                Nivel = GetInt(dr, "nivel"),
                IdEmpresa = GetInt(dr, "idempresa"),
                Porcentaje = GetFloat(dr, "porcentaje"),
                PrecioKg = GetFloat(dr, "preciokg"),
                PrecioKgReferencia = GetFloat(dr, "preciokg"),
                IngresoRapidoEmbutido = GetBool(dr, "ingresorapidoembutido"),
                Habilitado = GetBool(dr, "habilitado"),
                EnCierreStock = ColumnaExiste(dr, "encierrestock") && dr["encierrestock"] != DBNull.Value
                    ? Convert.ToBoolean(dr["encierrestock"]) : true,
                PorcentajeHueso = GetFloat(dr, "porcentajehueso"),
                Independiente = GetInt(dr, "independiente"),
                DesvioEstandar = GetFloat(dr, "desvioestandar"),
                Creado = dr["creado"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["creado"]),
                Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"]),
                IdAlicuotaIva = GetInt(dr, "idalicuotaiva"),
                AlicuotaIva = GetFloat(dr, "alicuotaiva"),
                Pesable = GetBool(dr, "pesable")
            };

            int idMarca = GetInt(dr, "idmarca");
            if (idMarca > 0)
                oCorteE.Marca = _personaRepo.findById(idMarca);

            if (cargarMaestro)
            {
                int idMaestro = GetInt(dr, "idcortemaestro");
                oCorteE.CorteMaestro = idMaestro > 0 ? findCorteById(idMaestro, false) : null;
            }

            oCorteE.Presentacion = oCorteE.EsPresentacion(oCorteE.porcentajeHueso);
            if (oCorteE.Presentacion)
                oCorteE.porcentaje = oCorteE.getCantPresentacion(oCorteE.porcentajeHueso);

            return oCorteE;
        }

        private Corte MapCorteListado(NpgsqlDataReader dr)
        {
            var oCorteE = new Corte
            {
                IdCorte = GetInt(dr, "idcorte"),
                Codigo = GetLong(dr, "codigo"),
                CorteDesc = GetString(dr, "corte"),
                Tipo = GetString(dr, "tipo"),
                Promedio = GetFloat(dr, "promedio"),
                PuntoStock = GetInt(dr, "puntostock"),
                Nivel = GetInt(dr, "nivel"),
                IdEmpresa = GetInt(dr, "idempresa"),
                Porcentaje = GetFloat(dr, "porcentaje"),
                PrecioKg = GetFloat(dr, "preciokg"),
                PrecioKgReferencia = GetFloat(dr, "preciokg"),
                IngresoRapidoEmbutido = GetBool(dr, "ingresorapidoembutido"),
                Habilitado = GetBool(dr, "habilitado"),
                EnCierreStock = ColumnaExiste(dr, "encierrestock") && dr["encierrestock"] != DBNull.Value
                    ? Convert.ToBoolean(dr["encierrestock"]) : true,
                PorcentajeHueso = GetFloat(dr, "porcentajehueso"),
                Independiente = GetInt(dr, "independiente"),
                DesvioEstandar = GetFloat(dr, "desvioestandar"),
                Creado = dr["creado"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["creado"]),
                Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"]),
                IdAlicuotaIva = GetInt(dr, "idalicuotaiva"),
                AlicuotaIva = GetFloat(dr, "alicuotaiva"),
                Pesable = GetBool(dr, "pesable")
            };

            int marcaId = GetInt(dr, "marcaidpersona");
            if (marcaId > 0)
                oCorteE.Marca = new Persona { IdPersona = marcaId, RazonSocial = GetString(dr, "marcanombrejoin") };

            int corteMaestroId = GetInt(dr, "cortemaestroidjoin");
            if (corteMaestroId > 0)
                oCorteE.CorteMaestro = new Corte { IdCorte = corteMaestroId, CorteDesc = GetString(dr, "cortemaestronombrejoin") };

            oCorteE.Presentacion = oCorteE.EsPresentacion(oCorteE.PorcentajeHueso);
            if (oCorteE.Presentacion)
                oCorteE.Porcentaje = oCorteE.getCantPresentacion(oCorteE.PorcentajeHueso);

            return oCorteE;
        }

        #endregion

        #region Cortes

        public List<Corte> findAllCortes(bool buscarMaestro)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM corte ORDER BY codigo ASC;",
                dr => MapCorte(dr, buscarMaestro));
        }

        public List<Corte> findAllCortesListado()
        {
            const string sql = @"
                SELECT
                    c.*,
                    m.idpersona AS marcaidpersona, m.razonsocial AS marcanombrejoin,
                    cm.idcorte AS cortemaestroidjoin, cm.corte AS cortemaestronombrejoin
                FROM corte c
                LEFT JOIN personas m ON c.idmarca = m.idpersona
                LEFT JOIN corte cm ON c.idcortemaestro = cm.idcorte
                ORDER BY c.codigo ASC;";

            return DbPg.Reader(_connectionString, _idEmpresa, sql, MapCorteListado);
        }

        public Corte findCorteById(int idCorte, bool buscarMaestro)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM corte WHERE idcorte = @idCorte;",
                dr => MapCorte(dr, buscarMaestro),
                p => p.AddWithValue("idCorte", idCorte));

            return lista.Count > 0 ? lista[0] : null;
        }

        public Corte findCorteByCodigo(long codigo, bool buscarMaestro)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM corte WHERE codigo = @codigo;",
                dr => MapCorte(dr, buscarMaestro),
                p => p.AddWithValue("codigo", codigo));

            return lista.Count > 0 ? lista[0] : null;
        }

        public List<Corte> ObtenerCortesPorEmpresa(int idEmpresa, bool buscarMaestro)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM corte WHERE idempresa = @idEmpresa ORDER BY codigo ASC;",
                dr => MapCorte(dr, buscarMaestro),
                p => p.AddWithValue("idEmpresa", idEmpresa));
        }

        public List<Corte> ObtenerCortesPorEmpresaListado(int idEmpresa)
        {
            const string sql = @"
                SELECT
                    c.*,
                    m.idpersona AS marcaidpersona, m.razonsocial AS marcanombrejoin,
                    cm.idcorte AS cortemaestroidjoin, cm.corte AS cortemaestronombrejoin
                FROM corte c
                LEFT JOIN personas m ON c.idmarca = m.idpersona
                LEFT JOIN corte cm ON c.idcortemaestro = cm.idcorte
                WHERE c.idempresa = @idEmpresa
                ORDER BY c.codigo ASC;";

            return DbPg.Reader(_connectionString, _idEmpresa, sql, MapCorteListado,
                p => p.AddWithValue("idEmpresa", idEmpresa));
        }

        public Corte findCorteByCodigoEmpresa(long codigo, int idEmpresa, bool buscarMaestro)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM corte WHERE codigo = @codigo AND idempresa = @idEmpresa;",
                dr => MapCorte(dr, buscarMaestro),
                p =>
                {
                    p.AddWithValue("codigo", codigo);
                    p.AddWithValue("idEmpresa", idEmpresa);
                });

            return lista.Count > 0 ? lista[0] : null;
        }

        public void editPrecioCorte(Corte oCorteE)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE corte SET preciokg = @precioKg WHERE idcorte = @idCorte;",
                p =>
                {
                    p.AddWithValue("precioKg", oCorteE.precioKg);
                    p.AddWithValue("idCorte", oCorteE.idCorte);
                });
        }

        public void addOrEditCorte(Corte oCorteE)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            // Espeja el SP real dbo.addOrEditCorte (verificado con sp_helptext contra la base
            // viva -- el snapshot del repo estaba truncado para esta SP, ver docs/DECISIONS.md).
            int idCorte = oCorteE.idCorte;

            if (idCorte == 0)
            {
                // Rama de "importacion": si ya existe un Corte con ese codigo, se edita en vez
                // de insertar. RLS ya limita la busqueda al tenant actual (igual que el original).
                object encontrado = DbPg.Scalar(_connectionString, _idEmpresa,
                    "SELECT idcorte FROM corte WHERE codigo = @codigo LIMIT 1;",
                    p => p.AddWithValue("codigo", oCorteE.codigo));
                if (encontrado != null && encontrado != DBNull.Value)
                    idCorte = Convert.ToInt32(encontrado);
            }

            int idCorteMaestro = oCorteE.corteMaestro != null ? oCorteE.corteMaestro.idCorte : 0;
            int nivel = CalcularNivel(idCorteMaestro);
            int idMarca = oCorteE.Marca != null ? oCorteE.Marca.IdPersona : 0;

            if (idCorte > 0)
            {
                const string sqlUpdate = @"
                    UPDATE corte SET codigo=@codigo, corte=@corte, preciokg=@precioKg, tipo=@tipo,
                        ingresorapidoembutido=@ingresoRapidoEmbutido, habilitado=@habilitado, encierrestock=@enCierreStock,
                        independiente=@independiente, idcortemaestro=@idCorteMaestro, porcentaje=@porcentaje,
                        porcentajehueso=@porcentajeHueso, desvioestandar=@desvioEstandar, promedio=@promedio,
                        puntostock=@puntoStock, idalicuotaiva=@idAlicuotaIva, alicuotaiva=@alicuotaIva,
                        pesable=@pesable, nivel=@nivel, idmarca=@idMarca, actualizado=now()
                    WHERE idcorte=@idCorte;";

                DbPg.NonQuery(_connectionString, _idEmpresa, sqlUpdate, p =>
                {
                    p.AddWithValue("idCorte", idCorte);
                    p.AddWithValue("codigo", oCorteE.codigo);
                    p.AddWithValue("corte", oCorteE.corte ?? "");
                    p.AddWithValue("precioKg", oCorteE.precioKg);
                    p.AddWithValue("tipo", oCorteE.tipo ?? "");
                    p.AddWithValue("ingresoRapidoEmbutido", oCorteE.IngresoRapidoEmbutido);
                    p.AddWithValue("habilitado", oCorteE.Habilitado);
                    p.AddWithValue("enCierreStock", oCorteE.EnCierreStock);
                    p.AddWithValue("independiente", oCorteE.independiente);
                    p.AddWithValue("idCorteMaestro", idCorteMaestro > 0 ? (object)idCorteMaestro : DBNull.Value);
                    p.AddWithValue("porcentaje", oCorteE.porcentaje);
                    p.AddWithValue("porcentajeHueso", oCorteE.porcentajeHueso);
                    p.AddWithValue("desvioEstandar", oCorteE.desvioEstandar);
                    p.AddWithValue("promedio", oCorteE.Promedio);
                    p.AddWithValue("puntoStock", oCorteE.PuntoStock);
                    p.AddWithValue("idAlicuotaIva", oCorteE.IdAlicuotaIva);
                    p.AddWithValue("alicuotaIva", oCorteE.AlicuotaIva);
                    p.AddWithValue("pesable", oCorteE.Pesable);
                    p.AddWithValue("nivel", nivel);
                    p.AddWithValue("idMarca", idMarca > 0 ? (object)idMarca : DBNull.Value);
                });

                // Historial write-only (nadie lo lee de vuelta) -- replica el insert real de la
                // SP: creado queda NULL, actualizado = ahora. Ver docs/DECISIONS.md, Etapa 6.
                const string sqlHist = @"
                    INSERT INTO actualizacioncorte
                        (idcorte, codigo, corte, preciokg, ingresorapidoembutido, encierrestock, tipo,
                         independiente, idcortemaestro, porcentaje, porcentajehueso, desvioestandar,
                         promedio, creado, actualizado, idempresa)
                    VALUES
                        (@idCorte, @codigo, @corte, @precioKg, @ingresoRapidoEmbutido, @enCierreStock, @tipo,
                         @independiente, @idCorteMaestro, @porcentaje, @porcentajeHueso, @desvioEstandar,
                         @promedio, NULL, now(), @idEmpresa);";

                DbPg.NonQuery(_connectionString, _idEmpresa, sqlHist, p =>
                {
                    p.AddWithValue("idCorte", idCorte);
                    p.AddWithValue("codigo", oCorteE.codigo);
                    p.AddWithValue("corte", oCorteE.corte ?? "");
                    p.AddWithValue("precioKg", oCorteE.precioKg);
                    p.AddWithValue("ingresoRapidoEmbutido", oCorteE.IngresoRapidoEmbutido);
                    p.AddWithValue("enCierreStock", oCorteE.EnCierreStock);
                    p.AddWithValue("tipo", oCorteE.tipo ?? "");
                    p.AddWithValue("independiente", oCorteE.independiente);
                    p.AddWithValue("idCorteMaestro", idCorteMaestro > 0 ? (object)idCorteMaestro : DBNull.Value);
                    p.AddWithValue("porcentaje", oCorteE.porcentaje);
                    p.AddWithValue("porcentajeHueso", oCorteE.porcentajeHueso);
                    p.AddWithValue("desvioEstandar", oCorteE.desvioEstandar);
                    p.AddWithValue("promedio", oCorteE.Promedio);
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });
            }
            else
            {
                const string sqlInsert = @"
                    INSERT INTO corte
                        (codigo, corte, preciokg, ingresorapidoembutido, habilitado, encierrestock, tipo,
                         independiente, idcortemaestro, porcentaje, porcentajehueso, desvioestandar, promedio,
                         puntostock, idalicuotaiva, alicuotaiva, pesable, nivel, idmarca, creado, idempresa)
                    VALUES
                        (@codigo, @corte, @precioKg, @ingresoRapidoEmbutido, @habilitado, @enCierreStock, @tipo,
                         @independiente, @idCorteMaestro, @porcentaje, @porcentajeHueso, @desvioEstandar, @promedio,
                         @puntoStock, @idAlicuotaIva, @alicuotaIva, @pesable, @nivel, @idMarca, now(), @idEmpresa)
                    RETURNING idcorte;";

                object nuevoId = DbPg.Scalar(_connectionString, _idEmpresa, sqlInsert, p =>
                {
                    p.AddWithValue("codigo", oCorteE.codigo);
                    p.AddWithValue("corte", oCorteE.corte ?? "");
                    p.AddWithValue("precioKg", oCorteE.precioKg);
                    p.AddWithValue("ingresoRapidoEmbutido", oCorteE.IngresoRapidoEmbutido);
                    p.AddWithValue("habilitado", oCorteE.Habilitado);
                    p.AddWithValue("enCierreStock", oCorteE.EnCierreStock);
                    p.AddWithValue("tipo", oCorteE.tipo ?? "");
                    p.AddWithValue("independiente", oCorteE.independiente);
                    p.AddWithValue("idCorteMaestro", idCorteMaestro > 0 ? (object)idCorteMaestro : DBNull.Value);
                    p.AddWithValue("porcentaje", oCorteE.porcentaje);
                    p.AddWithValue("porcentajeHueso", oCorteE.porcentajeHueso);
                    p.AddWithValue("desvioEstandar", oCorteE.desvioEstandar);
                    p.AddWithValue("promedio", oCorteE.Promedio);
                    p.AddWithValue("puntoStock", oCorteE.PuntoStock);
                    p.AddWithValue("idAlicuotaIva", oCorteE.IdAlicuotaIva);
                    p.AddWithValue("alicuotaIva", oCorteE.AlicuotaIva);
                    p.AddWithValue("pesable", oCorteE.Pesable);
                    p.AddWithValue("nivel", nivel);
                    p.AddWithValue("idMarca", idMarca > 0 ? (object)idMarca : DBNull.Value);
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });

                idCorte = Convert.ToInt32(nuevoId);
            }

            oCorteE.idCorte = idCorte;
        }

        public int InsertarCorteEnEmpresa(Corte oCorteE)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            const string sql = @"
                INSERT INTO corte
                    (idempresa, codigo, corte, tipo, idmarca, puntostock, promedio, independiente, preciokg,
                     ingresorapidoembutido, habilitado, encierrestock, idcortemaestro, nivel, porcentaje,
                     porcentajehueso, desvioestandar, creado, idalicuotaiva, alicuotaiva, pesable)
                VALUES
                    (@idEmpresa, @codigo, @corte, @tipo, @idMarca, @puntoStock, @promedio, @independiente, @precioKg,
                     @ingresoRapidoEmbutido, @habilitado, @enCierreStock, @idCorteMaestro, @nivel, @porcentaje,
                     @porcentajeHueso, @desvioEstandar, now(), @idAlicuotaIva, @alicuotaIva, @pesable)
                RETURNING idcorte;";

            object result = DbPg.Scalar(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idEmpresa", _idEmpresa);
                p.AddWithValue("codigo", oCorteE.codigo);
                p.AddWithValue("corte", oCorteE.corte ?? "");
                p.AddWithValue("tipo", oCorteE.tipo ?? "");
                p.AddWithValue("idMarca", oCorteE.Marca != null && oCorteE.Marca.IdPersona > 0 ? (object)oCorteE.Marca.IdPersona : DBNull.Value);
                p.AddWithValue("puntoStock", oCorteE.PuntoStock);
                p.AddWithValue("promedio", oCorteE.Promedio);
                p.AddWithValue("independiente", oCorteE.independiente);
                p.AddWithValue("precioKg", oCorteE.precioKg);
                p.AddWithValue("ingresoRapidoEmbutido", oCorteE.IngresoRapidoEmbutido);
                p.AddWithValue("habilitado", oCorteE.Habilitado);
                p.AddWithValue("enCierreStock", oCorteE.EnCierreStock);
                p.AddWithValue("idCorteMaestro", oCorteE.corteMaestro != null && oCorteE.corteMaestro.idCorte > 0 ? (object)oCorteE.corteMaestro.idCorte : DBNull.Value);
                p.AddWithValue("nivel", oCorteE.Nivel);
                p.AddWithValue("porcentaje", oCorteE.porcentaje);
                p.AddWithValue("porcentajeHueso", oCorteE.porcentajeHueso);
                p.AddWithValue("desvioEstandar", oCorteE.desvioEstandar);
                p.AddWithValue("idAlicuotaIva", oCorteE.IdAlicuotaIva);
                p.AddWithValue("alicuotaIva", oCorteE.AlicuotaIva);
                p.AddWithValue("pesable", oCorteE.Pesable);
            });

            return (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
        }

        public DataTable buscarCorte(string txtBusqueda)
        {
            // Espeja el SP real dbo.buscarCorte (sp_helptext). Las columnas "efectivo/debito/
            // credito/Billetera/Qr/Transf" son todas preciokg repetido en el original -- se
            // replica igual, no es invencion nuestra (nombre de columnas fiel al SP).
            const string sql = @"
                SELECT
                    corteP.idcorte, corteP.codigo, corteP.corte, corteP.preciokg,
                    corteP.preciokg AS efectivo, corteP.preciokg AS debito, corteP.preciokg AS credito,
                    corteP.preciokg AS ""Billetera"", corteP.preciokg AS ""Qr"", corteP.preciokg AS ""Transf"",
                    personas.identificacion AS ""Marca"",
                    corteP.puntostock, corteP.habilitado, corteP.mayorista, corteP.encierrestock, corteP.alicuotaiva,
                    corteP.tipo, corteP.pesable, corteP.nivel, corteP.idcortemaestro,
                    CASE WHEN corteP.porcentajehueso > 1000 THEN 'PRES.:' || corteM.corte ELSE corteM.corte END AS cortemaestro,
                    CAST(corteP.porcentaje AS numeric(10,2)) AS porcentaje,
                    CASE WHEN corteP.porcentajehueso > 1000 THEN 0 ELSE corteP.porcentajehueso END AS porcentajehueso,
                    corteP.desvioestandar, corteP.independiente, corteP.promedio, corteP.ingresorapidoembutido
                FROM personas RIGHT OUTER JOIN corte AS corteP ON personas.idpersona = corteP.idmarca
                LEFT OUTER JOIN corte AS corteM ON corteP.idcortemaestro = corteM.idcorte
                WHERE corteP.corte ILIKE @texto OR CAST(corteP.codigo AS text) = @textoExacto OR corteM.corte ILIKE @texto
                ORDER BY corteP.codigo;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("texto", "%" + (txtBusqueda ?? "") + "%");
                p.AddWithValue("textoExacto", txtBusqueda ?? "");
            });
        }

        public DataTable buscarCorteSinMaestro(string txtBusqueda)
        {
            const string sql = @"
                SELECT corteP.idcorte, corteP.codigo, corteP.corte, corteP.preciokg
                FROM corte AS corteP
                WHERE corteP.corte ILIKE @texto OR CAST(corteP.codigo AS text) LIKE @prefijo
                ORDER BY corteP.codigo;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("texto", "%" + (txtBusqueda ?? "") + "%");
                p.AddWithValue("prefijo", (txtBusqueda ?? "") + "%");
            });
        }

        public DataTable buscarCodigoCorte(long codigo)
        {
            return DbPg.DataTable(_connectionString, _idEmpresa,
                "SELECT * FROM corte WHERE codigo = @codigo;",
                p => p.AddWithValue("codigo", codigo));
        }

        public void eliminarCorte(Corte oCorteE)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            // Espeja EliminarCorte, salvo el DELETE de StockCorteSucursal (tabla obsoleta,
            // nunca portada -- ver docs/DECISIONS.md, Etapa 6, no es un olvido).
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM corte WHERE idcorte = @idCorte;",
                p => p.AddWithValue("idCorte", oCorteE.idCorte));
        }

        public DataTable obtenerCortes()
        {
            const string sql = @"
                SELECT corteP.idcorte, corteP.codigo, corteP.corte, corteP.preciokg, corteP.ingresorapidoembutido,
                    corteP.encierrestock, corteP.tipo, corteP.pesable, corteP.nivel, corteP.idcortemaestro,
                    corteM.corte AS cortemaestro, corteP.porcentaje, corteP.porcentajehueso, corteP.desvioestandar,
                    corteP.independiente, corteP.promedio, corteP.idalicuotaiva, corteP.alicuotaiva
                FROM corte AS corteM RIGHT OUTER JOIN corte AS corteP ON corteM.idcorte = corteP.idcortemaestro;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql);
        }

        public DataTable cargarDtCortes()
        {
            return DbPg.DataTable(_connectionString, _idEmpresa, "SELECT * FROM corte;");
        }

        public long sugerirCodigo(string tipo)
        {
            const string sql = @"
                SELECT MIN(codigo + 1) AS codigodisponible
                FROM corte
                WHERE tipo = @tipo
                  AND NOT EXISTS (
                      SELECT 1 FROM corte c2 WHERE c2.codigo = corte.codigo + 1 AND c2.tipo = @tipo
                  );";

            object obj = DbPg.Scalar(_connectionString, _idEmpresa, sql, p => p.AddWithValue("tipo", tipo ?? ""));
            return (obj == null || obj == DBNull.Value) ? -1 : Convert.ToInt64(obj);
        }

        public int obtenerNivelCorte(int idCorteMaestro)
        {
            return CalcularNivel(idCorteMaestro);
        }

        #endregion

        #region Catalogo global (importacion)

        public void AsegurarTablaImportacionCatalogoGlobal()
        {
            // No-op deliberado: la tabla catalogoglobalimportacionproductos ya existe siempre
            // en Postgres (creada por la migracion de schema), a diferencia de SQL Server donde
            // este metodo la crea en runtime si falta. Ver docs/DECISIONS.md, Etapa 6.
        }

        public List<CatalogoGlobalImportacionProducto> ObtenerImportacionesCatalogoGlobal(IEnumerable<int> idsProductosGlobales = null)
        {
            var ids = (idsProductosGlobales ?? new int[0]).Distinct().Where(x => x > 0).ToList();

            var sql = new StringBuilder(@"
                SELECT idcatalogoglobalimportacionproducto, idempresa, idproductoglobal, idproductoempresa, fechaalta, idusuarioalta
                FROM catalogoglobalimportacionproductos
                WHERE idempresa = @idEmpresa");

            if (ids.Count > 0)
            {
                sql.Append(" AND idproductoglobal = ANY(@idsGlobal)");
            }
            sql.Append(" ORDER BY idproductoglobal;");

            return DbPg.Reader(_connectionString, _idEmpresa, sql.ToString(),
                dr => new CatalogoGlobalImportacionProducto
                {
                    IdCatalogoGlobalImportacionProducto = Convert.ToInt32(dr["idcatalogoglobalimportacionproducto"]),
                    IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                    IdProductoGlobal = Convert.ToInt32(dr["idproductoglobal"]),
                    IdProductoEmpresa = Convert.ToInt32(dr["idproductoempresa"]),
                    FechaAlta = Convert.ToDateTime(dr["fechaalta"]),
                    IdUsuarioAlta = dr["idusuarioalta"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["idusuarioalta"])
                },
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    if (ids.Count > 0)
                        p.AddWithValue("idsGlobal", ids.ToArray());
                });
        }

        public void GuardarImportacionCatalogoGlobal(int idProductoGlobal, int idProductoEmpresa, int? idUsuarioAlta)
        {
            const string sql = @"
                INSERT INTO catalogoglobalimportacionproductos (idempresa, idproductoglobal, idproductoempresa, fechaalta, idusuarioalta)
                VALUES (@idEmpresa, @idProductoGlobal, @idProductoEmpresa, now(), @idUsuarioAlta)
                ON CONFLICT (idempresa, idproductoglobal) DO UPDATE SET
                    idproductoempresa = EXCLUDED.idproductoempresa,
                    idusuarioalta = EXCLUDED.idusuarioalta,
                    fechaalta = now();";

            DbPg.NonQuery(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idEmpresa", _idEmpresa);
                p.AddWithValue("idProductoGlobal", idProductoGlobal);
                p.AddWithValue("idProductoEmpresa", idProductoEmpresa);
                p.AddWithValue("idUsuarioAlta", (object)idUsuarioAlta ?? DBNull.Value);
            });
        }

        #endregion

        #region Formulas

        public DataTable buscarFormula(string texto)
        {
            const string sql = @"
                SELECT DISTINCT f.idformula, c.codigo, c.corte, f.creado, f.actualizado
                FROM corte c
                INNER JOIN formulas f ON c.idcorte = f.idembutido
                WHERE c.corte ILIKE @texto
                ORDER BY c.codigo;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("texto", "%" + (texto ?? "") + "%"));
        }

        public Formula findFormulaByID(int idFormula, int idEmbutido)
        {
            string sql = idFormula > 0
                ? "SELECT * FROM formulas WHERE idformula = @idFormula;"
                : "SELECT * FROM formulas WHERE idembutido = @idEmbutido;";

            var lista = DbPg.Reader(_connectionString, _idEmpresa, sql, dr =>
            {
                var oFormula = new Formula
                {
                    IdFormula = Convert.ToInt32(dr["idformula"]),
                    Embutido = findCorteById(Convert.ToInt32(dr["idembutido"]), false),
                    Receta = dr["receta"] == DBNull.Value ? "" : Convert.ToString(dr["receta"]),
                    Creado = dr["creado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["creado"]),
                    Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"])
                };

                int idCreadoPor = dr["creadopor"] == DBNull.Value ? 0 : Convert.ToInt32(dr["creadopor"]);
                int idActualizadoPor = dr["actualizadopor"] == DBNull.Value ? 0 : Convert.ToInt32(dr["actualizadopor"]);
                oFormula.CreadoPor = idCreadoPor > 0 ? GetUsuarioLiviano(idCreadoPor) : null;
                oFormula.ActualizadoPor = idActualizadoPor > 0 ? GetUsuarioLiviano(idActualizadoPor) : null;

                return oFormula;
            }, p =>
            {
                if (idFormula > 0) p.AddWithValue("idFormula", idFormula);
                else p.AddWithValue("idEmbutido", idEmbutido);
            });

            if (lista.Count == 0) return null;

            var formula = lista[0];
            formula.ListaCortesEnFormula = cargarCortesPorFormula(formula);
            return formula;
        }

        public List<CortePorFormula> cargarCortesPorFormula(Formula oFormula)
        {
            if (oFormula == null) throw new ArgumentNullException(nameof(oFormula));

            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM corteporformula WHERE idformula = @idFormula;",
                dr => new CortePorFormula
                {
                    IdCorteEnFormula = Convert.ToInt32(dr["idcorteporformula"]),
                    Formula = oFormula,
                    CorteEnFormula = findCorteById(Convert.ToInt32(dr["idcorte"]), false),
                    Porcentaje = Convert.ToSingle(dr["porcentaje"]),
                    AgregarAuto = dr["agregarauto"] != DBNull.Value && Convert.ToBoolean(dr["agregarauto"])
                },
                p => p.AddWithValue("idFormula", oFormula.IdFormula));
        }

        public int existeFormula(int idEmbutido)
        {
            object obj = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT idformula FROM formulas WHERE idembutido = @idEmbutido;",
                p => p.AddWithValue("idEmbutido", idEmbutido));

            return (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
        }

        public int addOrEditFormula(Formula oFormula, List<CortePorFormula> listaCortesPorFormula)
        {
            if (oFormula == null) throw new ArgumentNullException(nameof(oFormula));
            if (listaCortesPorFormula == null) listaCortesPorFormula = new List<CortePorFormula>();

            int idFormula = oFormula.IdFormula;

            if (idFormula == 0)
            {
                const string sqlInsert = @"
                    INSERT INTO formulas (idembutido, receta, creado, creadopor, idempresa)
                    VALUES (@idEmbutido, @receta, now(), @creadoPor, @idEmpresa)
                    RETURNING idformula;";

                object nuevoId = DbPg.Scalar(_connectionString, _idEmpresa, sqlInsert, p =>
                {
                    p.AddWithValue("idEmbutido", oFormula.Embutido.idCorte);
                    p.AddWithValue("receta", oFormula.Receta ?? "");
                    p.AddWithValue("creadoPor", oFormula.CreadoPor.Id);
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });
                idFormula = Convert.ToInt32(nuevoId);
            }
            else
            {
                const string sqlUpdate = @"
                    UPDATE formulas SET idembutido = @idEmbutido, receta = @receta, actualizado = now(), actualizadopor = @actualizadoPor
                    WHERE idformula = @idFormula;";

                DbPg.NonQuery(_connectionString, _idEmpresa, sqlUpdate, p =>
                {
                    p.AddWithValue("idEmbutido", oFormula.Embutido.idCorte);
                    p.AddWithValue("receta", oFormula.Receta ?? "");
                    p.AddWithValue("actualizadoPor", oFormula.ActualizadoPor != null ? oFormula.ActualizadoPor.Id : 0);
                    p.AddWithValue("idFormula", idFormula);
                });

                // La SP real borra el detalle antes de que el caller lo vuelva a insertar --
                // se replica igual (ver Datos/Corte.cs.addOrEditFormula, comentario original).
                DbPg.NonQuery(_connectionString, _idEmpresa,
                    "DELETE FROM corteporformula WHERE idformula = @idFormula;",
                    p => p.AddWithValue("idFormula", idFormula));
            }

            oFormula.IdFormula = idFormula;

            foreach (var item in listaCortesPorFormula)
            {
                DbPg.NonQuery(_connectionString, _idEmpresa,
                    "INSERT INTO corteporformula (idformula, idcorte, porcentaje, agregarauto, idempresa) VALUES (@idFormula, @idCorte, @porcentaje, @agregarAuto, @idEmpresa);",
                    p =>
                    {
                        p.AddWithValue("idFormula", idFormula);
                        p.AddWithValue("idCorte", item.CorteEnFormula1.idCorte);
                        p.AddWithValue("porcentaje", item.Porcentaje);
                        p.AddWithValue("agregarAuto", item.AgregarAuto);
                        p.AddWithValue("idEmpresa", _idEmpresa);
                    });
            }

            return idFormula;
        }

        public void eliminarFormula(int idFormula)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM corteporformula WHERE idformula = @idFormula;",
                p => p.AddWithValue("idFormula", idFormula));

            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM formulas WHERE idformula = @idFormula;",
                p => p.AddWithValue("idFormula", idFormula));
        }

        public DataTable getFormulaEmbutido(int idEmbutido)
        {
            const string sql = @"
                SELECT c.idcorte, c.codigo, c.corte, cpf.porcentaje, '' AS kgs, cpf.agregarauto
                FROM formulas f
                INNER JOIN corteporformula cpf ON f.idformula = cpf.idformula
                INNER JOIN corte c ON cpf.idcorte = c.idcorte
                WHERE f.idembutido = @idEmbutido
                ORDER BY cpf.agregarauto DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("idEmbutido", idEmbutido));
        }

        #endregion

        #region Alicuotas IVA

        public DataTable obtenerAlicuotasIva(bool mostrarTodos)
        {
            string sql = "SELECT idiva, iva FROM alicuotasiva";
            sql += mostrarTodos ? " ORDER BY orden;" : " WHERE mostrar = true ORDER BY orden;";
            return DbPg.DataTable(_connectionString, _idEmpresa, sql);
        }

        public AlicuotaIva findAlicuotaIvaById(int idIva)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM alicuotasiva WHERE idiva = @idIva;",
                dr => new AlicuotaIva
                {
                    // Fiel al original: Datos/Corte.cs.findAlicuotaIvaById asigna Convert.ToInt32
                    // a una propiedad float (trunca/redondea decimales) -- se replica igual.
                    IdIva = Convert.ToInt32(dr["idiva"]),
                    Iva = Convert.ToInt32(dr["iva"]),
                    Orden = Convert.ToInt32(dr["orden"]),
                    Mostrar = Convert.ToBoolean(dr["mostrar"])
                },
                p => p.AddWithValue("idIva", idIva));

            return lista.Count > 0 ? lista[0] : null;
        }

        #endregion

        #region Tipos de producto

        public DataTable obtenerTiposProducto(bool mostrarTodos)
        {
            string sql = "SELECT tipo FROM tiposproducto WHERE (reservadosistema = true OR idempresa = @idEmpresa)";
            sql += mostrarTodos ? " ORDER BY orden, tipo;" : " AND orden > 0 ORDER BY orden, tipo;";
            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("idEmpresa", _idEmpresa));
        }

        public DataTable obtenerTiposProductoGrilla(string buscarText)
        {
            const string sql = @"
                SELECT tipo, orden, creado AS ""Creado"", actualizado AS ""Actualizado"", reservadosistema AS ""Reservado""
                FROM tiposproducto
                WHERE (@buscar IS NULL OR tipo ILIKE @buscar)
                ORDER BY orden, tipo;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("buscar", string.IsNullOrWhiteSpace(buscarText) ? (object)DBNull.Value : "%" + buscarText.Trim() + "%");
            });
        }

        public DataTable obtenerTiposProductoGrillaEmpresa(string buscarText)
        {
            const string sql = @"
                SELECT tipo, orden, creado AS ""Creado"", actualizado AS ""Actualizado"", reservadosistema AS ""Reservado""
                FROM tiposproducto
                WHERE (reservadosistema = true OR idempresa = @idEmpresa)
                  AND (@buscar IS NULL OR tipo ILIKE @buscar)
                ORDER BY orden, tipo;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idEmpresa", _idEmpresa);
                p.AddWithValue("buscar", string.IsNullOrWhiteSpace(buscarText) ? (object)DBNull.Value : "%" + buscarText.Trim() + "%");
            });
        }

        public DataTable obtenerTiposProductoCatalogoGlobal(string buscarText)
        {
            const string sql = @"
                SELECT tipo, orden
                FROM tiposproducto
                WHERE idempresa = 0 AND reservadosistema = false
                  AND (@buscar IS NULL OR tipo ILIKE @buscar)
                ORDER BY orden, tipo;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("buscar", string.IsNullOrWhiteSpace(buscarText) ? (object)DBNull.Value : "%" + buscarText.Trim() + "%");
            });
        }

        public string importarTiposProductoGlobales(IEnumerable<string> tiposProducto, int? idUsuarioAlta)
        {
            var tiposNormalizados = (tiposProducto ?? Enumerable.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!tiposNormalizados.Any()) return "";

            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    foreach (string tipo in tiposNormalizados)
                    {
                        int existe;
                        using (var cmdExiste = new NpgsqlCommand(
                            "SELECT COUNT(*) FROM tiposproducto WHERE idempresa = @idEmpresa AND TRIM(tipo) = TRIM(@tipo);", con, tx))
                        {
                            cmdExiste.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                            cmdExiste.Parameters.AddWithValue("tipo", tipo);
                            existe = Convert.ToInt32(cmdExiste.ExecuteScalar());
                        }
                        if (existe > 0) continue;

                        using (var cmdInsert = new NpgsqlCommand(@"
                            INSERT INTO tiposproducto (tipo, orden, reservadosistema, creado, idempresa)
                            SELECT tipo, orden, false, @creado, @idEmpresa
                            FROM tiposproducto
                            WHERE idempresa = 0 AND reservadosistema = false AND TRIM(tipo) = TRIM(@tipo)
                            LIMIT 1;", con, tx))
                        {
                            cmdInsert.Parameters.AddWithValue("tipo", tipo);
                            cmdInsert.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                            cmdInsert.Parameters.AddWithValue("creado", DateTime.Now);

                            int insertados = cmdInsert.ExecuteNonQuery();
                            if (insertados <= 0)
                            {
                                tx.Rollback();
                                return "No se pudo importar el tipo de producto \"" + tipo + "\" desde el catálogo global.";
                            }
                        }
                    }

                    tx.Commit();
                    return "";
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
                    throw;
                }
            }
        }

        public string addOrEditTipoProducto(string tiposProducto, string orden, bool esInsert, string tipoToUpdate)
        {
            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    if (esInsert)
                    {
                        using (var cmdDup = new NpgsqlCommand(
                            "SELECT COUNT(*) FROM tiposproducto WHERE TRIM(tipo) = TRIM(@tipo) AND (reservadosistema = true OR idempresa = @idEmpresa);", con, tx))
                        {
                            cmdDup.Parameters.AddWithValue("tipo", tiposProducto ?? "");
                            cmdDup.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                            int existe = Convert.ToInt32(cmdDup.ExecuteScalar());
                            if (existe != 0)
                            {
                                tx.Rollback();
                                return "Ya existe un Tipo con el mismo nombre.";
                            }
                        }

                        using (var cmd = new NpgsqlCommand(
                            "INSERT INTO tiposproducto (tipo, orden, reservadosistema, creado, idempresa) VALUES (@tipo, @orden, @reservadoSistema, @creado, @idEmpresa);", con, tx))
                        {
                            cmd.Parameters.AddWithValue("tipo", tiposProducto ?? "");
                            cmd.Parameters.AddWithValue("orden", int.TryParse(orden, out int ordenVal) ? ordenVal : 0);
                            cmd.Parameters.AddWithValue("reservadoSistema", false);
                            cmd.Parameters.AddWithValue("creado", DateTime.Now);
                            cmd.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmd1 = new NpgsqlCommand(
                            "UPDATE tiposproducto SET tipo = @tipo, orden = @orden, actualizado = @actualizado WHERE tipo ILIKE @tipoToUpdate AND reservadosistema = false AND idempresa = @idEmpresa;", con, tx))
                        {
                            cmd1.Parameters.AddWithValue("tipo", tiposProducto ?? "");
                            cmd1.Parameters.AddWithValue("orden", int.TryParse(orden, out int ordenVal) ? ordenVal : 0);
                            cmd1.Parameters.AddWithValue("actualizado", DateTime.Now);
                            cmd1.Parameters.AddWithValue("tipoToUpdate", tipoToUpdate ?? "");
                            cmd1.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                            cmd1.ExecuteNonQuery();
                        }

                        using (var cmd2 = new NpgsqlCommand(
                            "UPDATE corte SET tipo = @tipo WHERE tipo ILIKE @tipoToUpdate AND idempresa = @idEmpresa;", con, tx))
                        {
                            cmd2.Parameters.AddWithValue("tipo", tiposProducto ?? "");
                            cmd2.Parameters.AddWithValue("tipoToUpdate", tipoToUpdate ?? "");
                            cmd2.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                            cmd2.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                    return "";
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
                    throw;
                }
            }
        }

        public string eliminarTipoProducto(string tiposProducto)
        {
            object existeObj = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT COUNT(*) FROM corte WHERE tipo = @tipo AND idempresa = @idEmpresa;",
                p =>
                {
                    p.AddWithValue("tipo", tiposProducto ?? "");
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });

            int existe = existeObj == null || existeObj == DBNull.Value ? 0 : Convert.ToInt32(existeObj);
            if (existe != 0)
                return "Existen Productos/Cortes con el Tipo que quiere eliminar.\n\nPara poder eliminar el Tipo debe cambiar todo los Productos/Cortes asociados a éste.";

            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM tiposproducto WHERE tipo = @tipo AND reservadosistema = false AND idempresa = @idEmpresa;",
                p =>
                {
                    p.AddWithValue("tipo", tiposProducto ?? "");
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });

            return "";
        }

        #endregion
    }
}
