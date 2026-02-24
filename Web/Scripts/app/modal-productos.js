(function () {

    function pickFirst(obj, keys, fallback) {
        for (var i = 0; i < keys.length; i++) {
            var k = keys[i];
            if (obj && obj[k] !== undefined && obj[k] !== null) return obj[k];
        }
        return fallback;
    }

    function escHtml(v) {
        return String(v == null ? '' : v)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function normalizarProducto(p) {
        p = p || {};
        var id = pickFirst(p, ['id', 'Id', 'idCorte', 'IdCorte'], null);
        var codigo = pickFirst(p, ['codigo', 'Codigo'], '');
        var nombre = pickFirst(p, ['nombre', 'Nombre', 'descripcion', 'Descripcion', 'corte', 'CorteDesc'], '');
        var precio = pickFirst(p, ['precio', 'Precio', 'precioKg', 'PrecioKg'], 0);

        return { id: id, codigo: codigo, nombre: nombre, precio: precio, raw: p };
    }

    function getState($modal) {
        var state = $modal.data('__buscarProductoState');
        if (!state) {
            state = {
                mostrarPrecio: String($modal.data('mostrar-precio')).toLowerCase() !== 'false',
                onSelect: null,
                debounceTimer: null,
                initialized: false
            };
            $modal.data('__buscarProductoState', state);
        }
        return state;
    }

    function getRefs($modal) {
        // Compatible con partial viejo (IDs) y nuevo (clases)
        var $input = $modal.find('.js-buscar-producto-input');
        if (!$input.length) $input = $modal.find('#filtroProducto');

        var $tbody = $modal.find('.js-buscar-producto-tbody');
        if (!$tbody.length) $tbody = $modal.find('#tablaProductos');

        return {
            $input: $input.first(),
            $tbody: $tbody.first()
        };
    }

    function getColspanMensajes(state) {
        return state.mostrarPrecio ? 3 : 2;
    }

    function aplicarVisibilidadPrecio($modal, state) {
        var mostrar = !!state.mostrarPrecio;

        // Header compatible (nuevo + viejo)
        var $thPrecio = $modal.find('.js-col-precio-header, .col-precio-header');
        if ($thPrecio.length) $thPrecio.toggle(mostrar);

        var refs = getRefs($modal);
        refs.$tbody.find('.col-precio-cell').toggle(mostrar);

        refs.$tbody.find('tr td[data-msg="1"]').attr('colspan', getColspanMensajes(state));
    }

    function renderTabla($modal, items) {
        var state = getState($modal);
        var refs = getRefs($modal);
        var $tbody = refs.$tbody;

        if (!items || !items.length) {
            $tbody.html(
                '<tr><td data-msg="1" colspan="' + getColspanMensajes(state) + '" class="text-center text-muted">No se encontraron productos</td></tr>'
            );
            return;
        }

        var html = '';

        for (var i = 0; i < items.length; i++) {
            var p = normalizarProducto(items[i]);

            var precioNum = parseFloat(String(p.precio == null ? 0 : p.precio).replace(',', '.'));
            var precioTexto = isNaN(precioNum) ? '0.00' : precioNum.toFixed(2);
            var precioData = isNaN(precioNum) ? 0 : precioNum;

            html += ''
                + '<tr '
                + 'data-id="' + escHtml(p.id != null ? p.id : '') + '" '
                + 'data-codigo="' + escHtml(p.codigo) + '" '
                + 'data-nombre="' + escHtml(p.nombre) + '" '
                + 'data-precio="' + escHtml(precioData) + '" '
                + 'style="cursor:pointer;">'
                + '    <td>' + escHtml(p.codigo) + '</td>'
                + '    <td>' + escHtml(p.nombre) + '</td>'
                + '    <td class="text-right col-precio-cell">$ ' + precioTexto + '</td>'
                + '</tr>';
        }

        $tbody.html(html);
        aplicarVisibilidadPrecio($modal, state);

        // marcar primero
        $tbody.find('tr:first').addClass('table-active');
    }

    function cargarProductos($modal, q) {
        var state = getState($modal);
        var refs = getRefs($modal);
        var $tbody = refs.$tbody;
        var apiUrl = $modal.data('api-url');

        if (!apiUrl) {
            $tbody.html(
                '<tr><td data-msg="1" colspan="' + getColspanMensajes(state) + '" class="text-center text-danger">No hay URL configurada para buscar productos</td></tr>'
            );
            return;
        }

        $.get(apiUrl, { q: q || '' })
            .done(function (items) {
                renderTabla($modal, items || []);
            })
            .fail(function (xhr) {
                var detalle = (xhr && xhr.status) ? (' (' + xhr.status + ')') : '';
                $tbody.html(
                    '<tr><td data-msg="1" colspan="' + getColspanMensajes(state) + '" class="text-center text-danger">Error al cargar productos' + detalle + '</td></tr>'
                );
            });
    }

    function getProductoDesdeRow($row) {
        return {
            id: $row.attr('data-id'),
            codigo: $row.attr('data-codigo'),
            nombre: $row.attr('data-nombre'),
            precio: $row.attr('data-precio')
        };
    }

    function esFilaMensaje($row) {
        return !$row.length || $row.find('td[data-msg="1"]').length > 0;
    }

    function seleccionarProductoDesdeRow($modal, $row) {
        if (!$row || !$row.length || esFilaMensaje($row)) return;

        var state = getState($modal);
        var producto = getProductoDesdeRow($row);

        // Caso reutilizable (alta/edición, etc.)
        if (typeof state.onSelect === 'function') {
            $modal.modal('hide');

            $modal.one('hidden.bs.modal.buscarProductoSelect', function () {
                state.onSelect(producto);
            });

            return;
        }

        // Caso default POS (compatibilidad)
        var codigo = producto.codigo;
        var $inputPOS = $('#inputCodigo');

        if (!$inputPOS.length) {
            $modal.modal('hide');
            return;
        }

        $inputPOS.val(codigo);
        $modal.modal('hide');

        $modal.one('hidden.bs.modal.buscarProductoSelect', function () {
            if (typeof window.terminarEscritura === 'function') {
                window.terminarEscritura(codigo, function () {
                    var $inputCantidad = $('#inputCantidad');
                    if ($inputCantidad.length) $inputCantidad.focus().select();
                });
            } else {
                $inputPOS.trigger('input');
                var $inputCantidad2 = $('#inputCantidad');
                if ($inputCantidad2.length) $inputCantidad2.focus().select();
            }
        });
    }

    function initModalIfNeeded($modal) {
        var state = getState($modal);
        if (state.initialized) return true;

        var refs = getRefs($modal);
        var $input = refs.$input;
        var $tbody = refs.$tbody;

        if (!$input.length || !$tbody.length) {
            console.error('El modal existe pero no contiene input/tbody esperados (clases nuevas o IDs viejos):', $modal.attr('id'));
            return false;
        }

        // Buscar en vivo
        $input.on('input.buscarProducto', function () {
            clearTimeout(state.debounceTimer);
            state.debounceTimer = setTimeout(function () {
                cargarProductos($modal, ($input.val() || '').trim());
            }, 250);
        });

        // Enter -> seleccionar activa (o primera)
        $input.on('keydown.buscarProducto', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();

                var $sel = $tbody.find('tr.table-active:first');
                var $row = $sel.length ? $sel : $tbody.find('tr:first');

                if ($row.length && !esFilaMensaje($row)) {
                    seleccionarProductoDesdeRow($modal, $row);
                }
            }
        });

        // Click -> marcar
        $tbody.on('click.buscarProducto', 'tr', function () {
            var $row = $(this);
            if (esFilaMensaje($row)) return;

            $tbody.find('tr').removeClass('table-active');
            $row.addClass('table-active');
        });

        // Doble click -> seleccionar
        $tbody.on('dblclick.buscarProducto', 'tr', function () {
            var $row = $(this);
            if (esFilaMensaje($row)) return;

            seleccionarProductoDesdeRow($modal, $row);
        });

        // Al abrir -> foco + limpiar + cargar
        $modal.on('shown.bs.modal.buscarProducto', function () {
            $input.val('');
            $tbody.empty();
            aplicarVisibilidadPrecio($modal, state);

            setTimeout(function () {
                $input.focus().select();
                cargarProductos($modal, '');
            }, 60);
        });

        state.initialized = true;
        return true;
    }

    /* =========================
       API pública reutilizable
       ========================= */
    window.abrirBuscarProductoModal = function (opts) {
        opts = opts || {};

        var modalSelector = opts.modalSelector || '#modalBuscarProducto';
        var $modal = $(modalSelector);

        if (!$modal.length) {
            console.error('No se encontró el modal de búsqueda de productos:', modalSelector);
            alert('No se encontró el modal de búsqueda de productos: ' + modalSelector);
            return;
        }

        var okInit = initModalIfNeeded($modal);
        if (!okInit) {
            alert('El modal existe pero su HTML no coincide con el script (input/tabla no encontrados).');
            return;
        }

        var state = getState($modal);

        state.mostrarPrecio = (typeof opts.mostrarPrecio === 'boolean')
            ? opts.mostrarPrecio
            : (String($modal.data('mostrar-precio')).toLowerCase() !== 'false');

        state.onSelect = (typeof opts.onSelect === 'function') ? opts.onSelect : null;

        aplicarVisibilidadPrecio($modal, state);
        $modal.modal('show');
    };

})();