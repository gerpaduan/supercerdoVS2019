// Existencia por sucursales en MODO MOVIMIENTO: la pantalla funciona como panel de stock de la sucursal DESTINO de Movimientos/Editar (dentro
// de un iframe de esa pantalla o en una ventana independiente). Lo que agrega a existencia-sucursales.js:
//  - Columna "Sugerido a enviar" = consumo del periodo (ventas + uso en elaborados, desde / hasta) - stock del destino, con la MISMA regla de
//    la sugerencia de "Ingresar pedido" (ExistenciaPedidoCalculo.calcularSugerencia, modo "consumo"). Se ve, no se carga sola.
//  - Mensajes con el movimiento por un BroadcastChannel (mismo origen): doble click en una fila (o click en el sugerido) elige el producto en el
//    movimiento; el movimiento manda sus lineas (se marcan "+N en mov.") y el cambio de destino (la pantalla se recarga con la nueva sucursal).
//  - Recalcula las sugerencias en cada actualizacion del stock (automatica cada 15 min o forzada) y avisa cuantas cambiaron, sin tocar el movimiento.
//  - Periodo APLICADO vs escrito: los atajos (1, 2, 3, 7, 15 d) se aplican al instante; una fecha escrita a mano queda PENDIENTE (aviso ambar,
//    sugeridos atenuados y sin click) hasta "Actualizar sugerencias", Enter en la fecha o el ⟳ del movimiento. Asi recorrer fechas dia a dia no
//    dispara una consulta por cada cambio. La actualizacion automatica usa siempre el periodo aplicado.
// Inerte fuera del modo movimiento. Depende de existencia-sucursales.js (window.ExistenciaPantalla) y de existencia-pedido.js (el calculo).
// Reglas y porques: docs/DECISIONS.md (2026-10-20, "Stock del destino al lado del movimiento").
(function ($, window) {
    'use strict';

    var cfg = window.ExistenciaConfig || {};
    var pantalla = window.ExistenciaPantalla;
    var calculo = window.ExistenciaPedidoCalculo;
    if (!$ || !cfg.modoMovimiento || !pantalla || !calculo || !document.getElementById('existenciaPage')) return;

    var MS_POR_DIA = 24 * 60 * 60 * 1000;
    var PREFIJO_CANAL = 'carnisys-mov-stock-';

    var $page = $('#existenciaPage');
    var $form = $('#formExistenciaSucursales');
    var $contenedor = $('#contenedorTablaExistenciaSucursales');
    var $leyenda = $('#lblLeyendaSugeridoMov');
    var $recalculado = $('#lblRecalculadoMov');
    var $bloque = $('#bloqueMovExistencia');
    var $resumen = $('#lblResumenMov');
    var $btnColapsar = $('#btnColapsarBloqueMov');
    var $avisoPendiente = $('#avisoPendienteMov');
    var $lblPendiente = $('#lblPendienteMov');
    var $btnAplicar = $('#btnAplicarPeriodoMov');

    var consumo = {};              // idCorte -> { ventas, elaboracion } del destino, para el periodo cargado
    var consumoEstado = 'sin';     // sin | cargando | ok | error | nodisponible
    var consumoClave = '';         // 'desde|hasta' del consumo cargado (para no pedirlo de nuevo si no cambio)
    var consumoDesde = '', consumoHasta = '';
    var mensajeConsumo = '';
    var secConsumo = 0;
    var aplicado = null;           // { desde, hasta } con el que se calculan las sugerencias (null hasta la primera carga del stock)
    var aplicarAlCargar = false;   // el proximo stock que llegue viene del ⟳ del movimiento: aplica tambien el periodo escrito
    var sugActual = {};            // idCorte -> sugerido de la ultima pasada
    var sugBase = null;            // sugerido ANTES de la actualizacion en curso (null = no se compara: primera carga o cambio de periodo a mano)
    var ultimaCuenta = { hora: '', cambiaron: 0 };
    var enMov = {};                // codigo -> { u: unidades, k: kilos } que ya lleva el movimiento
    var canal = null;

    var fmtCant = new Intl.NumberFormat('es-AR', { maximumFractionDigits: 3, useGrouping: false });

    // ---------- Utilidades ----------

    function idSucursal() {
        return cfg.idSucursalMovimiento || parseInt($contenedor.find('th.ex-grupo[data-id-sucursal]').first().attr('data-id-sucursal'), 10) || 0;
    }

    function fechaCorta(iso) { // 'aaaa-mm-dd' -> 'dd/mm'
        var m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(iso || '');
        return m ? m[3] + '/' + m[2] : '';
    }

    function aFecha(iso) {
        var m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(iso || '');
        return m ? new Date(parseInt(m[1], 10), parseInt(m[2], 10) - 1, parseInt(m[3], 10)) : null;
    }

    function hoyIso() {
        var d = new Date();
        return d.getFullYear() + '-' + ('0' + (d.getMonth() + 1)).slice(-2) + '-' + ('0' + d.getDate()).slice(-2);
    }

    function horaCorta() {
        var d = new Date();
        return ('0' + d.getHours()).slice(-2) + ':' + ('0' + d.getMinutes()).slice(-2);
    }

    // Un periodo cerrado (fecha hasta anterior a hoy) no cambia con el tiempo: al actualizar solo se vuelve a leer el stock.
    function periodoCerrado(hasta) {
        return !!hasta && hasta < hoyIso();
    }

    // Periodo escrito en los controles. Una fecha hasta de hoy (o futura) equivale a no tenerla: "Fecha hasta" arranca en hoy al agregarla y
    // no debe contar como un cambio pendiente.
    function periodoEscrito() {
        var hasta = pantalla.periodoHasta();
        return { desde: pantalla.periodoDesde(), hasta: hasta && hasta < hoyIso() ? hasta : '' };
    }

    function claveDe(p) {
        return p.desde + '|' + p.hasta;
    }

    // Hay fechas escritas que todavia no se aplicaron a las sugerencias.
    function hayPendiente() {
        return !!aplicado && claveDe(periodoEscrito()) !== claveDe(aplicado);
    }

    // "13/10 al hoy (7 d)" para las leyendas.
    function textoPeriodo(desde, hasta) {
        var d = aFecha(desde), h = hasta ? aFecha(hasta) : new Date();
        var dias = d && h ? Math.round((new Date(h.getFullYear(), h.getMonth(), h.getDate()).getTime() - d.getTime()) / MS_POR_DIA) : null;
        return {
            corto: fechaCorta(desde) + ' al ' + (hasta ? fechaCorta(hasta) : 'hoy') + (dias !== null ? ' (' + dias + ' d)' : ''),
            largo: fechaCorta(desde) + ' al ' + (hasta ? fechaCorta(hasta) : 'día de hoy') + (dias !== null ? ' (' + dias + (dias === 1 ? ' día' : ' días') + ')' : '')
        };
    }

    // ---------- Mensajes con el movimiento ----------

    function enviar(mensaje) {
        if (canal) canal.postMessage(mensaje);
    }

    // Lineas del movimiento: { "123": { u: 2, k: 6.5 } }. Se sanea todo (input de otra pantalla): solo claves numericas y numeros finitos.
    function sanearLineas(items) {
        var limpio = {};
        if (!items || typeof items !== 'object') return limpio;
        Object.keys(items).forEach(function (cod) {
            var it = items[cod];
            if (!/^\d{1,18}$/.test(cod) || !it || typeof it !== 'object') return;
            var u = Number(it.u), k = Number(it.k);
            limpio[cod] = { u: isFinite(u) && u > 0 ? u : 0, k: isFinite(k) && k > 0 ? k : 0 };
        });
        return limpio;
    }

    function alRecibir(ev) {
        var m = ev && ev.data;
        if (!m || typeof m.tipo !== 'string') return;

        if (m.tipo === 'lineas') {
            enMov = sanearLineas(m.items);
            marcarEnMov();
        } else if (m.tipo === 'destino') {
            // Cambio de sucursal destino en el movimiento: la pantalla se recarga con esa sucursal (un destino invalido o igual al actual se ignora).
            var id = parseInt(m.idSucursal, 10);
            if (id > 0 && id !== cfg.idSucursalMovimiento) {
                window.location.replace(cfg.urlMovimiento + '?popup=true&modo=movimiento&idSucursal=' + id
                    + '&canal=' + encodeURIComponent(cfg.canalMovimiento || ''));
            }
        } else if (m.tipo === 'actualizar') {
            forzarActualizacion();
        }
    }

    if (typeof window.BroadcastChannel !== 'undefined' && cfg.canalMovimiento) {
        canal = new window.BroadcastChannel(PREFIJO_CANAL + cfg.canalMovimiento);
        canal.onmessage = alRecibir;
    }

    // ---------- Consumo del periodo y sugerido ----------

    function renderLeyenda() {
        if (consumoEstado === 'cargando' || consumoEstado === 'sin') {
            $leyenda.text('Calculando las sugerencias…');
            $resumen.text('Sugerido: calculando…');
        } else if (consumoEstado === 'nodisponible') {
            $leyenda.text('El sugerido a enviar solo está disponible cuando el sistema usa Postgres.');
            $resumen.text('Sugerido no disponible');
        } else if (consumoEstado === 'error') {
            $leyenda.text('No se pudo calcular el consumo del período' + (mensajeConsumo ? ' (' + mensajeConsumo + ')' : '') + ': no hay sugerido.');
            $resumen.text('Sugerido: sin datos');
        } else {
            // Resumen de una linea (bloque compacto) y explicacion completa: periodo del consumo cargado, en dias.
            var texto = textoPeriodo(consumoDesde, consumoHasta);
            $resumen.text('Sugerido: ' + texto.corto);
            $leyenda.text('Sugerido a enviar = ventas + uso en elaborados del ' + texto.largo + ', menos el stock actual de este destino.');
        }
        $recalculado.text(ultimaCuenta.hora
            ? 'Recalculado ' + ultimaCuenta.hora + ' · ' + (ultimaCuenta.cambiaron > 0 ? ultimaCuenta.cambiaron + (ultimaCuenta.cambiaron === 1 ? ' cambió' : ' cambiaron') : 'sin cambios')
            : '');
        renderPendiente();
    }

    // Aviso de fechas sin aplicar: tira ambar con "Actualizar sugerencias" (se ve aun con el bloque compacto) y sugeridos atenuados y sin click,
    // para no cargar al movimiento una cantidad calculada con el periodo anterior.
    function renderPendiente() {
        var pendiente = hayPendiente();
        $bloque.toggleClass('ex-mov-pendiente', pendiente);
        $page.toggleClass('ex-sug-pendiente', pendiente);
        $avisoPendiente.toggleClass('d-none', !pendiente);
        if (!pendiente) return;
        var escrito = periodoEscrito();
        $lblPendiente.text(escrito.desde
            ? 'Período cambiado a ' + textoPeriodo(escrito.desde, escrito.hasta).corto + ': sugerencias sin actualizar.'
            : 'Elegí la fecha desde para actualizar las sugerencias.');
        $btnAplicar.prop('disabled', !escrito.desde);
    }

    // Aplica el periodo escrito: las sugerencias se recalculan con ese consumo (sin marcar "cambiaron": no es una actualizacion del stock).
    function aplicarPeriodo() {
        aplicado = periodoEscrito();
        sugBase = null;
        ultimaCuenta = { hora: '', cambiaron: 0 };
        cargarConsumo();
    }

    // Pinta la columna "Sugerido" de cada fila con el stock actual de la tabla y el consumo cargado. Compara con la actualizacion anterior
    // (sugBase) para marcar las celdas que cambiaron.
    function recalcular() {
        var idSuc = idSucursal();
        var nuevo = {}, cambiaron = 0;

        $contenedor.find('tr.existencia-producto-row').each(function () {
            var fila = this;
            var idCorte = parseInt(fila.getAttribute('data-id-corte'), 10);
            var td = fila.querySelector('td.ex-col-sug');
            if (!td) return;
            td.className = td.className.replace(/\bex-sug-cambio\b/g, '').trim();
            td.removeAttribute('title');
            $(td).find('.ex-sug-num, .ex-sug-ok').remove();
            fila.removeAttribute('data-g-' + idSuc);
            if (consumoEstado !== 'ok') return;

            var stock = parseFloat(fila.getAttribute('data-s-' + idSuc)) || 0;
            var c = consumo[idCorte] || { ventas: 0, elaboracion: 0 };
            var r = calculo.calcularSugerencia(stock, 0, c.ventas + c.elaboracion, 'consumo');
            var pesable = fila.getAttribute('data-pes') === '1';
            // Un producto por unidades se envia entero: se redondea hacia arriba (se carga en Cant. Un.).
            var sugerido = pesable ? r.sugerido : Math.ceil(r.sugerido - 1e-9);
            nuevo[idCorte] = sugerido;
            fila.setAttribute('data-g-' + idSuc, String(sugerido));

            var titulo = 'Vendido ' + fmtCant.format(c.ventas) + ' + usado en elaborados ' + fmtCant.format(c.elaboracion) + ' del ' + fechaCorta(consumoDesde)
                + ' al ' + (consumoHasta ? fechaCorta(consumoHasta) : 'día de hoy') + ' = ' + fmtCant.format(r.consumo) + ' − stock '
                + (stock < 0 ? '0 (el stock es negativo: ' + fmtCant.format(stock) + ')' : fmtCant.format(r.stock)) + ' = sugerido ' + fmtCant.format(sugerido);

            if (sugerido > 0) {
                var boton = document.createElement('button');
                boton.type = 'button';
                boton.className = 'ex-sug-num';
                boton.textContent = fmtCant.format(sugerido);
                boton.setAttribute('data-valor', String(sugerido));
                boton.title = titulo + ' · click para cargarlo en el movimiento';
                td.appendChild(boton);
            } else if (r.stockSuficiente) {
                var ok = document.createElement('span');
                ok.className = 'ex-sug-ok';
                ok.textContent = '✔ alcanza';
                ok.title = 'El stock (' + fmtCant.format(r.stock) + ') cubre el consumo del período (' + fmtCant.format(r.consumo) + ')';
                td.appendChild(ok);
            }

            if (sugBase && sugBase[idCorte] !== undefined && sugBase[idCorte] !== sugerido) {
                cambiaron++;
                td.className += ' ex-sug-cambio';
                td.title = 'El sugerido cambió con la última actualización: antes ' + fmtCant.format(sugBase[idCorte]) + ', ahora ' + fmtCant.format(sugerido);
                fila.setAttribute('data-sug-cambio', '1');
            } else {
                fila.removeAttribute('data-sug-cambio');
            }
        });

        sugActual = nuevo;
        if (consumoEstado === 'ok') ultimaCuenta = { hora: horaCorta(), cambiaron: sugBase ? cambiaron : 0 };
        marcarEnMov();
        renderLeyenda();
    }

    // Marca las filas de los productos que el movimiento ya lleva: fondo suave y "+N en mov." junto al sugerido (ambar si ese sugerido
    // cambio con la ultima actualizacion: conviene revisar la cantidad cargada; el movimiento no se toca).
    function marcarEnMov() {
        $contenedor.find('tr.existencia-producto-row').each(function () {
            var fila = this;
            var td = fila.querySelector('td.ex-col-sug');
            if (!td) return;
            $(td).find('.ex-en-mov-tag').remove();
            var info = enMov[String(fila.getAttribute('data-cod'))];
            $(fila).toggleClass('ex-en-mov', !!info);
            if (!info) return;

            var tag = document.createElement('span');
            tag.className = 'ex-en-mov-tag' + (fila.getAttribute('data-sug-cambio') === '1' ? ' ex-revisar' : '');
            tag.textContent = '+' + (info.k > 0 ? fmtCant.format(info.k) + ' kg' : fmtCant.format(info.u) + ' un.') + ' en mov.';
            tag.title = 'Ya está en el movimiento' + (fila.getAttribute('data-sug-cambio') === '1' ? ' · el sugerido cambió: revisá la cantidad' : '');
            td.insertBefore(tag, td.firstChild);
        });
    }

    // Pide el consumo del periodo APLICADO (no del escrito: una fecha a mano sin confirmar no se consulta).
    function cargarConsumo() {
        if (!aplicado) aplicado = periodoEscrito();
        var desde = aplicado.desde;
        var hasta = aplicado.hasta;
        var urlConsumo = (window.ExistenciaPedidoConfig || {}).consumoUrl;
        var seq = ++secConsumo;

        if (!desde || !urlConsumo) {
            consumoEstado = 'error';
            mensajeConsumo = 'elegí la fecha de «Período desde»';
            renderLeyenda();
            recalcular();
            return;
        }

        consumoEstado = 'cargando';
        renderLeyenda();
        var datos = { desde: desde, idsSucursales: idSucursal() };
        if (hasta) datos.hasta = hasta;
        $.ajax({ url: urlConsumo, type: 'GET', data: datos }).done(function (resp) {
            if (seq !== secConsumo) return; // llego una respuesta vieja (se cambio el periodo mientras tanto)
            if (resp && resp.ok) {
                consumo = {};
                (resp.items || []).forEach(function (it) {
                    consumo[it.idCorte] = { ventas: it.ventas || 0, elaboracion: it.elaboracion || 0 };
                });
                consumoDesde = resp.desde || desde;
                consumoHasta = resp.hasta || '';
                consumoClave = desde + '|' + hasta;
                consumoEstado = 'ok';
                mensajeConsumo = '';
            } else {
                consumo = {};
                consumoClave = '';
                consumoEstado = resp && resp.noDisponible ? 'nodisponible' : 'error';
                mensajeConsumo = (resp && resp.mensaje) || '';
            }
        }).fail(function () {
            if (seq !== secConsumo) return;
            consumo = {};
            consumoClave = '';
            consumoEstado = 'error';
            mensajeConsumo = 'sin respuesta del servidor';
        }).always(function () {
            if (seq !== secConsumo) return;
            recalcular();
        });
    }

    // Actualizar ahora (⟳ del movimiento): busqueda manual (el guard de carga nunca la frena), aplica las fechas escritas y pide el consumo de nuevo.
    function forzarActualizacion() {
        aplicarAlCargar = true;
        // Con un periodo cerrado el consumo no puede haber cambiado: solo se relee el stock.
        if (!periodoCerrado(periodoEscrito().hasta)) consumoClave = '';
        $form.trigger('submit');
    }

    // ---------- Eventos de la pantalla ----------

    // El stock se actualizo (primera carga, Buscar, automatica o forzada): se recalcula ya con el stock nuevo, con el periodo APLICADO (una fecha
    // escrita y sin confirmar no se aplica sola). El consumo se vuelve a pedir salvo que sea el mismo periodo y este CERRADO (no cambia).
    $page.on('existencia:tablaCargada', function () {
        var cambioPeriodo = false;
        if (!aplicado) {
            aplicado = periodoEscrito();
        } else if (aplicarAlCargar && hayPendiente()) {
            aplicado = periodoEscrito();
            cambioPeriodo = true;
        }
        aplicarAlCargar = false;
        var igual = consumoEstado === 'ok' && consumoClave === claveDe(aplicado);
        // Se compara contra lo que habia antes de esta actualizacion; en la primera carga o con otro periodo no hay nada con que comparar.
        sugBase = !cambioPeriodo && Object.keys(sugActual).length ? sugActual : null;
        if (igual && periodoCerrado(aplicado.hasta)) {
            recalcular();
        } else {
            recalcular(); // con el stock nuevo y el consumo anterior, para que la columna no quede vacia mientras llega el nuevo
            cargarConsumo();
        }
    });

    // Cambio de fechas: un atajo se aplica al instante (es un solo click); una fecha a mano queda pendiente hasta confirmarla.
    $page.on('existencia:periodoCambiado', function (e, info) {
        if (info && info.origen === 'atajo') {
            aplicarPeriodo();
        } else {
            renderPendiente();
        }
    });

    $btnAplicar.on('click', function () {
        if (periodoEscrito().desde) aplicarPeriodo();
    });

    // Enter en una de las fechas confirma el periodo (como "Actualizar sugerencias").
    $('#filtroPeriodoDesdeExistencia, #filtroPeriodoHastaExistencia').on('keydown', function (e) {
        if (e.key !== 'Enter') return;
        e.preventDefault();
        if (hayPendiente() && periodoEscrito().desde) aplicarPeriodo();
    });

    // Doble click en una fila: elige el producto en el movimiento (carga el codigo y deja el foco en la cantidad).
    $contenedor.on('dblclick', '.existencia-producto-row', function (e) {
        if ($(e.target).closest('.btn-toggle-existencia, .ex-btn-pto, .ex-sug-num').length) return;
        var codigo = parseInt(this.getAttribute('data-cod'), 10);
        if (codigo > 0) enviar({ tipo: 'elegir', codigo: codigo });
    });

    // Click en el numero sugerido: carga el codigo Y esa cantidad (kilos si el producto es por peso, unidades si no).
    $contenedor.on('click', '.ex-sug-num', function (e) {
        e.stopPropagation();
        // Con fechas sin aplicar el numero es del periodo anterior: no se carga (el aviso ambar dice como actualizarlo).
        if (hayPendiente()) return;
        var fila = $(this).closest('tr.existencia-producto-row')[0];
        var codigo = fila ? parseInt(fila.getAttribute('data-cod'), 10) : 0;
        var cantidad = parseFloat(this.getAttribute('data-valor'));
        if (codigo > 0 && isFinite(cantidad) && cantidad > 0) {
            enviar({ tipo: 'elegir', codigo: codigo, cantidad: cantidad, pesable: fila.getAttribute('data-pes') === '1' });
        }
    });

    // El bloque del periodo y la leyenda arrancan compactos (una linea) SIEMPRE: el boton "Cambiar período" los abre y "Ocultar" los cierra.
    function alternarBloque(colapsar) {
        $bloque.toggleClass('ex-mov-colapsado', colapsar);
        $btnColapsar.attr('aria-expanded', colapsar ? 'false' : 'true')
            .attr('title', colapsar ? 'Mostrar el período y la explicación del sugerido' : 'Ocultar el período y la explicación del sugerido')
            .find('i').toggleClass('fa-chevron-down', colapsar).toggleClass('fa-chevron-up', !colapsar);
        $btnColapsar.find('.ex-mov-colapsar-txt').text(colapsar ? 'Cambiar período' : 'Ocultar');
        pantalla.ajustarAlto();
    }

    $btnColapsar.on('click', function (e) {
        e.stopPropagation();
        alternarBloque(!$bloque.hasClass('ex-mov-colapsado'));
    });

    // Compacto, toda la tira abre el bloque (no solo el boton). Abierto no: ahi se esta usando las fechas.
    $bloque.on('click', function (e) {
        if (!$bloque.hasClass('ex-mov-colapsado') || $(e.target).closest('button, input, a').length) return;
        alternarBloque(false);
    });

    // ---------- Arranque ----------

    // El periodo (desde / hasta) sale del panel de Filtros y queda siempre a la vista junto a la leyenda (los controles se mueven, no se copian:
    // siguen siendo los mismos con sus eventos).
    $('#hostPeriodoMov').append($('.ex-ctl-periodo').first());
    renderLeyenda();
    // Le avisa al movimiento que ya puede mandarle sus lineas.
    enviar({ tipo: 'listo' });
})(window.jQuery, window);
