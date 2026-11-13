(function () {
    // $foco (opcional): campo que recibe el foco al CERRAR el aviso. Swal, al cerrarse, devuelve el foco a lo que lo tenia antes (el boton
    // "+"), asi que un .focus() hecho mientras el aviso esta abierto se perdia (pedido 2026-11-04: volver al campo de kilos).
    function showAlert(icon, title, text, $foco) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            var aviso = Swal.fire({ icon: icon, title: title, text: text, returnFocus: !$foco });
            if ($foco && aviso && typeof aviso.then === 'function') {
                aviso.then(function () { $foco.trigger('focus').trigger('select'); });
            }
        } else {
            alert(text);
            if ($foco) $foco.trigger('focus').trigger('select');
        }
    }

    function toInt(value) {
        var n = parseInt(value, 10);
        return isNaN(n) ? 0 : n;
    }

    function parseDecimal(value) {
        if (value === null || value === undefined) return { ok: false, value: 0 };

        var text = String(value).trim();
        if (!text) return { ok: false, value: 0 };

        text = text.replace(/\s/g, '');

        var lastComma = text.lastIndexOf(',');
        var lastDot = text.lastIndexOf('.');
        var decimalSep = '';

        if (lastComma >= 0 && lastDot >= 0) {
            decimalSep = lastComma > lastDot ? ',' : '.';
        } else if (lastComma >= 0) {
            decimalSep = ',';
        } else if (lastDot >= 0) {
            decimalSep = '.';
        }

        var normalized = '';
        var decimalIndex = decimalSep ? text.lastIndexOf(decimalSep) : -1;

        for (var i = 0; i < text.length; i++) {
            var ch = text.charAt(i);
            if (ch >= '0' && ch <= '9') {
                normalized += ch;
            } else if (ch === '-' && normalized.length === 0) {
                normalized += ch;
            } else if ((ch === ',' || ch === '.') && i === decimalIndex) {
                normalized += '.';
            }
        }

        if (!normalized || normalized === '-' || normalized === '.' || normalized === '-.') {
            return { ok: false, value: 0 };
        }

        var n = parseFloat(normalized);
        return { ok: !isNaN(n), value: isNaN(n) ? 0 : n };
    }

    function toFloat(value) {
        return parseDecimal(value).value;
    }

    function isIntegerDecimal(value) {
        var parsed = parseDecimal(value);
        if (!parsed.ok) return false;

        return Math.abs(parsed.value - Math.round(parsed.value)) < 0.000001;
    }

    function formatKg(value) {
        return toFloat(value).toLocaleString('es-AR', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
    }

    function formatDecimalForPost(value) {
        var parsed = parseDecimal(value);
        if (!parsed.ok) return '';
        return String(parsed.value).replace('.', ',');
    }

    function formatInt(value) {
        return toInt(value).toLocaleString('es-AR', { maximumFractionDigits: 0 });
    }

    function initIndex() {
        var $page = $('[data-movimientos-index="1"]');
        if (!$page.length) return;

        var detalleUrl = $page.data('detalle-url');

        function loadDetalle(id, $row, done) {
            if ($row.attr('data-loaded') === 'true') {
                if (typeof done === 'function') done();
                return;
            }

            $.get(detalleUrl, { id: id })
                .done(function (html) {
                    $row.find('.js-detalle-container').html(html);
                    $row.attr('data-loaded', 'true');
                    if (typeof done === 'function') done();
                })
                .fail(function () {
                    $row.find('.js-detalle-container').html('<div class="text-danger">No se pudo cargar el detalle.</div>');
                    if (typeof done === 'function') done();
                });
        }

        function expandRow($btn) {
            var id = $btn.data('id');
            var target = $btn.data('target');
            var $row = $(target);
            if (!$row.length) return;

            loadDetalle(id, $row, function () {
                $row.removeClass('d-none');
                $btn.find('i').removeClass('fa-angle-down').addClass('fa-angle-up');
                $btn.attr('data-open', 'true');
            });
        }

        function collapseRow($btn) {
            var target = $btn.data('target');
            $(target).addClass('d-none');
            $btn.find('i').removeClass('fa-angle-up').addClass('fa-angle-down');
            $btn.attr('data-open', 'false');
        }

        $(document).on('click.movimientosIndex', '.js-toggle-detalle', function () {
            var $btn = $(this);
            if ($btn.attr('data-open') === 'true') collapseRow($btn);
            else expandRow($btn);
        });

        $(document).on('dblclick.movimientosIndex', 'tr[data-movimiento-row]', function (e) {
            if ($(e.target).closest('a, button').length) return;
            $(this).find('.js-toggle-detalle').trigger('click');
        });

        $('#verDetalles').on('change.movimientosIndex', function () {
            var checked = $(this).is(':checked');
            $('.js-toggle-detalle').each(function () {
                var $btn = $(this);
                if (checked) expandRow($btn);
                else collapseRow($btn);
            });
        });

        if ($('#verDetalles').is(':checked')) {
            $('#verDetalles').trigger('change');
        }

        // Boton "Imprimir" nuevo en la columna Acciones (cuarta ronda de pedidos, 2026-09-10, ver
        // docs/DECISIONS.md "Batch 7"). Trae pdfUrl/imprimirUrl/whatsappTexto recien al click
        // (MovimientosController.ImprimirInfo) -- el listado arma la tabla desde un DataTable
        // liviano sin entidades cargadas por fila, calcularlos para cada fila visible seria un
        // N+1 real.
        var imprimirInfoUrl = $page.data('imprimir-info-url');

        $(document).on('click.movimientosIndex', '.js-imprimir-movimiento', function () {
            var id = $(this).data('id');
            if (!imprimirInfoUrl || !id) return;

            $.getJSON(imprimirInfoUrl, { id: id })
                .done(function (resp) {
                    if (!resp || !resp.ok) {
                        showAlert('warning', 'Movimiento', 'No se pudo preparar la impresión de este movimiento.');
                        return;
                    }

                    if (!window.PostMovimientoModal || typeof window.PostMovimientoModal.open !== 'function') return;

                    window.PostMovimientoModal.open({
                        redirectUrl: '',
                        pdfUrl: resp.pdfUrl,
                        imprimirUrl: resp.imprimirUrl,
                        imprimirPayloadUrl: resp.imprimirPayloadUrl,
                        whatsappTexto: resp.whatsappTexto,
                        stayOnPage: true,
                        titulo: 'Imprimir movimiento'
                    });
                })
                .fail(function () {
                    showAlert('error', 'Movimiento', 'No se pudo preparar la impresión de este movimiento.');
                });
        });
    }

    function initEdit() {
        var $page = $('[data-movimiento-page="1"]');
        if (!$page.length) return;

        var config = window.movimientosConfig || {};
        var state = {
            lines: Array.isArray(window.movimientoLineasIniciales) ? window.movimientoLineasIniciales.slice() : [],
            draftTimer: null,
            saving: false,
            enviando: false,    // hay un POST de guardado en vuelo: otro submit se ignora (ver #formMovimiento submit)
            productoTimer: null,
            productoRequestSeq: 0,
            readOnly: !!config.soloLecturaInicial,
            sortKey: 'ordenIngreso',
            // En el celular arranca en "ultimos primero": lo recien cargado queda arriba, cerca del formulario (2026-10-27,
            // pedido del usuario; en desktop sigue el orden de carga). El orden guardado no depende de esto (ver renderLines).
            sortDirection: (window.matchMedia && window.matchMedia('(max-width: 767.98px)').matches) ? 'desc' : 'asc',
            lineSequence: 0,
            searchText: '',
            // Borrador en servidor (ver docs/DECISIONS.md "Borradores de Compras/Stock/Movimientos/
            // Embutidos"): se completa abajo (crearBorradorGenerico), null si esta deshabilitado
            // (ahi se sigue con localStorage/CapturaRespaldo de siempre).
            borradorGenerico: null
        };
        state.borradorGenerico = crearBorradorGenerico(config.borradorGenerico);

        var $codigo = $('#txtCodigoProducto');
        var $productoId = $('#txtProductoId');
        var $productoNombre = $('#txtProductoNombre');
        var $productoPesable = $('#txtProductoPesable');
        var $productoTipo = $('#txtProductoTipo');
        var $productoPromedio = $('#txtProductoPromedio');
        var $cantUnidad = $('#txtCantUnidad');
        var $cantKgs = $('#txtCantKgs');
        var $balanza = $('#chkBalanzaLinea');
        var $permitirWrap = $('#wrapPermitirIngreso');
        var $permitir = $('#chkPermitirIngreso');
        var $warning = $('#movimientoWarning');
        var $btnImprimirMovimiento = $('#btnImprimirMovimiento');
        var $observaciones = $('#Observaciones');
        var balanzaDisponible = false;
        var balanzaManualDesactivada = false;
        var balanzaClientStarted = false;
        var balanzaUltimaLectura = null;

        function esAccionVisible($button) {
            return !!($button && $button.length && !$button.prop('disabled') && !$button.hasClass('d-none'));
        }

        function getPrimaryActionButton() {
            var $modify = $('#btnHabilitarEdicionMovimiento');
            var $save = $('#btnGuardarMovimiento');

            if (esAccionVisible($modify)) return $modify;
            if (esAccionVisible($save)) return $save;
            return $();
        }

        function syncPrimaryAction() {
            var esEdicion = toInt($('#IdMovimiento').val()) > 0;
            if (!esEdicion) return;

            var $modify = $('#btnHabilitarEdicionMovimiento');
            var $save = $('#btnGuardarMovimiento');
            if (!$modify.length || !$save.length) return;

            if (state.readOnly) {
                $modify
                    .html('<i class="fas fa-edit mr-1"></i> Modificar')
                    .toggleClass('d-none', false)
                    .attr('title', 'Modificar (Alt+Enter)');

                $save
                    .toggleClass('d-none', true)
                    .attr('title', 'Guardar movimiento (Alt+Enter)');
                return;
            }

            $modify.toggleClass('d-none', true);
            $save
                .toggleClass('d-none', false)
                .attr('title', 'Guardar movimiento (Alt+Enter)');
        }

        function productoEsPesable() {
            return String($productoPesable.val() || '').toLowerCase() === 'true';
        }

        function syncReadOnlyUi() {
            var hayProducto = toInt($productoId.val()) > 0;
            $('#tablaLineasMovimiento .js-remove-line').prop('disabled', !!state.readOnly);
            // En modo ver (solo lectura) el tacho no se puede usar: en escritorio esa columna muestra la hora de carga de cada linea.
            $('#tablaLineasMovimiento').toggleClass('mov-solo-ver', !!state.readOnly);
            $('#tablaLineasMovimiento thead th').last().text(state.readOnly ? 'Hora' : 'Acción');
            $cantKgs.prop('readonly', !!($balanza.is(':checked') && balanzaDisponible && !balanzaManualDesactivada && (!hayProducto || productoEsPesable())));
            syncPrimaryAction();
        }

        // Aviso de "ya abierto en otra pestaña" (documento-abierto.js): se arma mas abajo, antes de aplicarTraspaso().
        var documento = null;

        // Clave del borrador local. Un Nuevo usa una clave POR PESTAÑA (movimiento_draft_new_<instancia>) para que dos Nuevo abiertos no
        // se pisen; un movimiento existente conserva su clave por id.
        function getDraftKey() {
            var base = config.draftKey || '';
            if (documento && base && base.slice(-4) === '_new') return documento.claveBorradorLocal(base);
            return base;
        }

        function readDraft() {
            var key = getDraftKey();
            if (!key || !window.localStorage) return null;
            try {
                var raw = window.localStorage.getItem(key);
                return raw ? JSON.parse(raw) : null;
            } catch (err) {
                return null;
            }
        }

        function hideDraftBanner() {
            $page.find('#movimientoDraftBanner').addClass('d-none');
        }

        function showDraftBanner() {
            $page.find('#movimientoDraftBanner').removeClass('d-none');
        }

        function clearDraft() {
            var key = getDraftKey();
            if (!key || !window.localStorage) return;
            window.localStorage.removeItem(key);
            hideDraftBanner();
        }

        function buildDraft() {
            return {
                idSucursalOrigen: $('#IdSucursalOrigen').val(),
                idSucursalDestino: $('#IdSucursalDestino').val(),
                fechaMovimiento: $('#FechaMovimiento').val(),
                fechaManual: $fechaManual.val() === 'true',
                tiempoEnvioMinutos: $envioMinutos.length ? $envioMinutos.val() : '',
                observaciones: $observaciones.val(),
                currentLine: {
                    id: $productoId.val(),
                    codigo: $codigo.val(),
                    nombre: $productoNombre.val(),
                    tipo: $productoTipo.val(),
                    promedio: $productoPromedio.val(),
                    cantUnidad: $cantUnidad.val(),
                    cantKg: $cantKgs.val(),
                    pesoBalanza: $balanza.is(':checked'),
                    permitirIngreso: $permitir.is(':checked')
                },
                lineas: state.lines
            };
        }

        function saveDraft() {
            var key = getDraftKey();
            if (!key || !window.localStorage || state.saving) return;

            if (!state.lines.length) {
                clearDraft();
                return;
            }

            try {
                // guardadoEn: documento-abierto.js lo usa para elegir el borrador mas reciente que dejo una pestaña cerrada.
                window.localStorage.setItem(key, JSON.stringify($.extend({}, buildDraft(), { guardadoEn: Date.now() })));
            } catch (err) {
            }
        }

        // Respaldo por captura de pantalla (ver captura-respaldo.js): se omite cuando el borrador en
        // servidor esta habilitado, es un reemplazo mejor del mismo caso de uso (ver docs/DECISIONS.md
        // "Borradores de Compras/Stock/Movimientos/Embutidos").
        function capturarRespaldo(etiqueta) {
            if (state.borradorGenerico && state.borradorGenerico.estaHabilitado()) return;
            window.CapturaRespaldo && window.CapturaRespaldo.capturar(etiqueta);
        }

        function scheduleDraft() {
            // Con el borrador en servidor habilitado, ese es el unico resguardo: no se escribe en
            // localStorage.
            if (state.borradorGenerico && state.borradorGenerico.estaHabilitado()) {
                state.borradorGenerico.programarGuardado();
                return;
            }

            window.clearTimeout(state.draftTimer);
            state.draftTimer = window.setTimeout(function () {
                saveDraft();
            }, 250);
        }

        // Arma la instancia de borrador en servidor (ver borrador-generico.js) a partir del config
        // que inyecta la vista. null si esta deshabilitado.
        function crearBorradorGenerico(cfgBorrador) {
            if (!cfgBorrador || cfgBorrador.habilitado !== true) return null;

            return window.BorradorGenerico.crear({
                modulo: cfgBorrador.modulo,
                habilitado: true,
                latidoSegundos: cfgBorrador.latidoSegundos,
                urls: cfgBorrador.urls,
                obtenerIdSucursal: function () { return toInt($('#IdSucursalOrigen').val()) || cfgBorrador.idSucursal || 0; },
                obtenerIdRegistro: function () { return cfgBorrador.idRegistro || null; },
                obtenerIdOperador: function () { return cfgBorrador.idOperador || 0; },
                obtenerNombreOperador: function () { return cfgBorrador.nombreOperador || ''; },
                obtenerSnapshot: buildDraft,
                obtenerCantLineas: function () { return (state.lines || []).length; },
                obtenerResumen: function () {
                    var origen = $('#IdSucursalOrigen option:selected').text() || '';
                    var destino = $('#IdSucursalDestino option:selected').text() || '';
                    var partes = [];
                    if (origen.trim() && destino.trim()) partes.push(origen.trim() + ' → ' + destino.trim());
                    partes.push((state.lines || []).length + ((state.lines || []).length === 1 ? ' línea' : ' líneas'));
                    return partes.join(' — ');
                },
                hayFormularioCargado: function () { return (state.lines || []).length > 0; },
                finalizando: function () { return state.saving === true; },
                aplicarPayload: function (payload) { applyDraft(payload); },
                renderizarLineas: function (payload) {
                    var lineas = (payload && payload.lineas) || [];
                    return lineas.filter(function (l) { return !!l; }).map(function (l) {
                        var cant = toFloat(l.CantKg) > 0 ? formatKg(l.CantKg) + ' kg' : (toInt(l.CantUnidad) || 0) + ' un.';
                        return { codigo: l.Codigo, producto: l.Producto, cantidad: cant };
                    });
                }
            });
        }

        function showWarning(text) {
            $warning.text(text).removeClass('d-none');
        }

        function clearWarning() {
            $warning.addClass('d-none').text('');
        }

        // ---- Hora de carga de cada linea y tiempo de envio (2026-11-11, ver docs/DECISIONS.md "Movimientos: tiempo de envio, llegada y
        // hora de carga por linea") ----
        function dos(n) { return (n < 10 ? '0' : '') + n; }

        // Hora actual de la PC como "dd/MM/yyyy HH:mm" (mismo formato que la hora de carga de Stock). El servidor la lee con
        // MovimientoTiempos.LeerCreadoLinea y la corrige si el reloj de la PC estaba adelantado.
        function fechaHoraActualTexto() {
            var d = new Date();
            return dos(d.getDate()) + '/' + dos(d.getMonth() + 1) + '/' + d.getFullYear() + ' ' + dos(d.getHours()) + ':' + dos(d.getMinutes());
        }

        // Datos para mostrar la hora de carga de una linea ("dd/MM/yyyy HH:mm", vacio en las lineas anteriores al cambio).
        // otroDia = se cargo otro dia que el guardado del movimiento (solo en edicion: config.fechaReferenciaCarga, "yyyy-MM-dd"); se
        // marca en ambar y el detalle va en el tooltip (mismo criterio que Compras/Stock, Helpers/HoraLineaDetalle.cs).
        function datosHoraLinea(creadoTexto) {
            var m = /^(\d{2})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2})$/.exec(String(creadoTexto || '').trim());
            if (!m) return { hora: '', titulo: '', otroDia: false };
            var hora = m[4] + ':' + m[5];
            var ref = String(config.fechaReferenciaCarga || '');
            return {
                hora: hora,
                titulo: 'Cargado el ' + m[1] + '/' + m[2] + '/' + m[3] + ' a las ' + hora,
                otroDia: ref !== '' && (m[3] + '-' + m[2] + '-' + m[1]) !== ref
            };
        }

        var $envioMinutos = $('#TiempoEnvioMinutos');   // solo existe en un movimiento nuevo
        var $fechaManual = $('#FechaManual');

        function fechaLocalIso(d) {
            return d.getFullYear() + '-' + dos(d.getMonth() + 1) + '-' + dos(d.getDate()) + 'T' + dos(d.getHours()) + ':' + dos(d.getMinutes());
        }

        function envioMinutos() {
            var max = toInt($envioMinutos.attr('max')) || 1440;
            return Math.min(Math.max(toInt($envioMinutos.val()), 0), max);
        }

        // Movimiento nuevo: la Fecha muestra la llegada estimada (ahora + tiempo de envio). Es solo una vista previa: al guardar el
        // servidor la vuelve a calcular con SU reloj (MovimientoTiempos.CalcularLlegada). Si el usuario tipeo la fecha a mano no se toca.
        function actualizarLlegadaEstimada() {
            if (!$envioMinutos.length || $fechaManual.val() === 'true') return;
            $('#FechaMovimiento').val(fechaLocalIso(new Date(Date.now() + envioMinutos() * 60000)));
        }

        function marcarFechaManual(manual) {
            $fechaManual.val(manual ? 'true' : 'false');
        }

        function ensureLineMetadata(line, fallbackOrder) {
            if (!line || typeof line !== 'object') return line;

            if (!line.__ordenIngreso || toInt(line.__ordenIngreso) <= 0) {
                line.__ordenIngreso = fallbackOrder > 0 ? fallbackOrder : (state.lineSequence + 1);
            }

            state.lineSequence = Math.max(state.lineSequence, toInt(line.__ordenIngreso));
            return line;
        }

        function ensureAllLinesMetadata() {
            state.lineSequence = 0;
            state.lines.forEach(function (line, index) {
                ensureLineMetadata(line, index + 1);
            });
        }

        function getSortValue(line, sortKey) {
            switch (sortKey) {
                case 'ordenIngreso':
                    return toInt(line.__ordenIngreso);
                case 'codigo':
                    return toInt(line.Codigo);
                case 'producto':
                    return String(line.Producto || '').toLowerCase();
                case 'cantUnidad':
                    return toInt(line.CantUnidad);
                case 'cantKg':
                    return toFloat(line.CantKg);
                case 'pesoBalanza':
                    return line.PesoBalanza ? 1 : 0;
                case 'permitirIngreso':
                    return line.PermitirIngreso ? 1 : 0;
                default:
                    return '';
            }
        }

        function getSortedLinesWithIndex() {
            return state.lines
                .map(function (line, index) {
                    return {
                        line: line,
                        originalIndex: index
                    };
                })
                .sort(function (a, b) {
                    var left = getSortValue(a.line, state.sortKey);
                    var right = getSortValue(b.line, state.sortKey);
                    var result = 0;

                    if (typeof left === 'string' || typeof right === 'string') {
                        result = String(left).localeCompare(String(right), 'es', { numeric: true, sensitivity: 'base' });
                    } else {
                        result = left === right ? 0 : (left > right ? 1 : -1);
                    }

                    if (result === 0) {
                        result = toInt(a.line.__ordenIngreso) - toInt(b.line.__ordenIngreso);
                    }

                    return state.sortDirection === 'desc' ? result * -1 : result;
                });
        }

        function lineMatchesSearch(line, searchText) {
            var needle = $.trim(searchText || '').toLowerCase();
            if (!needle) return true;
            return String(line.Producto || '').toLowerCase().indexOf(needle) !== -1
                || String(line.Codigo || '').toLowerCase().indexOf(needle) !== -1;
        }

        function syncSortHeaders() {
            var $headers = $('#tablaLineasMovimiento thead th[data-sort-key]');
            $headers.css('cursor', 'pointer');
            $headers.removeClass('movimiento-sort-active movimiento-sort-asc movimiento-sort-desc');

            $headers.each(function () {
                var $th = $(this);
                var label = ($th.data('base-label') || $th.text() || '').trim();
                var key = String($th.data('sort-key') || '');

                if (!$th.data('base-label')) {
                    $th.data('base-label', label);
                }

                if (key === state.sortKey) {
                    $th.text(label + (state.sortDirection === 'desc' ? ' ↓' : ' ↑'));
                    $th.addClass('movimiento-sort-active ' + (state.sortDirection === 'desc' ? 'movimiento-sort-desc' : 'movimiento-sort-asc'));
                    $th.attr('title', 'Ordenado por ' + label + ' (' + (state.sortDirection === 'desc' ? 'descendente' : 'ascendente') + ')');
                } else {
                    $th.text(label);
                    $th.attr('title', 'Ordenar por ' + label);
                }
            });

            // Selector "Ordenar" del celular (#ordenLineasMovimiento, solo < 768px): refleja el orden actual. Si el orden
            // vigente no es una de sus opciones (ej. codigo descendente elegido en desktop) queda en blanco.
            var $orden = $('#ordenLineasMovimiento');
            if ($orden.length) {
                var valorOrden = state.sortKey + ':' + state.sortDirection;
                $orden.val($orden.find('option[value="' + valorOrden + '"]').length ? valorOrden : '');
            }
        }

        function ensureSortableHeaders() {
            var $theadRow = $('#tablaLineasMovimiento thead tr');
            if (!$theadRow.length) return;

            if (!$theadRow.find('th[data-sort-key="ordenIngreso"]').length) {
                $theadRow.prepend('<th style="width:60px;" data-sort-key="ordenIngreso">#</th>');
            }

            var keys = ['ordenIngreso', 'codigo', 'producto', 'cantUnidad', 'cantKg', 'pesoBalanza', 'permitirIngreso'];
            $theadRow.find('th').each(function (index) {
                if (index >= keys.length) return;
                var $th = $(this);
                if (!$th.attr('data-sort-key')) {
                    $th.attr('data-sort-key', keys[index]);
                }
            });

            syncSortHeaders();
        }

        function setProducto(producto) {
            $productoId.val(producto.id || '');
            $codigo.val(producto.codigo || '');
            $productoNombre.val(producto.nombre || '');
            $productoPesable.val(producto.pesable === true ? 'true' : 'false');
            $productoTipo.val(producto.tipo || '');
            $productoPromedio.val(producto.promedio || 0);
            if (!productoEsPesable()) {
                $cantKgs.val('');
            } else if ($balanza.is(':checked') && balanzaDisponible && balanzaUltimaLectura) {
                $cantKgs.val(balanzaUltimaLectura.pesoDisplay || balanzaUltimaLectura.pesoTexto || '');
            }
            syncReadOnlyUi();
            scheduleDraft();
        }

        function setProductoEstado(texto) {
            $productoId.val('');
            $productoNombre.val(texto || '');
            $productoTipo.val('');
            $productoPromedio.val('');
        }

        function clearProducto() {
            $productoId.val('');
            $codigo.val('');
            $productoNombre.val('');
            $productoPesable.val('');
            $productoTipo.val('');
            $productoPromedio.val('');
            $cantUnidad.val('');
            if ($balanza.is(':checked') && balanzaDisponible && balanzaUltimaLectura && !balanzaManualDesactivada) {
                $cantKgs.val(balanzaUltimaLectura.pesoDisplay || balanzaUltimaLectura.pesoTexto || '');
            } else {
                $cantKgs.val('');
            }
            $permitir.prop('checked', false);
            $permitirWrap.addClass('d-none');
            clearWarning();
        }

        function applyDraft(draft) {
            if (!draft) return;

            $('#IdSucursalOrigen').val(draft.idSucursalOrigen || $('#IdSucursalOrigen').val());
            $('#IdSucursalDestino').val(draft.idSucursalDestino || $('#IdSucursalDestino').val());
            if ($envioMinutos.length) {
                // Movimiento nuevo: la fecha del borrador solo vale si se tipeo a mano; si no, es la llegada estimada de AHORA.
                if (draft.tiempoEnvioMinutos !== undefined && draft.tiempoEnvioMinutos !== '') $envioMinutos.val(draft.tiempoEnvioMinutos);
                if (draft.fechaManual === true && draft.fechaMovimiento) {
                    $('#FechaMovimiento').val(draft.fechaMovimiento);
                    marcarFechaManual(true);
                } else {
                    marcarFechaManual(false);
                    actualizarLlegadaEstimada();
                }
            } else {
                $('#FechaMovimiento').val(draft.fechaMovimiento || $('#FechaMovimiento').val());
            }
            $observaciones.val(draft.observaciones || '');

            if (draft.currentLine) {
                $productoId.val(draft.currentLine.id || '');
                $codigo.val(draft.currentLine.codigo || '');
                $productoNombre.val(draft.currentLine.nombre || '');
                $productoTipo.val(draft.currentLine.tipo || '');
                $productoPromedio.val(draft.currentLine.promedio || '');
                $cantUnidad.val(draft.currentLine.cantUnidad || '');
                $cantKgs.val(draft.currentLine.cantKg || '');
                $balanza.prop('checked', draft.currentLine.pesoBalanza === true);
                $permitir.prop('checked', draft.currentLine.permitirIngreso === true);
            }

            state.lines = $.isArray(draft.lineas) ? draft.lineas : [];
            renderLines();
            validarRelacionCantidadKilos();
            autoResizeObservaciones();
            scheduleDraft();
        }

        // ---- Traspaso a la ventana independiente (2026-11-12, ver docs/DECISIONS.md) ----
        // Antes, "Abrir en ventana" con productos cargados avisaba "cambios sin guardar" y no abria. Ahora lo cargado VIAJA a la ventana nueva:
        // pantalla-trabajo.js llama a window.PantallaTrabajoTraspaso. preparar() deja el formulario (el mismo borrador que se guarda en
        // localStorage / servidor) en una clave temporal de localStorage y la ventana nueva la recibe por ?traspaso=<clave> y la aplica (aplicarTraspaso);
        // alAbrirOk() suelta esta pestana (sin avisar al servidor: la ventana retoma el mismo borrador, ver BorradorGenerico.soltar) y la deja
        // salir sin el aviso del navegador. Solo en un movimiento NUEVO: uno ya guardado se abre en solo lectura y habria que pasar por "Modificar".
        var PREFIJO_TRASPASO = 'carnisys-traspaso-mov-';
        var VIGENCIA_TRASPASO_MS = 24 * 60 * 60 * 1000;   // una clave que quedo sin usar (ventana que no abrio) se limpia al dia siguiente

        function limpiarTraspasosViejos() {
            try {
                var ahora = Date.now();
                for (var i = window.localStorage.length - 1; i >= 0; i--) {
                    var k = window.localStorage.key(i);
                    if (!k || k.indexOf(PREFIJO_TRASPASO) !== 0) continue;
                    var creado = toInt(k.substring(PREFIJO_TRASPASO.length).split('-')[0]);
                    if (!creado || ahora - creado > VIGENCIA_TRASPASO_MS) window.localStorage.removeItem(k);
                }
            } catch (err) { }
        }

        function aplicarTraspaso() {
            var params = new URLSearchParams(window.location.search);
            var clave = params.get('traspaso') || '';
            if (clave.indexOf(PREFIJO_TRASPASO) !== 0) return;

            var crudo = null;
            try {
                crudo = window.localStorage.getItem(clave);
                window.localStorage.removeItem(clave);   // se usa una sola vez: recargar la ventana no vuelve a pisar lo que se cargue
            } catch (err) { }

            // La clave sale de la URL para que un F5 no la arrastre.
            if (window.history && window.history.replaceState) {
                params.delete('traspaso');
                var consulta = params.toString();
                window.history.replaceState(null, '', window.location.pathname + (consulta ? '?' + consulta : '') + window.location.hash);
            }

            var paquete = null;
            try { paquete = crudo ? JSON.parse(crudo) : null; } catch (err) { paquete = null; }
            if (!paquete || !paquete.draft || state.readOnly || toInt($('#IdMovimiento').val()) > 0) return;

            aplicarPaquete(paquete);

            if (window.Swal && typeof window.Swal.fire === 'function') {
                window.Swal.fire({ toast: true, position: 'top', icon: 'success', title: 'Pasó lo que tenías cargado a esta ventana.', showConfirmButton: false, timer: 3500 });
            }
        }

        // Paquete de trabajo = {draft, clientId}: lo que viaja a la ventana independiente (arriba) y a otra pestaña (documento-abierto.js).
        function obtenerPaquete() {
            var conServidor = state.borradorGenerico && state.borradorGenerico.estaHabilitado();
            return { draft: buildDraft(), clientId: conServidor ? state.borradorGenerico.getClientId() : null };
        }

        // Aplica un paquete recibido: toma el clientId del borrador en servidor (asi la otra pestaña y esta no generan dos borradores) y
        // carga el formulario. applyDraft vuelve a programar el guardado del borrador con el clientId adoptado.
        function aplicarPaquete(paquete) {
            if (!paquete || !paquete.draft) throw new Error('El paquete de trabajo no trae borrador.');
            if (paquete.clientId && state.borradorGenerico && state.borradorGenerico.estaHabilitado()) {
                state.borradorGenerico.adoptarClientId(paquete.clientId);
            }
            applyDraft(paquete.draft);
            hideDraftBanner();
        }

        // Esta pestaña entrego su trabajo a otra (documento-abierto.js): se frena todo lo que guarda y se deja salir sin el aviso del navegador.
        function alCederTrabajo() {
            window.clearTimeout(state.draftTimer);
            if (state.borradorGenerico) {
                state.borradorGenerico.soltar();   // sin avisar al servidor: la otra pestaña retomo el mismo borrador
            } else {
                // Con borrador local la otra pestaña ya lo guardo en SU clave: el de esta quedaria como un borrador viejo que ofrecer.
                try { window.localStorage.removeItem(getDraftKey()); } catch (err) { console.warn('No se pudo limpiar el borrador local al ceder el trabajo', err); }
            }
            $('form').each(function () {
                var api = $(this).data('editPageGuardApi');
                if (api && typeof api.allowNavigation === 'function') api.allowNavigation();
            });
            if (typeof window.desactivarProteccionSalida === 'function') window.desactivarProteccionSalida();
            else window.__protegerSalida = false;
        }

        function crearDocumentoAbierto() {
            var cfgDoc = config.documentoAbierto;
            if (!cfgDoc || !window.DocumentoAbierto) return null;
            return window.DocumentoAbierto.crear({
                modulo: 'MOVIMIENTO',
                idUsuario: cfgDoc.idUsuario,
                grupo: '',
                urls: cfgDoc.urls,
                textos: { singular: 'movimiento', plural: 'movimientos' },
                resumen: function () { return { cantLineas: (state.lines || []).length }; },
                obtenerPaquete: obtenerPaquete,
                aplicarPaquete: aplicarPaquete,
                alCeder: alCederTrabajo
            });
        }

        window.PantallaTrabajoTraspaso = {
            hayCambios: function () {
                return !state.readOnly && toInt($('#IdMovimiento').val()) <= 0 &&
                    (state.lines.length > 0 || $.trim($observaciones.val() || '') !== '');
            },
            // Devuelve la clave del traspaso, o null si no se pudo (movimiento ya guardado, o localStorage no disponible): ahi se mantiene el aviso de siempre.
            preparar: function () {
                if (state.readOnly || toInt($('#IdMovimiento').val()) > 0) return null;
                limpiarTraspasosViejos();
                try {
                    var clave = PREFIJO_TRASPASO + Date.now() + '-' + Math.random().toString(36).slice(2, 8);
                    var conServidor = state.borradorGenerico && state.borradorGenerico.estaHabilitado();
                    window.localStorage.setItem(clave, JSON.stringify({
                        draft: buildDraft(),
                        clientId: conServidor ? state.borradorGenerico.getClientId() : null
                    }));
                    return clave;
                } catch (err) {
                    return null;
                }
            },
            // La ventana nueva ya se abrio: esta pestana se va al listado sin guardar ni avisar nada.
            alAbrirOk: function () {
                window.clearTimeout(state.draftTimer);
                if (state.borradorGenerico) state.borradorGenerico.soltar();
                // En el modo con localStorage el borrador queda en su clave (la misma que usa la ventana nueva): no se borra.
                $('form').each(function () {
                    var api = $(this).data('editPageGuardApi');
                    if (api && typeof api.allowNavigation === 'function') api.allowNavigation();
                });
                if (typeof window.desactivarProteccionSalida === 'function') window.desactivarProteccionSalida();
                else window.__protegerSalida = false;
            }
        };

        function focusCodigo() {
            setTimeout(function () {
                $codigo.focus();
                var input = $codigo.get(0);
                if (input && typeof input.setSelectionRange === 'function') {
                    var end = ($codigo.val() || '').toString().length;
                    // El Codigo es <input type="number">: ahi setSelectionRange lanza InvalidStateError (el foco ya quedo puesto); no es un error real.
                    try { input.setSelectionRange(end, end); } catch (e) { /* tipo sin seleccion */ }
                }
            }, 30);
        }

        function autoResizeObservaciones() {
            if (!$observaciones.length) return;
            $observaciones.css('height', 'auto');
            $observaciones.css('height', $observaciones[0].scrollHeight + 'px');
        }

        function setBalanzaDisponible(disponible) {
            balanzaDisponible = !!disponible;
            syncReadOnlyUi();
        }

        function normalizarBalanzaPayload(data) {
            if (window.CarnisysBalanzaUtils && typeof window.CarnisysBalanzaUtils.normalize === 'function') {
                return window.CarnisysBalanzaUtils.normalize(data);
            }

            return {
                ok: !!(data && data.ok),
                conectada: !!(data && data.conectada),
                peso: 0,
                pesoTexto: '0.000',
                pesoDisplay: '0.000',
                raw: data || null
            };
        }

        function renderBalanzaStatus(payload) {
            if (window.CarnisysBalanzaUtils && typeof window.CarnisysBalanzaUtils.renderStatus === 'function') {
                window.CarnisysBalanzaUtils.renderStatus('#estadoBalanzaMovimiento', '#barraBalanzaMovimiento', payload);
            }
        }

        function syncBalanzaStatusVisibility() {
            $('#balanzaStatusWrapMovimiento').toggle(!!$balanza.is(':checked'));
        }

        function aplicarLecturaBalanza(payload) {
            var normalized = normalizarBalanzaPayload(payload);
            balanzaUltimaLectura = normalized;
            setBalanzaDisponible(normalized.conectada === true);
            renderBalanzaStatus(payload);

            var hayProducto = toInt($productoId.val()) > 0;
            if (!normalized.conectada || !$balanza.is(':checked') || balanzaManualDesactivada || (hayProducto && !productoEsPesable())) {
                return;
            }

            $cantKgs.val(normalized.pesoDisplay || normalized.pesoTexto || '');
            syncReadOnlyUi();
            validarRelacionCantidadKilos();
        }

        function aplicarStatusBalanza(payload) {
            var normalized = normalizarBalanzaPayload(payload);
            setBalanzaDisponible(normalized.conectada === true);
            renderBalanzaStatus(payload);
        }

        function verificarBalanza(callback) {
            if (!window.CarnisysBalanza) {
                setBalanzaDisponible(false);
                renderBalanzaStatus(null);
                if (typeof callback === 'function') callback(false);
                return;
            }

            if (!balanzaClientStarted) {
                balanzaClientStarted = true;
                window.CarnisysBalanza.start({
                    baseUrl: 'http://127.0.0.1:5100',
                    statusIntervalMs: 3000,
                    pesoIntervalMs: 250,
                    onStatus: function (data) {
                        aplicarStatusBalanza(data);
                    },
                    onPeso: function (data) {
                        aplicarLecturaBalanza(data);
                    },
                    onError: function () {
                        setBalanzaDisponible(false);
                        renderBalanzaStatus(null);
                    }
                });
            }

            window.CarnisysBalanza.leerAhora().then(function (data) {
                var disponible = normalizarBalanzaPayload(data).conectada === true;
                aplicarLecturaBalanza(data);
                if (typeof callback === 'function') callback(disponible);
            }).catch(function () {
                setBalanzaDisponible(false);
                renderBalanzaStatus(null);
                if (typeof callback === 'function') callback(false);
            });
        }

        function verificarBalanzaInicial() {
            verificarBalanza(function (disponible) {
                if (!disponible) {
                    balanzaManualDesactivada = true;
                    $balanza.prop('checked', false);
                }
                syncBalanzaStatusVisibility();
                syncReadOnlyUi();
            });
        }

        // Foco al campo de kilos con el valor seleccionado. Funciona tambien con la balanza activa (campo de solo lectura): ahi el operador ve el
        // peso a la vista y Enter registra la linea (opcion C del recorrido con Enter, 2026-11-12).
        function enfocarKilos() {
            $cantKgs.trigger('focus').trigger('select');
        }

        // enfocar (opcional): lo pide el atajo * para que el foco quede en Kgs. cuando termina la verificacion (asincrona) de la balanza.
        function intentarActivarBalanza(enfocar) {
            verificarBalanza(function (disponible) {
                if (!disponible) {
                    $balanza.prop('checked', false);
                    // Sin balanza el * igual deja el foco en Kgs. (se tipea a mano); el aviso lo devuelve a ese campo al cerrarse.
                    showAlert('warning', 'Balanza', 'No hay balanza conectada.', enfocar ? $cantKgs : undefined);
                    return;
                }

                balanzaManualDesactivada = false;
                $balanza.prop('checked', true);
                window.CarnisysBalanza.activar();
                if (balanzaUltimaLectura && balanzaUltimaLectura.conectada) {
                    $cantKgs.val(balanzaUltimaLectura.pesoDisplay || balanzaUltimaLectura.pesoTexto || '');
                }
                syncReadOnlyUi();
                if (enfocar) enfocarKilos();
            });
        }

        function renderLines() {
            var html = '';
            var hidden = '';
            var totalItems = state.lines.length;
            var totalUnidades = 0;
            var totalKilos = 0;
            var sortedLines = getSortedLinesWithIndex();

            sortedLines.forEach(function (item) {
                var line = item.line;
                var mostrarCantUnidad = toInt(line.CantUnidad) === 0 ? '' : formatInt(line.CantUnidad);
                var kilosEnteros = isIntegerDecimal(line.CantKg);
                var ocultarKilos = line.Pesable === false && kilosEnteros && toInt(line.CantUnidad) === toInt(line.CantKg);
                var mostrarKilos = ocultarKilos ? '' : formatKg(line.CantKg);
                totalUnidades += toInt(line.CantUnidad);
                totalKilos += toFloat(line.CantKg);

                // La búsqueda solo afecta qué filas se ven: totales y hidden inputs siguen contemplando TODAS las líneas.
                // Clases cc-*: en el celular la tabla es una lista (tabla-cards-lista-edicion, wwwroot/Content/css/tabla-cards.css);
                // el # pasa a prefijo del codigo, Balanza / Permitir ingreso a iconos y el tacho a la derecha. En desktop no cambia.
                if (lineMatchesSearch(line, state.searchText)) {
                    var orden = toInt(line.__ordenIngreso);
                    var textoBalanza = line.PesoBalanza ? 'Pesado en balanza' : 'Sin balanza';
                    var textoIngreso = line.PermitirIngreso ? 'Ingreso permitido: línea autorizada aunque los kilos no coincidían' : 'Ingreso normal';
                    var horaLinea = datosHoraLinea(line.CreadoTexto);
                    var tituloHora = 'Ítem ' + orden + (horaLinea.hora ? ' · ' + horaLinea.titulo : ' · sin hora de carga (anterior al cambio)');
                    html += '<tr>'
                        + '<td class="text-center font-weight-bold cc-solo-tabla" title="' + tituloHora + '">' + orden + '</td>'
                        + '<td class="cc-l-cod"><span class="cc-solo-card">#' + orden + ' · Cód. </span>' + line.Codigo + '</td>'
                        + '<td class="cc-l-prod">' + line.Producto + '</td>'
                        + '<td class="text-right cc-l-cant">' + mostrarCantUnidad + (mostrarCantUnidad ? '<span class="cc-solo-card"> un.</span>' : '') + '</td>'
                        + '<td class="text-right cc-l-kg">' + mostrarKilos + '</td>'
                        + '<td class="text-center cc-l-bal"><span class="cc-solo-tabla">' + (line.PesoBalanza ? 'Sí' : 'No') + '</span>'
                        + '<i class="cc-solo-card fas fa-balance-scale ' + (line.PesoBalanza ? 'text-primary' : 'cc-ico-off') + '" role="img" aria-label="' + textoBalanza + '" title="' + textoBalanza + '"></i></td>'
                        + '<td class="text-center cc-l-ing"><span class="cc-solo-tabla">' + (line.PermitirIngreso ? 'Sí' : 'No') + '</span>'
                        + '<i class="cc-solo-card fas fa-user-shield ' + (line.PermitirIngreso ? 'text-success' : 'cc-ico-off') + '" role="img" aria-label="' + textoIngreso + '" title="' + textoIngreso + '"></i></td>'
                        + '<td class="cc-l-hora cc-solo-card' + (horaLinea.otroDia ? ' cc-l-hora-otro-dia' : '') + (horaLinea.hora === '' ? ' cc-vacio' : '') + '" title="' + horaLinea.titulo + '">'
                        + (horaLinea.hora ? '<i class="far fa-clock"></i>' + horaLinea.hora : '') + '</td>'
                        + '<td class="cc-l-acc"><span class="cc-solo-tabla mov-acc-hora-txt' + (horaLinea.otroDia ? ' mov-hora-otro-dia' : '') + '" title="' + tituloHora + '">' + (horaLinea.hora || '—') + '</span>'
                        + '<button type="button" class="btn btn-sm btn-outline-danger js-remove-line" data-index="' + item.originalIndex + '" title="Quitar línea" aria-label="Quitar línea"' + (state.readOnly ? ' disabled="disabled"' : '') + '><i class="fas fa-trash"></i></button></td>'
                        + '</tr>';
                }
            });

            // Hidden inputs en el ORDEN DE CARGA (state.lines), no en el de pantalla: con "ultimos primero" (celular) o al
            // ordenar por una columna, las lineas nuevas se guardarian invertidas o mezcladas (2026-10-27, ver docs/DECISIONS.md).
            state.lines.forEach(function (line, index) {
                hidden += '<input type="hidden" name="Lineas[' + index + '].IdCorteMovimiento" value="' + (line.IdCorteMovimiento || 0) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].IdCorte" value="' + (line.IdCorte || 0) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].Codigo" value="' + (line.Codigo || 0) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].Producto" value="' + $('<div>').text(line.Producto || '').html() + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].TipoProducto" value="' + $('<div>').text(line.TipoProducto || '').html() + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].PromedioProducto" value="' + formatDecimalForPost(line.PromedioProducto) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].CantUnidad" value="' + toInt(line.CantUnidad) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].CantKg" value="' + formatDecimalForPost(line.CantKg) + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].PesoBalanza" value="' + (line.PesoBalanza ? 'true' : 'false') + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].PermitirIngreso" value="' + (line.PermitirIngreso ? 'true' : 'false') + '" />';
                hidden += '<input type="hidden" name="Lineas[' + index + '].CreadoTexto" value="' + (line.CreadoTexto || '') + '" />';
            });

            if (!html) {
                html = state.lines.length
                    ? '<tr><td colspan="7" class="text-center text-muted">Sin resultados para la búsqueda.</td></tr>'
                    : '<tr><td colspan="7" class="text-center text-muted">Todavía no agregaste productos al movimiento.</td></tr>';
            }

            html = html.replace('colspan="7"', 'colspan="8"');
            $('#tablaLineasMovimiento tbody').html(html);
            $('#lineasHiddenContainer').html(hidden);
            $('#lblTotalItems').text(formatInt(totalItems));
            $('#lblTotalUnidades').text(formatInt(totalUnidades));
            $('#lblTotalKilos').text(formatKg(totalKilos));
            syncSortHeaders();
            syncReadOnlyUi();
            notificarLineas();
        }

        // Aviso a quien quiera saber que lleva el movimiento (hoy: el panel de stock del destino, movimiento-stock-destino.js): un evento con las
        // cantidades acumuladas por codigo { "123": { u: unidades, k: kilos } }. No cambia nada de la pantalla.
        function resumenDeLineas() {
            var resumen = {};
            state.lines.forEach(function (line) {
                var cod = String(line.Codigo || '').trim();
                if (!/^\d{1,18}$/.test(cod)) return;
                if (!resumen[cod]) resumen[cod] = { u: 0, k: 0 };
                resumen[cod].u += toInt(line.CantUnidad);
                resumen[cod].k += toFloat(line.CantKg);
            });
            return resumen;
        }

        function notificarLineas() {
            $(document).trigger('movimiento:lineas', [resumenDeLineas()]);
        }

        // Orden del modal de acumulados (independiente del de la tabla principal). Arranca por codigo y se reinicia cada vez que se abre.
        var acumuladosOrden = { sortKey: 'codigo', sortDirection: 'asc' };

        // Columna "Veces" y detalle por carga del modal (helper acumulados-detalle.js). Se redibuja al abrir/cerrar un detalle o filtrar.
        var acumuladosDetalle = window.AcumuladosDetalle.crear({
            modal: '#modalAcumuladosMovimiento',
            tabla: '#tablaAcumuladosMovimiento',
            alCambiar: function () { renderAcumulados(); }
        });

        // Agrupa las lineas por producto sumando unidades y kilos. "Orden" = posicion de la primera carga de ese producto (columna #);
        // se cuenta aparte porque Object.keys reordena las claves numericas por valor y no conserva el orden de insercion.
        // "Veces" = en cuantos items esta el producto y "Cargas" = cada una, con su # de la tabla principal (__ordenIngreso; si faltara, su posicion).
        function buildAcumulados() {
            var map = {};
            var siguienteOrden = 1;
            state.lines.forEach(function (line, indice) {
                var key = String(line.IdCorte || '') || String(line.Codigo || '');
                if (!map[key]) {
                    map[key] = {
                        Clave: key,
                        Orden: siguienteOrden++,
                        Codigo: line.Codigo,
                        Producto: line.Producto,
                        CantUnidad: 0,
                        CantKg: 0,
                        Veces: 0,
                        Cargas: []
                    };
                }
                map[key].CantUnidad += toInt(line.CantUnidad);
                map[key].CantKg += toFloat(line.CantKg);
                map[key].Veces++;
                map[key].Cargas.push({
                    item: toInt(line.__ordenIngreso) || (indice + 1),
                    hora: datosHoraLinea(line.CreadoTexto).hora,
                    un: toInt(line.CantUnidad),
                    kg: toFloat(line.CantKg)
                });
            });
            var valorOrden = function (item) {
                switch (acumuladosOrden.sortKey) {
                    case 'producto': return String(item.Producto || '').toLowerCase();
                    case 'cantUnidad': return item.CantUnidad;
                    case 'cantKg': return item.CantKg;
                    case 'veces': return item.Veces;
                    case 'orden': return item.Orden;
                    default: return String(item.Codigo || '');
                }
            };
            var factor = acumuladosOrden.sortDirection === 'desc' ? -1 : 1;
            return Object.keys(map).map(function (key) { return map[key]; }).sort(function (a, b) {
                var resultado = window.OrdenTabla.comparar(valorOrden(a), valorOrden(b));
                return (resultado === 0 ? a.Orden - b.Orden : resultado) * factor;
            });
        }

        function renderAcumulados() {
            var items = buildAcumulados();
            var html = '';
            // Repetidos = productos cargados en mas de un item; "Solo repetidos" muestra unicamente esos.
            var repetidos = items.filter(function (item) { return item.Veces > 1; });
            var visibles = acumuladosDetalle.soloRepetidos() ? repetidos : items;
            window.OrdenTabla.sincronizarEncabezados($('#tablaAcumuladosMovimiento thead th[data-sort-key]'), acumuladosOrden);
            acumuladosDetalle.pintarResumen(items.length, state.lines.length, repetidos.length);
            if (!visibles.length) {
                html = '<tr class="js-empty-row"><td colspan="6" class="text-center text-muted">'
                    + acumuladosDetalle.textoVacio('No hay productos cargados.') + '</td></tr>';
            } else {
                visibles.forEach(function (item) {
                    // Columnas del detalle desplegable: cada carga y, al pie, el total (igual a la fila acumulada).
                    var columnasDetalle = [
                        { titulo: 'Hora', valor: function (carga) { return carga.hora || '—'; }, total: '' },
                        { titulo: 'Cant.Un.', valor: function (carga) { return formatInt(carga.un); }, total: formatInt(item.CantUnidad) },
                        { titulo: 'Kilos', valor: function (carga) { return formatKg(carga.kg); }, total: formatKg(item.CantKg) }
                    ];
                    html += '<tr' + acumuladosDetalle.atributosFila(item.Clave, item.Veces) + '>'
                        + '<td class="text-center font-weight-bold d-none d-md-table-cell">' + item.Orden + '</td>'
                        + '<td>' + item.Codigo + '</td>'
                        + '<td>' + item.Producto + '</td>'
                        + '<td class="text-right">' + formatInt(item.CantUnidad) + '</td>'
                        + '<td class="text-right">' + formatKg(item.CantKg) + '</td>'
                        + '<td class="text-center text-nowrap">' + acumuladosDetalle.vecesHtml(item.Veces) + '</td>'
                        + '</tr>'
                        + acumuladosDetalle.filaDetalle(item.Clave, item.Producto, item.Veces, item.Cargas, columnasDetalle, 6);
                });
            }
            $('#tbodyAcumuladosMovimiento').html(html);
        }

        // En escritorio el boton "Permitir ingreso" flota arriba de Cant.Un. (CSS, position:absolute) sobre el renglon del cartel: el cartel se
        // angosta hasta el borde izquierdo del boton para no quedar tapado. Fuera de escritorio el boton va en la columna y el cartel usa todo el ancho.
        function ajustarAnchoCartel() {
            $warning.css('max-width', '');
            if ($permitirWrap.hasClass('d-none') || $warning.hasClass('d-none')) return;
            if ($permitirWrap.css('position') !== 'absolute') return;
            var limite = $permitirWrap[0].getBoundingClientRect().left - $warning[0].getBoundingClientRect().left - 8;
            if (limite > 160) $warning.css('max-width', limite + 'px');
        }
        $(window).on('resize', ajustarAnchoCartel);

        function validarRelacionCantidadKilos() {
            var promedio = toFloat($productoPromedio.val());
            var cantUnidad = toInt($cantUnidad.val());
            var cantKg = toFloat($cantKgs.val());

            $permitirWrap.addClass('d-none');
            clearWarning();

            if (promedio <= 0 || cantUnidad <= 0 || cantKg <= 0 || $permitir.is(':checked')) return true;

            var limitInferior = promedio * (cantUnidad - 1);
            var limitSuperior = promedio * (cantUnidad + 1);
            if (!(limitInferior < cantKg && cantKg < limitSuperior)) {
                $permitirWrap.removeClass('d-none');
                // 2026-11-12: el cartel dice cuanto deberia pesar (mismo rango de la regla de arriba) y que hacer, en vez del aviso generico.
                var kg = function (valor) { return valor.toLocaleString('es-AR', { minimumFractionDigits: 0, maximumFractionDigits: 2 }); };
                showWarning(cantUnidad + (cantUnidad === 1 ? ' un. debería' : ' un. deberían') + ' pesar entre ' + kg(Math.max(limitInferior, 0)) + ' y '
                    + kg(limitSuperior) + ' kg (promedio ' + kg(promedio) + ' c/u) y cargaste ' + kg(cantKg)
                    + ' kg. Corregí uno o tocá «Permitir ingreso».');
                ajustarAnchoCartel();
                return false;
            }

            return true;
        }

        function buildCurrentLine() {
            return ensureLineMetadata({
                IdCorteMovimiento: 0,
                IdCorte: toInt($productoId.val()),
                Codigo: toInt($codigo.val()),
                Producto: $productoNombre.val(),
                TipoProducto: $productoTipo.val(),
                Pesable: productoEsPesable(),
                PromedioProducto: toFloat($productoPromedio.val()),
                CantUnidad: toInt($cantUnidad.val()),
                CantKg: toFloat($cantKgs.val()),
                PesoBalanza: $balanza.is(':checked') && productoEsPesable(),
                PermitirIngreso: $permitir.is(':checked'),
                CreadoTexto: fechaHoraActualTexto()
            }, state.lineSequence + 1);
        }

        function validateCurrentLine() {
            var parsedCantKg = parseDecimal($cantKgs.val());

            if (toInt($productoId.val()) <= 0) {
                showAlert('warning', 'Movimiento', 'Seleccioná un producto válido.');
                focusCodigo();
                return false;
            }
            if (($cantUnidad.val() || '').trim() === '') {
                showAlert('warning', 'Movimiento', 'Ingresá la cantidad.', $cantUnidad);
                return false;
            }
            if (($cantKgs.val() || '').trim() === '') {
                showAlert('warning', 'Movimiento', 'Ingresá una cantidad de kilogramos mayor a cero.', $cantKgs);
                return false;
            }
            if (!parsedCantKg.ok) {
                showAlert('warning', 'Movimiento', 'Ingresá una cantidad de kilogramos válida.', $cantKgs);
                return false;
            }
            if (parsedCantKg.value <= 0) {
                showAlert('warning', 'Movimiento', 'Ingresá una cantidad de kilogramos mayor a cero.', $cantKgs);
                return false;
            }
            if (!validarRelacionCantidadKilos() && !$permitir.is(':checked')) {
                $cantUnidad.focus().select();
                return false;
            }
            return true;
        }

        function addCurrentLine() {
            if (!validateCurrentLine()) {
                window.BusquedaFeedback && window.BusquedaFeedback.beepError();
                return;
            }
            var line = buildCurrentLine();
            state.lines.push(line);
            renderLines();
            // El pitido de exito (abajo) reemplaza al alert de "Agregado correctamente" -- ya no se
            // muestra en exito, solo queda el feedback sonoro. Los warnings/errores si se siguen viendo.
            window.BusquedaFeedback && window.BusquedaFeedback.beepExito();
            capturarRespaldo('Movimiento');
            clearProducto();
            scheduleDraft();
            focusCodigo();
        }

        function permitirIngresoVisible() {
            return $permitirWrap.is(':visible') && !$permitirWrap.hasClass('d-none');
        }

        function registrarConPermitirIngreso() {
            if (!permitirIngresoVisible()) return false;
            $permitir.prop('checked', true);
            addCurrentLine();
            return true;
        }

        function buscarPorCodigo(callback, preserveFocus) {
            var codigo = toInt($codigo.val());
            if (!codigo || !config.urlBuscarProductoPorCodigo) return;
            state.productoRequestSeq += 1;
            var requestSeq = state.productoRequestSeq;
            setProductoEstado('Buscando...');

            $.get(config.urlBuscarProductoPorCodigo, { codigo: codigo })
                .done(function (resp) {
                    if (requestSeq !== state.productoRequestSeq) return;
                    if (resp && resp.ok) {
                        setProducto(resp);
                        if (preserveFocus) {
                            focusCodigo();
                        }
                        if (typeof callback === 'function') callback(true);
                    } else {
                        $productoId.val('');
                        $productoTipo.val('');
                        $productoPromedio.val('');
                        $productoNombre.val('No existe o sin coincidencia');
                        $codigo.val(codigo);
                        if (preserveFocus) {
                            focusCodigo();
                            if (typeof callback === 'function') callback(false);
                            return;
                        }
                        if (preserveFocus) {
                            showAlert('warning', 'Producto', (resp && resp.mensaje) || 'No se encontró el producto.');
                            focusCodigo();
                        }
                        if (typeof callback === 'function') callback(false);
                    }
                })
                .fail(function () {
                    if (requestSeq !== state.productoRequestSeq) return;
                    $productoId.val('');
                    $productoTipo.val('');
                    $productoPromedio.val('');
                    $productoNombre.val('No existe o sin coincidencia');
                    if (preserveFocus) {
                        focusCodigo();
                        if (typeof callback === 'function') callback(false);
                        return;
                    }
                    focusCodigo();
                    if (typeof callback === 'function') callback(false);
                });
        }

        function openProductoModal() {
            if (typeof window.abrirBuscarProductoModal !== 'function') {
                showAlert('error', 'Producto', 'No se pudo abrir el buscador de productos.');
                return;
            }

            window.abrirBuscarProductoModal({
                modalSelector: '#modalBuscarProductoMovimiento',
                mostrarPrecio: false,
                onSelect: function (producto) {
                    $codigo.val(producto.codigo || '');
                    buscarPorCodigo(function (ok) {
                        if (ok) {
                            $cantUnidad.focus().select();
                        }
                    });
                }
            });
        }

        // Resumen que muestra el modal "Movimiento guardado" (ver modal-postmovimiento.js): sucursales, items y kilos y la fecha. Sale de lo que ya
        // esta cargado en la pantalla. esAlta + tiempo de envio > 0: la fecha se presenta como "Llega ..." (llegada estimada, ver Editar.cshtml).
        function armarResumenModal(esAlta) {
            var totalKilos = 0;
            state.lines.forEach(function (line) { totalKilos += toFloat(line.CantKg); });

            var fecha = String($('#FechaMovimiento').val() || '');   // yyyy-MM-ddTHH:mm
            var partes = fecha.match(/^(\d{4})-(\d{2})-(\d{2})T(\d{2}:\d{2})/);
            var minutosEnvio = esAlta && $envioMinutos.length ? Math.max(toInt($envioMinutos.val()), 0) : 0;

            return {
                ruta: $.trim($('#IdSucursalOrigen option:selected').text()) + ' → ' + $.trim($('#IdSucursalDestino option:selected').text()),
                items: state.lines.length,
                kilos: formatKg(totalKilos),
                fecha: partes ? partes[3] + '/' + partes[2] + '/' + partes[1] + ' ' + partes[4] : '',
                etiquetaFecha: minutosEnvio > 0 ? 'Llega' : 'Fecha',
                minutosEnvio: minutosEnvio
            };
        }

        function openPrintOptions() {
            // El guard mira config.pdfUrl (no imprimirUrl): en un movimiento nuevo sin guardar
            // ambos vienen vacios y no hay nada que imprimir todavia. imprimirUrl/
            // imprimirPayloadUrl se pasan al modal para el ticket termico (ver ticket-print.js).
            if (!config.pdfUrl || !window.PostMovimientoModal || typeof window.PostMovimientoModal.open !== 'function') {
                showAlert('warning', 'Movimiento', 'Todavía no se pueden mostrar las opciones de impresión para este movimiento.');
                return;
            }

            window.PostMovimientoModal.open({
                redirectUrl: '',
                pdfUrl: config.pdfUrl,
                imprimirUrl: config.imprimirUrl,
                imprimirPayloadUrl: config.imprimirPayloadUrl,
                whatsappTexto: config.whatsappTexto,
                stayOnPage: true,
                titulo: 'Imprimir movimiento',
                resumen: armarResumenModal(false)
            });
        }

        $(document).on('click.movimientoBuscarProducto', '#btnBuscarProducto', function (e) {
            e.preventDefault();
            openProductoModal();
        });

        $('#btnAgregarProducto').on('click', function () {
            addCurrentLine();
        });

        // "Enter" (o el "Sig./Ir" del teclado tactil en mobile, que en la
        // mayoria de los navegadores dispara el mismo evento) sobre Kgs
        // agrega el producto, igual que tocar el boton +.
        // Recorrido con Enter (2026-11-12, opcion C): Codigo -> Cant.Un. -> Kgs. (con la balanza se ve el peso, solo lectura) -> Enter registra y el
        // foco vuelve a Codigo. El boton + ya no es una parada del teclado (queda para clic / tactil). Antes, con la balanza activa este Enter solo
        // pasaba el foco al +, y con el campo editable otro manejador le sacaba el foco a + de inmediato (el foco llegaba a Codigo solo por el
        // setTimeout de focusCodigo): ahora hay un solo manejador.
        $cantKgs.on('keydown.movimientoAgregar', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            addCurrentLine();
        });

        // Clic en un encabezado del modal de acumulados: ordena (delegado en document, el modal esta fuera del form).
        $(document).on('click.movimientoAcumulados', '#tablaAcumuladosMovimiento thead th[data-sort-key]', function () {
            var clave = String($(this).data('sort-key') || '');
            if (!clave) return;
            window.OrdenTabla.alternar(acumuladosOrden, clave);
            renderAcumulados();
        });

        $('#btnVerAcumulados').on('click', function () {
            acumuladosOrden.sortKey = 'codigo';
            acumuladosOrden.sortDirection = 'asc';
            acumuladosDetalle.reiniciar();
            renderAcumulados();
            $('#modalAcumuladosMovimiento').modal('show');
        });

        $(document).on('click.movimientoLine', '.js-remove-line', function () {
            if (state.readOnly) return;
            var index = toInt($(this).data('index'));
            state.lines.splice(index, 1);
            renderLines();
            capturarRespaldo('Movimiento');
            scheduleDraft();
        });

        $(document).on('click.movimientoSort', '#tablaLineasMovimiento thead th[data-sort-key]', function () {
            var key = String($(this).data('sort-key') || '');
            if (!key) return;

            if (state.sortKey === key) {
                state.sortDirection = state.sortDirection === 'asc' ? 'desc' : 'asc';
            } else {
                state.sortKey = key;
                state.sortDirection = 'asc';
            }

            renderLines();
        });

        // Celular: sin encabezados para tocar, el orden se elige con el selector (valor "clave:direccion").
        $(document).on('change.movimientoSort', '#ordenLineasMovimiento', function () {
            var partes = String($(this).val() || '').split(':');
            if (!partes[0]) return;
            state.sortKey = partes[0];
            state.sortDirection = partes[1] === 'desc' ? 'desc' : 'asc';
            renderLines();
        });

        $(document).on('click.movimientoSearchToggle', '#btnToggleBusquedaLineasMovimiento', function () {
            var $btn = $(this);
            var $input = $('#filtroLineasMovimiento');
            var mostrar = $input.hasClass('d-none');
            $input.toggleClass('d-none', !mostrar);
            $btn.attr('aria-expanded', mostrar ? 'true' : 'false');

            if (mostrar) {
                $input.focus();
            } else {
                $input.val('');
                state.searchText = '';
                renderLines();
            }
        });

        $(document).on('input.movimientoSearch', '#filtroLineasMovimiento', function () {
            state.searchText = $(this).val() || '';
            renderLines();
        });

        $btnImprimirMovimiento.on('click', function (e) {
            e.preventDefault();
            openPrintOptions();
        });

        $codigo.on('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                window.clearTimeout(state.productoTimer);
                buscarPorCodigo(function (ok) {
                    if (ok) $cantUnidad.focus().select();
                }, false);
            } else if (e.key === 'F10') {
                e.preventDefault();
                openProductoModal();
            }
        });

        $codigo.on('input', function () {
            window.clearTimeout(state.productoTimer);

            var raw = ($codigo.val() || '').trim();
            if (!raw) {
                clearProducto();
                return;
            }

            if (!/^\d+$/.test(raw)) {
                clearProducto();
                $codigo.val(raw.replace(/\D/g, ''));
                return;
            }

            setProductoEstado('Buscando...');

            state.productoTimer = window.setTimeout(function () {
                buscarPorCodigo(null, true);
            }, 250);
        });

        $cantUnidad.on('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                // Producto no pesable + cantidad cargada: Cant.Kg ya viene
                // reflejado solo (mirror mas abajo, on input/change), asi que
                // pasar por ese campo a mano es un paso de mas -- Enter agrega
                // la linea directo. addCurrentLine() valida todo lo necesario.
                if (!productoEsPesable() && toInt($cantUnidad.val()) !== 0) {
                    addCurrentLine();
                    return;
                }
                $cantKgs.focus().select();
            }
        });

        $('#btnAgregarProducto').on('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                addCurrentLine();
            }
        });

        $('#txtCantUnidad').on('input change', function () {
            if (!productoEsPesable()) {
                $cantKgs.val($cantUnidad.val() || '');
            }
            validarRelacionCantidadKilos();
        });

        $('#txtCantKgs, #chkPermitirIngreso').on('input change', function () {
            validarRelacionCantidadKilos();
        });

        // Boton "Permitir ingreso" (2026-11-12): un clic registra la linea igual (misma accion que la tecla +).
        $('#btnPermitirIngreso').on('click', function () {
            registrarConPermitirIngreso();
        });

        $observaciones.on('input', autoResizeObservaciones);

        $balanza.on('change', function () {
            if ($balanza.is(':checked')) {
                if (!productoEsPesable()) {
                    syncBalanzaStatusVisibility();
                    syncReadOnlyUi();
                    return;
                }
                balanzaManualDesactivada = false;
                if (!balanzaDisponible) {
                    intentarActivarBalanza();
                    return;
                }

                if (window.CarnisysBalanza) {
                    window.CarnisysBalanza.activar();
                }
                if (balanzaUltimaLectura && balanzaUltimaLectura.conectada) {
                    $cantKgs.val(balanzaUltimaLectura.pesoDisplay || balanzaUltimaLectura.pesoTexto || '');
                }
            } else {
                balanzaManualDesactivada = true;
                if (window.CarnisysBalanza) {
                    window.CarnisysBalanza.desactivar();
                }
            }

            syncBalanzaStatusVisibility();
            syncReadOnlyUi();
            scheduleDraft();
        });

        $(document).on('keydown.movimientoGlobal', function (e) {
            if (!$page.length) return;
            var tag = e.target && e.target.tagName ? e.target.tagName.toLowerCase() : '';
            var key = String(e.key || '').toLowerCase();
            if (e.key === 'F10' && tag !== 'textarea') {
                e.preventDefault();
                openProductoModal();
                return;
            }

            if (e.altKey && !e.ctrlKey && !e.metaKey && !e.shiftKey && !e.repeat && key === 'enter') {
                // Con un modal abierto, el backdrop bloquea el mouse pero no el teclado -- sin
                // este chequeo el atajo dispara el boton primario de esta pantalla detras del modal.
                if ($(".modal.show").length) return;
                var $primaryAction = getPrimaryActionButton();
                if (!$primaryAction.length || $primaryAction.prop('disabled')) return;

                e.preventDefault();
                $primaryAction.trigger('click');
                return;
            }

            if ((e.key === '+' || e.key === 'Add') && tag !== 'textarea') {
                if (registrarConPermitirIngreso()) {
                    e.preventDefault();
                    return;
                }
            }

            if (e.key === '*' && tag !== 'textarea') {
                e.preventDefault();
                if ($balanza.is(':checked')) {
                    $balanza.prop('checked', false).trigger('change');
                    // 2026-11-12: el foco va siempre a Kgs. (al desactivar queda editable para tipear el peso).
                    enfocarKilos();
                } else {
                    intentarActivarBalanza(true);
                }
            }
        });

        // Tiempo de envio (movimiento nuevo): al cambiarlo se recalcula la llegada estimada; si antes se habia tipeado la fecha a mano,
        // vuelve a ser automatica (gana lo ultimo que toco el usuario). Tipear la Fecha la deja manual: ya no se suman minutos.
        $envioMinutos.on('input change', function () {
            marcarFechaManual(false);
            actualizarLlegadaEstimada();
            scheduleDraft();
        });
        if ($envioMinutos.length) {
            $('#FechaMovimiento').on('input change', function () {
                marcarFechaManual(true);
                scheduleDraft();
            });
            actualizarLlegadaEstimada();
            // La vista previa avanza con el reloj mientras el usuario carga productos (el servidor recalcula al guardar).
            setInterval(actualizarLlegadaEstimada, 30000);
        }

        $('#formMovimiento').on('submit', function (e) {
            e.preventDefault();

            // Un solo guardado a la vez. Se ignora el submit si (a) todavia hay un guardado en vuelo (doble click,
            // Enter repetido) o (b) el aviso "Movimiento guardado" sigue arriba: Alt+G / Alt+Enter de
            // edit-page-guard.js llegan hasta aca aunque el modal este abierto. Sin esto cada submit extra
            // creaba otro movimiento (el servidor no distingue un reintento de un movimiento nuevo).
            if (state.enviando) return;
            if (window.PostMovimientoModal && typeof window.PostMovimientoModal.estaAbierto === 'function' && window.PostMovimientoModal.estaAbierto()) return;

            // Codigo escrito en el buscador sin agregar a la tabla: no se guarda hasta agregarlo o borrarlo.
            if (window.ProductoPendienteGuard && window.ProductoPendienteGuard.bloqueaGuardado({
                codigoSelector: '#txtCodigoProducto',
                destino: 'el movimiento'
            })) {
                return;
            }
            renderLines();
            clearWarning();

            var $form = $(this);

            function guardarMovimientoReal() {
                state.saving = true;
                state.enviando = true;
                // Alta (todavia sin id) o modificacion: el resumen del modal "Movimiento guardado" presenta la fecha como llegada solo en el alta.
                var esAlta = toInt($('#IdMovimiento').val()) <= 0;

                $.ajax({
                    url: $form.attr('action'),
                    type: 'POST',
                    data: $form.serialize()
                }).done(function (resp) {
                    if (!resp || !resp.ok) {
                        state.saving = false;
                        showAlert('error', 'Movimiento', (resp && resp.mensaje) || 'No se pudo guardar el movimiento.');
                        return;
                    }

                    // El formulario queda enlazado al movimiento recien guardado. Tras "Imprimir" la persona se queda en
                    // esta pantalla (stayOnPage) con el formulario de "nuevo" todavia armado: sin esto, un segundo
                    // "Guardar" desde aca (boton, Enter, Alt+Enter) creaba OTRO movimiento igual (IdMovimiento=0);
                    // ahora lo actualiza (el servidor reemplaza las lineas, no las duplica). Tambien se actualizan las
                    // URLs de impresion de esta pantalla (openPrintOptions) con las del movimiento guardado.
                    var idGuardado = toInt(resp.movimientoId);
                    if (idGuardado > 0) {
                        $('#IdMovimiento').val(idGuardado);
                        if (resp.fechaMovimiento) {
                            $('#FechaMovimiento').val(resp.fechaMovimiento);
                            marcarFechaManual(true);
                        }
                        config.pdfUrl = resp.pdfUrl || config.pdfUrl;
                        config.imprimirUrl = resp.imprimirUrl || config.imprimirUrl;
                        config.imprimirPayloadUrl = resp.imprimirPayloadUrl || config.imprimirPayloadUrl;
                        config.whatsappTexto = resp.whatsappTexto || config.whatsappTexto;
                        // Un Nuevo recien guardado pasa a ser la edicion de ese movimiento (si la persona se queda en la pantalla).
                        if (documento) documento.convertirAEdicion(idGuardado);
                    }

                    if (state.borradorGenerico && state.borradorGenerico.estaHabilitado()) {
                        state.borradorGenerico.marcarFinalizado(resp.movimientoId);
                    } else {
                        clearDraft();
                    }
                    if (window.PostMovimientoModal && typeof window.PostMovimientoModal.open === 'function') {
                        window.PostMovimientoModal.open($.extend({}, resp, { resumen: armarResumenModal(esAlta) }));
                    } else {
                        showAlert('success', 'Movimiento', resp.mensaje || 'El movimiento se guardó correctamente.');
                        window.location.href = resp.redirectUrl || config.redirectUrl || '/Movimientos';
                    }
                }).fail(function (xhr) {
                    state.saving = false;
                    var mensaje = 'No se pudo guardar el movimiento.';
                    if (xhr && xhr.responseJSON && xhr.responseJSON.mensaje) mensaje = xhr.responseJSON.mensaje;
                    showAlert('error', 'Movimiento', mensaje);
                }).always(function () {
                    // Va ultimo a proposito: jQuery corre los callbacks en el orden en que se registran, asi que para
                    // cuando se libera el guardado el aviso post-guardado (que tambien frena submits) ya esta abierto.
                    state.enviando = false;
                });
            }

            // Usuario de produccion (sala de piso): antes de guardar, se elige de un modal
            // quien es el empleado real -- ver Web/Scripts/app/seleccion-usuario-produccion.js.
            // Para cualquier otro usuario, esto guarda directo (sin cambios de comportamiento).
            if (window.SeleccionUsuarioProduccion) {
                window.SeleccionUsuarioProduccion.conSeleccionDeUsuario($form, guardarMovimientoReal, {
                    titulo: '¿Quién está cargando este movimiento?'
                });
            } else {
                guardarMovimientoReal();
            }
        });

        $page.on('click.movimientosDraft', '[data-action="restore-draft"]', function () {
            var draft = readDraft();
            if (!draft) return;
            applyDraft(draft);
            hideDraftBanner();
        });

        $page.on('click.movimientosDraft', '[data-action="clear-draft"]', function () {
            if (!confirm('Se eliminará el borrador local de este movimiento. ¿Continuar?')) return;
            clearDraft();
        });

        $page.on('click.movimientosDraft', '#btnVerBorradoresMovimientos', function () {
            state.borradorGenerico && state.borradorGenerico.abrirModalBorradores();
        });

        $('#formMovimiento').on('input change', 'input, select, textarea', function () {
            scheduleDraft();
        });

        ensureAllLinesMetadata();
        ensureSortableHeaders();
        renderLines();
        renderBalanzaStatus(null);
        verificarBalanzaInicial();
        autoResizeObservaciones();
        syncPrimaryAction();
        // El documento se arma ANTES de aplicarTraspaso(): desde ahi el borrador local ya usa la clave de esta pestaña.
        documento = crearDocumentoAbierto();
        // Llega desde "Abrir en ventana": la pestaña de origen se esta yendo, no corresponde avisar de ella.
        var vieneDeTraspaso = /[?&]traspaso=/.test(window.location.search);
        var esNuevoActivo = !state.readOnly && toInt($('#IdMovimiento').val()) <= 0;

        if (state.borradorGenerico) {
            state.borradorGenerico.actualizarBadgeInicial();
        }
        aplicarTraspaso();

        // Un Nuevo se registra y avisa si hay otros abiertos en otras pestañas (traer / cargar aparte / volver). El cartel de
        // "restaurar borrador local" espera a esa respuesta: primero se adopta el borrador que dejo una pestaña ya cerrada.
        var anuncio = (documento && esNuevoActivo)
            ? documento.anunciarNuevo({ sinAviso: vieneDeTraspaso })
            : Promise.resolve({ accion: 'seguir' });
        anuncio.then(function (resultado) {
            if (resultado.accion === 'listado' || resultado.accion === 'traido') return;
            if (state.borradorGenerico || vieneDeTraspaso) return;
            if (documento && esNuevoActivo) documento.adoptarBorradorHuerfano(config.draftKey);
            if (readDraft()) showDraftBanner();
        });
        focusCodigo();

        window.MovimientosEdit = window.MovimientosEdit || {};
        window.MovimientosEdit.setReadOnly = function (readOnly) {
            state.readOnly = !!readOnly;
            syncReadOnlyUi();
        };
        window.MovimientosEdit.syncPrimaryAction = syncPrimaryAction;
        // "Modificar" de un movimiento ya guardado: antes de pasar a edicion se chequea que otra pestaña no lo este modificando
        // (documento-abierto.js). `habilitar` es la funcion de edit-readonly.js que pasa a edicion; si no se llama, queda en solo lectura.
        window.MovimientosEdit.antesDeModificar = function (habilitar) {
            var idMovimiento = toInt($('#IdMovimiento').val());
            if (!documento || idMovimiento <= 0) {
                habilitar();
                return;
            }
            documento.tomarEdicion(idMovimiento).then(function (puede) {
                if (puede) habilitar();
            });
        };
        // Para el panel de stock del destino: carga un producto como si se hubiera elegido en el buscador (codigo -> busca el producto -> foco en
        // la cantidad). Con `cantidad` (el sugerido a enviar) la deja puesta: kilos si el producto es por peso, unidades si no; no agrega la
        // linea (el usuario confirma con +). Sin permiso de edicion no hace nada.
        window.MovimientosEdit.resumenLineas = resumenDeLineas;
        window.MovimientosEdit.cargarProducto = function (codigo, cantidad, pesable) {
            if (state.readOnly) return;
            $codigo.val(codigo || '');
            // Se limpian las cantidades del producto anterior: la linea exige Cant. Un. y Kgs., y no debe arrastrarse un valor ajeno.
            $cantUnidad.val('');
            $cantKgs.val('');
            buscarPorCodigo(function (ok) {
                if (!ok) return;
                var c = Number(cantidad);
                if (isFinite(c) && c > 0) {
                    if (pesable) {
                        // Por peso: los kilos sugeridos y, si el producto tiene promedio, las unidades equivalentes (la linea exige las dos y
                        // validarRelacionCantidadKilos las compara). El foco queda en Cant. Un. para que el usuario lo confirme.
                        var kg = Math.round(c * 1000) / 1000;
                        var promedio = toFloat($productoPromedio.val());
                        $cantKgs.val(String(kg).replace('.', ',')).trigger('input').trigger('change');
                        if (promedio > 0) $cantUnidad.val(String(Math.max(1, Math.round(kg / promedio)))).trigger('input').trigger('change');
                    } else {
                        // Por unidades: se envia entero; los kilos acompañan a las unidades (como en el resto de la pantalla para los no pesables).
                        var unidades = Math.max(1, Math.ceil(c - 1e-9));
                        $cantUnidad.val(String(unidades)).trigger('input').trigger('change');
                        $cantKgs.val(String(unidades)).trigger('input').trigger('change');
                    }
                }
                $cantUnidad.focus().select();
            });
        };
    }

    $(function () {
        initIndex();
        initEdit();
    });
})();
