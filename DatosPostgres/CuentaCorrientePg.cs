using System;
using System.Collections.Generic;
using System.Data;
using Entidades;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.ICuentaCorrienteRepository (MovCtaCte, Pagos,
    // Cheques, Bancos). Ver docs/DECISIONS.md, Etapa 5.
    //
    // Decision de alcance documentada (no es un comportamiento inventado): en SQL Server,
    // getChequePorIDorNro/getChequesPorPago resuelven CreadoPor/ActualizadoPor via
    // Datos.Usuario.getUsuarioById, que ademas carga Sucursal+Empresa anidada del usuario.
    // Acá se usa el mismo patron "liviano" (GetUsuarioLiviano) que ya usa el resto de esta
    // clase en SQL Server (MapUsuarioLiviano, sin Sucursal/Empresa anidada) para los 19
    // metodos -- es mas angosto en ese unico punto, documentado, no silencioso.
    //
    // eliminarPago: NotImplementedException -- el SP real no existe en SQL Server (bug
    // preexistente confirmado, fuera de alcance, ver ICuentaCorrienteRepository).
    //
    // Cerrado (ver docs/DECISIONS.md): los alias de columna de obtenerCtasCtes/
    // obtenerResumenDashboard/obtenerCheques/obtenerTotalesPagosBalance/
    // obtenerUltimosPagosDashboard se verificaron y corrigieron contra Datos/CuentaCorriente.cs
    // (algunos con espacios/puntos, ej. "Nombre Identif.", "Razon Social", "obs.", citados
    // entre comillas dobles) al conectar HomeController/FinanzasController/ReportesController
    // via NegocioFactory. obtenerChequesPendientesDashboard no necesito cambios (sin alias
    // multi-palabra en el original). obtenerPagos sigue sin verificar -- sin caller en los
    // controllers ya cableados.
    public class CuentaCorrientePg : Contratos.ICuentaCorrienteRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;
        private readonly Contratos.IPersonaRepository _personaRepo;

        public CuentaCorrientePg(string connectionString, int idEmpresa, Contratos.IPersonaRepository personaRepo)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
            _personaRepo = personaRepo ?? throw new ArgumentNullException(nameof(personaRepo));
        }

        #region Helpers de mapeo (livianos, con prefijo de columna)

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

        private Usuario MapUsuarioLiviano(NpgsqlDataReader dr, string prefix)
        {
            int id = GetInt(dr, prefix + "id");
            if (id <= 0) return null;
            return new Usuario
            {
                Id = id,
                Nombre = GetString(dr, prefix + "nombre"),
                User = GetString(dr, prefix + "user"),
                Email = GetString(dr, prefix + "email"),
                IdSucursal = GetInt(dr, prefix + "idsucursal"),
                IdEmpresa = GetInt(dr, prefix + "idempresa")
            };
        }

        private Sucursal MapSucursalLiviana(NpgsqlDataReader dr, string prefix)
        {
            int id = GetInt(dr, prefix + "id");
            if (id <= 0) return null;
            return new Sucursal
            {
                IdSucursal = id,
                SucursalNombre = GetString(dr, prefix + "nombre"),
                IdEmpresa = GetInt(dr, prefix + "idempresa"),
                CodPuntoVentaAfip = GetInt(dr, prefix + "codpuntoventaafip"),
                Direccion = GetString(dr, prefix + "direccion"),
                Localidad = GetString(dr, prefix + "localidad"),
                Provincia = GetString(dr, prefix + "provincia"),
                Pais = GetString(dr, prefix + "pais")
            };
        }

        private Persona MapPersonaLiviana(NpgsqlDataReader dr, string prefix)
        {
            int id = GetInt(dr, prefix + "id");
            if (id <= 0) return null;
            return new Persona
            {
                idPersona = id,
                Identificacion = GetString(dr, prefix + "identificacion"),
                razonSocial = GetString(dr, prefix + "razonsocial"),
                IdIva = GetInt(dr, prefix + "idiva"),
                Iva = GetString(dr, prefix + "iva"),
                Cuit = GetString(dr, prefix + "cuit"),
                Telefono = GetString(dr, prefix + "telefono"),
                Domicilio = GetString(dr, prefix + "domicilio"),
                Ciudad = GetString(dr, prefix + "ciudad"),
                CtaCte = GetBool(dr, prefix + "ctacte"),
                Bonificacion = GetFloat(dr, prefix + "bonificacion")
            };
        }

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

        #endregion

        #region Cuenta Corriente

        // Alias EXACTOS de Datos/CuentaCorriente.cs (con espacios/puntos donde corresponda,
        // entre comillas dobles) -- HomeController/FinanzasController leen algunos por
        // indexer directo (row["Recibido_De"]), sin fallback, asi que tienen que matchear
        // byte a byte (case-insensitive, Postgres respeta la mayuscula/espacio si se cita).
        public DataTable obtenerCtasCtes(string txtBusqueda, int? idPersona, string ordenSaldo = "DESC")
        {
            string orden = string.Equals(ordenSaldo, "ASC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
            string texto = "%" + (txtBusqueda ?? "").Trim() + "%";

            // @idPersona se castea explicito (::int) en cada uso: Npgsql no puede inferir el
            // tipo de un parametro DBNull sin contexto cuando idPersona es null (mismo patron
            // ya resuelto en VentaPg esta sesion).
            string sql = $@"
                WITH saldos AS (
                    SELECT m.idpersona, SUM(m.importe) AS saldo
                    FROM movctacte m
                    GROUP BY m.idpersona
                )
                SELECT
                    p.idpersona AS ""IdPersona"",
                    p.identificacion AS ""Nombre Identif."",
                    p.razonsocial AS ""Razon Social"",
                    s.saldo AS ""Saldo""
                FROM saldos s
                INNER JOIN personas p ON p.idpersona = s.idpersona
                WHERE
                    (@idPersona::int IS NOT NULL AND @idPersona::int <> 0 AND p.idpersona = @idPersona::int)
                    OR (
                        (@idPersona::int IS NULL OR @idPersona::int = 0)
                        AND (@textoBusqueda = '' OR p.identificacion ILIKE @texto OR p.razonsocial ILIKE @texto)
                    )
                ORDER BY
                    CASE WHEN @ordenSaldo = 'ASC' THEN s.saldo END ASC,
                    CASE WHEN @ordenSaldo = 'DESC' THEN s.saldo END DESC,
                    p.razonsocial ASC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idPersona", (object)idPersona ?? DBNull.Value);
                p.AddWithValue("textoBusqueda", (txtBusqueda ?? "").Trim());
                p.AddWithValue("texto", texto);
                p.AddWithValue("ordenSaldo", orden);
            });
        }

        public DataTable obtenerResumenDashboard()
        {
            const string sql = @"
                WITH saldos AS (
                    SELECT m.idpersona, SUM(m.importe) AS saldo FROM movctacte m GROUP BY m.idpersona
                )
                SELECT
                    COUNT(CASE WHEN s.saldo <> 0 THEN 1 END) AS ""CantidadConSaldo"",
                    COUNT(CASE WHEN s.saldo < -100 THEN 1 END) AS ""CantidadDeudores"",
                    COUNT(CASE WHEN s.saldo > 0 THEN 1 END) AS ""CantidadAcreedores"",
                    SUM(CASE WHEN s.saldo < -100 THEN ABS(s.saldo) ELSE 0 END) AS ""TotalACobrar"",
                    SUM(CASE WHEN s.saldo > 0 THEN s.saldo ELSE 0 END) AS ""TotalAPagar""
                FROM saldos s
                INNER JOIN personas p ON p.idpersona = s.idpersona;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql);
        }

        public DataTable getCtaCteByIdPersona(int idPersona, DateTime fechaDesde)
        {
            // Espeja el SP real dbo.getCtaCteByIdPersona (verificado contra SQL Server real,
            // no inventado -- ver docs/08-relevamiento/snapshot-2026-08-18/stored-procedures.sql
            // linea 3743). Limite estricto de fecha preservado exacto (> / <, no >= / <=): una
            // fila justo en @fechaDesde no aparece en ninguno de los dos bloques.
            //
            // La primera rama del SP original usa literales '-' para id/tabla/idTabla/nroDoc/
            // tipo/sucursal/CreadoPor/ActualizadoPor; SQL Server, al unificar tipos del UNION,
            // convierte esos literales a 0 para las columnas int (comportamiento verificado
            // empiricamente: CAST('-' AS INT) da 0 en SQL Server, no error) y los deja como '-'
            // para las columnas de texto. Postgres no tiene esa conversion implicita, asi que
            // se replican los valores observados directamente en vez de portar la sintaxis.
            const string sql = @"
                SELECT * FROM (
                    SELECT
                        p.idpersona AS idpersona, p.razonsocial AS razonsocial,
                        0 AS id, @fechaDesde AS fecha, '-' AS tabla, 0 AS idtabla, '-' AS nrodoc,
                        'Saldo Anterior' AS detalle, '-' AS tipo, sat.saldoanterior AS importe,
                        0.00 AS saldo, '-' AS sucursal,
                        @fechaDesde AS creado, '-' AS creadopor,
                        @fechaDesde AS actualizado, '-' AS actualizadopor
                    FROM (
                        SELECT m.idpersona, SUM(m.importe) AS saldoanterior
                        FROM movctacte m
                        INNER JOIN personas p ON m.idpersona = p.idpersona
                        WHERE p.idpersona = @idPersona AND m.fecha < @fechaDesde
                        GROUP BY m.idpersona
                    ) sat
                    INNER JOIN personas p ON sat.idpersona = p.idpersona

                    UNION

                    SELECT
                        p.idpersona, p.razonsocial,
                        m.id, m.fecha, m.tabla, m.idtabla, m.nrodoc,
                        m.detalle, m.tipo, m.importe,
                        0.00 AS saldo, s.sucursal,
                        m.creado, creadopor.nombre AS creadopor,
                        m.actualizado, actualizadopor.nombre AS actualizadopor
                    FROM movctacte m
                    INNER JOIN personas p ON m.idpersona = p.idpersona
                    LEFT JOIN sucursal s ON m.idsucursal = s.idsucursal
                    LEFT JOIN usuarios creadopor ON m.creadopor = creadopor.id
                    LEFT JOIN usuarios actualizadopor ON m.actualizadopor = actualizadopor.id
                    WHERE p.idpersona = @idPersona AND m.fecha > @fechaDesde
                ) t
                ORDER BY fecha;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idPersona", idPersona);
                p.AddWithValue("fechaDesde", fechaDesde);
            });
        }

        public MovCtaCte getMovCtaCteBy(int id, MovCtaCte.tablas tabla, int idTabla, MovCtaCte.getBy getBy)
        {
            string where = (getBy == MovCtaCte.getBy.Id) ? "m.id = @id" : "m.tabla = @tabla AND m.idtabla = @idTabla";

            string sql = $@"
                SELECT
                    m.*,
                    p.idpersona AS personaid, p.identificacion AS personaidentificacion, p.razonsocial AS personarazonsocial,
                    p.idiva AS personaidiva, iva.iva AS personaiva, p.cuit AS personacuit, p.telefono AS personatelefono,
                    p.domicilio AS personadomicilio, p.ciudad AS personaciudad, p.ctacte AS personactacte, p.bonificacion AS personabonificacion,
                    s.idsucursal AS sucursalid, s.sucursal AS sucursalnombre, s.idempresa AS sucursalidempresa,
                    s.codpuntoventaafip AS sucursalcodpuntoventaafip, s.direccion AS sucursaldireccion,
                    s.localidad AS sucursallocalidad, s.provincia AS sucursalprovincia, s.pais AS sucursalpais,
                    uc.id AS creadoporid, uc.nombre AS creadopornombre, uc.usuario AS creadoporuser, uc.email AS creadoporemail,
                    uc.idsucursaluser AS creadoporidsucursal, uc.idempresa AS creadoporidempresa,
                    ua.id AS actualizadoporid, ua.nombre AS actualizadopornombre, ua.usuario AS actualizadoporuser, ua.email AS actualizadoporemail,
                    ua.idsucursaluser AS actualizadoporidsucursal, ua.idempresa AS actualizadoporidempresa
                FROM movctacte m
                LEFT JOIN personas p ON p.idpersona = m.idpersona
                LEFT JOIN iva ON iva.id = p.idiva
                LEFT JOIN sucursal s ON s.idsucursal = m.idsucursal
                LEFT JOIN usuarios uc ON uc.id = m.creadopor
                LEFT JOIN usuarios ua ON ua.id = m.actualizadopor
                WHERE {where}
                ORDER BY m.id DESC
                LIMIT 1;";

            var lista = DbPg.Reader(_connectionString, _idEmpresa, sql, dr => new MovCtaCte
            {
                Id = Convert.ToInt32(dr["id"]),
                Fecha = Convert.ToDateTime(dr["fecha"]),
                Tabla = GetString(dr, "tabla"),
                IdTabla = Convert.ToInt32(dr["idtabla"]),
                NroDoc = GetString(dr, "nrodoc"),
                Detalle = GetString(dr, "detalle"),
                Tipo = GetString(dr, "tipo"),
                Importe = Convert.ToSingle(dr["importe"]),
                QuitadoCtaCta = dr["quitadoctacte"] != DBNull.Value && Convert.ToBoolean(dr["quitadoctacte"]),
                // Creado sin guard de NULL: el original (Datos/CuentaCorriente.cs) tampoco lo
                // guarda -- asume que Creado siempre viene poblado. Se replica igual.
                Creado = Convert.ToDateTime(dr["creado"]),
                Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"]),
                Persona = MapPersonaLiviana(dr, "persona"),
                Sucursal = MapSucursalLiviana(dr, "sucursal"),
                CreadoPor = MapUsuarioLiviano(dr, "creadopor"),
                ActualizadoPor = MapUsuarioLiviano(dr, "actualizadopor")
            }, p =>
            {
                p.AddWithValue("id", id);
                p.AddWithValue("tabla", tabla.ToString());
                p.AddWithValue("idTabla", idTabla);
            });

            return lista.Count > 0 ? lista[0] : null;
        }

        public MovCtaCte addOrEditMovCtaCte(MovCtaCte oMovCtaCteE)
        {
            if (oMovCtaCteE == null) throw new ArgumentNullException(nameof(oMovCtaCteE));
            if (oMovCtaCteE.Persona == null) throw new ArgumentException("MovCtaCte.Persona no puede ser null");
            if (oMovCtaCteE.Sucursal == null) throw new ArgumentException("MovCtaCte.Sucursal no puede ser null");
            if (oMovCtaCteE.CreadoPor == null) throw new ArgumentException("MovCtaCte.CreadoPor no puede ser null");

            oMovCtaCteE.Fecha = new DateTime(oMovCtaCteE.Fecha.Year, oMovCtaCteE.Fecha.Month, oMovCtaCteE.Fecha.Day,
                oMovCtaCteE.Fecha.Hour, oMovCtaCteE.Fecha.Minute, oMovCtaCteE.Fecha.Second);

            if (oMovCtaCteE.Id == 0)
            {
                const string sql = @"
                    INSERT INTO movctacte (idpersona, fecha, tabla, idtabla, nrodoc, detalle, tipo, importe, quitadoctacte, idsucursal, creado, creadopor, idempresa)
                    VALUES (@idPersona, @fecha, @tabla, @idTabla, @nroDoc, @detalle, @tipo, @importe, @quitadoCtaCte, @idSucursal, now(), @creadoPor, @idEmpresa)
                    RETURNING id;";

                object nuevoId = DbPg.Scalar(_connectionString, _idEmpresa, sql, p =>
                {
                    p.AddWithValue("idPersona", oMovCtaCteE.Persona.idPersona);
                    p.AddWithValue("fecha", oMovCtaCteE.Fecha);
                    p.AddWithValue("tabla", oMovCtaCteE.Tabla ?? "");
                    p.AddWithValue("idTabla", oMovCtaCteE.IdTabla);
                    p.AddWithValue("nroDoc", oMovCtaCteE.NroDoc ?? "");
                    p.AddWithValue("detalle", oMovCtaCteE.Detalle ?? "");
                    p.AddWithValue("tipo", oMovCtaCteE.Tipo ?? "");
                    p.AddWithValue("importe", oMovCtaCteE.Importe);
                    p.AddWithValue("quitadoCtaCte", oMovCtaCteE.QuitadoCtaCta);
                    p.AddWithValue("idSucursal", oMovCtaCteE.Sucursal.idSucursal);
                    p.AddWithValue("creadoPor", oMovCtaCteE.CreadoPor.Id);
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });
                oMovCtaCteE.Id = Convert.ToInt32(nuevoId);
            }
            else
            {
                const string sql = @"
                    UPDATE movctacte SET
                        idpersona = @idPersona, fecha = @fecha, tabla = @tabla, idtabla = @idTabla, nrodoc = @nroDoc,
                        detalle = @detalle, tipo = @tipo, importe = @importe, quitadoctacte = @quitadoCtaCte,
                        idsucursal = @idSucursal, actualizado = now(), actualizadopor = @actualizadoPor
                    WHERE id = @id;";

                DbPg.NonQuery(_connectionString, _idEmpresa, sql, p =>
                {
                    p.AddWithValue("id", oMovCtaCteE.Id);
                    p.AddWithValue("idPersona", oMovCtaCteE.Persona.idPersona);
                    p.AddWithValue("fecha", oMovCtaCteE.Fecha);
                    p.AddWithValue("tabla", oMovCtaCteE.Tabla ?? "");
                    p.AddWithValue("idTabla", oMovCtaCteE.IdTabla);
                    p.AddWithValue("nroDoc", oMovCtaCteE.NroDoc ?? "");
                    p.AddWithValue("detalle", oMovCtaCteE.Detalle ?? "");
                    p.AddWithValue("tipo", oMovCtaCteE.Tipo ?? "");
                    p.AddWithValue("importe", oMovCtaCteE.Importe);
                    p.AddWithValue("quitadoCtaCte", oMovCtaCteE.QuitadoCtaCta);
                    p.AddWithValue("idSucursal", oMovCtaCteE.Sucursal.idSucursal);
                    p.AddWithValue("actualizadoPor", oMovCtaCteE.ActualizadoPor != null ? oMovCtaCteE.ActualizadoPor.Id : -1);
                });
            }

            return oMovCtaCteE;
        }

        #endregion

        #region Cheques

        public DataTable obtenerCheques(string texto, DateTime fechaDesde, DateTime fechaHasta, bool soloPropios, string estado)
        {
            const string sql = @"
                SELECT
                    c.id, c.nrocheque, c.banco, c.propio,
                    CASE c.propio WHEN true THEN 'Propio' ELSE '3ro' END AS ""Origen"",
                    c.fechaemision, c.fechapago, c.importe, c.estado, c.titular, c.recibidode,
                    recibidopor.identificacion AS ""Recibido_De"",
                    c.entregadoa, entregadopor.identificacion AS ""Entregado_A"",
                    c.observaciones,
                    CASE WHEN LENGTH(c.observaciones) > 30 THEN LEFT(c.observaciones, 30) || '...' ELSE c.observaciones END AS ""obs."",
                    c.creado, creadopor.nombre AS ""CreadoPor"",
                    c.actualizado, actualizadopor.nombre AS ""ActualizadoPor""
                FROM cheques c
                LEFT JOIN pagos pagoentregado ON pagoentregado.id = c.entregadoa
                LEFT JOIN personas entregadopor ON entregadopor.idpersona = pagoentregado.idpersona
                LEFT JOIN pagos pagorecibido ON pagorecibido.id = c.recibidode
                LEFT JOIN personas recibidopor ON recibidopor.idpersona = pagorecibido.idpersona
                LEFT JOIN usuarios actualizadopor ON c.actualizadopor = actualizadopor.id
                LEFT JOIN usuarios creadopor ON c.creadopor = creadopor.id
                WHERE
                    c.fechapago >= @fechaDesde AND c.fechapago < @fechaHasta
                    AND c.nrocheque ILIKE @texto
                    AND c.estado ILIKE @estado
                    AND (
                        (@soloPropios = true AND c.propio = true)
                        OR (@soloPropios = false AND (c.propio = true OR c.propio = false))
                    )
                ORDER BY c.id DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta.AddDays(1));
                p.AddWithValue("texto", "%" + (texto ?? "").Trim() + "%");
                p.AddWithValue("estado", "%" + (estado ?? "").Trim() + "%");
                p.AddWithValue("soloPropios", soloPropios);
            });
        }

        public Cheque getChequePorIDorNro(int id, string nroCheque)
        {
            bool porNro = !string.IsNullOrEmpty(nroCheque);
            string sql = porNro
                ? "SELECT * FROM cheques WHERE nrocheque = @nroCheque ORDER BY id DESC LIMIT 1;"
                : "SELECT * FROM cheques WHERE id = @id;";

            var lista = DbPg.Reader(_connectionString, _idEmpresa, sql, MapCheque, p =>
            {
                if (porNro) p.AddWithValue("nroCheque", nroCheque);
                else p.AddWithValue("id", id);
            });

            var cheque = lista.Count > 0 ? lista[0] : null;
            if (cheque == null) return null;

            cheque.CreadoPor = cheque.IdCreadoPor > 0 ? GetUsuarioLiviano(cheque.IdCreadoPor) : null;
            cheque.ActualizadoPor = cheque.IdActualizadoPor.HasValue ? GetUsuarioLiviano(cheque.IdActualizadoPor.Value) : null;
            cheque.PagoDe = cheque.RecibidoDe > 0 ? getPagoById(cheque.RecibidoDe) : null;
            cheque.PagoA = cheque.EntregadoA > 0 ? getPagoById(cheque.EntregadoA) : null;

            return cheque;
        }

        private Cheque MapCheque(NpgsqlDataReader r) => new Cheque
        {
            Id = Convert.ToInt32(r["id"]),
            NroCheque = GetString(r, "nrocheque"),
            Banco = GetString(r, "banco"),
            Propio = r["propio"] != DBNull.Value && Convert.ToBoolean(r["propio"]),
            FechaEmision = GetString(r, "fechaemision"),
            FechaPago = Convert.ToDateTime(r["fechapago"]),
            Importe = Convert.ToDouble(r["importe"]),
            Estado = GetString(r, "estado"),
            Titular = GetString(r, "titular"),
            Observaciones = GetString(r, "observaciones"),
            RecibidoDe = r["recibidode"] == DBNull.Value ? 0 : Convert.ToInt32(r["recibidode"]),
            EntregadoA = r["entregadoa"] == DBNull.Value ? 0 : Convert.ToInt32(r["entregadoa"]),
            // Creado sin guard de NULL, igual que el original (ver nota en getMovCtaCteBy).
            Creado = Convert.ToDateTime(r["creado"]),
            Actualizado = r["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["actualizado"]),
            IdCreadoPor = r["creadopor"] == DBNull.Value ? 0 : Convert.ToInt32(r["creadopor"]),
            IdActualizadoPor = r["actualizadopor"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["actualizadopor"])
        };

        public List<Cheque> getChequesPorPago(int idPago, bool conPagos = true)
        {
            if (idPago <= 0) return new List<Cheque>();

            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM cheques WHERE recibidode = @idPago OR entregadoa = @idPago;",
                MapCheque,
                p => p.AddWithValue("idPago", idPago));

            for (int i = 0; i < lista.Count; i++)
            {
                lista[i].PagoDe = lista[i].RecibidoDe > 0 ? getPagoById(lista[i].RecibidoDe, conPagos) : null;
                lista[i].PagoA = lista[i].EntregadoA > 0 ? getPagoById(lista[i].EntregadoA, conPagos) : null;
                lista[i].CreadoPor = lista[i].IdCreadoPor > 0 ? GetUsuarioLiviano(lista[i].IdCreadoPor) : null;
                if (lista[i].IdActualizadoPor.HasValue)
                    lista[i].ActualizadoPor = GetUsuarioLiviano(lista[i].IdActualizadoPor.Value);
            }

            return lista;
        }

        private void AddOrEditChequeEnTransaccion(NpgsqlConnection con, NpgsqlTransaction tx, Cheque oCheque)
        {
            string sql = (oCheque.Id == 0)
                ? @"INSERT INTO cheques (nrocheque, banco, propio, fechaemision, fechapago, importe, estado, titular,
                        observaciones, recibidode, entregadoa, creado, creadopor, idempresa)
                    VALUES (@nroCheque, @banco, @propio, @fechaEmision, @fechaPago, @importe, @estado, @titular,
                        @observaciones, @recibidoDe, @entregadoA, @creado, @creadoPor, @idEmpresa);"
                : @"UPDATE cheques SET nrocheque=@nroCheque, banco=@banco, propio=@propio, fechaemision=@fechaEmision,
                        fechapago=@fechaPago, importe=@importe, estado=@estado, titular=@titular,
                        observaciones=@observaciones, recibidode=@recibidoDe, entregadoa=@entregadoA,
                        actualizado=@actualizado, actualizadopor=@actualizadoPor
                    WHERE id=@id;";

            using (var cmd = new NpgsqlCommand(sql, con, tx))
            {
                cmd.Parameters.AddWithValue("nroCheque", oCheque.NroCheque ?? "");
                cmd.Parameters.AddWithValue("banco", oCheque.Banco ?? "");
                cmd.Parameters.AddWithValue("propio", oCheque.Propio);
                cmd.Parameters.AddWithValue("fechaEmision", oCheque.FechaEmision ?? "");
                cmd.Parameters.AddWithValue("fechaPago", oCheque.FechaPago);
                cmd.Parameters.AddWithValue("importe", oCheque.Importe);
                cmd.Parameters.AddWithValue("estado", oCheque.Estado ?? "");
                cmd.Parameters.AddWithValue("titular", oCheque.Titular ?? "");
                cmd.Parameters.AddWithValue("observaciones", oCheque.Observaciones ?? "");
                cmd.Parameters.AddWithValue("recibidoDe", oCheque.RecibidoDe);
                cmd.Parameters.AddWithValue("entregadoA", oCheque.EntregadoA);

                if (oCheque.Id == 0)
                {
                    cmd.Parameters.AddWithValue("creado", (object)oCheque.Creado ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("creadoPor", oCheque.CreadoPor != null ? oCheque.CreadoPor.Id : 0);
                    cmd.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                }
                else
                {
                    cmd.Parameters.AddWithValue("actualizado", (object)oCheque.Actualizado ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("actualizadoPor", oCheque.ActualizadoPor != null ? oCheque.ActualizadoPor.Id : 0);
                    cmd.Parameters.AddWithValue("id", oCheque.Id);
                }

                cmd.ExecuteNonQuery();
            }
        }

        public bool AddOrEditCheque(Cheque oCheque)
        {
            if (oCheque == null) throw new ArgumentNullException(nameof(oCheque));

            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    AddOrEditChequeEnTransaccion(con, tx, oCheque);
                    tx.Commit();
                    return true;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        public bool EliminarCheque(int id)
        {
            int filas = DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM cheques WHERE id = @id;", p => p.AddWithValue("id", id));
            return filas > 0;
        }

        public bool resetearChequesAsignados(int idPago)
        {
            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    using (var cmd1 = new NpgsqlCommand("UPDATE cheques SET recibidode = 0 WHERE recibidode = @idPago;", con, tx))
                    {
                        cmd1.Parameters.AddWithValue("idPago", idPago);
                        cmd1.ExecuteNonQuery();
                    }
                    using (var cmd2 = new NpgsqlCommand("UPDATE cheques SET entregadoa = 0, estado = @estadoReset WHERE entregadoa = @idPago;", con, tx))
                    {
                        cmd2.Parameters.AddWithValue("idPago", idPago);
                        cmd2.Parameters.AddWithValue("estadoReset", "PENDIENTE");
                        cmd2.ExecuteNonQuery();
                    }
                    tx.Commit();
                    return true;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        public List<string> getBancos()
        {
            return DbPg.Reader(_connectionString, _idEmpresa, "SELECT banco FROM bancos;",
                r => (r["banco"] as string ?? "").Trim());
        }

        #endregion

        #region Pagos

        public int getUltimoIdPago()
        {
            object result = DbPg.Scalar(_connectionString, _idEmpresa, "SELECT id FROM pagos ORDER BY id DESC LIMIT 1;");
            return (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
        }

        public Pago addOrEditPago(Pago oPagoE)
        {
            if (oPagoE == null) throw new ArgumentNullException(nameof(oPagoE));
            if (oPagoE.Persona == null) throw new ArgumentException("Pago.Persona no puede ser null");
            if (oPagoE.Sucursal == null) throw new ArgumentException("Pago.Sucursal no puede ser null");
            if (oPagoE.CreadoPor == null) throw new ArgumentException("Pago.CreadoPor no puede ser null");
            if (oPagoE.Cheques == null) oPagoE.Cheques = new List<Cheque>();

            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    // 1) Guardar Pago (siempre alta en este piloto -- ver nota mas abajo)
                    const string sqlInsert = @"
                        INSERT INTO pagos (nrorecibo, fecha, idpersona, aproveedor, formapago, banco, nrocheque,
                            titularcheque, importe, efectivo, observaciones, idsucursal, creado, creadopor, idempresa)
                        VALUES (@nroRecibo, @fecha, @idPersona, @aProveedor, @formaPago, @banco, @nroCheque,
                            @titularCheque, @importe, @efectivo, @observaciones, @idSucursal, now(), @creadoPor, @idEmpresa)
                        RETURNING id;";

                    const string sqlUpdate = @"
                        UPDATE pagos SET nrorecibo=@nroRecibo, fecha=@fecha, idpersona=@idPersona, aproveedor=@aProveedor,
                            formapago=@formaPago, banco=@banco, nrocheque=@nroCheque, titularcheque=@titularCheque,
                            importe=@importe, efectivo=@efectivo, observaciones=@observaciones, idsucursal=@idSucursal,
                            actualizado=now(), actualizadopor=@actualizadoPor
                        WHERE id=@id;";

                    using (var cmd = new NpgsqlCommand(oPagoE.Id == 0 ? sqlInsert : sqlUpdate, con, tx))
                    {
                        cmd.Parameters.AddWithValue("nroRecibo", oPagoE.NroRecibo ?? "");
                        cmd.Parameters.AddWithValue("fecha", oPagoE.Fecha);
                        cmd.Parameters.AddWithValue("idPersona", oPagoE.Persona.idPersona);
                        cmd.Parameters.AddWithValue("aProveedor", oPagoE.AProveedor);
                        cmd.Parameters.AddWithValue("formaPago", oPagoE.FormaPago ?? "");
                        cmd.Parameters.AddWithValue("banco", oPagoE.Banco ?? "");
                        cmd.Parameters.AddWithValue("nroCheque", oPagoE.NroCheque ?? "");
                        cmd.Parameters.AddWithValue("titularCheque", oPagoE.TitularCheque ?? "");
                        cmd.Parameters.AddWithValue("importe", oPagoE.Importe);
                        cmd.Parameters.AddWithValue("efectivo", oPagoE.Efectivo);
                        cmd.Parameters.AddWithValue("observaciones", oPagoE.Observaciones ?? "");
                        cmd.Parameters.AddWithValue("idSucursal", oPagoE.Sucursal.idSucursal);

                        if (oPagoE.Id == 0)
                        {
                            cmd.Parameters.AddWithValue("creadoPor", oPagoE.CreadoPor.Id);
                            cmd.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                            oPagoE.Id = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("actualizadoPor", oPagoE.ActualizadoPor != null ? oPagoE.ActualizadoPor.Id : 0);
                            cmd.Parameters.AddWithValue("id", oPagoE.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 2) Asignar cheques al pago, misma transaccion
                    foreach (Cheque itemBase in oPagoE.Cheques)
                    {
                        var item = itemBase;
                        if (oPagoE.AProveedor)
                        {
                            item.EntregadoA = oPagoE.Id;
                            item.Estado = Cheque.EstadoEnum.ENTREGADO.ToString();
                        }
                        else
                        {
                            item.RecibidoDe = oPagoE.Id;
                        }
                        AddOrEditChequeEnTransaccion(con, tx, item);
                    }

                    tx.Commit();
                    return oPagoE;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        public void eliminarPago(Pago oPagoE)
        {
            throw new NotImplementedException("TODO(claude): el SP 'eliminarPago' no existe en SQL Server (bug real preexistente, confirmado contra sys.procedures) -- solo alcanzable desde Presentacion/WinForms, fuera de alcance de esta migracion. Ver docs/DECISIONS.md, Etapa 5.");
        }

        public DataTable obtenerPagos(string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = @"
                SELECT
                    p.id, p.fecha, per.razonsocial AS ""razonSocial"",
                    p.nrorecibo AS ""nroRecibo"", p.importe, p.aproveedor AS ""aProveedor"",
                    CASE p.aproveedor WHEN false THEN 'Cobro' ELSE 'Pago' END AS ""Operacion"",
                    p.formapago AS ""formaPago"", p.efectivo, p.observaciones, p.creado, creadopor.nombre AS ""CreadoPor"",
                    p.actualizado, actualizadopor.nombre AS ""ActualizadoPor""
                FROM pagos p
                INNER JOIN personas per ON p.idpersona = per.idpersona
                LEFT JOIN usuarios actualizadopor ON p.actualizadopor = actualizadopor.id
                LEFT JOIN usuarios creadopor ON p.creadopor = creadopor.id
                WHERE p.fecha >= @fechaDesde AND p.fecha < @fechaHasta
                  AND (per.razonsocial ILIKE @texto OR p.nrorecibo ILIKE @texto)
                ORDER BY p.fecha DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta.AddDays(1));
                p.AddWithValue("texto", "%" + (texto ?? "").Trim() + "%");
            });
        }

        public DataTable obtenerTotalesPagosBalance(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal)
        {
            const string sql = @"
                SELECT
                    SUM(CASE WHEN p.aproveedor = false THEN p.importe ELSE 0 END) AS ""TotalCobros"",
                    SUM(CASE WHEN p.aproveedor = true THEN p.importe ELSE 0 END) AS ""TotalPagos""
                FROM pagos p
                WHERE p.fecha >= @fechaDesde AND p.fecha < @fechaHasta
                  AND (@idSucursal = -1 OR p.idsucursal = @idSucursal);";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta.AddDays(1));
                p.AddWithValue("idSucursal", idSucursal ?? -1);
            });
        }

        public DataTable obtenerUltimosPagosDashboard(int cantidad)
        {
            const string sql = @"
                SELECT
                    p.id, p.fecha, per.razonsocial,
                    p.nrorecibo, p.importe, p.aproveedor,
                    CASE p.aproveedor WHEN false THEN 'Cobro' ELSE 'Pago' END AS ""Operacion"",
                    p.formapago, p.efectivo, p.observaciones,
                    s.sucursal AS ""Sucursal""
                FROM pagos p
                INNER JOIN personas per ON p.idpersona = per.idpersona
                LEFT JOIN sucursal s ON s.idsucursal = p.idsucursal
                ORDER BY p.fecha DESC, p.id DESC
                LIMIT @cantidad;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("cantidad", cantidad));
        }

        public DataTable obtenerChequesPendientesDashboard(int cantidad, DateTime fechaActual)
        {
            const string sql = @"
                SELECT c.nrocheque, c.banco, c.titular, c.fechapago, c.importe, c.estado, c.observaciones
                FROM cheques c
                WHERE c.estado = @estado AND (c.fechapago + interval '40 days') <= @fechaActual
                ORDER BY c.fechapago ASC, c.id ASC
                LIMIT @cantidad;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("cantidad", cantidad);
                p.AddWithValue("estado", Cheque.EstadoEnum.PENDIENTE.ToString());
                p.AddWithValue("fechaActual", fechaActual.Date);
            });
        }

        public Pago getPagoById(int idPago, bool conCheques = true)
        {
            const string sql = @"
                SELECT
                    p.*,
                    per.idpersona AS personaid, per.identificacion AS personaidentificacion, per.razonsocial AS personarazonsocial,
                    per.idiva AS personaidiva, iva.iva AS personaiva, per.cuit AS personacuit, per.telefono AS personatelefono,
                    per.domicilio AS personadomicilio, per.ciudad AS personaciudad, per.ctacte AS personactacte, per.bonificacion AS personabonificacion,
                    s.idsucursal AS sucursalid, s.sucursal AS sucursalnombre, s.idempresa AS sucursalidempresa,
                    s.codpuntoventaafip AS sucursalcodpuntoventaafip, s.direccion AS sucursaldireccion,
                    s.localidad AS sucursallocalidad, s.provincia AS sucursalprovincia, s.pais AS sucursalpais,
                    uc.id AS creadoporid, uc.nombre AS creadopornombre, uc.usuario AS creadoporuser, uc.email AS creadoporemail,
                    uc.idsucursaluser AS creadoporidsucursal, uc.idempresa AS creadoporidempresa,
                    ua.id AS actualizadoporid, ua.nombre AS actualizadopornombre, ua.usuario AS actualizadoporuser, ua.email AS actualizadoporemail,
                    ua.idsucursaluser AS actualizadoporidsucursal, ua.idempresa AS actualizadoporidempresa
                FROM pagos p
                LEFT JOIN personas per ON per.idpersona = p.idpersona
                LEFT JOIN iva ON iva.id = per.idiva
                LEFT JOIN sucursal s ON s.idsucursal = p.idsucursal
                LEFT JOIN usuarios uc ON uc.id = p.creadopor
                LEFT JOIN usuarios ua ON ua.id = p.actualizadopor
                WHERE p.id = @id;";

            var lista = DbPg.Reader(_connectionString, _idEmpresa, sql, dr => new Pago
            {
                Id = Convert.ToInt32(dr["id"]),
                IdPersona = Convert.ToInt32(dr["idpersona"]),
                Fecha = Convert.ToDateTime(dr["fecha"]),
                NroRecibo = GetString(dr, "nrorecibo"),
                AProveedor = dr["aproveedor"] != DBNull.Value && Convert.ToBoolean(dr["aproveedor"]),
                // Compat con el cambio del 11/12/2025 en SQL Server (Datos/CuentaCorriente.cs
                // getPagoById): "Eftvo+Cheque" persistido antes de ese cambio se traduce a
                // "EftvoCheque" al leer. Se replica igual, no es invencion.
                FormaPago = GetString(dr, "formapago") == "Eftvo+Cheque" ? "EftvoCheque" : GetString(dr, "formapago"),
                Banco = GetString(dr, "banco"),
                NroCheque = GetString(dr, "nrocheque"),
                TitularCheque = GetString(dr, "titularcheque"),
                Importe = Convert.ToSingle(dr["importe"]),
                Efectivo = Convert.ToSingle(dr["efectivo"]),
                Observaciones = GetString(dr, "observaciones"),
                IdSucursal = Convert.ToInt32(dr["idsucursal"]),
                // Creado sin guard de NULL, igual que el original (ver nota en getMovCtaCteBy).
                Creado = Convert.ToDateTime(dr["creado"]),
                Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"]),
                IdCreadoPor = Convert.ToInt32(dr["creadopor"]),
                IdActualizadoPor = dr["actualizadopor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["actualizadopor"]),
                Persona = MapPersonaLiviana(dr, "persona"),
                Sucursal = MapSucursalLiviana(dr, "sucursal"),
                CreadoPor = MapUsuarioLiviano(dr, "creadopor"),
                ActualizadoPor = MapUsuarioLiviano(dr, "actualizadopor")
            }, p => p.AddWithValue("id", idPago));

            var oPagoE = lista.Count > 0 ? lista[0] : null;
            if (oPagoE == null) return null;

            // Persona resuelta vía el repositorio inyectado, no una instancia propia (mismo
            // patron ya usado en PersonaPg/SucursalPg desde las etapas anteriores).
            oPagoE.Persona = _personaRepo.findById(oPagoE.IdPersona);

            if (conCheques)
                oPagoE.Cheques = getChequesPorPago(oPagoE.Id, false);

            return oPagoE;
        }

        #endregion
    }
}
