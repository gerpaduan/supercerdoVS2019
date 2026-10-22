// Modal "productos que usan este tipo / esta marca" (Productos > Tipos y Marcas, 2026-10-22).
// Cuando se intenta eliminar un tipo o una marca que tiene productos, el servidor responde enUso y esta pantalla
// abre la lista (parcial _ReasignarProductosModal) para pasar esos productos a otro tipo/marca de una sola vez.
// Dependencias: jQuery y Bootstrap modal. Cada pagina aporta el modal vacio (#modalReasignarProductos).
// Uso: ReasignarProductos.abrir({ urlModal: '...', parametros: { tipo: 'X' } });
(function (window, $) {
    'use strict';

    var SELECTOR_MODAL = '#modalReasignarProductos';
    var SELECTOR_CUERPO = '#modalReasignarProductosBody';

    function mostrarMensajeFinal(mensaje) {
        // Mismo aviso de "guardado" que usan las demas pantallas; si no esta cargado, alert simple.
        if (window.SaveSuccessAlert && typeof window.SaveSuccessAlert.show === 'function') {
            window.SaveSuccessAlert.show(mensaje, { then: function () { window.location.reload(); } });
            return;
        }
        window.alert(mensaje);
        window.location.reload();
    }

    function abrir(config) {
        var $modal = $(SELECTOR_MODAL);
        var $cuerpo = $(SELECTOR_CUERPO);

        $cuerpo.html('<div class="text-center text-muted py-4">Cargando...</div>');
        $modal.modal('show');

        $.get(config.urlModal, config.parametros || {})
            .done(function (html) { $cuerpo.html(html); })
            .fail(function (xhr) {
                $cuerpo.html(xhr && xhr.responseText
                    ? xhr.responseText
                    : '<div class="alert alert-danger mb-0">No se pudo abrir la lista de productos.</div>');
            });
    }

    // "Aplicar a todos": copia el destino elegido arriba a todas las filas.
    $(document)
        .off('click.reasignar', '#btnReasignarAplicarTodos')
        .on('click.reasignar', '#btnReasignarAplicarTodos', function () {
            var valor = $('#reasignarTodos').val();
            if (valor === '' || valor == null) return;
            $('#tablaReasignar .js-reasignar-nuevo').val(valor);
        });

    // Guardar: manda solo las filas con un destino elegido.
    $(document)
        .off('submit.reasignar', '#formReasignarProductos')
        .on('submit.reasignar', '#formReasignarProductos', function (e) {
            e.preventDefault();

            var $form = $(this);
            var $btn = $('#btnGuardarReasignacion');
            var $error = $('#reasignarError');
            var cambios = [];

            $('#tablaReasignar tbody tr').each(function () {
                var nuevo = $(this).find('.js-reasignar-nuevo').val();
                if (nuevo === '' || nuevo == null) return;
                cambios.push({ idCorte: parseInt($(this).data('id'), 10), nuevo: String(nuevo) });
            });

            $error.addClass('d-none').text('');
            if (!cambios.length) {
                $error.removeClass('d-none').text('Elegí al menos un producto para cambiar.');
                return;
            }

            $btn.prop('disabled', true);

            $.ajax({
                url: $form.data('url'),
                type: 'POST',
                data: {
                    __RequestVerificationToken: $form.find('input[name="__RequestVerificationToken"]').val(),
                    origen: String($form.data('origen')),
                    cambiosJson: JSON.stringify(cambios)
                },
                success: function (resp) {
                    if (!resp || !resp.success) {
                        $error.removeClass('d-none').text((resp && resp.message) || 'No se pudieron guardar los cambios.');
                        $btn.prop('disabled', false);
                        return;
                    }
                    mostrarMensajeFinal(resp.message || 'Los cambios se guardaron correctamente.');
                },
                error: function () {
                    $error.removeClass('d-none').text('No se pudieron guardar los cambios.');
                    $btn.prop('disabled', false);
                }
            });
        });

    window.ReasignarProductos = { abrir: abrir };
})(window, window.jQuery);
