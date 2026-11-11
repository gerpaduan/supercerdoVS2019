/*
 * Vigilancia de caja del POS (2026-11-11, ver docs/DECISIONS.md "Vigilancia de caja del POS").
 * Problema: si le cierran la caja al cajero desde otra terminal, su POS seguia mostrando una pantalla de venta normal y
 * dejaba cargar (y hasta registrar) ventas contra una caja ya cerrada. El POS solo validaba la caja al cargar la pagina.
 *
 * Que hace:
 *  - Cada N segundos (PosCajaVigilante:Segundos), al volver a la pestana y al recuperar el foco, le pregunta al servidor
 *    (VentasController.EstadoCajaPOS) si la caja del vendedor sigue abierta. Solo lectura.
 *  - Si ya no esta abierta (o FinalizarVenta rechazo la venta con cajaCerrada), CORTA: bloquea teclado y lector de codigos,
 *    resguarda la venta en curso (local + servidor, marcada como interrumpida para que se pueda recuperar al instante),
 *    avisa con un cartel que no se puede cerrar y recarga el POS, que pide abrir caja de nuevo. La venta se recupera con la
 *    caja nueva (el mismo cajero la retoma sola al recargar; otro cajero, por Ayuda F1 -> Ver ventas sin guardar).
 *
 * Reglas que conviene no romper:
 *  - Un fallo de red, una sesion caida o una respuesta que no es el JSON esperado NUNCA corta la venta: se ignora y se
 *    reintenta en el proximo ciclo (la verdad la sigue teniendo el servidor al guardar la venta).
 *  - No se chequea con un cobro en curso (POSFinalizandoVenta / POSVentaFinalizada) ni con el POS bloqueado por inactividad
 *    (pos-bloqueo.js ya recarga al desbloquear si la caja se cerro).
 * Config en window.POSCajaVigilanteConfig (la arma Views/Ventas/POS.cshtml solo cuando hay caja real abierta y no es una
 * edicion de venta; sin config no hace nada). Depende de jQuery y SweetAlert2.
 */
(function (window, $) {
    'use strict';

    var cfg = window.POSCajaVigilanteConfig;
    if (!cfg || !cfg.urlEstado || !(cfg.segundos > 0) || window.POSCajaVigilante) return;

    var MS_SEGUNDO = 1000;

    var cortado = false;      // ya se disparo el corte: no se vuelve a chequear ni a cortar
    var verificando = false;  // hay una consulta en vuelo
    var timer = null;

    function token() {
        var input = document.querySelector('#globalAntiForgeryToken input[name="__RequestVerificationToken"]')
            || document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function posInstanceId() {
        return (window.POSModo && window.POSModo.instanceId) || cfg.posInstanceId || '';
    }

    // ---- Bloqueo de entrada una vez cortado -----------------------------------------------------
    // El cartel de SweetAlert tapa el mouse, pero el lector de codigos de barras y los atajos del POS escuchan el teclado
    // del documento: se descartan en captura todos los eventos que no sean del propio cartel.
    function entradaDelCartel(elemento) {
        return !!(elemento && elemento.closest && elemento.closest('.swal2-container'));
    }
    ['keydown', 'keypress', 'keyup', 'beforeinput', 'paste'].forEach(function (tipo) {
        window.addEventListener(tipo, function (e) {
            if (!cortado || entradaDelCartel(e.target)) return;
            e.preventDefault();
            e.stopImmediatePropagation();
        }, true);
    });

    // ---- Corte ----------------------------------------------------------------------------------
    // Recarga conservando la instancia (un reload plano genera un posInstanceId nuevo y, en cuentas de produccion,
    // perderia al operador autorizado). Misma logica que recargarMismaInstancia de pos-bloqueo.js.
    function recargarMismaInstancia() {
        var url = new URL(window.location.href);
        var id = posInstanceId();
        if (id) url.searchParams.set('posInstanceId', id);
        // Recarga a proposito, con la venta ya resguardada: sin el "¿Deseas abandonar el sitio?" de pos-cart.js.
        if (typeof window.desactivarAvisoSalidaPOS === 'function') window.desactivarAvisoSalidaPOS();
        window.PosNavegandoInternoPOS = true;
        window.location.href = url.toString();
    }

    // Resguarda la venta en curso (si la hay). Mejor esfuerzo: si el servidor no responde queda el respaldo local, que el
    // mismo cajero recupera solo al recargar. cierrePestana deja la venta "interrumpida" de inmediato en el servidor, asi
    // otro cajero puede tomarla sin esperar el umbral de latido.
    function resguardarVenta() {
        var borrador = window.POSBorrador;
        if (!borrador || typeof borrador.estaHabilitado !== 'function' || !borrador.estaHabilitado()
            || typeof borrador.guardarAhora !== 'function') {
            return Promise.resolve(false);
        }
        return borrador.guardarAhora({ cierrePestana: true }).catch(function () { return false; });
    }

    // Corta la venta: se llama una sola vez, desde el chequeo periodico o desde forma-pago.js cuando el servidor rechaza
    // el cobro con cajaCerrada.
    function cortarPorCajaCerrada() {
        if (cortado) return;
        cortado = true;
        window.POSCajaCerrada = true;
        clearInterval(timer);

        // Respaldo local ANTES de levantar POSFinalizandoVenta (con esa marca POSDraft.save() borra el respaldo local).
        try {
            if (window.POSDraft && typeof window.POSDraft.save === 'function') window.POSDraft.save();
        } catch (e) { /* el respaldo local no frena el corte */ }
        // Frena los latidos y autoguardados de pos-borrador.js: el cartel puede quedar abierto un buen rato y un latido
        // tardio volveria a marcar la venta como "en uso" y nadie podria recuperarla hasta que venza el umbral.
        window.POSFinalizandoVenta = true;

        // Si hay un modal abierto (ej. forma de pago) no se cierra: el cartel de SweetAlert queda por encima y la pagina se
        // recarga al confirmar; cerrar el modal a mano dispararia sus propios handlers de cancelacion.
        resguardarVenta().then(function () {
            return Swal.fire({
                icon: 'warning',
                title: 'La caja fue cerrada',
                html: '<div class="text-left" style="font-size:.9rem">'
                    + '<p class="mb-2">Cerraron tu caja desde otra terminal, por eso ya no se pueden registrar ventas.</p>'
                    + '<p class="mb-2">La venta en curso <b>quedó resguardada</b>. Abrí la caja de nuevo para registrarla: se carga sola o, si no aparece, la recuperás desde Ayuda (F1) → Ver ventas sin guardar. '
                    + 'Si hay cambio de turno, el cajero que abra su caja también la puede tomar desde ahí.</p>'
                    + '</div>',
                confirmButtonText: 'Entendido',
                allowOutsideClick: false,
                allowEscapeKey: false
            });
        }).then(recargarMismaInstancia);
    }

    // ---- Chequeo periodico ----------------------------------------------------------------------
    function puedeVerificar() {
        return !cortado && !verificando
            && window.POSFinalizandoVenta !== true && window.POSVentaFinalizada !== true
            && window.POSBloqueado !== true;
    }

    function verificar() {
        if (!puedeVerificar()) return;
        verificando = true;

        fetch(cfg.urlEstado, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8', 'RequestVerificationToken': token() },
            body: 'posInstanceId=' + encodeURIComponent(posInstanceId())
        }).then(function (respuesta) {
            // 401/400 (sesion caida o token de otro usuario) o HTML de login: no se puede juzgar, se reintenta.
            return respuesta.ok ? respuesta.json() : null;
        }).then(function (resp) {
            // Solo corta con la respuesta explicita "caja cerrada" (nunca por ausencia de dato).
            if (resp && resp.ok === true && resp.indeterminado !== true && resp.cajaAbierta === false) {
                cortarPorCajaCerrada();
            }
        }).catch(function () {
            /* sin conexion o respuesta invalida: se reintenta en el proximo ciclo */
        }).then(function () {
            verificando = false;
        });
    }

    timer = setInterval(verificar, cfg.segundos * MS_SEGUNDO);
    // Al volver a la pestana (PC suspendida, cambio de ventana) se chequea ya, sin esperar el proximo ciclo.
    document.addEventListener('visibilitychange', function () {
        if (document.visibilityState === 'visible') verificar();
    });
    window.addEventListener('focus', verificar);

    window.POSCajaVigilante = { cortarPorCajaCerrada: cortarPorCajaCerrada };
})(window, window.jQuery);
