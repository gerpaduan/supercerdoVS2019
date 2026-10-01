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
        private bool ctaCteReservada;

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
        private string email;

        public string Telefono
        {
            get { return telefono; }
            set { telefono = value; }
        }

        public string Email
        {
            get { return email; }
            set { email = value; }
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

        // Cuenta corriente reservada: los movimientos de esta persona (ventas, compras, cobros,
        // pagos, cta cte) solo los ve un admin / quien tenga formCtasCtes; el resto ve unicamente
        // lo que cargo el mismo desde la apertura de su caja (docs/DECISIONS.md, 2026-10-01).
        public bool CtaCteReservada
        {
            get { return ctaCteReservada; }
            set { ctaCteReservada = value; }
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

        // Pedido explicito del usuario (2026-09-28, ver docs/DECISIONS.md): en las pantallas que
        // muestran la razon social de un cliente/proveedor, mostrar tambien la identificacion
        // (CUIT/DNI) debajo, pero solo cuando aporta un dato distinto (evita duplicar el mismo
        // texto dos veces, ej. cuando la identificacion quedo cargada igual a la razon social).
        public bool TieneIdentificacionDistinta()
        {
            return !string.IsNullOrWhiteSpace(identificacion)
                && !string.Equals(identificacion, razonSocial, StringComparison.OrdinalIgnoreCase);
        }
    }
}
