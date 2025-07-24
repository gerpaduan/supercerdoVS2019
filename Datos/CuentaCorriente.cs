using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class CuentaCorriente
    {
        Utilidades.Conexion conn = new Utilidades.Conexion();
        SqlDataAdapter daCtaCte;
        SqlCommand cmCtaCte;        

        public DataTable obtenerCtasCtes(string txtBusqueda)
        {
            DataTable dtCtasCtes = new DataTable();
            string consulta = "SELECT dbo.Personas.idPersona as IdPersona, dbo.Personas.identificacion [Nombre Identif.], dbo.Personas.razonSocial AS [Razon Social], SUM(dbo.MovCtaCte.importe) AS Saldo " +
                                "FROM dbo.Personas INNER JOIN dbo.MovCtaCte ON dbo.Personas.idPersona = dbo.MovCtaCte.idPersona "+
                                "Where  dbo.Personas.identificacion like '%" + txtBusqueda + "%' OR dbo.Personas.razonSocial like '%" + txtBusqueda + "%' " +
                                "GROUP BY dbo.Personas.idPersona, dbo.Personas.identificacion, dbo.Personas.razonSocial";
            daCtaCte = new SqlDataAdapter(consulta, conn.conectar());
            daCtaCte.Fill(dtCtasCtes);

            return dtCtasCtes;
        }

        public DataTable getCtaCteByIdPersona(int idPersona, DateTime fechaDesde)
        {
            DataTable dtMovCtaCte = new DataTable();
            daCtaCte = new SqlDataAdapter();

            cmCtaCte = new SqlCommand();
            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.Connection.Open();
            cmCtaCte.CommandType = CommandType.StoredProcedure; cmCtaCte.CommandTimeout = conn.TimeOut();
            cmCtaCte.CommandText = "getCtaCteByIdPersona";
            cmCtaCte.Parameters.AddWithValue("@idPersona", idPersona);
            cmCtaCte.Parameters.AddWithValue("@fechaDesde", fechaDesde);

            daCtaCte.SelectCommand = cmCtaCte;
            daCtaCte.Fill(dtMovCtaCte);

            cmCtaCte.Connection.Close();

            return dtMovCtaCte;
        }

        public Entidades.MovCtaCte getMovCtaCteBy(int id, Entidades.MovCtaCte.tablas tabla, int idTabla, Entidades.MovCtaCte.getBy getBy)
        {
	        cmCtaCte = new SqlCommand();
            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.CommandType = CommandType.Text;
            if (getBy.Equals(Entidades.MovCtaCte.getBy.Id))
            {
                cmCtaCte.CommandText = "Select top 1 MovCtaCte.* from MovCtaCte where id = "+id+" order by id desc";
            } 
            if (getBy.Equals(Entidades.MovCtaCte.getBy.TablaAndId))
            {
                cmCtaCte.CommandText = "Select top 1 MovCtaCte.* from MovCtaCte where tabla = \'" + tabla.ToString() + "\' and idTabla = " + idTabla + " order by id desc";
            }

            Entidades.MovCtaCte oMovCtaCteE = new Entidades.MovCtaCte();
            try
            {
	            cmCtaCte.Connection.Open();
                SqlDataReader drMovCtaCte = cmCtaCte.ExecuteReader();
                using (drMovCtaCte)
                {
	                while(drMovCtaCte.Read())
                    {
                        oMovCtaCteE.Id = Convert.ToInt32(drMovCtaCte["id"]);
                        Datos.Persona oPersonaD = new Datos.Persona();
                        oMovCtaCteE.Persona = oPersonaD.findById(Convert.ToInt32(drMovCtaCte["idPersona"]));

                        oMovCtaCteE.Fecha = Convert.ToDateTime(drMovCtaCte["fecha"]);
                        oMovCtaCteE.Tabla = Convert.ToString(drMovCtaCte["tabla"]);
                        oMovCtaCteE.IdTabla = Convert.ToInt32(drMovCtaCte["idTabla"]);
                        oMovCtaCteE.NroDoc = Convert.ToString(drMovCtaCte["nroDoc"]);
                        oMovCtaCteE.Detalle = Convert.ToString(drMovCtaCte["detalle"]);
                        oMovCtaCteE.Tipo = Convert.ToString(drMovCtaCte["tipo"]);
                        oMovCtaCteE.Importe = float.Parse(drMovCtaCte["importe"].ToString());

                        Datos.Sucursal oSucursalD = new Sucursal();
                        oMovCtaCteE.Sucursal = oSucursalD.findById(Convert.ToInt32(drMovCtaCte["idSucursal"]));

                        oMovCtaCteE.QuitadoCtaCta = drMovCtaCte["quitadoCtaCte"].Equals(DBNull.Value) ? false : Convert.ToBoolean(drMovCtaCte["quitadoCtaCte"]);

                        oMovCtaCteE.Creado = Convert.ToDateTime(drMovCtaCte["creado"]);
                        oMovCtaCteE.Actualizado = drMovCtaCte["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drMovCtaCte["actualizado"]);

                        Datos.Usuario oUsuarioD = new Usuario();
                        oMovCtaCteE.CreadoPor = oUsuarioD.getUsuarioById(Convert.ToInt32(drMovCtaCte["creadoPor"]));
                        oMovCtaCteE.ActualizadoPor = drMovCtaCte["actualizadoPor"].Equals(DBNull.Value) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drMovCtaCte["actualizadoPor"]));

                    }
                    return oMovCtaCteE;
                }
            }
            finally
            {
	            cmCtaCte.Connection.Close();
                oMovCtaCteE = null;
            }
        }

        public Entidades.MovCtaCte addOrEditMovCtaCte(Entidades.MovCtaCte oMovCtaCteE)
        {
            cmCtaCte = new SqlCommand();
            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.CommandType = CommandType.StoredProcedure;
            cmCtaCte.CommandText = "addOrEditMovCtaCte";

            cmCtaCte.Parameters.AddWithValue("@id", oMovCtaCteE.Id);
            cmCtaCte.Parameters.AddWithValue("@idPersona", oMovCtaCteE.Persona.idPersona);
            cmCtaCte.Parameters.AddWithValue("@fecha", oMovCtaCteE.Fecha);
            cmCtaCte.Parameters.AddWithValue("@tabla", oMovCtaCteE.Tabla);
            cmCtaCte.Parameters.AddWithValue("@idTabla", oMovCtaCteE.IdTabla);
            cmCtaCte.Parameters.AddWithValue("@nroDoc", oMovCtaCteE.NroDoc);
            cmCtaCte.Parameters.AddWithValue("@detalle", oMovCtaCteE.Detalle);
            cmCtaCte.Parameters.AddWithValue("@tipo", oMovCtaCteE.Tipo);
            cmCtaCte.Parameters.AddWithValue("@importe", oMovCtaCteE.Importe);
            cmCtaCte.Parameters.AddWithValue("@quitadoCtaCte", oMovCtaCteE.QuitadoCtaCta);
            cmCtaCte.Parameters.AddWithValue("@idSucursal", oMovCtaCteE.Sucursal.idSucursal);
            cmCtaCte.Parameters.AddWithValue("@creadoPor", oMovCtaCteE.CreadoPor.Id);
            cmCtaCte.Parameters.AddWithValue("@actualizadoPor", oMovCtaCteE.ActualizadoPor != null ? oMovCtaCteE.ActualizadoPor.Id : -1);

            cmCtaCte.Connection.Open();
            oMovCtaCteE.Id = (int)cmCtaCte.ExecuteScalar();
            cmCtaCte.Connection.Close();

            return oMovCtaCteE;
        }

        #region Cheques

        public DataTable obtenerCheques(string texto, DateTime fechaDesde, DateTime fechaHasta, bool soloPropios, string estado)
        {
            DataTable dtCheques = new DataTable();
            daCtaCte = new SqlDataAdapter();

            cmCtaCte = new SqlCommand();
            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.CommandType = CommandType.Text;
            cmCtaCte.CommandText = "SELECT dbo.Cheques.id, dbo.Cheques.nroCheque, dbo.Cheques.banco, dbo.Cheques.propio, " +
                " CASE dbo.Cheques.propio WHEN 1 THEN 'Propio' WHEN 0 THEN '3ro' END AS Origen, "+
                " dbo.Cheques.fechaEmision, dbo.Cheques.fechaPago, dbo.Cheques.importe, dbo.Cheques.estado, dbo.Cheques.recibidoDe, " +
                " RecibidoPor.identificacion AS Recibido_De, dbo.Cheques.entregadoA, EntregadoPor.identificacion AS Entregado_A,  " +
                " CASE WHEN LEN(dbo.Cheques.observaciones) > 30  THEN LEFT(dbo.Cheques.observaciones, 30) + '...' ELSE dbo.Cheques.observaciones  END AS 'obs.'," +
                " dbo.Cheques.creado, CreadoPor.nombre AS CreadoPor, dbo.Cheques.actualizado,  ActualizadoPor.nombre AS ActualizadoPor " +
                " FROM     dbo.Pagos AS PagoEntregado INNER JOIN " +
                "  dbo.Personas AS EntregadoPor ON PagoEntregado.idPersona = EntregadoPor.idPersona RIGHT OUTER JOIN " +
                " dbo.Cheques ON PagoEntregado.id = dbo.Cheques.entregadoA LEFT OUTER JOIN " +
                " dbo.Personas AS RecibidoPor INNER JOIN " +
                " dbo.Pagos AS PagoRecibido ON RecibidoPor.idPersona = PagoRecibido.idPersona ON dbo.Cheques.recibidoDe = PagoRecibido.id LEFT OUTER JOIN " +
                " dbo.Usuarios AS ActualizadoPor ON dbo.Cheques.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN " +
                " dbo.Usuarios AS CreadoPor ON dbo.Cheques.creadoPor = CreadoPor.id " +
                " WHERE dbo.Cheques.fechaPago between @fechaDesde and @fechaHasta" +
                " and dbo.Cheques.nroCheque like '%" + texto + "%' AND dbo.Cheques.estado like '%" + estado + "%' AND ((@soloPropios = 1 AND propio = 1) OR (@soloPropios = 0 AND (propio = 1 or propio = 0))) " +
                " ORDER BY dbo.Cheques.id DESC";

            cmCtaCte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCtaCte.Parameters.AddWithValue("@fechaHasta", fechaHasta.AddDays(1));
            cmCtaCte.Parameters.AddWithValue("@soloPropios", soloPropios ? 1 : 0);

            daCtaCte.SelectCommand = cmCtaCte;
            daCtaCte.Fill(dtCheques);

            cmCtaCte.Connection.Close();

            return dtCheques;
        }
        public Cheque getChequePorIDorNro(int id, string nroCheque)
        {
            Cheque cheque = null;

            // Conexión a la base de datos
            using (SqlConnection connection = conn.conectar())
            {

                string query = "SELECT * FROM Cheques WHERE id = @id";

                if (!string.IsNullOrEmpty(nroCheque))
                    //se obtiene el ultimo cheque cargado, en caso q haya dos nros de cheques iguales
                    query = "SELECT TOP 1 * FROM Cheques WHERE nroCheque = @nroCheque order by id desc";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@nroCheque", nroCheque);
                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Datos.Usuario oUserD = new Usuario();
                            cheque = new Cheque
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                NroCheque = reader["nroCheque"].ToString(),
                                Banco = reader["banco"].ToString(),
                                Propio = Convert.ToBoolean(reader["propio"]),
                                FechaEmision = reader["fechaEmision"].ToString(),
                                FechaPago = Convert.ToDateTime(reader["fechaPago"]),
                                Importe = Convert.ToDouble(reader["importe"]),
                                Estado = reader["estado"].ToString(),
                                Titular = reader["titular"].ToString(),
                                Observaciones = reader["observaciones"].ToString(),
                                RecibidoDe = Convert.ToInt32(reader["recibidoDe"]),
                                EntregadoA = Convert.ToInt32(reader["entregadoA"]),
                                PagoDe = Convert.ToInt32(reader["recibidoDe"]) > 0 ? getPagoById(Convert.ToInt32(reader["recibidoDe"])) : null,
                                PagoA = Convert.ToInt32(reader["entregadoA"]) > 0 ? getPagoById(Convert.ToInt32(reader["entregadoA"])) : null,
                                Creado = Convert.ToDateTime(reader["creado"]),
                                CreadoPor = Convert.ToInt32(reader["creadoPor"]) > 0 ? oUserD.getUsuarioById(Convert.ToInt32(reader["creadoPor"])) : null,
                                Actualizado = reader["actualizado"] != DBNull.Value ? Convert.ToDateTime(reader["actualizado"]) : (DateTime?)null,
                                ActualizadoPor = Convert.ToInt32(reader["actualizadoPor"]) > 0 ? oUserD.getUsuarioById(Convert.ToInt32(reader["creadoPor"])) : null,
                            };
                        }
                    }
                }
            }

            return cheque;
        }


        public List<Entidades.Cheque> getChequesPorPago(int idPago)
        {
            Cheque cheque = null;
            List<Cheque> listCheques = new List<Cheque>();
            // Conexión a la base de datos
            using (SqlConnection connection = conn.conectar())
            {

                string query = "SELECT * FROM Cheques WHERE recibidoDe = @idPago OR entregadoA = @idPago";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@idPago", idPago);
                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Datos.Usuario oUserD = new Usuario();
                            cheque = new Cheque
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                NroCheque = reader["nroCheque"].ToString(),
                                Banco = reader["banco"].ToString(),
                                Propio = Convert.ToBoolean(reader["propio"]),
                                FechaEmision = reader["fechaEmision"].ToString(),
                                FechaPago = Convert.ToDateTime(reader["fechaPago"]),
                                Importe = Convert.ToDouble(reader["importe"]),
                                Estado = reader["estado"].ToString(),
                                Titular = reader["titular"].ToString(),
                                Observaciones = reader["observaciones"].ToString(),
                                RecibidoDe = Convert.ToInt32(reader["recibidoDe"]),
                                EntregadoA = Convert.ToInt32(reader["entregadoA"]),
                                ///se comenta estas lineas xq sino entre en bucle
                                //PagoDe = Convert.ToInt32(reader["recibidoDe"]) > 0 ? getPagoById(Convert.ToInt32(reader["recibidoDe"])) : null,
                                //PagoA = Convert.ToInt32(reader["entregadoA"]) > 0 ? getPagoById(Convert.ToInt32(reader["entregadoA"])) : null,
                                Creado = Convert.ToDateTime(reader["creado"]),
                                CreadoPor = Convert.ToInt32(reader["creadoPor"]) > 0 ? oUserD.getUsuarioById(Convert.ToInt32(reader["creadoPor"])) : null,
                                Actualizado = reader["actualizado"] != DBNull.Value ? Convert.ToDateTime(reader["actualizado"]) : (DateTime?)null,
                                ActualizadoPor = Convert.ToInt32(reader["actualizadoPor"]) > 0 ? oUserD.getUsuarioById(Convert.ToInt32(reader["creadoPor"])) : null,
                            };
                            listCheques.Add(cheque);
                        }
                    }
                }
            }

            return listCheques;
        }

        public bool AddOrEditCheque(Cheque oCheque)
        {
            // Conexión a la base de datos
            using (SqlConnection connection = conn.conectar())
            {
                string query;

                if (oCheque.Id == 0)
                {
                    query = @"INSERT INTO Cheques (
                        nroCheque, banco, propio, fechaEmision, fechaPago,
                        importe, estado, titular, observaciones, recibidoDe,
                        entregadoA, creado, creadoPor, actualizado, actualizadoPor
                      ) VALUES (
                        @nroCheque, @banco, @propio, @fechaEmision, @fechaPago,
                        @importe, @estado, @titular, @observaciones, @recibidoDe,
                        @entregadoA, @creado, @creadoPor, @actualizado, @actualizadoPor
                      )";
                }
                else
                {
                    query = @"UPDATE Cheques SET
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
                        creado = @creado,
                        creadoPor = @creadoPor,
                        actualizado = @actualizado,
                        actualizadoPor = @actualizadoPor
                      WHERE id = @id";
                }

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    // Parámetros
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
                    cmd.Parameters.AddWithValue("@creado", oCheque.Creado ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("@creadoPor", oCheque.CreadoPor.Id);
                    cmd.Parameters.AddWithValue("@actualizado", (object)oCheque.Actualizado ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@actualizadoPor", oCheque.ActualizadoPor != null ? oCheque.ActualizadoPor.Id : 0);

                    if (oCheque.Id != 0)
                        cmd.Parameters.AddWithValue("@id", oCheque.Id);

                    connection.Open();
                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }

        public bool EliminarCheque(int id)
        {
            using (SqlConnection connection = conn.conectar())
            {
                string query = "DELETE FROM Cheques WHERE id = @id";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public bool resetearChequesAsignados(int idPago)
        {
            using (SqlConnection connection = conn.conectar())
            {
                string query = "UPDATE Cheques SET recibidoDe = 0 FROM Cheques WHERE recibidoDe = @idPago;"+
                     "UPDATE Cheques SET entregadoA = 0, estado = @estadoReset FROM Cheques WHERE entregadoA = @idPago;";
                // "(UPDATE Cheques SET recibidoDe = 0, entregadoA = 0, estado = PENDIENTE FROM Cheques WHERE recibidoDe = @idPago OR entregadoA = @idPago)" ;
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@idPago", idPago);
                    cmd.Parameters.AddWithValue("@estadoReset", "PENDIENTE");// Entidades.Cheque.EstadoEnum.PENDIENTE.ToString());
                    connection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public List<string> getBancos()
        {
            List<string> bancos = new List<string>();
            using (SqlConnection connection = conn.conectar())
            {
                string query = "SELECT banco FROM Bancos";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        bancos.Add(reader["banco"].ToString().Trim());
                    }
                }
            }
            return bancos;
        }

        #endregion

        #region Pagos

        public int getUltimoIdPago()
        {
            cmCtaCte = new SqlCommand();
            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.CommandType = CommandType.Text;
            cmCtaCte.CommandText = "Select top 1 id from Pagos order by id desc";

            cmCtaCte.Connection.Open();
            int idPago = (int)cmCtaCte.ExecuteScalar();
            cmCtaCte.Connection.Close();

            return idPago;
        }

        public Entidades.Pago addOrEditPago(Entidades.Pago oPagoE)
        {
            cmCtaCte = new SqlCommand();
            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.CommandType = CommandType.StoredProcedure;
            cmCtaCte.CommandText = "addOrEditPago";

            cmCtaCte.Parameters.AddWithValue("@id", oPagoE.Id);
            cmCtaCte.Parameters.AddWithValue("@nroRecibo", oPagoE.NroRecibo);
            cmCtaCte.Parameters.AddWithValue("@fecha", oPagoE.Fecha);
            cmCtaCte.Parameters.AddWithValue("@idPersona", oPagoE.Persona.idPersona);
            cmCtaCte.Parameters.AddWithValue("@aProveedor", oPagoE.AProveedor);
            cmCtaCte.Parameters.AddWithValue("@formaPago", oPagoE.FormaPago);
            cmCtaCte.Parameters.AddWithValue("@banco", oPagoE.Banco);
            cmCtaCte.Parameters.AddWithValue("@nroCheque", oPagoE.NroCheque);
            cmCtaCte.Parameters.AddWithValue("@titularCheque", oPagoE.TitularCheque);
            cmCtaCte.Parameters.AddWithValue("@importe", oPagoE.Importe);
            cmCtaCte.Parameters.AddWithValue("@efectivo", oPagoE.Efectivo);
            cmCtaCte.Parameters.AddWithValue("@observaciones", oPagoE.Observaciones);
            cmCtaCte.Parameters.AddWithValue("@idSucursal", oPagoE.Sucursal.idSucursal);
            cmCtaCte.Parameters.AddWithValue("@creadoPor", oPagoE.CreadoPor.Id);
            cmCtaCte.Parameters.AddWithValue("@actualizadoPor", oPagoE.ActualizadoPor != null ? oPagoE.ActualizadoPor.Id : 0);

            cmCtaCte.Connection.Open();
            oPagoE.Id = (int)cmCtaCte.ExecuteScalar();
            cmCtaCte.Connection.Close();

            ///Asigno el pago a los cheques,

            foreach (Entidades.Cheque item in oPagoE.Cheques)
            {
                //si se entregó el cheque
                if (oPagoE.AProveedor)
                {
                    item.EntregadoA = oPagoE.Id;
                    item.Estado = Entidades.Cheque.EstadoEnum.ENTREGADO.ToString();
                }
                else //de quien se recibió
                    item.RecibidoDe = oPagoE.Id;

                AddOrEditCheque(item);
            }

            return oPagoE;
        }



        public void eliminarPago(Entidades.Pago oPagoE)
        {
            cmCtaCte = new SqlCommand();

            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.Connection.Open();
            cmCtaCte.CommandType = CommandType.StoredProcedure;
            cmCtaCte.CommandText = "eliminarPago";

            cmCtaCte.Parameters.AddWithValue("@Id", oPagoE.Id);

            cmCtaCte.ExecuteNonQuery();
            cmCtaCte.Connection.Close();

            cmCtaCte = null;
        }

        public DataTable obtenerPagos(string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtPagos = new DataTable();
            daCtaCte = new SqlDataAdapter();

            cmCtaCte = new SqlCommand();
            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.CommandType = CommandType.Text;
            cmCtaCte.CommandText = "SELECT     dbo.Pagos.id, dbo.Pagos.fecha, dbo.Personas.razonSocial, " +
                " dbo.Pagos.nroRecibo, dbo.Pagos.importe,dbo.Pagos.aProveedor, CASE dbo.Pagos.aProveedor WHEN 0 THEN 'Cobro' WHEN 1 THEN 'Pago' END AS Operacion, "+
                "dbo.Pagos.formaPago, dbo.Pagos.efectivo, dbo.Pagos.observaciones, dbo.Pagos.creado, CreadoPor.nombre AS CreadoPor, " +
                " dbo.Pagos.actualizado, ActualizadoPor.nombre AS ActualizadoPor " +
                " FROM  dbo.Pagos INNER JOIN dbo.Personas ON dbo.Pagos.idPersona = dbo.Personas.idPersona LEFT OUTER JOIN " +
                " dbo.Usuarios AS ActualizadoPor ON dbo.Pagos.creadoPor = ActualizadoPor.id LEFT OUTER JOIN " +
                " dbo.Usuarios AS CreadoPor ON dbo.Pagos.actualizadoPor = CreadoPor.id " +
               // " WHERE dbo.Pagos.fecha between '" + fechaDesde + "' and '" + fechaHasta.AddDays(1) + "'" +
                " WHERE dbo.Pagos.fecha between @fechaDesde and @fechaHasta" +
                " and (dbo.Personas.razonSocial like '%" + texto + "%' or dbo.Pagos.nroRecibo like '%" + texto + "%')"+
                " ORDER BY dbo.Pagos.fecha DESC";

            cmCtaCte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCtaCte.Parameters.AddWithValue("@fechaHasta", fechaHasta.AddDays(1));

            daCtaCte.SelectCommand = cmCtaCte;
            daCtaCte.Fill(dtPagos);

            cmCtaCte.Connection.Close();

            return dtPagos;
        }

        public Entidades.Pago getPagoById(int idPago)
        {
            cmCtaCte = new SqlCommand();
            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.CommandType = CommandType.Text;
            cmCtaCte.CommandText = "Select Pagos.* from Pagos where id = " + idPago;

            Entidades.Pago oPagoE = new Entidades.Pago();
            try
            {
                cmCtaCte.Connection.Open();
                SqlDataReader drPago = cmCtaCte.ExecuteReader();
                using (drPago)
                {
                    while (drPago.Read())
                    {
                        oPagoE.Id = Convert.ToInt32(drPago["id"]);
                        Datos.Persona oPersonaD = new Datos.Persona();
                        oPagoE.Persona = oPersonaD.findById(Convert.ToInt32(drPago["idPersona"]));

                        oPagoE.Fecha = Convert.ToDateTime(drPago["fecha"]);
                        oPagoE.NroRecibo = Convert.ToString(drPago["nroRecibo"]);
                        oPagoE.AProveedor = drPago["aProveedor"].Equals(DBNull.Value) ? false : Convert.ToBoolean(drPago["aProveedor"]);
                        oPagoE.FormaPago = Convert.ToString(drPago["formaPago"]);
                        oPagoE.Banco = Convert.ToString(drPago["banco"]);
                        oPagoE.NroCheque = Convert.ToString(drPago["nroCheque"]);
                        oPagoE.TitularCheque = Convert.ToString(drPago["titularCheque"]);
                        oPagoE.Importe = float.Parse(drPago["importe"].ToString());
                        oPagoE.Efectivo = float.Parse(drPago["efectivo"].ToString());
                        oPagoE.Observaciones = Convert.ToString(drPago["observaciones"]);
                        oPagoE.Cheques = getChequesPorPago(oPagoE.Id);

                        Datos.Sucursal oSucursalD = new Sucursal();
                        oPagoE.Sucursal = oSucursalD.findById(Convert.ToInt32(drPago["idSucursal"]));


                        oPagoE.Creado = Convert.ToDateTime(drPago["creado"]);
                        oPagoE.Actualizado = drPago["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drPago["actualizado"]);

                        Datos.Usuario oUsuarioD = new Usuario();
                        oPagoE.CreadoPor = oUsuarioD.getUsuarioById(Convert.ToInt32(drPago["creadoPor"]));
                        oPagoE.ActualizadoPor = drPago["actualizadoPor"].Equals(DBNull.Value) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drPago["actualizadoPor"]));
                    }
                    return oPagoE;
                }
            }
            finally
            {
                cmCtaCte.Connection.Close();
                oPagoE = null;
            }
        }

        #endregion

    }
}
