using Entidades;

namespace Web.Helpers
{
    public static class PermisosPantallasWeb
    {
        public static class Cajas
        {
            public const string CajasAbiertasConsulta = Permisos.Caja.CierresDeCaja;
            public const string AbrirCaja = Permisos.Caja.CerrarCaja;
            public const string CerrarCaja = Permisos.Caja.CerrarCaja;
        }

        public static class EgresosCaja
        {
            public const string Consulta = Permisos.EgresoCaja.VerEgresosCaja;
            public const string AltaEdicion = Permisos.EgresoCaja.AddOrEditEgresoCaja;
            public const string TiposConsulta = Permisos.EgresoCaja.VerTiposEgresos;
            public const string TiposAltaEdicion = Permisos.EgresoCaja.AddOrEditTipoEgreso;
        }
    }
}
