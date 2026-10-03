---
pantalla: SystemAdministration.AltaRapidaEmpresa
titulo: Alta rápida de empresa
rol: superadmin
modulo: Administración de la plataforma
orden: 5
alias: SystemAdministration.GuardarAltaRapidaEmpresa
permiso: Super administrador
revisada: 2026-10-03
---
## Para qué sirve
Dar de alta una empresa **completa** en una sola pantalla: la empresa, su **primera sucursal** y su **usuario administrador inicial**.

## Cómo se usa
1. **Datos de empresa:** razón social, **CUIT**, condición de IVA, nombre de fantasía, domicilio, teléfono, email y las marcas *Empresa propia* / *Es carnicería*. El botón de la **lupa** junto al CUIT **busca en AFIP/ARCA** (necesita un CUIT de 11 dígitos) y completa razón social, condición de IVA, domicilio y ciudad.
2. **Primera sucursal:** nombre, dirección, localidad, **punto de venta AFIP**, provincia y país.
3. **Usuario administrador inicial:** nombre, usuario, email, **Administrador** (viene tildado), clave y confirmación, **Usuario activo** (viene tildado).
4. **Producto "Código Genérico":** código, nombre y IVA.
5. **Crear empresa completa.** Aviso: *"La empresa, la sucursal inicial y el usuario administrador se crearon correctamente."* y vuelve al listado.

## Qué crea
- La **empresa** con todo lo de [Nueva empresa](ayuda:pantalla/SystemAdministration.EditarEmpresa) (parámetros copiados de la plantilla, productos *Ajuste de Fórmula* y *Código Genérico*).
- La **sucursal** (la que se crea por defecto se actualiza con los datos que cargaste) y el **usuario** en esa empresa y sucursal.
- **No** crea otros usuarios, ni abre cajas, ni carga permisos por módulo.

## Reglas y cuidados
- Valen las mismas validaciones que en empresa y usuario: CUIT único y válido, usuario y email únicos en todo el sistema, clave con la política de contraseñas.
- La creación ocurre en **dos pasos**; si fallara el segundo (sucursal/usuario), **la empresa ya queda creada**. Revisá el listado de [Empresas](ayuda:pantalla/SystemAdministration.Empresas) si ves un error.
- Si cargás un CUIT ya existente, el sistema lo rechaza: la empresa ya está dada de alta.
