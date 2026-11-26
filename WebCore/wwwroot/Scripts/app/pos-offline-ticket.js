/*
 * POS sin conexion: ticket de la venta armado en la PC (2026-11-26). Mismas lineas que el ticket X del servidor
 * (VentasController.ConstruirLineasTicketVenta, rama no facturada) mas la leyenda de venta sin conexion. Va al agente
 * de impresion local (127.0.0.1:18777, mismo endpoint que print-agent.js) y, si no esta, a la impresion del navegador.
 * Numero del ticket: no hay idVenta hasta subirla, se imprime "OFF-" + los primeros 8 caracteres del clientId.
 */
(function (root) {
    'use strict';

    var AGENTE = 'http://127.0.0.1:18777';
    // Mismos codigos que WebCore/Services/EscPosFormato.cs.
    var NEGRITA_ON = '\x1B\x45\x01', NEGRITA_OFF = '\x1B\x45\x00';
    var DOBLE_ON = '\x1D\x21\x11', DOBLE_OFF = '\x1D\x21\x00';

    function truncar(texto, max) {
        texto = texto == null ? '' : String(texto);
        return texto.length > max ? texto.substring(0, max) : texto;
    }

    function centrar(texto, ancho) {
        texto = truncar(texto, ancho);
        var izq = Math.max(0, Math.floor((ancho - texto.length) / 2));
        return new Array(izq + 1).join(' ') + texto;
    }

    // Formato N2 de .NET en es-AR: miles con punto y decimales con coma.
    function n2(valor) {
        return Number(valor || 0).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function f3(valor) {
        return Number(valor || 0).toLocaleString('es-AR', { minimumFractionDigits: 3, maximumFractionDigits: 3, useGrouping: false });
    }

    function extremos(izq, der, ancho) {
        izq = truncar(izq, 18);
        der = truncar(der, 18);
        return izq + new Array(Math.max(1, ancho - izq.length - der.length) + 1).join(' ') + der;
    }

    function total(etiqueta, importe, ancho) {
        var der = n2(importe);
        return etiqueta + new Array(Math.max(1, ancho - etiqueta.length - der.length) + 1).join(' ') + der;
    }

    function dosDigitos(n) { return (n < 10 ? '0' : '') + n; }

    // venta: { clientId, fecha (Date), formaPago, efectivoMixto, lineas: [{ nombre, cantKg, precioKg }], total, pagaCon, vuelto }.
    // encabezado: catalogo.ticket ({ negocio, slogan1..3, ocultarEncabezado }).
    function lineas(venta, encabezado, mm) {
        var ancho = mm === 58 ? 32 : 43;
        var anchoDoble = Math.floor(ancho / 2);
        var r = [];
        r.push(DOBLE_ON + centrar('X', anchoDoble) + DOBLE_OFF);
        r.push(centrar('-No valido como Factura-', ancho));
        r.push(centrar('VENTA SIN CONEXION', ancho));
        r.push('');
        encabezado = encabezado || {};
        if (!encabezado.ocultarEncabezado) {
            if (encabezado.negocio) r.push(DOBLE_ON + centrar(encabezado.negocio, anchoDoble) + DOBLE_OFF);
            if (encabezado.slogan1) r.push(centrar(encabezado.slogan1, ancho));
            if (encabezado.slogan2) r.push(centrar(encabezado.slogan2, ancho));
            if (encabezado.slogan3 && encabezado.slogan3 !== '-') r.push(centrar(encabezado.slogan3, ancho));
        }
        var forma = (venta.formaPago || '') + (venta.efectivoMixto > 0 ? '|Efvo' : '');
        var f = venta.fecha instanceof Date ? venta.fecha : new Date(venta.fecha);
        r.push(truncar('A Consumidor Final', ancho));
        r.push(truncar('Forma Pago: ' + forma, ancho));
        r.push(truncar('Nro. T. OFF-' + String(venta.clientId || '').substring(0, 8).toUpperCase(), ancho));
        r.push(extremos('Fecha: ' + dosDigitos(f.getDate()) + '/' + dosDigitos(f.getMonth() + 1) + '/' + f.getFullYear(),
            'Hora: ' + dosDigitos(f.getHours()) + ':' + dosDigitos(f.getMinutes()), ancho));
        r.push(new Array(ancho + 1).join('-'));
        (venta.lineas || []).forEach(function (l) {
            r.push(truncar((l.nombre || '').trim(), ancho));
            r.push(total(f3(l.cantKg) + ' x ' + n2(l.precioKg), l.cantKg * l.precioKg, ancho));
        });
        r.push(('-------').padStart(ancho, ' '));
        r.push(NEGRITA_ON + total('Total', venta.total, ancho) + NEGRITA_OFF);
        if (venta.pagaCon > 0) {
            r.push(total('Pago', venta.pagaCon, ancho));
            r.push(total('Vuelto', venta.vuelto || 0, ancho));
        }
        r.push('');
        r.push(centrar('Se carga al volver la conexion', ancho));
        return r;
    }

    // Igual que print-agent.js: el agente cuenta caracteres contra un Content-Length en bytes, asi que todo lo no ASCII
    // va escapado a \uXXXX.
    function jsonAscii(datos) {
        return JSON.stringify(datos).replace(/[\u0080-￿]/g, function (c) {
            return '\\u' + ('0000' + c.charCodeAt(0).toString(16)).slice(-4);
        });
    }

    function porAgente(lineasTicket, mm) {
        var Red = root.PosOfflineRed;
        return Red.conTimeout(root.fetch(AGENTE + '/health', { cache: 'no-store' }), 2000)
            .then(function (r) { return r.json(); })
            .then(function (salud) {
                if (!salud || !salud.ok) throw new Error('agente no disponible');
                return Red.conTimeout(root.fetch(AGENTE + '/print/expendio', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json; charset=utf-8' },
                    body: jsonAscii({ ok: true, ticketMm: mm, ticketLines: lineasTicket, qrValue: '', tieneFactura: false })
                }), 5000);
            })
            .then(function (r) { if (!r.ok) throw new Error('el agente no imprimió'); return true; });
    }

    // Respaldo sin agente: ticket en HTML en un iframe oculto y el dialogo de impresion del navegador.
    function porNavegador(lineasTicket, mm) {
        var texto = lineasTicket.map(function (l) { return l.replace(/[\x1B\x1D][\s\S]{2}/g, ''); }).join('\n');
        var escapar = function (s) { return s.replace(/&/g, '&amp;').replace(/</g, '&lt;'); };
        var html = '<!doctype html><html><head><meta charset="utf-8"><style>@page{margin:0}body{margin:0;padding:4px;' +
            'font:12px monospace;width:' + (mm === 58 ? '58mm' : '80mm') + '}pre{margin:0;white-space:pre-wrap}</style></head><body><pre>' +
            escapar(texto) + '</pre></body></html>';
        var viejo = document.getElementById('iframeTicketOffline');
        if (viejo) viejo.remove();
        var iframe = document.createElement('iframe');
        iframe.id = 'iframeTicketOffline';
        iframe.style.cssText = 'position:fixed;right:0;bottom:0;width:1px;height:1px;border:0;opacity:0;pointer-events:none;';
        iframe.onload = function () {
            try { iframe.contentWindow.focus(); iframe.contentWindow.print(); } catch (e) { console.error('No se pudo imprimir el ticket sin conexión', e); }
        };
        iframe.srcdoc = html;
        document.body.appendChild(iframe);
        return Promise.resolve(true);
    }

    // Ancho elegido en el POS normal (misma clave de localStorage que el post-venta).
    function anchoElegido() {
        try { return root.localStorage.getItem('postventa_ticket_mm') === '58' ? 58 : 80; } catch (e) { return 80; }
    }

    function imprimir(venta, encabezado) {
        var mm = anchoElegido();
        var ls = lineas(venta, encabezado, mm);
        return porAgente(ls, mm).catch(function () { return porNavegador(ls, mm); });
    }

    var api = { lineas: lineas, imprimir: imprimir };
    root.PosOfflineTicket = api;
    if (typeof module !== 'undefined' && module.exports) module.exports = api;
})(typeof window !== 'undefined' ? window : globalThis);
