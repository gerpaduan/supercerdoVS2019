---
pantalla: DispositivosSeguros.Index
titulo: Dispositivos seguros
rol: admin
modulo: Acceso y seguridad
orden: 10
permiso: Admin de la empresa
revisada: 2026-10-22
---
## Para qué sirve
Administrar los **dispositivos autorizados** para ingresar cuando la empresa (o un usuario) exige [dispositivo seguro](ayuda:concepto/dispositivo-seguro). Está en *Configuración → Dispositivos seguros*, y el ítem lo ve **cualquier usuario**, pero la pantalla solo la opera un **administrador**:
- Si entrás como administrador, se abre directo.
- Si entrás como usuario de producción o como usuario común, primero se pide el **usuario y la contraseña de un administrador** (el mismo tipo de login que se pide para entrar a Ventas o Compras; el combo lista solo administradores). Ese administrador queda activo **hasta que salgas de la pantalla**: al ir a otra sección y volver, se pide de nuevo. Tras 5 contraseñas incorrectas seguidas se bloquea unos minutos.
- Todo lo que se haga queda a nombre de ese administrador (por ejemplo, quién creó o aprobó un dispositivo).

## Qué ves
**1. Solicitudes de autorización pendientes** (tarjeta amarilla, solo si hay): pedidos de empleados desde la pantalla *"Este dispositivo no está autorizado"*. Por cada una: usuario, nombre del dispositivo y mensaje, IP, fecha y:
- **Aprobar:** da de alta el dispositivo (origen *Solicitud*). Antes podés **editar el nombre** del campo que está al lado del botón. El empleado ya puede ingresar desde ahí.
- **Rechazar:** descarta el pedido (pide confirmación).
Si el dispositivo ya está cargado **y bloqueado**, no se re-autoriza: primero hay que desbloquearlo en el listado.

**2. Agregar un dispositivo a mano:**
- **Número de serie:** el identificador del equipo. En una PC con el agente de impresión de CarniSys se completa solo ("Detectado automáticamente"); si no, se tipea (el empleado lo ve en el menú de su usuario → *Número de serie*, o en la pantalla de dispositivo no autorizado).
- **Descripción** (hasta 200 caracteres), por ejemplo *"Notebook de Juan"*.
- **Agregar.** El origen queda *Manual*. El número de serie es obligatorio.

**3. Listado de dispositivos:** número de serie (los navegadores se muestran como *"Navegador (xxxxxxxx…)"*), descripción, **origen** (*Manual*, *Autoservicio* si el empleado lo autorizó con el código al mail, *Solicitud* si lo aprobaste vos), mail de alta, fecha, quién lo creó, **estado** (Activo / Bloqueado) y **fichaje**.

## Acciones de cada dispositivo
- **Bloquear / Desbloquear:** un dispositivo bloqueado deja de servir para ingresar y el empleado **no puede volver a autorizarlo por su cuenta**. Úsalo si se perdió un equipo o se fue un empleado.
- **Habilitar fichaje / Deshabilitar fichaje:** independiente del bloqueo. Un dispositivo habilitado permite que **cualquier empleado marque entrada y salida sin contraseña** en *Jornadas → Fichaje* (pensado para una tablet fija en el local).
- **No bloquear sesión / Volver a bloquear sesión:** por defecto la sesión se bloquea por inactividad en todos los dispositivos. Con *No bloquear sesión*, en ese dispositivo no se avisa ni se bloquea, sea cual sea el usuario (pensado para la PC propia de un administrador). La columna **Sesión** muestra *Se bloquea* o *No se bloquea*. **Rige desde el próximo inicio de sesión** desde esa PC. Ver [bloqueo por inactividad](ayuda:concepto/bloqueo-por-inactividad).
- **Eliminar:** lo saca de la lista (pide confirmación).

## Reglas y efectos
- Un dispositivo autorizado además **no queda frenado por la IP** cuando alguien se equivoca de contraseña; el bloqueo por usuario (7 errores) sigue aplicando.
- Aprobar y rechazar solicitudes, y cada ingreso desde estos dispositivos, queda en la [Auditoría de accesos](ayuda:pantalla/AuditoriaLogin.Index). **Bloquear, habilitar fichaje, no bloquear sesión y eliminar no dejan registro de quién lo hizo.**
- Si el servidor no tiene aplicada la tabla de solicitudes, la tarjeta de pendientes simplemente no aparece. **PENDIENTE:** confirmar si en Postgres se pueden cargar dos veces el mismo número de serie (en SQL Server no).

## Configuraciones relacionadas
[Exigir dispositivo seguro (Mi Empresa)](ayuda:pantalla/Empresa.Index) · [Requiere dispositivo seguro (por usuario)](ayuda:pantalla/Usuarios.Editar) · [mapa de configuraciones](ayuda:referencia/configuraciones)
