// Comportamiento de "Sugerir puntos de stock" (Reposicion/Puntos): búsqueda por AJAX, selección de filas, edición del
// punto sugerido y guardado con confirmación. Las reglas NO están acá: el servidor calcula las sugerencias y vuelve a
// validar todo al guardar (no pisa puntos ya cargados, solo productos que llevan stock, rangos). El selector de sucursales
// y "Más filtros" están en reposicion-comun.js. Depende de jQuery, SweetAlert2 (opcional) y window.puntosConfig (vista).
// Ayuda: docs/11-manual-de-uso/pantallas/Reposicion.Puntos.md.
(function () {
    "use strict";

    var cfg = window.puntosConfig || {};
    var $form = $("#formPuntos");
    var $contenedor = $("#contenedorPuntos");
    var $btnBuscar = $("#btnBuscarPuntos");
    var $btnBuscarTexto = $btnBuscar.find(".btn-buscar-texto");

    // Selector de sucursales y panel "Más filtros" (comunes a las dos pantallas de Reposicion).
    window.reposicionComun.iniciarFiltros();

    function tieneSwal() {
        return window.Swal && typeof window.Swal.fire === "function";
    }

    // Texto a HTML seguro: los nombres de producto vienen de la base y se muestran dentro de un cuadro de SweetAlert.
    function esc(texto) {
        return String(texto == null ? "" : texto).replace(/[&<>"']/g, function (c) {
            return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c];
        });
    }

    // ---- Búsqueda ----

    function setBuscando(buscando) {
        $btnBuscar.prop("disabled", buscando || !cfg.disponible);
        $btnBuscarTexto.text(buscando ? "Buscando..." : "Buscar");
    }

    function buscar() {
        if (!cfg.disponible) return;
        setBuscando(true);

        return $.ajax({ url: cfg.buscarUrl, type: "GET", data: $form.serialize() })
            .done(function (html) {
                $contenedor.html(html);
                actualizarSeleccion();
            })
            .fail(function (xhr) {
                var mensaje = xhr && xhr.status === 403
                    ? "No tenés permiso para ver el stock."
                    : "No se pudieron calcular las sugerencias. Probá de nuevo; si sigue fallando, avisá a soporte.";
                $contenedor.html('<div class="alert alert-danger">' + esc(mensaje) + "</div>");
            })
            .always(function () {
                setBuscando(false);
            });
    }

    $form.on("submit", function (e) {
        e.preventDefault();
        buscar();
    });

    // ---- Selección y edición ----

    function filasTildadas() {
        return $contenedor.find(".js-punto-check:checked");
    }

    // Valor entero 1..max de un input, o null si no es válido.
    function valorValido($input) {
        var maximo = Number($contenedor.find("#puntosTablaCard").data("max-punto")) || 999999;
        var texto = String($input.val() || "").trim();
        if (!/^\d+$/.test(texto)) return null;
        var valor = parseInt(texto, 10);
        return valor >= 1 && valor <= maximo ? valor : null;
    }

    function actualizarSeleccion() {
        var $tildadas = filasTildadas();
        var total = $contenedor.find(".js-punto-check").length;
        var invalidas = 0;
        $tildadas.each(function () {
            var $input = $(this).closest("tr").find(".js-punto-input");
            var ok = valorValido($input) !== null;
            $input.toggleClass("is-invalid", !ok);
            if (!ok) invalidas++;
        });

        var texto = $tildadas.length === 0
            ? "Ninguno seleccionado. Tildá los que quieras cargar (" + total + " se pueden cargar)."
            : $tildadas.length + (($tildadas.length === 1) ? " seleccionado" : " seleccionados") + " de " + total
              + (invalidas > 0 ? " · hay " + invalidas + " con un número inválido (entero de 1 en adelante)" : "") + ".";
        $contenedor.find("#lblSeleccionPuntos").text(texto);
        $contenedor.find("#btnGuardarPuntos").prop("disabled", $tildadas.length === 0 || invalidas > 0);

        var $todos = $contenedor.find("#chkSeleccionarTodosPuntos");
        $todos.prop("checked", total > 0 && $tildadas.length === total);
        $todos.prop("indeterminate", $tildadas.length > 0 && $tildadas.length < total);
    }

    $contenedor.on("change", ".js-punto-check", actualizarSeleccion);

    $contenedor.on("change", "#chkSeleccionarTodosPuntos", function () {
        $contenedor.find(".js-punto-check").prop("checked", $(this).is(":checked"));
        actualizarSeleccion();
    });

    // Tocar el número tilda la fila (si lo cambió es porque quiere cargarlo) y revalida.
    $contenedor.on("input", ".js-punto-input", function () {
        $(this).closest("tr").find(".js-punto-check").prop("checked", true);
        actualizarSeleccion();
    });

    // ---- Guardado ----

    function armarPedido() {
        var valores = [];
        filasTildadas().each(function () {
            var $fila = $(this).closest("tr");
            var punto = valorValido($fila.find(".js-punto-input"));
            if (punto === null) return;
            valores.push({ idCorte: Number($fila.data("id-corte")), idSucursal: Number($fila.data("id-sucursal")), puntoStock: punto });
        });
        return valores;
    }

    function postGuardar(valores) {
        return fetch(cfg.guardarUrl, {
            method: "POST",
            credentials: "same-origin",
            headers: { "Content-Type": "application/json", "RequestVerificationToken": window.reposicionComun.tokenAntiForgery() },
            body: JSON.stringify({ valores: valores })
        }).then(function (respuesta) {
            return respuesta.json().catch(function () { return { ok: false, error: "Respuesta inesperada del servidor (" + respuesta.status + ")." }; });
        }).catch(function () {
            return { ok: false, error: "No hay conexión con el servidor. No se guardó nada." };
        });
    }

    function avisarResultado(resp, enviados) {
        var rechazados = (resp && resp.rechazados) || [];
        var lista = rechazados.length
            ? '<ul class="text-left small mb-0 mt-2">' + rechazados.map(function (r) { return "<li>" + esc(r) + "</li>"; }).join("") + "</ul>"
            : "";

        if (resp && resp.ok) {
            var titulo = "Se cargaron " + resp.guardados + (resp.guardados === 1 ? " punto de stock" : " puntos de stock");
            if (tieneSwal()) {
                window.Swal.fire({ icon: rechazados.length ? "warning" : "success", title: titulo,
                    html: rechazados.length ? "No se cargaron " + rechazados.length + ":" + lista : "", timer: rechazados.length ? undefined : 2500,
                    showConfirmButton: rechazados.length > 0 });
            } else {
                window.alert(titulo + (rechazados.length ? "\nNo se cargaron: " + rechazados.join(" | ") : ""));
            }
            return;
        }

        var mensaje = (resp && resp.error) || "No se pudieron guardar los puntos de stock.";
        if (tieneSwal()) {
            window.Swal.fire({ icon: "error", title: "No se guardó", html: esc(mensaje) + lista });
        } else {
            window.alert(mensaje);
        }
    }

    function guardar() {
        var valores = armarPedido();
        if (valores.length === 0) return;

        var $btn = $contenedor.find("#btnGuardarPuntos");
        var ejecutar = function () {
            $btn.prop("disabled", true);
            postGuardar(valores).then(function (resp) {
                avisarResultado(resp, valores.length);
                // Lo cargado ya tiene punto: se recalcula para que desaparezca de la lista (y se vea el resto como quedó).
                return buscar();
            });
        };

        var pregunta = "Se van a cargar " + valores.length + (valores.length === 1 ? " punto de stock" : " puntos de stock") + ". "
            + "No se modifica el stock ni se crean pedidos, y no se pisan los puntos que ya estén cargados.";
        if (tieneSwal()) {
            window.Swal.fire({ icon: "question", title: "¿Cargar los puntos de stock?", text: pregunta, showCancelButton: true,
                confirmButtonText: "Cargar", cancelButtonText: "Cancelar" }).then(function (r) { if (r.isConfirmed) ejecutar(); });
        } else if (window.confirm(pregunta)) {
            ejecutar();
        }
    }

    $contenedor.on("click", "#btnGuardarPuntos", guardar);

    // Al abrir ya se calcula (tarda ~1 s). Con el motor sin soporte no se busca.
    if (cfg.disponible) {
        buscar();
    } else {
        $contenedor.find(".card-body").text("");
    }
})();
