using System;
using System.Collections.Generic;
using System.Data;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Cubre los cambios en cajas YA CERRADAS (2026-10-06, ver docs/DECISIONS.md): la regla pura que decide si hace falta
    // confirmar (CambioCajaCerrada) y el servicio que ubica las cajas afectadas y deja el aviso (CajaCerradaServicio).
    public class CajaCerradaTests
    {
        private static readonly DateTime Fecha = new DateTime(2026, 10, 5, 10, 30, 15);

        // ---------- CambioCajaCerrada.HuboCambioQueAfectaCierre ----------

        [Fact]
        public void SinCambios_NoAfectaElCierre()
        {
            Assert.False(Negocio.CambioCajaCerrada.HuboCambioQueAfectaCierre(100, 100, "Efectivo", "Efectivo", Fecha, Fecha, 1, 1));
        }

        [Fact]
        public void MontoDistinto_Afecta()
        {
            Assert.True(Negocio.CambioCajaCerrada.HuboCambioQueAfectaCierre(100, 150, "Efectivo", "Efectivo", Fecha, Fecha, 1, 1));
        }

        [Fact]
        public void DiferenciaDeMontoMenorALaTolerancia_NoAfecta()
        {
            // float -> double deja ruido de centesimas de centavo: no cuenta como cambio de monto.
            Assert.False(Negocio.CambioCajaCerrada.HuboCambioQueAfectaCierre(100.001, 100.0, "Efectivo", "Efectivo", Fecha, Fecha, 1, 1));
        }

        [Fact]
        public void FormaDePagoDistinta_Afecta_SinImportarMayusculasNiEspacios()
        {
            Assert.True(Negocio.CambioCajaCerrada.HuboCambioQueAfectaCierre(100, 100, "Efectivo", "Debito", Fecha, Fecha, 1, 1));
            Assert.False(Negocio.CambioCajaCerrada.HuboCambioQueAfectaCierre(100, 100, "Efectivo", " efectivo ", Fecha, Fecha, 1, 1));
        }

        [Fact]
        public void FormaDePagoNulaYVacia_SonLoMismo()
        {
            Assert.False(Negocio.CambioCajaCerrada.HuboCambioQueAfectaCierre(100, 100, null, "", Fecha, Fecha, 1, 1));
        }

        [Fact]
        public void SucursalDistinta_Afecta()
        {
            Assert.True(Negocio.CambioCajaCerrada.HuboCambioQueAfectaCierre(100, 100, "Efectivo", "Efectivo", Fecha, Fecha, 1, 2));
        }

        [Fact]
        public void FechaDistintaEnMinutos_Afecta()
        {
            Assert.True(Negocio.CambioCajaCerrada.HuboCambioQueAfectaCierre(100, 100, "Efectivo", "Efectivo", Fecha, Fecha.AddMinutes(5), 1, 1));
        }

        [Fact]
        public void FechaConDistintosSegundos_NoCuentaComoCambio()
        {
            // El input datetime-local no manda segundos y la base si: guardar sin tocar la fecha no es un cambio.
            Assert.False(Negocio.CambioCajaCerrada.CambioFecha(Fecha, new DateTime(2026, 10, 5, 10, 30, 0)));
        }

        [Fact]
        public void CambioFecha_ConUnaSolaFechaNula_EsCambio()
        {
            Assert.True(Negocio.CambioCajaCerrada.CambioFecha(null, Fecha));
            Assert.False(Negocio.CambioCajaCerrada.CambioFecha(null, null));
        }

        // ---------- CajaCerradaServicio ----------

        // Fila de cierrecaja "cerrada" con las columnas que consume Negocio.CierreCaja.convertDatatableToList.
        private static DataTable FilaCierre(int id, int idUsuario, int idSucursal)
        {
            var dt = new DataTable();
            foreach (var col in new[]
            {
                "id", "idSucursal", "usuarioInicio", "usuarioCierre", "fechaHoraInicio", "fechaHoraCierre", "cajaInicio",
                "ventas", "gastos", "cajaCierre", "diferencia", "cajaInicioSiguiente", "importeRetirado", "vendedor", "Cerrada_Por"
            })
                dt.Columns.Add(col, typeof(object));

            dt.Rows.Add(id, idSucursal, idUsuario.ToString(), 1, new DateTime(2026, 10, 5, 8, 0, 0), new DateTime(2026, 10, 5, 14, 0, 0),
                1000f, 5000f, 200f, 5800f, 0f, 5800f, 0f, "Cajero Test", "Encargado Test");
            return dt;
        }

        private sealed class SucursalRepoFake : Contratos.ISucursalRepository
        {
            public DataTable obtenerSucursales() => new DataTable();
            public Sucursal findById(int id) => new Sucursal { IdSucursal = id, SucursalNombre = "Sucursal " + id };
            public List<Sucursal> findAll() => new List<Sucursal>();
            public Empresa findEmpresaById(int idEmpresa) => null;
            public Empresa findEmpresaByCuit(long cuit) => null;
            public void ActualizarDatosBasicos(Sucursal oSucursalE) { }
            public DataTable obtenerSucursalSanMartin() => new DataTable();
            public DataTable obtenerSucursalSanLorenzo() => new DataTable();
            public DataTable obtenerConexiones(bool? mostrarEnPrincipal, bool? mostrarEnStockActual) => new DataTable();
            public int getIdSucursalByConexion(string nameConnString) => 0;
        }

        private static (Negocio.CajaCerradaServicio servicio, FakeCierreCajaRepository repo) CrearServicio(bool soporta = true)
        {
            var repo = new FakeCierreCajaRepository { SoportaAuditoriaCierre = soporta };
            var cierreN = new Negocio.CierreCaja(repo, new EmpresaContextFake(1), null, null, new SucursalRepoFake());
            return (new Negocio.CajaCerradaServicio(cierreN), repo);
        }

        [Fact]
        public void Evaluar_SinSoporte_DevuelveVacio()
        {
            var (servicio, repo) = CrearServicio(soporta: false);
            repo.CierreCerradoQueContiene = FilaCierre(7, 5, 1);

            Assert.False(servicio.Soporta);
            Assert.Empty(servicio.Evaluar(new Negocio.CajaCerradaConsulta(5, 1, Fecha)));
        }

        [Fact]
        public void Evaluar_SinCajaCerradaQueContenga_DevuelveVacio()
        {
            var (servicio, repo) = CrearServicio();
            repo.CierreCerradoQueContiene = new DataTable();

            Assert.Empty(servicio.Evaluar(new Negocio.CajaCerradaConsulta(5, 1, Fecha)));
        }

        [Fact]
        public void Evaluar_ConCajaCerrada_DevuelveLaCajaConSusDatos()
        {
            var (servicio, repo) = CrearServicio();
            repo.CierreCerradoQueContiene = FilaCierre(7, 5, 1);

            var cajas = servicio.Evaluar(new Negocio.CajaCerradaConsulta(5, 1, Fecha));

            var caja = Assert.Single(cajas);
            Assert.Equal(7, caja.Id);
            Assert.Equal(5, caja.UsuarioInicio.Id);
            Assert.Equal("Cajero Test", caja.UsuarioInicio.Nombre);
            Assert.Equal("Encargado Test", caja.UsuarioCierre.Nombre);
        }

        [Fact]
        public void Evaluar_FechaOriginalYNuevaEnLaMismaCaja_NoDuplicaLaCaja()
        {
            // Una venta que se edita sin salir de su caja consulta dos veces la misma caja: se informa una sola vez.
            var (servicio, repo) = CrearServicio();
            repo.CierreCerradoQueContiene = FilaCierre(7, 5, 1);

            var cajas = servicio.Evaluar(
                new Negocio.CajaCerradaConsulta(5, 1, Fecha),
                new Negocio.CajaCerradaConsulta(5, 1, Fecha.AddHours(1)));

            Assert.Single(cajas);
        }

        [Fact]
        public void Registrar_DejaUnaAuditoriaPorCajaConLosDatosDelCambio()
        {
            var (servicio, repo) = CrearServicio();
            repo.CierreCerradoQueContiene = FilaCierre(7, 5, 1);
            var cajas = servicio.Evaluar(new Negocio.CajaCerradaConsulta(5, 1, Fecha));

            servicio.Registrar(AuditoriaCierreCaja.OrigenVenta, 99, AuditoriaCierreCaja.AccionModificacion, cajas,
                idUsuario: 3, usuario: "Admin", importeAnterior: 100, importeNuevo: 150,
                formaPagoAnterior: "Efectivo", formaPagoNueva: "Debito", detalle: "Venta modificada");

            var auditoria = Assert.Single(repo.AuditoriasRegistradas);
            Assert.Equal(7, auditoria.IdCierreCaja);
            Assert.Equal(AuditoriaCierreCaja.TipoModificacion, auditoria.Tipo);
            Assert.Equal("VENTA", auditoria.Origen);
            Assert.Equal(99, auditoria.IdOrigen);
            Assert.Equal("MODIFICACION", auditoria.Accion);
            Assert.Equal(3, auditoria.IdUsuario);
            Assert.Equal(100, auditoria.ImporteAnterior);
            Assert.Equal(150, auditoria.ImporteNuevo);
            Assert.Equal("Efectivo", auditoria.FormaPagoAnterior);
            Assert.Equal("Debito", auditoria.FormaPagoNueva);
        }

        [Fact]
        public void Registrar_SinSoporte_NoHaceNada()
        {
            var (servicio, repo) = CrearServicio(soporta: false);
            var caja = new Entidades.CierreCaja { Id = 7 };

            servicio.Registrar(AuditoriaCierreCaja.OrigenPago, 1, AuditoriaCierreCaja.AccionEliminacion,
                new List<Entidades.CierreCaja> { caja }, 3, "Admin", null, null, null, null, "");

            Assert.Empty(repo.AuditoriasRegistradas);
        }
    }
}
