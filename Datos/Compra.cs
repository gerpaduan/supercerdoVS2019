using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class Compra
    {
        Utilidades.Conexion conn = new Utilidades.Conexion();
        SqlDataAdapter daCompra;
        SqlCommand cmCompra;

        public void anularCompra(int idCompra)
        {
            cmCompra = new SqlCommand();
            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText="anularCompra";
            cmCompra.Parameters.AddWithValue("idCompra", idCompra);
            cmCompra.ExecuteNonQuery();
            cmCompra.Connection.Close();
            cmCompra = null;
        }

        public DataTable obtenerCompras(int idSucursal, string tipoCompra, string texto, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal)
        {
            DataTable dtCompras = new DataTable();
            daCompra = new SqlDataAdapter();

            cmCompra = new SqlCommand();
            cmCompra.Connection = string.IsNullOrEmpty(conexionSucursal) ? conn.conectar() : conn.conectar(conexionSucursal);
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "obtenerCompras";
            cmCompra.Parameters.AddWithValue("@texto", texto);
            cmCompra.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCompra.Parameters.AddWithValue("@fechaHasta", fechaHasta);
            cmCompra.Parameters.AddWithValue("@tipoCompra", tipoCompra);
            cmCompra.Parameters.AddWithValue("@idSucursal", idSucursal);

            daCompra.SelectCommand = cmCompra;
            daCompra.Fill(dtCompras);

            cmCompra.Connection.Close();

            return dtCompras;
        }

        public DataTable getLineasCompras(int idSucursal, string tipoCompra, string texto, string codigo, string corte, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal)
        {
            DataTable dtCompras = new DataTable();
            daCompra = new SqlDataAdapter();

            cmCompra = new SqlCommand();
            cmCompra.Connection = string.IsNullOrEmpty(conexionSucursal) ? conn.conectar() : conn.conectar(conexionSucursal);
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "getLineasCompras";
            cmCompra.Parameters.AddWithValue("@texto", texto);
            cmCompra.Parameters.AddWithValue("@codigo", codigo);
            cmCompra.Parameters.AddWithValue("@corte", corte);
            cmCompra.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCompra.Parameters.AddWithValue("@fechaHasta", fechaHasta);
            cmCompra.Parameters.AddWithValue("@tipoCompra", tipoCompra);
            cmCompra.Parameters.AddWithValue("@idSucursal", idSucursal);

            daCompra.SelectCommand = cmCompra;
            daCompra.Fill(dtCompras);

            cmCompra.Connection.Close();

            return dtCompras;
        }

        public DataTable findById(int idCompra)
        {
            DataTable dtCompras = new DataTable();
            daCompra = new SqlDataAdapter();

            cmCompra = new SqlCommand();
            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandText = "select * from Compras where idCompra = "+idCompra;

            daCompra.SelectCommand = cmCompra;
            daCompra.Fill(dtCompras);

            cmCompra.Connection.Close();

            return dtCompras;
        }

        public int agregarCompra(Entidades.Compra oCompraE)
        {            
            cmCompra = new SqlCommand();

            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "agregarCompra";

            cmCompra.Parameters.AddWithValue("@nroRemito", oCompraE.NroRemito);
            cmCompra.Parameters.AddWithValue("@fechaCompra", oCompraE.FechaCompra);
            cmCompra.Parameters.AddWithValue("@idProveedor", oCompraE.Proveedor.idPersona);
            cmCompra.Parameters.AddWithValue("@estado", oCompraE.Estado);
            cmCompra.Parameters.AddWithValue("@observaciones", oCompraE.Observaciones);
            cmCompra.Parameters.AddWithValue("@tipoCompra", oCompraE.TipoCompra);
            cmCompra.Parameters.AddWithValue("@cantMedias", oCompraE.CantMedias);
            cmCompra.Parameters.AddWithValue("@kgsMedias", oCompraE.KgsMedias);
            cmCompra.Parameters.AddWithValue("@enCtaCte", oCompraE.EnCtaCte);
            cmCompra.Parameters.AddWithValue("@idSucursal", oCompraE.Sucursal.idSucursal);
            cmCompra.Parameters.AddWithValue("@creadoPor", oCompraE.CreadoPor.Id);

            SqlDataReader drCompra = cmCompra.ExecuteReader();

            int idCompra = 0;
            while (drCompra.Read())
            {
                idCompra = Convert.ToInt32(drCompra["idCompra"].ToString());// Convert.ToInt32();

            }

            cmCompra.Connection.Close();

            cmCompra = null;

            return idCompra;

        }

        public void ModificarCompra(Entidades.Compra oCompraE)
        {
            cmCompra = new SqlCommand();

            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "modificarCompra";

            cmCompra.Parameters.AddWithValue("@idCompra", oCompraE.IdCompra);
            cmCompra.Parameters.AddWithValue("@nroRemito", oCompraE.NroRemito);
            cmCompra.Parameters.AddWithValue("@fechaCompra", oCompraE.FechaCompra);
            cmCompra.Parameters.AddWithValue("@idProveedor", oCompraE.Proveedor.idPersona);
            cmCompra.Parameters.AddWithValue("@estado", oCompraE.Estado);
            cmCompra.Parameters.AddWithValue("@observaciones", oCompraE.Observaciones);
            cmCompra.Parameters.AddWithValue("@tipoCompra", oCompraE.TipoCompra);
            cmCompra.Parameters.AddWithValue("@cantMedias", oCompraE.CantMedias);
            cmCompra.Parameters.AddWithValue("@kgsMedias", oCompraE.KgsMedias);
            cmCompra.Parameters.AddWithValue("@enCtaCte", oCompraE.EnCtaCte);
            cmCompra.Parameters.AddWithValue("@idSucursal", oCompraE.Sucursal.idSucursal);
            cmCompra.Parameters.AddWithValue("@actualizadoPor", oCompraE.ActualizadoPor.Id);

            cmCompra.ExecuteNonQuery();
            cmCompra.Connection.Close();

            cmCompra = null;
        }
        
        public float getTotalCompra(int idCompra, string tipoCompra)
        {
            DataTable dtTotalCompra = new DataTable();
            cmCompra = new SqlCommand();
            cmCompra.Connection = conn.conectar();
            string consulta = "";

            switch (Entidades.Compra.tipoCompraToEnum(tipoCompra))
            {
                case Entidades.Compra.tipoCompraEnum.Cortes:
                    consulta = "SELECT SUM(cantKg * precioKg) AS total " +
                                "FROM dbo.CortePorCompra " +
                                "WHERE     idCompra = " + idCompra + " " +
                                "GROUP BY idCompra";
                    break;
                case Entidades.Compra.tipoCompraEnum.MediaRes:
                    consulta = "SELECT SUM(kgMedia * precioMedia) AS total " +
                                "FROM dbo.MediaRes " +
                                "WHERE     idCompra = " + idCompra + " " +
                                "GROUP BY idCompra";
                    break;
            }

            cmCompra.CommandText = consulta;
            cmCompra.CommandType = CommandType.Text;
            cmCompra.Connection.Open();
            double totalCompraD = cmCompra.ExecuteScalar().Equals(DBNull.Value) ? 0 : (double)cmCompra.ExecuteScalar();
            float totalCompra = (float)totalCompraD;
            cmCompra.Connection.Close();
            return totalCompra;
        }

        public void modificarPrecioMedia(int idCompra, float precioKg)
        {
            cmCompra = new SqlCommand();

            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "modificarPrecioMedia";

            cmCompra.Parameters.AddWithValue("@idCompra", idCompra);
            cmCompra.Parameters.AddWithValue("@precioKg", precioKg);

            cmCompra.ExecuteNonQuery();
            cmCompra.Connection.Close();

            cmCompra = null;
        }

        public void agregarCortePorCompra(Entidades.CortePorCompra oCorteE)
        {
            cmCompra = new SqlCommand();

            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "agregarCortePorCompra";//se agrega el corte y se actualizan stock del corte y sus sub-cortes

            cmCompra.Parameters.AddWithValue("@idCompra", oCorteE.compra.IdCompra);
            cmCompra.Parameters.AddWithValue("@idCorte", oCorteE.corte.idCorte);
            cmCompra.Parameters.AddWithValue("@idSucursal", oCorteE.sucursal.IdSucursal);
            cmCompra.Parameters.AddWithValue("@precioKg", oCorteE.precioKg);
            cmCompra.Parameters.AddWithValue("@cantKg", oCorteE.cantKgs);
            cmCompra.Parameters.AddWithValue("@balanza", oCorteE.Balanza);
            cmCompra.Parameters.AddWithValue("@creado", oCorteE.Creado);
            cmCompra.Parameters.AddWithValue("@creadoPor", oCorteE.CreadoPor != null ? oCorteE.CreadoPor.Id : 0);

            cmCompra.ExecuteNonQuery();
            cmCompra.Connection.Close();

            cmCompra = null;
        }

        public void agregarMediaRes(Entidades.MediaRes oMediaResE)
        {
            cmCompra = new SqlCommand();

            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "agregarMediaRes";//se actualizan los stock de todos los cortes

            cmCompra.Parameters.AddWithValue("@idCompra", oMediaResE.compra.IdCompra);
            cmCompra.Parameters.AddWithValue("@nroTropa", oMediaResE.nroTropa);
            cmCompra.Parameters.AddWithValue("@idSucursal", oMediaResE.sucursal.IdSucursal);
            cmCompra.Parameters.AddWithValue("@precioMedia", oMediaResE.precioMedia);
            cmCompra.Parameters.AddWithValue("@kgMedia", oMediaResE.kgMedia);

            cmCompra.ExecuteNonQuery();
            cmCompra.Connection.Close();

            cmCompra = null;
        }

        //actualiza stock de todos los cortes existentes
        public void actualizarStockCortes()
        { 
        
        }

        public int obtenerIdUltimaCompra()
        {
            cmCompra=new SqlCommand();

            cmCompra.Connection = conn.conectar();
           
            cmCompra.CommandText="select top 1 * from Compras order by idCompra desc";
            cmCompra.Connection.Open();
            SqlDataReader drUltimaCompra=cmCompra.ExecuteReader();

            int idUltimaCompra, idCompraActual=0;
            while (drUltimaCompra.Read())
            {
                idUltimaCompra =Convert.ToInt32( drUltimaCompra[2].ToString());// Convert.ToInt32();
                idCompraActual = idUltimaCompra + 1;
            }
           
           conn.cerraConexion();
            return idCompraActual;
        }

        public DataTable obtenerCortesPorCompra(int idCompra)
        {
            DataTable dtCortesPorCompra = new DataTable();
            daCompra = new SqlDataAdapter();
            cmCompra = new SqlCommand();
            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "obtenerCortesPorCompra";
            cmCompra.Parameters.AddWithValue("@idCompra", idCompra);

            daCompra.SelectCommand = cmCompra;
            cmCompra.Connection.Close();

            daCompra.Fill(dtCortesPorCompra);

            return dtCortesPorCompra;
        }

        public DataTable obtenerMediasPorCompra(int idCompra)
        {
            DataTable dtMediasPorCompra = new DataTable();
            daCompra = new SqlDataAdapter();
            cmCompra = new SqlCommand();
            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "obtenerMediasPorCompra";
            cmCompra.Parameters.AddWithValue("@idCompra", idCompra);

            daCompra.SelectCommand = cmCompra;
            cmCompra.Connection.Close();

            daCompra.Fill(dtMediasPorCompra);

            return dtMediasPorCompra;
        }

        public void modificarMediaPorCompra(Entidades.MediaRes oMediaResE, int idCompra)
        {
            cmCompra = new SqlCommand();

            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "modificarMediaPorCompra";//se agrega el corte y se actualizan stock del corte y sus sub-cortes
            
            cmCompra.Parameters.AddWithValue("@idCompra", idCompra);
            cmCompra.Parameters.AddWithValue("@idMedia", oMediaResE.idMedia);
            cmCompra.Parameters.AddWithValue("@idSucursal", oMediaResE.sucursal.IdSucursal);
            cmCompra.Parameters.AddWithValue("@nroTropa", oMediaResE.nroTropa);
            cmCompra.Parameters.AddWithValue("@precioMedia", oMediaResE.precioMedia);
            cmCompra.Parameters.AddWithValue("@kgMedia", oMediaResE.kgMedia);

            cmCompra.ExecuteNonQuery();
            cmCompra.Connection.Close();

            cmCompra = null;
        }

        public void modificarCortePorCompra(Entidades.CortePorCompra oCorteE, int idCompra)
        {
            cmCompra = new SqlCommand();

            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "modificarCortePorCompra";//se agrega el corte y se actualizan stock del corte y sus sub-cortes

            cmCompra.Parameters.AddWithValue("@idCompra", idCompra);
            cmCompra.Parameters.AddWithValue("@idCorte", oCorteE.corte.idCorte);
            cmCompra.Parameters.AddWithValue("@idSucursal", oCorteE.sucursal.IdSucursal);
            cmCompra.Parameters.AddWithValue("@precioKg", oCorteE.precioKg);
            cmCompra.Parameters.AddWithValue("@cantKg", oCorteE.cantKgs);

            cmCompra.ExecuteNonQuery();
            cmCompra.Connection.Close();

            cmCompra = null;
        }

        public void quitarStockMedia(Entidades.MediaRes oMediaResE, int idCompra)
        {
            cmCompra = new SqlCommand();

            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "quitarStockMedia";

            cmCompra.Parameters.AddWithValue("@idCompra", idCompra);
            cmCompra.Parameters.AddWithValue("@idMedia", oMediaResE.idMedia);
            cmCompra.Parameters.AddWithValue("@idSucursal", oMediaResE.sucursal.IdSucursal);
            
            cmCompra.ExecuteNonQuery();
            cmCompra.Connection.Close();

            cmCompra = null;

        }

        public void quitarStockTeoricoMedia(Entidades.MediaRes oMediaResE, int idCompra)
        {
            cmCompra = new SqlCommand();

            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "quitarStockTeoricoMedia";

            cmCompra.Parameters.AddWithValue("@idCompra", idCompra);
            cmCompra.Parameters.AddWithValue("@idMedia", oMediaResE.idMedia);
            cmCompra.Parameters.AddWithValue("@idSucursal", oMediaResE.sucursal.IdSucursal);

            cmCompra.ExecuteNonQuery();
            cmCompra.Connection.Close();

            cmCompra = null;

        }

        public void quitarStockCorte(Entidades.CortePorCompra oCorteE, int idCompra)
        {
            cmCompra = new SqlCommand();

            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "quitarStockCorte";

            cmCompra.Parameters.AddWithValue("@idCompra", idCompra);
            cmCompra.Parameters.AddWithValue("@idCorte", oCorteE.corte.idCorte);
            cmCompra.Parameters.AddWithValue("@idSucursal", oCorteE.sucursal.IdSucursal);

            cmCompra.ExecuteNonQuery();
            cmCompra.Connection.Close();

            cmCompra = null;
        }

        public DataTable porcentajeCortesPorCompra(int idCompra)
        {
            DataTable dtPorcentajeCortesCompra = new DataTable();
            daCompra = new SqlDataAdapter();
            cmCompra = new SqlCommand();
            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "porcentajeCortesPorCompra";
            cmCompra.Parameters.AddWithValue("@idCompra", idCompra);

            daCompra.SelectCommand = cmCompra;
            cmCompra.Connection.Close();

            daCompra.Fill(dtPorcentajeCortesCompra);

            return dtPorcentajeCortesCompra;
        }

        public DataTable getPromMedias(int idCompra)
        {
            DataTable dt = new DataTable();
            daCompra = new SqlDataAdapter();
            cmCompra = new SqlCommand();
            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "getPromMedias";
            cmCompra.Parameters.AddWithValue("@id", idCompra);

            daCompra.SelectCommand = cmCompra;
            cmCompra.Connection.Close();

            daCompra.Fill(dt);

            return dt;

        }

        public DataTable getPorcCortesEnMedias(int idCompra)
        {
            DataTable dt = new DataTable();
            daCompra = new SqlDataAdapter();
            cmCompra = new SqlCommand();
            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();
            cmCompra.CommandType = CommandType.StoredProcedure; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "getPorcCortesEnMedias";
            cmCompra.Parameters.AddWithValue("@id", idCompra);

            daCompra.SelectCommand = cmCompra;
            cmCompra.Connection.Close();

            daCompra.Fill(dt);

            return dt;
        }

        //Se comprueba que el Pesaje tenga el ajuste realizado. Retorna ID <> 0 si tiene
        public int getIdAjusteDelPesaje(int idPesaje)
        {
            SqlCommand cmCompra = new SqlCommand();

            cmCompra.Connection = conn.conectar();

            //el Id del PesajeStock se lo registra en nroRemito en AjusteStock (esto se hace para no agregar otro campo a la tabla)
            cmCompra.CommandText = "SELECT idCompra FROM dbo.Compras " +
                                    "WHERE tipoCompra = '"+ Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.AjusteStock) +
                                    "' AND nroRemito = '"+ idPesaje.ToString() +"'";
            cmCompra.Connection.Open();
            SqlDataReader drCompra = cmCompra.ExecuteReader();

            int idAjuste = 0;
            while (drCompra.Read())
            {
                idAjuste = Convert.ToInt32(drCompra["idCompra"].ToString());
            }

            conn.cerraConexion();
            return idAjuste;
        }

        public void actualizarEstadoPesaje(int idPesaje, Entidades.Compra.estadoAjusteStock estadoAjStock)
        {

            cmCompra = new SqlCommand();

            cmCompra.Connection = conn.conectar();
            cmCompra.Connection.Open();

            cmCompra.CommandType = CommandType.Text; cmCompra.CommandTimeout = 90;
            cmCompra.CommandText = "UPDATE Compras SET estado = @estado WHERE idCompra = " + idPesaje;
            cmCompra.Parameters.AddWithValue("@estado", Entidades.Compra.estadoAjStockToString(estadoAjStock));

            cmCompra.ExecuteNonQuery();
            cmCompra.Connection.Close();

            cmCompra = null;
        
        }

        public void backup(string destino)
        {

          

            //string BDaCopiar = "MiBaseDeDatos";

            //string archivoParaLaCopia = @"C:\....\Copia.bak";

            //string sentencia = "BACKUP DATABASE [" + BDaCopiar + "] TO DISK='" + archivoParaLaCopia + "'";

           
            string rutaDestino = @destino;  //"E:\SuperCerdo\SuperCerdo.bak";

            //string sentencia ="backup database [SuperCerdo] to disk='"+rutaDestino+"' " ;

            string sentencia = "BACKUP DATABASE [SuperCerdo] TO  DISK ='" + rutaDestino + "' WITH NOFORMAT, INIT,  NAME = N'SuperCerdo', SKIP, NOREWIND, NOUNLOAD,  STATS = 10";
            SqlCommand cmCompra=new SqlCommand(sentencia);
            cmCompra.Connection=conn.conectar();
            cmCompra.Connection.Open();

            cmCompra.ExecuteNonQuery();

            cmCompra.Connection.Close();
        }

        public void restaurarBD(string dataSource, string bdAuxiliar, string rutaOrigen)
        { 
            SqlConnection connMaster=new SqlConnection();
            connMaster.ConnectionString = "Data Source='" + dataSource + "';Initial Catalog='" + bdAuxiliar + "';Integrated Security=True";

//            FROM DISK = 'C\:Dbname.bak'
//WITH MOVE 'Dbname_Data' TO 'C:\Data\datafile.mdf',
//MOVE 'Dbname_Log' TO 'C:\Data\logfile.ldf',
             
            string sentencia ="RESTORE DATABASE [SuperCerdo] FROM  DISK ='"+ rutaOrigen +"' WITH  FILE = 1,  NOUNLOAD,  REPLACE,  STATS = 10" ;


            SqlCommand cmCompra = new SqlCommand(sentencia);

            cmCompra.Connection = connMaster;
            cmCompra.Connection.Open();
            cmCompra.ExecuteNonQuery();
            cmCompra.Connection.Close();

            connMaster.ConnectionString = null;
            connMaster = null;
        }
    }
}
