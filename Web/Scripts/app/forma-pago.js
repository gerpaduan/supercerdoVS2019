// ===============================
// VARIABLES GLOBALES DEL MODULO
// ===============================
let totalVentaActual = 0;
let otroTipoPagoSeleccionado = null;

// ===============================
// CUANDO SE ABRE EL MODAL
// ===============================
$('#modalFormaPago').on('shown.bs.modal', function () {

    totalVentaActual = obtenerTotalVenta(); // tu función existente
    $('#totalVenta').val(totalVentaActual.toFixed(2));

    resetPagoMixto();
});

// ===============================
// CHECKBOX PAGO MIXTO
// ===============================
$('#chkPagoMixto').on('change', function () {

    if ($(this).is(':checked')) {

        $('#bloquePagoMixto').slideDown();

        // Deshabilitar Efectivo y Cuenta Corriente
        $('.btn-forma-pago[data-tipo="Efectivo"]').prop('disabled', true);
        $('.btn-forma-pago[data-tipo="Cuenta Corriente"]').prop('disabled', true);

    } else {

        $('#bloquePagoMixto').slideUp();
        resetPagoMixto();

        // Habilitar todos
        $('.btn-forma-pago').prop('disabled', false);
    }
});

// ===============================
// CLICK EN FORMA DE PAGO
// ===============================
$('.btn-forma-pago').on('click', function () {

    const tipoPago = $(this).data('tipo');

    // ---------------------------
    // PAGO SIMPLE
    // ---------------------------
    if (!$('#chkPagoMixto').is(':checked')) {

        finalizarVenta({
            pagoMixto: false,
            tipoPago: tipoPago,
            monto: totalVentaActual
        });

        return;
    }

    // ---------------------------
    // PAGO MIXTO
    // ---------------------------
    otroTipoPagoSeleccionado = tipoPago;
    $('#labelOtroPago').text(tipoPago);
});

// ===============================
// CALCULO AUTOMATICO
// ===============================
$('#montoEfectivo').on('input', function () {

    let efectivo = parseFloat($(this).val()) || 0;
    let restante = totalVentaActual - efectivo;

    if (restante < 0) restante = 0;

    $('#montoOtroPago').val(restante.toFixed(2));
});

$('#montoOtroPago').on('input', function () {

    let otro = parseFloat($(this).val()) || 0;
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
            text: 'Seleccione la segunda forma de pago para completar el pago mixto.',
            confirmButtonText: 'Aceptar'
        });

        return;
    }

    let efectivo = parseFloat($('#montoEfectivo').val()) || 0;
    let otro = parseFloat($('#montoOtroPago').val()) || 0;
    let total = efectivo + otro;

    if (total.toFixed(2) !== totalVentaActual.toFixed(2)) {
        Swal.fire({
            icon: 'warning',
            title: 'Importes incorrectos',
            text: 'La suma de los importes ingresados no coincide con el total de la venta.',
            confirmButtonText: 'Revisar'
        });

        return;
    }

    guardarVenta({
        pagoMixto: true,
        efectivo: efectivo,
        otroMonto: otro,
        otroTipo: otroTipoPagoSeleccionado
    });
});

// ===============================
// RESET DEL BLOQUE MIXTO
// ===============================
function resetPagoMixto() {

    otroTipoPagoSeleccionado = null;

    $('#montoEfectivo').val('');
    $('#montoOtroPago').val('');
    $('#labelOtroPago').text('Otro Medio');
}

function seleccionarFormaPago(tipo) {

    const esPagoMixto = $('#chkPagoMixto').is(':checked');

    // Si es pago mixto, no finaliza acá
    if (esPagoMixto) {
        $('#labelOtroPago').text(tipo);
        $('#bloquePagoMixto').slideDown();
        return;
    }

    // Pago normal → finalizar venta directo
    finalizarVenta({
        formaPago: tipo,
        monto: parseFloat($('#totalVenta').val().replace(',', '.'))
    });
}
let ventaEnProceso = false;

function finalizarVenta(data) {

    // ⛔ Evitar doble envío
    if (ventaEnProceso) return;

    try {

        // 🔍 Validar productos
        if (!window.lineasVenta || window.lineasVenta.length === 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Venta vacía',
                text: 'No hay productos cargados en la venta'
            });
            return;
        }

        ventaEnProceso = true;

        // 🔒 Bloquear botón finalizar
        $('#btnFinalizarPago')
            .prop('disabled', true)
            .text('Finalizando...');

        $.ajax({
            url: api.venta.finalizar,
            type: 'POST',
            data: data,
            success: function (resp) {

                if (!resp || resp.ok !== true) {
                    ventaEnProceso = false;

                    $('#btnFinalizarPago')
                        .prop('disabled', false)
                        .text('Finalizar pago');

                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: resp?.msg || 'No se pudo finalizar la venta'
                    });
                    return;
                }

                Swal.fire({
                    icon: 'success',
                    title: 'Venta finalizada',
                    timer: 1500,
                    showConfirmButton: false
                });

                $('#modalFormaPago').modal('hide');

                setTimeout(() => location.reload(), 1600);
            },
            error: function () {

                ventaEnProceso = false;

                $('#btnFinalizarPago')
                    .prop('disabled', false)
                    .text('Finalizar pago');

                Swal.fire({
                    icon: 'error',
                    title: 'Error de comunicación',
                    text: 'No se pudo contactar al servidor'
                });
            }
        });

    } catch (e) {

        ventaEnProceso = false;

        $('#btnFinalizarPago')
            .prop('disabled', false)
            .text('Finalizar pago');

        console.error(e);

        Swal.fire({
            icon: 'error',
            title: 'Error inesperado',
            text: 'Ocurrió un error al finalizar la venta'
        });
    }
}



$(document).ready(function () {

    $(document).on('keydown', function (e) {
        if (ventaEnProceso) return;

        // Solo si el modal está visible
        if (!$('#modalFormaPago').hasClass('show')) return;

        const esPagoMixto = $('#chkPagoMixto').is(':checked');

        // =========================
        // PAGO MIXTO ACTIVADO
        // =========================
        if (esPagoMixto) {

            // SOLO tecla FIN
            if (e.key === 'End') {
                e.preventDefault();
                $('#btnFinalizarPagoMixto').click();
            }

            return; // bloquea números
        }

        // =========================
        // PAGO NORMAL (NO MIXTO)
        // =========================
        switch (e.key) {

            case '1':
                e.preventDefault();
                seleccionarFormaPago('Efectivo');
                break;

            case '2':
                e.preventDefault();
                seleccionarFormaPago('Débito');
                break;

            case '3':
                e.preventDefault();
                seleccionarFormaPago('Crédito');
                break;

            case '4':
                e.preventDefault();
                seleccionarFormaPago('Cuenta Corriente');
                break;

            case '5':
                e.preventDefault();
                seleccionarFormaPago('QR');
                break;

            case '6':
                e.preventDefault();
                seleccionarFormaPago('Transferencia');
                break;

            // ❌ FIN DESACTIVADA
            case 'End':
                e.preventDefault();
                break;
        }
    });

});
