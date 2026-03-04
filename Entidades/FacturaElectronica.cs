using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Entidades
{
    public class FacturaElectronica
        {//1: Factura A
         //2: Nota de Débito A
         //3: Nota de Crédito A
         //4: Recibo A
         //6: Factura B
         //7: Nota de Débito B
         //8: Nota de Crédito B
         //9: Recibo B
         //11: Factura C
         //12: Nota de Débito C
         //13: Nota de Crédito C
         //15: Recibo C

         //Tipos Comprobantes
        public const int codFacturaA_Afip = 1;
        public const int codNotaCreditoA_Afip = 3;
        public const int codFacturaB_Afip = 6;
        public const int codNotaCreditoB_Afip = 8;
        public const int codFacturaC_Afip = 11;
        public const int codNotaCreditoC_Afip = 13;

        ///Condicion frente al IVA 
        //id  iva                     abrev
        //1   Consumidor Final        Cons.Final
        //2   Responsable Inscripto   Resp.Incr.
        //3   Monotributista          Monotr.
        //4   Exento                  Exento
        public const int codCF_IvaAfip = 1;
        public const int codRRII_IvaAfip = 2;
        public const int codMonotributo_IvaAfip = 3;
        public const int codExento_IvaAfip = 4;


        //Concepto
        public const int codConceptoProductos_Afip = 1;


        #region CONDICION IVA RECEPTOR - CODIGOS AFIP
        //1 = IVA Responsable Inscripto
        //4 = IVA Sujeto Exento
        //5 = Consumidor Final
        //6 = Responsable Monotributo
        #endregion

        public const int codCondicionIva_RRII = 1;
        public const int codCondicionIva_Exento = 4;
        public const int codCondicionIva_ConsFinal = 5;
        public const int codCondicionIva_Monotributo = 6;


        public int MapearCondicionIVAReceptorIdAfip(int idLocalIva)
        {
            // Reemplazo la expresión switch por una instrucción switch tradicional para compatibilidad con C# 7.3
            switch (idLocalIva)
            {
                case 1: // Consumidor Final
                    return codCondicionIva_ConsFinal;
                case 2: // Responsable Inscripto
                    return codCondicionIva_RRII;
                case 3: // Monotributista
                    return codCondicionIva_Monotributo;
                case 4: // Exento
                    return codCondicionIva_Exento;
                default:
                    // Manejo de casos no contemplados para evitar CS8509
                    return codCondicionIva_ConsFinal; // Valor por defecto (puede ajustarse según necesidades)
            }
        }

        //Los “ID” (códigos) de Tipo de documento que usa AFIP en WSFE son estos(los más comunes):

        //    80 = CUIT
        //    86 = CUIL
        //    87 = CDI
        //    96 = DNI

        public const int codTipoDoc_CUIT= 80;
        public const int codTipoDoc_DNI = 96;
        public const int codTipoDoc_SinIdentif = 99;


        /// <summary>
        /// Pasando en (String) Codigo TipoComprobante se retorna bool si es Factura A (Codigo Afip 001)
        /// </summary>
        /// <returns></returns>
        public bool esFacturaA(string codTipoCbte)
        {
            if (string.IsNullOrEmpty(codTipoCbte))
                return false;

            int codTipoCbte_int = Convert.ToInt32(codTipoCbte);
            bool esFacturaA_ = (codTipoCbte_int == codFacturaA_Afip);
            return esFacturaA_;
        }

        /// <summary>
        /// Pasando el codigo de tipo de comprobante se pasa letra de identificacion del mismo (A, B, X)
        /// </summary>
        public char getLetraId_TipoCbte(int codTipoCbteAFIP)
        { 
            char letraId_TipoCbte = 'X';//inicia en Remito
            switch (codTipoCbteAFIP)
	        {
                case codFacturaA_Afip:
                    letraId_TipoCbte = 'A';
                    break;
                case codNotaCreditoA_Afip:
                    letraId_TipoCbte = 'A';
                    break;
                case codFacturaB_Afip:
                    letraId_TipoCbte = 'B';
                    break;
                case codNotaCreditoB_Afip:
                    letraId_TipoCbte = 'B';
                    break;
                case codFacturaC_Afip:
                    letraId_TipoCbte = 'C';
                    break;
                case codNotaCreditoC_Afip:
                    letraId_TipoCbte = 'C';
                    break;
            }
            return letraId_TipoCbte;
        }

        public int getCodTipoCbteAFIP_segunLetraFactura(string TipoCbteAFIP, bool esRRII, bool esNotaCredito)
        {
            int codTipoCbteAFIP=0;
            switch (TipoCbteAFIP)
            {
                case "A":
                    codTipoCbteAFIP = esNotaCredito ? codNotaCreditoA_Afip : codFacturaA_Afip;
                    break;
                case "B":
                    codTipoCbteAFIP = esNotaCredito ? codNotaCreditoB_Afip : codFacturaB_Afip;
                    break;
                case "C":
                     codTipoCbteAFIP = esNotaCredito ? codNotaCreditoC_Afip : codFacturaC_Afip;
                    break;
                default:
                    codTipoCbteAFIP = esRRII ? codFacturaB_Afip : codFacturaC_Afip;
                    break ;
            }
            return codTipoCbteAFIP;
        }

        public int getCodTipoCbteAFIP(bool esRRII_Empresa, bool esRRII_Cliente, bool esNotaCredito)
        {
            if (esRRII_Empresa)
                return esRRII_Cliente ? (esNotaCredito ? codNotaCreditoA_Afip : codFacturaA_Afip) : (esNotaCredito ? codNotaCreditoB_Afip : codFacturaB_Afip);
            else
                 return esNotaCredito ? codNotaCreditoC_Afip : codFacturaC_Afip;
        }

        int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        string ptoVtaAfip;

        public string PtoVtaAfip
        {
            get { return ptoVtaAfip; }
            set { ptoVtaAfip = value; }
        }

        string descTipoCbteAfip;

        public string DescTipoCbteAfip
        {
            get { return descTipoCbteAfip; }
            set { descTipoCbteAfip = value; }
        }

        int codTipoCbteAfip;

        public int CodTipoCbteAfip
        {
            get { return codTipoCbteAfip; }
            set { codTipoCbteAfip = value; }
        }

        string nroCbteAfip;

        public string NroCbteAfip
        {
            get { return nroCbteAfip; }
            set { nroCbteAfip = value; }
        }
        DateTime? fechaEmisionAfip;

        public DateTime? FechaEmisionAfip
        {
            get { return fechaEmisionAfip; }
            set { fechaEmisionAfip = value; }
        }
        string tipoDocAfip;

        public string TipoDocAfip
        {
            get { return tipoDocAfip; }
            set { tipoDocAfip = value; }
        }
        string nroDocAfip;

        public string NroDocAfip
        {
            get { return nroDocAfip; }
            set { nroDocAfip = value; }
        }
        string razonSocialAFIP;

        public string RazonSocialAFIP
        {
            get { return razonSocialAFIP; }
            set { razonSocialAFIP = value; }
        }
        string condicionIvaAFIP;

        public string CondicionIvaAFIP
        {
            get { return condicionIvaAFIP; }
            set { condicionIvaAFIP = value; }
        }
        string domicilioAFIP;

        public string DomicilioAFIP
        {
            get { return domicilioAFIP; }
            set { domicilioAFIP = value; }
        }
        string condicionVenta;//Contado-Cta.Cte

        public string CondicionVenta
        {
            get { return condicionVenta; }
            set { condicionVenta = value; }
        }
        string formaPago;

        public string FormaPago
        {
            get { return formaPago; }
            set { formaPago = value; }
        }
        string CAE;

        public string CAE1
        {
            get { return CAE; }
            set { CAE = value; }
        }
        string fecVtoCAE;

        public string FecVtoCAE
        {
            get { return fecVtoCAE; }
            set { fecVtoCAE = value; }
        }
        float importeNetoGravado;

        public float ImporteNetoGravado
        {
            get { return importeNetoGravado; }
            set { importeNetoGravado = value; }
        }
        float iva;

        public float Iva
        {
            get { return iva; }
            set { iva = value; }
        }
        float importeTotal;

        public float ImporteTotal
        {
            get { return importeTotal; }
            set { importeTotal = value; }
        }
        int idVenta;

        public int IdVenta
        {
            get { return idVenta; }
            set { idVenta = value; }
        }
        DateTime creado;

        public DateTime Creado
        {
            get { return creado; }
            set { creado = value; }
        }

        bool error;

        public bool Error
        {
            get { return error; }
            set { error = value; }
        }

        string mensajeError;

        public string MensajeError
        {
            get { return mensajeError; }
            set { mensajeError = value; }
        }

        DateTime? fechaError;

        public DateTime? FechaError
        {
            get { return fechaError; }
            set { fechaError = value; }
        }

        Venta venta;

        public Venta Venta
        {
            get { return venta; }
            set { venta = value; }
        }

        float porcentajeFacturacion;

        string descItemUnitario;
        public float PorcentajeFacturacion { get => porcentajeFacturacion; set => porcentajeFacturacion = value; }
        public string DescItemUnitario { get => descItemUnitario; set => descItemUnitario = value; }

        private List<Entidades.AlicuotaIva> listaAlicuota = new List<Entidades.AlicuotaIva>();

        public List<AlicuotaIva> ListaAlicuota
        {
            get => listaAlicuota;
            set => listaAlicuota = value ?? new List<AlicuotaIva>(); // opcional: evita que te la seteen en null
        }
    }
}
