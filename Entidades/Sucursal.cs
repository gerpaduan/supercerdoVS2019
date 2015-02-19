using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Sucursal
    {
        public int idSucursal;
        public string sucursal;

        public int IdSucursal
        {
            get
            {
                return idSucursal;
            }
            set
            {
                idSucursal = value;
            }
        }

        public string SucursalNombre
        {
            get
            {
                return sucursal;
            }
            set
            {
                sucursal = value;
            }
        }
    }
}
