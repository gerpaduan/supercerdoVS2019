# Evaluacion de Web

## Proyecto

`Web`

## Funcion actual

Aplicacion web `ASP.NET MVC` existente dentro de la solucion actual.

## Evidencia relevada

- Proyecto web `ASP.NET MVC 5` sobre `.NET Framework 4.7.2`.
- Configurado con `UseIISExpress=true`, `System.Web.Mvc 5.2.7` y dependencias clasicas de `System.Web`.
- Superficie funcional intermedia: `77` archivos `.cs` relevados.
- Controladores visibles por modulo: `Cajas`, `Compras`, `Elaborados`, `Finanzas`, `Movimientos`, `Parametros`, `Personas`, `PuntosExpendio`, `Productos`, `Reportes`, `Stock`, `Usuarios`, `Ventas`, `WhatsApp`, `SystemAdministration`.
- Modelos ya orientados a UI web y DTOs para ventas, punto de expendio, catalogo global y facturacion.
- Filtros y `BaseController` consumen `Negocio` directamente.
- Incluye descargas de agentes como `Carnisys.Balanza.Agent.zip` y `CarniSys.PrintAgent.zip`, lo que confirma integracion con componentes externos.
- Se toma como dato funcional confirmado que este proyecto web ya fue trabajado con criterio de UX operativa, priorizando atajos y reduciendo en lo posible el uso del mouse.

## Responsabilidad principal

- Flujos web ya implementados.
- Pantallas compartidas o complementarias del sistema.

## Dependencias visibles

- MVC clasico
- Modelos, controladores y vistas legacy
- Posibles referencias a Negocio, Datos o Entidades
- `Negocio` consumido desde filtros y controladores base
- `System.Web`
- `IIS Express`
- scripts y assets front-end tradicionales

## Nivel de acoplamiento

- Alto

## Compatibilidad con .NET moderno

- Media

## Compatibilidad con Linux

- Baja

## Valor funcional acumulado

- Alto

## Riesgo operativo si se toca mal

- Alto

## Problemas o deuda tecnica probable

- Arquitectura previa a ASP.NET Core.
- Posible mezcla de vista, permisos y reglas de negocio.
- Dependencias historicas de framework.
- Acoplamiento a `Negocio` legacy.
- Dependencia a hosting y pipeline de `System.Web`.
- Superficie UI y permisos acoplada a MVC clasico en lugar de arquitectura NG.

## Estrategia sugerida

- Reutilizar parcialmente

## Justificacion

- Conviene relevar sus flujos y rescatar decisiones utiles, pero no arrastrar su estructura como base directa de NG.
- Aporta valor especial porque ya contiene caminos web para `Ventas`, `PuntosExpendio`, `Catalogo Global` y permisos.
- Tambien aporta valor como referencia de experiencia de uso web orientada a teclado, no solo como referencia de negocio.
- No conviene reutilizarlo como base tecnica porque seguiria atando NG a `System.Web`, `IIS Express` y al modelo legacy de `Negocio`.

## Dependencia temporal permitida en NG

- No

## Condiciones para depender temporalmente

- Solo como referencia de comportamiento o apoyo transitorio fuera del core de NG.

## Estrategia de salida futura

- Migrar gradualmente sus flujos a `ASP.NET Core` con arquitectura nueva.

## Estado del relevamiento

- Confirmado en primera pasada
