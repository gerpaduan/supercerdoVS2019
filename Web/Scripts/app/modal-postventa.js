function imprimirTicket(mm) {
    const ventaId = $('#modalPostVenta').data('venta-id');

    // borrar iframe previo
    $('#iframePrint').remove();

    const iframe = $('<iframe>', {
        id: 'iframePrint',
        style: 'display:none;',
        src: `/Ventas/ImprimirTicket?id=${ventaId}&mm=${mm}`
    });

    $('body').append(iframe);

    $('#modalPostVenta').modal('hide');
    setTimeout(() => location.reload(), 400);
}


$('#btnFacturaElectronica').on('click', function () {
    const ventaId = $('#modalPostVenta').data('venta-id');

    $.post(api.venta.imprimir, { id: ventaId }, function (resp) {
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
