using System;

namespace Negocio
{
    // Capa de negocio de la conexion OAuth con Mercado Pago (una cuenta de MP por empresa). Sin
    // precedente en SQL Server -- nace directo sobre Postgres (Contratos.IMercadoPagoConfigRepository),
    // sin constructor "modo SQL Server" como las clases migradas (ver DispositivoSeguro.cs).
    //
    // Cifra/descifra los tokens con Utilidades.MercadoPagoTokenCipher antes de tocar el
    // repositorio -- el repositorio (DatosPostgres.MercadoPagoConfigPg) nunca ve texto plano.
    public class MercadoPagoConfig
    {
        private readonly Contratos.IMercadoPagoConfigRepository oMercadoPagoConfigD;
        private readonly byte[] _claveCifrado;

        public MercadoPagoConfig(Contratos.IMercadoPagoConfigRepository repositorio, byte[] claveCifrado)
        {
            oMercadoPagoConfigD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
            _claveCifrado = claveCifrado ?? throw new ArgumentNullException(nameof(claveCifrado));
        }

        // Devuelve el estado de conexion con los tokens YA DESCIFRADOS -- quien llame es
        // responsable de nunca loggearlos ni exponerlos fuera del uso inmediato (armar el header
        // Authorization contra la API de Mercado Pago).
        public Entidades.MercadoPagoConfig ObtenerPorEmpresa(int idEmpresa)
        {
            var config = oMercadoPagoConfigD.ObtenerPorEmpresa(idEmpresa);
            if (config == null) return null;

            config.AccessToken = DescifrarSiHayValor(config.AccessToken);
            config.RefreshToken = DescifrarSiHayValor(config.RefreshToken);
            return config;
        }

        public string IniciarConexion(int idEmpresa)
        {
            string clientState = Utilidades.PasswordSecurity.GenerateToken();
            oMercadoPagoConfigD.GuardarClientState(idEmpresa, clientState);
            return clientState;
        }

        public void GuardarTokens(int idEmpresa, string accessToken, string refreshToken, DateTime tokenExpiraUtc, int idUsuarioConexion, string mpUserId)
        {
            if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentException("El access_token es obligatorio.", nameof(accessToken));

            string accessTokenCifrado = Utilidades.MercadoPagoTokenCipher.Encrypt(accessToken, _claveCifrado);
            string refreshTokenCifrado = Utilidades.MercadoPagoTokenCipher.Encrypt(refreshToken ?? "", _claveCifrado);

            oMercadoPagoConfigD.GuardarTokens(idEmpresa, accessTokenCifrado, refreshTokenCifrado, tokenExpiraUtc, idUsuarioConexion, mpUserId);
        }

        public void Desconectar(int idEmpresa)
        {
            oMercadoPagoConfigD.Desconectar(idEmpresa);
        }

        private string DescifrarSiHayValor(string valorCifrado)
        {
            return string.IsNullOrEmpty(valorCifrado) ? "" : Utilidades.MercadoPagoTokenCipher.Decrypt(valorCifrado, _claveCifrado);
        }
    }
}
