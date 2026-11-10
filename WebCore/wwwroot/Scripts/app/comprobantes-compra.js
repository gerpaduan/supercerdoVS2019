// Modal "Comprobantes de la compra": factura, notas de credito y de debito del proveedor, titular propio y estado de factura.
// Se abre con cualquier boton .btn-comprobantes-compra[data-id] (bandeja de Compras/Index y pantalla Compras/Editar).
// Flujo: boton -> GET Compras/Comprobantes?id= (el HTML del modal lo arma _ComprobantesCompra.cshtml) -> acciones por fetch (guardar,
// eliminar con motivo, marcar "no lleva factura", cambiar titular) -> tras cada accion se vuelve a pedir el contenido.
// Al cerrar, si hubo cambios, avisa con el evento 'comprobantes-compra:cambio' en document (Index recarga la lista; Editar copia
// el resumen). No re-graba la compra, ni el stock ni la cuenta corriente: el servidor solo toca comprobantes, estado e idtitular.
// Config: atributo data-config de <div id="comprobantesCompraConfig"> (URLs de las acciones). Depende de: jQuery, Bootstrap 4 y SweetAlert2.
// Ver docs/DECISIONS.md (2026-11-09, "Compras: titular propio y comprobantes del proveedor").
function iniciarComprobantesCompra(window, $) {
    'use strict';

    if (!$) return;

    // Largo minimo/maximo del motivo de baja de un comprobante (el servidor valida el suyo; esto solo evita ida y vuelta).
    var MOTIVO_MIN = 5;
    var MOTIVO_MAX = 300;
    // Texto de la ayuda de calculo cuando la alicuota esta en "A mano" (el valor de ese option es la cadena vacia).
    var ALICUOTA_A_MANO = '';

    var cfg = null;
    var idCompraActual = 0;
    var huboCambios = false;
    var ocupado = false;

    // Todo texto que viene del servidor se inserta escapado.
    function escapar(texto) {
        return $('<div>').text(texto == null ? '' : String(texto)).html();
    }

    function leerConfig() {
        var nodo = document.getElementById('comprobantesCompraConfig');
        if (!nodo) return null;
        try { return JSON.parse(nodo.getAttribute('data-config') || ''); } catch (e) { return null; }
    }

    function aviso(icono, texto) {
        if (window.Swal) { window.Swal.fire({ icon: icono, text: texto, confirmButtonText: 'Aceptar' }); }
        else { window.alert(texto); }
    }

    // ---- Numeros: "." y "," son decimales y no hay separador de miles (igual que Elaborados/Editar) ----

    // Texto del usuario -> numero (NaN si no es un numero valido). Vacio = 0.
    function leerImporte(texto) {
        var t = String(texto == null ? '' : texto).trim().replace(',', '.');
        if (t === '') return 0;
        if (!/^\d+(\.\d+)?$/.test(t)) return NaN;
        return parseFloat(t);
    }

    function redondear2(n) { return Math.round((n + Number.EPSILON) * 100) / 100; }

    // Numero -> texto con punto decimal y dos decimales (es lo que espera el servidor).
    function aTexto(n) { return redondear2(n).toFixed(2); }

    // ---- Armado del modal ----

    function armarCaja() {
        $('#modalComprobantesCompra').remove();
        var html = ''
            + '<div class="modal fade" id="modalComprobantesCompra" tabindex="-1" role="dialog" aria-modal="true" data-backdrop="static">'
            + '<style>'
            + '#modalComprobantesCompra{font-size:.85rem}'
            + '#modalComprobantesCompra .cc-etiqueta{font-size:.75rem;font-weight:600;color:#6c757d;margin-bottom:.15rem}'
            + '#modalComprobantesCompra .cc-cabecera{display:flex;flex-wrap:wrap;gap:.25rem 1.25rem}'
            + '#modalComprobantesCompra .cc-form{border-top:1px solid #dee2e6;padding-top:.6rem}'
            + '#modalComprobantesCompra .cc-form-titulo{font-weight:600}'
            + '#modalComprobantesCompra .cc-tabla{font-size:.8rem}'
            + '#modalComprobantesCompra .cc-fila-resta td{background:#fff5f5}'
            + '#modalComprobantesCompra .cc-total-item{font-size:.85rem}'
            + '#modalComprobantesCompra .form-control-sm{font-size:.8rem}'
            + '#modalComprobantesCompra .btn-sm{font-size:.8rem}'
            + '</style>'
            + '<div class="modal-dialog modal-lg modal-dialog-scrollable" role="document"><div class="modal-content">'
            + '<div class="modal-header py-2"><h6 class="modal-title mb-0">Comprobantes del proveedor</h6>'
            + '<button type="button" class="close" data-dismiss="modal" aria-label="Cerrar"><span aria-hidden="true">&times;</span></button></div>'
            + '<div class="modal-body py-2" id="ccCuerpo"><div class="text-muted">Cargando…</div></div>'
            + '<div class="modal-footer py-2"><button type="button" class="btn btn-sm btn-secondary" data-dismiss="modal">Cerrar</button></div>'
            + '</div></div></div>';
        $('body').append(html);
        var $m = $('#modalComprobantesCompra');

        // Delegacion: el contenido se reemplaza en cada recarga.
        $m.on('click', '#ccBtnIgualTotal', igualAlTotal);
        $m.on('input change', '#ccTotal, #ccOtros, #ccAlicuota', recalcular);
        $m.on('input', '.cc-entero', function () { this.value = this.value.replace(/\D/g, ''); });
        $m.on('submit', '#ccForm', function (e) { e.preventDefault(); guardar(); });
        $m.on('click', '.cc-btn-editar', function () { cargarParaEditar($(this)); });
        $m.on('click', '.cc-btn-borrar', function () { pedirMotivoYBorrar($(this)); });
        $m.on('click', '#ccBtnCancelarEdicion', limpiarFormulario);
        $m.on('click', '#ccBtnNoLleva', function () { marcarSinFactura(false); });
        $m.on('click', '#ccBtnVolverPendiente', function () { marcarSinFactura(true); });
        $m.on('change', '#ccTitular', function () { cambiarTitular($(this), false); });
        $m.on('hidden.bs.modal', function () {
            var cambio = huboCambios;
            $m.remove();
            if (cambio) $(document).trigger('comprobantes-compra:cambio', [{ idCompra: idCompraActual }]);
        });
        return $m;
    }

    // Pide el contenido del modal y lo pinta. Devuelve la promesa del GET.
    function cargarContenido() {
        var $cuerpo = $('#ccCuerpo');
        return $.get(cfg.urlVer, { id: idCompraActual })
            .done(function (html) {
                // El servidor devuelve HTML de su propia vista (mismo origen); los mensajes de error vienen como texto plano.
                if (typeof html === 'string' && html.indexOf('cc-modal-contenido') === -1) {
                    $cuerpo.html('<div class="alert alert-warning mb-0">' + escapar(html) + '</div>');
                    return;
                }
                $cuerpo.html(html);
                recalcular();
            })
            .fail(function (xhr) {
                var texto = xhr && xhr.responseText ? xhr.responseText : 'No se pudieron cargar los comprobantes.';
                $cuerpo.html('<div class="alert alert-danger mb-0">' + escapar(texto) + '</div>');
            });
    }

    function abrir(idCompra) {
        cfg = leerConfig();
        if (!cfg) { aviso('error', 'Falta la configuración de comprobantes en la pantalla.'); return; }
        idCompraActual = idCompra;
        huboCambios = false;
        var $m = armarCaja();
        $m.modal('show');
        cargarContenido();
    }

    // ---- Formulario ----

    function $c(id) { return $('#modalComprobantesCompra ' + id); }

    function totalCompra() { return parseFloat($c('.cc-modal-contenido').attr('data-total-compra')) || 0; }

    // Con alicuota elegida: neto e IVA salen del total (menos otros impuestos) y se bloquean. "A mano": se escriben.
    function recalcular() {
        var alic = $c('#ccAlicuota').val();
        var aMano = alic === ALICUOTA_A_MANO;
        $c('#ccNeto, #ccIva').prop('readonly', !aMano);
        if (aMano) return;
        var total = leerImporte($c('#ccTotal').val());
        var otros = leerImporte($c('#ccOtros').val());
        var pct = parseFloat(alic);
        if (isNaN(total) || isNaN(otros) || isNaN(pct)) return;
        var baseConIva = total - otros;
        if (baseConIva < 0) baseConIva = 0;
        var neto = redondear2(baseConIva / (1 + pct / 100));
        var iva = redondear2(baseConIva - neto);
        $c('#ccNeto').val(aTexto(neto));
        $c('#ccIva').val(aTexto(iva));
    }

    function igualAlTotal() {
        $c('#ccTotal').val(aTexto(totalCompra()));
        recalcular();
    }

    function limpiarFormulario() {
        var $f = $c('#ccForm');
        $c('#ccIdComprobante').val('0');
        $c('#ccTipo').val($c('.cc-modal-contenido').attr('data-tipo-sugerido'));
        $c('#ccPtoVta, #ccNumero, #ccTotal, #ccNeto, #ccIva, #ccOtros').val('');
        $c('#ccFecha').val($c('.cc-modal-contenido').attr('data-fecha-compra'));
        $c('#ccAlicuota').val($c('.cc-modal-contenido').attr('data-alicuota-default'));
        $c('#ccFormTitulo').text('Agregar comprobante');
        $c('#ccBtnGuardarTexto').text('Guardar comprobante');
        $c('#ccBtnCancelarEdicion').addClass('d-none');
        $c('#ccError').addClass('d-none').text('');
        if ($f.length) recalcular();
    }

    function cargarParaEditar($btn) {
        $c('#ccIdComprobante').val($btn.attr('data-id'));
        $c('#ccTipo').val($btn.attr('data-tipo'));
        $c('#ccPtoVta').val($btn.attr('data-ptovta'));
        $c('#ccNumero').val($btn.attr('data-numero'));
        $c('#ccFecha').val($btn.attr('data-fecha'));
        $c('#ccAlicuota').val($btn.attr('data-alicuota'));
        $c('#ccTotal').val($btn.attr('data-total'));
        $c('#ccNeto').val($btn.attr('data-neto'));
        $c('#ccIva').val($btn.attr('data-iva'));
        $c('#ccOtros').val($btn.attr('data-otros'));
        $c('#ccFormTitulo').text('Editar comprobante');
        $c('#ccBtnGuardarTexto').text('Guardar cambios');
        $c('#ccBtnCancelarEdicion').removeClass('d-none');
        $c('#ccError').addClass('d-none').text('');
        // Neto e IVA se respetan tal como estaban guardados: solo se recalculan si el usuario toca total, otros o alicuota.
        $c('#ccNeto, #ccIva').prop('readonly', $c('#ccAlicuota').val() !== ALICUOTA_A_MANO);
        var form = $c('#ccForm')[0];
        if (form && form.scrollIntoView) form.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    function mostrarError(texto) {
        $c('#ccError').text(texto).removeClass('d-none');
    }

    // ---- Acciones contra el servidor ----

    function token() { return $c('input[name="__RequestVerificationToken"]').first().val() || ''; }

    // POST de un formulario; resuelve siempre con un objeto { ok, mensaje, ... } (las fallas de red tambien).
    function enviar(url, datos) {
        datos = datos || {};
        datos.__RequestVerificationToken = token();
        return $.ajax({ url: url, type: 'POST', data: datos, dataType: 'json' })
            .then(function (r) { return r; }, function (xhr) {
                var texto = xhr && xhr.status === 400 ? 'La sesión de la pantalla venció. Cerrá y volvé a abrir los comprobantes.' : 'No se pudo conectar con el servidor.';
                return $.Deferred().resolve({ ok: false, mensaje: texto }).promise();
            });
    }

    function conOcupado(fn) {
        if (ocupado) return;
        ocupado = true;
        $c('button').prop('disabled', true);
        fn().always(function () { ocupado = false; $c('button').prop('disabled', false); });
    }

    function guardar() {
        var total = leerImporte($c('#ccTotal').val());
        var neto = leerImporte($c('#ccNeto').val());
        var iva = leerImporte($c('#ccIva').val());
        var otros = leerImporte($c('#ccOtros').val());
        if (isNaN(total) || isNaN(neto) || isNaN(iva) || isNaN(otros)) {
            mostrarError('Los importes tienen que ser números (decimales con punto o coma, sin separador de miles).');
            return;
        }
        var ptoVta = $c('#ccPtoVta').val();
        var numero = $c('#ccNumero').val();
        if (!ptoVta || !numero) { mostrarError('Completá el punto de venta y el número del comprobante.'); return; }
        $c('#ccError').addClass('d-none');

        var alic = $c('#ccAlicuota').val();
        var datos = {
            IdComprobante: $c('#ccIdComprobante').val(),
            IdCompra: idCompraActual,
            TipoComprobante: $c('#ccTipo').val(),
            PtoVta: ptoVta,
            Numero: numero,
            Fecha: $c('#ccFecha').val(),
            AlicuotaIva: alic === ALICUOTA_A_MANO ? '' : alic,
            NetoGravado: aTexto(neto),
            Iva: aTexto(iva),
            OtrosImpuestos: aTexto(otros),
            Total: aTexto(total)
        };
        conOcupado(function () {
            return enviar(cfg.urlGuardar, datos).then(function (r) {
                if (!r.ok) { mostrarError(r.mensaje || 'No se pudo guardar el comprobante.'); return; }
                huboCambios = true;
                return cargarContenido();
            });
        });
    }

    function pedirMotivoYBorrar($btn) {
        var idComprobante = $btn.attr('data-id');
        var descripcion = $btn.attr('data-descripcion') || 'el comprobante';
        if (!window.Swal) { aviso('error', 'No se pudo abrir la confirmación.'); return; }
        window.Swal.fire({
            title: 'Eliminar comprobante',
            text: descripcion + '. Queda registrado con el motivo.',
            input: 'text',
            inputPlaceholder: 'Motivo (obligatorio)',
            inputAttributes: { maxlength: MOTIVO_MAX },
            showCancelButton: true,
            confirmButtonText: 'Eliminar',
            cancelButtonText: 'Cancelar',
            inputValidator: function (v) {
                return (v || '').trim().length < MOTIVO_MIN ? 'Escribí el motivo (mínimo ' + MOTIVO_MIN + ' letras).' : null;
            }
        }).then(function (res) {
            if (!res.isConfirmed) return;
            conOcupado(function () {
                return enviar(cfg.urlEliminar, { idComprobante: idComprobante, idCompra: idCompraActual, motivo: res.value.trim() })
                    .then(function (r) {
                        if (!r.ok) { mostrarError(r.mensaje || 'No se pudo eliminar el comprobante.'); return; }
                        huboCambios = true;
                        return cargarContenido();
                    });
            });
        });
    }

    function marcarSinFactura(volverAPendiente) {
        conOcupado(function () {
            return enviar(cfg.urlSinFactura, { idCompra: idCompraActual, volverAPendiente: volverAPendiente ? 'true' : 'false' })
                .then(function (r) {
                    if (!r.ok) { mostrarError(r.mensaje || 'No se pudo actualizar la compra.'); return; }
                    huboCambios = true;
                    return cargarContenido();
                });
        });
    }

    // Cambia el titular de la compra. Si ya tiene comprobantes el servidor pide confirmar (van al CUIT del titular).
    function cambiarTitular($select, confirmado) {
        var nuevo = $select.val();
        var anterior = $c('.cc-modal-contenido').attr('data-titular-actual');
        conOcupado(function () {
            return enviar(cfg.urlCambiarTitular, { idCompra: idCompraActual, idTitular: nuevo, confirmado: confirmado ? 'true' : 'false' })
                .then(function (r) {
                    if (r.requiereConfirmacion && window.Swal) {
                        // La confirmacion se hace fuera de conOcupado para no dejar los botones trabados mientras se espera.
                        return window.Swal.fire({
                            icon: 'question', text: r.mensaje, showCancelButton: true,
                            confirmButtonText: 'Cambiar titular', cancelButtonText: 'Cancelar'
                        }).then(function (c) {
                            if (c.isConfirmed) { window.setTimeout(function () { cambiarTitular($select, true); }, 100); }
                            else { $select.val(anterior); }
                        });
                    }
                    if (!r.ok) { $select.val(anterior); mostrarError2(r.mensaje); return; }
                    huboCambios = true;
                    return cargarContenido();
                });
        });
    }

    // El formulario puede no existir (sin permiso): el error del titular se muestra como aviso.
    function mostrarError2(texto) { aviso('warning', texto || 'No se pudo cambiar el titular.'); }

    // ---- Apertura desde los botones de la pantalla ----

    $(document).on('click', '.btn-comprobantes-compra', function (e) {
        e.preventDefault();
        var id = parseInt($(this).attr('data-id'), 10);
        if (id > 0) abrir(id);
    });

    // ---- Compras/Editar: factura que vino con la mercaderia (alta) ----
    // El bloque #fpBloque vive dentro del formulario de la compra: mientras el tilde esta apagado sus campos van deshabilitados
    // (no viajan), asi un formulario a medio llenar no ensucia el alta. Los importes se normalizan a punto decimal al escribir
    // porque el servidor los lee con cultura invariante.
    function iniciarFacturaEnAlta() {
        var $tilde = $('#ConFacturaProveedor');
        var $bloque = $('#fpBloque');
        if (!$tilde.length || !$bloque.length) return;

        // El total de la compra se muestra en formato es-AR (1.234,50): se convierte a numero.
        function totalDeLaCompra() {
            var texto = ($('#totalImporte').text() || '').replace(/\./g, '').replace(',', '.');
            var n = parseFloat(texto);
            return isNaN(n) ? 0 : n;
        }

        function recalcularFactura() {
            var alic = $('#fpAlicuota').val();
            var aMano = alic === ALICUOTA_A_MANO;
            $('#fpNeto, #fpIva').prop('readonly', !aMano);
            if (aMano) return;
            var total = leerImporte($('#fpTotal').val());
            var otros = leerImporte($('#fpOtros').val());
            var pct = parseFloat(alic);
            if (isNaN(total) || isNaN(otros) || isNaN(pct)) return;
            var baseConIva = Math.max(total - otros, 0);
            var neto = redondear2(baseConIva / (1 + pct / 100));
            $('#fpNeto').val(aTexto(neto));
            $('#fpIva').val(aTexto(redondear2(baseConIva - neto)));
        }

        function aplicarTilde() {
            var activo = $tilde.is(':checked');
            $bloque.toggleClass('d-none', !activo);
            $bloque.find('input, select').prop('disabled', !activo);
            if (activo) recalcularFactura();
        }

        $tilde.on('change', aplicarTilde);
        $bloque.on('input change', '#fpTotal, #fpOtros, #fpAlicuota', recalcularFactura);
        $bloque.on('input', '.fp-entero', function () { this.value = this.value.replace(/\D/g, ''); });
        $bloque.on('input', '.fp-importe', function () { this.value = this.value.replace(',', '.').replace(/[^\d.]/g, ''); });
        $('#fpBtnIgualTotal').on('click', function () {
            $('#fpTotal').val(aTexto(totalDeLaCompra()));
            recalcularFactura();
        });
        aplicarTilde();
    }

    // ---- Compras/Editar (edicion): refresca el resumen de solo lectura al cerrar el modal con cambios ----
    $(document).on('comprobantes-compra:cambio', function (e, datos) {
        var $destino = $('#compraResumenComprobantes');
        var cfgLocal = leerConfig();
        // Solo Editar tiene este bloque; Index recarga la pagina por su cuenta.
        if (!$destino.length || !cfgLocal || !datos) return;
        $.get(cfgLocal.urlVer, { id: datos.idCompra }).done(function (html) {
            var $tmp = $('<div>').html(html);
            var $resumen = $tmp.find('[data-cc-resumen]').first();
            if (!$resumen.length) return;
            $resumen.find('.cc-col-acciones').remove();
            $destino.empty().append($resumen);
            var nombre = $tmp.find('#ccTitular option:selected').text();
            if (nombre) $('#compraTitularNombre').text(nombre.replace(/ \((principal|deshabilitado)\)$/, ''));
        });
    });

    $(iniciarFacturaEnAlta);
}

(function () {
    if (window.jQuery) iniciarComprobantesCompra(window, window.jQuery);
})();
