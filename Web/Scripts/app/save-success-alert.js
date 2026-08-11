// Cartel unico de "guardado correctamente" para pantallas LayoutBase (nunca
// cargar desde _LayoutPOS.cshtml). Se autocierra a los 2s, pero el usuario
// puede cerrarlo antes con Enter o el boton OK (swal-single-confirm.js, ya
// cargado antes que este archivo, agrega ese comportamiento automaticamente
// a cualquier Swal.fire simple sin showConfirmButton:false).
(function (window) {
    'use strict';

    function show(message, options) {
        options = options || {};

        if (!window.Swal || typeof window.Swal.fire !== 'function') {
            window.alert(message || 'El registro se guardó correctamente.');
            if (typeof options.then === 'function') options.then();
            return;
        }

        return window.Swal.fire({
            icon: 'success',
            title: options.title || 'Guardado',
            text: message || 'El registro se guardó correctamente.',
            timer: 2000,
            timerProgressBar: true,
            showConfirmButton: true,
            confirmButtonText: 'OK'
        }).then(function (result) {
            if (typeof options.then === 'function') options.then(result);
        });
    }

    window.SaveSuccessAlert = { show: show };
})(window);
