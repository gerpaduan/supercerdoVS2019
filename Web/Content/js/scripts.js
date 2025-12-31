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
