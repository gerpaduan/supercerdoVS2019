using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Persona
    {
        public int idPersona;
        public string razonSocial;
        public string otrosDatos;
        public string tipo;
        private bool ctaCte;

        private int idIva;
        private int idEmpresa;
        private bool marca;
        private int? idPropietario;

        public int IdIva
        {
            get { return idIva; }
            set { idIva = value; }
        }

        private string iva;

        public string Iva
        {
            get { return iva; }
            set { iva = value; }
        }

        public int IdEmpresa
        {
            get { return idEmpresa; }
            set { idEmpresa = value; }
        }

        private string identificacion;

        public string Identificacion
        {
            get { return identificacion; }
            set { identificacion = value; }
        }

        private string cuit;

        public string Cuit
        {
            get { return cuit; }
            set { cuit = value; }
        }
        private string telefono;

        public string Telefono
        {
            get { return telefono; }
            set { telefono = value; }
        }
        private string domicilio;

        public string Domicilio
        {
            get { return domicilio; }
            set { domicilio = value; }
        }
        private string ciudad;

        public string Ciudad
        {
            get { return ciudad; }
            set { ciudad = value; }
        }

        public bool CtaCte
        {
            get { return ctaCte; }
            set { ctaCte = value; }
        }
        private float bonificacion;

        public float Bonificacion
        {
            get { return bonificacion; }
            set { bonificacion = value; }
        }

        public int IdPersona
        {
            get
            {
                return idPersona;
            }
            set
            {
                idPersona = value;
            }
        }

        public string RazonSocial
        {
            get
            {
                return razonSocial;
            }
            set
            {
                razonSocial = value;
            }
        }

        public string OtrosDatos
        {
            get
            {
                return otrosDatos;
            }
            set
            {
                otrosDatos = value;
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

        public DateTime Creado { get => creado; set => creado = value; }
        public bool Marca { get => marca; set => marca = value; }

        Persona propietario;
        public int? IdPropietario { get => idPropietario; set => idPropietario = value; }
        public Persona Propietario { get => propietario; set => propietario = value; }

        DateTime creado;


        public bool ConsumidorFinal { get; set; }

        public static int idIndefinido = 4;
        public static int idConsumidorFinal = 6;
        public static bool esConsumidorFinal(Entidades.Persona oPersona)
        {
            return (oPersona.idPersona == idConsumidorFinal);// Persona.idConsumidorFinal);
        }


        ///Cond.Iva:  1 - Consumidor Final / 2 - RRII / 3 - Monotributo / 4 - Exento
        ///
        public static int codIvaRRII_Afip = 2;
        public bool EsRRII(int idIvaPersona)
        {
            return (this.idIva == codIvaRRII_Afip);

        }
    }
}
