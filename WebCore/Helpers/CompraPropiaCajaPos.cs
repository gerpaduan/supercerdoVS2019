// Regla de permiso para modificar o eliminar una compra desde el POS ("Mis actividades", F6): una persona puede operar SIN el
// permiso "Modificar compra" las compras que ella misma registro y que caen en su caja abierta, de su sucursal. Todo lo demas sigue
// exigiendo el permiso. Es una funcion pura para poder probarla; los controllers le pasan los datos ya resueltos.
// Ver docs/DECISIONS.md (2026-10-28, "Compras propias de la caja desde el POS").
namespace WebCore.Helpers
{
    public static class CompraPropiaCajaPos
    {
        // desdePos: la operacion viene del modal del POS (origen=pos); fuera del POS no hay excepcion.
        // idCreadorCompra / idSucursalCompra: de la compra GUARDADA (no de lo que manda el formulario).
        // idPersona / idSucursalSesion: quien opera (el operador real si es cuenta de produccion) y su sucursal activa.
        // fechaEnCajaAbiertaDeLaPersona: la fecha de la compra cae dentro de la caja abierta de esa persona
        //   (Negocio.CierreCaja.validarCajaAbiertaVendedor).
        public static bool PuedeOperarSinPermiso(bool desdePos, int idCompra, int idCreadorCompra, int idPersona,
            int idSucursalCompra, int idSucursalSesion, bool fechaEnCajaAbiertaDeLaPersona)
        {
            return desdePos
                && idCompra > 0
                && idPersona > 0
                && idCreadorCompra == idPersona
                && idSucursalCompra > 0
                && idSucursalCompra == idSucursalSesion
                && fechaEnCajaAbiertaDeLaPersona;
        }
    }
}
