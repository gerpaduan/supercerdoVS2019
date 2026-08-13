(function (window, $) {
    'use strict';

    if (!window || !$) return;

    function parseNumber(value) {
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

        var num = parseFloat(normalized);
        return { ok: !isNaN(num), value: isNaN(num) ? 0 : num };
    }

    function toNumber(value) {
        return parseNumber(value).value;
    }

    function escapeHtml(value) {
        return String(value == null ? '' : value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function formatNumber(value, decimals) {
        var places = typeof decimals === 'number' ? decimals : 3;
        return toNumber(value).toFixed(places);
    }

    function formatNumberForPost(value) {
        var parsed = parseNumber(value);
        if (!parsed.ok) return '';
        return String(parsed.value).replace('.', ',');
    }

    function buildState(config) {
        return {
            config: config || {},
            lineas: [],
            noCargados: [],
            draftTimer: null,
            filtrosComprasPesajeTimer: null,
            filtrosPesajesTimer: null,
            productoTimer: null,
            productoRequestSeq: 0,
            comprasPesajeDetalle: {},
            loadingDetalleCompraPesaje: {},
            loadingPersonaModal: false,
            loadingNoCargados: false,
            loadingComprasPesaje: false,
            loadingPesajesVinculables: false,
            loadingPorcentajes: false,
            generandoAjuste: false,
            balanzaDisponible: false,
            balanzaFaltanteDetectada: false,
            balanzaDesactivadaManual: false,
            balanzaPolling: null,
            balanzaClientStarted: false,
            balanzaUltimaLectura: null,
            focusAgregarPendiente: false,
            sortKey: 'creado',
            sortDirection: 'asc',
            searchText: ''
        };
    }

    function getState($form) {
        return $form.data('__stockState');
    }

    function setState($form, state) {
        $form.data('__stockState', state);
    }

    function showFeedback($form, text) {
        var $feedback = $form.find('#stockFeedback');
        $feedback.text(text).removeClass('d-none');
        window.setTimeout(function () {
            $feedback.addClass('d-none').text('');
        }, 2200);
    }

    function focusCantidadManual($form) {
        window.setTimeout(function () {
            var $input = $form.find('#txtCantKgs');
            if (!$input.prop('readonly')) {
                $input.focus().select();
            }
        }, 20);
    }

    function focusAgregarLinea($form) {
        window.setTimeout(function () {
            $form.find('#btnAgregarLineaStock').focus();
        }, 20);
    }

    //function focusCodigo($form) {
    //    window.setTimeout(function () {
    //        var $input = $form.find('#txtCodigoProducto');
    //        if (!$input.length) return;
    //        $input.focus();
    //        var input = $input.get(0);
    //        if (input && typeof input.setSelectionRange === 'function') {
    //            var end = ($input.val() || '').toString().length;
    //            input.setSelectionRange(end, end);
    //        }
    //    }, 20);
    //}


    function supportsSelection(input) {
        if (!input) return false;
        // algunos navegadores exponen setSelectionRange pero no lo permiten en type="number"
        return typeof input.setSelectionRange === 'function' && input.type !== 'number';
    }

    function focusCodigo($form) {
        window.setTimeout(function () {
            var $input = $form.find('#txtCodigoProducto');
            if (!$input.length) return;
            $input.focus();
            var input = $input.get(0);
            if (supportsSelection(input)) {
                var end = ($input.val() || '').toString().length;
                try {
                    input.setSelectionRange(end, end);
                } catch (e) {
                    // protegemos contra navegadores/implementaciones inesperadas
                }
            }
        }, 20);
    }


    function autoResizeObservaciones($form) {
        var $observaciones = $form.find('#Observaciones');
        if (!$observaciones.length) return;
        $observaciones.css('height', 'auto');
        $observaciones.css('height', $observaciones[0].scrollHeight + 'px');
    }

    function showWarning($form, text) {
        var $warning = $form.find('#stockWarning');
        $warning.text(text).removeClass('d-none');
        window.setTimeout(function () {
            $warning.addClass('d-none').text('');
        }, 1600);
    }

    function clearWarning($form) {
        $form.find('#stockWarning').addClass('d-none').text('');
    }

    function showNoCargadosWarning($form, text) {
        var $warning = $('#stockNoCargadosWarning');
        $warning.text(text).removeClass('d-none');
    }

    function clearNoCargadosWarning() {
        $('#stockNoCargadosWarning').addClass('d-none').text('');
    }

    function getNoCargadosModal() {
        return $('#modalProductosNoCargadosStock');
    }

    function getPorcentajesModal() {
        return $('#modalPorcentajesPesajeStock');
    }

    function getComprasPesajeModal() {
        return $('#modalSeleccionCompraPesajeStock');
    }

    function getVincularPesajesModal() {
        return $('#modalVincularPesajesStock');
    }

    function showPorcentajeWarning(text) {
        $('#stockPorcentajeWarning').text(text).removeClass('d-none');
    }

    function clearPorcentajeWarning() {
        $('#stockPorcentajeWarning').addClass('d-none').text('');
    }

    function showComprasPesajeWarning(text) {
        $('#stockComprasPesajeWarning').text(text).removeClass('d-none');
    }

    function clearComprasPesajeWarning() {
        $('#stockComprasPesajeWarning').addClass('d-none').text('');
    }

    function showVincularPesajesWarning(text) {
        $('#stockVincularPesajesWarning').text(text).removeClass('d-none');
    }

    function clearVincularPesajesWarning() {
        $('#stockVincularPesajesWarning').addClass('d-none').text('');
    }

    function getNoCargadosSeleccionados() {
        var seleccionados = getNoCargadosModal().data('selectedNoCargados');
        return seleccionados && typeof seleccionados === 'object' ? seleccionados : {};
    }

    function setNoCargadosSeleccionados(seleccionados) {
        getNoCargadosModal().data('selectedNoCargados', seleccionados || {});
    }

    function syncSeleccionNoCargadosDesdeTabla() {
        var seleccionados = getNoCargadosSeleccionados();
        getNoCargadosModal().find('.js-no-cargado-check').each(function () {
            var itemId = parseInt($(this).data('id'), 10) || 0;
            if (itemId <= 0) return;

            if ($(this).is(':checked')) {
                seleccionados[itemId] = true;
            } else {
                delete seleccionados[itemId];
            }
        });
        setNoCargadosSeleccionados(seleccionados);
    }

    function showNoCargadosInfo(text) {
        $('#stockNoCargadosInfo').text(text).removeClass('d-none');
    }

    function clearNoCargadosInfo() {
        $('#stockNoCargadosInfo').addClass('d-none').text('');
    }

    function syncCantidadReadonly($form) {
        var state = getState($form);
        var hayProducto = !!$.trim($form.find('#txtProductoId').val() || '');
        var soloLecturaVista = $form.hasClass('edit-readonly-active');
        var soloLecturaBalanza = !!(state && state.balanzaDisponible && $form.find('#chkBalanzaLinea').is(':checked') && (!hayProducto || esPesableActual($form)));
        var soloLectura = soloLecturaVista || soloLecturaBalanza;
        $form.find('#txtCantKgs').prop('readonly', soloLectura);
    }

    function esAccionVisible($button) {
        return !!($button && $button.length && !$button.prop('disabled') && !$button.hasClass('d-none'));
    }

    function getPrimaryActionButton($form) {
        var $modify = $form.find('#btnHabilitarEdicionStock');
        var $save = $form.find('#btnGuardarStock');

        if (esAccionVisible($modify)) return $modify;
        if (esAccionVisible($save)) return $save;
        return $();
    }

    function syncPrimaryAction($form) {
        if (!$form || !$form.length) return;

        var esEdicion = (parseInt($form.find('#IdCompra').val(), 10) || 0) > 0;
        if (!esEdicion) return;

        var $modify = $form.find('#btnHabilitarEdicionStock');
        var $save = $form.find('#btnGuardarStock');
        if (!$modify.length || !$save.length) return;

        var readOnly = $form.hasClass('edit-readonly-active');

        $modify.removeAttr('accesskey');
        $save.removeAttr('accesskey');

        if (readOnly) {
            $modify
                .html('<i class="fas fa-edit mr-1"></i> Modificar')
                .toggleClass('d-none', false)
                .attr('title', 'Modificar (Alt+Enter)');

            $save
                .toggleClass('d-none', true)
                .attr('title', 'Guardar stock (Alt+Enter)');
            return;
        }

        $modify.toggleClass('d-none', true);
        $save
            .toggleClass('d-none', false)
            .attr('title', 'Guardar stock (Alt+Enter)');
    }

    function normalizeLinea(linea) {
        return {
            index: linea.Index || linea.index || 0,
            idCorte: linea.IdCorte || linea.idCorte || 0,
            codigo: linea.Codigo || linea.codigo || '',
            producto: linea.Producto || linea.producto || linea.CorteNombre || linea.corteNombre || '',
            cantKgs: toNumber(linea.CantKgs || linea.cantKgs),
            balanza: linea.Balanza === true || linea.balanza === true,
            creadoTexto: linea.CreadoTexto || linea.creadoTexto || '',
            pesable: linea.Pesable === true || linea.pesable === true,
            noContado: linea.NoContado === true || linea.noContado === true,
            idPesajeVinculado: linea.IdPesajeVinculado || linea.idPesajeVinculado || 0,
            pesajeVinculadoTexto: linea.PesajeVinculadoTexto || linea.pesajeVinculadoTexto || ''
        };
    }

    function setProveedor($form, persona) {
        $form.find('#IdProveedor').val(persona && persona.id ? persona.id : 0);
        $form.find('#razonSocial').val(persona && persona.razon ? persona.razon : '');
        $form.find('#lblProveedorCuit').text(persona && persona.cuit ? ('CUIT: ' + persona.cuit) : 'Sin proveedor seleccionado');
    }

    function clearCompraVinculadaPesaje($form) {
        $form.find('#IdPesajeAjustado').val('');
        $form.find('#stockCompraVinculadaId').text('');
        $form.find('#stockCompraVinculadaProveedor').text('-');
        $form.find('#stockCompraVinculadaFecha').text('-');
        $form.find('#stockCompraVinculadaEstado').addClass('d-none').text('');
        $form.find('#stockCompraVinculadaDatos').removeClass('d-none');
        $form.find('#stockCompraVinculadaBox').addClass('d-none');
    }

    function setCompraVinculadaPesaje($form, compra) {
        var idCompra = compra && compra.idCompra ? parseInt(compra.idCompra, 10) || 0 : 0;
        if (idCompra <= 0) {
            clearCompraVinculadaPesaje($form);
            return;
        }

        $form.find('#IdPesajeAjustado').val(idCompra);
        $form.find('#stockCompraVinculadaId').text('#' + idCompra);
        $form.find('#stockCompraVinculadaProveedor').text(compra.proveedor || '-');
        $form.find('#stockCompraVinculadaFecha').text(compra.fechaCompra || '-');

        var estado = $.trim(compra.estado || '');
        if (estado) {
            $form.find('#stockCompraVinculadaEstado').removeClass('d-none').text(estado);
            $form.find('#stockCompraVinculadaDatos').addClass('d-none');
        } else {
            $form.find('#stockCompraVinculadaEstado').addClass('d-none').text('');
            $form.find('#stockCompraVinculadaDatos').removeClass('d-none');
        }

        $form.find('#stockCompraVinculadaBox').removeClass('d-none');
    }

    function setProductoActual($form, producto) {
        $form.find('#txtProductoId').val(producto && producto.id ? producto.id : '');
        $form.find('#txtCodigoProducto').val(producto && producto.codigo ? producto.codigo : '');
        $form.find('#txtProductoNombre').val(producto && producto.nombre ? producto.nombre : '');
        $form.find('#txtProductoPesable').val(producto && producto.pesable ? 'true' : 'false');
        $form.find('#txtProductoTipo').val(producto && producto.tipo ? producto.tipo : '');
        $form.find('#txtProductoPromedio').val(producto && producto.promedio ? producto.promedio : '');
        actualizarEtiquetaCantidad($form);
    }

    function clearProductoInputs($form, preserveCode) {
        var state = getState($form);
        var codigo = preserveCode ? ($form.find('#txtCodigoProducto').val() || '') : '';
        $form.find('#txtProductoId').val('');
        $form.find('#txtCodigoProducto').val(codigo);
        $form.find('#txtProductoNombre').val('');
        $form.find('#txtProductoPesable').val('');
        $form.find('#txtProductoTipo').val('');
        $form.find('#txtProductoPromedio').val('');
        if (state && state.balanzaDisponible && $form.find('#chkBalanzaLinea').is(':checked') && !state.balanzaDesactivadaManual && state.balanzaUltimaLectura) {
            $form.find('#txtCantKgs').val(state.balanzaUltimaLectura.pesoDisplay || state.balanzaUltimaLectura.pesoTexto || '');
            if (window.CarnisysBalanza) {
                window.CarnisysBalanza.activar();
            }
        } else {
            $form.find('#txtCantKgs').val('');
        }
        actualizarEtiquetaCantidad($form);
        syncCantidadReadonly($form);
    }

    function esPesableActual($form) {
        return String($form.find('#txtProductoPesable').val() || '').toLowerCase() === 'true';
    }

    function actualizarEtiquetaCantidad($form) {
        $form.find('#lblCantidadStock').text('Cantidad');
        syncCantidadReadonly($form);
    }

    function getTipoCompraLabel(tipoCompra) {
        switch (String(tipoCompra || '').toLowerCase()) {
            case 'ingreso stock': return 'Ingreso';
            case 'egreso stock': return 'Egreso';
            case 'cierre stock': return 'Cierre';
            case 'ajuste stock': return 'Ajuste';
            case 'pesaje cortes': return 'Pesaje';
            default: return tipoCompra || '';
        }
    }

    function actualizarContextoNoCargados($form) {
        var sucursal = $form.find('#IdSucursal option:selected').text() || '';
        var fechaRaw = $form.find('#FechaCompra').val() || '';
        var fecha = fechaRaw;

        if (fechaRaw) {
            var dt = new Date(fechaRaw);
            if (!isNaN(dt.getTime())) {
                var dd = String(dt.getDate()).padStart(2, '0');
                var mm = String(dt.getMonth() + 1).padStart(2, '0');
                var yyyy = dt.getFullYear();
                var hh = String(dt.getHours()).padStart(2, '0');
                var mi = String(dt.getMinutes()).padStart(2, '0');
                fecha = dd + '/' + mm + '/' + yyyy + ' ' + hh + ':' + mi;
            }
        }

        $('#lblSucursalNoCargadosContexto').text($.trim(sucursal));
        $('#lblFechaNoCargadosContexto').text($.trim(fecha));
    }

    function syncTipoCompraUi($form) {
        var state = getState($form);
        var tipoCompra = $form.find('#TipoCompra').val() || '';
        var esPesaje = String(tipoCompra).toLowerCase() === 'pesaje cortes';
        var esEgreso = String(tipoCompra).toLowerCase() === 'egreso stock';
        var permiteCantidadNegativa = esEgreso || String(tipoCompra).toLowerCase() === 'ajuste stock';

        state.config.tipoCompra = tipoCompra;
        state.config.esPesaje = esPesaje;
        state.config.esEgreso = esEgreso;
        state.config.permiteCantidadNegativa = permiteCantidadNegativa;

        $form.find('#TipoCompraVisual').val(tipoCompra);
        $form.find('#stockAccionActual').text(tipoCompra);
        $form.find('#bloquePesajeStock').toggleClass('d-none', !esPesaje);
        $form.find('#wrapBtnSeleccionarCompraPesaje').toggleClass('d-none', !esPesaje);
        $form.find('#wrapBtnVincularPesaje').toggleClass('d-none', !esPesaje);
        $form.find('#btnVerPorcentajePesaje').toggleClass('d-none', !esPesaje);
        $form.find('#btnProductosNoCargados').toggleClass('d-none', String(tipoCompra).toLowerCase() !== 'cierre stock');
        actualizarContextoNoCargados($form);
        $form.find('#txtAyudaCantidad').text(permiteCantidadNegativa ? 'Puede ingresar valores positivos o negativos según el ajuste o egreso.' : '');

        if (String(tipoCompra).toLowerCase() !== 'cierre stock') {
            $('#modalProductosNoCargadosStock').modal('hide');
        }

        if (!esPesaje) {
            getPorcentajesModal().modal('hide');
            getVincularPesajesModal().modal('hide');
            actualizarEstadoAjustePesaje($form, '');
        } else {
            actualizarEstadoAjustePesaje($form, $.trim($('#stockEstadoAjusteTexto').text() || ''));
        }

        if (!esPesaje) {
            setProveedor($form, {
                id: state.config.proveedorDefaultId || 0,
                razon: state.config.proveedorDefaultNombre || '',
                cuit: state.config.proveedorDefaultCuit || ''
            });
            $form.find('#CantMedias').val('');
            $form.find('#KgsMedias').val('');
        }

        recalculate($form);
        scheduleDraft($form);
    }

    function actualizarEstadoAjustePesaje($form, estado) {
        var texto = $.trim(estado || '');
        var actualizado = texto.toLowerCase() === 'actualizado';
        var $badge = $form.find('#stockEstadoAjusteBadge');
        var $badgeTexto = $form.find('#stockEstadoAjusteTexto');
        var $estadoModal = $('#lblEstadoAjusteModalStock');

        if (!stateEsPesaje($form) || !texto) {
            $badge.addClass('d-none');
        } else {
            $badge.removeClass('d-none')
                .toggleClass('badge-success', actualizado)
                .toggleClass('badge-warning', !actualizado);
        }

        $badgeTexto.text(texto);
        $estadoModal.text(texto || '-')
            .toggleClass('text-success', actualizado)
            .toggleClass('text-danger', !actualizado);
    }

    function stateEsPesaje($form) {
        var state = getState($form);
        return !!(state && state.config && state.config.esPesaje);
    }

    function recalculate($form) {
        var state = getState($form);
        var totalKg = 0;

        $.each(state.lineas, function (_, linea) {
            totalKg += toNumber(linea.cantKgs);
        });

        $form.find('#totalItems').text(state.lineas.length);
        $form.find('#totalKg').text(formatNumber(totalKg, 3));

        if (state.config.esPesaje) {
            var kgsMediasActual = $.trim($form.find('#KgsMedias').val() || '');
            if (!kgsMediasActual) {
                $form.find('#KgsMedias').val(formatNumber(totalKg, 2));
            }
        }
    }

    function rebuildHiddenInputs($form) {
        var state = getState($form);
        var $container = $form.find('#lineasHiddenContainer');
        var html = '';

        $.each(state.lineas, function (index, linea) {
            var cantKgsValue = formatNumberForPost(linea.cantKgs);
            html += '<input type="hidden" name="Lineas[' + index + '].Index" value="' + escapeHtml(index + 1) + '"/>';
            html += '<input type="hidden" name="Lineas[' + index + '].IdCorte" value="' + escapeHtml(linea.idCorte || 0) + '"/>';
            html += '<input type="hidden" name="Lineas[' + index + '].Codigo" value="' + escapeHtml(linea.codigo || '') + '"/>';
            html += '<input type="hidden" name="Lineas[' + index + '].Producto" value="' + escapeHtml(linea.producto || '') + '"/>';
            html += '<input type="hidden" name="Lineas[' + index + '].CantKgs" value="' + cantKgsValue + '"/>';
            html += '<input type="hidden" name="Lineas[' + index + '].Balanza" value="' + (linea.balanza ? 'true' : 'false') + '"/>';
            html += '<input type="hidden" name="Lineas[' + index + '].CreadoTexto" value="' + escapeHtml(linea.creadoTexto || '') + '"/>';
            html += '<input type="hidden" name="Lineas[' + index + '].Pesable" value="' + (linea.pesable ? 'true' : 'false') + '"/>';
            html += '<input type="hidden" name="Lineas[' + index + '].IdPesajeVinculado" value="' + escapeHtml(linea.idPesajeVinculado || 0) + '"/>';
            html += '<input type="hidden" name="Lineas[' + index + '].PesajeVinculadoTexto" value="' + escapeHtml(linea.pesajeVinculadoTexto || '') + '"/>';
        });

        $container.html(html);
        rebuildPesajesVinculadosInputs($form);
        renderPesajesVinculadosInfo($form);
    }

    // Devuelve TODOS los pesajes vinculados a este registro: los que ya estaban (cargados por el
    // servidor en state.config.pesajesVinculadosExistentes, ver StockController.CrearViewModelEdicion)
    // MAS los que se agregan en esta misma sesion de edicion (con id/texto propios, via las lineas
    // que trae cada pesaje recien vinculado). Es clave incluir los existentes aca: SincronizarPesajesVinculados
    // desvincula todo lo que NO este en esta lista al guardar -- si solo mirara las lineas de la sesion
    // actual (que arrancan vacias en cada carga de pagina, ver linea 1490 de StockController.cs),
    // cualquier guardado no relacionado desvinculaba en silencio los pesajes ya vinculados de antes
    // (bug real encontrado en vivo el 2026-08-13, ver docs/DECISIONS.md).
    function getPesajesVinculados($form) {
        var state = getState($form);
        var map = {};
        var items = [];

        $.each((state.config && state.config.pesajesVinculadosExistentes) || [], function (_, id) {
            var idPesaje = parseInt(id, 10) || 0;
            if (idPesaje <= 0 || map[idPesaje]) return;

            map[idPesaje] = true;
            items.push({ id: idPesaje, texto: '' });
        });

        $.each(state.lineas || [], function (_, linea) {
            var idPesaje = parseInt(linea.idPesajeVinculado, 10) || 0;
            if (idPesaje <= 0 || map[idPesaje]) return;

            map[idPesaje] = true;
            items.push({
                id: idPesaje,
                texto: $.trim(linea.pesajeVinculadoTexto || '')
            });
        });

        items.sort(function (a, b) { return a.id - b.id; });
        return items;
    }

    function rebuildPesajesVinculadosInputs($form) {
        var $container = $form.find('#pesajesVinculadosHiddenContainer');
        var html = '';

        $.each(getPesajesVinculados($form), function (index, item) {
            html += '<input type="hidden" name="PesajesVinculadosIds[' + index + ']" value="' + escapeHtml(item.id) + '"/>';
        });

        $container.html(html);
    }

    function renderPesajesVinculadosInfo($form) {
        var $info = $form.find('#stockPesajesVinculadosInfo');
        var items = getPesajesVinculados($form);

        if (!items.length) {
            $info.addClass('d-none').text('');
            return;
        }

        var html = 'Pesajes vinculados en esta edici&oacute;n: ';
        $.each(items, function (index, item) {
            if (index > 0) html += ' | ';
            html += '<strong>#' + escapeHtml(item.id) + '</strong>';
            if (item.texto) {
                html += ' <span class="text-muted">(' + escapeHtml(item.texto) + ')</span>';
            }
        });

        $info.html(html).removeClass('d-none');
    }

    function getSortValueLinea(linea, sortKey) {
        switch (sortKey) {
            case 'producto':
                return String(linea.producto || '').toLowerCase();
            case 'codigo':
                return String(linea.codigo || '').toLowerCase();
            case 'cantidad':
                return toNumber(linea.cantKgs);
            case 'creado':
            default:
                var fecha = parseCreadoTexto(linea.creadoTexto);
                return fecha ? fecha.getTime() : null;
        }
    }

    // Ordena una copia de state.lineas sin mutar el array original (a diferencia de la vieja sortLineasByCreado),
    // para que el índice real de cada línea (indiceOriginal) se preserve aunque cambie el orden visual.
    function getLineasOrdenadas($form) {
        var state = getState($form);
        return state.lineas
            .map(function (linea, index) {
                return { linea: linea, indiceOriginal: index };
            })
            .sort(function (a, b) {
                var left = getSortValueLinea(a.linea, state.sortKey);
                var right = getSortValueLinea(b.linea, state.sortKey);
                var result;

                if (left === null && right === null) {
                    result = 0;
                } else if (left === null) {
                    result = -1;
                } else if (right === null) {
                    result = 1;
                } else if (typeof left === 'string' || typeof right === 'string') {
                    result = String(left).localeCompare(String(right), 'es', { numeric: true, sensitivity: 'base' });
                } else {
                    result = left === right ? 0 : (left > right ? 1 : -1);
                }

                if (result === 0) {
                    result = a.indiceOriginal - b.indiceOriginal;
                }

                return state.sortDirection === 'desc' ? result * -1 : result;
            });
    }

    function lineaMatchesBusqueda(linea, searchText) {
        if (!searchText) return true;
        var needle = searchText.toLowerCase();
        return String(linea.producto || '').toLowerCase().indexOf(needle) !== -1
            || String(linea.codigo || '').toLowerCase().indexOf(needle) !== -1;
    }

    function getLineasVisibles($form) {
        var state = getState($form);
        var needle = $.trim(state.searchText || '').toLowerCase();
        return getLineasOrdenadas($form).filter(function (item) {
            return lineaMatchesBusqueda(item.linea, needle);
        });
    }

    function syncSortHeadersStock($form) {
        var state = getState($form);
        var $headers = $form.find('#tablaLineasStock thead [data-sort-key]');
        $headers.css('cursor', 'pointer');
        $headers.removeClass('stock-sort-active stock-sort-asc stock-sort-desc');

        $headers.each(function () {
            var $el = $(this);
            var label = ($el.data('base-label') || $el.text() || '').trim();
            var key = String($el.data('sort-key') || '');

            if (!$el.data('base-label')) {
                $el.data('base-label', label);
            }

            if (key === state.sortKey) {
                $el.text(label + (state.sortDirection === 'desc' ? ' ↓' : ' ↑'));
                $el.addClass('stock-sort-active ' + (state.sortDirection === 'desc' ? 'stock-sort-desc' : 'stock-sort-asc'));
                $el.attr('title', 'Ordenado por ' + label + ' (' + (state.sortDirection === 'desc' ? 'descendente' : 'ascendente') + ')');
            } else {
                $el.text(label);
                $el.attr('title', 'Ordenar por ' + label);
            }
        });
    }

    function renderLineas($form) {
        var state = getState($form);
        var $tbody = $form.find('#tablaLineasStock tbody');
        var html = '';

        // El detalle se vuelve a dibujar completo en cada cambio para mantener sincronizados grilla, totales e inputs hidden.
        if (!$tbody.length) return;

        if (!state.lineas.length) {
            $tbody.html('<tr class="js-empty-row"><td colspan="5" class="text-center text-muted">Todavía no hay líneas cargadas.</td></tr>');
            recalculate($form);
            rebuildHiddenInputs($form);
            syncSortHeadersStock($form);
            return;
        }

        var visibles = getLineasVisibles($form);

        if (!visibles.length) {
            $tbody.html('<tr class="js-empty-row"><td colspan="5" class="text-center text-muted">Sin resultados para la búsqueda.</td></tr>');
            recalculate($form);
            rebuildHiddenInputs($form);
            syncSortHeadersStock($form);
            return;
        }

        $.each(visibles, function (_, item) {
            var linea = item.linea;
            var vinculoHtml = linea.idPesajeVinculado > 0
                ? '<span class="badge badge-info mb-1">Pesaje vinculado #' + escapeHtml(linea.idPesajeVinculado) + '</span><br>'
                : '';

            html += '<tr data-index="' + item.indiceOriginal + '">'
                + '<td><strong>' + escapeHtml(linea.producto || '') + '</strong><br><small class="text-muted">Código: ' + escapeHtml(linea.codigo || '') + '</small></td>'
                + '<td class="text-right">' + formatNumber(linea.cantKgs, 3) + '</td>'
                + '<td class="text-center">' + (linea.balanza ? '*' : '') + '</td>'
                + '<td>' + (linea.noContado ? '<span class="badge badge-warning mb-1">No contado</span><br>' : '') + vinculoHtml + escapeHtml(linea.creadoTexto || '-') + '</td>'
                + '<td class="text-center"><button type="button" class="btn btn-sm btn-outline-danger" data-action="remove-line" data-index="' + item.indiceOriginal + '"><i class="fas fa-trash"></i></button></td>'
                + '</tr>';
        });

        $tbody.html(html);
        recalculate($form);
        rebuildHiddenInputs($form);
        syncSortHeadersStock($form);
    }

    function buildAcumulados($form) {
        var state = getState($form);
        var map = {};

        $.each(state.lineas, function (_, linea) {
            var key = String(linea.idCorte || linea.codigo || '');
            if (!map[key]) {
                map[key] = {
                    codigo: linea.codigo || '',
                    producto: linea.producto || '',
                    cantidad: 0
                };
            }
            map[key].cantidad += toNumber(linea.cantKgs);
        });

        return Object.keys(map).map(function (key) { return map[key]; }).sort(function (a, b) {
            return String(a.codigo || '').localeCompare(String(b.codigo || ''), undefined, {
                numeric: true,
                sensitivity: 'base'
            });
        });
    }

    function renderAcumulados($form) {
        var items = buildAcumulados($form);
        var html = '';

        if (!items.length) {
            html = '<tr><td colspan="3" class="text-center text-muted">No hay productos cargados.</td></tr>';
        } else {
            $.each(items, function (_, item) {
                html += '<tr>'
                    + '<td>' + escapeHtml(item.codigo) + '</td>'
                    + '<td>' + escapeHtml(item.producto) + '</td>'
                    + '<td class="text-right">' + formatNumber(item.cantidad, 3) + '</td>'
                    + '</tr>';
            });
        }

        $('#tbodyAcumuladosStock').html(html);
    }

    function renderTablaPorcentajes($table, tabla, emptyText) {
        var $thead = $table.find('thead');
        var $tbody = $table.find('tbody');
        var columnas = tabla && $.isArray(tabla.columnas) ? tabla.columnas : [];
        var filas = tabla && $.isArray(tabla.filas) ? tabla.filas : [];
        var visibles = $.grep(columnas, function (col) { return !col.oculta; });
        var headHtml = '';
        var bodyHtml = '';

        if (!visibles.length) {
            $thead.html('');
            $tbody.html('<tr><td class="text-center text-muted py-3">' + escapeHtml(emptyText || 'Sin datos.') + '</td></tr>');
            return;
        }

        headHtml += '<tr>';
        $.each(columnas, function (_, col) {
            if (col.oculta) return;
            headHtml += '<th' + (col.alineacionDerecha ? ' class="text-right"' : '') + '>' + escapeHtml(col.nombre || '') + '</th>';
        });
        headHtml += '</tr>';

        if (!filas.length) {
            bodyHtml = '<tr><td colspan="' + visibles.length + '" class="text-center text-muted py-3">' + escapeHtml(emptyText || 'Sin datos.') + '</td></tr>';
        } else {
            $.each(filas, function (_, fila) {
                bodyHtml += '<tr>';
                $.each(columnas, function (colIndex, col) {
                    if (col.oculta) return;
                    var valor = $.isArray(fila) ? (fila[colIndex] || '') : '';
                    bodyHtml += '<td' + (col.alineacionDerecha ? ' class="text-right"' : '') + '>' + escapeHtml(valor) + '</td>';
                });
                bodyHtml += '</tr>';
            });
        }

        $thead.html(headHtml);
        $tbody.html(bodyHtml);
    }

    function loadPorcentajesPesaje($form) {
        var state = getState($form);
        if (!state.config.urls || !state.config.urls.verPorcentajesPesaje || state.loadingPorcentajes) return;

        state.loadingPorcentajes = true;
        clearPorcentajeWarning();
        renderTablaPorcentajes($('#tablaPromMediasStock'), null, 'Cargando...');
        renderTablaPorcentajes($('#tablaPorcCortesStock'), null, 'Cargando...');

        $.ajax({
            url: state.config.urls.verPorcentajesPesaje,
            method: 'POST',
            data: {
                idCompra: parseInt($form.find('#IdCompra').val(), 10) || 0
            }
        }).done(function (resp) {
            if (!resp || resp.ok !== true) {
                showPorcentajeWarning((resp && resp.mensaje) || 'No se pudo obtener el análisis del pesaje.');
                renderTablaPorcentajes($('#tablaPromMediasStock'), null, 'Sin datos.');
                renderTablaPorcentajes($('#tablaPorcCortesStock'), null, 'Sin datos.');
                return;
            }

            actualizarEstadoAjustePesaje($form, resp.estado || '');
            $('#btnGenerarAjustePesajeStock').prop('disabled', resp.puedeGenerarAjuste !== true);
            renderTablaPorcentajes($('#tablaPromMediasStock'), resp.promMedias, 'Sin datos de promedios.');
            renderTablaPorcentajes($('#tablaPorcCortesStock'), resp.porcCortes, 'Sin datos de porcentajes.');
        }).fail(function () {
            showPorcentajeWarning('No se pudo obtener el análisis del pesaje.');
            renderTablaPorcentajes($('#tablaPromMediasStock'), null, 'Sin datos.');
            renderTablaPorcentajes($('#tablaPorcCortesStock'), null, 'Sin datos.');
        }).always(function () {
            state.loadingPorcentajes = false;
        });
    }

    function renderComprasPesaje(items) {
        var html = '';

        if (!items || !items.length) {
            html = '<tr><td colspan="6" class="text-center text-muted py-4">No hay compras disponibles para seleccionar.</td></tr>';
        } else {
            $.each(items, function (index, item) {
                var detailId = 'compraPesajeDetalle_' + escapeHtml(item.idCompra) + '_' + index;
                var proveedor = item.proveedor || '-';
                var detalleHtml = '';

                if (item.lineas && item.lineas.length) {
                    detalleHtml += '<div class="table-responsive">';
                    detalleHtml += '<table class="table table-sm table-bordered mb-0">';
                    detalleHtml += '<thead class="thead-light"><tr><th>Producto/Corte</th><th class="text-right" style="width:120px;">Cantidad</th><th class="text-right" style="width:120px;">Kilos</th></tr></thead><tbody>';
                    $.each(item.lineas, function (_, linea) {
                        detalleHtml += '<tr>'
                            + '<td>' + escapeHtml(linea.producto || '-') + '</td>'
                            + '<td class="text-right">' + escapeHtml(linea.cantidad || '-') + '</td>'
                            + '<td class="text-right">' + escapeHtml(linea.kilos || '-') + '</td>'
                            + '</tr>';
                    });
                    detalleHtml += '</tbody></table></div>';
                } else {
                    detalleHtml = '<div class="text-muted">No hay líneas para mostrar.</div>';
                }

                html += '<tr>'
                    + '<td>' + escapeHtml(item.fechaCompra || '-') + '</td>'
                    + '<td><div class="font-weight-bold">' + escapeHtml(proveedor) + '</div>'
                    + (item.tipoCompra ? '<div class="small text-muted">' + escapeHtml(item.tipoCompra) + '</div>' : '')
                    + '</td>'
                    + '<td class="text-right font-weight-bold">' + formatNumber(item.kgsMedias || item.totalKg || 0, 3) + '</td>'
                    + '<td class="text-right font-weight-bold">' + escapeHtml(item.cantMedias || 0) + '</td>'
                    + '<td class="text-center"><button type="button" class="btn btn-sm btn-outline-info" data-toggle="collapse" data-target="#' + detailId + '" aria-expanded="false" aria-controls="' + detailId + '"><i class="fas fa-list mr-1"></i>Detalle</button></td>'
                    + '<td class="text-center"><button type="button" class="btn btn-sm btn-primary" data-action="select-compra-pesaje" data-id-compra="' + escapeHtml(item.idCompra) + '" data-id-proveedor="' + escapeHtml(item.idProveedor) + '" data-proveedor="' + escapeHtml(item.proveedor || '') + '" data-proveedor-cuit="' + escapeHtml(item.proveedorCuit || '') + '" data-cant-medias="' + escapeHtml(item.cantMedias || 0) + '" data-kgs-medias="' + escapeHtml(item.kgsMedias || item.totalKg || 0) + '">Seleccionar</button></td>'
                    + '</tr>';
                html += '<tr class="bg-light">'
                    + '<td colspan="6" class="p-0">'
                    + '<div id="' + detailId + '" class="collapse">'
                    + '<div class="p-3">' + detalleHtml + '</div>'
                    + '</div>'
                    + '</td>'
                    + '</tr>';
            });
        }

        $('#tbodyComprasPesajeStock').html(html);
    }

    function cargarComprasPesaje($form) {
        var state = getState($form);
        if (!state.config.urls || !state.config.urls.ultimasComprasPesaje || state.loadingComprasPesaje) return;

        state.loadingComprasPesaje = true;
        clearComprasPesajeWarning();
        $('#lblSucursalComprasPesajeStock').text($form.find('#IdSucursal option:selected').text() || '-');
        $('#tbodyComprasPesajeStock').html('<tr><td colspan="6" class="text-center text-muted py-4">Cargando...</td></tr>');

        $.ajax({
            url: state.config.urls.ultimasComprasPesaje,
            method: 'GET',
            cache: false,
            data: {
                idSucursal: parseInt($form.find('#IdSucursal').val(), 10) || 0,
                soloComprasPesaje: true,
                proveedor: $.trim($('#txtFiltroProveedorComprasPesaje').val() || ''),
                fechaDesde: $('#txtFechaDesdeComprasPesaje').val() || '',
                fechaHasta: $('#txtFechaHastaComprasPesaje').val() || ''
            }
        }).done(function (resp) {
            if (!resp || resp.ok !== true) {
                showComprasPesajeWarning((resp && resp.mensaje) || 'No se pudieron cargar las compras.');
                renderComprasPesaje([]);
                return;
            }

            renderComprasPesaje(resp.items || []);
        }).fail(function () {
            showComprasPesajeWarning('No se pudieron cargar las compras.');
            renderComprasPesaje([]);
        }).always(function () {
            state.loadingComprasPesaje = false;
        });
    }

    function resetFiltrosComprasPesaje() {
        var today = new Date();
        var desde = new Date(today.getFullYear(), today.getMonth(), today.getDate() - 7);
        $('#txtFiltroProveedorComprasPesaje').val('');
        $('#txtFechaDesdeComprasPesaje').val(formatDateInputValue(desde));
        $('#txtFechaHastaComprasPesaje').val(formatDateInputValue(today));
    }

    function scheduleCargaComprasPesaje($form) {
        var state = getState($form);
        window.clearTimeout(state.filtrosComprasPesajeTimer);
        state.filtrosComprasPesajeTimer = window.setTimeout(function () {
            cargarComprasPesaje($form);
        }, 250);
    }

    function seleccionarCompraPesaje($form, $button) {
        if (!$button || !$button.length) return;

        setProveedor($form, {
            id: $button.data('id-proveedor') || 0,
            razon: $button.data('proveedor') || '',
            cuit: $button.data('proveedor-cuit') || ''
        });

        $form.find('#CantMedias').val($button.data('cant-medias') || '');
        $form.find('#KgsMedias').val(formatNumber($button.data('kgs-medias') || 0, 2));
        clearWarning($form);
        scheduleDraft($form);
        getComprasPesajeModal().modal('hide');
        showFeedback($form, 'Compra seleccionada correctamente.');
    }

    function renderComprasPesaje(items) {
        var html = '';

        if (!items || !items.length) {
            html = '<tr><td colspan="6" class="text-center text-muted py-4">No hay compras disponibles para seleccionar.</td></tr>';
        } else {
            $.each(items, function (index, item) {
                var detailId = 'compraPesajeDetalle_' + escapeHtml(item.idCompra) + '_' + index;
                var proveedor = item.proveedor || '-';

                html += '<tr>'
                    + '<td>' + escapeHtml(item.fechaCompra || '-') + '</td>'
                    + '<td><div class="font-weight-bold">' + escapeHtml(proveedor) + '</div>'
                    + (item.tipoCompra ? '<div class="small text-muted">' + escapeHtml(item.tipoCompra) + '</div>' : '')
                    + '</td>'
                    + '<td class="text-right font-weight-bold">' + formatNumber(item.kgsMedias || item.totalKg || 0, 3) + '</td>'
                    + '<td class="text-right font-weight-bold">' + escapeHtml(item.cantMedias || 0) + '</td>'
                    + '<td class="text-center"><button type="button" class="btn btn-sm btn-outline-info" data-action="toggle-detalle-compra-pesaje" data-id-compra="' + escapeHtml(item.idCompra) + '" data-target="#' + detailId + '" aria-expanded="false" aria-controls="' + detailId + '"><i class="fas fa-list mr-1"></i>Detalle</button></td>'
                    + '<td class="text-center"><button type="button" class="btn btn-sm btn-primary" data-action="select-compra-pesaje" data-id-compra="' + escapeHtml(item.idCompra) + '" data-id-proveedor="' + escapeHtml(item.idProveedor || 0) + '" data-proveedor="' + escapeHtml(item.proveedor || '') + '">Vincular</button></td>'
                    + '</tr>';
                html += '<tr class="bg-light">'
                    + '<td colspan="6" class="p-0">'
                    + '<div id="' + detailId + '" class="collapse">'
                    + '<div class="p-3"><div class="text-muted">Presione Detalle para cargar las lineas de la compra.</div></div>'
                    + '</div>'
                    + '</td>'
                    + '</tr>';
            });
        }

        $('#tbodyComprasPesajeStock').html(html);
    }

    function buildCompraPesajeDetalleHtml(item) {
        var detalleHtml = '';
        if (item && item.lineas && item.lineas.length) {
            detalleHtml += '<div class="table-responsive">';
            detalleHtml += '<table class="table table-sm table-bordered mb-0">';
            detalleHtml += '<thead class="thead-light"><tr><th>Producto/Corte</th><th class="text-right" style="width:120px;">Cantidad</th><th class="text-right" style="width:120px;">Kilos</th></tr></thead><tbody>';
            $.each(item.lineas, function (_, linea) {
                detalleHtml += '<tr>'
                    + '<td>' + escapeHtml(linea.producto || '-') + '</td>'
                    + '<td class="text-right">' + escapeHtml(linea.cantidad || '-') + '</td>'
                    + '<td class="text-right">' + escapeHtml(linea.kilos || '-') + '</td>'
                    + '</tr>';
            });
            detalleHtml += '</tbody></table></div>';
            return detalleHtml;
        }

        return '<div class="text-muted">No hay lineas para mostrar.</div>';
    }

    function cargarDetalleCompraPesaje($form, idCompra, onSuccess, onError) {
        var state = getState($form);
        if (!state || !state.config.urls || !state.config.urls.detalleCompraPesaje) {
            if ($.isFunction(onError)) onError('No se pudo cargar el detalle de la compra.');
            return;
        }

        idCompra = parseInt(idCompra, 10) || 0;
        if (idCompra <= 0) {
            if ($.isFunction(onError)) onError('Seleccione una compra valida.');
            return;
        }

        if (state.comprasPesajeDetalle[idCompra]) {
            if ($.isFunction(onSuccess)) onSuccess(state.comprasPesajeDetalle[idCompra]);
            return;
        }

        if (state.loadingDetalleCompraPesaje[idCompra]) {
            return;
        }

        state.loadingDetalleCompraPesaje[idCompra] = true;

        $.ajax({
            url: state.config.urls.detalleCompraPesaje,
            method: 'GET',
            cache: false,
            data: {
                idCompra: idCompra
            }
        }).done(function (resp) {
            if (!resp || resp.ok !== true || !resp.item) {
                if ($.isFunction(onError)) onError((resp && resp.mensaje) || 'No se pudo cargar el detalle de la compra.');
                return;
            }

            state.comprasPesajeDetalle[idCompra] = resp.item;
            if ($.isFunction(onSuccess)) onSuccess(resp.item);
        }).fail(function () {
            if ($.isFunction(onError)) onError('No se pudo cargar el detalle de la compra.');
        }).always(function () {
            state.loadingDetalleCompraPesaje[idCompra] = false;
        });
    }

    function toggleDetalleCompraPesaje($form, $button) {
        if (!$button || !$button.length) return;

        var idCompra = parseInt($button.data('id-compra'), 10) || 0;
        var target = $button.data('target');
        var $collapse = target ? $(target) : $();
        if (!$collapse.length) return;

        if ($collapse.hasClass('show')) {
            $collapse.collapse('hide');
            return;
        }

        var $body = $collapse.find('.p-3').first();
        $body.html('<div class="text-muted">Cargando detalle...</div>');

        cargarDetalleCompraPesaje($form, idCompra, function (item) {
            $body.html(buildCompraPesajeDetalleHtml(item));
            $collapse.collapse('show');
        }, function (mensaje) {
            $body.html('<div class="text-danger">' + escapeHtml(mensaje || 'No se pudo cargar el detalle de la compra.') + '</div>');
            $collapse.collapse('show');
        });
    }

    function seleccionarCompraPesaje($form, $button) {
        if (!$button || !$button.length) return;

        var idCompra = parseInt($button.data('id-compra'), 10) || 0;
        if (idCompra <= 0) {
            showComprasPesajeWarning('Seleccione una compra valida.');
            return;
        }

        $button.prop('disabled', true);
        clearComprasPesajeWarning();

        setProveedor($form, {
            id: $button.data('id-proveedor') || 0,
            razon: $button.data('proveedor') || '',
            cuit: ''
        });

        cargarDetalleCompraPesaje($form, idCompra, function (item) {
            setProveedor($form, {
                id: item.idProveedor || $button.data('id-proveedor') || 0,
                razon: item.proveedor || $button.data('proveedor') || '',
                cuit: item.proveedorCuit || ''
            });

            setCompraVinculadaPesaje($form, {
                idCompra: item.idCompra || idCompra,
                fechaCompra: item.fechaCompra || '',
                proveedor: item.proveedor || '',
                estado: ''
            });
            $form.find('#CantMedias').val(item.cantMedias || '');
            $form.find('#KgsMedias').val(formatNumber(item.kgsMedias || item.totalKg || 0, 2));
            clearWarning($form);
            scheduleDraft($form);
            getComprasPesajeModal().modal('hide');
            showFeedback($form, 'Compra vinculada correctamente.');
            $button.prop('disabled', false);
        }, function (mensaje) {
            showComprasPesajeWarning(mensaje || 'No se pudo seleccionar la compra.');
            $button.prop('disabled', false);
        });
    }

    function mostrarExitoVinculacion($form, mensaje) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({
                icon: 'success',
                title: 'Pesaje vinculado correctamente',
                text: mensaje || '',
                timer: 1800,
                showConfirmButton: false
            });
            return;
        }

        showFeedback($form, 'Pesaje vinculado correctamente.');
    }

    function mostrarResultadoAjustePesaje(esExito, mensaje, onClose) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({
                icon: esExito ? 'success' : 'error',
                title: esExito ? 'Ajuste generado correctamente' : 'No se pudo generar el ajuste',
                text: mensaje || ''
            }).then(function () {
                if (typeof onClose === 'function') onClose();
            });
            return;
        }

        if (typeof onClose === 'function') {
            window.setTimeout(function () {
                onClose();
            }, 200);
        }
    }

    function formatDateInputValue(date) {
        if (!date || Object.prototype.toString.call(date) !== '[object Date]' || isNaN(date.getTime())) {
            return '';
        }

        var yyyy = date.getFullYear();
        var mm = String(date.getMonth() + 1).padStart(2, '0');
        var dd = String(date.getDate()).padStart(2, '0');
        return yyyy + '-' + mm + '-' + dd;
    }

    function parseCreadoTexto(value) {
        var text = $.trim(value || '');
        if (!text) return null;

        var match = /^(\d{1,2})\/(\d{1,2})\/(\d{4})\s+(\d{1,2}):(\d{2})$/.exec(text);
        if (!match) return null;

        var day = parseInt(match[1], 10);
        var month = parseInt(match[2], 10) - 1;
        var year = parseInt(match[3], 10);
        var hour = parseInt(match[4], 10);
        var minute = parseInt(match[5], 10);
        var date = new Date(year, month, day, hour, minute, 0, 0);

        return isNaN(date.getTime()) ? null : date;
    }

    function resetFiltrosPesajesVinculables() {
        var today = new Date();
        var desde = new Date(today.getFullYear(), today.getMonth(), today.getDate() - 7);
        $('#txtFiltroProveedorVincularPesajes').val('');
        $('#txtFechaDesdeVincularPesajes').val(formatDateInputValue(desde));
        $('#txtFechaHastaVincularPesajes').val(formatDateInputValue(today));
    }

    function yaEstaVinculadoEnEdicion($form, idPesaje) {
        idPesaje = parseInt(idPesaje, 10) || 0;
        if (idPesaje <= 0) return false;

        var items = getPesajesVinculados($form);
        for (var i = 0; i < items.length; i++) {
            if ((parseInt(items[i].id, 10) || 0) === idPesaje) {
                return true;
            }
        }

        return false;
    }

    function renderPesajesVinculables($form, items) {
        var html = '';

        if (!items || !items.length) {
            html = '<tr><td colspan="8" class="text-center text-muted py-4">No hay pesajes disponibles para vincular.</td></tr>';
        } else {
            $.each(items, function (index, item) {
                var detailId = 'pesajeVinculableDetalle_' + escapeHtml(item.idCompra) + '_' + index;
                var proveedor = item.proveedor || '-';
                var yaVinculado = yaEstaVinculadoEnEdicion($form, item.idCompra);
                var esActual = item.esActual === true;
                var textoBoton = esActual ? 'Pesaje actual' : (yaVinculado ? 'Ya vinculado' : 'Vincular');
                var disabled = esActual || yaVinculado;

                html += '<tr>'
                    + '<td>' + escapeHtml(item.idCompra || 0) + '</td>'
                    + '<td>' + escapeHtml(item.fechaCompra || '-') + '</td>'
                    + '<td><div class="font-weight-bold">' + escapeHtml(proveedor) + '</div>'
                    + (item.tipoCompra ? '<div class="small text-muted">' + escapeHtml(item.tipoCompra) + '</div>' : '')
                    + '</td>'
                    + '<td class="text-right font-weight-bold">' + escapeHtml(item.cantMedias || 0) + '</td>'
                    + '<td class="text-right font-weight-bold">' + formatNumber(item.kgsMedias || item.totalKg || 0, 3) + '</td>'
                    + '<td>' + escapeHtml(item.sucursal || '-') + '</td>'
                    + '<td class="text-center"><button type="button" class="btn btn-sm btn-outline-info" data-action="toggle-detalle-vincular-pesaje" data-id-compra="' + escapeHtml(item.idCompra) + '" data-target="#' + detailId + '" aria-expanded="false" aria-controls="' + detailId + '"><i class="fas fa-list mr-1"></i>Detalle</button></td>'
                    + '<td class="text-center"><button type="button" class="btn btn-sm ' + (disabled ? 'btn-outline-secondary' : 'btn-info') + '" data-action="vincular-pesaje" data-id-compra="' + escapeHtml(item.idCompra) + '"' + (disabled ? ' disabled="disabled"' : '') + '>' + textoBoton + '</button></td>'
                    + '</tr>';
                html += '<tr class="bg-light">'
                    + '<td colspan="8" class="p-0">'
                    + '<div id="' + detailId + '" class="collapse">'
                    + '<div class="p-3"><div class="text-muted">Presione Detalle para cargar las líneas del pesaje.</div></div>'
                    + '</div>'
                    + '</td>'
                    + '</tr>';
            });
        }

        $('#tbodyVincularPesajesStock').html(html);
    }

    function cargarPesajesVinculables($form) {
        var state = getState($form);
        if (!state.config.urls || !state.config.urls.ultimosPesajesVinculables || state.loadingPesajesVinculables) return;

        state.loadingPesajesVinculables = true;
        clearVincularPesajesWarning();
        $('#lblSucursalVincularPesajesStock').text($form.find('#IdSucursal option:selected').text() || '-');
        $('#tbodyVincularPesajesStock').html('<tr><td colspan="8" class="text-center text-muted py-4">Cargando...</td></tr>');

        $.ajax({
            url: state.config.urls.ultimosPesajesVinculables,
            method: 'GET',
            cache: false,
            data: {
                idSucursal: parseInt($form.find('#IdSucursal').val(), 10) || 0,
                idCompraActual: parseInt($form.find('#IdCompra').val(), 10) || 0,
                soloPesajes: true,
                proveedor: $.trim($('#txtFiltroProveedorVincularPesajes').val() || ''),
                fechaDesde: $('#txtFechaDesdeVincularPesajes').val() || '',
                fechaHasta: $('#txtFechaHastaVincularPesajes').val() || ''
            }
        }).done(function (resp) {
            if (!resp || resp.ok !== true) {
                showVincularPesajesWarning((resp && resp.mensaje) || 'No se pudieron cargar los pesajes.');
                renderPesajesVinculables($form, []);
                return;
            }

            renderPesajesVinculables($form, resp.items || []);
        }).fail(function () {
            showVincularPesajesWarning('No se pudieron cargar los pesajes.');
            renderPesajesVinculables($form, []);
        }).always(function () {
            state.loadingPesajesVinculables = false;
        });
    }

    function scheduleCargaPesajesVinculables($form) {
        var state = getState($form);
        window.clearTimeout(state.filtrosPesajesTimer);
        state.filtrosPesajesTimer = window.setTimeout(function () {
            cargarPesajesVinculables($form);
        }, 250);
    }

    function vincularPesaje($form, $button) {
        if (!$button || !$button.length) return;

        var idCompra = parseInt($button.data('id-compra'), 10) || 0;
        if (idCompra <= 0) {
            showVincularPesajesWarning('Seleccione un pesaje válido.');
            return;
        }

        if (yaEstaVinculadoEnEdicion($form, idCompra)) {
            showVincularPesajesWarning('Ese pesaje ya fue vinculado en esta edición.');
            return;
        }

        $button.prop('disabled', true);
        clearVincularPesajesWarning();

        cargarDetalleCompraPesaje($form, idCompra, function (item) {
            var state = getState($form);
            var lineasAgregadas = 0;
            var textoPesaje = (item.fechaCompra || '-') + ' | ' + (item.proveedor || '-');

            $.each(item.lineas || [], function (_, linea) {
                var idCorte = parseInt(linea.idCorte, 10) || 0;
                if (idCorte <= 0) return;

                state.lineas.push(normalizeLinea({
                    idCorte: idCorte,
                    codigo: linea.codigo || '',
                    producto: linea.producto || '',
                    cantKgs: toNumber(linea.kilos),
                    balanza: false,
                    creadoTexto: fechaHoraActualTexto(),
                    pesable: linea.pesable === true,
                    noContado: false,
                    idPesajeVinculado: idCompra,
                    pesajeVinculadoTexto: textoPesaje
                }));
                lineasAgregadas++;
            });

            if (!lineasAgregadas) {
                showVincularPesajesWarning('El pesaje seleccionado no tiene líneas de cortes para vincular.');
                $button.prop('disabled', false);
                return;
            }

            renderLineas($form);
            scheduleDraft($form);
            getVincularPesajesModal().modal('hide');
            mostrarExitoVinculacion($form, '');
            $button.prop('disabled', false);
        }, function (mensaje) {
            showVincularPesajesWarning(mensaje || 'No se pudo vincular el pesaje.');
            $button.prop('disabled', false);
        });
    }

    function generarAjustePesaje($form) {
        var state = getState($form);
        if (!state.config.urls || !state.config.urls.generarAjustePesaje || state.generandoAjuste) return;

        state.generandoAjuste = true;
        clearPorcentajeWarning();
        $('#btnGenerarAjustePesajeStock').prop('disabled', true);

        $.ajax({
            url: state.config.urls.generarAjustePesaje,
            method: 'POST',
            data: {
                __RequestVerificationToken: $form.find('input[name="__RequestVerificationToken"]').val(),
                idCompra: parseInt($form.find('#IdCompra').val(), 10) || 0
            }
        }).done(function (resp) {
            if (!resp || resp.ok !== true) {
                var mensajeError = (resp && resp.mensaje) || 'No se pudo generar el ajuste.';
                showPorcentajeWarning(mensajeError);
                mostrarResultadoAjustePesaje(false, mensajeError);
                return;
            }

            actualizarEstadoAjustePesaje($form, resp.estado || 'Actualizado');
            getPorcentajesModal().modal('hide');
            showFeedback($form, resp.mensaje || 'El Ajuste de Stock se realizó correctamente.');
            mostrarResultadoAjustePesaje(true, resp.mensaje || 'El Ajuste de Stock se realizó correctamente.', function () {
                window.location.reload();
            });
        }).fail(function () {
            showPorcentajeWarning('No se pudo generar el ajuste.');
            mostrarResultadoAjustePesaje(false, 'No se pudo generar el ajuste.');
        }).always(function () {
            state.generandoAjuste = false;
            if ($.trim($('#lblEstadoAjusteModalStock').text() || '').toLowerCase() !== 'actualizado') {
                $('#btnGenerarAjustePesajeStock').prop('disabled', false);
            }
        });
    }

    function getCodigosCargados($form) {
        var state = getState($form);
        var codigos = [];
        $.each(state.lineas, function (_, linea) {
            var codigo = parseInt(linea.codigo, 10);
            if (!isNaN(codigo) && codigo > 0) {
                codigos.push(codigo);
            }
        });
        return codigos;
    }

    function getNoCargadosFiltrados($form) {
        var state = getState($form);
        var $modal = getNoCargadosModal();
        var filtro = String($modal.find('#filtroStockNoCargados').val() || 'todos').toLowerCase();
        var texto = String($modal.find('#txtBuscarNoCargadosStock').val() || '').toLowerCase().trim();

        return $.grep(state.noCargados || [], function (item) {
            var stock = toNumber(item.stockActual);
            if (filtro === 'con-stock' && stock === 0) return false;
            if (filtro === 'sin-stock' && stock !== 0) return false;

            if (!texto) return true;

            return String(item.codigo || '').toLowerCase().indexOf(texto) >= 0
                || String(item.producto || '').toLowerCase().indexOf(texto) >= 0;
        });
    }

    function renderNoCargados($form) {
        syncSeleccionNoCargadosDesdeTabla();

        var items = getNoCargadosFiltrados($form);
        var $modal = getNoCargadosModal();
        var $tbody = $modal.find('#tablaNoCargadosStock tbody');
        var seleccionados = getNoCargadosSeleccionados();
        var html = '';

        if (!items.length) {
            html = '<tr><td colspan="4" class="text-center text-muted py-4">No hay productos pendientes para mostrar.</td></tr>';
        } else {
            $.each(items, function (_, item) {
                var itemId = parseInt(item.idCorte, 10) || 0;
                var checked = !!seleccionados[itemId];
                html += '<tr>'
                    + '<td class="text-center"><input type="checkbox" class="js-no-cargado-check" data-id="' + escapeHtml(item.idCorte) + '"' + (checked ? ' checked="checked"' : '') + ' /></td>'
                    + '<td>' + escapeHtml(item.codigo) + '</td>'
                    + '<td>' + escapeHtml(item.producto) + '</td>'
                    + '<td class="text-right">' + formatNumber(item.stockActual, 3) + '</td>'
                    + '</tr>';
            });
        }

        $tbody.html(html);
        actualizarContadoresNoCargados($form);
    }

    function actualizarContadoresNoCargados($form) {
        syncSeleccionNoCargadosDesdeTabla();

        var state = getState($form);
        var $modal = getNoCargadosModal();
        var items = getNoCargadosFiltrados($form);
        var seleccionadosMap = getNoCargadosSeleccionados();
        var seleccionados = 0;

        $.each(state.noCargados || [], function (_, item) {
            var itemId = parseInt(item.idCorte, 10) || 0;
            if (itemId > 0 && seleccionadosMap[itemId]) {
                seleccionados++;
            }
        });

        $modal.find('#lblPendientesNoCargados').text(items.length + ' productos');
        $modal.find('#lblSeleccionadosNoCargados').text(seleccionados);

        var visibles = $modal.find('.js-no-cargado-check').length;
        var seleccionVisibles = $modal.find('.js-no-cargado-check:checked').length;
        var parcial = seleccionVisibles > 0 && seleccionVisibles < visibles;
        $modal.find('#chkSeleccionarTodosNoCargados')
            .prop('checked', visibles > 0 && visibles === seleccionVisibles)
            .prop('indeterminate', parcial);

        if (parcial) {
            showNoCargadosInfo('Hay una selección parcial. Si lo desea, puede seleccionar o deseleccionar todos.');
        } else {
            clearNoCargadosInfo();
        }
    }

    function cargarProductosNoCargados($form) {
        var state = getState($form);
        if (!state.config.urls || !state.config.urls.productosNoCargadosCierre || state.loadingNoCargados) return;

        state.loadingNoCargados = true;
        clearNoCargadosWarning();
        clearNoCargadosInfo();
        actualizarContextoNoCargados($form);
        getNoCargadosModal().find('#tablaNoCargadosStock tbody').html('<tr><td colspan="4" class="text-center text-muted py-4">Cargando...</td></tr>');

        $.ajax({
            url: state.config.urls.productosNoCargadosCierre,
            method: 'POST',
            traditional: true,
            data: {
                idSucursal: $form.find('#IdSucursal').val(),
                fechaCompra: $form.find('#FechaCompra').val(),
                idCompra: parseInt($form.find('#IdCompra').val(), 10) || 0,
                codigosCargados: getCodigosCargados($form)
            }
        }).done(function (resp) {
            if (!resp || resp.ok !== true) {
                showWarning($form, (resp && resp.mensaje) || 'No se pudieron cargar los productos pendientes.');
                showNoCargadosWarning($form, (resp && resp.mensaje) || 'No se pudieron cargar los productos pendientes.');
                state.noCargados = [];
                renderNoCargados($form);
                return;
            }

            state.noCargados = $.map(resp.items || [], function (item) {
                return {
                    idCorte: item.idCorte,
                    codigo: item.codigo,
                    producto: item.producto,
                    stockActual: toNumber(item.stockActual)
                };
            });
            var seleccionadosPrevios = getNoCargadosSeleccionados();
            var seleccionadosVigentes = {};
            $.each(state.noCargados, function (_, item) {
                var itemId = parseInt(item.idCorte, 10) || 0;
                if (seleccionadosPrevios[itemId]) {
                    seleccionadosVigentes[itemId] = true;
                }
            });
            setNoCargadosSeleccionados(seleccionadosVigentes);
            if (!state.noCargados.length) {
                showNoCargadosWarning($form, 'No hay productos pendientes para esta sucursal y fecha.');
            } else {
                clearNoCargadosWarning();
                clearNoCargadosInfo();
            }
            renderNoCargados($form);
        }).fail(function () {
            showWarning($form, 'No se pudieron cargar los productos pendientes.');
            showNoCargadosWarning($form, 'No se pudieron cargar los productos pendientes.');
            state.noCargados = [];
            renderNoCargados($form);
        }).always(function () {
            state.loadingNoCargados = false;
        });
    }

    function getSeleccionadosNoCargados($form) {
        var state = getState($form);
        var seleccionados = getNoCargadosSeleccionados();
        return $.grep(state.noCargados || [], function (item) {
            return !!seleccionados[parseInt(item.idCorte, 10) || 0];
        });
    }

    function agregarProductosNoCargados($form, usarStockActual) {
        var state = getState($form);
        var seleccionados = getSeleccionadosNoCargados($form);
        if (!seleccionados.length) {
            showWarning($form, 'Seleccione al menos un producto.');
            return;
        }
        if (seleccionados.length >= 80 && !window.confirm('Vas a agregar ' + seleccionados.length + ' productos, ¿confirmar?')) {
            return;
        }

        var agregados = 0;
        var codigosActuales = getCodigosCargados($form);

        $.each(seleccionados, function (_, item) {
            if (codigosActuales.indexOf(parseInt(item.codigo, 10) || 0) >= 0) {
                return;
            }

            state.lineas.push({
                idCorte: item.idCorte,
                codigo: item.codigo,
                producto: item.producto,
                cantKgs: usarStockActual ? toNumber(item.stockActual) : -0.0006,
                balanza: false,
                creadoTexto: fechaHoraActualTexto(),
                pesable: false,
                noContado: true
            });
            codigosActuales.push(parseInt(item.codigo, 10) || 0);
            agregados++;
        });

        if (!agregados) {
            showWarning($form, 'No se agregaron productos nuevos.');
            return;
        }

        state.noCargados = $.grep(state.noCargados || [], function (item) {
            return codigosActuales.indexOf(parseInt(item.codigo, 10) || 0) < 0;
        });
        var seleccionadosActuales = getNoCargadosSeleccionados();
        $.each(seleccionados, function (_, item) {
            delete seleccionadosActuales[parseInt(item.idCorte, 10) || 0];
        });
        setNoCargadosSeleccionados(seleccionadosActuales);

        renderLineas($form);
        renderNoCargados($form);
        scheduleDraft($form);
        showFeedback($form, 'Se agregaron ' + agregados + ' productos');
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

    function saveDraft($form) {
        var state = getState($form);
        var key = $form.find('#DraftKey').val();
        if (!key || !window.localStorage) return;

        try {
            window.localStorage.setItem(key, JSON.stringify({
                idSucursal: $form.find('#IdSucursal').val(),
                fechaCompra: $form.find('#FechaCompra').val(),
                observaciones: $form.find('#Observaciones').val(),
                idProveedor: $form.find('#IdProveedor').val(),
                idPesajeAjustado: $form.find('#IdPesajeAjustado').val(),
                proveedorNombre: $form.find('#razonSocial').val(),
                proveedorCuit: ($form.find('#lblProveedorCuit').text() || '').replace(/^CUIT:\s*/i, ''),
                compraVinculadaFecha: $form.find('#stockCompraVinculadaFecha').text() || '',
                compraVinculadaProveedor: $form.find('#stockCompraVinculadaProveedor').text() || '',
                compraVinculadaEstado: $form.find('#stockCompraVinculadaEstado').text() || '',
                cantMedias: $form.find('#CantMedias').val(),
                kgsMedias: $form.find('#KgsMedias').val(),
                currentLine: {
                    id: $form.find('#txtProductoId').val(),
                    codigo: $form.find('#txtCodigoProducto').val(),
                    nombre: $form.find('#txtProductoNombre').val(),
                    pesable: $form.find('#txtProductoPesable').val(),
                    tipo: $form.find('#txtProductoTipo').val(),
                    promedio: $form.find('#txtProductoPromedio').val(),
                    cantidad: $form.find('#txtCantKgs').val(),
                    balanza: $form.find('#chkBalanzaLinea').is(':checked')
                },
                lineas: state.lineas
            }));
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

    function clearDraft($form) {
        var key = $form.find('#DraftKey').val();
        if (!key || !window.localStorage) return;
        window.localStorage.removeItem(key);
        $form.closest('.stock-page').find('#stockDraftBanner').addClass('d-none');
    }

    function applyDraft($form, draft) {
        var state = getState($form);
        if (!draft) return;

        if ($form.find('#IdSucursal').length) $form.find('#IdSucursal').val(draft.idSucursal || $form.find('#IdSucursal').val());
        if (draft.fechaCompra) $form.find('#FechaCompra').val(draft.fechaCompra);
        if (draft.observaciones !== undefined) $form.find('#Observaciones').val(draft.observaciones);
        autoResizeObservaciones($form);

        if (state.config.esPesaje) {
            setProveedor($form, {
                id: draft.idProveedor || 0,
                razon: draft.proveedorNombre || '',
                cuit: draft.proveedorCuit || ''
            });
            setCompraVinculadaPesaje($form, {
                idCompra: draft.idPesajeAjustado || 0,
                fechaCompra: draft.compraVinculadaFecha || '',
                proveedor: draft.compraVinculadaProveedor || '',
                estado: draft.compraVinculadaEstado || ''
            });
            $form.find('#CantMedias').val(draft.cantMedias || '');
            $form.find('#KgsMedias').val(draft.kgsMedias || '');
        }

        if (draft.currentLine) {
            setProductoActual($form, {
                id: draft.currentLine.id || '',
                codigo: draft.currentLine.codigo || '',
                nombre: draft.currentLine.nombre || '',
                pesable: String(draft.currentLine.pesable || '').toLowerCase() === 'true',
                tipo: draft.currentLine.tipo || '',
                promedio: draft.currentLine.promedio || ''
            });
            $form.find('#txtCantKgs').val(draft.currentLine.cantidad || '');
            $form.find('#chkBalanzaLinea').prop('checked', draft.currentLine.balanza === true);
        }

        state.lineas = $.isArray(draft.lineas) ? $.map(draft.lineas, function (linea) { return normalizeLinea(linea); }) : [];
        renderLineas($form);
    }

    function hideDraftBanner($form) {
        $form.closest('.stock-page').find('#stockDraftBanner').addClass('d-none');
    }

    function showDraftBanner($form) {
        $form.closest('.stock-page').find('#stockDraftBanner').removeClass('d-none');
    }

    function setBalanzaDisponible($form, disponible) {
        var state = getState($form);
        state.balanzaDisponible = !!disponible;
        if (disponible) {
            state.balanzaFaltanteDetectada = false;
        } else {
            state.balanzaFaltanteDetectada = true;
        }
        syncCantidadReadonly($form);
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
            inestable: false,
            raw: data || null
        };
    }

    function renderBalanzaStatus($form, payload) {
        if (window.CarnisysBalanzaUtils && typeof window.CarnisysBalanzaUtils.renderStatus === 'function') {
            window.CarnisysBalanzaUtils.renderStatus('#estadoBalanzaStock', '#barraBalanzaStock', payload);
        }
    }

    function syncBalanzaStatusVisibility($form) {
        $form.find('#balanzaStatusWrapStock').toggle(!!$form.find('#chkBalanzaLinea').is(':checked'));
    }

    function balanzaConectadaDesdePayload(data) {
        return normalizarBalanzaPayload(data).conectada === true;
    }

    function extraerPeso(data) {
        return normalizarBalanzaPayload(data).peso || 0;
    }

    function aplicarLecturaBalanza($form, payload) {
        var state = getState($form);
        var normalized = normalizarBalanzaPayload(payload);
        state.balanzaUltimaLectura = normalized;
        setBalanzaDisponible($form, normalized.conectada === true);
        renderBalanzaStatus($form, payload);

        if (!normalized.conectada) {
            return;
        }

        var hayProducto = !!$.trim($form.find('#txtProductoId').val() || '');
        if (!$form.find('#chkBalanzaLinea').is(':checked') || state.balanzaDesactivadaManual || (hayProducto && !esPesableActual($form))) {
            return;
        }

        $form.find('#txtCantKgs').val(normalized.pesoDisplay || normalized.pesoTexto || formatNumber(normalized.peso, 3));
        if (state.focusAgregarPendiente) {
            state.focusAgregarPendiente = false;
            focusAgregarLinea($form);
        }
    }

    function aplicarStatusBalanza($form, payload) {
        var normalized = normalizarBalanzaPayload(payload);
        setBalanzaDisponible($form, normalized.conectada === true);
        renderBalanzaStatus($form, payload);
    }

    function ensureBalanzaClient($form) {
        var state = getState($form);
        if (state.balanzaClientStarted || !window.CarnisysBalanza) {
            return;
        }

        state.balanzaClientStarted = true;
        window.CarnisysBalanza.start({
            baseUrl: 'http://127.0.0.1:5100',
            statusIntervalMs: 3000,
            pesoIntervalMs: 250,
            onStatus: function (data) {
                aplicarStatusBalanza($form, data);
            },
            onPeso: function (data) {
                aplicarLecturaBalanza($form, data);
            },
            onError: function () {
                setBalanzaDisponible($form, false);
                renderBalanzaStatus($form, null);
            }
        });
    }

    function verificarBalanza($form, callback) {
        if (!window.CarnisysBalanza) {
            setBalanzaDisponible($form, false);
            renderBalanzaStatus($form, null);
            if (typeof callback === 'function') callback(false, null);
            return;
        }

        ensureBalanzaClient($form);
        window.CarnisysBalanza.leerAhora().then(function (data) {
            var disponible = balanzaConectadaDesdePayload(data);
            aplicarLecturaBalanza($form, data);
            if (typeof callback === 'function') callback(disponible, data);
        }).catch(function () {
            setBalanzaDisponible($form, false);
            renderBalanzaStatus($form, null);
            if (typeof callback === 'function') callback(false, null);
        });
    }

    function verificarBalanzaInicial($form) {
        verificarBalanza($form, function (disponible) {
            var state = getState($form);
            if (!disponible) {
                state.balanzaDesactivadaManual = true;
                $form.find('#chkBalanzaLinea').prop('checked', false);
                syncCantidadReadonly($form);
            }
        });
    }

    function detenerLecturaBalanza($form) {
        if (window.CarnisysBalanza) {
            window.CarnisysBalanza.desactivar();
        }
        syncCantidadReadonly($form);
    }

    function iniciarLecturaBalanza($form, silencioso, allowFocusManual) {
        var state = getState($form);
        detenerLecturaBalanza($form);

        verificarBalanza($form, function (disponible, data) {
            if (!disponible) {
                $form.find('#chkBalanzaLinea').prop('checked', false);
                syncCantidadReadonly($form);
                if (!silencioso) {
                    showWarning($form, 'No existe balanza conectada.');
                }
                if (allowFocusManual) {
                    focusCantidadManual($form);
                }
                return;
            }

            clearWarning($form);
            $form.find('#chkBalanzaLinea').prop('checked', true);
            syncCantidadReadonly($form);
            if (window.CarnisysBalanza) {
                window.CarnisysBalanza.activar();
            }
            aplicarLecturaBalanza($form, data);
        });
    }

    function activarBalanzaManual($form) {
        var state = getState($form);
        state.balanzaDesactivadaManual = false;
        iniciarLecturaBalanza($form, false, true);
        scheduleDraft($form);
    }

    function desactivarBalanzaManual($form) {
        var state = getState($form);
        state.balanzaDesactivadaManual = true;
        $form.find('#chkBalanzaLinea').prop('checked', false);
        detenerLecturaBalanza($form);
        clearWarning($form);
        syncCantidadReadonly($form);
        scheduleDraft($form);
    }

    function syncBalanzaConProducto($form, allowFocusManual) {
        var state = getState($form);
        var pesable = esPesableActual($form);

        if (!pesable) {
            state.focusAgregarPendiente = false;
            $form.find('#txtCantKgs').val('');
            syncCantidadReadonly($form);
            if (allowFocusManual) {
                focusCantidadManual($form);
            }
            return;
        }

        if (state.balanzaDesactivadaManual) {
            state.focusAgregarPendiente = false;
            $form.find('#chkBalanzaLinea').prop('checked', false);
            syncCantidadReadonly($form);
            if (allowFocusManual) {
                focusCantidadManual($form);
            }
            return;
        }

        state.focusAgregarPendiente = !!allowFocusManual;
        iniciarLecturaBalanza($form, true, !!allowFocusManual);
    }

    function loadProductByCode($form, codigo, focusCantidad) {
        var state = getState($form);
        state.productoRequestSeq += 1;
        var requestSeq = state.productoRequestSeq;

        $.getJSON(state.config.urls.buscarCortePorCodigo, { codigo: codigo })
            .done(function (res) {
                if (requestSeq !== state.productoRequestSeq) {
                    return;
                }

                if (!res || res.ok !== true) {
                    clearProductoInputs($form, true);
                    $form.find('#txtProductoNombre').val('No existe o sin coincidencia');
                    if (!focusCantidad) {
                        focusCodigo($form);
                    }
                    return;
                }

                setProductoActual($form, res);
                syncBalanzaConProducto($form, !!focusCantidad);
                if (focusCantidad && !esPesableActual($form)) {
                    $form.find('#txtCantKgs').focus().select();
                } else if (!focusCantidad) {
                    focusCodigo($form);
                }
            })
            .fail(function () {
                if (requestSeq !== state.productoRequestSeq) {
                    return;
                }
                clearProductoInputs($form, true);
                $form.find('#txtProductoNombre').val('No existe o sin coincidencia');
                if (!focusCantidad) {
                    focusCodigo($form);
                }
            });
    }

    function loadProductsModal($form, filtro) {
        var state = getState($form);
        var $modal = $(state.config.modalProductoSelector);
        var $tbody = $modal.find('.js-buscar-producto-tbody');
        $tbody.html('<tr><td colspan="3" class="text-center text-muted">Cargando...</td></tr>');

        $.getJSON(state.config.urls.buscarCorte, { q: filtro || '' })
            .done(function (items) {
                if (!$.isArray(items) || !items.length) {
                    $tbody.html('<tr><td colspan="3" class="text-center text-muted">Sin resultados.</td></tr>');
                    return;
                }

                var html = '';
                $.each(items, function (_, item) {
                    html += '<tr class="js-buscar-producto-row"'
                        + ' data-id="' + escapeHtml(item.id) + '"'
                        + ' data-codigo="' + escapeHtml(item.codigo) + '"'
                        + ' data-nombre="' + escapeHtml(item.nombre) + '"'
                        + ' data-pesable="' + (item.pesable ? 'true' : 'false') + '"'
                        + ' data-tipo="' + escapeHtml(item.tipo || '') + '"'
                        + ' data-promedio="' + escapeHtml(item.promedio || 0) + '">'
                        + '<td>' + escapeHtml(item.codigo) + '</td>'
                        + '<td>' + escapeHtml(item.nombre) + '</td>'
                        + '<td class="text-right text-muted">' + (item.pesable ? 'Pesable' : 'Unidad') + '</td>'
                        + '</tr>';
                });

                $tbody.html(html);
            })
            .fail(function () {
                $tbody.html('<tr><td colspan="3" class="text-center text-danger">No se pudo cargar el listado.</td></tr>');
            });
    }

    function selectProductFromModal($form, $row) {
        if (!$row || !$row.length) return;

        setProductoActual($form, {
            id: $row.data('id'),
            codigo: $row.data('codigo'),
            nombre: $row.data('nombre'),
            pesable: String($row.data('pesable')) === 'true',
            tipo: $row.data('tipo'),
            promedio: $row.data('promedio')
        });

        syncBalanzaConProducto($form, true);
        var state = getState($form);
        $(state.config.modalProductoSelector).modal('hide');
        if (!esPesableActual($form)) {
            $form.find('#txtCantKgs').focus().select();
        }
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
        window.setTimeout(function () {
            $('#filtroPersona').focus().select();
        }, 80);
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
                $('#contenedorModalPersonaStock').html(html);
                mostrarProveedorModal($form);
            })
            .fail(function () {
                window.ModalRequestLoading && window.ModalRequestLoading.hide();
                showWarning($form, 'No se pudo cargar el buscador de proveedores.');
            })
            .always(function () {
                state.loadingPersonaModal = false;
            });
    }

    function fechaHoraActualTexto() {
        var ahora = new Date();
        var dd = String(ahora.getDate()).padStart(2, '0');
        var mm = String(ahora.getMonth() + 1).padStart(2, '0');
        var yyyy = ahora.getFullYear();
        var hh = String(ahora.getHours()).padStart(2, '0');
        var mi = String(ahora.getMinutes()).padStart(2, '0');
        return dd + '/' + mm + '/' + yyyy + ' ' + hh + ':' + mi;
    }

    function validarLinea($form) {
        var state = getState($form);
        var idCorte = parseInt($form.find('#txtProductoId').val(), 10) || 0;
        var cantidadTexto = $.trim($form.find('#txtCantKgs').val() || '');
        var parsedCantidad = parseNumber(cantidadTexto);
        var cantidad = parsedCantidad.value;

        if (idCorte <= 0) {
            showWarning($form, 'Seleccione un producto válido.');
            $form.find('#txtCodigoProducto').focus().select();
            return null;
        }

        if (cantidadTexto === '') {
            showWarning($form, 'Ingrese la cantidad.');
            $form.find('#txtCantKgs').focus().select();
            return null;
        }

        if (!parsedCantidad.ok) {
            showWarning($form, 'Ingrese una cantidad válida.');
            $form.find('#txtCantKgs').focus().select();
            return null;
        }

        if (state.config.esEgreso) {
            if (cantidad === 0) {
                showWarning($form, 'Ingrese una cantidad distinta de cero.');
                $form.find('#txtCantKgs').focus().select();
                return null;
            }
            if (cantidad > 0) {
                cantidad = cantidad * -1;
            }
        } else if (state.config.permiteCantidadNegativa) {
            if (cantidad === 0) {
                showWarning($form, 'Ingrese una cantidad distinta de cero.');
                $form.find('#txtCantKgs').focus().select();
                return null;
            }
        } else {
            if (cantidad <= 0) {
                showWarning($form, 'Ingrese una cantidad mayor a cero.');
                $form.find('#txtCantKgs').focus().select();
                return null;
            }
        }

        clearWarning($form);
        return {
            idCorte: idCorte,
            codigo: $form.find('#txtCodigoProducto').val(),
            producto: $form.find('#txtProductoNombre').val(),
            cantKgs: cantidad,
            balanza: $form.find('#chkBalanzaLinea').is(':checked') && esPesableActual($form),
            creadoTexto: fechaHoraActualTexto(),
            pesable: esPesableActual($form),
            noContado: false,
            idPesajeVinculado: 0,
            pesajeVinculadoTexto: ''
        };
    }

    function addLinea($form) {
        var state = getState($form);
        var linea = validarLinea($form);
        if (!linea) {
            window.BusquedaFeedback && window.BusquedaFeedback.beepError();
            return;
        }

        state.lineas.push(linea);
        renderLineas($form);
        scheduleDraft($form);
        // El pitido de exito (abajo) reemplaza al alert de "Agregado correctamente" -- ya no se
        // muestra en exito, solo queda el feedback sonoro. Los warnings/errores si se siguen viendo.
        window.BusquedaFeedback && window.BusquedaFeedback.beepExito();
        window.CapturaRespaldo && window.CapturaRespaldo.capturar('Stock');
        clearProductoInputs($form);
        $form.find('#txtCodigoProducto').focus();
    }

    function bindEvents($form) {
        var state = getState($form);
        var $modalNoCargados = getNoCargadosModal();
        var $modalPorcentajes = getPorcentajesModal();
        var $modalComprasPesaje = getComprasPesajeModal();
        var $modalVincularPesajes = getVincularPesajesModal();

        // Los handlers se registran por delegación sobre el form/modal para que sigan funcionando si la vista se carga dinámicamente.
        $form.off('.stock');
        $(document).off('.stock');
        $modalNoCargados.off('.stockModal');
        $modalPorcentajes.off('.stockPorcentaje');
        $modalComprasPesaje.off('.stockCompras');
        $modalVincularPesajes.off('.stockVincular');

        $form.on('input.stock change.stock', '#IdSucursal, #FechaCompra, #Observaciones, #CantMedias, #KgsMedias', function () {
            actualizarContextoNoCargados($form);
            scheduleDraft($form);
        });

        $form.on('click.stock', '#btnHabilitarTipoCompra', function () {
            var $select = $form.find('#TipoCompraVisual');
            $select.prop('disabled', false).focus();
        });

        $form.on('change.stock', '#TipoCompraVisual', function () {
            $form.find('#TipoCompra').val($(this).val() || '');
            syncTipoCompraUi($form);
            $(this).prop('disabled', true);
        });

        $form.on('blur.stock', '#TipoCompraVisual', function () {
            $(this).prop('disabled', true);
        });

        $form.on('input.stock', '#Observaciones', function () {
            autoResizeObservaciones($form);
        });

        $form.on('click.stock', '#btnBuscarProducto', function () {
            loadProductsModal($form, '');
            window.ModalRequestLoading && window.ModalRequestLoading.show('Cargando solicitud...');
            $(state.config.modalProductoSelector).modal('show');
        });

        $form.on('click.stock', '#btnBuscarProveedor', function () {
            abrirProveedorModal($form);
        });

        $form.on('click.stock', '#btnLimpiarProveedor', function () {
            setProveedor($form, {
                id: state.config.proveedorDefaultId || 0,
                razon: state.config.proveedorDefaultNombre || '',
                cuit: state.config.proveedorDefaultCuit || ''
            });
            clearCompraVinculadaPesaje($form);
            scheduleDraft($form);
        });

        $form.on('click.stock', '#btnVerAcumulados', function () {
            renderAcumulados($form);
            window.ModalRequestLoading && window.ModalRequestLoading.show('Cargando solicitud...');
            $('#modalAcumuladosStock').modal('show');
        });

        $form.on('click.stock', '#btnSeleccionarCompraPesaje', function () {
            resetFiltrosComprasPesaje();
            cargarComprasPesaje($form);
            window.ModalRequestLoading && window.ModalRequestLoading.show('Cargando solicitud...');
            $modalComprasPesaje.modal('show');
        });

        $form.on('click.stock', '#btnVincularPesajes', function () {
            resetFiltrosPesajesVinculables();
            cargarPesajesVinculables($form);
            window.ModalRequestLoading && window.ModalRequestLoading.show('Cargando solicitud...');
            $modalVincularPesajes.modal('show');
        });

        $form.on('click.stock', '#btnVerPorcentajePesaje', function () {
            var idCompra = parseInt($form.find('#IdCompra').val(), 10) || 0;
            var cantMedias = $.trim($form.find('#CantMedias').val() || '');
            var kgsMedias = $.trim($form.find('#KgsMedias').val() || '');

            if (idCompra <= 0 || !cantMedias || !kgsMedias) {
                showWarning($form, 'Ingrese KgsMedias y CantMedias, presione Guardar y vuelva a intentarlo.');
                return;
            }

            clearPorcentajeWarning();
            loadPorcentajesPesaje($form);
            window.ModalRequestLoading && window.ModalRequestLoading.show('Cargando solicitud...');
            $modalPorcentajes.modal('show');
        });

        $form.on('click.stock', '#btnProductosNoCargados', function () {
            cargarProductosNoCargados($form);
            window.ModalRequestLoading && window.ModalRequestLoading.show('Cargando solicitud...');
            $('#modalProductosNoCargadosStock').modal('show');
        });

        $modalPorcentajes.on('click.stockPorcentaje', '#btnGenerarAjustePesajeStock', function () {
            generarAjustePesaje($form);
        });

        $modalComprasPesaje.on('click.stockCompras', '[data-action="select-compra-pesaje"]', function () {
            seleccionarCompraPesaje($form, $(this));
        });

        $modalComprasPesaje.on('click.stockCompras', '[data-action="toggle-detalle-compra-pesaje"]', function () {
            toggleDetalleCompraPesaje($form, $(this));
        });

        $modalComprasPesaje.on('shown.bs.modal.stockCompras', function () {
            $(this).find('#txtFiltroProveedorComprasPesaje').focus().select();
        });

        $modalComprasPesaje.on('input.stockCompras change.stockCompras', '#txtFiltroProveedorComprasPesaje, #txtFechaDesdeComprasPesaje, #txtFechaHastaComprasPesaje', function () {
            scheduleCargaComprasPesaje($form);
        });

        $modalComprasPesaje.on('click.stockCompras', '#btnLimpiarFiltrosComprasPesaje', function () {
            resetFiltrosComprasPesaje();
            cargarComprasPesaje($form);
        });

        $modalVincularPesajes.on('click.stockVincular', '[data-action="toggle-detalle-vincular-pesaje"]', function () {
            toggleDetalleCompraPesaje($form, $(this));
        });

        $modalVincularPesajes.on('click.stockVincular', '[data-action="vincular-pesaje"]', function () {
            vincularPesaje($form, $(this));
        });

        $modalVincularPesajes.on('shown.bs.modal.stockVincular', function () {
            $(this).find('#txtFiltroProveedorVincularPesajes').focus().select();
        });

        $modalVincularPesajes.on('input.stockVincular change.stockVincular', '#txtFiltroProveedorVincularPesajes, #txtFechaDesdeVincularPesajes, #txtFechaHastaVincularPesajes', function () {
            scheduleCargaPesajesVinculables($form);
        });

        $modalVincularPesajes.on('click.stockVincular', '#btnLimpiarFiltrosVincularPesajes', function () {
            resetFiltrosPesajesVinculables();
            cargarPesajesVinculables($form);
        });

        $modalNoCargados.on('input.stockModal change.stockModal', '#txtBuscarNoCargadosStock, #filtroStockNoCargados', function () {
            renderNoCargados($form);
        });

        $modalNoCargados.on('change.stockModal', '#chkSeleccionarTodosNoCargados', function () {
            var checked = $(this).is(':checked');
            var seleccionados = getNoCargadosSeleccionados();
            $.each(getNoCargadosFiltrados($form), function (_, item) {
                var itemId = parseInt(item.idCorte, 10) || 0;
                if (itemId <= 0) return;
                if (checked) {
                    seleccionados[itemId] = true;
                } else {
                    delete seleccionados[itemId];
                }
            });
            setNoCargadosSeleccionados(seleccionados);
            $modalNoCargados.find('.js-no-cargado-check').prop('checked', checked);
            actualizarContadoresNoCargados($form);
        });

        $modalNoCargados.on('change.stockModal', '.js-no-cargado-check', function () {
            var itemId = parseInt($(this).data('id'), 10) || 0;
            var seleccionados = getNoCargadosSeleccionados();
            if (itemId > 0) {
                if ($(this).is(':checked')) {
                    seleccionados[itemId] = true;
                } else {
                    delete seleccionados[itemId];
                }
            }
            setNoCargadosSeleccionados(seleccionados);
            actualizarContadoresNoCargados($form);
        });

        $modalNoCargados.on('dblclick.stockModal', '#tablaNoCargadosStock tbody tr', function () {
            var $check = $(this).find('.js-no-cargado-check');
            if (!$check.length) return;
            $check.prop('checked', !$check.is(':checked')).trigger('change');
        });

        $modalNoCargados.on('shown.bs.modal.stockModal', function () {
            $(this).find('#txtBuscarNoCargadosStock').focus().select();
        });

        $modalNoCargados.on('click.stockModal', '#btnAgregarNoCargadosStockActual', function () {
            agregarProductosNoCargados($form, true);
        });

        $modalNoCargados.on('click.stockModal', '#btnAgregarNoCargadosSinStock', function () {
            agregarProductosNoCargados($form, false);
        });

        $form.on('input.stock', '#txtCodigoProducto', function () {
            var codigo = $.trim($form.find('#txtCodigoProducto').val() || '');
            window.clearTimeout(state.productoTimer);
            if (!codigo) {
                state.productoRequestSeq += 1;
                clearProductoInputs($form, true);
                return;
            }

            state.productoTimer = window.setTimeout(function () {
                loadProductByCode($form, codigo, false);
            }, 250);
        });

        $form.on('keydown.stock', '#txtCodigoProducto', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            var codigo = $.trim($form.find('#txtCodigoProducto').val() || '');
            if (!codigo) return;
            window.clearTimeout(state.productoTimer);
            loadProductByCode($form, codigo, true);
        });

        $form.on('keydown.stock', '#txtCantKgs', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            addLinea($form);
        });

        $form.on('click.stock', '#btnAgregarLineaStock', function () {
            addLinea($form);
        });

        $form.on('click.stock', '[data-action="remove-line"]', function () {
            var index = parseInt($(this).data('index'), 10);
            if (isNaN(index)) return;
            state.lineas.splice(index, 1);
            renderLineas($form);
            window.CapturaRespaldo && window.CapturaRespaldo.capturar('Stock');
            scheduleDraft($form);
        });

        $form.on('click.stock', '.js-stock-sort, #tablaLineasStock thead th[data-sort-key]', function (e) {
            e.preventDefault();
            var key = String($(this).data('sort-key') || '');
            if (!key) return;
            if (state.sortKey === key) {
                state.sortDirection = state.sortDirection === 'asc' ? 'desc' : 'asc';
            } else {
                state.sortKey = key;
                state.sortDirection = 'asc';
            }
            renderLineas($form);
        });

        $form.on('click.stock', '#btnToggleBusquedaLineasStock', function () {
            var $btn = $(this);
            var $input = $form.find('#filtroLineasStock');
            var mostrar = $input.hasClass('d-none');
            $input.toggleClass('d-none', !mostrar);
            $btn.attr('aria-expanded', mostrar ? 'true' : 'false');

            if (mostrar) {
                $input.focus();
            } else {
                $input.val('');
                state.searchText = '';
                renderLineas($form);
            }
        });

        $form.on('input.stock', '#filtroLineasStock', function () {
            state.searchText = $(this).val() || '';
            renderLineas($form);
        });

        $form.on('change.stock', '#chkBalanzaLinea', function () {
            if ($(this).is(':checked')) {
                activarBalanzaManual($form);
            } else {
                desactivarBalanzaManual($form);
            }
            syncBalanzaStatusVisibility($form);
            syncCantidadReadonly($form);
        });

        $form.on('submit.stock', function () {
            detenerLecturaBalanza($form);
            rebuildHiddenInputs($form);
            var kgsMedias = $form.find('#KgsMedias');
            if (kgsMedias.length) {
                var kgsMediasRaw = $.trim(kgsMedias.val() || '');
                if (kgsMediasRaw) {
                    kgsMedias.val(formatNumberForPost(kgsMediasRaw));
                }
            }
        });

        $form.closest('.stock-page').on('click.stock', '[data-action="restore-draft"]', function () {
            var draft = readDraft($form);
            if (!draft) return;
            applyDraft($form, draft);
            hideDraftBanner($form);
        });

        $form.closest('.stock-page').on('click.stock', '[data-action="clear-draft"]', function () {
            if (!window.confirm('Se eliminará el borrador local de este movimiento. ¿Continuar?')) return;
            clearDraft($form);
        });

        $(document)
            .on('input.stock', state.config.modalProductoSelector + ' .js-buscar-producto-input', function () {
                loadProductsModal($form, $(this).val() || '');
            })
            .on('click.stock', state.config.modalProductoSelector + ' .js-buscar-producto-row', function () {
                $(state.config.modalProductoSelector).find('.js-buscar-producto-row').removeClass('is-selected');
                $(this).addClass('is-selected');
            })
            .on('dblclick.stock', state.config.modalProductoSelector + ' .js-buscar-producto-row', function () {
                selectProductFromModal($form, $(this));
            })
            .on('keydown.stock', state.config.modalProductoSelector + ' .js-buscar-producto-input', function (e) {
                if (e.key !== 'Enter') return;
                e.preventDefault();
                var $modal = $(state.config.modalProductoSelector);
                var $target = $modal.find('.js-buscar-producto-row.is-selected').first();
                if (!$target.length) $target = $modal.find('.js-buscar-producto-row').first();
                selectProductFromModal($form, $target);
            })
            .on('input.stock', '#filtroPersona', function () {
                cargarProveedores($form, $(this).val() || '');
            })
            .on('click.stock', '#tablaPersonas tr.fila-persona', function () {
                $('#tablaPersonas tr.fila-persona').removeClass('is-selected');
                $(this).addClass('is-selected');
            })
            .on('dblclick.stock', '#tablaPersonas tr.fila-persona', function () {
                setProveedor($form, {
                    id: $(this).data('id'),
                    razon: $(this).data('razon'),
                    cuit: $(this).data('cuit')
                });
                clearCompraVinculadaPesaje($form);
                $('#modalBuscarPersona').modal('hide');
                scheduleDraft($form);
            })
            .on('keydown.stock', '#filtroPersona', function (e) {
                if (e.key !== 'Enter') return;
                e.preventDefault();
                var $target = $('#tablaPersonas tr.fila-persona.is-selected').first();
                if (!$target.length) $target = $('#tablaPersonas tr.fila-persona:first');
                if (!$target.length) return;
                setProveedor($form, {
                    id: $target.data('id'),
                    razon: $target.data('razon'),
                    cuit: $target.data('cuit')
                });
                clearCompraVinculadaPesaje($form);
                $('#modalBuscarPersona').modal('hide');
                scheduleDraft($form);
            });

        $(document).on('keydown.stock', function (e) {
            var tag = e.target && e.target.tagName ? e.target.tagName.toLowerCase() : '';
            var key = String(e.key || '').toLowerCase();

            if (e.key === 'F10' && tag !== 'textarea') {
                e.preventDefault();
                loadProductsModal($form, '');
                $(state.config.modalProductoSelector).modal('show');
                return;
            }

            if (state.config.esPesaje && e.key === 'F9' && tag !== 'textarea') {
                e.preventDefault();
                abrirProveedorModal($form);
                return;
            }

            if (e.altKey && !e.ctrlKey && !e.metaKey && !e.shiftKey && !e.repeat && key === 'enter') {
                // Antes: $(e.target).closest('.modal') -- solo protegia si el evento se originaba
                // DENTRO de un modal. No cubria el caso real (el foco quedo en un input de esta
                // pantalla, fuera de cualquier modal, con el modal igual abierto encima) -- mismo
                // bug que se encontro y corrigio en POS/Movimientos/Elaborados/Compras el mismo dia.
                if ($(".modal.show").length) return;
                if (window.Swal && typeof window.Swal.isVisible === 'function' && window.Swal.isVisible()) return;

                var $primaryAction = getPrimaryActionButton($form);
                if (!$primaryAction.length || $primaryAction.prop('disabled')) return;

                e.preventDefault();
                $primaryAction.trigger('click');
                return;
            }

            if (e.key === '*' && tag !== 'textarea') {
                e.preventDefault();
                if ($form.find('#chkBalanzaLinea').is(':checked')) {
                    desactivarBalanzaManual($form);
                } else {
                    activarBalanzaManual($form);
                }
            }
        });
    }

    window.StockUI = window.StockUI || {
        init: function (config) {
            var $form = $(config && config.formSelector ? config.formSelector : '#formStock');
            if (!$form.length) return;

            var state = buildState(config || {});
            state.lineas = $.isArray(config.initialLines) ? $.map(config.initialLines, function (linea) { return normalizeLinea(linea); }) : [];
            setState($form, state);
            renderLineas($form);
            bindEvents($form);
            syncTipoCompraUi($form);

            renderBalanzaStatus($form, null);
            syncBalanzaStatusVisibility($form);
            verificarBalanzaInicial($form);
            syncCantidadReadonly($form);
            autoResizeObservaciones($form);
            actualizarEstadoAjustePesaje($form, $.trim($('#stockEstadoAjusteTexto').text() || ''));
            syncPrimaryAction($form);
            if (readDraft($form)) {
                showDraftBanner($form);
            }

            $form.find('#txtCodigoProducto').focus();
        }
    };

    window.StockUI.syncPrimaryAction = syncPrimaryAction;
})(window, window.jQuery);
