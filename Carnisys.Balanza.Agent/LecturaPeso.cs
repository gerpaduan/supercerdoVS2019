using System;

namespace Carnisys.Balanza.Agent
{
    public sealed class LecturaPeso
    {
        public bool Ok { get; set; }
        public bool Conectada { get; set; }
        public decimal Peso { get; set; }
        public string PesoTexto { get; set; }
        public string PesoDisplay { get; set; }
        public string Unidad { get; set; }
        public bool Estable { get; set; }
        public bool Inestable { get; set; }
        public bool Negativo { get; set; }
        public string Raw { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Puerto { get; set; }
        public DateTime FechaHora { get; set; }
        public string Error { get; set; }

        public LecturaPeso Clone()
        {
            return (LecturaPeso)MemberwiseClone();
        }

        public static LecturaPeso CrearError(string error, BalanzaConfig config = null, string raw = null, bool conectada = false)
        {
            return new LecturaPeso
            {
                Ok = false,
                Conectada = conectada,
                Peso = 0m,
                PesoTexto = "0.000",
                PesoDisplay = "0.000",
                Unidad = "kg",
                Estable = false,
                Inestable = false,
                Negativo = false,
                Raw = raw ?? string.Empty,
                Marca = config != null ? config.Marca ?? string.Empty : string.Empty,
                Modelo = config != null ? config.Modelo ?? string.Empty : string.Empty,
                Puerto = config != null ? config.Puerto ?? string.Empty : string.Empty,
                FechaHora = DateTime.Now,
                Error = error ?? "Error de lectura."
            };
        }
    }
}
