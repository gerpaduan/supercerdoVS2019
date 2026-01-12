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


$('#btnFacturaElectronica').click(function () {
    const ventaId = $('#modalPostVenta').data('venta-id');

    $('#FacturaVentaId').val(ventaId);  // pasamos idVenta
    $('#FacturaVentaMm').val(0);        // mm=0 para factura

    $('#modalPostVenta').modal('hide');
    $('#modalFacturaElectronica').modal('show');
});



$('#btnNoImprimir').on('click', function () {
    $('#modalPostVenta').modal('hide');
    setTimeout(() => location.reload(), 400);
});
