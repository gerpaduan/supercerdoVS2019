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
//  - Cartel (2026-11-19): una fila por cada abierto con que es, donde esta (pestaña / ventana / app instalada / dentro del POS), desde cuando y
//    que items tiene, mas "Traer este", "Señalar" (esa pestaña titila) y "Cerrar" (solo un Nuevo vacio; la otra pestaña lo vuelve a comprobar).
//  - ?desdeVentana=1 (lo pone "Abrir en ventana"): la ventana nueva no avisa de la pestaña de origen, que se esta yendo.
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
    var PREFIJO_FRAME_POS = 'carnisys-trabajo-pos-';   // nombre del <iframe> de las cargas del POS (pos-trabajos.js, crearIframe)
    var DURACION_SENAL_MS = 10000;   // cuanto titila una pestaña "señalada" desde el cartel de otra
    var MAX_ITEMS_VISTA_PREVIA = 3;  // items que se muestran por documento abierto en el cartel (el resto va como "y N más")
    var GRACIA_SIN_RESPONDER_MS = 1500;  // cuanto se insiste con un lock sin respuesta antes de avisar "una pestaña que no responde"
    var PASO_GRACIA_MS = 300;            // pausa entre cada reintento de esa insistencia

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

    function dosDigitos(n) {
        return (n < 10 ? '0' : '') + n;
    }

    // "desde las 10:42 (hace 25 min)"; si no es de hoy suma la fecha. `ms` = marca de tiempo de cuando se abrio la pagina.
    function textoApertura(ms) {
        var d = new Date(ms);
        var ahora = new Date();
        var hora = dosDigitos(d.getHours()) + ':' + dosDigitos(d.getMinutes());
        if (d.toDateString() !== ahora.toDateString()) hora += ' del ' + dosDigitos(d.getDate()) + '/' + dosDigitos(d.getMonth() + 1);
        var min = Math.max(0, Math.round((ahora.getTime() - ms) / 60000));
        var hace;
        if (min < 1) hace = 'recién';
        else if (min < 60) hace = 'hace ' + min + ' min';
        else hace = 'hace ' + Math.floor(min / 60) + ' h' + (min % 60 ? ' ' + (min % 60) + ' min' : '');
        return 'desde las ' + hora + ' (' + hace + ')';
    }

    // Kilos con coma decimal y hasta 3 decimales, para los textos de la vista previa de cada pantalla.
    function formatoKg(n) {
        var v = Number(n);
        if (!isFinite(v)) return '';
        return v.toLocaleString('es-AR', { maximumFractionDigits: 3 });
    }

    // Helper para el resumen() de cada pantalla: arma {vistaPrevia, masItems} con los primeros items (aTexto(item) -> "Producto 4 kg").
    function vistaPrevia(items, aTexto) {
        var lista = (items || []).filter(Boolean);
        return {
            vistaPrevia: lista.slice(0, MAX_ITEMS_VISTA_PREVIA).map(function (i) { return aTexto(i); }),
            masItems: Math.max(0, lista.length - MAX_ITEMS_VISTA_PREVIA)
        };
    }

    // Marca ?desdeVentana=1 que pone "Abrir en ventana" (pantalla-trabajo.js). Se lee una sola vez y se saca de la URL: asi un F5 de la
    // ventana vuelve a avisar como siempre.
    function consumirMarcaDesdeVentana() {
        try {
            var url = new URL(window.location.href);
            if (url.searchParams.get('desdeVentana') !== '1') return false;
            url.searchParams.delete('desdeVentana');
            window.history.replaceState(window.history.state, '', url.pathname + url.search + url.hash);
            return true;
        } catch (err) {
            console.warn('[DocumentoAbierto] no se pudo leer la marca desdeVentana', err);
            return false;
        }
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
        var abiertoEn = Date.now();          // cuando se cargo esta pagina: el cartel de otra pestaña muestra "desde las HH:MM"
        var temporizadorSenal = null;        // mientras esta pestaña titila por un "Señalar" de otra
        var escuchaCerrado = null;
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

        // Salida de la pagina (2026-11-19): se sueltan los locks sin esperar a que el navegador destruya la pestaña. Antes dependia de eso, y si
        // tardaba (o la pagina quedaba congelada en el cache de "atras/adelante") otra pestaña veia un lock sin dueño que contesta y avisaba
        // "una pestaña que no responde". `estado` se conserva: si el navegador RESTAURA la pagina del cache (pageshow persisted) se retoman.
        window.addEventListener('pagehide', function () {
            if (estado) soltarTodosLosLocks();
        });

        window.addEventListener('pageshow', function (ev) {
            if (!ev || !ev.persisted || !estado || cedido) return;
            var nombre = estado.tipo === 'edicion' ? nombreEdicion(estado.idRegistro) : nombreNuevo(instanceId);
            tomarLock(nombre, false).then(function (ok) {
                if (!ok) console.warn('[DocumentoAbierto] no se pudo retomar el lock al volver a la pagina', nombre);
            });
        });

        // Reintenta (hasta GRACIA_SIN_RESPONDER_MS) mientras haya locks de otras pestañas sin respuesta: en cada paso vuelve a mirar los locks y a
        // preguntar "quien". Un lock que se suelta (la otra pestaña se estaba yendo) o una pestaña lenta que por fin contesta ya no cuentan.
        // `contar(respuestas, ajenos)` devuelve cuantos locks siguen sin explicar. Resuelve { respuestas, ajenos } con lo ultimo visto.
        function reverificarSinResponder(contar, respuestas, ajenos) {
            var inicio = Date.now();
            function paso(resp, aj) {
                if (!contar(resp, aj) || Date.now() - inicio >= GRACIA_SIN_RESPONDER_MS) return Promise.resolve({ respuestas: resp, ajenos: aj });
                return new Promise(function (listo) { window.setTimeout(listo, PASO_GRACIA_MS); })
                    .then(function () { return locksAjenos(); })
                    .then(function (aj2) {
                        return descubrir().then(function (resp2) { return paso(resp2, aj2); });
                    });
            }
            return paso(respuestas, ajenos);
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
                    var lugar = detectarLugar();
                    enviar({
                        t: 'estoy', a: m.de, de: instanceId, tipo: estado.tipo, idRegistro: estado.idRegistro, ts: estado.ts,
                        clave: claveInstancia, cantLineas: r.cantLineas || 0, etiqueta: r.etiqueta || '', detalle: r.detalle || '',
                        vistaPrevia: (r.vistaPrevia || []).slice(0, MAX_ITEMS_VISTA_PREVIA).map(String), masItems: r.masItems || 0,
                        abiertoEn: abiertoEn, lugar: lugar.texto, enPos: lugar.enPos, minimizada: lugar.minimizada,
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
                } else if (m.t === 'senalar') {
                    if (m.a === instanceId) senalarEstaPestana();
                } else if (m.t === 'cerrar') {
                    if (m.a === instanceId) atenderCierre(m);
                } else if (m.t === 'cerrado') {
                    if (m.a === instanceId && escuchaCerrado) escuchaCerrado(m);
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

        // ---- lugar y acciones sobre otra pestaña ----
        // Donde vive ESTA pagina, en palabras del usuario, para que el cartel de la otra pestaña le diga donde buscarla. Una carga del POS
        // (menu Cargas) vive en un <iframe> cuyo nombre empieza con PREFIJO_FRAME_POS (pos-trabajos.js) y, minimizada, no se ve en ninguna pestaña.
        function detectarLugar() {
            var oculta = document.visibilityState === 'hidden';
            if ((window.name || '').indexOf(PREFIJO_FRAME_POS) === 0) {
                var marco = null;
                try { marco = window.frameElement; } catch (err) { marco = null; }   // sin acceso al marco: se asume minimizada
                var visible = !!(marco && marco.closest && marco.closest('.modal.show') && !marco.classList.contains('d-none'));
                return { texto: 'dentro del POS (menú Cargas)' + (visible ? '' : ', minimizada'), enPos: true, minimizada: !visible };
            }
            var texto = 'pestaña del navegador';
            if (document.documentElement.classList.contains('modo-ventana')) {
                var instalada = !!(window.matchMedia && window.matchMedia('(display-mode: standalone)').matches) || window.navigator.standalone === true;
                texto = instalada ? 'app instalada' : 'ventana independiente';
            }
            return { texto: texto + (oculta ? ' (en segundo plano)' : ''), enPos: false, minimizada: oculta };
        }

        // "Señalar" del cartel de otra pestaña: hace titilar el titulo de esta (y muestra una cinta si se la mira) para encontrarla en la barra
        // de pestañas o de tareas. Se apaga sola a los DURACION_SENAL_MS o al tocar la cinta.
        function senalarEstaPestana() {
            if (temporizadorSenal) return;
            var tituloOriginal = document.title;
            var tituloAlterno = '▶ ACÁ · ' + tituloOriginal;
            var alterno = false;
            var cinta = document.createElement('div');
            cinta.setAttribute('role', 'status');
            cinta.style.cssText = 'position:fixed;top:0;left:0;right:0;z-index:20001;padding:.4rem .75rem;text-align:center;font-weight:600;' +
                'background:#ffc107;color:#212529;cursor:pointer;';
            cinta.textContent = 'Esta es la pantalla que te señaló el aviso de la otra pestaña. (Tocá acá para ocultar este cartel)';
            if (document.body) document.body.appendChild(cinta);
            var intervalo = window.setInterval(function () {
                alterno = !alterno;
                document.title = alterno ? tituloAlterno : tituloOriginal;
            }, 700);
            function terminar() {
                window.clearInterval(intervalo);
                window.clearTimeout(temporizadorSenal);
                temporizadorSenal = null;
                document.title = tituloOriginal;
                if (cinta.parentNode) cinta.parentNode.removeChild(cinta);
            }
            cinta.addEventListener('click', terminar);
            temporizadorSenal = window.setTimeout(terminar, DURACION_SENAL_MS);
        }

        // Lado B: pide a la pestaña `idOtra` que se cierre (vuelva al listado). Resuelve {ok} o null si no contesto a tiempo.
        function pedirCierre(idOtra) {
            return new Promise(function (resolve) {
                var temporizador = window.setTimeout(function () { escuchaCerrado = null; resolve(null); }, ESPERA_TRABAJO_MS);
                escuchaCerrado = function (m) {
                    if (m.de !== idOtra) return;
                    window.clearTimeout(temporizador);
                    escuchaCerrado = null;
                    resolve({ ok: !!m.ok });
                };
                enviar({ t: 'cerrar', a: idOtra, de: instanceId });
            });
        }

        // Lado A: otra pestaña pidio cerrar esta. Solo se acepta si es un Nuevo SIN lineas (se vuelve a comprobar aca, el cartel pudo
        // quedar viejo) y no es una carga del POS. Contesta primero y recien despues navega, para que el mensaje alcance a salir.
        function atenderCierre(m) {
            var puede = !!estado && estado.tipo === 'nuevo' && !cedido && (window.name || '').indexOf(PREFIJO_FRAME_POS) !== 0 &&
                (resumenPropio().cantLineas || 0) === 0;
            enviar({ t: 'cerrado', a: m.de, de: instanceId, ok: puede });
            if (!puede) return;
            cedido = true;
            soltarTodosLosLocks();
            estado = null;
            try {
                if (cfg.alCeder) cfg.alCeder();   // frena borradores y deja salir sin el aviso de "cambios sin guardar"
            } catch (err) {
                console.error('[DocumentoAbierto] fallo alCeder() al cerrar por pedido de otra pestaña', err);
            }
            window.setTimeout(irAlListado, 150);
        }

        // ---- avisos ----
        // Devuelve {accion:'traer', id}|{accion:'aparte'}|{accion:'listado'}|{accion:'cerrado'}.
        // o: {titulo, mensajes[], filas[{id, texto, sub, items, ayuda, acciones[{texto, titulo, clase, alClick(ctx)}]}], idTraer, textoTraer,
        //     textoAparte, permiteCerrar}. ctx = {nota(texto), ocupado(bool), quitarFila()}: si se quitan todas las filas, el cartel se cierra
        // como "cargar aparte" (ya no queda nada abierto que avisar).
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
                var filasRestantes = (o.filas || []).length;

                function lineaDeTexto(caja, texto, estilo) {
                    if (!texto) return;
                    var linea = document.createElement('div');
                    linea.style.cssText = estilo;
                    linea.textContent = texto;
                    caja.appendChild(linea);
                }

                (o.filas || []).forEach(function (fila) {
                    var caja = document.createElement('div');
                    caja.style.cssText = 'border:1px solid var(--bs-border-color,#dee2e6);border-radius:.4rem;padding:.4rem .6rem;margin:.4rem 0;';
                    lineaDeTexto(caja, fila.texto, 'font-weight:600;');
                    lineaDeTexto(caja, fila.sub, 'font-size:.85em;opacity:.85;');
                    // Detalle de lo cargado (2026-11-19): mas grande y en negrita, sobre un fondo tenue con borde de color, para que se lea de un vistazo.
                    // Fondo azul translucido (no un color fijo ni --bs-tertiary-bg: en modo oscuro quedaba claro con texto claro) y color de texto heredado.
                    lineaDeTexto(caja, fila.items, 'font-size:1rem;font-weight:600;line-height:1.35;margin:.35rem 0;padding:.4rem .6rem;' +
                        'background:rgba(13,110,253,.13);border-left:4px solid #0d6efd;border-radius:.3rem;');
                    lineaDeTexto(caja, fila.ayuda, 'font-size:.85em;font-style:italic;');
                    var nota = document.createElement('div');
                    nota.style.cssText = 'font-size:.85em;color:var(--bs-primary,#0d6efd);';
                    caja.appendChild(nota);

                    var botones = document.createElement('div');
                    botones.style.cssText = 'display:flex;flex-wrap:wrap;gap:.4rem;margin-top:.3rem;';
                    var nuevoBoton = function (texto, clase) {
                        var boton = document.createElement('button');
                        boton.type = 'button';
                        boton.className = 'btn btn-sm ' + clase;
                        boton.textContent = texto;
                        botones.appendChild(boton);
                        return boton;
                    };
                    if (fila.id) {
                        nuevoBoton('Traer este', 'btn-primary').addEventListener('click', function () {
                            elegido = { accion: 'traer', id: fila.id };
                            window.Swal.close();
                        });
                    }
                    var ctx = {
                        nota: function (texto) { nota.textContent = texto || ''; },
                        ocupado: function (valor) {
                            Array.prototype.forEach.call(botones.querySelectorAll('button'), function (b) { b.disabled = !!valor; });
                        },
                        quitarFila: function () {
                            if (caja.parentNode) caja.parentNode.removeChild(caja);
                            filasRestantes--;
                            if (filasRestantes <= 0) {
                                elegido = { accion: 'aparte' };
                                window.Swal.close();
                            }
                        }
                    };
                    (fila.acciones || []).forEach(function (accion) {
                        var boton = nuevoBoton(accion.texto, accion.clase || 'btn-outline-secondary');
                        if (accion.titulo) boton.title = accion.titulo;
                        boton.addEventListener('click', function () { accion.alClick(ctx); });
                    });
                    if (botones.childNodes.length) caja.appendChild(botones);
                    contenido.appendChild(caja);
                });
                window.Swal.fire({
                    icon: 'warning',
                    title: o.titulo,
                    html: contenido,
                    width: (o.filas || []).length ? '42rem' : undefined,
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
            // "Abrir en ventana" marca la URL de la ventana nueva (pantalla-trabajo.js): la pestaña de origen se esta yendo, no corresponde
            // avisar de ella. Igual se descubren las pestañas vivas (para no adoptar el borrador local de una que sigue abierta).
            var vieneDeVentana = consumirMarcaDesdeVentana();
            return tomarLock(nombreNuevo(instanceId), false).then(function () {
                if (opciones.sinAviso) return { accion: 'seguir' };
                return locksAjenos().then(function (ajenos) {
                    return descubrir().then(function (respuestas) {
                        if (vieneDeVentana) return { accion: 'seguir' };
                        // Un lock sin respuesta se reverifica unos instantes antes de avisar (puede ser una pestaña que se esta yendo o lenta).
                        return reverificarSinResponder(function (resp, aj) { return locksNuevoSinResponder(resp, aj).length; }, respuestas, ajenos)
                            .then(function (r) { return evaluarNuevo(r.respuestas, r.ajenos); });
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

        // Una fila del cartel por cada documento abierto en otra pestaña/ventana: que es, donde esta, desde cuando y que tiene cargado,
        // con "Traer este" / "Señalar" / "Cerrar" segun corresponda. Los textos vienen de otra pestaña: se pintan con textContent.
        function filaDeAbierto(m) {
            var esNuevoOtra = m.tipo === 'nuevo';
            var soportaAcciones = !!m.lugar;   // una pestaña con la version vieja de este script no entiende "senalar" ni "cerrar"
            var vacio = !(m.cantLineas > 0);

            var partes = [esNuevoOtra ? 'Nuevo' : 'Edición #' + m.idRegistro];
            if (m.etiqueta) partes.push(m.etiqueta);
            partes.push(m.detalle || (vacio ? 'vacío' : textoLineas(m.cantLineas) + (m.cantLineas === 1 ? ' cargada' : ' cargadas')));

            var items = (m.vistaPrevia || []).join(' · ');
            if (items && m.masItems > 0) items += ' · y ' + m.masItems + ' más';

            var acciones = [];
            // Una carga dentro del POS no tiene pestaña que titilar ni se puede cerrar desde afuera: se vuelve a ella desde la barra del POS.
            if (soportaAcciones && !m.enPos) {
                acciones.push({
                    texto: 'Señalar',
                    titulo: 'Hace titilar el título de esa pestaña o ventana para que la encuentres',
                    clase: 'btn-outline-secondary',
                    alClick: function (ctx) {
                        enviar({ t: 'senalar', a: m.de, de: instanceId });
                        ctx.nota('Señalada: mirá la barra de pestañas o la de tareas, titila unos segundos.');
                    }
                });
                // Solo un Nuevo vacio se puede cerrar sin perder nada; la otra pestaña lo vuelve a comprobar antes de irse.
                if (esNuevoOtra && vacio) {
                    acciones.push({
                        texto: 'Cerrar',
                        titulo: 'Cierra ese ' + singular + ' vacío (esa pestaña vuelve al listado)',
                        clase: 'btn-outline-danger',
                        cerrar: true,
                        alClick: function (ctx) {
                            ctx.ocupado(true);
                            pedirCierre(m.de).then(function (r) {
                                ctx.ocupado(false);
                                if (r && r.ok) ctx.quitarFila();
                                else ctx.nota(r ? 'No se cerró: ya tiene algo cargado.' : 'La otra pestaña no respondió.');
                            });
                        }
                    });
                }
            }

            return {
                // "Traer este": solo un Nuevo con algo cargado y compatible (una edicion tiene su propio id y no cabe en un formulario nuevo).
                id: esNuevoOtra && !vacio && puedeTraer(m) ? m.de : null,
                texto: partes.join(' · '),
                sub: [m.lugar || 'otra pestaña o ventana', m.abiertoEn ? textoApertura(m.abiertoEn) : ''].filter(Boolean).join(' · '),
                items: items,
                ayuda: m.enPos ? 'Para volver a ella: botón amarillo «… en curso» de la barra del POS (menú Cargas).' : '',
                acciones: acciones
            };
        }

        // Locks de Nuevo que ninguna respuesta explica = pestañas suspendidas que no contestan el canal.
        function locksNuevoSinResponder(respuestas, ajenos) {
            return ajenos.filter(function (nombre) {
                var pref = prefijoNuevo();
                if (nombre.indexOf(pref) !== 0) return false;
                var id = nombre.substring(pref.length);
                return !respuestas.some(function (m) { return m.de === id; });
            });
        }

        function evaluarNuevo(respuestas, ajenos) {
            // En modoLista (Elaborados) tambien se listan las ediciones abiertas, para que se vea TODO lo que hay abierto.
            var anteriores = respuestas.filter(function (m) { return (cfg.modoLista || m.tipo === 'nuevo') && esAnterior(m); });
            var sinResponder = locksNuevoSinResponder(respuestas, ajenos);
            var total = anteriores.length + sinResponder.length;
            if (!total) return Promise.resolve({ accion: 'seguir' });
            // Diagnostico (2026-11-19): si el cartel vuelve a salir sin que haya otra pestaña, esto dice que lock lo causo.
            if (sinResponder.length) console.warn('[DocumentoAbierto] lock(s) de otra pestaña sin respuesta tras ' + GRACIA_SIN_RESPONDER_MS + ' ms:', sinResponder);

            var titulo = (total === 1 ? 'Ya tenés un ' + singular + ' nuevo abierto' : 'Tenés ' + total + ' ' + plural + ' nuevos abiertos') + sufijoGrupo();
            var filas = anteriores.map(filaDeAbierto);
            sinResponder.forEach(function () {
                filas.push({
                    texto: 'Una pestaña que no responde',
                    sub: 'Puede estar suspendida por el navegador: no se puede traer su trabajo, buscala entre tus pestañas.',
                    acciones: []
                });
            });

            var hayTraible = filas.some(function (f) { return !!f.id; });
            var hayCerrable = filas.some(function (f) { return f.acciones.some(function (a) { return a.cerrar; }); });
            var mensajes = [(total === 1 ? 'Está abierto' : 'Están abiertos') + ' en otra pestaña, otra ventana o dentro del POS. Abajo ves dónde, desde cuándo y qué tiene cargado.'];
            mensajes.push(hayTraible
                ? 'Podés traer lo cargado a esta pestaña, cargar otro nuevo aparte (independiente) o volver al listado.'
                : 'Podés cargar otro nuevo aparte (independiente) o volver al listado.');
            if (hayCerrable) mensajes.push('Si es uno vacío que ya no usás, tocá «Cerrar» y seguí acá.');

            return preguntar({
                titulo: titulo, mensajes: mensajes, filas: filas, textoAparte: 'Cargar otro nuevo aparte'
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
            // Lock de ESTE registro tomado por otra pestaña que no contesta el canal.
            function edicionSinResponder(resp, aj) {
                return aj.indexOf(nombreEdicion(idRegistro)) !== -1 &&
                    !resp.some(function (m) { return m.tipo === 'edicion' && m.idRegistro === idRegistro; });
            }
            return locksAjenos().then(function (ajenos) {
                return descubrir().then(function (respuestasIniciales) {
                    // Se reverifica unos instantes antes de dar el lock por colgado (puede ser una pestaña que se esta yendo o lenta).
                    return reverificarSinResponder(function (resp, aj) { return edicionSinResponder(resp, aj) ? 1 : 0; }, respuestasIniciales, ajenos)
                        .then(function (r) { return { respuestas: r.respuestas, ajenos: r.ajenos }; });
                }).then(function (visto) {
                    var respuestas = visto.respuestas;
                    ajenos = visto.ajenos;
                    var anteriores = respuestas.filter(function (m) { return m.tipo === 'edicion' && m.idRegistro === idRegistro && esAnterior(m); });
                    var hayLockSinResponder = edicionSinResponder(respuestas, ajenos);
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
                mensajes.push('Otra pestaña o ventana lo tiene abierto para modificar (' + textoLineas(otra.cantLineas) + (otra.lugar ? ', ' + otra.lugar : '') + (otra.abiertoEn ? ', ' + textoApertura(otra.abiertoEn) : '') + ').');
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

    window.DocumentoAbierto = { crear: crear, vistaPrevia: vistaPrevia, formatoKg: formatoKg };
})(window, document);
