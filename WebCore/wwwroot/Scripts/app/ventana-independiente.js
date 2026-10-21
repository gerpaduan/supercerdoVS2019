// Abre una pantalla de WebCore en una ventana independiente del navegador (sin menus): la URL que se
// pasa lleva ?popup=true y la vista correspondiente cambia a _LayoutEmbed. Lo usan el boton "Abrir en
// ventana" de Stock/ExistenciaPorSucursales (inline en la vista) y de Reportes (reportes.js).
// Con el mismo "nombre" el navegador reutiliza la ventana ya abierta: la recarga con la URL nueva y la
// trae al frente (no abre otra). Doc: docs/DECISIONS.md (2026-10-13, ventana independiente).
(function (window) {
    // Tamano inicial; el usuario puede moverla, redimensionarla y maximizarla.
    var VENTANA_ANCHO_PX = 1100;
    var VENTANA_ALTO_PX = 800;

    // Pantallas angostas (celular/tablet): ahi el navegador ignora el tamano y una fraccion daria una ventana inutil.
    var DESKTOP_ANCHO_MIN_PX = 992;
    // Piso para la ventana pedida como fraccion de la pantalla: con menos de esto la tabla de Existencia (Cod. 15 + Producto 30 caracteres,
    // Stock y Estado) ya no entra sin desplazar. En una pantalla de 1920 px un tercio (640) supera el piso; en una de 1366 manda el piso.
    var VENTANA_ANCHO_FRACCION_MIN_PX = 560;

    // Ancho de la ventana: el fijo de siempre o, si se pidio una fraccion de la pantalla (0-1) y estamos en desktop,
    // esa fraccion del ancho disponible (nunca menos del piso).
    function anchoDeVentana(opciones) {
        var fraccion = opciones && opciones.anchoFraccionPantalla;
        var disponible = window.screen && (window.screen.availWidth || window.screen.width);
        if (!(fraccion > 0 && fraccion <= 1) || !disponible || disponible < DESKTOP_ANCHO_MIN_PX) return VENTANA_ANCHO_PX;
        return Math.max(VENTANA_ANCHO_FRACCION_MIN_PX, Math.round(disponible * fraccion));
    }

    // url: direccion completa (con popup=true). nombre: identifica la ventana (una por pantalla).
    // opciones (opcional): { anchoFraccionPantalla: 0.333 } abre la ventana con esa fraccion del ancho de la pantalla (solo desktop).
    // Devuelve la ventana, o null si el navegador la bloqueo (en ese caso avisa al usuario).
    window.abrirVentanaIndependiente = function (url, nombre, opciones) {
        var caracteristicas = "popup=yes,width=" + anchoDeVentana(opciones) + ",height=" + VENTANA_ALTO_PX +
            ",resizable=yes,scrollbars=yes";
        var ventana = window.open(url, nombre, caracteristicas);
        if (ventana) {
            ventana.focus();
            return ventana;
        }

        // El navegador bloqueo la ventana emergente: se avisa en vez de fallar en silencio.
        var titulo = "Ventana bloqueada";
        var mensaje = "El navegador bloqueó la ventana emergente. Permitila para este sitio e intentá de nuevo.";
        if (window.Swal && typeof window.Swal.fire === "function") {
            window.Swal.fire(titulo, mensaje, "warning");
        } else {
            window.alert(titulo + ": " + mensaje);
        }
        return null;
    };
})(window);
