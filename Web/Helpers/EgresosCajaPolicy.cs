using System;
using Entidades;
using Utilidades;

namespace Web.Helpers
{
    public class EgresoCajaPermisoResultado
    {
        public bool PuedeModificar { get; set; }
        public string MensajeBloqueo { get; set; }
    }

    public static class EgresosCajaPolicy
    {
        public static EgresoCajaPermisoResultado EvaluarModificacion(
            Entidades.Usuario usuario,
            EgresoCaja egreso,
            bool desdePos,
            IEmpresaContext empresa,
            Func<DateTime, Sucursal, Entidades.Usuario, bool> validarCajaAbierta)
        {
            if (usuario == null)
                return Bloqueado("Sesion invalida.");

            if (egreso == null || egreso.Id == 0)
                return Bloqueado("No se encontro el egreso de caja.");

            if (EsCompra(egreso))
                return Bloqueado("Este registro corresponde a una compra y debe modificarse desde Compras.");

            if (EsPagoElectronico(egreso))
                return Bloqueado("Los pagos electronicos se modifican desde Ventas.");

            if (EsCuentaCorriente(egreso))
                return Bloqueado("Los movimientos de cuenta corriente se modifican desde su modulo original.");

            if (!desdePos)
            {
                bool puedeEditar = PermisosHelper.TienePermiso(
                    usuario,
                    empresa,
                    PermisosPantallasWeb.EgresosCaja.AltaEdicion,
                    egreso.Fecha,
                    egreso.CreadoPor
                );

                return puedeEditar
                    ? Permitido()
                    : Bloqueado("No tiene permisos para modificar este egreso de caja.");
            }

            if (egreso.Sucursal == null || egreso.Sucursal.idSucursal <= 0)
                return Bloqueado("No se pudo determinar la sucursal del egreso.");

            if (usuario.IdSucursal <= 0 || usuario.IdSucursal != egreso.Sucursal.idSucursal)
                return Bloqueado("Solo puede modificar egresos de la sucursal activa en la sesion.");

            if (validarCajaAbierta == null || !validarCajaAbierta(egreso.Fecha, egreso.Sucursal, usuario))
                return Bloqueado("La fecha y hora del egreso debe corresponder a una caja abierta del vendedor.");

            return Permitido();
        }

        public static bool EsPagoElectronico(EgresoCaja egreso)
        {
            if (egreso == null)
                return false;

            return egreso.IdTipoEgresoCaja == EgresoCaja.idPagoTarjeta ||
                   string.Equals(egreso.Tabla, EgresoCaja.tablas.Ventas.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsCuentaCorriente(EgresoCaja egreso)
        {
            if (egreso == null)
                return false;

            return egreso.esEgresoCtaCte(egreso.IdTipoEgresoCaja) ||
                   string.Equals(egreso.Tabla, EgresoCaja.tablas.Pagos.ToString(), StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(egreso.Tabla, EgresoCaja.tablas.MovCtaCte.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsCompra(EgresoCaja egreso)
        {
            if (egreso == null)
                return false;

            return (egreso.IdCompra.HasValue && egreso.IdCompra.Value > 0) ||
                   string.Equals(egreso.Tabla, EgresoCaja.tablas.Compras.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static EgresoCajaPermisoResultado Permitido()
        {
            return new EgresoCajaPermisoResultado
            {
                PuedeModificar = true,
                MensajeBloqueo = string.Empty
            };
        }

        private static EgresoCajaPermisoResultado Bloqueado(string mensaje)
        {
            return new EgresoCajaPermisoResultado
            {
                PuedeModificar = false,
                MensajeBloqueo = mensaje ?? "No se puede modificar este egreso."
            };
        }
    }
}
