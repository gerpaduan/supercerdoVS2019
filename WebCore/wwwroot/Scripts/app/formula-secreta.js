// Formula secreta (2026-10-04, ver docs/DECISIONS.md "Formula secreta con re-login").
// Modulo compartido por Ingreso Rapido, Carga y el historial de elaborados (Detalle/Lineas):
//  - FormulaSecreta.ver(cfg, destino): abre el selector de usuario (el mismo de Cierre de Caja,
//    seleccion-usuario.js) para elegir de la lista a alguien con permiso sobre formulas y pedirle la
//    contraseña; las valida contra ElaboradosController.AutorizarVerFormula. Devuelve una
//    Promise<boolean> (true = autorizado y el servidor registro la elevacion; false = el usuario cancelo).
//  - FormulaSecreta.ocultar(cfg, idCorte): quita la elevacion (boton "Ocultar").
//  - FormulaSecreta.ocultarAlSalir(cfg): revoca todas las elevaciones al cerrar/abandonar la pagina.
// El modulo NO muestra ni guarda la formula: eso lo hace cada pantalla con lo que devuelve el servidor.
// cfg = { autorizarUrl, ocultarUrl, usuarios? }. usuarios = [{id, nombre}]; si falta se usa
// window.UsuariosConPermisoFormula (lo publica cada pantalla desde ViewBag.UsuariosConPermisoFormula:
// solo usuarios activos con permiso sobre fórmulas).
// destino = { idCorte } o { idEmbutido } (el historial manda el embutido).
// Requiere en la pagina: _ModalSeleccionUsuario.cshtml + seleccion-usuario.js.
(function () {
    'use strict';

    function ver(cfg, destino) {
        return new Promise(function (resolve) {
            if (!window.SeleccionUsuario) {
                // Sin el componente no se puede autenticar: se deniega (falla cerrado) y queda el aviso en consola.
                console.error('FormulaSecreta.ver: falta seleccion-usuario.js / _ModalSeleccionUsuario en la pagina.');
                resolve(false);
                return;
            }

            // seleccion-usuario.js hace el POST (idUsuario + clave + token) y maneja error, bloqueo por
            // intentos y cuenta regresiva; datosExtra suma el producto cuya formula se quiere ver.
            window.SeleccionUsuario.abrir({
                titulo: 'Ver fórmula',
                usuarios: cfg.usuarios || window.UsuariosConPermisoFormula || [],
                requierePassword: true,
                validarUrl: cfg.autorizarUrl,
                datosExtra: { idCorte: destino.idCorte || 0, idEmbutido: destino.idEmbutido || 0 },
                alCancelar: function () { resolve(false); }
            }).then(function () {
                resolve(true);
            });
        });
    }

    function ocultar(cfg, idCorte) {
        return $.ajax({ url: cfg.ocultarUrl, type: 'POST', data: { idCorte: idCorte || 0 } })
            .then(function () { return true; }, function () { return false; });
    }

    // sendBeacon no puede adjuntar el token antiforgery: por eso OcultarFormula no lo exige (solo oculta).
    // Se engancha a pagehide (no beforeunload) para no revocar si el usuario cancela el aviso de "salir sin guardar".
    function ocultarAlSalir(cfg) {
        window.addEventListener('pagehide', function () {
            if (navigator.sendBeacon && cfg && cfg.ocultarUrl) {
                navigator.sendBeacon(cfg.ocultarUrl, new Blob([''], { type: 'application/x-www-form-urlencoded' }));
            }
        });
    }

    window.FormulaSecreta = { ver: ver, ocultar: ocultar, ocultarAlSalir: ocultarAlSalir };
})();
