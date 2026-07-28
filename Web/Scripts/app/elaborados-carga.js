(function () {
    function showAlert(icon, title, text) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            Swal.fire({ icon: icon, title: title, text: text });
        } else {
            alert(text);
        }
    }

    function showSuccessAndRedirect(message, redirectUrl) {
        var destination = redirectUrl || '/Elaborados';

        if (window.Swal && typeof window.Swal.fire === 'function') {
            Swal.fire({
                icon: 'success',
                title: 'Elaborado guardado correctamente',
                text: message || 'El elaborado se guardó correctamente.',
                timer: 2000,
                timerProgressBar: true,
                showConfirmButton: false,
                allowOutsideClick: false,
                allowEscapeKey: false,
                allowEnterKey: false,
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

        var normalized = '';
        var decimalIndex = decimalSep ? text.lastIndexOf(decimalSep) : -1;

        for (var i = 0; i < text.length; i++) {
            var ch = text.charAt(i);
            if (ch >= '0' && ch <= '9') normalized += ch;
            else if (ch === '-' && normalized.length === 0) normalized += ch;
            else if ((ch === ',' || ch === '.') && i === decimalIndex) normalized += '.';
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

    function toInt(value) {
        var n = parseInt(value, 10);
        return isNaN(n) ? 0 : n;
    }

    function formatKg(value) {
        return toFloat(value).toLocaleString('es-AR', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
    }

    function formatDecimalForPost(value) {
        var parsed = parseDecimal(value);
        if (!parsed.ok) return '';
        return String(parsed.value).replace('.', ',');
    }

    function autoResizeTextarea($textarea) {
        if (!$textarea || !$textarea.length) return;
        $textarea.css('height', 'auto');
        $textarea.css('height', $textarea.get(0).scrollHeight + 'px');
    }

    function initCarga() {
        var $page = $('[data-elaborados-page="carga"]');
        if (!$page.length) return;

        var config = window.elaboradosCargaConfig || {};

        var state = {
            lineas: $.isArray(config.initialLines) ? config.initialLines.slice() : [],
            formula: $.isArray(config.initialFormula) ? config.initialFormula.slice() : [],
            balanzaDisponible: false,
            balanzaDesactivadaManual: false,
            balanzaClientStarted: false,
            balanzaUltimaLectura: null,
            ingredienteTimer: null,
            elaboradoTimer: null,
            ingredienteRequestSeq: 0,
            elaboradoRequestSeq: 0,
            draftTimer: null,
            guardando: false
        };

        var $form = $('#formCargaElaborado');
        var $receta = $('#Receta');
        var $elaboradoId = $('#IdElaborado');
        var $elaboradoCodigo = $('#txtCodigoElaborado');
        var $elaboradoNombre = $('#txtElaboradoNombre');
        var $elaboradoTipo = $('#txtElaboradoTipo');
        var $elaboradoPromedio = $('#txtElaboradoPromedio');
        var $elaboradoWarning = $('#elaboradoWarning');
        var $ingredienteId = $('#txtIngredienteId');
        var $ingredienteCodigo = $('#txtCodigoIngrediente');
        var $ingredienteNombre = $('#txtIngredienteNombre');
        var $ingredientePesable = $('#txtIngredientePesable');
        var $ingredienteTipo = $('#txtIngredienteTipo');
        var $ingredientePromedio = $('#txtIngredientePromedio');
        var $ingredienteKg = $('#txtKgIngrediente');
        var $balanza = $('#chkBalanzaIngrediente');
        var $feedback = $('#cargaFeedback');
        var $warning = $('#cargaWarning');

        function esAccionVisible($button) {
            return !!($button && $button.length && !$button.prop('disabled') && !$button.hasClass('d-none'));
        }

        function getPrimaryActionButton() {
            var $modify = $('#btnHabilitarEdicionElaborado');
            var $save = $('#btnGuardarElaborado');

            if (esAccionVisible($modify)) return $modify;
            if (esAccionVisible($save)) return $save;
            return $();
        }

        function syncPrimaryAction() {
            if (!config.esEdicion) return;

            var $modify = $('#btnHabilitarEdicionElaborado');
            var $save = $('#btnGuardarElaborado');
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

        function ingredienteEsPesable() {
            return String($ingredientePesable.val() || '').toLowerCase() === 'true';
        }

        function syncIngredienteReadonly() {
            var hayIngrediente = toInt($ingredienteId.val()) > 0;
            var soloLectura = !!($balanza.is(':checked') && state.balanzaDisponible && !state.balanzaDesactivadaManual && (!hayIngrediente || ingredienteEsPesable()));
            $ingredienteKg.prop('readonly', soloLectura || !!config.esAnulado);
        }

        function getDraftKey() {
            return config.draftKey || '';
        }

        function readDraft() {
            var key = getDraftKey();
            if (!key || !window.localStorage) return null;
            try {
                var raw = window.localStorage.getItem(key);
                return raw ? JSON.parse(raw) : null;
            } catch (err) {
                return null;
            }
        }

        function hideDraftBanner() {
            $('#elaboradoCargaDraftBanner').addClass('d-none');
        }

        function showDraftBanner() {
            $('#elaboradoCargaDraftBanner').removeClass('d-none');
        }

        function clearDraft() {
            var key = getDraftKey();
            if (!key || !window.localStorage) return;
            window.localStorage.removeItem(key);
            hideDraftBanner();
        }

        function buildDraft() {
            return {
                idSucursal: $('#IdSucursal').val(),
                fechaEmbutido: $('#FechaEmbutido').val(),
                observaciones: $('#Observaciones').val(),
                elaborado: {
                    id: $elaboradoId.val(),
                    codigo: $elaboradoCodigo.val(),
                    nombre: $elaboradoNombre.val(),
                    tipo: $elaboradoTipo.val(),
                    promedio: $elaboradoPromedio.val(),
                    receta: $receta.val()
                },
                ingredienteActual: {
                    id: $ingredienteId.val(),
                    codigo: $ingredienteCodigo.val(),
                    nombre: $ingredienteNombre.val(),
                    tipo: $ingredienteTipo.val(),
                    promedio: $ingredientePromedio.val(),
                    cantKg: $ingredienteKg.val(),
                    pesoBalanza: $balanza.is(':checked')
                },
                lineas: state.lineas,
                formula: state.formula
            };
        }

        function saveDraft() {
            var key = getDraftKey();
            if (!key || !window.localStorage || state.guardando) return;
            try {
                window.localStorage.setItem(key, JSON.stringify(buildDraft()));
            } catch (err) {
            }
        }

        function scheduleDraft() {
            window.clearTimeout(state.draftTimer);
            state.draftTimer = window.setTimeout(function () {
                saveDraft();
            }, 250);
        }

        function applyDraft(draft) {
            if (!draft) return;

            $('#IdSucursal').val(draft.idSucursal || $('#IdSucursal').val());
            $('#FechaEmbutido').val(draft.fechaEmbutido || $('#FechaEmbutido').val());
            $('#Observaciones').val(draft.observaciones || '');

            if (draft.elaborado) {
                $elaboradoId.val(draft.elaborado.id || '');
                $elaboradoCodigo.val(draft.elaborado.codigo || '');
                $elaboradoNombre.val(draft.elaborado.nombre || '');
                $elaboradoTipo.val(draft.elaborado.tipo || '');
                $elaboradoPromedio.val(draft.elaborado.promedio || '');
                $receta.val(draft.elaborado.receta || '');
            }

            if (draft.ingredienteActual) {
                $ingredienteId.val(draft.ingredienteActual.id || '');
                $ingredienteCodigo.val(draft.ingredienteActual.codigo || '');
                $ingredienteNombre.val(draft.ingredienteActual.nombre || '');
                $ingredienteTipo.val(draft.ingredienteActual.tipo || '');
                $ingredientePromedio.val(draft.ingredienteActual.promedio || '');
                $ingredienteKg.val(draft.ingredienteActual.cantKg || '');
                $balanza.prop('checked', draft.ingredienteActual.pesoBalanza === true);
            }

            state.lineas = $.isArray(draft.lineas) ? draft.lineas : [];
            state.formula = $.isArray(draft.formula) ? draft.formula : [];
            renderLineas();
            renderFormula();
            autoResizeTextarea($receta);
            scheduleDraft();
        }

        function clearWarning() {
            $warning.addClass('d-none').text('');
        }

        function showFeedback(text) {
            $feedback.html(text).removeClass('d-none');
            setTimeout(function () { $feedback.addClass('d-none').text(''); }, 2200);
        }

        function focusIngredienteCodigo() {
            setTimeout(function () {
                $ingredienteCodigo.focus();
            }, 30);
        }

        function clearIngrediente() {
            $ingredienteId.val('');
            $ingredienteCodigo.val('');
            clearIngredienteSeleccion(false);
        }

        function clearIngredienteSeleccion(preserveCodigo) {
            var codigoActual = $ingredienteCodigo.val();
            $ingredienteNombre.val('');
            $ingredientePesable.val('');
            $ingredienteTipo.val('');
            $ingredientePromedio.val('');
            if (preserveCodigo) {
                $ingredienteCodigo.val(codigoActual);
            }
            if ($balanza.is(':checked') && state.balanzaDisponible && state.balanzaUltimaLectura && !state.balanzaDesactivadaManual) {
                $ingredienteKg.val(state.balanzaUltimaLectura.pesoDisplay || state.balanzaUltimaLectura.pesoTexto || '');
            } else {
                $ingredienteKg.val('');
            }
            syncIngredienteReadonly();
        }

        function setIngrediente(producto) {
            $ingredienteId.val(producto.id || '');
            $ingredienteCodigo.val(producto.codigo || '');
            $ingredienteNombre.val(producto.nombre || '');
            $ingredientePesable.val(producto.pesable === true ? 'true' : 'false');
            $ingredienteTipo.val(producto.tipo || '');
            $ingredientePromedio.val(producto.promedio || 0);
            actualizarBalanzaSegunTipo();
            if (!ingredienteEsPesable()) {
                $ingredienteKg.val('');
            } else if ($balanza.is(':checked') && state.balanzaDisponible && state.balanzaUltimaLectura && !state.balanzaDesactivadaManual) {
                $ingredienteKg.val(state.balanzaUltimaLectura.pesoDisplay || state.balanzaUltimaLectura.pesoTexto || '');
            }
            syncIngredienteReadonly();
        }

        function clearElaborado() {
            $elaboradoId.val('');
            if (!config.esEdicion) {
                $elaboradoCodigo.val('');
            }
            clearElaboradoSeleccion(config.esEdicion);
        }

        function clearElaboradoSeleccion(preserveCodigo) {
            var codigoActual = $elaboradoCodigo.val();
            $elaboradoNombre.val('');
            $elaboradoTipo.val('');
            $elaboradoPromedio.val('');
            $receta.val('');
            state.formula = [];
            renderFormula();
            $elaboradoWarning.addClass('d-none').text('');
            autoResizeTextarea($receta);
            if (preserveCodigo) {
                $elaboradoCodigo.val(codigoActual);
            }
        }

        function setElaborado(producto) {
            $elaboradoId.val(producto.id || '');
            $elaboradoCodigo.val(producto.codigo || '');
            $elaboradoNombre.val(producto.nombre || '');
            $elaboradoTipo.val(producto.tipo || '');
            $elaboradoPromedio.val(producto.promedio || 0);

            if (producto.ingresoRapido === true) {
                $elaboradoWarning.removeClass('d-none').text('Este producto sugiere ingreso rapido en WinForms. En Web igual podes cargarlo desde esta pantalla.');
            } else {
                $elaboradoWarning.addClass('d-none').text('');
            }

            obtenerFormula(producto.id);
        }

        function formatBool(v) {
            return v ? 'Si' : 'No';
        }

        function renderLineas() {
            var html = '';
            var hidden = '';
            var totalManual = 0;

            state.lineas.forEach(function (linea, index) {
                totalManual += toFloat(linea.CantKg);
                html += '<tr>'
                    + '<td>' + linea.Codigo + '</td>'
                    + '<td>' + linea.Producto + '</td>'
                    + '<td class="text-right">' + formatKg(linea.CantKg) + '</td>'
                    + '<td class="text-center">' + formatBool(linea.PesoBalanza) + '</td>'
                    + '<td><button type="button" class="btn btn-sm btn-outline-danger js-remove-linea" data-index="' + index + '"' + (config.esAnulado ? ' disabled' : '') + '><i class="fas fa-trash"></i></button></td>'
                    + '</tr>';

                hidden += '<input type="hidden" name="Lineas[' + index + '].IdCorte" value="' + (linea.IdCorte || 0) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].Codigo" value="' + (linea.Codigo || 0) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].Producto" value="' + $('<div>').text(linea.Producto || '').html() + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].TipoProducto" value="' + $('<div>').text(linea.TipoProducto || '').html() + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].CantKg" value="' + formatDecimalForPost(linea.CantKg) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].PesoBalanza" value="' + (linea.PesoBalanza ? 'true' : 'false') + '" />';
            });

            if (!html) {
                html = '<tr><td colspan="5" class="text-center text-muted">Todavia no agregaste ingredientes manuales.</td></tr>';
            }

            $('#tablaLineasElaborado tbody').html(html);
            $('#lineasHiddenContainer').html(hidden);
            $('#lblTotalManual').text(formatKg(totalManual));
            recalcularFormula();
        }

        function renderFormula() {
            var html = '';
            var totalAuto = 0;
            if (!state.formula.length) {
                html = '<tr><td colspan="3" class="text-center text-muted">El elaborado no tiene formula cargada.</td></tr>';
            } else {
                state.formula.forEach(function (item) {
                    if (item.AgregarAuto) {
                        totalAuto += toFloat(item.Kgs);
                    }
                    html += '<tr>'
                        + '<td>' + item.Producto + '</td>'
                        + '<td class="text-right">' + formatKg(item.Kgs) + '</td>'
                        + '<td class="text-center">' + formatBool(item.AgregarAuto) + '</td>'
                        + '</tr>';
                });
            }
            $('#tablaFormulaElaborado tbody').html(html);
            $('#lblTotalProducir').text(formatKg(toFloat($('#lblTotalManual').text()) + totalAuto));
        }

        function recalcularFormula() {
            if (!state.formula.length) {
                renderFormula();
                return;
            }

            var autoCodes = {};
            state.formula.forEach(function (item) {
                if (item.AgregarAuto) autoCodes[String(item.Codigo)] = true;
            });

            var totalKgSinCond = 0;
            state.lineas.forEach(function (linea) {
                if (!autoCodes[String(linea.Codigo)]) {
                    totalKgSinCond += toFloat(linea.CantKg);
                }
            });

            state.formula = state.formula.map(function (item) {
                var copy = $.extend({}, item);
                copy.Kgs = Math.round((0.01 * totalKgSinCond * toFloat(copy.Porcentaje)) * 1000) / 1000;
                return copy;
            });

            renderFormula();
        }

        function obtenerFormula(idCorte) {
            if (!idCorte || !config.obtenerFormulaUrl) {
                state.formula = [];
                $receta.val('');
                renderFormula();
                autoResizeTextarea($receta);
                return;
            }

            $.get(config.obtenerFormulaUrl, { idCorte: idCorte })
                .done(function (resp) {
                    if (!resp || !resp.ok) {
                        state.formula = [];
                        $receta.val('');
                        renderFormula();
                        autoResizeTextarea($receta);
                        return;
                    }

                    $receta.val(resp.receta || '');
                    state.formula = $.isArray(resp.formula) ? resp.formula.slice() : [];
                    recalcularFormula();
                    autoResizeTextarea($receta);
                })
                .fail(function () {
                    state.formula = [];
                    $receta.val('');
                    renderFormula();
                    autoResizeTextarea($receta);
                });
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

        function renderBalanzaStatus(payload) {
            if (window.CarnisysBalanzaUtils && typeof window.CarnisysBalanzaUtils.renderStatus === 'function') {
                window.CarnisysBalanzaUtils.renderStatus('#estadoBalanzaElaborado', '#barraBalanzaElaborado', payload);
            }
        }

        function syncBalanzaStatusVisibility() {
            $('#balanzaStatusWrapElaborado').toggle(!!$balanza.is(':checked'));
        }

        function aplicarLecturaBalanza(payload) {
            var normalized = normalizarBalanzaPayload(payload);
            state.balanzaDisponible = normalized.conectada === true;
            state.balanzaUltimaLectura = normalized;
            renderBalanzaStatus(payload);
            syncIngredienteReadonly();

            if (!normalized.conectada || !$balanza.is(':checked') || state.balanzaDesactivadaManual) {
                return;
            }

            var hayIngrediente = toInt($ingredienteId.val()) > 0;
            if (hayIngrediente && !ingredienteEsPesable()) {
                return;
            }

            $ingredienteKg.val(normalized.pesoDisplay || normalized.pesoTexto || formatKg(normalized.peso));
        }

        function aplicarStatusBalanza(payload) {
            var normalized = normalizarBalanzaPayload(payload);
            state.balanzaDisponible = normalized.conectada === true;
            renderBalanzaStatus(payload);
            syncIngredienteReadonly();
        }

        function balanzaConectadaDesdePayload(data) {
            return normalizarBalanzaPayload(data).conectada === true;
        }

        function verificarBalanza(callback) {
            if (!window.CarnisysBalanza) {
                state.balanzaDisponible = false;
                renderBalanzaStatus(null);
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
                        syncIngredienteReadonly();
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
                syncIngredienteReadonly();
                if (typeof callback === 'function') callback(false);
            });
        }

        function verificarBalanzaInicial() {
            verificarBalanza(function (disponible) {
                if (!disponible) {
                    state.balanzaDesactivadaManual = true;
                    $balanza.prop('checked', false);
                }
                syncIngredienteReadonly();
            });
        }

        function actualizarBalanzaSegunTipo() {
            if (!ingredienteEsPesable()) {
                $ingredienteKg.val('');
            }
            syncIngredienteReadonly();
        }

        function intentarLeerBalanza() {
            verificarBalanza(function (disponible, data) {
                if (!disponible) {
                    $balanza.prop('checked', false);
                    showAlert('warning', 'Balanza', 'No hay balanza conectada.');
                    return;
                }

                if (window.CarnisysBalanza) {
                    window.CarnisysBalanza.activar();
                }
                aplicarLecturaBalanza(data);
            });
        }

        function buildAcumulados() {
            var map = {};

            state.lineas.forEach(function (linea) {
                var key = String(linea.IdCorte || linea.Codigo || '');
                if (!map[key]) {
                    map[key] = {
                        codigo: linea.Codigo || '',
                        producto: linea.Producto || '',
                        cantidad: 0
                    };
                }
                map[key].cantidad += toFloat(linea.CantKg);
            });

            return Object.keys(map).map(function (key) { return map[key]; }).sort(function (a, b) {
                return String(a.codigo).localeCompare(String(b.codigo));
            });
        }

        function renderAcumulados() {
            var items = buildAcumulados();
            var html = '';

            if (!items.length) {
                html = '<tr><td colspan="3" class="text-center text-muted">No hay productos cargados.</td></tr>';
            } else {
                items.forEach(function (item) {
                    html += '<tr>'
                        + '<td>' + item.codigo + '</td>'
                        + '<td>' + item.producto + '</td>'
                        + '<td class="text-right">' + formatKg(item.cantidad) + '</td>'
                        + '</tr>';
                });
            }

            $('#tbodyAcumuladosElaborado').html(html);
        }

        function validarLineaActual() {
            var parsedKg = parseDecimal($ingredienteKg.val());
            if (toInt($ingredienteId.val()) <= 0) {
                showAlert('warning', 'Ingrediente', 'Selecciona un ingrediente valido.');
                focusIngredienteCodigo();
                return false;
            }
            if (($ingredienteKg.val() || '').trim() === '' || !parsedKg.ok || parsedKg.value <= 0) {
                showAlert('warning', 'Ingrediente', 'Ingresa una cantidad de kilos mayor a cero.');
                $ingredienteKg.focus().select();
                return false;
            }
            return true;
        }

        function agregarLinea() {
            clearWarning();
            if (!validarLineaActual()) return;

            var linea = {
                IdCorte: toInt($ingredienteId.val()),
                Codigo: toInt($ingredienteCodigo.val()),
                Producto: $ingredienteNombre.val(),
                TipoProducto: $ingredienteTipo.val(),
                CantKg: toFloat($ingredienteKg.val()),
                PesoBalanza: $balanza.is(':checked')
            };

            state.lineas.push(linea);
            renderLineas();
            clearIngrediente();
            showFeedback('Agregado correctamente: <strong>' + linea.Producto + '</strong> | Cantidad <strong>' + formatKg(linea.CantKg) + '</strong>');
            scheduleDraft();
            focusIngredienteCodigo();
        }

        function buscarPorCodigo($codigo, callback) {
            var codigo = toInt($codigo.val());
            if (!codigo || !config.buscarProductoPorCodigoUrl) return;
            var esElaborado = $codigo.is($elaboradoCodigo);
            if (esElaborado) {
                state.elaboradoRequestSeq += 1;
            } else {
                state.ingredienteRequestSeq += 1;
            }

            var requestSeq = esElaborado ? state.elaboradoRequestSeq : state.ingredienteRequestSeq;

            $.get(config.buscarProductoPorCodigoUrl, { codigo: codigo })
                .done(function (resp) {
                    if (requestSeq !== (esElaborado ? state.elaboradoRequestSeq : state.ingredienteRequestSeq)) return;
                    if (resp && resp.ok) {
                        if (esElaborado) setElaborado(resp);
                        else setIngrediente(resp);

                        if (typeof callback === 'function') callback(true, resp);
                    } else {
                        if (esElaborado) {
                            clearElaboradoSeleccion(true);
                            $elaboradoNombre.val('No existe o sin coincidencia');
                            $elaboradoCodigo.val(codigo);
                        } else {
                            clearIngredienteSeleccion(true);
                            $ingredienteNombre.val('No existe o sin coincidencia');
                            $ingredienteCodigo.val(codigo);
                        }
                        if (typeof callback === 'function') callback(false);
                    }
                })
                .fail(function () {
                    if (requestSeq !== (esElaborado ? state.elaboradoRequestSeq : state.ingredienteRequestSeq)) return;
                    if (esElaborado) {
                        clearElaboradoSeleccion(true);
                        $elaboradoNombre.val('No existe o sin coincidencia');
                    } else {
                        clearIngredienteSeleccion(true);
                        $ingredienteNombre.val('No existe o sin coincidencia');
                    }
                    if (typeof callback === 'function') callback(false);
                });
        }

        function openModalElaborado() {
            window.abrirBuscarProductoModal({
                modalSelector: '#modalBuscarElaborado',
                mostrarPrecio: false,
                onSelect: function (producto) {
                    $elaboradoCodigo.val(producto.codigo || '');
                    buscarPorCodigo($elaboradoCodigo, function (ok) {
                        if (ok) focusIngredienteCodigo();
                    });
                }
            });
        }

        function openModalIngrediente() {
            window.abrirBuscarProductoModal({
                modalSelector: '#modalBuscarIngrediente',
                mostrarPrecio: false,
                onSelect: function (producto) {
                    $ingredienteCodigo.val(producto.codigo || '');
                    buscarPorCodigo($ingredienteCodigo, function (ok) {
                        if (ok) $ingredienteKg.focus().select();
                    });
                }
            });
        }

        function validarFormulario() {
            if (config.esEdicion && !config.permiteGuardarEdicion) return 'La modificación del elaborado existente todavía no está habilitada en Web.';
            if (toInt($('#IdSucursal').val()) <= 0) return 'Debe seleccionar una sucursal.';
            if (toInt($elaboradoId.val()) <= 0) return 'Debe seleccionar el elaborado.';
            if (!state.lineas.length) return 'Debe agregar al menos un ingrediente manual.';
            return '';
        }

        $(document).on('click', '.js-remove-linea', function () {
            if (config.esAnulado) return;
            var index = toInt($(this).data('index'));
            state.lineas.splice(index, 1);
            renderLineas();
            scheduleDraft();
        });

        $('#btnBuscarElaborado').on('click', function (e) {
            if (config.esAnulado) return;
            e.preventDefault();
            openModalElaborado();
        });

        $('#btnBuscarIngrediente').on('click', function (e) {
            if (config.esAnulado) return;
            e.preventDefault();
            openModalIngrediente();
        });

        $('#btnAgregarIngrediente').on('click', function () {
            if (config.esAnulado) return;
            agregarLinea();
        });

        $('#btnVerAcumuladosElaborado').on('click', function () {
            renderAcumulados();
            $('#modalAcumuladosElaborado').modal('show');
        });

        $('#btnAnularElaborado').on('click', function () {
            if (!config.puedeAnular) return;

            var ejecutar = function () {
                $.ajax({
                    url: config.anularUrl,
                    type: 'POST',
                    data: {
                        __RequestVerificationToken: $form.find('input[name="__RequestVerificationToken"]').val(),
                        idEmbutido: toInt($('#IdEmbutido').val())
                    }
                }).done(function (resp) {
                    if (!resp || !resp.ok) {
                        showAlert('error', 'Elaborado', (resp && resp.mensaje) || 'No se pudo anular el elaborado.');
                        return;
                    }
                    clearDraft();
                    window.location.href = resp.redirectUrl || config.redirectUrl || '/Elaborados';
                }).fail(function (xhr) {
                    var mensaje = 'No se pudo anular el elaborado.';
                    if (xhr && xhr.responseJSON && xhr.responseJSON.mensaje) mensaje = xhr.responseJSON.mensaje;
                    showAlert('error', 'Elaborado', mensaje);
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

        $elaboradoCodigo.on('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                window.clearTimeout(state.elaboradoTimer);
                buscarPorCodigo($elaboradoCodigo, function (ok) {
                    if (ok) focusIngredienteCodigo();
                });
            } else if (e.key === 'F9') {
                e.preventDefault();
                openModalElaborado();
            }
        });

        $ingredienteCodigo.on('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                window.clearTimeout(state.ingredienteTimer);
                buscarPorCodigo($ingredienteCodigo, function (ok) {
                    if (ok) $ingredienteKg.focus().select();
                });
            } else if (e.key === 'F10') {
                e.preventDefault();
                openModalIngrediente();
            }
        });

        $ingredienteKg.on('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                agregarLinea();
            }
        });

        $elaboradoCodigo.on('input', function () {
            window.clearTimeout(state.elaboradoTimer);

            var raw = ($elaboradoCodigo.val() || '').trim();
            if (!raw) {
                clearElaborado();
                return;
            }

            if (!/^\d+$/.test(raw)) {
                clearElaborado();
                $elaboradoCodigo.val(raw.replace(/\D/g, ''));
                return;
            }

            state.elaboradoTimer = setTimeout(function () {
                buscarPorCodigo($elaboradoCodigo);
            }, 250);
        });

        $ingredienteCodigo.on('input', function () {
            window.clearTimeout(state.ingredienteTimer);

            var raw = ($ingredienteCodigo.val() || '').trim();
            if (!raw) {
                clearIngrediente();
                return;
            }

            if (!/^\d+$/.test(raw)) {
                clearIngrediente();
                $ingredienteCodigo.val(raw.replace(/\D/g, ''));
                return;
            }

            state.ingredienteTimer = setTimeout(function () {
                buscarPorCodigo($ingredienteCodigo);
            }, 250);
        });

        $balanza.on('change', function () {
            if (config.esAnulado) return;
            if ($balanza.is(':checked')) {
                state.balanzaDesactivadaManual = false;
                actualizarBalanzaSegunTipo();
                if ($balanza.is(':checked')) intentarLeerBalanza();
            } else {
                state.balanzaDesactivadaManual = true;
                if (window.CarnisysBalanza) {
                    window.CarnisysBalanza.desactivar();
                }
            }
            syncBalanzaStatusVisibility();
            syncIngredienteReadonly();
        });

        $(document).on('keydown.elaboradosCarga', function (e) {
            if (config.esAnulado) return;
            var tag = e.target && e.target.tagName ? e.target.tagName.toLowerCase() : '';
            var key = String(e.key || '').toLowerCase();
            if (tag === 'textarea') return;

            if (e.key === 'F9') {
                e.preventDefault();
                openModalElaborado();
                return;
            }

            if (e.key === 'F10') {
                e.preventDefault();
                openModalIngrediente();
                return;
            }

            if (e.altKey && !e.ctrlKey && !e.metaKey && !e.shiftKey && !e.repeat && key === 'enter') {
                var $primaryAction = getPrimaryActionButton();
                if (!$primaryAction.length || $primaryAction.prop('disabled')) return;

                e.preventDefault();
                $primaryAction.trigger('click');
                return;
            }

            if (e.key === '*') {
                e.preventDefault();
                if ($balanza.is(':checked')) {
                    $balanza.prop('checked', false);
                } else {
                    $balanza.prop('checked', true).trigger('change');
                }
            }
        });

        $form.on('input change', 'input, select, textarea', function () {
            scheduleDraft();
            if ($(this).is('#Receta')) autoResizeTextarea($receta);
        });

        $form.on('submit', function (e) {
            e.preventDefault();
            if (state.guardando) return;

            clearWarning();
            var error = validarFormulario();
            if (error) {
                showAlert('warning', 'Elaborado', error);
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
                    showAlert('error', 'Elaborado', (resp && resp.mensaje) || 'No se pudo guardar el elaborado.');
                    return;
                }

                clearDraft();
                showSuccessAndRedirect(resp.mensaje, resp.redirectUrl || config.redirectUrl || '/Elaborados');
            }).fail(function (xhr) {
                state.guardando = false;
                var mensaje = 'No se pudo guardar el elaborado.';
                if (xhr && xhr.responseJSON && xhr.responseJSON.mensaje) mensaje = xhr.responseJSON.mensaje;
                showAlert('error', 'Elaborado', mensaje);
            });
        });

        renderLineas();
        renderFormula();
        autoResizeTextarea($receta);
        if (toInt($elaboradoId.val()) > 0 && $.trim($receta.val())) {
            $elaboradoWarning.addClass('d-none').text('');
        }
        renderBalanzaStatus(null);
        verificarBalanzaInicial();
        syncBalanzaStatusVisibility();
        syncIngredienteReadonly();
        syncPrimaryAction();
        if (readDraft()) {
            showDraftBanner();
        }

        $page.on('click.elaboradosDraft', '[data-action="restore-draft"]', function () {
            var draft = readDraft();
            if (!draft) return;
            applyDraft(draft);
            hideDraftBanner();
        });

        $page.on('click.elaboradosDraft', '[data-action="clear-draft"]', function () {
            if (!confirm('Se eliminara el borrador local de este elaborado. ¿Continuar?')) return;
            clearDraft();
        });
        window.ElaboradosCarga = window.ElaboradosCarga || {};
        window.ElaboradosCarga.syncPrimaryAction = syncPrimaryAction;
    }

    $(function () {
        initCarga();
    });
})();
