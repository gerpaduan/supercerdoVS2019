using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class Sucursal
    {
        private readonly Utilidades.Conexion _cx;
        private readonly IEmpresaContext _empresa;

        public Sucursal(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
            _cx = new Utilidades.Conexion();
        }

        public DataTable obtenerSucursales()
        {
            var dt = new DataTable();

            using (SqlConnection cn = _cx.conectar(_empresa)) // ya viene abierta
            using (SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM sucursal", cn))
            {
                da.Fill(dt);
            }

            return dt;
        }

        public Entidades.Sucursal findById(int id)
        {
            Entidades.Sucursal oSucursalE = null;

            using (SqlConnection cn = _cx.conectar(_empresa)) // ya viene abierta
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM sucursal WHERE idSucursal = @id", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        oSucursalE = mapSucursal(dr);
                }
            }

            // === Cargar Empresa si corresponde ===
            if (oSucursalE != null && oSucursalE.IdEmpresa > 0)
            {
                var emp = findEmpresaById(oSucursalE.IdEmpresa);
                oSucursalE.Empresa = emp;
            }

            return oSucursalE;
        }

        public List<Entidades.Sucursal> findAll()
        {
            var lista = new List<Entidades.Sucursal>();

            using (SqlConnection cn = _cx.conectar(_empresa)) // ya viene abierta
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM sucursal", cn))
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    lista.Add(mapSucursal(dr));
            }

            return lista;
        }

        public Entidades.Empresa findEmpresaById(int idEmpresa)
        {
            Entidades.Empresa oEmpresa = null;

            using (SqlConnection cn = _cx.conectar(_empresa)) // ya viene abierta
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Empresas WHERE idEmpresa = @id", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEmpresa;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        oEmpresa = mapEmpresa(dr);
                }
            }

            return oEmpresa;
        }

        public Entidades.Empresa findEmpresaByCuit(long cuit)
        {
            Entidades.Empresa oEmpresa = null;

            using (SqlConnection cn = _cx.conectar(_empresa)) // ya viene abierta
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Empresas WHERE cuit = @cuit", cn))
            {
                cmd.Parameters.Add("@cuit", SqlDbType.BigInt).Value = cuit;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        oEmpresa = mapEmpresa(dr);
                }
            }

            return oEmpresa;
        }

        public DataTable obtenerSucursalSanMartin()
        {
            var dt = new DataTable();

            using (SqlConnection cn = _cx.conectar(_empresa))
            using (SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM sucursal WHERE idSucursal = 2", cn))
            {
                da.Fill(dt);
            }

            return dt;
        }

        public DataTable obtenerSucursalSanLorenzo()
        {
            var dt = new DataTable();

            using (SqlConnection cn = _cx.conectar(_empresa))
            using (SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM sucursal WHERE idSucursal = 1", cn))
            {
                da.Fill(dt);
            }

            return dt;
        }

        public DataTable obtenerConexiones(bool? mostrarEnPrincipal, bool? mostrarEnStockActual)
        {
            var dt = new DataTable();

            // ✅ Parametrizado (evita concatenación)
            var sql = @"
                SELECT *
                FROM Conexiones
                WHERE (@mp IS NULL OR mostrarEnPrincipal = @mp)
                  AND (@ms IS NULL OR mostrarEnStockActual = @ms);";

            using (SqlConnection cn = _cx.conectar(_empresa))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, cn))
            {
                da.SelectCommand.Parameters.Add("@mp", SqlDbType.Bit).Value =
                    (object)mostrarEnPrincipal ?? DBNull.Value;

                da.SelectCommand.Parameters.Add("@ms", SqlDbType.Bit).Value =
                    (object)mostrarEnStockActual ?? DBNull.Value;

                da.Fill(dt);
            }

            return dt;
        }

        public int getIdSucursalByConexion(string nameConnString)
        {
            if (string.IsNullOrWhiteSpace(nameConnString))
                return 0;

            using (SqlConnection cn = _cx.conectar(_empresa)) // ya viene abierta
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 idSucursal FROM Conexiones WHERE name = @name", cn))
            {
                cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = nameConnString.Trim();

                object obj = cmd.ExecuteScalar();
                if (obj == null || obj == DBNull.Value) return 0;

                return Convert.ToInt32(obj);
            }
        }

        // --------------------
        // Mapeos
        // --------------------
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
    }
}
