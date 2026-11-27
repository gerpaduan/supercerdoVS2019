/*
 * POS sin conexion: reglas de vigencia de lo guardado en la PC (2026-11-26). Funciones puras (se prueban con Node,
 * WebCore.Tests/js/pos-offline-vigencia.test.js).
 *  - La PC puede vender sin conexion 'diasVigencia' dias desde la ultima vez que se actualizo con una sesion abierta
 *    (cada inicio de sesion la renueva). Pasados, se borran las claves guardadas y no deja vender; la cola se conserva.
 *  - Si el reloj de la PC aparece atrasado respecto de lo ya visto, se bloquea hasta volver a conectarse (si no,
 *    atrasar la fecha estiraria la vigencia). Tolerancia de 1 hora para ajustes normales del reloj.
 *  - Precios mas viejos que 'horasAvisoPrecios' se avisan en rojo (no bloquea).
 *  - La clave de un administrador solo llega cuando inicia sesion el mismo: al actualizar con otra sesion se conserva la
 *    que ya estaba en la PC mientras el servidor la siga dando por vigente (misma version).
 */
(function (root) {
    'use strict';

    var MS_DIA = 86400000;
    var MS_HORA = 3600000;
    var TOLERANCIA_RELOJ_MS = MS_HORA;
    var DIAS_DEFAULT = 15;
    var HORAS_AVISO_DEFAULT = 24;

    // catalogo: lo guardado en IndexedDB (con 'revisado' ISO). ultimoVisto / ahora: milisegundos (reloj de la PC).
    // Devuelve { vigente, motivo: ''|'sinDatos'|'vencida'|'reloj', diasRestantes, preciosViejos, revisado }.
    function evaluar(catalogo, ultimoVisto, ahora) {
        var revisado = catalogo && catalogo.revisado ? Date.parse(catalogo.revisado) : NaN;
        if (!catalogo || !catalogo.productos || isNaN(revisado)) return { vigente: false, motivo: 'sinDatos' };

        var referencia = Math.max(revisado, Number(ultimoVisto) || 0);
        if (ahora < referencia - TOLERANCIA_RELOJ_MS) return { vigente: false, motivo: 'reloj', revisado: revisado };

        var dias = Number(catalogo.diasVigencia) || DIAS_DEFAULT;
        var vence = revisado + dias * MS_DIA;
        if (ahora >= vence) return { vigente: false, motivo: 'vencida', revisado: revisado };

        var horasAviso = Number(catalogo.horasAvisoPrecios) || HORAS_AVISO_DEFAULT;
        return {
            vigente: true,
            motivo: '',
            diasRestantes: Math.ceil((vence - ahora) / MS_DIA),
            preciosViejos: ahora - revisado > horasAviso * MS_HORA,
            revisado: revisado
        };
    }

    // Usuarios a guardar al recibir un catalogo nuevo: los que manda el servidor, mas los administradores que ya estaban
    // en la PC (de la misma empresa) y siguen vigentes con la misma version de clave.
    function combinarUsuarios(anterior, nuevo) {
        var usuarios = ((nuevo && nuevo.usuarios) || []).slice();
        if (!anterior || !nuevo || anterior.idEmpresa !== nuevo.idEmpresa) return usuarios;

        var vigentes = {};
        (nuevo.adminsVigentes || []).forEach(function (a) { vigentes[a.id] = a.version; });
        (anterior.usuarios || []).forEach(function (u) {
            var yaEsta = usuarios.some(function (n) { return n.id === u.id; });
            if (u.admin && !yaEsta && vigentes[u.id] === u.version) usuarios.push(u);
        });
        return usuarios;
    }

    // Copia del catalogo sin ninguna clave guardada (vigencia vencida). version vacia: la proxima actualizacion baja todo
    // de nuevo (si no, el servidor responderia "sin cambios" y los usuarios no volverian).
    function sinClaves(catalogo) {
        var copia = {};
        Object.keys(catalogo || {}).forEach(function (k) { copia[k] = catalogo[k]; });
        copia.usuarios = [];
        copia.adminsVigentes = [];
        copia.version = '';
        return copia;
    }

    var TEXTOS = {
        sinDatos: 'Esta PC todavía no tiene datos para vender sin conexión: alguien tiene que iniciar sesión con internet.',
        vencida: 'Venció el permiso para vender sin conexión: pasaron demasiados días sin que nadie inicie sesión con internet en esta PC.',
        reloj: 'La fecha u hora de esta PC está atrasada. Corregila, o conectate a internet e iniciá sesión.'
    };

    var api = {
        evaluar: evaluar,
        combinarUsuarios: combinarUsuarios,
        sinClaves: sinClaves,
        texto: function (motivo) { return TEXTOS[motivo] || ''; }
    };
    root.PosOfflineVigencia = api;
    if (typeof module !== 'undefined' && module.exports) module.exports = api;
})(typeof window !== 'undefined' ? window : globalThis);
