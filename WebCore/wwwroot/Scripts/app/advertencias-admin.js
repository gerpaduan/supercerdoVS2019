// Campana de notificaciones del admin + detalle/revision de advertencias (ventas en curso interrumpidas y
// "productos pesados sin agregar"). Solo se carga para el admin y solo con Postgres (ver _Layout.cshtml y
// docs/DECISIONS.md "Ventas en curso: borrador en servidor y advertencias del POS").
//
// Depende de window.AdvertenciasAdminConfig (URLs) y de Bootstrap 5 (dropdown/modal). Usa delegacion de
// eventos porque el detalle se inserta con innerHTML (los scripts de un parcial cargado asi no corren).
// Todo texto que viene del servidor se pinta con textContent (nunca innerHTML) para no abrir XSS.
(function (window, document) {
    'use strict';

    var cfg = window.AdvertenciasAdminConfig;
    if (!cfg) return;

    function token() {
        var input = document.querySelector('#advertenciasAntiForgery input[name="__RequestVerificationToken"]')
            || document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function postJson(url, body) {
        return fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token() },
            body: JSON.stringify(body || {})
        }).then(function (r) { return r.json(); });
    }

    // "Atender" recibe el id como formulario (int id), no como JSON.
    function postFormulario(url, campos) {
        var datos = new URLSearchParams();
        Object.keys(campos).forEach(function (k) { datos.append(k, String(campos[k])); });
        datos.append('__RequestVerificationToken', token());
        return fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: datos.toString()
        }).then(function (r) { return r.json(); });
    }

    // ------------------------------------------------------------------------------------------
    // Campana
    // ------------------------------------------------------------------------------------------

    function pintarCampana(data) {
        var contador = document.getElementById('campanaContador');
        var lista = document.getElementById('campanaLista');
        if (!contador || !lista) return;

        var pendientes = (data && data.pendientes) || 0;
        contador.textContent = pendientes > 99 ? '99+' : String(pendientes);
        contador.classList.toggle('d-none', pendientes === 0);

        lista.textContent = '';
        var items = (data && data.items) || [];
        if (!items.length) {
            var vacio = document.createElement('div');
            vacio.className = 'px-3 py-2 text-muted small';
            vacio.textContent = 'No hay notificaciones pendientes.';
            lista.appendChild(vacio);
            return;
        }

        items.forEach(function (n) {
            var a = document.createElement('a');
            a.href = '#';
            a.className = 'dropdown-item js-abrir-detalle-advertencia';
            a.style.whiteSpace = 'normal';
            a.setAttribute('data-url', n.detalleUrl || '');

            var titulo = document.createElement('div');
            titulo.className = 'fw-bold small';
            var icono = document.createElement('i');
            icono.className = 'fas fa-triangle-exclamation mr-1 ' + (n.severidad === 'ADVERTENCIA' ? 'text-warning' : 'text-info');
            titulo.appendChild(icono);
            titulo.appendChild(document.createTextNode(n.titulo || ''));

            var mensaje = document.createElement('div');
            mensaje.className = 'small';
            mensaje.textContent = n.mensaje || '';

            var fecha = document.createElement('div');
            fecha.className = 'text-muted';
            fecha.style.fontSize = '.72rem';
            fecha.textContent = n.fecha || '';

            a.appendChild(titulo);
            a.appendChild(mensaje);
            a.appendChild(fecha);
            lista.appendChild(a);
        });
    }

    function actualizarCampana() {
        if (!cfg.resumenUrl) return Promise.resolve();
        return fetch(cfg.resumenUrl, { credentials: 'same-origin', cache: 'no-store' })
            .then(function (r) { return r.ok ? r.json() : null; })
            .then(function (data) { if (data && data.ok) pintarCampana(data); })
            .catch(function () { /* la campana es informativa: si falla la consulta se reintenta en el proximo ciclo */ });
    }

    // ------------------------------------------------------------------------------------------
    // Detalle en modal
    // ------------------------------------------------------------------------------------------

    function abrirDetalle(url) {
        var modalEl = document.getElementById('modalAdvertenciaDetalle');
        var body = document.getElementById('modalAdvertenciaDetalleBody');
        if (!modalEl || !body || !url) return;

        body.textContent = 'Cargando...';
        var modal = window.bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.show();

        fetch(url, { credentials: 'same-origin', cache: 'no-store' })
            .then(function (r) {
                if (!r.ok) throw new Error('HTTP ' + r.status);
                return r.text();
            })
            .then(function (html) { body.innerHTML = html; })
            .catch(function () { body.textContent = 'No se pudo cargar el detalle.'; });
    }
    window.abrirDetalleAdvertencia = abrirDetalle;

    function cerrarDetalle() {
        var modalEl = document.getElementById('modalAdvertenciaDetalle');
        if (!modalEl) return;
        var modal = window.bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
    }

    // ------------------------------------------------------------------------------------------
    // Eventos
    // ------------------------------------------------------------------------------------------

    document.addEventListener('click', function (e) {
        var abrir = e.target.closest('.js-abrir-detalle-advertencia');
        if (abrir) {
            e.preventDefault();
            abrirDetalle(abrir.getAttribute('data-url'));
            return;
        }

        var revisar = e.target.closest('.js-revisar-producto');
        if (revisar) {
            e.preventDefault();
            var fila = revisar.closest('tr[data-advertencia-id]');
            if (!fila) return;

            var valor = fila.querySelector('.js-revision-valor');
            var comentario = fila.querySelector('.js-revision-comentario');
            revisar.disabled = true;

            postJson(cfg.revisarUrl, {
                id: parseInt(fila.getAttribute('data-advertencia-id'), 10),
                revision: valor ? valor.value : '',
                comentario: comentario ? comentario.value : ''
            }).then(function (resp) {
                revisar.disabled = false;
                var estado = fila.querySelector('.js-revision-estado');
                if (!estado) return;
                estado.textContent = '';

                if (!resp || !resp.ok) {
                    var error = document.createElement('span');
                    error.className = 'text-danger';
                    error.textContent = (resp && resp.msg) || 'No se pudo guardar.';
                    estado.appendChild(error);
                    return;
                }

                var badge = document.createElement('span');
                var sospechosa = valor && valor.value === 'SOSPECHOSA';
                badge.className = 'badge ' + (sospechosa ? 'bg-danger' : 'bg-success');
                badge.textContent = sospechosa ? 'Sospechosa' : 'Justificada';
                estado.appendChild(badge);
                estado.appendChild(document.createTextNode(' guardada por ' + (resp.revisor || '')));
            }).catch(function () {
                revisar.disabled = false;
            });
            return;
        }

        var atender = e.target.closest('.js-atender-notificacion');
        if (atender) {
            e.preventDefault();
            atender.disabled = true;
            postFormulario(cfg.atenderUrl, { id: parseInt(atender.getAttribute('data-id'), 10) })
                .then(function () {
                    cerrarDetalle();
                    actualizarCampana();
                })
                .catch(function () { atender.disabled = false; });
        }
    });

    // ------------------------------------------------------------------------------------------
    // Polling (solo con la pestana visible)
    // ------------------------------------------------------------------------------------------

    document.addEventListener('visibilitychange', function () {
        if (document.visibilityState === 'visible') actualizarCampana();
    });

    setInterval(function () {
        if (document.visibilityState === 'visible') actualizarCampana();
    }, cfg.intervaloMs || 60000);

    actualizarCampana();
})(window, document);
