using System.Collections.Generic;

namespace Contratos
{
    // Espeja Datos.DispositivoSeguro completo. ExisteSerieSegura se usa en el login (antes de
    // autenticar) para decidir si se saltea LoginRateLimiter y, desde 2026-09-19, si el
    // dispositivo cumple la exigencia de "dispositivo seguro" -- el idEmpresa ya se conoce en ese
    // punto (resuelto del candidato por usuario/email), asi que RLS estandar no genera el problema
    // de "tenant todavia no conocido" que tiene usuarios (Etapa 13a).
    public interface IDispositivoSeguroRepository
    {
        List<Entidades.DispositivoSeguro> Listar(int idEmpresa);
        void Agregar(Entidades.DispositivoSeguro dispositivo);
        void Eliminar(int id, int idEmpresa);

        // true solo si existe Y no esta bloqueado por un admin.
        bool ExisteSerieSegura(string numeroSerie, int idEmpresa);

        // Devuelve el dispositivo (bloqueado o no) o null -- el login lo usa para distinguir
        // "no autorizado" de "bloqueado por el administrador".
        Entidades.DispositivoSeguro ObtenerPorSerie(string numeroSerie, int idEmpresa);

        void SetBloqueado(int id, int idEmpresa, bool bloqueado);
    }
}
