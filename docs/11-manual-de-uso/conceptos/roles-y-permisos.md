---
titulo: Roles y permisos (quién puede hacer qué)
rol: usuario
modulo: Conceptos generales
orden: 1
revisada: 2026-10-03
---
# Roles y permisos

En CarniSys hay cuatro tipos de usuario. Lo que ves en el menú y lo que podés hacer depende de cuál sos.

| Tipo | Quién es | Qué puede hacer |
|---|---|---|
| **Super administrador** | Quien administra la **plataforma** (todas las empresas). Se marca aparte en el usuario; no es lo mismo que "Admin". | Menú **Administración del sistema**: crear y editar empresas, sucursales y usuarios de cualquier empresa, alta rápida de empresa. |
| **Administrador de empresa** (*Admin*) | El responsable de **una** empresa. | Todo lo de su empresa: ve el menú **Configuración** (Usuarios, Auditoría de accesos, Parámetros, Mi Empresa, Mis Sucursales, Dispositivos seguros), el dashboard con datos y puede todo en los módulos. **No le aplican** el horario laboral ni la exigencia de dispositivo seguro, y **no usa PIN**. |
| **Usuario con permisos** | Un empleado. No es Admin. | Solo lo que el administrador le tildó en **Usuarios → Ver permisos** (por módulo: ver, editar, cuántos días hacia atrás y si ve todo o solo lo suyo). |
| **Usuario de producción** | Una cuenta **compartida** para la PC de un sector (varios operarios). Nunca es Admin. | No tiene Ventas ni Finanzas ni Fórmulas, sin importar los permisos tildados. Al guardar movimientos, stock o elaborados, no pide contraseña: se elige de una lista **quién** lo hizo. |

## Cómo funcionan los permisos de un usuario común
En [Permisos del usuario](ayuda:pantalla/Usuarios.Permisos) hay una fila por módulo con:
- **Puede ver / Días ver:** si puede consultar y hasta cuántos días hacia atrás (0 = solo hoy).
- **Puede editar / Días editar:** si puede modificar registros y de hasta cuántos días atrás.
- **Alcance:** *Propios* (solo lo que cargó él) o *Todos*.

Si un usuario entra a una pantalla sin permiso, ve **"Acceso denegado"**. Además, **"Puede operar POS"** (en [Editar usuario](ayuda:pantalla/Usuarios.Editar)) le da el permiso mínimo para vender.

> Mostrar u ocultar un ítem del menú **no es** el control de seguridad: el permiso se verifica siempre en el servidor.

## Dónde se configura cada cosa
Ver el [mapa de configuraciones](ayuda:referencia/configuraciones) para saber qué interruptor existe, quién lo cambia y qué efecto tiene.
