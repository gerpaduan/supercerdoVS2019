// Selector de usuario (sin password) para la sala de produccion: cuando el usuario logueado es
// el usuario compartido de produccion (window.EsUsuarioProduccion), envuelve la accion de
// guardar de Movimientos/Stock/Elaborados para que primero se elija quien esta actuando
// realmente -- ese id viaja en un input hidden y en el server sustituye al creador (ver
// BaseController.ResolverUsuarioCreador). Mismo patron de "envolver un callback" que ya usa
// conAutorizacionCierre() en Cajas/CajasAbiertas.cshtml, pero sin password (window.SeleccionUsuario
// ya soporta requierePassword:false).
(function (window, $) {
    'use strict';
    if (!window || !$) return;

    function conSeleccionDeUsuario($form, accion, opciones) {
        opciones = opciones || {};
        var inputName = opciones.inputName || 'idUsuarioCreador';

        if (!window.EsUsuarioProduccion) {
            accion();
            return;
        }

        if (!window.SeleccionUsuario) {
            accion();
            return;
        }

        window.SeleccionUsuario.abrir({
            titulo: opciones.titulo || '¿Quién está haciendo esta carga?',
            usuarios: window.UsuariosActivosEmpresa || [],
            requierePassword: false
        }).then(function (usuario) {
            var $input = $form.find('input[name="' + inputName + '"]');
            if (!$input.length) {
                $input = $('<input type="hidden" />').attr('name', inputName).appendTo($form);
            }
            $input.val(usuario.id);
            accion();
        }).catch(function () {
            // Cancelado (Cancelar/backdrop/Esc): no se ejecuta la accion original.
        });
    }

    window.SeleccionUsuarioProduccion = { conSeleccionDeUsuario: conSeleccionDeUsuario };
})(window, window.jQuery);
