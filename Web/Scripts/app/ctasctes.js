(function (window, document, $) {
    "use strict";

    function obtenerConfig() {
        return window.CtasCtesConfig || {};
    }

    function normalizarTextoBasico(valor) {
        return (valor || "")
            .toString()
            .toLowerCase()
            .replace(/[\u00e1\u00e0\u00e4\u00e2]/g, "a")
            .replace(/[\u00e9\u00e8\u00eb\u00ea]/g, "e")
            .replace(/[\u00ed\u00ec\u00ef\u00ee]/g, "i")
            .replace(/[\u00f3\u00f2\u00f6\u00f4]/g, "o")
            .replace(/[\u00fa\u00f9\u00fc\u00fb]/g, "u")
            .replace(/\u00f1/g, "n")
            .replace(/\s+/g, " ")
            .trim();
    }

    function normalizarTexto(texto) {
        return (texto || "")
            .toString()
            .toLowerCase()
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .replace(/\s+/g, " ")
            .trim();
    }

    function construirUrlCuentaCorriente(idPersona) {
        var config = obtenerConfig();
        var hoy = new Date();
        hoy.setMonth(hoy.getMonth() - 1);
        var fechaDesde = hoy.toISOString().split("T")[0];

        return config.ctaCtePersonaUrl
            + "?idPersona=" + encodeURIComponent(idPersona)
            + "&fechaDesde=" + encodeURIComponent(fechaDesde)
            + "&returnUrl=" + encodeURIComponent(window.location.pathname + window.location.search)
            + "&desdePos=" + (config.desdePos ? "true" : "false");
    }

    function irACtaCte(row) {
        var id = (row.getAttribute("data-id-persona") || "").trim();
        if (!id && row.children.length) {
            id = row.children[0].innerText.trim();
        }

        var url = construirUrlCuentaCorriente(id);
        var config = obtenerConfig();
        if (config.desdePos && window.POSFinanzas) {
            window.POSFinanzas.cargar(url, "Cuenta corriente");
            return;
        }

        window.location.href = url;
    }

    function filtrarCtasCtesVivo() {
        var input = document.getElementById("txtBuscar");
        var tbody = document.getElementById("tbodyCtas");
        var tabla = document.getElementById("tablaCtasCtes");
        if (!input || !tbody || !tabla) return;

        var filtro = normalizarTextoBasico(input.value);
        var headers = tabla.querySelectorAll("thead th");
        var idxNombreIdentif = -1;
        var idxRazonSocial = -1;

        for (var i = 0; i < headers.length; i++) {
            var textoHeader = normalizarTextoBasico(headers[i].innerText || headers[i].textContent || "");
            if (textoHeader.indexOf("nombre identif") >= 0 || textoHeader.indexOf("identificacion") >= 0) {
                idxNombreIdentif = i;
            }
            if (textoHeader.indexOf("razon social") >= 0) {
                idxRazonSocial = i;
            }
        }

        var filas = tbody.getElementsByTagName("tr");
        for (var rowIndex = 0; rowIndex < filas.length; rowIndex++) {
            var fila = filas[rowIndex];
            var celdas = fila.getElementsByTagName("td");
            var textoNombre = idxNombreIdentif >= 0 && celdas[idxNombreIdentif]
                ? normalizarTextoBasico(celdas[idxNombreIdentif].innerText || celdas[idxNombreIdentif].textContent || "")
                : "";
            var textoRazon = idxRazonSocial >= 0 && celdas[idxRazonSocial]
                ? normalizarTextoBasico(celdas[idxRazonSocial].innerText || celdas[idxRazonSocial].textContent || "")
                : "";
            var textoFila = (textoNombre + " " + textoRazon).trim();

            if (!textoFila) {
                textoFila = normalizarTextoBasico(fila.innerText || fila.textContent || "");
            }

            fila.style.display = !filtro || textoFila.indexOf(filtro) >= 0 ? "" : "none";
        }
    }

    function init() {
        var $txtBuscar = $("#txtBuscar");
        var $tbody = $("#tbodyCtas");
        var $tabla = $("#tablaCtasCtes");
        if (!$txtBuscar.length || !$tbody.length || !$tabla.length) return;

        var indiceNombreIdentif = -1;
        var indiceRazonSocial = -1;

        function formatearSaldo() {
            try {
                var indiceSaldo = -1;

                $tabla.find("thead th").each(function (i) {
                    if ($(this).text().trim().toLowerCase().startsWith("saldo")) {
                        indiceSaldo = i;
                    }
                });

                if (indiceSaldo < 0) return;

                $tbody.find("tr").each(function () {
                    var celda = this.children[indiceSaldo];
                    if (!celda) return;

                    var valor = (celda.innerText || "").replace(",", ".").trim();
                    if (!isNaN(valor) && valor !== "") {
                        celda.innerText = parseFloat(valor).toFixed(2);
                    }
                });
            } catch (e) {
                console.error("Error formateando columna Saldo:", e);
            }
        }

        function filasVisibles() {
            return $tbody.find("tr").filter(function () {
                return $(this).css("display") !== "none";
            });
        }

        function detectarColumnasBusqueda() {
            indiceNombreIdentif = -1;
            indiceRazonSocial = -1;

            $tabla.find("thead th").each(function (i) {
                var texto = normalizarTexto($(this).clone().children().remove().end().text() || "");

                if (texto.includes("nombre identif") || texto.includes("identificacion") || texto.includes("identif.") || texto === "persona") {
                    indiceNombreIdentif = i;
                }

                if (texto.includes("razon social")) {
                    indiceRazonSocial = i;
                }
            });
        }

        function marcarSeleccion($fila) {
            $tbody.find("tr").removeClass("is-selected");
            if ($fila && $fila.length) {
                $fila.addClass("is-selected");
            }
        }

        function asegurarSeleccionVisible() {
            var $visibles = filasVisibles();
            if (!$visibles.length) {
                marcarSeleccion($());
                return;
            }

            var $actual = $visibles.filter(".is-selected").first();
            if ($actual.length) return;

            marcarSeleccion($visibles.first());
        }

        function filtrarCuentas() {
            var filtro = normalizarTexto($txtBuscar.val() || "");

            $tbody.find("tr").each(function () {
                var celdas = this.children;
                var textoNombreIdentif = indiceNombreIdentif >= 0 && celdas[indiceNombreIdentif]
                    ? normalizarTexto(celdas[indiceNombreIdentif].innerText || celdas[indiceNombreIdentif].textContent || "")
                    : "";
                var textoRazonSocial = indiceRazonSocial >= 0 && celdas[indiceRazonSocial]
                    ? normalizarTexto(celdas[indiceRazonSocial].innerText || celdas[indiceRazonSocial].textContent || "")
                    : "";
                var textoBusqueda = (textoNombreIdentif + " " + textoRazonSocial).trim()
                    || normalizarTexto($(this).text() || "");

                this.style.display = !filtro || textoBusqueda.includes(filtro) ? "" : "none";
            });

            asegurarSeleccionVisible();
        }

        function moverSeleccion(delta) {
            var $visibles = filasVisibles();
            if (!$visibles.length) return;

            var indexActual = $visibles.index($visibles.filter(".is-selected").first());
            if (indexActual < 0) indexActual = 0;

            var nuevoIndex = Math.max(0, Math.min($visibles.length - 1, indexActual + delta));
            var $nueva = $visibles.eq(nuevoIndex);

            marcarSeleccion($nueva);

            if ($nueva.length && $nueva[0].scrollIntoView) {
                $nueva[0].scrollIntoView({ block: "nearest" });
            }
        }

        function abrirFilaSeleccionada() {
            var $fila = filasVisibles().filter(".is-selected").first();
            if ($fila.length) {
                irACtaCte($fila[0]);
            }
        }

        function enfocarBuscador() {
            function intentarFoco() {
                var input = $txtBuscar.get(0);
                if (!input) return;

                input.focus();
                $txtBuscar.trigger("focus");
                $txtBuscar.trigger("select");
                asegurarSeleccionVisible();
            }

            setTimeout(intentarFoco, 0);
            setTimeout(intentarFoco, 120);
            setTimeout(intentarFoco, 260);
        }

        formatearSaldo();
        detectarColumnasBusqueda();
        filtrarCuentas();
        filtrarCtasCtesVivo();
        enfocarBuscador();

        $txtBuscar
            .off(".ctasCtes")
            .on("input.ctasCtes", function () {
                filtrarCuentas();
            })
            .on("keydown.ctasCtes", function (e) {
                if (e.key === "ArrowDown") {
                    e.preventDefault();
                    moverSeleccion(1);
                    return;
                }

                if (e.key === "ArrowUp") {
                    e.preventDefault();
                    moverSeleccion(-1);
                    return;
                }

                if (e.key === "Enter") {
                    e.preventDefault();
                    abrirFilaSeleccionada();
                }
            });

        $tbody
            .off("click.ctasCtes", "tr")
            .on("click.ctasCtes", "tr", function () {
                marcarSeleccion($(this));
            });
    }

    window.irACtaCte = irACtaCte;
    window.filtrarCtasCtesVivo = filtrarCtasCtesVivo;
    window.CtasCtesPage = {
        init: init,
        irACtaCte: irACtaCte,
        filtrarCtasCtesVivo: filtrarCtasCtesVivo
    };
})(window, document, window.jQuery);
