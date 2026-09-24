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

    function ajax(path, options) {
        options = options || {};
        return $.ajax({
            url: BASE_URL + path,
            method: options.method || 'GET',
            data: options.data ? JSON.stringify(options.data) : null,
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
