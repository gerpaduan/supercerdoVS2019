---
pantalla: SystemAdministration.EditarUsuario
titulo: Nuevo / Modificar usuario (administración del sistema)
rol: superadmin
modulo: Administración de la plataforma
orden: 4
alias: SystemAdministration.GuardarUsuario
permiso: Super administrador
revisada: 2026-10-03
---
## Para qué sirve
Crear o modificar un usuario **de cualquier empresa**. Se llega desde [Usuarios](ayuda:pantalla/SystemAdministration.Usuarios). Botón final: **Guardar usuario**.

## Campos
- **Empresa** (obligatoria) y **Sucursal** (la lista se actualiza al cambiar de empresa; *"Sin sucursal por defecto"* es válido).
- **Nombre** y **Usuario** (obligatorios), **Email**.
- **Clave** y **Confirmar clave:** obligatorias en un alta; en una edición, vacías = se conserva la actual. Deben cumplir: mínimo 8 caracteres, con letra, número y un carácter especial, sin espacios, distinta del usuario y no demasiado común.
- Interruptores: **Administrador**, **Activo**, **Permitir login fuera de sucursal**.

## Reglas
- El **usuario** y el **email** son únicos **en todo el sistema** (todas las empresas), sin distinguir mayúsculas.
- Acá se puede **mover un usuario a otra empresa**.
- Esta pantalla **no** tiene *Usuario de producción*, *Requiere dispositivo seguro* ni *Puede operar POS*: esos ajustes los hace el administrador de la empresa en [Editar usuario](ayuda:pantalla/Usuarios.Editar). Tampoco aplica la regla de "usuario admin reservado".
