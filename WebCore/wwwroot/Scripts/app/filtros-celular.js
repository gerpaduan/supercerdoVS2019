/* Filtros plegados en celular (2026-11-25): en < 768 px la tarjeta marcada con data-filtros-celular arranca cerrada y se abre con una barra
   «Filtros: …» que resume lo elegido. Opt-in: cualquier pantalla lo suma marcando la tarjeta e incluyendo Shared/_FiltrosCelular.cshtml.
   Solo DOM: no toca ids, names, el submit ni el guardado de filtros de cada pantalla. En escritorio la barra existe pero el CSS
   (Content/css/filtros-celular.css) la oculta y la tarjeta se ve como siempre. Si hay un aviso visible dentro de la tarjeta (fechas
   pendientes, permiso de fecha) se abre sola. Sin dependencias; armarResumen es puro y tiene test (WebCore.Tests/js/filtros-celular.test.js). */
(function (root) {
    'use strict';

    // Valores que no filtran nada: no se muestran en el resumen.
    var GENERICOS = /^(todos|todas|ver todos|seleccione\.*|-+)$/i;
    var SIN_FILTROS = 'Sin filtros';

    // 'yyyy-MM-dd' o 'yyyy-MM-ddTHH:mm' -> 'dd/mm' (cadena vacia si no es una fecha).
    function fechaCorta(valor) {
        var m = /^\d{4}-(\d{2})-(\d{2})/.exec(String(valor || ''));
        return m ? m[2] + '/' + m[1] : '';
    }

    // campos: [{ tipo, nombre, valor, texto }] en el orden de la pantalla. Las fechas «desde» y «hasta» se juntan en un solo tramo, en el lugar de
    // la primera fecha. De un select se muestra el texto elegido (no el valor) salvo que sea generico («Todas»); de un texto, lo escrito.
    function armarResumen(campos) {
        var partes = [];
        var desde = '', hasta = '', lugarFechas = -1;
        (campos || []).forEach(function (c) {
            if (c.tipo === 'date' || c.tipo === 'datetime-local') {
                var corta = fechaCorta(c.valor);
                if (!corta) return;
                if (lugarFechas < 0) { lugarFechas = partes.length; partes.push(''); }
                if (/hasta/i.test(c.nombre || '')) hasta = corta;
                else if (!desde) desde = corta;
                else hasta = corta;
                return;
            }
            var texto = String((c.tipo === 'select' ? c.texto : c.valor) || '').trim();
            if (!texto || GENERICOS.test(texto)) return;
            partes.push(texto);
        });
        if (lugarFechas >= 0) {
            partes[lugarFechas] = (desde && hasta) ? (desde === hasta ? desde : desde + ' al ' + hasta)
                : (desde ? 'desde ' + desde : 'hasta ' + hasta);
        }
        return partes.length ? partes.join(' · ') : SIN_FILTROS;
    }

    if (typeof module !== 'undefined' && module.exports) {
        module.exports = { armarResumen: armarResumen, fechaCorta: fechaCorta };
    }
    if (!root || !root.document) return;
    var documento = root.document;

    // Campos visibles-por-diseno de la tarjeta. Se saltean ocultos, botones, casillas y los selectores auxiliares de fecha para celular de Ventas.
    function leerCampos(tarjeta) {
        var campos = [];
        Array.prototype.forEach.call(tarjeta.querySelectorAll('select, input'), function (el) {
            var tipo = el.tagName === 'SELECT' ? 'select' : (el.type || 'text');
            if (/^(hidden|submit|button|checkbox|radio|file|reset)$/.test(tipo)) return;
            if (el.closest('.d-none, .fecha-mobile-picker, .dropdown-menu')) return;
            var opcion = tipo === 'select' && el.selectedIndex >= 0 ? el.options[el.selectedIndex] : null;
            campos.push({ tipo: tipo, nombre: el.name || el.id || '', valor: el.value, texto: opcion ? opcion.textContent : '' });
        });
        return campos;
    }

    function hayAvisoVisible(tarjeta) {
        return !!tarjeta.querySelector('.alert:not(.d-none)');
    }

    function preparar(tarjeta) {
        var barra = documento.createElement('button');
        barra.type = 'button';
        barra.className = 'fcel-barra';
        barra.setAttribute('data-edit-readonly-ignore', 'true');
        barra.setAttribute('aria-expanded', 'false');
        barra.innerHTML = '<i class="fas fa-filter" aria-hidden="true"></i><span class="fcel-resumen"></span><i class="fas fa-chevron-down fcel-flecha" aria-hidden="true"></i>';
        tarjeta.parentNode.insertBefore(barra, tarjeta);
        tarjeta.classList.add('fcel-activo');
        var resumen = barra.querySelector('.fcel-resumen');

        function actualizar() { resumen.textContent = armarResumen(leerCampos(tarjeta)); }
        function abrir(abierto) {
            tarjeta.classList.toggle('fcel-abierto', abierto);
            barra.setAttribute('aria-expanded', abierto ? 'true' : 'false');
        }
        barra.addEventListener('click', function () {
            actualizar();
            abrir(!tarjeta.classList.contains('fcel-abierto'));
        });
        tarjeta.addEventListener('change', actualizar);
        tarjeta.addEventListener('input', actualizar);
        // Un aviso que aparece dentro de la tarjeta (otros scripts le sacan d-none) la abre: si quedara cerrada, no se veria.
        if (root.MutationObserver) {
            new root.MutationObserver(function () { if (hayAvisoVisible(tarjeta)) abrir(true); })
                .observe(tarjeta, { subtree: true, attributes: true, attributeFilter: ['class'] });
        }
        actualizar();
        if (hayAvisoVisible(tarjeta)) abrir(true);
        // Las pantallas restauran filtros guardados despues de cargar: se relee cuando termina la carga.
        root.addEventListener('load', actualizar);
    }

    function iniciar() {
        Array.prototype.forEach.call(documento.querySelectorAll('[data-filtros-celular]'), function (tarjeta) {
            if (!tarjeta.classList.contains('fcel-activo')) preparar(tarjeta);
        });
    }

    if (documento.readyState === 'loading') documento.addEventListener('DOMContentLoaded', iniciar);
    else iniciar();
})(typeof window !== 'undefined' ? window : null);
