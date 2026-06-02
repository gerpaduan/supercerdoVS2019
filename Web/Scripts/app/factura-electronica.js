(function () {

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
        $('#feInputTotalUI').val(toServerDec(total, 2));
        $('#feInputNetoUI').val(toServerDec(neto, 2));
        $('#feInputIvaUI').val(toServerDec(iva, 2));
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
        const forzarAgrupacion = esFacturacionParcial();
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

        $.post((window.AppUrls && window.AppUrls.ventasGenerarNotaCredito) || '/Ventas/GenerarNotaCredito', {
            idFactura: idFactura,
            anularVenta: !!anularVenta
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
        } else {
            habilitarInputsEdicion(ajusteInicialActivo);
        }

        // Recalcular o restaurar respetando el porcentaje guardado
        aplicarPorcentaje(porcentajeInicial);

        $('#feAgruparItemUnitario').prop('checked', $f.data('agrupar-item') === 1 || $f.data('agrupar-item') === "1");
        actualizarEstadoAgruparItemUnitario();
        syncObservacionesComprobante();

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
    // Submit
    // =========================
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
        const datos = $form.serialize();

        $.post((window.AppUrls && window.AppUrls.ventasGenerarFactura) || '/Ventas/GenerarFactura', datos)
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
