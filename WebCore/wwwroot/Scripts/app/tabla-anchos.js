// Columnas redimensionables para tablas de layout fijo (hoy: las 4 tablas de Reportes). Arrastrando el borde derecho de un
// encabezado th[data-rs="<tipo>"] cambia el ancho de TODAS las columnas de ese tipo (ej. todas las "Kg" de cada periodo).
// Dos partes: (1) logica pura (topes, leer/guardar lo de localStorage), testeada en WebCore.Tests/js/tabla-anchos.test.js, y
// (2) el cableado del navegador (manijas, arrastre, minimo medido, doble clic, restablecer). Mismo contrato que
// existencia-anchos.js, que es propio de Existencia y no se toca. Porque y reglas: docs/DECISIONS.md 2026-11-24.
(function (global) {
    'use strict';

    // ---------- Parte 1: logica pura (sin DOM) ----------

    // Crea el modelo de anchos de una tabla. columnas = { tipo: { min, max } } en px (ancho total del th). Devuelve funciones
    // puras que solo aceptan esos tipos: cualquier otro dato (tipo desconocido, valor no numerico) se descarta.
    function crear(columnas) {
        var cols = {};
        Object.keys(columnas || {}).forEach(function (tipo) {
            var c = columnas[tipo] || {};
            var min = Number(c.min), max = Number(c.max);
            if (isFinite(min) && isFinite(max) && min > 0 && max >= min) cols[tipo] = { min: min, max: max };
        });

        function esColumna(tipo) {
            return typeof tipo === 'string' && Object.prototype.hasOwnProperty.call(cols, tipo);
        }

        // Recorta un ancho a los topes del tipo y lo redondea a px enteros. minimoExtra (opcional) sube el piso: es el
        // contenido mas ancho medido en pantalla, para que un numero nunca quede cortado. Devuelve null si no es valido.
        function limitar(tipo, px, minimoExtra) {
            if (!esColumna(tipo)) return null;
            if (typeof px === 'boolean' || px === null || px === '' || px === undefined) return null;
            var n = Number(px);
            if (!isFinite(n)) return null;
            var piso = cols[tipo].min;
            var extra = Number(minimoExtra);
            if (isFinite(extra) && extra > piso) piso = Math.min(extra, cols[tipo].max);
            return Math.round(Math.min(cols[tipo].max, Math.max(piso, n)));
        }

        // Lee lo guardado (JSON de localStorage). Es entrada externa: JSON roto, tipos desconocidos o valores raros se
        // descartan en silencio y esa columna queda con su ancho por defecto. Siempre devuelve un objeto { tipo: px }.
        function leer(texto) {
            var anchos = {};
            if (typeof texto !== 'string' || !texto) return anchos;
            var crudo;
            try {
                crudo = JSON.parse(texto);
            } catch (e) {
                return anchos; // guardado corrupto: se arranca con los anchos por defecto (no vale la pena avisar)
            }
            if (!crudo || typeof crudo !== 'object' || Array.isArray(crudo)) return anchos;
            Object.keys(cols).forEach(function (tipo) {
                if (!Object.prototype.hasOwnProperty.call(crudo, tipo)) return;
                var px = limitar(tipo, crudo[tipo]);
                if (px !== null) anchos[tipo] = px;
            });
            return anchos;
        }

        // Texto para guardar; null si no hay ningun ancho cambiado (la pagina borra la clave en vez de guardar "{}").
        function serializar(anchos) {
            var limpio = leer(JSON.stringify(anchos || {}));
            return Object.keys(limpio).length ? JSON.stringify(limpio) : null;
        }

        // Ancho nuevo al arrastrar: el de arranque mas lo que se movio el mouse, dentro de los topes y del minimo medido.
        function anchoArrastrado(tipo, pxInicial, desplazamiento, minimoExtra) {
            return limitar(tipo, Number(pxInicial) + Number(desplazamiento), minimoExtra);
        }

        return { columnas: cols, esColumna: esColumna, limitar: limitar, leer: leer, serializar: serializar, anchoArrastrado: anchoArrastrado };
    }

    // Columna logica (contando colspan y rowspan) de cada celda de una seccion de tabla. Hace falta porque cellIndex no sirve
    // con encabezados de dos filas (Proyeccion con varias sucursales). filas = [[{colSpan, rowSpan}, ...], ...] (cualquier
    // objeto con esas dos propiedades, asi se testea sin DOM). Devuelve [[columna, ...], ...] con la misma forma.
    function columnasLogicas(filas) {
        var ocupadas = []; // ocupadas[fila][columna] = true por un rowspan de una fila de arriba
        return filas.map(function (celdas, f) {
            var col = 0;
            ocupadas[f] = ocupadas[f] || [];
            return celdas.map(function (celda) {
                while (ocupadas[f][col]) col++;
                var inicio = col;
                var span = Math.max(1, Number(celda.colSpan) || 1);
                var alto = Math.max(1, Number(celda.rowSpan) || 1);
                for (var r = 1; r < alto; r++) {
                    ocupadas[f + r] = ocupadas[f + r] || [];
                    for (var c = 0; c < span; c++) ocupadas[f + r][inicio + c] = true;
                }
                col += span;
                return inicio;
            });
        });
    }

    // ---------- Parte 2: navegador ----------

    var ESPERA_CLICK_TRAS_ARRASTRE_MS = 300; // el click que dispara soltar el mouse no tiene que ordenar la columna

    function leerAlmacen(clave) {
        try {
            return global.localStorage.getItem(clave);
        } catch (e) {
            return null; // almacenamiento bloqueado (ventana privada, politicas): la tabla funciona con los anchos por defecto
        }
    }

    function guardarAlmacen(clave, texto) {
        try {
            if (texto === null) {
                global.localStorage.removeItem(clave);
            } else {
                global.localStorage.setItem(clave, texto);
            }
        } catch (e) { /* sin almacenamiento: el ancho no se recuerda al recargar, no es un error */ }
    }

    // Ancho del contenido que NO se puede cortar en una celda: el elemento indicado por "selector" (ej. el numero de Faltante)
    // o, si no hay selector, todo el contenido. Se mide con un Range: da el ancho real del texto aunque la celda sea mas angosta
    // (con overflow: hidden el texto sigue ocupando su lugar). Suma el padding de la celda y su borde.
    function anchoContenido(celda, selector) {
        var objetivos = selector ? celda.querySelectorAll(selector) : [celda];
        var ancho = 0;
        for (var i = 0; i < objetivos.length; i++) {
            var rango = document.createRange();
            rango.selectNodeContents(objetivos[i]);
            ancho = Math.max(ancho, rango.getBoundingClientRect().width);
        }
        if (!ancho) return 0;
        var cs = global.getComputedStyle(celda);
        return ancho + parseFloat(cs.paddingLeft) + parseFloat(cs.paddingRight)
            + parseFloat(cs.borderLeftWidth) + parseFloat(cs.borderRightWidth) + 2; // +2: redondeo del navegador
    }

    // Activa la redimension en una tabla. opciones:
    //   clave: clave de localStorage (una por tabla).
    //   columnas: { tipo: { min, max, contenido } } en px; contenido = null (sin minimo medido: ej. Producto, que se corta con
    //             "..."), "" (todo el contenido de la celda) o un selector (la parte que no se puede cortar).
    //   boton: elemento opcional "Anchos" que restablece todo; se muestra solo si hay algun ancho cambiado.
    //   sinManijas: media query en la que no hay redimension (celular): ahi rigen los anchos del CSS y no se ven las manijas
    //               (el CSS las oculta con la misma media query).
    // El ancho elegido se escribe como variable CSS --rs-<tipo> en la tabla, y el CSS de cada tabla la usa en el th Y en las td de
    // ese tipo con el ancho de siempre como valor por defecto (ej. width: var(--rs-faltante, 32ch)). Hace falta en las dos: Chrome
    // tambien respeta el ancho de las td en layout fijo (medido 2026-11-24: con ancho solo en el th la columna no cambiaba).
    // Mientras haya algun ancho elegido la tabla lleva la clase tabla-rs-con-anchos (Balance la usa para dejar de estirarse).
    function activar(tabla, opciones) {
        if (!tabla || !opciones || !opciones.clave) return null;
        var config = opciones.columnas || {};
        var modelo = crear(config);
        var anchos = modelo.leer(leerAlmacen(opciones.clave));
        var boton = opciones.boton || null;
        var mediaSinManijas = opciones.sinManijas && global.matchMedia ? global.matchMedia(opciones.sinManijas) : null;
        var finArrastre = 0;

        function aplicar() {
            var activo = !(mediaSinManijas && mediaSinManijas.matches);
            var hay = false;
            Object.keys(modelo.columnas).forEach(function (tipo) {
                if (activo && anchos[tipo]) {
                    tabla.style.setProperty('--rs-' + tipo, anchos[tipo] + 'px');
                    hay = true;
                } else {
                    tabla.style.removeProperty('--rs-' + tipo);
                }
            });
            tabla.classList.toggle('tabla-rs-con-anchos', hay);
            if (boton) boton.classList.toggle('d-none', !hay);
        }
        if (mediaSinManijas && mediaSinManijas.addEventListener) mediaSinManijas.addEventListener('change', aplicar);

        function guardar() {
            guardarAlmacen(opciones.clave, modelo.serializar(anchos));
        }

        // Columna logica de cada th del encabezado (sirve con colspan / rowspan).
        function mapaEncabezado() {
            var filas = Array.prototype.slice.call(tabla.tHead ? tabla.tHead.rows : []);
            var celdas = filas.map(function (fila) { return Array.prototype.slice.call(fila.cells); });
            var logicas = columnasLogicas(celdas);
            var mapa = [];
            celdas.forEach(function (fila, f) {
                fila.forEach(function (th, c) { mapa.push({ th: th, columna: logicas[f][c] }); });
            });
            return mapa;
        }

        // Minimo medido del tipo: el contenido mas ancho que no se puede cortar, entre las filas visibles de todas sus columnas.
        // Las filas del cuerpo no tienen colspan (salvo las filas de detalle/grafico, que se saltean), asi que la columna logica
        // coincide con el indice de la celda.
        function minimoMedido(tipo) {
            var c = config[tipo] || {};
            if (c.contenido === null || c.contenido === undefined) return 0;
            var columnas = mapaEncabezado().filter(function (x) {
                return x.th.getAttribute('data-rs') === tipo && x.th.colSpan === 1;
            }).map(function (x) { return x.columna; });
            var maximo = 0;
            var cuerpos = Array.prototype.slice.call(tabla.tBodies);
            cuerpos.forEach(function (tbody) {
                Array.prototype.forEach.call(tbody.rows, function (fila) {
                    if (fila.offsetParent === null) return; // fila oculta por un filtro
                    var celdas = fila.cells;
                    if (celdas.length === 1 && celdas[0].colSpan > 1) return; // fila de detalle o grafico
                    columnas.forEach(function (col) {
                        var celda = celdas[col];
                        if (celda && celda.offsetParent !== null) maximo = Math.max(maximo, anchoContenido(celda, c.contenido));
                    });
                });
            });
            return maximo;
        }

        // Manija en cada encabezado redimensionable. El th tiene que estar posicionado para ubicarla; si ya es sticky se deja asi.
        Array.prototype.forEach.call(tabla.querySelectorAll('thead th[data-rs]'), function (th) {
            if (!modelo.esColumna(th.getAttribute('data-rs'))) return;
            if (global.getComputedStyle(th).position === 'static') th.style.position = 'relative';
            var grip = document.createElement('span');
            grip.className = 'tabla-rs-grip';
            grip.setAttribute('aria-hidden', 'true');
            th.appendChild(grip);
        });
        tabla.classList.add('tabla-rs');

        tabla.addEventListener('pointerdown', function (e) {
            var grip = e.target.closest ? e.target.closest('.tabla-rs-grip') : null;
            if (!grip || e.button !== 0) return;
            var th = grip.parentNode;
            var tipo = th.getAttribute('data-rs');
            if (!modelo.esColumna(tipo)) return;
            e.preventDefault();      // sin seleccion de texto mientras se arrastra
            e.stopPropagation();

            var px0 = th.getBoundingClientRect().width;
            // Nunca mas que el ancho de arranque: si el contenido ya no entraba con el ancho de siempre (ej. un stock de 10 cifras),
            // empezar a angostar no tiene que ensanchar la columna de golpe; solo no la deja angostar mas.
            var minimo = Math.min(minimoMedido(tipo), px0);
            var x0 = e.clientX;
            var movio = false;
            var pendiente = false;
            var ultimoX = x0;
            grip.classList.add('tabla-rs-activo');
            document.body.classList.add('tabla-rs-arrastrando');

            function pintar() {
                pendiente = false;
                var px = modelo.anchoArrastrado(tipo, px0, ultimoX - x0, minimo);
                if (px === null || px === anchos[tipo]) return;
                movio = true;
                anchos[tipo] = px;
                aplicar();
            }
            // Se escucha en el documento: el mouse sale de la manija apenas empieza a arrastrar. Un cambio por cuadro.
            function mover(ev) {
                ultimoX = ev.clientX;
                if (pendiente) return;
                pendiente = true;
                global.requestAnimationFrame(pintar);
            }
            function soltar() {
                document.removeEventListener('pointermove', mover);
                document.removeEventListener('pointerup', soltar);
                document.removeEventListener('pointercancel', soltar);
                if (pendiente) pintar();
                grip.classList.remove('tabla-rs-activo');
                document.body.classList.remove('tabla-rs-arrastrando');
                if (!movio) return;
                finArrastre = Date.now();
                guardar();
            }
            document.addEventListener('pointermove', mover);
            document.addEventListener('pointerup', soltar);
            document.addEventListener('pointercancel', soltar);
        });

        // En fase de captura: corta el click antes de que llegue a los handlers de ordenar (delegados en document) si viene de la
        // manija o de soltar un arrastre.
        tabla.addEventListener('click', function (e) {
            var enManija = e.target.closest && e.target.closest('.tabla-rs-grip');
            if (enManija || Date.now() - finArrastre < ESPERA_CLICK_TRAS_ARRASTRE_MS) {
                e.stopPropagation();
                e.preventDefault();
            }
        }, true);

        // Doble clic en la manija: esa columna vuelve a su ancho por defecto.
        tabla.addEventListener('dblclick', function (e) {
            var grip = e.target.closest ? e.target.closest('.tabla-rs-grip') : null;
            if (!grip) return;
            e.stopPropagation();
            delete anchos[grip.parentNode.getAttribute('data-rs')];
            aplicar();
            guardar();
        }, true);

        function restablecer() {
            anchos = {};
            aplicar();
            guardar();
        }
        if (boton) boton.addEventListener('click', restablecer);

        aplicar();
        return { restablecer: restablecer, anchos: function () { return JSON.parse(JSON.stringify(anchos)); } };
    }

    global.TablaAnchos = { crear: crear, columnasLogicas: columnasLogicas, activar: activar };
})(window);
