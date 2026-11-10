---
pantalla: Usuarios.Asistente
titulo: Asistente de usuarios y permisos
rol: admin
modulo: Usuarios
orden: 2
alias: Usuarios.GuardarAsistente
permiso: Usuario.NuevoUsuario (editar)
revisada: 2026-11-10
---
## Para qué sirve
Crear un usuario y **modificar** uno existente (datos, ingreso y seguridad, y permisos) en una sola pantalla, con los permisos agrupados por módulo y explicados en llano. Se llega desde [Usuarios](ayuda:pantalla/Usuarios.Index): tocando la fila de un usuario (o **Modificar** / **Permisos** en el menú ⋯ de la fila) para uno existente, o **Nuevo usuario** para un alta. La grilla completa anterior sigue disponible como **Vista avanzada de permisos** ([Permisos del usuario](ayuda:pantalla/Usuarios.Permisos)). La dirección vieja *Usuarios/Editar* redirige acá.

> PENDIENTE (2026-11-10): capturas y video de esta pantalla (rediseño del mismo día).

## Modificar un usuario existente: la ficha
Arriba, la **cabecera** muestra quién es (iniciales, nombre, usuario, email y sucursal) y su estado con etiquetas: tipo (Empleado / Administrador / Producción), Activo o Inactivo, **si entra al POS** (se actualiza solo al cambiar permisos), si tiene PIN y si está bloqueado. A la derecha, **Desbloquear** (solo si está bloqueado, pide confirmación) y **Quitar PIN** (solo si tiene PIN, pide confirmación).

Tres pestañas, **un solo guardado**:
- **Datos:** nombre, usuario (el que escribe para ingresar), email, sucursal, **clave nueva** (vacía = se mantiene la actual; el ojito 👁 deja ver lo que escribís) y **tipo de usuario** (Empleado / Administrador / Producción). Cambiar el tipo cambia qué permisos aplican: un administrador no necesita permisos y una cuenta de producción nunca tiene Ventas, POS, Finanzas, Fórmulas ni Personal.
- **Ingreso y seguridad:** interruptores **Activo**, **Requiere dispositivo seguro** y **Permitir ingreso fuera de la sucursal**, **minutos de inactividad** (0 = nunca) y el estado de la **clave rápida (PIN)**, que solo crea el propio usuario (si la olvidó puede crear otra con su contraseña, o vos se la quitás).
- **Permisos:** el editor de permisos (ver abajo). Con el contador de permisos con acceso.

Al **tocar cualquier cosa** aparece abajo una **barra flotante** «N cambios sin guardar» con **Descartar** (recarga la ficha con lo guardado) y **Guardar cambios**. Si intentás salir de la pantalla con cambios sin guardar, el navegador avisa. Al guardar se validan en el servidor las mismas reglas que siempre (usuario y email sin repetir, clave segura): los errores salen junto a cada campo y la ficha abre en la pestaña donde está el campo. Avisos al guardar: *"El usuario y sus permisos se actualizaron correctamente."*

Un administrador no usa los permisos: el editor se ve apagado con un aviso y lo que se toque ahí **no se guarda**.

## Crear un usuario nuevo: tres pasos
1. **Quién es:** elegí el **tipo de usuario** (tarjetas) y completá los **datos** (nombre, usuario, email, sucursal, clave inicial con ojito). Debajo, *Opciones de ingreso (opcional)*. Al tocar **Siguiente** se validan en el servidor las mismas reglas del guardado y los errores salen junto a cada campo. Si el tipo es **Administrador**, el paso 2 se saltea.
   - **Empleado con permisos:** solo puede hacer lo que le des en el paso siguiente. Es lo habitual.
   - **Administrador:** puede todo en su empresa, sin permisos.
   - **Cuenta de producción (PC compartida):** para la PC de un sector. Nunca tiene Ventas, POS, Finanzas, Fórmulas ni Personal (Empleados y Liquidación), sin importar lo que se elija.
2. **Qué puede hacer:** el mismo editor de permisos de la ficha.
3. **Revisar y crear:** lista, por módulo, lo que podrá hacer (con sus días y alcance) y si **entra al POS**. Si no eligió ningún permiso avisa que solo verá el inicio. **Crear usuario** guarda los datos y los permisos juntos: *"El usuario se creó correctamente."*

## El editor de permisos
Tres columnas: a la izquierda los **módulos** con un contador de permisos con acceso (por ejemplo *4/5*); en el centro los **permisos del módulo elegido**; a la derecha un **resumen en vivo** de lo que va a poder hacer (si entra al POS, qué tiene en cada módulo y una sección **Atención** con efectos colaterales, por ejemplo *puede vender sin caja abierta y cambiar la fecha de las ventas*). En pantallas angostas las columnas se apilan.

Los módulos son: **Ventas y POS, Caja, Compras, Stock y producción, Productos, Reportes, Finanzas, Personal y Configuración** (más *Otros* si el sistema suma un permiso nuevo). El **buscador** de arriba (*Buscar permiso…*) muestra los permisos de todos los módulos que coinciden con lo escrito. Cada permiso tiene un selector, junto a su nombre:
- **Sin acceso / Ver / Ver y modificar** en los permisos que tienen las dos cosas (debajo se lee qué habilita cada nivel).
- **Sin acceso / Sí** en los que tienen un solo nivel.

Solo aparecen los controles que **cambian algo** en ese permiso:
- **Ve hasta … atrás** (lista: *Solo hoy, 7, 14, 30, 90 días, Todo el historial* u *Otro…* para escribir una cantidad): hasta qué fecha vieja puede consultar. Aparece solo en Compras, Movimientos, Stock, Elaborados, Egresos de caja, Cuentas corrientes, POS - Cuentas corrientes de clientes y los reportes.
- **Modifica hasta … atrás:** hasta qué antigüedad puede modificar. Aparece solo donde el sistema compara con la fecha del registro (Compras, Movimientos, Stock, Elaborados, Pagos y cobros, POS - Cuentas corrientes de clientes y POS - Mis últimas ventas).
- **Qué registros** (*Solo los suyos* / *Todos*): aparece donde el sistema compara con quien creó el registro (Compras, Movimientos, Stock, Elaborados, Fórmulas, Cheques, Pagos y cobros).

En los demás permisos (por ejemplo Cheques, Cerrar caja o Bonificar) los días **no restringen nada** —el sistema mira siempre «hoy»—, por eso no se piden. Si el usuario ya tenía días guardados en un permiso así, se conservan.

**Todo el historial** se guarda como 3650 días (10 años).

### Perfiles y copiar
- **Perfil Vendedor / Cajero** y **Perfil Vendedor encargado:** cargan un conjunto de permisos para ajustar después. No queda guardado el perfil, solo los permisos.
  - **Vendedor / Cajero:** vende en el POS, ve sus ventas (**sin modificarlas**), ve la cuenta corriente de los clientes (30 días), las últimas 5 ventas de un cliente y carga egresos de caja.
  - **Vendedor encargado:** lo anterior y además **modifica sus ventas**, bonifica precios, carga compras y recibe mercadería (Movimientos: ver 30 días, cargar 7 días). Atención: modificar ventas (*POS - Mis últimas ventas → Ver y modificar*) también deja **vender sin tener la caja abierta y cambiar la fecha de la venta**.
- **Copiar de otro usuario** (lista): trae los permisos de otro usuario de la empresa. Los que tienen una combinación especial (ver abajo) no se copian y se avisa cuántos.
- **Quitar todos:** deja todo en *Sin acceso*.

Perfil, copiar y quitar todos **no piden confirmación**: avisan cuántos permisos cambiaron y ofrecen **Deshacer** unos segundos (también se pueden ajustar permiso por permiso después).

### «Entra al POS»
El indicador de arriba se pone en **Sí** con cualquiera de estos: *POS - Cuentas corrientes de clientes*, *POS - Bonificar precios*, *POS - Mis últimas ventas*, *POS - Últimas 5 ventas de un cliente*, o *Pagos y cobros → Ver y modificar* (comparte el permiso de cargar cobros). *Ventas (historial)* **no** da acceso al POS.

### Valor especial guardado
Si un usuario tiene una combinación que el asistente no sabe representar (por ejemplo, poder modificar un permiso sin poder verlo), el permiso muestra **«Valor especial guardado»**: se **conserva tal cual** al guardar. Para cambiarlo, tocá **Reemplazar** y elegí un nivel, o usá la [vista avanzada](ayuda:pantalla/Usuarios.Permisos).

## Reglas y efectos
- **Un usuario nuevo arranca sin ningún permiso** y se guarda exactamente lo que se ve en pantalla. Al crear un usuario por otro camino (por ejemplo el alta rápida de empresa), en cambio, el sistema le carga permisos por defecto (entre ellos poder modificar sus últimas ventas en el POS); con el asistente eso no pasa.
- **Guardar sin cambios no altera ningún permiso:** solo se escriben los permisos que el asistente muestra y con los valores que se ven.
- **No se muestran** los permisos de pantallas que solo existen en el sistema clásico: *Costo por tipo de cobro*, *Exportar lista de precios* e *Imprimir lista de precios en ticket*. La vista avanzada los muestra marcados *sin efecto hoy*.
- **Productos y Tipos de producto** ofrecen *Ver* y *Ver y modificar*: modificar es crear, modificar y eliminar (en Productos, también cambiar precios, administrar marcas, cargar puntos de stock y dar de alta un producto rápido desde el POS).
- **Etiquetas de productos** (módulo Productos) deja generar el PDF de etiquetas. Cada **reporte** tiene su permiso en el módulo **Reportes** (stock actual, cierre de stock, stock retroactivo, proyección de ventas vs stock, ventas por producto y balance económico): *Días ver* limita hasta cuántos días atrás puede poner la fecha «desde». El permiso **Stock** ya **no** habilita los reportes.
- **Cuentas corrientes** (entrar al listado y a la cuenta de cualquier persona, con sus días) y **Ver cuentas corrientes reservadas** (ver completas las cuentas reservadas y poder marcar a una persona como reservada) son permisos distintos. Sin el segundo, una cuenta reservada se ve con el saldo oculto y solo lo que cargó el propio usuario desde que abrió su caja. Ver [Cuentas corrientes](ayuda:pantalla/Finanzas.CtasCtes).
- **Un administrador no necesita permisos.**
- Si los datos del usuario se guardan pero los permisos fallan (son dos guardados seguidos), la pantalla vuelve a abrirse con el usuario ya creado y el motivo del error: reintentá desde la pestaña Permisos.
- Esta pantalla la ven los administradores y quienes tienen permiso de **edición** de usuarios; los demás vuelven al listado con un aviso.
