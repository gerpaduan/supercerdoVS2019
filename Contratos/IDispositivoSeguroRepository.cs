using System.Collections.Generic;

namespace Contratos
{
    // Espeja Datos.DispositivoSeguro completo (4/4 metodos). ExisteSerieSegura se usa en el
    // login (antes de autenticar) para decidir si se saltea LoginRateLimiter -- el idEmpresa ya
    // se conoce en ese punto (resuelto del candidato por usuario/email), asi que RLS estandar
    // no genera el problema de "tenant todavia no conocido" que tiene usuarios (Etapa 13a).
    public interface IDispositivoSeguroRepository
    {
        List<Entidades.DispositivoSeguro> Listar(int idEmpresa);
        void Agregar(Entidades.DispositivoSeguro dispositivo);
        void Eliminar(int id, int idEmpresa);
        bool ExisteSerieSegura(string numeroSerie, int idEmpresa);
    }
}
