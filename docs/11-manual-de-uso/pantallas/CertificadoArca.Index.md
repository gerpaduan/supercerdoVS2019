---
pantalla: CertificadoArca.Index
titulo: Certificado ARCA
rol: admin
modulo: Facturación
orden: 4
permiso: Admin de la empresa
revisada: 2026-10-12
---
## Para qué sirve
Cargar, renovar y probar el **certificado digital de ARCA (ex AFIP)** con el que se emiten las facturas electrónicas, y ver cuándo vence. Está en *Configuración → Certificado ARCA* (solo administradores y solo con base Postgres). Hay **un certificado por entorno** (Producción y Pruebas/homologación) y **por titular**: cargar uno nunca toca los otros.

## Cómo se usa
1. **Titular**: si la empresa tiene más de un [titular de facturación](ayuda:pantalla/TitularesFiscales.Index), elegilo arriba (por defecto el principal = la empresa). Todo lo de abajo se aplica al titular elegido y a su CUIT.
2. Elegí la pestaña del entorno (la que dice *en uso* es con la que la empresa factura hoy; el entorno lo cambia el administrador del sistema y vale para todos los titulares).
3. **Paso 1 – Generar el pedido (.csr)**: poné un alias (solo letras y números) y descargá el pedido. La clave privada queda guardada en el servidor.
4. En ARCA (*Administración de Certificados Digitales*): subí el .csr, bajá el .crt y autorizá el servicio de facturación (wsfe) para ese alias. El tutorial de la pantalla lo explica paso a paso.
5. **Paso 2 – Subir el certificado (.crt)**: el sistema verifica que sea del último pedido y del CUIT del titular, arma el .pfx y lo instala. Si ya había uno en ese entorno queda una copia de respaldo.
6. **Probar conexión**: comprueba (sin emitir nada) que el certificado abre, que ARCA lo acepta y que el punto de venta está habilitado.

## Qué ves
- **Estado**: Vigente, Por vencer, Vencido, No se puede leer o Sin certificado, con la fecha de vencimiento leída del certificado real y la carpeta del servidor (`AFIP/<CUIT del titular>/prod` u `homo`).
- Te avisamos en la campana de notificaciones cuando falten pocos días y cuando venza; el aviso indica **de qué titular** es.

## Reglas
- Si el entorno en uso no tiene certificado cargado, **no se puede facturar** con ese titular hasta cargarlo.
- Un titular sin certificado no se puede asignar a una sucursal.
- **Certificado compartido (2026-10-12)**: si el CUIT del titular también es titular en otras empresas, **todas usan el mismo certificado**. Solo **una empresa lo administra** (pedir, instalar, renovar, descartar un pedido): aquella donde ese CUIT es el titular principal (si en ninguna lo es, la del titular más antiguo) y el superadmin. En las demás se ve el estado y el vencimiento con un cartel *"Este certificado lo administra otra empresa"*, y al renovar desde la administradora **las demás lo toman solas**. A la empresa administradora se le avisa en la propia pantalla que otras empresas lo comparten. Un titular con la titularidad cancelada no se puede abrir acá. Ver [titular compartido entre empresas](ayuda:concepto/titular-compartido-entre-empresas).
- Las claves de los certificados se guardan cifradas; si se pierden las claves de cifrado del servidor, el certificado hay que cargarlo de nuevo.

Ver también: [titulares de facturación](ayuda:pantalla/TitularesFiscales.Index) · [mapa de configuraciones](ayuda:referencia/configuraciones)
