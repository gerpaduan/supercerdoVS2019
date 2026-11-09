// Scripts que se inyectan en la pagina para las capturas y los clips: recuadros numerados sobre los controles,
// cartel con el paso en curso y un puntero visible (el video del navegador no dibuja el cursor).
namespace AyudaMedia.Comun;

public static class Overlay
{
    // marcar(el, n) / limpiarMarcas() / cartel(texto). Recuadro con position:fixed para que coincida con la captura
    // de la vista. Se evalua con page.EvaluateAsync despues de cada navegacion.
    public const string JsHelpers = @"
window.marcar = function (el, numero) {
    if (!el) return false;
    var r = el.getBoundingClientRect();
    var caja = document.createElement('div');
    caja.setAttribute('data-ayuda-marca', '1');
    caja.style.cssText = 'position:fixed;z-index:2147483000;pointer-events:none;border:3px solid #e11d48;border-radius:6px;box-shadow:0 0 0 2px rgba(255,255,255,.85);'
        + 'left:' + (r.left - 4) + 'px;top:' + (r.top - 4) + 'px;width:' + (r.width + 8) + 'px;height:' + (r.height + 8) + 'px;';
    var chapa = document.createElement('div');
    chapa.textContent = String(numero);
    chapa.style.cssText = 'position:absolute;left:' + (r.left < 24 ? 2 : -14) + 'px;top:-14px;width:26px;height:26px;border-radius:50%;background:#e11d48;color:#fff;font:700 15px/26px Arial,sans-serif;text-align:center;box-shadow:0 1px 4px rgba(0,0,0,.4);';
    caja.appendChild(chapa);
    document.body.appendChild(caja);
    return true;
};
window.limpiarMarcas = function () {
    document.querySelectorAll('[data-ayuda-marca]').forEach(function (n) { n.remove(); });
};
// Cartel con el paso en curso (en los clips mudos es la unica explicacion; con voz, repite lo que se dice).
window.cartel = function (texto) {
    var c = document.getElementById('__cartel');
    if (!c) {
        c = document.createElement('div');
        c.id = '__cartel';
        c.style.cssText = 'position:fixed;z-index:2147483600;left:50%;bottom:64px;transform:translateX(-50%);background:rgba(17,24,39,.92);color:#fff;'
            + 'font:700 34px Arial,sans-serif;padding:12px 28px;border-radius:10px;box-shadow:0 4px 14px rgba(0,0,0,.4);max-width:80%;text-align:center;';
        document.body.appendChild(c);
    }
    c.textContent = texto;
    c.style.display = texto ? 'block' : 'none';
};
";

    // tecla('Enter') muestra arriba al centro una tecla dibujada durante 1,4 s: en los clips que ensenan atajos se
    // llama justo antes de apretar la tecla de verdad, para que se vea que se uso el teclado y no el mouse.
    public const string JsTeclas = @"
window.tecla = function (nombre) {
    var t = document.getElementById('__tecla');
    if (!t) {
        t = document.createElement('div');
        t.id = '__tecla';
        t.style.cssText = 'position:fixed;z-index:2147483601;left:50%;top:84px;transform:translateX(-50%);pointer-events:none;'
            + 'min-width:84px;padding:14px 26px;border-radius:12px;background:linear-gradient(#ffffff,#e5e7eb);color:#111827;'
            + 'font:800 38px Arial,sans-serif;text-align:center;border:2px solid #9ca3af;box-shadow:0 6px 0 #6b7280, 0 10px 22px rgba(0,0,0,.35);';
        document.body.appendChild(t);
    }
    t.textContent = nombre;
    t.style.display = 'block';
    clearTimeout(window.__teclaTimer);
    window.__teclaTimer = setTimeout(function () { t.style.display = 'none'; }, 1400);
};
null;
";

    // Circulo que sigue al mouse. Se registra con context.AddInitScriptAsync para que este en cada pagina.
    public const string JsPuntero = @"
(function () {
    function crear() {
        if (document.getElementById('__puntero')) return;
        var p = document.createElement('div');
        p.id = '__puntero';
        p.style.cssText = 'position:fixed;z-index:2147483647;pointer-events:none;width:34px;height:34px;margin:-6px 0 0 -6px;border-radius:50%;'
            + 'background:rgba(225,29,72,.55);border:2px solid #e11d48;left:-50px;top:-50px;transition:transform .08s;';
        document.documentElement.appendChild(p);
        document.addEventListener('mousemove', function (e) { p.style.left = e.clientX + 'px'; p.style.top = e.clientY + 'px'; }, true);
        document.addEventListener('mousedown', function () { p.style.transform = 'scale(.6)'; }, true);
        document.addEventListener('mouseup', function () { p.style.transform = 'scale(1)'; }, true);
    }
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', crear); else crear();
})();
";
}
