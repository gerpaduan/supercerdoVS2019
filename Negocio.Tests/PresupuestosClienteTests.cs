using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Negocio;
using Xunit;

namespace NegocioTests
{
    // Cubre Negocio.PresupuestosCliente: armado desde las filas del repositorio, estado por
    // producto (Futuro / Reemplazado / Caducado / Aplicable) y solapa inicial del historial.
    public class PresupuestosClienteTests
    {
        private static readonly DateTime Ahora = new DateTime(2026, 10, 4, 10, 0, 0);

        private const int Lomo = 1;
        private const int Asado = 2;

        private static PresupuestoCliente Presupuesto(int id, DateTime vigencia, DateTime? caducidad, params int[] productos)
        {
            var presupuesto = new PresupuestoCliente { IdExpendio = id, Vigencia = vigencia, FechaCaducidad = caducidad };
            foreach (int idCorte in productos)
                presupuesto.Lineas.Add(new LineaPresupuestoCliente { IdCorte = idCorte, Codigo = idCorte.ToString(), Producto = "P" + idCorte, PrecioKg = 100 });
            return presupuesto;
        }

        private static EstadoLineaPresupuestoCliente EstadoDe(PresupuestoCliente p, int idCorte)
        {
            return p.Lineas.Single(l => l.IdCorte == idCorte).Estado;
        }

        // --- Estado del presupuesto (cabecera) ---

        [Fact]
        public void Evaluar_presupuesto_ya_empezado_y_dentro_de_la_caducidad_es_vigente()
        {
            var p = Presupuesto(1, Ahora.AddDays(-10), Ahora.Date.AddDays(20), Lomo);

            PresupuestosCliente.Evaluar(new[] { p }, Ahora);

            Assert.Equal(EstadoPresupuestoCliente.Vigente, p.Estado);
            Assert.Equal(EstadoLineaPresupuestoCliente.Aplicable, EstadoDe(p, Lomo));
        }

        [Fact]
        public void Evaluar_presupuesto_con_vigencia_futura_es_futuro_y_sus_lineas_no_aplican()
        {
            var p = Presupuesto(1, Ahora.AddDays(5), null, Lomo);

            PresupuestosCliente.Evaluar(new[] { p }, Ahora);

            Assert.Equal(EstadoPresupuestoCliente.Futuro, p.Estado);
            Assert.Equal(EstadoLineaPresupuestoCliente.Futuro, EstadoDe(p, Lomo));
        }

        [Fact]
        public void Evaluar_el_ultimo_dia_de_caducidad_todavia_es_vigente()
        {
            // Caduca el mismo dia de "ahora": valido durante todo el dia, aunque la hora ya pasó.
            var p = Presupuesto(1, Ahora.AddDays(-30), Ahora.Date, Lomo);

            PresupuestosCliente.Evaluar(new[] { p }, Ahora);

            Assert.Equal(EstadoPresupuestoCliente.Vigente, p.Estado);
            Assert.Equal(EstadoLineaPresupuestoCliente.Aplicable, EstadoDe(p, Lomo));
        }

        [Fact]
        public void Evaluar_el_dia_siguiente_a_la_caducidad_es_caducado()
        {
            var p = Presupuesto(1, Ahora.AddDays(-30), Ahora.Date.AddDays(-1), Lomo);

            PresupuestosCliente.Evaluar(new[] { p }, Ahora);

            Assert.Equal(EstadoPresupuestoCliente.Caducado, p.Estado);
            Assert.Equal(EstadoLineaPresupuestoCliente.Caducado, EstadoDe(p, Lomo));
        }

        [Fact]
        public void Evaluar_sin_fecha_de_caducidad_asume_vigencia_mas_90_dias()
        {
            var vigencia = Ahora.AddDays(-89);
            var dentro = Presupuesto(1, vigencia, null, Lomo);
            var fuera = Presupuesto(2, Ahora.AddDays(-91), null, Asado);

            PresupuestosCliente.Evaluar(new[] { dentro, fuera }, Ahora);

            Assert.Equal(vigencia.Date.AddDays(SectorPuntoExpendio.DiasCaducidadPresupuestoPorDefecto), dentro.CaducaEl);
            Assert.Equal(EstadoPresupuestoCliente.Vigente, dentro.Estado);
            Assert.Equal(EstadoPresupuestoCliente.Caducado, fuera.Estado);
        }

        // --- Quien manda cuando un producto esta en varios presupuestos ---

        [Fact]
        public void Evaluar_el_presupuesto_mas_reciente_manda_y_el_anterior_queda_reemplazado()
        {
            var viejo = Presupuesto(1, Ahora.AddDays(-20), Ahora.Date.AddDays(60), Lomo);
            var nuevo = Presupuesto(2, Ahora.AddDays(-5), Ahora.Date.AddDays(60), Lomo);

            PresupuestosCliente.Evaluar(new[] { nuevo, viejo }, Ahora);

            Assert.Equal(EstadoLineaPresupuestoCliente.Aplicable, EstadoDe(nuevo, Lomo));
            Assert.Equal(EstadoLineaPresupuestoCliente.Reemplazado, EstadoDe(viejo, Lomo));
            Assert.Equal(2, viejo.Lineas.Single().ReemplazadoPor);
        }

        [Fact]
        public void Evaluar_el_reemplazo_se_decide_por_producto_no_por_presupuesto()
        {
            // El nuevo solo trae Lomo: el Asado del viejo sigue siendo el ultimo precio del Asado.
            var viejo = Presupuesto(1, Ahora.AddDays(-20), Ahora.Date.AddDays(60), Lomo, Asado);
            var nuevo = Presupuesto(2, Ahora.AddDays(-5), Ahora.Date.AddDays(60), Lomo);

            PresupuestosCliente.Evaluar(new[] { nuevo, viejo }, Ahora);

            Assert.Equal(EstadoLineaPresupuestoCliente.Reemplazado, EstadoDe(viejo, Lomo));
            Assert.Equal(EstadoLineaPresupuestoCliente.Aplicable, EstadoDe(viejo, Asado));
        }

        [Fact]
        public void Evaluar_un_presupuesto_futuro_no_reemplaza_al_vigente_hasta_que_empieza()
        {
            var vigente = Presupuesto(1, Ahora.AddDays(-20), Ahora.Date.AddDays(60), Lomo);
            var futuro = Presupuesto(2, Ahora.AddDays(3), Ahora.Date.AddDays(90), Lomo);

            PresupuestosCliente.Evaluar(new[] { futuro, vigente }, Ahora);

            Assert.Equal(EstadoLineaPresupuestoCliente.Aplicable, EstadoDe(vigente, Lomo));
            Assert.Equal(EstadoLineaPresupuestoCliente.Futuro, EstadoDe(futuro, Lomo));

            // Pasados 3 dias el futuro "entra" y el anterior queda reemplazado.
            PresupuestosCliente.Evaluar(new[] { futuro, vigente }, Ahora.AddDays(4));

            Assert.Equal(EstadoLineaPresupuestoCliente.Reemplazado, EstadoDe(vigente, Lomo));
            Assert.Equal(EstadoLineaPresupuestoCliente.Aplicable, EstadoDe(futuro, Lomo));
        }

        [Fact]
        public void Evaluar_si_el_ultimo_presupuesto_caduco_no_resucita_al_anterior()
        {
            // El nuevo caduco ayer; el viejo todavia estaria dentro de su plazo, pero el nuevo
            // fue la ultima palabra: ningun precio se puede aplicar.
            var viejo = Presupuesto(1, Ahora.AddDays(-60), Ahora.Date.AddDays(30), Lomo);
            var nuevo = Presupuesto(2, Ahora.AddDays(-20), Ahora.Date.AddDays(-1), Lomo);

            PresupuestosCliente.Evaluar(new[] { nuevo, viejo }, Ahora);

            Assert.Equal(EstadoLineaPresupuestoCliente.Caducado, EstadoDe(nuevo, Lomo));
            Assert.Equal(EstadoLineaPresupuestoCliente.Reemplazado, EstadoDe(viejo, Lomo));
        }

        [Fact]
        public void Evaluar_con_la_misma_vigencia_gana_el_de_mayor_numero()
        {
            var a = Presupuesto(7, Ahora.AddDays(-3), Ahora.Date.AddDays(60), Lomo);
            var b = Presupuesto(9, Ahora.AddDays(-3), Ahora.Date.AddDays(60), Lomo);

            PresupuestosCliente.Evaluar(new[] { a, b }, Ahora);

            Assert.Equal(EstadoLineaPresupuestoCliente.Reemplazado, EstadoDe(a, Lomo));
            Assert.Equal(9, a.Lineas.Single().ReemplazadoPor);
            Assert.Equal(EstadoLineaPresupuestoCliente.Aplicable, EstadoDe(b, Lomo));
        }

        // --- Solapa inicial ---

        [Fact]
        public void AbrirEnPresupuestos_con_uno_vigente_es_true()
        {
            var lista = new[] { Presupuesto(1, Ahora.AddDays(-400), Ahora.Date.AddDays(10), Lomo) };
            PresupuestosCliente.Evaluar(lista, Ahora);

            Assert.True(PresupuestosCliente.AbrirEnPresupuestos(lista, Ahora));
        }

        [Fact]
        public void AbrirEnPresupuestos_con_uno_caducado_hace_menos_de_6_meses_es_true()
        {
            var lista = new[] { Presupuesto(1, Ahora.AddMonths(-5), Ahora.Date.AddMonths(-3), Lomo) };
            PresupuestosCliente.Evaluar(lista, Ahora);

            Assert.True(PresupuestosCliente.AbrirEnPresupuestos(lista, Ahora));
        }

        [Fact]
        public void AbrirEnPresupuestos_el_borde_exacto_de_6_meses_cuenta()
        {
            var lista = new[] { Presupuesto(1, Ahora.AddMonths(-6), Ahora.Date.AddMonths(-5), Lomo) };
            PresupuestosCliente.Evaluar(lista, Ahora);

            Assert.True(PresupuestosCliente.AbrirEnPresupuestos(lista, Ahora));
        }

        [Fact]
        public void AbrirEnPresupuestos_con_uno_caducado_hace_mas_de_6_meses_es_false()
        {
            var lista = new[] { Presupuesto(1, Ahora.AddMonths(-6).AddMinutes(-1), Ahora.Date.AddMonths(-5), Lomo) };
            PresupuestosCliente.Evaluar(lista, Ahora);

            Assert.False(PresupuestosCliente.AbrirEnPresupuestos(lista, Ahora));
        }

        [Fact]
        public void AbrirEnPresupuestos_con_uno_futuro_es_true()
        {
            var lista = new[] { Presupuesto(1, Ahora.AddDays(10), null, Lomo) };
            PresupuestosCliente.Evaluar(lista, Ahora);

            Assert.True(PresupuestosCliente.AbrirEnPresupuestos(lista, Ahora));
        }

        [Fact]
        public void AbrirEnPresupuestos_sin_presupuestos_es_false()
        {
            Assert.False(PresupuestosCliente.AbrirEnPresupuestos(new List<PresupuestoCliente>(), Ahora));
            Assert.False(PresupuestosCliente.AbrirEnPresupuestos(null, Ahora));
        }

        // --- Armado desde las filas del repositorio ---

        private static DataTable Filas()
        {
            var tabla = new DataTable();
            tabla.Columns.Add("idexpendio", typeof(int));
            tabla.Columns.Add("fechaexpendio", typeof(DateTime));
            tabla.Columns.Add("fechacaducidad", typeof(DateTime));
            tabla.Columns.Add("importe", typeof(double));
            tabla.Columns.Add("idcorte", typeof(int));
            tabla.Columns.Add("codigo", typeof(int));
            tabla.Columns.Add("producto", typeof(string));
            tabla.Columns.Add("preciokg", typeof(double));
            tabla.Columns.Add("cantkg", typeof(double));
            return tabla;
        }

        [Fact]
        public void Armar_agrupa_las_filas_por_presupuesto_y_respeta_el_orden()
        {
            DataTable filas = Filas();
            filas.Rows.Add(5, Ahora.AddDays(-1), Ahora.Date.AddDays(30), 300d, Lomo, 11, "Lomo", 150d, 2d);
            filas.Rows.Add(5, Ahora.AddDays(-1), Ahora.Date.AddDays(30), 300d, Asado, 12, "Asado", 120d, 1d);
            filas.Rows.Add(3, Ahora.AddDays(-9), DBNull.Value, 90d, Lomo, 11, "Lomo", 100d, 1d);

            List<PresupuestoCliente> presupuestos = PresupuestosCliente.Armar(filas);

            Assert.Equal(new[] { 5, 3 }, presupuestos.Select(p => p.IdExpendio).ToArray());
            Assert.Equal(2, presupuestos[0].Lineas.Count);
            Assert.Equal("Asado", presupuestos[0].Lineas[1].Producto);
            Assert.Equal(150d, presupuestos[0].Lineas[0].PrecioKg);
            Assert.Null(presupuestos[1].FechaCaducidad);
            Assert.Equal(Ahora.Date.AddDays(30), presupuestos[0].FechaCaducidad);
        }

        [Fact]
        public void Armar_ignora_lineas_sin_producto_pero_conserva_el_presupuesto()
        {
            DataTable filas = Filas();
            filas.Rows.Add(5, Ahora.AddDays(-1), DBNull.Value, 0d, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);

            List<PresupuestoCliente> presupuestos = PresupuestosCliente.Armar(filas);

            Assert.Single(presupuestos);
            Assert.Empty(presupuestos[0].Lineas);
        }

        [Fact]
        public void Armar_con_tabla_nula_devuelve_lista_vacia()
        {
            Assert.Empty(PresupuestosCliente.Armar(null));
        }
    }
}
