(function () {

    // El auto-inject de modal-request-loading.js (ajaxSend global) no siempre llega a tiempo en
    // los flujos de modal del POS -- mismo problema ya encontrado y resuelto en forma-pago.js
    // para FinalizarVenta/ModificarVenta (ver ese archivo). _FacturaElectronica.cshtml ademas
    // tiene su propio @Html.AntiForgeryToken() comentado, asi que el $.post de este modulo
    // dependia 100% del fallback global -- si ese timing fallaba, el filtro antiforgery global
    // (Web/Filters/ValidateAppAntiForgeryTokenAttribute) rechazaba el POST con 400 y el jQuery
    // .fail() mostraba "Error en la peticion", dejando la venta colgada (bug real reportado
    // probando "Cerrar sin facturar", 2026-08-21, ver docs/DECISIONS.md). Mismo fix en los 5
    // $.post de este archivo: leer el token a mano y mandarlo siempre por header.
    function tokenAntiForgery() {
        const input = document.querySelector('#globalAntiForgeryToken input[name="__RequestVerificationToken"]')
            || document.querySelector('input[name="__RequestVerificationToken"]');
        return (input && input.value) || '';
    }

    // =========================
    // Helpers formato
    // =========================
    const nfMoney = new Intl.NumberFormat('es-AR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    const nfQty = new Intl.NumberFormat('es-AR', {
        minimumFractionDigits: 3,
        maximumFractionDigits: 3
    });

    function toServerDec(n, dec) {
        const x = Number(n);
        const safe = isNaN(x) ? 0 : x;
        return safe.toFixed(dec).replace('.', ',');
    }

    function toNum(v) {
        if (v == null) return 0;

        let s = String(v).trim();
        s = s.replace(/\s/g, '');

        if (s.indexOf('.') !== -1 && s.indexOf(',') !== -1) {
            // 1.234,56 -> 1234.56
            s = s.replace(/\./g, '').replace(',', '.');
        } else if (s.indexOf(',') !== -1 && s.indexOf('.') === -1) {
            // 123,45 -> 123.45
            s = s.replace(',', '.');
        }

        const n = parseFloat(s);
        return isNaN(n) ? 0 : n;
    }

    function clamp(n, min, max) {
        if (n < min) return min;
        if (n > max) return max;
        return n;
    }

    // =========================
    // Estado
    // =========================
    let updating = false;

    function $root() {
        return $('#formFacturaElectronica');
    }

    function isYaEmitida() {
        return $root().data('ya-emitida') === 1 || $root().data('ya-emitida') === "1";
    }

    function esModoSinVenta() {
        return $root().data('sin-venta') === 1 || $root().data('sin-venta') === "1";
    }

    function normalizarFormaPagoFactura(valor) {
        const texto = String(valor || '').trim().toLowerCase();
        if (!texto) return '';

        if (texto.indexOf('ctacte') >= 0 || texto.indexOf('cta cte') >= 0 || texto.indexOf('cta. cte') >= 0) {
            return 'ctacte';
        }

        if (texto.indexOf('efectivo') >= 0 || texto.indexOf('efvo') >= 0) {
            return 'efectivo';
        }

        if (texto.indexOf('debito') >= 0 || texto.indexOf('débito') >= 0) {
            return 'debito';
        }

        if (texto.indexOf('credito') >= 0 || texto.indexOf('crédito') >= 0) {
            return 'credito';
        }

        if (texto.indexOf('transferencia') >= 0) {
            return 'transferencia';
        }

        if (texto === 'qr' || texto.indexOf('qr') >= 0) {
            return 'qr';
        }

        return texto;
    }

    function requiereConfirmacionCancelarSinFacturar() {
        if (isYaEmitida()) return false;

        const formaPago = normalizarFormaPagoFactura($root().find('input[name="FormaPago"]').val());
        return formaPago && formaPago !== 'efectivo' && formaPago !== 'ctacte';
    }

    function dataNum(name, fallback) {
        const v = $root().data(name);
        const n = parseFloat(String(v));
        return isNaN(n) ? (fallback || 0) : n;
    }

    function totalOriginal() {
        return dataNum('total-orig', toNum($('#feInputTotal').val()) || toNum($('#feInputTotalUI').val()));
    }

    function netoOriginal() {
        return dataNum('neto-orig', toNum($('#feInputNeto').val()) || toNum($('#feInputNetoUI').val()));
    }

    function ivaOriginal() {
        return dataNum('iva-orig', toNum($('#feInputIva').val()) || toNum($('#feInputIvaUI').val()));
    }

    function showPctWarning(show) {
        const $w = $('#fePctWarning');
        if (show) {
            $w.show();
        } else {
            $w.hide();
        }
    }

    function setPorcentaje(p) {
        $('#fePorcentajeFacturacion').val(toServerDec(p || 0, 4));

        const $ui = $('#feInputPorcentajeUI');
        if (document.activeElement === $ui[0]) return;

        $ui.val(toServerDec(p || 0, 2));
    }

    function setTotalFacturar(t) {
        const $input = $('#feInputTotalFacturar');
        if (document.activeElement === $input[0]) return;

        $input.val(toServerDec(t || 0, 2));
    }

    function setTotales(totales) {
        const total = totales.total || 0;
        const neto = totales.neto || 0;
        const iva = totales.iva || 0;

        // Hidden
        $('#feInputTotal').val(toServerDec(total, 2));
        $('#feInputNeto').val(toServerDec(neto, 2));
        $('#feInputIva').val(toServerDec(iva, 2));

        // UI
        $('#feInputTotalUI').val(nfMoney.format(total));
        $('#feInputNetoUI').val(nfMoney.format(neto));
        $('#feInputIvaUI').val(nfMoney.format(iva));
    }

    function modoEdicion() {
        return $('#feSwitchAjuste').is(':checked');
    }

    function modo() {
        return $('#feModoTotal').is(':checked') ? 'total' : 'porcentaje';
    }

    function porcentajeActual() {
        return toNum($('#fePorcentajeFacturacion').val());
    }

    function esFacturacionParcial() {
        return porcentajeActual() < 99.9999;
    }

    function asegurarDescripcionAgrupada() {
        const $desc = $('#feDescItemUnitario');
        if (!$desc.val().trim()) {
            $desc.val('Productos varios');
        }
    }

    function actualizarEstadoAgruparItemUnitario() {
        // En modo sin venta asociada agrupar en item individual es obligatorio (no
        // se puede apagar), igual que ya pasa hoy con la facturacion parcial.
        const forzarAgrupacion = esFacturacionParcial() || esModoSinVenta();
        const $check = $('#feAgruparItemUnitario');

        if (forzarAgrupacion) {
            $check.prop('checked', true);
            asegurarDescripcionAgrupada();
        }

        $check.prop('disabled', forzarAgrupacion);
        syncAgruparItemUnitario();
    }

    function syncAgruparItemUnitario() {
        const activo = $('#feAgruparItemUnitario').is(':checked');
        const $desc = $('#feDescItemUnitario');

        $('#feAgruparItemUnitarioHidden').val(activo ? 'true' : 'false');
        $('#feAgruparItemWrap').toggle(activo);
        $desc.prop('disabled', !activo);

        if (activo && !$desc.val()) {
            asegurarDescripcionAgrupada();
        }
    }

    function syncObservacionesComprobante() {
        const activo = $('#feUsarObservaciones').is(':checked');
        const $obs = $('#feObservaciones');

        $('#feObservacionesWrap').toggle(activo);
        $obs.prop('disabled', !activo);
    }

    function syncResumenSticky() {
        const $tipo = $root().find('select[name="CodTipoCbteAfip"]');
        const $pto = $root().find('input[name="PtoVtaAfip"]');
        const $cliente = $root().find('input[name="RazonSocialAFIP"]');

        $('#feStickyTipo').text(($tipo.find('option:selected').text() || '').trim() || '-');
        $('#feStickyPuntoVenta').text(($pto.val() || '').trim() || '-');
        $('#feStickyCliente').text(($cliente.val() || '').trim() || '-');
    }

    function generarNotaCredito(anularVenta) {
        const idFactura = parseInt($root().find('input[name="IdFactura"]').val(), 10) || 0;
        if (!idFactura) {
            Swal.fire({
                icon: 'error',
                title: 'Nota de crédito',
                text: 'No se encontró la factura origen.'
            });
            return;
        }

        const $btn = $('#btnGenerarNotaCredito').prop('disabled', true);

        $.ajax({
            url: (window.AppUrls && window.AppUrls.ventasGenerarNotaCredito) || '/Ventas/GenerarNotaCredito',
            type: 'POST',
            headers: { RequestVerificationToken: tokenAntiForgery() },
            data: {
                idFactura: idFactura,
                anularVenta: !!anularVenta
            }
        })
            .done(function (resp) {
                if (resp && resp.ok) {
                    const titulo = resp.already ? 'Nota de crédito existente' : 'Nota de crédito generada';
                    const texto = resp.mensaje || (resp.already
                        ? 'Ya existe una nota de crédito para esta venta.'
                        : 'La nota de crédito se generó correctamente.');

                    Swal.fire({
                        icon: 'success',
                        title: titulo,
                        text: texto
                    }).then(function () {
                        if (resp.detalleUrl) {
                            window.location.href = resp.detalleUrl;
                            return;
                        }

                        window.location.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: (resp && resp.msg) ? resp.msg : 'No se pudo generar la nota de crédito.'
                    });
                }
            })
            .fail(function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Error en la petición'
                });
            })
            .always(function () {
                $btn.prop('disabled', false);
            });
    }

    function habilitarInputsEdicion(on) {
        const editable = !!on;

        $('#feAjusteBlock').toggleClass('d-none', !editable);

        $('#feInputPorcentajeUI').prop('readonly', !editable || modo() !== 'porcentaje');
        $('#feInputTotalFacturar').prop('readonly', !editable || modo() !== 'total');

        $('#feModoPorcentaje, #feModoTotal').prop('disabled', !editable);

        if (editable) {
            if (modo() === 'porcentaje') {
                $('#feInputPorcentajeUI').focus().select();
            } else {
                $('#feInputTotalFacturar').focus().select();
            }
        }
    }

    function getFilasItems() {
        return $('#feTablaItems tbody tr.fe-item');
    }

    function renderTablaIva(grupos) {
        const $tb = $('#feTablaIva tbody');
        $tb.empty();

        const keys = Object.keys(grupos || {}).sort((a, b) => parseFloat(a) - parseFloat(b));

        keys.forEach(k => {
            const g = grupos[k];
            const aliTxt = (g.ali % 1 === 0) ? g.ali.toFixed(0) : g.ali.toFixed(1);

            $tb.append(`
                <tr>
                    <td>${aliTxt}%</td>
                    <td class="text-right">${nfMoney.format(g.neto)}</td>
                    <td class="text-right">${nfMoney.format(g.iva)}</td>
                    <td class="text-right font-weight-bold">${nfMoney.format(g.total)}</td>
                </tr>
            `);
        });
    }

    // =========================
    // Recalcular
    // =========================
    function recalcularDesdePorcentaje(pct) {
        const p = clamp(pct, 0, 100);
        const factor = p / 100;

        const $filas = getFilasItems();

        // Fallback robusto:
        // si no hay filas cargadas, no pisamos con 0; escalamos desde los totales originales
        if (!$filas.length) {
            const total = totalOriginal() * factor;
            const neto = netoOriginal() * factor;
            const iva = ivaOriginal() * factor;

            const r = {
                total: total,
                neto: neto,
                iva: iva,
                porcentaje: p
            };

            setTotales(r);
            renderTablaIva({});
            return r;
        }

        let total = 0;
        let neto = 0;
        let iva = 0;

        const grupos = {};

        $filas.each(function () {
            const $tr = $(this);

            const qty = toNum($tr.data('qty'));
            const precioOrig = toNum($tr.data('precio-orig'));
            const ali = toNum($tr.data('iva'));

            const precio = precioOrig * factor;
            const subtotal = qty * precio;

            $tr.find('.fe-precio').text(toServerDec(precio, 2));
            $tr.find('.fe-subtotal').text(toServerDec(subtotal, 2));
            $tr.find('.fe-qty').text(nfQty.format(qty));
            $tr.find('.fe-iva').text((ali || 0).toFixed(ali % 1 === 0 ? 0 : 1));

            total += subtotal;

            const divisor = 1 + (ali / 100);
            const net = divisor > 0 ? (subtotal / divisor) : subtotal;
            const iv = subtotal - net;

            neto += net;
            iva += iv;

            const key = (ali || 0).toFixed(2);
            if (!grupos[key]) {
                grupos[key] = {
                    ali: ali || 0,
                    neto: 0,
                    iva: 0,
                    total: 0
                };
            }

            grupos[key].neto += net;
            grupos[key].iva += iv;
            grupos[key].total += subtotal;
        });

        const r = {
            total: total,
            neto: neto,
            iva: iva,
            porcentaje: p
        };

        setTotales(r);
        renderTablaIva(grupos);

        return r;
    }

    function aplicarPorcentaje(p) {
        if (updating) return;
        updating = true;

        try {
            const r = recalcularDesdePorcentaje(p);
            setPorcentaje(r.porcentaje);
            setTotalFacturar(r.total);
            actualizarEstadoAgruparItemUnitario();
        } finally {
            updating = false;
        }
    }

    function aplicarTotalObjetivo(totalObj) {
        if (updating) return;
        updating = true;

        try {
            const orig = totalOriginal();
            const t = Math.max(0, totalObj || 0);

            let pct = 0;
            if (orig > 0) {
                pct = (t / orig) * 100;
            }

            pct = clamp(pct, 0, 100);

            const r = recalcularDesdePorcentaje(pct);
            setPorcentaje(r.porcentaje);
            setTotalFacturar(r.total);
        } finally {
            updating = false;
        }
    }

    // =========================
    // Alturas modal
    // =========================
    function ajustarAlturasModal() {
        try {
            const $modal = $('#modalFacturaElectronica');
            if (!$modal.length) return;

            const vh = Math.max(window.innerHeight || document.documentElement.clientHeight, 600);
            const maxModal = Math.round(vh * 0.92);
            const headerH = $modal.find('.modal-header').outerHeight(true) || 0;
            const footerH = $modal.find('.modal-footer').outerHeight(true) || 0;
            const contentPad = 24;

            const bodyMax = Math.max(120, maxModal - (headerH + footerH + contentPad));
            $modal.find('.modal-body.fe-body').css('max-height', bodyMax + 'px');

            const topH = $modal.find('.fe-top').outerHeight(true) || 0;
            const summaryH = $modal.find('.fe-summary').outerHeight(true) || 0;
            const availForTable = Math.max(120, bodyMax - (topH + summaryH + 24));

            $modal.find('.fe-table-scroll').css('max-height', availForTable + 'px');
        } catch (e) {
            console.warn('ajuste alturas modal', e);
        }
    }

    // =========================
    // Init
    // =========================
    function initFacturaModal() {
        const $f = $root();
        if (!$f.length) return;
        const porcentajeInicial = clamp(porcentajeActual() || 100, 0, 100);
        const ajusteInicialActivo = porcentajeInicial < 99.9999;

        // Estado inicial consistente
        $('#feModoPorcentaje').prop('checked', true);
        $('#feModoTotal').prop('checked', false);
        $('#feSwitchAjuste').prop('checked', ajusteInicialActivo);

        setPorcentaje(porcentajeInicial);
        setTotalFacturar(totalOriginal());

        showPctWarning(false);
        $('#feInputPorcentajeUI').removeClass('is-invalid');
        $('#feAjusteBlock').toggleClass('d-none', !ajusteInicialActivo);

        if (isYaEmitida()) {
            $('#feInputPorcentajeUI').prop('readonly', true);
            $('#feInputTotalFacturar').prop('readonly', true);
            $('#feModoPorcentaje, #feModoTotal').prop('disabled', true);
            // No recalcular: los importes ya vienen correctos desde el server
            // (Model.ImporteTotal/ImporteNetoGravado/Iva, ya persistidos con el
            // % aplicado en su momento). Si se llamara a aplicarPorcentaje aca,
            // volveria a multiplicar ese total ya reducido por el mismo % de
            // nuevo (bug: 56% de un total ya facturado al 56% -> 31.36%).
        } else {
            habilitarInputsEdicion(ajusteInicialActivo);
            // Recalcular desde el % actual: solo aplica a una factura nueva
            // (el total todavia es el 100% de la venta).
            aplicarPorcentaje(porcentajeInicial);
        }

        $('#feAgruparItemUnitario').prop('checked', $f.data('agrupar-item') === 1 || $f.data('agrupar-item') === "1");
        actualizarEstadoAgruparItemUnitario();
        syncObservacionesComprobante();
        syncResumenSticky();

        setTimeout(() => {
            ajustarAlturasModal();

            const el = document.getElementById('feLblTotal');
            if (el && el.scrollIntoView) {
                el.scrollIntoView({ block: 'center', behavior: 'auto' });
            }

            $('#btnRegistrarFactura').trigger('focus');
        }, 150);
    }

    // =========================
    // Eventos
    // =========================
    $(document).on('shown.bs.modal', '#modalFacturaElectronica', function () {
        initFacturaModal();
        ajustarAlturasModal();
        $(window).on('resize.factura', ajustarAlturasModal);
    });

    $(document).on('hidden.bs.modal', '#modalFacturaElectronica', function () {
        $(window).off('resize.factura');
    });

    $(document).on('keydown', function (e) {
        const $modal = $('#modalFacturaElectronica');
        if (!$modal.hasClass('show')) return;
        if (window.Swal && Swal.isVisible && Swal.isVisible()) return;

        const esAsterisco = e.key === '*' || e.code === 'NumpadMultiply';
        if (esAsterisco) {
            const $btnCerrarSinFacturar = $('#btnCerrarVentaSinFacturar');
            if (!$btnCerrarSinFacturar.length || $btnCerrarSinFacturar.prop('disabled')) return;

            e.preventDefault();
            $btnCerrarSinFacturar.trigger('click');
            return;
        }

        // Supr (antes Alt+C, cambiado a pedido del usuario 2026-08-22): cerrar venta sin
        // facturar. Sin modificador, asi que a diferencia de Alt+C hay que ignorar el foco en
        // campos de texto -- si no, borrar texto seleccionado con Supr en Razon Social,
        // Observaciones, etc. dispararia el cierre en vez de borrar.
        if (e.key === 'Delete' && !e.altKey && !e.ctrlKey && !e.metaKey && !e.shiftKey && !e.repeat) {
            const tagName = (document.activeElement && document.activeElement.tagName || '').toLowerCase();
            if (tagName === 'input' || tagName === 'select' || tagName === 'textarea') return;

            const $btnCerrarSinFacturar = $('#btnCerrarVentaSinFacturar');
            if (!$btnCerrarSinFacturar.length || $btnCerrarSinFacturar.prop('disabled') || !$btnCerrarSinFacturar.is(':visible')) return;

            e.preventDefault();
            e.stopPropagation();
            $btnCerrarSinFacturar.trigger('click');
            return;
        }

        // Atajo Alt+R (registrar factura)
        const esAtajoValido = e.altKey && !e.ctrlKey && !e.metaKey && !e.shiftKey && !e.repeat;
        if (!esAtajoValido) return;

        const tecla = String(e.key || '').toLowerCase();
        if (tecla === 'r') {
            const $btnRegistrar = $('#btnRegistrarFactura');
            if (!$btnRegistrar.length || $btnRegistrar.prop('disabled') || !$btnRegistrar.is(':visible')) return;

            e.preventDefault();
            e.stopPropagation();
            $btnRegistrar.trigger('click');
        }
    });

    // Enter en un campo del formulario no debe disparar el submit (Registrar factura).
    // Si el foco esta en el propio boton, el comportamiento nativo (Enter = click) sigue andando
    // porque este handler solo escucha sobre input/select, nunca sobre el boton.
    $(document).on('keydown', '#formFacturaElectronica input, #formFacturaElectronica select', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
        }
    });

    $(document).on('change', '#feSwitchAjuste', function () {
        if (isYaEmitida()) return;

        const on = $(this).is(':checked');
        habilitarInputsEdicion(on);

        if (!on) {
            $('#feModoPorcentaje').prop('checked', true);
            $('#feModoTotal').prop('checked', false);
            habilitarInputsEdicion(false);
            aplicarPorcentaje(100);
            showPctWarning(false);
            $('#feInputPorcentajeUI').removeClass('is-invalid');
        } else {
            setTimeout(() => {
                const el = document.getElementById('feAjusteBlock');
                if (el && el.scrollIntoView) {
                    el.scrollIntoView({ block: 'center', behavior: 'smooth' });
                }
                ajustarAlturasModal();
            }, 80);
        }
    });

    $(document).on('change', '#feModoPorcentaje, #feModoTotal', function () {
        if (isYaEmitida()) return;
        habilitarInputsEdicion(modoEdicion());
        ajustarAlturasModal();
    });

    $(document).on('change', '#feAgruparItemUnitario', function () {
        if (esFacturacionParcial()) {
            $(this).prop('checked', true);
            asegurarDescripcionAgrupada();
        }

        syncAgruparItemUnitario();
        ajustarAlturasModal();
    });

    $(document).on('change', '#feUsarObservaciones', function () {
        syncObservacionesComprobante();
        ajustarAlturasModal();
    });

    $(document).on('change', '#formFacturaElectronica select[name="CodTipoCbteAfip"]', function () {
        syncResumenSticky();
    });

    $(document).on('input change', '#formFacturaElectronica input[name="RazonSocialAFIP"], #formFacturaElectronica input[name="PtoVtaAfip"]', function () {
        syncResumenSticky();
    });

    $(document).on('input', '#feInputPorcentajeUI', function () {
        if (isYaEmitida()) return;
        if (!modoEdicion() || modo() !== 'porcentaje') return;

        const raw = $(this).val();
        const p = toNum(raw);

        if (p > 100) {
            showPctWarning(true);
            $(this).addClass('is-invalid');
        } else {
            showPctWarning(false);
            $(this).removeClass('is-invalid');
        }

        aplicarPorcentaje(Math.min(p, 100));
    });

    $(document).on('blur', '#feInputPorcentajeUI', function () {
        const p = toNum($(this).val());
        const fixed = Math.min(Math.max(p || 0, 0), 100);

        $(this).val(toServerDec(fixed, 2));
        $('#fePorcentajeFacturacion').val(toServerDec(fixed, 4));
        ajustarAlturasModal();
    });

    $(document).on('input', '#feInputTotalFacturar', function () {
        if (isYaEmitida()) return;
        if (!modoEdicion() || modo() !== 'total') return;

        const t = toNum($(this).val());
        aplicarTotalObjetivo(t);
    });

    $(document).on('blur', '#feInputTotalFacturar', function () {
        const t = toNum($(this).val());
        $(this).val(toServerDec(t || 0, 2));
        ajustarAlturasModal();
    });

    // =========================
    // Facturación manual (sin venta asociada)
    // =========================
    function recalcularDesdeTotalManual() {
        const total = toNum($('#feMontoTotalManual').val());
        const alicuotaPct = toNum($('#feAlicuotaManual option:selected').data('pct'));
        const neto = alicuotaPct > 0 ? total / (1 + alicuotaPct / 100) : total;
        const iva = total - neto;

        setTotales({ total: total, neto: neto, iva: iva });
    }

    $(document).on('input change', '#feMontoTotalManual, #feAlicuotaManual', function () {
        if (!esModoSinVenta()) return;
        recalcularDesdeTotalManual();
    });

    $(document).on('shown.bs.modal', '#modalFacturaElectronica', function () {
        if (esModoSinVenta()) {
            recalcularDesdeTotalManual();
        }
    });

    // Buscar cliente: reusa la ventana _BuscarPersona.cshtml pero con logica propia
    // (la de persona-buscar.js escribe en #idPersona/#razonSocial, ids del POS que no
    // existen aca, y no expone el CUIT como data-attribute).
    function cargarPersonasFactura(filtro) {
        // PersonasController.Listar ya devuelve iva/domicilio/ciudad por persona (no hace
        // falta un endpoint aparte) -- se agregan como data-* para no pedirlos de nuevo al
        // seleccionar. Ver docs/DECISIONS.md, 2026-08-22.
        $.get(window.AppUrls.personaListar, { filtro: filtro || '' }, function (data) {
            let html = '';
            (data || []).forEach(function (p) {
                html += '<tr class="fila-persona" data-id="' + p.idPersona + '" data-razon="' + (p.razonSocial || '') + '" data-cuit="' + (p.cuit || '') + '"'
                    + ' data-iva="' + (p.iva || '') + '" data-domicilio="' + (p.domicilio || '') + '" data-ciudad="' + (p.ciudad || '') + '">'
                    + '<td>' + (p.cuit || '') + '</td>'
                    + '<td>' + (p.razonSocial || '') + '</td>'
                    + '<td>' + (p.identificacion || '') + '</td>'
                    + '</tr>';
            });
            $('#tablaPersonas').html(html);
            $('#tablaPersonas tr.fila-persona:first').addClass('is-selected');
        });
    }

    function seleccionarPersonaFactura($fila) {
        const idPersona = $fila.data('id');
        const razonSocial = $fila.data('razon') || '';
        const cuit = ($fila.data('cuit') || '').toString().replace(/-/g, '');
        const iva = $fila.data('iva') || '';
        const domicilio = $fila.data('domicilio') || '';
        const ciudad = $fila.data('ciudad') || '';

        $('#feIdPersonaSeleccionada').val(idPersona);
        $('#feRazonSocialAFIP').val(razonSocial);
        $('#NroDocAfip').val(cuit).trigger('input');
        $('#feTipoDocAfip').val(cuit ? '80' : '99');

        // Antes quedaban fijos en "Consumidor Final" / vacio sin importar el cliente elegido
        // (bug real, encontrado probando "Nueva factura sin venta" -- ver docs/DECISIONS.md,
        // 2026-08-22). El formato del domicilio replica el que arma BuildFacturaDTO
        // (VentasController.cs) para una venta real: "Domicilio - Ciudad".
        if (iva) {
            $('select[name="CondicionIvaAFIP"]').val(iva).trigger('change');
        }
        $('input[name="DomicilioAFIP"]').val(domicilio ? (domicilio + (ciudad ? ' - ' + ciudad : '')) : '');

        $('#modalBuscarPersona').modal('hide');
    }

    // A pedido del usuario (2026-08-22, ver docs/DECISIONS.md): elegir "Responsable Inscripto"
    // fuerza Tipo de Factura = A. Corre tanto al elegir cliente (seleccionarPersonaFactura
    // dispara "change" a mano) como si el usuario cambia el select de Cond. IVA directamente --
    // no interfiere con el calculo server-side (FacturaElectronica.getCodTipoCbteAFIP), que solo
    // corre una vez al abrir el modal; esto es puramente reactividad dentro del modal ya abierto.
    $(document).on('change', 'select[name="CondicionIvaAFIP"]', function () {
        if ($(this).val() === 'Responsable Inscripto') {
            $('select[name="CodTipoCbteAfip"]').val('1');
        }
    });

    $(document).on('click', '#btnFeBuscarCliente', function () {
        $('#modalBuscarPersona').modal('show');
        $('#filtroPersona').val('');
        cargarPersonasFactura('');
    });

    $(document).on('shown.bs.modal', '#modalBuscarPersona', function () {
        if (!esModoSinVenta()) return;
        $('#filtroPersona').trigger('focus');
    });

    $(document).on('input', '#filtroPersona', function () {
        if (!esModoSinVenta()) return;
        cargarPersonasFactura($(this).val());
    });

    $(document).on('click', '#tablaPersonas tr.fila-persona', function () {
        if (!esModoSinVenta()) return;
        $('#tablaPersonas tr.fila-persona').removeClass('is-selected');
        $(this).addClass('is-selected');
    });

    $(document).on('dblclick', '#tablaPersonas tr.fila-persona', function () {
        if (!esModoSinVenta()) return;
        seleccionarPersonaFactura($(this));
    });

    $(document).on('keydown', '#filtroPersona', function (e) {
        if (!esModoSinVenta() || e.key !== 'Enter') return;
        e.preventDefault();

        const $target = $('#tablaPersonas tr.fila-persona.is-selected').first().length
            ? $('#tablaPersonas tr.fila-persona.is-selected').first()
            : $('#tablaPersonas tr.fila-persona').first();

        if ($target.length) seleccionarPersonaFactura($target);
    });

    // =========================
    // Submit
    // =========================
    function enviarGenerarFactura($form, $btn, esSinVenta) {
        const datos = $form.serialize();

        $.ajax({
            url: (window.AppUrls && window.AppUrls.ventasGenerarFactura) || '/Ventas/GenerarFactura',
            type: 'POST',
            headers: { RequestVerificationToken: tokenAntiForgery() },
            data: datos
        })
            .done(function (resp) {
                if (resp && resp.ok) {
                    const esActualizacion = isYaEmitida() || resp.updated === true;
                    Swal.fire({
                        icon: 'success',
                        title: esActualizacion ? 'Factura actualizada' : 'Factura registrada',
                        text: esActualizacion ? (resp.msg || 'Los cambios se guardaron correctamente.') : ('Nro: ' + (resp.nro || ''))
                    });

                    $('#modalFacturaElectronica').modal('hide');
                    if (esActualizacion) {
                        $(document).trigger('factura:actualizada', [resp]);
                    } else {
                        $(document).trigger('venta:facturada', [resp]);
                    }

                    // La linea temporal de la venta manual ya cumplio su proposito
                    // (la factura ya tiene CAE): se limpia sin bloquear el cierre del modal.
                    if (esSinVenta) {
                        const idVentaCreada = parseInt($('#feIdVenta').val() || '0', 10) || 0;
                        if (idVentaCreada) {
                            $.ajax({
                                url: window.AppUrls.ventasLimpiarLineasVentaManual,
                                type: 'POST',
                                headers: { RequestVerificationToken: tokenAntiForgery() },
                                data: { idVenta: idVentaCreada }
                            });
                        }
                    }
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: (resp && resp.msg) ? resp.msg : 'Error desconocido'
                    });
                }
            })
            .fail(function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Error en la petición'
                });
            })
            .always(function () {
                $btn.prop('disabled', false);
            });
    }

    $(document).on('submit', '#formFacturaElectronica', function (e) {
        e.preventDefault();

        const $form = $(this);
        if (!$('#feAgruparItemUnitario').is(':checked') || $('#feDescItemUnitario').is(':disabled')) {
            $('#feAgruparItemUnitarioHidden').val('false');
            $('#feDescItemUnitario').val('');
        }
        if (!$('#feUsarObservaciones').is(':checked') || $('#feObservaciones').is(':disabled')) {
            $('#feObservaciones').val('');
        }

        const $btn = $form.find('#btnRegistrarFactura').prop('disabled', true);
        const esSinVenta = esModoSinVenta();

        if (!esSinVenta) {
            enviarGenerarFactura($form, $btn, false);
            return;
        }

        // Modo sin venta asociada: primero se crea una venta real minima (Efectivo,
        // 1 linea con el Total ingresado) para que GenerarFactura funcione sin cambios.
        const idPersona = parseInt($('#feIdPersonaSeleccionada').val() || '0', 10) || 0;
        const montoTotal = toNum($('#feMontoTotalManual').val());
        const idAlicuota = parseInt($('#feAlicuotaManual').val() || '0', 10) || 0;
        const alicuotaPct = toNum($('#feAlicuotaManual option:selected').data('pct'));

        if (!idPersona) {
            Swal.fire({ icon: 'error', title: 'Error', text: 'Seleccioná un cliente.' });
            $btn.prop('disabled', false);
            return;
        }
        if (!montoTotal || montoTotal <= 0) {
            Swal.fire({ icon: 'error', title: 'Error', text: 'Ingresá el monto total a facturar.' });
            $btn.prop('disabled', false);
            return;
        }

        $.ajax({
            url: window.AppUrls.ventasCrearVentaManual,
            type: 'POST',
            headers: { RequestVerificationToken: tokenAntiForgery() },
            data: {
                idPersona: idPersona,
                montoTotal: toServerDec(montoTotal, 2),
                idAlicuotaIva: idAlicuota,
                alicuotaIva: alicuotaPct
            }
        })
            .done(function (resp) {
                if (!resp || !resp.ok) {
                    Swal.fire({ icon: 'error', title: 'Error', text: (resp && resp.msg) ? resp.msg : 'No se pudo crear la venta.' });
                    $btn.prop('disabled', false);
                    return;
                }

                $('#feIdVenta').val(resp.idVenta);
                enviarGenerarFactura($form, $btn, true);
            })
            .fail(function () {
                Swal.fire({ icon: 'error', title: 'Error', text: 'Error en la petición' });
                $btn.prop('disabled', false);
            });
    });

    $(document).on('click', '#btnCerrarVentaSinFacturar', function () {
        const $btn = $(this);
        const $form = $('#formFacturaElectronica');
        const idVenta = parseInt($form.find('input[name="IdVenta"]').val() || '0', 10) || 0;

        if (!idVenta) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'No se encontró la venta a cerrar.'
            });
            return;
        }

        Swal.fire({
            icon: 'warning',
            title: 'Cerrar venta sin facturar',
            text: 'Se cerrará la venta sin emitir factura electrónica y se continuará con una nueva venta.',
            showCancelButton: true,
            focusConfirm: true,
            confirmButtonText: 'Sí, cerrar venta',
            cancelButtonText: 'Cancelar',
            didOpen: function () {
                // Este Swal se abre encima del modal Bootstrap _FacturaElectronica, ya abierto.
                // Bootstrap 4 reafirma el foco dentro de SU modal via un handler global de
                // "focusin" en document (_enforceFocus, Content/vendor/bootstrap/js/bootstrap.js)
                // que le devuelve el foco a su propio elemento apenas detecta que se movio a algo
                // que no es descendiente suyo -- como el boton de SweetAlert2 cuelga de <body>,
                // por fuera del modal Bootstrap, le ganaba la pulseada al .focus() de aca abajo y
                // el usuario nunca veia el foco puesto en "Si, cerrar venta". Se desactiva ese
                // handler mientras este Swal esta abierto; no hace falta restaurarlo a mano,
                // Bootstrap ya lo desactiva el solo al cerrar el modal (_hideModal).
                $(document).off('focusin.bs.modal');
                var confirmButton = Swal.getConfirmButton ? Swal.getConfirmButton() : null;
                if (confirmButton && confirmButton.focus) {
                    confirmButton.focus();
                }
            }
        }).then(function (result) {
            if (!result.isConfirmed) return;

            $btn.prop('disabled', true);
            $('#btnRegistrarFactura').prop('disabled', true);

            $.ajax({
                url: (window.AppUrls && window.AppUrls.ventasCerrarSinFacturar) || '/Ventas/CerrarVentaSinFacturar',
                type: 'POST',
                headers: { RequestVerificationToken: tokenAntiForgery() },
                data: { idVenta: idVenta }
            })
                .done(function (resp) {
                    if (resp && resp.ok) {
                        $('#modalFacturaElectronica').modal('hide');
                        $(document).trigger('venta:cerradaSinFacturar', [resp]);
                        return;
                    }

                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: (resp && resp.msg) ? resp.msg : 'No se pudo cerrar la venta sin facturar.'
                    });
                })
                .fail(function () {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'Error en la petición'
                    });
                })
                .always(function () {
                    $btn.prop('disabled', false);
                    $('#btnRegistrarFactura').prop('disabled', false);
                });
        });
    });

    // La X del header no debe cerrar el modal "por atras": debe pasar por
    // la misma logica que el boton Cancelar (que ya decide si hace falta
    // forzar "Cerrar venta sin facturar" segun la forma de pago).
    $(document).on('click', '#btnCerrarXFacturaElectronica', function (e) {
        e.preventDefault();
        $('#btnCancelarFacturaElectronica').trigger('click');
    });

    $(document).on('click', '#btnCancelarFacturaElectronica', function (e) {
        if (!requiereConfirmacionCancelarSinFacturar()) return;

        const $btnCerrarSinFacturar = $('#btnCerrarVentaSinFacturar');
        if (!$btnCerrarSinFacturar.length || $btnCerrarSinFacturar.prop('disabled')) return;

        e.preventDefault();
        e.stopImmediatePropagation();
        $btnCerrarSinFacturar.trigger('click');
    });

    $(document).on('click', '#btnGenerarNotaCredito', function () {
        if (!window.Swal) return;

        Swal.fire({
            icon: 'question',
            title: 'Generar nota de crédito',
            text: 'Elegí si además querés anular la venta asociada.',
            showCancelButton: true,
            showDenyButton: true,
            confirmButtonText: 'Generar y anular venta',
            denyButtonText: 'Generar sin anular',
            cancelButtonText: 'Cancelar'
        }).then(function (result) {
            if (result.isConfirmed) {
                generarNotaCredito(true);
            } else if (result.isDenied) {
                generarNotaCredito(false);
            }
        });
    });

})();
