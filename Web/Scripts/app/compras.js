(function (window, $) {
    'use strict';

    if (!window || !$) return;

    function toNumber(value) {
        if (value === null || value === undefined || value === '') return 0;
        var text = String(value).replace(/\s/g, '');
        if (text.indexOf(',') >= 0 && text.indexOf('.') >= 0) {
            text = text.replace(/\./g, '').replace(',', '.');
        } else if (text.indexOf(',') >= 0) {
            text = text.replace(',', '.');
        }
        var num = parseFloat(text);
        return isNaN(num) ? 0 : num;
    }

    function escapeHtml(value) {
        return String(value == null ? '' : value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function formatNumber(value) {
        return toNumber(value).toFixed(2);
    }

    function formatDecimalForPost(value) {
        return String(toNumber(value)).replace('.', ',');
    }

    function parseInputNumber($input) {
        var raw = $.trim(($input && $input.length ? $input.val() : '') || '');
        if (raw === '') return { empty: true, valid: false, value: 0 };

        var normalized = raw.replace(/\s/g, '');
        if (normalized.indexOf(',') >= 0 && normalized.indexOf('.') >= 0) {
            normalized = normalized.replace(/\./g, '').replace(',', '.');
        } else if (normalized.indexOf(',') >= 0) {
            normalized = normalized.replace(',', '.');
        }

        var value = parseFloat(normalized);
        return {
            empty: false,
            valid: !isNaN(value),
            value: isNaN(value) ? 0 : value
        };
    }

    function buildState(config) {
        return {
            config: config || {},
            lineas: [],
            lineasSortAsc: false,
            saving: false,
            personaTimer: null,
            productoTimer: null,
            draftTimer: null,
            loadingPersonaModal: false,
            ultimoAgregadoTimer: null
        };
    }

    function getState($form) {
        return $form.data('__comprasState');
    }

    function setState($form, state) {
        $form.data('__comprasState', state);
    }

    function tipoActual($form) {
        return String($form.find('#TipoCompra').val() || '');
    }

    function esMediaRes($form) {
        return tipoActual($form).toLowerCase() === 'media res';
    }

    function permiteMediaRes($form) {
        var state = getState($form);
        return !!(state && state.config && state.config.permiteMediaRes);
    }

    function setProveedor($form, persona) {
        $form.find('#idPersona').val(persona && persona.id ? persona.id : 0);
        $form.find('#razonSocial').val(persona && persona.razon ? persona.razon : '');
        $form.find('#lblProveedorCuit').text(persona && persona.cuit ? ('CUIT: ' + persona.cuit) : 'Sin proveedor seleccionado');
    }

    var nativeAlert = window.alert ? window.alert.bind(window) : function () { };

    function focusField($field) {
        if (!$field || !$field.length) return;

        window.setTimeout(function () {
            $field.trigger('focus');
            if (typeof $field.select === 'function') {
                $field.select();
            }
        }, 0);
    }

    function showValidationMessage(message, $field) {
        if (window.Swal) {
            Swal.fire({
                icon: 'warning',
                title: 'Validacion',
                text: message || 'Revise los datos ingresados.'
            }).then(function () {
                focusField($field);
            });
            return;
        }

        nativeAlert(message || 'Revise los datos ingresados.');
        focusField($field);
    }

    function resolveValidationField(message) {
        var text = String(message || '').toLowerCase();
        var $form = $('#formCompra');
        if (!$form.length) return $();

        if (text.indexOf('producto') >= 0 && text.indexOf('val') >= 0) {
            return $form.find('#txtCodigoProducto');
        }
        if (text.indexOf('cantidad mayor a cero') >= 0) {
            return $form.find('#txtCantKgs');
        }
        if (text.indexOf('kilos mayores a cero') >= 0) {
            return $form.find('#txtKgMedia');
        }
        if (text.indexOf('margen no puede ser negativo') >= 0) {
            return $form.find('#txtMargen');
        }
        if (text.indexOf('precio mayor a cero') >= 0) {
            var $activo = $(document.activeElement);
            if ($activo.is('#txtPrecioMedia, #txtKgMedia, #btnAgregarLineaMediaRes')) {
                return $form.find('#txtPrecioMedia');
            }
            return $form.find('#txtPrecioKg');
        }

        return $();
    }

    function alert(message) {
        nativeAlert(message);
        focusField(resolveValidationField(message));
    }

    function getDescRecargoValue($form) {
        if (!$form.find('#chkAplicarDescRecargo').is(':checked')) return 0;
        return toNumber($form.find('#txtDescRecargo').val());
    }

    function getIvaValue($form) {
        if (!$form.find('#chkAplicarIva').is(':checked')) return 0;
        return toNumber($form.find('#txtIvaCompra').val());
    }

    function getBasePrice($form) {
        return toNumber($form.find('#txtPrecioKg').val());
    }

    function getCurrentProductPrice($form) {
        return toNumber($form.find('#txtPrecioActualProducto').val());
    }

    function setCurrentProductPrice($form, price) {
        var formatted = toNumber(price) > 0 ? formatNumber(price) : '';
        $form.find('#txtPrecioActualProducto').val(formatted);
        $form.find('#lblPrecioActualProducto').text(formatted || '-');
    }

    function updatePrecioVentaWarning($form) {
        var showWarning = false;
        var enabled = $form.find('#chkAplicarPrecioVentaMargen').is(':checked');
        var currentPrice = getCurrentProductPrice($form);
        var precioVenta = parseInputNumber($form.find('#txtPrecioVenta'));

        if (enabled && currentPrice > 0 && precioVenta.valid && precioVenta.value > 0 && precioVenta.value < currentPrice) {
            showWarning = true;
        }

        $form.find('#lblAdvertenciaPrecioVenta').toggle(showWarning);
    }

    function getAdjustedUnitPrice($form) {
        var price = getBasePrice($form);
        var descRecargo = getDescRecargoValue($form);
        var iva = getIvaValue($form);

        if ($form.find('#chkAplicarDescRecargo').is(':checked')) {
            price = price * (1 + (descRecargo / 100));
        }

        if ($form.find('#chkAplicarIva').is(':checked')) {
            price = price * (1 + (iva / 100));
        }

        return price;
    }

    function getCurrentSubtotal($form) {
        return toNumber($form.find('#txtCantKgs').val()) * getAdjustedUnitPrice($form);
    }

    function resolvePrecioVentaUpdate($form) {
        var enabled = $form.find('#chkAplicarPrecioVentaMargen').is(':checked');
        var byMargin = $form.find('#optMargen').is(':checked');
        var precioVenta = parseInputNumber($form.find('#txtPrecioVenta'));
        var margen = parseInputNumber($form.find('#txtMargen'));
        var actualizar = false;

        if (enabled) {
            if (byMargin) {
                actualizar = margen.valid && margen.value > 0 && precioVenta.valid && precioVenta.value > 0;
            } else {
                actualizar = precioVenta.valid && precioVenta.value > 0;
            }
        }

        return {
            precioVenta: precioVenta.valid ? precioVenta.value : 0,
            actualizar: actualizar
        };
    }

    function isContinuousProductMode($form) {
        return $form.find('#chkCargaContinuaProducto').is(':checked');
    }

    function isContinuousMediaMode($form) {
        return $form.find('#chkCargaContinuaMedia').is(':checked');
    }

    function setCantMedias($form, value) {
        var text = value === null || value === undefined ? '' : String(value);
        $form.find('#CantMedias').val(text);
        $form.find('.js-cant-medias-visible').val(text);
    }

    function getCantMedias($form) {
        return $.trim($form.find('#CantMedias').val() || '');
    }

    function syncContinuousProductState($form) {
        var enabled = isContinuousProductMode($form);
        var productName = $.trim($form.find('#txtProductoNombre').val() || '');
        var priceText = $.trim($form.find('#txtPrecioKg').val() || '');

        $form.find('#panelCargaContinuaProducto').toggle(enabled);
        $form.find('#lblProductoContinuo').text(productName || '-');
        $form.find('#lblPrecioContinuo').text(priceText ? formatNumber(priceText) : '-');
    }

    function syncContinuousMediaState($form) {
        var enabled = isContinuousMediaMode($form);
        var priceText = $.trim($form.find('#txtPrecioMedia').val() || '');

        $form.find('#panelCargaContinuaMedia').toggle(enabled);
        $form.find('#lblPrecioMediaContinuo').text(priceText ? formatNumber(priceText) : '-');
    }

    function updateSubtotalPreview($form) {
        $form.find('#txtSubtotalLinea').val(formatNumber(getCurrentSubtotal($form)));
    }

    function syncPrecioVentaMargen($form, source) {
        var enabled = $form.find('#chkAplicarPrecioVentaMargen').is(':checked');
        var $panel = $form.find('#panelPrecioVentaMargen');
        var byMargin = $form.find('#optMargen').is(':checked');
        var adjustedPrice = getAdjustedUnitPrice($form);

        $panel.toggle(enabled);
        $form.find('#txtMargen').prop('readonly', !enabled || !byMargin);
        $form.find('#txtPrecioVenta').prop('readonly', !enabled || byMargin);

        if (!enabled) {
            $form.find('#txtMargen').val('');
            $form.find('#txtPrecioVenta').val('');
            updatePrecioVentaWarning($form);
            return;
        }

        if (adjustedPrice <= 0) {
            if (byMargin) {
                $form.find('#txtPrecioVenta').val('');
            } else {
                $form.find('#txtMargen').val('');
            }
            updatePrecioVentaWarning($form);
            return;
        }

        if (byMargin) {
            var margen = parseInputNumber($form.find('#txtMargen'));
            var nuevoPrecioVenta = margen.valid ? adjustedPrice * (1 + (margen.value / 100)) : NaN;
            if (margen.valid && margen.value > 0 && isFinite(nuevoPrecioVenta) && nuevoPrecioVenta > 0) {
                $form.find('#txtPrecioVenta').val(formatNumber(nuevoPrecioVenta));
            } else {
                $form.find('#txtPrecioVenta').val('');
            }
        } else {
            var precioVenta = parseInputNumber($form.find('#txtPrecioVenta'));
            var margenCalculado = precioVenta.valid ? (((precioVenta.value / adjustedPrice) - 1) * 100) : NaN;
            if (precioVenta.valid && precioVenta.value > 0 && isFinite(margenCalculado)) {
                $form.find('#txtMargen').val(formatNumber(margenCalculado));
            } else {
                $form.find('#txtMargen').val('');
            }
        }

        updatePrecioVentaWarning($form);
    }

    function syncPriceHelpers($form, source) {
        $form.find('#panelDescRecargo').toggle($form.find('#chkAplicarDescRecargo').is(':checked'));
        $form.find('#panelIva').toggle($form.find('#chkAplicarIva').is(':checked'));
        updateSubtotalPreview($form);
        syncPrecioVentaMargen($form, source);
        syncContinuousProductState($form);
    }

    function clearProductoInputs($form, preserveCode) {
        var codigo = preserveCode ? ($form.find('#txtCodigoProducto').val() || '') : '';
        $form.find('#txtProductoId').val('');
        $form.find('#txtPrecioActualProducto').val('');
        $form.find('#txtCodigoProducto').val(codigo);
        $form.find('#txtProductoNombre').val('');
        $form.find('#lblPrecioActualProducto').text('-');
        $form.find('#txtCantKgs').val('');
        $form.find('#txtPrecioKg').val('');
        $form.find('#txtSubtotalLinea').val('');
        $form.find('#txtPrecioVenta').val('');
        syncPriceHelpers($form, 'reset');
    }

    function clearProductoCantidadOnly($form) {
        $form.find('#txtCantKgs').val('');
        $form.find('#txtSubtotalLinea').val('');
        syncPriceHelpers($form, 'change');
    }

    function clearMediaResInputs($form) {
        $form.find('#txtNroTropa').val('');
        $form.find('#txtKgMedia').val('');
        $form.find('#txtPrecioMedia').val('');
        syncContinuousMediaState($form);
    }

    function clearMediaResCantidadOnly($form) {
        $form.find('#txtKgMedia').val('');
    }

    function setProductoActual($form, producto) {
        $form.find('#txtProductoId').val(producto && producto.id ? producto.id : '');
        $form.find('#txtCodigoProducto').val(producto && producto.codigo ? producto.codigo : '');
        $form.find('#txtProductoNombre').val(producto && producto.nombre ? producto.nombre : '');
        setCurrentProductPrice($form, producto && producto.precio !== undefined && producto.precio !== null ? producto.precio : 0);
        $form.find('#txtPrecioKg').val('');
        syncPriceHelpers($form, 'producto');
    }

    function showUltimoAgregado($form, message, alertSelector) {
        var state = getState($form);
        var $alert = $form.find(alertSelector || '#alertUltimoAgregadoProducto');
        if (!$alert.length) return;

        window.clearTimeout(state.ultimoAgregadoTimer);
        $alert.stop(true, true)
            .removeClass('alert-info')
            .addClass('alert-success')
            .text(message)
            .fadeIn(120);

        state.ultimoAgregadoTimer = window.setTimeout(function () {
            $alert.fadeOut(250);
        }, 2600);
    }

    function recalculate($form) {
        var state = getState($form);
        var totalKg = 0;
        var totalImporte = 0;

        $.each(state.lineas, function (_, linea) {
            if (linea.tipoLinea === 'MediaRes') {
                totalKg += toNumber(linea.kgMedia);
                totalImporte += toNumber(linea.kgMedia) * toNumber(linea.precioMedia);
            } else {
                totalKg += toNumber(linea.cantKgs);
                totalImporte += toNumber(linea.totalLinea || (linea.cantKgs * linea.precioKg));
            }
        });

        $form.find('#totalItems').text(state.lineas.length);
        $form.find('#totalKg').text(formatNumber(totalKg));
        $form.find('#totalImporte').text(formatNumber(totalImporte));

        if (permiteMediaRes($form) && esMediaRes($form)) {
            setCantMedias($form, state.lineas.length ? state.lineas.length : '');
            $form.find('#KgsMedias').val(formatNumber(totalKg));
        } else {
            $form.find('#KgsMedias').val('0');
        }
    }

    function renderLineas($form) {
        var state = getState($form);
        var $tbody = $form.find('#tablaLineasCompra tbody');
        var html = '';
        var start = state.lineasSortAsc ? 0 : state.lineas.length - 1;
        var end = state.lineasSortAsc ? state.lineas.length : -1;
        var step = state.lineasSortAsc ? 1 : -1;

        $form.find('.js-line-order-indicator').text(state.lineasSortAsc ? '↑' : '↓');
        $form.find('.js-line-order-toggle').attr('title', state.lineasSortAsc ? 'Orden ascendente por número' : 'Orden descendente por número');

        if (!state.lineas.length) {
            $tbody.html('<tr class="js-empty-row"><td colspan="8" class="text-center text-muted">Todavía no hay líneas cargadas.</td></tr>');
            recalculate($form);
            return;
        }

        for (var index = start; index !== end; index += step) {
            var linea = state.lineas[index];
            var numeroLinea = index + 1;
            if (linea.tipoLinea === 'MediaRes') {
                var totalMedia = toNumber(linea.kgMedia) * toNumber(linea.precioMedia);
                html += '<tr data-index="' + index + '">'
                    + '<td class="text-center font-weight-bold">' + numeroLinea + '</td>'
                    + '<td><strong>Media</strong></td>'
                    + '<td class="text-right">' + formatNumber(linea.kgMedia) + '</td>'
                    + '<td class="text-right">' + formatNumber(linea.precioMedia) + '</td>'
                    + '<td class="text-right">-</td>'
                    + '<td class="text-right">-</td>'
                    + '<td class="text-right">' + formatNumber(totalMedia) + '</td>'
                    + '<td class="text-right"><button type="button" class="btn btn-sm btn-outline-danger" data-action="remove-line" data-index="' + index + '"><i class="fas fa-trash"></i></button></td>'
                    + '</tr>';
            } else {
                var totalCorte = toNumber(linea.totalLinea || (linea.cantKgs * linea.precioKg));
                html += '<tr data-index="' + index + '">'
                    + '<td class="text-center font-weight-bold">' + numeroLinea + '</td>'
                    + '<td><strong>' + escapeHtml(linea.corteNombre || '') + '</strong><br><small class="text-muted">Código: ' + escapeHtml(linea.codigo || '') + '</small></td>'
                    + '<td class="text-right">' + formatNumber(linea.cantKgs) + '</td>'
                    + '<td class="text-right">' + formatNumber(linea.precioKg) + '</td>'
                    + '<td class="text-right">' + (toNumber(linea.precioVenta) > 0 ? formatNumber(linea.precioVenta) : '-') + '</td>'
                    + '<td class="text-right">' + (toNumber(linea.margen) !== 0 ? formatNumber(linea.margen) : '-') + '</td>'
                    + '<td class="text-right">' + formatNumber(totalCorte) + '</td>'
                    + '<td class="text-right"><button type="button" class="btn btn-sm btn-outline-danger" data-action="remove-line" data-index="' + index + '"><i class="fas fa-trash"></i></button></td>'
                    + '</tr>';
            }
        }

        $tbody.html(html);
        recalculate($form);
    }

    function rebuildHiddenInputs($form) {
        var state = getState($form);
        var $container = $form.find('#lineasHiddenContainer');
        var html = '';

        $.each(state.lineas, function (index, linea) {
            html += '<input type="hidden" name="Lineas[' + index + '].TipoLinea" value="' + escapeHtml(linea.tipoLinea) + '"/>';
            if (linea.tipoLinea === 'MediaRes') {
                html += '<input type="hidden" name="Lineas[' + index + '].NroTropa" value="' + escapeHtml(linea.nroTropa || '') + '"/>';
                html += '<input type="hidden" name="Lineas[' + index + '].KgMedia" value="' + escapeHtml(formatDecimalForPost(linea.kgMedia)) + '"/>';
                html += '<input type="hidden" name="Lineas[' + index + '].PrecioMedia" value="' + escapeHtml(formatDecimalForPost(linea.precioMedia)) + '"/>';
            } else {
                html += '<input type="hidden" name="Lineas[' + index + '].IdCorte" value="' + escapeHtml(linea.idCorte || 0) + '"/>';
                html += '<input type="hidden" name="Lineas[' + index + '].Codigo" value="' + escapeHtml(linea.codigo || '') + '"/>';
                html += '<input type="hidden" name="Lineas[' + index + '].CorteNombre" value="' + escapeHtml(linea.corteNombre || '') + '"/>';
                html += '<input type="hidden" name="Lineas[' + index + '].CantKgs" value="' + escapeHtml(formatDecimalForPost(linea.cantKgs)) + '"/>';
                html += '<input type="hidden" name="Lineas[' + index + '].PrecioKg" value="' + escapeHtml(formatDecimalForPost(linea.precioKg)) + '"/>';
                html += '<input type="hidden" name="Lineas[' + index + '].PrecioVenta" value="' + escapeHtml(formatDecimalForPost(linea.precioVenta)) + '"/>';
                html += '<input type="hidden" name="Lineas[' + index + '].ActualizarPrecioVenta" value="' + (linea.actualizarPrecioVenta ? 'true' : 'false') + '"/>';
                html += '<input type="hidden" name="Lineas[' + index + '].Margen" value="' + escapeHtml(formatDecimalForPost(linea.margen)) + '"/>';
                html += '<input type="hidden" name="Lineas[' + index + '].DescRecargo" value="' + escapeHtml(formatDecimalForPost(linea.descRecargo)) + '"/>';
                html += '<input type="hidden" name="Lineas[' + index + '].IvaCompra" value="' + escapeHtml(formatDecimalForPost(linea.ivaCompra)) + '"/>';
                html += '<input type="hidden" name="Lineas[' + index + '].Balanza" value="false"/>';
                html += '<input type="hidden" name="Lineas[' + index + '].TotalLinea" value="' + escapeHtml(formatDecimalForPost(linea.totalLinea)) + '"/>';
            }
        });

        $container.html(html);
    }

    function readDraft($form) {
        var key = $form.find('#DraftKey').val();
        if (!key || !window.localStorage) return null;
        try {
            var raw = window.localStorage.getItem(key);
            return raw ? JSON.parse(raw) : null;
        } catch (err) {
            return null;
        }
    }

    function clearDraft($form) {
        var key = $form.find('#DraftKey').val();
        if (!key || !window.localStorage) return;
        window.localStorage.removeItem(key);
        $form.closest('.compra-page').find('#compraDraftBanner').addClass('d-none');
    }

    function buildDraft($form) {
        var state = getState($form);
        return {
            tipoCompra: $form.find('#TipoCompra').val(),
            idSucursal: $form.find('#IdSucursal').val(),
            fechaCompra: $form.find('#FechaCompra').val(),
            idProveedor: $form.find('#idPersona').val(),
            proveedorNombre: $form.find('#razonSocial').val(),
            proveedorCuit: ($form.find('#lblProveedorCuit').text() || '').replace(/^CUIT:\s*/i, ''),
            enCtaCte: $form.find('#EnCtaCte').is(':checked'),
            nroRemito: $form.find('#NroRemito').val(),
            observaciones: $form.find('#Observaciones').val(),
            cantMedias: getCantMedias($form),
            lineas: state.lineas
        };
    }

    function saveDraft($form) {
        var state = getState($form);
        var key = $form.find('#DraftKey').val();
        if (!key || !window.localStorage || state.saving) return;
        try {
            window.localStorage.setItem(key, JSON.stringify(buildDraft($form)));
        } catch (err) {
        }
    }

    function scheduleDraft($form) {
        var state = getState($form);
        window.clearTimeout(state.draftTimer);
        state.draftTimer = window.setTimeout(function () {
            saveDraft($form);
        }, 250);
    }

    function applyDraft($form, draft) {
        var state = getState($form);
        if (!draft) return;

        if (permiteMediaRes($form) && draft.tipoCompra && !(state.config.desdePos && state.config.esEdicion !== true)) {
            $form.find('#TipoCompra').val(draft.tipoCompra);
        }

        if ($form.find('#IdSucursal').is('select')) {
            $form.find('#IdSucursal').val(draft.idSucursal || $form.find('#IdSucursal').val());
        }

        $form.find('#FechaCompra').val(draft.fechaCompra || $form.find('#FechaCompra').val());
        setProveedor($form, {
            id: draft.idProveedor || 0,
            razon: draft.proveedorNombre || '',
            cuit: draft.proveedorCuit || ''
        });
        $form.find('#EnCtaCte').prop('checked', draft.enCtaCte === true);
        $form.find('#NroRemito').val(draft.nroRemito || '');
        $form.find('#Observaciones').val(draft.observaciones || '');
        setCantMedias($form, draft.cantMedias || '');

        state.lineas = $.isArray(draft.lineas) ? draft.lineas : [];
        syncTipoPanels($form);
        renderLineas($form);
        rebuildHiddenInputs($form);
        scheduleDraft($form);
    }

    function syncTipoPanels($form) {
        var mediaRes = permiteMediaRes($form) && esMediaRes($form);
        $form.find('#panelLineaMediaRes').toggle(mediaRes);
        $form.find('#panelLineaCorte').toggle(!mediaRes);
        $form.find('.js-mediares-only').toggle(mediaRes);
        setCantMedias($form, getCantMedias($form));
        recalculate($form);
    }

    function cargarProveedores($form, filtro) {
        var state = getState($form);
        $.get(state.config.urls.personaListar, { filtro: filtro || '' })
            .done(function (items) {
                var html = '';
                $.each(items || [], function (_, item) {
                    html += '<tr class="fila-persona" data-id="' + item.idPersona + '" data-razon="' + escapeHtml(item.razonSocial) + '" data-cuit="' + escapeHtml(item.cuit || '') + '">'
                        + '<td>' + escapeHtml(item.cuit || '') + '</td>'
                        + '<td>' + escapeHtml(item.razonSocial || '') + '</td>'
                        + '<td>' + escapeHtml(item.identificacion || '') + '</td>'
                        + '</tr>';
                });
                $('#tablaPersonas').html(html);
                $('#tablaPersonas tr.fila-persona:first').addClass('is-selected');
            });
    }

    function mostrarProveedorModal($form) {
        window.ModalRequestLoading && window.ModalRequestLoading.show('Cargando solicitud...');
        $('#modalBuscarPersona').modal('show');
        $('#filtroPersona').val('');
        cargarProveedores($form, '');
    }

    function abrirProveedorModal($form) {
        var state = getState($form);
        if ($('#modalBuscarPersona').length) {
            mostrarProveedorModal($form);
            return;
        }

        if (state.loadingPersonaModal) return;
        state.loadingPersonaModal = true;
        window.ModalRequestLoading && window.ModalRequestLoading.show('Cargando solicitud...');

        $.get(state.config.urls.personaBuscarModal)
            .done(function (html) {
                $('#contenedorModalPersonaCompra').html(html);
                mostrarProveedorModal($form);
            })
            .fail(function () {
                window.ModalRequestLoading && window.ModalRequestLoading.hide();
                alert('No se pudo cargar el buscador de proveedores.');
            })
            .always(function () {
                state.loadingPersonaModal = false;
            });
    }

    function cargarProductosModal($form, filtro) {
        var state = getState($form);
        var $modal = $(state.config.modalProductoSelector);
        var apiUrl = $modal.data('api-url');
        if (!$modal.length || !apiUrl) return;

        $.get(apiUrl, { q: filtro || '' })
            .done(function (items) {
                var mostrarPrecio = String($modal.data('mostrar-precio') || 'true').toLowerCase() === 'true';
                var html = '';
                $.each(items || [], function (_, item) {
                    html += '<tr class="js-buscar-producto-row" data-id="' + item.id + '" data-codigo="' + escapeHtml(item.codigo) + '" data-nombre="' + escapeHtml(item.nombre) + '" data-precio="' + escapeHtml(item.precio) + '">'
                        + '<td>' + escapeHtml(item.codigo) + '</td>'
                        + '<td>' + escapeHtml(item.nombre) + '</td>'
                        + '<td class="text-right js-col-precio-cell"' + (mostrarPrecio ? '' : ' style="display:none;"') + '>' + formatNumber(item.precio) + '</td>'
                        + '</tr>';
                });
                $modal.find('.js-col-precio-header, .js-col-precio-cell').toggle(mostrarPrecio);
                $modal.find('.js-buscar-producto-tbody').html(html);
                $modal.find('.js-buscar-producto-row:first').addClass('is-selected');
            });
    }

    function seleccionarProductoDesdeModal($form, $row) {
        if (!$row || !$row.length) return;
        setProductoActual($form, {
            id: $row.data('id'),
            codigo: $row.data('codigo'),
            nombre: $row.data('nombre'),
            precio: $row.data('precio')
        });
        $(getState($form).config.modalProductoSelector).modal('hide');
        $form.find('#txtCantKgs').focus();
    }

    function abrirProductoModal($form) {
        var state = getState($form);
        var $modal = $(state.config.modalProductoSelector);
        if (!$modal.length) {
            alert('No está disponible la búsqueda de productos.');
            return;
        }

        window.ModalRequestLoading && window.ModalRequestLoading.show('Cargando solicitud...');
        $modal.modal('show');
        $modal.find('.js-buscar-producto-input').val('');
        cargarProductosModal($form, '');
    }

    function buscarProductoPorCodigo($form, codigo, enfocarCantidad) {
        var state = getState($form);
        if (!codigo) {
            clearProductoInputs($form);
            return;
        }

        $.get(state.config.urls.buscarCortePorCodigo, { codigo: codigo })
            .done(function (res) {
                if (!res || res.ok !== true) {
                    clearProductoInputs($form, true);
                    return;
                }
                setProductoActual($form, res);
                if (enfocarCantidad) {
                    $form.find('#txtCantKgs').focus();
                }
            })
            .fail(function () {
                clearProductoInputs($form, true);
            });
    }

    function addLineaCorte($form) {
        var state = getState($form);
        var adjustedPrice = getAdjustedUnitPrice($form);
        var precioVentaData = resolvePrecioVentaUpdate($form);
        var margen = toNumber($form.find('#txtMargen').val());
        var linea = {
            tipoLinea: 'Corte',
            idCorte: parseInt($form.find('#txtProductoId').val(), 10) || 0,
            codigo: $form.find('#txtCodigoProducto').val(),
            corteNombre: $form.find('#txtProductoNombre').val(),
            cantKgs: toNumber($form.find('#txtCantKgs').val()),
            precioKg: adjustedPrice,
            precioVenta: precioVentaData.precioVenta,
            actualizarPrecioVenta: precioVentaData.actualizar,
            margen: margen,
            descRecargo: getDescRecargoValue($form),
            ivaCompra: getIvaValue($form),
            balanza: false,
            totalLinea: getCurrentSubtotal($form)
        };

        if (!linea.idCorte || !linea.corteNombre) {
            alert('Seleccione un producto válido.');
            return;
        }
        if (linea.cantKgs <= 0) {
            alert('Ingrese una cantidad mayor a cero.');
            return;
        }
        if (adjustedPrice <= 0) {
            alert('Ingrese un precio mayor a cero.');
            return;
        }
        if (linea.margen < 0) {
            alert('El margen no puede ser negativo.');
            return;
        }

        state.lineas.push(linea);
        renderLineas($form);
        rebuildHiddenInputs($form);
        showUltimoAgregado($form, 'Agregado correctamente: ' + linea.corteNombre + ' | Cantidad ' + formatNumber(linea.cantKgs) + ' | Precio ' + formatNumber(linea.precioKg), '#alertUltimoAgregadoProducto');
        scheduleDraft($form);
        if (isContinuousProductMode($form)) {
            clearProductoCantidadOnly($form);
            $form.find('#txtCantKgs').focus().select();
        } else {
            clearProductoInputs($form);
            $form.find('#txtCodigoProducto').focus();
        }
    }

    function addLineaMediaRes($form) {
        var state = getState($form);
        var linea = {
            tipoLinea: 'MediaRes',
            nroTropa: '',
            kgMedia: toNumber($form.find('#txtKgMedia').val()),
            precioMedia: toNumber($form.find('#txtPrecioMedia').val())
        };

        if (linea.kgMedia <= 0) {
            alert('Ingrese kilos mayores a cero.');
            return;
        }
        if (linea.precioMedia <= 0) {
            alert('Ingrese un precio mayor a cero.');
            return;
        }

        state.lineas.push(linea);
        renderLineas($form);
        rebuildHiddenInputs($form);
        showUltimoAgregado($form, 'Agregado correctamente: Media | Kg ' + formatNumber(linea.kgMedia) + ' | Precio ' + formatNumber(linea.precioMedia), '#alertUltimoAgregadoMedia');
        scheduleDraft($form);
        if (isContinuousMediaMode($form)) {
            clearMediaResCantidadOnly($form);
            syncContinuousMediaState($form);
            $form.find('#txtKgMedia').focus().select();
        } else {
            clearMediaResInputs($form);
            $form.find('#txtKgMedia').focus();
        }
    }

    function focusNextAfterPrice($form) {
        if ($form.find('#chkAplicarDescRecargo').is(':checked')) {
            $form.find('#txtDescRecargo').focus().select();
            return;
        }
        if ($form.find('#chkAplicarIva').is(':checked')) {
            $form.find('#txtIvaCompra').focus().select();
            return;
        }
        if ($form.find('#chkAplicarPrecioVentaMargen').is(':checked')) {
            if ($form.find('#optMargen').is(':checked')) {
                $form.find('#txtMargen').focus().select();
            } else {
                $form.find('#txtPrecioVenta').focus().select();
            }
            return;
        }
        if (isContinuousProductMode($form)) {
            $form.find('#txtCantKgs').focus().select();
            return;
        }
        $form.find('#btnAgregarLineaCorte').focus();
    }

    function focusNextAfterIva($form) {
        if ($form.find('#chkAplicarPrecioVentaMargen').is(':checked')) {
            if ($form.find('#optMargen').is(':checked')) {
                $form.find('#txtMargen').focus().select();
            } else {
                $form.find('#txtPrecioVenta').focus().select();
            }
            return;
        }
        if (isContinuousProductMode($form)) {
            $form.find('#txtCantKgs').focus().select();
            return;
        }
        $form.find('#btnAgregarLineaCorte').focus();
    }

    function submitForm($form) {
        var state = getState($form);
        if (state.saving) return;

        rebuildHiddenInputs($form);
        state.saving = true;
        $form.find('#btnGuardarCompra').prop('disabled', true);

        $.ajax({
            url: $form.attr('action'),
            type: 'POST',
            data: $form.serialize(),
            success: function (res) {
                if (!res || res.ok !== true) {
                    alert(res && res.mensaje ? res.mensaje : 'No se pudo guardar la compra.');
                    return;
                }

                clearDraft($form);

                if (state.config.desdePos) {
                    if (window.Swal) {
                        Swal.fire({ icon: 'success', title: 'Compra registrada', text: res.mensaje || 'La compra se guardó correctamente.', timer: 1800, showConfirmButton: false });
                    }
                    $('#modalFinanzasPOS').modal('hide');
                    return;
                }

                if (res.redirectUrl) {
                    window.location.href = res.redirectUrl;
                    return;
                }

                if (window.Swal) {
                    Swal.fire({ icon: 'success', title: 'Compra guardada', text: res.mensaje || 'La compra se guardó correctamente.' });
                } else {
                    alert(res.mensaje || 'La compra se guardó correctamente.');
                }
            },
            error: function () {
                alert('No se pudo guardar la compra.');
            },
            complete: function () {
                state.saving = false;
                $form.find('#btnGuardarCompra').prop('disabled', false);
            }
        });
    }

    function bindEvents($form) {
        var state = getState($form);
        var $page = $form.closest('.compra-page');

        $form.off('.compras');
        $page.off('.compras');

        var modalProductoSelector = state.config.modalProductoSelector || '#modalBuscarProducto';

        $form.on('change.compras', '#TipoCompra', function () {
            syncTipoPanels($form);
            state.lineas = [];
            renderLineas($form);
            rebuildHiddenInputs($form);
            scheduleDraft($form);
        });

        $form.on('input.compras change.compras', '.js-cant-medias-visible', function () {
            setCantMedias($form, $(this).val());
            scheduleDraft($form);
        });

        $form.on('input.compras change.compras', '#txtCantKgs, #txtPrecioKg, #txtDescRecargo, #txtIvaCompra, #txtMargen, #txtPrecioVenta, #chkAplicarDescRecargo, #chkAplicarIva, #chkAplicarPrecioVentaMargen, #optMargen, #optPrecioVenta', function () {
            syncPriceHelpers($form, this.id === 'txtPrecioVenta' || this.id === 'txtMargen' ? 'manual' : 'change');
        });

        $form.on('input.compras change.compras', '#txtPrecioMedia, #chkCargaContinuaMedia', function () {
            syncContinuousMediaState($form);
        });

        $form.on('change.compras', '#chkCargaContinuaProducto', function () {
            syncContinuousProductState($form);
        });

        $form.on('click.compras', '#btnBuscarProveedor', function () {
            abrirProveedorModal($form);
        });

        $form.on('click.compras', '#btnLimpiarProveedor', function () {
            setProveedor($form, null);
            scheduleDraft($form);
        });

        $(document)
            .off('shown.bs.modal.comprasPersona', '#modalBuscarPersona')
            .on('shown.bs.modal.comprasPersona', '#modalBuscarPersona', function () {
                $('#filtroPersona').focus().select();
            });

        $(document)
            .off('hidden.bs.modal.comprasPersona', '#modalBuscarPersona')
            .on('hidden.bs.modal.comprasPersona', '#modalBuscarPersona', function () {
                $form.find('#NroRemito').focus().select();
            });

        $(document)
            .off('input.comprasPersona', '#filtroPersona')
            .on('input.comprasPersona', '#filtroPersona', function () {
                window.clearTimeout(state.personaTimer);
                state.personaTimer = window.setTimeout(function () {
                    cargarProveedores($form, $('#filtroPersona').val());
                }, 200);
            });

        $(document)
            .off('click.comprasPersona', '#tablaPersonas tr.fila-persona')
            .on('click.comprasPersona', '#tablaPersonas tr.fila-persona', function () {
                $('#tablaPersonas tr.fila-persona').removeClass('is-selected');
                $(this).addClass('is-selected');
            });

        $(document)
            .off('dblclick.comprasPersona', '#tablaPersonas tr.fila-persona')
            .on('dblclick.comprasPersona', '#tablaPersonas tr.fila-persona', function () {
                var $row = $(this);
                setProveedor($form, {
                    id: $row.data('id'),
                    razon: $row.data('razon'),
                    cuit: $row.data('cuit')
                });
                $('#modalBuscarPersona').modal('hide');
                scheduleDraft($form);
            });

        $(document)
            .off('keydown.comprasPersona', '#filtroPersona')
            .on('keydown.comprasPersona', '#filtroPersona', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    var $target = $('#tablaPersonas tr.fila-persona.is-selected').first();
                    if (!$target.length) $target = $('#tablaPersonas tr.fila-persona').first();
                    if ($target.length) $target.trigger('dblclick');
                }
            });

        $(document)
            .off('shown.bs.modal.comprasProducto', modalProductoSelector)
            .on('shown.bs.modal.comprasProducto', modalProductoSelector, function () {
                $(this).find('.js-buscar-producto-input').focus().select();
            });

        $form.on('click.compras', '#btnBuscarProducto', function () {
            abrirProductoModal($form);
        });

        $form.on('input.compras', '#txtCodigoProducto', function () {
            var codigo = $.trim($form.find('#txtCodigoProducto').val() || '');
            window.clearTimeout(state.productoTimer);
            if (!codigo) {
                clearProductoInputs($form);
                return;
            }
            state.productoTimer = window.setTimeout(function () {
                buscarProductoPorCodigo($form, codigo, false);
            }, 250);
        });

        $form.on('keydown.compras', '#txtCodigoProducto', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            var codigo = $.trim($form.find('#txtCodigoProducto').val() || '');
            if (!codigo) return;
            buscarProductoPorCodigo($form, codigo, true);
        });

        $form.on('keydown.compras', '#txtCantKgs', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            if (isContinuousProductMode($form)) {
                addLineaCorte($form);
                return;
            }
            $form.find('#txtPrecioKg').focus().select();
        });

        $form.on('keydown.compras', '#txtPrecioKg', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            focusNextAfterPrice($form);
        });

        $form.on('keydown.compras', '#txtDescRecargo', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            if ($form.find('#chkAplicarIva').is(':checked')) {
                $form.find('#txtIvaCompra').focus().select();
                return;
            }
            if ($form.find('#chkAplicarPrecioVentaMargen').is(':checked')) {
                if ($form.find('#optMargen').is(':checked')) {
                    $form.find('#txtMargen').focus().select();
                } else {
                    $form.find('#txtPrecioVenta').focus().select();
                }
                return;
            }
            $form.find('#btnAgregarLineaCorte').focus();
        });

        $form.on('keydown.compras', '#txtIvaCompra', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            focusNextAfterIva($form);
        });

        $form.on('keydown.compras', '#txtMargen, #txtPrecioVenta', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            $form.find('#btnAgregarLineaCorte').focus();
        });

        $form.on('keydown.compras', '#txtKgMedia', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            if (isContinuousMediaMode($form)) {
                addLineaMediaRes($form);
                return;
            }
            $form.find('#txtPrecioMedia').focus().select();
        });

        $form.on('keydown.compras', '#txtPrecioMedia', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            if (isContinuousMediaMode($form)) {
                $form.find('#txtKgMedia').focus().select();
                return;
            }
            $form.find('#btnAgregarLineaMediaRes').focus();
        });

        $(document)
            .off('input.comprasProductoModal', state.config.modalProductoSelector + ' .js-buscar-producto-input')
            .on('input.comprasProductoModal', state.config.modalProductoSelector + ' .js-buscar-producto-input', function () {
                var filtro = $(this).val() || '';
                window.clearTimeout(state.productoTimer);
                state.productoTimer = window.setTimeout(function () {
                    cargarProductosModal($form, filtro);
                }, 200);
            });

        $(document)
            .off('click.comprasProductoModal', state.config.modalProductoSelector + ' .js-buscar-producto-row')
            .on('click.comprasProductoModal', state.config.modalProductoSelector + ' .js-buscar-producto-row', function () {
                $(state.config.modalProductoSelector).find('.js-buscar-producto-row').removeClass('is-selected');
                $(this).addClass('is-selected');
            });

        $(document)
            .off('dblclick.comprasProductoModal', state.config.modalProductoSelector + ' .js-buscar-producto-row')
            .on('dblclick.comprasProductoModal', state.config.modalProductoSelector + ' .js-buscar-producto-row', function () {
                seleccionarProductoDesdeModal($form, $(this));
            });

        $(document)
            .off('keydown.comprasProductoModal', state.config.modalProductoSelector + ' .js-buscar-producto-input')
            .on('keydown.comprasProductoModal', state.config.modalProductoSelector + ' .js-buscar-producto-input', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    var $modal = $(state.config.modalProductoSelector);
                    var $target = $modal.find('.js-buscar-producto-row.is-selected').first();
                    if (!$target.length) $target = $modal.find('.js-buscar-producto-row').first();
                    seleccionarProductoDesdeModal($form, $target);
                }
            });

        $form.on('click.compras', '#btnAgregarLineaCorte', function () {
            addLineaCorte($form);
        });

        $form.on('keydown.compras', '#btnAgregarLineaCorte', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            addLineaCorte($form);
        });

        $form.on('click.compras', '#btnAgregarLineaMediaRes', function () {
            addLineaMediaRes($form);
        });

        $form.on('keydown.compras', '#btnAgregarLineaMediaRes', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            addLineaMediaRes($form);
        });

        $form.on('click.compras', '[data-action="remove-line"]', function () {
            var index = parseInt($(this).data('index'), 10);
            if (isNaN(index)) return;
            state.lineas.splice(index, 1);
            renderLineas($form);
            rebuildHiddenInputs($form);
            scheduleDraft($form);
        });

        $form.on('click.compras', '[data-action="toggle-line-order"]', function () {
            state.lineasSortAsc = !state.lineasSortAsc;
            renderLineas($form);
        });

        $form.on('submit.compras', function (e) {
            e.preventDefault();
            submitForm($form);
        });

        $form.on('input.compras change.compras', 'input, select, textarea', function () {
            scheduleDraft($form);
        });

        $form.on('click.compras', '#btnLimpiarBorrador', function () {
            if (!confirm('Se eliminará el borrador local de esta compra. ¿Continuar?')) return;
            clearDraft($form);
        });

        $page.on('click.compras', '[data-action="restore-draft"]', function () {
            var draft = readDraft($form);
            if (!draft) return;
            applyDraft($form, draft);
            $page.find('#compraDraftBanner').addClass('d-none');
        });

        $page.on('click.compras', '[data-action="clear-draft"]', function () {
            clearDraft($form);
        });

        $(document)
            .off('keydown.comprasHotkeys')
            .on('keydown.comprasHotkeys', function (e) {
                if (!$form.length || !$form.is(':visible')) return;
                if (e.key === 'F9') {
                    e.preventDefault();
                    e.stopPropagation();
                    abrirProveedorModal($form);
                    return;
                }
                if (e.key === 'F10') {
                    e.preventDefault();
                    e.stopPropagation();
                    abrirProductoModal($form);
                    return;
                }

                if (e.altKey && !e.ctrlKey && !e.metaKey && !e.shiftKey && String(e.key || '').toLowerCase() === 'c') {
                    var $cancel = $form.find('#btnCancelarCompra');
                    if (!$cancel.length || !$cancel.is(':visible')) return;

                    e.preventDefault();
                    e.stopPropagation();
                    $cancel[0].click();
                }
            });
    }

    function normalizeLinea(linea) {
        return {
            tipoLinea: linea.TipoLinea || linea.tipoLinea || (linea.EsMediaRes ? 'MediaRes' : 'Corte'),
            idCorte: linea.IdCorte || linea.idCorte || 0,
            codigo: linea.Codigo || linea.codigo || '',
            corteNombre: linea.CorteNombre || linea.corteNombre || '',
            cantKgs: linea.CantKgs || linea.cantKgs || 0,
            precioKg: linea.PrecioKg || linea.precioKg || 0,
            precioVenta: linea.PrecioVenta || linea.precioVenta || 0,
            actualizarPrecioVenta: linea.ActualizarPrecioVenta === true || linea.actualizarPrecioVenta === true,
            margen: linea.Margen || linea.margen || 0,
            descRecargo: linea.DescRecargo || linea.descRecargo || 0,
            ivaCompra: linea.IvaCompra || linea.ivaCompra || 0,
            balanza: false,
            nroTropa: linea.NroTropa || linea.nroTropa || '',
            kgMedia: linea.KgMedia || linea.kgMedia || 0,
            precioMedia: linea.PrecioMedia || linea.precioMedia || 0,
            totalLinea: linea.TotalLinea || linea.totalLinea || ((linea.CantKgs || linea.cantKgs || 0) * (linea.PrecioKg || linea.precioKg || 0))
        };
    }

    function initState($form, config) {
        var state = buildState(config);
        state.lineas = $.isArray(config.initialLines) ? $.map(config.initialLines, function (linea) { return normalizeLinea(linea); }) : [];
        setState($form, state);
        syncTipoPanels($form);
        syncPriceHelpers($form, 'init');
        syncContinuousProductState($form);
        syncContinuousMediaState($form);
        renderLineas($form);
        rebuildHiddenInputs($form);

        var draft = readDraft($form);
        if (draft) {
            $form.closest('.compra-page').find('#compraDraftBanner').removeClass('d-none');
        }
    }

    window.ComprasUI = window.ComprasUI || {
        init: function (config) {
            var $form = $(config && config.formSelector ? config.formSelector : '#formCompra');
            if (!$form.length) return;
            initState($form, config || {});
            bindEvents($form);
        }
    };
})(window, window.jQuery);
