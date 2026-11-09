// Impresion simulada para los videos (pedido del usuario, 2026-11-09: "no mostrar la vista previa, mas vale simular
// como seria la impresion"). En vez del dialogo del navegador o de la pestana con el PDF se dibuja una impresora de la
// que sale el papel. El papel muestra el comprobante REAL de la venta (ticket HTML de Ventas/ImprimirTicketHtml o PDF
// de Ventas/Imprimir), con los datos ficticios aplicados y sin la "X" del encabezado. Todo pasa solo en el navegador
// que graba: la app no cambia.
namespace AyudaMedia.Comun;

public static class Impresion
{
    // Version exacta de pdf.js (cdnjs) para dibujar la primera pagina del PDF en un canvas. Solo se carga en el
    // navegador que graba.
    private const string PdfJsBase = "https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/";

    // window.__simularTicket(url) / window.__simularPdf(url) (= __prepararPdf + __mostrarPdf) -> Promise que se resuelve cuando el papel termino de
    // salir. window.__cerrarImpresion() saca la escena.
    public const string JsEscena = @"
// Escena comun: fondo oscurecido, impresora abajo al centro y una ventana por donde sube el papel (el encabezado
// sale primero). 'papel' es el nodo ya armado; ancho/alto su tamano en pantalla.
window.__escenaImpresora = async function (papel, ancho, alto, etiqueta, anchoImpresora) {
    var capa = document.createElement('div');
    capa.id = '__impresion';
    capa.style.cssText = 'position:fixed;inset:0;z-index:2147483500;background:rgba(15,23,42,.62);';
    var impresora = document.createElement('div');
    impresora.style.cssText = 'position:absolute;left:50%;bottom:24px;transform:translateX(-50%);width:' + anchoImpresora + 'px;height:112px;border-radius:22px;'
        + 'background:linear-gradient(#4b5563,#1f2937);box-shadow:0 14px 30px rgba(0,0,0,.45), inset 0 2px 0 rgba(255,255,255,.18);';
    impresora.innerHTML = '<div style=""position:absolute;left:36px;right:36px;top:14px;height:10px;border-radius:5px;background:#0b0f17;""></div>'
        + '<div style=""position:absolute;right:28px;bottom:22px;width:12px;height:12px;border-radius:50%;background:#22c55e;box-shadow:0 0 10px #22c55e;""></div>'
        + '<div style=""position:absolute;left:28px;bottom:18px;color:#cbd5e1;font:700 15px Arial,sans-serif;letter-spacing:.08em;"">' + etiqueta + '</div>';
    var salida = document.createElement('div');
    salida.style.cssText = 'position:absolute;left:50%;bottom:' + (24 + 112 - 16) + 'px;transform:translateX(-50%);height:0;overflow:hidden;'
        + 'display:flex;flex-direction:column;justify-content:flex-start;filter:drop-shadow(0 6px 10px rgba(0,0,0,.35));';
    papel.style.width = ancho + 'px';
    papel.style.height = alto + 'px';
    papel.style.flex = '0 0 auto';
    salida.appendChild(papel);
    capa.appendChild(salida);
    capa.appendChild(impresora);
    document.body.appendChild(capa);
    // Mientras esta la impresora, el cartel va arriba (abajo taparia la impresora).
    var estilo = document.createElement('style');
    estilo.id = '__impresionCartel';
    estilo.textContent = '#__cartel{top:24px !important;bottom:auto !important;}';
    document.head.appendChild(estilo);

    var ms = 4200, t0 = performance.now();
    await new Promise(function (ok) {
        function paso(t) {
            var p = Math.min(1, (t - t0) / ms);
            salida.style.height = Math.round(alto * p) + 'px';
            if (p < 1) requestAnimationFrame(paso); else ok();
        }
        requestAnimationFrame(paso);
    });
};
// Alto para el papel: deja abajo la impresora y arriba lugar para el cartel (que durante la impresion va arriba).
window.__altoDisponible = function () { return window.innerHeight - (24 + 112 - 16) - 150; };

// Ticket termico: HTML real limpio (sin scripts ni onload, que llama a window.print, y sin la X), en un iframe.
window.__simularTicket = async function (url) {
    var html = await (await fetch(url, { credentials: 'same-origin' })).text();
    var d = new DOMParser().parseFromString(html, 'text/html');
    d.body.removeAttribute('onload');
    d.querySelectorAll('script').forEach(function (s) { s.remove(); });
    var letra = d.querySelector('.ticket-title');
    if (letra) letra.remove();
    // Imagenes fuera: el QR de AFIP de una factura codifica el CUIT real del emisor (no se puede reemplazar como texto).
    d.querySelectorAll('img, svg, canvas').forEach(function (n) { n.remove(); });
    // Renglones de homologacion (*** PRUEBA *** y SIN VALIDEZ FISCAL): solo existen en modo prueba de AFIP.
    d.querySelectorAll('.ticket-line').forEach(function (n) {
        if (/^\s*(\*+\s*PRUEBA\s*\*+|SIN VALIDEZ FISCAL)\s*$/i.test(n.textContent || '')) n.remove();
    });
    if (window.__ficticios) window.__ficticios(d);

    // Se mide fuera de pantalla antes de armar la escena.
    var marco = document.createElement('iframe');
    marco.setAttribute('scrolling', 'no');
    marco.style.cssText = 'position:fixed;left:-5000px;top:0;border:0;display:block;background:#fff;transform-origin:top left;';
    document.body.appendChild(marco);
    await new Promise(function (ok) { marco.onload = ok; marco.srcdoc = '<!doctype html>' + d.documentElement.outerHTML; });
    var cuerpo = marco.contentDocument.documentElement;
    var ancho = Math.max(cuerpo.scrollWidth, 280), alto = cuerpo.scrollHeight;
    var escala = Math.min(1.25, window.__altoDisponible() / alto);
    marco.style.width = ancho + 'px';
    marco.style.height = alto + 'px';
    marco.style.transform = 'scale(' + escala + ')';
    marco.style.position = 'static';
    var papel = document.createElement('div');
    papel.style.cssText = 'background:#fff;overflow:hidden;';
    papel.appendChild(marco);
    await window.__escenaImpresora(papel, Math.round(ancho * escala), Math.round(alto * escala), 'IMPRESORA TÉRMICA', 400);
};

// PDF A4 (__prepararPdf): primera pagina dibujada con pdf.js. Los textos con datos reales se tapan y se reescriben con los ficticios,
// y la letra del comprobante (la X del recuadro) se tapa.
window.__prepararPdf = async function (url) {
    if (!window.pdfjsLib) {
        await new Promise(function (ok, ko) {
            var s = document.createElement('script');
            s.src = '" + PdfJsBase + @"pdf.min.js';
            s.onload = ok;
            s.onerror = function () { ko(new Error('No se pudo cargar pdf.js')); };
            document.head.appendChild(s);
        });
        pdfjsLib.GlobalWorkerOptions.workerSrc = '" + PdfJsBase + @"pdf.worker.min.js';
    }
    var datos = await (await fetch(url, { credentials: 'same-origin' })).arrayBuffer();
    var pdf = await pdfjsLib.getDocument({ data: datos }).promise;
    var pagina = await pdf.getPage(1);
    var base = pagina.getViewport({ scale: 1 });
    // Hoja de 640 px de ancho para que se lea: no entra entera, asi que se ve la parte de arriba (la que tiene el
    // comprobante), como una hoja que todavia esta saliendo de la impresora.
    var escala = 640 / base.width;
    var vista = pagina.getViewport({ scale: escala });
    var canvas = document.createElement('canvas');
    canvas.width = Math.round(vista.width);
    canvas.height = Math.round(vista.height);
    var ctx = canvas.getContext('2d');
    ctx.fillStyle = '#fff';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    await pagina.render({ canvasContext: ctx, viewport: vista }).promise;

    var textos = await pagina.getTextContent();
    textos.items.forEach(function (item) {
        var s = item.str || '';
        if (!s.trim()) return;
        var tx = pdfjsLib.Util.transform(vista.transform, item.transform);
        var alto = Math.hypot(tx[2], tx[3]);
        var x = tx[4], y = tx[5], ancho = item.width * escala;
        var esLetra = s.trim() === 'X';
        // Leyenda de homologacion (solo aparece en modo prueba de AFIP; en produccion no esta): se tapa el renglon
        // entero, de borde a borde, junto con su franja de color.
        if (/PRUEBA|SIN VALIDEZ FISCAL|HOMOLOGACI/i.test(s)) {
            ctx.fillStyle = '#fff';
            ctx.fillRect(0, y - alto * 1.6, canvas.width, alto * 2.4);
            return;
        }
        var nuevo = window.__ficticiosReemplazar ? window.__ficticiosReemplazar(s) : s;
        if (!esLetra && nuevo === s) return;
        ctx.fillStyle = '#fff';
        if (esLetra) {
            // Solo la letra (en el PDF de detalle no tiene recuadro).
            // La X no tiene descendentes: se tapa hasta apenas debajo de la linea base, sin tocar la leyenda de abajo.
            ctx.fillRect(x - 2, y - alto * 0.95, ancho + 4, alto * 1.0);
            return;
        }
        ctx.fillRect(x - 1, y - alto * 1.05, ancho + 2, alto * 1.35);
        ctx.fillStyle = '#111';
        ctx.font = (item.fontName && /bold/i.test(item.fontName) ? 'bold ' : '') + Math.round(alto) + 'px Helvetica, Arial, sans-serif';
        ctx.textBaseline = 'alphabetic';
        ctx.fillText(nuevo, x, y);
    });

    var papel = document.createElement('div');
    papel.style.cssText = 'background:#fff;overflow:hidden;';
    // La hoja se muestra como imagen DENTRO de un iframe, como el ticket: dentro del POS, un canvas o un img sueltos
    // se veian en blanco aunque tenian el dibujo (algo del CSS del POS los tapa; verificado 2026-11-09). El iframe
    // aisla la hoja del CSS de la pagina.
    var marco = document.createElement('iframe');
    marco.setAttribute('scrolling', 'no');
    marco.style.cssText = 'border:0;display:block;background:#fff;width:' + canvas.width + 'px;height:' + canvas.height + 'px;';
    // El iframe carga su contenido cuando entra en la escena (srcdoc con la imagen embebida: es inmediato).
    marco.srcdoc = '<!doctype html><html><body style=""margin:0;background:#fff""><img style=""display:block;width:' + canvas.width
        + 'px;height:' + canvas.height + 'px"" src=""' + canvas.toDataURL('image/png') + '""></body></html>';
    papel.appendChild(marco);
    window.__pdfListo = { papel: papel, ancho: canvas.width, alto: Math.min(canvas.height, Math.round(window.__altoDisponible())) };
};
// Saca la hoja ya preparada por __prepararPdf (separado para que la voz arranque cuando aparece la impresora y no
// mientras se descarga y dibuja el PDF).
window.__mostrarPdf = function () {
    var p = window.__pdfListo;
    if (!p) throw new Error('Falta __prepararPdf');
    return window.__escenaImpresora(p.papel, p.ancho, p.alto, 'IMPRESORA', Math.max(460, p.ancho + 80));
};
window.__simularPdf = async function (url) {
    await window.__prepararPdf(url);
    await window.__mostrarPdf();
};
window.__cerrarImpresion = function () {
    var c = document.getElementById('__impresion');
    if (c) c.remove();
    var e = document.getElementById('__impresionCartel');
    if (e) e.remove();
};
// Playwright invoca el resultado de EvaluateAsync si es una funcion: se termina con null para que solo defina.
null;
";

    // Reemplaza la impresion real del ticket (TicketPrint.imprimir abre el dialogo del navegador) por la simulada.
    // Cuando el papel termino de salir (y se dejo ver un momento) cierra la escena y llama a onDone, como la
    // impresion real: en el POS eso pasa solo a una venta nueva (pvbContinuarNuevaVenta en Ventas/POS.cshtml).
    public const string JsInterceptarTicket = @"
window.TicketPrint.imprimir = function (opts) {
    window.__impresionEnCurso = window.__simularTicket(opts.ticketUrl)
        .then(function () { return new Promise(function (ok) { setTimeout(ok, 1800); }); })
        .then(function () {
            window.__cerrarImpresion();
            if (opts && typeof opts.onDone === 'function') opts.onDone();
        });
    return window.__impresionEnCurso;
};
null;
";

    // Script de inicio: "Imprimir PDF" abre el PDF en una pestana nueva (window.open) y enseguida el POS recarga para
    // una venta nueva. En la grabacion no se abre la pestana: se guarda la URL y, ya en la pagina recargada, el
    // escenario muestra la impresora con ese PDF (window.__pdfPendiente()).
    public const string JsInterceptarPdf = @"
(function () {
    var abrir = window.open;
    window.open = function (url) {
        if (typeof url === 'string' && /\/Ventas\/Imprimir\?/i.test(url)) {
            sessionStorage.setItem('__pdfSimulado', url);
            return null;
        }
        return abrir.apply(window, arguments);
    };
    window.__pdfPendiente = function () {
        var u = sessionStorage.getItem('__pdfSimulado');
        sessionStorage.removeItem('__pdfSimulado');
        return u;
    };
})();
";
}
