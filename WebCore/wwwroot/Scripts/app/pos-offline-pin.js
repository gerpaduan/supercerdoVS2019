/*
 * POS sin conexion: ingreso sin servidor con la contrasena de siempre o con el PIN (2026-11-27, decision del usuario).
 * Los datos guardados traen, por usuario, la huella de su clave y de su PIN que guarda el servidor
 * (Utilidades.PasswordSecurity: PBKDF2-SHA256, sal de 16 bytes, hash de 32 bytes, en base64) y aca se verifica con
 * WebCrypto. La clave y el PIN nunca se guardan ni viajan.
 * Intentos: MAX_INTENTOS fallos seguidos bloquean a ese usuario MINUTOS_BLOQUEO en esta PC (se guarda en IndexedDB,
 * asi recargar la pagina no lo resetea). Con internet rige el bloqueo de siempre del servidor.
 */
(function (root) {
    'use strict';

    var MAX_INTENTOS = 5;
    var MINUTOS_BLOQUEO = 5;
    var ITERACIONES_DEFAULT = 100000;
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
    function hashClave(texto, salBase64, iteraciones) {
        var subtle = root.crypto && root.crypto.subtle;
        if (!subtle) return Promise.reject(new Error('Este navegador no permite verificar la clave sin conexión (hace falta HTTPS).'));
        return subtle.importKey('raw', new TextEncoder().encode(texto), 'PBKDF2', false, ['deriveBits'])
            .then(function (clave) {
                return subtle.deriveBits({ name: 'PBKDF2', hash: 'SHA-256', salt: base64ABytes(salBase64), iterations: iteraciones }, clave, 256);
            })
            .then(bytesABase64);
    }

    function tieneFormaDePin(pin) {
        return typeof pin === 'string' && pin.length >= LARGO_MIN && pin.length <= LARGO_MAX && /^[0-9]+$/.test(pin);
    }

    // true si 'texto' es el PIN del usuario o su contrasena. El PIN solo se prueba si el texto tiene forma de PIN.
    function coincide(usuario, texto) {
        var probarPin = usuario.pinHash && usuario.pinSalt && tieneFormaDePin(texto)
            ? hashClave(texto, usuario.pinSalt, usuario.pinIteraciones || ITERACIONES_DEFAULT)
                .then(function (h) { return iguales(h, usuario.pinHash); })
            : Promise.resolve(false);
        return probarPin.then(function (ok) {
            if (ok || !usuario.claveHash || !usuario.claveSalt) return ok;
            return hashClave(texto, usuario.claveSalt, usuario.claveIteraciones || ITERACIONES_DEFAULT)
                .then(function (h) { return iguales(h, usuario.claveHash); });
        });
    }

    function tieneCredencial(usuario) {
        return !!(usuario && ((usuario.claveHash && usuario.claveSalt) || (usuario.pinHash && usuario.pinSalt)));
    }

    // db: PosOfflineDb (o un doble de test con leer/guardar). Resuelve { ok, usuario?, mensaje? }.
    function verificar(usuario, texto, db, ahora) {
        ahora = ahora || Date.now();
        if (!tieneCredencial(usuario)) {
            return Promise.resolve({ ok: false, mensaje: 'Ese usuario no tiene clave guardada en esta PC: tiene que iniciar sesión con internet.' });
        }

        return db.leer('intentosPin').then(function (todos) {
            todos = todos || {};
            var registro = todos[usuario.id] || { fallos: 0, bloqueadoHasta: 0 };
            if (registro.bloqueadoHasta > ahora) {
                var min = Math.ceil((registro.bloqueadoHasta - ahora) / 60000);
                return { ok: false, mensaje: 'Demasiados intentos. Esperá ' + min + ' min o entrá con internet.' };
            }
            if (!texto) return { ok: false, mensaje: 'Escribí tu contraseña o tu PIN.' };

            return coincide(usuario, texto).then(function (ok) {
                if (ok) {
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
                            ? 'Contraseña o PIN incorrecto. Te quedan ' + restantes + ' intentos.'
                            : 'Contraseña o PIN incorrecto. Usuario bloqueado ' + MINUTOS_BLOQUEO + ' min en esta PC.'
                    };
                });
            });
        });
    }

    var api = { verificar: verificar, hashClave: hashClave, MAX_INTENTOS: MAX_INTENTOS, MINUTOS_BLOQUEO: MINUTOS_BLOQUEO };
    root.PosOfflinePin = api;
    if (typeof module !== 'undefined' && module.exports) module.exports = api;
})(typeof window !== 'undefined' ? window : globalThis);
