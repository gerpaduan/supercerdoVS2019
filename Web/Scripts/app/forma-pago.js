let formaPagoSeleccionada = null;

$('#chkPagoMixto').on('change', function () {

    if (this.checked) {
        // Deshabilitar Efectivo y Cta Cte
        $('.btn-forma-pago').each(function () {
            let tipo = $(this).data('tipo');
            if (tipo === 'Efectivo' || tipo === 'Cuenta Corriente') {
                $(this).prop('disabled', true);
            }
        });
    } else {
        // Restaurar todo
        $('.btn-forma-pago').prop('disabled', false);
        $('#bloquePagoMixto').hide();
        formaPagoSeleccionada = null;
    }
});

$(document).on('click', '.btn-forma-pago', function () {

    let tipo = $(this).data('tipo');

    if (!$('#chkPagoMixto').is(':checked')) {
        // Pago simple
        finalizarVenta(tipo);
        return;
    }

    // Pago mixto
    formaPagoSeleccionada = tipo;
    $('#labelOtroPago').text(tipo);
    $('#bloquePagoMixto').slideDown();
});
