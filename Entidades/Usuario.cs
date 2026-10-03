using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace Entidades
{
    public class Usuario
    {
        int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        string nombre;

        public string Nombre
        {
            get { return nombre; }
            set { nombre = value; }
        }
        string usuario;

        public string User
        {
            get { return usuario; }
            set { usuario = value; }
        }
        string clave;

        public string Clave
        {
            get { return clave; }
            set { clave = value; }
        }
        bool admin;

        public bool Admin
        {
            get { return admin; }
            set { admin = value; }
        }

        bool activo;
        string colorForm;

        public string ColorForm
        {
            get { return colorForm; }
            set { colorForm = value; }
        }

        public bool Activo { get => activo; set => activo = value; }


        // Lista de permisos asociados a este usuario
        public List<PermisosUsuarios> Permisos { get; set; } = new List<PermisosUsuarios>();

        //[Required(ErrorMessage = "El email es obligatorio")]
        //[EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public bool EsEmailValido(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public string Email { get => email; set => email = value; }

        string email;

        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public int PasswordHashIterations { get; set; }
        public DateTime? PasswordUpdatedAtUtc { get; set; }

        public int IdSucursal { get; set; }
        public bool PermitirLoginFueraSucursal { get; set; }

        // Login solo desde dispositivos seguros (2026-09-19): si true, este usuario (no admin) solo
        // puede iniciar sesion desde un dispositivo autorizado. Se suma al switch de empresa
        // Empresa.ExigirDispositivoSeguro. Los admin siempre estan exentos.
        public bool RequiereDispositivoSeguro { get; set; }

        // Usuario compartido de sala de produccion: sin acceso a Ventas ni Finanzas (bloqueado
        // server-side en UsuariosController.GuardarPermisos), y nunca admin (validado al guardar
        // en UsuariosController). Al guardar en Movimientos/Stock/Elaborados, el creador real no
        // es este usuario sino el que se elige del modal de seleccion sin password -- ver
        // BaseController.ResolverUsuarioCreador.
        public bool EsUsuarioProduccion { get; set; }

        // Bloqueo de cuenta tras N intentos fallidos de contraseña (ver
        // Security:AccountLockoutMaxAttempts en Web.config). Se resetea a 0/false al desbloquear
        // (por email o por un admin) o al lograr un login exitoso. Distinto de LoginRateLimiter
        // (limite en memoria por IP, se auto-desbloquea por tiempo) -- esto es persistente y
        // requiere desbloqueo explicito.
        public int IntentosFallidosLogin { get; set; }
        public bool Bloqueado { get; set; }
        public DateTime? FechaBloqueoUtc { get; set; }

        // Bloqueo segun origen del intento (2026-10-02, ver docs/DECISIONS.md "Login por CUIT, clave
        // rapida (PIN) y politica de clave"): los fallos desde un dispositivo NO seguro llevan su
        // propio contador/bloqueo, asi un desconocido que adivina contrasenas desde cualquier lado
        // solo bloquea los logins desde dispositivos no seguros de ese usuario; el dueño sigue
        // entrando desde su dispositivo seguro. IntentosFallidosLogin/Bloqueado (arriba) pasan a
        // contar solo los fallos desde dispositivo seguro (el propio usuario equivocandose) y,
        // al llegar al maximo, bloquean la cuenta completa.
        public int IntentosFallidosNoSeguro { get; set; }
        public bool BloqueadoNoSeguro { get; set; }
        public DateTime? FechaBloqueoNoSeguroUtc { get; set; }

        // Clave rapida (PIN) de 4-6 digitos, valida SOLO desde un dispositivo seguro. Hash aparte
        // de la clave: no es un prefijo de la clave segura ni se deriva de ella.
        public string PinHash { get; set; }
        public string PinSalt { get; set; }
        public int PinHashIterations { get; set; }
        public DateTime? PinUpdatedAtUtc { get; set; }
        public bool TienePin
        {
            get { return !string.IsNullOrWhiteSpace(PinHash) && !string.IsNullOrWhiteSpace(PinSalt); }
        }

        public Entidades.Sucursal Sucursal { get; set; }
        public string SucursalNombre { get; set; }
        public List<Entidades.Sucursal> ListaSucursales{ get; set; }

        public int IdEmpresa { get; set; }

        public Entidades.Empresa Empresa { get; set; }
    }
}
