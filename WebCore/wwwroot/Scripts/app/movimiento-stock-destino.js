// Movimientos/Editar: stock de la sucursal DESTINO al lado de la carga del movimiento (Views/Movimientos/Editar.cshtml).
// Boton "Stock destino": con pantalla >= 1200 px abre un panel a la izquierda (un iframe de Stock/ExistenciaPorSucursales en modo movimiento) y la
// carga del movimiento queda a la derecha; con una pantalla mas chica abre ese mismo stock en una ventana independiente. El panel tambien se
// puede independizar (boton de la cabecera). Habla con el panel / ventana por un BroadcastChannel (un canal por pestana: dos movimientos abiertos
// no se cruzan):
//   movimiento -> stock:  { tipo: 'lineas', items }  (lo que lleva el movimiento: el stock marca "+N en mov."),
//                         { tipo: 'destino', idSucursal }, { tipo: 'actualizar' }
//   stock -> movimiento:  { tipo: 'listo' }, { tipo: 'elegir', codigo[, cantidad, pesable] }
// El otro extremo es existencia-movimiento.js. Depende de movimientos.js (window.MovimientosEdit) y de ventana-independiente.js.
// Reglas y porques: docs/DECISIONS.md (2026-10-20, "Stock del destino al lado del movimiento").
(function ($, window) {
    'use strict';

    var $page = $('[data-movimiento-page="1"]');
    var $boton = $('#btnStockDestino');
    // Sin BroadcastChannel (navegadores muy viejos) no hay forma de conectar las dos pantallas: el boton no se ofrece.
    if (!$page.length || typeof window.BroadcastChannel === 'undefined') { $boton.remove(); return; }

    var cfg = window.movimientoStockDestinoConfig || {};
    var CLAVE_PANEL = 'movimientoStockDestino';      // '1' = el panel queda encendido (comodidad por navegador)
    var ANCHO_DOS_COLUMNAS_PX = 1200;                // con menos ancho no hay lugar para dos columnas: el stock va en ventana
    var PREFIJO_CANAL = 'carnisys-mov-stock-';
    var NOMBRE_VENTANA = 'stockDestinoMovimiento';
    var FRACCION_VENTANA = 1 / 3;

    var canalId = 'm' + Math.random().toString(36).slice(2, 12);   // solo letras y numeros (el servidor lo valida igual)
    var canal = new window.BroadcastChannel(PREFIJO_CANAL + canalId);

    var $layout = $('#movLayout');
    var $panel = $('#panelStockDestino');
    var $frame = $('#frameStockDestino');
    var $aviso = $('#avisoStockDestino');
    var $titulo = $('#lblTituloStockDestino');
    var $destino = $('#IdSucursalDestino');

    function leerLS(clave) {
        try { return window.localStorage.getItem(clave); } catch (e) { return null; /* sin almacenamiento: no se recuerda */ }
    }

    function guardarLS(clave, valor) {
        try { window.localStorage.setItem(clave, valor); } catch (e) { /* no es un error: simplemente no se recuerda */ }
    }

    function avisar(titulo, texto) {
        if (window.Swal && typeof window.Swal.fire === 'function') window.Swal.fire(titulo, texto, 'info');
        else window.alert(titulo + ': ' + texto);
    }

    function idDestino() { return parseInt($destino.val(), 10) || 0; }

    function nombreDestino() { return ($destino.find('option:selected').text() || '').trim(); }

    function urlStock(id) {
        return (cfg.url || '') + '?popup=true&modo=movimiento&idSucursal=' + id + '&canal=' + encodeURIComponent(canalId);
    }

    function panelVisible() { return $layout.hasClass('mov-con-stock'); }

    // ---------- Panel ----------

    // Carga (o recarga) el iframe con el destino elegido; sin destino muestra el aviso. data-id evita recargar si ya muestra esa sucursal.
    function cargarFrame() {
        var id = idDestino();
        $titulo.text(id > 0 ? 'Stock en ' + nombreDestino() : 'Stock de la sucursal destino');
        $aviso.toggleClass('d-none', id > 0);
        $frame.toggleClass('d-none', id <= 0);
        if (id <= 0) { $frame.attr('src', 'about:blank').attr('data-id', ''); return; }
        if (String($frame.attr('data-id') || '') !== String(id)) {
            $frame.attr('data-id', String(id)).attr('src', urlStock(id));
        }
    }

    function mostrarPanel(mostrar, recordar) {
        $layout.toggleClass('mov-con-stock', mostrar);
        $panel.toggleClass('d-none', !mostrar);
        $boton.attr('aria-pressed', mostrar ? 'true' : 'false').toggleClass('active', mostrar);
        if (mostrar) cargarFrame();
        else $frame.attr('src', 'about:blank').attr('data-id', ''); // cerrado: no sigue consultando el stock en segundo plano
        if (recordar) guardarLS(CLAVE_PANEL, mostrar ? '1' : '0');
    }

    function abrirVentana() {
        var id = idDestino();
        if (id <= 0) { avisar('Sucursal destino', 'Elegí primero la sucursal destino del movimiento.'); return; }
        if (typeof window.abrirVentanaIndependiente !== 'function') return;
        // El stock pasa a la ventana: el panel se cierra para no consultarlo dos veces.
        if (panelVisible()) mostrarPanel(false, false);
        window.abrirVentanaIndependiente(urlStock(id), NOMBRE_VENTANA, { anchoFraccionPantalla: FRACCION_VENTANA });
    }

    // ---------- Mensajes ----------

    function enviarLineas() {
        var resumen = window.MovimientosEdit && typeof window.MovimientosEdit.resumenLineas === 'function' ? window.MovimientosEdit.resumenLineas() : {};
        canal.postMessage({ tipo: 'lineas', items: resumen });
    }

    canal.onmessage = function (ev) {
        var m = ev && ev.data;
        if (!m || typeof m.tipo !== 'string') return;

        if (m.tipo === 'listo') {
            enviarLineas();
        } else if (m.tipo === 'elegir') {
            // Input de otra pantalla: solo un codigo entero positivo y, si viene, una cantidad numerica positiva.
            var codigo = parseInt(m.codigo, 10);
            var cantidad = Number(m.cantidad);
            if (!(codigo > 0) || !window.MovimientosEdit || typeof window.MovimientosEdit.cargarProducto !== 'function') return;
            window.MovimientosEdit.cargarProducto(codigo, isFinite(cantidad) && cantidad > 0 ? cantidad : 0, m.pesable === true);
            try { window.focus(); } catch (e) { /* el navegador puede negar el foco a una ventana en segundo plano */ }
        }
    };

    // ---------- Eventos de la pantalla ----------

    $(document).on('movimiento:lineas', function (e, resumen) {
        canal.postMessage({ tipo: 'lineas', items: resumen });
    });

    // Al cambiar el destino, el stock sigue a esa sucursal. Si el panel esta a la vista se recarga su iframe directamente (no depende de que
    // el iframe ya este escuchando); si el stock esta en una ventana independiente (el panel esta cerrado) se le avisa por el canal y se recarga sola.
    $destino.on('change', function () {
        var id = idDestino();
        if (panelVisible()) cargarFrame();
        else if (id > 0) canal.postMessage({ tipo: 'destino', idSucursal: id });
    });

    $boton.on('click', function () {
        if (panelVisible()) { mostrarPanel(false, true); return; }
        if (window.innerWidth >= ANCHO_DOS_COLUMNAS_PX) mostrarPanel(true, true);
        else abrirVentana();
    });
    $('#btnStockDestinoCerrar').on('click', function () { mostrarPanel(false, true); });
    $('#btnStockDestinoVentana').on('click', abrirVentana);
    $('#btnStockDestinoActualizar').on('click', function () { canal.postMessage({ tipo: 'actualizar' }); });

    // Si la ventana se achica por debajo de lo que permiten dos columnas, el panel se cierra (sin olvidar la eleccion: vuelve al agrandarla).
    $(window).on('resize', function () {
        if (panelVisible() && window.innerWidth < ANCHO_DOS_COLUMNAS_PX) mostrarPanel(false, false);
        else if (!panelVisible() && leerLS(CLAVE_PANEL) === '1' && window.innerWidth >= ANCHO_DOS_COLUMNAS_PX) mostrarPanel(true, false);
    });

    // ---------- Arranque ----------

    if (leerLS(CLAVE_PANEL) === '1' && window.innerWidth >= ANCHO_DOS_COLUMNAS_PX) mostrarPanel(true, false);
})(window.jQuery, window);
