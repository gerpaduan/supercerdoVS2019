/*
 * POS sin conexion: login con PIN sin servidor (2026-11-26, decision del usuario: todos los PIN de la sucursal, y los
 * admins con un PIN que solo sirve sin conexion). El catalogo trae, por usuario, el hash del PIN que guarda el servidor
 * (Utilidades.PasswordSecurity: PBKDF2-SHA256, sal de 16 bytes, hash de 32 bytes, en base64) y aca se verifica con
 * WebCrypto. El PIN nunca se guarda ni viaja.
 * Intentos: MAX_INTENTOS fallos seguidos bloquean a ese usuario MINUTOS_BLOQUEO en esta PC (se guarda en IndexedDB,
 * asi recargar la pagina no lo resetea). Con internet rige el bloqueo de siempre del servidor.
 */
(function (root) {
    'use strict';

    var MAX_INTENTOS = 5;
    var MINUTOS_BLOQUEO = 5;
    var LARGO_MIN = 4;
    var LARGO_MAX = 6;

    function base64ABytes(b64) {
        var bin = root.atob(b64 || '');
        var bytes = new Uint8Array(bin.length);
        for (var i = 0; i < bin.length; i++) bytes[i] = bin.charCodeAt(i);
        return bytes;
    }

    function bytesABase64(buffer) {
        var bytes = new Uint8Array(buffer);
        var bin = '';
        for (var i = 0; i < bytes.length; i++) bin += String.fromCharCode(bytes[i]);
        return root.btoa(bin);
    }

    // Comparacion sin cortar en la primera diferencia.
    function iguales(a, b) {
        if (typeof a !== 'string' || typeof b !== 'string' || a.length !== b.length) return false;
        var dif = 0;
        for (var i = 0; i < a.length; i++) dif |= a.charCodeAt(i) ^ b.charCodeAt(i);
        return dif === 0;
    }

    // Mismo calculo que PasswordSecurity.VerifyPassword: Rfc2898DeriveBytes(texto UTF-8, sal, iteraciones, SHA256), 32 bytes.
    function hashPin(pin, salBase64, iteraciones) {
        var subtle = root.crypto && root.crypto.subtle;
        if (!subtle) return Promise.reject(new Error('Este navegador no permite verificar el PIN sin conexión (hace falta HTTPS).'));
        return subtle.importKey('raw', new TextEncoder().encode(pin), 'PBKDF2', false, ['deriveBits'])
            .then(function (clave) {
                return subtle.deriveBits({ name: 'PBKDF2', hash: 'SHA-256', salt: base64ABytes(salBase64), iterations: iteraciones }, clave, 256);
            })
            .then(bytesABase64);
    }

    function tieneFormaDePin(pin) {
        return typeof pin === 'string' && pin.length >= LARGO_MIN && pin.length <= LARGO_MAX && /^[0-9]+$/.test(pin);
    }

    // db: PosOfflineDb (o un doble de test con leer/guardar). Resuelve { ok, usuario?, mensaje? }.
    function verificar(usuario, pin, db, ahora) {
        ahora = ahora || Date.now();
        if (!usuario || !usuario.pinHash || !usuario.pinSalt) return Promise.resolve({ ok: false, mensaje: 'Ese usuario no tiene PIN en esta PC.' });

        return db.leer('intentosPin').then(function (todos) {
            todos = todos || {};
            var registro = todos[usuario.id] || { fallos: 0, bloqueadoHasta: 0 };
            if (registro.bloqueadoHasta > ahora) {
                var min = Math.ceil((registro.bloqueadoHasta - ahora) / 60000);
                return { ok: false, mensaje: 'Demasiados intentos. Esperá ' + min + ' min o entrá con internet.' };
            }
            if (!tieneFormaDePin(pin)) return { ok: false, mensaje: 'El PIN tiene de 4 a 6 dígitos.' };

            return hashPin(pin, usuario.pinSalt, usuario.pinIteraciones || 100000).then(function (calculado) {
                if (iguales(calculado, usuario.pinHash)) {
                    delete todos[usuario.id];
                    return db.guardar('intentosPin', todos).then(function () { return { ok: true, usuario: usuario }; });
                }
                registro.fallos += 1;
                var restantes = MAX_INTENTOS - registro.fallos;
                if (restantes <= 0) {
                    registro = { fallos: 0, bloqueadoHasta: ahora + MINUTOS_BLOQUEO * 60000 };
                }
                todos[usuario.id] = registro;
                return db.guardar('intentosPin', todos).then(function () {
                    return {
                        ok: false,
                        mensaje: restantes > 0
                            ? 'PIN incorrecto. Te quedan ' + restantes + ' intentos.'
                            : 'PIN incorrecto. Usuario bloqueado ' + MINUTOS_BLOQUEO + ' min en esta PC.'
                    };
                });
            });
        });
    }

    var api = { verificar: verificar, hashPin: hashPin, MAX_INTENTOS: MAX_INTENTOS, MINUTOS_BLOQUEO: MINUTOS_BLOQUEO };
    root.PosOfflinePin = api;
    if (typeof module !== 'undefined' && module.exports) module.exports = api;
})(typeof window !== 'undefined' ? window : globalThis);
