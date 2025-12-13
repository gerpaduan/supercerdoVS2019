// ===============================
// MODAL ALTA CHEQUE (REUTILIZABLE)
// ===============================

var ModalAltaCheque = {

    abrir: function (nroCheque, cheque = null) {

        $("#formCheque")[0].reset();

        $("#ChequeId").val(cheque?.Id || 0);
        $("#NroCheque").val(cheque?.NroCheque || nroCheque || "");
        $("#BancoId").val(cheque?.Banco || "");
        $("#Importe").val(cheque?.Importe || "");
        $("#FechaEmision").val(cheque?.FechaEmision || "");
        $("#FechaPago").val(cheque?.FechaPago || "");
        $("#Estado").val(cheque?.Estado || "PENDIENTE");
        $("#Propio").val(cheque?.Propio ? "true" : "false");
        $("#Titular").val(cheque?.Titular || "");
        $("#Observaciones").val(cheque?.Observaciones || "");

        $("#tituloModalCheque").text(cheque ? "Editar Cheque" : "Alta de Cheque");

        $("#modalAltaCheque").modal("show");
    }
};

$(document).ready(function () {

    // Cuando se hace clic en "Guardar Cheque" en el modal
    $("#btnGuardarCheque").on("click", function () {
        // NORMALIZAR IMPORTE
        let imp = $("#importe").val().trim();
        $("#importe").val(imp);

        let cheque = {
            Id: $("#ChequeId").val(),
            NroCheque: $("#NroCheque").val(),
            Banco: $("#Banco").val(),
            Importe: $("#Importe").val(),
            FechaEmision: $("#FechaEmision").val(),
            FechaPago: $("#FechaPago").val(),
            Estado: $("#Estado").val(),
            Propio: $("#Propio").val() === "true",
            Titular: $("#Titular").val(),
            Observaciones: $("#Observaciones").val()
        };

        $.post("/Finanzas/GuardarCheque", cheque, function (resp) {

            var mensajeValidacion = "";

            var Importe = $("#Importe").val();
            if (Importe === "" || Importe === null || Importe === "0") {
                mensajeValidacion += "\n-Ingrese un importe válido.";
                //Swal.fire("Atención", "Ingrese una fecha de pago.", "warning");
                //return false; // ❌ Bloquea el envío
            }

            var FechaPago = $("#FechaPago").val();
            if (FechaPago === "" || FechaPago === null) {
                mensajeValidacion += "\n-Ingrese una fecha de pago.";
                //Swal.fire("Atención", "Ingrese una fecha de pago.", "warning");
                //return false; // ❌ Bloquea el envío
            }

            if (mensajeValidacion != "") {
                Swal.fire({
                    title: "Error",
                    html: mensajeValidacion.replace(/\n/g, "<br>"),
                    icon: "error"
                });
                return false;
            }

            if (!resp.ok) {
                //Swal.fire("Error", resp.mensaje, "error");
                Swal.fire({
                    title: "Error",
                    html: resp.message,//.replace(/\n/g, "<br>"),
                    icon: "error"
                });
                return;
            }

            $("#modalAltaCheque").modal("hide");

                //// opcional → agregar directo a la tabla de pago
                //agregarChequeATabla(
                //    resp.cheque.Id,
                //    resp.cheque.NroCheque,
                //    resp.cheque.Banco,
                //    resp.cheque.FechaPago,
                //    resp.cheque.Importe
                //);
        });
    });

});
