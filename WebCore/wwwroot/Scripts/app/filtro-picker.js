// Selector multi-seleccion para los filtros Cliente / Vendedor / Proveedor (Ventas Indice, Facturas,
// Lineas y Compras Indice, Lineas). Convierte un <input type="text"> de filtro existente en:
//   [ input ][ x limpiar ][ lupa ]  + etiqueta chica debajo con la identificacion
// - Sin seleccion: el input es de texto libre y el filtro busca por coincidencia de palabras (como siempre).
// - Con seleccion (modal con checkboxes, F9 / F10): el input queda de SOLO LECTURA y el filtro es EXACTO por ID.
// - "Limpiar": borra la seleccion, habilita el input y vuelve a la busqueda por texto.
//
// Uso: <input data-filtro-picker="cliente|vendedor|proveedor" data-filtro-picker-tecla="F10"
//             data-ids-name="idsCliente" data-ids="1,2" data-items='[{"id":1,"nombre":"..","sub":".."}]'>
//  - data-ids-name (opcional): crea un <input type="hidden" name="..."> con los IDs, para forms GET nativos.
//  - data-ids / data-items (opcional): seleccion inicial (el servidor la resuelve desde la URL). Si viene
//    data-ids sin data-items, los nombres se piden al endpoint "resolver".
// Contrato con las pantallas (para no pisarse con sus listeners existentes):
//  - input.dataset.ids = CSV de IDs seleccionados ("" si no hay): cada pantalla filtra por esto si no esta vacio.
//  - Al cambiar la seleccion se dispara un evento "input" (bubbles) en el input, asi los listeners que ya
//    escuchaban "input" para el texto libre siguen funcionando, y ademas "filtropicker:change".
// API publica: window.FiltroPicker { getIds, getTexto, getItems, setItems, limpiar }.
// El modal (#modalFiltroPicker) lo aporta Views/Shared/_FiltroPickerModal.cshtml, que tambien trae las URLs
// de los endpoints (FiltroPickerController) en data-url-*. Detalle: docs/DECISIONS.md (2026-10-13).
(function () {
    'use strict';

    // Configuracion por tipo de filtro. "fuente" elige el endpoint (personas = clientes y proveedores, que son
    // la misma tabla; vendedores = usuarios).
    var TIPOS = {
        cliente: { fuente: 'personas', singular: 'cliente', plural: 'clientes', titulo: 'Buscar clientes', colNombre: 'Razón social', colSub: 'Identificación', colExtra: 'CUIT', placeholder: 'Escribí para buscar por razón social, identificación o CUIT' },
        proveedor: { fuente: 'personas', singular: 'proveedor', plural: 'proveedores', titulo: 'Buscar proveedores', colNombre: 'Razón social', colSub: 'Identificación', colExtra: 'CUIT', placeholder: 'Escribí para buscar por razón social, identificación o CUIT' },
        vendedor: { fuente: 'vendedores', singular: 'vendedor', plural: 'vendedores', titulo: 'Buscar vendedores', colNombre: 'Nombre', colSub: 'Usuario', colExtra: '', placeholder: 'Escribí para buscar por nombre o usuario' }
    };

    // Espera tras tipear antes de pedir al servidor (mismo orden de magnitud que compras.js).
    var DEBOUNCE_BUSQUEDA_MS = 200;

    // Filas visibles con Alt+1..9 (las de la columna "#").
    var MAX_ATAJOS_ALT = 9;

    var estados = [];          // un estado por input mejorado
    var modalEl = null;        // #modalFiltroPicker (se resuelve al abrir por primera vez)
    var modalUi = null;        // referencias a los elementos del modal
    var sesion = null;         // sesion de seleccion en curso { estado, seleccion: Map(id -> item), token, timer, items }

    // ---------------------------------------------------------------- utilidades

    function esc(valor) {
        return String(valor == null ? '' : valor)
            .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
    }

    function normalizarItem(raw) {
        return {
            id: Number(raw && raw.id) || 0,
            nombre: String((raw && raw.nombre) || ''),
            sub: String((raw && raw.sub) || ''),
            cuit: String((raw && raw.cuit) || ''),
            reservada: !!(raw && raw.reservada),
            inactivo: !!(raw && raw.inactivo)
        };
    }

    function dispararCambio(input) {
        // "input" para los listeners de texto libre ya existentes; "filtropicker:change" para quien quiera distinguir.
        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.dispatchEvent(new CustomEvent('filtropicker:change', { bubbles: true }));
    }

    function buscarEstado(input) {
        for (var i = 0; i < estados.length; i++) {
            if (estados[i].input === input) return estados[i];
        }
        return null;
    }

    function pedirJson(url, params) {
        var qs = new URLSearchParams(params).toString();
        // silentLoading: no mostrar el loader global en cada tecla (opcion que ya usan otras pantallas).
        return fetch(url + (qs ? '?' + qs : ''), { credentials: 'same-origin', silentLoading: true })
            .then(function (r) {
                if (!r.ok) throw new Error('HTTP ' + r.status);
                return r.json();
            });
    }

    // ---------------------------------------------------------------- input mejorado

    function mejorarInput(input) {
        if (input.dataset.filtroPickerListo === '1') return;

        var cfg = TIPOS[input.dataset.filtroPicker];
        if (!cfg) {
            console.error('filtro-picker: tipo desconocido "' + input.dataset.filtroPicker + '"');
            return;
        }
        input.dataset.filtroPickerListo = '1';

        var tecla = (input.dataset.filtroPickerTecla || '').toUpperCase();

        // Envoltorio: input-group con [input][limpiar][lupa] + etiqueta chica debajo.
        var wrapper = document.createElement('div');
        wrapper.className = 'filtro-picker';
        input.parentNode.insertBefore(wrapper, input);

        var grupo = document.createElement('div');
        grupo.className = 'input-group filtro-picker-group';
        wrapper.appendChild(grupo);
        grupo.appendChild(input);

        var btnLimpiar = document.createElement('button');
        btnLimpiar.type = 'button';
        btnLimpiar.className = 'btn filtro-picker-btn filtro-picker-limpiar';
        btnLimpiar.title = 'Limpiar (vuelve a buscar por texto)';
        btnLimpiar.setAttribute('aria-label', 'Limpiar ' + cfg.singular);
        btnLimpiar.innerHTML = '<i class="fas fa-times"></i>';
        grupo.appendChild(btnLimpiar);

        var btnBuscar = document.createElement('button');
        btnBuscar.type = 'button';
        btnBuscar.className = 'btn filtro-picker-btn filtro-picker-buscar';
        btnBuscar.title = cfg.titulo + (tecla ? ' (' + tecla + ')' : '');
        btnBuscar.setAttribute('aria-label', cfg.titulo);
        btnBuscar.innerHTML = '<i class="fas fa-search"></i>';
        grupo.appendChild(btnBuscar);

        var sub = document.createElement('small');
        sub.className = 'filtro-picker-sub d-none';
        wrapper.appendChild(sub);

        // Hidden con los IDs para los forms GET nativos (la pantalla decide el nombre del parametro).
        var hidden = null;
        if (input.dataset.idsName) {
            hidden = document.createElement('input');
            hidden.type = 'hidden';
            hidden.name = input.dataset.idsName;
            wrapper.appendChild(hidden);
        }

        // El texto libre ya no viaja en el form si hay seleccion (el input muestra un resumen, no un filtro):
        // se guarda el name original y se quita mientras haya seleccion.
        var nameOriginal = input.getAttribute('name');

        var estado = {
            input: input, cfg: cfg, tipo: input.dataset.filtroPicker, tecla: tecla,
            btnBuscar: btnBuscar, btnLimpiar: btnLimpiar, sub: sub, hidden: hidden,
            nameOriginal: nameOriginal, items: []
        };
        estados.push(estado);

        btnBuscar.addEventListener('click', function () { abrirModal(estado); });
        btnLimpiar.addEventListener('click', function () { limpiarEstado(estado, true, true); });

        hidratarInicial(estado);
    }

    // Seleccion inicial que trae el servidor (desde la URL): data-items (JSON) o, si solo hay data-ids, se resuelve por endpoint.
    function hidratarInicial(estado) {
        var input = estado.input;
        var items = [];

        if (input.dataset.items) {
            try {
                items = JSON.parse(input.dataset.items).map(normalizarItem).filter(function (x) { return x.id > 0; });
            } catch (err) {
                console.error('filtro-picker: data-items invalido', err);
            }
        }

        if (items.length > 0) {
            aplicarItems(estado, items);
            return;
        }

        var ids = (input.dataset.ids || '').split(',').map(function (x) { return x.trim(); }).filter(Boolean);
        if (ids.length > 0) {
            resolverIds(estado, ids.join(','));
        } else {
            pintar(estado);
        }
    }

    // Pide al servidor los nombres de unos IDs (cuando una pantalla restaura solo IDs). No dispara "input":
    // es solo pintar lo que la pantalla ya filtro.
    function resolverIds(estado, idsCsv) {
        var url = modalUrl('resolver');
        if (!url) {
            console.error('filtro-picker: falta el modal (_FiltroPickerModal) en la pagina');
            return;
        }

        // Mientras llegan los nombres el input ya se ve bloqueado con los IDs (el filtro ya esta aplicado).
        estado.input.dataset.ids = idsCsv;
        estado.input.readOnly = true;

        pedirJson(url, { tipo: estado.tipo, ids: idsCsv })
            .then(function (resp) {
                var items = ((resp && resp.items) || []).map(normalizarItem).filter(function (x) { return x.id > 0; });
                if (items.length === 0) {
                    // Los IDs ya no existen: se limpia en silencio el estado visual, el filtro por esos IDs quedaria vacio.
                    limpiarEstado(estado, true, false);
                    return;
                }
                aplicarItems(estado, items);
            })
            .catch(function (err) {
                console.error('filtro-picker: no se pudieron resolver los IDs', err);
                limpiarEstado(estado, false, false);
            });
    }

    // Fija la seleccion (sin disparar eventos) y repinta.
    function aplicarItems(estado, items) {
        estado.items = items.slice();
        pintar(estado);
    }

    // Refleja el estado en el DOM: input readonly + resumen, etiqueta debajo, hidden, dataset.ids.
    function pintar(estado) {
        var input = estado.input;
        var items = estado.items;
        var n = items.length;
        var ids = items.map(function (x) { return x.id; }).join(',');

        input.dataset.ids = ids;
        if (estado.hidden) estado.hidden.value = ids;

        if (n === 0) {
            input.readOnly = false;
            input.classList.remove('filtro-picker-seleccionado');
            input.removeAttribute('title');
            if (estado.nameOriginal) input.setAttribute('name', estado.nameOriginal);
            estado.sub.textContent = '';
            estado.sub.removeAttribute('title');
            estado.sub.classList.add('d-none');
            return;
        }

        var nombres = items.map(function (x) { return x.nombre; });

        input.readOnly = true;
        input.classList.add('filtro-picker-seleccionado');
        input.value = n === 1 ? items[0].nombre : n + ' ' + estado.cfg.plural + ' seleccionados';
        input.title = nombres.join(', ');
        if (estado.nameOriginal) input.removeAttribute('name');

        // Etiqueta chica: con 1 elegido, su identificacion (solo si existe y difiere del nombre: el servidor ya
        // lo resuelve y manda sub vacio si no aplica); con varios, los nombres.
        var textoSub = n === 1 ? items[0].sub : nombres.join(' · ');
        estado.sub.textContent = textoSub;
        estado.sub.title = n === 1 ? textoSub : nombres.join(', ');
        estado.sub.classList.toggle('d-none', !textoSub);
    }

    // Quita la seleccion: input editable y vacio (si habia seleccion el texto era un resumen; si no, "limpiar"
    // borra lo tipeado). dispararEvento: avisar a la pantalla que el filtro cambio (false en limpiezas internas
    // donde la pantalla no filtro nada). enfocar: solo cuando lo pide el usuario con el boton (no al cargar la pagina).
    function limpiarEstado(estado, dispararEvento, enfocar) {
        estado.items = [];
        pintar(estado);
        estado.input.value = '';
        if (enfocar) estado.input.focus();
        if (dispararEvento) dispararCambio(estado.input);
    }

    // ---------------------------------------------------------------- modal

    function modalUrl(clave) {
        if (!resolverModal()) return '';
        return modalEl.getAttribute('data-url-' + clave) || '';
    }

    function resolverModal() {
        if (modalEl && document.body.contains(modalEl)) return true;

        modalEl = document.getElementById('modalFiltroPicker');
        if (!modalEl) return false;

        modalUi = {
            titulo: modalEl.querySelector('.js-fp-titulo'),
            buscar: modalEl.querySelector('.js-fp-buscar'),
            tbody: modalEl.querySelector('.js-fp-tbody'),
            colNombre: modalEl.querySelector('.js-fp-col-nombre'),
            colSub: modalEl.querySelector('.js-fp-col-sub'),
            colExtra: modalEl.querySelector('.js-fp-col-extra'),
            chips: modalEl.querySelector('.js-fp-chips'),
            contador: modalEl.querySelector('.js-fp-contador'),
            aviso: modalEl.querySelector('.js-fp-aviso'),
            btnAplicar: modalEl.querySelector('.js-fp-aplicar'),
            btnLimpiarSel: modalEl.querySelector('.js-fp-limpiar-sel')
        };

        enlazarModal();
        return true;
    }

    function enlazarModal() {
        modalUi.buscar.addEventListener('input', function () {
            if (!sesion) return;
            clearTimeout(sesion.timer);
            sesion.timer = setTimeout(function () { cargarLista(); }, DEBOUNCE_BUSQUEDA_MS);
        });

        modalUi.buscar.addEventListener('keydown', alTeclearBusqueda);

        modalUi.tbody.addEventListener('click', function (e) {
            var fila = e.target.closest('tr.fp-fila');
            if (!fila) return;
            var checkbox = fila.querySelector('input[type=checkbox]');
            // Click directo en el checkbox: el navegador ya cambio su estado; en otro lado de la fila se alterna a mano.
            var tildado = e.target === checkbox ? checkbox.checked : !(sesion && sesion.seleccion.has(Number(fila.dataset.id)));
            fijarFila(fila, tildado);
        });

        modalUi.chips.addEventListener('click', function (e) {
            var quitar = e.target.closest('.js-fp-chip-quitar');
            if (!quitar || !sesion) return;
            sesion.seleccion.delete(Number(quitar.dataset.id));
            sincronizarChecks();
            pintarChips();
        });

        modalUi.btnLimpiarSel.addEventListener('click', function () {
            if (!sesion) return;
            sesion.seleccion.clear();
            sincronizarChecks();
            pintarChips();
            modalUi.buscar.focus();
        });

        modalUi.btnAplicar.addEventListener('click', aplicarSeleccion);

        // Foco en el buscador al abrir.
        modalEl.addEventListener('shown.bs.modal', function () {
            modalUi.buscar.focus();
            modalUi.buscar.select();
        });

        // Al cerrar (aplicar, cancelar o Esc) se descarta la sesion y el foco vuelve al input que lo abrio.
        modalEl.addEventListener('hidden.bs.modal', function () {
            var estado = sesion ? sesion.estado : null;
            if (sesion) clearTimeout(sesion.timer);
            sesion = null;
            modalUi.tbody.innerHTML = '';
            if (estado) estado.input.focus();
        });
    }

    function abrirModal(estado) {
        if (!resolverModal()) {
            console.error('filtro-picker: falta incluir _FiltroPickerModal en la pagina');
            return;
        }
        if (document.querySelector('.modal.show')) return; // ya hay un modal abierto

        sesion = { estado: estado, seleccion: new Map(), token: 0, timer: null, items: [] };
        estado.items.forEach(function (item) { sesion.seleccion.set(item.id, item); });

        modalUi.titulo.textContent = estado.cfg.titulo;
        modalUi.colNombre.textContent = estado.cfg.colNombre;
        modalUi.buscar.placeholder = estado.cfg.placeholder;
        modalUi.colSub.textContent = estado.cfg.colSub;
        modalUi.colExtra.textContent = estado.cfg.colExtra;
        modalUi.colExtra.classList.toggle('d-none', !estado.cfg.colExtra);
        modalUi.buscar.value = '';
        modalUi.tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted py-3">Cargando...</td></tr>';
        modalUi.aviso.classList.add('d-none');
        pintarChips();

        window.bootstrap.Modal.getOrCreateInstance(modalEl).show();
        cargarLista();
    }

    // Pide la lista al servidor segun el texto del buscador. El token descarta respuestas viejas que llegan tarde.
    function cargarLista() {
        if (!sesion) return;

        var token = ++sesion.token;
        var estado = sesion.estado;
        var url = modalUrl(estado.cfg.fuente);

        pedirJson(url, { filtro: modalUi.buscar.value })
            .then(function (resp) {
                if (!sesion || token !== sesion.token) return;
                sesion.items = ((resp && resp.items) || []).map(normalizarItem).filter(function (x) { return x.id > 0; });
                pintarFilas(!!(resp && resp.hayMas));
            })
            .catch(function (err) {
                if (!sesion || token !== sesion.token) return;
                console.error('filtro-picker: no se pudo cargar la lista', err);
                modalUi.tbody.innerHTML = '<tr><td colspan="5" class="text-center text-danger py-3">No se pudo cargar la lista.</td></tr>';
            });
    }

    function pintarFilas(hayMas) {
        var estado = sesion.estado;
        var html = '';

        sesion.items.forEach(function (item, indice) {
            var tildado = sesion.seleccion.has(item.id);
            var marcas = (item.reservada ? ' <i class="fas fa-lock text-muted ml-1" title="Cuenta corriente reservada"></i>' : '')
                + (item.inactivo ? ' <span class="badge bg-secondary">inactivo</span>' : '');
            html += '<tr class="fp-fila' + (tildado ? ' fp-tildada' : '') + '" data-id="' + item.id + '">'
                + '<td class="fp-col-check"><input type="checkbox" tabindex="-1"' + (tildado ? ' checked' : '') + '></td>'
                + '<td class="fp-col-num d-none d-md-table-cell">' + (indice < MAX_ATAJOS_ALT ? (indice + 1) : '–') + '</td>'
                + '<td>' + esc(item.nombre) + marcas + '</td>'
                + '<td>' + esc(item.sub) + '</td>'
                + (estado.cfg.colExtra ? '<td>' + esc(item.cuit) + '</td>' : '<td class="d-none"></td>')
                + '</tr>';
        });

        modalUi.tbody.innerHTML = html || '<tr><td colspan="5" class="text-center text-muted py-3">Sin coincidencias.</td></tr>';
        modalUi.aviso.classList.toggle('d-none', !hayMas);

        // Primera fila activa para navegar con el teclado.
        var primera = modalUi.tbody.querySelector('tr.fp-fila');
        if (primera) primera.classList.add('is-selected');
    }

    function fijarFila(fila, tildado) {
        if (!sesion) return;
        var id = Number(fila.dataset.id);

        if (tildado) {
            var item = sesion.items.find(function (x) { return x.id === id; });
            if (item) sesion.seleccion.set(id, item);
        } else {
            sesion.seleccion.delete(id);
        }

        fila.classList.toggle('fp-tildada', tildado);
        var checkbox = fila.querySelector('input[type=checkbox]');
        if (checkbox) checkbox.checked = tildado;

        // La fila clickeada pasa a ser la activa.
        modalUi.tbody.querySelectorAll('tr.is-selected').forEach(function (f) { f.classList.remove('is-selected'); });
        fila.classList.add('is-selected');

        pintarChips();
    }

    // Repinta los checks de las filas visibles segun la seleccion (tras quitar un chip o limpiar todo).
    function sincronizarChecks() {
        modalUi.tbody.querySelectorAll('tr.fp-fila').forEach(function (fila) {
            var tildado = sesion.seleccion.has(Number(fila.dataset.id));
            fila.classList.toggle('fp-tildada', tildado);
            var checkbox = fila.querySelector('input[type=checkbox]');
            if (checkbox) checkbox.checked = tildado;
        });
    }

    // Chips con todo lo elegido (incluye lo que ya no aparece en la busqueda actual) + contador.
    function pintarChips() {
        var n = sesion ? sesion.seleccion.size : 0;
        var html = '';
        if (sesion) {
            sesion.seleccion.forEach(function (item) {
                html += '<span class="fp-chip">' + esc(item.nombre)
                    + '<button type="button" class="js-fp-chip-quitar" data-id="' + item.id + '" aria-label="Quitar ' + esc(item.nombre) + '">&times;</button></span>';
            });
        }
        modalUi.chips.innerHTML = html;
        modalUi.chips.classList.toggle('d-none', n === 0);
        modalUi.contador.textContent = n === 0 ? 'Ninguno seleccionado' : (n === 1 ? '1 seleccionado' : n + ' seleccionados');
        modalUi.btnAplicar.textContent = n > 0 ? 'Aplicar (' + n + ')' : 'Aplicar';
    }

    // ---------------------------------------------------------------- teclado del modal

    // Alt+digito del NUMPAD en Windows compone un caracter a nivel de SO y lo entrega al soltar Alt contra el
    // campo que tenga el foco: se cancela el primer beforeinput/keypress tras el atajo (mismo bug y guard que
    // persona-buscar.js, 2026-09-23; duplicado a proposito: no hay modulo de utils compartido).
    var suprimirCaracterAlt = false;
    var suprimirCaracterAltTimer = null;
    function armarSupresionCaracterAlt() {
        suprimirCaracterAlt = true;
        clearTimeout(suprimirCaracterAltTimer);
        suprimirCaracterAltTimer = setTimeout(function () { suprimirCaracterAlt = false; }, 500);
    }
    function cancelarSiSupresionActiva(e) {
        if (!suprimirCaracterAlt) return;
        suprimirCaracterAlt = false;
        clearTimeout(suprimirCaracterAltTimer);
        e.preventDefault();
    }
    document.addEventListener('beforeinput', cancelarSiSupresionActiva, true);
    document.addEventListener('keypress', cancelarSiSupresionActiva, true);

    // Alt+1..9 por e.code (no e.key: en layouts no-US Alt puede alterar el caracter).
    function altDigito(e) {
        if (!e.altKey || e.ctrlKey || e.metaKey || e.shiftKey || e.repeat) return null;
        var m = /^(?:Digit|Numpad)([1-9])$/.exec(e.code || '');
        return m ? parseInt(m[1], 10) : null;
    }

    function filaActiva() {
        return modalUi.tbody.querySelector('tr.fp-fila.is-selected') || modalUi.tbody.querySelector('tr.fp-fila');
    }

    function alternarFila(fila) {
        if (!fila || !sesion) return;
        fijarFila(fila, !sesion.seleccion.has(Number(fila.dataset.id)));
    }

    // Teclado en el buscador del modal. La barra espaciadora NO tilda (es parte del texto de busqueda):
    //  - flechas: mover la fila activa      - Enter: tildar / destildar la fila activa
    //  - Alt+Enter: aplicar (mismo atajo de "guardar" de toda la app)      - Alt+1..9: tildar esa fila
    //  - Esc: cerrar sin aplicar (lo maneja Bootstrap)
    function alTeclearBusqueda(e) {
        var digito = altDigito(e);
        if (digito != null) {
            e.preventDefault();
            armarSupresionCaracterAlt();
            alternarFila(modalUi.tbody.querySelectorAll('tr.fp-fila')[digito - 1]);
            return;
        }

        if (e.key === 'Enter') {
            e.preventDefault();
            if (e.altKey) aplicarSeleccion();
            else alternarFila(filaActiva());
            return;
        }

        if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
            e.preventDefault();
            var actual = filaActiva();
            if (!actual) return;
            var destino = e.key === 'ArrowDown' ? actual.nextElementSibling : actual.previousElementSibling;
            if (destino && destino.classList.contains('fp-fila')) {
                actual.classList.remove('is-selected');
                destino.classList.add('is-selected');
                destino.scrollIntoView({ block: 'nearest' });
            }
        }
    }

    // ---------------------------------------------------------------- aplicar

    function aplicarSeleccion() {
        if (!sesion) return;

        var estado = sesion.estado;
        var elegidos = [];
        sesion.seleccion.forEach(function (item) { elegidos.push(item); });

        var habiaSeleccion = estado.items.length > 0;
        var mismaSeleccion = elegidos.length === estado.items.length
            && elegidos.every(function (x) { return estado.items.some(function (y) { return y.id === x.id; }); });

        window.bootstrap.Modal.getOrCreateInstance(modalEl).hide();

        if (mismaSeleccion) return; // nada cambio (incluye "sin seleccion" -> "sin seleccion": no se pisa el texto tipeado)

        if (elegidos.length === 0 && habiaSeleccion) {
            // Se destildo todo: equivale a "limpiar" (input editable y vacio).
            estado.items = [];
            pintar(estado);
            estado.input.value = '';
        } else {
            aplicarItems(estado, elegidos);
        }
        dispararCambio(estado.input);
    }

    // ---------------------------------------------------------------- atajos F9 / F10

    // Los atajos se registran a nivel de documento y abren el selector cuyo input declara esa tecla. Se ignoran
    // con otro modal abierto (la pantalla de fondo no debe reaccionar) y si el input esta oculto.
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'F9' && e.key !== 'F10') return;
        if (e.altKey || e.ctrlKey || e.metaKey || e.shiftKey) return;
        if (document.querySelector('.modal.show')) return;

        for (var i = 0; i < estados.length; i++) {
            var estado = estados[i];
            if (estado.tecla !== e.key) continue;
            if (!estado.input.offsetParent) continue; // oculto

            e.preventDefault();
            abrirModal(estado);
            return;
        }
    });

    // ---------------------------------------------------------------- API publica

    window.FiltroPicker = {
        // CSV de IDs seleccionados ("" si no hay seleccion: la pantalla filtra por texto).
        getIds: function (input) { return (input && input.dataset.ids) || ''; },
        // Texto libre escrito; vacio si hay seleccion (el valor del input es solo un resumen).
        getTexto: function (input) { return input && !input.dataset.ids ? input.value : ''; },
        // Items seleccionados [{id, nombre, sub, ...}] (para que la pantalla los guarde, p. ej. en sessionStorage).
        getItems: function (input) { var e = buscarEstado(input); return e ? e.items.map(function (x) { return Object.assign({}, x); }) : []; },
        // Fija la seleccion desde la pantalla (p. ej. al restaurar sessionStorage). No dispara eventos.
        setItems: function (input, items) {
            var e = buscarEstado(input);
            if (!e) return;
            var lista = (items || []).map(normalizarItem).filter(function (x) { return x.id > 0; });
            if (lista.length === 0) { e.items = []; pintar(e); return; }
            aplicarItems(e, lista);
        },
        limpiar: function (input) { var e = buscarEstado(input); if (e) limpiarEstado(e, true, true); }
    };

    // Inicializacion idempotente (mejorarInput ignora los inputs ya mejorados). Se corre apenas se carga el script
    // -- los scripts de cada pantalla (que leen data-ids / usan FiltroPicker.setItems) corren sincronicos justo
    // despues, y los inputs ya estan parseados porque el <script> va debajo del filtro -- y de nuevo al terminar
    // de cargar la pagina por si algun input se agrego mas abajo.
    function iniciarTodos() {
        document.querySelectorAll('input[data-filtro-picker]').forEach(mejorarInput);
    }
    iniciarTodos();
    document.addEventListener('DOMContentLoaded', iniciarTodos);
})();
