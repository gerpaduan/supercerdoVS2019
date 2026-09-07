// Port de Web/Scripts/app/ventas-expendios-pos.js (batch 5, ver
// docs/10-migracion-aspnet-core/PLAN-POS-UI.md) + refinamiento pedido explicito por el usuario
// (2026-09-04, ver docs/DECISIONS.md): filtros avanzados (fecha hasta + sucursal), "Limpiar
// filtros", "Cargar todos" con confirmacion, columna Sucursal condicional. Sigue apuntando a
// BuscarExpendiosPOS/ObtenerExpendioPOS (portados y verificados con datos reales, PLAN-POS.md) --
// BuscarExpendiosPOS ahora recibe fechaDesde/fechaHasta/idSucursal reales en vez de
// ultimosMinutos (ver VentasController.cs); ObtenerExpendioPOS ya no bloquea cargar un expendio
// de otra sucursal (cambio deliberado, autorizado explicitamente por el usuario).
(function (window, $) {
    'use strict';

    function createPOSExpendios(options) {
        const POSState = options.POSState;
        let allItems = [];
        let visibleItems = [];
        let remoteFiltersDirty = false;

        // Observaciones acumuladas de los expendios cargados en ESTA venta (mas
        // antiguo primero). Vive en memoria mientras dura la venta actual, igual
        // que POSState -- se resetea con cada venta nueva porque la instancia de
        // este modulo se recrea por pagina.
        let expendioObservaciones = [];
        const MAX_COMENTARIO_VENTA = 200; // debe coincidir con MAX_LENGTH de pos-comment.js
        // Recuerda el ultimo bloque de texto que este modulo agrego al comentario
        // de la venta, para poder reemplazarlo (no duplicarlo) si el vendedor carga
        // otro expendio con observacion y vuelve a tocar "Agregar comentario a la
        // venta". No se usan marcadores visibles en el texto (ej. "[INICIO...]")
        // porque el comentario de la venta se imprime en el ticket -- un marcador
        // ahi quedaria feo. En cambio, se guarda el bloque exacto insertado y se
        // busca/reemplaza por texto plano.
        let ultimoBloqueObservacionesInsertado = null;

        function idSucursalUsuario() {
            return parseInt(window.idSucursalUsuarioPOS, 10) || 0;
        }

        function showMessage(type, text) {
            const $msg = $('#msgExpendiosPOS');
            if (!$msg.length) return;

            if (!text) {
                $msg.addClass('d-none').removeClass('alert-info alert-warning alert-danger alert-success').text('');
                return;
            }

            const css = {
                info: 'alert-info',
                warning: 'alert-warning',
                danger: 'alert-danger',
                success: 'alert-success'
            }[type || 'info'] || 'alert-info';

            $msg
                .removeClass('d-none alert-info alert-warning alert-danger alert-success')
                .addClass(css)
                .text(text);
        }

        function extractAjaxError(xhr, fallback) {
            if (xhr && xhr.responseJSON && xhr.responseJSON.msg) {
                return xhr.responseJSON.msg;
            }

            const raw = xhr && typeof xhr.responseText === 'string'
                ? xhr.responseText.replace(/\s+/g, ' ').trim()
                : '';

            if (raw) {
                return raw.slice(0, 300);
            }

            return fallback || 'No se pudieron consultar los expendios.';
        }

        function formatKg(value) {
            const num = Number(value || 0);
            return num.toLocaleString('es-AR', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
        }

        // Precio/Total por item (2026-09-06, pedido explicito del usuario -- ver
        // docs/DECISIONS.md). BuscarExpendiosPOS ya devuelve precioKg/total por linea
        // (VentasController.cs), solo faltaba mostrarlos en la tabla.
        function formatMoney(value) {
            const num = Number(value || 0);
            return '$ ' + num.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        }

        function formatDate(value) {
            if (!value) return '';

            const date = new Date(value);
            if (isNaN(date.getTime())) return '';

            return pad2(date.getDate()) + '/' + pad2(date.getMonth() + 1) + '/' + date.getFullYear();
        }

        function pad2(value) {
            return value < 10 ? '0' + value : '' + value;
        }

        function toDateTimeLocalValue(date) {
            return date.getFullYear()
                + '-' + pad2(date.getMonth() + 1)
                + '-' + pad2(date.getDate())
                + 'T'
                + pad2(date.getHours())
                + ':' + pad2(date.getMinutes());
        }

        // Deja los filtros exactamente como estarian si el modal se abriera por primera vez:
        // fecha desde = inicio del dia de hoy, fecha hasta vacia, sucursal = la del usuario
        // actual, estado = Pendientes, busqueda vacia, filtros avanzados colapsados, "cargar
        // todos" destildado. Se usa tanto al abrir el modal como en "Limpiar filtros" y al
        // finalizar una venta (pedido explicito del usuario, ver docs/DECISIONS.md).
        function resetFiltros() {
            const now = new Date();
            const startOfDay = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);

            $('#expFiltroFechaDesde').val(toDateTimeLocalValue(startOfDay));
            $('#expFiltroFechaHasta').val('');
            $('#expFiltroSucursal').val(String(idSucursalUsuario()));
            $('#expFiltroEstado').val('Pendientes');
            $('#expFiltroTexto').val('');
            $('#chkCargarTodosExpendiosPOS').prop('checked', false);
            $('#panelFiltrosAvanzadosExpendiosPOS').addClass('d-none');
            $('#expSwitchFiltrosAvanzados').prop('checked', false);

            actualizarColumnaSucursal();
        }

        function parseFecha(selector) {
            const raw = ($(selector).val() || '').trim();
            if (!raw) return null;

            const value = new Date(raw);
            return isNaN(value.getTime()) ? null : value;
        }

        // Saca acentos (normaliza a NFD y filtra los diacriticos combinantes, rango Unicode
        // U+0300-U+036F) para que la busqueda no distinga "García" de "Garcia".
        function normalizeText(value) {
            var sinAcentos = (value || '')
                .toString()
                .toUpperCase()
                .normalize('NFD')
                .split('')
                .filter(function (ch) {
                    var code = ch.charCodeAt(0);
                    return code < 0x0300 || code > 0x036f;
                })
                .join('');
            return sinAcentos;
        }

        function getIdsActuales() {
            return (POSState.getListaExpendios ? POSState.getListaExpendios() : [])
                .filter(function (id) { return (parseInt(id, 10) || 0) > 0; });
        }

        function syncAssignedFromCart() {
            const presentes = {};
            (POSState.getLineas() || []).forEach(function (linea) {
                const id = parseInt(linea && linea.idExpendio, 10) || 0;
                if (id > 0) presentes[id] = true;
            });

            const ids = getIdsActuales().filter(function (id) {
                return !!presentes[id];
            });

            POSState.setListaExpendios?.(ids);
            updateRemoveButton();
        }

        function updateRemoveButton() {
            const ids = getIdsActuales();
            $('#btnQuitarExpendiosPOS').prop('disabled', ids.length === 0);
        }

        // La columna Sucursal solo aporta algo cuando la busqueda puede traer expendios de mas
        // de una sucursal -- se muestra si el filtro es "Todas" (-1) o una sucursal puntual
        // DISTINTA a la del usuario actual; se oculta si es "mi sucursal" (caso mas comun).
        function actualizarColumnaSucursal() {
            const idFiltro = parseInt($('#expFiltroSucursal').val(), 10);
            const mostrar = idFiltro !== idSucursalUsuario();
            $('.exp-col-sucursal').toggleClass('d-none', !mostrar);
        }

        // Agrupa la lista plana que devuelve BuscarExpendiosPOS (una fila por CADA linea/producto
        // de cada expendio) en grupos por idExpendio, preservando el orden de primera aparicion.
        // No depende de que el backend devuelva las filas de un mismo expendio contiguas (aunque
        // hoy lo hace, por el ORDER BY fechaExpendio, idExpendio) -- agrupar por Map es correcto
        // sin importar el orden real de la respuesta.
        function agruparPorExpendio(items) {
            const orden = [];
            const mapa = new Map();

            (items || []).forEach(function (item) {
                const id = parseInt(item.idExpendio, 10) || 0;
                if (!mapa.has(id)) {
                    mapa.set(id, { idExpendio: id, cabecera: item, lineas: [] });
                    orden.push(id);
                }
                mapa.get(id).lineas.push(item);
            });

            return orden.map(function (id) { return mapa.get(id); });
        }

        // Pedido explicito del usuario (2026-09-06, ver docs/DECISIONS.md): la tabla se organiza
        // por expendio -- una fila de "encabezado" con fecha/hora/nro/identificacion/sector/
        // vendedor/observacion y el UNICO boton "Cargar" de todo el grupo, y debajo una fila por
        // cada item (producto + cantidad) del expendio, sin boton propio. cargarExpendioInterno ya
        // trae SIEMPRE todas las lineas del expendio via ObtenerExpendioPOS (no las de esta lista,
        // que es solo para mostrar) -- agrupar la vista no cambia que se cargue "todo o nada".
        function renderRows(items) {
            const $tbody = $('#tablaExpendiosPOS tbody');
            if (!$tbody.length) return;

            visibleItems = Array.isArray(items) ? items.slice() : [];

            $tbody.empty();

            const mostrarSucursal = !$('.exp-col-sucursal').hasClass('d-none');
            const colspan = mostrarSucursal ? 13 : 12;

            if (!items || !items.length) {
                $tbody.append('<tr><td colspan="' + colspan + '" class="text-center text-muted py-4">No se encontraron expendios para esos filtros.</td></tr>');
                return;
            }

            // Colspan de la parte "identificatoria" del expendio (Fecha..Sucursal) que las filas
            // de item dejan en blanco/indentado, y de la parte final (Vendedor+Obs+Accion) que la
            // fila de encabezado sí completa pero la fila de item deja en blanco.
            const colspanIzquierdo = mostrarSucursal ? 6 : 5;

            const grupos = agruparPorExpendio(items);

            grupos.forEach(function (grupo) {
                const item = grupo.cabecera;

                const estadoBadge = item.cargadoEnVentaActual
                    ? '<span class="badge badge-info">En venta actual</span>'
                    : (item.asignado ? '<span class="badge badge-secondary">Asignado</span>' : '<span class="badge badge-warning">Pendiente</span>');

                const disabled = item.cargadoEnVentaActual || (item.asignado && !item.cargadoEnVentaActual);
                const btnClass = disabled ? 'btn-outline-secondary' : 'btn-outline-primary';
                const btnLabel = item.cargadoEnVentaActual ? 'Cargado' : 'Cargar';

                const tieneObservacion = !!(item.observaciones && String(item.observaciones).trim());
                const celdaObservacion = tieneObservacion
                    ? '<span class="btnVerObservacionExpendio" data-observacion="' + escapeHtml(item.observaciones) + '" ' +
                      'title="Tiene una observacion -- click para leerla" ' +
                      'style="display:inline-block;width:12px;height:12px;border-radius:50%;background:#dc3545;cursor:pointer;"></span>'
                    : '';

                $tbody.append(
                    '<tr class="exp-group-header" data-id-expendio="' + item.idExpendio + '">' +
                    '<td>' + formatDate(item.fechaExpendio) + '</td>' +
                    '<td>' + (item.hora || '') + '</td>' +
                    '<td><strong>' + item.idExpendio + '</strong><div>' + estadoBadge + '</div></td>' +
                    '<td>' + (item.identificacionExpendio || '') + '</td>' +
                    '<td>' + (item.sector || '') + '</td>' +
                    (mostrarSucursal ? '<td class="exp-col-sucursal">' + escapeHtml(item.sucursal || '') + '</td>' : '') +
                    '<td colspan="4" class="text-muted small">' + grupo.lineas.length + ' ítem(s)</td>' +
                    '<td>' + (item.vendedor || '') + '</td>' +
                    '<td class="text-center">' + celdaObservacion + '</td>' +
                    '<td class="text-center">' +
                    '<button type="button" class="btn btn-sm ' + btnClass + ' btnCargarExpendioPOS" data-id-expendio="' + item.idExpendio + '"' + (disabled ? ' disabled' : '') + '>' + btnLabel + '</button>' +
                    '</td>' +
                    '</tr>'
                );

                grupo.lineas.forEach(function (linea) {
                    const total = linea.total != null ? linea.total : (Number(linea.precioKg || 0) * Number(linea.cantKg || 0));
                    $tbody.append(
                        '<tr class="exp-group-item" data-id-expendio="' + item.idExpendio + '">' +
                        '<td colspan="' + colspanIzquierdo + '"></td>' +
                        '<td><div>' + (linea.producto || '') + '</div><div class="text-muted small">Cod. ' + (linea.codigo || 0) + '</div></td>' +
                        '<td class="text-right">' + formatKg(linea.cantKg) + '</td>' +
                        '<td class="text-right">' + formatMoney(linea.precioKg) + '</td>' +
                        '<td class="text-right">' + formatMoney(total) + '</td>' +
                        '<td colspan="3"></td>' +
                        '</tr>'
                    );
                });
            });

            actualizarColumnaSucursal();
        }

        function escapeHtml(value) {
            return String(value || '')
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;')
                .replace(/'/g, '&#39;');
        }

        function collectRemoteFilters() {
            const fechaDesde = parseFecha('#expFiltroFechaDesde') || new Date(new Date().setHours(0, 0, 0, 0));
            const fechaHasta = parseFecha('#expFiltroFechaHasta');

            return {
                fechaDesde: toDateTimeLocalValue(fechaDesde),
                fechaHasta: fechaHasta ? toDateTimeLocalValue(fechaHasta) : '',
                idSucursal: parseInt($('#expFiltroSucursal').val(), 10),
                estado: 'Todos',
                texto: '',
                idsActuales: getIdsActuales().join(',')
            };
        }

        function applyLocalFilters() {
            const estado = ($('#expFiltroEstado').val() || 'Pendientes').trim();
            const texto = normalizeText(($('#expFiltroTexto').val() || '').trim());

            const filtered = (allItems || []).filter(function (item) {
                const cargadoEnVentaActual = item.cargadoEnVentaActual === true;
                const asignado = item.asignado === true;

                if (estado === 'Pendientes' && (cargadoEnVentaActual || asignado)) {
                    return false;
                }

                if (estado === 'Asignados' && !cargadoEnVentaActual && !asignado) {
                    return false;
                }

                if (texto) {
                    const candidate = normalizeText(
                        (item.idExpendio || '')
                        + ' '
                        + (item.identificacionExpendio || '')
                    );

                    if (candidate.indexOf(texto) < 0) {
                        return false;
                    }
                }

                return true;
            });

            renderRows(filtered);

            if (!filtered.length) {
                showMessage('info', remoteFiltersDirty
                    ? 'Los filtros cambiaron. Presione Buscar para actualizar desde la fecha seleccionada.'
                    : 'No hay expendios para los filtros seleccionados.');
                return;
            }

            if (remoteFiltersDirty) {
                showMessage('info', 'La fecha cambió. Presione Buscar para actualizar la lista.');
            } else {
                showMessage(null, '');
            }
        }

        function markRemoteFiltersDirty() {
            remoteFiltersDirty = true;
            showMessage('info', 'La fecha cambió. Presione Buscar para actualizar la lista.');
        }

        function clearRemoteFiltersDirty() {
            remoteFiltersDirty = false;
        }

        function getFirstVisibleExpendioId() {
            if (visibleItems && visibleItems.length) {
                const firstAvailable = visibleItems.find(function (item) {
                    return item && item.cargadoEnVentaActual !== true && item.asignado !== true;
                });

                if (firstAvailable) {
                    return parseInt(firstAvailable.idExpendio, 10) || 0;
                }

                return parseInt(visibleItems[0].idExpendio, 10) || 0;
            }

            return 0;
        }

        function buscar() {
            clearRemoteFiltersDirty();
            showMessage(null, '');
            renderRows([]);
            $('#tablaExpendiosPOS tbody').html('<tr><td colspan="11" class="text-center text-muted py-4">Buscando expendios...</td></tr>');

            $.ajax({
                url: window.api?.venta?.buscarExpendiosPos,
                type: 'GET',
                dataType: 'json',
                cache: false,
                data: collectRemoteFilters(),
                success: function (resp) {
                    if (!resp || resp.ok === false) {
                        allItems = [];
                        renderRows([]);
                        showMessage('warning', resp?.msg || 'No se pudieron consultar los expendios.');
                        return;
                    }

                    allItems = Array.isArray(resp.items) ? resp.items : [];
                    applyLocalFilters();
                    if (!allItems.length) {
                        showMessage('info', 'No hay expendios para los filtros seleccionados.');
                    }
                },
                error: function (xhr) {
                    allItems = [];
                    renderRows([]);
                    showMessage('danger', extractAjaxError(xhr, 'No se pudieron consultar los expendios.'));
                }
            });
        }

        function buildLinea(item, detalle) {
            const precio = Number(detalle.precioKg || 0);
            const cant = Number(detalle.cantKg || 0);
            const subtotal = precio * cant;
            const idExpendio = parseInt(item.idExpendio, 10) || 0;

            return {
                index: POSState.nextIndex(),
                idCorte: 0,
                idExpendio: idExpendio,
                producto: detalle.producto || ('Expendio #' + item.idExpendio),
                descripcion: detalle.producto || '',
                codigo: detalle.codigo || 0,
                cant: cant.toFixed(3),
                precio: '$ ' + precio.toFixed(2),
                precioOriginal: '$ ' + precio.toFixed(2),
                subtotal: '$ ' + subtotal.toFixed(2),
                bonificacion: Number(detalle.bonificacion || 0),
                anulado: false,
                indexAnulado: -1,
                balanza: detalle.balanza === true,
                // Los expendios siempre se cargan por peso (cantKg), no hay
                // variante "por unidad" para este flujo.
                pesable: true,
                // Etiqueta "Cambio precio" en el carrito (2026-09-06, pedido explicito del
                // usuario -- ver docs/DECISIONS.md): ObtenerExpendioPOS ya compara el precio
                // guardado en la linea del expendio contra el precio de lista ACTUAL del
                // producto (join en vivo contra corte) -- se propaga tal cual, pos-cart.js la
                // usa para mostrar el aviso.
                cambioPrecio: detalle.cambioPrecio === true,
                precioListaActual: Number(detalle.precioListaActual || 0)
            };
        }

        // Nucleo real de "cargar un expendio a la venta" -- separado de cargarExpendio() para
        // que "Cargar todos" pueda reusarlo sin cerrar el modal ni abrir el modal de
        // observaciones en cada iteracion (silent=true). Devuelve una Promise para poder
        // encadenar la carga secuencial de "Cargar todos" sin disparar N requests en paralelo.
        function cargarExpendioInterno(idExpendio, silent) {
            const id = parseInt(idExpendio, 10) || 0;
            if (id <= 0) return Promise.resolve({ ok: false, msg: 'Expendio inválido.' });

            if (getIdsActuales().indexOf(id) >= 0) {
                if (!silent) {
                    showMessage('warning', 'El expendio ya fue cargado en la venta actual.');
                    buscar();
                }
                return Promise.resolve({ ok: false, msg: 'Ya cargado.' });
            }

            return $.ajax({
                url: window.api?.venta?.obtenerExpendioPos,
                type: 'GET',
                dataType: 'json',
                cache: false,
                data: { idExpendio: id }
            }).then(function (resp) {
                if (!resp || resp.ok === false) {
                    const msg = (resp && resp.msg ? String(resp.msg) : '').trim();
                    const msgNormalizado = msg.toUpperCase();
                    const noDisponible = msgNormalizado.indexOf('NO EXISTE') >= 0
                        || msgNormalizado.indexOf('ASIGNAD') >= 0;

                    if (!silent) {
                        showMessage('warning', noDisponible
                            ? 'El expendio ya fue cargado o asignado y no está disponible para esta venta.'
                            : (msg || 'No se pudo cargar el expendio.'));
                    }
                    return { ok: false, msg: msg };
                }

                const expendio = resp.expendio || {};
                const lineas = resp.lineas || [];

                if (expendio.asignado === true) {
                    if (!silent) {
                        showMessage('warning', 'El expendio ya fue cargado o asignado y no está disponible para esta venta.');
                        buscar();
                    }
                    return { ok: false, msg: 'Asignado.' };
                }

                if (!lineas.length) {
                    if (!silent) showMessage('warning', 'El expendio no tiene líneas para cargar.');
                    return { ok: false, msg: 'Sin líneas.' };
                }

                lineas.forEach(function (detalle) {
                    POSState.addLinea(buildLinea(expendio, detalle));
                });

                POSState.addExpendio?.(id);
                options.renderTable(POSState.getLineas());
                options.recalculateTotal();
                options.updateSaleState();
                options.updateCartSummary?.();
                options.beep?.();
                navigator.vibrate?.(80);
                updateRemoveButton();

                const observacion = (expendio.observaciones || '').trim();
                if (observacion) {
                    expendioObservaciones.push({ idExpendio: id, observacion: observacion });
                }

                return { ok: true, observacion: observacion };
            }, function (xhr) {
                if (!silent) showMessage('danger', extractAjaxError(xhr, 'No se pudo cargar el expendio seleccionado.'));
                return { ok: false, msg: 'Error de red.' };
            });
        }

        function cargarExpendio(idExpendio) {
            cargarExpendioInterno(idExpendio, false).then(function (resultado) {
                if (!resultado || !resultado.ok) return;

                $('#inputCodigo').val('');
                options.showWaiting?.();
                $('#modalExpendiosPOS').modal('hide');

                if (resultado.observacion) {
                    // Se dispara en CADA carga que traiga observacion, no solo
                    // la primera de la venta -- el modal siempre muestra el
                    // acumulado completo hasta este momento.
                    abrirModalObservacionesExpendio();
                } else {
                    options.focusCodigo?.();
                }
            });
        }

        // "Cargar todos" (pedido explicito del usuario, con confirmacion previa -- ver
        // docs/DECISIONS.md): carga secuencialmente (una request a la vez, no en paralelo, para
        // no saturar al servidor ni pisar el estado del carrito) todos los expendios
        // actualmente visibles en la tabla que todavia se puedan cargar (no asignados, no ya
        // cargados en esta venta). Al terminar, refresca la busqueda, muestra un resumen y abre
        // el modal de observaciones acumuladas una sola vez si hizo falta.
        function cargarTodosVisibles() {
            // Validacion 2026-09-06 (ver docs/DECISIONS.md): visibleItems es la lista PLANA de
            // BuscarExpendiosPOS (una fila por cada linea/producto), asi que sin deduplicar por
            // idExpendio un expendio con 3 lineas contaba 3 veces -- el contador de "Cargando N
            // expendio(s)..." y el resumen final mentian (mostraban lineas, no expendios), y se
            // disparaban N-1 requests redundantes que cargarExpendioInterno descartaba en
            // silencio por "ya cargado". Agrupar primero asegura que la cuenta y las requests
            // sean por EXPENDIO real, uno por uno, no por linea.
            const candidatos = agruparPorExpendio(visibleItems || [])
                .filter(function (grupo) { return grupo.cabecera.cargadoEnVentaActual !== true && grupo.cabecera.asignado !== true; })
                .map(function (grupo) { return grupo.idExpendio; })
                .filter(function (id) { return id > 0; });

            if (!candidatos.length) {
                showMessage('info', 'No hay expendios disponibles para cargar con los filtros actuales.');
                return;
            }

            showMessage('info', 'Cargando ' + candidatos.length + ' expendio(s)...');

            let cargados = 0;
            let huboObservacion = false;

            function siguiente(index) {
                if (index >= candidatos.length) {
                    $('#modalExpendiosPOS').modal('hide');
                    buscar();

                    if (huboObservacion) {
                        abrirModalObservacionesExpendio();
                    } else {
                        options.focusCodigo?.();
                    }

                    if (window.Swal) {
                        window.Swal.fire({
                            icon: cargados > 0 ? 'success' : 'warning',
                            title: cargados > 0 ? 'Expendios cargados' : 'No se cargó ningún expendio',
                            text: cargados + ' de ' + candidatos.length + ' expendio(s) se cargaron a la venta.'
                        });
                    }
                    return;
                }

                cargarExpendioInterno(candidatos[index], true).then(function (resultado) {
                    if (resultado && resultado.ok) {
                        cargados++;
                        if (resultado.observacion) huboObservacion = true;
                    }
                    siguiente(index + 1);
                });
            }

            siguiente(0);
        }

        function buildTextoObservacionesAcumuladas() {
            // Mas antiguo primero, una por linea -- el orden en que se fueron
            // cargando los expendios a esta venta.
            return expendioObservaciones.map(function (item) { return item.observacion; }).join('\n');
        }

        function abrirModalObservacionesExpendio() {
            const $textarea = $('#txtObservacionesExpendio');
            const $switch = $('#chkEditarObservacionesExpendio');

            $switch.prop('checked', false);
            $textarea.val(buildTextoObservacionesAcumuladas()).prop('readonly', true);

            $('#modalObservacionesExpendio').modal('show');
        }

        function mergeObservacionesEnComentario(textoBloque) {
            let base = POSState.getObservaciones() || '';

            // Si ya habiamos agregado un bloque antes (ej. al cargar un expendio
            // anterior en esta misma venta), lo sacamos primero para no duplicarlo
            // -- el bloque nuevo (acumulado y actualizado) lo reemplaza.
            if (ultimoBloqueObservacionesInsertado && base.indexOf(ultimoBloqueObservacionesInsertado) >= 0) {
                base = base.replace(ultimoBloqueObservacionesInsertado, '').replace(/\n{3,}/g, '\n\n').trim();
            }

            const merged = base ? (base + '\n\n' + textoBloque) : textoBloque;
            ultimoBloqueObservacionesInsertado = textoBloque;

            return merged.slice(0, MAX_COMENTARIO_VENTA);
        }

        function agregarObservacionesAComentarioVenta() {
            const textoBloque = ($('#txtObservacionesExpendio').val() || '').trim();
            if (!textoBloque) {
                $('#modalObservacionesExpendio').modal('hide');
                return;
            }

            POSState.setObservaciones(mergeObservacionesEnComentario(textoBloque));
            options.updateComentarioBadge?.();
            $('#modalObservacionesExpendio').modal('hide');
        }

        function bindObservacionesModalEvents() {
            $(document)
                .off('change.posObsExpendioEditar', '#chkEditarObservacionesExpendio')
                .on('change.posObsExpendioEditar', '#chkEditarObservacionesExpendio', function () {
                    $('#txtObservacionesExpendio').prop('readonly', !this.checked);
                })
                .off('click.posObsExpendioAgregar', '#btnAgregarObservacionExpendioAVenta')
                .on('click.posObsExpendioAgregar', '#btnAgregarObservacionExpendioAVenta', agregarObservacionesAComentarioVenta);

            $('#modalObservacionesExpendio')
                .off('keydown.posObsExpendio')
                .on('keydown.posObsExpendio', function (e) {
                    if (e.key !== 'Enter') return;
                    // Si el vendedor esta editando el texto, Enter inserta un
                    // salto de linea normal en vez de cerrar el modal.
                    if (e.target && e.target.id === 'txtObservacionesExpendio' && !$('#txtObservacionesExpendio').prop('readonly')) {
                        return;
                    }

                    e.preventDefault();
                    $('#modalObservacionesExpendio').modal('hide');
                })
                .off('hidden.bs.modal.posObsExpendio')
                .on('hidden.bs.modal.posObsExpendio', function () {
                    if (!$('.modal.show').length) {
                        options.focusCodigo?.();
                    }
                });
        }

        function quitarExpendios() {
            const ids = getIdsActuales();
            if (!ids.length) {
                showMessage('info', 'La venta actual no tiene expendios cargados.');
                return;
            }

            if (!window.Swal) {
                removeExpendiosNow();
                return;
            }

            window.Swal.fire({
                icon: 'warning',
                title: '¿Quitar expendios cargados?',
                text: 'Se eliminarán del carrito las líneas cargadas desde expendios.',
                showCancelButton: true,
                confirmButtonText: 'Sí',
                cancelButtonText: 'No'
            }).then(function (result) {
                if (!result.isConfirmed) return;
                removeExpendiosNow();
            });
        }

        function removeExpendiosNow() {
            const lineas = (POSState.getLineas() || []).slice();
            lineas.forEach(function (linea) {
                if ((parseInt(linea && linea.idExpendio, 10) || 0) > 0) {
                    POSState.removeLinea(linea.index);
                }
            });

            POSState.setListaExpendios?.([]);
            options.renderTable(POSState.getLineas());
            options.recalculateTotal();
            options.updateSaleState();
            options.updateCartSummary?.();
            updateRemoveButton();
            buscar();
        }

        function open() {
            resetFiltros();
            clearRemoteFiltersDirty();
            showMessage(null, '');
            updateRemoveButton();

            const abierto = window.POSGuard
                ? window.POSGuard.requestModalOpen('expendios', function () {
                    $('#modalExpendiosPOS').modal('show');
                })
                : true;

            if (!abierto) return;

            setTimeout(function () {
                $('#expFiltroTexto').focus().select();
            }, 120);

            buscar();
        }

        function bindEvents() {
            if (window.POSGuard) {
                window.POSGuard.bindModal('#modalExpendiosPOS', 'expendios');
            }

            $(document)
                .off('shown.bs.modal.posExpendiosFocus', '#modalExpendiosPOS')
                .on('shown.bs.modal.posExpendiosFocus', '#modalExpendiosPOS', function () {
                    $('#expFiltroTexto').trigger('focus').select();
                })
                .off('click.posExpendiosBuscar', '#btnBuscarExpendiosPOS')
                .on('click.posExpendiosBuscar', '#btnBuscarExpendiosPOS', buscar)
                .off('click.posExpendiosCargar', '.btnCargarExpendioPOS')
                .on('click.posExpendiosCargar', '.btnCargarExpendioPOS', function () {
                    cargarExpendio($(this).data('id-expendio'));
                })
                .off('click.posExpendiosVerObs', '.btnVerObservacionExpendio')
                .on('click.posExpendiosVerObs', '.btnVerObservacionExpendio', function (e) {
                    e.stopPropagation();
                    const texto = $(this).data('observacion') || '';
                    if (window.Swal) {
                        window.Swal.fire({ icon: 'info', title: 'Observación del expendio', text: texto });
                    } else {
                        alert(texto);
                    }
                })
                .off('click.posExpendiosQuitar', '#btnQuitarExpendiosPOS')
                .on('click.posExpendiosQuitar', '#btnQuitarExpendiosPOS', quitarExpendios)
                .off('keydown.posExpendiosFecha', '#expFiltroFechaDesde')
                .on('keydown.posExpendiosFecha', '#expFiltroFechaDesde', function (e) {
                    if (e.key !== 'Enter') return;
                    e.preventDefault();
                    buscar();
                })
                .off('change.posExpendiosFecha', '#expFiltroFechaDesde')
                .on('change.posExpendiosFecha', '#expFiltroFechaDesde', markRemoteFiltersDirty)
                .off('change.posExpendiosFechaHasta', '#expFiltroFechaHasta')
                .on('change.posExpendiosFechaHasta', '#expFiltroFechaHasta', markRemoteFiltersDirty)
                .off('change.posExpendiosSucursal', '#expFiltroSucursal')
                .on('change.posExpendiosSucursal', '#expFiltroSucursal', function () {
                    actualizarColumnaSucursal();
                    markRemoteFiltersDirty();
                })
                .off('input.posExpendiosTexto', '#expFiltroTexto')
                .on('input.posExpendiosTexto', '#expFiltroTexto', applyLocalFilters)
                .off('keydown.posExpendiosTexto', '#expFiltroTexto')
                .on('keydown.posExpendiosTexto', '#expFiltroTexto', function (e) {
                    if (e.key !== 'Enter') return;
                    e.preventDefault();

                    const id = getFirstVisibleExpendioId();
                    if (id > 0) {
                        cargarExpendio(id);
                    }
                })
                .off('change.posExpendiosEstado', '#expFiltroEstado')
                .on('change.posExpendiosEstado', '#expFiltroEstado', applyLocalFilters)
                .off('change.posExpendiosAvanzados', '#expSwitchFiltrosAvanzados')
                .on('change.posExpendiosAvanzados', '#expSwitchFiltrosAvanzados', function () {
                    $('#panelFiltrosAvanzadosExpendiosPOS').toggleClass('d-none', !this.checked);
                })
                .off('click.posExpendiosLimpiar', '#btnLimpiarFiltrosExpendiosPOS')
                .on('click.posExpendiosLimpiar', '#btnLimpiarFiltrosExpendiosPOS', function () {
                    resetFiltros();
                    buscar();
                })
                .off('change.posExpendiosCargarTodos', '#chkCargarTodosExpendiosPOS')
                .on('change.posExpendiosCargarTodos', '#chkCargarTodosExpendiosPOS', function () {
                    const $chk = $(this);
                    if (!$chk.is(':checked')) return;

                    const cantidad = (visibleItems || []).filter(function (item) {
                        return item && item.cargadoEnVentaActual !== true && item.asignado !== true;
                    }).length;

                    if (!cantidad) {
                        showMessage('info', 'No hay expendios disponibles para cargar con los filtros actuales.');
                        $chk.prop('checked', false);
                        return;
                    }

                    const confirmar = function () {
                        $chk.prop('checked', false);
                        cargarTodosVisibles();
                    };

                    const cancelar = function () {
                        $chk.prop('checked', false);
                    };

                    if (window.Swal) {
                        window.Swal.fire({
                            icon: 'warning',
                            title: '¿Cargar todos los expendios?',
                            text: 'Se van a cargar ' + cantidad + ' expendio(s) mostrados a la venta actual.',
                            showCancelButton: true,
                            confirmButtonText: 'Sí, cargar todos',
                            cancelButtonText: 'Cancelar'
                        }).then(function (result) {
                            if (result.isConfirmed) confirmar();
                            else cancelar();
                        });
                    } else if (confirm('¿Cargar ' + cantidad + ' expendio(s) mostrados a la venta actual?')) {
                        confirmar();
                    } else {
                        cancelar();
                    }
                });
        }

        const api = {
            init: function () {
                bindEvents();
                bindObservacionesModalEvents();
                updateRemoveButton();
            },
            open: open,
            buscar: buscar,
            cargarExpendio: cargarExpendio,
            syncAssignedFromCart: syncAssignedFromCart,
            quitarExpendios: quitarExpendios,
            // Se llama al finalizar una venta con exito (ver POS.cshtml, mostrarModalPostVenta) --
            // pedido explicito del usuario para que el modal vuelva a su estado inicial en la
            // proxima venta, en vez de arrastrar los filtros de la venta anterior.
            resetFiltros: resetFiltros
        };

        return api;
    }

    window.POSExpendios = {
        create: createPOSExpendios
    };
})(window, window.jQuery);
