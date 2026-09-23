// Vigilancia del "producto pendiente" del POS: detecta un producto pesado que queda en pantalla con
// cantidad estable mas de N segundos (el cliente ve kilos y total) y luego sale SIN agregarse al carrito
// (fraude: pesar, cobrar y borrar el codigo). Registra una ADVERTENCIA silenciosa para el admin. Ver
// docs/DECISIONS.md "Ventas en curso: borrador en servidor y advertencias del POS".
//
// Reglas que conviene no romper:
//  - El cajero NO debe enterarse: nada visible (sin toast, sin bloqueo). Cualquier falla se ignora.
//  - Las duraciones se miden con performance.now() (reloj monotonico), nunca con Date: el servidor fija las
//    horas reales (inicio/fin) y un cajero no puede falsearlas cambiando la hora de su PC.
//  - Agregar el producto al carrito es el camino normal: pos-cart.js llama onLineaAgregada() justo despues de
//    POSState.addLinea y eso CANCELA el pendiente (sin eso, el showWaiting() de addProduct daria falso positivo).
//  - Es una advertencia, no una acusacion: un cliente arrepentido o un codigo equivocado tambien la disparan.
//
// Maquina de estados por "producto en pantalla":
//   onProductoChanged(producto)  -> arranca (o cambia de) pendiente; con null = codigo borrado.
//   tick cada 500 ms             -> suma tiempo "estable" mientras cantidad >= minimo y la balanza no marque
//                                   peso inestable; detecta cantidad 0/vacia sostenida.
//   salidas que registran        -> CODIGO_BORRADO, CODIGO_CAMBIADO, CANTIDAD_CERO (sostenida), FINALIZAR
//                                   (al confirmar el cobro con el producto pendiente) y CIERRE_PESTANA.
(function (window) {
    'use strict';

    // Cantidad minima que el POS acepta para una linea (misma que CANT_MINIMA de pos-cart.js).
    var CANT_MINIMA = 0.010;
    var TICK_MS = 500;

    var cfg = window.POSBorradorConfig || {};
    var accesores = { getModoCantidad: function () { return 'Manual'; }, getUltimaLectura: function () { return null; } };
    var activo = false;
    var pendiente = null;
    var timer = null;

    function ahora() {
        return window.performance && typeof window.performance.now === 'function' ? window.performance.now() : Date.now();
    }

    // Mismo parseo tolerante que pos-balanza.js (acepta "1,500" y "1.500"; el ultimo separador es el decimal).
    function parseCantidad(valor) {
        var texto = String(valor == null ? '' : valor).trim();
        if (!texto) return NaN;
        texto = texto.replace(/[^0-9,.\-]/g, '');
        if (!texto) return NaN;

        var primerSeparador = texto.search(/[.,]/);
        if (primerSeparador >= 0) {
            var entera = texto.slice(0, primerSeparador).replace(/[.,]/g, '');
            var decimal = texto.slice(primerSeparador + 1).replace(/[.,]/g, '');
            texto = entera + '.' + decimal;
        } else {
            texto = texto.replace(/[.,]/g, '');
        }

        var numero = parseFloat(texto);
        return isNaN(numero) ? NaN : numero;
    }

    function leerCantidad() {
        var input = document.getElementById('inputCantidad');
        return input ? parseCantidad(input.value) : NaN;
    }

    function balanzaInestable() {
        if (accesores.getModoCantidad() !== 'Balanza') return false;
        var lectura = accesores.getUltimaLectura();
        return !!(lectura && lectura.inestable === true);
    }

    function nuevoPendiente(producto) {
        var t = ahora();
        return {
            codigo: producto && producto.codigo != null ? String(producto.codigo) : '',
            inicio: t,
            ultimoTick: t,
            estableMs: 0,
            kgEstable: 0,
            origen: 'MANUAL',
            cantidadCeroDesde: null,
            emitido: false
        };
    }

    // Registra la advertencia en el servidor si el producto estuvo estable mas del umbral. finMs es el instante
    // (performance.now) en que el producto realmente "salio" (para CANTIDAD_CERO es cuando la cantidad bajo a 0,
    // no cuando se confirmo la salida).
    function emitir(motivo, finMs, keepalive) {
        var p = pendiente;
        if (!p || p.emitido || !p.codigo) return;

        var umbralMs = (cfg.segundosProductoSinAgregar || 7) * 1000;
        if (p.estableMs < umbralMs || !(p.kgEstable > 0)) return;

        p.emitido = true;

        var t = ahora();
        var fin = typeof finMs === 'number' ? finMs : t;
        var body = {
            clientId: window.POSBorrador && window.POSBorrador.getClientId ? window.POSBorrador.getClientId() : null,
            posInstanceId: (window.POSModo && window.POSModo.instanceId) || '',
            idSucursalPOS: parseInt(document.getElementById('idSucursalPOS') && document.getElementById('idSucursalPOS').value, 10) || 0,
            codigo: p.codigo,
            cantidadKg: p.kgEstable,
            segundosEnPantalla: Math.max(0, Math.round((fin - p.inicio) / 1000)),
            segundosEstables: Math.max(0, Math.round(p.estableMs / 1000)),
            segundosDesdeSalida: Math.max(0, Math.round((t - fin) / 1000)),
            origen: p.origen,
            motivo: motivo
        };

        if (window.POSBorrador && typeof window.POSBorrador.post === 'function') {
            // Nunca se propaga una falla: el cajero no debe notar nada.
            window.POSBorrador.post(cfg.urls && cfg.urls.sinAgregar, body, keepalive === true).catch(function () { });
        }
    }

    function tick() {
        var p = pendiente;
        if (!p) return;

        var t = ahora();
        var dt = t - p.ultimoTick;
        p.ultimoTick = t;

        var cantidad = leerCantidad();
        var positiva = isFinite(cantidad) && cantidad >= CANT_MINIMA;

        if (positiva && !balanzaInestable()) {
            // Cantidad estable: suma tiempo y recuerda el ultimo peso/origen validos.
            p.estableMs += dt;
            p.kgEstable = cantidad;
            p.origen = accesores.getModoCantidad() === 'Balanza' ? 'BALANZA' : 'MANUAL';
            p.cantidadCeroDesde = null;
            return;
        }

        if (positiva) {
            // Peso inestable (el cliente acomoda la pieza): no suma, tampoco cuenta como salida.
            p.cantidadCeroDesde = null;
            return;
        }

        // Cantidad 0 o vacia: solo cuenta como salida si se sostiene unos segundos (una tara o un salto
        // momentaneo de la balanza no debe disparar la advertencia).
        if (p.cantidadCeroDesde == null) {
            p.cantidadCeroDesde = t;
            return;
        }

        var confirmacionMs = (cfg.segundosCantidadCero || 3) * 1000;
        if (t - p.cantidadCeroDesde >= confirmacionMs) {
            var codigoAnterior = p.codigo;
            emitir('CANTIDAD_CERO', p.cantidadCeroDesde, false);
            // El producto sigue seleccionado: un peso nuevo empieza un pendiente nuevo.
            pendiente = nuevoPendiente({ codigo: codigoAnterior });
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Entradas (las llaman pos-product.js via POS.cshtml, pos-cart.js y forma-pago.js)
    // ---------------------------------------------------------------------------------------------

    // producto = producto reconocido en pantalla, o null si se borro el codigo / no hubo coincidencia.
    function onProductoChanged(producto) {
        if (!activo) return;

        if (!producto) {
            emitir('CODIGO_BORRADO', undefined, false);
            pendiente = null;
            return;
        }

        var codigoNuevo = producto.codigo != null ? String(producto.codigo) : '';
        if (pendiente && pendiente.codigo === codigoNuevo) return; // mismo producto: sigue el mismo pendiente

        if (pendiente) emitir('CODIGO_CAMBIADO', undefined, false);
        pendiente = nuevoPendiente(producto);
    }

    // El producto se agrego al carrito: camino normal, se cancela el pendiente sin registrar nada.
    function onLineaAgregada() {
        pendiente = null;
    }

    // Se confirmo el cobro/finalizacion con un producto pesado todavia sin agregar en pantalla.
    function alFinalizarVenta() {
        if (!activo) return;
        emitir('FINALIZAR', undefined, false);
    }

    function alCerrarPestana() {
        if (!activo) return;
        if (window.PosNavegandoInternoPOS) return;
        emitir('CIERRE_PESTANA', undefined, true);
    }

    function init(opciones) {
        activo = cfg.advertenciaHabilitada === true && !!cfg.urls
            && !window.esEdicionVenta && !(window.POSModo && window.POSModo.soloFormaPago)
            && !!(window.POSBorrador && window.POSBorrador.estaHabilitado && window.POSBorrador.estaHabilitado());
        if (!activo) return;

        if (opciones) {
            if (typeof opciones.getModoCantidad === 'function') accesores.getModoCantidad = opciones.getModoCantidad;
            if (typeof opciones.getUltimaLectura === 'function') accesores.getUltimaLectura = opciones.getUltimaLectura;
        }

        clearInterval(timer);
        timer = setInterval(tick, TICK_MS);
        window.addEventListener('pagehide', alCerrarPestana);
    }

    window.POSProductoPendiente = {
        init: init,
        onProductoChanged: onProductoChanged,
        onLineaAgregada: onLineaAgregada,
        alFinalizarVenta: alFinalizarVenta
    };
})(window);
