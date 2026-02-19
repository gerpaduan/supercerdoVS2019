// =====================
// Config
// =====================
const KEY_TICKET_MM = "postventa_ticket_mm";

// =====================
// Helpers
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

function cerrarPostVentaYRecargar() {
    const $modal = $('#modalPostVenta');
    $modal.data('permitir-cierre', true);
    $modal.modal('hide');
    setTimeout(() => location.reload(), 400);
}

function imprimirTicket(mm) {
    const ventaId = $('#modalPostVenta').data('venta-id');

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

    // guardo selección “actual” en el modal (para Enter)
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
    });
}

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
        if (mm) {
            imprimirTicket(mm);
        } else {
            abrirOpcionesTicket(58);
        }
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

    // 3) Factura
    $('#btnPostVenta3').on('click', function () {
        const ventaId = $('#modalPostVenta').data('venta-id');

        $.get('/Ventas/ImprimirTicket', { id: ventaId, mm: 0 })
            .done(function (html) {
                $('#contenedorFacturaElectronica').html(html);
                $('#modalFacturaElectronica').modal('show');
            });

        $('#modalPostVenta').data('permitir-cierre', true);
        $('#modalPostVenta').modal('hide');
    });

    // recarga al cerrar factura
    $('#modalFacturaElectronica').on('hidden.bs.modal', function () {
        location.reload();
    });

    // 4) PDF (pendiente)
    $('#btnPostVenta4').on('click', function () {
        alert('Pendiente: Generar PDF');
    });

    // 5) WhatsApp (pendiente)
    $('#btnPostVenta5').on('click', function () {
        alert('Pendiente: Enviar a WhatsApp');
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
        if (e.key === '3') { e.preventDefault(); $('#btnPostVenta3').click(); return; }
        if (e.key === '4') { e.preventDefault(); $('#btnPostVenta4').click(); return; }
        if (e.key === '5') { e.preventDefault(); $('#btnPostVenta5').click(); return; }

        // Si el bloque de ticket NO está desplegado, no manejamos flechas/enter
        const ticketVisible = $('#bloqueTicketOpciones').hasClass('show');
        if (!ticketVisible) return;

        // Flechas: alternar 58/80
        if (e.key === 'ArrowUp' || e.key === 'ArrowDown' || e.key === 'ArrowLeft' || e.key === 'ArrowRight') {
            e.preventDefault();

            const actual = getTicketSeleccionActual() ?? getUltimoTicketMm() ?? 58;
            const nuevo = (actual === 58) ? 80 : 58;

            marcarTicketSeleccionado(nuevo);
            return;
        }

        // Enter: imprimir lo seleccionado (y guardar)
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
