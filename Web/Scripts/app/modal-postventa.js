// =====================
// Config
// =====================
const KEY_TICKET_MM = "postventa_ticket_mm";

let pvAgentAvailable = false;
let pvAgentChecked = false;
let pvAgentPrinterName = "";

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
function pvGetOrigen() {
    return ($('#modalPostVenta').data('origen') || 'pos').toString();
}
function pvGetVentaId() {
    const v = parseInt($('#modalPostVenta').data('venta-id'), 10);
    return isNaN(v) ? 0 : v;
}
function pvSetMeta(meta) {
    const $m = $('#modalPostVenta');
    meta = meta || {};

    if (meta.context) $m.data('context', meta.context);
    if (meta.origen) $m.data('origen', meta.origen);
    if (meta.ventaId != null) $m.data('venta-id', meta.ventaId);

    if (meta.facturaId != null) $m.data('factura-id', meta.facturaId);
    if (meta.nro != null) $m.data('nro', meta.nro);
    if (meta.cae != null) $m.data('cae', meta.cae);

    if (meta.whatsapp != null) $m.data('whatsapp', meta.whatsapp);
    if (meta.pdfUrl != null) $m.data('pdf-url', meta.pdfUrl);
    if (meta.imprimirPayloadUrl != null) $m.data('imprimir-payload-url', meta.imprimirPayloadUrl);
}

function pvGetPayloadUrl(mm) {
    const $m = $('#modalPostVenta');
    const explicit = ($m.data('imprimir-payload-url') || '').toString();
    if (explicit) {
        const sep = explicit.indexOf('?') >= 0 ? '&' : '?';
        return explicit + sep + 'mm=' + mm;
    }

    const ventaId = pvGetVentaId();
    if (!ventaId) return '';
    const basePayloadUrl = (window.AppUrls && window.AppUrls.ventasImprimirPayload)
        || (window.api && window.api.venta && window.api.venta.imprimirPayload)
        || '';
    if (!basePayloadUrl) return '';
    return `${basePayloadUrl}?id=${ventaId}&mm=${mm}`;
}

// Aplica textos/estados según contexto
function pvAplicarContextoUI() {
    const ctx = pvGetContext();
    const origen = pvGetOrigen();
    const $m = $('#modalPostVenta');

    const nro = ($m.data('nro') || '').toString();
    const cae = ($m.data('cae') || '').toString();

    const $title = $('#pvModalTitle');
    const $sub = $('#pvModalSubTitle');
    const $pregunta = $('#pvPregunta');
    const $lblNoImprimir = $('#pvLblNoImprimirBtn');

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

        $('#btnPostVenta3').prop('disabled', true);
        $('#pvLblFacturaBtn').text('Factura (ya emitida)');

        $('#pvLblPdfBtn').text('Abrir PDF');
        $('#pvLblWpBtn').text('Enviar factura por WhatsApp');
        $pregunta.text('¿Qué deseas hacer ahora?');
        $lblNoImprimir.text('No imprimir');

    } else {
        if (origen === 'detalle') {
            $title.text('Imprimir venta');
            $pregunta.text('¿Qué deseas hacer?');
            $lblNoImprimir.text('Cerrar');
        } else {
            $title.text('¡Venta completada! ✔');
            $pregunta.text('¿Qué deseas hacer ahora?');
            $lblNoImprimir.text('No imprimir');
        }

        $sub.hide().text('');

        $('#btnPostVenta3').prop('disabled', false);
        $('#pvLblFacturaBtn').text('Factura');

        $('#pvLblPdfBtn').text('Generar PDF');
        $('#pvLblWpBtn').text('Enviar a WhatsApp');
    }
}

// =====================
// Agente local
// =====================
function pvActualizarEstadoAgente(texto, disponible) {
    $('#estadoAgenteVenta')
        .text(texto)
        .toggleClass('text-success', !!disponible)
        .toggleClass('text-muted', !disponible);
}

function pvRefrescarEstadoAgente() {
    if (!pvAgentChecked) {
        pvActualizarEstadoAgente('Impresión local: verificando agente...', false);
        return;
    }

    if (pvAgentAvailable) {
        let texto = 'Impresión local activa';
        if (pvAgentPrinterName) {
            texto += ': ' + pvAgentPrinterName;
        }
        pvActualizarEstadoAgente(texto + '.', true);
        return;
    }

    pvActualizarEstadoAgente('Impresión local: usando navegador.', false);
}

function pvVerificarAgente() {
    const dfd = $.Deferred();
    if (!window.CarniSysPrintAgent) {
        pvAgentChecked = true;
        pvAgentAvailable = false;
        pvAgentPrinterName = '';
        pvRefrescarEstadoAgente();
        return dfd.resolve(false).promise();
    }

    window.CarniSysPrintAgent.health()
        .done(function (resp) {
            pvAgentChecked = true;
            pvAgentAvailable = !!(resp && resp.ok);
            pvAgentPrinterName = resp && resp.printerName ? resp.printerName : '';
            pvRefrescarEstadoAgente();
            dfd.resolve(pvAgentAvailable);
        })
        .fail(function () {
            pvAgentChecked = true;
            pvAgentAvailable = false;
            pvAgentPrinterName = '';
            pvRefrescarEstadoAgente();
            dfd.resolve(false);
        });

    return dfd.promise();
}

function pvCargarConfiguracionAgente() {
    const dfd = $.Deferred();
    if (!window.CarniSysPrintAgent) return dfd.reject().promise();

    $.when(window.CarniSysPrintAgent.getPrinters(), window.CarniSysPrintAgent.getConfig())
        .done(function (printersResp, configResp) {
            const printersData = printersResp[0] || {};
            const configData = configResp[0] || {};
            const items = printersData.items || [];
            const $cmb = $('#cmbImpresoraAgenteVenta');
            $cmb.empty();

            $.each(items, function (_, item) {
                const printerName = item.name || item.Name || '';
                const isDefault = !!(item.isDefault || item.IsDefault);
                $('<option>')
                    .val(printerName)
                    .text(printerName + (isDefault ? ' (predeterminada)' : ''))
                    .appendTo($cmb);
            });

            if (configData.printerName) {
                $cmb.val(configData.printerName);
            }

            $('#cmbMmAgenteVenta').val((configData.ticketMm === 80 ? 80 : 58).toString());
            dfd.resolve();
        })
        .fail(function () {
            dfd.reject();
        });

    return dfd.promise();
}

// =====================
// Cierre controlado + recarga
// =====================
function cerrarPostVentaYRecargar(delayMs) {
    const $modal = $('#modalPostVenta');
    $modal.data('permitir-cierre', true);
    $modal.modal('hide');
    setTimeout(() => location.reload(), delayMs || 400);
}

function cerrarPostVentaSegunOrigen(delayMs) {
    const $modal = $('#modalPostVenta');
    $modal.data('permitir-cierre', true);
    $modal.modal('hide');

    if (pvGetOrigen() === 'detalle') {
        return;
    }

    setTimeout(() => location.reload(), delayMs || 400);
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
        style: 'position:fixed; right:0; bottom:0; width:1px; height:1px; border:0; opacity:0; pointer-events:none;',
        src: `${((window.AppUrls && window.AppUrls.ventasImprimir) || (window.api && window.api.venta && window.api.venta.imprimir) || '')}?id=${ventaId}&mm=${mm}`
    });

    iframe.on('load', function () {
        try {
            const frameWindow = this.contentWindow;
            if (frameWindow) {
                frameWindow.focus();
                if (typeof frameWindow.print === 'function') {
                    frameWindow.print();
                }
            }
        } catch (e) { }

        cerrarPostVentaSegunOrigen(1200);
    });

    $('body').append(iframe);
}

function pvImprimirConAgente(mm) {
    const payloadUrl = pvGetPayloadUrl(mm);
    if (!payloadUrl || !window.CarniSysPrintAgent) {
        imprimirTicket(mm);
        return;
    }

    $.getJSON(payloadUrl)
        .done(function (payload) {
            if (!payload || payload.ok === false) {
                imprimirTicket(mm);
                return;
            }

            window.CarniSysPrintAgent.printExpendio(payload)
                .done(function () {
                    pvVerificarAgente();
                    cerrarPostVentaSegunOrigen(400);
                })
                .fail(function () {
                    imprimirTicket(mm);
                });
        })
        .fail(function () {
            imprimirTicket(mm);
        });
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

function pvAbrirNuevaPestanaConSesion(url) {
    if (!url) return;

    const link = document.createElement('a');
    link.href = url;
    link.target = '_blank';
    link.rel = 'noopener';
    link.style.display = 'none';

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function pvAbrirPdf() {
    const $modal = $('#modalPostVenta');
    const $btn = $('#btnPostVenta4');
    const modalPdfUrl = (($modal.data('pdf-url') || '').toString().trim());
    const buttonPdfUrl = (($btn.data('venta-pdf-url') || '').toString().trim());
    const baseUrl = modalPdfUrl || buttonPdfUrl;
    const ventaId = pvGetVentaId();

    if (!ventaId) {
        alert('No se encontró el id de la venta.');
        return;
    }

    if (!baseUrl) {
        alert('No se configuró la URL del PDF.');
        return;
    }

    const url = new URL(baseUrl, window.location.origin);
    if (!modalPdfUrl && !url.searchParams.has('id')) {
        url.searchParams.set('id', ventaId);
    }

    pvAbrirNuevaPestanaConSesion(url.toString());
    cerrarPostVentaSegunOrigen();
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

    cerrarPostVentaSegunOrigen();
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
        pvRefrescarEstadoAgente();
        pvVerificarAgente();

        setTimeout(() => $('#btnPostVenta2').trigger('focus'), 50);
    });
}

// =====================
// API pública para abrirlo
// =====================
window.PostModal = {
    openVenta: function (ventaId, extras = {}) {
        pvSetMeta({
            context: 'venta',
            origen: extras.origen || 'pos',
            ventaId: ventaId,
            facturaId: 0,
            nro: '',
            cae: '',
            pdfUrl: extras.pdfUrl || '',
            whatsapp: extras.whatsapp || '',
            imprimirPayloadUrl: extras.imprimirPayloadUrl || ((((window.AppUrls && window.AppUrls.ventasImprimirPayload) || (window.api && window.api.venta && window.api.venta.imprimirPayload) || '')) + `?id=${ventaId}`)
        });
        $('#modalPostVenta').modal('show');
    },

    openFactura: function (meta = {}) {
        meta.context = 'factura';

        if (!meta.ventaId) {
            const fromForm = $('#formFacturaElectronica input[name="IdVenta"]').val();
            meta.ventaId = parseInt(fromForm || "0", 10) || pvGetVentaId();
        }

        if (!meta.imprimirPayloadUrl && meta.ventaId) {
            meta.imprimirPayloadUrl = (((window.AppUrls && window.AppUrls.ventasImprimirPayload) || (window.api && window.api.venta && window.api.venta.imprimirPayload) || '')) + `?id=${meta.ventaId}`;
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

    $('#btnPostVenta1').on('click', function () {
        cerrarPostVentaYRecargar();
    });

    $('#btnPostVenta2').on('click', function () {
        const mm = getUltimoTicketMm();
        if (mm) {
            if (pvAgentAvailable) pvImprimirConAgente(mm);
            else imprimirTicket(mm);
        } else {
            abrirOpcionesTicket(58);
        }
    });

    $('#btnCambiarTicket').on('click', function () {
        abrirOpcionesTicket();
    });

    $(document).on('click', '.btnTicketOpt', function () {
        const mm = parseInt($(this).data('mm'), 10);
        if (mm !== 58 && mm !== 80) return;

        setUltimoTicketMm(mm);
        actualizarTextoTicket();
        marcarTicketSeleccionado(mm);
        if (pvAgentAvailable) pvImprimirConAgente(mm);
        else imprimirTicket(mm);
    });

    $('#btnConfigurarAgenteVenta').on('click', function () {
        $('#msgConfigAgenteVenta').addClass('d-none').text('');

        pvVerificarAgente().done(function (available) {
            if (!available) {
                $('#msgConfigAgenteVenta')
                    .removeClass('d-none')
                    .text('No se detectó el agente local. Descargalo e instalalo en esta terminal.');
                $('#modalConfigAgenteVenta').modal('show');
                return;
            }

            pvCargarConfiguracionAgente()
                .done(function () {
                    $('#modalConfigAgenteVenta').modal('show');
                })
                .fail(function () {
                    $('#msgConfigAgenteVenta')
                        .removeClass('d-none')
                        .text('No se pudieron leer las impresoras instaladas.');
                    $('#modalConfigAgenteVenta').modal('show');
                });
        });
    });

    $('#btnGuardarConfigAgenteVenta').on('click', function () {
        if (!window.CarniSysPrintAgent) return;

        const printerName = ($('#cmbImpresoraAgenteVenta').val() || '').toString();
        const ticketMm = parseInt($('#cmbMmAgenteVenta').val(), 10);
        window.CarniSysPrintAgent.saveConfig({
            printerName: printerName,
            ticketMm: ticketMm === 80 ? 80 : 58
        }).done(function () {
            if (ticketMm === 58 || ticketMm === 80) {
                setUltimoTicketMm(ticketMm);
                actualizarTextoTicket();
            }
            $('#modalConfigAgenteVenta').modal('hide');
            pvVerificarAgente();
        }).fail(function () {
            $('#msgConfigAgenteVenta')
                .removeClass('d-none')
                .text('No se pudo guardar la configuración de impresión.');
        });
    });

    let facturaOk = false;

    function abrirFacturaVentaModal(ventaId, opciones) {
        const opts = opciones || {};
        const volverAPostVenta = opts.volverAPostVenta === true;

        if (!ventaId) return;

        facturaOk = false;

        $.get((window.AppUrls && window.AppUrls.ventasImprimir) || (window.api && window.api.venta && window.api.venta.imprimir) || '', { id: ventaId, mm: 0 })
            .done(function (html) {
                $('#contenedorFacturaElectronica').html(html);

                const $modal = $('#modalFacturaElectronica');
                $modal.data('volver-postventa', volverAPostVenta);
                $modal.find('.modal-dialog').removeClass('modal-fullscreen-dialog');

                function ajustarFacturaModal() {
                    try {
                        const vh = Math.max(window.innerHeight || document.documentElement.clientHeight, 600);
                        const maxModal = Math.round(vh * 0.92);
                        const $header = $modal.find('.modal-header');
                        const $footer = $modal.find('.modal-footer');
                        const $top = $modal.find('.fe-top');
                        const headerH = $header.length ? $header.outerHeight(true) : 0;
                        const footerH = $footer.length ? $footer.outerHeight(true) : 0;
                        const topH = $top.length ? $top.outerHeight(true) : 0;
                        const summaryH = $modal.find('.fe-summary').length ? $modal.find('.fe-summary').outerHeight(true) : 0;
                        const padding = 40;
                        const available = Math.max(120, maxModal - (headerH + footerH + topH + summaryH + padding));
                        $modal.find('.fe-table-scroll').css('max-height', available + 'px');
                    } catch (err) {
                        console.warn('Error ajustando modal factura', err);
                    }
                }

                $modal.off('shown.factura').on('shown.bs.modal.factura', function () {
                    ajustarFacturaModal();
                    $(window).on('resize.factura', ajustarFacturaModal);
                });

                $modal.off('hidden.factura').on('hidden.bs.modal.factura', function () {
                    $(window).off('resize.factura');
                    $modal.off('.factura');
                });

                $modal.modal('show');
            });
    }

    $('#btnPostVenta3').on('click', function () {
        if (pvGetContext() !== 'venta') return;

        const ventaId = pvGetVentaId();
        abrirFacturaVentaModal(ventaId, { volverAPostVenta: true });

        $('#modalPostVenta').data('permitir-cierre', true);
        $('#modalPostVenta').modal('hide');
    });

    $('#modalFacturaElectronica').on('hidden.bs.modal', function () {
        const volverAPostVenta = $(this).data('volver-postventa') === true;
        $(this).removeData('volver-postventa');

        if (!facturaOk && volverAPostVenta) {
            pvSetContext('venta');
            $('#modalPostVenta').modal('show');
        }
    });

    $('#btnPostVenta4').on('click', function () {
        pvAbrirPdf();
    });

    $('#btnPostVenta5').on('click', function () {
        pvEnviarWhatsapp();
    });

    $(document).on('venta:facturada', function (e, resp) {
        facturaOk = true;

        resp = resp || {};
        window.PostModal.openFactura({
            ventaId: resp.ventaId || resp.idVenta || pvGetVentaId(),
            facturaId: resp.facturaId || resp.idFactura || 0,
            nro: resp.nro || resp.NroCbteAfip || '',
            cae: resp.cae || resp.CAE || '',
            whatsapp: resp.whatsapp || $('#formFacturaElectronica input[name="Whatsapp"]').val() || '',
            pdfUrl: resp.pdfUrl || ''
        });
    });

    window.VentasFacturaModal = window.VentasFacturaModal || {
        abrir: abrirFacturaVentaModal
    };

    $(document).on('keydown', function (e) {
        const $modal = $('#modalPostVenta');
        if (!$modal.hasClass('show')) return;

        const tag = (e.target && e.target.tagName) ? e.target.tagName.toLowerCase() : "";
        if (tag === "input" || tag === "textarea") return;

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
            if (pvAgentAvailable) pvImprimirConAgente(mm);
            else imprimirTicket(mm);
            return;
        }
    });

});
