// Componente reusable: modal de seleccion rapida de usuario, con o sin
// verificacion de contrasena. Primer caso de uso: autorizacion temporal en
// Cierre de Caja (ver cajas-abiertas.js), pero no depende de ese modulo --
// cualquier vista puede llamar a window.SeleccionUsuario.abrir(opciones).
(function (window, $) {
    'use strict';

    function createSeleccionUsuario() {
        var usuariosActuales = [];
        var requierePassword = false;
        var validarUrl = null;
        var resolverPromesa = null;
        var usuarioMarcado = null;
        var countdownInterval = null;

        // Mismo criterio de normalizacion (mayusculas + sacar acentos via NFD)
        // que ya usa ventas-expendios-pos.js (normalizeText), pero con \p{Diacritic}
        // (Unicode property escape, ES2018) en vez del rango ̀-ͯ -- mismo
        // resultado, sin tener que escribir el rango de caracteres combinados en el
        // codigo fuente.
        function normalizar(texto) {
            return (texto || '')
                .toString()
                .toUpperCase()
                .normalize('NFD')
                .replace(/\p{Diacritic}/gu, '');
        }

        function renderLista(filtro) {
            var $lista = $('#listaSeleccionUsuario');
            $lista.empty();

            var filtroNorm = normalizar(filtro);
            var coincidencias = usuariosActuales.filter(function (u) {
                return !filtroNorm || normalizar(u.nombre).indexOf(filtroNorm) >= 0;
            });

            coincidencias.forEach(function (u, index) {
                var $item = $('<button type="button" class="list-group-item list-group-item-action seleccion-usuario-item"></button>')
                    .text(u.nombre)
                    .attr('data-id', u.id)
                    .attr('data-nombre', u.nombre);

                if (index === 0) $item.addClass('is-selected');
                $lista.append($item);
            });
        }

        function marcarUsuario($item) {
            if (!$item || !$item.length) return;

            usuarioMarcado = { id: parseInt($item.attr('data-id'), 10), nombre: $item.attr('data-nombre') };
            $('#listaSeleccionUsuario .seleccion-usuario-item').removeClass('is-selected');
            $item.addClass('is-selected');
            $('#txtSeleccionUsuario').val(usuarioMarcado.nombre);
        }

        function mostrarError(msg) {
            $('#seleccionUsuarioError').removeClass('d-none').text(msg);
        }

        function limpiarError() {
            $('#seleccionUsuarioError').addClass('d-none').text('');
        }

        function mostrarBloqueado(segundos) {
            $('#seleccionUsuarioFormulario').addClass('d-none');
            $('#seleccionUsuarioBloqueado').removeClass('d-none');
            iniciarCountdown(segundos);
        }

        function iniciarCountdown(segundos) {
            clearInterval(countdownInterval);
            var restante = Math.max(0, parseInt(segundos, 10) || 0);

            function tick() {
                var m = Math.floor(restante / 60);
                var s = restante % 60;
                $('#seleccionUsuarioCountdown').text(m + ':' + (s < 10 ? '0' : '') + s);

                if (restante <= 0) {
                    clearInterval(countdownInterval);
                    $('#seleccionUsuarioBloqueado').addClass('d-none');
                    $('#seleccionUsuarioFormulario').removeClass('d-none');
                    limpiarError();
                    return;
                }

                restante--;
            }

            tick();
            countdownInterval = setInterval(tick, 1000);
        }

        function confirmar() {
            limpiarError();

            if (!usuarioMarcado) {
                var $primera = $('#listaSeleccionUsuario .seleccion-usuario-item.is-selected').first();
                if ($primera.length) marcarUsuario($primera);
            }

            if (!usuarioMarcado) {
                mostrarError('Seleccioná un usuario.');
                return;
            }

            if (!requierePassword) {
                resolver({ id: usuarioMarcado.id, nombre: usuarioMarcado.nombre });
                return;
            }

            var clave = $('#passSeleccionUsuario').val() || '';
            if (!clave) {
                mostrarError('Ingresá la contraseña.');
                $('#passSeleccionUsuario').trigger('focus');
                return;
            }

            $('#btnConfirmarSeleccionUsuario').prop('disabled', true);

            $.ajax({
                url: validarUrl,
                type: 'POST',
                data: {
                    idUsuario: usuarioMarcado.id,
                    clave: clave,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
                },
                success: function (resp) {
                    $('#btnConfirmarSeleccionUsuario').prop('disabled', false);

                    if (resp && resp.ok) {
                        resolver({ id: usuarioMarcado.id, nombre: resp.nombre || usuarioMarcado.nombre });
                        return;
                    }

                    if (resp && resp.bloqueado) {
                        mostrarBloqueado(resp.segundosRestantes || 0);
                        return;
                    }

                    mostrarError((resp && resp.msg) || 'Usuario o contraseña incorrectos.');
                    $('#passSeleccionUsuario').val('').trigger('focus');
                },
                error: function () {
                    $('#btnConfirmarSeleccionUsuario').prop('disabled', false);
                    mostrarError('No se pudo validar la autorización.');
                }
            });
        }

        function resolver(usuario) {
            $('#modalSeleccionUsuario').data('resuelto', true);

            var cb = resolverPromesa;
            resolverPromesa = null;
            $('#modalSeleccionUsuario').modal('hide');

            if (typeof cb === 'function') cb(usuario);
        }

        function reset() {
            usuarioMarcado = null;
            clearInterval(countdownInterval);
            $('#txtSeleccionUsuario').val('');
            $('#passSeleccionUsuario').val('');
            $('#listaSeleccionUsuario').empty();
            $('#btnConfirmarSeleccionUsuario').prop('disabled', false);
            limpiarError();
            $('#seleccionUsuarioFormulario').removeClass('d-none');
            $('#seleccionUsuarioBloqueado').addClass('d-none');
            $('#seleccionUsuarioPasswordWrap').addClass('d-none');
        }

        function abrir(opciones) {
            opciones = opciones || {};
            usuariosActuales = Array.isArray(opciones.usuarios) ? opciones.usuarios : [];
            requierePassword = !!opciones.requierePassword;
            validarUrl = opciones.validarUrl || null;

            reset();
            $('#seleccionUsuarioTitulo').text(opciones.titulo || 'Seleccionar usuario');
            $('#seleccionUsuarioPasswordWrap').toggleClass('d-none', !requierePassword);
            renderLista('');

            return new Promise(function (resolve) {
                resolverPromesa = resolve;
                $('#modalSeleccionUsuario').data('resuelto', false).modal('show');
            });
        }

        function navegarLista(direccion) {
            var $items = $('#listaSeleccionUsuario .seleccion-usuario-item');
            if (!$items.length) return;

            var $actual = $items.filter('.is-selected').first();
            var $destino = direccion === 'down'
                ? ($actual.length ? $actual.next('.seleccion-usuario-item') : $items.first())
                : $actual.prev('.seleccion-usuario-item');

            if ($destino.length) {
                $items.removeClass('is-selected');
                $destino.addClass('is-selected');
                $destino[0].scrollIntoView({ block: 'nearest' });
            }
        }

        function bindEvents() {
            $(document)
                .off('shown.bs.modal.seleccionUsuario', '#modalSeleccionUsuario')
                .on('shown.bs.modal.seleccionUsuario', '#modalSeleccionUsuario', function () {
                    $('#txtSeleccionUsuario').trigger('focus');
                })
                .off('hidden.bs.modal.seleccionUsuario', '#modalSeleccionUsuario')
                .on('hidden.bs.modal.seleccionUsuario', '#modalSeleccionUsuario', function () {
                    // Si se cerro sin resolver (Cancelar/backdrop/Esc) no queda ninguna
                    // promesa pendiente colgada -- se limpia sin invocar el callback.
                    resolverPromesa = null;
                    reset();
                })
                .off('input.seleccionUsuario', '#txtSeleccionUsuario')
                .on('input.seleccionUsuario', '#txtSeleccionUsuario', function () {
                    usuarioMarcado = null;
                    renderLista($(this).val());
                })
                .off('focus.seleccionUsuario click.seleccionUsuario', '#txtSeleccionUsuario')
                .on('focus.seleccionUsuario click.seleccionUsuario', '#txtSeleccionUsuario', function () {
                    if (!$(this).val()) renderLista('');
                })
                .off('click.seleccionUsuario', '.seleccion-usuario-item')
                .on('click.seleccionUsuario', '.seleccion-usuario-item', function () {
                    marcarUsuario($(this));
                    if (requierePassword) $('#passSeleccionUsuario').trigger('focus');
                })
                .off('keydown.seleccionUsuario', '#txtSeleccionUsuario')
                .on('keydown.seleccionUsuario', '#txtSeleccionUsuario', function (e) {
                    if (e.key === 'ArrowDown') { e.preventDefault(); navegarLista('down'); return; }
                    if (e.key === 'ArrowUp') { e.preventDefault(); navegarLista('up'); return; }

                    if (e.key === 'Enter') {
                        e.preventDefault();
                        var $marcar = $('#listaSeleccionUsuario .seleccion-usuario-item.is-selected').first();
                        if ($marcar.length) marcarUsuario($marcar);

                        if (requierePassword) {
                            $('#passSeleccionUsuario').trigger('focus');
                        } else {
                            confirmar();
                        }
                    }
                })
                .off('keydown.seleccionUsuario', '#passSeleccionUsuario')
                .on('keydown.seleccionUsuario', '#passSeleccionUsuario', function (e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        confirmar();
                    }
                })
                .off('click.seleccionUsuario', '#btnConfirmarSeleccionUsuario')
                .on('click.seleccionUsuario', '#btnConfirmarSeleccionUsuario', confirmar);
        }

        bindEvents();

        return { abrir: abrir };
    }

    window.SeleccionUsuario = createSeleccionUsuario();
})(window, window.jQuery);
