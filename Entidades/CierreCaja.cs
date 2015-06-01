using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class CierreCaja
    {
        public enum tipoBusqueda {FindLast, FindAll, FindById, FindOpen};
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

        public float? Gastos
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
    }
}
