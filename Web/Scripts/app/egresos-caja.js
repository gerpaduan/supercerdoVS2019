(function (window, $) {
    "use strict";

    function urls() {
        return window.EgresosCajaUrls || {};
    }

    function permisos() {
        return window.EgresosCajaPermisos || {};
    }

    function mostrarPermisoPopup(mensaje) {
        var texto = mensaje || "No tiene permisos para realizar esta accion.";

        if (window.Swal && typeof window.Swal.fire === "function") {
            window.Swal.fire({
                icon: "warning",
                title: "Sin permiso",
                text: texto,
                confirmButtonText: "Aceptar"
            });
            return;
        }

        window.alert(texto);
    }

    function renderConScripts(html, destino) {
        var $destino = $(destino);
        var $tmp = $("<div>").html(html);
        var scripts = [];

        $tmp.find("script").each(function () {
            scripts.push(this.text || this.textContent || this.innerHTML || "");
            $(this).remove();
        });

        $destino.html($tmp.html());

        scripts.forEach(function (code) {
            if ($.trim(code)) {
                $.globalEval(code);
            }
        });
    }

    function mostrarError($box, mensaje) {
        $box.removeClass("d-none").text(mensaje || "No se pudo completar la operacion.");
    }

    function confirmarSalidaCambios(callback) {
        if (window.Swal && typeof window.Swal.fire === "function") {
            window.Swal.fire({
                icon: "warning",
                title: "Salir sin guardar",
                text: "Puede perder todos los datos ingresados. Desea salir?",
                showCancelButton: true,
                confirmButtonText: "Si, salir",
                cancelButtonText: "Cancelar"
            }).then(function (r) {
                if (r && r.isConfirmed && typeof callback === "function") {
                    callback();
                }
            });
            return;
        }

        if (window.confirm("Puede perder todos los datos ingresados. Desea salir?") && typeof callback === "function") {
            callback();
        }
    }

    function formEgresoActivo() {
        return $("#formEgresoCaja").length > 0;
    }

    function formEgresoDirty() {
        var $form = $("#formEgresoCaja");
        if (!$form.length || $form.data("permitir-salida") === true) return false;
        return $form.serialize() !== $form.data("estado-inicial");
    }

    function prepararProteccionSalida(selector) {
        var $form = $(selector);
        $form.data("estado-inicial", $form.serialize());
        $form.data("permitir-salida", false);
    }

    function setTituloModal(texto) {
        $("#modalEgresoCajaTitulo").text(texto || "Nuevo Egreso de Caja");
    }

    function estaEnPos() {
        return $("#modalFinanzasPOS").hasClass("show") || (urls().nuevo || "").toLowerCase().indexOf("desdepos=true") >= 0;
    }

    function idCierreActividadActual() {
        var $form = $("#formEgresoCaja");
        var desdeForm = parseInt($form.data("id-cierre"), 10);
        if (!isNaN(desdeForm) && desdeForm > 0) return desdeForm;

        if (window.CajasAbiertas) {
            var desdeModal = parseInt(window.CajasAbiertas.idCierreActividadActual, 10);
            if (!isNaN(desdeModal) && desdeModal > 0) return desdeModal;
        }

        return 0;
    }

    function urlConParametro(url, nombre, valor) {
        if (!url) return url;
        var separador = url.indexOf("?") >= 0 ? "&" : "?";
        return url + separador + encodeURIComponent(nombre) + "=" + encodeURIComponent(valor);
    }

    function aplicarVistaCompleta() {
        var $switch = $("#switchVistaCompletaEgresos");
        if (!$switch.length) return;

        var activa = $switch.prop("checked");
        localStorage.setItem("vistaCompletaEgresosCaja", activa ? "1" : "0");

        var $contenedor = $("#misActividadesContenido");
        var $detalles = $contenedor.length
            ? $contenedor.find(".egreso-detalle-collapse")
            : $(".egreso-detalle-collapse");

        $detalles.each(function () {
            if (activa) {
                $(this).collapse("show");
            } else {
                $(this).collapse("hide");
            }
        });
    }

    function initVistaCompleta() {
        var $switch = $("#switchVistaCompletaEgresos");
        if (!$switch.length) return;

        var activa = localStorage.getItem("vistaCompletaEgresosCaja") === "1";
        $switch.prop("checked", activa);
        setTimeout(aplicarVistaCompleta, 50);
    }

    function filtrar() {
        var cfg = urls();
        if (!cfg.listar) return;

        var data = $("#formFiltrosEgresos").serializeArray();
        data.push({ name: "ajax", value: "true" });

        $.ajax({
            url: cfg.listar,
            type: "GET",
            data: $.param(data),
            cache: false
        }).done(function (html) {
            $("#tablaEgresosCaja").html(html);
            aplicarVistaCompleta();
        }).fail(function (xhr) {
            if (xhr && xhr.status === 403) {
                mostrarPermisoPopup((xhr.responseText || xhr.statusText || "").replace(/<[^>]+>/g, "").trim());
                return;
            }

            if (window.Swal) {
                Swal.fire("Error", "No se pudieron cargar los egresos.", "error");
            }
        });
    }

    function abrirFormulario(id) {
        var cfg = urls();
        if (!cfg.nuevo) return;

        var esEdicion = id && parseInt(id, 10) > 0;
        var urlFormulario = esEdicion ? urlConParametro(cfg.nuevo, "id", id) : cfg.nuevo;
        var idCierreActual = idCierreActividadActual();

        if (idCierreActual > 0) {
            urlFormulario = urlConParametro(urlFormulario, "idCierre", idCierreActual);
        }

        if (estaEnPos() && window.POSEgresos && typeof window.POSEgresos.cargarEnModal === "function") {
            window.POSEgresos.cargarEnModal(urlFormulario, esEdicion ? "Modificar Egreso de Caja" : "Nuevo Egreso de Caja", true);
            return;
        }

        setTituloModal(esEdicion ? "Modificar Egreso de Caja" : "Nuevo Egreso de Caja");
        $("#contenedorEgresoCaja").html('<div class="p-4 text-center text-muted">Cargando...</div>');
        $("#modalEgresoCaja").modal("show");

        $.ajax({
            url: urlFormulario,
            type: "GET",
            cache: false
        }).done(function (html) {
            renderConScripts(html, "#contenedorEgresoCaja");
        }).fail(function (xhr) {
            var mensaje = (xhr && (xhr.statusText || xhr.responseText)) || "No se pudo cargar el formulario.";
            if (xhr && xhr.status === 403) {
                mensaje = (xhr.responseText || xhr.statusText || "").replace(/<[^>]+>/g, "").trim() || "No tiene permisos para realizar esta accion.";
                $("#modalEgresoCaja").modal("hide");
                mostrarPermisoPopup(mensaje);
                return;
            }
            $("#contenedorEgresoCaja").html('<div class="alert alert-danger m-3">' + $("<div>").text(mensaje).html() + '</div>');
        });
    }

    function abrirNuevo() {
        abrirFormulario(0);
    }

    function abrirModificar(id) {
        abrirFormulario(id);
    }

    function bindForm(selector) {
        $(document)
            .off("submit.egresoCaja", selector)
            .on("submit.egresoCaja", selector, function (e) {
                e.preventDefault();

                var cfg = urls();
                var $form = $(this);
                var $error = $form.find("#egresoCajaError");
                var desdePos = String($form.data("desde-pos")).toLowerCase() === "true";

                $error.addClass("d-none").text("");
                var idCierreActual = idCierreActividadActual();

                $.ajax({
                    url: cfg.guardar,
                    type: "POST",
                    data: $form.serialize(),
                    dataType: "json"
                }).done(function (resp) {
                    if (!resp || !resp.ok) {
                        if (resp && resp.mensaje && resp.mensaje.toLowerCase().indexOf("permiso") >= 0) {
                            mostrarPermisoPopup(resp.mensaje);
                        }
                        mostrarError($error, resp && resp.mensaje);
                        return;
                    }

                    $form.data("permitir-salida", true);

                    if (desdePos && window.POSEgresos && typeof window.POSEgresos.abrirMis === "function") {
                        window.POSEgresos.abrirMis();
                        return;
                    }

                    if (idCierreActual > 0 && window.CajasAbiertas && typeof window.CajasAbiertas.abrirActividades === "function") {
                        $("#modalEgresoCaja").modal("hide");
                        window.CajasAbiertas.abrirActividades(idCierreActual);
                        return;
                    }

                    $("#modalEgresoCaja").modal("hide");
                    filtrar();

                    if (window.Swal) {
                        Swal.fire("Guardado", resp.mensaje || "El egreso se guardo correctamente.", "success");
                    }
                }).fail(function () {
                    mostrarError($error, "No se pudo guardar el egreso de caja.");
                });
            });

        prepararProteccionSalida(selector);
    }

    function bindGeneral() {
        $(document)
            .off("submit.egresosFiltro", "#formFiltrosEgresos")
            .on("submit.egresosFiltro", "#formFiltrosEgresos", function (e) {
                e.preventDefault();
                filtrar();
            })
            .off("change.egresosFiltro", "#filtroSucursalEgreso, #filtroUsuarioEgreso, #filtroTipoEgreso, #filtroDesdeEgreso, #filtroHastaEgreso, #soloGastos")
            .on("change.egresosFiltro", "#filtroSucursalEgreso, #filtroUsuarioEgreso, #filtroTipoEgreso, #filtroDesdeEgreso, #filtroHastaEgreso, #soloGastos", function () {
                filtrar();
            })
            .off("keydown.egresosFiltro", "#filtroDescripcionEgreso")
            .on("keydown.egresosFiltro", "#filtroDescripcionEgreso", function (e) {
                if (e.key === "Enter") {
                    e.preventDefault();
                    filtrar();
                }
            })
            .off("click.egresosNuevo", "#btnNuevoEgresoCaja")
            .on("click.egresosNuevo", "#btnNuevoEgresoCaja", function () {
                abrirNuevo();
            })
            .off("click.egresosModificar", ".btn-modificar-egreso")
            .on("click.egresosModificar", ".btn-modificar-egreso", function () {
                abrirModificar($(this).data("id"));
            })
            .off("change.egresosVistaCompleta", "#switchVistaCompletaEgresos")
            .on("change.egresosVistaCompleta", "#switchVistaCompletaEgresos", function () {
                aplicarVistaCompleta();
            })
            .off("click.egresosVolver", "#btnVolverMisEgresos")
            .on("click.egresosVolver", "#btnVolverMisEgresos", function (e) {
                e.preventDefault();
                if (window.POSEgresos && typeof window.POSEgresos.abrirMis === "function") {
                    if (formEgresoDirty()) {
                        confirmarSalidaCambios(function () {
                            $("#formEgresoCaja").data("permitir-salida", true);
                            window.POSEgresos.abrirMis();
                        });
                    } else {
                        window.POSEgresos.abrirMis();
                    }
                }
            });

        initVistaCompleta();
    }

    $(document)
        .off("hide.bs.modal.egresoCaja", "#modalEgresoCaja, #modalFinanzasPOS")
        .on("hide.bs.modal.egresoCaja", "#modalEgresoCaja, #modalFinanzasPOS", function (e) {
            if (!formEgresoActivo() || !formEgresoDirty()) return;

            e.preventDefault();
            confirmarSalidaCambios(function () {
                $("#formEgresoCaja").data("permitir-salida", true);
                $(e.target).modal("hide");
            });
        });

    window.EgresosCaja = {
        filtrar: filtrar,
        abrirNuevo: abrirNuevo,
        abrirModificar: abrirModificar,
        bindForm: bindForm,
        renderConScripts: renderConScripts,
        aplicarVistaCompleta: aplicarVistaCompleta,
        mostrarPermisoPopup: mostrarPermisoPopup,
        formDirty: formEgresoDirty,
        confirmarSalidaCambios: confirmarSalidaCambios,
        permitirSalida: function () {
            $("#formEgresoCaja").data("permitir-salida", true);
        }
    };

    $(function () {
        bindGeneral();
    });
})(window, window.jQuery);
