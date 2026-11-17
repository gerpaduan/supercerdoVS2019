/* Botones − / + para achicar o agrandar el ANCHO de las pantallas independientes (2026-11-17, pedido del usuario; ajustado el mismo dia: solo el ancho
   y junto al boton «... sin guardar»).
   Donde hay un boton «Stock / Movimientos / Elaborados sin guardar» (o «Borradores sin guardar» en Existencia) el par − / + se inserta justo ANTES de ese
   boton, en la misma barra de la derecha. En las pantallas sin ese boton (listados, POS, Expendio) no aparece nada: una pastilla flotante abajo a la derecha
   se probo y tapaba «Guardar» / «Cancelar» (aviso del usuario, 2026-11-17).
   Aparece solo en: (a) Stock / Movimientos / Elaborados en modo ventana (html.modo-ventana, ver _Layout.cshtml), (b) las mini-apps instaladas
   (display-mode: standalone con <meta name="carnisys-app-scope"> o layout embebido), (c) las ventanas emergentes de _LayoutEmbed (tienen window.opener).
   Nunca en iframes ni en una pestana normal.
   Cada clic cambia el ANCHO de la ventana un 10 % del ancho que tenia al abrirse, con el borde DERECHO fijo (crece o se achica hacia la izquierda) (window.resizeTo + moveTo, el mismo mecanismo que ya usan el boton «Ensanchar» de
   Movimientos y el cartel de Existencia), con minimo del 50 % y maximo hasta el borde izquierdo de la pantalla; el alto no cambia. Si el navegador no deja redimensionar esa
   ventana avisa «no permite cambiar el tamano». El ultimo nivel se recuerda por seccion (localStorage) y se reaplica al abrir.
   Limites: otros scripts que cambian el tamano (Movimientos a 1/3 del ancho, «Ensanchar») pueden pisar lo recordado. Decision: docs/DECISIONS.md (2026-11-17 e). */
(function () {
    'use strict';

    if (window !== window.top) return; // dentro de un iframe (panel de stock del destino, etc.) no

    var PASO = 0.10;                    // 10 % por clic
    var RATIO_MINIMO = 0.5;             // la ventana no baja del 50 % del ancho original
    var ESPERA_MEDIR_MS = 400;          // el navegador aplica el resizeTo un instante despues
    var CLAVE_BASE = 'carnisys-ventana-base';       // sessionStorage: ancho de la ventana al abrirse (sobrevive a la navegacion dentro de la ventana)
    var CLAVE_NIVEL = 'carnisys-ventana-nivel-';    // localStorage + seccion: { ratio }

    // Botones «sin guardar» junto a los que va el par − / +. 'envoltorio' = el contenedor que se mueve (en Existencia el boton va dentro de un .ex-ctl).
    // Existencia: si los borradores estan apagados no hay «Borradores sin guardar»; entonces el par va antes del bloque Excel / PDF (#btnExcelExistencia).
    var ANCLAS = ['#btnVerBorradoresStock', '#btnVerBorradoresMovimientos', '#btnVerBorradoresElaborados', '#btnVerBorradoresPedido', '#btnExcelExistencia'];

    function enAppInstalada() {
        return (window.matchMedia && window.matchMedia('(display-mode: standalone)').matches) || window.navigator.standalone === true;
    }

    function aplica() {
        var embebida = !!(document.body && document.body.classList.contains('app-embed'));
        if (enAppInstalada()) return !!document.querySelector('meta[name="carnisys-app-scope"]') || embebida;
        if (document.documentElement.classList.contains('modo-ventana')) return true;
        return embebida && !!window.opener;
    }

    function seccion() {
        // Con PathBase (/CarniSysWeb/Stock/...) el primer segmento seria el prefijo: si hay meta de scope se usa su ultimo segmento.
        var scope = (document.querySelector('meta[name="carnisys-app-scope"]') || {}).content || '';
        var segmentosScope = scope.toLowerCase().split('/').filter(Boolean);
        if (segmentosScope.length) return segmentosScope[segmentosScope.length - 1];
        return window.location.pathname.toLowerCase().split('/').filter(Boolean)[0] || 'inicio';
    }

    function leer(almacen, clave) {
        try { return JSON.parse(almacen.getItem(clave)); } catch (e) { return null; }
    }
    function escribir(almacen, clave, valor) {
        try { almacen.setItem(clave, JSON.stringify(valor)); } catch (e) { /* sin almacenamiento: el nivel simplemente no se recuerda */ }
    }

    function base() {
        var guardado = leer(window.sessionStorage, CLAVE_BASE);
        if (guardado && guardado.w > 0) return guardado;
        var nuevo = { w: window.outerWidth };
        escribir(window.sessionStorage, CLAVE_BASE, nuevo);
        return nuevo;
    }

    var claveNivel = null;
    var ratioActual = 1;

    function pantallaDisponible() {
        var s = window.screen || {};
        return { x: s.availLeft || 0, w: s.availWidth || window.outerWidth };
    }

    // El nivel actual se ve en el tooltip de los botones (no hay numero en pantalla: ocuparia lugar en la barra).
    function pintarNivel() {
        var detalle = 'Ancho de la ventana al ' + Math.round(ratioActual * 100) + ' %';
        var menos = document.querySelectorAll('.vt-menos'), mas = document.querySelectorAll('.vt-mas');
        Array.prototype.forEach.call(menos, function (b) { b.title = 'Achicar el ancho 10 %. ' + detalle; });
        Array.prototype.forEach.call(mas, function (b) { b.title = 'Agrandar el ancho 10 %. ' + detalle; });
    }

    function guardarNivel() { escribir(window.localStorage, claveNivel, { ratio: ratioActual }); }

    function avisar(texto) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({ toast: true, position: 'top', icon: 'info', title: texto, showConfirmButton: false, timer: 3000 });
        } else {
            window.alert(texto);
        }
    }

    // Pone el ancho de la ventana en "ratio" veces el original; avisa por alTerminar(cambio, ratioAplicado) si el navegador la redimensiono.
    function aplicarRatio(ratio, alTerminar) {
        var b = base();
        var p = pantallaDisponible();
        // El borde DERECHO de la ventana queda fijo: crece o se achica hacia la izquierda (pedido del usuario). Se lee antes de redimensionar.
        // Por eso el maximo es lo que hay entre ese borde y el borde izquierdo de la pantalla.
        var derecha = window.screenX + window.outerWidth;
        var tope = (derecha - p.x) / b.w;
        var r = Math.min(Math.max(ratio, RATIO_MINIMO), Math.max(tope, RATIO_MINIMO));
        var w = Math.round(b.w * r);
        var antes = window.outerWidth;
        try {
            window.resizeTo(w, window.outerHeight);
            window.moveTo(Math.max(p.x, derecha - w), window.screenY || 0);
        } catch (e) {
            console.warn('[ventana-tamano] no se pudo redimensionar la ventana', e);
        }
        window.setTimeout(function () {
            var cambio = Math.abs(window.outerWidth - antes) >= 2 || Math.abs(window.outerWidth - w) < 6;
            alTerminar(cambio, r);
        }, ESPERA_MEDIR_MS);
    }

    function cambiar(direccion) {
        var b = base();
        // Pasos enteros respecto del ancho original, a partir de donde esta la ventana hoy (puede haberla movido el usuario con el borde).
        var pasos = Math.round((window.outerWidth / b.w - 1) / PASO) + direccion;
        var meta = 1 + pasos * PASO;
        aplicarRatio(meta, function (cambio, r) {
            if (cambio) {
                ratioActual = Math.round(r * 100) / 100;
                pintarNivel();
                guardarNivel();
            } else if (r === meta) {
                // pidio un cambio valido y el navegador no lo hizo: esta ventana no se puede redimensionar desde la pagina
                avisar('El navegador no permite cambiar el tamaño de esta ventana.');
            } // si r !== meta se llego al minimo o al ancho de la pantalla: no hay nada mas que hacer
        });
    }

    // Par − / + con el estilo de los botones de la barra (Bootstrap), para insertarlo junto al boton «sin guardar».
    function crearGrupoEnBarra() {
        var grupo = document.createElement('span');
        grupo.className = 'btn-group vt-grupo';
        grupo.setAttribute('role', 'group');
        grupo.setAttribute('aria-label', 'Ancho de la ventana');
        grupo.innerHTML =
            '<button type="button" class="btn btn-outline-secondary vt-menos" aria-label="Achicar el ancho de la ventana"><i class="fas fa-minus"></i></button>' +
            '<button type="button" class="btn btn-outline-secondary vt-mas" aria-label="Agrandar el ancho de la ventana"><i class="fas fa-plus"></i></button>';
        return grupo;
    }

    function conectar(contenedor) {
        contenedor.querySelector('.vt-menos').addEventListener('click', function () { cambiar(-1); });
        contenedor.querySelector('.vt-mas').addEventListener('click', function () { cambiar(1); });
    }

    // Busca el boton «sin guardar» de la pantalla; devuelve el elemento ANTES del cual insertar (en Existencia, su .ex-ctl).
    function buscarAncla() {
        // Existencia en modo movimiento (panel de stock del destino): ahi la barra no tiene Excel/PDF/borradores y el ancho lo maneja Movimientos.
        if (document.querySelector('.ex-modo-mov')) return null;
        for (var i = 0; i < ANCLAS.length; i++) {
            var boton = document.querySelector(ANCLAS[i]);
            if (!boton) continue;
            var envoltorio = boton.closest('.ex-ctl');
            return { boton: boton, referencia: envoltorio || boton, enExistencia: !!envoltorio };
        }
        return null;
    }

    function insertarEnBarra(ancla) {
        var grupo = crearGrupoEnBarra();
        if (ancla.enExistencia) {
            // Existencia: la barra son bloques .ex-ctl con una etiqueta de relleno arriba; se arma uno igual para el par.
            var bloque = document.createElement('div');
            bloque.className = 'ex-ctl ex-sec';
            bloque.innerHTML = '<label class="ex-lbl d-none d-md-block">&nbsp;</label>';
            bloque.appendChild(grupo);
            ancla.referencia.parentNode.insertBefore(bloque, ancla.referencia);
        } else {
            ancla.referencia.parentNode.insertBefore(grupo, ancla.referencia);
        }
        conectar(grupo);
    }

    document.addEventListener('DOMContentLoaded', function () {
        if (!aplica()) return;
        claveNivel = CLAVE_NIVEL + seccion();
        var guardado = leer(window.localStorage, claveNivel);
        if (guardado && guardado.ratio > 0) ratioActual = guardado.ratio;
        base(); // fija el ancho original antes de que nadie lo cambie

        // Sin boton «sin guardar» en la pantalla no se muestra nada (una pastilla flotante tapaba los botones de Guardar/Cancelar).
        var ancla = buscarAncla();
        if (!ancla) return;
        insertarEnBarra(ancla);
        pintarNivel();

        // Reaplica lo recordado (mejor esfuerzo: si el navegador no deja redimensionar al abrir, queda el 100 % y el usuario usa los botones).
        if (ratioActual !== 1) {
            aplicarRatio(ratioActual, function (cambio, r) { if (cambio) ratioActual = Math.round(r * 100) / 100; pintarNivel(); });
        }
    });
})();
