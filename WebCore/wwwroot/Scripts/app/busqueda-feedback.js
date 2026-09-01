// Utilitarios compartidos para las pantallas que buscan un producto/elaborado
// por codigo (Movimientos, Stock, Elaborados/Carga, PuntosExpendio/Abrir,
// Compras/Editar): reposicionar el aviso de validacion debajo del bloque de
// busqueda solo en mobile, y sonidos de exito/error al buscar por codigo.
// No depende de nada mas que jQuery (ya cargado en todas estas vistas).
(function (window, $) {
    'use strict';

    var mqMobile = window.matchMedia('(max-width: 767.98px)');

    // Mueve los elementos de avisoSelector para que queden justo despues del
    // elemento anclaSelector SOLO en mobile (tipicamente la columna de
    // "codigo", asi el aviso aparece entre codigo y producto); en desktop
    // quedan en su posicion original del markup. anclaSelector se espera que
    // sea una columna Bootstrap (`col-*`) dentro de una `.row`: se le agrega
    // la clase `col-12` al aviso mientras esta ahi adentro para que tome el
    // mismo gutter que sus hermanas y ocupe su propio renglon en el flex-wrap
    // de la fila (se saca esa clase al volver a la posicion original, para no
    // dejarle un padding de grid que no le corresponde fuera de la fila). Se
    // re-evalua si el usuario rota el dispositivo o redimensiona la ventana
    // (ej. tablet cruzando el breakpoint).
    function anclarAvisoResponsive(avisoSelector, anclaSelector) {
        var $avisos = $(avisoSelector);
        var $ancla = $(anclaSelector);
        if (!$avisos.length || !$ancla.length) return;

        // marcador invisible que preserva la posicion original en el DOM
        // para poder volver a ella si la ventana pasa a tamaño desktop.
        var $marcador = $('<span class="d-none" aria-hidden="true"></span>').insertBefore($avisos.first());

        function aplicar() {
            if (mqMobile.matches) {
                $avisos.addClass('col-12');
                $ancla.after($avisos);
            } else {
                $avisos.removeClass('col-12');
                $marcador.after($avisos);
            }
        }

        aplicar();
        if (mqMobile.addEventListener) {
            mqMobile.addEventListener('change', aplicar);
        } else if (mqMobile.addListener) {
            mqMobile.addListener(aplicar);
        }
    }

    // --- Beeps de exito/error (Web Audio API, sin archivos de audio nuevos) ---

    var audioCtx = null;
    function getAudioCtx() {
        if (audioCtx) return audioCtx;
        var Ctx = window.AudioContext || window.webkitAudioContext;
        if (!Ctx) return null;
        audioCtx = new Ctx();
        return audioCtx;
    }

    function tono(ctx, freq, inicioSeg, duracionSeg, volumen) {
        var osc = ctx.createOscillator();
        var gain = ctx.createGain();
        osc.type = 'sine';
        osc.frequency.value = freq;
        // rampa de volumen (evita el "click" de arrancar/parar un oscilador seco)
        gain.gain.setValueAtTime(0, ctx.currentTime + inicioSeg);
        gain.gain.linearRampToValueAtTime(volumen, ctx.currentTime + inicioSeg + 0.01);
        gain.gain.linearRampToValueAtTime(0, ctx.currentTime + inicioSeg + duracionSeg);
        osc.connect(gain);
        gain.connect(ctx.destination);
        osc.start(ctx.currentTime + inicioSeg);
        osc.stop(ctx.currentTime + inicioSeg + duracionSeg + 0.02);
    }

    function beepExito() {
        var ctx = getAudioCtx();
        if (!ctx) return;
        if (ctx.state === 'suspended') { ctx.resume(); }
        tono(ctx, 1046.5, 0, 0.09, 0.12); // un solo tono agudo (C6)
    }

    function beepError() {
        var ctx = getAudioCtx();
        if (!ctx) return;
        if (ctx.state === 'suspended') { ctx.resume(); }
        tono(ctx, 220, 0, 0.12, 0.14);    // doble tono grave (A3), distinguible del exito
        tono(ctx, 220, 0.16, 0.12, 0.14);
    }

    window.BusquedaFeedback = {
        anclarAvisoResponsive: anclarAvisoResponsive,
        beepExito: beepExito,
        beepError: beepError
    };
})(window, jQuery);
