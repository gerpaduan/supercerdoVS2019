(function () {
    function showAlert(icon, title, text) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            Swal.fire({ icon: icon, title: title, text: text });
        } else {
            alert(text);
        }
    }

    function toInt(value) {
        var n = parseInt(value, 10);
        return isNaN(n) ? 0 : n;
    }

    function parseDecimal(value) {
        if (value === null || value === undefined) return { ok: false, value: 0 };

        var text = String(value).trim();
        if (!text) return { ok: false, value: 0 };

        text = text.replace(/\s/g, '');

        var lastComma = text.lastIndexOf(',');
        var lastDot = text.lastIndexOf('.');
        var decimalSep = '';

        if (lastComma >= 0 && lastDot >= 0) {
            decimalSep = lastComma > lastDot ? ',' : '.';
        } else if (lastComma >= 0) {
            decimalSep = ',';
        } else if (lastDot >= 0) {
            decimalSep = '.';
        }

        var normalized = '';
        var decimalIndex = decimalSep ? text.lastIndexOf(decimalSep) : -1;

        for (var i = 0; i < text.length; i++) {
            var ch = text.charAt(i);
            if (ch >= '0' && ch <= '9') {
                normalized += ch;
            } else if (ch === '-' && normalized.length === 0) {
                normalized += ch;
            } else if ((ch === ',' || ch === '.') && i === decimalIndex) {
                normalized += '.';
            }
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

    function formatDecimalForPost(value) {
        var parsed = parseDecimal(value);
        if (!parsed.ok) return '';
        return String(parsed.value).replace('.', ',');
    }

    function formatInt(value) {
        return toInt(value).toLocaleString('es-AR', { maximumFractionDigits: 0 });
    }

    function initIndex() {
        var $page = $('[data-movimientos-index="1"]');
        if (!$page.length) return;

        var detalleUrl = $page.data('detalle-url');

        function loadDetalle(id, $row, done) {
            if ($row.attr('data-loaded') === 'true') {
                if (typeof done === 'function') done();
                return;
            }

            $.get(detalleUrl, { id: id })
                .done(function (html) {
                    $row.find('.js-detalle-container').html(html);
                    $row.attr('data-loaded', 'true');
                    if (typeof done === 'function') done();
                })
                .fail(function () {
                    $row.find('.js-detalle-container').html('<div class="text-danger">No se pudo cargar el detalle.</div>');
                    if (typeof done === 'function') done();
                });
        }

        function expandRow($btn) {
            var id = $btn.data('id');
            var target = $btn.data('target');
            var $row = $(target);
            if (!$row.length) return;

            loadDetalle(id, $row, function () {
                $row.removeClass('d-none');
                $btn.find('i').removeClass('fa-angle-down').addClass('fa-angle-up');
                $btn.attr('data-open', 'true');
            });
        }

        function collapseRow($btn) {
            var target = $btn.data('target');
            $(target).addClass('d-none');
            $btn.find('i').removeClass('fa-angle-up').addClass('fa-angle-down');
            $btn.attr('data-open', 'false');
        }

        $(document).on('click.movimientosIndex', '.js-toggle-detalle', function () {
            var $btn = $(this);
            if ($btn.attr('data-open') === 'true') collapseRow($btn);
            else expandRow($btn);
        });

        $('#verDetalles').on('change.movimientosIndex', function () {
            var checked = $(this).is(':checked');
            $('.js-toggle-detalle').each(function () {
                var $btn = $(this);
                if (checked) expandRow($btn);
                else collapseRow($btn);
            });
        });

        if ($('#verDetalles').is(':checked')) {
            $('#verDetalles').trigger('change');
        }
    }

    function initEdit() {
        var $page = $('[data-movimiento-page="1"]');
        if (!$page.length) return;

        var config = window.movimientosConfig || {};
        var state = {
            lines: Array.isArray(window.movimientoLineasIniciales) ? window.movimientoLineasIniciales.slice() : [],
            draftTimer: null,
            saving: false,
            productoTimer: null,
            productoRequestSeq: 0,
            readOnly: !!config.soloLecturaInicial
        };

        var $codigo = $('#txtCodigoProducto');
        var $productoId = $('#txtProductoId');
        var $productoNombre = $('#txtProductoNombre');
        var $productoPesable = $('#txtProductoPesable');
        var $productoTipo = $('#txtProductoTipo');
        var $productoPromedio = $('#txtProductoPromedio');
        var $cantUnidad = $('#txtCantUnidad');
        var $cantKgs = $('#txtCantKgs');
        var $balanza = $('#chkBalanzaLinea');
        var $permitirWrap = $('#wrapPermitirIngreso');
        var $permitir = $('#chkPermitirIngreso');
        var $feedback = $('#movimientoFeedback');
        var $warning = $('#movimientoWarning');
        var $btnImprimirMovimiento = $('#btnImprimirMovimiento');
        var $observaciones = $('#Observaciones');
        var balanzaDisponible = false;
        var balanzaManualDesactivada = false;
        var balanzaClientStarted = false;
        var balanzaUltimaLectura = null;

        function productoEsPesable() {
            return String($productoPesable.val() || '').toLowerCase() === 'true';
        }

        function syncReadOnlyUi() {
            var hayProducto = toInt($productoId.val()) > 0;
            $('#tablaLineasMovimiento .js-remove-line').prop('disabled', !!state.readOnly);
            $cantKgs.prop('readonly', !!($balanza.is(':checked') && balanzaDisponible && !balanzaManualDesactivada && (!hayProducto || productoEsPesable())));
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
            $page.find('#movimientoDraftBanner').addClass('d-none');
        }

        function showDraftBanner() {
            $page.find('#movimientoDraftBanner').removeClass('d-none');
        }

        function clearDraft() {
            var key = getDraftKey();
            if (!key || !window.localStorage) return;
            window.localStorage.removeItem(key);
            hideDraftBanner();
        }

        function buildDraft() {
            return {
                idSucursalOrigen: $('#IdSucursalOrigen').val(),
                idSucursalDestino: $('#IdSucursalDestino').val(),
                fechaMovimiento: $('#FechaMovimiento').val(),
                observaciones: $observaciones.val(),
                currentLine: {
                    id: $productoId.val(),
                    codigo: $codigo.val(),
                    nombre: $productoNombre.val(),
                    tipo: $productoTipo.val(),
                    promedio: $productoPromedio.val(),
                    cantUnidad: $cantUnidad.val(),
                    cantKg: $cantKgs.val(),
                    pesoBalanza: $balanza.is(':checked'),
                    permitirIngreso: $permitir.is(':checked')
                },
                lineas: state.lines
            };
        }

        function saveDraft() {
            var key = getDraftKey();
            if (!key || !window.localStorage || state.saving) return;
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

        function showFeedback(text) {
            $feedback.text(text).removeClass('d-none');
            setTimeout(function () { $feedback.addClass('d-none'); }, 2200);
        }

        function showWarning(text) {
            $warning.text(text).removeClass('d-none');
        }

        function clearWarning() {
            $warning.addClass('d-none').text('');
        }

        function setProducto(producto) {
            $productoId.val(producto.id || '');
            $codigo.val(producto.codigo || '');
            $productoNombre.val(producto.nombre || '');
            $productoPesable.val(producto.pesable === true ? 'true' : 'false');
            $productoTipo.val(producto.tipo || '');
            $productoPromedio.val(producto.promedio || 0);
            if (!productoEsPesable()) {
                $cantKgs.val('');
            } else if ($balanza.is(':checked') && balanzaDisponible && balanzaUltimaLectura) {
                $cantKgs.val(balanzaUltimaLectura.pesoDisplay || balanzaUltimaLectura.pesoTexto || '');
            }
            syncReadOnlyUi();
            scheduleDraft();
        }

        function setProductoEstado(texto) {
            $productoId.val('');
            $productoNombre.val(texto || '');
            $productoTipo.val('');
            $productoPromedio.val('');
        }

        function clearProducto() {
            $productoId.val('');
            $codigo.val('');
            $productoNombre.val('');
            $productoPesable.val('');
            $productoTipo.val('');
            $productoPromedio.val('');
            $cantUnidad.val('');
            if ($balanza.is(':checked') && balanzaDisponible && balanzaUltimaLectura && !balanzaManualDesactivada) {
                $cantKgs.val(balanzaUltimaLectura.pesoDisplay || balanzaUltimaLectura.pesoTexto || '');
            } else {
                $cantKgs.val('');
            }
            $permitir.prop('checked', false);
            $permitirWrap.addClass('d-none');
            clearWarning();
        }

        function applyDraft(draft) {
            if (!draft) return;

            $('#IdSucursalOrigen').val(draft.idSucursalOrigen || $('#IdSucursalOrigen').val());
            $('#IdSucursalDestino').val(draft.idSucursalDestino || $('#IdSucursalDestino').val());
            $('#FechaMovimiento').val(draft.fechaMovimiento || $('#FechaMovimiento').val());
            $observaciones.val(draft.observaciones || '');

            if (draft.currentLine) {
                $productoId.val(draft.currentLine.id || '');
                $codigo.val(draft.currentLine.codigo || '');
                $productoNombre.val(draft.currentLine.nombre || '');
                $productoTipo.val(draft.currentLine.tipo || '');
                $productoPromedio.val(draft.currentLine.promedio || '');
                $cantUnidad.val(draft.currentLine.cantUnidad || '');
                $cantKgs.val(draft.currentLine.cantKg || '');
                $balanza.prop('checked', draft.currentLine.pesoBalanza === true);
                $permitir.prop('checked', draft.currentLine.permitirIngreso === true);
            }

            state.lines = $.isArray(draft.lineas) ? draft.lineas : [];
            renderLines();
            validarRelacionCantidadKilos();
            autoResizeObservaciones();
            scheduleDraft();
        }

        function focusCodigo() {
            setTimeout(function () {
                $codigo.focus();
                var input = $codigo.get(0);
                if (input && typeof input.setSelectionRange === 'function') {
                    var end = ($codigo.val() || '').toString().length;
                    input.setSelectionRange(end, end);
                }
            }, 30);
        }

        function autoResizeObservaciones() {
            if (!$observaciones.length) return;
            $observaciones.css('height', 'auto');
            $observaciones.css('height', $observaciones[0].scrollHeight + 'px');
        }

        function setBalanzaDisponible(disponible) {
            balanzaDisponible = !!disponible;
            syncReadOnlyUi();
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
                window.CarnisysBalanzaUtils.renderStatus('#estadoBalanzaMovimiento', '#barraBalanzaMovimiento', payload);
            }
        }

        function aplicarLecturaBalanza(payload) {
            var normalized = normalizarBalanzaPayload(payload);
            balanzaUltimaLectura = normalized;
            setBalanzaDisponible(normalized.conectada === true);
            renderBalanzaStatus(payload);

            var hayProducto = toInt($productoId.val()) > 0;
            if (!normalized.conectada || !$balanza.is(':checked') || balanzaManualDesactivada || (hayProducto && !productoEsPesable())) {
                return;
            }

            $cantKgs.val(normalized.pesoDisplay || normalized.pesoTexto || '');
            syncReadOnlyUi();
            validarRelacionCantidadKilos();
        }

        function aplicarStatusBalanza(payload) {
            var normalized = normalizarBalanzaPayload(payload);
            setBalanzaDisponible(normalized.conectada === true);
            renderBalanzaStatus(payload);
        }

        function verificarBalanza(callback) {
            if (!window.CarnisysBalanza) {
                setBalanzaDisponible(false);
                renderBalanzaStatus(null);
                if (typeof callback === 'function') callback(false);
                return;
            }

            if (!balanzaClientStarted) {
                balanzaClientStarted = true;
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
                        setBalanzaDisponible(false);
                        renderBalanzaStatus(null);
                    }
                });
            }

            window.CarnisysBalanza.leerAhora().then(function (data) {
                var disponible = normalizarBalanzaPayload(data).conectada === true;
                aplicarLecturaBalanza(data);
                if (typeof callback === 'function') callback(disponible);
            }).catch(function () {
                setBalanzaDisponible(false);
                renderBalanzaStatus(null);
                if (typeof callback === 'function') callback(false);
            });
        }

        function intentarActivarBalanza() {
            verificarBalanza(function (disponible) {
                if (!disponible) {
                    $balanza.prop('checked', false);
                    showAlert('warning', 'Balanza', 'No hay balanza conectada.');
                    return;
                }

                balanzaManualDesactivada = false;
                $balanza.prop('checked', true);
                window.CarnisysBalanza.activar();
                if (balanzaUltimaLectura && balanzaUltimaLectura.conectada) {
                    $cantKgs.val(balanzaUltimaLectura.pesoDisplay || balanzaUltimaLectura.pesoTexto || '');
                }
                syncReadOnlyUi();
            });
        }

        function renderLines() {
            var html = '';
            var hidden = '';
            var totalItems = state.lines.length;
            var totalUnidades = 0;
            var totalKilos = 0;

            state.lines.forEach(function (line, index) {
                totalUnidades += toInt(line.CantUnidad);
                totalKilos += toFloat(line.CantKg);

                html += '<tr>'
                    + '<td>' + line.Codigo + '</td>'
                    + '<td>' + line.Producto + '</td>'
                    + '<td class="text-right">' + formatInt(line.CantUnidad) + '</td>'
                    + '<td class="text-right">' + formatKg(line.CantKg) + '</td>'
                    + '<td class="text-center">' + (line.PesoBalanza ? 'Sí' : 'No') + '</td>'
                    + '<td class="text-center">' + (line.PermitirIngreso ? 'Sí' : 'No') + '</td>'
                    + '<td><button type="button" class="btn btn-sm btn-outline-danger js-remove-line" data-index="' + index + '"' + (state.readOnly ? ' disabled="disabled"' : '') + '><i class="fas fa-trash"></i></button></td>'
                    + '</tr>';

                hidden += '<input type="hidden" name="Lineas[' + index + '].IdCorteMovimiento" value="' + (line.IdCorteMovimiento || 0) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].IdCorte" value="' + (line.IdCorte || 0) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].Codigo" value="' + (line.Codigo || 0) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].Producto" value="' + $('<div>').text(line.Producto || '').html() + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].TipoProducto" value="' + $('<div>').text(line.TipoProducto || '').html() + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].PromedioProducto" value="' + formatDecimalForPost(line.PromedioProducto) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].CantUnidad" value="' + toInt(line.CantUnidad) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].CantKg" value="' + formatDecimalForPost(line.CantKg) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].PesoBalanza" value="' + (line.PesoBalanza ? 'true' : 'false') + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].PermitirIngreso" value="' + (line.PermitirIngreso ? 'true' : 'false') + '" />';
            });

            if (!html) {
                html = '<tr><td colspan="7" class="text-center text-muted">Todavía no agregaste productos al movimiento.</td></tr>';
            }

            $('#tablaLineasMovimiento tbody').html(html);
            $('#lineasHiddenContainer').html(hidden);
            $('#lblTotalItems').text(formatInt(totalItems));
            $('#lblTotalUnidades').text(formatInt(totalUnidades));
            $('#lblTotalKilos').text(formatKg(totalKilos));
            syncReadOnlyUi();
        }

        function buildAcumulados() {
            var map = {};
            state.lines.forEach(function (line) {
                var key = String(line.IdCorte || '') || String(line.Codigo || '');
                if (!map[key]) {
                    map[key] = {
                        Codigo: line.Codigo,
                        Producto: line.Producto,
                        CantUnidad: 0,
                        CantKg: 0
                    };
                }
                map[key].CantUnidad += toInt(line.CantUnidad);
                map[key].CantKg += toFloat(line.CantKg);
            });
            return Object.keys(map).map(function (key) { return map[key]; }).sort(function (a, b) {
                return String(a.Codigo).localeCompare(String(b.Codigo));
            });
        }

        function renderAcumulados() {
            var items = buildAcumulados();
            var html = '';
            if (!items.length) {
                html = '<tr><td colspan="4" class="text-center text-muted">No hay productos cargados.</td></tr>';
            } else {
                items.forEach(function (item) {
                    html += '<tr>'
                        + '<td>' + item.Codigo + '</td>'
                        + '<td>' + item.Producto + '</td>'
                        + '<td class="text-right">' + formatInt(item.CantUnidad) + '</td>'
                        + '<td class="text-right">' + formatKg(item.CantKg) + '</td>'
                        + '</tr>';
                });
            }
            $('#tbodyAcumuladosMovimiento').html(html);
        }

        function validarRelacionCantidadKilos() {
            var promedio = toFloat($productoPromedio.val());
            var cantUnidad = toInt($cantUnidad.val());
            var cantKg = toFloat($cantKgs.val());

            $permitirWrap.addClass('d-none');
            clearWarning();

            if (promedio <= 0 || cantUnidad <= 0 || cantKg <= 0 || $permitir.is(':checked')) return true;

            var limitInferior = promedio * (cantUnidad - 1);
            var limitSuperior = promedio * (cantUnidad + 1);
            if (!(limitInferior < cantKg && cantKg < limitSuperior)) {
                $permitirWrap.removeClass('d-none');
                showWarning('La cantidad ingresada no es consistente con los kilos del producto. Revisá la línea o habilitá permitir ingreso.');
                return false;
            }

            return true;
        }

        function buildCurrentLine() {
            return {
                IdCorteMovimiento: 0,
                IdCorte: toInt($productoId.val()),
                Codigo: toInt($codigo.val()),
                Producto: $productoNombre.val(),
                TipoProducto: $productoTipo.val(),
                PromedioProducto: toFloat($productoPromedio.val()),
                CantUnidad: toInt($cantUnidad.val()),
                CantKg: toFloat($cantKgs.val()),
                PesoBalanza: $balanza.is(':checked') && productoEsPesable(),
                PermitirIngreso: $permitir.is(':checked')
            };
        }

        function validateCurrentLine() {
            var parsedCantKg = parseDecimal($cantKgs.val());

            if (toInt($productoId.val()) <= 0) {
                showAlert('warning', 'Movimiento', 'Seleccioná un producto válido.');
                focusCodigo();
                return false;
            }
            if (($cantUnidad.val() || '').trim() === '') {
                showAlert('warning', 'Movimiento', 'Ingresá la cantidad.');
                $cantUnidad.focus().select();
                return false;
            }
            if (($cantKgs.val() || '').trim() === '') {
                showAlert('warning', 'Movimiento', 'Ingresá una cantidad de kilogramos mayor a cero.');
                $cantKgs.focus().select();
                return false;
            }
            if (!parsedCantKg.ok) {
                showAlert('warning', 'Movimiento', 'Ingresá una cantidad de kilogramos válida.');
                $cantKgs.focus().select();
                return false;
            }
            if (parsedCantKg.value <= 0) {
                showAlert('warning', 'Movimiento', 'Ingresá una cantidad de kilogramos mayor a cero.');
                $cantKgs.focus().select();
                return false;
            }
            if (!validarRelacionCantidadKilos() && !$permitir.is(':checked')) {
                $cantUnidad.focus().select();
                return false;
            }
            return true;
        }

        function addCurrentLine() {
            if (!validateCurrentLine()) return;
            var line = buildCurrentLine();
            state.lines.push(line);
            renderLines();
            showFeedback('Agregado correctamente: ' + line.Producto + ' | Cantidad ' + line.CantUnidad + ' | Kilos ' + formatKg(line.CantKg));
            clearProducto();
            scheduleDraft();
            focusCodigo();
        }

        function permitirIngresoVisible() {
            return $permitirWrap.is(':visible') && !$permitirWrap.hasClass('d-none');
        }

        function registrarConPermitirIngreso() {
            if (!permitirIngresoVisible()) return false;
            $permitir.prop('checked', true);
            addCurrentLine();
            return true;
        }

        function buscarPorCodigo(callback, preserveFocus) {
            var codigo = toInt($codigo.val());
            if (!codigo || !config.urlBuscarProductoPorCodigo) return;
            state.productoRequestSeq += 1;
            var requestSeq = state.productoRequestSeq;
            setProductoEstado('Buscando...');

            $.get(config.urlBuscarProductoPorCodigo, { codigo: codigo })
                .done(function (resp) {
                    if (requestSeq !== state.productoRequestSeq) return;
                    if (resp && resp.ok) {
                        setProducto(resp);
                        if (preserveFocus) {
                            focusCodigo();
                        }
                        if (typeof callback === 'function') callback(true);
                    } else {
                        $productoId.val('');
                        $productoTipo.val('');
                        $productoPromedio.val('');
                        $productoNombre.val('No existe o sin coincidencia');
                        $codigo.val(codigo);
                        if (preserveFocus) {
                            focusCodigo();
                            if (typeof callback === 'function') callback(false);
                            return;
                        }
                        if (preserveFocus) {
                            showAlert('warning', 'Producto', (resp && resp.mensaje) || 'No se encontró el producto.');
                            focusCodigo();
                        }
                        if (typeof callback === 'function') callback(false);
                    }
                })
                .fail(function () {
                    if (requestSeq !== state.productoRequestSeq) return;
                    $productoId.val('');
                    $productoTipo.val('');
                    $productoPromedio.val('');
                    $productoNombre.val('No existe o sin coincidencia');
                    if (preserveFocus) {
                        focusCodigo();
                        if (typeof callback === 'function') callback(false);
                        return;
                    }
                    focusCodigo();
                    if (typeof callback === 'function') callback(false);
                });
        }

        function openProductoModal() {
            if (typeof window.abrirBuscarProductoModal !== 'function') {
                showAlert('error', 'Producto', 'No se pudo abrir el buscador de productos.');
                return;
            }

            window.abrirBuscarProductoModal({
                modalSelector: '#modalBuscarProductoMovimiento',
                mostrarPrecio: false,
                onSelect: function (producto) {
                    $codigo.val(producto.codigo || '');
                    buscarPorCodigo(function (ok) {
                        if (ok) {
                            $cantUnidad.focus().select();
                        }
                    });
                }
            });
        }

        function openPrintOptions() {
            if (!config.imprimirUrl || !window.PostMovimientoModal || typeof window.PostMovimientoModal.open !== 'function') {
                showAlert('warning', 'Movimiento', 'Todavía no se pueden mostrar las opciones de impresión para este movimiento.');
                return;
            }

            window.PostMovimientoModal.open({
                redirectUrl: '',
                imprimirUrl: config.imprimirUrl,
                imprimirPayloadUrl: config.imprimirPayloadUrl,
                pdfUrl: config.pdfUrl,
                whatsappTexto: config.whatsappTexto,
                stayOnPage: true
            });
        }

        $(document).on('click.movimientoBuscarProducto', '#btnBuscarProducto', function (e) {
            e.preventDefault();
            openProductoModal();
        });

        $('#btnAgregarProducto').on('click', function () {
            addCurrentLine();
        });

        $('#btnVerAcumulados').on('click', function () {
            renderAcumulados();
            $('#modalAcumuladosMovimiento').modal('show');
        });

        $(document).on('click.movimientoLine', '.js-remove-line', function () {
            if (state.readOnly) return;
            var index = toInt($(this).data('index'));
            state.lines.splice(index, 1);
            renderLines();
            scheduleDraft();
        });

        $btnImprimirMovimiento.on('click', function (e) {
            e.preventDefault();
            openPrintOptions();
        });

        $codigo.on('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                window.clearTimeout(state.productoTimer);
                buscarPorCodigo(function (ok) {
                    if (ok) $cantUnidad.focus().select();
                }, false);
            } else if (e.key === 'F10') {
                e.preventDefault();
                openProductoModal();
            }
        });

        $codigo.on('input', function () {
            window.clearTimeout(state.productoTimer);

            var raw = ($codigo.val() || '').trim();
            if (!raw) {
                clearProducto();
                return;
            }

            if (!/^\d+$/.test(raw)) {
                clearProducto();
                $codigo.val(raw.replace(/\D/g, ''));
                return;
            }

            setProductoEstado('Buscando...');

            state.productoTimer = window.setTimeout(function () {
                buscarPorCodigo(null, true);
            }, 250);
        });

        $cantUnidad.on('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                $cantKgs.focus().select();
            }
        });

        $cantKgs.on('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                $('#btnAgregarProducto').focus();
            }
        });

        $('#btnAgregarProducto').on('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                addCurrentLine();
            }
        });

        $('#txtCantUnidad').on('input change', function () {
            if (!productoEsPesable()) {
                $cantKgs.val($cantUnidad.val() || '');
            }
            validarRelacionCantidadKilos();
        });

        $('#txtCantKgs, #chkPermitirIngreso').on('input change', function () {
            validarRelacionCantidadKilos();
        });

        $observaciones.on('input', autoResizeObservaciones);

        $balanza.on('change', function () {
            if ($balanza.is(':checked')) {
                if (!productoEsPesable()) {
                    syncReadOnlyUi();
                    return;
                }
                balanzaManualDesactivada = false;
                if (!balanzaDisponible) {
                    intentarActivarBalanza();
                    return;
                }

                if (window.CarnisysBalanza) {
                    window.CarnisysBalanza.activar();
                }
                if (balanzaUltimaLectura && balanzaUltimaLectura.conectada) {
                    $cantKgs.val(balanzaUltimaLectura.pesoDisplay || balanzaUltimaLectura.pesoTexto || '');
                }
            } else {
                balanzaManualDesactivada = true;
                if (window.CarnisysBalanza) {
                    window.CarnisysBalanza.desactivar();
                }
            }

            syncReadOnlyUi();
            scheduleDraft();
        });

        $(document).on('keydown.movimientoGlobal', function (e) {
            if (!$page.length) return;
            var tag = e.target && e.target.tagName ? e.target.tagName.toLowerCase() : '';
            if (e.key === 'F10' && tag !== 'textarea') {
                e.preventDefault();
                openProductoModal();
                return;
            }

            if ((e.key === '+' || e.key === 'Add') && tag !== 'textarea') {
                if (registrarConPermitirIngreso()) {
                    e.preventDefault();
                    return;
                }
            }

            if (e.key === '*' && tag !== 'textarea') {
                e.preventDefault();
                if ($balanza.is(':checked')) {
                    $balanza.prop('checked', false).trigger('change');
                } else {
                    intentarActivarBalanza();
                }
            }
        });

        $('#formMovimiento').on('submit', function (e) {
            e.preventDefault();
            renderLines();
            clearWarning();
            state.saving = true;

            $.ajax({
                url: $(this).attr('action'),
                type: 'POST',
                data: $(this).serialize()
            }).done(function (resp) {
                if (!resp || !resp.ok) {
                    state.saving = false;
                    showAlert('error', 'Movimiento', (resp && resp.mensaje) || 'No se pudo guardar el movimiento.');
                    return;
                }

                clearDraft();
                if (window.PostMovimientoModal && typeof window.PostMovimientoModal.open === 'function') {
                    window.PostMovimientoModal.open(resp);
                } else {
                    showAlert('success', 'Movimiento', resp.mensaje || 'El movimiento se guardó correctamente.');
                    window.location.href = resp.redirectUrl || config.redirectUrl || '/Movimientos';
                }
            }).fail(function (xhr) {
                state.saving = false;
                var mensaje = 'No se pudo guardar el movimiento.';
                if (xhr && xhr.responseJSON && xhr.responseJSON.mensaje) mensaje = xhr.responseJSON.mensaje;
                showAlert('error', 'Movimiento', mensaje);
            });
        });

        $page.on('click.movimientosDraft', '[data-action="restore-draft"]', function () {
            var draft = readDraft();
            if (!draft) return;
            applyDraft(draft);
            hideDraftBanner();
        });

        $page.on('click.movimientosDraft', '[data-action="clear-draft"]', function () {
            if (!confirm('Se eliminará el borrador local de este movimiento. ¿Continuar?')) return;
            clearDraft();
        });

        $('#formMovimiento').on('input change', 'input, select, textarea', function () {
            scheduleDraft();
        });

        renderLines();
        renderBalanzaStatus(null);
        verificarBalanza();
        autoResizeObservaciones();
        if (readDraft()) {
            showDraftBanner();
        }
        focusCodigo();

        window.MovimientosEdit = window.MovimientosEdit || {};
        window.MovimientosEdit.setReadOnly = function (readOnly) {
            state.readOnly = !!readOnly;
            syncReadOnlyUi();
        };
    }

    $(function () {
        initIndex();
        initEdit();
    });
})();
