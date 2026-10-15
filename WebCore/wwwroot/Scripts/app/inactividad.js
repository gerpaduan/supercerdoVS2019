/*
 * Bloqueo por inactividad de la web normal (2026-10-15, ver docs/DECISIONS.md "Bloqueo por inactividad").
 * Tras N minutos sin actividad del usuario (mouse, teclado, touch) muestra "Extender sesion" durante
 * `avisoMinutos`; si el usuario elige cerrar o se agota la cuenta regresiva, cierra la sesion.
 * La inactividad se mide aca (navegador) y no con la cookie: los latidos de fondo (borradores, avisos del
 * admin) renuevan la cookie aunque nadie toque la PC. El ultimo instante de actividad se comparte entre
 * pestanas via localStorage. Config en window.__inactividad (la arma _Layout.cshtml; no se carga si el
 * usuario es de produccion, tiene 0 minutos o el dispositivo es "No bloquear sesion").
 * El POS no usa este script: tiene su propio bloqueo con clave (pos-bloqueo.js).
 */
(function () {
    "use strict";

    var cfg = window.__inactividad;
    if (!cfg || !(cfg.minutos > 0)) return;

    var MS_MINUTO = 60 * 1000;
    var limiteInactividadMs = cfg.minutos * MS_MINUTO;
    var avisoMs = (cfg.avisoMinutos > 0 ? cfg.avisoMinutos : 5) * MS_MINUTO;
    // Clave por usuario: dos usuarios en el mismo navegador no deben compartir el reloj.
    var claveUltima = "carnisys-inactividad-ultima-" + cfg.idUsuario;
    var intervaloChequeoMs = 10 * 1000;   // cada cuanto se evalua si vencio el plazo
    var intervaloEscrituraMs = 5 * 1000;  // throttle de la escritura en localStorage

    var ultimaLocal = Date.now();
    var ultimaEscritura = 0;
    var avisoAbierto = false;
    var cerrando = false;

    // localStorage puede no estar disponible (ventana privada, sitio bloqueado): se cae al reloj local.
    function leerUltima() {
        try {
            var v = parseInt(window.localStorage.getItem(claveUltima), 10);
            if (isFinite(v) && v > ultimaLocal) ultimaLocal = v;
        } catch (e) { /* sin localStorage: vale el reloj de esta pestana */ }
        return ultimaLocal;
    }

    function marcarActividad(forzar) {
        var ahora = Date.now();
        ultimaLocal = ahora;
        if (!forzar && ahora - ultimaEscritura < intervaloEscrituraMs) return;
        ultimaEscritura = ahora;
        try { window.localStorage.setItem(claveUltima, String(ahora)); } catch (e) { /* ver leerUltima */ }
    }

    // Con el aviso abierto la actividad pasiva NO extiende: hace falta pulsar el boton a proposito
    // (si no, mover el mouse cerraria el aviso sin que nadie lo lea).
    function alActividad() {
        if (avisoAbierto || cerrando) return;
        marcarActividad(false);
    }

    function cerrarSesion() {
        if (cerrando) return;
        cerrando = true;
        // Sin esto el beforeunload de "Salir sin guardar" frenaria el cierre de sesion.
        if (typeof window.desactivarProteccionSalida === "function") window.desactivarProteccionSalida();
        window.location.href = cfg.logoutUrl;
    }

    function extenderSesion() {
        // Se reinicia el contador YA (antes de esperar al servidor): si el chequeo periodico corriera con la
        // respuesta en vuelo volveria a abrir el aviso apenas el usuario eligio extender.
        marcarActividad(true);
        fetch(cfg.extenderUrl, {
            method: "POST",
            credentials: "same-origin",
            headers: { "X-Requested-With": "XMLHttpRequest" }
        }).then(function (resp) {
            // 401 = la sesion ya no existe en el servidor: no tiene sentido seguir.
            if (resp.status === 401 || resp.redirected) { cerrarSesion(); return; }
            marcarActividad(true);
        }).catch(function () {
            // Sin red no se puede confirmar, pero el usuario eligio seguir: se reinicia el contador local.
            marcarActividad(true);
        });
    }

    function mostrarAviso(vencimientoMs) {
        if (avisoAbierto || cerrando) return;
        // Fail-closed: sin SweetAlert2 no hay forma de preguntar, asi que se cierra al vencer el plazo.
        if (typeof window.Swal === "undefined") return;
        avisoAbierto = true;

        var restante = Math.max(1000, vencimientoMs - Date.now());
        var intervaloCuenta = null;

        function formatear(ms) {
            var s = Math.max(0, Math.ceil(ms / 1000));
            var m = Math.floor(s / 60);
            var r = s % 60;
            return m + ":" + (r < 10 ? "0" : "") + r;
        }

        window.Swal.fire({
            icon: "warning",
            title: "¿Extender sesión?",
            html: "Por inactividad, tu sesión se cerrará en <b id=\"inactividadCuenta\">" + formatear(restante) + "</b>.",
            showCancelButton: true,
            confirmButtonText: "Extender sesión",
            cancelButtonText: "Cerrar sesión",
            allowOutsideClick: false,
            allowEscapeKey: false,
            timer: restante,
            timerProgressBar: true,
            didOpen: function () {
                intervaloCuenta = window.setInterval(function () {
                    var el = document.getElementById("inactividadCuenta");
                    var left = window.Swal.getTimerLeft();
                    if (el && left != null) el.textContent = formatear(left);
                }, 500);
            },
            willClose: function () {
                if (intervaloCuenta) window.clearInterval(intervaloCuenta);
            }
        }).then(function (resultado) {
            // Si el aviso lo cerro chequear() porque otra pestana registro actividad, avisoAbierto ya
            // esta en false: no se hace nada (no es un "Cerrar sesion" del usuario).
            if (!avisoAbierto) return;
            avisoAbierto = false;
            if (resultado.isConfirmed) {
                extenderSesion();
            } else {
                // "Cerrar sesion" o se agoto el tiempo (dismiss === "timer").
                cerrarSesion();
            }
        });
    }

    function chequear() {
        if (cerrando) return;

        var ultima = leerUltima();
        var ahora = Date.now();
        var inactivoMs = ahora - ultima;

        if (avisoAbierto) {
            // Otra pestana registro actividad real (o extendio): se cierra el aviso de esta.
            if (inactivoMs < limiteInactividadMs && window.Swal && window.Swal.isVisible()) {
                avisoAbierto = false;
                window.Swal.close();
            }
            return;
        }

        if (inactivoMs >= limiteInactividadMs + avisoMs) {
            // Ya paso todo el plazo (ej. la PC estuvo suspendida): se cierra sin preguntar.
            cerrarSesion();
        } else if (inactivoMs >= limiteInactividadMs) {
            mostrarAviso(ultima + limiteInactividadMs + avisoMs);
        }
    }

    ["mousemove", "mousedown", "keydown", "touchstart", "scroll", "wheel"].forEach(function (evento) {
        document.addEventListener(evento, alActividad, { passive: true, capture: true });
    });

    // Cargar una pagina es una accion del usuario: reinicia el contador.
    marcarActividad(true);
    window.setInterval(chequear, intervaloChequeoMs);
    document.addEventListener("visibilitychange", function () {
        if (document.visibilityState === "visible") chequear();
    });
})();
