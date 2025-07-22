using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Cheque
    {
        public enum EstadoEnum
        {
            PENDIENTE,
            ENTREGADO,
            DEPOSITADO,
            ACREDITADO,
            RECHAZADO,
        }

        /// TODO: seguir con la creacion de la clase y el alta de cheque
        public int Id { get; set; }
        public string NroCheque { get; set; }
        public string Banco { get; set; }
        public bool Propio { get; set; }  // tinyint como booleano
        public string FechaEmision { get; set; }  // Podés usar DateTime si lo preferís
        public DateTime FechaPago { get; set; }
        public double Importe { get; set; }
        public string Estado { get; set; }
        public string Titular { get; set; }
        public string Observaciones { get; set; }
        public int RecibidoDe { get; set; }
        public int EntregadoA { get; set; }
        public Pago PagoDe { get; set; }
        public Pago PagoA { get; set; }
        public DateTime? Creado { get; set; }
        public Usuario CreadoPor { get; set; }
        public DateTime? Actualizado { get; set; }
        public Usuario ActualizadoPor { get; set; }

    }
}
