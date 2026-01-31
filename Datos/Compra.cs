using System;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class Compra
    {
        private readonly Utilidades.Conexion conn;
        private readonly IEmpresaContext _empresa;

        public Compra(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
            conn = new Utilidades.Conexion();
        }

        public void anularCompra(int idCompra)
        {
            using (SqlConnection cn = conn.conectar(_empresa)) // YA viene abierta
            using (SqlCommand cmd = new SqlCommand("anularCompra", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idCompra", idCompra);
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable obtenerCompras(int idSucursal, string tipoCompra, string texto, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = string.IsNullOrEmpty(conexionSucursal)
                    ? conn.conectar(_empresa)
                    : conn.conectar(conexionSucursal, _empresa)) // YA viene abierta
            using (SqlCommand cmd = new SqlCommand("obtenerCompras", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);
                cmd.Parameters.AddWithValue("@tipoCompra", tipoCompra ?? "");
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable getLineasCompras(int idSucursal, string tipoCompra, string texto, string codigo, string corte, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = string.IsNullOrEmpty(conexionSucursal)
                    ? conn.conectar(_empresa)
                    : conn.conectar(conexionSucursal, _empresa)) // YA viene abierta
            using (SqlCommand cmd = new SqlCommand("getLineasCompras", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@codigo", codigo ?? "");
                cmd.Parameters.AddWithValue("@corte", corte ?? "");
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);
                cmd.Parameters.AddWithValue("@tipoCompra", tipoCompra ?? "");
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable findById(int idCompra)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa)) // YA viene abierta
            using (SqlCommand cmd = new SqlCommand("select * from Compras where idCompra = @idCompra", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@idCompra", SqlDbType.Int).Value = idCompra;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public int addOrEditCompra(Entidades.Compra oCompraE)
        {
            if (oCompraE == null) throw new ArgumentNullException(nameof(oCompraE));

            using (SqlConnection cn = conn.conectar(_empresa)) // YA viene abierta
            using (SqlCommand cmd = new SqlCommand("addOrEditCompra", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idCompra", oCompraE.IdCompra);
                cmd.Parameters.AddWithValue("@nroRemito", oCompraE.NroRemito ?? "");
                cmd.Parameters.AddWithValue("@fechaCompra", oCompraE.FechaCompra);
                cmd.Parameters.AddWithValue("@idProveedor", oCompraE.Proveedor.idPersona);
                cmd.Parameters.AddWithValue("@estado", oCompraE.Estado ?? "");
                cmd.Parameters.AddWithValue("@observaciones", oCompraE.Observaciones ?? "");
                cmd.Parameters.AddWithValue("@tipoCompra", oCompraE.TipoCompra ?? "");
                cmd.Parameters.AddWithValue("@cantMedias", oCompraE.CantMedias);
                cmd.Parameters.AddWithValue("@kgsMedias", oCompraE.KgsMedias);
                cmd.Parameters.AddWithValue("@enCtaCte", oCompraE.EnCtaCte);
                cmd.Parameters.AddWithValue("@idSucursal", oCompraE.Sucursal.idSucursal);
                cmd.Parameters.AddWithValue("@creadoPor", oCompraE.CreadoPor.Id);
                cmd.Parameters.AddWithValue("@actualizadoPor", oCompraE.ActualizadoPor != null ? oCompraE.ActualizadoPor.Id : 0);

                object obj = cmd.ExecuteScalar();
                oCompraE.IdCompra = (obj == null || obj == DBNull.Value) ? oCompraE.IdCompra : Convert.ToInt32(obj);
                return oCompraE.IdCompra;
            }
        }

        public int agregarCompra(Entidades.Compra oCompraE)
        {
            if (oCompraE == null) throw new ArgumentNullException(nameof(oCompraE));

            using (SqlConnection cn = conn.conectar(_empresa)) // YA viene abierta
            using (SqlCommand cmd = new SqlCommand("agregarCompra", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@nroRemito", oCompraE.NroRemito ?? "");
                cmd.Parameters.AddWithValue("@fechaCompra", oCompraE.FechaCompra);
                cmd.Parameters.AddWithValue("@idProveedor", oCompraE.Proveedor.idPersona);
                cmd.Parameters.AddWithValue("@estado", oCompraE.Estado ?? "");
                cmd.Parameters.AddWithValue("@observaciones", oCompraE.Observaciones ?? "");
                cmd.Parameters.AddWithValue("@tipoCompra", oCompraE.TipoCompra ?? "");
                cmd.Parameters.AddWithValue("@cantMedias", oCompraE.CantMedias);
                cmd.Parameters.AddWithValue("@kgsMedias", oCompraE.KgsMedias);
                cmd.Parameters.AddWithValue("@enCtaCte", oCompraE.EnCtaCte);
                cmd.Parameters.AddWithValue("@idSucursal", oCompraE.Sucursal.idSucursal);
                cmd.Parameters.AddWithValue("@creadoPor", oCompraE.CreadoPor.Id);

                // tu SP devuelve reader con idCompra
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    int idCompra = 0;
                    while (dr.Read())
                    {
                        idCompra = Convert.ToInt32(dr["idCompra"].ToString());
                    }
                    return idCompra;
                }
            }
        }

        public void ModificarCompra(Entidades.Compra oCompraE)
        {
            if (oCompraE == null) throw new ArgumentNullException(nameof(oCompraE));

            using (SqlConnection cn = conn.conectar(_empresa)) // YA viene abierta
            using (SqlCommand cmd = new SqlCommand("modificarCompra", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idCompra", oCompraE.IdCompra);
                cmd.Parameters.AddWithValue("@nroRemito", oCompraE.NroRemito ?? "");
                cmd.Parameters.AddWithValue("@fechaCompra", oCompraE.FechaCompra);
                cmd.Parameters.AddWithValue("@idProveedor", oCompraE.Proveedor.idPersona);
                cmd.Parameters.AddWithValue("@estado", oCompraE.Estado ?? "");
                cmd.Parameters.AddWithValue("@observaciones", oCompraE.Observaciones ?? "");
                cmd.Parameters.AddWithValue("@tipoCompra", oCompraE.TipoCompra ?? "");
                cmd.Parameters.AddWithValue("@cantMedias", oCompraE.CantMedias);
                cmd.Parameters.AddWithValue("@kgsMedias", oCompraE.KgsMedias);
                cmd.Parameters.AddWithValue("@enCtaCte", oCompraE.EnCtaCte);
                cmd.Parameters.AddWithValue("@idSucursal", oCompraE.Sucursal.idSucursal);
                cmd.Parameters.AddWithValue("@actualizadoPor", oCompraE.ActualizadoPor.Id);

                cmd.ExecuteNonQuery();
            }
        }

        public float getTotalCompra(int idCompra, string tipoCompra)
        {
            string consulta = "";

            switch (Entidades.Compra.tipoCompraToEnum(tipoCompra))
            {
                case Entidades.Compra.tipoCompraEnum.Cortes:
                    consulta =
                        "SELECT SUM(cantKg * precioKg) AS total " +
                        "FROM dbo.CortePorCompra " +
                        "WHERE idCompra = @idCompra " +
                        "GROUP BY idCompra";
                    break;

                case Entidades.Compra.tipoCompraEnum.MediaRes:
                    consulta =
                        "SELECT SUM(kgMedia * precioMedia) AS total " +
                        "FROM dbo.MediaRes " +
                        "WHERE idCompra = @idCompra " +
                        "GROUP BY idCompra";
                    break;
            }

            using (SqlConnection cn = conn.conectar(_empresa)) // YA viene abierta
            using (SqlCommand cmd = new SqlCommand(consulta, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.Add("@idCompra", SqlDbType.Int).Value = idCompra;

                object result = cmd.ExecuteScalar();
                double totalCompraD = (result == null || result == DBNull.Value) ? 0 : Convert.ToDouble(result);
                return (float)totalCompraD;
            }
        }

        public void modificarPrecioMedia(int idCompra, float precioKg)
        {
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("modificarPrecioMedia", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idCompra", idCompra);
                cmd.Parameters.AddWithValue("@precioKg", precioKg);
                cmd.ExecuteNonQuery();
            }
        }

        public void agregarCortePorCompra(Entidades.CortePorCompra oCorteE)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("agregarCortePorCompra", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idCompra", oCorteE.compra.IdCompra);
                cmd.Parameters.AddWithValue("@idCorte", oCorteE.corte.idCorte);
                cmd.Parameters.AddWithValue("@idSucursal", oCorteE.sucursal.IdSucursal);
                cmd.Parameters.AddWithValue("@precioKg", oCorteE.precioKg);
                cmd.Parameters.AddWithValue("@cantKg", oCorteE.cantKgs);
                cmd.Parameters.AddWithValue("@balanza", oCorteE.Balanza);
                cmd.Parameters.AddWithValue("@creado", oCorteE.Creado);
                cmd.Parameters.AddWithValue("@creadoPor", oCorteE.CreadoPor != null ? oCorteE.CreadoPor.Id : 0);

                cmd.ExecuteNonQuery();
            }
        }

        public void agregarMediaRes(Entidades.MediaRes oMediaResE)
        {
            if (oMediaResE == null) throw new ArgumentNullException(nameof(oMediaResE));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("agregarMediaRes", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idCompra", oMediaResE.compra.IdCompra);
                cmd.Parameters.AddWithValue("@nroTropa", oMediaResE.nroTropa ?? "");
                cmd.Parameters.AddWithValue("@idSucursal", oMediaResE.sucursal.IdSucursal);
                cmd.Parameters.AddWithValue("@precioMedia", oMediaResE.precioMedia);
                cmd.Parameters.AddWithValue("@kgMedia", oMediaResE.kgMedia);

                cmd.ExecuteNonQuery();
            }
        }

        public int obtenerIdUltimaCompra()
        {
            // OJO: tu código anterior leía drUltimaCompra[2] (raro)
            // Te lo dejo mejor: MAX(idCompra)+1
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(idCompra), 0) FROM Compras", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                object obj = cmd.ExecuteScalar();
                int maxId = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
                return maxId + 1;
            }
        }

        public DataTable obtenerCortesPorCompra(int idCompra)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerCortesPorCompra", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idCompra", idCompra);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable obtenerMediasPorCompra(int idCompra)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerMediasPorCompra", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idCompra", idCompra);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public void modificarMediaPorCompra(Entidades.MediaRes oMediaResE, int idCompra)
        {
            if (oMediaResE == null) throw new ArgumentNullException(nameof(oMediaResE));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("modificarMediaPorCompra", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idCompra", idCompra);
                cmd.Parameters.AddWithValue("@idMedia", oMediaResE.idMedia);
                cmd.Parameters.AddWithValue("@idSucursal", oMediaResE.sucursal.IdSucursal);
                cmd.Parameters.AddWithValue("@nroTropa", oMediaResE.nroTropa ?? "");
                cmd.Parameters.AddWithValue("@precioMedia", oMediaResE.precioMedia);
                cmd.Parameters.AddWithValue("@kgMedia", oMediaResE.kgMedia);

                cmd.ExecuteNonQuery();
            }
        }

        public void modificarCortePorCompra(Entidades.CortePorCompra oCorteE, int idCompra)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("modificarCortePorCompra", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idCompra", idCompra);
                cmd.Parameters.AddWithValue("@idCorte", oCorteE.corte.idCorte);
                cmd.Parameters.AddWithValue("@idSucursal", oCorteE.sucursal.IdSucursal);
                cmd.Parameters.AddWithValue("@precioKg", oCorteE.precioKg);
                cmd.Parameters.AddWithValue("@cantKg", oCorteE.cantKgs);

                cmd.ExecuteNonQuery();
            }
        }

        public void quitarStockMedia(Entidades.MediaRes oMediaResE, int idCompra)
        {
            if (oMediaResE == null) throw new ArgumentNullException(nameof(oMediaResE));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("quitarStockMedia", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idCompra", idCompra);
                cmd.Parameters.AddWithValue("@idMedia", oMediaResE.idMedia);
                cmd.Parameters.AddWithValue("@idSucursal", oMediaResE.sucursal.IdSucursal);
                cmd.ExecuteNonQuery();
            }
        }

        public void quitarStockTeoricoMedia(Entidades.MediaRes oMediaResE, int idCompra)
        {
            if (oMediaResE == null) throw new ArgumentNullException(nameof(oMediaResE));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("quitarStockTeoricoMedia", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idCompra", idCompra);
                cmd.Parameters.AddWithValue("@idMedia", oMediaResE.idMedia);
                cmd.Parameters.AddWithValue("@idSucursal", oMediaResE.sucursal.IdSucursal);
                cmd.ExecuteNonQuery();
            }
        }

        public void quitarStockCorte(Entidades.CortePorCompra oCorteE, int idCompra)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("quitarStockCorte", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idCompra", idCompra);
                cmd.Parameters.AddWithValue("@idCorte", oCorteE.corte.idCorte);
                cmd.Parameters.AddWithValue("@idSucursal", oCorteE.sucursal.IdSucursal);
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable porcentajeCortesPorCompra(int idCompra)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("porcentajeCortesPorCompra", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idCompra", idCompra);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable getPromMedias(int idCompra)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("getPromMedias", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@id", idCompra);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable getPorcCortesEnMedias(int idCompra)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("getPorcCortesEnMedias", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@id", idCompra);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        // Se comprueba que el Pesaje tenga el ajuste realizado. Retorna ID <> 0 si tiene
        public int getIdAjusteDelPesaje(int idPesaje)
        {
            string tipoAj = Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.AjusteStock);

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT idCompra FROM dbo.Compras WHERE tipoCompra = @tipo AND nroRemito = @nroRemito", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.Add("@tipo", SqlDbType.NVarChar, 50).Value = tipoAj;
                cmd.Parameters.Add("@nroRemito", SqlDbType.NVarChar, 50).Value = idPesaje.ToString();

                object obj = cmd.ExecuteScalar();
                return (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
            }
        }

        public void actualizarEstadoPesaje(int idPesaje, Entidades.Compra.estadoAjusteStock estadoAjStock)
        {
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Compras SET estado = @estado WHERE idCompra = @id", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.Add("@estado", SqlDbType.NVarChar, 50).Value =
                    Entidades.Compra.estadoAjStockToString(estadoAjStock);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idPesaje;

                cmd.ExecuteNonQuery();
            }
        }

        public void backup(string destino)
        {
            // OJO: esto siempre backuppea SuperCerdo. Mantengo tu lógica.
            string rutaDestino = destino ?? "";
            string sentencia =
                "BACKUP DATABASE [SuperCerdo] TO DISK = @ruta " +
                "WITH NOFORMAT, INIT, NAME = N'SuperCerdo', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sentencia, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.Add("@ruta", SqlDbType.NVarChar, 400).Value = rutaDestino;
                cmd.ExecuteNonQuery();
            }
        }

        public void restaurarBD(string dataSource, string bdAuxiliar, string rutaOrigen)
        {
            // Esto va a otra DB (master/aux). Lo dejo como lo tenías, pero con using.
            string cs = "Data Source=" + dataSource + ";Initial Catalog=" + bdAuxiliar + ";Integrated Security=True";

            string sentencia =
                "RESTORE DATABASE [SuperCerdo] FROM DISK = @ruta " +
                "WITH FILE = 1, NOUNLOAD, REPLACE, STATS = 10";

            using (SqlConnection cn = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(sentencia, cn))
            {
                if (cn.State != ConnectionState.Open) cn.Open();
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0; // restore puede tardar
                cmd.Parameters.Add("@ruta", SqlDbType.NVarChar, 400).Value = rutaOrigen ?? "";
                cmd.ExecuteNonQuery();
            }
        }
    }
}
