// Utilidades: boton Instalar / Actualizar / Reinstalar del agente de balanza (2026-11-20).
// La pagina detecta el agente de ESTA PC en http://127.0.0.1:5100/status (version y estado) y la version publicada en
// Content/downloads/Carnisys.Balanza.Agent.json:
//  - sin agente  -> "Instalar": baja el .exe y muestra la guia para abrirlo (el navegador no deja ejecutar un archivo descargado;
//                   lo maximo es el boton «Abrir» de su barra de descargas) y detecta solo cuando el agente queda instalado;
//  - agente viejo -> "Actualizar": le pide al agente (POST /actualizar) que se baje y se instale solo la version publicada. El agente
//                   verifica la firma del .exe y solo acepta pedidos de los sitios autorizados en su config.json;
//  - al dia      -> "Reinstalar" (misma guia que Instalar).
// Si la actualizacion automatica no es posible (agente anterior sin /actualizar, sitio no autorizado, error) cae en la descarga guiada.
(function (window, document) {
    'use strict';

    // Direccion del agente local (por defecto la de siempre; el atributo data-api permite probar contra otro puerto).
    var API_BASE_POR_DEFECTO = 'http://127.0.0.1:5100';
    var API_BASE = API_BASE_POR_DEFECTO;
    var TIMEOUT_DETECCION_MS = 2000;
    var INTERVALO_ESPERA_INSTALACION_MS = 2500;
    var ESPERA_MAXIMA_INSTALACION_MS = 5 * 60 * 1000;
    var INTERVALO_ACTUALIZACION_MS = 1000;
    var ESPERA_MAXIMA_ACTUALIZACION_MS = 120 * 1000;

    var raiz = document.getElementById('agenteBalanza');
    if (!raiz) return;

    API_BASE = (raiz.getAttribute('data-api') || API_BASE_POR_DEFECTO).replace(/\/+$/, '');
    var urlManifiesto = raiz.getAttribute('data-manifiesto');
    var urlExe = raiz.getAttribute('data-exe');
    var nombreExe = raiz.getAttribute('data-nombre-exe') || 'Carnisys.Balanza.Agent.exe';
    var lblEstado = document.getElementById('agenteBalanzaEstado');
    var btn = document.getElementById('btnAgenteBalanza');
    var btnTexto = btn ? btn.querySelector('.agente-btn-texto') : null;
    var btnIcono = btn ? btn.querySelector('i') : null;

    var estado = { accion: 'instalar', agente: null, publicada: null };

    function conTimeout(url, opciones, ms) {
        var controlador = typeof AbortController === 'function' ? new AbortController() : null;
        var temporizador = controlador ? window.setTimeout(function () { controlador.abort(); }, ms) : null;
        var config = opciones || {};
        if (controlador) config.signal = controlador.signal;
        return window.fetch(url, config).then(function (r) {
            if (temporizador) window.clearTimeout(temporizador);
            return r;
        }, function (err) {
            if (temporizador) window.clearTimeout(temporizador);
            throw err;
        });
    }

    // Agente de esta PC: { version (null si es de una version anterior sin ese dato), actualizacion } o null si no responde.
    function detectarAgente() {
        return conTimeout(API_BASE + '/status', { cache: 'no-store' }, TIMEOUT_DETECCION_MS)
            .then(function (r) { return r.ok ? r.json() : null; })
            .then(function (j) {
                if (!j || j.ok !== true) return null;
                return { version: j.version || null, actualizacion: j.actualizacion || null };
            })
            .catch(function () { return null; });
    }

    // Version publicada en el servidor (string "x.y.z") o null.
    function leerPublicada() {
        return window.fetch(urlManifiesto, { cache: 'no-store' })
            .then(function (r) { return r.ok ? r.json() : null; })
            .then(function (j) { return j && j.version ? String(j.version) : null; })
            .catch(function () { return null; });
    }

    // -1 / 0 / 1 comparando x.y.z numericamente; una version ausente cuenta como la mas vieja.
    function compararVersiones(a, b) {
        if (!a && !b) return 0;
        if (!a) return -1;
        if (!b) return 1;
        var pa = a.split('.'), pb = b.split('.');
        for (var i = 0; i < 3; i++) {
            var x = parseInt(pa[i], 10) || 0, y = parseInt(pb[i], 10) || 0;
            if (x !== y) return x < y ? -1 : 1;
        }
        return 0;
    }

    function pintarBoton(accion, texto, icono, primario) {
        estado.accion = accion;
        btnTexto.textContent = texto;
        btnIcono.className = 'fas ' + icono + ' mr-1';
        btn.className = 'btn btn-sm ' + (primario ? 'btn-primary' : 'btn-outline-secondary');
        btn.disabled = false;
    }

    function refrescar() {
        return Promise.all([detectarAgente(), leerPublicada()]).then(function (r) {
            estado.agente = r[0];
            estado.publicada = r[1];

            if (!estado.agente) {
                lblEstado.textContent = 'El agente de balanza no está instalado en esta PC (o no está abierto).';
                pintarBoton('instalar', 'Instalar', 'fa-download', true);
            } else if (!estado.agente.version || compararVersiones(estado.agente.version, estado.publicada) < 0) {
                lblEstado.textContent = 'Agente instalado' + (estado.agente.version ? ' (versión ' + estado.agente.version + ')' : ' (versión anterior)') +
                    (estado.publicada ? '. Hay una versión nueva: ' + estado.publicada + '.' : '. Hay una versión nueva.');
                pintarBoton('actualizar', 'Actualizar', 'fa-sync-alt', true);
            } else {
                lblEstado.textContent = 'Agente instalado y al día (versión ' + estado.agente.version + ').';
                pintarBoton('reinstalar', 'Reinstalar', 'fa-download', false);
            }
        });
    }

    function dispararDescarga() {
        var enlace = document.createElement('a');
        enlace.href = urlExe;
        enlace.download = nombreExe;
        enlace.style.display = 'none';
        document.body.appendChild(enlace);
        enlace.click();
        document.body.removeChild(enlace);
    }

    // ---- Instalar: descarga guiada ----

    function guiaDescarga(introduccion) {
        dispararDescarga();
        var sondeo = null;
        var inicio = Date.now();

        window.Swal.fire({
            icon: 'info',
            title: 'Instalar el agente de balanza',
            html:
                '<div class="text-left small">' +
                '<p id="agenteGuiaIntro" class="mb-2"></p>' +
                '<ol class="pl-3 mb-2">' +
                '<li class="mb-1">Se está descargando <strong>' + nombreExe + '</strong>. Abrilo con el botón <strong>Abrir</strong> de la barra de descargas del navegador, o desde la carpeta <em>Descargas</em>.</li>' +
                '<li class="mb-1">Si Windows muestra <strong>«Windows protegió su PC»</strong>: tocá <strong>Más información</strong> y después <strong>Ejecutar de todas formas</strong>.</li>' +
                '<li class="mb-1">Esperá a que la ventana azul diga <strong>Listo</strong>. Se instala solo, sin permisos de administrador.</li>' +
                '</ol>' +
                '<div id="agenteGuiaEstado" class="font-weight-bold text-muted"><i class="fas fa-spinner fa-spin mr-1"></i>Esperando que se instale… (esta pantalla lo detecta sola)</div>' +
                '</div>',
            confirmButtonText: 'Cerrar',
            allowOutsideClick: false,
            didOpen: function () {
                var intro = document.getElementById('agenteGuiaIntro');
                if (intro) intro.textContent = introduccion || '';
                if (intro && !introduccion) intro.style.display = 'none';

                sondeo = window.setInterval(function () {
                    if (Date.now() - inicio > ESPERA_MAXIMA_INSTALACION_MS) {
                        window.clearInterval(sondeo);
                        var e = document.getElementById('agenteGuiaEstado');
                        if (e) e.textContent = 'No se detectó el agente. Si ya lo instalaste, cerrá este cuadro y recargá la página.';
                        return;
                    }
                    detectarAgente().then(function (agente) {
                        if (!agente || sondeo === null) return;
                        window.clearInterval(sondeo);
                        sondeo = null;
                        window.Swal.update({ icon: 'success', title: 'Agente de balanza instalado' });
                        var e = document.getElementById('agenteGuiaEstado');
                        if (e) {
                            e.className = 'font-weight-bold text-success';
                            e.textContent = 'Listo: el agente está funcionando' + (agente.version ? ' (versión ' + agente.version + ')' : '') + '.';
                        }
                        refrescar();
                    });
                }, INTERVALO_ESPERA_INSTALACION_MS);
            },
            willClose: function () {
                if (sondeo !== null) window.clearInterval(sondeo);
                sondeo = null;
                refrescar();
            }
        });
    }

    // ---- Actualizar: lo hace el agente ----

    function actualizarAutomatico() {
        btn.disabled = true;
        window.fetch(API_BASE + '/actualizar', { method: 'POST' }).then(function (r) {
            return r.json().then(function (j) { return { status: r.status, json: j || {} }; }, function () { return { status: r.status, json: {} }; });
        }).then(function (res) {
            if (res.status === 202) {
                mostrarProgresoActualizacion();
            } else if (res.status === 404) {
                // Agente anterior a la auto-actualizacion: no conoce /actualizar.
                guiaDescarga('Esta versión del agente todavía no se actualiza sola. Se instala la nueva a mano, una única vez.');
            } else {
                guiaDescarga((res.json.mensaje || 'No se pudo actualizar automáticamente.') + ' Se instala la nueva a mano.');
            }
        }).catch(function () {
            refrescar().then(function () { guiaDescarga('No se pudo hablar con el agente de esta PC. Se instala la nueva versión a mano.'); });
        });
    }

    function mostrarProgresoActualizacion() {
        var inicio = Date.now();
        var sondeo = null;
        var termino = false;
        var objetivo = estado.publicada;

        function terminar(ok, texto) {
            termino = true;
            if (sondeo !== null) window.clearInterval(sondeo);
            sondeo = null;
            window.Swal.update({ icon: ok ? 'success' : 'error', title: ok ? 'Agente actualizado' : 'No se pudo actualizar' });
            var p = document.getElementById('agenteActualizandoTexto');
            if (p) p.textContent = texto;
            window.Swal.hideLoading();
            var acciones = window.Swal.getActions();
            if (acciones) acciones.style.display = '';
        }

        window.Swal.fire({
            title: 'Actualizando el agente de balanza',
            html: '<p id="agenteActualizandoTexto" class="mb-0 small">Buscando una versión nueva…</p>',
            showConfirmButton: true,
            confirmButtonText: 'Cerrar',
            allowOutsideClick: false,
            didOpen: function () {
                window.Swal.showLoading();
                var acciones = window.Swal.getActions();
                if (acciones) acciones.style.display = 'none';

                sondeo = window.setInterval(function () {
                    if (termino) return;
                    if (Date.now() - inicio > ESPERA_MAXIMA_ACTUALIZACION_MS) {
                        terminar(false, 'La actualización tardó demasiado. Recargá la página y volvé a intentar; si sigue igual, usá «Reinstalar».');
                        return;
                    }
                    detectarAgente().then(function (agente) {
                        if (termino) return;
                        var texto = document.getElementById('agenteActualizandoTexto');
                        if (!agente) {
                            if (texto) texto.textContent = 'Reiniciando el agente…';
                            return;
                        }
                        var act = agente.actualizacion || {};
                        if (act.estado === 'error') {
                            terminar(false, (act.mensaje || 'Falló la actualización.') + ' Podés instalarla a mano con «Reinstalar».');
                        } else if (agente.version && compararVersiones(agente.version, objetivo) >= 0) {
                            if (act.estado === 'instalando') return;
                            terminar(true, 'Listo: el agente quedó en la versión ' + (agente.version || objetivo) + '.');
                        } else if (texto && act.mensaje) {
                            texto.textContent = act.mensaje;
                        }
                    });
                }, INTERVALO_ACTUALIZACION_MS);
            },
            willClose: function () {
                if (sondeo !== null) window.clearInterval(sondeo);
                sondeo = null;
                refrescar();
            }
        });
    }

    if (btn && btnTexto && btnIcono && lblEstado && window.fetch && window.Swal) {
        btn.addEventListener('click', function () {
            if (estado.accion === 'actualizar') actualizarAutomatico();
            else guiaDescarga();
        });
        btn.disabled = true;
        refrescar();
    }
})(window, document);
