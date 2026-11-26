/*
 * POS sin conexion: almacenamiento en la PC (IndexedDB, base 'carnisys-pos-offline'). Lo usan el POS normal (para
 * preparar el modo offline y subir la cola) y la pagina sin conexion (pos-offline.html).
 *  - kv:   clave/valor. 'catalogo' (lo que devuelve Ventas/CatalogoOffline + 'bajado'), 'sesion' (quien entro con PIN),
 *          'cajas' (estado de caja por vendedor), 'intentosPin', 'ultimaSubida'.
 *  - cola: operaciones hechas sin conexion que faltan subir, por clientId. tipo 'caja' (apertura) o 'venta'.
 *          estado 'pendiente' o 'rechazada' (el servidor no la acepto tal cual: queda para revisar, nunca se borra sola).
 * Si IndexedDB no esta disponible (navegador privado, datos bloqueados) todas las operaciones rechazan con un Error:
 * quien llama muestra "esta PC no puede trabajar sin conexion".
 */
(function (root) {
    'use strict';

    var NOMBRE_BASE = 'carnisys-pos-offline';
    var VERSION_BASE = 1;
    var basePromesa = null;

    function abrir() {
        if (basePromesa) return basePromesa;
        basePromesa = new Promise(function (resolve, reject) {
            if (!root.indexedDB) {
                reject(new Error('Este navegador no permite guardar datos para trabajar sin conexión.'));
                return;
            }
            var pedido = root.indexedDB.open(NOMBRE_BASE, VERSION_BASE);
            pedido.onupgradeneeded = function () {
                var db = pedido.result;
                if (!db.objectStoreNames.contains('kv')) db.createObjectStore('kv');
                if (!db.objectStoreNames.contains('cola')) {
                    var cola = db.createObjectStore('cola', { keyPath: 'clientId' });
                    cola.createIndex('orden', 'orden');
                }
            };
            pedido.onsuccess = function () { resolve(pedido.result); };
            pedido.onerror = function () { reject(pedido.error || new Error('No se pudo abrir el almacenamiento local.')); };
            pedido.onblocked = function () { reject(new Error('El almacenamiento local está en uso por otra pestaña.')); };
        });
        // Si falla, el proximo intento vuelve a probar (no se cachea el rechazo).
        basePromesa.catch(function () { basePromesa = null; });
        return basePromesa;
    }

    // Ejecuta fn(store) en una transaccion y resuelve con el resultado del IDBRequest que devuelva fn (o undefined).
    function enTransaccion(nombreStore, modo, fn) {
        return abrir().then(function (db) {
            return new Promise(function (resolve, reject) {
                var tx = db.transaction(nombreStore, modo);
                var store = tx.objectStore(nombreStore);
                var pedido = fn(store);
                var resultado;
                if (pedido) pedido.onsuccess = function () { resultado = pedido.result; };
                tx.oncomplete = function () { resolve(resultado); };
                tx.onerror = function () { reject(tx.error || new Error('Error del almacenamiento local.')); };
                tx.onabort = function () { reject(tx.error || new Error('Se canceló la operación del almacenamiento local.')); };
            });
        });
    }

    var api = {
        leer: function (clave) {
            return enTransaccion('kv', 'readonly', function (s) { return s.get(clave); });
        },
        guardar: function (clave, valor) {
            return enTransaccion('kv', 'readwrite', function (s) { return s.put(valor, clave); });
        },
        borrar: function (clave) {
            return enTransaccion('kv', 'readwrite', function (s) { return s.delete(clave); });
        },

        // Agrega una operacion a la cola. orden = momento de alta (las aperturas de caja van antes que las ventas al subir).
        encolar: function (item) {
            if (!item || !item.clientId) return Promise.reject(new Error('Operación sin código.'));
            item.estado = item.estado || 'pendiente';
            item.orden = item.orden || Date.now();
            return enTransaccion('cola', 'readwrite', function (s) { return s.add(item); });
        },
        actualizarItem: function (item) {
            return enTransaccion('cola', 'readwrite', function (s) { return s.put(item); });
        },
        quitarItem: function (clientId) {
            return enTransaccion('cola', 'readwrite', function (s) { return s.delete(clientId); });
        },
        // Toda la cola, en orden de alta.
        cola: function () {
            return enTransaccion('cola', 'readonly', function (s) { return s.index('orden').getAll(); })
                .then(function (items) { return items || []; });
        },

        // Solicita que el navegador no borre estos datos si le falta espacio (la app instalada suele obtenerlo).
        pedirPersistencia: function () {
            if (root.navigator && root.navigator.storage && root.navigator.storage.persist) {
                return root.navigator.storage.persist().catch(function () { return false; });
            }
            return Promise.resolve(false);
        }
    };

    root.PosOfflineDb = api;
})(typeof window !== 'undefined' ? window : globalThis);
