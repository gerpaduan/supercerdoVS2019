// Existencia por sucursales: anchos de columna elegidos por el usuario (arrastrando el borde del encabezado). Solo logica pura, sin DOM:
// que columnas se pueden redimensionar, sus topes en px, y leer / escribir lo guardado en localStorage. El arrastre y la aplicacion
// en pantalla viven en existencia-sucursales.js. Tests: WebCore.Tests/js/existencia-anchos.test.js. Porque: docs/DECISIONS.md 2026-11-20.
(function (global) {
    'use strict';

    // Columnas redimensionables y su ancho minimo / maximo en px. Stock y Estado son UN ancho para todas las sucursales (y el Total):
    // si cada sucursal tuviera el suyo la tabla quedaria despareja. El minimo real de Stock y Estado lo pone ademas el navegador: nunca
    // achica una columna por debajo de su numero o de su estado mas largo (no se corta un numero de stock).
    var COLUMNAS = {
        cod: { min: 24, max: 400 },
        prod: { min: 48, max: 800 },
        stock: { min: 30, max: 400 },
        estado: { min: 30, max: 400 }
    };

    // Clave de localStorage por modo de la pantalla: el panel del movimiento y la ventana independiente son mas angostos que la pantalla
    // normal, asi que cada uno recuerda sus propios anchos (agrandar Producto en la pantalla grande no rompe el panel del movimiento).
    var CLAVES = { normal: 'existenciaAnchos', popup: 'existenciaAnchosPopup', movimiento: 'existenciaAnchosMov' };

    function claveAlmacen(modo) {
        return CLAVES[modo] || CLAVES.normal;
    }

    function esColumna(columna) {
        return Object.prototype.hasOwnProperty.call(COLUMNAS, columna);
    }

    // Recorta un ancho a los topes de su columna y lo redondea a px enteros. Devuelve null si la columna no existe o el valor no es un numero.
    function limitar(columna, px) {
        if (!esColumna(columna)) return null;
        var n = Number(px);
        if (typeof px === 'boolean' || px === null || px === '' || !isFinite(n)) return null;
        var tope = COLUMNAS[columna];
        return Math.round(Math.min(tope.max, Math.max(tope.min, n)));
    }

    // Lee lo guardado (texto JSON de localStorage). Es entrada externa: cualquier cosa rara (JSON roto, columnas desconocidas, valores no
    // numericos) se descarta en silencio y la columna queda con su ancho por defecto. Siempre devuelve un objeto { columna: px }.
    function leer(texto) {
        var anchos = {};
        if (typeof texto !== 'string' || !texto) return anchos;
        var crudo;
        try {
            crudo = JSON.parse(texto);
        } catch (e) {
            return anchos; // guardado corrupto: se arranca con los anchos por defecto (no es un error que valga avisar)
        }
        if (!crudo || typeof crudo !== 'object' || Array.isArray(crudo)) return anchos;
        Object.keys(COLUMNAS).forEach(function (columna) {
            var px = limitar(columna, crudo[columna]);
            if (px !== null) anchos[columna] = px;
        });
        return anchos;
    }

    // Texto para guardar. Sin columnas personalizadas devuelve null: la vista borra la clave en vez de guardar "{}".
    function serializar(anchos) {
        var limpio = leer(JSON.stringify(anchos || {}));
        return Object.keys(limpio).length ? JSON.stringify(limpio) : null;
    }

    // Ancho nuevo al arrastrar: el de arranque mas lo que se movio el mouse, dentro de los topes.
    function anchoArrastrado(columna, pxInicial, desplazamiento) {
        return limitar(columna, Number(pxInicial) + Number(desplazamiento));
    }

    global.ExistenciaAnchos = {
        COLUMNAS: COLUMNAS,
        claveAlmacen: claveAlmacen,
        esColumna: esColumna,
        limitar: limitar,
        leer: leer,
        serializar: serializar,
        anchoArrastrado: anchoArrastrado
    };
})(window);
