$(document).on("click", ".btnSucursal", function () {

    const $btn = $(this);
    const idSucursal = $btn.data("id");
    const nombreSucursal = $btn.data("nombre");

    $.post(urlCambiarSucursal,
        { idSucursal: idSucursal },
        function (resp) {

            if (!resp.ok) {
                Swal.fire("Error", resp.msg || "No se pudo cambiar la sucursal", "error");
                return;
            }

            // 1️⃣ actualizar texto en el dropdown
            $("#lblSucursalActual").text(resp.sucursalNombre || nombreSucursal);

            // 2️⃣ marcar botones
            $(".btnSucursal")
                .removeClass("btn-primary")
                .addClass("btn-outline-secondary")
                .prop("disabled", false)
                .find(".badge").remove();

            $btn
                .removeClass("btn-outline-secondary")
                .addClass("btn-primary")
                .prop("disabled", true)
                .append(' <span class="badge badge-light ml-2">Actual</span>');

            // 3️⃣ cerrar modal
            $("#modalSucursales").modal("hide");

            // 4️⃣ (opcional) evento global
            $(document).trigger("sucursal:cambiada", [resp.idSucursal]);
        }
    );
});
