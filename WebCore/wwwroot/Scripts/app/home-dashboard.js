// Port literal de Web/Scripts/app/home-dashboard.js (2026-09-07, pedido explicito del usuario --
// ver docs/DECISIONS.md). Orquesta la carga progresiva por AJAX del dashboard post-login
// (Home/Index): resumen/KPIs, ventas por hora (Chart.js line), top productos (Chart.js
// horizontalBar), cuentas corrientes, finanzas, ultimas ventas/elaborados/movimientos, y el
// estado de la balanza local via CarnisysBalanza. Sin cambios de logica: los endpoints relativos
// se resuelven server-side en los data-url-* de Home/Index.cshtml, y los nombres de accion
// coinciden 1:1 con WebCore/Controllers/HomeController.cs.
//
// Bug real (2026-09-07, ver docs/DECISIONS.md): el original de Web clasico serializaba con
// Newtonsoft.Json (PascalCase por default), pero WebCore usa System.Text.Json (camelCase por
// default, WebCore/Program.cs sin AddJsonOptions) -- mismo criterio que el resto de los modulos
// ya migrados (ver reportes.js). Este archivo, portado literal, seguia leyendo las propiedades
// en PascalCase (item.Total, data.VentasTotales, etc.) que no existen en el JSON real -- en JS
// leer una propiedad inexistente da "undefined" sin tirar error, asi que el dashboard se veia
// vacio ($0,00 / 0) sin ningun error de consola. Fix: todas las lecturas de propiedades pasan a
// camelCase, igual que ya lee reportes.js.
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
        actividades: $root.data('url-actividades'),
        ventasHora: $root.data('url-ventas-hora'),
        topProductos: $root.data('url-top-productos'),
        topDeudores: $root.data('url-top-deudores'),
        topAcreedores: $root.data('url-top-acreedores'),
        ultimasVentas: $root.data('url-ultimas-ventas'),
        ultimosElaborados: $root.data('url-ultimos-elaborados'),
        movimientosDashboard: $root.data('url-movimientos-dashboard'),
        finanzas: $root.data('url-finanzas')
    };
    var puedeVerDashboard = String($root.data('puede-ver-dashboard') || '').toLowerCase() === 'true';

    var state = {
        charts: {
            ventasHora: null,
            topProductos: null
        },
        reloadToken: 0,
        activeRequests: []
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

    function formatMoneyAbs(value) {
        var number = Math.abs(Number(value || 0));
        return '$ ' + number.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // Totales del tablero (2026-11-10, ver docs/DECISIONS.md "Dashboard: totales y actividades"): sin centavos para que
    // los montos grandes entren en una linea; el importe exacto con centavos queda en el tooltip (title).
    // absoluto=true para el saldo a cobrar, que el backend devuelve negativo (mismo criterio que formatMoneyAbs).
    function setKpiMoney(kpi, value, absoluto) {
        var number = Number(value || 0);
        if (absoluto) {
            number = Math.abs(number);
        }

        $('[data-kpi="' + kpi + '"]')
            .text('$ ' + number.toLocaleString('es-AR', { minimumFractionDigits: 0, maximumFractionDigits: 0 }))
            .attr('title', formatMoney(number));
    }

    function formatCount(value) {
        return Number(value || 0).toLocaleString('es-AR');
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

    function removeActiveRequest(xhr) {
        state.activeRequests = $.grep(state.activeRequests, function (item) {
            return item !== xhr;
        });
    }

    function abortActiveRequests() {
        $.each(state.activeRequests.slice(0), function (_, xhr) {
            if (xhr && xhr.readyState !== 4) {
                xhr.abort();
            }
        });

        state.activeRequests = [];
    }

    function getJsonSilently(url) {
        var xhr = $.ajax({
            url: url,
            method: 'GET',
            dataType: 'json',
            silentLoading: true
        });

        state.activeRequests.push(xhr);
        xhr.always(function () {
            removeActiveRequest(xhr);
        });

        return xhr;
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

    function cargarResumen(token) {
        if (!puedeVerDashboard) {
            return $.Deferred().resolve().promise();
        }
        var filtros = getFiltros();
        return getJsonSilently(buildUrl(urls.resumen, filtros))
            .done(function (response) {
                if (token !== state.reloadToken) {
                    return;
                }

                if (!response || !response.ok || !response.data) {
                    return;
                }

                var data = response.data;
                setKpiMoney('ventasTotales', data.ventasTotales, false);
                $('[data-kpi="cantidadVentas"]').text(formatCount(data.cantidadVentas));
                $('[data-kpi="cantidadClientes"]').text(formatCount(data.cantidadClientes));
                setKpiMoney('promedioPorVenta', data.promedioPorVenta, false);
                setKpiMoney('saldoACobrar', data.saldoACobrar, true);
                setKpiMoney('saldoAPagar', data.saldoAPagar, false);
                $('[data-kpi-meta="periodo"]').text((data.periodoEtiqueta || 'Hoy') + ' · ' + (data.sucursalEtiqueta || 'Todas'));
            });
    }

    function cargarVentasPorHora(token) {
        if (!puedeVerDashboard) {
            return $.Deferred().resolve().promise();
        }
        var filtros = getFiltros();
        return getJsonSilently(buildUrl(urls.ventasHora, filtros))
            .done(function (response) {
                if (token !== state.reloadToken) {
                    return;
                }

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
                                data: $.map(data.series || [], function (item) { return item.total || 0; }),
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

    function cargarTopProductos(token) {
        if (!puedeVerDashboard) {
            if (state.charts.topProductos) {
                state.charts.topProductos.destroy();
                state.charts.topProductos = null;
            }
            return $.Deferred().resolve().promise();
        }
        var filtros = getFiltros();
        return getJsonSilently(buildUrl(urls.topProductos, filtros))
            .done(function (response) {
                if (token !== state.reloadToken) {
                    return;
                }

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
                                return item.producto || 'Sin nombre';
                            }),
                            datasets: [{
                                label: 'Kg vendidos',
                                data: $.map(items, function (item) { return item.kg || 0; }),
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
                                        return ' ' + formatKg(item.kg || 0) + ' · ' + formatMoney(item.importe || 0);
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
            var nombre = item.persona || item.identificacion || 'Sin nombre';
            var extra = item.identificacion ? '<div class="dashboard-subtle">' + escapeHtml(item.identificacion) + '</div>' : '';
            return '' +
                '<tr>' +
                '<td><div class="font-weight-bold">' + escapeHtml(nombre) + '</div>' + extra + '</td>' +
                '<td class="text-right font-weight-bold">' + formatMoney(item.saldo || 0) + '</td>' +
                '</tr>';
        }).join('');

        $(selector).html(rows);
    }

    function cargarCuentasCorrientes(token) {
        if (!puedeVerDashboard) {
            return $.Deferred().resolve().promise();
        }
        return getJsonSilently(urls.topDeudores)
            .done(function (response) {
                if (token !== state.reloadToken) {
                    return;
                }

                renderSaldoRows('#tablaTopDeudores', response && response.ok ? response.data : []);
            })
            .then(function () {
                if (token !== state.reloadToken) {
                    return $.Deferred().resolve().promise();
                }

                return getJsonSilently(urls.topAcreedores)
                    .done(function (response) {
                        if (token !== state.reloadToken) {
                            return;
                        }

                        renderSaldoRows('#tablaTopAcreedores', response && response.ok ? response.data : []);
                    });
            });
    }

    // Tipo de actividad -> [icono Font Awesome, color del circulo] (2026-11-10). Los textos salen de
    // WebCore/Services/ActividadesFeedService.cs; los subtipos de compra (Cortes, Media Res, Ingreso Stock...)
    // no estan en el mapa y caen en ACTIVIDAD_ICONO_DEFAULT. Las advertencias del POS usan siempre el icono de alerta.
    var ACTIVIDAD_ICONOS = {
        'Precio': ['fa-tag', 'is-blue'],
        'Venta anulada': ['fa-ban', 'is-red'],
        'Precio modificado en venta': ['fa-percent', 'is-red'],
        'Egreso de caja': ['fa-money-bill-wave', 'is-gray'],
        'Movimiento': ['fa-truck', 'is-gray'],
        'Elaborado': ['fa-flask', 'is-gray'],
        'Autorización POS': ['fa-user-shield', 'is-blue']
    };
    var ACTIVIDAD_ICONO_DEFAULT = ['fa-shopping-cart', 'is-gray'];
    var ACTIVIDAD_ICONO_ADVERTENCIA = ['fa-exclamation-triangle', 'is-warn'];
    // Filas visibles despues de agrupar repetidos; el backend manda algunas de mas (ver
    // HomeController.ObtenerUltimasActividadesDashboard) para que el agrupado no deje la lista corta.
    var ACTIVIDADES_FILAS_VISIBLES = 10;

    function pad2(n) {
        return (n < 10 ? '0' : '') + n;
    }

    // "20:06" si es de hoy, "ayer 17:10", si no "08/11 11:05". fechaIso viene sin zona (hora local del servidor,
    // la misma que usa el resto del sistema). Si no se puede leer, cae al texto "dd/MM HH:mm" que manda el backend.
    function horaRelativaActividad(item) {
        var match = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})/.exec(item.fechaIso || '');
        if (!match) {
            return item.fecha || '-';
        }

        var fecha = new Date(+match[1], +match[2] - 1, +match[3]);
        var ahora = new Date();
        var hoy = new Date(ahora.getFullYear(), ahora.getMonth(), ahora.getDate());
        var dias = Math.round((hoy - fecha) / 86400000);
        var hhmm = match[4] + ':' + match[5];

        if (dias === 0) {
            return hhmm;
        }
        if (dias === 1) {
            return 'ayer ' + hhmm;
        }
        return match[3] + '/' + match[2] + ' ' + hhmm;
    }

    // Junta actividades consecutivas con el mismo tipo y descripcion (la lista ya viene ordenada por fecha desc):
    // queda la mas reciente con un contador. Caso real: varias "Sesion cerrada con venta en curso" iguales seguidas.
    function agruparActividadesRepetidas(items) {
        var agrupadas = [];
        $.each(items, function (_, item) {
            var anterior = agrupadas[agrupadas.length - 1];
            if (anterior && anterior.tipo === item.tipo && anterior.descripcion === item.descripcion) {
                anterior.veces++;
                return;
            }
            agrupadas.push($.extend({ veces: 1 }, item));
        });
        return agrupadas;
    }

    // Arma una fila. La descripcion se escapa siempre; si la actividad es de una venta, el "#idVenta" del texto pasa a
    // ser el link a DetalleVenta (si el texto no lo contiene, se agrega un link "Ver venta" al final).
    function renderActividad(item) {
        var icono = item.esAdvertencia ? ACTIVIDAD_ICONO_ADVERTENCIA : (ACTIVIDAD_ICONOS[item.tipo] || ACTIVIDAD_ICONO_DEFAULT);
        var descripcion = escapeHtml(item.descripcion || '-');

        if (item.detalleUrl && item.idVenta) {
            var marca = '#' + item.idVenta;
            var link = '<a href="' + escapeHtml(item.detalleUrl) + '">';
            descripcion = descripcion.indexOf(marca) >= 0
                ? descripcion.replace(marca, link + marca + '</a>')
                : descripcion + ' ' + link + 'Ver venta</a>';
        }

        return '' +
            '<div class="act-item' + (item.esAdvertencia ? ' is-warn' : '') + '">' +
            '<span class="act-ico ' + icono[1] + '"><i class="fas ' + icono[0] + '"></i></span>' +
            '<div class="act-body">' +
            '<div class="act-tipo">' + escapeHtml(item.tipo || '-') +
            (item.veces > 1 ? '<span class="act-veces" title="Se repitió ' + item.veces + ' veces seguidas">×' + item.veces + '</span>' : '') +
            '</div>' +
            '<div class="act-desc">' + descripcion + '</div>' +
            '</div>' +
            '<span class="act-hora">' + escapeHtml(horaRelativaActividad(item)) + '</span>' +
            '</div>';
    }

    // Item 4 de la segunda ronda de pedidos (2026-09-10, ver docs/DECISIONS.md "Batch 8:
    // Actividades en el dashboard") -- ultimas actividades (mezcla de fuentes heterogeneas,
    // ver HomeController.ObtenerUltimasActividadesDashboard/ActividadesFeedService). Rediseño 2026-11-10: icono por
    // tipo, hora relativa, advertencias resaltadas y repetidos consecutivos agrupados (renderActividad).
    function cargarActividades(token) {
        if (!puedeVerDashboard || !urls.actividades) {
            return $.Deferred().resolve().promise();
        }
        var filtros = getFiltros();
        $('#dashboardActividadesLista').html('<div class="dashboard-subtle text-center py-3">Cargando...</div>');

        return getJsonSilently(buildUrl(urls.actividades, filtros))
            .done(function (response) {
                if (token !== state.reloadToken) {
                    return;
                }

                if (!response || !response.ok || !response.data || !response.data.length) {
                    $('#dashboardActividadesLista').html('<div class="dashboard-empty text-center py-3">No hay actividad en el periodo seleccionado.</div>');
                    return;
                }

                var filas = $.map(agruparActividadesRepetidas(response.data).slice(0, ACTIVIDADES_FILAS_VISIBLES), renderActividad).join('');
                $('#dashboardActividadesLista').html(filas);
            });
    }

    function cargarUltimasVentas(token) {
        if (!puedeVerDashboard) {
            return $.Deferred().resolve().promise();
        }
        var filtros = getFiltros();
        setLoadingTable('#tablaUltimasVentas', 6);

        return getJsonSilently(buildUrl(urls.ultimasVentas, filtros))
            .done(function (response) {
                if (token !== state.reloadToken) {
                    return;
                }

                if (!response || !response.ok || !response.data || !response.data.length) {
                    setEmptyTable('#tablaUltimasVentas', 6, 'No hay ventas en el periodo seleccionado.');
                    return;
                }

                var rows = $.map(response.data, function (item) {
                    var detalle = item.detalleUrl
                        ? '<a class="btn btn-sm btn-outline-primary" href="' + escapeHtml(item.detalleUrl) + '">Ver</a>'
                        : '<span class="text-muted">-</span>';

                    return '' +
                        '<tr>' +
                        '<td>' + escapeHtml(item.fechaHora || '-') + '</td>' +
                        '<td>' + escapeHtml(item.cliente || '-') + '</td>' +
                        '<td class="text-right font-weight-bold">' + formatMoney(item.total || 0) + '</td>' +
                        '<td>' + escapeHtml(item.usuario || '-') + '</td>' +
                        '<td>' + escapeHtml(item.sucursal || '-') + '</td>' +
                        '<td class="text-center">' + detalle + '</td>' +
                        '</tr>';
                }).join('');

                $('#tablaUltimasVentas').html(rows);
            });
    }

    function cargarUltimosElaborados(token) {
        if (!puedeVerDashboard) {
            return $.Deferred().resolve().promise();
        }
        var filtros = getFiltros();
        setLoadingTable('#tablaUltimosElaborados', 4);

        return getJsonSilently(buildUrl(urls.ultimosElaborados, filtros))
            .done(function (response) {
                if (token !== state.reloadToken) {
                    return;
                }

                if (!response || !response.ok || !response.data || !response.data.length) {
                    setEmptyTable('#tablaUltimosElaborados', 4, 'No hay elaborados en el periodo seleccionado.');
                    return;
                }

                var rows = $.map(response.data, function (item) {
                    return '' +
                        '<tr>' +
                        '<td>' + escapeHtml(item.fecha || '-') + '</td>' +
                        '<td><div class="font-weight-bold">' + escapeHtml(item.producto || '-') + '</div></td>' +
                        '<td class="text-right font-weight-bold">' + Number(item.cantidad || 0).toLocaleString('es-AR', { minimumFractionDigits: 3, maximumFractionDigits: 3 }) + '</td>' +
                        '<td>' + escapeHtml(item.sucursal || '-') + '</td>' +
                        '</tr>';
                }).join('');

                $('#tablaUltimosElaborados').html(rows);
            });
    }

    function cargarUltimosMovimientosDashboard(token) {
        if (!puedeVerDashboard) {
            return $.Deferred().resolve().promise();
        }

        setLoadingTable('#tablaUltimosMovimientosDashboard', 3);

        return getJsonSilently(urls.movimientosDashboard)
            .done(function (response) {
                if (token !== state.reloadToken) {
                    return;
                }

                if (!response || !response.ok || !response.data || !response.data.length) {
                    setEmptyTable('#tablaUltimosMovimientosDashboard', 3, 'No hay movimientos recientes.');
                    return;
                }

                var rows = $.map(response.data, function (item) {
                    return '' +
                        '<tr>' +
                        '<td>' + escapeHtml(item.fecha || '-') + '</td>' +
                        '<td>' + escapeHtml(item.origen || '-') + '</td>' +
                        '<td>' + escapeHtml(item.destino || '-') + '</td>' +
                        '</tr>';
                }).join('');

                $('#tablaUltimosMovimientosDashboard').html(rows);
            })
            .fail(function () {
                if (token !== state.reloadToken) {
                    return;
                }

                setEmptyTable('#tablaUltimosMovimientosDashboard', 3, 'No hay movimientos recientes.');
            });
    }

    function renderResumenFinanzas(resumen) {
        var rows = [
            {
                label: 'Cuentas con saldo',
                value: Number(resumen && resumen.cantidadConSaldo || 0).toLocaleString('es-AR'),
                meta: (Number(resumen && resumen.cantidadAcreedores || 0).toLocaleString('es-AR')) + ' acreedores'
            },
            {
                label: 'A cobrar',
                value: Math.abs(Number(resumen && resumen.cantidadDeudores || 0)).toLocaleString('es-AR'),
                meta: 'Personas con saldo a cobrar'
            },
            {
                label: 'Total a cobrar',
                value: formatMoneyAbs(resumen && resumen.totalACobrar || 0),
                meta: 'Saldo pendiente de cobro'
            },
            {
                label: 'Total a pagar',
                value: formatMoney(resumen && resumen.totalAPagar || 0),
                meta: 'Saldo pendiente de pago'
            }
        ];

        var html = $.map(rows, function (item) {
            return '' +
                '<div class="col-sm-6 mb-3">' +
                '<div class="dashboard-finanzas-resumen">' +
                '<div class="summary-label">' + escapeHtml(item.label) + '</div>' +
                '<div class="summary-value">' + escapeHtml(item.value) + '</div>' +
                '<div class="dashboard-subtle mt-2">' + escapeHtml(item.meta) + '</div>' +
                '</div>' +
                '</div>';
        }).join('');

        $('#resumenFinanzasCtaCte').html(html);
    }

    function cargarFinanzas(token) {
        if (!puedeVerDashboard) {
            return $.Deferred().resolve().promise();
        }

        setLoadingTable('#tablaFinanzasMovimientos', 4);
        setLoadingTable('#tablaFinanzasCheques', 7);

        return getJsonSilently(urls.finanzas)
            .done(function (response) {
                if (token !== state.reloadToken) {
                    return;
                }

                if (!response || !response.ok || !response.data) {
                    setEmptyTable('#tablaFinanzasMovimientos', 4, 'No hay movimientos recientes.');
                    setEmptyTable('#tablaFinanzasCheques', 7, 'No hay cheques vencidos.');
                    return;
                }

                var data = response.data || {};
                renderResumenFinanzas(data.resumen || {});

                if (!data.movimientos || !data.movimientos.length) {
                    setEmptyTable('#tablaFinanzasMovimientos', 4, 'No hay movimientos recientes.');
                } else {
                    var rowsMovimientos = $.map(data.movimientos, function (item) {
                        return '' +
                            '<tr>' +
                            '<td>' + escapeHtml(item.fecha || '-') + '</td>' +
                            '<td>' + escapeHtml(item.persona || '-') + '</td>' +
                            '<td>' + escapeHtml(item.tipo || '-') + '</td>' +
                            '<td class="text-right font-weight-bold">' + formatMoney(item.monto || 0) + '</td>' +
                            '</tr>';
                    }).join('');
                    $('#tablaFinanzasMovimientos').html(rowsMovimientos);
                }

                if (!data.cheques || !data.cheques.length) {
                    setEmptyTable('#tablaFinanzasCheques', 7, 'No hay cheques vencidos.');
                } else {
                    var rowsCheques = $.map(data.cheques, function (item) {
                        var obsClass = '';
                        if ((item.observacionCalculada || '') === 'Cheque vencido') {
                            obsClass = ' is-danger';
                        } else if ((item.observacionCalculada || '') === 'Listo para cobrar') {
                            obsClass = ' is-warning';
                        }

                        var observacion = item.observacionCalculada
                            ? '<span class="dashboard-cheque-obs' + obsClass + '">' + escapeHtml(item.observacionCalculada) + '</span>'
                            : '<span class="text-muted">-</span>';

                        return '' +
                            '<tr>' +
                            '<td>' + escapeHtml(item.nroCheque || '-') + '</td>' +
                            '<td>' + escapeHtml(item.banco || '-') + '</td>' +
                            '<td>' + escapeHtml(item.titular || '-') + '</td>' +
                            '<td>' + escapeHtml(item.fechaPago || '-') + '</td>' +
                            '<td class="text-right font-weight-bold">' + formatMoney(item.importe || 0) + '</td>' +
                            '<td>' + escapeHtml(item.estado || '-') + '</td>' +
                            '<td>' + observacion + '</td>' +
                            '</tr>';
                    }).join('');
                    $('#tablaFinanzasCheques').html(rowsCheques);
                }
            })
            .fail(function () {
                if (token !== state.reloadToken) {
                    return;
                }

                setEmptyTable('#tablaFinanzasMovimientos', 4, 'No hay movimientos recientes.');
                setEmptyTable('#tablaFinanzasCheques', 7, 'No hay cheques vencidos.');
            });
    }

    function runDashboardStep(sequence, task, token) {
        return sequence.then(function () {
            if (token !== state.reloadToken) {
                return $.Deferred().resolve().promise();
            }

            var request = task(token);
            if (request && $.isFunction(request.always)) {
                return request;
            }

            return $.Deferred().resolve().promise();
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

        state.reloadToken += 1;
        abortActiveRequests();

        var token = state.reloadToken;
        var sequence = $.Deferred().resolve().promise();

        sequence = runDashboardStep(sequence, cargarResumen, token);
        sequence = runDashboardStep(sequence, cargarActividades, token);
        sequence = runDashboardStep(sequence, cargarVentasPorHora, token);
        sequence = runDashboardStep(sequence, cargarTopProductos, token);
        sequence = runDashboardStep(sequence, cargarCuentasCorrientes, token);
        sequence = runDashboardStep(sequence, cargarFinanzas, token);
        sequence = runDashboardStep(sequence, cargarUltimasVentas, token);
        sequence = runDashboardStep(sequence, cargarUltimosElaborados, token);
        runDashboardStep(sequence, cargarUltimosMovimientosDashboard, token);
    }

    $(function () {
        if (puedeVerDashboard) {
            setLoadingTable('#tablaTopDeudores', 2);
            setLoadingTable('#tablaTopAcreedores', 2);
            setLoadingTable('#tablaFinanzasMovimientos', 4);
            setLoadingTable('#tablaFinanzasCheques', 7);
            setLoadingTable('#tablaUltimosMovimientosDashboard', 3);
        } else {
            setEmptyTable('#tablaTopDeudores', 2, 'Disponible solo para administradores.');
            setEmptyTable('#tablaTopAcreedores', 2, 'Disponible solo para administradores.');
            setEmptyTable('#tablaFinanzasMovimientos', 4, 'Disponible solo para administradores.');
            setEmptyTable('#tablaFinanzasCheques', 7, 'Disponible solo para administradores.');
            setEmptyTable('#tablaUltimasVentas', 6, 'Disponible solo para administradores.');
            setEmptyTable('#tablaUltimosElaborados', 4, 'Disponible solo para administradores.');
            setEmptyTable('#tablaUltimosMovimientosDashboard', 3, 'Disponible solo para administradores.');
        }
        initBalanza();
        recargarDashboard();

        $('#dashboardPeriodo, #dashboardSucursal').on('change', function () {
            recargarDashboard();
        });
    });
})(window, window.jQuery);
