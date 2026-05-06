namespace Web.Models
{
    public class TipoEgresoCajaEditVm
    {
        public int Id { get; set; }
        public string TipoEgresoCaja { get; set; }
        public bool EsGasto { get; set; }
        public bool Reservado { get; set; }
    }
}
