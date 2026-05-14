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

        window.buscarProductoUrl = config.urlBuscarProductoPos;
        window.api = window.api || {};
        window.api.persona = {
            listar: config.urlPersonaListar,
            buscar: config.urlPersonaBuscar
        };

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

        function clickIfEnabled(selector) {
            var button = document.querySelector(selector);
            if (button && !button.disabled) {
                button.click();
            }
        }

        var posProduct = null;
        var posCart = null;
        var posKeyboard = window.POSKeyboard.create({
            handleEnter: function () { return posProduct ? posProduct.handleEnter() : null; },
            calculateSubtotal: function () { return posCart ? posCart.calculateSubtotal() : null; },
            finishTyping: function (value) { return posProduct ? posProduct.finishTyping(value) : null; },
            setEnterDesdeTecladoVirtual: function (value) { return posProduct ? posProduct.setEnterDesdeTecladoVirtual(value) : null; }
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
            showConnectionError: showConnectionError
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
            setEnterDesdeTecladoVirtual: function (value) { return posProduct.setEnterDesdeTecladoVirtual(value); }
        });

        posProduct.init();
        posCart.init();
        posKeyboard.init();
        posHelp.init();
        posComment.init();
        actualizarFechaHora();
        window.setInterval(actualizarFechaHora, 60000);

        $('#inputPrecioManualExpendio').on('input', function () {
            posCart.calculateSubtotal();
        });

        $(document).on('click', '#btnBuscarPersona', function () {
            var $btn = $(this);
            var $contenedor = $('#contenedorModalPersona');

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
        });

        $('#razonSocial').on('input', function () {
            $('#idPersona').val('0');
        });

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
                Swal.fire({ icon: 'warning', title: 'Sector', text: 'Debe seleccionar un sector.' });
                openSectorModal();
                return;
            }

            if (!payload.LineasVenta.length) {
                Swal.fire({ icon: 'warning', title: 'Punto de expendio', text: 'Debe agregar al menos un producto.' });
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
                        Swal.fire({
                            icon: 'error',
                            title: 'Punto de Expendio',
                            text: resp && resp.mensaje ? resp.mensaje : 'No se pudo guardar el punto de expendio.'
                        });
                        return;
                    }

                    window.PostPuntoExpendioModal.open(resp);
                })
                .fail(function () {
                    Swal.fire({
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
                clickIfEnabled('#btnFinalizar');
                return;
            }

            if (e.key === 'Home') {
                e.preventDefault();
                posKeyboard.focusCodigo();
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

        setTimeout(function () {
            posKeyboard.focusCodigo();
        }, 50);
    });
})(window, window.jQuery, document);
