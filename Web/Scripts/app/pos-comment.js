(function (window, $) {
    'use strict';

    function createPOSComment(options) {
        // Este modulo administra el comentario general de la venta.
        // Guarda el valor en POSState para que pueda viajar junto con el resto
        // del estado del POS y mantiene sincronizada la UI del boton.
        const MAX_LENGTH = 200;

        function normalizeComment(value) {
            return String(value ?? '').replace(/\r\n/g, '\n');
        }

        function hasComment(value) {
            return normalizeComment(value).trim().length > 0;
        }

        function updateCounter(value) {
            $('#contadorComentarioVenta').text(normalizeComment(value).length);
        }

        // Refuerza visualmente el boton cuando hay comentario cargado.
        // Usamos una pequena badge con "1" para avisar sin meter demasiado ruido.
        function updateButtonState() {
            const comment = options.POSState.getObservaciones();
            const hasValue = hasComment(comment);
            const $button = $('#btnLimpiar');
            const $badge = $('#badgeComentarioVenta');

            $button.toggleClass('btn-outline-secondary', !hasValue);
            $button.toggleClass('btn-outline-info', hasValue);
            $button.attr('title', hasValue ? 'Editar comentario de la venta' : 'Agregar comentario de la venta');

            $badge.toggleClass('d-none', !hasValue);
            $badge.text(hasValue ? '1' : '');
        }

        function loadModalFromState() {
            const current = options.POSState.getObservaciones();
            $('#txtComentarioVenta').val(current);
            updateCounter(current);
        }

        function saveComment() {
            const comment = normalizeComment($('#txtComentarioVenta').val()).slice(0, MAX_LENGTH);
            options.POSState.setObservaciones(comment);
            updateButtonState();
            $('#modalComentarioVenta').modal('hide');
        }

        function clearComment() {
            $('#txtComentarioVenta').val('');
            updateCounter('');
        }

        function openCommentModal() {
            loadModalFromState();
            $('#modalComentarioVenta').modal('show');
        }

        function bindButton() {
            $(document)
                .off('click.posComentarioBtn', '#btnLimpiar')
                .on('click.posComentarioBtn', '#btnLimpiar', function (e) {
                    e.preventDefault();
                    openCommentModal();
                });
        }

        function bindModal() {
            $('#modalComentarioVenta')
                .off('shown.bs.modal.posComentario hidden.bs.modal.posComentario')
                .on('shown.bs.modal.posComentario', function () {
                    const textarea = document.getElementById('txtComentarioVenta');
                    if (textarea) {
                        textarea.focus();
                        textarea.selectionStart = textarea.value.length;
                        textarea.selectionEnd = textarea.value.length;
                    }
                })
                .on('hidden.bs.modal.posComentario', function () {
                    if (!$('.modal.show').length) {
                        options.focusCodigo();
                    }
                });
        }

        function bindInputs() {
            $('#txtComentarioVenta')
                .off('input.posComentario keydown.posComentario')
                .on('input.posComentario', function () {
                    updateCounter(this.value);
                })
                .on('keydown.posComentario', function (e) {
                    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'enter') {
                        e.preventDefault();
                        saveComment();
                    }
                });

            $('#btnGuardarComentarioVenta')
                .off('click.posComentarioGuardar')
                .on('click.posComentarioGuardar', function () {
                    saveComment();
                });

            $('#btnBorrarComentarioVenta')
                .off('click.posComentarioBorrar')
                .on('click.posComentarioBorrar', function () {
                    clearComment();
                });
        }

        const api = {
            init: function () {
                bindButton();
                bindModal();
                bindInputs();
                updateButtonState();
            },
            openCommentModal: openCommentModal,
            updateButtonState: updateButtonState
        };

        return api;
    }

    window.POSComment = {
        create: createPOSComment
    };
})(window, window.jQuery);
