// ===============================
// VARIABLES GLOBALES
// ===============================
let totalVentaActual = 0;
let otroTipoPagoSeleccionado = null;
let ventaEnProceso = false;

window.mostrarModalPostVenta = window.mostrarModalPostVenta || function (ventaId, telefonoCliente) {

    const tel = telefonoCliente || '';

    if (!window.PostModal || typeof window.PostModal.openVenta !== 'function') {
        console.error('PostModal no está listo. Revisá orden de carga de scripts.');
        return;
    }

    window.PostModal.openVenta(ventaId, { whatsapp: tel });
};


// ===============================
// AL ABRIR MODAL
// ===============================
$('#modalFormaPago').on('shown.bs.modal', function () {

    totalVentaActual = obtenerTotalVenta(); // ya existente
    $('#totalVenta').val(totalVentaActual.toFixed(2));

    resetPagoMixto();
});

// ===============================
// Atajos: seleccionar forma de pago (GLOBAL)
// ===============================
window.seleccionarFormaPago = function (tipo) {
    // modal abierto?
    if (!$('#modalFormaPago').hasClass('show')) return;

    const $btn = $(`.btn-forma-pago[data-tipo="${tipo}"]`).first();
    if (!$btn.length) return;
    if ($btn.prop('disabled')) return;

    $btn.trigger('click');
};


// ===============================
// CHECK PAGO MIXTO
// ===============================
$('#chkPagoMixto').on('change', function () {

    if (this.checked) {

        $('#bloquePagoMixto').slideDown();

        $('.btn-forma-pago[data-tipo="Efectivo"]').prop('disabled', true);
        $('.btn-forma-pago[data-tipo="CtaCte"]').prop('disabled', true);

    } else {

        $('#bloquePagoMixto').slideUp();
        resetPagoMixto();

        $('.btn-forma-pago').prop('disabled', false);
    }
});

// ===============================
// CLICK FORMA DE PAGO
// ===============================
$('.btn-forma-pago').on('click', function () {

    const tipo = $(this).data('tipo');
    const esPagoMixto = $('#chkPagoMixto').is(':checked');

    // ---------------------------
    // PAGO NORMAL
    // ---------------------------
    if (!esPagoMixto) {

        finalizarVenta({
            formaPago: tipo,
            esPagoMixto: false,
            efectivo: 0,
            idPersona: $('#idPersona').val()
        });

        return;
    }

    // ---------------------------
    // PAGO MIXTO
    // ---------------------------
    otroTipoPagoSeleccionado = tipo;
    $('#labelOtroPago').text(tipo);
});

// ===============================
// CALCULO AUTOMATICO
// ===============================
$('#montoEfectivo').on('input', function () {

    const efectivo = parseFloat(this.value) || 0;
    let restante = totalVentaActual - efectivo;
    if (restante < 0) restante = 0;

    $('#montoOtroPago').val(restante.toFixed(2));
});

$('#montoOtroPago').on('input', function () {

    const otro = parseFloat(this.value) || 0;
    let restante = totalVentaActual - otro;
    if (restante < 0) restante = 0;

    $('#montoEfectivo').val(restante.toFixed(2));
});

// ===============================
// FINALIZAR PAGO MIXTO
// ===============================
$('#btnFinalizarPagoMixto').on('click', function () {

    if (!otroTipoPagoSeleccionado) {
        Swal.fire({
            icon: 'info',
            title: 'Forma de pago',
            text: 'Seleccione la segunda forma de pago'
        });
        return;
    }

    const efectivo = parseFloat($('#montoEfectivo').val()) || 0;
    const otro = parseFloat($('#montoOtroPago').val()) || 0;

    if ((efectivo + otro).toFixed(2) !== totalVentaActual.toFixed(2)) {
        Swal.fire({
            icon: 'warning',
            title: 'Importes incorrectos',
            text: 'La suma no coincide con el total'
        });
        return;
    }

    finalizarVenta({
        formaPago: otroTipoPagoSeleccionado,
        esPagoMixto: true,
        efectivo: efectivo,
        idPersona: $('#idPersona').val()
    });
});


// ===============================
// FINALIZAR VENTA (POST)
// ===============================
function finalizarVenta(data) {

    if (ventaEnProceso) return;

    if (!window.lineasVenta || window.lineasVenta.length === 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Venta vacía',
            text: 'No hay productos cargados en la venta'
        });
        return;
    }

    ventaEnProceso = true;

    const payload = {
        formaPago: data.formaPago,
        esPagoMixto: data.esPagoMixto,
        efectivo: data.efectivo,
        idPersona: data.idPersona,
        lineasVenta: window.lineasVenta.map(l => ({
            Codigo: l.codigo,
            CantKg: parseFloat(l.cant),
            PrecioKg: parseFloat(l.precio.replace('$', '')),
            Bonificacion: l.bonificacion,
            Estado: (l.anulado ? 1 : 0),
            Balanza: l.balanza
        }))
    };

    $.ajax({
        url: api.venta.finalizar,
        type: 'POST',
        contentType: 'application/json',
        dataType: 'json', // 🔥 CLAVE
        data: JSON.stringify(payload),

        success: function (resp) {

            if (!resp.ok) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: resp.msg || 'No se pudo finalizar la venta'
                });
                ventaEnProceso = false;
                return;
            }


            // ✅ Venta OK
            window.desactivarAvisoSalidaPOS?.();

            const ventaId = resp.ventaId;
            const tel = resp.whatsapp || ''; // o de donde lo saques

            const $fp = $('#modalFormaPago');

            // 1) Sacar el foco de adentro ANTES de ocultar (evita el warning)
            $fp.find(':focus').trigger('blur');
            if (document.activeElement) document.activeElement.blur();

            // 2) Cuando el modal terminó de ocultarse, recién ahí abrimos PostVenta
            $fp.one('hidden.bs.modal', function () {
                window.mostrarModalPostVenta(ventaId, tel);
            });

            // 3) Ocultar modal forma pago
            $fp.modal('hide');

            hayVentaEnCurso = false;
        },

        error: function (xhr) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: xhr.responseJSON?.msg || 'Error del servidor'
            });
            ventaEnProceso = false;
        }
    });

}


// ===============================
// RESET MIXTO
// ===============================
function resetPagoMixto() {

    otroTipoPagoSeleccionado = null;

    $('#chkPagoMixto').prop('checked', false);
    $('#montoEfectivo').val('');
    $('#montoOtroPago').val('');
    $('#labelOtroPago').text('Otro Medio');
}

// ===============================
// ATAJOS DE TECLADO
// ===============================
$(document).ready(function () {

    $(document).on('keydown', function (e) {

        if (ventaEnProceso) return;
        if (!$('#modalFormaPago').hasClass('show')) return;

        const esPagoMixto = $('#chkPagoMixto').is(':checked');

        // ---- MIXTO ----
        if (esPagoMixto) {
            if (e.key === 'End') {
                e.preventDefault();
                $('#btnFinalizarPagoMixto').click();
            }
            return;
        }

        const mapa = { '1': 'Efectivo', '2': 'Debito', '3': 'Credito', '4': 'CtaCte', '5': 'QR', '6': 'Transferencia' };
        const k = mapa[e.key] || mapa[(e.code || '').replace('Numpad', '')];
        if (k) window.seleccionarFormaPago(k);

    });
});
