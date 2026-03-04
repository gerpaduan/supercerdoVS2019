(function () {

    // =========================
    // Helpers formato
    // =========================
    const nfMoney = new Intl.NumberFormat('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    const nfQty = new Intl.NumberFormat('es-AR', { minimumFractionDigits: 3, maximumFractionDigits: 3 });

    function toServerDec(n, dec) {
        const x = (n || 0);
        // toFixed usa punto; lo convertimos a coma para es-AR
        return x.toFixed(dec).replace('.', ',');
    }

    function toNum(v) {
        if (v == null) return 0;
        let s = String(v).trim();

        // Normalizar espacios
        s = s.replace(/\s/g, '');

        // Caso: tiene ambos separadores -> asumimos '.' miles y ',' decimal
        if (s.indexOf('.') !== -1 && s.indexOf(',') !== -1) {
            s = s.replace(/\./g, '').replace(',', '.');
        }
        // Solo tiene ',' -> coma decimal
        else if (s.indexOf(',') !== -1 && s.indexOf('.') === -1) {
            s = s.replace(',', '.');
        }
        // Solo tiene '.' -> punto decimal (no tocar)
        // No separadores -> se parsea directo
        const n = parseFloat(s);
        return isNaN(n) ? 0 : n;
    }

    function money(n) { return '$ ' + nfMoney.format(n || 0); }

    function clamp(n, min, max) {
        if (n < min) return min;
        if (n > max) return max;
        return n;
    }

    // =========================
    // Estado por modal (evitar loops)
    // =========================
    let updating = false;

    function $root() { return $('#formFacturaElectronica'); }

    function isYaEmitida() {
        return $root().data('ya-emitida') === 1 || $root().data('ya-emitida') === "1";
    }

    function totalOriginal() {
        const v = $root().data('total-orig');
        const n = parseFloat(String(v));
        return isNaN(n) ? 0 : n;
    }

    function getPorcentaje() {
        return toNum($('#feInputPorcentajeUI').val()) || 0;
    }

    function showPctWarning(show) {
        const $w = $('#fePctWarning');
        if (show) $w.show(); else $w.hide();
    }

    function setPorcentaje(p) {
        // hidden (POST) con coma
        $('#fePorcentajeFacturacion').val(toServerDec(p || 0, 4));

        // UI (si no está editando)
        const $ui = $('#feInputPorcentajeUI');
        if (document.activeElement === $ui[0]) return;
        $ui.val(toServerDec(p || 0, 2));
    }

    function setTotalFacturar(t) {
        const $input = $('#feInputTotalFacturar');
        if (document.activeElement === $input[0]) {
            return;
        }
        $input.val((t || 0).toFixed(2));
    }

    function modoEdicion() {
        return $('#feSwitchAjuste').is(':checked');
    }

    function modo() {
        return $('#feModoTotal').is(':checked') ? 'total' : 'porcentaje';
    }

    function habilitarInputsEdicion(on) {
        const editable = !!on;

        // Mostrar/ocultar bloque de ajuste
        $('#feAjusteBlock').toggleClass('d-none', !editable);

        // Inputs readonly según modo y editabilidad
        $('#feInputPorcentajeUI').prop('readonly', !editable || modo() !== 'porcentaje');
        $('#feInputTotalFacturar').prop('readonly', !editable || modo() !== 'total');

        // Radios
        $('#feModoPorcentaje, #feModoTotal').prop('disabled', !editable);

        // cuando se habilita, enfocamos el input adecuado para que el usuario comience a editar
        if (editable) {
            if (modo() === 'porcentaje') {
                $('#feInputPorcentajeUI').focus().select();
            } else {
                $('#feInputTotalFacturar').focus().select();
            }
        }
    }

    // =========================
    // Recalcular tabla + IVA
    // =========================
    function recalcularDesdePorcentaje(pct) {
        const p = clamp(pct, 0.01, 100);
        const factor = p / 100;

        let total = 0;
        let neto = 0;
        let iva = 0;

        const grupos = {}; // key: alicuota (string) => {neto, iva, total}

        $('#feTablaItems tbody tr.fe-item').each(function () {
            const $tr = $(this);

            const qty = toNum($tr.data('qty'));
            const precioOrig = toNum($tr.data('precio-orig'));
            const ali = toNum($tr.data('iva'));

            const precio = precioOrig * factor;
            const subtotal = qty * precio;

            $tr.find('.fe-precio').text(precio.toFixed(2));
            $tr.find('.fe-subtotal').text(subtotal.toFixed(2));
            $tr.find('.fe-qty').text(nfQty.format(qty));
            $tr.find('.fe-iva').text((ali || 0).toFixed(ali % 1 === 0 ? 0 : 1));

            total += subtotal;

            const divisor = 1 + (ali / 100);
            const net = divisor > 0 ? (subtotal / divisor) : subtotal;
            const iv = subtotal - net;

            neto += net;
            iva += iv;

            const key = (ali || 0).toFixed(2);
            if (!grupos[key]) grupos[key] = { ali: ali || 0, neto: 0, iva: 0, total: 0 };
            grupos[key].neto += net;
            grupos[key].iva += iv;
            grupos[key].total += subtotal;
        });

        // Hidden (POST) con coma
        $('#feInputTotal').val(toServerDec(total, 2));
        $('#feInputNeto').val(toServerDec(neto, 2));
        $('#feInputIva').val(toServerDec(iva, 2));

        // UI visible
        $('#feInputTotalUI').val(toServerDec(total, 2));
        $('#feInputNetoUI').val(toServerDec(neto, 2));
        $('#feInputIvaUI').val(toServerDec(iva, 2));

        // Tabla IVA
        const $tb = $('#feTablaIva tbody');
        $tb.empty();

        const keys = Object.keys(grupos).sort((a, b) => parseFloat(a) - parseFloat(b));
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

        return { total, neto, iva, porcentaje: p };
    }

    function aplicarPorcentaje(p) {
        if (updating) return;
        updating = true;

        const r = recalcularDesdePorcentaje(p);
        setPorcentaje(r.porcentaje);
        setTotalFacturar(r.total);

        updating = false;
    }

    function aplicarTotalObjetivo(totalObj) {
        if (updating) return;
        updating = true;

        const orig = totalOriginal();
        const t = Math.max(0, totalObj || 0);

        let pct = 100;
        if (orig > 0) pct = (t / orig) * 100;

        pct = clamp(pct, 0.01, 100);

        const r = recalcularDesdePorcentaje(pct);
        setPorcentaje(r.porcentaje);
        setTotalFacturar(r.total);

        updating = false;
    }

    // Ajusta alturas de body y tabla para que footer quede visible y body scrollee
    function ajustarAlturasModal() {
        try {
            const $modal = $('#modalFacturaElectronica');
            if (!$modal.length) return;
            const vh = Math.max(window.innerHeight || document.documentElement.clientHeight, 600);
            const maxModal = Math.round(vh * 0.92);
            const headerH = $modal.find('.modal-header').outerHeight(true) || 0;
            const footerH = $modal.find('.modal-footer').outerHeight(true) || 0;
            const contentPad = 24; // margen / paddings
            const bodyMax = Math.max(120, maxModal - (headerH + footerH + contentPad));
            $modal.find('.modal-body.fe-body').css('max-height', bodyMax + 'px');
            // dejar parte para tabla: restar espacio de summary y top
            const topH = $modal.find('.fe-top').outerHeight(true) || 0;
            const summaryH = $modal.find('.fe-summary').outerHeight(true) || 0;
            const availForTable = Math.max(120, bodyMax - (topH + summaryH + 24));
            $modal.find('.fe-table-scroll').css('max-height', availForTable + 'px');
        } catch (e) {
            console.warn('ajuste alturas modal', e);
        }
    }

    // =========================
    // Init cuando se muestra modal
    // =========================
    function initFacturaModal() {
        const $f = $root();
        if (!$f.length) return;

        // Asegurar estado del bloque de ajuste según switch
        $('#feAjusteBlock').toggleClass('d-none', !$('#feSwitchAjuste').is(':checked'));

        // Si ya emitida: solo recalculamos para mostrar bien (sin edición)
        if (isYaEmitida()) {
            aplicarPorcentaje(100);
            setTimeout(() => {
                const el = document.getElementById('feLblTotal');
                if (el && el.scrollIntoView) el.scrollIntoView({ block: 'center', behavior: 'auto' });
                $('#btnRegistrarFactura').trigger('focus');
            }, 120);
            return;
        }

        // Defaults
        $('#feInputPorcentaje').val('100');
        $('#feInputTotalFacturar').val(totalOriginal().toFixed(2));
        $('#feModoPorcentaje').prop('checked', true);
        habilitarInputsEdicion($('#feSwitchAjuste').is(':checked'));

        // Inicializa totales en pantalla
        aplicarPorcentaje(100);

        // Ajustar alturas y hacer visible total
        setTimeout(() => {
            ajustarAlturasModal();
            const el = document.getElementById('feLblTotal');
            if (el && el.scrollIntoView) el.scrollIntoView({ block: 'center', behavior: 'auto' });
            $('#btnRegistrarFactura').trigger('focus');
        }, 150);
    }

    // =========================
    // Eventos UI
    // =========================
    $(document).on('shown.bs.modal', '#modalFacturaElectronica', function () {
        initFacturaModal();
        ajustarAlturasModal();
        $(window).on('resize.factura', ajustarAlturasModal);
    });

    $(document).on('hidden.bs.modal', '#modalFacturaElectronica', function () {
        $(window).off('resize.factura');
    });

    // Toggle: mostrar/ocultar bloque AJUSTE
    $(document).on('change', '#feSwitchAjuste', function () {
        if (isYaEmitida()) return;
        const on = $(this).is(':checked');
        habilitarInputsEdicion(on);

        if (!on) {
            $('#feModoPorcentaje').prop('checked', true);
            habilitarInputsEdicion(false);
            aplicarPorcentaje(100);
            showPctWarning(false);
        } else {
            // desplazar vista para que el bloque quede visible
            setTimeout(() => {
                const el = document.getElementById('feAjusteBlock');
                if (el && el.scrollIntoView) el.scrollIntoView({ block: 'center', behavior: 'smooth' });
                ajustarAlturasModal();
            }, 80);
        }
    });

    $(document).on('change', '#feModoPorcentaje, #feModoTotal', function () {
        if (isYaEmitida()) return;
        habilitarInputsEdicion(modoEdicion());
        ajustarAlturasModal();
    });

    // Input porcentaje (texto): parse flexible (coma o punto), no formatear mientras el usuario escribe
    $(document).on('input', '#feInputPorcentajeUI', function () {
        if (isYaEmitida()) return;
        if (!modoEdicion() || modo() !== 'porcentaje') return;

        const raw = $(this).val();
        const p = toNum(raw);

        // mostrar warning si mayor a 100
        if (p > 100) {
            showPctWarning(true);
            $(this).addClass('is-invalid');
        } else {
            showPctWarning(false);
            $(this).removeClass('is-invalid');
        }

        aplicarPorcentaje(Math.min(p, 100));
    });

    // Al salir del porcentaje, si >100 volver a 100
    $(document).on('blur', '#feInputPorcentajeUI', function () {
        const p = toNum($(this).val());
        const fixed = Math.min(Math.max(p || 0, 0.01), 100);
        $(this).val(toServerDec(fixed, 2));
        $('#fePorcentajeFacturacion').val(toServerDec(fixed, 4));
        ajustarAlturasModal();
    });

    // Input total objetivo (texto)
    $(document).on('input', '#feInputTotalFacturar', function () {
        if (isYaEmitida()) return;
        if (!modoEdicion() || modo() !== 'total') return;

        const t = toNum($(this).val());
        aplicarTotalObjetivo(t);
    });

    // Normalizar formato en blur (no durante la edición)
    $(document).on('blur', '#feInputTotalFacturar', function () {
        const t = toNum($(this).val());
        $(this).val((t || 0).toFixed(2));
        ajustarAlturasModal();
    });

    // =========================
    // Submit
    // =========================
    $(document).on('submit', '#formFacturaElectronica', function (e) {
        e.preventDefault();

        const $form = $(this);
        const $btn = $form.find('#btnRegistrarFactura').prop('disabled', true);

        const datos = $form.serialize();

        $.post('/Ventas/GenerarFactura', datos)
            .done(function (resp) {
                if (resp && resp.ok) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Factura registrada',
                        text: 'Nro: ' + (resp.nro || '')
                    });

                    $('#modalFacturaElectronica').modal('hide');
                    $(document).trigger('venta:facturada', [resp]);
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: (resp && resp.msg) ? resp.msg : 'Error desconocido'
                    });
                }
            })
            .fail(function () {
                Swal.fire({ icon: 'error', title: 'Error', text: 'Error en la petición' });
            })
            .always(function () {
                $btn.prop('disabled', false);
            });
    });

})();