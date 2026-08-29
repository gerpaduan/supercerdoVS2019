// ===============================
// VARIABLES GLOBALES
// ===============================
let totalVentaActual = 0;
let otroTipoPagoSeleccionado = null;
let ventaEnProceso = false;

function getPOSStateFormaPago() {
    return window.POSState || null;
}

function normalizarTipoFormaPago(tipo) {
    if (window.POSFormaPagoPrecios && typeof window.POSFormaPagoPrecios.normalizeFormaPago === 'function') {
        return window.POSFormaPagoPrecios.normalizeFormaPago(tipo);
    }

    return tipo || '';
}

function getModoFormaPagoActual() {
    return getPOSStateFormaPago()?.getModoFormaPago?.() || 'finalizacion';
}

function requierePreseleccionFormaPago() {
    return getPOSStateFormaPago()?.getRequierePreseleccionFormaPago?.() === true;
}

function getFormaPagoPreseleccionada() {
    return getPOSStateFormaPago()?.getFormaPagoPreseleccionada?.() || null;
}

function setFormaPagoPreseleccionada(data) {
    return getPOSStateFormaPago()?.setFormaPagoPreseleccionada?.(data || null);
}

function formatearImporteFormaPago(valor) {
    const numero = Number(valor || 0);
    return numero.toLocaleString('es-AR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

function normalizarNombreFormaPago(tipo) {
    const mapa = {
        Debito: 'Débito',
        Credito: 'Crédito',
        CtaCte: 'Cta. Cte.',
        QR: 'QR',
        Qr: 'QR',
        Transferencia: 'Transferencia',
        Efectivo: 'Efectivo'
    };

    return mapa[tipo] || tipo || '';
}

function obtenerAtajoFormaPago(tipo) {
    const mapa = {
        Efectivo: '1',
        Debito: '2',
        Credito: '3',
        CtaCte: '4',
        QR: '5',
        Qr: '5',
        Transferencia: '6'
    };

    return mapa[tipo] || '';
}

function formatearNombreFormaPagoConAtajo(tipo) {
    const nombre = normalizarNombreFormaPago(tipo);
    const atajo = obtenerAtajoFormaPago(tipo);
    return atajo ? (nombre + ' ' + atajo) : nombre;
}

function actualizarLeyendaFormaPagoActual() {
    const $info = $('#formaPagoActualInfo');
    if (!$info.length) return;

    const data = window.POSFormaPagoActual || {};
    const formaPago = String(data.formaPago || '').trim();
    const efectivo = Number(data.pagoMixtoEfectivo || 0);
    const total = Number(data.total || obtenerTotalVenta() || 0);

    if (!formaPago) {
        $info.hide().html('');
        return;
    }

    let texto = '<strong>Forma de pago actual:</strong> ' + formatearNombreFormaPagoConAtajo(formaPago);

    if (efectivo > 0) {
        const otroMonto = total - efectivo;
        texto = '<strong>Forma de pago actual:</strong> Mixto - Efectivo $' + formatearImporteFormaPago(efectivo) +
            ' + ' + formatearNombreFormaPagoConAtajo(formaPago) + ' $' + formatearImporteFormaPago(otroMonto > 0 ? otroMonto : 0);
    }

    $info.html(texto).show();
}

function limpiarSeleccionFormaPago() {
    $('.btn-forma-pago').removeClass('active');
}

function getBotonFormaPago(tipo) {
    const normalized = normalizarTipoFormaPago(tipo);
    return $('.btn-forma-pago').filter(function () {
        return normalizarTipoFormaPago($(this).data('tipo')) === normalized;
    }).first();
}

function marcarFormaPagoSeleccionada(tipo) {
    limpiarSeleccionFormaPago();
    if (!tipo) return;
    getBotonFormaPago(tipo).addClass('active');
}

function actualizarPOSFormaPagoActual(data) {
    window.POSFormaPagoActual = window.POSFormaPagoActual || {};
    window.POSFormaPagoActual.formaPago = normalizarTipoFormaPago(data?.tipo || data?.formaPago || '');
    window.POSFormaPagoActual.pagoMixtoEfectivo = Number(data?.pagoMixtoEfectivo || 0);
    window.POSFormaPagoActual.total = Number(data?.total || obtenerTotalVenta() || 0);
}

function bloquearFormasPagoNoPreseleccionadas() {
    const modo = getModoFormaPagoActual();
    const preseleccion = getFormaPagoPreseleccionada();

    if (modo !== 'finalizacion' || !requierePreseleccionFormaPago() || !preseleccion || !preseleccion.tipo) {
        $('.btn-forma-pago').prop('disabled', ventaEnProceso);
        return;
    }

    const tipoSeleccionado = normalizarTipoFormaPago(preseleccion.tipo);
    $('.btn-forma-pago').each(function () {
        const coincide = normalizarTipoFormaPago($(this).data('tipo')) === tipoSeleccionado;
        $(this).prop('disabled', ventaEnProceso || !coincide);
    });
}

window.actualizarResumenFormaPagoPOS = function () {
    const $wrap = $('#posFormaPagoActual');
    const $nombre = $('#posFormaPagoNombre');
    if (!$wrap.length || !$nombre.length) return;

    const seleccion = getFormaPagoPreseleccionada();
    if (!seleccion || !seleccion.tipo) {
        $wrap.addClass('d-none');
        $nombre.text('-');
        return;
    }

    $nombre.text(normalizarNombreFormaPago(seleccion.nombre || seleccion.tipo));
    $wrap.removeClass('d-none');
};

function configurarModalFormaPagoSegunModo() {
    const modo = getModoFormaPagoActual();
    const preseleccion = getFormaPagoPreseleccionada();
    const mostrarPagoMixto = modo === 'finalizacion' && preseleccion && preseleccion.tipo &&
        normalizarTipoFormaPago(preseleccion.tipo) !== 'Efectivo' &&
        normalizarTipoFormaPago(preseleccion.tipo) !== 'CtaCte';

    $('#chkPagoMixto').prop('checked', false);
    $('#bloquePagoMixto').hide();
    $('#montoEfectivo, #montoOtroPago').val('');
    $('#labelOtroPago').text('Otro Medio');
    otroTipoPagoSeleccionado = null;

    $('.form-group.form-check.mb-4').toggle(modo === 'finalizacion');
    $('#totalVenta').val(modo === 'preseleccion' ? '-' : formatearImporteFormaPago(totalVentaActual));
    $('#totalVenta').closest('.form-group').toggle(true);

    if (modo === 'preseleccion') {
        $('.btn-forma-pago').prop('disabled', ventaEnProceso);
        return;
    }

    $('#chkPagoMixto').prop('disabled', ventaEnProceso || !mostrarPagoMixto);
    bloquearFormasPagoNoPreseleccionadas();
}

window.preCargarFormaPagoActual = function () {
    const preseleccion = getFormaPagoPreseleccionada();
    if (preseleccion && preseleccion.tipo) {
        actualizarPOSFormaPagoActual({
            tipo: preseleccion.tipo,
            pagoMixtoEfectivo: window.POSFormaPagoActual?.pagoMixtoEfectivo || 0,
            total: obtenerTotalVenta()
        });
    }

    const data = window.POSFormaPagoActual || {};
    const total = Number(data.total || obtenerTotalVenta() || 0);
    const formaPago = normalizarTipoFormaPago(data.formaPago || '');
    const efectivo = Number(data.pagoMixtoEfectivo || 0);
    const esMixto = efectivo > 0;
    const otroMonto = total - efectivo;

    totalVentaActual = total;
    limpiarSeleccionFormaPago();
    actualizarLeyendaFormaPagoActual();
    configurarModalFormaPagoSegunModo();

    if (!formaPago) return;

    marcarFormaPagoSeleccionada(formaPago);

    if (esMixto) {
        $('#chkPagoMixto').prop('checked', true);
        $('#bloquePagoMixto').show();
        $('#montoEfectivo').val(efectivo.toFixed(2));
        $('#montoOtroPago').val((otroMonto > 0 ? otroMonto : 0).toFixed(2));
        $('#labelOtroPago').text(normalizarNombreFormaPago(formaPago));
        otroTipoPagoSeleccionado = formaPago;
        $('.btn-forma-pago').each(function () {
            const tipo = normalizarTipoFormaPago($(this).data('tipo'));
            $(this).prop('disabled', ventaEnProceso || tipo !== formaPago);
        });
    }
};

function setEstadoVentaEnProceso(activa) {
    ventaEnProceso = !!activa;

    // Deshabilitamos los puntos de accion que podrian repetir la venta
    // mientras el servidor aun no termino de responder.
    $('#btnFinalizar').prop('disabled', ventaEnProceso);
    $('#btnFinalizarPagoMixto').prop('disabled', ventaEnProceso);
    configurarModalFormaPagoSegunModo();
}

function aplicarFormaPagoPreseleccionada(tipo) {
    const formaPago = normalizarTipoFormaPago(tipo);
    if (!formaPago) return;

    setFormaPagoPreseleccionada({
        id: formaPago,
        nombre: formaPago,
        tipo: formaPago
    });

    actualizarPOSFormaPagoActual({
        tipo: formaPago,
        pagoMixtoEfectivo: 0,
        total: obtenerTotalVenta()
    });

    if (window.POSFormaPagoPrecios && window.POSState?.getLineas) {
        const lineas = window.POSState.getLineas() || [];
        const recalculadas = window.POSFormaPagoPrecios.recalcularCarritoSegunFormaPago(lineas, formaPago, window.POSFormaPagoConfig || {});
        if (recalculadas > 0) {
            window.renderTablaProductos?.(lineas);
            window.recalcularTotal?.();
            window.actualizarEstadoVentaEnCurso?.();
        }
    }

    window.actualizarResumenFormaPagoPOS?.();
    if (window.esEdicionVenta === true && window.POSState?.getLineas) {
        const lineas = window.POSState.getLineas() || [];
        const historicas = lineas.filter(function (linea) {
            return !!linea && linea.esHistorica === true;
        }).length;
        const nuevas = lineas.filter(function (linea) {
            return !!linea && linea.esHistorica !== true && linea.anulado !== true && linea.anulado !== 1 && linea.anulado !== '1' && linea.anulado !== 'true';
        }).length;

        if (historicas > 0 && nuevas === 0) {
            window.Swal?.fire({
                icon: 'info',
                title: 'Forma de pago actualizada',
                text: 'Los productos ya cargados mantienen su precio original. Los nuevos productos usarán la forma de pago seleccionada.'
            });
        }
    }
}

window.abrirModalFormaPagoPreseleccion = function () {
    getPOSStateFormaPago()?.setModoFormaPago?.('preseleccion');
    window.preCargarFormaPagoActual?.();
    $('#modalFormaPago').modal('show');
};

window.abrirModalFormaPagoFinalizacion = function () {
    getPOSStateFormaPago()?.setModoFormaPago?.('finalizacion');
    window.preCargarFormaPagoActual?.();
    $('#modalFormaPago').modal('show');
};

window.mostrarModalPostVenta = window.mostrarModalPostVenta || function (ventaId, telefonoCliente) {

    const tel = telefonoCliente || '';

    if (!window.PostModal || typeof window.PostModal.openVenta !== 'function') {
        console.error('PostModal no está listo. Revisá orden de carga de scripts.');
        return;
    }

    window.PostModal.openVenta(ventaId, { whatsapp: tel });
};


// ===============================
// AL ABRIR MODAL
// ===============================
$('#modalFormaPago').on('shown.bs.modal', function () {

    totalVentaActual = obtenerTotalVenta(); // ya existente
    resetPagoMixto();
    window.preCargarFormaPagoActual?.();
});

if (window.POSGuard) {
    window.POSGuard.bindModal('#modalFormaPago', 'formaPago');
}

// ===============================
// Atajos: seleccionar forma de pago (GLOBAL)
// ===============================
window.seleccionarFormaPago = function (tipo) {
    // modal abierto?
    if (!$('#modalFormaPago').hasClass('show')) return;
    if (window.POSGuard && !window.POSGuard.isModalOnTop('#modalFormaPago')) return;

    const $btn = getBotonFormaPago(tipo);
    if (!$btn.length) return;
    if ($btn.prop('disabled')) return;

    $btn.trigger('click');
};


// ===============================
// CHECK PAGO MIXTO
// ===============================
$('#chkPagoMixto').on('change', function () {
    if (getModoFormaPagoActual() !== 'finalizacion') {
        this.checked = false;
        return;
    }

    if (this.checked) {

        $('#bloquePagoMixto').slideDown();
        bloquearFormasPagoNoPreseleccionadas();

    } else {

        $('#bloquePagoMixto').slideUp();
        resetPagoMixto();
        bloquearFormasPagoNoPreseleccionadas();
    }
});

// ===============================
// CLICK FORMA DE PAGO
// ===============================
$('.btn-forma-pago').on('click', function () {

    const tipo = normalizarTipoFormaPago($(this).data('tipo'));
    const modo = getModoFormaPagoActual();
    const esPagoMixto = $('#chkPagoMixto').is(':checked');

    if (modo === 'preseleccion') {
        aplicarFormaPagoPreseleccionada(tipo);
        $('#modalFormaPago').modal('hide');
        setTimeout(function () {
            if ($('.modal.show').length === 0) {
                $('#inputCodigo').trigger('focus');
                const codigoPendiente = String($('#inputCodigo').val() || '').trim();
                if (codigoPendiente) {
                    $('#inputCodigo').trigger('keyup');
                }
            }
        }, 60);
        return;
    }

    // ---------------------------
    // PAGO NORMAL
    // ---------------------------
    if (!esPagoMixto) {

        finalizarVenta({
            formaPago: tipo,
            esPagoMixto: false,
            efectivo: 0,
            idPersona: $('#idPersona').val()
        });

        return;
    }

    // ---------------------------
    // PAGO MIXTO
    // ---------------------------
    otroTipoPagoSeleccionado = tipo;
    $('#labelOtroPago').text(normalizarNombreFormaPago(tipo));
    marcarFormaPagoSeleccionada(tipo);
});

// ===============================
// CALCULO AUTOMATICO
// ===============================
$('#montoEfectivo').on('input', function () {

    const efectivo = parseFloat(this.value) || 0;
    let restante = totalVentaActual - efectivo;
    if (restante < 0) restante = 0;

    $('#montoOtroPago').val(restante.toFixed(2));
});

$('#montoOtroPago').on('input', function () {

    const otro = parseFloat(this.value) || 0;
    let restante = totalVentaActual - otro;
    if (restante < 0) restante = 0;

    $('#montoEfectivo').val(restante.toFixed(2));
});

// ===============================
// FINALIZAR PAGO MIXTO
// ===============================
$('#btnFinalizarPagoMixto').on('click', function () {

    if (!otroTipoPagoSeleccionado) {
        Swal.fire({
            icon: 'info',
            title: 'Forma de pago',
            text: 'Seleccione la segunda forma de pago'
        });
        return;
    }

    const valorEfectivo = ($('#montoEfectivo').val() || '').trim();
    const valorOtro = ($('#montoOtroPago').val() || '').trim();
    const efectivo = parseFloat(valorEfectivo) || 0;
    const otro = parseFloat(valorOtro) || 0;

    if (!valorEfectivo || !valorOtro || efectivo <= 0 || otro <= 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Pago mixto incompleto',
            text: 'Debe ingresar ambos importes y los dos deben ser mayores a cero.'
        });
        return;
    }

    if ((efectivo + otro).toFixed(2) !== totalVentaActual.toFixed(2)) {
        Swal.fire({
            icon: 'warning',
            title: 'Importes incorrectos',
            text: 'La suma no coincide con el total'
        });
        return;
    }

    finalizarVenta({
        formaPago: otroTipoPagoSeleccionado,
        esPagoMixto: true,
        efectivo: efectivo,
        idPersona: $('#idPersona').val()
    });
});


// ===============================
// FINALIZAR VENTA (POST)
// ===============================
function parseNumeroPOS(value) {
    let s = String(value ?? '').trim();
    s = s.replace(/[^0-9,.\-]/g, '');

    const hasComma = s.includes(',');
    const hasDot = s.includes('.');

    if (hasComma && hasDot) {
        s = s.replace(/\./g, '').replace(',', '.');
    } else if (hasComma) {
        s = s.replace(',', '.');
    }

    const n = parseFloat(s);
    return Number.isFinite(n) ? n : NaN;
}

function getMensajeErrorFinalizarVenta(resp) {
    const msg = resp?.msg || '';
    return msg || 'No se pudo finalizar la venta';
}

function esLineaAnuladaPOS(linea) {
    return linea && (linea.anulado === true || linea.anulado === 1 || linea.anulado === '1' || linea.anulado === 'true');
}

function finalizarVenta(data) {

    if (ventaEnProceso) return;
    if (window.POSGuard && !window.POSGuard.startAction('venta:finalizar')) return;

    const lineasParaFinalizar = window.POSState?.getLineas?.() || window.lineasVenta || [];

    if (!lineasParaFinalizar.length) {
        Swal.fire({
            icon: 'warning',
            title: 'Venta vacía',
            text: 'No hay productos cargados en la venta'
        });
        window.POSFinalizandoVenta = false;
        window.POSGuard?.endAction('venta:finalizar');
        return;
    }

    window.POSFinalizandoVenta = true;
    window.POSVentaFinalizada = false;
    setEstadoVentaEnProceso(true);

    const lineasPayload = lineasParaFinalizar.map(l => ({
        Codigo: l.codigo,
        CantKg: parseNumeroPOS(l.cant),
        PrecioKg: parseNumeroPOS(l.precio),
        Bonificacion: parseNumeroPOS(l.bonificacion) || 0,
        Estado: (esLineaAnuladaPOS(l) ? 1 : 0),
        IndexAnulado: Number.isFinite(parseInt(l.indexAnulado, 10)) ? parseInt(l.indexAnulado, 10) : -1,
        Balanza: l.balanza,
        IdExpendio: parseInt(l.idExpendio, 10) || 0
    }));

    const lineaInvalida = lineasPayload.find(l =>
        l.Estado !== 1 && (
            !l.Codigo ||
            !Number.isFinite(l.CantKg) || l.CantKg < 0.010 ||
            !Number.isFinite(l.PrecioKg) || l.PrecioKg <= 0
        )
    );

    if (lineaInvalida) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Hay una línea de venta con cantidad, precio o código inválido. Vuelva a cargar el producto.'
        });
        window.POSFinalizandoVenta = false;
        window.POSVentaFinalizada = false;
        setEstadoVentaEnProceso(false);
        window.POSGuard?.endAction('venta:finalizar');
        if (data.omitirPostVenta) {
            window.POSCancelacionEnCurso = false;
            $(document).trigger('ventaCancelacion:error');
        }
        return;
    }

    const payload = {
        idVenta: parseInt($('#idVentaEditar').val(), 10) || 0,
        fechaVenta: ($('#fechaVenta').val() || null),
        formaPago: data.formaPago,
        esPagoMixto: data.esPagoMixto,
        efectivo: data.efectivo,
        idPersona: data.idPersona,
        idSucursalPOS: parseInt($('#idSucursalPOS').val(), 10) || 0,
        soloFormaPago: !!window.POSModo?.soloFormaPago,
        Observaciones: window.POSState?.getObservaciones?.() || '',
        listaExpendios: window.POSState?.getListaExpendios?.() || [],
        lineasVenta: lineasPayload,
        posInstanceId: window.POSModo?.instanceId || ''
    };

    // El filtro global de antiforgery (Web/Filters/ValidateAppAntiForgeryTokenAttribute.cs) exige el token
    // en el form o en el header RequestVerificationToken. Como este POST manda JSON crudo, Request.Form
    // queda vacio y hay que mandarlo a mano en el header (el auto-inject de modal-request-loading.js
    // no llega a tiempo/no corre en algunos flujos de navegacion del POS y el server devuelve 400 siempre).
    const tokenAntiForgeryFinalizar = document.querySelector('#globalAntiForgeryToken input[name="__RequestVerificationToken"]')?.value
        || document.querySelector('input[name="__RequestVerificationToken"]')?.value
        || '';

    $.ajax({
        url: payload.idVenta > 0 ? api.venta.modificar : api.venta.finalizar,
        type: 'POST',
        contentType: 'application/json',
        dataType: 'json', // 🔥 CLAVE
        headers: { RequestVerificationToken: tokenAntiForgeryFinalizar },
        data: JSON.stringify(payload),

        success: function (resp) {

            if (!resp.ok) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: getMensajeErrorFinalizarVenta(resp)
                });
                window.POSFinalizandoVenta = false;
                window.POSVentaFinalizada = false;
                setEstadoVentaEnProceso(false);
                window.POSGuard?.endAction('venta:finalizar');
                if (data.omitirPostVenta) {
                    window.POSCancelacionEnCurso = false;
                    $(document).trigger('ventaCancelacion:error');
                }
                return;
            }


            // ✅ Venta OK
            window.POSFinalizandoVenta = true;
            window.POSVentaFinalizada = true;
            window.desactivarAvisoSalidaPOS?.();
            window.POSDraft?.clear?.();

            const ventaId = resp.ventaId;
            const tel = resp.whatsapp || ''; // o de donde lo saques

            if (data.omitirPostVenta) {
                window.POSCancelacionEnCurso = false;
                setEstadoVentaEnProceso(false);
                window.POSGuard?.endAction('venta:finalizar');
                if (typeof window.resetPOSDespuesDeFinalizar === 'function') {
                    window.resetPOSDespuesDeFinalizar();
                } else {
                    window.POSState?.clear?.();
                    window.location.href = (window.AppUrls && window.AppUrls.ventasPos) || (window.api && window.api.venta && window.api.venta.pos) || baseUrl;
                }
                return;
            }

            if (payload.idVenta > 0) {
                if (window.POSModo?.soloFormaPago) {
                    window.POSSoloFormaPagoGuardado = true;
                }
                const $fpEdicion = $('#modalFormaPago');
                $fpEdicion.find(':focus').trigger('blur');
                if (document.activeElement) document.activeElement.blur();
                $fpEdicion.modal('hide');

                Swal.fire({
                    icon: 'success',
                    title: 'Venta actualizada',
                    text: 'Los cambios se guardaron correctamente.'
                }).then(function () {
                    const returnUrl = window.POSModo?.returnUrl || $('#returnUrlPOS').val() || '';
                    if (returnUrl) {
                        window.location.href = returnUrl;
                        return;
                    }

                    window.location.href = api.venta.detalle + '?id=' + encodeURIComponent(ventaId);
                });

                hayVentaEnCurso = false;
                window.POSGuard?.endAction('venta:finalizar');
                setEstadoVentaEnProceso(false);
                return;
            }

            const $fp = $('#modalFormaPago');
            const requiereFacturaAutomatica = window.VentasFacturaModal
                && typeof window.VentasFacturaModal.requiereFacturaAutomatica === 'function'
                && window.VentasFacturaModal.requiereFacturaAutomatica(payload.formaPago);

            // 1) Sacar el foco de adentro ANTES de ocultar (evita el warning)
            $fp.find(':focus').trigger('blur');
            if (document.activeElement) document.activeElement.blur();

            // 2) Cuando el modal terminó de ocultarse, recién ahí abrimos PostVenta
            $fp.one('hidden.bs.modal', function () {
                if (requiereFacturaAutomatica && window.VentasFacturaModal && typeof window.VentasFacturaModal.abrir === 'function') {
                    window.VentasFacturaModal.abrir(ventaId, { facturaObligatoria: true });
                    return;
                }

                window.mostrarModalPostVenta(ventaId, tel);
            });

            // 3) Ocultar modal forma pago
            $fp.modal('hide');

            hayVentaEnCurso = false;
        },

        error: function (xhr) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: getMensajeErrorFinalizarVenta(xhr.responseJSON) || 'Error del servidor'
            });
            window.POSFinalizandoVenta = false;
            window.POSVentaFinalizada = false;
            setEstadoVentaEnProceso(false);
            window.POSGuard?.endAction('venta:finalizar');
            if (data.omitirPostVenta) {
                window.POSCancelacionEnCurso = false;
                $(document).trigger('ventaCancelacion:error');
            }
        }
    });

}


// ===============================
// RESET MIXTO
// ===============================
function resetPagoMixto() {

    otroTipoPagoSeleccionado = null;
    limpiarSeleccionFormaPago();

    $('#chkPagoMixto').prop('checked', false);
    $('#montoEfectivo').val('');
    $('#montoOtroPago').val('');
    $('#labelOtroPago').text('Otro Medio');

    if (!ventaEnProceso) {
        bloquearFormasPagoNoPreseleccionadas();
        $('#chkPagoMixto').prop('disabled', getModoFormaPagoActual() !== 'finalizacion');
        $('#btnFinalizarPagoMixto').prop('disabled', false);
    }
}

// ===============================
// ATAJOS DE TECLADO
// ===============================
$(document).ready(function () {

    $(document).on('keydown', function (e) {

        if (ventaEnProceso) return;
        if (!$('#modalFormaPago').hasClass('show')) return;
        if (window.POSGuard && !window.POSGuard.isModalOnTop('#modalFormaPago')) return;

        const esPagoMixto = $('#chkPagoMixto').is(':checked');

        // ---- MIXTO ----
        if (esPagoMixto) {
            if (e.key === 'End') {
                e.preventDefault();
                $('#btnFinalizarPagoMixto').click();
            }
            return;
        }

        const mapa = { '1': 'Efectivo', '2': 'Debito', '3': 'Credito', '4': 'CtaCte', '5': 'Qr', '6': 'Transferencia' };
        const k = mapa[e.key] || mapa[(e.code || '').replace('Numpad', '')];
        if (k) window.seleccionarFormaPago(k);

    });
});
