(function (window, $) {
    'use strict';

    function createPOSProduct(options) {
        // Este modulo concentra el estado del producto que el cajero esta
        // trabajando en este momento: codigo, resultado de la busqueda,
        // precio actual y reglas especiales como "2X123".
        let typingTimer = null;
        let productoSeleccionado = null;
        let precioActual = 0;
        let buscandoProducto = false;
        let ultimoCodigoPedido = null;
        let reqProducto = null;
        let enterDesdeTecladoVirtual = false;
        const soloFormaPago = options.soloFormaPago === true;

        // Alta rapida de producto desde el POS (codigo no encontrado, ver docs/DECISIONS.md
        // 2026-09-12). Scoped a la vista via window.POS_ALTA_RAPIDA_HABILITADA (declarado solo en
        // Ventas/POS.cshtml, no en el _LayoutPOS.cshtml compartido) para no activarse en
        // PuntosExpendio/POS.cshtml, que tiene su propio scanner/panel no verificado para esto.
        let modoAltaProductoActivo = false;
        let codigoParaAltaRapida = null;
        let timestampCongeladoAlta = 0;
        const FREEZE_ALTA_RAPIDA_MS = 5000;
        // Origen de la ultima lectura por camara: permite exigir dos lecturas iguales consecutivas
        // antes de ofrecer el alta rapida SOLO para ese canal (cámara) -- no gatea la busqueda en
        // si (manejarEnter sigue disparandose en cada lectura, igual que siempre), solo si se
        // ofrece o no la sugerencia cuando el codigo no existe.
        let origenLecturaCamara = null;

        // Normaliza el codigo para que todas las comparaciones hablen el mismo
        // idioma: mayusculas, sin espacios sobrantes y con la X consistente.
        function normalizeInput(raw) {
            return String(raw ?? '')
                .trim()
                .toUpperCase()
                .replace(/\u00D7/g, 'X');
        }

        // Convierte un numero escrito con formato local a float.
        // Acepta "1,25" o "1.25".
        function parseFloatAR(value) {
            const number = parseFloat(String(value ?? '').trim().replace(',', '.'));
            return Number.isFinite(number) ? number : NaN;
        }

        // Detecta el formato "cantidad X codigo".
        // Si la entrada no aplica, devolvemos null para seguir con la busqueda normal.
        function parseCantidadXCodigo(input) {
            const normalized = normalizeInput(input);
            const match = normalized.match(/^([0-9]+(?:[.,][0-9]+)?)X(.+)$/);
            if (!match) return null;

            const cantidad = parseFloatAR(match[1]);
            const codigo = String(match[2] ?? '').trim();

            if (!codigo) return null;
            if (!Number.isFinite(cantidad) || cantidad <= 0) return null;

            return {
                cant: cantidad,
                codigo: codigo
            };
        }

        // Detecta codigos de barra de punto de expendio en formato PE123F.
        function parseExpendioBarcode(input) {
            const normalized = normalizeInput(input);
            const match = normalized.match(/^PE(\d+)F$/);
            if (!match) return null;

            const idExpendio = parseInt(match[1], 10) || 0;
            if (idExpendio <= 0) return null;

            return {
                idExpendio: idExpendio
            };
        }

        // Devuelve el foco al input principal del POS.
        function focusCodigo(selectText) {
            if (soloFormaPago) return;
            const $input = $('#inputCodigo');
            $input.focus();

            if (selectText !== false) {
                $input.select();
            }
        }

        // Lleva el foco a cantidad cuando ya confirmamos un producto valido.
        function focusCantidad() {
            if (soloFormaPago) return;
            $('#inputCantidad').focus().select();
        }

        function setSearchingState(active) {
            $('#inputCodigo').toggleClass('pos-input-buscando', !!active);
        }

        function showMessage(icon, title, text) {
            if (window.Swal) {
                Swal.fire({ icon: icon, title: title, text: text });
                return;
            }

            alert(text || title);
        }

        function debeAbrirPreseleccionFormaPago(codigo) {
            if (soloFormaPago) return false;
            if (!codigo) return false;
            if (window.POSState?.getRequierePreseleccionFormaPago?.() !== true) return false;
            if (window.POSState?.getFormaPagoPreseleccionada?.()?.tipo) return false;
            if ($('#modalFormaPago').hasClass('show')) return false;
            return typeof window.abrirModalFormaPagoPreseleccion === 'function';
        }

        // Restablece el panel de producto al estado neutro.
        function showWaiting() {
            setSearchingState(false);
            productoSeleccionado = null;
            precioActual = 0;

            $('#prodNombre').text('Info producto...').removeClass('fw-bold pos-campo-cargado').addClass('text-muted');
            $('#prodPrecio').text('').removeClass('pos-campo-cargado');
            $('#prodSubtotal').text('$ 0,00');
            $('#inputCantidad').prop('disabled', true).val('').removeClass('pos-campo-cargado');
            $('#btnAgregarProducto').prop('disabled', true);
            if (typeof options.onProductoChanged === 'function') {
                options.onProductoChanged(null);
            }
        }

        function abortPendingProductRequest() {
            if (reqProducto && reqProducto.readyState !== 4) {
                reqProducto.abort();
            }

            reqProducto = null;
            buscandoProducto = false;
            ultimoCodigoPedido = null;
            setSearchingState(false);
        }

        // Muestra un mensaje de espera mientras el backend responde.
        function showSearching(message, keepQuantity) {
            setSearchingState(true);
            const cantidadActual = keepQuantity ? $('#inputCantidad').val() : '';

            productoSeleccionado = null;
            precioActual = 0;

            $('#prodNombre').text(message || 'Buscando...').removeClass('fw-bold pos-campo-cargado').addClass('text-muted');
            $('#prodPrecio').text('').addClass('text-muted').removeClass('pos-campo-cargado');
            $('#prodSubtotal').text('$ 0,00');
            $('#inputCantidad').prop('disabled', true).val(cantidadActual);
            $('#inputCantidad').toggleClass('pos-campo-cargado', !!(cantidadActual && String(cantidadActual).trim()));
            $('#btnAgregarProducto').prop('disabled', true);
        }

        // Deja la UI lista para un nuevo intento cuando no encontramos coincidencia.
        function showNoMatch(message) {
            setSearchingState(false);
            productoSeleccionado = null;
            precioActual = 0;

            $('#prodNombre').text(message || 'Sin coincidencia').removeClass('fw-bold pos-campo-cargado').addClass('text-muted');
            $('#prodPrecio').text('').addClass('text-muted').removeClass('pos-campo-cargado');
            $('#prodSubtotal').text('$ 0,00');
            $('#inputCantidad').prop('disabled', true).val('').removeClass('pos-campo-cargado');
            $('#btnAgregarProducto').prop('disabled', true);
            if (typeof options.onProductoChanged === 'function') {
                options.onProductoChanged(null);
            }

            focusCodigo(false);
        }

        // Carga en pantalla el producto elegido y delega el subtotal al modulo
        // del carrito, que es quien conoce la logica de cantidades/subtotales.
        function showProduct(producto) {
            const formaPagoSeleccionada = window.POSState?.getFormaPagoPreseleccionada?.();
            const precioBase = Number(producto.precioOriginal || producto.precioKg || 0);
            const precioMostrado = formaPagoSeleccionada && window.POSFormaPagoPrecios
                ? window.POSFormaPagoPrecios.calcularPrecioFormaPago(precioBase, formaPagoSeleccionada.tipo)
                : precioBase;

            producto.precioOriginal = precioBase;
            producto.precioKg = precioMostrado;

            $('#prodNombre').text(producto.nombre).removeClass('text-muted').addClass('fw-bold pos-campo-cargado');
            $('#prodPrecio').text('$ ' + precioMostrado.toLocaleString('es-AR')).removeClass('text-muted').addClass('fw-bold pos-campo-cargado');

            const $precioManual = $('#inputPrecioManualExpendio');
            if ($precioManual.length && !$precioManual.prop('readonly')) {
                $precioManual.val(precioMostrado.toFixed(2).replace('.', ','));
            }

            precioActual = precioMostrado;

            $('#inputCantidad').prop('disabled', false);
            $('#prodSubtotal').text('$ 0,00');
            options.calculateSubtotal();
            $('#btnAgregarProducto').prop('disabled', false);
            if (typeof options.onProductoChanged === 'function') {
                options.onProductoChanged(producto);
            }
        }

        // Hace la consulta real al backend. Mantiene varias defensas del flujo
        // original: abortar requests viejos, ignorar respuestas atrasadas y
        // permitir callbacks para los casos de auto-agregado.
        // omitirAutoContinuar: bug real (2026-09-12, ver docs/DECISIONS.md) -- processCodeWithQuantity
        // ya tiene su PROPIO callback que fija cantidad y llama a options.addProduct() para el
        // caso EAN. Sin este flag, el bloque interno de aca abajo (linea ~250) TAMBIEN detecta
        // el mismo EAN valido y dispara su propio auto-continuar (handleEnter() -> addProduct())
        // antes de que corra ese callback -- el producto se agregaba dos veces (o la segunda
        // pasada chocaba con el estado ya limpiado por la primera, y visualmente "no pasaba
        // nada"). Nunca se habia notado: esEANValido no existia hasta ahora, esta rama jamas se
        // habia ejecutado. Los demas call-sites (busqueda en vivo, elegir del modal, codigo no-EAN
        // por Enter) siguen usando el bloque interno tal cual, sin este flag.
        // omitirSugerenciaAlta: bug real (2026-09-12, ver docs/DECISIONS.md) -- sin este flag, CUALQUIER
        // llamador de finishTyping (busqueda en vivo mientras se tipea, seleccion desde el modal
        // #modalBuscarProducto) dispararia la sugerencia de "codigo no encontrado, dar de alta" del
        // POS ante un data.success===false. Eso es incorrecto para esos dos casos: bindLiveSearch()
        // corre en cada tecla (el codigo puede estar a medio escribir) y openSearchModal() es una
        // busqueda/seleccion de un producto YA EXISTENTE (si no matchea es una carrera rara, no un
        // codigo recien escaneado). Solo los llamadores que representan "el usuario termino de
        // ingresar un codigo" (handleEnter(), las dos ramas de processCodeWithQuantity()) dejan este
        // flag en su default (permitido).
        function finishTyping(codigo, callback, ingresoCantidadXParam, omitirAutoContinuar, omitirSugerenciaAlta) {
            if (soloFormaPago) return;
            const codigoTrim = normalizeInput(codigo);

            if (!codigoTrim) {
                showWaiting();
                return;
            }

            showSearching(null, Boolean(ingresoCantidadXParam));

            if (reqProducto && reqProducto.readyState !== 4) {
                reqProducto.abort();
            }

            buscandoProducto = true;
            ultimoCodigoPedido = codigoTrim;

            reqProducto = $.ajax({
                url: window.buscarProductoUrl,
                type: 'GET',
                dataType: 'json',
                data: {
                    codigo: codigoTrim,
                    ingresoCantidadX: Boolean(ingresoCantidadXParam)
                },
                timeout: 3000,
                success: function (data) {
                    buscandoProducto = false;
                    setSearchingState(false);

                    const codigoActual = normalizeInput($('#inputCodigo').val());
                    if (codigoActual !== ultimoCodigoPedido) return;

                    if (data && data.success === false) {
                        productoSeleccionado = null;
                        showNoMatch(data.message);

                        if (!omitirSugerenciaAlta) {
                            evaluarSugerenciaAlta(codigoTrim);
                        }

                        if (typeof callback === 'function') {
                            callback(false, data);
                        }
                        return;
                    }

                    productoSeleccionado = data;
                    showProduct(data);

                    // Si el codigo es EAN valido, conservamos el comportamiento heredado:
                    // continuacion automatica del flujo. La cantidad es 1 salvo que el
                    // servidor haya interpretado el codigo como interno de balanza con
                    // TipoValor=Cantidad (data.cantidadSugerida) -- ahi el peso viene
                    // embebido en el codigo, no es "1 unidad" (ver BarcodeInterpreter).
                    if (!omitirAutoContinuar && typeof window.esEANValido === 'function' && window.esEANValido(codigoTrim)) {
                        var cantidadDesdeServidor = Number(data && data.cantidadSugerida);
                        $('#inputCantidad').val(cantidadDesdeServidor > 0 ? String(cantidadDesdeServidor) : '1');
                        document.querySelector('#inputCantidad')?.focus();
                        handleEnter();
                    }

                    if (typeof callback === 'function') {
                        callback(true, data);
                    }
                },
                error: function (xhr, status) {
                    if (status === 'abort') return;

                    buscandoProducto = false;
                    setSearchingState(false);

                    if (status === 'timeout') {
                        options.showConnectionError('La conexion es lenta. Reintente.');
                        return;
                    }

                    if (!navigator.onLine) {
                        options.showConnectionError('Sin conexion a Internet');
                        return;
                    }

                    options.showConnectionError('No se pudo contactar al servidor');
                }
            });
        }

        // Interpreta formatos rapidos como "2X123" o un EAN directo.
        // Si consigue resolverlos y dispara auto-agregado, devuelve true.
        function processCodeWithQuantity() {
            if (soloFormaPago) return false;
            const input = document.getElementById('inputCodigo');
            if (!input) return false;

            const entrada = normalizeInput(input.value);
            const parsed = parseCantidadXCodigo(entrada);

            if (parsed) {
                $('#inputCantidad').val(String(parsed.cant));
                $('#inputCodigo').val(parsed.codigo);

                finishTyping(parsed.codigo, function (ok) {
                    if (ok === false) return;
                    // showProduct() ya disparo onProductoChanged, que para productos no
                    // pesables (como el generico) limpia #inputCantidad para forzar carga
                    // manual. Como acá la cantidad ya vino explicita en "cantXcodigo",
                    // la reponemos antes de agregar.
                    $('#inputCantidad').val(String(parsed.cant));
                    options.addProduct();
                    showWaiting();
                }, true, true);

                return true;
            }

            if (typeof window.esEANValido === 'function' && window.esEANValido(entrada)) {
                $('#inputCantidad').val('1');
                $('#inputCodigo').val(entrada);

                finishTyping(entrada, function (ok, data) {
                    if (ok === false) return;
                    // Mismo caso que arriba: si el producto escaneado no es pesable,
                    // onProductoChanged ya limpio la cantidad por defecto (1). Si el
                    // servidor interpreto el codigo como interno de balanza con
                    // TipoValor=Cantidad (data.cantidadSugerida), usamos ese peso real en
                    // vez de forzar 1 -- sin esto, cualquier pesada real quedaria cargada
                    // como cantidad=1 (ver BarcodeInterpreter).
                    var cantidadDesdeServidor = Number(data && data.cantidadSugerida);
                    $('#inputCantidad').val(cantidadDesdeServidor > 0 ? String(cantidadDesdeServidor) : '1');
                    options.addProduct();
                    showWaiting();
                }, true, true);

                return true;
            }

            return false;
        }

        function processExpendioBarcode() {
            if (soloFormaPago) return false;

            const entrada = normalizeInput($('#inputCodigo').val());
            const parsed = parseExpendioBarcode(entrada);
            if (!parsed) return false;

            abortPendingProductRequest();
            window.POSExpendiosCurrent?.cargarExpendio?.(parsed.idExpendio);
            return true;
        }

        // Alta rapida de producto desde el POS (codigo no encontrado, ver docs/DECISIONS.md
        // 2026-09-12). Alterna entre el grupo normal del panel (.pos-product-metric +
        // #btnAgregarProducto) y los bloques nuevos via la clase "pos-alta-rapida-activa" en
        // .producto-resumen (ver pos.css) -- asi no se toca el DOM de #prodPrecio/#inputCantidad/
        // #prodSubtotal/#btnAgregarProducto, que el resto del modulo sigue referenciando por id.
        function mostrarPanelNormal(mostrar) {
            $('.producto-resumen').toggleClass('pos-alta-rapida-activa', !mostrar);
        }

        // Le informa al modulo que la camara del POS detecto "codigo", y si ya es la segunda
        // lectura igual consecutiva. Solo se usa para decidir si se ofrece el alta rapida cuando el
        // codigo no existe -- la busqueda en si (manejarEnter) se dispara en cada lectura, sin
        // cambios respecto al comportamiento de siempre.
        function registrarOrigenLecturaCamara(codigo, confirmadoPorDobleLectura) {
            origenLecturaCamara = { codigo: normalizeInput(codigo), confirmado: !!confirmadoPorDobleLectura };
        }

        // Decide si corresponde ofrecer el alta rapida para "codigo" (ya confirmado por el server
        // como inexistente). No hace nada si: la feature esta deshabilitada en esta vista, ya hay
        // un alta en curso, el codigo vino de la camara sin las dos lecturas iguales todavia, o el
        // mismo codigo ya fue sugerido/descartado hace menos de 5s (freeze).
        function evaluarSugerenciaAlta(codigo) {
            if (!window.POS_ALTA_RAPIDA_HABILITADA) return;
            if (modoAltaProductoActivo) return;

            if (origenLecturaCamara && origenLecturaCamara.codigo === codigo && !origenLecturaCamara.confirmado) {
                return;
            }

            const ahora = Date.now();
            if (codigoParaAltaRapida === codigo && (ahora - timestampCongeladoAlta) < FREEZE_ALTA_RAPIDA_MS) {
                return;
            }

            codigoParaAltaRapida = codigo;
            timestampCongeladoAlta = ahora;
            showSugerenciaAltaRapida(codigo);
        }

        function showSugerenciaAltaRapida(codigo) {
            mostrarPanelNormal(false);
            $('#prodAltaRapidaForm').addClass('d-none');
            $('#prodAltaRapidaSugerenciaMsg').text('Código ' + codigo + ' no encontrado. ¿Desea darlo de alta?');
            $('#prodAltaRapidaSugerencia').removeClass('d-none');
        }

        function ocultarBloquesAltaRapida() {
            modoAltaProductoActivo = false;
            $('#prodAltaRapidaSugerencia, #prodAltaRapidaForm').addClass('d-none');
            mostrarPanelNormal(true);
        }

        function showFormularioAltaRapida(codigo) {
            modoAltaProductoActivo = true;
            mostrarPanelNormal(false);
            $('#prodAltaRapidaSugerencia').addClass('d-none');

            $('#inputAltaRapidaDescripcion').val('');
            $('#inputAltaRapidaPrecio').val('');
            $('#checkAltaRapidaPesable').prop('checked', false);
            $('#prodAltaRapidaAutocompleteMsg').addClass('d-none').text('');
            $('#selectAltaRapidaIva').empty();

            $('#prodAltaRapidaForm').removeClass('d-none');

            let ultimoIva = null;
            try {
                ultimoIva = window.localStorage.getItem('posAltaRapidaUltimoIva');
            } catch (e) { /* localStorage no disponible (ej. navegacion privada) -- sin default */ }

            // El autocompletado desde el catalogo global (mas abajo) selecciona un <option> del
            // combo de IVA por value -- se encadena DESPUES de poblar el combo (no en paralelo) para
            // evitar la carrera de "seleccionar un value que todavia no existe" si esta respuesta
            // llegara antes que la de ObtenerAlicuotasIva.
            const $selectIva = $('#selectAltaRapidaIva');
            const poblarCombosPromise = window.obtenerAlicuotasIvaUrl
                ? $.get(window.obtenerAlicuotasIvaUrl).done(function (data) {
                    if (!data || !data.ok) return;
                    (data.alicuotas || []).forEach(function (a) {
                        $selectIva.append($('<option>').val(a.value).text(a.text));
                    });
                    if (ultimoIva) $selectIva.val(ultimoIva);
                })
                : $.Deferred().resolve();

            // Autocompletado desde el catalogo global: mismo endpoint y mismo gate cliente
            // (solo EAN-8/13 valido) que ya usa AddOrEdit.cshtml -- un codigo generico ni siquiera
            // dispara la consulta (el propio endpoint tambien lo rechazaria server-side).
            if (typeof window.esEANValido === 'function' && window.esEANValido(codigo) && window.buscarProductoGlobalParaAltaUrl) {
                poblarCombosPromise.always(function () {
                    $.get(window.buscarProductoGlobalParaAltaUrl, { codigoBarra: codigo }).done(function (data) {
                        if (!data || !data.ok || !data.producto) return;

                        $('#inputAltaRapidaDescripcion').val(data.producto.descripcion || '');
                        if (data.producto.precioKg) {
                            $('#inputAltaRapidaPrecio').val(String(data.producto.precioKg).replace('.', ','));
                        }
                        $('#checkAltaRapidaPesable').prop('checked', !!data.producto.pesable);
                        if (data.producto.idAlicuotaIva) {
                            $selectIva.val(data.producto.idAlicuotaIva);
                        }
                        $('#prodAltaRapidaAutocompleteMsg').text('Se autocompletaron los datos desde el catálogo global.').removeClass('d-none');
                    });
                });
            }

            document.querySelector('#inputAltaRapidaDescripcion')?.focus();
        }

        function guardarAltaRapida() {
            const codigo = codigoParaAltaRapida;
            const descripcion = String($('#inputAltaRapidaDescripcion').val() ?? '').trim();
            const precio = parseFloatAR($('#inputAltaRapidaPrecio').val());
            const idAlicuotaIva = $('#selectAltaRapidaIva').val();
            const pesable = $('#checkAltaRapidaPesable').is(':checked');

            if (!descripcion) {
                showMessage('warning', 'Alta rápida', 'Ingresá una descripción.');
                return;
            }

            if (!Number.isFinite(precio) || precio <= 0) {
                showMessage('warning', 'Alta rápida', 'Ingresá un precio válido.');
                return;
            }

            const tokenAntiForgery = document.querySelector('#globalAntiForgeryToken input[name="__RequestVerificationToken"]')?.value
                || document.querySelector('input[name="__RequestVerificationToken"]')?.value
                || '';

            $.ajax({
                url: window.guardarRapidoPOSUrl,
                type: 'POST',
                dataType: 'json',
                data: {
                    Codigo: codigo,
                    CorteDesc: descripcion,
                    PrecioKg: precio,
                    IdAlicuotaIva: idAlicuotaIva,
                    Pesable: pesable,
                    __RequestVerificationToken: tokenAntiForgery
                }
            }).done(function (data) {
                if (!data || !data.ok) {
                    showMessage('error', 'Alta rápida', (data && data.error) || 'No se pudo guardar el producto.');
                    return;
                }

                try {
                    window.localStorage.setItem('posAltaRapidaUltimoIva', String(idAlicuotaIva));
                } catch (e) { /* localStorage no disponible -- no se persiste, sin impacto funcional */ }

                ocultarBloquesAltaRapida();

                productoSeleccionado = data.corte;
                showProduct(data.corte);
                $('#inputCantidad').val('1');
                options.addProduct();
                showWaiting();
            }).fail(function () {
                showMessage('error', 'Alta rápida', 'No se pudo contactar al servidor.');
            });
        }

        function bindAltaRapidaEvents() {
            $('#btnAltaRapidaSi').off('click').on('click', function () {
                showFormularioAltaRapida(codigoParaAltaRapida);
            });
            $('#btnAltaRapidaNo').off('click').on('click', function () {
                ocultarBloquesAltaRapida();
                focusCodigo();
            });
            $('#btnAltaRapidaGuardar').off('click').on('click', guardarAltaRapida);
            $('#btnAltaRapidaCancelar').off('click').on('click', function () {
                ocultarBloquesAltaRapida();
                focusCodigo();
            });
        }

        // Centraliza la accion Enter del bloque de producto.
        // Si el foco esta en cantidad, agrega.
        // Si el foco esta en codigo, busca o intenta auto-agregado.
        function handleEnter() {
            // Si hay un modal abierto (ej. Compras) el foco puede haber quedado en un input de
            // esta pantalla de atras -- el backdrop de Bootstrap bloquea el mouse pero no el
            // teclado. Sin este chequeo, Enter (fisico o del teclado virtual, que tambien pasa
            // por aca) terminaba agregando un producto detras del modal.
            if ($(".modal.show").length) return;
            if (soloFormaPago) return;
            const inputActivo = options.getInputActivo();
            if (!inputActivo) return;

            if (inputActivo.id === 'inputCantidad') {
                options.addProduct();
                enterDesdeTecladoVirtual = false;
                return;
            }

            if (enterDesdeTecladoVirtual || inputActivo.id === 'inputCodigo') {
                const autoCargaExpendio = processExpendioBarcode();
                if (autoCargaExpendio) {
                    enterDesdeTecladoVirtual = false;
                    return;
                }

                const autoAgregado = processCodeWithQuantity();
                enterDesdeTecladoVirtual = false;

                if (autoAgregado) return;

                const codigoInput = normalizeInput($('#inputCodigo').val());

                if (codigoInput.includes('X')) {
                    showNoMatch('Formato invalido (use 2X123)');
                    return;
                }

                if (buscandoProducto) return;

                finishTyping(codigoInput, function () {
                    const codigoActual = normalizeInput($('#inputCodigo').val());
                    const codigoProducto = normalizeInput(productoSeleccionado?.codigo);

                    if (!codigoProducto || codigoProducto !== codigoActual) {
                        showNoMatch();
                        productoSeleccionado = null;
                        return;
                    }

                    if (typeof options.onProductoConfirmed === 'function') {
                        options.onProductoConfirmed(productoSeleccionado);
                    }

                    focusCantidad();
                });
            }
        }

        // Abre el modal global de productos y, cuando el usuario elige uno,
        // reusa el mismo flujo de busqueda que usa el input de codigo.
        function openSearchModal() {
            if (soloFormaPago) return;
            if (typeof window.abrirBuscarProductoModal !== 'function') {
                showMessage('error', 'Buscador de productos', 'No se pudo abrir el buscador de productos. Verifica que el script global del modal esté cargado.');
                return;
            }

            options.clearInputActivo();

            window.abrirBuscarProductoModal({
                modalSelector: '#modalBuscarProducto',
                mostrarPrecio: true,
                onSelect: function (producto) {
                    const codigo = String((producto && producto.codigo) || '').trim();
                    if (!codigo) {
                        focusCodigo();
                        return;
                    }

                    $('#inputCodigo').val(codigo);
                    clearTimeout(typingTimer);

                    // omitirSugerenciaAlta=true: es una seleccion desde el buscador de productos
                    // existentes, no un escaneo/tipeo de codigo nuevo -- si no matchea es una
                    // carrera rara (el producto se borro entre listar y elegir), no un caso de alta.
                    finishTyping(codigo, function (ok) {
                        if (ok === false) {
                            focusCodigo();
                            return;
                        }

                        const codigoActual = normalizeInput($('#inputCodigo').val());
                        const codigoProducto = normalizeInput(productoSeleccionado?.codigo);

                        if (!codigoProducto || codigoProducto !== codigoActual) {
                            showNoMatch();
                            productoSeleccionado = null;
                            focusCodigo();
                            return;
                        }

                        if (typeof options.onProductoConfirmed === 'function') {
                            options.onProductoConfirmed(productoSeleccionado);
                        }

                        if (typeof window.esEANValido === 'function' && window.esEANValido(codigoActual)) {
                            return;
                        }

                        focusCantidad();
                    }, undefined, undefined, true);
                }
            });
        }

        // Escucha lo que se escribe en codigo y dispara una busqueda con debounce.
        // El Enter no se procesa aca para no duplicar logica.
        function bindLiveSearch() {
            if (soloFormaPago) return;
            $('#inputCodigo').on('keyup', function (e) {
                const codigo = String(this.value ?? '').trim().toUpperCase();

                if (e.key === 'Enter') {
                    clearTimeout(typingTimer);
                    return;
                }

                if (codigo.includes('X')) {
                    // Mientras el usuario arma un patron tipo 4X5 o 4X5G
                    // no consultamos al backend ni forzamos el foco.
                    // El procesamiento real se hace al confirmar con Enter.
                    clearTimeout(typingTimer);
                    return;
                }

                clearTimeout(typingTimer);

                if (debeAbrirPreseleccionFormaPago(codigo)) {
                    window.abrirModalFormaPagoPreseleccion();
                    return;
                }

                typingTimer = setTimeout(function () {
                    // omitirSugerenciaAlta=true: se sigue tipeando, el codigo puede estar incompleto.
                    finishTyping(codigo, undefined, undefined, undefined, true);
                }, 250);
            });
        }

        // Mientras el modal de busqueda esta abierto, el teclado del POS no debe
        // quedar apuntando a inputs que no pertenecen a la pantalla principal.
        function bindSearchModalFocus() {
            $('#modalBuscarProducto')
                .off('shown.bs.modal.posBuscar hidden.bs.modal.posBuscar')
                .on('shown.bs.modal.posBuscar', function () {
                    options.clearInputActivo();
                })
                .on('hidden.bs.modal.posBuscar', function () {
                    // Bug real reportado 2026-09-12 (ver docs/DECISIONS.md "Batch 3b"):
                    // #modalBuscarProducto es compartido con el buscador de producto embebido en
                    // Compras dentro de POS (compras.js, abrirProductoModal) -- si el modal se abrio
                    // desde ahi, el foco debe quedar en #txtCantKgs del formulario de Compra, no
                    // volver aca a #inputCodigo de la venta de fondo. Mismo criterio ya usado para
                    // #modalBuscarPersona/origen-persona-buscar (persona-buscar.js).
                    if ($(this).data('origen-producto-buscar') === 'compra-embebida') return;

                    setTimeout(function () {
                        focusCodigo();
                    }, 0);
                });
        }

        function bindDomEvents() {
            if (soloFormaPago) return;
            $('#btnAgregarManual').off('click').on('click', function (e) {
                e.preventDefault();
                openSearchModal();
            });

            bindLiveSearch();
            bindSearchModalFocus();
            bindAltaRapidaEvents();
        }

        const api = {
            init: function () {
                bindDomEvents();
            },
            getProductoSeleccionado: function () {
                return productoSeleccionado;
            },
            setProductoSeleccionado: function (value) {
                productoSeleccionado = value || null;
                return productoSeleccionado;
            },
            getPrecioActual: function () {
                return precioActual;
            },
            getTypingTimer: function () {
                return typingTimer;
            },
            setEnterDesdeTecladoVirtual: function (value) {
                enterDesdeTecladoVirtual = Boolean(value);
            },
            focusCodigo: focusCodigo,
            focusCantidad: focusCantidad,
            showWaiting: showWaiting,
            showSearching: showSearching,
            showNoMatch: showNoMatch,
            showProduct: showProduct,
            handleEnter: handleEnter,
            finishTyping: finishTyping,
            openSearchModal: openSearchModal,
            normalizeInput: normalizeInput,
            parseFloatAR: parseFloatAR,
            parseCantidadXCodigo: parseCantidadXCodigo,
            parseExpendioBarcode: parseExpendioBarcode,
            registrarOrigenLecturaCamara: registrarOrigenLecturaCamara,
            isModoAltaProductoActivo: function () {
                return modoAltaProductoActivo;
            }
        };

        // Wrappers globales para mantener compatibilidad con el codigo que todavia
        // sigue dentro de la vista o en scripts heredados.
        window.abrirBuscadorProductosPOS = openSearchModal;
        window.manejarEnter = handleEnter;
        window.terminarEscritura = finishTyping;
        window.normalizarEntrada = normalizeInput;
        window.parseFloatAR = parseFloatAR;
        window.parseCantidadXCodigo = parseCantidadXCodigo;
        window.parseExpendioBarcode = parseExpendioBarcode;
        window.mostrarProducto = showProduct;
        window.mostrarSinCoincidencia = showNoMatch;
        window.mostrarEsperando = showWaiting;
        window.mostrarBuscando = showSearching;

        return api;
    }

    window.POSProduct = {
        create: createPOSProduct
    };
})(window, window.jQuery);
