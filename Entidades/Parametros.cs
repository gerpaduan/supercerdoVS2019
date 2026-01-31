using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Entidades
{
    public static class Parametros
    {
        //public static float porcAjEfectivo;//	Porcentaje diferencia al precio de lista EFECTIVO
        //public static float porcAjDebito;//	Porcentaje diferencia al precio de lista DEBITO
        //public static float porcAjCredito;//	Porcentaje diferencia al precio de lista CREDITO
        //public static float porcAjCtaCte;//	Porcentaje diferencia al precio de lista CtaCte
        //public static float porcAjQr;//	Porcentaje diferencia al precio de lista con Qr
        //public static float porcAjTranf;//	Porcentaje diferencia al precio de lista Tranf
        //public static int idConsumidorFinal;//	6	id del cliento CONSUMIDOR FINAL en la Base de Datos
        //public static float limiteKgParaAjuste;//	6	Cantidad maxima limite de kgs a apartir de los cuales no se realiza el ajuste por tarjeta 
        //public static float comisionDebito;//6	comisionDebito	0.045	Comision que cobra MercadoPago en cobros con DEBITO
        //public static float comisionCredito;//7	comisionCredito	0.075	Comision que cobra MercadoPago en cobros con CREDITO
        //public static int idIndefinido;//	4	id del cliente INDEFINIDO en la Base de Datos
        //public static int minAccesoUltimaVentaVendedor;//	cantidad de minutos que pueden pasar para q Vendedor pueda modificar ultima venta en la Base de Datos
        //public static int idPagoTarjetaEgresoCaja;//idPagoTarjetaEgresoCaja Reservado para los Egresos de Caja
        //public static int idCtaCteEgresoCaja;//idCtaCteEgresoCaja  Reservado para los Egresos de Caja
        //public static int idCompraEgresoCaja;//idCompraEgresoCaja  Reservado para los Egresos de Caja
        //public static bool mayuscula;//	1 : Mayuscula Predefinida | 0: No
        //public static bool loginRapidoMovimiento;	//	1 : Si | 0: No
        //public static bool loginRapidoElaborado;	//	1 : Si | 0: No
        //public static bool loginRapidoStock;		// 1 : Si | 0: No
        //public static int diasLimitFechaDesde;//	Cuantos días hacia atrás se pueden consultar sin permisos los registros de Movimientos y Elaborados
        //public static int importeMaxRedondeo;//	Cuantos días hacia atrás se pueden consultar sin permisos los registros de Movimientos y Elaborados
        //public static int idPagoCobroEgresoCaja;//idPago (Recibi de / Pagar a..) registro en Cta.Cte Reservado para los Egresos de Caja
        //public static long codProdGenerico;//Codigo de Producto Generico para setear el precio por el usuario

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

    }
}
