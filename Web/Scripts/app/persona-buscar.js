// Abrir modal
$('#btnBuscarPersona').on('click', function () {

    $('#contenedorModalPersona').load('/Personas/Buscar', function () {
        $('#modalBuscarPersona').modal('show');
        cargarPersonas();
    });

});

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


    //$.get(personaUrls.listar, { filtro: filtro }, function (data) {
    $.get('/Personas/Listar', { filtro: filtro }, function (data) {

        let html = '';

        data.forEach(p => {
            html += `
                <tr>
                    <td>${p.razonSocial}</td>
                    <td>${p.cuit ?? ''}</td>
                    <td>${p.identificacion ?? ''}</td>
                    <td class="text-right">
                        <button class="btn btn-sm btn-success"
                                onclick="seleccionarPersona(${p.idPersona}, '${p.razonSocial.replace(/'/g, "\\'")}')">
                            <i class="fas fa-check"></i>
                        </button>
                    </td>
                `;
        });

        $('#tablaPersonas').html(html);
    });
}

function seleccionarPersona(idPersona, razonSocial) {
    $('#idPersona').val(idPersona);
    $('#txtPersona').val(razonSocial);
    $('#modalBuscarPersona').modal('hide');
}
