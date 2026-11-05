// Autorizacion provisoria de otro usuario en el POS (2026-11-03, ver docs/DECISIONS.md "POS: autorizacion provisoria de
// otro usuario"). Cuando el operador no tiene un permiso (bonificar, cuenta corriente, pagos, mis ventas, modificar una
// venta), el POS abre el selector de usuario: si otro usuario que SI lo tiene pone su usuario y clave, el operador puede
// hacer esa accion hasta que se cierre la ventana que la usa. Depende de seleccion-usuario.js y de la configuracion que
// arma Ventas/POS.cshtml en window.PosOperadorConfig (posInstanceId, usuariosActivos, urlAutorizarAccion, urlRevocarAccion).
// Este archivo solo pide y avisa el fin; quien decide si vale es siempre el servidor (VentasController.AutorizarAccionPOS).
(function (window, $) {
    'use strict';

    function config() {
        return window.PosOperadorConfig || {};
    }

    // Abre el selector de usuario. opciones: { accion, idRegistro, motivo }. Devuelve una promesa que se cumple con true si
    // alguien autorizo y con false si se cancelo o no se puede pedir (nunca se rechaza).
    function pedir(opciones) {
        opciones = opciones || {};

        return new Promise(function (resolve) {
            var cfg = config();
            if (!window.SeleccionUsuario || !cfg.urlAutorizarAccion || !cfg.posInstanceId) {
                resolve(false);
                return;
            }

            var terminado = false;
            function terminar(valor) {
                if (terminado) return;
                terminado = true;
                resolve(valor);
            }

            window.SeleccionUsuario.abrir({
                titulo: 'Autorizar con otro usuario',
                aviso: opciones.motivo || '',
                usuarios: cfg.usuariosActivos || [],
                requierePassword: true,
                validarUrl: cfg.urlAutorizarAccion,
                datosExtra: {
                    posInstanceId: cfg.posInstanceId,
                    accion: opciones.accion,
                    idRegistro: opciones.idRegistro || 0
                },
                alCancelar: function () { terminar(false); }
            }).then(function () { terminar(true); });
        });
    }

    // Avisa al servidor que termino la ventana de la accion. Con sendBeacon para que salga aunque la pagina se este
    // cerrando; el servidor solo quita (nunca habilita), por eso no lleva antiforgery. idRegistro vacio = cualquiera.
    function revocar(accion, idRegistro) {
        var cfg = config();
        if (!cfg.urlRevocarAccion || !cfg.posInstanceId) return;

        var datos = new URLSearchParams();
        datos.set('posInstanceId', cfg.posInstanceId);
        datos.set('accion', accion);
        if (idRegistro !== undefined && idRegistro !== null) datos.set('idRegistro', String(idRegistro));

        if (navigator.sendBeacon) {
            navigator.sendBeacon(cfg.urlRevocarAccion, datos);
            return;
        }

        $.post(cfg.urlRevocarAccion, datos.toString());
    }

    // El 403 de Finanzas conserva su cuerpo de texto; el encabezado dice que se puede pedir la autorizacion.
    // Devuelve { accion, idRegistro } o null.
    function leerEncabezado(xhr) {
        if (!xhr || typeof xhr.getResponseHeader !== 'function') return null;

        var accion = xhr.getResponseHeader('X-Pos-Autorizacion');
        if (!accion) return null;

        return { accion: accion, idRegistro: parseInt(xhr.getResponseHeader('X-Pos-Autorizacion-Id'), 10) || 0 };
    }

    // Boton "Autorizar con otro usuario" que trae el mensaje de falta de permiso dentro de un modal del POS
    // (.js-pos-autorizar con data-accion, data-id-registro, data-recargar-url y data-recargar-titulo): al autorizar vuelve
    // a cargar ese contenido, que ahora si se muestra.
    $(document).on('click', '.js-pos-autorizar', function () {
        var $boton = $(this);
        var recargarUrl = $boton.data('recargar-url');
        var recargarTitulo = $boton.data('recargar-titulo');
        var modoPago = $boton.data('recargar-modo') === 'pago';

        pedir({
            accion: $boton.data('accion'),
            idRegistro: parseInt($boton.data('id-registro'), 10) || 0,
            motivo: $boton.data('motivo') || ''
        }).then(function (ok) {
            if (!ok || !recargarUrl || !window.POSFinanzas) return;

            // El formulario de pago se abre en su propio modal (cargarPago); el resto, en el de Finanzas (cargar).
            var recargar = modoPago ? window.POSFinanzas.cargarPago : window.POSFinanzas.cargar;
            if (typeof recargar === 'function') recargar.call(window.POSFinanzas, recargarUrl, recargarTitulo || '');
        });
    });

    // "Modificar venta (requiere autorizacion)": el operador no tiene el permiso. Se pide la autorizacion de otro usuario
    // para ESA venta y, si la dan, se abre la edicion como siempre (conservando al operador de la pestana). La
    // autorizacion termina al guardar la venta o salir de la edicion (POS.cshtml).
    $(document).on('click', '.js-pos-modificar-autorizar', function (evento) {
        evento.preventDefault();
        var $enlace = $(this);
        var idVenta = parseInt($enlace.data('id-venta'), 10) || 0;
        var destino = $enlace.attr('href');

        pedir({
            accion: 'ModificarVenta',
            idRegistro: idVenta,
            motivo: 'El vendedor no tiene permiso para modificar ventas (POS - Últimas Ventas).'
        }).then(function (ok) {
            if (!ok) return;

            if (typeof window.desactivarAvisoSalidaPOS === 'function') window.desactivarAvisoSalidaPOS();
            if (String($enlace.data('conservar-operador')) === 'true') window.PosConservarOperadorAlSalir = true;
            window.location.href = destino;
        });
    });

    // El selector de usuario se abre muchas veces ENCIMA de otro modal del POS (Finanzas, pago): se lo sube de capa para
    // que no quede detras.
    $(document).on('shown.bs.modal', '#modalSeleccionUsuario', function () {
        $(this).css('z-index', 1100);
        $('.modal-backdrop').last().css('z-index', 1090);
    });

    // Termina la autorizacion cuando se cierra la ventana que la usaba: el modal de Finanzas (cuenta corriente, Mis
    // ventas) y el del formulario de pago.
    $(document).on('hidden.bs.modal', '#modalFinanzasPOS', function () {
        revocar('CtaCte');
        revocar('MisVentas');
    });
    $(document).on('hidden.bs.modal', '#modalPagoPOS', function () {
        revocar('Pago');
    });

    window.PosAutorizacion = { pedir: pedir, revocar: revocar, leerEncabezado: leerEncabezado };
})(window, window.jQuery);
