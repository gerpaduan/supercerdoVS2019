---
titulo: Un doble clic en Guardar no guarda dos veces
rol: usuario
modulo: Uso general
orden: 31
revisada: 2026-11-26
---
# Un doble clic en Guardar no guarda dos veces

## Qué es
Desde el 2026-11-26, en **Stock** (cargar o modificar), **Movimientos**, **Elaborados** (carga e ingreso rápido / desarme) y **Pagos y cobros**, el mismo guardado queda grabado **una sola vez** aunque:
- toques **Guardar** dos veces seguidas (doble clic o doble toque en el celular),
- aprietes **Enter** o **Alt + Enter** varias veces,
- se corte la conexión justo al guardar y vuelvas a tocar **Guardar** con los mismos datos.

Antes, en Stock cada clic creaba otro registro: el 2026-11-26 en San Martín quedaron dos «Egreso Stock» iguales de Carré (−6,5 kg) guardados con un tercio de segundo de diferencia.

## Cómo se ve
- En **Stock**, al tocar **Guardar stock** el botón pasa a **«Guardando…»** y no responde hasta que se abre el listado. Si un aviso frena el guardado (por ejemplo *«El campo Código tiene un valor sin agregar»* o *«Guardar en cero»*), el botón sigue disponible.
- En las otras pantallas no cambia nada a la vista: los envíos de más se ignoran.
- Si el sistema recibe el mismo guardado otra vez, responde lo mismo que la primera vez (el aviso de guardado y el mismo número de registro) **sin crear otro**.
- Si el primer guardado todavía no terminó después de unos 30 segundos, aparece *«Ese guardado ya se está procesando. Esperá unos segundos y revisá el listado antes de volver a guardar.»*

## Qué sí se guarda de nuevo
- Si **cambiás algo** (otra cantidad, otro producto, otra fecha) y guardás, se guarda: solo se frena un envío con **los mismos datos**.
- Pasados **10 minutos**, un envío igual desde la misma pantalla abierta se guarda como uno nuevo. La pantalla vuelve a tener un número propio cada vez que se abre.
- Si el sistema se reinicia justo entre los dos envíos, el segundo se guarda (el control vive en la memoria del servidor).

## Detalle técnico
`WebCore/Helpers/GuardadoUnico.cs` y `docs/DECISIONS.md` (2026-11-26 d). Se configura en App.config: `GuardadoUnico:VentanaSegundos` (600), `GuardadoUnico:VentanaSinTokenSegundos` (30) y `GuardadoUnico:EsperaMaxSegundos` (30).
