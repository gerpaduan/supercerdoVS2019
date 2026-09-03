namespace Entidades
{
    // Que representa el campo "Valor" extraido de un codigo de barras interno (balanza,
    // prefijo EAN 20-29): un precio ya calculado, o una cantidad/peso a multiplicar por el
    // precio por kg del producto. Ver Negocio.BarcodeInterpreter.
    public enum TipoValorCodigoBarras
    {
        Precio,
        Cantidad
    }
}
