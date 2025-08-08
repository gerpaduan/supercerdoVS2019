using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Utilidades
{
    public static class Mensajes
    {
        public static void MensajeInicioSesion()
        {
            MessageBox.Show(
                "Inicie Sesión para ingresar a esta función.",
                "Inicie Sesion",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        public static void ErrorPermisoAcceso()
        {
            MessageBox.Show(
                "No tienes permiso para acceder a esta área o al rango de fechas seleccionado.",
                "Acceso denegado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        public static void ErrorPermisoEdicion()
        {
            MessageBox.Show(
                "No tienes permiso para crear o editar registros que no hayas generado tú o que correspondan a la fecha seleccionada.",
                "Edición no permitida",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

}
