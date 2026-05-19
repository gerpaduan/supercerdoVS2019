(function (window, document) {
    var BODY_SELECTOR = '.js-sync-scroll-body';
    var HOST_CLASS = 'sync-scroll-host';
    var BAR_CLASS = 'sync-scroll-floating';
    var INNER_CLASS = 'sync-scroll-floating-inner';

    function toArray(list) {
        return Array.prototype.slice.call(list || []);
    }

    function ensureHost(body) {
        var parent = body.parentElement;
        if (parent && parent.classList.contains(HOST_CLASS)) {
            return parent;
        }

        var host = document.createElement('div');
        host.className = HOST_CLASS;
        body.parentNode.insertBefore(host, body);
        host.appendChild(body);
        return host;
    }

    function ensureBar(host, body) {
        var stickyAnchor = host.previousElementSibling;
        if (stickyAnchor && stickyAnchor.classList.contains('js-sync-scroll-anchor')) {
            var anchoredBar = stickyAnchor.querySelector(':scope > .' + BAR_CLASS);
            if (anchoredBar) {
                return anchoredBar;
            }

            anchoredBar = document.createElement('div');
            anchoredBar.className = BAR_CLASS;
            anchoredBar.setAttribute('aria-hidden', 'true');

            var anchoredInner = document.createElement('div');
            anchoredInner.className = INNER_CLASS;
            anchoredBar.appendChild(anchoredInner);

            stickyAnchor.appendChild(anchoredBar);
            return anchoredBar;
        }

        var bar = host.querySelector(':scope > .' + BAR_CLASS);
        if (bar) {
            return bar;
        }

        bar = document.createElement('div');
        bar.className = BAR_CLASS;
        bar.setAttribute('aria-hidden', 'true');

        var inner = document.createElement('div');
        inner.className = INNER_CLASS;
        bar.appendChild(inner);

        host.insertBefore(bar, body);
        return bar;
    }

    function syncPair(body, bar) {
        if (body.__syncScrollBound) {
            return;
        }

        var lock = false;
        var inner = bar.firstElementChild;

        function update() {
            var scrollWidth = body.scrollWidth || 0;
            var clientWidth = body.clientWidth || 0;
            var hasOverflow = scrollWidth > clientWidth + 1;

            inner.style.width = scrollWidth + 'px';
            bar.style.display = hasOverflow ? 'block' : 'none';

            if (!hasOverflow) {
                body.scrollLeft = 0;
                bar.scrollLeft = 0;
                return;
            }

            if (Math.abs(bar.scrollLeft - body.scrollLeft) > 1) {
                bar.scrollLeft = body.scrollLeft;
            }
        }

        body.addEventListener('scroll', function () {
            if (lock) return;
            lock = true;
            bar.scrollLeft = body.scrollLeft;
            lock = false;
        }, { passive: true });

        bar.addEventListener('scroll', function () {
            if (lock) return;
            lock = true;
            body.scrollLeft = bar.scrollLeft;
            lock = false;
        }, { passive: true });

        if (window.ResizeObserver) {
            var observer = new ResizeObserver(update);
            observer.observe(body);
            if (body.firstElementChild) {
                observer.observe(body.firstElementChild);
            }
            body.__syncResizeObserver = observer;
        }

        body.__syncScrollUpdate = update;
        body.__syncScrollBound = true;
        update();
    }

    function refresh(root) {
        var scope = root || document;
        toArray(scope.querySelectorAll(BODY_SELECTOR)).forEach(function (body) {
            var host = ensureHost(body);
            var bar = ensureBar(host, body);
            syncPair(body, bar);
            if (body.__syncScrollUpdate) {
                body.__syncScrollUpdate();
            }
        });
    }

    window.TableScrollSync = {
        refresh: refresh
    };

    document.addEventListener('DOMContentLoaded', function () {
        refresh(document);
    });

    window.addEventListener('resize', function () {
        refresh(document);
    });

    if (window.jQuery) {
        window.jQuery(document).ajaxComplete(function () {
            refresh(document);
        });
    }
})(window, document);
