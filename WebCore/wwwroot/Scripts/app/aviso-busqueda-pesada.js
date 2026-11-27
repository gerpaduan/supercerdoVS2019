/* Aviso antes de una busqueda pesada (2026-11-26, ver docs/DECISIONS.md). Reusable: la pantalla llama a
   AvisoBusquedaPesada.confirmar(urlContar, pantalla, filtros) ANTES de recargar y recibe { continuar, mensaje }. El servidor
   cuenta y decide si es pesada ({ ok, pesada, cantidad, unidad }). Si no lo es, o si el conteo falla o tarda demasiado, se
   busca sin preguntar: el aviso nunca impide una consulta. Usa SweetAlert2 si esta cargado; si no, window.confirm.
   armarUrl y textos son puros y tienen test (WebCore.Tests/js/aviso-busqueda-pesada.test.js). */
(function (root) {
    'use strict';

    // Cuanto se espera el conteo antes de seguir sin preguntar.
    var ESPERA_MAXIMA_MS = 15000;

    // Filtros vacios no viajan: el servidor usa su valor por defecto.
    function armarUrl(urlContar, pantalla, filtros) {
        var partes = ['pantalla=' + encodeURIComponent(pantalla)];
        ['fechaDesde', 'fechaHasta', 'idSucursal'].forEach(function (clave) {
            var valor = filtros ? filtros[clave] : '';
            if (valor !== undefined && valor !== null && valor !== '') partes.push(clave + '=' + encodeURIComponent(valor));
        });
        return urlContar + (urlContar.indexOf('?') >= 0 ? '&' : '?') + partes.join('&');
    }

    // '2026-11-26T00:00' -> '26/11/2026' (cadena vacia si no es una fecha).
    function fechaCorta(valor) {
        var m = /^(\d{4})-(\d{2})-(\d{2})/.exec(String(valor || ''));
        return m ? m[3] + '/' + m[2] + '/' + m[1] : '';
    }

    // Textos del aviso y del cartel de carga. cantidad y unidad vienen del servidor propio; igual se fuerzan a numero y texto plano.
    function textos(respuesta, filtros) {
        var cantidad = Number(respuesta.cantidad || 0).toLocaleString('es-AR');
        var unidad = String(respuesta.unidad || 'registros').replace(/[<>&"]/g, '');
        var desde = fechaCorta(filtros && filtros.fechaDesde);
        var hasta = fechaCorta(filtros && filtros.fechaHasta);
        var periodo = desde && hasta ? 'Del ' + desde + ' al ' + hasta + ' hay' : 'La búsqueda trae';
        return {
            periodo: periodo,
            cifra: cantidad + ' ' + unidad,
            advertencia: 'Cargarlas puede tardar varios segundos o minutos y la computadora puede quedar sin responder mientras tanto.',
            carga: 'Cargando ' + cantidad + ' ' + unidad + '. La pantalla puede dejar de responder: esperá, sin cerrar ni recargar la página.'
        };
    }

    if (typeof module !== 'undefined' && module.exports) {
        module.exports = { armarUrl: armarUrl, fechaCorta: fechaCorta, textos: textos };
    }
    if (!root || !root.document) return;

    // true = "Buscar igual". El boton destacado (y Escape) es "Cambiar el periodo": equivocarse no dispara la carga pesada.
    function preguntar(t) {
        if (!root.Swal || typeof root.Swal.fire !== 'function') {
            return Promise.resolve(root.confirm(t.periodo + ' ' + t.cifra + '.\n' + t.advertencia + '\n\n¿Buscar igual?'));
        }
        return root.Swal.fire({
            icon: 'warning',
            title: 'Esta búsqueda puede demorar',
            html: '<div style="text-align:left;font-size:.95rem">' + t.periodo
                + '<div style="text-align:center;margin:.6rem 0;padding:.5rem;border-radius:.6rem;background:#fff3d6;color:#000;font-size:1.05rem"><strong>' + t.cifra + '</strong></div>'
                + t.advertencia + '</div>',
            showCancelButton: true,
            confirmButtonText: 'Cambiar el período',
            cancelButtonText: 'Buscar igual',
            confirmButtonColor: '#2563eb',
            cancelButtonColor: '#6b7280',
            allowOutsideClick: false
        }).then(function (resultado) {
            return resultado.dismiss === root.Swal.DismissReason.cancel;
        });
    }

    function contar(url) {
        var control = root.AbortController ? new root.AbortController() : null;
        var reloj = control ? root.setTimeout(function () { control.abort(); }, ESPERA_MAXIMA_MS) : null;
        // silentLoading: el conteo no abre el cartel global de "Cargando".
        return root.fetch(url, { silentLoading: true, credentials: 'same-origin', signal: control ? control.signal : undefined })
            .then(function (r) { return r.json(); })
            .finally(function () { if (reloj) root.clearTimeout(reloj); });
    }

    function confirmar(urlContar, pantalla, filtros) {
        var seguir = { continuar: true, mensaje: '' };
        return contar(armarUrl(urlContar, pantalla, filtros))
            .then(function (respuesta) {
                if (!respuesta || !respuesta.ok || !respuesta.pesada) return seguir;
                var t = textos(respuesta, filtros);
                return preguntar(t).then(function (continuar) {
                    return { continuar: continuar, mensaje: continuar ? t.carga : '' };
                });
            })
            .catch(function (error) {
                // Sin conteo (red, sesion vencida, demora) se busca como siempre; queda registrado en la consola.
                if (root.console) root.console.warn('No se pudo contar la búsqueda; se continúa sin avisar.', error);
                return seguir;
            });
    }

    root.AvisoBusquedaPesada = { confirmar: confirmar };
})(typeof window !== 'undefined' ? window : null);
