//// Abrir modal
//$('#btnBuscarPersona').on('click', function () {

//    $('#contenedorModalPersona').load('/Personas/Buscar', function () {
//        $('#modalBuscarPersona').modal('show');
//        cargarPersonas();
//    });

//});

// Buscar en vivo
$(document).on('keyup', '#filtroPersona', function () {
    cargarPersonas();
});


// Auto-focus al abrir modal
$(document).on('shown.bs.modal', '#modalBuscarPersona', function () {
    $('#filtroPersona').focus();
});

// Enter selecciona la primera persona
$(document).on('keydown', '#filtroPersona', function (e) {
    if (e.key === 'Enter') {
        $('#tablaPersonas button:first').click();
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
    });
}

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

