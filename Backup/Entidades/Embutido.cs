using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Embutido
    {
        public int idEmbutido;
         public DateTime fechaEmbutido;
         public Corte corte;
         public Sucursal sucursal;
         public string observaciones;
         public string estado;
         private Usuario creadoPor;
         private Usuario actualizadoPor;
         DateTime creado;
         DateTime? actualizado;

         public DateTime Creado
         {
             get { return creado; }
             set { creado = value; }
         }

         public DateTime? Actualizado
         {
             get { return actualizado; }
             set { actualizado = value; }
         }

         public Usuario CreadoPor
         {
             get { return creadoPor; }
             set { creadoPor = value; }
         }

         public Usuario ActualizadoPor
         {
             get { return actualizadoPor; }
             set { actualizadoPor = value; }
         }

         List<CortePorEmbutido> cortesEnEmbutido;

         public List<CortePorEmbutido> CortesEnEmbutido
         {
             get { return cortesEnEmbutido; }
             set { cortesEnEmbutido = value; }
         }

        public int IdEmbutido
        {
            get
            {
                return idEmbutido;
            }
            set
            {
                idEmbutido = value;
            }
        }

        public DateTime FechaEmbutido
        {
            get
            {
                return fechaEmbutido;
            }
            set
            {
                fechaEmbutido = value;
            }
        }

        public Corte Corte
        {
            get
            {
                return corte;
            }
            set
            {
                corte = value;
            }
        }

        public Sucursal Sucursal
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

        public string Estado
        {
            get
            {
                return estado;
            }
            set
            {
                estado = value;
            }
        }
    }
}
