function showToast(message, type = "error") {
    const toastEl = $("#globalToast");
    const toastBody = $("#globalToastBody");

    toastBody.text(message);

    // Resetear estilos
    toastEl.removeClass("bg-success bg-danger bg-info bg-warning text-white text-dark");

    // Asignar según tipo
    switch (type) {
        case "success":
            toastEl.addClass("bg-success text-white");
            break;
        case "info":
            toastEl.addClass("bg-info text-white");
            break;
        case "warning":
            toastEl.addClass("bg-warning text-dark");
            break;
        default:
            toastEl.addClass("bg-danger text-white");
            break;
    }

    // Inicializar y mostrar
    toastEl.toast({ delay: 4000 });
    toastEl.toast("show");
}