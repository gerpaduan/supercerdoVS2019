namespace Entidades
{
    // Los 5 casos diagnosticos que puede devolver Negocio.BarcodeInterpreter.Interpretar --
    // permite al caller (POS Ventas/Expendio) distinguir exactamente por que un codigo no se
    // resolvio, sin depender de parsear un mensaje de texto.
    public enum CasoInterpretacionBarcode
    {
        // Ni siquiera es candidato a codigo interno: no es EAN-13, checksum invalido, o
        // el prefijo no cae en el rango reservado 20-29. Camino de siempre: buscar el
        // codigo completo tal cual, sin cambios de comportamiento.
        CodigoInvalido = 1,

        // Prefijo 20-29 valido, pero esta empresa no tiene un FormatoCodigoBarras activo
        // para ese prefijo. Camino de siempre: buscar el codigo completo tal cual (no rompe
        // EAN-13 reales que por casualidad empiecen 20-29).
        PrefijoSinFormato = 2,

        // Hay un formato activo para el prefijo, pero el codigo real no coincide con su
        // longitud/posiciones configuradas -- no se puede parsear con confianza.
        EstructuraInvalida = 3,

        // El codigo se parseo correctamente (PLU + valor extraidos), pero no existe ningun
        // Corte con ese codigo interno en la empresa.
        ProductoNoEncontrado = 4,

        // Parseo e interpretacion completos: PLU, TipoValor, Valor y Producto resueltos.
        Interpretado = 5
    }
}
