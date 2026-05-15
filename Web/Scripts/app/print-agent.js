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
