/*
 * Bloqueo por inactividad del POS (2026-10-15, ver docs/DECISIONS.md "Bloqueo por inactividad").
 * Tras N minutos sin actividad (mouse, teclado, touch) tapa el POS con un overlay que pide la clave del
 * VENDEDOR ACTIVO. No navega ni recarga: el carrito en memoria queda intacto, y antes de bloquear se
 * fuerza un guardado (POSDraft local + borrador en el servidor).
 *
 * Desbloqueo: lo valida el servidor (VentasController.DesbloquearPOS); la clave de otro usuario no sirve.
 * Otro vendedor que necesite la PC:
 *  - cuenta de produccion: "Cambiar de vendedor" (modal de seleccion de usuario ya existente). La venta
 *    bloqueada queda resguardada en el servidor ("Ver ventas sin cerrar", F1: solo la retoma su dueno o un
 *    supervisor) y el carrito local se limpia para que el nuevo vendedor no lo herede.
 *  - usuario normal: "Cerrar sesion" (el pagehide del borrador deja constancia de la venta).
 * Mientras esta bloqueado: window.POSBloqueado = true, el resto de la pagina queda inert y se frena el
 * teclado fuera del overlay (incluye los atajos del POS y el lector de codigo de barras).
 * Riesgo aceptado: es un bloqueo de cliente (ver DECISIONS.md): alguien con las herramientas de desarrollo
 * podria saltarlo. La amenaza que cubre es el companero que pasa por la PC, no un atacante.
 * Config en window.POSBloqueoConfig (la arma _BloqueoPOS.cshtml; sin config no hace nada).
 */
(function (window, $) {
    'use strict';

    var cfg = window.POSBloqueoConfig;
    if (!cfg || !cfg.activo || !(cfg.minutos > 0)) return;

    var MS_MINUTO = 60 * 1000;
    var limiteMs = cfg.minutos * MS_MINUTO;
    var CHEQUEO_MS = 10 * 1000;

    var overlay = document.getElementById('posBloqueoOverlay');
    if (!overlay) return;
    // Hijo directo del body: aplicarInert() marca el resto de los hijos del body y el overlay no puede quedar
    // dentro de ninguno de ellos.
    if (overlay.parentElement !== document.body) document.body.appendChild(overlay);

    var bloqueado = false;
    var inertDelModal = null;
    var ultimaActividad = Date.now();
    var inertAplicados = [];
    var timerEspera = null;
    var validando = false;

    // ---- Actividad ----------------------------------------------------------------------------
    function alActividad() {
        if (!bloqueado) ultimaActividad = Date.now();
    }
    ['mousemove', 'mousedown', 'keydown', 'touchstart', 'wheel', 'scroll'].forEach(function (evento) {
        document.addEventListener(evento, alActividad, { passive: true, capture: true });
    });

    // ---- Aislamiento mientras esta bloqueado -------------------------------------------------
    function permitido(elemento) {
        if (!elemento || !elemento.closest) return false;
        // El modal de seleccion de usuario ("Cambiar de vendedor") convive con el overlay.
        return overlay.contains(elemento) || !!elemento.closest('#modalSeleccionUsuario');
    }

    // Teclado fuera del overlay: se descarta antes de que lo vean los atajos del POS o el lector de barras.
    ['keydown', 'keypress', 'keyup', 'beforeinput', 'paste'].forEach(function (tipo) {
        window.addEventListener(tipo, function (e) {
            if (!bloqueado || permitido(e.target)) return;
            e.preventDefault();
            e.stopImmediatePropagation();
        }, true);
    });

    // Si el foco se escapa a un elemento de la pagina, vuelve al contenedor del overlay (no a un input).
    document.addEventListener('focusin', function (e) {
        if (!bloqueado || permitido(e.target)) return;
        overlay.focus();
    }, true);

    // inert saca del foco/teclado/lector de pantalla todo lo que hay detras. Solo se marcan los hijos
    // directos del body que no tenian inert, para devolver el estado exacto al desbloquear.
    function aplicarInert() {
        inertAplicados = [];
        Array.prototype.forEach.call(document.body.children, function (hijo) {
            if (hijo === overlay || hijo.id === 'modalSeleccionUsuario') return;
            if (hijo.hasAttribute('inert')) return;
            hijo.setAttribute('inert', '');
            inertAplicados.push(hijo);
        });
    }
    function quitarInert() {
        inertAplicados.forEach(function (hijo) { hijo.removeAttribute('inert'); });
        inertAplicados = [];
        inertDelModal = null;
    }

    // Hijo directo del body que contiene (o es) el modal de seleccion de usuario. Mientras el modal esta abierto
    // ese contenedor no puede estar inert o el modal no recibiria clics ni teclado; el overlay (debajo del
    // backdrop del modal) sigue tapando y bloqueando el resto del POS.
    function contenedorDelModal() {
        var modal = document.getElementById('modalSeleccionUsuario');
        if (!modal) return null;
        var hijo = modal;
        while (hijo.parentElement && hijo.parentElement !== document.body) hijo = hijo.parentElement;
        return hijo;
    }
    function liberarInertDelModal() {
        var contenedor = contenedorDelModal();
        if (contenedor && contenedor.hasAttribute('inert') && inertAplicados.indexOf(contenedor) >= 0) {
            contenedor.removeAttribute('inert');
            inertDelModal = contenedor;
        }
    }
    function reponerInertDelModal() {
        if (inertDelModal) {
            inertDelModal.setAttribute('inert', '');
            inertDelModal = null;
        }
    }

    // ---- Resguardo de la venta ----------------------------------------------------------------
    function hayCarrito() {
        var lineas = (window.POSState && typeof window.POSState.getLineas === 'function' && window.POSState.getLineas()) || [];
        return lineas.some(function (l) { return !!l; });
    }

    // Resuelve true si la venta en curso esta a salvo en el servidor (o no hay venta).
    function resguardarVenta() {
        if (!hayCarrito()) return Promise.resolve(true);
        var borrador = window.POSBorrador;
        if (borrador && typeof borrador.estaHabilitado === 'function' && borrador.estaHabilitado()
            && typeof borrador.guardarAhora === 'function') {
            return borrador.guardarAhora();
        }
        return Promise.resolve(false);
    }

    // ---- Bloquear / desbloquear ---------------------------------------------------------------
    function bloquear() {
        if (bloqueado) return;
        bloqueado = true;
        window.POSBloqueado = true;

        // Respaldo local inmediato + guardado en el servidor (mejor esfuerzo; sigue latiendo cada 20 s).
        try { if (window.POSDraft && typeof window.POSDraft.save === 'function') window.POSDraft.save(); } catch (e) { /* respaldo local: no frena el bloqueo */ }
        resguardarVenta();

        if (document.activeElement && typeof document.activeElement.blur === 'function') document.activeElement.blur();
        aplicarInert();
        $('#posBloqueoClave').val('');
        ocultarError();
        overlay.classList.remove('d-none', 'pos-bloqueo-bajo');
        overlay.focus();
    }

    function ocultarOverlay() {
        bloqueado = false;
        window.POSBloqueado = false;
        quitarInert();
        overlay.classList.add('d-none');
        $('#posBloqueoClave').val('');
        ultimaActividad = Date.now();
    }

    function mostrarError(msg) {
        $('#posBloqueoError').removeClass('d-none').text(msg);
    }
    function ocultarError() {
        $('#posBloqueoError').addClass('d-none').text('');
    }

    function mostrarEspera(segundos) {
        clearInterval(timerEspera);
        var restante = Math.max(0, parseInt(segundos, 10) || 0);
        $('#posBloqueoForm').addClass('d-none');
        $('#posBloqueoEspera').removeClass('d-none');

        function tick() {
            var m = Math.floor(restante / 60);
            var s = restante % 60;
            $('#posBloqueoCuenta').text(m + ':' + (s < 10 ? '0' : '') + s);
            if (restante <= 0) {
                clearInterval(timerEspera);
                $('#posBloqueoEspera').addClass('d-none');
                $('#posBloqueoForm').removeClass('d-none');
                return;
            }
            restante--;
        }
        tick();
        timerEspera = setInterval(tick, 1000);
    }

    // Recarga el POS conservando la instancia (un reload plano genera un posInstanceId nuevo y, en cuentas de
    // produccion, perderia el operador autorizado). Mismo criterio que recargarConOperadorAutorizado de POS.cshtml.
    function recargarMismaInstancia() {
        var url = new URL(window.location.href);
        if (cfg.posInstanceId) url.searchParams.set('posInstanceId', cfg.posInstanceId);
        window.PosNavegandoInternoPOS = true;
        window.location.href = url.toString();
    }

    function desbloquear(e) {
        if (e) e.preventDefault();
        if (!bloqueado || validando) return;

        var clave = $('#posBloqueoClave').val() || '';
        if (!clave) {
            mostrarError('Ingresá tu contraseña.');
            $('#posBloqueoClave').trigger('focus');
            return;
        }

        validando = true;
        ocultarError();
        $('#posBloqueoBtnDesbloquear').prop('disabled', true);

        $.ajax({
            url: cfg.urlDesbloquear,
            type: 'POST',
            data: {
                clave: clave,
                posInstanceId: cfg.posInstanceId,
                validarCaja: cfg.validarCaja,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
            }
        }).done(function (resp) {
            if (resp && resp.ok) {
                // Si mientras estaba bloqueado le cerraron la caja, se recarga para que rija el gate de caja normal
                // (modal de abrir caja) en vez de dejarlo cargar productos y fallar recien al cobrar.
                if (cfg.validarCaja && cfg.cajaAbiertaAlCargar && resp.cajaAbierta === false) {
                    recargarMismaInstancia();
                    return;
                }
                ocultarOverlay();
                return;
            }
            if (resp && resp.requiereOperador) {
                // La autorizacion del operador vencio en el servidor: se vuelve a pedir desde cero (el carrito se restaura).
                recargarMismaInstancia();
                return;
            }
            if (resp && resp.bloqueado) {
                mostrarEspera(resp.segundosRestantes || 0);
                return;
            }
            mostrarError((resp && resp.msg) || 'Contraseña incorrecta.');
            $('#posBloqueoClave').val('').trigger('focus');
        }).fail(function () {
            mostrarError('No se pudo validar la contraseña. Revisá la conexión e intentá de nuevo.');
        }).always(function () {
            validando = false;
            $('#posBloqueoBtnDesbloquear').prop('disabled', false);
        });
    }

    // ---- Otro vendedor ------------------------------------------------------------------------
    // Cuenta de produccion: reusa el modal de seleccion de usuario del "Cambiar operario". El overlay baja de
    // z-index (queda debajo del modal pero sigue tapando el POS) y vuelve a subir si se cancela.
    function cambiarVendedor() {
        var operadorCfg = window.PosOperadorConfig;
        if (!window.SeleccionUsuario || !operadorCfg) {
            mostrarError('No se pudo abrir el cambio de vendedor. Recargá la página.');
            return;
        }

        ocultarError();
        resguardarVenta().then(function (aSalvo) {
            // Sin resguardo en el servidor el nuevo vendedor no puede tomar la PC: se perderia la venta.
            if (!aSalvo) {
                mostrarError('No se pudo resguardar la venta en curso. El vendedor debe desbloquear, o revisá la conexión.');
                return;
            }

            overlay.classList.add('pos-bloqueo-bajo');
            liberarInertDelModal();
            // La venta ya esta a salvo en el servidor. Desde que el servidor registra al nuevo operador hasta que la
            // pagina se recarga, un latido o guardado de pos-borrador.js volveria a subir el carrito que sigue en
            // memoria, ahora a nombre del NUEVO vendedor (borrador duplicado con la venta del otro). Con
            // POSFinalizandoVenta el guardado queda frenado (pos-borrador.js finalizando()); se repone si se cancela.
            window.POSFinalizandoVenta = true;
            window.SeleccionUsuario.abrir({
                titulo: 'Cambiar de vendedor',
                usuarios: operadorCfg.usuariosActivos,
                requierePassword: true,
                validarUrl: operadorCfg.urlAutorizar + '?posInstanceId=' + encodeURIComponent(cfg.posInstanceId),
                obligatorio: false,
                alCancelar: function () {
                    window.POSFinalizandoVenta = false;
                    reponerInertDelModal();
                    overlay.classList.remove('pos-bloqueo-bajo');
                    overlay.focus();
                }
            }).then(function () {
                // El nuevo vendedor ya se autentico. La venta anterior esta en el servidor: se limpia el carrito
                // local para que no la herede (POSDraft.restore() la cargaria a su nombre al recargar).
                try { if (window.POSDraft && typeof window.POSDraft.clear === 'function') window.POSDraft.clear(); } catch (e) { /* ya esta a salvo en el servidor */ }
                if (typeof window.desactivarAvisoSalidaPOS === 'function') window.desactivarAvisoSalidaPOS();
                recargarMismaInstancia();
            });
        });
    }

    // Usuario normal: cerrar sesion. El pagehide de pos-borrador.js manda el carrito completo antes de irse.
    function cerrarSesion() {
        ocultarError();
        resguardarVenta().then(function () {
            if (typeof window.desactivarAvisoSalidaPOS === 'function') window.desactivarAvisoSalidaPOS();
            window.location.href = cfg.urlLogout;
        });
    }

    // ---- Chequeo de inactividad ---------------------------------------------------------------
    function chequear() {
        if (bloqueado) return;
        // No se bloquea en medio de un cobro (hay un request en vuelo que no conviene interrumpir).
        if (window.POSFinalizandoVenta === true) { ultimaActividad = Date.now(); return; }
        if (Date.now() - ultimaActividad >= limiteMs) bloquear();
    }

    $('#posBloqueoForm').on('submit', desbloquear);
    $('#posBloqueoBtnCambiar').on('click', cambiarVendedor);
    $('#posBloqueoBtnSalir').on('click', cerrarSesion);

    setInterval(chequear, CHEQUEO_MS);
    // Al volver a la pestana (PC suspendida, cambio de ventana) se evalua ya, sin esperar el proximo ciclo.
    document.addEventListener('visibilitychange', function () {
        if (document.visibilityState === 'visible') chequear();
    });
})(window, window.jQuery);
