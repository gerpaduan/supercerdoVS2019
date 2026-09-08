// Port de Web/Scripts/app/modal-sucursales.js (2026-09-07, pedido explicito del usuario -- ver
// docs/DECISIONS.md). Misma logica -- unico cambio real: el clasico usa window.SaveSuccessAlert
// (helper propio, Web/Scripts/app/save-success-alert.js, NO portado a WebCore todavia) para el
// cartel final; aca se usa Swal.fire directo (toast chico), ya disponible globalmente.
let sucursalRecienCambiada = null;

$(document).on("click", ".btnSucursal", function () {
    const $btn = $(this);
    const idSucursal = $btn.data("id");
    const nombreSucursal = $btn.data("nombre");

    // Sucursal actual: clickeable a proposito (para poder confirmar "me quedo en esta" sin
    // depender solo del boton "x"), pero no hace falta pegarle al servidor -- ya es la que esta seteada.
    if ($btn.hasClass("btn-primary")) {
        sucursalRecienCambiada = null;
        $("#modalSucursales").modal("hide");
        return;
    }

    $.post(window.urlCambiarSucursal, { idSucursal: idSucursal }, function (resp) {
        if (!resp || !resp.ok) {
            Swal.fire({ icon: "error", title: "Error", text: (resp && resp.msg) || "No se pudo cambiar la sucursal" });
            return;
        }

        sucursalRecienCambiada = resp.sucursalNombre || nombreSucursal;

        $("#lblSucursalActual").text(sucursalRecienCambiada);

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

        $("#modalSucursales").modal("hide");

        $(document).trigger("sucursal:cambiada", [resp.idSucursal]);
    });
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
    if (!window.Swal) return;

    if (sucursalRecienCambiada) {
        Swal.fire({ toast: true, position: "top-end", icon: "success", title: "Cambió a " + sucursalRecienCambiada + ".", showConfirmButton: false, timer: 1800, timerProgressBar: true });
    } else {
        const nombreActual = $("#lblSucursalActual").text().trim();
        if (nombreActual) {
            Swal.fire({ toast: true, position: "top-end", icon: "info", title: "Continúa en " + nombreActual + ".", showConfirmButton: false, timer: 1500, timerProgressBar: true });
        }
    }

    sucursalRecienCambiada = null;
});
