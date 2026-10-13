---
pantalla: SystemAdministration.Empresas
titulo: Empresas (administración del sistema)
rol: superadmin
modulo: Administración de la plataforma
orden: 1
permiso: Super administrador
revisada: 2026-10-12
---
## Para qué sirve
Ver todas las **empresas** de la plataforma y entrar a editarlas o crear una nueva. Está en *Administración del sistema → Empresas*. Solo lo ve el **super administrador** (no alcanza con ser Admin de una empresa); cualquier otro usuario ve *"Acceso denegado"*.

## Qué ves
Listado con **Empresa** (razón social), **Fantasía**, **CUIT**, **IVA**, Teléfono, Email, **Estado** (Activa / Inactiva) y las acciones **Editar** y **Titulares**. El buscador filtra por empresa, fantasía o CUIT.

## Acciones
- **Nueva empresa** → [Editar empresa](ayuda:pantalla/SystemAdministration.EditarEmpresa) en modo alta.
- **Editar** → la misma pantalla con los datos de esa empresa.
- **Titulares** → [Titulares de facturación](ayuda:pantalla/TitularesFiscales.Index) de esa empresa: acá (y solo acá) el super administrador da de alta y edita los titulares fiscales; el administrador de la empresa solo los habilita o deshabilita.
- Para crear la empresa **con su primera sucursal y su administrador en un solo paso**, usá [Alta rápida de empresa](ayuda:pantalla/SystemAdministration.AltaRapidaEmpresa).

Ver también: [mapa de configuraciones de la plataforma](ayuda:referencia/configuraciones-plataforma)

## Titulares y CUIT compartido (2026-10-12)
El botón **Titulares** de cada empresa abre el listado de titulares de facturación de esa empresa para dar de alta, editar, **cancelar** o **reponer** titulares. Un CUIT puede estar como titular en varias empresas (mismo certificado): ver [titular compartido entre empresas](ayuda:concepto/titular-compartido-entre-empresas).
