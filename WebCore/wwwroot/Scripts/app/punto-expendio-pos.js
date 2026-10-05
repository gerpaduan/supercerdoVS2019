// Port recortado de Web/Scripts/app/punto-expendio-pos.js (974 lineas) -- UI de Expendios, ver
// docs/10-migracion-aspnet-core/PLAN-POS-EXPENDIOS-UI.md. Recortes deliberados respecto al
// original (mismo criterio ya usado en Ventas/POS.cshtml):
//   - Sin "Mis expendios" (F6) ni su modal -- misExpendiosCache y todas las funciones
//     relacionadas (loadMisExpendios/renderMisExpendios/etc.) se sacaron enteras.
//   - Sin PosOperadorConfig/login de operador de produccion -- "esperandoOperadorPOS" queda
//     siempre false (window.PosOperadorConfig nunca se define en WebCore).
//   - Sin buscar cliente real (#btnBuscarPersona esta disabled en la vista) -- se saco el
//     handler que abria el modal de busqueda de personas.
//   - window.PostPuntoExpendioModal.open(resp) (modal-postexpendio.js, no portado) reemplazado
//     por el wiring directo del modal basico (_ModalPostPuntoExpendioBasico.cshtml, solo PDF +
//     email, mismos endpoints ya portados que ExpendiosGenerados.cshtml).
(function (window, $, document) {
    'use strict';

    function parseMoney(value) {
        return parseDecimal(String(value == null ? '' : value).replace(/\$/g, '').trim());
    }

    function parseDecimal(value) {
        var text = String(value == null ? '' : value).trim();
        if (!text) return 0;

        text = text.replace(/\s/g, '');

        var lastComma = text.lastIndexOf(',');
        var lastDot = text.lastIndexOf('.');
        var decimalSep = '';

        if (lastComma >= 0 && lastDot >= 0) decimalSep = lastComma > lastDot ? ',' : '.';
        else if (lastComma >= 0) decimalSep = ',';
        else if (lastDot >= 0) decimalSep = '.';

        var normalized = '';
        var decimalIndex = decimalSep ? text.lastIndexOf(decimalSep) : -1;

        for (var i = 0; i < text.length; i++) {
            var ch = text.charAt(i);
            if (ch >= '0' && ch <= '9') normalized += ch;
            else if (ch === '-' && normalized.length === 0) normalized += ch;
            else if ((ch === ',' || ch === '.') && i === decimalIndex) normalized += '.';
        }

        var n = parseFloat(normalized);
        return isNaN(n) ? 0 : n;
    }

    $(function () {
        var config = window.puntoExpendioPosConfig || {};
        if (!document.getElementById('pos-app')) return;
        // "Mis expendios" (F6, 2026-09-06, retomado -- ver docs/DECISIONS.md).
        var misExpendiosCache = [];
        var misExpendiosFetchState = { fechaDesde: '', fechaHasta: '' };

        window.buscarProductoUrl = config.urlBuscarProductoPos;

        var POSState = window.POSState;
        POSState.clear();
        POSState.setFechaVenta(new Date());

        var fechaInput = document.getElementById('fechaExpendio');
        var fechaLabel = document.getElementById('fechaHoraPOSExpendio');

        function pad(value) {
            return String(value).padStart(2, '0');
        }

        function formatFechaSql(fecha) {
            return fecha.getFullYear() + '-' + pad(fecha.getMonth() + 1) + '-' + pad(fecha.getDate()) +
                'T' + pad(fecha.getHours()) + ':' + pad(fecha.getMinutes()) + ':' + pad(fecha.getSeconds());
        }

        // "Mis expendios" (F6, 2026-09-06, retomado -- ver docs/DECISIONS.md): formato para
        // <input type="date">, distinto de formatFechaSql (que incluye hora).
        function formatDateInput(fecha) {
            return fecha.getFullYear() + '-' + pad(fecha.getMonth() + 1) + '-' + pad(fecha.getDate());
        }

        function formatFechaVisible(fecha) {
            var dias = ['domingo', 'lunes', 'martes', 'miércoles', 'jueves', 'viernes', 'sábado'];
            var meses = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'];

            return dias[fecha.getDay()] + ' ' + fecha.getDate() + ' de ' + meses[fecha.getMonth()] + ', ' +
                fecha.getFullYear() + '. ' + pad(fecha.getHours()) + ':' + pad(fecha.getMinutes());
        }

        // Sector actual (hidden #sectorPuntoExpendio). PRESUPUESTO y REMITOS son sectores
        // globales con reglas propias (ver Negocio.SectorPuntoExpendio en el servidor, que es
        // quien decide de verdad: esto es solo UX).
        function sectorActual() {
            return String($('#sectorPuntoExpendio').val() || '').trim().toUpperCase();
        }

        function esSectorPresupuesto() { return sectorActual() === 'PRESUPUESTO'; }
        function esSectorRemitos() { return sectorActual() === 'REMITOS'; }

        // Con true, el usuario eligio una fecha a mano (chip clickeable): el reloj de 60 s ya no
        // debe pisar #fechaExpendio con la hora actual.
        var fechaManual = false;

        function actualizarFechaHora() {
            if (fechaManual) return;

            var lineas = POSState.getLineas().filter(function (linea) { return !!linea; });
            if (!(lineas.length === 0 || lineas.length === 1)) return;

            var ahora = new Date();
            POSState.setFechaVenta(ahora);

            if (fechaInput) fechaInput.value = formatFechaSql(ahora);
            if (fechaLabel) fechaLabel.textContent = formatFechaVisible(ahora);
            actualizarCaducaEl();   // la caducidad cuenta desde la vigencia: si cambia la fecha, se recalcula
        }

        function beep() {
            var audio = document.getElementById('beep');
            if (!audio) return;
            audio.currentTime = 0;
            audio.play().catch(function () { });
        }

        document.addEventListener('click', function habilitarAudio() {
            var audio = document.getElementById('beep');
            if (!audio) return;
            audio.volume = 0;
            audio.play().finally(function () {
                audio.pause();
                audio.currentTime = 0;
                audio.volume = 1;
            });
        }, { once: true });

        // Alta rapida de producto (mismo criterio que Ventas/POS): con el formulario abierto se ignoran
        // las lecturas, y el alta solo se ofrece en la segunda lectura IGUAL consecutiva (la busqueda
        // en si se dispara en cada lectura, como siempre). posProduct se resuelve en runtime: se
        // crea mas abajo, antes de que la camara pueda detectar algo.
        var ultimoCodigoLeidoCamara = null;

        var scanner = new BarcodeScanner({
            videoSelector: '#videoScanner',
            containerSelector: '#scannerContainer',
            onCodeDetected: function (codigo) {
                if (posProduct && typeof posProduct.isModoAltaProductoActivo === 'function' && posProduct.isModoAltaProductoActivo()) {
                    return;
                }

                var confirmadoPorDobleLectura = (ultimoCodigoLeidoCamara === codigo);
                ultimoCodigoLeidoCamara = codigo;

                if (posProduct && typeof posProduct.registrarOrigenLecturaCamara === 'function') {
                    posProduct.registrarOrigenLecturaCamara(codigo, confirmadoPorDobleLectura);
                }

                document.querySelector('#inputCodigo').value = codigo;
                document.querySelector('#inputCodigo').focus();
                window.manejarEnter();
            }
        });

        document.getElementById('btnScanner')?.addEventListener('click', function () { scanner.iniciar(); });
        document.getElementById('btnCerrarScanner')?.addEventListener('click', function () { scanner.cerrar(); });
        document.getElementById('btnFlash')?.addEventListener('click', async function () {
            var estado = await scanner.toggleFlash();
            var btn = document.getElementById('btnFlash');
            if (!btn) return;
            btn.textContent = estado ? 'Flash ON' : 'Flash';
            btn.classList.toggle('btn-warning', !!estado);
            btn.classList.toggle('btn-secondary', !estado);
        });

        function getPrecioParaAgregar(productoSeleccionado) {
            if (!config.permiteEditarPrecio) {
                return productoSeleccionado ? productoSeleccionado.precioKg : 0;
            }

            var precioManual = parseDecimal($('#inputPrecioManualExpendio').val());
            return precioManual > 0 ? precioManual : (productoSeleccionado ? productoSeleccionado.precioKg : 0);
        }

        function showConnectionError(msg) {
            Swal.fire({ icon: 'error', title: 'Sin conexión', text: msg });
        }

        function mostrarAvisoBalanzaDiscreto(msg) {
            var $msg = $('#msgBalanzaPOS');
            if (!$msg.length) return;
            $msg.text(msg || '');
        }

        function normalizarProductoParaBalanza(producto) {
            if (!producto) return null;

            var normalized = $.extend({}, producto);
            normalized.pesable = producto.pesable === true || producto.balanza === true;
            normalized.balanza = producto.balanza === true || normalized.pesable === true;
            return normalized;
        }

        function clickIfEnabled(selector) {
            var button = document.querySelector(selector);
            if (button && !button.disabled) button.click();
        }

        function firePosAlert(options) {
            return Swal.fire($.extend(true, {
                focusConfirm: true,
                didOpen: function () {
                    var confirmButton = Swal.getConfirmButton();
                    if (confirmButton) confirmButton.focus();
                }
            }, options || {}));
        }

        function scrollPantallaMobile() {
            if (!window.matchMedia('(max-width: 576px)').matches) return;
            var anchor = document.getElementById('pos-bottom-anchor');
            if (anchor) anchor.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }

        var posProduct = null;
        var posCart = null;
        var posBalanza = null;
        var posKeyboard = window.POSKeyboard.create({
            handleEnter: function () { return posProduct ? posProduct.handleEnter() : null; },
            calculateSubtotal: function () { return posCart ? posCart.calculateSubtotal() : null; },
            finishTyping: function (value) { return posProduct ? posProduct.finishTyping(value) : null; },
            setEnterDesdeTecladoVirtual: function (value) { return posProduct ? posProduct.setEnterDesdeTecladoVirtual(value) : null; },
            onManualModeRequested: function (inputId) { return posBalanza ? posBalanza.alternarDesdeAsterisco(inputId) : null; }
        });
        var posHelp = window.POSHelp.create({
            focusCodigo: function () { return posKeyboard.focusCodigo(); },
            clickIfEnabled: clickIfEnabled
        });
        var posComment = window.POSComment.create({
            POSState: POSState,
            focusCodigo: function () { return posKeyboard.focusCodigo(); }
        });

        posProduct = window.POSProduct.create({
            soloFormaPago: false,
            calculateSubtotal: function () { return posCart ? posCart.calculateSubtotal() : null; },
            addProduct: function () { return posCart ? posCart.addProduct() : null; },
            getInputActivo: function () { return posKeyboard.getInputActivo(); },
            clearInputActivo: function () { return posKeyboard.clearInputActivo(); },
            showConnectionError: showConnectionError,
            onProductoChanged: function (producto) { return posBalanza ? posBalanza.onProductoChanged(normalizarProductoParaBalanza(producto)) : null; },
            onProductoConfirmed: function (producto) { return posBalanza ? posBalanza.onProductoConfirmed(normalizarProductoParaBalanza(producto)) : null; }
        });

        posCart = window.POSCart.create({
            POSState: POSState,
            soloFormaPago: false,
            eliminarFisicoLineasPendientes: true,
            deshabilitarAvisoSalida: true,
            puedeBonificar: config.puedeBonificar === true,
            mensajeSinPermisoBonificar: config.mensajeSinPermisoBonificar || 'No tiene permisos para bonificar.',
            getProductoSeleccionado: function () { return posProduct.getProductoSeleccionado(); },
            setProductoSeleccionado: function (value) { return posProduct.setProductoSeleccionado(value); },
            getPrecioActual: function () { return getPrecioParaAgregar(posProduct.getProductoSeleccionado()); },
            getPrecioParaAgregar: getPrecioParaAgregar,
            beep: beep,
            focusCodigo: function () { return posKeyboard.focusCodigo(); },
            showWaiting: function () { return posProduct.showWaiting(); },
            scrollPantallaMobile: scrollPantallaMobile,
            handleEnter: function () { return posProduct.handleEnter(); },
            getTypingTimer: function () { return posProduct.getTypingTimer(); },
            setEnterDesdeTecladoVirtual: function (value) { return posProduct.setEnterDesdeTecladoVirtual(value); },
            beforeAddProduct: function (producto, cantidad) {
                return posBalanza
                    ? posBalanza.beforeAddProduct(normalizarProductoParaBalanza(producto), cantidad)
                    : { ok: true };
            }
        });

        posBalanza = window.POSBalanza.create({
            baseUrl: 'http://127.0.0.1:5100',
            statusIntervalMs: 2500,
            pesoIntervalMs: 250,
            calculateSubtotal: function () { return posCart ? posCart.calculateSubtotal() : null; },
            focusCantidad: function () { return posKeyboard.focusCantidad(); },
            focusCodigo: function () { return posKeyboard.focusCodigo(); },
            showNotice: mostrarAvisoBalanzaDiscreto
        });

        posProduct.init();
        posCart.init();
        posBalanza.init();
        posKeyboard.init();
        posHelp.init();
        posComment.init();
        window.POSMultiInstance?.init?.();
        actualizarFechaHora();
        window.setInterval(actualizarFechaHora, 60000);

        // Fecha y hora editable (solo PRESUPUESTO y REMITOS, mismo patron click-para-editar que
        // Ventas/POS). REMITOS: nunca futura. PRESUPUESTO: futura hasta N dias (precios que rigen
        // a partir de otro dia). Fuera de rango vuelve a "ahora". El servidor valida lo mismo
        // (Negocio.SectorPuntoExpendio.ResolverFecha) -- esto no es el unico control.
        var $fechaChip = $('#fechaHoraPOSExpendioChip');
        var $fechaEditable = $('#fechaExpendioEditable');

        if ($fechaChip.length && $fechaEditable.length) {
            var diasFuturoMax = parseInt($fechaEditable.data('dias-futuro-max'), 10) || 0;

            function formatDatetimeLocal(fecha) {
                return fecha.getFullYear() + '-' + pad(fecha.getMonth() + 1) + '-' + pad(fecha.getDate()) +
                    'T' + pad(fecha.getHours()) + ':' + pad(fecha.getMinutes());
            }

            function revelarFechaEditable() {
                $fechaEditable.val(formatDatetimeLocal(new Date($('#fechaExpendio').val() || new Date())));
                $('#fechaHoraPOSExpendioWrap').addClass('d-none');
                $('#fechaExpendioEditableWrap').removeClass('d-none');
                $fechaEditable.trigger('focus');
            }

            $fechaChip.on('click', revelarFechaEditable).on('keydown', function (e) {
                if (e.key !== 'Enter' && e.key !== ' ') return;
                e.preventDefault();
                revelarFechaEditable();
            });

            $fechaEditable.on('change', function () {
                var valor = $(this).val();
                if (!valor) return;

                var seleccionada = new Date(valor);
                var ahora = new Date();
                var maxima = esSectorPresupuesto()
                    ? new Date(ahora.getTime() + diasFuturoMax * 24 * 60 * 60 * 1000)
                    : ahora;

                if (isNaN(seleccionada.getTime()) || seleccionada > maxima) {
                    seleccionada = ahora;
                    $(this).val(formatDatetimeLocal(ahora));
                    firePosAlert({
                        icon: 'warning',
                        title: 'Fecha',
                        text: esSectorPresupuesto()
                            ? 'La fecha del presupuesto no puede superar ' + diasFuturoMax + ' días a futuro.'
                            : 'La fecha no puede ser futura.'
                    });
                }

                fechaManual = true;
                POSState.setFechaVenta(seleccionada);
                if (fechaInput) fechaInput.value = formatFechaSql(seleccionada);
                if (fechaLabel) fechaLabel.textContent = formatFechaVisible(seleccionada);
                actualizarCaducaEl();   // vigencia nueva -> "Caduca el ..." nuevo
            });
        }

        $('#inputPrecioManualExpendio').on('input', function () {
            posCart.calculateSubtotal();
        });

        $('#btnConsumidorFinal').on('click', function () {
            $('#idPersona').val(config.idConsumidorFinal || 0);
            $('#razonSocial').val('Consumidor Final');
            setClienteIdentificacionVisual('', 'Consumidor Final');
            // Si habia un cliente real fijo, Consumidor Final lo reemplaza y el campo vuelve a ser editable.
            actualizarEstadoClienteFijo(config.idConsumidorFinal || 0);
        });

        // Buscador de cliente real (F9), 2026-09-06 (retomado -- ver docs/DECISIONS.md). Port
        // literal de Web/Scripts/app/punto-expendio-pos.js:525-556 -- a diferencia de Ventas/POS
        // (persona-buscar.js maneja el click), aca el handler es propio porque no usa POSGuard
        // (PuntosExpendio no tiene esa infraestructura de guard de modales apilados).
        window.api = window.api || {};
        window.api.persona = {
            listar: config.urlPersonaListar,
            buscar: config.urlPersonaBuscar,
            obtener: config.urlPersonaObtener,
            crear: config.urlPersonaCrear,
            guardarCrear: config.urlPersonaGuardarCrear
        };

        $(document).on('click', '#btnBuscarPersona', function () {
            var $btn = $(this);
            var $contenedor = $('#contenedorModalPersona');

            $('#modalBuscarPersona').removeData('origen-persona-buscar');

            if ($contenedor.find('#modalBuscarPersona').length) {
                $('#modalBuscarPersona').modal('show');
                if (typeof cargarPersonas === 'function') cargarPersonas();
                return;
            }

            $btn.prop('disabled', true);
            $.get(window.api.persona.buscar)
                .done(function (html) {
                    $contenedor.html(html);
                    $('#modalBuscarPersona').modal('show');
                    if (typeof cargarPersonas === 'function') cargarPersonas();
                })
                .fail(function () {
                    Swal.fire({ icon: 'error', title: 'Cliente', text: 'No se pudo cargar el buscador de clientes' });
                })
                .always(function () {
                    $btn.prop('disabled', false);
                });
        });

        $('#razonSocial').on('input', function () {
            $('#idPersona').val('0');
            setClienteIdentificacionVisual('', '');
        });

        $('#razonSocial').on('keydown', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            clickIfEnabled('#btnFinalizar');
        });

        function setClienteIdentificacionVisual(identificacion, razonSocial) {
            var razon = (razonSocial || '').toString().trim();
            var identificacionNormalizada = (identificacion || '').toString().trim();
            var $wrap = $('#clienteRazonSocialWrap');
            var $texto = $('#clienteRazonSocialTexto');
            var $valor = $('#clienteRazonSocialValor');

            if (!$wrap.length || !$texto.length) return;

            if (identificacionNormalizada) $('#razonSocial').val(identificacionNormalizada);

            var mostrar = !!identificacionNormalizada &&
                !!razon &&
                identificacionNormalizada.localeCompare(razon, undefined, { sensitivity: 'accent' }) !== 0;

            if ($valor.length) $valor.val(razon);

            $texto.text(razon);
            $wrap.attr('title', razon);
            $wrap.toggleClass('d-none', !mostrar);
        }
        window.setClienteIdentificacionVisual = setClienteIdentificacionVisual;

        // ===== Cliente fijo + historial de precios (2026-10-04, ver docs/DECISIONS.md) =====
        // Al elegir un cliente real (F9) el campo "Cliente" queda de solo lectura y su id viaja al
        // guardar (IdPersona); la X lo suelta y se vuelve a identificacion manual (texto libre).
        // Consumidor Final NO se fija: sigue siendo texto editable y sin id. Tras guardar, la
        // pantalla se recarga y vuelve a estar vacia y en modo manual, como siempre.

        // Id del cliente real fijado, o 0 si el cliente es manual / Consumidor Final.
        function idClienteFijo() {
            var id = parseInt($('#idPersona').val(), 10) || 0;
            return (id > 0 && id !== (config.idConsumidorFinal || 0)) ? id : 0;
        }

        // PageDown / AvPag: con cliente fijo no hay nada para tipear, asi que el foco va a la X.
        function enfocarCliente() {
            if (idClienteFijo()) $('#btnQuitarCliente').trigger('focus');
            else $('#razonSocial').trigger('focus');
        }

        // Cada cambio de cliente invalida las respuestas de "Personas/Obtener" que sigan en vuelo.
        var tokenAccesoHistorial = 0;

        // Aplica el estado de "cliente fijo" a la pantalla: campo de solo lectura, X visible y boton
        // de historial. persona-buscar.js (compartido con Ventas/Compras) lo invoca al elegir una
        // persona a traves de window.actualizarAccesoHistorialPreciosCliente -- por eso se llama asi.
        function actualizarEstadoClienteFijo(idPersona) {
            idPersona = parseInt(idPersona, 10) || 0;
            var fijo = idPersona > 0 && idPersona !== (config.idConsumidorFinal || 0);
            var token = ++tokenAccesoHistorial;

            $('#razonSocial').prop('readonly', fijo).toggleClass('pos-cliente-fijo', fijo);
            $('#btnQuitarCliente').toggleClass('d-none', !fijo);

            // El historial arranca oculto; solo se ofrece con cliente fijo y si el servidor lo permite.
            var $btnHistorial = $('#btnHistorialPreciosCliente').addClass('d-none');
            if (!fijo || !window.api || !window.api.persona || !window.api.persona.obtener) return;

            $.get(window.api.persona.obtener, { id: idPersona })
                .done(function (resp) {
                    if (token !== tokenAccesoHistorial) return;
                    // historialPreciosOculto: cliente de cuenta corriente reservada y usuario sin permiso
                    // (el servidor lo vuelve a aplicar en Ventas/HistorialPreciosCliente).
                    if (resp && resp.ok && !resp.historialPreciosOculto) $btnHistorial.removeClass('d-none');
                });
        }
        window.actualizarAccesoHistorialPreciosCliente = actualizarEstadoClienteFijo;

        $('#btnQuitarCliente').on('click', function () {
            $('#idPersona').val('0');
            $('#razonSocial').val('');
            setClienteIdentificacionVisual('', '');
            actualizarEstadoClienteFijo(0);
            $('#razonSocial').trigger('focus');
        });

        // Historial de precios del cliente fijo (F8): el mismo modal de Ventas, con solapas
        // Compras | Presupuestos y un "Agregar" por producto (modo=expendio).
        var tokenHistorial = 0;

        function abrirHistorialPrecios() {
            var idPersona = idClienteFijo();
            if (!idPersona || $('#btnHistorialPreciosCliente').hasClass('d-none') || !config.urlHistorialPrecios) return;

            var token = ++tokenHistorial;
            var nombre = ($('#clienteRazonSocialValor').val() || $('#razonSocial').val() || '').toString().trim();
            $('#hpeClienteNombre').text(nombre);
            $('#contenedorHistorialPreciosExpendio').html('<div class="text-center text-muted py-4">Cargando...</div>');
            $('#modalHistorialPreciosExpendio').modal('show');

            $.get(config.urlHistorialPrecios, { idPersona: idPersona, modo: 'expendio' })
                .done(function (html) {
                    if (token === tokenHistorial) $('#contenedorHistorialPreciosExpendio').html(html);
                })
                .fail(function () {
                    if (token !== tokenHistorial) return;
                    $('#contenedorHistorialPreciosExpendio').html('<div class="alert alert-danger mb-0">No se pudo cargar el historial de precios.</div>');
                });
        }

        $('#btnHistorialPreciosCliente').on('click', abrirHistorialPrecios);
        // pos-help.js captura F8 y ejecuta este hook (solo si no hay otro modal abierto).
        window.posHotkeysHooks = window.posHotkeysHooks || {};
        window.posHotkeysHooks.F8 = abrirHistorialPrecios;

        // Al cerrar el modal el foco vuelve al codigo de producto, como con el buscador de clientes.
        $(document).on('hidden.bs.modal', '#modalHistorialPreciosExpendio', function () {
            $(document).trigger('pos:foco-codigo');
        });

        // "Agregar" de una fila del historial: mete el producto en el carrito con el precio nuevo y
        // la cantidad indicados. El servidor revalida producto, precio y cantidad al guardar
        // (FinalizarPOS); aca solo se evita agregar lineas obviamente invalidas.
        var CANT_MINIMA_HISTORIAL = 0.01;

        function agregarLineaDesdeHistorial($boton) {
            var $fila = $boton.closest('tr');
            var precio = parseMoney($fila.find('.js-hp-precio').val());
            var cantidad = parseDecimal($fila.find('.js-hp-cant').val());
            var producto = String($boton.data('producto') || '');
            var codigo = String($boton.data('codigo') || '').trim();

            if (!(precio > 0)) {
                firePosAlert({ icon: 'warning', title: 'Precio', text: 'Ingresá un precio mayor a cero.' });
                $fila.find('.js-hp-precio').trigger('focus').trigger('select');
                return;
            }
            if (!(cantidad >= CANT_MINIMA_HISTORIAL)) {
                firePosAlert({ icon: 'warning', title: 'Cantidad', text: 'La cantidad mínima es 0,010 kg.' });
                $fila.find('.js-hp-cant').trigger('focus').trigger('select');
                return;
            }

            var idCorte = parseInt($boton.data('idcorte'), 10) || 0;
            if (idCorte > 0) {
                insertarLineaHistorial($boton, $fila, idCorte, producto, codigo, cantidad, precio);
                return;
            }

            // Las filas de Compras no traen el id interno del producto (el servidor lo exige al
            // guardar): se resuelve por codigo con el mismo endpoint del buscador del POS, que de
            // paso confirma que el producto sigue existiendo. El boton se bloquea mientras tanto.
            $boton.prop('disabled', true);
            $.getJSON(config.urlBuscarProductoPos, { codigo: codigo })
                .done(function (encontrado) {
                    if (!encontrado || !(parseInt(encontrado.id, 10) > 0)) {
                        firePosAlert({ icon: 'warning', title: 'Producto', text: (encontrado && encontrado.message) || 'No se encontró el producto.' });
                        return;
                    }
                    insertarLineaHistorial($boton, $fila, parseInt(encontrado.id, 10), producto, codigo, cantidad, precio);
                })
                .fail(function () {
                    firePosAlert({ icon: 'error', title: 'Producto', text: 'No se pudo verificar el producto. Intentá de nuevo.' });
                })
                .always(function () {
                    $boton.prop('disabled', false);
                });
        }

        // Mete la linea ya validada en el carrito y marca la fila del historial como agregada.
        function insertarLineaHistorial($boton, $fila, idCorte, producto, codigo, cantidad, precio) {
            var subtotal = cantidad * precio;
            POSState.addLinea({
                index: POSState.nextIndex(),
                idCorte: idCorte,
                producto: producto,
                descripcion: producto,
                codigo: codigo,
                cant: cantidad.toFixed(3),
                precio: '$ ' + precio.toFixed(2),
                precioOriginal: '$ ' + precio.toFixed(2),
                subtotal: '$ ' + subtotal.toFixed(2),
                bonificacion: 0,
                anulado: false,
                indexAnulado: -1,
                balanza: false,
                // Los expendios siempre se cargan por peso (igual que al cargar un expendio en Ventas).
                pesable: true,
                esHistorica: false,
                formaPagoAplicada: ''
            });

            posCart.renderTable(POSState.getLineas());
            posCart.recalculateTotal();
            posCart.updateSaleState();
            beep();

            // Feedback en la fila: queda marcada y el boton muestra un tilde un instante.
            $fila.addClass('hpe-agregado');
            var $icono = $boton.find('i').removeClass('fa-plus').addClass('fa-check');
            setTimeout(function () { $icono.removeClass('fa-check').addClass('fa-plus'); }, 1200);
        }

        $(document).on('click', '#modalHistorialPreciosExpendio .js-hp-agregar', function () {
            var $boton = $(this);
            var codigo = String($boton.data('codigo') || '').trim();
            var yaEnCarrito = codigo && POSState.getLineas().some(function (linea) {
                return linea && !linea.anulado && String(linea.codigo == null ? '' : linea.codigo).trim() === codigo;
            });

            if (!yaEnCarrito) {
                agregarLineaDesdeHistorial($boton);
                return;
            }

            // Ya esta cargado: se avisa en vez de duplicar en silencio.
            Swal.fire({
                icon: 'question',
                title: 'Producto ya cargado',
                text: String($boton.data('producto') || 'Este producto') + ' ya está en el carrito. ¿Agregarlo otra vez?',
                showCancelButton: true,
                confirmButtonText: 'Sí, agregar',
                cancelButtonText: 'No'
            }).then(function (resultado) {
                if (resultado.isConfirmed) agregarLineaDesdeHistorial($boton);
            });
        });

        // Enter en precio o cantidad = Agregar esa fila.
        $(document).on('keydown', '#modalHistorialPreciosExpendio .js-hp-precio, #modalHistorialPreciosExpendio .js-hp-cant', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            $(this).closest('tr').find('.js-hp-agregar').trigger('click');
        });

        // ===== Dias de validez del presupuesto (solo sector PRESUPUESTO, 2026-10-04) =====
        // El usuario elige DIAS (vacio = por defecto, 90); el servidor calcula la fecha de caducidad
        // = dia de vigencia + dias. Aca "Caduca el ..." es solo informativo.
        function diasCaducidadValidos() {
            var $dias = $('#diasCaducidadExpendio');
            if (!$dias.length) return true;

            var texto = String($dias.val() || '').trim();
            if (!texto) return true;            // vacio = por defecto
            if (!/^\d+$/.test(texto)) return false;

            var dias = parseInt(texto, 10);
            var maximo = parseInt($dias.attr('max'), 10) || 365;
            return dias >= 1 && dias <= maximo;
        }

        // null = no aplica (otro sector) o vacio (el servidor usa el valor por defecto).
        function diasCaducidadParaPayload() {
            var $dias = $('#diasCaducidadExpendio');
            if (!esSectorPresupuesto() || !$dias.length) return null;

            var texto = String($dias.val() || '').trim();
            return texto ? (parseInt(texto, 10) || null) : null;
        }

        function actualizarCaducaEl() {
            var $dias = $('#diasCaducidadExpendio');
            var $texto = $('#caducaElExpendio');
            if (!$dias.length || !$texto.length) return;

            if (!diasCaducidadValidos()) {
                $texto.addClass('text-danger').text('Entre 1 y ' + ($dias.attr('max') || 365));
                return;
            }

            var texto = String($dias.val() || '').trim();
            var dias = texto ? parseInt(texto, 10) : (parseInt($dias.data('dias-defecto'), 10) || 90);
            var vigencia = new Date($('#fechaExpendio').val() || new Date());
            var caduca = new Date(vigencia.getFullYear(), vigencia.getMonth(), vigencia.getDate() + dias);

            $texto.removeClass('text-danger')
                .text('Caduca el ' + pad(caduca.getDate()) + '/' + pad(caduca.getMonth() + 1) + '/' + caduca.getFullYear());
        }

        $('#diasCaducidadExpendio').on('input change', actualizarCaducaEl);
        actualizarCaducaEl();

        function actualizarSectorUI(sector) {
            var texto = sector || 'Sin seleccionar';
            $('#sectorPuntoExpendio').val(sector || '');
            $('#estadoSectorPuntoExpendioFooter').text(texto);
            $('#btnFinalizar').prop('disabled', !sector);
        }

        function openSectorModal() {
            $('#modalSectoresPuntoExpendio').modal('show');
            setTimeout(function () {
                $('#txtBuscarSectorExpendio').focus().select();
            }, 50);
        }

        $('#txtBuscarSectorExpendio').on('input', function () {
            var texto = ($(this).val() || '').toLowerCase();
            $('#listaSectoresExpendio .js-sector-item').each(function () {
                var $item = $(this);
                $item.toggle(($item.text() || '').toLowerCase().indexOf(texto) >= 0);
            });
        });

        // Siempre incluye posInstanceId al navegar de vuelta a POS -- mismo motivo que en Ventas/
        // POS.cshtml: si se arma la URL sin el, pos-multi-instance.js genera uno nuevo y pierde
        // el registro de "instancia ya abierta" contra localStorage.
        function urlPosConSector(sector) {
            var params = [];
            if (sector) params.push('sector=' + encodeURIComponent(sector));
            if (config.posInstanceId) params.push('posInstanceId=' + encodeURIComponent(config.posInstanceId));
            return config.urlPos + (params.length ? ('?' + params.join('&')) : '');
        }

        $(document).on('click', '.js-sector-item', function () {
            var sector = $(this).data('sector');
            if (!sector) return;
            window.location.href = urlPosConSector(sector);
        });

        // Bug real (2026-09-09, ver docs/DECISIONS.md "Batch B: permisos reales + operador de
        // produccion"): si la sesion es la cuenta de produccion sin operador todavia resuelto, el
        // <script> inline de POS.cshtml (que corre ANTES que este archivo) ya disparo
        // pedirOperador(true) -- abriendo #modalSeleccionUsuario. Si aca se llamaba
        // openSectorModal() igual, los dos modales de Bootstrap 5 competian por el backdrop casi
        // al mismo tiempo (dos ".modal('show')" en la misma carga de pagina): el backdrop
        // "static" del modal de sector terminaba tapando los inputs del modal de seleccion de
        // usuario -- "no se puede escribir para buscar usuario ni contraseña". Con un operador
        // pendiente de resolver, la eleccion de sector se salta: al elegir el operador la pagina
        // recarga entera (recargarConOperadorAutorizado(), ver POS.cshtml) y esta misma logica
        // corre de nuevo, ya sin "requiereOperadorPOS" -- ahi si abre el modal de sector solo.
        var operadorPendiente = window.PosOperadorConfig && window.PosOperadorConfig.requiereOperadorPOS;
        if (!operadorPendiente) {
            if (!config.sectorSeleccionado) {
                openSectorModal();
            } else {
                actualizarSectorUI(config.sectorSeleccionado);
            }
        }

        function construirPayload() {
            var lineas = POSState.getLineas()
                .filter(function (linea) { return linea && !linea.anulado; })
                .map(function (linea) {
                    return {
                        IdCorte: parseInt(linea.idCorte || 0, 10) || 0,
                        Codigo: parseInt(linea.codigo || 0, 10) || 0,
                        Descripcion: linea.descripcion || linea.producto || '',
                        CantKg: parseDecimal(linea.cant),
                        PrecioKg: parseMoney(linea.precio),
                        Importe: parseMoney(linea.subtotal),
                        Estado: 0,
                        Balanza: linea.balanza === true
                    };
                });

            return {
                FechaExpendio: $('#fechaExpendio').val(),
                Sector: $('#sectorPuntoExpendio').val(),
                IdentificacionCliente: ($('#razonSocial').val() || '').trim(),
                // Cliente real fijado (0 = manual) y dias de validez del presupuesto (null = por defecto).
                // El servidor valida ambos (PuntosExpendioController.FinalizarPOS).
                IdPersona: idClienteFijo(),
                DiasCaducidad: diasCaducidadParaPayload(),
                Observaciones: POSState.getObservaciones(),
                // Solo sector REMITOS (existe #nroRemito); en los demas sectores no se envia nada.
                NroRemito: ($('#nroRemito').val() || '').trim(),
                LineasVenta: lineas,
                PosInstanceId: config.posInstanceId || ''
            };
        }

        // ===== Post-guardado basico (PDF + email, ver _ModalPostPuntoExpendioBasico.cshtml) =====
        // Expendio ya guardado en esta pantalla ({ resp, firma }) mientras la vista sigue cargada
        // (el usuario cerro el modal con Escape). Finalizar de nuevo reabre el modal en vez de
        // guardar otra vez (evita duplicados) -- ver docs/DECISIONS.md 2026-09-30.
        // { resp, firma, modificando }: modificando=true tras confirmar "Si, modificar" -> el
        // proximo Finalizar actualiza ese mismo expendio (IdExpendioModificar) en vez de crear otro.
        var expendioGuardado = null;
        // true cuando el usuario eligio una opcion del modal (cierra el flujo); si el modal se
        // cierra sin eso (Escape) se pregunta si quiere modificar el expendio ya generado.
        var ppebOpcionElegida = false;
        // true si el modal se abrio desde "Mis expendios" (solo reimpresion): ahi las opciones no
        // limpian la vista actual, que puede tener un expendio en curso.
        var modalPostDesdeHistorial = false;

        // Tamaño de ticket recordado: misma clave que el post-venta de Ventas (Ventas/POS.cshtml,
        // pvbObtenerMedidaTicket) -- es la misma impresora termica del puesto.
        var KEY_TICKET_MM = 'postventa_ticket_mm';
        function ppebObtenerMedidaTicket() {
            var guardado = parseInt(localStorage.getItem(KEY_TICKET_MM), 10);
            return (guardado === 58 || guardado === 80) ? guardado : null;
        }
        function ppebActualizarTextoMedida() {
            $('#ppebTicketMedidaTexto').text((ppebObtenerMedidaTicket() || 80) + 'mm');
        }
        function ppebElegirMedidaTicket() {
            return Swal.fire({
                title: 'Tamaño de ticket',
                input: 'select',
                inputOptions: { 80: '80 mm', 58: '58 mm' },
                inputValue: ppebObtenerMedidaTicket() || 80,
                showCancelButton: true,
                confirmButtonText: 'Usar este tamaño',
                cancelButtonText: 'Cancelar'
            }).then(function (result) {
                if (!result.isConfirmed) return null;
                var mm = parseInt(result.value, 10);
                try { localStorage.setItem(KEY_TICKET_MM, String(mm)); } catch (e) { /* localStorage no disponible: se vuelve a preguntar */ }
                ppebActualizarTextoMedida();
                return mm;
            });
        }

        // Vuelve a un expendio en blanco. PosNavegandoInternoPOS evita que el pagehide mande el
        // sendBeacon de CerrarOperadorPOS (es navegacion interna, mismo criterio que Ventas).
        function ppebNuevoExpendio() {
            ppebOpcionElegida = true;
            $('#modalPostPuntoExpendioBasico').modal('hide');
            window.PosNavegandoInternoPOS = true;
            window.location.href = urlPosConSector($('#sectorPuntoExpendio').val());
        }

        // Fin de una opcion (ticket / PDF / email): desde el guardado limpia la vista; desde
        // "Mis expendios" solo cierra el modal.
        function ppebTerminarOpcion() {
            if (modalPostDesdeHistorial) {
                $('#modalPostPuntoExpendioBasico').modal('hide');
                return;
            }
            ppebNuevoExpendio();
        }

        // Escape (o cualquier cierre sin elegir opcion) del modal recien guardado: el expendio ya
        // existe, asi que antes de dejar la pantalla editable se pide confirmacion. "No" vuelve al
        // modal con las opciones de impresion; "Si, modificar" deja editar y el proximo Finalizar
        // actualiza el mismo expendio.
        $('#modalPostPuntoExpendioBasico').on('hidden.bs.modal', function () {
            if (ppebOpcionElegida || modalPostDesdeHistorial || !expendioGuardado) return;
            // Un modal hijo abierto (email) no cuenta: este evento solo sale del modal principal.
            Swal.fire({
                icon: 'warning',
                title: 'Modificar expendio',
                text: '¿Está seguro que quiere modificar el expendio #' + expendioGuardado.resp.idExpendio + ' ya generado?',
                showCancelButton: true,
                confirmButtonText: 'Sí, modificar',
                cancelButtonText: 'No',
                allowEscapeKey: false,
                allowOutsideClick: false
            }).then(function (result) {
                if (result.isConfirmed) {
                    expendioGuardado.modificando = true;
                    posKeyboard.focusCodigo();
                } else {
                    mostrarModalPostExpendio(expendioGuardado.resp);
                }
            });
        });

        function mostrarModalPostExpendio(resp, desdeHistorial) {
            // Sin beep al finalizar (2026-09-06, pedido explicito del usuario -- ver
            // docs/DECISIONS.md): se confundia con el beep de "producto agregado al carrito",
            // que sigue sonando igual que siempre. Se saca SOLO este beep puntual.
            modalPostDesdeHistorial = desdeHistorial === true;
            ppebOpcionElegida = false;
            ppebActualizarTextoMedida();
            $('#ppebResumen').text('Expendio #' + resp.idExpendio + ' registrado correctamente.');
            $('#btnPpebPdf').attr('href', resp.pdfUrl);
            $('#btnPpebPdf, #btnPpebEmail, #btnPpebTicket').data('id-expendio', resp.idExpendio);

            // Presupuesto: ademas del PDF completo se ofrece la lista de precios (sin cantidades
            // ni totales) y, en el email, elegir cual se adjunta. Otros sectores no cambian.
            var esPresupuesto = esSectorPresupuesto();
            var separador = (resp.pdfUrl || '').indexOf('?') >= 0 ? '&' : '?';
            $('#btnPpebPdfPrecios')
                .attr('href', (resp.pdfUrl || '') + separador + 'formato=precios')
                .toggleClass('d-none', !esPresupuesto);
            $('#ppebEmailFormatoWrap').toggleClass('d-none', !esPresupuesto);
            $('#ppebEmailFormato').val('completo');
            $('#modalPostPuntoExpendioBasico').modal('show');
        }

        // 1 = Nuevo expendio.
        $('#btnPpebContinuar').on('click', ppebNuevoExpendio);

        // 2 = Ticket termico. Agente local si esta instalado; si no, dialogo del navegador via
        // iframe oculto (ticket-print.js, el mismo helper que usa el post-venta de Ventas).
        $('#btnPpebTicket').on('click', function () {
            var idExpendio = $(this).data('id-expendio');

            function abrirTicket(mm) {
                window.TicketPrint.imprimir({
                    ticketUrl: config.urlTicketHtml + '?id=' + idExpendio + '&mm=' + mm,
                    payloadUrl: config.urlTicketPayload + '?id=' + idExpendio + '&mm=' + mm,
                    // El retraso deja que el spool del navegador/agente termine antes de recargar.
                    onDone: function () { setTimeout(ppebTerminarOpcion, 400); }
                });
            }

            var mmRecordado = ppebObtenerMedidaTicket();
            if (mmRecordado) {
                abrirTicket(mmRecordado);
            } else {
                ppebElegirMedidaTicket().then(function (mm) {
                    if (mm) abrirTicket(mm);
                });
            }
        });

        $('#ppebTicketCambiarMedida').on('click', function (e) {
            e.preventDefault();
            ppebElegirMedidaTicket();
        });

        // 3 y 5 = PDF (completo / lista de precios): son enlaces target=_blank, el navegador abre
        // la pestaña y aca solo se cierra el flujo.
        $('#btnPpebPdf, #btnPpebPdfPrecios').on('click', function () {
            setTimeout(ppebTerminarOpcion, 400);
        });

        // Atajos numericos en el modal de "Expendio guardado" (pedido del usuario 2026-09-12 y
        // ampliado 2026-09-30, mismo patron que el post-venta de Ventas): 1 Nuevo expendio,
        // 2 Ticket, 3 PDF, 4 Email, 5 PDF lista de precios (solo PRESUPUESTO, boton oculto en el
        // resto). Escape no tiene atajo propio: cierra el modal y deja la vista como esta.
        $(document).on('keydown', function (e) {
            if (!$('#modalPostPuntoExpendioBasico').hasClass('show')) return;
            // Con el modal de email encima, los numeros son para escribir, no atajos.
            if ($('#modalEmailPostPuntoExpendio').hasClass('show')) return;
            var tag = (e.target.tagName || '').toUpperCase();
            if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return;

            var mapaAtajos = {
                '1': '#btnPpebContinuar',
                '2': '#btnPpebTicket',
                '3': '#btnPpebPdf',
                '4': '#btnPpebEmail',
                '5': '#btnPpebPdfPrecios'
            };
            var selector = mapaAtajos[e.key];
            if (!selector) return;

            e.preventDefault();
            var boton = $(selector).not('.d-none')[0];
            // click nativo: para los <a target=_blank> jQuery.trigger no abre el enlace.
            if (boton) boton.click();
        });

        $('#btnPpebEmail').on('click', function () {
            var idExpendio = $(this).data('id-expendio');
            $('#ppebEmailError').addClass('d-none').text('');

            $.getJSON(config.urlEmailConfig, { idExpendio: idExpendio }).done(function (resp) {
                if (!resp || !resp.ok) {
                    Swal.fire({ icon: 'error', title: 'No se pudo preparar el email', text: (resp && resp.msg) || 'Error desconocido.' });
                    return;
                }

                $('#ppebEmailDestino').val(resp.email || '');
                $('#ppebEmailAsunto').val(resp.asunto || '');
                $('#ppebEmailMensaje').val(resp.mensaje || '');
                $('#modalEmailPostPuntoExpendio').modal('show');
            }).fail(function () {
                Swal.fire({ icon: 'error', title: 'No se pudo preparar el email', text: 'Error de conexión.' });
            });
        });

        $('#btnPpebConfirmarEmail').on('click', function () {
            var idExpendio = $('#btnPpebEmail').data('id-expendio');
            var email = ($('#ppebEmailDestino').val() || '').trim();
            var asunto = ($('#ppebEmailAsunto').val() || '').trim();
            var mensaje = ($('#ppebEmailMensaje').val() || '').trim();

            if (!email) {
                $('#ppebEmailError').removeClass('d-none').text('Ingrese un email destino.');
                return;
            }

            var $btn = $(this);
            $btn.prop('disabled', true);

            $.post(config.urlEmailEnviar, {
                idExpendio: idExpendio,
                emailDestino: email,
                asunto: asunto,
                mensaje: mensaje,
                formato: esSectorPresupuesto() ? ($('#ppebEmailFormato').val() || 'completo') : 'completo'
            }).done(function (resp) {
                $btn.prop('disabled', false);

                if (!resp || !resp.ok) {
                    $('#ppebEmailError').removeClass('d-none').text((resp && resp.msg) || 'No se pudo enviar el email.');
                    return;
                }

                $('#modalEmailPostPuntoExpendio').modal('hide');
                // Email enviado: cierra el flujo (limpia la vista salvo desde "Mis expendios").
                Swal.fire({ icon: 'success', title: 'Email enviado', text: 'El comprobante se envió correctamente.', timer: 1500, showConfirmButton: false })
                    .then(ppebTerminarOpcion);
            }).fail(function (xhr) {
                $btn.prop('disabled', false);
                $('#ppebEmailError').removeClass('d-none').text((xhr.responseJSON && xhr.responseJSON.msg) || 'No se pudo enviar el email.');
            });
        });

        var guardando = false;

        $('#btnFinalizar').on('click', function () {
            if (guardando) return;

            var payload = construirPayload();

            // Expendio ya guardado en esta pantalla: nunca se crea otro. Sin cambios, o sin haber
            // confirmado "Si, modificar", solo se reabre el modal de opciones; con la modificacion
            // confirmada y cambios reales, mas abajo se manda IdExpendioModificar para actualizarlo.
            if (expendioGuardado && (!expendioGuardado.modificando || JSON.stringify(payload) === expendioGuardado.firma)) {
                mostrarModalPostExpendio(expendioGuardado.resp);
                return;
            }
            if (expendioGuardado) payload.IdExpendioModificar = expendioGuardado.resp.idExpendio;

            if (!payload.Sector) {
                firePosAlert({ icon: 'warning', title: 'Sector', text: 'Debe seleccionar un sector.' });
                openSectorModal();
                return;
            }

            if (!payload.LineasVenta.length) {
                firePosAlert({ icon: 'warning', title: 'Punto de expendio', text: 'Debe agregar al menos un producto.' });
                posKeyboard.focusCodigo();
                return;
            }

            // REMITOS: el numero de remito es obligatorio (el servidor lo vuelve a validar).
            if (esSectorRemitos() && !payload.NroRemito) {
                firePosAlert({ icon: 'warning', title: 'Remito', text: 'Debe indicar el número de remito.' });
                $('#nroRemito').trigger('focus');
                return;
            }

            // PRESUPUESTO: dias de validez entre 1 y el maximo (vacio = por defecto). El servidor lo valida igual.
            if (esSectorPresupuesto() && !diasCaducidadValidos()) {
                firePosAlert({ icon: 'warning', title: 'Presupuesto', text: 'Los días de validez deben estar entre 1 y ' + ($('#diasCaducidadExpendio').attr('max') || 365) + '.' });
                $('#diasCaducidadExpendio').trigger('focus');
                return;
            }

            guardando = true;
            $('#btnFinalizar').prop('disabled', true);

            $.ajax({
                url: config.urlFinalizar,
                type: 'POST',
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(payload)
            })
                .done(function (resp) {
                    if (!resp || !resp.ok) {
                        // Numero de remito repetido: el servidor devuelve el siguiente libre y se carga en el campo.
                        if (resp && resp.nroRemitoSugerido && $('#nroRemito').length) $('#nroRemito').val(resp.nroRemitoSugerido);
                        firePosAlert({
                            icon: 'error',
                            title: 'Punto de Expendio',
                            text: resp && resp.mensaje ? resp.mensaje : 'No se pudo guardar el punto de expendio.'
                        });
                        return;
                    }

                    // La firma se calcula sin IdExpendioModificar: compara solo el contenido de la pantalla.
                    var payloadFirma = $.extend({}, payload);
                    delete payloadFirma.IdExpendioModificar;
                    expendioGuardado = { resp: resp, firma: JSON.stringify(payloadFirma), modificando: false };
                    mostrarModalPostExpendio(resp);
                })
                .fail(function () {
                    firePosAlert({
                        icon: 'error',
                        title: 'Punto de Expendio',
                        text: 'No se pudo guardar el punto de expendio.'
                    });
                })
                .always(function () {
                    guardando = false;
                    $('#btnFinalizar').prop('disabled', false);
                });
        });

        $('#btnCancelarItem').on('click', function () {
            var lineas = POSState.getLineas().filter(function (linea) { return !!linea; });
            if (!lineas.length) {
                window.location.href = urlPosConSector($('#sectorPuntoExpendio').val());
                return;
            }

            Swal.fire({
                icon: 'warning',
                title: 'Cancelar registro',
                text: 'Se quitarán las líneas cargadas en este punto de expendio.',
                showCancelButton: true,
                confirmButtonText: 'Sí, cancelar',
                cancelButtonText: 'No'
            }).then(function (result) {
                if (!result.isConfirmed) return;
                expendioGuardado = null;   // pantalla en blanco: el proximo Finalizar guarda un expendio nuevo
                POSState.clear();
                posCart.renderTable(POSState.getLineas());
                posCart.recalculateTotal();
                posCart.updateSaleState();
                posComment.updateButtonState();
                posProduct.showWaiting();
                posKeyboard.focusCodigo();
            });
        });

        // ===== "Mis expendios" (F6, 2026-09-06, retomado -- ver docs/DECISIONS.md). Port literal
        // de Web/Scripts/app/punto-expendio-pos.js:199-227,229-450,564-627 -- unica diferencia
        // real: el boton "Imprimir" reusa mostrarModalPostExpendio (PDF+email, ya portado) en vez
        // de window.PostPuntoExpendioModal (ticket ESC/POS, modal-postexpendio.js, no portado). =====
        function formatKg(value) {
            return Number(value || 0).toLocaleString('es-AR', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
        }

        function formatMoney(value) {
            return '$ ' + Number(value || 0).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        }

        function escapeHtml(value) {
            return String(value == null ? '' : value)
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;')
                .replace(/'/g, '&#39;');
        }

        function normalizeText(value) {
            return String(value == null ? '' : value).toLowerCase().replace(/\s+/g, ' ').trim();
        }

        function setMisExpendiosDateHint(visible) {
            $('#msgFiltroFechaMisExpendios').toggleClass('d-none', !visible);
        }

        function ensureMisExpendiosDefaultDates() {
            var hoy = formatDateInput(new Date());
            var $fechaDesde = $('#filtroMisExpendiosFechaDesde');
            var $fechaHasta = $('#filtroMisExpendiosFechaHasta');
            if (!$fechaDesde.val()) $fechaDesde.val(hoy);
            if (!$fechaHasta.val()) $fechaHasta.val(hoy);
        }

        function populateMisExpendiosSelects(items) {
            var estadoActual = $('#filtroMisExpendiosEstado').val() || '';
            var sucursalActual = $('#filtroMisExpendiosSucursal').val() || '';
            var estados = {};
            var sucursales = {};

            (items || []).forEach(function (item) {
                var estado = String(item && item.estado ? item.estado : '').trim();
                var sucursal = String(item && item.sucursal ? item.sucursal : '').trim();
                if (estado) estados[estado] = true;
                if (sucursal) sucursales[sucursal] = true;
            });

            var estadoOptions = ['<option value="">Todos</option>'];
            Object.keys(estados).sort().forEach(function (estado) {
                estadoOptions.push('<option value="' + escapeHtml(estado) + '">' + escapeHtml(estado) + '</option>');
            });
            $('#filtroMisExpendiosEstado').html(estadoOptions.join('')).val(estadoActual);
            if ($('#filtroMisExpendiosEstado').val() !== estadoActual) $('#filtroMisExpendiosEstado').val('');

            var sucursalOptions = ['<option value="">Todas</option>'];
            Object.keys(sucursales).sort().forEach(function (sucursal) {
                sucursalOptions.push('<option value="' + escapeHtml(sucursal) + '">' + escapeHtml(sucursal) + '</option>');
            });
            $('#filtroMisExpendiosSucursal').html(sucursalOptions.join('')).val(sucursalActual);
            if ($('#filtroMisExpendiosSucursal').val() !== sucursalActual) $('#filtroMisExpendiosSucursal').val('');
        }

        function getMisExpendiosFilteredItems() {
            var cliente = normalizeText($('#filtroMisExpendiosCliente').val());
            var producto = normalizeText($('#filtroMisExpendiosProducto').val());
            var sucursal = normalizeText($('#filtroMisExpendiosSucursal').val());
            var estado = normalizeText($('#filtroMisExpendiosEstado').val());

            return (misExpendiosCache || []).filter(function (item) {
                // El texto del filtro busca por identificacion del cliente O por numero de remito.
                var clienteTexto = normalizeText((item.identificacionExpendio || '') + ' ' + (item.nroRemito || ''));
                var sucursalTexto = normalizeText(item.sucursal);
                var estadoTexto = normalizeText(item.estado);
                var coincideProducto = !producto || (item.lineas || []).some(function (linea) {
                    return normalizeText(linea.producto).indexOf(producto) >= 0;
                });

                if (cliente && clienteTexto.indexOf(cliente) < 0) return false;
                if (sucursal && sucursalTexto !== sucursal) return false;
                if (estado && estadoTexto !== estado) return false;
                if (!coincideProducto) return false;
                return true;
            });
        }

        function renderDetalleMisExpendios(lineas) {
            if (!lineas || !lineas.length) {
                return '<div class="small text-muted">Sin detalle de líneas.</div>';
            }

            var html = '<div class="table-responsive"><table class="table table-sm table-borderless mb-0"><thead><tr>'
                + '<th style="width:90px;">Código</th>'
                + '<th>Producto</th>'
                + '<th style="width:95px;" class="text-right">Kgs.</th>'
                + '<th style="width:110px;" class="text-right">Precio</th>'
                + '<th style="width:110px;" class="text-right">Total</th>'
                + '</tr></thead><tbody>';

            lineas.forEach(function (linea) {
                html += '<tr>'
                    + '<td>' + escapeHtml(linea.codigo || 0) + '</td>'
                    + '<td>' + escapeHtml(linea.producto || '') + '</td>'
                    + '<td class="text-right">' + formatKg(linea.cantKg) + '</td>'
                    + '<td class="text-right">' + formatMoney(linea.precioKg) + '</td>'
                    + '<td class="text-right">' + formatMoney(linea.total) + '</td>'
                    + '</tr>';
            });

            html += '</tbody></table></div>';
            return html;
        }

        function showMisExpendiosMessage(type, text) {
            var $msg = $('#msgMisExpendiosPuntoExpendio');
            if (!$msg.length) return;

            if (!text) {
                $msg.addClass('d-none').removeClass('alert-info alert-warning alert-danger alert-success').text('');
                return;
            }

            var css = { info: 'alert-info', warning: 'alert-warning', danger: 'alert-danger', success: 'alert-success' }[type || 'info'] || 'alert-info';
            $msg.removeClass('d-none alert-info alert-warning alert-danger alert-success').addClass(css).text(text);
        }

        function renderMisExpendios(items) {
            var $tbody = $('#tablaMisExpendiosPuntoExpendio tbody');
            if (!$tbody.length) return;

            $tbody.empty();

            if (!items || !items.length) {
                $tbody.append('<tr><td colspan="9" class="text-center text-muted py-4">No hay expendios para mostrar.</td></tr>');
                return;
            }

            items.forEach(function (item) {
                var estadoClass = item.estado === 'Asignado' ? 'badge-secondary' : 'badge-warning';

                $tbody.append(
                    '<tr>' +
                    '<td>' + (item.fecha || '') + '</td>' +
                    '<td>' + (item.hora || '') + '</td>' +
                    '<td><strong>' + (item.idExpendio || 0) + '</strong></td>' +
                    '<td>' + (item.identificacionExpendio || '') + (item.nroRemito ? ' <span class="badge badge-secondary">Remito ' + item.nroRemito + '</span>' : '') + '</td>' +
                    '<td><span class="badge ' + estadoClass + '">' + (item.estado || '') + '</span></td>' +
                    '<td class="text-right">' + (item.cantItems || '0') + '</td>' +
                    '<td class="text-right">' + formatKg(item.totalKg) + '</td>' +
                    '<td class="text-right">' + formatMoney(item.totalImporte) + '</td>' +
                    '<td class="text-center">' +
                    '<button type="button" class="btn btn-sm btn-outline-primary btnImprimirMisExpendio" data-id-expendio="' + (item.idExpendio || 0) + '">Imprimir</button>' +
                    '</td>' +
                    '</tr>' +
                    '<tr class="bg-light">' +
                    '<td colspan="9">' +
                    '<div class="small font-weight-bold text-muted mb-1">Detalle</div>' +
                    renderDetalleMisExpendios(item.lineas || []) +
                    '</td>' +
                    '</tr>'
                );
            });
        }

        function applyMisExpendiosFilters() {
            renderMisExpendios(getMisExpendiosFilteredItems());
        }

        function loadMisExpendios() {
            var fechaDesde = $('#filtroMisExpendiosFechaDesde').val() || '';
            var fechaHasta = $('#filtroMisExpendiosFechaHasta').val() || '';

            if (fechaDesde && fechaHasta && fechaDesde > fechaHasta) {
                showMisExpendiosMessage('warning', 'La fecha desde no puede ser mayor a la fecha hasta.');
                return;
            }

            misExpendiosFetchState.fechaDesde = fechaDesde;
            misExpendiosFetchState.fechaHasta = fechaHasta;
            showMisExpendiosMessage(null, '');
            renderMisExpendios([]);
            $('#tablaMisExpendiosPuntoExpendio tbody').html('<tr><td colspan="9" class="text-center text-muted py-4">Consultando expendios...</td></tr>');
            setMisExpendiosDateHint(false);

            $.ajax({
                url: config.urlMisExpendios,
                type: 'GET',
                data: { fechaDesde: fechaDesde, fechaHasta: fechaHasta, posInstanceId: config.posInstanceId || '' },
                dataType: 'json',
                cache: false
            })
                .done(function (resp) {
                    if (!resp || resp.ok === false) {
                        misExpendiosCache = [];
                        renderMisExpendios([]);
                        showMisExpendiosMessage('warning', resp && resp.mensaje ? resp.mensaje : 'No se pudieron consultar los expendios.');
                        return;
                    }

                    misExpendiosCache = resp.items || [];
                    populateMisExpendiosSelects(misExpendiosCache);
                    applyMisExpendiosFilters();
                    if (!misExpendiosCache.length) {
                        showMisExpendiosMessage('info', 'No hay expendios para el rango de fechas seleccionado.');
                    }
                })
                .fail(function () {
                    misExpendiosCache = [];
                    renderMisExpendios([]);
                    showMisExpendiosMessage('danger', 'No se pudieron consultar los expendios.');
                });
        }

        function abrirMisExpendios() {
            if ($('.modal.show').not('#modalAyudaPOS').length && !$('#modalAyudaPOS').hasClass('show')) return;

            ensureMisExpendiosDefaultDates();
            $('#modalMisExpendiosPuntoExpendio').modal('show');
            loadMisExpendios();
        }

        $('#btnBuscarMisExpendiosPuntoExpendio').on('click', function () {
            loadMisExpendios();
        });

        $('#filtroMisExpendiosFechaDesde, #filtroMisExpendiosFechaHasta').on('change', function () {
            var cambioPendiente = misExpendiosFetchState.fechaDesde !== ($('#filtroMisExpendiosFechaDesde').val() || '') ||
                misExpendiosFetchState.fechaHasta !== ($('#filtroMisExpendiosFechaHasta').val() || '');
            setMisExpendiosDateHint(cambioPendiente);
        });

        $('#filtroMisExpendiosCliente, #filtroMisExpendiosProducto').on('input', function () {
            applyMisExpendiosFilters();
        });

        $('#filtroMisExpendiosSucursal, #filtroMisExpendiosEstado').on('change', function () {
            applyMisExpendiosFilters();
        });

        $(document).on('click', '.btnImprimirMisExpendio', function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();

            var idExpendio = parseInt($(this).data('id-expendio') || 0, 10) || 0;
            if (idExpendio <= 0) return;

            var item = null;
            for (var i = 0; i < misExpendiosCache.length; i++) {
                if ((misExpendiosCache[i].idExpendio || 0) === idExpendio) {
                    item = misExpendiosCache[i];
                    break;
                }
            }

            if (!item) {
                Swal.fire({ icon: 'warning', title: 'Mis expendios', text: 'No se pudo recuperar la información del expendio seleccionado.' });
                return;
            }

            $('#modalMisExpendiosPuntoExpendio').modal('hide');
            mostrarModalPostExpendio({ idExpendio: item.idExpendio || idExpendio, pdfUrl: item.pdfUrl || '' }, true);
        });

        window.posHotkeysHooks = window.posHotkeysHooks || {};
        window.posHotkeysHooks.F6 = function () {
            abrirMisExpendios();
        };

        document.addEventListener('keydown', function (e) {
            if ($('.modal.show').length && !$('#modalAyudaPOS').hasClass('show')) return;

            if (e.key === 'End') {
                e.preventDefault();

                var $cliente = $('#razonSocial');
                var clienteVacio = !($cliente.val() || '').trim();
                var clienteTieneFoco = document.activeElement === $cliente.get(0);

                if (clienteVacio && !clienteTieneFoco) {
                    $cliente.trigger('focus');
                    return;
                }

                clickIfEnabled('#btnFinalizar');
                return;
            }

            if (e.key === 'Home') {
                e.preventDefault();
                posKeyboard.focusCodigo();
                return;
            }

            if (e.key === 'PageDown') {
                e.preventDefault();
                enfocarCliente();
                return;
            }

            if (e.key === 'F9') {
                e.preventDefault();
                clickIfEnabled('#btnBuscarPersona');
                return;
            }

            if (e.key === 'F10') {
                e.preventDefault();
                // Guard agregado 2026-09-06 (retomado, ver docs/DECISIONS.md): #btnAgregarManual
                // esta disabled (buscador avanzado real, modal-productos.js, no portado) -- sin
                // este chequeo, abrirBuscadorProductosPOS() SI existe (la define pos-product.js)
                // pero su dependencia interna (abrirBuscarProductoModal) no, y tira un error
                // visible al usuario. Mismo criterio que el guard ya usado para F9 arriba.
                var btnManual = document.getElementById('btnAgregarManual');
                if (btnManual && !btnManual.disabled && typeof window.abrirBuscadorProductosPOS === 'function') {
                    window.abrirBuscadorProductosPOS();
                }
            }
        });

        window.posHotkeysHooks = window.posHotkeysHooks || {};
        window.posHotkeysHooks.AvPag = function () {
            enfocarCliente();
        };
        // F6 (Mis expendios) NO se registra a proposito -- no portado en este MVP, ver header.
        window.posHotkeysHooks.F4 = function () {
            $('#tablaItems tr.fila-item').last().trigger('click');
        };

        setTimeout(function () {
            posKeyboard.focusCodigo();
        }, 50);
    });
})(window, window.jQuery, document);
