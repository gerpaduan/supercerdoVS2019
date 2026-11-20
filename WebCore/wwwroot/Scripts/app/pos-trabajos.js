// Cargas desde el POS (2026-11-18, docs/DECISIONS.md "Cargas desde el POS"): el cajero abre desde el boton "Cargas" de la barra una
// carga de Stock (ingreso, egreso, pesaje, ajuste), Movimientos o Elaborados (nuevo, ingreso rapido) SIN salir de su caja.
//
// Cada carga es la pantalla real, tal cual, dentro de un <iframe> de #modalTrabajoPOS (su balanza, atajos, borrador y modales
// quedan en su propio contexto: no pisan los del POS). El modal NO se cierra por accidente (fondo estatico, sin X): se MINIMIZA
// (el iframe sigue vivo con lo cargado y la barra muestra "<carga> en curso", que la trae de vuelta) o se DESCARTA con
// confirmacion. Una carga NUEVA abierta de cada tipo a la vez. Mismo patron que POSCompras (Ventas/POS.cshtml, "Compra en curso").
//
// Tambien arma la lista "Cargado desde que abriste la caja" del menu (para no cargar dos veces lo mismo): el ojo de cada fila abre
// ESE registro en la misma ventana, en solo lectura y con el boton Modificar de su pantalla (2026-11-19, regla "propio de la caja"
// en el servidor). Un registro abierto es un slot mas, con clave "<tipo>:<id>" (puede convivir con una carga nueva del mismo tipo).
// Al abrir un tipo NUEVO, si ya hay una carga igual en esa lista, avisa con una franja amarilla.
//
// Depende de: jQuery, Bootstrap 5 (modal y dropdown), SweetAlert2, POSGuard, PosAutorizacion (opcional, para pedir la autorizacion
// de otro usuario) y POSBloqueo (opcional, para que el POS no se bloquee mientras se carga dentro del iframe).
// Config en window.POSTrabajosConfig (la arma Ventas/POS.cshtml): urlAbrir, urlCargas, posInstanceId, tipos {tipo: {titulo}}.
// Las pantallas dentro del iframe avisan que guardaron con postMessage (ventana-trabajo.js, evento 'guardado').
(function (window, $) {
    'use strict';

    var cfg = window.POSTrabajosConfig;
    if (!cfg || !$) return;

    var MENSAJE_TIPO = 'carnisys-trabajo-pos';
    var ACTIVIDAD = ['keydown', 'mousedown', 'mousemove', 'touchstart', 'wheel'];

    // Icono de cada tipo en la lista (el color sale de .pos-carga-icono--<tipo> en pos.css). "cierre" y "desarme" figuran en la lista
    // pero no en "Nueva carga".
    var ICONOS = {
        ingreso: 'fa-arrow-down', egreso: 'fa-arrow-up', pesaje: 'fa-balance-scale', ajuste: 'fa-sliders-h',
        movimiento: 'fa-truck', elaborado: 'fa-utensils', rapido: 'fa-bolt', desarme: 'fa-cubes', cierre: 'fa-lock'
    };

    var slots = {};          // clave ("tipo" o "tipo:id") -> { clave, tipo, id, titulo, accion, iframe }: cargas abiertas (en curso)
    var activo = null;       // clave que se esta viendo en el modal (null = minimizado o cerrado)
    var abriendo = false;    // hay un GET de AbrirCargaPOS en vuelo
    var ultimaLista = [];    // items de "Cargado desde que abriste la caja" (para el aviso de posible duplicado)

    function $modal() { return $('#modalTrabajoPOS'); }

    function tituloDeTipo(tipo) {
        var info = cfg.tipos && cfg.tipos[tipo];
        return info && info.titulo ? info.titulo : tipo;
    }

    // Titulo de un slot: el de la respuesta del servidor ("Movimiento #1008777" en un registro) o el del tipo.
    function tituloDe(clave) {
        var slot = slots[clave];
        return slot && slot.titulo ? slot.titulo : tituloDeTipo(clave);
    }

    function claveDe(tipo, id) { return id ? tipo + ':' + id : tipo; }

    function hayEnCurso() { return Object.keys(slots).length > 0; }

    // ---- Menu "Cargas" y botones "en curso" ------------------------------------------------------
    // Cada carga abierta tiene su boton amarillo en la barra, salvo la que se esta viendo en el modal.
    function actualizarBarra() {
        var visible = $modal().hasClass('show');
        var $zona = $('#posCargasEnCurso').empty();

        Object.keys(slots).forEach(function (clave) {
            if (visible && activo === clave) return;
            $('<button type="button" class="btn btn-warning btn-sm fw-bold pos-compra-en-curso js-pos-trabajo-en-curso"></button>')
                .attr({ 'data-clave': clave, title: 'Hay una carga abierta: tocá para volver a ella' })
                .append('<i class="fas fa-play me-1"></i>')
                .append(document.createTextNode(tituloDe(clave) + ' en curso'))
                .appendTo($zona);
        });

        // Marca "en curso" en el menu de cargas (solo las cargas nuevas: un registro abierto no es "una carga nueva en curso").
        $('#posCargasTipos .js-pos-carga').each(function () {
            $(this).find('.js-pos-carga-en-curso').toggleClass('d-none', !slots[$(this).data('tipo')]);
        });
    }

    // ---- Aviso de posible duplicado -------------------------------------------------------------
    // Al ver una carga NUEVA cuyo tipo ya tiene un registro en la lista de esta caja CON LA MISMA CANTIDAD (los mismos kilos): franja amarilla
    // arriba. No bloquea. (2026-11-19: antes avisaba con cualquier carga previa del mismo tipo y confundia; ahora solo si coincide la cantidad.)
    // Como lo que se carga cambia mientras se escribe, se vuelve a comprobar cada segundo mientras esa carga esta a la vista.
    var VIGILANCIA_MS = 1000;
    var TOLERANCIA_KG = 0.0005;
    var temporizadorVigilancia = null;

    // Numero de un texto de pantalla: "14.000" (punto decimal) o "1.234,500" (formato es-AR).
    function numeroDeTexto(texto) {
        var t = String(texto == null ? '' : texto).replace(/\s/g, '');
        if (!t) return 0;
        var n = t.indexOf(',') >= 0 ? parseFloat(t.replace(/\./g, '').replace(',', '.')) : parseFloat(t);
        return isNaN(n) ? 0 : n;
    }

    // Suma del valor absoluto de los inputs hidden Lineas[i].<campo> de la pantalla del iframe (igual que el servidor: SUM(ABS(kg))).
    function sumarKilosOcultos(doc, campo) {
        var total = 0;
        Array.prototype.forEach.call(doc.querySelectorAll('input[type="hidden"][name$=".' + campo + '"]'), function (input) {
            total += Math.abs(numeroDeTexto(input.value));
        });
        return total;
    }

    // Kilos de lo que se esta cargando ahora en la carga `slot` (misma medida que "kg" de la lista del servidor), o null si no se puede leer.
    // Lee la pantalla del iframe (mismo origen): hidden de lineas en Stock y Movimientos, y el total en pantalla en los elaborados.
    function kilosActuales(slot) {
        var doc;
        try { doc = slot && slot.iframe && slot.iframe.contentDocument; } catch (e) { return null; }
        if (!doc) return null;
        try {
            switch (slot.tipo) {
                case 'ingreso': case 'egreso': case 'pesaje': case 'ajuste':
                    return sumarKilosOcultos(doc, 'CantKgs');
                case 'movimiento':
                    return sumarKilosOcultos(doc, 'CantKg');
                case 'rapido': case 'desarme': {
                    var rapido = doc.getElementById('lblTotalFormulaRapida');
                    return rapido ? Math.abs(numeroDeTexto(rapido.textContent)) : null;
                }
                case 'elaborado': {
                    var total = doc.getElementById('lblTotalProducir');
                    return total ? Math.abs(numeroDeTexto(total.textContent)) : null;
                }
                default:
                    return null;
            }
        } catch (e) {
            return null;
        }
    }

    function detenerVigilancia() {
        if (temporizadorVigilancia) { window.clearInterval(temporizadorVigilancia); temporizadorVigilancia = null; }
    }

    function actualizarAvisoDuplicado() {
        var $aviso = $('#trabajoPOSAviso');
        var slot = activo ? slots[activo] : null;
        var nueva = !!slot && !slot.id;

        // Solo las cargas del mismo tipo con kilos (las de 0 kg no se pueden comparar).
        var candidatas = nueva ? ultimaLista.filter(function (i) { return i.tipo === slot.tipo && Number(i.kg) > TOLERANCIA_KG; }) : [];
        if (!candidatas.length) {
            detenerVigilancia();
            $aviso.addClass('d-none').text('');
            return;
        }

        // Hay con que comparar: se sigue mirando la pantalla mientras esta carga este a la vista.
        if (!temporizadorVigilancia) temporizadorVigilancia = window.setInterval(actualizarAvisoDuplicado, VIGILANCIA_MS);

        var kg = kilosActuales(slot);
        var previa = kg !== null && kg > TOLERANCIA_KG
            ? candidatas.filter(function (i) { return Math.abs(Number(i.kg) - kg) < TOLERANCIA_KG; })[0]
            : null;
        if (!previa) { $aviso.addClass('d-none').text(''); return; }

        var texto = 'Ya cargaste un ' + tituloDeTipo(slot.tipo).toLowerCase() + ' con la misma cantidad a las ' + previa.hora
            + (previa.resumen ? ' (' + previa.resumen + ')' : '') + '. Revisá que no sea el mismo.';
        $aviso.removeClass('d-none').text(texto);
    }

    // ---- Modal ----------------------------------------------------------------------------------
    function mostrar(clave) {
        var slot = slots[clave];
        if (!slot) return;

        activo = clave;
        $('#contenedorTrabajoPOS .pos-trabajo-frame').each(function () {
            $(this).toggleClass('d-none', this !== slot.iframe);
        });
        $('#modalTrabajoPOSTitle').text(slot.titulo || tituloDeTipo(slot.tipo));
        actualizarAvisoDuplicado();

        var $m = $modal();
        if (!$m.hasClass('show')) {
            if (window.POSGuard) window.POSGuard.requestModalOpen('trabajo', function () { $m.modal('show'); });
            else $m.modal('show');
        }
        actualizarBarra();
    }

    function minimizar() {
        $modal().modal('hide');
    }

    // Quita una carga: el iframe se destruye (el borrador del servidor queda "interrumpido" por falta de latido, igual que al
    // descartar una compra) y, si ninguna otra carga abierta usa la misma autorizacion, se avisa al servidor que termino.
    function cerrarCarga(clave) {
        var slot = slots[clave];
        if (!slot) return;

        delete slots[clave];
        if (slot.iframe && slot.iframe.parentNode) slot.iframe.parentNode.removeChild(slot.iframe);

        var otraUsaLaAccion = Object.keys(slots).some(function (c) { return slots[c].accion === slot.accion; });
        if (slot.accion && !otraUsaLaAccion && window.PosAutorizacion) window.PosAutorizacion.revocar(slot.accion);

        if (activo === clave) {
            activo = null;
            $modal().modal('hide');
        }
        actualizarBarra();
    }

    function descartar() {
        if (!activo) { minimizar(); return; }

        var clave = activo;
        var esRegistro = !!(slots[clave] && slots[clave].id);
        Swal.fire({
            icon: 'warning',
            title: '¿Descartar «' + tituloDe(clave) + '» en curso?',
            text: esRegistro ? 'Se pierden los cambios que no guardaste. El registro no se borra.' : 'Se pierde lo que cargaste y no se guarda nada.',
            showCancelButton: true,
            confirmButtonText: 'Descartar',
            cancelButtonText: 'Seguir con la carga',
            focusCancel: true
        }).then(function (r) {
            if (r.isConfirmed) cerrarCarga(clave);
        });
    }

    // La pantalla del iframe termino de guardar: se cierra la carga, se avisa con un toast y se refresca la lista.
    function alGuardar(clave) {
        var slot = slots[clave];
        var titulo = slot ? tituloDeTipo(slot.tipo) : tituloDe(clave);
        cerrarCarga(clave);
        if (window.Swal) {
            Swal.fire({ toast: true, position: 'top-end', icon: 'success', title: titulo + ' guardado', showConfirmButton: false, timer: 2800 });
        }
        cargarLista();
    }

    // ---- Abrir una carga ------------------------------------------------------------------------
    function crearIframe(clave, resp) {
        var iframe = document.createElement('iframe');
        iframe.className = 'pos-trabajo-frame d-none';
        // El nombre empieza con "carnisys-trabajo-": _Layout.cshtml lo toma como "modo ventana" (sin menu lateral ni barra) y se
        // conserva en toda la navegacion dentro del iframe. Distinto por slot (tambien la marca de guardado de Stock lo usa).
        iframe.name = 'carnisys-trabajo-pos-' + clave.replace(':', '-');
        iframe.title = resp.titulo || tituloDe(clave);
        iframe.setAttribute('allow', 'camera');
        iframe.addEventListener('load', function () { enlazarActividad(iframe); });
        iframe.src = resp.url;
        document.getElementById('contenedorTrabajoPOS').appendChild(iframe);
        return iframe;
    }

    // Abre (o vuelve a) un slot: una carga nueva (id vacio) o un registro ya cargado (id > 0). La clave evita duplicados.
    function abrirSlot(tipo, id) {
        var clave = claveDe(tipo, id);

        // Ya esta abierta: se vuelve a ella (no se abre otra).
        if (slots[clave]) { mostrar(clave); return; }
        if (abriendo) return;
        if (window.POSGuard && !window.POSGuard.startAction('trabajo:abrir')) return;

        abriendo = true;
        window.POSModalLoading && window.POSModalLoading.show(id ? 'Abriendo el registro...' : 'Abriendo ' + tituloDeTipo(tipo).toLowerCase() + '...');

        var datos = { tipo: tipo, posInstanceId: cfg.posInstanceId };
        if (id) datos.id = id;

        $.ajax({ url: cfg.urlAbrir, type: 'GET', cache: false, data: datos })
            .done(function (resp) {
                if (!resp || !resp.ok || !resp.url) { avisarError(tipo, 'No se pudo abrir la carga.'); return; }
                slots[clave] = { clave: clave, tipo: tipo, id: id || 0, titulo: resp.titulo || '', accion: resp.accion || '', iframe: crearIframe(clave, resp) };
                mostrar(clave);
            })
            .fail(function (xhr) { manejarError(xhr, tipo, id); })
            .always(function () {
                abriendo = false;
                window.POSModalLoading && window.POSModalLoading.hide();
                if (window.POSGuard) window.POSGuard.endAction('trabajo:abrir');
            });
    }

    function abrir(tipo) {
        if (!cfg.tipos || !cfg.tipos[tipo]) return;
        abrirSlot(tipo, 0);
    }

    // El ojo de la lista: abre ese registro para verlo y, con su boton Modificar, cambiarlo.
    function abrirRegistro(tipo, id) {
        id = parseInt(id, 10) || 0;
        if (!tipo || id <= 0) return;
        abrirSlot(tipo, id);
    }

    function avisarError(tipo, texto) {
        Swal.fire({ icon: 'warning', title: tituloDeTipo(tipo), text: texto });
    }

    // 403 con el encabezado de autorizacion: el operador no tiene el permiso pero otro usuario que si lo tiene puede autorizar
    // (usuario y clave). Si autoriza, se reintenta. Cualquier otro error se muestra tal cual.
    function manejarError(xhr, tipo, id) {
        var detalle = xhr && xhr.status === 403 && typeof xhr.responseText === 'string' ? xhr.responseText.trim() : '';
        if (!(detalle && detalle.length <= 200 && detalle.indexOf('<') === -1)) detalle = '';

        var autorizable = window.PosAutorizacion && window.PosAutorizacion.leerEncabezado(xhr);
        if (autorizable && detalle) {
            window.PosAutorizacion.pedir({ accion: autorizable.accion, idRegistro: autorizable.idRegistro, motivo: detalle })
                .then(function (ok) { if (ok) abrirSlot(tipo, id || 0); });
            return;
        }

        avisarError(tipo, detalle || 'No se pudo abrir la carga. Revisá la conexión e intentá de nuevo.');
    }

    // ---- Actividad dentro del iframe -------------------------------------------------------------
    // El teclado y el mouse del iframe no llegan a los listeners del POS: sin esto el bloqueo por inactividad se dispararia con el
    // cajero cargando. Se vuelve a enlazar en cada pagina nueva del iframe (guardar y volver al listado, por ejemplo).
    function enlazarActividad(iframe) {
        var doc;
        try { doc = iframe.contentDocument; } catch (e) { return; /* otro origen: no se puede (no deberia pasar) */ }
        if (!doc) return;

        function registrar() {
            if (window.POSBloqueo && typeof window.POSBloqueo.registrarActividad === 'function') window.POSBloqueo.registrarActividad();
        }
        ACTIVIDAD.forEach(function (evento) { doc.addEventListener(evento, registrar, { passive: true, capture: true }); });
    }

    // La pantalla del iframe avisa que guardo. Se identifica por la ventana de origen (no por el texto del mensaje): otra pagina no
    // puede hacerse pasar por una carga.
    window.addEventListener('message', function (e) {
        if (e.origin !== window.location.origin) return;
        var datos = e.data;
        if (!datos || datos.tipo !== MENSAJE_TIPO || datos.evento !== 'guardado') return;

        var clave = Object.keys(slots).filter(function (c) {
            return slots[c].iframe && slots[c].iframe.contentWindow === e.source;
        })[0];
        if (clave) alGuardar(clave);
    });

    // ---- "Cargado desde que abriste la caja" ----------------------------------------------------
    function escapar(texto) {
        return $('<div>').text(texto == null ? '' : String(texto)).html();
    }

    function pintarLista(resp) {
        var items = (resp && resp.items) || [];
        ultimaLista = items;
        $('#posCargasTitulo').text((resp && resp.titulo) || 'Cargado desde que abriste la caja');
        $('#posCargasDesde').text((resp && resp.desde) || '');

        var $lista = $('#posCargasLista');
        if (!items.length) {
            $lista.html('<div class="px-3 py-2 small text-muted">Todavía no cargaste nada' + (resp && resp.titulo === 'Cargado hoy' ? ' hoy' : ' en esta caja') + '.</div>');
        } else {
            $lista.html(items.map(function (i) {
                var tipo = escapar(i.tipo);
                return '<div class="pos-carga-fila d-flex align-items-center px-3 py-1 js-pos-carga-ver" role="button" tabindex="0" title="Ver o modificar"'
                    + ' data-tipo="' + tipo + '" data-id="' + escapar(i.id) + '">'
                    + '<i class="fas ' + (ICONOS[i.tipo] || 'fa-box') + ' pos-carga-icono pos-carga-icono--' + tipo + ' me-2"></i>'
                    + '<span class="text-muted small pos-carga-hora">' + escapar(i.hora) + '</span>'
                    + '<span class="flex-grow-1 mx-2" style="min-width:0;"><span class="fw-bold">' + escapar(i.titulo) + '</span>'
                    + '<br><span class="text-muted small">' + escapar(i.resumen) + '</span></span>'
                    + '<i class="fas fa-eye text-secondary" aria-label="Ver o modificar"></i>'
                    + '</div>';
            }).join(''));
        }
        actualizarAvisoDuplicado();
    }

    function cargarLista() {
        if (!cfg.urlCargas) return;
        $.ajax({ url: cfg.urlCargas, type: 'GET', cache: false, data: { posInstanceId: cfg.posInstanceId } })
            .done(pintarLista)
            .fail(function () {
                $('#posCargasLista').html('<div class="px-3 py-2 small text-muted">No se pudo cargar la lista. Intentá de nuevo.</div>');
            });
    }

    // ---- Eventos --------------------------------------------------------------------------------
    $(document)
        .on('click.posTrabajos', '.js-pos-carga', function (e) { e.preventDefault(); abrir($(this).data('tipo')); })
        .on('click.posTrabajos', '.js-pos-trabajo-en-curso', function () { mostrar($(this).attr('data-clave')); })
        .on('click.posTrabajos', '#modalTrabajoPOS .js-trabajo-minimizar', minimizar)
        .on('click.posTrabajos', '#modalTrabajoPOS .js-trabajo-descartar', descartar)
        .on('click.posTrabajos', '.js-pos-carga-ver', function (e) {
            e.preventDefault();
            abrirRegistro($(this).attr('data-tipo'), $(this).attr('data-id'));
        })
        .on('keydown.posTrabajos', '.js-pos-carga-ver', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                abrirRegistro($(this).attr('data-tipo'), $(this).attr('data-id'));
            }
        })
        .on('show.bs.dropdown.posTrabajos', '#posCargasWrap', cargarLista)
        .on('shown.bs.modal.posTrabajos', '#modalTrabajoPOS', function () {
            actualizarBarra();
            // El foco va al iframe para que se pueda escribir enseguida.
            var slot = activo && slots[activo];
            try { if (slot && slot.iframe.contentWindow) slot.iframe.contentWindow.focus(); } catch (e) { /* sin foco: no frena nada */ }
        })
        .on('hidden.bs.modal.posTrabajos', '#modalTrabajoPOS', function () {
            activo = null;
            actualizarBarra();
        });

    // La lista se carga al abrir el POS para que el aviso de duplicado ya la tenga cuando se abra una carga.
    $(cargarLista);

    // ---- Salir del POS con cosas sin guardar -----------------------------------------------------
    // "Inicio" / "Cerrar" (#btnSalirPOS): si hay cargas abiertas o una compra en curso, antes de salir se avisa NOMBRANDOLAS (2026-11-19).
    // Va en la fase de captura de window: corre antes que el interceptor de enlaces de la app instalada (app-instalada.js, captura en
    // document), que si no cerraria la ventana sin preguntar. Al confirmar se marca la salida (POSSalidaConfirmada: pos-cart.js ya no
    // pregunta por modales/compra/cargas con su aviso nativo; la venta en curso conserva el suyo) y se repite el clic.
    function sinGuardar() {
        var nombres = Object.keys(slots).map(tituloDe);
        if (window.POSCompras && typeof window.POSCompras.hayCompraEnCurso === 'function' && window.POSCompras.hayCompraEnCurso()) {
            nombres.push('Compra');
        }
        return nombres;
    }

    window.addEventListener('click', function (e) {
        var boton = e.target && e.target.closest ? e.target.closest('#btnSalirPOS') : null;
        if (!boton || window.POSSalidaConfirmada === true) return;

        var nombres = sinGuardar();
        if (!nombres.length) return;

        e.preventDefault();
        e.stopImmediatePropagation();
        Swal.fire({
            icon: 'warning',
            title: 'Tenés sin guardar',
            text: nombres.join(', ') + '. Si salís se pierden.',
            showCancelButton: true,
            confirmButtonText: 'Salir igual',
            cancelButtonText: 'Seguir en el POS',
            focusCancel: true,
            // Salida segura (swal-single-confirm.js): Enter solo activa el boton con foco (arranca en «Seguir en el POS»), flechas entre los botones.
            customClass: { popup: 'swal-salir-seguro' }
        }).then(function (r) {
            if (!r.isConfirmed) return;
            window.POSSalidaConfirmada = true;
            boton.click();
        });
    }, true);

    window.POSTrabajos = {
        abrir: abrir,
        abrirRegistro: abrirRegistro,
        restaurar: function (clave) { mostrar(clave); },
        minimizar: minimizar,
        descartar: descartar,
        hayEnCurso: hayEnCurso,
        // Nombres de lo que esta abierto (para avisar antes de salir del POS): "Movimiento #1008777", "Ingreso de stock".
        titulosEnCurso: function () { return Object.keys(slots).map(tituloDe); },
        actualizarLista: cargarLista
    };
})(window, window.jQuery);
