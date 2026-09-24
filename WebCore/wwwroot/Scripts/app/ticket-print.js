// Impresion de tickets termicos (58/80mm) de Ventas y Movimientos sin abrir pestañas ni salir de
// la vista (ver docs/DECISIONS.md, 2026-09-23). Port de imprimirTicket/imprimirConAgente de
// Web/Scripts/app/modal-postventa.js y modal-postmovimiento.js, compartido por los 3 puntos de
// impresion de WebCore (Ventas/POS.cshtml, detalle-venta-comprobante.js, modal-postmovimiento.js).
//
// Flujo de TicketPrint.imprimir({ ticketUrl, payloadUrl, onDone }):
//  1. Si el agente local (window.CarniSysPrintAgent, print-agent.js) responde a /health, pide el
//     payload JSON al server (payloadUrl) y lo manda al agente -> imprime directo, sin dialogo.
//  2. Si no hay agente, o algo falla en el camino, carga ticketUrl (vista HTML del ticket) en un
//     <iframe> oculto y la imprime desde aca -> solo aparece el dialogo nativo de impresion del
//     navegador, sin pestaña nueva y sin cambiar la URL de la pagina actual.
//  onDone (opcional) se llama una sola vez cuando el ticket ya se mando a imprimir (por el agente,
//  o cuando el dialogo del navegador se cerro); no se llama si el ticket no pudo cargarse.
// Depende de: jQuery, print-agent.js (opcional: sin el, va siempre por navegador).
(function (window, $) {
    'use strict';

    if (!$ || window.TicketPrint) return;

    var IFRAME_ID = 'iframeTicketPrint';

    // Fallback por navegador: se carga la vista del ticket en el iframe y, al terminar de cargar,
    // se imprime desde aca (focus + print, igual que el clasico). Las vistas del ticket solo se
    // autoimprimen cuando NO estan enmarcadas (window.top === window): el window.print() de su
    // onload dentro del iframe sin foco no mostraba el dialogo en Chrome. print() bloquea hasta
    // que se cierra el dialogo, por eso onDone se llama justo despues.
    function imprimirPorNavegador(ticketUrl, onDone) {
        $('#' + IFRAME_ID).remove();

        var $iframe = $('<iframe>', {
            id: IFRAME_ID,
            style: 'position:fixed; right:0; bottom:0; width:1px; height:1px; border:0; opacity:0; pointer-events:none;'
        });

        $iframe.on('load', function () {
            try {
                var frameWindow = this.contentWindow;
                if (frameWindow && typeof frameWindow.print === 'function') {
                    frameWindow.focus();
                    frameWindow.print();
                }
            } catch (e) {
                console.error('No se pudo imprimir el ticket desde el iframe', e);
            }

            if (typeof onDone === 'function') onDone();
        });

        $iframe.attr('src', ticketUrl).appendTo('body');
    }

    // Intenta imprimir directo por el agente; ante cualquier falla cae al navegador.
    function imprimirPorAgente(payloadUrl, ticketUrl, onDone) {
        function alNavegador() { imprimirPorNavegador(ticketUrl, onDone); }

        window.CarniSysPrintAgent.health()
            .done(function (health) {
                if (!health || !health.ok) {
                    alNavegador();
                    return;
                }

                $.getJSON(payloadUrl)
                    .done(function (payload) {
                        if (!payload || payload.ok === false) {
                            alNavegador();
                            return;
                        }

                        window.CarniSysPrintAgent.printExpendio(payload)
                            .done(function () { if (typeof onDone === 'function') onDone(); })
                            .fail(alNavegador);
                    })
                    .fail(alNavegador);
            })
            .fail(alNavegador);
    }

    window.TicketPrint = {
        imprimir: function (opciones) {
            var ticketUrl = opciones && opciones.ticketUrl;
            var payloadUrl = opciones && opciones.payloadUrl;
            var onDone = opciones && opciones.onDone;
            if (!ticketUrl) return;

            if (window.CarniSysPrintAgent && payloadUrl) {
                imprimirPorAgente(payloadUrl, ticketUrl, onDone);
            } else {
                imprimirPorNavegador(ticketUrl, onDone);
            }
        }
    };
})(window, window.jQuery);
