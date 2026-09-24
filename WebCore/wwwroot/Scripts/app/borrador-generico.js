// Borradores en servidor de Compras/Stock/Movimientos/Embutidos: resguardo en el servidor + "Ver
// borradores sin guardar". Analogo generico de pos-borrador.js -- ver docs/DECISIONS.md "Borradores de
// Compras/Stock/Movimientos/Embutidos".
//
// A diferencia de POSBorrador (singleton, un solo carrito por pestana), BorradorGenerico es una
// FACTORY: cada pantalla que lo usa llama BorradorGenerico.crear(config) y se queda con su propia
// instancia, porque un mismo tipo de formulario puede abrirse embebido en mas de un lugar (ej.
// Compras dentro de un modal de Cta. Cte.).
//
// El config no asume nombres de globals de cada modulo (a diferencia de POSDraft/POSState en POS):
// recibe callbacks (obtenerSnapshot, obtenerResumen, obtenerCantLineas, etc.) que cada pantalla
// implementa reusando literalmente lo que ya arma su buildDraft($form) local.
//
// Reglas que conviene no romper (identicas a pos-borrador.js):
//  - Nunca debe frenar ni ensuciar el formulario: cualquier falla de red/servidor se ignora en
//    silencio (el borrador local en localStorage sigue siendo el respaldo cuando esta activo).
//  - Las horas las pone el servidor; aca solo se mandan duraciones/estado, nunca fechas armadas en el navegador.
//  - Todo texto que viene del servidor se pinta con textContent, nunca con innerHTML.
(function (window, $) {
    'use strict';

    var DEBOUNCE_GUARDADO_MS = 1500;

    function nuevoGuid() {
        if (window.crypto && typeof window.crypto.randomUUID === 'function') return window.crypto.randomUUID();
        var bytes = new Uint8Array(16);
        window.crypto.getRandomValues(bytes);
        bytes[6] = (bytes[6] & 0x0f) | 0x40;
        bytes[8] = (bytes[8] & 0x3f) | 0x80;
        var hex = Array.prototype.map.call(bytes, function (b) { return ('0' + b.toString(16)).slice(-2); }).join('');
        return hex.slice(0, 8) + '-' + hex.slice(8, 12) + '-' + hex.slice(12, 16) + '-' + hex.slice(16, 20) + '-' + hex.slice(20);
    }

    function token() {
        var input = document.querySelector('#globalAntiForgeryToken input[name="__RequestVerificationToken"]')
            || document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    // POST JSON con antiforgery. keepalive=true permite que la llamada sobreviva al cierre de la pestana.
    // Devuelve siempre una promesa que resuelve a un objeto (nunca rechaza): { ok:false } si algo falla.
    function post(url, cuerpo, keepalive) {
        if (!url) return Promise.resolve({ ok: false });
        return fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            keepalive: keepalive === true,
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token() },
            body: JSON.stringify(cuerpo || {})
        }).then(function (respuesta) {
            return respuesta.json().catch(function () { return { ok: false, httpStatus: respuesta.status }; });
        }).catch(function () {
            return { ok: false, sinConexion: true };
        });
    }

    function el(tag, clase, texto) {
        var nodo = document.createElement(tag);
        if (clase) nodo.className = clase;
        if (texto != null) nodo.textContent = texto;
        return nodo;
    }

    function horaLocal() {
        var d = new Date();
        function dos(n) { return ('0' + n).slice(-2); }
        return dos(d.getHours()) + ':' + dos(d.getMinutes()) + ':' + dos(d.getSeconds());
    }

    function pedirSupervisor(mensaje) {
        var $html = $('<div></div>');
        $html.append($('<p class="text-left"></p>').text(mensaje || 'Hace falta la autorización de un supervisor.'));
        $html.append('<input id="swalSupervisorUsuarioBG" class="swal2-input" placeholder="Usuario del supervisor" autocomplete="off">');
        $html.append('<input id="swalSupervisorClaveBG" type="password" class="swal2-input" placeholder="Contraseña" autocomplete="off">');

        return Swal.fire({
            title: 'Autorización de supervisor',
            html: $html.get(0),
            focusConfirm: false,
            showCancelButton: true,
            confirmButtonText: 'Autorizar',
            cancelButtonText: 'Cancelar',
            preConfirm: function () {
                var usuario = ($('#swalSupervisorUsuarioBG').val() || '').trim();
                var clave = $('#swalSupervisorClaveBG').val() || '';
                if (!usuario || !clave) {
                    Swal.showValidationMessage('Ingresá el usuario y la contraseña del supervisor.');
                    return false;
                }
                return { usuario: usuario, clave: clave };
            }
        }).then(function (r) { return r.isConfirmed ? r.value : null; });
    }

    function crear(cfg) {
        cfg = cfg || {};
        var urls = cfg.urls || {};
        var modulo = cfg.modulo;
        var habilitado = cfg.habilitado === true && !!modulo && !!urls.guardar;

        var $modal = $(cfg.modalSelector || '#modalBorradoresGenerico');
        var $contenedor = $(cfg.contenedorSelector || '#contenedorBorradoresGenerico');
        var $badge = $(cfg.badgeSelector || '#badgeBorradoresGenerico');

        var estado = {
            clientId: null,
            creadoEnServidor: false,
            pendiente: false,
            enviando: false,
            reintentar: false,
            timerGuardado: null,
            timerLatido: null
        };

        function idSucursal() { return (cfg.obtenerIdSucursal && cfg.obtenerIdSucursal()) || 0; }
        function idRegistro() { return (cfg.obtenerIdRegistro && cfg.obtenerIdRegistro()) || null; }
        function idOperador() { return (cfg.obtenerIdOperador && cfg.obtenerIdOperador()) || 0; }
        function nombreOperador() { return (cfg.obtenerNombreOperador && cfg.obtenerNombreOperador()) || ''; }

        function snapshotActual() {
            return typeof cfg.obtenerSnapshot === 'function' ? cfg.obtenerSnapshot() : null;
        }

        function cantLineasActual() {
            return typeof cfg.obtenerCantLineas === 'function' ? (cfg.obtenerCantLineas() || 0) : 0;
        }

        function resumenActual() {
            return typeof cfg.obtenerResumen === 'function' ? (cfg.obtenerResumen() || '') : '';
        }

        function mostrarEstado(texto, esError) {
            if (!cfg.estadoSelector) return;
            var $el = $(cfg.estadoSelector);
            if (!$el.length) return;
            $el.text(texto || '')
                .toggleClass('d-none', !texto)
                .toggleClass('text-danger', !!esError)
                .toggleClass('text-muted', !esError);
        }

        function getClientId() {
            if (!habilitado) return null;
            if (!estado.clientId) estado.clientId = nuevoGuid();
            return estado.clientId;
        }

        // Retoma el clientId de un borrador recuperado: el servidor puede o no conocerlo, se resuelve en
        // el proximo guardado.
        function adoptarClientId(clientId) {
            if (!habilitado) return;
            estado.clientId = clientId || nuevoGuid();
            estado.creadoEnServidor = false;
            estado.pendiente = true;
        }

        // Nuevo formulario (tras guardar/descartar, o si el servidor ya cerro el anterior).
        function reiniciar() {
            clearTimeout(estado.timerGuardado);
            estado.clientId = null;
            estado.creadoEnServidor = false;
            estado.pendiente = false;
            estado.reintentar = false;
            mostrarEstado('');
        }

        function deshabilitar() {
            habilitado = false;
            clearTimeout(estado.timerGuardado);
            clearInterval(estado.timerLatido);
            mostrarEstado('');
        }

        function finalizando() {
            return typeof cfg.finalizando === 'function' && cfg.finalizando() === true;
        }

        function enviarSnapshot() {
            if (!habilitado || finalizando()) return;
            if (estado.enviando) { estado.reintentar = true; return; }

            var snap = snapshotActual();
            if (!snap) return;

            estado.enviando = true;
            estado.pendiente = false;
            estado.reintentar = false;

            post(urls.guardar, {
                modulo: modulo,
                clientId: getClientId(),
                idSucursal: idSucursal(),
                idRegistro: idRegistro(),
                idOperador: idOperador(),
                nombreOperador: nombreOperador(),
                soloLatido: false,
                resumen: resumenActual(),
                cantLineas: cantLineasActual(),
                payload: snap
            }).then(function (resp) {
                estado.enviando = false;

                if (resp.deshabilitado) { deshabilitar(); return; }

                // El servidor ya cerro ese borrador (se guardo o se descarto) o es de otro operador: se
                // sigue con un clientId nuevo para que lo que hay en pantalla no se pierda.
                if ((resp.ok && resp.cerrado) || resp.ajeno) {
                    if (finalizando()) return;
                    estado.clientId = nuevoGuid();
                    estado.creadoEnServidor = false;
                    if (cantLineasActual() > 0) programarGuardado();
                    return;
                }

                if (resp.ok) {
                    estado.creadoEnServidor = true;
                    mostrarEstado(cantLineasActual() > 0 ? 'Borrador resguardado ' + horaLocal() : '');
                } else {
                    estado.pendiente = true;
                    mostrarEstado(resp.sinConexion || resp.httpStatus
                        ? 'Sin conexión con el servidor: el borrador solo está guardado en esta PC'
                        : '', true);
                }

                if (estado.reintentar || (estado.pendiente && resp.ok)) programarGuardado();
            });
        }

        // Agenda el guardado del formulario actual (debounce). Se llama en cada cambio del carrito.
        function programarGuardado() {
            if (!habilitado || finalizando()) return;

            var snap = snapshotActual();
            if (!snap) return;
            if (cantLineasActual() <= 0 && !estado.creadoEnServidor) return;

            estado.pendiente = true;
            clearTimeout(estado.timerGuardado);
            estado.timerGuardado = setTimeout(enviarSnapshot, estado.creadoEnServidor ? DEBOUNCE_GUARDADO_MS : 0);
        }

        function latido() {
            if (!habilitado || finalizando()) return;
            if (cantLineasActual() <= 0) return;

            if (!estado.creadoEnServidor || estado.pendiente) { enviarSnapshot(); return; }

            post(urls.guardar, {
                modulo: modulo,
                clientId: getClientId(),
                idSucursal: idSucursal(),
                idRegistro: idRegistro(),
                idOperador: idOperador(),
                nombreOperador: nombreOperador(),
                soloLatido: true
            }).then(function (resp) {
                if (resp.deshabilitado) { deshabilitar(); return; }
                if (resp.ok && resp.necesitaGuardar) { estado.creadoEnServidor = false; enviarSnapshot(); return; }
                if (!resp.ok && resp.sinConexion) mostrarEstado('Sin conexión con el servidor: el borrador solo está guardado en esta PC', true);
                else if (resp.ok) mostrarEstado('Borrador resguardado ' + horaLocal());
            });
        }

        // Marca el borrador como guardado (el registro real ya se confirmo): el servidor lo pasa a
        // FINALIZADA y vacia el payload. Se llama en el success del submit, junto al clearDraft local.
        function marcarFinalizado(idResultado) {
            if (!habilitado || !estado.creadoEnServidor) { reiniciar(); return; }
            post(urls.marcarFinalizado, {
                modulo: modulo,
                clientId: getClientId(),
                idResultado: idResultado || 0
            });
            reiniciar();
        }

        // Aviso de cierre de pestana con el formulario sin guardar. Un corte de luz no dispara nada: se
        // distingue en el servidor por la falta de latido.
        function alCerrarPestana() {
            if (!habilitado || !estado.creadoEnServidor) return;
            if (finalizando()) return;
            if (cantLineasActual() <= 0) return;

            post(urls.evento, {
                modulo: modulo,
                clientId: getClientId(),
                idSucursal: idSucursal(),
                idOperador: idOperador()
            }, true);
        }

        // ------------------------------------------------------------------------------------------
        // "Ver borradores sin guardar"
        // ------------------------------------------------------------------------------------------

        function actualizarBadge(cantidad) {
            var n = cantidad || 0;
            $badge.text(n).toggleClass('d-none', n === 0);
        }

        // Arma la tabla de lineas de un borrador para la fila de detalle desplegable. cfg.renderizarLineas
        // (por modulo, ver compras.js/stock.js/movimientos.js/elaborados-*.js) recibe el payload crudo
        // (forma especifica de cada modulo) y devuelve un array de {codigo, producto, cantidad}. Sin
        // ese callback (o si no devuelve nada), se muestra un aviso en vez de inventar una forma.
        function construirDetalleLineas(p) {
            var lineas = typeof cfg.renderizarLineas === 'function' ? (cfg.renderizarLineas(p.payload) || []) : null;

            if (!lineas || !lineas.length) {
                return el('div', 'p-2 text-muted small', 'Sin detalle de líneas disponible.');
            }

            var tabla = el('table', 'table table-sm table-bordered mb-0 bg-white');
            var thead = el('thead');
            var trh = el('tr');
            ['Código', 'Producto', 'Cantidad'].forEach(function (t) { trh.appendChild(el('th', null, t)); });
            thead.appendChild(trh);
            tabla.appendChild(thead);

            var tbody = el('tbody');
            lineas.forEach(function (l) {
                var tr = el('tr');
                tr.appendChild(el('td', null, l.codigo || ''));
                tr.appendChild(el('td', null, l.producto || ''));
                tr.appendChild(el('td', 'text-right', l.cantidad || ''));
                tbody.appendChild(tr);
            });
            tabla.appendChild(tbody);

            var envoltorio = el('div', 'p-2');
            envoltorio.appendChild(tabla);
            return envoltorio;
        }

        function pintarLista(resp) {
            var cont = $contenedor.get(0);
            if (!cont) return;
            cont.textContent = '';

            var pendientes = (resp && resp.pendientes) || [];
            actualizarBadge((resp && resp.interrumpidas) || 0);

            if (!pendientes.length) {
                cont.appendChild(el('div', 'p-4 text-center text-muted', 'No hay borradores sin guardar en esta sucursal.'));
                return;
            }

            var tabla = el('table', 'table table-sm table-hover mb-0');
            var thead = el('thead', 'thead-light');
            var trh = el('tr');
            ['', 'Usuario', 'Inicio', 'Última señal', 'Estado', 'Resumen', ''].forEach(function (t) { trh.appendChild(el('th', null, t)); });
            thead.appendChild(trh);
            tabla.appendChild(thead);

            var tbody = el('tbody');
            pendientes.forEach(function (p) {
                var tr = el('tr', 'borrador-generico-fila');
                tr.style.cursor = 'pointer';
                tr.title = 'Doble clic para ver las líneas';

                var tdChevron = el('td', 'text-center');
                var btnChevron = el('button', 'btn btn-sm btn-link p-0 js-toggle-detalle', null);
                btnChevron.type = 'button';
                var icoChevron = el('i', 'fas fa-chevron-down');
                btnChevron.appendChild(icoChevron);
                btnChevron.title = 'Ver líneas';
                tdChevron.appendChild(btnChevron);
                tr.appendChild(tdChevron);

                var tdUsuario = el('td', null, p.operador);
                if (p.esMio) tdUsuario.appendChild(el('span', 'badge badge-info ml-1', 'mío'));
                tr.appendChild(tdUsuario);
                tr.appendChild(el('td', 'text-nowrap', p.inicio));
                tr.appendChild(el('td', 'text-nowrap', p.sinLatido));

                var tdEstado = el('td');
                tdEstado.appendChild(el('span', 'badge ' + (p.puedeCargar ? 'badge-warning' : 'badge-secondary'), p.estado));
                tr.appendChild(tdEstado);

                tr.appendChild(el('td', null, p.resumen));

                var tdAcciones = el('td', 'text-nowrap text-right');

                var btnCargar = el('button', 'btn btn-sm btn-success mr-1', 'Cargar');
                btnCargar.type = 'button';
                btnCargar.disabled = !p.puedeCargar;
                if (!p.puedeCargar) btnCargar.title = 'Está en uso en otra terminal';
                btnCargar.addEventListener('click', function (e) { e.stopPropagation(); cargar(p, null); });
                tdAcciones.appendChild(btnCargar);

                var btnDescartar = el('button', 'btn btn-sm btn-outline-danger', 'Descartar');
                btnDescartar.type = 'button';
                btnDescartar.disabled = !p.puedeCargar;
                if (!p.puedeCargar) btnDescartar.title = 'Está en uso en otra terminal';
                btnDescartar.addEventListener('click', function (e) { e.stopPropagation(); descartar(p, null); });
                tdAcciones.appendChild(btnDescartar);

                tr.appendChild(tdAcciones);
                tbody.appendChild(tr);

                // Fila de detalle con las lineas (oculta hasta desplegarla): interpretada del lado del
                // cliente por cada modulo (cfg.renderizarLineas), el payload es opaco para este JS
                // compartido. Se arma una sola vez y se cachea en el propio <tr>, no en cada toggle.
                var trDetalle = el('tr', 'd-none bg-light');
                var tdDetalle = el('td');
                tdDetalle.colSpan = 7;
                trDetalle.appendChild(tdDetalle);
                tbody.appendChild(trDetalle);

                var detalleCargado = false;
                function toggleDetalle() {
                    if (!detalleCargado) {
                        tdDetalle.appendChild(construirDetalleLineas(p));
                        detalleCargado = true;
                    }
                    trDetalle.classList.toggle('d-none');
                    icoChevron.classList.toggle('fa-chevron-down');
                    icoChevron.classList.toggle('fa-chevron-up');
                }

                btnChevron.addEventListener('click', function (e) { e.stopPropagation(); toggleDetalle(); });
                tr.addEventListener('dblclick', function () { toggleDetalle(); });
            });
            tabla.appendChild(tbody);

            var envoltorio = el('div', 'table-responsive');
            envoltorio.appendChild(tabla);
            cont.appendChild(envoltorio);
        }

        function cargarLista() {
            var cont = $contenedor.get(0);
            if (cont) cont.appendChild(el('div', 'p-4 text-center text-muted', 'Cargando...'));

            return post(urls.listar, {
                modulo: modulo,
                idSucursal: idSucursal(),
                idOperador: idOperador(),
                clientIdLocal: estado.clientId
            }).then(function (resp) {
                if (!resp.ok) {
                    if (cont) {
                        cont.textContent = '';
                        cont.appendChild(el('div', 'p-4 text-center text-danger', 'No se pudieron consultar los borradores sin guardar.'));
                    }
                    return resp;
                }
                pintarLista(resp);
                return resp;
            });
        }

        function abrirModalBorradores() {
            if (!habilitado || !$modal.length) return;
            var cont = $contenedor.get(0);
            if (cont) cont.textContent = '';
            $modal.modal('show');
            cargarLista();
        }

        function cargar(item, supervisor) {
            if (typeof cfg.hayFormularioCargado === 'function' && cfg.hayFormularioCargado()) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Hay un formulario en pantalla',
                    text: 'Guardá o cancelá el formulario actual antes de cargar otro borrador.'
                });
                return;
            }

            post(urls.recuperar, {
                modulo: modulo,
                id: item.id,
                idSucursal: idSucursal(),
                idOperador: idOperador(),
                supervisor: supervisor
            }).then(function (resp) {
                if (resp.requiereSupervisor) {
                    pedirSupervisor(resp.msg).then(function (sup) { if (sup) cargar(item, sup); });
                    return;
                }

                if (!resp.ok) {
                    Swal.fire({ icon: 'error', title: 'No se pudo cargar', text: resp.msg || 'Intentá de nuevo.' });
                    cargarLista();
                    return;
                }

                adoptarClientId(resp.clientId);
                if (typeof cfg.aplicarPayload === 'function') cfg.aplicarPayload(resp.payload, resp.clientId, resp.idRegistro);
                $modal.modal('hide');
                actualizarBadge(0);
                Swal.fire({
                    icon: 'info',
                    title: 'Borrador cargado',
                    text: 'Revisá los datos antes de guardar: son los de cuando se armó el borrador.',
                    timer: 3500,
                    timerProgressBar: true
                });
            });
        }

        function descartar(item, supervisor) {
            var pedirMotivo = supervisor
                ? Promise.resolve({ isConfirmed: true, value: item.motivo })
                : Swal.fire({
                    icon: 'warning',
                    title: 'Descartar borrador sin guardar',
                    text: 'Indicá el motivo. Queda registrado y se le avisa al administrador.',
                    input: 'text',
                    inputAttributes: { maxlength: 300, autocomplete: 'off' },
                    showCancelButton: true,
                    confirmButtonText: 'Descartar',
                    cancelButtonText: 'Cancelar',
                    inputValidator: function (valor) { return (valor || '').trim() ? null : 'El motivo es obligatorio.'; }
                });

            pedirMotivo.then(function (r) {
                if (!r.isConfirmed) return;
                item.motivo = (r.value || '').trim();

                post(urls.descartar, {
                    modulo: modulo,
                    id: item.id,
                    idSucursal: idSucursal(),
                    idOperador: idOperador(),
                    motivo: item.motivo,
                    supervisor: supervisor
                }).then(function (resp) {
                    if (resp.requiereSupervisor) {
                        pedirSupervisor(resp.msg).then(function (sup) { if (sup) descartar(item, sup); });
                        return;
                    }

                    if (!resp.ok) {
                        Swal.fire({ icon: 'error', title: 'No se pudo descartar', text: resp.msg || 'Intentá de nuevo.' });
                    }
                    cargarLista();
                });
            });
        }

        // Cuenta los borradores interrumpidos para el badge, sin abrir el modal. Se llama al cargar la pantalla.
        function actualizarBadgeInicial() {
            if (!habilitado) return;
            post(urls.listar, { modulo: modulo, idSucursal: idSucursal(), idOperador: idOperador(), clientIdLocal: estado.clientId })
                .then(function (resp) {
                    if (resp.deshabilitado) { deshabilitar(); return; }
                    if (!resp.ok) return;
                    actualizarBadge(resp.interrumpidas || 0);
                });
        }

        if (habilitado) {
            clearInterval(estado.timerLatido);
            estado.timerLatido = setInterval(latido, (cfg.latidoSegundos || 20) * 1000);
            window.addEventListener('pagehide', alCerrarPestana);
        }

        return {
            estaHabilitado: function () { return habilitado; },
            getClientId: getClientId,
            adoptarClientId: adoptarClientId,
            reiniciar: reiniciar,
            programarGuardado: programarGuardado,
            marcarFinalizado: marcarFinalizado,
            abrirModalBorradores: abrirModalBorradores,
            actualizarBadgeInicial: actualizarBadgeInicial
        };
    }

    window.BorradorGenerico = { crear: crear };
})(window, window.jQuery);
