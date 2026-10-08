using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class CierreCaja
    {
        public enum tipoBusqueda {FindLast, FindAll, FindById, FindOpen, FindLastOpen};
        int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        Sucursal sucursal;
        public Sucursal Sucursal
        {
            get { return sucursal; }
            set { sucursal = value; }
        }
        DateTime? fechaHoraInicio;

        public DateTime? FechaHoraInicio
        {
            get { return fechaHoraInicio; }
            set { fechaHoraInicio = value; }
        }
        DateTime? fechaHoraCierre;

        public DateTime? FechaHoraCierre
        {
            get { return fechaHoraCierre; }
            set { fechaHoraCierre = value; }
        }
        float? cajaInicio;

        public float? CajaInicio
        {
            get { return cajaInicio; }
            set { cajaInicio = value; }
        }
        float? ventas;

        public float? Ventas
        {
            get { return ventas; }
            set { ventas = value; }
        }
        float? gastos;

        public float? EgresosCaja
        {
            get { return gastos; }
            set { gastos = value; }
        }
        float? saldoCaja;

        public float? SaldoCaja
        {
            get { return saldoCaja; }
            set { saldoCaja = value; }
        }
        float? cajaCierre;

        public float? CajaCierre
        {
            get { return cajaCierre; }
            set { cajaCierre = value; }
        }
        float? diferencia;

        public float? Diferencia
        {
            get { return diferencia; }
            set { diferencia = value; }
        }
        float? cajaInicioSiguiente;

        public float? CajaInicioSiguiente
        {
            get { return cajaInicioSiguiente; }
            set { cajaInicioSiguiente = value; }
        }
        float? importeRetirado;

        public float? ImporteRetirado
        {
            get { return importeRetirado; }
            set { importeRetirado = value; }
        }


        // Conteo de cierre que declaro el cajero antes de entregar la caja (pre-cierre). NULL = no cargo.
        // El importe oficial sigue siendo CajaCierre (lo confirma el encargado); ver docs/DECISIONS.md 2026-10-06.
        public float? CajaCierreCajero { get; set; }
        // Detalle multilinea del contador de billetes del cajero.
        public string ConteoBilletesCajero { get; set; }
        public DateTime? FechaConteoCajero { get; set; }

        Entidades.Usuario usuarioInicio;

        public Entidades.Usuario UsuarioInicio
        {
            get { return usuarioInicio; }
            set { usuarioInicio = value; }
        }
        Entidades.Usuario usuarioCierre;

        public Entidades.Usuario UsuarioCierre
        {
            get { return usuarioCierre; }
            set { usuarioCierre = value; }
        }
        DateTime? creado;

        public DateTime? Creado
        {
            get { return creado; }
            set { creado = value; }
        }
        DateTime? actualizado;

        public DateTime? Actualizado
        {
            get { return actualizado; }
            set { actualizado = value; }
        }
        public class ResultadoOperacion
        {
            public bool Ok { get; set; }
            public string Mensaje { get; set; }
        }
    }
}
