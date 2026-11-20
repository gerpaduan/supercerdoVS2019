/* Marco y agarres para cambiar el tamano de las ventanas independientes (2026-11-20, pedido del usuario).
   Problema: en una ventana independiente (popup / app instalada) el borde real de la ventana lo dibuja el sistema, casi no se ve y es dificil
   agarrarlo. Una pagina web no puede engrosarlo, asi que se dibuja un marco PROPIO dentro de la ventana (sobre el contenido, sin cambiar el layout:
   ninguna pantalla se corre ni gana scroll) y sus bordes y esquinas son agarres: al pasar el cursor se resaltan y cambia el cursor; al arrastrar se
   cambia el tamano de la ventana con window.resizeTo / moveTo (el mismo mecanismo que los botones − / + de ventana-tamano.js), dejando fijo el borde
   opuesto. Arrastrar un lado cambia solo el ancho o el alto; una esquina, los dos.
   Aparece donde aparecen los − / +: las apps instaladas desde Utilidades (menos el POS), las ventanas de «Abrir en ventana» y las emergentes de
   _LayoutEmbed con window.opener (la condicion es VentanaTamano.aplica(), de ventana-tamano.js, que debe cargarse ANTES). Nunca en iframes, en
   pestanas normales ni en pantalla completa.
   Si la pagina tiene barra de desplazamiento vertical/horizontal, el marco y los agarres de ese lado se corren hacia adentro el ancho de la barra
   para no taparla. Si el navegador no deja redimensionar la ventana, avisa una vez.
   La geometria (limites de pantalla, minimos, borde opuesto fijo) esta en calcularGeometria(), pura, con tests en WebCore.Tests/js/marco-ventana.test.js.
   calcularPosicion() (tambien pura y con tests) la usa titulo-ventana.js para mover la ventana arrastrando su franja de titulo.
   Decision: docs/DECISIONS.md (2026-11-20 c). */
(function (window) {
    'use strict';

    // Tamano minimo de la ventana (debajo de esto las pantallas de trabajo no entran).
    var MIN_ANCHO_PX = 480;
    var MIN_ALTO_PX = 360;
    // Marco de doble borde (2026-11-20, opcion B elegida por el usuario): filete naranja por fuera + azul por dentro. El naranja lo separa del fondo
    // tanto oscuro (menu de la app) como claro; el azul hace juego con la franja de titulo (titulo-ventana.js). Grosor total = FILETE + AZUL.
    var FILETE_PX = 2;
    var AZUL_PX = 5;
    var MARCO_PX = FILETE_PX + AZUL_PX;
    // Grosor del agarre (incluye el marco) y lado de las esquinas.
    var AGARRE_PX = 8;
    var ESQUINA_PX = 16;
    // Si el usuario arrastro al menos esto y la ventana no cambio de tamano, el navegador no la deja redimensionar.
    var ARRASTRE_MINIMO_PX = 8;
    var Z_INDEX = 2147483000;

    // Cursor de cada agarre: n = arriba, s = abajo, e = derecha, w = izquierda y sus combinaciones (esquinas).
    var CURSORES = { n: 'ns-resize', s: 'ns-resize', e: 'ew-resize', w: 'ew-resize', ne: 'nesw-resize', sw: 'nesw-resize', nw: 'nwse-resize', se: 'nwse-resize' };

    function limitar(valor, minimo, maximo) {
        // Si no entra (maximo < minimo) gana el minimo: la ventana nunca baja del minimo.
        return Math.max(minimo, Math.min(valor, maximo));
    }

    // Nueva posicion y tamano de la ventana al arrastrar "borde" (n/s/e/w/ne/nw/se/sw) dx / dy pixeles de pantalla desde el inicio.
    //   inicio   {x, y, w, h}: posicion y tamano EXTERIOR de la ventana al empezar el arrastre.
    //   pantalla {x, y, w, h}: area disponible de la pantalla.
    //   minimo   {w, h}.
    // El borde opuesto al que se arrastra no se mueve; la ventana no sale del area disponible de la pantalla.
    function calcularGeometria(borde, inicio, dx, dy, pantalla, minimo) {
        var r = { x: inicio.x, y: inicio.y, w: inicio.w, h: inicio.h };
        if (borde.indexOf('e') >= 0) {
            r.w = limitar(inicio.w + dx, minimo.w, pantalla.x + pantalla.w - inicio.x);
        } else if (borde.indexOf('w') >= 0) {
            var derecha = inicio.x + inicio.w;
            r.w = limitar(inicio.w - dx, minimo.w, derecha - pantalla.x);
            r.x = derecha - r.w;
        }
        if (borde.indexOf('s') >= 0) {
            r.h = limitar(inicio.h + dy, minimo.h, pantalla.y + pantalla.h - inicio.y);
        } else if (borde.indexOf('n') >= 0) {
            var abajo = inicio.y + inicio.h;
            r.h = limitar(inicio.h - dy, minimo.h, abajo - pantalla.y);
            r.y = abajo - r.h;
        }
        return r;
    }

    // Nueva posicion de la ventana al arrastrarla desde la franja de titulo (titulo-ventana.js), dx / dy pixeles de pantalla desde el inicio.
    //   inicio {x, y, w, h}: posicion y tamano exterior al empezar; pantalla {x, y, w, h}: area disponible; visiblePx: lo minimo que debe quedar a la vista.
    // La ventana no sube por encima de la pantalla (la franja siempre se puede volver a agarrar) y siempre deja visible al menos "visiblePx" a los costados y abajo.
    function calcularPosicion(inicio, dx, dy, pantalla, visiblePx) {
        return {
            x: limitar(inicio.x + dx, pantalla.x - inicio.w + visiblePx, pantalla.x + pantalla.w - visiblePx),
            y: limitar(inicio.y + dy, pantalla.y, pantalla.y + pantalla.h - visiblePx)
        };
    }

    window.MarcoVentana = { calcularGeometria: calcularGeometria, calcularPosicion: calcularPosicion };

    // ---------- a partir de aca, solo navegador (los tests solo usan calcularGeometria) ----------
    if (!window.document || window !== window.top) return;

    var raiz = null;          // contenedor fijo que aloja la linea del marco y los agarres
    var arrastre = null;      // estado del arrastre en curso, o null
    var avisoDado = false;

    function avisar(texto) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({ toast: true, position: 'top', icon: 'info', title: texto, showConfirmButton: false, timer: 3000 });
        } else {
            window.alert(texto);
        }
    }

    function pantallaDisponible() {
        var s = window.screen || {};
        return { x: s.availLeft || 0, y: s.availTop || 0, w: s.availWidth || window.outerWidth, h: s.availHeight || window.outerHeight };
    }

    window.MarcoVentana.pantallaDisponible = pantallaDisponible; // lo usa titulo-ventana.js para mover la ventana

    function inyectarEstilos() {
        var g = AGARRE_PX + 'px', e = ESQUINA_PX + 'px', m = MARCO_PX + 'px';
        var estilo = document.createElement('style');
        estilo.id = 'vt-marco-estilos';
        estilo.textContent =
            '#vtMarco{position:fixed;top:0;left:0;right:var(--vt-sbx,0px);bottom:var(--vt-sby,0px);pointer-events:none;z-index:' + Z_INDEX + '}' +
            '#vtMarco .vt-linea{position:absolute;top:0;left:0;right:0;bottom:0;box-sizing:border-box;border:' + FILETE_PX + 'px solid #e19143}' +
            '#vtMarco .vt-linea::after{content:"";position:absolute;top:0;left:0;right:0;bottom:0;border:' + AZUL_PX + 'px solid #35479b}' +
            '#vtMarco .vt-agarre{position:absolute;pointer-events:auto;touch-action:none}' +
            // Resaltado al pasar el cursor: blanco (el naranja ya es el filete exterior del marco).
            '#vtMarco .vt-agarre::before{content:"";position:absolute;background:#fff;opacity:0;transition:opacity .12s}' +
            '#vtMarco .vt-agarre:hover::before,#vtMarco .vt-agarre.vt-activo::before{opacity:.95}' +
            '#vtMarco .vt-g-n,#vtMarco .vt-g-s{left:' + e + ';right:' + e + ';height:' + g + '}' +
            '#vtMarco .vt-g-w,#vtMarco .vt-g-e{top:' + e + ';bottom:' + e + ';width:' + g + '}' +
            '#vtMarco .vt-g-n{top:0}#vtMarco .vt-g-s{bottom:0}#vtMarco .vt-g-w{left:0}#vtMarco .vt-g-e{right:0}' +
            '#vtMarco .vt-g-n::before,#vtMarco .vt-g-s::before{left:0;right:0;height:' + m + '}' +
            '#vtMarco .vt-g-n::before{top:0}#vtMarco .vt-g-s::before{bottom:0}' +
            '#vtMarco .vt-g-w::before,#vtMarco .vt-g-e::before{top:0;bottom:0;width:' + m + '}' +
            '#vtMarco .vt-g-w::before{left:0}#vtMarco .vt-g-e::before{right:0}' +
            '#vtMarco .vt-g-ne,#vtMarco .vt-g-nw,#vtMarco .vt-g-se,#vtMarco .vt-g-sw{width:' + e + ';height:' + e + '}' +
            '#vtMarco .vt-g-ne::before,#vtMarco .vt-g-nw::before,#vtMarco .vt-g-se::before,#vtMarco .vt-g-sw::before{width:100%;height:100%}' +
            '#vtMarco .vt-g-ne{top:0;right:0}#vtMarco .vt-g-nw{top:0;left:0}#vtMarco .vt-g-se{bottom:0;right:0}#vtMarco .vt-g-sw{bottom:0;left:0}' +
            'html.dark-mode #vtMarco .vt-linea::after{border-color:#5b6fd6}' +
            'html.vt-arrastrando,html.vt-arrastrando *{user-select:none!important;-webkit-user-select:none!important}' +
            '#vtMarco.vt-oculto{display:none}' +
            '@media print{#vtMarco{display:none}}';
        document.head.appendChild(estilo);
    }

    // Corre el marco hacia adentro el ancho de la barra de desplazamiento (si la hay) para no taparla ni robarle los clics.
    function ajustarBarras() {
        var raizHtml = document.documentElement;
        var sbx = Math.max(0, window.innerWidth - raizHtml.clientWidth);
        var sby = Math.max(0, window.innerHeight - raizHtml.clientHeight);
        raizHtml.style.setProperty('--vt-sbx', sbx + 'px');
        raizHtml.style.setProperty('--vt-sby', sby + 'px');
    }

    // ---------- arrastre ----------
    var cuadroPendiente = null; // ultimo pointermove sin aplicar (se aplica una vez por cuadro de animacion)

    function aplicarArrastre() {
        var evento = cuadroPendiente;
        cuadroPendiente = null;
        if (!arrastre || !evento) return;
        var g = calcularGeometria(arrastre.borde, arrastre.inicio, evento.screenX - arrastre.sx, evento.screenY - arrastre.sy,
            pantallaDisponible(), { w: MIN_ANCHO_PX, h: MIN_ALTO_PX });
        try {
            window.resizeTo(Math.round(g.w), Math.round(g.h));
            // Solo se mueve cuando el arrastre es por el lado izquierdo / superior (cambia el origen de la ventana).
            if (g.x !== arrastre.inicio.x || g.y !== arrastre.inicio.y) window.moveTo(Math.round(g.x), Math.round(g.y));
        } catch (error) {
            console.warn('[marco-ventana] no se pudo redimensionar la ventana', error);
        }
    }

    function alMover(evento) {
        if (!arrastre) return;
        var primero = cuadroPendiente === null;
        cuadroPendiente = evento;
        if (primero) window.requestAnimationFrame(aplicarArrastre);
    }

    function terminarArrastre(evento) {
        if (!arrastre) return;
        var agarre = arrastre.agarre;
        var movio = Math.abs(evento.screenX - arrastre.sx) >= ARRASTRE_MINIMO_PX || Math.abs(evento.screenY - arrastre.sy) >= ARRASTRE_MINIMO_PX;
        var inicio = arrastre.inicio;
        agarre.classList.remove('vt-activo');
        document.documentElement.classList.remove('vt-arrastrando');
        agarre.removeEventListener('pointermove', alMover);
        try { agarre.releasePointerCapture(evento.pointerId); } catch (e) { /* ya liberado */ }
        arrastre = null;
        cuadroPendiente = null;
        // El navegador aplica el resizeTo un instante despues: se mide con una pequena espera.
        window.setTimeout(function () {
            var cambio = Math.abs(window.outerWidth - inicio.w) >= 2 || Math.abs(window.outerHeight - inicio.h) >= 2;
            if (movio && !cambio && !avisoDado) {
                avisoDado = true;
                avisar('El navegador no permite cambiar el tamaño de esta ventana.');
            }
        }, 400);
    }

    function alPresionar(evento) {
        if (evento.button !== 0) return;
        var agarre = evento.currentTarget;
        evento.preventDefault();
        arrastre = {
            borde: agarre.getAttribute('data-borde'),
            agarre: agarre,
            // screenX/Y no cambian cuando la propia ventana se mueve (no hay realimentacion como con clientX/Y).
            sx: evento.screenX,
            sy: evento.screenY,
            inicio: { x: window.screenX, y: window.screenY, w: window.outerWidth, h: window.outerHeight }
        };
        agarre.classList.add('vt-activo');
        document.documentElement.classList.add('vt-arrastrando');
        try { agarre.setPointerCapture(evento.pointerId); } catch (e) { /* sin captura: el arrastre sigue mientras el cursor este sobre el agarre */ }
        agarre.addEventListener('pointermove', alMover);
    }

    function crearMarco() {
        inyectarEstilos();
        raiz = document.createElement('div');
        raiz.id = 'vtMarco';
        raiz.setAttribute('aria-hidden', 'true');
        var linea = document.createElement('div');
        linea.className = 'vt-linea';
        raiz.appendChild(linea);
        Object.keys(CURSORES).forEach(function (borde) {
            var agarre = document.createElement('div');
            agarre.className = 'vt-agarre vt-g-' + borde;
            agarre.setAttribute('data-borde', borde);
            agarre.style.cursor = CURSORES[borde];
            agarre.addEventListener('pointerdown', alPresionar);
            agarre.addEventListener('pointerup', terminarArrastre);
            agarre.addEventListener('pointercancel', terminarArrastre);
            raiz.appendChild(agarre);
        });
        document.body.appendChild(raiz);

        // Pantalla completa (boton «Pantalla completa» de las pantallas de trabajo): sin marco.
        document.addEventListener('fullscreenchange', function () {
            raiz.classList.toggle('vt-oculto', !!document.fullscreenElement);
        });

        // La barra de desplazamiento aparece / desaparece segun el contenido y el tamano de la ventana.
        var pendiente = false;
        function reajustar() {
            if (pendiente) return;
            pendiente = true;
            window.requestAnimationFrame(function () { pendiente = false; ajustarBarras(); });
        }
        window.addEventListener('resize', reajustar);
        if (typeof window.ResizeObserver === 'function') {
            var observador = new window.ResizeObserver(reajustar);
            observador.observe(document.documentElement);
            observador.observe(document.body);
        }
        ajustarBarras();
    }

    document.addEventListener('DOMContentLoaded', function () {
        // VentanaTamano.aplica() decide si esta ventana es independiente (ver ventana-tamano.js); sin el, no hay marco.
        if (!window.VentanaTamano || typeof window.VentanaTamano.aplica !== 'function' || !window.VentanaTamano.aplica()) return;
        crearMarco();
    });
})(window);
