// ===============================
// MODAL ALTA / EDICION CHEQUE
// ===============================
(function (window) {

    function inicializarModalAltaCheque() {
        var $ = window.jQuery;
        if (!$) {
            window.setTimeout(inicializarModalAltaCheque, 60);
            return;
        }

        if (window.ModalAltaCheque && window.ModalAltaCheque.__inicializado) {
            return;
        }

        window.ModalAltaCheque = (function () {
            function escapeHtml(valor) {
                return $("<div>").text(valor == null ? "" : valor).html();
            }

            function mostrarErrores(errores, titulo) {
                var lista = (errores || []).map(function (error) {
                    return "<li>" + escapeHtml(error) + "</li>";
                }).join("");

                Swal.fire({
                    title: titulo || "Revisa estos datos",
                    html: [
                        '<div style="text-align:left">',
                        '  <div style="margin-bottom:.5rem;">Corregi los siguientes puntos antes de continuar:</div>',
                        '  <ul style="margin:0; padding-left:1.25rem;">',
                             lista,
                        '  </ul>',
                        '</div>'
                    ].join(""),
                    icon: "warning",
                    confirmButtonText: "Entendido"
                });
            }

            function abrir(nroCheque, cheque) {
                nroCheque = nroCheque || "";
                cheque = cheque || null;

                var form = document.getElementById("formCheque");
                if (form) {
                    form.reset();
                }

                $("#ChequeId").val(cheque && cheque.Id ? cheque.Id : 0);
                $("#NroCheque").val(cheque && cheque.NroCheque ? cheque.NroCheque : nroCheque);
                $("#Banco").val(cheque && cheque.Banco ? cheque.Banco : "");
                $("#Importe").val(cheque && cheque.Importe ? cheque.Importe : "");
                $("#FechaEmision").val(cheque && cheque.FechaEmision ? cheque.FechaEmision : "");
                $("#FechaPago").val(cheque && cheque.FechaPago ? cheque.FechaPago : "");
                $("#Estado").val(cheque && cheque.Estado ? cheque.Estado : "PENDIENTE");
                $("#Propio").val(cheque && cheque.Propio ? "true" : "false");
                $("#Titular").val(cheque && cheque.Titular ? cheque.Titular : "");
                $("#Observaciones").val(cheque && cheque.Observaciones ? cheque.Observaciones : "");

                $("#tituloModalCheque").text(
                    cheque ? "Editar Cheque" : "Alta de Cheque"
                );

                $("#modalAltaCheque").modal("show");
            }

            function validar() {
                var errores = [];

                var banco = ($("#Banco").val() || "").trim();
                if (!banco) {
                    errores.push("El banco ingresado no es valido.");
                }

                var nroCheque = ($("#NroCheque").val() || "").trim();
                if (!nroCheque) {
                    errores.push("Debe ingresar el numero de cheque.");
                }

                var importe = $("#Importe").val();
                if (!importe || importe === "0") {
                    errores.push("Ingrese un importe valido.");
                }

                var fechaPago = $("#FechaPago").val();
                if (!fechaPago) {
                    errores.push("Ingrese una fecha de pago.");
                }

                if (errores.length > 0) {
                    mostrarErrores(errores);
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
                    mostrarErrores(["No esta configurada la URL para guardar el cheque."], "No se puede guardar");
                    return;
                }

                var cheque = {
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

                if ($("#AProveedor").length) {
                    cheque.esAProveedor = $("#AProveedor").val() === "true";
                }

                $.post(urlGuardarCheque, cheque)
                    .done(function (resp) {
                        if (!resp || !resp.ok) {
                            mostrarErrores([
                                (resp && (resp.message || resp.mensaje)) || "No se pudo guardar el cheque."
                            ], "No se pudo guardar");
                            return;
                        }

                        $("#modalAltaCheque").modal("hide");

                        $(document).trigger("cheque:guardado", resp);
                        $(document).trigger("chequeCreado", [resp.cheque || cheque]);

                        Swal.fire("OK", "Cheque guardado correctamente", "success");
                    })
                    .fail(function () {
                        mostrarErrores(["No se pudo guardar el cheque."], "Error de guardado");
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
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", inicializarModalAltaCheque);
    } else {
        inicializarModalAltaCheque();
    }

    window.addEventListener("load", inicializarModalAltaCheque);
})(window);
