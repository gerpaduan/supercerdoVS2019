/* Columna "Veces" y detalle por carga de los modales de acumulados (Movimientos/Editar, Stock/Editar y Elaborados/Carga).
   Problema que resuelve: el acumulado suma todas las cargas de un producto y oculta si se cargo dos veces por error. Aca se cuenta en
   cuantos items esta el producto (2 o mas = ambar, posible duplicado) y, al tocar la fila, se despliegan las cargas una por una con su
   numero de item (el # de la tabla principal) para poder ubicarlas y corregirlas.
   No agrupa ni ordena: cada pantalla sigue armando sus acumulados (buildAcumulados) y le pasa a este helper clave, veces y cargas.
   Estilos: ui-refresh.css (.tabla-acumulados, .acum-*). Detalle de la decision en docs/DECISIONS.md (2026-11-11). */
(function (window, $) {
    'use strict';

    // Escapa texto para usarlo en HTML o dentro de un atributo (el producto y la clave vienen de datos de usuario).
    function escapar(valor) {
        return String(valor === null || valor === undefined ? '' : valor)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    /* Crea el controlador de UN modal de acumulados.
       opciones.modal      selector del modal (contiene .js-acum-resumen y .js-acum-solo-rep).
       opciones.tabla      selector de la tabla de acumulados (las filas llevan los atributos de atributosFila).
       opciones.alCambiar  funcion que redibuja la tabla; se llama al abrir/cerrar un detalle o al tildar "Solo repetidos".
       El estado (detalles abiertos, filtro) vive en la instancia y se limpia con reiniciar() cada vez que se abre el modal. */
    function crear(opciones) {
        var abiertos = {};
        var soloRepetidos = false;

        // Redibuja y devuelve el foco a la fila que se acaba de tocar (el redibujado reemplaza el DOM y el teclado se perderia).
        function alternarFila($fila) {
            var clave = String($fila.attr('data-clave'));
            if (abiertos[clave]) {
                delete abiertos[clave];
            } else {
                abiertos[clave] = true;
            }
            opciones.alCambiar();
            $(opciones.tabla + ' tbody tr.acum-fila').filter(function () {
                return String($(this).attr('data-clave')) === clave;
            }).first().trigger('focus');
        }

        // Clic en una fila de acumulado: abre o cierra su detalle. Delegado en document porque el modal esta fuera del form.
        $(document).on('click', opciones.tabla + ' tbody tr.acum-fila', function () {
            alternarFila($(this));
        });

        // Teclado: Enter o espacio sobre la fila enfocada hacen lo mismo que el clic.
        $(document).on('keydown', opciones.tabla + ' tbody tr.acum-fila', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                alternarFila($(this));
            }
        });

        $(document).on('change', opciones.modal + ' .js-acum-solo-rep', function () {
            soloRepetidos = $(this).is(':checked');
            opciones.alCambiar();
        });

        return {
            // Cierra todos los detalles y apaga "Solo repetidos": cada apertura del modal arranca limpia.
            reiniciar: function () {
                abiertos = {};
                soloRepetidos = false;
                $(opciones.modal + ' .js-acum-solo-rep').prop('checked', false);
            },

            soloRepetidos: function () {
                return soloRepetidos;
            },

            // Texto de arriba: "N productos distintos en M items" + aviso si hay repetidos.
            pintarResumen: function (productos, items, repetidos) {
                var texto = productos + (productos === 1 ? ' producto distinto en ' : ' productos distintos en ') + items + (items === 1 ? ' ítem' : ' ítems');
                if (repetidos > 0) {
                    texto += ' · <span class="acum-aviso"><i class="fas fa-exclamation-triangle"></i> ' + repetidos + (repetidos === 1 ? ' repetido' : ' repetidos') + '</span>';
                }
                $(opciones.modal + ' .js-acum-resumen').html(texto);
            },

            // Texto de la fila vacia: con el filtro encendido no es "no hay productos" sino "no hay repetidos".
            textoVacio: function (textoSinProductos) {
                return soloRepetidos ? 'No hay productos repetidos.' : textoSinProductos;
            },

            // Contenido de la celda "Veces": globito con la cantidad de items (ambar si es mas de uno) y la flecha del detalle.
            vecesHtml: function (veces) {
                var repetido = veces > 1;
                return '<span class="acum-veces' + (repetido ? ' rep' : '') + '" title="' + (repetido
                    ? 'Este producto está cargado en ' + veces + ' ítems: revisá que no sea un duplicado'
                    : 'Cargado en un solo ítem') + '">'
                    + (repetido ? '<i class="fas fa-exclamation-triangle"></i> ' : '') + '×' + veces + '</span>'
                    + '<i class="fas fa-chevron-right acum-chev" aria-hidden="true"></i>';
            },

            // Atributos del <tr> del acumulado: clase (repetido / abierta), clave para encontrarla, foco y estado para lectores de pantalla.
            atributosFila: function (clave, veces) {
                var abierta = !!abiertos[String(clave)];
                return ' class="acum-fila' + (veces > 1 ? ' rep' : '') + (abierta ? ' abierta' : '') + '"'
                    + ' data-clave="' + escapar(clave) + '" tabindex="0" role="button" aria-expanded="' + (abierta ? 'true' : 'false') + '"';
            },

            /* Fila extra con las cargas de un producto, o '' si su detalle esta cerrado.
               cargas   = [{ item: n, ... }] en orden de carga; "item" es el # de la tabla principal.
               columnas = [{ titulo, valor: function (carga) -> html, total: html }] (las columnas de cantidad, alineadas a la derecha);
                          la columna "Item" va siempre primera. */
            filaDetalle: function (clave, producto, veces, cargas, columnas, colspan) {
                if (!abiertos[String(clave)]) return '';

                var html = '<tr class="acum-detalle' + (veces > 1 ? ' rep' : '') + '"><td colspan="' + colspan + '">'
                    + '<div class="acum-det-tit">Cargas de ' + escapar(producto) + '</div>'
                    + '<table class="acum-det"><thead><tr><th class="text-center">Ítem</th>';
                columnas.forEach(function (columna) {
                    html += '<th class="text-right">' + escapar(columna.titulo) + '</th>';
                });
                html += '</tr></thead><tbody>';
                cargas.forEach(function (carga) {
                    html += '<tr><td class="text-center acum-det-item">#' + escapar(carga.item) + '</td>';
                    columnas.forEach(function (columna) {
                        html += '<td class="text-right">' + columna.valor(carga) + '</td>';
                    });
                    html += '</tr>';
                });
                html += '</tbody><tfoot><tr><td class="text-center">Total</td>';
                columnas.forEach(function (columna) {
                    html += '<td class="text-right">' + columna.total + '</td>';
                });
                html += '</tr></tfoot></table></td></tr>';
                return html;
            }
        };
    }

    window.AcumuladosDetalle = { crear: crear };
})(window, window.jQuery);
