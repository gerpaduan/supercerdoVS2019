// Existencia por sucursales: toda la logica del navegador de la pantalla (Views/Stock/ExistenciaPorSucursales.cshtml).
//  - Busqueda por AJAX y autoactualizacion (arranca encendida; se posterga si el servidor esta ocupado; al agotar las vueltas
//    pregunta si seguir).
//  - Orden por encabezado, filtros de la barra (texto, Estado) y filtros avanzados de seleccion multiple.
//  - Diseno compacto recordado por navegador, columna Estado, edicion del punto de stock sin salir de la pantalla y exportacion.
//  - "Ingresar pedido" (Scripts/app/existencia-pedido.js) se engancha por window.ExistenciaPantalla y por eventos jQuery sobre
//    #existenciaPage: existencia:tablaCargada, existencia:filtrado, existencia:puntoCambiado y existencia:periodoCambiado.
// Lee su configuracion de window.ExistenciaConfig (la deja la vista). La tabla la dibuja _TablaExistenciaPorSucursales.cshtml
// y se reemplaza entera en cada Buscar: por eso los handlers de la tabla van delegados sobre #contenedorTablaExistenciaSucursales.
// Reglas y porques: docs/DECISIONS.md 2026-10-19 y docs/11-manual-de-uso/pantallas/Stock.ExistenciaPorSucursales.md.
(function ($) {
    'use strict';

    var cfg = window.ExistenciaConfig || {};

    var $page = $('#existenciaPage');
    var $form = $('#formExistenciaSucursales');
    var $contenedor = $('#contenedorTablaExistenciaSucursales');
    var $fechaHasta = $('#filtroFechaHastaExistencia');
    var $texto = $('#filtroTextoExistencia');
    var $claseEstado = $('#filtroClaseEstadoExistencia');
    var $btnBuscar = $('#btnBuscarExistencia');
    var $btnBuscarTexto = $btnBuscar.find('.btn-buscar-texto');
    var $panelAvanzados = $('#panelFiltrosAvanzadosExistencia');
    var $btnAvanzados = $('#btnFiltrosAvanzadosExistencia');
    var $badgeAvanzados = $('#badgeFiltrosExistencia');
    var $periodo = $('#filtroPeriodoDesdeExistencia');
    var $chkMovimiento = $('#chkConMovimientoExistencia');
    var $movMinimo = $('#filtroMovimientoMinimoExistencia');
    var $movMedida = $('#filtroMovimientoMedidaExistencia');
    var $switchAuto = $('#switchAutoActualizarExistencia');
    var $switchEstado = $('#switchEstadoStockExistencia');
    var $switchExpandir = $('#switchExpandirExistencia');
    var $switchCompacto = $('#switchCompactoExistencia');
    var $lblUltima = $('#lblUltimaActualizacionExistencia');
    var $lblPendiente = $('#lblAvisoPendienteExistencia');
    var $lblEstado = $('#lblEstadoConsultaExistencia');
    var $lblAuto = $('#lblAutoActualizarExistencia');
    var $lblAutoChip = $('#lblAutoChipExistencia');
    var $lblFechaMinima = $('#lblFechaMinimaExistencia');
    var $lblPie = $('#lblPieExistencia');
    var $chkSucursales = $('.chk-sucursal-existencia');
    var $chkTodasSucursales = $('#chkSucursalTodasExistencia');
    var $lblSucursales = $('#lblSucursalesExistencia');

    var DIAS_PERIODO_DEFECTO = 30;
    var CLAVE_DIAS_PERIODO = 'existenciaPeriodoDias';
    var DIAS_PERIODO_ATAJOS = [7, 15, 30, 60, 90, 180, 365];

    var consultaAplicada = null;       // foto de los filtros de la ultima busqueda (para el aviso "hay filtros sin aplicar")
    var ultimaConsulta = null;         // querystring de la ultima busqueda exitosa (lo usa la exportacion)
    var orden = { clave: 'cod', dir: 1 };
    var sincronizandoScrollDetalle = false;
    var ultimoToqueDetalle = { target: '', time: 0 };

    // Autoactualizacion
    var intervaloAutoMinutos = cfg.intervaloAutoMinutos || 15;
    var maxAutoActualizaciones = cfg.maxAutoActualizaciones || 6;
    var autoActualizacionesEjecutadas = 0;
    var autoActualizacionTimer = null;
    var autoActualizacionCountdownTimer = null;
    var autoActualizacionProximaEjecucion = null;
    var inicialTimer = null;
    var disparoAutomatico = false;     // la proxima busqueda es automatica (manda auto=true al servidor)
    var disparoCuentaVuelta = false;   // ... y ademas cuenta como una de las vueltas del ciclo
    var editorPuntoAbierto = false;    // mientras se edita un punto de stock la autoactualizacion espera
    var pedidoActivo = false;          // "Ingresar pedido" encendido: la autoactualizacion se pausa (reemplazar la tabla mataria el foco)

    // ---------- Utilidades ----------

    var fmt3 = new Intl.NumberFormat('es-AR', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
    var fmt0 = new Intl.NumberFormat('es-AR', { maximumFractionDigits: 0 });

    function leerLS(clave, porDefecto) {
        try {
            var v = window.localStorage.getItem(clave);
            return v === null ? porDefecto : v;
        } catch (e) {
            return porDefecto; // almacenamiento bloqueado (ventana privada, politicas): la pantalla funciona igual
        }
    }

    function guardarLS(clave, valor) {
        try { window.localStorage.setItem(clave, valor); } catch (e) { /* sin almacenamiento: no se recuerda, no es un error */ }
    }

    function dos(n) { return ('0' + n).slice(-2); }

    function isoFecha(d) { return d.getFullYear() + '-' + dos(d.getMonth() + 1) + '-' + dos(d.getDate()); }

    function horaCorta(d) { return dos(d.getHours()) + ':' + dos(d.getMinutes()); }

    function ahoraLocalInput() {
        var ahora = new Date();
        var local = new Date(ahora.getTime() - (ahora.getTimezoneOffset() * 60000));
        return local.toISOString().slice(0, 16);
    }

    function avisar(titulo, texto, icono) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire(titulo, texto, icono || 'info');
        } else {
            window.alert(titulo + ': ' + texto);
        }
    }

    // ---------- Estado de una celda (espejo de Negocio.ExistenciaEstado: si cambia la regla, cambiarla en los dos) ----------

    function calcularEstado(stock, punto) {
        var est = { stock: stock, punto: punto, faltante: 0, rango: 0, codigo: '' };
        if (stock < 0) {
            est.rango = 0; est.codigo = 'NEGATIVO';
        } else if (punto > 0 && stock <= punto) {
            est.rango = 1;
            if (stock === punto) { est.codigo = 'EN_PUNTO'; } else { est.codigo = 'FALTA'; est.faltante = punto - stock; }
        } else if (punto > 0) {
            est.rango = 2; est.codigo = 'OK';
        } else {
            est.rango = 3; est.codigo = stock === 0 ? 'SIN_STOCK' : 'SIN_PUNTO';
        }
        est.clave = est.rango * 1000000 + (stock - punto);
        est.claseFiltro = est.rango <= 1 ? 'falta' : (est.rango === 2 ? 'bien' : '');
        return est;
    }

    function textoEstado(est) {
        switch (est.codigo) {
            case 'NEGATIVO': return 'Negativo';
            case 'FALTA': return 'Falta ' + fmt3.format(est.faltante);
            case 'EN_PUNTO': return 'En el punto';
            case 'OK': return 'OK';
            case 'SIN_STOCK': return 'Sin stock';
            default: return 'Sin punto';
        }
    }

    function claseEstado(est) {
        switch (est.codigo) {
            case 'NEGATIVO': return 'ex-est-neg';
            case 'FALTA': return 'ex-est-falta';
            case 'EN_PUNTO': return 'ex-est-punto';
            case 'OK': return 'ex-est-ok';
            default: return 'ex-est-sin';
        }
    }

    function claseStock(est) {
        switch (est.codigo) {
            case 'NEGATIVO': return 'text-danger font-weight-bold';
            case 'FALTA':
            case 'EN_PUNTO': return 'text-warning font-weight-bold';
            case 'SIN_STOCK': return 'text-muted';
            default: return '';
        }
    }

    // ---------- Sucursales ----------

    function obtenerIdsSucursalesTildadas() {
        return $chkSucursales.filter(':checked').map(function () { return this.value; }).get();
    }

    // Texto del boton: "Todas", el nombre si hay una sola, o "N sucursales".
    function actualizarTextoSucursales() {
        var tildadas = $chkSucursales.filter(':checked');
        var texto;
        if (tildadas.length === $chkSucursales.length) texto = 'Todas';
        else if (tildadas.length === 1) texto = tildadas.first().data('nombre');
        else texto = tildadas.length + ' sucursales';
        $lblSucursales.text(texto);
        $('#btnSucursalesExistencia').attr('title', tildadas.map(function () { return $(this).data('nombre'); }).get().join(', '));
    }

    // Mantiene el checkbox "Todas" coherente con los individuales y evita dejar 0 tildadas.
    function sincronizarSucursales(origen) {
        if (origen === 'todas') {
            $chkSucursales.prop('checked', true);
            $chkTodasSucursales.prop('checked', true);
        } else if ($chkSucursales.filter(':checked').length === 0) {
            $(origen).prop('checked', true); // intento de destildar la ultima: se revierte
        }
        $chkTodasSucursales.prop('checked', $chkSucursales.filter(':checked').length === $chkSucursales.length);
        actualizarTextoSucursales();
    }

    // El calculo parte del ultimo cierre de stock de cada sucursal: no tiene sentido pedir una fecha anterior (daria un
    // resultado incorrecto, ver docs/DECISIONS.md). Informa "Calculo desde ..." segun las sucursales tildadas.
    function actualizarFechaMinimaExistencia() {
        $.ajax({
            url: cfg.fechaMinimaUrl,
            type: 'GET',
            traditional: true, // idsSucursales=1&idsSucursales=3 (sin corchetes) para int[]
            data: { idsSucursales: obtenerIdsSucursalesTildadas() }
        }).done(function (resp) {
            if (!resp || !resp.ok) return;
            $fechaHasta.attr('min', resp.fechaMinima || '');
            if (resp.fechaMinima) {
                var fechaCorta = resp.fechaMinima.split('T')[0].split('-').reverse().join('/');
                var detalle = (resp.detalle || []).map(function (d) { return d.Sucursal + ': ' + d.fecha; }).join(' · ');
                $lblFechaMinima.text('Cálculo desde ' + fechaCorta).attr('title', detalle);
            } else {
                $lblFechaMinima.text('Sin cierre de stock registrado').attr('title', '');
            }
        });
    }

    // ---------- Filtros avanzados (Tipos / Proveedores / Marcas, periodo y movimiento) ----------

    function valoresMarcados(clave) {
        return $('.ex-ms[data-ms="' + clave + '"] .ex-ms-chk:checked').map(function () { return this.value; }).get();
    }

    function actualizarResumenMs($ms) {
        var $marcados = $ms.find('.ex-ms-chk:checked');
        var n = $marcados.length;
        var texto = n === 0 ? 'Todos' : (n === 1 ? $marcados.first().data('nombre') : n + ' ' + $ms.data('plural'));
        $ms.find('.ex-ms-resumen').text(texto).attr('title', $marcados.map(function () { return $(this).data('nombre'); }).get().join(', '));
        $ms.find('.ex-ms-todos').prop('checked', n === 0);
    }

    // Cantidad de grupos de filtros avanzados activos: se muestra en el boton "Filtros" para que no se olviden con el panel cerrado.
    function contarFiltrosAvanzadosActivos() {
        var n = 0;
        if (valoresMarcados('tipos').length) n++;
        if (valoresMarcados('proveedores').length) n++;
        if (valoresMarcados('marcas').length) n++;
        if ($chkMovimiento.is(':checked')) n++;
        return n;
    }

    function actualizarBadgeAvanzados() {
        var n = contarFiltrosAvanzadosActivos();
        $badgeAvanzados.text(n).toggleClass('d-none', n === 0);
        $btnAvanzados.toggleClass('btn-outline-secondary', n === 0).toggleClass('btn-outline-primary', n > 0);
    }

    function marcarAtajoPeriodo(dias) {
        $('.ex-periodo-dias').removeClass('active').filter('[data-dias="' + dias + '"]').addClass('active');
    }

    function fechaDesdeDias(dias) {
        var d = new Date();
        d.setHours(0, 0, 0, 0);
        d.setDate(d.getDate() - dias);
        return isoFecha(d);
    }

    function aplicarPeriodoDias(dias) {
        $periodo.val(fechaDesdeDias(dias));
        guardarLS(CLAVE_DIAS_PERIODO, String(dias));
        marcarAtajoPeriodo(dias);
        actualizarPendiente();
        $page.trigger('existencia:periodoCambiado');
    }

    function iniciarPeriodo() {
        var dias = parseInt(leerLS(CLAVE_DIAS_PERIODO, String(DIAS_PERIODO_DEFECTO)), 10);
        if (DIAS_PERIODO_ATAJOS.indexOf(dias) < 0) dias = DIAS_PERIODO_DEFECTO;
        $periodo.attr('max', isoFecha(new Date()));
        $periodo.val(fechaDesdeDias(dias));
        marcarAtajoPeriodo(dias);
    }

    function actualizarControlesMovimiento() {
        var activo = $chkMovimiento.is(':checked') && !$chkMovimiento.prop('disabled');
        $movMinimo.prop('disabled', !activo);
        $movMedida.prop('disabled', !activo);
    }

    // ---------- Consulta aplicada / pendiente ----------

    function obtenerEstadoConsulta() {
        var conMovimiento = $chkMovimiento.is(':checked');
        return {
            texto: ($texto.val() || '').trim(),
            idsSucursales: obtenerIdsSucursalesTildadas(),
            tipos: valoresMarcados('tipos'),
            idsProveedores: valoresMarcados('proveedores'),
            idsMarcas: valoresMarcados('marcas'),
            conMovimiento: conMovimiento,
            movimientoMinimo: conMovimiento ? $movMinimo.val() : '',
            movimientoMedida: conMovimiento ? $movMedida.val() : '',
            periodoDesde: conMovimiento ? $periodo.val() : ''
        };
    }

    function actualizarPendiente() {
        if (!consultaAplicada) {
            $lblPendiente.addClass('d-none');
            return;
        }
        $lblPendiente.toggleClass('d-none', JSON.stringify(consultaAplicada) === JSON.stringify(obtenerEstadoConsulta()));
    }

    // ---------- Orden ----------

    // Claves: cod, nom, st / et (total), s<id> / e<id> / p<id> (stock, estado, A pedir de una sucursal), tp / imp (Total pedido e
    // Importe estimado: los mantiene existencia-pedido.js en data-tp / data-imp / data-p-<id> de cada fila).
    function valorOrden(fila, clave) {
        if (clave === 'cod') return Number(fila.getAttribute('data-cod')) || 0;
        if (clave === 'nom') return (fila.getAttribute('data-nom') || '').toLowerCase();
        var atributo = clave === 'st' ? 'data-s-t'
            : clave === 'et' ? 'data-e-t'
                : clave === 'tp' ? 'data-tp'
                    : clave === 'imp' ? 'data-imp'
                        : (clave.charAt(0) === 's' ? 'data-s-' : clave.charAt(0) === 'p' ? 'data-p-' : 'data-e-') + clave.slice(1);
        return Number(fila.getAttribute(atributo)) || 0;
    }

    // Primera direccion al tocar un encabezado: stock y cantidades pedidas, primero el mayor; codigo, producto y estado, primero el menor
    // (estado = primero los faltantes).
    function direccionInicialOrden(clave) {
        if (clave === 'tp' || clave === 'imp') return -1;
        var c = clave.charAt(0);
        return ((c === 's' || c === 'p') && clave !== 'nom') ? -1 : 1;
    }

    // Reordena las filas de producto llevandose juntas su fila de detalle. Estable: ante empate queda el orden del servidor (codigo).
    function ordenarTabla() {
        var tbody = $contenedor.find('tbody')[0];
        if (!tbody) return;

        var filas = [].slice.call(tbody.querySelectorAll('tr.existencia-producto-row'));
        var esTexto = orden.clave === 'nom';
        var coll = new Intl.Collator('es', { sensitivity: 'base', numeric: true });
        var pares = filas.map(function (fila, i) {
            return { fila: fila, detalle: fila.nextElementSibling, clave: valorOrden(fila, orden.clave), i: i };
        });

        pares.sort(function (a, b) {
            var c = esTexto ? coll.compare(a.clave, b.clave) : (a.clave < b.clave ? -1 : (a.clave > b.clave ? 1 : 0));
            return c === 0 ? a.i - b.i : c * orden.dir;
        });

        var fragmento = document.createDocumentFragment();
        pares.forEach(function (par) {
            fragmento.appendChild(par.fila);
            if (par.detalle) fragmento.appendChild(par.detalle);
        });
        tbody.appendChild(fragmento);

        $contenedor.find('th.ex-sortable').removeClass('ex-orden-asc ex-orden-desc')
            .filter('[data-sort-key="' + orden.clave + '"]').addClass(orden.dir > 0 ? 'ex-orden-asc' : 'ex-orden-desc');
    }

    function nombreOrden() {
        var titulo = $contenedor.find('th.ex-sortable[data-sort-key="' + orden.clave + '"]').first().attr('title') || '';
        return titulo.replace(/^Ordenar por /, '') || orden.clave;
    }

    // ---------- Filtros visuales (instantaneos, sobre lo ya cargado) ----------

    function aplicarExpandidoGlobal() {
        var expandir = $switchExpandir.is(':checked');
        $contenedor.find('.existencia-detalle-row').each(function () {
            var $fila = $(this);
            var visiblePadre = !$fila.prev('.existencia-producto-row').hasClass('d-none');
            $fila.toggleClass('d-none', !(expandir && visiblePadre));
        });
        $contenedor.find('.btn-toggle-existencia i').toggleClass('fa-minus-square', expandir).toggleClass('fa-plus-square', !expandir);
    }

    function aplicarFiltrosVisuales() {
        var texto = ($texto.val() || '').trim().toUpperCase();
        var clase = $claseEstado.val() || 'todos';

        $contenedor.find('.existencia-producto-row').each(function () {
            var $row = $(this);
            var textoRow = ($row.attr('data-texto') || '').toUpperCase();
            var clases = ' ' + ($row.attr('data-clases') || '') + ' ';
            var coincideTexto = !texto || textoRow.indexOf(texto) >= 0;
            var coincideEstado = clase === 'todos'
                || (clase === 'sinpto' ? $row.attr('data-sinpto') === '1' : clases.indexOf(' ' + clase + ' ') >= 0);
            var visible = coincideTexto && coincideEstado;
            var target = $row.attr('data-detail-target');

            $row.toggleClass('d-none', !visible);
            if (!visible && target) $(target).addClass('d-none');
        });

        aplicarExpandidoGlobal();
        actualizarPie();
        $page.trigger('existencia:filtrado'); // las filas visibles cambiaron (resumen y "aceptar sugerencias" del pedido)
    }

    // La columna Estado se oculta/muestra con el interruptor; los encabezados de grupo ajustan su colspan (Stock + Estado + A pedir:
    // cada una cuenta solo si esta visible; "A pedir" aparece con "Ingresar pedido" encendido).
    function aplicarEstadoStockExistencia() {
        var mostrar = $switchEstado.is(':checked');
        $contenedor.find('.ex-col-estado').toggleClass('d-none', !mostrar);
        $contenedor.find('th.ex-grupo[data-id-sucursal]').attr('colspan', 1 + (mostrar ? 1 : 0) + (pedidoActivo ? 1 : 0));
        $contenedor.find('th.ex-grupo-total').attr('colspan', mostrar ? 2 : 1);
    }

    function actualizarPie() {
        var total = $contenedor.find('.existencia-producto-row').length;
        if (!total) {
            $lblPie.text('');
            return;
        }
        var visibles = $contenedor.find('.existencia-producto-row:not(.d-none)').length;
        $lblPie.text(visibles + (visibles !== total ? ' de ' + total : '') + ' productos · Orden: ' + nombreOrden() + (orden.dir > 0 ? ' ↑' : ' ↓'));
    }

    // La tabla scrollea por dentro (encabezado y las dos primeras columnas quedan fijos): se le da el alto que queda en la ventana.
    function ajustarAltoTabla() {
        var $tf = $contenedor.find('.tabla-full');
        if (!$tf.length) return;

        var arriba = $tf[0].getBoundingClientRect().top + (window.pageYOffset || 0);
        var abajoPie = $page.hasClass('existencia-compacto') ? ($('.ex-estado-linea').outerHeight(true) || 0) : 0;
        $tf.css('max-height', Math.max(260, window.innerHeight - arriba - abajoPie - 14) + 'px');

        // "Producto" queda fijo justo a la derecha de "Cod.", cuyo ancho depende del codigo mas largo.
        var $tabla = $tf.find('table.tabla-existencia-sucursales');
        var anchoCod = $tabla.find('thead th.ex-col-cod').first().outerWidth();
        if ($tabla.length && anchoCod) $tabla[0].style.setProperty('--ex-cod-w', anchoCod + 'px');

        // La 2.a fila del encabezado se pega justo debajo de la 1.a. Se mide la fila (no una celda: "Cod." y "Producto" ocupan
        // las dos filas con rowspan y su alto es el de las dos juntas).
        var alto1 = $tf.find('thead tr:first-child')[0].offsetHeight || 0;
        $tf.find('thead tr:nth-child(2) th').css('top', alto1 + 'px');
    }

    function vincularScrollDetalles() {
        $contenedor.find('.existencia-detalle-scroll-sync')
            .off('scroll.syncDetalle')
            .on('scroll.syncDetalle', function () {
                if (sincronizandoScrollDetalle) return;
                sincronizandoScrollDetalle = true;
                var scrollLeft = this.scrollLeft;
                $contenedor.find('.existencia-detalle-scroll-sync').not(this).each(function () { this.scrollLeft = scrollLeft; });
                sincronizandoScrollDetalle = false;
            });
    }

    function toggleDetalleExistencia(target) {
        var $detalle = target ? $(target) : $();
        if (!$detalle.length) return;
        var expandido = !$detalle.hasClass('d-none');
        $detalle.toggleClass('d-none', expandido);
        $('.btn-toggle-existencia[data-target="' + target + '"]').find('i')
            .toggleClass('fa-plus-square', expandido).toggleClass('fa-minus-square', !expandido);
    }

    // Todo lo que hay que reaplicar cuando la tabla se reemplaza o cambia algo que la afecta.
    function refrescarVistaTabla() {
        aplicarEstadoStockExistencia();
        ordenarTabla();
        aplicarFiltrosVisuales();
        vincularScrollDetalles();
        ajustarAltoTabla();
        $page.trigger('existencia:tablaCargada'); // el pedido vuelve a poner las cantidades cargadas en la tabla nueva
    }

    // ---------- Compacto ----------

    function iniciarCompacto() {
        $switchCompacto.prop('checked', $page.hasClass('existencia-compacto'));
    }

    function cambiarCompacto() {
        var compacto = $switchCompacto.is(':checked');
        $page.toggleClass('existencia-compacto', compacto);
        guardarLS(cfg.popup ? 'existenciaCompactoPopup' : 'existenciaCompacto', compacto ? '1' : '0');
        ajustarAltoTabla();
    }

    // ---------- Autoactualizacion ----------

    function setBuscando(buscando) {
        $btnBuscar.prop('disabled', buscando);
        $btnBuscarTexto.text(buscando ? 'Buscando...' : 'Buscar');
    }

    function limpiarTimerAutoActualizacion() {
        if (autoActualizacionTimer) { window.clearTimeout(autoActualizacionTimer); autoActualizacionTimer = null; }
        if (autoActualizacionCountdownTimer) { window.clearInterval(autoActualizacionCountdownTimer); autoActualizacionCountdownTimer = null; }
        autoActualizacionProximaEjecucion = null;
    }

    function actualizarTextoAutoActualizacion() {
        if (!$switchAuto.is(':checked')) {
            $lblAuto.removeClass('text-success text-warning').addClass('text-muted')
                .text('Autoactualización apagada. Cuando está encendida, actualiza cada ' + intervaloAutoMinutos + ' minutos, hasta ' + maxAutoActualizaciones + ' veces.');
            $lblAutoChip.text('');
            return;
        }

        // Con un pedido en curso el reloj se detiene: actualizar reemplazaria la tabla y se perderia el foco de lo que se esta tipeando.
        if (pedidoActivo) {
            $lblAuto.removeClass('text-success text-muted').addClass('text-warning')
                .text('Autoactualización en pausa mientras se ingresa un pedido. Se reanuda al apagar «Ingresar pedido».');
            $lblAutoChip.text('pausa');
            return;
        }

        var restantes = Math.max(0, maxAutoActualizaciones - autoActualizacionesEjecutadas);
        if (!autoActualizacionProximaEjecucion) {
            $lblAuto.removeClass('text-muted text-warning').addClass('text-success')
                .text('Autoactualización activa. Restan ' + restantes + ' ejecuciones.');
            $lblAutoChip.text('');
            return;
        }

        var totalSegundos = Math.ceil(Math.max(0, autoActualizacionProximaEjecucion - Date.now()) / 1000);
        var cuenta = Math.floor(totalSegundos / 60) + ':' + dos(totalSegundos % 60);
        $lblAuto.removeClass('text-muted text-warning').addClass('text-success')
            .text('Autoactualización activa. Próxima en ' + cuenta + '. Restan ' + restantes + ' ejecuciones.');
        $lblAutoChip.text(cuenta);
    }

    // Se agotaron las vueltas: se apaga y se pregunta (con un cartel que obliga a elegir) si se quiere seguir actualizando.
    function preguntarSeguirActualizando() {
        limpiarTimerAutoActualizacion();
        autoActualizacionesEjecutadas = 0;
        $switchAuto.prop('checked', false);
        actualizarTextoAutoActualizacion();
        $lblEstado.text('La actualización automática se detuvo. Elegí si querés seguir.');

        if (!(window.Swal && typeof window.Swal.fire === 'function')) {
            return;
        }

        window.Swal.fire({
            icon: 'question',
            title: '¿Seguir actualizando el stock?',
            text: 'Se completaron las ' + maxAutoActualizaciones + ' actualizaciones automáticas. Si seguís, el stock se actualiza ahora y vuelve a actualizarse cada '
                + intervaloAutoMinutos + ' minutos (otras ' + maxAutoActualizaciones + ' veces).',
            showCancelButton: true,
            confirmButtonText: 'Sí, seguir actualizando',
            cancelButtonText: 'No, dejar apagada',
            allowOutsideClick: false
        }).then(function (r) {
            if (r.isConfirmed) {
                $switchAuto.prop('checked', true);
                autoActualizacionesEjecutadas = 0;
                disparoAutomatico = true;
                disparoCuentaVuelta = false;
                $form.trigger('submit');
            } else {
                $lblEstado.text('Autoactualización apagada. Podés volver a encenderla con el botón Auto.');
            }
        });
    }

    // Programa la proxima busqueda automatica. retardoMs: por defecto el intervalo completo; tras "servidor ocupado" el reintento.
    // cuentaVuelta=false: esa busqueda no gasta una de las vueltas del ciclo (reintento de la 1.a busqueda).
    function programarProximaAutoActualizacion(retardoMs, cuentaVuelta) {
        limpiarTimerAutoActualizacion();

        if (!$switchAuto.is(':checked')) {
            actualizarTextoAutoActualizacion();
            return;
        }

        // Pedido en curso: no se programa nada; al apagarlo establecerPedidoActivo(false) vuelve a programar.
        if (pedidoActivo) {
            actualizarTextoAutoActualizacion();
            return;
        }

        if (autoActualizacionesEjecutadas >= maxAutoActualizaciones) {
            preguntarSeguirActualizando();
            return;
        }

        var espera = retardoMs || (intervaloAutoMinutos * 60 * 1000);
        var cuenta = cuentaVuelta !== false;
        autoActualizacionProximaEjecucion = Date.now() + espera;
        autoActualizacionTimer = window.setTimeout(function () {
            autoActualizacionTimer = null;
            if (!$switchAuto.is(':checked')) { actualizarTextoAutoActualizacion(); return; }
            // Con un editor de punto de stock abierto no se pisa la tabla: espera un rato.
            if (editorPuntoAbierto) { programarProximaAutoActualizacion(15000, cuenta); return; }
            // Un pedido en curso pausa la autoactualizacion (el temporizador ya se limpio al encenderlo; esto cubre la carrera).
            if (pedidoActivo) { actualizarTextoAutoActualizacion(); return; }
            disparoAutomatico = true;
            disparoCuentaVuelta = cuenta;
            $form.trigger('submit');
        }, espera);

        autoActualizacionCountdownTimer = window.setInterval(actualizarTextoAutoActualizacion, 1000);
        actualizarTextoAutoActualizacion();
    }

    function cancelarBusquedaInicial() {
        if (inicialTimer) { window.clearTimeout(inicialTimer); inicialTimer = null; }
    }

    // Al abrir la pantalla: autoactualizacion encendida y primera busqueda sola, con una espera al azar para que las pantallas
    // abiertas a la vez (apertura del turno) no lleguen todas juntas al servidor.
    function iniciarAutoActualizacion() {
        $switchAuto.prop('checked', true);
        var maxMs = (cfg.jitterInicialMaxSegundos || 0) * 1000;
        var espera = maxMs > 0 ? Math.floor(Math.random() * (maxMs + 1)) : 0;
        $lblEstado.text(espera > 0 ? 'Consultando el stock en unos segundos…' : 'Consultando stock...');
        actualizarTextoAutoActualizacion();
        inicialTimer = window.setTimeout(function () {
            inicialTimer = null;
            disparoAutomatico = true;
            disparoCuentaVuelta = false;
            $form.trigger('submit');
        }, espera);
    }

    // ---------- Busqueda ----------

    $form.on('submit', function (e) {
        e.preventDefault();
        cancelarBusquedaInicial();

        var esAutomatica = disparoAutomatico;
        var cuentaVuelta = disparoCuentaVuelta;
        disparoAutomatico = false;
        disparoCuentaVuelta = false;
        var reintento = null;

        // Fecha hasta siempre se recalcula al momento exacto de la busqueda (manual o autoactualizada).
        $fechaHasta.val(ahoraLocalInput());
        setBuscando(true);
        $lblEstado.text('Consultando stock...');

        var datos = $form.serialize() + (esAutomatica ? '&auto=true' : '');

        $.ajax({ url: cfg.buscarUrl, type: 'GET', data: datos }).done(function (html) {
            // Una busqueda AUTOMATICA que ya estaba en vuelo cuando se encendio "Ingresar pedido" no pisa la tabla (se perderia el
            // foco de lo que se esta tipeando). Solo si ya habia una tabla: la primera busqueda del arranque siempre se muestra.
            if (esAutomatica && pedidoActivo && consultaAplicada) {
                $lblEstado.text('Actualización omitida: hay un pedido en curso.');
                return;
            }
            $contenedor.html(html);
            ultimaConsulta = datos;
            consultaAplicada = obtenerEstadoConsulta();
            $lblUltima.text(new Date().toLocaleString('es-AR'));
            actualizarPendiente();
            refrescarVistaTabla();
            if (esAutomatica && cuentaVuelta) {
                autoActualizacionesEjecutadas += 1;
                $lblEstado.text('Consulta actualizada. Autoactualización ' + autoActualizacionesEjecutadas + ' de ' + maxAutoActualizaciones + '.');
            } else {
                $lblEstado.text('Consulta actualizada.');
            }
        }).fail(function (xhr) {
            if (xhr.status === 503 && esAutomatica) {
                // Servidor ocupado: no se pisa la tabla que ya se ve, no se gasta una vuelta y se reintenta mas tarde.
                var seg = (xhr.responseJSON && xhr.responseJSON.reintentoSegundos) || cfg.reintentoCargaSegundos || 120;
                reintento = { ms: seg * 1000, cuentaVuelta: cuentaVuelta };
                $lblEstado.text('Servidor ocupado, reintento a las ' + horaCorta(new Date(Date.now() + seg * 1000)) + '.');
                return;
            }
            $contenedor.html('<div class="alert alert-danger m-3 mb-0">No se pudo consultar la existencia por sucursales.</div>');
            $lblEstado.text('Ocurrió un error al consultar stock.');
        }).always(function () {
            setBuscando(false);
            if (!$switchAuto.is(':checked')) {
                actualizarTextoAutoActualizacion();
                return;
            }
            if (reintento) {
                programarProximaAutoActualizacion(reintento.ms, reintento.cuentaVuelta);
            } else if (esAutomatica && cuentaVuelta && autoActualizacionesEjecutadas >= maxAutoActualizaciones) {
                preguntarSeguirActualizando();
            } else {
                programarProximaAutoActualizacion();
            }
        });
    });

    // ---------- Punto de stock: editor flotante ----------

    function cerrarEditorPunto() {
        $('.ex-pop-pto').remove();
        editorPuntoAbierto = false;
        $(document).off('mousedown.exPunto keydown.exPunto');
    }

    // Recalcula la celda Estado (y el color del stock) de una sucursal con el punto nuevo.
    function actualizarCeldaEstado($td, punto) {
        var stock = parseFloat($td.attr('data-stock'));
        var est = calcularEstado(stock, punto);
        $td.attr('data-punto', punto);
        $td.find('.js-ex-estado-texto').attr('class', 'ex-est js-ex-estado-texto ' + claseEstado(est)).text(textoEstado(est));
        $td.find('.js-ex-pto').text(punto > 0 ? 'pto ' + fmt0.format(punto) : '');
        $td.prev('td').removeClass('text-danger text-warning text-muted font-weight-bold').addClass(claseStock(est));
    }

    // Recalcula lo que depende del punto en toda la fila: claves de orden, clases del filtro y el Estado del Total.
    function recalcularFila($tr) {
        var clases = {};
        var sinPunto = false;
        var sumaPuntos = 0;

        $tr.find('td.ex-col-estado[data-id-sucursal]').each(function () {
            var $td = $(this);
            var est = calcularEstado(parseFloat($td.attr('data-stock')), parseFloat($td.attr('data-punto')));
            $tr.attr('data-e-' + $td.attr('data-id-sucursal'), est.clave);
            if (est.claseFiltro) clases[est.claseFiltro] = true;
            if (est.punto <= 0) sinPunto = true;
            sumaPuntos += est.punto;
        });

        $tr.attr('data-clases', Object.keys(clases).join(' ')).attr('data-sinpto', sinPunto ? '1' : '0');

        var $total = $tr.find('td.ex-estado-total');
        if ($total.length) {
            var estTotal = calcularEstado(parseFloat($tr.attr('data-s-t')), sumaPuntos);
            $tr.attr('data-e-t', estTotal.clave);
            $total.find('.js-ex-estado-texto').attr('class', 'ex-est js-ex-estado-texto ' + claseEstado(estTotal)).text(textoEstado(estTotal));
            $total.find('.js-ex-pto').text(sumaPuntos > 0 ? 'pto ' + fmt0.format(sumaPuntos) : '');
        }
    }

    function abrirEditorPunto($btn) {
        cerrarEditorPunto();

        var $td = $btn.closest('td');
        var $tr = $td.closest('tr');
        var idCorte = parseInt($td.attr('data-id-corte'), 10);
        var idSucursal = parseInt($td.attr('data-id-sucursal'), 10);
        var puntoActual = parseFloat($td.attr('data-punto')) || 0;

        var $pop = $(
            '<div class="ex-pop-pto" role="dialog" aria-label="Editar punto de stock">' +
            '<div class="ex-pop-titulo">Punto de stock</div><div class="ex-pop-sub"></div>' +
            '<div class="ex-pop-fila"><input type="number" class="form-control form-control-sm" min="0" step="1" inputmode="numeric" />' +
            '<button type="button" class="btn btn-primary btn-sm ex-pop-guardar">Guardar</button>' +
            '<button type="button" class="btn btn-outline-secondary btn-sm ex-pop-cancelar" title="Cerrar">&times;</button></div>' +
            '<div class="ex-pop-error" role="alert"></div></div>');
        $pop.find('.ex-pop-sub').text($btn.data('nombre') + ' · ' + $btn.data('sucursal'));
        var $input = $pop.find('input').val(Math.round(puntoActual));
        $('body').append($pop);

        // Posicion: debajo del lapiz, sin salirse de la ventana.
        var r = $btn[0].getBoundingClientRect();
        var ancho = $pop.outerWidth(), alto = $pop.outerHeight();
        var izquierda = Math.max(8, Math.min(r.left, window.innerWidth - ancho - 8));
        var arriba = r.bottom + 6 + alto > window.innerHeight ? Math.max(8, r.top - alto - 6) : r.bottom + 6;
        $pop.css({ left: izquierda + 'px', top: arriba + 'px' });

        editorPuntoAbierto = true;
        $input.trigger('focus').trigger('select');

        function guardar() {
            var valor = parseInt($input.val(), 10);
            if (!Number.isFinite(valor) || valor < 0 || String(valor) !== String($input.val()).trim()) {
                $pop.find('.ex-pop-error').text('El punto de stock debe ser un número entero mayor o igual a 0.');
                return;
            }

            $pop.find('.ex-pop-guardar').prop('disabled', true);
            var datos = { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val(), idCorte: idCorte };
            datos['valores[0].IdSucursal'] = idSucursal;
            datos['valores[0].PuntoStock'] = valor;

            $.ajax({ url: cfg.guardarPuntoUrl, type: 'POST', data: datos }).done(function (resp) {
                if (resp && resp.error) {
                    $pop.find('.ex-pop-error').text(resp.error);
                    $pop.find('.ex-pop-guardar').prop('disabled', false);
                    return;
                }
                actualizarCeldaEstado($td, valor);
                recalcularFila($tr);
                cerrarEditorPunto();
                ordenarTabla();
                aplicarFiltrosVisuales();
                $page.trigger('existencia:puntoCambiado', [$tr.attr('data-id-corte'), idSucursal]); // las sugerencias del pedido dependen del punto
            }).fail(function () {
                $pop.find('.ex-pop-error').text('No se pudo guardar el punto de stock. Intentá de nuevo.');
                $pop.find('.ex-pop-guardar').prop('disabled', false);
            });
        }

        $pop.on('click', '.ex-pop-guardar', guardar);
        $pop.on('click', '.ex-pop-cancelar', cerrarEditorPunto);
        $input.on('keydown', function (e) {
            if (e.key === 'Enter') { e.preventDefault(); guardar(); }
        });
        $(document).on('keydown.exPunto', function (e) { if (e.key === 'Escape') cerrarEditorPunto(); });
        $(document).on('mousedown.exPunto', function (e) {
            if (!$(e.target).closest('.ex-pop-pto, .ex-btn-pto').length) cerrarEditorPunto();
        });
    }

    // ---------- Exportar (Excel / PDF): lo que se ve, con el filtro y el orden actuales ----------

    function exportar(formato) {
        if (!ultimaConsulta) {
            avisar('Sin datos', 'Primero hacé una búsqueda para poder exportar.', 'info');
            return;
        }

        var ids = $contenedor.find('.existencia-producto-row:not(.d-none)').map(function () { return $(this).attr('data-id-corte'); }).get();
        if (!ids.length) {
            avisar('Sin productos', 'No hay productos para exportar con los filtros actuales.', 'info');
            return;
        }

        var cuerpo = new URLSearchParams(ultimaConsulta);
        cuerpo.set('formato', formato);
        cuerpo.set('idsCorte', ids.join(','));
        cuerpo.set('mostrarEstado', $switchEstado.is(':checked') ? 'true' : 'false');
        cuerpo.set('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').first().val() || '');
        cuerpo.delete('auto');

        var $botones = $('#btnExcelExistencia, #btnPdfExistencia').prop('disabled', true);
        window.fetch(cfg.exportarUrl, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8' },
            body: cuerpo.toString()
        }).then(function (resp) {
            if (!resp.ok) {
                return resp.text().then(function (t) { throw new Error(t || ('Error ' + resp.status)); });
            }
            var disposicion = resp.headers.get('Content-Disposition') || '';
            var m = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(disposicion);
            var nombre = m ? decodeURIComponent(m[1]) : ('existencia-por-sucursales.' + (formato === 'pdf' ? 'pdf' : 'xlsx'));
            return resp.blob().then(function (blob) { return { blob: blob, nombre: nombre }; });
        }).then(function (archivo) {
            var url = window.URL.createObjectURL(archivo.blob);
            var a = document.createElement('a');
            a.href = url;
            a.download = archivo.nombre;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.setTimeout(function () { window.URL.revokeObjectURL(url); }, 4000);
        }).catch(function (err) {
            avisar('No se pudo exportar', (err && err.message ? err.message : 'Intentá de nuevo.').slice(0, 300), 'error');
        }).finally(function () {
            $botones.prop('disabled', false);
        });
    }

    // ---------- Eventos ----------

    $('#btnAbrirVentanaExistencia').on('click', function () {
        window.abrirVentanaIndependiente(cfg.ventanaUrl, cfg.ventanaNombre);
    });
    $('#btnExcelExistencia').on('click', function () { exportar('xlsx'); });
    $('#btnPdfExistencia').on('click', function () { exportar('pdf'); });

    function alternarPanelAvanzados(abrir) {
        $panelAvanzados.toggleClass('d-none', !abrir);
        $btnAvanzados.attr('aria-expanded', abrir ? 'true' : 'false');
        window.setTimeout(ajustarAltoTabla, 0);
    }

    $btnAvanzados.on('click', function () { alternarPanelAvanzados($panelAvanzados.hasClass('d-none')); });

    // Sucursales: el menu no se cierra al tildar.
    $('#menuSucursalesExistencia').on('click', function (e) { e.stopPropagation(); });
    $chkSucursales.on('change', function () {
        sincronizarSucursales(this);
        actualizarPendiente();
        actualizarFechaMinimaExistencia();
    });
    $chkTodasSucursales.on('change', function () {
        sincronizarSucursales('todas');
        actualizarPendiente();
        actualizarFechaMinimaExistencia();
    });

    // Tipos / Proveedores / Marcas: sin nada tildado = sin filtro ("Todos").
    $('.ex-ms-menu').on('click', function (e) { e.stopPropagation(); });
    $('.ex-ms').on('change', '.ex-ms-chk', function () {
        actualizarResumenMs($(this).closest('.ex-ms'));
        actualizarBadgeAvanzados();
        actualizarPendiente();
    });
    $('.ex-ms').on('change', '.ex-ms-todos', function () {
        var $ms = $(this).closest('.ex-ms');
        $ms.find('.ex-ms-chk').prop('checked', false); // "Todos" = sin filtro; destildarlo a mano no tiene efecto (se vuelve a tildar)
        actualizarResumenMs($ms);
        actualizarBadgeAvanzados();
        actualizarPendiente();
    });
    $('.ex-ms').on('input', '.ex-ms-buscar', function () {
        var q = ($(this).val() || '').trim().toLowerCase();
        $(this).closest('.ex-ms').find('.ex-ms-item').each(function () {
            $(this).toggleClass('d-none', q.length > 0 && ($(this).attr('data-texto') || '').indexOf(q) < 0);
        });
    });

    $('.ex-periodo-dias').on('click', function () { aplicarPeriodoDias(parseInt($(this).attr('data-dias'), 10)); });
    $periodo.on('change input', function () {
        $('.ex-periodo-dias').removeClass('active'); // fecha tipeada a mano: ningun atajo coincide
        actualizarPendiente();
        $page.trigger('existencia:periodoCambiado');
    });
    $chkMovimiento.on('change', function () {
        actualizarControlesMovimiento();
        actualizarBadgeAvanzados();
        actualizarPendiente();
    });
    $movMinimo.add($movMedida).on('input change', actualizarPendiente);

    // Filtros visuales de la barra.
    $texto.on('input', function () { actualizarPendiente(); aplicarFiltrosVisuales(); });
    $claseEstado.on('change', aplicarFiltrosVisuales);

    $switchEstado.on('change', function () { aplicarEstadoStockExistencia(); ajustarAltoTabla(); });
    $switchExpandir.on('change', aplicarExpandidoGlobal);
    $switchCompacto.on('change', cambiarCompacto);

    $switchAuto.on('change', function () {
        cancelarBusquedaInicial();
        if ($(this).is(':checked')) {
            autoActualizacionesEjecutadas = 0;
            $lblEstado.text('Autoactualización activada. Se actualizará cada ' + intervaloAutoMinutos + ' minutos hasta ' + maxAutoActualizaciones + ' veces.');
            programarProximaAutoActualizacion();
        } else {
            limpiarTimerAutoActualizacion();
            autoActualizacionesEjecutadas = 0;
            actualizarTextoAutoActualizacion();
            if (consultaAplicada) $lblEstado.text('Autoactualización desactivada.');
        }
    });

    // Tabla (delegado: se reemplaza entera en cada busqueda).
    $contenedor.on('click', 'th.ex-sortable', function () {
        var clave = $(this).attr('data-sort-key');
        if (!clave) return;
        if (orden.clave === clave) {
            orden.dir = -orden.dir;
        } else {
            orden.clave = clave;
            orden.dir = direccionInicialOrden(clave);
        }
        ordenarTabla();
        actualizarPie();
    });

    $contenedor.on('click', '.btn-toggle-existencia', function (e) {
        e.stopPropagation();
        toggleDetalleExistencia($(this).attr('data-target'));
    });

    $contenedor.on('click', '.ex-btn-pto', function (e) {
        e.stopPropagation();
        abrirEditorPunto($(this));
    });

    $contenedor.on('dblclick', '.existencia-producto-row', function (e) {
        // Los inputs "A pedir" y sus pistas no expanden el detalle (doble click = seleccionar el texto de la cantidad).
        if ($(e.target).closest('.btn-toggle-existencia, .ex-btn-pto, .ex-pedir-cell').length) return;
        toggleDetalleExistencia($(this).attr('data-detail-target'));
    });

    $contenedor.on('touchend', '.existencia-producto-row', function (e) {
        if ($(e.target).closest('.btn-toggle-existencia, .ex-btn-pto, .ex-pedir-cell').length) return;
        var ahora = Date.now();
        var target = $(this).attr('data-detail-target') || '';
        var esDobleToque = ultimoToqueDetalle.target === target && (ahora - ultimoToqueDetalle.time) <= 350;
        ultimoToqueDetalle = { target: target, time: ahora };
        if (!esDobleToque) return;
        e.preventDefault();
        toggleDetalleExistencia(target);
        ultimoToqueDetalle = { target: '', time: 0 };
    });

    $(window).on('resize', ajustarAltoTabla);

    // ---------- API para "Ingresar pedido" (existencia-pedido.js) ----------

    // Enciende/apaga el modo pedido: marca la pagina (muestra las columnas A pedir), pausa la autoactualizacion mientras este
    // encendido y la reanuda al apagarlo (si el interruptor Auto sigue tildado).
    function establecerPedidoActivo(activo) {
        pedidoActivo = !!activo;
        $page.toggleClass('existencia-pedido-on', pedidoActivo);
        if (pedidoActivo) {
            cancelarBusquedaInicial();
            limpiarTimerAutoActualizacion();
            actualizarTextoAutoActualizacion();
        } else if ($switchAuto.is(':checked')) {
            programarProximaAutoActualizacion();
        } else {
            actualizarTextoAutoActualizacion();
        }
        aplicarEstadoStockExistencia();
        ajustarAltoTabla();
    }

    // Abre el panel de Filtros avanzados y deja el cursor en "Periodo desde" (la leyenda del pedido tiene un "Cambiar").
    function irAlPeriodo() {
        alternarPanelAvanzados(true);
        $periodo.trigger('focus');
        if ($periodo[0] && $periodo[0].scrollIntoView) $periodo[0].scrollIntoView({ block: 'nearest' });
    }

    // Deja la barra y los filtros avanzados como estaban en una consulta guardada (foto de obtenerEstadoConsulta) y busca de nuevo.
    // Lo usa el pedido al cargar un borrador. No toca "Periodo desde": la sugerencia se recalcula con el periodo actual.
    function restaurarConsulta(c) {
        c = c || {};
        var idsSuc = (c.idsSucursales || []).map(String);
        if (idsSuc.length) {
            $chkSucursales.each(function () { this.checked = idsSuc.indexOf(this.value) >= 0; });
            if ($chkSucursales.filter(':checked').length === 0) $chkSucursales.prop('checked', true); // ninguna reconocida: todas
            $chkTodasSucursales.prop('checked', $chkSucursales.filter(':checked').length === $chkSucursales.length);
            actualizarTextoSucursales();
            actualizarFechaMinimaExistencia();
        }

        [['tipos', c.tipos], ['proveedores', c.idsProveedores], ['marcas', c.idsMarcas]].forEach(function (par) {
            var valores = (par[1] || []).map(String);
            var $ms = $('.ex-ms[data-ms="' + par[0] + '"]');
            $ms.find('.ex-ms-chk').each(function () { this.checked = valores.indexOf(this.value) >= 0; });
            actualizarResumenMs($ms);
        });

        $texto.val(c.texto || '');
        if (c.conMovimiento && !$chkMovimiento.prop('disabled')) {
            $chkMovimiento.prop('checked', true);
            if (c.movimientoMinimo) $movMinimo.val(c.movimientoMinimo);
            if (c.movimientoMedida) $movMedida.val(c.movimientoMedida);
        } else {
            $chkMovimiento.prop('checked', false);
        }
        actualizarControlesMovimiento();
        actualizarBadgeAvanzados();
        $form.trigger('submit'); // busqueda manual: el stock que se ve sale fresco
    }

    window.ExistenciaPantalla = {
        establecerPedidoActivo: establecerPedidoActivo,
        restaurarConsulta: restaurarConsulta,
        irAlPeriodo: irAlPeriodo,
        ordenar: ordenarTabla,
        reaplicarFiltros: aplicarFiltrosVisuales,
        ajustarAlto: ajustarAltoTabla,
        hayTabla: function () { return consultaAplicada !== null; },
        periodoDesde: function () { return $periodo.val() || ''; },
        diasPeriodoActual: function () {
            var activo = $('.ex-periodo-dias.active');
            return activo.length ? parseInt(activo.attr('data-dias'), 10) : null;
        },
        idsSucursalesTildadas: obtenerIdsSucursalesTildadas,
        consultaAplicada: function () { return consultaAplicada ? JSON.parse(JSON.stringify(consultaAplicada)) : null; },
        avisar: avisar
    };

    // ---------- Arranque ----------

    iniciarCompacto();
    iniciarPeriodo();
    actualizarControlesMovimiento();
    $('.ex-ms').each(function () { actualizarResumenMs($(this)); });
    actualizarBadgeAvanzados();
    actualizarTextoSucursales();
    refrescarVistaTabla();
    iniciarAutoActualizacion();
})(window.jQuery);
