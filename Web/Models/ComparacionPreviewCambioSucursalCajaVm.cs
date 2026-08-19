namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para
    // obtenerPreviewCambioSucursalCaja (Etapa 10) -- de solo lectura, no muta datos.
    public class ComparacionPreviewCambioSucursalCajaVm
    {
        public Contratos.CambioSucursalCajaPreview SqlServer { get; set; }
        public Contratos.CambioSucursalCajaPreview Postgres { get; set; }
    }
}
