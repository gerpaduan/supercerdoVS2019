// Existencia por sucursales: "Ingresar pedido" (Views/Stock/ExistenciaPorSucursales.cshtml).
// Al encender el interruptor grande, cada sucursal gana la columna "A pedir" (editable) y al final de la tabla aparecen "Total pedido",
// "$ ult. compra" e "Importe est.". Las sugerencias SE VEN (pista gris) pero no se cargan solas: el usuario las acepta por celda, por
// sucursal o todas. El pedido en curso se resguarda en el servidor con el sistema de borradores (modulo PEDIDO_STOCK). Guardar abre una
// vista preliminar con SOLO los productos con cantidad; el guardado definitivo es la Fase 3 (modulo Pedidos).
//
// Dos bloques:
//  1) ExistenciaPedidoCalculo: reglas PURAS (sugerencia, importe, equivalente en unidades, parseo). Espejo de Negocio.PedidoSugerencia:
//     si cambia una regla, cambiarla en los dos lados y en Negocio.Tests/PedidoSugerenciaTests.cs.
//  2) La pantalla: engancha con existencia-sucursales.js por window.ExistenciaPantalla y por eventos sobre #existenciaPage.
// Reglas y porques: docs/DECISIONS.md (2026-10-19) y docs/11-manual-de-uso/pantallas/Stock.ExistenciaPorSucursales.md.

// ============================================================================================================================
// 1) Calculo puro (sin DOM)
// ============================================================================================================================
(function (root) {
    'use strict';

    var DECIMALES = 3;                 // mismos decimales que el stock (N3)
    var MAX_CANTIDAD = 999999.999;     // tope de una cantidad a pedir (frena un valor mal tipeado)
    var SEGUN_CONSUMO = 'consumo', SEGUN_PUNTO = 'punto', SEGUN_MAYOR = 'mayor';

    function saneado(v) {
        return (typeof v !== 'number' || !isFinite(v) || v < 0) ? 0 : v;
    }

    function redondear3(v) {
        return Math.round(v * Math.pow(10, DECIMALES)) / Math.pow(10, DECIMALES);
    }

    function normalizarSegun(segun) {
        var s = String(segun || '').trim().toLowerCase();
        return (s === SEGUN_PUNTO || s === SEGUN_MAYOR) ? s : SEGUN_CONSUMO;
    }

    // Cantidad sugerida de UNA celda (producto x sucursal). Espejo de Negocio.PedidoSugerencia.Calcular:
    //  - stock negativo se toma como 0 (un stock negativo es un error de carga; contarlo inflaria el pedido);
    //  - consumo = ventas + uso en elaborados del periodo; punto = punto de stock de esa sucursal (0 = sin punto);
    //  - sugerido segun el modo: 'consumo' (consumo - stock, por defecto), 'punto' (punto - stock) o 'mayor' (el mayor de los dos).
    function calcularSugerencia(stock, punto, consumo, segun) {
        var s = saneado(stock), p = saneado(punto), c = saneado(consumo);
        var r = { stock: s, punto: p, consumo: c, faltaPorConsumo: 0, faltaHastaPunto: 0, sugerido: 0, bajoPunto: false, stockSuficiente: false };

        r.faltaPorConsumo = c > s ? redondear3(c - s) : 0;
        r.bajoPunto = p > 0 && s < p;
        r.faltaHastaPunto = r.bajoPunto ? redondear3(p - s) : 0;

        var modo = normalizarSegun(segun);
        r.sugerido = modo === SEGUN_PUNTO ? r.faltaHastaPunto
            : modo === SEGUN_MAYOR ? Math.max(r.faltaPorConsumo, r.faltaHastaPunto)
                : r.faltaPorConsumo;

        // El stock cubre el consumo del periodo y el punto, y hay algun dato (consumo o punto) que lo respalde.
        r.stockSuficiente = s >= c && s >= p && (c > 0 || p > 0);
        return r;
    }

    // Redondeo a 2 decimales "al par" en el empate (igual que Math.Round(decimal, 2) de C#, que usa "Que pedir"): 0,125 -> 0,12 y 0,135 -> 0,14.
    function redondearPar2(x) {
        var escalado = x * 100;
        var piso = Math.floor(escalado);
        var resto = escalado - piso;
        var r = Math.abs(resto - 0.5) < 1e-9 ? (piso % 2 === 0 ? piso : piso + 1) : Math.round(escalado);
        return r / 100;
    }

    // Importe estimado de una linea: cantidad x precio de la ultima compra. Sin precio (> 0) o sin cantidad: null ("sin precio", nunca $0 en silencio).
    function importeLinea(cantidad, precio) {
        if (typeof precio !== 'number' || !isFinite(precio) || precio <= 0) return null;
        if (typeof cantidad !== 'number' || !isFinite(cantidad) || cantidad <= 0) return null;
        return redondearPar2(cantidad * precio);
    }

    // Solo productos pesables con promedio cargado (misma regla que StockUnidadesHelper.FormatearEquivalenteUnidades). null = no aplica.
    function equivalenteUnidades(cantidad, promedio, pesable) {
        if (!pesable || !(promedio > 0) || !(cantidad > 0)) return null;
        return cantidad / promedio;
    }

    // Lo que el usuario tipea en "A pedir". Acepta coma o punto decimal ("52,5" / "52.5"); con las dos ("1.234,5") el ultimo es el
    // decimal y el otro separa miles. Devuelve { vacio: true } | { ok: true, valor } | { ok: false }. Mas de 3 decimales se redondean.
    function parsearCantidad(texto) {
        var t = String(texto == null ? '' : texto).replace(/\s+/g, '');
        if (t === '') return { vacio: true };
        if (!/^[0-9.,]+$/.test(t)) return { ok: false };

        var ultimaComa = t.lastIndexOf(','), ultimoPunto = t.lastIndexOf('.');
        var posDecimal = Math.max(ultimaComa, ultimoPunto);
        var entero = t, decimales = '';
        if (posDecimal >= 0) {
            var otraClaseMiles = (ultimaComa >= 0 && ultimoPunto >= 0);
            var apariciones = t.split(t.charAt(posDecimal)).length - 1;
            if (otraClaseMiles || apariciones === 1) {
                entero = t.slice(0, posDecimal);
                decimales = t.slice(posDecimal + 1);
            } // varias apariciones de un solo simbolo ("1.234.567"): son miles, sin decimales
        }
        entero = entero.replace(/[.,]/g, '');
        if (entero === '' && decimales === '') return { ok: false };
        if (!/^[0-9]*$/.test(entero) || !/^[0-9]*$/.test(decimales)) return { ok: false };

        var valor = redondear3(parseFloat((entero === '' ? '0' : entero) + (decimales === '' ? '' : '.' + decimales)));
        if (!isFinite(valor) || valor < 0 || valor > MAX_CANTIDAD) return { ok: false };
        return { ok: true, valor: valor };
    }

    root.ExistenciaPedidoCalculo = {
        MAX_CANTIDAD: MAX_CANTIDAD,
        normalizarSegun: normalizarSegun,
        calcularSugerencia: calcularSugerencia,
        redondear3: redondear3,
        redondearPar2: redondearPar2,
        importeLinea: importeLinea,
        equivalenteUnidades: equivalenteUnidades,
        parsearCantidad: parsearCantidad
    };
})(typeof window !== 'undefined' ? window : globalThis);

// ============================================================================================================================
// 2) Pantalla
// ============================================================================================================================
(function ($, window) {
    'use strict';

    var calculo = window.ExistenciaPedidoCalculo;
    var pantalla = window.ExistenciaPantalla;
    if (!$ || !pantalla || !document.getElementById('existenciaPage')) return; // fuera de la pantalla (o en pruebas sin DOM)

    var cfg = window.ExistenciaPedidoConfig || {};
    var cfgBorrador = cfg.borrador || {};
    // Textos del importe estimado y limite de "precio viejo", del servidor (una sola fuente: Negocio.PedidoTextos y Pedidos:DiasPrecioViejo).
    var cfgPrecios = cfg.precios || {};
    var TXT_ACLARACION = cfgPrecios.aclaracion || 'Importes estimados con el precio de la última compra de cada producto, sin tener en cuenta el proveedor.';
    var TXT_TOTAL = cfgPrecios.etiquetaTotal || 'Total estimado';
    var TXT_TOTAL_PARCIAL = cfgPrecios.etiquetaTotalParcial || 'Total estimado (parcial)';
    var DIAS_PRECIO_VIEJO = cfgPrecios.diasPrecioViejo || 90;

    var DEBOUNCE_PERIODO_MS = 500;                // espera tras cambiar la fecha antes de pedir el consumo de nuevo
    var CLAVE_SEGUN = 'existenciaPedidoSegun';
    var CLAVE_BARRA_COLAPSADA = 'existenciaPedidoBarraColapsada';   // '1' = la barra del pedido queda en una sola linea (se recuerda en este navegador)
    var MS_POR_DIA = 24 * 60 * 60 * 1000;

    var $page = $('#existenciaPage');
    var $contenedor = $('#contenedorTablaExistenciaSucursales');
    var $switch = $('#switchPedidoExistencia');
    var $barra = $('#barraPedidoExistencia');
    var $leyenda = $('#lblLeyendaPedidoExistencia');
    var $selSegun = $('#selSugerirSegunExistencia');
    var $btnTodas = $('#btnAceptarTodasExistencia');
    var $btnPunto = $('#btnCompletarPuntoExistencia');
    var $lblCantPunto = $('#lblCantBajoPuntoExistencia');
    var $btnLimpiar = $('#btnLimpiarPedidoExistencia');
    var $btnGuardar = $('#btnGuardarPedidoExistencia');
    var $lblContador = $('#lblContadorPedidoExistencia');
    var $lblResumenCeldas = $('#lblResumenCeldasPedido');
    var $lblResumenTotal = $('#lblResumenTotalPedido');
    var $modalPrevia = $('#modalPreviaPedidoExistencia');
    var $cuerpoPrevia = $('#cuerpoPreviaPedidoExistencia');
    var $resumenPrevia = $('#resumenPreviaPedidoExistencia');
    var $btnBorradores = $('#btnVerBorradoresPedido');
    var $btnConfirmar = $('#btnConfirmarPedidoExistencia');
    var $labelToggle = $switch.next('label');
    var $switchPrecios = $('#switchPreciosPedidoExistencia');   // "Ver precios": muestra las columnas $ ult. compra e Importe est.
    var $btnColapsar = $('#btnColapsarBarraPedido');            // colapsa la barra del pedido a una linea (resumen + Guardar)
    var $lblMini = $('#lblMiniPedidoExistencia');
    var $btnGuardarMini = $('#btnGuardarMiniPedidoExistencia');

    // ---------- Estado ----------

    var activo = false;              // interruptor "Ingresar pedido" encendido
    var cantidades = {};             // idCorte -> { idSucursal -> cantidad }. Un 0 tipeado se conserva (cuenta como "cargada" al aceptar sugerencias) pero no suma.
    var catalogo = {};               // idCorte -> { codigo, nombre, promedio, pesable, stock:{suc}, punto:{suc} } de la ultima tabla (y de las anteriores)
    var consumo = {};                // idCorte -> { idSucursal -> { ventas, elaboracion } }
    var consumoDesde = null;         // 'aaaa-mm-dd' del consumo cargado
    var consumoHasta = '';           // 'aaaa-mm-dd' del ultimo dia del periodo ('' = sin fecha hasta: se conto hasta ahora)
    var consumoEstado = 'sin';       // sin | cargando | ok | error | nodisponible
    var mensajeConsumo = '';
    var precios = {};                // idCorte -> { precio, proveedor, fecha }
    var preciosDescartados = {};     // idCorte -> 'aaaa-mm-dd' de una compra tan vieja que NO se usa como precio (ver Pedidos:DiasPrecioMaximo)
    var diasPrecioMaximo = 0;        // limite con el que el servidor descarto (para explicarlo en el tooltip)
    var preciosEstado = 'sin';       // sin | cargando | ok | error | nodisponible
    var segun = calculo.normalizarSegun(leerLS(CLAVE_SEGUN, 'consumo'));
    var indice = { tbody: null, filas: {}, celdas: {}, sucursales: [] };
    var secConsumo = 0, secPrecios = 0;
    var timerPeriodo = null;
    var proponerAnchoPendiente = false;  // recien encendido: cuando carguen las sugerencias se ofrece ensanchar la ventana independiente si hace falta
    var restauracionPendiente = false;   // se cargo un borrador: al llegar la tabla se avisa que lineas no aparecen
    var proteccionActiva = false;
    var guardando = false;               // se esta guardando el pedido definitivo (frena el autoguardado del borrador)
    var guardandoPedido = false;         // hay un POST a Pedidos/Guardar en vuelo (evita el doble click)
    var clientIdLocal = null;            // id del pedido cuando no hay borrador en servidor (ver clientIdPedido)
    var seleccionPendiente = null;       // input recien enfocado con el mouse: su mouseup no debe deseleccionar el texto
    var borrador = null;

    // ---------- Utilidades ----------

    var fmtCant3 = new Intl.NumberFormat('es-AR', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
    var fmtInput = new Intl.NumberFormat('es-AR', { maximumFractionDigits: 3, useGrouping: false });
    var fmtUn = new Intl.NumberFormat('es-AR', { maximumFractionDigits: 1 });
    var fmtPlata = new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', minimumFractionDigits: 2, maximumFractionDigits: 2 });

    function leerLS(clave, porDefecto) {
        try {
            var v = window.localStorage.getItem(clave);
            return v === null ? porDefecto : v;
        } catch (e) {
            return porDefecto; // almacenamiento bloqueado: la pantalla funciona igual
        }
    }

    function guardarLS(clave, valor) {
        try { window.localStorage.setItem(clave, valor); } catch (e) { /* sin almacenamiento: no se recuerda, no es un error */ }
    }

    function el(tag, clase, texto) {
        var nodo = document.createElement(tag);
        if (clase) nodo.className = clase;
        if (texto != null) nodo.textContent = texto;
        return nodo;
    }

    function plural(n, singular, pluralTexto) { return n + ' ' + (n === 1 ? singular : pluralTexto); }

    function formatearFecha(iso) { // 'aaaa-mm-dd' -> 'dd/mm/aaaa'
        var p = String(iso || '').split('-');
        return p.length === 3 ? p[2] + '/' + p[1] + '/' + p[0] : '';
    }

    // Dias entre una fecha 'aaaa-mm-dd' y otra (por defecto, hoy). null si la fecha no es valida.
    function diasDesde(iso, hastaIso) {
        var p = String(iso || '').split('-');
        if (p.length !== 3) return null;
        var desde = new Date(parseInt(p[0], 10), parseInt(p[1], 10) - 1, parseInt(p[2], 10));
        var h = String(hastaIso || '').split('-');
        var fin = h.length === 3 ? new Date(parseInt(h[0], 10), parseInt(h[1], 10) - 1, parseInt(h[2], 10)) : new Date();
        fin.setHours(0, 0, 0, 0);
        return Math.round((fin.getTime() - desde.getTime()) / MS_POR_DIA);
    }

    // Dias que tiene el precio de ultima compra (null si no se sabe de cuando es) y si superan el limite configurado: "precio viejo".
    // Sin fecha no se marca (misma regla que Negocio.PedidoTextos.PrecioViejo).
    function diasDelPrecio(precio) { return precio ? diasDesde(precio.fecha) : null; }
    function precioEsViejo(precio) {
        var dias = diasDelPrecio(precio);
        return dias !== null && dias > DIAS_PRECIO_VIEJO;
    }
    function textoOrigenPrecio(precio) { // 'Proveedor X, compra del dd/mm/aaaa'
        return (precio.proveedor ? 'Proveedor ' + precio.proveedor + ', ' : '') + 'compra del ' + formatearFecha(precio.fecha);
    }
    // Por que un producto sin precio no tiene: o nunca se compro, o su ultima compra es tan vieja que el servidor no la usa.
    function textoSinPrecio(idCorte) {
        var fecha = preciosDescartados[idCorte];
        if (fecha) {
            return 'La última compra es del ' + formatearFecha(fecha) + (diasPrecioMaximo ? ' (hace más de ' + plural(diasPrecioMaximo, 'día', 'días') + ')' : '')
                + ': un precio tan viejo no se usa para estimar el importe';
        }
        return 'Sin compras registradas de este producto: no hay precio para estimar el importe';
    }

    function nombreSucursal(id) {
        return $('#chkSucursalExistencia_' + id).attr('data-nombre') || ('Sucursal ' + id);
    }

    function avisar(titulo, texto, icono) { pantalla.avisar(titulo, texto, icono); }

    // ---------- Cantidades ----------

    function cantidadDe(idCorte, idSuc) {
        var porSuc = cantidades[idCorte];
        return porSuc ? porSuc[idSuc] : undefined;
    }

    function fijarCantidad(idCorte, idSuc, valor) { // valor: numero (>= 0) o null para borrar
        if (valor === null || valor === undefined) {
            if (cantidades[idCorte]) {
                delete cantidades[idCorte][idSuc];
                if (Object.keys(cantidades[idCorte]).length === 0) delete cantidades[idCorte];
            }
            return;
        }
        if (!cantidades[idCorte]) cantidades[idCorte] = {};
        cantidades[idCorte][idSuc] = valor;
    }

    function totalProducto(idCorte) {
        var porSuc = cantidades[idCorte];
        if (!porSuc) return 0;
        var total = 0;
        Object.keys(porSuc).forEach(function (s) { if (porSuc[s] > 0) total += porSuc[s]; });
        return calculo.redondear3(total);
    }

    // Lineas del pedido = pares producto-sucursal con cantidad > 0 (lo que se guarda, se previsualiza y se cuenta).
    function lineasConCantidad() {
        var lineas = [];
        Object.keys(cantidades).forEach(function (idCorte) {
            Object.keys(cantidades[idCorte]).forEach(function (idSuc) {
                var q = cantidades[idCorte][idSuc];
                if (q > 0) lineas.push({ idCorte: parseInt(idCorte, 10), idSuc: parseInt(idSuc, 10), cantidad: q });
            });
        });
        return lineas;
    }

    function contarLineas() { return lineasConCantidad().length; }

    function contarProductos() {
        var n = 0;
        Object.keys(cantidades).forEach(function (idCorte) { if (totalProducto(idCorte) > 0) n++; });
        return n;
    }

    // ---------- Indice de la tabla (se rearma cuando el servidor la reemplaza) ----------

    function asegurarIndice() {
        var tbody = $contenedor.find('tbody')[0] || null;
        if (tbody !== null && tbody === indice.tbody) return;

        indice = { tbody: tbody, filas: {}, celdas: {}, sucursales: [] };
        if (!tbody) return;

        $contenedor.find('thead th.ex-grupo[data-id-sucursal]').each(function () {
            indice.sucursales.push({ id: parseInt(this.getAttribute('data-id-sucursal'), 10), nombre: $(this).find('.ex-grupo-nombre').text() });
        });

        $contenedor.find('tr.existencia-producto-row').each(function () {
            var tr = this;
            var idCorte = parseInt(tr.getAttribute('data-id-corte'), 10);
            var prod = catalogo[idCorte] || (catalogo[idCorte] = { stock: {}, punto: {} });
            prod.codigo = tr.getAttribute('data-cod') || '';
            prod.nombre = tr.getAttribute('data-nom') || '';
            prod.promedio = parseFloat(tr.getAttribute('data-prom')) || 0;
            prod.pesable = tr.getAttribute('data-pes') === '1';
            indice.filas[idCorte] = tr;

            $(tr).find('td.ex-col-estado[data-id-sucursal]').each(function () {
                var s = parseInt(this.getAttribute('data-id-sucursal'), 10);
                prod.stock[s] = parseFloat(this.getAttribute('data-stock')) || 0;
                prod.punto[s] = parseFloat(this.getAttribute('data-punto')) || 0;
            });

            $(tr).find('td.ex-pedir-cell').each(function () {
                var s = parseInt(this.getAttribute('data-id-sucursal'), 10);
                indice.celdas[idCorte + '_' + s] = {
                    idCorte: idCorte, idSuc: s, td: this,
                    input: this.querySelector('input.ex-in-pedir'),
                    eqEl: this.querySelector('.ex-ped-eq'),
                    hintEl: this.querySelector('.ex-ped-hint')
                };
            });
        });
    }

    function celdaDeInput(input) {
        asegurarIndice();
        return indice.celdas[input.getAttribute('data-id-corte') + '_' + input.getAttribute('data-id-sucursal')] || null;
    }

    // ---------- Sugerencias ----------

    function consumoDeCelda(idCorte, idSuc) {
        var porSuc = consumo[idCorte];
        var c = porSuc ? porSuc[idSuc] : null;
        var ventas = c ? c.ventas : 0, elab = c ? c.elaboracion : 0;
        return { ventas: ventas, elaboracion: elab, total: calculo.redondear3(ventas + elab) };
    }

    function resultadoDeCelda(idCorte, idSuc, modo) {
        var prod = catalogo[idCorte];
        if (!prod) return null;
        var c = consumoDeCelda(idCorte, idSuc);
        return { res: calculo.calcularSugerencia(prod.stock[idSuc] || 0, prod.punto[idSuc] || 0, c.total, modo || segun), consumo: c, prod: prod };
    }

    function iguales(a, b) { return a !== undefined && b !== undefined && Math.abs(a - b) < 0.0005; }

    // Texto del tooltip de una pista: el desglose del calculo, para que el usuario vea de donde sale el numero.
    function tituloPista(d, idSuc) {
        var desdeCorto = consumoDesde ? formatearFecha(consumoDesde).slice(0, 5) : '';
        if (consumoHasta) desdeCorto += ' hasta ' + formatearFecha(consumoHasta).slice(0, 5);
        var stockReal = d.prod.stock[idSuc] || 0;
        var stockTxt = stockReal < 0 ? 'stock 0 (el stock es negativo: ' + fmtCant3.format(stockReal) + ')' : 'stock ' + fmtCant3.format(d.res.stock);
        var porConsumo = 'Vendido ' + fmtCant3.format(d.consumo.ventas) + ' + usado en elaborados ' + fmtCant3.format(d.consumo.elaboracion)
            + ' desde ' + desdeCorto + ' = ' + fmtCant3.format(d.res.consumo) + ' − ' + stockTxt + ' = sugerido ' + fmtCant3.format(d.res.faltaPorConsumo);
        var hastaPunto = 'Punto de stock ' + fmtCant3.format(d.res.punto) + ' − ' + stockTxt + ' = ' + fmtCant3.format(d.res.faltaHastaPunto);
        if (segun === 'punto') return hastaPunto;
        if (segun === 'mayor') return 'El mayor entre: (1) ' + porConsumo + ' · (2) ' + hastaPunto;
        return porConsumo;
    }

    // ---------- Dibujo de una celda / fila / totales ----------

    function renderHint(celda, qty) {
        var hint = celda.hintEl;
        if (!hint) return;
        hint.textContent = '';
        if (!activo) return;

        var d = resultadoDeCelda(celda.idCorte, celda.idSuc);
        if (!d) return;

        // Las pistas muestran el numero tal como se va a cargar en la casilla (sin ceros de relleno): ocupan menos y no hay dos formatos.
        // Las partes de texto largas van en .ex-h-lbl: el diseno compacto las oculta (el tooltip y el Estado de la fila ya lo explican).
        if (d.res.sugerido > 0 && !iguales(qty, d.res.sugerido)) {
            var pista = el('button', 'ex-pista', 'sug. ' + fmtInput.format(d.res.sugerido));
            pista.type = 'button';
            pista.setAttribute('data-valor', String(d.res.sugerido));
            pista.title = tituloPista(d, celda.idSuc) + ' (click para cargarla)';
            hint.appendChild(pista);
        }

        // Con "Hasta el punto" la pista ya es esa cantidad: el chip sobraria.
        if (d.res.bajoPunto && segun !== 'punto' && !iguales(qty, d.res.faltaHastaPunto)) {
            var chip = el('button', 'ex-chip-punto', '⚠ ');
            chip.appendChild(el('span', 'ex-h-lbl', 'bajo pto · '));
            chip.appendChild(document.createTextNode('+' + fmtInput.format(d.res.faltaHastaPunto)));
            chip.type = 'button';
            chip.setAttribute('data-valor', String(d.res.faltaHastaPunto));
            chip.title = 'Está bajo el punto de stock: punto ' + fmtCant3.format(d.res.punto) + ' − stock ' + fmtCant3.format(d.res.stock)
                + ' = ' + fmtCant3.format(d.res.faltaHastaPunto) + ' (click para completar hasta el punto)';
            hint.appendChild(chip);
        }

        if (d.res.stockSuficiente && consumoEstado === 'ok') {
            var ok = el('span', 'ex-suficiente', '✔');
            ok.appendChild(el('span', 'ex-h-lbl', ' stock suficiente'));
            ok.title = 'El stock (' + fmtCant3.format(d.res.stock) + ') cubre el consumo del período (' + fmtCant3.format(d.res.consumo)
                + ') y el punto de stock (' + fmtCant3.format(d.res.punto) + ')';
            hint.appendChild(ok);
        }
    }

    function renderCelda(celda) {
        var qty = cantidadDe(celda.idCorte, celda.idSuc);
        var prod = catalogo[celda.idCorte];
        if (!celda.input) return;

        // El input que se esta tipeando no se pisa (se perderia el cursor), ni un texto invalido que el usuario todavia tiene que corregir.
        if (document.activeElement !== celda.input && !$(celda.input).hasClass('ex-in-invalido')) {
            celda.input.value = qty === undefined ? '' : fmtInput.format(qty);
            $(celda.input).removeClass('ex-in-invalido').removeAttr('title');
        }
        $(celda.input).toggleClass('ex-in-cargada', qty > 0);

        var eq = prod ? calculo.equivalenteUnidades(qty || 0, prod.promedio, prod.pesable) : null;
        celda.eqEl.textContent = eq === null ? '' : '≈ ' + fmtUn.format(eq) + ' un.';
        renderHint(celda, qty);
    }

    function precioDe(idCorte) { return precios[idCorte] || null; }

    function importeDe(idCorte, total) {
        var p = precioDe(idCorte);
        return p ? calculo.importeLinea(total, p.precio) : null;
    }

    // Total pedido, precio de la ultima compra e importe de una fila. Tambien deja los data- que usa el orden de la tabla.
    function actualizarFila(idCorte) {
        asegurarIndice();
        var tr = indice.filas[idCorte];
        if (!tr) return;

        var prod = catalogo[idCorte];
        var total = totalProducto(idCorte);
        tr.setAttribute('data-tp', String(total));
        indice.sucursales.forEach(function (sc) {
            var q = cantidadDe(idCorte, sc.id);
            tr.setAttribute('data-p-' + sc.id, String(q > 0 ? q : 0));
        });

        var $tp = $(tr).find('.ex-tp-cant');
        $tp.text(total > 0 ? fmtCant3.format(total) : '');
        var eq = prod ? calculo.equivalenteUnidades(total, prod.promedio, prod.pesable) : null;
        $(tr).find('.ex-tp-eq').text(eq === null ? '' : '≈ ' + fmtUn.format(eq) + ' un.');

        var precio = precioDe(idCorte);
        var $precio = $(tr).find('.ex-precio-cell').empty().removeAttr('title');
        if (precio) {
            $precio.text(fmtPlata.format(precio.precio));
            var tituloPrecio = textoOrigenPrecio(precio);
            if (precioEsViejo(precio)) {
                // Precio viejo: marca visible y el motivo en el tooltip (el limite sale de Pedidos:DiasPrecioViejo).
                $precio.append(el('span', 'text-warning ml-1', '⚠'));
                tituloPrecio += ' · hace ' + plural(diasDelPrecio(precio), 'día', 'días') + ': el precio puede estar desactualizado';
            }
            $precio.attr('title', tituloPrecio);
        } else if (preciosEstado === 'ok') {
            $precio.append(el('span', 'ex-sinprecio', '—')).attr('title', textoSinPrecio(idCorte));
        }

        var importe = importeDe(idCorte, total);
        tr.setAttribute('data-imp', String(importe === null ? 0 : importe));
        $(tr).find('.ex-importe-cell').text(importe === null ? '' : fmtPlata.format(importe));
    }

    // Contador, total estimado y avisos de la barra del pedido.
    function actualizarTotales() {
        var productos = 0, lineas = contarLineas(), centavos = 0, sinPrecio = 0;
        Object.keys(cantidades).forEach(function (idCorte) {
            var total = totalProducto(idCorte);
            if (total <= 0) return;
            productos++;
            var imp = importeDe(idCorte, total);
            if (imp === null) sinPrecio++; else centavos += Math.round(imp * 100);
        });

        $lblContador.text('(' + plural(productos, 'producto', 'productos') + ')');
        $btnGuardar.prop('disabled', productos === 0);
        $labelToggle.find('.ex-badge-pedido').remove();
        if (productos > 0) $labelToggle.append(el('span', 'badge badge-light ml-1 ex-badge-pedido', String(productos)));

        var partes = [];
        if (productos > 0) {
            if (preciosEstado === 'ok') {
                // Con productos sin precio el total esta incompleto: se rotula "parcial" (no un numero que parezca completo).
                partes.push((sinPrecio > 0 ? TXT_TOTAL_PARCIAL : TXT_TOTAL) + ' ' + fmtPlata.format(centavos / 100));
                if (sinPrecio > 0) partes.push(plural(sinPrecio, 'producto sin precio de compra (no suma)', 'productos sin precio de compra (no suman)'));
            } else if (preciosEstado === 'cargando') {
                partes.push('Calculando importes…');
            } else {
                partes.push('Importes no disponibles');
            }
            partes.push(plural(lineas, 'línea', 'líneas'));
            if (cfgBorrador.habilitado && lineas > (cfgBorrador.maxLineas || 0)) {
                partes.push('⚠ supera las ' + cfgBorrador.maxLineas + ' líneas: el pedido ya no se resguarda solo');
            }
        }
        $lblResumenTotal.text(partes.join(' · '));

        // Resumen de la barra colapsada: cantidad de productos y total (lo primero que dice el resumen de arriba) y su propio Guardar.
        var textoMini = productos === 0 ? 'Pedido sin cantidades cargadas' : plural(productos, 'producto', 'productos') + (partes.length ? ' · ' + partes[0] : '');
        $lblMini.text(textoMini).attr('title', textoMini);
        $btnGuardarMini.prop('disabled', productos === 0);
    }

    // Colapsa o expande la barra del pedido. Colapsada queda UNA linea (boton + resumen + Guardar) y no ocupa el alto de la pantalla; la tabla
    // recupera ese espacio. recordar = guardar la eleccion en este navegador (al abrir el pedido se aplica sin volver a guardar).
    function aplicarColapso(colapsar, recordar) {
        $barra.toggleClass('ex-colapsada', colapsar);
        $btnColapsar.attr('aria-expanded', colapsar ? 'false' : 'true')
            .attr('title', colapsar ? 'Mostrar las opciones del pedido' : 'Ocultar las opciones del pedido (queda una sola línea con el resumen y Guardar)')
            .find('i').toggleClass('fa-chevron-up', !colapsar).toggleClass('fa-chevron-down', colapsar);
        if (recordar) guardarLS(CLAVE_BARRA_COLAPSADA, colapsar ? '1' : '0');
        pantalla.ajustarAlto();
    }

    // Resumen de las celdas que se ven: cuantas estan bajo el punto de stock (por sucursal) y cuantas tienen stock suficiente.
    function actualizarResumenCeldas() {
        if (!activo) return;
        asegurarIndice();

        var bajo = 0, suficientes = 0, porSuc = {};
        $contenedor.find('tr.existencia-producto-row:not(.d-none)').each(function () {
            var idCorte = parseInt(this.getAttribute('data-id-corte'), 10);
            indice.sucursales.forEach(function (sc) {
                var d = resultadoDeCelda(idCorte, sc.id);
                if (!d) return;
                if (d.res.bajoPunto) { bajo++; porSuc[sc.id] = (porSuc[sc.id] || 0) + 1; }
                if (d.res.stockSuficiente) suficientes++;
            });
        });

        var partes = [];
        if (bajo > 0) {
            var detalle = indice.sucursales.filter(function (sc) { return porSuc[sc.id]; })
                .map(function (sc) { return sc.nombre + ' ' + porSuc[sc.id]; }).join(', ');
            partes.push('⚠ ' + plural(bajo, 'celda bajo el punto de stock', 'celdas bajo el punto de stock') + (indice.sucursales.length > 1 ? ' (' + detalle + ')' : ''));
        }
        if (consumoEstado === 'ok' && suficientes > 0) partes.push('✔ ' + suficientes + ' con stock suficiente según el consumo');
        $lblResumenCeldas.text(partes.join(' · '));

        $lblCantPunto.text('(' + bajo + ')');
        $btnPunto.prop('disabled', bajo === 0);
    }

    function renderTodo() {
        asegurarIndice();
        Object.keys(indice.celdas).forEach(function (k) { renderCelda(indice.celdas[k]); });
        Object.keys(indice.filas).forEach(function (id) { actualizarFila(parseInt(id, 10)); });
        actualizarTotales();
        actualizarResumenCeldas();
        sincronizarProteccionSalida();
    }

    // Refresca lo que cambia al tocar UNA celda (cantidad cargada o sugerencia aceptada).
    function refrescarCelda(celda) {
        renderCelda(celda);
        actualizarFila(celda.idCorte);
        actualizarTotales();
        actualizarResumenCeldas();
        alCambiarPedido();
    }

    function alCambiarPedido() {
        sincronizarProteccionSalida();
        if (borrador) borrador.programarGuardado();
    }

    // Con cantidades sin guardar se activa la proteccion de salida del layout (cerrar la pestana o navegar pide confirmacion).
    function sincronizarProteccionSalida() {
        var hay = contarLineas() > 0;
        if (hay && !proteccionActiva && typeof window.activarProteccionSalida === 'function') {
            window.activarProteccionSalida();
            proteccionActiva = true;
        } else if (!hay && proteccionActiva && typeof window.desactivarProteccionSalida === 'function') {
            window.desactivarProteccionSalida();
            proteccionActiva = false;
        }
    }

    // ---------- Leyenda de la sugerencia ----------

    // Cierra la leyenda con de donde salen los importes (frase unica de Negocio.PedidoTextos), se haya podido calcular el consumo o no:
    // el total estimado se ve siempre y siempre tiene que ir acompanado de esa aclaracion.
    function agregarTextoPrecios() {
        if (preciosEstado === 'ok') {
            $leyenda.append(document.createTextNode(' · '));
            $leyenda.append(el('span', null, TXT_ACLARACION));
        } else {
            $leyenda.append(document.createTextNode(' · ' + (preciosEstado === 'cargando' ? 'cargando precios…' : 'sin precios de compra')));
        }
    }

    function renderLeyenda() {
        $leyenda.empty();
        if (consumoEstado === 'cargando' || consumoEstado === 'sin') {
            $leyenda.text('Calculando las sugerencias…');
            agregarTextoPrecios();
            return;
        }
        if (consumoEstado !== 'ok') {
            $leyenda.text('No se pudo calcular el consumo del período' + (mensajeConsumo ? ' (' + mensajeConsumo + ')' : '')
                + ': solo se ofrece completar hasta el punto de stock. Podés cargar las cantidades a mano.');
            agregarTextoPrecios();
            return;
        }
        // Sin fecha hasta: "desde 20/09/2026 (30 días)" = hasta hoy. Con fecha hasta: "desde 01/09/2026 hasta 30/09/2026 (29 días)".
        var dias = diasDesde(consumoDesde, consumoHasta);
        $leyenda.append(document.createTextNode('Sugerencia = ventas + uso en elaborados desde '));
        $leyenda.append(el('b', null, formatearFecha(consumoDesde)));
        if (consumoHasta) {
            $leyenda.append(document.createTextNode(' hasta '));
            $leyenda.append(el('b', null, formatearFecha(consumoHasta)));
        }
        $leyenda.append(document.createTextNode((dias !== null ? ' (' + plural(dias, 'día', 'días') + ')' : '') + ', menos el stock actual'));
        agregarTextoPrecios();
    }

    // ---------- Carga de datos (consumo del periodo y precios) ----------

    function cargarConsumo() {
        var desde = pantalla.periodoDesde();
        var seq = ++secConsumo;
        if (!desde) {
            consumoEstado = 'error';
            mensajeConsumo = 'elegí la fecha de «Período desde»';
            renderLeyenda();
            renderTodo();
            proponerAnchoSiCorresponde();
            return;
        }

        consumoEstado = 'cargando';
        renderLeyenda();
        // Sin fecha hasta no se manda el parametro: el servidor cuenta hasta ahora.
        var datos = { desde: desde };
        var hasta = pantalla.periodoHasta();
        if (hasta) datos.hasta = hasta;
        $.ajax({ url: cfg.consumoUrl, type: 'GET', data: datos }).done(function (resp) {
            if (seq !== secConsumo) return; // llego una respuesta vieja (se cambio la fecha mientras tanto)
            if (resp && resp.ok) {
                consumo = {};
                (resp.items || []).forEach(function (it) {
                    (consumo[it.idCorte] || (consumo[it.idCorte] = {}))[it.idSucursal] = { ventas: it.ventas || 0, elaboracion: it.elaboracion || 0 };
                });
                consumoDesde = resp.desde || desde;
                consumoHasta = resp.hasta || '';
                consumoEstado = 'ok';
                mensajeConsumo = '';
            } else {
                consumo = {};
                consumoEstado = resp && resp.noDisponible ? 'nodisponible' : 'error';
                mensajeConsumo = (resp && resp.mensaje) || '';
            }
        }).fail(function () {
            if (seq !== secConsumo) return;
            consumo = {};
            consumoEstado = 'error';
            mensajeConsumo = 'sin respuesta del servidor';
        }).always(function () {
            if (seq !== secConsumo) return;
            renderLeyenda();
            renderTodo();
            proponerAnchoSiCorresponde();
        });
    }

    // Una sola vez por encendido, ya con las sugerencias dibujadas (definen el ancho de "A pedir"): en la ventana independiente, si hay
    // columnas fuera de vista, se ofrece ensancharla (ExistenciaPantalla.proponerEnsancharVentana decide si hace falta).
    function proponerAnchoSiCorresponde() {
        if (!proponerAnchoPendiente) return;
        proponerAnchoPendiente = false;
        window.setTimeout(pantalla.proponerEnsancharVentana, 0);
    }

    // Precio de la ultima compra de cada producto: se pide una vez al encender el pedido (no carga la consulta a quien solo mira stock).
    function cargarPrecios() {
        var seq = ++secPrecios;
        if (!cfg.preciosUrl) {
            preciosEstado = 'nodisponible';
            return;
        }

        preciosEstado = 'cargando';
        $.ajax({ url: cfg.preciosUrl, type: 'GET' }).done(function (resp) {
            if (seq !== secPrecios) return;
            if (resp && resp.ok) {
                precios = {};
                preciosDescartados = {};
                diasPrecioMaximo = resp.diasMaximo || 0;
                (resp.items || []).forEach(function (it) {
                    if (it.precio > 0) precios[it.idCorte] = { precio: it.precio, proveedor: it.proveedor || '', fecha: it.fecha || '' };
                });
                (resp.descartados || []).forEach(function (it) { preciosDescartados[it.idCorte] = it.fecha || ''; });
                preciosEstado = 'ok';
            } else {
                precios = {};
                preciosDescartados = {};
                preciosEstado = 'error';
            }
        }).fail(function () {
            if (seq !== secPrecios) return;
            precios = {};
            preciosDescartados = {};
            preciosEstado = 'error';
        }).always(function () {
            if (seq !== secPrecios) return;
            renderLeyenda();
            renderTodo();
        });
    }

    // ---------- Encender / apagar ----------

    function encender() {
        if (!cfg.disponible) {
            $switch.prop('checked', false);
            avisar('No disponible', 'Ingresar pedido solo está disponible cuando el sistema usa Postgres.', 'info');
            return;
        }
        activo = true;
        $barra.removeClass('d-none');
        aplicarColapso(leerLS(CLAVE_BARRA_COLAPSADA, '0') === '1', false); // como la dejo la ultima vez
        proponerAnchoPendiente = true;
        pantalla.establecerPedidoActivo(true); // muestra las columnas, pausa la autoactualizacion y ajusta el alto de la tabla
        asegurarIndice();
        renderTodo();
        cargarConsumo();
        cargarPrecios();
    }

    function apagar() {
        activo = false;
        clearTimeout(timerPeriodo);
        $barra.addClass('d-none');
        pantalla.establecerPedidoActivo(false); // reanuda la autoactualizacion
        // Las cantidades cargadas se conservan (siguen en el borrador y en la proteccion de salida): vuelven al encenderlo de nuevo.
        actualizarTotales();
    }

    // ---------- Aceptar sugerencias ----------

    // Celdas candidatas: filas VISIBLES (con el filtro y el orden actuales) de la sucursal pedida (null = todas las mostradas).
    function celdasVisibles(idSucursal) {
        asegurarIndice();
        var lista = [];
        $contenedor.find('tr.existencia-producto-row:not(.d-none)').each(function () {
            var idCorte = parseInt(this.getAttribute('data-id-corte'), 10);
            indice.sucursales.forEach(function (sc) {
                if (idSucursal && sc.id !== idSucursal) return;
                var celda = indice.celdas[idCorte + '_' + sc.id];
                if (celda) lista.push(celda);
            });
        });
        return lista;
    }

    function aceptarSugerencias(idSucursal, soloBajoPunto) {
        var modo = soloBajoPunto ? 'punto' : segun;
        var candidatas = [];
        celdasVisibles(idSucursal).forEach(function (celda) {
            var d = resultadoDeCelda(celda.idCorte, celda.idSuc, modo);
            if (d && d.res.sugerido > 0) candidatas.push({ celda: celda, valor: d.res.sugerido });
        });

        if (candidatas.length === 0) {
            avisar('Sin sugerencias', soloBajoPunto
                ? 'No hay celdas bajo el punto de stock entre los productos que se ven.'
                : 'No hay sugerencias para cargar entre los productos que se ven: el stock alcanza o no hubo consumo en el período.', 'info');
            return;
        }

        function aplicar(reemplazar) {
            var cargadas = 0;
            candidatas.forEach(function (c) {
                var ocupada = cantidadDe(c.celda.idCorte, c.celda.idSuc) !== undefined;
                if (ocupada && !reemplazar) return;
                fijarCantidad(c.celda.idCorte, c.celda.idSuc, c.valor);
                cargadas++;
            });
            renderTodo();
            alCambiarPedido();
            mostrarMensajeTemporal(cargadas === 0 ? 'No se cargó nada.' : 'Se cargaron ' + plural(cargadas, 'sugerencia', 'sugerencias') + '.');
        }

        var conCantidad = candidatas.filter(function (c) { return cantidadDe(c.celda.idCorte, c.celda.idSuc) !== undefined; }).length;
        if (conCantidad === 0 || !(window.Swal && typeof window.Swal.fire === 'function')) {
            aplicar(false);
            return;
        }

        window.Swal.fire({
            icon: 'question',
            title: 'Hay celdas con cantidad ya cargada',
            text: conCantidad + ' de las ' + candidatas.length + ' celdas con sugerencia ya tienen una cantidad. ¿Qué hacemos con ellas?',
            showDenyButton: true,
            showCancelButton: true,
            confirmButtonText: 'Solo vacías',
            denyButtonText: 'Reemplazar también las cargadas',
            cancelButtonText: 'Cancelar'
        }).then(function (r) {
            if (r.isConfirmed) aplicar(false);
            else if (r.isDenied) aplicar(true);
        });
    }

    // Mensaje corto y temporal en la linea de resumen (no usa Swal: no interrumpe).
    var timerMensaje = null;
    function mostrarMensajeTemporal(texto) {
        var $msg = $lblResumenCeldas;
        var anterior = $msg.text();
        $msg.text(texto);
        clearTimeout(timerMensaje);
        timerMensaje = setTimeout(function () { actualizarResumenCeldas(); if ($msg.text() === texto) $msg.text(anterior); }, 3500);
    }

    function limpiarPedido() {
        if (contarLineas() === 0 && Object.keys(cantidades).length === 0) return;
        function limpiar() {
            cantidades = {};
            renderTodo();
            alCambiarPedido();
        }
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({
                icon: 'warning', title: 'Borrar el pedido',
                text: 'Se borran todas las cantidades cargadas (' + plural(contarProductos(), 'producto', 'productos') + '). No se puede deshacer.',
                showCancelButton: true, confirmButtonText: 'Sí, borrar', cancelButtonText: 'Cancelar'
            }).then(function (r) { if (r.isConfirmed) limpiar(); });
        } else {
            limpiar();
        }
    }

    // ---------- Teclado ----------

    // Enter baja por la misma sucursal entre las filas visibles y en el orden actual; al final pasa a la sucursal siguiente.
    // Shift+Enter sube. En la ultima fila de la ultima sucursal (o la primera de la primera) no hace nada.
    function moverFoco(input, delta) {
        asegurarIndice();
        var filasVisibles = $contenedor.find('tr.existencia-producto-row:not(.d-none)').map(function () {
            return parseInt(this.getAttribute('data-id-corte'), 10);
        }).get();
        if (filasVisibles.length === 0) return;

        var idCorte = parseInt(input.getAttribute('data-id-corte'), 10);
        var idSuc = parseInt(input.getAttribute('data-id-sucursal'), 10);
        var posFila = filasVisibles.indexOf(idCorte);
        var posSuc = -1;
        indice.sucursales.forEach(function (sc, i) { if (sc.id === idSuc) posSuc = i; });
        if (posFila < 0 || posSuc < 0) return;

        var nuevaFila = posFila + delta;
        var nuevaSuc = posSuc;
        if (nuevaFila >= filasVisibles.length) { nuevaSuc = posSuc + 1; nuevaFila = 0; }
        else if (nuevaFila < 0) { nuevaSuc = posSuc - 1; nuevaFila = filasVisibles.length - 1; }
        if (nuevaSuc < 0 || nuevaSuc >= indice.sucursales.length) return;

        var destino = indice.celdas[filasVisibles[nuevaFila] + '_' + indice.sucursales[nuevaSuc].id];
        if (destino && destino.input) destino.input.focus();
    }

    // ---------- Vista preliminar (solo los productos con cantidad) ----------

    function productosConCantidad() {
        var items = [];
        Object.keys(cantidades).forEach(function (k) {
            var idCorte = parseInt(k, 10);
            var total = totalProducto(idCorte);
            if (total <= 0) return;
            var prod = catalogo[idCorte] || {};
            var porSuc = {}, suficiente = {};
            Object.keys(cantidades[idCorte]).forEach(function (s) {
                var q = cantidades[idCorte][s];
                if (!(q > 0)) return;
                porSuc[s] = q;
                var d = resultadoDeCelda(idCorte, parseInt(s, 10));
                // «Stock suficiente» solo se afirma con el consumo del periodo cargado (si no, no hay con que compararlo).
                suficiente[s] = !!(d && consumoEstado === 'ok' && d.res.stockSuficiente);
            });
            var precio = precioDe(idCorte);
            items.push({ idCorte: idCorte, codigo: prod.codigo || '', nombre: prod.nombre || ('Producto ' + idCorte), total: total, porSuc: porSuc, suficiente: suficiente, precio: precio, importe: importeDe(idCorte, total), prod: prod });
        });
        items.sort(function (a, b) {
            var na = parseFloat(a.codigo), nb = parseFloat(b.codigo);
            if (isFinite(na) && isFinite(nb) && na !== nb) return na - nb;
            return String(a.codigo).localeCompare(String(b.codigo), 'es', { numeric: true });
        });
        return items;
    }

    function abrirVistaPrevia() {
        var items = productosConCantidad();
        if (items.length === 0) {
            avisar('Pedido vacío', 'No hay ningún producto con cantidad cargada. Cargá al menos una cantidad para revisar el pedido.', 'info');
            return;
        }

        // Columnas de sucursales: las que tienen alguna cantidad, en el orden de la tabla y luego las demas por nombre.
        var usadas = {};
        items.forEach(function (it) { Object.keys(it.porSuc).forEach(function (s) { usadas[s] = true; }); });
        var ordenTabla = indice.sucursales.map(function (sc) { return String(sc.id); });
        var sucs = Object.keys(usadas).sort(function (a, b) {
            var ia = ordenTabla.indexOf(a), ib = ordenTabla.indexOf(b);
            if (ia >= 0 && ib >= 0) return ia - ib;
            if (ia >= 0) return -1;
            if (ib >= 0) return 1;
            return nombreSucursal(a).localeCompare(nombreSucursal(b), 'es');
        });

        var tabla = el('table', 'table table-sm table-bordered table-hover ex-previa-tabla');
        var thead = el('thead');
        var trh = el('tr');
        [['Cód.', ''], ['Producto', '']].forEach(function (t) { trh.appendChild(el('th', null, t[0])); });
        sucs.forEach(function (s) { trh.appendChild(el('th', 'text-right', nombreSucursal(s))); });
        if (sucs.length > 1) trh.appendChild(el('th', 'text-right', 'Total pedido'));
        // Precio e importe solo si el usuario encendio "Ver precios" (por defecto la vista preliminar no los muestra).
        var verPrecios = $switchPrecios.is(':checked');
        if (verPrecios) {
            trh.appendChild(el('th', 'text-right', '$ últ. compra'));
            trh.appendChild(el('th', 'text-right', 'Importe est.'));
        }
        thead.appendChild(trh);
        tabla.appendChild(thead);

        var tbody = el('tbody');
        var centavos = 0, sinPrecio = 0, conAviso = 0, conPrecioViejo = 0;
        items.forEach(function (it) {
            var tr = el('tr');
            tr.appendChild(el('td', 'text-nowrap', it.codigo));
            tr.appendChild(el('td', null, it.nombre));
            sucs.forEach(function (s) {
                var td = el('td', 'text-right text-nowrap');
                if (it.porSuc[s] > 0) {
                    td.textContent = fmtCant3.format(it.porSuc[s]);
                    if (it.suficiente[s]) {
                        var aviso = el('span', 'ex-previa-aviso', '⚠');
                        aviso.title = 'Stock suficiente según el consumo del período: el stock de esta sucursal ya cubre lo que se consumió';
                        td.appendChild(aviso);
                        conAviso++;
                    }
                }
                tr.appendChild(td);
            });
            if (sucs.length > 1) {
                var tdTotal = el('td', 'text-right text-nowrap font-weight-bold', fmtCant3.format(it.total));
                var eq = calculo.equivalenteUnidades(it.total, it.prod.promedio, it.prod.pesable);
                if (eq !== null) tdTotal.appendChild(el('div', 'small text-muted font-weight-normal', '≈ ' + fmtUn.format(eq) + ' un.'));
                tr.appendChild(tdTotal);
            }
            // Los totales del pie se calculan siempre (con o sin las columnas a la vista).
            var viejo = it.precio ? precioEsViejo(it.precio) : false;
            if (viejo) conPrecioViejo++;
            if (it.importe === null) sinPrecio++; else centavos += Math.round(it.importe * 100);

            if (verPrecios) {
                var tdPrecio = el('td', 'text-right text-nowrap', it.precio ? fmtPlata.format(it.precio.precio) : '');
                if (it.precio) {
                    // De quien y de cuando es el precio, a la vista (no solo en el tooltip); en naranja con los dias si es viejo.
                    tdPrecio.title = textoOrigenPrecio(it.precio);
                    var origen = (it.precio.proveedor ? it.precio.proveedor + ' · ' : '') + formatearFecha(it.precio.fecha);
                    if (viejo) origen = '⚠ ' + origen + ' · hace ' + plural(diasDelPrecio(it.precio), 'día', 'días');
                    if (origen) tdPrecio.appendChild(el('div', 'small ' + (viejo ? 'text-warning' : 'text-muted'), origen));
                    if (viejo) tdPrecio.title += ' · el precio puede estar desactualizado';
                } else {
                    var sinPrecioSpan = el('span', 'text-muted small', preciosEstado === 'ok' ? 'sin precio' : '—');
                    if (preciosEstado === 'ok') sinPrecioSpan.title = textoSinPrecio(it.idCorte);
                    tdPrecio.appendChild(sinPrecioSpan);
                }
                tr.appendChild(tdPrecio);
                tr.appendChild(el('td', 'text-right text-nowrap', it.importe === null ? '' : fmtPlata.format(it.importe)));
            }
            tbody.appendChild(tr);
        });
        tabla.appendChild(tbody);

        var envoltorio = el('div', 'table-responsive');
        envoltorio.appendChild(tabla);
        $cuerpoPrevia.empty().append(envoltorio);
        // De donde sale el importe (frase unica de Negocio.PedidoTextos) y, si hay, la leyenda de la marca de precio viejo.
        // Solo con las columnas de precio a la vista: la aclaracion habla de ellas.
        if (verPrecios && preciosEstado === 'ok') {
            $cuerpoPrevia.append(el('div', 'small text-muted mt-2', TXT_ACLARACION
                + (conPrecioViejo > 0 ? ' ' + (cfgPrecios.avisoPrecioViejo || ('Los precios marcados son de hace más de ' + plural(DIAS_PRECIO_VIEJO, 'día', 'días') + '.')) : '')));
        }

        $resumenPrevia.empty();
        $resumenPrevia.append(el('b', null, plural(items.length, 'producto', 'productos')));
        if (preciosEstado === 'ok') {
            $resumenPrevia.append(document.createTextNode(' · ' + (sinPrecio > 0 ? TXT_TOTAL_PARCIAL : TXT_TOTAL) + ' '));
            $resumenPrevia.append(el('b', null, fmtPlata.format(centavos / 100)));
            if (sinPrecio > 0) $resumenPrevia.append(document.createTextNode(' · ' + plural(sinPrecio, 'producto sin precio de compra (no suma)', 'productos sin precio de compra (no suman)')));
        } else {
            $resumenPrevia.append(document.createTextNode(' · Importes no disponibles'));
        }
        if (conAviso > 0) $resumenPrevia.append(document.createTextNode(' · ⚠ ' + plural(conAviso, 'línea con stock suficiente', 'líneas con stock suficiente') + ' según el consumo del período'));

        $modalPrevia.modal('show');
    }

    // ---------- Borrador en servidor ----------

    function construirSnapshot() {
        var lineas = lineasConCantidad();
        var cant = {}, productos = {}, sucursales = {};
        lineas.forEach(function (l) {
            (cant[l.idCorte] || (cant[l.idCorte] = {}))[l.idSuc] = l.cantidad;
            var prod = catalogo[l.idCorte] || {};
            productos[l.idCorte] = [prod.codigo || '', prod.nombre || ''];
            sucursales[l.idSuc] = nombreSucursal(l.idSuc);
        });
        // sugerirSegun y la consulta (sucursales y filtros de la busqueda) permiten recuperar el mismo contexto. Las sugerencias NO se guardan.
        return { v: 1, consulta: pantalla.consultaAplicada() || {}, sugerirSegun: segun, sucursales: sucursales, productos: productos, cantidades: cant };
    }

    function construirResumen() {
        var lineas = lineasConCantidad();
        var nombres = {};
        lineas.forEach(function (l) { nombres[nombreSucursal(l.idSuc)] = true; });
        return 'Pedido: ' + Object.keys(nombres).join(', ') + ' — ' + plural(contarProductos(), 'producto', 'productos') + ', ' + plural(lineas.length, 'línea', 'líneas');
    }

    // Detalle de un borrador para el modal "Borradores sin guardar": una fila por producto y sucursal.
    function renderizarLineasBorrador(payload) {
        var filas = [];
        if (!payload || !payload.cantidades) return filas;
        Object.keys(payload.cantidades).forEach(function (idCorte) {
            var prod = (payload.productos && payload.productos[idCorte]) || ['', 'Producto ' + idCorte];
            Object.keys(payload.cantidades[idCorte]).forEach(function (idSuc) {
                var nombreSuc = (payload.sucursales && payload.sucursales[idSuc]) || ('Sucursal ' + idSuc);
                filas.push({ codigo: prod[0], producto: prod[1] + ' · ' + nombreSuc, cantidad: fmtCant3.format(payload.cantidades[idCorte][idSuc]) });
            });
        });
        return filas;
    }

    // Carga un borrador recuperado: vuelve a poner las cantidades, enciende el pedido y repite la busqueda con las mismas sucursales y filtros.
    function aplicarSnapshot(payload) {
        if (!payload || payload.v !== 1 || !payload.cantidades) {
            avisar('Borrador no válido', 'El borrador tiene un formato que esta versión no reconoce. No se cargó nada.', 'error');
            return;
        }

        cantidades = {};
        Object.keys(payload.cantidades).forEach(function (idCorte) {
            Object.keys(payload.cantidades[idCorte]).forEach(function (idSuc) {
                var q = Number(payload.cantidades[idCorte][idSuc]);
                if (isFinite(q) && q > 0 && q <= calculo.MAX_CANTIDAD) fijarCantidad(parseInt(idCorte, 10), parseInt(idSuc, 10), calculo.redondear3(q));
            });
        });
        // Nombres de productos que quiza no vuelvan en la busqueda: se conservan para la vista previa y para el aviso.
        Object.keys(payload.productos || {}).forEach(function (idCorte) {
            var id = parseInt(idCorte, 10);
            var prod = catalogo[id] || (catalogo[id] = { stock: {}, punto: {}, promedio: 0, pesable: false });
            if (!prod.nombre) { prod.codigo = payload.productos[idCorte][0]; prod.nombre = payload.productos[idCorte][1]; }
        });
        if (payload.sugerirSegun) { segun = calculo.normalizarSegun(payload.sugerirSegun); $selSegun.val(segun); }

        restauracionPendiente = true;
        if (!activo) {
            $switch.prop('checked', true);
            encender();
        }
        pantalla.restaurarConsulta(payload.consulta);
        renderTodo();
        alCambiarPedido();
    }

    // Tras cargar un borrador y volver la tabla: avisa las lineas cuyo producto o sucursal ya no aparece (no se pierden en silencio).
    function avisarLineasNoEncontradas() {
        asegurarIndice();
        var sinFila = 0;
        lineasConCantidad().forEach(function (l) { if (!indice.celdas[l.idCorte + '_' + l.idSuc]) sinFila++; });
        if (sinFila === 0) return;
        avisar('Líneas no encontradas', plural(sinFila, 'línea del borrador no aparece', 'líneas del borrador no aparecen') + ' en esta búsqueda (el producto o la sucursal ya no se muestra). '
            + 'Se conservan en el pedido y se ven al volver a mostrar esos productos o sucursales; también entran en «Guardar pedido».', 'warning');
    }

    function crearBorrador() {
        if (!cfgBorrador.habilitado || !window.BorradorGenerico) return null;
        return window.BorradorGenerico.crear({
            modulo: cfgBorrador.modulo,
            habilitado: true,
            latidoSegundos: cfgBorrador.latidoSegundos,
            urls: cfgBorrador.urls,
            obtenerIdSucursal: function () { return cfgBorrador.idSucursal || 0; },
            obtenerIdRegistro: function () { return null; },
            obtenerIdOperador: function () { return cfgBorrador.idOperador || 0; },
            obtenerNombreOperador: function () { return cfgBorrador.nombreOperador || ''; },
            obtenerSnapshot: construirSnapshot,
            obtenerCantLineas: contarLineas,
            obtenerResumen: construirResumen,
            hayFormularioCargado: function () { return contarLineas() > 0; },
            finalizando: function () { return guardando; },
            aplicarPayload: aplicarSnapshot,
            renderizarLineas: renderizarLineasBorrador
        });
    }

    // ---------- API para el guardado definitivo (Fase 3, modulo Pedidos) ----------

    function obtenerPedido() {
        asegurarIndice();
        var lineas = lineasConCantidad().map(function (l) {
            var prod = catalogo[l.idCorte] || {};
            var precio = precioDe(l.idCorte);
            return {
                idCorte: l.idCorte, idSucursal: l.idSuc, cantidad: l.cantidad,
                codigo: prod.codigo || '', producto: prod.nombre || '', sucursal: nombreSucursal(l.idSuc),
                pesable: !!prod.pesable, promedio: prod.promedio || 0,
                stock: prod.stock ? (prod.stock[l.idSuc] || 0) : 0, punto: prod.punto ? (prod.punto[l.idSuc] || 0) : 0,
                precio: precio ? precio.precio : null, proveedor: precio ? precio.proveedor : null, fechaPrecio: precio ? precio.fecha : null
            };
        });
        return { clientId: clientIdPedido(), lineas: lineas };
    }

    // Id del pedido en curso (idempotencia de Pedidos/Guardar: reintentar un guardado no duplica). Es el del borrador en servidor
    // cuando esta habilitado; si no, uno propio que dura hasta que el pedido se guarda.
    function clientIdPedido() {
        var id = borrador ? borrador.getClientId() : null;
        if (id) return id;
        if (!clientIdLocal) {
            clientIdLocal = (window.crypto && typeof window.crypto.randomUUID === 'function')
                ? window.crypto.randomUUID()
                : 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                    var r = Math.floor(Math.random() * 16);
                    return (c === 'x' ? r : ((r & 0x3) | 0x8)).toString(16);
                });
        }
        return clientIdLocal;
    }

    // "Confirmar y guardar" de la vista preliminar: manda el pedido a Pedidos/Guardar (modulo Pedidos, Fase 3). El servidor repite las
    // reglas (descarta lineas <= 0, rechaza un pedido vacio, toma nombres y codigos de su catalogo): el navegador solo manda ids,
    // cantidades y el contexto informativo (precio de ultima compra, stock y punto del momento).
    function confirmarGuardado() {
        if (guardandoPedido) return;
        var pedido = obtenerPedido();
        if (pedido.lineas.length === 0) {
            avisar('Pedido vacío', 'No hay ninguna cantidad cargada para guardar.', 'info');
            return;
        }
        if (!cfg.guardarUrl) {
            avisar('No disponible', 'El guardado de pedidos no está disponible en este sistema.', 'warning');
            return;
        }

        var cuerpo = {
            clientId: pedido.clientId,
            nota: '',
            idUsuarioCreador: 0, // 0 = el usuario de la sesion (Pedidos/Guardar resuelve la cuenta de produccion)
            lineas: pedido.lineas.map(function (l) {
                return {
                    idCorte: l.idCorte, idSucursal: l.idSucursal, cantidad: l.cantidad,
                    precioUnitario: l.precio, proveedor: l.proveedor, fechaPrecio: l.fechaPrecio || null,
                    stockActual: l.stock, puntoStock: l.punto
                };
            })
        };

        guardandoPedido = true;
        guardando = true; // el autoguardado del borrador se frena mientras se guarda (no debe pisar el cierre del borrador)
        $btnConfirmar.prop('disabled', true);
        var token = $('input[name="__RequestVerificationToken"]').first().val() || '';

        window.fetch(cfg.guardarUrl, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body: JSON.stringify(cuerpo)
        }).then(function (resp) {
            return resp.json().catch(function () { return { ok: false, mensaje: 'Respuesta inesperada del servidor (' + resp.status + ').' }; });
        }).then(function (resp) {
            if (resp && resp.ok) {
                alGuardarPedido(resp);
            } else {
                guardando = false;
                avisar('No se pudo guardar el pedido', (resp && resp.mensaje) || 'Intentá de nuevo.', 'error');
            }
        }).catch(function () {
            guardando = false;
            avisar('Sin conexión', 'No se pudo comunicar con el servidor. El pedido sigue cargado: intentá guardarlo de nuevo.', 'error');
        }).then(function () {
            guardandoPedido = false;
            $btnConfirmar.prop('disabled', false);
        });
    }

    // El pedido se guardo: se cierra el borrador, se limpia lo cargado y se ofrece ticket / PDF / mail / Excel (pedido-acciones.js).
    function alGuardarPedido(resp) {
        pedidoGuardado(resp.id);
        clientIdLocal = null;
        var mensaje = 'Pedido N° ' + resp.id + (resp.yaExistia ? ' (ya estaba guardado)' : ' guardado')
            + (resp.totalEstimado > 0 ? ' · ' + (resp.lineasSinPrecio > 0 ? TXT_TOTAL_PARCIAL : TXT_TOTAL).toLowerCase() + ' ' + fmtPlata.format(resp.totalEstimado) : '')
            + (resp.lineasSinPrecio > 0 ? ' · ' + plural(resp.lineasSinPrecio, 'línea sin precio', 'líneas sin precio') : '')
            + '. ¿Qué querés hacer ahora?';
        if (window.PedidoAcciones && typeof window.PedidoAcciones.abrirDialogo === 'function') {
            window.PedidoAcciones.abrirDialogo({ id: resp.id, mensaje: mensaje, acciones: resp.acciones });
        } else {
            avisar('Pedido guardado', 'Pedido N° ' + resp.id + ' guardado.', 'success');
        }
    }

    // La Fase 3 la llama cuando el pedido se guardo bien: cierra el borrador (idempotencia), limpia las cantidades y apaga el modo pedido.
    function pedidoGuardado(idPedido) {
        guardando = true;
        if (borrador) borrador.marcarFinalizado(idPedido);
        cantidades = {};
        renderTodo();
        sincronizarProteccionSalida();
        $modalPrevia.modal('hide');
        $switch.prop('checked', false);
        apagar();
        guardando = false;
    }

    window.ExistenciaPedido = {
        estaActivo: function () { return activo; },
        obtenerPedido: obtenerPedido,
        pedidoGuardado: pedidoGuardado
    };

    // ---------- Eventos ----------

    $switch.on('change', function () { if (this.checked) encender(); else apagar(); });

    $selSegun.val(segun).on('change', function () {
        segun = calculo.normalizarSegun($(this).val());
        guardarLS(CLAVE_SEGUN, segun);
        renderTodo();
    });

    $('#lnkCambiarPeriodoPedido').on('click', function (e) { e.preventDefault(); pantalla.irAlPeriodo(); });
    $btnTodas.on('click', function () { aceptarSugerencias(null, false); });
    $btnPunto.on('click', function () { aceptarSugerencias(null, true); });
    // "Ver precios": muestra/oculta las columnas $ ult. compra e Importe est. de la tabla (la clase la usa el CSS de la vista). Al ocultarlas, si la
    // tabla estaba ordenada por Importe vuelve al orden por codigo (no quedaria ninguna columna que explique el orden). No se recuerda.
    $switchPrecios.on('change', function () {
        $page.toggleClass('existencia-precios-on', this.checked);
        if (!this.checked) pantalla.restablecerOrdenSi('imp');
        pantalla.ajustarAlto();
        // Dos columnas mas: en la ventana independiente puede que ya no entren todas.
        if (this.checked) window.setTimeout(pantalla.proponerEnsancharVentana, 0);
    });
    $btnColapsar.on('click', function () { aplicarColapso(!$barra.hasClass('ex-colapsada'), true); });
    $btnGuardarMini.on('click', abrirVistaPrevia);
    $btnLimpiar.on('click', limpiarPedido);
    $btnGuardar.on('click', abrirVistaPrevia);
    $btnConfirmar.on('click', confirmarGuardado);
    if ($btnBorradores.length) {
        $btnBorradores.on('click', function () { if (borrador) borrador.abrirModalBorradores(); });
    }

    // Cambio de la fecha del periodo: el consumo se pide de nuevo (con espera, por si se esta tipeando).
    $page.on('existencia:periodoCambiado', function () {
        if (!activo) return;
        clearTimeout(timerPeriodo);
        consumoEstado = 'cargando';
        renderLeyenda();
        timerPeriodo = setTimeout(cargarConsumo, DEBOUNCE_PERIODO_MS);
    });

    // La tabla se reemplazo (Buscar): se vuelven a poner las cantidades en los inputs nuevos.
    $page.on('existencia:tablaCargada', function () {
        asegurarIndice();
        if (activo) renderTodo();
        if (restauracionPendiente) {
            restauracionPendiente = false;
            avisarLineasNoEncontradas();
        }
    });

    // Cambio el filtro de la barra (texto, Estado): las filas visibles son otras.
    $page.on('existencia:filtrado', function () { if (activo) actualizarResumenCeldas(); });

    // Se edito un punto de stock desde la tabla: el stock no cambia pero el punto si, y con el la sugerencia.
    $page.on('existencia:puntoCambiado', function (e, idCorte, idSucursal) {
        asegurarIndice();
        var prod = catalogo[idCorte];
        var td = indice.filas[idCorte] ? $(indice.filas[idCorte]).find('td.ex-col-estado[data-id-sucursal="' + idSucursal + '"]')[0] : null;
        if (prod && td) prod.punto[idSucursal] = parseFloat(td.getAttribute('data-punto')) || 0;
        if (!activo) return;
        var celda = indice.celdas[idCorte + '_' + idSucursal];
        if (celda) renderCelda(celda);
        actualizarResumenCeldas();
    });

    // Tabla (delegado: se reemplaza entera en cada busqueda).
    $contenedor.on('input', 'input.ex-in-pedir', function () {
        var celda = celdaDeInput(this);
        if (!celda) return;
        var r = calculo.parsearCantidad(this.value);
        var $in = $(this);
        if (r.vacio) {
            fijarCantidad(celda.idCorte, celda.idSuc, null);
            $in.removeClass('ex-in-invalido').removeAttr('title');
        } else if (r.ok) {
            fijarCantidad(celda.idCorte, celda.idSuc, r.valor);
            $in.removeClass('ex-in-invalido').removeAttr('title');
        } else {
            fijarCantidad(celda.idCorte, celda.idSuc, null); // lo invalido no cuenta
            $in.addClass('ex-in-invalido').attr('title', 'Cantidad inválida: usá un número (hasta 3 decimales), por ejemplo 12,5');
        }
        refrescarCelda(celda);
    });

    // Al salir del input se deja la cantidad con el formato normal (52,5 en vez de 52.50).
    $contenedor.on('blur', 'input.ex-in-pedir', function () {
        seleccionPendiente = null;
        var celda = celdaDeInput(this);
        if (celda) renderCelda(celda);
    });

    // Al enfocar se selecciona todo: lo que se escriba reemplaza la cantidad. El mouseup del mismo click no debe deseleccionarlo.
    $contenedor.on('focus', 'input.ex-in-pedir', function () { this.select(); seleccionPendiente = this; });
    $contenedor.on('mouseup', 'input.ex-in-pedir', function (e) { if (seleccionPendiente === this) { e.preventDefault(); seleccionPendiente = null; } });

    $contenedor.on('keydown', 'input.ex-in-pedir', function (e) {
        if (e.key === 'Enter') { // nunca envia un formulario
            e.preventDefault();
            moverFoco(this, e.shiftKey ? -1 : 1);
        }
    });

    // Aceptar una sugerencia puntual (click en la pista o en el chip del punto).
    $contenedor.on('click', '.ex-pista, .ex-chip-punto', function (e) {
        e.stopPropagation();
        var td = $(this).closest('td.ex-pedir-cell')[0];
        if (!td) return;
        asegurarIndice();
        var celda = indice.celdas[td.getAttribute('data-id-corte') + '_' + td.getAttribute('data-id-sucursal')];
        var valor = parseFloat(this.getAttribute('data-valor'));
        if (!celda || !(valor > 0)) return;
        fijarCantidad(celda.idCorte, celda.idSuc, valor);
        refrescarCelda(celda);
    });

    // Aceptar las sugerencias de una sucursal (boton en el encabezado de su grupo).
    $contenedor.on('click', '.ex-btn-aceptar-suc', function (e) {
        e.stopPropagation();
        aceptarSugerencias(parseInt(this.getAttribute('data-id-sucursal'), 10), false);
    });

    // ---------- Arranque ----------

    borrador = crearBorrador();
    if (borrador) borrador.actualizarBadgeInicial();
})(window.jQuery, window);
