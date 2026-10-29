// Eliminar una compra, una operacion de stock o un movimiento (Compras/Editar, Stock/Editar, Movimientos/Editar).
// Flujo: boton -> GET vista previa (resumen, lineas, efectos y bloqueos) -> modal con motivo + usuario y contraseña ->
// POST eliminar. El servidor revalida usuario y contraseña en CADA eliminacion (no hay ventana de "ya me valide") y decide
// el permiso con la persona que puso la clave. Si el efecto toca una caja ya cerrada, pide confirmar (CajaCerradaConfirm) y
// reenvia con confirmarCajaCerrada=true. Config: atributo data-config de <div id="eliminarRegistroConfig"> del parcial
// Views/Shared/_EliminarRegistro.cshtml (se lee al tocar el boton: dentro del modal del POS el script se carga una sola vez). Depende de: jQuery, Bootstrap 4 (modal) y SweetAlert2.
// Ver docs/DECISIONS.md "Eliminar compras, stock y movimientos".
function iniciarEliminarRegistro(window, $) {
    'use strict';

    if (!$) return;

    var MOTIVO_MIN = 10;
    var MOTIVO_MAX = 300;
    var cfg = null;
    var vista = null;
    var enviando = false;
    var timerBloqueo = null;

    // Todo texto que viene del servidor se inserta escapado (no se confia en el contenido).
    function escapar(texto) {
        return $('<div>').text(texto == null ? '' : String(texto)).html();
    }

    function leerConfig() {
        var nodo = document.getElementById('eliminarRegistroConfig');
        if (!nodo) return null;
        try { return JSON.parse(nodo.getAttribute('data-config') || ''); } catch (e) { return null; }
    }

    // Dentro del modal del POS (Bootstrap 5) el modal de eliminar se apila sobre el del POS y el focus trap de este le roba el
    // foco al de arriba (no se puede escribir el motivo ni la clave). Se desactiva mientras esta abierto y se reactiva al cerrar
    // (mismo arreglo que compras.js, gestionarFocusTrapModalAbierto).
    function ajustarFocusTrapSobreModalDelPos($m) {
        if (cfg.origen !== 'pos' || !window.bootstrap || !window.bootstrap.Modal) return;
        var elFondo = document.getElementById('modalFinanzasPOS');
        if (!elFondo) return;
        $m.on('show.bs.modal', function () {
            var fondo = window.bootstrap.Modal.getInstance(elFondo);
            if (fondo && fondo._focustrap && typeof fondo._focustrap.deactivate === 'function') fondo._focustrap.deactivate();
        });
        $m.on('hidden.bs.modal', function () {
            var fondo = window.bootstrap.Modal.getInstance(elFondo);
            if (fondo && fondo._focustrap && typeof fondo._focustrap.activate === 'function') fondo._focustrap.activate();
        });
    }

    function token() {
        return $('input[name="__RequestVerificationToken"]').first().val() || '';
    }

    function armarModal() {
        $('#modalEliminarRegistro').remove();

        var credenciales;
        if (cfg.esCuentaProduccion) {
            var opciones = '<option value="">Elegí tu nombre…</option>' + (cfg.usuarios || []).map(function (u) {
                return '<option value="' + escapar(u.id) + '">' + escapar(u.nombre) + '</option>';
            }).join('');
            credenciales = '<label class="mb-1 er-label" for="erUsuario">Usuario</label><select id="erUsuario" class="form-control form-control-sm">' + opciones + '</select>';
        } else {
            credenciales = '<label class="mb-1 er-label" for="erUsuarioFijo">Usuario</label>'
                + '<input id="erUsuarioFijo" class="form-control form-control-sm" readonly value="' + escapar(cfg.nombreUsuarioSesion) + '">';
        }

        var html = ''
            + '<div class="modal fade" id="modalEliminarRegistro" tabindex="-1" role="dialog" aria-modal="true" data-backdrop="static" data-keyboard="false">'
            + '<style>'
            + '#modalEliminarRegistro .er-label{font-size:.8rem;font-weight:600;color:#6c757d}'
            + '#modalEliminarRegistro .er-resumen{font-size:.85rem}'
            + '#modalEliminarRegistro .er-lineas{max-height:210px;overflow:auto;font-size:.8rem}'
            + '#modalEliminarRegistro .er-lineas table{margin-bottom:0}'
            + '#modalEliminarRegistro .er-lineas th{position:sticky;top:0;background:#f8f9fa}'
            + '#modalEliminarRegistro .er-contador{font-size:.75rem}'
            + '</style>'
            + '<div class="modal-dialog modal-lg modal-dialog-scrollable" role="document"><div class="modal-content">'
            + '<div class="modal-header py-2 bg-danger text-white"><h6 class="modal-title mb-0" id="erTitulo"></h6></div>'
            + '<div class="modal-body py-2">'
            + '<div id="erResumen" class="er-resumen mb-2"></div>'
            + '<div id="erBloqueos" class="alert alert-danger py-2 small d-none mb-2"></div>'
            + '<div id="erEfectos" class="small mb-2"></div>'
            + '<div id="erLineas" class="er-lineas border rounded mb-2"></div>'
            + '<div class="form-group mb-2"><label class="mb-1 er-label" for="erMotivo">Motivo de la eliminación (obligatorio)</label>'
            + '<textarea id="erMotivo" class="form-control form-control-sm" rows="2" maxlength="' + MOTIVO_MAX + '" placeholder="Por qué se elimina (queda registrado)"></textarea>'
            + '<div class="er-contador text-muted text-right"><span id="erContador">0</span>/' + MOTIVO_MAX + ' (mínimo ' + MOTIVO_MIN + ')</div></div>'
            + '<div class="border rounded p-2 bg-light"><div class="small mb-1"><i class="fas fa-lock mr-1"></i>Para eliminar confirmá tu usuario y contraseña.</div>'
            + '<div class="form-row"><div class="col-md-6 mb-1">' + credenciales + '</div>'
            + '<div class="col-md-6 mb-1"><label class="mb-1 er-label" for="erClave">Contraseña</label>'
            + '<input id="erClave" type="password" class="form-control form-control-sm" autocomplete="off" data-lpignore="true"></div></div></div>'
            + '<div id="erError" class="alert alert-danger py-2 small d-none mt-2 mb-0"></div>'
            + '</div>'
            + '<div class="modal-footer py-2">'
            + '<button type="button" class="btn btn-outline-secondary btn-sm" id="erCancelar">Cancelar</button>'
            + '<button type="button" class="btn btn-danger btn-sm" id="erConfirmar" disabled><i class="fas fa-trash-alt mr-1"></i>Eliminar definitivamente</button>'
            + '</div></div></div></div>';

        $('body').append(html);
        var $m = $('#modalEliminarRegistro');
        ajustarFocusTrapSobreModalDelPos($m);
        $m.on('input change keyup', '#erMotivo, #erClave, #erUsuario', actualizarEstado);
        $m.on('click', '#erCancelar', function () { if (!enviando) $m.modal('hide'); });
        $m.on('click', '#erConfirmar', function () { enviar(false); });
        $m.on('keydown', '#erClave', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                if (!$('#erConfirmar').prop('disabled')) enviar(false);
            }
        });
        $m.on('hidden.bs.modal', function () {
            // La clave nunca se conserva: se limpia al cerrar.
            $('#erClave').val('');
            window.clearInterval(timerBloqueo);
            timerBloqueo = null;
            $m.remove();
        });
        return $m;
    }

    function pintarVista($m) {
        $m.find('#erTitulo').text('Eliminar ' + cfg.sustantivo + ' #' + vista.idRegistro);

        var filas = [];
        filas.push(['Tipo', vista.tipo]);
        filas.push(['Fecha', vista.fecha]);
        if (vista.sucursalDestino) {
            filas.push(['Origen → Destino', (vista.sucursal || '') + ' → ' + vista.sucursalDestino]);
        } else {
            filas.push(['Sucursal', vista.sucursal]);
        }
        if (vista.persona) filas.push([vista.modulo === 'COMPRA' ? 'Proveedor' : 'Persona', vista.persona]);
        if (vista.nroRemito) filas.push(['Remito', vista.nroRemito]);
        if (vista.modulo === 'COMPRA') {
            filas.push(['Importe', vista.importeTexto + (vista.enCtaCte ? ' · en cuenta corriente' : '')]);
        }
        filas.push(['Líneas / Kg', vista.cantLineas + ' · ' + vista.totalKgTexto + ' kg']);

        var resumen = '<table class="table table-sm table-borderless mb-0"><tbody>' + filas.map(function (f) {
            return '<tr><td class="text-muted pr-3" style="width:35%">' + escapar(f[0]) + '</td><td><b>' + escapar(f[1]) + '</b></td></tr>';
        }).join('') + '</tbody></table>';
        $m.find('#erResumen').html(resumen);

        if (vista.bloqueos && vista.bloqueos.length) {
            $m.find('#erBloqueos').removeClass('d-none').html('<b>No se puede eliminar:</b><ul class="mb-0 pl-3">'
                + vista.bloqueos.map(function (b) { return '<li>' + escapar(b) + '</li>'; }).join('') + '</ul>');
        }

        var efectos = [];
        if (vista.efectoCtaCte) efectos.push('<li>' + escapar(vista.efectoCtaCte) + '</li>');
        (vista.avisos || []).forEach(function (a) { efectos.push('<li>' + escapar(a) + '</li>'); });
        efectos.push('<li>El registro se borra. Queda constancia de quién lo eliminó, cuándo, el motivo y todo su contenido en <b>Registros eliminados</b>. No se puede deshacer.</li>');
        $m.find('#erEfectos').html('<ul class="mb-0 pl-3">' + efectos.join('') + '</ul>');

        if (vista.lineas && vista.lineas.length) {
            var cuerpo = vista.lineas.map(function (l) {
                return '<tr><td>' + escapar(l.codigo) + '</td><td>' + escapar(l.producto) + (l.detalle ? ' <span class="text-muted">(' + escapar(l.detalle) + ')</span>' : '')
                    + '</td><td class="text-right">' + escapar(l.cantKg) + '</td><td class="text-right">' + escapar(l.cantUnidad)
                    + '</td><td class="text-right">' + escapar(l.precioKg) + '</td></tr>';
            }).join('');
            $m.find('#erLineas').html('<table class="table table-sm table-striped"><thead><tr><th>Cód.</th><th>Producto</th>'
                + '<th class="text-right">Kg</th><th class="text-right">Un.</th><th class="text-right">Precio</th></tr></thead><tbody>' + cuerpo + '</tbody></table>');
        } else {
            $m.find('#erLineas').addClass('d-none');
        }
    }

    function actualizarEstado() {
        var motivo = ($('#erMotivo').val() || '').trim();
        $('#erContador').text(($('#erMotivo').val() || '').length);
        var usuario = cfg.esCuentaProduccion ? $('#erUsuario').val() : cfg.idUsuarioSesion;
        var hayBloqueos = vista && vista.bloqueos && vista.bloqueos.length > 0;
        var listo = !enviando && !hayBloqueos && motivo.length >= MOTIVO_MIN && usuario && ($('#erClave').val() || '').length > 0 && !timerBloqueo;
        $('#erConfirmar').prop('disabled', !listo);
    }

    function mostrarError(texto) {
        $('#erError').removeClass('d-none').text(texto);
    }

    // Cuenta regresiva cuando la sesion quedo bloqueada por intentos fallidos.
    function mostrarBloqueo(segundos) {
        window.clearInterval(timerBloqueo);
        var restante = segundos;
        function pintar() {
            if (restante <= 0) {
                window.clearInterval(timerBloqueo);
                timerBloqueo = null;
                $('#erError').addClass('d-none');
                actualizarEstado();
                return;
            }
            mostrarError('Demasiados intentos. Esperá ' + restante + ' segundos para volver a intentar.');
            restante--;
        }
        timerBloqueo = window.setInterval(pintar, 1000);
        pintar();
        $('#erConfirmar').prop('disabled', true);
    }

    function restaurarBoton() {
        $('#erConfirmar').html('<i class="fas fa-trash-alt mr-1"></i>Eliminar definitivamente');
    }

    function enviar(confirmarCajaCerrada) {
        if (enviando) return;
        enviando = true;
        $('#erError').addClass('d-none');
        $('#erConfirmar').prop('disabled', true).html('<span class="spinner-border spinner-border-sm mr-1"></span>Eliminando…');

        $.ajax({
            url: cfg.urlEliminar,
            type: 'POST',
            data: {
                id: cfg.idRegistro,
                motivo: $('#erMotivo').val(),
                idUsuarioAutoriza: cfg.esCuentaProduccion ? $('#erUsuario').val() : cfg.idUsuarioSesion,
                clave: $('#erClave').val(),
                confirmarCajaCerrada: !!confirmarCajaCerrada,
                origen: cfg.origen || ''
            },
            headers: { RequestVerificationToken: token() }
        }).done(function (resp) {
            enviando = false;
            restaurarBoton();

            if (window.CajaCerradaConfirm && window.CajaCerradaConfirm.manejar(resp, function () { enviar(true); }, { textoConfirmar: 'Eliminar igual' })) {
                return;
            }
            if (resp && resp.bloqueado) {
                $('#erClave').val('');
                mostrarBloqueo(resp.segundosRestantes || 300);
                return;
            }
            if (!resp || !resp.ok) {
                $('#erClave').val('').trigger('focus');
                mostrarError((resp && resp.mensaje) || 'No se pudo eliminar.');
                actualizarEstado();
                return;
            }

            $('#erClave').val('');
            var cierreEnPos = cfg.alEliminar === 'pos-mis-actividades';
            $('#modalEliminarRegistro').modal('hide');
            Swal.fire({
                icon: resp.advertencia ? 'warning' : 'success',
                title: 'Eliminado',
                text: resp.advertencia || '',
                timer: resp.advertencia ? undefined : 1500,
                showConfirmButton: !!resp.advertencia
            }).then(function () {
                // En el POS no se navega (se perderia la venta en curso): se recarga Mis actividades en el mismo modal, donde la
                // compra queda con su egreso "ELIMINADO".
                if (cierreEnPos && window.POSEgresos && typeof window.POSEgresos.abrirMis === 'function') {
                    window.POSEgresos.abrirMis();
                    return;
                }
                window.location.href = resp.redirectUrl || cfg.urlRedireccion;
            });
        }).fail(function (xhr) {
            enviando = false;
            restaurarBoton();
            mostrarError((xhr.responseJSON && xhr.responseJSON.mensaje) || 'Error de conexión. Verificá que el registro no se haya eliminado antes de reintentar.');
            actualizarEstado();
        });
    }

    function abrir() {
        $.getJSON(cfg.urlPrevisualizar, { id: cfg.idRegistro }).done(function (resp) {
            if (!resp || !resp.ok) {
                Swal.fire({ icon: 'error', title: 'No se puede eliminar', text: (resp && resp.mensaje) || 'No se pudo leer el registro.' });
                return;
            }
            vista = resp;
            var $m = armarModal();
            pintarVista($m);
            actualizarEstado();
            $m.modal('show');
            $m.one('shown.bs.modal', function () { $('#erMotivo').trigger('focus'); });
        }).fail(function (xhr) {
            Swal.fire({ icon: 'error', title: 'No se puede eliminar', text: (xhr.responseJSON && xhr.responseJSON.mensaje) || 'Error de conexión.' });
        });
    }

    // Delegado en document: dentro del modal del POS este script se carga una sola vez, pero el boton (y su config) se vuelve a
    // inyectar cada vez que se abre una compra. La config se lee en cada toque.
    $(document).off('click.eliminarRegistro').on('click.eliminarRegistro', '#btnEliminarRegistro', function () {
        if ($(this).prop('disabled')) return;
        cfg = leerConfig();
        if (cfg) abrir();
    });
}

// La pantalla incluye este script en medio de la pagina, antes de que el layout cargue jQuery: se espera a la carga completa.
if (window.jQuery) {
    iniciarEliminarRegistro(window, window.jQuery);
} else {
    window.addEventListener("load", function () { if (window.jQuery) iniciarEliminarRegistro(window, window.jQuery); });
}
