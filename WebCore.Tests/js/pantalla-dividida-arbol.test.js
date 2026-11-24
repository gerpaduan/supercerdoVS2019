// Tests del arbol de paneles de la Pantalla dividida (WebCore/wwwroot/Scripts/app/pantalla-dividida-arbol.js).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js
// El script se carga en un sandbox (vm) sin document: solo expone funciones puras sobre arboles.
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const SCRIPT = fs.readFileSync(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'pantalla-dividida-arbol.js'), 'utf8');
const sandbox = {};
sandbox.window = sandbox;
vm.createContext(sandbox);
vm.runInContext(SCRIPT, sandbox);
const A = sandbox.PantallaDivididaArbol;
// Los objetos vienen de otro "realm" (vm): se pasan por JSON para que deepEqual los compare bien.
const plano = (valor) => JSON.parse(JSON.stringify(valor));

const aceptaTodo = () => true;
const soloRelativas = (url) => url.startsWith('/') && !url.startsWith('//') && !/^\/ventas\/pos/i.test(url);

test('el tope de paneles coincide con el del servidor (PantallaDivididaRutas.MaxPaneles = 6)', () => {
    assert.equal(A.MAX_PANELES, 6);
});

test('dividir a lo ancho deja el panel original a la izquierda y uno vacio a la derecha', () => {
    const r = A.dividir(A.crearPanel(1, '/Stock'), 1, 'fila', 2);
    assert.deepEqual(plano(r.raiz), {
        tipo: 'division', direccion: 'fila', proporcion: 0.5,
        a: { tipo: 'panel', id: 1, url: '/Stock' }, b: { tipo: 'panel', id: 2, url: null }
    });
    assert.equal(r.nuevoId, 2);
});

test('dividir a lo alto apila el panel nuevo debajo', () => {
    const r = A.dividir(A.crearPanel(1, '/Stock'), 1, 'columna', 2);
    assert.equal(r.raiz.direccion, 'columna');
    assert.equal(r.raiz.b.id, 2);
});

test('dividir no muta el arbol original', () => {
    const original = A.crearPanel(1, '/Stock');
    A.dividir(original, 1, 'fila', 2);
    assert.deepEqual(plano(original), { tipo: 'panel', id: 1, url: '/Stock' });
});

test('dividir un panel del medio solo cambia ese panel', () => {
    const arbol = A.distribucionTres(['/Stock', '/Movimientos', '/Reportes']);
    const r = A.dividir(arbol, 2, 'columna', 4);
    assert.equal(A.contarPaneles(r.raiz), 4);
    assert.equal(A.buscarPanel(r.raiz, 1).url, '/Stock');
    assert.equal(A.buscarPanel(r.raiz, 3).url, '/Reportes');
    assert.equal(A.buscarPanel(r.raiz, 4).url, null);
});

test('dividir devuelve null con direccion invalida, panel inexistente o tope alcanzado', () => {
    assert.equal(A.dividir(A.crearPanel(1, null), 1, 'diagonal', 2), null);
    assert.equal(A.dividir(A.crearPanel(1, null), 99, 'fila', 2), null);
    let arbol = A.crearPanel(1, null);
    for (let i = 2; i <= A.MAX_PANELES; i++) arbol = A.dividir(arbol, 1, 'fila', i).raiz;
    assert.equal(A.contarPaneles(arbol), A.MAX_PANELES);
    assert.equal(A.dividir(arbol, 1, 'fila', 99), null);
});

test('agregarAlCostado pone lo nuevo a la derecha de todo y respeta el tope', () => {
    const arbol = A.distribucionTres([null, null, null]);
    const r = A.agregarAlCostado(arbol, 4, '/Compras');
    assert.equal(r.raiz.direccion, 'fila');
    assert.equal(r.raiz.b.url, '/Compras');
    assert.equal(A.contarPaneles(r.raiz), 4);

    let lleno = A.crearPanel(1, null);
    for (let i = 2; i <= A.MAX_PANELES; i++) lleno = A.dividir(lleno, 1, 'fila', i).raiz;
    assert.equal(A.agregarAlCostado(lleno, 99, '/x'), null);
});

test('cerrar un panel hace que su hermano ocupe el lugar', () => {
    const arbol = A.dividir(A.crearPanel(1, '/Stock'), 1, 'fila', 2).raiz;
    const resultado = A.cerrar(arbol, 2);
    assert.deepEqual(plano(resultado), { tipo: 'panel', id: 1, url: '/Stock' });
});

test('cerrar un panel anidado conserva el resto de la distribucion', () => {
    const arbol = A.distribucionTres(['/Stock', '/Movimientos', '/Reportes']);
    const resultado = A.cerrar(arbol, 2);
    assert.equal(A.contarPaneles(resultado), 2);
    assert.deepEqual(plano(A.listarPaneles(resultado).map((p) => p.id)), [1, 3]);
});

test('cerrar el unico panel deja el arbol vacio (null); cerrar un id inexistente no cambia nada', () => {
    assert.equal(A.cerrar(A.crearPanel(1, null), 1), null);
    const arbol = A.crearPanel(1, '/Stock');
    assert.equal(A.cerrar(arbol, 77), arbol);
});

test('cargarUrl cambia solo la url del panel pedido', () => {
    const arbol = A.distribucionTres([null, null, null]);
    const r = A.cargarUrl(arbol, 2, '/Stock/Index');
    assert.equal(A.buscarPanel(r, 2).url, '/Stock/Index');
    assert.equal(A.buscarPanel(r, 1).url, null);
});

test('siguienteId y urlsAbiertas', () => {
    const arbol = A.distribucionTres(['/Stock', null, '/Reportes']);
    assert.equal(A.siguienteId(arbol), 4);
    assert.deepEqual(plano(A.urlsAbiertas(arbol)), ['/Stock', '/Reportes']);
    assert.equal(A.siguienteId(null), 1);
});

test('las distribuciones de atajo tienen 3 y 4 paneles con ids correlativos', () => {
    assert.deepEqual(plano(A.listarPaneles(A.distribucionTres()).map((p) => p.id)), [1, 2, 3]);
    assert.deepEqual(plano(A.listarPaneles(A.distribucionCuadro()).map((p) => p.id)), [1, 2, 3, 4]);
});

test('acotarProporcion limita a 10 %..90 % y reemplaza lo no numerico', () => {
    assert.equal(A.acotarProporcion(0.01), 0.1);
    assert.equal(A.acotarProporcion(5), 0.9);
    assert.equal(A.acotarProporcion(0.4), 0.4);
    assert.equal(A.acotarProporcion('abc'), 0.5);
    assert.equal(A.acotarProporcion(NaN), 0.5);
});

// ---------- guardado ----------

test('serializar y deserializar son inversos', () => {
    const arbol = A.distribucionTres(['/Stock', '/Movimientos', null]);
    const leido = A.deserializar(A.serializar(arbol, 2), soloRelativas);
    assert.equal(leido.ok, true);
    assert.deepEqual(plano(leido.raiz), plano(arbol));
    assert.equal(leido.activo, 2);
});

test('deserializar: sin nada guardado, texto roto o version desconocida -> no ok con motivo', () => {
    for (const texto of [null, '', '{no es json', '{"version":9,"raiz":{}}', '[]', '"hola"']) {
        const r = A.deserializar(texto, aceptaTodo);
        assert.equal(r.ok, false, String(texto));
        assert.ok(r.motivo.length > 0);
    }
});

test('deserializar rechaza una url no permitida (otro sitio o el POS)', () => {
    for (const url of ['http://otro.com/x', '//otro.com/x', 'javascript:alert(1)', '/Ventas/POS']) {
        const texto = A.serializar(A.crearPanel(1, url), 1);
        const r = A.deserializar(texto, soloRelativas);
        assert.equal(r.ok, false, url);
    }
});

test('deserializar rechaza ids repetidos, tipos desconocidos y direcciones invalidas', () => {
    const repetido = { version: 1, activo: 1, raiz: { tipo: 'division', direccion: 'fila', proporcion: 0.5, a: { tipo: 'panel', id: 1, url: null }, b: { tipo: 'panel', id: 1, url: null } } };
    assert.equal(A.deserializar(JSON.stringify(repetido), aceptaTodo).ok, false);
    assert.equal(A.deserializar(JSON.stringify({ version: 1, raiz: { tipo: 'otra-cosa' } }), aceptaTodo).ok, false);
    const diagonal = { version: 1, raiz: { tipo: 'division', direccion: 'diagonal', proporcion: 0.5, a: { tipo: 'panel', id: 1 }, b: { tipo: 'panel', id: 2 } } };
    assert.equal(A.deserializar(JSON.stringify(diagonal), aceptaTodo).ok, false);
});

test('deserializar rechaza mas paneles que el maximo y un id no entero', () => {
    let arbol = { tipo: 'panel', id: 1, url: null };
    for (let i = 2; i <= A.MAX_PANELES + 1; i++) {
        arbol = { tipo: 'division', direccion: 'fila', proporcion: 0.5, a: arbol, b: { tipo: 'panel', id: i, url: null } };
    }
    assert.equal(A.deserializar(JSON.stringify({ version: 1, raiz: arbol }), aceptaTodo).ok, false);
    assert.equal(A.deserializar(JSON.stringify({ version: 1, raiz: { tipo: 'panel', id: 1.5, url: null } }), aceptaTodo).ok, false);
    assert.equal(A.deserializar(JSON.stringify({ version: 1, raiz: { tipo: 'panel', id: '1', url: null } }), aceptaTodo).ok, false);
});

test('deserializar acota proporciones fuera de rango y usa el primer panel si el activo no existe', () => {
    const texto = JSON.stringify({
        version: 1, activo: 99,
        raiz: { tipo: 'division', direccion: 'fila', proporcion: 7, a: { tipo: 'panel', id: 4, url: null }, b: { tipo: 'panel', id: 5, url: null } }
    });
    const r = A.deserializar(texto, aceptaTodo);
    assert.equal(r.ok, true);
    assert.equal(r.raiz.proporcion, 0.9);
    assert.equal(r.activo, 4);
});

test('deserializar sin validador de urls no confia en nada', () => {
    assert.equal(A.deserializar(A.serializar(A.crearPanel(1, null), 1)).ok, false);
});

// ---------- geometria ----------

const cerca = (real, esperado) => assert.ok(Math.abs(real - esperado) < 1e-9, real + ' != ' + esperado);

test('calcularRectangulos: un solo panel ocupa todo el area y no hay divisores', () => {
    const g = A.calcularRectangulos(A.crearPanel(1, null));
    assert.deepEqual(plano(g.paneles), [{ id: 1, x: 0, y: 0, w: 1, h: 1 }]);
    assert.equal(g.divisores.length, 0);
});

test('calcularRectangulos: tres columnas reparten el ancho 1/3 cada una', () => {
    const g = A.calcularRectangulos(A.distribucionTres());
    assert.equal(g.paneles.length, 3);
    g.paneles.forEach((p) => { cerca(p.w, 1 / 3); cerca(p.h, 1); cerca(p.y, 0); });
    cerca(g.paneles[0].x, 0);
    cerca(g.paneles[1].x, 1 / 3);
    cerca(g.paneles[2].x, 2 / 3);
    assert.equal(g.divisores.length, 2);
    assert.deepEqual(plano(g.divisores.map((d) => d.ruta)), ['', 'b']);
});

test('calcularRectangulos: el cuadro de 4 usa columna arriba/abajo y filas dentro', () => {
    const g = A.calcularRectangulos(A.distribucionCuadro());
    assert.equal(g.paneles.length, 4);
    g.paneles.forEach((p) => { cerca(p.w, 0.5); cerca(p.h, 0.5); });
    assert.equal(g.divisores.filter((d) => d.direccion === 'columna').length, 1);
    assert.equal(g.divisores.filter((d) => d.direccion === 'fila').length, 2);
});

test('calcularRectangulos: el divisor lleva el rectangulo de su division para poder arrastrarlo', () => {
    const g = A.calcularRectangulos(A.distribucionTres());
    const interno = g.divisores.find((d) => d.ruta === 'b');
    cerca(interno.padre.x, 1 / 3);
    cerca(interno.padre.w, 2 / 3);
    cerca(interno.x, 2 / 3);
});

test('cambiarProporcion cambia solo la division de la ruta y acota el valor', () => {
    const arbol = A.distribucionTres();
    const r = A.cambiarProporcion(arbol, 'b', 0.8);
    assert.equal(r.proporcion, arbol.proporcion);
    assert.equal(r.b.proporcion, 0.8);
    assert.equal(A.cambiarProporcion(arbol, '', 5).proporcion, 0.9);
    assert.equal(arbol.b.proporcion, 0.5);   // no muta
});

test('cambiarProporcion con ruta invalida o sobre un panel no cambia nada', () => {
    const arbol = A.distribucionTres();
    assert.deepEqual(plano(A.cambiarProporcion(arbol, 'x', 0.8)), plano(arbol));
    const panel = A.crearPanel(1, null);
    assert.equal(A.cambiarProporcion(panel, '', 0.8), panel);
});

// ---------- ventana independiente ("Independizar") ----------

const VENTANA = 'carnisys-trabajo-menu-stock-index';

test('independizar marca el panel con su ventana y conserva la url; no muta el arbol', () => {
    const arbol = A.distribucionTres(['/Stock/Index', '/Movimientos', null]);
    const r = A.independizar(arbol, 1, VENTANA);
    assert.deepEqual(plano(A.buscarPanel(r, 1)), { tipo: 'panel', id: 1, url: '/Stock/Index', ventana: VENTANA });
    assert.equal(A.buscarPanel(r, 2).ventana, undefined);
    assert.equal(A.buscarPanel(arbol, 1).ventana, undefined);
});

test('independizar un panel vacio o sin nombre no cambia nada', () => {
    const arbol = A.distribucionTres([null, '/Movimientos', null]);
    assert.equal(A.buscarPanel(A.independizar(arbol, 1, VENTANA), 1).ventana, undefined);
    assert.equal(A.buscarPanel(A.independizar(arbol, 2, ''), 2).ventana, undefined);
});

test('traerDeVuelta quita la marca y deja la url', () => {
    const r = A.traerDeVuelta(A.independizar(A.crearPanel(1, '/Stock/Index'), 1, VENTANA), 1);
    assert.deepEqual(plano(r), { tipo: 'panel', id: 1, url: '/Stock/Index' });
});

test('cargarUrl en un panel independizado lo vuelve un panel comun', () => {
    const independizado = A.independizar(A.crearPanel(1, '/Stock/Index'), 1, VENTANA);
    assert.deepEqual(plano(A.cargarUrl(independizado, 1, '/Reportes')), { tipo: 'panel', id: 1, url: '/Reportes' });
});

test('la marca de ventana se guarda y se lee igual', () => {
    const arbol = A.independizar(A.distribucionTres(['/Stock/Index', '/Movimientos', null]), 1, VENTANA);
    const leido = A.deserializar(A.serializar(arbol, 1), soloRelativas);
    assert.equal(leido.ok, true);
    assert.equal(A.buscarPanel(leido.raiz, 1).ventana, VENTANA);
});

test('deserializar rechaza un nombre de ventana ajeno o una ventana sin url', () => {
    for (const ventana of ['otra-cosa', 'carnisys-trabajo-', 'carnisys-trabajo-MAYUSCULA', 'carnisys-trabajo-a b', 7, '']) {
        const texto = JSON.stringify({ version: 1, raiz: { tipo: 'panel', id: 1, url: '/Stock', ventana } });
        assert.equal(A.deserializar(texto, aceptaTodo).ok, false, String(ventana));
    }
    const sinUrl = JSON.stringify({ version: 1, raiz: { tipo: 'panel', id: 1, url: null, ventana: VENTANA } });
    assert.equal(A.deserializar(sinUrl, aceptaTodo).ok, false);
});

// ---- mapearUrls (2026-11-21, pasar toda la pantalla dividida a una ventana independiente) ----

test('mapearUrls reemplaza la url de cada panel con pantalla y deja los vacios y la forma del arbol', () => {
    let r = A.dividir(A.crearPanel(1, '/Movimientos/Nuevo'), 1, 'fila', 2);   // 1 con pantalla, 2 vacio
    r = A.dividir(r.raiz, 2, 'columna', 3);
    const raiz = A.cargarUrl(r.raiz, 3, '/Stock/Editar');
    const nueva = A.mapearUrls(raiz, (p) => p.url + '?traspasoDoc=k' + p.id);
    const paneles = A.listarPaneles(nueva);
    assert.deepEqual(plano(paneles.map((p) => [p.id, p.url])), [[1, '/Movimientos/Nuevo?traspasoDoc=k1'], [2, null], [3, '/Stock/Editar?traspasoDoc=k3']]);
    assert.equal(nueva.direccion, raiz.direccion);
    assert.equal(nueva.proporcion, raiz.proporcion);
});

test('mapearUrls no modifica el arbol original', () => {
    const raiz = A.dividir(A.crearPanel(1, '/Stock'), 1, 'fila', 2).raiz;
    const antes = JSON.stringify(raiz);
    A.mapearUrls(raiz, () => '/Otra');
    assert.equal(JSON.stringify(raiz), antes);
});

test('mapearUrls: si urlDe no devuelve texto el panel conserva su url, y conserva la marca de ventana independiente', () => {
    const raiz = A.independizar(A.dividir(A.crearPanel(1, '/Stock'), 1, 'fila', 2).raiz, 1, 'carnisys-trabajo-menu-stock');
    const nueva = A.mapearUrls(raiz, () => undefined);
    const p1 = A.buscarPanel(nueva, 1);
    assert.equal(p1.url, '/Stock');
    assert.equal(p1.ventana, 'carnisys-trabajo-menu-stock');
});

test('un arbol con urls mapeadas se serializa y se lee de vuelta (el destino lo valida igual que lo guardado)', () => {
    const raiz = A.dividir(A.crearPanel(1, '/Movimientos/Nuevo'), 1, 'fila', 2).raiz;
    const texto = A.serializar(A.mapearUrls(raiz, (p) => p.url + '?traspasoDoc=carnisys-traspaso-doc-1-abc'), 2);
    const leido = A.deserializar(texto, soloRelativas);
    assert.equal(leido.ok, true);
    assert.equal(A.buscarPanel(leido.raiz, 1).url, '/Movimientos/Nuevo?traspasoDoc=carnisys-traspaso-doc-1-abc');
    assert.equal(leido.activo, 2);
});

test('contarConPantalla cuenta solo los paneles con una pantalla cargada en el panel (no vacios ni en ventana independiente)', () => {
    assert.equal(A.contarConPantalla(A.distribucionCuadro(['/Stock', '/Movimientos', '/Reportes', '/Compras'])), 4);
    // Un panel vacio no pierde nada al reacomodar: no cuenta (4 paneles, 3 con pantalla -> entran en «3 columnas»).
    assert.equal(A.contarConPantalla(A.distribucionCuadro(['/Stock', '/Movimientos', '/Reportes', null])), 3);
    assert.equal(A.contarConPantalla(A.distribucionTres([null, null, null])), 0);
    assert.equal(A.contarConPantalla(null), 0);
    // Un panel pasado a una ventana independiente tiene url pero su pantalla vive en su ventana: tampoco cuenta.
    const conVentana = A.independizar(A.distribucionTres(['/Stock', '/Movimientos', '/Reportes']), 2, 'carnisys-trabajo-menu-movimientos');
    assert.equal(A.contarConPantalla(conVentana), 2);
});

test('agregarJunto pone el panel nuevo vacio a la derecha o a la izquierda y solo achica a ese panel', () => {
    const base = A.distribucionTres(['/Stock', '/Movimientos', '/Reportes']);
    const rectDe = (arbol, id) => A.calcularRectangulos(arbol).paneles.find((p) => p.id === id);
    const antes3 = rectDe(base, 3);

    const der = A.agregarJunto(base, 2, 'derecha', 4);
    assert.equal(der.nuevoId, 4);
    assert.deepEqual(plano(A.listarPaneles(der.raiz).map((p) => p.id)), [1, 2, 4, 3]);   // 4 queda justo a la derecha del 2
    assert.equal(A.buscarPanel(der.raiz, 4).url, null);
    // el panel 3 y el 1 no cambian; el 2 y el 4 se reparten el ancho que tenia el 2
    assert.deepEqual(plano(rectDe(der.raiz, 3)), plano(antes3));
    assert.ok(Math.abs(rectDe(der.raiz, 2).w - rectDe(der.raiz, 4).w) < 1e-9);
    assert.ok(Math.abs(rectDe(der.raiz, 2).w * 2 - rectDe(base, 2).w) < 1e-9);

    const izq = A.agregarJunto(base, 2, 'izquierda', 4);
    assert.deepEqual(plano(A.listarPaneles(izq.raiz).map((p) => p.id)), [1, 4, 2, 3]);
});

test('agregarJunto devuelve null con lado invalido, panel inexistente o tope alcanzado, y no muta el arbol', () => {
    const base = A.distribucionTres(['/Stock', null, null]);
    const copia = plano(base);
    assert.equal(A.agregarJunto(base, 1, 'arriba', 9), null);
    assert.equal(A.agregarJunto(base, 99, 'derecha', 9), null);
    assert.equal(A.agregarJunto(null, 1, 'derecha', 9), null);
    let lleno = base;
    for (let id = 4; id <= 6; id++) lleno = A.agregarJunto(lleno, 1, 'derecha', id).raiz;
    assert.equal(A.contarPaneles(lleno), 6);
    assert.equal(A.agregarJunto(lleno, 1, 'derecha', 7), null);
    assert.deepEqual(plano(base), copia);
});

test('agregarJunto dentro de un cuadro parte solo la celda del panel', () => {
    const cuadro = A.distribucionCuadro(['/a', '/b', '/c', '/d']);
    const r = A.agregarJunto(cuadro, 3, 'derecha', 5);
    assert.equal(A.contarPaneles(r.raiz), 5);
    const g = A.calcularRectangulos(r.raiz).paneles;
    const p = (id) => g.find((x) => x.id === id);
    assert.ok(Math.abs(p(3).w - 0.25) < 1e-9 && Math.abs(p(5).w - 0.25) < 1e-9);   // la celda de abajo a la izquierda (0.5) se parte en dos
    assert.ok(Math.abs(p(1).w - 0.5) < 1e-9 && Math.abs(p(4).w - 0.5) < 1e-9);       // las demas celdas no cambian
});

test('agregarAlExtremoIzquierdo pone el panel nuevo primero con un tercio del ancho y respeta el tope', () => {
    const base = A.distribucionTres(['/a', '/b', '/c']);
    const r = A.agregarAlExtremoIzquierdo(base, 4, null);
    assert.deepEqual(plano(A.listarPaneles(r.raiz).map((p) => p.id)), [4, 1, 2, 3]);
    const g = A.calcularRectangulos(r.raiz).paneles;
    assert.ok(Math.abs(g.find((p) => p.id === 4).w - 0.34) < 1e-9 && g.find((p) => p.id === 4).x === 0);
    assert.equal(A.agregarAlExtremoIzquierdo(null, 4, null), null);
    let lleno = base;
    for (let id = 4; id <= 6; id++) lleno = A.agregarAlExtremoIzquierdo(lleno, id, null).raiz;
    assert.equal(A.agregarAlExtremoIzquierdo(lleno, 7, null), null);
});
