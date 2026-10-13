---
titulo: Titular compartido entre empresas (mismo CUIT en varias empresas)
rol: superadmin
modulo: Administración de la plataforma
orden: 20
revisada: 2026-10-12
---
# Titular compartido entre empresas

Un mismo **CUIT** puede ser **titular de facturación en varias empresas**: todas facturan con **el mismo certificado de ARCA** de ese CUIT. Sirve, por ejemplo, para que una persona que es dueña de una empresa también figure como titular en la empresa de un familiar o socio, sin pedir un certificado nuevo por empresa. **Solo el administrador del sistema (superadmin) puede dar de alta y cancelar titulares**; el administrador de cada empresa solo los habilita o deshabilita.

## Qué hay que saber antes de usarlo
- **No hay que generar ni pedir ningún certificado nuevo.** El archivo del certificado (`AFIP/<CUIT>/prod` y `/homo`) no se toca: las demás empresas usan ese mismo archivo. Lo único que se agrega es que la clave del certificado queda registrada **por CUIT** (tabla `certificados_arca_cuit`), de modo que cualquier empresa con ese titular pueda abrirlo.
- Un CUIT es **titular principal de una sola empresa** (el CUIT de la empresa, con el que se entra en `/Login/<CUIT>`). En otras empresas puede estar como **titular agregado**.
- Quien puede entrar al certificado es **toda empresa que tenga a ese CUIT como titular** (para facturar). Quien lo **administra** (pedirlo, instalarlo, renovarlo, descartar un pedido) es **una sola empresa**: aquella donde ese CUIT es el titular principal; si en ninguna lo es, la del titular más antiguo. Las demás lo ven y lo usan, pero no pueden cambiarlo (se les muestra el cartel *"Este certificado lo administra otra empresa"*). El superadmin también puede administrarlo.
- Al renovar el certificado desde la empresa administradora, **las demás empresas lo toman solas** (misma carpeta y misma clave).
- **Riesgo que se acepta**: quien tenga a ese CUIT como titular puede emitir comprobantes a nombre de esa persona. Por eso el alta es solo del superadmin y pide confirmación explícita. Asegurate de que la persona está de acuerdo.

## Paso a paso: asignar un CUIT a otra empresa
1. Entrá a *Administración del sistema → Empresas* y tocá **Titulares** en la empresa a la que querés agregarlo (un cartel amarillo muestra sobre cuál trabajás).
2. **Nuevo titular** → buscá la **persona** (tiene que estar cargada en *Personas de esa empresa*, con CUIT válido y condición Responsable Inscripto o Monotributo) y **tocá el resultado** para elegirla.
3. Si el CUIT **ya es titular en otras empresas**, aparece un aviso amarillo con sus nombres y un casillero **"Confirmo que esta empresa podrá facturar con el certificado de ese CUIT"**. Sin tildarlo no se puede crear (el sistema también lo exige del lado del servidor).
4. Cargá **inicio de actividades**, **Ingresos Brutos** y los **topes** si querés → **Crear titular**.
5. Cargá o traé los **puntos de venta** del titular en esa empresa (*Traer de ARCA* no anda desde otra empresa: cargalos a mano). Si **otra empresa ya usa el mismo punto de venta de ese CUIT**, el sistema lo avisa (ver abajo).
6. El administrador de la empresa lo **habilita** y lo **asigna a sus sucursales** desde *Mis Sucursales*.

## Avisos que vas a ver
- **Topes (suman todas las empresas)**: el límite real es de la **persona**, así que cuando el CUIT figura en más de una empresa el tope se compara con el **total que ese CUIT factura en todas las empresas**. En el listado cada fila muestra *"$ X (esta empresa)"* y *"Total del CUIT: $ Y"*, y el porcentaje del tope se calcula sobre el total. **Todos los administradores de las empresas con ese CUIT ven el total**; solo el **administrador del sistema** ve además el desglose por empresa. Mismo criterio que siempre: solo producción, con CAE, sin errores y neto de notas de crédito; cuentan también las empresas donde la titularidad fue cancelada (la persona facturó igual) y lo facturado antes de existir los titulares por el CUIT principal. Cada empresa carga su propio tope en su fila: conviene cargar el **mismo valor** en todas. Si el CUIT está en una sola empresa, nada cambia. Solo cuenta lo facturado desde CarniSys.
- **Punto de venta repetido**: el número de comprobante es por CUIT + punto de venta + tipo. Si dos empresas facturan a la vez con el mismo punto de venta del mismo CUIT, **compiten por el mismo número** y ARCA rechaza uno. Es un aviso (no bloquea): conviene dar de alta en ARCA un punto de venta distinto por empresa.

## Cancelar la titularidad
En el listado de titulares de la empresa, botón **Cancelar titularidad** (solo superadmin; el principal no se puede cancelar). Pide confirmación y hace esto:
- **Si el titular nunca facturó en esa empresa** (ninguna factura, ni con error ni de prueba): se **elimina** junto con sus puntos de venta y asignaciones, y la empresa **ya no lo ve**. Es lo que corresponde si fue una asignación equivocada. No queda registro visible para la empresa.
- **Si ya facturó**: queda marcado **"titularidad cancelada"**, de solo lectura, porque hay historia que explicar (facturas ya emitidas se siguen viendo e imprimiendo).
- En ambos casos las **sucursales que lo usaban vuelven al titular principal** de la empresa.
- Mientras esté cancelado, la empresa **no puede emitir facturas ni notas de crédito** con ese titular (tampoco para anular facturas viejas) y no puede habilitarlo ni abrir su certificado.
- **Reponer titularidad** (solo si ya había facturado): quita la cancelación; queda **deshabilitado** hasta que la empresa lo habilite.
- Cancelar **no toca** el certificado ni a las otras empresas que usan ese CUIT.

## Qué NO se puede hacer
- **Cambiar el CUIT de la empresa dueña** mientras otras empresas lo usen como titular: el sistema lo impide (*"No se puede cambiar el CUIT de la empresa… lo usan como titular N empresa(s) más"*); primero hay que cancelar esas titularidades.
- Tener el **mismo CUIT dos veces en la misma empresa**, ni como su CUIT y como titular agregado. *"Esta empresa ya tiene a ese CUIT como titular…"*. Si la titularidad estaba cancelada, hay que reponerla en lugar de crearla de nuevo.
- Crear una **empresa nueva** con un CUIT que ya existe como CUIT de otra empresa (eso sigue siendo único). Sí se puede crear con un CUIT que ya es titular agregado en otra.

## Mensajes y qué hacer
| Mensaje | Causa | Qué hacer |
|---|---|---|
| *"Ese CUIT ya es titular en: …; confirmalo para continuar"* | Alta sin tildar la confirmación | Tildá el casillero de confirmación |
| *"El certificado de este CUIT lo administra otra empresa…"* | Una empresa no administradora quiso pedir/instalar/descartar | Hacerlo desde la empresa administradora (o como superadmin) |
| *"La titularidad … fue cancelada por el administrador del sistema…"* | La empresa intentó facturar o anular con un titular cancelado | Reponer la titularidad (superadmin), si corresponde |
| *"No se puede cambiar el CUIT de la empresa…"* | Otras empresas usan ese CUIT como titular | Cancelar esas titularidades primero |
| Aviso de *punto de venta* ya usado por otra empresa | Mismo CUIT y mismo punto de venta en dos empresas | Usar un punto de venta distinto por empresa |

## Dónde mirar
- Pantalla del listado: [Titulares de facturación](ayuda:pantalla/TitularesFiscales.Index).
- Certificado: [Certificado ARCA](ayuda:pantalla/CertificadoArca.Index).
- Cambio de CUIT de empresa: [Editar empresa](ayuda:pantalla/SystemAdministration.EditarEmpresa).
- Detalle técnico: `docs/DECISIONS.md` (2026-10-12) y `docs/06-datos-e-integraciones/afip-y-facturacion.md`.

**Nota de privacidad**: el total le muestra a cada administrador cuánto factura el CUIT en el conjunto de empresas (restando lo propio, cuánto en las otras). Se decidió así porque la persona acepta que se use su CUIT; el desglose con nombres queda solo para el superadmin.

**PENDIENTE verificar con ARCA real** (homologación con dos empresas que comparten un CUIT): que la numeración y el ticket de acceso se comporten bien cuando dos empresas emiten con el mismo CUIT, y la nota de crédito con un cliente con CUIT.
