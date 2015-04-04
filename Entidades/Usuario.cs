using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        string colorForm;

        public string ColorForm
        {
            get { return colorForm; }
            set { colorForm = value; }
        }
    }
}
