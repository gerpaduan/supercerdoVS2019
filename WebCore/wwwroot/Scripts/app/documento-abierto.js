// Aviso de "documento ya abierto en otra pestaña" (2026-11-12, ver docs/DECISIONS.md "Documento abierto en otra pestaña").
// Problema: en la misma PC se puede abrir el mismo movimiento (o varios "Nuevo") en dos pestañas/ventanas y pisarse al guardar.
// Una pagina web NO puede traer al frente otra pestaña de forma confiable, asi que en vez de "saltar a la otra" este modulo avisa y deja
// PASAR EL TRABAJO a la pestaña actual (la otra queda tapada con un cartel), cargar uno nuevo aparte o volver al listado.
//
// Como funciona (solo este navegador; sin servidor):
//  - BroadcastChannel: cada pestaña con un Nuevo o una edicion activa contesta "estoy" cuando otra pregunta "quien". Asi se sabe cuantas
//    hay, cuantas lineas tienen y (en modo lista) como se llaman. El traspaso del trabajo tambien viaja por este canal.
//  - Web Locks (navigator.locks): cada Nuevo/edicion toma un lock con nombre. Sirve para detectar pestañas SUSPENDIDAS por el navegador que
//    no contestan el canal pero siguen "abiertas". Si navigator.locks no existe (contexto no seguro) se trabaja solo con el canal.
//  - Desempate: solo cuentan las pestañas ANTERIORES (marca de tiempo menor; a igualdad, id menor), asi dos pestañas que abren a la vez
//    no se avisan mutuamente en un bucle: la mas nueva es la que avisa.
//  - Grupo: separa los NUEVO entre si (p.ej. la accion de Stock: un Nuevo de «Egreso» no avisa por uno de «Cierre»). Un registro existente
//    se identifica por su id y no depende del grupo. cfg.modoLista: el aviso lista todos los abiertos con «Traer este» por fila (Elaborados).
// Uso: var doc = DocumentoAbierto.crear({...}); doc.anunciarNuevo() / doc.tomarEdicion(id) / doc.convertirAEdicion(id) / doc.cambiarGrupo(g) / doc.liberar().
// Depende de: SweetAlert (window.Swal). Todo texto de otras pestañas se pinta con textContent, nunca con innerHTML.
(function (window, document) {
    'use strict';

    var NOMBRE_CANAL = 'carnisys-documento-abierto-v1';
    var PREFIJO_LOCK = 'carnisys-doc-';
    var CLAVE_SESSION = 'carnisys-doc-instancia-v1';
    var ESPERA_DESCUBRIR_MS = 300;   // cuanto se espera a que las otras pestañas contesten "estoy"
    var ESPERA_TRABAJO_MS = 3000;    // cuanto se espera el trabajo de la otra pestaña tras pedirlo
    var ESPERA_ACUSE_MS = 5000;      // cuanto espera la pestaña que entrega el "recibido" antes de quedarse con su trabajo
    var ESPERA_LOCK_MS = 3000;       // cuanto se espera a que la pestaña que cedio suelte su lock

    function nuevoId() {
        if (window.crypto && typeof window.crypto.randomUUID === 'function') return window.crypto.randomUUID();
        return 'i' + Date.now().toString(36) + Math.random().toString(36).slice(2, 10);
    }

    // Id que sobrevive al F5 de la misma pestaña (sessionStorage): sirve para la clave del borrador local de cada Nuevo.
    // Ojo: "Duplicar pestaña" copia el sessionStorage, por eso crear() lo regenera si otra pestaña viva ya lo usa.
    function leerClaveInstancia() {
        try {
            var v = window.sessionStorage.getItem(CLAVE_SESSION);
            if (!v) {
                v = nuevoId();
                window.sessionStorage.setItem(CLAVE_SESSION, v);
            }
            return v;
        } catch (err) {
            console.warn('[DocumentoAbierto] sessionStorage no disponible, la clave del borrador no sobrevive al F5', err);
            return nuevoId();
        }
    }

    function guardarClaveInstancia(valor) {
        try { window.sessionStorage.setItem(CLAVE_SESSION, valor); } catch (err) { console.warn('[DocumentoAbierto] no se pudo guardar la clave de instancia', err); }
    }

    function textoLineas(n) {
        return (n || 0) + ((n || 0) === 1 ? ' línea' : ' líneas');
    }

    function crear(cfg) {
        var modulo = String(cfg.modulo || '');
        var idUsuario = String(cfg.idUsuario || 0);
        var grupo = String(cfg.grupo || '');
        var textos = cfg.textos || {};
        var singular = textos.singular || 'documento';
        var plural = textos.plural || (singular + 's');
        var urls = cfg.urls || {};

        var instanceId = nuevoId();          // identifica ESTA carga de pagina en los mensajes
        var claveInstancia = leerClaveInstancia();
        var estado = null;                   // {tipo:'nuevo'|'edicion', idRegistro, ts} mientras esta pestaña tiene un documento activo
        var liberadores = {};                // nombre de lock -> funcion que lo suelta
        var vivos = [];                      // claveInstancia de las pestañas vivas vistas en el ultimo descubrimiento
        var cedido = false;
        var escuchaEstoy = null;
        var escuchaTrabajo = null;
        var escuchaAcuse = null;
        var canal = null;

        try {
            if (window.BroadcastChannel) canal = new window.BroadcastChannel(NOMBRE_CANAL);
        } catch (err) {
            console.warn('[DocumentoAbierto] no se pudo abrir el BroadcastChannel', err);
        }
        if (!canal) console.warn('[DocumentoAbierto] sin BroadcastChannel: sin aviso de pestañas duplicadas');

        // ---- nombres de lock ----
        // El grupo (p.ej. la accion de Stock) solo separa los NUEVO: un registro existente se identifica por su id, sin importar el grupo.
        function prefijoLock() {
            return PREFIJO_LOCK + [modulo, idUsuario].map(encodeURIComponent).join('-') + '-';
        }
        function prefijoNuevo() { return prefijoLock() + 'g' + encodeURIComponent(grupo) + '-nuevo-'; }
        function nombreNuevo(id) { return prefijoNuevo() + id; }
        function nombreEdicion(idRegistro) { return prefijoLock() + 'edicion-' + idRegistro; }

        // Toma un lock y lo mantiene hasta soltarlo. esperar=false: si ya esta tomado responde false enseguida; esperar=true: espera
        // hasta ESPERA_LOCK_MS (la pestaña que cedio todavia lo esta soltando). Sin navigator.locks resuelve true (se trabaja sin lock).
        function tomarLock(nombre, esperar) {
            if (!window.navigator.locks) return Promise.resolve(true);
            return new Promise(function (resolve) {
                var opciones = { mode: 'exclusive' };
                var temporizador = null;
                if (esperar && window.AbortController) {
                    var control = new window.AbortController();
                    opciones.signal = control.signal;
                    temporizador = window.setTimeout(function () { control.abort(); }, ESPERA_LOCK_MS);
                } else {
                    opciones.ifAvailable = true;
                }
                var pedido = window.navigator.locks.request(nombre, opciones, function (lock) {
                    if (temporizador) window.clearTimeout(temporizador);
                    if (!lock) { resolve(false); return null; }
                    resolve(true);
                    // La promesa devuelta mantiene el lock; se resuelve al liberar.
                    return new Promise(function (soltar) { liberadores[nombre] = soltar; });
                });
                if (pedido && typeof pedido.catch === 'function') {
                    pedido.catch(function (err) {
                        if (temporizador) window.clearTimeout(temporizador);
                        if (!(err && err.name === 'AbortError')) console.warn('[DocumentoAbierto] fallo el lock', nombre, err);
                        resolve(false);
                    });
                }
            });
        }

        function soltarLock(nombre) {
            var soltar = liberadores[nombre];
            if (!soltar) return;
            delete liberadores[nombre];
            try { soltar(); } catch (err) { console.warn('[DocumentoAbierto] no se pudo soltar el lock', nombre, err); }
        }

        function soltarTodosLosLocks() {
            Object.keys(liberadores).forEach(soltarLock);
        }

        // Nombres de lock de este modulo/usuario/grupo tomados por OTRAS pestañas (los propios estan en `liberadores`).
        function locksAjenos() {
            if (!window.navigator.locks || typeof window.navigator.locks.query !== 'function') return Promise.resolve([]);
            return window.navigator.locks.query().then(function (r) {
                var prefijo = prefijoLock();
                return ((r && r.held) || []).map(function (h) { return h.name; }).filter(function (n) {
                    return typeof n === 'string' && n.indexOf(prefijo) === 0 && !liberadores[n];
                });
            }).catch(function (err) {
                console.warn('[DocumentoAbierto] no se pudo consultar los locks', err);
                return [];
            });
        }

        // ---- mensajes ----
        // Una pestaña con un Nuevo solo contesta a las de su mismo grupo; una con una edicion contesta a todas (se identifica por id).
        function debeContestar(m) {
            if (m.mod !== modulo || m.user !== idUsuario) return false;
            return estado.tipo === 'edicion' || m.grupo === grupo;
        }

        function resumenPropio() {
            try {
                return cfg.resumen ? (cfg.resumen() || {}) : {};
            } catch (err) {
                console.warn('[DocumentoAbierto] fallo resumen()', err);
                return {};
            }
        }

        function enviar(mensaje) {
            if (!canal) return;
            try {
                canal.postMessage(mensaje);
            } catch (err) {
                console.warn('[DocumentoAbierto] no se pudo enviar', mensaje && mensaje.t, err);
            }
        }

        if (canal) {
            canal.onmessage = function (ev) {
                var m = ev && ev.data;
                if (!m || typeof m !== 'object') return;
                if (m.t === 'quien') {
                    if (m.de === instanceId || !estado || !debeContestar(m)) return;
                    var r = resumenPropio();
                    enviar({
                        t: 'estoy', a: m.de, de: instanceId, tipo: estado.tipo, idRegistro: estado.idRegistro, ts: estado.ts,
                        clave: claveInstancia, cantLineas: r.cantLineas || 0, etiqueta: r.etiqueta || '', detalle: r.detalle || '',
                        compat: cfg.compatibilidad ? String(cfg.compatibilidad()) : ''
                    });
                } else if (m.t === 'estoy') {
                    if (m.a === instanceId && escuchaEstoy) escuchaEstoy(m);
                } else if (m.t === 'pedir') {
                    if (m.a === instanceId) atenderPedido(m);
                } else if (m.t === 'trabajo') {
                    if (m.a === instanceId && escuchaTrabajo) escuchaTrabajo(m);
                } else if (m.t === 'recibido') {
                    if (m.a === instanceId && escuchaAcuse) escuchaAcuse(m);
                }
            };
        }

        // Pregunta "quien" y junta las respuestas durante ESPERA_DESCUBRIR_MS.
        function descubrir() {
            return new Promise(function (resolve) {
                var respuestas = [];
                if (!canal) { resolve(respuestas); return; }
                escuchaEstoy = function (m) { respuestas.push(m); };
                enviar({ t: 'quien', de: instanceId, mod: modulo, user: idUsuario, grupo: grupo });
                window.setTimeout(function () {
                    escuchaEstoy = null;
                    vivos = respuestas.map(function (m) { return m.clave; });
                    // "Duplicar pestaña" copia el sessionStorage: si otra pestaña viva ya usa mi clave de instancia, se regenera.
                    if (vivos.indexOf(claveInstancia) !== -1) {
                        claveInstancia = nuevoId();
                        guardarClaveInstancia(claveInstancia);
                    }
                    resolve(respuestas);
                }, ESPERA_DESCUBRIR_MS);
            });
        }

        // cfg.compatibilidad() (opcional): solo se puede traer el trabajo de una pestaña con la misma "compatibilidad" (p.ej. el mismo
        // elaborado y modo en Ingreso rapido: no tiene sentido pasar la cantidad de un elaborado a otro).
        function puedeTraer(m) {
            return !cfg.compatibilidad || m.compat === String(cfg.compatibilidad());
        }

        function esAnterior(m) {
            if (!estado) return false;
            return m.ts < estado.ts || (m.ts === estado.ts && m.de < instanceId);
        }

        // ---- traspaso del trabajo ----
        // Lado B (pestaña actual): pide el trabajo a la pestaña `idOtra`. Resuelve el paquete o null si no contesto a tiempo.
        function pedirTrabajo(idOtra) {
            return new Promise(function (resolve) {
                var temporizador = window.setTimeout(function () { escuchaTrabajo = null; resolve(null); }, ESPERA_TRABAJO_MS);
                escuchaTrabajo = function (m) {
                    if (m.de !== idOtra) return;
                    window.clearTimeout(temporizador);
                    escuchaTrabajo = null;
                    resolve(m.paquete || {});
                };
                enviar({ t: 'pedir', a: idOtra, de: instanceId });
            });
        }

        // Lado A (la que entrega): manda su paquete y cede SOLO cuando B confirma que lo aplico; si B no contesta, A se queda con su
        // trabajo (en el peor caso queda duplicado, nunca perdido).
        function atenderPedido(m) {
            if (cedido || !estado) return;
            var paquete;
            try {
                paquete = JSON.parse(JSON.stringify(cfg.obtenerPaquete()));
            } catch (err) {
                console.error('[DocumentoAbierto] no se pudo armar el paquete para pasar el trabajo', err);
                return;
            }
            var solicitante = m.de;
            var temporizador = window.setTimeout(function () {
                escuchaAcuse = null;
                console.warn('[DocumentoAbierto] la otra pestaña no confirmo el traspaso: esta pestaña se queda con su trabajo');
            }, ESPERA_ACUSE_MS);
            escuchaAcuse = function (r) {
                if (r.de !== solicitante) return;
                window.clearTimeout(temporizador);
                escuchaAcuse = null;
                ceder();
            };
            enviar({ t: 'trabajo', a: solicitante, de: instanceId, paquete: paquete });
        }

        function ceder() {
            cedido = true;
            soltarTodosLosLocks();
            estado = null;
            try {
                if (cfg.alCeder) cfg.alCeder();
            } catch (err) {
                console.error('[DocumentoAbierto] fallo alCeder()', err);
            }
            mostrarCartelCedido();
        }

        function mostrarCartelCedido() {
            var capa = document.createElement('div');
            capa.setAttribute('role', 'alertdialog');
            capa.style.cssText = 'position:fixed;inset:0;z-index:20000;display:flex;align-items:center;justify-content:center;padding:1rem;' +
                'background:var(--bs-body-bg,#fff);color:var(--bs-body-color,#212529);';
            var caja = document.createElement('div');
            caja.style.cssText = 'max-width:30rem;text-align:center;font-size:0.95rem;';
            var titulo = document.createElement('h5');
            titulo.textContent = 'Lo que tenías cargado pasó a otra pestaña';
            var texto = document.createElement('p');
            texto.textContent = 'Seguí trabajando en la otra pestaña o ventana. Esta ya no guarda nada.';
            var boton = document.createElement('button');
            boton.type = 'button';
            boton.className = 'btn btn-primary btn-sm';
            boton.textContent = 'Ir al listado';
            boton.addEventListener('click', function () { irAlListado(); });
            caja.appendChild(titulo);
            caja.appendChild(texto);
            caja.appendChild(boton);
            capa.appendChild(caja);
            document.body.appendChild(capa);
        }

        function irAlListado() {
            if (urls.listado) window.location.href = urls.listado;
        }

        // ---- avisos ----
        // Devuelve {accion:'traer', id}|{accion:'aparte'}|{accion:'listado'}|{accion:'cerrado'}.
        // o: {titulo, mensajes[], filas[{id, texto}], idTraer, textoTraer, textoAparte, permiteCerrar}
        function preguntar(o) {
            return new Promise(function (resolve) {
                if (!window.Swal) {
                    console.warn('[DocumentoAbierto] SweetAlert no disponible, no se puede avisar');
                    resolve({ accion: 'cerrado' });
                    return;
                }
                var contenido = document.createElement('div');
                contenido.style.cssText = 'text-align:left;font-size:0.9rem;';
                (o.mensajes || []).forEach(function (texto) {
                    var p = document.createElement('p');
                    p.textContent = texto;
                    contenido.appendChild(p);
                });
                var elegido = null;
                (o.filas || []).forEach(function (fila) {
                    var linea = document.createElement('div');
                    linea.style.cssText = 'display:flex;align-items:center;justify-content:space-between;gap:.5rem;margin:.25rem 0;';
                    var rotulo = document.createElement('span');
                    rotulo.textContent = fila.texto;
                    linea.appendChild(rotulo);
                    if (fila.id) {
                        var boton = document.createElement('button');
                        boton.type = 'button';
                        boton.className = 'btn btn-sm btn-primary';
                        boton.textContent = 'Traer este';
                        boton.addEventListener('click', function () {
                            elegido = { accion: 'traer', id: fila.id };
                            window.Swal.close();
                        });
                        linea.appendChild(boton);
                    }
                    contenido.appendChild(linea);
                });
                window.Swal.fire({
                    icon: 'warning',
                    title: o.titulo,
                    html: contenido,
                    showConfirmButton: !!o.idTraer,
                    confirmButtonText: o.textoTraer || 'Traer a esta pestaña',
                    showDenyButton: true,
                    denyButtonText: o.textoAparte,
                    denyButtonColor: '#198754',   // el rojo por defecto de "deny" sugiere algo peligroso y "cargar aparte" no lo es
                    showCancelButton: true,
                    cancelButtonText: 'Volver al listado',
                    allowOutsideClick: !!o.permiteCerrar,
                    allowEscapeKey: !!o.permiteCerrar
                }).then(function (r) {
                    if (elegido) resolve(elegido);
                    else if (r.isConfirmed) resolve({ accion: 'traer', id: o.idTraer });
                    else if (r.isDenied) resolve({ accion: 'aparte' });
                    else if (r.dismiss === window.Swal.DismissReason.cancel) resolve({ accion: 'listado' });
                    else resolve({ accion: 'cerrado' });
                });
            });
        }

        function avisoSimple(titulo, texto) {
            if (!window.Swal) return Promise.resolve();
            return window.Swal.fire({ icon: 'info', title: titulo, text: texto });
        }

        // Pide el trabajo a la pestaña `idOtra`, lo aplica y confirma. Resuelve true si pasó.
        function traerDe(idOtra) {
            return pedirTrabajo(idOtra).then(function (paquete) {
                if (!paquete) return false;
                try {
                    cfg.aplicarPaquete(paquete);
                } catch (err) {
                    console.error('[DocumentoAbierto] no se pudo aplicar el trabajo recibido', err);
                    return false;
                }
                enviar({ t: 'recibido', a: idOtra, de: instanceId });
                return true;
            });
        }

        // ---- API: Nuevo ----
        // opciones.sinAviso: solo se registra (p.ej. llega desde "Abrir en ventana", donde la otra pestaña ya se esta yendo).
        // Resuelve {accion:'seguir'|'traido'|'listado'}.
        function anunciarNuevo(opciones) {
            opciones = opciones || {};
            cedido = false;
            estado = { tipo: 'nuevo', idRegistro: 0, ts: Date.now() };
            return tomarLock(nombreNuevo(instanceId), false).then(function () {
                if (opciones.sinAviso) return { accion: 'seguir' };
                return locksAjenos().then(function (ajenos) {
                    return descubrir().then(function (respuestas) {
                        return evaluarNuevo(respuestas, ajenos);
                    });
                });
            });
        }

        // Texto del grupo para los titulos (p.ej. " con la acción «Egreso Stock»"); vacio si la pantalla no lo informa.
        function sufijoGrupo() {
            if (!textos.rotuloGrupo) return '';
            // etiquetaGrupo() da el nombre con sus mayusculas originales (el grupo se compara en minusculas).
            return ' ' + textos.rotuloGrupo.replace('{grupo}', cfg.etiquetaGrupo ? cfg.etiquetaGrupo() : grupo);
        }

        function evaluarNuevo(respuestas, ajenos) {
            // En modoLista (Elaborados) tambien se listan las ediciones abiertas, para que se vea TODO lo que hay abierto.
            var anteriores = respuestas.filter(function (m) { return (cfg.modoLista || m.tipo === 'nuevo') && esAnterior(m); });
            // Locks de Nuevo que ninguna respuesta explica = pestañas suspendidas que no contestan el canal.
            var sinResponder = ajenos.filter(function (nombre) {
                var pref = prefijoNuevo();
                if (nombre.indexOf(pref) !== 0) return false;
                var id = nombre.substring(pref.length);
                return !respuestas.some(function (m) { return m.de === id; });
            });
            var total = anteriores.length + sinResponder.length;
            if (!total) return Promise.resolve({ accion: 'seguir' });

            var mensajes = [];
            var filas = [];
            var idTraer = null;
            var titulo;
            if (cfg.modoLista) {
                titulo = 'Ya tenés ' + (total === 1 ? 'un ' + singular + ' nuevo abierto' : total + ' ' + plural + ' nuevos abiertos');
                mensajes.push('Están abiertos en otras pestañas o ventanas. Elegí cuál traer a esta pestaña, o cargá uno nuevo aparte.');
                anteriores.forEach(function (m) {
                    var esNuevoOtra = m.tipo === 'nuevo';
                    var partes = [m.etiqueta || ('Nuevo ' + singular), esNuevoOtra ? 'nuevo' : 'edición #' + m.idRegistro, m.detalle || textoLineas(m.cantLineas)];
                    // Solo un Nuevo con algo cargado y compatible se puede traer: una edicion tiene su propio id y no cabe en un formulario nuevo.
                    filas.push({ id: esNuevoOtra && m.cantLineas > 0 && puedeTraer(m) ? m.de : null, texto: partes.join(' · ') });
                });
            } else if (total === 1 && anteriores.length === 1) {
                var unico = anteriores[0];
                titulo = 'Ya tenés un ' + singular + ' nuevo abierto' + sufijoGrupo();
                mensajes.push('Hay un ' + singular + ' nuevo abierto en otra pestaña o ventana (' +
                    (unico.cantLineas > 0 ? textoLineas(unico.cantLineas) + (unico.cantLineas === 1 ? ' cargada' : ' cargadas') : 'todavía vacío') + ').');
                mensajes.push(unico.cantLineas > 0
                    ? 'Podés traerlo a esta pestaña, cargar otro nuevo aparte (independiente) o volver al listado.'
                    : 'Podés cargar otro nuevo aparte (independiente) o volver al listado.');
                if (unico.cantLineas > 0 && puedeTraer(unico)) idTraer = unico.de;
            } else {
                titulo = 'Tenés ' + total + ' ' + plural + ' nuevos abiertos' + sufijoGrupo();
                mensajes.push('Están en otras pestañas o ventanas: buscalos ahí para continuar con alguno. Desde acá podés cargar otro nuevo aparte o volver al listado.');
            }
            if (sinResponder.length) {
                mensajes.push(sinResponder.length + (sinResponder.length === 1 ? ' pestaña no responde' : ' pestañas no responden') +
                    ' (puede estar suspendida por el navegador): no se puede traer su trabajo, buscala entre tus pestañas.');
            }

            return preguntar({
                titulo: titulo, mensajes: mensajes, filas: filas, idTraer: idTraer, textoAparte: 'Cargar otro nuevo aparte'
            }).then(function (r) {
                if (r.accion === 'listado') { irAlListado(); return { accion: 'listado' }; }
                if (r.accion === 'traer') {
                    return traerDe(r.id).then(function (ok) {
                        if (ok) return { accion: 'traido' };
                        return avisoSimple('No se pudo traer', 'La otra pestaña no respondió. Podés seguir con un nuevo aparte.').then(function () { return { accion: 'seguir' }; });
                    });
                }
                return { accion: 'seguir' };
            });
        }

        // ---- API: edicion de un registro existente ----
        // Se llama al tocar "Modificar". Resuelve true si esta pestaña puede editar (libre, o trajo la edicion de la otra).
        function tomarEdicion(idRegistro) {
            cedido = false;
            estado = { tipo: 'edicion', idRegistro: idRegistro, ts: Date.now() };
            return locksAjenos().then(function (ajenos) {
                return descubrir().then(function (respuestas) {
                    var anteriores = respuestas.filter(function (m) { return m.tipo === 'edicion' && m.idRegistro === idRegistro && esAnterior(m); });
                    var hayLockSinResponder = ajenos.indexOf(nombreEdicion(idRegistro)) !== -1 &&
                        !respuestas.some(function (m) { return m.tipo === 'edicion' && m.idRegistro === idRegistro; });
                    if (!anteriores.length && !hayLockSinResponder) {
                        return tomarLock(nombreEdicion(idRegistro), false).then(function (ok) {
                            if (ok) return true;
                            return conflictoEdicion([], true);   // otra pestaña lo tomo justo en el medio
                        });
                    }
                    return conflictoEdicion(anteriores, hayLockSinResponder);
                });
            }).then(function (puede) {
                if (!puede) { soltarTodosLosLocks(); estado = null; }
                return puede;
            });
        }

        function conflictoEdicion(anteriores) {
            var otra = anteriores[0] || null;
            var mensajes = [];
            if (otra) {
                mensajes.push('Otra pestaña o ventana lo tiene abierto para modificar (' + textoLineas(otra.cantLineas) + ').');
                mensajes.push('Si lo modificás en las dos, la última que guarde pisa a la otra. Podés traer esa modificación a esta pestaña, cargar uno nuevo aparte o volver al listado.');
            } else {
                mensajes.push('Este ' + singular + ' está abierto para modificar en una pestaña que no responde (puede estar suspendida por el navegador). Buscala entre tus pestañas.');
                mensajes.push('Desde acá podés cargar uno nuevo aparte o volver al listado.');
            }
            return preguntar({
                titulo: 'Este ' + singular + ' ya se está modificando',
                mensajes: mensajes,
                idTraer: otra ? otra.de : null,
                textoTraer: 'Traer la modificación a esta pestaña',
                textoAparte: 'Cargar uno nuevo aparte',
                permiteCerrar: true
            }).then(function (r) {
                if (r.accion === 'listado') { irAlListado(); return false; }
                if (r.accion === 'aparte') {
                    if (urls.nuevo) window.location.href = urls.nuevo;
                    return false;
                }
                if (r.accion === 'traer') {
                    return traerDe(r.id).then(function (ok) {
                        if (!ok) {
                            return avisoSimple('No se pudo traer', 'La otra pestaña no respondió.').then(function () { return false; });
                        }
                        // La pestaña que cedio suelta su lock al recibir el "recibido": se espera para tomarlo.
                        return tomarLock(nombreEdicion(estado.idRegistro), true).then(function (tomado) {
                            if (!tomado) console.warn('[DocumentoAbierto] no se pudo tomar el lock tras el traspaso; se sigue sin lock');
                            return true;
                        });
                    });
                }
                return false;   // cerro el aviso: la pestaña queda en solo lectura
            });
        }

        // Un Nuevo que se guardo pasa a ser la edicion de ese id (p.ej. "Imprimir" y quedarse en la pantalla).
        function convertirAEdicion(idRegistro) {
            if (!estado) return Promise.resolve(false);
            soltarTodosLosLocks();
            estado = { tipo: 'edicion', idRegistro: idRegistro, ts: estado.ts };
            return tomarLock(nombreEdicion(idRegistro), false);
        }

        function liberar() {
            soltarTodosLosLocks();
            estado = null;
        }

        // El grupo de un Nuevo ya registrado cambio (p.ej. se cambio la accion de Stock): se re-registra en el grupo nuevo y se vuelve a
        // chequear si ya hay otros Nuevo de ese grupo. Resuelve igual que anunciarNuevo. Si no es un Nuevo (o no cambio) no hace nada.
        function cambiarGrupo(nuevoGrupo) {
            nuevoGrupo = String(nuevoGrupo || '');
            if (nuevoGrupo === grupo || !estado || estado.tipo !== 'nuevo') {
                grupo = nuevoGrupo;
                return Promise.resolve({ accion: 'seguir' });
            }
            soltarTodosLosLocks();
            grupo = nuevoGrupo;
            return anunciarNuevo();
        }

        // ---- borrador local por pestaña ----
        // Clave del borrador local de ESTA pestaña: dos Nuevo abiertos ya no comparten (ni se pisan) la misma clave.
        function claveBorradorLocal(base) {
            return base + '_' + claveInstancia;
        }

        // Pasa a la clave de esta pestaña el borrador local mas reciente que dejo una pestaña que ya no existe (o el de la version vieja
        // sin sufijo). Devuelve true si habia uno. Los drafts guardan `guardadoEn` (ms) para elegir el mas reciente.
        function adoptarBorradorHuerfano(base) {
            try {
                var propia = claveBorradorLocal(base);
                var mejor = null;
                var mejorTs = -1;
                for (var i = 0; i < window.localStorage.length; i++) {
                    var k = window.localStorage.key(i);
                    if (!k || k === propia) continue;
                    if (k !== base && k.indexOf(base + '_') !== 0) continue;
                    var sufijo = k === base ? '' : k.substring(base.length + 1);
                    if (sufijo && (sufijo === claveInstancia || vivos.indexOf(sufijo) !== -1)) continue;   // de una pestaña viva
                    var ts = 0;
                    try { ts = (JSON.parse(window.localStorage.getItem(k)) || {}).guardadoEn || 0; } catch (err) { ts = 0; }
                    if (ts > mejorTs) { mejorTs = ts; mejor = k; }
                }
                if (!mejor) return false;
                window.localStorage.setItem(propia, window.localStorage.getItem(mejor));
                window.localStorage.removeItem(mejor);
                return true;
            } catch (err) {
                console.warn('[DocumentoAbierto] no se pudo adoptar el borrador huerfano', err);
                return false;
            }
        }

        return {
            anunciarNuevo: anunciarNuevo,
            tomarEdicion: tomarEdicion,
            convertirAEdicion: convertirAEdicion,
            liberar: liberar,
            cambiarGrupo: cambiarGrupo,
            claveBorradorLocal: claveBorradorLocal,
            adoptarBorradorHuerfano: adoptarBorradorHuerfano,
            instanceId: function () { return instanceId; }
        };
    }

    window.DocumentoAbierto = { crear: crear };
})(window, document);
