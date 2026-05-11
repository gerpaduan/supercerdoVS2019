(function () {
    function showAlert(icon, title, text) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            Swal.fire({ icon: icon, title: title, text: text });
        } else {
            alert(text);
        }
    }

    function parseDecimal(value) {
        if (value === null || value === undefined) return { ok: false, value: 0 };
        var text = String(value).trim();
        if (!text) return { ok: false, value: 0 };

        text = text.replace(/\s/g, '');
        var lastComma = text.lastIndexOf(',');
        var lastDot = text.lastIndexOf('.');
        var decimalSep = '';

        if (lastComma >= 0 && lastDot >= 0) decimalSep = lastComma > lastDot ? ',' : '.';
        else if (lastComma >= 0) decimalSep = ',';
        else if (lastDot >= 0) decimalSep = '.';

        var decimalIndex = decimalSep ? text.lastIndexOf(decimalSep) : -1;
        var normalized = '';

        for (var i = 0; i < text.length; i++) {
            var ch = text.charAt(i);
            if (ch >= '0' && ch <= '9') normalized += ch;
            else if ((ch === ',' || ch === '.') && i === decimalIndex) normalized += '.';
            else if (ch === '-' && normalized.length === 0) normalized += ch;
        }

        if (!normalized || normalized === '-' || normalized === '.' || normalized === '-.') {
            return { ok: false, value: 0 };
        }

        var n = parseFloat(normalized);
        return { ok: !isNaN(n), value: isNaN(n) ? 0 : n };
    }

    function toFloat(value) {
        return parseDecimal(value).value;
    }

    function formatKg(value) {
        return toFloat(value).toLocaleString('es-AR', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
    }

    function autoResizeTextarea($textarea) {
        if (!$textarea || !$textarea.length) return;
        $textarea.css('height', 'auto');
        $textarea.css('height', $textarea.get(0).scrollHeight + 'px');
    }

    function balanzaConectadaDesdePayload(data) {
        if (!data) return false;
        var estado = String(data.estado || data.status || '').toLowerCase();
        var pesoRaw = data.peso;
        if (pesoRaw === undefined || pesoRaw === null) pesoRaw = data.valor;
        if (pesoRaw === undefined || pesoRaw === null) pesoRaw = data.weight;
        var peso = Number(pesoRaw);
        return estado === 'ok' || estado === 'connected' || (!isNaN(peso) && peso !== 0);
    }

    function initLista() {
        var $page = $('[data-elaborados-page="ingreso-rapido-lista"]');
        if (!$page.length) return;

        var $rows = $('#tablaIngresoRapidoLista tbody .js-rapido-row');
        var $filtro = $('#filtroRapidoTexto');
        var $sinResultados = $('#sinResultadosRapidos');

        function aplicarFiltro() {
            var texto = ($filtro.val() || '').toLowerCase().trim();
            var visibles = 0;

            $rows.each(function () {
                var $row = $(this);
                var codigo = String($row.data('codigo') || '').toLowerCase();
                var producto = String($row.data('producto') || '').toLowerCase();
                var visible = !texto || codigo.indexOf(texto) >= 0 || producto.indexOf(texto) >= 0;

                $row.toggleClass('d-none', !visible);
                if (visible) visibles++;
            });

            $sinResultados.toggleClass('d-none', visibles > 0);
        }

        $filtro.on('input', aplicarFiltro);
        aplicarFiltro();
        window.setTimeout(function () { $filtro.trigger('focus'); }, 40);

        var successAlert = document.getElementById('elaboradosSuccessAlert');
        if (successAlert) {
            window.setTimeout(function () {
                $(successAlert).alert('close');
            }, 3200);
        }
    }

    function initEdicion() {
        var $page = $('[data-elaborados-page="ingreso-rapido-edicion"]');
        if (!$page.length) return;

        var config = window.elaboradosIngresoRapidoConfig || {};
        var state = {
            formula: $.isArray(config.initialFormula) ? config.initialFormula.slice() : [],
            guardando: false,
            balanzaDisponible: false
        };

        var $form = $('#formIngresoRapidoElaborado');
        var $cantidad = $('#CantidadRapida');
        var $tabla = $('#tablaFormulaRapida tbody');
        var $total = $('#lblTotalFormulaRapida');
        var $balanza = $('#chkBalanzaRapido');
        var $alertaBalanza = $('#alertaBalanzaRapido');
        var $receta = $('#RecetaRapido');

        function renderFormula() {
            var html = '';
            var total = 0;

            if (!state.formula.length) {
                html = '<tr><td colspan="3" class="text-center text-muted">El elaborado no tiene fórmula cargada.</td></tr>';
            } else {
                state.formula.forEach(function (item, index) {
                    total += toFloat(item.Kgs);
                    html += '<tr>'
                        + '<td>' + item.Producto + '<input type="hidden" name="Formula[' + index + '].IdCorte" value="' + (item.IdCorte || 0) + '" /><input type="hidden" name="Formula[' + index + '].Codigo" value="' + (item.Codigo || 0) + '" /><input type="hidden" name="Formula[' + index + '].Producto" value="' + $('<div>').text(item.Producto || '').html() + '" /><input type="hidden" name="Formula[' + index + '].Porcentaje" value="' + toFloat(item.Porcentaje).toString().replace('.', ',') + '" /><input type="hidden" name="Formula[' + index + '].AgregarAuto" value="' + (item.AgregarAuto ? 'true' : 'false') + '" /><input type="hidden" name="Formula[' + index + '].Kgs" value="' + toFloat(item.Kgs).toString().replace('.', ',') + '" /></td>'
                        + '<td class="text-right">' + formatKg(item.Porcentaje) + '</td>'
                        + '<td class="text-right">' + formatKg(item.Kgs) + '</td>'
                        + '</tr>';
                });
            }

            $tabla.html(html);
            $total.text(formatKg(total));
        }

        function recalcularFormula() {
            var parsed = parseDecimal($cantidad.val());
            var cantidad = parsed.ok ? Math.abs(parsed.value) : 0;
            if (config.esDesarme) cantidad *= -1;

            state.formula = state.formula.map(function (item) {
                var copy = $.extend({}, item);
                copy.Kgs = Math.round((0.01 * cantidad * toFloat(copy.Porcentaje)) * 1000) / 1000;
                return copy;
            });

            renderFormula();
        }

        function verificarBalanza(callback) {
            if (!config.balanzaUrl) {
                state.balanzaDisponible = false;
                if (typeof callback === 'function') callback(false);
                return;
            }

            $.ajax({
                url: config.balanzaUrl,
                method: 'GET',
                cache: false,
                timeout: 1200
            }).done(function (data) {
                state.balanzaDisponible = balanzaConectadaDesdePayload(data);
                if (typeof callback === 'function') callback(state.balanzaDisponible, data);
            }).fail(function () {
                state.balanzaDisponible = false;
                if (typeof callback === 'function') callback(false);
            });
        }

        function verificarBalanzaInicial() {
            verificarBalanza(function (disponible) {
                if (!disponible) {
                    $balanza.prop('checked', false);
                    $alertaBalanza.removeClass('d-none').text('No se detectó balanza activa. La carga seguirá manual.');
                } else {
                    $alertaBalanza.addClass('d-none').text('');
                }
            });
        }

        function intentarLeerBalanza() {
            verificarBalanza(function (disponible, data) {
                if (!disponible) {
                    $balanza.prop('checked', false);
                    $alertaBalanza.removeClass('d-none').text('No hay balanza conectada.');
                    return;
                }

                $alertaBalanza.addClass('d-none').text('');
                var pesoRaw = data && (data.peso !== undefined ? data.peso : (data.valor !== undefined ? data.valor : data.weight));
                if (pesoRaw !== undefined && pesoRaw !== null) {
                    $cantidad.val(formatKg(pesoRaw));
                    recalcularFormula();
                }
            });
        }

        function validar() {
            if (config.esAnulado) return 'El elaborado fue anulado y no puede ser modificado.';
            if ($('#IdSucursalRapido').val() === '0') return 'Debe seleccionar una sucursal.';
            var parsed = parseDecimal($cantidad.val());
            if (!parsed.ok || parsed.value <= 0) return 'Debe ingresar una cantidad mayor a cero.';
            if (!state.formula.length) return 'El elaborado no tiene fórmula cargada.';
            return '';
        }

        $cantidad.on('input', recalcularFormula);
        $cantidad.on('keydown', function (e) {
            if (config.esAnulado) return;
            if (e.key === 'Enter') {
                e.preventDefault();
                $form.trigger('submit');
            }
        });

        $balanza.on('change', function () {
            if (config.esAnulado) return;
            if ($balanza.is(':checked')) {
                intentarLeerBalanza();
            } else {
                $alertaBalanza.addClass('d-none').text('');
            }
        });

        $('#btnAnularIngresoRapido').on('click', function () {
            if (!config.puedeAnular) return;

            var ejecutar = function () {
                $.ajax({
                    url: config.anularUrl,
                    type: 'POST',
                    data: {
                        __RequestVerificationToken: $form.find('input[name="__RequestVerificationToken"]').val(),
                        idEmbutido: parseInt($form.find('input[name="IdEmbutido"]').val(), 10) || 0
                    }
                }).done(function (resp) {
                    if (!resp || !resp.ok) {
                        showAlert('error', config.esDesarme ? 'Desarme' : 'Ingreso rápido', (resp && resp.mensaje) || 'No se pudo anular el elaborado.');
                        return;
                    }

                    window.location.href = resp.redirectUrl || config.redirectUrl || '/Elaborados';
                }).fail(function (xhr) {
                    var mensaje = 'No se pudo anular el elaborado.';
                    if (xhr && xhr.responseJSON && xhr.responseJSON.mensaje) mensaje = xhr.responseJSON.mensaje;
                    showAlert('error', config.esDesarme ? 'Desarme' : 'Ingreso rápido', mensaje);
                });
            };

            if (window.Swal && typeof window.Swal.fire === 'function') {
                Swal.fire({
                    icon: 'warning',
                    title: 'Anular elaborado',
                    text: '¿Está seguro que desea anular el elaborado?',
                    showCancelButton: true,
                    confirmButtonText: 'Sí, anular',
                    cancelButtonText: 'Cancelar'
                }).then(function (result) {
                    if (result.isConfirmed) ejecutar();
                });
            } else if (window.confirm('¿Está seguro que desea anular el elaborado?')) {
                ejecutar();
            }
        });

        $(document).on('keydown.elaboradosRapidos', function (e) {
            if (config.esAnulado) return;
            if (e.key === '*') {
                e.preventDefault();
                $balanza.prop('checked', !$balanza.is(':checked')).trigger('change');
            }
        });

        $form.on('submit', function (e) {
            e.preventDefault();
            if (state.guardando) return;

            var error = validar();
            if (error) {
                showAlert('warning', config.esDesarme ? 'Desarme' : 'Ingreso rápido', error);
                return;
            }

            state.guardando = true;
            $.ajax({
                url: $form.attr('action'),
                type: 'POST',
                data: $form.serialize()
            }).done(function (resp) {
                state.guardando = false;
                if (!resp || !resp.ok) {
                    showAlert('error', config.esDesarme ? 'Desarme' : 'Ingreso rápido', (resp && resp.mensaje) || 'No se pudo guardar el movimiento.');
                    return;
                }

                window.location.href = resp.redirectUrl || config.redirectUrl || '/Elaborados';
            }).fail(function (xhr) {
                state.guardando = false;
                var mensaje = 'No se pudo guardar el movimiento.';
                if (xhr && xhr.responseJSON && xhr.responseJSON.mensaje) mensaje = xhr.responseJSON.mensaje;
                showAlert('error', config.esDesarme ? 'Desarme' : 'Ingreso rápido', mensaje);
            });
        });

        autoResizeTextarea($receta);
        renderFormula();
        recalcularFormula();
        verificarBalanzaInicial();
    }

    $(function () {
        initLista();
        initEdicion();
    });
})();
