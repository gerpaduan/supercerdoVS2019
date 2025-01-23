using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using Entidades;

namespace Datos
{
    public class OtrasClases
    {
        Utilidades.Conexion conn = new Utilidades.Conexion();
        SqlCommand cmOtrasClases;
        SqlDataAdapter daOtrasClases;

        public bool Login(string clave)
        {
            cmOtrasClases = new SqlCommand();

            cmOtrasClases.Connection = conn.conectar();
            cmOtrasClases.Connection.Open();
            cmOtrasClases.CommandText = "select * from Claves where Clave="+clave;

            bool resp= Convert.ToBoolean(cmOtrasClases.ExecuteScalar());
            cmOtrasClases.Connection.Close();

            //Utilidades.FormLogin frmLogin = new Utilidades.FormLogin();
            //frmLogin.Logueado = resp;

            return resp;

        }

        public DataTable obtenerParametros()
        {
            DataTable dtParametros = new DataTable();
            daOtrasClases = new SqlDataAdapter("Select * from Parametros", conn.conectar());
            daOtrasClases.Fill(dtParametros);

            return dtParametros;
        }

        public void actualizarParametros(DataTable dtParametros)
        {
            cmOtrasClases = new SqlCommand();

            cmOtrasClases.Connection = conn.conectar();
            using (cmOtrasClases.Connection)
            {
                // Define el comando UPDATE del SqlDataAdapter
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT  id, nombre, valor, descripcion FROM Parametros", cmOtrasClases.Connection);

                //DataTable dt = new DataTable();
                //// Genera automáticamente los comandos INSERT, UPDATE y DELETE
                //SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adapter);
                //adapter.Fill(dt);
                //dtParametros.PrimaryKey = new DataColumn[] { dtParametros.Columns["id"] };
                //dt.PrimaryKey = new DataColumn[] { dt.Columns["id"] };
                //cmOtrasClases.Connection.Open();
                //dt = dtParametros;
                //// Ejecuta el update en la base de datos con los cambios del DataTable
                //adapter.Update(dt);
                //conn.cerraConexion();

                //Llenar el esquema del DataTable
                DataTable table = new DataTable();
                adapter.FillSchema(table, SchemaType.Source); // Importante para obtener información de claves primarias
                adapter.Fill(table);

                dtParametros.PrimaryKey = new DataColumn[] { dtParametros.Columns["id"] };
                //Realizar cambios en el DataTable
                table = dtParametros;
                table.PrimaryKey = new DataColumn[] { table.Columns["id"] };

                //Actualizar la base de datos
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                adapter.Update(table);

            }
        }

            #region Licencia
            public bool existeLicencia(string nroLicencia)
        {           
            cmOtrasClases = new SqlCommand();

            cmOtrasClases.Connection = conn.conectar();
            cmOtrasClases.Connection.Open();
            cmOtrasClases.CommandText = "Select COUNT(*) from Licencias Where nroLicencia = @nroLicencia";
            cmOtrasClases.Parameters.AddWithValue("@NroLicencia", nroLicencia);

            bool resp = Convert.ToBoolean(cmOtrasClases.ExecuteScalar());
            cmOtrasClases.Connection.Close();

            return resp;
        }

        public void agregarLicencia(string nroLicencia, string identificacion)
        {
            cmOtrasClases = new SqlCommand();

            cmOtrasClases.Connection = conn.conectar();
            cmOtrasClases.Connection.Open();

            cmOtrasClases.CommandText = "Insert into Licencias (NroLicencia, identificacion, creado) values (@NroLicencia, @identificacion, @creado)";
            cmOtrasClases.Parameters.AddWithValue("@NroLicencia", nroLicencia);
            cmOtrasClases.Parameters.AddWithValue("@identificacion", identificacion);
            cmOtrasClases.Parameters.AddWithValue("@creado", DateTime.Now);
            cmOtrasClases.ExecuteNonQuery();
            cmOtrasClases.Connection.Close();
        }


        #endregion

        #region VencimientosLicencia
        public DataTable obtenerVencimientoLicencia(DateTime fechaDesde)
        {
            DataTable dt = new DataTable();
            daOtrasClases = new SqlDataAdapter("SELECT fechaVencimiento, case WHEN pagado = 1 then 'PAGADO' ELSE 'PENDIENTE' END AS pagado, fechaPago "+
                "FROM VencimientosLicencia WHERE (pagado = 0 or fechaVencimiento > @fechaDesde) and (fechaVencimiento < DATEADD(MONTH, 2, GETDATE())) order by fechaVencimiento", conn.conectar());
            daOtrasClases.SelectCommand.Parameters.Add("@fechaDesde", SqlDbType.DateTime).Value = fechaDesde;
            daOtrasClases.Fill(dt);

            return dt;
        }

        public DateTime fechaVencimientoLicencia()
        {
            cmOtrasClases = new SqlCommand();

            cmOtrasClases.Connection = conn.conectar();
            cmOtrasClases.Connection.Open();
            cmOtrasClases.CommandText = "SELECT TOP 1 fechaVencimiento FROM VencimientosLicencia WHERE (pagado = 0) ORDER BY fechaVencimiento";

            DateTime resp = Convert.ToDateTime(cmOtrasClases.ExecuteScalar());
            cmOtrasClases.Connection.Close();

            return resp;
        }

        public bool existePagoLicenciaHoy()
        {
            cmOtrasClases = new SqlCommand();

            cmOtrasClases.Connection = conn.conectar();
            cmOtrasClases.Connection.Open();
            cmOtrasClases.CommandText = "SELECT COUNT(*) FROM VencimientosLicencia WHERE fechaPago = " + DateTime.Now.Date;

            bool resp = Convert.ToBoolean(cmOtrasClases.ExecuteScalar());
            cmOtrasClases.Connection.Close();

            return resp;
        }

        public void agregaVencimientosLicencia(DateTime fechaDesde)
        {
            cmOtrasClases = new SqlCommand();

            cmOtrasClases.Connection = conn.conectar();
            cmOtrasClases.Connection.Open();
            for (int i = 0; i < 400; i++)
            {                
                cmOtrasClases.CommandText = "Insert into VencimientosLicencia (fechaVencimiento, pagado) values (@fechaVencimiento, @pagado)";
                cmOtrasClases.Parameters.AddWithValue("@fechaVencimiento", fechaDesde.AddMonths(i));
                cmOtrasClases.Parameters.AddWithValue("@pagado", false);
                cmOtrasClases.ExecuteNonQuery();

                cmOtrasClases.Parameters.Clear();   
            }
            cmOtrasClases.Connection.Close();
        }
        public void agregarPagoCuota(DateTime fechaVencimiento)
        {
            cmOtrasClases = new SqlCommand();

            cmOtrasClases.Connection = conn.conectar();
            cmOtrasClases.Connection.Open();
            cmOtrasClases.CommandText = "UPDATE VencimientosLicencia SET pagado = @pagado, fechaPago = @fechaPago where fechaVencimiento = @fechaVencimiento";
            cmOtrasClases.Parameters.AddWithValue("@fechaVencimiento", fechaVencimiento);
            cmOtrasClases.Parameters.AddWithValue("@pagado", true);
            cmOtrasClases.Parameters.AddWithValue("@fechaPago", DateTime.Today);
            cmOtrasClases.ExecuteNonQuery();
            cmOtrasClases.Connection.Close();
        }

        #endregion

    }
}
