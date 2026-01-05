document.addEventListener('DOMContentLoaded', function () {
    const toggle = document.getElementById('toggleDarkMode');
    if (!toggle) return;

    const saved = localStorage.getItem('darkMode') === 'true';
    document.body.classList.toggle('dark-mode', saved);
    toggle.checked = saved;

    toggle.addEventListener('change', function () {
        document.body.classList.toggle('dark-mode', this.checked);
        localStorage.setItem('darkMode', this.checked);
    });
});

// ===============================
// CONTROL DE SESIÓN (POS REAL)
// ===============================
let chequeandoSesion = false;
let modalSesionMostrado = false;

function verificarSesion() {

    if (chequeandoSesion || modalSesionMostrado) return;

    chequeandoSesion = true;

    fetch(window.keepAliveUrl, {
        credentials: 'same-origin',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(resp => {

            console.log('STATUS:', resp.status);

            if (resp.status === 401) {
                mostrarSesionExpirada();
            }

        })
        .catch(() => {
            mostrarSesionExpirada();
        })
        .finally(() => {
            chequeandoSesion = false;
        });
}


// ===============================
// MODAL SESIÓN EXPIRADA (BS4)
// ===============================

function mostrarSesionExpirada() {
    $('#modalSesionExpirada').modal({
        backdrop: 'static',
        keyboard: false
    });
}
function redirigirLogin() {
    window.location.href = window.loginUrl;
}


verificarSesion();
setInterval(verificarSesion, 60 * 5 * 1000);
