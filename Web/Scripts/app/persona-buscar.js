//// Abrir modal

// Buscar en vivo
$(document).on('keyup', '#filtroPersona', function (e) {
    // Evitar que Enter dispare cargar de nuevo
    if (e.key === 'Enter') return;
    cargarPersonas();
});

// Auto-focus al abrir modal
$(document).on('shown.bs.modal', '#modalBuscarPersona', function () {
    $('#filtroPersona').focus().select();
    // opcional: cargar listado inicial
    cargarPersonas();
});

// Navegación + Enter selecciona primera/seleccionada
$(document).on('keydown', '#filtroPersona', function (e) {

    const $rows = $('#tablaPersonas tr.fila-persona');
    if (!$rows.length) return;

    // Si no hay seleccionada, seleccionamos la primera
    let $sel = $rows.filter('.is-selected').first();
    if (!$sel.length) {
        $rows.removeClass('is-selected');
        $sel = $rows.first().addClass('is-selected');
    }

    // ↓
    if (e.key === 'ArrowDown') {
        e.preventDefault();
        const $next = $sel.next('.fila-persona');
        if ($next.length) {
            $sel.removeClass('is-selected');
            $next.addClass('is-selected');
            $next[0].scrollIntoView({ block: 'nearest' });
        }
        return;
    }

    // ↑
    if (e.key === 'ArrowUp') {
        e.preventDefault();
        const $prev = $sel.prev('.fila-persona');
        if ($prev.length) {
            $sel.removeClass('is-selected');
            $prev.addClass('is-selected');
            $prev[0].scrollIntoView({ block: 'nearest' });
        }
        return;
    }

    // Enter => seleccionar la fila seleccionada (o la primera)
    if (e.key === 'Enter') {
        e.preventDefault();

        const $target = $('#tablaPersonas tr.fila-persona.is-selected').first().length
            ? $('#tablaPersonas tr.fila-persona.is-selected').first()
            : $('#tablaPersonas tr.fila-persona').first();

        if (!$target.length) return;

        const idPersona = $target.data('id');
        const razonSocial = $target.data('razon');

        seleccionarPersona(idPersona, razonSocial);
        return;
    }
});

function cargarPersonas() {
    let filtro = $('#filtroPersona').val();

    $.get(window.api.persona.listar, { filtro: filtro }, function (data) {

        let html = '';

        data.forEach(p => {
            html += `
                <tr class="fila-persona"
                    data-id="${p.idPersona}"
                    data-razon="${p.razonSocial}">
                    <td>${p.cuit ?? ''}</td>
                    <td>${p.razonSocial}</td>
                    <td>${p.identificacion ?? ''}</td>
                </tr>
            `;
        });

        $('#tablaPersonas').html(html);

        // ✅ deja seleccionada la primera fila al cargar
        const $first = $('#tablaPersonas tr.fila-persona').first();
        if ($first.length) $first.addClass('is-selected');
    });
}

// Click simple: marcar selección
$(document).on('click', '#tablaPersonas tr.fila-persona', function () {
    $('#tablaPersonas tr.fila-persona').removeClass('is-selected');
    $(this).addClass('is-selected');
});

// Doble click: seleccionar
$(document).on('dblclick', '#tablaPersonas tr.fila-persona', function () {
    let idPersona = $(this).data('id');
    let razonSocial = $(this).data('razon');
    seleccionarPersona(idPersona, razonSocial);
});

function seleccionarPersona(idPersona, razonSocial) {
    $('#idPersona').val(idPersona);
    $('#razonSocial').val(razonSocial);
    $('#modalBuscarPersona').modal('hide');
}
