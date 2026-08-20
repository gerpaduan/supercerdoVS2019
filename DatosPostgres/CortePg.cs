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
                  AND (@buscar::text IS NULL OR tipo ILIKE @buscar::text)
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
                  AND (@buscar::text IS NULL OR tipo ILIKE @buscar::text)
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

        #region Embutido (Etapa 11a)

        // obtenerEmbutidos NO se implementa aca: no esta en ICorteRepository (ver el comentario
        // de cabecera del archivo y Contratos/ICorteRepository.cs) -- codigo muerto y el SP real
        // siempre devuelve 0 filas en SQL Server (INNER JOIN contra StockCorteSucursal, vacia).

        private Sucursal GetSucursalLiviana(int idSucursal)
        {
            if (idSucursal <= 0) return null;

            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT idsucursal, sucursal FROM sucursal WHERE idsucursal = @idSucursal;",
                dr => new Sucursal
                {
                    idSucursal = Convert.ToInt32(dr["idsucursal"]),
                    SucursalNombre = dr["sucursal"] as string
                },
                p => p.AddWithValue("idSucursal", idSucursal));

            return lista.Count > 0 ? lista[0] : null;
        }

        public DataTable getListaElegirEmbutido()
        {
            const string sql = @"
                SELECT idcorte AS idcorteembutido, codigo AS codigoembutido, corte AS corteembutido
                FROM corte WHERE ingresorapidoembutido = true ORDER BY codigo;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql);
        }

        public DataTable buscarEmbutido(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = @"
                SELECT e.idembutido AS ""Id"", e.fechaembutido AS ""Fecha"", ce.codigo AS ""Codigo"", ce.corte AS ""Corte"",
                       SUM(cpe.kgutilizados) AS ""Kgs"", s.sucursal AS ""Sucursal"", e.estado AS ""Estado"",
                       (CASE WHEN length(COALESCE(e.observaciones, '')) <= 20 THEN e.observaciones
                             ELSE substring(e.observaciones from 1 for 20) || '...' END) AS ""Observaciones"",
                       e.creado AS ""Creado"", cp.nombre AS ""Creado Por"", e.actualizado AS ""Actualizado"", ap.nombre AS ""Actualizado Por""
                FROM corte c
                INNER JOIN corteporembutido cpe ON c.idcorte = cpe.idcorte
                INNER JOIN embutidos e ON cpe.idembutido = e.idembutido
                INNER JOIN corte ce ON e.idcorte = ce.idcorte
                INNER JOIN sucursal s ON e.idsucursal = s.idsucursal
                LEFT JOIN usuarios cp ON e.creadopor = cp.id
                LEFT JOIN usuarios ap ON e.actualizadopor = ap.id
                WHERE e.fechaembutido BETWEEN @fechaDesde AND @fechaHasta
                  AND ((@idSucursal > 0 AND s.idsucursal = @idSucursal) OR (@idSucursal <= 0 AND e.idsucursal > 0))
                  AND (ce.codigo::text = @texto OR ce.corte ILIKE '%' || @texto || '%'
                       OR cp.nombre ILIKE '%' || @texto || '%' OR ap.nombre ILIKE '%' || @texto || '%')
                GROUP BY e.idembutido, e.fechaembutido, ce.codigo, ce.corte, s.sucursal, e.creado, cp.nombre,
                         e.actualizado, ap.nombre, e.estado, e.observaciones
                ORDER BY e.fechaembutido, e.creado DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("texto", texto ?? "");
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
            });
        }

        public DataTable obtenerUltimosElaboradosDashboard(int cantidad, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = @"
                SELECT e.idembutido AS ""Id"", e.fechaembutido AS ""Fecha"", ce.corte AS ""Corte"",
                       SUM(cpe.kgutilizados) AS ""Kgs"", s.sucursal AS ""Sucursal"", uc.nombre AS ""Usuario""
                FROM embutidos e
                INNER JOIN corteporembutido cpe ON cpe.idembutido = e.idembutido
                INNER JOIN corte ce ON ce.idcorte = e.idcorte
                INNER JOIN sucursal s ON s.idsucursal = e.idsucursal
                LEFT JOIN usuarios uc ON uc.id = e.creadopor
                WHERE e.fechaembutido BETWEEN @fechaDesde AND @fechaHasta
                  AND ((@idSucursal > 0 AND e.idsucursal = @idSucursal) OR (@idSucursal <= 0 AND e.idsucursal > 0))
                GROUP BY e.idembutido, e.fechaembutido, ce.corte, s.sucursal, uc.nombre
                ORDER BY e.fechaembutido DESC, e.idembutido DESC
                LIMIT @cantidad;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("cantidad", cantidad);
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
            });
        }

        public DataTable obtenerLineasEmb(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = @"
                SELECT e.idembutido AS ""Id"", e.fechaembutido AS ""Fecha"", ce.codigo AS ""Cod.Emb"", ce.corte AS ""Embutido"",
                       c.codigo AS ""Codigo"", c.corte AS ""Corte"", cpe.kgutilizados AS ""Kgs"", cpe.pesobalanza AS ""Balanza"",
                       s.sucursal AS ""Sucursal"", e.estado AS ""Estado"",
                       (CASE WHEN length(COALESCE(e.observaciones, '')) <= 20 THEN e.observaciones
                             ELSE substring(e.observaciones from 1 for 20) || '...' END) AS ""Observaciones"",
                       e.creado AS ""Creado"", cp.nombre AS ""Creado Por"", e.actualizado AS ""Actualizado"", ap.nombre AS ""Actualizado Por""
                FROM corte c
                INNER JOIN corteporembutido cpe ON c.idcorte = cpe.idcorte
                INNER JOIN embutidos e ON cpe.idembutido = e.idembutido
                INNER JOIN corte ce ON e.idcorte = ce.idcorte
                INNER JOIN sucursal s ON e.idsucursal = s.idsucursal
                LEFT JOIN usuarios cp ON e.creadopor = cp.id
                LEFT JOIN usuarios ap ON e.actualizadopor = ap.id
                WHERE e.fechaembutido BETWEEN @fechaDesde AND @fechaHasta
                  AND ((@idSucursal > 0 AND s.idsucursal = @idSucursal) OR e.idsucursal > 0)
                  AND (e.idembutido::text = @texto OR ce.codigo::text = @texto OR ce.corte ILIKE '%' || @texto || '%'
                       OR c.codigo::text = @texto OR c.corte ILIKE '%' || @texto || '%'
                       OR cp.nombre ILIKE '%' || @texto || '%' OR ap.nombre ILIKE '%' || @texto || '%')
                ORDER BY e.fechaembutido DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("texto", texto ?? "");
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
            });
        }

        // Sin trocear en lotes de 500 (arrays nativos de Postgres, mismo criterio ya aplicado en
        // Etapa 9 con obtenerPesajesVinculadosPorDestinos).
        public HashSet<int> ObtenerIdsEmbutidosIngresoRapido(IEnumerable<int> idsEmbutidos)
        {
            var resultado = new HashSet<int>();
            var ids = (idsEmbutidos ?? Enumerable.Empty<int>()).Where(x => x > 0).Distinct().ToArray();
            if (ids.Length == 0) return resultado;

            var filas = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT e.idembutido FROM embutidos e INNER JOIN corte c ON c.idcorte = e.idcorte " +
                "WHERE c.ingresorapidoembutido = true AND e.idembutido = ANY(@ids);",
                dr => Convert.ToInt32(dr["idembutido"]),
                p => p.AddWithValue("ids", ids));

            foreach (int id in filas) resultado.Add(id);
            return resultado;
        }

        public DataTable obtenerInfoCorte(int idCorte)
        {
            const string sql = @"
                SELECT cp.idcorte, cp.codigo, cp.corte, cp.preciokg, cp.nivel, cp.independiente, cp.ingresorapidoembutido,
                       cp.habilitado, cp.encierrestock, cp.tipo, cp.idcortemaestro, cm.corte AS cortemaestro, cp.porcentaje,
                       cp.porcentajehueso, cp.desvioestandar, cp.promedio, cp.alicuotaiva, cp.idmarca, p.identificacion AS marca
                FROM corte cp
                LEFT JOIN personas p ON cp.idmarca = p.idpersona
                LEFT JOIN corte cm ON cp.idcortemaestro = cm.idcorte
                WHERE cp.idcorte = @idCorte;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("idCorte", idCorte));
        }

        public DataTable obtenerCorteProveedor(int idCorte)
        {
            const string sql = @"
                SELECT p.razonsocial, cpr.ultimoprecio, cpr.fechaultimacompra
                FROM corte c
                INNER JOIN corteproveedor cpr ON c.idcorte = cpr.idcorte
                INNER JOIN personas p ON cpr.idproveedor = p.idpersona
                WHERE cpr.idcorte = @idCorte
                ORDER BY cpr.fechaultimacompra DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("idCorte", idCorte));
        }

        public DataTable obtenerCortesPorProveedor(int idProveedor)
        {
            const string sql = @"
                SELECT DISTINCT cpc.idcorte
                FROM compras c
                INNER JOIN corteporcompra cpc ON cpc.idcompra = c.idcompra
                WHERE c.idproveedor = @idProveedor AND COALESCE(c.estado, '') = '';";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("idProveedor", idProveedor));
        }

        public Embutido findEmbutidoById(int idEmbutido)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM embutidos WHERE idembutido = @idEmbutido;",
                dr =>
                {
                    var o = new Embutido();
                    o.IdEmbutido = Convert.ToInt32(dr["idembutido"]);
                    o.FechaEmbutido = Convert.ToDateTime(dr["fechaembutido"]);
                    o.Corte = findCorteById(Convert.ToInt32(dr["idcorte"]), true);
                    o.Sucursal = GetSucursalLiviana(Convert.ToInt32(dr["idsucursal"]));
                    o.Observaciones = GetString(dr, "observaciones");
                    o.Estado = GetString(dr, "estado");
                    o.Creado = Convert.ToDateTime(dr["creado"]);
                    o.Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"]);

                    int idCreadoPor = dr["creadopor"] == DBNull.Value ? 0 : Convert.ToInt32(dr["creadopor"]);
                    int idActualizadoPor = dr["actualizadopor"] == DBNull.Value ? 0 : Convert.ToInt32(dr["actualizadopor"]);
                    o.CreadoPor = idCreadoPor > 0 ? GetUsuarioLiviano(idCreadoPor) : null;
                    o.ActualizadoPor = idActualizadoPor > 0 ? GetUsuarioLiviano(idActualizadoPor) : null;

                    return o;
                },
                p => p.AddWithValue("idEmbutido", idEmbutido));

            if (lista.Count == 0) return null;

            var emb = lista[0];
            emb.CortesEnEmbutido = ObtenerCortesEnEmbutido(emb);
            return emb;
        }

        private List<CortePorEmbutido> ObtenerCortesEnEmbutido(Embutido oEmbutidoParam)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                @"SELECT cpe.idcorteembutido, cpe.idembutido, cpe.idcorte, cpe.kgutilizados, cpe.pesobalanza, c.codigo, c.corte, c.tipo
                  FROM corteporembutido cpe
                  INNER JOIN corte c ON c.idcorte = cpe.idcorte
                  WHERE cpe.idembutido = @idEmbutido;",
                dr => new CortePorEmbutido
                {
                    IdCorteEmbutido = Convert.ToInt32(dr["idcorteembutido"]),
                    Embutido = oEmbutidoParam,
                    Corte = new Corte
                    {
                        IdCorte = Convert.ToInt32(dr["idcorte"]),
                        Codigo = dr["codigo"] == DBNull.Value ? 0L : Convert.ToInt64(dr["codigo"]),
                        CorteDesc = GetString(dr, "corte"),
                        Tipo = GetString(dr, "tipo")
                    },
                    KgUtilizado = GetFloat(dr, "kgutilizados"),
                    PesoBalanza = GetBool(dr, "pesobalanza")
                },
                p => p.AddWithValue("idEmbutido", oEmbutidoParam.idEmbutido));
        }

        public int agregarEmbutido(Embutido oEmbutido)
        {
            if (oEmbutido == null) throw new ArgumentNullException(nameof(oEmbutido));

            const string sql = @"
                INSERT INTO embutidos (fechaembutido, idcorte, idsucursal, estado, observaciones, creado, creadopor, idempresa)
                VALUES (@fechaEmbutido, @idCorte, @idSucursal, '', @observaciones, now(), @creadoPor, @idEmpresa)
                RETURNING idembutido;";

            object obj = DbPg.Scalar(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("fechaEmbutido", oEmbutido.fechaEmbutido);
                p.AddWithValue("idCorte", oEmbutido.corte.idCorte);
                p.AddWithValue("idSucursal", oEmbutido.sucursal.IdSucursal);
                p.AddWithValue("creadoPor", oEmbutido.CreadoPor.Id);
                p.AddWithValue("observaciones", oEmbutido.observaciones ?? "");
                p.AddWithValue("idEmpresa", _idEmpresa);
            });

            return obj == null || obj == DBNull.Value ? 0 : Convert.ToInt32(obj);
        }

        public void anularEmbutido(Embutido oEmbutidoE)
        {
            if (oEmbutidoE == null) throw new ArgumentNullException(nameof(oEmbutidoE));

            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE embutidos SET estado = 'Anulado', actualizado = now(), actualizadopor = @actualizadoPor WHERE idembutido = @idEmbutido;",
                p =>
                {
                    p.AddWithValue("idEmbutido", oEmbutidoE.idEmbutido);
                    p.AddWithValue("actualizadoPor", oEmbutidoE.ActualizadoPor.Id);
                });
        }

        public DataTable obtenerCortesPorEmbutidos(Embutido oEmbutidoE)
        {
            const string sql = @"
                SELECT cpe.idembutido, cpe.idcorte, c.codigo, c.corte, cpe.kgutilizados, cpe.pesobalanza
                FROM corte c
                INNER JOIN corteporembutido cpe ON c.idcorte = cpe.idcorte
                INNER JOIN embutidos e ON cpe.idembutido = e.idembutido
                WHERE e.idembutido = @idEmbutido;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("idEmbutido", oEmbutidoE.idEmbutido));
        }

        // Solo replica la parte real del SP (INSERT INTO CortePorEmbutido) -- las 8 UPDATE
        // StockCorteSucursal (cascada de stock del corte usado en el embutido) son no-op,
        // StockCorteSucursal nunca se porta a Postgres (Etapa 6).
        public void agregarCortePorEmbutido(CortePorEmbutido oCortePorEmbutido)
        {
            if (oCortePorEmbutido == null) throw new ArgumentNullException(nameof(oCortePorEmbutido));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO corteporembutido (idembutido, idcorte, kgutilizados, pesobalanza, idempresa)
                VALUES (@idEmbutido, @idCorte, @kgUtilizados, @pesoBalanza, @idEmpresa);", p =>
            {
                p.AddWithValue("idEmbutido", oCortePorEmbutido.embutido.idEmbutido);
                p.AddWithValue("idCorte", oCortePorEmbutido.corte.idCorte);
                p.AddWithValue("kgUtilizados", oCortePorEmbutido.kgUtilizado);
                p.AddWithValue("pesoBalanza", oCortePorEmbutido.PesoBalanza);
                p.AddWithValue("idEmpresa", _idEmpresa);
            });
        }

        #endregion

        #region Movimiento (Etapa 11b)

        // EstadoPendiente_valor: el SP real addOrEditMovimiento recibe este parametro con
        // default 2, y Datos.Corte nunca lo pasa explicito -- siempre usa el default. Se
        // hardcodea aca, mismo comportamiento real observado.
        private const int EstadoPendienteValor = 2;

        public int addOrEditMovimiento(Movimiento oMovimientoE)
        {
            if (oMovimientoE == null) throw new ArgumentNullException(nameof(oMovimientoE));

            if (oMovimientoE.IdMovimiento == 0)
            {
                const string sqlInsert = @"
                    INSERT INTO movimiento (fechamovimiento, sucursalorigen, sucursaldestino, actualizacioncompleta, observaciones, creado, creadopor, idempresa)
                    VALUES (@fechaMovimiento, @sucursalOrigen, @sucursalDestino, @estadoPendiente, @observaciones, now(), @creadoPor, @idEmpresa)
                    RETURNING idmovimiento;";

                object obj = DbPg.Scalar(_connectionString, _idEmpresa, sqlInsert, p =>
                {
                    p.AddWithValue("fechaMovimiento", oMovimientoE.FechaMovimiento);
                    p.AddWithValue("sucursalOrigen", oMovimientoE.SucursalOrigen.idSucursal);
                    p.AddWithValue("sucursalDestino", oMovimientoE.SucursalDestino.idSucursal);
                    p.AddWithValue("estadoPendiente", EstadoPendienteValor);
                    p.AddWithValue("observaciones", oMovimientoE.Observaciones ?? "");
                    p.AddWithValue("creadoPor", oMovimientoE.CreadoPor.Id);
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });

                oMovimientoE.IdMovimiento = obj == null || obj == DBNull.Value ? 0 : Convert.ToInt32(obj);
                return oMovimientoE.IdMovimiento;
            }

            // Edicion real: snapshot en MovimientoHistorial (una fila por linea actual) + UPDATE
            // + ajuste de actualizacionCompleta (solo si es un movimiento raiz) + limpieza de
            // CortePorMovimiento (el caller vuelve a cargar las lineas) -- misma secuencia de 4
            // statements que el SP original, en una transaccion.
            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    using (var cmdHist = new NpgsqlCommand(@"
                        INSERT INTO movimientohistorial (idmovimiento, fechamovimiento, idsucorigen, idsucdestino, idcorte, cantkg, cantunidad, pesobalanza, actualizadopor, actualizado, observaciones, idempresa)
                        SELECT m.idmovimiento, m.fechamovimiento, m.sucursalorigen, m.sucursaldestino, cpm.idcorte, cpm.cantkg, cpm.cantunidad, cpm.pesobalanza, @actualizadoPor, now(), m.observaciones, @idEmpresa
                        FROM movimiento m
                        INNER JOIN cortepormovimiento cpm ON m.idmovimiento = cpm.idmovimientos
                        WHERE m.idmovimiento = @idMovimiento;", con, tx))
                    {
                        cmdHist.Parameters.AddWithValue("actualizadoPor", oMovimientoE.ActualizadoPor.Id);
                        cmdHist.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                        cmdHist.Parameters.AddWithValue("idMovimiento", oMovimientoE.IdMovimiento);
                        cmdHist.ExecuteNonQuery();
                    }

                    using (var cmdUpd = new NpgsqlCommand(@"
                        UPDATE movimiento SET fechamovimiento=@fechaMovimiento, sucursalorigen=@sucursalOrigen, sucursaldestino=@sucursalDestino,
                            observaciones=@observaciones, actualizado=now(), actualizadopor=@actualizadoPor
                        WHERE idmovimiento=@idMovimiento;", con, tx))
                    {
                        cmdUpd.Parameters.AddWithValue("fechaMovimiento", oMovimientoE.FechaMovimiento);
                        cmdUpd.Parameters.AddWithValue("sucursalOrigen", oMovimientoE.SucursalOrigen.idSucursal);
                        cmdUpd.Parameters.AddWithValue("sucursalDestino", oMovimientoE.SucursalDestino.idSucursal);
                        cmdUpd.Parameters.AddWithValue("observaciones", oMovimientoE.Observaciones ?? "");
                        cmdUpd.Parameters.AddWithValue("actualizadoPor", oMovimientoE.ActualizadoPor.Id);
                        cmdUpd.Parameters.AddWithValue("idMovimiento", oMovimientoE.IdMovimiento);
                        cmdUpd.ExecuteNonQuery();
                    }

                    using (var cmdEstado = new NpgsqlCommand(
                        "UPDATE movimiento SET actualizacioncompleta = @estadoPendiente WHERE idmovimiento = @idMovimiento AND idmovorigen IS NULL;", con, tx))
                    {
                        cmdEstado.Parameters.AddWithValue("estadoPendiente", EstadoPendienteValor);
                        cmdEstado.Parameters.AddWithValue("idMovimiento", oMovimientoE.IdMovimiento);
                        cmdEstado.ExecuteNonQuery();
                    }

                    using (var cmdDel = new NpgsqlCommand("DELETE FROM cortepormovimiento WHERE idmovimientos = @idMovimiento;", con, tx))
                    {
                        cmdDel.Parameters.AddWithValue("idMovimiento", oMovimientoE.IdMovimiento);
                        cmdDel.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
                    throw;
                }
            }

            return oMovimientoE.IdMovimiento;
        }

        // Sin caller vivo hoy (wrapper comentado en Negocio/Corte.cs), pero real y sin StockCorteSucursal.
        public void modificarMovimiento(Movimiento oMovimientoE)
        {
            if (oMovimientoE == null) throw new ArgumentNullException(nameof(oMovimientoE));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE movimiento SET fechamovimiento=@fechaMovimiento, sucursalorigen=@sucursalOrigen, sucursaldestino=@sucursalDestino,
                    observaciones=@observaciones, actualizado=now(), actualizadopor=@actualizadoPor
                WHERE idmovimiento=@idMovimiento;", p =>
            {
                p.AddWithValue("idMovimiento", oMovimientoE.IdMovimiento);
                p.AddWithValue("fechaMovimiento", oMovimientoE.FechaMovimiento);
                p.AddWithValue("sucursalOrigen", oMovimientoE.SucursalOrigen.idSucursal);
                p.AddWithValue("sucursalDestino", oMovimientoE.SucursalDestino.idSucursal);
                p.AddWithValue("observaciones", oMovimientoE.Observaciones ?? "");
                p.AddWithValue("actualizadoPor", oMovimientoE.ActualizadoPor.Id);
            });
        }

        public void eliminarMovimiento(int idMovimiento, Usuario oUsuario)
        {
            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    using (var cmdHist = new NpgsqlCommand(@"
                        INSERT INTO movimientohistorial (idmovimiento, fechamovimiento, idsucorigen, idsucdestino, idcorte, cantkg, cantunidad, pesobalanza, actualizadopor, actualizado, observaciones, idempresa)
                        SELECT m.idmovimiento, m.fechamovimiento, m.sucursalorigen, m.sucursaldestino, cpm.idcorte, cpm.cantkg, cpm.cantunidad, cpm.pesobalanza, @actualizadoPor, now(), m.observaciones, @idEmpresa
                        FROM movimiento m
                        INNER JOIN cortepormovimiento cpm ON m.idmovimiento = cpm.idmovimientos
                        WHERE m.idmovimiento = @idMovimiento;", con, tx))
                    {
                        cmdHist.Parameters.AddWithValue("actualizadoPor", oUsuario.Id);
                        cmdHist.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                        cmdHist.Parameters.AddWithValue("idMovimiento", idMovimiento);
                        cmdHist.ExecuteNonQuery();
                    }

                    using (var cmdDelLineas = new NpgsqlCommand("DELETE FROM cortepormovimiento WHERE idmovimientos = @idMovimiento;", con, tx))
                    {
                        cmdDelLineas.Parameters.AddWithValue("idMovimiento", idMovimiento);
                        cmdDelLineas.ExecuteNonQuery();
                    }

                    using (var cmdDelMov = new NpgsqlCommand("DELETE FROM movimiento WHERE idmovimiento = @idMovimiento;", con, tx))
                    {
                        cmdDelMov.Parameters.AddWithValue("idMovimiento", idMovimiento);
                        cmdDelMov.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
                    throw;
                }
            }
        }

        // Solo replica la parte real del SP (INSERT). El resto (cascada StockCorteSucursal) ya
        // esta comentado en el propio SP de SQL Server -- no-op ya deshabilitado en origen.
        public void agregarCortePorMovimiento(CortePorMovimiento cortePorMovimiento)
        {
            if (cortePorMovimiento == null) throw new ArgumentNullException(nameof(cortePorMovimiento));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO cortepormovimiento (idmovimientos, idcorte, cantkg, cantunidad, pesobalanza, permitiringreso, idempresa)
                VALUES (@idMovimiento, @idCorte, @cantKg, @cantUnidad, @pesoBalanza, @permitirIngreso, @idEmpresa);", p =>
            {
                p.AddWithValue("idMovimiento", cortePorMovimiento.Movimientos.IdMovimiento);
                p.AddWithValue("idCorte", cortePorMovimiento.Corte.IdCorte);
                p.AddWithValue("cantKg", cortePorMovimiento.CantKg);
                p.AddWithValue("cantUnidad", cortePorMovimiento.CantUnidad);
                p.AddWithValue("pesoBalanza", cortePorMovimiento.PesoBalanza);
                p.AddWithValue("permitirIngreso", cortePorMovimiento.PermitirIngreso ? 1 : 0);
                p.AddWithValue("idEmpresa", _idEmpresa);
            });
        }

        // Sin caller vivo hoy (wrapper comentado en Negocio/Corte.cs). Solo replica la parte
        // real del SP (DELETE final); el resto (cascada StockCorteSucursal) es no-op, Etapa 6.
        public void quitarCortesPorMovimiento(Movimiento oMovimientoE)
        {
            if (oMovimientoE == null) throw new ArgumentNullException(nameof(oMovimientoE));

            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM cortepormovimiento WHERE idmovimientos = @idMovimiento;",
                p => p.AddWithValue("idMovimiento", oMovimientoE.IdMovimiento));
        }

        public DataTable obtenerMovimientos(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            // Precedencia AND/OR preservada tal cual el SP original en las dos ramas (no es un
            // reformateo cosmetico): en la rama 1 el filtro de texto es independiente del rango
            // de fecha/sucursales; en la rama 2 el "sin lineas" (count=0) esta agrupado con
            // fecha/sucursales, y esa agrupacion completa se OR-ea con el filtro de texto.
            const string sql = @"
                (SELECT m.idmovimiento AS ""Id Movimiento"", m.fechamovimiento AS ""Fecha Movimiento"", so.sucursal AS ""Origen"", m.idmovorigen AS ""Id Origen"",
                        (CASE WHEN m.actualizacioncompleta = 2 THEN 'PENDIENTE'
                              WHEN (m.idmovorigen > 0 AND m.actualizacioncompleta = 1) OR m.idmovorigen IS NULL THEN 'OK'
                              ELSE 'ERROR' END) AS ""Estado"",
                        sd.sucursal AS ""Destino"", SUM(cpm.cantunidad) AS ""Total Un."", CAST(SUM(cpm.cantkg) AS numeric(10,3)) AS ""Total Kg"",
                        m.observaciones, m.creado, cp.nombre AS ""creado por"", m.actualizado, ap.nombre AS ""actualizado por""
                 FROM cortepormovimiento cpm
                 INNER JOIN movimiento m ON cpm.idmovimientos = m.idmovimiento
                 INNER JOIN sucursal so ON so.idsucursal = m.sucursalorigen
                 INNER JOIN sucursal sd ON m.sucursaldestino = sd.idsucursal
                 LEFT JOIN usuarios ap ON m.actualizadopor = ap.id
                 LEFT JOIN usuarios cp ON m.creadopor = cp.id
                 WHERE (m.fechamovimiento BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                        AND so.sucursal ILIKE '%' || @sucOrigen || '%' AND sd.sucursal ILIKE '%' || @sucDestino || '%')
                    OR (m.idmovimiento::text ILIKE @texto)
                 GROUP BY m.idmovimiento, m.fechamovimiento, so.sucursal, m.idmovorigen, m.actualizacioncompleta, sd.sucursal,
                          m.observaciones, m.creado, cp.nombre, m.actualizado, ap.nombre)

                UNION

                (SELECT m.idmovimiento AS ""Id Movimiento"", m.fechamovimiento AS ""Fecha Movimiento"", so.sucursal AS ""Origen"", m.idmovorigen AS ""Id Origen"",
                        (CASE WHEN m.idmovorigen > 0 AND m.actualizacioncompleta = 0 THEN 'Error' ELSE 'OK' END) AS ""Estado"",
                        sd.sucursal AS ""Destino"", 0 AS ""Total Un."", 0 AS ""Total Kg"",
                        m.observaciones, m.creado, cp.nombre AS ""creado por"", m.actualizado, ap.nombre AS ""actualizado por""
                 FROM movimiento m
                 INNER JOIN sucursal so ON so.idsucursal = m.sucursalorigen
                 INNER JOIN sucursal sd ON m.sucursaldestino = sd.idsucursal
                 LEFT JOIN usuarios ap ON m.actualizadopor = ap.id
                 LEFT JOIN usuarios cp ON m.creadopor = cp.id
                 WHERE ((SELECT COUNT(*) FROM cortepormovimiento cpm2 WHERE cpm2.idmovimientos = m.idmovimiento) = 0
                        AND m.fechamovimiento BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                        AND so.sucursal ILIKE '%' || @sucOrigen || '%' AND sd.sucursal ILIKE '%' || @sucDestino || '%')
                    OR (m.idmovimiento::text ILIKE @texto))

                ORDER BY ""Fecha Movimiento"" DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("texto", texto ?? "");
                p.AddWithValue("sucOrigen", sucOrigen ?? "");
                p.AddWithValue("sucDestino", sucDestino ?? "");
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
            });
        }

        public DataTable obtenerUltimosMovimientosDashboard(int cantidad)
        {
            const string sql = @"
                SELECT m.fechamovimiento AS ""Fecha Movimiento"", so.sucursal AS ""Origen"", sd.sucursal AS ""Destino""
                FROM movimiento m
                INNER JOIN sucursal so ON so.idsucursal = m.sucursalorigen
                INNER JOIN sucursal sd ON sd.idsucursal = m.sucursaldestino
                ORDER BY m.fechamovimiento DESC, m.idmovimiento DESC
                LIMIT @cantidad;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("cantidad", cantidad));
        }

        public Movimiento cargarMovimiento(int idMovimiento, bool acumulado)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT m.idmovimiento, m.fechamovimiento, m.sucursalorigen AS idorigen, so.sucursal AS origen, m.idmovorigen,
                       m.sucursaldestino AS iddestino, sd.sucursal AS destino, m.observaciones, m.creado, m.actualizado, m.creadopor, m.actualizadopor
                FROM sucursal so
                INNER JOIN movimiento m ON so.idsucursal = m.sucursalorigen
                INNER JOIN sucursal sd ON m.sucursaldestino = sd.idsucursal
                WHERE m.idmovimiento = @idMovimiento;",
                dr =>
                {
                    var oMovimiento = new Movimiento();
                    oMovimiento.IdMovimiento = Convert.ToInt32(dr["idmovimiento"]);
                    oMovimiento.FechaMovimiento = Convert.ToDateTime(dr["fechamovimiento"]);

                    oMovimiento.SucursalOrigen = new Sucursal { idSucursal = Convert.ToInt32(dr["idorigen"]), sucursal = dr["origen"] as string };
                    oMovimiento.IdMovOrigen = dr["idmovorigen"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idmovorigen"]);
                    oMovimiento.SucursalDestino = new Sucursal { idSucursal = Convert.ToInt32(dr["iddestino"]), sucursal = dr["destino"] as string };
                    oMovimiento.Observaciones = GetString(dr, "observaciones");
                    oMovimiento.Creado = Convert.ToDateTime(dr["creado"]);
                    oMovimiento.Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"]);

                    int idCreadoPor = dr["creadopor"] == DBNull.Value ? 0 : Convert.ToInt32(dr["creadopor"]);
                    int idActualizadoPor = dr["actualizadopor"] == DBNull.Value ? 0 : Convert.ToInt32(dr["actualizadopor"]);
                    oMovimiento.CreadoPor = idCreadoPor > 0 ? GetUsuarioLiviano(idCreadoPor) : null;
                    oMovimiento.ActualizadoPor = idActualizadoPor > 0 ? GetUsuarioLiviano(idActualizadoPor) : null;

                    return oMovimiento;
                },
                p => p.AddWithValue("idMovimiento", idMovimiento));

            if (lista.Count == 0) return new Movimiento();

            var mov = lista[0];
            mov.ListaCortesPorMov = cargarCortesPorMovimiento(mov.IdMovimiento, acumulado);
            return mov;
        }

        public List<CortePorMovimiento> cargarCortesPorMovimiento(int idMovimiento, bool acumulado)
        {
            if (!acumulado)
            {
                return DbPg.Reader(_connectionString, _idEmpresa, @"
                    SELECT cpm.idcortemovimiento, c.idcorte, c.codigo, c.corte, cpm.cantkg, cpm.cantunidad, cpm.pesobalanza, cpm.permitiringreso
                    FROM cortepormovimiento cpm
                    INNER JOIN corte c ON cpm.idcorte = c.idcorte
                    WHERE cpm.idmovimientos = @idMovimiento;",
                    dr => new CortePorMovimiento
                    {
                        IdCorteMovimiento = Convert.ToInt32(dr["idcortemovimiento"]),
                        Corte = new Corte { idCorte = Convert.ToInt32(dr["idcorte"]), codigo = GetLong(dr, "codigo"), corte = GetString(dr, "corte") },
                        CantKg = GetFloat(dr, "cantkg"),
                        CantUnidad = (int)GetLong(dr, "cantunidad"),
                        PesoBalanza = GetBool(dr, "pesobalanza"),
                        PermitirIngreso = GetBool(dr, "permitiringreso")
                    },
                    p => p.AddWithValue("idMovimiento", idMovimiento));
            }

            // Acumulado: agrupado por corte, sin pesoBalanza/permitirIngreso (igual que el original).
            return DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT c.codigo, c.corte, SUM(cpm.cantunidad) AS cantunidad, SUM(cpm.cantkg) AS cantkg, cpm.idcorte, cpm.idmovimientos
                FROM cortepormovimiento cpm
                INNER JOIN corte c ON cpm.idcorte = c.idcorte
                WHERE cpm.idmovimientos = @idMovimiento
                GROUP BY c.codigo, c.corte, cpm.idcorte, cpm.idmovimientos
                ORDER BY c.codigo;",
                dr => new CortePorMovimiento
                {
                    IdCorteMovimiento = 0,
                    Corte = new Corte { idCorte = Convert.ToInt32(dr["idcorte"]), codigo = GetLong(dr, "codigo"), corte = GetString(dr, "corte") },
                    CantKg = GetFloat(dr, "cantkg"),
                    CantUnidad = (int)GetLong(dr, "cantunidad")
                },
                p => p.AddWithValue("idMovimiento", idMovimiento));
        }

        // Sin trocear en lotes de 500 (arrays nativos de Postgres, mismo criterio de siempre).
        public Dictionary<int, Tuple<decimal, decimal>> ObtenerTotalesPorMovimiento(IEnumerable<int> idsMovimiento)
        {
            var resultado = new Dictionary<int, Tuple<decimal, decimal>>();
            var ids = (idsMovimiento ?? Enumerable.Empty<int>()).Where(x => x > 0).Distinct().ToArray();
            if (ids.Length == 0) return resultado;

            var filas = DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT idmovimientos AS idmovimiento, SUM(COALESCE(cantunidad, 0)) AS totalunidad, SUM(COALESCE(cantkg, 0)) AS totalkilos
                FROM cortepormovimiento
                WHERE idmovimientos = ANY(@ids)
                GROUP BY idmovimientos;",
                dr => new
                {
                    IdMovimiento = Convert.ToInt32(dr["idmovimiento"]),
                    TotalUnidad = dr["totalunidad"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["totalunidad"]),
                    TotalKilos = dr["totalkilos"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["totalkilos"])
                },
                p => p.AddWithValue("ids", ids));

            foreach (var item in filas)
                resultado[item.IdMovimiento] = Tuple.Create(item.TotalUnidad, item.TotalKilos);

            return resultado;
        }

        public DataTable obtenerLineasMov(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            const string sql = @"
                SELECT m.idmovimiento AS ""Id Movimiento"", m.fechamovimiento AS ""Fecha Movimiento"", c.codigo AS ""Codigo"", c.corte AS ""Corte"",
                       cpm.cantunidad AS ""Total Un."", CAST(cpm.cantkg AS numeric(10,3)) AS ""Total Kg"", (cpm.permitiringreso <> 0) AS ""Permitir Ingr."",
                       cpm.pesobalanza AS ""Balanza"",
                       (CASE WHEN m.actualizacioncompleta = 2 THEN 'PENDIENTE'
                             WHEN (m.idmovorigen > 0 AND m.actualizacioncompleta = 1) OR m.idmovorigen IS NULL THEN 'OK'
                             ELSE 'ERROR' END) AS ""Estado"",
                       so.sucursal AS ""Origen"", m.idmovorigen AS ""Id Origen"", sd.sucursal AS ""Destino"",
                       m.observaciones, m.creado, cp.nombre AS ""creado por"", m.actualizado, ap.nombre AS ""actualizado por""
                FROM cortepormovimiento cpm
                INNER JOIN movimiento m ON cpm.idmovimientos = m.idmovimiento
                INNER JOIN sucursal so ON so.idsucursal = m.sucursalorigen
                INNER JOIN sucursal sd ON m.sucursaldestino = sd.idsucursal
                INNER JOIN corte c ON cpm.idcorte = c.idcorte
                LEFT JOIN usuarios ap ON m.actualizadopor = ap.id
                LEFT JOIN usuarios cp ON m.creadopor = cp.id
                WHERE m.fechamovimiento BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                  AND so.sucursal ILIKE '%' || @sucOrigen || '%'
                  AND sd.sucursal ILIKE '%' || @sucDestino || '%'
                  AND (m.idmovimiento::text = @texto OR c.codigo::text = @texto OR c.corte ILIKE '%' || @texto || '%')
                ORDER BY m.fechamovimiento DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("sucOrigen", sucOrigen ?? "");
                p.AddWithValue("sucDestino", sucDestino ?? "");
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                p.AddWithValue("texto", texto ?? "");
            });
        }

        #endregion

        #region Stock/Reportes (Etapa 11c)

        // Equivalente a ISNUMERIC(@texto) de SQL Server para los reportes que separan
        // "buscar por nombre" de "buscar por codigo exacto". Los codigos reales son
        // bigint (solo digitos), asi que un long.TryParse cubre el caso real de uso;
        // no replica los casos borde de ISNUMERIC (decimales, notacion cientifica,
        // simbolos de moneda), que no aplican a esta busqueda.
        private static bool EsNumero(string texto) => long.TryParse((texto ?? "").Trim(), out _);

        // Query compartida por reporteTeoricoReal e imprimirTeoricoReal (mismo SP de
        // origen, StockTeoricoReal). imprimirTeoricoReal ignora su parametro
        // dtTeoricoReal en el original tambien -- ver Datos/Corte.cs, comentario in-situ
        // "mantengo tu firma". Sin caller vivo (ver docs/DECISIONS.md), se migra por
        // costo marginal ~0.
        private DataTable StockTeoricoRealQuery(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = @"
                SELECT
                    stockteorico.corte AS ""Corte"",
                    stockteorico.sucursal AS ""Sucursal"",
                    stockteorico.stockteorico AS ""Stock Teorico"",
                    stockreal.stockreal AS ""Stock Real"",
                    (stockteorico.stockteorico - COALESCE(stockreal.stockreal, 0)) AS ""Diferencia""
                FROM
                (
                    (SELECT cortep.idcorte, cortep.corte, sucursal.idsucursal, sucursal.sucursal,
                            SUM(mediares.kgmedia * cortep.porcentaje / 100) AS stockteorico
                     FROM corte cortemediares
                     INNER JOIN corte cortep ON cortemediares.idcorte = cortep.idcortemaestro AND cortemediares.idcorte <> cortep.idcorte
                     CROSS JOIN compras
                     INNER JOIN mediares ON compras.idcompra = mediares.idcompra
                     INNER JOIN sucursal ON mediares.idsucursal = sucursal.idsucursal
                     WHERE cortemediares.codigo < 1
                       AND compras.estado = 'Stock Borrado'
                       AND compras.fechacompra BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                       AND mediares.idsucursal = @idSucursal
                       AND compras.nroremito ILIKE '%' || @texto || '%'
                     GROUP BY cortep.corte, cortep.idcorte, sucursal.sucursal, sucursal.idsucursal)
                    UNION
                    (SELECT cortep.idcorte, cortep.corte, sucursal.idsucursal, sucursal.sucursal,
                            SUM(mediares.kgmedia * cortep.porcentaje / 100 * cortem.porcentaje / 100) AS stockteorico
                     FROM corte cortem
                     INNER JOIN corte cortep ON cortem.idcorte = cortep.idcortemaestro
                     INNER JOIN corte cortemediares ON cortem.idcortemaestro = cortemediares.idcorte AND cortem.idcorte <> cortemediares.idcorte
                     CROSS JOIN compras
                     INNER JOIN mediares ON compras.idcompra = mediares.idcompra
                     INNER JOIN sucursal ON mediares.idsucursal = sucursal.idsucursal
                     WHERE cortemediares.codigo < 1
                       AND cortep.independiente = 1
                       AND compras.estado = 'Stock Borrado'
                       AND compras.fechacompra BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                       AND mediares.idsucursal = @idSucursal
                       AND compras.nroremito ILIKE '%' || @texto || '%'
                     GROUP BY cortep.corte, cortep.idcorte, sucursal.idsucursal, sucursal.sucursal)
                ) AS stockteorico
                LEFT JOIN
                (
                    (SELECT cortep.idcorte, cortep.corte, corteporcompra.idsucursal, sucursal.sucursal,
                            SUM(corteporcompra.cantkg) AS stockreal
                     FROM compras
                     INNER JOIN corteporcompra ON compras.idcompra = corteporcompra.idcompra
                     INNER JOIN sucursal ON corteporcompra.idsucursal = sucursal.idsucursal
                     INNER JOIN corte cortep ON corteporcompra.idcorte = cortep.idcorte
                     WHERE cortep.independiente = 1
                       AND compras.fechacompra BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                       AND corteporcompra.idsucursal = @idSucursal
                       AND compras.nroremito ILIKE '%' || @texto || '%'
                     GROUP BY cortep.idcorte, cortep.corte, corteporcompra.idsucursal, sucursal.sucursal)
                    UNION
                    (SELECT cortep.idcorte, cortep.corte, corteporcompra.idsucursal, sucursal.sucursal,
                            SUM(corteporcompra.cantkg * cortep.porcentaje / 100) AS stockreal
                     FROM compras
                     INNER JOIN corteporcompra ON compras.idcompra = corteporcompra.idcompra
                     INNER JOIN sucursal ON corteporcompra.idsucursal = sucursal.idsucursal
                     INNER JOIN corte cortem ON corteporcompra.idcorte = cortem.idcorte
                     INNER JOIN corte cortep ON cortem.idcorte = cortep.idcortemaestro AND cortem.idcorte <> cortep.idcorte
                     WHERE cortep.independiente = 1
                       AND compras.fechacompra BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                       AND corteporcompra.idsucursal = @idSucursal
                       AND compras.nroremito ILIKE '%' || @texto || '%'
                     GROUP BY cortep.idcorte, cortep.corte, corteporcompra.idsucursal, sucursal.sucursal)
                ) AS stockreal
                ON stockreal.idsucursal = stockteorico.idsucursal AND stockreal.idcorte = stockteorico.idcorte;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                p.AddWithValue("texto", texto ?? "");
            });
        }

        public DataTable reporteTeoricoReal(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
            => StockTeoricoRealQuery(texto, idSucursal, fechaDesde, fechaHasta);

        public DataTable imprimirTeoricoReal(DataTable dtTeoricoReal, string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
            => StockTeoricoRealQuery(texto, idSucursal, fechaDesde, fechaHasta);

        public DateTime fechaUltimoCierreStock_Sucursal(int idSucursal)
        {
            const string sql = @"
                SELECT fechacompra
                FROM compras
                WHERE tipocompra = 'Cierre Stock' AND idsucursal = @idSucursal
                ORDER BY fechacompra DESC
                LIMIT 1;";

            object obj = DbPg.Scalar(_connectionString, _idEmpresa, sql, p => p.AddWithValue("idSucursal", idSucursal));
            return (obj == null || obj == DBNull.Value) ? DateTime.MinValue : Convert.ToDateTime(obj);
        }

        // Traduccion de a_CierreStockWeb (SP re-escrito 2026-08 para Web, ver su propio
        // header en sp_helptext con el detalle de 2 fixes de calculo -- fechas y firma
        // verificadas directo contra la base viva, no contra el snapshot de docs/08-
        // relevamiento/, confirmado desactualizado varias veces en esta migracion).
        // #Sucursales/#MapaCorte/#AllCortes/#Operaciones -> CTEs (WITH RECURSIVE para
        // el mapa madre/hija jerarquico, con columna nivel + WHERE nivel<10 reemplazando
        // OPTION(MAXRECURSION 20), que no tiene equivalente nativo en Postgres). Sin
        // funciones/procedimientos Postgres, mismo criterio que el resto del proyecto.
        // "c.fechaCompra LIKE @fechaDesde/@fechaHasta" del original (match exacto de
        // datetime via conversion implicita a string, sin wildcards) -> "=" directo:
        // el propio SP documenta la intencion como "cargado EXACTO en @fecha".
        public DataTable CierreStockWeb(string texto, int idEmpresa, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo, int idProveedor, int idMarca)
        {
            const string sql = @"
                WITH RECURSIVE
                selfmap AS (
                    SELECT c.idcorte AS idcorteorigen, c.idcorte AS idcortestock, 1::numeric(38,10) AS factor
                    FROM corte c
                    WHERE c.independiente = 1 AND c.idempresa = @idEmpresa
                ),
                descendientes AS (
                    SELECT padre.idcorte AS idcorteorigen, hijo.idcorte AS idcortestock,
                           (COALESCE(hijo.porcentaje,0) / 100.0)::numeric(38,10) AS factor,
                           1 AS nivel
                    FROM corte padre
                    INNER JOIN corte hijo ON hijo.idcortemaestro = padre.idcorte AND hijo.idcorte <> padre.idcorte
                    WHERE padre.idempresa = @idEmpresa
                    UNION ALL
                    SELECT d.idcorteorigen, hijo.idcorte,
                           (d.factor * (COALESCE(hijo.porcentaje,0) / 100.0))::numeric(38,10),
                           d.nivel + 1
                    FROM descendientes d
                    INNER JOIN corte actual ON actual.idcorte = d.idcortestock
                    INNER JOIN corte hijo ON hijo.idcortemaestro = actual.idcorte AND hijo.idcorte <> actual.idcorte
                    WHERE d.nivel < 10
                ),
                ascendientes AS (
                    SELECT hijo.idcorte AS idcorteorigen, padre.idcorte AS idcortestock,
                           (1 + COALESCE(hijo.porcentajehueso / NULLIF(hijo.porcentaje,0), 0))::numeric(38,10) AS factor,
                           1 AS nivel
                    FROM corte hijo
                    INNER JOIN corte padre ON hijo.idcortemaestro = padre.idcorte AND hijo.idcorte <> padre.idcorte
                    WHERE hijo.idempresa = @idEmpresa
                    UNION ALL
                    SELECT a.idcorteorigen, padre.idcorte,
                           (a.factor * (1 + COALESCE(actual.porcentajehueso / NULLIF(actual.porcentaje,0), 0)))::numeric(38,10),
                           a.nivel + 1
                    FROM ascendientes a
                    INNER JOIN corte actual ON actual.idcorte = a.idcortestock
                    INNER JOIN corte padre ON actual.idcortemaestro = padre.idcorte AND actual.idcorte <> padre.idcorte
                    WHERE a.nivel < 10
                ),
                mapa AS (
                    SELECT idcorteorigen, idcortestock, factor FROM selfmap
                    UNION ALL
                    SELECT d.idcorteorigen, d.idcortestock, d.factor
                    FROM descendientes d
                    INNER JOIN corte cstock ON cstock.idcorte = d.idcortestock AND cstock.independiente = 1
                    UNION ALL
                    SELECT a.idcorteorigen, a.idcortestock, a.factor
                    FROM ascendientes a
                    INNER JOIN corte cstock ON cstock.idcorte = a.idcortestock AND cstock.independiente = 1
                ),
                mapacorte AS (
                    SELECT idcorteorigen, idcortestock, SUM(factor) AS factor
                    FROM mapa
                    WHERE factor <> 0
                    GROUP BY idcorteorigen, idcortestock
                ),
                sucursales AS (
                    SELECT s.idsucursal, s.sucursal
                    FROM sucursal s
                    WHERE (@idSucursal = 0 OR s.idsucursal = @idSucursal)
                      AND s.idempresa = @idEmpresa
                ),
                allcortes AS (
                    SELECT DISTINCT
                        c.idcorte, c.codigo::text AS codigo, c.corte, s.idsucursal, s.sucursal,
                        COALESCE(c.promedio,0)::numeric(18,3) AS promedio,
                        COALESCE(c.puntostock,0)::numeric(18,3) AS puntostock,
                        COALESCE(c.pesable,false) AS pesable
                    FROM corte c
                    CROSS JOIN sucursales s
                    LEFT JOIN corteproveedor cp ON cp.idcorte = c.idcorte
                    WHERE c.independiente = 1
                      AND COALESCE(c.encierrestock,false) = true
                      AND c.idempresa = @idEmpresa
                      AND (@tipo = '' OR c.tipo = @tipo)
                      AND (@idProveedor = 0 OR cp.idproveedor = @idProveedor)
                      AND (@idMarca = 0 OR c.idmarca = @idMarca)
                ),
                operaciones AS (
                    SELECT 'StockInicial' AS tipooperacion, cpc.idsucursal, mc.idcortestock AS idcorte,
                           SUM(COALESCE(cpc.cantkg,0)::numeric(38,6) * mc.factor) AS kg
                    FROM compras c
                    INNER JOIN corteporcompra cpc ON cpc.idcompra = c.idcompra
                    INNER JOIN sucursales s ON s.idsucursal = cpc.idsucursal
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = cpc.idcorte
                    WHERE c.tipocompra = 'Cierre Stock' AND COALESCE(c.estado,'') = '' AND c.fechacompra = @fechaDesde
                    GROUP BY cpc.idsucursal, mc.idcortestock

                    UNION ALL

                    SELECT 'StockCierre', cpc.idsucursal, mc.idcortestock,
                           SUM(COALESCE(cpc.cantkg,0)::numeric(38,6) * mc.factor)
                    FROM compras c
                    INNER JOIN corteporcompra cpc ON cpc.idcompra = c.idcompra
                    INNER JOIN sucursales s ON s.idsucursal = cpc.idsucursal
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = cpc.idcorte
                    WHERE c.tipocompra = 'Cierre Stock' AND COALESCE(c.estado,'') = '' AND c.fechacompra = @fechaHasta
                    GROUP BY cpc.idsucursal, mc.idcortestock

                    UNION ALL

                    SELECT 'Compras', mr.idsucursal, mc.idcortestock,
                           SUM(COALESCE(mr.kgmedia,0)::numeric(38,6) * mc.factor)
                    FROM compras c
                    INNER JOIN mediares mr ON mr.idcompra = c.idcompra
                    INNER JOIN sucursales s ON s.idsucursal = mr.idsucursal
                    INNER JOIN corte cortemedia ON cortemedia.codigo = 0
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = cortemedia.idcorte
                    WHERE COALESCE(c.estado,'') = '' AND c.fechacompra >= @fechaDesde AND c.fechacompra <= @fechaHasta
                    GROUP BY mr.idsucursal, mc.idcortestock

                    UNION ALL

                    SELECT
                        CASE c.tipocompra
                            WHEN 'Cortes' THEN 'Compras'
                            WHEN 'Ingreso Stock' THEN 'IngresoStock'
                            WHEN 'Ajuste Stock' THEN 'AjusteStock'
                            WHEN 'Egreso Stock' THEN 'EgresoStock'
                        END,
                        cpc.idsucursal, mc.idcortestock,
                        SUM(COALESCE(cpc.cantkg,0)::numeric(38,6) * mc.factor * CASE WHEN c.tipocompra = 'Egreso Stock' THEN -1 ELSE 1 END)
                    FROM compras c
                    INNER JOIN corteporcompra cpc ON cpc.idcompra = c.idcompra
                    INNER JOIN sucursales s ON s.idsucursal = cpc.idsucursal
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = cpc.idcorte
                    WHERE c.tipocompra IN ('Cortes','Ingreso Stock','Ajuste Stock','Egreso Stock')
                      AND COALESCE(c.estado,'') = '' AND c.fechacompra >= @fechaDesde AND c.fechacompra <= @fechaHasta
                    GROUP BY
                        CASE c.tipocompra
                            WHEN 'Cortes' THEN 'Compras'
                            WHEN 'Ingreso Stock' THEN 'IngresoStock'
                            WHEN 'Ajuste Stock' THEN 'AjusteStock'
                            WHEN 'Egreso Stock' THEN 'EgresoStock'
                        END,
                        cpc.idsucursal, mc.idcortestock

                    UNION ALL

                    SELECT 'Ventas', v.idsucursal, mc.idcortestock,
                           SUM((COALESCE(lv.cantkg,0) - COALESCE(lv.kgsajustetarj,0))::numeric(38,6) * mc.factor)
                    FROM ventas v
                    INNER JOIN lineaventa lv ON lv.idventa = v.idventa
                    INNER JOIN sucursales s ON s.idsucursal = v.idsucursal
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = lv.idcorte
                    WHERE v.fechaventa >= @fechaDesde AND v.fechaventa <= @fechaHasta
                    GROUP BY v.idsucursal, mc.idcortestock

                    UNION ALL

                    SELECT 'IngresoMovimiento', m.sucursaldestino, mc.idcortestock,
                           SUM(COALESCE(cpm.cantkg,0)::numeric(38,6) * mc.factor)
                    FROM movimiento m
                    INNER JOIN cortepormovimiento cpm ON cpm.idmovimientos = m.idmovimiento
                    INNER JOIN sucursales s ON s.idsucursal = m.sucursaldestino
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = cpm.idcorte
                    WHERE m.fechamovimiento >= @fechaDesde AND m.fechamovimiento <= @fechaHasta
                    GROUP BY m.sucursaldestino, mc.idcortestock

                    UNION ALL

                    SELECT 'EgresoMovimiento', m.sucursalorigen, mc.idcortestock,
                           SUM(COALESCE(cpm.cantkg,0)::numeric(38,6) * mc.factor)
                    FROM movimiento m
                    INNER JOIN cortepormovimiento cpm ON cpm.idmovimientos = m.idmovimiento
                    INNER JOIN sucursales s ON s.idsucursal = m.sucursalorigen
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = cpm.idcorte
                    WHERE m.fechamovimiento >= @fechaDesde AND m.fechamovimiento <= @fechaHasta
                    GROUP BY m.sucursalorigen, mc.idcortestock

                    UNION ALL

                    SELECT 'IngresoElaborado', e.idsucursal, mc.idcortestock,
                           SUM(COALESCE(cpe.kgutilizados,0)::numeric(38,6) * mc.factor)
                    FROM embutidos e
                    INNER JOIN corteporembutido cpe ON cpe.idembutido = e.idembutido
                    INNER JOIN sucursales s ON s.idsucursal = e.idsucursal
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = e.idcorte
                    WHERE COALESCE(e.estado,'') = '' AND e.fechaembutido >= @fechaDesde AND e.fechaembutido <= @fechaHasta
                    GROUP BY e.idsucursal, mc.idcortestock

                    UNION ALL

                    SELECT 'EgresoElaborado', e.idsucursal, mc.idcortestock,
                           SUM(COALESCE(cpe.kgutilizados,0)::numeric(38,6) * mc.factor)
                    FROM embutidos e
                    INNER JOIN corteporembutido cpe ON cpe.idembutido = e.idembutido
                    INNER JOIN sucursales s ON s.idsucursal = e.idsucursal
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = cpe.idcorte
                    WHERE COALESCE(e.estado,'') = '' AND e.fechaembutido >= @fechaDesde AND e.fechaembutido <= @fechaHasta
                    GROUP BY e.idsucursal, mc.idcortestock
                ),
                resumen AS (
                    SELECT o.idsucursal, o.idcorte,
                        SUM(CASE WHEN o.tipooperacion = 'StockInicial' THEN o.kg ELSE 0 END) AS stockinicial,
                        SUM(CASE WHEN o.tipooperacion = 'StockCierre' THEN o.kg ELSE 0 END) AS stockcierre,
                        SUM(CASE WHEN o.tipooperacion = 'Compras' THEN o.kg ELSE 0 END) AS compras,
                        SUM(CASE WHEN o.tipooperacion = 'IngresoElaborado' THEN o.kg ELSE 0 END) AS ingresoelaborado,
                        SUM(CASE WHEN o.tipooperacion = 'IngresoStock' THEN o.kg ELSE 0 END) AS ingresostock,
                        SUM(CASE WHEN o.tipooperacion = 'IngresoMovimiento' THEN o.kg ELSE 0 END) AS ingresomovimiento,
                        SUM(CASE WHEN o.tipooperacion = 'AjusteStock' THEN o.kg ELSE 0 END) AS ajustestock,
                        SUM(CASE WHEN o.tipooperacion = 'EgresoStock' THEN o.kg ELSE 0 END) AS egresostock,
                        SUM(CASE WHEN o.tipooperacion = 'EgresoMovimiento' THEN o.kg ELSE 0 END) AS egresomovimiento,
                        SUM(CASE WHEN o.tipooperacion = 'EgresoElaborado' THEN o.kg ELSE 0 END) AS egresoelaborado,
                        SUM(CASE WHEN o.tipooperacion = 'Ventas' THEN o.kg ELSE 0 END) AS ventas
                    FROM operaciones o
                    GROUP BY o.idsucursal, o.idcorte
                ),
                final AS (
                    SELECT
                        ac.idcorte, ac.codigo, ac.corte, ac.idsucursal, ac.sucursal,
                        COALESCE(r.stockinicial,0)::numeric(18,3) AS stockini,
                        COALESCE(r.stockcierre,0)::numeric(18,3) AS stockcierre,
                        COALESCE(r.compras,0)::numeric(18,3) AS compras,
                        COALESCE(r.ingresoelaborado,0)::numeric(18,3) AS ingresoelaborado,
                        COALESCE(r.ingresostock,0)::numeric(18,3) AS ingresostock,
                        COALESCE(r.ingresomovimiento,0)::numeric(18,3) AS ingresomovimiento,
                        COALESCE(r.ajustestock,0)::numeric(18,3) AS ajustestock,
                        COALESCE(r.egresostock,0)::numeric(18,3) AS egresostock,
                        COALESCE(r.egresomovimiento,0)::numeric(18,3) AS egresomovimiento,
                        COALESCE(r.egresoelaborado,0)::numeric(18,3) AS egresoelaborado,
                        COALESCE(r.ventas,0)::numeric(18,3) AS ventas,
                        ac.promedio, ac.puntostock, ac.pesable
                    FROM allcortes ac
                    LEFT JOIN resumen r ON r.idsucursal = ac.idsucursal AND r.idcorte = ac.idcorte
                )
                SELECT
                    f.idcorte, f.codigo AS ""Codigo"", f.corte AS ""Corte"", f.idsucursal AS ""idSucursal"", f.sucursal AS ""Sucursal"",
                    f.stockini AS ""Stock.Ini"", f.compras AS ""Compras"",
                    f.ingresoelaborado AS ""Ingr.Elab"", f.ingresostock AS ""Ingr.Stock"", f.ingresomovimiento AS ""Ingr. Mov"", f.ajustestock AS ""Ajus.Stock"",
                    (f.compras + f.ingresoelaborado + f.ingresostock + f.ingresomovimiento + f.ajustestock)::numeric(18,3) AS ""Tot.INGR"",
                    f.egresostock AS ""Egr.Stock"", f.egresomovimiento AS ""Egr.Mov"", f.egresoelaborado AS ""Egr.Elab"", f.ventas AS ""Ventas"",
                    (f.egresostock + f.egresomovimiento + f.egresoelaborado + f.ventas)::numeric(18,3) AS ""Tot.EGR"",
                    (f.stockini + f.compras + f.ingresoelaborado + f.ingresostock + f.ingresomovimiento + f.ajustestock
                     - f.egresostock - f.egresomovimiento - f.egresoelaborado - f.ventas)::numeric(18,3) AS ""DIF"",
                    f.stockcierre AS ""Stock.Cierre"",
                    ((f.stockini + f.compras + f.ingresoelaborado + f.ingresostock + f.ingresomovimiento + f.ajustestock
                      - f.egresostock - f.egresomovimiento - f.egresoelaborado - f.ventas) - f.stockcierre)::numeric(18,3) AS ""Faltante"",
                    f.promedio AS ""promedio"",
                    CASE
                        WHEN f.promedio = 0 THEN
                            ((f.stockini + f.compras + f.ingresoelaborado + f.ingresostock + f.ingresomovimiento + f.ajustestock
                              - f.egresostock - f.egresomovimiento - f.egresoelaborado - f.ventas) - f.stockcierre)::numeric(18,2)
                        ELSE
                            ROUND(
                                (((f.stockini + f.compras + f.ingresoelaborado + f.ingresostock + f.ingresomovimiento + f.ajustestock
                                   - f.egresostock - f.egresomovimiento - f.egresoelaborado - f.ventas) - f.stockcierre) / f.promedio),
                                0
                            )
                    END AS ""Stock.Un"",
                    CASE
                        WHEN f.puntostock > 0
                         AND (
                            ((f.stockini + f.compras + f.ingresoelaborado + f.ingresostock + f.ingresomovimiento + f.ajustestock
                              - f.egresostock - f.egresomovimiento - f.egresoelaborado - f.ventas) < 0)
                            OR f.puntostock > (f.stockini + f.compras + f.ingresoelaborado + f.ingresostock + f.ingresomovimiento + f.ajustestock
                              - f.egresostock - f.egresomovimiento - f.egresoelaborado - f.ventas)
                         )
                        THEN 'X' ELSE '' END AS ""Falta"",
                    f.puntostock AS ""Pto.Stock"", f.pesable AS ""Pesable""
                FROM final f
                WHERE
                    (
                        @textoLimpio = ''
                        AND (
                            f.stockini <> 0 OR f.compras <> 0 OR f.ingresoelaborado <> 0 OR f.ingresostock <> 0
                            OR f.ingresomovimiento <> 0 OR f.ajustestock <> 0 OR f.egresostock <> 0
                            OR f.egresomovimiento <> 0 OR f.egresoelaborado <> 0 OR f.ventas <> 0 OR f.stockcierre <> 0
                        )
                    )
                    OR
                    (
                        @textoLimpio <> ''
                        AND (f.corte ILIKE '%' || @textoLimpio || '%' OR f.codigo ILIKE '%' || @textoLimpio || '%')
                    )
                ORDER BY
                    CASE
                        WHEN TRIM(COALESCE(f.codigo,'')) <> '' AND TRIM(COALESCE(f.codigo,'')) ~ '^[0-9]+$'
                        THEN TRIM(f.codigo)::numeric(18,0)
                        ELSE 9999999999999999
                    END ASC,
                    TRIM(COALESCE(f.codigo,'')) ASC,
                    f.sucursal ASC;";

            string textoLimpio = (texto ?? "").Trim();

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("textoLimpio", textoLimpio);
                p.AddWithValue("idEmpresa", idEmpresa);
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                p.AddWithValue("tipo", tipo ?? "");
                p.AddWithValue("idProveedor", idProveedor);
                p.AddWithValue("idMarca", idMarca);
            });
        }

        // Traduccion de Acum_Ventas. El C# original (Datos/Corte.cs) nunca pasa
        // @idEmpresa (no esta en la firma de ICorteRepository tampoco) -- el SP lo
        // declara opcional (default NULL), asi que idEmpresaFiltro=0 siempre en la
        // practica real; se replica igual (RLS de tabla ya aisla por tenant).
        public DataTable acum_Ventas(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo, int idProveedor, int idMarca)
        {
            const string sql = @"
                SELECT
                    allcortes.codigo::text AS ""Codigo"", allcortes.corte AS ""Corte"",
                    0::numeric AS ""StockActual"", egresoventas.totalventa AS ""Ventas"", 0::numeric AS ""DIF""
                FROM
                (
                    SELECT cortep.idcorte, cortep.codigo, cortep.corte, s.idsucursal, s.sucursal, 0::numeric AS stockingreso
                    FROM corte cortep
                    LEFT JOIN corteproveedor cp ON cortep.idcorte = cp.idcorte
                    CROSS JOIN sucursal s
                    WHERE cortep.independiente = 1
                      AND @idSucursal > 0 AND s.idsucursal = @idSucursal
                      AND (@tipo = '' OR cortep.tipo = @tipo)
                      AND (@idProveedor = 0 OR cp.idproveedor = @idProveedor)
                      AND (@idMarca = 0 OR cortep.idmarca = @idMarca)
                    GROUP BY cortep.idcorte, cortep.codigo, cortep.corte, s.idsucursal, s.sucursal
                ) AS allcortes
                LEFT JOIN
                (
                    SELECT idcorte, codigo, corte, idsucursal, sucursal, SUM(totalventa) AS totalventa
                    FROM
                    (
                        SELECT c.idcorte, c.codigo, c.corte, s.idsucursal, s.sucursal,
                               SUM(lv.cantkg - lv.kgsajustetarj) AS totalventa
                        FROM ventas v
                        INNER JOIN lineaventa lv ON v.idventa = lv.idventa
                        INNER JOIN sucursal s ON v.idsucursal = s.idsucursal
                        INNER JOIN corte c ON lv.idcorte = c.idcorte
                        WHERE v.fechaventa BETWEEN @fechaDesde AND @fechaHasta
                          AND @idSucursal > 0 AND v.idsucursal = @idSucursal
                          AND c.independiente = 1
                        GROUP BY s.idsucursal, s.sucursal, c.idcorte, c.codigo, c.corte

                        UNION

                        SELECT c.idcorte, c.codigo, c.corte, s.idsucursal, s.sucursal,
                               SUM((lv.cantkg - lv.kgsajustetarj) + (lv.cantkg - lv.kgsajustetarj) * cortep.porcentajehueso / cortep.porcentaje) AS totalventa
                        FROM ventas v
                        INNER JOIN lineaventa lv ON v.idventa = lv.idventa
                        INNER JOIN sucursal s ON v.idsucursal = s.idsucursal
                        INNER JOIN corte cortep ON lv.idcorte = cortep.idcorte
                        INNER JOIN corte c ON cortep.idcortemaestro = c.idcorte
                        WHERE v.fechaventa BETWEEN @fechaDesde AND @fechaHasta
                          AND @idSucursal > 0 AND v.idsucursal = @idSucursal
                          AND c.codigo > 0 AND c.independiente = 1
                        GROUP BY s.idsucursal, s.sucursal, c.idcorte, c.codigo, c.corte

                        UNION

                        SELECT c.idcorte, c.codigo, c.corte, s.idsucursal, s.sucursal,
                               SUM(
                                   ((lv.cantkg - lv.kgsajustetarj) + (lv.cantkg - lv.kgsajustetarj) * corte1.porcentajehueso / corte1.porcentaje)
                                   + (((lv.cantkg - lv.kgsajustetarj) + (lv.cantkg - lv.kgsajustetarj) * corte1.porcentajehueso / corte1.porcentaje)
                                      * cortep.porcentajehueso / cortep.porcentaje)
                               ) AS totalventa
                        FROM ventas v
                        INNER JOIN lineaventa lv ON v.idventa = lv.idventa
                        INNER JOIN sucursal s ON v.idsucursal = s.idsucursal
                        INNER JOIN corte corte1 ON corte1.idcorte = lv.idcorte
                        INNER JOIN corte cortep ON corte1.idcortemaestro = cortep.idcorte
                        INNER JOIN corte c ON c.idcorte = cortep.idcortemaestro
                        WHERE v.fechaventa BETWEEN @fechaDesde AND @fechaHasta
                          AND @idSucursal > 0 AND v.idsucursal = @idSucursal
                          AND c.codigo > 0 AND c.independiente = 1
                        GROUP BY s.idsucursal, s.sucursal, c.idcorte, c.codigo, c.corte
                    ) egresoventas
                    GROUP BY idcorte, codigo, corte, idsucursal, sucursal
                ) AS egresoventas
                ON egresoventas.idsucursal = allcortes.idsucursal AND egresoventas.idcorte = allcortes.idcorte
                WHERE
                    (@texto = '')
                    OR
                    (
                        @texto <> ''
                        AND (
                            (@esNumero = false AND allcortes.corte ILIKE '%' || @texto || '%')
                            OR (@esNumero = true AND allcortes.codigo::text = @texto)
                        )
                    )
                ORDER BY allcortes.codigo;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("texto", texto ?? "");
                p.AddWithValue("esNumero", EsNumero(texto));
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                p.AddWithValue("tipo", tipo ?? "");
                p.AddWithValue("idProveedor", idProveedor);
                p.AddWithValue("idMarca", idMarca);
            });
        }

        // El SP declara su propio @idEmpresa (default NULL) pero Datos.Corte.
        // TotalPorCortesVendidos nunca lo pasa (no esta en ICorteRepository tampoco) --
        // esa condicion es siempre verdadera en la practica, se omite del todo (RLS de
        // tabla sigue aislando por tenant). No se "arregla" pasandolo: cambiaria
        // comportamiento sin pedido (ver docs/DECISIONS.md).
        public DataTable TotalPorCortesVendidos(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo, int idProveedor, int idMarca)
        {
            const string sql = @"
                SELECT
                    c.codigo::text AS ""Codigo"", c.corte AS ""Corte"",
                    CASE WHEN @idSucursal = 0 THEN 'Todas' ELSE MAX(s.sucursal) END AS ""Sucursal"",
                    SUM(lv.cantkg) AS ""Total Kgs"", SUM(lv.cantkg * lv.preciokg) AS ""Total $""
                FROM corte c
                INNER JOIN lineaventa lv ON c.idcorte = lv.idcorte
                INNER JOIN ventas v ON lv.idventa = v.idventa
                INNER JOIN sucursal s ON v.idsucursal = s.idsucursal
                LEFT JOIN corteproveedor cp ON c.idcorte = cp.idcorte
                WHERE
                    v.fechaventa BETWEEN @fechaDesde AND @fechaHasta
                    AND (@idSucursal = 0 OR v.idsucursal = @idSucursal)
                    AND (@tipo = '' OR c.tipo = @tipo)
                    AND (
                        (@esNumero = false AND c.corte ILIKE '%' || @texto || '%')
                        OR (@esNumero = true AND c.codigo::text = @texto)
                    )
                    AND (@idProveedor = 0 OR cp.idproveedor = @idProveedor)
                    AND (@idMarca = 0 OR c.idmarca = @idMarca)
                GROUP BY c.codigo, c.corte
                ORDER BY c.corte;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("texto", texto ?? "");
                p.AddWithValue("esNumero", EsNumero(texto));
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                p.AddWithValue("tipo", tipo ?? "");
                p.AddWithValue("idProveedor", idProveedor);
                p.AddWithValue("idMarca", idMarca);
            });
        }

        // El WHERE del segundo branch del original tiene una precedencia AND/OR sin
        // parentesis ("(A AND B AND C AND D) OR E": si el texto matchea idMovimiento
        // como numero, se saltea el filtro de fecha/sucursal/corte por completo). Se
        // replica tal cual -- no se "arregla" de paso (sin caller Web, WinForms-only,
        // ver docs/DECISIONS.md).
        public DataTable TotalMovimientosPorCorte(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = @"
                SELECT
                    movimientos.codigo::text AS ""Codigo"", movimientos.corte AS ""Corte"",
                    SUM(movimientos.totalunidad) AS ""Total Unidades"", SUM(movimientos.totalegreso) AS ""Total Kgs"",
                    movimientos.origen AS ""Sucursal Origen"", movimientos.destino AS ""Sucursal Destino""
                FROM
                (
                    (SELECT c.idcorte, c.codigo, c.corte, so.idsucursal AS idorigen, so.sucursal AS origen,
                            sd.idsucursal AS iddestino, sd.sucursal AS destino, 0::numeric AS totalunidad, 0::numeric AS totalegreso
                     FROM sucursal so
                     INNER JOIN sucursal sd ON so.idsucursal <> sd.idsucursal
                     CROSS JOIN corte c
                     WHERE (c.corte ILIKE '%' || @texto || '%' OR c.codigo::text ILIKE '%' || @texto || '%')
                       AND c.codigo > 0
                       AND sd.idsucursal = @idSucursal
                       AND c.independiente = 1)

                    UNION

                    (SELECT c.idcorte, c.codigo, c.corte, so.idsucursal AS idorigen, so.sucursal AS origen,
                            sd.idsucursal AS iddestino, sd.sucursal AS destino,
                            SUM(cpm.cantunidad) AS totalunidad, SUM(cpm.cantkg) AS totalegreso
                     FROM corte c
                     INNER JOIN cortepormovimiento cpm ON c.idcorte = cpm.idcorte
                     INNER JOIN movimiento m ON cpm.idmovimientos = m.idmovimiento
                     INNER JOIN sucursal so ON m.sucursalorigen = so.idsucursal
                     INNER JOIN sucursal sd ON m.sucursaldestino = sd.idsucursal
                     WHERE (
                             m.fechamovimiento BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                             AND sd.idsucursal = @idSucursal
                             AND (c.corte ILIKE '%' || @texto || '%' OR c.codigo::text ILIKE '%' || @texto || '%')
                             AND c.codigo > 0
                           )
                           OR (m.idmovimiento::text LIKE @texto)
                     GROUP BY c.idcorte, c.codigo, c.corte, so.idsucursal, so.sucursal, sd.idsucursal, sd.sucursal)
                ) AS movimientos
                GROUP BY movimientos.codigo, movimientos.corte, movimientos.origen, movimientos.destino;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("texto", texto ?? "");
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
            });
        }

        public DataTable ObtenerSerieVentasPorCorte(int idCorte, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo, int idMarca, string agrupacionTemporal)
        {
            const string sql = @"
                SELECT
                    CASE WHEN @agrupacionTemporal = 'dia' THEN date_trunc('day', v.fechaventa) ELSE date_trunc('hour', v.fechaventa) END AS bucketfecha,
                    SUM(COALESCE(lv.cantkg,0) - COALESCE(lv.kgsajustetarj,0)) AS totalkg,
                    SUM((COALESCE(lv.cantkg,0) - COALESCE(lv.kgsajustetarj,0)) * COALESCE(lv.preciokg,0)) AS totalimporte
                FROM ventas v
                INNER JOIN lineaventa lv ON lv.idventa = v.idventa
                INNER JOIN corte c ON c.idcorte = lv.idcorte
                WHERE
                    lv.idcorte = @idCorte
                    AND v.fechaventa >= @fechaDesde AND v.fechaventa <= @fechaHasta
                    AND (@idSucursal <= 0 OR v.idsucursal = @idSucursal)
                    AND (@tipo = '' OR c.tipo = @tipo)
                    AND (@idMarca <= 0 OR c.idmarca = @idMarca)
                GROUP BY CASE WHEN @agrupacionTemporal = 'dia' THEN date_trunc('day', v.fechaventa) ELSE date_trunc('hour', v.fechaventa) END
                ORDER BY bucketfecha;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idCorte", idCorte);
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                p.AddWithValue("tipo", (tipo ?? "").Trim());
                p.AddWithValue("idMarca", idMarca);
                p.AddWithValue("agrupacionTemporal", (agrupacionTemporal ?? "hora").Trim().ToLowerInvariant());
            });
        }

        // Traduccion de BalanceConsFinal_FecDesde_Hasta. El original mezcla SUM(...)
        // numerico con literales '' en el mismo UNION (SQL Server ensancha implicitamente
        // a un tipo comun); Postgres exige tipos identicos en todas las ramas del UNION,
        // asi que Kgs/Monto/Tickets se castean a text en cada rama -- incluidas las
        // numericas -- para reproducir exactamente esa mezcla (las filas de detalle
        // quedan con '', las de datos con el numero como string, igual que el DataTable
        // que ve hoy el post-procesamiento de Datos.Corte.Balance). El original no tiene
        // ORDER BY explicito (confia en el orden implicito del UNION); se agrega
        // "ORDER BY orden" en Postgres porque esa garantia no es portable entre motores
        // y la columna "orden" (1-12) deja clara la intencion de orden narrativo del
        // reporte -- no es un cambio de comportamiento, es hacer explicito lo que el
        // original daba por sentado.
        private DataTable BalanceQuery(int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            const string ventasBase = @"
                SELECT ventas.idventa, ventas.idsucursal, SUM(lineaventa.cantkg) AS kgs, SUM(lineaventa.cantkg * lineaventa.preciokg) AS monto
                FROM ventas
                INNER JOIN lineaventa ON ventas.idventa = lineaventa.idventa
                INNER JOIN corte ON lineaventa.idcorte = corte.idcorte
                INNER JOIN sucursal ON ventas.idsucursal = sucursal.idsucursal
                WHERE ventas.idsucursal = @idSucursal AND ventas.fechaventa BETWEEN @fechaDesde AND @fechaHasta AND ventas.enctacte = {0}
                GROUP BY ventas.idventa, ventas.idsucursal";

            string ventasEnCtaCteFalse = string.Format(ventasBase, "false");
            string ventasEnCtaCteTrue = string.Format(ventasBase, "true");
            string ventasBancarizadas = ventasEnCtaCteFalse + " AND ventas.formapago <> 'Efectivo'";
            string ventasEfectivo = ventasEnCtaCteFalse + " AND ventas.formapago = 'Efectivo'";

            string sql = $@"
                SELECT descripcion AS ""Descripcion"", kgs AS ""Kgs"", monto AS ""Monto"", tickets AS ""Tickets""
                FROM (
                    SELECT 1 AS orden, 'BALANCE' AS descripcion,
                           SUM(kgs)::text AS kgs, SUM(monto)::text AS monto, SUM(tickets)::text AS tickets
                    FROM (
                        SELECT compras.idsucursal, 0::numeric AS kgs, SUM(-mediares.kgmedia * mediares.preciomedia) AS monto, 0::numeric AS tickets
                        FROM compras INNER JOIN mediares ON compras.idcompra = mediares.idcompra
                        WHERE compras.idsucursal = @idSucursal AND compras.fechacompra BETWEEN @fechaDesde AND @fechaHasta
                          AND (compras.tipocompra = 'Media Res' OR compras.tipocompra = 'Cortes')
                        GROUP BY compras.idsucursal

                        UNION ALL

                        SELECT compras.idsucursal, 0::numeric, SUM(-corteporcompra.cantkg * corteporcompra.preciokg), 0::numeric
                        FROM compras INNER JOIN corteporcompra ON compras.idcompra = corteporcompra.idcompra
                        WHERE compras.idsucursal = @idSucursal AND compras.fechacompra BETWEEN @fechaDesde AND @fechaHasta
                          AND (compras.tipocompra = 'Media Res' OR compras.tipocompra = 'Cortes')
                        GROUP BY compras.idsucursal

                        UNION ALL

                        SELECT egresoscaja.idsucursal, 0::numeric, SUM(-egresoscaja.monto), 0::numeric
                        FROM egresoscaja INNER JOIN tiposegresocaja ON egresoscaja.idtipoegresocaja = tiposegresocaja.id
                        WHERE egresoscaja.idsucursal = @idSucursal AND egresoscaja.fechahora BETWEEN @fechaDesde AND @fechaHasta
                          AND tiposegresocaja.esgasto = true AND egresoscaja.idcompra IS NULL
                        GROUP BY egresoscaja.idsucursal

                        UNION ALL

                        SELECT idsucursal, SUM(kgs), SUM(monto), COUNT(*)::numeric
                        FROM ({ventasEnCtaCteFalse}) ventasagg
                        GROUP BY idsucursal
                    ) balance
                    GROUP BY idsucursal

                    UNION ALL
                    SELECT 2, 'DETALLE BALANCE', ''::text, ''::text, ''::text

                    UNION ALL
                    SELECT 3, 'VENTAS A CONS.FINAL', SUM(kgs)::text, SUM(monto)::text, COUNT(*)::text
                    FROM ({ventasEnCtaCteFalse}) ventacf GROUP BY idsucursal

                    UNION ALL
                    SELECT 4, 'COMPRAS MEDIAS RESES', 0::text, SUM(mediares.kgmedia * mediares.preciomedia)::text, 0::text
                    FROM compras INNER JOIN mediares ON compras.idcompra = mediares.idcompra
                    WHERE compras.idsucursal = @idSucursal AND compras.fechacompra BETWEEN @fechaDesde AND @fechaHasta
                      AND (compras.tipocompra = 'Media Res' OR compras.tipocompra = 'Cortes')
                    GROUP BY compras.idsucursal

                    UNION ALL
                    SELECT 5, 'COMPRAS', 0::text, SUM(-corteporcompra.cantkg * corteporcompra.preciokg)::text, 0::text
                    FROM compras INNER JOIN corteporcompra ON compras.idcompra = corteporcompra.idcompra
                    WHERE compras.idsucursal = @idSucursal AND compras.fechacompra BETWEEN @fechaDesde AND @fechaHasta
                      AND (compras.tipocompra = 'Media Res' OR compras.tipocompra = 'Cortes')
                    GROUP BY compras.idsucursal

                    UNION ALL
                    SELECT 6, 'GASTOS', 0::text, SUM(-egresoscaja.monto)::text, 0::text
                    FROM egresoscaja INNER JOIN tiposegresocaja ON egresoscaja.idtipoegresocaja = tiposegresocaja.id
                    WHERE egresoscaja.idsucursal = @idSucursal AND egresoscaja.fechahora BETWEEN @fechaDesde AND @fechaHasta
                      AND tiposegresocaja.esgasto = true AND egresoscaja.idcompra IS NULL
                    GROUP BY egresoscaja.idsucursal

                    UNION ALL
                    SELECT 7, 'DETALLE VENTAS', ''::text, ''::text, ''::text

                    UNION ALL
                    SELECT 8, 'VENTAS CTA.CTE *', SUM(kgs)::text, SUM(monto)::text, COUNT(*)::text
                    FROM ({ventasEnCtaCteTrue}) ventactacte GROUP BY idsucursal

                    UNION ALL
                    SELECT 9, 'VENTAS BANCARIZADAS/NO EFECTIVO **', SUM(kgs)::text, SUM(monto)::text, COUNT(*)::text
                    FROM ({ventasBancarizadas}) ventabanco GROUP BY idsucursal

                    UNION ALL
                    SELECT 9, 'VENTAS EFECTIVO **', SUM(kgs)::text, SUM(monto)::text, COUNT(*)::text
                    FROM ({ventasEfectivo}) ventaefectivo GROUP BY idsucursal

                    UNION ALL
                    SELECT 10, 'NOTAS', ''::text, ''::text, ''::text

                    UNION ALL
                    SELECT 11, '*No sumadas al balance', ''::text, ''::text, ''::text

                    UNION ALL
                    SELECT 12, '**Sumadas al balance', ''::text, ''::text, ''::text
                ) x
                ORDER BY orden;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
            });
        }

        // Post-procesamiento portado literal desde Datos/Corte.cs (Balance): sin esto
        // el grid queda con la columna "orden" visible y sin las filas de
        // detalle/notas en blanco que el reporte original muestra.
        public DataTable Balance(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dt = BalanceQuery(idSucursal, fechaDesde, fechaHasta.AddDays(1));

            foreach (DataRow row in dt.Rows)
            {
                string desc = row["Descripcion"]?.ToString() ?? "";

                if (desc.Contains("DETALLE") || desc.Contains("NOTAS") || (desc.Length > 0 && desc[0] == '*'))
                {
                    for (int i = 1; i < dt.Columns.Count; i++)
                        row[i] = DBNull.Value;
                }

                if (desc.Contains("COMPRAS") || desc.Contains("GASTOS"))
                {
                    if (dt.Columns.Contains("Tickets"))
                        row["Tickets"] = DBNull.Value;
                }
            }

            return dt;
        }

        // Traduccion de a_ExistenciaStockPorSucursales (SP re-escrito 2026-08, mismos 2
        // fixes de calculo que a_CierreStockWeb -- signo de EgresoStock y
        // FechaUltimoCierre acotado por @fechaHasta -- mas un fix propio de filtrado por
        // empresa en el ancla de la CTE recursiva. Verificado directo contra la base
        // viva con sp_helptext, no contra el snapshot de docs/08-relevamiento/. Mismo
        // patron de WITH RECURSIVE que CierreStockWeb, con FechaUltimoCierre calculado
        // por sucursal (no un @fechaDesde fijo) y el CROSS APPLY final resuelto como
        // columnas calculadas directas (no hace falta LATERAL, son expresiones simples).
        public List<Entidades.ExistenciaStockPorSucursalPlanoVm> ObtenerExistenciaPorSucursalesPlano(
            string texto, int idSucursal, DateTime? fechaHasta, string tipo, int idProveedor, int idMarca, int idCorte, bool soloConStock)
        {
            DateTime fechaHastaEfectiva = fechaHasta ?? DateTime.Now;
            string textoLimpio = (texto ?? "").Trim();

            const string sql = @"
                WITH RECURSIVE
                sucursales AS (
                    SELECT s.idsucursal, s.sucursal,
                           COALESCE(MAX(c.fechacompra), '1900-01-01'::timestamp) AS fechaultimocierre
                    FROM sucursal s
                    LEFT JOIN corteporcompra cpc ON cpc.idsucursal = s.idsucursal
                    LEFT JOIN compras c ON c.idcompra = cpc.idcompra
                        AND c.tipocompra = 'Cierre Stock' AND COALESCE(c.estado,'') = '' AND c.fechacompra <= @fechaHasta
                    WHERE (@idSucursal = 0 OR s.idsucursal = @idSucursal) AND s.idempresa = @idEmpresa
                    GROUP BY s.idsucursal, s.sucursal
                ),
                selfmap AS (
                    SELECT c.idcorte AS idcorteorigen, c.idcorte AS idcortestock, 1::numeric(38,10) AS factor
                    FROM corte c
                    WHERE c.independiente = 1 AND c.idempresa = @idEmpresa
                ),
                descendientes AS (
                    SELECT padre.idcorte AS idcorteorigen, hijo.idcorte AS idcortestock,
                           (COALESCE(hijo.porcentaje,0) / 100.0)::numeric(38,10) AS factor, 1 AS nivel
                    FROM corte padre
                    INNER JOIN corte hijo ON hijo.idcortemaestro = padre.idcorte AND hijo.idcorte <> padre.idcorte
                    WHERE padre.idempresa = @idEmpresa
                    UNION ALL
                    SELECT d.idcorteorigen, hijo.idcorte,
                           (d.factor * (COALESCE(hijo.porcentaje,0) / 100.0))::numeric(38,10), d.nivel + 1
                    FROM descendientes d
                    INNER JOIN corte actual ON actual.idcorte = d.idcortestock
                    INNER JOIN corte hijo ON hijo.idcortemaestro = actual.idcorte AND hijo.idcorte <> actual.idcorte
                    WHERE d.nivel < 10
                ),
                ascendientes AS (
                    SELECT hijo.idcorte AS idcorteorigen, padre.idcorte AS idcortestock,
                           (1 + COALESCE(hijo.porcentajehueso / NULLIF(hijo.porcentaje,0), 0))::numeric(38,10) AS factor, 1 AS nivel
                    FROM corte hijo
                    INNER JOIN corte padre ON hijo.idcortemaestro = padre.idcorte AND hijo.idcorte <> padre.idcorte
                    WHERE hijo.idempresa = @idEmpresa
                    UNION ALL
                    SELECT a.idcorteorigen, padre.idcorte,
                           (a.factor * (1 + COALESCE(actual.porcentajehueso / NULLIF(actual.porcentaje,0), 0)))::numeric(38,10), a.nivel + 1
                    FROM ascendientes a
                    INNER JOIN corte actual ON actual.idcorte = a.idcortestock
                    INNER JOIN corte padre ON actual.idcortemaestro = padre.idcorte AND actual.idcorte <> padre.idcorte
                    WHERE a.nivel < 10
                ),
                mapa AS (
                    SELECT idcorteorigen, idcortestock, factor FROM selfmap
                    UNION ALL
                    SELECT d.idcorteorigen, d.idcortestock, d.factor
                    FROM descendientes d INNER JOIN corte cstock ON cstock.idcorte = d.idcortestock AND cstock.independiente = 1
                    UNION ALL
                    SELECT a.idcorteorigen, a.idcortestock, a.factor
                    FROM ascendientes a INNER JOIN corte cstock ON cstock.idcorte = a.idcortestock AND cstock.independiente = 1
                ),
                mapacorte AS (
                    SELECT idcorteorigen, idcortestock, SUM(factor) AS factor
                    FROM mapa
                    WHERE factor <> 0 AND (@idCorte = 0 OR idcortestock = @idCorte)
                    GROUP BY idcorteorigen, idcortestock
                ),
                allcortes AS (
                    SELECT DISTINCT
                        c.idcorte, c.codigo::text AS codigo, c.corte, s.idsucursal, s.sucursal, s.fechaultimocierre,
                        COALESCE(c.promedio,0)::numeric(18,3) AS promedio,
                        COALESCE(c.puntostock,0)::numeric(18,3) AS puntostock,
                        COALESCE(c.pesable,false) AS pesable
                    FROM corte c
                    CROSS JOIN sucursales s
                    LEFT JOIN corteproveedor cp ON cp.idcorte = c.idcorte
                    WHERE c.independiente = 1
                      AND COALESCE(c.encierrestock,false) = true
                      AND c.idempresa = @idEmpresa
                      AND (@tipo = '' OR c.tipo = @tipo)
                      AND (@idProveedor = 0 OR cp.idproveedor = @idProveedor)
                      AND (@idMarca = 0 OR c.idmarca = @idMarca)
                      AND (@idCorte = 0 OR c.idcorte = @idCorte)
                      AND (@textoLimpio = '' OR c.corte ILIKE '%' || @textoLimpio || '%' OR c.codigo::text ILIKE '%' || @textoLimpio || '%')
                ),
                operaciones AS (
                    SELECT 'StockInicial' AS tipooperacion, cpc.idsucursal, mc.idcortestock AS idcorte,
                           SUM(COALESCE(cpc.cantkg,0)::numeric(38,6) * mc.factor) AS kg
                    FROM compras c
                    INNER JOIN corteporcompra cpc ON cpc.idcompra = c.idcompra
                    INNER JOIN sucursales s ON s.idsucursal = cpc.idsucursal AND c.fechacompra = s.fechaultimocierre
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = cpc.idcorte
                    WHERE c.tipocompra = 'Cierre Stock' AND COALESCE(c.estado,'') = ''
                    GROUP BY cpc.idsucursal, mc.idcortestock

                    UNION ALL

                    SELECT 'Compras', mr.idsucursal, mc.idcortestock,
                           SUM(COALESCE(mr.kgmedia,0)::numeric(38,6) * mc.factor)
                    FROM compras c
                    INNER JOIN mediares mr ON mr.idcompra = c.idcompra
                    INNER JOIN sucursales s ON s.idsucursal = mr.idsucursal
                    INNER JOIN corte cortemedia ON cortemedia.codigo = 0
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = cortemedia.idcorte
                    WHERE COALESCE(c.estado,'') = '' AND c.fechacompra >= s.fechaultimocierre AND c.fechacompra <= @fechaHasta
                    GROUP BY mr.idsucursal, mc.idcortestock

                    UNION ALL

                    SELECT
                        CASE c.tipocompra
                            WHEN 'Cortes' THEN 'Compras'
                            WHEN 'Ingreso Stock' THEN 'IngresoStock'
                            WHEN 'Ajuste Stock' THEN 'AjusteStock'
                            WHEN 'Egreso Stock' THEN 'EgresoStock'
                        END,
                        cpc.idsucursal, mc.idcortestock,
                        SUM(COALESCE(cpc.cantkg,0)::numeric(38,6) * mc.factor * CASE WHEN c.tipocompra = 'Egreso Stock' THEN -1 ELSE 1 END)
                    FROM compras c
                    INNER JOIN corteporcompra cpc ON cpc.idcompra = c.idcompra
                    INNER JOIN sucursales s ON s.idsucursal = cpc.idsucursal
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = cpc.idcorte
                    WHERE c.tipocompra IN ('Cortes','Ingreso Stock','Ajuste Stock','Egreso Stock')
                      AND COALESCE(c.estado,'') = '' AND c.fechacompra >= s.fechaultimocierre AND c.fechacompra <= @fechaHasta
                    GROUP BY
                        CASE c.tipocompra
                            WHEN 'Cortes' THEN 'Compras'
                            WHEN 'Ingreso Stock' THEN 'IngresoStock'
                            WHEN 'Ajuste Stock' THEN 'AjusteStock'
                            WHEN 'Egreso Stock' THEN 'EgresoStock'
                        END,
                        cpc.idsucursal, mc.idcortestock

                    UNION ALL

                    SELECT 'Ventas', v.idsucursal, mc.idcortestock,
                           SUM((COALESCE(lv.cantkg,0) - COALESCE(lv.kgsajustetarj,0))::numeric(38,6) * mc.factor)
                    FROM ventas v
                    INNER JOIN lineaventa lv ON lv.idventa = v.idventa
                    INNER JOIN sucursales s ON s.idsucursal = v.idsucursal
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = lv.idcorte
                    WHERE v.fechaventa >= s.fechaultimocierre AND v.fechaventa <= @fechaHasta
                    GROUP BY v.idsucursal, mc.idcortestock

                    UNION ALL

                    SELECT 'IngresoMovimiento', m.sucursaldestino, mc.idcortestock,
                           SUM(COALESCE(cpm.cantkg,0)::numeric(38,6) * mc.factor)
                    FROM movimiento m
                    INNER JOIN cortepormovimiento cpm ON cpm.idmovimientos = m.idmovimiento
                    INNER JOIN sucursales s ON s.idsucursal = m.sucursaldestino
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = cpm.idcorte
                    WHERE m.fechamovimiento >= s.fechaultimocierre AND m.fechamovimiento <= @fechaHasta
                    GROUP BY m.sucursaldestino, mc.idcortestock

                    UNION ALL

                    SELECT 'EgresoMovimiento', m.sucursalorigen, mc.idcortestock,
                           SUM(COALESCE(cpm.cantkg,0)::numeric(38,6) * mc.factor)
                    FROM movimiento m
                    INNER JOIN cortepormovimiento cpm ON cpm.idmovimientos = m.idmovimiento
                    INNER JOIN sucursales s ON s.idsucursal = m.sucursalorigen
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = cpm.idcorte
                    WHERE m.fechamovimiento >= s.fechaultimocierre AND m.fechamovimiento <= @fechaHasta
                    GROUP BY m.sucursalorigen, mc.idcortestock

                    UNION ALL

                    SELECT 'IngresoElaborado', e.idsucursal, mc.idcortestock,
                           SUM(COALESCE(cpe.kgutilizados,0)::numeric(38,6) * mc.factor)
                    FROM embutidos e
                    INNER JOIN corteporembutido cpe ON cpe.idembutido = e.idembutido
                    INNER JOIN sucursales s ON s.idsucursal = e.idsucursal
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = e.idcorte
                    WHERE COALESCE(e.estado,'') = '' AND e.fechaembutido >= s.fechaultimocierre AND e.fechaembutido <= @fechaHasta
                    GROUP BY e.idsucursal, mc.idcortestock

                    UNION ALL

                    SELECT 'EgresoElaborado', e.idsucursal, mc.idcortestock,
                           SUM(COALESCE(cpe.kgutilizados,0)::numeric(38,6) * mc.factor)
                    FROM embutidos e
                    INNER JOIN corteporembutido cpe ON cpe.idembutido = e.idembutido
                    INNER JOIN sucursales s ON s.idsucursal = e.idsucursal
                    INNER JOIN mapacorte mc ON mc.idcorteorigen = cpe.idcorte
                    WHERE COALESCE(e.estado,'') = '' AND e.fechaembutido >= s.fechaultimocierre AND e.fechaembutido <= @fechaHasta
                    GROUP BY e.idsucursal, mc.idcortestock
                ),
                resumen AS (
                    SELECT o.idsucursal, o.idcorte,
                        SUM(CASE WHEN o.tipooperacion = 'StockInicial' THEN o.kg ELSE 0 END) AS stockinicial,
                        SUM(CASE WHEN o.tipooperacion = 'Compras' THEN o.kg ELSE 0 END) AS compras,
                        SUM(CASE WHEN o.tipooperacion = 'IngresoElaborado' THEN o.kg ELSE 0 END) AS ingresoelaborado,
                        SUM(CASE WHEN o.tipooperacion = 'IngresoStock' THEN o.kg ELSE 0 END) AS ingresostock,
                        SUM(CASE WHEN o.tipooperacion = 'IngresoMovimiento' THEN o.kg ELSE 0 END) AS ingresomovimiento,
                        SUM(CASE WHEN o.tipooperacion = 'AjusteStock' THEN o.kg ELSE 0 END) AS ajustestock,
                        SUM(CASE WHEN o.tipooperacion = 'EgresoStock' THEN o.kg ELSE 0 END) AS egresostock,
                        SUM(CASE WHEN o.tipooperacion = 'EgresoMovimiento' THEN o.kg ELSE 0 END) AS egresomovimiento,
                        SUM(CASE WHEN o.tipooperacion = 'EgresoElaborado' THEN o.kg ELSE 0 END) AS egresoelaborado,
                        SUM(CASE WHEN o.tipooperacion = 'Ventas' THEN o.kg ELSE 0 END) AS ventas
                    FROM operaciones o
                    GROUP BY o.idsucursal, o.idcorte
                ),
                final AS (
                    SELECT
                        ac.idcorte, ac.codigo, ac.corte, ac.idsucursal, ac.sucursal, ac.fechaultimocierre,
                        COALESCE(r.stockinicial,0)::numeric(18,3) AS stockinicial,
                        COALESCE(r.compras,0)::numeric(18,3) AS compras,
                        COALESCE(r.ingresoelaborado,0)::numeric(18,3) AS ingresoelaborado,
                        COALESCE(r.ingresostock,0)::numeric(18,3) AS ingresostock,
                        COALESCE(r.ingresomovimiento,0)::numeric(18,3) AS ingresomovimiento,
                        COALESCE(r.ajustestock,0)::numeric(18,3) AS ajustestock,
                        COALESCE(r.egresostock,0)::numeric(18,3) AS egresostock,
                        COALESCE(r.egresomovimiento,0)::numeric(18,3) AS egresomovimiento,
                        COALESCE(r.egresoelaborado,0)::numeric(18,3) AS egresoelaborado,
                        COALESCE(r.ventas,0)::numeric(18,3) AS ventas,
                        ac.promedio, ac.puntostock, ac.pesable
                    FROM allcortes ac
                    LEFT JOIN resumen r ON r.idsucursal = ac.idsucursal AND r.idcorte = ac.idcorte
                )
                SELECT
                    f.idcorte, f.codigo, f.corte, f.idsucursal, f.sucursal, f.fechaultimocierre,
                    f.stockinicial, f.compras, f.ingresoelaborado, f.ingresostock, f.ingresomovimiento, f.ajustestock,
                    (f.compras + f.ingresoelaborado + f.ingresostock + f.ingresomovimiento + f.ajustestock)::numeric(18,3) AS totalingresos,
                    f.egresostock, f.egresomovimiento, f.egresoelaborado, f.ventas,
                    (f.egresostock + f.egresomovimiento + f.egresoelaborado + f.ventas)::numeric(18,3) AS totalegresos,
                    (f.stockinicial + f.compras + f.ingresoelaborado + f.ingresostock + f.ingresomovimiento + f.ajustestock
                     - f.egresostock - f.egresomovimiento - f.egresoelaborado - f.ventas)::numeric(18,3) AS stockactual,
                    f.promedio, f.puntostock, f.pesable,
                    CASE
                        WHEN (f.stockinicial + f.compras + f.ingresoelaborado + f.ingresostock + f.ingresomovimiento + f.ajustestock
                              - f.egresostock - f.egresomovimiento - f.egresoelaborado - f.ventas) < 0 THEN 'NEGATIVO'
                        WHEN f.puntostock > 0 AND (f.stockinicial + f.compras + f.ingresoelaborado + f.ingresostock + f.ingresomovimiento + f.ajustestock
                              - f.egresostock - f.egresomovimiento - f.egresoelaborado - f.ventas) <= f.puntostock THEN 'BAJO'
                        WHEN (f.stockinicial + f.compras + f.ingresoelaborado + f.ingresostock + f.ingresomovimiento + f.ajustestock
                              - f.egresostock - f.egresomovimiento - f.egresoelaborado - f.ventas) = 0 THEN 'SIN STOCK'
                        ELSE 'OK'
                    END AS estadostock
                FROM final f
                WHERE
                    @soloConStock = false
                    OR ABS(f.stockinicial + f.compras + f.ingresoelaborado + f.ingresostock + f.ingresomovimiento + f.ajustestock
                           - f.egresostock - f.egresomovimiento - f.egresoelaborado - f.ventas) > 0.000
                ORDER BY
                    CASE
                        WHEN TRIM(COALESCE(f.codigo,'')) <> '' AND TRIM(COALESCE(f.codigo,'')) ~ '^[0-9]+$'
                        THEN TRIM(f.codigo)::numeric(18,0)
                        ELSE 999999999999999999
                    END ASC,
                    TRIM(COALESCE(f.codigo,'')) ASC,
                    f.sucursal ASC;";

            return DbPg.Reader(_connectionString, _idEmpresa, sql,
                dr => new Entidades.ExistenciaStockPorSucursalPlanoVm
                {
                    IdCorte = dr["idcorte"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idcorte"]),
                    Codigo = dr["codigo"] == DBNull.Value ? 0L : Convert.ToInt64(dr["codigo"]),
                    Corte = dr["corte"] == DBNull.Value ? "" : Convert.ToString(dr["corte"]),
                    IdSucursal = dr["idsucursal"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idsucursal"]),
                    Sucursal = dr["sucursal"] == DBNull.Value ? "" : Convert.ToString(dr["sucursal"]),
                    FechaUltimoCierre = dr["fechaultimocierre"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["fechaultimocierre"]),
                    StockInicial = dr["stockinicial"] == DBNull.Value ? 0f : Convert.ToSingle(dr["stockinicial"]),
                    Compras = dr["compras"] == DBNull.Value ? 0f : Convert.ToSingle(dr["compras"]),
                    IngresoElaborado = dr["ingresoelaborado"] == DBNull.Value ? 0f : Convert.ToSingle(dr["ingresoelaborado"]),
                    IngresoStock = dr["ingresostock"] == DBNull.Value ? 0f : Convert.ToSingle(dr["ingresostock"]),
                    IngresoMovimiento = dr["ingresomovimiento"] == DBNull.Value ? 0f : Convert.ToSingle(dr["ingresomovimiento"]),
                    AjusteStock = dr["ajustestock"] == DBNull.Value ? 0f : Convert.ToSingle(dr["ajustestock"]),
                    TotalIngresos = dr["totalingresos"] == DBNull.Value ? 0f : Convert.ToSingle(dr["totalingresos"]),
                    EgresoStock = dr["egresostock"] == DBNull.Value ? 0f : Convert.ToSingle(dr["egresostock"]),
                    EgresoMovimiento = dr["egresomovimiento"] == DBNull.Value ? 0f : Convert.ToSingle(dr["egresomovimiento"]),
                    EgresoElaborado = dr["egresoelaborado"] == DBNull.Value ? 0f : Convert.ToSingle(dr["egresoelaborado"]),
                    Ventas = dr["ventas"] == DBNull.Value ? 0f : Convert.ToSingle(dr["ventas"]),
                    TotalEgresos = dr["totalegresos"] == DBNull.Value ? 0f : Convert.ToSingle(dr["totalegresos"]),
                    StockActual = dr["stockactual"] == DBNull.Value ? 0f : Convert.ToSingle(dr["stockactual"]),
                    Promedio = dr["promedio"] == DBNull.Value ? 0f : Convert.ToSingle(dr["promedio"]),
                    PuntoStock = dr["puntostock"] == DBNull.Value ? 0f : Convert.ToSingle(dr["puntostock"]),
                    Pesable = dr["pesable"] != DBNull.Value && Convert.ToBoolean(dr["pesable"]),
                    EstadoStock = dr["estadostock"] == DBNull.Value ? "" : Convert.ToString(dr["estadostock"])
                },
                p =>
                {
                    p.AddWithValue("textoLimpio", textoLimpio);
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idSucursal", idSucursal);
                    p.AddWithValue("fechaHasta", fechaHastaEfectiva);
                    p.AddWithValue("tipo", tipo ?? "");
                    p.AddWithValue("idProveedor", idProveedor);
                    p.AddWithValue("idMarca", idMarca);
                    p.AddWithValue("idCorte", idCorte);
                    p.AddWithValue("soloConStock", soloConStock);
                });
        }

        #endregion
    }
}
