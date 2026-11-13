// Movimientos/Editar: stock de la sucursal DESTINO al lado de la carga del movimiento (Views/Movimientos/Editar.cshtml).
// Boton "Stock destino": con pantalla >= 1200 px abre un panel a la izquierda (un iframe de Stock/ExistenciaPorSucursales en modo movimiento) y la
// carga del movimiento queda a la derecha; con una pantalla mas chica abre ese mismo stock en una ventana independiente. El panel tambien se
// puede independizar (boton de la cabecera). Habla con el panel / ventana por un BroadcastChannel (un canal por pestana: dos movimientos abiertos
// no se cruzan):
//   movimiento -> stock:  { tipo: 'lineas', items }  (lo que lleva el movimiento: el stock marca "+N en mov."),
//                         { tipo: 'origen', idSucursal, nombre }  (sucursal de la que sale la mercaderia: columna "Stock origen"),
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

    // Ventana independiente angosta (ventana-angosta.js, 2026-11-13): el stock al lado SOLO cuando la ventana es lo bastante ancha, y la ventana NO se
    // ensancha sola: lo hace el usuario con el boton "Ensanchar" (o arrastrando el borde). Ancho necesario = panel + carga minima + separacion.
    var enVentanaAngosta = window.__ventanaAngosta === true;
    var PANEL_REM = 39;                 // ancho del panel (pantalla-trabajo.css, --pt-mov-ancho-panel-stock)
    var CARGA_MIN_PX = 480;             // lo minimo que necesita la carga (diseno angosto de movimientos-compacto.css)
    var SEPARACION_Y_MARGENES_PX = 52;  // hueco entre columnas (12 px) + margenes de la pagina
    var ESPERA_MEDIR_VENTANA_MS = 400;  // el navegador aplica el cambio de tamano un instante despues del resizeTo
    var $btnEnsanchar = null;
    var tamanoOriginal = null;          // { ancho, izquierda } de la ventana antes de ensancharla
    var abrirPanelAlEnsanchar = false;  // el usuario eligio "Ensanchar la ventana" desde el aviso: al estar ancha se abre el panel

    var canalId = 'm' + Math.random().toString(36).slice(2, 12);   // solo letras y numeros (el servidor lo valida igual)
    var canal = new window.BroadcastChannel(PREFIJO_CANAL + canalId);

    var $layout = $('#movLayout');
    var $panel = $('#panelStockDestino');
    var $frame = $('#frameStockDestino');
    var $aviso = $('#avisoStockDestino');
    var $titulo = $('#lblTituloStockDestino');
    var $destino = $('#IdSucursalDestino');
    var $origen = $('#IdSucursalOrigen');

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

    // En la ventana angosta .mov-con-stock queda fija (es su acomodo); el panel abierto se marca con .mov-stock-abierto.
    function panelVisible() { return $layout.hasClass(enVentanaAngosta ? 'mov-stock-abierto' : 'mov-con-stock'); }

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
        if (!enVentanaAngosta) $layout.toggleClass('mov-con-stock', mostrar);
        $layout.toggleClass('mov-stock-abierto', mostrar);
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

    // ---------- Ventana angosta: boton Ensanchar ----------

    function remPx() { return parseFloat(window.getComputedStyle(document.documentElement).fontSize) || 16; }

    // Ancho interior que necesita la ventana para ver el stock al lado de la carga.
    function anchoNecesarioPx() { return Math.round(PANEL_REM * remPx() + CARGA_MIN_PX + SEPARACION_Y_MARGENES_PX); }

    function anchoPantallaPx() { return (window.screen && (window.screen.availWidth || window.screen.width)) || 0; }

    // Marco de la ventana (bordes y barra de titulo laterales): lo que separa el ancho exterior del interior.
    function marcoPx() { return Math.max(0, window.outerWidth - window.innerWidth); }

    function entraEnPantalla() {
        var disponible = anchoPantallaPx();
        return disponible <= 0 || anchoNecesarioPx() + marcoPx() <= disponible;
    }

    // Cuanto mas ancha tiene que quedar la ventana para que entren el stock y la carga, en % del ancho de hoy.
    function porcentajeParaEnsanchar() {
        return Math.max(0, Math.round((anchoNecesarioPx() / Math.max(window.innerWidth, 1) - 1) * 100));
    }

    function cambiarAncho(anchoExterior, izquierda) {
        try {
            window.resizeTo(anchoExterior, window.outerHeight);
            window.moveTo(izquierda, window.screenY || 0);
        } catch (e) {
            console.error('No se pudo cambiar el tamano de la ventana', e);
        }
    }

    // Ensancha lo justo para que entren los dos (tope: el ancho de la pantalla) y, si se saldria por la derecha, corre la ventana a la izquierda.
    function ensancharVentana() {
        var disponible = anchoPantallaPx();
        var origen = (window.screen && window.screen.availLeft) || 0;
        var objetivo = anchoNecesarioPx() + marcoPx();
        if (disponible > 0) objetivo = Math.min(objetivo, disponible);
        var izquierda = window.screenX || 0;
        if (disponible > 0 && izquierda + objetivo > origen + disponible) izquierda = Math.max(origen, origen + disponible - objetivo);
        if (!tamanoOriginal) tamanoOriginal = { ancho: window.outerWidth, izquierda: window.screenX || 0 };
        var anchoAntes = window.outerWidth;

        cambiarAncho(objetivo, izquierda);

        window.setTimeout(function () {
            if (window.outerWidth <= anchoAntes) {
                abrirPanelAlEnsanchar = false;
                avisar('No se pudo ensanchar sola', 'Ensanchá la ventana arrastrando el borde: para ver el stock al lado necesita unos ' + anchoNecesarioPx() + ' px de ancho.');
            }
            actualizarBotonEnsanchar();
        }, ESPERA_MEDIR_VENTANA_MS);
    }

    function restaurarVentana() {
        abrirPanelAlEnsanchar = false;
        if (!tamanoOriginal) return;
        cambiarAncho(tamanoOriginal.ancho, tamanoOriginal.izquierda);
        tamanoOriginal = null;
    }

    function actualizarBotonEnsanchar() {
        if (!$btnEnsanchar) return;
        var ancha = window.innerWidth >= anchoNecesarioPx();
        $btnEnsanchar.find('i').attr('class', 'fas ' + (ancha ? 'fa-compress-arrows-alt' : 'fa-arrows-alt-h'));
        var titulo = ancha ? 'Volver al ancho de antes' : 'Ensanchar la ventana para ver el stock al lado (' + porcentajeParaEnsanchar() + ' % más ancha)';
        $btnEnsanchar.attr({ title: titulo, 'aria-label': titulo });
    }

    // «Stock destino» con la ventana demasiado angosta: no se ensancha sola; se le pregunta al usuario.
    function preguntarEnsanchar() {
        if (!(window.Swal && typeof window.Swal.fire === 'function')) { abrirVentana(); return; }
        var cabe = entraEnPantalla();
        var texto = cabe
            ? 'Para ver el stock al lado de la carga la ventana tiene que medir unos ' + anchoNecesarioPx() + ' px (hoy ' + window.innerWidth + ' px: ' +
              porcentajeParaEnsanchar() + ' % más ancha).'
            : 'Tu pantalla no alcanza para ver el stock al lado de la carga (hacen falta unos ' + (anchoNecesarioPx() + marcoPx()) + ' px). Podés abrirlo en otra ventana.';
        window.Swal.fire({
            title: 'El stock no entra con este ancho',
            text: texto,
            icon: 'info',
            showConfirmButton: cabe,
            confirmButtonText: 'Ensanchar la ventana',
            showDenyButton: true,
            denyButtonText: 'Abrir en otra ventana',
            showCancelButton: true,
            cancelButtonText: 'Cancelar'
        }).then(function (r) {
            if (r.isConfirmed) { abrirPanelAlEnsanchar = true; ensancharVentana(); }
            else if (r.isDenied) abrirVentana();
        });
    }

    // ---------- Mensajes ----------

    function enviarLineas() {
        var resumen = window.MovimientosEdit && typeof window.MovimientosEdit.resumenLineas === 'function' ? window.MovimientosEdit.resumenLineas() : {};
        canal.postMessage({ tipo: 'lineas', items: resumen });
    }

    // Sucursal origen del movimiento (id + nombre) para la columna "Stock origen". Sin origen elegido manda id 0 (el stock vacia la columna).
    function enviarOrigen() {
        var id = parseInt($origen.val(), 10) || 0;
        var nombre = id > 0 ? ($origen.find('option:selected').text() || '').trim() : '';
        canal.postMessage({ tipo: 'origen', idSucursal: id, nombre: nombre });
    }

    canal.onmessage = function (ev) {
        var m = ev && ev.data;
        if (!m || typeof m.tipo !== 'string') return;

        if (m.tipo === 'listo') {
            enviarLineas();
            enviarOrigen();
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

    // Al cambiar el origen, el stock (panel o ventana independiente) actualiza la columna "Stock origen".
    $origen.on('change', enviarOrigen);

    // Al cambiar el destino, el stock sigue a esa sucursal. Si el panel esta a la vista se recarga su iframe directamente (no depende de que
    // el iframe ya este escuchando); si el stock esta en una ventana independiente (el panel esta cerrado) se le avisa por el canal y se recarga sola.
    $destino.on('change', function () {
        var id = idDestino();
        if (panelVisible()) cargarFrame();
        else if (id > 0) canal.postMessage({ tipo: 'destino', idSucursal: id });
    });

    $boton.on('click', function () {
        if (panelVisible()) { mostrarPanel(false, true); return; }
        if (enVentanaAngosta) {
            if (window.innerWidth >= anchoNecesarioPx()) mostrarPanel(true, true);
            else preguntarEnsanchar();
            return;
        }
        if (window.innerWidth >= ANCHO_DOS_COLUMNAS_PX) mostrarPanel(true, true);
        else abrirVentana();
    });
    $('#btnStockDestinoCerrar').on('click', function () { mostrarPanel(false, true); });
    $('#btnStockDestinoVentana').on('click', abrirVentana);
    $('#btnStockDestinoActualizar').on('click', function () { canal.postMessage({ tipo: 'actualizar' }); });

    // Si la ventana se achica por debajo de lo que permiten dos columnas, el panel se cierra (sin olvidar la eleccion: vuelve al agrandarla).
    $(window).on('resize', function () {
        // Ventana independiente angosta: el panel se cierra si la ventana vuelve a quedar angosta y se abre solo si el usuario eligio
        // «Ensanchar la ventana» desde el aviso; el boton Ensanchar refleja el ancho de hoy.
        if (enVentanaAngosta) {
            var cabe = window.innerWidth >= anchoNecesarioPx();
            if (panelVisible() && !cabe) mostrarPanel(false, false);
            else if (!panelVisible() && cabe && abrirPanelAlEnsanchar) { abrirPanelAlEnsanchar = false; mostrarPanel(true, true); }
            actualizarBotonEnsanchar();
            return;
        }
        if (panelVisible() && window.innerWidth < ANCHO_DOS_COLUMNAS_PX) mostrarPanel(false, false);
        else if (!panelVisible() && leerLS(CLAVE_PANEL) === '1' && window.innerWidth >= ANCHO_DOS_COLUMNAS_PX) mostrarPanel(true, false);
    });

    // ---------- Arranque ----------

    if (enVentanaAngosta) {
        // Boton «Ensanchar» (solo icono) a la izquierda de pantalla completa. Un clic ensancha lo justo para que entren el stock y la carga; si la
        // ventana ya esta ancha, la vuelve al tamano de antes.
        $btnEnsanchar = $('<button type="button" class="btn btn-outline-secondary" id="btnEnsancharVentana" data-edit-readonly-ignore="true"><i class="fas fa-arrows-alt-h"></i></button>');
        var $pantallaCompleta = $('#btnPantallaCompleta');
        if ($pantallaCompleta.length) $btnEnsanchar.insertBefore($pantallaCompleta);
        else $('.mov-barra-grupo-der').append($btnEnsanchar);
        $btnEnsanchar.on('click', function () {
            if (window.innerWidth >= anchoNecesarioPx()) restaurarVentana();
            else ensancharVentana();
        });
        actualizarBotonEnsanchar();
    }

    if (leerLS(CLAVE_PANEL) === '1' && window.innerWidth >= ANCHO_DOS_COLUMNAS_PX) mostrarPanel(true, false);
})(window.jQuery, window);
