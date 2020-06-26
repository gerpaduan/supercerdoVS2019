using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class FacturaElectronica
    {
        /// <summary>
        /// Pasando en (String) Codigo TipoComprobante se retorna bool si es Factura A (Codigo Afip 001)
        /// </summary>
        /// <returns></returns>
        public bool esFacturaA(string codTipoCbte)
        {
            if (string.IsNullOrEmpty(codTipoCbte))
                return false;

            int codTipoCbte_int = Convert.ToInt32(codTipoCbte);
            bool esFacturaA_ = (codTipoCbte_int == 1);
            return esFacturaA_;
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
    }
}
