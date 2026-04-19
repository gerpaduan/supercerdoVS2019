using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidades;
using Utilidades;

namespace Datos
{
    public class CuentaCorriente
    {
        private readonly IEmpresaContext _empresa;private readonly IParametrosContext _param;

        public CuentaCorriente(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa)); _param = param;
        }

        #region Helpers (LIKE seguro + DBNull)

        private static object DbNullIfNull(object value) => value ?? DBNull.Value;

        private static string EscapeLike(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace(@"\", @"\\")
                .Replace("%", @"\%")
                .Replace("_", @"\_")
                .Replace("[", @"\[");

        }

        private static string LikePattern(string text) => "%" + EscapeLike((text ?? "").Trim()) + "%";

        private static bool ColumnaExiste(SqlDataReader dr, string columna)
        {
            try { return dr.GetOrdinal(columna) >= 0; }
            catch { return false; }
        }

        private static string GetString(SqlDataReader dr, string columna)
        {
            return ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value
                ? Convert.ToString(dr[columna])
                : "";
        }

        private static int GetInt(SqlDataReader dr, string columna)
        {
            return ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value
                ? Convert.ToInt32(dr[columna])
                : 0;
        }

        private static float GetFloat(SqlDataReader dr, string columna)
        {
            return ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value
                ? Convert.ToSingle(dr[columna])
                : 0f;
        }

        private static bool GetBool(SqlDataReader dr, string columna)
        {
            return ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value && Convert.ToBoolean(dr[columna]);
        }

        private Entidades.Usuario MapUsuarioLiviano(SqlDataReader dr, string prefix)
        {
            int id = GetInt(dr, prefix + "Id");
            if (id <= 0) return null;

            return new Entidades.Usuario
            {
                Id = id,
                Nombre = GetString(dr, prefix + "Nombre"),
                User = GetString(dr, prefix + "User"),
                Email = GetString(dr, prefix + "Email"),
                IdSucursal = GetInt(dr, prefix + "IdSucursal"),
                IdEmpresa = GetInt(dr, prefix + "IdEmpresa")
            };
        }

        private Entidades.Sucursal MapSucursalLiviana(SqlDataReader dr, string prefix)
        {
            int id = GetInt(dr, prefix + "Id");
            if (id <= 0) return null;

            return new Entidades.Sucursal
            {
                IdSucursal = id,
                SucursalNombre = GetString(dr, prefix + "Nombre"),
                IdEmpresa = GetInt(dr, prefix + "IdEmpresa"),
                CodPuntoVentaAfip = GetInt(dr, prefix + "CodPuntoVentaAfip"),
                Direccion = GetString(dr, prefix + "Direccion"),
                Localidad = GetString(dr, prefix + "Localidad"),
                Provincia = GetString(dr, prefix + "Provincia"),
                Pais = GetString(dr, prefix + "Pais")
            };
        }

        private Entidades.Persona MapPersonaLiviana(SqlDataReader dr, string prefix)
        {
            int id = GetInt(dr, prefix + "Id");
            if (id <= 0) return null;

            return new Entidades.Persona
            {
                idPersona = id,
                Identificacion = GetString(dr, prefix + "Identificacion"),
                razonSocial = GetString(dr, prefix + "RazonSocial"),
                IdIva = GetInt(dr, prefix + "IdIva"),
                Iva = GetString(dr, prefix + "Iva"),
                Cuit = GetString(dr, prefix + "Cuit"),
                Telefono = GetString(dr, prefix + "Telefono"),
                Domicilio = GetString(dr, prefix + "Domicilio"),
                Ciudad = GetString(dr, prefix + "Ciudad"),
                CtaCte = GetBool(dr, prefix + "CtaCte"),
                Bonificacion = GetFloat(dr, prefix + "Bonificacion")
            };
        }

        #endregion

        #region Cuenta Corriente

        public DataTable obtenerCtasCtes(string txtBusqueda, int? idPersona, string ordenSaldo = "DESC")
        {
            string orden = string.Equals(ordenSaldo, "ASC", StringComparison.OrdinalIgnoreCase)
                ? "ASC"
                : "DESC";

            const string sql = @"
                WITH Saldos AS (
                    SELECT
                        m.idPersona,
                        SUM(m.importe) AS Saldo
                    FROM dbo.MovCtaCte m
                    GROUP BY m.idPersona
                )
                SELECT
                    p.idPersona AS IdPersona,
                    p.identificacion AS [Nombre Identif.],
                    p.razonSocial AS [Razon Social],
                    s.Saldo AS Saldo
                FROM Saldos s
                INNER JOIN dbo.Personas p ON p.idPersona = s.idPersona
                WHERE
                    (
                        @idPersona IS NOT NULL
                        AND @idPersona <> 0
                        AND p.idPersona = @idPersona
                    )
                    OR
                    (
                        (@idPersona IS NULL OR @idPersona = 0)
                        AND
                        (
                            @textoBusqueda = ''
                            OR p.identificacion LIKE @texto ESCAPE '\'
                            OR p.razonSocial LIKE @texto ESCAPE '\'
                        )
                    )
                ORDER BY
                    CASE WHEN @ordenSaldo = 'ASC' THEN s.Saldo END ASC,
                    CASE WHEN @ordenSaldo = 'DESC' THEN s.Saldo END DESC,
                    p.razonSocial ASC;";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idPersona", SqlDbType.Int).Value = (object)idPersona ?? DBNull.Value;
                    p.Add("@textoBusqueda", SqlDbType.NVarChar, 200).Value = (txtBusqueda ?? "").Trim();
                    p.Add("@texto", SqlDbType.NVarChar, 200).Value = LikePattern(txtBusqueda);
                    p.Add("@ordenSaldo", SqlDbType.VarChar, 4).Value = orden;
                }
            );
        }

        public DataTable getCtaCteByIdPersona(int idPersona, DateTime fechaDesde)
        {
            return Db.DataTable(
                _empresa,
                "getCtaCteByIdPersona",
                CommandType.StoredProcedure,
                setParams: p =>
                {
                    p.Add("@idPersona", SqlDbType.Int).Value = idPersona;
                    p.Add("@fechaDesde", SqlDbType.DateTime).Value = fechaDesde;
                }
            );
        }

        public Entidades.MovCtaCte getMovCtaCteBy(
            int id,
            Entidades.MovCtaCte.tablas tabla,
            int idTabla,
            Entidades.MovCtaCte.getBy getBy)
        {
            string sql = (getBy == Entidades.MovCtaCte.getBy.Id)
                ? @"
                    SELECT TOP 1
                        m.*,
                        p.idPersona AS personaId,
                        p.identificacion AS personaIdentificacion,
                        p.razonSocial AS personaRazonSocial,
                        p.idIva AS personaIdIva,
                        iva.iva AS personaIva,
                        p.cuit AS personaCuit,
                        p.telefono AS personaTelefono,
                        p.domicilio AS personaDomicilio,
                        p.ciudad AS personaCiudad,
                        p.ctaCte AS personaCtaCte,
                        p.bonificacion AS personaBonificacion,
                        s.idSucursal AS sucursalId,
                        s.sucursal AS sucursalNombre,
                        s.idEmpresa AS sucursalIdEmpresa,
                        s.codPuntoVentaAfip AS sucursalCodPuntoVentaAfip,
                        s.direccion AS sucursalDireccion,
                        s.localidad AS sucursalLocalidad,
                        s.provincia AS sucursalProvincia,
                        s.pais AS sucursalPais,
                        uc.id AS creadoPorId,
                        uc.nombre AS creadoPorNombre,
                        uc.usuario AS creadoPorUser,
                        uc.email AS creadoPorEmail,
                        uc.idSucursalUser AS creadoPorIdSucursal,
                        uc.idEmpresa AS creadoPorIdEmpresa,
                        ua.id AS actualizadoPorId,
                        ua.nombre AS actualizadoPorNombre,
                        ua.usuario AS actualizadoPorUser,
                        ua.email AS actualizadoPorEmail,
                        ua.idSucursalUser AS actualizadoPorIdSucursal,
                        ua.idEmpresa AS actualizadoPorIdEmpresa
                    FROM MovCtaCte m
                    LEFT JOIN Personas p ON p.idPersona = m.idPersona
                    LEFT JOIN Iva iva ON iva.id = p.idIva
                    LEFT JOIN Sucursal s ON s.idSucursal = m.idSucursal
                    LEFT JOIN Usuarios uc ON uc.id = m.creadoPor
                    LEFT JOIN Usuarios ua ON ua.id = m.actualizadoPor
                    WHERE m.id = @id
                    ORDER BY m.id DESC"
                : @"
                    SELECT TOP 1
                        m.*,
                        p.idPersona AS personaId,
                        p.identificacion AS personaIdentificacion,
                        p.razonSocial AS personaRazonSocial,
                        p.idIva AS personaIdIva,
                        iva.iva AS personaIva,
                        p.cuit AS personaCuit,
                        p.telefono AS personaTelefono,
                        p.domicilio AS personaDomicilio,
                        p.ciudad AS personaCiudad,
                        p.ctaCte AS personaCtaCte,
                        p.bonificacion AS personaBonificacion,
                        s.idSucursal AS sucursalId,
                        s.sucursal AS sucursalNombre,
                        s.idEmpresa AS sucursalIdEmpresa,
                        s.codPuntoVentaAfip AS sucursalCodPuntoVentaAfip,
                        s.direccion AS sucursalDireccion,
                        s.localidad AS sucursalLocalidad,
                        s.provincia AS sucursalProvincia,
                        s.pais AS sucursalPais,
                        uc.id AS creadoPorId,
                        uc.nombre AS creadoPorNombre,
                        uc.usuario AS creadoPorUser,
                        uc.email AS creadoPorEmail,
                        uc.idSucursalUser AS creadoPorIdSucursal,
                        uc.idEmpresa AS creadoPorIdEmpresa,
                        ua.id AS actualizadoPorId,
                        ua.nombre AS actualizadoPorNombre,
                        ua.usuario AS actualizadoPorUser,
                        ua.email AS actualizadoPorEmail,
                        ua.idSucursalUser AS actualizadoPorIdSucursal,
                        ua.idEmpresa AS actualizadoPorIdEmpresa
                    FROM MovCtaCte m
                    LEFT JOIN Personas p ON p.idPersona = m.idPersona
                    LEFT JOIN Iva iva ON iva.id = p.idIva
                    LEFT JOIN Sucursal s ON s.idSucursal = m.idSucursal
                    LEFT JOIN Usuarios uc ON uc.id = m.creadoPor
                    LEFT JOIN Usuarios ua ON ua.id = m.actualizadoPor
                    WHERE m.tabla = @tabla AND m.idTabla = @idTabla
                    ORDER BY m.id DESC";

            using (var con = Db.Open(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = Conexion.timeOut;

                // ojo: aunque no se usen en el sql (según getBy), no molesta tenerlos.
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@tabla", SqlDbType.NVarChar, 50).Value = tabla.ToString();
                cmd.Parameters.Add("@idTabla", SqlDbType.Int).Value = idTabla;

                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                        return null;

                    return new Entidades.MovCtaCte
                    {
                        Id = Convert.ToInt32(dr["id"]),
                        Fecha = Convert.ToDateTime(dr["fecha"]),
                        Tabla = Convert.ToString(dr["tabla"]),
                        IdTabla = Convert.ToInt32(dr["idTabla"]),
                        NroDoc = Convert.ToString(dr["nroDoc"]),
                        Detalle = Convert.ToString(dr["detalle"]),
                        Tipo = Convert.ToString(dr["tipo"]),
                        Importe = float.Parse(dr["importe"].ToString()),
                        QuitadoCtaCta = dr["quitadoCtaCte"] != DBNull.Value && Convert.ToBoolean(dr["quitadoCtaCte"]),
                        Creado = Convert.ToDateTime(dr["creado"]),
                        Actualizado = dr["actualizado"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(dr["actualizado"]),
                        Persona = MapPersonaLiviana(dr, "persona"),
                        Sucursal = MapSucursalLiviana(dr, "sucursal"),
                        CreadoPor = MapUsuarioLiviano(dr, "creadoPor"),
                        ActualizadoPor = MapUsuarioLiviano(dr, "actualizadoPor")
                    };
                }
            }
        }

        public Entidades.MovCtaCte addOrEditMovCtaCte(Entidades.MovCtaCte oMovCtaCteE)
        {
            if (oMovCtaCteE == null) throw new ArgumentNullException(nameof(oMovCtaCteE));
            if (oMovCtaCteE.Persona == null) throw new ArgumentException("MovCtaCte.Persona no puede ser null");
            if (oMovCtaCteE.Sucursal == null) throw new ArgumentException("MovCtaCte.Sucursal no puede ser null");
            if (oMovCtaCteE.CreadoPor == null) throw new ArgumentException("MovCtaCte.CreadoPor no puede ser null");

            // Truncar a segundos
            oMovCtaCteE.Fecha = new DateTime(
                oMovCtaCteE.Fecha.Year,
                oMovCtaCteE.Fecha.Month,
                oMovCtaCteE.Fecha.Day,
                oMovCtaCteE.Fecha.Hour,
                oMovCtaCteE.Fecha.Minute,
                oMovCtaCteE.Fecha.Second
            );

            object obj = Db.Scalar(
                _empresa,
                "addOrEditMovCtaCte",
                CommandType.StoredProcedure,
                setParams: p =>
                {
                    p.Add("@id", SqlDbType.Int).Value = oMovCtaCteE.Id;
                    p.Add("@idPersona", SqlDbType.Int).Value = oMovCtaCteE.Persona.idPersona;
                    p.Add("@fecha", SqlDbType.DateTime).Value = oMovCtaCteE.Fecha;
                    p.Add("@tabla", SqlDbType.NVarChar, 50).Value = oMovCtaCteE.Tabla ?? "";
                    p.Add("@idTabla", SqlDbType.Int).Value = oMovCtaCteE.IdTabla;
                    p.Add("@nroDoc", SqlDbType.NVarChar, 50).Value = oMovCtaCteE.NroDoc ?? "";
                    p.Add("@detalle", SqlDbType.NVarChar, 400).Value = oMovCtaCteE.Detalle ?? "";
                    p.Add("@tipo", SqlDbType.NVarChar, 50).Value = oMovCtaCteE.Tipo ?? "";
                    p.Add("@importe", SqlDbType.Float).Value = oMovCtaCteE.Importe;
                    p.Add("@quitadoCtaCte", SqlDbType.Bit).Value = oMovCtaCteE.QuitadoCtaCta;
                    p.Add("@idSucursal", SqlDbType.Int).Value = oMovCtaCteE.Sucursal.idSucursal;
                    p.Add("@creadoPor", SqlDbType.Int).Value = oMovCtaCteE.CreadoPor.Id;
                    p.Add("@actualizadoPor", SqlDbType.Int).Value = oMovCtaCteE.ActualizadoPor != null ? oMovCtaCteE.ActualizadoPor.Id : -1;
                }
            );

            oMovCtaCteE.Id = (obj == null || obj == DBNull.Value) ? oMovCtaCteE.Id : Convert.ToInt32(obj);
            return oMovCtaCteE;
        }

        #endregion

        #region Cheques

        public DataTable obtenerCheques(string texto, DateTime fechaDesde, DateTime fechaHasta, bool soloPropios, string estado)
        {
            const string sql = @"
                SELECT 
                    c.id, c.nroCheque, c.banco, c.propio,
                    CASE c.propio WHEN 1 THEN 'Propio' WHEN 0 THEN '3ro' END AS Origen,
                    c.fechaEmision, c.fechaPago, c.importe, c.estado, c.recibidoDe,
                    RecibidoPor.identificacion AS Recibido_De,
                    c.entregadoA, EntregadoPor.identificacion AS Entregado_A,
                    CASE WHEN LEN(c.observaciones) > 30 THEN LEFT(c.observaciones, 30) + '...' ELSE c.observaciones END AS [obs.],
                    c.creado, CreadoPor.nombre AS CreadoPor,
                    c.actualizado, ActualizadoPor.nombre AS ActualizadoPor
                FROM dbo.Cheques c
                LEFT JOIN dbo.Pagos PagoEntregado ON PagoEntregado.id = c.entregadoA
                LEFT JOIN dbo.Personas EntregadoPor ON EntregadoPor.idPersona = PagoEntregado.idPersona
                LEFT JOIN dbo.Pagos PagoRecibido ON PagoRecibido.id = c.recibidoDe
                LEFT JOIN dbo.Personas RecibidoPor ON RecibidoPor.idPersona = PagoRecibido.idPersona
                LEFT JOIN dbo.Usuarios ActualizadoPor ON c.actualizadoPor = ActualizadoPor.id
                LEFT JOIN dbo.Usuarios CreadoPor ON c.creadoPor = CreadoPor.id
                WHERE 
                    c.fechaPago >= @fechaDesde AND c.fechaPago < @fechaHasta
                    AND c.nroCheque LIKE @texto ESCAPE '\'
                    AND c.estado LIKE @estado ESCAPE '\'
                    AND (
                        (@soloPropios = 1 AND c.propio = 1)
                        OR (@soloPropios = 0 AND (c.propio = 1 OR c.propio = 0))
                    )
                ORDER BY c.id DESC;";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@fechaDesde", SqlDbType.DateTime).Value = fechaDesde;
                    p.Add("@fechaHasta", SqlDbType.DateTime).Value = fechaHasta.AddDays(1); // rango abierto
                    p.Add("@texto", SqlDbType.NVarChar, 120).Value = LikePattern(texto);
                    p.Add("@estado", SqlDbType.NVarChar, 80).Value = LikePattern(estado);
                    p.Add("@soloPropios", SqlDbType.Bit).Value = soloPropios;
                }
            );
        }

        public Cheque getChequePorIDorNro(int id, string nroCheque)
        {
            bool porNro = !string.IsNullOrEmpty(nroCheque);

            string sql = porNro
                ? "SELECT TOP 1 * FROM Cheques WHERE nroCheque = @nroCheque ORDER BY id DESC"
                : "SELECT * FROM Cheques WHERE id = @id";

            var list = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                map: r =>
                {
                    int creadoPorId = r["creadoPor"] == DBNull.Value ? 0 : Convert.ToInt32(r["creadoPor"]);
                    int actualizadoPorId = r["actualizadoPor"] == DBNull.Value ? 0 : Convert.ToInt32(r["actualizadoPor"]);

                    return new Cheque
                    {
                        Id = Convert.ToInt32(r["id"]),
                        NroCheque = Convert.ToString(r["nroCheque"]),
                        Banco = Convert.ToString(r["banco"]),
                        Propio = r["propio"] != DBNull.Value && Convert.ToBoolean(r["propio"]),
                        FechaEmision = Convert.ToString(r["fechaEmision"]),
                        FechaPago = Convert.ToDateTime(r["fechaPago"]),
                        Importe = Convert.ToDouble(r["importe"]),
                        Estado = Convert.ToString(r["estado"]),
                        Titular = Convert.ToString(r["titular"]),
                        Observaciones = Convert.ToString(r["observaciones"]),
                        RecibidoDe = r["recibidoDe"] == DBNull.Value ? 0 : Convert.ToInt32(r["recibidoDe"]),
                        EntregadoA = r["entregadoA"] == DBNull.Value ? 0 : Convert.ToInt32(r["entregadoA"]),
                        Creado = Convert.ToDateTime(r["creado"]),
                        Actualizado = r["actualizado"] != DBNull.Value ? Convert.ToDateTime(r["actualizado"]) : (DateTime?)null,

                        IdCreadoPor = creadoPorId,
                        IdActualizadoPor = actualizadoPorId == 0 ? (int?)null : actualizadoPorId
                    };
                },
                setParams: p =>
                {
                    if (porNro)
                        p.Add("@nroCheque", SqlDbType.NVarChar, 50).Value = nroCheque;
                    else
                        p.Add("@id", SqlDbType.Int).Value = id;
                }
            );

            Cheque cheque = (list.Count > 0) ? list[0] : null;
            if (cheque == null) return null;

            // Relaciones fuera del reader
            var oUserD = new Usuario(_empresa);
            cheque.CreadoPor = cheque.IdCreadoPor > 0 ? oUserD.getUsuarioById(cheque.IdCreadoPor) : null;
            cheque.ActualizadoPor = cheque.IdActualizadoPor.HasValue ? oUserD.getUsuarioById(cheque.IdActualizadoPor.Value) : null;

            cheque.PagoDe = cheque.RecibidoDe > 0 ? getPagoById(cheque.RecibidoDe) : null;
            cheque.PagoA = cheque.EntregadoA > 0 ? getPagoById(cheque.EntregadoA) : null;

            return cheque;
        }

        public List<Entidades.Cheque> getChequesPorPago(int idPago, bool conPagos = true)
        {
            if (idPago <= 0) return new List<Cheque>();

            var listCheques = Db.Reader(
                _empresa,
                "SELECT * FROM Cheques WHERE recibidoDe = @idPago OR entregadoA = @idPago",
                CommandType.Text,
                map: r => new Cheque
                {
                    Id = Convert.ToInt32(r["id"]),
                    NroCheque = Convert.ToString(r["nroCheque"]),
                    Banco = Convert.ToString(r["banco"]),
                    Propio = r["propio"] != DBNull.Value && Convert.ToBoolean(r["propio"]),
                    FechaEmision = Convert.ToString(r["fechaEmision"]),
                    FechaPago = Convert.ToDateTime(r["fechaPago"]),
                    Importe = Convert.ToDouble(r["importe"]),
                    Estado = Convert.ToString(r["estado"]),
                    Titular = Convert.ToString(r["titular"]),
                    Observaciones = Convert.ToString(r["observaciones"]),
                    RecibidoDe = r["recibidoDe"] == DBNull.Value ? 0 : Convert.ToInt32(r["recibidoDe"]),
                    EntregadoA = r["entregadoA"] == DBNull.Value ? 0 : Convert.ToInt32(r["entregadoA"]),
                    Creado = Convert.ToDateTime(r["creado"]),
                    Actualizado = r["actualizado"] != DBNull.Value ? Convert.ToDateTime(r["actualizado"]) : (DateTime?)null,

                    IdCreadoPor = r["creadoPor"] == DBNull.Value ? 0 : Convert.ToInt32(r["creadoPor"]),
                    IdActualizadoPor = r["actualizadoPor"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["actualizadoPor"])
                },
                setParams: p => p.Add("@idPago", SqlDbType.Int).Value = idPago
            );

            // Relaciones fuera del reader
            var oUserD = new Usuario(_empresa);

            for (int i = 0; i < listCheques.Count; i++)
            {
                listCheques[i].PagoDe = listCheques[i].RecibidoDe > 0 ? getPagoById(listCheques[i].RecibidoDe, conPagos) : null;
                listCheques[i].PagoA = listCheques[i].EntregadoA > 0 ? getPagoById(listCheques[i].EntregadoA, conPagos) : null;

                listCheques[i].CreadoPor = listCheques[i].IdCreadoPor > 0 ? oUserD.getUsuarioById(listCheques[i].IdCreadoPor) : null;
                if (listCheques[i].IdActualizadoPor.HasValue)
                    listCheques[i].ActualizadoPor = oUserD.getUsuarioById(listCheques[i].IdActualizadoPor.Value);
            }

            return new List<Entidades.Cheque>(listCheques);
        }

        // Overload interno para usar transacciones (Pago + Cheques)
        private bool AddOrEditCheque(SqlConnection con, SqlTransaction tx, Cheque oCheque)
        {
            const string sqlInsert = @"
                INSERT INTO Cheques (
                    nroCheque, banco, propio, fechaEmision, fechaPago,
                    importe, estado, titular, observaciones, recibidoDe,
                    entregadoA, creado, creadoPor, actualizado, actualizadoPor
                )
                VALUES (
                    @nroCheque, @banco, @propio, @fechaEmision, @fechaPago,
                    @importe, @estado, @titular, @observaciones, @recibidoDe,
                    @entregadoA, @creado, @creadoPor, @actualizado, @actualizadoPor
                );";

            const string sqlUpdate = @"
                UPDATE Cheques SET
                    nroCheque = @nroCheque,
                    banco = @banco,
                    propio = @propio,
                    fechaEmision = @fechaEmision,
                    fechaPago = @fechaPago,
                    importe = @importe,
                    estado = @estado,
                    titular = @titular,
                    observaciones = @observaciones,
                    recibidoDe = @recibidoDe,
                    entregadoA = @entregadoA,
                    actualizado = @actualizado,
                    actualizadoPor = @actualizadoPor
                WHERE id = @id;";

            string sql = (oCheque.Id == 0) ? sqlInsert : sqlUpdate;

            using (var cmd = new SqlCommand(sql, con, tx))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = Conexion.timeOut;

                cmd.Parameters.Add("@nroCheque", SqlDbType.NVarChar, 50).Value = oCheque.NroCheque ?? "";
                cmd.Parameters.Add("@banco", SqlDbType.NVarChar, 50).Value = oCheque.Banco ?? "";
                cmd.Parameters.Add("@propio", SqlDbType.Bit).Value = oCheque.Propio;
                cmd.Parameters.Add("@fechaEmision", SqlDbType.NVarChar, 50).Value = oCheque.FechaEmision ?? "";
                cmd.Parameters.Add("@fechaPago", SqlDbType.DateTime).Value = oCheque.FechaPago;
                cmd.Parameters.Add("@importe", SqlDbType.Float).Value = oCheque.Importe;
                cmd.Parameters.Add("@estado", SqlDbType.NVarChar, 30).Value = oCheque.Estado ?? "";
                cmd.Parameters.Add("@titular", SqlDbType.NVarChar, 120).Value = oCheque.Titular ?? "";
                cmd.Parameters.Add("@observaciones", SqlDbType.NVarChar, -1).Value = (object)(oCheque.Observaciones ?? "") ?? DBNull.Value;
                cmd.Parameters.Add("@recibidoDe", SqlDbType.Int).Value = oCheque.RecibidoDe;
                cmd.Parameters.Add("@entregadoA", SqlDbType.Int).Value = oCheque.EntregadoA;

                if (oCheque.Id == 0)
                {
                    cmd.Parameters.Add("@creado", SqlDbType.DateTime2).Value = (object)(oCheque.Creado ?? DateTime.Now);
                    cmd.Parameters.Add("@creadoPor", SqlDbType.Int).Value = oCheque.CreadoPor != null ? oCheque.CreadoPor.Id : 0;
                    cmd.Parameters.Add("@actualizado", SqlDbType.DateTime2).Value = DbNullIfNull(oCheque.Actualizado);
                    cmd.Parameters.Add("@actualizadoPor", SqlDbType.Int).Value = oCheque.ActualizadoPor != null ? oCheque.ActualizadoPor.Id : 0;
                }
                else
                {
                    cmd.Parameters.Add("@actualizado", SqlDbType.DateTime2).Value = (object)(oCheque.Actualizado ?? DateTime.Now);
                    cmd.Parameters.Add("@actualizadoPor", SqlDbType.Int).Value = oCheque.ActualizadoPor != null ? oCheque.ActualizadoPor.Id : 0;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = oCheque.Id;
                }

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool AddOrEditCheque(Cheque oCheque)
        {
            if (oCheque == null) throw new ArgumentNullException(nameof(oCheque));

            using (var con = Db.Open(_empresa))
            using (var tx = con.BeginTransaction())
            {
                try
                {
                    bool ok = AddOrEditCheque(con, tx, oCheque);
                    tx.Commit();
                    return ok;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public bool EliminarCheque(int id)
        {
            int rows = Db.NonQuery(
                _empresa,
                "DELETE FROM Cheques WHERE id = @id",
                CommandType.Text,
                setParams: p => p.Add("@id", SqlDbType.Int).Value = id
            );

            return rows > 0;
        }

        public bool resetearChequesAsignados(int idPago)
        {
            using (var con = Db.Open(_empresa))
            using (var tx = con.BeginTransaction())
            {
                try
                {
                    using (var cmd1 = new SqlCommand("UPDATE Cheques SET recibidoDe = 0 WHERE recibidoDe = @idPago;", con, tx))
                    {
                        cmd1.CommandType = CommandType.Text;
                        cmd1.CommandTimeout = Conexion.timeOut;
                        cmd1.Parameters.Add("@idPago", SqlDbType.Int).Value = idPago;
                        cmd1.ExecuteNonQuery();
                    }

                    using (var cmd2 = new SqlCommand("UPDATE Cheques SET entregadoA = 0, estado = @estadoReset WHERE entregadoA = @idPago;", con, tx))
                    {
                        cmd2.CommandType = CommandType.Text;
                        cmd2.CommandTimeout = Conexion.timeOut;
                        cmd2.Parameters.Add("@idPago", SqlDbType.Int).Value = idPago;
                        cmd2.Parameters.Add("@estadoReset", SqlDbType.NVarChar, 30).Value = "PENDIENTE";
                        cmd2.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public List<string> getBancos()
        {
            return Db.Reader(
                _empresa,
                "SELECT banco FROM Bancos",
                CommandType.Text,
                map: r => Convert.ToString(r["banco"]).Trim()
            );
        }

        #endregion

        #region Pagos

        public int getUltimoIdPago()
        {
            object result = Db.Scalar(
                _empresa,
                "SELECT TOP 1 id FROM Pagos ORDER BY id DESC",
                CommandType.Text
            );

            return (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
        }

        public Entidades.Pago addOrEditPago(Entidades.Pago oPagoE)
        {
            if (oPagoE == null) throw new ArgumentNullException(nameof(oPagoE));
            if (oPagoE.Persona == null) throw new ArgumentException("Pago.Persona no puede ser null");
            if (oPagoE.Sucursal == null) throw new ArgumentException("Pago.Sucursal no puede ser null");
            if (oPagoE.CreadoPor == null) throw new ArgumentException("Pago.CreadoPor no puede ser null");
            if (oPagoE.Cheques == null) oPagoE.Cheques = new List<Entidades.Cheque>();

            using (var con = Db.Open(_empresa))
            using (var tx = con.BeginTransaction())
            {
                try
                {
                    // 1) Guardar Pago
                    using (var cmd = new SqlCommand("addOrEditPago", con, tx))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = Conexion.timeOut;

                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = oPagoE.Id;
                        cmd.Parameters.Add("@nroRecibo", SqlDbType.NVarChar, 50).Value = oPagoE.NroRecibo ?? "";
                        cmd.Parameters.Add("@fecha", SqlDbType.DateTime).Value = oPagoE.Fecha;
                        cmd.Parameters.Add("@idPersona", SqlDbType.Int).Value = oPagoE.Persona.idPersona;
                        cmd.Parameters.Add("@aProveedor", SqlDbType.Bit).Value = oPagoE.AProveedor;
                        cmd.Parameters.Add("@formaPago", SqlDbType.NVarChar, 50).Value = oPagoE.FormaPago ?? "";
                        cmd.Parameters.Add("@banco", SqlDbType.NVarChar, 50).Value = oPagoE.Banco ?? "";
                        cmd.Parameters.Add("@nroCheque", SqlDbType.NVarChar, 50).Value = oPagoE.NroCheque ?? "";
                        cmd.Parameters.Add("@titularCheque", SqlDbType.NVarChar, 120).Value = oPagoE.TitularCheque ?? "";
                        cmd.Parameters.Add("@importe", SqlDbType.Float).Value = oPagoE.Importe;
                        cmd.Parameters.Add("@efectivo", SqlDbType.Float).Value = oPagoE.Efectivo;
                        cmd.Parameters.Add("@observaciones", SqlDbType.NVarChar, -1).Value = (object)(oPagoE.Observaciones ?? "") ?? DBNull.Value;
                        cmd.Parameters.Add("@idSucursal", SqlDbType.Int).Value = oPagoE.Sucursal.idSucursal;
                        cmd.Parameters.Add("@creadoPor", SqlDbType.Int).Value = oPagoE.CreadoPor.Id;
                        cmd.Parameters.Add("@actualizadoPor", SqlDbType.Int).Value = oPagoE.ActualizadoPor != null ? oPagoE.ActualizadoPor.Id : 0;

                        oPagoE.Id = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 2) Asignar cheques al pago dentro de la misma transacción
                    foreach (Entidades.Cheque itemBase in oPagoE.Cheques)
                    {
                        var item = (Cheque)itemBase;

                        if (oPagoE.AProveedor)
                        {
                            item.EntregadoA = oPagoE.Id;
                            item.Estado = Entidades.Cheque.EstadoEnum.ENTREGADO.ToString();
                        }
                        else
                        {
                            item.RecibidoDe = oPagoE.Id;
                        }

                        AddOrEditCheque(con, tx, item);
                    }

                    tx.Commit();
                    return oPagoE;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public void eliminarPago(Entidades.Pago oPagoE)
        {
            if (oPagoE == null) throw new ArgumentNullException(nameof(oPagoE));

            Db.NonQuery(
                _empresa,
                "eliminarPago",
                CommandType.StoredProcedure,
                setParams: p => p.Add("@Id", SqlDbType.Int).Value = oPagoE.Id
            );
        }

        public DataTable obtenerPagos(string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = @"
                SELECT 
                    p.id, p.fecha, per.razonSocial,
                    p.nroRecibo, p.importe, p.aProveedor,
                    CASE p.aProveedor WHEN 0 THEN 'Cobro' WHEN 1 THEN 'Pago' END AS Operacion,
                    p.formaPago, p.efectivo, p.observaciones, p.creado, CreadoPor.nombre AS CreadoPor,
                    p.actualizado, ActualizadoPor.nombre AS ActualizadoPor
                FROM dbo.Pagos p
                INNER JOIN dbo.Personas per ON p.idPersona = per.idPersona
                LEFT JOIN dbo.Usuarios ActualizadoPor ON p.actualizadoPor = ActualizadoPor.id
                LEFT JOIN dbo.Usuarios CreadoPor ON p.creadoPor = CreadoPor.id
                WHERE 
                    p.fecha >= @fechaDesde AND p.fecha < @fechaHasta
                    AND (per.razonSocial LIKE @texto ESCAPE '\' OR p.nroRecibo LIKE @texto ESCAPE '\')
                ORDER BY p.fecha DESC;";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@fechaDesde", SqlDbType.DateTime).Value = fechaDesde;
                    p.Add("@fechaHasta", SqlDbType.DateTime).Value = fechaHasta.AddDays(1);
                    p.Add("@texto", SqlDbType.NVarChar, 200).Value = LikePattern(texto);
                }
            );
        }

        public Entidades.Pago getPagoById(int idPago, bool conCheques = true)
        {
            Entidades.Pago oPagoE = null;

            using (var con = Db.Open(_empresa))
            using (var cmd = new SqlCommand(@"
                SELECT
                    p.*,
                    per.idPersona AS personaId,
                    per.identificacion AS personaIdentificacion,
                    per.razonSocial AS personaRazonSocial,
                    per.idIva AS personaIdIva,
                    iva.iva AS personaIva,
                    per.cuit AS personaCuit,
                    per.telefono AS personaTelefono,
                    per.domicilio AS personaDomicilio,
                    per.ciudad AS personaCiudad,
                    per.ctaCte AS personaCtaCte,
                    per.bonificacion AS personaBonificacion,
                    s.idSucursal AS sucursalId,
                    s.sucursal AS sucursalNombre,
                    s.idEmpresa AS sucursalIdEmpresa,
                    s.codPuntoVentaAfip AS sucursalCodPuntoVentaAfip,
                    s.direccion AS sucursalDireccion,
                    s.localidad AS sucursalLocalidad,
                    s.provincia AS sucursalProvincia,
                    s.pais AS sucursalPais,
                    uc.id AS creadoPorId,
                    uc.nombre AS creadoPorNombre,
                    uc.usuario AS creadoPorUser,
                    uc.email AS creadoPorEmail,
                    uc.idSucursalUser AS creadoPorIdSucursal,
                    uc.idEmpresa AS creadoPorIdEmpresa,
                    ua.id AS actualizadoPorId,
                    ua.nombre AS actualizadoPorNombre,
                    ua.usuario AS actualizadoPorUser,
                    ua.email AS actualizadoPorEmail,
                    ua.idSucursalUser AS actualizadoPorIdSucursal,
                    ua.idEmpresa AS actualizadoPorIdEmpresa
                FROM Pagos p
                LEFT JOIN Personas per ON per.idPersona = p.idPersona
                LEFT JOIN Iva iva ON iva.id = per.idIva
                LEFT JOIN Sucursal s ON s.idSucursal = p.idSucursal
                LEFT JOIN Usuarios uc ON uc.id = p.creadoPor
                LEFT JOIN Usuarios ua ON ua.id = p.actualizadoPor
                WHERE p.id = @id", con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = Conexion.timeOut;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idPago;

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        oPagoE = new Entidades.Pago
                        {
                            Id = Convert.ToInt32(dr["id"]),
                            IdPersona = Convert.ToInt32(dr["idPersona"]),
                            Fecha = Convert.ToDateTime(dr["fecha"]),
                            NroRecibo = Convert.ToString(dr["nroRecibo"]),
                            AProveedor = dr["aProveedor"] != DBNull.Value && Convert.ToBoolean(dr["aProveedor"]),
                            // compat por tu cambio del 11/12/2025
                            FormaPago = Convert.ToString(dr["formaPago"]).Equals("Eftvo+Cheque") ? "EftvoCheque" : Convert.ToString(dr["formaPago"]),
                            Banco = Convert.ToString(dr["banco"]),
                            NroCheque = Convert.ToString(dr["nroCheque"]),
                            TitularCheque = Convert.ToString(dr["titularCheque"]),
                            Importe = float.Parse(dr["importe"].ToString()),
                            Efectivo = float.Parse(dr["efectivo"].ToString()),
                            Observaciones = Convert.ToString(dr["observaciones"]),
                            IdSucursal = Convert.ToInt32(dr["idSucursal"]),
                            Creado = Convert.ToDateTime(dr["creado"]),
                            Actualizado = dr["actualizado"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(dr["actualizado"]),
                            IdCreadoPor = Convert.ToInt32(dr["creadoPor"]),
                            IdActualizadoPor = dr["actualizadoPor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["actualizadoPor"]),
                            Persona = MapPersonaLiviana(dr, "persona"),
                            Sucursal = MapSucursalLiviana(dr, "sucursal"),
                            CreadoPor = MapUsuarioLiviano(dr, "creadoPor"),
                            ActualizadoPor = MapUsuarioLiviano(dr, "actualizadoPor")
                        };
                    }
                }
            }

            if (oPagoE == null)
                return null;

            // Relaciones fuera del reader
            var oPersonaD = new Datos.Persona(_empresa, _param);
            oPagoE.Persona = oPersonaD.findById(oPagoE.IdPersona);

            if (conCheques)
                oPagoE.Cheques = getChequesPorPago(oPagoE.Id, false);

            return oPagoE;
        }

        #endregion
    }
}
