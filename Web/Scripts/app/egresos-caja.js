(function (window, $) {
    "use strict";
    var tiposFiltroState = {
        buscar: "",
        origen: "todos",
        gasto: "todos"
    };

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

    function formActivo() {
        return $("#formEgresoCaja, #formComisionesElectronicas").length > 0;
    }

    function formActual() {
        var $form = $("#formComisionesElectronicas");
        if ($form.length) return $form;
        return $("#formEgresoCaja");
    }

    function formDirty() {
        var $form = formActual();
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

    function setTituloTiposModal(texto) {
        $("#modalTiposEgresoCajaTitulo").text(texto || "Tipos Egresos");
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

    function abrirCalculoComisiones() {
        var cfg = urls();
        if (!cfg.comisionesFormulario) return;

        var data = {
            fechaDesde: $("#filtroDesdeEgreso").val() || "",
            fechaHasta: $("#filtroHastaEgreso").val() || "",
            idSucursal: $("#filtroSucursalEgreso").val() || 0
        };

        setTituloModal("Calcular comisiones electrónicas");
        $("#contenedorEgresoCaja").html('<div class="p-4 text-center text-muted">Cargando...</div>');
        $("#modalEgresoCaja").modal("show");

        $.ajax({
            url: cfg.comisionesFormulario,
            type: "GET",
            data: data,
            cache: false
        }).done(function (html) {
            renderConScripts(html, "#contenedorEgresoCaja");
        }).fail(function (xhr) {
            var mensaje = (xhr && (xhr.statusText || xhr.responseText)) || "No se pudo cargar el cálculo de comisiones.";
            if (xhr && xhr.status === 403) {
                mensaje = (xhr.responseText || xhr.statusText || "").replace(/<[^>]+>/g, "").trim() || "No tiene permisos para realizar esta accion.";
                $("#modalEgresoCaja").modal("hide");
                mostrarPermisoPopup(mensaje);
                return;
            }

            $("#contenedorEgresoCaja").html('<div class="alert alert-danger m-3">' + $("<div>").text(mensaje).html() + '</div>');
        });
    }

    function abrirModificar(id) {
        abrirFormulario(id);
    }

    function abrirPago(url, titulo) {
        if (!url) return;

        if (estaEnPos() && window.POSFinanzas && typeof window.POSFinanzas.cargarPago === "function") {
            window.POSFinanzas.cargarPago(url, titulo || "Pago / Cobro");
            return;
        }

        if ($("#modalEgresoCaja").length) {
            setTituloModal(titulo || "Pago / Cobro");
            $("#contenedorEgresoCaja").html('<div class="p-4 text-center text-muted">Cargando...</div>');
            $("#modalEgresoCaja").modal("show");

            $.ajax({
                url: url,
                type: "GET",
                cache: false
            }).done(function (html) {
                renderConScripts(html, "#contenedorEgresoCaja");
            }).fail(function (xhr) {
                var mensaje = (xhr && (xhr.statusText || xhr.responseText)) || "No se pudo cargar el pago.";
                $("#contenedorEgresoCaja").html('<div class="alert alert-danger m-3">' + $("<div>").text(mensaje).html() + '</div>');
            });
            return;
        }

        window.location.href = url;
    }

    function tokenTipos() {
        return $("#formTokenTiposEgresoCaja input[name='__RequestVerificationToken']").val()
            || $("#formTipoEgresoCaja input[name='__RequestVerificationToken']").val()
            || $("input[name='__RequestVerificationToken']").first().val();
    }

    function syncTiposFiltroStateDesdeUI() {
        tiposFiltroState.buscar = ($("#buscarTipoEgresoCaja").val() || "").toString();
        tiposFiltroState.origen = ($("#filtroOrigenTipoEgreso").val() || "todos").toString();
        tiposFiltroState.gasto = ($("#filtroGastoTipoEgreso").val() || "todos").toString();
    }

    function aplicarTiposFiltroStateAUI() {
        $("#buscarTipoEgresoCaja").val(tiposFiltroState.buscar || "");
        $("#filtroOrigenTipoEgreso").val(tiposFiltroState.origen || "todos");
        $("#filtroGastoTipoEgreso").val(tiposFiltroState.gasto || "todos");
    }

    function aplicarFiltrosTiposEgreso() {
        syncTiposFiltroStateDesdeUI();

        var buscar = (tiposFiltroState.buscar || "").toLowerCase().trim();
        var origen = tiposFiltroState.origen || "todos";
        var gasto = tiposFiltroState.gasto || "todos";
        var visibles = 0;

        $("#tablaTiposEgresoCaja .tipo-egreso-row").each(function () {
            var $row = $(this);
            var nombre = ($row.data("nombre") || "").toString().toLowerCase();
            var reservado = String($row.attr("data-reservado")).toLowerCase() === "true";
            var esGasto = String($row.attr("data-es-gasto")).toLowerCase() === "true";
            var visible = true;

            if (buscar && nombre.indexOf(buscar) < 0 && String($row.data("id")).indexOf(buscar) < 0) {
                visible = false;
            }

            if (visible && origen === "propios" && reservado) {
                visible = false;
            }

            if (visible && origen === "sistema" && !reservado) {
                visible = false;
            }

            if (visible && gasto === "gastos" && !esGasto) {
                visible = false;
            }

            if (visible && gasto === "no-gastos" && esGasto) {
                visible = false;
            }

            $row.toggle(visible);
            if (visible) {
                visibles += 1;
            }
        });

        var $tbody = $("#tablaTiposEgresoCaja tbody");
        $tbody.find(".tipos-egreso-empty-filter").remove();

        if (visibles === 0 && $("#tablaTiposEgresoCaja .tipo-egreso-row").length > 0) {
            $tbody.append('<tr class="tipos-egreso-empty-filter"><td colspan="5" class="text-center text-muted py-4">No hay tipos de egreso para el filtro seleccionado.</td></tr>');
        }
    }

    function refrescarComboTiposIndex() {
        var cfg = urls();
        var $combo = $("#filtroTipoEgreso");
        if (!cfg.tiposOpciones || !$combo.length) return;

        var seleccionado = ($combo.val() || "0").toString();

        $.ajax({
            url: cfg.tiposOpciones,
            type: "GET",
            cache: false,
            dataType: "json"
        }).done(function (resp) {
            if (!resp || !resp.ok || !$.isArray(resp.items)) return;

            $combo.empty();
            $combo.append('<option value="0">Todos</option>');

            $.each(resp.items, function (_, item) {
                var id = item && item.id != null ? String(item.id) : "";
                var nombre = item && item.nombre != null ? String(item.nombre) : "";
                if (!id) return;

                var $opt = $("<option>").val(id).text(nombre);
                if (id === seleccionado) {
                    $opt.prop("selected", true);
                }
                $combo.append($opt);
            });

            if ($combo.find("option:selected").length === 0) {
                $combo.val("0");
            }
        });
    }

    function cargarTipos(buscar) {
        var cfg = urls();
        var perms = permisos();
        if (!cfg.tipos) return;

        if (perms.puedeVerTipos === false) {
            mostrarPermisoPopup("No tiene permisos para ver tipos de egreso.");
            return;
        }

        setTituloTiposModal("Tipos Egresos");
        $("#contenedorTiposEgresoCaja").html('<div class="p-4 text-center text-muted">Cargando...</div>');
        $("#modalTiposEgresoCaja").modal("show");

        $.ajax({
            url: cfg.tipos,
            type: "GET",
            data: { buscar: buscar || "" },
            cache: false
        }).done(function (html) {
            renderConScripts(html, "#contenedorTiposEgresoCaja");
            aplicarTiposFiltroStateAUI();
            aplicarFiltrosTiposEgreso();
        }).fail(function (xhr) {
            var mensaje = (xhr && (xhr.responseText || xhr.statusText)) || "No se pudieron cargar los tipos de egreso.";
            if (xhr && xhr.status === 403) {
                $("#modalTiposEgresoCaja").modal("hide");
                mostrarPermisoPopup(mensaje.replace(/<[^>]+>/g, "").trim());
                return;
            }

            $("#contenedorTiposEgresoCaja").html('<div class="alert alert-danger m-3">' + $("<div>").text(mensaje).html() + '</div>');
        });
    }

    function abrirTipos() {
        cargarTipos("");
    }

    function abrirFormularioTipo(id) {
        var cfg = urls();
        var perms = permisos();
        if (!cfg.tipoFormulario) return;

        if (perms.puedeEditarTipos === false) {
            mostrarPermisoPopup("No tiene permisos para administrar tipos de egreso.");
            return;
        }

        setTituloTiposModal(id && parseInt(id, 10) > 0 ? "Editar Tipo Egreso" : "Nuevo Tipo Egreso");
        $("#contenedorTiposEgresoCaja").html('<div class="p-4 text-center text-muted">Cargando...</div>');

        $.ajax({
            url: cfg.tipoFormulario,
            type: "GET",
            data: { id: id || 0 },
            cache: false
        }).done(function (html) {
            renderConScripts(html, "#contenedorTiposEgresoCaja");
        }).fail(function (xhr) {
            var mensaje = (xhr && (xhr.responseText || xhr.statusText)) || "No se pudo cargar el tipo de egreso.";
            $("#contenedorTiposEgresoCaja").html('<div class="alert alert-danger m-3">' + $("<div>").text(mensaje.replace(/<[^>]+>/g, "").trim()).html() + '</div>');
        });
    }

    function bindTipoForm(selector) {
        $(document)
            .off("submit.tiposEgresoForm", selector)
            .on("submit.tiposEgresoForm", selector, function (e) {
                e.preventDefault();

                var cfg = urls();
                var $form = $(this);
                var $error = $form.find("#tipoEgresoCajaError");

                $error.addClass("d-none").text("");

                $.ajax({
                    url: cfg.tipoGuardar,
                    type: "POST",
                    data: $form.serialize(),
                    dataType: "json"
                }).done(function (resp) {
                    if (!resp || !resp.ok) {
                        mostrarError($error, resp && resp.mensaje);
                        return;
                    }

                    refrescarComboTiposIndex();
                    cargarTipos(tiposFiltroState.buscar || "");
                    if (window.Swal) {
                        Swal.fire("Guardado", resp.mensaje || "El tipo de egreso se guardó correctamente.", "success");
                    }
                }).fail(function () {
                    mostrarError($error, "No se pudo guardar el tipo de egreso.");
                });
            });
    }

    function eliminarTipo(id, nombre) {
        var cfg = urls();
        var perms = permisos();
        if (!cfg.tipoEliminar) return;

        if (perms.usuarioAdmin === false) {
            mostrarPermisoPopup("Debe tener permiso de Administrador para eliminar un Tipo Egreso.");
            return;
        }

        var ejecutar = function () {
            $.ajax({
                url: cfg.tipoEliminar,
                type: "POST",
                dataType: "json",
                data: {
                    __RequestVerificationToken: tokenTipos(),
                    id: id
                }
            }).done(function (resp) {
                if (!resp || !resp.ok) {
                    if (window.Swal) {
                        Swal.fire("Atención", resp && resp.mensaje ? resp.mensaje : "No se pudo eliminar el tipo de egreso.", "warning");
                    }
                    return;
                }

                refrescarComboTiposIndex();
                cargarTipos(tiposFiltroState.buscar || "");
                if (window.Swal) {
                    Swal.fire("Eliminado", resp.mensaje || "El tipo de egreso se eliminó correctamente.", "success");
                }
            }).fail(function () {
                if (window.Swal) {
                    Swal.fire("Error", "No se pudo eliminar el tipo de egreso.", "error");
                }
            });
        };

        if (window.Swal && typeof window.Swal.fire === "function") {
            window.Swal.fire({
                icon: "question",
                title: "Eliminar Tipo Egreso",
                text: "¿Está seguro que desea eliminar el tipo egreso: " + (nombre || "").toUpperCase() + "?",
                showCancelButton: true,
                confirmButtonText: "Eliminar",
                cancelButtonText: "Cancelar"
            }).then(function (r) {
                if (r && r.isConfirmed) ejecutar();
            });
            return;
        }

        if (window.confirm("¿Está seguro que desea eliminar el tipo egreso: " + (nombre || "").toUpperCase() + "?")) {
            ejecutar();
        }
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

    function parseNumeroDecimal(valor) {
        if (valor == null) return 0;

        var texto = String(valor).trim().replace(/\$/g, "").replace(/\s+/g, "");
        if (!texto) return 0;

        var ultimaComa = texto.lastIndexOf(",");
        var ultimoPunto = texto.lastIndexOf(".");
        if (ultimaComa >= 0 && ultimoPunto >= 0) {
            if (ultimaComa > ultimoPunto) {
                texto = texto.replace(/\./g, "").replace(",", ".");
            } else {
                texto = texto.replace(/,/g, "");
            }
        } else {
            texto = texto.replace(",", ".");
        }

        var numero = parseFloat(texto);
        return isNaN(numero) ? 0 : numero;
    }

    function formatNumeroAr(valor) {
        var numero = parseNumeroDecimal(valor);
        return numero.toLocaleString("es-AR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function formatPorcentajeAr(valor) {
        var numero = parseNumeroDecimal(valor);
        return numero.toLocaleString("es-AR", { minimumFractionDigits: 0, maximumFractionDigits: 2 });
    }

    function recalcularComisiones($form) {
        var totalGeneral = 0;

        $form.find("#tablaComisionesElectronicas tbody tr").each(function () {
            var $row = $(this);
            var totalCobrado = parseNumeroDecimal($row.find(".js-total-cobrado").data("valor"));
            var porcentaje = parseNumeroDecimal($row.find(".js-comision-porcentaje").val());
            var importe = Math.round((totalCobrado * porcentaje) * 100) / 10000;

            totalGeneral += importe;
            $row.find(".js-importe-comision")
                .data("valor", importe)
                .text("$" + formatNumeroAr(importe));
        });

        totalGeneral = Math.round(totalGeneral * 100) / 100;
        $form.find("#totalComisionesElectronicas")
            .data("valor", totalGeneral)
            .text("$" + formatNumeroAr(totalGeneral));
    }

    function actualizarTotalesComisiones($form, items) {
        if (!$.isArray(items)) return;

        $.each(items, function (_, item) {
            var codigo = item && item.codigo ? String(item.codigo) : "";
            if (!codigo) return;

            var totalCobrado = parseNumeroDecimal(item.totalCobrado);
            var $row = $form.find('#tablaComisionesElectronicas tbody tr[data-codigo="' + codigo + '"]');
            if (!$row.length) return;

            $row.find(".js-total-cobrado")
                .data("valor", totalCobrado)
                .text("$" + formatNumeroAr(totalCobrado));
        });

        recalcularComisiones($form);
    }

    function refrescarResumenComisiones($form) {
        var cfg = urls();
        if (!cfg.comisionesResumen) return;

        var fechaDesde = $form.find('[name="fechaDesde"]').val() || "";
        var fechaHasta = $form.find('[name="fechaHasta"]').val() || "";
        var idSucursal = $form.find('[name="idSucursal"]').val() || 0;
        var $error = $form.find("#comisionesElectronicasError");

        $error.addClass("d-none").text("");

        $.ajax({
            url: cfg.comisionesResumen,
            type: "GET",
            dataType: "json",
            cache: false,
            data: {
                fechaDesde: fechaDesde,
                fechaHasta: fechaHasta,
                idSucursal: idSucursal
            }
        }).done(function (resp) {
            if (!resp || !resp.ok) {
                mostrarError($error, resp && resp.mensaje);
                return;
            }

            actualizarTotalesComisiones($form, resp.items);
            prepararProteccionSalida("#formComisionesElectronicas");
        }).fail(function () {
            mostrarError($error, "No se pudieron recalcular las comisiones.");
        });
    }

    function bindComisionesForm(selector) {
        $(document)
            .off("submit.comisionesElectronicas", selector)
            .on("submit.comisionesElectronicas", selector, function (e) {
                e.preventDefault();

                var cfg = urls();
                var $form = $(this);
                var $error = $form.find("#comisionesElectronicasError");
                var desdePos = String($form.data("desde-pos")).toLowerCase() === "true";

                $error.addClass("d-none").text("");
                var idCierreActual = idCierreActividadActual();
                recalcularComisiones($form);

                $.ajax({
                    url: cfg.comisionesGuardar,
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
                    mostrarError($error, "No se pudo guardar el egreso por comisiones.");
                });
            })
            .off("input.comisionesPorcentaje", selector + " .js-comision-porcentaje")
            .on("input.comisionesPorcentaje", selector + " .js-comision-porcentaje", function () {
                recalcularComisiones($(selector));
            })
            .off("click.comisionesAplicarTodas", "#btnAplicarComisionGlobal")
            .on("click.comisionesAplicarTodas", "#btnAplicarComisionGlobal", function () {
                var $form = $(selector);
                var valor = $form.find(".js-comision-porcentaje-global").val() || "";
                $form.find(".js-comision-porcentaje").val(valor);
                recalcularComisiones($form);
            })
            .off("click.comisionesRecalcular", "#btnRecalcularComisiones")
            .on("click.comisionesRecalcular", "#btnRecalcularComisiones", function () {
                refrescarResumenComisiones($(selector));
            })
            .off("change.comisionesFechas", selector + ' [name="fechaDesde"], ' + selector + ' [name="fechaHasta"]')
            .on("change.comisionesFechas", selector + ' [name="fechaDesde"], ' + selector + ' [name="fechaHasta"]', function () {
                refrescarResumenComisiones($(selector));
            })
            .off("change.comisionesSucursal", selector + ' [name="idSucursal"]')
            .on("change.comisionesSucursal", selector + ' [name="idSucursal"]', function () {
                refrescarResumenComisiones($(selector));
            })
            .off("blur.comisionesFormato", selector + " .js-comision-porcentaje-global, " + selector + " .js-comision-porcentaje")
            .on("blur.comisionesFormato", selector + " .js-comision-porcentaje-global, " + selector + " .js-comision-porcentaje", function () {
                var valor = $(this).val();
                if ($.trim(valor) === "") return;
                $(this).val(formatPorcentajeAr(valor));
            });

        recalcularComisiones($(selector));
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
            .off("click.egresosComisiones", "#btnCalcularComisionesElectronicas")
            .on("click.egresosComisiones", "#btnCalcularComisionesElectronicas", function () {
                abrirCalculoComisiones();
            })
            .off("click.tiposEgresoAbrir", "#btnTiposEgresoCaja")
            .on("click.tiposEgresoAbrir", "#btnTiposEgresoCaja", function () {
                abrirTipos();
            })
            .off("click.egresosModificar", ".btn-modificar-egreso")
            .on("click.egresosModificar", ".btn-modificar-egreso", function () {
                var tipoModificacion = ($(this).data("tipo-modificacion") || "").toString().toLowerCase();
                var url = $(this).data("url");

                if (tipoModificacion === "pago" && url) {
                    abrirPago(url, "Modificar Pago / Cobro");
                    return;
                }

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
                    if (formDirty()) {
                        confirmarSalidaCambios(function () {
                            formActual().data("permitir-salida", true);
                            window.POSEgresos.abrirMis();
                        });
                    } else {
                        window.POSEgresos.abrirMis();
                    }
                }
            })
            .off("submit.tiposEgresoFiltro", "#formFiltroTiposEgresoCaja")
            .on("submit.tiposEgresoFiltro", "#formFiltroTiposEgresoCaja", function (e) {
                e.preventDefault();
                aplicarFiltrosTiposEgreso();
            })
            .off("input.tiposEgresoBuscar", "#buscarTipoEgresoCaja")
            .on("input.tiposEgresoBuscar", "#buscarTipoEgresoCaja", function () {
                aplicarFiltrosTiposEgreso();
            })
            .off("change.tiposEgresoFiltros", "#filtroOrigenTipoEgreso, #filtroGastoTipoEgreso")
            .on("change.tiposEgresoFiltros", "#filtroOrigenTipoEgreso, #filtroGastoTipoEgreso", function () {
                aplicarFiltrosTiposEgreso();
            })
            .off("click.tiposEgresoNuevo", "#btnNuevoTipoEgresoCaja")
            .on("click.tiposEgresoNuevo", "#btnNuevoTipoEgresoCaja", function () {
                abrirFormularioTipo(0);
            })
            .off("click.tiposEgresoEditar", ".btn-editar-tipo-egreso")
            .on("click.tiposEgresoEditar", ".btn-editar-tipo-egreso", function () {
                abrirFormularioTipo($(this).data("id"));
            })
            .off("dblclick.tiposEgresoFila", ".tipo-egreso-row")
            .on("dblclick.tiposEgresoFila", ".tipo-egreso-row", function () {
                if (String($(this).attr("data-puede-editar")).toLowerCase() !== "true" || String($(this).attr("data-reservado")).toLowerCase() === "true") return;
                abrirFormularioTipo($(this).data("id"));
            })
            .off("click.tiposEgresoVolver", "#btnVolverTiposEgresoCaja")
            .on("click.tiposEgresoVolver", "#btnVolverTiposEgresoCaja", function () {
                cargarTipos(tiposFiltroState.buscar || "");
            })
            .off("click.tiposEgresoEliminar", ".btn-eliminar-tipo-egreso")
            .on("click.tiposEgresoEliminar", ".btn-eliminar-tipo-egreso", function () {
                eliminarTipo($(this).data("id"), $(this).data("nombre"));
            });

        initVistaCompleta();
    }

    $(document)
        .off("hide.bs.modal.egresoCaja", "#modalEgresoCaja, #modalFinanzasPOS")
        .on("hide.bs.modal.egresoCaja", "#modalEgresoCaja, #modalFinanzasPOS", function (e) {
            if (!formActivo() || !formDirty()) return;

            e.preventDefault();
            confirmarSalidaCambios(function () {
                formActual().data("permitir-salida", true);
                $(e.target).modal("hide");
            });
        });

    window.EgresosCaja = {
        filtrar: filtrar,
        abrirNuevo: abrirNuevo,
        abrirModificar: abrirModificar,
        abrirCalculoComisiones: abrirCalculoComisiones,
        abrirTipos: abrirTipos,
        bindTipoForm: bindTipoForm,
        aplicarFiltrosTiposEgreso: aplicarFiltrosTiposEgreso,
        refrescarComboTiposIndex: refrescarComboTiposIndex,
        bindForm: bindForm,
        bindComisionesForm: bindComisionesForm,
        renderConScripts: renderConScripts,
        aplicarVistaCompleta: aplicarVistaCompleta,
        mostrarPermisoPopup: mostrarPermisoPopup,
        formDirty: formDirty,
        confirmarSalidaCambios: confirmarSalidaCambios,
        permitirSalida: function () {
            formActual().data("permitir-salida", true);
        }
    };

    $(function () {
        bindGeneral();
    });
})(window, window.jQuery);
