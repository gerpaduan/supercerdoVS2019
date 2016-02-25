using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Movimiento
    {
        private DateTime fechaMovimiento;
        private Sucursal sucursalOrigen;
        private Sucursal sucursalDestino;
        private int idMovimiento;
        private int? idMovOrigen;
        private string observaciones;
        private DateTime? creado;
        private DateTime? actualizado;

        public DateTime? Actualizado
        {
            get { return actualizado; }
            set { actualizado = value; }
        }

        public DateTime? Creado
        {
            get { return creado; }
            set { creado = value; }
        }

        public DateTime FechaMovimiento
        {
            get
            {
                return fechaMovimiento;
            }
            set
            {
                fechaMovimiento = value;
            }
        }

        public Sucursal SucursalOrigen
        {
            get
            {
                return sucursalOrigen;
            }
            set
            {
                sucursalOrigen = value;
            }
        }

        public Sucursal SucursalDestino
        {
            get
            {
                return sucursalDestino;
            }
            set
            {
                sucursalDestino = value;
            }
        }

        public int IdMovimiento
        {
            get
            {
                return idMovimiento;
            }
            set
            {
                idMovimiento = value;
            }
        }

        public int? IdMovOrigen
        {
            get { return idMovOrigen; }
            set { idMovOrigen = value; }
        }

        public string Observaciones
        {
            get
            {
                return observaciones;
            }
            set
            {
                observaciones = value;
            }
        }
    }
}
