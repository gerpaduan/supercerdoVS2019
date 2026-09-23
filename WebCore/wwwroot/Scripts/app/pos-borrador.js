// Ventas en curso del POS: resguardo en el servidor + "Ver ventas sin guardar" (panel de Ayuda F1).
// Ver docs/DECISIONS.md "Ventas en curso: borrador en servidor y advertencias del POS".
//
// Que hace:
//  - Cada vez que cambia el carrito (POSDraft.autosave, enganchado en pos-cart.js updateSaleState) manda el
//    carrito al servidor con un debounce corto, y late cada N segundos mientras haya items: asi una venta
//    interrumpida (corte de luz, cierre forzado, cierre de caja) deja rastro y se puede recuperar.
//  - Modal "Ver ventas sin guardar": lista las de toda la sucursal (usuario e items) y permite cargarlas al
//    carrito o descartarlas (motivo obligatorio; las de otro usuario piden autorizacion de un supervisor).
//  - Badge con la cantidad de ventas interrumpidas sobre el boton de Ayuda.
//
// Reglas que conviene no romper:
//  - Nunca debe frenar ni ensuciar la venta: cualquier falla de red/servidor se ignora en silencio (el POSDraft
//    local sigue siendo el respaldo).
//  - Las horas las pone el servidor; aca solo se mandan duraciones (performance.now()).
//  - Todo texto que viene del servidor se pinta con textContent, nunca con innerHTML.
(function (window, $) {
    'use strict';

    // Espera antes de mandar el carrito tras un cambio (agrupa ediciones seguidas). El primer guardado va sin demora.
    var DEBOUNCE_GUARDADO_MS = 1500;

    var cfg = window.POSBorradorConfig || {};
    var habilitado = false;

    var estado = {
        clientId: null,
        creadoEnServidor: false,   // el servidor ya conoce este clientId
        pendiente: false,          // hay cambios locales sin mandar
        enviando: false,
        reintentar: false,
        timerGuardado: null,
        timerLatido: null
    };

    // ------------------------------------------------------------------------------------------
    // Utilidades
    // ------------------------------------------------------------------------------------------

    function nuevoGuid() {
        if (window.crypto && typeof window.crypto.randomUUID === 'function') return window.crypto.randomUUID();
        // Respaldo para navegadores sin randomUUID (contexto no seguro): UUID v4 con getRandomValues.
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

    function posInstanceId() {
        return (window.POSModo && window.POSModo.instanceId) || cfg.posInstanceId || '';
    }

    function idSucursalPOS() {
        return parseInt($('#idSucursalPOS').val(), 10) || 0;
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

    function hayLineas(snap) {
        return !!(snap && Array.isArray(snap.lineas) && snap.lineas.some(function (l) { return !!l; }));
    }

    function hayLineasActivas(snap) {
        return !!(snap && Array.isArray(snap.lineas) && snap.lineas.some(function (l) { return !!l && !l.anulado; }));
    }

    function snapshotActual() {
        return window.POSDraft && typeof window.POSDraft.snapshot === 'function' ? window.POSDraft.snapshot() : null;
    }

    function mostrarEstado(texto, esError) {
        var $el = $('#posBorradorEstado');
        if (!$el.length) return;
        $el.text(texto || '')
            .toggleClass('d-none', !texto)
            .toggleClass('text-danger', !!esError)
            .toggleClass('text-muted', !esError);
    }

    function horaLocal() {
        var d = new Date();
        function dos(n) { return ('0' + n).slice(-2); }
        return dos(d.getHours()) + ':' + dos(d.getMinutes()) + ':' + dos(d.getSeconds());
    }

    // ------------------------------------------------------------------------------------------
    // Identidad del carrito
    // ------------------------------------------------------------------------------------------

    function getClientId() {
        if (!habilitado) return null;
        if (!estado.clientId) estado.clientId = nuevoGuid();
        return estado.clientId;
    }

    // Retoma el clientId de un borrador local o recuperado: el servidor puede o no conocerlo, se resuelve en
    // el primer guardado.
    function adoptarClientId(clientId) {
        if (!habilitado) return;
        estado.clientId = clientId || nuevoGuid();
        estado.creadoEnServidor = false;
        estado.pendiente = true;
    }

    // Nuevo carrito (tras finalizar/descartar, o si el servidor ya cerro el anterior).
    function reiniciar() {
        clearTimeout(estado.timerGuardado);
        estado.clientId = null;
        estado.creadoEnServidor = false;
        estado.pendiente = false;
        estado.reintentar = false;
        mostrarEstado('');
    }

    // ------------------------------------------------------------------------------------------
    // Guardado en el servidor
    // ------------------------------------------------------------------------------------------

    function deshabilitar() {
        habilitado = false;
        clearTimeout(estado.timerGuardado);
        clearInterval(estado.timerLatido);
        mostrarEstado('');
    }

    // Mientras se confirma el cobro (o ya se cobro) no se guarda nada mas: el servidor cierra ese carrito y un
    // guardado tardio crearia una "venta sin guardar" fantasma.
    function finalizando() {
        return window.POSFinalizandoVenta === true || window.POSVentaFinalizada === true;
    }

    function enviarSnapshot() {
        if (!habilitado || finalizando()) return;
        if (estado.enviando) { estado.reintentar = true; return; }

        var snap = snapshotActual();
        if (!snap) return;

        estado.enviando = true;
        estado.pendiente = false;
        estado.reintentar = false;

        var clientIdEnviado = getClientId();
        post(cfg.urls.guardar, {
            clientId: clientIdEnviado,
            posInstanceId: posInstanceId(),
            idSucursalPOS: idSucursalPOS(),
            soloLatido: false,
            payload: snap
        }).then(function (resp) {
            estado.enviando = false;

            if (resp.deshabilitado) { deshabilitar(); return; }

            // El servidor ya cerro ese carrito (se finalizo o se descarto) o es de otro operador: se sigue
            // con un clientId nuevo para que lo que hay en pantalla no se pierda.
            if ((resp.ok && resp.cerrado) || resp.ajeno) {
                if (finalizando()) return;
                estado.clientId = nuevoGuid();
                estado.creadoEnServidor = false;
                if (hayLineas(snap)) programarGuardado();
                return;
            }

            if (resp.ok) {
                estado.creadoEnServidor = true;
                mostrarEstado(hayLineas(snap) ? 'Venta resguardada ' + horaLocal() : '');
            } else {
                // Sin conexion o rechazo del servidor: queda el respaldo local; se reintenta en el proximo latido.
                estado.pendiente = true;
                mostrarEstado(resp.sinConexion || resp.httpStatus
                    ? 'Sin conexión con el servidor: la venta solo está guardada en esta PC'
                    : '', true);
            }

            if (estado.reintentar || estado.pendiente && resp.ok) programarGuardado();
        });
    }

    // Agenda el guardado del carrito actual (debounce). Lo llama POSDraft.autosave() en cada cambio.
    function programarGuardado() {
        if (!habilitado || finalizando()) return;

        var snap = snapshotActual();
        if (!snap) return;

        // Sin items y sin registro en el servidor no hay nada que resguardar.
        if (!hayLineas(snap) && !estado.creadoEnServidor) return;

        estado.pendiente = true;
        clearTimeout(estado.timerGuardado);
        estado.timerGuardado = setTimeout(enviarSnapshot, estado.creadoEnServidor ? DEBOUNCE_GUARDADO_MS : 0);
    }

    function latido() {
        if (!habilitado || finalizando()) return;

        var snap = snapshotActual();
        if (!hayLineasActivas(snap)) return;

        // Si nunca se pudo guardar o quedaron cambios pendientes, se manda el carrito entero.
        if (!estado.creadoEnServidor || estado.pendiente) { enviarSnapshot(); return; }

        post(cfg.urls.guardar, {
            clientId: getClientId(),
            posInstanceId: posInstanceId(),
            idSucursalPOS: idSucursalPOS(),
            soloLatido: true
        }).then(function (resp) {
            if (resp.deshabilitado) { deshabilitar(); return; }
            if (resp.ok && resp.necesitaGuardar) { estado.creadoEnServidor = false; enviarSnapshot(); return; }
            if (!resp.ok && resp.sinConexion) mostrarEstado('Sin conexión con el servidor: la venta solo está guardada en esta PC', true);
            else if (resp.ok) mostrarEstado('Venta resguardada ' + horaLocal());
        });
    }

    // Aviso de cierre de pestana con la venta sin finalizar. Un corte de luz no dispara nada: se distingue
    // en el servidor por la falta de latido.
    function alCerrarPestana() {
        if (!habilitado || !estado.creadoEnServidor) return;
        if (window.PosNavegandoInternoPOS || window.POSFinalizandoVenta || window.POSVentaFinalizada) return;
        if (!hayLineasActivas(snapshotActual())) return;

        post(cfg.urls.evento, {
            clientId: getClientId(),
            posInstanceId: posInstanceId(),
            idSucursalPOS: idSucursalPOS()
        }, true);
    }

    // ------------------------------------------------------------------------------------------
    // "Ver ventas sin guardar"
    // ------------------------------------------------------------------------------------------

    function hayCarritoCargado() {
        var lineas = (window.POSState && window.POSState.getLineas && window.POSState.getLineas()) || [];
        return lineas.some(function (l) { return !!l; });
    }

    function el(tag, clase, texto) {
        var nodo = document.createElement(tag);
        if (clase) nodo.className = clase;
        if (texto != null) nodo.textContent = texto;
        return nodo;
    }

    function pedirSupervisor(mensaje) {
        var $html = $('<div></div>');
        $html.append($('<p class="text-left"></p>').text(mensaje || 'Hace falta la autorización de un supervisor.'));
        $html.append('<input id="swalSupervisorUsuario" class="swal2-input" placeholder="Usuario del supervisor" autocomplete="off">');
        $html.append('<input id="swalSupervisorClave" type="password" class="swal2-input" placeholder="Contraseña" autocomplete="off">');

        return Swal.fire({
            title: 'Autorización de supervisor',
            html: $html.get(0),
            focusConfirm: false,
            showCancelButton: true,
            confirmButtonText: 'Autorizar',
            cancelButtonText: 'Cancelar',
            preConfirm: function () {
                var usuario = ($('#swalSupervisorUsuario').val() || '').trim();
                var clave = $('#swalSupervisorClave').val() || '';
                if (!usuario || !clave) {
                    Swal.showValidationMessage('Ingresá el usuario y la contraseña del supervisor.');
                    return false;
                }
                return { usuario: usuario, clave: clave };
            }
        }).then(function (r) { return r.isConfirmed ? r.value : null; });
    }

    function actualizarBadge(cantidad) {
        var n = cantidad || 0;
        $('#badgeVentasSinCerrar, #badgeVentasSinCerrarAyuda')
            .text(n)
            .toggleClass('d-none', n === 0);
    }

    // Campana de "ventas sin guardar" del cajero (topbar de POS, ver _LayoutPOS.cshtml): a
    // diferencia del badge de Ayuda (cuenta interrumpidas de TODA la sucursal), esta lista solo
    // las propias (esMia), incluidas las que siguen "en uso" en esta misma pestana (asi el cajero
    // ve que tiene una venta en curso, aunque todavia no pueda cargarla desde otra sesion). Reusa
    // el mismo fetch de ListarBorradoresPOS que ya hace actualizarBadgeInicial/cargarLista -- sin
    // pedido extra al servidor.
    function pintarCampanaPropia(resp) {
        var $badge = $('#campanaVentasPOSContador');
        var $lista = $('#campanaVentasPOSLista');
        if (!$badge.length && !$lista.length) return;

        var propias = ((resp && resp.pendientes) || []).filter(function (p) { return p.esMia; });

        $badge.text(propias.length).toggleClass('d-none', propias.length === 0);

        if (!$lista.length) return;
        $lista.empty();

        if (!propias.length) {
            $lista.append($('<div class="px-3 py-2 text-muted small"></div>').text('Ninguna por ahora.'));
            return;
        }

        propias.forEach(function (p) {
            var $item = $('<div class="px-3 py-2 border-bottom small"></div>');
            $item.append($('<div class="d-flex justify-content-between align-items-center"></div>')
                .append($('<span></span>').text(p.cliente + ' — ' + p.total))
                .append($('<span class="badge ' + (p.puedeCargar ? 'badge-warning' : 'badge-secondary') + '"></span>').text(p.estado)));
            $item.append($('<div class="text-muted"></div>').text(p.cantLineas + ' ítem(s) · ' + p.sinLatido));

            if (p.puedeCargar) {
                var $btn = $('<button type="button" class="btn btn-sm btn-success mt-1 w-100">Cargar al carrito</button>');
                $btn.on('click', function () { cargar(p, null); });
                $item.append($btn);
            }

            $lista.append($item);
        });
    }

    // Pinta la tabla de ventas sin guardar en el modal.
    function pintarLista(resp) {
        var cont = document.getElementById('contenedorVentasSinCerrarPOS');
        if (!cont) return;
        cont.textContent = '';

        var pendientes = (resp && resp.pendientes) || [];
        actualizarBadge((resp && resp.interrumpidas) || 0);

        if (!pendientes.length) {
            cont.appendChild(el('div', 'p-4 text-center text-muted', 'No hay ventas sin guardar en esta sucursal.'));
            return;
        }

        var tabla = el('table', 'table table-sm table-hover mb-0');
        var thead = el('thead', 'thead-light');
        var trh = el('tr');
        ['Usuario', 'Inicio', 'Última señal', 'Estado', 'Cliente', 'Total', ''].forEach(function (t) { trh.appendChild(el('th', null, t)); });
        thead.appendChild(trh);
        tabla.appendChild(thead);

        var tbody = el('tbody');
        pendientes.forEach(function (p) {
            var tr = el('tr');

            var tdUsuario = el('td', null, p.operador);
            if (p.esMia) tdUsuario.appendChild(el('span', 'badge badge-info ml-1', 'mía'));
            tr.appendChild(tdUsuario);
            tr.appendChild(el('td', 'text-nowrap', p.inicio));
            tr.appendChild(el('td', 'text-nowrap', p.ultimoLatido + ' (' + p.sinLatido + ')'));

            var tdEstado = el('td');
            tdEstado.appendChild(el('span', 'badge ' + (p.puedeCargar ? 'badge-warning' : 'badge-secondary'), p.estado));
            tr.appendChild(tdEstado);

            tr.appendChild(el('td', null, p.cliente));
            tr.appendChild(el('td', 'text-nowrap font-weight-bold', p.total));

            var tdAcciones = el('td', 'text-nowrap text-right');

            var btnItems = el('button', 'btn btn-sm btn-outline-secondary mr-1', 'Ver ítems (' + p.cantLineas + ')');
            btnItems.type = 'button';
            tdAcciones.appendChild(btnItems);

            var btnCargar = el('button', 'btn btn-sm btn-success mr-1', 'Cargar al carrito');
            btnCargar.type = 'button';
            btnCargar.disabled = !p.puedeCargar;
            if (!p.puedeCargar) btnCargar.title = 'Está en uso en otra terminal';
            btnCargar.addEventListener('click', function () { cargar(p, null); });
            tdAcciones.appendChild(btnCargar);

            var btnDescartar = el('button', 'btn btn-sm btn-outline-danger', 'Descartar');
            btnDescartar.type = 'button';
            btnDescartar.disabled = !p.puedeCargar;
            btnDescartar.addEventListener('click', function () { descartar(p, null); });
            tdAcciones.appendChild(btnDescartar);

            tr.appendChild(tdAcciones);
            tbody.appendChild(tr);

            // Fila de detalle con los items (oculta hasta apretar "Ver ítems").
            var trDetalle = el('tr', 'd-none bg-light');
            var tdDetalle = el('td');
            tdDetalle.colSpan = 7;
            var tablaItems = el('table', 'table table-sm table-bordered mb-0 bg-white');
            var theadItems = el('thead');
            var trhItems = el('tr');
            ['Código', 'Producto', 'Cantidad', 'Precio', 'Importe'].forEach(function (t) { trhItems.appendChild(el('th', null, t)); });
            theadItems.appendChild(trhItems);
            tablaItems.appendChild(theadItems);
            var tbodyItems = el('tbody');
            (p.items || []).forEach(function (it) {
                var trIt = el('tr', it.anulado ? 'text-muted' : '');
                trIt.appendChild(el('td', null, it.codigo));
                trIt.appendChild(el('td', null, it.producto + (it.anulado ? ' (anulado)' : '')));
                trIt.appendChild(el('td', 'text-right', it.cantidad));
                trIt.appendChild(el('td', 'text-right', it.precio));
                trIt.appendChild(el('td', 'text-right', it.importe));
                tbodyItems.appendChild(trIt);
            });
            tablaItems.appendChild(tbodyItems);
            tdDetalle.appendChild(tablaItems);
            trDetalle.appendChild(tdDetalle);
            tbody.appendChild(trDetalle);

            btnItems.addEventListener('click', function () { trDetalle.classList.toggle('d-none'); });
        });
        tabla.appendChild(tbody);

        var envoltorio = el('div', 'table-responsive');
        envoltorio.appendChild(tabla);
        cont.appendChild(envoltorio);
    }

    function cargarLista() {
        var cont = document.getElementById('contenedorVentasSinCerrarPOS');
        if (cont) cont.appendChild(el('div', 'p-4 text-center text-muted', 'Cargando...'));

        return post(cfg.urls.listar, {
            clientIdLocal: estado.clientId,
            posInstanceId: posInstanceId(),
            idSucursalPOS: idSucursalPOS()
        }).then(function (resp) {
            if (!resp.ok) {
                if (cont) {
                    cont.textContent = '';
                    cont.appendChild(el('div', 'p-4 text-center text-danger', 'No se pudo consultar las ventas sin guardar.'));
                }
                return resp;
            }
            pintarLista(resp);
            pintarCampanaPropia(resp);
            return resp;
        });
    }

    function abrir() {
        if (!habilitado) return;
        var cont = document.getElementById('contenedorVentasSinCerrarPOS');
        if (cont) cont.textContent = '';
        $('#modalVentasSinCerrarPOS').modal('show');
        cargarLista();
    }

    function cargar(item, supervisor) {
        if (hayCarritoCargado()) {
            Swal.fire({
                icon: 'warning',
                title: 'Hay una venta en pantalla',
                text: 'Terminá o cancelá la venta actual antes de cargar otra.'
            });
            return;
        }

        post(cfg.urls.recuperar, {
            id: item.id,
            posInstanceId: posInstanceId(),
            idSucursalPOS: idSucursalPOS(),
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

            window.POSDraft.restaurarDesdeSnapshot(resp.payload, resp.clientId);
            $('#modalVentasSinCerrarPOS').modal('hide');
            actualizarBadge(0);
            Swal.fire({
                icon: 'info',
                title: 'Venta cargada al carrito',
                text: 'Revisá los precios antes de finalizar: son los de cuando se armó la venta.',
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
                title: 'Descartar venta sin guardar',
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

            post(cfg.urls.descartar, {
                id: item.id,
                motivo: item.motivo,
                posInstanceId: posInstanceId(),
                idSucursalPOS: idSucursalPOS(),
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

    // Al abrir el POS: cuenta las ventas interrumpidas para el badge y, si el carrito local ya se habia
    // convertido en una venta real (ej. se corto la luz justo despues de confirmar), lo limpia para no
    // reenviarla.
    function actualizarBadgeInicial() {
        if (!habilitado) return;

        post(cfg.urls.listar, {
            clientIdLocal: estado.clientId,
            posInstanceId: posInstanceId(),
            idSucursalPOS: idSucursalPOS()
        }).then(function (resp) {
            if (resp.deshabilitado) { deshabilitar(); return; }
            if (!resp.ok) return;

            actualizarBadge(resp.interrumpidas || 0);
            pintarCampanaPropia(resp);

            if (resp.ventaLocalFinalizada) {
                window.desactivarAvisoSalidaPOS?.();
                window.POSDraft?.clear?.();
                Swal.fire({
                    icon: 'info',
                    title: 'Esa venta ya estaba registrada',
                    text: 'La venta que había en pantalla ya se había finalizado (venta #' + resp.ventaLocalFinalizada + '). Se limpió el carrito.',
                    allowOutsideClick: false
                }).then(function () {
                    window.PosNavegandoInternoPOS = true;
                    window.location.reload();
                });
            }
        });
    }

    // Pantalla de apertura de caja: avisa si el usuario tiene ventas sin guardar para que sepa que las va a
    // poder recuperar (F1 -> Ver ventas sin guardar) al abrir la caja.
    function avisarSinCaja() {
        if (!cfg.habilitado || !cfg.urls) return;

        post(cfg.urls.listar, { clientIdLocal: null, posInstanceId: posInstanceId(), idSucursalPOS: idSucursalPOS() })
            .then(function (resp) {
                if (!resp.ok) return;
                var propias = (resp.pendientes || []).filter(function (p) { return p.esMia; });
                if (!propias.length) return;

                var aviso = el('div', 'alert alert-warning small');
                aviso.textContent = 'Tenés ' + propias.length + (propias.length === 1 ? ' venta sin guardar' : ' ventas sin guardar')
                    + '. Abrí la caja y recuperala desde Ayuda (F1) → "Ver ventas sin guardar".';
                var cuerpo = document.querySelector('#modalAbrirCaja .modal-body');
                if (cuerpo) cuerpo.insertBefore(aviso, cuerpo.firstChild);
            });
    }

    // ------------------------------------------------------------------------------------------
    // Inicio
    // ------------------------------------------------------------------------------------------

    function init() {
        habilitado = cfg.habilitado === true && !!cfg.urls
            && !window.esEdicionVenta && !(window.POSModo && window.POSModo.soloFormaPago);
        if (!habilitado) return;

        $('#colVentasSinCerrarAyuda').removeClass('d-none');
        $('#campanaVentasPOSVerTodas').on('click', function (e) {
            e.preventDefault();
            abrir();
        });
        clearInterval(estado.timerLatido);
        estado.timerLatido = setInterval(latido, (cfg.latidoSegundos || 20) * 1000);
        window.addEventListener('pagehide', alCerrarPestana);
    }

    window.POSBorrador = {
        init: init,
        estaHabilitado: function () { return habilitado; },
        getClientId: getClientId,
        adoptarClientId: adoptarClientId,
        reiniciar: reiniciar,
        programarGuardado: programarGuardado,
        abrir: abrir,
        actualizarBadgeInicial: actualizarBadgeInicial,
        avisarSinCaja: avisarSinCaja,
        // Compartido con pos-producto-pendiente.js: POST JSON con antiforgery.
        post: post
    };
})(window, window.jQuery);
