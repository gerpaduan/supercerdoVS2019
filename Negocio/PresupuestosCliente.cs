using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Negocio
{
    // Estado de un presupuesto como documento (cabecera), segun su vigencia y su caducidad.
    public enum EstadoPresupuestoCliente
    {
        Vigente,
        Futuro,
        Caducado
    }

    // Estado de un producto dentro de un presupuesto: solo "Aplicable" permite copiar su precio a
    // una venta (POS de Ventas). Ver PresupuestosCliente.Evaluar.
    public enum EstadoLineaPresupuestoCliente
    {
        Aplicable,
        Futuro,
        Reemplazado,
        Caducado
    }

    public class LineaPresupuestoCliente
    {
        public int IdCorte { get; set; }
        public string Codigo { get; set; }
        public string Producto { get; set; }
        public double PrecioKg { get; set; }
        public double CantKg { get; set; }
        public EstadoLineaPresupuestoCliente Estado { get; set; }
        // Solo si Estado == Reemplazado: numero del presupuesto que lo reemplaza.
        public int ReemplazadoPor { get; set; }
    }

    public class PresupuestoCliente
    {
        public int IdExpendio { get; set; }
        // "Vigencia de precios desde": fecha del expendio (puede ser futura).
        public DateTime Vigencia { get; set; }
        // Fecha guardada; null en presupuestos anteriores a la caducidad (se asume vigencia + 90 dias).
        public DateTime? FechaCaducidad { get; set; }
        public double Importe { get; set; }
        // Ultimo dia (inclusive) en que sus precios se pueden aplicar. La completa Evaluar.
        public DateTime CaducaEl { get; set; }
        public EstadoPresupuestoCliente Estado { get; set; }
        public List<LineaPresupuestoCliente> Lineas { get; set; } = new List<LineaPresupuestoCliente>();
    }

    // Regla de los presupuestos de un cliente en el historial de precios (F8 de Ventas y de
    // Expendio). Logica pura, sin base de datos ni hora del sistema (la hora entra por parametro)
    // para poder probarla. Ver docs/DECISIONS.md (2026-10-04).
    public static class PresupuestosCliente
    {
        // Agrupa las filas planas de IPresupuestoClienteRepository.obtenerPresupuestosPorCliente
        // (una por producto de cada presupuesto) en presupuestos con sus lineas, respetando el
        // orden recibido (el repositorio los entrega del mas reciente al mas viejo). Sin evaluar.
        public static List<PresupuestoCliente> Armar(DataTable filas)
        {
            var resultado = new List<PresupuestoCliente>();
            if (filas == null) return resultado;

            var porId = new Dictionary<int, PresupuestoCliente>();
            foreach (DataRow fila in filas.Rows)
            {
                int idExpendio = Convert.ToInt32(fila["idexpendio"]);

                if (!porId.TryGetValue(idExpendio, out PresupuestoCliente presupuesto))
                {
                    presupuesto = new PresupuestoCliente
                    {
                        IdExpendio = idExpendio,
                        Vigencia = Convert.ToDateTime(fila["fechaexpendio"]),
                        FechaCaducidad = fila["fechacaducidad"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(fila["fechacaducidad"]).Date,
                        Importe = fila["importe"] == DBNull.Value ? 0d : Convert.ToDouble(fila["importe"])
                    };
                    porId[idExpendio] = presupuesto;
                    resultado.Add(presupuesto);
                }

                // Linea sin producto (corte borrado): no se puede mostrar ni copiar.
                if (fila["idcorte"] == DBNull.Value) continue;

                presupuesto.Lineas.Add(new LineaPresupuestoCliente
                {
                    IdCorte = Convert.ToInt32(fila["idcorte"]),
                    Codigo = fila["codigo"] == DBNull.Value ? "" : Convert.ToString(fila["codigo"]),
                    Producto = fila["producto"] == DBNull.Value ? "" : Convert.ToString(fila["producto"]),
                    PrecioKg = fila["preciokg"] == DBNull.Value ? 0d : Convert.ToDouble(fila["preciokg"]),
                    CantKg = fila["cantkg"] == DBNull.Value ? 0d : Convert.ToDouble(fila["cantkg"])
                });
            }

            return resultado;
        }

        // Completa CaducaEl y Estado de cada presupuesto y Estado/ReemplazadoPor de cada linea.
        // Por producto, en este orden de precedencia:
        //  1. Futuro: la vigencia del presupuesto todavia no empezo.
        //  2. Reemplazado: hay otro presupuesto ya vigente (vigencia <= ahora) con ese producto y
        //     mayor vigencia (empate: mayor numero) -- AUNQUE ese otro ya haya caducado: si el
        //     ultimo presupuesto caduco no se "resucita" el anterior con un precio viejo.
        //  3. Caducado: es el ultimo de ese producto pero ahora.Date > CaducaEl.
        //  4. Aplicable: es el ultimo de ese producto y no caduco.
        public static void Evaluar(IList<PresupuestoCliente> presupuestos, DateTime ahora)
        {
            if (presupuestos == null) return;

            foreach (PresupuestoCliente p in presupuestos)
            {
                p.CaducaEl = (p.FechaCaducidad ?? p.Vigencia.Date.AddDays(SectorPuntoExpendio.DiasCaducidadPresupuestoPorDefecto)).Date;
                p.Estado = p.Vigencia > ahora
                    ? EstadoPresupuestoCliente.Futuro
                    : (ahora.Date > p.CaducaEl ? EstadoPresupuestoCliente.Caducado : EstadoPresupuestoCliente.Vigente);
            }

            // Ganador por producto: el presupuesto ya vigente mas reciente que lo contiene.
            var ganadorPorProducto = new Dictionary<int, PresupuestoCliente>();
            foreach (PresupuestoCliente p in presupuestos.Where(x => x.Vigencia <= ahora))
            {
                foreach (int idCorte in p.Lineas.Select(l => l.IdCorte).Distinct())
                {
                    if (!ganadorPorProducto.TryGetValue(idCorte, out PresupuestoCliente actual) || EsMasReciente(p, actual))
                        ganadorPorProducto[idCorte] = p;
                }
            }

            foreach (PresupuestoCliente p in presupuestos)
            {
                foreach (LineaPresupuestoCliente linea in p.Lineas)
                {
                    linea.ReemplazadoPor = 0;

                    if (p.Estado == EstadoPresupuestoCliente.Futuro)
                    {
                        linea.Estado = EstadoLineaPresupuestoCliente.Futuro;
                        continue;
                    }

                    PresupuestoCliente ganador = ganadorPorProducto[linea.IdCorte];
                    if (ganador.IdExpendio != p.IdExpendio)
                    {
                        linea.Estado = EstadoLineaPresupuestoCliente.Reemplazado;
                        linea.ReemplazadoPor = ganador.IdExpendio;
                    }
                    else
                    {
                        linea.Estado = p.Estado == EstadoPresupuestoCliente.Caducado
                            ? EstadoLineaPresupuestoCliente.Caducado
                            : EstadoLineaPresupuestoCliente.Aplicable;
                    }
                }
            }
        }

        // Solapa inicial del historial de precios: Presupuestos si el cliente tiene algun
        // presupuesto vigente (ya empezo y no caduco) y/o con vigencia en los ultimos
        // MesesPresupuestoRecienteSolapaInicial meses (incluye caducados recientes y futuros);
        // si no, Compras. Requiere los presupuestos ya evaluados (Evaluar).
        public static bool AbrirEnPresupuestos(IEnumerable<PresupuestoCliente> presupuestos, DateTime ahora)
        {
            if (presupuestos == null) return false;

            DateTime desde = ahora.AddMonths(-SectorPuntoExpendio.MesesPresupuestoRecienteSolapaInicial);
            return presupuestos.Any(p => p.Estado == EstadoPresupuestoCliente.Vigente || p.Vigencia >= desde);
        }

        // Mayor vigencia gana; con la misma vigencia, el de mayor numero (cargado despues).
        private static bool EsMasReciente(PresupuestoCliente candidato, PresupuestoCliente actual)
        {
            if (candidato.Vigencia != actual.Vigencia) return candidato.Vigencia > actual.Vigencia;
            return candidato.IdExpendio > actual.IdExpendio;
        }
    }
}
