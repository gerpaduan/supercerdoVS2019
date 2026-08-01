(function () {
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

    function formatKg(value) {
        return toFloat(value).toLocaleString('es-AR', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
    }

    function formatMoney(value) {
        return toFloat(value).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function showWarning(text) {
        var $warning = $('#puntoExpendioWarning');
        if (!$warning.length) return;
        $warning.text(text || '').removeClass('d-none');
    }

    function clearWarning() {
        var $warning = $('#puntoExpendioWarning');
        if (!$warning.length) return;
        $warning.addClass('d-none').text('');
    }

    function initPage() {
        var $page = $('[data-punto-expendio-page="1"]');
        if (!$page.length) return;

        var config = window.puntoExpendioConfig || {};
        var esGuardado = !!config.esGuardado;
        var permiteEditarPrecio = !!config.permiteEditarPrecio;
        var state = {
            lines: Array.isArray(window.puntoExpendioLineasIniciales) ? window.puntoExpendioLineasIniciales.slice() : []
        };

        function setProducto(producto) {
            $('#txtProductoIdExpendio').val(producto.id || '');
            $('#txtCodigoProductoExpendio').val(producto.codigo || '');
            $('#txtProductoNombreExpendio').val(producto.nombre || '');
            if (!permiteEditarPrecio) {
                $('#txtPrecioKgExpendio').val(producto.precio !== undefined ? formatMoney(producto.precio) : '');
            }
        }

        function clearProducto() {
            $('#txtProductoIdExpendio').val('');
            $('#txtCodigoProductoExpendio').val('');
            $('#txtProductoNombreExpendio').val('');
            if (!permiteEditarPrecio) {
                $('#txtPrecioKgExpendio').val('');
            }
            $('#txtCantKgsExpendio').val('');
        }

        function renderLines() {
            var $tbody = $('#tablaLineasPuntoExpendio tbody');
            if (!$tbody.length) return;

            if (!state.lines.length) {
                $tbody.html('<tr><td colspan="7" class="text-center text-muted">No hay productos cargados.</td></tr>');
                updateTotals();
                return;
            }

            var html = '';
            for (var i = 0; i < state.lines.length; i++) {
                var item = state.lines[i] || {};
                html += ''
                    + '<tr data-index="' + i + '">'
                    + '<td>' + (item.codigo || '') + '</td>'
                    + '<td>' + (item.producto || '') + '</td>'
                    + '<td class="text-right">' + formatKg(item.cantKg) + '</td>'
                    + '<td class="text-right">$ ' + formatMoney(item.precioKg) + '</td>'
                    + '<td class="text-right">$ ' + formatMoney(item.total) + '</td>'
                    + '<td class="text-center">' + (item.pesoBalanza ? 'Sí' : 'No') + '</td>'
                    + '<td>'
                    + '<button type="button" class="btn btn-sm btn-outline-danger js-remove-linea-expendio" data-index="' + i + '">'
                    + '<i class="fas fa-trash"></i>'
                    + '</button>'
                    + '</td>'
                    + '</tr>';
            }

            $tbody.html(html);
            updateTotals();
        }

        function updateTotals() {
            var totalItems = state.lines.length;
            var totalKg = 0;
            var totalImporte = 0;

            for (var i = 0; i < state.lines.length; i++) {
                totalKg += toFloat(state.lines[i].cantKg);
                totalImporte += toFloat(state.lines[i].total);
            }

            $('#lblTotalItemsExpendio').text(totalItems);
            $('#lblTotalKilosExpendio').text(formatKg(totalKg));
            $('#lblTotalImporteExpendio').text('$ ' + formatMoney(totalImporte));
        }

        function buildHiddenInputs() {
            var $container = $('#lineasHiddenContainer');
            if (!$container.length) return;

            $container.empty();

            function appendHidden(name, value) {
                $('<input>', {
                    type: 'hidden',
                    name: name,
                    value: value
                }).appendTo($container);
            }

            for (var i = 0; i < state.lines.length; i++) {
                var item = state.lines[i] || {};
                appendHidden('Lineas[' + i + '].IdCorte', item.idCorte || 0);
                appendHidden('Lineas[' + i + '].Codigo', item.codigo || 0);
                appendHidden('Lineas[' + i + '].Producto', item.producto || '');
                appendHidden('Lineas[' + i + '].CantKg', String(toFloat(item.cantKg)).replace('.', ','));
                appendHidden('Lineas[' + i + '].PrecioKg', String(toFloat(item.precioKg)).replace('.', ','));
                appendHidden('Lineas[' + i + '].PesoBalanza', item.pesoBalanza ? 'true' : 'false');
                appendHidden('Lineas[' + i + '].Total', String(toFloat(item.total)).replace('.', ','));
            }
        }

        function addLine() {
            clearWarning();

            var idCorte = toInt($('#txtProductoIdExpendio').val());
            var codigo = toInt($('#txtCodigoProductoExpendio').val());
            var producto = $.trim($('#txtProductoNombreExpendio').val() || '');
            var cantKg = parseDecimal($('#txtCantKgsExpendio').val());
            var precioKg = parseDecimal($('#txtPrecioKgExpendio').val());
            var pesoBalanza = $('#chkBalanzaLinea').is(':checked');

            if (idCorte <= 0 || !producto) {
                window.BusquedaFeedback && window.BusquedaFeedback.beepError();
                showWarning('Debe seleccionar un producto válido.');
                return;
            }

            if (!cantKg.ok || cantKg.value <= 0) {
                window.BusquedaFeedback && window.BusquedaFeedback.beepError();
                showWarning('Debe ingresar una cantidad en kilos mayor a cero.');
                return;
            }

            if (!precioKg.ok || precioKg.value <= 0) {
                window.BusquedaFeedback && window.BusquedaFeedback.beepError();
                showWarning('Debe ingresar un precio por kilo mayor a cero.');
                return;
            }

            state.lines.push({
                idCorte: idCorte,
                codigo: codigo,
                producto: producto,
                cantKg: cantKg.value,
                precioKg: precioKg.value,
                pesoBalanza: pesoBalanza,
                total: cantKg.value * precioKg.value
            });

            renderLines();
            window.BusquedaFeedback && window.BusquedaFeedback.beepExito();
            clearProducto();
            $('#txtCodigoProductoExpendio').trigger('focus');
        }

        function buscarProductoPorCodigo() {
            clearWarning();

            var codigo = toInt($('#txtCodigoProductoExpendio').val());
            if (codigo <= 0 || !config.urlBuscarProductoPorCodigo) return;

            $.get(config.urlBuscarProductoPorCodigo, { codigo: codigo })
                .done(function (resp) {
                    if (!resp || !resp.ok) {
                        showWarning(resp && resp.mensaje ? resp.mensaje : 'No se encontró el producto.');
                        return;
                    }

                    setProducto(resp);
                    $('#txtCantKgsExpendio').trigger('focus').select();
                })
                .fail(function () {
                    showWarning('No se pudo buscar el producto por código.');
                });
        }

        function openSectorModal() {
            $('#modalSectoresPuntoExpendio').modal('show');
            setTimeout(function () {
                $('#txtBuscarSectorExpendio').trigger('focus').select();
            }, 80);
        }

        function filtrarSectores() {
            var texto = ($('#txtBuscarSectorExpendio').val() || '').toLowerCase();
            $('#listaSectoresExpendio .js-sector-item').each(function () {
                var $item = $(this);
                var match = !texto || ($item.text() || '').toLowerCase().indexOf(texto) >= 0;
                $item.toggle(match);
            });
        }

        if (!esGuardado) {
            renderLines();

            $(document)
                .off('click.puntoExpendioBuscarProducto', '#btnBuscarProductoExpendio')
                .on('click.puntoExpendioBuscarProducto', '#btnBuscarProductoExpendio', function () {
                    if (typeof window.abrirBuscarProductoModal !== 'function') return;
                    window.abrirBuscarProductoModal({
                        modalSelector: '#modalBuscarProductoPuntoExpendio',
                        mostrarPrecio: true,
                        onSelect: function (producto) {
                            setProducto(producto || {});
                            $('#txtCantKgsExpendio').trigger('focus').select();
                        }
                    });
                })
                .off('click.puntoExpendioAgregar', '#btnAgregarLineaExpendio')
                .on('click.puntoExpendioAgregar', '#btnAgregarLineaExpendio', function () {
                    addLine();
                })
                .off('click.puntoExpendioRemove', '.js-remove-linea-expendio')
                .on('click.puntoExpendioRemove', '.js-remove-linea-expendio', function () {
                    var index = toInt($(this).data('index'));
                    if (index < 0 || index >= state.lines.length) return;
                    state.lines.splice(index, 1);
                    renderLines();
                })
                .off('blur.puntoExpendioCodigo', '#txtCodigoProductoExpendio')
                .on('blur.puntoExpendioCodigo', '#txtCodigoProductoExpendio', function () {
                    if (!$('#txtProductoIdExpendio').val()) {
                        buscarProductoPorCodigo();
                    }
                })
                .off('keydown.puntoExpendioCodigo', '#txtCodigoProductoExpendio')
                .on('keydown.puntoExpendioCodigo', '#txtCodigoProductoExpendio', function (e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        buscarProductoPorCodigo();
                    }
                })
                .off('keydown.puntoExpendioCant', '#txtCantKgsExpendio, #txtPrecioKgExpendio')
                .on('keydown.puntoExpendioCant', '#txtCantKgsExpendio, #txtPrecioKgExpendio', function (e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        addLine();
                    }
                })
                .off('submit.puntoExpendio', '#formPuntoExpendio')
                .on('submit.puntoExpendio', '#formPuntoExpendio', function () {
                    buildHiddenInputs();
                });
        }

        $(document)
            .off('click.puntoExpendioSector', '#btnCambiarSector, #btnSeleccionarSectorInicial')
            .on('click.puntoExpendioSector', '#btnCambiarSector, #btnSeleccionarSectorInicial', function () {
                openSectorModal();
            })
            .off('input.puntoExpendioSectores', '#txtBuscarSectorExpendio')
            .on('input.puntoExpendioSectores', '#txtBuscarSectorExpendio', function () {
                filtrarSectores();
            })
            .off('click.puntoExpendioSelectSector', '.js-sector-item')
            .on('click.puntoExpendioSelectSector', '.js-sector-item', function () {
                var sector = $(this).data('sector');
                if (!sector || !config.urlAbrir) return;
                window.location.href = config.urlAbrir + '?sector=' + encodeURIComponent(sector);
            })
            .off('click.puntoExpendioImprimir', '#btnImprimirPuntoExpendio')
            .on('click.puntoExpendioImprimir', '#btnImprimirPuntoExpendio', function () {
                var url = $(this).data('imprimir-url');
                if (!url) return;
                window.open(url + (url.indexOf('?') >= 0 ? '&' : '?') + 'mm=58', '_blank', 'noopener');
            });

        if (!esGuardado && !config.sectorSeleccionado && $('#modalSectoresPuntoExpendio').length && $('#listaSectoresExpendio .js-sector-item').length) {
            openSectorModal();
        }
    }

    $(initPage);
})();
