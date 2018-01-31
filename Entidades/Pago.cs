using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Pago
    {
        public enum formasPago
        {
            Efectivo,
            Cheque,
            EftvoCheque,
            Transferencia,
            Otro,
        }

        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        Persona persona;

        public Persona Persona
        {
            get { return persona; }
            set { persona = value; }
        }
        private string nroRecibo;

        public string NroRecibo
        {
            get { return nroRecibo; }
            set { nroRecibo = value; }
        }
        private DateTime fecha;

        public DateTime Fecha
        {
            get { return fecha; }
            set { fecha = value; }
        }
        private bool aProveedor;

        public bool AProveedor
        {
            get { return aProveedor; }
            set { aProveedor = value; }
        }
        private string formaPago;

        public string FormaPago
        {
            get { return formaPago; }
            set { formaPago = value; }
        }

        private string banco;

        public string Banco
        {
            get { return banco; }
            set { banco = value; }
        }
        private string nroCheque;

        public string NroCheque
        {
            get { return nroCheque; }
            set { nroCheque = value; }
        }
        private string titularCheque;

        public string TitularCheque
        {
            get { return titularCheque; }
            set { titularCheque = value; }
        }
        private float importe;

        public float Importe
        {
            get { return importe; }
            set { importe = value; }
        }
        private string observaciones;

        public string Observaciones
        {
            get { return observaciones; }
            set { observaciones = value; }
        }
        Sucursal sucursal;

        public Sucursal Sucursal
        {
            get { return sucursal; }
            set { sucursal = value; }
        }
        private DateTime? creado;

        public DateTime? Creado
        {
            get { return creado; }
            set { creado = value; }
        }
        private DateTime? actualizado;

        public DateTime? Actualizado
        {
            get { return actualizado; }
            set { actualizado = value; }
        }
        private Usuario creadoPor;

        public Usuario CreadoPor
        {
            get { return creadoPor; }
            set { creadoPor = value; }
        }
        private Usuario actualizadoPor;

        public Usuario ActualizadoPor
        {
            get { return actualizadoPor; }
            set { actualizadoPor = value; }
        }        
        
    }
}
