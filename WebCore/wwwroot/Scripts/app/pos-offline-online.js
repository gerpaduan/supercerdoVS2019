/*
 * POS sin conexion, lado POS NORMAL (2026-11-26). Se carga desde Views/Ventas/_PosOffline.cshtml solo con PosOffline
 * habilitado. Con conexion:
 *  - registra el service worker (sw-pos.js) que despues muestra la pagina offline si el servidor no responde;
 *  - baja el catalogo (y las huellas de PIN) al abrir y cada PosOffline:MinutosRefrescoCatalogo;
 *  - sube la cola de ventas hechas sin conexion (al abrir y cada vez que vuelve la red) y muestra el resultado;
 *  - si se corta, ofrece "Seguir vendiendo sin conexión" (no saca al cajero solo: el carrito en curso queda guardado
 *    como siempre en el borrador del POS).
 * Indicador: #posOfflineEstado, dentro del panel derecho del POS debajo de "Conexion balanza" (.pos-balanza-card); si ese
 * panel no esta (POS sin caja), queda fijo arriba al centro.
 */
(function (window, document) {
    'use strict';

    var Db = window.PosOfflineDb;
    var Red = window.PosOfflineRed;
    var Sync = window.PosOfflineSync;
    var raiz = document.getElementById('posOfflineEstado');
    if (!raiz || !Db || !Red || !Sync) return;

    var SEGUNDOS_CHEQUEO_RED = 30;
    var config = { minutosRefrescoCatalogo: 10, segundosTimeoutPing: 4, urlChequeoInternet: '' };
    var posInstanceId = raiz.getAttribute('data-pos-instance') || '';
    var preparado = false;
    var textoEstado = raiz.querySelector('[data-texto]');
    var botonOffline = raiz.querySelector('[data-ir-offline]');
    var avisoCorte = document.getElementById('posOfflineCorte');

    var enPanel = false;

    function setEstado(texto, clase) {
        textoEstado.textContent = texto;
        raiz.title = texto;
        raiz.className = 'pos-offline-estado ' + (clase || '') + (enPanel ? ' en-panel' : '');
    }

    // Mueve el indicador y el resultado al panel de la balanza (lugar propio, sin tapar el POS).
    function ubicarEnPanel() {
        var panel = document.querySelector('.pos-balanza-card');
        if (!panel) return;
        enPanel = true;
        panel.appendChild(raiz);
        raiz.classList.add('en-panel');
        var resultado = document.getElementById('posOfflineResultado');
        if (resultado) { panel.appendChild(resultado); resultado.classList.add('en-panel'); }
    }

    function irOffline() {
        window.location.href = Red.base() + 'Ventas/POS?modo=offline';
    }

    function registrarServiceWorker() {
        if (!('serviceWorker' in navigator) || !window.isSecureContext) {
            return Promise.reject(new Error('el navegador no permite trabajar sin conexión en esta dirección (hace falta HTTPS)'));
        }
        var url = raiz.getAttribute('data-sw-url');
        var scope = raiz.getAttribute('data-sw-scope');
        return navigator.serviceWorker.register(url, { scope: scope });
    }

    function describirPreparacion(catalogo) {
        var cuando = catalogo && catalogo.bajado
            ? new Date(catalogo.bajado).toLocaleString('es-AR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' })
            : '';
        if (!catalogo) return { texto: 'Sin conexión: esta PC todavía no está preparada.', clase: 'pendiente' };
        if (!catalogo.dispositivoAutorizado) return { texto: 'Sin conexión: PC no autorizada, no se podrá entrar con PIN.', clase: 'pendiente' };
        if (!catalogo.usuarios || catalogo.usuarios.length === 0) return { texto: 'Sin conexión: nadie tiene PIN en esta sucursal.', clase: 'pendiente' };
        return { texto: 'Listo para vender sin conexión (precios del ' + cuando + ').', clase: 'listo' };
    }

    function refrescarCatalogo() {
        return Sync.actualizarCatalogo().then(function (r) {
            if (r.ok) {
                var d = describirPreparacion(r.catalogo);
                preparado = d.clase === 'listo';
                setEstado(d.texto, d.clase);
            }
            return r;
        }).catch(function () { return { ok: false }; });
    }

    function subir() {
        return Sync.subirCola({ posInstanceId: posInstanceId }).then(function (r) {
            if (!r || r.enCurso || r.deshabilitado) return r;
            return Db.leer('configOffline').then(function (c) {
                if (c) config = Object.assign(config, c);
                var partes = [];
                if (r.subidas > 0) partes.push('Se subieron ' + r.subidas + ' operaciones hechas sin conexión.');
                if (r.rechazadas > 0 || r.rechazadasTotal > 0) partes.push((r.rechazadasTotal || r.rechazadas) + ' para revisar (ver en el modo sin conexión).');
                if (r.pendientes > 0) partes.push(r.pendientes + ' pendientes de subir' + (r.deOtroVendedor ? ' (de otro vendedor)' : '') + '.');
                (r.avisos || []).forEach(function (a) { partes.push(a); });
                if (partes.length) mostrarResultado(partes);
                return r;
            });
        });
    }

    function mostrarResultado(partes) {
        var cont = document.getElementById('posOfflineResultado');
        if (!cont) return;
        cont.innerHTML = '';
        partes.forEach(function (p) {
            var div = document.createElement('div');
            div.textContent = p;
            cont.appendChild(div);
        });
        cont.hidden = false;
    }

    function chequearRed() {
        return Red.chequear({ segundosTimeout: config.segundosTimeoutPing, urlChequeoInternet: config.urlChequeoInternet }).then(function (r) {
            if (avisoCorte) {
                avisoCorte.hidden = r.online;
                var causa = avisoCorte.querySelector('[data-causa]');
                if (causa) causa.textContent = Red.textoCausa(r.causa);
                var boton = avisoCorte.querySelector('[data-ir-offline]');
                if (boton) boton.hidden = !preparado;
            }
            return r;
        });
    }

    function iniciar() {
        ubicarEnPanel();
        botonOffline.addEventListener('click', irOffline);
        if (avisoCorte) {
            var b = avisoCorte.querySelector('[data-ir-offline]');
            if (b) b.addEventListener('click', irOffline);
        }
        Db.pedirPersistencia();
        registrarServiceWorker().catch(function (e) {
            setEstado('Sin conexión no disponible: ' + e.message + '.', 'pendiente');
        });
        Db.leer('catalogo').then(function (c) {
            var d = describirPreparacion(c);
            preparado = d.clase === 'listo';
            setEstado(d.texto, d.clase);
        }).catch(function (e) { setEstado('Sin conexión no disponible: ' + e.message, 'pendiente'); });

        // Primero subir lo pendiente (trae tambien la configuracion) y despues el catalogo.
        subir().then(refrescarCatalogo).then(function () {
            setInterval(refrescarCatalogo, Math.max(1, config.minutosRefrescoCatalogo || 10) * 60000);
            setInterval(chequearRed, SEGUNDOS_CHEQUEO_RED * 1000);
        });

        window.addEventListener('online', function () { chequearRed().then(function (r) { if (r.online) subir(); }); });
        window.addEventListener('offline', chequearRed);
    }

    iniciar();
})(window, document);
