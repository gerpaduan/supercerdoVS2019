# Incidencias frecuentes

## Objetivo

Registrar fallas repetidas, sintomas, diagnostico y resolucion conocida.

## Secciones

- Sintoma
- Causa probable
- Verificaciones
- Resolucion

---

## 2026-07-29 - "Error de instancia" al iniciar sesion en la web

- **Sintoma**: `System.InvalidOperationException: Error de instancia.` en `Utilidades\Conexion.cs` linea 72 (`cn.Open()`) al hacer login.
- **Causa**: en `Web\Config\connectionStrings.config` el `Data Source` estaba escrito `.\\sqlexpress` (doble backslash). El parser de connection strings de .NET no interpreta el backslash como escape, por lo que busca una instancia literal inexistente.
- **Verificacion**: abrir una `SqlConnection` con esa cadena falla; con `.\sqlexpress` devuelve `ServerVersion` y `Database`.
- **Resolucion**: dejar un solo backslash: `Data Source=.\sqlexpress`. El archivo es local y esta fuera de git (ver `connectionStrings.config.example`).

## 2026-07-29 - Atajos de teclado muertos en Cuenta Corriente

- **Sintoma**: en `Finanzas/CtaCtePersona` no respondia ningun atajo (Alt+Enter / Alt++ para nuevo pago, Alt+F, Alt+S, Enter en "Desde") ni los botones de enviar por mail.
- **Causa**: el layout carga jQuery al final del `<body>`, pero el script inline de la vista se ejecuta antes. El `$(document)` de ese bloque lanzaba `ReferenceError: $ is not defined` y cortaba la funcion antes de llegar a `bindAtajosCuentaCorrienteCaptura()`, dejando todo el bloque sin efecto.
- **Verificacion**: en el navegador, `window.__ctacteAtajosCapturaBound` quedaba `false` al terminar de cargar la pagina.
- **Resolucion**: envolver el bloque en `inicializarCtaCtePersona()` y llamarlo desde un `esperarJQuery()` (mismo patron que `Views\Personas\Editar.cshtml`). Aplica a cualquier vista que corra scripts con jQuery fuera de una seccion `scripts`.
