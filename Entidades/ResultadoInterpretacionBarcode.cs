namespace Entidades
{
    // Resultado estructurado de Negocio.BarcodeInterpreter.Interpretar. El caller (POS Ventas/
    // Expendio) solo consume este objeto -- nunca necesita saber como esta armado el codigo
    // de barras por dentro.
    public class ResultadoInterpretacionBarcode
    {
        public CasoInterpretacionBarcode Caso { get; set; }

        // true en los casos 3, 4 y 5 (el codigo SI es del rango interno 20-29, aunque no se
        // haya podido resolver del todo). false en los casos 1 y 2 -- ahi el caller debe
        // seguir el camino de siempre (buscar por codigo completo).
        public bool EsCodigoInterno { get; set; }

        // true en los casos 3, 4 y 5: existe un FormatoCodigoBarras activo para el prefijo.
        public bool FormatoEncontrado { get; set; }

        // PLU extraido del codigo, seteado en los casos 4 y 5.
        public long? CodigoProductoInterno { get; set; }

        public TipoValorCodigoBarras? TipoValor { get; set; }

        // Valor extraido y convertido segun CantidadDecimales del formato. Seteado en el
        // caso 5 (y en el 4, a titulo informativo aunque no haya producto).
        public decimal? Valor { get; set; }

        // Producto encontrado, seteado solo en el caso 5 (Interpretado).
        public Corte Producto { get; set; }

        // Nunca null -- para logging/UX. Vacio solo en el caso 5 (no hay nada que reportar).
        public string MensajeDiagnostico { get; set; }
    }
}
