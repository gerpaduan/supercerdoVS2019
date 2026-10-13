---
pantalla: TitularesFiscales.Index
titulo: Titulares de facturación
rol: admin
modulo: Facturación
orden: 5
alias: TitularesFiscales.Editar
permiso: Admin de la empresa (listado y habilitar); alta y edicion solo superadmin
revisada: 2026-10-12
---
## Para qué sirve
Administrar **quiénes pueden figurar como emisores de las facturas electrónicas** de la empresa. Cada titular es una persona (por ejemplo un monotributista) con su propio CUIT y su propio certificado de ARCA. Las facturas salen a nombre de la empresa pero con los datos fiscales (CUIT, razón social, condición IVA, ingresos brutos, inicio de actividades) del titular que emite. Está en *Configuración → Titulares de facturación* (solo en la versión con base Postgres). **El alta y la edición de titulares los hace solo el administrador del sistema** (superadmin) desde *Administración del sistema → Empresas → Titulares* de la empresa; el **administrador de la empresa** ve el listado, carga el certificado de cada titular, lo asigna a sus sucursales y puede **habilitarlo o deshabilitarlo**.

El titular **principal** es la propia empresa (su CUIT es el de *Mi Empresa*); se crea solo y no se puede quitar.

## Cómo se usa
1. **Listado**: cada fila muestra el titular, qué letra emite (Resp. Inscripto: A/B; Monotributo: C), el estado y vencimiento de su certificado (en el entorno que usa la empresa), lo facturado en el mes y el año elegidos frente a sus topes, y las sucursales que lo usan. El filtro **Mes / Año** cambia el periodo.
2. **Nuevo titular** (solo administrador del sistema; el botón no aparece para el administrador de la empresa): elegí una **persona** (tiene que estar cargada en Personas con CUIT válido y condición Responsable Inscripto o Monotributo), cargá **inicio de actividades** e **Ingresos Brutos**, los topes si querés, y **Crear titular**. *Validar en ARCA* consulta el padrón por el CUIT y avisa si la condición IVA no coincide (es una ayuda: el alta no depende de ella).
3. **Cargá el certificado** del titular en [Certificado ARCA](ayuda:pantalla/CertificadoArca.Index) (botón *Certificado* de la fila). Sin certificado no se lo puede asignar a una sucursal.
4. **Puntos de venta** (solo administrador del sistema): en la ficha del titular, *Traer de ARCA* (necesita el certificado cargado) o *Agregar a mano* los puntos de venta tipo "web service" que dio de alta en ARCA. Después elegí cuál usa en cada sucursal (grilla *Punto de venta que usa este titular en cada sucursal*).
5. **Asignar a una sucursal**: en *Mis Sucursales → Editar → Titular de facturación*. Toda sucursal nueva nace con el titular principal.
6. **Habilitar / Deshabilitar** (administrador de la empresa o del sistema): un titular no habilitado no se puede asignar ni elegir al facturar. No se puede deshabilitar mientras sea el titular de alguna sucursal, y el titular principal (la empresa) no se deshabilita nunca.

## Qué significan los datos
- **Topes mensual y anual** (los carga el administrador del sistema): son **solo informativos** (no avisan ni bloquean nada). Lo facturado es lo emitido por CarniSys en **producción con CAE**, neto de notas de crédito. El tope anual se mide sobre **los últimos 12 meses** o sobre **el año calendario**, según se elija en cada titular. No reemplaza el control real de ARCA.
- **Razón social y condición IVA** se copian de la persona al crear el titular. Si después cambian en Personas, las facturas nuevas siguen con lo copiado hasta que toques **Actualizar desde la persona**. El **CUIT no se puede cambiar** (es la carpeta de su certificado): hay que dar de baja el titular y crear otro.
- **Titular principal**: solo se editan sus topes; sus datos fiscales son los de la empresa (los cambia el administrador del sistema).

## Cómo se factura con un titular
- Cada venta se factura con el titular de **su sucursal**. En el modal de la factura se puede elegir **otro titular solo para esa factura** (no queda guardado para la próxima).
- El **tipo de comprobante lo define el sistema**: titular monotributista → Factura C (y NC C); titular Responsable Inscripto → A si el cliente es Resp. Inscripto o monotributista, B en otro caso.
- Si el cliente es Responsable Inscripto y el titular de la sucursal no lo es, al tocar *Facturar* el sistema **sugiere** facturar con un titular Responsable Inscripto (si hay uno habilitado). Cerrar el aviso sigue con el titular de la sucursal (Factura C).
- Cada factura guarda los datos del titular que la emitió: reimprimirla o hacerle una nota de crédito usa siempre ese titular y su punto de venta.
- El **domicilio impreso** es la dirección y localidad de la sucursal (si no tiene cargada, el de la empresa).

## Reglas
- Un CUIT es **titular principal de una sola empresa**, pero puede ser **titular agregado en varias** y todas usan el mismo certificado (ver [titular compartido entre empresas](ayuda:concepto/titular-compartido-entre-empresas)). Dentro de una misma empresa no se repite. *"Esta empresa ya tiene a ese CUIT como titular…"*
- Sin punto de venta definido en la sucursal el titular no puede facturar ahí.
- **PENDIENTE verificar con ARCA real** (homologación con dos CUIT): *Traer de ARCA* (qué puntos de venta se consideran activos) y factura A a un monotributista.

Ver también: [certificado ARCA](ayuda:pantalla/CertificadoArca.Index) · [mapa de configuraciones](ayuda:referencia/configuraciones)

## Administrador del sistema sobre otra empresa
- Desde *Administración del sistema → Empresas* el botón **Titulares** abre esta misma pantalla sobre esa empresa (`/TitularesFiscales?idEmpresa=<id>`); un cartel amarillo indica sobre cuál se trabaja. Solo un usuario con `superadmin` puede pedir otra empresa: para un administrador común esa URL da "Acceso denegado".
- Sobre otra empresa **no se ven ni se cargan certificados** (se leen con la sesión de esa empresa; la columna muestra "—") y **"Traer de ARCA" no funciona**: los puntos de venta se cargan a mano. El certificado lo carga el administrador de la empresa en [Certificado ARCA](ayuda:pantalla/CertificadoArca.Index).

## Titular compartido entre empresas (2026-10-12)
- Un titular con la insignia **"CUIT compartido"** tiene el mismo CUIT como titular en otra empresa y **usa el mismo certificado** que ella: **no hace falta cargarlo ni pedirlo**; si lo administra otra empresa, en [Certificado ARCA](ayuda:pantalla/CertificadoArca.Index) solo ves su estado y vencimiento.
- **Topes con CUIT compartido**: cuando el CUIT factura en más de una empresa, cada celda de *Facturado* muestra lo de **esta empresa** y el **"Total del CUIT"** (todas las empresas), y el porcentaje del tope se calcula sobre ese total, con un aviso al lado del tope anual. Lo ven todos los administradores de las empresas con ese CUIT; el **desglose por empresa** (con nombres) lo ve solo el superadmin.
- **Cancelar titularidad** (solo superadmin, titulares que no son el principal): si nunca facturó en la empresa se **elimina** y la empresa ya no lo ve; si ya facturó queda con la insignia **"titularidad cancelada"**, de solo lectura. Sus sucursales vuelven al titular principal y la empresa **no puede emitir facturas ni notas de crédito** con él ni habilitarlo. **Reponer titularidad** (solo superadmin) la quita; queda deshabilitado hasta que la empresa lo habilite.
- Paso a paso, avisos y errores para el superadmin: [titular compartido entre empresas](ayuda:concepto/titular-compartido-entre-empresas).
