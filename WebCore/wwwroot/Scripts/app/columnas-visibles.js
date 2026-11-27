/* Elegir que columnas de una tabla se ven (2026-11-27, ver docs/DECISIONS.md). Opt-in y declarativo: la pantalla pone un
   contenedor [data-columnas] con data-columnas-tabla (selector de la tabla), data-columnas-clave (donde se recuerda) y una
   casilla por columna (value = sufijo de la clase "lc-<value>" que llevan su th y sus td; data-control = entra en la vista
   rapida "Control"; disabled = siempre visible). Se ocultan con CSS por clase, asi sirve aunque la tabla se arme despues.
   La eleccion se guarda en este navegador. Las funciones puras tienen test (WebCore.Tests/js/columnas-visibles.test.js). */
(function (root) {
    'use strict';

    // Con esta cantidad de columnas visibles o menos la tabla se compacta (quedan juntas a la izquierda).
    var MAXIMO_COMPACTA = 7;

    // columnas: [{ clave, control, fija }]. Devuelve las claves ocultas validas a partir de lo guardado (texto JSON):
    // ignora basura, claves que ya no existen y columnas fijas.
    function leerOcultas(guardado, columnas) {
        var lista;
        try { lista = JSON.parse(guardado || '[]'); } catch (e) { return []; }
        if (!Array.isArray(lista)) return [];
        return columnas.filter(function (c) { return !c.fija && lista.indexOf(c.clave) >= 0; }).map(function (c) { return c.clave; });
    }

    // Vista "Control": se oculta todo lo que no es de control ni fijo.
    function ocultasControl(columnas) {
        return columnas.filter(function (c) { return !c.control && !c.fija; }).map(function (c) { return c.clave; });
    }

    function mismasClaves(a, b) {
        return a.length === b.length && a.every(function (clave) { return b.indexOf(clave) >= 0; });
    }

    function textoBoton(cantidadOcultas) {
        if (!cantidadOcultas) return 'Columnas';
        return 'Columnas (' + cantidadOcultas + (cantidadOcultas === 1 ? ' oculta)' : ' ocultas)');
    }

    function esCompacta(total, cantidadOcultas) {
        return total - cantidadOcultas <= MAXIMO_COMPACTA;
    }

    // Reglas CSS que ocultan th y td de las columnas elegidas. La clave se limpia: termina dentro de un selector.
    function reglas(selectorTabla, ocultas) {
        return ocultas.map(function (clave) {
            return selectorTabla + ' .lc-' + String(clave).replace(/[^a-z0-9-]/g, '') + ' { display: none !important; }';
        }).join('\n');
    }

    if (typeof module !== 'undefined' && module.exports) {
        module.exports = { leerOcultas: leerOcultas, ocultasControl: ocultasControl, mismasClaves: mismasClaves, textoBoton: textoBoton, esCompacta: esCompacta, reglas: reglas };
    }
    if (!root || !root.document) return;
    var documento = root.document;

    function preparar(contenedor) {
        var selector = contenedor.getAttribute('data-columnas-tabla');
        var claveGuardado = contenedor.getAttribute('data-columnas-clave');
        var boton = contenedor.querySelector('.js-columnas-boton');
        var panel = contenedor.querySelector('.js-columnas-panel');
        var texto = contenedor.querySelector('.js-columnas-texto');
        var todas = contenedor.querySelector('.js-columnas-todas');
        var control = contenedor.querySelector('.js-columnas-control');
        var casillas = Array.prototype.slice.call(contenedor.querySelectorAll('.js-columnas-lista input[type="checkbox"]'));
        if (!selector || !boton || !panel || !casillas.length) return;

        var columnas = casillas.map(function (c) { return { clave: c.value, control: c.hasAttribute('data-control'), fija: c.disabled }; });
        var estilo = documento.createElement('style');
        documento.head.appendChild(estilo);

        var guardado = null;
        try {
            guardado = root.localStorage.getItem(claveGuardado);
        } catch (e) {
            // Sin storage (modo privado, datos bloqueados): se muestran todas y no se recuerda.
            if (root.console) root.console.warn('No se pudo leer la eleccion de columnas.', e);
        }
        var ocultas = leerOcultas(guardado, columnas);

        function aplicar() {
            estilo.textContent = reglas(selector, ocultas);
            casillas.forEach(function (c) { c.checked = ocultas.indexOf(c.value) < 0; });
            if (texto) texto.textContent = textoBoton(ocultas.length);
            boton.classList.toggle('hay-ocultas', ocultas.length > 0);
            if (todas) todas.classList.toggle('activa', ocultas.length === 0);
            if (control) control.classList.toggle('activa', mismasClaves(ocultas, ocultasControl(columnas)));
            var tabla = documento.querySelector(selector);
            if (tabla) tabla.classList.toggle('compacta', esCompacta(columnas.length, ocultas.length));
        }

        function elegir(nuevas) {
            ocultas = nuevas;
            aplicar();
            try {
                root.localStorage.setItem(claveGuardado, JSON.stringify(ocultas));
            } catch (e) {
                // Solo es una comodidad: la eleccion vale hasta recargar.
                if (root.console) root.console.warn('No se pudo guardar la eleccion de columnas.', e);
            }
        }

        function abrir(abierto) {
            panel.classList.toggle('abierto', abierto);
            boton.setAttribute('aria-expanded', abierto ? 'true' : 'false');
        }

        boton.addEventListener('click', function () { abrir(!panel.classList.contains('abierto')); });
        documento.addEventListener('click', function (event) { if (!contenedor.contains(event.target)) abrir(false); });
        documento.addEventListener('keydown', function (event) {
            if (event.key !== 'Escape' || !panel.classList.contains('abierto')) return;
            abrir(false);
            boton.focus();
        });
        if (todas) todas.addEventListener('click', function () { elegir([]); });
        if (control) control.addEventListener('click', function () { elegir(ocultasControl(columnas)); });
        casillas.forEach(function (casilla) {
            casilla.addEventListener('change', function () {
                elegir(casillas.filter(function (c) { return !c.checked && !c.disabled; }).map(function (c) { return c.value; }));
            });
        });
        aplicar();
    }

    function iniciar() {
        Array.prototype.forEach.call(documento.querySelectorAll('[data-columnas]'), preparar);
    }

    if (documento.readyState === 'loading') documento.addEventListener('DOMContentLoaded', iniciar);
    else iniciar();
})(typeof window !== 'undefined' ? window : null);
