(function () {
    function normalizarTexto(value) {
        return (value || '').toString().toLowerCase().trim();
    }

    function toggleAvisoFiltrosPendientes(show) {
        var alerta = document.getElementById('alertaFiltrosPendientes');
        if (!alerta) return;
        alerta.classList.toggle('d-none', !show);
    }

    function bindFiltrosPendientes(page) {
        var recarga = page.querySelectorAll('.js-filtro-recarga');
        Array.prototype.forEach.call(recarga, function (input) {
            input.addEventListener('change', function () {
                toggleAvisoFiltrosPendientes(true);
            });
        });

        var form = page.querySelector('.js-form-filtros-lineas');
        if (form) {
            form.addEventListener('submit', function () {
                toggleAvisoFiltrosPendientes(false);
            });
        }
    }

    function syncDetalles(page) {
        var expandir = false;
        var toggle = page.querySelector('.js-toggle-detalles-general');
        if (toggle) expandir = !!toggle.checked;

        Array.prototype.forEach.call(page.querySelectorAll('.linea-registro-extra'), function (item) {
            item.classList.toggle('d-none', !expandir);
        });
    }

    function getLiveFilterValue(page, key) {
        var input = page.querySelector('[data-live-filter="' + key + '"]');
        if (!input) return '';

        if (input.type === 'checkbox') {
            return input.checked ? '1' : '';
        }

        return normalizarTexto(input.value);
    }

    function getCheckedValues(page, key) {
        var items = page.querySelectorAll('[data-live-filter="' + key + '"]:checked');
        return Array.prototype.map.call(items, function (item) { return normalizarTexto(item.value); });
    }

    // IDs elegidos en el selector multi-seleccion (filtro-picker.js) para un filtro; [] si esta en modo texto libre.
    // El selector deja el CSV en data-ids del input y, con seleccion, el valor del input es solo un resumen.
    function getSelectedIds(page, key) {
        var input = page.querySelector('[data-live-filter="' + key + '"]');
        var csv = input && input.dataset ? input.dataset.ids : '';
        return csv ? csv.split(',').filter(Boolean) : [];
    }

    function cardMatchesGroupFilters(card, page) {
        var groupFilters = ['cliente', 'vendedor', 'proveedor', 'tipoCompra', 'tipoOperacion', 'estado', 'formaPago'];
        for (var i = 0; i < groupFilters.length; i++) {
            var key = groupFilters[i];

            // Seleccion exacta (cliente / vendedor / proveedor): la card tiene que traer su ID en data-id-<filtro>.
            var ids = getSelectedIds(page, key);
            if (ids.length > 0) {
                if (ids.indexOf(card.getAttribute('data-id-' + key) || '') < 0) return false;
                continue;
            }

            if (key === 'formaPago') {
                var pagos = getCheckedValues(page, key);
                if (pagos.length > 0) {
                    var valuePago = normalizarTexto(card.getAttribute('data-' + key));
                    if (pagos.indexOf(valuePago) < 0) return false;
                }
                continue;
            }

            var value = getLiveFilterValue(page, key);
            if (!value) continue;

            var cardValue = normalizarTexto(card.getAttribute('data-' + key));
            if (cardValue.indexOf(value) < 0) return false;
        }

        return true;
    }

    function lineMatchesFilters(line, page) {
        var producto = getLiveFilterValue(page, 'producto');
        if (!producto) return true;

        var codigo = normalizarTexto(line.getAttribute('data-codigo'));
        var descripcion = normalizarTexto(line.getAttribute('data-producto'));
        return codigo.indexOf(producto) >= 0 || descripcion.indexOf(producto) >= 0;
    }

    function formatNumber(value, decimals) {
        return Number(value || 0).toLocaleString('es-AR', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        });
    }

    function formatCurrency(value) {
        return Number(value || 0).toLocaleString('es-AR', {
            style: 'currency',
            currency: 'ARS'
        });
    }

    function parseDecimal(value) {
        if (value === null || value === undefined) return 0;

        var text = value.toString().trim();
        if (!text) return 0;

        text = text.replace(/\s+/g, '');

        var commaIndex = text.lastIndexOf(',');
        var dotIndex = text.lastIndexOf('.');

        if (commaIndex >= 0 && dotIndex >= 0) {
            if (commaIndex > dotIndex) {
                text = text.replace(/\./g, '').replace(',', '.');
            } else {
                text = text.replace(/,/g, '');
            }
        } else if (commaIndex >= 0) {
            text = text.replace(',', '.');
        }

        var parsed = parseFloat(text);
        return isNaN(parsed) ? 0 : parsed;
    }

    function renderClientEmpty(page, visibleCards) {
        var empty = page.querySelector('.js-lineas-empty-client');
        var server = page.querySelector('.js-lineas-empty-server');

        if (server) server.classList.toggle('d-none', visibleCards > 0);
        if (empty) empty.classList.toggle('d-none', visibleCards > 0);
    }

    function recalcTotals(page) {
        var cards = page.querySelectorAll('.js-registro-card:not(.d-none)');
        var totalRegistros = 0;
        var totalLineas = 0;
        var totalKg = 0;
        var totalImporte = 0;
        var totalUnidades = 0;

        Array.prototype.forEach.call(cards, function (card) {
            totalRegistros++;
            var rows = card.querySelectorAll('.js-linea-row:not(.d-none)');
            Array.prototype.forEach.call(rows, function (row) {
                totalLineas++;
                totalKg += parseDecimal(row.getAttribute('data-kg'));
                totalImporte += parseDecimal(row.getAttribute('data-total'));
                totalUnidades += parseDecimal(row.getAttribute('data-unidades'));
            });
        });

        var map = {
            totalRegistros: totalRegistros,
            totalLineas: totalLineas,
            totalKg: formatNumber(totalKg, 3),
            totalImporte: formatCurrency(totalImporte),
            totalUnidades: formatNumber(totalUnidades, 0)
        };

        Object.keys(map).forEach(function (key) {
            var el = document.getElementById(key);
            if (el) el.textContent = map[key];
        });
    }

    function filtrar(page) {
        var cards = page.querySelectorAll('.js-registro-card');
        var visibles = 0;

        Array.prototype.forEach.call(cards, function (card) {
            var groupOk = cardMatchesGroupFilters(card, page);
            var lineasVisibles = 0;
            var rows = card.querySelectorAll('.js-linea-row');

            Array.prototype.forEach.call(rows, function (row) {
                var match = groupOk && lineMatchesFilters(row, page);
                row.classList.toggle('d-none', !match);
                if (match) lineasVisibles++;
            });

            var showCard = groupOk && lineasVisibles > 0;
            card.classList.toggle('d-none', !showCard);
            if (showCard) visibles++;
        });

        renderClientEmpty(page, visibles);
        recalcTotals(page);
        sincronizarVistaPlana();
    }

    // ---------------------------------------------------------------------------------------------
    // Vista "Ver solo lineas" (Compras y Ventas): tabla plana con una fila por linea, ordenable.
    // Solo se activa si la pantalla tiene el boton #btnVistaSoloLineas (Stock y Movimientos no lo tienen).
    // La fuente de datos son las tarjetas de siempre: la tabla se arma UNA vez (la primera vez que se activa) y despues
    // solo se sincroniza su visibilidad con los filtros en vivo y se reordena; las tarjetas no se ocultan (los exports
    // las consultan), se oculta su contenedor .js-vista-agrupada.
    // ---------------------------------------------------------------------------------------------
    var vistaPlana = null;
    var comparadorTexto = (typeof Intl !== 'undefined' && Intl.Collator)
        ? new Intl.Collator('es', { numeric: true, sensitivity: 'base' })
        : { compare: function (a, b) { return a < b ? -1 : (a > b ? 1 : 0); } };

    function escaparHtml(value) {
        return (value === null || value === undefined ? '' : String(value))
            .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    // Pares etiqueta/valor de la cabecera de una tarjeta (los que muestra "Ver detalles").
    function leerCamposCabecera(card) {
        var campos = [];
        Array.prototype.forEach.call(card.querySelectorAll('.linea-registro-extra > div'), function (col) {
            var etiqueta = col.children[0];
            var valor = col.children[1];
            if (!etiqueta || !valor) return;
            campos.push({ etiqueta: etiqueta.textContent.trim(), valor: valor.textContent.trim() });
        });
        return campos;
    }

    function definirColumnasVistaPlana(camposCabecera) {
        var columnas = [
            { clave: 'fecha', texto: 'Fecha', tipo: 'fecha', primerOrden: 'desc' },
            { clave: 'creado', texto: 'Creado', tipo: 'fecha', primerOrden: 'desc' }
        ];

        // Datos de la cabecera de la compra/venta (proveedor, sucursal, cliente, vendedor...), por etiqueta: asi la misma
        // logica sirve para Compras y Ventas. "Fecha" ya es la primera columna.
        camposCabecera.forEach(function (campo) {
            if (campo.etiqueta.toLowerCase() === 'fecha') return;
            columnas.push({ clave: 'campo:' + campo.etiqueta, texto: campo.etiqueta, tipo: 'texto', etiquetaCampo: campo.etiqueta });
        });

        columnas.push({ clave: 'codigo', texto: 'Código', tipo: 'codigo' });
        columnas.push({ clave: 'producto', texto: 'Producto', tipo: 'texto' });
        columnas.push({ clave: 'kg', texto: 'Cantidad / kg', tipo: 'num', derecha: true });
        columnas.push({ clave: 'precio', texto: 'Precio', tipo: 'num', derecha: true });
        columnas.push({ clave: 'total', texto: 'Total línea', tipo: 'num', derecha: true });
        columnas.push({ clave: 'acciones', texto: '', tipo: 'acciones' });
        return columnas;
    }

    function construirVistaPlana(page) {
        var cards = page.querySelectorAll('.js-registro-card');
        var camposPrimera = cards.length > 0 ? leerCamposCabecera(cards[0]) : [];
        var columnas = definirColumnasVistaPlana(camposPrimera);
        var filas = [];
        var html = [];

        Array.prototype.forEach.call(cards, function (card) {
            var porEtiqueta = {};
            leerCamposCabecera(card).forEach(function (campo) { porEtiqueta[campo.etiqueta] = campo.valor; });

            var creadoTexto = card.getAttribute('data-creado-texto') || '-';
            var enlace = card.querySelector('.linea-resumen-tools a.btn');
            var href = enlace ? enlace.getAttribute('href') : '';

            Array.prototype.forEach.call(card.querySelectorAll('.js-linea-row'), function (row) {
                var celdas = row.children;
                var textos = {
                    fecha: porEtiqueta['Fecha'] || '',
                    creado: creadoTexto,
                    codigo: celdas[0] ? celdas[0].textContent.trim() : '',
                    producto: celdas[1] ? celdas[1].textContent.trim() : '',
                    kg: celdas[2] ? celdas[2].textContent.trim() : '',
                    precio: celdas[3] ? celdas[3].textContent.trim() : '',
                    total: celdas[4] ? celdas[4].textContent.trim() : ''
                };

                // Valores para ordenar: fechas en ISO (se comparan como texto), numeros reales, textos sin mayusculas.
                var orden = {
                    fecha: card.getAttribute('data-fecha') || '',
                    creado: card.getAttribute('data-creado') || '',
                    codigo: textos.codigo,
                    producto: normalizarTexto(textos.producto),
                    kg: parseDecimal(row.getAttribute('data-kg')),
                    precio: parseDecimal(row.getAttribute('data-precio')),
                    total: parseDecimal(row.getAttribute('data-total'))
                };

                var celdasHtml = columnas.map(function (col) {
                    if (col.tipo === 'acciones') {
                        return '<td class="text-nowrap">' + (href
                            ? '<a class="btn btn-sm btn-outline-primary" href="' + escaparHtml(href) + '" title="Ver registro" aria-label="Ver registro"><i class="fas fa-eye"></i></a>'
                            : '') + '</td>';
                    }

                    var texto;
                    if (col.etiquetaCampo) {
                        texto = porEtiqueta[col.etiquetaCampo] || '';
                        orden[col.clave] = normalizarTexto(texto);
                    } else {
                        texto = textos[col.clave];
                    }

                    var clases = [];
                    if (col.derecha) clases.push('text-right');
                    // Solo "Fecha" va en una linea; "Creado" (con segundos) puede partirse para que la tabla entre en pantalla.
                    if (col.clave === 'fecha') clases.push('js-plana-nowrap');
                    return '<td' + (clases.length ? ' class="' + clases.join(' ') + '"' : '') + '>' + escaparHtml(texto) + '</td>';
                });

                html.push('<tr>' + celdasHtml.join('') + '</tr>');
                filas.push({ tr: null, card: card, row: row, orden: orden, idx: filas.length });
            });
        });

        var cuerpo = page.querySelector('.js-vista-plana .js-plana-cuerpo');
        var encabezado = page.querySelector('.js-vista-plana .js-plana-encabezado');
        cuerpo.innerHTML = html.join('');
        Array.prototype.forEach.call(cuerpo.children, function (tr, i) { filas[i].tr = tr; });

        encabezado.innerHTML = columnas.map(function (col) {
            if (col.tipo === 'acciones') return '<th></th>';
            return '<th class="js-plana-ordenable' + (col.derecha ? ' text-right' : '') + '" data-clave="' + escaparHtml(col.clave)
                // <wbr> tras la "/" deja partir "Nro/comprobante" en dos renglones (si no es la columna mas ancha de la tabla).
                + '" title="Ordenar por ' + escaparHtml(col.texto) + '">' + escaparHtml(col.texto).replace(/\//g, '/<wbr>') + '<span class="js-plana-flecha"></span></th>';
        }).join('');

        // Un solo listener en el encabezado: primer clic en una columna ordena ascendente (fechas: la mas reciente primero); otro clic invierte.
        encabezado.addEventListener('click', function (event) {
            var th = event.target.closest ? event.target.closest('th.js-plana-ordenable') : null;
            if (!th) return;

            var clave = th.getAttribute('data-clave');
            var col = vistaPlana.columnas.filter(function (c) { return c.clave === clave; })[0];
            if (!col) return;

            if (vistaPlana.orden.clave === clave) {
                vistaPlana.orden.dir = vistaPlana.orden.dir === 'asc' ? 'desc' : 'asc';
            } else {
                vistaPlana.orden = { clave: clave, dir: col.primerOrden || 'asc' };
            }
            ordenarVistaPlana();
        });

        vistaPlana.columnas = columnas;
        vistaPlana.filas = filas;
        vistaPlana.cuerpo = cuerpo;
        vistaPlana.encabezado = encabezado;
        vistaPlana.construida = true;
        ordenarVistaPlana();
    }

    // Reordena las filas ya creadas (las vuelve a anexar en el orden nuevo) y actualiza las flechas del encabezado.
    function ordenarVistaPlana() {
        var criterio = vistaPlana.orden;
        var col = vistaPlana.columnas.filter(function (c) { return c.clave === criterio.clave; })[0];
        if (!col) return;

        var sentido = criterio.dir === 'asc' ? 1 : -1;
        var copia = vistaPlana.filas.slice();

        copia.sort(function (a, b) {
            var va = a.orden[criterio.clave];
            var vb = b.orden[criterio.clave];

            // Sin dato ('' o '-', p. ej. codigo de una media res) siempre al final, sea cual sea el sentido.
            if (col.tipo !== 'num') {
                var vaVacio = va === '' || va === '-' || va === undefined;
                var vbVacio = vb === '' || vb === '-' || vb === undefined;
                if (vaVacio !== vbVacio) return vaVacio ? 1 : -1;
            }

            var cmp;
            if (col.tipo === 'num') cmp = va - vb;
            else if (col.tipo === 'fecha') cmp = va < vb ? -1 : (va > vb ? 1 : 0);
            else cmp = comparadorTexto.compare(String(va || ''), String(vb || ''));

            // Empate: se mantiene el orden original (estable) sin importar el sentido.
            return cmp === 0 ? a.idx - b.idx : cmp * sentido;
        });

        var fragmento = document.createDocumentFragment();
        copia.forEach(function (fila) { fragmento.appendChild(fila.tr); });
        vistaPlana.cuerpo.appendChild(fragmento);

        Array.prototype.forEach.call(vistaPlana.encabezado.querySelectorAll('th.js-plana-ordenable'), function (th) {
            var activa = th.getAttribute('data-clave') === criterio.clave;
            th.querySelector('.js-plana-flecha').textContent = activa ? (criterio.dir === 'asc' ? '▲' : '▼') : '';
            if (activa) th.setAttribute('aria-sort', criterio.dir === 'asc' ? 'ascending' : 'descending');
            else th.removeAttribute('aria-sort');
        });
    }

    // Muestra u oculta cada fila plana segun el estado de su linea y su tarjeta (los filtros en vivo ya las dejaron resueltas).
    function sincronizarVistaPlana() {
        if (!vistaPlana || !vistaPlana.activa || !vistaPlana.construida) return;

        vistaPlana.filas.forEach(function (fila) {
            var oculta = fila.row.classList.contains('d-none') || fila.card.classList.contains('d-none');
            fila.tr.classList.toggle('d-none', oculta);
        });
    }

    function claveAlmacenamientoVista(boton) {
        return 'lineasVistaSoloLineas_' + (boton.getAttribute('data-clave-vista') || 'general');
    }

    function aplicarModoVista(page, boton, activa) {
        if (activa && !vistaPlana.construida) construirVistaPlana(page);
        vistaPlana.activa = activa;

        var agrupada = page.querySelector('.js-vista-agrupada');
        var plana = page.querySelector('.js-vista-plana');
        if (agrupada) agrupada.classList.toggle('d-none', activa);
        if (plana) plana.classList.toggle('d-none', !activa);

        // "Ver detalles" solo tiene sentido en la vista agrupada.
        var switchDetalles = page.querySelector('.js-detalles-switch-wrap');
        if (switchDetalles) switchDetalles.classList.toggle('d-none', activa);

        var texto = boton.querySelector('.js-vista-texto');
        if (texto) texto.textContent = boton.getAttribute(activa ? 'data-texto-agrupada' : 'data-texto-plana');
        var icono = boton.querySelector('i');
        if (icono) {
            icono.classList.toggle('fa-list', !activa);
            icono.classList.toggle('fa-layer-group', activa);
        }
        boton.setAttribute('aria-pressed', activa ? 'true' : 'false');

        sincronizarVistaPlana();
    }

    function iniciarVistaPlana(page) {
        var boton = page.querySelector('#btnVistaSoloLineas');
        if (!boton) return;

        vistaPlana = { activa: false, construida: false, filas: [], columnas: [], cuerpo: null, encabezado: null, orden: { clave: 'fecha', dir: 'desc' } };

        boton.addEventListener('click', function () {
            var activa = !vistaPlana.activa;
            aplicarModoVista(page, boton, activa);
            try {
                window.localStorage.setItem(claveAlmacenamientoVista(boton), activa ? '1' : '0');
            } catch (e) {
                // Solo es una comodidad (sin storage la proxima busqueda vuelve a la vista agrupada); no se loguea.
            }
        });

        // "Buscar" y el cambio de sucursal recargan la pagina: se recuerda el modo elegido para no volver siempre a las tarjetas.
        var guardado = '0';
        try {
            guardado = window.localStorage.getItem(claveAlmacenamientoVista(boton)) || '0';
        } catch (e) {
            // localStorage no disponible (modo privado, datos bloqueados): arranca en la vista agrupada.
        }
        if (guardado === '1') aplicarModoVista(page, boton, true);
    }

    document.addEventListener('DOMContentLoaded', function () {
        var page = document.querySelector('[data-lineas-agrupadas-page]');
        if (!page) return;

        bindFiltrosPendientes(page);
        syncDetalles(page);

        Array.prototype.forEach.call(page.querySelectorAll('.js-filtro-vivo'), function (input) {
            input.addEventListener(input.type === 'checkbox' ? 'change' : 'input', function () {
                filtrar(page);
            });
        });

        var toggle = page.querySelector('.js-toggle-detalles-general');
        if (toggle) {
            toggle.addEventListener('change', function () {
                syncDetalles(page);
            });
        }

        filtrar(page);
        iniciarVistaPlana(page);
    });
})();
