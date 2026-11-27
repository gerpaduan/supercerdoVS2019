/*
 * POS sin conexion, lado CON CONEXION (2026-11-27). Se carga desde Views/Shared/_PosOffline.cshtml en cualquier
 * pantalla con sesion iniciada (diseños _Layout y _LayoutPOS):
 *  - registra el service worker (sw-pos.js) que despues muestra la pagina offline si el servidor no responde;
 *  - PREPARA la PC (catalogo y usuarios; renueva su vigencia) apenas alguien inicia sesion, al cambiar de sucursal y
 *    cada PosOffline:MinutosRefrescoCatalogo mientras haya una pantalla abierta;
 *  - sube la cola de operaciones hechas sin conexion (al abrir y cada vez que vuelve la red);
 *  - en el POS ademas muestra el indicador (#posOfflineEstado, dentro del panel de la balanza) y, si se corta la red, la
 *    banda con "Seguir vendiendo sin conexión" (no saca al cajero solo: el carrito queda en el borrador del POS).
 * Sin HTTPS valido (contexto no seguro) no hace nada: el navegador no permite trabajar sin conexion y no se guardan claves.
 */
(function (window, document) {
    'use strict';

    var Db = window.PosOfflineDb;
    var Red = window.PosOfflineRed;
    var Sync = window.PosOfflineSync;
    var datos = document.getElementById('posOfflineDatos');
    if (!datos || !Db || !Red || !Sync) return;

    var SEGUNDOS_CHEQUEO_RED = 30;
    var MINUTOS_REFRESCO_DEFAULT = 10;
    // Cada cuanto se mira si ya toca volver a preparar la PC.
    var SEGUNDOS_REVISION = 60;

    var indicador = document.getElementById('posOfflineEstado');
    var avisoCorte = document.getElementById('posOfflineCorte');
    if (!window.isSecureContext || !('serviceWorker' in navigator) || !window.indexedDB) {
        if (indicador) indicador.hidden = true;
        return;
    }

    // Identifica el inicio de sesion (usuario, sucursal y momento): si cambia, hay que preparar la PC enseguida.
    var login = datos.getAttribute('data-login') || '';
    var posInstanceId = datos.getAttribute('data-pos-instance') || '';
    var config = { minutosRefrescoCatalogo: MINUTOS_REFRESCO_DEFAULT, segundosTimeoutPing: 4, urlChequeoInternet: '' };
    var preparado = false;
    var enPanel = false;

    function setEstado(texto, clase) {
        if (!indicador) return;
        indicador.querySelector('[data-texto]').textContent = texto;
        indicador.title = texto;
        indicador.className = 'pos-offline-estado ' + (clase || '') + (enPanel ? ' en-panel' : '');
    }

    // Mueve el indicador y el resultado al panel de la balanza (lugar propio, sin tapar el POS).
    function ubicarEnPanel() {
        var panel = document.querySelector('.pos-balanza-card');
        if (!panel || !indicador) return;
        enPanel = true;
        panel.appendChild(indicador);
        indicador.classList.add('en-panel');
        var resultado = document.getElementById('posOfflineResultado');
        if (resultado) { panel.appendChild(resultado); resultado.classList.add('en-panel'); }
    }

    function irOffline() {
        window.location.href = Red.base() + 'Ventas/POS?modo=offline';
    }

    function registrarServiceWorker() {
        return navigator.serviceWorker.register(datos.getAttribute('data-sw-url'), { scope: datos.getAttribute('data-sw-scope') });
    }

    function mostrarPreparacion(catalogo) {
        var texto = 'Sin conexión: esta PC todavía no está preparada.';
        var clase = 'pendiente';
        preparado = false;
        if (catalogo && catalogo.revisado) {
            if (!catalogo.usuarios || catalogo.usuarios.length === 0) {
                texto = 'Sin conexión: no hay usuarios guardados en esta PC.';
            } else {
                var cuando = new Date(catalogo.revisado).toLocaleString('es-AR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
                texto = 'Listo para vender sin conexión (precios del ' + cuando + ').';
                clase = 'listo';
                preparado = true;
            }
        }
        setEstado(texto, clase);
    }

    function aplicarConfig() {
        return Db.leer('configOffline').then(function (c) { if (c) config = Object.assign(config, c); });
    }

    function mostrarResultado(partes) {
        var cont = document.getElementById('posOfflineResultado');
        if (!cont) {
            // Fuera del POS: aviso discreto que se puede cerrar con un clic.
            cont = document.createElement('div');
            cont.id = 'posOfflineResultado';
            cont.setAttribute('role', 'status');
            cont.style.cssText = 'position:fixed;right:12px;bottom:12px;z-index:1060;max-width:360px;font-size:.78rem;' +
                'background:#fff8e1;color:#212529;border:1px solid #ffe08a;border-radius:6px;padding:8px 10px;cursor:pointer;';
            cont.title = 'Tocá para cerrar';
            cont.addEventListener('click', function () { cont.remove(); });
            document.body.appendChild(cont);
        }
        cont.innerHTML = '';
        partes.forEach(function (p) {
            var div = document.createElement('div');
            div.textContent = p;
            cont.appendChild(div);
        });
        cont.hidden = false;
    }

    // Sube lo pendiente. Solo consulta al servidor si hay algo en la cola.
    function subir() {
        return Db.cola().then(function (cola) {
            if (!cola.some(function (i) { return i.estado === 'pendiente'; })) return null;
            return Sync.subirCola({ posInstanceId: posInstanceId }).then(function (r) {
                if (!r || r.enCurso || r.deshabilitado) return r;
                var partes = [];
                if (r.subidas > 0) partes.push((r.subidas === 1 ? 'Se subió 1 operación hecha' : 'Se subieron ' + r.subidas + ' operaciones hechas') + ' sin conexión.');
                if (r.rechazadasTotal > 0) partes.push(r.rechazadasTotal + ' para revisar (ver en el modo sin conexión).');
                if (r.pendientes > 0) partes.push(r.pendientes + ' pendientes de subir' + (r.deOtraSucursal ? ' (de otra sucursal)' : '') + '.');
                (r.avisos || []).forEach(function (a) { partes.push(a); });
                if (partes.length) mostrarResultado(partes);
                return r;
            });
        });
    }

    // Baja catalogo y usuarios (renueva la vigencia) y recuerda para que inicio de sesion y cuando se hizo.
    function preparar() {
        return Sync.recordarEstado(posInstanceId)
            .then(aplicarConfig)
            .then(Sync.actualizarCatalogo)
            .then(function (r) {
                if (!r.ok) return r;
                mostrarPreparacion(r.catalogo);
                Db.pedirPersistencia();
                return Db.guardar('preparado', { login: login, fecha: Date.now() }).then(function () { return r; });
            });
    }

    // Prepara solo si cambio el inicio de sesion o paso el intervalo (compartido entre pestanas).
    function prepararSiToca() {
        return Db.leer('preparado').then(function (p) {
            var minutos = Math.max(1, config.minutosRefrescoCatalogo || MINUTOS_REFRESCO_DEFAULT);
            if (p && p.login === login && Date.now() - p.fecha < minutos * 60000) return null;
            return preparar();
        });
    }

    function ciclo() {
        return subir().then(prepararSiToca).catch(function (e) {
            console.warn('POS sin conexión: no se pudo preparar la PC', e);
        });
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
        [indicador, avisoCorte].forEach(function (el) {
            var boton = el && el.querySelector('[data-ir-offline]');
            if (boton) boton.addEventListener('click', irOffline);
        });

        registrarServiceWorker().catch(function (e) {
            setEstado('Sin conexión no disponible: ' + e.message + '.', 'pendiente');
        });
        Db.leer('catalogo').then(mostrarPreparacion).catch(function (e) { setEstado('Sin conexión no disponible: ' + e.message, 'pendiente'); });

        aplicarConfig().then(ciclo);
        setInterval(ciclo, SEGUNDOS_REVISION * 1000);
        // El aviso de corte es solo del POS.
        if (avisoCorte) {
            setInterval(chequearRed, SEGUNDOS_CHEQUEO_RED * 1000);
            window.addEventListener('offline', chequearRed);
        }
        window.addEventListener('online', function () { chequearRed().then(function (r) { if (r.online) ciclo(); }); });
    }

    iniciar();
})(window, document);
