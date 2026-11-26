/*
 * POS sin conexion: logica de la pagina pos-offline.html (2026-11-26). Flujo:
 *   catalogo guardado? -> quien vende (PIN) -> caja (la abierta que informo el servidor, o apertura sin conexion)
 *   -> venta (codigo / F10 / balanza, Consumidor Final, sin descuentos) -> cobro (todas las formas menos Cta Cte)
 *   -> la venta va a la cola (IndexedDB) y sale el ticket. Cuando vuelve la conexion se suben solas (PosOfflineSync).
 * Nada de esto toca el servidor mientras no hay conexion. Detalle: docs/03-modulos/ventas-y-pos.md "POS sin conexion".
 */
(function (window, document) {
    'use strict';

    var Db = window.PosOfflineDb;
    var Red = window.PosOfflineRed;
    var Buscar = window.PosOfflineBuscar;
    var Pin = window.PosOfflinePin;
    var Sync = window.PosOfflineSync;
    var Ticket = window.PosOfflineTicket;

    // Sesion offline: vence a las 12 h, igual que la cookie del servidor.
    var HORAS_SESION = 12;
    var SEGUNDOS_CHEQUEO_RED = 20;
    var FORMAS = [
        { clave: 'Efectivo', texto: 'Efectivo', tecla: '1' },
        { clave: 'Debito', texto: 'Débito', tecla: '2' },
        { clave: 'Credito', texto: 'Crédito', tecla: '3' },
        { clave: 'Qr', texto: 'QR', tecla: '4' },
        { clave: 'Transferencia', texto: 'Transferencia', tecla: '5' }
    ];

    var estado = {
        catalogo: null,
        config: {},
        sesion: null,
        usuarioElegido: null,
        carrito: [],
        causa: null,
        online: false,
        cobro: null
    };

    function $(id) { return document.getElementById(id); }
    function mostrar(id, si) { $(id).hidden = !si; }

    function mensaje(id, texto, ok) {
        var el = $(id);
        el.textContent = texto || '';
        el.classList.toggle('text-success', !!ok);
        el.classList.toggle('text-danger', !ok);
    }

    function dinero(n) {
        return '$ ' + Number(n || 0).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // Mismo redondeo que pos-forma-pago-precios.js (roundPrice).
    function redondear(n) { return Math.round((Number(n) + Number.EPSILON) * 100) / 100; }

    // Acepta "1.234,56", "1234,56" o "1234.56".
    function numero(texto) {
        var t = String(texto == null ? '' : texto).trim().replace(/[^0-9,.\-]/g, '');
        if (t.indexOf(',') >= 0 && t.indexOf('.') >= 0) t = t.replace(/\./g, '').replace(',', '.');
        else if (t.indexOf(',') >= 0) t = t.replace(',', '.');
        var n = parseFloat(t);
        return isFinite(n) ? n : NaN;
    }

    function uuid() {
        if (window.crypto && window.crypto.randomUUID) return window.crypto.randomUUID();
        var b = window.crypto.getRandomValues(new Uint8Array(16));
        b[6] = (b[6] & 0x0f) | 0x40; b[8] = (b[8] & 0x3f) | 0x80;
        var h = Array.prototype.map.call(b, function (x) { return ('0' + x.toString(16)).slice(-2); }).join('');
        return h.substr(0, 8) + '-' + h.substr(8, 4) + '-' + h.substr(12, 4) + '-' + h.substr(16, 4) + '-' + h.substr(20);
    }

    // Fecha local "yyyy-MM-ddTHH:mm:ss" sin zona: el servidor la toma como hora local (la de la venta).
    function fechaLocalIso(d) {
        function p(n) { return (n < 10 ? '0' : '') + n; }
        return d.getFullYear() + '-' + p(d.getMonth() + 1) + '-' + p(d.getDate()) + 'T' + p(d.getHours()) + ':' + p(d.getMinutes()) + ':' + p(d.getSeconds());
    }

    function fechaTexto(d) {
        return d.toLocaleString('es-AR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
    }

    function factor(forma) {
        var f = estado.catalogo && estado.catalogo.factores ? Number(estado.catalogo.factores[forma]) : 1;
        return isFinite(f) && f > 0 ? f : 1;
    }

    // Precio de una linea con la forma de pago: solo las de precio de lista llevan el factor (como el POS normal).
    function precioLinea(linea, forma) {
        return linea.origenPrecio === 'catalogo' ? redondear(linea.precioBase * factor(forma)) : linea.precioBase;
    }

    function totalCon(forma) {
        return redondear(estado.carrito.reduce(function (s, l) { return s + precioLinea(l, forma) * l.cantKg; }, 0));
    }

    // ---------------------------------------------------------------- franja y vistas

    function actualizarFranja() {
        var franja = $('franja');
        franja.classList.toggle('ok', estado.online);
        franja.querySelector('strong').textContent = estado.online ? 'VOLVIÓ LA CONEXIÓN' : 'SIN CONEXIÓN';
        $('textoCausa').textContent = estado.online ? 'Podés volver al POS normal: las ventas se suben solas.' : Red.textoCausa(estado.causa);
        mostrar('btnVolverPos', estado.online);
        $('textoVendedor').textContent = estado.sesion ? 'Vende: ' + estado.sesion.nombre : '';
        mostrar('btnSalir', !!estado.sesion);
        if (estado.catalogo && estado.catalogo.bajado) {
            $('textoCatalogo').textContent = 'Precios del ' + fechaTexto(new Date(estado.catalogo.bajado));
        }
    }

    function refrescarCola() {
        return Db.cola().then(function (cola) {
            var pendientes = cola.filter(function (i) { return i.estado === 'pendiente'; });
            var rechazadas = cola.filter(function (i) { return i.estado === 'rechazada'; });
            $('textoPendientes').textContent = pendientes.length
                ? pendientes.length + (pendientes.length === 1 ? ' operación pendiente de subir' : ' operaciones pendientes de subir')
                : 'Nada pendiente de subir';
            mostrar('vistaCola', cola.length > 0);
            var lista = $('listaCola');
            lista.innerHTML = '';
            cola.slice().reverse().forEach(function (i) {
                var div = document.createElement('div');
                div.className = 'border-bottom py-1' + (i.estado === 'rechazada' ? ' estado-rechazada' : '');
                var desc = i.tipo === 'caja'
                    ? 'Apertura de caja · ' + i.fechaTexto + ' · ' + dinero(i.pedido.cajaInicio)
                    : 'Venta · ' + i.fechaTexto + ' · ' + (i.pedido.formaPago || '') + ' · ' + dinero(i.total);
                div.textContent = desc + ' · ' + (i.nombreVendedor || '') + (i.estado === 'rechazada' ? ' · REVISAR: ' + (i.motivo || '') : ' · pendiente');
                lista.appendChild(div);
            });
            return cola;
        });
    }

    function irA(vista) {
        ['vistaNoPreparada', 'vistaLogin', 'vistaCaja', 'vistaVenta'].forEach(function (v) { mostrar(v, v === vista); });
        actualizarFranja();
        if (vista === 'vistaVenta') { renderCarrito(); $('inputCodigo').focus(); }
        if (vista === 'vistaCaja') $('inputCajaInicio').focus();
    }

    // ---------------------------------------------------------------- login con PIN

    function renderUsuarios() {
        var usuarios = (estado.catalogo.usuarios || []).slice().sort(function (a, b) { return a.nombre.localeCompare(b.nombre); });
        var cont = $('listaUsuarios');
        cont.innerHTML = '';
        if (!estado.catalogo.dispositivoAutorizado) {
            mensaje('mensajeLogin', 'Esta PC no figura como dispositivo autorizado: sin conexión no se puede entrar. Pedile a un administrador que la autorice.');
            return;
        }
        if (usuarios.length === 0) {
            mensaje('mensajeLogin', 'Nadie de esta sucursal tiene PIN. Cada cajero lo configura en su menú de usuario → Clave rápida (PIN).');
            return;
        }
        usuarios.forEach(function (u) {
            var b = document.createElement('button');
            b.type = 'button';
            b.className = 'btn btn-outline-primary';
            b.textContent = u.nombre + (u.admin ? ' (admin)' : '');
            b.addEventListener('click', function () {
                estado.usuarioElegido = u;
                Array.prototype.forEach.call(cont.children, function (x) { x.classList.remove('activo'); });
                b.classList.add('activo');
                $('nombrePin').textContent = u.nombre;
                mostrar('formPin', true);
                $('inputPin').value = '';
                $('inputPin').focus();
                mensaje('mensajeLogin', '');
            });
            cont.appendChild(b);
        });
    }

    function entrar(evento) {
        evento.preventDefault();
        var u = estado.usuarioElegido;
        if (!u) return;
        Pin.verificar(u, $('inputPin').value, Db).then(function (r) {
            $('inputPin').value = '';
            if (!r.ok) { mensaje('mensajeLogin', r.mensaje); return; }
            estado.sesion = { idUsuario: u.id, nombre: u.nombre, admin: !!u.admin, inicio: Date.now() };
            return Db.guardar('sesion', estado.sesion).then(irSegunCaja);
        }).catch(function (e) { mensaje('mensajeLogin', e.message); });
    }

    function salir() {
        estado.sesion = null;
        estado.carrito = [];
        Db.borrar('sesion').then(function () { irA('vistaLogin'); renderUsuarios(); });
    }

    // ---------------------------------------------------------------- caja

    function irSegunCaja() {
        return Promise.all([Db.leer('cajas'), recuperarCarrito()]).then(function (valores) {
            var cajas = valores[0];
            var caja = cajas && cajas[estado.sesion.idUsuario];
            irA(caja && caja.abierta ? 'vistaVenta' : 'vistaCaja');
        });
    }

    function abrirCaja(evento) {
        evento.preventDefault();
        var monto = numero($('inputCajaInicio').value);
        if (!(monto > 0)) { mensaje('mensajeCaja', 'Ingresá un monto mayor a 0.'); return; }
        var ahora = new Date();
        var clientId = uuid();
        var item = {
            clientId: clientId,
            tipo: 'caja',
            idVendedor: estado.sesion.idUsuario,
            nombreVendedor: estado.sesion.nombre,
            idSucursal: estado.catalogo.sucursal ? estado.catalogo.sucursal.id : 0,
            fechaTexto: fechaTexto(ahora),
            pedido: {
                clientId: clientId,
                idVendedor: estado.sesion.idUsuario,
                idSucursalPOS: estado.catalogo.sucursal ? estado.catalogo.sucursal.id : 0,
                fechaLocal: fechaLocalIso(ahora),
                cajaInicio: monto,
                causaCorte: estado.causa
            }
        };
        Db.encolar(item)
            .then(function () { return Db.leer('cajas'); })
            .then(function (cajas) {
                cajas = cajas || {};
                cajas[estado.sesion.idUsuario] = { abierta: true, offline: true, fechaInicio: item.pedido.fechaLocal, informado: ahora.toISOString() };
                return Db.guardar('cajas', cajas);
            })
            .then(function () { $('inputCajaInicio').value = ''; return refrescarCola(); })
            .then(function () { irA('vistaVenta'); })
            .catch(function (e) { mensaje('mensajeCaja', 'No se pudo abrir la caja: ' + e.message); });
    }

    // ---------------------------------------------------------------- carrito

    function renderCarrito() {
        var cuerpo = $('cuerpoCarrito');
        cuerpo.innerHTML = '';
        estado.carrito.forEach(function (l, i) {
            var tr = document.createElement('tr');
            var precio = precioLinea(l, 'Efectivo');
            [l.nombre, l.cantKg.toLocaleString('es-AR', { maximumFractionDigits: 3 }), dinero(precio), dinero(precio * l.cantKg)].forEach(function (texto, j) {
                var td = document.createElement('td');
                td.textContent = texto;
                if (j > 0) td.className = 'text-end';
                tr.appendChild(td);
            });
            var tdQuitar = document.createElement('td');
            var b = document.createElement('button');
            b.type = 'button';
            b.className = 'btn btn-sm btn-outline-danger';
            b.textContent = 'Quitar';
            b.addEventListener('click', function () { estado.carrito.splice(i, 1); renderCarrito(); $('inputCodigo').focus(); });
            tdQuitar.appendChild(b);
            tr.appendChild(tdQuitar);
            cuerpo.appendChild(tr);
        });
        $('textoTotal').textContent = dinero(totalCon('Efectivo'));
        renderBotonesPago();
        guardarCarrito();
    }

    // El carrito en curso se guarda en la PC en cada cambio: un corte de luz o una recarga no lo pierde (mismo criterio
    // que el borrador del POS normal). Se recupera solo para el mismo vendedor.
    function guardarCarrito() {
        if (!estado.sesion) return;
        Db.guardar('carrito', { idVendedor: estado.sesion.idUsuario, lineas: estado.carrito }).catch(function (e) {
            console.warn('No se pudo guardar el carrito sin conexión', e);
        });
    }

    function recuperarCarrito() {
        return Db.leer('carrito').then(function (c) {
            estado.carrito = c && estado.sesion && c.idVendedor === estado.sesion.idUsuario && Array.isArray(c.lineas) ? c.lineas : [];
        }).catch(function () { estado.carrito = []; });
    }

    function renderBotonesPago() {
        var cont = $('botonesPago');
        cont.innerHTML = '';
        var hay = estado.carrito.length > 0;
        FORMAS.forEach(function (f) {
            var b = document.createElement('button');
            b.type = 'button';
            b.className = 'btn ' + (f.clave === 'Efectivo' ? 'btn-success' : 'btn-outline-primary');
            b.disabled = !hay;
            b.innerHTML = '';
            b.appendChild(document.createTextNode(f.texto + ' (' + f.tecla + ')'));
            var small = document.createElement('small');
            small.textContent = hay ? dinero(totalCon(f.clave)) : '';
            b.appendChild(small);
            b.addEventListener('click', function () { cobrar(f.clave, false); });
            cont.appendChild(b);
        });
        var mixto = document.createElement('button');
        mixto.type = 'button';
        mixto.className = 'btn btn-outline-secondary';
        mixto.disabled = !hay;
        mixto.textContent = 'Pago mixto (6)';
        mixto.addEventListener('click', function () { cobrar(null, true); });
        cont.appendChild(mixto);
    }

    // Cantidad para la linea, mismo criterio que pos-product.js: etiqueta de peso > campo Cantidad > codigo escaneado
    // (EAN valido: 1, aunque sea pesable, porque la etiqueta ya trae el precio) > balanza (pesable) > 1.
    function cantidadPara(resultado, codigo) {
        if (resultado.cantidadSugerida) return { ok: true, valor: Number(resultado.cantidadSugerida), balanza: false };
        var tipeada = numero($('inputCantidad').value);
        if (tipeada > 0) return { ok: true, valor: tipeada, balanza: false };
        if (Buscar.esEan13Valido(codigo) || Buscar.esEan8Valido(codigo)) return { ok: true, valor: 1, balanza: false };
        if (resultado.pesable && window.CarnisysBalanza && window.CarnisysBalanza.estaDisponible()) {
            var v = window.CarnisysBalanza.validarParaAgregar();
            if (!v.ok) return { ok: false, mensaje: v.mensaje + ' O escribí la cantidad a mano.' };
            return { ok: true, valor: Number(v.lectura.peso), balanza: true };
        }
        if (resultado.pesable) return { ok: false, mensaje: 'Producto pesable: ingresá la cantidad en kg (la balanza no responde).' };
        return { ok: true, valor: 1, balanza: false };
    }

    function agregarPorCodigo(codigo) {
        var r = Buscar.buscarProducto(codigo, $('chkPrecioManual').checked, estado.catalogo);
        if (r.error || r.success === false) { mensaje('mensajeVenta', r.error || r.message); return false; }
        var cant = cantidadPara(r, String(codigo).trim());
        if (!cant.ok) { mensaje('mensajeVenta', cant.mensaje); return false; }
        if (estado.carrito.length >= (estado.config.maxLineas || 300)) { mensaje('mensajeVenta', 'La venta llegó al máximo de líneas.'); return false; }
        estado.carrito.push({
            codigo: r.codigo,
            nombre: r.nombre,
            cantKg: redondearCantidad(cant.valor),
            precioBase: r.precioKg,
            origenPrecio: r.origenPrecio,
            balanza: cant.balanza
        });
        mensaje('mensajeVenta', '');
        $('inputCantidad').value = '';
        $('chkPrecioManual').checked = false;
        renderCarrito();
        return true;
    }

    function redondearCantidad(n) { return Math.round(n * 1000) / 1000; }

    // ---------------------------------------------------------------- busqueda F10

    function abrirBusqueda() {
        mostrar('capaBuscar', true);
        $('inputBuscar').value = $('inputCodigo').value;
        renderBusqueda();
        $('inputBuscar').focus();
    }

    function renderBusqueda() {
        var lista = $('listaBuscar');
        lista.innerHTML = '';
        Buscar.buscarPorTexto($('inputBuscar').value, estado.catalogo, 100).forEach(function (p) {
            var b = document.createElement('button');
            b.type = 'button';
            var izq = document.createElement('span');
            izq.textContent = p.codigo + ' · ' + p.nombre;
            var der = document.createElement('span');
            der.textContent = dinero(p.precio);
            b.appendChild(izq);
            b.appendChild(der);
            b.addEventListener('click', function () {
                mostrar('capaBuscar', false);
                if (agregarPorCodigo(String(p.codigo))) $('inputCodigo').value = '';
                $('inputCodigo').focus();
            });
            lista.appendChild(b);
        });
    }

    // ---------------------------------------------------------------- cobro

    function cobrar(forma, mixto) {
        if (estado.carrito.length === 0) return;
        if (!mixto && forma !== 'Efectivo') { registrarVenta(forma, false, 0, 0); return; }
        estado.cobro = { forma: forma, mixto: mixto };
        mostrar('grupoMixto', mixto);
        var select = $('selectOtraForma');
        select.innerHTML = '';
        FORMAS.filter(function (f) { return f.clave !== 'Efectivo'; }).forEach(function (f) {
            var o = document.createElement('option');
            o.value = f.clave;
            o.textContent = f.texto;
            select.appendChild(o);
        });
        $('labelEfectivo').textContent = mixto ? 'Parte en efectivo ($)' : 'Paga con ($, vacío = justo)';
        $('inputEfectivo').value = '';
        mensaje('mensajeCobro', '');
        actualizarCobro();
        mostrar('capaCobro', true);
        $('inputEfectivo').focus();
    }

    function formaDelCobro() { return estado.cobro.mixto ? $('selectOtraForma').value : 'Efectivo'; }

    function actualizarCobro() {
        if (!estado.cobro) return;
        var total = totalCon(formaDelCobro());
        $('cobroTotal').textContent = dinero(total);
        var efectivo = numero($('inputEfectivo').value);
        var texto = '';
        if (!estado.cobro.mixto && efectivo > 0) texto = efectivo >= total ? 'Vuelto: ' + dinero(efectivo - total) : 'Falta: ' + dinero(total - efectivo);
        if (estado.cobro.mixto && efectivo > 0) texto = 'Con ' + $('selectOtraForma').selectedOptions[0].textContent + ': ' + dinero(Math.max(0, total - efectivo));
        $('textoVuelto').textContent = texto;
    }

    function confirmarCobro(evento) {
        evento.preventDefault();
        var forma = formaDelCobro();
        var total = totalCon(forma);
        var efectivo = numero($('inputEfectivo').value);
        if (estado.cobro.mixto) {
            if (!(efectivo > 0) || efectivo >= total) { mensaje('mensajeCobro', 'En pago mixto, la parte en efectivo tiene que ser mayor a 0 y menor al total.'); return; }
            registrarVenta(forma, true, efectivo, 0);
        } else {
            if (efectivo > 0 && efectivo < total) { mensaje('mensajeCobro', 'El pago es menor al total.'); return; }
            registrarVenta('Efectivo', false, 0, efectivo > 0 ? efectivo : 0);
        }
        mostrar('capaCobro', false);
    }

    // Arma la venta, la deja en la cola (lo primero: si algo falla despues, la venta ya esta a salvo) e imprime el ticket.
    function registrarVenta(forma, esMixto, efectivoMixto, pagaCon) {
        var ahora = new Date();
        var clientId = uuid();
        var lineas = estado.carrito.map(function (l) {
            return {
                codigo: l.codigo,
                nombre: l.nombre,
                cantKg: l.cantKg,
                precioKg: precioLinea(l, forma),
                bonificacion: 0,
                balanza: l.balanza,
                estado: 0,
                indexAnulado: -1,
                origenPrecio: l.origenPrecio
            };
        });
        var total = totalCon(forma);
        var item = {
            clientId: clientId,
            tipo: 'venta',
            idVendedor: estado.sesion.idUsuario,
            nombreVendedor: estado.sesion.nombre,
            idSucursal: estado.catalogo.sucursal ? estado.catalogo.sucursal.id : 0,
            fechaTexto: fechaTexto(ahora),
            total: total,
            pedido: {
                clientId: clientId,
                idVendedor: estado.sesion.idUsuario,
                idSucursalPOS: estado.catalogo.sucursal ? estado.catalogo.sucursal.id : 0,
                idPersona: estado.catalogo.idConsumidorFinal,
                fechaLocal: fechaLocalIso(ahora),
                formaPago: forma,
                esPagoMixto: esMixto,
                efectivo: esMixto ? efectivoMixto : 0,
                observaciones: '',
                causaCorte: estado.causa,
                versionCatalogo: estado.catalogo.version,
                lineas: lineas.map(function (l) {
                    return { codigo: l.codigo, cantKg: l.cantKg, precioKg: l.precioKg, bonificacion: 0, balanza: l.balanza, estado: 0, indexAnulado: -1, origenPrecio: l.origenPrecio };
                })
            }
        };
        Db.encolar(item).then(function () {
            estado.carrito = [];
            renderCarrito();
            mensaje('mensajeVenta', 'Venta registrada en esta PC (' + dinero(total) + '). Se sube sola cuando vuelva la conexión.', true);
            $('inputCodigo').focus();
            refrescarCola();
            return Ticket.imprimir({
                clientId: clientId, fecha: ahora, formaPago: forma, efectivoMixto: esMixto ? efectivoMixto : 0,
                lineas: lineas, total: total, pagaCon: pagaCon, vuelto: pagaCon > 0 ? pagaCon - total : 0
            }, estado.catalogo.ticket);
        }).catch(function (e) {
            // La venta NO quedo guardada: el carrito sigue en pantalla para reintentar.
            mensaje('mensajeVenta', 'No se pudo guardar la venta en esta PC: ' + e.message);
        });
    }

    // ---------------------------------------------------------------- conexion y subida

    function chequearRed() {
        return Red.chequear({ segundosTimeout: estado.config.segundosTimeoutPing, urlChequeoInternet: estado.config.urlChequeoInternet })
            .then(function (r) {
                estado.online = r.online;
                if (!r.online) estado.causa = r.causa;
                actualizarFranja();
                if (r.online) return subirEnSegundoPlano();
            });
    }

    // Con conexion, intenta subir la cola sin sacar al cajero de la venta. Si falta la sesion, lo indica.
    function subirEnSegundoPlano() {
        return Sync.subirCola({}).then(function (r) {
            if (r.sinSesion) {
                mostrarAviso('Volvió la conexión. Tocá "Volver al POS normal" e ingresá con tu usuario para subir las ventas.');
            } else if (r.subidas > 0 || r.rechazadas > 0) {
                mostrarAviso('Se subieron ' + r.subidas + ' operaciones' + (r.rechazadas ? ', ' + r.rechazadas + ' quedaron para revisar' : '') + '.' +
                    (r.avisos.length ? ' ' + r.avisos.join(' ') : ''));
            } else if (r.deOtroVendedor > 0) {
                mostrarAviso('Hay ventas de otro vendedor: se suben cuando él entre al POS con internet.');
            }
            return refrescarCola();
        });
    }

    function mostrarAviso(texto) {
        var el = $('mensajeGeneral');
        el.textContent = texto;
        el.hidden = !texto;
    }

    function descargarRespaldo() {
        Db.cola().then(function (cola) {
            var blob = new Blob([JSON.stringify({ generado: new Date().toISOString(), cola: cola }, null, 2)], { type: 'application/json' });
            var a = document.createElement('a');
            var d = new Date();
            a.href = URL.createObjectURL(blob);
            a.download = 'ventas-sin-conexion-' + fechaLocalIso(d).replace(/[-:T]/g, '').substring(0, 12) + '.json';
            document.body.appendChild(a);
            a.click();
            setTimeout(function () { URL.revokeObjectURL(a.href); a.remove(); }, 1000);
        });
    }

    // ---------------------------------------------------------------- teclado y arranque

    function teclas(e) {
        if (e.key === 'Escape') { mostrar('capaBuscar', false); mostrar('capaCobro', false); $('inputCodigo').focus(); return; }
        if ($('vistaVenta').hidden) return;
        if (e.key === 'F10') { e.preventDefault(); abrirBusqueda(); return; }
        var enCampo = /INPUT|SELECT|TEXTAREA/.test((document.activeElement || {}).tagName || '');
        if (enCampo || !$('capaBuscar').hidden || !$('capaCobro').hidden) return;
        var forma = FORMAS.filter(function (f) { return f.tecla === e.key; })[0];
        if (forma) { e.preventDefault(); cobrar(forma.clave, false); }
        if (e.key === '6') { e.preventDefault(); cobrar(null, true); }
    }

    function enlazar() {
        $('formPin').addEventListener('submit', entrar);
        $('formCaja').addEventListener('submit', abrirCaja);
        // El formulario tiene dos campos de texto y ningun boton de envio: el navegador no lo envia con Enter (envio
        // implicito del HTML), asi que Enter se maneja a mano. Enter en Cantidad pasa a Codigo.
        $('formCodigo').addEventListener('submit', function (e) { e.preventDefault(); });
        $('inputCodigo').addEventListener('keydown', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            var codigo = $('inputCodigo').value.trim();
            if (!codigo) return;
            if (agregarPorCodigo(codigo)) $('inputCodigo').value = '';
            else $('inputCodigo').select();
        });
        $('inputCantidad').addEventListener('keydown', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            $('inputCodigo').focus();
        });
        $('btnBuscar').addEventListener('click', abrirBusqueda);
        $('inputBuscar').addEventListener('input', renderBusqueda);
        $('formCobro').addEventListener('submit', confirmarCobro);
        $('inputEfectivo').addEventListener('input', actualizarCobro);
        $('selectOtraForma').addEventListener('change', actualizarCobro);
        $('btnSalir').addEventListener('click', salir);
        $('btnProbar').addEventListener('click', function () { $('textoCausa').textContent = 'Comprobando…'; chequearRed(); });
        $('btnVolverPos').addEventListener('click', function () { window.location.href = Red.base() + 'Ventas/POS'; });
        $('btnRespaldo').addEventListener('click', descargarRespaldo);
        Array.prototype.forEach.call(document.querySelectorAll('[data-cerrar]'), function (b) {
            b.addEventListener('click', function () { mostrar('capaBuscar', false); mostrar('capaCobro', false); $('inputCodigo').focus(); });
        });
        document.addEventListener('keydown', teclas);
        window.addEventListener('online', chequearRed);
        window.addEventListener('offline', function () { estado.online = false; estado.causa = 'sinred'; actualizarFranja(); });
    }

    function noPreparada(texto) {
        if (texto) $('textoNoPreparada').textContent = texto;
        irA('vistaNoPreparada');
    }

    function iniciar() {
        enlazar();
        if (!Db || !window.indexedDB) { noPreparada('Este navegador no permite guardar datos para trabajar sin conexión.'); return; }
        Db.pedirPersistencia();
        if (window.CarnisysBalanza) {
            window.CarnisysBalanza.start({ onPeso: function (l) { $('textoBalanza').textContent = l && l.peso > 0 ? 'Balanza: ' + l.peso + ' kg' : ''; } });
            window.CarnisysBalanza.activar();
        }
        Promise.all([Db.leer('catalogo'), Db.leer('configOffline'), Db.leer('sesion')])
            .then(function (valores) {
                estado.catalogo = valores[0];
                estado.config = valores[1] || {};
                var sesion = valores[2];
                refrescarCola();
                if (!estado.catalogo || !estado.catalogo.productos) { noPreparada(); return; }
                var vigente = sesion && (Date.now() - sesion.inicio) < HORAS_SESION * 3600 * 1000
                    && (estado.catalogo.usuarios || []).some(function (u) { return u.id === sesion.idUsuario; });
                if (vigente) {
                    estado.sesion = sesion;
                    irSegunCaja();
                } else {
                    irA('vistaLogin');
                    renderUsuarios();
                }
            })
            .catch(function (e) { noPreparada('No se pudo leer lo guardado en esta PC: ' + e.message); })
            .then(function () {
                chequearRed();
                setInterval(chequearRed, SEGUNDOS_CHEQUEO_RED * 1000);
            });
    }

    iniciar();
})(window, document);
