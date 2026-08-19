using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Entidades;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres del bloque Compras/CortePorCompra/MediaRes/CorteProveedor de
    // Contratos.ICompraRepository. backup/restaurarBD (BACKUP DATABASE/RESTORE DATABASE de SQL
    // Server) no estan cubiertos -- sin equivalente 1:1 en Postgres, ver docs/DECISIONS.md, Etapa 9.
    //
    // No-ops deliberados (SP real solo tocaba StockCorteSucursal, tabla que nunca se porta --
    // decision de la Etapa 6): quitarStockMedia completo; la parte de anularCompra que no sea el
    // UPDATE de estado; la parte de quitarStockTeoricoMedia que no sea el DELETE de MediaRes; la
    // parte de agregarMediaRes que no sea el INSERT.
    public class CompraPg : Contratos.ICompraRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public CompraPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        #region Compras

        public void anularCompra(int idCompra)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE compras SET estado = 'Anulado' WHERE idcompra = @idCompra;",
                p => p.AddWithValue("idCompra", idCompra));
        }

        // conexionSucursal (ruteo a otra conexion SQL Server para sucursales remotas San
        // Martin/San Lorenzo, ver Utilidades/Db.cs) no tiene equivalente en Postgres -- se ignora,
        // siempre consulta la base local. Mismo tratamiento que SucursalPg con esas sucursales.
        public DataTable obtenerCompras(int idSucursal, string tipoCompra, string texto, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal)
        {
            const string sql = @"
                SELECT idcompra, idpesajeajustado, nroremito, fechacompra, idproveedor, razonsocial, tipocompra, idsucursal, sucursal,
                       cantkg, totals, cantmedias, estado, observaciones, creado, creadopor, actualizado, actualizadopor
                FROM (
                    SELECT c.idcompra, c.idpesajeajustado, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra, c.idsucursal, s.sucursal,
                           SUM(m.kgmedia) AS cantkg, SUM(m.kgmedia * m.preciomedia) AS totals, c.cantmedias, c.estado,
                           c.observaciones AS observaciones, c.creado, cp.nombre AS creadopor, c.actualizado, ap.nombre AS actualizadopor
                    FROM compras c
                    INNER JOIN mediares m ON c.idcompra = m.idcompra
                    INNER JOIN personas p ON c.idproveedor = p.idpersona
                    INNER JOIN sucursal s ON c.idsucursal = s.idsucursal
                    LEFT JOIN usuarios ap ON c.actualizadopor = ap.id
                    LEFT JOIN usuarios cp ON c.creadopor = cp.id
                    WHERE (c.tipocompra ILIKE '%' || @tipoCompra || '%' OR @tipoCompra ILIKE 'Todos')
                      AND (@idSucursal = 0 OR c.idsucursal = @idSucursal)
                      AND c.fechacompra BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                      AND (p.razonsocial ILIKE '%' || @texto || '%' OR c.nroremito ILIKE '%' || @texto || '%')
                    GROUP BY c.idcompra, c.idpesajeajustado, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra,
                             c.idsucursal, s.sucursal, c.cantmedias, c.estado, c.observaciones, c.creado, cp.nombre, ap.nombre, c.actualizado

                    UNION

                    SELECT c.idcompra, c.idpesajeajustado, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra, c.idsucursal, s.sucursal,
                           SUM(cpc.cantkg) AS cantkg, SUM(cpc.preciokg * cpc.cantkg) AS totals, c.cantmedias, c.estado,
                           c.observaciones, c.creado, cp.nombre AS creadopor, c.actualizado, ap.nombre AS actualizadopor
                    FROM corteporcompra cpc
                    INNER JOIN compras c ON cpc.idcompra = c.idcompra
                    INNER JOIN personas p ON c.idproveedor = p.idpersona
                    INNER JOIN sucursal s ON c.idsucursal = s.idsucursal
                    LEFT JOIN usuarios ap ON c.actualizadopor = ap.id
                    LEFT JOIN usuarios cp ON c.creadopor = cp.id
                    WHERE c.tipocompra ILIKE 'Cortes'
                      AND (@tipoCompra ILIKE '' OR @tipoCompra ILIKE 'Cortes' OR @tipoCompra ILIKE 'Todos')
                      AND (@idSucursal = 0 OR c.idsucursal = @idSucursal)
                      AND c.fechacompra BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                      AND (p.razonsocial ILIKE '%' || @texto || '%' OR c.nroremito ILIKE '%' || @texto || '%')
                    GROUP BY c.idcompra, c.idpesajeajustado, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra,
                             c.idsucursal, s.sucursal, c.cantmedias, c.estado, c.observaciones, c.creado, cp.nombre, ap.nombre, c.actualizado

                    UNION

                    SELECT c.idcompra, c.idpesajeajustado, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra, c.idsucursal, s.sucursal,
                           SUM(cpc.cantkg) AS cantkg, SUM(cpc.preciokg * cpc.cantkg) AS totals, c.cantmedias, c.estado,
                           c.observaciones, c.creado, cp.nombre AS creadopor, c.actualizado, ap.nombre AS actualizadopor
                    FROM corteporcompra cpc
                    INNER JOIN compras c ON cpc.idcompra = c.idcompra
                    INNER JOIN sucursal s ON c.idsucursal = s.idsucursal
                    LEFT JOIN personas p ON c.idproveedor = p.idpersona
                    LEFT JOIN usuarios ap ON c.actualizadopor = ap.id
                    LEFT JOIN usuarios cp ON c.creadopor = cp.id
                    WHERE c.tipocompra ILIKE 'Ingreso Stock'
                      AND (@tipoCompra ILIKE '' OR @tipoCompra ILIKE 'Ingreso Stock' OR @tipoCompra ILIKE 'Ver Todos')
                      AND (@idSucursal = 0 OR c.idsucursal = @idSucursal)
                      AND c.fechacompra BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                    GROUP BY c.idcompra, c.idpesajeajustado, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra,
                             c.idsucursal, s.sucursal, c.cantmedias, c.estado, c.observaciones, c.creado, cp.nombre, ap.nombre, c.actualizado

                    UNION

                    SELECT c.idcompra, c.idpesajeajustado, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra, c.idsucursal, s.sucursal,
                           SUM(cpc.cantkg) AS cantkg, SUM(cpc.preciokg * cpc.cantkg) AS totals, c.cantmedias, c.estado,
                           c.observaciones, c.creado, cp.nombre AS creadopor, c.actualizado, ap.nombre AS actualizadopor
                    FROM corteporcompra cpc
                    INNER JOIN compras c ON cpc.idcompra = c.idcompra
                    INNER JOIN sucursal s ON c.idsucursal = s.idsucursal
                    LEFT JOIN personas p ON c.idproveedor = p.idpersona
                    LEFT JOIN usuarios ap ON c.actualizadopor = ap.id
                    LEFT JOIN usuarios cp ON c.creadopor = cp.id
                    WHERE c.tipocompra ILIKE 'Egreso Stock'
                      AND (@tipoCompra ILIKE '' OR @tipoCompra ILIKE 'Egreso Stock' OR @tipoCompra ILIKE 'Ver Todos')
                      AND (@idSucursal = 0 OR c.idsucursal = @idSucursal)
                      AND c.fechacompra BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                    GROUP BY c.idcompra, c.idpesajeajustado, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra,
                             c.idsucursal, s.sucursal, c.cantmedias, c.estado, c.observaciones, c.creado, cp.nombre, ap.nombre, c.actualizado

                    UNION

                    SELECT c.idcompra, c.idpesajeajustado, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra, c.idsucursal, s.sucursal,
                           SUM(cpc.cantkg) AS cantkg, SUM(cpc.preciokg * cpc.cantkg) AS totals, c.cantmedias, c.estado,
                           c.observaciones, c.creado, cp.nombre AS creadopor, c.actualizado, ap.nombre AS actualizadopor
                    FROM corteporcompra cpc
                    INNER JOIN compras c ON cpc.idcompra = c.idcompra
                    INNER JOIN sucursal s ON c.idsucursal = s.idsucursal
                    LEFT JOIN personas p ON c.idproveedor = p.idpersona
                    LEFT JOIN usuarios ap ON c.actualizadopor = ap.id
                    LEFT JOIN usuarios cp ON c.creadopor = cp.id
                    WHERE c.tipocompra ILIKE 'Cierre Stock'
                      AND (@tipoCompra ILIKE '' OR @tipoCompra ILIKE 'Cierre Stock' OR @tipoCompra ILIKE 'Ver Todos')
                      AND (@idSucursal = 0 OR c.idsucursal = @idSucursal)
                      AND c.fechacompra BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                    GROUP BY c.idcompra, c.idpesajeajustado, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra,
                             c.idsucursal, s.sucursal, c.cantmedias, c.estado, c.observaciones, c.creado, cp.nombre, ap.nombre, c.actualizado

                    UNION

                    SELECT c.idcompra, c.idpesajeajustado, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra, c.idsucursal, s.sucursal,
                           SUM(cpc.cantkg) AS cantkg, SUM(cpc.preciokg * cpc.cantkg) AS totals, c.cantmedias, c.estado,
                           (substring(p.razonsocial from 1 for 4) || ' | ' || (COALESCE(c.kgsmedias::text, '') || ' Kgs') || ' | ' ||
                            (COALESCE(c.cantmedias::text, '') || ' Medias') || chr(13) || chr(10) || COALESCE(c.observaciones, '')) AS observaciones,
                           c.creado, cp.nombre AS creadopor, c.actualizado, ap.nombre AS actualizadopor
                    FROM corteporcompra cpc
                    INNER JOIN compras c ON cpc.idcompra = c.idcompra
                    INNER JOIN sucursal s ON c.idsucursal = s.idsucursal
                    LEFT JOIN personas p ON c.idproveedor = p.idpersona
                    LEFT JOIN usuarios ap ON c.actualizadopor = ap.id
                    LEFT JOIN usuarios cp ON c.creadopor = cp.id
                    WHERE c.tipocompra ILIKE 'Pesaje Cortes'
                      AND (@tipoCompra ILIKE '' OR @tipoCompra ILIKE 'Pesaje Cortes' OR @tipoCompra ILIKE 'Ver Todos')
                      AND (@idSucursal = 0 OR c.idsucursal = @idSucursal)
                      AND c.fechacompra BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                    GROUP BY c.idcompra, c.idpesajeajustado, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra,
                             c.idsucursal, s.sucursal, c.cantmedias, c.estado, c.kgsmedias, c.observaciones, c.creado, cp.nombre, ap.nombre, c.actualizado

                    UNION

                    SELECT c.idcompra, c.idpesajeajustado, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra, c.idsucursal, s.sucursal,
                           SUM(cpc.cantkg) AS cantkg, SUM(cpc.preciokg * cpc.cantkg) AS totals, c.cantmedias, c.estado,
                           (CASE WHEN length(c.nroremito) > 0
                                 THEN 'ID Pesaje:' || c.nroremito || chr(13) || chr(10) || COALESCE(c.observaciones, '')
                                 ELSE c.observaciones END) AS observaciones,
                           c.creado, cp.nombre AS creadopor, c.actualizado, ap.nombre AS actualizadopor
                    FROM corteporcompra cpc
                    INNER JOIN compras c ON cpc.idcompra = c.idcompra
                    INNER JOIN sucursal s ON c.idsucursal = s.idsucursal
                    LEFT JOIN personas p ON c.idproveedor = p.idpersona
                    LEFT JOIN usuarios ap ON c.actualizadopor = ap.id
                    LEFT JOIN usuarios cp ON c.creadopor = cp.id
                    WHERE c.tipocompra ILIKE 'Ajuste Stock'
                      AND (@tipoCompra ILIKE '' OR @tipoCompra ILIKE 'Ajuste Stock' OR @tipoCompra ILIKE 'Ver Todos')
                      AND (@idSucursal = 0 OR c.idsucursal = @idSucursal)
                      AND c.fechacompra BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                    GROUP BY c.idcompra, c.idpesajeajustado, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra,
                             c.idsucursal, s.sucursal, c.cantmedias, c.estado, c.observaciones, c.creado, cp.nombre, ap.nombre, c.actualizado
                ) AS obtenercompras
                ORDER BY fechacompra DESC, idcompra DESC, creado DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("texto", texto ?? "");
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                p.AddWithValue("tipoCompra", tipoCompra ?? "");
                p.AddWithValue("idSucursal", idSucursal);
            });
        }

        public DataTable getLineasCompras(int idSucursal, string tipoCompra, string texto, string codigo, string corte, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal)
        {
            const string sql = @"
                (SELECT c.idcompra, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra, '-' AS codigo, 'Media Res' AS corte,
                        m.kgmedia AS cantkg, m.preciomedia AS preciokg, m.kgmedia * m.preciomedia AS totals, c.estado, c.idsucursal,
                        s.sucursal, c.observaciones, c.creado, cp.nombre AS creadopor, c.actualizado, ap.nombre AS actualizadopor
                 FROM compras c
                 INNER JOIN mediares m ON c.idcompra = m.idcompra
                 INNER JOIN personas p ON c.idproveedor = p.idpersona
                 INNER JOIN sucursal s ON c.idsucursal = s.idsucursal
                 LEFT JOIN usuarios ap ON c.actualizadopor = ap.id
                 LEFT JOIN usuarios cp ON c.creadopor = cp.id
                 WHERE (c.tipocompra ILIKE '%' || @tipoCompra || '%' OR @tipoCompra ILIKE 'Todos')
                   AND (@idSucursal = 0 OR c.idsucursal = @idSucursal)
                   AND c.fechacompra BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                   AND (p.razonsocial ILIKE '%' || @texto || '%' OR c.nroremito ILIKE '%' || @texto || '%')

                UNION

                SELECT c.idcompra, c.nroremito, c.fechacompra, c.idproveedor, p.razonsocial, c.tipocompra, co.codigo::text AS codigo, co.corte AS corte,
                       cpc.cantkg AS cantkg, cpc.preciokg AS preciokg, cpc.preciokg * cpc.cantkg AS totals, c.estado, c.idsucursal,
                       s.sucursal, c.observaciones, c.creado, cp.nombre AS creadopor, c.actualizado, ap.nombre AS actualizadopor
                 FROM corteporcompra cpc
                 INNER JOIN compras c ON cpc.idcompra = c.idcompra
                 INNER JOIN personas p ON c.idproveedor = p.idpersona
                 INNER JOIN sucursal s ON c.idsucursal = s.idsucursal
                 INNER JOIN corte co ON cpc.idcorte = co.idcorte
                 LEFT JOIN usuarios ap ON c.actualizadopor = ap.id
                 LEFT JOIN usuarios cp ON c.creadopor = cp.id
                 WHERE c.tipocompra ILIKE 'Cortes'
                   AND (@tipoCompra ILIKE '' OR @tipoCompra ILIKE 'Cortes' OR @tipoCompra ILIKE 'Todos')
                   AND (@idSucursal = 0 OR c.idsucursal = @idSucursal)
                   AND c.fechacompra BETWEEN @fechaDesde AND @fechaHasta + interval '1 day'
                   AND (p.razonsocial ILIKE '%' || @texto || '%' OR c.nroremito ILIKE '%' || @texto || '%')
                   AND co.codigo::text ILIKE '%' || @codigo || '%'
                   AND co.corte ILIKE '%' || @corte || '%')
                ORDER BY fechacompra DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("texto", texto ?? "");
                p.AddWithValue("codigo", codigo ?? "");
                p.AddWithValue("corte", corte ?? "");
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                p.AddWithValue("tipoCompra", tipoCompra ?? "");
                p.AddWithValue("idSucursal", idSucursal);
            });
        }

        public DataTable findById(int idCompra)
        {
            return DbPg.DataTable(_connectionString, _idEmpresa,
                "SELECT * FROM compras WHERE idcompra = @idCompra;",
                p => p.AddWithValue("idCompra", idCompra));
        }

        public int addOrEditCompra(Compra oCompraE)
        {
            if (oCompraE == null) throw new ArgumentNullException(nameof(oCompraE));

            if (oCompraE.IdCompra == 0)
            {
                const string sqlInsert = @"
                    INSERT INTO compras (nroremito, fechacompra, idproveedor, estado, observaciones, tipocompra, cantmedias, kgsmedias,
                        idpesajeajustado, enctacte, idsucursal, creado, creadopor, idempresa)
                    VALUES (@nroRemito, @fechaCompra, @idProveedor, @estado, @observaciones, @tipoCompra, @cantMedias, @kgsMedias,
                        @idPesajeAjustado, @enCtaCte, @idSucursal, now(), @creadoPor, @idEmpresa)
                    RETURNING idcompra;";

                object nuevoId = DbPg.Scalar(_connectionString, _idEmpresa, sqlInsert, p =>
                {
                    p.AddWithValue("nroRemito", oCompraE.NroRemito ?? "");
                    p.AddWithValue("fechaCompra", oCompraE.FechaCompra);
                    p.AddWithValue("idProveedor", oCompraE.Proveedor.idPersona);
                    p.AddWithValue("estado", oCompraE.Estado ?? "");
                    p.AddWithValue("observaciones", oCompraE.Observaciones ?? "");
                    p.AddWithValue("tipoCompra", oCompraE.TipoCompra ?? "");
                    p.AddWithValue("cantMedias", (object)oCompraE.CantMedias ?? DBNull.Value);
                    p.AddWithValue("kgsMedias", (object)oCompraE.KgsMedias ?? DBNull.Value);
                    p.AddWithValue("idPesajeAjustado", (object)oCompraE.IdPesajeAjustado ?? DBNull.Value);
                    p.AddWithValue("enCtaCte", oCompraE.EnCtaCte);
                    p.AddWithValue("idSucursal", oCompraE.Sucursal.idSucursal);
                    p.AddWithValue("creadoPor", oCompraE.CreadoPor.Id);
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });

                oCompraE.IdCompra = Convert.ToInt32(nuevoId);
                return oCompraE.IdCompra;
            }

            const string sqlUpdate = @"
                UPDATE compras SET nroremito=@nroRemito, fechacompra=@fechaCompra, idproveedor=@idProveedor, estado=@estado,
                    observaciones=@observaciones, tipocompra=@tipoCompra, cantmedias=@cantMedias, kgsmedias=@kgsMedias,
                    idpesajeajustado=@idPesajeAjustado, enctacte=@enCtaCte, idsucursal=@idSucursal, actualizado=now(), actualizadopor=@actualizadoPor
                WHERE idcompra=@idCompra;";

            DbPg.NonQuery(_connectionString, _idEmpresa, sqlUpdate, p =>
            {
                p.AddWithValue("idCompra", oCompraE.IdCompra);
                p.AddWithValue("nroRemito", oCompraE.NroRemito ?? "");
                p.AddWithValue("fechaCompra", oCompraE.FechaCompra);
                p.AddWithValue("idProveedor", oCompraE.Proveedor.idPersona);
                p.AddWithValue("estado", oCompraE.Estado ?? "");
                p.AddWithValue("observaciones", oCompraE.Observaciones ?? "");
                p.AddWithValue("tipoCompra", oCompraE.TipoCompra ?? "");
                p.AddWithValue("cantMedias", (object)oCompraE.CantMedias ?? DBNull.Value);
                p.AddWithValue("kgsMedias", (object)oCompraE.KgsMedias ?? DBNull.Value);
                p.AddWithValue("idPesajeAjustado", (object)oCompraE.IdPesajeAjustado ?? DBNull.Value);
                p.AddWithValue("enCtaCte", oCompraE.EnCtaCte);
                p.AddWithValue("idSucursal", oCompraE.Sucursal.idSucursal);
                p.AddWithValue("actualizadoPor", oCompraE.ActualizadoPor != null ? oCompraE.ActualizadoPor.Id : 0);
            });

            // addOrEditCompra (edicion) real: limpia CortePorCompra/MediaRes para que el caller
            // vuelva a cargar las lineas -- mismo patron que addOrEditFormula de Corte.cs.
            DbPg.NonQuery(_connectionString, _idEmpresa, "DELETE FROM corteporcompra WHERE idcompra = @idCompra;",
                p => p.AddWithValue("idCompra", oCompraE.IdCompra));
            DbPg.NonQuery(_connectionString, _idEmpresa, "DELETE FROM mediares WHERE idcompra = @idCompra;",
                p => p.AddWithValue("idCompra", oCompraE.IdCompra));

            return oCompraE.IdCompra;
        }

        public int agregarCompra(Compra oCompraE)
        {
            if (oCompraE == null) throw new ArgumentNullException(nameof(oCompraE));

            const string sqlInsert = @"
                INSERT INTO compras (nroremito, fechacompra, idproveedor, estado, observaciones, tipocompra, cantmedias, kgsmedias,
                    idpesajeajustado, enctacte, idsucursal, creado, creadopor, idempresa)
                VALUES (@nroRemito, @fechaCompra, @idProveedor, @estado, @observaciones, @tipoCompra, @cantMedias, @kgsMedias,
                    @idPesajeAjustado, @enCtaCte, @idSucursal, now(), @creadoPor, @idEmpresa)
                RETURNING idcompra;";

            object nuevoId = DbPg.Scalar(_connectionString, _idEmpresa, sqlInsert, p =>
            {
                p.AddWithValue("nroRemito", oCompraE.NroRemito ?? "");
                p.AddWithValue("fechaCompra", oCompraE.FechaCompra);
                p.AddWithValue("idProveedor", oCompraE.Proveedor.idPersona);
                p.AddWithValue("estado", oCompraE.Estado ?? "");
                p.AddWithValue("observaciones", oCompraE.Observaciones ?? "");
                p.AddWithValue("tipoCompra", oCompraE.TipoCompra ?? "");
                p.AddWithValue("cantMedias", (object)oCompraE.CantMedias ?? DBNull.Value);
                p.AddWithValue("kgsMedias", (object)oCompraE.KgsMedias ?? DBNull.Value);
                p.AddWithValue("idPesajeAjustado", (object)oCompraE.IdPesajeAjustado ?? DBNull.Value);
                p.AddWithValue("enCtaCte", oCompraE.EnCtaCte);
                p.AddWithValue("idSucursal", oCompraE.Sucursal.idSucursal);
                p.AddWithValue("creadoPor", oCompraE.CreadoPor.Id);
                p.AddWithValue("idEmpresa", _idEmpresa);
            });

            return nuevoId == null || nuevoId == DBNull.Value ? 0 : Convert.ToInt32(nuevoId);
        }

        public void ModificarCompra(Compra oCompraE)
        {
            if (oCompraE == null) throw new ArgumentNullException(nameof(oCompraE));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE compras SET nroremito=@nroRemito, fechacompra=@fechaCompra, idproveedor=@idProveedor, estado=@estado,
                    observaciones=@observaciones, tipocompra=@tipoCompra, cantmedias=@cantMedias, kgsmedias=@kgsMedias,
                    idpesajeajustado=@idPesajeAjustado, enctacte=@enCtaCte, idsucursal=@idSucursal, actualizado=now(), actualizadopor=@actualizadoPor
                WHERE idcompra=@idCompra;", p =>
            {
                p.AddWithValue("idCompra", oCompraE.IdCompra);
                p.AddWithValue("nroRemito", oCompraE.NroRemito ?? "");
                p.AddWithValue("fechaCompra", oCompraE.FechaCompra);
                p.AddWithValue("idProveedor", oCompraE.Proveedor.idPersona);
                p.AddWithValue("estado", oCompraE.Estado ?? "");
                p.AddWithValue("observaciones", oCompraE.Observaciones ?? "");
                p.AddWithValue("tipoCompra", oCompraE.TipoCompra ?? "");
                p.AddWithValue("cantMedias", (object)oCompraE.CantMedias ?? DBNull.Value);
                p.AddWithValue("kgsMedias", (object)oCompraE.KgsMedias ?? DBNull.Value);
                p.AddWithValue("idPesajeAjustado", (object)oCompraE.IdPesajeAjustado ?? DBNull.Value);
                p.AddWithValue("enCtaCte", oCompraE.EnCtaCte);
                p.AddWithValue("idSucursal", oCompraE.Sucursal.idSucursal);
                p.AddWithValue("actualizadoPor", oCompraE.ActualizadoPor.Id);
            });
        }

        public void actualizarObservacionesCompra(int idCompra, string observaciones, int actualizadoPor)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE compras SET observaciones = @observaciones, actualizado = now(), actualizadopor = @actualizadoPor WHERE idcompra = @idCompra;",
                p =>
                {
                    p.AddWithValue("idCompra", idCompra);
                    p.AddWithValue("observaciones", observaciones ?? "");
                    p.AddWithValue("actualizadoPor", actualizadoPor);
                });
        }

        public List<int> obtenerPesajesVinculadosPorDestino(int idPesajeDestino)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT idcompra FROM compras WHERE tipocompra = @tipoCompra AND idpesajeajustado = @idPesajeDestino AND idcompra <> @idPesajeDestino;",
                dr => Convert.ToInt32(dr["idcompra"]),
                p =>
                {
                    p.AddWithValue("tipoCompra", Compra.tipoCompraToString(Compra.tipoCompraEnum.PesajeCortes));
                    p.AddWithValue("idPesajeDestino", idPesajeDestino);
                });
        }

        // Version batch: Postgres soporta arrays nativos (= ANY(@ids)), asi que a diferencia del
        // original no hace falta trocear en lotes de 900 (esa limitacion era del maximo de
        // parametros de SQL Server, no aplica aca).
        public Dictionary<int, List<int>> obtenerPesajesVinculadosPorDestinos(IEnumerable<int> idsDestino)
        {
            var resultado = new Dictionary<int, List<int>>();
            if (idsDestino == null) return resultado;

            var ids = idsDestino.Where(x => x > 0).Distinct().ToArray();
            if (ids.Length == 0) return resultado;

            string tipoPesaje = Compra.tipoCompraToString(Compra.tipoCompraEnum.PesajeCortes);

            var filas = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT idcompra, idpesajeajustado FROM compras WHERE tipocompra = @tipo AND idpesajeajustado IS NOT NULL AND idpesajeajustado = ANY(@ids) AND idcompra <> idpesajeajustado;",
                dr => new { IdDestino = Convert.ToInt32(dr["idpesajeajustado"]), IdHijo = Convert.ToInt32(dr["idcompra"]) },
                p =>
                {
                    p.AddWithValue("tipo", tipoPesaje);
                    p.AddWithValue("ids", ids);
                });

            foreach (var fila in filas)
            {
                if (!resultado.TryGetValue(fila.IdDestino, out var lista))
                {
                    lista = new List<int>();
                    resultado[fila.IdDestino] = lista;
                }
                lista.Add(fila.IdHijo);
            }

            return resultado;
        }

        public void actualizarIdPesajeAjustado(int idCompra, int? idPesajeAjustado, int actualizadoPor)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE compras SET idpesajeajustado = @idPesajeAjustado, actualizado = now(), actualizadopor = @actualizadoPor WHERE idcompra = @idCompra;",
                p =>
                {
                    p.AddWithValue("idCompra", idCompra);
                    p.AddWithValue("idPesajeAjustado", (object)idPesajeAjustado ?? DBNull.Value);
                    p.AddWithValue("actualizadoPor", actualizadoPor);
                });
        }

        // Nunca invocado hoy (unico call-site en Negocio/Compra.cs esta comentado). Se replica el
        // switch tal cual el original, sin agregar un default -- si algun dia se llama con un
        // tipoCompra no contemplado, debe fallar igual que el original (consulta vacia).
        public float getTotalCompra(int idCompra, string tipoCompra)
        {
            string consulta = "";

            switch (Compra.tipoCompraToEnum(tipoCompra))
            {
                case Compra.tipoCompraEnum.Cortes:
                    consulta = "SELECT SUM(cantkg * preciokg) AS total FROM corteporcompra WHERE idcompra = @idCompra GROUP BY idcompra;";
                    break;
                case Compra.tipoCompraEnum.MediaRes:
                    consulta = "SELECT SUM(kgmedia * preciomedia) AS total FROM mediares WHERE idcompra = @idCompra GROUP BY idcompra;";
                    break;
            }

            object result = DbPg.Scalar(_connectionString, _idEmpresa, consulta, p => p.AddWithValue("idCompra", idCompra));
            double totalCompraD = (result == null || result == DBNull.Value) ? 0 : Convert.ToDouble(result);
            return (float)totalCompraD;
        }

        public void modificarPrecioMedia(int idCompra, float precioKg)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE mediares SET preciomedia = @precioKg WHERE idcompra = @idCompra;",
                p =>
                {
                    p.AddWithValue("idCompra", idCompra);
                    p.AddWithValue("precioKg", precioKg);
                });
        }

        #endregion

        #region CortePorCompra / CorteProveedor

        // agregarCortePorCompra real (verificado con sp_helptext) hace 2 cosas: el INSERT en
        // CortePorCompra, y -- hallazgo nuevo de esta etapa -- si la compra es tipo 'Cortes',
        // un upsert condicional en CorteProveedor (ultimo precio/fecha por proveedor+corte).
        // Ambas van en una sola transaccion.
        public void agregarCortePorCompra(CortePorCompra oCorteE)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    using (var cmdIns = new NpgsqlCommand(@"
                        INSERT INTO corteporcompra (idcompra, idcorte, idsucursal, preciokg, cantkg, balanza, creado, creadopor, idempresa)
                        VALUES (@idCompra, @idCorte, @idSucursal, @precioKg, @cantKg, @balanza, @creado, @creadoPor, @idEmpresa);", con, tx))
                    {
                        cmdIns.Parameters.AddWithValue("idCompra", oCorteE.Compra.IdCompra);
                        cmdIns.Parameters.AddWithValue("idCorte", oCorteE.Corte.idCorte);
                        cmdIns.Parameters.AddWithValue("idSucursal", oCorteE.Sucursal.IdSucursal);
                        cmdIns.Parameters.AddWithValue("precioKg", oCorteE.precioKg);
                        cmdIns.Parameters.AddWithValue("cantKg", oCorteE.cantKgs);
                        cmdIns.Parameters.AddWithValue("balanza", oCorteE.Balanza);
                        cmdIns.Parameters.AddWithValue("creado", (object)oCorteE.Creado ?? DBNull.Value);
                        cmdIns.Parameters.AddWithValue("creadoPor", oCorteE.CreadoPor != null ? oCorteE.CreadoPor.Id : 0);
                        cmdIns.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                        cmdIns.ExecuteNonQuery();
                    }

                    int idProveedor = 0;
                    DateTime fechaCompra = default;
                    bool esTipoCortes = false;

                    using (var cmdCompra = new NpgsqlCommand(
                        "SELECT idproveedor, fechacompra FROM compras WHERE idcompra = @idCompra AND tipocompra = 'Cortes';", con, tx))
                    {
                        cmdCompra.Parameters.AddWithValue("idCompra", oCorteE.Compra.IdCompra);
                        using (var dr = cmdCompra.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                esTipoCortes = true;
                                idProveedor = Convert.ToInt32(dr["idproveedor"]);
                                fechaCompra = Convert.ToDateTime(dr["fechacompra"]);
                            }
                        }
                    }

                    if (esTipoCortes)
                    {
                        bool existeCorteProveedor;
                        using (var cmdExiste = new NpgsqlCommand(
                            "SELECT 1 FROM corteproveedor WHERE idproveedor = @idProveedor AND idcorte = @idCorte;", con, tx))
                        {
                            cmdExiste.Parameters.AddWithValue("idProveedor", idProveedor);
                            cmdExiste.Parameters.AddWithValue("idCorte", oCorteE.Corte.idCorte);
                            existeCorteProveedor = cmdExiste.ExecuteScalar() != null;
                        }

                        if (existeCorteProveedor)
                        {
                            using (var cmdUpd = new NpgsqlCommand(@"
                                UPDATE corteproveedor SET ultimoprecio = @precioKg, fechaultimacompra = @fechaCompra
                                WHERE idproveedor = @idProveedor AND idcorte = @idCorte AND fechaultimacompra < @fechaCompra;", con, tx))
                            {
                                cmdUpd.Parameters.AddWithValue("precioKg", oCorteE.precioKg);
                                cmdUpd.Parameters.AddWithValue("fechaCompra", fechaCompra);
                                cmdUpd.Parameters.AddWithValue("idProveedor", idProveedor);
                                cmdUpd.Parameters.AddWithValue("idCorte", oCorteE.Corte.idCorte);
                                cmdUpd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            using (var cmdInsCp = new NpgsqlCommand(@"
                                INSERT INTO corteproveedor (idproveedor, idcorte, ultimoprecio, fechaultimacompra, idempresa)
                                VALUES (@idProveedor, @idCorte, @precioKg, @fechaCompra, @idEmpresa);", con, tx))
                            {
                                cmdInsCp.Parameters.AddWithValue("idProveedor", idProveedor);
                                cmdInsCp.Parameters.AddWithValue("idCorte", oCorteE.Corte.idCorte);
                                cmdInsCp.Parameters.AddWithValue("precioKg", oCorteE.precioKg);
                                cmdInsCp.Parameters.AddWithValue("fechaCompra", fechaCompra);
                                cmdInsCp.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                                cmdInsCp.ExecuteNonQuery();
                            }
                        }
                    }

                    tx.Commit();
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
                    throw;
                }
            }
        }

        public void limpiarCortesPorCompra(int idCompra)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM corteporcompra WHERE idcompra = @idCompra;",
                p => p.AddWithValue("idCompra", idCompra));
        }

        public DataTable obtenerCortesPorCompra(int idCompra)
        {
            const string sql = @"
                SELECT cpc.idcorteporcompra, cpc.idcorte, co.codigo, co.corte, cpc.cantkg, cpc.preciokg,
                       cpc.cantkg * cpc.preciokg AS totals, cpc.balanza, cpc.idsucursal, s.sucursal, cpc.creado, cpc.creadopor
                FROM corteporcompra cpc
                INNER JOIN corte co ON cpc.idcorte = co.idcorte
                INNER JOIN sucursal s ON cpc.idsucursal = s.idsucursal
                WHERE cpc.idcompra = @idCompra
                ORDER BY co.codigo;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("idCompra", idCompra));
        }

        public void modificarCortePorCompra(CortePorCompra oCorteE, int idCompra)
        {
            // El SP 'modificarCortePorCompra' NO existe en la base SQL Server real (confirmado
            // con sp_helptext: "The object 'dbo.modificarCortePorCompra' does not exist").
            // Datos.Compra.modificarCortePorCompra lo invoca igual, asi que hoy mismo, en SQL
            // Server, llamar a este metodo tira excepcion -- y no tiene ningun caller real en
            // Web/ (verificado por grep). No se inventa un UPDATE plausible (regla CLAUDE.md
            // §2.7): se preserva la misma clase de falla que produce el original.
            throw new NotSupportedException(
                "modificarCortePorCompra: el SP real no existe en SQL Server (confirmado con sp_helptext) y no tiene callers en Web/. " +
                "Ver docs/DECISIONS.md, Etapa 9.");
        }

        public void quitarStockCorte(CortePorCompra oCorteE, int idCompra)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM corteporcompra WHERE idcompra = @idCompra AND idcorte = @idCorte AND idsucursal = @idSucursal;",
                p =>
                {
                    p.AddWithValue("idCompra", idCompra);
                    p.AddWithValue("idCorte", oCorteE.corte.idCorte);
                    p.AddWithValue("idSucursal", oCorteE.sucursal.IdSucursal);
                });
        }

        #endregion

        #region MediaRes

        public void agregarMediaRes(MediaRes oMediaResE)
        {
            if (oMediaResE == null) throw new ArgumentNullException(nameof(oMediaResE));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO mediares (idcompra, nrotropa, idsucursal, preciomedia, kgmedia, idempresa)
                VALUES (@idCompra, @nroTropa, @idSucursal, @precioMedia, @kgMedia, @idEmpresa);", p =>
            {
                p.AddWithValue("idCompra", oMediaResE.compra.IdCompra);
                p.AddWithValue("nroTropa", oMediaResE.nroTropa ?? "");
                p.AddWithValue("idSucursal", oMediaResE.sucursal.IdSucursal);
                p.AddWithValue("precioMedia", oMediaResE.precioMedia);
                p.AddWithValue("kgMedia", oMediaResE.kgMedia);
                p.AddWithValue("idEmpresa", _idEmpresa);
            });
        }

        public DataTable obtenerMediasPorCompra(int idCompra)
        {
            const string sql = @"
                SELECT m.idmedia, m.nrotropa, m.kgmedia, m.preciomedia, m.kgmedia * m.preciomedia AS totals, m.idsucursal, s.sucursal
                FROM compras c
                INNER JOIN mediares m ON c.idcompra = m.idcompra
                INNER JOIN sucursal s ON m.idsucursal = s.idsucursal
                WHERE m.idcompra = @idCompra;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("idCompra", idCompra));
        }

        // Pese al nombre, el SP real (comentado en el propio codigo original) nunca hizo un
        // UPDATE -- siempre INSERT. Se preserva el comportamiento real, no el nombre.
        public void modificarMediaPorCompra(MediaRes oMediaResE, int idCompra)
        {
            if (oMediaResE == null) throw new ArgumentNullException(nameof(oMediaResE));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO mediares (nrotropa, idcompra, idsucursal, kgmedia, preciomedia, idempresa)
                VALUES (@nroTropa, @idCompra, @idSucursal, @kgMedia, @precioMedia, @idEmpresa);", p =>
            {
                p.AddWithValue("nroTropa", oMediaResE.nroTropa ?? "");
                p.AddWithValue("idCompra", idCompra);
                p.AddWithValue("idSucursal", oMediaResE.sucursal.IdSucursal);
                p.AddWithValue("kgMedia", oMediaResE.kgMedia);
                p.AddWithValue("precioMedia", oMediaResE.precioMedia);
                p.AddWithValue("idEmpresa", _idEmpresa);
            });
        }

        public void quitarStockMedia(MediaRes oMediaResE, int idCompra)
        {
            // No-op deliberado: el SP real (3 UPDATE StockCorteSucursal) no toca ninguna otra
            // tabla. StockCorteSucursal nunca se porta a Postgres (Etapa 6).
        }

        public void quitarStockTeoricoMedia(MediaRes oMediaResE, int idCompra)
        {
            if (oMediaResE == null) throw new ArgumentNullException(nameof(oMediaResE));

            // Solo se replica la parte real del SP (DELETE FROM MediaRes); el resto son 3 UPDATE
            // StockCorteSucursal, no-op (Etapa 6).
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM mediares WHERE idmedia = @idMedia;",
                p => p.AddWithValue("idMedia", oMediaResE.idMedia));
        }

        #endregion

        #region Reportes (porcentaje / promedio de medias)

        public DataTable porcentajeCortesPorCompra(int idCompra)
        {
            const string sql = @"
                SELECT corte AS ""Corte"", sucursal AS ""Sucursal"", stockteorico AS ""Cantidad Kgs"",
                       stockmin AS ""Stock Min"", stockmax AS ""Stock Max""
                FROM (
                    (SELECT cp.idcorte AS idcorte, cp.corte AS corte, s.idsucursal AS idsucursal, s.sucursal AS sucursal,
                            SUM(m.kgmedia * cp.porcentaje / 100) AS stockteorico,
                            SUM(m.kgmedia * (cp.porcentaje - cp.desvioestandar) / 100) AS stockmin,
                            SUM(m.kgmedia * (cp.porcentaje + cp.desvioestandar) / 100) AS stockmax
                     FROM corte cmr
                     INNER JOIN corte cp ON cmr.idcorte = cp.idcortemaestro AND cmr.idcorte <> cp.idcorte
                     CROSS JOIN mediares m
                     INNER JOIN sucursal s ON m.idsucursal = s.idsucursal
                     WHERE cmr.codigo = 0 AND m.idcompra = @idCompra
                     GROUP BY cp.corte, cp.idcorte, s.sucursal, s.idsucursal)
                    UNION
                    (SELECT cp.idcorte AS idcorte, cp.corte AS corte, s.idsucursal AS idsucursal, s.sucursal AS sucursal,
                            SUM(m.kgmedia * cp.porcentaje / 100 * cm.porcentaje / 100) AS stockteorico,
                            SUM(m.kgmedia * cm.porcentaje / 100 * (cp.porcentaje - cp.desvioestandar) / 100) AS stockmin,
                            SUM(m.kgmedia * cm.porcentaje / 100 * (cp.porcentaje + cp.desvioestandar) / 100) AS stockmax
                     FROM corte cm
                     INNER JOIN corte cp ON cm.idcorte = cp.idcortemaestro
                     INNER JOIN corte cmr ON cm.idcortemaestro = cmr.idcorte AND cm.idcorte <> cmr.idcorte
                     CROSS JOIN mediares m
                     INNER JOIN sucursal s ON m.idsucursal = s.idsucursal
                     WHERE cmr.codigo = 0 AND cp.independiente = 1 AND m.idcompra = @idCompra
                     GROUP BY cp.corte, cp.idcorte, s.idsucursal, s.sucursal)
                ) AS porcentajesporcorte;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("idCompra", idCompra));
        }

        public DataTable getPromMedias(int idCompra)
        {
            const string sql = @"
                SELECT c.cantmedias AS ""CantMedias"", c.kgsmedias AS ""Kgs"", (c.kgsmedias::double precision / c.cantmedias) AS ""PromMedias"",
                       p.razonsocial AS ""Proveedor"", to_char(c.fechacompra, 'DD/MM/YYYY') AS ""Fecha""
                FROM corte co
                INNER JOIN corteporcompra cpc ON co.idcorte = cpc.idcorte
                INNER JOIN compras c ON cpc.idcompra = c.idcompra
                INNER JOIN personas p ON c.idproveedor = p.idpersona
                WHERE cpc.idcompra = @idCompra
                LIMIT 1;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("idCompra", idCompra));
        }

        public DataTable getPorcCortesEnMedias(int idCompra)
        {
            const string sql = @"
                SELECT idcorte AS ""idCorte"", codigo AS ""Codigo"", corte AS ""Corte"", cantkg AS ""CantKg"", prompormedia AS ""PromPorMedia"",
                       porcreal AS ""PorcReal"", porcteo AS ""PorcTeo"", dif AS ""Dif."", espacio AS ""-"", preciokg AS ""PrecioKg"", gan AS ""Gan.""
                FROM (
                    (SELECT co.idcorte AS idcorte, co.codigo AS codigo, co.corte AS corte,
                            SUM(cpc.cantkg) AS cantkg,
                            (SUM(cpc.cantkg) / c.cantmedias) AS prompormedia,
                            (SUM(cpc.cantkg) / c.kgsmedias) AS porcreal,
                            (co.porcentaje / 100) AS porcteo,
                            (SUM(cpc.cantkg) - (c.kgsmedias * (co.porcentaje / 100))) AS dif,
                            ''::text AS espacio,
                            co.preciokg AS preciokg,
                            (SUM(cpc.cantkg) - (c.kgsmedias * (co.porcentaje / 100))) * co.preciokg AS gan
                     FROM corte co
                     INNER JOIN corteporcompra cpc ON co.idcorte = cpc.idcorte
                     INNER JOIN compras c ON cpc.idcompra = c.idcompra
                     WHERE cpc.idcompra = @idCompra
                     GROUP BY c.idcompra, c.cantmedias, c.kgsmedias, co.idcorte, co.codigo, co.corte, co.porcentaje, co.preciokg)
                    UNION
                    (SELECT NULL::integer AS idcorte, 99999::bigint AS codigo, ''::text AS corte,
                            NULL::double precision AS cantkg, NULL::double precision AS prompormedia, NULL::double precision AS porcreal,
                            NULL::double precision AS porcteo, NULL::double precision AS dif, NULL::text AS espacio,
                            NULL::double precision AS preciokg, 0::double precision AS gan
                     FROM corte co
                     INNER JOIN corteporcompra cpc ON co.idcorte = cpc.idcorte
                     INNER JOIN compras c ON cpc.idcompra = c.idcompra
                     WHERE cpc.idcompra = @idCompra)
                ) AS tablaunion(idcorte, codigo, corte, cantkg, prompormedia, porcreal, porcteo, dif, espacio, preciokg, gan)
                ORDER BY codigo;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("idCompra", idCompra));
        }

        #endregion

        #region Pesajes / Ajustes

        public int obtenerIdUltimaCompra()
        {
            object obj = DbPg.Scalar(_connectionString, _idEmpresa, "SELECT COALESCE(MAX(idcompra), 0) FROM compras;");
            int maxId = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
            return maxId + 1;
        }

        public int getIdAjusteDelPesaje(int idPesaje)
        {
            string tipoAj = Compra.tipoCompraToString(Compra.tipoCompraEnum.AjusteStock);

            const string sql = @"
                SELECT idcompra
                FROM compras
                WHERE tipocompra = @tipo AND (idpesajeajustado = @idPesaje OR nroremito = @nroRemito)
                ORDER BY CASE WHEN idpesajeajustado = @idPesaje THEN 0 ELSE 1 END, idcompra DESC
                LIMIT 1;";

            object obj = DbPg.Scalar(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("tipo", tipoAj);
                p.AddWithValue("idPesaje", idPesaje);
                p.AddWithValue("nroRemito", idPesaje.ToString());
            });

            return (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
        }

        // Version batch, sin trocear en lotes de 900 (arrays nativos de Postgres, ver comentario
        // de obtenerPesajesVinculadosPorDestinos).
        public Dictionary<int, int> getIdsAjustePorPesajes(IEnumerable<int> idsPesaje)
        {
            var resultado = new Dictionary<int, int>();
            if (idsPesaje == null) return resultado;

            var ids = idsPesaje.Where(x => x > 0).Distinct().ToArray();
            if (ids.Length == 0) return resultado;

            string tipoAj = Compra.tipoCompraToString(Compra.tipoCompraEnum.AjusteStock);
            var nros = ids.Select(x => x.ToString()).ToArray();

            const string sql = @"
                SELECT idcompra, idpesajeajustado, nroremito
                FROM compras
                WHERE tipocompra = @tipo
                  AND ((idpesajeajustado IS NOT NULL AND idpesajeajustado = ANY(@ids)) OR nroremito = ANY(@nros))
                ORDER BY idcompra DESC;";

            var filas = DbPg.Reader(_connectionString, _idEmpresa, sql,
                dr => new
                {
                    IdCompra = Convert.ToInt32(dr["idcompra"]),
                    IdPesajeAjustado = dr["idpesajeajustado"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["idpesajeajustado"]),
                    NroRemito = dr["nroremito"] == DBNull.Value ? null : Convert.ToString(dr["nroremito"])
                },
                p =>
                {
                    p.AddWithValue("tipo", tipoAj);
                    p.AddWithValue("ids", ids);
                    p.AddWithValue("nros", nros);
                });

            foreach (int idPesaje in ids)
            {
                var fila = filas
                    .Where(f => (f.IdPesajeAjustado.HasValue && f.IdPesajeAjustado.Value == idPesaje)
                                || (f.NroRemito != null && string.Equals(f.NroRemito, idPesaje.ToString(), StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(f => f.IdPesajeAjustado.HasValue && f.IdPesajeAjustado.Value == idPesaje ? 0 : 1)
                    .ThenByDescending(f => f.IdCompra)
                    .FirstOrDefault();

                if (fila != null)
                    resultado[idPesaje] = fila.IdCompra;
            }

            return resultado;
        }

        public void actualizarEstadoPesaje(int idPesaje, Compra.estadoAjusteStock estadoAjStock)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE compras SET estado = @estado WHERE idcompra = @id;",
                p =>
                {
                    p.AddWithValue("estado", Compra.estadoAjStockToString(estadoAjStock));
                    p.AddWithValue("id", idPesaje);
                });
        }

        #endregion
    }
}
