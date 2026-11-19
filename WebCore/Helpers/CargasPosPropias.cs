// Regla "propio de la caja" de las cargas del POS (2026-11-19, docs/DECISIONS.md "Cargas desde el POS: modificar lo cargado"):
// un registro de Stock, Movimientos o Elaborados que el operador cargo desde que abrio su caja (o desde hoy a las 00:00 con "operar
// sin caja") lo puede abrir y modificar desde el POS aunque no tenga el permiso del modulo. Mismo criterio que la compra del POS
// (ComprasController.PuedeOperarCompraPropiaEnPos). Eliminar y el ajuste de stock NO entran en esta regla.
// Una sola fuente del calculo de "desde" para la lista "Cargado desde que abriste la caja" (VentasController.CargasDesdeAperturaPOS) y
// para las pantallas destino (StockController, MovimientosController, ElaboradosController).
using Utilidades;

namespace WebCore.Helpers
{
    public static class CargasPosPropias
    {
        // La apertura de caja toma la hora del navegador (sin segundos) y "creado" la de la base de datos: el margen evita perder
        // una carga hecha en el mismo minuto de la apertura.
        public const int MargenAperturaMinutos = 2;

        // Desde cuando se cuentan las cargas del operador: la apertura de su caja menos el margen, o hoy a las 00:00 si no hay
        // apertura ("operar sin caja"). conCaja dice cual de los dos casos fue (cambia el titulo de la lista).
        public static DateTime CalcularDesde(DateTime? aperturaCaja, DateTime ahora, out bool conCaja)
        {
            if (aperturaCaja.HasValue)
            {
                conCaja = true;
                return aperturaCaja.Value.AddMinutes(-MargenAperturaMinutos);
            }

            conCaja = false;
            return ahora.Date;
        }

        // Origen de datos ('stock' | 'movimiento' | 'elaborado') de un tipo de la lista; null si el tipo no existe.
        // 'cierre' y 'desarme' figuran en la lista pero no en "Nueva carga".
        public static string? OrigenDe(string? tipo)
        {
            switch ((tipo ?? "").Trim())
            {
                case "ingreso":
                case "egreso":
                case "pesaje":
                case "ajuste":
                case "cierre":
                    return "stock";
                case "movimiento":
                    return "movimiento";
                case "elaborado":
                case "rapido":
                case "desarme":
                    return "elaborado";
                default:
                    return null;
            }
        }

        // Titulo de un tipo de la lista (para el modal del registro abierto con el ojo). Si el tipo no existe devuelve "Carga".
        public static string TituloDe(string? tipo)
        {
            switch ((tipo ?? "").Trim())
            {
                case "ingreso": return "Ingreso de stock";
                case "egreso": return "Egreso de stock";
                case "pesaje": return "Pesaje de cortes";
                case "ajuste": return "Ajuste de stock";
                case "cierre": return "Cierre de stock";
                case "movimiento": return "Movimiento";
                case "elaborado": return "Elaborado";
                case "rapido": return "Ingreso rápido de elaborado";
                case "desarme": return "Desarme de elaborado";
                default: return "Carga";
            }
        }

        // true si el operador cargo ese registro desde que abrio su caja. Consulta la caja abierta del operador y el repositorio
        // (solo Postgres: con SQL Server no hay repositorio y devuelve false, rigen los permisos de siempre).
        public static bool EsPropiaDeLaCaja(IEmpresaContext empresa, IParametrosContext param, Entidades.Usuario sesion,
            Entidades.Usuario operador, string origen, int id)
        {
            if (sesion == null || operador == null || operador.Id <= 0 || id <= 0) return false;

            var repo = WebCore.Infrastructure.NegocioFactory.CrearCargasPosRepository(empresa);
            if (repo == null) return false;

            var cierre = WebCore.Infrastructure.NegocioFactory.CrearCierreCaja(empresa, param).CajaAbiertaDelVendedor(sesion.IdSucursal, operador);
            DateTime desde = CalcularDesde(cierre?.FechaHoraInicio, DateTime.Now, out _);
            return repo.EsCargaPropia(origen, id, operador.Id, desde);
        }
    }
}
