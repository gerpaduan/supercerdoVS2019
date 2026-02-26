(function () {

    // =========================
    // Helpers formato
    // =========================
    const nfMoney = new Intl.NumberFormat('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    const nfQty = new Intl.NumberFormat('es-AR', { minimumFractionDigits: 3, maximumFractionDigits: 3 });

    function toNum(v) {
        if (v == null) return 0;
        // admite "1.234,56" o "1234.56"
        const s = String(v).trim()
            .replace(/\./g, '')     // miles
            .replace(',', '.');     // decimal
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
        return toNum($('#feInputPorcentaje').val()) || 0;
    }

    function setPorcentaje(p) {
        $('#feInputPorcentaje').val((p || 0).toFixed(2));
        $('#fePorcentajeFacturacion').val((p || 0).toFixed(4)); // lo que guardás
    }

    function setTotalFacturar(t) {
        // visual: mantenemos string simple "1234.56"
        $('#feInputTotalFacturar').val((t || 0).toFixed(2));
    }

    function modoEdicion() {
        return $('#feSwitchAjuste').is(':checked');
    }

    function modo() {
        return $('#feModoTotal').is(':checked') ? 'total' : 'porcentaje';
    }

    function habilitarInputsEdicion(on) {
        const editable = !!on;

        // Si no está en modo edición, quedan readonly
        $('#feInputPorcentaje').prop('readonly', !editable || modo() !== 'porcentaje');
        $('#feInputTotalFacturar').prop('readonly', !editable || modo() !== 'total');

        // Radio siempre activos mientras switch on
        $('#feModoPorcentaje, #feModoTotal').prop('disabled', !editable);
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

        // KPI
        $('#feLblTotal').text(money(total));
        $('#feLblNeto').text(money(neto));
        $('#feLblIva').text(money(iva));

        // Inputs (server)
        $('#feInputTotal').val(total.toFixed(2));
        $('#feInputNeto').val(neto.toFixed(2));
        $('#feInputIva').val(iva.toFixed(2));

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

    // =========================
    // Init cuando se muestra modal
    // =========================
    function initFacturaModal() {
        const $f = $root();
        if (!$f.length) return;

        // Si ya emitida: solo recalculamos para mostrar bien (sin edición)
        if (isYaEmitida()) {
            aplicarPorcentaje(100);
            // foco igual al botón (para Enter = guardar)
            setTimeout(() => $('#btnRegistrarFactura').trigger('focus'), 120);
            return;
        }

        // Defaults
        setPorcentaje(100);
        $('#feSwitchAjuste').prop('checked', false);
        $('#feModoPorcentaje').prop('checked', true);
        habilitarInputsEdicion(false);

        // Inicializa totales en pantalla
        aplicarPorcentaje(100);

        // Focus al registrar
        setTimeout(() => $('#btnRegistrarFactura').trigger('focus'), 120);
    }

    // =========================
    // Eventos UI
    // =========================
    $(document).on('shown.bs.modal', '#modalFacturaElectronica', function () {
        initFacturaModal();
    });

    $(document).on('change', '#feSwitchAjuste', function () {
        if (isYaEmitida()) return;

        const on = $(this).is(':checked');
        habilitarInputsEdicion(on);

        // Si apaga: vuelve a 100%
        if (!on) {
            $('#feModoPorcentaje').prop('checked', true);
            habilitarInputsEdicion(false);
            aplicarPorcentaje(100);
        }
    });

    $(document).on('change', '#feModoPorcentaje, #feModoTotal', function () {
        if (isYaEmitida()) return;
        habilitarInputsEdicion(modoEdicion());
    });

    // Input porcentaje
    $(document).on('input', '#feInputPorcentaje', function () {
        if (isYaEmitida()) return;
        if (!modoEdicion() || modo() !== 'porcentaje') return;

        const p = toNum($(this).val());
        aplicarPorcentaje(p);
    });

    // Input total objetivo
    $(document).on('input', '#feInputTotalFacturar', function () {
        if (isYaEmitida()) return;
        if (!modoEdicion() || modo() !== 'total') return;

        const t = toNum($(this).val());
        aplicarTotalObjetivo(t);
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