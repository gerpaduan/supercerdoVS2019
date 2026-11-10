// Ficha y alta de usuarios (Views/Usuarios/Asistente.cshtml; rediseno 2026-11-10, ver docs/DECISIONS.md "Rediseno de Usuarios").
// Dos modos sobre el mismo editor de permisos:
//   - ficha (usuario existente): pestanas Datos / Ingreso y seguridad / Permisos, barra de "cambios sin guardar" y un solo guardado.
//   - alta (usuario nuevo): 3 pasos (quien es, que puede hacer, revisar).
// El editor dibuja los permisos desde el JSON de la pagina (catalogo + estado actual: solo muestra los controles que tienen efecto
// real), aplica perfiles y "copiar de otro usuario" (con "Deshacer"), calcula "Entra al POS" y, al guardar, vuelca el estado de cada
// permiso en campos ocultos (Permisos[i].*). La traduccion estado <-> fila guardada y las reglas de produccion las repite el servidor
// (AsistentePermisosMapper / UsuariosController.GuardarPermisosDelAsistente): lo de este archivo es comodidad de pantalla, no seguridad.
(function () {
    'use strict';

    var raiz = document.getElementById('usrApp');
    var formulario = document.getElementById('formAsistente');
    var bloqueDatos = document.getElementById('asistenteDatos');
    if (!raiz || !formulario || !bloqueDatos) return;

    var datos = JSON.parse(bloqueDatos.textContent);
    var esFicha = raiz.getAttribute('data-modo') === 'ficha';
    var pasoInicial = parseInt(raiz.getAttribute('data-paso-inicial'), 10) || 1;

    // Todos los permisos en una lista plana + indice por idForm.
    var permisos = [];
    var permisoPorId = {};
    datos.modulos.forEach(function (modulo) {
        modulo.items.forEach(function (item) {
            item.ui = { otroVer: false, otroEditar: false }; // "Otro..." elegido en la lista de dias (solo de pantalla)
            permisos.push(item);
            permisoPorId[item.idForm] = item;
        });
    });

    var moduloActivo = datos.modulos.length ? datos.modulos[0].nombre : '';
    var filtroTexto = '';
    var guardando = false;

    function porId(id) { return document.getElementById(id); }
    function region(nombre) { return raiz.querySelector('[data-r="' + nombre + '"]'); }

    function escapar(texto) {
        return String(texto == null ? '' : texto)
            .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    function tipoUsuario() {
        var marcado = formulario.querySelector('input[name="TipoUsuario"]:checked');
        return marcado ? marcado.value : 'empleado';
    }
    function esAdmin() { return tipoUsuario() === 'admin'; }
    function esProduccion() { return tipoUsuario() === 'produccion'; }

    // Una cuenta de produccion nunca tiene los permisos bloqueados: se ven deshabilitados y valen "sin acceso".
    function estaBloqueado(item) { return esProduccion() && item.bloqueadoProduccion; }
    function nivelEfectivo(item) { return estaBloqueado(item) ? 0 : item.estado.nivel; }
    function esDeUnNivel(item) { return item.tipo !== 'VerYModificar'; }

    // ---------- Avisos con "Deshacer" ----------
    var temporizadorAviso = null;
    function mostrarAviso(texto, deshacer) {
        var caja = porId('usrToast');
        caja.textContent = texto + ' ';
        if (deshacer) {
            var boton = document.createElement('button');
            boton.type = 'button';
            boton.textContent = 'Deshacer';
            boton.addEventListener('click', function () { deshacer(); caja.classList.remove('on'); });
            caja.appendChild(boton);
        }
        caja.classList.add('on');
        window.clearTimeout(temporizadorAviso);
        temporizadorAviso = window.setTimeout(function () { caja.classList.remove('on'); }, 7000);
    }

    // ---------- Instantaneas del estado (para "Deshacer" y para contar cambios) ----------
    function claveEstado(estado) {
        return JSON.stringify([estado.nivel, estado.diasVer, !!estado.todoVer, estado.diasEditar, !!estado.todoEditar, estado.soloPropios !== false, !!estado.personalizado]);
    }
    function instantanea() {
        return permisos.map(function (item) { return JSON.parse(JSON.stringify(item.estado)); });
    }
    function restaurar(copia) {
        permisos.forEach(function (item, i) { item.estado = JSON.parse(JSON.stringify(copia[i])); });
        dibujarEditor();
    }
    function cuantosCambiaron(copia) {
        var n = 0;
        permisos.forEach(function (item, i) { if (claveEstado(item.estado) !== claveEstado(copia[i])) n++; });
        return n;
    }
    var estadoInicial = instantanea();

    // ---------- Editor de permisos ----------
    var OPCIONES_DIAS = [['0', 'Solo hoy'], ['7', '7 días'], ['14', '14 días'], ['30', '30 días'], ['90', '90 días'], ['todo', 'Todo el historial'], ['otro', 'Otro…']];

    function valorDias(dias, todo, forzarOtro) {
        if (todo) return 'todo';
        if (forzarOtro) return 'otro';
        var texto = String(dias);
        return ['0', '7', '14', '30', '90'].indexOf(texto) >= 0 ? texto : 'otro';
    }

    function selectorDias(campo, dias, todo, forzarOtro) {
        var actual = valorDias(dias, todo, forzarOtro);
        var html = '<select class="usr-sel-dias" data-campo="' + campo + '" aria-label="Días hacia atrás">';
        OPCIONES_DIAS.forEach(function (o) { html += '<option value="' + o[0] + '"' + (o[0] === actual ? ' selected' : '') + '>' + o[1] + '</option>'; });
        html += '</select>';
        if (actual === 'otro') {
            html += '<input type="number" min="0" max="36500" class="usr-num-dias" data-campo="' + campo + '" value="' + dias + '" aria-label="Cantidad de días" /> días';
        }
        return html + ' atrás';
    }

    function textoPermiso(item, nivel) {
        var texto = item.textoVer || '';
        if (nivel === 2 && item.textoModificar) texto += ' ' + item.textoModificar;
        else if (nivel === 0 && item.tipo === 'VerYModificar' && item.textoModificar) texto += ' ' + item.textoModificar;
        return texto;
    }

    function dibujarFila(item) {
        var estado = item.estado;
        var nivel = nivelEfectivo(item);
        var bloqueado = estaBloqueado(item);
        var maximo = esDeUnNivel(item) ? 1 : 2;
        var html = '<div class="usr-fila' + (nivel === 0 ? ' off' : '') + '" data-id-form="' + item.idForm + '"><div>' +
            '<span class="ti">' + escapar(item.titulo) + '</span><div class="tx">';

        if (estado.personalizado && !bloqueado) {
            html += 'Este usuario tiene una combinación que el editor no representa (por ejemplo, modificar sin ver). Se conserva tal cual al guardar; ' +
                'para cambiarla usá la vista avanzada, o tocá «Reemplazar» para elegir de nuevo.';
        } else {
            html += escapar(textoPermiso(item, nivel));
        }
        html += '</div></div><div>';

        if (estado.personalizado && !bloqueado) {
            html += '<span class="usr-chip warn">Valor especial guardado</span> <button type="button" class="usr-btn usr-btn-sm usr-reemplazar">Reemplazar</button>';
        } else {
            html += '<div class="usr-seg" role="group" aria-label="Nivel de ' + escapar(item.titulo) + '">';
            for (var n = 0; n <= maximo; n++) {
                var etiqueta = n === 0 ? 'Sin acceso' : esDeUnNivel(item) ? 'Sí' : (n === 1 ? 'Ver' : 'Ver y modificar');
                html += '<button type="button" class="usr-nivel' + (nivel === n ? ' on' : '') + (n === 0 ? ' ninguno' : '') + '" data-nivel="' + n + '" aria-pressed="' + (nivel === n) + '"' +
                    (bloqueado ? ' disabled' : '') + '>' + etiqueta + '</button>';
            }
            html += '</div>';
        }
        html += '</div>';

        if (bloqueado) {
            html += '<div class="usr-aviso info">ⓘ <span>No está disponible en cuentas de producción.</span></div>';
        } else if (!estado.personalizado) {
            var controles = '';
            if (item.usaDiasVer && nivel >= 1)
                controles += '<span class="usr-ctrl">Ve hasta ' + selectorDias('ver', estado.diasVer, estado.todoVer, item.ui.otroVer) + '</span>';
            if (item.usaDiasEditar && nivel >= 2)
                controles += '<span class="usr-ctrl">Modifica hasta ' + selectorDias('editar', estado.diasEditar, estado.todoEditar, item.ui.otroEditar) + '</span>';
            if (item.usaAlcance && nivel >= 2) {
                controles += '<span class="usr-ctrl">Qué registros <span class="usr-seg" role="group" aria-label="Alcance">' +
                    '<button type="button" class="usr-alcance' + (estado.soloPropios !== false ? ' on' : '') + '" data-propios="true">Solo los suyos</button>' +
                    '<button type="button" class="usr-alcance' + (estado.soloPropios === false ? ' on' : '') + '" data-propios="false">Todos</button></span></span>';
            }
            if (controles) html += '<div class="usr-ctrls">' + controles + '</div>';

            // El aviso de un permiso con riesgo al modificar solo aparece al darlo; el resto, apenas hay acceso.
            var verAviso = item.aviso && (item.riesgoAlModificar ? nivel === 2 : nivel >= 1);
            if (verAviso) html += '<div class="usr-aviso">ⓘ <span>' + escapar(item.aviso) + '</span></div>';
        }
        return html + '</div>';
    }

    function activosDe(modulo) {
        return modulo.items.filter(function (i) { return nivelEfectivo(i) > 0; }).length;
    }

    function dibujarModulos() {
        var html = '';
        datos.modulos.forEach(function (modulo) {
            var activos = activosDe(modulo);
            html += '<button type="button" data-modulo="' + escapar(modulo.nombre) + '" class="' + (modulo.nombre === moduloActivo && !filtroTexto ? 'on' : '') + '">' +
                escapar(modulo.nombre) + '<span class="c' + (activos === modulo.items.length ? ' full' : '') + '">' + activos + '/' + modulo.items.length + '</span></button>';
        });
        region('mods').innerHTML = html;
    }

    function dibujarLista() {
        var texto = filtroTexto;
        var lista;
        if (texto) {
            lista = datos.modulos.map(function (m) {
                return { nombre: m.nombre, items: m.items.filter(function (i) {
                    return (i.titulo + ' ' + i.textoVer + ' ' + (i.textoModificar || '') + ' ' + (i.ayuda || '')).toLowerCase().indexOf(texto) >= 0;
                }), total: m.items.length, todos: m };
            }).filter(function (m) { return m.items.length; });
        } else {
            lista = datos.modulos.filter(function (m) { return m.nombre === moduloActivo; })
                .map(function (m) { return { nombre: m.nombre, items: m.items, total: m.items.length, todos: m }; });
        }
        var html = '';
        lista.forEach(function (m) {
            html += '<div class="usr-mod-titulo"><h3>' + escapar(m.nombre) + '</h3>' +
                (texto ? '' : '<span class="usr-crumb">' + activosDe(m.todos) + ' de ' + m.total + ' con acceso</span>') + '</div>';
            m.items.forEach(function (item) { html += dibujarFila(item); });
        });
        if (!lista.length) html = '<div class="usr-sec usr-crumb">Ningún permiso coincide con «' + escapar(texto) + '».</div>';
        region('lista').innerHTML = html;
    }

    // Misma regla que VentasController.TieneAlgunPermisoPOS (ver "Da acceso al POS" en CatalogoPermisos).
    function daAccesoAlPos() {
        if (esAdmin()) return true;
        return permisos.some(function (item) {
            var nivel = nivelEfectivo(item);
            if (nivel >= 1 && item.posConVer) return true;
            if (nivel >= 2 && item.posConModificar) return true;
            return esDeUnNivel(item) && nivel >= 1 && item.posConModificar;
        });
    }

    function dibujarResumenLateral() {
        var pos = daAccesoAlPos();
        var html = '<h4>Lo que va a poder hacer</h4><div class="usr-big">Entra al POS <span class="usr-chip' + (pos ? ' ok' : '') + '">' + (pos ? 'Sí' : 'No') + '</span></div>';
        if (esAdmin()) {
            html += '<div class="usr-help">Es administrador: puede hacer todo en su empresa. Los permisos de esta pantalla no se usan (se dejan como estaban).</div>';
        } else {
            var lineas = '';
            datos.modulos.forEach(function (modulo) {
                var activos = modulo.items.filter(function (i) { return nivelEfectivo(i) > 0 || (i.estado.personalizado && !estaBloqueado(i)); });
                if (!activos.length) return;
                lineas += '<li><b>' + escapar(modulo.nombre) + '</b>: ' + activos.map(function (i) {
                    var n = nivelEfectivo(i);
                    return escapar(i.titulo) + (i.estado.personalizado ? ' (valor especial)' : (n === 2 ? ' (modifica)' : ''));
                }).join(', ') + '</li>';
            });
            html += lineas ? '<ul>' + lineas + '</ul>' : '<div class="usr-crumb">Todavía sin permisos.</div>';
            var riesgos = [];
            permisos.forEach(function (i) { if (i.riesgoAlModificar && nivelEfectivo(i) === 2) riesgos.push(i.riesgoAlModificar); });
            if (riesgos.length) {
                html += '<h4 style="margin-top:.8rem">Atención</h4><ul class="atencion">' +
                    riesgos.map(function (r) { return '<li>' + escapar(r) + '</li>'; }).join('') + '</ul>';
            }
        }
        region('res').innerHTML = html;

        // Cabecera de la ficha y pestana: se mantienen al dia con lo elegido.
        var chip = porId('usrChipPos');
        if (chip) { chip.className = 'usr-chip' + (pos ? ' ok' : ''); chip.textContent = pos ? 'Entra al POS' : 'No entra al POS'; }
        var cuenta = porId('usrCntPerm');
        if (cuenta) cuenta.textContent = permisos.filter(function (i) { return nivelEfectivo(i) > 0; }).length;
    }

    function dibujarBanner() {
        var texto = '';
        if (esAdmin()) texto = 'Es un administrador: no necesita permisos (puede hacer todo en su empresa). Lo que elijas acá no se guarda.';
        else if (esProduccion()) texto = 'Es una cuenta de producción: Ventas, POS, Finanzas, Fórmulas y Personal no están disponibles, sin importar lo que se elija.';
        region('banner').innerHTML = texto ? '<div class="usr-nota warn">' + escapar(texto) + '</div>' : '';
        var contenedor = region('contenedor');
        if (contenedor) { contenedor.style.opacity = esAdmin() ? '.5' : ''; contenedor.style.pointerEvents = esAdmin() ? 'none' : ''; }
    }

    function dibujarEditor() {
        var editor = porId('usrEditor');
        if (!editor) return;
        if (!editor.firstChild) {
            var perfiles = datos.perfiles.map(function (p) {
                return '<button type="button" class="usr-btn usr-btn-sm usr-perfil" data-perfil="' + escapar(p.clave) + '" title="' + escapar(p.descripcion) + '">Perfil ' + escapar(p.nombre) + '</button>';
            }).join('');
            var copiar = '<option value="">Copiar de otro usuario…</option>' + (datos.copiar || []).map(function (u) {
                return '<option value="' + escapar(u.id) + '">' + escapar(u.texto) + '</option>';
            }).join('');
            editor.innerHTML =
                '<div class="usr-perm" data-r="contenedor">' +
                '<div class="usr-card usr-mods" data-r="mods"></div>' +
                '<div class="usr-card" style="min-width:0"><div data-r="banner"></div>' +
                '<div class="usr-atajos"><span class="t">Atajos</span>' + perfiles +
                '<select class="usr-input usr-btn-sm" id="usrCopiarDe" aria-label="Copiar de otro usuario">' + copiar + '</select>' +
                '<button type="button" class="usr-btn usr-btn-sm usr-btn-ghost" id="usrLimpiar">Quitar todos</button>' +
                '<input type="search" class="usr-input usr-buscar" id="usrBuscarPerm" placeholder="Buscar permiso…" aria-label="Buscar permiso" /></div>' +
                '<div data-r="lista"></div></div>' +
                '<div class="usr-card usr-resumen" data-r="res"></div></div>';
        }
        dibujarBanner();
        dibujarModulos();
        dibujarLista();
        dibujarResumenLateral();
        actualizarCambios();
    }

    // Redibuja solo una fila (para no perder el foco ni la posicion) y actualiza contadores y resumen.
    function redibujarFila(item) {
        var fila = region('lista').querySelector('.usr-fila[data-id-form="' + item.idForm + '"]');
        if (fila) fila.outerHTML = dibujarFila(item);
        dibujarModulos();
        dibujarResumenLateral();
        actualizarCambios();
    }

    function itemDeElemento(elemento) {
        var fila = elemento.closest('.usr-fila');
        return fila ? permisoPorId[parseInt(fila.getAttribute('data-id-form'), 10)] : null;
    }

    function acotarDias(texto) {
        var valor = parseInt(texto, 10);
        if (isNaN(valor) || valor < 0) valor = 0;
        return Math.min(valor, 36500);
    }

    var editorRaiz = porId('usrEditor');
    if (editorRaiz) {
        editorRaiz.addEventListener('click', function (evento) {
            var objetivo = evento.target;
            var modulo = objetivo.closest('[data-modulo]');
            if (modulo) {
                moduloActivo = modulo.getAttribute('data-modulo');
                filtroTexto = '';
                var buscador = porId('usrBuscarPerm');
                if (buscador) buscador.value = '';
                dibujarModulos();
                dibujarLista();
                return;
            }
            var nivel = objetivo.closest('.usr-nivel');
            if (nivel) {
                var item = itemDeElemento(nivel);
                if (!item || estaBloqueado(item)) return;
                item.estado.nivel = parseInt(nivel.getAttribute('data-nivel'), 10);
                redibujarFila(item);
                return;
            }
            var alcance = objetivo.closest('.usr-alcance');
            if (alcance) {
                var itemAlcance = itemDeElemento(alcance);
                if (!itemAlcance) return;
                itemAlcance.estado.soloPropios = alcance.getAttribute('data-propios') === 'true';
                redibujarFila(itemAlcance);
                return;
            }
            var reemplazar = objetivo.closest('.usr-reemplazar');
            if (reemplazar) {
                var itemEspecial = itemDeElemento(reemplazar);
                if (!itemEspecial) return;
                itemEspecial.estado.personalizado = false;
                itemEspecial.estado.nivel = 0;
                redibujarFila(itemEspecial);
                return;
            }
            var perfil = objetivo.closest('.usr-perfil');
            if (perfil) { aplicarPerfil(perfil.getAttribute('data-perfil')); return; }
            if (objetivo.closest('#usrLimpiar')) quitarTodos();
        });

        editorRaiz.addEventListener('change', function (evento) {
            var objetivo = evento.target;
            if (objetivo.id === 'usrCopiarDe') { copiarDe(objetivo); return; }
            if (!objetivo.classList.contains('usr-sel-dias')) return;
            var item = itemDeElemento(objetivo);
            if (!item) return;
            var campo = objetivo.getAttribute('data-campo'); // "ver" | "editar"
            var campoDias = campo === 'ver' ? 'diasVer' : 'diasEditar';
            var campoTodo = campo === 'ver' ? 'todoVer' : 'todoEditar';
            var campoOtro = campo === 'ver' ? 'otroVer' : 'otroEditar';
            if (objetivo.value === 'todo') { item.estado[campoTodo] = true; item.ui[campoOtro] = false; }
            else if (objetivo.value === 'otro') { item.estado[campoTodo] = false; item.ui[campoOtro] = true; }
            else { item.estado[campoTodo] = false; item.ui[campoOtro] = false; item.estado[campoDias] = parseInt(objetivo.value, 10); }
            redibujarFila(item);
        });

        editorRaiz.addEventListener('input', function (evento) {
            var objetivo = evento.target;
            if (objetivo.id === 'usrBuscarPerm') {
                filtroTexto = objetivo.value.trim().toLowerCase();
                dibujarModulos();
                dibujarLista();
                return;
            }
            if (!objetivo.classList.contains('usr-num-dias')) return;
            var item = itemDeElemento(objetivo);
            if (!item) return;
            // Se actualiza el valor sin redibujar la fila (para no perder el foco mientras se escribe).
            var valor = acotarDias(objetivo.value);
            if (objetivo.getAttribute('data-campo') === 'ver') item.estado.diasVer = valor; else item.estado.diasEditar = valor;
            dibujarResumenLateral();
            actualizarCambios();
        });
    }

    // ---------- Atajos: perfiles, copiar, quitar todos ----------
    function copiarEstado(destino, origen) {
        destino.nivel = origen.nivel;
        destino.diasVer = origen.diasVer;
        destino.todoVer = !!origen.todoVer;
        destino.diasEditar = origen.diasEditar;
        destino.todoEditar = !!origen.todoEditar;
        destino.soloPropios = origen.soloPropios !== false;
        destino.personalizado = false;
    }

    function aplicarPerfil(clave) {
        var perfil = datos.perfiles.filter(function (p) { return p.clave === clave; })[0];
        if (!perfil) return;
        var antes = instantanea();
        // El perfil define todo lo que habilita: lo demas queda sin acceso (los valores especiales no se tocan).
        permisos.forEach(function (item) { if (!item.estado.personalizado) item.estado.nivel = 0; });
        perfil.estados.forEach(function (estado) {
            var item = permisoPorId[estado.idForm];
            if (item) copiarEstado(item.estado, estado);
        });
        dibujarEditor();
        mostrarAviso('Se aplicó el perfil «' + perfil.nombre + '»: cambian ' + cuantosCambiaron(antes) + ' permisos. Ajustá lo que haga falta.', function () { restaurar(antes); });
    }

    function copiarDe(seleccion) {
        var id = seleccion.value;
        if (!id) return;
        var nombreOrigen = seleccion.options[seleccion.selectedIndex].text;
        seleccion.value = '';
        var antes = instantanea();
        fetch(raiz.getAttribute('data-url-copiar') + '?id=' + encodeURIComponent(id), { credentials: 'same-origin' })
            .then(function (respuesta) {
                if (!respuesta.ok) throw new Error('El servidor respondió ' + respuesta.status);
                return respuesta.json();
            })
            .then(function (resultado) {
                // Copiar reemplaza lo elegido: lo que el usuario origen no tiene queda sin acceso.
                permisos.forEach(function (item) { if (!item.estado.personalizado) item.estado.nivel = 0; });
                (resultado.estados || []).forEach(function (estado) {
                    var item = permisoPorId[estado.idForm];
                    if (item) copiarEstado(item.estado, estado);
                });
                dibujarEditor();
                var texto = 'Se copiaron los permisos de ' + (resultado.nombre || nombreOrigen) + ': cambian ' + cuantosCambiaron(antes) + ' permisos.';
                if (resultado.omitidos) texto += ' ' + resultado.omitidos + ' con valores especiales no se copiaron.';
                mostrarAviso(texto, function () { restaurar(antes); });
            })
            .catch(function (error) { mostrarAviso('No se pudieron copiar los permisos: ' + error.message); });
    }

    function quitarTodos() {
        if (!permisos.some(function (i) { return i.estado.nivel > 0 || i.estado.personalizado; })) return;
        var antes = instantanea();
        permisos.forEach(function (item) { item.estado.nivel = 0; item.estado.personalizado = false; });
        dibujarEditor();
        mostrarAviso('Se quitaron todos los permisos.', function () { restaurar(antes); });
    }

    // ---------- Cambios sin guardar (solo ficha) ----------
    // Valores de los campos del formulario (sin los ocultos que arma el propio script ni el token): para contar cuales cambiaron.
    function leerCampos() {
        var valores = {};
        formulario.querySelectorAll('input, select, textarea').forEach(function (el) {
            var nombre = el.name;
            if (!nombre || el.type === 'hidden' || nombre.indexOf('Permisos[') === 0) return;
            if (el.type === 'radio') { if (el.checked) valores[nombre] = el.value; }
            else if (el.type === 'checkbox') valores[nombre] = el.checked;
            else valores[nombre] = el.value;
        });
        return valores;
    }
    var camposIniciales = leerCampos();

    function contarCambios() {
        var actuales = leerCampos();
        var n = 0;
        Object.keys(actuales).forEach(function (k) { if (actuales[k] !== camposIniciales[k]) n++; });
        n += cuantosCambiaron(estadoInicial);
        return n;
    }

    function actualizarCambios() {
        if (!esFicha) return;
        var n = contarCambios();
        var barra = porId('usrSavebar');
        barra.classList.toggle('on', n > 0);
        porId('usrSaveTxt').textContent = n === 1 ? '1 cambio sin guardar' : n + ' cambios sin guardar';
    }

    // ---------- Tipo de usuario, ojito de la clave ----------
    function sincronizarTipos() {
        formulario.querySelectorAll('#usrSegTipo label, #usrTipos .usr-tipo').forEach(function (etiqueta) {
            var radio = etiqueta.querySelector('input[type=radio]');
            etiqueta.classList.toggle('on', !!(radio && radio.checked));
        });
        var chip = porId('usrChipTipo');
        if (chip) {
            var tipo = tipoUsuario();
            chip.textContent = tipo === 'admin' ? 'Administrador' : tipo === 'produccion' ? 'Producción' : 'Empleado';
            chip.className = 'usr-chip ' + (tipo === 'admin' ? 'ok' : tipo === 'produccion' ? 'warn' : 'pri');
        }
    }

    formulario.addEventListener('change', function (evento) {
        var objetivo = evento.target;
        if (objetivo.name === 'TipoUsuario') {
            sincronizarTipos();
            if (porId('usrEditor') && porId('usrEditor').firstChild) dibujarEditor();
            if (!esFicha) marcarPasos();
        }
        actualizarCambios();
    });
    formulario.addEventListener('input', function (evento) {
        if (evento.target.closest && evento.target.closest('#usrEditor')) return; // los campos del editor ya actualizan por su cuenta
        actualizarCambios();
    });

    raiz.addEventListener('click', function (evento) {
        var ojo = evento.target.closest('.usr-ojo');
        if (ojo) {
            var campo = ojo.parentElement.querySelector('input');
            campo.type = campo.type === 'password' ? 'text' : 'password';
        }
    });

    // ---------- Validacion de los datos en el servidor (mismas reglas que el guardado) ----------
    function validarDatos() {
        var url = raiz.getAttribute('data-url-validar');
        var cuerpo = new FormData(formulario);
        raiz.querySelectorAll('[data-errores-generales]').forEach(function (c) { c.classList.add('d-none'); });
        raiz.querySelectorAll('[data-error-de]').forEach(function (nodo) { nodo.textContent = ''; });

        return fetch(url, { method: 'POST', body: cuerpo, credentials: 'same-origin' })
            .then(function (respuesta) {
                if (!respuesta.ok) throw new Error('El servidor respondió ' + respuesta.status);
                return respuesta.json();
            })
            .then(function (resultado) {
                if (resultado.ok) return true;
                var sinCampo = [];
                Object.keys(resultado.errores || {}).forEach(function (campo) {
                    var nodo = raiz.querySelector('[data-error-de="' + campo + '"]');
                    if (nodo) nodo.textContent = resultado.errores[campo];
                    else sinCampo.push(resultado.errores[campo]);
                });
                if (sinCampo.length) {
                    raiz.querySelectorAll('[data-errores-generales]').forEach(function (c) { c.textContent = sinCampo.join(' '); c.classList.remove('d-none'); });
                }
                // En la ficha, el error se muestra en la pestana donde esta el campo.
                if (esFicha) {
                    var primero = raiz.querySelector('[data-error-de]:not(:empty)');
                    var panel = primero ? primero.closest('[data-pane]') : null;
                    mostrarTab(panel ? panel.getAttribute('data-pane') : 'datos');
                }
                return false;
            })
            .catch(function (error) {
                raiz.querySelectorAll('[data-errores-generales]').forEach(function (c) { c.textContent = 'No se pudo validar los datos: ' + error.message; c.classList.remove('d-none'); });
                return false;
            });
    }

    // ---------- Resumen final (alta) ----------
    function describirDias(dias, todo) { return todo ? 'todo el historial' : (dias === 0 ? 'solo hoy' : 'hasta ' + dias + ' días atrás'); }

    function describir(item) {
        var estado = item.estado;
        var nivel = nivelEfectivo(item);
        var detalles = [];
        if (item.usaDiasVer && nivel >= 1) detalles.push('ve ' + describirDias(estado.diasVer, estado.todoVer));
        if (item.usaDiasEditar && nivel >= 2) detalles.push('modifica ' + describirDias(estado.diasEditar, estado.todoEditar));
        if (item.usaAlcance && nivel >= 2) detalles.push(estado.soloPropios === false ? 'todo lo de todos' : 'solo lo que cargó él');
        return '<strong>' + escapar(item.titulo) + '</strong>' + (nivel === 1 && !esDeUnNivel(item) ? ' (ver)' : '') + ': ' + escapar(textoPermiso(item, nivel)) +
            (detalles.length ? ' <span class="usr-crumb">(' + escapar(detalles.join('; ')) + ')</span>' : '');
    }

    function dibujarResumenFinal() {
        var nombre = formulario.querySelector('[name="Nombre"]').value.trim();
        var usuario = formulario.querySelector('[name="Usuario"]').value.trim();
        var sucursal = formulario.querySelector('[name="IdSucursalUser"]');
        var tipo = tipoUsuario();
        var etiquetaTipo = tipo === 'admin' ? 'Administrador' : tipo === 'produccion' ? 'Cuenta de producción' : 'Empleado';
        var html = '<div class="usr-card" style="padding:.8rem;margin-bottom:.8rem"><b>' + escapar(nombre || '(sin nombre)') + '</b>' +
            '<div class="usr-crumb">' + escapar(usuario) + ' · ' + etiquetaTipo + (sucursal && sucursal.value !== '0' ? ' · Sucursal ' + escapar(sucursal.options[sucursal.selectedIndex].text) : '') + '</div></div>';

        if (esAdmin()) {
            html += '<div class="alert alert-info py-2">Es <strong>administrador</strong>: puede hacer todo en su empresa, sin necesidad de permisos.</div>';
        } else {
            if (esProduccion()) html += '<div class="alert alert-warning py-2">Es una <strong>cuenta de producción</strong> (PC compartida): nunca tiene Ventas, POS, Finanzas, Fórmulas ni Personal.</div>';
            var hayAlguno = false;
            html += '<div>Entra al POS: <span class="usr-chip' + (daAccesoAlPos() ? ' ok' : '') + '">' + (daAccesoAlPos() ? 'Sí' : 'No') + '</span></div>';
            datos.modulos.forEach(function (modulo) {
                var activos = modulo.items.filter(function (i) { return nivelEfectivo(i) > 0 || (i.estado.personalizado && !estaBloqueado(i)); });
                if (!activos.length) return;
                hayAlguno = true;
                html += '<h3>' + escapar(modulo.nombre) + '</h3><ul>';
                activos.forEach(function (item) {
                    html += '<li>' + (item.estado.personalizado ? '<strong>' + escapar(item.titulo) + '</strong>: valor especial guardado (se conserva)' : describir(item)) + '</li>';
                });
                html += '</ul>';
            });
            if (!hayAlguno) html += '<div class="alert alert-warning py-2 mt-2">No tiene ningún permiso: al ingresar solo verá el inicio. Podés crearlo así y darle permisos después.</div>';
        }
        porId('usrResumenFinal').innerHTML = html;
    }

    // ---------- Ficha: pestanas ----------
    var NOMBRES_TAB = { 1: 'datos', 2: 'ingreso', 3: 'permisos' };
    function mostrarTab(nombre) {
        raiz.querySelectorAll('[data-pane]').forEach(function (panel) { panel.classList.toggle('on', panel.getAttribute('data-pane') === nombre); });
        raiz.querySelectorAll('#usrTabs [data-tab]').forEach(function (boton) {
            var activo = boton.getAttribute('data-tab') === nombre;
            boton.classList.toggle('on', activo);
            boton.setAttribute('aria-selected', activo ? 'true' : 'false');
        });
        if (nombre === 'permisos') dibujarEditor();
    }

    // ---------- Alta: pasos ----------
    var pasoActual = 1;
    function siguienteDe(paso) { return paso === 1 && esAdmin() ? 3 : paso + 1; }
    function anteriorDe(paso) { return paso === 3 && esAdmin() ? 1 : paso - 1; }

    function marcarPasos() {
        raiz.querySelectorAll('#usrPasos [data-paso-ind]').forEach(function (nodo) {
            var n = parseInt(nodo.getAttribute('data-paso-ind'), 10);
            nodo.classList.toggle('on', n === pasoActual);
            nodo.classList.toggle('hecho', n < pasoActual);
            nodo.classList.toggle('omitido', n === 2 && esAdmin());
        });
    }

    function mostrarPaso(numero) {
        pasoActual = numero;
        raiz.querySelectorAll('[data-paso-cuerpo]').forEach(function (cuerpo) { cuerpo.classList.toggle('on', parseInt(cuerpo.getAttribute('data-paso-cuerpo'), 10) === numero); });
        marcarPasos();
        porId('usrAtras').style.visibility = numero === 1 ? 'hidden' : 'visible';
        porId('usrSiguiente').classList.toggle('d-none', numero === 3);
        porId('usrCrear').classList.toggle('d-none', numero !== 3);
        if (numero === 2) dibujarEditor();
        if (numero === 3) dibujarResumenFinal();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    function irAlSiguiente() {
        if (pasoActual === 1) {
            validarDatos().then(function (ok) { if (ok) mostrarPaso(siguienteDe(1)); });
            return;
        }
        mostrarPaso(siguienteDe(pasoActual));
    }

    // ---------- Guardar ----------
    function volcarPermisosAlFormulario() {
        var destino = porId('usrPermisosPost');
        var html = '';
        permisos.forEach(function (item, indice) {
            var estado = item.estado;
            // Una cuenta de produccion postea "sin acceso" en lo bloqueado (el servidor lo fuerza igual).
            var nivel = nivelEfectivo(item);
            var prefijo = 'Permisos[' + indice + '].';
            html += '<input type="hidden" name="' + prefijo + 'IdForm" value="' + item.idForm + '" />' +
                '<input type="hidden" name="' + prefijo + 'Nivel" value="' + nivel + '" />' +
                '<input type="hidden" name="' + prefijo + 'DiasVer" value="' + estado.diasVer + '" />' +
                '<input type="hidden" name="' + prefijo + 'TodoVer" value="' + !!estado.todoVer + '" />' +
                '<input type="hidden" name="' + prefijo + 'DiasEditar" value="' + estado.diasEditar + '" />' +
                '<input type="hidden" name="' + prefijo + 'TodoEditar" value="' + !!estado.todoEditar + '" />' +
                '<input type="hidden" name="' + prefijo + 'SoloPropios" value="' + (estado.soloPropios !== false) + '" />' +
                '<input type="hidden" name="' + prefijo + 'Personalizado" value="' + (!!estado.personalizado && !estaBloqueado(item)) + '" />';
        });
        destino.innerHTML = html;
    }

    function enviar() {
        guardando = true;
        volcarPermisosAlFormulario();
        var boton = porId(esFicha ? 'usrGuardar' : 'usrCrear');
        window.setTimeout(function () { boton.disabled = true; }, 0); // evita el doble envio
        formulario.submit();
    }

    formulario.addEventListener('submit', function (evento) {
        evento.preventDefault();
        if (guardando) return;
        // Los datos se validan en el servidor antes de enviar: el error se ve junto al campo, sin perder lo cargado.
        validarDatos().then(function (ok) {
            if (ok) { enviar(); return; }
            if (!esFicha) mostrarPaso(1);
        });
    });

    // Enter en un campo no debe guardar sin pasar por el resumen: en el alta avanza al siguiente paso; en la ficha no hace nada.
    formulario.addEventListener('keydown', function (evento) {
        if (evento.key === 'Enter' && evento.target.tagName === 'INPUT' && evento.target.type !== 'submit' && evento.target.type !== 'radio') {
            evento.preventDefault();
            if (!esFicha && pasoActual < 3) irAlSiguiente();
        }
    });

    // Aviso si se intenta salir de la ficha con cambios sin guardar.
    window.addEventListener('beforeunload', function (evento) {
        if (esFicha && !guardando && contarCambios() > 0) { evento.preventDefault(); evento.returnValue = ''; }
    });

    // ---------- Arranque ----------
    sincronizarTipos();
    if (esFicha) {
        raiz.querySelectorAll('#usrTabs [data-tab]').forEach(function (boton) {
            boton.addEventListener('click', function () { mostrarTab(boton.getAttribute('data-tab')); });
        });
        porId('usrDescartar').addEventListener('click', function () { window.location.reload(); });
        var inicial = window.location.hash === '#permisos' ? 'permisos' : (NOMBRES_TAB[pasoInicial] || 'datos');
        mostrarTab(inicial);
        dibujarEditor(); // deja el contador de permisos y el chip "Entra al POS" al dia aunque se abra otra pestana
    } else {
        porId('usrSiguiente').addEventListener('click', irAlSiguiente);
        porId('usrAtras').addEventListener('click', function () { mostrarPaso(anteriorDe(pasoActual)); });
        mostrarPaso(1);
    }
})();
