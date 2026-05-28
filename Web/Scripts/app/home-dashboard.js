(function (window, $) {
    'use strict';

    if (!$) {
        return;
    }

    var $root = $('#dashboardHome');
    if (!$root.length) {
        return;
    }

    var urls = {
        resumen: $root.data('url-resumen'),
        ventasHora: $root.data('url-ventas-hora'),
        topProductos: $root.data('url-top-productos'),
        topDeudores: $root.data('url-top-deudores'),
        topAcreedores: $root.data('url-top-acreedores'),
        ultimasVentas: $root.data('url-ultimas-ventas'),
        ultimosElaborados: $root.data('url-ultimos-elaborados')
    };
    var puedeVerDashboard = String($root.data('puede-ver-dashboard') || '').toLowerCase() === 'true';

    var state = {
        charts: {
            ventasHora: null,
            topProductos: null
        }
    };

    function getFiltros() {
        return {
            periodo: $('#dashboardPeriodo').val() || 'hoy',
            idSucursal: $('#dashboardSucursal').val() || '0'
        };
    }

    function formatMoney(value) {
        var number = Number(value || 0);
        return '$ ' + number.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function formatKg(value) {
        var number = Number(value || 0);
        return number.toLocaleString('es-AR', { minimumFractionDigits: 3, maximumFractionDigits: 3 }) + ' kg';
    }

    function escapeHtml(text) {
        return $('<div/>').text(text == null ? '' : String(text)).html();
    }

    function buildUrl(baseUrl, params) {
        return baseUrl + (baseUrl.indexOf('?') >= 0 ? '&' : '?') + $.param(params || {});
    }

    function setLoadingTable(selector, colspan) {
        $(selector).html('<tr><td colspan="' + colspan + '" class="dashboard-loading">Cargando...</td></tr>');
    }

    function setEmptyTable(selector, colspan, message) {
        $(selector).html('<tr><td colspan="' + colspan + '" class="dashboard-empty">' + escapeHtml(message) + '</td></tr>');
    }

    function createOrUpdateChart(key, canvasId, configBuilder) {
        var canvas = document.getElementById(canvasId);
        if (!canvas || !window.Chart) {
            return;
        }

        if (state.charts[key]) {
            state.charts[key].destroy();
        }

        state.charts[key] = new Chart(canvas.getContext('2d'), configBuilder());
    }

    function cargarResumen() {
        if (!puedeVerDashboard) {
            return;
        }
        var filtros = getFiltros();
        $.getJSON(buildUrl(urls.resumen, filtros))
            .done(function (response) {
                if (!response || !response.ok || !response.data) {
                    return;
                }

                var data = response.data;
                $('[data-kpi="ventasTotales"]').text(formatMoney(data.VentasTotales));
                $('[data-kpi="cantidadVentas"]').text(data.CantidadVentas || 0);
                $('[data-kpi="cantidadClientes"]').text(data.CantidadClientes || 0);
                $('[data-kpi="promedioPorVenta"]').text(formatMoney(data.PromedioPorVenta));
                $('[data-kpi="saldoACobrar"]').text(formatMoney(data.SaldoACobrar));
                $('[data-kpi="saldoAPagar"]').text(formatMoney(data.SaldoAPagar));
                $('[data-kpi-meta="periodo"]').text((data.PeriodoEtiqueta || 'Hoy') + ' · ' + (data.SucursalEtiqueta || 'Todas'));
            });
    }

    function cargarVentasPorHora() {
        if (!puedeVerDashboard) {
            return;
        }
        var filtros = getFiltros();
        $.getJSON(buildUrl(urls.ventasHora, filtros))
            .done(function (response) {
                if (!response || !response.ok || !response.data) {
                    return;
                }

                var data = response.data;
                $('#ventasHoraScope').text((data.periodo || 'Hoy') + ' · ' + (data.sucursal || 'Todas'));

                createOrUpdateChart('ventasHora', 'dashboardVentasHoraChart', function () {
                    return {
                        type: 'line',
                        data: {
                            labels: data.labels || [],
                            datasets: [{
                                label: 'Ventas',
                                data: $.map(data.series || [], function (item) { return item.Total || 0; }),
                                borderColor: '#cb6d32',
                                backgroundColor: 'rgba(203,109,50,0.14)',
                                fill: true,
                                lineTension: 0.22,
                                pointRadius: 3,
                                pointHoverRadius: 5,
                                borderWidth: 3
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            legend: {
                                display: false
                            },
                            tooltips: {
                                callbacks: {
                                    label: function (tooltipItem) {
                                        return ' ' + formatMoney(tooltipItem.yLabel || 0);
                                    }
                                }
                            },
                            scales: {
                                yAxes: [{
                                    ticks: {
                                        beginAtZero: true,
                                        callback: function (value) {
                                            return '$ ' + Number(value || 0).toLocaleString('es-AR');
                                        }
                                    },
                                    gridLines: {
                                        color: 'rgba(22,50,79,0.08)'
                                    }
                                }],
                                xAxes: [{
                                    gridLines: {
                                        display: false
                                    }
                                }]
                            }
                        }
                    };
                });
            });
    }

    function cargarTopProductos() {
        if (!puedeVerDashboard) {
            if (state.charts.topProductos) {
                state.charts.topProductos.destroy();
                state.charts.topProductos = null;
            }
            return;
        }
        var filtros = getFiltros();
        $.getJSON(buildUrl(urls.topProductos, filtros))
            .done(function (response) {
                if (!response || !response.ok) {
                    return;
                }

                var items = response.data || [];
                if (!items.length) {
                    if (state.charts.topProductos) {
                        state.charts.topProductos.destroy();
                        state.charts.topProductos = null;
                    }
                    return;
                }

                createOrUpdateChart('topProductos', 'dashboardTopProductosChart', function () {
                    return {
                        type: 'horizontalBar',
                        data: {
                            labels: $.map(items, function (item) {
                                return item.Producto || 'Sin nombre';
                            }),
                            datasets: [{
                                label: 'Kg vendidos',
                                data: $.map(items, function (item) { return item.Kg || 0; }),
                                backgroundColor: 'rgba(45,106,138,0.82)',
                                borderColor: 'rgba(22,50,79,0.95)',
                                borderWidth: 1
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            legend: {
                                display: false
                            },
                            tooltips: {
                                callbacks: {
                                    label: function (tooltipItem, data) {
                                        var index = tooltipItem.index;
                                        var item = items[index] || {};
                                        return ' ' + formatKg(item.Kg || 0) + ' · ' + formatMoney(item.Importe || 0);
                                    }
                                }
                            },
                            scales: {
                                xAxes: [{
                                    ticks: {
                                        beginAtZero: true,
                                        callback: function (value) {
                                            return Number(value || 0).toLocaleString('es-AR', { minimumFractionDigits: 0, maximumFractionDigits: 1 });
                                        }
                                    },
                                    gridLines: {
                                        color: 'rgba(22,50,79,0.08)'
                                    }
                                }],
                                yAxes: [{
                                    gridLines: {
                                        display: false
                                    }
                                }]
                            }
                        }
                    };
                });
            });
    }

    function renderSaldoRows(selector, items) {
        if (!items || !items.length) {
            setEmptyTable(selector, 2, 'Sin datos para mostrar.');
            return;
        }

        var rows = $.map(items, function (item) {
            var nombre = item.Persona || item.Identificacion || 'Sin nombre';
            var extra = item.Identificacion ? '<div class="dashboard-subtle">' + escapeHtml(item.Identificacion) + '</div>' : '';
            return '' +
                '<tr>' +
                '<td><div class="font-weight-bold">' + escapeHtml(nombre) + '</div>' + extra + '</td>' +
                '<td class="text-right font-weight-bold">' + formatMoney(item.Saldo || 0) + '</td>' +
                '</tr>';
        }).join('');

        $(selector).html(rows);
    }

    function cargarCuentasCorrientes() {
        if (!puedeVerDashboard) {
            return;
        }
        $.getJSON(urls.topDeudores).done(function (response) {
            renderSaldoRows('#tablaTopDeudores', response && response.ok ? response.data : []);
        });

        $.getJSON(urls.topAcreedores).done(function (response) {
            renderSaldoRows('#tablaTopAcreedores', response && response.ok ? response.data : []);
        });
    }

    function cargarUltimasVentas() {
        if (!puedeVerDashboard) {
            return;
        }
        var filtros = getFiltros();
        setLoadingTable('#tablaUltimasVentas', 6);

        $.getJSON(buildUrl(urls.ultimasVentas, filtros))
            .done(function (response) {
                if (!response || !response.ok || !response.data || !response.data.length) {
                    setEmptyTable('#tablaUltimasVentas', 6, 'No hay ventas en el periodo seleccionado.');
                    return;
                }

                var rows = $.map(response.data, function (item) {
                    var detalle = item.DetalleUrl
                        ? '<a class="btn btn-sm btn-outline-primary" href="' + escapeHtml(item.DetalleUrl) + '">Ver</a>'
                        : '<span class="text-muted">-</span>';

                    return '' +
                        '<tr>' +
                        '<td>' + escapeHtml(item.FechaHora || '-') + '</td>' +
                        '<td>' + escapeHtml(item.Cliente || '-') + '</td>' +
                        '<td class="text-right font-weight-bold">' + formatMoney(item.Total || 0) + '</td>' +
                        '<td>' + escapeHtml(item.Usuario || '-') + '</td>' +
                        '<td>' + escapeHtml(item.Sucursal || '-') + '</td>' +
                        '<td class="text-center">' + detalle + '</td>' +
                        '</tr>';
                }).join('');

                $('#tablaUltimasVentas').html(rows);
            });
    }

    function cargarUltimosElaborados() {
        if (!puedeVerDashboard) {
            return;
        }
        var filtros = getFiltros();
        setLoadingTable('#tablaUltimosElaborados', 4);

        $.getJSON(buildUrl(urls.ultimosElaborados, filtros))
            .done(function (response) {
                if (!response || !response.ok || !response.data || !response.data.length) {
                    setEmptyTable('#tablaUltimosElaborados', 4, 'No hay elaborados en el periodo seleccionado.');
                    return;
                }

                var rows = $.map(response.data, function (item) {
                    return '' +
                        '<tr>' +
                        '<td>' + escapeHtml(item.Fecha || '-') + '</td>' +
                        '<td><div class="font-weight-bold">' + escapeHtml(item.Producto || '-') + '</div></td>' +
                        '<td class="text-right font-weight-bold">' + Number(item.Cantidad || 0).toLocaleString('es-AR', { minimumFractionDigits: 3, maximumFractionDigits: 3 }) + '</td>' +
                        '<td>' + escapeHtml(item.Sucursal || '-') + '</td>' +
                        '</tr>';
                }).join('');

                $('#tablaUltimosElaborados').html(rows);
            });
    }

    function renderEstadoBalanza(status, peso) {
        var dot = $('#balanzaEstadoDot');
        var estadoTexto = $('#balanzaEstadoTexto');
        var estadoSecundario = $('#balanzaEstadoSecundario');
        var puertoTexto = $('#balanzaPuertoTexto');
        var badge = $('#balanzaBadge');
        var pesoActual = $('#balanzaPesoActual');

        var normalized = window.CarnisysBalanzaUtils && window.CarnisysBalanzaUtils.normalize
            ? window.CarnisysBalanzaUtils.normalize(peso || status || {})
            : null;

        var ok = !!(status && status.ok);
        var conectada = normalized ? normalized.conectada : false;
        var puerto = normalized && normalized.puerto ? normalized.puerto : '';

        dot.removeClass('is-online is-offline');

        if (!ok) {
            dot.addClass('is-offline');
            estadoTexto.text('Agente no detectado');
            estadoSecundario.text('Sin respuesta local');
            badge.text('Offline').removeClass('badge-success badge-warning').addClass('badge-secondary');
            puertoTexto.text('Puerto sin detectar');
            return;
        }

        if (conectada) {
            dot.addClass('is-online');
            estadoTexto.text('Conectada');
            estadoSecundario.text(normalized.inestable ? 'Lectura inestable' : 'Lectura estable');
            badge.text(normalized.inestable ? 'Inestable' : 'Online').removeClass('badge-secondary badge-success badge-warning').addClass(normalized.inestable ? 'badge-warning' : 'badge-success');
            puertoTexto.text(puerto ? ('Puerto ' + puerto) : 'Puerto configurado');
            if (normalized.pesoDisplay) {
                pesoActual.text((normalized.pesoDisplay || '0.000').replace(' i', '') + ' kg');
            }
        } else {
            dot.addClass('is-offline');
            estadoTexto.text('Desconectada');
            estadoSecundario.text('Balanza sin conexion');
            badge.text('Desconectada').removeClass('badge-secondary badge-success').addClass('badge-warning');
            puertoTexto.text(puerto ? ('Puerto ' + puerto) : 'Sin puerto detectado');
            pesoActual.text('0.000 kg');
        }
    }

    function initBalanza() {
        if (!window.CarnisysBalanza) {
            return;
        }

        var ultimoStatus = null;
        var ultimoPeso = null;

        window.CarnisysBalanza.start({
            onStatus: function (status) {
                ultimoStatus = status || null;
                renderEstadoBalanza(ultimoStatus, ultimoPeso);
            },
            onPeso: function (peso) {
                ultimoPeso = peso || null;
                renderEstadoBalanza(ultimoStatus, ultimoPeso);
                $('#balanzaUltimaLectura').text(new Date().toLocaleTimeString());
            },
            onError: function () {
                renderEstadoBalanza(ultimoStatus, ultimoPeso);
            }
        });

        window.CarnisysBalanza.activar();
    }

    function recargarDashboard() {
        if (!puedeVerDashboard) {
            return;
        }
        cargarResumen();
        cargarVentasPorHora();
        cargarTopProductos();
        cargarCuentasCorrientes();
        cargarUltimasVentas();
        cargarUltimosElaborados();
    }

    $(function () {
        if (puedeVerDashboard) {
            setLoadingTable('#tablaTopDeudores', 2);
            setLoadingTable('#tablaTopAcreedores', 2);
        } else {
            setEmptyTable('#tablaTopDeudores', 2, 'Disponible solo para administradores.');
            setEmptyTable('#tablaTopAcreedores', 2, 'Disponible solo para administradores.');
            setEmptyTable('#tablaUltimasVentas', 6, 'Disponible solo para administradores.');
            setEmptyTable('#tablaUltimosElaborados', 4, 'Disponible solo para administradores.');
        }
        initBalanza();
        recargarDashboard();

        $('#dashboardPeriodo, #dashboardSucursal').on('change', function () {
            recargarDashboard();
        });
    });
})(window, window.jQuery);
