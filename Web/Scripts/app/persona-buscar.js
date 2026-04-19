//// Abrir modal

// Buscar en vivo solo cuando cambia el texto.
// Si escuchamos keyup para todo, las flechas tambien disparan una nueva carga
// y se pierde la seleccion mientras el usuario navega la lista.
$(document).on('input', '#filtroPersona', function () {
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
    // Marcamos que hubo una seleccion real para que el POS pueda decidir
    // acciones posteriores cuando el modal termine de cerrarse.
    $('#modalBuscarPersona').data('persona-seleccionada', true);
    $('#modalBuscarPersona').modal('hide');
}

// Cuando el modal termina de cerrarse, devolvemos el foco al codigo del POS.
// Si hubo seleccion, el comportamiento es el mismo; si se cerro sin elegir,
// tambien volvemos al flujo principal de carga.
$(document).on('hidden.bs.modal', '#modalBuscarPersona', function () {
    $('#filtroPersona').val('');
    $('#tablaPersonas').empty();
    $(this).removeData('persona-seleccionada');
    $(document).trigger('pos:foco-codigo');

    // Refuerzo extra: cuando Bootstrap termina de devolver el foco,
    // nos aseguramos de volver al input de codigo del POS.
    setTimeout(function () {
        $(document).trigger('pos:foco-codigo');
    }, 0);
});
