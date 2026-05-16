(function (window, $) {
    'use strict';

    function createPOSExpendios(options) {
        const POSState = options.POSState;
        let allItems = [];
        let visibleItems = [];
        let remoteFiltersDirty = false;

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

        function setDefaultFechaDesde() {
            const $input = $('#expFiltroFechaDesde');
            if (!$input.length) return;

            const now = new Date();
            const startOfDay = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
            $input.val(toDateTimeLocalValue(startOfDay));
        }

        function parseFechaDesde() {
            const raw = ($('#expFiltroFechaDesde').val() || '').trim();
            if (!raw) return null;

            const value = new Date(raw);
            return isNaN(value.getTime()) ? null : value;
        }

        function calculateUltimosMinutos() {
            const fechaDesde = parseFechaDesde();
            if (!fechaDesde) return 300;

            const diffMs = new Date().getTime() - fechaDesde.getTime();
            if (diffMs <= 0) return 1;

            return Math.max(1, Math.ceil(diffMs / 60000));
        }

        function normalizeText(value) {
            return (value || '')
                .toString()
                .toUpperCase()
                .normalize('NFD')
                .replace(/[\u0300-\u036f]/g, '');
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

        function renderRows(items) {
            const $tbody = $('#tablaExpendiosPOS tbody');
            if (!$tbody.length) return;

            visibleItems = Array.isArray(items) ? items.slice() : [];

            $tbody.empty();

            if (!items || !items.length) {
                $tbody.append('<tr><td colspan="9" class="text-center text-muted py-4">No se encontraron expendios para esos filtros.</td></tr>');
                return;
            }

            items.forEach(function (item) {
                const estadoBadge = item.cargadoEnVentaActual
                    ? '<span class="badge badge-info">En venta actual</span>'
                    : (item.asignado ? '<span class="badge badge-secondary">Asignado</span>' : '<span class="badge badge-warning">Pendiente</span>');

                const disabled = item.cargadoEnVentaActual || (item.asignado && !item.cargadoEnVentaActual);
                const btnClass = disabled ? 'btn-outline-secondary' : 'btn-outline-primary';
                const btnLabel = item.cargadoEnVentaActual ? 'Cargado' : 'Cargar';

                $tbody.append(
                    '<tr data-id-expendio="' + item.idExpendio + '">' +
                    '<td>' + formatDate(item.fechaExpendio) + '</td>' +
                    '<td>' + (item.hora || '') + '</td>' +
                    '<td><strong>' + item.idExpendio + '</strong><div>' + estadoBadge + '</div></td>' +
                    '<td>' + (item.identificacionExpendio || '') + '</td>' +
                    '<td>' + (item.sector || '') + '</td>' +
                    '<td><div><strong>' + (item.producto || '') + '</strong></div><div class="text-muted small">Cod. ' + (item.codigo || 0) + '</div></td>' +
                    '<td class="text-right">' + formatKg(item.cantKg) + '</td>' +
                    '<td>' + (item.vendedor || '') + '</td>' +
                    '<td class="text-center">' +
                    '<button type="button" class="btn btn-sm ' + btnClass + ' btnCargarExpendioPOS" data-id-expendio="' + item.idExpendio + '"' + (disabled ? ' disabled' : '') + '>' + btnLabel + '</button>' +
                    '</td>' +
                    '</tr>'
                );
            });
        }

        function collectRemoteFilters() {
            return {
                ultimosMinutos: calculateUltimosMinutos(),
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
                showMessage(remoteFiltersDirty ? 'info' : 'info', remoteFiltersDirty
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
            $('#tablaExpendiosPOS tbody').html('<tr><td colspan="8" class="text-center text-muted py-4">Buscando expendios...</td></tr>');

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
                balanza: detalle.balanza === true
            };
        }

        function cargarExpendio(idExpendio) {
            const id = parseInt(idExpendio, 10) || 0;
            if (id <= 0) return;

            if (getIdsActuales().indexOf(id) >= 0) {
                showMessage('warning', 'El expendio ya fue cargado en la venta actual.');
                buscar();
                return;
            }

            $.ajax({
                url: window.api?.venta?.obtenerExpendioPos,
                type: 'GET',
                dataType: 'json',
                cache: false,
                data: { idExpendio: id },
                success: function (resp) {
                    if (!resp || resp.ok === false) {
                        const msg = (resp && resp.msg ? String(resp.msg) : '').trim();
                        const msgNormalizado = msg.toUpperCase();
                        const noDisponible = msgNormalizado.indexOf('NO EXISTE') >= 0
                            || msgNormalizado.indexOf('ASIGNAD') >= 0;

                        showMessage('warning', noDisponible
                            ? 'El expendio ya fue cargado o asignado y no está disponible para esta venta.'
                            : (msg || 'No se pudo cargar el expendio.'));
                        return;
                    }

                    const expendio = resp.expendio || {};
                    const lineas = resp.lineas || [];

                    if (expendio.asignado === true) {
                        showMessage('warning', 'El expendio ya fue cargado o asignado y no está disponible para esta venta.');
                        buscar();
                        return;
                    }

                    if (!lineas.length) {
                        showMessage('warning', 'El expendio no tiene líneas para cargar.');
                        return;
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
                    $('#inputCodigo').val('');
                    options.showWaiting?.();
                    updateRemoveButton();

                    $('#modalExpendiosPOS').modal('hide');
                    options.focusCodigo?.();
                },
                error: function (xhr) {
                    showMessage('danger', extractAjaxError(xhr, 'No se pudo cargar el expendio seleccionado.'));
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
            setDefaultFechaDesde();
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
                .on('change.posExpendiosEstado', '#expFiltroEstado', applyLocalFilters);
        }

        const api = {
            init: function () {
                bindEvents();
                updateRemoveButton();
            },
            open: open,
            buscar: buscar,
            cargarExpendio: cargarExpendio,
            syncAssignedFromCart: syncAssignedFromCart,
            quitarExpendios: quitarExpendios
        };

        return api;
    }

    window.POSExpendios = {
        create: createPOSExpendios
    };
})(window, window.jQuery);
