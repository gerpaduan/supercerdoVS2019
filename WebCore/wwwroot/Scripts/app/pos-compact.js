// Port de la IIFE de ajuste de altura de Web/Views/Ventas/POS.cshtml (~linea 2961, "ayuda en
// 1366x768 / zoom 125%") -- nunca se habia portado a WebCore (la CSS de .pos-compact/.pos-tiny/
// .pos-footer-fit SI se copio byte a byte en pos.css, pero quedaba muerta sin este JS). Bug real
// encontrado 2026-09-06: en cualquier pantalla con menos de 960px de alto util (portatil 1366x768
// con zoom de Windows al 125%/150%, ventana no maximizada, etc. -- un caso MUY comun, no un borde)
// el teclado numerico del POS quedaba en su tamaño "grande" por defecto en vez de compactarse como
// hace el clasico, lo que en la practica se ve/siente mas chico y desproporcionado porque el
// padding/font-size del boton no bajan junto con la fila (estan calibrados juntos solo dentro de
// las reglas .pos-compact/.pos-tiny). Reusable para Ventas/POS.cshtml y PuntosExpendio/POS.cshtml
// (ambos tienen #pos-app + .pos-footer-panel; solo Ventas tiene .pos-ventas-workbench-col -- el
// ajuste de footer se saltea sola si ese selector no existe, mismo criterio que el original).
(function () {
    function getViewportHeight() {
        if (window.visualViewport && window.visualViewport.height) {
            return Math.round(window.visualViewport.height);
        }

        return window.innerHeight || document.documentElement.clientHeight || 0;
    }

    function aplicarAjusteFooter() {
        var workbench = document.querySelector('.pos-ventas-workbench-col');
        var footer = document.querySelector('.pos-footer-panel');
        if (!workbench || !footer) {
            document.documentElement.classList.remove('pos-footer-fit');
            document.documentElement.classList.remove('pos-footer-tiny-fit');
            return;
        }

        var viewportHeight = getViewportHeight();
        var workbenchRect = workbench.getBoundingClientRect();
        var footerRect = footer.getBoundingClientRect();
        var overflow = Math.max(0, Math.ceil(footerRect.bottom - workbenchRect.bottom));

        var footerFit = overflow > 0 || viewportHeight < 950;
        var footerTinyFit = overflow > 24 || viewportHeight < 860;

        document.documentElement.classList.toggle('pos-footer-fit', footerFit);
        document.documentElement.classList.toggle('pos-footer-tiny-fit', footerTinyFit);
    }

    function aplicarCompacto() {
        if (!document.getElementById('pos-app')) return;

        var h = getViewportHeight();

        document.documentElement.classList.toggle('pos-compact', h < 960);
        document.documentElement.classList.toggle('pos-tiny', h < 860);

        window.requestAnimationFrame(aplicarAjusteFooter);
    }

    window.addEventListener('resize', aplicarCompacto);
    window.addEventListener('orientationchange', aplicarCompacto);
    if (window.visualViewport) {
        window.visualViewport.addEventListener('resize', aplicarCompacto);
        window.visualViewport.addEventListener('scroll', aplicarCompacto);
    }
    document.addEventListener('DOMContentLoaded', aplicarCompacto);
    aplicarCompacto();
})();
