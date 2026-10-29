(function (window, document) {
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

    // Decide si una fila coincide con lo escrito. Busca en data-buscar (nombre identif., razon social y CUIT con y sin
    // guiones, armado en CtasCtes.cshtml); el IdPersona ya no participa (2026-10-29). Si lo escrito parece un CUIT
    // (solo digitos, guiones, puntos o espacios) tambien se compara sin separadores: "20-1234" encuentra "20123456789".
    function filaCoincide(fila, filtro) {
        if (!filtro) return true;

        var textoFila = normalizarTextoBasico(fila.getAttribute("data-buscar") || fila.innerText || fila.textContent || "");
        if (textoFila.indexOf(filtro) >= 0) return true;

        if (/^[\d\s.\-]+$/.test(filtro)) {
            var filtroSoloDigitos = filtro.replace(/\D/g, "");
            return filtroSoloDigitos !== "" && textoFila.indexOf(filtroSoloDigitos) >= 0;
        }

        return false;
    }

    function filtrarCtasCtesVivo() {
        var input = document.getElementById("txtBuscar");
        var tbody = document.getElementById("tbodyCtas");
        if (!input || !tbody) return;

        var filtro = normalizarTextoBasico(input.value);
        var filas = tbody.getElementsByTagName("tr");
        for (var rowIndex = 0; rowIndex < filas.length; rowIndex++) {
            filas[rowIndex].style.display = filaCoincide(filas[rowIndex], filtro) ? "" : "none";
        }
    }

    // jQuery se lee al llamar, no al cargar el archivo: con _Layout la vista (y este script) van antes que
    // jquery.min.js, y capturarlo al cargar dejaba $ undefined, init() fallaba y el buscador no tomaba el foco
    // (bug 2026-10-29). CtasCtes.cshtml llama a init() recien en DOMContentLoaded si jQuery todavia no esta.
    function init() {
        var $ = window.jQuery;
        if (!$) {
            console.error("CtasCtes: init() sin jQuery cargado; no se activan el foco ni la navegacion con flechas.");
            return;
        }

        var $txtBuscar = $("#txtBuscar");
        var $tbody = $("#tbodyCtas");
        var $tabla = $("#tablaCtasCtes");
        if (!$txtBuscar.length || !$tbody.length || !$tabla.length) return;

        // El saldo ya viene formateado del servidor (N2 en CtasCtes.cshtml). Aca habia un formatearSaldo() que lo
        // re-escribia y, con cultura es-AR, dejaba "12.50" junto a "1.234,50": se quito el 2026-10-29.

        function filasVisibles() {
            return $tbody.find("tr").filter(function () {
                return $(this).css("display") !== "none";
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

        // Mismo criterio que filtrarCtasCtesVivo (el oninput/onkeyup del input llama a esa y este handler a esta):
        // si difirieran, la ultima en correr pisaba a la otra.
        function filtrarCuentas() {
            filtrarCtasCtesVivo();
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

        filtrarCuentas();
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
})(window, document);
