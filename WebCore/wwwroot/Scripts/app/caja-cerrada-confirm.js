// Aviso + confirmacion cuando lo que se esta guardando AFECTA UNA CAJA YA CERRADA (venta, compra, pago/cobro, egreso
// de caja; 2026-10-06, ver docs/DECISIONS.md "Cambios en cajas cerradas y reapertura").
// El servidor decide: si el cambio altera una caja cerrada y el request no trae confirmarCajaCerrada=true, responde
//   { ok:false, requiereConfirmacion:true, tipoConfirmacion:'cajaCerrada', mensaje, nota, cambio:[...], cajas:[...] }
// (WebCore/Helpers/CajaCerradaRespuesta.cs). Cada pantalla llama a CajaCerradaConfirm.manejar(resp, reenviar) ANTES de
// tratar la respuesta como error: si devuelve true se mostro el aviso y, si el usuario confirma, se llama a reenviar()
// (la pantalla vuelve a mandar el mismo formulario con confirmarCajaCerrada=true). Mismo patron que CerrarCaja
// (confirmarVentaEnCurso).
// Depende de: jQuery y SweetAlert2 (Swal). Sin input / boton de denegar / boton de cerrar en el Swal: asi lo
// reconoce swal-single-confirm.js (Enter confirma, Escape cancela y se suspende el focus trap de los modales).
(function (window, $) {
    'use strict';

    if (!$ || window.CajaCerradaConfirm) return;

    // Todo texto que viene del servidor se inserta ya escapado (no se confia en el contenido).
    function escapar(texto) {
        return $('<div>').text(texto == null ? '' : String(texto)).html();
    }

    function esConfirmacion(resp) {
        return !!(resp && resp.requiereConfirmacion && resp.tipoConfirmacion === 'cajaCerrada');
    }

    function armarHtml(resp) {
        var html = '<div class="text-left" style="font-size:.9rem">';
        html += '<p class="mb-2">' + escapar(resp.mensaje) + '</p>';

        var cajas = resp.cajas || [];
        if (cajas.length) {
            html += '<ul class="mb-2 pl-3">';
            cajas.forEach(function (c) {
                html += '<li>Caja <b>#' + escapar(c.id) + '</b> de ' + escapar(c.cajero)
                    + (c.sucursal ? ' (' + escapar(c.sucursal) + ')' : '')
                    + ', del ' + escapar(c.desde) + ' al ' + escapar(c.hasta)
                    + (c.cerradaPor ? ', cerrada por ' + escapar(c.cerradaPor) : '') + '</li>';
            });
            html += '</ul>';
        }

        var cambio = resp.cambio || [];
        if (cambio.length) {
            html += '<div class="mb-2"><b>Qué se modifica:</b><ul class="mb-0 pl-3">';
            cambio.forEach(function (linea) { html += '<li>' + escapar(linea) + '</li>'; });
            html += '</ul></div>';
        }

        if (resp.nota) html += '<p class="mb-2 text-muted">' + escapar(resp.nota) + '</p>';

        html += '<p class="mb-0">Si guardás igual, queda un <b>aviso en el cierre de esa caja</b> indicando que se modificaron datos.</p>';
        html += '</div>';
        return html;
    }

    // Devuelve true si `resp` era un pedido de confirmacion (y se mostro el aviso); false si no es de este tipo y
    // la pantalla debe seguir con su manejo normal. `opciones`: { textoConfirmar, onCancelar }.
    function manejar(resp, reenviar, opciones) {
        if (!esConfirmacion(resp)) return false;

        opciones = opciones || {};

        Swal.fire({
            icon: 'warning',
            title: 'Esta caja ya fue cerrada',
            html: armarHtml(resp),
            showCancelButton: true,
            confirmButtonText: opciones.textoConfirmar || 'Guardar igual',
            cancelButtonText: 'Volver',
            confirmButtonColor: '#e74a3b'
        }).then(function (r) {
            if (r && r.isConfirmed) {
                if (typeof reenviar === 'function') reenviar();
            } else if (typeof opciones.onCancelar === 'function') {
                opciones.onCancelar();
            }
        });

        return true;
    }

    window.CajaCerradaConfirm = { manejar: manejar, esConfirmacion: esConfirmacion };
})(window, window.jQuery);
