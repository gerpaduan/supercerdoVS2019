using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Entidades
{
    public class Parametros
    {
        public static float	porcAjEfectivo;//	Porcentaje diferencia al precio de lista EFECTIVO
        public static float porcAjDebito;//	Porcentaje diferencia al precio de lista DEBITO
        public static float porcAjCredito;//	Porcentaje diferencia al precio de lista CREDITO
        public static float porcAjBilletera;//	Porcentaje diferencia al precio de lista Billetera
        public static float porcAjQr;//	Porcentaje diferencia al precio de lista con Qr
        public static float porcAjTranf;//	Porcentaje diferencia al precio de lista Tranf
        public static int	idConsumidorFinal;//	6	id del cliento CONSUMIDOR FINAL en la Base de Datos
        public static float	limiteKgParaAjuste;//	6	Cantidad maxima limite de kgs a apartir de los cuales no se realiza el ajuste por tarjeta 
        public static float	comisionDebito;//6	comisionDebito	0.045	Comision que cobra MercadoPago en cobros con DEBITO
        public static float comisionCredito;//7	comisionCredito	0.075	Comision que cobra MercadoPago en cobros con CREDITO
        public static float salChorizo;//	0,022	Cantidad de Sal por Kg en Chorizo
        public static float	pimientaChorizo;//	0,0017	Cantidad de Pimienta por Kg en Chorizo
        public static float	nuezChorizo;//	0,0007	Cantidad de Nuez por Kg en Chorizo
        public static float	bracolorChorizo;//	0,002	Cantidad de Bracolor por Kg en Chorizo
        public static float	salSalame;//	0,025	Cantidad de Sal por Kg en SALAME
        public static float	pimientaSalame;//	0,002	Cantidad de Pimienta por Kg en SALAME
        public static float	nuezSalame;//	0,0007	Cantidad de Nuez por Kg en SALAME
        public static float	productoSalame;//	0,0018	Cantidad de Producto por Kg en SALAME
        public static float	salSalchicha;//	0,022	Cantidad de Sal por Kg en SALCHICHA
        public static float	pimientaSalchicha;//	0,0017	Cantidad de Pimienta por Kg en SALCHICHA
        public static float	bracolorSalchicha;//	0,017	Cantidad de Bracolor por Kg en SALCHICHA
        public static float	pimentonSalchicha;//	0,001	Cantidad de Pimenton por Kg en SALCHICHA
        public static float	salQueso;//	0	Cantidad de Sal por Kg en QUESO
        public static float	pimientaQueso;//	0	Cantidad de Pimienta por Kg en QUESO
        public static float nuezQueso;
        public static float bracolorQueso;
        public static float	salMorcilla;//	0,022	Cantidad de Sal por Kg en MORCILLA
        public static float	pimientaMorcilla;//	0,0017	Cantidad de Pimienta por Kg en MORCILLA
        public static float	nuezMorcilla;//	0,0007	Cantidad de Nuez por Kg en MORCILLA
        public static float bracolorMorcilla;//	0,002	Cantidad de Bracolor por Kg en MORCILLA
        public static float salCodeguin;//	0,022	Cantidad de Sal por Kg en CODEGUIN
        public static float pimientaCodeguin;//	0,0017	Cantidad de Pimienta por Kg en CODEGUIN
        public static float nuezCodeguin;//	0,0007	Cantidad de Nuez por Kg en CODEGUIN
        public static float bracolorCodeguin;//	0,002	Cantidad de Bracolor por Kg en CODEGUIN
        public static float salMilanesa; // 35		0,022	Cantidad de Sal por Kg en MILANESA
        public static float pimientaMilanesa;//36	0,0017	Cantidad de Pimienta por Kg en MILANESA
        public static float porcPanRayadoMilanesa;//	0,2	porcentaje de pan rayado en milanesa
        public static float porcMermaEnPote;//	0,3	porcentaje Merma porque pones son 700 gramos
        public static float porcGrasaEnPote;//	0,7	porcentaje de Grasa en Pote
        public static float porcGrasaLiquida;//	0,75	porcenta de Grasa Liquida que se obtiene al cocinar
    }
}
