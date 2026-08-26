using System;
using System.Collections.Generic;
using Npgsql;

namespace DatosPostgres
{
    // Backend Postgres del modulo de administracion de plataforma (alta/edicion de Empresas,
    // Sucursales, Usuarios cruzando TODOS los tenants, gate de superadmin, alicuotas IVA).
    // Contraparte de Web/Helpers/SystemAdministrationRepository.cs (SQL Server) -- consumida por
    // el adaptador Web/Helpers/SystemAdministrationRepositoryPg.cs (que traduce VM<->Entidades.*,
    // ver docs/DECISIONS.md 2026-08-25 para por que esta clase no vive junto a las demas
    // Contratos.IXRepository/DatosPostgres.XPg).
    //
    // Habla solo Entidades.*/primitivos -- cero Web.Models, para poder vivir en netstandard2.0.
    //
    // Todos los metodos usan AbrirAdmin (SET LOCAL ROLE carnisys_sysadmin_bypass, ver migracion
    // 20260825b), igual que el original usa Db.OpenAdmin (session_context EsAdminCarniSys=1) en
    // el 100% de sus metodos, sin excepcion -- este modulo es cross-tenant por diseno, el
    // filtrado por tenant queda a cargo explicito de cada WHERE idempresa, no de RLS.
    public class SystemAdministrationPg
    {
        private readonly string _connectionString;

        public SystemAdministrationPg(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
        }

        #region Conexion admin

        private NpgsqlConnection AbrirAdmin(out NpgsqlTransaction tx)
        {
            var cn = new NpgsqlConnection(_connectionString);
            cn.Open();
            tx = cn.BeginTransaction();
            using (var cmd = new NpgsqlCommand("SET LOCAL ROLE carnisys_sysadmin_bypass;", cn, tx))
                cmd.ExecuteNonQuery();
            return cn;
        }

        private object ScalarAdmin(string sql, Action<NpgsqlParameterCollection> setParams)
        {
            using (var cn = AbrirAdmin(out var tx))
            {
                try
                {
                    object value;
                    using (var cmd = new NpgsqlCommand(sql, cn, tx))
                    {
                        setParams?.Invoke(cmd.Parameters);
                        value = cmd.ExecuteScalar();
                    }
                    tx.Commit();
                    return value;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        private void NonQueryAdmin(string sql, Action<NpgsqlParameterCollection> setParams)
        {
            using (var cn = AbrirAdmin(out var tx))
            {
                try
                {
                    using (var cmd = new NpgsqlCommand(sql, cn, tx))
                    {
                        setParams?.Invoke(cmd.Parameters);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
                catch { tx.Rollback(); throw; }
            }
        }

        private List<T> ReaderAdmin<T>(string sql, Func<NpgsqlDataReader, T> map, Action<NpgsqlParameterCollection> setParams)
        {
            using (var cn = AbrirAdmin(out var tx))
            {
                try
                {
                    var lista = new List<T>();
                    using (var cmd = new NpgsqlCommand(sql, cn, tx))
                    {
                        setParams?.Invoke(cmd.Parameters);
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                                lista.Add(map(dr));
                        }
                    }
                    tx.Commit();
                    return lista;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        #endregion

        #region Helpers de mapeo

        private static bool ColumnaExiste(NpgsqlDataReader dr, string columna)
        {
            try { return dr.GetOrdinal(columna) >= 0; } catch { return false; }
        }

        private static string GetString(NpgsqlDataReader dr, string columna) =>
            ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value ? Convert.ToString(dr[columna]) : "";

        private static long GetLong(NpgsqlDataReader dr, string columna) =>
            ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value ? Convert.ToInt64(dr[columna]) : 0;

        private static bool GetBool(NpgsqlDataReader dr, string columna) =>
            ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value && Convert.ToBoolean(dr[columna]);

        private static byte GetByte(NpgsqlDataReader dr, string columna) =>
            ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value ? Convert.ToByte(dr[columna]) : (byte)0;

        private static DateTime GetDate(NpgsqlDataReader dr, string columna) =>
            ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value ? Convert.ToDateTime(dr[columna]) : default(DateTime);

        private static object NullIfEmpty(string value) =>
            string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();

        // Lectura defensiva por columna (igual que GetString): esta clase reusa el mismo mapper
        // tanto para el SELECT * de ObtenerEmpresa(id) como para el SELECT parcial (8 columnas)
        // de ObtenerEmpresas() -- acceder una columna ausente por indice tira
        // IndexOutOfRangeException en Npgsql, no null, asi que cada campo se chequea antes de leer.
        private Entidades.Empresa MapEmpresa(NpgsqlDataReader dr)
        {
            return new Entidades.Empresa
            {
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                RazonSocialAfip = GetString(dr, "razonsocialafip"),
                Cuit = GetLong(dr, "cuit"),
                NombreFantasia = GetString(dr, "nombrefantasia"),
                Slogan1 = GetString(dr, "slogan1"),
                Slogan2 = GetString(dr, "slogan2"),
                Slogan3 = GetString(dr, "slogan3"),
                Iibb = GetLong(dr, "iibb"),
                CondicionIVA = GetString(dr, "condicioniva"),
                InicioActividad = GetDate(dr, "inicioactividad"),
                TenantSlug = GetString(dr, "tenantslug"),
                Domicilio = GetString(dr, "domicilio"),
                Ciudad = GetString(dr, "ciudad"),
                Pais = GetString(dr, "pais"),
                Telefono = GetString(dr, "telefono"),
                Email = GetString(dr, "email"),
                BasePath = GetString(dr, "basepath"),
                EsRRII = GetBool(dr, "esrrii"),
                NombreCertificado_pfx = GetString(dr, "nombrecertificado_pfx"),
                Entorno_HOMO_PROD = GetString(dr, "entorno_homo_prod"),
                BaseDatosNombre = GetString(dr, "basedatosnombre"),
                Activa = GetByte(dr, "activa"),
                Observaciones = GetString(dr, "observaciones")
            };
        }

        private Entidades.Sucursal MapSucursal(NpgsqlDataReader dr, bool incluirEmpresaNombre)
        {
            var s = new Entidades.Sucursal
            {
                IdSucursal = Convert.ToInt32(dr["idsucursal"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                SucursalNombre = GetString(dr, "sucursal"),
                Direccion = GetString(dr, "direccion"),
                Localidad = GetString(dr, "localidad"),
                Provincia = GetString(dr, "provincia"),
                Pais = GetString(dr, "pais"),
                CodPuntoVentaAfip = dr["codpuntoventaafip"] == DBNull.Value ? 0 : Convert.ToInt32(dr["codpuntoventaafip"]),
                Creado = ColumnaExiste(dr, "creado") && dr["creado"] != DBNull.Value ? Convert.ToDateTime(dr["creado"]) : (DateTime?)null,
                Observaciones = ColumnaExiste(dr, "observaciones") ? GetString(dr, "observaciones") : ""
            };

            if (incluirEmpresaNombre)
                s.Empresa = new Entidades.Empresa { RazonSocialAfip = GetString(dr, "razonsocialafip") };

            return s;
        }

        private Entidades.Usuario MapUsuario(NpgsqlDataReader dr, bool incluirNombresRelacionados)
        {
            var u = new Entidades.Usuario
            {
                Id = Convert.ToInt32(dr["id"]),
                Nombre = GetString(dr, "nombre"),
                User = GetString(dr, "usuario"),
                Email = GetString(dr, "email"),
                IdEmpresa = dr["idempresa"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idempresa"]),
                IdSucursal = dr["idsucursaluser"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idsucursaluser"]),
                Admin = dr["admin"] != DBNull.Value && Convert.ToBoolean(dr["admin"]),
                Activo = dr["activo"] != DBNull.Value && Convert.ToBoolean(dr["activo"]),
                PermitirLoginFueraSucursal = ColumnaExiste(dr, "permitirloginfuerasucursal") && dr["permitirloginfuerasucursal"] != DBNull.Value && Convert.ToBoolean(dr["permitirloginfuerasucursal"])
            };

            if (incluirNombresRelacionados)
            {
                u.Empresa = new Entidades.Empresa { RazonSocialAfip = GetString(dr, "razonsocialafip") };
                u.SucursalNombre = GetString(dr, "sucursal");
            }

            return u;
        }

        #endregion

        public bool EsSuperAdmin(int idUsuario)
        {
            if (idUsuario <= 0) return false;

            object value = ScalarAdmin(
                "SELECT COALESCE(superadmin, false) FROM usuarios WHERE id = @id;",
                p => p.AddWithValue("id", idUsuario));

            return value != null && value != DBNull.Value && Convert.ToBoolean(value);
        }

        public List<Entidades.Empresa> ObtenerEmpresas()
        {
            const string sql = @"
                SELECT idempresa, razonsocialafip, nombrefantasia, cuit, condicioniva, telefono, email, activa
                FROM empresas
                ORDER BY activa DESC, razonsocialafip ASC, idempresa ASC;";

            return ReaderAdmin(sql, dr => MapEmpresa(dr), null);
        }

        public Entidades.Empresa ObtenerEmpresa(int idEmpresa)
        {
            var list = ReaderAdmin(
                "SELECT * FROM empresas WHERE idempresa = @idEmpresa LIMIT 1;",
                dr => MapEmpresa(dr),
                p => p.AddWithValue("idEmpresa", idEmpresa));

            return list.Count > 0 ? list[0] : null;
        }

        public int CrearEmpresa(Entidades.Empresa empresa, long codigoGenericoCodigo, string codigoGenericoNombre, int codigoGenericoIdAlicuotaIva)
        {
            using (var cn = AbrirAdmin(out var tx))
            {
                try
                {
                    int idEmpresa = CrearEmpresaInterna(cn, tx, empresa, codigoGenericoCodigo, codigoGenericoNombre, codigoGenericoIdAlicuotaIva);
                    tx.Commit();
                    return idEmpresa;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        // Traduccion directa del cuerpo real de dbo.AA_AltaEmpresa (extraido via sp_helptext en
        // vivo, 2026-08-25): validar CUIT unico, gap-fill de idEmpresa, insertar Empresa, copiar
        // EmpresaParametros desde la plantilla idEmpresa=-1, crear la Sucursal default, y los 2
        // productos fijos de Corte. LOCK TABLE en vez de SERIALIZABLE+UPDLOCK/HOLDLOCK: el alta
        // de empresa es rarisima, el lock de tabla evita el riesgo de serialization_failure sin
        // necesitar logica de reintento (decision documentada en docs/DECISIONS.md).
        private int CrearEmpresaInterna(NpgsqlConnection cn, NpgsqlTransaction tx, Entidades.Empresa empresa, long codigoGenericoCodigo, string codigoGenericoNombre, int codigoGenericoIdAlicuotaIva)
        {
            using (var cmdLock = new NpgsqlCommand("LOCK TABLE empresas IN SHARE ROW EXCLUSIVE MODE;", cn, tx))
                cmdLock.ExecuteNonQuery();

            if (empresa.Cuit > 0)
            {
                using (var cmd = new NpgsqlCommand("SELECT 1 FROM empresas WHERE cuit = @cuit LIMIT 1;", cn, tx))
                {
                    cmd.Parameters.AddWithValue("cuit", empresa.Cuit);
                    if (cmd.ExecuteScalar() != null)
                        throw new InvalidOperationException("Ya existe una empresa con ese mismo CUIT.");
                }
            }

            int idEmpresa;
            using (var cmd = new NpgsqlCommand(@"
                SELECT CASE
                    WHEN NOT EXISTS (SELECT 1 FROM empresas WHERE idempresa = 1) THEN 1
                    ELSE COALESCE(
                        (SELECT MIN(e.idempresa + 1) FROM empresas e
                         WHERE e.idempresa >= 1
                           AND NOT EXISTS (SELECT 1 FROM empresas e2 WHERE e2.idempresa = e.idempresa + 1)),
                        (SELECT MAX(idempresa) + 1 FROM empresas WHERE idempresa >= 1)
                    )
                END;", cn, tx))
            {
                idEmpresa = Convert.ToInt32(cmd.ExecuteScalar());
            }

            using (var cmd = new NpgsqlCommand(@"
                INSERT INTO empresas (idempresa, razonsocialafip, cuit, nombrefantasia, slogan1, slogan2, slogan3,
                    iibb, condicioniva, inicioactividad, tenantslug, domicilio, ciudad, pais, telefono, email,
                    basepath, esrrii, nombrecertificado_pfx, entorno_homo_prod, basedatosnombre, activa, observaciones)
                VALUES (@idEmpresa, @razonSocialAfip, @cuit, @nombreFantasia, @slogan1, @slogan2, @slogan3,
                    @iibb, @condicionIVA, @inicioActividad, @tenantSlug, @domicilio, @ciudad, @pais, @telefono,
                    @email, @basePath, @esRRII, @nombreCertificadoPfx, @entornoHomoProd, @baseDatosNombre,
                    @activa, @observaciones);", cn, tx))
            {
                cmd.Parameters.AddWithValue("idEmpresa", idEmpresa);
                cmd.Parameters.AddWithValue("razonSocialAfip", NullIfEmpty(empresa.RazonSocialAfip));
                cmd.Parameters.AddWithValue("cuit", empresa.Cuit > 0 ? (object)empresa.Cuit : DBNull.Value);
                cmd.Parameters.AddWithValue("nombreFantasia", NullIfEmpty(empresa.NombreFantasia));
                cmd.Parameters.AddWithValue("slogan1", NullIfEmpty(empresa.Slogan1));
                cmd.Parameters.AddWithValue("slogan2", NullIfEmpty(empresa.Slogan2));
                cmd.Parameters.AddWithValue("slogan3", NullIfEmpty(empresa.Slogan3));
                cmd.Parameters.AddWithValue("iibb", empresa.Iibb > 0 ? (object)empresa.Iibb : DBNull.Value);
                cmd.Parameters.AddWithValue("condicionIVA", NullIfEmpty(empresa.CondicionIVA));
                cmd.Parameters.AddWithValue("inicioActividad", empresa.InicioActividad == default(DateTime) ? (object)DBNull.Value : empresa.InicioActividad.Date);
                cmd.Parameters.AddWithValue("tenantSlug", NullIfEmpty(empresa.TenantSlug));
                cmd.Parameters.AddWithValue("domicilio", NullIfEmpty(empresa.Domicilio));
                cmd.Parameters.AddWithValue("ciudad", NullIfEmpty(empresa.Ciudad));
                cmd.Parameters.AddWithValue("pais", NullIfEmpty(empresa.Pais));
                cmd.Parameters.AddWithValue("telefono", NullIfEmpty(empresa.Telefono));
                cmd.Parameters.AddWithValue("email", NullIfEmpty(empresa.Email));
                cmd.Parameters.AddWithValue("basePath", NullIfEmpty(empresa.BasePath));
                cmd.Parameters.AddWithValue("esRRII", empresa.EsRRII);
                cmd.Parameters.AddWithValue("nombreCertificadoPfx", NullIfEmpty(empresa.NombreCertificado_pfx));
                cmd.Parameters.AddWithValue("entornoHomoProd", NullIfEmpty(empresa.Entorno_HOMO_PROD));
                cmd.Parameters.AddWithValue("baseDatosNombre", NullIfEmpty(empresa.BaseDatosNombre));
                cmd.Parameters.Add("activa", NpgsqlTypes.NpgsqlDbType.Smallint).Value = (short)empresa.Activa;
                cmd.Parameters.AddWithValue("observaciones", NullIfEmpty(empresa.Observaciones));
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new NpgsqlCommand(@"
                INSERT INTO empresaparametros (idempresa, idparametro, valor)
                SELECT @idEmpresa, ep.idparametro, ep.valor FROM empresaparametros ep
                WHERE ep.idempresa = -1
                  AND NOT EXISTS (SELECT 1 FROM empresaparametros ep2 WHERE ep2.idempresa = @idEmpresa AND ep2.idparametro = ep.idparametro);", cn, tx))
            {
                cmd.Parameters.AddWithValue("idEmpresa", idEmpresa);
                cmd.ExecuteNonQuery();
            }

            string nombreSucursal = "Suc." + (empresa.RazonSocialAfip ?? "");
            if (nombreSucursal.Length > 50) nombreSucursal = nombreSucursal.Substring(0, 50);

            using (var cmd = new NpgsqlCommand(@"
                INSERT INTO sucursal (sucursal, idempresa, direccion, localidad, provincia, pais, codpuntoventaafip, creado, observaciones)
                VALUES (@sucursal, @idEmpresa, NULL, NULL, NULL, NULL, NULL, @creado, @observaciones);", cn, tx))
            {
                cmd.Parameters.AddWithValue("sucursal", nombreSucursal);
                cmd.Parameters.AddWithValue("idEmpresa", idEmpresa);
                cmd.Parameters.AddWithValue("creado", DateTime.Today);
                cmd.Parameters.AddWithValue("observaciones", NullIfEmpty(empresa.Observaciones));
                cmd.ExecuteNonQuery();
            }

            InsertarProductoAjusteFormula(cn, tx, idEmpresa);
            InsertarProductoCodigoGenerico(cn, tx, idEmpresa, codigoGenericoCodigo, codigoGenericoNombre, codigoGenericoIdAlicuotaIva);

            return idEmpresa;
        }

        // Mismos valores exactos que CrearProductoAjusteFormulaInterna (SQL Server). Ver
        // Negocio/Corte.ObtenerProductoAjusteFormula y docs/DECISIONS.md 2026-08-22.
        private void InsertarProductoAjusteFormula(NpgsqlConnection cn, NpgsqlTransaction tx, int idEmpresa)
        {
            using (var cmd = new NpgsqlCommand(@"
                INSERT INTO corte
                (idempresa, codigo, corte, tipo, preciokg, habilitado, encierrestock,
                 independiente, ingresorapidoembutido, pesable, porcentaje, porcentajehueso,
                 desvioestandar, creado)
                VALUES
                (@idEmpresa, -1, 'Ajuste de Formula', 'Ajuste de Formula', 0, false, false,
                 0, false, false, 0, 0, 0, now());", cn, tx))
            {
                cmd.Parameters.AddWithValue("idEmpresa", idEmpresa);
                cmd.ExecuteNonQuery();
            }
        }

        // Mismos valores exactos que CrearProductoCodigoGenericoInterna (SQL Server), defaults
        // resueltos aca igual que alla (codigo<=0 -> 999999, nombre vacio -> "Codigo Generico",
        // idAlicuotaIva<=0 -> 4 = 10,5%).
        private void InsertarProductoCodigoGenerico(NpgsqlConnection cn, NpgsqlTransaction tx, int idEmpresa, long codigoGenericoCodigo, string codigoGenericoNombre, int codigoGenericoIdAlicuotaIva)
        {
            long codigo = codigoGenericoCodigo > 0 ? codigoGenericoCodigo : 999999;
            string nombre = !string.IsNullOrWhiteSpace(codigoGenericoNombre) ? codigoGenericoNombre : "Codigo Generico";
            int idAlicuotaIva = codigoGenericoIdAlicuotaIva > 0 ? codigoGenericoIdAlicuotaIva : 4;

            using (var cmd = new NpgsqlCommand(@"
                INSERT INTO corte
                (idempresa, codigo, corte, tipo, preciokg, habilitado, encierrestock,
                 independiente, ingresorapidoembutido, pesable, idalicuotaiva, porcentaje,
                 porcentajehueso, desvioestandar, creado)
                VALUES
                (@idEmpresa, @codigo, @nombre, 'Producto Generico', 0, true, false,
                 1, false, false, @idAlicuotaIva, 0, 0, 0, now());", cn, tx))
            {
                cmd.Parameters.AddWithValue("idEmpresa", idEmpresa);
                cmd.Parameters.AddWithValue("codigo", codigo);
                cmd.Parameters.AddWithValue("nombre", nombre);
                cmd.Parameters.AddWithValue("idAlicuotaIva", idAlicuotaIva);
                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarEmpresa(Entidades.Empresa empresa)
        {
            NonQueryAdmin(@"
                UPDATE empresas SET
                    razonsocialafip = @razonSocialAfip, cuit = @cuit, nombrefantasia = @nombreFantasia,
                    slogan1 = @slogan1, slogan2 = @slogan2, slogan3 = @slogan3, iibb = @iibb,
                    condicioniva = @condicionIVA, inicioactividad = @inicioActividad, tenantslug = @tenantSlug,
                    domicilio = @domicilio, ciudad = @ciudad, pais = @pais, telefono = @telefono, email = @email,
                    basepath = @basePath, esrrii = @esRRII, nombrecertificado_pfx = @nombreCertificadoPfx,
                    entorno_homo_prod = @entornoHomoProd, basedatosnombre = @baseDatosNombre, activa = @activa,
                    observaciones = @observaciones
                WHERE idempresa = @idEmpresa;",
                p =>
                {
                    p.AddWithValue("idEmpresa", empresa.IdEmpresa);
                    p.AddWithValue("razonSocialAfip", NullIfEmpty(empresa.RazonSocialAfip));
                    p.AddWithValue("cuit", empresa.Cuit > 0 ? (object)empresa.Cuit : DBNull.Value);
                    p.AddWithValue("nombreFantasia", NullIfEmpty(empresa.NombreFantasia));
                    p.AddWithValue("slogan1", NullIfEmpty(empresa.Slogan1));
                    p.AddWithValue("slogan2", NullIfEmpty(empresa.Slogan2));
                    p.AddWithValue("slogan3", NullIfEmpty(empresa.Slogan3));
                    p.AddWithValue("iibb", empresa.Iibb > 0 ? (object)empresa.Iibb : DBNull.Value);
                    p.AddWithValue("condicionIVA", NullIfEmpty(empresa.CondicionIVA));
                    p.AddWithValue("inicioActividad", empresa.InicioActividad == default(DateTime) ? (object)DBNull.Value : empresa.InicioActividad.Date);
                    p.AddWithValue("tenantSlug", NullIfEmpty(empresa.TenantSlug));
                    p.AddWithValue("domicilio", NullIfEmpty(empresa.Domicilio));
                    p.AddWithValue("ciudad", NullIfEmpty(empresa.Ciudad));
                    p.AddWithValue("pais", NullIfEmpty(empresa.Pais));
                    p.AddWithValue("telefono", NullIfEmpty(empresa.Telefono));
                    p.AddWithValue("email", NullIfEmpty(empresa.Email));
                    p.AddWithValue("basePath", NullIfEmpty(empresa.BasePath));
                    p.AddWithValue("esRRII", empresa.EsRRII);
                    p.AddWithValue("nombreCertificadoPfx", NullIfEmpty(empresa.NombreCertificado_pfx));
                    p.AddWithValue("entornoHomoProd", NullIfEmpty(empresa.Entorno_HOMO_PROD));
                    p.AddWithValue("baseDatosNombre", NullIfEmpty(empresa.BaseDatosNombre));
                    p.Add("activa", NpgsqlTypes.NpgsqlDbType.Smallint).Value = (short)empresa.Activa;
                    p.AddWithValue("observaciones", NullIfEmpty(empresa.Observaciones));
                });
        }

        public List<Entidades.Sucursal> ObtenerSucursales(int idEmpresa)
        {
            const string sql = @"
                SELECT s.*, e.razonsocialafip
                FROM sucursal s
                INNER JOIN empresas e ON e.idempresa = s.idempresa
                WHERE (@idEmpresa = 0 OR s.idempresa = @idEmpresa)
                ORDER BY e.razonsocialafip ASC, s.sucursal ASC, s.idsucursal ASC;";

            return ReaderAdmin(sql, dr => MapSucursal(dr, true), p => p.AddWithValue("idEmpresa", idEmpresa));
        }

        public Entidades.Sucursal ObtenerSucursal(int idSucursal)
        {
            var list = ReaderAdmin(
                "SELECT * FROM sucursal WHERE idsucursal = @idSucursal LIMIT 1;",
                dr => MapSucursal(dr, false),
                p => p.AddWithValue("idSucursal", idSucursal));

            return list.Count > 0 ? list[0] : null;
        }

        public int CrearSucursal(Entidades.Sucursal sucursal, int? idSucursalOrigenPuntoStock)
        {
            using (var cn = AbrirAdmin(out var tx))
            {
                try
                {
                    int idSucursalNueva;
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO sucursal (sucursal, idempresa, direccion, localidad, provincia, pais, codpuntoventaafip, creado, observaciones)
                        VALUES (@sucursal, @idEmpresa, @direccion, @localidad, @provincia, @pais, @codPuntoVentaAfip, @creado, @observaciones)
                        RETURNING idsucursal;", cn, tx))
                    {
                        SetSucursalParams(cmd.Parameters, sucursal);
                        idSucursalNueva = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    if (idSucursalNueva > 0)
                        SembrarPuntoStockSucursalNueva(cn, tx, sucursal.IdEmpresa, idSucursalNueva, idSucursalOrigenPuntoStock);

                    tx.Commit();
                    return idSucursalNueva;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        // Mismo criterio que SembrarPuntoStockSucursalNueva (SQL Server): cada producto existente
        // de la empresa necesita una fila en cortepuntostocksucursal para la sucursal nueva.
        private void SembrarPuntoStockSucursalNueva(NpgsqlConnection cn, NpgsqlTransaction tx, int idEmpresa, int idSucursalNueva, int? idSucursalOrigen)
        {
            bool copiarDeSucursalExistente = idSucursalOrigen.HasValue && idSucursalOrigen.Value > 0;

            string sql = copiarDeSucursalExistente
                ? @"
                    INSERT INTO cortepuntostocksucursal (idempresa, idcorte, idsucursal, puntostock)
                    SELECT c.idempresa, c.idcorte, @idSucursalNueva, COALESCE(origen.puntostock, 0)
                    FROM corte c
                    LEFT JOIN cortepuntostocksucursal origen
                        ON origen.idcorte = c.idcorte AND origen.idsucursal = @idSucursalOrigen
                    WHERE c.idempresa = @idEmpresa;"
                : @"
                    INSERT INTO cortepuntostocksucursal (idempresa, idcorte, idsucursal, puntostock)
                    SELECT c.idempresa, c.idcorte, @idSucursalNueva, 0
                    FROM corte c
                    WHERE c.idempresa = @idEmpresa;";

            using (var cmd = new NpgsqlCommand(sql, cn, tx))
            {
                cmd.Parameters.AddWithValue("idEmpresa", idEmpresa);
                cmd.Parameters.AddWithValue("idSucursalNueva", idSucursalNueva);
                if (copiarDeSucursalExistente)
                    cmd.Parameters.AddWithValue("idSucursalOrigen", idSucursalOrigen.Value);

                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarSucursal(Entidades.Sucursal sucursal)
        {
            using (var cn = AbrirAdmin(out var tx))
            {
                try
                {
                    ActualizarSucursalInterna(cn, tx, sucursal);
                    tx.Commit();
                }
                catch { tx.Rollback(); throw; }
            }
        }

        private void ActualizarSucursalInterna(NpgsqlConnection cn, NpgsqlTransaction tx, Entidades.Sucursal sucursal)
        {
            using (var cmd = new NpgsqlCommand(@"
                UPDATE sucursal SET
                    sucursal = @sucursal, idempresa = @idEmpresa, direccion = @direccion,
                    localidad = @localidad, provincia = @provincia, pais = @pais,
                    codpuntoventaafip = @codPuntoVentaAfip, observaciones = @observaciones
                WHERE idsucursal = @idSucursal;", cn, tx))
            {
                SetSucursalParams(cmd.Parameters, sucursal);
                cmd.Parameters.AddWithValue("idSucursal", sucursal.IdSucursal);
                cmd.ExecuteNonQuery();
            }
        }

        private static void SetSucursalParams(NpgsqlParameterCollection p, Entidades.Sucursal sucursal)
        {
            p.AddWithValue("sucursal", NullIfEmpty(sucursal.SucursalNombre));
            p.AddWithValue("idEmpresa", sucursal.IdEmpresa);
            p.AddWithValue("direccion", NullIfEmpty(sucursal.Direccion));
            p.AddWithValue("localidad", NullIfEmpty(sucursal.Localidad));
            p.AddWithValue("provincia", NullIfEmpty(sucursal.Provincia));
            p.AddWithValue("pais", NullIfEmpty(sucursal.Pais));
            p.AddWithValue("codPuntoVentaAfip", sucursal.CodPuntoVentaAfip > 0 ? (object)sucursal.CodPuntoVentaAfip : DBNull.Value);
            p.AddWithValue("creado", DateTime.Today);
            p.AddWithValue("observaciones", NullIfEmpty(sucursal.Observaciones));
        }

        public List<Entidades.Usuario> ObtenerUsuarios(int idEmpresa)
        {
            const string sql = @"
                SELECT u.*, e.razonsocialafip, s.sucursal
                FROM usuarios u
                LEFT JOIN empresas e ON e.idempresa = u.idempresa
                LEFT JOIN sucursal s ON s.idsucursal = u.idsucursaluser
                WHERE (@idEmpresa = 0 OR u.idempresa = @idEmpresa)
                ORDER BY e.razonsocialafip ASC, u.nombre ASC, u.usuario ASC;";

            return ReaderAdmin(sql, dr => MapUsuario(dr, true), p => p.AddWithValue("idEmpresa", idEmpresa));
        }

        public Entidades.Usuario ObtenerUsuario(int idUsuario)
        {
            var list = ReaderAdmin(
                "SELECT * FROM usuarios WHERE id = @id LIMIT 1;",
                dr => MapUsuario(dr, false),
                p => p.AddWithValue("id", idUsuario));

            return list.Count > 0 ? list[0] : null;
        }

        // clave (plaintext, columna legacy) se guarda igual que el original -- ver comentario de
        // UsuarioPg.addOrEditUser. passwordHash/Salt/Iterations se calculan en el adaptador Web/
        // (Utilidades.PasswordSecurity no es alcanzable desde este proyecto netstandard2.0) y
        // llegan ya resueltos; si vienen nulos/vacios, no se toca ninguna columna de hash.
        public int CrearUsuario(Entidades.Usuario usuario, string passwordHash, string passwordSalt, int passwordHashIterations)
        {
            using (var cn = AbrirAdmin(out var tx))
            {
                try
                {
                    int idUsuario = CrearUsuarioInterno(cn, tx, usuario, passwordHash, passwordSalt, passwordHashIterations);
                    tx.Commit();
                    return idUsuario;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        private int CrearUsuarioInterno(NpgsqlConnection cn, NpgsqlTransaction tx, Entidades.Usuario usuario, string passwordHash, string passwordSalt, int passwordHashIterations)
        {
            int idUsuario;
            using (var cmd = new NpgsqlCommand(@"
                INSERT INTO usuarios (nombre, usuario, email, clave, admin, activo, idempresa, idsucursaluser, colorform, permitirloginfuerasucursal)
                VALUES (@nombre, @usuario, @email, @clave, @admin, @activo, @idEmpresa, @idSucursalUser, @colorForm, @permitirLoginFueraSucursal)
                RETURNING id;", cn, tx))
            {
                cmd.Parameters.AddWithValue("nombre", NullIfEmpty(usuario.Nombre));
                cmd.Parameters.AddWithValue("usuario", NullIfEmpty(usuario.User));
                cmd.Parameters.AddWithValue("email", NullIfEmpty(usuario.Email));
                cmd.Parameters.AddWithValue("clave", NullIfEmpty(usuario.Clave));
                cmd.Parameters.AddWithValue("admin", usuario.Admin);
                cmd.Parameters.AddWithValue("activo", usuario.Activo);
                cmd.Parameters.AddWithValue("idEmpresa", usuario.IdEmpresa);
                cmd.Parameters.AddWithValue("idSucursalUser", usuario.IdSucursal > 0 ? usuario.IdSucursal : 0);
                cmd.Parameters.AddWithValue("colorForm", "SteelBlue");
                cmd.Parameters.AddWithValue("permitirLoginFueraSucursal", usuario.PermitirLoginFueraSucursal);
                idUsuario = Convert.ToInt32(cmd.ExecuteScalar());
            }

            if (idUsuario > 0 && !string.IsNullOrWhiteSpace(passwordHash))
                ActualizarPasswordWebSeguroInterna(cn, tx, idUsuario, passwordHash, passwordSalt, passwordHashIterations);

            return idUsuario;
        }

        public void ActualizarUsuario(Entidades.Usuario usuario, string claveNueva, string passwordHash, string passwordSalt, int passwordHashIterations)
        {
            using (var cn = AbrirAdmin(out var tx))
            {
                try
                {
                    bool actualizaClave = !string.IsNullOrWhiteSpace(claveNueva);

                    string sql = @"
                        UPDATE usuarios SET
                            nombre = @nombre, usuario = @usuario, email = @email, admin = @admin,
                            activo = @activo, idempresa = @idEmpresa, idsucursaluser = @idSucursalUser,
                            colorform = @colorForm, permitirloginfuerasucursal = @permitirLoginFueraSucursal"
                        + (actualizaClave ? ", clave = @clave" : "")
                        + " WHERE id = @id;";

                    using (var cmd = new NpgsqlCommand(sql, cn, tx))
                    {
                        cmd.Parameters.AddWithValue("id", usuario.Id);
                        cmd.Parameters.AddWithValue("nombre", NullIfEmpty(usuario.Nombre));
                        cmd.Parameters.AddWithValue("usuario", NullIfEmpty(usuario.User));
                        cmd.Parameters.AddWithValue("email", NullIfEmpty(usuario.Email));
                        cmd.Parameters.AddWithValue("admin", usuario.Admin);
                        cmd.Parameters.AddWithValue("activo", usuario.Activo);
                        cmd.Parameters.AddWithValue("idEmpresa", usuario.IdEmpresa);
                        cmd.Parameters.AddWithValue("idSucursalUser", usuario.IdSucursal > 0 ? usuario.IdSucursal : 0);
                        cmd.Parameters.AddWithValue("colorForm", "SteelBlue");
                        cmd.Parameters.AddWithValue("permitirLoginFueraSucursal", usuario.PermitirLoginFueraSucursal);
                        if (actualizaClave)
                            cmd.Parameters.AddWithValue("clave", claveNueva.Trim());
                        cmd.ExecuteNonQuery();
                    }

                    if (actualizaClave)
                        ActualizarPasswordWebSeguroInterna(cn, tx, usuario.Id, passwordHash, passwordSalt, passwordHashIterations);

                    tx.Commit();
                }
                catch { tx.Rollback(); throw; }
            }
        }

        private void ActualizarPasswordWebSeguroInterna(NpgsqlConnection cn, NpgsqlTransaction tx, int idUsuario, string passwordHash, string passwordSalt, int passwordHashIterations)
        {
            using (var cmd = new NpgsqlCommand(@"
                UPDATE usuarios SET
                    passwordhash = @passwordHash, passwordsalt = @passwordSalt,
                    passwordhashiterations = @passwordHashIterations, passwordupdatedatutc = now()
                WHERE id = @idUsuario;", cn, tx))
            {
                cmd.Parameters.AddWithValue("idUsuario", idUsuario);
                cmd.Parameters.AddWithValue("passwordHash", passwordHash ?? "");
                cmd.Parameters.AddWithValue("passwordSalt", passwordSalt ?? "");
                cmd.Parameters.AddWithValue("passwordHashIterations", passwordHashIterations);
                cmd.ExecuteNonQuery();
            }
        }

        // Mismas 2 fases que el original CrearAltaRapida: fase 1 crea la empresa (commit propio),
        // fase 2 (transaccion nueva) busca la sucursal default recien creada, la actualiza con los
        // datos del formulario, y crea el usuario admin. Ambas fases via AbrirAdmin -- no hace
        // falta el Db.Open(EmpresaContextFijo) tenant-scoped del original porque ya estamos en el
        // rol bypass.
        public int CrearAltaRapida(Entidades.Empresa empresa, long codigoGenericoCodigo, string codigoGenericoNombre, int codigoGenericoIdAlicuotaIva,
            Entidades.Sucursal sucursal, Entidades.Usuario usuario, string passwordHash, string passwordSalt, int passwordHashIterations)
        {
            int idEmpresa;
            using (var cn = AbrirAdmin(out var tx))
            {
                try
                {
                    idEmpresa = CrearEmpresaInterna(cn, tx, empresa, codigoGenericoCodigo, codigoGenericoNombre, codigoGenericoIdAlicuotaIva);
                    tx.Commit();
                }
                catch { tx.Rollback(); throw; }
            }

            using (var cn = AbrirAdmin(out var tx))
            {
                try
                {
                    int idSucursal;
                    using (var cmd = new NpgsqlCommand("SELECT idsucursal FROM sucursal WHERE idempresa = @idEmpresa ORDER BY idsucursal DESC LIMIT 1;", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("idEmpresa", idEmpresa);
                        object value = cmd.ExecuteScalar();
                        idSucursal = value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
                    }

                    if (idSucursal <= 0)
                        throw new InvalidOperationException("No se pudo generar la sucursal inicial de la empresa.");

                    sucursal.IdSucursal = idSucursal;
                    sucursal.IdEmpresa = idEmpresa;
                    ActualizarSucursalInterna(cn, tx, sucursal);

                    usuario.IdEmpresa = idEmpresa;
                    usuario.IdSucursal = idSucursal;
                    int idUsuario = CrearUsuarioInterno(cn, tx, usuario, passwordHash, passwordSalt, passwordHashIterations);

                    tx.Commit();
                    return idUsuario;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        public bool ExisteCuit(long cuit, int idEmpresaExcluir)
        {
            object value = ScalarAdmin(
                "SELECT COUNT(1) FROM empresas WHERE cuit = @cuit AND (@idEmpresaExcluir <= 0 OR idempresa <> @idEmpresaExcluir);",
                p =>
                {
                    p.AddWithValue("cuit", cuit);
                    p.AddWithValue("idEmpresaExcluir", idEmpresaExcluir);
                });

            return value != null && value != DBNull.Value && Convert.ToInt64(value) > 0;
        }

        public bool ExisteUsuario(string usuario, int idUsuarioExcluir)
        {
            object value = ScalarAdmin(
                "SELECT COUNT(1) FROM usuarios WHERE LOWER(COALESCE(usuario, '')) = LOWER(@usuario) AND (@idUsuarioExcluir <= 0 OR id <> @idUsuarioExcluir);",
                p =>
                {
                    p.AddWithValue("usuario", usuario ?? "");
                    p.AddWithValue("idUsuarioExcluir", idUsuarioExcluir);
                });

            return value != null && value != DBNull.Value && Convert.ToInt64(value) > 0;
        }

        public bool ExisteEmail(string email, int idUsuarioExcluir)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            object value = ScalarAdmin(
                "SELECT COUNT(1) FROM usuarios WHERE LOWER(COALESCE(email, '')) = LOWER(@email) AND (@idUsuarioExcluir <= 0 OR id <> @idUsuarioExcluir);",
                p =>
                {
                    p.AddWithValue("email", email);
                    p.AddWithValue("idUsuarioExcluir", idUsuarioExcluir);
                });

            return value != null && value != DBNull.Value && Convert.ToInt64(value) > 0;
        }

        public List<Entidades.AlicuotaIva> ObtenerAlicuotasIva()
        {
            return ReaderAdmin(
                "SELECT idiva, iva FROM alicuotasiva WHERE mostrar = true ORDER BY iva;",
                dr => new Entidades.AlicuotaIva
                {
                    IdIva = Convert.ToInt32(dr["idiva"]),
                    Iva = Convert.ToSingle(dr["iva"])
                },
                null);
        }

        // Tabla "iva": catalogo global sin idempresa (verificado en vivo, 4 filas: Consumidor
        // Final/Responsable Inscripto/Monotributista/Exento) -- sin scoping por tenant en ningun
        // motor, igual que PersonaPg.getIva() (SELECT * FROM iva, sin WHERE).
        public List<Entidades.CondicionIva> ObtenerCondicionesIva()
        {
            return ReaderAdmin(
                "SELECT id, iva FROM iva ORDER BY iva;",
                dr => new Entidades.CondicionIva
                {
                    Id = Convert.ToInt32(dr["id"]),
                    Descripcion = GetString(dr, "iva")
                },
                null);
        }
    }
}
