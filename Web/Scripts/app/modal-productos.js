(function () {

    let debounceTimer = null;

    const $modal = $('#modalBuscarProducto');
    const $input = $('#filtroProducto');
    const $tbody = $('#tablaProductos');


    /* =========================
       CARGA DE PRODUCTOS
       ========================= */

    function cargarProductos(q) {
        const apiUrl = $modal.data('api-url');

        $.get(apiUrl, { q: q || '' })
            .done(function (items) {
                renderTabla(items || []);
            })
            .fail(function () {
                $tbody.html(
                    '<tr><td colspan="3" class="text-center text-danger">Error al cargar productos</td></tr>'
                );
            });
    }

    function renderTabla(items) {

        if (!items.length) {
            $tbody.html(
                '<tr><td colspan="3" class="text-center text-muted">No se encontraron productos</td></tr>'
            );
            return;
        }

        let html = '';

        items.forEach(p => {
            html += `
                <tr data-codigo="${p.codigo}">
                    <td>${p.codigo}</td>
                    <td>${p.nombre}</td>
                    <td class="text-right">$ ${parseFloat(p.precio || 0).toFixed(2)}</td>
                </tr>`;
        });

        $tbody.html(html);

        // marcar primero
        $tbody.find('tr:first').addClass('table-active');
    }

    /* =========================
       SELECCIÓN
       ========================= */

    function seleccionarProducto(codigo) {

        const $inputPOS = $('#inputCodigo');
        if (!$inputPOS.length) return;

        $inputPOS.val(codigo);

        if (typeof window.terminarEscritura === 'function') {
            window.terminarEscritura(codigo);
        } else {
            $inputPOS.trigger('input');
        }

        $modal.modal('hide');

        const $inputCantidad = $('#inputCantidad');
        $inputCantidad.focus();
    }

    /* =========================
       EVENTOS
       ========================= */

    // Buscar en vivo
    $input.on('input', function () {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            cargarProductos($input.val().trim());
        }, 250);
    });

    // Enter → seleccionar primero
    $input.on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            const $first = $tbody.find('tr:first');
            if ($first.length) {
                seleccionarProducto($first.data('codigo'));
            }
        }
    });

    // Click → marcar
    $tbody.on('click', 'tr', function () {
        $tbody.find('tr').removeClass('table-active');
        $(this).addClass('table-active');
    });

    // Doble click → seleccionar
    $tbody.on('dblclick', 'tr', function () {
        seleccionarProducto($(this).data('codigo'));
    });

    // Al abrir el modal
    $modal.on('shown.bs.modal', function () {
        $input.val('');
        $tbody.empty();
        $input.focus();
        cargarProductos();
    });

})();