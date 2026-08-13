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

    // El boton "Nueva persona" se oculta cuando compras.js marco esta apertura
    // como una compra embebida dentro de POS venta (feature deshabilitada ahi
    // a proposito, ver compras.js).
    if ($(this).data('origen-persona-buscar') === 'compra-embebida') {
        $('#btnNuevaPersonaDesdeBuscar').addClass('d-none');
        return;
    }
    $('#btnNuevaPersonaDesdeBuscar').removeClass('d-none');
});

// Click en "Nueva persona": cierra el buscador y abre el alta rapida.
$(document).on('click', '#btnNuevaPersonaDesdeBuscar', function () {
    if ($('#modalBuscarPersona').data('origen-persona-buscar') === 'compra-embebida') return;
    abrirModalCrearPersonaVenta();
});

function abrirModalCrearPersonaVenta() {
    var $modalBuscar = $('#modalBuscarPersona');
    $modalBuscar.data('suprimir-foco-codigo', true);

    $modalBuscar.one('hidden.bs.modal', function () {
        $modalBuscar.removeData('suprimir-foco-codigo');
        $('#modalPersonaCrearVentaBody').html('<div class="text-center text-muted py-4">Cargando...</div>');

        var abrir = function () {
            $('#modalPersonaCrearVenta').modal('show');
            $.get(window.api.persona.crear)
                .done(function (html) { $('#modalPersonaCrearVentaBody').html(html); })
                .fail(function () {
                    $('#modalPersonaCrearVentaBody').html('<div class="alert alert-danger mb-0">No se pudo abrir el formulario de alta.</div>');
                });
        };

        if (window.POSGuard) {
            if (!window.POSGuard.requestModalOpen('personaCrear', abrir)) return;
        } else {
            abrir();
        }
    });

    $modalBuscar.modal('hide');
}

// Submit del alta rapida: guarda la persona y la adjunta automaticamente
// como cliente de la venta en curso, sin recargar la pagina.
$(document).on('submit', '#formPersonaModal', function (e) {
    e.preventDefault();
    var $form = $(this);
    var $btn = $('#btnGuardarPersonaModal');
    $btn.prop('disabled', true);

    $.ajax({
        url: window.api.persona.guardarCrear,
        type: 'POST',
        data: $form.serialize(),
        success: function (resp) {
            if (!resp || !resp.success) {
                $('#personaModalError').removeClass('d-none').text((resp && resp.message) || 'No se pudo guardar la persona.');
                $btn.prop('disabled', false);
                return;
            }
            $('#modalPersonaCrearVenta').modal('hide');
            seleccionarPersona(resp.idPersona, resp.razonSocial, resp.identificacion);

            var nombre = resp.razonSocial || 'La persona';
            if (window.Swal && typeof window.Swal.fire === 'function') {
                window.Swal.fire({
                    icon: 'success',
                    title: 'Persona agregada',
                    text: nombre + ' se creó correctamente y quedó como cliente de la venta.',
                    timer: 2200,
                    timerProgressBar: true,
                    showConfirmButton: true,
                    confirmButtonText: 'OK'
                }).then(function () {
                    $(document).trigger('pos:foco-codigo');
                });
            } else {
                $(document).trigger('pos:foco-codigo');
            }
        },
        error: function () {
            $('#personaModalError').removeClass('d-none').text('No se pudo guardar la persona.');
            $btn.prop('disabled', false);
        }
    });
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
        const identificacion = $target.data('identificacion') || '';

        seleccionarPersona(idPersona, razonSocial, identificacion);
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
                    data-razon="${p.razonSocial}"
                    data-identificacion="${p.identificacion ?? ''}">
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
    let identificacion = $(this).data('identificacion') || '';
    seleccionarPersona(idPersona, razonSocial, identificacion);
});

function seleccionarPersona(idPersona, razonSocial, identificacion) {
    $('#idPersona').val(idPersona);
    $('#razonSocial').val(razonSocial);
    if (typeof window.setClienteIdentificacionVisual === 'function') {
        window.setClienteIdentificacionVisual(identificacion, razonSocial);
    }
    if (typeof window.actualizarAccesoHistorialPreciosCliente === 'function') {
        window.actualizarAccesoHistorialPreciosCliente(idPersona);
    }
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

    // El cierre es parte de la transicion hacia "Nueva persona": no devolvemos
    // el foco al codigo de barras, el modal de alta enfoca su propio campo.
    if ($(this).data('suprimir-foco-codigo')) return;

    $(document).trigger('pos:foco-codigo');

    // Refuerzo extra: cuando Bootstrap termina de devolver el foco,
    // nos aseguramos de volver al input de codigo del POS.
    setTimeout(function () {
        $(document).trigger('pos:foco-codigo');
    }, 0);
});
