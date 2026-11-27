/*
 * POS sin conexion: subida de la cola y actualizacion del catalogo (2026-11-26). Lo usan el POS normal (al abrir y cada
 * vez que vuelve la conexion) y la pagina sin conexion (cuando detecta que volvio la red).
 * Reglas:
 *  - Se sube de a una operacion, en orden: primero las aperturas de caja y despues las ventas, para que cada venta caiga
 *    dentro de su caja por fecha.
 *  - Cualquier usuario con sesion sube las de todos los vendedores de la sucursal en la que esta trabajando; las de otra
 *    sucursal se saltean y esperan.
 *  - ok -> se quita de la cola. rechazada -> queda marcada para revisar (nunca se borra sola).
 *    Error de red o del servidor -> se corta y se reintenta despues.
 *  - Sin sesion (la cookie vencio) el servidor redirige al login: se informa sinSesion y no se toca la cola.
 *  - Una sola subida a la vez entre pestanas (navigator.locks).
 *  - Actualizar el catalogo con sesion renueva la vigencia de la PC (ver pos-offline-vigencia.js).
 */
(function (root) {
    'use strict';

    var Db = function () { return root.PosOfflineDb; };
    var Red = function () { return root.PosOfflineRed; };
    var Vig = function () { return root.PosOfflineVigencia; };
    var CANDADO_SUBIDA = 'pos-offline-subida';
    var subiendo = false;

    // GET JSON. Distingue "no hay sesion" (redireccion al login / HTML) de error de red.
    function getJson(url) {
        return root.fetch(url, { credentials: 'same-origin', cache: 'no-store', headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (r) {
                var tipo = r.headers.get('content-type') || '';
                if (r.redirected || tipo.indexOf('json') < 0) return { sinSesion: true };
                if (!r.ok) return { errorHttp: r.status };
                return r.json();
            });
    }

    function postJson(url, cuerpo, token) {
        return root.fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            cache: 'no-store',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token || '', 'X-Requested-With': 'XMLHttpRequest' },
            body: JSON.stringify(cuerpo)
        }).then(function (r) {
            var tipo = r.headers.get('content-type') || '';
            if (r.redirected || tipo.indexOf('json') < 0) return { sinSesion: true };
            return r.json().then(function (j) { if (!r.ok) j.errorHttp = r.status; return j; });
        });
    }

    // Baja el catalogo si cambio y renueva la vigencia. Resuelve { ok, cambio, catalogo } o { ok:false, respuesta }.
    function actualizarCatalogo() {
        var base = Red().base();
        return Db().leer('catalogo').then(function (actual) {
            var version = actual && actual.version ? actual.version : '';
            return getJson(base + 'Ventas/CatalogoOffline?version=' + encodeURIComponent(version)).then(function (r) {
                if (!r || r.sinSesion || r.deshabilitado || r.errorHttp || r.ok === false) return { ok: false, respuesta: r };
                var momento = new Date();
                var ahora = momento.toISOString();
                var cambio = !(r.sinCambios && actual);
                var catalogo = actual;
                if (cambio) {
                    r.usuarios = Vig().combinarUsuarios(actual, r);
                    r.bajado = ahora;
                    catalogo = r;
                }
                catalogo.revisado = ahora;
                // El reloj visto vuelve a empezar desde esta conexion (destraba una PC que tenia la fecha adelantada).
                return Db().guardar('catalogo', catalogo)
                    .then(function () { return Db().guardar('ultimoVisto', momento.getTime()); })
                    .then(function () { return { ok: true, cambio: cambio, catalogo: catalogo }; });
            });
        });
    }

    // Guarda el estado de caja que informa el servidor para el operador (lo usa la pagina offline al entrar con PIN).
    // Tambien guarda la configuracion (timeouts, URL de chequeo de internet) para que la pagina offline la tenga sin red.
    function recordarCaja(estado) {
        if (!estado) return Promise.resolve();
        var guardarConfig = estado.config ? Db().guardar('configOffline', estado.config) : Promise.resolve();
        if (!estado.operador) return guardarConfig;
        return guardarConfig.then(function () { return Db().leer('cajas'); }).then(function (cajas) {
            cajas = cajas || {};
            cajas[claveCaja(estado.sucursal && estado.sucursal.id, estado.operador.id)] = estado.caja
                ? { abierta: true, idCierre: estado.caja.id, fechaInicio: estado.caja.fechaInicio, offline: false, informado: new Date().toISOString() }
                : { abierta: false, informado: new Date().toISOString() };
            return Db().guardar('cajas', cajas);
        });
    }

    // La caja guardada en la PC es por sucursal y vendedor.
    function claveCaja(idSucursal, idVendedor) {
        return (idSucursal || 0) + ':' + idVendedor;
    }

    function marcar(item, cambios) {
        Object.keys(cambios).forEach(function (k) { item[k] = cambios[k]; });
        return Db().actualizarItem(item);
    }

    // Trae del servidor la configuracion y la caja del operador y las guarda (la pagina offline las usa sin red).
    function recordarEstado(posInstanceId) {
        return getJson(Red().base() + 'Ventas/EstadoPosOffline?posInstanceId=' + encodeURIComponent(posInstanceId || ''))
            .then(function (estado) {
                if (!estado || estado.ok !== true) return null;
                return recordarCaja(estado).then(function () { return estado; });
            });
    }

    // Sube la cola. opciones.posInstanceId: pestana del POS (cuenta de produccion). Resuelve un resumen:
    // { subidas, rechazadas, pendientes, deOtraSucursal, corte (motivo si se corto), avisos: [...], sinSesion, deshabilitado,
    //   enCurso (otra pestana la esta subiendo) }.
    function subirCola(opciones) {
        if (subiendo) return Promise.resolve({ enCurso: true });
        subiendo = true;
        var locks = root.navigator && root.navigator.locks;
        var tarea = locks && locks.request
            ? locks.request(CANDADO_SUBIDA, { ifAvailable: true }, function (candado) {
                return candado ? subirSinCandado(opciones || {}) : { enCurso: true };
            })
            : subirSinCandado(opciones || {});
        return Promise.resolve(tarea)
            .then(function (r) { subiendo = false; return r; }, function (e) { subiendo = false; throw e; });
    }

    function subirSinCandado(opciones) {
        var base = Red().base();
        var resumen = { subidas: 0, rechazadas: 0, pendientes: 0, deOtraSucursal: 0, avisos: [], corte: null };

        return getJson(base + 'Ventas/EstadoPosOffline?posInstanceId=' + encodeURIComponent(opciones.posInstanceId || ''))
            .then(function (estado) {
                if (!estado || estado.sinSesion) { resumen.sinSesion = true; return resumen; }
                if (estado.deshabilitado) { resumen.deshabilitado = true; return resumen; }
                if (estado.errorHttp || estado.ok === false) { resumen.corte = 'error'; return resumen; }

                return recordarCaja(estado).then(function () { return Db().cola(); }).then(function (cola) {
                    var pendientes = cola.filter(function (i) { return i.estado === 'pendiente'; });
                    // Aperturas primero (orden de alta dentro de cada tipo).
                    pendientes.sort(function (a, b) {
                        if (a.tipo !== b.tipo) return a.tipo === 'caja' ? -1 : 1;
                        return a.orden - b.orden;
                    });
                    var idSucursal = estado.sucursal ? estado.sucursal.id : 0;

                    function siguiente(i) {
                        if (i >= pendientes.length) return Promise.resolve(resumen);
                        var item = pendientes[i];
                        if (item.idSucursal !== idSucursal) {
                            resumen.deOtraSucursal++;
                            return siguiente(i + 1);
                        }
                        var url = base + (item.tipo === 'caja' ? 'Ventas/SincronizarAperturaCajaOffline' : 'Ventas/SincronizarVentaOffline');
                        var cuerpo = Object.assign({}, item.pedido, { posInstanceId: opciones.posInstanceId || '' });
                        return postJson(url, cuerpo, estado.token).then(function (r) {
                            if (r && r.ok) {
                                resumen.subidas++;
                                if (r.llegoTarde) resumen.avisos.push('Venta del ' + item.fechaTexto + ': llegó con la caja ya cerrada.');
                                if (r.sinCaja) resumen.avisos.push('Venta del ' + item.fechaTexto + ': no cayó en ninguna caja.');
                                if (r.pendienteFacturar) resumen.avisos.push('Venta del ' + item.fechaTexto + ' (' + (item.pedido.formaPago || '') + '): queda para facturar.');
                                if (r.vendedorNoHabilitado) resumen.avisos.push('Operación del ' + item.fechaTexto + ' de ' + (item.nombreVendedor || 'un vendedor') + ': ese usuario ya no está habilitado; se avisó al administrador.');
                                return Db().quitarItem(item.clientId).then(function () { return siguiente(i + 1); });
                            }
                            if (r && r.rechazada) {
                                resumen.rechazadas++;
                                return marcar(item, { estado: 'rechazada', motivo: r.msg || 'Rechazada por el servidor.' })
                                    .then(function () { return siguiente(i + 1); });
                            }
                            if (r && r.otraSucursal) {
                                resumen.deOtraSucursal++;
                                return siguiente(i + 1);
                            }
                            // Sin sesion o fallo del servidor: se corta y se reintenta despues.
                            resumen.corte = r && r.sinSesion ? 'sinSesion' : 'error';
                            if (r && r.msg) resumen.avisos.push(r.msg);
                            return resumen;
                        }, function () {
                            resumen.corte = 'red';
                            return resumen;
                        });
                    }
                    return siguiente(0);
                });
            })
            .then(function (r) {
                return Db().cola().then(function (cola) {
                    r.pendientes = cola.filter(function (i) { return i.estado === 'pendiente'; }).length;
                    r.rechazadasTotal = cola.filter(function (i) { return i.estado === 'rechazada'; }).length;
                    if (r.subidas > 0) return Db().guardar('ultimaSubida', { fecha: new Date().toISOString(), subidas: r.subidas, avisos: r.avisos }).then(function () { return r; });
                    return r;
                });
            })
            .catch(function (e) {
                resumen.corte = 'red';
                resumen.error = e && e.message;
                return resumen;
            });
    }

    root.PosOfflineSync = {
        actualizarCatalogo: actualizarCatalogo,
        subirCola: subirCola,
        recordarEstado: recordarEstado,
        claveCaja: claveCaja
    };
})(typeof window !== 'undefined' ? window : globalThis);
