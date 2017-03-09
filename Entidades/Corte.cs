using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Corte
    {
        public enum tipoCorte
        {
            Embutido,
            Corte,
            Unidad,
            Otro
        }

        public int idCorte;
        public string corte;
        public int codigo;
        public float porcentaje;
        public string tipo;
        public int independiente;
        private bool mayorista;
        private bool enCierreStock;
        public Corte corteMaestro;
        public float precioKg;
        public float porcentajeHueso;
        public float desvioEstandar;
        DateTime creado;
        DateTime? actualizado;
        private float promedio;

        public float Promedio
        {
            get { return promedio; }
            set { promedio = value; }
        }

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


        public int IdCorte
        {
            get
            {
                return idCorte;
            }
            set
            {
                idCorte = value;
            }
        }

        public int Codigo
        {
            get
            {
                return codigo;
            }
            set
            {
                codigo = value;
            }
        }

        public float PrecioKg
        {
            get
            {
                return precioKg;
            }
            set
            {
                precioKg = value;
            }
        }


        public float Porcentaje
        {
            get
            {
                return porcentaje;
            }
            set
            {
                porcentaje = value;
            }
        }

        public string Tipo
        {
            get
            {
                return tipo;
            }
            set
            {
                tipo = value;
            }
        }


        public bool Mayorista
        {
            get { return mayorista; }
            set { mayorista = value; }
        }

        public bool EnCierreStock
        {
            get { return enCierreStock; }
            set { enCierreStock = value; }
        }

        public int Independiente
        {
            get
            {
                return independiente;
            }
            set
            {
                independiente = value;
            }
        }


        public string CorteDesc
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

        public Corte CorteMaestro
        {
            get
            {
                return corteMaestro;
            }
            set
            {
                corteMaestro = value;
            }
        }

        public float PorcentajeHueso
        {
            get
            {
                return porcentajeHueso;
            }
            set
            {
                porcentajeHueso = value;
            }
        }

        public float DesvioEstandar
        {
            get
            {
                return desvioEstandar;
            }
            set
            {
                desvioEstandar = value;
            }
        }
    }
}
