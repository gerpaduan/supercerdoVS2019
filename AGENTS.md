# AGENTS.md

## Prioridad máxima
Este proyecto WinForms tiene partes en producción que ya funcionan correctamente.
La prioridad principal es NO romper comportamiento existente.

## Regla principal
Antes de modificar cualquier archivo:
- leer y entender el flujo actual
- identificar dependencias
- preservar compatibilidad
- evitar refactors innecesarios
- hacer cambios mínimos, puntuales y reversibles

## Restricciones importantes
- No reescribir módulos completos si no es estrictamente necesario.
- No cambiar nombres de métodos públicos, eventos, formularios o controles ya utilizados.
- No cambiar firmas de métodos existentes salvo necesidad explícita.
- No mover lógica entre capas si eso puede alterar comportamiento actual.
- No eliminar código existente sin verificar primero para qué se usa.
- No modificar inicialización de formularios, bindings, DataSources o eventos sin revisar impacto.
- No cambiar consultas SQL, transacciones o acceso a datos que ya funcionan, salvo en el punto exacto requerido.
- No tocar configuraciones de impresión, balanzas, serial ports, facturación o sincronización si el cambio pedido no está relacionado con eso.
- No romper compatibilidad con .NET Framework, WinForms, librerías actuales ni con el entorno de producción.

## Sobre WinForms
Prestar especial atención a:
- eventos de botones, grillas, combos y timers
- código en Load, Shown, FormClosing y eventos de controles
- acceso a DataTables, BindingSource y DataGridView
- variables compartidas entre formularios
- flujos de permisos, validaciones y mensajes al usuario
- comportamiento visual que el usuario ya conoce

## Forma de trabajar
- Hacer primero un análisis del flujo afectado.
- Explicar qué archivo o método se va a tocar y por qué.
- Proponer el cambio más chico posible.
- Mantener intacto todo lo que no esté directamente relacionado con el pedido.
- Si hay dos caminos posibles, elegir el menos invasivo.
- Antes de terminar, revisar posibles efectos colaterales.

## Al modificar código
- Respetar estilo y estructura existente del proyecto.
- Mantener nombres y patrones actuales.
- No introducir arquitectura nueva sin necesidad.
- No “mejorar” código que no fue pedido cambiar.
- No simplificar código si eso cambia comportamiento.
- Si hay lógica vieja pero estable, priorizar estabilidad antes que prolijidad.

## Validación obligatoria
Antes de dar el trabajo por terminado:
- verificar que no se rompa el flujo actual
- revisar eventos asociados
- revisar referencias cruzadas del método o formulario modificado
- confirmar que el cambio no afecta otras pantallas
- detallar qué se modificó exactamente

## Cuando haya dudas
Si una modificación puede afectar algo que ya funciona en producción:
- frenar
- advertir el riesgo
- elegir la opción más conservadora

## Objetivo
Aplicar solamente el cambio solicitado, con el menor impacto posible, sin romper nada de lo que ya funciona en WinForms y está en producción.