---
pantalla: SystemAdministration.EditarEmpresa
titulo: Nueva / Modificar empresa
rol: superadmin
modulo: Administración de la plataforma
orden: 2
alias: SystemAdministration.GuardarEmpresa
permiso: Super administrador
revisada: 2026-10-12
---
## Para qué sirve
Crear o modificar una empresa de la plataforma. Se llega desde [Empresas](ayuda:pantalla/SystemAdministration.Empresas).

## Cómo se usa
Completá los datos y tocá **Guardar empresa**. Avisos: *"La empresa se creó correctamente."* / *"La empresa se actualizó correctamente."*

## Campos
- **Obligatorios:** Razón social, **CUIT** y **Condición IVA**. Ojo: el "Cond. IVA" que se imprime en facturas, tickets y recibos **no sale de ese texto** sino de la marca *Empresa RRII* (marcada = "Responsable Inscripto"; sin marcar = "Responsable Monotributo").
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

## CUIT y titulares compartidos (2026-10-12)
- El **CUIT de la empresa** sigue siendo único entre empresas (es el del ingreso `/Login/<CUIT>`). Pero ese CUIT puede ser **titular agregado en otras empresas**, que usan el mismo certificado.
- **No se puede cambiar el CUIT** de una empresa mientras otras empresas lo usen como titular: *"No se puede cambiar el CUIT de la empresa: lo usan como titular N empresa(s) más…"*. Primero hay que cancelar esas titularidades (ver [titular compartido entre empresas](ayuda:concepto/titular-compartido-entre-empresas)).
- Tampoco se puede poner como CUIT de la empresa uno que ella ya tiene como titular agregado: cancelá antes esa titularidad.
- Sí se puede **crear una empresa nueva** con un CUIT que ya es titular agregado en otra: pasa a ser su titular principal y la empresa administradora del certificado.
