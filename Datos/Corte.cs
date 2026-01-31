using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidades;
using Utilidades;

namespace Datos
{
    public class Corte
    {
        private readonly Utilidades.Conexion conn;
        private readonly IEmpresaContext _empresa;

        public Corte(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
            conn = new Utilidades.Conexion();
        }

        // =========================
        // MAPPER
        // =========================
        private Entidades.Corte MapCorte(SqlDataReader drCorte, bool cargarMaestro)
        {
            Entidades.Corte oCorteE = new Entidades.Corte();

            oCorteE.IdCorte = Convert.ToInt32(drCorte["idCorte"]);
            oCorteE.Codigo = Convert.ToInt64(drCorte["codigo"]);
            oCorteE.CorteDesc = Convert.ToString(drCorte["corte"]);

            if (drCorte["idMarca"] != DBNull.Value)
            {
                Datos.Persona oPersonaD = new Datos.Persona(_empresa);
                oCorteE.Marca = oPersonaD.findById(Convert.ToInt32(drCorte["idMarca"]));
            }

            oCorteE.Tipo = Convert.ToString(drCorte["tipo"]);
            oCorteE.Promedio = float.Parse(drCorte["promedio"].ToString());
            oCorteE.PuntoStock = Convert.ToInt32(drCorte["puntoStock"]);
            oCorteE.Nivel = Convert.ToInt32(drCorte["nivel"]);

            if (cargarMaestro)
            {
                int idMaestro = drCorte["idCorteMaestro"] == DBNull.Value ? 0 : Convert.ToInt32(drCorte["idCorteMaestro"]);
                oCorteE.CorteMaestro = (idMaestro > 0) ? findCorteById(idMaestro, false) : null;
            }

            oCorteE.Porcentaje = float.Parse(drCorte["porcentaje"].ToString());
            oCorteE.PrecioKg = float.Parse(drCorte["precioKg"].ToString());
            oCorteE.PrecioKgReferencia = float.Parse(drCorte["precioKg"].ToString());
            oCorteE.IngresoRapidoEmbutido = Convert.ToBoolean(drCorte["ingresoRapidoEmbutido"]);
            oCorteE.Habilitado = Convert.ToBoolean(drCorte["habilitado"]);
            oCorteE.EnCierreStock = Convert.ToBoolean(drCorte["enCierreStock"]);
            oCorteE.PorcentajeHueso = float.Parse(drCorte["porcentajeHueso"].ToString());
            oCorteE.Independiente = Convert.ToInt32(drCorte["independiente"]);
            oCorteE.DesvioEstandar = float.Parse(drCorte["desvioEstandar"].ToString());

            oCorteE.Creado = Convert.ToDateTime(drCorte["creado"]);
            oCorteE.Actualizado = drCorte["actualizado"] == DBNull.Value ? null : (DateTime?)drCorte["actualizado"];

            oCorteE.IdAlicuotaIva = Convert.ToInt32(drCorte["idAlicuotaIva"]);
            oCorteE.AlicuotaIva = float.Parse(drCorte["alicuotaIva"].ToString());
            oCorteE.Pesable = Convert.ToBoolean(drCorte["pesable"]);

            // Presentación
            oCorteE.Presentacion = oCorteE.EsPresentacion(oCorteE.porcentajeHueso);
            if (oCorteE.Presentacion)
            {
                oCorteE.porcentaje = oCorteE.getCantPresentacion(oCorteE.porcentajeHueso);
            }

            return oCorteE;
        }

        // =========================
        // CORTES
        // =========================
        public List<Entidades.Corte> findAllCortes(bool buscarMaestro)
        {
            var lista = new List<Entidades.Corte>();

            using (SqlConnection cn = conn.conectar(_empresa)) // YA viene abierta
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Corte ORDER BY codigo ASC", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(MapCorte(dr, buscarMaestro));
                    }
                }
            }

            return lista;
        }

        public Entidades.Corte findCorteById(int idCorte, bool buscarMaestro)
        {
            using (SqlConnection cn = conn.conectar(_empresa)) // YA viene abierta
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Corte WHERE idCorte = @idCorte", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.Add("@idCorte", SqlDbType.Int).Value = idCorte;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return MapCorte(dr, buscarMaestro);
                }
            }

            return null;
        }

        public Entidades.Corte findCorteByCodigo(long codigo, bool buscarMaestro)
        {
            using (SqlConnection cn = conn.conectar(_empresa)) // YA viene abierta
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Corte WHERE codigo = @codigo", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.Add("@codigo", SqlDbType.BigInt).Value = codigo;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return MapCorte(dr, buscarMaestro);
                }
            }

            return null;
        }

        public void editPrecioCorte(Entidades.Corte oCorteE)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("UPDATE Corte SET precioKg = @precioKg WHERE idCorte = @idCorte", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@precioKg", oCorteE.precioKg);
                cmd.Parameters.AddWithValue("@idCorte", oCorteE.idCorte);
                cmd.ExecuteNonQuery();
            }
        }

        public void addOrEditCorte(Entidades.Corte oCorteE)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("addOrEditCorte", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idCorte", oCorteE.idCorte);
                cmd.Parameters.AddWithValue("@codigo", oCorteE.codigo);
                cmd.Parameters.AddWithValue("@corte", oCorteE.corte ?? "");
                cmd.Parameters.AddWithValue("@tipo", oCorteE.tipo ?? "");
                cmd.Parameters.AddWithValue("@idMarca", oCorteE.Marca != null ? oCorteE.Marca.IdPersona : 0);
                cmd.Parameters.AddWithValue("@puntoStock", oCorteE.PuntoStock);
                cmd.Parameters.AddWithValue("@promedio", oCorteE.Promedio);
                cmd.Parameters.AddWithValue("@independiente", oCorteE.independiente);
                cmd.Parameters.AddWithValue("@precioKg", oCorteE.precioKg);
                cmd.Parameters.AddWithValue("@ingresoRapidoEmbutido", oCorteE.IngresoRapidoEmbutido);
                cmd.Parameters.AddWithValue("@habilitado", oCorteE.Habilitado);
                cmd.Parameters.AddWithValue("@enCierreStock", oCorteE.EnCierreStock);
                cmd.Parameters.AddWithValue("@idCorteMaestro", oCorteE.corteMaestro != null ? oCorteE.corteMaestro.idCorte : 0);
                cmd.Parameters.AddWithValue("@porcentaje", oCorteE.porcentaje);
                cmd.Parameters.AddWithValue("@porcentajeHueso", oCorteE.porcentajeHueso);
                cmd.Parameters.AddWithValue("@desvioEstandar", oCorteE.desvioEstandar);
                cmd.Parameters.AddWithValue("@idAlicuotaIva", oCorteE.IdAlicuotaIva);
                cmd.Parameters.AddWithValue("@alicuotaIva", oCorteE.AlicuotaIva);
                cmd.Parameters.AddWithValue("@pesable", oCorteE.Pesable);

                cmd.ExecuteNonQuery();
            }
        }

        public DataTable buscarCorte(string txtBusqueda)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("buscarCorte", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@texto", txtBusqueda ?? "");

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable buscarCorteSinMaestro(string txtBusqueda)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("buscarCorteSinMaestro", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@texto", txtBusqueda ?? "");

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable buscarCodigoCorte(long codigo)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("buscarCodigoCorte", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@codigo", codigo);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public void eliminarCorte(Entidades.Corte oCorteE)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("EliminarCorte", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idCorte", oCorteE.idCorte);
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable obtenerCortes()
        {
            DataTable dt = new DataTable();

            string sql =
                "SELECT CorteP.idCorte, CorteP.codigo, CorteP.corte, CorteP.precioKg, CorteP.ingresoRapidoEmbutido, CorteP.enCierreStock, " +
                "CorteP.tipo, CorteP.pesable, CorteP.nivel, CorteP.idCorteMaestro, CorteM.corte AS corteMaestro, CorteP.porcentaje, CorteP.porcentajeHueso, CorteP.desvioEstandar, " +
                "CorteP.independiente, CorteP.promedio, CorteP.idAlicuotaIva, CorteP.alicuotaIva " +
                "FROM dbo.Corte AS CorteM RIGHT OUTER JOIN dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable cargarDtCortes()
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Corte", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable obtenerEmbutidos(string txtBusqueda)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerEmbutidos", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@texto", txtBusqueda ?? "");

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable getListaElegirEmbutido()
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("getListaElegirEmbutido", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable buscarEmbutido(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("buscarEmbutido", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable obtenerLineasEmb(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerLineasEmb", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable obtenerInfoCorte(int idCorte)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerInfoCorte", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idCorte", idCorte);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable obtenerCorteProveedor(int idCorte)
        {
            DataTable dt = new DataTable();

            string sql =
                "SELECT dbo.Personas.razonSocial, dbo.CorteProveedor.ultimoPrecio, dbo.CorteProveedor.fechaUltimaCompra " +
                "FROM dbo.Corte " +
                "INNER JOIN dbo.CorteProveedor ON dbo.Corte.idCorte = dbo.CorteProveedor.idCorte " +
                "INNER JOIN dbo.Personas ON dbo.CorteProveedor.idProveedor = dbo.Personas.idPersona " +
                "WHERE dbo.CorteProveedor.idCorte = @idCorte " +
                "ORDER BY dbo.CorteProveedor.fechaUltimaCompra DESC";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idCorte", idCorte);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        // =========================
        // FORMULAS
        // =========================
        public DataTable buscarFormula(string texto)
        {
            DataTable dt = new DataTable();

            string sql =
                "SELECT DISTINCT F.idFormula, C.codigo, C.corte, F.creado, F.actualizado " +
                "FROM dbo.Corte C " +
                "INNER JOIN dbo.Formulas F ON C.idCorte = F.idEmbutido " +
                "WHERE C.corte LIKE @texto " +
                "ORDER BY C.codigo";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@texto", "%" + (texto ?? "") + "%");

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public Entidades.Formula findFormulaByID(int idFormula, int idEmbutido)
        {
            string sql = idFormula > 0
                ? "SELECT * FROM Formulas WHERE idFormula = @idFormula"
                : "SELECT * FROM Formulas WHERE idEmbutido = @idEmbutido";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                if (idFormula > 0)
                    cmd.Parameters.Add("@idFormula", SqlDbType.Int).Value = idFormula;
                else
                    cmd.Parameters.Add("@idEmbutido", SqlDbType.Int).Value = idEmbutido;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    Entidades.Formula oFormula = null;

                    while (dr.Read())
                    {
                        oFormula = new Entidades.Formula();
                        oFormula.IdFormula = Convert.ToInt32(dr["idFormula"]);
                        oFormula.Embutido = findCorteById(Convert.ToInt32(dr["idEmbutido"]), false);
                        oFormula.Receta = dr["receta"].ToString();

                        oFormula.Creado = Convert.ToDateTime(dr["creado"]);
                        oFormula.Actualizado = dr["actualizado"] == DBNull.Value ? null : (DateTime?)dr["actualizado"];

                        Datos.Usuario oUsuarioD = new Usuario(_empresa);
                        oFormula.CreadoPor = string.IsNullOrEmpty(dr["creadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(dr["creadoPor"]));
                        oFormula.ActualizadoPor = string.IsNullOrEmpty(dr["actualizadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(dr["actualizadoPor"]));
                    }

                    if (oFormula != null)
                        oFormula.ListaCortesEnFormula = cargarCortesPorFormula(oFormula);

                    return oFormula;
                }
            }
        }

        public List<Entidades.CortePorFormula> cargarCortesPorFormula(Entidades.Formula oFormula)
        {
            if (oFormula == null) throw new ArgumentNullException(nameof(oFormula));

            var lista = new List<Entidades.CortePorFormula>();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM CortePorFormula WHERE idFormula = @idFormula", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.Add("@idFormula", SqlDbType.Int).Value = oFormula.IdFormula;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Entidades.CortePorFormula item = new Entidades.CortePorFormula();
                        item.IdCorteEnFormula = Convert.ToInt32(dr["idCortePorFormula"]);
                        item.Formula = oFormula;
                        item.CorteEnFormula = findCorteById(Convert.ToInt32(dr["idCorte"]), false);
                        item.Porcentaje = float.Parse(dr["porcentaje"].ToString());
                        item.AgregarAuto = Convert.ToBoolean(dr["agregarAuto"]);
                        lista.Add(item);
                    }
                }
            }

            return lista;
        }

        public int existeFormula(int idEmbutido)
        {
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("SELECT idFormula FROM Formulas WHERE idEmbutido = @idEmbutido", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.Add("@idEmbutido", SqlDbType.Int).Value = idEmbutido;

                object obj = cmd.ExecuteScalar();
                return (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
            }
        }

        public int addOrEditFormula(Entidades.Formula oFormula, List<Entidades.CortePorFormula> listaCortesPorFormula)
        {
            if (oFormula == null) throw new ArgumentNullException(nameof(oFormula));
            if (listaCortesPorFormula == null) listaCortesPorFormula = new List<Entidades.CortePorFormula>();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("addOrEditFormula", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idFormula", oFormula.IdFormula);
                cmd.Parameters.AddWithValue("@idEmbutido", oFormula.Embutido.idCorte);
                cmd.Parameters.AddWithValue("@receta", oFormula.Receta ?? "");
                cmd.Parameters.AddWithValue("@creadoPor", oFormula.CreadoPor.Id);
                cmd.Parameters.AddWithValue("@actualizadoPor", oFormula.ActualizadoPor != null ? oFormula.ActualizadoPor.Id : 0);

                object obj = cmd.ExecuteScalar();
                oFormula.IdFormula = (obj == null || obj == DBNull.Value) ? oFormula.IdFormula : Convert.ToInt32(obj);

                // Inserta detalles
                cmd.CommandText = "agregarCortePorFormula";
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (var item in listaCortesPorFormula)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@idFormula", oFormula.IdFormula);
                    cmd.Parameters.AddWithValue("@idCorte", item.CorteEnFormula1.idCorte);
                    cmd.Parameters.AddWithValue("@porcentaje", item.Porcentaje);
                    cmd.Parameters.AddWithValue("@agregarAuto", item.AgregarAuto);
                    cmd.ExecuteNonQuery();
                }

                return oFormula.IdFormula;
            }
        }

        public void eliminarFormula(int idFormula)
        {
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.CommandText = "DELETE FROM CortePorFormula WHERE idFormula = @idFormula";
                cmd.Parameters.Add("@idFormula", SqlDbType.Int).Value = idFormula;
                cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();
                cmd.CommandText = "DELETE FROM Formulas WHERE idFormula = @idFormula";
                cmd.Parameters.Add("@idFormula", SqlDbType.Int).Value = idFormula;
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable getFormulaEmbutido(int idEmbutido)
        {
            DataTable dt = new DataTable();

            string sql =
                "SELECT C.idCorte, C.codigo, C.corte, CPF.porcentaje, '' AS kgs, CPF.agregarAuto " +
                "FROM dbo.Formulas F " +
                "INNER JOIN dbo.CortePorFormula CPF ON F.idFormula = CPF.idFormula " +
                "INNER JOIN dbo.Corte C ON CPF.idCorte = C.idCorte " +
                "WHERE F.idEmbutido = @idEmbutido " +
                "ORDER BY CPF.agregarAuto DESC";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idEmbutido", idEmbutido);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable obtenerTiposProducto(bool mostrarTodos)
        {
            DataTable dt = new DataTable();

            string sql = "SELECT tipo FROM TiposProducto";
            sql += mostrarTodos ? " ORDER BY orden, tipo" : " WHERE orden > 0 ORDER BY orden, tipo";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        // =========================
        // ALICUOTAS IVA
        // =========================
        public DataTable obtenerAlicuotasIva(bool mostrarTodos)
        {
            DataTable dt = new DataTable();

            string sql = "SELECT idIva, iva FROM AlicuotasIva";
            sql += mostrarTodos ? " ORDER BY orden" : " WHERE mostrar = 1 ORDER BY orden";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public Entidades.AlicuotaIva findAlicuotaIvaById(int idIva)
        {
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM AlicuotasIva WHERE idIva = @idIva", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.Add("@idIva", SqlDbType.Int).Value = idIva;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return null;

                    Entidades.AlicuotaIva o = new Entidades.AlicuotaIva();
                    o.IdIva = Convert.ToInt32(dr["idIva"]);
                    o.Iva = Convert.ToInt32(dr["iva"]);
                    o.Orden = Convert.ToInt32(dr["orden"]);
                    o.Mostrar = Convert.ToBoolean(dr["mostrar"]);
                    return o;
                }
            }
        }

        // =========================
        // EMBUTIDOS
        // =========================
        public Entidades.Embutido findEmbutidoById(int idEmbutido)
        {
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Embutidos WHERE idEmbutido = @idEmbutido", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.Add("@idEmbutido", SqlDbType.Int).Value = idEmbutido;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    Entidades.Embutido o = null;

                    while (dr.Read())
                    {
                        o = new Entidades.Embutido();
                        o.IdEmbutido = Convert.ToInt32(dr["idEmbutido"]);
                        o.FechaEmbutido = Convert.ToDateTime(dr["fechaEmbutido"]);
                        o.Corte = findCorteById(Convert.ToInt32(dr["idCorte"]), true);

                        Datos.Sucursal oSucursalD = new Sucursal(_empresa);
                        o.Sucursal = oSucursalD.findById(Convert.ToInt32(dr["idSucursal"]));

                        o.Observaciones = Convert.ToString(dr["observaciones"]);
                        o.Estado = Convert.ToString(dr["estado"]);
                        o.Creado = Convert.ToDateTime(dr["creado"]);
                        o.Actualizado = dr["actualizado"] == DBNull.Value ? null : (DateTime?)dr["actualizado"];

                        Datos.Usuario oUsuarioD = new Usuario(_empresa);
                        o.CreadoPor = string.IsNullOrEmpty(dr["creadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(dr["creadoPor"]));
                        o.ActualizadoPor = string.IsNullOrEmpty(dr["actualizadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(dr["actualizadoPor"]));
                    }

                    if (o != null)
                        o.CortesEnEmbutido = obtenerCortesEnEmbutido(o);

                    return o;
                }
            }
        }

        public List<Entidades.CortePorEmbutido> obtenerCortesEnEmbutido(Entidades.Embutido oEmbutidoParam)
        {
            if (oEmbutidoParam == null) throw new ArgumentNullException(nameof(oEmbutidoParam));

            var lista = new List<Entidades.CortePorEmbutido>();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM CortePorEmbutido WHERE idEmbutido = @idEmbutido", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.Add("@idEmbutido", SqlDbType.Int).Value = oEmbutidoParam.idEmbutido;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Entidades.CortePorEmbutido item = new Entidades.CortePorEmbutido();
                        item.IdCorteEmbutido = Convert.ToInt32(dr["idCorteEmbutido"]);
                        item.Embutido = oEmbutidoParam;
                        item.Corte = findCorteById(Convert.ToInt32(dr["idCorte"]), false);
                        item.KgUtilizado = float.Parse(dr["kgUtilizados"].ToString());
                        item.PesoBalanza = Convert.ToBoolean(dr["pesoBalanza"]);
                        lista.Add(item);
                    }
                }
            }

            return lista;
        }

        public int agregarEmbutido(Entidades.Embutido oEmbutido)
        {
            if (oEmbutido == null) throw new ArgumentNullException(nameof(oEmbutido));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("agregarEmbutido", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@fechaEmbutido", oEmbutido.fechaEmbutido);
                cmd.Parameters.AddWithValue("@idCorte", oEmbutido.corte.idCorte);
                cmd.Parameters.AddWithValue("@idSucursal", oEmbutido.sucursal.IdSucursal);
                cmd.Parameters.AddWithValue("@creadoPor", oEmbutido.CreadoPor.Id);
                cmd.Parameters.AddWithValue("@observaciones", oEmbutido.observaciones ?? "");

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    int id = 0;
                    while (dr.Read())
                        id = Convert.ToInt32(dr["idEmbutido"].ToString());
                    return id;
                }
            }
        }

        public void anularEmbutido(Entidades.Embutido oEmbutidoE)
        {
            if (oEmbutidoE == null) throw new ArgumentNullException(nameof(oEmbutidoE));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("anularEmbutido", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idEmbutido", oEmbutidoE.idEmbutido);
                cmd.Parameters.AddWithValue("@actualizadoPor", oEmbutidoE.ActualizadoPor.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable obtenerCortesPorEmbutidos(Entidades.Embutido oEmbutidoE)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerCortesPorEmbutidos", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idEmbutido", oEmbutidoE.idEmbutido);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public void agregarCortePorEmbutido(Entidades.CortePorEmbutido oCortePorEmbutido)
        {
            if (oCortePorEmbutido == null) throw new ArgumentNullException(nameof(oCortePorEmbutido));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("agregarCortePorEmbutido", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idEmbutido", oCortePorEmbutido.embutido.idEmbutido);
                cmd.Parameters.AddWithValue("@idCorte", oCortePorEmbutido.corte.idCorte);
                cmd.Parameters.AddWithValue("@kgUtilizados", oCortePorEmbutido.kgUtilizado);
                cmd.Parameters.AddWithValue("@idSucursal", oCortePorEmbutido.embutido.sucursal.IdSucursal);
                cmd.Parameters.AddWithValue("@pesoBalanza", oCortePorEmbutido.PesoBalanza);

                cmd.ExecuteNonQuery();
            }
        }

        public void actualizarStockEmbutido(DataRow cortePorEmbutido, Entidades.Embutido oEmbutidoE)
        {
            if (cortePorEmbutido == null) throw new ArgumentNullException(nameof(cortePorEmbutido));
            if (oEmbutidoE == null) throw new ArgumentNullException(nameof(oEmbutidoE));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("actualizarStockEmbutido", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idEmbutido", cortePorEmbutido["idEmbutido"]);
                cmd.Parameters.AddWithValue("@idCorte", cortePorEmbutido["idCorte"]);
                cmd.Parameters.AddWithValue("@kgUtilizados", cortePorEmbutido["kgUtilizados"]);
                cmd.Parameters.AddWithValue("@idSucursal", oEmbutidoE.sucursal.idSucursal);

                cmd.ExecuteNonQuery();
            }
        }

        // =========================
        // MOVIMIENTO
        // =========================
        public int addOrEditMovimiento(Entidades.Movimiento oMovimientoE)
        {
            if (oMovimientoE == null) throw new ArgumentNullException(nameof(oMovimientoE));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("addOrEditMovimiento", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento);
                cmd.Parameters.AddWithValue("@fechaMovimiento", oMovimientoE.FechaMovimiento);
                cmd.Parameters.AddWithValue("@sucursalOrigen", oMovimientoE.SucursalOrigen.idSucursal);
                cmd.Parameters.AddWithValue("@sucursalDestino", oMovimientoE.SucursalDestino.idSucursal);
                cmd.Parameters.AddWithValue("@observaciones", oMovimientoE.Observaciones ?? "");
                cmd.Parameters.AddWithValue("@creadoPor", oMovimientoE.CreadoPor.Id);

                if (oMovimientoE.IdMovimiento == 0)
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                            oMovimientoE.IdMovimiento = Convert.ToInt32(dr["idMovimiento"].ToString());
                    }
                }
                else
                {
                    cmd.Parameters.AddWithValue("@actualizadoPor", oMovimientoE.ActualizadoPor.Id);
                    cmd.ExecuteNonQuery();
                }

                return oMovimientoE.IdMovimiento;
            }
        }

        public void modificarMovimiento(Entidades.Movimiento oMovimientoE)
        {
            if (oMovimientoE == null) throw new ArgumentNullException(nameof(oMovimientoE));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("modificarMovimiento", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento);
                cmd.Parameters.AddWithValue("@fechaMovimiento", oMovimientoE.FechaMovimiento);
                cmd.Parameters.AddWithValue("@sucursalOrigen", oMovimientoE.SucursalOrigen.idSucursal);
                cmd.Parameters.AddWithValue("@sucursalDestino", oMovimientoE.SucursalDestino.idSucursal);
                cmd.Parameters.AddWithValue("@observaciones", oMovimientoE.Observaciones ?? "");
                cmd.Parameters.AddWithValue("@actualizadoPor", oMovimientoE.ActualizadoPor.Id);

                cmd.ExecuteNonQuery();
            }
        }

        public void eliminarMovimiento(int idMovimiento, Entidades.Usuario oUsuario)
        {
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("eliminarMovimiento", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idMovimiento", idMovimiento);
                cmd.Parameters.AddWithValue("@actualizadoPor", oUsuario.Id);

                cmd.ExecuteNonQuery();
            }
        }

        public void agregarCortePorMovimiento(Entidades.CortePorMovimiento cortePorMovimiento)
        {
            if (cortePorMovimiento == null) throw new ArgumentNullException(nameof(cortePorMovimiento));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("agregarCortePorMovimiento", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idMovimiento", cortePorMovimiento.Movimientos.IdMovimiento);
                cmd.Parameters.AddWithValue("@idCorte", cortePorMovimiento.Corte.IdCorte);
                cmd.Parameters.AddWithValue("@cantKg", cortePorMovimiento.CantKg);
                cmd.Parameters.AddWithValue("@cantUnidad", cortePorMovimiento.CantUnidad);
                cmd.Parameters.AddWithValue("@pesoBalanza", cortePorMovimiento.PesoBalanza);
                cmd.Parameters.AddWithValue("@permitirIngreso", cortePorMovimiento.PermitirIngreso);

                cmd.ExecuteNonQuery();
            }
        }

        public void quitarCortesPorMovimiento(Entidades.Movimiento oMovimientoE)
        {
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("quitarCortesPorMovimiento", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento);
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable obtenerMovimientos(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerMovimientos", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@sucOrigen", sucOrigen ?? "");
                cmd.Parameters.AddWithValue("@sucDestino", sucDestino ?? "");
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);
                cmd.Parameters.AddWithValue("@texto", texto ?? "");

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public Entidades.Movimiento cargarMovimiento(int idMovimiento, bool acumulado)
        {
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("cargarMovimiento", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idMovimiento", idMovimiento);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    Entidades.Movimiento oMovimiento = new Entidades.Movimiento();

                    while (dr.Read())
                    {
                        oMovimiento.IdMovimiento = Convert.ToInt32(dr["idMovimiento"]);
                        oMovimiento.FechaMovimiento = Convert.ToDateTime(dr["fechaMovimiento"]);

                        Entidades.Sucursal origen = new Entidades.Sucursal();
                        origen.idSucursal = Convert.ToInt32(dr["idOrigen"]);
                        origen.sucursal = dr["origen"].ToString();
                        oMovimiento.SucursalOrigen = origen;

                        oMovimiento.IdMovOrigen = string.IsNullOrEmpty(dr["idMovOrigen"].ToString()) ? 0 : Convert.ToInt32(dr["idMovOrigen"]);

                        Entidades.Sucursal destino = new Entidades.Sucursal();
                        destino.idSucursal = Convert.ToInt32(dr["idDestino"]);
                        destino.sucursal = dr["destino"].ToString();
                        oMovimiento.SucursalDestino = destino;

                        oMovimiento.Observaciones = dr["observaciones"].ToString();

                        oMovimiento.Creado = Convert.ToDateTime(dr["creado"]);
                        oMovimiento.Actualizado = dr["actualizado"] == DBNull.Value ? null : (DateTime?)dr["actualizado"];

                        Datos.Usuario oUsuarioD = new Usuario(_empresa);
                        oMovimiento.CreadoPor = string.IsNullOrEmpty(dr["creadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(dr["creadoPor"]));
                        oMovimiento.ActualizadoPor = string.IsNullOrEmpty(dr["actualizadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(dr["actualizadoPor"]));

                        oMovimiento.ListaCortesPorMov = cargarCortesPorMovimiento(oMovimiento.IdMovimiento, acumulado);
                    }

                    return oMovimiento;
                }
            }
        }

        public List<Entidades.CortePorMovimiento> cargarCortesPorMovimiento(int idMovimiento, bool acumulado)
        {
            var lista = new List<Entidades.CortePorMovimiento>();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("cargarCortesPorMovimiento", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idMovimiento", idMovimiento);
                cmd.Parameters.AddWithValue("@acumulado", acumulado);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Entidades.CortePorMovimiento item = new Entidades.CortePorMovimiento();
                        item.IdCorteMovimiento = Convert.ToInt32(dr["idCorteMovimiento"]);

                        Entidades.Corte corte = new Entidades.Corte();
                        corte.idCorte = Convert.ToInt32(dr["idCorte"]);
                        corte.codigo = Convert.ToInt64(dr["codigo"]);
                        corte.corte = dr["corte"].ToString();
                        item.Corte = corte;

                        item.CantKg = float.Parse(dr["cantKg"].ToString());
                        item.CantUnidad = Convert.ToInt32(dr["cantUnidad"]);

                        if (!acumulado)
                        {
                            item.PesoBalanza = dr["pesoBalanza"] == DBNull.Value ? false : Convert.ToBoolean(dr["pesoBalanza"]);
                            item.PermitirIngreso = dr["permitirIngreso"] == DBNull.Value ? false : Convert.ToBoolean(dr["permitirIngreso"]);
                        }

                        lista.Add(item);
                    }
                }
            }

            return lista;
        }

        public DataTable obtenerLineasMov(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerLineasMov", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@sucOrigen", sucOrigen ?? "");
                cmd.Parameters.AddWithValue("@sucDestino", sucDestino ?? "");
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);
                cmd.Parameters.AddWithValue("@texto", texto ?? "");

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public void reiniciarStockReal(int idSucursal)
        {
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("reiniciarStock", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.ExecuteNonQuery();
            }
        }

        public void reiniciarStockTeorico(int idSucursal)
        {
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("reiniciarStockTeorico", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable reporteTeoricoReal(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("StockTeoricoReal", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DateTime fechaUltimoCierreStock_Sucursal(int idSucursal)
        {
            DateTime fecha = DateTime.MinValue;

            string sql =
                "SELECT TOP 1 fechaCompra " +
                "FROM Compras " +
                "WHERE tipoCompra = 'Cierre Stock' AND idSucursal = @idSucursal " +
                "ORDER BY fechaCompra DESC";

            using (SqlConnection cn = conn.conectar(_empresa)) // YA viene abierta
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    fecha = Convert.ToDateTime(result);
            }

            return fecha;
        }

        public DataTable CierreStock(int nroCierre, string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta,
                                     string conexionSucursal, string tipo, int idProveedor, int idMarca)
        {
            DataTable dt = new DataTable();

            string sp = (nroCierre == 2) ? "StockCierre_2" : "a_CierreStock";

            using (SqlConnection cn = string.IsNullOrEmpty(conexionSucursal)
                    ? conn.conectar(_empresa)
                    : conn.conectar(conexionSucursal, _empresa))
            using (SqlCommand cmd = new SqlCommand(sp, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);
                cmd.Parameters.AddWithValue("@tipo", tipo ?? "");
                cmd.Parameters.AddWithValue("@idProveedor", idProveedor);
                cmd.Parameters.AddWithValue("@idMarca", idMarca);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable acum_Ventas(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo, int idProveedor, int idMarca)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("Acum_Ventas", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);
                cmd.Parameters.AddWithValue("@tipo", tipo ?? "");
                cmd.Parameters.AddWithValue("@idProveedor", idProveedor);
                cmd.Parameters.AddWithValue("@idMarca", idMarca);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable StockIngresoEgreso(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("StockIngresoEgreso", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable TotalPorCortesVendidos(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo, int idProveedor, int idMarca)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("TotalPorCortesVendidos", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);
                cmd.Parameters.AddWithValue("@tipo", tipo ?? "");
                cmd.Parameters.AddWithValue("@idProveedor", idProveedor);
                cmd.Parameters.AddWithValue("@idMarca", idMarca);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable imprimirTeoricoReal(DataTable dtTeoricoReal, string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            if (dtTeoricoReal == null) dtTeoricoReal = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("StockTeoricoReal", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dtTeoricoReal);
                }
            }

            return dtTeoricoReal;
        }

        public DataTable TotalKgsCortePorCompra(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("a_CierreStock", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable TotalMovimientosPorCorte(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("TotalMovimientosPorCorte", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable Balance(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("BalanceConsFinal_FecDesde_Hasta", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.Parameters.AddWithValue("@FechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@FechaHasta", fechaHasta.AddDays(1)); // incluir día completo

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            if (dt.Columns.Contains("orden"))
                dt.Columns.Remove("orden");

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

        // =========================
        // TIPOS PRODUCTO
        // =========================
        public DataTable obtenerTiposProductoGrilla(string buscarText)
        {
            DataTable dt = new DataTable();

            string sql = "SELECT tipo, orden, creado as Creado, actualizado as Actualizado, reservadoSistema as Reservado " +
                         "FROM TiposProducto " +
                         (string.IsNullOrEmpty(buscarText) ? "" : "WHERE tipo LIKE @buscar ") +
                         "ORDER BY orden, tipo";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                if (!string.IsNullOrEmpty(buscarText))
                    cmd.Parameters.AddWithValue("@buscar", "%" + buscarText + "%");

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public string addOrEditTipoProducto(string tiposProducto, string orden, bool esInsert, string tipoToUpdate)
        {
            string mensaje = "";

            using (SqlConnection cn = conn.conectar(_empresa))
            {
                // Validación de duplicado si es insert
                if (esInsert)
                {
                    using (SqlCommand cmdDup = new SqlCommand("SELECT COUNT(*) FROM TiposProducto WHERE tipo = @tipo", cn))
                    {
                        cmdDup.CommandType = CommandType.Text;
                        cmdDup.CommandTimeout = conn.TimeOut();
                        cmdDup.Parameters.AddWithValue("@tipo", tiposProducto);

                        int existe = Convert.ToInt32(cmdDup.ExecuteScalar());
                        if (existe != 0)
                            return "Ya existe un Tipo con el mismo nombre.";
                    }
                }

                string query = esInsert
                    ? "INSERT INTO TiposProducto (tipo, orden, reservadoSistema, creado) VALUES (@tipo, @orden, @reservadoSistema, @creado)"
                    : "UPDATE TiposProducto SET tipo = @tipo, orden = @orden, actualizado = @actualizado WHERE tipo LIKE @tipoToUpdate; " +
                      "UPDATE Corte SET tipo = @tipo WHERE tipo LIKE @tipoToUpdate;";

                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = conn.TimeOut();

                    cmd.Parameters.AddWithValue("@tipo", tiposProducto);
                    cmd.Parameters.AddWithValue("@tipoToUpdate", tipoToUpdate ?? "");
                    cmd.Parameters.AddWithValue("@orden", orden);
                    cmd.Parameters.AddWithValue("@reservadoSistema", false);
                    cmd.Parameters.AddWithValue("@creado", DateTime.Now);
                    cmd.Parameters.AddWithValue("@actualizado", DateTime.Now);

                    cmd.ExecuteNonQuery();
                }
            }

            return mensaje;
        }

        public string eliminarTipoProducto(string tiposProducto)
        {
            using (SqlConnection cn = conn.conectar(_empresa))
            {
                // verifica si hay cortes con ese tipo
                using (SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(*) FROM Corte WHERE tipo = @tipo", cn))
                {
                    cmdCheck.CommandType = CommandType.Text;
                    cmdCheck.CommandTimeout = conn.TimeOut();
                    cmdCheck.Parameters.AddWithValue("@tipo", tiposProducto);

                    int existe = Convert.ToInt32(cmdCheck.ExecuteScalar());
                    if (existe != 0)
                        return "Existen Productos/Cortes con el Tipo que quiere eliminar.\n\nPara poder eliminar el Tipo debe cambiar todo los Productos/Cortes asociados a éste.";
                }

                using (SqlCommand cmd = new SqlCommand("DELETE FROM TiposProducto WHERE tipo = @tipo", cn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = conn.TimeOut();
                    cmd.Parameters.AddWithValue("@tipo", tiposProducto);
                    cmd.ExecuteNonQuery();
                }
            }

            return "";
        }

        public long sugerirCodigo(string tipo)
        {
            long codigoDisponible = -1;

            string query = @"
                SELECT MIN(Codigo + 1) AS CodigoDisponible
                FROM Corte 
                WHERE Tipo = @tipo
                AND NOT EXISTS (
                    SELECT 1 FROM Corte c2 
                    WHERE c2.Codigo = Corte.Codigo + 1 AND c2.Tipo = @tipo
                );";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@tipo", tipo);

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    codigoDisponible = Convert.ToInt64(result);
            }

            return codigoDisponible;
        }

        public int obtenerNivelCorte(int idCorteMaestro)
        {
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerNivelCorte", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idCorteMaestro", idCorteMaestro);

                object result = cmd.ExecuteScalar();
                return (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
            }
        }
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
//using System.Security.Cryptography;

//namespace Datos
//{
//    public class Corte
//    {
//        private SqlCommand cmCorte;
//        private SqlDataAdapter daCorte;

//        Utilidades.Conexion conn;
//        private readonly IEmpresaContext _empresa;
//        public Corte(IEmpresaContext empresa)
//        {
//            _empresa = empresa ??
//                throw new ArgumentNullException(nameof(empresa));

//            conn = new Utilidades.Conexion();
//        }


//        private Entidades.Corte MapCorte(SqlDataReader drCorte, bool cargarMaestro)
//        {
//            Entidades.Corte oCorteE = new Entidades.Corte();

//            oCorteE.IdCorte = Convert.ToInt32(drCorte["idCorte"]);
//            oCorteE.Codigo = Convert.ToInt64(drCorte["codigo"]);
//            oCorteE.CorteDesc = Convert.ToString(drCorte["corte"]);
//            if (drCorte["idMarca"] != DBNull.Value)
//            {
//                Datos.Persona oPersonaD = new Datos.Persona(_empresa);
//                oCorteE.Marca = oPersonaD.findById(Convert.ToInt32(drCorte["idMarca"]));
//            }
//            oCorteE.Tipo = Convert.ToString(drCorte["tipo"]);
//            oCorteE.Promedio = float.Parse(drCorte["promedio"].ToString());
//            oCorteE.PuntoStock = Convert.ToInt32(drCorte["puntoStock"]);
//            oCorteE.Nivel = Convert.ToInt32(drCorte["nivel"]);
//            if (cargarMaestro)
//            {
//                oCorteE.CorteMaestro = findCorteById(Convert.ToInt32(drCorte["idCorteMaestro"]), false);

//            }
//            oCorteE.Porcentaje = float.Parse(drCorte["porcentaje"].ToString());
//            oCorteE.PrecioKg = float.Parse(drCorte["precioKg"].ToString());
//            oCorteE.PrecioKgReferencia = float.Parse(drCorte["precioKg"].ToString());
//            oCorteE.IngresoRapidoEmbutido = Convert.ToBoolean(drCorte["ingresoRapidoEmbutido"]);
//            oCorteE.Habilitado = Convert.ToBoolean(drCorte["habilitado"]);
//            oCorteE.EnCierreStock = Convert.ToBoolean(drCorte["enCierreStock"]);
//            oCorteE.PorcentajeHueso = float.Parse(drCorte["porcentajeHueso"].ToString());
//            oCorteE.Independiente = Convert.ToInt32(drCorte["independiente"]);
//            oCorteE.DesvioEstandar = float.Parse(drCorte["desvioEstandar"].ToString());
//            oCorteE.Creado = Convert.ToDateTime(drCorte["creado"]);
//            oCorteE.Actualizado = drCorte["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drCorte["actualizado"]);
//            oCorteE.IdAlicuotaIva = Convert.ToInt32(drCorte["idAlicuotaIva"]);
//            oCorteE.AlicuotaIva = float.Parse(drCorte["alicuotaIva"].ToString());
//            oCorteE.Pesable = Convert.ToBoolean(drCorte["pesable"]);

//            ///se valida si es presentacion
//            oCorteE.Presentacion = oCorteE.EsPresentacion(oCorteE.porcentajeHueso);
//            if (oCorteE.Presentacion)
//            {
//                oCorteE.porcentaje = oCorteE.getCantPresentacion(oCorteE.porcentajeHueso);
//            }

//            return oCorteE;
//            //idCorte = Convert.ToInt32(dr["idCorte"]),
//            //codigo = Convert.ToInt64(dr["codigo"]),
//            //corte = dr["corte"].ToString(),
//            //tipo = dr["tipo"].ToString(),
//            //Promedio = float.Parse(dr["promedio"].ToString()),
//            //PuntoStock = Convert.ToInt32(dr["puntoStock"]),
//            //Nivel = Convert.ToInt32(dr["nivel"]),
//            //CorteMaestro = buscarMaestro && dr["idCorteMaestro"] != DBNull.Value
//            //    ? findCorteById(Convert.ToInt32(dr["idCorteMaestro"]), false)
//            //    : null,
//            //precioKg = float.Parse(dr["precioKg"].ToString()),
//            //IngresoRapidoEmbutido = Convert.ToBoolean(dr["ingresoRapidoEmbutido"]),
//            //EnCierreStock = Convert.ToBoolean(dr["enCierreStock"]),
//            //independiente = Convert.ToInt32(dr["independiente"]),
//            //porcentaje = float.Parse(dr["porcentaje"].ToString()),
//            //desvioEstandar = float.Parse(dr["desvioEstandar"].ToString()),
//            //porcentajeHueso = float.Parse(dr["porcentajeHueso"].ToString()),
//            //Creado = Convert.ToDateTime(dr["creado"]),
//            //Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"]),
//            //IdAlicuotaIva = Convert.ToInt32(dr["idAlicuotaIva"]),
//            //AlicuotaIva = float.Parse(dr["alicuotaIva"].ToString()),
//            //Pesable = Convert.ToBoolean(dr["pesable"])
//        }
//        public List<Entidades.Corte> findAllCortes(bool buscarMaestro)
//        {
//            var lista = new List<Entidades.Corte>();

//            using (var connSql = conn.conectar(_empresa))
//            using (var cmd = new SqlCommand("SELECT * FROM Corte ORDER BY codigo ASC", connSql))
//            {
//                cmd.CommandType = CommandType.Text;
//                cmd.CommandTimeout = conn.TimeOut();

//                connSql.Open();
//                using (var dr = cmd.ExecuteReader())
//                {
//                    while (dr.Read())
//                    {
//                        lista.Add(MapCorte(dr, buscarMaestro));
//                    }
//                }
//            }

//            return lista;
//        }

//        public Entidades.Corte findCorteById(int idCorte, bool buscarMaestro)
//        {
//            using (var connSql = conn.conectar(_empresa))
//            using (var cmd = new SqlCommand("SELECT * FROM Corte WHERE idCorte = @idCorte", connSql))
//            {
//                cmd.Parameters.AddWithValue("@idCorte", idCorte);
//                cmd.CommandType = CommandType.Text;
//                cmd.CommandTimeout = conn.TimeOut();

//                connSql.Open();
//                using (var dr = cmd.ExecuteReader())
//                {
//                    if (dr.Read())
//                    {
//                        return MapCorte(dr, buscarMaestro);
//                    }
//                }
//            }

//            return null; // No se encontró
//        }
//        public Entidades.Corte findCorteByCodigo(Int64 codigo, bool buscarMaestro)
//        {
//            using (var connSql = conn.conectar(_empresa))
//            using (var cmd = new SqlCommand("SELECT * FROM Corte WHERE codigo = @codigo", connSql))
//            {
//                cmd.Parameters.AddWithValue("@codigo", codigo);
//                cmd.CommandType = CommandType.Text;
//                cmd.CommandTimeout = conn.TimeOut();

//                connSql.Open();
//                using (var dr = cmd.ExecuteReader())
//                {
//                    if (dr.Read())
//                    {
//                        return MapCorte(dr, buscarMaestro);
//                    }
//                }
//            }

//            return null; // No se encontró
//        }


//        //public Entidades.Corte getCorteById(int id, bool cargarMaestro)
//        //{
//        //    cmCorte = new SqlCommand();
//        //    cmCorte.Connection = conn.conectar(_empresa);
//        //    cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();
//        //    cmCorte.CommandText = "Select Corte.* from Corte where idCorte = " + id;
//        //    Entidades.Corte oCorteE = new Entidades.Corte();
//        //    try
//        //    {
//        //        cmCorte.Connection.Open();
//        //        SqlDataReader drCorte = cmCorte.ExecuteReader();
//        //        using (drCorte)
//        //        {
//        //            while (drCorte.Read())
//        //            {
//        //                oCorteE.IdCorte = Convert.ToInt32(drCorte["idCorte"]);
//        //                oCorteE.Codigo = Convert.ToInt64(drCorte["codigo"]);
//        //                oCorteE.CorteDesc = Convert.ToString(drCorte["corte"]);
//        //                if (drCorte["idMarca"] != DBNull.Value)
//        //                {
//        //                    Datos.Persona oPersonaD = new Datos.Persona();
//        //                    oCorteE.Marca = oPersonaD.findById(Convert.ToInt32(drCorte["idMarca"]));
//        //                }
//        //                oCorteE.Tipo = Convert.ToString(drCorte["tipo"]);
//        //                oCorteE.Promedio = float.Parse(drCorte["promedio"].ToString());
//        //                oCorteE.PuntoStock = Convert.ToInt32(drCorte["puntoStock"]);
//        //                oCorteE.Nivel = Convert.ToInt32(drCorte["nivel"]);
//        //                if (cargarMaestro)
//        //                {
//        //                    oCorteE.CorteMaestro = getCorteById(Convert.ToInt32(drCorte["idCorteMaestro"]), false);

//        //                }
//        //                oCorteE.Porcentaje = float.Parse(drCorte["porcentaje"].ToString());
//        //                oCorteE.PrecioKg = float.Parse(drCorte["precioKg"].ToString());
//        //                oCorteE.PrecioKgReferencia = float.Parse(drCorte["precioKg"].ToString());
//        //                oCorteE.IngresoRapidoEmbutido = Convert.ToBoolean(drCorte["ingresoRapidoEmbutido"]);
//        //                oCorteE.Habilitado = Convert.ToBoolean(drCorte["habilitado"]);
//        //                oCorteE.EnCierreStock = Convert.ToBoolean(drCorte["enCierreStock"]);
//        //                oCorteE.PorcentajeHueso = float.Parse(drCorte["porcentajeHueso"].ToString());
//        //                oCorteE.Independiente = Convert.ToInt32(drCorte["independiente"]);
//        //                oCorteE.DesvioEstandar = float.Parse(drCorte["desvioEstandar"].ToString());
//        //                oCorteE.Creado = Convert.ToDateTime(drCorte["creado"]);
//        //                oCorteE.Actualizado = drCorte["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drCorte["actualizado"]);
//        //                oCorteE.IdAlicuotaIva = Convert.ToInt32(drCorte["idAlicuotaIva"]);
//        //                oCorteE.AlicuotaIva = float.Parse(drCorte["alicuotaIva"].ToString());
//        //                oCorteE.Pesable = Convert.ToBoolean(drCorte["pesable"]);

//        //                ///se valida si es presentacion
//        //                oCorteE.Presentacion = oCorteE.EsPresentacion(oCorteE.porcentajeHueso);
//        //                if (oCorteE.Presentacion)
//        //                {
//        //                    oCorteE.porcentaje = oCorteE.getCantPresentacion(oCorteE.porcentajeHueso);
//        //                }
//        //            }
//        //            return oCorteE;
//        //        }
//        //    }
//        //    finally
//        //    {
//        //        cmCorte.Connection.Close();
//        //        oCorteE = null;
//        //    }
//        //}

//        public void editPrecioCorte(Entidades.Corte oCorteE)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();            
//            cmCorte.CommandText = "UPDATE Corte SET precioKg = @precioKg WHERE idCorte = "+oCorteE.idCorte;
//            cmCorte.Parameters.AddWithValue("@precioKg", oCorteE.precioKg);

//            cmCorte.ExecuteNonQuery();
//            cmCorte.Connection.Close();

//            cmCorte = null;
//        }

//        public void addOrEditCorte(Entidades.Corte oCorteE)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "addOrEditCorte";

//            cmCorte.Parameters.AddWithValue("@idCorte", oCorteE.idCorte);
//            cmCorte.Parameters.AddWithValue("@codigo", oCorteE.codigo);
//            cmCorte.Parameters.AddWithValue("@corte", oCorteE.corte);
//            cmCorte.Parameters.AddWithValue("@tipo", oCorteE.tipo);
//            cmCorte.Parameters.AddWithValue("@idMarca", oCorteE.Marca != null ? oCorteE.Marca.IdPersona : 0);
//            cmCorte.Parameters.AddWithValue("@puntoStock", oCorteE.PuntoStock);
//            cmCorte.Parameters.AddWithValue("@promedio", oCorteE.Promedio);
//            cmCorte.Parameters.AddWithValue("@independiente", oCorteE.independiente);
//            cmCorte.Parameters.AddWithValue("@precioKg", oCorteE.precioKg);
//            cmCorte.Parameters.AddWithValue("@ingresoRapidoEmbutido", oCorteE.IngresoRapidoEmbutido);
//            cmCorte.Parameters.AddWithValue("@habilitado", oCorteE.Habilitado);
//            cmCorte.Parameters.AddWithValue("@enCierreStock", oCorteE.EnCierreStock);
//            cmCorte.Parameters.AddWithValue("@idCorteMaestro", oCorteE.corteMaestro != null ? oCorteE.corteMaestro.idCorte : 0);
//            cmCorte.Parameters.AddWithValue("@porcentaje", oCorteE.porcentaje);
//            cmCorte.Parameters.AddWithValue("@porcentajeHueso", oCorteE.porcentajeHueso);
//            cmCorte.Parameters.AddWithValue("@desvioEstandar", oCorteE.desvioEstandar);
//            cmCorte.Parameters.AddWithValue("@idAlicuotaIva", oCorteE.IdAlicuotaIva);
//            cmCorte.Parameters.AddWithValue("@alicuotaIva", oCorteE.AlicuotaIva);
//            cmCorte.Parameters.AddWithValue("@pesable", oCorteE.Pesable);

//            cmCorte.ExecuteNonQuery();
//            cmCorte.Connection.Close();

//            cmCorte = null;
//        }

//        public DataTable buscarCorte(string txtBusqueda)
//        {
//            DataTable dtCortes = new DataTable();

//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "buscarCorte";
//            cmCorte.Parameters.AddWithValue("@texto", txtBusqueda);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtCortes);
//            cmCorte.Connection.Close();

//            return dtCortes;
//        }

//        public DataTable buscarCorteSinMaestro(string txtBusqueda)
//        {
//            DataTable dtCortes = new DataTable();

//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "buscarCorteSinMaestro";
//            cmCorte.Parameters.AddWithValue("@texto", txtBusqueda);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtCortes);
//            cmCorte.Connection.Close();

//            return dtCortes;
//        }

//        public DataTable buscarCodigoCorte(long codigo)
//        {
//            DataTable dtCortes = new DataTable();

//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "buscarCodigoCorte";
//            cmCorte.Parameters.AddWithValue("@codigo", codigo);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtCortes);
//            cmCorte.Connection.Close();

//            return dtCortes;
//        }

//        public void eliminarCorte(Entidades.Corte oCorteE)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "EliminarCorte";

//            cmCorte.Parameters.AddWithValue("@idCorte", oCorteE.idCorte);

//            cmCorte.ExecuteNonQuery();
//            cmCorte.Connection.Close();

//            cmCorte = null;
//        }

//        public DataTable obtenerCortes()
//        {
//            DataTable dtCortes=new DataTable();
//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            string consultaSQL = "SELECT     CorteP.idCorte, CorteP.codigo, CorteP.corte, CorteP.precioKg, CorteP.ingresoRapidoEmbutido, CorteP.enCierreStock, " +
//                " CorteP.tipo, CorteP.pesable, CorteP.nivel, CorteP.idCorteMaestro, CorteM.corte AS corteMaestro, CorteP.porcentaje, CorteP.porcentajeHueso, CorteP.desvioEstandar, " +
//                " CorteP.independiente, CorteP.promedio, CorteP.idAlicuotaIva, CorteP.alicuotaIva FROM  dbo.Corte AS CorteM RIGHT OUTER JOIN " +
//                " dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro";
//            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = consultaSQL;

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtCortes);

//            cmCorte.Connection.Close();            
//            return dtCortes;
//        }
//        public DataTable cargarDtCortes()
//        {
//            DataTable dtCortes = new DataTable();
//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            string consultaSQL = "SELECT * FROM Corte";
//            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = consultaSQL;

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtCortes);

//            cmCorte.Connection.Close();
//            return dtCortes;
//        }

//        public DataTable obtenerEmbutidos(string txtBusqueda)
//        {
//            DataTable dtCortes = new DataTable();
//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "obtenerEmbutidos";
//            cmCorte.Parameters.AddWithValue("@texto", txtBusqueda);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtCortes);

//            cmCorte.Connection.Close();

//            return dtCortes;

//        }

//        public DataTable getListaElegirEmbutido()
//        {
//            DataTable dtCortes = new DataTable();
//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; 
//            cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "getListaElegirEmbutido";

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtCortes);

//            cmCorte.Connection.Close();

//            return dtCortes;

//        }

//        public DataTable buscarEmbutido(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta)
//        {
//            DataTable dtCortes = new DataTable();
//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "buscarEmbutido";
//            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
//            cmCorte.Parameters.AddWithValue("@texto",texto);
//            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);

//            cmCorte.Connection.Open();
//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtCortes);
//            cmCorte.Connection.Close();

//            return dtCortes;
//        }

//        public DataTable obtenerLineasEmb(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta)
//        {
//            DataTable dtCortes = new DataTable();
//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "obtenerLineasEmb";
//            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
//            cmCorte.Parameters.AddWithValue("@texto", texto);
//            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);

//            cmCorte.Connection.Open();
//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtCortes);
//            cmCorte.Connection.Close();

//            return dtCortes;
//        }

//        public DataTable obtenerInfoCorte(int idCorte)
//        {
//            DataTable dtCorte = new DataTable();

//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "obtenerInfoCorte";
//            cmCorte.Parameters.AddWithValue("@idCorte", idCorte);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtCorte);

//            cmCorte.Connection.Close();

//            return dtCorte;
//        }


//        //public List<Entidades.Corte> findAllCortes(bool buscarMaestro)
//        //{
//        //    cmCorte = new SqlCommand();
//        //    cmCorte.Connection = conn.conectar(_empresa);
//        //    cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();
//        //    cmCorte.CommandText = "Select Corte.* from Corte order by codigo asc";
//        //    List<Entidades.Corte> listaCortes = new List<Entidades.Corte>();
//        //    Entidades.Corte oCorteE;// = new Entidades.Corte();
//        //    try
//        //    {
//        //        cmCorte.Connection.Open();
//        //        SqlDataReader drCorte = cmCorte.ExecuteReader();

//        //        using (drCorte)
//        //        {
//        //            while (drCorte.Read())
//        //            {
//        //                oCorteE = new Entidades.Corte();
//        //                oCorteE.idCorte = Convert.ToInt32(drCorte["idCorte"].ToString());
//        //                oCorteE.codigo = Convert.ToInt64(drCorte["codigo"].ToString());
//        //                oCorteE.corte = drCorte["corte"].ToString();
//        //                oCorteE.tipo = drCorte["tipo"].ToString();
//        //                oCorteE.Promedio = float.Parse(drCorte["promedio"].ToString());
//        //                oCorteE.PuntoStock = Convert.ToInt32(drCorte["puntoStock"]);
//        //                oCorteE.Nivel = Convert.ToInt32(drCorte["nivel"]);
//        //                oCorteE.CorteMaestro = buscarMaestro ? findCorteById(Convert.ToInt32(drCorte["idCorteMaestro"].ToString()), false) : null;
//        //                oCorteE.precioKg = float.Parse(drCorte["precioKg"].ToString());
//        //                oCorteE.IngresoRapidoEmbutido = Convert.ToBoolean(drCorte["ingresoRapidoEmbutido"]);
//        //                oCorteE.EnCierreStock = Convert.ToBoolean(drCorte["enCierreStock"]);
//        //                oCorteE.independiente = Convert.ToInt32(drCorte["independiente"].ToString());
//        //                oCorteE.porcentaje = float.Parse(drCorte["porcentaje"].ToString());
//        //                oCorteE.desvioEstandar = float.Parse(drCorte["desvioEstandar"].ToString());
//        //                oCorteE.porcentajeHueso = float.Parse(drCorte["porcentajeHueso"].ToString());
//        //                oCorteE.Creado = Convert.ToDateTime(drCorte["creado"]);
//        //                oCorteE.Actualizado = drCorte["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drCorte["actualizado"]);
//        //                oCorteE.IdAlicuotaIva = Convert.ToInt32(drCorte["idAlicuotaIva"]);
//        //                oCorteE.AlicuotaIva = float.Parse(drCorte["alicuotaIva"].ToString());
//        //                oCorteE.Pesable = Convert.ToBoolean(drCorte["pesable"]);

//        //                listaCortes.Add(oCorteE);
//        //            }
//        //            return listaCortes;
//        //        }
//        //    }
//        //    finally
//        //    {
//        //        cmCorte.Connection.Close();
//        //        oCorteE = null;
//        //    }
//        //}

//        //public Entidades.Corte findCorteById(int idCorte, bool buscarMaestro)
//        //{
//        //    cmCorte = new SqlCommand();
//        //    cmCorte.Connection = conn.conectar(_empresa);
//        //    cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();
//        //    cmCorte.CommandText = "Select Corte.* from Corte where idCorte =" + idCorte;

//        //    Entidades.Corte oCorteE = new Entidades.Corte();
//        //    try
//        //    {
//        //        cmCorte.Connection.Open();
//        //        SqlDataReader drCorte = cmCorte.ExecuteReader();

//        //        using (drCorte)
//        //        {
//        //            while (drCorte.Read())
//        //            {
//        //                oCorteE.idCorte = Convert.ToInt32(drCorte["idCorte"].ToString());
//        //                oCorteE.codigo = Convert.ToInt64(drCorte["codigo"].ToString());
//        //                oCorteE.corte = drCorte["corte"].ToString();
//        //                oCorteE.tipo = drCorte["tipo"].ToString();
//        //                oCorteE.Promedio = float.Parse(drCorte["promedio"].ToString());
//        //                oCorteE.PuntoStock = Convert.ToInt32(drCorte["puntoStock"]);
//        //                oCorteE.Nivel = Convert.ToInt32(drCorte["nivel"]);
//        //                oCorteE.CorteMaestro = buscarMaestro ? findCorteById(Convert.ToInt32(drCorte["idCorteMaestro"].ToString()), false) : null;
//        //                oCorteE.precioKg = float.Parse(drCorte["precioKg"].ToString());
//        //                oCorteE.IngresoRapidoEmbutido = Convert.ToBoolean(drCorte["ingresoRapidoEmbutido"]);
//        //                oCorteE.EnCierreStock = Convert.ToBoolean(drCorte["enCierreStock"]);
//        //                oCorteE.independiente = Convert.ToInt32(drCorte["independiente"].ToString());
//        //                oCorteE.porcentaje = float.Parse(drCorte["porcentaje"].ToString());
//        //                oCorteE.desvioEstandar = float.Parse(drCorte["desvioEstandar"].ToString());
//        //                oCorteE.porcentajeHueso = float.Parse(drCorte["porcentajeHueso"].ToString());
//        //                oCorteE.Creado = Convert.ToDateTime(drCorte["creado"]);
//        //                oCorteE.Actualizado = drCorte["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drCorte["actualizado"]);
//        //                oCorteE.IdAlicuotaIva = Convert.ToInt32(drCorte["idAlicuotaIva"]);
//        //                oCorteE.AlicuotaIva = float.Parse(drCorte["alicuotaIva"].ToString());
//        //                oCorteE.Pesable = Convert.ToBoolean(drCorte["pesable"]);
//        //            }
//        //            return oCorteE;
//        //        }
//        //    }
//        //    finally
//        //    {
//        //        cmCorte.Connection.Close();
//        //        oCorteE = null;
//        //    }
//        //}

//        public DataTable obtenerCorteProveedor(int idCorte)
//        {
//            DataTable dtCorteProveedor = new DataTable();

//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "SELECT  "+
//                " dbo.Personas.razonSocial, dbo.CorteProveedor.ultimoPrecio, dbo.CorteProveedor.fechaUltimaCompra"+
//                "  FROM dbo.Corte INNER JOIN dbo.CorteProveedor ON dbo.Corte.idCorte = dbo.CorteProveedor.idCorte INNER JOIN "+"" +
//                " dbo.Personas ON dbo.CorteProveedor.idProveedor = dbo.Personas.idPersona "+
//                " WHERE  (dbo.CorteProveedor.idCorte = @idCorte)"+
//                " ORDER By dbo.CorteProveedor.fechaUltimaCompra desc";
//            cmCorte.Parameters.AddWithValue("@idCorte", idCorte);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtCorteProveedor);

//            cmCorte.Connection.Close();

//            return dtCorteProveedor;
//        }

//        #region formula 
//        public DataTable buscarFormula(string texto)
//        {
//            DataTable dtFormulas = new DataTable();
//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();

//            string consulta = "SELECT DISTINCT dbo.Formulas.idFormula, dbo.Corte.codigo, dbo.Corte.corte, dbo.Formulas.creado, dbo.Formulas.actualizado FROM" +
//                " dbo.Corte INNER JOIN  dbo.Formulas ON dbo.Corte.idCorte = dbo.Formulas.idEmbutido " +
//                " Where dbo.Corte.corte like '%" + texto + "%' "+// or dbo.Corte.codigo = " + texto + ""+
//                " ORDER BY dbo.Corte.codigo ";  //dbo.Corte.corte  like '%\" + texto + \"%'";
//            cmCorte.CommandText = consulta;
//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtFormulas);
//            cmCorte.Parameters.AddWithValue("@texto", texto);

//            return dtFormulas;
//        }
//        /// <summary>
//        /// Busca formula segun el ID por parámetro
//        /// </summary>
//        /// <param name="idFormula"></param>
//        /// <param name="idEmbutido"></param>
//        /// <returns></returns>
//        public Entidades.Formula findFormulaByID(int idFormula, int idEmbutido)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();

//            cmCorte.CommandText = idFormula > 0 ?
//                "Select Formulas.* from Formulas where idFormula =" + idFormula :
//                "Select Formulas.* from Formulas where idEmbutido =" + idEmbutido;

//            SqlDataReader drFormula = cmCorte.ExecuteReader();

//            Entidades.Formula oFormula = new Entidades.Formula();
//            while (drFormula.Read())
//            {
//                oFormula.IdFormula = Convert.ToInt32(drFormula["idFormula"].ToString());
//                oFormula.Embutido = findCorteById(Convert.ToInt32(drFormula["idEmbutido"].ToString()), false);
//                oFormula.Receta = drFormula["receta"].ToString();


//                oFormula.Creado = Convert.ToDateTime(drFormula["creado"]);
//                oFormula.Actualizado = drFormula["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drFormula["actualizado"]);

//                Datos.Usuario oUsuarioD = new Usuario(_empresa);
//                oFormula.CreadoPor = string.IsNullOrEmpty(drFormula["creadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drFormula["creadoPor"]));
//                oFormula.ActualizadoPor = string.IsNullOrEmpty(drFormula["actualizadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drFormula["actualizadoPor"]));

//                oFormula.ListaCortesEnFormula = cargarCortesPorFormula(oFormula);
//            }
//            cmCorte.Connection.Close();
//            return oFormula;
//        }
//        public List<Entidades.CortePorFormula> cargarCortesPorFormula(Entidades.Formula oFormula)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "SELECT * FROM CortePorFormula WHERE idFormula = " + oFormula.IdFormula;

//            List<Entidades.CortePorFormula> listaCortesPorFormula = new List<Entidades.CortePorFormula>();
//            SqlDataReader drFormula = cmCorte.ExecuteReader();

//            while (drFormula.Read())
//            {
//                Entidades.CortePorFormula oCortePorFormula = new Entidades.CortePorFormula();

//                oCortePorFormula.IdCorteEnFormula = Convert.ToInt32(drFormula["idCortePorFormula"].ToString());
//                oCortePorFormula.Formula = oFormula;
//                oCortePorFormula.CorteEnFormula = findCorteById(Convert.ToInt32(drFormula["idCorte"].ToString()),false);
//                oCortePorFormula.Porcentaje = float.Parse(drFormula["porcentaje"].ToString());
//                oCortePorFormula.AgregarAuto = Convert.ToBoolean(drFormula["agregarAuto"].ToString());

//                listaCortesPorFormula.Add(oCortePorFormula);
//                oCortePorFormula = null;
//            }
//            cmCorte.Connection.Close();
//            return listaCortesPorFormula;
//        }


//        public int existeFormula(int idEmbutido)
//        {
//            int idFormula = 0;
//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.CommandType = CommandType.Text;
//            cmCorte.CommandText = "Select idFormula from Formulas where idEmbutido = " + idEmbutido;
//            try
//            {
//                cmCorte.Connection.Open();
//                SqlDataReader drCorte = cmCorte.ExecuteReader();
//                using (drCorte)
//                {
//                    while (drCorte.Read())
//                    {
//                        idFormula = Convert.ToInt32(drCorte["idFormula"]);
//                    }
//                    return idFormula;
//                }
//            }
//            finally
//            {
//                cmCorte.Connection.Close();
//            }
//        }

//        public int addOrEditFormula(Entidades.Formula oFormula, List<Entidades.CortePorFormula> listaCortesPorFormula)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure;
//            cmCorte.CommandText = "addOrEditFormula";
//            cmCorte.Parameters.AddWithValue("@idFormula", oFormula.IdFormula);
//            cmCorte.Parameters.AddWithValue("@idEmbutido", oFormula.Embutido.idCorte); 
//            cmCorte.Parameters.AddWithValue("@receta", oFormula.Receta);
//            cmCorte.Parameters.AddWithValue("@creadoPor", oFormula.CreadoPor.Id);
//            cmCorte.Parameters.AddWithValue("@actualizadoPor", oFormula.ActualizadoPor!=null ? oFormula.ActualizadoPor.Id : 0);

//            oFormula.IdFormula = (int)cmCorte.ExecuteScalar();

//            cmCorte.CommandText = "agregarCortePorFormula";
//            cmCorte.Parameters.Clear();
//            foreach (Entidades.CortePorFormula item in listaCortesPorFormula)
//            {
//                cmCorte.Parameters.AddWithValue("@idFormula", oFormula.IdFormula);
//                cmCorte.Parameters.AddWithValue("@idCorte", item.CorteEnFormula1.idCorte );
//                cmCorte.Parameters.AddWithValue("@porcentaje", item.Porcentaje);
//                cmCorte.Parameters.AddWithValue("@agregarAuto", item.AgregarAuto);
//                cmCorte.ExecuteNonQuery();

//                cmCorte.Parameters.Clear();
//            }

//            cmCorte.Connection.Close();

//            return oFormula.IdFormula;
//        }

//        public void eliminarFormula(int idFormula)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);

//            cmCorte.Connection.Open();

//            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "DELETE FROM  CortePorFormula WHERE idFormula = " + idFormula;
//            cmCorte.ExecuteNonQuery();

//            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "DELETE FROM  Formulas WHERE idFormula = " + idFormula;
//            cmCorte.ExecuteNonQuery();

//            cmCorte.Connection.Close();
//        }

//        public DataTable getFormulaEmbutido(int idEmbutido)
//        {
//            DataTable dtFormula = new DataTable();
//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);

//            cmCorte.CommandType = CommandType.Text;
//            cmCorte.CommandText = "SELECT dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.CortePorFormula.porcentaje, '' AS 'kgs', dbo.CortePorFormula.agregarAuto " +
//                "FROM  dbo.Formulas INNER JOIN  dbo.CortePorFormula ON dbo.Formulas.idFormula = dbo.CortePorFormula.idFormula INNER JOIN "+
//                "dbo.Corte ON dbo.CortePorFormula.idCorte = dbo.Corte.idCorte WHERE  dbo.Formulas.idEmbutido = " + idEmbutido + 
//                " ORDER BY dbo.CortePorFormula.agregarAuto desc";

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtFormula);

//            cmCorte.Connection.Close();
//            return dtFormula;
//        }

//        public DataTable obtenerTiposProducto(bool mostrarTodos)
//        {
//            DataTable dtTiposProducto = new DataTable();
//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();
//            string consulta = "SELECT  tipo FROM  TiposProducto";
//            consulta += mostrarTodos ? " ORDER BY orden, tipo" : " where orden > 0 ORDER BY orden, tipo";
//            cmCorte.CommandText = consulta;

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtTiposProducto);

//            cmCorte.Connection.Close();

//            return dtTiposProducto;
//        }


//        #endregion

//        #region Alicuota Iva
//        public DataTable obtenerAlicuotasIva(bool mostrarTodos)
//        {
//            DataTable dtAlicuotasIva = new DataTable();
//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();
//            string consulta = "Select idIva, iva from AlicuotasIva";
//            consulta += mostrarTodos ? " order by orden" : " where mostrar = 1 order by orden";
//            cmCorte.CommandText =  consulta;

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtAlicuotasIva);

//            cmCorte.Connection.Close();

//            return dtAlicuotasIva;
//        }

//        public Entidades.AlicuotaIva findAlicuotaIvaById(int idIva)
//        {
//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "Select * from AlicuotasIva where idIva =" + idIva;

//            Entidades.AlicuotaIva oAlicuotaIvaE = new Entidades.AlicuotaIva();
//            try
//            {
//                cmCorte.Connection.Open();
//                SqlDataReader drCorte = cmCorte.ExecuteReader();

//                using (drCorte)
//                {
//                    while (drCorte.Read())
//                    {
//                        oAlicuotaIvaE.IdIva = Convert.ToInt32(drCorte["idIva"].ToString());
//                        oAlicuotaIvaE.Iva = Convert.ToInt32(drCorte["iva"].ToString());
//                        oAlicuotaIvaE.Orden = Convert.ToInt32(drCorte["orden"].ToString());
//                        oAlicuotaIvaE.Mostrar = bool.Parse(drCorte["mostrar"].ToString());
//                    }
//                    return oAlicuotaIvaE;
//                }
//            }
//            finally
//            {
//                cmCorte.Connection.Close();
//                oAlicuotaIvaE = null;
//            }
//        }
//        #endregion

//        #region Embutidos

//        public Entidades.Embutido findEmbutidoById(int idEmbutido)
//        {
//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "Select Embutidos.* from Embutidos where idEmbutido =" + idEmbutido;

//            Entidades.Embutido oEmbutidoE = new Entidades.Embutido();

//            try
//            {
//                cmCorte.Connection.Open();
//                SqlDataReader drEmbutido = cmCorte.ExecuteReader();

//                using (drEmbutido)
//                {
//                    while (drEmbutido.Read())
//                    {
//                        oEmbutidoE.IdEmbutido = Convert.ToInt32(drEmbutido["idEmbutido"]);
//                        oEmbutidoE.FechaEmbutido = Convert.ToDateTime(drEmbutido["fechaEmbutido"]);
//                        oEmbutidoE.Corte = findCorteById(Convert.ToInt32(drEmbutido["idCorte"]), true);
//                        Datos.Sucursal oSucursalD = new Sucursal(_empresa);
//                        oEmbutidoE.Sucursal = oSucursalD.findById(Convert.ToInt32(drEmbutido["idSucursal"]));
//                        oEmbutidoE.Observaciones = Convert.ToString(drEmbutido["observaciones"]);
//                        oEmbutidoE.Estado = Convert.ToString(drEmbutido["estado"]);
//                        oEmbutidoE.Creado = Convert.ToDateTime(drEmbutido["creado"]);
//                        oEmbutidoE.Actualizado = drEmbutido["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drEmbutido["actualizado"]);

//                        Datos.Usuario oUsuarioD = new Usuario(_empresa);
//                        oEmbutidoE.CreadoPor = string.IsNullOrEmpty(drEmbutido["creadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drEmbutido["creadoPor"]));
//                        oEmbutidoE.ActualizadoPor = string.IsNullOrEmpty(drEmbutido["actualizadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drEmbutido["actualizadoPor"]));

//                        oEmbutidoE.CortesEnEmbutido = obtenerCortesEnEmbutido(oEmbutidoE);
//                    }
//                    return oEmbutidoE;
//                }
//            }
//            finally
//            {
//                cmCorte.Connection.Close();
//                oEmbutidoE = null;
//            }
//        }

//        public List<Entidades.CortePorEmbutido> obtenerCortesEnEmbutido(Entidades.Embutido oEmbutidoParam)
//        {
//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "Select CortePorEmbutido.* from CortePorEmbutido where idEmbutido =" + oEmbutidoParam.idEmbutido;

//            List<Entidades.CortePorEmbutido> cortesEnEmbutido = new List<Entidades.CortePorEmbutido>();
//            Entidades.CortePorEmbutido oCorteEnEmbutido;
//            try
//            {
//                cmCorte.Connection.Open();
//                SqlDataReader drEmbutido = cmCorte.ExecuteReader();

//                using (drEmbutido)
//                {
//                    while (drEmbutido.Read())
//                    {
//                        oCorteEnEmbutido = new Entidades.CortePorEmbutido();
//                        oCorteEnEmbutido.IdCorteEmbutido = Convert.ToInt32(drEmbutido["idCorteEmbutido"]);
//                        oCorteEnEmbutido.Embutido = oEmbutidoParam;
//                        oCorteEnEmbutido.Corte = findCorteById(Convert.ToInt32(drEmbutido["idCorte"]), false);
//                        oCorteEnEmbutido.KgUtilizado = float.Parse(drEmbutido["kgUtilizados"].ToString());
//                        oCorteEnEmbutido.PesoBalanza = Convert.ToBoolean(drEmbutido["pesoBalanza"]);

//                        cortesEnEmbutido.Add(oCorteEnEmbutido);
//                    }
//                    return cortesEnEmbutido;
//                }
//            }
//            finally
//            {
//                cmCorte.Connection.Close();
//                cortesEnEmbutido = null;
//            }
//        }

//        public int agregarEmbutido(Entidades.Embutido oEmbutido)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "agregarEmbutido";
//            cmCorte.Parameters.AddWithValue("@fechaEmbutido", oEmbutido.fechaEmbutido);
//            cmCorte.Parameters.AddWithValue("@idCorte", oEmbutido.corte.idCorte);
//            cmCorte.Parameters.AddWithValue("@idSucursal", oEmbutido.sucursal.IdSucursal);
//            cmCorte.Parameters.AddWithValue("@creadoPor", oEmbutido.CreadoPor.Id);
//            cmCorte.Parameters.AddWithValue("@observaciones", oEmbutido.observaciones);

//            SqlDataReader drEmbutido=cmCorte.ExecuteReader();
//            int idEmbutido=0;
//            while (drEmbutido.Read())
//            {
//                idEmbutido =Convert.ToInt32( drEmbutido["idEmbutido"].ToString());// Convert.ToInt32();
//            }
//            cmCorte.Connection.Close();
//            cmCorte = null;

//            return idEmbutido;        
//        }

//        public void anularEmbutido(Entidades.Embutido oEmbutidoE)
//        {
//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "anularEmbutido";
//            cmCorte.Parameters.AddWithValue("@idEmbutido", oEmbutidoE.idEmbutido);
//            cmCorte.Parameters.AddWithValue("@actualizadoPor", oEmbutidoE.ActualizadoPor.Id);

//            cmCorte.ExecuteNonQuery();
//            cmCorte.Connection.Close();

//            cmCorte = null;
//        }

//        public DataTable obtenerCortesPorEmbutidos(Entidades.Embutido oEmbutidoE)
//        {
//            DataTable dtCortePorEmbutido = new DataTable();

//            daCorte = new SqlDataAdapter();

//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "obtenerCortesPorEmbutidos";
//            cmCorte.Parameters.AddWithValue("@idEmbutido", oEmbutidoE.idEmbutido);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtCortePorEmbutido);

//            cmCorte.Connection.Close();

//            return dtCortePorEmbutido;
//        }

//        public void agregarCortePorEmbutido(Entidades.CortePorEmbutido oCortePorEmbutido)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "agregarCortePorEmbutido";

//            cmCorte.Parameters.AddWithValue("@idEmbutido", oCortePorEmbutido.embutido.idEmbutido);
//            cmCorte.Parameters.AddWithValue("@idCorte", oCortePorEmbutido.corte.idCorte);
//            cmCorte.Parameters.AddWithValue("@kgUtilizados", oCortePorEmbutido.kgUtilizado);
//            cmCorte.Parameters.AddWithValue("@idSucursal", oCortePorEmbutido.embutido.sucursal.IdSucursal);
//            cmCorte.Parameters.AddWithValue("@pesoBalanza", oCortePorEmbutido.PesoBalanza);

//            cmCorte.ExecuteNonQuery();
//            cmCorte.Connection.Close();

//            cmCorte = null;
//        }

//        public void actualizarStockEmbutido(DataRow cortePorEmbutido, Entidades.Embutido oEmbutidoE)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "actualizarStockEmbutido";

//            cmCorte.Parameters.AddWithValue("@idEmbutido", cortePorEmbutido["idEmbutido"]);
//            cmCorte.Parameters.AddWithValue("@idCorte", cortePorEmbutido["idCorte"]);
//            cmCorte.Parameters.AddWithValue("@kgUtilizados", cortePorEmbutido["kgUtilizados"]);
//            cmCorte.Parameters.AddWithValue("@idSucursal", oEmbutidoE.sucursal.idSucursal);

//            cmCorte.ExecuteNonQuery();
//            cmCorte.Connection.Close();

//            cmCorte = null;
//        }
//        #endregion

//        #region Movimiento

//        public int addOrEditMovimiento(Entidades.Movimiento oMovimientoE)
//        {
//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "addOrEditMovimiento";
//            cmCorte.Parameters.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento);
//            cmCorte.Parameters.AddWithValue("@fechaMovimiento", oMovimientoE.FechaMovimiento);
//            cmCorte.Parameters.AddWithValue("@sucursalOrigen", oMovimientoE.SucursalOrigen.idSucursal);
//            cmCorte.Parameters.AddWithValue("@sucursalDestino", oMovimientoE.SucursalDestino.idSucursal);
//            cmCorte.Parameters.AddWithValue("@observaciones", oMovimientoE.Observaciones);
//            cmCorte.Parameters.AddWithValue("@creadoPor", oMovimientoE.CreadoPor.Id);

//            if (oMovimientoE.IdMovimiento.Equals(0))
//            {
//                SqlDataReader drMovimiento = cmCorte.ExecuteReader();
//                while (drMovimiento.Read())
//                {
//                    oMovimientoE.IdMovimiento = Convert.ToInt32(drMovimiento["idMovimiento"].ToString());
//                }                
//            }
//            else
//            {
//                cmCorte.Parameters.AddWithValue("@actualizadoPor", oMovimientoE.ActualizadoPor.Id);
//                cmCorte.ExecuteNonQuery();
//            }
//            cmCorte.Connection.Close();
//            cmCorte = null;

//            return oMovimientoE.IdMovimiento;
//        }

//        public void modificarMovimiento(Entidades.Movimiento oMovimientoE)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "modificarMovimiento";

//            cmCorte.Parameters.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento);
//            cmCorte.Parameters.AddWithValue("@fechaMovimiento", oMovimientoE.FechaMovimiento);
//            cmCorte.Parameters.AddWithValue("@sucursalOrigen", oMovimientoE.SucursalOrigen.idSucursal);
//            cmCorte.Parameters.AddWithValue("@sucursalDestino", oMovimientoE.SucursalDestino.idSucursal);
//            cmCorte.Parameters.AddWithValue("@observaciones", oMovimientoE.Observaciones);
//            cmCorte.Parameters.AddWithValue("@actualizadoPor", oMovimientoE.ActualizadoPor.Id);

//            cmCorte.ExecuteNonQuery();

//            cmCorte.Connection.Close();
//        }

//        public void eliminarMovimiento(int idMovimiento, Entidades.Usuario oUsuario)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);

//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "eliminarMovimiento";
//            cmCorte.Parameters.AddWithValue("@idMovimiento", idMovimiento);
//            cmCorte.Parameters.AddWithValue("@actualizadoPor", oUsuario.Id);

//            cmCorte.Connection.Open();
//            cmCorte.ExecuteNonQuery();
//            cmCorte.Connection.Close();
//        }

//        public void agregarCortePorMovimiento(Entidades.CortePorMovimiento cortePorMovimiento)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "agregarCortePorMovimiento";
//            cmCorte.Parameters.AddWithValue("@idMovimiento", cortePorMovimiento.Movimientos.IdMovimiento);
//            cmCorte.Parameters.AddWithValue("@idCorte", cortePorMovimiento.Corte.IdCorte);
//            cmCorte.Parameters.AddWithValue("@cantKg", cortePorMovimiento.CantKg);
//            cmCorte.Parameters.AddWithValue("@cantUnidad", cortePorMovimiento.CantUnidad);
//            cmCorte.Parameters.AddWithValue("@pesoBalanza", cortePorMovimiento.PesoBalanza);
//            cmCorte.Parameters.AddWithValue("@permitirIngreso", cortePorMovimiento.PermitirIngreso);

//            cmCorte.ExecuteNonQuery();
//            cmCorte.Connection.Close();
//        }

//        public void quitarCortesPorMovimiento(Entidades.Movimiento oMovimientoE)
//        {
//            cmCorte = new SqlCommand();
//            cmCorte.Connection = conn.conectar(_empresa);

//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "quitarCortesPorMovimiento";
//            cmCorte.Parameters.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento);

//            cmCorte.Connection.Open();           
//            cmCorte.ExecuteNonQuery();
//            cmCorte.Connection.Close();
//        }

//        public DataTable obtenerMovimientos(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto)
//        {
//            DataTable dtMovimientos = new DataTable();

//            daCorte = new SqlDataAdapter();
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "obtenerMovimientos";
//            cmCorte.Parameters.AddWithValue("@sucOrigen", sucOrigen);
//            cmCorte.Parameters.AddWithValue("@sucDestino", sucDestino);
//            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);
//            cmCorte.Parameters.AddWithValue("@texto", texto);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtMovimientos);

//            return dtMovimientos;
//        }

//        public Entidades.Movimiento cargarMovimiento(int idMovimiento, bool acumulado)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "cargarMovimiento";
//            cmCorte.Parameters.AddWithValue("@idMovimiento", idMovimiento);

//            SqlDataReader drMovimiento = cmCorte.ExecuteReader();

//            Entidades.Movimiento oMovimiento = new Entidades.Movimiento();
//            while (drMovimiento.Read())
//            {
//                oMovimiento.IdMovimiento = Convert.ToInt32(drMovimiento["idMovimiento"].ToString());
//                oMovimiento.FechaMovimiento = Convert.ToDateTime(drMovimiento["fechaMovimiento"].ToString());

//                Entidades.Sucursal origen = new Entidades.Sucursal();
//                origen.idSucursal = Convert.ToInt32(drMovimiento["idOrigen"].ToString());
//                origen.sucursal = drMovimiento["origen"].ToString();

//                oMovimiento.SucursalOrigen = origen;

//                oMovimiento.IdMovOrigen = !string.IsNullOrEmpty(drMovimiento["idMovOrigen"].ToString()) ? Convert.ToInt32(drMovimiento["idMovOrigen"].ToString()) : 0;

//                Entidades.Sucursal destino = new Entidades.Sucursal();
//                destino.idSucursal = Convert.ToInt32(drMovimiento["idDestino"].ToString());
//                destino.sucursal = drMovimiento["destino"].ToString();

//                oMovimiento.SucursalDestino = destino;

//                oMovimiento.Observaciones = drMovimiento["observaciones"].ToString();

//                ///Borrar si funciona el seteo nuevo
//                ///
//                //oMovimiento.Creado = drMovimiento["creado"].Equals(null) ? (DateTime?)null : (DateTime?)Convert.ToDateTime(drMovimiento["creado"].ToString());
//                //DateTime fechaNull = Convert.ToDateTime("01/01/1990");
//                //oMovimiento.Actualizado = !String.IsNullOrEmpty(drMovimiento["actualizado"].ToString()) ? (Convert.ToDateTime(drMovimiento["actualizado"].ToString())) : fechaNull;

//                oMovimiento.Creado = Convert.ToDateTime(drMovimiento["creado"]);
//                oMovimiento.Actualizado = drMovimiento["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drMovimiento["actualizado"]);          

//                Datos.Usuario oUsuarioD = new Usuario(_empresa);
//                oMovimiento.CreadoPor = string.IsNullOrEmpty(drMovimiento["creadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drMovimiento["creadoPor"]));
//                oMovimiento.ActualizadoPor = string.IsNullOrEmpty(drMovimiento["actualizadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drMovimiento["actualizadoPor"]));

//                oMovimiento.ListaCortesPorMov = cargarCortesPorMovimiento(oMovimiento.IdMovimiento, acumulado);
//            }
//            cmCorte.Connection.Close();
//            return oMovimiento;
//        }

//        public List<Entidades.CortePorMovimiento> cargarCortesPorMovimiento(int idMovimiento, bool acumulado)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "cargarCortesPorMovimiento";
//            cmCorte.Parameters.AddWithValue("@idMovimiento", idMovimiento);
//            cmCorte.Parameters.AddWithValue("@acumulado", acumulado);

//            List<Entidades.CortePorMovimiento> listaCortesPorMovimiento = new List<Entidades.CortePorMovimiento>();
//            SqlDataReader drMovimiento = cmCorte.ExecuteReader();

//            while (drMovimiento.Read())
//            {
//                Entidades.CortePorMovimiento oCortePorMovimiento = new Entidades.CortePorMovimiento();

//                oCortePorMovimiento.IdCorteMovimiento = Convert.ToInt32(drMovimiento["idCorteMovimiento"].ToString());

//                Entidades.Corte corte =new Entidades.Corte();
//                corte.idCorte = Convert.ToInt32(drMovimiento["idCorte"].ToString());
//                corte.codigo = Convert.ToInt64(drMovimiento["codigo"].ToString());
//                corte.corte = drMovimiento["corte"].ToString();

//                oCortePorMovimiento.Corte = corte;

//                oCortePorMovimiento.CantKg = float.Parse(drMovimiento["cantKg"].ToString());
//                oCortePorMovimiento.CantUnidad = Convert.ToInt32(drMovimiento["cantUnidad"].ToString());
//                //try
//                //{
//                //    oCortePorMovimiento.PesoBalanza = Convert.ToBoolean(drMovimiento["pesoBalanza"]);
//                //}
//                //catch (Exception)
//                //{
//                //    oCortePorMovimiento.PesoBalanza = false;
//                //}
//                ///si no es acumulado directamente se establece falso el Permitir ingreso
//                ///porque no interesa agruparlo por cada valor de permitirIngreso
//                if (!acumulado)
//                {
//                    oCortePorMovimiento.PesoBalanza = drMovimiento["pesoBalanza"].Equals(DBNull.Value) ? false : Convert.ToBoolean(drMovimiento["pesoBalanza"]);
//                    oCortePorMovimiento.PermitirIngreso = drMovimiento["permitirIngreso"].Equals(DBNull.Value) ? false : Convert.ToBoolean(drMovimiento["permitirIngreso"]);
//                }
//                listaCortesPorMovimiento.Add(oCortePorMovimiento);
//                oCortePorMovimiento = null;               
//            }
//            cmCorte.Connection.Close();
//            return listaCortesPorMovimiento;
//        }


//        public DataTable obtenerLineasMov(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto)
//        {
//            DataTable dtLineasMov = new DataTable();

//            daCorte = new SqlDataAdapter();
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "obtenerLineasMov";
//            cmCorte.Parameters.AddWithValue("@sucOrigen", sucOrigen);
//            cmCorte.Parameters.AddWithValue("@sucDestino", sucDestino);
//            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);
//            cmCorte.Parameters.AddWithValue("@texto", texto);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtLineasMov);

//            return dtLineasMov;
//        }

//        public void reiniciarStockReal(int idSucursal)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "reiniciarStock";

//            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);

//            cmCorte.ExecuteNonQuery();

//            cmCorte.Connection.Close();

//            cmCorte = null;
//        }

//        public void reiniciarStockTeorico(int idSucursal)
//        {
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "reiniciarStockTeorico";
//            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);

//            cmCorte.ExecuteNonQuery();

//            cmCorte.Connection.Close();

//            cmCorte = null;
//        }

//        public DataTable reporteTeoricoReal(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
//        {
//            DataTable dtReporteTeoricoReal = new DataTable();
//            daCorte = new SqlDataAdapter();
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "StockTeoricoReal";
//            cmCorte.Parameters.AddWithValue("@texto",texto);
//            cmCorte.Parameters.AddWithValue("@idSucursal",idSucursal);
//            cmCorte.Parameters.AddWithValue("@fechaDesde",fechaDesde);
//            cmCorte.Parameters.AddWithValue("@fechaHasta",fechaHasta);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtReporteTeoricoReal);
//            cmCorte.Connection.Close();

//            cmCorte = null;
//            daCorte = null;

//            return dtReporteTeoricoReal;
//        }

//        public DateTime fechaUltimoCierreStock_Sucursal(int idSucursal)
//        {
//            DateTime fecha = DateTime.MinValue;

//            string sql = @"
//                        SELECT TOP 1 fechaCompra
//                        FROM Compras
//                        WHERE tipoCompra = 'Cierre Stock'
//                          AND idSucursal = @idSucursal
//                        ORDER BY fechaCompra DESC";

//            using (SqlConnection con = conn.conectar(_empresa))
//            using (SqlCommand cmd = new SqlCommand(sql, con))
//            {
//                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);

//                if (con.State != ConnectionState.Open) con.Open();
//                object result = cmd.ExecuteScalar();

//                if (result != null && result != DBNull.Value)
//                    fecha = Convert.ToDateTime(result);
//            }

//            return fecha;
//        }

//        public DataTable CierreStock(int nroCierre,string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal, string tipo, int idProveedor, int idMarca)
//        {
//            DataTable dtCierreStock = new DataTable();
//            daCorte = new SqlDataAdapter();
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = string.IsNullOrEmpty(conexionSucursal) ? conn.conectar(_empresa) : conn.conectar(conexionSucursal, _empresa);

//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; //cmCorte.CommandTimeout = conn.TimeOut();
//            if (nroCierre==1)
//            {
//                //cmCorte.CommandText = "a_InicioCierreStock";
//                cmCorte.CommandText = "a_CierreStock";
//            }
//            if (nroCierre == 2)
//            {
//                cmCorte.CommandText = "StockCierre_2";
//            }
//            cmCorte.Parameters.AddWithValue("@texto", texto);
//            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
//            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);
//            cmCorte.Parameters.AddWithValue("@tipo", tipo);
//            cmCorte.Parameters.AddWithValue("@idProveedor", idProveedor);
//            cmCorte.Parameters.AddWithValue("@idMarca", idMarca);

//            daCorte.SelectCommand = cmCorte;

//            try
//            {
//                daCorte.Fill(dtCierreStock);
//                cmCorte.Connection.Close();
//            }
//            catch (Exception ex)
//            {
//                string d = ex.Message;
//                cmCorte.Connection.Close();
//            }

//            cmCorte = null;
//            daCorte = null;

//            return dtCierreStock;
//        }

//        public DataTable acum_Ventas(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo , int idProveedor, int idMarca)
//        {
//            DataTable dtStockIngresoEgreso = new DataTable();
//            daCorte = new SqlDataAdapter();
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "Acum_Ventas";
//            cmCorte.Parameters.AddWithValue("@texto", texto);
//            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
//            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);
//            cmCorte.Parameters.AddWithValue("@tipo", tipo);
//            cmCorte.Parameters.AddWithValue("@idProveedor", idProveedor);
//            cmCorte.Parameters.AddWithValue("@idMarca", idMarca);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtStockIngresoEgreso);
//            cmCorte.Connection.Close();

//            cmCorte = null;
//            daCorte = null;

//            return dtStockIngresoEgreso;
//        }

//        public DataTable StockIngresoEgreso(string texto,int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
//        {
//            DataTable dtStockIngresoEgreso = new DataTable();
//            daCorte = new SqlDataAdapter();
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "StockIngresoEgreso";
//            cmCorte.Parameters.AddWithValue("@texto", texto);
//            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
//            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtStockIngresoEgreso);
//            cmCorte.Connection.Close();

//            cmCorte = null;
//            daCorte = null;

//            return dtStockIngresoEgreso;
//        }

//        public DataTable TotalPorCortesVendidos(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo, int idProveedor, int idMarca)
//        {
//            DataTable dtTotalPorCortesVendidos = new DataTable();
//            daCorte = new SqlDataAdapter();
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "TotalPorCortesVendidos";
//            cmCorte.Parameters.AddWithValue("@texto", texto);
//            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
//            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);
//            cmCorte.Parameters.AddWithValue("@tipo", tipo);
//            cmCorte.Parameters.AddWithValue("@idProveedor", idProveedor);
//            cmCorte.Parameters.AddWithValue("@idMarca", idMarca);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtTotalPorCortesVendidos);
//            cmCorte.Connection.Close();

//            cmCorte = null;
//            daCorte = null;

//            return dtTotalPorCortesVendidos;
//        }

//        public DataTable imprimirTeoricoReal(DataTable dtTeoricoReal,string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
//        {

//            daCorte = new SqlDataAdapter();
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "StockTeoricoReal";
//            cmCorte.Parameters.AddWithValue("@texto",texto);
//            cmCorte.Parameters.AddWithValue("@idSucursal",idSucursal);
//            cmCorte.Parameters.AddWithValue("@fechaDesde",fechaDesde);
//            cmCorte.Parameters.AddWithValue("@fechaHasta",fechaHasta);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtTeoricoReal);
//            cmCorte.Connection.Close();

//            cmCorte = null;
//            daCorte = null;

//            return dtTeoricoReal;
//        }

//        public DataTable TotalKgsCortePorCompra(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
//        {
//            DataTable dtTotalKgsCortePorCompra = new DataTable();
//            daCorte = new SqlDataAdapter();
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            //cmCorte.CommandText = "TotalKgsCortePorCompra";
//            cmCorte.CommandText = "a_CierreStock"; // "a_IngresoStock";
//            cmCorte.Parameters.AddWithValue("@texto", texto);
//            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
//            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtTotalKgsCortePorCompra);
//            cmCorte.Connection.Close();

//            cmCorte = null;
//            daCorte = null;

//            return dtTotalKgsCortePorCompra;
//        }

//        public DataTable TotalMovimientosPorCorte(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
//        {
//            DataTable dtTotalMovimientosPorCorte = new DataTable();
//            daCorte = new SqlDataAdapter();
//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();
//            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = conn.TimeOut();
//            cmCorte.CommandText = "TotalMovimientosPorCorte";
//            cmCorte.Parameters.AddWithValue("@texto", texto);
//            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
//            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);

//            daCorte.SelectCommand = cmCorte;
//            daCorte.Fill(dtTotalMovimientosPorCorte);
//            cmCorte.Connection.Close();

//            cmCorte = null;
//            daCorte = null;

//            return dtTotalMovimientosPorCorte;
//        }
//        public DataTable Balance(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
//        {
//            DataTable dtBalance = new DataTable();
//            daCorte = new SqlDataAdapter();
//            cmCorte = new SqlCommand();

//            using (conn.conectar(_empresa))
//            {
//                //// Crear un comando SQL para ejecutar el procedimiento almacenado
//                SqlCommand cmCorte = new SqlCommand("BalanceConsFinal_FecDesde_Hasta", conn.conectar(_empresa));
//                cmCorte.CommandType = CommandType.StoredProcedure;
//                cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
//                cmCorte.Parameters.AddWithValue("@FechaDesde", fechaDesde);
//                cmCorte.Parameters.AddWithValue("@FechaHasta", fechaHasta.AddDays(1));//sumo uno xq sino toma hora 00:00

//                // Crear un adaptador para llenar el DataTable con los resultados
//                SqlDataAdapter adapter = new SqlDataAdapter(cmCorte);

//                // Crear y llenar un DataTable con los resultados del procedimiento
//                adapter.Fill(dtBalance);
//            }

//            dtBalance.Columns.Remove("orden");//elimino la colmuna porque fue creada para traer los datos ordenados desde sqlserver

//            foreach (DataRow row in dtBalance.Rows)
//            {
//                // Modificar el valor de una columna específica
//                if (row["Descripcion"].ToString().Contains("DETALLE") || row["Descripcion"].ToString().Contains("NOTAS") ||
//                    row["Descripcion"].ToString()[0].ToString().Contains('*'))
//                {
//                    for (int i = 1; i < dtBalance.Columns.Count; i++)
//                    {
//                        row[i] = DBNull.Value;
//                    }
//                }

//                if (row["Descripcion"].ToString().Contains("COMPRAS") || row["Descripcion"].ToString().Contains("GASTOS"))
//                {

//                    row["Tickets"] = DBNull.Value;
//                }
//            }

//            cmCorte = null;
//            daCorte = null;

//            return dtBalance;
//        }

//        #endregion

//        #region Tipos Producto/Corte
//        public DataTable obtenerTiposProductoGrilla(string buscarText)
//        {
//            string where = string.IsNullOrEmpty(buscarText) ? "" : $"WHERE tipo LIKE '%{buscarText}%'";
//            string selectText = "Select  tipo, orden, creado as Creado, actualizado as Actualizado, reservadoSistema as Reservado from TiposProducto " + where + " order by orden, tipo";
//            DataTable dtTiposProducto = new DataTable();
//            SqlDataAdapter daCorte = new SqlDataAdapter(selectText, conn.conectar(_empresa));
//            daCorte.Fill(dtTiposProducto);
//            conn.cerraConexion();

//            return dtTiposProducto;
//        }

//        public string addOrEditTipoProducto(string tiposProducto, string orden, bool esInsert, string tipoToUpdate)
//        {
//            string mensaje = "";
//            //significa que es un nuevo registro
//            if (esInsert)
//            {
//                // Consulta si existe un registro con el mismo nombre de tipo
//                string selectQuery = "SELECT COUNT(*) FROM TiposProducto WHERE tipo = @tiposProducto";
//                // Obtener el valor más grande de Id
//                SqlCommand cmCorte1 = new SqlCommand(selectQuery);
//                cmCorte1.Parameters.AddWithValue("@tiposProducto", tiposProducto);
//                cmCorte1.Connection = conn.conectar(_empresa);
//                cmCorte1.Connection.Open();

//                object result = cmCorte1.ExecuteScalar(); // Obtener el resultado

//                // Si hay resultados, informa
//                if ((int)result != 0)
//                {
//                    mensaje = "Ya existe un Tipo con el mismo nombre.";
//                    cmCorte1.Connection.Close();
//                    return mensaje;
//                }
//                cmCorte1.Connection.Close();
//            }

//            cmCorte = new SqlCommand();

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            string query = esInsert ?
//                $"INSERT INTO TiposProducto (tipo, orden, reservadoSistema, creado) VALUES (@tipo, @orden, @reservadoSistema, @creado)" :
//                $"UPDATE TiposProducto SET tipo = @tipo, orden = @orden, actualizado = @actualizado WHERE  tipo like @tipoToUpdate;" +
//                $"UPDATE Corte SET tipo = @tipo WHERE  tipo like @tipoToUpdate;";

//            cmCorte.CommandType = CommandType.Text;
//            cmCorte.CommandText = query;
//            cmCorte.Parameters.AddWithValue("@tipo", tiposProducto); 
//            cmCorte.Parameters.AddWithValue("@tipoToUpdate", tipoToUpdate); 
//            cmCorte.Parameters.AddWithValue("@orden", orden);
//            cmCorte.Parameters.AddWithValue("@reservadoSistema", false);
//            cmCorte.Parameters.AddWithValue("@creado", DateTime.Now);
//            cmCorte.Parameters.AddWithValue("@actualizado", DateTime.Now);

//            cmCorte.ExecuteNonQuery();
//            cmCorte.Connection.Close();

//            return mensaje;
//        }

//        public string eliminarTipoProducto(string tiposProducto)
//        {
//            cmCorte = new SqlCommand();
//            string mensaje = "";

//            // Consulta si existen cortes con el tipo
//            string selectQuery = "SELECT COUNT(*) FROM Corte WHERE tipo = @tiposProducto";

//            // Obtener el valor más grande de Id
//            SqlCommand cmCorte1 = new SqlCommand(selectQuery);
//            cmCorte1.Parameters.AddWithValue("@tiposProducto", tiposProducto);
//            cmCorte1.Connection = conn.conectar(_empresa);
//            cmCorte1.Connection.Open();

//            object result = cmCorte1.ExecuteScalar(); // Obtener el resultado

//            // Si hay resultados, informa
//            if ((int)result != 0)
//            {
//                mensaje = "Existen Productos/Cortes con el Tipo que quiere eliminar.\n\nPara poder eliminar el Tipo debe cambiar todo los Productos/Cortes asociados a éste.";
//                cmCorte1.Connection.Close();
//                return mensaje;
//            }
//            cmCorte1.Connection.Close();


//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            string query = $"DELETE FROM TiposProducto WHERE  tipo = @tipo";

//            cmCorte.CommandType = CommandType.Text;
//            cmCorte.CommandText = query;
//            cmCorte.Parameters.AddWithValue("@tipo", tiposProducto);

//            cmCorte.ExecuteNonQuery();
//            cmCorte.Connection.Close();

//            return mensaje;
//        }

//        /// <summary>
//        /// Sugiere el menor codigo libre segun el tipo de producto
//        /// </summary>
//        /// <param name="tipo"></param>
//        /// <returns></returns>
//        public long sugerirCodigo(string tipo)
//        {
//            cmCorte = new SqlCommand();
//            long codigoDisponible = -1; // Valor por defecto si la tabla está vacía

//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            string query = @"SELECT MIN(Codigo + 1) AS CodigoDisponible
//                            FROM Corte 
//                            WHERE Tipo = @tipo
//                            AND NOT EXISTS (
//                                SELECT 1 FROM Corte c2 WHERE c2.Codigo = Corte.Codigo + 1 AND c2.Tipo = @tipo
//                            );";

//            cmCorte.CommandType = CommandType.Text;
//            cmCorte.CommandText = query;
//            cmCorte.Parameters.AddWithValue("@tipo", tipo);

//            object result = cmCorte.ExecuteScalar();
//            if (result != DBNull.Value && result != null)
//            {
//                codigoDisponible = Convert.ToInt64(result);
//            }


//            return codigoDisponible;
//        }

//        #endregion
//        public int obtenerNivelCorte(int idCorteMaestro)
//        {
//            SqlCommand cmCorte = new SqlCommand("obtenerNivelCorte", conn.conectar(_empresa));
//            cmCorte.CommandType = CommandType.StoredProcedure;
//            cmCorte.Parameters.AddWithValue("@idCorteMaestro", idCorteMaestro);
//            cmCorte.Connection = conn.conectar(_empresa);
//            cmCorte.Connection.Open();

//            object result = cmCorte.ExecuteScalar(); // Obtener el resultado

//            return (int)result;
//        }

//    }
//}
