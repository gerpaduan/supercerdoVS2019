// Item 1 de la segunda ronda de pedidos (2026-09-10, ver docs/DECISIONS.md "Batch 9: Factura/
// Imprimir en DetalleVenta/DetalleFactura"). Wire de #modalComprobanteVenta (_ModalComprobanteVenta.
// cshtml), copia derivada y recortada del bloque equivalente de Ventas/POS.cshtml (ticket termico
// HTML, PDF con eleccion de tipo de comprobante, email, factura electronica AFIP) adaptada a una
// venta ya guardada (ventaId FIJO desde window.DetalleVentaComprobanteConfig, no dinamico como en
// POS). Ver el comentario de cabecera de _ModalComprobanteVenta.cshtml para el detalle de que se
// dejo deliberadamente afuera (agente de impresion local ESC/POS, WhatsApp, atajos numericos,
// "Nueva venta").
(function ($, window) {
    'use strict';

    if (!$) return;

    $(function () {
        var cfg = window.DetalleVentaComprobanteConfig;
        if (!cfg || !cfg.ventaId) return;

        var ventaId = cfg.ventaId;
        var urls = cfg.urls;
        var pvbDocumentoEmail = 'detalle';

        // ===== Tamaño de ticket recordado -- misma clave literal que usa Ventas/POS.cshtml
        // (postventa_ticket_mm), mismo concepto, distinto origen de localStorage. =====
        var KEY_TICKET_MM = 'postventa_ticket_mm';
        function obtenerMedidaTicket() {
            var guardado = parseInt(localStorage.getItem(KEY_TICKET_MM), 10);
            return (guardado === 58 || guardado === 80) ? guardado : null;
        }
        function guardarMedidaTicket(mm) {
            try { localStorage.setItem(KEY_TICKET_MM, String(mm)); } catch (e) { }
        }
        function actualizarTextoMedida() {
            var mm = obtenerMedidaTicket() || 80;
            $('#cvTicketMedidaTexto').text(mm + 'mm');
        }
        function elegirMedidaTicket() {
            return Swal.fire({
                title: 'Tamaño de ticket',
                input: 'select',
                inputOptions: { 80: '80 mm', 58: '58 mm' },
                inputValue: obtenerMedidaTicket() || 80,
                showCancelButton: true,
                confirmButtonText: 'Usar este tamaño',
                cancelButtonText: 'Cancelar'
            }).then(function (result) {
                if (!result.isConfirmed) return null;
                var mm = parseInt(result.value, 10);
                guardarMedidaTicket(mm);
                actualizarTextoMedida();
                return mm;
            });
        }

        $('#modalComprobanteVenta').on('show.bs.modal', actualizarTextoMedida);
        actualizarTextoMedida();

        $('#btnCvTicket').on('click', function () {
            var mmRecordado = obtenerMedidaTicket();

            function abrirTicket(mm) {
                window.open(urls.imprimirTicketHtml + '?id=' + ventaId + '&mm=' + mm, '_blank');
            }

            if (mmRecordado) {
                abrirTicket(mmRecordado);
            } else {
                elegirMedidaTicket().then(function (mm) {
                    if (mm) abrirTicket(mm);
                });
            }
        });

        $('#cvTicketCambiarMedida').on('click', function (e) {
            e.preventDefault();
            elegirMedidaTicket();
        });

        // ===== PDF con eleccion de tipo de comprobante -- mismo criterio que
        // Ventas/POS.cshtml (pvSeleccionarOpcionSimple del clasico): sin factura -> detalle
        // directo; con nota de credito -> elegir entre detalle/factura/nc; agrupa items ->
        // elegir entre detalle/factura; si no, factura directo. =====
        $('#btnCvImprimir').on('click', function () {
            function abrirPdf(documento) {
                window.open(urls.imprimir + '?id=' + ventaId + '&documento=' + documento, '_blank');
            }

            $.ajax({
                url: urls.obtenerDatosEmailComprobante,
                type: 'GET',
                dataType: 'json',
                data: { id: ventaId }
            }).done(function (resp) {
                if (!resp || !resp.ok) {
                    Swal.fire({ icon: 'error', title: 'No se pudo preparar el PDF', text: (resp && resp.msg) || 'Error desconocido.' });
                    return;
                }

                if (!resp.tieneFactura) {
                    abrirPdf('detalle');
                    return;
                }

                if (resp.tieneNotaCredito) {
                    Swal.fire({
                        title: '¿Qué comprobante desea imprimir?',
                        input: 'select',
                        inputOptions: { detalle: 'Detalle', factura: 'Factura', nc: 'Nota de Crédito' },
                        inputValue: 'factura',
                        showCancelButton: true,
                        confirmButtonText: 'Imprimir'
                    }).then(function (result) {
                        if (result.isConfirmed) abrirPdf(result.value);
                    });
                    return;
                }

                if (resp.facturaAgrupaItems) {
                    Swal.fire({
                        title: '¿Qué comprobante desea imprimir?',
                        input: 'select',
                        inputOptions: { detalle: 'Detalle', factura: 'Factura' },
                        inputValue: 'factura',
                        showCancelButton: true,
                        confirmButtonText: 'Imprimir'
                    }).then(function (result) {
                        if (result.isConfirmed) abrirPdf(result.value);
                    });
                    return;
                }

                abrirPdf('factura');
            }).fail(function (xhr) {
                Swal.fire({ icon: 'error', title: 'No se pudo preparar el PDF', text: (xhr.responseJSON && xhr.responseJSON.msg) || 'Error de conexión.' });
            });
        });

        // ===== Email =====
        $('#btnCvEmail').on('click', function () {
            $('#cvEmailError').addClass('d-none').text('');

            $.ajax({
                url: urls.obtenerDatosEmailComprobante,
                type: 'GET',
                dataType: 'json',
                data: { id: ventaId }
            }).done(function (resp) {
                if (!resp || !resp.ok) {
                    Swal.fire({ icon: 'error', title: 'No se pudo preparar el email', text: (resp && resp.msg) || 'Error desconocido.' });
                    return;
                }

                pvbDocumentoEmail = resp.tieneFactura ? 'factura' : 'detalle';
                $('#cvEmailDestino').val(resp.email || '');
                $('#cvEmailAsunto').val(resp.asunto || '');
                $('#cvEmailMensaje').val(resp.mensaje || '');
                $('#modalComprobanteVenta').modal('hide');
                $('#modalEmailComprobanteVenta').modal('show');
            }).fail(function (xhr) {
                Swal.fire({ icon: 'error', title: 'No se pudo preparar el email', text: (xhr.responseJSON && xhr.responseJSON.msg) || 'Error de conexión.' });
            });
        });

        $('#btnCvConfirmarEmail').on('click', function () {
            var email = ($('#cvEmailDestino').val() || '').trim();
            var asunto = ($('#cvEmailAsunto').val() || '').trim();
            var mensaje = ($('#cvEmailMensaje').val() || '').trim();

            if (!email) {
                $('#cvEmailError').removeClass('d-none').text('Ingrese un email destino.');
                return;
            }

            var $btn = $(this);
            $btn.prop('disabled', true);

            $.ajax({
                url: urls.enviarComprobanteEmail,
                type: 'POST',
                dataType: 'json',
                data: {
                    idVenta: ventaId,
                    emailDestino: email,
                    asunto: asunto,
                    mensaje: mensaje,
                    documento: pvbDocumentoEmail
                }
            }).done(function (resp) {
                $btn.prop('disabled', false);

                if (!resp || !resp.ok) {
                    $('#cvEmailError').removeClass('d-none').text((resp && resp.msg) || 'No se pudo enviar el email.');
                    return;
                }

                $('#modalEmailComprobanteVenta').modal('hide');
                Swal.fire({ icon: 'success', title: 'Email enviado', text: 'El comprobante se envió correctamente.' });
            }).fail(function (xhr) {
                $btn.prop('disabled', false);
                $('#cvEmailError').removeClass('d-none').text((xhr.responseJSON && xhr.responseJSON.msg) || 'No se pudo enviar el email.');
            });
        });

        // ===== Factura Electronica (AFIP) -- port acotado de Web/Scripts/app/modal-postventa.js,
        // mismo patron que Ventas/POS.cshtml (abrirFacturaVentaModal/traerModalFacturaAlFrente).
        // Diferencia deliberada: al facturar/cerrar-sin-facturar con exito, en vez de reabrir un
        // modal de post-venta (no aplica aca, no hay "seguir vendiendo"), se recarga la pagina
        // para que el badge "Factura asociada" de _DetalleVentaCard.cshtml se actualice con el
        // estado real. =====
        function traerModalFacturaAlFrente($modal) {
            if (!$modal || !$modal.length) return;
            if (!$modal.parent().is('body')) $modal.appendTo('body');

            var zBase = 1040;
            $('.modal.show').not($modal).each(function () {
                var zActual = parseInt($(this).css('z-index'), 10) || 1040;
                if (zActual > zBase) zBase = zActual;
            });

            $modal.css('z-index', zBase + 20);
            window.setTimeout(function () {
                $('.modal-backdrop').last().css('z-index', zBase + 10);
            }, 0);
        }

        var facturaOkCv = false;

        function abrirFacturaVentaModal() {
            facturaOkCv = false;

            // dataType:'html' fuerza a jQuery a NO auto-parsear la respuesta como JSON aunque el
            // Content-Type sea application/json (el catch generico de ImprimirTicket devuelve
            // {ok:false,msg:...} con ese content-type ante cualquier excepcion).
            $.ajax({
                url: window.AppUrls.ventasImprimir,
                type: 'GET',
                data: { id: ventaId, mm: 0 },
                dataType: 'html'
            })
                .fail(function () {
                    Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo abrir la factura electrónica.' });
                })
                .done(function (html) {
                    var pareceError = typeof html === 'string' && /^\s*\{\s*"ok"\s*:\s*false/.test(html);
                    if (pareceError) {
                        var msg = 'No se pudo abrir la factura electrónica.';
                        try { msg = JSON.parse(html).msg || msg; } catch (e) { }
                        Swal.fire({ icon: 'error', title: 'Error', text: msg });
                        return;
                    }

                    $('#contenedorFacturaElectronica').html(html);
                    var $modal = $('#modalFacturaElectronica');

                    traerModalFacturaAlFrente($modal);
                    $modal.modal({ backdrop: 'static', keyboard: false, show: true });
                });
        }

        $('#btnCvFactura').on('click', function () {
            $('#modalComprobanteVenta').modal('hide');
            abrirFacturaVentaModal();
        });

        $(document).on('venta:facturada venta:cerradaSinFacturar', function () {
            facturaOkCv = true;
            window.location.reload();
        });

        $('#modalFacturaElectronica').on('hidden.bs.modal', function () {
            if (!facturaOkCv) {
                $('#modalComprobanteVenta').modal('show');
            }
        });
    });
})(window.jQuery, window);
