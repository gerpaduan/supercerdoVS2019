// Acciones sobre un pedido ya guardado: ticket termico (58 / 80 mm), PDF, enviar por mail con el PDF adjunto y exportar a Excel.
// Lo usan el dialogo "¿Qué querés hacer ahora?" que aparece al guardar un pedido (Existencia por sucursales / Pedidos) y los botones de
// Pedidos/Detalle. Las URLs las arma PedidosController.AccionesPedido y viajan en la respuesta de Guardar / Modificar.
// Depende de: jQuery, SweetAlert2 (Swal), ticket-print.js (impresion por agente ESC/POS o por iframe; esta en _Layout, NO en _LayoutEmbed:
// la ventana independiente debe cargarlo antes de usar el ticket) y, para el mail, el token anti-falsificacion de la pagina.
//
// PedidoAcciones.abrirDialogo({ id, titulo, mensaje, acciones, onClose })  -> dialogo con Cerrar, Ticket, PDF, Mail y Excel
// PedidoAcciones.ticket(acciones) | .pdf(acciones) | .excel(acciones) | .email(id, acciones, alVolver)
(function (window, $) {
    'use strict';

    if (!$ || window.PedidoAcciones) return;

    var KEY_TICKET_MM = 'pedido_ticket_mm';

    function leerMm() {
        var valor = null;
        try { valor = window.localStorage.getItem(KEY_TICKET_MM); } catch (e) { /* sin almacenamiento: se usa 80 */ }
        var mm = parseInt(valor, 10);
        return (mm === 58 || mm === 80) ? mm : 80;
    }

    function guardarMm(mm) {
        try { window.localStorage.setItem(KEY_TICKET_MM, String(mm)); } catch (e) { /* no es un error: simplemente no se recuerda */ }
    }

    function conMm(url, mm) {
        if (!url) return '';
        return url + (url.indexOf('?') >= 0 ? '&' : '?') + 'mm=' + mm;
    }

    function token() {
        var $t = $('input[name="__RequestVerificationToken"]').first();
        return $t.length ? $t.val() : '';
    }

    function avisar(titulo, texto, icono) {
        if (window.Swal && typeof window.Swal.fire === 'function') return window.Swal.fire(titulo, texto, icono || 'info');
        window.alert(titulo + ': ' + texto);
        return $.Deferred().resolve().promise();
    }

    function ticket(acciones) {
        if (!acciones || !acciones.ticketUrl) return;
        if (!window.TicketPrint) {
            avisar('Ticket no disponible', 'No se cargó el módulo de impresión de tickets en esta ventana.', 'warning');
            return;
        }
        var mm = leerMm();
        window.TicketPrint.imprimir({ ticketUrl: conMm(acciones.ticketUrl, mm), payloadUrl: conMm(acciones.ticketPayloadUrl, mm) });
    }

    function pdf(acciones) {
        if (acciones && acciones.pdfUrl) window.open(acciones.pdfUrl, '_blank');
    }

    // La descarga es un GET con Content-Disposition: attachment, asi que no abre pestana ni cambia la pagina.
    function excel(acciones) {
        if (acciones && acciones.excelUrl) window.location.href = acciones.excelUrl;
    }

    // Formulario de mail (destinatario, asunto y mensaje) y envio. alVolver: se llama al terminar (enviado o cancelado).
    function email(id, acciones, alVolver) {
        function volver() { if (typeof alVolver === 'function') alVolver(); }

        $.getJSON(acciones.emailDatosUrl).done(function (datos) {
            if (!datos || datos.ok === false) {
                avisar('No se pudo preparar el mail', (datos && datos.msg) || 'Intentá de nuevo.', 'error').then(volver);
                return;
            }

            var $html = $('<div class="text-left"></div>');
            $html.append('<label class="small mb-0">Para</label><input id="swalPedidoMailPara" type="email" class="swal2-input mt-1" placeholder="correo@dominio.com" autocomplete="off">');
            $html.append('<label class="small mb-0">Asunto</label><input id="swalPedidoMailAsunto" type="text" class="swal2-input mt-1" autocomplete="off">');
            $html.append('<label class="small mb-0">Mensaje</label><textarea id="swalPedidoMailMensaje" class="swal2-textarea mt-1" rows="7"></textarea>');

            window.Swal.fire({
                title: 'Enviar el pedido por mail',
                html: $html.get(0),
                width: 560,
                focusConfirm: false,
                showCancelButton: true,
                confirmButtonText: 'Enviar con el PDF adjunto',
                cancelButtonText: 'Volver',
                didOpen: function () {
                    $('#swalPedidoMailPara').val(datos.email || '').trigger('focus');
                    $('#swalPedidoMailAsunto').val(datos.asunto || '');
                    $('#swalPedidoMailMensaje').val(datos.mensaje || '');
                },
                preConfirm: function () {
                    var para = ($('#swalPedidoMailPara').val() || '').trim();
                    var asunto = ($('#swalPedidoMailAsunto').val() || '').trim();
                    if (!para) { window.Swal.showValidationMessage('Ingresá el correo de destino.'); return false; }
                    if (!asunto) { window.Swal.showValidationMessage('Ingresá un asunto.'); return false; }

                    return $.ajax({
                        url: acciones.emailEnviarUrl,
                        type: 'POST',
                        data: { __RequestVerificationToken: token(), id: id, emailDestino: para, asunto: asunto, mensaje: $('#swalPedidoMailMensaje').val() || '' }
                    }).then(function (resp) {
                        if (!resp || !resp.ok) { window.Swal.showValidationMessage((resp && resp.msg) || 'No se pudo enviar el mail.'); return false; }
                        return resp;
                    }, function (xhr) {
                        var msg = xhr && xhr.responseJSON && xhr.responseJSON.msg;
                        window.Swal.showValidationMessage(msg || 'No se pudo enviar el mail. Intentá de nuevo.');
                        return false;
                    });
                }
            }).then(function (r) {
                if (r.isConfirmed) {
                    window.Swal.fire({ icon: 'success', title: 'Pedido enviado', timer: 1800, showConfirmButton: false }).then(volver);
                } else {
                    volver();
                }
            });
        }).fail(function () {
            avisar('No se pudo preparar el mail', 'Intentá de nuevo.', 'error').then(volver);
        });
    }

    // Dialogo posterior al guardado. "Cerrar" va primero: el pedido ya esta guardado, las demas acciones son opcionales y se pueden repetir.
    function abrirDialogo(opciones) {
        opciones = opciones || {};
        var acciones = opciones.acciones || {};
        var id = opciones.id;
        var mmActual = leerMm();

        if (!(window.Swal && typeof window.Swal.fire === 'function')) {
            if (typeof opciones.onClose === 'function') opciones.onClose();
            return;
        }

        var html =
            '<p class="mb-3">' + $('<div>').text(opciones.mensaje || '¿Qué querés hacer ahora?').html() + '</p>' +
            '<div class="d-grid gap-2" style="display:grid;gap:.5rem">' +
            '<button type="button" class="swal2-cancel swal2-styled" data-pa="cerrar" style="margin:0">Cerrar</button>' +
            '<div style="display:flex;gap:.5rem;align-items:center;justify-content:center">' +
            '<button type="button" class="swal2-confirm swal2-styled" data-pa="ticket" style="margin:0"><i class="fas fa-print"></i> Imprimir ticket</button>' +
            '<span class="small text-muted">Papel</span>' +
            '<button type="button" class="btn btn-sm btn-outline-secondary pa-mm" data-mm="58">58 mm</button>' +
            '<button type="button" class="btn btn-sm btn-outline-secondary pa-mm" data-mm="80">80 mm</button></div>' +
            '<button type="button" class="swal2-styled" data-pa="pdf" style="margin:0;background:#6c757d"><i class="far fa-file-pdf"></i> PDF</button>' +
            '<button type="button" class="swal2-styled" data-pa="email" style="margin:0;background:#6c757d"><i class="far fa-envelope"></i> Enviar por email</button>' +
            '<button type="button" class="swal2-styled" data-pa="excel" style="margin:0;background:#6c757d"><i class="far fa-file-excel"></i> Exportar a Excel</button>' +
            (acciones.detalleUrl ? '<a class="small mt-2" href="' + acciones.detalleUrl + '">Ver el pedido</a>' : '') +
            '</div>';

        var cerrado = false;
        function alCerrar() {
            if (cerrado) return;
            cerrado = true;
            if (typeof opciones.onClose === 'function') opciones.onClose();
        }

        function marcarMm(mm) {
            $('.pa-mm').removeClass('active').attr('aria-pressed', 'false');
            $('.pa-mm[data-mm="' + mm + '"]').addClass('active').attr('aria-pressed', 'true');
        }

        window.Swal.fire({
            icon: 'success',
            title: opciones.titulo || 'Pedido guardado',
            html: html,
            showConfirmButton: false,
            showCloseButton: true,
            allowOutsideClick: false,
            didOpen: function (popup) {
                marcarMm(mmActual);
                $(popup).on('click', '.pa-mm', function () {
                    mmActual = parseInt($(this).attr('data-mm'), 10);
                    guardarMm(mmActual);
                    marcarMm(mmActual);
                });
                $(popup).on('click', '[data-pa]', function () {
                    var accion = $(this).attr('data-pa');
                    if (accion === 'cerrar') { window.Swal.close(); return; }
                    if (accion === 'ticket') { ticket(acciones); return; }
                    if (accion === 'pdf') { pdf(acciones); return; }
                    if (accion === 'excel') { excel(acciones); return; }
                    if (accion === 'email') {
                        // El dialogo principal se cierra para dejar lugar al formulario del mail y se reabre al volver.
                        window.Swal.close();
                        cerrado = true;
                        email(id, acciones, function () { cerrado = false; abrirDialogo(opciones); });
                    }
                });
            },
            willClose: function () {
                // Un cierre "de paso" (para abrir el mail) no cuenta como terminar el flujo.
                window.setTimeout(function () { if (!document.querySelector('.swal2-container')) alCerrar(); }, 50);
            }
        });
    }

    window.PedidoAcciones = { abrirDialogo: abrirDialogo, ticket: ticket, pdf: pdf, excel: excel, email: email };
})(window, window.jQuery);
