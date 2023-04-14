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

        public int getPtoVtaAfip(int idSucursal)
        {
            int nroPtoVtaAfip = 0;
            switch (idSucursal)
            {
                case 1:
                    nroPtoVtaAfip = 7;
                    break;
                case 2: 
                    nroPtoVtaAfip = 6;
                    break;
                default:
                    break;
            }
            return nroPtoVtaAfip;
        }

        public string getNomSucPorPtoVtaAfip(int ptoVtaAfip)
        {
            string nomSucursal = "No Encontrada";
            switch (ptoVtaAfip)
            {
                case 6:
                    nomSucursal = "San Martin";
                    break;
                case 7:
                    nomSucursal = "San Lorenzo";
                    break;
                default:
                    break;
            }
            return nomSucursal;
        }
    }
}
