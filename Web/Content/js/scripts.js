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


function validarEAN(codigo) {
    if (!/^\d{8}$/.test(codigo) && !/^\d{13}$/.test(codigo)) {
        return false;
    }

    const digits = codigo.split('').map(d => parseInt(d, 10));
    const length = digits.length;

    // Último dígito → dígito verificador real
    const checkDigit = digits[length - 1];

    let sum = 0;

    // Recorremos todos menos el último
    for (let i = 0; i < length - 1; i++) {
        const digit = digits[i];

        if (length === 13) {
            // EAN-13: posiciones pares (índice impar) ×3
            sum += (i % 2 === 0) ? digit : digit * 3;
        } else if (length === 8) {
            // EAN-8: posiciones impares (índice par) ×3
            sum += (i % 2 === 0) ? digit * 3 : digit;
        }
    }

    const calculated = (10 - (sum % 10)) % 10;
    return calculated === checkDigit;
}

function esEANValido(codigo) {
    return validarEAN(codigo);
}
