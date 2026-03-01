using System;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class Compra
    {
        private readonly IEmpresaContext _empresa;private readonly IParametrosContext _param;

        public Compra(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa)); _param = param;
        }

        public void anularCompra(int idCompra)
        {
            Db.NonQuery(
                _empresa,
                "anularCompra",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@idCompra", idCompra)
            );
        }

        public DataTable obtenerCompras(int idSucursal, string tipoCompra, string texto, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal)
        {
            return Db.DataTable(
                _empresa,
                "obtenerCompras",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                    p.AddWithValue("@tipoCompra", tipoCompra ?? "");
                    p.AddWithValue("@idSucursal", idSucursal);
                },
                conexionSucursal: string.IsNullOrWhiteSpace(conexionSucursal) ? null : conexionSucursal
            );
        }

        public DataTable getLineasCompras(int idSucursal, string tipoCompra, string texto, string codigo, string corte, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal)
        {
            return Db.DataTable(
                _empresa,
                "getLineasCompras",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@codigo", codigo ?? "");
                    p.AddWithValue("@corte", corte ?? "");
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                    p.AddWithValue("@tipoCompra", tipoCompra ?? "");
                    p.AddWithValue("@idSucursal", idSucursal);
                },
                conexionSucursal: string.IsNullOrWhiteSpace(conexionSucursal) ? null : conexionSucursal
            );
        }

        public DataTable findById(int idCompra)
        {
            const string sql = "SELECT * FROM Compras WHERE idCompra = @idCompra;";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                p => p.Add("@idCompra", SqlDbType.Int).Value = idCompra
            );
        }

        public int addOrEditCompra(Entidades.Compra oCompraE)
        {
            if (oCompraE == null) throw new ArgumentNullException(nameof(oCompraE));

            object obj = Db.Scalar(
                _empresa,
                "addOrEditCompra",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idCompra", oCompraE.IdCompra);
                    p.AddWithValue("@nroRemito", oCompraE.NroRemito ?? "");
                    p.AddWithValue("@fechaCompra", oCompraE.FechaCompra);
                    p.AddWithValue("@idProveedor", oCompraE.Proveedor.idPersona);
                    p.AddWithValue("@estado", oCompraE.Estado ?? "");
                    p.AddWithValue("@observaciones", oCompraE.Observaciones ?? "");
                    p.AddWithValue("@tipoCompra", oCompraE.TipoCompra ?? "");
                    p.AddWithValue("@cantMedias", oCompraE.CantMedias);
                    p.AddWithValue("@kgsMedias", oCompraE.KgsMedias);
                    p.AddWithValue("@enCtaCte", oCompraE.EnCtaCte);
                    p.AddWithValue("@idSucursal", oCompraE.Sucursal.idSucursal);
                    p.AddWithValue("@creadoPor", oCompraE.CreadoPor.Id);
                    p.AddWithValue("@actualizadoPor", oCompraE.ActualizadoPor != null ? oCompraE.ActualizadoPor.Id : 0);
                }
            );

            oCompraE.IdCompra = (obj == null || obj == DBNull.Value) ? oCompraE.IdCompra : Convert.ToInt32(obj);
            return oCompraE.IdCompra;
        }

        public int agregarCompra(Entidades.Compra oCompraE)
        {
            if (oCompraE == null) throw new ArgumentNullException(nameof(oCompraE));

            // Tu SP devuelve reader con idCompra. Lo resolvemos con Db.Reader y tomamos el último/primero.
            var ids = Db.Reader(
                _empresa,
                "agregarCompra",
                CommandType.StoredProcedure,
                dr => Convert.ToInt32(dr["idCompra"].ToString()),
                p =>
                {
                    p.AddWithValue("@nroRemito", oCompraE.NroRemito ?? "");
                    p.AddWithValue("@fechaCompra", oCompraE.FechaCompra);
                    p.AddWithValue("@idProveedor", oCompraE.Proveedor.idPersona);
                    p.AddWithValue("@estado", oCompraE.Estado ?? "");
                    p.AddWithValue("@observaciones", oCompraE.Observaciones ?? "");
                    p.AddWithValue("@tipoCompra", oCompraE.TipoCompra ?? "");
                    p.AddWithValue("@cantMedias", oCompraE.CantMedias);
                    p.AddWithValue("@kgsMedias", oCompraE.KgsMedias);
                    p.AddWithValue("@enCtaCte", oCompraE.EnCtaCte);
                    p.AddWithValue("@idSucursal", oCompraE.Sucursal.idSucursal);
                    p.AddWithValue("@creadoPor", oCompraE.CreadoPor.Id);
                }
            );

            return ids.Count > 0 ? ids[ids.Count - 1] : 0;
        }

        public void ModificarCompra(Entidades.Compra oCompraE)
        {
            if (oCompraE == null) throw new ArgumentNullException(nameof(oCompraE));

            Db.NonQuery(
                _empresa,
                "modificarCompra",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idCompra", oCompraE.IdCompra);
                    p.AddWithValue("@nroRemito", oCompraE.NroRemito ?? "");
                    p.AddWithValue("@fechaCompra", oCompraE.FechaCompra);
                    p.AddWithValue("@idProveedor", oCompraE.Proveedor.idPersona);
                    p.AddWithValue("@estado", oCompraE.Estado ?? "");
                    p.AddWithValue("@observaciones", oCompraE.Observaciones ?? "");
                    p.AddWithValue("@tipoCompra", oCompraE.TipoCompra ?? "");
                    p.AddWithValue("@cantMedias", oCompraE.CantMedias);
                    p.AddWithValue("@kgsMedias", oCompraE.KgsMedias);
                    p.AddWithValue("@enCtaCte", oCompraE.EnCtaCte);
                    p.AddWithValue("@idSucursal", oCompraE.Sucursal.idSucursal);
                    p.AddWithValue("@actualizadoPor", oCompraE.ActualizadoPor.Id);
                }
            );
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

            object result = Db.Scalar(
                _empresa,
                consulta,
                CommandType.Text,
                p => p.Add("@idCompra", SqlDbType.Int).Value = idCompra
            );

            double totalCompraD = (result == null || result == DBNull.Value) ? 0 : Convert.ToDouble(result);
            return (float)totalCompraD;
        }

        public void modificarPrecioMedia(int idCompra, float precioKg)
        {
            Db.NonQuery(
                _empresa,
                "modificarPrecioMedia",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idCompra", idCompra);
                    p.AddWithValue("@precioKg", precioKg);
                }
            );
        }

        public void agregarCortePorCompra(Entidades.CortePorCompra oCorteE)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            Db.NonQuery(
                _empresa,
                "agregarCortePorCompra",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idCompra", oCorteE.compra.IdCompra);
                    p.AddWithValue("@idCorte", oCorteE.corte.idCorte);
                    p.AddWithValue("@idSucursal", oCorteE.sucursal.IdSucursal);
                    p.AddWithValue("@precioKg", oCorteE.precioKg);
                    p.AddWithValue("@cantKg", oCorteE.cantKgs);
                    p.AddWithValue("@balanza", oCorteE.Balanza);
                    p.AddWithValue("@creado", oCorteE.Creado);
                    p.AddWithValue("@creadoPor", oCorteE.CreadoPor != null ? oCorteE.CreadoPor.Id : 0);
                }
            );
        }

        public void agregarMediaRes(Entidades.MediaRes oMediaResE)
        {
            if (oMediaResE == null) throw new ArgumentNullException(nameof(oMediaResE));

            Db.NonQuery(
                _empresa,
                "agregarMediaRes",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idCompra", oMediaResE.compra.IdCompra);
                    p.AddWithValue("@nroTropa", oMediaResE.nroTropa ?? "");
                    p.AddWithValue("@idSucursal", oMediaResE.sucursal.IdSucursal);
                    p.AddWithValue("@precioMedia", oMediaResE.precioMedia);
                    p.AddWithValue("@kgMedia", oMediaResE.kgMedia);
                }
            );
        }

        public int obtenerIdUltimaCompra()
        {
            const string sql = "SELECT ISNULL(MAX(idCompra), 0) FROM Compras;";

            object obj = Db.Scalar(_empresa, sql, CommandType.Text);
            int maxId = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
            return maxId + 1;
        }

        public DataTable obtenerCortesPorCompra(int idCompra)
        {
            return Db.DataTable(
                _empresa,
                "obtenerCortesPorCompra",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@idCompra", idCompra)
            );
        }

        public DataTable obtenerMediasPorCompra(int idCompra)
        {
            return Db.DataTable(
                _empresa,
                "obtenerMediasPorCompra",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@idCompra", idCompra)
            );
        }

        public void modificarMediaPorCompra(Entidades.MediaRes oMediaResE, int idCompra)
        {
            if (oMediaResE == null) throw new ArgumentNullException(nameof(oMediaResE));

            Db.NonQuery(
                _empresa,
                "modificarMediaPorCompra",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idCompra", idCompra);
                    p.AddWithValue("@idMedia", oMediaResE.idMedia);
                    p.AddWithValue("@idSucursal", oMediaResE.sucursal.IdSucursal);
                    p.AddWithValue("@nroTropa", oMediaResE.nroTropa ?? "");
                    p.AddWithValue("@precioMedia", oMediaResE.precioMedia);
                    p.AddWithValue("@kgMedia", oMediaResE.kgMedia);
                }
            );
        }

        public void modificarCortePorCompra(Entidades.CortePorCompra oCorteE, int idCompra)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            Db.NonQuery(
                _empresa,
                "modificarCortePorCompra",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idCompra", idCompra);
                    p.AddWithValue("@idCorte", oCorteE.corte.idCorte);
                    p.AddWithValue("@idSucursal", oCorteE.sucursal.IdSucursal);
                    p.AddWithValue("@precioKg", oCorteE.precioKg);
                    p.AddWithValue("@cantKg", oCorteE.cantKgs);
                }
            );
        }

        public void quitarStockMedia(Entidades.MediaRes oMediaResE, int idCompra)
        {
            if (oMediaResE == null) throw new ArgumentNullException(nameof(oMediaResE));

            Db.NonQuery(
                _empresa,
                "quitarStockMedia",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idCompra", idCompra);
                    p.AddWithValue("@idMedia", oMediaResE.idMedia);
                    p.AddWithValue("@idSucursal", oMediaResE.sucursal.IdSucursal);
                }
            );
        }

        public void quitarStockTeoricoMedia(Entidades.MediaRes oMediaResE, int idCompra)
        {
            if (oMediaResE == null) throw new ArgumentNullException(nameof(oMediaResE));

            Db.NonQuery(
                _empresa,
                "quitarStockTeoricoMedia",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idCompra", idCompra);
                    p.AddWithValue("@idMedia", oMediaResE.idMedia);
                    p.AddWithValue("@idSucursal", oMediaResE.sucursal.IdSucursal);
                }
            );
        }

        public void quitarStockCorte(Entidades.CortePorCompra oCorteE, int idCompra)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            Db.NonQuery(
                _empresa,
                "quitarStockCorte",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idCompra", idCompra);
                    p.AddWithValue("@idCorte", oCorteE.corte.idCorte);
                    p.AddWithValue("@idSucursal", oCorteE.sucursal.IdSucursal);
                }
            );
        }

        public DataTable porcentajeCortesPorCompra(int idCompra)
        {
            return Db.DataTable(
                _empresa,
                "porcentajeCortesPorCompra",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@idCompra", idCompra)
            );
        }

        public DataTable getPromMedias(int idCompra)
        {
            return Db.DataTable(
                _empresa,
                "getPromMedias",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@id", idCompra)
            );
        }

        public DataTable getPorcCortesEnMedias(int idCompra)
        {
            return Db.DataTable(
                _empresa,
                "getPorcCortesEnMedias",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@id", idCompra)
            );
        }

        // Se comprueba que el Pesaje tenga el ajuste realizado. Retorna ID <> 0 si tiene
        public int getIdAjusteDelPesaje(int idPesaje)
        {
            string tipoAj = Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.AjusteStock);

            const string sql = "SELECT idCompra FROM dbo.Compras WHERE tipoCompra = @tipo AND nroRemito = @nroRemito;";

            object obj = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                p =>
                {
                    p.Add("@tipo", SqlDbType.NVarChar, 50).Value = tipoAj;
                    p.Add("@nroRemito", SqlDbType.NVarChar, 50).Value = idPesaje.ToString();
                }
            );

            return (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
        }

        public void actualizarEstadoPesaje(int idPesaje, Entidades.Compra.estadoAjusteStock estadoAjStock)
        {
            const string sql = "UPDATE Compras SET estado = @estado WHERE idCompra = @id;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                p =>
                {
                    p.Add("@estado", SqlDbType.NVarChar, 50).Value =
                        Entidades.Compra.estadoAjStockToString(estadoAjStock);
                    p.Add("@id", SqlDbType.Int).Value = idPesaje;
                }
            );
        }

        public void backup(string destino)
        {
            // OJO: esto siempre backuppea SuperCerdo. Mantengo tu lógica.
            // Si querés evitar RLS/tenant para operaciones admin, se puede usar una conexión separada a master.
            string rutaDestino = destino ?? "";
            string sentencia =
                "BACKUP DATABASE [SuperCerdo] TO DISK = @ruta " +
                "WITH NOFORMAT, INIT, NAME = N'SuperCerdo', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

            Db.NonQuery(
                _empresa,
                sentencia,
                CommandType.Text,
                p => p.Add("@ruta", SqlDbType.NVarChar, 400).Value = rutaDestino
            );
        }

        public void restaurarBD(string dataSource, string bdAuxiliar, string rutaOrigen)
        {
            // Esto va a otra DB (master/aux) => NO usa Db helper
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
