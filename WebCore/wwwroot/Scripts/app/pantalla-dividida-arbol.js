/* Pantalla dividida: arbol de paneles (funciones PURAS, sin DOM) (2026-11-20, docs/DECISIONS.md "Pantalla dividida").
   Un nodo es un panel {tipo:'panel', id, url} (url null = panel vacio) o una division {tipo:'division', direccion:'fila'|'columna', proporcion, a, b}.
   'fila' pone a y b lado a lado (a a la izquierda); 'columna' los apila (a arriba). proporcion = parte del espacio que ocupa a (0..1).
   Todas las funciones devuelven arboles NUEVOS (no mutan), asi se prueban solas: WebCore.Tests/js/pantalla-dividida-arbol.test.js.
   deserializar trata el texto guardado en localStorage como dato hostil: valida estructura, profundidad, cantidad de paneles, ids y URLs.
   El tope MAX_PANELES debe coincidir con WebCore/Helpers/PantallaDivididaRutas.cs (MaxPaneles). Dependencias: ninguna. */
(function (global) {
    'use strict';

    var MAX_PANELES = 6;            // mismo valor que PantallaDivididaRutas.MaxPaneles (C#)
    var PROPORCION_MIN = 0.1;       // un panel nunca ocupa menos del 10 % de su division (el minimo en pixeles lo aplica el dibujo)
    var PROPORCION_MAX = 0.9;
    var PROPORCION_INICIAL = 0.5;
    var PROPORCION_PANEL_NUEVO_AL_COSTADO = 0.66;   // "abrir en un panel nuevo": lo existente conserva dos tercios
    var PROFUNDIDAD_MAX = MAX_PANELES;              // con MAX_PANELES hojas, una cadena de divisiones no pasa de MAX_PANELES - 1 niveles

    var DIRECCIONES = { fila: true, columna: true };

    // Nombre de las ventanas independientes de trabajo (mismo prefijo que _Layout.cshtml usa para activar el modo ventana), solo minusculas, numeros y guiones.
    var NOMBRE_VENTANA = /^carnisys-trabajo-[a-z0-9-]{1,120}$/;

    // ventana (opcional): nombre de la ventana independiente a la que se paso la pantalla de este panel (2026-11-20, "Independizar"). Mientras este, el panel
    // NO carga la url (la muestra la ventana) y solo ofrece ir a ella o traerla de vuelta. Sin ventana, el objeto no lleva el campo.
    function crearPanel(id, url, ventana) {
        var panel = { tipo: 'panel', id: id, url: url || null };
        if (ventana) panel.ventana = ventana;
        return panel;
    }

    function esPanel(nodo) { return !!nodo && nodo.tipo === 'panel'; }

    function acotarProporcion(valor) {
        var n = Number(valor);
        if (!isFinite(n)) return PROPORCION_INICIAL;
        return Math.min(PROPORCION_MAX, Math.max(PROPORCION_MIN, n));
    }

    function contarPaneles(nodo) {
        if (!nodo) return 0;
        return esPanel(nodo) ? 1 : contarPaneles(nodo.a) + contarPaneles(nodo.b);
    }

    function buscarPanel(nodo, id) {
        if (!nodo) return null;
        if (esPanel(nodo)) return nodo.id === id ? nodo : null;
        return buscarPanel(nodo.a, id) || buscarPanel(nodo.b, id);
    }

    function primerPanel(nodo) {
        if (!nodo) return null;
        return esPanel(nodo) ? nodo : primerPanel(nodo.a);
    }

    function listarPaneles(nodo) {
        if (!nodo) return [];
        return esPanel(nodo) ? [nodo] : listarPaneles(nodo.a).concat(listarPaneles(nodo.b));
    }

    // Siguiente id libre (mayor id + 1).
    function siguienteId(raiz) {
        return listarPaneles(raiz).reduce(function (max, p) { return Math.max(max, p.id); }, 0) + 1;
    }

    // URLs de los paneles que tienen algo cargado (para avisar al servidor que pantallas siguen abiertas).
    function urlsAbiertas(raiz) {
        return listarPaneles(raiz).map(function (p) { return p.url; }).filter(function (u) { return !!u; });
    }

    function reemplazarPanel(nodo, id, fabrica) {
        if (esPanel(nodo)) return nodo.id === id ? fabrica(nodo) : nodo;
        return {
            tipo: 'division', direccion: nodo.direccion, proporcion: nodo.proporcion,
            a: reemplazarPanel(nodo.a, id, fabrica), b: reemplazarPanel(nodo.b, id, fabrica)
        };
    }

    // Divide el panel idPanel en dos (el original + uno nuevo vacio a la derecha / abajo). null si no existe o se pasa del tope.
    function dividir(raiz, idPanel, direccion, idNuevo) {
        if (!DIRECCIONES[direccion] || !buscarPanel(raiz, idPanel) || contarPaneles(raiz) >= MAX_PANELES) return null;
        var nuevo = crearPanel(idNuevo, null);
        var arbol = reemplazarPanel(raiz, idPanel, function (panel) {
            return { tipo: 'division', direccion: direccion, proporcion: PROPORCION_INICIAL, a: panel, b: nuevo };
        });
        return { raiz: arbol, nuevoId: idNuevo };
    }

    // Agrega un panel nuevo (con url) al costado derecho de TODO lo existente. null si se pasa del tope.
    function agregarAlCostado(raiz, idNuevo, url) {
        if (!raiz || contarPaneles(raiz) >= MAX_PANELES) return null;
        var nuevo = crearPanel(idNuevo, url);
        return {
            raiz: { tipo: 'division', direccion: 'fila', proporcion: PROPORCION_PANEL_NUEVO_AL_COSTADO, a: raiz, b: nuevo },
            nuevoId: idNuevo
        };
    }

    // Cierra el panel: su hermano ocupa el lugar de la division. null si era el unico panel (el arbol queda vacio).
    function cerrar(raiz, idPanel) {
        if (!raiz || !buscarPanel(raiz, idPanel)) return raiz;
        if (esPanel(raiz)) return null;
        if (esPanel(raiz.a) && raiz.a.id === idPanel) return raiz.b;
        if (esPanel(raiz.b) && raiz.b.id === idPanel) return raiz.a;
        return {
            tipo: 'division', direccion: raiz.direccion, proporcion: raiz.proporcion,
            a: cerrar(raiz.a, idPanel), b: cerrar(raiz.b, idPanel)
        };
    }

    // Cargar otra pantalla en un panel lo vuelve un panel comun (se pierde la marca de ventana independiente; la ventana, si sigue abierta, queda aparte).
    function cargarUrl(raiz, idPanel, url) {
        return reemplazarPanel(raiz, idPanel, function (panel) { return crearPanel(panel.id, url); });
    }

    // Marca el panel como pasado a la ventana independiente "nombreVentana" (conserva su url para poder traerlo de vuelta). Sin url, no cambia nada.
    function independizar(raiz, idPanel, nombreVentana) {
        return reemplazarPanel(raiz, idPanel, function (panel) {
            return panel.url && nombreVentana ? crearPanel(panel.id, panel.url, nombreVentana) : panel;
        });
    }

    // Quita la marca de ventana: el panel vuelve a cargar su url.
    function traerDeVuelta(raiz, idPanel) {
        return reemplazarPanel(raiz, idPanel, function (panel) { return crearPanel(panel.id, panel.url); });
    }

    // Distribuciones de atajo. ids correlativos desde 1; el primer panel es el activo.
    function distribucionTres(urls) {
        urls = urls || [];
        return {
            tipo: 'division', direccion: 'fila', proporcion: 1 / 3,
            a: crearPanel(1, urls[0]),
            b: { tipo: 'division', direccion: 'fila', proporcion: 0.5, a: crearPanel(2, urls[1]), b: crearPanel(3, urls[2]) }
        };
    }

    function distribucionCuadro(urls) {
        urls = urls || [];
        return {
            tipo: 'division', direccion: 'columna', proporcion: 0.5,
            a: { tipo: 'division', direccion: 'fila', proporcion: 0.5, a: crearPanel(1, urls[0]), b: crearPanel(2, urls[1]) },
            b: { tipo: 'division', direccion: 'fila', proporcion: 0.5, a: crearPanel(3, urls[2]), b: crearPanel(4, urls[3]) }
        };
    }

    // ---------- geometria ----------
    // Rectangulos (fracciones 0..1 del area total) de cada panel y de cada divisor. Los paneles se posicionan en forma ABSOLUTA con estos
    // valores: asi dividir, mover un divisor o maximizar solo cambian estilos y NUNCA se saca un <iframe> del DOM (moverlo lo recargaria).
    // Cada divisor lleva su ruta ('a'/'b' desde la raiz) para cambiarle la proporcion, y el rectangulo de su division (padre).
    function calcularRectangulos(raiz) {
        var paneles = [];
        var divisores = [];
        (function recorrer(nodo, r, ruta) {
            if (!nodo) return;
            if (esPanel(nodo)) {
                paneles.push({ id: nodo.id, x: r.x, y: r.y, w: r.w, h: r.h });
                return;
            }
            var p = nodo.proporcion;
            if (nodo.direccion === 'fila') {
                divisores.push({ ruta: ruta, direccion: 'fila', x: r.x + r.w * p, y: r.y, w: 0, h: r.h, padre: r });
                recorrer(nodo.a, { x: r.x, y: r.y, w: r.w * p, h: r.h }, ruta + 'a');
                recorrer(nodo.b, { x: r.x + r.w * p, y: r.y, w: r.w * (1 - p), h: r.h }, ruta + 'b');
            } else {
                divisores.push({ ruta: ruta, direccion: 'columna', x: r.x, y: r.y + r.h * p, w: r.w, h: 0, padre: r });
                recorrer(nodo.a, { x: r.x, y: r.y, w: r.w, h: r.h * p }, ruta + 'a');
                recorrer(nodo.b, { x: r.x, y: r.y + r.h * p, w: r.w, h: r.h * (1 - p) }, ruta + 'b');
            }
        })(raiz, { x: 0, y: 0, w: 1, h: 1 }, '');
        return { paneles: paneles, divisores: divisores };
    }

    // Cambia la proporcion de la division que esta en "ruta" (cadena de 'a'/'b' desde la raiz; '' = la raiz). Ruta invalida: no cambia nada.
    function cambiarProporcion(nodo, ruta, valor) {
        if (!nodo || esPanel(nodo)) return nodo;
        if (ruta === '') {
            return { tipo: 'division', direccion: nodo.direccion, proporcion: acotarProporcion(valor), a: nodo.a, b: nodo.b };
        }
        var lado = ruta.charAt(0);
        var resto = ruta.slice(1);
        if (lado === 'a') return { tipo: 'division', direccion: nodo.direccion, proporcion: nodo.proporcion, a: cambiarProporcion(nodo.a, resto, valor), b: nodo.b };
        if (lado === 'b') return { tipo: 'division', direccion: nodo.direccion, proporcion: nodo.proporcion, a: nodo.a, b: cambiarProporcion(nodo.b, resto, valor) };
        return nodo;
    }

    // Devuelve una copia del arbol con la url de cada panel que tenga una calculada por urlDe(panel) (2026-11-21, pasar TODA la pantalla dividida a una
    // ventana independiente: cada panel viaja con su url + la marca del traspaso de lo cargado). Si urlDe no devuelve texto, el panel conserva la suya.
    // No modifica el arbol original. Los paneles sin url (vacios) quedan igual.
    function mapearUrls(nodo, urlDe) {
        if (!nodo) return nodo;
        if (esPanel(nodo)) {
            if (!nodo.url) return nodo;
            var nueva = urlDe(nodo);
            return crearPanel(nodo.id, typeof nueva === 'string' && nueva !== '' ? nueva : nodo.url, nodo.ventana);
        }
        return { tipo: 'division', direccion: nodo.direccion, proporcion: nodo.proporcion, a: mapearUrls(nodo.a, urlDe), b: mapearUrls(nodo.b, urlDe) };
    }

    // ---------- guardado ----------
    function serializar(raiz, idActivo) {
        return JSON.stringify({ version: 1, activo: idActivo || null, raiz: raiz });
    }

    // Valida un nodo recursivamente. ids = set de ids ya vistos (repetidos = invalido). Devuelve el nodo limpio o lanza Error con el motivo.
    function validarNodo(nodo, urlValida, ids, profundidad) {
        if (!nodo || typeof nodo !== 'object') throw new Error('nodo vacio o no es un objeto');
        if (profundidad > PROFUNDIDAD_MAX) throw new Error('arbol demasiado profundo');
        if (nodo.tipo === 'panel') {
            if (!Number.isInteger(nodo.id) || nodo.id < 1) throw new Error('id de panel invalido');
            if (ids[nodo.id]) throw new Error('id de panel repetido');
            ids[nodo.id] = true;
            var url = nodo.url;
            if (url !== null && url !== undefined) {
                if (typeof url !== 'string' || !urlValida(url)) throw new Error('url de panel no permitida');
            }
            // Ventana independiente: solo nombres de la familia de ventanas de trabajo, y solo con una url que traer de vuelta.
            var ventana = nodo.ventana;
            if (ventana !== null && ventana !== undefined) {
                if (typeof ventana !== 'string' || !NOMBRE_VENTANA.test(ventana)) throw new Error('nombre de ventana independiente invalido');
                if (!url) throw new Error('panel en ventana independiente sin url');
            }
            return crearPanel(nodo.id, url, ventana);
        }
        if (nodo.tipo === 'division') {
            if (!DIRECCIONES[nodo.direccion]) throw new Error('direccion invalida');
            var a = validarNodo(nodo.a, urlValida, ids, profundidad + 1);
            var b = validarNodo(nodo.b, urlValida, ids, profundidad + 1);
            return { tipo: 'division', direccion: nodo.direccion, proporcion: acotarProporcion(nodo.proporcion), a: a, b: b };
        }
        throw new Error('tipo de nodo desconocido');
    }

    // Lee lo guardado. Devuelve {ok:true, raiz, activo} o {ok:false, motivo} (el llamador arranca vacio y deja el motivo en consola).
    // urlValida(url) decide si una URL cargada en un panel es aceptable (relativa al sitio, no el POS, etc.).
    function deserializar(texto, urlValida) {
        if (typeof urlValida !== 'function') return { ok: false, motivo: 'falta el validador de urls' };
        if (typeof texto !== 'string' || texto === '') return { ok: false, motivo: 'no hay nada guardado' };
        try {
            var datos = JSON.parse(texto);
            if (!datos || datos.version !== 1) throw new Error('version desconocida');
            var raiz = validarNodo(datos.raiz, urlValida, {}, 0);
            if (contarPaneles(raiz) > MAX_PANELES) throw new Error('hay mas paneles que el maximo');
            var activo = Number.isInteger(datos.activo) && buscarPanel(raiz, datos.activo) ? datos.activo : primerPanel(raiz).id;
            return { ok: true, raiz: raiz, activo: activo };
        } catch (e) {
            return { ok: false, motivo: e && e.message ? e.message : String(e) };
        }
    }

    global.PantallaDivididaArbol = {
        MAX_PANELES: MAX_PANELES,
        PROPORCION_MIN: PROPORCION_MIN,
        PROPORCION_MAX: PROPORCION_MAX,
        crearPanel: crearPanel,
        acotarProporcion: acotarProporcion,
        contarPaneles: contarPaneles,
        buscarPanel: buscarPanel,
        primerPanel: primerPanel,
        listarPaneles: listarPaneles,
        siguienteId: siguienteId,
        urlsAbiertas: urlsAbiertas,
        dividir: dividir,
        agregarAlCostado: agregarAlCostado,
        cerrar: cerrar,
        cargarUrl: cargarUrl,
        independizar: independizar,
        traerDeVuelta: traerDeVuelta,
        calcularRectangulos: calcularRectangulos,
        cambiarProporcion: cambiarProporcion,
        distribucionTres: distribucionTres,
        distribucionCuadro: distribucionCuadro,
        mapearUrls: mapearUrls,
        serializar: serializar,
        deserializar: deserializar
    };
})(typeof window !== 'undefined' ? window : this);
