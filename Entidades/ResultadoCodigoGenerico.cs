namespace Entidades
{
    // Resultado de Negocio.BarcodeInterpreter.InterpretarCodigoGenerico -- migracion 1:1 del
    // mecanismo "codigo generico" (sufijo G<n>, o precio manual con punto decimal) que antes
    // vivia duplicado en VentasController.BuscarProducto y PuntosExpendioController.
    // BuscarProductoPOS.
    public class ResultadoCodigoGenerico
    {
        // true si el codigo (normalizado, coma->punto) tiene mas de un punto -- ni siquiera
        // llega a evaluarse como generico o no. Mismo mensaje que ambos controllers ya
        // devolvian antes de la migracion ("Formato de código inválido").
        public bool FormatoInvalido { get; set; }

        public bool EsGenerico { get; set; }

        // Codigo de Corte a buscar (parametro CodProdGenerico + el numero sumado del sufijo
        // "G<n>"). Solo valido si EsGenerico == true.
        public long CodigoProducto { get; set; }

        // Precio cargado a mano (parte del codigo antes del "."), si el usuario lo tipeo asi.
        public float? PrecioManual { get; set; }
    }
}
