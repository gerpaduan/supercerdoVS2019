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
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM ventas WHERE idventa = @idVenta;",
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

        public int agregarVenta(Venta oVentaE)
        {
            const string sql = @"
                INSERT INTO ventas (idvendedor, fechaventa, idsucursal, turno, diafestivo, observaciones, idpersona,
                    nroremito, estado, enctacte, formapago, tipocomprobante, cuit, email, acumredondeokgs,
                    acumredondeoimporte, comisiontarjeta, pagomixtoefectivo, creado)
                VALUES (@idVendedor, @fechaVenta, @idSucursal, @turno, NULL, @observaciones, @idPersona,
                    @nroRemito, '', @enCtaCte, @formaPago, @tipoComprobante, @cuit, @email, @acumRedondeoKgs,
                    @acumRedondeoImporte, @comisionTarjeta, @pagoMixtoEfectivo, now());
                SELECT idventa FROM ventas WHERE idsucursal = @idSucursal ORDER BY idventa DESC LIMIT 1;";

            object result = DbPg.Scalar(_connectionString, _idEmpresa, sql, p =>
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
                p.AddWithValue("pagoMixtoEfectivo", oVentaE.PagoMixtoEfectivo);
            });

            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public void modificarVenta(Venta oVentaE, int sucAnterior, bool eliminarLineas)
        {
            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
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

                    tx.Commit();
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
                    throw;
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

        public LineaVenta agregarLineaVenta(LineaVenta oLineaE)
        {
            const string sql = @"
                INSERT INTO lineaventa (idventa, idcorte, idanulado, idlineaventaanulado, cantkg, kgsajustetarj,
                    porckgsajustetarj, idalicuotaiva, alicuotaiva, preciokg, ajusteprecio, pesobalanza, bonificacion, idempresa)
                VALUES (@idVenta, @idCorte, @idAnulado, @idLineaVentaAnulado, @cantKg, @kgsAjusteTarj,
                    @porcKgsAjusteTarj, @idAlicuotaIva, @alicuotaIva, @precioKg, @ajustePrecio, @pesoBalanza, @bonificacion, @idEmpresa)
                RETURNING idlineaventa;";

            object nuevoId = DbPg.Scalar(_connectionString, _idEmpresa, sql, p =>
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
            });

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
            // INNER JOIN a corte a proposito (aunque no se consuman sus columnas): el SP real
            // (obtenerLineasVenta, verificado con sp_helptext) hace el mismo INNER JOIN, asi que
            // una linea cuyo idCorte no es visible para el tenant actual (RLS de Corte, ej. un
            // idCorte de otra empresa por un dato viejo/cruzado) queda excluida del resultado --
            // no solo con Corte=null. Confirmado con datos reales (Venta #23, idEmpresa=1, 2
            // lineas con idCorte=3 que pertenece a idEmpresa=3).
            return DbPg.Reader(_connectionString, _idEmpresa,
                @"SELECT lv.idlineaventa, lv.idventa, lv.idcorte, lv.cantkg, lv.idalicuotaiva, lv.alicuotaiva, lv.preciokg,
                    lv.kgsajustetarj, lv.bonificacion, lv.idlineaventaanulado, lv.pesobalanza, lv.idanulado
                  FROM lineaventa lv
                  INNER JOIN corte c ON lv.idcorte = c.idcorte
                  WHERE lv.idventa = @idVenta
                  ORDER BY c.codigo;",
                dr =>
                {
                    var oLinea = new LineaVenta
                    {
                        IdLineaVenta = Convert.ToInt32(dr["idlineaventa"]),
                        Venta = new Venta { IdVenta = Convert.ToInt32(dr["idventa"]) },
                        Corte = _corteRepo.findCorteById(Convert.ToInt32(dr["idcorte"]), false),
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
                    oLinea.Estado = dr["idanulado"] == DBNull.Value ? 0 : 1;

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

                    tx.Commit();
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
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

                    tx.Commit();
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
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
    }
}
