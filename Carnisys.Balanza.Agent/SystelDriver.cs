using System.IO.Ports;

namespace Carnisys.Balanza.Agent
{
    internal sealed class SystelDriver : IBalanzaDriver
    {
        public string Marca => "Systel";

        public string Modelo => "SystelGenerica";

        public void PrepararPuerto(SerialPort port, BalanzaConfig config)
        {
            port.StopBits = StopBits.One;
        }

        public void SolicitarPeso(SerialPort port)
        {
            var buffer = new byte[] { 7, 7 };
            port.Write(buffer, 0, buffer.Length);
        }

        public LecturaPeso ParsearRespuesta(string raw, BalanzaConfig config)
        {
            string texto = PesoHelper.LimpiarRaw(raw);
            if (string.IsNullOrWhiteSpace(texto))
            {
                return LecturaPeso.CrearError("Sin respuesta de la balanza.", config, raw);
            }

            if (texto.Length > 20)
            {
                return LecturaPeso.CrearError("Respuesta Systel inválida.", config, raw, true);
            }

            bool esNegativo = false;
            int contar = 0;
            char[] indices = new char[texto.Length + 1];

            foreach (char letra in texto)
            {
                if (letra == '-')
                {
                    indices[contar++] = letra;
                    esNegativo = true;
                }

                if (contar == 3 && !esNegativo)
                {
                    indices[contar++] = '.';
                }
                else if (contar == 4 && esNegativo)
                {
                    indices[contar++] = '.';
                }

                if (char.IsDigit(letra))
                {
                    indices[contar++] = letra;
                }
            }

            if (contar <= 0)
            {
                return LecturaPeso.CrearError("No se pudo interpretar la lectura Systel.", config, raw, true);
            }

            string pesoTexto = new string(indices, 0, contar);
            bool inestable = texto.Contains("i") && !texto.Contains("ei");

            if (pesoTexto.Length < 5)
            {
                return LecturaPeso.CrearError("Lectura Systel incompleta.", config, raw, true);
            }

            if (!PesoHelper.TryParsePesoTexto(pesoTexto, out decimal peso))
            {
                return LecturaPeso.CrearError("Peso Systel inválido.", config, raw, true);
            }

            return PesoHelper.CrearLecturaOk(config, raw, peso, inestable);
        }
    }
}
