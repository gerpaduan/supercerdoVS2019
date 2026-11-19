// Panel "Corregir linea" compartido (Elaborados, y luego Stock y Movimientos). Mismo patron que el editor de Compras
// (compras.js, "Corregir una linea ya cargada"), pero generico: cada pantalla guarda su propio estado del editor
// (indice de la linea, valores escritos, error) y este modulo solo pinta el panel y reparte los eventos de teclado y clic.
// Uso:
//   - htmlFila({ titulo, campos:[{id,label,valor}], error, avisos:[html], colspan })  -> <tr> del panel (va justo debajo de la fila).
//   - enlazar({ onAplicar, onCancelar, onInput(idCampo, valor) })                       -> engancha los eventos (una sola vez por pantalla).
//   - enfocar(idCampo) / mostrarError(texto)                                            -> ayudas despues de repintar.
// Decision y motivo: docs/DECISIONS.md (2026-11-19, "Corregir una linea").
(function (window, $) {
    'use strict';

    if (!window || !$) return;

    // Escapa texto para HTML y atributos (el valor escrito por la persona vuelve al value="").
    function esc(valor) {
        return String(valor === null || valor === undefined ? '' : valor)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    // Fila del panel: un solo td que ocupa todo el ancho. En celular .cc-fila-detalle (tabla-cards.css) la deja pegada a su tarjeta.
    function htmlFila(opciones) {
        var campos = opciones.campos || [];
        var avisos = opciones.avisos || [];
        var html = '<tr class="cc-fila-detalle linea-editor-fila"><td colspan="' + (opciones.colspan || 1) + '">'
            + '<div class="linea-editor" role="group" aria-label="' + esc(opciones.titulo) + '">'
            + '<div class="linea-editor-head"><strong>' + esc(opciones.titulo) + '</strong>'
            + '<span class="text-muted">Enter aplica · Esc cancela</span></div>'
            + '<div class="linea-editor-campos">';

        $.each(campos, function (_, campo) {
            html += '<div><label for="' + esc(campo.id) + '">' + esc(campo.label) + '</label>'
                // campo.entero: numero sin decimales (ej. unidades). Se rotula con data-le-entero para que cada pantalla le saque el guardia de decimales.
                + '<input type="text" id="' + esc(campo.id) + '" class="form-control form-control-sm linea-editor-campo" data-le-campo="' + esc(campo.id) + '"'
                + (campo.entero ? ' data-le-entero="1"' : '')
                + ' inputmode="' + (campo.entero ? 'numeric' : 'decimal') + '" autocomplete="off" value="' + esc(campo.valor) + '" /></div>';
        });

        html += '</div>';

        // Bloque extra opcional (HTML armado por la pantalla con texto propio, ej. la casilla "Permitir ingreso" de Movimientos).
        if (opciones.htmlExtra) html += '<div class="linea-editor-extra">' + opciones.htmlExtra + '</div>';

        html += ''
            + '<div class="linea-editor-error text-danger" role="alert">' + esc(opciones.error || '') + '</div>';

        if (avisos.length) {
            html += '<div class="linea-editor-avisos">';
            $.each(avisos, function (_, aviso) {
                // Los avisos los arma la pantalla con texto propio (no con datos escritos por la persona).
                html += '<div><i class="fas fa-info-circle" aria-hidden="true"></i><span>' + aviso + '</span></div>';
            });
            html += '</div>';
        }

        html += '<div class="linea-editor-botones">'
            + '<button type="button" class="btn btn-sm btn-outline-secondary" data-le-accion="cancelar" data-sin-foco tabindex="-1">Cancelar</button>'
            + '<button type="button" class="btn btn-sm btn-primary" data-le-accion="aplicar" data-sin-foco tabindex="-1">Aplicar</button>'
            + '</div></div></td></tr>';
        return html;
    }

    // Engancha los eventos del panel. Delegados en document porque la tabla se repinta entera cada vez.
    function enlazar(manejadores) {
        $(document).off('.lineaEditor');

        $(document).on('click.lineaEditor', '.linea-editor [data-le-accion="aplicar"]', function () {
            if (manejadores.onAplicar) manejadores.onAplicar();
        });

        $(document).on('click.lineaEditor', '.linea-editor [data-le-accion="cancelar"]', function () {
            if (manejadores.onCancelar) manejadores.onCancelar();
        });

        $(document).on('input.lineaEditor', '.linea-editor-campo', function () {
            if (manejadores.onInput) manejadores.onInput(String($(this).attr('data-le-campo') || ''), $(this).val());
        });

        // Casilla del bloque extra: se informa como un campo mas (valor true/false).
        $(document).on('change.lineaEditor', '.linea-editor-extra input[type="checkbox"]', function () {
            if (manejadores.onInput) manejadores.onInput(String($(this).attr('data-le-campo') || ''), this.checked);
        });

        // Enter aplica y Esc cierra. El Enter se corta aca: dentro del formulario dispararia el envio (guardar el registro).
        $(document).on('keydown.lineaEditor', '.linea-editor', function (e) {
            if (e.key === 'Escape') {
                e.preventDefault();
                e.stopPropagation();
                if (manejadores.onCancelar) manejadores.onCancelar();
                return;
            }
            if (e.key !== 'Enter' || e.altKey || e.ctrlKey || e.metaKey) return;
            if ($(e.target).is('button')) return;
            e.preventDefault();
            e.stopPropagation();
            if (manejadores.onAplicar) manejadores.onAplicar();
        });
    }

    // Foco al primer campo (o al indicado) con el texto seleccionado, para escribir encima.
    function enfocar(idCampo) {
        var $campo = idCampo ? $('#' + idCampo) : $('.linea-editor-campo').first();
        if (!$campo.length) return;
        window.setTimeout(function () {
            $campo.trigger('focus');
            if ($campo[0] && typeof $campo[0].select === 'function') $campo[0].select();
        }, 0);
    }

    // Muestra un error dentro del panel abierto sin repintar la tabla.
    function mostrarError(texto) {
        $('.linea-editor-error').text(texto || '');
    }

    window.LineaEditor = {
        htmlFila: htmlFila,
        enlazar: enlazar,
        enfocar: enfocar,
        mostrarError: mostrarError
    };
})(window, window.jQuery);
