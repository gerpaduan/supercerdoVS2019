using System.IO.Ports;

namespace Carnisys.Balanza.Agent
{
    internal sealed class KretzDriver : IBalanzaDriver
    {
        public string Marca => "Kretz";

        public string Modelo => "KretzGenerica";

        public void PrepararPuerto(SerialPort port, BalanzaConfig config)
        {
            port.StopBits = StopBits.Two;
        }

        public void SolicitarPeso(SerialPort port)
        {
            port.Write("p");
        }

        public LecturaPeso ParsearRespuesta(string raw, BalanzaConfig config)
        {
            string texto = PesoHelper.LimpiarRaw(raw);
            if (string.IsNullOrWhiteSpace(texto))
            {
                return LecturaPeso.CrearError("Sin respuesta de la balanza.", config, raw);
            }

            string pesoTexto = texto.Length > 1
                ? texto.Substring(1, texto.Length - 2).Trim()
                : texto.Trim();

            if (!PesoHelper.TryParsePesoTexto(pesoTexto, out decimal peso))
            {
                return LecturaPeso.CrearError("No se pudo interpretar la lectura Kretz.", config, raw, true);
            }

            return PesoHelper.CrearLecturaOk(config, raw, peso, false);
        }
    }
}
