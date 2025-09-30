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
            Pesable,
            Elaborado,
            Otro
        }

        public int idCorte;
        public string corte;
        public long codigo;
        public float porcentaje;
        public string tipo;
        public int independiente;
        private bool ingresoRapidoEmbutido;
        private bool enCierreStock;
        public Corte corteMaestro;
        private Persona marca;
        public float precioKg;
        public float precioKgReferencia;
        public float porcentajeHueso;
        public float desvioEstandar;
        DateTime creado;
        DateTime? actualizado;
        private float promedio;
        private bool habilitado;
        private int idAlicuotaIva;
        private float alicuotaIva;
        private bool pesable;
        private int nivel;
        private int puntoStock;
        private bool codBarraValidadoEnPos = false;

        public bool Habilitado
        {
            get { return habilitado; }
            set { habilitado = value; }
        }

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

        public long Codigo
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


        public float PrecioKgReferencia
        {
            get
            {
                return precioKgReferencia;
            }
            set
            {
                precioKgReferencia = value;
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


        public bool IngresoRapidoEmbutido
        {
            get { return ingresoRapidoEmbutido; }
            set { ingresoRapidoEmbutido = value; }
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
        public int IdAlicuotaIva { get => idAlicuotaIva; set => idAlicuotaIva = value; }
        public float AlicuotaIva { get => alicuotaIva; set => alicuotaIva = value; }
        public bool Pesable { get => pesable; set => pesable = value; }
        public int Nivel { get => nivel; set => nivel = value; }
        public int PuntoStock { get => puntoStock; set => puntoStock = value; }
        public Persona Marca { get => marca; set => marca = value; }
        public bool CodBarraValidadoEnPos { get => codBarraValidadoEnPos; set => codBarraValidadoEnPos = value; }
    }
}
