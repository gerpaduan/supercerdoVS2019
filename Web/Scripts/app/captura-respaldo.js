// Respaldo automatico por captura de pantalla al agregar/quitar una linea, en Compras/Movimientos/
// Elaborados/Stock -- replica en Web la funcionalidad que ya existe en WinForms
// (Utilidades/Util_Form.cs, capturarPantalla), pero sin subir nada a un servidor: el navegador
// pide permiso de escritura sobre una carpeta UNA sola vez (File System Access API, requiere un
// click real del usuario) y de ahi en mas escribe ahi solo, en silencio, en cada agregar/quitar
// linea -- pensado como respaldo de recuperacion si la PC se apaga o el sistema se cierra
// inesperado a mitad de una edicion (mismo espiritu que la version de WinForms, que tampoco
// documenta el motivo pero es evidente por el mecanismo). Solo funciona en navegadores Chromium
// (Chrome/Edge, que son los unicos con la File System Access API): en el resto, el boton de
// activar no aparece y la pantalla sigue funcionando igual, sin respaldo -- degradacion total,
// nunca rompe el flujo de edicion.
//
// El handle de la carpeta elegida se guarda en IndexedDB (es el unico storage del navegador que
// soporta persistir un FileSystemDirectoryHandle -- localStorage solo guarda strings) bajo una
// clave fija, COMPARTIDA entre las 4 pantallas: el usuario la elige una sola vez en cualquiera de
// ellas y las otras 3 la reconocen sola.
(function (window) {
    'use strict';

    var DB_NAME = 'CarniSysCapturaRespaldo';
    var STORE_NAME = 'handles';
    var HANDLE_KEY = 'carpetaRespaldo';

    function disponible() {
        return typeof window.showDirectoryPicker === 'function' && typeof window.indexedDB !== 'undefined';
    }

    function abrirDb() {
        return new Promise(function (resolve, reject) {
            var req = window.indexedDB.open(DB_NAME, 1);
            req.onupgradeneeded = function () {
                req.result.createObjectStore(STORE_NAME);
            };
            req.onsuccess = function () { resolve(req.result); };
            req.onerror = function () { reject(req.error); };
        });
    }

    async function leerHandleGuardado() {
        var db = await abrirDb();
        return new Promise(function (resolve, reject) {
            var tx = db.transaction(STORE_NAME, 'readonly');
            var req = tx.objectStore(STORE_NAME).get(HANDLE_KEY);
            req.onsuccess = function () { resolve(req.result || null); };
            req.onerror = function () { reject(req.error); };
        });
    }

    async function guardarHandle(handle) {
        var db = await abrirDb();
        return new Promise(function (resolve, reject) {
            var tx = db.transaction(STORE_NAME, 'readwrite');
            tx.objectStore(STORE_NAME).put(handle, HANDLE_KEY);
            tx.oncomplete = function () { resolve(); };
            tx.onerror = function () { reject(tx.error); };
        });
    }

    // Handle guardado + permiso de escritura vigente, o null si no esta activado / el permiso
    // caduco. No re-pide permiso: eso solo se puede hacer desde un click real del usuario (activar()).
    async function obtenerHandleConPermiso() {
        if (!disponible()) return null;
        try {
            var handle = await leerHandleGuardado();
            if (!handle) return null;
            var estado = await handle.queryPermission({ mode: 'readwrite' });
            return estado === 'granted' ? handle : null;
        } catch (e) {
            return null;
        }
    }

    async function estaActivado() {
        var handle = await obtenerHandleConPermiso();
        return handle != null;
    }

    // Debe llamarse desde el handler de un click real -- showDirectoryPicker() tira
    // SecurityError si no hay gesto de usuario en curso.
    async function activar() {
        if (!disponible()) throw new Error('File System Access API no disponible en este navegador.');
        var handle = await window.showDirectoryPicker({ id: 'carnisys-capturas', mode: 'readwrite' });
        await guardarHandle(handle);
        return handle;
    }

    function pad2(n) { return n < 10 ? '0' + n : '' + n; }

    // Mismo formato que WinForms (Util_Form.cs): "dd-MM-yyyy HHmmss".
    function timestamp(fecha) {
        return pad2(fecha.getDate()) + '-' + pad2(fecha.getMonth() + 1) + '-' + fecha.getFullYear()
            + ' ' + pad2(fecha.getHours()) + pad2(fecha.getMinutes()) + pad2(fecha.getSeconds());
    }

    // Estado por pantalla (etiqueta), en memoria -- se resetea solo en cada carga de pagina, que
    // es la granularidad correcta: una carpeta por visita a la pantalla (una nueva compra, un
    // movimiento, etc.), no una carpeta nueva por cada captura individual.
    var sesiones = {};

    function obtenerSesion(etiqueta) {
        if (!sesiones[etiqueta]) {
            sesiones[etiqueta] = {
                carpeta: timestamp(new Date()) + ' - ' + etiqueta,
                contador: 0,
                timer: null
            };
        }
        return sesiones[etiqueta];
    }

    async function ejecutarCaptura(etiqueta, sesion) {
        if (typeof window.html2canvas !== 'function') return;

        var dirHandle = await obtenerHandleConPermiso();
        if (!dirHandle) return; // no activado o permiso caducado -- se omite en silencio, sin interrumpir al usuario

        // El numero se asigna aca, recien cuando la captura efectivamente se va a ejecutar (no al
        // llamar capturar()) -- asi refleja capturas reales, no eventos crudos que el debounce de
        // abajo termino colapsando en una sola.
        sesion.contador += 1;
        var nombreArchivo = etiqueta + ' (' + sesion.contador + ') - ' + timestamp(new Date());

        // windowWidth/windowHeight = alto/ancho del DOCUMENTO completo, no del viewport -- el
        // pedido explicito del usuario era capturar campos que quedan fuera de la altura de
        // pantalla, no solo lo visible.
        var canvas = await window.html2canvas(document.documentElement, {
            windowWidth: document.documentElement.scrollWidth,
            windowHeight: document.documentElement.scrollHeight,
            useCORS: true
        });

        var blob = await new Promise(function (resolve) {
            canvas.toBlob(function (b) { resolve(b); }, 'image/png');
        });
        if (!blob) return;

        var subDir = await dirHandle.getDirectoryHandle(sesion.carpeta, { create: true });
        var fileHandle = await subDir.getFileHandle(nombreArchivo + '.png', { create: true });
        var writable = await fileHandle.createWritable();
        await writable.write(blob);
        await writable.close();
    }

    // Punto de entrada llamado desde cada pantalla al agregar/quitar una linea. Debounce de 5s:
    // cada llamada reinicia el timer pendiente -- si hay una rafaga de agregados/quitados mas
    // rapida que eso (ej. una carga automatica de varios productos seguidos), no se genera una
    // captura por cada uno, se espera a que se aquiete y se genera una sola para todo el grupo.
    // En uso normal (acciones espaciadas por mas de 5s) cada una termina generando su propia
    // captura igual, solo que con ~5s de demora -- invisible para el usuario, la captura ya era
    // "por detras" sin bloquear la UI. Cualquier error se traga en silencio (mismo criterio que el
    // catch vacio de Util_Form.capturarPantalla en WinForms: un respaldo que falla no debe
    // interrumpir el flujo de edicion del usuario).
    var DEBOUNCE_MS = 5000;

    function capturar(etiqueta) {
        var sesion = obtenerSesion(etiqueta);
        if (sesion.timer) {
            window.clearTimeout(sesion.timer);
        }
        sesion.timer = window.setTimeout(function () {
            sesion.timer = null;
            ejecutarCaptura(etiqueta, sesion).catch(function () { });
        }, DEBOUNCE_MS);
    }

    // Wiring del botoncito "Activar respaldo automatico" / etiqueta "activado", identico en las
    // 4 pantallas (mismos 2 ids de markup en cada vista) -- se centraliza aca para no repetir la
    // misma logica de inicializacion 4 veces.
    function iniciarUI() {
        if (typeof window.jQuery === 'undefined') return;
        var $ = window.jQuery;
        var $btn = $('#btnActivarCapturaRespaldo');
        var $activo = $('#lblCapturaRespaldoActiva');
        if (!$btn.length && !$activo.length) return;

        if (!disponible()) return; // navegador sin soporte: ninguno de los 2 se muestra

        estaActivado().then(function (activo) {
            if (activo) $activo.removeClass('d-none');
            else $btn.removeClass('d-none');
        });

        $btn.on('click', function () {
            activar().then(function () {
                $btn.addClass('d-none');
                $activo.removeClass('d-none');
            }).catch(function () {
                // el usuario cerro el selector de carpeta sin elegir nada, o lo nego -- no hacer nada
            });
        });
    }

    window.CapturaRespaldo = {
        disponible: disponible,
        estaActivado: estaActivado,
        activar: activar,
        capturar: capturar,
        iniciarUI: iniciarUI
    };
})(window);
