// Abre una pantalla de WebCore en una ventana independiente del navegador (sin menus): la URL que se
// pasa lleva ?popup=true y la vista correspondiente cambia a _LayoutEmbed. Lo usan el boton "Abrir en
// ventana" de Stock/ExistenciaPorSucursales (inline en la vista) y de Reportes (reportes.js).
// Con el mismo "nombre" el navegador reutiliza la ventana ya abierta: la recarga con la URL nueva y la
// trae al frente (no abre otra). Doc: docs/DECISIONS.md (2026-10-13, ventana independiente).
(function (window) {
    // Tamano inicial; el usuario puede moverla, redimensionarla y maximizarla.
    var VENTANA_ANCHO_PX = 1100;
    var VENTANA_ALTO_PX = 800;
    var VENTANA_OPCIONES = "popup=yes,width=" + VENTANA_ANCHO_PX + ",height=" + VENTANA_ALTO_PX +
        ",resizable=yes,scrollbars=yes";

    // url: direccion completa (con popup=true). nombre: identifica la ventana (una por pantalla).
    // Devuelve la ventana, o null si el navegador la bloqueo (en ese caso avisa al usuario).
    window.abrirVentanaIndependiente = function (url, nombre) {
        var ventana = window.open(url, nombre, VENTANA_OPCIONES);
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
