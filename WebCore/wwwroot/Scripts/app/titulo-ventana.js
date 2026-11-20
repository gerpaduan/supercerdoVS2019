/* Titulo claro para las ventanas independientes (2026-11-20, pedido del usuario: que se vea bien cual es la ventana, por ejemplo «Stock» o «Movimientos»).
   Dos cosas, solo en una ventana independiente (la misma condicion que el marco y los − / +: VentanaTamano.aplica(), de ventana-tamano.js, que debe cargarse ANTES):
   1) Titulo del sistema (barra de titulo, Alt+Tab, mouse sobre la barra de tareas): «Seccion · Pantalla», sin el « - CarniSys» del final que lo cortaba
      («Stock · Gestión de stock», «Cajas · Cajas abiertas»). Si la pantalla se llama igual que la seccion, se dice una sola vez.
   2) Franja de titulo dentro de la ventana (solo en _Layout, que es el que oculta la barra superior en modo ventana): 28 px visibles con el icono, el nombre de la
      seccion y la pantalla al lado. Va EN el flujo (primer hijo de #content-wrapper, que es una columna flex): las pantallas de trabajo, que fijan #content-wrapper
      a 100dvh, le restan ese alto a la tabla; el resto crece sin cambios. Arriba lleva 7 px de relleno (filete naranja 2 + azul 5) porque el marco (marco-ventana.js) dibuja su linea encima.
   La franja tambien sirve para MOVER la ventana arrastrandola (window.moveTo, ver habilitarArrastre).
   La seccion y el icono los emite _SeccionVentana.cshtml (<meta name="carnisys-seccion">). Decision: docs/DECISIONS.md (2026-11-20 d). */
(function (window) {
    'use strict';

    // Alto de la franja (incluye los 6 px que tapa la linea del marco) y separador entre seccion y pantalla en el titulo del sistema.
    var FRANJA_ALTO_PX = 35;
    var FRANJA_RELLENO_SUPERIOR_PX = 7;
    var SEPARADOR_TITULO = ' · ';
    // Al arrastrar la franja: lo minimo de la ventana que debe quedar a la vista, y cuanto hay que mover para considerar que el usuario quiso arrastrar.
    var VISIBLE_MINIMO_PX = 120;
    var ARRASTRE_MINIMO_PX = 8;

    // Sin acentos ni mayusculas, para comparar «Gestión de stock» con «gestion de stock».
    function normalizar(texto) {
        return String(texto || '').normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().trim();
    }

    // Titulo de la pantalla sin el « - CarniSys» final ni una seccion ya antepuesta («Movimientos · Nuevo…»).
    function tituloDePantalla(seccion) {
        var titulo = String(document.title || '').replace(/\s*-\s*CarniSys\s*$/i, '').trim();
        var prefijo = seccion + SEPARADOR_TITULO;
        if (titulo.indexOf(prefijo) === 0) titulo = titulo.substring(prefijo.length).trim();
        return titulo;
    }

    function inyectarEstilos() {
        var estilo = document.createElement('style');
        estilo.id = 'vt-titulo-estilos';
        estilo.textContent =
            '.vt-titulo{display:flex;align-items:center;gap:.65rem;flex:0 0 auto;box-sizing:border-box;height:' + FRANJA_ALTO_PX + 'px;' +
            'padding:' + FRANJA_RELLENO_SUPERIOR_PX + 'px 14px 0;background:#35479b;color:#fff;overflow:hidden;white-space:nowrap;' +
            'cursor:move;user-select:none;-webkit-user-select:none;touch-action:none}' +
            '.vt-titulo i{font-size:1rem;flex:none}' +
            '.vt-titulo b{font-size:1rem;font-weight:600;flex:none}' +
            '.vt-titulo span{font-size:.8rem;opacity:.85;padding-left:.65rem;border-left:1px solid rgba(255,255,255,.4);overflow:hidden;text-overflow:ellipsis}' +
            'html.dark-mode .vt-titulo{background:#3b4a9c}' +
            '@media print{.vt-titulo{display:none}}';
        document.head.appendChild(estilo);
    }

    function avisar(texto) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({ toast: true, position: 'top', icon: 'info', title: texto, showConfirmButton: false, timer: 3000 });
        } else {
            window.alert(texto);
        }
    }

    // Mover la ventana arrastrando la franja (2026-11-20, pedido del usuario: es lo mas visible y practico para agarrar). Una pagina no puede arrastrar
    // la ventana como la barra del sistema, asi que se sigue el cursor con window.moveTo (mismo mecanismo y pointer events con captura que los agarres
    // del marco, marco-ventana.js); la posicion sale de MarcoVentana.calcularPosicion (con tests). Si el navegador no la mueve (ventana maximizada o
    // sin permiso), avisa una vez.
    function habilitarArrastre(franja) {
        var marco = window.MarcoVentana;
        if (!marco || typeof marco.calcularPosicion !== 'function' || typeof marco.pantallaDisponible !== 'function') return;
        var arrastre = null;     // { sx, sy, inicio } del arrastre en curso
        var pendiente = null;    // ultimo pointermove sin aplicar (se aplica una vez por cuadro de animacion)
        var avisoDado = false;
        franja.title = 'Arrastrá para mover la ventana';

        function aplicar() {
            var evento = pendiente;
            pendiente = null;
            if (!arrastre || !evento) return;
            var p = marco.calcularPosicion(arrastre.inicio, evento.screenX - arrastre.sx, evento.screenY - arrastre.sy, marco.pantallaDisponible(), VISIBLE_MINIMO_PX);
            try { window.moveTo(Math.round(p.x), Math.round(p.y)); } catch (error) { console.warn('[titulo-ventana] no se pudo mover la ventana', error); }
        }

        franja.addEventListener('pointerdown', function (evento) {
            if (evento.button !== 0) return;
            evento.preventDefault();
            // screenX/Y no cambian cuando la propia ventana se mueve (sin realimentacion como con clientX/Y).
            arrastre = { sx: evento.screenX, sy: evento.screenY, inicio: { x: window.screenX, y: window.screenY, w: window.outerWidth, h: window.outerHeight } };
            try { franja.setPointerCapture(evento.pointerId); } catch (e) { /* sin captura: sigue mientras el cursor este sobre la franja */ }
            document.documentElement.classList.add('vt-arrastrando');
        });
        franja.addEventListener('pointermove', function (evento) {
            if (!arrastre) return;
            var primero = pendiente === null;
            pendiente = evento;
            if (primero) window.requestAnimationFrame(aplicar);
        });
        function terminar(evento) {
            if (!arrastre) return;
            var inicio = arrastre.inicio;
            var movio = Math.abs(evento.screenX - arrastre.sx) >= ARRASTRE_MINIMO_PX || Math.abs(evento.screenY - arrastre.sy) >= ARRASTRE_MINIMO_PX;
            try { franja.releasePointerCapture(evento.pointerId); } catch (e) { /* ya liberado */ }
            document.documentElement.classList.remove('vt-arrastrando');
            arrastre = null;
            pendiente = null;
            // El navegador aplica el moveTo un instante despues: se mide con una pequena espera.
            window.setTimeout(function () {
                var cambio = Math.abs(window.screenX - inicio.x) >= 2 || Math.abs(window.screenY - inicio.y) >= 2;
                if (movio && !cambio && !avisoDado) {
                    avisoDado = true;
                    avisar('El navegador no permite mover esta ventana.');
                }
            }, 400);
        }
        franja.addEventListener('pointerup', terminar);
        franja.addEventListener('pointercancel', terminar);
    }

    function crearFranja(seccion, icono, pantalla) {
        var franja = document.createElement('div');
        franja.className = 'vt-titulo';
        franja.setAttribute('role', 'banner');
        habilitarArrastre(franja);
        var i = document.createElement('i');
        i.className = 'fas ' + icono;
        i.setAttribute('aria-hidden', 'true');
        franja.appendChild(i);
        var nombre = document.createElement('b');
        nombre.textContent = seccion;
        franja.appendChild(nombre);
        if (pantalla) {
            var detalle = document.createElement('span');
            detalle.textContent = pantalla;
            franja.appendChild(detalle);
        }
        return franja;
    }

    document.addEventListener('DOMContentLoaded', function () {
        if (window !== window.top) return; // iframes (cargas del POS, panel de stock del destino): no
        if (!window.VentanaTamano || typeof window.VentanaTamano.aplica !== 'function' || !window.VentanaTamano.aplica()) return;

        var meta = document.querySelector('meta[name="carnisys-seccion"]');
        var seccion = meta ? (meta.getAttribute('content') || '').trim() : '';
        if (!seccion) return; // sin dato de seccion se deja el titulo como esta

        var pantalla = tituloDePantalla(seccion);
        if (normalizar(pantalla) === normalizar(seccion)) pantalla = '';
        document.title = pantalla ? seccion + SEPARADOR_TITULO + pantalla : seccion;

        // Franja: solo donde hay #content-wrapper (_Layout) y la barra superior esta oculta por el modo ventana.
        var envoltorio = document.getElementById('content-wrapper');
        if (!envoltorio || !document.documentElement.classList.contains('modo-ventana')) return;
        inyectarEstilos();
        envoltorio.insertBefore(crearFranja(seccion, (meta.getAttribute('data-icono') || 'fa-window-maximize'), pantalla), envoltorio.firstChild);
    });
})(window);
