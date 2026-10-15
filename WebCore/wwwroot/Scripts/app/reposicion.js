// Comportamiento de la pantalla "Qué pedir" (Reposicion/Index): búsqueda por AJAX, exportación a Excel, impresión y
// ventana independiente. Las reglas de negocio NO están acá: el servidor calcula todo (Negocio.ReposicionStock) y devuelve
// la tabla ya armada (Reposicion/_Tabla). El selector de sucursales y "Más filtros" están en reposicion-comun.js.
// Depende de jQuery y de window.reposicionConfig (URLs), que define la vista. Ayuda: docs/11-manual-de-uso/pantallas/Reposicion.Index.md.
(function () {
    "use strict";

    var cfg = window.reposicionConfig || {};
    var $form = $("#formReposicion");
    var $contenedor = $("#contenedorReposicion");
    var $btnBuscar = $("#btnBuscarReposicion");
    var $btnBuscarTexto = $btnBuscar.find(".btn-buscar-texto");
    var $btnExportar = $("#btnExportarReposicion");
    var $btnImprimir = $("#btnImprimirReposicion");
    var $switchSinMov = $("#switchSinMovimientoReposicion");

    // Filtros de la última búsqueda aplicada: el Excel se pide con ESTOS (lo que se ve en pantalla), no con lo que
    // el usuario haya tocado después sin volver a buscar.
    var ultimaConsulta = null;

    // Selector de sucursales y panel "Más filtros" (comunes a las dos pantallas de Reposicion).
    window.reposicionComun.iniciarFiltros();

    // ---- Búsqueda ----

    function setBuscando(buscando) {
        $btnBuscar.prop("disabled", buscando || !cfg.disponible);
        $btnBuscarTexto.text(buscando ? "Buscando..." : "Buscar");
    }

    function buscar() {
        if (!cfg.disponible) return;

        var consulta = $form.serialize();
        setBuscando(true);
        $btnExportar.prop("disabled", true);
        $btnImprimir.prop("disabled", true);

        $.ajax({ url: cfg.buscarUrl, type: "GET", data: consulta })
            .done(function (html) {
                $contenedor.html(html);
                ultimaConsulta = consulta;
                // Hay algo que exportar/imprimir solo si se armó la tabla (si hubo un error, no).
                var hayTabla = $contenedor.find("#tablaReposicion").length > 0;
                $btnExportar.prop("disabled", false);
                $btnImprimir.prop("disabled", !hayTabla);
            })
            .fail(function (xhr) {
                var mensaje = xhr && xhr.status === 403
                    ? "No tenés permiso para ver el stock."
                    : "No se pudo calcular qué pedir. Probá de nuevo; si sigue fallando, avisá a soporte.";
                $contenedor.html('<div class="alert alert-danger">' + mensaje + "</div>");
            })
            .always(function () {
                setBuscando(false);
            });
    }

    $form.on("submit", function (e) {
        e.preventDefault();
        buscar();
    });

    // "Mostrar / Ocultar" de la tarjeta "En cero, sin movimiento": cambia el filtro y vuelve a buscar.
    $contenedor.on("click", "#btnToggleSinMovimiento", function () {
        $switchSinMov.prop("checked", !$switchSinMov.is(":checked"));
        buscar();
    });

    // ---- Excel, imprimir, ventana independiente ----

    $btnExportar.on("click", function () {
        if (!ultimaConsulta) return;
        window.location.href = cfg.exportarUrl + "?" + ultimaConsulta;
    });

    $btnImprimir.on("click", function () {
        window.print();
    });

    // Ventana independiente (helper compartido ventana-independiente.js): un nombre fijo por pantalla, así un segundo
    // clic reutiliza la ventana abierta.
    $("#btnAbrirVentanaReposicion").on("click", function () {
        if (typeof window.abrirVentanaIndependiente === "function")
            window.abrirVentanaIndependiente(cfg.ventanaUrl, cfg.ventanaNombre);
    });

    // Al abrir la pantalla ya se calcula (tarda ~1 s): evita un clic de más. Con el motor sin soporte no se busca.
    if (cfg.disponible) {
        buscar();
    } else {
        $("#reposicionEstadoInicial").text("");
    }
})();
