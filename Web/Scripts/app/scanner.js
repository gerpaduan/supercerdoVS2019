class BarcodeScanner {
    constructor({ videoSelector, containerSelector, onCodeDetected }) {
        this.video = document.querySelector(videoSelector);
        this.container = document.querySelector(containerSelector);
        this.onCodeDetected = onCodeDetected;

        this.scanning = false;
        this.detector = null;   // motor nativo (BarcodeDetector), solo Chromium
        this.stream = null;

        this.zxingReader = null;    // motor de respaldo (ZXing), para Firefox/Safari/iOS

        this.track = null;     // ✅ para torch
        this.flashOn = false;  // ✅ estado flash

        this.ultimoCodigo = null;
        this.ultimoTiempo = 0;
        this.TIEMPO_RELECTURA = 2000;
    }

    // Formatos soportados por el ticket/POS. Se usan tanto para el detector nativo
    // como para el fallback de ZXing, asi ambos motores leen exactamente lo mismo.
    static get FORMATOS_NATIVOS() {
        return ["ean_13", "ean_8", "code_128", "upc_a"];
    }

    async iniciar() {
        if (this.scanning) return;
        this.scanning = true;
        this.container.style.display = "";

        const usaDetectorNativo = "BarcodeDetector" in window;
        const usaZXing = !usaDetectorNativo && typeof window.ZXing !== "undefined";

        if (!usaDetectorNativo && !usaZXing) {
            alert("Este navegador no soporta lectura de codigos de barra.");
            this.scanning = false;
            return;
        }

        this.stream = await navigator.mediaDevices.getUserMedia({
            video: {
                facingMode: { ideal: "environment" },
                width: { ideal: 640 },
                height: { ideal: 480 }
            }
        });

        this.video.srcObject = this.stream;

        // ✅ guardamos el track para el flash (funciona igual con cualquiera de los dos motores)
        this.track = this.stream.getVideoTracks()[0];

        await this.video.play();

        if (usaDetectorNativo) {
            // Chrome/Edge: Shape Detection API nativa, no requiere libreria externa.
            this.detector = new BarcodeDetector({ formats: BarcodeScanner.FORMATOS_NATIVOS });
            this.leerContinuamente();
            return;
        }

        // Firefox y Safari/iOS no implementan BarcodeDetector: se decodifica con ZXing
        // leyendo la misma camara (this.stream) frame a frame.
        const hints = new Map();
        hints.set(window.ZXing.DecodeHintType.POSSIBLE_FORMATS, [
            window.ZXing.BarcodeFormat.EAN_13,
            window.ZXing.BarcodeFormat.EAN_8,
            window.ZXing.BarcodeFormat.CODE_128,
            window.ZXing.BarcodeFormat.UPC_A
        ]);
        this.zxingReader = new window.ZXing.BrowserMultiFormatReader(hints);

        // decodeFromStream no devuelve un objeto de control utilizable en la version
        // vendorizada: el corte real del escaneo se hace deteniendo this.stream en
        // cerrar() (funciona igual para los dos motores).
        await this.zxingReader.decodeFromStream(this.stream, this.video, (result) => {
            if (!result || !this.scanning) return;
            this.procesarCodigoDetectado(result.getText());
        });
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

    // Bucle de lectura del motor nativo (BarcodeDetector). ZXing maneja su propio
    // bucle internamente via decodeFromStream, por eso este metodo solo aplica a Chromium.
    async leerContinuamente() {
        if (!this.scanning) return;

        try {
            const codigos = await this.detector.detect(this.video);
            if (codigos.length > 0) {
                this.procesarCodigoDetectado(codigos[0].rawValue);
            }
        } catch (e) {
            console.error("Error en detector:", e);
        }

        if (this.scanning) setTimeout(() => this.leerContinuamente(), 200);
    }

    cerrar() {
        this.scanning = false;

        // opcional: apagar flash al cerrar
        if (this.track && this.flashOn) {
            this.track.applyConstraints({ advanced: [{ torch: false }] }).catch(() => { });
            this.flashOn = false;
        }

        this.zxingReader = null;

        if (this.stream) this.stream.getTracks().forEach(t => t.stop());

        this.video.srcObject = null;
        this.container.style.display = "none";

        this.track = null;
        this.stream = null;
        this.detector = null;
    }
}
