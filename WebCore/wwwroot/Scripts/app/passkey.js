// Login por huella y alta de huellas (passkeys WebAuthn) -- cliente. Ver docs/DECISIONS.md "Login
// por huella (passkeys)". Habla con LoginController.PasskeyOptions/PasskeyLogin (pre-sesion) y con
// PasskeyController (Listar/RegistrationOptions/RegistrationComplete/Revocar, con sesion). Las URLs
// llegan por atributos data-* del HTML (Url.Action, respeta el PathBase de IIS): nada hardcodeado.
// La huella NUNCA pasa por este codigo: el navegador la pide al sensor del equipo y solo devuelve
// una firma. Sin dependencias (fetch nativo).
(function (window, document) {
    'use strict';

    // ---- Conversion base64url <-> ArrayBuffer (formato que usa la libreria Fido2 del servidor) ----
    function b64uABuf(texto) {
        var b64 = texto.replace(/-/g, '+').replace(/_/g, '/');
        var relleno = b64.length % 4;
        if (relleno) b64 += '===='.slice(relleno);
        var binario = window.atob(b64);
        var bytes = new Uint8Array(binario.length);
        for (var i = 0; i < binario.length; i++) bytes[i] = binario.charCodeAt(i);
        return bytes.buffer;
    }

    function bufAB64u(buffer) {
        var bytes = new Uint8Array(buffer);
        var binario = '';
        for (var i = 0; i < bytes.length; i++) binario += String.fromCharCode(bytes[i]);
        return window.btoa(binario).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
    }

    // WebAuthn solo funciona en contexto seguro (HTTPS o localhost) y con navegador compatible.
    function soportado() {
        return !!(window.PublicKeyCredential && window.isSecureContext && navigator.credentials);
    }

    // El servidor manda campos en null (tambien anidados, ej. authenticatorSelection.authenticatorAttachment):
    // el navegador no acepta null en miembros enum/diccionario, solo ausentes. Limpia en profundidad.
    function limpiarNulos(objeto) {
        Object.keys(objeto).forEach(function (clave) {
            var valor = objeto[clave];
            if (valor === null || valor === undefined) {
                delete objeto[clave];
            } else if (typeof valor === 'object' && !Array.isArray(valor)) {
                limpiarNulos(valor);
            } else if (Array.isArray(valor)) {
                valor.forEach(function (item) {
                    if (item && typeof item === 'object') limpiarNulos(item);
                });
            }
        });
        return objeto;
    }

    function tokenAntiforgery(origen) {
        var campo = (origen || document).querySelector('input[name="__RequestVerificationToken"]');
        return campo ? campo.value : '';
    }

    // POST JSON con antiforgery por header. Devuelve { ok, status, datos }; nunca lanza por HTTP != 2xx.
    function postJson(url, cuerpo, token) {
        return window.fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body: cuerpo === undefined ? '' : JSON.stringify(cuerpo)
        }).then(function (resp) {
            return resp.json().catch(function () { return {}; }).then(function (datos) {
                return { ok: resp.ok, status: resp.status, datos: datos };
            });
        });
    }

    // Mensaje amigable para los errores tipicos del navegador (cancelar el dialogo es normal).
    function mensajeDeError(error) {
        if (error && error.name === 'NotAllowedError') return 'Se canceló o venció la solicitud de huella.';
        if (error && error.name === 'InvalidStateError') return 'Esta huella ya está registrada en este dispositivo.';
        if (error && error.name === 'NotSupportedError') return 'Este dispositivo no permite usar huella o PIN para ingresar.';
        return 'No se pudo usar la huella. Probá de nuevo o ingresá con usuario y contraseña.';
    }

    // ---- Login (pre-sesion) ----
    function iniciarLogin(boton) {
        var contenedor = document.getElementById('passkeyLogin');
        var form = document.querySelector('form.carnisys-login-form');
        var estado = document.getElementById('passkeyLoginEstado');
        if (!soportado()) return; // sin soporte el bloque sigue oculto: solo se ve el login clasico

        contenedor.hidden = false;

        function mostrarError(texto) {
            estado.textContent = texto;
            estado.hidden = !texto;
        }

        boton.addEventListener('click', function () {
            mostrarError('');
            boton.disabled = true;
            var token = tokenAntiforgery(form);

            postJson(boton.getAttribute('data-options-url'), undefined, token)
                .then(function (r) {
                    if (!r.ok) throw { mensaje: r.datos.error || 'No se pudo iniciar el ingreso con huella.' };
                    var opciones = limpiarNulos(r.datos);
                    opciones.challenge = b64uABuf(opciones.challenge);
                    return navigator.credentials.get({ publicKey: opciones });
                })
                .then(function (credencial) {
                    var respuesta = credencial.response;
                    var serie = document.getElementById('NumeroSerieDispositivo');
                    var returnUrl = document.getElementById('ReturnUrl');
                    var url = boton.getAttribute('data-login-url')
                        + '?returnUrl=' + encodeURIComponent(returnUrl ? returnUrl.value : '')
                        + '&serieDispositivo=' + encodeURIComponent(serie ? serie.value : '');

                    return postJson(url, {
                        id: credencial.id,
                        rawId: bufAB64u(credencial.rawId),
                        type: credencial.type,
                        response: {
                            authenticatorData: bufAB64u(respuesta.authenticatorData),
                            clientDataJSON: bufAB64u(respuesta.clientDataJSON),
                            signature: bufAB64u(respuesta.signature),
                            userHandle: respuesta.userHandle ? bufAB64u(respuesta.userHandle) : null
                        }
                    }, token);
                })
                .then(function (r) {
                    if (!r.ok) throw { mensaje: r.datos.error || 'No se pudo iniciar sesión con la huella.' };
                    window.location.href = r.datos.redirectUrl;
                })
                .catch(function (error) {
                    mostrarError(error && error.mensaje ? error.mensaje : mensajeDeError(error));
                    boton.disabled = false;
                });
        });
    }

    // ---- Alta / listado / baja (con sesion, modal "Mi huella" del menu de usuario) ----
    function iniciarMiHuella(modal) {
        var lista = modal.querySelector('#miHuellaLista');
        var estado = modal.querySelector('#miHuellaEstado');
        var nombre = modal.querySelector('#miHuellaNombre');
        var botonAgregar = modal.querySelector('#btnMiHuellaAgregar');
        var token = tokenAntiforgery(modal);

        if (!soportado()) {
            botonAgregar.disabled = true;
            estado.textContent = 'Este navegador o esta conexión no permite usar huella (hace falta HTTPS y un navegador compatible).';
            estado.className = 'small text-danger';
            return;
        }

        function mostrar(texto, esError) {
            estado.textContent = texto;
            estado.className = 'small ' + (esError ? 'text-danger' : 'text-success');
        }

        function celda(texto) {
            var td = document.createElement('td');
            td.textContent = texto;
            return td;
        }

        function cargar() {
            window.fetch(modal.getAttribute('data-listar-url'), { credentials: 'same-origin' })
                .then(function (resp) { return resp.json(); })
                .then(function (datos) {
                    lista.innerHTML = '';
                    if (!datos.huellas || datos.huellas.length === 0) {
                        var fila = document.createElement('tr');
                        var td = celda('Todavía no registraste ninguna huella.');
                        td.colSpan = 4;
                        td.className = 'text-muted';
                        fila.appendChild(td);
                        lista.appendChild(fila);
                        return;
                    }
                    datos.huellas.forEach(function (h) {
                        var fila = document.createElement('tr');
                        fila.appendChild(celda(h.nombre));
                        fila.appendChild(celda(h.fechaAlta));
                        fila.appendChild(celda(h.ultimoUso || '—'));
                        var acciones = document.createElement('td');
                        var quitar = document.createElement('button');
                        quitar.type = 'button';
                        quitar.className = 'btn btn-sm btn-outline-danger';
                        quitar.textContent = 'Quitar';
                        quitar.addEventListener('click', function () {
                            if (!window.confirm('¿Quitar la huella "' + h.nombre + '"? Ya no podrás ingresar con ella.')) return;
                            postJson(modal.getAttribute('data-revocar-url') + '?id=' + h.id, undefined, token)
                                .then(function (r) {
                                    if (!r.ok) throw { mensaje: r.datos.error || 'No se pudo quitar la huella.' };
                                    mostrar('Huella quitada.', false);
                                    cargar();
                                })
                                .catch(function (error) { mostrar(error.mensaje || mensajeDeError(error), true); });
                        });
                        acciones.appendChild(quitar);
                        fila.appendChild(acciones);
                        lista.appendChild(fila);
                    });
                })
                .catch(function () { mostrar('No se pudo cargar la lista de huellas.', true); });
        }

        botonAgregar.addEventListener('click', function () {
            mostrar('', false);
            botonAgregar.disabled = true;

            postJson(modal.getAttribute('data-options-url'), undefined, token)
                .then(function (r) {
                    if (!r.ok) throw { mensaje: r.datos.error || 'No se pudo iniciar el registro de la huella.' };
                    var opciones = limpiarNulos(r.datos);
                    opciones.challenge = b64uABuf(opciones.challenge);
                    opciones.user.id = b64uABuf(opciones.user.id);
                    (opciones.excludeCredentials || []).forEach(function (c) { c.id = b64uABuf(c.id); });
                    return navigator.credentials.create({ publicKey: opciones });
                })
                .then(function (credencial) {
                    var respuesta = credencial.response;
                    var url = modal.getAttribute('data-complete-url') + '?nombre=' + encodeURIComponent(nombre.value);
                    return postJson(url, {
                        id: credencial.id,
                        rawId: bufAB64u(credencial.rawId),
                        type: credencial.type,
                        response: {
                            attestationObject: bufAB64u(respuesta.attestationObject),
                            clientDataJSON: bufAB64u(respuesta.clientDataJSON),
                            transports: respuesta.getTransports ? respuesta.getTransports() : []
                        }
                    }, token);
                })
                .then(function (r) {
                    if (!r.ok) throw { mensaje: r.datos.error || 'No se pudo registrar la huella.' };
                    nombre.value = '';
                    mostrar('Huella registrada. Ya podés ingresar con ella desde el inicio de sesión.', false);
                    cargar();
                })
                .catch(function (error) { mostrar(error && error.mensaje ? error.mensaje : mensajeDeError(error), true); })
                .then(function () { botonAgregar.disabled = false; });
        });

        modal.addEventListener('shown.bs.modal', cargar);
    }

    document.addEventListener('DOMContentLoaded', function () {
        var botonLogin = document.getElementById('btnPasskeyLogin');
        if (botonLogin) iniciarLogin(botonLogin);

        var modal = document.getElementById('modalMiHuella');
        if (modal) iniciarMiHuella(modal);
    });

    window.CarniSysPasskey = { soportado: soportado };
})(window, document);
