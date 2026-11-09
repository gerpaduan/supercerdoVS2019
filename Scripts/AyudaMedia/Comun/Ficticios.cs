// Datos ficticios en la grabacion (pedido del usuario, 2026-11-09: "siempre usar datos ficticios"). Lo que ya existe
// en la base y se ve en pantalla (empresa, sucursales, leyendas del ticket) se reemplaza SOLO en el navegador que
// graba, segun una tabla real -> ficticio. La tabla vive FUERA del repo (AYUDA_MEDIA_FICTICIOS = ruta a un JSON
// {"texto real": "texto ficticio", ...}) para no versionar datos reales.
using System.Text.Json;

namespace AyudaMedia.Comun;

public static class Ficticios
{
    public static IReadOnlyDictionary<string, string> CargarDesdeEntorno()
    {
        string ruta = Environment.GetEnvironmentVariable("AYUDA_MEDIA_FICTICIOS")
            ?? throw new InvalidOperationException("Falta la variable de entorno AYUDA_MEDIA_FICTICIOS (JSON real -> ficticio, fuera del repo).");
        var tabla = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(ruta))
            ?? throw new InvalidOperationException($"El JSON de ficticios {ruta} esta vacio.");
        if (tabla.Count == 0 || tabla.Keys.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException($"El JSON de ficticios {ruta} no tiene reemplazos validos.");
        return tabla;
    }

    // Falla si alguna pagina mostro un texto real o si el script de ficticios no esta activo (un error en el script
    // dejaria todos los datos reales a la vista). Se llama antes de armar el .mp4.
    public static async Task VerificarSinFugasAsync(Microsoft.Playwright.IPage page)
    {
        bool activo = await page.EvaluateAsync<bool>("() => typeof window.__ficticios === 'function'");
        if (!activo)
            throw new InvalidOperationException("El script de datos ficticios no esta activo en la pagina: el video podria mostrar datos reales. No se genera.");
        string? fugas = await page.EvaluateAsync<string?>("() => sessionStorage.getItem('__fugaFicticios')");
        if (!string.IsNullOrEmpty(fugas))
            throw new InvalidOperationException($"Se vieron datos reales en pantalla ({fugas}). No se genera el video.");
    }

    // Script de inicio de pagina: reemplaza en textos, value/placeholder/title y en el titulo del documento, y vuelve a
    // aplicar ante cada cambio del DOM (el POS arma casi todo por JS). Expone window.__ficticios(doc) para aplicarlo en
    // documentos aparte (el HTML del ticket que se muestra en la impresora simulada).
    public static string ScriptInicio(IReadOnlyDictionary<string, string> tabla)
    {
        // Primero los textos mas largos: asi "San Martín 123" no queda a medio reemplazar por "San Martín".
        var pares = tabla.OrderByDescending(p => p.Key.Length).Select(p => new[] { p.Key, p.Value }).ToArray();
        string json = JsonSerializer.Serialize(pares);
        return @"
(function () {
    var pares = " + json + @";
    function reemplazar(s) {
        if (!s) return s;
        for (var i = 0; i < pares.length; i++) if (s.indexOf(pares[i][0]) >= 0) s = s.split(pares[i][0]).join(pares[i][1]);
        return s;
    }
    function aplicar(raiz) {
        var doc = raiz.ownerDocument || raiz;
        var w = doc.createTreeWalker(raiz, NodeFilter.SHOW_TEXT, null);
        var n;
        while ((n = w.nextNode())) { var t = reemplazar(n.nodeValue); if (t !== n.nodeValue) n.nodeValue = t; }
        if (raiz.querySelectorAll) raiz.querySelectorAll('input,textarea,[title],[placeholder]').forEach(function (el) {
            ['value', 'placeholder', 'title'].forEach(function (a) {
                var v = a === 'value' ? el.value : el.getAttribute(a);
                if (v) { var r = reemplazar(v); if (r !== v) { if (a === 'value') el.value = r; else el.setAttribute(a, r); } }
            });
        });
        if (doc.title) doc.title = reemplazar(doc.title);
    }
    window.__ficticios = function (doc) { aplicar(doc.body || doc); };
    // Para textos que no estan en el DOM (los del PDF dibujado en un canvas).
    window.__ficticiosReemplazar = reemplazar;
    // Fuga encontrada el 2026-11-09: al recargar el POS se grabo un cuadro con los textos reales, pintado antes de
    // DOMContentLoaded. Por eso (1) la pagina queda invisible hasta el primer reemplazo completo y (2) el observer
    // arranca ya, sobre todo el documento, y reemplaza cada nodo a medida que el parser lo agrega.
    // Al correr este script el documento puede no tener todavia <html>: el estilo se agrega apenas aparece.
    var oculto = document.createElement('style');
    oculto.textContent = 'html{visibility:hidden !important}';
    var mostrado = false;
    function ocultarSiSePuede() {
        if (!mostrado && document.documentElement && !oculto.isConnected) document.documentElement.appendChild(oculto);
    }
    ocultarSiSePuede();
    new MutationObserver(function (ms) {
        ocultarSiSePuede();
        ms.forEach(function (m) {
            if (m.type === 'characterData') { var t = reemplazar(m.target.nodeValue); if (t !== m.target.nodeValue) m.target.nodeValue = t; }
            m.addedNodes && m.addedNodes.forEach(function (x) { if (x.nodeType === 3) { var t = reemplazar(x.nodeValue); if (t !== x.nodeValue) x.nodeValue = t; } else if (x.nodeType === 1) aplicar(x); });
        });
    }).observe(document, { childList: true, subtree: true, characterData: true });
    // Control de fugas: si despues de reemplazar queda visible un texto real, se anota (sessionStorage sobrevive a
    // las recargas del POS) y el escenario falla al terminar (Ficticios.VerificarSinFugasAsync).
    function controlarFugas() {
        var texto = (document.body && document.body.innerText) || '';
        for (var i = 0; i < pares.length; i++) {
            if (texto.indexOf(pares[i][0]) >= 0) {
                try { sessionStorage.setItem('__fugaFicticios', (sessionStorage.getItem('__fugaFicticios') || '') + location.pathname + '; '); }
                catch (e) { console.error('No se pudo anotar la fuga de datos reales', e); }
                return;
            }
        }
    }
    function iniciar() {
        aplicar(document.body);
        mostrado = true;
        oculto.remove();
        // Un .value asignado por JS no dispara el observer: se repasa cada 300 ms (es idempotente).
        setInterval(function () { aplicar(document.body); controlarFugas(); }, 300);
    }
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', iniciar); else iniciar();
})();
";
    }
}
