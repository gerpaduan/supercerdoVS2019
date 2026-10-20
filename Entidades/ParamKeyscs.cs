using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public static class ParamKeys
    {
        public const string PorcAjEfectivo = "porcAjEfectivo";
        public const string PorcAjDebito = "porcAjDebito";
        public const string PorcAjCredito = "porcAjCredito";
        public const string PorcAjCtaCte = "porcAjCtaCte";
        public const string PorcAjQr = "porcAjQr";
        public const string PorcAjTranf = "porcAjTranf";

        public const string IdConsumidorFinal = "idConsumidorFinal";
        public const string LimiteKgParaAjuste = "limiteKgParaAjuste";
        public const string ComisionDebito = "comisionDebito";
        public const string ComisionCredito = "comisionCredito";
        public const string IdIndefinido = "idIndefinido";
        public const string MinAccesoUltimaVentaVendedor = "minAccesoUltimaVentaVendedor";
        public const string IdPagoTarjetaEgresoCaja = "idPagoTarjetaEgresoCaja";
        public const string IdCtaCteEgresoCaja = "idCtaCteEgresoCaja";
        public const string IdCompraEgresoCaja = "idCompraEgresoCaja";

        public const string Mayuscula = "mayuscula";
        public const string LoginRapidoMovimiento = "loginRapidoMovimiento";
        public const string LoginRapidoElaborado = "loginRapidoElaborado";
        public const string LoginRapidoStock = "loginRapidoStock";

        public const string DiasLimitFechaDesde = "diasLimitFechaDesde";
        public const string ImporteMaxRedondeo = "importeMaxRedondeo";
        public const string IdPagoCobroEgresoCaja = "idPagoCobroEgresoCaja";
        public const string CodProdGenerico = "codProdGenerico";

        // Saldo de cuenta corriente (en pesos) hasta el cual el POS no lo muestra ni lo ofrece en el
        // ticket (2026-10-19). Ver Negocio.CalculoSaldoCliente.
        public const string SaldoMinimoInformar = "saldoMinimoInformar";

        // Sí/No (2026-10-19): con Sí el ticket termico de venta NO fiscal (X) no imprime el nombre ni los
        // slogans de la empresa. Sin valor = No (se imprimen, como siempre). El ticket facturado no cambia.
        // Ver VentasController.OcultarEncabezadoTicketNoFiscal.
        public const string OcultarEncabezadoTicketNoFiscal = "ocultarEncabezadoTicketNoFiscal";
    }

}
