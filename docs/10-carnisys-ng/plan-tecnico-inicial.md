# Plan tecnico inicial

## Objetivo

Definir el orden recomendado de construccion de `CarniSys NG`, con entregables concretos, dependencias, validaciones y criterio de cierre por etapa.

## Regla general

No avanzar al siguiente bloque solo porque el anterior compila.

Cada etapa debe cerrar con:

- validacion funcional;
- validacion de permisos;
- coherencia arquitectonica;
- documentacion actualizada.

## Etapa 0. Preparacion de la solucion

### Objetivo

Dejar lista la base tecnica de la solucion `CarniSys.NG.*` dentro de `CarniSys.sln`.

### Entregables

- alta de proyectos `CarniSys.NG.*`
- referencias entre proyectos segun arquitectura definida
- configuracion inicial de compilacion
- base de pruebas unitarias e integracion
- estructura de carpetas por modulo

### No incluye

- logica funcional real
- migracion de modulos legacy

### Criterio de cierre

- solucion compila
- dependencias entre proyectos respetan las reglas definidas
- no existen referencias estructurales al legacy

## Etapa 1. Fundacion del sistema

### Objetivo

Construir los tres pilares base:

- autenticacion
- empresa
- permisos

### Alcance

- login clasico
- resolucion automatica de empresa por usuario
- contexto de usuario autenticado
- contexto de empresa
- sucursal activa en sesion
- motor de permisos con las tres dimensiones acordadas

### Entregables

- modelo de usuario NG
- contexto de empresa y sucursal activa
- middleware o mecanismo equivalente de contexto
- base de autorizacion en web
- estructura de permisos reutilizable por modulos

### Validaciones

- el usuario no puede elegir empresa
- el sistema resuelve empresa correctamente
- los permisos de lectura y edicion respetan rango de dias
- el alcance propio/todos funciona correctamente

### Criterio de cierre

- navegacion autenticada funcionando
- empresa y sucursal resueltas correctamente
- permisos aplicables sin hardcode por pantalla puntual

## Etapa 2. Shell web y experiencia base

### Objetivo

Construir la base operativa de la nueva interfaz web, siguiendo la UX del proyecto MVC actual.

### Alcance

- layout principal
- menu
- selector de sucursal
- mensajes globales
- base responsive
- infraestructura de atajos y foco

### Entregables

- shell principal NG
- menu por permisos
- cambio de sucursal desde menu de usuario
- restriccion de cambio de sucursal si POS esta abierto

### Validaciones

- navegacion fluida con teclado
- cambio de sucursal consistente
- comportamiento previsible y rapido

### Criterio de cierre

- la app ya puede usarse como contenedor real de modulos

## Etapa 3. Primer modulo vertical

### Objetivo

Implementar un flujo funcional chico, real y seguro para validar de punta a punta la arquitectura.

### Modulo recomendado

`Catalogo global y productos`

### Justificacion

- conecta con multitenancy;
- ejercita permisos;
- ejercita RLS;
- obliga a resolver clonacion desde `EmpresaID = 0`;
- tiene menos riesgo operativo que POS;
- deja una base util para modulos posteriores.

### Alcance minimo

- consulta de productos globales y de empresa
- pantalla de catalogo global
- clonacion a empresa
- validacion por codigo duplicado
- alta y modificacion solo sobre productos de empresa
- precio ingresado y validado al importar, respetando MVC actual

### Criterio de cierre

- un usuario puede incorporar correctamente un producto global a su empresa
- no puede duplicar codigo
- no puede operar directamente con producto global

## Etapa 4. Parametros, maestras y soporte

### Objetivo

Completar la base de configuracion necesaria para que otros modulos no nazcan con atajos o hardcode.

### Alcance

- parametros base
- formas de pago
- IVA
- consumidor final
- otras maestras globales o por empresa ya existentes

### Regla

No redefinir arbitrariamente que es global y que no.

Debe respetarse el comportamiento ya validado en el sistema actual.

### Criterio de cierre

- los modulos posteriores pueden apoyarse en maestras reales y no en datos ficticios

## Etapa 5. Personas

### Objetivo

Implementar personas, clientes y proveedores segun la separacion real que exista en el sistema actual.

### Alcance

- busqueda
- alta
- edicion
- permisos
- validaciones operativas

### Justificacion

Este modulo suele ser transversal y de riesgo moderado.

Conviene resolverlo antes de ventas y POS.

## Etapa 6. Stock basico

### Objetivo

Construir una base de stock suficiente para validar movimientos principales sin entrar todavia en toda la complejidad del POS.

### Alcance

- consulta de stock
- stock por sucursal
- reglas minimas de disponibilidad
- integracion con productos y empresa
- base de `punto stock` por sucursal

### Requisitos adicionales

- crear estructura nueva de punto stock por sucursal;
- asumir `0` cuando no exista configuracion;
- no controlar faltante cuando el punto sea `0`;
- marcar faltante cuando `stock actual <= punto stock`.

### Regla

No inventar simplificaciones que rompan la logica actual de sucursal, movimientos o disponibilidad.

### Extension recomendada

Una vez establecida la base, agregar:

- edicion por sucursal en ficha de producto;
- edicion masiva por lote;
- visualizacion con diferencia positiva o negativa.

## Etapa 7. POS y Punto de Expendio

### Objetivo

Construir el primer POS NG tomando como referencia principal el proyecto MVC actual.

### Requisitos previos

- autenticacion lista
- empresa lista
- permisos listos
- sucursal activa lista
- caja abierta validable
- productos y personas suficientemente resueltos

### Alcance inicial

- apertura de POS
- aviso de sucursal actual
- opcion de cambio previa
- validacion de caja abierta
- secuencia de carga de venta
- atajos y foco
- impresion segun flujo actual

### Regla

No alterar el comportamiento actual sin validacion previa.

### Criterio de cierre

- POS NG puede compararse de forma seria contra POS MVC actual

## Etapa 8. AFIP e impresion completa

### Objetivo

Integrar facturacion electronica, notas de credito e impresion completa segun comportamiento actual.

### Alcance

- integracion AFIP encapsulada
- modal de impresion
- termica
- PDF
- mail

### Regla

Tomar como referencia principal el flujo web MVC actual.

## Etapa 9. Dispositivos

### Objetivo

Resolver lectura de balanza y perifericos sin comprometer multiplataforma ni operacion real.

### Alcance

- decision final sobre agente local
- integracion con balanza
- soporte lector fisico
- soporte camara mobile

### Regla

Si no aparece una opcion claramente superior, se mantiene agente local separado.

## Etapa 10. Validacion de reemplazo por modulo

### Objetivo

Determinar modulo por modulo si NG ya puede reemplazar la parte legacy.

### Checklist de reemplazo

- cubre el flujo actual completo
- respeta permisos
- respeta impresion e integraciones
- valida resultados contra sistema actual
- fue probado por uso real

## Orden resumido recomendado

1. Proyectos base
2. Autenticacion, empresa y permisos
3. Shell web y experiencia base
4. Catalogo global y productos
5. Parametros y maestras
6. Personas
7. Stock basico
8. POS y Punto de Expendio
9. AFIP e impresion completa
10. Dispositivos y validacion de reemplazo

## Notas finales

- POS debe diseñarse desde el principio, aunque no sea el primer modulo implementado.
- La arquitectura debe validarse con un flujo real cuanto antes.
- El primer flujo real recomendado sigue siendo `catalogo global y productos`.
