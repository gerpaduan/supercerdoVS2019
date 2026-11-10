using System;
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    // Arma el resumen de la cuenta corriente de un proveedor por titular propio: bloque "Deuda" (comprado, pagado, saldo) y
    // bloque "Control fiscal" (facturado, pagado bancarizado/efectivo, diferencia). Logica pura sobre filas ya leidas.
    // Compras: titular propio y comprobantes del proveedor (2026-11-09, ver docs/DECISIONS.md).
    //  - Los sub-saldos por titular pueden no cerrar en cero (se puede pagar con un titular lo comprado con otro): el TOTAL es
    //    la deuda real.
    //  - Facturado = comprobantes de las compras del periodo (por fecha de compra), con las notas de credito restando.
    //  - "Pagado bancarizado" mayor que "Facturado" se marca: es lo que el usuario quiere controlar. No bloquea nada.
    // Codigo C# 7.3: Negocio tambien compila para net472.
    public static class ResumenCtaCtePorTitular
    {
        public const string TablaCompras = "Compras";
        public const string TablaPagos = "Pagos";

        // desde: inicio del periodo. Lo anterior a esa fecha solo entra como saldo anterior. filtroTitular > 0 deja solo ese titular.
        public static Entidades.ResumenProveedor Armar(
            IEnumerable<Entidades.MovimientoResumen> movimientos,
            IEnumerable<Entidades.PagoResumen> pagos,
            IEnumerable<Entidades.FacturadoResumen> facturado,
            DateTime desde,
            IDictionary<int, string> nombresTitular,
            int filtroTitular)
        {
            var filas = new Dictionary<int, Entidades.FilaResumenTitular>();

            Func<int, Entidades.FilaResumenTitular> fila = idTitular =>
            {
                Entidades.FilaResumenTitular f;
                if (!filas.TryGetValue(idTitular, out f))
                {
                    string nombre;
                    if (nombresTitular == null || !nombresTitular.TryGetValue(idTitular, out nombre))
                        nombre = idTitular > 0 ? "Titular " + idTitular : "Sin titular";
                    f = new Entidades.FilaResumenTitular { IdTitular = idTitular, NombreTitular = nombre };
                    filas[idTitular] = f;
                }
                return f;
            };

            Func<int, bool> aplica = idTitular => filtroTitular <= 0 || idTitular == filtroTitular;

            foreach (var m in movimientos ?? Enumerable.Empty<Entidades.MovimientoResumen>())
            {
                if (m == null || !aplica(m.IdTitular)) continue;
                var f = fila(m.IdTitular);
                if (m.Fecha < desde) { f.SaldoAnterior += m.Importe; continue; }

                // El importe lleva el signo de la base: compra = credito (+), pago a proveedor = debito (-).
                if (m.Tabla == TablaCompras) f.Comprado += m.Importe;
                else if (m.Tabla == TablaPagos) f.Pagado -= m.Importe;
                else f.Otros += m.Importe;
            }

            foreach (var p in pagos ?? Enumerable.Empty<Entidades.PagoResumen>())
            {
                if (p == null || p.Fecha < desde || !aplica(p.IdTitular)) continue;
                decimal bancarizado, efectivo;
                ClasificacionMedioPago.Clasificar(p.FormaPago, p.Importe, p.Efectivo, out bancarizado, out efectivo);
                var f = fila(p.IdTitular);
                f.PagadoBancarizado += bancarizado;
                f.PagadoEfectivo += efectivo;
            }

            foreach (var fa in facturado ?? Enumerable.Empty<Entidades.FacturadoResumen>())
            {
                if (fa == null || fa.FechaCompra < desde || !aplica(fa.IdTitular)) continue;
                fila(fa.IdTitular).FacturadoNeto += fa.TotalConSigno;
            }

            var resultado = new Entidades.ResumenProveedor();
            foreach (var f in filas.Values.OrderBy(x => x.IdTitular))
            {
                Completar(f);
                resultado.Filas.Add(f);

                var t = resultado.Total;
                t.SaldoAnterior += f.SaldoAnterior;
                t.Comprado += f.Comprado;
                t.Pagado += f.Pagado;
                t.Otros += f.Otros;
                t.FacturadoNeto += f.FacturadoNeto;
                t.PagadoBancarizado += f.PagadoBancarizado;
                t.PagadoEfectivo += f.PagadoEfectivo;
            }
            Completar(resultado.Total);
            return resultado;
        }

        // Saldo acumulado del extracto (columna "Saldo"), fila a fila, igual que Negocio.CuentaCorriente.getCtaCteByIdPersona. Se usa con el
        // extracto por titular, que viene del repositorio sin saldo.
        public static void AcumularSaldo(System.Data.DataTable extracto)
        {
            if (extracto == null) return;
            decimal acumulado = 0m;
            foreach (System.Data.DataRow fila in extracto.Rows)
            {
                acumulado += Convert.ToDecimal(fila["importe"]);
                fila["Saldo"] = acumulado;
            }
        }

        // Campos derivados de una fila (la del total tambien).
        private static void Completar(Entidades.FilaResumenTitular f)
        {
            f.Saldo = f.SaldoAnterior + f.Comprado - f.Pagado + f.Otros;
            f.SinFactura = f.Comprado - f.FacturadoNeto;
            f.DiferenciaFacturadoBancarizado = f.FacturadoNeto - f.PagadoBancarizado;
            f.PagadoBancarizadoExcedeFacturado = f.PagadoBancarizado > f.FacturadoNeto;
        }
    }
}
