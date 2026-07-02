(function (window, $) {
    'use strict';

    if (!window || !$) return;

    var active = false;

    function show(message) {
        var text = message || 'Cargando solicitud...';

        if (window.POSModalLoading && typeof window.POSModalLoading.show === 'function') {
            active = true;
            window.POSModalLoading.show(text);
            return;
        }

        if (!window.Swal || typeof window.Swal.fire !== 'function') return;

        active = true;
        window.Swal.fire({
            title: text,
            text: 'Espere un momento.',
            allowOutsideClick: false,
            allowEscapeKey: false,
            allowEnterKey: false,
            showConfirmButton: false,
            didOpen: function () {
                if (window.Swal && typeof window.Swal.showLoading === 'function') {
                    window.Swal.showLoading();
                }
            }
        });
    }

    function hide() {
        if (!active) return;
        active = false;

        if (window.POSModalLoading && typeof window.POSModalLoading.hide === 'function') {
            window.POSModalLoading.hide();
            return;
        }

        if (window.Swal && typeof window.Swal.close === 'function') {
            window.Swal.close();
        }
    }

    $(document)
        .off('.modalRequestLoading')
        .on('shown.bs.modal.modalRequestLoading hidden.bs.modal.modalRequestLoading', '.modal', function () {
            hide();
        });

    window.ModalRequestLoading = window.ModalRequestLoading || {
        show: show,
        hide: hide
    };
})(window, window.jQuery);
