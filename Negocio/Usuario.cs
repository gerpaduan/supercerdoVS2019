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

        public DataTable obtenerUsuarios(bool soloActivos)
        {
            oUsuarioD = new Datos.Usuario();
            dtUsuarios = oUsuarioD.obtenerUsuarios(soloActivos);
            convertDatatableToList();
            return dtUsuarios;
        }

        public DataTable getUsuarioActivos()
        {
            DataTable dtUserActivos;
            oUsuarioD = new Datos.Usuario();
            dtUserActivos = oUsuarioD.getUsuarioActivos();
            return dtUserActivos;
        }

        public DataTable obtenerUsuariosConTodos(bool soloActivos)
        {
            dtUsuarios = obtenerUsuarios(soloActivos);
            DataRow drTodos = dtUsuarios.NewRow();
            drTodos["id"] = -1;
            drTodos["nombre"] = "Todos";
            dtUsuarios.Rows.Add(drTodos);
            dtUsuarios.DefaultView.Sort = "id";

            return dtUsuarios;
        }

        public List<Entidades.Usuario> convertDatatableToList()
        {
            if (dtUsuarios == null || (listUsuarios != null && listUsuarios.Count != dtUsuarios.Rows.Count))
            {
                obtenerUsuarios(false);
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
                    user.Activo = Convert.ToBoolean(drUsuario["activo"]);
                    user.ColorForm = Convert.ToString(drUsuario["colorForm"]);

                    listUsuarios.Add(user);
                }
            }
            return listUsuarios;
        }

        public List<Entidades.Usuario> listaUsuario()
        {
            return convertDatatableToList();
        }

        public Entidades.Usuario validarUsuario(string usuario, string clave, bool soloNombreUsuario)
        {
            Entidades.Usuario userEncontrado = null;
            if (listUsuarios == null)
            {
                listUsuarios = convertDatatableToList();
            }
            foreach (Entidades.Usuario user in listUsuarios)
            {
                if (soloNombreUsuario)
                {
                    if (user.User.Equals(usuario))
                    {
                        userEncontrado = new Entidades.Usuario();
                        userEncontrado = user;
                        break;
                    }
                }
                else
                {
                    if (user.User.Equals(usuario) && user.Clave.Equals(clave))
                    {
                        userEncontrado = new Entidades.Usuario();
                        userEncontrado = user;
                        break;
                    }
                }
            }
            return userEncontrado;
        }

        public Entidades.Usuario getUser(string usuario)
        {
            convertDatatableToList();

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

        public List<Entidades.PermisosUsuarios> getPermisosUsuario(int idUsuario)
        {
            return oUsuarioD.getPermisosUsuario(idUsuario);
        }
        public void AddOrEditPermisos(List<Entidades.PermisosUsuarios> permisos)
        {
            oUsuarioD.AddOrEditPermisos(permisos);
        }
    }
}
