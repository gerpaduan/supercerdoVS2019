// Modal generico "¿Qué querés hacer ahora?" que se abre tras GUARDAR un comprobante (pago/cobro, egreso de caja):
//   1 cerrar sin imprimir · 2 ticket térmico (58 / 80 mm) · 3 PDF · 4 enviar por mail con el PDF adjunto.
// Markup en Views/Shared/_ModalPostComprobante.cshtml. Base: modal-postmovimiento.js (tamaño de ticket recordado
// en localStorage, impresión vía TicketPrint = agente ESC/POS o iframe) + el flujo de mail del post-venta.
// Depende de: jQuery, Bootstrap 5 (modal), SweetAlert2 (Swal), ticket-print.js.
//
// PostComprobanteModal.open({
//   titulo, mensaje,                       // textos del encabezado y de la pregunta (opcionales)
//   ticketUrl, ticketPayloadUrl,           // HTML del ticket y payload del agente; se les agrega &mm=58|80
//   pdfUrl,                                // PDF del comprobante (se abre en pestaña nueva)
//   emailDatosUrl,                         // GET -> { ok, email, asunto, mensaje, msg }
//   emailEnviarUrl, emailIdCampo, id,      // POST emailEnviarUrl con { [emailIdCampo]: id, emailDestino, asunto, mensaje }
//   onClose                                // se llama UNA vez al terminar el flujo (cerrar / imprimir / PDF / mail enviado)
// })
(function (window, $) {
    'use strict';

    if (!$ || window.PostComprobanteModal) return;

    var KEY_TICKET_MM = 'postcomprobante_ticket_mm';
    var MODAL = '#modalPostComprobante';
    var MODAL_EMAIL = '#modalEmailPostComprobante';

    var opciones = null;     // lo que paso quien abrio el modal
    var cerrado = true;      // evita llamar onClose dos veces

    function leerMm() {
        var valor = null;
        try { valor = window.localStorage.getItem(KEY_TICKET_MM); } catch (e) { }
        var mm = parseInt(valor, 10);
        return (mm === 58 || mm === 80) ? mm : 80;
    }

    function guardarMm(mm) {
        try { window.localStorage.setItem(KEY_TICKET_MM, String(mm)); } catch (e) { }
    }

    function marcarMm(mm) {
        $('.btnPcMm').removeClass('active').attr('aria-pressed', 'false');
        $('.btnPcMm[data-mm="' + mm + '"]').addClass('active').attr('aria-pressed', 'true');
    }

    function conMm(url, mm) {
        if (!url) return '';
        return url + (url.indexOf('?') >= 0 ? '&' : '?') + 'mm=' + mm;
    }

    // Oculta un modal solo si esta abierto; si todavia esta animando su apertura, Bootstrap ignoraria el hide() y
    // el modal quedaria abierto para siempre (con su backdrop y su focus trap): se espera a que termine de abrir.
    function ocultar($modal) {
        if (!$modal.hasClass('show')) return;

        var instancia = window.bootstrap && window.bootstrap.Modal ? window.bootstrap.Modal.getInstance($modal[0]) : null;
        if (instancia && instancia._isTransitioning) {
            $modal.one('shown.bs.modal', function () { $modal.modal('hide'); });
            return;
        }

        $modal.modal('hide');
    }

    // Cierra todo y avisa a quien abrió el modal (para que siga con lo suyo: cerrar el formulario, refrescar, etc.).
    function terminar() {
        if (cerrado) return;
        cerrado = true;

        ocultar($(MODAL_EMAIL).data('permitir-cierre', true));
        ocultar($(MODAL));

        var alTerminar = opciones && opciones.onClose;
        if (typeof alTerminar !== 'function') return;

        // onClose corre UNA vez, cuando Bootstrap termino de ocultar este modal (para no pelearse con el modal que
        // sigue, p. ej. #modalPagoPOS); el temporizador es solo una red de seguridad si ese evento no llegara.
        var llamado = false;
        function ejecutar() {
            if (llamado) return;
            llamado = true;
            alTerminar();
        }
        $(MODAL).one('hidden.bs.modal', function () { window.setTimeout(ejecutar, 50); });
        window.setTimeout(ejecutar, 1200);
    }

    function imprimirTicket() {
        var mm = leerMm();
        if (!opciones || !opciones.ticketUrl) return;

        window.TicketPrint.imprimir({
            ticketUrl: conMm(opciones.ticketUrl, mm),
            payloadUrl: conMm(opciones.ticketPayloadUrl, mm)
        });
        terminar();
    }

    function abrirPdf() {
        if (!opciones || !opciones.pdfUrl) return;
        window.open(opciones.pdfUrl, '_blank', 'noopener');
        terminar();
    }

    function mostrarErrorMail(texto) {
        $('#pcEmailError').removeClass('d-none').text(texto);
    }

    function prepararMail() {
        if (!opciones || !opciones.emailDatosUrl) return;

        $('#pcEmailError').addClass('d-none').text('');

        $.ajax({ url: opciones.emailDatosUrl, type: 'GET', dataType: 'json' })
            .done(function (resp) {
                if (!resp || !resp.ok) {
                    Swal.fire({ icon: 'error', title: 'No se pudo preparar el mail', text: (resp && (resp.msg || resp.mensaje)) || 'Error desconocido.' });
                    return;
                }

                $('#pcEmailDestino').val(resp.email || '');
                $('#pcEmailAsunto').val(resp.asunto || '');
                $('#pcEmailMensaje').val(resp.mensaje || '');
                $(MODAL_EMAIL).data('permitir-cierre', false).modal('show');
            })
            .fail(function (xhr) {
                Swal.fire({ icon: 'error', title: 'No se pudo preparar el mail', text: (xhr.responseJSON && (xhr.responseJSON.msg || xhr.responseJSON.mensaje)) || 'Error de conexión.' });
            });
    }

    function enviarMail() {
        var email = ($('#pcEmailDestino').val() || '').trim();
        var asunto = ($('#pcEmailAsunto').val() || '').trim();
        var mensaje = ($('#pcEmailMensaje').val() || '').trim();

        if (!email) { mostrarErrorMail('Ingrese un email destino.'); return; }
        if (!asunto) { mostrarErrorMail('Ingrese un asunto.'); return; }

        var datos = { emailDestino: email, asunto: asunto, mensaje: mensaje };
        datos[opciones.emailIdCampo || 'id'] = opciones.id;

        var $btn = $('#btnPcEmailEnviar').prop('disabled', true);

        $.ajax({ url: opciones.emailEnviarUrl, type: 'POST', dataType: 'json', data: datos })
            .done(function (resp) {
                $btn.prop('disabled', false);

                if (!resp || !resp.ok) {
                    mostrarErrorMail((resp && (resp.msg || resp.mensaje)) || 'No se pudo enviar el mail.');
                    return;
                }

                // Mail enviado: se cierra el modal de mail y todo el flujo (el aviso de éxito no bloquea).
                $(MODAL_EMAIL).data('permitir-cierre', true).modal('hide');
                Swal.fire({ icon: 'success', title: 'Mail enviado', text: 'El comprobante se envió correctamente.', timer: 1800, timerProgressBar: true });
                terminar();
            })
            .fail(function (xhr) {
                $btn.prop('disabled', false);
                mostrarErrorMail((xhr.responseJSON && (xhr.responseJSON.msg || xhr.responseJSON.mensaje)) || 'No se pudo enviar el mail.');
            });
    }

    // El apilado sobre otros modales (z-index, backdrop propio, saneo) lo resuelve de forma global modal-stack.js
    // (ModalStack): este modal se abre sobre el de Pago/Egreso del POS y queda arriba solo. Antes habia aca una copia
    // propia (2026-10-06, ver docs/DECISIONS.md).

    // Bootstrap 5 deja un FocusTrap activo por modal: el del modal de abajo le roba el foco al de arriba. Se deja
    // activo solo el del modal con mayor z-index (igual que calculadora-billetes.js).
    function reajustarFocusTraps() {
        if (!window.bootstrap || !window.bootstrap.Modal) return;

        var modales = Array.prototype.slice.call(document.querySelectorAll('.modal.show'));
        var maxZ = -1;
        var superior = null;
        modales.forEach(function (el) {
            var z = parseInt(window.getComputedStyle(el).zIndex, 10) || 0;
            if (z >= maxZ) { maxZ = z; superior = el; }
        });

        modales.forEach(function (el) {
            var instancia = window.bootstrap.Modal.getInstance(el);
            var trap = instancia && instancia._focustrap;
            if (!trap) return;
            if (el === superior && typeof trap.activate === 'function') trap.activate();
            else if (el !== superior && typeof trap.deactivate === 'function') trap.deactivate();
        });
    }

    window.PostComprobanteModal = {
        open: function (opts) {
            opciones = $.extend({}, opts || {});
            cerrado = false;

            $('#pcTitulo').text(opciones.titulo || 'Guardado');
            $('#pcMensaje').text(opciones.mensaje || '¿Qué querés hacer ahora?');
            $('#btnPcTicket, #btnPcPdf, #btnPcEmail').prop('disabled', false);
            $('#btnPcTicket').toggleClass('d-none', !opciones.ticketUrl);
            $('#btnPcPdf').toggleClass('d-none', !opciones.pdfUrl);
            $('#btnPcEmail').toggleClass('d-none', !opciones.emailDatosUrl);
            $('.btnPcMm').closest('.btn-group').toggleClass('d-none', !opciones.ticketUrl);
            marcarMm(leerMm());

            // Bootstrap 5: .modal(objetoOpciones) solo crea la instancia; el .modal('show') explícito la muestra
            // (misma regla que calculadora-billetes.js y modal-postmovimiento.js).
            $(MODAL).modal({ backdrop: 'static', keyboard: false });
            $(MODAL).modal('show');
        }
    };

    $(function () {
        $(MODAL + ', ' + MODAL_EMAIL).on('shown.bs.modal hidden.bs.modal', function () {
            reajustarFocusTraps();
        });

        $(MODAL).on('shown.bs.modal', function () {
            window.setTimeout(function () { $('#btnPcCerrar').trigger('focus'); }, 50);
        });

        $(MODAL_EMAIL).on('shown.bs.modal', function () {
            $('#pcEmailDestino').trigger('focus');
        });

        $('#btnPcCerrar').on('click', terminar);
        $('#btnPcTicket').on('click', imprimirTicket);
        $('#btnPcPdf').on('click', abrirPdf);
        $('#btnPcEmail').on('click', prepararMail);
        $('#btnPcEmailEnviar').on('click', enviarMail);

        // Cancelar el mail vuelve al modal de opciones (no cierra el flujo): se puede elegir otra opción.
        $('#btnPcEmailCancelar').on('click', function () {
            $(MODAL_EMAIL).data('permitir-cierre', true).modal('hide');
            window.setTimeout(function () { $('#btnPcCerrar').trigger('focus'); }, 200);
        });

        $('.btnPcMm').on('click', function () {
            var mm = parseInt($(this).data('mm'), 10);
            if (mm !== 58 && mm !== 80) return;
            guardarMm(mm);
            marcarMm(mm);
        });

        // Atajos 1-4 mientras el modal de opciones está visible (y no se está escribiendo en el de mail).
        $(document).on('keydown.postComprobante', function (e) {
            if (!$(MODAL).hasClass('show') || $(MODAL_EMAIL).hasClass('show')) return;

            var tag = (e.target && e.target.tagName || '').toLowerCase();
            if (tag === 'input' || tag === 'textarea' || tag === 'select') return;

            if (e.key === '1') { e.preventDefault(); $('#btnPcCerrar').trigger('click'); }
            else if (e.key === '2') { e.preventDefault(); if (!$('#btnPcTicket').hasClass('d-none')) $('#btnPcTicket').trigger('click'); }
            else if (e.key === '3') { e.preventDefault(); if (!$('#btnPcPdf').hasClass('d-none')) $('#btnPcPdf').trigger('click'); }
            else if (e.key === '4') { e.preventDefault(); if (!$('#btnPcEmail').hasClass('d-none')) $('#btnPcEmail').trigger('click'); }
        });
    });
})(window, window.jQuery);
