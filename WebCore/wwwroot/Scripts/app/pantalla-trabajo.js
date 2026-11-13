/* Pantallas de trabajo (Stock/Editar, Movimientos/Editar, Elaborados/Carga): comportamiento de ESCRITORIO (>= 1200 px) de pantalla-trabajo.css.
   - Cuenta las filas de la tabla y se las pasa al CSS (--pt-filas) para el minimo de 20 filas visibles.
   - Mueve los botones de guardar/cancelar al pie de la columna derecha (y los devuelve a su lugar por debajo de 1200 px).
   - Cabecera (Sucursal / Fecha / Accion / Proveedor...): siempre visible; en escritorio el CSS oculta las etiquetas y aca se pasan a
     placeholder / tooltip del campo. Los campos no se mueven ni se duplican: ids, binding y POST no cambian.
     (Hasta el 2026-11-04 la cabecera se plegaba en una linea resumen; el usuario pidio sacarlo y dejarla siempre visible.)
   - Modo solo lectura (seccion 7): chip "Solo lectura", Modificar arriba, cuadro de carga escondido y aviso al intentar escribir (2026-11-13).
   Se activa solo si la pagina tiene .pantalla-trabajo[data-pt]. Dependencias: ninguna (JS plano). Detalle: docs/DECISIONS.md (2026-11-04). */
(function () {
    'use strict';

    var root = document.querySelector('.pantalla-trabajo[data-pt]');
    if (!root) return;

    var pantalla = root.getAttribute('data-pt');
    var esEscritorio = window.matchMedia('(min-width: 1200px)');

    // ---------- utilidades ----------
    function $1(selector, ambito) { return (ambito || document).querySelector(selector); }
    function $todos(selector, ambito) { return Array.prototype.slice.call((ambito || document).querySelectorAll(selector)); }
    function normalizar(texto) { return String(texto || '').replace(/\s+/g, ' ').trim(); }

    // ---------- 1. Cantidad de filas de la tabla (para el minimo de 20 filas del CSS) ----------
    var tbody = $1('.pt-tabla-wrap tbody', root);
    var filasActuales = 0;

    function contarFilas() {
        if (!tbody) return 0;
        var n = 0;
        $todos('tr', tbody).forEach(function (tr) {
            // Las filas de aviso ("Todavia no hay lineas", "Sin resultados") llevan un td con colspan: no cuentan.
            if (!tr.querySelector('td[colspan]')) n++;
        });
        return n;
    }

    function actualizarFilas() {
        var n = contarFilas();
        if (n !== filasActuales) {
            filasActuales = n;
            root.style.setProperty('--pt-filas', String(n));
        } else if (!root.style.getPropertyValue('--pt-filas')) {
            root.style.setProperty('--pt-filas', String(n));
        }
    }

    if (tbody) {
        // Las pantallas redibujan el tbody completo en cada cambio (renderLineas / renderLines): alcanza con mirar sus hijos directos.
        new MutationObserver(actualizarFilas).observe(tbody, { childList: true });
    }

    // ---------- 2. Botones de guardar / cancelar al pie de la columna derecha ----------
    var marcadorAcciones = null;

    function ubicarAcciones() {
        var acciones = $1('.pt-acciones', root);
        var columnaDerecha = $1('.pt-col-der', root);
        if (!acciones || !columnaDerecha) return;
        // El marcador recuerda el lugar original para devolver los botones cuando la ventana baja de 1200 px.
        if (!marcadorAcciones) {
            marcadorAcciones = document.createComment('pt-acciones');
            acciones.parentNode.insertBefore(marcadorAcciones, acciones);
        }
        if (esEscritorio.matches && acciones.parentNode !== columnaDerecha) {
            columnaDerecha.appendChild(acciones);
        } else if (!esEscritorio.matches && acciones.parentNode === columnaDerecha) {
            marcadorAcciones.parentNode.insertBefore(acciones, marcadorAcciones.nextSibling);
        }
    }

    // ---------- 3. Cabecera siempre visible, sin etiquetas (escritorio) ----------
    // El CSS (seccion 3 de pantalla-trabajo.css) oculta las etiquetas de estas filas; para que se siga sabiendo que es cada campo, el texto
    // de la etiqueta pasa a placeholder (inputs de texto/numero sin placeholder propio) y a tooltip (todos, incluidos select y fecha).
    // Movimientos (.mov-fila-datos) ya no entra: sus etiquetas Origen / Destino / Fecha se ven siempre, a la izquierda del campo (2026-11-09).
    var FILAS_CABECERA = '.stock-fila-datos, .stock-fila-pesaje, .elaborados-fila-datos';

    function prepararCabecera() {
        $todos(FILAS_CABECERA, root).forEach(function (fila) {
            $todos('label', fila).forEach(function (etiqueta) {
                // Las etiquetas de los interruptores (Solo productos) se ven siempre: no son la etiqueta de un campo.
                if (etiqueta.classList.contains('custom-control-label')) return;
                var texto = normalizar(etiqueta.textContent);
                if (!texto) return;
                // Etiqueta con "for" (Sucursal, Fecha...) o, si no tiene, el campo de su mismo form-group (Usuario / Creado de Elaborados).
                var campo = etiqueta.getAttribute('for')
                    ? document.getElementById(etiqueta.getAttribute('for'))
                    : (etiqueta.closest('.form-group') ? etiqueta.closest('.form-group').querySelector('input, select, textarea') : null);
                if (campo) registrarEtiqueta(campo, texto);
            });
        });
        aplicarPlaceholders();
    }

    // ---------- 4. "Agregar producto" en una fila ----------
    // El CSS (seccion 4 de pantalla-trabajo.css) necesita: la clase en el cuadro, las etiquetas de los campos como placeholder (se ocultan
    // para ganar altura) y un tooltip en cada boton que queda solo con el icono.
    function prepararEntrada() {
        // El cuadro de carga es el que contiene el layout del teclado numerico (hay otros .stock-section-box en la pagina).
        var caja = $todos('.stock-section-box, .mov-box-agregar, .elaborados-box-ingredientes', root).filter(function (c) {
            return !!c.querySelector(':scope > .numeric-keypad-toggle-layout');
        })[0];
        if (!caja) return;
        caja.classList.add('pt-box-agregar');
        // Los placeholders solo existen en escritorio (donde el CSS oculta las etiquetas): por debajo de 1200 px el formulario queda como siempre.
        ponerPlaceholders($todos('.numeric-keypad-toggle-fields label[for]', caja));
        var cabecera = caja.firstElementChild;
        if (!cabecera) return;
        $todos('.btn', cabecera).forEach(function (b) {
            var texto = normalizar(b.textContent);
            if (!b.getAttribute('title') && texto) b.setAttribute('title', texto);
            if (!b.getAttribute('aria-label') && (b.getAttribute('title') || texto)) b.setAttribute('aria-label', b.getAttribute('title') || texto);
        });
    }

    // Movimientos con "Stock destino": Balanza / Ver acumulados / teclado van posicionados al extremo derecho del renglon del Codigo
    // (seccion 5b del CSS). Cuando aparece la advertencia arriba, ese renglon baja: se mide donde esta el campo Codigo y se le pasa al CSS
    // (--pt-mov-botones-top / --pt-mov-botones-alto). Solo mide; en el resto de los casos el CSS no usa estas variables.
    function seguirRenglonCodigo() {
        var caja = $1('.pt-box-agregar', root);
        var codigo = document.getElementById('txtCodigoProducto');
        if (pantalla !== 'mov' || !caja || !codigo || typeof window.ResizeObserver !== 'function') return;
        var grupo = codigo.closest('.input-group') || codigo;
        function medir() {
            var rCaja = caja.getBoundingClientRect();
            var rGrupo = grupo.getBoundingClientRect();
            if (!rGrupo.height) return;
            caja.style.setProperty('--pt-mov-botones-top', Math.round(rGrupo.top - rCaja.top) + 'px');
            caja.style.setProperty('--pt-mov-botones-alto', Math.round(rGrupo.height) + 'px');
        }
        new ResizeObserver(medir).observe(caja);
        medir();
    }

    // Stock con accion Pesaje (pedido 2026-11-04: ver al menos 15 productos): en escritorio la caja "Compra vinculada" (la del alta,
    // #stockCompraVinculadaBox, y la del registro guardado, en .stock-fila-vinculada) pasa al renglon del Proveedor / Medias / Kgs, y el
    // estado de la balanza al renglon de la lupa. Se MUEVEN (no se duplican: ids y manejadores de stock.js siguen igual) y vuelven a su lugar
    // si la accion deja de ser Pesaje (o "Solo productos" oculta el bloque) o la ventana baja de 1200 px. El acomodo en una linea es CSS
    // (seccion 7 de pantalla-trabajo.css), que se cuelga de las clases que pone esta funcion.
    var ubicarPesajeStock = function () { };
    function prepararPesajeStock() {
        if (pantalla !== 'stock') return;
        var bloque = $1('#bloquePesajeStock', root);
        if (!bloque) return;
        var movibles = $todos('#stockCompraVinculadaBox, .stock-fila-vinculada .stock-compra-vinculada-box', root).map(function (caja) {
            return { el: caja, destino: bloque, clase: 'pt-vinc-en-linea' };
        });
        var estadoBalanza = $1('#balanzaStatusWrapStock', root);
        var barraLupa = $1('.stock-search-toolbar', root);
        if (estadoBalanza && barraLupa) movibles.push({ el: estadoBalanza, destino: barraLupa, clase: 'pt-balanza-en-lupa' });
        // Un marcador por elemento recuerda su lugar original.
        movibles.forEach(function (m) {
            m.marca = document.createComment('pt-pesaje');
            m.el.parentNode.insertBefore(m.marca, m.el);
        });
        // Vincular compra / Vincular pesajes quedan solo con icono: el nombre pasa a tooltip (solo escritorio, ver aplicarPlaceholders).
        ['#btnSeleccionarCompraPesaje', '#btnVincularPesajes'].forEach(function (selector) {
            var boton = $1(selector, root);
            if (boton) registrarEtiqueta(boton, normalizar(boton.textContent));
        });
        aplicarPlaceholders();
        ubicarPesajeStock = function () {
            var activo = esEscritorio.matches && !bloque.classList.contains('d-none');
            movibles.forEach(function (m) {
                if (activo && m.el.parentNode !== m.destino) {
                    m.destino.appendChild(m.el);
                    m.el.classList.add(m.clase);
                } else if (!activo && m.el.parentNode === m.destino) {
                    m.marca.parentNode.insertBefore(m.el, m.marca.nextSibling);
                    m.el.classList.remove(m.clase);
                }
            });
            root.classList.toggle('pt-pesaje-linea', activo);
        };
        // Cambiar de accion o tocar "Solo productos" muestra / oculta el bloque con d-none (stock.js).
        new MutationObserver(function () { ubicarPesajeStock(); }).observe(bloque, { attributes: true, attributeFilter: ['class'] });
        ubicarPesajeStock();
    }

    // Elaborados: el cuadro "Elaborado" (codigo + nombre) tambien pasa sus etiquetas a placeholder (el CSS las oculta).
    function prepararCuadroElaborado() {
        ponerPlaceholders($todos('.elaborados-box-elaborado .row label[for]', root));
    }

    // Etiqueta -> placeholder del campo, solo en escritorio y solo en campos que no traen placeholder propio. Se recuerda que campos se tocaron
    // para quitarles el placeholder si la ventana baja de 1200 px.
    var camposConPlaceholder = [];
    function ponerPlaceholders(etiquetas) {
        etiquetas.forEach(function (etiqueta) {
            var campo = document.getElementById(etiqueta.getAttribute('for'));
            if (campo && !campo.getAttribute('placeholder')) {
                camposConPlaceholder.push({ campo: campo, texto: normalizar(etiqueta.textContent), placeholder: true, tooltip: false });
            }
        });
        aplicarPlaceholders();
    }

    // Cabecera: placeholder solo en inputs de texto/numero que no traen uno propio; tooltip solo si el campo no tiene title propio.
    function registrarEtiqueta(campo, texto) {
        var tipo = (campo.getAttribute('type') || 'text').toLowerCase();
        var admitePlaceholder = campo.tagName === 'INPUT' && (tipo === 'text' || tipo === 'number') && !campo.getAttribute('placeholder');
        var admiteTooltip = !campo.getAttribute('title');
        if (admitePlaceholder || admiteTooltip) {
            camposConPlaceholder.push({ campo: campo, texto: texto, placeholder: admitePlaceholder, tooltip: admiteTooltip });
        }
    }

    // Solo en escritorio (donde el CSS oculta las etiquetas); por debajo de 1200 px se sacan y el formulario queda como siempre.
    function aplicarPlaceholders() {
        camposConPlaceholder.forEach(function (c) {
            if (c.placeholder) {
                if (esEscritorio.matches) c.campo.setAttribute('placeholder', c.texto);
                else c.campo.removeAttribute('placeholder');
            }
            if (c.tooltip) {
                if (esEscritorio.matches) c.campo.setAttribute('title', c.texto);
                else c.campo.removeAttribute('title');
            }
        });
    }

    // ---------- 5. Ventana independiente ----------
    // "Abrir en ventana": la pantalla pasa a una ventana propia del navegador, sin menus (modo-ventana, script del <head> de _Layout.cshtml), que
    // se ve como una ventana mas en la barra de tareas. Una por pantalla (nombre fijo): asi se puede tener Stock, Movimientos y Elaborados abiertos
    // a la vez y pasar de una a otra con Alt+Tab o desde la barra de tareas.
    var NOMBRE_VENTANA = 'carnisys-trabajo-' + pantalla;
    // Boton "Abrir en ventana": oculto a pedido del usuario el 2026-11-04 y vuelto a mostrar el 2026-11-12 SOLO en Movimientos (en Stock y
    // Elaborados sigue oculto; el modo ventana funciona igual con las mini-apps ?ventana=1). Para sumar otra pantalla, agregar su data-pt aca.
    // 2026-11-13: tambien Stock y Elaborados (el usuario pidio el mismo estilo que Movimientos en las tres).
    var PANTALLAS_CON_BOTON_VENTANA = ['mov', 'stock', 'elab'];
    var MOSTRAR_BOTON_VENTANA = PANTALLAS_CON_BOTON_VENTANA.indexOf(pantalla) >= 0;
    // Tamano de la ventana (2026-11-12, pedido del usuario): UN TERCIO del ancho disponible de la pantalla y todo el alto, solo en escritorio
    // (el boton no existe en celular). Piso de 480 px para monitores chicos (con menos no entran los campos) y nunca mas que la pantalla.
    // Por debajo de 1200 px el diseno de escritorio lo aplica ventana-angosta.js (antes: 60 % con piso de 1200 px).
    var VENTANA_ANCHO_FRACCION = 1 / 3;
    var VENTANA_ANCHO_MIN_PX = 480;
    // Nombre de la pantalla para el titulo de la ventana (se ve en la barra de titulo y al pasar el mouse por la barra de tareas).
    var NOMBRE_PANTALLA = { mov: 'Movimientos', stock: 'Stock', elab: 'Elaborados' }[pantalla] || '';
    var enModoVentana = document.documentElement.classList.contains('modo-ventana');

    function avisar(texto, icono) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({ toast: true, position: 'top', icon: icono || 'info', title: texto, showConfirmButton: false, timer: 4000 });
        } else {
            window.alert(texto);
        }
    }

    var AVISO_CAMBIOS_SIN_GUARDAR = 'Esta pantalla tiene cambios sin guardar. Guardalos o cancelalos antes de abrirla en una ventana.';

    function abrirEnVentana() {
        // Con cambios sin guardar (edit-page-guard.js marca __protegerSalida): si la pantalla sabe pasarle lo cargado a la ventana nueva
        // (window.PantallaTrabajoTraspaso: hoy Movimientos, solo en un movimiento nuevo) se abre igual y lo cargado viaja; si no, el borrador de
        // esta pestana no puede viajar y no se abre.
        var traspaso = window.PantallaTrabajoTraspaso || null;
        var hayCambios = window.__protegerSalida === true || !!(traspaso && traspaso.hayCambios());
        if (hayCambios && !traspaso) {
            avisar(AVISO_CAMBIOS_SIN_GUARDAR, 'warning');
            return;
        }
        var disponibleAncho = (window.screen && (window.screen.availWidth || window.screen.width)) || 1280;
        var disponibleAlto = (window.screen && (window.screen.availHeight || window.screen.height)) || 800;
        var ancho = Math.min(disponibleAncho, Math.max(VENTANA_ANCHO_MIN_PX, Math.round(disponibleAncho * VENTANA_ANCHO_FRACCION)));
        var izquierda = Math.max(0, Math.round((disponibleAncho - ancho) / 2));
        var caracteristicas = 'popup=yes,left=' + izquierda + ',top=0,width=' + ancho + ',height=' + disponibleAlto + ',resizable=yes,scrollbars=yes';

        // Si ya hay una ventana de esta pantalla se la trae al frente SIN recargarla (no se pierde lo que tenga cargado). window.open con el
        // mismo nombre devuelve esa ventana; si no existia, abre una en blanco que se navega enseguida.
        var ventana = window.open('', NOMBRE_VENTANA, caracteristicas);
        if (!ventana) {
            avisar('El navegador bloqueó la ventana emergente. Permitila para este sitio e intentá de nuevo.', 'warning');
            return;
        }
        var yaTeniaPagina = false;
        try { yaTeniaPagina = ventana.location.href !== 'about:blank'; } catch (e) { yaTeniaPagina = true; }
        if (yaTeniaPagina) {
            ventana.focus();
            avisar('Ya hay una ventana abierta de esta pantalla. La traje al frente.');
            return;
        }
        var url = new URL(window.location.href);
        url.searchParams.set('ventana', '1');
        if (hayCambios) {
            // Lo cargado viaja en una clave temporal de localStorage (misma PC y navegador); si la pantalla no puede prepararlo, no se abre.
            var clave = traspaso.preparar();
            if (!clave) {
                try { ventana.close(); } catch (e) { }
                avisar(AVISO_CAMBIOS_SIN_GUARDAR, 'warning');
                return;
            }
            url.searchParams.set('traspaso', clave);
        }
        ventana.location.href = url.toString();
        ventana.focus();
        if (hayCambios) traspaso.alAbrirOk();
        // La pestana original vuelve al listado del modulo: el trabajo sigue en la ventana nueva.
        var volver = $1('[class*="-barra-botones"] a[href]', root);
        if (volver) window.location.href = volver.href;
    }

    function prepararVentana() {
        // Icono propio de cada pantalla (se ve en la ventana y en las pestanas): ver Content/img/ventanas.
        var script = document.currentScript || $1('script[src*="pantalla-trabajo.js"]');
        var base = script && script.src ? script.src.replace(/Scripts\/app\/pantalla-trabajo\.js.*$/, '') : '';
        if (base) {
            $todos('link[rel~="icon"]').forEach(function (l) { l.parentNode.removeChild(l); });
            var icono = document.createElement('link');
            icono.rel = 'icon';
            icono.type = 'image/svg+xml';
            icono.href = base + 'Content/img/ventanas/' + (pantalla === 'mov' ? 'movimientos' : pantalla === 'elab' ? 'elaborados' : 'stock') + '.svg';
            document.head.appendChild(icono);
        }
        // En la ventana independiente el titulo lleva el nombre de la pantalla al principio ("Movimientos · Nuevo Movimiento - CarniSys").
        if (enModoVentana && NOMBRE_PANTALLA && document.title.indexOf(NOMBRE_PANTALLA + ' · ') !== 0) {
            document.title = NOMBRE_PANTALLA + ' · ' + document.title;
        }
        if (enModoVentana || !MOSTRAR_BOTON_VENTANA) return;
        var barra = $1('[class*="-barra-botones"]', root);
        if (!barra) return;
        var b = document.createElement('button');
        b.type = 'button';
        b.setAttribute('data-edit-readonly-ignore', 'true');
        b.addEventListener('click', abrirEnVentana);
        // Las tres pantallas: solo icono, igual que pantalla completa, a su lado (a la izquierda de ella). Elaborados tiene un mb-2 en sus botones.
        b.className = 'btn btn-outline-secondary pt-btn-ventana' + (pantalla === 'elab' ? ' mb-2' : '');
        b.title = 'Abrir en una ventana independiente (sin menú lateral ni barra superior)';
        b.setAttribute('aria-label', 'Abrir en ventana independiente');
        b.innerHTML = '<i class="fas fa-external-link-alt"></i>';
        var pantallaCompleta = $1('#btnPantallaCompleta', barra);
        if (pantallaCompleta && pantallaCompleta.parentNode) pantallaCompleta.parentNode.insertBefore(b, pantallaCompleta);
        else barra.appendChild(b);
    }

    // ---------- 6. Foco inicial ----------
    // Al abrir un registro NUEVO el cursor queda en el campo Codigo (2026-11-13): Movimientos y Stock en el Codigo del producto; Elaborados en el
    // Codigo del cuadro "Elaborado" (el producto final; despues del Enter pasa a los ingredientes). Se reintenta unas veces durante los primeros
    // segundos porque otros scripts de la pagina (borradores, avisos, reacomodo de la pantalla) le sacaban el foco despues de cargar. Se deja de
    // insistir apenas el usuario toca algo, si hay un cartel / modal abierto, si el campo es de solo lectura (modo ver) o si ya trae un valor
    // (registro existente).
    var CAMPO_FOCO_INICIAL = { mov: '#txtCodigoProducto', stock: '#txtCodigoProducto', elab: '#txtCodigoElaborado' }[pantalla];
    var REINTENTOS_FOCO_MS = [0, 150, 400, 800, 1500, 2500];

    function prepararFocoInicial() {
        var campo = CAMPO_FOCO_INICIAL ? document.querySelector(CAMPO_FOCO_INICIAL) : null;
        if (!campo) return;
        var usuarioTocoAlgo = false;
        ['mousedown', 'keydown', 'touchstart'].forEach(function (evento) {
            document.addEventListener(evento, function () { usuarioTocoAlgo = true; }, true);
        });
        function intentar() {
            if (usuarioTocoAlgo || campo.disabled || campo.readOnly || campo.value) return;
            if (campo.offsetParent === null) return; // oculto
            if (document.querySelector('.modal.show, .swal2-container')) return; // no se le roba el foco a un cartel abierto
            if (document.activeElement !== campo) campo.focus();
        }
        REINTENTOS_FOCO_MS.forEach(function (ms) { window.setTimeout(intentar, ms); });
    }

    // ---------- 7. Modo solo lectura ("ver" un registro guardado) ----------
    // Pedido 2026-11-13: en modo ver el usuario no se daba cuenta de que no podia editar e intentaba tipear un codigo en un campo bloqueado.
    // edit-readonly.js (compartido con otras pantallas, NO se toca) deja la clase .edit-readonly-active en el <form> mientras esta en modo ver; aca
    // solo se la observa. Cuando esta puesta:
    //   a) el CSS (seccion 10 de pantalla-trabajo.css) esconde el cuadro "Agregar producto": si no hay campo, nadie intenta escribir;
    //   b) se muestra un chip "Solo lectura" y un boton Modificar en la barra de arriba (el de abajo, el original, sigue). El de arriba solo
    //      dispara el clic del original: toda la logica (permisos, otra pestana abierta, foto limpia del formulario) sigue siendo la de siempre;
    //   c) los botones que siguen activos en modo ver (Ver acumulados, Ver Porc) se mudan a una franja de aviso donde estaba el cuadro, y vuelven
    //      a su lugar al pasar a edicion;
    //   d) se le saca el foco al campo que lo tuviera, y si el usuario tipea letras o numeros (o pasa el lector de codigo de barras) aparece un
    //      aviso y el boton Modificar late una vez.
    // No cambia ids, binding ni POST. Un registro nuevo (sin modo ver) no pasa nunca por aca.
    var PISTA_MODIFICAR = 'Estás viendo este registro. Para cambiarlo, presioná Modificar.';
    var PISTA_SIN_MODIFICAR = 'Estás viendo este registro en modo lectura.';
    var AVISO_ESPERA_MS = 3000;
    var lecturaEnModoVer = false;
    var lecturaUI = null; // { chip, modificar, barra, texto, acciones, movidos: [{ boton, marca }] }
    var lecturaUltimoAviso = 0;

    function botonModificarOriginal() {
        // Un prefijo, no un id: cada pantalla tiene el suyo (...Stock, ...Movimiento, ...Elaborado). "btnHabilitarTipoCompra" (Stock) no empieza igual.
        return $1('[id^="btnHabilitarEdicion"]', root);
    }

    function crearUILectura(caja) {
        var chip = document.createElement('span');
        chip.className = 'pt-lectura-chip';
        chip.setAttribute('role', 'status');
        chip.title = PISTA_MODIFICAR;
        chip.innerHTML = '<i class="fas fa-eye" aria-hidden="true"></i> Solo lectura';

        var modificar = document.createElement('button');
        modificar.type = 'button';
        modificar.className = 'btn btn-primary pt-lectura-modificar';
        modificar.setAttribute('data-edit-readonly-ignore', 'true');
        modificar.innerHTML = '<i class="fas fa-edit mr-1" aria-hidden="true"></i> Modificar';
        modificar.addEventListener('click', function () {
            var original = botonModificarOriginal();
            if (original && !original.disabled) original.click();
        });

        // Chip y boton: en el grupo de la derecha de la barra (donde esta pantalla completa), el chip antes del grupo y Modificar al principio.
        var pantallaCompleta = $1('#btnPantallaCompleta', root);
        var grupo = pantallaCompleta ? pantallaCompleta.parentNode : null;
        var barra = $1('[class*="-barra-botones"]', root);
        if (grupo && barra && grupo !== barra) {
            barra.insertBefore(chip, grupo);
            grupo.insertBefore(modificar, grupo.firstChild);
        } else if (barra) {
            barra.appendChild(chip);
            barra.appendChild(modificar);
        }

        // Franja de aviso: va donde estaba el cuadro de carga (que el CSS esconde en modo ver) y aloja los botones que siguen activos.
        var franja = document.createElement('div');
        franja.className = 'pt-lectura-barra';
        franja.setAttribute('role', 'note');
        var texto = document.createElement('span');
        texto.className = 'pt-lectura-texto';
        var acciones = document.createElement('span');
        acciones.className = 'pt-lectura-acciones';
        franja.appendChild(texto);
        franja.appendChild(acciones);
        caja.parentNode.insertBefore(franja, caja);

        return { chip: chip, modificar: modificar, barra: franja, texto: texto, acciones: acciones, movidos: [] };
    }

    // Botones de la cabecera del cuadro que siguen habilitados en modo ver (edit-readonly.js deshabilita el resto): se mudan a la franja.
    function mudarBotonesActivos(caja) {
        var cabecera = caja.firstElementChild;
        if (!cabecera) return;
        $todos('.btn', cabecera).forEach(function (boton) {
            if (boton.disabled) return;
            var marca = document.createComment('pt-lectura');
            boton.parentNode.insertBefore(marca, boton);
            lecturaUI.acciones.appendChild(boton);
            lecturaUI.movidos.push({ boton: boton, marca: marca });
        });
    }

    function devolverBotonesActivos() {
        lecturaUI.movidos.forEach(function (m) {
            if (m.marca.parentNode) {
                m.marca.parentNode.insertBefore(m.boton, m.marca.nextSibling);
                m.marca.parentNode.removeChild(m.marca);
            }
        });
        lecturaUI.movidos = [];
    }

    // Sincroniza todo con el estado real del <form> (la fuente de verdad es edit-readonly.js, no este archivo).
    function sincronizarModoLectura(formulario, caja) {
        var enModoVer = formulario.classList.contains('edit-readonly-active');
        if (enModoVer === lecturaEnModoVer && lecturaUI) return;
        lecturaEnModoVer = enModoVer;
        if (!lecturaUI && enModoVer) lecturaUI = crearUILectura(caja);
        if (!lecturaUI) return;

        root.classList.toggle('pt-solo-lectura', enModoVer);
        if (enModoVer) {
            var original = botonModificarOriginal();
            var puedeModificar = !!original && !original.disabled;
            lecturaUI.modificar.disabled = !puedeModificar;
            lecturaUI.modificar.classList.toggle('d-none', !original);
            lecturaUI.texto.innerHTML = '<i class="fas fa-eye" aria-hidden="true"></i> ' + (puedeModificar
                ? 'Estás viendo este registro. Para cambiarlo, presioná <b>Modificar</b>.'
                : PISTA_SIN_MODIFICAR);
            lecturaUI.chip.title = puedeModificar ? PISTA_MODIFICAR : PISTA_SIN_MODIFICAR;
            mudarBotonesActivos(caja);
            // El campo que tenia el foco (el Codigo, que stock.js enfoca al cargar) ya no se puede usar: se lo suelta para que no quede el halo.
            var activo = document.activeElement;
            if (activo && root.contains(activo) && activo.matches('input, textarea, select') && !activo.closest('[data-edit-readonly-ignore]')) {
                activo.blur();
            }
        } else {
            devolverBotonesActivos();
        }
    }

    // Aviso al intentar escribir en modo ver: toast + el boton Modificar late una vez (los dos, el de arriba y el de abajo).
    function avisarModoLectura() {
        var ahora = Date.now();
        if (ahora - lecturaUltimoAviso < AVISO_ESPERA_MS) return;
        lecturaUltimoAviso = ahora;
        var original = botonModificarOriginal();
        var puedeModificar = !!original && !original.disabled;
        avisar(puedeModificar ? PISTA_MODIFICAR : PISTA_SIN_MODIFICAR, 'info');
        [lecturaUI && lecturaUI.modificar, original].forEach(function (boton) {
            if (!boton || boton.disabled) return;
            boton.classList.remove('pt-lectura-pulso');
            void boton.offsetWidth; // reinicia la animacion si ya estaba corriendo
            boton.classList.add('pt-lectura-pulso');
            window.setTimeout(function () { boton.classList.remove('pt-lectura-pulso'); }, 1500);
        });
    }

    function prepararModoLectura() {
        var caja = $1('.pt-box-agregar', root);
        var formulario = caja ? caja.closest('form') : null;
        if (!caja || !formulario) return;

        sincronizarModoLectura(formulario, caja);
        new MutationObserver(function () { sincronizarModoLectura(formulario, caja); })
            .observe(formulario, { attributes: true, attributeFilter: ['class'] });

        // Solo letras y numeros: no molesta con Tab, flechas, F-teclas, Enter ni con los atajos de la pantalla. Se ignoran los botones / links (la barra
        // espaciadora los activa), los campos que SI se pueden editar (filtro de la tabla, modales) y cualquier cartel abierto.
        document.addEventListener('keydown', function (e) {
            if (!lecturaEnModoVer || e.ctrlKey || e.metaKey || e.altKey) return;
            if (!/^[\p{L}\p{N}]$/u.test(e.key || '')) return;
            if (document.querySelector('.modal.show') || document.body.classList.contains('swal2-shown')) return;
            var blanco = e.target;
            if (!blanco || !blanco.closest) return;
            if (blanco.closest('button, a, select, [contenteditable="true"], [data-edit-readonly-ignore]')) return;
            if (blanco.matches('input, textarea') && !blanco.readOnly && !blanco.disabled) return;
            if (!root.contains(blanco) && blanco !== document.body && blanco !== document.documentElement) return;
            avisarModoLectura();
        }, true);
    }

    // ---------- arranque ----------
    prepararVentana();
    prepararEntrada();
    prepararModoLectura(); // despues de prepararEntrada(): necesita la clase .pt-box-agregar
    seguirRenglonCodigo();
    prepararCuadroElaborado();
    prepararCabecera();
    prepararPesajeStock();
    ubicarAcciones();
    actualizarFilas();
    prepararFocoInicial();
    function alCambiarAncho() {
        ubicarAcciones();
        ubicarPesajeStock();
        aplicarPlaceholders();
    }
    if (esEscritorio.addEventListener) esEscritorio.addEventListener('change', alCambiarAncho);
    else if (esEscritorio.addListener) esEscritorio.addListener(alCambiarAncho);
})();
