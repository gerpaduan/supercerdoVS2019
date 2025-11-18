using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class Sucursal
    {
        Utilidades.Conexion conn= new Utilidades.Conexion();
        SqlDataAdapter daSucursal;
       
        public DataTable obtenerSucursales()
        {
            DataTable dtSucursal = new DataTable();
            daSucursal = new SqlDataAdapter("Select * from sucursal", conn.conectar());
            daSucursal.Fill(dtSucursal);

            return dtSucursal;
        }

        //public Entidades.Sucursal findById(int id)
        //{
        //    Entidades.Sucursal oSucursalE = null;

        //    using (SqlConnection conn = this.conn.conectar())
        //    using (SqlCommand cmd = new SqlCommand("SELECT idSucursal, sucursal FROM sucursal WHERE idSucursal = @id", conn))
        //    {
        //        cmd.Parameters.AddWithValue("@id", id);

        //        conn.Open();
        //        using (SqlDataReader dr = cmd.ExecuteReader())
        //        {
        //            if (dr.Read())
        //            {
        //                oSucursalE = new Entidades.Sucursal
        //                {
        //                    idSucursal = Convert.ToInt32(dr["idSucursal"]),
        //                    sucursal = dr["sucursal"].ToString()
        //                };
        //            }
        //        }
        //    }

        //    return oSucursalE;
        //}
        public Entidades.Sucursal findById(int id)
        {
            Entidades.Sucursal oSucursalE = null;

            using (SqlConnection conn = this.conn.conectar())
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM sucursal WHERE idSucursal = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        oSucursalE = mapSucursal(dr);
                    }
                }
            }

            return oSucursalE;
        }

        public List<Entidades.Sucursal> findAll()
        {
            List<Entidades.Sucursal> lista = new List<Entidades.Sucursal>();

            using (SqlConnection conn = this.conn.conectar())
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM sucursal", conn))
            {
                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(mapSucursal(dr));
                    }
                }
            }

            return lista;
        }


        private Entidades.Sucursal mapSucursal(SqlDataReader dr)
        {
            return new Entidades.Sucursal
            {
                IdSucursal = dr["idSucursal"] != DBNull.Value ? Convert.ToInt32(dr["idSucursal"]) : 0,
                SucursalNombre = dr["sucursal"] != DBNull.Value ? dr["sucursal"].ToString() : string.Empty,
                IdEmpresa = dr["idEmpresa"] != DBNull.Value ? Convert.ToInt32(dr["idEmpresa"]) : 0,
                CodPtoVentaARCA = dr["codPtoVentaARCA"] != DBNull.Value ? Convert.ToInt32(dr["codPtoVentaARCA"]) : 0,
                Direccion = dr["direccion"] != DBNull.Value ? dr["direccion"].ToString() : string.Empty,
                Localidad = dr["localidad"] != DBNull.Value ? dr["localidad"].ToString() : string.Empty,
                Provincia = dr["provincia"] != DBNull.Value ? dr["provincia"].ToString() : string.Empty,
                Pais = dr["pais"] != DBNull.Value ? dr["pais"].ToString() : string.Empty
            };
        }


        public DataTable obtenerSucursalSanMartin()
        {
            DataTable dtSucursal = new DataTable();
            daSucursal = new SqlDataAdapter("Select * from sucursal where idSucursal = 2", conn.conectar());
            daSucursal.Fill(dtSucursal);

            return dtSucursal;

        }

        public DataTable obtenerSucursalSanLorenzo()
        {
            DataTable dtSucursal = new DataTable();
            daSucursal = new SqlDataAdapter("Select * from sucursal where idSucursal = 1", conn.conectar());
            daSucursal.Fill(dtSucursal);

            return dtSucursal;
        }

        public DataTable obtenerConexiones(bool? mostrarEnPrincipal, bool? mostrarEnStockActual)
        {
            DataTable dtConexiones = new DataTable();
            string consulta = "Select * from Conexiones WHERE 1=1 ";
            consulta += mostrarEnPrincipal == null ? " AND 1=1" : " AND mostrarEnPrincipal = " +
                (Convert.ToBoolean(mostrarEnPrincipal) ? 1 : 0);
            consulta += mostrarEnStockActual == null ? " AND 1=1" : " AND mostrarEnStockActual = " + 
                (Convert.ToBoolean(mostrarEnStockActual) ? 1 : 0);
            daSucursal = new SqlDataAdapter(consulta, conn.conectar());
            daSucursal.Fill(dtConexiones);

            return dtConexiones;
        }

        public int getIdSucursalByConexion(string nameConnString)
        {
            SqlCommand cmSucursal = new SqlCommand();

            cmSucursal.Connection = conn.conectar();

            cmSucursal.CommandText = "select top 1 idSucursal from Conexiones where name = \'" + nameConnString + "\'";
            cmSucursal.Connection.Open();
            SqlDataReader drSucursal = cmSucursal.ExecuteReader();

            int idSucursal = 0;
            while (drSucursal.Read())
            {
                idSucursal = Convert.ToInt32(drSucursal["idSucursal"].ToString());
            }

            conn.cerraConexion();
            return idSucursal;
        }

    }
}
