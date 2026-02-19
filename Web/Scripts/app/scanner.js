class BarcodeScanner {
    constructor({ videoSelector, containerSelector, onCodeDetected }) {
        this.video = document.querySelector(videoSelector);
        this.container = document.querySelector(containerSelector);
        this.onCodeDetected = onCodeDetected;

        this.scanning = false;
        this.detector = null;
        this.stream = null;

        this.track = null;     // ✅ para torch
        this.flashOn = false;  // ✅ estado flash

        this.ultimoCodigo = null;
        this.ultimoTiempo = 0;
        this.TIEMPO_RELECTURA = 2000;
    }

    async iniciar() {
        if (this.scanning) return;
        this.scanning = true;
        this.container.style.display = "";

        if (!("BarcodeDetector" in window)) {
            alert("Este navegador no soporta BarcodeDetector");
            this.scanning = false;
            return;
        }

        this.detector = new BarcodeDetector({ formats: ["ean_13", "ean_8", "code_128", "upc_a"] });

        this.stream = await navigator.mediaDevices.getUserMedia({
            video: {
                facingMode: { ideal: "environment" },
                width: { ideal: 640 },
                height: { ideal: 480 }
            }
        });

        this.video.srcObject = this.stream;

        // ✅ guardamos el track para el flash
        this.track = this.stream.getVideoTracks()[0];

        await this.video.play();
        this.leerContinuamente();
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

    async leerContinuamente() {
        if (!this.scanning) return;

        try {
            const codigos = await this.detector.detect(this.video);
            if (codigos.length > 0) {
                const codigo = codigos[0].rawValue;

                const ahora = Date.now();
                const mismo = codigo === this.ultimoCodigo;
                const tiempoOK = (ahora - this.ultimoTiempo) > this.TIEMPO_RELECTURA;

                if (!mismo || tiempoOK) {
                    this.ultimoCodigo = codigo;
                    this.ultimoTiempo = ahora;
                    this.onCodeDetected(codigo);
                }
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

        if (this.stream) this.stream.getTracks().forEach(t => t.stop());

        this.video.srcObject = null;
        this.container.style.display = "none";

        this.track = null;
        this.stream = null;
        this.detector = null;
    }
}
