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
            ingredienteTimer: null,
            elaboradoTimer: null,
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
        var $ingredienteTipo = $('#txtIngredienteTipo');
        var $ingredientePromedio = $('#txtIngredientePromedio');
        var $ingredienteKg = $('#txtKgIngrediente');
        var $balanza = $('#chkBalanzaIngrediente');
        var $feedback = $('#cargaFeedback');
        var $warning = $('#cargaWarning');

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
            $ingredienteNombre.val('');
            $ingredienteTipo.val('');
            $ingredientePromedio.val('');
            $ingredienteKg.val('');
        }

        function setIngrediente(producto) {
            $ingredienteId.val(producto.id || '');
            $ingredienteCodigo.val(producto.codigo || '');
            $ingredienteNombre.val(producto.nombre || '');
            $ingredienteTipo.val(producto.tipo || '');
            $ingredientePromedio.val(producto.promedio || 0);
            actualizarBalanzaSegunTipo();
        }

        function clearElaborado() {
            $elaboradoId.val('');
            if (!config.esEdicion) {
                $elaboradoCodigo.val('');
            }
            $elaboradoNombre.val('');
            $elaboradoTipo.val('');
            $elaboradoPromedio.val('');
            $receta.val('');
            state.formula = [];
            renderFormula();
            $elaboradoWarning.addClass('d-none').text('');
            autoResizeTextarea($receta);
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
                    + '<td><button type="button" class="btn btn-sm btn-outline-danger js-remove-linea" data-index="' + index + '"><i class="fas fa-trash"></i></button></td>'
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
            if (!state.formula.length) {
                html = '<tr><td colspan="3" class="text-center text-muted">El elaborado no tiene formula cargada.</td></tr>';
            } else {
                state.formula.forEach(function (item) {
                    html += '<tr>'
                        + '<td>' + item.Producto + '</td>'
                        + '<td class="text-right">' + formatKg(item.Kgs) + '</td>'
                        + '<td class="text-center">' + formatBool(item.AgregarAuto) + '</td>'
                        + '</tr>';
                });
            }
            $('#tablaFormulaElaborado tbody').html(html);
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

        function balanzaConectadaDesdePayload(data) {
            if (!data) return false;
            var estado = String(data.estado || data.status || '').toLowerCase();
            var pesoRaw = data.peso;
            if (pesoRaw === undefined || pesoRaw === null) pesoRaw = data.valor;
            if (pesoRaw === undefined || pesoRaw === null) pesoRaw = data.weight;
            var peso = Number(pesoRaw);
            return estado === 'ok' || estado === 'connected' || (!isNaN(peso) && peso !== 0);
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
                    state.balanzaDesactivadaManual = true;
                    $balanza.prop('checked', false);
                }
            });
        }

        function actualizarBalanzaSegunTipo() {
            var tipo = ($ingredienteTipo.val() || '').toLowerCase();
            if (tipo === 'unidad') {
                $balanza.prop('checked', false);
            }
        }

        function intentarLeerBalanza() {
            verificarBalanza(function (disponible, data) {
                if (!disponible) {
                    $balanza.prop('checked', false);
                    showAlert('warning', 'Balanza', 'No hay balanza conectada.');
                    return;
                }

                var pesoRaw = data && (data.peso !== undefined ? data.peso : (data.valor !== undefined ? data.valor : data.weight));
                if (pesoRaw !== undefined && pesoRaw !== null) {
                    $ingredienteKg.val(formatKg(pesoRaw));
                }
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
            activarProteccionSalida();
            focusIngredienteCodigo();
        }

        function buscarPorCodigo($codigo, callback) {
            var codigo = toInt($codigo.val());
            if (!codigo || !config.buscarProductoPorCodigoUrl) return;

            $.get(config.buscarProductoPorCodigoUrl, { codigo: codigo })
                .done(function (resp) {
                    if (resp && resp.ok) {
                        if ($codigo.is($elaboradoCodigo)) setElaborado(resp);
                        else setIngrediente(resp);

                        if (typeof callback === 'function') callback(true, resp);
                    } else {
                        if ($codigo.is($elaboradoCodigo)) clearElaborado();
                        else clearIngrediente();
                        if (typeof callback === 'function') callback(false);
                    }
                })
                .fail(function () {
                    if ($codigo.is($elaboradoCodigo)) clearElaborado();
                    else clearIngrediente();
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
            var index = toInt($(this).data('index'));
            state.lineas.splice(index, 1);
            renderLineas();
            activarProteccionSalida();
        });

        $('#btnBuscarElaborado').on('click', function (e) {
            e.preventDefault();
            openModalElaborado();
        });

        $('#btnBuscarIngrediente').on('click', function (e) {
            e.preventDefault();
            openModalIngrediente();
        });

        $('#btnAgregarIngrediente').on('click', function () {
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
                    desactivarProteccionSalida();
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
            clearTimeout(state.elaboradoTimer);
            if (!($elaboradoCodigo.val() || '').trim()) {
                clearElaborado();
                return;
            }
            state.elaboradoTimer = setTimeout(function () {
                buscarPorCodigo($elaboradoCodigo);
            }, 250);
        });

        $ingredienteCodigo.on('input', function () {
            clearTimeout(state.ingredienteTimer);
            if (!($ingredienteCodigo.val() || '').trim()) {
                clearIngrediente();
                return;
            }
            state.ingredienteTimer = setTimeout(function () {
                buscarPorCodigo($ingredienteCodigo);
            }, 250);
        });

        $balanza.on('change', function () {
            if ($balanza.is(':checked')) {
                state.balanzaDesactivadaManual = false;
                actualizarBalanzaSegunTipo();
                if ($balanza.is(':checked')) intentarLeerBalanza();
            } else {
                state.balanzaDesactivadaManual = true;
            }
        });

        $(document).on('keydown.elaboradosCarga', function (e) {
            var tag = e.target && e.target.tagName ? e.target.tagName.toLowerCase() : '';
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
            activarProteccionSalida();
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

                desactivarProteccionSalida();
                window.location.href = resp.redirectUrl || config.redirectUrl || '/Elaborados';
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
        verificarBalanzaInicial();
    }

    $(function () {
        initCarga();
    });
})();
