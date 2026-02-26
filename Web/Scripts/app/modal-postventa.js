// =====================
// Config
// =====================
const KEY_TICKET_MM = "postventa_ticket_mm";

// =====================
// Helpers storage ticket
// =====================
function getUltimoTicketMm() {
    const v = localStorage.getItem(KEY_TICKET_MM);
    const mm = parseInt(v, 10);
    return (mm === 58 || mm === 80) ? mm : null;
}
function setUltimoTicketMm(mm) {
    localStorage.setItem(KEY_TICKET_MM, String(mm));
}
function actualizarTextoTicket() {
    const mm = getUltimoTicketMm();
    const $lbl = $("#lblTicketAccion");
    if (!mm) $lbl.text("Ticket");
    else $lbl.text(`Imprimir (${mm} mm)`);
}

// =====================
// Contexto (venta / factura)
// =====================
function pvGetContext() {
    return ($('#modalPostVenta').data('context') || 'venta').toString();
}
function pvSetContext(ctx) {
    $('#modalPostVenta').data('context', (ctx || 'venta').toString());
}
function pvGetVentaId() {
    const v = parseInt($('#modalPostVenta').data('venta-id'), 10);
    return isNaN(v) ? 0 : v;
}
function pvSetMeta(meta) {
    const $m = $('#modalPostVenta');
    meta = meta || {};

    if (meta.context) $m.data('context', meta.context);
    if (meta.ventaId != null) $m.data('venta-id', meta.ventaId);

    if (meta.facturaId != null) $m.data('factura-id', meta.facturaId);
    if (meta.nro != null) $m.data('nro', meta.nro);
    if (meta.cae != null) $m.data('cae', meta.cae);

    if (meta.whatsapp != null) $m.data('whatsapp', meta.whatsapp);
    if (meta.pdfUrl != null) $m.data('pdf-url', meta.pdfUrl);
}

// Aplica textos/estados según contexto
function pvAplicarContextoUI() {
    const ctx = pvGetContext();
    const $m = $('#modalPostVenta');

    const nro = ($m.data('nro') || '').toString();
    const cae = ($m.data('cae') || '').toString();

    const $title = $('#pvModalTitle');
    const $sub = $('#pvModalSubTitle');

    if (ctx === 'factura') {
        $title.text('¡Factura registrada! ✔');

        const subTxt = [];
        if (nro) subTxt.push(`Nro: ${nro}`);
        if (cae) subTxt.push(`CAE: ${cae}`);

        if (subTxt.length) {
            $sub.text(subTxt.join('  |  ')).show();
        } else {
            $sub.hide().text('');
        }

        // Botón factura: deshabilitado (no duplicar facturación)
        $('#btnPostVenta3').prop('disabled', true);
        $('#pvLblFacturaBtn').text('Factura (ya emitida)');

        // Botones cambian texto
        $('#pvLblPdfBtn').text('Abrir PDF');
        $('#pvLblWpBtn').text('Enviar factura por WhatsApp');

    } else {
        $title.text('¡Venta completada! ✔');
        $sub.hide().text('');

        $('#btnPostVenta3').prop('disabled', false);
        $('#pvLblFacturaBtn').text('Factura');

        $('#pvLblPdfBtn').text('Generar PDF');
        $('#pvLblWpBtn').text('Enviar a WhatsApp');
    }
}

// =====================
// Cierre controlado + recarga
// =====================
function cerrarPostVentaYRecargar() {
    const $modal = $('#modalPostVenta');
    $modal.data('permitir-cierre', true);
    $modal.modal('hide');
    setTimeout(() => location.reload(), 400);
}

// =====================
// Ticket
// =====================
function imprimirTicket(mm) {
    const ventaId = pvGetVentaId();
    if (!ventaId) return;

    $('#iframePrint').remove();

    const iframe = $('<iframe>', {
        id: 'iframePrint',
        style: 'display:none;',
        src: `/Ventas/ImprimirTicket?id=${ventaId}&mm=${mm}`
    });

    $('body').append(iframe);

    cerrarPostVentaYRecargar();
}

function marcarTicketSeleccionado(mm) {
    $('.btnTicketOpt').removeClass('active btn-primary').addClass('btn-outline-primary');

    const $btn = $(`.btnTicketOpt[data-mm="${mm}"]`);
    $btn.addClass('active btn-primary').removeClass('btn-outline-primary');

    $('#modalPostVenta').data('ticket-mm-actual', mm);
}

function getTicketSeleccionActual() {
    const mm = $('#modalPostVenta').data('ticket-mm-actual');
    return (mm === 58 || mm === 80) ? mm : null;
}

function abrirOpcionesTicket(preseleccionarMm = null) {
    $('#bloqueTicketOpciones').collapse('show');
    const mm = preseleccionarMm ?? getUltimoTicketMm() ?? 58;
    marcarTicketSeleccionado(mm);
}

// =====================
// PDF / WhatsApp (según contexto)
// =====================
function pvNormalizarTelefono(raw) {
    if (!raw) return null;
    const digits = String(raw).replace(/\D/g, '');
    return (digits.length >= 8) ? digits : null;
}

function pvAbrirPdf() {
    const ctx = pvGetContext();
    const $m = $('#modalPostVenta');

    // 1) Si ya te pasaron un pdfUrl, usamos eso
    const explicit = ($m.data('pdf-url') || '').toString().trim();
    if (explicit) {
        window.open(explicit, '_blank', 'noopener');
        cerrarPostVentaYRecargar();
        return;
    }

    const ventaId = pvGetVentaId();
    if (!ventaId) return;

    // 2) Default por contexto (ajustalo a tus acciones reales)
    let url = '';
    if (ctx === 'factura') {
        url = `/Ventas/FacturaPdf?idVenta=${ventaId}`; // <-- ajustá endpoint si difiere
    } else {
        url = `/Ventas/VentaPdf?id=${ventaId}`; // <-- si no existe, te va a quedar "pendiente"
    }

    if (!url) {
        alert('Pendiente: PDF');
        return;
    }

    window.open(url, '_blank', 'noopener');
    cerrarPostVentaYRecargar();
}

function pvEnviarWhatsapp() {
    const ctx = pvGetContext();
    const $m = $('#modalPostVenta');

    const tel = pvNormalizarTelefono($m.data('whatsapp'));
    if (!tel) {
        Swal.fire({ icon: 'warning', title: 'WhatsApp', text: 'No hay un número válido cargado.' });
        return;
    }

    const nro = ($m.data('nro') || '').toString();
    const cae = ($m.data('cae') || '').toString();

    let msg = 'Hola!';
    if (ctx === 'factura') {
        const parts = ['Te envío tu factura.'];
        if (nro) parts.push(`Nro ${nro}.`);
        if (cae) parts.push(`CAE ${cae}.`);
        msg = parts.join(' ');
    } else {
        msg = 'Hola! Te envío el comprobante de tu compra.';
    }

    const url = `https://wa.me/${tel}?text=${encodeURIComponent(msg)}`;
    window.open(url, '_blank', 'noopener');

    cerrarPostVentaYRecargar();
}

// =====================
// Bloquear cierre modal
// =====================
function configurarModalPostVentaBloqueado() {
    const $modal = $('#modalPostVenta');

    $modal.modal({
        backdrop: 'static',
        keyboard: false,
        show: false
    });

    $modal.on('hide.bs.modal', function (e) {
        if (!$modal.data('permitir-cierre')) {
            e.preventDefault();
            e.stopImmediatePropagation();
            return false;
        }
    });

    $modal.on('shown.bs.modal', function () {
        $modal.data('permitir-cierre', false);
        $modal.data('ticket-mm-actual', null);
        $('#bloqueTicketOpciones').collapse('hide');
        actualizarTextoTicket();
        pvAplicarContextoUI();

        // ✅ foco afuera del modal anterior
        setTimeout(() => $('#btnPostVenta2').trigger('focus'), 50);
    });
}

// =====================
// API pública para abrirlo
// =====================
window.PostModal = {
    // Se llama al terminar venta
    openVenta: function (ventaId, extras = {}) {
        pvSetMeta({
            context: 'venta',
            ventaId: ventaId,
            facturaId: 0,
            nro: '',
            cae: '',
            pdfUrl: extras.pdfUrl || '',
            whatsapp: extras.whatsapp || ''
        });
        $('#modalPostVenta').modal('show');
    },

    // Se llama al terminar facturación (con datos de la factura)
    openFactura: function (meta = {}) {
        // meta: { ventaId, facturaId, nro, cae, whatsapp, pdfUrl }
        meta.context = 'factura';

        // Fallback de ventaId si no vino:
        if (!meta.ventaId) {
            const fromForm = $('#formFacturaElectronica input[name="IdVenta"]').val();
            meta.ventaId = parseInt(fromForm || "0", 10) || pvGetVentaId();
        }

        pvSetMeta(meta);
        $('#modalPostVenta').modal('show');
    }
};

// =====================
// Ready
// =====================
$(document).ready(function () {

    configurarModalPostVentaBloqueado();

    // 1) No imprimir
    $('#btnPostVenta1').on('click', function () {
        cerrarPostVentaYRecargar();
    });

    // 2) Ticket (modo rápido)
    $('#btnPostVenta2').on('click', function () {
        const mm = getUltimoTicketMm();
        if (mm) imprimirTicket(mm);
        else abrirOpcionesTicket(58);
    });

    // Cambiar tamaño: siempre despliega opciones
    $('#btnCambiarTicket').on('click', function () {
        abrirOpcionesTicket();
    });

    // Click en 58/80: selecciona + guarda + imprime
    $(document).on('click', '.btnTicketOpt', function () {
        const mm = parseInt($(this).data('mm'), 10);
        if (mm !== 58 && mm !== 80) return;

        setUltimoTicketMm(mm);
        actualizarTextoTicket();
        marcarTicketSeleccionado(mm);
        imprimirTicket(mm);
    });

    // 3) Factura (solo en contexto venta)
    let facturaOk = false;

    $('#btnPostVenta3').on('click', function () {
        if (pvGetContext() !== 'venta') return; // en factura está deshabilitado igual

        facturaOk = false;
        const ventaId = pvGetVentaId();

        $.get('/Ventas/ImprimirTicket', { id: ventaId, mm: 0 })
            .done(function (html) {
                $('#contenedorFacturaElectronica').html(html);
                $('#modalFacturaElectronica').modal('show');
            });

        // oculto post modal mientras factura está abierta
        $('#modalPostVenta').data('permitir-cierre', true);
        $('#modalPostVenta').modal('hide');
    });

    // Si cierran el modal de factura sin registrar -> volver al post modal en "venta"
    $('#modalFacturaElectronica').on('hidden.bs.modal', function () {
        if (!facturaOk) {
            // vuelve al flujo original
            pvSetContext('venta');
            $('#modalPostVenta').modal('show');
        }
    });

    // 4) PDF
    $('#btnPostVenta4').on('click', function () {
        pvAbrirPdf();
    });

    // 5) WhatsApp
    $('#btnPostVenta5').on('click', function () {
        pvEnviarWhatsapp();
    });

    // Cuando se registra factura: reabrir este mismo modal en contexto "factura"
    $(document).on('venta:facturada', function (e, resp) {
        facturaOk = true;

        resp = resp || {};
        window.PostModal.openFactura({
            ventaId: resp.ventaId || resp.idVenta || pvGetVentaId(),
            facturaId: resp.facturaId || resp.idFactura || 0,
            nro: resp.nro || resp.NroCbteAfip || '',
            cae: resp.cae || resp.CAE || '',
            whatsapp: resp.whatsapp || $('#formFacturaElectronica input[name="Whatsapp"]').val() || '',
            pdfUrl: resp.pdfUrl || '' // si tu backend lo devuelve
        });
    });

    // =====================
    // Teclado 1..5 + Flechas/Enter para ticket
    // =====================
    $(document).on('keydown', function (e) {
        const $modal = $('#modalPostVenta');
        if (!$modal.hasClass('show')) return;

        const tag = (e.target && e.target.tagName) ? e.target.tagName.toLowerCase() : "";
        if (tag === "input" || tag === "textarea") return;

        // Atajos principales
        if (e.key === '1') { e.preventDefault(); $('#btnPostVenta1').click(); return; }
        if (e.key === '2') { e.preventDefault(); $('#btnPostVenta2').click(); return; }
        if (e.key === '3') { e.preventDefault(); if (pvGetContext() === 'venta') $('#btnPostVenta3').click(); return; }
        if (e.key === '4') { e.preventDefault(); $('#btnPostVenta4').click(); return; }
        if (e.key === '5') { e.preventDefault(); $('#btnPostVenta5').click(); return; }

        const ticketVisible = $('#bloqueTicketOpciones').hasClass('show');
        if (!ticketVisible) return;

        if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(e.key)) {
            e.preventDefault();

            const actual = getTicketSeleccionActual() ?? getUltimoTicketMm() ?? 58;
            const nuevo = (actual === 58) ? 80 : 58;

            marcarTicketSeleccionado(nuevo);
            return;
        }

        if (e.key === 'Enter') {
            e.preventDefault();

            const mm = getTicketSeleccionActual() ?? getUltimoTicketMm() ?? 58;

            setUltimoTicketMm(mm);
            actualizarTextoTicket();
            imprimirTicket(mm);
            return;
        }
    });

});