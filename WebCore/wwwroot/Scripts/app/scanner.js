// Lector de codigos de barra por camara (EAN-13, EAN-8, UPC-A, Code 128) para el POS y las
// pantallas de carga. Usa el detector nativo del navegador (BarcodeDetector) si lo trae y
// reconoce EAN-13; si no, cae a ZXing (libreria vendorizada en Content/vendor/zxing).
// Depende de: window.ZXing (solo para el respaldo) y de que el visor <video> exista en la vista.
// Detalle de las decisiones de lectura en docs/DECISIONS.md (2026-11-20).
class BarcodeScanner {
    // tiempoRelectura: ms durante los que se ignora un codigo IGUAL al ultimo entregado (evita que
    // el mismo codigo, aun frente a la camara, dispare onCodeDetected en cada frame). Default 2000 =
    // comportamiento historico; un consumidor que pide doble lectura propia (BarcodeCodeInput,
    // alta de producto) pasa 0 para que la segunda lectura llegue en el siguiente frame y no 2 s despues.
    // intervaloLecturaMs: pausa entre deteccion y deteccion (ambos motores).
    constructor({ videoSelector, containerSelector, onCodeDetected, tiempoRelectura = 2000, intervaloLecturaMs = 100 }) {
        this.video = document.querySelector(videoSelector);
        this.container = document.querySelector(containerSelector);
        this.onCodeDetected = onCodeDetected;

        this.scanning = false;
        this.sesionId = 0;      // se incrementa en iniciar() y cerrar(): un bucle viejo no sobrevive a un cierre/reapertura
        this.detector = null;   // motor nativo (BarcodeDetector), solo Chromium
        this.stream = null;
        this.timerLectura = null;

        this.zxingReader = null;    // motor de respaldo (ZXing), para Firefox/Safari/iOS
        this.zxingHints = null;
        this.canvasFranja = null;   // canvas reutilizable donde se recorta la franja que lee ZXing

        this.track = null;     // ✅ para torch
        this.flashOn = false;  // ✅ estado flash

        this.ultimoCodigo = null;
        this.ultimoTiempo = 0;
        this.TIEMPO_RELECTURA = tiempoRelectura;
        this.INTERVALO_LECTURA_MS = intervaloLecturaMs;
    }

    // Formatos soportados por el ticket/POS. Se usan tanto para el detector nativo
    // como para el fallback de ZXing, asi ambos motores leen exactamente lo mismo.
    static get FORMATOS_NATIVOS() {
        return ["ean_13", "ean_8", "code_128", "upc_a"];
    }

    // Zoom que se pide a la camara si la soporta (Chrome/Android). Con 1.5x se puede leer desde mas
    // lejos que la distancia minima de enfoque del celular y el codigo sigue llenando suficientes pixeles.
    static get ZOOM_DESEADO() {
        return 1.5;
    }

    // Fraccion minima de la altura del cuadro que ZXing analiza, aunque el visor sea mas bajo.
    static get FRANJA_MIN_RELATIVA() {
        return 0.25;
    }

    // Devuelve los formatos que el BarcodeDetector del navegador SI reconoce (de FORMATOS_NATIVOS),
    // o null si no hay detector nativo utilizable. Hay Androids sin Google Play Services donde
    // BarcodeDetector existe pero no reconoce ningun formato: ahi no da error, simplemente nunca lee,
    // por eso se verifica con getSupportedFormats() y, si no sirve, se usa ZXing.
    async formatosNativosSoportados() {
        if (!("BarcodeDetector" in window)) return null;

        // Navegadores viejos sin getSupportedFormats: se asume el comportamiento historico (usar nativo).
        if (typeof window.BarcodeDetector.getSupportedFormats !== "function") {
            return BarcodeScanner.FORMATOS_NATIVOS;
        }

        try {
            const soportados = await window.BarcodeDetector.getSupportedFormats();
            const utiles = BarcodeScanner.FORMATOS_NATIVOS.filter(f => soportados.includes(f));
            // Sin EAN-13 el detector nativo no sirve para los productos de la cadena.
            return utiles.includes("ean_13") ? utiles : null;
        } catch (e) {
            console.warn("No se pudo consultar los formatos del BarcodeDetector; se usa tal cual:", e);
            return BarcodeScanner.FORMATOS_NATIVOS;
        }
    }

    async iniciar() {
        if (this.scanning) return;
        this.scanning = true;
        const sesion = ++this.sesionId;
        this.container.style.display = "";

        try {
            await this.arrancar(sesion);
        } catch (e) {
            // Si falla (permiso de camara denegado, etc.) se deja el lector reutilizable: sin esto
            // "scanning" quedaba en true y los reintentos de iniciar() no hacian nada.
            if (sesion === this.sesionId) {
                this.scanning = false;
                this.detenerStream();
            }
            throw e;
        }
    }

    async arrancar(sesion) {
        const formatosNativos = await this.formatosNativosSoportados();
        const usaDetectorNativo = !!formatosNativos;
        const usaZXing = !usaDetectorNativo && typeof window.ZXing !== "undefined";

        if (!usaDetectorNativo && !usaZXing) {
            alert("Este navegador no soporta lectura de codigos de barra.");
            this.scanning = false;
            return;
        }

        // Resolucion: 1280x720 para los dos motores. Con 640x480 un EAN-13 chico o algo lejos se ve
        // borroso y no se lee. ZXing decodifica en JS pero ya no analiza el cuadro entero sino solo
        // la franja central que se ve en el visor (ver leerFranjaZXing), asi que 720p no lo vuelve lento.
        const ideal = { width: { ideal: 1280 }, height: { ideal: 720 } };

        const stream = await navigator.mediaDevices.getUserMedia({
            video: { facingMode: { ideal: "environment" }, ...ideal }
        });

        // Si mientras se pedia el permiso se cerro el lector (o se reabrio), se suelta esta camara.
        if (!this.scanning || sesion !== this.sesionId) {
            stream.getTracks().forEach(t => t.stop());
            return;
        }

        this.stream = stream;
        this.video.srcObject = this.stream;

        // ✅ guardamos el track para el flash (funciona igual con cualquiera de los dos motores)
        this.track = this.stream.getVideoTracks()[0];

        await this.configurarCamara();

        await this.video.play();

        if (!this.scanning || sesion !== this.sesionId) return;

        if (usaDetectorNativo) {
            // Chrome/Edge: Shape Detection API nativa, no requiere libreria externa.
            this.detector = new BarcodeDetector({ formats: formatosNativos });
            this.leerContinuamente(sesion);
            return;
        }

        // Firefox y Safari/iOS no implementan BarcodeDetector: se decodifica con ZXing, frame a frame,
        // sobre la franja central del video (el mismo recorte que muestra el visor).
        const Z = window.ZXing;
        this.zxingHints = new Map();
        this.zxingHints.set(Z.DecodeHintType.POSSIBLE_FORMATS, [
            Z.BarcodeFormat.EAN_13,
            Z.BarcodeFormat.EAN_8,
            Z.BarcodeFormat.CODE_128,
            Z.BarcodeFormat.UPC_A
        ]);
        // TRY_HARDER: revisa todos los renglones de la franja (y el codigo invertido). Sin el, ZXing
        // mira solo ~25 renglones; al ser la franja chica, el costo es bajo.
        this.zxingHints.set(Z.DecodeHintType.TRY_HARDER, true);
        this.zxingReader = new Z.MultiFormatReader();
        this.canvasFranja = document.createElement("canvas");

        this.leerContinuamenteZXing(sesion);
    }

    // Enfoque continuo y zoom. Son opcionales: solo se aplican si el dispositivo los declara, cada uno
    // por separado, y un fallo de cualquiera de los dos no debe impedir escanear.
    async configurarCamara() {
        const cap = this.track.getCapabilities?.();
        if (!cap) return;

        // Autoenfoque continuo: sin el, la camara puede quedar enfocada lejos y el codigo, que esta
        // a pocos cm, sale borroso hasta que el usuario "busca" el foco.
        try {
            if (cap.focusMode?.includes("continuous")) {
                await this.track.applyConstraints({ advanced: [{ focusMode: "continuous" }] });
            }
        } catch (e) {
            console.warn("No se pudo activar el enfoque continuo:", e);
        }

        try {
            if (cap.zoom && typeof cap.zoom.max === "number") {
                const minimo = typeof cap.zoom.min === "number" ? cap.zoom.min : 1;
                const zoom = Math.min(cap.zoom.max, Math.max(minimo, BarcodeScanner.ZOOM_DESEADO));
                if (zoom > minimo) {
                    await this.track.applyConstraints({ advanced: [{ zoom }] });
                }
            }
        } catch (e) {
            console.warn("No se pudo aplicar el zoom:", e);
        }
    }

    procesarCodigoDetectado(codigo) {
        const ahora = Date.now();
        const mismo = codigo === this.ultimoCodigo;
        const tiempoOK = (ahora - this.ultimoTiempo) > this.TIEMPO_RELECTURA;

        if (!mismo || tiempoOK) {
            this.ultimoCodigo = codigo;
            this.ultimoTiempo = ahora;
            this.onCodeDetected(codigo);
        }
    }

    async soporteFlash() {
        if (!this.track) return false;
        const cap = this.track.getCapabilities?.();
        return !!cap?.torch;
    }

    async toggleFlash() {
        if (!this.track) {
            alert("Primero abrí la cámara");
            return false;
        }

        const cap = this.track.getCapabilities?.();
        if (!cap?.torch) {
            alert("Este dispositivo/navegador no soporta flash");
            return false;
        }

        this.flashOn = !this.flashOn;
        await this.track.applyConstraints({
            advanced: [{ torch: this.flashOn }]
        });

        return this.flashOn;
    }

    // Programa la proxima vuelta de un bucle de lectura. Se guarda el timer para poder cancelarlo
    // en cerrar(), y la sesion para que un bucle de una apertura anterior no siga vivo.
    programarProximaLectura(sesion, funcion) {
        this.timerLectura = setTimeout(() => {
            this.timerLectura = null;
            funcion.call(this, sesion);
        }, this.INTERVALO_LECTURA_MS);
    }

    // Bucle de lectura del motor nativo (BarcodeDetector): analiza el cuadro completo.
    async leerContinuamente(sesion) {
        if (!this.scanning || sesion !== this.sesionId) return;

        try {
            const codigos = await this.detector.detect(this.video);
            if (codigos.length > 0 && this.scanning && sesion === this.sesionId) {
                this.procesarCodigoDetectado(codigos[0].rawValue);
            }
        } catch (e) {
            console.error("Error en detector:", e);
        }

        if (this.scanning && sesion === this.sesionId) this.programarProximaLectura(sesion, this.leerContinuamente);
    }

    // Recorta del video la franja que el usuario ve en el visor y la arma como imagen para ZXing.
    // El visor muestra el video con object-fit:cover (recorte centrado); aca se calcula ese mismo
    // recorte en coordenadas del video, asi lo que se lee es lo que se ve y ZXing no pierde tiempo
    // en zonas que el usuario no apunta. Devuelve null si el video todavia no tiene cuadros.
    crearBitmapFranja() {
        const v = this.video;
        const anchoVideo = v.videoWidth;
        const altoVideo = v.videoHeight;
        if (!anchoVideo || !altoVideo || v.readyState < 2) return null;

        let anchoFranja = anchoVideo;
        let altoFranja = altoVideo * 0.4; // si el visor no tiene medida (oculto), franja central del 40 %

        if (v.clientWidth > 0 && v.clientHeight > 0) {
            // object-fit:cover escala el video hasta cubrir el visor: se ve ancho/escala x alto/escala del original.
            const escala = Math.max(v.clientWidth / anchoVideo, v.clientHeight / altoVideo);
            anchoFranja = Math.min(anchoVideo, v.clientWidth / escala);
            altoFranja = Math.min(altoVideo, v.clientHeight / escala);
        }
        altoFranja = Math.max(altoFranja, altoVideo * BarcodeScanner.FRANJA_MIN_RELATIVA);

        const x = (anchoVideo - anchoFranja) / 2;
        const y = (altoVideo - altoFranja) / 2;

        const canvas = this.canvasFranja;
        canvas.width = Math.round(anchoFranja);
        canvas.height = Math.round(altoFranja);
        const ctx = canvas.getContext("2d", { willReadFrequently: true });
        ctx.drawImage(v, x, y, anchoFranja, altoFranja, 0, 0, canvas.width, canvas.height);

        const Z = window.ZXing;
        return new Z.BinaryBitmap(new Z.HybridBinarizer(new Z.HTMLCanvasElementLuminanceSource(canvas)));
    }

    // Bucle de lectura del motor de respaldo (ZXing). Que no se encuentre un codigo en el cuadro es lo
    // normal (NotFound/Checksum/Format): no se loguea. Cualquier otra excepcion si.
    async leerContinuamenteZXing(sesion) {
        if (!this.scanning || sesion !== this.sesionId) return;

        let texto = null;
        try {
            const bitmap = this.crearBitmapFranja();
            if (bitmap) {
                texto = this.zxingReader.decode(bitmap, this.zxingHints).getText();
            }
        } catch (e) {
            const Z = window.ZXing;
            const esNormal = e instanceof Z.NotFoundException
                || e instanceof Z.ChecksumException
                || e instanceof Z.FormatException;
            if (!esNormal) console.error("Error en ZXing:", e);
        } finally {
            this.zxingReader?.reset();
        }

        if (texto && this.scanning && sesion === this.sesionId) {
            this.procesarCodigoDetectado(texto);
        }

        if (this.scanning && sesion === this.sesionId) this.programarProximaLectura(sesion, this.leerContinuamenteZXing);
    }

    detenerStream() {
        if (this.stream) this.stream.getTracks().forEach(t => t.stop());
        this.stream = null;
    }

    cerrar() {
        this.scanning = false;
        this.sesionId++;
        if (this.timerLectura) {
            clearTimeout(this.timerLectura);
            this.timerLectura = null;
        }

        // opcional: apagar flash al cerrar
        if (this.track && this.flashOn) {
            this.track.applyConstraints({ advanced: [{ torch: false }] }).catch(() => { });
            this.flashOn = false;
        }

        this.zxingReader = null;
        this.zxingHints = null;
        this.canvasFranja = null;

        this.detenerStream();

        this.video.srcObject = null;
        this.container.style.display = "none";

        this.track = null;
        this.detector = null;
    }
}
