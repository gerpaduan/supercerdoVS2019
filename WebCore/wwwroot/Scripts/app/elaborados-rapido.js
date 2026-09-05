(function () {
    function showAlert(icon, title, text) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            Swal.fire({ icon: icon, title: title, text: text });
        } else {
            alert(text);
        }
    }

    function permitirSalidaSinAdvertencia() {
        var guardApi = $('#formIngresoRapidoElaborado').data('editPageGuardApi');
        if (guardApi && typeof guardApi.allowNavigation === 'function') {
            guardApi.allowNavigation();
        }
        if (typeof window.desactivarProteccionSalida === 'function') {
            window.desactivarProteccionSalida();
        } else {
            window.__protegerSalida = false;
        }
    }

    function showSuccessAndRedirect(message, redirectUrl, esDesarme) {
        var destination = redirectUrl || '/Elaborados';
        permitirSalidaSinAdvertencia();

        if (window.Swal && typeof window.Swal.fire === 'function') {
            Swal.fire({
                icon: 'success',
                title: esDesarme ? 'Desarme guardado correctamente' : 'Ingreso rápido guardado correctamente',
                text: message || 'El movimiento se guardó correctamente.',
                timer: 2000,
                timerProgressBar: true,
                showConfirmButton: true,
                confirmButtonText: 'OK',
                allowOutsideClick: false,
                returnFocus: false
            }).then(function () {
                window.location.href = destination;
            });
            return;
        }

        window.setTimeout(function () {
            window.location.href = destination;
        }, 2000);
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

    function normalizarBalanzaPayload(data) {
        if (window.CarnisysBalanzaUtils && typeof window.CarnisysBalanzaUtils.normalize === 'function') {
            return window.CarnisysBalanzaUtils.normalize(data);
        }

        return {
            ok: !!(data && data.ok),
            conectada: !!(data && data.conectada),
            peso: 0,
            pesoTexto: '0.000',
            pesoDisplay: '0.000',
            raw: data || null
        };
    }

    function balanzaConectadaDesdePayload(data) {
        return normalizarBalanzaPayload(data).conectada === true;
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
        $rows.on('dblclick', function (e) {
            var $target = $(e.target);
            if ($target.closest('a, button, input, textarea, select, label').length) return;

            var $seleccionar = $(this).find('a.btn').first();
            if ($seleccionar.length) {
                window.location.href = $seleccionar.attr('href');
            }
        });
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
            balanzaDisponible: false,
            balanzaClientStarted: false,
            balanzaUltimaLectura: null,
            balanzaDesactivadaManual: false
        };

        var $form = $('#formIngresoRapidoElaborado');
        var $cantidad = $('#CantidadRapida');
        var $tabla = $('#tablaFormulaRapida tbody');
        var $total = $('#lblTotalFormulaRapida');
        var $balanza = $('#chkBalanzaRapido');
        var $alertaBalanza = $('#alertaBalanzaRapido');
        var $receta = $('#RecetaRapido');

        function esAccionVisible($button) {
            return !!($button && $button.length && !$button.prop('disabled') && !$button.hasClass('d-none'));
        }

        function getPrimaryActionButton() {
            var $modify = $('#btnHabilitarEdicionIngresoRapido');
            var $save = $('#btnGuardarIngresoRapido');

            if (esAccionVisible($modify)) return $modify;
            if (esAccionVisible($save)) return $save;
            return $();
        }

        function syncPrimaryAction() {
            var esEdicion = (parseInt($form.find('input[name="IdEmbutido"]').val(), 10) || 0) > 0;
            if (!esEdicion) return;

            var $modify = $('#btnHabilitarEdicionIngresoRapido');
            var $save = $('#btnGuardarIngresoRapido');
            if (!$modify.length || !$save.length) return;

            var readOnly = $form.hasClass('edit-readonly-active');

            if (readOnly) {
                $modify
                    .html('<i class="fas fa-edit mr-1"></i> Modificar')
                    .toggleClass('d-none', false)
                    .attr('title', 'Modificar (Alt+Enter)');

                $save
                    .toggleClass('d-none', true)
                    .attr('title', 'Guardar elaborado (Alt+Enter)');
                return;
            }

            $modify.toggleClass('d-none', true);
            $save
                .toggleClass('d-none', false)
                .attr('title', 'Guardar elaborado (Alt+Enter)');
        }

        function esPesable() {
            return config.esPesable === true || config.esPesable === 'true';
        }

        function syncCantidadReadonly() {
            $cantidad.prop('readonly', !!(esPesable() && $balanza.is(':checked') && state.balanzaDisponible && !state.balanzaDesactivadaManual) || !!config.esAnulado);
        }

        function renderBalanzaStatus(payload) {
            if (window.CarnisysBalanzaUtils && typeof window.CarnisysBalanzaUtils.renderStatus === 'function') {
                window.CarnisysBalanzaUtils.renderStatus('#estadoBalanzaRapido', '#barraBalanzaRapido', payload);
            }
        }

        function syncBalanzaStatusVisibility() {
            $('#balanzaStatusWrapRapido').toggle(!!$balanza.is(':checked'));
        }

        function aplicarLecturaBalanza(payload) {
            var normalized = normalizarBalanzaPayload(payload);
            state.balanzaDisponible = normalized.conectada === true;
            state.balanzaUltimaLectura = normalized;
            renderBalanzaStatus(payload);
            syncCantidadReadonly();

            if (!esPesable() || !normalized.conectada || !$balanza.is(':checked') || state.balanzaDesactivadaManual) {
                return;
            }

            $alertaBalanza.addClass('d-none').text('');
            $cantidad.val(normalized.pesoDisplay || normalized.pesoTexto || formatKg(normalized.peso));
            recalcularFormula();
        }

        function aplicarStatusBalanza(payload) {
            var normalized = normalizarBalanzaPayload(payload);
            state.balanzaDisponible = normalized.conectada === true;
            renderBalanzaStatus(payload);
            syncCantidadReadonly();
        }

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
            if (!window.CarnisysBalanza) {
                state.balanzaDisponible = false;
                renderBalanzaStatus(null);
                syncCantidadReadonly();
                if (typeof callback === 'function') callback(false);
                return;
            }

            if (!state.balanzaClientStarted) {
                state.balanzaClientStarted = true;
                window.CarnisysBalanza.start({
                    baseUrl: 'http://127.0.0.1:5100',
                    statusIntervalMs: 3000,
                    pesoIntervalMs: 250,
                    onStatus: function (data) {
                        aplicarStatusBalanza(data);
                    },
                    onPeso: function (data) {
                        aplicarLecturaBalanza(data);
                    },
                    onError: function () {
                        state.balanzaDisponible = false;
                        renderBalanzaStatus(null);
                        syncCantidadReadonly();
                    }
                });
            }

            window.CarnisysBalanza.leerAhora().then(function (data) {
                state.balanzaDisponible = balanzaConectadaDesdePayload(data);
                aplicarLecturaBalanza(data);
                if (typeof callback === 'function') callback(state.balanzaDisponible, data);
            }).catch(function () {
                state.balanzaDisponible = false;
                renderBalanzaStatus(null);
                syncCantidadReadonly();
                if (typeof callback === 'function') callback(false);
            });
        }

        function verificarBalanzaInicial() {
            if (!esPesable()) {
                state.balanzaDesactivadaManual = true;
                $balanza.prop('checked', false);
                $cantidad.val('');
                syncCantidadReadonly();
                window.setTimeout(function () {
                    $cantidad.focus().select();
                }, 30);
                return;
            }

            verificarBalanza(function (disponible) {
                if (!disponible) {
                    $balanza.prop('checked', false);
                    $alertaBalanza.removeClass('d-none').text('No se detectó balanza activa. La carga seguirá manual.');
                } else {
                    $alertaBalanza.addClass('d-none').text('');
                }
                syncCantidadReadonly();
            });
        }

        function intentarLeerBalanza() {
            if (!esPesable()) {
                $balanza.prop('checked', false);
                state.balanzaDesactivadaManual = true;
                $cantidad.val('');
                syncCantidadReadonly();
                window.setTimeout(function () {
                    $cantidad.focus().select();
                }, 30);
                return;
            }

            verificarBalanza(function (disponible, data) {
                if (!disponible) {
                    $balanza.prop('checked', false);
                    $alertaBalanza.removeClass('d-none').text('No hay balanza conectada.');
                    return;
                }

                state.balanzaDesactivadaManual = false;
                if (window.CarnisysBalanza) {
                    window.CarnisysBalanza.activar();
                }
                aplicarLecturaBalanza(data);
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
                state.balanzaDesactivadaManual = false;
                intentarLeerBalanza();
            } else {
                state.balanzaDesactivadaManual = true;
                if (window.CarnisysBalanza) {
                    window.CarnisysBalanza.desactivar();
                }
                $alertaBalanza.addClass('d-none').text('');
                if (!esPesable()) {
                    $cantidad.val('');
                    window.setTimeout(function () {
                        $cantidad.focus().select();
                    }, 30);
                }
            }
            syncBalanzaStatusVisibility();
            syncCantidadReadonly();
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
            var key = String(e.key || '').toLowerCase();

            if (e.altKey && !e.ctrlKey && !e.metaKey && !e.shiftKey && !e.repeat && key === 'enter') {
                // Con un modal abierto, el backdrop bloquea el mouse pero no el teclado -- sin
                // este chequeo el atajo dispara el boton primario de esta pantalla detras del modal.
                if ($(".modal.show").length) return;
                var $primaryAction = getPrimaryActionButton();
                if ($primaryAction.length && !$primaryAction.prop('disabled')) {
                    e.preventDefault();
                    $primaryAction.trigger('click');
                    return;
                }
            }

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

            function guardarIngresoRapidoReal() {
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

                    showSuccessAndRedirect(resp.mensaje, resp.redirectUrl || config.redirectUrl || '/Elaborados', config.esDesarme);
                }).fail(function (xhr) {
                    state.guardando = false;
                    var mensaje = 'No se pudo guardar el movimiento.';
                    if (xhr && xhr.responseJSON && xhr.responseJSON.mensaje) mensaje = xhr.responseJSON.mensaje;
                    showAlert('error', config.esDesarme ? 'Desarme' : 'Ingreso rápido', mensaje);
                });
            }

            // Usuario de produccion (sala de piso): antes de guardar, se elige de un modal
            // quien es el empleado real -- ver Web/Scripts/app/seleccion-usuario-produccion.js.
            if (window.SeleccionUsuarioProduccion) {
                window.SeleccionUsuarioProduccion.conSeleccionDeUsuario($form, guardarIngresoRapidoReal, {
                    titulo: '¿Quién está haciendo esta carga?'
                });
            } else {
                guardarIngresoRapidoReal();
            }
        });

        autoResizeTextarea($receta);
        renderFormula();
        recalcularFormula();
        renderBalanzaStatus(null);
        verificarBalanzaInicial();
        syncBalanzaStatusVisibility();
        syncCantidadReadonly();

        window.ElaboradosRapido = window.ElaboradosRapido || {};
        window.ElaboradosRapido.syncPrimaryAction = syncPrimaryAction;
        syncPrimaryAction();
    }

    $(function () {
        initLista();
        initEdicion();
    });
})();
