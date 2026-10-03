---
pantalla: AuditoriaLogin.Index
titulo: Auditoría de accesos
rol: admin
modulo: Acceso y seguridad
orden: 20
permiso: Admin o permiso de edición de usuarios
revisada: 2026-10-03
---
## Para qué sirve
Saber **quién ingresó, cuándo y desde dónde**, y revisar intentos fallidos, bloqueos y cambios de seguridad. Está en *Configuración → Auditoría de accesos*. Pueden entrar los administradores y quienes tengan permiso de **edición** de usuarios (el permiso de solo ver usuarios no alcanza).

## Cómo se usa
1. Elegí **Desde** y **Hasta** (por defecto, los últimos 7 días hasta hoy; "hasta" incluye todo el día).
2. Tocá **Buscar**.

## Qué muestra (más reciente primero, máximo 500 filas)
| Columna | Qué dice |
|---|---|
| **Usuario** | A quién corresponde el evento. |
| **Fecha / hora** | Cuándo ocurrió. |
| **Sucursal** | Sucursal del usuario en ese momento. |
| **Resultado** | *Permitido* (verde) o *Bloqueado* (rojo). |
| **Motivo** | Qué pasó, por ejemplo: *Login correcto*, *Login desde dispositivo seguro*, *Login con clave rápida (PIN)*, *Credencial incorrecta (N/7, …)*, *Usuario bloqueado por intentos fallidos*, *Desbloqueo solicitado por mail*, *Usuario desbloqueado por mail / por administrador*, *Dispositivo no autorizado*, *Dispositivo autorizado por administrador*, *Clave rápida (PIN) configurada / eliminada*. |
| **Dispositivo** | Desde qué dispositivo autorizado se ingresó (con el mail de alta si lo autorizó el empleado). *"Sin dispositivo registrado"* = ingresó desde uno no registrado. |
| **IP** | Desde qué conexión. |
| **Ver en Maps** | Solo si el evento trae ubicación. |

## Para qué mirarla
- Muchas filas **"Credencial incorrecta"** de un usuario: alguien está probando contraseñas. Si vienen de dispositivos no autorizados, ese usuario puede seguir entrando desde el suyo.
- **"Dispositivo no autorizado"** repetidos: un empleado que necesita que lo autorices (o alguien ajeno que conoce su contraseña: conviene cambiársela).
- Quién **desbloqueó** un usuario, quitó un PIN o aprobó un dispositivo, y cuándo.

## Límites
- Solo muestra usuarios de **tu empresa**.
- No hay paginación: si hay más de 500 eventos en el rango, achicá las fechas. **PENDIENTE:** la pantalla no avisa cuando corta en 500.
- No se registran todavía los ingresos rechazados por horario laboral ni los reintentos contra un usuario que ya estaba bloqueado.
