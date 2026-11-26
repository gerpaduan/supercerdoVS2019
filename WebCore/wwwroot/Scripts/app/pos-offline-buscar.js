/*
 * POS sin conexion: busqueda de productos contra el catalogo guardado en la PC (IndexedDB), con el MISMO resultado
 * que el servidor. Funciones puras, sin DOM ni red: las usa pos-offline-*.js y se prueban con Node
 * (WebCore.Tests/js/pos-offline-buscar.test.js) contra un fixture que genera el C# real
 * (WebCore.Tests/PosOfflineBarcodeParidadTests.cs).
 *
 * Replica:
 *  - VentasController.BuscarProducto: orden codigo generico -> codigo interno de balanza -> codigo completo, y la forma
 *    del JSON que devuelve ({ id, nombre, precioKg, precioOriginal, codigo, pesable, cantidadSugerida } o
 *    { success:false, message } / { error }).
 *  - Negocio.BarcodeInterpreter.Interpretar (EAN-13 prefijo 20-29 con el formato configurado de la empresa) e
 *    InterpretarCodigoGenerico (sufijo G<n> o precio manual con punto).
 *  - ProductosController.ListarProductos para la busqueda por texto del F10.
 * Si cambia alguno de esos en C#, el test de paridad falla hasta regenerar el fixture y adaptar este archivo.
 */
(function (root) {
    'use strict';

    var PREFIJO_MIN = 20;
    var PREFIJO_MAX = 29;
    var LONGITUD_EAN13 = 13;
    var LONGITUD_MINIMA_EAN8 = 8;
    var MAX_RESULTADOS_TEXTO = 200;
    var REGEX_SUFIJO_GENERICO = /^[^G]*G(\d+)[^G]*$/;

    // Math.Round(double, 2) de .NET: redondeo al par en el punto medio exacto.
    function redondear2(valor) {
        var escalado = valor * 100;
        var piso = Math.floor(escalado);
        var diferencia = escalado - piso;
        var resultado;
        if (diferencia > 0.5) resultado = piso + 1;
        else if (diferencia < 0.5) resultado = piso;
        else resultado = (piso % 2 === 0) ? piso : piso + 1;
        return resultado / 100;
    }

    // El precio del producto en C# es float: un valor asignado (etiqueta o precio manual) pierde precision igual que alla.
    function comoFloat(valor) {
        return Math.fround(valor);
    }

    function soloDigitos(texto) {
        return /^[0-9]+$/.test(texto);
    }

    // Utilidades.ValidacionEan.EsEan13Valido.
    function esEan13Valido(codigo) {
        if (typeof codigo !== 'string' || codigo.length !== LONGITUD_EAN13 || !soloDigitos(codigo)) return false;
        var suma = 0;
        for (var i = 0; i < 12; i++) {
            var digito = codigo.charCodeAt(i) - 48;
            suma += (i % 2 === 0) ? digito : digito * 3;
        }
        var verificador = (10 - (suma % 10)) % 10;
        return verificador === (codigo.charCodeAt(12) - 48);
    }

    // Utilidades.ValidacionEan.EsEan8Valido.
    function esEan8Valido(codigo) {
        if (typeof codigo !== 'string' || codigo.length !== 8 || !soloDigitos(codigo)) return false;
        var suma = 0;
        for (var i = 0; i < 7; i++) {
            var digito = codigo.charCodeAt(i) - 48;
            suma += (i % 2 === 0) ? digito * 3 : digito;
        }
        return (10 - (suma % 10)) % 10 === (codigo.charCodeAt(7) - 48);
    }

    // Indice codigo -> producto. Con codigos repetidos gana el primero (como el SELECT del servidor, sin ORDER BY).
    function indexar(catalogo) {
        if (catalogo._porCodigo) return catalogo._porCodigo;
        var mapa = new Map();
        (catalogo.productos || []).forEach(function (p) {
            if (p && !mapa.has(String(p.codigo))) mapa.set(String(p.codigo), p);
        });
        Object.defineProperty(catalogo, '_porCodigo', { value: mapa, enumerable: false });
        return mapa;
    }

    function buscarPorCodigo(catalogo, codigo) {
        return indexar(catalogo).get(String(codigo)) || null;
    }

    // Negocio.BarcodeInterpreter.InterpretarCodigoGenerico.
    function interpretarCodigoGenerico(codigo, ingresoCantidadX, codigoBaseGenerico) {
        codigo = codigo || '';
        if (codigo.split('.').length - 1 > 1) return { formatoInvalido: true };

        var match = REGEX_SUFIJO_GENERICO.exec(codigo);
        var numeroSumaGen = match ? parseInt(match[1], 10) : 0;

        var esGenerico = !!ingresoCantidadX
            && (codigo.indexOf('.') >= 0 || codigo.indexOf('G') >= 0 || codigo.length < LONGITUD_MINIMA_EAN8);
        if (!esGenerico) return { esGenerico: false };

        var indexG = codigo.indexOf('G');
        var precioTexto = indexG !== -1 ? codigo.substring(0, indexG) : codigo;
        // float.Parse(InvariantCulture): un texto que no es numero tira excepcion (aca: precioInvalido).
        var precio = /^[+-]?(\d+\.?\d*|\.\d+)$/.test(precioTexto) ? Number(precioTexto) : NaN;
        return {
            esGenerico: true,
            codigoProducto: Number(codigoBaseGenerico || 0) + numeroSumaGen,
            precioManual: precio,
            precioInvalido: isNaN(precio)
        };
    }

    function entraEnRango(posicion, longitud, total) {
        return posicion >= 1 && longitud >= 1 && (posicion + longitud - 1) <= total;
    }

    function formatoActivo(catalogo, prefijo) {
        var lista = catalogo.formatos || [];
        for (var i = 0; i < lista.length; i++) {
            if (lista[i] && Number(lista[i].prefijo) === prefijo) return lista[i];
        }
        return null;
    }

    // Negocio.BarcodeInterpreter.Interpretar. caso: 'codigoInvalido' | 'prefijoSinFormato' | 'estructuraInvalida' |
    // 'productoNoEncontrado' | 'interpretado'.
    function interpretar(codigo, catalogo) {
        if (!esEan13Valido(codigo)) return { caso: 'codigoInvalido', esCodigoInterno: false };

        var prefijo = parseInt(codigo.substring(0, 2), 10);
        if (prefijo < PREFIJO_MIN || prefijo > PREFIJO_MAX) return { caso: 'codigoInvalido', esCodigoInterno: false };

        var formato = formatoActivo(catalogo, prefijo);
        if (!formato) return { caso: 'prefijoSinFormato', esCodigoInterno: false };

        var mensajeEstructura = 'El código no coincide con la estructura del formato "' + (formato.nombre || '') +
            '" configurado para el prefijo ' + formato.prefijo + '.';
        if (Number(formato.longitudTotal) !== codigo.length
            || !entraEnRango(formato.posicionCodigo, formato.longitudCodigo, formato.longitudTotal)
            || !entraEnRango(formato.posicionValor, formato.longitudValor, formato.longitudTotal)) {
            return { caso: 'estructuraInvalida', esCodigoInterno: true, mensaje: mensajeEstructura };
        }

        var strCodigo = codigo.substr(formato.posicionCodigo - 1, formato.longitudCodigo);
        var strValor = codigo.substr(formato.posicionValor - 1, formato.longitudValor);
        if (!soloDigitos(strCodigo) || !soloDigitos(strValor)) {
            return { caso: 'estructuraInvalida', esCodigoInterno: true, mensaje: mensajeEstructura };
        }

        var codigoProducto = parseInt(strCodigo, 10);
        var valor = parseInt(strValor, 10) / Math.pow(10, Number(formato.cantidadDecimales) || 0);
        var producto = buscarPorCodigo(catalogo, codigoProducto);
        if (!producto) return { caso: 'productoNoEncontrado', esCodigoInterno: true, codigoProductoInterno: codigoProducto };

        return {
            caso: 'interpretado',
            esCodigoInterno: true,
            codigoProductoInterno: codigoProducto,
            tipoValor: formato.tipoValor,
            valor: valor,
            producto: producto
        };
    }

    function respuestaProducto(producto, precio, cantidadSugerida, origenPrecio) {
        return {
            id: producto.id,
            nombre: producto.nombre,
            precioKg: precio,
            precioOriginal: precio,
            codigo: producto.codigo,
            pesable: !!producto.pesable,
            cantidadSugerida: cantidadSugerida === undefined ? null : cantidadSugerida,
            // Solo offline: de donde salio el precio (el servidor lo usa para no marcar diferencias falsas).
            origenPrecio: origenPrecio
        };
    }

    // VentasController.BuscarProducto contra el catalogo local.
    function buscarProducto(codigo, ingresoCantidadX, catalogo) {
        if (!catalogo) return { success: false, message: 'No hay catálogo guardado en esta PC.' };
        if (codigo === null || codigo === undefined || String(codigo).trim() === '') return { error: 'Código vacío' };

        codigo = String(codigo).replace(/,/g, '.');

        var generico = interpretarCodigoGenerico(codigo, ingresoCantidadX, catalogo.codProdGenerico);
        if (generico.formatoInvalido) return { error: 'Formato de código inválido' };

        if (generico.esGenerico) {
            // En C# el precio se parsea dentro de InterpretarCodigoGenerico, antes de buscar el producto.
            if (generico.precioInvalido) return { success: false, message: 'Código inválido' };
            var productoGenerico = buscarPorCodigo(catalogo, generico.codigoProducto);
            if (!productoGenerico) return { success: false, message: 'No existe  el código genérico' };
            return respuestaProducto(productoGenerico, redondear2(comoFloat(generico.precioManual)), null, 'generico');
        }

        var interno = interpretar(codigo, catalogo);
        if (interno.esCodigoInterno) {
            if (interno.caso === 'estructuraInvalida') return { success: false, message: interno.mensaje };
            if (interno.caso === 'productoNoEncontrado') return { success: false, message: 'Código inexistente' };
            if (interno.tipoValor === 'Precio') {
                return respuestaProducto(interno.producto, redondear2(comoFloat(interno.valor)), null, 'etiqueta');
            }
            return respuestaProducto(interno.producto, interno.producto.precioKg, interno.valor, 'catalogo');
        }

        // Convert.ToInt64: algo que no es un entero tira excepcion en el servidor (que devuelve su mensaje).
        if (!/^[+-]?\d+$/.test(codigo.trim())) return { success: false, message: 'Código inválido' };
        var producto = buscarPorCodigo(catalogo, String(parseInt(codigo.trim(), 10)));
        if (!producto) return { success: false, message: 'Código inexistente' };
        return respuestaProducto(producto, producto.precioKg, null, 'catalogo');
    }

    // Contratos.CodigoProducto.SinCerosIzquierda.
    function sinCerosIzquierda(texto) {
        var sin = String(texto || '').replace(/^0+/, '');
        return sin.length === 0 ? texto : sin;
    }

    // ProductosController.ListarProductos contra el catalogo local (modal F10). Devuelve [{ id, codigo, nombre, precio }].
    function buscarPorTexto(texto, catalogo, max) {
        var limite = max || MAX_RESULTADOS_TEXTO;
        var productos = ((catalogo && catalogo.productos) || []).filter(function (p) { return p && Number(p.codigo) >= 0; });
        var q = String(texto || '').trim();
        if (q.length > 0) {
            var qMin = q.toLowerCase();
            var qSinCeros = sinCerosIzquierda(q);
            productos = productos.filter(function (p) {
                var codigo = String(p.codigo);
                return (p.nombre && String(p.nombre).toLowerCase().indexOf(qMin) >= 0)
                    || codigo.indexOf(q) >= 0
                    || codigo.indexOf(qSinCeros) >= 0;
            });
        }
        return productos.slice(0, limite).map(function (p) {
            return { id: p.id, codigo: String(p.codigo), nombre: p.nombre, precio: p.precioKg };
        });
    }

    var api = {
        buscarProducto: buscarProducto,
        buscarPorTexto: buscarPorTexto,
        interpretar: interpretar,
        interpretarCodigoGenerico: interpretarCodigoGenerico,
        esEan13Valido: esEan13Valido,
        esEan8Valido: esEan8Valido,
        redondear2: redondear2
    };

    root.PosOfflineBuscar = api;
    if (typeof module !== 'undefined' && module.exports) module.exports = api;
})(typeof window !== 'undefined' ? window : globalThis);
