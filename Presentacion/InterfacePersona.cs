using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Presentacion
{
    public interface InterfacePersona
    {
        void EnviarPersona(Entidades.Persona persona);
    }

    public interface InterfaceFormaPago
    {
        void EnviarFormaPago(Entidades.Venta.formaPagoEnum formaPago);
    }

    public interface InterfaceTipoComprobante
    {
        void EnviarTipoComprobante(Entidades.Venta.tipoComprobanteEnum tipoComprobante);
    }

    public interface InterfaceImprimirCbte
    {
        void EnviarImprimirCbte(Entidades.Venta.imprimirCbteEnum imprimirTipoCbte);
    }
}
