// Piezas de pantalla compartidas por "Qué pedir" (reposicion.js) y "Sugerir puntos de stock" (reposicion-puntos.js):
// el selector de sucursales (una, varias o todas; siempre al menos una) y el panel "Más filtros".
// Mismos ids/clases en las dos vistas (#btnSucursalesReposicion, .chk-sucursal-reposicion, ...). Depende de jQuery.
(function (global) {
    "use strict";

    // Conecta el selector de sucursales y el panel de filtros avanzados de la vista actual.
    // Mismo comportamiento que Existencia por sucursales: el menú no se cierra al tildar y la última sucursal no se destilda.
    function iniciarFiltros() {
        var $chkSucursales = $(".chk-sucursal-reposicion");
        var $chkTodas = $("#chkSucursalTodasReposicion");
        var $lblSucursales = $("#lblSucursalesReposicion");

        function actualizarTexto() {
            var tildadas = $chkSucursales.filter(":checked");
            var texto;
            if (tildadas.length === $chkSucursales.length) texto = "Todas";
            else if (tildadas.length === 1) texto = tildadas.first().data("nombre");
            else texto = tildadas.length + " sucursales";
            $lblSucursales.text(texto);
            $("#btnSucursalesReposicion").attr("title", tildadas.map(function () { return $(this).data("nombre"); }).get().join(", "));
        }

        // Mantiene "Todas" coherente con las individuales y evita dejar 0 tildadas.
        function sincronizar(origen) {
            if (origen === "todas") {
                $chkSucursales.prop("checked", true);
                $chkTodas.prop("checked", true);
            } else if ($chkSucursales.filter(":checked").length === 0) {
                $(origen).prop("checked", true); // intento de destildar la última: se revierte
            }
            $chkTodas.prop("checked", $chkSucursales.filter(":checked").length === $chkSucursales.length);
            actualizarTexto();
        }

        // El menú no se cierra al tildar (si no, habría que reabrirlo por cada sucursal).
        $("#menuSucursalesReposicion").on("click", function (e) { e.stopPropagation(); });
        $chkSucursales.on("change", function () { sincronizar(this); });
        $chkTodas.on("change", function () { sincronizar("todas"); });
        actualizarTexto();

        $("#switchFiltrosAvanzadosReposicion").on("change", function () {
            $("#panelAvanzadosReposicion").toggleClass("d-none", !$(this).is(":checked"));
        });
    }

    // Token anti-falsificación del layout (para los POST por AJAX).
    function tokenAntiForgery() {
        var input = document.querySelector('#globalAntiForgeryToken input[name="__RequestVerificationToken"]')
            || document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : "";
    }

    global.reposicionComun = { iniciarFiltros: iniciarFiltros, tokenAntiForgery: tokenAntiForgery };
})(window);
