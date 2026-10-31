/* Orden por encabezado de las tablas de items (Elaborados/Carga, Stock/Editar, Compras/Editar y los modales de acumulados).
   Es la misma logica que ya tenia Movimientos/Editar (movimientos.js, syncSortHeaders): clic en la columna ordena de menor a
   mayor, otro clic invierte. Cada pantalla guarda su estado { sortKey, sortDirection } y decide como dibujar las filas; esto
   solo compara, alterna y pinta la flecha. Detalle de la pantalla en docs/DECISIONS.md (2026-10-31). */
(function (window, $) {
    'use strict';

    // Compara dos valores de orden: texto con collation en espanol y numeros dentro del texto ("2" antes que "10"),
    // numeros por valor. null va primero. Devuelve <0, 0 o >0; el desempate y la direccion los resuelve quien llama.
    function comparar(izquierda, derecha) {
        if (izquierda === null && derecha === null) return 0;
        if (izquierda === null) return -1;
        if (derecha === null) return 1;
        if (typeof izquierda === 'string' || typeof derecha === 'string') {
            return String(izquierda).localeCompare(String(derecha), 'es', { numeric: true, sensitivity: 'base' });
        }
        return izquierda === derecha ? 0 : (izquierda > derecha ? 1 : -1);
    }

    // Clic en un encabezado: misma clave invierte la direccion, otra clave arranca de menor a mayor.
    function alternar(estado, clave) {
        if (estado.sortKey === clave) {
            estado.sortDirection = estado.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            estado.sortKey = clave;
            estado.sortDirection = 'asc';
        }
    }

    // Pinta la flecha y la clase "orden-activa" en los encabezados con data-sort-key. El texto original se guarda la primera vez
    // en data-base-label para no acumular flechas en cada redibujado.
    function sincronizarEncabezados($encabezados, estado) {
        $encabezados.each(function () {
            var $th = $(this);
            if (!$th.data('base-label')) {
                $th.data('base-label', $.trim($th.text()));
            }
            var etiqueta = $th.data('base-label');
            var activo = String($th.data('sort-key') || '') === estado.sortKey;
            var descendente = estado.sortDirection === 'desc';

            $th.text(activo ? etiqueta + (descendente ? ' ↓' : ' ↑') : etiqueta);
            $th.toggleClass('orden-activa', activo);
            $th.attr('title', activo
                ? 'Ordenado por ' + etiqueta + (descendente ? ' (descendente)' : ' (ascendente)')
                : 'Ordenar por ' + etiqueta);
        });
    }

    window.OrdenTabla = { comparar: comparar, alternar: alternar, sincronizarEncabezados: sincronizarEncabezados };
})(window, window.jQuery);
