(function () {
    "use strict";

    var menuButton = document.querySelector(".menu-toggle");
    var navigation = document.getElementById("siteNavigation");

    function closeMenu() {
        if (!menuButton || !navigation) return;
        menuButton.setAttribute("aria-expanded", "false");
        menuButton.setAttribute("aria-label", "Abrir menú");
        navigation.classList.remove("is-open");
        document.body.classList.remove("menu-open");
    }

    if (menuButton && navigation) {
        menuButton.addEventListener("click", function () {
            var isOpen = menuButton.getAttribute("aria-expanded") === "true";
            menuButton.setAttribute("aria-expanded", isOpen ? "false" : "true");
            menuButton.setAttribute("aria-label", isOpen ? "Abrir menú" : "Cerrar menú");
            navigation.classList.toggle("is-open", !isOpen);
            document.body.classList.toggle("menu-open", !isOpen);
        });

        navigation.addEventListener("click", function (event) {
            if (event.target.closest("a")) closeMenu();
        });

        document.addEventListener("keydown", function (event) {
            if (event.key === "Escape") closeMenu();
        });

        window.addEventListener("resize", function () {
            if (window.innerWidth > 900) closeMenu();
        });
    }

    var videoShell = document.querySelector(".video-shell[data-youtube-id]");
    if (!videoShell) return;

    var videoId = (videoShell.getAttribute("data-youtube-id") || "").trim();
    var validVideoId = /^[a-zA-Z0-9_-]{11}$/.test(videoId);
    var playButton = videoShell.querySelector(".video-play");

    if (!validVideoId) {
        videoShell.classList.add("video-pending");
        if (playButton) {
            playButton.disabled = true;
            playButton.setAttribute("aria-label", "Video de presentación próximamente");
        }
        return;
    }

    var iframe = document.createElement("iframe");
    iframe.title = "Video de presentación de CARNISYS";
    iframe.loading = "eager";
    iframe.allow = "accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share";
    iframe.allowFullscreen = true;
    iframe.src = "https://www.youtube-nocookie.com/embed/" + encodeURIComponent(videoId)
        + "?autoplay=1&mute=1&loop=1&playlist=" + encodeURIComponent(videoId)
        + "&controls=1&rel=0&modestbranding=1&playsinline=1";

    videoShell.replaceChildren(iframe);
}());
