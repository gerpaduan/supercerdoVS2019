using System;

namespace Entidades
{
    // Contexto de visibilidad de un usuario "restringido" frente a las personas con cuenta
    // corriente reservada (Persona.CtaCteReservada, docs/DECISIONS.md 2026-10-01): sobre esas
    // personas solo ve lo que cargo el mismo (IdUsuario) desde la apertura de su caja (Desde).
    // Un usuario autorizado (admin / formCtasCtes) no tiene restriccion (se usa null).
    public class RestriccionCtaCteReservada
    {
        // Tablas cuyos registros se pueden ocultar (valores aceptados por idsRegistrosOcultos).
        public const string TablaVentas = "Ventas";
        public const string TablaCompras = "Compras";
        public const string TablaPagos = "Pagos";
        public const string TablaMovCtaCte = "MovCtaCte";

        // Sin caja abierta no hay "desde la apertura": ningun registro propio cuenta, asi que
        // se usa una fecha futura que ningun "creado" alcanza (queda todo lo reservado oculto).
        public static readonly DateTime SinCajaAbierta = new DateTime(9999, 12, 31);

        // Operador que carga los registros (en la cuenta compartida de produccion es el operador
        // resuelto por step-up, no el usuario de sesion).
        public int IdUsuario { get; set; }

        // Hora de apertura de la caja abierta del operador; un registro propio solo es visible
        // si su "creado" (hora del servidor, no editable) es >= a esta fecha.
        public DateTime Desde { get; set; }
    }
}
