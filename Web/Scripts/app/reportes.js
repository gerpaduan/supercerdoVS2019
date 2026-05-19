(function ($) {
    if (!$) {
        return;
    }

    function toNumber(value) {
        if (value === null || value === undefined || value === "") {
            return 0;
        }

        var n = parseFloat(String(value).replace(",", "."));
        return isNaN(n) ? 0 : n;
    }

    function formatNumber(value) {
        return toNumber(value).toLocaleString("es-AR", {
            minimumFractionDigits: 3,
            maximumFractionDigits: 3
        });
    }

    $(function () {
        var $form = $("#formReportes");
        if (!$form.length) {
            return;
        }

        var $mainFilters = $form.find(".filtro-principal");
        var $liveFilters = $form.find(".filtro-vivo");
        var $pendingCard = $("#cardAvisoPendienteReporte");
        var $table = $("#tablaReporteStockActual");
        var $rows = $table.find(".reporte-stock-row");
        var chart = null;
        var config = window.reportesConfig || {};
        var cierres = config.cierres || [];
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

            var topbarHeight = $(".topbar").outerHeight() || 0;
            var topOffset = topbarHeight + 12;
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

        function reportRequiresSpecificBranch() {
            var tipo = $.trim($tipoReporte.val() || "");
            return tipo === "Stock Actual" || tipo === "Cierre Stock" || tipo === "Stock Retroactivo";
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

        function nowLocalValue() {
            var now = new Date();
            var offset = now.getTimezoneOffset();
            var local = new Date(now.getTime() - (offset * 60000));
            return local.toISOString().slice(0, 16);
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
        }

        function applyReportMode(resetValues) {
            var tipo = $.trim($tipoReporte.val() || "");
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

        function showPending(show) {
            $pendingCard.toggleClass("d-none", !show);
        }

        function updateVisibleTotals() {
            if (!$rows.length) {
                return;
            }

            var totalKg = 0;
            var totalIngresos = 0;
            var totalEgresos = 0;
            var productos = {};
            var registros = 0;
            var chartData = [];

            $rows.each(function () {
                var $row = $(this);
                if ($row.hasClass("d-none")) {
                    return;
                }

                registros += 1;
                productos[$row.attr("data-idcorte")] = true;
                totalKg += toNumber($row.attr("data-stockactual"));
                totalIngresos += toNumber($row.attr("data-totalingresos"));
                totalEgresos += toNumber($row.attr("data-totalegresos"));

                chartData.push({
                    label: $.trim($row.attr("data-producto") || "") + (($row.attr("data-codigo") || "") ? " / " + $.trim($row.attr("data-codigo")) : ""),
                    value: toNumber($row.attr("data-stockactual"))
                });
            });

            $("#totalKgReporte").text(formatNumber(totalKg));
            $("#totalIngresosReporte").text(formatNumber(totalIngresos));
            $("#totalEgresosReporte").text(formatNumber(totalEgresos));
            $("#cantidadProductosReporte").text(Object.keys(productos).length);
            $("#cantidadRegistrosReporte").text(registros);
            $("#filaSinResultadosReporte").toggleClass("d-none", registros !== 0);

            updateChart(chartData);
        }

        function updateChart(chartData) {
            var canvas = document.getElementById("graficoReporteStockActual");
            var $card = $("#cardGraficoReporte");
            if (!canvas || typeof Chart === "undefined") {
                return;
            }

            chartData.sort(function (a, b) { return b.value - a.value; });
            chartData = chartData.slice(0, 10);

            if (!chartData.length) {
                $card.addClass("d-none");
                if (chart) {
                    chart.destroy();
                    chart = null;
                }
                return;
            }

            $card.removeClass("d-none");

            if (chart) {
                chart.destroy();
            }

            chart = new Chart(canvas.getContext("2d"), {
                type: "bar",
                data: {
                    labels: chartData.map(function (x) { return x.label; }),
                    datasets: [{
                        label: "Stock actual",
                        data: chartData.map(function (x) { return x.value; }),
                        backgroundColor: "rgba(54, 123, 245, 0.65)",
                        borderColor: "rgba(54, 123, 245, 1)",
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    legend: { display: false },
                    scales: {
                        yAxes: [{
                            ticks: {
                                beginAtZero: true,
                                callback: function (value) { return formatNumber(value); }
                            }
                        }]
                    }
                }
            });
        }

        function applyLiveFilters() {
            if (!$rows.length) {
                return;
            }

            var text = ($.trim($("#busquedaProductoReporte").val() || "")).toUpperCase();
            var tipo = ($.trim($("#tipoProductoReporte").val() || "")).toUpperCase();
            var marcaId = $("#marcaReporte").val() || "0";
            var estado = ($.trim($("#estadoStockReporte").val() || "Todos")).toUpperCase();

            $rows.each(function () {
                var $row = $(this);
                var search = ($row.attr("data-search") || "").toUpperCase();
                var rowTipo = ($row.attr("data-tipo") || "").toUpperCase();
                var rowMarca = $row.attr("data-marcaid") || "0";
                var rowEstado = ($row.attr("data-estado") || "").toUpperCase();

                var matchText = !text || search.indexOf(text) >= 0;
                var matchTipo = !tipo || rowTipo === tipo;
                var matchMarca = marcaId === "0" || rowMarca === marcaId;
                var matchEstado = !estado || estado === "TODOS" || rowEstado === estado;
                var visible = matchText && matchTipo && matchMarca && matchEstado;

                $row.toggleClass("d-none", !visible);
            });

            updateVisibleTotals();
        }

        $mainFilters.on("change input", function () {
            showPending(true);
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

        $form.on("submit", function () {
            showPending(false);
            $("#buscarReportes").val("true");
        });

        applyReportMode(false);
        applyDetailsMode();
        applyLiveFilters();
        updateFloatingStack();

        $(window).on("scroll resize", function () {
            updateFloatingStack();
        });
    });
})(window.jQuery);
