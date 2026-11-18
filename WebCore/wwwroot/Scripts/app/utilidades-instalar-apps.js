/* Utilidades > «Aplicaciones instalables»: abre la pagina anfitriona de instalacion de cada mini-app en una VENTANA CHICA (2026-11-17).
   Antes (2026-11-13/17) se probo instalar directo con navigator.install(): en Chrome instalo la app general «CarniSys» en lugar de la mini-app (la firma de esa
   API cambia entre versiones), asi que se descarto. Ahora cada boton abre {Carpeta}/InstalarApp (InstalarAppController), que enlaza el manifest de ESA app y
   muestra el boton «Instalar» (pwa-instalar.js); al confirmar, esa ventana se cierra sola.
   Instalacion en lote (2026-11-17): casillas por app, «Seleccionar todas», «Instalar seleccionadas» e «Instalar todas». Abren UNA ventana chica con la primera app
   y la lista de las que faltan en ?cola=A,B,C; pwa-instalar.js pasa sola a la siguiente cuando esta se instala, se rechaza o no se ofrece. El navegador exige un clic
   en «Instalar» por cada app (el gesto del usuario no se arrastra entre paginas), por eso no se puede instalar todo con un solo clic.
   Si el navegador bloquea el popup, en el boton individual rige el enlace normal; en el lote se avisa. Decision: docs/DECISIONS.md (2026-11-17 c y h). */
(function () {
    'use strict';

    var CARACTERISTICAS_VENTANA = 'popup=yes,width=520,height=640';
    var NOMBRE_VENTANA_LOTE = 'carnisys-instalar-cola';

    function avisar(titulo, texto) {
        if (window.Swal && typeof window.Swal.fire === 'function') window.Swal.fire(titulo, texto, 'warning');
        else window.alert(titulo + ': ' + texto);
    }

    // Instalacion de una sola app (boton «Instalar» de la tarjeta).
    document.addEventListener('click', function (evento) {
        var boton = evento.target.closest('.js-instalar-app');
        if (!boton) return;

        // Nombre fijo por app: un segundo clic reutiliza la ventana ya abierta en lugar de apilar otra.
        var nombreVentana = 'carnisys-instalar-' + boton.id;
        var ventana = window.open(boton.href, nombreVentana, CARACTERISTICAS_VENTANA);
        if (!ventana) return; // popup bloqueado: sigue el enlace normal (pestana nueva)

        evento.preventDefault();
        ventana.focus();
    });

    // ---------- Instalacion en lote ----------

    function casillas() { return Array.prototype.slice.call(document.querySelectorAll('.js-instalar-marca')); }
    function marcadas() { return casillas().filter(function (c) { return c.checked; }).map(function (c) { return c.getAttribute('data-app'); }); }
    function todas() { return casillas().map(function (c) { return c.getAttribute('data-app'); }); }

    // La URL de cada app es la del boton «Instalar» de su tarjeta (ya trae el PathBase del sitio).
    function hrefDe(clave) {
        var enlace = document.querySelector('.js-instalar-app[data-app="' + clave + '"]');
        return enlace ? enlace.href : null;
    }

    // Abre una ventana chica con la primera app; las que faltan viajan en ?cola= (pwa-instalar.js las recorre).
    function instalarLote(claves) {
        if (!claves.length) return;
        var primera = hrefDe(claves[0]);
        if (!primera) return;
        var resto = claves.slice(1);
        var url = primera + (resto.length ? '?cola=' + resto.join(',') : '');
        var ventana = window.open(url, NOMBRE_VENTANA_LOTE, CARACTERISTICAS_VENTANA);
        if (!ventana) {
            avisar('Ventana bloqueada', 'El navegador bloqueó la ventana de instalación. Permití las ventanas emergentes para este sitio e intentá de nuevo.');
            return;
        }
        ventana.focus();
    }

    var $maestra = document.getElementById('chkInstalarTodas');
    var $botonSeleccionadas = document.getElementById('btnInstalarSeleccionadas');
    var $contador = document.getElementById('cntInstalarSeleccionadas');
    var $botonTodas = document.getElementById('btnInstalarTodas');
    if (!$maestra || !$botonSeleccionadas || !$botonTodas) return; // la pantalla no tiene la barra de lote

    function actualizarBarra() {
        var n = marcadas().length, total = casillas().length;
        $contador.textContent = String(n);
        $botonSeleccionadas.disabled = n === 0;
        $maestra.checked = total > 0 && n === total;
        $maestra.indeterminate = n > 0 && n < total;
    }

    $maestra.addEventListener('change', function () {
        casillas().forEach(function (c) { c.checked = $maestra.checked; });
        actualizarBarra();
    });
    document.addEventListener('change', function (evento) {
        if (evento.target.classList && evento.target.classList.contains('js-instalar-marca')) actualizarBarra();
    });
    $botonSeleccionadas.addEventListener('click', function () { instalarLote(marcadas()); });
    $botonTodas.addEventListener('click', function () { instalarLote(todas()); });

    actualizarBarra();
})();
