// ===============================
// MODAL ALTA / EDICIÓN CHEQUE
// ===============================
(function (window, $) {

    if (window.ModalAltaCheque && window.ModalAltaCheque.__inicializado) {
        return;
    }

    window.ModalAltaCheque = (function () {

        function abrir(nroCheque = "", cheque = null) {
            var form = document.getElementById("formCheque");
            if (form) {
                form.reset();
            }

            $("#ChequeId").val(cheque?.Id ?? 0);
            $("#NroCheque").val(cheque?.NroCheque ?? nroCheque);
            $("#Banco").val(cheque?.Banco ?? "");
            $("#Importe").val(cheque?.Importe ?? "");
            $("#FechaEmision").val(cheque?.FechaEmision ?? "");
            $("#FechaPago").val(cheque?.FechaPago ?? "");
            $("#Estado").val(cheque?.Estado ?? "PENDIENTE");
            $("#Propio").val(cheque?.Propio ? "true" : "false");
            $("#Titular").val(cheque?.Titular ?? "");
            $("#Observaciones").val(cheque?.Observaciones ?? "");

            $("#tituloModalCheque").text(
                cheque ? "Editar Cheque" : "Alta de Cheque"
            );

            $("#modalAltaCheque").modal("show");
        }

        function validar() {
            let errores = [];

            let importe = $("#Importe").val();
            if (!importe || importe === "0") {
                errores.push("Ingrese un importe válido.");
            }

            let fechaPago = $("#FechaPago").val();
            if (!fechaPago) {
                errores.push("Ingrese una fecha de pago.");
            }

            if (errores.length > 0) {
                Swal.fire({
                    title: "Error",
                    html: errores.join("<br>"),
                    icon: "error"
                });
                return false;
            }

            return true;
        }

        function guardar() {
            if (!validar()) return;

            var urlGuardarCheque = window.urls && window.urls.guardarCheque
                ? window.urls.guardarCheque
                : null;

            if (!urlGuardarCheque) {
                Swal.fire("Error", "No está configurada la URL para guardar el cheque.", "error");
                return;
            }

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

            $.post(urlGuardarCheque, cheque)
                .done(function (resp) {
                    if (!resp.ok) {
                        Swal.fire("Error", resp.message || resp.mensaje || "No se pudo guardar el cheque.", "error");
                        return;
                    }

                    $("#modalAltaCheque").modal("hide");

                    $(document).trigger("cheque:guardado", resp);
                    $(document).trigger("chequeCreado", [resp.cheque || cheque]);

                    Swal.fire("OK", "Cheque guardado correctamente", "success");
                })
                .fail(function () {
                    Swal.fire("Error", "No se pudo guardar el cheque", "error");
                });
        }

        return {
            abrir: abrir,
            guardar: guardar,
            __inicializado: true
        };

    })();

    $(document)
        .off("click.modalAltaCheque", "#btnGuardarCheque")
        .on("click.modalAltaCheque", "#btnGuardarCheque", function () {
            window.ModalAltaCheque.guardar();
        });

})(window, jQuery);
//const ModalAltaCheque = (function () {

//    const urlGuardarCheque = window.urls.guardarCheque;

//    function abrir(nroCheque = "", cheque = null) {

//        $("#formCheque")[0].reset();

//        $("#ChequeId").val(cheque?.Id ?? 0);
//        $("#NroCheque").val(cheque?.NroCheque ?? nroCheque);
//        $("#Banco").val(cheque?.Banco ?? "");
//        $("#Importe").val(cheque?.Importe ?? "");
//        $("#FechaEmision").val(cheque?.FechaEmision ?? "");
//        $("#FechaPago").val(cheque?.FechaPago ?? "");
//        $("#Estado").val(cheque?.Estado ?? "PENDIENTE");
//        $("#Propio").val(cheque?.Propio ? "true" : "false");
//        $("#Titular").val(cheque?.Titular ?? "");
//        $("#Observaciones").val(cheque?.Observaciones ?? "");

//        $("#tituloModalCheque").text(
//            cheque ? "Editar Cheque" : "Alta de Cheque"
//        );

//        $("#modalAltaCheque").modal("show");
//    }

//    function validar() {

//        let errores = [];

//        let importe = $("#Importe").val();
//        if (!importe || importe === "0") {
//            errores.push("Ingrese un importe válido.");
//        }

//        let fechaPago = $("#FechaPago").val();
//        if (!fechaPago) {
//            errores.push("Ingrese una fecha de pago.");
//        }

//        if (errores.length > 0) {
//            Swal.fire({
//                title: "Error",
//                html: errores.join("<br>"),
//                icon: "error"
//            });
//            return false;
//        }

//        return true;
//    }

//    function guardar() {

//        if (!validar()) return;

//        let cheque = {
//            Id: $("#ChequeId").val(),
//            NroCheque: $("#NroCheque").val(),
//            Banco: $("#Banco").val(),
//            Importe: $("#Importe").val(),
//            FechaEmision: $("#FechaEmision").val(),
//            FechaPago: $("#FechaPago").val(),
//            Estado: $("#Estado").val(),
//            Propio: $("#Propio").val() === "true",
//            Titular: $("#Titular").val(),
//            Observaciones: $("#Observaciones").val()
//        };

//        $.post(urlGuardarCheque, cheque)
//            .done(function (resp) {

//                if (!resp.ok) {
//                    Swal.fire("Error", resp.message, "error");
//                    return;
//                }

//                $("#modalAltaCheque").modal("hide");

//                // 🔔 Emitir evento (sin saber quién escucha)
//                $(document).trigger("cheque:guardado", resp);

//                Swal.fire("OK", "Cheque guardado correctamente", "success");

//            })
//            .fail(function () {
//                Swal.fire("Error", "No se pudo guardar el cheque", "error");
//            });
//    }

//    return {
//        abrir,
//        guardar
//    };

//})();


//// ===============================
//// EVENTOS
//// ===============================

//$(document).ready(function () {

//    $("#btnGuardarCheque").on("click", function () {
//        ModalAltaCheque.guardar();
//    });

//});
