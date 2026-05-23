using System.IO.Ports;

namespace Carnisys.Balanza.Agent
{
    public interface IBalanzaDriver
    {
        string Marca { get; }
        string Modelo { get; }
        void PrepararPuerto(SerialPort port, BalanzaConfig config);
        void SolicitarPeso(SerialPort port);
        LecturaPeso ParsearRespuesta(string raw, BalanzaConfig config);
    }
}
