(function ($) {
    if (!$) {
        return;
    }

    function toNumber(value) {
        if (value === null || value === undefined || value === "") {
            return 0;
        }

        var text = String(value).trim();
        if (!text) {
            return 0;
        }

        text = text.replace(/[^0-9,.\-]/g, "");
        if (!text) {
            return 0;
        }

        var commaIndex = text.lastIndexOf(",");
        var dotIndex = text.lastIndexOf(".");
        if (commaIndex > -1 && dotIndex > -1) {
            if (commaIndex > dotIndex) {
                text = text.replace(/\./g, "").replace(",", ".");
            } else {
                text = text.replace(/,/g, "");
            }
        } else if (commaIndex > -1) {
            text = text.replace(/\./g, "").replace(",", ".");
        }

        var n = parseFloat(text);
        return isNaN(n) ? 0 : n;
    }

    function formatNumber(value, decimals) {
        return toNumber(value).toLocaleString("es-AR", {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        });
    }

    function formatCurrency(value) {
        return toNumber(value).toLocaleString("es-AR", {
            style: "currency",
            currency: "ARS",
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function nowLocalValue() {
        var now = new Date();
        var offset = now.getTimezoneOffset();
        var local = new Date(now.getTime() - (offset * 60000));
        return local.toISOString().slice(0, 16);
    }

    function parseLocalDate(value) {
        return value ? new Date(value) : null;
    }

    function formatDateDisplay(value) {
        var date = parseLocalDate(value);
        if (!date || isNaN(date.getTime())) {
            return "";
        }

        return date.toLocaleString("es-AR", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit"
        });
    }

    function formatLocalDateTimeValue(date) {
        if (!date || isNaN(date.getTime())) {
            return "";
        }

        var year = date.getFullYear();
        var month = String(date.getMonth() + 1).padStart(2, "0");
        var day = String(date.getDate()).padStart(2, "0");
        var hour = String(date.getHours()).padStart(2, "0");
        var minute = String(date.getMinutes()).padStart(2, "0");
        return year + "-" + month + "-" + day + "T" + hour + ":" + minute;
    }

    function combineDateWithTime(dateValue, timeSource) {
        if (!dateValue || !timeSource) {
            return null;
        }

        var datePart = String(dateValue).slice(0, 10);
        var timePart = String(timeSource).slice(11, 16);
        if (!datePart || !timePart || timePart.length < 5) {
            return null;
        }

        return parseLocalDate(datePart + "T" + timePart);
    }

    $(function () {
        var $form = $("#formReportes");
        if (!$form.length) {
            return;
        }

        var config = window.reportesConfig || {};
        var cierres = config.cierres || [];
        var chartInstances = {};

        var $mainFilters = $form.find(".filtro-principal");
        var $liveFilters = $form.find(".filtro-vivo");
        var $pendingCard = $("#cardAvisoPendienteReporte");
        var $tipoReporte = $("#tipoReporte");
        var $sucursal = $("#sucursalReporte");
        var $fechaDesdeDate = $("#fechaDesdeReporte");
        var $fechaHastaDate = $("#fechaHastaReporte");
        var $fechaDesdeCierre = $("#fechaDesdeCierreReporte");
        var $fechaHastaCierre = $("#fechaHastaCierreReporte");
        var $fechaHastaDateWrap = $("#fechaHastaDateWrap");
        var $btnAhora = $("#btnAhoraReporte");
        var $switchDetalles = $("#switchMostrarDetallesReporte");
        var $floatingZone = $("#reportesFloatingZone");
        var $stickyStack = $("#reportesStickyStack");
        var $stickyPlaceholder = $();
        var $tablaStock = $("#tablaReporteStockActual");
        var $rowsStock = $tablaStock.find(".reporte-stock-row");
        var stockSortState = {
            key: "",
            direction: "asc"
        };
        var $modalDetalleStock = $("#modalDetalleStockReporte");
        var $tablaVentas = $("#tablaReporteVentasProducto");
        var $rowsVentas = $tablaVentas.find(".reporte-ventas-row");
        var $tablaProyeccion = $("#tablaReporteProyeccionVentasStock");
        var $rowsProyeccion = $tablaProyeccion.find(".reporte-proyeccion-row");
        var proyeccionSortState = {
            key: "",
            index: -1,
            direction: "asc"
        };
        var $btnAgregarPeriodo = $("#btnAgregarPeriodoComparativo");
        var $listaPeriodos = $("#listaPeriodosComparativos");
        var $agrupacionTemporal = $("#agrupacionTemporalReporte");

        function reportType() {
            return $.trim($tipoReporte.val() || "");
        }

        function isStockReport() {
            var tipo = reportType();
            return tipo === "Stock Actual" || tipo === "Cierre Stock" || tipo === "Stock Retroactivo";
        }

        function isVentasProductoReport() {
            return reportType() === "Ventas por Producto";
        }

        function isProyeccionReport() {
            return reportType() === "Proyeccion Ventas vs Stock";
        }

        function isBalanceReport() {
            return reportType() === "Balance Economico";
        }

        function showPending(show) {
            $pendingCard.toggleClass("d-none", !show);
        }

        function reportRequiresSpecificBranch() {
            return isStockReport();
        }

        function applyBranchRule() {
            var requiresSpecificBranch = reportRequiresSpecificBranch();
            var $allOption = $sucursal.find('option[value="0"]');
            $allOption.prop("disabled", requiresSpecificBranch);

            if (requiresSpecificBranch && ($sucursal.val() === "0" || !$sucursal.val())) {
                var preferred = config.sucursalActual && config.sucursalActual !== "0"
                    ? config.sucursalActual
                    : ($sucursal.find('option:not([value="0"])').first().val() || "0");
                $sucursal.val(preferred);
            }
        }

        function getFilteredClosures() {
            var sucursalId = parseInt($sucursal.val() || "0", 10);
            var list = $.grep(cierres, function (item) {
                return !sucursalId || item.idSucursal === sucursalId;
            });

            list.sort(function (a, b) {
                if (a.fechaIso === b.fechaIso) {
                    return (a.sucursal || "").localeCompare(b.sucursal || "");
                }
                return a.fechaIso < b.fechaIso ? 1 : -1;
            });

            return list;
        }

        function fillClosureSelect($select, selectedValue, addBranchName) {
            var list = getFilteredClosures();
            $select.empty();

            if (!list.length) {
                $select.append($("<option />", { value: "", text: "Sin cierres disponibles" }));
                return list;
            }

            $.each(list, function (_, item) {
                var text = item.texto;
                if (addBranchName && item.sucursal) {
                    text += " - " + item.sucursal;
                }

                $select.append($("<option />", {
                    value: item.fechaIso,
                    text: text
                }));
            });

            if (selectedValue && $select.find('option[value="' + selectedValue + '"]').length) {
                $select.val(selectedValue);
            }

            return list;
        }

        function syncClosureNames() {
            $fechaDesdeCierre.attr("name", $fechaDesdeCierre.hasClass("d-none") ? "" : "fechaDesde");
            $fechaHastaCierre.attr("name", $fechaHastaCierre.hasClass("d-none") ? "" : "fechaHasta");
            $fechaDesdeDate.attr("name", $fechaDesdeDate.hasClass("d-none") ? "" : "fechaDesde");
            $fechaHastaDate.attr("name", $fechaHastaDateWrap.hasClass("d-none") ? "" : "fechaHasta");
        }

        function applyDateMode(useClosureFrom, useClosureTo) {
            $fechaDesdeDate.toggleClass("d-none", useClosureFrom);
            $fechaDesdeCierre.toggleClass("d-none", !useClosureFrom);
            $fechaHastaDateWrap.toggleClass("d-none", useClosureTo);
            $fechaHastaCierre.toggleClass("d-none", !useClosureTo);
            syncClosureNames();
        }

        function applyDetailsMode() {
            var showDetails = $switchDetalles.length && $switchDetalles.is(":checked");
            $(".reporte-detalle-col").toggleClass("d-none", !showDetails);
            $tablaStock.toggleClass("reportes-stock-detalles-activos", showDetails);
        }

        function applyReportMode(resetValues) {
            var tipo = reportType();
            var list = getFilteredClosures();
            var ultimo = list.length ? list[0].fechaIso : "";
            var anteultimo = list.length > 1 ? list[1].fechaIso : ultimo;
            var ahora = nowLocalValue();

            applyBranchRule();

            if (tipo === "Stock Actual") {
                applyDateMode(true, false);
                fillClosureSelect($fechaDesdeCierre, resetValues ? ultimo : ($fechaDesdeCierre.val() || config.fechaDesdeActual), parseInt($sucursal.val() || "0", 10) === 0);
                if (resetValues || !$fechaHastaDate.val()) {
                    $fechaHastaDate.val(ahora);
                }
            } else if (tipo === "Cierre Stock") {
                applyDateMode(true, true);
                fillClosureSelect($fechaDesdeCierre, resetValues ? anteultimo : ($fechaDesdeCierre.val() || config.fechaDesdeActual), parseInt($sucursal.val() || "0", 10) === 0);
                fillClosureSelect($fechaHastaCierre, resetValues ? ultimo : ($fechaHastaCierre.val() || config.fechaHastaActual), parseInt($sucursal.val() || "0", 10) === 0);
            } else if (tipo === "Stock Retroactivo") {
                applyDateMode(true, false);
                fillClosureSelect($fechaDesdeCierre, resetValues ? ultimo : ($fechaDesdeCierre.val() || config.fechaDesdeActual), parseInt($sucursal.val() || "0", 10) === 0);
                if (resetValues || !$fechaHastaDate.val()) {
                    $fechaHastaDate.val(ahora);
                }
            } else {
                applyDateMode(false, false);
            }
        }

        function getMainDurationMs() {
            var desde = parseLocalDate($fechaDesdeDate.hasClass("d-none") ? $fechaDesdeCierre.val() : $fechaDesdeDate.val());
            var hasta = parseLocalDate($fechaHastaDateWrap.hasClass("d-none") ? $fechaHastaCierre.val() : $fechaHastaDate.val());
            if (!desde || !hasta || isNaN(desde.getTime()) || isNaN(hasta.getTime())) {
                return 0;
            }

            return Math.max(hasta.getTime() - desde.getTime(), 0);
        }

        function recalculatePeriodRow($row) {
            var duration = getMainDurationMs();
            var principalStartValue = $fechaDesdeDate.hasClass("d-none") ? $fechaDesdeCierre.val() : $fechaDesdeDate.val();
            var $fecha = $row.find(".periodo-comparativo-fecha");
            var $desde = $row.find(".periodo-comparativo-desde");
            var $hora = $row.find(".periodo-comparativo-hora");
            var $hasta = $row.find(".periodo-comparativo-hasta");
            var start = combineDateWithTime($fecha.val(), principalStartValue);

            if (!start || isNaN(start.getTime())) {
                $desde.val("");
                $hora.text("");
                $hasta.val("");
                return;
            }

            $desde.val(formatLocalDateTimeValue(start));
            $hora.text(formatLocalDateTimeValue(start).slice(11, 16));
            var end = new Date(start.getTime() + duration);
            $hasta.val(formatDateDisplay(formatLocalDateTimeValue(end)));
        }

        function recalculateAllPeriodRows() {
            $listaPeriodos.find(".reportes-periodo-comparativo").each(function () {
                recalculatePeriodRow($(this));
            });
        }

        function addPeriodRow(defaultFrom) {
            if (!$listaPeriodos.length) {
                return;
            }

            var defaultDate = defaultFrom ? String(defaultFrom).slice(0, 10) : "";
            var defaultDateTime = defaultFrom ? String(defaultFrom).slice(0, 16) : "";

            var html = [
                '<div class="reportes-periodo-comparativo card border-0 bg-light mb-2">',
                '  <div class="card-body py-2 px-3">',
                '    <div class="reportes-periodo-grid">',
                '      <div>',
                '        <label class="stock-filtro-label mb-1">Fecha comparativa</label>',
                '        <div class="input-group input-group-sm reportes-periodo-fecha-group">',
                '          <input type="date" class="form-control filtro-principal periodo-comparativo-fecha" value="' + defaultDate + '" />',
                '          <div class="input-group-append">',
                '            <span class="input-group-text periodo-comparativo-hora"></span>',
                '          </div>',
                '        </div>',
                '        <input type="hidden" name="periodoComparativoDesde" class="periodo-comparativo-desde" value="' + defaultDateTime + '" />',
                '      </div>',
                '      <div>',
                '        <label class="stock-filtro-label mb-1">Rango calculado</label>',
                '        <input type="text" class="form-control form-control-sm periodo-comparativo-hasta" readonly="readonly" />',
                '      </div>',
                '      <div class="reportes-periodo-accion">',
                '        <button type="button" class="btn btn-outline-danger btn-sm btn-quitar-periodo"><i class="fas fa-times"></i></button>',
                '      </div>',
                '    </div>',
                '  </div>',
                '</div>'
            ].join("");

            var $row = $(html);
            $listaPeriodos.append($row);
            recalculatePeriodRow($row);
        }

        function initializePeriodsFromConfig() {
            if (!$listaPeriodos.length || $listaPeriodos.children().length) {
                recalculateAllPeriodRows();
                return;
            }

            $.each(config.periodosComparativos || [], function (_, item) {
                addPeriodRow(item.fechaDesde);
            });
        }

        function getLastComparativeStart() {
            if (!$listaPeriodos.length) {
                return null;
            }

            var $lastRow = $listaPeriodos.find(".reportes-periodo-comparativo").last();
            if (!$lastRow.length) {
                return null;
            }

            var value = $lastRow.find(".periodo-comparativo-desde").val();
            var date = parseLocalDate(value);
            return date && !isNaN(date.getTime()) ? date : null;
        }

        function ensureStickyPlaceholder() {
            if (!$stickyStack.length) {
                return;
            }

            $stickyPlaceholder = $("#reportesStickyPlaceholder");
            if ($stickyPlaceholder.length) {
                return;
            }

            $stickyPlaceholder = $('<div id="reportesStickyPlaceholder" style="display:none;"></div>');
            $stickyStack.after($stickyPlaceholder);
        }

        function resetFloatingStack() {
            if (!$stickyStack.length) {
                return;
            }

            $stickyStack
                .removeClass("is-floating is-docked")
                .css({
                    position: "",
                    top: "",
                    left: "",
                    width: ""
                });

            if ($stickyPlaceholder.length) {
                $stickyPlaceholder.hide().height(0);
            }
        }

        function updateFloatingStack() {
            if (!$floatingZone.length || !$stickyStack.length) {
                return;
            }

            var $host = $floatingZone.find(".sync-scroll-host").first();
            if (!$host.length || !$host.is(":visible")) {
                resetFloatingStack();
                return;
            }

            ensureStickyPlaceholder();

            var zoneOffset = $floatingZone.offset();
            if (!zoneOffset) {
                resetFloatingStack();
                return;
            }

            var topOffset = 0;
            var scrollTop = $(window).scrollTop() || 0;
            var zoneTop = zoneOffset.top;
            var zoneHeight = $floatingZone.outerHeight() || 0;
            var zoneBottom = zoneTop + zoneHeight;
            var stackHeight = $stickyStack.outerHeight() || 0;
            var referenceOffset = $stickyPlaceholder.length && $stickyPlaceholder.is(":visible")
                ? $stickyPlaceholder.offset()
                : zoneOffset;
            var referenceWidth = $stickyPlaceholder.length && $stickyPlaceholder.is(":visible")
                ? ($stickyPlaceholder.outerWidth() || $floatingZone.outerWidth() || 0)
                : ($floatingZone.outerWidth() || 0);

            if (!zoneHeight || !stackHeight) {
                resetFloatingStack();
                return;
            }

            if (scrollTop + topOffset <= zoneTop) {
                resetFloatingStack();
                return;
            }

            $stickyPlaceholder.show().height(stackHeight);

            if (scrollTop + topOffset + stackHeight >= zoneBottom) {
                $stickyStack
                    .removeClass("is-floating")
                    .addClass("is-docked")
                    .css({
                        position: "absolute",
                        top: Math.max(zoneHeight - stackHeight, 0) + "px",
                        left: "0",
                        width: "100%"
                    });
                return;
            }

            $stickyStack
                .removeClass("is-docked")
                .addClass("is-floating")
                .css({
                    position: "fixed",
                    top: topOffset + "px",
                    left: referenceOffset.left + "px",
                    width: referenceWidth + "px"
                });
        }

        function updateVisibleTotalsStock() {
            if (!$rowsStock.length) {
                return;
            }

            var totalKg = 0;
            var totalIngresos = 0;
            var totalEgresos = 0;
            var productos = {};
            var registros = 0;

            $rowsStock.each(function () {
                var $row = $(this);
                if ($row.hasClass("d-none")) {
                    return;
                }

                registros += 1;
                productos[$row.attr("data-idcorte")] = true;
                totalKg += toNumber($row.attr("data-stockactual"));
                totalIngresos += toNumber($row.attr("data-totalingresos"));
                totalEgresos += toNumber($row.attr("data-totalegresos"));
            });

            $("#totalKgReporte").text(formatNumber(totalKg, 3));
            $("#totalIngresosReporte").text(formatNumber(totalIngresos, 3));
            $("#totalEgresosReporte").text(formatNumber(totalEgresos, 3));
            $("#cantidadProductosReporte").text(Object.keys(productos).length);
            $("#cantidadRegistrosReporte").text(registros);
            $("#filaSinResultadosReporte").toggleClass("d-none", registros !== 0);
        }

        function sortStockRows(sortKey, sortType) {
            if (!$tablaStock.length || !$rowsStock.length || !sortKey) {
                return;
            }

            if (stockSortState.key === sortKey) {
                stockSortState.direction = stockSortState.direction === "asc" ? "desc" : "asc";
            } else {
                stockSortState.key = sortKey;
                stockSortState.direction = "asc";
            }

            var direction = stockSortState.direction === "asc" ? 1 : -1;
            var rows = $rowsStock.get().sort(function (a, b) {
                var $a = $(a);
                var $b = $(b);
                var valueA = $a.attr("data-" + sortKey);
                var valueB = $b.attr("data-" + sortKey);

                if (sortType === "number") {
                    valueA = toNumber(valueA);
                    valueB = toNumber(valueB);

                    if (valueA === valueB) {
                        return (($a.attr("data-producto") || "").toUpperCase())
                            .localeCompare(($b.attr("data-producto") || "").toUpperCase()) * direction;
                    }

                    return (valueA - valueB) * direction;
                }

                valueA = (valueA || "").toString().toUpperCase();
                valueB = (valueB || "").toString().toUpperCase();

                var compare = valueA.localeCompare(valueB);
                if (compare === 0) {
                    compare = (($a.attr("data-producto") || "").toUpperCase())
                        .localeCompare(($b.attr("data-producto") || "").toUpperCase());
                }

                return compare * direction;
            });

            var $tbody = $tablaStock.find("tbody");
            var $emptyRow = $("#filaSinResultadosReporte");
            var fragment = document.createDocumentFragment();

            $.each(rows, function (_, row) {
                fragment.appendChild(row);
            });

            if ($emptyRow.length) {
                $emptyRow.before(fragment);
            } else {
                $tbody.append(fragment);
            }

            $rowsStock = $tablaStock.find(".reporte-stock-row");
            $tablaStock.find(".reporte-sortable-col")
                .removeClass("is-sort-asc is-sort-desc")
                .attr("aria-sort", "none");
            $tablaStock.find('.reporte-sortable-col[data-sort-key="' + sortKey + '"]')
                .addClass(direction === 1 ? "is-sort-asc" : "is-sort-desc")
                .attr("aria-sort", direction === 1 ? "ascending" : "descending");

            applyLiveFilters();

            if (window.TableScrollSync && window.TableScrollSync.refresh) {
                window.TableScrollSync.refresh(document);
            }
        }

        function updateVisibleTotalsVentas() {
            if (!$rowsVentas.length) {
                return;
            }

            var totalKg = 0;
            var totalImporte = 0;
            var registros = 0;

            $rowsVentas.each(function () {
                var $row = $(this);
                if ($row.hasClass("d-none")) {
                    return;
                }

                registros += 1;
                var $cells = $row.children("td");
                if ($cells.length >= 3) {
                    for (var i = 1; i < $cells.length; i += 2) {
                        totalKg += toNumber($cells.eq(i).text());
                        if (i + 1 < $cells.length) {
                            totalImporte += toNumber($cells.eq(i + 1).text());
                        }
                    }
                }
            });

            $("#totalKgReporte").text(formatNumber(totalKg, 3));
            $("#totalIngresosReporte").text(totalImporte.toLocaleString("es-AR", {
                style: "currency",
                currency: "ARS",
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }));
            $("#cantidadProductosReporte").text(registros);
            $("#cantidadRegistrosReporte").text(registros);
            $("#filaSinResultadosVentasReporte").toggleClass("d-none", registros !== 0);
        }

        function updateVisibleTotalsProyeccion() {
            if (!$rowsProyeccion.length) {
                return;
            }

            var totalStock = 0;
            var totalVentas = 0;
            var totalDiferencia = 0;
            var registros = 0;

            $rowsProyeccion.each(function () {
                var $row = $(this);
                if ($row.hasClass("d-none")) {
                    return;
                }

                registros += 1;
                totalStock += toNumber($row.attr("data-stockactual"));
                totalVentas += toNumber($row.attr("data-totalventas"));
                totalDiferencia += toNumber($row.attr("data-totaldiferencia"));
            });

            $("#totalKgReporte").text(formatNumber(totalStock, 3));
            $("#totalIngresosReporte").text(formatNumber(totalVentas, 3));
            $("#totalEgresosReporte").text(formatNumber(totalDiferencia, 3));
            $("#cantidadProductosReporte").text(registros);
            $("#cantidadRegistrosReporte").text(registros);
            $("#filaSinResultadosProyeccionReporte").toggleClass("d-none", registros !== 0);
        }

        function sortProyeccionRows(sortKey, sortType, sortIndex) {
            if (!$tablaProyeccion.length || !$rowsProyeccion.length || !sortKey) {
                return;
            }

            var normalizedIndex = parseInt(sortIndex, 10);
            if (isNaN(normalizedIndex)) {
                normalizedIndex = -1;
            }

            if (proyeccionSortState.key === sortKey && proyeccionSortState.index === normalizedIndex) {
                proyeccionSortState.direction = proyeccionSortState.direction === "asc" ? "desc" : "asc";
            } else {
                proyeccionSortState.key = sortKey;
                proyeccionSortState.index = normalizedIndex;
                proyeccionSortState.direction = "asc";
            }

            var direction = proyeccionSortState.direction === "asc" ? 1 : -1;
            var rows = $rowsProyeccion.get().sort(function (a, b) {
                var $a = $(a);
                var $b = $(b);
                var valueA;
                var valueB;

                if (sortKey === "producto") {
                    valueA = ($a.attr("data-producto") || "").toString().toUpperCase();
                    valueB = ($b.attr("data-producto") || "").toString().toUpperCase();
                    return valueA.localeCompare(valueB) * direction;
                }

                valueA = toNumber($a.children("td").eq(normalizedIndex).text());
                valueB = toNumber($b.children("td").eq(normalizedIndex).text());

                if (valueA === valueB) {
                    return (($a.attr("data-producto") || "").toUpperCase())
                        .localeCompare(($b.attr("data-producto") || "").toUpperCase()) * direction;
                }

                return (valueA - valueB) * direction;
            });

            var $tbody = $tablaProyeccion.find("tbody");
            var $emptyRow = $("#filaSinResultadosProyeccionReporte");
            var fragment = document.createDocumentFragment();

            $.each(rows, function (_, row) {
                fragment.appendChild(row);
            });

            if ($emptyRow.length) {
                $emptyRow.before(fragment);
            } else {
                $tbody.append(fragment);
            }

            $rowsProyeccion = $tablaProyeccion.find(".reporte-proyeccion-row");
            $tablaProyeccion.find('.reporte-sortable-col[data-sort-scope="proyeccion"]')
                .removeClass("is-sort-asc is-sort-desc")
                .attr("aria-sort", "none");

            var selector = sortKey === "producto"
                ? '.reporte-sortable-col[data-sort-scope="proyeccion"][data-sort-key="producto"]'
                : '.reporte-sortable-col[data-sort-scope="proyeccion"][data-sort-key="cell"][data-sort-index="' + normalizedIndex + '"]';

            $tablaProyeccion.find(selector)
                .addClass(direction === 1 ? "is-sort-asc" : "is-sort-desc")
                .attr("aria-sort", direction === 1 ? "ascending" : "descending");

            applyLiveFilters();

            if (window.TableScrollSync && window.TableScrollSync.refresh) {
                window.TableScrollSync.refresh(document);
            }
        }

        function applyLiveFilters() {
            var text = ($.trim($("#busquedaProductoReporte").val() || "")).toUpperCase();
            var tipo = ($.trim($("#tipoProductoReporte").val() || "")).toUpperCase();
            var marcaId = $("#marcaReporte").val() || "0";
            var estado = ($.trim($("#estadoStockReporte").val() || "Todos")).toUpperCase();

            if ($rowsStock.length) {
                $rowsStock.each(function () {
                    var $row = $(this);
                    var search = ($row.attr("data-search") || "").toUpperCase();
                    var rowTipo = ($row.attr("data-tipo") || "").toUpperCase();
                    var rowMarca = $row.attr("data-marcaid") || "0";
                    var rowEstado = ($row.attr("data-estado") || "").toUpperCase();

                    var visible = (!text || search.indexOf(text) >= 0) &&
                        (!tipo || rowTipo === tipo) &&
                        (marcaId === "0" || rowMarca === marcaId) &&
                        (!estado || estado === "TODOS" || rowEstado === estado);

                    $row.toggleClass("d-none", !visible);
                });

                updateVisibleTotalsStock();
            }

            if ($rowsVentas.length) {
                $rowsVentas.each(function () {
                    var $row = $(this);
                    var search = ($row.attr("data-search") || "").toUpperCase();
                    var rowTipo = ($row.attr("data-tipo") || "").toUpperCase();
                    var rowMarca = $row.attr("data-marcaid") || "0";

                    var visible = (!text || search.indexOf(text) >= 0) &&
                        (!tipo || rowTipo === tipo) &&
                        (marcaId === "0" || rowMarca === marcaId);

                    $row.toggleClass("d-none", !visible);
                    $row.next(".reporte-ventas-grafico-row").toggleClass("d-none", true);
                });

                updateVisibleTotalsVentas();
            }

            if ($rowsProyeccion.length) {
                $rowsProyeccion.each(function () {
                    var $row = $(this);
                    var search = ($row.attr("data-search") || "").toUpperCase();
                    var rowTipo = ($row.attr("data-tipo") || "").toUpperCase();
                    var rowMarca = $row.attr("data-marcaid") || "0";

                    var visible = (!text || search.indexOf(text) >= 0) &&
                        (!tipo || rowTipo === tipo) &&
                        (marcaId === "0" || rowMarca === marcaId);

                    $row.toggleClass("d-none", !visible);
                });

                updateVisibleTotalsProyeccion();
            }
        }

        function serializeComparativePeriodsForGraph() {
            var result = [];
            $listaPeriodos.find(".periodo-comparativo-desde").each(function () {
                var value = $(this).val();
                if (value) {
                    result.push(value);
                }
            });
            return result;
        }

        function formatSignedNumber(value, decimals, suffix) {
            if (value === null || value === undefined || value === "") {
                return "-";
            }

            var n = toNumber(value);
            var sign = n > 0 ? "+" : "";
            return sign + formatNumber(n, decimals) + (suffix || "");
        }

        function openStockDetailModal($row) {
            if (!$modalDetalleStock.length || !$row || !$row.length) {
                return;
            }

            $("#modalDetalleStockReporteProducto").text($row.attr("data-producto") || "-");
            $("#modalDetalleStockReporteCodigo").text("Codigo: " + ($row.attr("data-codigo") || "-"));
            $("#modalDetalleStockReporteStockActual").text(formatNumber($row.attr("data-stockactual"), 3));
            $("#modalDetalleStockReportePuntoStock").text(formatNumber($row.attr("data-puntostock"), 3));
            $("#modalDetalleStockReporteEstado").text($row.children("td").last().text() || "-");
            $("#modalDetalleStockReporteStockInicial").text(formatNumber($row.attr("data-stockinicial"), 3));
            $("#modalDetalleStockReporteCompras").text(formatNumber($row.attr("data-compras"), 3));
            $("#modalDetalleStockReporteIngresoElaborado").text(formatNumber($row.attr("data-ingresoelaborado"), 3));
            $("#modalDetalleStockReporteIngresoStock").text(formatNumber($row.attr("data-ingresostock"), 3));
            $("#modalDetalleStockReporteIngresoMovimiento").text(formatNumber($row.attr("data-ingresomovimiento"), 3));
            $("#modalDetalleStockReporteAjusteStock").text(formatNumber($row.attr("data-ajustestock"), 3));
            $("#modalDetalleStockReporteTotalIngresos").text(formatNumber($row.attr("data-totalingresos"), 3));
            $("#modalDetalleStockReporteEgresoStock").text(formatNumber($row.attr("data-egresostock"), 3));
            $("#modalDetalleStockReporteEgresoMovimiento").text(formatNumber($row.attr("data-egresomovimiento"), 3));
            $("#modalDetalleStockReporteEgresoElaborado").text(formatNumber($row.attr("data-egresoelaborado"), 3));
            $("#modalDetalleStockReporteVentas").text(formatNumber($row.attr("data-ventas"), 3));
            $("#modalDetalleStockReporteTotalEgresos").text(formatNumber($row.attr("data-totalegresos"), 3));
            $("#modalDetalleStockReporteStockCierre").text(formatNumber($row.attr("data-stockcierre"), 3));

            $modalDetalleStock.modal("show");
        }

        function applyVentasMetricToChart(idCorte, metric) {
            var state = chartInstances[idCorte];
            if (!state || !state.lineChart || !state.response) {
                return;
            }

            var isImporte = metric === "importe";
            state.metric = isImporte ? "importe" : "kg";

            state.lineChart.data.datasets = $.map(state.response.series || [], function (serie) {
                return {
                    label: serie.titulo,
                    borderColor: serie.color,
                    backgroundColor: serie.color,
                    fill: false,
                    lineTension: 0,
                    data: $.map(serie.puntos || [], function (punto) {
                        return isImporte ? punto.importe : punto.kg;
                    }),
                    metaPuntos: serie.puntos || []
                };
            });

            state.lineChart.options.tooltips.callbacks.label = function (tooltipItem, data) {
                var dataset = (data.datasets || [])[tooltipItem.datasetIndex] || {};
                if (isImporte) {
                    return (dataset.label || "Periodo") + ": " + formatCurrency(tooltipItem.yLabel);
                }

                return (dataset.label || "Periodo") + ": " + formatNumber(tooltipItem.yLabel, 3) + " kg";
            };

            state.lineChart.options.scales.yAxes[0].ticks.callback = function (value) {
                return isImporte ? formatCurrency(value) : formatNumber(value, 3);
            };

            state.lineChart.update();
        }

        function renderVentasSummary(idCorte, response) {
            var $target = $("#resumenVentasProducto_" + idCorte);
            if (!$target.length || !response || !response.series) {
                return;
            }

            var promedioLabel = response.promedioLabel || "Promedio";
            var html = $.map(response.series, function (serie) {
                var mejor = serie.mejor || {};
                return [
                    '<div class="reporte-ventas-resumen-card' + (serie.esPrincipal ? ' is-principal' : '') + '">',
                    '  <div class="reporte-ventas-resumen-title" style="color:' + serie.color + ';">' + serie.titulo + '<small>' + (serie.rangoCorto || '') + '</small></div>',
                    '  <div class="reporte-ventas-resumen-line"><span>Total vendido</span><strong>' + formatNumber(serie.totalKg, 3) + ' kg</strong></div>',
                    '  <div class="reporte-ventas-resumen-line"><span>' + promedioLabel + '</span><strong>' + formatNumber(serie.promedioKg, 3) + ' kg</strong></div>',
                    '  <div class="reporte-ventas-resumen-line"><span>Mejor tramo</span><strong>' + (mejor.etiquetaRelativa || '-') + '</strong></div>',
                    '  <div class="small text-muted mb-2">' + (mejor.fechaReal || '-') + ' · ' + (mejor.kg !== undefined ? formatNumber(mejor.kg, 3) + ' kg' : '-') + '</div>',
                    '  <div class="reporte-ventas-resumen-line"><span>Diferencia vs principal</span><strong>' + formatSignedNumber(serie.diferenciaKg, 3, ' kg') + '</strong></div>',
                    '  <div class="reporte-ventas-resumen-line"><span>Diferencia %</span><strong>' + (serie.diferenciaPorcentual === null || serie.diferenciaPorcentual === undefined ? '-' : formatSignedNumber(serie.diferenciaPorcentual, 2, '%')) + '</strong></div>',
                    '</div>'
                ].join("");
            }).join("");

            $target.html(html);
        }

        function renderBalanceCharts() {
            if (!isBalanceReport() || typeof Chart === "undefined") {
                return;
            }

            var data = config.balanceCharts || {};
            var periodos = data.periodos || [];
            var gastos = data.gastos || [];
            if (!periodos.length) {
                return;
            }

            var labels = $.map(periodos, function (item) {
                return item.titulo;
            });

            function moneyTooltip(value) {
                return formatCurrency(value);
            }

            function kilosTooltip(value) {
                return formatNumber(value, 2) + " kg";
            }

            function cantidadTooltip(value) {
                return formatNumber(value, 0);
            }

            function renderBalanceLineChart(canvasId, datasetLabel, color, backgroundColor, valueKey, tooltipPrefix, valueFormatter) {
                var canvas = document.getElementById(canvasId);
                if (!canvas) {
                    return;
                }

                new Chart(canvas.getContext("2d"), {
                    type: "line",
                    data: {
                        labels: labels,
                        datasets: [{
                            label: datasetLabel,
                            borderColor: color,
                            backgroundColor: backgroundColor,
                            fill: true,
                            lineTension: 0,
                            data: $.map(periodos, function (x) { return x[valueKey]; })
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        tooltips: {
                            callbacks: {
                                label: function (tooltipItem) {
                                    return tooltipPrefix + ": " + valueFormatter(tooltipItem.yLabel);
                                },
                                afterTitle: function (items) {
                                    var idx = items && items.length ? items[0].index : -1;
                                    return idx >= 0 && periodos[idx] ? periodos[idx].rango : "";
                                }
                            }
                        },
                        scales: {
                            yAxes: [{
                                ticks: {
                                    callback: function (value) { return valueFormatter(value); }
                                }
                            }]
                        }
                    }
                });
            }

            var canvasEconomico = document.getElementById("graficoBalanceEconomico");
            if (canvasEconomico) {
                new Chart(canvasEconomico.getContext("2d"), {
                    type: "bar",
                    data: {
                        labels: labels,
                        datasets: [
                            { label: "Ventas", backgroundColor: "#2563eb", data: $.map(periodos, function (x) { return x.ventas; }) },
                            { label: "Compras", backgroundColor: "#dc2626", data: $.map(periodos, function (x) { return x.compras; }) },
                            { label: "Gastos", backgroundColor: "#f59e0b", data: $.map(periodos, function (x) { return x.gastos; }) },
                            { label: "Balance", backgroundColor: "#16a34a", data: $.map(periodos, function (x) { return x.balance; }) }
                        ]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        tooltips: {
                            callbacks: {
                                label: function (tooltipItem, chartData) {
                                    var ds = chartData.datasets[tooltipItem.datasetIndex] || {};
                                    return (ds.label || "") + ": " + moneyTooltip(tooltipItem.yLabel);
                                },
                                afterTitle: function (items) {
                                    var idx = items && items.length ? items[0].index : -1;
                                    return idx >= 0 && periodos[idx] ? periodos[idx].rango : "";
                                }
                            }
                        },
                        scales: {
                            yAxes: [{
                                ticks: {
                                    callback: function (value) { return moneyTooltip(value); }
                                }
                            }]
                        }
                    }
                });
            }

            renderBalanceLineChart("graficoBalanceLinea", "Balance economico", "#16a34a", "rgba(22,163,74,0.12)", "balance", "Balance", moneyTooltip);
            renderBalanceLineChart("graficoBalanceKilos", "Cantidades vendidas", "#2563eb", "rgba(37,99,235,0.12)", "kilosVendidos", "Kilos", kilosTooltip);
            renderBalanceLineChart("graficoBalanceCantidadVentas", "Cantidad Tickets", "#7c3aed", "rgba(124,58,237,0.12)", "cantidadVentas", "Tickets", cantidadTooltip);
            renderBalanceLineChart("graficoBalanceKilosPesables", "Cantidades vendidas de productos pesables", "#0f766e", "rgba(15,118,110,0.12)", "kilosVendidosPesables", "Kilos", kilosTooltip);

            var canvasLiquidez = document.getElementById("graficoBalanceLiquidez");
            if (canvasLiquidez) {
                new Chart(canvasLiquidez.getContext("2d"), {
                    type: "bar",
                    data: {
                        labels: labels,
                        datasets: [
                            { label: "Cobros", backgroundColor: "#0891b2", data: $.map(periodos, function (x) { return x.cobros; }) },
                            { label: "Pagos", backgroundColor: "#7c3aed", data: $.map(periodos, function (x) { return x.pagos; }) },
                            { label: "Diferencia", backgroundColor: "#334155", data: $.map(periodos, function (x) { return x.diferencia; }) }
                        ]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        tooltips: {
                            callbacks: {
                                label: function (tooltipItem, chartData) {
                                    var ds = chartData.datasets[tooltipItem.datasetIndex] || {};
                                    return (ds.label || "") + ": " + moneyTooltip(tooltipItem.yLabel);
                                }
                            }
                        },
                        scales: {
                            yAxes: [{
                                ticks: {
                                    callback: function (value) { return moneyTooltip(value); }
                                }
                            }]
                        }
                    }
                });
            }

            var canvasGastos = document.getElementById("graficoBalanceGastos");
            if (canvasGastos) {
                new Chart(canvasGastos.getContext("2d"), {
                    type: "horizontalBar",
                    data: {
                        labels: $.map(gastos, function (x) { return x.categoria; }),
                        datasets: [{
                            label: "Gastos",
                            backgroundColor: ["#ef4444", "#f97316", "#eab308", "#14b8a6", "#6366f1", "#a855f7"],
                            data: $.map(gastos, function (x) { return x.total; })
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        legend: { display: false },
                        tooltips: {
                            callbacks: {
                                label: function (tooltipItem) {
                                    return moneyTooltip(tooltipItem.xLabel);
                                }
                            }
                        },
                        scales: {
                            xAxes: [{
                                ticks: {
                                    callback: function (value) { return moneyTooltip(value); }
                                }
                            }]
                        }
                    }
                });
            }
        }

        function toggleVentasChart($button) {
            var idCorte = $button.attr("data-idcorte");
            var $dataRow = $button.closest("tr");
            var $chartRow = $dataRow.next(".reporte-ventas-grafico-row");
            var $summary = $("#resumenVentasProducto_" + idCorte);
            if (!$chartRow.length) {
                return;
            }

            if (!$chartRow.hasClass("d-none")) {
                $chartRow.addClass("d-none");
                $button.text("Mostrar grafico");
                return;
            }

            $chartRow.removeClass("d-none");
            $button.text("Ocultar grafico");

            if (chartInstances[idCorte] && chartInstances[idCorte].lineChart) {
                return;
            }

            var canvas = document.getElementById("graficoVentasProducto_" + idCorte);
            if (!canvas || typeof Chart === "undefined") {
                $summary.html('<div class="alert alert-warning mb-0">No se pudo inicializar el grafico en esta pantalla.</div>');
                return;
            }

            $.ajax({
                url: config.ventasPorProductoSerieUrl || "/Reportes/VentasPorProductoSerie",
                type: "GET",
                dataType: "json",
                traditional: true,
                data: {
                    idCorte: idCorte,
                    fechaDesde: config.fechaDesdeActual,
                    fechaHasta: config.fechaHastaActual,
                    idSucursal: config.sucursalActual || "0",
                    tipoProducto: $("#tipoProductoReporte").val() || "",
                    marcaId: $("#marcaReporte").val() || "0",
                    agrupacionTemporal: config.agrupacionTemporalActual || "hora",
                    periodoComparativoDesde: serializeComparativePeriodsForGraph()
                }
            }).done(function (response) {
                if (!response || !response.ok) {
                    $summary.html('<div class="alert alert-warning mb-0">' + ((response && response.mensaje) || "No se pudo obtener la serie del producto.") + '</div>');
                    return;
                }

                renderVentasSummary(idCorte, response);

                var datasets = $.map(response.series || [], function (serie) {
                    return {
                        label: serie.titulo,
                        borderColor: serie.color,
                        backgroundColor: serie.color,
                        fill: false,
                        lineTension: 0,
                        data: $.map(serie.puntos || [], function (punto) { return punto.kg; }),
                        metaPuntos: serie.puntos || []
                    };
                });

                chartInstances[idCorte] = {
                    response: response,
                    lineChart: new Chart(canvas.getContext("2d"), {
                        type: "line",
                        data: {
                            labels: response.labels || [],
                            datasets: datasets
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            elements: {
                                point: {
                                    radius: 2
                                }
                            },
                            legend: {
                                display: true
                            },
                            tooltips: {
                                callbacks: {
                                    title: function (items, data) {
                                        if (!items || !items.length) {
                                            return "";
                                        }

                                        return (data.labels || [])[items[0].index] || "";
                                    },
                                    label: function (tooltipItem, data) {
                                        var dataset = (data.datasets || [])[tooltipItem.datasetIndex] || {};
                                        return (dataset.label || "Periodo") + ": " + formatNumber(tooltipItem.yLabel, 3) + " kg";
                                    },
                                    afterLabel: function (tooltipItem, data) {
                                        var dataset = (data.datasets || [])[tooltipItem.datasetIndex] || {};
                                        var meta = (dataset.metaPuntos || [])[tooltipItem.index] || {};
                                        return "Fecha real: " + (meta.fechaReal || "-");
                                    }
                                }
                            },
                            scales: {
                                yAxes: [{
                                    ticks: {
                                        beginAtZero: true,
                                        callback: function (value) { return formatNumber(value, 3); }
                                    }
                                }]
                            }
                        }
                    }),
                    agrupacionAplicada: response.agrupacionAplicada || "",
                    metric: "kg"
                };

                applyVentasMetricToChart(idCorte, "kg");
            }).fail(function () {
                $summary.html('<div class="alert alert-warning mb-0">No se pudo cargar el grafico del producto.</div>');
            });
        }

        $mainFilters.on("change input", function () {
            showPending(true);
            recalculateAllPeriodRows();
        });

        $tipoReporte.on("change", function () {
            applyReportMode(true);
        });

        $sucursal.on("change", function () {
            applyReportMode(true);
        });

        $btnAhora.on("click", function () {
            $fechaHastaDate.val(nowLocalValue()).trigger("change");
        });

        $switchDetalles.on("change", function () {
            applyDetailsMode();
        });

        $liveFilters.on("change input keyup", function () {
            applyLiveFilters();
        });

        $btnAgregarPeriodo.on("click", function () {
            var duration = getMainDurationMs();
            var baseStart = getLastComparativeStart() || parseLocalDate($fechaDesdeDate.val());
            var defaultStart = baseStart && duration > 0
                ? new Date(baseStart.getTime() - duration)
                : new Date();
            var iso = new Date(defaultStart.getTime() - (defaultStart.getTimezoneOffset() * 60000)).toISOString().slice(0, 16);
            addPeriodRow(iso);
            showPending(true);
        });

        $form.on("click", ".btn-quitar-periodo", function () {
            $(this).closest(".reportes-periodo-comparativo").remove();
            showPending(true);
        });

        $form.on("change", ".periodo-comparativo-fecha", function () {
            recalculatePeriodRow($(this).closest(".reportes-periodo-comparativo"));
            showPending(true);
        });

        $(document).on("click", ".btn-toggle-grafico-ventas", function () {
            toggleVentasChart($(this));
        });

        $(document).on("dblclick", "#tablaReporteStockActual .reporte-stock-row", function () {
            if (window.innerWidth < 768) {
                return;
            }

            openStockDetailModal($(this));
        });

        $(document).on("click", ".reporte-stock-producto-toggle", function (e) {
            e.preventDefault();
            e.stopPropagation();
            openStockDetailModal($(this).closest(".reporte-stock-row"));
        });

        $(document).on("click", "#tablaReporteStockActual .reporte-sortable-col", function () {
            sortStockRows($(this).attr("data-sort-key"), $(this).attr("data-sort-type"));
        });

        $(document).on("click", "#tablaReporteProyeccionVentasStock .reporte-sortable-col[data-sort-scope='proyeccion']", function () {
            sortProyeccionRows(
                $(this).attr("data-sort-key"),
                $(this).attr("data-sort-type"),
                $(this).attr("data-sort-index")
            );
        });

        $(document).on("click", ".btn-metrica-ventas", function () {
            var $button = $(this);
            var idCorte = $button.attr("data-idcorte");
            var metric = $button.attr("data-metrica") || "kg";
            var $group = $button.closest(".btn-group");

            $group.find(".btn-metrica-ventas").removeClass("active");
            $button.addClass("active");
            applyVentasMetricToChart(idCorte, metric);
        });

        $form.on("submit", function () {
            showPending(false);
            $("#buscarReportes").val("true");
            $("#btnBuscarReporte").prop("disabled", true).find(".btn-buscar-texto").text("Cargando...");
            $("#estadoReporteTexto").text("Cargando reporte...");
        });

        applyReportMode(false);
        applyDetailsMode();
        initializePeriodsFromConfig();
        applyLiveFilters();
        renderBalanceCharts();
        updateFloatingStack();

        $(window).on("scroll resize", function () {
            updateFloatingStack();
        });

        if (window.TableScrollSync && window.TableScrollSync.refresh) {
            window.TableScrollSync.refresh(document);
        }
    });
})(window.jQuery);
