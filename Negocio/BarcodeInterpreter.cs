using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Utilidades;

namespace Negocio
{
    // Motor centralizado de interpretacion de codigos de barra leidos en el POS (Ventas y
    // Expendio comparten esta misma clase -- ver Web/Controllers/VentasController.cs y
    // PuntosExpendioController.cs, metodos BuscarProducto/BuscarProductoPOS). Cubre 2
    // mecanismos, cada uno con su propio metodo publico:
    //
    // 1) Interpretar(): codigos internos de balanza, EAN-13 con prefijo 20-29, cuyo formato
    //    (posicion/longitud de PLU y de valor, por empresa) se configura en la pantalla
    //    "Codigos de barra" (ver Negocio.FormatoCodigoBarras). Si el prefijo no es 20-29, o no
    //    hay formato configurado para esta empresa, el caller debe caer al camino de siempre
    //    (buscar por el codigo completo) -- este metodo nunca reemplaza esa busqueda, solo la
    //    complementa cuando corresponde. Nunca tira excepcion por una estructura de formato
    //    invalida: todo caso se refleja en el "Caso" del resultado.
    //
    // 2) InterpretarCodigoGenerico(): migracion 1:1 (mismo regex, mismo umbral) del mecanismo
    //    "codigo generico" (sufijo "G<n>", o precio manual con punto decimal) que antes vivia
    //    duplicado en los dos controllers del POS.
    public class BarcodeInterpreter
    {
        public const int PrefijoMin = 20;
        public const int PrefijoMax = 29;
        private const int LongitudEanConPrefijoInterno = 13;
        private const int LongitudMinimaEan8 = 8;

        private static readonly Regex RegexSufijoGenerico = new Regex(@"^[^G]*G(\d+)[^G]*$");

        private readonly Contratos.IFormatoCodigoBarrasRepository oFormatoD;
        private readonly Contratos.ICorteBusquedaSimpleRepository oCorteD;

        // SQL Server: mismo patron que Negocio.Corte/Negocio.DispositivoSeguro.
        public BarcodeInterpreter(IEmpresaContext empresa, IParametrosContext param = null)
        {
            oFormatoD = new Datos.FormatoCodigoBarras(empresa);
            oCorteD = new Datos.Corte(empresa, param);
        }

        // Constructor nuevo, aditivo: inyecta cualquier implementacion de los 2 repositorios
        // (ej. DatosPostgres.FormatoCodigoBarrasPg + DatosPostgres.CortePg, o fakes de test).
        public BarcodeInterpreter(
            Contratos.IFormatoCodigoBarrasRepository formatoRepositorio,
            Contratos.ICorteBusquedaSimpleRepository corteRepositorio)
        {
            oFormatoD = formatoRepositorio ?? throw new ArgumentNullException(nameof(formatoRepositorio));
            oCorteD = corteRepositorio ?? throw new ArgumentNullException(nameof(corteRepositorio));
        }

        public Entidades.ResultadoInterpretacionBarcode Interpretar(string codigoDeBarras, int idEmpresa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codigoDeBarras)
                    || codigoDeBarras.Length != LongitudEanConPrefijoInterno
                    || !codigoDeBarras.All(char.IsDigit)
                    || !ValidacionEan.EsEan13Valido(codigoDeBarras))
                {
                    return CodigoInvalido();
                }

                int prefijo = int.Parse(codigoDeBarras.Substring(0, 2), CultureInfo.InvariantCulture);
                if (prefijo < PrefijoMin || prefijo > PrefijoMax)
                    return CodigoInvalido();

                var formato = oFormatoD.ObtenerActivoPorPrefijo(idEmpresa, prefijo);
                if (formato == null)
                    return PrefijoSinFormato(prefijo);

                if (formato.LongitudTotal != codigoDeBarras.Length
                    || !EntraEnRango(formato.PosicionCodigo, formato.LongitudCodigo, formato.LongitudTotal)
                    || !EntraEnRango(formato.PosicionValor, formato.LongitudValor, formato.LongitudTotal))
                {
                    return EstructuraInvalida(formato);
                }

                string strCodigo = codigoDeBarras.Substring(formato.PosicionCodigo - 1, formato.LongitudCodigo);
                string strValor = codigoDeBarras.Substring(formato.PosicionValor - 1, formato.LongitudValor);

                long codigoProductoInterno;
                long valorCrudo;
                if (!long.TryParse(strCodigo, NumberStyles.None, CultureInfo.InvariantCulture, out codigoProductoInterno)
                    || !long.TryParse(strValor, NumberStyles.None, CultureInfo.InvariantCulture, out valorCrudo))
                {
                    return EstructuraInvalida(formato);
                }

                decimal valor = valorCrudo / (decimal)Math.Pow(10, formato.CantidadDecimales);

                var producto = idEmpresa > 0
                    ? oCorteD.findCorteByCodigoEmpresa(codigoProductoInterno, idEmpresa, false)
                    : oCorteD.findCorteByCodigo(codigoProductoInterno, false);

                if (producto == null || (idEmpresa > 0 && producto.IdEmpresa != idEmpresa))
                    return ProductoNoEncontrado(codigoProductoInterno, formato);

                return new Entidades.ResultadoInterpretacionBarcode
                {
                    Caso = Entidades.CasoInterpretacionBarcode.Interpretado,
                    EsCodigoInterno = true,
                    FormatoEncontrado = true,
                    CodigoProductoInterno = codigoProductoInterno,
                    TipoValor = formato.TipoValor,
                    Valor = valor,
                    Producto = producto,
                    MensajeDiagnostico = ""
                };
            }
            catch
            {
                // Nunca debe romper el POS -- cualquier excepcion no prevista (parseos,
                // formato corrupto en la base, etc.) cae en "estructura invalida" generico.
                return new Entidades.ResultadoInterpretacionBarcode
                {
                    Caso = Entidades.CasoInterpretacionBarcode.EstructuraInvalida,
                    EsCodigoInterno = true,
                    FormatoEncontrado = true,
                    MensajeDiagnostico = "No se pudo interpretar el código de barras interno."
                };
            }
        }

        public Entidades.ResultadoCodigoGenerico InterpretarCodigoGenerico(string codigoNormalizado, bool ingresoCantidadX, long codigoBaseGenerico)
        {
            codigoNormalizado = codigoNormalizado ?? "";

            int cantidadPuntos = codigoNormalizado.Split('.').Length - 1;
            if (cantidadPuntos > 1)
                return new Entidades.ResultadoCodigoGenerico { FormatoInvalido = true };

            var match = RegexSufijoGenerico.Match(codigoNormalizado);
            long numeroSumaGen = match.Success ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : 0;

            bool esGenerico = ingresoCantidadX
                && (codigoNormalizado.Contains(".") || codigoNormalizado.Contains("G") || codigoNormalizado.Length < LongitudMinimaEan8);

            if (!esGenerico)
                return new Entidades.ResultadoCodigoGenerico { EsGenerico = false };

            long codigoProducto = codigoBaseGenerico + numeroSumaGen;

            // Precio cargado a mano: la parte antes de "G" (o el codigo completo si no hay
            // "G"), tal cual el mecanismo original -- puede tirar FormatException si no es un
            // numero valido, y eso se deja propagar igual que antes (el caller decide si lo
            // atrapa o no, mismo comportamiento que tenian los 2 controllers antes de migrar).
            int indexG = codigoNormalizado.IndexOf('G');
            string precioTexto = indexG != -1 ? codigoNormalizado.Substring(0, indexG) : codigoNormalizado;
            float precioManual = float.Parse(precioTexto, CultureInfo.InvariantCulture);

            return new Entidades.ResultadoCodigoGenerico
            {
                EsGenerico = true,
                CodigoProducto = codigoProducto,
                PrecioManual = precioManual
            };
        }

        private static bool EntraEnRango(int posicion, int longitud, int longitudTotal)
        {
            return posicion >= 1 && longitud >= 1 && (posicion + longitud - 1) <= longitudTotal;
        }

        private static Entidades.ResultadoInterpretacionBarcode CodigoInvalido()
        {
            return new Entidades.ResultadoInterpretacionBarcode
            {
                Caso = Entidades.CasoInterpretacionBarcode.CodigoInvalido,
                EsCodigoInterno = false,
                FormatoEncontrado = false,
                MensajeDiagnostico = "El código no es un EAN-13 válido de prefijo interno."
            };
        }

        private static Entidades.ResultadoInterpretacionBarcode PrefijoSinFormato(int prefijo)
        {
            return new Entidades.ResultadoInterpretacionBarcode
            {
                Caso = Entidades.CasoInterpretacionBarcode.PrefijoSinFormato,
                EsCodigoInterno = false,
                FormatoEncontrado = false,
                MensajeDiagnostico = $"No hay un formato configurado (o está inactivo) para el prefijo {prefijo} en esta empresa."
            };
        }

        private static Entidades.ResultadoInterpretacionBarcode EstructuraInvalida(Entidades.FormatoCodigoBarras formato)
        {
            return new Entidades.ResultadoInterpretacionBarcode
            {
                Caso = Entidades.CasoInterpretacionBarcode.EstructuraInvalida,
                EsCodigoInterno = true,
                FormatoEncontrado = true,
                MensajeDiagnostico = $"El código no coincide con la estructura del formato \"{formato.Nombre}\" configurado para el prefijo {formato.Prefijo}."
            };
        }

        private static Entidades.ResultadoInterpretacionBarcode ProductoNoEncontrado(long codigoProductoInterno, Entidades.FormatoCodigoBarras formato)
        {
            return new Entidades.ResultadoInterpretacionBarcode
            {
                Caso = Entidades.CasoInterpretacionBarcode.ProductoNoEncontrado,
                EsCodigoInterno = true,
                FormatoEncontrado = true,
                CodigoProductoInterno = codigoProductoInterno,
                TipoValor = formato.TipoValor,
                MensajeDiagnostico = $"No existe ningún producto con código interno {codigoProductoInterno} en esta empresa."
            };
        }
    }
}
