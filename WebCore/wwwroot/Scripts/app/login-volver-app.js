/* Login dentro de una app instalada (2026-11-21, pedido del usuario): al cerrar sesion en la ventana de una app independiente (Punto de venta, Punto de
   expendio, Stock, Movimientos, Elaborados, Existencia) y volver a ingresar, se vuelve a ESA app y no al dashboard. Asi el POS abre directo el modal de
   operador (usuario de produccion) y el Punto de expendio el de operador y el de sector, que ya aparecen solos al cargar.
   Como: app-independiente.js guarda en la memoria de la ventana (sessionStorage) la direccion de arranque de la app y la publica en window.__appInicio.
   Si el login llega SIN destino (el cierre de sesion redirige a /Login sin returnUrl) y la ventana tiene esa direccion, se completa el campo oculto
   ReturnUrl del formulario; el ingreso con huella (passkey.js) lee el mismo campo. El servidor igual valida que sea una direccion local
   (LoginController.RedirigirPostLogin, Url.IsLocalUrl). Un destino que ya trae el login (sesion vencida) no se pisa. Decision: docs/DECISIONS.md (2026-11-21 «Login en una app instalada vuelve a la app»). */
(function () {
    'use strict';

    // true si el valor es una ruta del mismo sitio ("/Ventas/POS?ventana=1"): empieza con una sola "/" (no "//" ni "/\", que el navegador toma como otro sitio).
    function esRutaLocal(ruta) {
        return typeof ruta === 'string' && ruta.length > 1 && ruta.charAt(0) === '/' && ruta.charAt(1) !== '/' && ruta.charAt(1) !== '\\';
    }

    // Devuelve el destino que debe usar el login: el que ya trae (si trae uno) o, si viene vacio, el inicio guardado de la app (si es una ruta local).
    // null = no cambiar nada (el login sigue al dashboard como siempre).
    function destinoLogin(returnUrlActual, inicioApp) {
        if (typeof returnUrlActual === 'string' && returnUrlActual.trim() !== '') return null;
        return esRutaLocal(inicioApp) ? inicioApp : null;
    }

    window.LoginVolverApp = { destinoLogin: destinoLogin, esRutaLocal: esRutaLocal };

    var campo = document.getElementById('ReturnUrl');
    if (!campo) return;
    var destino = destinoLogin(campo.value, window.__appInicio);
    if (destino) campo.value = destino;
})();
