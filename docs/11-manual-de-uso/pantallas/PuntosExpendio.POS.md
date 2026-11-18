---
pantalla: PuntosExpendio.POS
titulo: Punto de expendio (pantalla de trabajo y app instalada)
rol: usuario
modulo: Punto de Expendio
orden: 1
permiso: cualquier usuario activo puede operar Expendio (verificado en `PuntosExpendioController.POS`); no pide permiso POS ni apertura de caja
revisada: 2026-11-17
---
## Para qué sirve
Pantalla de trabajo del **Punto de expendio**: se elige el sector y se cargan los expendios. **Esta ayuda cubre solo cómo usarlo como aplicación instalada (desde 2026-11-17).** El resto de la pantalla está PENDIENTE de documentar.

## Como aplicación instalada
- Se instala desde **Configuración → Utilidades → Aplicaciones instalables → Punto de expendio → Instalar** (ver [Utilidades](ayuda:pantalla/Home.Utilidades)). Queda con el nombre **Punto de expendio**, su propio ícono y su propia ventana.
- Abre directo en esta pantalla, sin menú lateral. Los enlaces a otras secciones del sistema no funcionan en esta ventana: muestran «No disponible en esta ventana».
- El botón **Salir** (← arriba) y **Cancelar** al elegir el sector **cierran la ventana** en lugar de ir a Inicio. Si hay un expendio a medio cargar, el navegador pregunta antes de cerrar.
- Si se vence la sesión, la ventana pide usuario y clave y vuelve a esta pantalla.
- **Sin menú solo en las apps de Utilidades:** únicamente las apps instaladas desde **Utilidades** abren sin menú lateral. Si usás la app general **CarniSys** (o una pestaña del navegador) y entrás a esta pantalla, el **menú se ve** y los enlaces funcionan; la ventana de **Abrir en ventana** sí va sin menú. Si instalaste esta app antes del 2026-11-17, **desinstalala y reinstalala desde Utilidades** para que tome esta regla (Stock, Movimientos y Elaborados ya la traían).
- PENDIENTE: no se probó la instalación real en producción.
