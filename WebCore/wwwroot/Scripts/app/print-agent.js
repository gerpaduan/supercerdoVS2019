// Port literal de Web/Scripts/app/print-agent.js (2026-09-10, cuarta ronda de pedidos, ver
// docs/DECISIONS.md "Batch 6: Dispositivos Seguros -- numero de serie + auto-deteccion +
// autoservicio"). Cliente HTTP contra el agente de impresion local (proyecto PrintAgent/, un .exe
// que el usuario instala y corre en su PC, escuchando en 127.0.0.1:18777). Cargado globalmente en
// _Layout.cshtml y _LayoutPOS.cshtml (igual que clasico). Consumidores: getDeviceId() en
// DispositivosSeguros/Index.cshtml y _ModalMiDispositivo.cshtml; health()/printExpendio() en
// ticket-print.js (tickets de Ventas y Movimientos, 2026-09-23) y calculadora-billetes.js;
// getPrinters()/getConfig()/saveConfig() en calculadora-billetes.js. Sin cambios de logica
// respecto al clasico.
(function (window, $) {
    if (!$) return;

    var BASE_URL = 'http://127.0.0.1:18777';

    // El agente (PrintAgent/LocalPrintServer.cs) lee el cuerpo contando CARACTERES contra un
    // Content-Length en BYTES: con un solo caracter no ASCII (acento, ñ, NBSP = 2 bytes en UTF-8) queda
    // esperando bytes que no llegan hasta el timeout, y el ticket cae al dialogo del navegador. Se
    // escapa todo lo no ASCII a \uXXXX (JSON valido; el agente lo decodifica) asi bytes == caracteres
    // y no hace falta reinstalar el agente.
    function toAsciiJson(data) {
        return JSON.stringify(data).replace(/[\u0080-￿]/g, function (c) {
            return '\\u' + ('0000' + c.charCodeAt(0).toString(16)).slice(-4);
        });
    }

    function ajax(path, options) {
        options = options || {};
        return $.ajax({
            url: BASE_URL + path,
            method: options.method || 'GET',
            data: options.data ? toAsciiJson(options.data) : null,
            dataType: 'json',
            contentType: options.data ? 'application/json; charset=utf-8' : undefined,
            timeout: options.timeout || 2000
        });
    }

    window.CarniSysPrintAgent = {
        health: function () {
            return ajax('/health');
        },
        getPrinters: function () {
            return ajax('/printers');
        },
        getDeviceId: function () {
            return ajax('/device-id');
        },
        getConfig: function () {
            return ajax('/config');
        },
        saveConfig: function (config) {
            return ajax('/config', {
                method: 'POST',
                data: config || {}
            });
        },
        printExpendio: function (payload) {
            return ajax('/print/expendio', {
                method: 'POST',
                data: payload || {},
                timeout: 5000
            });
        }
    };
})(window, window.jQuery);
