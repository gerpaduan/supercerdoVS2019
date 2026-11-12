/* Ventana independiente ANGOSTA de Movimientos (2026-11-12, docs/DECISIONS.md "Ventana independiente a 1/3 del ancho").
   La ventana de "Abrir en ventana" mide un tercio del ancho de la pantalla (~500-640 px): por debajo de 768 px el sitio usa el diseno de celular
   (tarjetas, campos uno por renglon). Aca se le pide al navegador que, SOLO en esa ventana, evalue las consultas de ancho (min-width / max-width)
   como si midiera ANCHO_VIRTUAL_PX: el diseno de escritorio compacto (el mismo de la columna de carga cuando se abre el Stock destino) se aplica
   en un contenedor angosto, sin duplicar CSS. Se hace de dos maneras:
     1) las reglas @media de las hojas de estilo se reescriben (CSSMediaRule.media.mediaText);
     2) window.matchMedia se envuelve, asi el JS que decide por ancho (pantalla-trabajo.js, esEscritorio) coincide con el CSS.
   Solo corre si: la pagina esta en modo ventana (clase html.modo-ventana, script del <head> de _Layout.cshtml), es Movimientos/Editar|Nuevo, la
   ventana mide menos de 1200 px y el dispositivo es de escritorio (mouse). Se carga SINCRONO al final del <head>, antes de pintar.
   Limite conocido: se decide al cargar; si despues se achica una ventana ancha, no se activa hasta recargar. */
(function () {
    'use strict';

    var ANCHO_VIRTUAL_PX = 1366;   // ancho con el que se evaluan las consultas (escritorio comun; >= 1200 y < 1400)
    var ANCHO_ESCRITORIO_PX = 1200; // desde este ancho el diseno de escritorio ya aplica solo
    var PX_POR_REM = 16;

    var matchMediaOriginal = window.matchMedia ? window.matchMedia.bind(window) : null;
    if (!matchMediaOriginal) return;

    var enModoVentana = document.documentElement.classList.contains('modo-ventana');
    var esPantallaMovimiento = /\/Movimientos\/(Editar|Nuevo)(\/|$|\?)/i.test(location.pathname);
    var esEscritorioReal = matchMediaOriginal('(hover: hover) and (pointer: fine)').matches;
    if (!enModoVentana || !esPantallaMovimiento || !esEscritorioReal || window.innerWidth >= ANCHO_ESCRITORIO_PX) return;

    window.__ventanaAngosta = true;
    document.documentElement.classList.add('ventana-angosta');

    // Cada (min-width: N) / (max-width: N) se reemplaza por algo siempre verdadero o siempre falso, segun ANCHO_VIRTUAL_PX.
    function reescribirConsulta(texto) {
        return String(texto).replace(/\(\s*(min|max)-width\s*:\s*([\d.]+)(px|rem|em)\s*\)/gi, function (original, tipo, numero, unidad) {
            var px = parseFloat(numero) * (unidad.toLowerCase() === 'px' ? 1 : PX_POR_REM);
            var cumple = tipo.toLowerCase() === 'min' ? ANCHO_VIRTUAL_PX >= px : ANCHO_VIRTUAL_PX <= px;
            return cumple ? '(min-width: 0px)' : '(min-width: 99999px)';
        });
    }

    // matchMedia: el JS ve el mismo "ancho" que el CSS. Devuelve el MediaQueryList real de la consulta reescrita (conserva los listeners).
    window.matchMedia = function (consulta) {
        return matchMediaOriginal(reescribirConsulta(consulta));
    };

    // Reglas ya reescritas (para no repetir el trabajo cuando se vuelve a pasar por las hojas).
    var procesadas = typeof WeakSet === 'function' ? new WeakSet() : null;

    function reescribirReglas(reglas) {
        for (var i = 0; i < reglas.length; i++) {
            var regla = reglas[i];
            if (regla.type === 4 /* CSSRule.MEDIA_RULE */) {
                if (!procesadas || !procesadas.has(regla)) {
                    var antes = regla.media.mediaText;
                    var despues = /print/i.test(antes) ? antes : reescribirConsulta(antes);
                    if (despues !== antes) regla.media.mediaText = despues;
                    if (procesadas) procesadas.add(regla);
                }
            }
            // @media dentro de @supports / @layer: se recorre el grupo (las reglas de estilo no tienen consultas propias).
            if (regla.cssRules && regla.type !== 1 /* CSSRule.STYLE_RULE */) reescribirReglas(regla.cssRules);
        }
    }

    function reescribirHojas() {
        for (var i = 0; i < document.styleSheets.length; i++) {
            try {
                reescribirReglas(document.styleSheets[i].cssRules);
            } catch (e) {
                // Hojas de otro origen (CDN: iconos, fuentes) no se pueden leer; no tienen consultas de ancho que importen aca.
            }
        }
    }

    reescribirHojas();                                        // hojas del <head>, antes de pintar
    document.addEventListener('DOMContentLoaded', function () {
        reescribirHojas();                                    // <style> que traen las vistas dentro del body
        // La ventana usa el mismo acomodo que la columna de carga con el Stock destino abierto (.mov-con-stock): Resumen / Observaciones /
        // Guardar en una franja bajo la tabla. El panel del stock queda oculto (d-none) y el CSS de movimientos-compacto.css lo reduce a 1 columna.
        var disposicion = document.querySelector('.mov-layout');
        if (disposicion) disposicion.classList.add('mov-con-stock');
    });
    window.addEventListener('load', reescribirHojas);         // por si alguna hoja termino de cargar despues
})();
