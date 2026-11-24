/* Pantalla dividida: dibujo y manejo de los paneles (2026-11-20, docs/DECISIONS.md "Pantalla dividida").
   Cada panel es un <iframe name="carnisys-trabajo-panel-<id>"> con la pantalla real (sin menu ni barra: el <head> de _Layout.cshtml activa html.modo-ventana
   por el prefijo carnisys-trabajo-). La estructura la decide pantalla-dividida-arbol.js (funciones puras); aca esta todo lo que toca el DOM.
   - Los paneles se posicionan en forma ABSOLUTA con los rectangulos del arbol: dividir, mover un divisor o maximizar solo cambian estilos. Mover un
     <iframe> en el DOM lo recargaria (y se perderia lo cargado), por eso nunca se reubican los nodos.
   - Panel activo = el ultimo tocado; un clic en el menu lateral carga ese modulo en el panel activo (Ctrl+clic navega como siempre).
   - Permisos: al cargar o cerrar un panel se avisa al servidor que pantallas siguen abiertas (LiberarPermisos) para que el operador autorizado de una
     pantalla cerrada se pierda. El punto de venta no se abre en un panel (se abre aparte).
   - Se recuerda la ultima distribucion de este usuario en este equipo (localStorage; se valida como dato hostil al leerla).
   Depende de: pantalla-dividida-arbol.js, _Layout.cshtml (menu lateral #accordionSidebar, confirmarSalidaSinGuardar, Swal). Uso: docs/11-manual-de-uso/pantallas/PantallaDividida.Index.md */
(function () {
    'use strict';

    var Arbol = window.PantallaDivididaArbol;
    var pagina = document.getElementById('pdPagina');
    var area = document.getElementById('pdArea');
    if (!Arbol || !pagina || !area) return;

    // ---------- constantes ----------
    var ESCRITORIO_ANCHO_MIN_PX = 992;      // por debajo la pantalla muestra un aviso en vez de los paneles (mismo piso que el menu lateral)
    var MIN_PANEL_PX = 220;                 // ningun panel queda mas angosto / bajo que esto al dividir o arrastrar un divisor
    var MARGEN_INFERIOR_PX = 24;            // espacio que queda debajo del area de paneles
    var ALTO_MINIMO_AREA_PX = 420;
    var PASO_TECLADO = 0.02;                // un divisor enfocado se mueve de a 2 % con las flechas
    // Ventana independiente ("Independizar" un panel):
    var VENTANA_MIN_ANCHO_PX = 480;         // minimo de la ventana (igual que marco-ventana.js)
    var VENTANA_MIN_ALTO_PX = 360;
    var VENTANA_ANCHO_PX = 1100;            // tamano por omision si no se puede medir el panel (maximizado u oculto)
    var VENTANA_ALTO_PX = 800;
    var VENTANA_NOMBRE_MAX = 100;           // largo maximo del nombre (el arbol acepta hasta 120 despues del prefijo)
    var REVISION_VENTANAS_MS = 1000;        // cada cuanto se mira si una ventana independiente se cerro
    var ESPERA_VACIAR_MARCO_MS = 1500;      // tope de espera a que el panel termine de vaciarse antes de cargar la ventana
    var ESPERA_CIERRE_VENTANA_MS = 100;     // al traer de vuelta: se espera a que la ventana termine de cerrarse (suelta sus candados de "documento abierto")
    var INTENTOS_CIERRE_VENTANA = 15;
    var GUARDADO_DEMORA_MS = 300;
    var LIBERAR_DEMORA_MS = 300;
    var PREFIJO_FRAME = 'carnisys-trabajo-panel-';
    var PREFIJO_VENTANA_PANTALLA = 'carnisys-trabajo-menu-';   // mismo que menu-ventana-independiente.js: una ventana por destino
    var SUFIJO_TITULO = ' - CarniSys';
    // Pasar TODA la pantalla dividida a una ventana independiente (2026-11-21): una sola ventana por usuario (como una pantalla = una ventana), con
    // nombre de la familia carnisys-trabajo-* (activa html.modo-ventana: sin menu ni barra) y su PROPIA distribucion guardada, para que la de la
    // pestaña y la de la ventana no se pisen.
    var NOMBRE_VENTANA_DIVIDIDA = 'carnisys-trabajo-pantalla-dividida';
    var EN_VENTANA_DIVIDIDA = window.name === NOMBRE_VENTANA_DIVIDIDA;
    var PARAM_DESDE_LAYOUT = 'desdeLayout';                  // ?desdeLayout=<clave>: la distribucion que viene de la pestaña (una sola vez)
    var PREFIJO_LAYOUT_TRASPASO = 'carnisys-pd-layout-';     // clave temporal en localStorage con esa distribucion
    var VIGENCIA_LAYOUT_MS = 10 * 60 * 1000;                 // una clave sin usar se descarta a los 10 minutos
    var CLAVE_GUARDADO = 'carnisys-pantalla-dividida-v1-' + (pagina.getAttribute('data-usuario') || '0') + (EN_VENTANA_DIVIDIDA ? '-ventana' : '');
    var PARAM_ABRIR = 'abrir';
    // Lugares de los atajos de la barra: deben coincidir con Arbol.distribucionTres / Arbol.distribucionCuadro (3 y 4 paneles).
    var CAPACIDAD_TRES = 3;
    var CAPACIDAD_CUADRO = 4;
    // Tooltips normales de esos botones (los mismos de Index.cshtml); cuando no entran las pantallas abiertas el tooltip pasa a ser el motivo.
    var TITULO_TRES = 'Acomoda tres columnas conservando lo que ya tenés abierto';
    var TITULO_CUADRO = 'Acomoda cuatro paneles en cuadro conservando lo que ya tenés abierto';

    var BASE = (pagina.getAttribute('data-base') || '/').replace(/\/+$/, '');   // PathBase sin barra final ("" o "/carnisys")
    var URL_LIBERAR = pagina.getAttribute('data-url-liberar') || '';
    var URL_INICIO = pagina.getAttribute('data-url-inicio') || (BASE + '/');   // dashboard: destino al cerrar con un unico panel vacio

    // Pantallas que NO se abren en un panel: el POS (usa su propia ventana y su heartbeat), esta misma pantalla (recursion), y el login/salir.
    var RUTAS_NO_PERMITIDAS = [/^\/ventas\/pos(\/|$)/i, /^\/pantalladividida(\/|$)/i, /^\/login(\/|$)/i, /^\/logout(\/|$)/i];
    var RUTA_POS = /^\/ventas\/pos(\/|$)/i;
    var RUTA_PANTALLA_DIVIDIDA = /^\/pantalladividida(\/|$)/i;   // el item «Pantalla dividida» del menu lateral (ya estamos en ella)

    // ---------- estado ----------
    var raiz = null;                // arbol de paneles
    var activo = null;              // id del panel activo
    var maximizado = null;          // id del panel maximizado, o null
    var activoFijoHasta = 0;        // hasta cuando (ms) el foco de un iframe no cambia el panel activo (ver dejarPanelVacioParaElegir)
    var ACTIVO_FIJO_MS = 6000;      // ventana en que las pantallas recien cargadas pueden robar el foco al arrancar
    var paneles = {};               // id -> { el, iframe, titulo, icono, vacio, aviso, urlActual }
    var divisores = {};             // ruta -> { el, info }
    var ventanas = {};              // nombre -> Window de cada ventana independiente abierta desde esta pantalla (la referencia se pierde al recargarla)
    var temporizadorGuardado = null;
    var temporizadorLiberar = null;
    var catalogo = [];              // modulos del menu lateral para el listado de un panel vacio
    var iniciado = false;
    var saliendo = false;           // true al cerrar la pantalla dividida: ya no se guarda la distribucion (pagehide y demoras pendientes)

    // ---------- utilidades ----------
    function avisar(texto, icono) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({ toast: true, position: 'top', icon: icono || 'info', title: texto, showConfirmButton: false, timer: 4000 });
        } else {
            window.alert(texto);
        }
    }

    function esEscritorio() {
        return window.matchMedia ? window.matchMedia('(min-width: ' + ESCRITORIO_ANCHO_MIN_PX + 'px)').matches : true;
    }

    // Ruta sin el PathBase ("/carnisys/Stock/Index" -> "/Stock/Index"), para comparar con las reglas.
    function rutaSinBase(pathname) {
        if (BASE && pathname.toLowerCase().indexOf(BASE.toLowerCase() + '/') === 0) return pathname.substring(BASE.length);
        return pathname;
    }

    // "/Stock/Index?x=1" (pathname + search del mismo sitio) o null si el enlace es de otro sitio o no es http(s).
    function rutaRelativa(href) {
        var destino;
        try { destino = new URL(href, window.location.href); } catch (e) { return null; }
        if (destino.origin !== window.location.origin) return null;
        return destino.pathname + destino.search;
    }

    // ¿Se puede cargar esta url en un panel? Se usa tambien al leer lo guardado (dato hostil).
    function esUrlPermitida(url) {
        if (typeof url !== 'string' || url.charAt(0) !== '/' || url.charAt(1) === '/') return false;
        var destino;
        try { destino = new URL(url, window.location.origin); } catch (e) { return false; }
        if (destino.origin !== window.location.origin) return false;
        if (BASE && destino.pathname.toLowerCase().indexOf(BASE.toLowerCase() + '/') !== 0 && destino.pathname.toLowerCase() !== BASE.toLowerCase()) return false;
        var ruta = rutaSinBase(destino.pathname);
        for (var i = 0; i < RUTAS_NO_PERMITIDAS.length; i++) {
            if (RUTAS_NO_PERMITIDAS[i].test(ruta)) return false;
        }
        return true;
    }

    // La marca ?traspasoDoc=<clave> (documento-abierto.js: lo cargado que viaja de una pantalla a otra) sirve UNA sola vez. La pantalla la saca
    // de su URL recien cuando termina de arrancar (puede ser despues del "load" del iframe), asi que aca se quita al leer la url del panel:
    // nunca se guarda en la distribucion ni se vuelve a mandar (si viajara, el destino leeria una clave ya usada y perderia lo cargado).
    var PARAM_TRASPASO_DOC = 'traspasoDoc';   // mismo nombre que PARAM_TRASPASO de documento-abierto.js
    function sinMarcaTraspaso(busqueda) {
        var params = new URLSearchParams(busqueda || '');
        params.delete(PARAM_TRASPASO_DOC);
        var texto = params.toString();
        return texto ? '?' + texto : '';
    }

    // Inicio ("/" o /Home/Index): sale de la pantalla dividida, no se carga en un panel.
    function esInicio(ruta) {
        var sinBase = rutaSinBase(ruta.split('?')[0]).replace(/\/+$/, '').toLowerCase();
        return sinBase === '' || sinBase === '/home/index';
    }

    function tieneCambiosSinGuardar(panel) {
        try {
            return !!(panel && panel.iframe.contentWindow && panel.iframe.contentWindow.__protegerSalida);
        } catch (e) {
            return false;   // sin acceso al documento del panel: no se puede saber, no se bloquea
        }
    }

    // Pide confirmacion con el aviso de la app («Salir sin guardar»); alConfirmar() corre solo si el usuario elige salir.
    function confirmarSalida(alConfirmar) {
        if (typeof window.confirmarSalidaSinGuardar === 'function') {
            window.confirmarSalidaSinGuardar(alConfirmar);
        } else if (window.confirm('¿Está seguro de salir sin guardar?')) {
            alConfirmar();
        }
    }

    function hayCambiosEnAlgunPanel() {
        return Arbol.listarPaneles(raiz).some(function (p) { return tieneCambiosSinGuardar(paneles[p.id]); });
    }

    // ---------- recarga accidental (F5 / Ctrl+R) ----------
    // 2026-11-21 (docs/DECISIONS.md): con una carga o una edicion sin guardar en algun panel, F5 / Ctrl+R / Ctrl+F5 / Ctrl+Mayus+R NO recargan: se cancela la
    // recarga y se muestra el aviso de la app («Salir sin guardar») con «No, quedarme» como opcion por omision, asi un F5 + Enter accidental ya no
    // pierde lo cargado (el cartel nativo del navegador tiene «Recargar» por omision y no se puede cambiar). Se escucha en esta pagina y, al cargar cada
    // pantalla, tambien dentro de su panel (el foco suele estar en el iframe). No se puede interceptar el boton de recargar del navegador, la barra
    // de direcciones ni cerrar la pestaña: ahi sigue el cartel nativo.
    function esTeclaDeRecarga(ev) {
        var tecla = String(ev.key || '').toLowerCase();
        return tecla === 'f5' || ((ev.ctrlKey || ev.metaKey) && tecla === 'r');
    }

    // Deja de proteger la salida en cada panel: el usuario ya confirmo, y su cartel nativo preguntaria de nuevo al recargar.
    function soltarProteccionDePaneles() {
        Arbol.listarPaneles(raiz).forEach(function (n) {
            var panel = paneles[n.id];
            try { if (panel) panel.iframe.contentWindow.desactivarProteccionSalida(); } catch (e) { /* la pantalla no tiene proteccion de salida */ }
        });
    }

    function alTeclaDeRecarga(ev) {
        if (ev.altKey || !esTeclaDeRecarga(ev) || !iniciado || !hayCambiosEnAlgunPanel()) return;
        ev.preventDefault();
        if (ev.repeat || document.querySelector('.swal2-container')) return;   // dejar apretada la tecla no apila avisos
        confirmarSalida(function () {
            // «Sí, salir» es una decision de descartar: no se devuelve lo cargado al recargar (ver «fotos de lo cargado»).
            apagarFotos();
            soltarProteccionDePaneles();
            window.location.reload();
        });
    }

    // «Recargar» y «Atrás» de la cabecera de un panel con cambios sin guardar: mismo aviso de la app en vez del cartel nativo.
    function conConfirmacionDelPanel(panel, accion) {
        if (!tieneCambiosSinGuardar(panel)) { accion(); return; }
        confirmarSalida(function () {
            descartarFotoDe(panel.id);   // el usuario eligio salir sin guardar de ESE panel
            try { panel.iframe.contentWindow.desactivarProteccionSalida(); } catch (e) { /* la pantalla no tiene proteccion de salida */ }
            accion();
        });
    }

    // ---------- fotos de lo cargado: recuperarlo al recargar (capa 2) ----------
    // 2026-11-21 (docs/DECISIONS.md): si la pagina se recarga igual (boton de recargar del navegador, cartel nativo aceptado, cuelgue), las ALTAS
    // (Elaborados, Elaborados rapido, Movimientos, Stock, Compras, Personas, Productos) vuelven a su panel con lo que tenian. Cada pocos segundos se
    // pide a cada panel con cambios sin guardar una «foto» (DocumentoAbierto.instantanea: el mismo paquete del traspaso, sin soltar nada) y se guarda
    // en localStorage junto a la distribucion. Al arrancar esta pagina por una RECARGA (performance navigation type "reload"), cada panel cuya pantalla
    // sigue siendo la misma se carga con un traspaso hecho de su foto (la misma marca ?traspasoDoc= de dividir / independizar). Entrar de nuevo por
    // un enlace NO devuelve nada (alli el usuario decidio salir). No cubre ediciones de registros ya guardados (instantanea devuelve null).
    var CLAVE_FOTOS = CLAVE_GUARDADO + '-fotos';
    var FOTOS_INTERVALO_MS = 3000;                    // cada cuanto se fotografian los paneles
    var FOTOS_VIGENCIA_MS = 12 * 60 * 60 * 1000;      // una foto mas vieja que esto no se devuelve
    var FOTOS_GRACIA_MS = 10000;                      // un panel recien devuelto no se vuelve a fotografiar hasta que termine de aplicar (o pasen 10 s)
    var fotos = {};            // idPanel -> {url, ts, modulo, idUsuario, grupo, paquete}
    var firmasFotos = {};      // idPanel -> JSON del paquete (no se reescribe lo que no cambio)
    var devueltas = {};        // idPanel -> ms hasta cuando dura la gracia: su foto se acaba de devolver
    var eraSucio = {};         // idPanel -> true: el panel tuvo cambios sin guardar (si despues queda limpio, se guardo o se descarto)
    var fotosApagadas = false; // true tras una salida decidida («Sí, salir»): no se toman mas fotos en esta pagina

    function esRecargaDePagina() {
        try {
            var entrada = window.performance.getEntriesByType('navigation')[0];
            return !!entrada && entrada.type === 'reload';
        } catch (e) {
            return false;
        }
    }

    function rutaSinConsulta(url) { return String(url || '').split('?')[0].toLowerCase(); }

    function guardarFotos() {
        try {
            if (Object.keys(fotos).length) window.localStorage.setItem(CLAVE_FOTOS, JSON.stringify(fotos));
            else window.localStorage.removeItem(CLAVE_FOTOS);
        } catch (e) {
            console.warn('Pantalla dividida: no se pudieron guardar las fotos de lo cargado en este equipo.', e);
        }
    }

    function borrarFotos() {
        fotos = {}; firmasFotos = {}; devueltas = {}; eraSucio = {};
        try { window.localStorage.removeItem(CLAVE_FOTOS); } catch (e) { console.warn('Pantalla dividida: no se pudieron borrar las fotos de lo cargado.', e); }
    }

    function apagarFotos() { fotosApagadas = true; borrarFotos(); }

    function descartarFotoDe(id) {
        if (!fotos[id] && !devueltas[id] && !eraSucio[id]) return;
        delete fotos[id]; delete firmasFotos[id]; delete devueltas[id]; delete eraSucio[id];
        guardarFotos();
    }

    // Lee las fotos guardadas descartando lo vencido o con forma rara (el navegador es input hostil: lo guardado se valida igual que la distribucion).
    function leerFotos() {
        var texto = null;
        try { texto = window.localStorage.getItem(CLAVE_FOTOS); } catch (e) { console.warn('Pantalla dividida: no se pudieron leer las fotos de lo cargado.', e); }
        if (!texto) return {};
        var limpias = {};
        try {
            var datos = JSON.parse(texto);
            Object.keys(datos || {}).forEach(function (id) {
                var f = datos[id];
                if (!f || typeof f !== 'object' || typeof f.url !== 'string' || !esUrlPermitida(f.url)) return;
                if (typeof f.modulo !== 'string' || f.paquete === null || f.paquete === undefined) return;
                if (typeof f.ts !== 'number' || Date.now() - f.ts > FOTOS_VIGENCIA_MS) return;
                limpias[id] = f;
            });
        } catch (e) {
            console.warn('Pantalla dividida: se ignoraron las fotos de lo cargado (' + (e && e.message ? e.message : e) + ').');
        }
        return limpias;
    }

    // Devuelve la distribucion con la url de cada panel que tiene foto (y sigue con la misma pantalla) apuntando a un traspaso hecho de esa foto.
    function conFotosDevueltas(arbol) {
        var api = window.DocumentoAbierto;
        if (!Object.keys(fotos).length || !api || typeof api.crearTraspasoDeFoto !== 'function') return arbol;
        var usadas = {};
        var nuevo = Arbol.mapearUrls(arbol, function (nodo) {
            var foto = fotos[nodo.id];
            if (!foto || nodo.ventana || rutaSinConsulta(foto.url) !== rutaSinConsulta(nodo.url)) return null;
            var traspaso = api.crearTraspasoDeFoto(foto);
            if (!traspaso) return null;
            usadas[nodo.id] = true;
            return traspaso.urlCon(foto.url);
        });
        // Lo que no se pudo devolver (la pantalla del panel cambio, o ya no hay panel) se descarta; lo devuelto sigue fotografiandose.
        Object.keys(fotos).forEach(function (id) {
            if (usadas[id]) devueltas[id] = Date.now() + FOTOS_GRACIA_MS;
            else delete fotos[id];
        });
        guardarFotos();
        return nuevo;
    }

    function tomarFotos() {
        if (!iniciado || saliendo || fotosApagadas || !raiz) return;
        var cambio = false;
        var vigentes = {};
        Arbol.listarPaneles(raiz).forEach(function (nodo) {
            var id = nodo.id;
            vigentes[id] = true;
            var panel = paneles[id];
            var foto = fotos[id];
            // Panel vacio o con su pantalla en una ventana independiente: no hay nada que fotografiar.
            if (!nodo.url || nodo.ventana) {
                if (foto) { delete fotos[id]; delete firmasFotos[id]; cambio = true; }
                delete devueltas[id]; delete eraSucio[id];
                return;
            }
            if (!panel || !panel.urlActual) return;   // la pantalla del panel todavia esta cargando: se conserva lo que haya
            if (foto && foto.url !== panel.urlActual) {   // el panel cambio de pantalla: la foto era de otra
                delete fotos[id]; delete firmasFotos[id]; delete devueltas[id]; delete eraSucio[id];
                foto = null; cambio = true;
            }
            var sucio = tieneCambiosSinGuardar(panel);
            if (sucio) eraSucio[id] = true;
            else if (eraSucio[id]) {   // tenia cambios sin guardar y ya no: se guardo o se descarto
                if (foto) { delete fotos[id]; delete firmasFotos[id]; cambio = true; }
                delete eraSucio[id]; delete devueltas[id];
                return;
            }
            // Recien devuelto: la pantalla puede no haber aplicado todavia lo suyo (no pisar la foto con una carga vacia).
            if (!sucio && devueltas[id] && Date.now() < devueltas[id]) return;
            if (!sucio && !devueltas[id]) {
                if (foto) { delete fotos[id]; delete firmasFotos[id]; cambio = true; }
                return;
            }
            var api = null;
            try { api = panel.iframe.contentWindow.DocumentoAbierto; } catch (e) { return; }
            var instantanea = api && typeof api.instantanea === 'function' ? api.instantanea() : null;
            if (!instantanea) {   // edicion de un registro, o una pantalla que no sabe armar su paquete
                if (foto && !devueltas[id]) { delete fotos[id]; delete firmasFotos[id]; cambio = true; }
                return;
            }
            var firma = JSON.stringify(instantanea.paquete);
            if (foto && firmasFotos[id] === firma) return;   // nada nuevo
            fotos[id] = { url: panel.urlActual, ts: Date.now(), modulo: instantanea.modulo, idUsuario: instantanea.idUsuario, grupo: instantanea.grupo, paquete: instantanea.paquete };
            firmasFotos[id] = firma;
            cambio = true;
        });
        Object.keys(fotos).forEach(function (id) {   // paneles que ya no existen
            if (!vigentes[id]) { delete fotos[id]; delete firmasFotos[id]; delete devueltas[id]; delete eraSucio[id]; cambio = true; }
        });
        if (cambio) guardarFotos();
    }

    // ---------- catalogo de modulos (menu lateral) ----------
    // El menu ya viene filtrado por los permisos del usuario: lo que no ve, no aparece aca.
    function construirCatalogo() {
        var menu = document.getElementById('accordionSidebar');
        if (!menu) return;
        var vistos = {};
        Array.prototype.forEach.call(menu.querySelectorAll('a[href]'), function (a) {
            var href = a.getAttribute('href') || '';
            if (href === '' || href.charAt(0) === '#' || a.getAttribute('data-bs-toggle') || a.classList.contains('sidebar-brand')) return;
            var ruta = rutaRelativa(a.href);
            if (!ruta || !esUrlPermitida(ruta) || esInicio(ruta) || vistos[ruta]) return;
            vistos[ruta] = true;
            var item = a.closest('.nav-item');
            var cabecera = item ? item.querySelector('a.nav-link span') : null;
            var texto = (a.textContent || '').replace(/\s+/g, ' ').trim();
            var grupo = cabecera && !a.classList.contains('nav-link') ? (cabecera.textContent || '').trim() : '';
            if (texto) catalogo.push({ texto: texto, grupo: grupo, url: ruta });
        });
    }

    // ---------- DOM de un panel ----------
    function crearBoton(accion, icono, titulo) {
        var b = document.createElement('button');
        b.type = 'button';
        b.setAttribute('data-accion', accion);
        b.title = titulo;
        b.setAttribute('aria-label', titulo);
        b.innerHTML = '<i class="fas ' + icono + '" aria-hidden="true"></i>';
        return b;
    }

    function crearPanelDom(id) {
        var el = document.createElement('div');
        el.className = 'pd-panel';
        el.setAttribute('data-id', String(id));

        var marco = document.createElement('div');
        marco.className = 'pd-marco';

        var cab = document.createElement('div');
        cab.className = 'pd-cab';
        var icono = document.createElement('i');
        icono.className = 'fas fa-columns pd-icono';
        icono.setAttribute('aria-hidden', 'true');
        var titulo = document.createElement('span');
        titulo.className = 'pd-titulo';
        titulo.textContent = 'Panel vacío';
        cab.appendChild(icono);
        cab.appendChild(titulo);
        cab.appendChild(crearBoton('atras', 'fa-arrow-left', 'Atrás'));
        cab.appendChild(crearBoton('recargar', 'fa-sync-alt', 'Recargar'));
        cab.appendChild(crearBotonesAgregar());
        cab.appendChild(crearBoton('independizar', 'fa-external-link-alt', 'Pasar a una ventana independiente'));
        cab.appendChild(crearBoton('cerrar', 'fa-times', 'Cerrar panel'));

        var cuerpo = document.createElement('div');
        cuerpo.className = 'pd-cuerpo';
        var iframe = document.createElement('iframe');
        iframe.name = PREFIJO_FRAME + id;
        iframe.title = 'Panel ' + id;
        iframe.hidden = true;   // hasta que tenga una pantalla cargada manda el listado del panel vacio
        var vacio = document.createElement('div');
        vacio.className = 'pd-vacio';
        var aviso = document.createElement('div');
        aviso.className = 'pd-aviso';
        aviso.hidden = true;
        cuerpo.appendChild(iframe);
        cuerpo.appendChild(vacio);
        cuerpo.appendChild(aviso);

        // Franja de «esta venta se esta modificando en otra ventana»: en el flujo del panel (entre la cabecera y el contenido) para no tapar el detalle.
        var franja = document.createElement('div');
        franja.className = 'pd-franja';
        franja.hidden = true;
        var franjaTexto = document.createElement('span');
        var franjaBoton = document.createElement('button');
        franjaBoton.type = 'button';
        franjaBoton.textContent = 'Ir a esa ventana';
        franja.appendChild(franjaTexto);
        franja.appendChild(franjaBoton);

        marco.appendChild(cab);
        marco.appendChild(franja);
        marco.appendChild(cuerpo);
        el.appendChild(marco);
        area.appendChild(el);

        var panel = {
            id: id, el: el, iframe: iframe, titulo: titulo, icono: icono, vacio: vacio, aviso: aviso, urlActual: null, ultimaValida: null,
            enVentana: null,          // nombre de la ventana independiente a la que se paso la pantalla (se muestra su aviso en vez del iframe)
            ventanaCerrada: false,    // true si se detecto que esa ventana ya no esta abierta
            avisoClave: null,         // evita rearmar el aviso en cada dibujo
            tituloPrevio: '',         // titulo que tenia el panel antes de pasar a la ventana
            alVaciar: null,           // callback a correr cuando el iframe termina de quedar en blanco
            franja: franja, franjaTexto: franjaTexto, franjaIdVenta: 0   // aviso de edicion de venta en otra ventana (actualizarFranjasDeEdicion)
        };
        franjaBoton.addEventListener('click', function () { irAVentanaDeEdicion(panel); });

        // Un clic en cualquier parte del panel (cabecera, listado) lo marca activo; dentro del iframe lo hace el listener de cada documento cargado.
        el.addEventListener('mousedown', function () { activar(id); }, true);
        cab.addEventListener('click', function (ev) {
            var boton = ev.target.closest ? ev.target.closest('button[data-accion]') : null;
            if (!boton) return;
            var accion = boton.getAttribute('data-accion');
            // La flechita de «Agregar panel» abre el menu de posiciones; el resto de los botones pasa por accionDePanel.
            if (accion === 'agregar-menu') { activar(id); alternarMenuAgregar(id, boton); } else accionDePanel(id, accion);
        });
        // 2026-11-24: el doble clic en la cabecera ya no maximiza (se quito «Maximizar»; ver docs/DECISIONS.md).
        iframe.addEventListener('load', function () { alCargarMarco(panel); });

        armarListadoVacio(panel);
        return panel;
    }

    // Listado de modulos del panel vacio, con buscador.
    function armarListadoVacio(panel) {
        panel.vacio.innerHTML = '';
        var titulo = document.createElement('h2');
        titulo.textContent = 'Elegí qué abrir en este panel';
        var buscador = document.createElement('input');
        buscador.type = 'search';
        buscador.placeholder = 'Buscar un módulo';
        buscador.setAttribute('aria-label', 'Buscar un módulo');
        var lista = document.createElement('ul');
        lista.className = 'pd-lista';
        catalogo.forEach(function (m) {
            var li = document.createElement('li');
            var b = document.createElement('button');
            b.type = 'button';
            b.setAttribute('data-url', m.url);
            b.setAttribute('data-busqueda', (m.grupo + ' ' + m.texto).toLowerCase());
            b.textContent = m.texto;
            if (m.grupo) {
                var g = document.createElement('small');
                g.textContent = m.grupo;
                b.appendChild(g);
            }
            li.appendChild(b);
            lista.appendChild(li);
        });
        buscador.addEventListener('input', function () {
            var q = buscador.value.trim().toLowerCase();
            Array.prototype.forEach.call(lista.children, function (li) {
                var b = li.firstChild;
                li.hidden = q !== '' && b.getAttribute('data-busqueda').indexOf(q) === -1;
            });
        });
        lista.addEventListener('click', function (ev) {
            var b = ev.target.closest ? ev.target.closest('button[data-url]') : null;
            if (b) cargarEnPanel(panel.id, b.getAttribute('data-url'));
        });
        panel.vacio.appendChild(titulo);
        panel.vacio.appendChild(buscador);
        panel.vacio.appendChild(lista);
    }

    // ---------- carga de paginas en un panel ----------
    // Navega el iframe SIN agregar una entrada al historial del navegador (replace); si no hay acceso al documento, usa src.
    function navegarMarco(panel, url) {
        var absoluta = window.location.origin + url;
        var ventana = null;
        try { ventana = panel.iframe.contentWindow; } catch (e) { ventana = null; }
        if (ventana && panel.iframe.getAttribute('src')) {
            try { ventana.location.replace(absoluta); return; } catch (e) { /* cae a src */ }
        }
        panel.iframe.src = absoluta;
    }

    function mostrarPanelCon(panel, url) {
        panel.enVentana = null;
        panel.ventanaCerrada = false;
        panel.avisoClave = null;
        panel.urlActual = url;
        panel.vacio.hidden = true;
        panel.aviso.hidden = true;
        panel.iframe.hidden = false;
        navegarMarco(panel, url);
    }

    function mostrarPanelVacio(panel) {
        panel.urlActual = null;
        panel.ultimaValida = null;
        panel.iframe.hidden = true;
        panel.aviso.hidden = true;
        panel.vacio.hidden = false;
        panel.titulo.textContent = 'Panel vacío';
        panel.icono.className = 'fas fa-columns pd-icono';
        try { panel.iframe.removeAttribute('src'); panel.iframe.contentWindow.location.replace('about:blank'); } catch (e) { /* sin acceso: no hay nada que limpiar */ }
    }

    // Carga una pantalla en un panel (desde el listado o el menu lateral). Pide confirmar si lo que hay tiene cambios sin guardar.
    function cargarEnPanel(id, url, confirmado) {
        var panel = paneles[id];
        if (!panel) return;
        if (!esUrlPermitida(url)) {
            avisar('Esa pantalla no se abre en un panel.', 'info');
            return;
        }
        if (!confirmado && tieneCambiosSinGuardar(panel)) {
            confirmarSalida(function () { cargarEnPanel(id, url, true); });
            return;
        }
        raiz = Arbol.cargarUrl(raiz, id, url);
        activar(id);
        mostrarPanelCon(panel, url);
        programarGuardado();
        programarLiberar();
    }

    // Termino de cargar una pagina dentro de un panel: toma su ruta real (puede haber navegado desde adentro), su titulo y engancha el "activo".
    function alCargarMarco(panel) {
        var ventana, documento, ubicacion;
        try {
            ventana = panel.iframe.contentWindow;
            documento = panel.iframe.contentDocument;
            ubicacion = ventana.location;
        } catch (e) {
            return;   // otro origen: no es una pantalla de la app
        }
        if (!documento || ubicacion.href === 'about:blank') {
            // El panel termino de quedar en blanco (por ejemplo al pasar su pantalla a una ventana independiente): se sigue con lo que esperaba.
            if (panel.alVaciar) {
                var seguir = panel.alVaciar;
                panel.alVaciar = null;
                seguir();
            }
            return;
        }

        var ruta = ubicacion.pathname + sinMarcaTraspaso(ubicacion.search);
        if (!esUrlPermitida(ruta)) {
            rechazarPantalla(panel, ruta, ventana);
            return;
        }

        panel.ultimaValida = ruta;
        panel.urlActual = ruta;
        panel.vacio.hidden = true;
        panel.aviso.hidden = true;
        // 2026-11-24: si el panel estaba rechazando una pantalla (iframe oculto con el cartel «se abre aparte») y se vuelve atras a una valida, el iframe
        // tiene que mostrarse de nuevo; antes quedaba oculto y el panel parecia vacio aunque el detalle estaba cargado.
        panel.iframe.hidden = false;
        if (Arbol.buscarPanel(raiz, panel.id)) raiz = Arbol.cargarUrl(raiz, panel.id, ruta);
        actualizarTitulo(panel, documento);

        // Los clics dentro del iframe no llegan a esta pagina: cada documento nuevo avisa que su panel pasa a ser el activo.
        try {
            ventana.addEventListener('pointerdown', function () { activar(panel.id); }, true);
            ventana.addEventListener('focus', function () { activar(panel.id); });
            ventana.addEventListener('keydown', alTeclaDeRecarga, true);   // F5 con el foco dentro del panel (ver «recarga accidental»)
            ventana.addEventListener('click', function (ev) { alClicEnPanel(panel, ev); }, true);   // «Modificar venta»: se abre aparte sin salir del detalle
        } catch (e) { /* sin acceso: queda el clic en la cabecera */ }

        programarGuardado();
        programarLiberar();
    }

    // El panel navego (por un enlace de adentro) a una pantalla que no puede vivir en un panel: se vacia el iframe y se ofrece abrirla aparte.
    function rechazarPantalla(panel, ruta, ventana) {
        // Una pantalla que se cargo entera (el POS) puede tener su propio aviso de salida: se lo apaga para que no salte «¿Deseas abandonar el sitio?».
        try { if (typeof ventana.desactivarAvisoSalidaPOS === 'function') ventana.desactivarAvisoSalidaPOS(); } catch (e) { /* sin acceso: se sigue igual */ }
        try { ventana.location.replace('about:blank'); } catch (e) { /* ya se esta descargando */ }
        panel.iframe.hidden = true;
        panel.aviso.innerHTML = '';
        var texto = document.createElement('div');
        texto.textContent = RUTA_POS.test(rutaSinBase(ruta.split('?')[0]))
            ? 'El punto de venta se abre aparte.'
            : 'Esta pantalla no se abre en un panel.';
        var boton = document.createElement('button');
        boton.type = 'button';
        boton.className = 'btn btn-sm btn-primary';
        boton.textContent = 'Abrir en ventana independiente';
        boton.addEventListener('click', function () { abrirAparte(ruta); });
        panel.aviso.appendChild(texto);
        panel.aviso.appendChild(boton);
        panel.aviso.hidden = false;
        panel.titulo.textContent = 'Se abre aparte';
        actualizarBotonCerrar();
    }

    // Titulo del panel = «Seccion · Pantalla» (la seccion sale del <meta name="carnisys-seccion"> de _SeccionVentana.cshtml).
    function actualizarTitulo(panel, documento) {
        var pantalla = (documento.title || '').replace(SUFIJO_TITULO, '').trim();
        var meta = documento.querySelector('meta[name="carnisys-seccion"]');
        var seccion = meta ? (meta.getAttribute('content') || '').trim() : '';
        var icono = meta ? (meta.getAttribute('data-icono') || '').trim() : '';
        var texto = seccion && pantalla && seccion.toLowerCase() !== pantalla.toLowerCase() ? seccion + ' · ' + pantalla : (pantalla || seccion || 'Panel');
        panel.titulo.textContent = texto;
        panel.titulo.title = texto;
        panel.icono.className = 'fas ' + (/^fa-[a-z0-9-]+$/.test(icono) ? icono : 'fa-window-maximize') + ' pd-icono';
    }

    // Nombre de la ventana independiente de una pantalla: el MISMO que arma el clic derecho del menu lateral (menu-ventana-independiente.js,
    // normalizarRuta), asi una pantalla tiene una sola ventana aunque se abra desde distintos lugares.
    function nombreVentanaDe(url) {
        var ruta = url.split('?')[0].toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '') || 'inicio';
        return (PREFIJO_VENTANA_PANTALLA + ruta).slice(0, PREFIJO_VENTANA_PANTALLA.length + VENTANA_NOMBRE_MAX);
    }

    // Copia: abre la pantalla en una ventana sin tocar el panel (la usa el aviso del POS).
    function abrirAparte(url) {
        var ventana = window.open(window.location.origin + url, nombreVentanaDe(url), 'popup=yes,width=' + VENTANA_ANCHO_PX + ',height=' + VENTANA_ALTO_PX + ',resizable=yes,scrollbars=yes');
        if (!ventana) avisar('El navegador bloqueó la ventana emergente. Permitila para este sitio e intentá de nuevo.', 'warning');
        else ventana.focus();
    }

    // ---------- editar una venta en una ventana aparte (desde el detalle de venta de un panel) ----------
    // 2026-11-24 (docs/DECISIONS.md): «Modificar venta» y «Cambiar forma de pago» del detalle de venta son enlaces al POS, que no puede vivir en un panel.
    // Antes el panel cargaba el POS entero, lo vaciaba (con el cartel «¿Deseas abandonar el sitio?») y perdia el detalle. Ahora el clic se intercepta
    // ANTES de navegar: el detalle queda en el panel, el POS se abre en una ventana aparte, una franja avisa que la venta se esta modificando y, al
    // guardar, el POS le avisa a esta pagina (window.opener.PantallaDividida.edicionDeVentaTerminada) y los paneles con ese detalle se recargan.
    var RUTA_DETALLE_VENTA = /^\/ventas\/detalleventa(\/|$)/i;
    var edicionesVenta = {};   // idVenta -> Window de la ventana donde se esta modificando esa venta (solo las abiertas desde esta pagina)

    // idVenta del enlace «Modificar venta» / «Cambiar forma de pago» (?idVentaEditar=N en /Ventas/POS), o 0 si el enlace no es de edicion.
    function idVentaDeEnlaceDeEdicion(ruta) {
        var destino;
        try { destino = new URL(ruta, window.location.origin); } catch (e) { return 0; }
        if (!RUTA_POS.test(rutaSinBase(destino.pathname))) return 0;
        var id = Number(destino.searchParams.get('idVentaEditar'));
        return isFinite(id) && id > 0 && Math.floor(id) === id ? id : 0;
    }

    // idVenta que muestra el panel si esta en el detalle de una venta (/Ventas/DetalleVenta?id=N), o 0.
    function idVentaDelDetalle(panel) {
        if (!panel || !panel.urlActual) return 0;
        var destino;
        try { destino = new URL(panel.urlActual, window.location.origin); } catch (e) { return 0; }
        if (!RUTA_DETALLE_VENTA.test(rutaSinBase(destino.pathname))) return 0;
        var id = Number(destino.searchParams.get('id'));
        return isFinite(id) && id > 0 && Math.floor(id) === id ? id : 0;
    }

    // Clic dentro de un panel (fase de captura, antes que los manejadores de la propia pantalla): si es un enlace de edicion de venta, no navega.
    function alClicEnPanel(panel, ev) {
        if (ev.defaultPrevented || ev.button !== 0 || ev.ctrlKey || ev.metaKey || ev.shiftKey || ev.altKey) return;
        var enlace = ev.target && ev.target.closest ? ev.target.closest('a[href]') : null;
        if (!enlace || enlace.target === '_blank' || enlace.hasAttribute('download')) return;
        var ruta = rutaRelativa(enlace.href);
        var idVenta = ruta ? idVentaDeEnlaceDeEdicion(ruta) : 0;
        if (!idVenta) return;   // cualquier otro enlace sigue su camino (un POS que no es edicion cae en rechazarPantalla)
        ev.preventDefault();
        ev.stopPropagation();
        abrirEdicionDeVentaAparte(panel, ruta, idVenta);
    }

    function abrirEdicionDeVentaAparte(panel, ruta, idVenta) {
        var viva = edicionesVenta[idVenta];
        if (viva && !viva.closed) {   // ya se esta modificando: se trae esa ventana al frente en vez de abrir otra
            try { viva.focus(); } catch (e) { console.warn('Pantalla dividida: el navegador no dejó traer al frente la ventana de edición.', e); }
            avisar('La venta #' + idVenta + ' ya se está modificando en otra ventana.', 'info');
            return;
        }
        var ventana = window.open(window.location.origin + ruta, 'carnisys-trabajo-editar-venta-' + idVenta, caracteristicasVentana(panel));
        if (!ventana) { avisarVentanaBloqueada(); return; }
        edicionesVenta[idVenta] = ventana;
        try { ventana.focus(); } catch (e) { /* window.open ya la activo */ }
        avisar('La edición de la venta #' + idVenta + ' se abre en una ventana aparte. Este detalle se actualiza solo al guardar.', 'info');
        actualizarFranjasDeEdicion();
    }

    // Lo llama el POS de una ventana abierta desde aca (forma-pago.js / POS.cshtml) al guardar o salir de la edicion. `ventana` es la del POS: solo se
    // atiende si esta pagina la abrio para editar una venta. Devuelve true si la atendio (el POS entonces se cierra solo).
    function edicionDeVentaTerminada(ventana, guardada) {
        var idVenta = 0;
        Object.keys(edicionesVenta).forEach(function (k) { if (edicionesVenta[k] === ventana) idVenta = Number(k); });
        if (!idVenta) return false;
        delete edicionesVenta[idVenta];
        if (guardada) {
            Object.keys(paneles).forEach(function (k) {
                var panel = paneles[k];
                if (idVentaDelDetalle(panel) !== idVenta) return;
                try { panel.iframe.contentWindow.location.reload(); } catch (e) { if (panel.urlActual) navegarMarco(panel, panel.urlActual); }
            });
            avisar('Venta #' + idVenta + ' actualizada.', 'success');
        }
        actualizarFranjasDeEdicion();
        return true;
    }

    // Franja sobre el contenido del panel mientras su detalle de venta se esta modificando en otra ventana.
    function actualizarFranjasDeEdicion() {
        Object.keys(edicionesVenta).forEach(function (k) {
            var v = edicionesVenta[k];
            if (!v || v.closed) delete edicionesVenta[k];   // la ventana se cerro sin guardar: se acaba el aviso
        });
        Object.keys(paneles).forEach(function (k) {
            var panel = paneles[k];
            var idVenta = idVentaDelDetalle(panel);
            var enEdicion = idVenta > 0 && !!edicionesVenta[idVenta];
            if (enEdicion && panel.franjaIdVenta !== idVenta) {
                panel.franjaTexto.textContent = 'Modificando la venta #' + idVenta + ' en otra ventana. Al guardar, este detalle se actualiza solo.';
                panel.franjaIdVenta = idVenta;
            }
            if (!enEdicion) panel.franjaIdVenta = 0;
            panel.franja.hidden = !enEdicion;
        });
    }

    function irAVentanaDeEdicion(panel) {
        var v = edicionesVenta[panel.franjaIdVenta];
        if (!v || v.closed) { actualizarFranjasDeEdicion(); return; }
        try { v.focus(); } catch (e) { avisar('El navegador no dejó traer la ventana al frente: buscala en la barra de tareas.', 'info'); }
    }

    // ---------- ventana independiente: pasar un panel a su propia ventana ----------
    // "Independizar" MUEVE la pantalla del panel a una ventana nueva (no la copia): el panel se vacia y muestra un aviso con «Ir a la pantalla independiente»
    // y «Traer de vuelta al panel». Vaciar el panel ANTES de cargar la ventana evita dos documentos iguales abiertos a la vez (aviso de documento abierto).
    // La ventana es la de siempre (nombre carnisys-trabajo-menu-<ruta>, sin menu lateral ni barra: ver _Layout.cshtml); al abrirla se ubica donde estaba el panel.

    // Ubicacion y tamano de pantalla donde estaba el panel (aproximado: el navegador agrega su barra de titulo). Panel no medible: tamano por omision.
    function caracteristicasVentana(panel) {
        var r = panel.el.getBoundingClientRect();
        var base = 'popup=yes,resizable=yes,scrollbars=yes,';
        if (r.width < 1 || r.height < 1) return base + 'width=' + VENTANA_ANCHO_PX + ',height=' + VENTANA_ALTO_PX;
        var bordeLateral = Math.max(0, (window.outerWidth - window.innerWidth) / 2);
        var barraSuperior = Math.max(0, window.outerHeight - window.innerHeight - bordeLateral);
        var izquierda = Math.round(window.screenX + bordeLateral + r.left);
        var arriba = Math.round(window.screenY + barraSuperior + r.top);
        var ancho = Math.max(VENTANA_MIN_ANCHO_PX, Math.round(r.width));
        var alto = Math.max(VENTANA_MIN_ALTO_PX, Math.round(r.height));
        return base + 'left=' + izquierda + ',top=' + arriba + ',width=' + ancho + ',height=' + alto;
    }

    function paginaCargada(ventana) {
        try { return ventana.location.href !== 'about:blank'; } catch (e) { return true; /* otro origen: ya tenia pagina */ }
    }

    // Pide a la pantalla de "ventana" (el iframe de un panel o una ventana independiente) que deje lo cargado listo para seguir en otro
    // documento (DocumentoAbierto.prepararTraspaso: hoy Elaborados, Elaborados rapido, Movimientos y Stock en un Nuevo). Devuelve
    // {urlCon, ceder, descartar} o null si esa pantalla no sabe hacerlo (ahi queda el aviso «Salir sin guardar»).
    function prepararTraspasoDe(ventana) {
        try {
            var api = ventana && ventana.DocumentoAbierto;
            return api && typeof api.prepararTraspaso === 'function' ? api.prepararTraspaso() : null;
        } catch (e) {
            console.warn('Pantalla dividida: no se pudo preparar el traspaso de lo cargado.', e);
            return null;
        }
    }

    function ventanaTieneCambios(ventana) {
        try { return !!ventana.__protegerSalida; } catch (e) { return false; }
    }

    // Devuelve la ventana independiente "nombre" si sigue abierta; null si ya no existe; false si el navegador bloqueo la busqueda.
    // Sin referencia (se recargo esta pantalla) la busca por nombre: window.open('', nombre) devuelve la existente y, si no habia, crea una en blanco
    // que se cierra enseguida. Hay que llamarla desde un clic del usuario.
    function obtenerVentana(nombre) {
        var guardada = ventanas[nombre];
        if (guardada && !guardada.closed) return guardada;
        delete ventanas[nombre];
        var ventana = window.open('', nombre);
        if (!ventana) return false;
        if (!paginaCargada(ventana)) {
            try { ventana.close(); } catch (e) { console.warn('Pantalla dividida: no se pudo cerrar la ventana en blanco.', e); }
            return null;
        }
        ventanas[nombre] = ventana;
        return ventana;
    }

    function avisarVentanaBloqueada() {
        avisar('El navegador bloqueó la ventana emergente. Permitila para este sitio e intentá de nuevo.', 'warning');
    }

    // Deja el iframe en blanco y corre alTerminar() cuando termino (con tope de espera: nunca queda colgado).
    function vaciarMarco(panel, alTerminar) {
        var hecho = false;
        var seguir = function () {
            if (hecho) return;
            hecho = true;
            panel.alVaciar = null;
            alTerminar();
        };
        panel.alVaciar = seguir;
        window.setTimeout(seguir, ESPERA_VACIAR_MARCO_MS);
        try { panel.iframe.contentWindow.location.replace('about:blank'); } catch (e) { seguir(); }
    }

    function tituloPorUrl(url) {
        for (var i = 0; i < catalogo.length; i++) {
            if (catalogo[i].url === url) return catalogo[i].texto;
        }
        return 'Pantalla';
    }

    function independizarPanel(id, confirmado) {
        var panel = paneles[id];
        var nodo = Arbol.buscarPanel(raiz, id);
        if (!panel || !nodo) return;
        if (nodo.ventana) { irAVentana(id); return; }   // ya esta en una ventana
        if (!panel.urlActual) { avisar('El panel está vacío: elegí una pantalla primero.', 'info'); return; }
        // Una carga que sabe pasar lo cargado sigue en la ventana con todo; si no, con cambios sin guardar se pide confirmar.
        var traspaso = confirmado ? null : prepararTraspasoDe(panel.iframe.contentWindow);
        if (!traspaso && !confirmado && tieneCambiosSinGuardar(panel)) {
            confirmarSalida(function () { independizarPanel(id, true); });
            return;
        }

        var url = panel.urlActual;
        var nombre = nombreVentanaDe(url);
        var ventana = window.open('', nombre, caracteristicasVentana(panel));   // dentro del clic (o del «Sí, salir»): el navegador la permite
        if (!ventana) {
            if (traspaso) traspaso.descartar();
            avisarVentanaBloqueada();
            return;
        }
        var yaTeniaPagina = paginaCargada(ventana);
        if (yaTeniaPagina && traspaso) {
            // Ya hay una ventana de esta pantalla con su propio contenido: lo cargado no tiene donde ir sin pisarla. El panel sigue como estaba.
            traspaso.descartar();
            try { ventana.focus(); } catch (e) { console.warn('Pantalla dividida: no se pudo traer la ventana al frente.', e); }
            avisar('Ya hay una ventana independiente de esta pantalla. Guardá o cerrá esa antes de pasar la de este panel.', 'warning');
            return;
        }
        if (traspaso) {
            url = traspaso.urlCon(url);
            traspaso.ceder();   // el panel deja de guardar borrador y de proteger la salida: lo cargado sigue en la ventana
        }
        ventanas[nombre] = ventana;

        panel.tituloPrevio = panel.titulo.textContent;
        raiz = Arbol.independizar(raiz, id, nombre);
        activo = id;
        vaciarMarco(panel, function () {
            try {
                if (yaTeniaPagina) {
                    avisar('Esa pantalla ya tenía una ventana independiente abierta: la traje al frente.');
                } else {
                    ventana.location.href = window.location.origin + url;
                }
                ventana.focus();
            } catch (e) {
                console.warn('Pantalla dividida: no se pudo cargar la ventana independiente.', e);
            }
        });
        dibujar();
        programarLiberar();
    }

    // Aviso del panel cuya pantalla esta en una ventana independiente (o que ya se cerro). Se rearma solo si cambia el estado.
    function mostrarAvisoVentana(panel, nodo) {
        var clave = nodo.ventana + '|' + (panel.ventanaCerrada ? 'cerrada' : 'abierta');
        if (panel.avisoClave === clave && !panel.aviso.hidden) return;
        panel.avisoClave = clave;
        panel.enVentana = nodo.ventana;
        panel.urlActual = null;
        panel.iframe.hidden = true;
        panel.vacio.hidden = true;

        var nombre = panel.tituloPrevio || tituloPorUrl(nodo.url);
        panel.titulo.textContent = 'Ventana independiente · ' + nombre;
        panel.icono.className = 'fas fa-external-link-alt pd-icono';

        panel.aviso.innerHTML = '';
        var texto = document.createElement('div');
        texto.textContent = panel.ventanaCerrada
            ? 'La ventana independiente de ' + nombre + ' se cerró.'
            : nombre + ' se abrió en una ventana independiente.';
        var botones = document.createElement('div');
        botones.className = 'pd-botones';
        var crear = function (clase, etiqueta, alClic) {
            var b = document.createElement('button');
            b.type = 'button';
            b.className = 'btn btn-sm ' + clase;
            b.textContent = etiqueta;
            b.addEventListener('click', alClic);
            botones.appendChild(b);
        };
        if (panel.ventanaCerrada) {
            crear('btn-primary', 'Volver a cargarla en el panel', function () { traerDeVuelta(panel.id); });
        } else {
            crear('btn-primary', 'Ir a la pantalla independiente', function () { irAVentana(panel.id); });
            crear('btn-outline-secondary', 'Traer de vuelta al panel', function () { traerDeVuelta(panel.id); });
        }
        panel.aviso.appendChild(texto);
        panel.aviso.appendChild(botones);
        panel.aviso.hidden = false;
    }

    function marcarVentanaCerrada(panel) {
        panel.ventanaCerrada = true;
        panel.avisoClave = null;
        dibujar();
        programarLiberar();   // la pantalla de una ventana cerrada deja de contar como abierta para el operador autorizado
    }

    // «Ir a la pantalla independiente»: la trae al frente. Si ya no existe, el panel pasa a ofrecer volver a cargarla.
    function irAVentana(id) {
        var panel = paneles[id];
        var nodo = Arbol.buscarPanel(raiz, id);
        if (!panel || !nodo || !nodo.ventana) return;
        var ventana = obtenerVentana(nodo.ventana);
        if (ventana === false) { avisarVentanaBloqueada(); return; }
        if (ventana === null) { marcarVentanaCerrada(panel); return; }
        try { ventana.focus(); } catch (e) { console.warn('Pantalla dividida: no se pudo traer la ventana al frente.', e); }
    }

    // «Traer de vuelta al panel»: cierra la ventana (pidiendo confirmar si tiene cambios sin guardar) y vuelve a cargar la pantalla en el panel.
    function traerDeVuelta(id, confirmado) {
        var panel = paneles[id];
        var nodo = Arbol.buscarPanel(raiz, id);
        if (!panel || !nodo || !nodo.ventana) return;

        var traspaso = null;   // lo cargado en la ventana, si la pantalla sabe pasarlo
        var cargarEnElPanel = function () {
            raiz = Arbol.traerDeVuelta(raiz, id);
            if (traspaso) raiz = Arbol.cargarUrl(raiz, id, traspaso.urlCon(nodo.url));   // una sola carga con la marca; al cargar queda la url limpia
            panel.enVentana = null;
            panel.ventanaCerrada = false;
            panel.avisoClave = null;
            activo = id;
            dibujar();            // el panel ya no tiene ventana: se vuelve a cargar con su url
            programarLiberar();
        };
        if (panel.ventanaCerrada) { cargarEnElPanel(); return; }

        var ventana = obtenerVentana(nodo.ventana);
        if (ventana === false) { avisarVentanaBloqueada(); return; }
        if (ventana === null) { cargarEnElPanel(); return; }   // ya no estaba abierta
        traspaso = confirmado ? null : prepararTraspasoDe(ventana);
        if (!traspaso && !confirmado && ventanaTieneCambios(ventana)) {
            confirmarSalida(function () { traerDeVuelta(id, true); });
            return;
        }
        if (traspaso) traspaso.ceder();   // la ventana deja de guardar borrador y de proteger la salida: lo cargado vuelve al panel
        try { ventana.close(); } catch (e) { console.warn('Pantalla dividida: no se pudo cerrar la ventana independiente.', e); }
        delete ventanas[nodo.ventana];
        // Se espera a que termine de cerrarse: asi suelta sus candados de «documento abierto» antes de que el panel cargue lo mismo.
        var intentos = 0;
        var esperar = function () {
            if (ventana.closed || ++intentos >= INTENTOS_CIERRE_VENTANA) cargarEnElPanel();
            else window.setTimeout(esperar, ESPERA_CIERRE_VENTANA_MS);
        };
        esperar();
    }

    // Cada segundo: si una ventana abierta desde esta pantalla se cerro, el panel lo avisa. Tras recargar la pantalla dividida no hay referencia a las
    // ventanas: ahi el cierre se descubre al tocar «Ir a la pantalla independiente».
    function revisarVentanas() {
        Arbol.listarPaneles(raiz).forEach(function (nodo) {
            var panel = paneles[nodo.id];
            var ventana = nodo.ventana ? ventanas[nodo.ventana] : null;
            if (panel && ventana && !panel.ventanaCerrada && ventana.closed) {
                delete ventanas[nodo.ventana];
                marcarVentanaCerrada(panel);
            }
        });
        actualizarFranjasDeEdicion();   // una ventana de edicion de venta que se cerro sin guardar apaga su franja
    }

    // Pantallas que siguen abiertas (para el operador autorizado): las de los paneles y las de las ventanas independientes que siguen abiertas.
    function urlsParaPermisos() {
        return Arbol.listarPaneles(raiz).filter(function (nodo) {
            var panel = paneles[nodo.id];
            return nodo.url && !(nodo.ventana && panel && panel.ventanaCerrada);
        }).map(function (nodo) { return nodo.url; });
    }

    // ---------- acciones sobre paneles ----------
    function activar(id) {
        if (activo === id || !paneles[id]) return;
        activo = id;
        Object.keys(paneles).forEach(function (k) { paneles[k].el.classList.toggle('pd-activo', Number(k) === id); });
        programarGuardado();
    }

    function accionDePanel(id, accion) {
        var panel = paneles[id];
        if (!panel) return;
        activar(id);
        cerrarMenuAgregar();
        if (accion === 'agregar') agregarPanel(id, 'derecha');
        else if (accion === 'cerrar') cerrarPanel(id);
        else if (accion === 'independizar') independizarPanel(id);
        else if (panel.enVentana) return;   // Atrás / Recargar no aplican: la pantalla esta en su ventana independiente
        else if (accion === 'recargar') {
            conConfirmacionDelPanel(panel, function () {
                try { panel.iframe.contentWindow.location.reload(); } catch (e) { if (panel.urlActual) navegarMarco(panel, panel.urlActual); }
            });
        }
        else if (accion === 'atras') {
            if (!panelTienePantalla(panel)) return;   // ya es el menu de modulos: no hay a donde volver
            conConfirmacionDelPanel(panel, function () { volverAtrasEnPanel(panel); });
        }
    }

    // ---------- «Atrás» de un panel ----------
    // 2026-11-24 (docs/DECISIONS.md): antes era iframe.contentWindow.history.back(), que retrocede el historial de TODA la pestaña (la pagina principal y
    // los iframes de todos los paneles, mezclados): en un panel sin pantallas previas sacaba al usuario de la pantalla dividida, y con varios paneles
    // deshacia la navegacion de OTRO panel. Ahora solo se mira el historial propio del iframe (Navigation API: navigation.entries() de un iframe son
    // solo las suyas) y, si no queda pantalla anterior, el panel vuelve a su menu de modulos. Nunca se toca el historial de la pestaña.
    function panelTienePantalla(panel) {
        var nodo = Arbol.buscarPanel(raiz, panel.id);
        return !!nodo && !!nodo.url && !nodo.ventana;
    }

    // true si el iframe del panel tiene una pantalla anterior propia a la que volver (una entrada about:blank no cuenta: es el panel vaciado).
    function hayPantallaAnteriorEnPanel(panel) {
        try {
            var nav = panel.iframe.contentWindow.navigation;
            if (!nav || !nav.canGoBack || !nav.currentEntry) return false;
            var previa = nav.entries()[nav.currentEntry.index - 1];
            return !!previa && !!previa.url && previa.url !== 'about:blank';
        } catch (e) {
            return false;   // sin Navigation API o sin acceso al documento: se va al menu, que es el destino seguro
        }
    }

    function volverAtrasEnPanel(panel) {
        if (hayPantallaAnteriorEnPanel(panel)) {
            try {
                var recorrido = panel.iframe.contentWindow.navigation.back();
                // Si la pantalla cancela la salida o el recorrido falla, la promesa se rechaza: se avisa en consola y el panel queda como estaba.
                var avisarFallo = function (e) { console.warn('Pantalla dividida: no se pudo volver atrás en el panel', panel.id, e); };
                if (recorrido && recorrido.finished) recorrido.finished.catch(avisarFallo);
                if (recorrido && recorrido.committed) recorrido.committed.catch(avisarFallo);
                return;
            } catch (e) {
                console.warn('Pantalla dividida: no se pudo volver atrás en el panel', panel.id, e);
            }
        }
        volverAlMenuDelPanel(panel);
    }

    // El panel queda vacio (con la lista de modulos), igual que un panel nuevo. No toca ningun otro panel.
    function volverAlMenuDelPanel(panel) {
        raiz = Arbol.cargarUrl(raiz, panel.id, null);
        descartarFotoDe(panel.id);   // ya no hay pantalla que fotografiar
        dibujar();                   // dibujar() ve el panel sin url y llama a mostrarPanelVacio
        programarLiberar();
    }

    // «Atrás» apagado (sin disabled de verdad, igual que los atajos: el tooltip explica) cuando el panel es el menu de modulos o su pantalla esta en una ventana.
    function actualizarBotonesAtras() {
        if (!raiz) return;
        Object.keys(paneles).forEach(function (k) {
            var panel = paneles[k];
            var boton = panel.el.querySelector('button[data-accion="atras"]');
            if (!boton) return;
            var activo = panelTienePantalla(panel);
            boton.setAttribute('aria-disabled', activo ? 'false' : 'true');
            boton.title = activo ? 'Atrás (si no hay pantalla anterior, vuelve al menú de módulos)' : 'Atrás: este panel ya está en el menú de módulos';
        });
    }

    function tamanoPx(id) {
        var rect = Arbol.calcularRectangulos(raiz).paneles.filter(function (p) { return p.id === id; })[0];
        return rect ? { w: rect.w * area.clientWidth, h: rect.h * area.clientHeight } : { w: 0, h: 0 };
    }

    function dividirPanel(id, direccion) {
        if (Arbol.contarPaneles(raiz) >= Arbol.MAX_PANELES) {
            avisar('Máximo ' + Arbol.MAX_PANELES + ' paneles. Cerrá uno para dividir otro.', 'warning');
            return;
        }
        var tam = tamanoPx(id);
        if ((direccion === 'fila' ? tam.w : tam.h) / 2 < MIN_PANEL_PX) {
            avisar('No hay lugar para otro panel acá. Agrandá este panel o cerrá otro.', 'warning');
            return;
        }
        var resultado = Arbol.dividir(raiz, id, direccion, Arbol.siguienteId(raiz));
        if (!resultado) return;
        raiz = resultado.raiz;
        activo = resultado.nuevoId;
        maximizado = null;
        dibujar();
    }

    // ---------- «Agregar panel» ----------
    // 2026-11-24 (docs/DECISIONS.md): reemplaza a «Dividir a lo ancho / a lo alto» y «Maximizar» en la cabecera de cada panel. El boton principal agrega un
    // panel vacio a la DERECHA de este panel; la flechita abre un menu con las otras posiciones (extremos de toda la pantalla, o a cada lado de este panel).
    var MENU_AGREGAR_ANCHO_PX = 230;
    var FRACCION_QUE_CONSERVAN_LOS_EXISTENTES = 0.66;   // mismo valor que PROPORCION_PANEL_NUEVO_AL_COSTADO de pantalla-dividida-arbol.js
    var TITULO_AGREGAR = 'Agregar panel a la derecha de este';
    var TITULO_AGREGAR_MENU = 'Elegir dónde agregar el panel';
    var menuAgregar = null;        // <div> del menu de posiciones (uno solo, vive en el area: la cabecera de un panel recortaria lo que se salga)
    var menuAgregarPanelId = null; // panel al que pertenece el menu abierto

    function crearBotonesAgregar() {
        var grupo = document.createElement('span');
        grupo.className = 'pd-agregar';
        var principal = document.createElement('button');
        principal.type = 'button';
        principal.setAttribute('data-accion', 'agregar');
        principal.title = TITULO_AGREGAR;
        principal.setAttribute('aria-label', 'Agregar panel');
        // El texto se oculta por CSS cuando el panel es angosto (container query de Index.cshtml): queda solo el «+» con su tooltip.
        principal.innerHTML = '<i class="fas fa-plus" aria-hidden="true"></i><span class="pd-agregar-texto">Agregar panel</span>';
        var flecha = document.createElement('button');
        flecha.type = 'button';
        flecha.setAttribute('data-accion', 'agregar-menu');
        flecha.title = TITULO_AGREGAR_MENU;
        flecha.setAttribute('aria-label', TITULO_AGREGAR_MENU);
        flecha.setAttribute('aria-haspopup', 'menu');
        flecha.setAttribute('aria-expanded', 'false');
        flecha.innerHTML = '<i class="fas fa-chevron-down" aria-hidden="true"></i>';
        grupo.appendChild(principal);
        grupo.appendChild(flecha);
        return grupo;
    }

    function hayLugarParaOtroPanel() { return Arbol.contarPaneles(raiz) < Arbol.MAX_PANELES; }

    // Agrega un panel vacio y lo deja activo. modo: 'derecha' | 'izquierda' (pegado a `id`) o 'extremo-izquierdo' | 'extremo-derecho' (de toda la pantalla).
    function agregarPanel(id, modo) {
        if (!hayLugarParaOtroPanel()) {
            avisar('Máximo ' + Arbol.MAX_PANELES + ' paneles. Cerrá uno para agregar otro.', 'warning');
            return;
        }
        var idNuevo = Arbol.siguienteId(raiz);
        var resultado = null;
        if (modo === 'extremo-izquierdo' || modo === 'extremo-derecho') {
            // En un extremo todos los paneles existentes se achican (conservan FRACCION_QUE_CONSERVAN_LOS_EXISTENTES de su ancho): el mas angosto no
            // puede quedar bajo el minimo, y el panel nuevo (el resto del ancho) tampoco.
            var masAngosto = Arbol.calcularRectangulos(raiz).paneles.reduce(function (min, p) { return Math.min(min, p.w * area.clientWidth); }, Infinity);
            if (masAngosto * FRACCION_QUE_CONSERVAN_LOS_EXISTENTES < MIN_PANEL_PX || area.clientWidth * (1 - FRACCION_QUE_CONSERVAN_LOS_EXISTENTES) < MIN_PANEL_PX) {
                avisar('No hay lugar para otro panel en el extremo. Cerrá un panel o agregalo junto a uno ancho.', 'warning');
                return;
            }
            resultado = modo === 'extremo-izquierdo' ? Arbol.agregarAlExtremoIzquierdo(raiz, idNuevo, null) : Arbol.agregarAlCostado(raiz, idNuevo, null);
        } else {
            // Junto a un panel, ese panel cede la mitad de su ancho: si no queda lugar para dos, se avisa (los extremos no cortan: achican a todos parejo).
            if (tamanoPx(id).w / 2 < MIN_PANEL_PX) {
                avisar('No hay lugar para otro panel acá. Agrandá este panel o cerrá otro.', 'warning');
                return;
            }
            resultado = Arbol.agregarJunto(raiz, id, modo, idNuevo);
        }
        if (!resultado) return;
        raiz = resultado.raiz;
        activo = resultado.nuevoId;
        maximizado = null;
        dibujar();
    }

    // Un panel esta pegado al borde izquierdo / derecho si su rectangulo toca ese borde (con cuadros, los paneles del medio no lo tocan).
    function bordesDelPanel(id) {
        var rect = Arbol.calcularRectangulos(raiz).paneles.filter(function (p) { return p.id === id; })[0];
        if (!rect) return { izquierdo: false, derecho: false };
        return { izquierdo: rect.x < 0.001, derecho: rect.x + rect.w > 0.999 };
    }

    function cerrarMenuAgregar() {
        if (!menuAgregar || menuAgregar.hidden) return;
        menuAgregar.hidden = true;
        var panel = paneles[menuAgregarPanelId];
        var flecha = panel ? panel.el.querySelector('button[data-accion="agregar-menu"]') : null;
        if (flecha) flecha.setAttribute('aria-expanded', 'false');
        menuAgregarPanelId = null;
    }

    function alternarMenuAgregar(id, flecha) {
        if (menuAgregar && !menuAgregar.hidden && menuAgregarPanelId === id) { cerrarMenuAgregar(); return; }
        cerrarMenuAgregar();
        if (!hayLugarParaOtroPanel()) {
            avisar('Máximo ' + Arbol.MAX_PANELES + ' paneles. Cerrá uno para agregar otro.', 'warning');
            return;
        }
        if (!menuAgregar) {
            menuAgregar = document.createElement('div');
            menuAgregar.className = 'pd-menu-agregar';
            menuAgregar.setAttribute('role', 'menu');
            menuAgregar.hidden = true;
            area.appendChild(menuAgregar);
            menuAgregar.addEventListener('click', function (ev) {
                var opcion = ev.target.closest ? ev.target.closest('button[data-modo]') : null;
                if (!opcion) return;
                var panelId = menuAgregarPanelId;
                cerrarMenuAgregar();
                if (panelId !== null) agregarPanel(panelId, opcion.getAttribute('data-modo'));
            });
            // Esc cierra el menu y devuelve el foco a la flechita; el clic fuera (o dentro de un iframe, que hace perder el foco a esta pagina) tambien.
            document.addEventListener('keydown', function (ev) {
                if (ev.key !== 'Escape' || !menuAgregar || menuAgregar.hidden) return;
                var panel = paneles[menuAgregarPanelId];
                var boton = panel ? panel.el.querySelector('button[data-accion="agregar-menu"]') : null;
                cerrarMenuAgregar();
                if (boton) boton.focus();
            });
            document.addEventListener('mousedown', function (ev) {
                if (!menuAgregar || menuAgregar.hidden) return;
                if (menuAgregar.contains(ev.target) || (ev.target.closest && ev.target.closest('button[data-accion="agregar-menu"]'))) return;
                cerrarMenuAgregar();
            }, true);
            window.addEventListener('blur', cerrarMenuAgregar);
            window.addEventListener('resize', cerrarMenuAgregar);
        }

        // Opciones: las que no cambian nada se ocultan (un panel ya pegado al borde izquierdo no ofrece «extremo izquierdo»).
        var bordes = bordesDelPanel(id);
        var opciones = [];
        if (!bordes.izquierdo) opciones.push(['extremo-izquierdo', 'fa-angle-double-left', 'Al extremo izquierdo']);
        opciones.push(['izquierda', 'fa-arrow-left', 'A la izquierda de este panel']);
        opciones.push(['derecha', 'fa-arrow-right', 'A la derecha de este panel']);
        if (!bordes.derecho) opciones.push(['extremo-derecho', 'fa-angle-double-right', 'Al extremo derecho']);
        menuAgregar.innerHTML = '';
        var titulo = document.createElement('div');
        titulo.className = 'pd-menu-titulo';
        titulo.textContent = 'Agregar panel…';
        menuAgregar.appendChild(titulo);
        opciones.forEach(function (o) {
            var b = document.createElement('button');
            b.type = 'button';
            b.setAttribute('role', 'menuitem');
            b.setAttribute('data-modo', o[0]);
            b.innerHTML = '<i class="fas ' + o[1] + '" aria-hidden="true"></i>';
            b.appendChild(document.createTextNode(o[2]));
            menuAgregar.appendChild(b);
        });

        // Posicion: debajo de la flechita, alineado a su borde derecho y sin salirse del area.
        var caja = area.getBoundingClientRect();
        var f = flecha.getBoundingClientRect();
        var izquierda = Math.min(Math.max(f.right - caja.left - MENU_AGREGAR_ANCHO_PX, 4), Math.max(caja.width - MENU_AGREGAR_ANCHO_PX - 4, 4));
        menuAgregar.style.left = izquierda + 'px';
        menuAgregar.style.top = (f.bottom - caja.top + 2) + 'px';
        menuAgregar.hidden = false;
        menuAgregarPanelId = id;
        flecha.setAttribute('aria-expanded', 'true');
        var primera = menuAgregar.querySelector('button');
        if (primera) primera.focus();
    }

    // «Agregar panel» apagado (sin disabled de verdad: el tooltip explica y al tocarlo se avisa) cuando ya hay el maximo de paneles.
    function actualizarBotonesAgregar() {
        if (!raiz) return;
        var lleno = !hayLugarParaOtroPanel();
        Object.keys(paneles).forEach(function (k) {
            var panel = paneles[k];
            var principal = panel.el.querySelector('button[data-accion="agregar"]');
            var flecha = panel.el.querySelector('button[data-accion="agregar-menu"]');
            var motivo = 'Máximo ' + Arbol.MAX_PANELES + ' paneles: cerrá uno para agregar otro';
            if (principal) { principal.setAttribute('aria-disabled', lleno ? 'true' : 'false'); principal.title = lleno ? motivo : TITULO_AGREGAR; }
            if (flecha) { flecha.setAttribute('aria-disabled', lleno ? 'true' : 'false'); flecha.title = lleno ? motivo : TITULO_AGREGAR_MENU; }
        });
    }

    function cerrarPanel(id, confirmado) {
        var panel = paneles[id];
        if (!panel) return;
        if (!confirmado && tieneCambiosSinGuardar(panel)) {
            confirmarSalida(function () { cerrarPanel(id, true); });
            return;
        }
        var idNuevoSiQuedaVacio = Arbol.siguienteId(raiz);
        var nuevo = Arbol.cerrar(raiz, id);
        raiz = nuevo || Arbol.crearPanel(idNuevoSiQuedaVacio, null);   // el ultimo panel cerrado deja uno vacio: la pantalla nunca queda sin nada
        if (maximizado === id) maximizado = null;
        if (activo === id) activo = Arbol.primerPanel(raiz).id;
        dibujar();
        programarLiberar();
    }

    // Cierra el panel cuya pantalla es `ventana` (su contentWindow), sin pedir confirmacion. Lo usa documento-abierto.js cuando la pantalla de un
    // panel entrego su trabajo a otra (2026-11-21, «Seguir con ese acá»): ya no guarda nada ni protege la salida. Devuelve true si cerro un panel.
    function cerrarPanelDeVentana(ventana) {
        if (!iniciado || !ventana) return false;
        var ids = Object.keys(paneles);
        for (var i = 0; i < ids.length; i++) {
            var panel = paneles[ids[i]];
            var contenido = null;
            try { contenido = panel && panel.iframe ? panel.iframe.contentWindow : null; } catch (e) { contenido = null; }
            if (contenido && contenido === ventana) {
                cerrarPanel(Number(ids[i]), true);
                return true;
            }
        }
        return false;
    }

    // Abre una pantalla en un panel nuevo a la derecha (o en el primer panel vacio). Lo usa el clic derecho del menu lateral.
    function abrirEnPanelNuevo(url) {
        if (!iniciado) return;   // pantalla angosta: todavia no hay paneles
        if (!esUrlPermitida(url)) { avisar('Esa pantalla no se abre en un panel.', 'info'); return; }
        var vacio = Arbol.listarPaneles(raiz).filter(function (p) { return !p.url; })[0];
        if (vacio) { cargarEnPanel(vacio.id, url); return; }
        var resultado = Arbol.agregarAlCostado(raiz, Arbol.siguienteId(raiz), url);
        if (!resultado) {
            avisar('Máximo ' + Arbol.MAX_PANELES + ' paneles: la abrí en el panel activo.', 'warning');
            cargarEnPanel(activo, url);
            return;
        }
        raiz = resultado.raiz;
        activo = resultado.nuevoId;
        maximizado = null;
        dibujar();
    }

    // «Dividir pantalla» desde otra pantalla: ademas de cargar la pantalla de origen, queda un panel vacio y activo al costado para elegir con que
    // compararla (con su lista de modulos y buscador). Si ya hay un panel vacio no se agrega otro; con el maximo de paneles se avisa.
    function dejarPanelVacioParaElegir() {
        activoFijoHasta = Date.now() + ACTIVO_FIJO_MS;
        var hayVacio = Arbol.listarPaneles(raiz).some(function (p) { return !p.url; });
        if (hayVacio) {
            // El vacio que ya habia pasa a ser el activo para que el siguiente clic del menu lateral caiga ahi.
            activo = Arbol.listarPaneles(raiz).filter(function (p) { return !p.url; })[0].id;
            dibujar();
            return;
        }
        var resultado = Arbol.agregarAlCostado(raiz, Arbol.siguienteId(raiz), null);
        if (!resultado) { avisar('Ya hay ' + Arbol.MAX_PANELES + ' paneles: cerrá alguno para abrir otra pantalla al lado.', 'info'); return; }
        raiz = resultado.raiz;
        activo = resultado.nuevoId;
        maximizado = null;
        dibujar();
    }

    function abrirEnPanelActivo(url) { if (iniciado) cargarEnPanel(activo, url); }

    // Motivo por el que un atajo de `capacidad` paneles no se puede usar ahora, o null si se puede. 2026-11-24 (docs/DECISIONS.md): antes, con mas
    // pantallas abiertas que lugares (p. ej. 4 en cuadro -> 3 columnas), la distribucion nueva descartaba la sobrante EN SILENCIO (el panel desaparecia
    // con lo que tenia cargado). Ahora hay que cerrar paneles a mano primero.
    function motivoAtajoBloqueado(capacidad) {
        var abiertas = Arbol.contarConPantalla(raiz);   // los paneles vacios no cuentan: reacomodar no les quita nada
        if (abiertas <= capacidad) return null;
        var sobran = abiertas - capacidad;
        return 'Tenés ' + abiertas + ' paneles con pantallas abiertas y acá entran ' + capacidad + ': cerrá ' + (sobran === 1 ? 'un panel' : sobran + ' paneles') + ' para usar este atajo.';
    }

    // Deja «3 columnas» / «4 en cuadro» apagados (sin disabled de verdad, igual que «Cerrar pantalla dividida»: el motivo se ve en el tooltip y al tocarlo)
    // cuando no entran las pantallas abiertas.
    function actualizarBotonesDeAtajo() {
        if (!raiz) return;
        [['pdTres', CAPACIDAD_TRES, TITULO_TRES], ['pdCuadro', CAPACIDAD_CUADRO, TITULO_CUADRO]].forEach(function (def) {
            var boton = document.getElementById(def[0]);
            if (!boton) return;
            var motivo = motivoAtajoBloqueado(def[1]);
            boton.setAttribute('aria-disabled', motivo ? 'true' : 'false');
            boton.title = motivo || def[2];
        });
    }

    // 3 columnas / 4 en cuadro: reacomoda conservando lo que ya estaba abierto (en orden). Pide confirmar si algun panel tiene cambios sin guardar.
    // Si hay mas pantallas abiertas que `capacidad`, no hace nada y avisa (no se descarta ninguna).
    function aplicarDistribucion(fabrica, capacidad) {
        var motivo = motivoAtajoBloqueado(capacidad);
        if (motivo) { avisar(motivo, 'warning'); return; }
        var reacomodar = function () {
            // Los paneles en ventana independiente no se reacomodan (su pantalla sigue viva en su ventana): se conserva lo que esta en los paneles.
            var enVentanas = Arbol.listarPaneles(raiz).filter(function (p) { return p.ventana; }).length;
            var urls = Arbol.listarPaneles(raiz).filter(function (p) { return p.url && !p.ventana; }).map(function (p) { return p.url; });
            raiz = fabrica(urls);
            if (enVentanas > 0) avisar('Las pantallas en ventana independiente siguen abiertas en sus ventanas; ya no tienen panel acá.', 'info');
            activo = Arbol.primerPanel(raiz).id;
            maximizado = null;
            dibujar();
            programarLiberar();
        };
        if (hayCambiosEnAlgunPanel()) confirmarSalida(reacomodar);
        else reacomodar();
    }

    // ---------- dibujo ----------
    function porcentaje(fraccion) { return (fraccion * 100) + '%'; }

    // Pone al dia el DOM con el arbol: crea/quita paneles, los posiciona y sincroniza lo que cada uno muestra. NUNCA reubica un <iframe>.
    function dibujar() {
        cerrarMenuAgregar();   // cualquier cambio de paneles deja sin sentido un menu de posiciones abierto
        var lista = Arbol.listarPaneles(raiz);
        var ids = {};
        lista.forEach(function (n) { ids[n.id] = true; });

        // Paneles que ya no estan en el arbol.
        Object.keys(paneles).forEach(function (k) {
            if (!ids[k]) {
                var p = paneles[k];
                try { p.iframe.src = 'about:blank'; } catch (e) { /* se descarta igual */ }
                if (p.el.parentNode) p.el.parentNode.removeChild(p.el);
                delete paneles[k];
            }
        });
        // Paneles nuevos.
        lista.forEach(function (n) { if (!paneles[n.id]) paneles[n.id] = crearPanelDom(n.id); });

        if (!paneles[activo]) activo = lista[0].id;
        if (maximizado !== null && !paneles[maximizado]) maximizado = null;

        var geometria = Arbol.calcularRectangulos(raiz);
        geometria.paneles.forEach(function (r) {
            var panel = paneles[r.id];
            var esMaximizado = maximizado === r.id;
            var rect = esMaximizado ? { x: 0, y: 0, w: 1, h: 1 } : r;
            panel.el.style.left = porcentaje(rect.x);
            panel.el.style.top = porcentaje(rect.y);
            panel.el.style.width = porcentaje(rect.w);
            panel.el.style.height = porcentaje(rect.h);
            panel.el.classList.toggle('pd-maximizado', esMaximizado);
            panel.el.classList.toggle('pd-oculto', maximizado !== null && !esMaximizado);
            panel.el.classList.toggle('pd-activo', r.id === activo);
        });

        // Lo que muestra cada panel sigue a la url del arbol (si no cambio, no se recarga).
        lista.forEach(function (n) {
            var panel = paneles[n.id];
            if (n.ventana) { mostrarAvisoVentana(panel, n); return; }   // la pantalla vive en su ventana independiente: el panel solo muestra el aviso
            if (n.url && n.url !== panel.urlActual) mostrarPanelCon(panel, n.url);
            else if (!n.url && (panel.urlActual || (panel.vacio.hidden && panel.aviso.hidden))) mostrarPanelVacio(panel);
        });

        dibujarDivisores(maximizado !== null ? [] : geometria.divisores);
        document.getElementById('pdContador').textContent = lista.length + '/' + Arbol.MAX_PANELES + ' paneles';
        programarGuardado();
    }

    function dibujarDivisores(lista) {
        var vigentes = {};
        lista.forEach(function (d) { vigentes[d.ruta] = true; });
        Object.keys(divisores).forEach(function (ruta) {
            if (!vigentes[ruta]) {
                var el = divisores[ruta].el;
                if (el.parentNode) el.parentNode.removeChild(el);
                delete divisores[ruta];
            }
        });
        lista.forEach(function (d) {
            var entrada = divisores[d.ruta];
            if (!entrada) {
                entrada = { el: crearDivisor(d.ruta), info: d };
                divisores[d.ruta] = entrada;
                area.appendChild(entrada.el);
            }
            entrada.info = d;
            var el = entrada.el;
            el.className = 'pd-divisor ' + (d.direccion === 'fila' ? 'pd-fila' : 'pd-columna');
            el.setAttribute('aria-orientation', d.direccion === 'fila' ? 'vertical' : 'horizontal');
            if (d.direccion === 'fila') {
                el.style.left = 'calc(' + porcentaje(d.x) + ' - 4px)';
                el.style.top = porcentaje(d.y);
                el.style.height = porcentaje(d.h);
                el.style.width = '';
                el.style.right = '';
            } else {
                el.style.top = 'calc(' + porcentaje(d.y) + ' - 4px)';
                el.style.left = porcentaje(d.x);
                el.style.width = porcentaje(d.w);
                el.style.height = '';
            }
        });
    }

    // Proporcion nueva de una division segun donde esta el puntero, respetando el minimo en pixeles de cada lado.
    function proporcionPara(info, clienteX, clienteY) {
        var caja = area.getBoundingClientRect();
        var padre = info.padre;
        var fila = info.direccion === 'fila';
        var inicio = fila ? caja.left + padre.x * caja.width : caja.top + padre.y * caja.height;
        var largo = fila ? padre.w * caja.width : padre.h * caja.height;
        if (largo <= 0) return 0.5;
        var proporcion = ((fila ? clienteX : clienteY) - inicio) / largo;
        var minimo = MIN_PANEL_PX / largo;
        if (minimo < 0.5) proporcion = Math.min(1 - minimo, Math.max(minimo, proporcion));   // area chica (minimo >= 50 %): solo rige el tope del arbol
        return proporcion;
    }

    function moverDivisor(ruta, proporcion) {
        raiz = Arbol.cambiarProporcion(raiz, ruta, proporcion);
        dibujar();
    }

    function crearDivisor(ruta) {
        var el = document.createElement('div');
        el.setAttribute('role', 'separator');
        el.tabIndex = 0;
        el.setAttribute('aria-label', 'Cambiar el tamaño de los paneles (flechas del teclado o arrastrar)');
        el.addEventListener('pointerdown', function (ev) {
            ev.preventDefault();
            var info = divisores[ruta] ? divisores[ruta].info : null;
            if (!info) return;
            try { el.setPointerCapture(ev.pointerId); } catch (e) { /* sin captura: sigue funcionando con el area */ }
            area.classList.add('pd-arrastrando');   // los iframes dejan de tragarse el puntero mientras se arrastra
            el.classList.add('pd-activo');
            var mover = function (e) { moverDivisor(ruta, proporcionPara(info, e.clientX, e.clientY)); };
            var soltar = function () {
                el.removeEventListener('pointermove', mover);
                el.removeEventListener('pointerup', soltar);
                el.removeEventListener('pointercancel', soltar);
                area.classList.remove('pd-arrastrando');
                el.classList.remove('pd-activo');
            };
            el.addEventListener('pointermove', mover);
            el.addEventListener('pointerup', soltar);
            el.addEventListener('pointercancel', soltar);
        });
        el.addEventListener('keydown', function (ev) {
            var info = divisores[ruta] ? divisores[ruta].info : null;
            if (!info) return;
            var fila = info.direccion === 'fila';
            var menos = fila ? 'ArrowLeft' : 'ArrowUp';
            var mas = fila ? 'ArrowRight' : 'ArrowDown';
            if (ev.key !== menos && ev.key !== mas) return;
            ev.preventDefault();
            // La proporcion vigente se lee del arbol (el divisor no la guarda).
            var nodo = raiz;
            for (var i = 0; i < ruta.length && nodo; i++) nodo = ruta.charAt(i) === 'a' ? nodo.a : nodo.b;
            if (!nodo || nodo.tipo !== 'division') return;
            moverDivisor(ruta, nodo.proporcion + (ev.key === mas ? PASO_TECLADO : -PASO_TECLADO));
        });
        return el;
    }

    function ajustarAlto() {
        var arriba = area.getBoundingClientRect().top + window.pageYOffset;
        area.style.height = Math.max(ALTO_MINIMO_AREA_PX, window.innerHeight - arriba - MARGEN_INFERIOR_PX) + 'px';
    }

    // ---------- cerrar la pantalla dividida ----------
    // Solo se puede con UN panel que tenga una pantalla cargada (ni vacio, ni rechazado, ni pasado a una ventana independiente): esa pantalla
    // pasa al layout normal, entera. Devuelve {ok:true, url, panel} o {ok:false, motivo} (el motivo se le muestra al usuario).
    function panelUnicoCerrable() {
        var lista = Arbol.listarPaneles(raiz);
        if (lista.length !== 1) return { ok: false, motivo: 'Cerrá o juntá los paneles hasta que quede uno solo con una pantalla.' };
        var nodo = lista[0];
        var panel = paneles[nodo.id];
        if (nodo.ventana) return { ok: false, motivo: 'La pantalla está en una ventana independiente: traela de vuelta al panel para cerrar la pantalla dividida.' };
        // Un unico panel vacio: no hay pantalla que llevar al layout, se cierra yendo al dashboard.
        if (panel && !panel.urlActual) return { ok: true, url: URL_INICIO, panel: panel, vacio: true };
        if (!panel || !panel.aviso.hidden) return { ok: false, motivo: 'El panel tiene un aviso: elegí otra pantalla para llevarla al layout.' };
        return { ok: true, url: panel.urlActual, panel: panel };
    }

    function actualizarBotonCerrar() {
        var boton = document.getElementById('pdCerrar');
        if (!boton || !raiz) return;
        var r = panelUnicoCerrable();
        boton.setAttribute('aria-disabled', r.ok ? 'false' : 'true');
        boton.title = r.ok ? (r.vacio ? 'Salir de la pantalla dividida e ir al inicio' : 'Ver esta pantalla entera, sin dividir') : r.motivo;
    }

    // «Cerrar pantalla dividida»: la pantalla del panel unico se abre entera en el layout normal (misma pestaña) y la distribucion guardada se borra,
    // porque se cerro a proposito. Con cambios sin guardar pide confirmar: la pagina se vuelve a cargar.
    function cerrarPantallaDividida(confirmado) {
        var r = panelUnicoCerrable();
        if (!r.ok) { avisar(r.motivo, 'info'); return; }
        // Una carga que sabe pasar lo cargado sigue entera en el layout con todo; si no, con cambios sin guardar se pide confirmar.
        var traspaso = (confirmado || r.vacio) ? null : prepararTraspasoDe(r.panel.iframe.contentWindow);
        if (!traspaso && !confirmado && !r.vacio && tieneCambiosSinGuardar(r.panel)) {
            confirmarSalida(function () { cerrarPantallaDividida(true); });
            return;
        }
        if (traspaso) {
            r.url = traspaso.urlCon(r.url);
            traspaso.ceder();
        }
        saliendo = true;
        if (temporizadorGuardado) { window.clearTimeout(temporizadorGuardado); temporizadorGuardado = null; }
        try { window.localStorage.removeItem(CLAVE_GUARDADO); } catch (e) { console.warn('Pantalla dividida: no se pudo borrar la distribución guardada.', e); }
        // El cartel del navegador de la pantalla del panel preguntaria de nuevo al salir: el usuario ya confirmo (o no habia nada sin guardar).
        try { r.panel.iframe.contentWindow.desactivarProteccionSalida(); } catch (e) { /* la pantalla no tiene proteccion de salida */ }
        window.location.assign(r.url);
    }

    // ---------- pasar TODA la pantalla dividida a una ventana independiente ----------
    // «Pasar a ventana independiente» (2026-11-21, docs/DECISIONS.md): MUEVE toda la distribucion (todos los paneles, sus tamaños) a una ventana nueva
    // sin menu ni barra, y esta pestaña vuelve a Inicio. Cada panel con una carga nueva que sabe pasar lo cargado (DocumentoAbierto.prepararTraspaso)
    // viaja con todo y sin avisos; si algun panel tiene cambios sin guardar que NO puede viajar, se pide confirmar UNA vez («Salir sin guardar»).
    // Los paneles cuya pantalla ya esta en su propia ventana independiente siguen apuntando a esa ventana (se la encuentra por nombre).
    // La distribucion viaja por una clave temporal de localStorage (?desdeLayout=) que la ventana lee una sola vez.
    function descartarTraspasos(traspasos) {
        Object.keys(traspasos).forEach(function (id) {
            try { traspasos[id].descartar(); } catch (e) { console.warn('Pantalla dividida: no se pudo descartar un traspaso.', e); }
        });
    }

    function independizarTodo(confirmado) {
        if (EN_VENTANA_DIVIDIDA || !iniciado) return;
        var lista = Arbol.listarPaneles(raiz);
        if (!lista.some(function (n) { return !!n.url; })) { avisar('No hay nada para pasar: abrí alguna pantalla en un panel.', 'info'); return; }

        // Lo cargado de cada panel que sabe pasarlo; y si algun panel con cambios sin guardar NO puede, hay que confirmar antes de soltar nada.
        var traspasos = {};
        var hayCambiosSinTraspaso = false;
        lista.forEach(function (nodo) {
            var panel = paneles[nodo.id];
            if (nodo.ventana || !panel || !panel.urlActual) return;
            var traspaso = prepararTraspasoDe(panel.iframe.contentWindow);
            if (traspaso) traspasos[nodo.id] = traspaso;
            else if (tieneCambiosSinGuardar(panel)) hayCambiosSinTraspaso = true;
        });
        if (hayCambiosSinTraspaso && !confirmado) {
            descartarTraspasos(traspasos);
            confirmarSalida(function () { independizarTodo(true); });
            return;
        }

        // La ventana se abre dentro del clic (o del «Sí, salir»): el navegador la permite. Se ubica donde estaba el area de paneles.
        var ventana = window.open('', NOMBRE_VENTANA_DIVIDIDA, caracteristicasVentana({ el: document.getElementById('pdArea') }));
        if (!ventana) { descartarTraspasos(traspasos); avisarVentanaBloqueada(); return; }
        if (paginaCargada(ventana)) {
            descartarTraspasos(traspasos);
            try { ventana.focus(); } catch (e) { console.warn('Pantalla dividida: no se pudo traer la ventana al frente.', e); }
            avisar('Ya hay una pantalla dividida en una ventana independiente: la traje al frente. Cerrala antes de pasar esta.', 'warning');
            return;
        }

        // Distribucion con la url de cada panel (+ la marca de su traspaso) en una clave temporal.
        var mapeado = Arbol.mapearUrls(raiz, function (nodo) {
            var panel = paneles[nodo.id];
            var url = !nodo.ventana && panel && panel.urlActual ? panel.urlActual : nodo.url;
            return traspasos[nodo.id] ? traspasos[nodo.id].urlCon(url) : url;
        });
        var clave = PREFIJO_LAYOUT_TRASPASO + Date.now() + '-' + Math.random().toString(36).slice(2, 8);
        try {
            window.localStorage.setItem(clave, Arbol.serializar(mapeado, activo));
        } catch (e) {
            console.warn('Pantalla dividida: no se pudo guardar la distribución para la ventana (localStorage).', e);
            descartarTraspasos(traspasos);
            try { ventana.close(); } catch (e2) { console.warn('Pantalla dividida: no se pudo cerrar la ventana en blanco.', e2); }
            avisar('No se pudo pasar la pantalla dividida a una ventana (el navegador no deja guardar la distribución).', 'warning');
            return;
        }

        // Se suelta lo de esta pestaña (los paneles dejan de guardar borrador y de proteger la salida) y recien entonces se carga la ventana.
        Object.keys(traspasos).forEach(function (id) { traspasos[id].ceder(); });
        saliendo = true;
        if (temporizadorGuardado) { window.clearTimeout(temporizadorGuardado); temporizadorGuardado = null; }
        try { window.localStorage.removeItem(CLAVE_GUARDADO); } catch (e) { console.warn('Pantalla dividida: no se pudo borrar la distribución guardada.', e); }
        lista.forEach(function (nodo) {
            var panel = paneles[nodo.id];
            try { if (panel) panel.iframe.contentWindow.desactivarProteccionSalida(); } catch (e) { /* la pantalla no tiene proteccion de salida */ }
        });
        try {
            ventana.location.href = window.location.origin + window.location.pathname + '?' + PARAM_DESDE_LAYOUT + '=' + encodeURIComponent(clave);
            ventana.focus();
        } catch (e) {
            console.warn('Pantalla dividida: no se pudo cargar la ventana independiente.', e);
        }
        window.location.assign(URL_INICIO);   // la distribucion se MOVIO: esta pestaña vuelve a Inicio
    }

    // Lee (una sola vez) la distribucion que trae ?desdeLayout=<clave>. null si no viene, vencio o no es valida (se arranca como siempre).
    function leerLayoutTraspasado() {
        var params = new URLSearchParams(window.location.search);
        var clave = params.get(PARAM_DESDE_LAYOUT) || '';
        if (!clave) return null;
        if (window.history && window.history.replaceState) window.history.replaceState(null, '', window.location.pathname);
        if (clave.indexOf(PREFIJO_LAYOUT_TRASPASO) !== 0) return null;
        var texto = null;
        try {
            texto = window.localStorage.getItem(clave);
            window.localStorage.removeItem(clave);   // se usa una sola vez
        } catch (e) {
            console.warn('Pantalla dividida: no se pudo leer la distribución traída de la pestaña.', e);
            return null;
        }
        var creado = parseInt(clave.substring(PREFIJO_LAYOUT_TRASPASO.length).split('-')[0], 10);
        if (!texto || !creado || Date.now() - creado > VIGENCIA_LAYOUT_MS) return null;
        var leido = Arbol.deserializar(texto, esUrlPermitida);
        if (!leido.ok) {
            console.warn('Pantalla dividida: se ignoró la distribución traída de la pestaña (' + leido.motivo + ').');
            return null;
        }
        return leido;
    }

    // ---------- guardado local ----------
    function programarGuardado() {
        if (!iniciado) return;
        actualizarBotonCerrar();   // todo cambio de paneles o de pantallas cargadas pasa por aca: el boton de cerrar sigue al estado
        actualizarBotonesAtras();     // «Atrás» de cada panel se apaga si el panel ya es el menu
        actualizarBotonesAgregar();   // y «Agregar panel» con el maximo de paneles
        actualizarFranjasDeEdicion(); // y la franja de «venta en edicion» sigue a la pantalla que muestra cada panel
        actualizarBotonesDeAtajo();   // y los atajos de 3 / 4 paneles se apagan si no entran las pantallas abiertas
        if (temporizadorGuardado) window.clearTimeout(temporizadorGuardado);
        temporizadorGuardado = window.setTimeout(guardar, GUARDADO_DEMORA_MS);
    }

    function guardar() {
        temporizadorGuardado = null;
        if (!raiz || saliendo) return;
        try {
            window.localStorage.setItem(CLAVE_GUARDADO, Arbol.serializar(raiz, activo));
        } catch (e) {
            console.warn('Pantalla dividida: no se pudo guardar la distribución en este equipo.', e);
        }
    }

    function restaurar() {
        var texto = null;
        try { texto = window.localStorage.getItem(CLAVE_GUARDADO); } catch (e) { console.warn('Pantalla dividida: no se pudo leer lo guardado.', e); }
        if (texto === null) return null;
        var leido = Arbol.deserializar(texto, esUrlPermitida);
        if (!leido.ok) {
            console.warn('Pantalla dividida: se ignoró lo guardado (' + leido.motivo + ').');
            return null;
        }
        return leido;
    }

    // ---------- permisos ----------
    function programarLiberar() {
        if (!iniciado || !URL_LIBERAR) return;
        if (temporizadorLiberar) window.clearTimeout(temporizadorLiberar);
        temporizadorLiberar = window.setTimeout(liberarPermisos, LIBERAR_DEMORA_MS);
    }

    // Avisa al servidor que pantallas siguen abiertas: el operador autorizado de las que ya no lo estan se pierde (PermisosHelper.LimpiarOperadoresFueraDe).
    function liberarPermisos() {
        temporizadorLiberar = null;
        var campo = document.querySelector('#pdAntiForgery input[name="__RequestVerificationToken"]');
        var rutas = urlsParaPermisos();
        fetch(URL_LIBERAR, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': campo ? campo.value : '' },
            body: JSON.stringify({ rutas: rutas })
        }).then(function (respuesta) {
            if (!respuesta.ok) console.warn('Pantalla dividida: el servidor no pudo actualizar los permisos (HTTP ' + respuesta.status + ').');
        }).catch(function (e) {
            console.warn('Pantalla dividida: no se pudo avisar al servidor qué pantallas siguen abiertas.', e);
        });
    }

    // ---------- menu lateral ----------
    // Un clic en un enlace del menu carga ese modulo en el panel activo (sin salir de esta pantalla). Ctrl/Shift/rueda, Inicio, el POS y los
    // encabezados desplegables conservan su comportamiento normal.
    function alClicEnMenu(ev) {
        if (!esEscritorio() || ev.defaultPrevented || ev.button !== 0 || ev.ctrlKey || ev.metaKey || ev.shiftKey || ev.altKey) return;
        var enlace = ev.target.closest ? ev.target.closest('a[href]') : null;
        if (!enlace) return;
        var href = enlace.getAttribute('href') || '';
        if (href === '' || href.charAt(0) === '#' || enlace.getAttribute('data-bs-toggle') || enlace.hasAttribute('data-pos-unico')
            || enlace.target === '_blank' || enlace.hasAttribute('download') || enlace.classList.contains('sidebar-brand')) return;
        var ruta = rutaRelativa(enlace.href);
        // El item «Pantalla dividida» del propio menu: ya estamos aca, navegar recargaria la pagina (y los paneles) para nada. 2026-11-24
        // (docs/DECISIONS.md): se ignora el clic. Con Ctrl / clic del medio (nueva pestaña) no llega aca: el navegador lo maneja.
        if (ruta && RUTA_PANTALLA_DIVIDIDA.test(rutaSinBase(ruta.split('?')[0]))) {
            ev.preventDefault();
            ev.stopPropagation();
            return;
        }
        if (!ruta || !esUrlPermitida(ruta)) return;   // otro sitio, el POS, esta misma pantalla...
        if (esInicio(ruta)) return;   // Inicio: sale de la pantalla dividida

        ev.preventDefault();
        ev.stopPropagation();
        abrirEnPanelActivo(ruta);
    }

    // ---------- arranque ----------
    function iniciar() {
        if (iniciado) return;
        iniciado = true;
        construirCatalogo();
        ajustarAlto();

        // En la ventana independiente: primero la distribucion que viene de la pestaña (?desdeLayout=), si no la que esa ventana guardo antes.
        var traido = leerLayoutTraspasado();
        var guardado = traido || restaurar();
        if (guardado) {
            raiz = guardado.raiz;
            activo = guardado.activo;
        } else {
            raiz = Arbol.crearPanel(1, null);
            activo = 1;
        }
        // Fotos de lo cargado (capa 2): solo una RECARGA de esta pagina devuelve a cada panel lo que tenia; entrar de nuevo por un enlace, o abrir la
        // ventana con lo que viene de la pestaña, parte de cero y borra las fotos viejas (el usuario decidio salir).
        if (!traido && esRecargaDePagina()) {
            fotos = leerFotos();
            raiz = conFotosDevueltas(raiz);
        } else {
            borrarFotos();
        }
        dibujar();

        var menu = document.getElementById('accordionSidebar');
        if (menu) menu.addEventListener('click', alClicEnMenu, true);
        document.getElementById('pdTres').addEventListener('click', function () { aplicarDistribucion(Arbol.distribucionTres, CAPACIDAD_TRES); });
        document.getElementById('pdCuadro').addEventListener('click', function () { aplicarDistribucion(Arbol.distribucionCuadro, CAPACIDAD_CUADRO); });
        var botonCerrar = document.getElementById('pdCerrar');
        if (botonCerrar) botonCerrar.addEventListener('click', function () { cerrarPantallaDividida(false); });
        // «Pasar a ventana independiente»: no tiene sentido estando ya en la ventana.
        var botonIndependizar = document.getElementById('pdIndependizar');
        if (botonIndependizar) {
            if (EN_VENTANA_DIVIDIDA) botonIndependizar.hidden = true;
            else botonIndependizar.addEventListener('click', function () { independizarTodo(false); });
        }
        window.addEventListener('resize', function () { ajustarAlto(); });
        window.addEventListener('keydown', alTeclaDeRecarga, true);   // F5 / Ctrl+R con el foco en esta pagina (los paneles lo engancha alCargarMarco)
        window.addEventListener('pagehide', guardar);
        window.setInterval(revisarVentanas, REVISION_VENTANAS_MS);
        // Fotos de lo cargado (capa 2): cada pocos segundos y justo antes de salir (con los iframes todavia vivos). Si el usuario cancela el cartel
        // nativo y se queda, solo se refrescan las fotos.
        window.setInterval(tomarFotos, FOTOS_INTERVALO_MS);
        window.addEventListener('beforeunload', tomarFotos);
        // Tocar un iframe saca el foco de esta pagina: el iframe enfocado es el panel activo.
        window.addEventListener('blur', function () {
            // Recien llegada con «Dividir pantalla»: la pantalla cargada enfoca sus campos al arrancar y eso NO es una eleccion del usuario;
            // durante un rato el panel vacio sigue siendo el activo (si no, el menu lateral cargaria sobre la pantalla que se quiere conservar).
            if (activoFijoHasta && Date.now() < activoFijoHasta) return;
            activoFijoHasta = 0;
            var el = document.activeElement;
            if (el && el.tagName === 'IFRAME' && el.name && el.name.indexOf(PREFIJO_FRAME) === 0) activar(Number(el.name.substring(PREFIJO_FRAME.length)));
        });

        // «Abrir en pantalla dividida» desde otra pantalla: llega con ?abrir=<ruta>.
        var abrir = new URLSearchParams(window.location.search).get(PARAM_ABRIR);
        if (abrir) {
            if (window.history && window.history.replaceState) window.history.replaceState(null, '', window.location.pathname);
            abrirEnPanelNuevo(abrir);
            dejarPanelVacioParaElegir();
        }
        programarLiberar();
    }

    // API para menu-ventana-independiente.js (clic derecho en el menu lateral).
    // edicionDeVentaTerminada: la usa el POS de una ventana abierta desde un panel para editar una venta (forma-pago.js: window.PosEdicionDesdePanel).
    window.PantallaDividida = {
        abrirEnPanelNuevo: abrirEnPanelNuevo, abrirEnPanelActivo: abrirEnPanelActivo, esUrlPermitida: esUrlPermitida,
        cerrarPanelDeVentana: cerrarPanelDeVentana, edicionDeVentaTerminada: edicionDeVentaTerminada
    };

    // Por debajo de 992 px la pantalla solo muestra un aviso: los paneles se arman recien si la ventana se agranda.
    if (esEscritorio()) {
        iniciar();
    } else if (window.matchMedia) {
        var consulta = window.matchMedia('(min-width: ' + ESCRITORIO_ANCHO_MIN_PX + 'px)');
        var alCambiar = function (e) {
            if (e.matches) {
                iniciar();
                if (consulta.removeEventListener) consulta.removeEventListener('change', alCambiar);
            }
        };
        if (consulta.addEventListener) consulta.addEventListener('change', alCambiar);
    }
})();
