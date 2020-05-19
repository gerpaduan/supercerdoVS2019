using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Entidades
{
    public class Parametros
    {
        public static float	porcAjEfectivo;//	0.01	porcentaje de ajuste que se agrega en kgs a quienes pagan en EFECTIVO
        public static float	porcAjDebito;//	0.01	porcentaje de ajuste que se agrega en kgs a quienes pagan con DEBITO
        public static float	porcAjCredito;//	0.01	porcentaje de ajuste que se agrega en kgs a quienes pagan con CREDITO
        public static int	idConsumidorFinal;//	6	id del cliento CONSUMIDOR FINAL en la Base de Datos
        public static float	limiteKgParaAjuste;//	6	Cantidad maxima limite de kgs a apartir de los cuales no se realiza el ajuste por tarjeta 
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
    }
}
