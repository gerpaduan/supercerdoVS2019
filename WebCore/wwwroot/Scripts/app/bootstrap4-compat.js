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

    // Registra $.fn.<nombre> solo si el componente de BS5 existe y el plugin no fue definido ya
    // por otra parte (nunca pisa una implementacion real si en algun momento se agrega jquery-ui
    // u otra libreria que sí traiga estos metodos).
    function registrarPluginJQuery(nombre, Ctor) {
        if (!Ctor || window.jQuery.fn[nombre]) return;

        window.jQuery.fn[nombre] = function (opcionesOComando) {
            return this.each(function () {
                var config = (opcionesOComando && typeof opcionesOComando === 'object') ? opcionesOComando : undefined;
                var instancia = getOrCreateInstance(Ctor, this, config);

                if (typeof opcionesOComando === 'string' && typeof instancia[opcionesOComando] === 'function') {
                    instancia[opcionesOComando]();
                }
            });
        };
    }

    registrarPluginJQuery('modal', window.bootstrap.Modal);
    registrarPluginJQuery('collapse', window.bootstrap.Collapse);
    registrarPluginJQuery('alert', window.bootstrap.Alert);
    registrarPluginJQuery('tooltip', window.bootstrap.Tooltip);
    registrarPluginJQuery('popover', window.bootstrap.Popover);
    registrarPluginJQuery('dropdown', window.bootstrap.Dropdown);
})();
