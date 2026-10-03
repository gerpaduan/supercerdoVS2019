---
titulo: Mapa de configuraciones de la empresa
rol: admin
modulo: Referencia
orden: 1
revisada: 2026-10-03
---
# Mapa de configuraciones de la empresa

Cada fila es **un interruptor o valor que cambia el comportamiento del sistema**: dónde se cambia, quién puede, cómo viene y qué efecto tiene. *Esta tabla se mantiene al día cada vez que se agrega una configuración.*

## Acceso y seguridad
| Configuración | Dónde se cambia | Quién | Por defecto | Efecto |
|---|---|---|---|---|
| **Exigir dispositivo seguro a no-administradores** | [Mi Empresa](ayuda:pantalla/Empresa.Index) → *Dispositivos seguros* | Admin | Apagado | **Encendido:** los empleados (no admin) solo ingresan desde dispositivos autorizados. **Apagado:** ingresan desde cualquiera con su contraseña. Los admin siempre pueden. Ver [dispositivo seguro](ayuda:concepto/dispositivo-seguro). |
| **Requiere dispositivo seguro** (por usuario) | [Editar usuario](ayuda:pantalla/Usuarios.Editar) | Admin | Apagado | Le exige dispositivo autorizado a ese usuario aunque el de la empresa esté apagado. No aplica a admin. |
| **Dispositivos autorizados** | [Dispositivos seguros](ayuda:pantalla/DispositivosSeguros.Index) | Admin | Ninguno | Lista de PCs/celulares desde los que se puede ingresar. Se cargan a mano, por el empleado con código al mail, o aprobando solicitudes. |
| **Horario laboral** (2 jornadas) | [Mi Empresa](ayuda:pantalla/Empresa.Index) → *Horario laboral* | Admin | 00:00 a 23:59 (sin restricción) | Los empleados (no admin) solo pueden iniciar sesión dentro de las 2 jornadas. Los admin están exentos. |
| **Clave rápida (PIN)** | El propio usuario, menú → [Clave rápida](ayuda:pantalla/Login.ChangePin) | Cada usuario (no admin) | Sin PIN | Permite ingresar con 4-6 dígitos **solo en dispositivo autorizado**. Un admin puede quitárselo desde [Usuarios](ayuda:pantalla/Usuarios.Index). |
| **Intentos de contraseña antes del bloqueo** | Configuración del servidor (`Security:AccountLockoutMaxAttempts`) | Quien administra el servidor | **7** por usuario | Al llegar al máximo se bloquea: desde dispositivo autorizado, todo el usuario; desde otro, solo el ingreso desde dispositivos no autorizados. Ver [bloqueo de usuarios](ayuda:concepto/bloqueo-de-usuarios). |
| **Aviso de intentos restantes** | Fijo en el sistema | — | Desde el 3.er error | Muestra "Te quedan N intentos". |
| **Política de contraseña** | Fija en el sistema | — | 8+ caracteres, letra + número + carácter especial | Aplica a contraseñas nuevas o cambiadas. Las anteriores siguen valiendo hasta cambiarlas. |
| **Permitir login fuera de sucursal** | [Editar usuario](ayuda:pantalla/Usuarios.Editar) | Admin | Apagado | Exceptúa a ese no-admin de la validación de ubicación (GPS) que tienen algunas sucursales. |
| **Dirección de ingreso de la empresa** | Automática: `/Login/<CUIT>` | — | — | Con un dispositivo autorizado muestra la lista de usuarios y acepta PIN. Ver [ingresar con el CUIT](ayuda:concepto/login-por-cuit). |

## Usuarios y permisos
| Configuración | Dónde se cambia | Quién | Por defecto | Efecto |
|---|---|---|---|---|
| **Admin** | [Editar usuario](ayuda:pantalla/Usuarios.Editar) | Admin | No | Acceso total a su empresa y exención de horario y dispositivo seguro. No puede ser usuario de producción. |
| **Activo** | [Editar usuario](ayuda:pantalla/Usuarios.Editar) | Admin | Sí (alta) | Un usuario inactivo no puede ingresar. |
| **Usuario de producción** | [Editar usuario](ayuda:pantalla/Usuarios.Editar) | Admin | No | Cuenta compartida de un sector: sin Ventas/Finanzas/Fórmulas; al guardar se elige quién lo hizo. No puede ser Admin. |
| **Puede operar POS** | [Editar usuario](ayuda:pantalla/Usuarios.Editar) | Admin | **Sí** (alta) | Otorga el permiso mínimo de Ventas para usar el POS (edición solo del día). Se apaga solo si es usuario de producción. |
| **Permisos por módulo** | [Permisos del usuario](ayuda:pantalla/Usuarios.Permisos) | Admin | Según alta | Ver/editar, días hacia atrás y alcance (propios/todos). |
| **Sucursal por defecto del usuario** | [Editar usuario](ayuda:pantalla/Usuarios.Editar) y menú → *Sucursal* | Admin / el propio usuario | Sin sucursal | En qué sucursal opera el usuario. |

## Datos de la empresa
| Configuración | Dónde se cambia | Quién | Efecto |
|---|---|---|---|
| Nombre de fantasía, teléfono, email, domicilio, ciudad, país, slogans | [Mi Empresa](ayuda:pantalla/Empresa.Index) | Admin | Datos que se muestran en comprobantes y pantallas. |
| Razón social (AFIP) y CUIT | [Editar empresa](ayuda:pantalla/SystemAdministration.EditarEmpresa) | **Super admin** | No se editan desde Mi Empresa. |
