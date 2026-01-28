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

            // === Cargar Empresa si corresponde ===
            if (oSucursalE != null && oSucursalE.IdEmpresa > 0)
            {
                var empresa = findEmpresaById(oSucursalE.IdEmpresa);
                oSucursalE.Empresa = empresa; // suponiendo que Sucursal tiene propiedad Empresa
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
                CodPuntoVentaAfip = dr["codPuntoVentaAfip"] != DBNull.Value ? Convert.ToInt32(dr["codPuntoVentaAfip"]) : 0,
                Direccion = dr["direccion"] != DBNull.Value ? dr["direccion"].ToString() : string.Empty,
                Localidad = dr["localidad"] != DBNull.Value ? dr["localidad"].ToString() : string.Empty,
                Provincia = dr["provincia"] != DBNull.Value ? dr["provincia"].ToString() : string.Empty,
                Pais = dr["pais"] != DBNull.Value ? dr["pais"].ToString() : string.Empty
            };
        }

        public Entidades.Empresa findEmpresaById(int idEmpresa)
        {
            Entidades.Empresa oEmpresa = null;

            using (SqlConnection conn = this.conn.conectar())
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Empresas WHERE idEmpresa = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", idEmpresa);

                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        oEmpresa = mapEmpresa(dr);
                    }
                }
            }

            return oEmpresa;
        }

        public Entidades.Empresa findEmpresaByCuit(long cuit)
        {
            Entidades.Empresa oEmpresa = null;

            using (SqlConnection conn = this.conn.conectar())
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Empresas WHERE cuit = @cuit", conn))
            {
                cmd.Parameters.AddWithValue("@cuit", cuit);

                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        oEmpresa = mapEmpresa(dr);
                    }
                }
            }

            return oEmpresa;
        }

        private Entidades.Empresa mapEmpresa(SqlDataReader dr)
        {
            return new Entidades.Empresa
            {
                IdEmpresa = dr["idEmpresa"] != DBNull.Value ? Convert.ToInt32(dr["idEmpresa"]) : 0,
                RazonSocialAfip = dr["razonSocialAfip"]?.ToString(),
                Cuit = dr["cuit"] != DBNull.Value ? Convert.ToInt64(dr["cuit"]) : 0,
                NombreFantasia = dr["nombreFantasia"]?.ToString(),
                Slogan1 = dr["slogan1"]?.ToString(),
                Slogan2 = dr["slogan2"]?.ToString(),
                Slogan3 = dr["slogan3"]?.ToString(),
                Iibb = dr["iibb"] != DBNull.Value ? Convert.ToInt64(dr["iibb"]) : 0,
                CondicionIVA = dr["condicionIVA"]?.ToString(),
                InicioActividad = dr["inicioActividad"] != DBNull.Value ? Convert.ToDateTime(dr["inicioActividad"]) : DateTime.MinValue,
                TenantSlug = dr["tenantSlug"]?.ToString(),
                Domicilio = dr["domicilio"]?.ToString(),
                Ciudad = dr["ciudad"]?.ToString(),
                Pais = dr["pais"]?.ToString(),
                Telefono = dr["telefono"]?.ToString(),
                Email = dr["email"]?.ToString(),
                BasePath = dr["basePath"]?.ToString(),
                EsRRII = dr["esRRII"] != DBNull.Value ? Convert.ToByte(dr["esRRII"]) : (byte)0,
                NombreCertificado_pfx = dr["nombreCertificado_pfx"]?.ToString(),
                Entorno_HOMO_PROD = dr["entorno_HOMO_PROD"]?.ToString()
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
