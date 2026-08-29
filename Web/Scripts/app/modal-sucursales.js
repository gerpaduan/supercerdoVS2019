// Nombre de la sucursal recien elegida en la apertura actual del modal, o null si
// se cerro sin cambiarla. Se usa en hidden.bs.modal para decidir el cartel de
// feedback ("Cambio a X" vs "Continua en X") una sola vez, ya con el modal
// completamente cerrado (evita superponer el Swal con la animacion de cierre).
let sucursalRecienCambiada = null;

$(document).on("click", ".btnSucursal", function () {

    const $btn = $(this);
    const idSucursal = $btn.data("id");
    const nombreSucursal = $btn.data("nombre");

    // Sucursal actual: clickeable a proposito (para poder confirmar "me quedo
    // en esta" sin depender solo del boton "x"), pero no hace falta pegarle
    // al servidor -- ya es la que esta seteada.
    if ($btn.hasClass("btn-primary")) {
        sucursalRecienCambiada = null;
        $("#modalSucursales").modal("hide");
        return;
    }

    $.post(urlCambiarSucursal,
        { idSucursal: idSucursal },
        function (resp) {

            if (!resp.ok) {
                Swal.fire("Error", resp.msg || "No se pudo cambiar la sucursal", "error");
                return;
            }

            sucursalRecienCambiada = resp.sucursalNombre || nombreSucursal;

            // 1️⃣ actualizar texto en el dropdown
            $("#lblSucursalActual").text(sucursalRecienCambiada);

            // 2️⃣ marcar botones (mismo texto plano que el render inicial del
            // servidor: "Nombre (seguir acá)" solo en el activo, sin badge, para
            // que el nombre siga centrado con la misma fuente del boton).
            $(".btnSucursal")
                .removeClass("btn-primary")
                .addClass("btn-outline-secondary")
                .each(function () {
                    $(this).text($(this).data("nombre"));
                });

            $btn
                .removeClass("btn-outline-secondary")
                .addClass("btn-primary")
                .text(sucursalRecienCambiada + " (seguir acá)");

            // 3️⃣ cerrar modal
            $("#modalSucursales").modal("hide");

            // 4️⃣ (opcional) evento global
            $(document).trigger("sucursal:cambiada", [resp.idSucursal]);
        }
    );
});

$(document).on("click", "#lnkContinuarSucursalActual", function (e) {
    e.preventDefault();
    sucursalRecienCambiada = null;
    $("#modalSucursales").modal("hide");
});

$(document).on("shown.bs.modal", "#modalSucursales", function () {
    sucursalRecienCambiada = null;
});

$(document).on("hidden.bs.modal", "#modalSucursales", function () {
    if (!window.SaveSuccessAlert) return;

    if (sucursalRecienCambiada) {
        window.SaveSuccessAlert.show("Cambió a " + sucursalRecienCambiada + ".", { title: "Sucursal actualizada" });
    } else {
        const nombreActual = $("#lblSucursalActual").text().trim();
        if (nombreActual) {
            window.SaveSuccessAlert.show("Continúa en " + nombreActual + ".", { title: "Sucursal sin cambios" });
        }
    }

    sucursalRecienCambiada = null;
});
