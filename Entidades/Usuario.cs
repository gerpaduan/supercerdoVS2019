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

        // Usuario compartido de sala de produccion: sin acceso a Ventas ni Finanzas (bloqueado
        // server-side en UsuariosController.GuardarPermisos), y nunca admin (validado al guardar
        // en UsuariosController). Al guardar en Movimientos/Stock/Elaborados, el creador real no
        // es este usuario sino el que se elige del modal de seleccion sin password -- ver
        // BaseController.ResolverUsuarioCreador.
        public bool EsUsuarioProduccion { get; set; }

        public Entidades.Sucursal Sucursal { get; set; }
        public string SucursalNombre { get; set; }
        public List<Entidades.Sucursal> ListaSucursales{ get; set; }

        public int IdEmpresa { get; set; }

        public Entidades.Empresa Empresa { get; set; }
    }
}
