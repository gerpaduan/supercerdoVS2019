$('#btnImprimirTicket').on('click', function () {
    const ventaId = $('#modalPostVenta').data('venta-id');

    $.get('/Venta/ImprimirTicket', { id: ventaId }, function (resp) {
        // resp puede ser PDF, ESC/POS, o base64 según tu implementación
        Swal.fire({
            icon: 'success',
            title: 'Ticket enviado',
            timer: 1500,
            showConfirmButton: false
        });
    });
});

$('#btnFacturaElectronica').on('click', function () {
    const ventaId = $('#modalPostVenta').data('venta-id');

    $.post('/Factura/Emitir', { id: ventaId }, function (resp) {
        Swal.fire({
            icon: resp.ok ? 'success' : 'error',
            title: resp.ok ? 'Factura generada' : 'Error AFIP',
            text: resp.msg || '',
        });
    });
});

$('#btnNoImprimir').on('click', function () {
    $('#modalPostVenta').modal('hide');
    setTimeout(() => location.reload(), 400);
});
