// Shim de compatibilidad Bootstrap 4 -> 5 (ver bootstrap4-compat.css para el detalle completo del
// hallazgo). BS5 solo reacciona a data-bs-toggle/data-bs-dismiss/data-bs-target -- el markup ya
// portado (30+ vistas) usa los nombres viejos de BS4 (data-toggle/data-dismiss/data-target), que
// BS5 ignora por completo. Este listener global delega los clicks sobre esos atributos viejos a
// la API real de BS5 (window.bootstrap), sin tener que reescribir el markup de cada vista.
(function () {
    'use strict';

    function getOrCreate(Ctor, el) {
        return Ctor.getOrCreateInstance ? Ctor.getOrCreateInstance(el) : (Ctor.getInstance(el) || new Ctor(el));
    }

    document.addEventListener('click', function (e) {
        if (!window.bootstrap) return;

        var dismissEl = e.target.closest('[data-dismiss]');
        if (dismissEl) {
            var dismissType = dismissEl.getAttribute('data-dismiss');

            if (dismissType === 'modal' && window.bootstrap.Modal) {
                var modalEl = dismissEl.closest('.modal');
                if (modalEl) getOrCreate(window.bootstrap.Modal, modalEl).hide();
            } else if (dismissType === 'alert' && window.bootstrap.Alert) {
                var alertEl = dismissEl.closest('.alert');
                if (alertEl) getOrCreate(window.bootstrap.Alert, alertEl).close();
            }
            return;
        }

        var toggleEl = e.target.closest('[data-toggle]');
        if (!toggleEl) return;

        var toggleType = toggleEl.getAttribute('data-toggle');
        var targetSelector = toggleEl.getAttribute('data-target') || toggleEl.getAttribute('href');
        var targetEl = targetSelector ? document.querySelector(targetSelector) : null;

        if (toggleType === 'modal' && targetEl && window.bootstrap.Modal) {
            e.preventDefault();
            getOrCreate(window.bootstrap.Modal, targetEl).show();
        } else if (toggleType === 'collapse' && targetEl && window.bootstrap.Collapse) {
            e.preventDefault();
            getOrCreate(window.bootstrap.Collapse, targetEl).toggle();
        }
        // "dropdown" no necesita manejo manual: bootstrap.bundle ya inicializa cada
        // [data-bs-toggle="dropdown"] via su propio data-api, pero para data-toggle="dropdown"
        // (nombre viejo) hace falta instanciarlo una vez -- se hace abajo, no por click.
    });

    document.addEventListener('DOMContentLoaded', function () {
        if (!window.bootstrap || !window.bootstrap.Dropdown) return;
        document.querySelectorAll('[data-toggle="dropdown"]').forEach(function (el) {
            getOrCreate(window.bootstrap.Dropdown, el);
        });
    });
})();

// Shim del plugin jQuery de Bootstrap 4 ($.fn.modal/.collapse/.alert/etc.), eliminado en BS5.
// Hallazgo real (2026-09-05, ver docs/DECISIONS.md y gaps.md): 16 archivos ya migrados llaman a
// $(el).modal('show')/.collapse('show'|'hide')/.alert('close') como lo hacia el markup original
// contra Bootstrap 4 -- en BS5 esos metodos no existen en el objeto jQuery ($(...).modal is not a
// function), tira un error real en consola y puede cortar el resto del bloque <script>. En vez de
// reescribir cada llamada a la API nativa de bootstrap.bundle.min.js en las 16+ vistas/scripts que
// ya la usan (y en las que se sigan portando), se restaura una version minima del plugin jQuery
// que delega a la instancia real de BS5 -- mismo criterio que el shim de arriba (data-toggle).
(function () {
    'use strict';

    if (!window.jQuery || !window.bootstrap) return;

    function getOrCreateInstance(Ctor, el, config) {
        return Ctor.getOrCreateInstance ? Ctor.getOrCreateInstance(el, config) : (Ctor.getInstance(el) || new Ctor(el, config));
    }

    // Registra $.fn.<nombre>, pisando cualquier definicion previa a proposito -- ver el hallazgo
    // de 2026-09-06 (ver docs/DECISIONS.md) mas abajo. Los 6 nombres que este archivo registra
    // (modal/collapse/alert/tooltip/popover/dropdown) son exclusivos de componentes de Bootstrap,
    // asi que la unica fuente real de un $.fn.<nombre> preexistente es el propio
    // bootstrap.bundle.min.js (ver abajo), nunca una libreria de terceros -- pisarlo es seguro.
    function registrarPluginJQuery(nombre, Ctor) {
        if (!Ctor) return;

        window.jQuery.fn[nombre] = function (opcionesOComando) {
            return this.each(function () {
                var config = (opcionesOComando && typeof opcionesOComando === 'object') ? opcionesOComando : undefined;
                var instancia = getOrCreateInstance(Ctor, this, config);

                if (typeof opcionesOComando === 'string' && typeof instancia[opcionesOComando] === 'function') {
                    instancia[opcionesOComando]();
                    return;
                }

                // Bug real encontrado 2026-09-06 (ver docs/DECISIONS.md), al portar el modal de
                // Factura Electronica: el modismo de Bootstrap 4 "$(el).modal({backdrop:'static',
                // keyboard:false, show:true})" (usado en varios archivos de Web/Scripts/app/*.js
                // sin portar todavia, ej. modal-postventa.js) configuraba la instancia de BS5 pero
                // NUNCA llamaba a show() -- ni con este shim (el objeto de opciones no es un
                // string, la rama de arriba nunca corria) NI con la interfaz jQuery nativa que trae
                // esta version de bootstrap.bundle.min.js (mismo defecto: solo atiende comandos
                // string). Sintoma real: el modal quedaba con class="modal fade" (sin "show"),
                // display:none, invisible, sin ningun error en consola -- muy dificil de diagnosticar
                // a simple vista. Mismo criterio que el plugin jQuery real de Bootstrap 4 (Modal.
                // prototype de bootstrap.js 4.x: "else if (_config.show) data.show(...)").
                if (config && config.show && typeof instancia.show === 'function') {
                    instancia.show();
                }
            });
        };
    }

    function registrarTodos() {
        registrarPluginJQuery('modal', window.bootstrap.Modal);
        registrarPluginJQuery('collapse', window.bootstrap.Collapse);
        registrarPluginJQuery('alert', window.bootstrap.Alert);
        registrarPluginJQuery('tooltip', window.bootstrap.Tooltip);
        registrarPluginJQuery('popover', window.bootstrap.Popover);
        registrarPluginJQuery('dropdown', window.bootstrap.Dropdown);
    }

    registrarTodos();

    // Hallazgo real 2026-09-06 (ver docs/DECISIONS.md): esta version de bootstrap.bundle.min.js
    // (5.0/5.1) trae su PROPIA interfaz jQuery nativa (Modal.jQueryInterface, etc., con el mismo
    // defecto de "show" documentado arriba), pero la registra en un listener propio de
    // DOMContentLoaded -- que corre DESPUES de este script (ambos son <script> sincronicos al
    // final del body, y DOMContentLoaded recien dispara cuando termina de parsearse TODO el
    // documento). Resultado: el registrarTodos() de arriba corria bien, pero milisegundos despues
    // Bootstrap pisaba $.fn.modal/etc con su propia version rota otra vez -- por eso hacia falta
    // volver a registrar aca, despues de ese mismo evento, para quedar con la ultima palabra.
    document.addEventListener('DOMContentLoaded', registrarTodos);
})();
