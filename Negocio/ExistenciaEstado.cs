// Estado de stock de UNA celda (producto x sucursal) de "Existencia por sucursales", calculado a partir del stock actual
// y del punto de stock REAL de esa sucursal. Funcion pura, con tests: Negocio.Tests/ExistenciaEstadoTests.cs.
//
// Por que existe: el campo EstadoStock que devuelve la consulta de existencia (SQL) se calcula contra el punto de stock
// GLOBAL legacy de `corte.puntostock`, no contra el punto por sucursal (Negocio.Corte.ObtenerExistenciaPlano pisa PuntoStock
// con el real pero no recalcula el estado). La pantalla muestra y ordena por el punto real, asi que el estado se calcula aca
// con la misma regla que la consulta pero con ese punto. La regla se repite en el JS de la pantalla (existencia-estado en
// ExistenciaPorSucursales.cshtml) para recalcular sin volver a buscar cuando el usuario edita un punto: si se cambia
// aca, cambiarla alla tambien. Decision: docs/DECISIONS.md 2026-10-19.
//
// Sintaxis a proposito conservadora (C# 7.3): Negocio tambien compila para net472.
using System;

namespace Negocio
{
    public sealed class ExistenciaEstado
    {
        // Rangos para ordenar "peores primero": 0 Negativo, 1 Falta / En el punto, 2 OK, 3 Sin punto / Sin stock.
        public const int RangoNegativo = 0;
        public const int RangoBajoElPunto = 1;
        public const int RangoOk = 2;
        public const int RangoSinPunto = 3;

        // Codigos estables (los usa el JS y los atributos data- de la tabla).
        public const string Negativo = "NEGATIVO";
        public const string Falta = "FALTA";
        public const string EnElPunto = "EN_PUNTO";
        public const string Ok = "OK";
        public const string SinStock = "SIN_STOCK";
        public const string SinPunto = "SIN_PUNTO";

        public int Rango { get; private set; }
        public string Codigo { get; private set; }
        // Cuanto falta para llegar al punto (solo con Codigo == Falta); 0 en los demas casos.
        public double Faltante { get; private set; }
        public double Stock { get; private set; }
        public double Punto { get; private set; }

        // Clave numerica para ordenar la columna Estado: primero el rango, dentro del rango el stock menos el punto
        // (asi el mayor faltante queda arriba). 1e6 separa los rangos: el stock de un producto nunca se acerca a ese valor.
        public double ClaveOrden
        {
            get { return Rango * 1000000d + (Stock - Punto); }
        }

        // Clase para el filtro de Estado de la barra: "falta" (Negativo, Falta, En el punto), "bien" (OK) o "" (sin punto / sin stock).
        public string ClaseFiltro
        {
            get { return Rango <= RangoBajoElPunto ? "falta" : (Rango == RangoOk ? "bien" : ""); }
        }

        // "Punto de stock 0" del filtro: no tiene punto cargado, sin importar el stock.
        public bool SinPuntoCargado
        {
            get { return Punto <= 0; }
        }

        private ExistenciaEstado() { }

        public static ExistenciaEstado Calcular(double stock, double punto)
        {
            var estado = new ExistenciaEstado { Stock = stock, Punto = punto };

            if (stock < 0)
            {
                estado.Rango = RangoNegativo;
                estado.Codigo = Negativo;
            }
            else if (punto > 0 && stock <= punto)
            {
                estado.Rango = RangoBajoElPunto;
                if (stock == punto)
                {
                    estado.Codigo = EnElPunto;
                }
                else
                {
                    estado.Codigo = Falta;
                    estado.Faltante = punto - stock;
                }
            }
            else if (punto > 0)
            {
                estado.Rango = RangoOk;
                estado.Codigo = Ok;
            }
            else
            {
                estado.Rango = RangoSinPunto;
                estado.Codigo = stock == 0 ? SinStock : SinPunto;
            }

            return estado;
        }
    }
}
