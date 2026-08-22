using System;
using System.Collections.Generic;
using System.Data;
using Entidades;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres del bloque Ventas/LineaVenta/TemporalLineaVenta de
    // Contratos.IVentaRepository. El resto de Datos.Venta (Expendios, Sectores,
    // FacturaElectronica) no esta cubierto todavia -- se agrega en una etapa futura.
    // Ver docs/DECISIONS.md, Etapa 7.
    //
    // Feriados: NUNCA se porta a Postgres (tabla obsoleta, 0 filas reales, confirmado por el
    // usuario) -- agregarVenta omite esa consulta y usa diafestivo = NULL directamente, mismo
    // resultado observable que en SQL Server (la tabla vacia siempre resuelve NULL alli tambien).
    //
    // modificarVenta: el reverso de EgresosCaja cuando se editan lineas de una venta cta-cte
    // con un egreso previo NO esta implementado -- depende de EgresosCaja/TiposEgresoCaja,
    // dominio de CierreCaja.cs sin migrar. Gap real, rastreado en docs/GAPS.md, a resolver
    // obligatoriamente cuando se aborde CierreCaja.cs (no es opcional).
    //
    // agregarStockVenta: no-op deliberado -- el SP real solo toca StockCorteSucursal (tabla
    // obsoleta, nunca portada, decision de la Etapa 6).
    public class VentaPg : Contratos.IVentaRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;
        private readonly Contratos.IPersonaRepository _personaRepo;
        private readonly Contratos.ISucursalRepository _sucursalRepo;
        private readonly Contratos.ICorteRepository _corteRepo;

        public VentaPg(string connectionString, int idEmpresa,
            Contratos.IPersonaRepository personaRepo,
            Contratos.ISucursalRepository sucursalRepo,
            Contratos.ICorteRepository corteRepo)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
            _personaRepo = personaRepo ?? throw new ArgumentNullException(nameof(personaRepo));
            _sucursalRepo = sucursalRepo ?? throw new ArgumentNullException(nameof(sucursalRepo));
            _corteRepo = corteRepo ?? throw new ArgumentNullException(nameof(corteRepo));
        }

        public Contratos.IUnitOfWork IniciarUnitOfWork() => UnitOfWorkPg.Iniciar(_connectionString, _idEmpresa);

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

        private static long GetLong(NpgsqlDataReader dr, string columna) =>
            ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value ? Convert.ToInt64(dr[columna]) : 0L;

        // Arma un Corte completo a partir de columnas ya joineadas con alias "co_*" (ver
        // obtenerLineasVenta/GetLineasExpendio: ambas hacen INNER JOIN corte c ... LEFT JOIN
        // personas mk ON c.idmarca = mk.idpersona con esos mismos alias). Evita el
        // findCorteById() por fila que antes pagaba una conexion+transaccion completa por cada
        // linea -- mismo patron ya usado en CortePg.MapCorteListado. CorteMaestro nunca se
        // resuelve aca (ninguno de los dos callers lo necesitaba: ambos llamaban
        // findCorteById(id, buscarMaestro: false)).
        private static Corte MapCorteDesdeJoin(NpgsqlDataReader dr, int idCorte)
        {
            var corte = new Corte
            {
                IdCorte = idCorte,
                Codigo = GetLong(dr, "co_codigo"),
                CorteDesc = GetString(dr, "co_corte"),
                Tipo = GetString(dr, "co_tipo"),
                Promedio = GetFloat(dr, "co_promedio"),
                PuntoStock = GetInt(dr, "co_puntostock"),
                Nivel = GetInt(dr, "co_nivel"),
                IdEmpresa = GetInt(dr, "co_idempresa"),
                Porcentaje = GetFloat(dr, "co_porcentaje"),
                PrecioKg = GetFloat(dr, "co_preciokg"),
                PrecioKgReferencia = GetFloat(dr, "co_preciokg"),
                IngresoRapidoEmbutido = GetBool(dr, "co_ingresorapidoembutido"),
                Habilitado = GetBool(dr, "co_habilitado"),
                EnCierreStock = ColumnaExiste(dr, "co_encierrestock") && dr["co_encierrestock"] != DBNull.Value
                    ? Convert.ToBoolean(dr["co_encierrestock"]) : true,
                PorcentajeHueso = GetFloat(dr, "co_porcentajehueso"),
                Independiente = GetInt(dr, "co_independiente"),
                DesvioEstandar = GetFloat(dr, "co_desvioestandar"),
                Creado = dr["co_creado"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["co_creado"]),
                Actualizado = dr["co_actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["co_actualizado"]),
                IdAlicuotaIva = GetInt(dr, "co_idalicuotaiva"),
                AlicuotaIva = GetFloat(dr, "co_alicuotaiva"),
                Pesable = GetBool(dr, "co_pesable")
            };

            int idMarcaCorte = GetInt(dr, "co_idmarca");
            if (idMarcaCorte > 0)
                corte.Marca = new Persona { IdPersona = idMarcaCorte, RazonSocial = GetString(dr, "co_marcanombre") };

            corte.Presentacion = corte.EsPresentacion(corte.PorcentajeHueso);
            if (corte.Presentacion)
                corte.Porcentaje = corte.getCantPresentacion(corte.PorcentajeHueso);

            return corte;
        }

        // Columnas de corte + LEFT JOIN de marca a agregar al SELECT de cualquier query que ya
        // haga "... JOIN corte c ..." y quiera evitar el findCorteById() por fila -- usar junto
        // con MapCorteDesdeJoin(dr, idCorte). Los alias co_* evitan choque con columnas de la
        // tabla principal que se llaman igual (preciokg/idalicuotaiva/alicuotaiva/idempresa
        // existen tanto en lineaventa/lineaexpendio como en corte).
        private const string ColumnasCorteJoin = @"c.codigo AS co_codigo, c.corte AS co_corte, c.tipo AS co_tipo, c.promedio AS co_promedio,
                    c.puntostock AS co_puntostock, c.nivel AS co_nivel, c.porcentaje AS co_porcentaje,
                    c.preciokg AS co_preciokg, c.ingresorapidoembutido AS co_ingresorapidoembutido,
                    c.habilitado AS co_habilitado, c.encierrestock AS co_encierrestock,
                    c.porcentajehueso AS co_porcentajehueso, c.independiente AS co_independiente,
                    c.desvioestandar AS co_desvioestandar, c.creado AS co_creado, c.actualizado AS co_actualizado,
                    c.idalicuotaiva AS co_idalicuotaiva, c.alicuotaiva AS co_alicuotaiva, c.pesable AS co_pesable,
                    c.idmarca AS co_idmarca, c.idempresa AS co_idempresa, mk.razonsocial AS co_marcanombre";

        private static bool GetBool(NpgsqlDataReader dr, string columna) =>
            ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value && Convert.ToBoolean(dr[columna]);

        private Usuario GetUsuarioLiviano(int id)
        {
            if (id <= 0) return null;

            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT id, nombre, usuario AS user, email, idsucursaluser AS idsucursal, idempresa FROM usuarios WHERE id = @id;",
                dr => new Usuario
                {
                    Id = Convert.ToInt32(dr["id"]),
                    Nombre = GetString(dr, "nombre"),
                    User = GetString(dr, "user"),
                    Email = GetString(dr, "email"),
                    IdSucursal = dr["idsucursal"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idsucursal"]),
                    IdEmpresa = Convert.ToInt32(dr["idempresa"])
                },
                p => p.AddWithValue("id", id));

            return lista.Count > 0 ? lista[0] : null;
        }

        private void CargarRelacionesVenta(NpgsqlDataReader dr, Venta oVentaE)
        {
            bool tieneJoinVendedor = ColumnaExiste(dr, "vendedornombre");
            bool tieneJoinSucursal = ColumnaExiste(dr, "sucursalnombre");
            bool tieneJoinPersona = ColumnaExiste(dr, "personarazonsocial");

            oVentaE.Vendedor = tieneJoinVendedor
                ? new Usuario
                {
                    Id = oVentaE.IdVendedor,
                    Nombre = GetString(dr, "vendedornombre"),
                    User = GetString(dr, "vendedorusuario"),
                    Email = GetString(dr, "vendedoremail"),
                    IdSucursal = oVentaE.IdSucursal,
                    IdEmpresa = GetInt(dr, "vendedoridempresa")
                }
                : GetUsuarioLiviano(oVentaE.IdVendedor);

            oVentaE.Sucursal = tieneJoinSucursal
                ? new Sucursal
                {
                    IdSucursal = oVentaE.IdSucursal,
                    SucursalNombre = GetString(dr, "sucursalnombre"),
                    IdEmpresa = GetInt(dr, "sucursalidempresa"),
                    CodPuntoVentaAfip = GetInt(dr, "sucursalcodpuntoventaafip"),
                    Direccion = GetString(dr, "sucursaldireccion"),
                    Localidad = GetString(dr, "sucursallocalidad"),
                    Provincia = GetString(dr, "sucursalprovincia"),
                    Pais = GetString(dr, "sucursalpais")
                }
                : _sucursalRepo.findById(oVentaE.IdSucursal);

            if (tieneJoinPersona)
            {
                oVentaE.Persona = new Persona
                {
                    idPersona = oVentaE.IdPersona,
                    razonSocial = GetString(dr, "personarazonsocial"),
                    Identificacion = GetString(dr, "personaidentificacion"),
                    IdIva = GetInt(dr, "personaidiva"),
                    Iva = GetString(dr, "personaiva"),
                    Cuit = GetString(dr, "personacuit"),
                    Telefono = GetString(dr, "personatelefono"),
                    Domicilio = GetString(dr, "personadomicilio"),
                    Ciudad = GetString(dr, "personaciudad"),
                    CtaCte = GetBool(dr, "personactacte"),
                    Bonificacion = GetFloat(dr, "personabonificacion")
                };
            }
            else
            {
                oVentaE.Persona = _personaRepo.findById(oVentaE.IdPersona);
            }
        }

        private Venta MapVenta(NpgsqlDataReader dr, bool cargarLineas = true)
        {
            string columnaCreado = ColumnaExiste(dr, "creado") ? "creado" : "ventacreado";
            string columnaActualizado = ColumnaExiste(dr, "actualizado") ? "actualizado" : "ventaactualizado";

            var oVentaE = new Venta
            {
                IdVenta = Convert.ToInt32(dr["idventa"]),
                FechaVenta = Convert.ToDateTime(dr["fechaventa"]),
                Turno = GetString(dr, "turno"),
                DiaFestivo = GetString(dr, "diafestivo"),
                Observaciones = GetString(dr, "observaciones"),
                NroRemito = GetString(dr, "nroremito"),
                Estado = GetString(dr, "estado"),
                EnCtaCte = dr["enctacte"] != DBNull.Value && Convert.ToBoolean(dr["enctacte"]),
                Cuit = GetString(dr, "cuit"),
                Email = GetString(dr, "email"),
                FormaPago = GetString(dr, "formapago"),
                TipoComprobante = dr["tipocomprobante"] == DBNull.Value ? 'X' : Convert.ToChar(GetString(dr, "tipocomprobante").Length > 0 ? GetString(dr, "tipocomprobante")[0] : 'X'),
                Creado = dr[columnaCreado] != DBNull.Value ? Convert.ToDateTime(dr[columnaCreado]) : DateTime.MinValue,
                Actualizado = dr[columnaActualizado] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr[columnaActualizado]),
                PagoMixtoEfectivo = dr["pagomixtoefectivo"] == DBNull.Value ? 0f : Convert.ToSingle(dr["pagomixtoefectivo"]),
                IdVendedor = dr["idvendedor"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idvendedor"]),
                IdSucursal = dr["idsucursal"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idsucursal"]),
                IdPersona = dr["idpersona"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idpersona"])
            };

            CargarRelacionesVenta(dr, oVentaE);

            if (cargarLineas)
            {
                oVentaE.LineasVenta = obtenerLineasVenta(oVentaE.IdVenta);
                oVentaE.CantItems = oVentaE.getCantItems(oVentaE).ToString();
            }
            else if (ColumnaExiste(dr, "cantitemscalculado"))
            {
                oVentaE.CantItems = GetInt(dr, "cantitemscalculado").ToString();
            }

            oVentaE.TotalImporte = ColumnaExiste(dr, "totalimportecalculado")
                ? GetFloat(dr, "totalimportecalculado")
                : getTotalVenta(oVentaE.IdVenta);
            oVentaE.TotalImporteOriginal = oVentaE.TotalImporte;

            return oVentaE;
        }

        private static Venta MapVentaBalance(NpgsqlDataReader dr) => new Venta
        {
            IdVenta = Convert.ToInt32(dr["idventa"]),
            Estado = GetString(dr, "estado"),
            EnCtaCte = dr["enctacte"] != DBNull.Value && Convert.ToBoolean(dr["enctacte"]),
            FormaPago = GetString(dr, "formapago"),
            PagoMixtoEfectivo = dr["pagomixtoefectivo"] == DBNull.Value ? 0f : Convert.ToSingle(dr["pagomixtoefectivo"]),
            TotalKgs = dr["totalkg"] == DBNull.Value ? 0f : Convert.ToSingle(dr["totalkg"]),
            TotalImporte = dr["totalimportecalculado"] == DBNull.Value ? 0f : Convert.ToSingle(dr["totalimportecalculado"]),
            TotalImporteOriginal = dr["totalimportecalculado"] == DBNull.Value ? 0f : Convert.ToSingle(dr["totalimportecalculado"])
        };

        #endregion

        #region Ventas

        public Venta getVentaById(int idVenta)
        {
            // Mismos JOINs que getAllVentas (vendedor/sucursal/persona+iva/total calculado) --
            // antes esta consulta era un SELECT * sin joins, asi que CargarRelacionesVenta caia
            // siempre en el fallback de _sucursalRepo.findById/_personaRepo.findById/
            // GetUsuarioLiviano (3 conexiones+transacciones extra) y ademas TotalImporte pegaba
            // otra vez a getTotalVenta() -- 4 queries de mas en cada carga de una sola venta
            // (DetalleVenta, "Modificar venta" del POS). Bug de performance real, no de motor
            // (getAllVentas ya evitaba esto por tener los joins) -- ver docs/DECISIONS.md,
            // 2026-08-21. El subquery de totalimportecalculado es la misma formula exacta que
            // getTotalVenta (SUM(cantkg*preciokg) sobre lineaventa, sin filtrar anuladas) --
            // mismo resultado, sin la query aparte.
            const string sql = @"
                SELECT
                    v.*,
                    COALESCE(lvt.totalimportecalculado, 0) AS totalimportecalculado,
                    COALESCE(lvt.cantitemscalculado, 0) AS cantitemscalculado,
                    u.nombre AS vendedornombre, u.usuario AS vendedorusuario, u.email AS vendedoremail, u.idempresa AS vendedoridempresa,
                    s.sucursal AS sucursalnombre, s.idempresa AS sucursalidempresa, s.codpuntoventaafip AS sucursalcodpuntoventaafip,
                    s.direccion AS sucursaldireccion, s.localidad AS sucursallocalidad, s.provincia AS sucursalprovincia, s.pais AS sucursalpais,
                    p.razonsocial AS personarazonsocial, p.identificacion AS personaidentificacion, p.idiva AS personaidiva, iv.iva AS personaiva,
                    p.cuit AS personacuit, p.telefono AS personatelefono, p.domicilio AS personadomicilio, p.ciudad AS personaciudad,
                    p.ctacte AS personactacte, p.bonificacion AS personabonificacion
                FROM ventas v
                LEFT JOIN usuarios u ON u.id = v.idvendedor
                LEFT JOIN sucursal s ON s.idsucursal = v.idsucursal
                LEFT JOIN personas p ON p.idpersona = v.idpersona
                LEFT JOIN iva iv ON iv.id = p.idiva
                LEFT JOIN (
                    SELECT idventa, SUM(cantkg * preciokg) AS totalimportecalculado, COUNT(*) AS cantitemscalculado
                    FROM lineaventa GROUP BY idventa
                ) lvt ON lvt.idventa = v.idventa
                WHERE v.idventa = @idVenta;";

            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                sql,
                dr => MapVenta(dr, true),
                p => p.AddWithValue("idVenta", idVenta));

            return lista.Count > 0 ? lista[0] : null;
        }

        public List<Venta> getAllVentas(DateTime fechaDesde, DateTime fechaHasta, string texto, int? idVendedor, int? idCliente, int? idSucursal, bool soloAnulados, bool cargarLineas)
        {
            const string sql = @"
                SELECT
                    v.*,
                    COALESCE(lv.totalimportecalculado, 0) AS totalimportecalculado,
                    COALESCE(lv.cantitemscalculado, 0) AS cantitemscalculado,
                    u.nombre AS vendedornombre, u.usuario AS vendedorusuario, u.email AS vendedoremail, u.idempresa AS vendedoridempresa,
                    s.sucursal AS sucursalnombre, s.idempresa AS sucursalidempresa, s.codpuntoventaafip AS sucursalcodpuntoventaafip,
                    s.direccion AS sucursaldireccion, s.localidad AS sucursallocalidad, s.provincia AS sucursalprovincia, s.pais AS sucursalpais,
                    p.razonsocial AS personarazonsocial, p.identificacion AS personaidentificacion, p.idiva AS personaidiva, iv.iva AS personaiva,
                    p.cuit AS personacuit, p.telefono AS personatelefono, p.domicilio AS personadomicilio, p.ciudad AS personaciudad,
                    p.ctacte AS personactacte, p.bonificacion AS personabonificacion
                FROM ventas v
                LEFT JOIN usuarios u ON u.id = v.idvendedor
                LEFT JOIN sucursal s ON s.idsucursal = v.idsucursal
                LEFT JOIN personas p ON p.idpersona = v.idpersona
                LEFT JOIN iva iv ON iv.id = p.idiva
                LEFT JOIN (
                    SELECT idventa, SUM(cantkg * preciokg) AS totalimportecalculado, COUNT(*) AS cantitemscalculado
                    FROM lineaventa GROUP BY idventa
                ) lv ON lv.idventa = v.idventa
                WHERE v.fechaventa >= @fechaDesde AND v.fechaventa < @fechaHastaMas1
                  AND (@idVendedor = -1 OR v.idvendedor = @idVendedor)
                  AND (@idCliente = -1 OR v.idpersona = @idCliente)
                  AND (@idSucursal = -1 OR v.idsucursal = @idSucursal)
                  AND (@soloAnulados = false OR v.estado = 'ANULADO')
                  AND (
                        @texto = '' OR
                        v.nroremito ILIKE @textoLike OR
                        v.observaciones ILIKE @textoLike OR
                        p.razonsocial ILIKE @textoLike OR
                        p.identificacion ILIKE @textoLike
                      )
                ORDER BY v.fechaventa DESC;";

            return DbPg.Reader(_connectionString, _idEmpresa, sql, dr => MapVenta(dr, cargarLineas), p =>
            {
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHastaMas1", fechaHasta.AddDays(1));
                p.AddWithValue("texto", (texto ?? "").Trim());
                p.AddWithValue("textoLike", "%" + (texto ?? "").Trim() + "%");
                p.AddWithValue("idVendedor", idVendedor ?? -1);
                p.AddWithValue("idCliente", idCliente ?? -1);
                p.AddWithValue("idSucursal", idSucursal ?? -1);
                p.AddWithValue("soloAnulados", soloAnulados);
            });
        }

        public List<Venta> getVentasBalancePeriodo(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal)
        {
            const string sql = @"
                SELECT
                    v.idventa, v.estado, v.enctacte, v.formapago, v.pagomixtoefectivo,
                    COALESCE(SUM(lv.cantkg), 0) AS totalkg,
                    COALESCE(SUM(lv.cantkg * lv.preciokg), 0) AS totalimportecalculado
                FROM ventas v
                LEFT JOIN lineaventa lv ON lv.idventa = v.idventa
                WHERE v.fechaventa >= @fechaDesde AND v.fechaventa < @fechaHastaMas1
                  AND (@idSucursal = -1 OR v.idsucursal = @idSucursal)
                  AND COALESCE(v.estado, '') <> 'ANULADO'
                GROUP BY v.idventa, v.estado, v.enctacte, v.formapago, v.pagomixtoefectivo
                ORDER BY v.idventa;";

            return DbPg.Reader(_connectionString, _idEmpresa, sql, MapVentaBalance, p =>
            {
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHastaMas1", fechaHasta.AddDays(1));
                p.AddWithValue("idSucursal", idSucursal ?? -1);
            });
        }

        public decimal getTotalKgsPesablesBalancePeriodo(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal, bool incluirVentasCuentaCorriente)
        {
            const string sql = @"
                SELECT COALESCE(SUM(lv.cantkg), 0)
                FROM ventas v
                INNER JOIN lineaventa lv ON lv.idventa = v.idventa
                INNER JOIN corte c ON c.idcorte = lv.idcorte
                WHERE v.fechaventa >= @fechaDesde AND v.fechaventa < @fechaHastaMas1
                  AND (@idSucursal = -1 OR v.idsucursal = @idSucursal)
                  AND COALESCE(v.estado, '') <> 'ANULADO'
                  AND (@incluirVentasCuentaCorriente = true OR COALESCE(v.enctacte, false) = false)
                  AND COALESCE(c.pesable, false) = true;";

            object result = DbPg.Scalar(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHastaMas1", fechaHasta.AddDays(1));
                p.AddWithValue("idSucursal", idSucursal ?? -1);
                p.AddWithValue("incluirVentasCuentaCorriente", incluirVentasCuentaCorriente);
            });

            return result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
        }

        public int agregarVenta(Venta oVentaE, Contratos.IUnitOfWork unitOfWork = null)
        {
            const string sql = @"
                INSERT INTO ventas (idvendedor, fechaventa, idsucursal, turno, diafestivo, observaciones, idpersona,
                    nroremito, estado, enctacte, formapago, tipocomprobante, cuit, email, acumredondeokgs,
                    acumredondeoimporte, comisiontarjeta, pagomixtoefectivo, creado, idempresa)
                VALUES (@idVendedor, @fechaVenta, @idSucursal, @turno, NULL, @observaciones, @idPersona,
                    @nroRemito, '', @enCtaCte, @formaPago, @tipoComprobante, @cuit, @email, @acumRedondeoKgs,
                    @acumRedondeoImporte, @comisionTarjeta, @pagoMixtoEfectivo, now(), @idEmpresa);
                SELECT idventa FROM ventas WHERE idsucursal = @idSucursal ORDER BY idventa DESC LIMIT 1;";

            Action<NpgsqlParameterCollection> setParams = p =>
            {
                p.AddWithValue("idVendedor", oVentaE.Vendedor.Id);
                p.AddWithValue("fechaVenta", oVentaE.FechaVenta);
                p.AddWithValue("idSucursal", oVentaE.Sucursal.idSucursal);
                p.AddWithValue("turno", oVentaE.Turno ?? "");
                p.AddWithValue("observaciones", oVentaE.Observaciones ?? "");
                p.AddWithValue("idPersona", oVentaE.Persona.idPersona);
                p.AddWithValue("nroRemito", oVentaE.NroRemito ?? "");
                p.AddWithValue("enCtaCte", oVentaE.EnCtaCte);
                p.AddWithValue("formaPago", oVentaE.FormaPago ?? "");
                p.AddWithValue("tipoComprobante", oVentaE.TipoComprobante.ToString());
                p.AddWithValue("cuit", oVentaE.Cuit ?? "");
                p.AddWithValue("email", oVentaE.Email ?? "");
                p.AddWithValue("acumRedondeoKgs", oVentaE.AcumRedondeoKgs);
                p.AddWithValue("acumRedondeoImporte", oVentaE.AcumRedondeoImporte);
                p.AddWithValue("comisionTarjeta", oVentaE.ComisionTarjeta);
                p.AddWithValue("idEmpresa", _idEmpresa);
                p.AddWithValue("pagoMixtoEfectivo", oVentaE.PagoMixtoEfectivo);
            };

            object result;
            var uow = unitOfWork as UnitOfWorkPg;
            if (uow != null)
            {
                using (var cmd = new NpgsqlCommand(sql, uow.Connection, uow.Transaction))
                {
                    setParams(cmd.Parameters);
                    result = cmd.ExecuteScalar();
                }
            }
            else
            {
                result = DbPg.Scalar(_connectionString, _idEmpresa, sql, setParams);
            }

            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public void modificarVenta(Venta oVentaE, int sucAnterior, bool eliminarLineas, Contratos.IUnitOfWork unitOfWork = null)
        {
            var uowCompartida = unitOfWork as UnitOfWorkPg;
            if (uowCompartida != null)
            {
                EjecutarModificarVenta(oVentaE, eliminarLineas, uowCompartida.Connection, uowCompartida.Transaction);
                return;
            }

            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    EjecutarModificarVenta(oVentaE, eliminarLineas, con, tx);
                    tx?.Commit();
                }
                catch
                {
                    try { tx?.Rollback(); } catch { }
                    throw;
                }
            }
        }

        private void EjecutarModificarVenta(Venta oVentaE, bool eliminarLineas, NpgsqlConnection con, NpgsqlTransaction tx)
        {
                    if (eliminarLineas)
                    {
                        using (var cmdDel = new NpgsqlCommand("DELETE FROM lineaventa WHERE idventa = @idVenta;", con, tx))
                        {
                            cmdDel.Parameters.AddWithValue("idVenta", oVentaE.IdVenta);
                            cmdDel.ExecuteNonQuery();
                        }
                    }

                    using (var cmd = new NpgsqlCommand(@"
                        UPDATE ventas SET fechaventa=@fechaVenta, turno=@turno, idsucursal=@idSucNueva, idvendedor=@idVendedor,
                            observaciones=@observaciones, idpersona=@idPersona, nroremito=@nroRemito, estado=@estado,
                            enctacte=@enCtaCte, formapago=@formaPago, tipocomprobante=@tipoComprobante, cuit=@cuit,
                            email=@email, acumredondeokgs=@acumRedondeoKgs, acumredondeoimporte=@acumRedondeoImporte,
                            comisiontarjeta=@comisionTarjeta, pagomixtoefectivo=@pagoMixtoEfectivo, actualizado=now()
                        WHERE idventa=@idVenta;", con, tx))
                    {
                        cmd.Parameters.AddWithValue("fechaVenta", oVentaE.FechaVenta);
                        cmd.Parameters.AddWithValue("turno", oVentaE.Turno ?? "");
                        cmd.Parameters.AddWithValue("idSucNueva", oVentaE.Sucursal.idSucursal);
                        cmd.Parameters.AddWithValue("idVendedor", oVentaE.Vendedor.Id);
                        cmd.Parameters.AddWithValue("observaciones", oVentaE.Observaciones ?? "");
                        cmd.Parameters.AddWithValue("idPersona", oVentaE.Persona.idPersona);
                        cmd.Parameters.AddWithValue("nroRemito", oVentaE.NroRemito ?? "");
                        cmd.Parameters.AddWithValue("estado", oVentaE.Estado ?? "");
                        cmd.Parameters.AddWithValue("enCtaCte", oVentaE.EnCtaCte);
                        cmd.Parameters.AddWithValue("formaPago", oVentaE.FormaPago ?? "");
                        cmd.Parameters.AddWithValue("tipoComprobante", oVentaE.TipoComprobante.ToString());
                        cmd.Parameters.AddWithValue("cuit", oVentaE.Cuit ?? "");
                        cmd.Parameters.AddWithValue("email", oVentaE.Email ?? "");
                        cmd.Parameters.AddWithValue("acumRedondeoKgs", oVentaE.AcumRedondeoKgs);
                        cmd.Parameters.AddWithValue("acumRedondeoImporte", oVentaE.AcumRedondeoImporte);
                        cmd.Parameters.AddWithValue("comisionTarjeta", oVentaE.ComisionTarjeta);
                        cmd.Parameters.AddWithValue("pagoMixtoEfectivo", oVentaE.PagoMixtoEfectivo);
                        cmd.Parameters.AddWithValue("idVenta", oVentaE.IdVenta);
                        cmd.ExecuteNonQuery();
                    }

                    // Reverso de EgresosCaja (gap cerrado en la Etapa 8, ver docs/DECISIONS.md):
                    // espeja exacto el SP real modificarVenta -- busca el ultimo EgresosCaja con
                    // tabla='Ventas' AND idtabla=@idVenta, y si su monto es positivo, copia toda
                    // la fila a un registro nuevo con el monto negado y la descripcion prefijada
                    // "Anulado:". Misma transaccion que el resto del metodo.
                    object montoObj;
                    using (var cmdMonto = new NpgsqlCommand(
                        "SELECT monto FROM egresoscaja WHERE tabla = 'Ventas' AND idtabla = @idVenta ORDER BY id DESC LIMIT 1;", con, tx))
                    {
                        cmdMonto.Parameters.AddWithValue("idVenta", oVentaE.IdVenta);
                        montoObj = cmdMonto.ExecuteScalar();
                    }

                    double monto = montoObj == null || montoObj == DBNull.Value ? 0 : Convert.ToDouble(montoObj);
                    if (monto > 0)
                    {
                        using (var cmdReverso = new NpgsqlCommand(@"
                            INSERT INTO egresoscaja (fechahora, idtipoegresocaja, descripcion, detalle, monto, idsucursal,
                                creado, creadopor, actualizado, actualizadopor, idcompra, esgasto, tabla, idtabla, idempresa)
                            SELECT fechahora, idtipoegresocaja, '' || 'Anulado:' || descripcion, detalle, -1 * monto, idsucursal,
                                now(), creadopor, NULL, NULL, idcompra, esgasto, tabla, idtabla, idempresa
                            FROM egresoscaja
                            WHERE tabla = 'Ventas' AND idtabla = @idVenta
                            ORDER BY id DESC LIMIT 1;", con, tx))
                        {
                            cmdReverso.Parameters.AddWithValue("idVenta", oVentaE.IdVenta);
                            cmdReverso.ExecuteNonQuery();
                        }
                    }
        }

        public DataTable obtenerVentas(int idSucursal, int idCliente, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool soloAnulados)
        {
            var sql = new System.Text.StringBuilder(@"
                SELECT v.idventa, v.fechaventa, v.idvendedor, u.nombre, v.nroremito,
                    v.idpersona, p.razonsocial, v.idsucursal, s.sucursal,
                    v.tipocomprobante, v.enctacte, v.formapago, v.pagomixtoefectivo, v.comisiontarjeta,
                    v.turno, v.diafestivo, v.observaciones, v.creado, v.actualizado, v.estado,
                    SUM(l.kgsajustetarj) AS totalkgaj,
                    SUM(l.kgsajustetarj * l.preciokg) AS totalimpaj,
                    SUM(l.cantkg) AS totalkg,
                    SUM(l.cantkg * l.preciokg) AS totals,
                    (v.comisiontarjeta * SUM(l.cantkg * l.preciokg)) AS totcomtarj,
                    SUM(l.ajusteprecio * l.cantkg) AS totajuste
                FROM lineaventa l
                INNER JOIN ventas v ON l.idventa = v.idventa
                INNER JOIN sucursal s ON v.idsucursal = s.idsucursal
                INNER JOIN personas p ON v.idpersona = p.idpersona
                INNER JOIN usuarios u ON v.idvendedor = u.id
                WHERE v.fechaventa BETWEEN @fechaDesde AND @fechaHasta");

            if (idSucursal >= 0) sql.Append(" AND v.idsucursal = @idSucursal");
            if (idVendedor >= 0) sql.Append(" AND v.idvendedor = @idVendedor");
            if (idCliente >= 0) sql.Append(" AND v.idpersona = @idCliente");
            if (!string.IsNullOrWhiteSpace(texto))
            {
                sql.Append(@"
                    AND (
                        CAST(v.idventa AS text) ILIKE @textoLike OR
                        v.nroremito ILIKE @textoLike OR
                        p.razonsocial ILIKE @textoLike OR
                        v.diafestivo ILIKE @textoLike OR
                        to_char(v.fechaventa, 'FMDay') ILIKE @textoLike
                    )");
            }
            if (soloAnulados) sql.Append(" AND l.cantkg < 0");

            sql.Append(@"
                GROUP BY v.idventa, v.fechaventa, v.idvendedor, u.nombre, v.nroremito,
                    v.idpersona, p.razonsocial, v.idsucursal, s.sucursal,
                    v.tipocomprobante, v.enctacte, v.formapago, v.pagomixtoefectivo, v.comisiontarjeta,
                    v.turno, v.diafestivo, v.observaciones, v.creado, v.actualizado,
                    v.estado, v.acumredondeokgs, v.acumredondeoimporte
                ORDER BY v.fechaventa DESC;");

            return DbPg.DataTable(_connectionString, _idEmpresa, sql.ToString(), p =>
            {
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                if (idSucursal >= 0) p.AddWithValue("idSucursal", idSucursal);
                if (idVendedor >= 0) p.AddWithValue("idVendedor", idVendedor);
                if (idCliente >= 0) p.AddWithValue("idCliente", idCliente);
                if (!string.IsNullOrWhiteSpace(texto)) p.AddWithValue("textoLike", "%" + texto.Trim() + "%");
            });
        }

        public DataTable getVentasVendedorCierreCaja(CierreCaja oCierreE, bool soloAnulados)
        {
            const string sql = @"
                SELECT v.idventa, v.fechaventa, v.idvendedor, u.nombre, v.nroremito, v.idpersona, p.razonsocial,
                    v.idsucursal, s.sucursal, v.formapago, v.tipocomprobante, v.observaciones, v.creado, v.actualizado,
                    v.estado, SUM(l.cantkg) AS totalkg, SUM(l.cantkg * l.preciokg) AS totals
                FROM lineaventa l
                INNER JOIN ventas v ON l.idventa = v.idventa
                INNER JOIN sucursal s ON v.idsucursal = s.idsucursal
                INNER JOIN personas p ON v.idpersona = p.idpersona
                INNER JOIN usuarios u ON v.idvendedor = u.id
                WHERE v.fechaventa BETWEEN @fechaDesde AND @fechaHasta AND v.idsucursal = @idSucursal AND v.idvendedor = @idVendedor
                  AND (v.nroremito ILIKE @textoLike OR p.razonsocial ILIKE @textoLike)
                  AND (@soloAnulados = false OR (@soloAnulados = true AND l.cantkg < 0))
                GROUP BY v.idventa, v.fechaventa, v.idvendedor, u.nombre, v.nroremito, v.idpersona, p.razonsocial,
                    v.idsucursal, s.sucursal, v.formapago, v.tipocomprobante, v.observaciones, v.creado, v.actualizado, v.estado
                ORDER BY v.fechaventa DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idVendedor", oCierreE.UsuarioInicio.Id);
                p.AddWithValue("fechaDesde", oCierreE.FechaHoraInicio);
                p.AddWithValue("fechaHasta", oCierreE.FechaHoraCierre ?? DateTime.Now);
                p.AddWithValue("idSucursal", oCierreE.Sucursal.idSucursal);
                p.AddWithValue("textoLike", "%%");
                p.AddWithValue("soloAnulados", soloAnulados);
            });
        }

        public float getTotalVenta(int idVenta)
        {
            object result = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT SUM(cantkg * preciokg) FROM lineaventa WHERE idventa = @idVenta;",
                p => p.AddWithValue("idVenta", idVenta));

            return result == null || result == DBNull.Value ? 0f : Convert.ToSingle(result);
        }

        public float getTotalKgsVenta(int idVenta)
        {
            object result = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT SUM(cantkg) FROM lineaventa WHERE idventa = @idVenta;",
                p => p.AddWithValue("idVenta", idVenta));

            return result == null || result == DBNull.Value ? 0f : Convert.ToSingle(result);
        }

        public float obtenerTotalVentas(int idVendedor, int idSucursal, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            const string sql = @"
                SELECT SUM(l.cantkg * l.preciokg) AS totals
                FROM lineaventa l
                INNER JOIN ventas v ON l.idventa = v.idventa
                INNER JOIN sucursal s ON v.idsucursal = s.idsucursal
                WHERE v.fechaventa BETWEEN @fechaDesde AND @fechaHasta AND v.idvendedor = @idVendedor AND v.idsucursal = @idSucursal;";

            object result = DbPg.Scalar(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idVendedor", idVendedor);
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("fechaDesde", (object)fechaDesde ?? DBNull.Value);
                p.AddWithValue("fechaHasta", (object)fechaHasta ?? DBNull.Value);
            });

            return result == null || result == DBNull.Value ? 0f : Convert.ToSingle(result);
        }

        public LineaVenta agregarLineaVenta(LineaVenta oLineaE, Contratos.IUnitOfWork unitOfWork = null)
        {
            const string sql = @"
                INSERT INTO lineaventa (idventa, idcorte, idanulado, idlineaventaanulado, cantkg, kgsajustetarj,
                    porckgsajustetarj, idalicuotaiva, alicuotaiva, preciokg, ajusteprecio, pesobalanza, bonificacion, idempresa)
                VALUES (@idVenta, @idCorte, @idAnulado, @idLineaVentaAnulado, @cantKg, @kgsAjusteTarj,
                    @porcKgsAjusteTarj, @idAlicuotaIva, @alicuotaIva, @precioKg, @ajustePrecio, @pesoBalanza, @bonificacion, @idEmpresa)
                RETURNING idlineaventa;";

            Action<NpgsqlParameterCollection> setParams = p =>
            {
                p.AddWithValue("idVenta", oLineaE.Venta.IdVenta);
                p.AddWithValue("idCorte", oLineaE.Corte.idCorte);
                p.AddWithValue("idAnulado", oLineaE.Estado);
                p.AddWithValue("idLineaVentaAnulado", oLineaE.IndexAnulado);
                p.AddWithValue("cantKg", Math.Round(oLineaE.CantKg, 3));
                p.AddWithValue("kgsAjusteTarj", Math.Round(oLineaE.KgsAjusteTarj, 3));
                p.AddWithValue("porcKgsAjusteTarj", oLineaE.CantKg == 0 ? 0 : Math.Round(oLineaE.KgsAjusteTarj / oLineaE.CantKg, 3));
                p.AddWithValue("idAlicuotaIva", oLineaE.Corte.IdAlicuotaIva);
                p.AddWithValue("alicuotaIva", oLineaE.Corte.AlicuotaIva);
                p.AddWithValue("precioKg", Math.Round(oLineaE.PrecioKg, 2));
                p.AddWithValue("ajustePrecio", Math.Round(oLineaE.AjustePrecio, 2));
                p.AddWithValue("pesoBalanza", oLineaE.PesoBalanza);
                p.AddWithValue("bonificacion", oLineaE.Bonificacion);
                p.AddWithValue("idEmpresa", _idEmpresa);
            };

            object nuevoId;
            var uow = unitOfWork as UnitOfWorkPg;
            if (uow != null)
            {
                using (var cmd = new NpgsqlCommand(sql, uow.Connection, uow.Transaction))
                {
                    setParams(cmd.Parameters);
                    nuevoId = cmd.ExecuteScalar();
                }
            }
            else
            {
                nuevoId = DbPg.Scalar(_connectionString, _idEmpresa, sql, setParams);
            }

            oLineaE.IdLineaVenta = Convert.ToInt32(nuevoId);
            return oLineaE;
        }

        public void actualizarAlicuotaLineaVenta(int idLineaVenta, int idAlicuotaIva, float alicuotaIva)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE lineaventa SET idalicuotaiva = @idAlicuotaIva, alicuotaiva = @alicuotaIva WHERE idlineaventa = @idLineaVenta;",
                p =>
                {
                    p.AddWithValue("idLineaVenta", idLineaVenta);
                    p.AddWithValue("idAlicuotaIva", idAlicuotaIva);
                    p.AddWithValue("alicuotaIva", alicuotaIva);
                });
        }

        public void eliminarLineasVenta(int idVenta)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM lineaventa WHERE idventa = @idVenta;",
                p => p.AddWithValue("idVenta", idVenta));
        }

        public Venta getUltimaVentaVendedor(CierreCaja oCierreE)
        {
            object scalar = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT idventa FROM ventas WHERE idvendedor = @idVendedor AND idsucursal = @idSucursal ORDER BY idventa DESC LIMIT 1;",
                p =>
                {
                    p.AddWithValue("idVendedor", oCierreE.UsuarioInicio.Id);
                    p.AddWithValue("idSucursal", oCierreE.Sucursal.idSucursal);
                });

            int idVenta = scalar == null || scalar == DBNull.Value ? 0 : Convert.ToInt32(scalar);
            return idVenta > 0 ? getVentaById(idVenta) : null;
        }

        public List<LineaVenta> obtenerLineasVenta(int idVenta)
        {
            // INNER JOIN a corte a proposito: el SP real (obtenerLineasVenta, verificado con
            // sp_helptext) hace el mismo INNER JOIN, asi que una linea cuyo idCorte no es
            // visible para el tenant actual (RLS de Corte, ej. un idCorte de otra empresa por
            // un dato viejo/cruzado) queda excluida del resultado -- no solo con Corte=null.
            // Confirmado con datos reales (Venta #23, idEmpresa=1, 2 lineas con idCorte=3 que
            // pertenece a idEmpresa=3).
            //
            // Antes este JOIN se usaba solo para ese filtro y las columnas de corte se
            // descartaban -- Corte se hidrataba con un _corteRepo.findCorteById() (conexion +
            // transaccion propia) POR CADA LINEA. Bug de performance real (no de motor):
            // confirmado con datos reales que una venta de 12 lineas tardaba varios cientos de
            // ms de mas por esto solo -- ver docs/DECISIONS.md, 2026-08-21. Fix: se agregan las
            // columnas de corte (con alias co_* para no chocar con columnas de lineaventa que
            // se llaman igual: preciokg, idalicuotaiva, alicuotaiva, idempresa) al mismo SELECT
            // que ya hacia el JOIN, y se arma el Corte directo de la fila -- mismo patron ya
            // usado en CortePg.MapCorteListado/ObtenerCortesPorEmpresaListado para el mismo
            // problema. Marca se resuelve con un LEFT JOIN liviano (igual que alli) en vez de
            // buscarCorteById con maestro (que ya venia en false aca, asi que CorteMaestro
            // nunca se poblaba -- sin cambio de comportamiento en ese punto).
            return DbPg.Reader(_connectionString, _idEmpresa,
                @"SELECT lv.idlineaventa, lv.idventa, lv.idcorte, lv.cantkg, lv.idalicuotaiva, lv.alicuotaiva, lv.preciokg,
                    lv.kgsajustetarj, lv.bonificacion, lv.idlineaventaanulado, lv.pesobalanza, lv.idanulado,
                    " + ColumnasCorteJoin + @"
                  FROM lineaventa lv
                  INNER JOIN corte c ON lv.idcorte = c.idcorte
                  LEFT JOIN personas mk ON c.idmarca = mk.idpersona
                  WHERE lv.idventa = @idVenta
                  ORDER BY c.codigo;",
                dr =>
                {
                    var oLinea = new LineaVenta
                    {
                        IdLineaVenta = Convert.ToInt32(dr["idlineaventa"]),
                        Venta = new Venta { IdVenta = Convert.ToInt32(dr["idventa"]) },
                        Corte = MapCorteDesdeJoin(dr, Convert.ToInt32(dr["idcorte"])),
                        CantKg = dr["cantkg"] == DBNull.Value ? 0 : Convert.ToSingle(dr["cantkg"]),
                        IdAlicuotaIva = dr["idalicuotaiva"] == DBNull.Value ? 0 : Convert.ToSingle(dr["idalicuotaiva"]),
                        AlicuotaIva = dr["alicuotaiva"] == DBNull.Value ? 0 : Convert.ToSingle(dr["alicuotaiva"]),
                        PrecioKg = dr["preciokg"] == DBNull.Value ? 0 : Convert.ToSingle(dr["preciokg"]),
                        KgsAjusteTarj = dr["kgsajustetarj"] == DBNull.Value ? 0 : Convert.ToSingle(dr["kgsajustetarj"]),
                        Bonificacion = dr["bonificacion"] == DBNull.Value ? 0 : Convert.ToSingle(dr["bonificacion"]),
                        IndexAnulado = dr["idlineaventaanulado"] == DBNull.Value ? -1 : Convert.ToInt32(dr["idlineaventaanulado"])
                    };

                    oLinea.KgsTotalCalculado = oLinea.CantKg;
                    oLinea.PrecioKgOriginal = oLinea.PrecioKg;
                    oLinea.PesoBalanza = dr["pesobalanza"] != DBNull.Value && Convert.ToBoolean(dr["pesobalanza"]);
                    // idanulado GUARDA el Estado tal cual (agregarLineaVenta hace
                    // AddWithValue("idAnulado", oLineaE.Estado) -- 0=activa, 1=anulada, ver
                    // Entidades.LineaVenta.estados). Antes se leia "no es NULL -> 1", lo que
                    // marcaba TODA linea activa (idanulado=0, un valor real, no NULL) como
                    // anulada -- bug real encontrado probando "modificar venta" desde el POS:
                    // el carrito se abria vacio porque la vista filtra por esAnulado(Estado).
                    // Ver docs/DECISIONS.md.
                    oLinea.Estado = dr["idanulado"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idanulado"]);

                    return oLinea;
                },
                p => p.AddWithValue("idVenta", idVenta));
        }

        public DataTable obtenerUltimosPreciosPorCliente(int idPersona, int topVentas = 10)
        {
            const string sql = @"
                WITH ultimasventas AS (
                    SELECT idventa, fechaventa
                    FROM ventas
                    WHERE idpersona = @idPersona AND estado <> 'ANULADO'
                    ORDER BY fechaventa DESC
                    LIMIT @topVentas
                ),
                lineascliente AS (
                    SELECT
                        lv.idcorte, lv.preciokg, lv.cantkg, uv.fechaventa,
                        ROW_NUMBER() OVER (PARTITION BY lv.idcorte ORDER BY uv.fechaventa DESC, lv.idlineaventa DESC) AS rn
                    FROM lineaventa lv
                    INNER JOIN ultimasventas uv ON uv.idventa = lv.idventa
                    WHERE lv.idlineaventaanulado = 0
                )
                SELECT c.codigo, c.corte AS producto, lc.preciokg, lc.cantkg, lc.fechaventa
                FROM lineascliente lc
                INNER JOIN corte c ON c.idcorte = lc.idcorte
                WHERE lc.rn = 1
                ORDER BY lc.fechaventa DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idPersona", idPersona);
                p.AddWithValue("topVentas", topVentas <= 0 ? 10 : topVentas);
            });
        }

        public void agregarStockVenta(Venta oVentaE)
        {
            // No-op deliberado: ver nota de cabecera de archivo.
        }

        public void agregarTemporalLineaVenta(TemporalLineaVenta oTemporalLV)
        {
            const string sql = @"
                INSERT INTO temporallineaventa (idvendedor, fechainiciopesada, idcorte, cantkg, preciokg, totalcorte, ventaencurso, idsucursal, creado, idempresa)
                VALUES (@idVendedor, @fechaInicioPesada, @idCorte, @cantKg, @precioKg, @totalCorte, @ventaEnCurso, @idSucursal, @creado, @idEmpresa);";

            DbPg.NonQuery(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idVendedor", oTemporalLV.Vendedor.Id);
                p.AddWithValue("fechaInicioPesada", oTemporalLV.FechaInicioPesada);
                p.AddWithValue("idCorte", oTemporalLV.Corte.idCorte);
                p.AddWithValue("cantKg", (decimal)oTemporalLV.CantKg);
                p.AddWithValue("precioKg", (decimal)oTemporalLV.Corte.PrecioKg);
                p.AddWithValue("totalCorte", (decimal)oTemporalLV.TotalCorte);
                p.AddWithValue("ventaEnCurso", oTemporalLV.VentaEnCurso);
                p.AddWithValue("idSucursal", oTemporalLV.Sucursal.idSucursal);
                p.AddWithValue("creado", DateTime.Now);
                p.AddWithValue("idEmpresa", _idEmpresa);
            });
        }

        public DataTable obtenerTemporalLineaVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool conVentas)
        {
            const string sql = @"
                (SELECT u.nombre, t.fechainiciopesada, c.codigo, c.corte, t.cantkg, t.preciokg, t.totalcorte, v.idventa, t.ventaencurso
                 FROM temporallineaventa t
                 INNER JOIN corte c ON t.idcorte = c.idcorte
                 INNER JOIN usuarios u ON t.idvendedor = u.id
                 CROSS JOIN ventas v
                 WHERE @conVentas = true AND t.ventaencurso = true AND t.fechainiciopesada BETWEEN @fechaDesde AND @fechaHasta
                   AND (t.fechainiciopesada BETWEEN v.fechaventa AND v.creado)
                   AND ((@idSucursal < 0 AND t.idsucursal >= 0) OR (@idSucursal >= 0 AND t.idsucursal = @idSucursal))
                   AND ((@idVendedor < 0 AND t.idvendedor >= 0) OR (@idVendedor >= 0 AND t.idvendedor = @idVendedor))
                   AND (CAST(c.codigo AS text) ILIKE @textoLike OR c.corte ILIKE @textoLike))
                UNION
                (SELECT u.nombre, t.fechainiciopesada, c.codigo, c.corte, t.cantkg, t.preciokg, t.totalcorte, NULL AS idventa, t.ventaencurso
                 FROM temporallineaventa t
                 INNER JOIN corte c ON t.idcorte = c.idcorte
                 INNER JOIN usuarios u ON t.idvendedor = u.id
                 WHERE t.ventaencurso = false AND t.fechainiciopesada BETWEEN @fechaDesde AND @fechaHasta
                   AND ((@idSucursal < 0 AND t.idsucursal >= 0) OR (@idSucursal >= 0 AND t.idsucursal = @idSucursal))
                   AND ((@idVendedor < 0 AND t.idvendedor >= 0) OR (@idVendedor >= 0 AND t.idvendedor = @idVendedor))
                   AND (CAST(c.codigo AS text) ILIKE @textoLike OR c.corte ILIKE @textoLike))
                ORDER BY fechainiciopesada DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                p.AddWithValue("textoLike", "%" + (texto ?? "") + "%");
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("idVendedor", idVendedor);
                p.AddWithValue("conVentas", conVentas);
            });
        }

        public DataTable getAllLineasVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            const string sql = @"
                SELECT u.nombre, v.idventa, v.fechaventa, p.razonsocial, c.codigo, c.corte, lv.cantkg, lv.preciokg,
                    lv.cantkg * lv.preciokg AS totalcorte, lv.bonificacion, lv.pesobalanza, lv.idanulado, s.sucursal
                FROM ventas v
                INNER JOIN lineaventa lv ON v.idventa = lv.idventa
                INNER JOIN corte c ON lv.idcorte = c.idcorte
                INNER JOIN usuarios u ON v.idvendedor = u.id
                INNER JOIN sucursal s ON v.idsucursal = s.idsucursal
                INNER JOIN personas p ON v.idpersona = p.idpersona
                WHERE v.fechaventa BETWEEN @fechaDesde AND @fechaHasta
                  AND ((@idSucursal < 0 AND v.idsucursal >= 0) OR (@idSucursal >= 0 AND v.idsucursal = @idSucursal))
                  AND ((@idVendedor < 0 AND v.idvendedor >= 0) OR (@idVendedor >= 0 AND v.idvendedor = @idVendedor))
                  AND (c.codigo::text ILIKE @textoLike OR c.corte ILIKE @textoLike OR p.razonsocial ILIKE @textoLike)
                ORDER BY v.fechaventa DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                p.AddWithValue("textoLike", "%" + (texto ?? "") + "%");
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("idVendedor", idVendedor);
            });
        }

        public DataTable ultimasVentasCliente(int idSucursal, int idPersona)
        {
            const string sql = @"
                SELECT u.nombre AS vendedor, v.idventa, v.fechaventa, p.razonsocial, c.codigo, c.corte, lv.cantkg, lv.preciokg,
                    lv.cantkg * lv.preciokg AS totalcorte, lv.bonificacion, lv.pesobalanza, lv.idanulado, s.sucursal
                FROM ventas v
                INNER JOIN lineaventa lv ON v.idventa = lv.idventa
                INNER JOIN corte c ON lv.idcorte = c.idcorte
                INNER JOIN usuarios u ON v.idvendedor = u.id
                INNER JOIN sucursal s ON v.idsucursal = s.idsucursal
                INNER JOIN personas p ON v.idpersona = p.idpersona
                WHERE v.idpersona = @idPersona AND v.idsucursal = @idSucursal
                  AND v.idventa IN (
                      SELECT vta.idventa FROM ventas vta
                      INNER JOIN sucursal suc ON vta.idsucursal = suc.idsucursal
                      INNER JOIN personas pers ON vta.idpersona = pers.idpersona
                      WHERE vta.idpersona = @idPersona AND vta.idsucursal = @idSucursal
                      ORDER BY vta.fechaventa DESC
                      LIMIT 5)
                ORDER BY v.fechaventa DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idPersona", idPersona);
                p.AddWithValue("idSucursal", idSucursal);
            });
        }

        public void actualizarLetraId_TipoCbte(int idVenta, char letraId_tipoCbte)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE ventas SET tipocomprobante = @tipoComprobante WHERE idventa = @idVenta;",
                p =>
                {
                    p.AddWithValue("tipoComprobante", letraId_tipoCbte.ToString());
                    p.AddWithValue("idVenta", idVenta);
                });
        }

        public void actualizarCliente(int idVenta, int idPersona)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE ventas SET idpersona = @idPersona WHERE idventa = @idVenta;",
                p =>
                {
                    p.AddWithValue("idPersona", idPersona);
                    p.AddWithValue("idVenta", idVenta);
                });
        }

        #endregion

        #region Sectores (Etapa 12a)

        // Sin filtro explicito de idempresa en el WHERE -- igual que el original SQL Server
        // (Datos/Venta.cs:1012), se apoya solo en la RLS de tabla.
        public DataTable obtenerSectores()
        {
            return DbPg.DataTable(_connectionString, _idEmpresa, "SELECT sector FROM sectores;");
        }

        public bool existeSector(string sector, string sectorActual = "")
        {
            object scalar = DbPg.Scalar(_connectionString, _idEmpresa, @"
                SELECT COUNT(1)
                FROM sectores
                WHERE UPPER(TRIM(sector)) = UPPER(TRIM(@sector))
                  AND (@sectorActual = '' OR UPPER(TRIM(sector)) <> UPPER(TRIM(@sectorActual)));",
                p =>
                {
                    p.AddWithValue("sector", sector ?? "");
                    p.AddWithValue("sectorActual", sectorActual ?? "");
                });

            return scalar != null && scalar != DBNull.Value && Convert.ToInt32(scalar) > 0;
        }

        // idempresa bindeado explicito -- el original SQL Server lo resuelve con un DEFAULT
        // atado a SESSION_CONTEXT('IdEmpresa') (confirmado con sys.default_constraints), sin
        // pasarlo en el INSERT; en Postgres no hay equivalente de DEFAULT por sesion, se
        // bindea directo (mismo criterio que el resto de VentaPg.cs/CortePg.cs).
        public void agregarSector(string sector)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "INSERT INTO sectores (sector, idempresa) VALUES (@sector, @idEmpresa);",
                p =>
                {
                    p.AddWithValue("sector", sector ?? "");
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });
        }

        public void modificarSector(string sectorActual, string sectorNuevo)
        {
            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    using (var cmd = new NpgsqlCommand("UPDATE sectores SET sector = @sectorNuevo WHERE sector = @sectorActual;", con, tx))
                    {
                        cmd.Parameters.AddWithValue("sectorNuevo", sectorNuevo ?? "");
                        cmd.Parameters.AddWithValue("sectorActual", sectorActual ?? "");
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new NpgsqlCommand("UPDATE expendios SET sector = @sectorNuevo WHERE sector = @sectorActual;", con, tx))
                    {
                        cmd.Parameters.AddWithValue("sectorNuevo", sectorNuevo ?? "");
                        cmd.Parameters.AddWithValue("sectorActual", sectorActual ?? "");
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new NpgsqlCommand("UPDATE licencias SET sector = @sectorNuevo WHERE sector = @sectorActual;", con, tx))
                    {
                        cmd.Parameters.AddWithValue("sectorNuevo", sectorNuevo ?? "");
                        cmd.Parameters.AddWithValue("sectorActual", sectorActual ?? "");
                        cmd.ExecuteNonQuery();
                    }

                    tx?.Commit();
                }
                catch
                {
                    try { tx?.Rollback(); } catch { }
                    throw;
                }
            }
        }

        public bool sectorEstaEnUso(string sector)
        {
            object scalar = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT COUNT(1) FROM expendios WHERE sector = @sector;",
                p => p.AddWithValue("sector", sector ?? ""));

            return scalar != null && scalar != DBNull.Value && Convert.ToInt32(scalar) > 0;
        }

        public void eliminarSector(string sector)
        {
            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    using (var cmd = new NpgsqlCommand("DELETE FROM sectores WHERE sector = @sector;", con, tx))
                    {
                        cmd.Parameters.AddWithValue("sector", sector ?? "");
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new NpgsqlCommand("UPDATE licencias SET sector = '' WHERE sector = @sector;", con, tx))
                    {
                        cmd.Parameters.AddWithValue("sector", sector ?? "");
                        cmd.ExecuteNonQuery();
                    }

                    tx?.Commit();
                }
                catch
                {
                    try { tx?.Rollback(); } catch { }
                    throw;
                }
            }
        }

        public string getUltimoSectorSelect(string serialCPU)
        {
            object scalar = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT sector FROM licencias WHERE nrolicencia = @nroLicencia LIMIT 1;",
                p => p.AddWithValue("nroLicencia", serialCPU ?? ""));

            return (scalar == null || scalar == DBNull.Value) ? "" : scalar.ToString().Trim();
        }

        #endregion

        #region Expendios (Etapa 12b)

        // Traduccion de agregarExpendio (SP real, via sp_helptext -- la firma C# no deja ver
        // que el SP tambien actualiza Licencias.sector antes de insertar en Expendios). El
        // parametro @idExpendio del SP original nunca se usa (dead param); el id real sale de
        // un SELECT TOP 1 ORDER BY idExpendio DESC, no de SCOPE_IDENTITY() -- se replica igual,
        // no se "arregla" la carrera teorica bajo concurrencia (no es peor que el original).
        public int agregarExpendio(Entidades.Venta oVentaE)
        {
            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    using (var cmd = new NpgsqlCommand("UPDATE licencias SET sector = @sector WHERE nrolicencia = @serialCPU;", con, tx))
                    {
                        cmd.Parameters.AddWithValue("sector", oVentaE.Sector ?? "");
                        cmd.Parameters.AddWithValue("serialCPU", oVentaE.SerialCPU ?? "");
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO expendios (idvendedor, fechaexpendio, idsucursal, identificacionexpendio, sector, cantitems, importe, creado, observaciones, idempresa)
                        VALUES (@idVendedor, @fechaExpendio, @idSucursal, @identificacionExpendio, @sector, @cantItems, @importe, now(), @observaciones, @idEmpresa);", con, tx))
                    {
                        cmd.Parameters.AddWithValue("idVendedor", oVentaE.Vendedor.Id);
                        cmd.Parameters.AddWithValue("fechaExpendio", oVentaE.FechaVenta);
                        cmd.Parameters.AddWithValue("idSucursal", oVentaE.Sucursal.idSucursal);
                        cmd.Parameters.AddWithValue("identificacionExpendio", oVentaE.IdentificacionExpendio ?? "");
                        cmd.Parameters.AddWithValue("sector", oVentaE.Sector ?? "");
                        cmd.Parameters.AddWithValue("cantItems", int.TryParse(oVentaE.CantItems, out int cantItems) ? cantItems : 0);
                        cmd.Parameters.AddWithValue("importe", oVentaE.TotalImporte);
                        cmd.Parameters.AddWithValue("observaciones", oVentaE.Observaciones ?? "");
                        cmd.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                        cmd.ExecuteNonQuery();
                    }

                    object idExpendioObj;
                    using (var cmd = new NpgsqlCommand(
                        "SELECT idexpendio FROM expendios WHERE idsucursal = @idSucursal ORDER BY idexpendio DESC LIMIT 1;", con, tx))
                    {
                        cmd.Parameters.AddWithValue("idSucursal", oVentaE.Sucursal.idSucursal);
                        idExpendioObj = cmd.ExecuteScalar();
                    }

                    tx?.Commit();

                    return (idExpendioObj == null || idExpendioObj == DBNull.Value) ? 0 : Convert.ToInt32(idExpendioObj);
                }
                catch
                {
                    try { tx?.Rollback(); } catch { }
                    throw;
                }
            }
        }

        public Entidades.LineaVenta agregarLineaExprendio(Entidades.LineaVenta oLineaE)
        {
            object scalar = DbPg.Scalar(_connectionString, _idEmpresa, @"
                INSERT INTO lineaexpendio (idexpendio, idcorte, cantkg, preciokg, pesobalanza, idempresa)
                VALUES (@idExpendio, @idCorte, @cantKg, @precioKg, @pesoBalanza, @idEmpresa)
                RETURNING idlineaexpendio;",
                p =>
                {
                    p.AddWithValue("idExpendio", oLineaE.Venta.IdVenta);
                    p.AddWithValue("idCorte", oLineaE.Corte.idCorte);
                    p.AddWithValue("cantKg", Math.Round(oLineaE.CantKg, 3));
                    p.AddWithValue("precioKg", Math.Round(oLineaE.PrecioKg, 2));
                    p.AddWithValue("pesoBalanza", oLineaE.PesoBalanza);
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });

            oLineaE.IdLineaVenta = (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
            return oLineaE;
        }

        public void asignarVentaEnExpendio(int idVenta, int idExpendio, Contratos.IUnitOfWork unitOfWork = null)
        {
            const string sql = "UPDATE expendios SET idventa = @idVenta WHERE idexpendio = @idExpendio;";
            Action<NpgsqlParameterCollection> setParams = p =>
            {
                p.AddWithValue("idVenta", idVenta);
                p.AddWithValue("idExpendio", idExpendio);
            };

            var uow = unitOfWork as UnitOfWorkPg;
            if (uow != null)
            {
                using (var cmd = new NpgsqlCommand(sql, uow.Connection, uow.Transaction))
                {
                    setParams(cmd.Parameters);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                DbPg.NonQuery(_connectionString, _idEmpresa, sql, setParams);
            }
        }

        public DataTable obtenerUltimosExpendios(int ultimosMinutos, int idSucursal)
        {
            DateTime fechaDesde = DateTime.Now.AddMinutes(-ultimosMinutos);

            return DbPg.DataTable(_connectionString, _idEmpresa, @"
                SELECT fechaexpendio,
                       e.idexpendio,
                       identificacionexpendio,
                       sector,
                       c.codigo,
                       c.corte,
                       le.cantkg,
                       le.preciokg,
                       (le.cantkg * le.preciokg) AS total,
                       idventa,
                       u.nombre AS vendedor,
                       e.observaciones
                FROM expendios e
                INNER JOIN lineaexpendio le ON e.idexpendio = le.idexpendio
                INNER JOIN corte c ON le.idcorte = c.idcorte
                INNER JOIN usuarios u ON e.idvendedor = u.id
                WHERE fechaexpendio > @fechaDesde AND e.idsucursal = @idSucursal
                ORDER BY fechaexpendio;",
                p =>
                {
                    p.AddWithValue("fechaDesde", fechaDesde);
                    p.AddWithValue("idSucursal", idSucursal);
                });
        }

        public DataTable obtenerExpendiosPorUsuario(int idSucursal, int idVendedor, int top = 100, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            return DbPg.DataTable(_connectionString, _idEmpresa, @"
                SELECT e.fechaexpendio,
                       e.idexpendio,
                       e.identificacionexpendio,
                       e.sector,
                       e.cantitems,
                       e.importe,
                       e.idventa,
                       u.nombre AS vendedor,
                       COALESCE(SUM(le.cantkg), 0) AS totalkg
                FROM expendios e
                INNER JOIN usuarios u ON e.idvendedor = u.id
                LEFT JOIN lineaexpendio le ON e.idexpendio = le.idexpendio
                WHERE e.idsucursal = @idSucursal
                  AND e.idvendedor = @idVendedor
                  AND (@fechaDesde::date IS NULL OR e.fechaexpendio::date >= @fechaDesde::date)
                  AND (@fechaHasta::date IS NULL OR e.fechaexpendio::date <= @fechaHasta::date)
                GROUP BY e.fechaexpendio, e.idexpendio, e.identificacionexpendio, e.sector,
                         e.cantitems, e.importe, e.idventa, u.nombre
                ORDER BY e.fechaexpendio DESC, e.idexpendio DESC
                LIMIT @top;",
                p =>
                {
                    p.AddWithValue("idSucursal", idSucursal);
                    p.AddWithValue("idVendedor", idVendedor);
                    p.AddWithValue("fechaDesde", fechaDesde.HasValue ? (object)fechaDesde.Value.Date : DBNull.Value);
                    p.AddWithValue("fechaHasta", fechaHasta.HasValue ? (object)fechaHasta.Value.Date : DBNull.Value);
                    p.AddWithValue("top", top <= 0 ? 100 : top);
                });
        }

        public DataTable obtenerExpendiosEmpresa(int top = 300, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            return DbPg.DataTable(_connectionString, _idEmpresa, @"
                SELECT e.fechaexpendio,
                       e.idexpendio,
                       e.identificacionexpendio,
                       e.sector,
                       e.cantitems,
                       e.importe,
                       e.idventa,
                       e.idsucursal,
                       e.idvendedor,
                       u.nombre AS vendedor,
                       s.sucursal AS sucursal,
                       COALESCE(SUM(le.cantkg), 0) AS totalkg
                FROM expendios e
                INNER JOIN usuarios u ON e.idvendedor = u.id
                LEFT JOIN sucursal s ON e.idsucursal = s.idsucursal
                LEFT JOIN lineaexpendio le ON e.idexpendio = le.idexpendio
                WHERE e.idempresa = @idEmpresa
                  AND (@fechaDesde::timestamp IS NULL OR e.fechaexpendio >= @fechaDesde)
                  AND (@fechaHasta::timestamp IS NULL OR e.fechaexpendio <= @fechaHasta)
                GROUP BY e.fechaexpendio, e.idexpendio, e.identificacionexpendio, e.sector,
                         e.cantitems, e.importe, e.idventa, e.idsucursal, e.idvendedor, u.nombre, s.sucursal
                ORDER BY e.fechaexpendio DESC, e.idexpendio DESC
                LIMIT @top;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("fechaDesde", fechaDesde.HasValue ? (object)fechaDesde.Value : DBNull.Value);
                    p.AddWithValue("fechaHasta", fechaHasta.HasValue ? (object)fechaHasta.Value : DBNull.Value);
                    p.AddWithValue("top", top <= 0 ? 300 : top);
                });
        }

        // Sin wrapper propio en Negocio.Venta -- solo se usa dentro de getExpedioById (mismo
        // patron que Datos.Venta.obtenerLineasExpendio). Mismo fix de N+1 que
        // obtenerLineasVenta (ver ese comentario, docs/DECISIONS.md 2026-08-21): antes hacia
        // un findCorteById() por linea, ahora se joinea corte en la misma query.
        //
        // A diferencia de obtenerLineasVenta, el JOIN a corte aca es LEFT, no INNER: verificado
        // contra el original de SQL Server (Datos/Venta.cs.obtenerLineasExpendio) que hace
        // "SELECT * FROM LineaExpendio" sin ningun JOIN y deja Corte=null si findCorteById no
        // encuentra nada -- ninguna fila se descarta por eso. Un INNER JOIN aca hubiera sido un
        // cambio de comportamiento real (filtrar lineas de expendio con corte no visible), no
        // solo una optimizacion.
        private List<Entidades.LineaVenta> GetLineasExpendio(int idExpendio)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                @"SELECT le.idlineaexpendio, le.idcorte, le.cantkg, le.preciokg, le.pesobalanza,
                    " + ColumnasCorteJoin + @"
                  FROM lineaexpendio le
                  LEFT JOIN corte c ON le.idcorte = c.idcorte
                  LEFT JOIN personas mk ON c.idmarca = mk.idpersona
                  WHERE le.idexpendio = @idExpendio;",
                dr => new Entidades.LineaVenta
                {
                    IdLineaVenta = Convert.ToInt32(dr["idlineaexpendio"]),
                    Corte = dr["idcorte"] != DBNull.Value && ColumnaExiste(dr, "co_codigo") && dr["co_codigo"] != DBNull.Value
                        ? MapCorteDesdeJoin(dr, Convert.ToInt32(dr["idcorte"]))
                        : null,
                    CantKg = dr["cantkg"] == DBNull.Value ? 0 : Convert.ToSingle(dr["cantkg"]),
                    PrecioKg = dr["preciokg"] == DBNull.Value ? 0 : Convert.ToSingle(dr["preciokg"]),
                    PesoBalanza = GetBool(dr, "pesobalanza")
                },
                p => p.AddWithValue("idExpendio", idExpendio));
        }

        public Entidades.Venta getExpedioById(int idExpendio)
        {
            var list = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM expendios WHERE idexpendio = @idExpendio;",
                dr =>
                {
                    var oExpendioE = new Entidades.Venta
                    {
                        IdExpendio = Convert.ToInt32(dr["idexpendio"]),
                        IdVenta = dr["idventa"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idventa"]),
                        FechaVenta = Convert.ToDateTime(dr["fechaexpendio"]),
                        IdentificacionExpendio = GetString(dr, "identificacionexpendio"),
                        Sector = GetString(dr, "sector"),
                        CantItems = GetString(dr, "cantitems"),
                        TotalImporte = GetFloat(dr, "importe"),
                        Observaciones = GetString(dr, "observaciones"),
                        Vendedor = GetUsuarioLiviano(Convert.ToInt32(dr["idvendedor"])),
                        Sucursal = _sucursalRepo.findById(Convert.ToInt32(dr["idsucursal"]))
                    };

                    return oExpendioE;
                },
                p => p.AddWithValue("idExpendio", idExpendio));

            if (list.Count == 0) return null;

            var oExpendioE = list[0];
            oExpendioE.LineasVenta = GetLineasExpendio(oExpendioE.IdExpendio);
            return oExpendioE;
        }

        #endregion

        #region FacturaElectronica (Etapa 12c)

        public int esVentaSinFacturar(int idVenta, bool esNotaCredito)
        {
            string validarComprobantes = esNotaCredito
                ? $"(codtipocbteafip = {FacturaElectronica.codNotaCreditoA_Afip} OR codtipocbteafip = {FacturaElectronica.codNotaCreditoB_Afip} OR codtipocbteafip = {FacturaElectronica.codNotaCreditoC_Afip})"
                : $"(codtipocbteafip = {FacturaElectronica.codFacturaA_Afip} OR codtipocbteafip = {FacturaElectronica.codFacturaB_Afip} OR codtipocbteafip = {FacturaElectronica.codFacturaC_Afip})";

            object scalar = DbPg.Scalar(_connectionString, _idEmpresa, $@"
                SELECT id
                FROM facturaelectronica
                WHERE idventa = @idVenta
                  AND cae IS NOT NULL
                  AND {validarComprobantes}
                ORDER BY id DESC
                LIMIT 1;",
                p => p.AddWithValue("idVenta", idVenta));

            return (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
        }

        public int existeFacturaElect(int idVenta)
        {
            object scalar = DbPg.Scalar(_connectionString, _idEmpresa, $@"
                SELECT id
                FROM facturaelectronica
                WHERE cae <> ''
                  AND idventa = @idVenta
                  AND (codtipocbteafip = {FacturaElectronica.codFacturaA_Afip}
                    OR codtipocbteafip = {FacturaElectronica.codFacturaB_Afip}
                    OR codtipocbteafip = {FacturaElectronica.codFacturaC_Afip})
                ORDER BY id DESC
                LIMIT 1;",
                p => p.AddWithValue("idVenta", idVenta));

            return (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
        }

        public int existeNotaCreditoElect(int idVenta)
        {
            object scalar = DbPg.Scalar(_connectionString, _idEmpresa, $@"
                SELECT id
                FROM facturaelectronica
                WHERE cae <> ''
                  AND idventa = @idVenta
                  AND (codtipocbteafip = {FacturaElectronica.codNotaCreditoA_Afip}
                    OR codtipocbteafip = {FacturaElectronica.codNotaCreditoB_Afip}
                    OR codtipocbteafip = {FacturaElectronica.codNotaCreditoC_Afip})
                ORDER BY id DESC
                LIMIT 1;",
                p => p.AddWithValue("idVenta", idVenta));

            return (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
        }

        // Transaccion explicita cabecera + alicuotas (mejora deliberada respecto al original
        // SQL Server, que no envuelve esto -- confirmado con el usuario, ver docs/DECISIONS.md).
        // fechaemisionafip NO se incluye en el UPDATE de la rama de edicion: una vez que AFIP
        // emite el CAE esa fecha queda legalmente inmutable (confirmado con el usuario -- el SP
        // original hace "fechaEmisionAfip = fechaEmisionAfip", auto-asignacion intencional, no
        // un bug de falta de @).
        public void addOrEditFactuElec(FacturaElectronica oFacturaElectronicaE)
        {
            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    if (oFacturaElectronicaE.Id == 0)
                    {
                        using (var cmd = new NpgsqlCommand(@"
                            INSERT INTO facturaelectronica
                                (ptovtaafip, fechaemisionafip, desctipocbteafip, codtipocbteafip, nrocbteafip, tipodocafip, nrodocafip,
                                 razonsocialafip, condicionivaafip, domicilioafip, condicionventa, formapago, cae, fecvtocae,
                                 importenetogravado, iva, importetotal, porcentajefacturacion, descitemunitario, observaciones,
                                 idventa, creado, error, mensajeerror, fechaerror, canterrores, idempresa)
                            VALUES
                                (@ptoVtaAfip, @fechaEmisionAfip, @descTipoCbteAfip, @codTipoCbteAfip, @nroCbteAfip, @tipoDocAfip, @nroDocAfip,
                                 @razonSocialAFIP, @condicionIvaAFIP, @domicilioAFIP, @condicionVenta, @formaPago, @CAE, @fecVtoCAE,
                                 @importeNetoGravado, @iva, @importeTotal, @porcentajeFacturacion, @descItemUnitario, @observaciones,
                                 @idVenta, now(), @error, @mensajeError, @fechaError, 0, @idEmpresa)
                            RETURNING id;", con, tx))
                        {
                            cmd.Parameters.AddWithValue("ptoVtaAfip", oFacturaElectronicaE.PtoVtaAfip ?? "");
                            // FechaEmisionAfip es DateTime? -- "x < fecha" con x nulo evalua false
                            // en C# (los operadores relacionales de Nullable<T> nunca son true si
                            // un operando es null), asi que el ternario de abajo caia SIEMPRE en
                            // la rama "no nulo" cuando FechaEmisionAfip era null, boxeando un
                            // DateTime? sin valor a un object null desnudo -- Npgsql lo rechaza
                            // (mismo bug ya encontrado y cerrado en CierreCajaPg.addOrEditCierreCaja,
                            // ver docs/DECISIONS.md). Hay que chequear HasValue antes de comparar.
                            cmd.Parameters.AddWithValue("fechaEmisionAfip",
                                (!oFacturaElectronicaE.FechaEmisionAfip.HasValue || oFacturaElectronicaE.FechaEmisionAfip.Value < DateTime.Today.AddYears(-100))
                                    ? (object)DBNull.Value
                                    : (object)oFacturaElectronicaE.FechaEmisionAfip.Value);
                            cmd.Parameters.AddWithValue("descTipoCbteAfip", oFacturaElectronicaE.DescTipoCbteAfip ?? "");
                            cmd.Parameters.AddWithValue("codTipoCbteAfip", oFacturaElectronicaE.CodTipoCbteAfip);
                            cmd.Parameters.AddWithValue("nroCbteAfip", oFacturaElectronicaE.NroCbteAfip ?? "");
                            cmd.Parameters.AddWithValue("tipoDocAfip", oFacturaElectronicaE.TipoDocAfip ?? "");
                            cmd.Parameters.AddWithValue("nroDocAfip", oFacturaElectronicaE.NroDocAfip ?? "");
                            cmd.Parameters.AddWithValue("razonSocialAFIP", oFacturaElectronicaE.RazonSocialAFIP ?? "");
                            cmd.Parameters.AddWithValue("condicionIvaAFIP", oFacturaElectronicaE.CondicionIvaAFIP ?? "");
                            cmd.Parameters.AddWithValue("domicilioAFIP", oFacturaElectronicaE.DomicilioAFIP ?? "");
                            cmd.Parameters.AddWithValue("condicionVenta", oFacturaElectronicaE.CondicionVenta ?? "");
                            cmd.Parameters.AddWithValue("formaPago", oFacturaElectronicaE.FormaPago ?? "");
                            cmd.Parameters.AddWithValue("CAE", oFacturaElectronicaE.CAE1 ?? "");
                            cmd.Parameters.AddWithValue("fecVtoCAE", oFacturaElectronicaE.FecVtoCAE ?? "");
                            cmd.Parameters.AddWithValue("importeNetoGravado", oFacturaElectronicaE.ImporteNetoGravado);
                            cmd.Parameters.AddWithValue("iva", oFacturaElectronicaE.Iva);
                            cmd.Parameters.AddWithValue("importeTotal", oFacturaElectronicaE.ImporteTotal);
                            cmd.Parameters.AddWithValue("porcentajeFacturacion", oFacturaElectronicaE.PorcentajeFacturacion);
                            cmd.Parameters.AddWithValue("descItemUnitario", oFacturaElectronicaE.DescItemUnitario ?? "");
                            cmd.Parameters.AddWithValue("observaciones", oFacturaElectronicaE.Observaciones ?? "");
                            cmd.Parameters.AddWithValue("idVenta", oFacturaElectronicaE.IdVenta);
                            cmd.Parameters.AddWithValue("error", oFacturaElectronicaE.Error);
                            cmd.Parameters.AddWithValue("mensajeError", oFacturaElectronicaE.MensajeError ?? "");
                            cmd.Parameters.AddWithValue("fechaError", (oFacturaElectronicaE.FechaError == null || oFacturaElectronicaE.FechaError < DateTime.Today.AddYears(-100)) ? (object)DBNull.Value : (object)oFacturaElectronicaE.FechaError);
                            cmd.Parameters.AddWithValue("idEmpresa", _idEmpresa);

                            oFacturaElectronicaE.Id = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                    else
                    {
                        int cantErrores = 0;
                        if (string.IsNullOrEmpty(oFacturaElectronicaE.CAE1))
                        {
                            using (var cmdCant = new NpgsqlCommand("SELECT canterrores + 1 FROM facturaelectronica WHERE id = @id;", con, tx))
                            {
                                cmdCant.Parameters.AddWithValue("id", oFacturaElectronicaE.Id);
                                object cantObj = cmdCant.ExecuteScalar();
                                cantErrores = (cantObj == null || cantObj == DBNull.Value) ? 0 : Convert.ToInt32(cantObj);
                            }
                        }

                        using (var cmd = new NpgsqlCommand(@"
                            UPDATE facturaelectronica SET
                                ptovtaafip = @ptoVtaAfip,
                                desctipocbteafip = @descTipoCbteAfip,
                                codtipocbteafip = @codTipoCbteAfip,
                                nrocbteafip = @nroCbteAfip,
                                tipodocafip = @tipoDocAfip,
                                nrodocafip = @nroDocAfip,
                                razonsocialafip = @razonSocialAFIP,
                                condicionivaafip = @condicionIvaAFIP,
                                domicilioafip = @domicilioAFIP,
                                condicionventa = @condicionVenta,
                                formapago = @formaPago,
                                cae = @CAE,
                                fecvtocae = @fecVtoCAE,
                                importenetogravado = @importeNetoGravado,
                                iva = @iva,
                                importetotal = @importeTotal,
                                porcentajefacturacion = @porcentajeFacturacion,
                                descitemunitario = @descItemUnitario,
                                observaciones = @observaciones,
                                idventa = @idVenta,
                                error = @error,
                                mensajeerror = @mensajeError,
                                fechaerror = @fechaError,
                                canterrores = @cantErrores
                            WHERE id = @id;", con, tx))
                        {
                            cmd.Parameters.AddWithValue("ptoVtaAfip", oFacturaElectronicaE.PtoVtaAfip ?? "");
                            cmd.Parameters.AddWithValue("descTipoCbteAfip", oFacturaElectronicaE.DescTipoCbteAfip ?? "");
                            cmd.Parameters.AddWithValue("codTipoCbteAfip", oFacturaElectronicaE.CodTipoCbteAfip);
                            cmd.Parameters.AddWithValue("nroCbteAfip", oFacturaElectronicaE.NroCbteAfip ?? "");
                            cmd.Parameters.AddWithValue("tipoDocAfip", oFacturaElectronicaE.TipoDocAfip ?? "");
                            cmd.Parameters.AddWithValue("nroDocAfip", oFacturaElectronicaE.NroDocAfip ?? "");
                            cmd.Parameters.AddWithValue("razonSocialAFIP", oFacturaElectronicaE.RazonSocialAFIP ?? "");
                            cmd.Parameters.AddWithValue("condicionIvaAFIP", oFacturaElectronicaE.CondicionIvaAFIP ?? "");
                            cmd.Parameters.AddWithValue("domicilioAFIP", oFacturaElectronicaE.DomicilioAFIP ?? "");
                            cmd.Parameters.AddWithValue("condicionVenta", oFacturaElectronicaE.CondicionVenta ?? "");
                            cmd.Parameters.AddWithValue("formaPago", oFacturaElectronicaE.FormaPago ?? "");
                            cmd.Parameters.AddWithValue("CAE", oFacturaElectronicaE.CAE1 ?? "");
                            cmd.Parameters.AddWithValue("fecVtoCAE", oFacturaElectronicaE.FecVtoCAE ?? "");
                            cmd.Parameters.AddWithValue("importeNetoGravado", oFacturaElectronicaE.ImporteNetoGravado);
                            cmd.Parameters.AddWithValue("iva", oFacturaElectronicaE.Iva);
                            cmd.Parameters.AddWithValue("importeTotal", oFacturaElectronicaE.ImporteTotal);
                            cmd.Parameters.AddWithValue("porcentajeFacturacion", oFacturaElectronicaE.PorcentajeFacturacion);
                            cmd.Parameters.AddWithValue("descItemUnitario", oFacturaElectronicaE.DescItemUnitario ?? "");
                            cmd.Parameters.AddWithValue("observaciones", oFacturaElectronicaE.Observaciones ?? "");
                            cmd.Parameters.AddWithValue("idVenta", oFacturaElectronicaE.IdVenta);
                            cmd.Parameters.AddWithValue("error", oFacturaElectronicaE.Error);
                            cmd.Parameters.AddWithValue("mensajeError", oFacturaElectronicaE.MensajeError ?? "");
                            cmd.Parameters.AddWithValue("fechaError", (oFacturaElectronicaE.FechaError == null || oFacturaElectronicaE.FechaError < DateTime.Today.AddYears(-100)) ? (object)DBNull.Value : (object)oFacturaElectronicaE.FechaError);
                            cmd.Parameters.AddWithValue("cantErrores", cantErrores);
                            cmd.Parameters.AddWithValue("id", oFacturaElectronicaE.Id);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    if (oFacturaElectronicaE.ListaAlicuota != null && oFacturaElectronicaE.ListaAlicuota.Count > 0)
                    {
                        foreach (var a in oFacturaElectronicaE.ListaAlicuota)
                        {
                            using (var cmdA = new NpgsqlCommand(@"
                                INSERT INTO alicuotaivaporfactura (idfacturaelectronica, idiva, baseimponible, importe, idempresa)
                                VALUES (@idFacturaElectronica, @idIva, @baseImponible, @importe, @idEmpresa);", con, tx))
                            {
                                cmdA.Parameters.AddWithValue("idFacturaElectronica", oFacturaElectronicaE.Id);
                                cmdA.Parameters.AddWithValue("idIva", a.IdIva);
                                cmdA.Parameters.AddWithValue("baseImponible", a.BaseImponible);
                                cmdA.Parameters.AddWithValue("importe", a.Importe);
                                cmdA.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                                cmdA.ExecuteNonQuery();
                            }
                        }
                    }

                    tx?.Commit();
                }
                catch
                {
                    try { tx?.Rollback(); } catch { }
                    throw;
                }
            }
        }

        public FacturaElectronica getFactuElecById(int idFactuElec)
        {
            var list = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM facturaelectronica WHERE id = @id;",
                dr => new FacturaElectronica
                {
                    Id = Convert.ToInt32(dr["id"]),
                    PtoVtaAfip = GetString(dr, "ptovtaafip"),
                    FechaEmisionAfip = dr["fechaemisionafip"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["fechaemisionafip"]),
                    DescTipoCbteAfip = GetString(dr, "desctipocbteafip"),
                    CodTipoCbteAfip = dr["codtipocbteafip"] == DBNull.Value ? 0 : Convert.ToInt32(dr["codtipocbteafip"]),
                    NroCbteAfip = GetString(dr, "nrocbteafip"),
                    TipoDocAfip = GetString(dr, "tipodocafip"),
                    NroDocAfip = GetString(dr, "nrodocafip"),
                    RazonSocialAFIP = GetString(dr, "razonsocialafip"),
                    CondicionIvaAFIP = GetString(dr, "condicionivaafip"),
                    DomicilioAFIP = GetString(dr, "domicilioafip"),
                    CondicionVenta = GetString(dr, "condicionventa"),
                    FormaPago = GetString(dr, "formapago"),
                    CAE1 = GetString(dr, "cae"),
                    FecVtoCAE = GetString(dr, "fecvtocae"),
                    ImporteNetoGravado = GetFloat(dr, "importenetogravado"),
                    Iva = GetFloat(dr, "iva"),
                    ImporteTotal = GetFloat(dr, "importetotal"),
                    PorcentajeFacturacion = dr["porcentajefacturacion"] == DBNull.Value ? 100f : Convert.ToSingle(dr["porcentajefacturacion"]),
                    DescItemUnitario = GetString(dr, "descitemunitario"),
                    Observaciones = GetString(dr, "observaciones"),
                    IdVenta = dr["idventa"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idventa"]),
                    Error = GetBool(dr, "error"),
                    MensajeError = GetString(dr, "mensajeerror"),
                    FechaError = dr["fechaerror"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["fechaerror"])
                },
                p => p.AddWithValue("id", idFactuElec));

            if (list.Count == 0) return null;

            var fact = list[0];
            fact.ListaAlicuota = GetAlicuotaIvaFactura(fact.Id);
            fact.Venta = getVentaById(fact.IdVenta);
            return fact;
        }

        // Sin wrapper propio en Negocio.Venta -- solo se usa dentro de getFactuElecById (mismo
        // patron que GetLineasExpendio en la Etapa 12b).
        private List<AlicuotaIva> GetAlicuotaIvaFactura(int idFacturaElectronica)
        {
            return DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT a.idiva, ai.iva, a.baseimponible, a.importe
                FROM alicuotaivaporfactura a
                INNER JOIN alicuotasiva ai ON a.idiva = ai.idiva
                WHERE a.idfacturaelectronica = @idFacturaElectronica;",
                dr => new AlicuotaIva
                {
                    IdIva = Convert.ToInt32(dr["idiva"]),
                    Iva = dr["iva"] == DBNull.Value ? 0 : Convert.ToSingle(dr["iva"]),
                    BaseImponible = dr["baseimponible"] == DBNull.Value ? 0 : Convert.ToSingle(dr["baseimponible"]),
                    Importe = dr["importe"] == DBNull.Value ? 0 : Convert.ToSingle(dr["importe"])
                },
                p => p.AddWithValue("idFacturaElectronica", idFacturaElectronica));
        }

        // Mismo WHERE dinamico que el original (Datos/Venta.cs:ConstruirWhereFacturas),
        // compartido entre BuscarFacturasPagina y ObtenerFacturasResumen para que las dos
        // consultas filtren exactamente igual. Placeholders numerados para los IN(...), nunca
        // concatenados directo.
        private static string ConstruirWhereFacturas(List<string> formasPago, List<int> codigosComprobante)
        {
            string whereFormaPago = "1 = 1";
            if (formasPago != null && formasPago.Count > 0)
            {
                string placeholders = "";
                for (int i = 0; i < formasPago.Count; i++)
                {
                    if (i > 0) placeholders += ", ";
                    placeholders += "@fp" + i;
                }
                whereFormaPago = "COALESCE(NULLIF(f.formapago, ''), v.formapago) IN (" + placeholders + ")";
            }

            string whereComprobante = "1 = 1";
            if (codigosComprobante != null && codigosComprobante.Count > 0)
            {
                string placeholders = "";
                for (int i = 0; i < codigosComprobante.Count; i++)
                {
                    if (i > 0) placeholders += ", ";
                    placeholders += "@cc" + i;
                }
                whereComprobante = "f.codtipocbteafip IN (" + placeholders + ")";
            }

            return $@"
                COALESCE(f.cae, '') <> ''
                AND f.fechaemisionafip >= @fechaDesde
                AND f.fechaemisionafip < @fechaHastaMas1
                AND (@idSucursal = -1 OR v.idsucursal = @idSucursal)
                AND (@cliente = '' OR p.razonsocial ILIKE @clienteBuscar OR p.identificacion ILIKE @clienteBuscar OR f.razonsocialafip ILIKE @clienteBuscar OR f.nrodocafip ILIKE @clienteBuscar)
                AND (@vendedor = '' OR u.nombre ILIKE @vendedorBuscar)
                AND ({whereFormaPago})
                AND ({whereComprobante})";
        }

        private static void AgregarParametrosFacturas(
            NpgsqlParameterCollection p,
            DateTime fechaDesde, DateTime fechaHasta, int idSucursal,
            string cliente, string vendedor,
            List<string> formasPago, List<int> codigosComprobante)
        {
            string clienteLimpio = (cliente ?? "").Trim();
            string vendedorLimpio = (vendedor ?? "").Trim();

            p.AddWithValue("fechaDesde", fechaDesde);
            p.AddWithValue("fechaHastaMas1", fechaHasta.AddDays(1));
            p.AddWithValue("idSucursal", idSucursal);
            p.AddWithValue("cliente", clienteLimpio);
            p.AddWithValue("clienteBuscar", "%" + clienteLimpio + "%");
            p.AddWithValue("vendedor", vendedorLimpio);
            p.AddWithValue("vendedorBuscar", "%" + vendedorLimpio + "%");

            if (formasPago != null)
                for (int i = 0; i < formasPago.Count; i++)
                    p.AddWithValue("fp" + i, formasPago[i]);

            if (codigosComprobante != null)
                for (int i = 0; i < codigosComprobante.Count; i++)
                    p.AddWithValue("cc" + i, codigosComprobante[i]);
        }

        // Mismo patron de paginacion CTE+ROW_NUMBER() que Datos.CatalogoGlobalProducto.
        // ObtenerCatalogoGlobalPagina. Los alias de columna (vendedornombre, sucursalnombre,
        // personarazonsocial, etc.) coinciden exactamente con los que ya reconoce
        // CargarRelacionesVenta -- MapFacturaCompleta reusa MapVenta/CargarRelacionesVenta tal
        // cual, igual que el original SQL Server. v.idventa/v.observaciones NO se listan aca a
        // proposito (facturaelectronica ya tiene columnas con esos nombres via f.*), mismo
        // criterio que el original.
        public List<FacturaElectronica> BuscarFacturasPagina(
            DateTime fechaDesde, DateTime fechaHasta, int idSucursal,
            string cliente, string vendedor, List<string> formasPago, List<int> codigosComprobante,
            int pagina, int cantidad, int cantidadExtra)
        {
            pagina = pagina < 1 ? 1 : pagina;
            cantidad = cantidad < 1 ? 1 : cantidad;
            cantidadExtra = cantidadExtra < 0 ? 0 : cantidadExtra;
            int desdeFila = (int)Math.Min(((long)(pagina - 1) * cantidad) + 1, int.MaxValue);
            int hastaFila = (int)Math.Min((long)desdeFila + cantidad + cantidadExtra - 1, int.MaxValue);

            string where = ConstruirWhereFacturas(formasPago, codigosComprobante);

            string sql = $@"
                WITH facturasfiltradas AS
                (
                    SELECT
                        f.*,
                        v.fechaventa,
                        v.turno,
                        v.diafestivo,
                        v.nroremito,
                        v.estado,
                        v.enctacte,
                        v.cuit,
                        v.email,
                        v.formapago AS ventaformapago,
                        v.tipocomprobante,
                        v.creado AS ventacreado,
                        v.actualizado AS ventaactualizado,
                        v.pagomixtoefectivo,
                        v.idvendedor,
                        v.idsucursal,
                        v.idpersona,
                        COALESCE(lv.totalimportecalculado, 0) AS totalimportecalculado,
                        COALESCE(lv.cantitemscalculado, 0) AS cantitemscalculado,
                        u.nombre AS vendedornombre,
                        u.usuario AS vendedorusuario,
                        u.email AS vendedoremail,
                        u.idempresa AS vendedoridempresa,
                        s.sucursal AS sucursalnombre,
                        s.idempresa AS sucursalidempresa,
                        s.codpuntoventaafip AS sucursalcodpuntoventaafip,
                        s.direccion AS sucursaldireccion,
                        s.localidad AS sucursallocalidad,
                        s.provincia AS sucursalprovincia,
                        s.pais AS sucursalpais,
                        p.razonsocial AS personarazonsocial,
                        p.identificacion AS personaidentificacion,
                        p.idiva AS personaidiva,
                        i.iva AS personaiva,
                        p.cuit AS personacuit,
                        p.telefono AS personatelefono,
                        p.domicilio AS personadomicilio,
                        p.ciudad AS personaciudad,
                        p.ctacte AS personactacte,
                        p.bonificacion AS personabonificacion,
                        ROW_NUMBER() OVER (ORDER BY f.fechaemisionafip DESC, f.id DESC) AS fila
                    FROM facturaelectronica f
                    INNER JOIN ventas v ON v.idventa = f.idventa
                    LEFT JOIN usuarios u ON u.id = v.idvendedor
                    LEFT JOIN sucursal s ON s.idsucursal = v.idsucursal
                    LEFT JOIN personas p ON p.idpersona = v.idpersona
                    LEFT JOIN iva i ON i.id = p.idiva
                    LEFT JOIN (
                        SELECT idventa, SUM(cantkg * preciokg) AS totalimportecalculado, COUNT(*) AS cantitemscalculado
                        FROM lineaventa
                        GROUP BY idventa
                    ) lv ON lv.idventa = v.idventa
                    WHERE {where}
                )
                SELECT *
                FROM facturasfiltradas
                WHERE fila BETWEEN @desdeFila AND @hastaFila
                ORDER BY fila ASC;";

            return DbPg.Reader(_connectionString, _idEmpresa, sql, MapFacturaCompleta,
                p =>
                {
                    AgregarParametrosFacturas(p, fechaDesde, fechaHasta, idSucursal, cliente, vendedor, formasPago, codigosComprobante);
                    p.AddWithValue("desdeFila", desdeFila);
                    p.AddWithValue("hastaFila", hastaFila);
                });
        }

        private FacturaElectronica MapFacturaCompleta(NpgsqlDataReader dr)
        {
            var factura = new FacturaElectronica
            {
                Id = Convert.ToInt32(dr["id"]),
                PtoVtaAfip = GetString(dr, "ptovtaafip"),
                FechaEmisionAfip = dr["fechaemisionafip"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["fechaemisionafip"]),
                DescTipoCbteAfip = GetString(dr, "desctipocbteafip"),
                CodTipoCbteAfip = dr["codtipocbteafip"] == DBNull.Value ? 0 : Convert.ToInt32(dr["codtipocbteafip"]),
                NroCbteAfip = GetString(dr, "nrocbteafip"),
                TipoDocAfip = GetString(dr, "tipodocafip"),
                NroDocAfip = GetString(dr, "nrodocafip"),
                RazonSocialAFIP = GetString(dr, "razonsocialafip"),
                CondicionIvaAFIP = GetString(dr, "condicionivaafip"),
                DomicilioAFIP = GetString(dr, "domicilioafip"),
                CondicionVenta = GetString(dr, "condicionventa"),
                FormaPago = GetString(dr, "formapago"),
                CAE1 = GetString(dr, "cae"),
                FecVtoCAE = GetString(dr, "fecvtocae"),
                ImporteNetoGravado = GetFloat(dr, "importenetogravado"),
                Iva = GetFloat(dr, "iva"),
                ImporteTotal = GetFloat(dr, "importetotal"),
                PorcentajeFacturacion = dr["porcentajefacturacion"] == DBNull.Value ? 100f : Convert.ToSingle(dr["porcentajefacturacion"]),
                DescItemUnitario = GetString(dr, "descitemunitario"),
                Observaciones = GetString(dr, "observaciones"),
                IdVenta = dr["idventa"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idventa"]),
                Error = GetBool(dr, "error"),
                MensajeError = GetString(dr, "mensajeerror"),
                FechaError = dr["fechaerror"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["fechaerror"])
            };

            var venta = MapVenta(dr, false);
            if (string.IsNullOrWhiteSpace(factura.FormaPago))
                factura.FormaPago = venta.FormaPago;

            factura.Venta = venta;
            return factura;
        }

        public (int Cantidad, decimal Total) ObtenerFacturasResumen(
            DateTime fechaDesde, DateTime fechaHasta, int idSucursal,
            string cliente, string vendedor, List<string> formasPago, List<int> codigosComprobante)
        {
            string where = ConstruirWhereFacturas(formasPago, codigosComprobante);

            string sql = $@"
                SELECT
                    COUNT(*) AS cantidad,
                    COALESCE(SUM(
                        CASE
                            WHEN f.codtipocbteafip IN ({FacturaElectronica.codNotaCreditoA_Afip}, {FacturaElectronica.codNotaCreditoB_Afip}, {FacturaElectronica.codNotaCreditoC_Afip})
                            THEN -f.importetotal
                            ELSE f.importetotal
                        END
                    ), 0) AS total
                FROM facturaelectronica f
                INNER JOIN ventas v ON v.idventa = f.idventa
                LEFT JOIN usuarios u ON u.id = v.idvendedor
                LEFT JOIN personas p ON p.idpersona = v.idpersona
                WHERE {where};";

            var filas = DbPg.Reader(_connectionString, _idEmpresa, sql,
                dr => (Cantidad: Convert.ToInt32(dr["cantidad"]), Total: Convert.ToDecimal(dr["total"])),
                p => AgregarParametrosFacturas(p, fechaDesde, fechaHasta, idSucursal, cliente, vendedor, formasPago, codigosComprobante));

            return filas.Count > 0 ? filas[0] : (Cantidad: 0, Total: 0m);
        }

        #endregion
    }
}
