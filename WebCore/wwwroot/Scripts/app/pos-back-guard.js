// Guardia del boton "Atras" del dispositivo en el POS (Ventas y Expendios, via _LayoutPOS.cshtml).
//
// Problema: en responsive, con un modal abierto, Atras disparaba el cartel nativo de salir del sitio
// (beforeunload de pos-cart.js) y, al aceptar, sacaba al usuario del POS. Decision (docs/DECISIONS.md,
// 2026-10-12): en el POS Atras se IGNORA siempre (haya modal o no) y se avisa que la salida es el boton "Salir".
//
// Tecnica: se deja el historial como [base, sentinel] y se queda parado en el sentinel. Si el usuario toca
// Atras cae en "base": se vuelve con history.forward() (en vez de re-pushState) porque Chrome Android saltea
// las entradas agregadas sin gesto del usuario; por eso el sentinel se arma recien en la primera interaccion.
// Sin dependencias (ni jQuery). Idempotente.
(function (window, document) {
    'use strict';

    if (window.PosBackGuard) return;

    var ESTADO_BASE = 'base';
    var ESTADO_SENTINEL = 'sentinel';
    var DURACION_AVISO_MS = 4000;
    var Z_INDEX_AVISO = 3000; // por encima de los modales (custom.css los fija en 1050; modal-stack.js suma de a 10)
    var MENSAJE = 'El botón Atrás está desactivado en el POS. Para salir presione el botón "Salir" (←) de la barra superior.';

    var armado = false;
    var avisoEl = null;
    var avisoTimer = null;

    function marca(estado) {
        return { posGuard: estado };
    }

    // Aviso propio en el DOM (no SweetAlert: Swal es de instancia unica y pisaria un confirm que este abierto).
    function mostrarAviso() {
        if (!avisoEl) {
            avisoEl = document.createElement('div');
            avisoEl.setAttribute('role', 'alert');
            avisoEl.style.cssText = 'position:fixed;left:50%;bottom:24px;transform:translateX(-50%);'
                + 'width:calc(100% - 32px);max-width:420px;padding:12px 16px;border-radius:8px;'
                + 'background:#212529;color:#fff;font-size:.95rem;line-height:1.35;text-align:center;'
                + 'box-shadow:0 4px 16px rgba(0,0,0,.4);pointer-events:none;z-index:' + Z_INDEX_AVISO;
            avisoEl.textContent = MENSAJE;
            document.body.appendChild(avisoEl);
        }

        avisoEl.style.display = 'block';
        window.clearTimeout(avisoTimer);
        avisoTimer = window.setTimeout(function () {
            if (avisoEl) avisoEl.style.display = 'none';
        }, DURACION_AVISO_MS);
    }

    // Arma [base, sentinel] una sola vez, en la primera interaccion real del usuario.
    function armar() {
        if (armado) return;
        armado = true;

        try {
            // replaceState/pushState conservan la URL actual (incluidos modoPos/posInstanceId de pos-multi-instance.js).
            window.history.replaceState(marca(ESTADO_BASE), document.title, window.location.href);
            window.history.pushState(marca(ESTADO_SENTINEL), document.title, window.location.href);
        } catch (e) {
            // HACK: sin History API usable no hay trampa; el POS queda como antes (con el aviso de beforeunload).
            if (window.console) console.warn('PosBackGuard: no se pudo armar la guardia de Atras', e);
        }
    }

    window.addEventListener('popstate', function (e) {
        var posGuard = e.state && e.state.posGuard;

        // Cayo en la entrada base = el usuario toco Atras: volver al sentinel y avisar.
        // Cualquier otro estado (sentinel por nuestro forward, null por cambio de #ancla) no es un Atras.
        if (posGuard === ESTADO_BASE) {
            window.history.forward();
            mostrarAviso();
        }
    });

    ['pointerdown', 'touchstart', 'keydown'].forEach(function (tipo) {
        document.addEventListener(tipo, armar, { once: true, passive: true, capture: true });
    });

    window.PosBackGuard = { mostrarAviso: mostrarAviso };
})(window, document);
