using System;
using System.Collections.Generic;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.ILiquidacionSueldoRepository.
    public class LiquidacionSueldoPg : Contratos.ILiquidacionSueldoRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public LiquidacionSueldoPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        // Misma mecanica que CuentaCorrientePg.IniciarUnitOfWork: Negocio.LiquidacionSueldo.Confirmar
        // necesita que Agregar (liquidacionsueldo + detalle) y el crearMovCtaCte de
        // Negocio.CuentaCorriente queden en una unica transaccion.
        public Contratos.IUnitOfWork IniciarUnitOfWork()
        {
            return UnitOfWorkPg.Iniciar(_connectionString, _idEmpresa);
        }

        private const string SelectCabecera = @"
                SELECT idliquidacionsueldo, idempresa, idempleado, periododesde, periodohasta,
                       fechaliquidacion, totalliquidado, detallectacte, estado, creado, creadopor
                FROM liquidacionsueldo ";

        private static Entidades.LiquidacionSueldo MapearCabecera(System.Data.IDataRecord dr)
        {
            return new Entidades.LiquidacionSueldo
            {
                Id = Convert.ToInt32(dr["idliquidacionsueldo"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                IdEmpleado = Convert.ToInt32(dr["idempleado"]),
                PeriodoDesde = Convert.ToDateTime(dr["periododesde"]),
                PeriodoHasta = Convert.ToDateTime(dr["periodohasta"]),
                FechaLiquidacion = Convert.ToDateTime(dr["fechaliquidacion"]),
                TotalLiquidado = Convert.ToDecimal(dr["totalliquidado"]),
                DetalleCtaCte = dr["detallectacte"] == DBNull.Value ? "" : Convert.ToString(dr["detallectacte"]),
                Estado = (Entidades.LiquidacionSueldo.estadoLiquidacion)Enum.Parse(typeof(Entidades.LiquidacionSueldo.estadoLiquidacion), Convert.ToString(dr["estado"])),
                Creado = dr["creado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["creado"]),
                CreadoPor = dr["creadopor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["creadopor"])
            };
        }

        private static Entidades.LiquidacionSueldoDetalle MapearDetalle(System.Data.IDataRecord dr)
        {
            return new Entidades.LiquidacionSueldoDetalle
            {
                Id = Convert.ToInt32(dr["idliquidacionsueldodetalle"]),
                IdLiquidacionSueldo = Convert.ToInt32(dr["idliquidacionsueldo"]),
                Concepto = Convert.ToString(dr["concepto"]),
                Origen = (Entidades.LiquidacionSueldoDetalle.origenDetalle)Enum.Parse(typeof(Entidades.LiquidacionSueldoDetalle.origenDetalle), Convert.ToString(dr["origen"])),
                Cantidad = Convert.ToDecimal(dr["cantidad"]),
                ValorUnitario = Convert.ToDecimal(dr["valorunitario"]),
                Subtotal = Convert.ToDecimal(dr["subtotal"]),
                SinTarifaConfigurada = Convert.ToBoolean(dr["sintarifaconfigurada"])
            };
        }

        private List<Entidades.LiquidacionSueldoDetalle> ObtenerDetalle(int idLiquidacion)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                @"SELECT idliquidacionsueldodetalle, idliquidacionsueldo, concepto, origen, cantidad, valorunitario, subtotal, sintarifaconfigurada
                  FROM liquidacionsueldodetalle WHERE idliquidacionsueldo = @id ORDER BY idliquidacionsueldodetalle;",
                MapearDetalle,
                p => p.AddWithValue("id", idLiquidacion));
        }

        public List<Entidades.LiquidacionSueldo> ListarPorEmpleado(int idEmpleado)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                SelectCabecera + "WHERE idempleado = @idEmpleado ORDER BY periododesde DESC;",
                MapearCabecera,
                p => p.AddWithValue("idEmpleado", idEmpleado));

            foreach (var liquidacion in lista)
                liquidacion.Detalle = ObtenerDetalle(liquidacion.Id);

            return lista;
        }

        public Entidades.LiquidacionSueldo ObtenerPorId(int id, int idEmpresa)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                SelectCabecera + "WHERE idliquidacionsueldo = @id AND idempresa = @idEmpresa;",
                MapearCabecera,
                p => { p.AddWithValue("id", id); p.AddWithValue("idEmpresa", idEmpresa); });

            if (lista.Count == 0) return null;
            lista[0].Detalle = ObtenerDetalle(lista[0].Id);
            return lista[0];
        }

        public Entidades.LiquidacionSueldo ObtenerSolapada(int idEmpleado, DateTime desde, DateTime hasta, int idExcluir)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                SelectCabecera + @"WHERE idempleado = @idEmpleado AND estado = 'Confirmada' AND idliquidacionsueldo <> @idExcluir
                                   AND periododesde < @hasta AND periodohasta > @desde
                                   ORDER BY periododesde LIMIT 1;",
                MapearCabecera,
                p =>
                {
                    p.AddWithValue("idEmpleado", idEmpleado);
                    p.AddWithValue("idExcluir", idExcluir);
                    p.AddWithValue("desde", desde);
                    p.AddWithValue("hasta", hasta);
                });
            return lista.Count > 0 ? lista[0] : null;
        }

        public Entidades.LiquidacionSueldo ObtenerUltimaConfirmada(int idEmpleado)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                SelectCabecera + "WHERE idempleado = @idEmpleado AND estado = 'Confirmada' ORDER BY periodohasta DESC LIMIT 1;",
                MapearCabecera,
                p => p.AddWithValue("idEmpleado", idEmpleado));
            return lista.Count > 0 ? lista[0] : null;
        }

        public bool EstaCubiertaPorLiquidacionConfirmada(int idEmpleado, DateTime fecha)
        {
            object resultado = DbPg.Scalar(_connectionString, _idEmpresa,
                @"SELECT 1 FROM liquidacionsueldo
                  WHERE idempleado = @idEmpleado AND estado = 'Confirmada' AND @fecha::date BETWEEN periododesde::date AND periodohasta::date
                  LIMIT 1;",
                p => { p.AddWithValue("idEmpleado", idEmpleado); p.AddWithValue("fecha", fecha.Date); });
            return resultado != null;
        }

        public int Agregar(Entidades.LiquidacionSueldo liquidacion, Contratos.IUnitOfWork unitOfWork)
        {
            const string sqlCabecera = @"
                INSERT INTO liquidacionsueldo
                    (idempresa, idempleado, periododesde, periodohasta, fechaliquidacion, totalliquidado, detallectacte, estado, creado, creadopor)
                VALUES
                    (@idEmpresa, @idEmpleado, @periodoDesde, @periodoHasta, @fechaLiquidacion, @total, @detalleCtaCte, @estado, now(), @creadoPor)
                RETURNING idliquidacionsueldo;";

            object nuevoId = DbPg.Scalar(_connectionString, _idEmpresa, sqlCabecera, p =>
            {
                p.AddWithValue("idEmpresa", liquidacion.IdEmpresa);
                p.AddWithValue("idEmpleado", liquidacion.IdEmpleado);
                p.AddWithValue("periodoDesde", liquidacion.PeriodoDesde);
                p.AddWithValue("periodoHasta", liquidacion.PeriodoHasta);
                p.AddWithValue("fechaLiquidacion", liquidacion.FechaLiquidacion);
                p.AddWithValue("total", liquidacion.TotalLiquidado);
                p.AddWithValue("detalleCtaCte", liquidacion.DetalleCtaCte ?? "");
                p.AddWithValue("estado", liquidacion.Estado.ToString());
                p.AddWithValue("creadoPor", (object)liquidacion.CreadoPor ?? DBNull.Value);
            }, unitOfWork);

            liquidacion.Id = Convert.ToInt32(nuevoId);

            const string sqlDetalle = @"
                INSERT INTO liquidacionsueldodetalle
                    (idempresa, idliquidacionsueldo, concepto, origen, cantidad, valorunitario, subtotal, sintarifaconfigurada)
                VALUES
                    (@idEmpresa, @idLiquidacion, @concepto, @origen, @cantidad, @valorUnitario, @subtotal, @sinTarifa);";

            foreach (var linea in liquidacion.Detalle ?? new List<Entidades.LiquidacionSueldoDetalle>())
            {
                DbPg.NonQuery(_connectionString, _idEmpresa, sqlDetalle, p =>
                {
                    p.AddWithValue("idEmpresa", liquidacion.IdEmpresa);
                    p.AddWithValue("idLiquidacion", liquidacion.Id);
                    p.AddWithValue("concepto", linea.Concepto ?? "");
                    p.AddWithValue("origen", linea.Origen.ToString());
                    p.AddWithValue("cantidad", linea.Cantidad);
                    p.AddWithValue("valorUnitario", linea.ValorUnitario);
                    p.AddWithValue("subtotal", linea.Subtotal);
                    p.AddWithValue("sinTarifa", linea.SinTarifaConfigurada);
                }, unitOfWork);
            }

            return liquidacion.Id;
        }

        public void Anular(int idLiquidacion, int idEmpresa, Contratos.IUnitOfWork unitOfWork)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE liquidacionsueldo SET estado = 'Anulada' WHERE idliquidacionsueldo = @id AND idempresa = @idEmpresa;",
                p => { p.AddWithValue("id", idLiquidacion); p.AddWithValue("idEmpresa", idEmpresa); },
                unitOfWork);
        }
    }
}
