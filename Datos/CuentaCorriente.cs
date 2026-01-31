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
        private readonly Utilidades.Conexion conn;
        private readonly IEmpresaContext _empresa;

        public CuentaCorriente(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
            conn = new Utilidades.Conexion();
        }

        #region Helpers (LIKE seguro + DBNull)

        private static object DbNullIfNull(object value) => value ?? DBNull.Value;

        private static string EscapeLike(string text)
        {
            // Escapa caracteres especiales de LIKE: %, _, [, y la barra invertida
            // Usamos ESCAPE '\'
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace(@"\", @"\\")
                .Replace("%", @"\%")
                .Replace("_", @"\_")
                .Replace("[", @"\[");
        }

        private static string LikePattern(string text) => "%" + EscapeLike((text ?? "").Trim()) + "%";

        #endregion

        #region Cuenta Corriente

        public DataTable obtenerCtasCtes(string txtBusqueda, int? idPersona)
        {
            var dt = new DataTable();

            string sql = @"
                SELECT 
                    p.idPersona AS IdPersona,
                    p.identificacion AS [Nombre Identif.],
                    p.razonSocial AS [Razon Social],
                    SUM(m.importe) AS Saldo
                FROM dbo.Personas p
                INNER JOIN dbo.MovCtaCte m ON p.idPersona = m.idPersona
                WHERE 
                    (
                        @idPersona IS NOT NULL AND @idPersona <> 0 AND p.idPersona = @idPersona
                    )
                    OR
                    (
                        (@idPersona IS NULL OR @idPersona = 0)
                        AND (p.identificacion LIKE @texto ESCAPE '\' OR p.razonSocial LIKE @texto ESCAPE '\')
                    )
                GROUP BY p.idPersona, p.identificacion, p.razonSocial
                ORDER BY p.razonSocial;";

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idPersona", (object)idPersona ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@texto", LikePattern(txtBusqueda));

                da.Fill(dt);
            }

            return dt;
        }

        public DataTable getCtaCteByIdPersona(int idPersona, DateTime fechaDesde)
        {
            var dt = new DataTable();

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("getCtaCteByIdPersona", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idPersona", idPersona);
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);

                da.Fill(dt);
            }

            return dt;
        }

        public Entidades.MovCtaCte getMovCtaCteBy(
            int id,
            Entidades.MovCtaCte.tablas tabla,
            int idTabla,
            Entidades.MovCtaCte.getBy getBy)
        {
            Entidades.MovCtaCte mov = null;

            string sql;
            if (getBy == Entidades.MovCtaCte.getBy.Id)
            {
                sql = "SELECT TOP 1 * FROM MovCtaCte WHERE id = @id ORDER BY id DESC";
            }
            else // TablaAndId
            {
                sql = "SELECT TOP 1 * FROM MovCtaCte WHERE tabla = @tabla AND idTabla = @idTabla ORDER BY id DESC";
            }

            int idPersona = 0;
            int idSucursal = 0;
            int idCreadoPor = 0;
            int? idActualizadoPor = null;

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@tabla", tabla.ToString());
                cmd.Parameters.AddWithValue("@idTabla", idTabla);

                if (con.State != ConnectionState.Open) con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                        return null;

                    mov = new Entidades.MovCtaCte
                    {
                        Id = Convert.ToInt32(dr["id"]),
                        Fecha = Convert.ToDateTime(dr["fecha"]),
                        Tabla = Convert.ToString(dr["tabla"]),
                        IdTabla = Convert.ToInt32(dr["idTabla"]),
                        NroDoc = Convert.ToString(dr["nroDoc"]),
                        Detalle = Convert.ToString(dr["detalle"]),
                        Tipo = Convert.ToString(dr["tipo"]),
                        Importe = float.Parse(dr["importe"].ToString()),
                        QuitadoCtaCta = dr["quitadoCtaCte"] == DBNull.Value ? false : Convert.ToBoolean(dr["quitadoCtaCte"]),
                        Creado = Convert.ToDateTime(dr["creado"]),
                        Actualizado = dr["actualizado"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(dr["actualizado"])
                    };

                    idPersona = dr["idPersona"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idPersona"]);
                    idSucursal = dr["idSucursal"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idSucursal"]);
                    idCreadoPor = dr["creadoPor"] == DBNull.Value ? 0 : Convert.ToInt32(dr["creadoPor"]);
                    idActualizadoPor = dr["actualizadoPor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["actualizadoPor"]);
                }
            }

            // Cargar relaciones fuera del reader
            var oUsuarioD = new Datos.Usuario(_empresa);
            mov.CreadoPor = idCreadoPor > 0 ? oUsuarioD.getUsuarioById(idCreadoPor) : null;
            mov.ActualizadoPor = idActualizadoPor.HasValue ? oUsuarioD.getUsuarioById(idActualizadoPor.Value) : null;

            var oSucursalD = new Datos.Sucursal(_empresa);
            mov.Sucursal = idSucursal > 0 ? oSucursalD.findById(idSucursal) : null;

            var oPersonaD = new Datos.Persona(_empresa);
            mov.Persona = idPersona > 0 ? oPersonaD.findById(idPersona) : null;

            return mov;
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

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("addOrEditMovCtaCte", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@id", oMovCtaCteE.Id);
                cmd.Parameters.AddWithValue("@idPersona", oMovCtaCteE.Persona.idPersona);
                cmd.Parameters.AddWithValue("@fecha", oMovCtaCteE.Fecha);
                cmd.Parameters.AddWithValue("@tabla", oMovCtaCteE.Tabla ?? "");
                cmd.Parameters.AddWithValue("@idTabla", oMovCtaCteE.IdTabla);
                cmd.Parameters.AddWithValue("@nroDoc", oMovCtaCteE.NroDoc ?? "");
                cmd.Parameters.AddWithValue("@detalle", oMovCtaCteE.Detalle ?? "");
                cmd.Parameters.AddWithValue("@tipo", oMovCtaCteE.Tipo ?? "");
                cmd.Parameters.AddWithValue("@importe", oMovCtaCteE.Importe);
                cmd.Parameters.AddWithValue("@quitadoCtaCte", oMovCtaCteE.QuitadoCtaCta);
                cmd.Parameters.AddWithValue("@idSucursal", oMovCtaCteE.Sucursal.idSucursal);
                cmd.Parameters.AddWithValue("@creadoPor", oMovCtaCteE.CreadoPor.Id);
                cmd.Parameters.AddWithValue("@actualizadoPor", oMovCtaCteE.ActualizadoPor != null ? oMovCtaCteE.ActualizadoPor.Id : -1);

                if (con.State != ConnectionState.Open) con.Open();
                oMovCtaCteE.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return oMovCtaCteE;
        }

        #endregion

        #region Cheques

        public DataTable obtenerCheques(string texto, DateTime fechaDesde, DateTime fechaHasta, bool soloPropios, string estado)
        {
            var dt = new DataTable();

            string sql = @"
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

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta.AddDays(1)); // rango abierto
                cmd.Parameters.AddWithValue("@texto", LikePattern(texto));
                cmd.Parameters.AddWithValue("@estado", LikePattern(estado));
                cmd.Parameters.AddWithValue("@soloPropios", soloPropios ? 1 : 0);

                da.Fill(dt);
            }

            return dt;
        }

        public Cheque getChequePorIDorNro(int id, string nroCheque)
        {
            Cheque cheque = null;

            string sql = string.IsNullOrEmpty(nroCheque)
                ? "SELECT * FROM Cheques WHERE id = @id"
                : "SELECT TOP 1 * FROM Cheques WHERE nroCheque = @nroCheque ORDER BY id DESC";

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@nroCheque", nroCheque ?? "");

                if (con.State != ConnectionState.Open) con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                        return null;

                    int creadoPorId = r["creadoPor"] == DBNull.Value ? 0 : Convert.ToInt32(r["creadoPor"]);
                    int actualizadoPorId = r["actualizadoPor"] == DBNull.Value ? 0 : Convert.ToInt32(r["actualizadoPor"]);

                    cheque = new Cheque
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
                }
            }

            // Relaciones fuera del DataReader
            var oUserD = new Usuario(_empresa);
            cheque.CreadoPor = cheque.IdCreadoPor > 0 ? oUserD.getUsuarioById(cheque.IdCreadoPor) : null;
            cheque.ActualizadoPor = cheque.IdActualizadoPor.HasValue ? oUserD.getUsuarioById(cheque.IdActualizadoPor.Value) : null;

            cheque.PagoDe = cheque.RecibidoDe > 0 ? getPagoById(cheque.RecibidoDe) : null;
            cheque.PagoA = cheque.EntregadoA > 0 ? getPagoById(cheque.EntregadoA) : null;

            return cheque;
        }

        /// <summary>
        /// Recupera los Cheques asociados a un Pago (conPagos=false si se llama desde Pagos para evitar bucle)
        /// </summary>
        public List<Entidades.Cheque> getChequesPorPago(int idPago, bool conPagos = true)
        {
            var listCheques = new List<Cheque>();
            if (idPago <= 0) return listCheques;

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("SELECT * FROM Cheques WHERE recibidoDe = @idPago OR entregadoA = @idPago", con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idPago", idPago);

                if (con.State != ConnectionState.Open) con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var cheque = new Cheque
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
                        };

                        listCheques.Add(cheque);
                    }
                }
            }

            // Cargar relaciones fuera del reader
            var oUserD = new Usuario(_empresa);

            for (int i = 0; i < listCheques.Count; i++)
            {
                listCheques[i].PagoDe = listCheques[i].RecibidoDe > 0 ? getPagoById(listCheques[i].RecibidoDe, conPagos) : null;
                listCheques[i].PagoA = listCheques[i].EntregadoA > 0 ? getPagoById(listCheques[i].EntregadoA, conPagos) : null;

                listCheques[i].CreadoPor = listCheques[i].IdCreadoPor > 0 ? oUserD.getUsuarioById(listCheques[i].IdCreadoPor) : null;
                if (listCheques[i].IdActualizadoPor.HasValue)
                    listCheques[i].ActualizadoPor = oUserD.getUsuarioById(listCheques[i].IdActualizadoPor.Value);
            }

            return listCheques;
        }

        // Overload interno para usar transacciones (Pago + Cheques)
        private bool AddOrEditCheque(SqlConnection con, SqlTransaction tx, Cheque oCheque)
        {
            string sqlInsert = @"
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

            string sqlUpdate = @"
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
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@nroCheque", oCheque.NroCheque ?? "");
                cmd.Parameters.AddWithValue("@banco", oCheque.Banco ?? "");
                cmd.Parameters.AddWithValue("@propio", oCheque.Propio ? 1 : 0);
                cmd.Parameters.AddWithValue("@fechaEmision", oCheque.FechaEmision ?? "");
                cmd.Parameters.AddWithValue("@fechaPago", oCheque.FechaPago);
                cmd.Parameters.AddWithValue("@importe", oCheque.Importe);
                cmd.Parameters.AddWithValue("@estado", oCheque.Estado ?? "");
                cmd.Parameters.AddWithValue("@titular", oCheque.Titular ?? "");
                cmd.Parameters.AddWithValue("@observaciones", oCheque.Observaciones ?? "");
                cmd.Parameters.AddWithValue("@recibidoDe", oCheque.RecibidoDe);
                cmd.Parameters.AddWithValue("@entregadoA", oCheque.EntregadoA);

                if (oCheque.Id == 0)
                {
                    cmd.Parameters.AddWithValue("@creado", oCheque.Creado ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("@creadoPor", oCheque.CreadoPor != null ? oCheque.CreadoPor.Id : 0);
                    cmd.Parameters.AddWithValue("@actualizado", DbNullIfNull(oCheque.Actualizado));
                    cmd.Parameters.AddWithValue("@actualizadoPor", oCheque.ActualizadoPor != null ? oCheque.ActualizadoPor.Id : 0);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@actualizado", (object)(oCheque.Actualizado ?? DateTime.Now));
                    cmd.Parameters.AddWithValue("@actualizadoPor", oCheque.ActualizadoPor != null ? oCheque.ActualizadoPor.Id : 0);
                    cmd.Parameters.AddWithValue("@id", oCheque.Id);
                }

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool AddOrEditCheque(Cheque oCheque)
        {
            if (oCheque == null) throw new ArgumentNullException(nameof(oCheque));

            using (var con = conn.conectar(_empresa))
            {
                if (con.State != ConnectionState.Open) con.Open();
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
        }

        public bool EliminarCheque(int id)
        {
            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("DELETE FROM Cheques WHERE id = @id", con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@id", id);

                if (con.State != ConnectionState.Open) con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool resetearChequesAsignados(int idPago)
        {
            using (var con = conn.conectar(_empresa))
            {
                if (con.State != ConnectionState.Open) con.Open();
                using (var tx = con.BeginTransaction())
                {
                    try
                    {
                        using (var cmd1 = new SqlCommand("UPDATE Cheques SET recibidoDe = 0 WHERE recibidoDe = @idPago;", con, tx))
                        {
                            cmd1.CommandType = CommandType.Text;
                            cmd1.CommandTimeout = conn.TimeOut();
                            cmd1.Parameters.AddWithValue("@idPago", idPago);
                            cmd1.ExecuteNonQuery();
                        }

                        using (var cmd2 = new SqlCommand("UPDATE Cheques SET entregadoA = 0, estado = @estadoReset WHERE entregadoA = @idPago;", con, tx))
                        {
                            cmd2.CommandType = CommandType.Text;
                            cmd2.CommandTimeout = conn.TimeOut();
                            cmd2.Parameters.AddWithValue("@idPago", idPago);
                            cmd2.Parameters.AddWithValue("@estadoReset", "PENDIENTE");
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
        }

        public List<string> getBancos()
        {
            var bancos = new List<string>();

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("SELECT banco FROM Bancos", con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                if (con.State != ConnectionState.Open) con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        bancos.Add(Convert.ToString(r["banco"]).Trim());
                }
            }

            return bancos;
        }

        #endregion

        #region Pagos

        public int getUltimoIdPago()
        {
            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("SELECT TOP 1 id FROM Pagos ORDER BY id DESC", con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                if (con.State != ConnectionState.Open) con.Open();
                object result = cmd.ExecuteScalar();
                return (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
            }
        }

        public Entidades.Pago addOrEditPago(Entidades.Pago oPagoE)
        {
            if (oPagoE == null) throw new ArgumentNullException(nameof(oPagoE));
            if (oPagoE.Persona == null) throw new ArgumentException("Pago.Persona no puede ser null");
            if (oPagoE.Sucursal == null) throw new ArgumentException("Pago.Sucursal no puede ser null");
            if (oPagoE.CreadoPor == null) throw new ArgumentException("Pago.CreadoPor no puede ser null");
            if (oPagoE.Cheques == null) oPagoE.Cheques = new List<Entidades.Cheque>();

            using (var con = conn.conectar(_empresa))
            {
                if (con.State != ConnectionState.Open) con.Open();
                using (var tx = con.BeginTransaction())
                {
                    try
                    {
                        // 1) Guardar Pago
                        using (var cmd = new SqlCommand("addOrEditPago", con, tx))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = conn.TimeOut();

                            cmd.Parameters.AddWithValue("@id", oPagoE.Id);
                            cmd.Parameters.AddWithValue("@nroRecibo", oPagoE.NroRecibo ?? "");
                            cmd.Parameters.AddWithValue("@fecha", oPagoE.Fecha);
                            cmd.Parameters.AddWithValue("@idPersona", oPagoE.Persona.idPersona);
                            cmd.Parameters.AddWithValue("@aProveedor", oPagoE.AProveedor);
                            cmd.Parameters.AddWithValue("@formaPago", oPagoE.FormaPago ?? "");
                            cmd.Parameters.AddWithValue("@banco", oPagoE.Banco ?? "");
                            cmd.Parameters.AddWithValue("@nroCheque", oPagoE.NroCheque ?? "");
                            cmd.Parameters.AddWithValue("@titularCheque", oPagoE.TitularCheque ?? "");
                            cmd.Parameters.AddWithValue("@importe", oPagoE.Importe);
                            cmd.Parameters.AddWithValue("@efectivo", oPagoE.Efectivo);
                            cmd.Parameters.AddWithValue("@observaciones", oPagoE.Observaciones ?? "");
                            cmd.Parameters.AddWithValue("@idSucursal", oPagoE.Sucursal.idSucursal);
                            cmd.Parameters.AddWithValue("@creadoPor", oPagoE.CreadoPor.Id);
                            cmd.Parameters.AddWithValue("@actualizadoPor", oPagoE.ActualizadoPor != null ? oPagoE.ActualizadoPor.Id : 0);

                            oPagoE.Id = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 2) Asignar cheques al pago dentro de la misma transacción
                        foreach (Entidades.Cheque item in oPagoE.Cheques)
                        {
                            if (oPagoE.AProveedor)
                            {
                                item.EntregadoA = oPagoE.Id;
                                item.Estado = Entidades.Cheque.EstadoEnum.ENTREGADO.ToString();
                            }
                            else
                            {
                                item.RecibidoDe = oPagoE.Id;
                            }

                            // Guardar cheque con la misma conexión/transacción
                            AddOrEditCheque(con, tx, (Cheque)item);
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
        }

        public void eliminarPago(Entidades.Pago oPagoE)
        {
            if (oPagoE == null) throw new ArgumentNullException(nameof(oPagoE));

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("eliminarPago", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@Id", oPagoE.Id);

                if (con.State != ConnectionState.Open) con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable obtenerPagos(string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            var dt = new DataTable();

            string sql = @"
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

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta.AddDays(1));
                cmd.Parameters.AddWithValue("@texto", LikePattern(texto));

                da.Fill(dt);
            }

            return dt;
        }

        public Entidades.Pago getPagoById(int idPago, bool conCheques = true)
        {
            Entidades.Pago oPagoE = null;

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("SELECT * FROM Pagos WHERE id = @id", con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@id", idPago);

                if (con.State != ConnectionState.Open) con.Open();
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
                            IdActualizadoPor = dr["actualizadoPor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["actualizadoPor"])
                        };
                    }
                }
            }

            if (oPagoE == null)
                return null;

            // Relaciones fuera del reader
            var oPersonaD = new Datos.Persona(_empresa);
            oPagoE.Persona = oPersonaD.findById(oPagoE.IdPersona);

            if (conCheques)
                oPagoE.Cheques = getChequesPorPago(oPagoE.Id, false);

            var oSucursalD = new Datos.Sucursal(_empresa);
            oPagoE.Sucursal = oSucursalD.findById(oPagoE.IdSucursal);

            var oUsuarioD = new Datos.Usuario(_empresa);
            oPagoE.CreadoPor = oUsuarioD.getUsuarioById(oPagoE.IdCreadoPor);
            if (oPagoE.IdActualizadoPor.HasValue)
                oPagoE.ActualizadoPor = oUsuarioD.getUsuarioById(oPagoE.IdActualizadoPor.Value);

            return oPagoE;
        }

        #endregion
    }
}


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Data;
//using System.Data.SqlClient;
//using Entidades;
//using Utilidades;

//namespace Datos
//{
//    public class CuentaCorriente
//    {

//        SqlDataAdapter daCtaCte;
//        SqlCommand cmCtaCte;

//        Utilidades.Conexion conn;
//        private readonly IEmpresaContext _empresa;
//        public CuentaCorriente(IEmpresaContext empresa)
//        {
//            _empresa = empresa ??
//                throw new ArgumentNullException(nameof(empresa));

//            conn = new Utilidades.Conexion();
//        }


//        public DataTable obtenerCtasCtes(string txtBusqueda, int? idPersona)
//        {
//            DataTable dtCtasCtes = new DataTable();
//            string where = idPersona != null && idPersona != 0 ?
//                                "Where  dbo.Personas.idPersona = " + idPersona : 
//                                "Where  dbo.Personas.identificacion like '%" + txtBusqueda + "%' OR dbo.Personas.razonSocial like '%" + txtBusqueda + "%' ";
//            string consulta = "SELECT dbo.Personas.idPersona as IdPersona, dbo.Personas.identificacion [Nombre Identif.], dbo.Personas.razonSocial AS [Razon Social], SUM(dbo.MovCtaCte.importe) AS Saldo " +
//                                "FROM dbo.Personas INNER JOIN dbo.MovCtaCte ON dbo.Personas.idPersona = dbo.MovCtaCte.idPersona "+
//                                where +
//                                "GROUP BY dbo.Personas.idPersona, dbo.Personas.identificacion, dbo.Personas.razonSocial";
//            daCtaCte = new SqlDataAdapter(consulta, conn.conectar(_empresa));
//            daCtaCte.Fill(dtCtasCtes);

//            return dtCtasCtes;
//        }

//        public DataTable getCtaCteByIdPersona(int idPersona, DateTime fechaDesde)
//        {
//            DataTable dtMovCtaCte = new DataTable();
//            daCtaCte = new SqlDataAdapter();

//            cmCtaCte = new SqlCommand();
//            cmCtaCte.Connection = conn.conectar(_empresa);
//            cmCtaCte.Connection.Open();
//            cmCtaCte.CommandType = CommandType.StoredProcedure; cmCtaCte.CommandTimeout = conn.TimeOut();
//            cmCtaCte.CommandText = "getCtaCteByIdPersona";
//            cmCtaCte.Parameters.AddWithValue("@idPersona", idPersona);
//            cmCtaCte.Parameters.AddWithValue("@fechaDesde", fechaDesde);

//            daCtaCte.SelectCommand = cmCtaCte;
//            daCtaCte.Fill(dtMovCtaCte);

//            cmCtaCte.Connection.Close();

//            return dtMovCtaCte;
//        }

//        public Entidades.MovCtaCte getMovCtaCteBy(int id, Entidades.MovCtaCte.tablas tabla, int idTabla, Entidades.MovCtaCte.getBy getBy)
//        {
//            Entidades.MovCtaCte oMovCtaCteE = null;
//            int idPersona = 0, idSucursal = 0, idCreadoPor = 0, idModifPor = 0;
//            string commandText = "";
//            if (getBy.Equals(Entidades.MovCtaCte.getBy.Id))
//            {
//                commandText = "Select top 1 MovCtaCte.* from MovCtaCte where id = " + id + " order by id desc";
//            }
//            if (getBy.Equals(Entidades.MovCtaCte.getBy.TablaAndId))
//            {
//                commandText = "Select top 1 MovCtaCte.* from MovCtaCte where tabla = \'" + tabla.ToString() + "\' and idTabla = " + idTabla + " order by id desc";
//            }

//            using (SqlConnection conn = this.conn.conectar(_empresa))
//            using (SqlCommand cmd = new SqlCommand(commandText, conn))
//            {
//                conn.Open();
//                using (SqlDataReader drMovCtaCte = cmd.ExecuteReader())
//                {
//                    while (drMovCtaCte.Read())
//                    {
//                        oMovCtaCteE = new Entidades.MovCtaCte();

//                        oMovCtaCteE.Id = Convert.ToInt32(drMovCtaCte["id"]);
//                        idPersona = Convert.ToInt32(drMovCtaCte["idPersona"]);

//                        oMovCtaCteE.Fecha = Convert.ToDateTime(drMovCtaCte["fecha"]);
//                        oMovCtaCteE.Tabla = Convert.ToString(drMovCtaCte["tabla"]);
//                        oMovCtaCteE.IdTabla = Convert.ToInt32(drMovCtaCte["idTabla"]);
//                        oMovCtaCteE.NroDoc = Convert.ToString(drMovCtaCte["nroDoc"]);
//                        oMovCtaCteE.Detalle = Convert.ToString(drMovCtaCte["detalle"]);
//                        oMovCtaCteE.Tipo = Convert.ToString(drMovCtaCte["tipo"]);
//                        oMovCtaCteE.Importe = float.Parse(drMovCtaCte["importe"].ToString());
//                        idSucursal = Convert.ToInt32(drMovCtaCte["idSucursal"]);
//                        oMovCtaCteE.QuitadoCtaCta = drMovCtaCte["quitadoCtaCte"].Equals(DBNull.Value) ? false : Convert.ToBoolean(drMovCtaCte["quitadoCtaCte"]);
//                        oMovCtaCteE.Creado = Convert.ToDateTime(drMovCtaCte["creado"]);
//                        oMovCtaCteE.Actualizado = drMovCtaCte["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drMovCtaCte["actualizado"]);
//                        idCreadoPor = Convert.ToInt32(drMovCtaCte["creadoPor"]);
//                        idModifPor = drMovCtaCte["actualizadoPor"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(drMovCtaCte["actualizadoPor"]);                    
//                    }
//                }
//            }

//            if (oMovCtaCteE == null)
//                return null;

//            Datos.Usuario oUsuarioD = new Usuario(_empresa);
//            oMovCtaCteE.CreadoPor = idCreadoPor.Equals(DBNull.Value) ? null : oUsuarioD.getUsuarioById(idCreadoPor);
//            oMovCtaCteE.ActualizadoPor = idModifPor.Equals(DBNull.Value) ? null : oUsuarioD.getUsuarioById(idModifPor);

//            Datos.Sucursal oSucursalD = new Sucursal(_empresa); 
//            oMovCtaCteE.Sucursal = oSucursalD.findById(idSucursal);

//            Datos.Persona oPersonaD = new Datos.Persona(_empresa);
//            oMovCtaCteE.Persona = oPersonaD.findById(idPersona);            

//            return oMovCtaCteE; 
//        }

//        public Entidades.MovCtaCte addOrEditMovCtaCte(Entidades.MovCtaCte oMovCtaCteE)
//        {
//            // Truncar a segundos la fecha antes de guardarla
//            oMovCtaCteE.Fecha = new DateTime(
//                oMovCtaCteE.Fecha.Year,
//                oMovCtaCteE.Fecha.Month,
//                oMovCtaCteE.Fecha.Day,
//                oMovCtaCteE.Fecha.Hour,
//                oMovCtaCteE.Fecha.Minute,
//                oMovCtaCteE.Fecha.Second
//            );

//            cmCtaCte = new SqlCommand();
//            cmCtaCte.Connection = conn.conectar(_empresa);
//            cmCtaCte.CommandType = CommandType.StoredProcedure;
//            cmCtaCte.CommandText = "addOrEditMovCtaCte";

//            cmCtaCte.Parameters.AddWithValue("@id", oMovCtaCteE.Id);
//            cmCtaCte.Parameters.AddWithValue("@idPersona", oMovCtaCteE.Persona.idPersona);
//            cmCtaCte.Parameters.AddWithValue("@fecha", oMovCtaCteE.Fecha);
//            cmCtaCte.Parameters.AddWithValue("@tabla", oMovCtaCteE.Tabla);
//            cmCtaCte.Parameters.AddWithValue("@idTabla", oMovCtaCteE.IdTabla);
//            cmCtaCte.Parameters.AddWithValue("@nroDoc", oMovCtaCteE.NroDoc);
//            cmCtaCte.Parameters.AddWithValue("@detalle", oMovCtaCteE.Detalle);
//            cmCtaCte.Parameters.AddWithValue("@tipo", oMovCtaCteE.Tipo);
//            cmCtaCte.Parameters.AddWithValue("@importe", oMovCtaCteE.Importe);
//            cmCtaCte.Parameters.AddWithValue("@quitadoCtaCte", oMovCtaCteE.QuitadoCtaCta);
//            cmCtaCte.Parameters.AddWithValue("@idSucursal", oMovCtaCteE.Sucursal.idSucursal);
//            cmCtaCte.Parameters.AddWithValue("@creadoPor", oMovCtaCteE.CreadoPor.Id);
//            cmCtaCte.Parameters.AddWithValue("@actualizadoPor", oMovCtaCteE.ActualizadoPor != null ? oMovCtaCteE.ActualizadoPor.Id : -1);

//            cmCtaCte.Connection.Open();
//            oMovCtaCteE.Id = (int)cmCtaCte.ExecuteScalar();
//            cmCtaCte.Connection.Close();

//            return oMovCtaCteE;
//        }

//        #region Cheques

//        public DataTable obtenerCheques(string texto, DateTime fechaDesde, DateTime fechaHasta, bool soloPropios, string estado)
//        {
//            DataTable dtCheques = new DataTable();
//            daCtaCte = new SqlDataAdapter();

//            cmCtaCte = new SqlCommand();
//            cmCtaCte.Connection = conn.conectar(_empresa);
//            cmCtaCte.CommandType = CommandType.Text;
//            cmCtaCte.CommandText = "SELECT dbo.Cheques.id, dbo.Cheques.nroCheque, dbo.Cheques.banco, dbo.Cheques.propio, " +
//                " CASE dbo.Cheques.propio WHEN 1 THEN 'Propio' WHEN 0 THEN '3ro' END AS Origen, "+
//                " dbo.Cheques.fechaEmision, dbo.Cheques.fechaPago, dbo.Cheques.importe, dbo.Cheques.estado, dbo.Cheques.recibidoDe, " +
//                " RecibidoPor.identificacion AS Recibido_De, dbo.Cheques.entregadoA, EntregadoPor.identificacion AS Entregado_A,  " +
//                " CASE WHEN LEN(dbo.Cheques.observaciones) > 30  THEN LEFT(dbo.Cheques.observaciones, 30) + '...' ELSE dbo.Cheques.observaciones  END AS 'obs.'," +
//                " dbo.Cheques.creado, CreadoPor.nombre AS CreadoPor, dbo.Cheques.actualizado,  ActualizadoPor.nombre AS ActualizadoPor " +
//                " FROM     dbo.Pagos AS PagoEntregado INNER JOIN " +
//                "  dbo.Personas AS EntregadoPor ON PagoEntregado.idPersona = EntregadoPor.idPersona RIGHT OUTER JOIN " +
//                " dbo.Cheques ON PagoEntregado.id = dbo.Cheques.entregadoA LEFT OUTER JOIN " +
//                " dbo.Personas AS RecibidoPor INNER JOIN " +
//                " dbo.Pagos AS PagoRecibido ON RecibidoPor.idPersona = PagoRecibido.idPersona ON dbo.Cheques.recibidoDe = PagoRecibido.id LEFT OUTER JOIN " +
//                " dbo.Usuarios AS ActualizadoPor ON dbo.Cheques.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN " +
//                " dbo.Usuarios AS CreadoPor ON dbo.Cheques.creadoPor = CreadoPor.id " +
//                " WHERE dbo.Cheques.fechaPago between @fechaDesde and @fechaHasta" +
//                " and dbo.Cheques.nroCheque like '%" + texto + "%' AND dbo.Cheques.estado like '%" + estado + "%' AND ((@soloPropios = 1 AND propio = 1) OR (@soloPropios = 0 AND (propio = 1 or propio = 0))) " +
//                " ORDER BY dbo.Cheques.id DESC";

//            cmCtaCte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmCtaCte.Parameters.AddWithValue("@fechaHasta", fechaHasta.AddDays(1));
//            cmCtaCte.Parameters.AddWithValue("@soloPropios", soloPropios ? 1 : 0);

//            daCtaCte.SelectCommand = cmCtaCte;
//            daCtaCte.Fill(dtCheques);

//            cmCtaCte.Connection.Close();

//            return dtCheques;
//        }
//        public Cheque getChequePorIDorNro(int id, string nroCheque)
//        {
//            Cheque cheque = null;

//            // Conexión a la base de datos
//            using (SqlConnection connection = conn.conectar(_empresa))
//            {

//                string query = "SELECT * FROM Cheques WHERE id = @id";

//                if (!string.IsNullOrEmpty(nroCheque))
//                    //se obtiene el ultimo cheque cargado, en caso q haya dos nros de cheques iguales
//                    query = "SELECT TOP 1 * FROM Cheques WHERE nroCheque = @nroCheque order by id desc";

//                using (SqlCommand cmd = new SqlCommand(query, connection))
//                {
//                    cmd.Parameters.AddWithValue("@id", id);
//                    cmd.Parameters.AddWithValue("@nroCheque", nroCheque);
//                    connection.Open();

//                    using (SqlDataReader reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            Datos.Usuario oUserD = new Usuario(_empresa);
//                            cheque = new Cheque
//                            {
//                                Id = reader.GetInt32(reader.GetOrdinal("id")),
//                                NroCheque = reader["nroCheque"].ToString(),
//                                Banco = reader["banco"].ToString(),
//                                Propio = Convert.ToBoolean(reader["propio"]),
//                                FechaEmision = reader["fechaEmision"].ToString(),
//                                FechaPago = Convert.ToDateTime(reader["fechaPago"]),
//                                Importe = Convert.ToDouble(reader["importe"]),
//                                Estado = reader["estado"].ToString(),
//                                Titular = reader["titular"].ToString(),
//                                Observaciones = reader["observaciones"].ToString(),
//                                RecibidoDe = Convert.ToInt32(reader["recibidoDe"]),
//                                EntregadoA = Convert.ToInt32(reader["entregadoA"]),
//                                PagoDe = Convert.ToInt32(reader["recibidoDe"]) > 0 ? getPagoById(Convert.ToInt32(reader["recibidoDe"])) : null,
//                                PagoA = Convert.ToInt32(reader["entregadoA"]) > 0 ? getPagoById(Convert.ToInt32(reader["entregadoA"])) : null,
//                                Creado = Convert.ToDateTime(reader["creado"]),
//                                CreadoPor = Convert.ToInt32(reader["creadoPor"]) > 0 ? oUserD.getUsuarioById(Convert.ToInt32(reader["creadoPor"])) : null,
//                                Actualizado = reader["actualizado"] != DBNull.Value ? Convert.ToDateTime(reader["actualizado"]) : (DateTime?)null,
//                                ActualizadoPor = Convert.ToInt32(reader["actualizadoPor"]) > 0 ? oUserD.getUsuarioById(Convert.ToInt32(reader["creadoPor"])) : null,
//                            };
//                        }
//                    }
//                }
//            }

//            return cheque;
//        }


//        /// <summary>
//        /// Recupera los datos del Cheque segun ID - conPagos debe ser falso si se llama desde Pagos para evitar bucle
//        /// </summary>
//        /// <param name="idPago"></param>
//        /// <param name="conPagos"></param>
//        /// <returns></returns>
//        public List<Entidades.Cheque> getChequesPorPago(int idPago, bool conPagos = true)
//        {
//            var listCheques = new List<Cheque>();
//            if (idPago == 0)
//                return listCheques;

//            using (SqlConnection connection = conn.conectar(_empresa))
//            using (SqlCommand cmd = new SqlCommand(
//                "SELECT * FROM Cheques WHERE recibidoDe = @idPago OR entregadoA = @idPago", connection))
//            {
//                cmd.Parameters.AddWithValue("@idPago", idPago);
//                connection.Open();

//                using (SqlDataReader reader = cmd.ExecuteReader())
//                {
//                    while (reader.Read())
//                    {
//                        // cachear valores que se usan más de una vez
//                        int idRecibidoDe = reader["recibidoDe"] != DBNull.Value ? Convert.ToInt32(reader["recibidoDe"]) : 0;
//                        int idEntregadoA = reader["entregadoA"] != DBNull.Value ? Convert.ToInt32(reader["entregadoA"]) : 0;
//                        int idCreadoPor = reader["creadoPor"] != DBNull.Value ? Convert.ToInt32(reader["creadoPor"]) : 0;
//                        int idActualizadoPor = reader["actualizadoPor"] != DBNull.Value ? Convert.ToInt32(reader["actualizadoPor"]) : 0;

//                        var cheque = new Cheque
//                        {
//                            Id = Convert.ToInt32(reader["id"]),
//                            NroCheque = reader["nroCheque"].ToString(),
//                            Banco = reader["banco"].ToString(),
//                            Propio = Convert.ToBoolean(reader["propio"]),
//                            FechaEmision = reader["fechaEmision"].ToString(),
//                            FechaPago = Convert.ToDateTime(reader["fechaPago"]),
//                            Importe = Convert.ToDouble(reader["importe"]),
//                            Estado = reader["estado"].ToString(),
//                            Titular = reader["titular"].ToString(),
//                            Observaciones = reader["observaciones"].ToString(),
//                            RecibidoDe = idRecibidoDe,
//                            EntregadoA = idEntregadoA,
//                            Creado = Convert.ToDateTime(reader["creado"]),
//                            Actualizado = reader["actualizado"] != DBNull.Value ? Convert.ToDateTime(reader["actualizado"]) : (DateTime?)null,

//                            IdCreadoPor = Convert.ToInt32(reader["creadoPor"]),
//                            IdActualizadoPor = reader["actualizadoPor"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["actualizadoPor"])
//                        };

//                        listCheques.Add(cheque);
//                    }
//                }
//            }

//            var oUserD = new Usuario(_empresa); // reutilizamos el mismo objeto
//            for (int i = 0; i < listCheques.Count; i++)
//            {
//                listCheques[i].PagoDe = listCheques[i].RecibidoDe > 0 ? getPagoById(listCheques[i].RecibidoDe, conPagos) : null;
//                listCheques[i].PagoA = listCheques[i].EntregadoA > 0 ? getPagoById(listCheques[i].EntregadoA, conPagos) : null;
//                listCheques[i].CreadoPor = listCheques[i].IdCreadoPor > 0 ? oUserD.getUsuarioById(listCheques[i].IdCreadoPor) : null;
//                if (listCheques[i].IdActualizadoPor.HasValue)
//                    listCheques[i].ActualizadoPor = oUserD.getUsuarioById(Convert.ToInt32(listCheques[i].IdActualizadoPor));
//            }

//            return listCheques;

//            //if (idPago < 1)
//            //{
//            //    Cheque cheque = null;
//            //    List<Cheque> listCheques = new List<Cheque>();
//            //    if (idPago == 0)
//            //        return listCheques;

//            //    // Conexión a la base de datos
//            //    using (SqlConnection connection = conn.conectar(_empresa))
//            //    {

//            //        string query = "SELECT * FROM Cheques WHERE recibidoDe = @idPago OR entregadoA = @idPago";

//            //        using (SqlCommand cmd = new SqlCommand(query, connection))
//            //        {
//            //            cmd.Parameters.AddWithValue("@idPago", idPago);
//            //            connection.Open();

//            //            Datos.Usuario oUserD = new Usuario();
//            //            using (SqlDataReader reader = cmd.ExecuteReader())
//            //            {
//            //                while (reader.Read())
//            //                {
//            //                    cheque = new Cheque
//            //                    {
//            //                        Id = reader.GetInt32(reader.GetOrdinal("id")),
//            //                        NroCheque = reader["nroCheque"].ToString(),
//            //                        Banco = reader["banco"].ToString(),
//            //                        Propio = Convert.ToBoolean(reader["propio"]),
//            //                        FechaEmision = reader["fechaEmision"].ToString(),
//            //                        FechaPago = Convert.ToDateTime(reader["fechaPago"]),
//            //                        Importe = Convert.ToDouble(reader["importe"]),
//            //                        Estado = reader["estado"].ToString(),
//            //                        Titular = reader["titular"].ToString(),
//            //                        Observaciones = reader["observaciones"].ToString(),
//            //                        RecibidoDe = Convert.ToInt32(reader["recibidoDe"]),
//            //                        EntregadoA = Convert.ToInt32(reader["entregadoA"]),
//            //                        ///se comenta estas lineas xq sino entre en bucle
//            //                        PagoDe = Convert.ToInt32(reader["recibidoDe"]) > 0 ? getPagoById(Convert.ToInt32(reader["recibidoDe"])) : null,
//            //                        PagoA = Convert.ToInt32(reader["entregadoA"]) > 0 ? getPagoById(Convert.ToInt32(reader["entregadoA"])) : null,
//            //                        Creado = Convert.ToDateTime(reader["creado"]),
//            //                        CreadoPor = Convert.ToInt32(reader["creadoPor"]) > 0 ? oUserD.getUsuarioById(Convert.ToInt32(reader["creadoPor"])) : null,
//            //                        Actualizado = reader["actualizado"] != DBNull.Value ? Convert.ToDateTime(reader["actualizado"]) : (DateTime?)null,
//            //                        ActualizadoPor = Convert.ToInt32(reader["actualizadoPor"]) > 0 ? oUserD.getUsuarioById(Convert.ToInt32(reader["creadoPor"])) : null,
//            //                    };
//            //                    listCheques.Add(cheque);
//            //                }
//            //            }
//            //        }
//            //    }

//            //    return listCheques;
//            //}
//        }

//        public bool AddOrEditCheque(Cheque oCheque)
//        {
//            // Conexión a la base de datos
//            using (SqlConnection connection = conn.conectar(_empresa))
//            {
//                string query;

//                if (oCheque.Id == 0)
//                {
//                    query = @"INSERT INTO Cheques (
//                        nroCheque, banco, propio, fechaEmision, fechaPago,
//                        importe, estado, titular, observaciones, recibidoDe,
//                        entregadoA, creado, creadoPor, actualizado, actualizadoPor
//                      ) VALUES (
//                        @nroCheque, @banco, @propio, @fechaEmision, @fechaPago,
//                        @importe, @estado, @titular, @observaciones, @recibidoDe,
//                        @entregadoA, @creado, @creadoPor, @actualizado, @actualizadoPor
//                      )";
//                }
//                else
//                {
//                    query = @"UPDATE Cheques SET
//                        nroCheque = @nroCheque,
//                        banco = @banco,
//                        propio = @propio,
//                        fechaEmision = @fechaEmision,
//                        fechaPago = @fechaPago,
//                        importe = @importe,
//                        estado = @estado,
//                        titular = @titular,
//                        observaciones = @observaciones,
//                        recibidoDe = @recibidoDe,
//                        entregadoA = @entregadoA,
//                        creado = @creado,
//                        creadoPor = @creadoPor,
//                        actualizado = @actualizado,
//                        actualizadoPor = @actualizadoPor
//                      WHERE id = @id";
//                }

//                using (SqlCommand cmd = new SqlCommand(query, connection))
//                {
//                    // Parámetros
//                    cmd.Parameters.AddWithValue("@nroCheque", oCheque.NroCheque ?? "");
//                    cmd.Parameters.AddWithValue("@banco", oCheque.Banco ?? "");
//                    cmd.Parameters.AddWithValue("@propio", oCheque.Propio ? 1 : 0);
//                    cmd.Parameters.AddWithValue("@fechaEmision", oCheque.FechaEmision ?? "");
//                    cmd.Parameters.AddWithValue("@fechaPago", oCheque.FechaPago);
//                    cmd.Parameters.AddWithValue("@importe", oCheque.Importe);
//                    cmd.Parameters.AddWithValue("@estado", oCheque.Estado ?? "");
//                    cmd.Parameters.AddWithValue("@titular", oCheque.Titular ?? "");
//                    cmd.Parameters.AddWithValue("@observaciones", oCheque.Observaciones ?? "");
//                    cmd.Parameters.AddWithValue("@recibidoDe", oCheque.RecibidoDe);
//                    cmd.Parameters.AddWithValue("@entregadoA", oCheque.EntregadoA);
//                    cmd.Parameters.AddWithValue("@creado", oCheque.Creado ?? DateTime.Now);
//                    cmd.Parameters.AddWithValue("@creadoPor", oCheque.CreadoPor.Id);
//                    cmd.Parameters.AddWithValue("@actualizado", (object)oCheque.Actualizado ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@actualizadoPor", oCheque.ActualizadoPor != null ? oCheque.ActualizadoPor.Id : 0);

//                    if (oCheque.Id != 0)
//                        cmd.Parameters.AddWithValue("@id", oCheque.Id);

//                    connection.Open();
//                    int result = cmd.ExecuteNonQuery();
//                    return result > 0;
//                }
//            }
//        }

//        public bool EliminarCheque(int id)
//        {
//            using (SqlConnection connection = conn.conectar(_empresa))
//            {
//                string query = "DELETE FROM Cheques WHERE id = @id";
//                using (SqlCommand cmd = new SqlCommand(query, connection))
//                {
//                    cmd.Parameters.AddWithValue("@id", id);
//                    connection.Open();
//                    int rowsAffected = cmd.ExecuteNonQuery();
//                    return rowsAffected > 0;
//                }
//            }
//        }

//        public bool resetearChequesAsignados(int idPago)
//        {
//            using (SqlConnection connection = conn.conectar(_empresa))
//            {
//                string query = "UPDATE Cheques SET recibidoDe = 0 FROM Cheques WHERE recibidoDe = @idPago;"+
//                     "UPDATE Cheques SET entregadoA = 0, estado = @estadoReset FROM Cheques WHERE entregadoA = @idPago;";
//                // "(UPDATE Cheques SET recibidoDe = 0, entregadoA = 0, estado = PENDIENTE FROM Cheques WHERE recibidoDe = @idPago OR entregadoA = @idPago)" ;
//                using (SqlCommand cmd = new SqlCommand(query, connection))
//                {
//                    cmd.Parameters.AddWithValue("@idPago", idPago);
//                    cmd.Parameters.AddWithValue("@estadoReset", "PENDIENTE");// Entidades.Cheque.EstadoEnum.PENDIENTE.ToString());
//                    connection.Open();
//                    int rowsAffected = cmd.ExecuteNonQuery();
//                    return rowsAffected > 0;
//                }
//            }
//        }

//        public List<string> getBancos()
//        {
//            List<string> bancos = new List<string>();
//            using (SqlConnection connection = conn.conectar(_empresa))
//            {
//                string query = "SELECT banco FROM Bancos";
//                using (SqlCommand cmd = new SqlCommand(query, connection))
//                {
//                    connection.Open();
//                    SqlDataReader reader = cmd.ExecuteReader();

//                    while (reader.Read())
//                    {
//                        bancos.Add(reader["banco"].ToString().Trim());
//                    }
//                }
//            }
//            return bancos;
//        }

//        #endregion

//        #region Pagos

//        public int getUltimoIdPago()
//        {
//            cmCtaCte = new SqlCommand();
//            cmCtaCte.Connection = conn.conectar(_empresa);
//            cmCtaCte.CommandType = CommandType.Text;
//            cmCtaCte.CommandText = "Select top 1 id from Pagos order by id desc";

//            cmCtaCte.Connection.Open();
//            int idPago = (int)cmCtaCte.ExecuteScalar();
//            cmCtaCte.Connection.Close();

//            return idPago;
//        }

//        public Entidades.Pago addOrEditPago(Entidades.Pago oPagoE)
//        {
//            cmCtaCte = new SqlCommand();
//            cmCtaCte.Connection = conn.conectar(_empresa);
//            cmCtaCte.CommandType = CommandType.StoredProcedure;
//            cmCtaCte.CommandText = "addOrEditPago";

//            cmCtaCte.Parameters.AddWithValue("@id", oPagoE.Id);
//            cmCtaCte.Parameters.AddWithValue("@nroRecibo", oPagoE.NroRecibo);
//            cmCtaCte.Parameters.AddWithValue("@fecha", oPagoE.Fecha);
//            cmCtaCte.Parameters.AddWithValue("@idPersona", oPagoE.Persona.idPersona);
//            cmCtaCte.Parameters.AddWithValue("@aProveedor", oPagoE.AProveedor);
//            cmCtaCte.Parameters.AddWithValue("@formaPago", oPagoE.FormaPago);
//            cmCtaCte.Parameters.AddWithValue("@banco", oPagoE.Banco);
//            cmCtaCte.Parameters.AddWithValue("@nroCheque", oPagoE.NroCheque);
//            cmCtaCte.Parameters.AddWithValue("@titularCheque", oPagoE.TitularCheque);
//            cmCtaCte.Parameters.AddWithValue("@importe", oPagoE.Importe);
//            cmCtaCte.Parameters.AddWithValue("@efectivo", oPagoE.Efectivo);
//            cmCtaCte.Parameters.AddWithValue("@observaciones", oPagoE.Observaciones);
//            cmCtaCte.Parameters.AddWithValue("@idSucursal", oPagoE.Sucursal.idSucursal);
//            cmCtaCte.Parameters.AddWithValue("@creadoPor", oPagoE.CreadoPor.Id);
//            cmCtaCte.Parameters.AddWithValue("@actualizadoPor", oPagoE.ActualizadoPor != null ? oPagoE.ActualizadoPor.Id : 0);

//            cmCtaCte.Connection.Open();
//            oPagoE.Id = (int)cmCtaCte.ExecuteScalar();
//            cmCtaCte.Connection.Close();

//            ///Asigno el pago a los cheques,

//            foreach (Entidades.Cheque item in oPagoE.Cheques)
//            {
//                //si se entregó el cheque
//                if (oPagoE.AProveedor)
//                {
//                    item.EntregadoA = oPagoE.Id;
//                    item.Estado = Entidades.Cheque.EstadoEnum.ENTREGADO.ToString();
//                }
//                else //de quien se recibió
//                    item.RecibidoDe = oPagoE.Id;

//                AddOrEditCheque(item);
//            }

//            return oPagoE;
//        }



//        public void eliminarPago(Entidades.Pago oPagoE)
//        {
//            cmCtaCte = new SqlCommand();

//            cmCtaCte.Connection = conn.conectar(_empresa);
//            cmCtaCte.Connection.Open();
//            cmCtaCte.CommandType = CommandType.StoredProcedure;
//            cmCtaCte.CommandText = "eliminarPago";

//            cmCtaCte.Parameters.AddWithValue("@Id", oPagoE.Id);

//            cmCtaCte.ExecuteNonQuery();
//            cmCtaCte.Connection.Close();

//            cmCtaCte = null;
//        }

//        public DataTable obtenerPagos(string texto, DateTime fechaDesde, DateTime fechaHasta)
//        {
//            DataTable dtPagos = new DataTable();
//            daCtaCte = new SqlDataAdapter();

//            cmCtaCte = new SqlCommand();
//            cmCtaCte.Connection = conn.conectar(_empresa);
//            cmCtaCte.CommandType = CommandType.Text;
//            cmCtaCte.CommandText = "SELECT     dbo.Pagos.id, dbo.Pagos.fecha, dbo.Personas.razonSocial, " +
//                " dbo.Pagos.nroRecibo, dbo.Pagos.importe,dbo.Pagos.aProveedor, CASE dbo.Pagos.aProveedor WHEN 0 THEN 'Cobro' WHEN 1 THEN 'Pago' END AS Operacion, "+
//                "dbo.Pagos.formaPago, dbo.Pagos.efectivo, dbo.Pagos.observaciones, dbo.Pagos.creado, CreadoPor.nombre AS CreadoPor, " +
//                " dbo.Pagos.actualizado, ActualizadoPor.nombre AS ActualizadoPor " +
//                " FROM  dbo.Pagos INNER JOIN dbo.Personas ON dbo.Pagos.idPersona = dbo.Personas.idPersona LEFT OUTER JOIN " +
//                " dbo.Usuarios AS ActualizadoPor ON dbo.Pagos.creadoPor = ActualizadoPor.id LEFT OUTER JOIN " +
//                " dbo.Usuarios AS CreadoPor ON dbo.Pagos.actualizadoPor = CreadoPor.id " +
//               // " WHERE dbo.Pagos.fecha between '" + fechaDesde + "' and '" + fechaHasta.AddDays(1) + "'" +
//                " WHERE dbo.Pagos.fecha between @fechaDesde and @fechaHasta" +
//                " and (dbo.Personas.razonSocial like '%" + texto + "%' or dbo.Pagos.nroRecibo like '%" + texto + "%')"+
//                " ORDER BY dbo.Pagos.fecha DESC";

//            cmCtaCte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmCtaCte.Parameters.AddWithValue("@fechaHasta", fechaHasta.AddDays(1));

//            daCtaCte.SelectCommand = cmCtaCte;
//            daCtaCte.Fill(dtPagos);

//            cmCtaCte.Connection.Close();

//            return dtPagos;
//        }

//        public Entidades.Pago getPagoById(int idPago, bool conCheques = true)
//        {
//            Entidades.Pago oPagoE = null;

//            using (SqlConnection connection = conn.conectar(_empresa))
//            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Pagos WHERE id = @id", connection))
//            {
//                cmd.Parameters.AddWithValue("@id", idPago);
//                connection.Open();

//                using (SqlDataReader drPago = cmd.ExecuteReader())
//                {
//                    if (drPago.Read())
//                    {
//                        // 1) Cargo solo lo básico en memoria
//                        oPagoE = new Entidades.Pago
//                        {
//                            Id = Convert.ToInt32(drPago["id"]),
//                            IdPersona = Convert.ToInt32(drPago["idPersona"]),
//                            Fecha = Convert.ToDateTime(drPago["fecha"]),
//                            NroRecibo = Convert.ToString(drPago["nroRecibo"]),
//                            AProveedor = drPago["aProveedor"] != DBNull.Value && Convert.ToBoolean(drPago["aProveedor"]),
//                            //11/12/2025 cambie la forma de pago Eftvo+Cheque por EftvoCheque
//                            FormaPago = Convert.ToString(drPago["formaPago"]).Equals("Eftvo+Cheque") ? 
//                                            "EftvoCheque" : Convert.ToString(drPago["formaPago"]),
//                            Banco = Convert.ToString(drPago["banco"]),
//                            NroCheque = Convert.ToString(drPago["nroCheque"]),
//                            TitularCheque = Convert.ToString(drPago["titularCheque"]),
//                            Importe = float.Parse(drPago["importe"].ToString()),
//                            Efectivo = float.Parse(drPago["efectivo"].ToString()),
//                            Observaciones = Convert.ToString(drPago["observaciones"]),
//                            IdSucursal = Convert.ToInt32(drPago["idSucursal"]),
//                            Creado = Convert.ToDateTime(drPago["creado"]),
//                            Actualizado = drPago["actualizado"] == DBNull.Value ? null : (DateTime?)drPago["actualizado"],
//                            IdCreadoPor = Convert.ToInt32(drPago["creadoPor"]),
//                            IdActualizadoPor = drPago["actualizadoPor"] == DBNull.Value ? (int?)null : Convert.ToInt32(drPago["actualizadoPor"])
//                        };
//                    }
//                }
//            }

//            if (oPagoE == null)
//                return null;

//            // 2) Ya no hay DataReader abierto → ahora puedo llamar a otros métodos sin conflicto
//            Datos.Persona oPersonaD = new Datos.Persona(_empresa);
//            oPagoE.Persona = oPersonaD.findById(oPagoE.IdPersona);

//            if (conCheques)
//                oPagoE.Cheques = getChequesPorPago(oPagoE.Id, false);

//            Datos.Sucursal oSucursalD = new Datos.Sucursal(_empresa);
//            oPagoE.Sucursal = oSucursalD.findById(oPagoE.IdSucursal);

//            Datos.Usuario oUsuarioD = new Datos.Usuario(_empresa);
//            oPagoE.CreadoPor = oUsuarioD.getUsuarioById(oPagoE.IdCreadoPor);
//            if (oPagoE.IdActualizadoPor.HasValue)
//                oPagoE.ActualizadoPor = oUsuarioD.getUsuarioById(oPagoE.IdActualizadoPor.Value);

//            return oPagoE;

//            //if (idPago > 0)
//            //{
//            //    cmCtaCte = new SqlCommand();
//            //    cmCtaCte.Connection = conn.conectar(_empresa);
//            //    cmCtaCte.CommandType = CommandType.Text;
//            //    cmCtaCte.CommandText = "Select Pagos.* from Pagos where id = " + idPago;

//            //    Entidades.Pago oPagoE = new Entidades.Pago();
//            //    try
//            //    {
//            //        cmCtaCte.Connection.Open();
//            //        SqlDataReader drPago = cmCtaCte.ExecuteReader();
//            //        using (drPago)
//            //        {
//            //            while (drPago.Read())
//            //            {
//            //                oPagoE.Id = Convert.ToInt32(drPago["id"]);
//            //                Datos.Persona oPersonaD = new Datos.Persona(_empresa);
//            //                oPagoE.Persona = oPersonaD.findById(Convert.ToInt32(drPago["idPersona"]));

//            //                oPagoE.Fecha = Convert.ToDateTime(drPago["fecha"]);
//            //                oPagoE.NroRecibo = Convert.ToString(drPago["nroRecibo"]);
//            //                oPagoE.AProveedor = drPago["aProveedor"].Equals(DBNull.Value) ? false : Convert.ToBoolean(drPago["aProveedor"]);
//            //                oPagoE.FormaPago = Convert.ToString(drPago["formaPago"]);
//            //                oPagoE.Banco = Convert.ToString(drPago["banco"]);
//            //                oPagoE.NroCheque = Convert.ToString(drPago["nroCheque"]);
//            //                oPagoE.TitularCheque = Convert.ToString(drPago["titularCheque"]);
//            //                oPagoE.Importe = float.Parse(drPago["importe"].ToString());
//            //                oPagoE.Efectivo = float.Parse(drPago["efectivo"].ToString());
//            //                oPagoE.Observaciones = Convert.ToString(drPago["observaciones"]);
//            //                oPagoE.Cheques = getChequesPorPago(oPagoE.Id);

//            //                Datos.Sucursal oSucursalD = new Sucursal();
//            //                oPagoE.Sucursal = oSucursalD.findById(Convert.ToInt32(drPago["idSucursal"]));


//            //                oPagoE.Creado = Convert.ToDateTime(drPago["creado"]);
//            //                oPagoE.Actualizado = drPago["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drPago["actualizado"]);

//            //                Datos.Usuario oUsuarioD = new Usuario();
//            //                oPagoE.CreadoPor = oUsuarioD.getUsuarioById(Convert.ToInt32(drPago["creadoPor"]));
//            //                oPagoE.ActualizadoPor = drPago["actualizadoPor"].Equals(DBNull.Value) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drPago["actualizadoPor"]));
//            //            }
//            //            return oPagoE;
//            //        }
//            //    }
//            //    finally
//            //    {
//            //        cmCtaCte.Connection.Close();
//            //        oPagoE = null;
//            //    }
//            //}

//        }

//        #endregion

//    }
//}
