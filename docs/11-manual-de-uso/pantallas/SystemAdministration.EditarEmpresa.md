---
pantalla: SystemAdministration.EditarEmpresa
titulo: Nueva / Modificar empresa
rol: superadmin
modulo: Administración de la plataforma
orden: 2
alias: SystemAdministration.GuardarEmpresa
permiso: Super administrador
revisada: 2026-10-03
---
## Para qué sirve
Crear o modificar una empresa de la plataforma. Se llega desde [Empresas](ayuda:pantalla/SystemAdministration.Empresas).

## Cómo se usa
Completá los datos y tocá **Guardar empresa**. Avisos: *"La empresa se creó correctamente."* / *"La empresa se actualizó correctamente."*

## Campos
- **Obligatorios:** Razón social, **CUIT** y **Condición IVA**.
- Datos generales: nombre de fantasía, ingresos brutos, inicio de actividad, domicilio, ciudad, país, teléfono, email, slogans y observaciones.
- Técnicos: *Tenant slug*, *Base path*, *Base de datos*, *Certificado PFX* y **Entorno AFIP** (PROD / HOMO; **HOMO = modo prueba, las facturas no tienen validez fiscal**).
- Marcas: *Empresa RRII*, **Empresa activa**, *Empresa propia*, *Es carnicería*. **PENDIENTE:** documentar el efecto exacto de cada marca en los módulos.
- **Producto "Código Genérico"** (solo en el alta): código (por defecto 999999), nombre y alícuota de IVA. Se crea junto con la empresa y sirve para vender algo sin un código cargado en el catálogo.

## Reglas
- **El CUIT no puede repetirse** entre empresas (*"Ya existe una empresa con ese CUIT."*) y debe ser un número válido. La dirección de ingreso `/Login/<CUIT>` depende de que sea único. Una empresa **inactiva** no tiene dirección de ingreso por CUIT.
- El email, si se carga, debe ser válido.
- Al **crear** una empresa el sistema también: le asigna su número de empresa, copia los **parámetros** desde la plantilla, crea una **sucursal por defecto** (*"Suc.<razón social>"*), y crea los productos *Ajuste de Fórmula* y *Código Genérico*.
- Valores por defecto: nombre de fantasía = razón social, país = Argentina; *tenant slug* y *base path* se derivan de la razón social si quedan vacíos.
- La razón social y el CUIT **solo se cambian desde acá**: el administrador de la empresa no puede editarlos en *Mi Empresa*.
