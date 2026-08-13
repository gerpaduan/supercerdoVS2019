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

    function formatDecimal(value, decimals) {
        return Number(value || 0).toLocaleString('es-AR', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        });
    }

    $(function () {
        var config = window.puntoExpendioPosConfig || {};
        if (!document.getElementById('pos-app')) return;
        var misExpendiosCache = [];
        var misExpendiosFetchState = {
            fechaDesde: '',
            fechaHasta: ''
        };

        window.buscarProductoUrl = config.urlBuscarProductoPos;
        window.api = window.api || {};
        window.api.persona = {
            listar: config.urlPersonaListar,
            buscar: config.urlPersonaBuscar,
            crear: config.urlPersonaCrear,
            guardarCrear: config.urlPersonaGuardarCrear
        };

        var POSState = window.POSState;
        POSState.clear();
        POSState.setFechaVenta(new Date());

        var fechaInput = document.getElementById('fechaExpendio');
        var fechaLabel = document.getElementById('fechaHoraPOSExpendio');

        function pad(value) {
            return String(value).padStart(2, '0');
        }

        function formatDateInput(fecha) {
            return fecha.getFullYear() + '-' + pad(fecha.getMonth() + 1) + '-' + pad(fecha.getDate());
        }

        function formatFechaSql(fecha) {
            return fecha.getFullYear() + '-' + pad(fecha.getMonth() + 1) + '-' + pad(fecha.getDate()) +
                'T' + pad(fecha.getHours()) + ':' + pad(fecha.getMinutes()) + ':' + pad(fecha.getSeconds());
        }

        function formatFechaVisible(fecha) {
            var dias = ['domingo', 'lunes', 'martes', 'miércoles', 'jueves', 'viernes', 'sábado'];
            var meses = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'];

            return dias[fecha.getDay()] + ' ' + fecha.getDate() + ' de ' + meses[fecha.getMonth()] + ', ' +
                fecha.getFullYear() + '. ' + pad(fecha.getHours()) + ':' + pad(fecha.getMinutes());
        }

        function actualizarFechaHora() {
            var lineas = POSState.getLineas().filter(function (linea) { return !!linea; });
            if (!(lineas.length === 0 || lineas.length === 1)) return;

            var ahora = new Date();
            POSState.setFechaVenta(ahora);

            if (fechaInput) {
                fechaInput.value = formatFechaSql(ahora);
            }

            if (fechaLabel) {
                fechaLabel.textContent = formatFechaVisible(ahora);
            }
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

        var scanner = new BarcodeScanner({
            videoSelector: '#videoScanner',
            containerSelector: '#scannerContainer',
            onCodeDetected: function (codigo) {
                document.querySelector('#inputCodigo').value = codigo;
                document.querySelector('#inputCodigo').focus();
                manejarEnter();
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

        function scrollPantallaMobile() {
            if (!window.matchMedia('(max-width: 576px)').matches) return;
            var anchor = document.getElementById('pos-bottom-anchor');
            if (anchor) anchor.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }

        function getPrecioParaAgregar(productoSeleccionado) {
            if (!config.permiteEditarPrecio) {
                return productoSeleccionado ? productoSeleccionado.precioKg : 0;
            }

            var precioManual = parseDecimal($('#inputPrecioManualExpendio').val());
            return precioManual > 0 ? precioManual : (productoSeleccionado ? productoSeleccionado.precioKg : 0);
        }

        function showConnectionError(msg) {
            Swal.fire({
                icon: 'error',
                title: 'Sin conexión',
                text: msg
            });
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
            if (button && !button.disabled) {
                button.click();
            }
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

        function formatKg(value) {
            return Number(value || 0).toLocaleString('es-AR', {
                minimumFractionDigits: 3,
                maximumFractionDigits: 3
            });
        }

        function formatMoney(value) {
            return '$ ' + Number(value || 0).toLocaleString('es-AR', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });
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
            return String(value == null ? '' : value)
                .toLowerCase()
                .replace(/\s+/g, ' ')
                .trim();
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
            if ($('#filtroMisExpendiosEstado').val() !== estadoActual) {
                $('#filtroMisExpendiosEstado').val('');
            }

            var sucursalOptions = ['<option value="">Todas</option>'];
            Object.keys(sucursales).sort().forEach(function (sucursal) {
                sucursalOptions.push('<option value="' + escapeHtml(sucursal) + '">' + escapeHtml(sucursal) + '</option>');
            });
            $('#filtroMisExpendiosSucursal').html(sucursalOptions.join('')).val(sucursalActual);
            if ($('#filtroMisExpendiosSucursal').val() !== sucursalActual) {
                $('#filtroMisExpendiosSucursal').val('');
            }
        }

        function getMisExpendiosFilteredItems() {
            var cliente = normalizeText($('#filtroMisExpendiosCliente').val());
            var producto = normalizeText($('#filtroMisExpendiosProducto').val());
            var sucursal = normalizeText($('#filtroMisExpendiosSucursal').val());
            var estado = normalizeText($('#filtroMisExpendiosEstado').val());

            return (misExpendiosCache || []).filter(function (item) {
                var clienteTexto = normalizeText(item.identificacionExpendio);
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
                $msg.addClass('d-none')
                    .removeClass('alert-info alert-warning alert-danger alert-success')
                    .text('');
                return;
            }

            var css = {
                info: 'alert-info',
                warning: 'alert-warning',
                danger: 'alert-danger',
                success: 'alert-success'
            }[type || 'info'] || 'alert-info';

            $msg.removeClass('d-none alert-info alert-warning alert-danger alert-success')
                .addClass(css)
                .text(text);
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
                    '<td>' + (item.identificacionExpendio || '') + '</td>' +
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
            var items = getMisExpendiosFilteredItems();
            renderMisExpendios(items);
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
                data: {
                    fechaDesde: fechaDesde,
                    fechaHasta: fechaHasta
                },
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

        $('#inputPrecioManualExpendio').on('input', function () {
            posCart.calculateSubtotal();
        });

        $(document).on('click', '#btnBuscarPersona', function () {
            var $btn = $(this);
            var $contenedor = $('#contenedorModalPersona');

            // Limpieza defensiva: si la ultima apertura de este modal fue desde una
            // compra embebida (ver compras.js), no debe arrastrarse esa marca aca.
            $('#modalBuscarPersona').removeData('origen-persona-buscar');

            if ($contenedor.find('#modalBuscarPersona').length) {
                $('#modalBuscarPersona').modal('show');
                cargarPersonas();
                return;
            }

            $btn.prop('disabled', true);
            $.get(window.api.persona.buscar)
                .done(function (html) {
                    $contenedor.html(html);
                    $('#modalBuscarPersona').modal('show');
                    cargarPersonas();
                })
                .fail(function () {
                    Swal.fire({
                        icon: 'error',
                        title: 'Cliente',
                        text: 'No se pudo cargar el buscador de clientes'
                    });
                })
                .always(function () {
                    $btn.prop('disabled', false);
                });
        });

        $('#btnConsumidorFinal').on('click', function () {
            $('#idPersona').val(config.idConsumidorFinal || 0);
            $('#razonSocial').val('Consumidor Final');
            setClienteIdentificacionVisual('', 'Consumidor Final');
        });

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
                Swal.fire({
                    icon: 'warning',
                    title: 'Mis expendios',
                    text: 'No se pudo recuperar la información del expendio seleccionado.'
                });
                return;
            }

            $('#modalMisExpendiosPuntoExpendio').modal('hide');

            window.PostPuntoExpendioModal.open({
                idExpendio: item.idExpendio || idExpendio,
                imprimirUrl: item.imprimirUrl || '',
                pdfUrl: item.pdfUrl || ''
            }, {
                returnModalSelector: '#modalMisExpendiosPuntoExpendio',
                titulo: 'Acciones del expendio',
                mensaje: 'Seleccione qué desea hacer con el expendio ' + idExpendio + '.'
            });
        });

        $(document).on('click', '.btnImprimirMisExpendio', function () {
            var idExpendio = parseInt($(this).data('id-expendio') || 0, 10) || 0;
            if (idExpendio <= 0) return;

            Swal.fire({
                icon: 'info',
                title: 'Impresión pendiente',
                text: 'La impresión específica del expendio ' + idExpendio + ' se conectará en el próximo paso.'
            });
        });

        $('#modalMisExpendiosPuntoExpendio').on('hidden.bs.modal', function () {
            setMisExpendiosDateHint(false);
            posKeyboard.focusCodigo();
        });

        $('#razonSocial').on('input', function () {
            $('#idPersona').val('0');
            setClienteIdentificacionVisual('', '');
        });

        // Mismo comportamiento que la tecla Fin cuando el campo ya tiene el
        // foco: Enter finaliza directo (no hace falta la rama de "vacio y sin
        // foco" del atajo Fin, porque si esto dispara es porque ya esta enfocado).
        $('#razonSocial').on('keydown', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            clickIfEnabled('#btnFinalizar');
        });

        // Muestra la identificacion del cliente debajo del input, igual que POS
        // Venta (Views/Ventas/POS.cshtml), solo cuando difiere de la razon social
        // (si son iguales o no hay identificacion, no aporta nada mostrarla).
        function setClienteIdentificacionVisual(identificacion, razonSocial) {
            var razon = (razonSocial || $('#razonSocial').val() || '').toString().trim();
            var identificacionNormalizada = (identificacion || '').toString().trim();
            var $wrap = $('#clienteIdentificacionWrap');
            var $texto = $('#clienteIdentificacionTexto');
            var $valor = $('#clienteIdentificacionValor');

            if (!$wrap.length || !$texto.length) return;

            var mostrar = !!identificacionNormalizada &&
                !!razon &&
                identificacionNormalizada.localeCompare(razon, undefined, { sensitivity: 'accent' }) !== 0;

            if ($valor.length) {
                $valor.val(identificacionNormalizada);
            }

            $texto.text(identificacionNormalizada);
            $wrap.attr('title', identificacionNormalizada);
            $wrap.toggleClass('d-none', !mostrar);
        }
        window.setClienteIdentificacionVisual = setClienteIdentificacionVisual;

        function actualizarSectorUI(sector) {
            var texto = sector || 'Sin seleccionar';
            $('#sectorPuntoExpendio').val(sector || '');
            $('#lblSectorSeleccionadoTop, #estadoSectorPuntoExpendio, #estadoSectorPuntoExpendioFooter').text(texto);
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

        $(document).on('click', '.js-sector-item', function () {
            var sector = $(this).data('sector');
            if (!sector) return;
            window.location.href = config.urlPos + '?sector=' + encodeURIComponent(sector);
        });

        if (!config.sectorSeleccionado) {
            openSectorModal();
        } else {
            actualizarSectorUI(config.sectorSeleccionado);
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
                Observaciones: POSState.getObservaciones(),
                LineasVenta: lineas
            };
        }

        var guardando = false;

        $('#btnFinalizar').on('click', function () {
            if (guardando) return;

            var payload = construirPayload();
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
                        firePosAlert({
                            icon: 'error',
                            title: 'Punto de Expendio',
                            text: resp && resp.mensaje ? resp.mensaje : 'No se pudo guardar el punto de expendio.'
                        });
                        return;
                    }

                    window.PostPuntoExpendioModal.open(resp);
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
                window.location.href = config.urlPos + ($('#sectorPuntoExpendio').val() ? ('?sector=' + encodeURIComponent($('#sectorPuntoExpendio').val())) : '');
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
                POSState.clear();
                posCart.renderTable(POSState.getLineas());
                posCart.recalculateTotal();
                posCart.updateSaleState();
                posComment.updateButtonState();
                posProduct.showWaiting();
                posKeyboard.focusCodigo();
            });
        });

        document.addEventListener('keydown', function (e) {
            if ($('.modal.show').length && !$('#modalAyudaPOS').hasClass('show')) return;

            if (e.key === 'End') {
                e.preventDefault();

                // Si el cliente/identificacion esta vacio y no tiene el foco, primero
                // llevamos el foco ahi (para que se pueda cargar antes de finalizar).
                // Si ya tiene contenido (con o sin foco), o si esta vacio pero ya tiene
                // el foco, se finaliza directo.
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
                $('#razonSocial').trigger('focus');
                return;
            }

            if (e.key === 'F9') {
                e.preventDefault();
                clickIfEnabled('#btnBuscarPersona');
                return;
            }

            if (e.key === 'F10') {
                e.preventDefault();
                if (typeof window.abrirBuscadorProductosPOS === 'function') {
                    window.abrirBuscadorProductosPOS();
                }
            }
        });

        window.posHotkeysHooks = window.posHotkeysHooks || {};
        window.posHotkeysHooks.AvPag = function () {
            $('#razonSocial').trigger('focus');
        };
        window.posHotkeysHooks.F6 = function () {
            abrirMisExpendios();
        };
        // Mismo comportamiento que POS Venta: simula click en la ultima fila del
        // carrito para abrir el modal "Linea del punto de expendio" de ese item.
        window.posHotkeysHooks.F4 = function () {
            $('#tablaItems tr.fila-item').last().trigger('click');
        };

        // Compactado adaptativo del teclado/producto-info por altura de viewport,
        // portado de Ventas/POS.cshtml (aplicarCompacto/aplicarAjusteFooter). A
        // diferencia de Venta, aca se agrega un guard de ancho: el pedido fue
        // "solo responsive", asi que estas clases nunca se aplican en desktop
        // aunque la ventana sea baja (Venta si lo hace, por diseno propio de esa
        // vista).
        (function () {
            function getViewportHeight() {
                if (window.visualViewport && window.visualViewport.height) {
                    return Math.round(window.visualViewport.height);
                }
                return window.innerHeight || document.documentElement.clientHeight || 0;
            }

            function aplicarAjusteFooter() {
                var workbench = document.querySelector('.pos-expendio-actions-col');
                var footer = document.querySelector('.pos-footer-panel');
                if (!workbench || !footer) {
                    document.documentElement.classList.remove('pos-footer-fit');
                    document.documentElement.classList.remove('pos-footer-tiny-fit');
                    return;
                }

                var viewportHeight = getViewportHeight();
                var workbenchRect = workbench.getBoundingClientRect();
                var footerRect = footer.getBoundingClientRect();
                var overflow = Math.max(0, Math.ceil(footerRect.bottom - workbenchRect.bottom));

                var footerFit = overflow > 0 || viewportHeight < 950;
                var footerTinyFit = overflow > 24 || viewportHeight < 860;

                document.documentElement.classList.toggle('pos-footer-fit', footerFit);
                document.documentElement.classList.toggle('pos-footer-tiny-fit', footerTinyFit);
            }

            function aplicarCompacto() {
                if (window.innerWidth <= 991) {
                    // Responsive: sin cambios respecto a la ronda anterior.
                    var h = getViewportHeight();
                    document.documentElement.classList.toggle('pos-compact', h < 960);
                    document.documentElement.classList.toggle('pos-tiny', h < 860);

                    window.requestAnimationFrame(aplicarAjusteFooter);
                    return;
                }

                // Desktop: mismo mecanismo (antes solo corria en responsive).
                // El problema real es el mismo en las dos resoluciones -- en
                // notebooks con poca altura (768-900px) el contenido de la
                // columna de acciones no entra y #pos-app (overflow:hidden) lo
                // recorta, sin ningun mecanismo de achique. Rama separada (en
                // vez de sacar el guard de ancho sin mas) para que el codigo de
                // arriba, que ya corre en responsive, quede sin tocar.
                var hDesktop = getViewportHeight();
                document.documentElement.classList.toggle('pos-compact', hDesktop < 960);
                document.documentElement.classList.toggle('pos-tiny', hDesktop < 860);

                window.requestAnimationFrame(aplicarAjusteFooter);
            }

            window.addEventListener('resize', aplicarCompacto);
            window.addEventListener('orientationchange', aplicarCompacto);
            if (window.visualViewport) {
                window.visualViewport.addEventListener('resize', aplicarCompacto);
                window.visualViewport.addEventListener('scroll', aplicarCompacto);
            }
            aplicarCompacto();
        })();

        setTimeout(function () {
            posKeyboard.focusCodigo();
        }, 50);
    });
})(window, window.jQuery, document);
