// Ayuda / manual de uso (2026-10-03, ver docs/DECISIONS.md "Manuales por rol y ayuda en cada pantalla").
// REGLA DE ORO: pedir ayuda NUNCA saca al usuario de su pantalla. Todo se carga por fetch y se muestra
// en el panel lateral (#panelAyuda, offcanvas de Bootstrap 5) encima de la pagina: no se navega, no se
// recarga, no se toca ningun formulario ni la venta en curso. Los enlaces internos de la ayuda
// (data-ayuda="tipo/clave" o "indice/<rol>") se abren ADENTRO del panel, con boton Volver.
// Solo en las paginas completas de ayuda (/Ayuda/Ver, /Ayuda/Manual, abiertas en pestaña nueva, sin panel)
// esos enlaces navegan a la pagina correspondiente.
(function () {
    'use strict';

    var panel = document.getElementById('panelAyuda');
    var base = panel ? (panel.getAttribute('data-base') || '') : '';

    // Sin panel en el layout (p. ej. una pagina con Layout = null) la base se toma de un elemento
    // marcado con data-ayuda-base; si tampoco hay, no hay nada que hacer.
    if (!base) {
        var otraBase = document.querySelector('[data-ayuda-base]');
        base = otraBase ? otraBase.getAttribute('data-ayuda-base') : '';
    }
    if (!base) return;

    function partir(destino) {
        // destino: "indice/usuario" | "pantalla/Usuarios.Index" | "concepto/slug" | "referencia/slug"
        var i = destino.indexOf('/');
        return {
            tipo: i < 0 ? destino : destino.substring(0, i),
            clave: i < 0 ? '' : destino.substring(i + 1)
        };
    }

    function urlFragmento(destino) {
        var d = partir(destino);
        if (d.tipo === 'indice') return base + '/Indice?rol=' + encodeURIComponent(d.clave);
        return base + '/Doc?tipo=' + encodeURIComponent(d.tipo) + '&clave=' + encodeURIComponent(d.clave);
    }

    function urlPagina(destino) {
        var d = partir(destino);
        if (d.tipo === 'indice') return base + '/Manual?rol=' + encodeURIComponent(d.clave);
        return base + '/Ver?tipo=' + encodeURIComponent(d.tipo) + '&clave=' + encodeURIComponent(d.clave);
    }

    // ---- Paginas completas (sin panel): los enlaces internos navegan a la pagina de ese documento ----
    if (!panel) {
        document.addEventListener('click', function (e) {
            var enlace = e.target.closest ? e.target.closest('[data-ayuda]') : null;
            if (!enlace) return;
            e.preventDefault();
            window.location.href = urlPagina(enlace.getAttribute('data-ayuda'));
        });
        return;
    }

    // ---- Panel ----
    var cuerpo = document.getElementById('panelAyudaCuerpo');
    var btnVolver = document.getElementById('btnAyudaVolver');
    var lnkPagina = document.getElementById('lnkAyudaPagina');
    var form = document.getElementById('formAyudaBuscar');
    var txtBuscar = document.getElementById('txtAyudaBuscar');
    var historial = [];      // urls ya mostradas en el panel (para Volver)
    var urlActual = null;
    var cargadoInicial = false;

    function actualizarBarra() {
        btnVolver.hidden = historial.length === 0;
        var contenido = cuerpo.querySelector('.ayuda-contenido');
        var pagina = contenido ? contenido.getAttribute('data-pagina-url') : '';
        if (pagina) lnkPagina.setAttribute('href', pagina);
    }

    function cargar(url, agregarAlHistorial) {
        if (agregarAlHistorial && urlActual) historial.push(urlActual);
        urlActual = url;
        cuerpo.innerHTML = '<div class="text-muted small">Cargando…</div>';

        fetch(url, { credentials: 'same-origin', headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (r) {
                if (!r.ok) throw new Error('HTTP ' + r.status);
                return r.text();
            })
            .then(function (html) {
                cuerpo.innerHTML = html;
                cuerpo.scrollTop = 0;
                actualizarBarra();
            })
            .catch(function () {
                cuerpo.innerHTML = '<div class="alert alert-warning small mb-0">No se pudo cargar la ayuda. Probá de nuevo en un momento.</div>';
                actualizarBarra();
            });
    }

    function cargarPantalla() {
        historial = [];
        urlActual = null;
        cargar(base + '/Panel?c=' + encodeURIComponent(panel.getAttribute('data-c') || '') +
            '&a=' + encodeURIComponent(panel.getAttribute('data-a') || ''), false);
        cargadoInicial = true;
    }

    function cargarIndice() {
        historial = [];
        urlActual = null;
        cargar(base + '/Indice', false);
    }

    function abrir(modo) {
        var oc = window.bootstrap && window.bootstrap.Offcanvas ? window.bootstrap.Offcanvas.getOrCreateInstance(panel) : null;
        if (oc) oc.show();
        if (modo === 'indice') {
            cargarIndice();
            cargadoInicial = true;
        } else if (!cargadoInicial) {
            cargarPantalla();
        }
    }

    // Boton "Ayuda" de la barra superior y cualquier otro disparador [data-ayuda-abrir].
    document.addEventListener('click', function (e) {
        var disparador = e.target.closest ? e.target.closest('[data-ayuda-abrir]') : null;
        if (!disparador) return;
        e.preventDefault();
        abrir(disparador.getAttribute('data-ayuda-abrir'));
    });

    document.getElementById('btnAyudaPantalla').addEventListener('click', cargarPantalla);
    document.getElementById('btnAyudaManuales').addEventListener('click', cargarIndice);
    btnVolver.addEventListener('click', function () {
        if (historial.length === 0) return;
        cargar(historial.pop(), false);
    });

    form.addEventListener('submit', function (e) {
        e.preventDefault();
        var q = (txtBuscar.value || '').trim();
        if (q.length < 2) return;
        cargar(base + '/Buscar?q=' + encodeURIComponent(q), true);
    });

    // Escribir en la busqueda no debe disparar atajos de teclado de la pantalla de atras (POS, etc.).
    ['keydown', 'keyup', 'keypress'].forEach(function (nombre) {
        txtBuscar.addEventListener(nombre, function (e) { e.stopPropagation(); });
    });

    // Enlaces internos dentro del panel: se abren adentro, sin navegar.
    panel.addEventListener('click', function (e) {
        var enlace = e.target.closest ? e.target.closest('[data-ayuda]') : null;
        if (!enlace) return;
        e.preventDefault();
        cargar(urlFragmento(enlace.getAttribute('data-ayuda')), true);
    });
})();
