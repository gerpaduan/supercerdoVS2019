namespace Web.Models
{
    public class MarcaEditVm
    {
        public int IdPersona { get; set; }
        public string RazonSocial { get; set; }
        public string OtrosDatos { get; set; }
        public int? IdPropietario { get; set; }
        public string PropietarioNombre { get; set; }
        public bool EsAdministrador { get; set; }
        public bool SoloLecturaNombre { get; set; }
        public bool ConfirmarMarcasParecidas { get; set; }
    }
}
