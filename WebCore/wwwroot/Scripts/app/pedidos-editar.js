// Pedidos/Editar: modificar un pedido guardado (cantidades por sucursal, quitar y agregar productos) y guardarlo.
//  - El total y el importe de cada producto se recalculan al escribir: importe = total pedido x precio de la ultima compra guardado
//    (data-precio de la fila; sin precio = no suma y se cuenta aparte). Es una ESTIMACION informativa; el servidor recalcula todo.
//  - Enter baja al input de abajo en la misma sucursal (Shift+Enter sube); al enfocar se selecciona el contenido.
//  - Un usuario de produccion (data-requiere-autorizacion=1) tiene que dar usuario y clave de una persona autorizada al guardar; si el
//    servidor rechaza la clave, se vuelve a pedir.
//  - Control optimista: manda la version con la que se abrio; si otra persona lo modifico, el servidor lo rechaza.
// Depende de: jQuery, SweetAlert2 (Swal). Controller: Pedidos/Modificar. Docs: docs/DECISIONS.md (2026-10-19, Pedidos).
(function ($) {
    'use strict';

    var $pagina = $('.pedido-editar-page');
    if (!$pagina.length) return;

    var idPedido = parseInt($pagina.attr('data-id'), 10);
    var version = parseInt($pagina.attr('data-version'), 10);
    var requiereAutorizacion = $pagina.attr('data-requiere-autorizacion') === '1';
    var guardarUrl = $pagina.attr('data-guardar-url');
    var detalleUrl = $pagina.attr('data-detalle-url');
    var buscarProductoUrl = $pagina.attr('data-buscar-producto-url');

    var $cuerpo = $('#cuerpoPedidoEditar');
    var $total = $('#pedTotalEstimado');
    var $sinPrecio = $('#pedSinPrecio');
    var $btnGuardar = $('#btnGuardarCambios');
    var $buscar = $('#pedBuscarProducto');
    var $resultados = $('#pedResultados');

    var MAX_CANTIDAD = 1000000000;
    var fmtDinero = new Intl.NumberFormat('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    var guardando = false;

    // "12,5" o "12.5" -> 12.5; con coma, los puntos son separador de miles ("1.234,5"); vacio o invalido -> 0 (esa celda no lleva cantidad).
    function leerCantidad(texto) {
        var limpio = String(texto || '').trim();
        if (limpio.indexOf(',') >= 0) limpio = limpio.replace(/\./g, '').replace(',', '.');
        var n = parseFloat(limpio);
        return isFinite(n) && n > 0 && n <= MAX_CANTIDAD ? Math.round(n * 1000) / 1000 : 0;
    }

    function precioDe($fila) {
        var p = parseFloat($fila.attr('data-precio'));
        return isFinite(p) && p > 0 ? p : null;
    }

    function recalcular() {
        var total = 0;
        var sinPrecio = 0;

        $cuerpo.find('.ped-fila').each(function () {
            var $fila = $(this);
            var cantidadFila = 0;
            $fila.find('.ped-cant').each(function () { cantidadFila += leerCantidad($(this).val()); });
            cantidadFila = Math.round(cantidadFila * 1000) / 1000;

            var precio = precioDe($fila);
            $fila.find('.ped-total').text(cantidadFila.toLocaleString('es-AR', { maximumFractionDigits: 3 }));
            $fila.toggleClass('ped-sin-cantidad', cantidadFila <= 0);

            if (cantidadFila > 0 && precio !== null) {
                var importe = Math.round(cantidadFila * precio * 100) / 100;
                total += importe;
                $fila.find('.ped-importe').text('$ ' + fmtDinero.format(importe));
            } else if (cantidadFila > 0) {
                sinPrecio++;
                $fila.find('.ped-importe').html('<span class="text-muted">sin precio</span>');
            } else {
                $fila.find('.ped-importe').html('<span class="text-muted">—</span>');
            }
        });

        $total.text('$ ' + fmtDinero.format(total));
        // Con productos sin precio el total esta incompleto: se rotula "parcial" (textos de Negocio.PedidoTextos, vienen en data- de la etiqueta).
        var $etiqueta = $('#pedTotalEtiqueta');
        $etiqueta.text($etiqueta.attr(sinPrecio > 0 ? 'data-etiqueta-parcial' : 'data-etiqueta'));
        $sinPrecio.toggleClass('d-none', sinPrecio === 0).text(sinPrecio + (sinPrecio === 1 ? ' producto sin precio de compra (no suma)' : ' productos sin precio de compra (no suman)'));
    }

    function lineas() {
        var resultado = [];
        $cuerpo.find('.ped-fila').each(function () {
            var idCorte = parseInt($(this).attr('data-id-corte'), 10);
            $(this).find('.ped-cant').each(function () {
                var cantidad = leerCantidad($(this).val());
                if (cantidad > 0) {
                    resultado.push({ idCorte: idCorte, idSucursal: parseInt($(this).attr('data-id-sucursal'), 10), cantidad: cantidad });
                }
            });
        });
        return resultado;
    }

    function token() {
        return $('input[name="__RequestVerificationToken"]').first().val() || '';
    }

    // ---------- Teclado ----------

    $cuerpo.on('focusin', '.ped-cant', function () { this.select(); });
    $cuerpo.on('input', '.ped-cant', recalcular);
    $cuerpo.on('keydown', '.ped-cant', function (e) {
        if (e.key !== 'Enter') return;
        e.preventDefault();
        var sucursal = $(this).attr('data-id-sucursal');
        var $columna = $cuerpo.find('.ped-cant[data-id-sucursal="' + sucursal + '"]');
        var siguiente = $columna.eq($columna.index(this) + (e.shiftKey ? -1 : 1));
        if (siguiente.length) siguiente.trigger('focus');
    });

    // ---------- Quitar y agregar ----------

    $cuerpo.on('click', '.ped-quitar', function () {
        var $fila = $(this).closest('.ped-fila');
        $fila.find('.ped-cant').val('');
        $fila.hide();
        $fila.addClass('ped-quitada');
        recalcular();
    });

    var timerBusqueda = null;
    $buscar.on('input', function () {
        var q = ($buscar.val() || '').trim();
        window.clearTimeout(timerBusqueda);
        if (q.length < 2) { $resultados.addClass('d-none').empty(); return; }

        timerBusqueda = window.setTimeout(function () {
            $.getJSON(buscarProductoUrl, { q: q }).done(function (lista) {
                $resultados.empty();
                (lista || []).slice(0, 30).forEach(function (p) {
                    var $item = $('<a href="#" class="list-group-item list-group-item-action"></a>');
                    $item.text(p.codigo + ' · ' + p.nombre);
                    $item.data('producto', p);
                    $resultados.append($item);
                });
                if (!$resultados.children().length) $resultados.append('<div class="list-group-item text-muted">Sin resultados</div>');
                $resultados.removeClass('d-none');
            }).fail(function () { $resultados.addClass('d-none').empty(); });
        }, 250);
    });

    $resultados.on('click', 'a', function (e) {
        e.preventDefault();
        agregarProducto($(this).data('producto'));
        $resultados.addClass('d-none').empty();
        $buscar.val('');
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('.ped-agregar').length) $resultados.addClass('d-none');
    });

    function agregarProducto(p) {
        if (!p || !p.id) return;

        // Si ya esta en el pedido (incluso quitado), se reutiliza esa fila.
        var $existente = $cuerpo.find('.ped-fila[data-id-corte="' + p.id + '"]');
        if ($existente.length) {
            $existente.removeClass('ped-quitada').show();
            $existente.find('.ped-cant').first().trigger('focus');
            return;
        }

        var $fila = $('<tr class="ped-fila"></tr>').attr({
            'data-id-corte': p.id,
            'data-precio': '',
            'data-pesable': p.pesable ? '1' : '0',
            'data-promedio': p.pesable && p.promedio > 0 ? p.promedio : ''
        });
        $fila.append($('<td class="text-muted"></td>').text(p.codigo));
        $fila.append($('<td></td>').text(p.nombre));

        // Una celda por sucursal, en el mismo orden que el encabezado (los ids salen de la 1.a fila o de los inputs de las demas).
        var ids = idsSucursalesEnOrden();
        ids.forEach(function (id) {
            var $input = $('<input type="text" inputmode="decimal" class="form-control form-control-sm text-right ped-cant">')
                .attr({ 'data-id-sucursal': id, 'aria-label': p.nombre });
            $fila.append($('<td class="text-right"></td>').append($input));
        });

        $fila.append('<td class="text-right text-nowrap font-weight-bold ped-total">0</td>');
        $fila.append('<td class="text-right text-nowrap ped-precio"><span class="text-muted">sin precio</span></td>');
        $fila.append('<td class="text-right text-nowrap ped-importe">—</td>');
        $fila.append('<td class="text-center"><button type="button" class="btn btn-sm btn-outline-danger ped-quitar" title="Quitar este producto del pedido"><i class="fas fa-trash"></i></button></td>');

        $cuerpo.append($fila);
        $fila.find('.ped-cant').first().trigger('focus');
        recalcular();
    }

    // Ids de sucursal en el orden de las columnas. Salen de los data-id-sucursal de los encabezados guardados en la tabla.
    var idsColumnas = null;
    function idsSucursalesEnOrden() {
        if (idsColumnas) return idsColumnas;
        var $primera = $cuerpo.find('.ped-fila').first();
        if ($primera.length) {
            idsColumnas = $primera.find('.ped-cant').map(function () { return parseInt($(this).attr('data-id-sucursal'), 10); }).get();
        } else {
            idsColumnas = [];
        }
        return idsColumnas;
    }

    // ---------- Guardar ----------

    function pedirAutorizacion(mensaje) {
        var $html = $('<div></div>');
        $html.append($('<p class="text-left"></p>').text(mensaje || 'Para modificar este pedido hace falta el usuario y la contraseña de una persona autorizada.'));
        $html.append('<input id="swalPedidoUsuario" class="swal2-input" placeholder="Usuario" autocomplete="off">');
        $html.append('<input id="swalPedidoClave" type="password" class="swal2-input" placeholder="Contraseña" autocomplete="off">');

        return window.Swal.fire({
            title: 'Autorización',
            html: $html.get(0),
            focusConfirm: false,
            showCancelButton: true,
            confirmButtonText: 'Autorizar y guardar',
            cancelButtonText: 'Cancelar',
            didOpen: function () {
                // Los inputs son parte del html propio (no los de SweetAlert), asi que Enter no confirma solo: se conecta a mano.
                $('#swalPedidoUsuario, #swalPedidoClave').on('keydown', function (e) {
                    if (e.key === 'Enter') { e.preventDefault(); window.Swal.clickConfirm(); }
                });
                $('#swalPedidoUsuario').trigger('focus');
            },
            preConfirm: function () {
                var usuario = ($('#swalPedidoUsuario').val() || '').trim();
                var clave = $('#swalPedidoClave').val() || '';
                if (!usuario || !clave) {
                    window.Swal.showValidationMessage('Ingresá el usuario y la contraseña.');
                    return false;
                }
                return { usuario: usuario, clave: clave };
            }
        }).then(function (r) { return r.isConfirmed ? r.value : null; });
    }

    function enviar(supervisor) {
        guardando = true;
        $btnGuardar.prop('disabled', true);

        var cuerpo = {
            nota: ($('#pedNota').val() || '').trim(),
            version: version,
            lineas: lineas()
        };
        if (supervisor) cuerpo.supervisor = supervisor;

        $.ajax({
            url: guardarUrl,
            type: 'POST',
            contentType: 'application/json',
            headers: { RequestVerificationToken: token() },
            data: JSON.stringify(cuerpo)
        }).done(function (resp) {
            if (resp && resp.ok) {
                window.Swal.fire({ icon: 'success', title: 'Pedido modificado', timer: 1500, showConfirmButton: false })
                    .then(function () { window.location.href = detalleUrl; });
                return;
            }

            guardando = false;
            $btnGuardar.prop('disabled', false);

            if (resp && resp.requiereAutorizacion) {
                // Falta la clave o fue rechazada: se vuelve a pedir (salvo que el servidor haya bloqueado por demasiados intentos).
                if (resp.bloqueado) { window.Swal.fire('Demasiados intentos', resp.mensaje || '', 'warning'); return; }
                pedirAutorizacion(supervisor ? resp.mensaje : null).then(function (datos) { if (datos) enviar(datos); });
                return;
            }

            window.Swal.fire('No se pudo guardar', (resp && resp.mensaje) || 'Intentá de nuevo.', 'warning');
        }).fail(function (xhr) {
            guardando = false;
            $btnGuardar.prop('disabled', false);
            var msg = xhr && xhr.responseJSON && xhr.responseJSON.mensaje;
            window.Swal.fire('No se pudo guardar', msg || 'Ocurrió un error. Intentá de nuevo.', 'error');
        });
    }

    $btnGuardar.on('click', function () {
        if (guardando) return;
        if (!lineas().length) {
            window.Swal.fire('Sin cantidades', 'El pedido tiene que quedar con al menos una cantidad.', 'info');
            return;
        }

        if (requiereAutorizacion) {
            pedirAutorizacion().then(function (datos) { if (datos) enviar(datos); });
        } else {
            enviar(null);
        }
    });

    recalcular();
})(window.jQuery);
