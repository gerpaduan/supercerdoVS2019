using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.ITitularFiscalRepository. Titulares fiscales por empresa y por sucursal
    // (2026-10-09, solo Postgres) -- ver docs/DECISIONS.md.
    //
    // El titular PRINCIPAL (espredeterminado) es una fila "ancla": su CUIT es el de la empresa y sus datos fiscales
    // (razon social, condicion IVA, IIBB, inicio de actividades, certificado) se leen SIEMPRE de la tabla empresas; la
    // fila solo guarda sus topes y la ventana anual. Los titulares no principales guardan sus propios datos
    // (copiados de la persona al guardarlos).
    public class TitularFiscalPg : Contratos.ITitularFiscalRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public TitularFiscalPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        // Datos "efectivos": en el principal salen de empresas; en el resto, de la fila del titular. La condicion IVA
        // del principal es RI (2) si empresas.esrrii, si no Monotributo (3): cualquier no-RI emite C.
        private const string SelectBase = @"
            SELECT t.idtitular, t.idempresa, t.idpersona, t.espredeterminado, (t.habilitado AND NOT t.revocado) AS habilitado, t.revocado, t.revocado_en,
                   CASE WHEN t.espredeterminado THEN COALESCE(e.cuit, 0) ELSE COALESCE(t.cuit, 0) END AS cuit,
                   CASE WHEN t.espredeterminado THEN e.razonsocialafip ELSE t.razonsocial END AS razonsocial,
                   CASE WHEN t.espredeterminado THEN (CASE WHEN COALESCE(e.esrrii, false) THEN 2 ELSE 3 END) ELSE COALESCE(t.idiva, 0) END AS idiva,
                   CASE WHEN t.espredeterminado THEN e.condicioniva ELSE t.condicioniva END AS condicioniva,
                   CASE WHEN t.espredeterminado THEN e.inicioactividad ELSE t.inicioactividad END AS inicioactividad,
                   CASE WHEN t.espredeterminado THEN COALESCE(e.iibb, 0) ELSE COALESCE(t.iibb, 0) END AS iibb,
                   CASE WHEN t.espredeterminado THEN e.nombrecertificado_pfx
                        ELSE COALESCE(NULLIF(t.nombrecertificado_pfx, ''),
                                      (SELECT c.nombrecertificado_pfx FROM certificados_arca_cuit c WHERE c.cuit = t.cuit ORDER BY (c.entorno = 'PROD') DESC LIMIT 1)) END AS nombrecertificado_pfx,
                   t.limitemensual, t.limiteanual, t.ventanaanual, t.creado
            FROM titulares_fiscales t
            JOIN empresas e ON e.idempresa = t.idempresa ";

        private static Entidades.TitularFiscal Mapear(System.Data.IDataRecord dr)
        {
            return new Entidades.TitularFiscal
            {
                IdTitular = Convert.ToInt32(dr["idtitular"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                IdPersona = dr["idpersona"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["idpersona"]),
                EsPredeterminado = Convert.ToBoolean(dr["espredeterminado"]),
                Habilitado = Convert.ToBoolean(dr["habilitado"]),
                Revocado = Convert.ToBoolean(dr["revocado"]),
                RevocadoEn = dr["revocado_en"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["revocado_en"]),
                Cuit = Convert.ToInt64(dr["cuit"]),
                RazonSocial = dr["razonsocial"] == DBNull.Value ? "" : Convert.ToString(dr["razonsocial"]),
                IdIva = Convert.ToInt32(dr["idiva"]),
                CondicionIVA = dr["condicioniva"] == DBNull.Value ? "" : Convert.ToString(dr["condicioniva"]),
                InicioActividad = dr["inicioactividad"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["inicioactividad"]),
                Iibb = Convert.ToInt64(dr["iibb"]),
                NombreCertificadoPfx = dr["nombrecertificado_pfx"] == DBNull.Value ? "" : Convert.ToString(dr["nombrecertificado_pfx"]),
                LimiteMensual = dr["limitemensual"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["limitemensual"]),
                LimiteAnual = dr["limiteanual"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["limiteanual"]),
                VentanaAnual = Convert.ToString(dr["ventanaanual"]),
                Creado = Convert.ToDateTime(dr["creado"])
            };
        }

        // Crea el titular principal de la empresa si falta (empresas importadas despues de la migracion). Idempotente y
        // concurrente: el indice unico parcial resuelve la carrera. Solo para la empresa de la sesion (la policy RLS no
        // deja insertar para otra).
        private void AsegurarPrincipal(int idEmpresa)
        {
            if (idEmpresa != _idEmpresa) return;
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO titulares_fiscales (idempresa, cuit, espredeterminado, habilitado)
                SELECT e.idempresa, CASE WHEN e.cuit > 0 THEN e.cuit END, true, true
                FROM empresas e
                WHERE e.idempresa = @idEmpresa
                  AND NOT EXISTS (SELECT 1 FROM titulares_fiscales t WHERE t.idempresa = e.idempresa AND t.espredeterminado)
                ON CONFLICT (idempresa) WHERE espredeterminado DO NOTHING;",
                p => p.AddWithValue("idEmpresa", idEmpresa));
        }

        public List<Entidades.TitularFiscal> Listar(int idEmpresa, bool soloHabilitados)
        {
            AsegurarPrincipal(idEmpresa);

            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                SelectBase + "WHERE t.idempresa = @idEmpresa" + (soloHabilitados ? " AND t.habilitado AND NOT t.revocado" : "") +
                " ORDER BY t.espredeterminado DESC, t.razonsocial, t.idtitular;",
                Mapear,
                p => p.AddWithValue("idEmpresa", idEmpresa));

            if (lista.Count == 0) return lista;

            // Puntos de venta de ARCA de cada titular.
            var puntos = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT idtitular, codpuntoventaafip, descripcion, habilitado, origen FROM titulares_fiscales_ptovta WHERE idempresa = @idEmpresa ORDER BY codpuntoventaafip;",
                MapearPuntoVenta,
                p => p.AddWithValue("idEmpresa", idEmpresa));
            foreach (var t in lista)
                t.PuntosVenta = puntos.Where(x => x.IdTitular == t.IdTitular).ToList();

            // Sucursales donde cada titular es el asignado (NULL = el principal).
            var usos = DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT s.sucursal AS nombre,
                       COALESCE(s.idtitularfacturacion,
                                (SELECT p.idtitular FROM titulares_fiscales p WHERE p.idempresa = s.idempresa AND p.espredeterminado)) AS idt
                FROM sucursal s WHERE s.idempresa = @idEmpresa ORDER BY s.sucursal;",
                dr => new KeyValuePair<int, string>(dr["idt"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idt"]), Convert.ToString(dr["nombre"])),
                p => p.AddWithValue("idEmpresa", idEmpresa));
            foreach (var t in lista)
                t.SucursalesQueLoUsan = usos.Where(u => u.Key == t.IdTitular).Select(u => u.Value).ToList();

            return lista;
        }

        public Entidades.TitularFiscal Obtener(int idTitular)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                SelectBase + "WHERE t.idtitular = @id;",
                Mapear,
                p => p.AddWithValue("id", idTitular));
            var titular = lista.FirstOrDefault();
            if (titular != null)
                titular.PuntosVenta = ListarPuntosVenta(idTitular);
            return titular;
        }

        public Entidades.TitularFiscal ObtenerPrincipal(int idEmpresa)
        {
            AsegurarPrincipal(idEmpresa);
            return DbPg.Reader(_connectionString, _idEmpresa,
                SelectBase + "WHERE t.idempresa = @idEmpresa AND t.espredeterminado;",
                Mapear,
                p => p.AddWithValue("idEmpresa", idEmpresa)).FirstOrDefault();
        }

        public int Guardar(Entidades.TitularFiscal titular)
        {
            try
            {
                if (titular.EsPredeterminado)
                {
                    // Principal: solo topes y ventana anual (sus datos fiscales son los de la empresa).
                    DbPg.NonQuery(_connectionString, _idEmpresa, @"
                        UPDATE titulares_fiscales SET limitemensual = @limMes, limiteanual = @limAnio, ventanaanual = @ventana
                        WHERE idtitular = @id;",
                        p =>
                        {
                            p.AddWithValue("id", titular.IdTitular);
                            p.AddWithValue("limMes", (object)titular.LimiteMensual ?? DBNull.Value);
                            p.AddWithValue("limAnio", (object)titular.LimiteAnual ?? DBNull.Value);
                            p.AddWithValue("ventana", titular.VentanaAnual);
                        });
                    return titular.IdTitular;
                }

                if (titular.IdTitular <= 0)
                {
                    object id = DbPg.Scalar(_connectionString, _idEmpresa, @"
                        INSERT INTO titulares_fiscales (idempresa, idpersona, cuit, espredeterminado, habilitado, razonsocial, idiva,
                            condicioniva, inicioactividad, iibb, limitemensual, limiteanual, ventanaanual)
                        VALUES (@idEmpresa, @idPersona, @cuit, false, true, @razon, @idIva, @condicion, @inicio, @iibb, @limMes, @limAnio, @ventana)
                        RETURNING idtitular;",
                        p => ParametrosDatos(p, titular));
                    return Convert.ToInt32(id);
                }

                // El CUIT no se actualiza: es la carpeta del certificado.
                DbPg.NonQuery(_connectionString, _idEmpresa, @"
                    UPDATE titulares_fiscales SET idpersona = @idPersona, razonsocial = @razon, idiva = @idIva, condicioniva = @condicion,
                        inicioactividad = @inicio, iibb = @iibb, limitemensual = @limMes, limiteanual = @limAnio, ventanaanual = @ventana
                    WHERE idtitular = @id AND NOT espredeterminado AND NOT revocado;",
                    p =>
                    {
                        ParametrosDatos(p, titular);
                        p.AddWithValue("id", titular.IdTitular);
                    });
                return titular.IdTitular;
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                // Indices unicos (idempresa, cuit) o (idempresa, idpersona): la empresa ya tiene a ese CUIT o a esa persona
                // (puede estar con la titularidad cancelada: en ese caso hay que reponerla, no crearla de nuevo).
                throw new InvalidOperationException("Esta empresa ya tiene a ese CUIT o a esa persona como titular (si la titularidad fue cancelada, hay que reponerla).", ex);
            }
        }

        private void ParametrosDatos(NpgsqlParameterCollection p, Entidades.TitularFiscal titular)
        {
            p.AddWithValue("idEmpresa", titular.IdEmpresa);
            p.AddWithValue("idPersona", (object)titular.IdPersona ?? DBNull.Value);
            p.AddWithValue("cuit", titular.Cuit);
            p.AddWithValue("razon", (object)titular.RazonSocial ?? DBNull.Value);
            p.AddWithValue("idIva", titular.IdIva);
            p.AddWithValue("condicion", (object)titular.CondicionIVA ?? DBNull.Value);
            p.AddWithValue("inicio", titular.InicioActividad.HasValue ? (object)titular.InicioActividad.Value.Date : DBNull.Value);
            p.AddWithValue("iibb", titular.Iibb);
            p.AddWithValue("limMes", (object)titular.LimiteMensual ?? DBNull.Value);
            p.AddWithValue("limAnio", (object)titular.LimiteAnual ?? DBNull.Value);
            p.AddWithValue("ventana", titular.VentanaAnual);
        }

        public void SetHabilitado(int idTitular, bool habilitado)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE titulares_fiscales SET habilitado = @h WHERE idtitular = @id AND NOT revocado;",
                p =>
                {
                    p.AddWithValue("id", idTitular);
                    p.AddWithValue("h", habilitado);
                });
        }

        // Solo cuenta lo de la PROPIA empresa: el CUIT de la empresa (que es su titular principal) y sus otros titulares,
        // incluso los de titularidad cancelada (el indice unico (idempresa, cuit) tambien los cuenta). Que el CUIT sea
        // titular en OTRAS empresas ya no es un error (titular compartido, 2026-10-12): ver OtrasEmpresasConCuit.
        public bool ExisteCuit(long cuit, int idTitularExcluir)
        {
            object enEmpresa = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT 1 FROM empresas WHERE cuit = @cuit AND idempresa = @idEmpresa LIMIT 1;",
                p =>
                {
                    p.AddWithValue("cuit", cuit);
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });
            if (enEmpresa != null && enEmpresa != DBNull.Value) return true;

            object enTitulares = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT 1 FROM titulares_fiscales WHERE cuit = @cuit AND idempresa = @idEmpresa AND idtitular <> @excluir LIMIT 1;",
                p =>
                {
                    p.AddWithValue("cuit", cuit);
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("excluir", idTitularExcluir);
                });
            return enTitulares != null && enTitulares != DBNull.Value;
        }

        // ------------------------------------------------------------------ titular compartido entre empresas

        // Consultas que cruzan empresas: corren con el rol de super-admin (SET LOCAL ROLE carnisys_sysadmin_bypass, igual
        // que SystemAdministrationPg), son de SOLO LECTURA y devuelven apenas ids y nombres de empresa, nunca datos de negocio.
        private List<T> LeerComoAdmin<T>(string sql, Func<NpgsqlDataReader, T> mapear, Action<NpgsqlParameterCollection> parametros)
        {
            using (var cn = new NpgsqlConnection(_connectionString))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new NpgsqlCommand("SET LOCAL ROLE carnisys_sysadmin_bypass;", cn, tx))
                            cmd.ExecuteNonQuery();

                        var lista = new List<T>();
                        using (var cmd = new NpgsqlCommand(sql, cn, tx))
                        {
                            parametros?.Invoke(cmd.Parameters);
                            using (var dr = cmd.ExecuteReader())
                                while (dr.Read()) lista.Add(mapear(dr));
                        }
                        tx.Commit();
                        return lista;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        private static Entidades.EmpresaConCuit MapearEmpresaConCuit(NpgsqlDataReader dr)
        {
            return new Entidades.EmpresaConCuit
            {
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                Nombre = Convert.ToString(dr["nombre"]),
                EsPrincipal = Convert.ToBoolean(dr["esprincipal"])
            };
        }

        public List<Entidades.EmpresaConCuit> OtrasEmpresasConCuit(long cuit, int idEmpresaPropia)
        {
            return LeerComoAdmin(@"
                SELECT e.idempresa, COALESCE(NULLIF(e.nombrefantasia, ''), e.razonsocialafip, '') AS nombre, t.espredeterminado AS esprincipal
                FROM titulares_fiscales t
                JOIN empresas e ON e.idempresa = t.idempresa
                WHERE t.cuit = @cuit AND t.idempresa <> @propia AND NOT t.revocado
                ORDER BY nombre;",
                MapearEmpresaConCuit,
                p =>
                {
                    p.AddWithValue("cuit", cuit);
                    p.AddWithValue("propia", idEmpresaPropia);
                });
        }

        // Administra el certificado del CUIT la empresa donde ese CUIT es el principal (el CUIT de la empresa) y, si en
        // ninguna lo es, la del titular mas antiguo: asi nunca hay dos empresas pidiendo o instalando a la vez.
        public int EmpresaAdministradoraDeCuit(long cuit)
        {
            var filas = LeerComoAdmin(@"
                SELECT idempresa FROM titulares_fiscales
                WHERE cuit = @cuit AND NOT revocado
                ORDER BY espredeterminado DESC, idtitular
                LIMIT 1;",
                dr => Convert.ToInt32(dr["idempresa"]),
                p => p.AddWithValue("cuit", cuit));
            return filas.Count == 0 ? 0 : filas[0];
        }

        public List<Entidades.EmpresaConCuit> OtrasEmpresasConPuntoVenta(long cuit, int codPuntoVenta, int idEmpresaPropia)
        {
            return LeerComoAdmin(@"
                SELECT DISTINCT e.idempresa, COALESCE(NULLIF(e.nombrefantasia, ''), e.razonsocialafip, '') AS nombre, x.esprincipal
                FROM (
                    -- titular principal: usa el punto de venta de sus sucursales
                    SELECT s.idempresa, true AS esprincipal
                    FROM sucursal s JOIN empresas em ON em.idempresa = s.idempresa
                    WHERE em.cuit = @cuit AND s.codpuntoventaafip = @pv
                    UNION
                    -- titular no principal: su lista de puntos de venta de ARCA
                    SELECT t.idempresa, false
                    FROM titulares_fiscales_ptovta p JOIN titulares_fiscales t ON t.idtitular = p.idtitular
                    WHERE t.cuit = @cuit AND p.codpuntoventaafip = @pv AND p.habilitado AND NOT t.revocado
                ) x
                JOIN empresas e ON e.idempresa = x.idempresa
                WHERE x.idempresa <> @propia
                ORDER BY nombre;",
                MapearEmpresaConCuit,
                p =>
                {
                    p.AddWithValue("cuit", cuit);
                    p.AddWithValue("pv", codPuntoVenta);
                    p.AddWithValue("propia", idEmpresaPropia);
                });
        }

        // ------------------------------------------------------------------ revocacion (super-admin)

        public bool TieneFacturas(int idTitular)
        {
            object hay = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT 1 FROM facturaelectronica WHERE idtitular = @id LIMIT 1;",
                p => p.AddWithValue("id", idTitular));
            return hay != null && hay != DBNull.Value;
        }

        // Una sola transaccion de la empresa: las sucursales que lo usaban vuelven al principal (NULL), se borran sus
        // asignaciones de punto de venta y, segun borrar, la fila (con sus puntos de venta) o solo se la marca revocada.
        // Nunca toca al principal.
        public void Revocar(int idTitular, bool borrar, int idUsuario)
        {
            using (var cn = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    Ejecutar(cn, tx, "UPDATE sucursal SET idtitularfacturacion = NULL WHERE idtitularfacturacion = @id AND idempresa = @e;", idTitular);
                    Ejecutar(cn, tx, "DELETE FROM titulares_fiscales_sucursal WHERE idtitular = @id AND idempresa = @e;", idTitular);
                    if (borrar)
                    {
                        Ejecutar(cn, tx, "DELETE FROM titulares_fiscales_ptovta WHERE idtitular = @id AND idempresa = @e;", idTitular);
                        Ejecutar(cn, tx, "DELETE FROM titulares_fiscales_clave WHERE idtitular = @id AND idempresa = @e;", idTitular);
                        Ejecutar(cn, tx, "DELETE FROM titulares_fiscales WHERE idtitular = @id AND idempresa = @e AND NOT espredeterminado;", idTitular);
                    }
                    else
                    {
                        using (var cmd = new NpgsqlCommand(@"
                            UPDATE titulares_fiscales SET revocado = true, revocado_en = now(), revocado_por = @u, habilitado = false
                            WHERE idtitular = @id AND idempresa = @e AND NOT espredeterminado;", cn, tx))
                        {
                            cmd.Parameters.AddWithValue("id", idTitular);
                            cmd.Parameters.AddWithValue("e", _idEmpresa);
                            cmd.Parameters.AddWithValue("u", idUsuario);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx?.Commit();
                }
                catch
                {
                    tx?.Rollback();
                    throw;
                }
            }
        }

        private void Ejecutar(NpgsqlConnection cn, NpgsqlTransaction tx, string sql, int idTitular)
        {
            using (var cmd = new NpgsqlCommand(sql, cn, tx))
            {
                cmd.Parameters.AddWithValue("id", idTitular);
                cmd.Parameters.AddWithValue("e", _idEmpresa);
                cmd.ExecuteNonQuery();
            }
        }

        // Repone la titularidad: queda deshabilitado, la empresa decide cuando habilitarlo.
        public void Reponer(int idTitular)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE titulares_fiscales SET revocado = false, revocado_en = NULL, revocado_por = NULL, habilitado = false
                WHERE idtitular = @id AND revocado;",
                p => p.AddWithValue("id", idTitular));
        }

        public List<string> SucursalesAsignadas(int idTitular)
        {
            return DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT s.sucursal FROM sucursal s
                WHERE s.idempresa = @idEmpresa
                  AND (s.idtitularfacturacion = @id
                       OR (s.idtitularfacturacion IS NULL
                           AND EXISTS (SELECT 1 FROM titulares_fiscales p WHERE p.idtitular = @id AND p.espredeterminado)))
                ORDER BY s.sucursal;",
                dr => Convert.ToString(dr["sucursal"]),
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("id", idTitular);
                });
        }

        // ------------------------------------------------------------------ sucursales

        public int? ObtenerIdTitularDeSucursal(int idSucursal)
        {
            object valor = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT idtitularfacturacion FROM sucursal WHERE idsucursal = @id;",
                p => p.AddWithValue("id", idSucursal));
            return (valor == null || valor == DBNull.Value) ? (int?)null : Convert.ToInt32(valor);
        }

        public void AsignarTitularSucursal(int idSucursal, int idTitular, int? codPuntoVenta)
        {
            using (var cn = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    using (var cmd = new NpgsqlCommand("UPDATE sucursal SET idtitularfacturacion = @t WHERE idsucursal = @s AND idempresa = @e;", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("t", idTitular);
                        cmd.Parameters.AddWithValue("s", idSucursal);
                        cmd.Parameters.AddWithValue("e", _idEmpresa);
                        cmd.ExecuteNonQuery();
                    }
                    if (codPuntoVenta.HasValue && codPuntoVenta.Value > 0)
                        UpsertPuntoVentaSucursal(cn, tx, idTitular, idSucursal, codPuntoVenta.Value);
                    tx?.Commit();
                }
                catch
                {
                    tx?.Rollback();
                    throw;
                }
            }
        }

        private void UpsertPuntoVentaSucursal(NpgsqlConnection cn, NpgsqlTransaction tx, int idTitular, int idSucursal, int cod)
        {
            using (var cmd = new NpgsqlCommand(@"
                INSERT INTO titulares_fiscales_sucursal (idempresa, idtitular, idsucursal, codpuntoventaafip)
                VALUES (@e, @t, @s, @c)
                ON CONFLICT (idtitular, idsucursal) DO UPDATE SET codpuntoventaafip = EXCLUDED.codpuntoventaafip;", cn, tx))
            {
                cmd.Parameters.AddWithValue("e", _idEmpresa);
                cmd.Parameters.AddWithValue("t", idTitular);
                cmd.Parameters.AddWithValue("s", idSucursal);
                cmd.Parameters.AddWithValue("c", cod);
                cmd.ExecuteNonQuery();
            }
        }

        public int? ObtenerPuntoVentaAsignado(int idTitular, int idSucursal)
        {
            object valor = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT codpuntoventaafip FROM titulares_fiscales_sucursal WHERE idtitular = @t AND idsucursal = @s;",
                p =>
                {
                    p.AddWithValue("t", idTitular);
                    p.AddWithValue("s", idSucursal);
                });
            return (valor == null || valor == DBNull.Value) ? (int?)null : Convert.ToInt32(valor);
        }

        public List<Entidades.TitularSucursalPuntoVenta> ListarPuntosVentaAsignados(int idTitular)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT idtitular, idsucursal, codpuntoventaafip FROM titulares_fiscales_sucursal WHERE idtitular = @t ORDER BY idsucursal;",
                dr => new Entidades.TitularSucursalPuntoVenta
                {
                    IdTitular = Convert.ToInt32(dr["idtitular"]),
                    IdSucursal = Convert.ToInt32(dr["idsucursal"]),
                    CodPuntoVentaAfip = Convert.ToInt32(dr["codpuntoventaafip"])
                },
                p => p.AddWithValue("t", idTitular));
        }

        public void GuardarPuntoVentaSucursal(int idTitular, int idSucursal, int? codPuntoVenta)
        {
            if (codPuntoVenta.HasValue && codPuntoVenta.Value > 0)
            {
                DbPg.NonQuery(_connectionString, _idEmpresa, @"
                    INSERT INTO titulares_fiscales_sucursal (idempresa, idtitular, idsucursal, codpuntoventaafip)
                    VALUES (@e, @t, @s, @c)
                    ON CONFLICT (idtitular, idsucursal) DO UPDATE SET codpuntoventaafip = EXCLUDED.codpuntoventaafip;",
                    p =>
                    {
                        p.AddWithValue("e", _idEmpresa);
                        p.AddWithValue("t", idTitular);
                        p.AddWithValue("s", idSucursal);
                        p.AddWithValue("c", codPuntoVenta.Value);
                    });
                return;
            }

            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM titulares_fiscales_sucursal WHERE idtitular = @t AND idsucursal = @s;",
                p =>
                {
                    p.AddWithValue("t", idTitular);
                    p.AddWithValue("s", idSucursal);
                });
        }

        // ------------------------------------------------------------------ puntos de venta

        private static Entidades.TitularPuntoVenta MapearPuntoVenta(System.Data.IDataRecord dr)
        {
            return new Entidades.TitularPuntoVenta
            {
                IdTitular = Convert.ToInt32(dr["idtitular"]),
                CodPuntoVentaAfip = Convert.ToInt32(dr["codpuntoventaafip"]),
                Descripcion = dr["descripcion"] == DBNull.Value ? "" : Convert.ToString(dr["descripcion"]),
                Habilitado = Convert.ToBoolean(dr["habilitado"]),
                Origen = Convert.ToString(dr["origen"])
            };
        }

        public List<Entidades.TitularPuntoVenta> ListarPuntosVenta(int idTitular)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT idtitular, codpuntoventaafip, descripcion, habilitado, origen FROM titulares_fiscales_ptovta WHERE idtitular = @t ORDER BY codpuntoventaafip;",
                MapearPuntoVenta,
                p => p.AddWithValue("t", idTitular));
        }

        public void GuardarPuntoVenta(int idEmpresa, Entidades.TitularPuntoVenta puntoVenta)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO titulares_fiscales_ptovta (idempresa, idtitular, codpuntoventaafip, descripcion, habilitado, origen)
                VALUES (@e, @t, @c, @d, @h, @o)
                ON CONFLICT (idtitular, codpuntoventaafip) DO UPDATE
                    SET descripcion = EXCLUDED.descripcion, habilitado = EXCLUDED.habilitado, origen = EXCLUDED.origen;",
                p =>
                {
                    p.AddWithValue("e", idEmpresa);
                    p.AddWithValue("t", puntoVenta.IdTitular);
                    p.AddWithValue("c", puntoVenta.CodPuntoVentaAfip);
                    p.AddWithValue("d", (object)puntoVenta.Descripcion ?? DBNull.Value);
                    p.AddWithValue("h", puntoVenta.Habilitado);
                    p.AddWithValue("o", puntoVenta.Origen ?? "MANUAL");
                });
        }

        public void EliminarPuntoVenta(int idTitular, int codPuntoVenta)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM titulares_fiscales_ptovta WHERE idtitular = @t AND codpuntoventaafip = @c;",
                p =>
                {
                    p.AddWithValue("t", idTitular);
                    p.AddWithValue("c", codPuntoVenta);
                });
        }

        // ------------------------------------------------------------------ facturado

        // Importe total de los comprobantes con CAE de produccion del titular entre las dos fechas (inclusive); las notas
        // de credito (3/8/13) restan. Los comprobantes anteriores a esta funcion (idtitular NULL) son del principal.
        public decimal Facturado(int idTitular, DateTime desde, DateTime hasta)
        {
            object total = DbPg.Scalar(_connectionString, _idEmpresa, @"
                SELECT COALESCE(SUM(CASE WHEN f.codtipocbteafip IN (3, 8, 13) THEN -f.importetotal ELSE f.importetotal END), 0)
                FROM facturaelectronica f
                WHERE (f.idtitular = @id
                       OR (f.idtitular IS NULL
                           AND EXISTS (SELECT 1 FROM titulares_fiscales p WHERE p.idtitular = @id AND p.espredeterminado AND p.idempresa = f.idempresa)))
                  AND COALESCE(f.cae, '') <> ''
                  AND COALESCE(f.esprueba, false) = false
                  AND COALESCE(f.error, false) = false
                  AND f.fechaemisionafip >= @desde AND f.fechaemisionafip < @hastaMasUno;",
                p =>
                {
                    p.AddWithValue("id", idTitular);
                    p.AddWithValue("desde", desde.Date);
                    p.AddWithValue("hastaMasUno", hasta.Date.AddDays(1));
                });
            return (total == null || total == DBNull.Value) ? 0m : Convert.ToDecimal(total);
        }

        // Lo que factura un CUIT en cada empresa donde es titular (la propia incluida), mismo criterio que Facturado(): produccion,
        // con CAE, sin error y netas de notas de credito. Los comprobantes anteriores a los titulares (idtitular NULL) son del
        // principal: cuentan para la empresa cuyo CUIT es este. Consulta cruzada con el rol de super-admin, solo lectura.
        public List<Entidades.FacturadoEmpresa> FacturadoCuitPorEmpresa(long cuit, DateTime desde, DateTime hasta)
        {
            return LeerComoAdmin(@"
                SELECT e.idempresa,
                       COALESCE(NULLIF(e.nombrefantasia, ''), e.razonsocialafip, '') AS nombre,
                       COALESCE((
                           SELECT SUM(CASE WHEN f.codtipocbteafip IN (3, 8, 13) THEN -f.importetotal ELSE f.importetotal END)
                           FROM facturaelectronica f
                           WHERE f.idempresa = e.idempresa
                             AND (f.idtitular IN (SELECT t.idtitular FROM titulares_fiscales t WHERE t.cuit = @cuit AND t.idempresa = e.idempresa)
                                  OR (f.idtitular IS NULL AND e.cuit = @cuit))
                             AND COALESCE(f.cae, '') <> ''
                             AND COALESCE(f.esprueba, false) = false
                             AND COALESCE(f.error, false) = false
                             AND f.fechaemisionafip >= @desde AND f.fechaemisionafip < @hastaMasUno
                       ), 0) AS importe
                FROM empresas e
                WHERE e.idempresa IN (SELECT t.idempresa FROM titulares_fiscales t WHERE t.cuit = @cuit)
                ORDER BY nombre;",
                dr => new Entidades.FacturadoEmpresa
                {
                    IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                    Nombre = Convert.ToString(dr["nombre"]),
                    Importe = Convert.ToDecimal(dr["importe"])
                },
                p =>
                {
                    p.AddWithValue("cuit", cuit);
                    p.AddWithValue("desde", desde.Date);
                    p.AddWithValue("hastaMasUno", hasta.Date.AddDays(1));
                });
        }

        // ------------------------------------------------------------------ certificado por CUIT

        private static string Entorno(bool homologacion) { return homologacion ? "HOMO" : "PROD"; }

        // Clave cifrada del .pfx de ese CUIT y entorno (tabla certificados_arca_cuit, sin RLS: la comparten todas las
        // empresas con ese CUIT como titular). null = sin clave guardada.
        public string ObtenerClaveCuit(long cuit, bool homologacion)
        {
            object valor = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT clave_protegida FROM certificados_arca_cuit WHERE cuit = @c AND entorno = @e;",
                p =>
                {
                    p.AddWithValue("c", cuit);
                    p.AddWithValue("e", Entorno(homologacion));
                });
            return (valor == null || valor == DBNull.Value) ? null : Convert.ToString(valor);
        }

        // El nombre del archivo solo se escribe al crear la fila: no pisa el nombre historico (certif-prod.pfx).
        public void GuardarClaveCuit(long cuit, bool homologacion, string nombreCertificado, string claveProtegida)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO certificados_arca_cuit (cuit, entorno, nombrecertificado_pfx, clave_protegida, actualizado)
                VALUES (@c, @e, @n, @k, now())
                ON CONFLICT (cuit, entorno) DO UPDATE SET clave_protegida = EXCLUDED.clave_protegida, actualizado = now();",
                p =>
                {
                    p.AddWithValue("c", cuit);
                    p.AddWithValue("e", Entorno(homologacion));
                    p.AddWithValue("n", nombreCertificado);
                    p.AddWithValue("k", (object)claveProtegida ?? DBNull.Value);
                });
        }
    }
}
