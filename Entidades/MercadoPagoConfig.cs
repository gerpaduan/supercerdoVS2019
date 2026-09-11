using System;

namespace Entidades
{
    // Credenciales OAuth de la cuenta de Mercado Pago que una empresa conecto para operar sus
    // terminales Point. Una fila por empresa (una sola cuenta de MP cubre todas sus sucursales,
    // ver docs/DECISIONS.md).
    //
    // AccessToken/RefreshToken tienen DOBLE VIDA segun quien construya el objeto -- ojo al
    // usarlos: DatosPostgres.MercadoPagoConfigPg los llena con el valor CIFRADO tal cual esta en
    // la columna; Negocio.MercadoPagoConfig.ObtenerPorEmpresa los pisa con el valor ya
    // DESCIFRADO antes de devolver el objeto a quien llama. El nombre del campo, a proposito,
    // no dice "Cifrado": el consumidor normal (controllers) siempre pasa por
    // Negocio.MercadoPagoConfig y siempre recibe texto plano listo para usar como Bearer token.
    public class MercadoPagoConfig
    {
        public int IdEmpresa { get; set; }

        // Token opaco generado al iniciar "Conectar con Mercado Pago", usado como parametro
        // OAuth "state" y validado en el callback antes de confiar en el idEmpresa de la URL.
        public string ClientState { get; set; }

        // "user_id" que devuelve Mercado Pago en el intercambio OAuth -- lo pide la API de
        // Stores (Fase 3, POST /users/{user_id}/stores). Agregado 2026-09-01, no existia en el
        // esquema original de la Fase 1.
        public string MpUserId { get; set; }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? TokenExpiraUtc { get; set; }
        public bool Conectado { get; set; }
        public int? IdUsuarioConexion { get; set; }
        public DateTime? FechaConexionUtc { get; set; }
        public DateTime? FechaActualizacionUtc { get; set; }
    }
}
