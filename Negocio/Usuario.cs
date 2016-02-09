using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Negocio
{
    public class Usuario
    {
        Datos.Usuario oUsuarioD;
        public DataTable dtUsuarios;
        List<Entidades.Usuario> listUsuarios;

        public DataTable obtenerUsuarios()
        {
            oUsuarioD = new Datos.Usuario();
            dtUsuarios = oUsuarioD.obtenerUsuarios();

            return dtUsuarios;
        }

        public List<Entidades.Usuario> convertDatatableToList()
        {
            if (dtUsuarios == null)
	        {
        	    obtenerUsuarios();
	        }
            if (dtUsuarios.Rows.Count > 0)
            {
                listUsuarios = new List<Entidades.Usuario>();
                Entidades.Usuario user;
                foreach (DataRow drUsuario in dtUsuarios.Rows)
                {
                    user = new Entidades.Usuario();
                    user.Id = Convert.ToInt32(drUsuario["id"]);
                    user.Nombre = Convert.ToString(drUsuario["nombre"]);
                    user.User = Convert.ToString(drUsuario["usuario"]);
                    user.Clave = Convert.ToString(drUsuario["clave"]);
                    user.Admin = Convert.ToBoolean(drUsuario["admin"]);
                    user.ColorForm = Convert.ToString(drUsuario["colorForm"]);
   
                    listUsuarios.Add(user);
                }
            }
            return listUsuarios;
        }

        public Entidades.Usuario validarUsuario(string usuario, string clave)
        {
            Entidades.Usuario userEncontrado = null;
            if (listUsuarios == null)
	        {
                listUsuarios = convertDatatableToList();
	        }
            foreach (Entidades.Usuario user in listUsuarios)
            {
                if (user.User.Equals(usuario) && user.Clave.Equals(clave))
                {
                    userEncontrado = new Entidades.Usuario();
                    userEncontrado = user;
                    break;
                }
            }
            return userEncontrado;
        }

        public Entidades.Usuario getUser(string usuario) {
            Entidades.Usuario userEncontrado = null;
            if (listUsuarios == null)
            {
                listUsuarios = convertDatatableToList();
            }
            foreach (Entidades.Usuario user in listUsuarios)
            {
                if (user.User.Equals(usuario))
                {
                    userEncontrado = new Entidades.Usuario();
                    userEncontrado = user;
                }
            }
            return userEncontrado;
        }

        public Entidades.Usuario getUserById(int idUsuario)
        {
            Entidades.Usuario userEncontrado = null;
            if (listUsuarios == null)
            {
                listUsuarios = convertDatatableToList();
            }
            foreach (Entidades.Usuario user in listUsuarios)
            {
                if (user.Id.Equals(idUsuario))
                {
                    userEncontrado = new Entidades.Usuario();
                    userEncontrado = user;
                }
            }
            return userEncontrado;
        }

        public void addOrEditUser(Entidades.Usuario oUsuarioE)
        {
            oUsuarioD.addOrEditUser(oUsuarioE);
        }
    }
}
