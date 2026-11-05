/* Pantallas de trabajo (Stock/Editar, Movimientos/Editar, Elaborados/Carga): comportamiento de ESCRITORIO (>= 1200 px) de pantalla-trabajo.css.
   - Cuenta las filas de la tabla y se las pasa al CSS (--pt-filas) para el minimo de 20 filas visibles.
   - Mueve los botones de guardar/cancelar al pie de la columna derecha (y los devuelve a su lugar por debajo de 1200 px).
   - Cabecera (Sucursal / Fecha / Accion / Proveedor...): siempre visible; en escritorio el CSS oculta las etiquetas y aca se pasan a
     placeholder / tooltip del campo. Los campos no se mueven ni se duplican: ids, binding y POST no cambian.
     (Hasta el 2026-11-04 la cabecera se plegaba en una linea resumen; el usuario pidio sacarlo y dejarla siempre visible.)
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
    var FILAS_CABECERA = '.stock-fila-datos, .stock-fila-pesaje, .mov-fila-datos, .elaborados-fila-datos';

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
    // Boton "Abrir en ventana" oculto a pedido del usuario (2026-11-04). El modo ventana sigue funcionando (mini-apps con ?ventana=1);
    // para volver a mostrar el boton, poner true.
    var MOSTRAR_BOTON_VENTANA = false;
    var enModoVentana = document.documentElement.classList.contains('modo-ventana');

    function avisar(texto, icono) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({ toast: true, position: 'top', icon: icono || 'info', title: texto, showConfirmButton: false, timer: 4000 });
        } else {
            window.alert(texto);
        }
    }

    function abrirEnVentana() {
        // El borrador de esta pestana no viaja a la ventana nueva: con cambios sin guardar no se abre (edit-page-guard.js marca __protegerSalida).
        if (window.__protegerSalida) {
            avisar('Esta pantalla tiene cambios sin guardar. Guardalos o cancelalos antes de abrirla en una ventana.', 'warning');
            return;
        }
        var disponibleAncho = (window.screen && (window.screen.availWidth || window.screen.width)) || 1280;
        var disponibleAlto = (window.screen && (window.screen.availHeight || window.screen.height)) || 800;
        var caracteristicas = 'popup=yes,left=0,top=0,width=' + disponibleAncho + ',height=' + disponibleAlto + ',resizable=yes,scrollbars=yes';

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
        ventana.location.href = url.toString();
        ventana.focus();
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
        if (enModoVentana || !MOSTRAR_BOTON_VENTANA) return;
        var barra = $1('[class*="-barra-botones"]', root);
        if (!barra) return;
        var b = document.createElement('button');
        b.type = 'button';
        b.className = 'btn btn-outline-primary pt-btn-ventana';
        b.title = 'Abrir esta pantalla en una ventana independiente (queda como una ventana más en la barra de tareas)';
        b.setAttribute('data-edit-readonly-ignore', 'true');
        b.innerHTML = '<i class="fas fa-external-link-alt mr-1"></i> Abrir en ventana';
        b.addEventListener('click', abrirEnVentana);
        var grupo = barra.firstElementChild && barra.firstElementChild.tagName === 'DIV' ? barra.firstElementChild : barra;
        grupo.appendChild(b);
    }

    // ---------- arranque ----------
    prepararVentana();
    prepararEntrada();
    seguirRenglonCodigo();
    prepararCuadroElaborado();
    prepararCabecera();
    prepararPesajeStock();
    ubicarAcciones();
    actualizarFilas();
    function alCambiarAncho() {
        ubicarAcciones();
        ubicarPesajeStock();
        aplicarPlaceholders();
    }
    if (esEscritorio.addEventListener) esEscritorio.addEventListener('change', alCambiarAncho);
    else if (esEscritorio.addListener) esEscritorio.addListener(alCambiarAncho);
})();
