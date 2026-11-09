// Balanza y lector de codigo de barras simulados para los videos (pedido del usuario, 2026-11-09: "como en la
// pantalla de presentacion, se simula el peso de un pedazo de carne en la balanza y la lectura del codigo de barras").
// La balanza: el POS consulta al agente local (Scripts/carnisys.balanza.js: GET http://127.0.0.1:5100/status y /peso);
// en la grabacion esas consultas las responde Playwright con el peso que fija el escenario. El POS hace con esa
// lectura lo mismo que con una balanza real (pos-balanza.js). El lector: un lector de mano "tipea" el codigo y Enter,
// asi que el escenario tipea el EAN; la escena dibujada (etiqueta + laser) es solo visual.
using System.Globalization;
using Microsoft.Playwright;

namespace AyudaMedia.Comun;

public sealed class BalanzaSimulada
{
    private const string UrlAgente = "http://127.0.0.1:5100/**";

    // false = el agente "no esta" (conexion rechazada): el POS trabaja con cantidad manual.
    public bool Conectada { get; set; }
    // Peso en kg que devuelve /peso.
    public double Peso { get; set; }

    public async Task InstalarAsync(IBrowserContext contexto)
    {
        await contexto.RouteAsync(UrlAgente, async ruta =>
        {
            if (!Conectada)
            {
                await ruta.AbortAsync("connectionrefused");
                return;
            }
            string peso = Peso.ToString("0.000", CultureInfo.InvariantCulture);
            string cuerpo = ruta.Request.Url.Contains("/peso", StringComparison.OrdinalIgnoreCase)
                ? $"{{\"ok\":true,\"conectada\":true,\"peso\":{peso},\"pesoTexto\":\"{peso}\",\"pesoDisplay\":\"{peso}\",\"inestable\":false,\"unidad\":\"kg\"}}"
                : "{\"ok\":true,\"conectada\":true,\"puerto\":\"COM3\",\"modelo\":\"Balanza\"}";
            await ruta.FulfillAsync(new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = cuerpo,
                // La pagina (127.0.0.1:5272) consulta otro puerto: sin este header el navegador descarta la respuesta.
                Headers = new Dictionary<string, string> { ["Access-Control-Allow-Origin"] = "*" }
            });
        });
    }

    // window.__balanza(kg) muestra la balanza dibujada con un trozo de carne que cae y el visor subiendo hasta kg;
    // window.__lector(ean, nombre) muestra la etiqueta con el codigo de barras y el laser; __perifericoOcultar() saca
    // la escena. Se dibujan sobre la parte vacia del carrito (abajo a la izquierda), sin tapar los controles.
    public const string JsEscenas = @"
window.__perifericoOcultar = function () {
    var e = document.getElementById('__periferico');
    if (e) e.remove();
};
window.__perifericoCaja = function () {
    window.__perifericoOcultar();
    var c = document.createElement('div');
    c.id = '__periferico';
    c.style.cssText = 'position:fixed;left:40px;top:330px;width:440px;height:200px;z-index:2147483400;border-radius:16px;'
        + 'background:linear-gradient(#f8fafc,#e2e8f0);box-shadow:0 10px 30px rgba(15,23,42,.35);overflow:hidden;font-family:Arial,sans-serif;';
    document.body.appendChild(c);
    return c;
};
window.__balanza = function (kg) {
    var c = window.__perifericoCaja();
    c.innerHTML = '<div style=""position:absolute;left:26px;right:26px;bottom:26px;height:22px;border-radius:8px;background:linear-gradient(#94a3b8,#475569);""></div>'
        + '<div style=""position:absolute;left:60px;right:150px;bottom:48px;height:10px;border-radius:5px;background:#cbd5e1;""></div>'
        + '<div id=""__carne"" style=""position:absolute;left:110px;bottom:180px;width:150px;height:62px;border-radius:48% 52% 45% 55%;'
        + 'background:radial-gradient(circle at 35% 40%,#e2766f 0,#b4403c 55%,#8a2a28 100%);box-shadow:inset 0 -8px 0 rgba(255,240,230,.55);transition:bottom .55s cubic-bezier(.3,1.4,.6,1);""></div>'
        + '<div style=""position:absolute;right:26px;bottom:54px;width:112px;height:58px;border-radius:8px;background:#0f172a;box-shadow:inset 0 0 0 3px #334155;""></div>'
        + '<div id=""__visor"" style=""position:absolute;right:34px;bottom:66px;width:96px;text-align:right;color:#4ade80;font:700 26px Consolas,monospace;letter-spacing:1px;"">0,000</div>'
        + '<div style=""position:absolute;right:36px;bottom:116px;color:#475569;font:700 12px Arial;letter-spacing:.1em;"">BALANZA  kg</div>';
    return new Promise(function (ok) {
        setTimeout(function () { document.getElementById('__carne').style.bottom = '58px'; }, 150);
        var t0 = performance.now() + 600, ms = 900;
        function paso(t) {
            var p = Math.max(0, Math.min(1, (t - t0) / ms));
            var v = document.getElementById('__visor');
            if (v) v.textContent = (kg * p).toFixed(3).replace('.', ',');
            if (p < 1) requestAnimationFrame(paso); else ok();
        }
        requestAnimationFrame(paso);
    });
};
// Barras EAN-13 (codificacion estandar L/G/R segun el primer digito) dibujadas en SVG.
window.__barrasEan13 = function (ean) {
    var L = ['0001101','0011001','0010011','0111101','0100011','0110001','0101111','0111011','0110111','0001011'];
    var G = ['0100111','0110011','0011011','0100001','0011101','0111001','0000101','0010001','0001001','0010111'];
    var R = ['1110010','1100110','1101100','1000010','1011100','1001110','1010000','1000100','1001000','1110100'];
    var P = ['LLLLLL','LLGLGG','LLGGLG','LLGGGL','LGLLGG','LGGLLG','LGGGLL','LGLGLG','LGLGGL','LGGLGL'];
    var d = ean.split('').map(Number), par = P[d[0]], bits = '101';
    for (var i = 1; i <= 6; i++) bits += (par[i - 1] === 'L' ? L : G)[d[i]];
    bits += '01010';
    for (var j = 7; j <= 12; j++) bits += R[d[j]];
    bits += '101';
    var w = 2, svg = '<svg xmlns=""http://www.w3.org/2000/svg"" width=""' + (bits.length * w) + '"" height=""80"">';
    for (var k = 0; k < bits.length; k++) if (bits[k] === '1') svg += '<rect x=""' + (k * w) + '"" y=""0"" width=""' + w + '"" height=""80"" fill=""#111""/>';
    return svg + '</svg>';
};
window.__lector = function (ean, nombre) {
    var c = window.__perifericoCaja();
    c.innerHTML = '<div style=""position:absolute;left:30px;top:22px;right:30px;bottom:22px;background:#fff;border-radius:10px;box-shadow:0 2px 6px rgba(0,0,0,.15);""></div>'
        + '<div style=""position:absolute;left:0;right:0;top:34px;text-align:center;color:#0f172a;font:700 17px Arial;"">' + nombre + '</div>'
        + '<div style=""position:absolute;left:0;right:0;top:66px;text-align:center;"">' + window.__barrasEan13(ean) + '</div>'
        + '<div style=""position:absolute;left:0;right:0;top:150px;text-align:center;color:#111;font:16px Consolas,monospace;letter-spacing:3px;"">' + ean + '</div>'
        + '<div id=""__laser"" style=""position:absolute;left:60px;right:60px;top:66px;height:3px;background:#ef4444;box-shadow:0 0 12px 3px rgba(239,68,68,.8);""></div>';
    return new Promise(function (ok) {
        var t0 = performance.now(), ms = 1300;
        function paso(t) {
            var p = Math.min(1, (t - t0) / ms), l = document.getElementById('__laser');
            if (l) l.style.top = (66 + 80 * (0.5 - 0.5 * Math.cos(p * Math.PI * 2))) + 'px';
            if (p < 1) requestAnimationFrame(paso);
            else { if (l) l.style.background = '#22c55e'; setTimeout(ok, 250); }
        }
        requestAnimationFrame(paso);
    });
};
null;
";
}
