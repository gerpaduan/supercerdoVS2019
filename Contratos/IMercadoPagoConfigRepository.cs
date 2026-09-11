using System;

namespace Contratos
{
    public interface IMercadoPagoConfigRepository
    {
        Entidades.MercadoPagoConfig ObtenerPorEmpresa(int idEmpresa);

        // Genera/renueva el "state" antes de redirigir a la pantalla de autorizacion de MP.
        void GuardarClientState(int idEmpresa, string clientState);

        // Se llama despues de intercambiar el "code" del callback por los tokens. mpUserId es
        // el "user_id" que devuelve Mercado Pago en esa misma respuesta (agregado 2026-09-01,
        // lo necesita la API de Stores de la Fase 3).
        void GuardarTokens(int idEmpresa, string accessTokenCifrado, string refreshTokenCifrado,
            DateTime tokenExpiraUtc, int idUsuarioConexion, string mpUserId);

        void Desconectar(int idEmpresa);
    }
}
