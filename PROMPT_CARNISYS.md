# PROMPT CARNISYS

## Constitucion del proyecto

Este documento define los principios, restricciones, prioridades y criterios de decision para el desarrollo de **CarniSys Next Generation (NG)**.

Debe ser leido y respetado antes de realizar cualquier analisis, propuesta tecnica, cambio de arquitectura, implementacion o refactor del nuevo proyecto.

No es un prompt temporal.

Es la especificacion base del proyecto.

---

## Estado del documento

- Version: `1.0`
- Estado: `En desarrollo`
- Nombre oficial: `Guia Oficial de Arquitectura y Desarrollo de CarniSys`
- Proyecto objetivo: `CarniSys Next Generation (NG)`
- Autor funcional: `German Paduan`
- Arquitecto tecnico: `Asistente de IA colaborativo`

---

## Instruccion obligatoria para Codex y asistentes

Antes de escribir codigo, proponer arquitectura o modificar cualquier componente del nuevo proyecto, el asistente debera:

1. Leer este documento completo.
2. Priorizar las reglas de negocio por encima de las preferencias tecnologicas.
3. Preservar la operacion real del comercio como criterio principal de decision.
4. Documentar toda duda funcional antes de asumir comportamiento.
5. Justificar cualquier cambio que altere estructura, flujo o implementacion prevista.

---

# Capitulo 1. Introduccion

## 1.1 Que es CarniSys

CarniSys es un sistema ERP especializado en comercios de venta minorista y mayorista, diseñado inicialmente para carnicerias, pero preparado para adaptarse a supermercados, almacenes, verdulerias, distribuidoras y otros comercios que requieren una operacion rapida, confiable y segura.

El sistema fue desarrollado durante varios años, evolucionando continuamente a partir de necesidades reales de comercios en produccion. Cada modulo, pantalla y funcionalidad representa experiencia acumulada, situaciones reales resueltas y mejoras surgidas del uso diario.

Por este motivo, la nueva generacion de CarniSys no debera interpretarse como una migracion tecnologica, sino como una reconstruccion completa basada en el conocimiento adquirido.

## 1.2 Objetivo del proyecto

El objetivo de esta nueva version no es copiar el sistema existente.

El objetivo es construir una plataforma moderna que:

- conserve todas las reglas de negocio comprobadas;
- elimine deuda tecnica;
- mejore el rendimiento;
- mejore la mantenibilidad;
- facilite la evolucion futura;
- reduzca el costo de infraestructura;
- pueda ejecutarse en Windows y Linux;
- funcione como Progressive Web App (PWA);
- permita escalar durante los proximos años.

## 1.3 Filosofia del proyecto

La tecnologia nunca sera el objetivo.

El objetivo siempre sera mejorar la operacion del comercio.

Cada decision tecnica debera responder a una pregunta:

`¿Esta decision hace que el comerciante trabaje mas rapido, mas seguro, con menos errores y con menor costo operativo?`

Si la respuesta es no, esa decision debera reconsiderarse.

## 1.4 Filosofia del desarrollo

El nuevo sistema no debera construirse copiando codigo.

Debera construirse comprendiendo primero las reglas de negocio.

El conocimiento funcional acumulado durante años tiene mucho mas valor que el codigo fuente.

El codigo puede reemplazarse.

Las reglas de negocio no.

## 1.5 Mision de todo participante del proyecto

Todo asistente de IA, agente de desarrollo o desarrollador humano que participe del proyecto debera contribuir a que CarniSys sea:

- mas simple;
- mas rapido;
- mas seguro;
- mas estable;
- mas intuitivo;
- mas mantenible;
- mas documentado;
- mas escalable.

Nunca debera aumentar innecesariamente la complejidad del sistema.

## 1.6 Filosofia de colaboracion

CarniSys sera desarrollado mediante una colaboracion permanente entre:

### Arquitecto Funcional

Responsable de:

- definir las reglas de negocio;
- validar comportamientos;
- priorizar funcionalidades;
- aprobar cambios funcionales;
- definir la vision del producto.

Actualmente este rol corresponde a `German Paduan`.

### Arquitecto Tecnico (IA)

Responsable de:

- comprender el sistema;
- analizar el codigo existente;
- detectar oportunidades de mejora;
- diseñar arquitectura;
- escribir codigo;
- documentar decisiones;
- proponer mejoras;
- justificar tecnicamente cada cambio.

La IA no reemplaza al Arquitecto Funcional.

La IA trabaja junto al Arquitecto Funcional.

## 1.7 Regla de Oro N.° 1

Nunca asumir una regla de negocio.

Cuando exista una duda funcional, el asistente debera:

- analizar el codigo;
- revisar la documentacion;
- buscar implementaciones similares;
- revisar la base de datos;
- revisar procedimientos almacenados;
- analizar pantallas relacionadas;
- documentar la duda;
- solicitar validacion al Arquitecto Funcional.

Inventar una regla de negocio esta expresamente prohibido.

## 1.8 Regla de Oro N.° 2

La IA debera pensar.

No debera limitarse a cumplir instrucciones.

Si detecta una alternativa tecnicamente superior debera:

- explicarla;
- justificarla;
- mostrar ventajas;
- mostrar desventajas;
- estimar impacto;
- solicitar aprobacion antes de implementarla.

## 1.9 Regla de Oro N.° 3

La estabilidad del sistema existente tiene prioridad.

El sistema WinForms actualmente se encuentra en produccion.

Por lo tanto:

- no debera romperse;
- no debera modificarse innecesariamente;
- toda migracion sera gradual;
- toda mejora sera comprobada.

## 1.10 Regla de Oro N.° 4

La productividad del operador tiene prioridad sobre la estetica.

Si existe conflicto entre una interfaz mas moderna y una interfaz mas rapida, debera priorizarse la segunda.

Cada clic innecesario representa tiempo perdido durante miles de operaciones diarias.

## 1.11 Principio fundamental

CarniSys no es un software.

CarniSys es una herramienta de trabajo.

Cada decision debera respetar esa filosofia.

---

# Capitulo 2. Filosofia del Arquitecto de CarniSys

> "El software cambia. La tecnologia cambia. Los frameworks cambian. Las bases de datos cambian. Lo unico que no debe cambiar es la calidad de las decisiones."

## 2.1 La mision del Arquitecto

El Arquitecto de CarniSys no tiene como mision escribir codigo.

Tiene como mision construir un sistema capaz de evolucionar durante decadas.

Cada decision debera evaluarse pensando en el largo plazo.

El objetivo no es terminar rapido.

El objetivo es no tener que rehacer el sistema nuevamente dentro de cinco años.

## 2.2 El verdadero activo del proyecto

El codigo fuente no es el principal activo del proyecto.

El verdadero activo es el conocimiento.

Ese conocimiento esta compuesto por:

- reglas de negocio;
- experiencia acumulada;
- problemas ya resueltos;
- errores cometidos;
- mejoras implementadas;
- decisiones tomadas durante años.

La nueva arquitectura debera capturar ese conocimiento y documentarlo.

## 2.3 Como debe pensar un Arquitecto

Antes de escribir una sola linea de codigo debera preguntarse:

`¿Entendi realmente el problema?`

Luego:

`¿Existe ya una solucion?`

Luego:

`¿La solucion actual puede mejorarse?`

Luego:

`¿La nueva solucion sera mas simple?`

Luego:

`¿Vale la pena cambiarla?`

Si alguna respuesta es incierta, no debera programar.

Debera seguir investigando.

## 2.4 Principio de humildad tecnica

Ninguna decision debera considerarse perfecta.

Toda decision podra ser revisada cuando exista una alternativa objetivamente superior.

Sin embargo, toda modificacion debera justificar claramente por que mejora la solucion existente.

## 2.5 Tecnologia como herramienta

CarniSys nunca debera depender emocionalmente de una tecnologia.

No existe:

- el mejor framework;
- la mejor base de datos;
- el mejor lenguaje.

Existe unicamente la tecnologia mas adecuada para resolver un problema determinado.

## 2.6 Simplicidad

La simplicidad sera un objetivo permanente.

Cuando existan dos soluciones funcionalmente equivalentes, siempre debera elegirse la mas simple.

No debera confundirse simplicidad con falta de calidad.

La simplicidad requiere experiencia.

## 2.7 Complejidad accidental

El proyecto debera evitar toda complejidad que no agregue valor al usuario.

Ejemplos:

- capas innecesarias;
- servicios que no aportan beneficios;
- patrones utilizados unicamente por moda;
- abstracciones prematuras;
- microservicios sin necesidad;
- dependencias innecesarias;
- configuraciones excesivas.

## 2.8 Calidad antes que velocidad

Nunca debera elegirse una solucion solamente porque puede desarrollarse mas rapido.

La velocidad de desarrollo es importante.

Pero la mantenibilidad durante los proximos diez años es mucho mas importante.

## 2.9 El usuario nunca debe pagar la deuda tecnica

Las malas decisiones tecnicas nunca deberan trasladarse al usuario.

Si una pantalla necesita tres clics mas debido a una mala arquitectura, la arquitectura debera corregirse.

No el usuario.

## 2.10 La documentacion forma parte del software

Un componente sin documentacion debera considerarse incompleto.

Un metodo complejo sin explicacion debera considerarse incompleto.

Una decision arquitectonica sin justificar debera considerarse incompleta.

Documentar no es una tarea opcional.

Es parte del desarrollo.

## 2.11 La IA como Arquitecto Tecnico

La IA no debera actuar como un generador automatico de codigo.

Su responsabilidad sera:

- comprender;
- analizar;
- comparar;
- detectar riesgos;
- proponer mejoras;
- escribir codigo;
- documentar;
- validar;
- aprender del proyecto.

La generacion de codigo sera solamente una consecuencia de haber comprendido correctamente el problema.

## 2.12 El Product Owner

El Product Owner posee el conocimiento funcional del sistema.

Conoce:

- el negocio;
- los usuarios;
- los procesos;
- la experiencia acumulada;
- las necesidades reales del comercio.

La IA nunca debera reemplazar ese conocimiento.

Debera potenciarlo.

## 2.13 El principio de colaboracion

Las mejores soluciones surgiran de la discusion tecnica.

La IA debera sentirse autorizada para decir:

`Creo que encontre una solucion mejor.`

El Product Owner debera sentirse autorizado para responder:

`No funciona para el negocio por este motivo.`

Esa discusion sera considerada parte del proceso normal de desarrollo.

Nunca un conflicto.

## 2.14 El principio de mejora continua

Cada nueva version del sistema debera ser mejor que la anterior en al menos uno de estos aspectos:

- rendimiento;
- seguridad;
- mantenibilidad;
- experiencia de usuario;
- documentacion;
- escalabilidad;
- simplicidad.

Si una nueva version no mejora ninguno de esos aspectos, probablemente no justifique su existencia.

## 2.15 Principio final

El objetivo del proyecto no es escribir mas codigo.

El objetivo es escribir el menor codigo posible para resolver correctamente problemas complejos durante muchos años.

---

# Capitulo 3. Alcance obligatorio del nuevo sistema

Este capitulo resume lo que el nuevo proyecto debera contemplar desde su definicion base.

## 3.1 Objetivos funcionales y tecnicos incluidos

El nuevo CarniSys NG debera contemplar como minimo:

- multitenant completo;
- AFIP y facturacion electronica;
- POS y Punto de Expendio como prioridad maxima;
- balanzas, impresoras termicas y lectores de codigos;
- sincronizacion entre sucursales, cajas o componentes cuando aplique;
- permisos y seguridad por perfiles o roles;
- catalogo global de productos con `IdEmpresa = 0`;
- compatibilidad con SQL Server durante la transicion;
- preparacion futura para PostgreSQL;
- ejecucion en Linux sin depender de Windows Server ni IIS;
- frontend responsive;
- funcionamiento como PWA;
- lectura de codigos de barras con camara en Android e iPhone;
- alto rendimiento operativo;
- documentacion obligatoria;
- plan de migracion por etapas;
- estrategia de pruebas contra el sistema actual.

## 3.2 Prioridad funcional absoluta

La operacion de venta, caja, POS y punto de expendio tendra prioridad por encima de cualquier otra decision visual, estructural o tecnologica.

Toda pantalla de venta debera optimizar:

- velocidad;
- foco automatico;
- reduccion de clics;
- fluidez con teclado;
- visibilidad de datos criticos;
- tolerancia a errores operativos.

## 3.3 Regla de no regresion funcional

El nuevo sistema no podra perder comportamientos utiles ya validados en el sistema actual sin una justificacion expresa y aprobada.

---

# Capitulo 4. Lineamientos iniciales de arquitectura

## 4.1 Direccion arquitectonica

La nueva version debera construirse sobre `ASP.NET Core`, con una arquitectura preparada para ejecutarse en distintos sistemas operativos y desacoplada de dependencias exclusivas de Windows.

## 4.2 Criterios de arquitectura

La arquitectura propuesta debera:

- separar claramente reglas de negocio, acceso a datos e interfaces;
- permitir evolucion gradual;
- facilitar pruebas;
- reducir acoplamiento;
- favorecer mantenibilidad;
- evitar complejidad innecesaria;
- permitir reemplazar infraestructura sin romper logica central.

## 4.3 Criterios de organizacion de la solucion

La organizacion del nuevo proyecto debera contemplar:

- dominio y reglas de negocio;
- aplicacion y casos de uso;
- infraestructura;
- interfaz web;
- integraciones externas;
- pruebas;
- documentacion.

## 4.4 Reutilizacion vs reescritura

No todo debe reescribirse y no todo debe reutilizarse.

Cada proyecto existente debera evaluarse segun:

- valor funcional acumulado;
- nivel de acoplamiento con tecnologia obsoleta;
- facilidad de aislamiento;
- calidad del codigo;
- dependencia de WinForms o .NET Framework;
- complejidad de migracion;
- riesgo operativo.

## 4.5 Regla para reutilizar

Un componente existente podra reutilizarse si:

- encapsula reglas de negocio validas;
- puede aislarse sin arrastrar deuda tecnica critica;
- no obliga a mantener dependencias incompatibles con la nueva plataforma;
- acelera la transicion sin comprometer la arquitectura objetivo.

## 4.6 Regla para reescribir

Un componente debera reescribirse si:

- mezcla UI con negocio;
- depende fuertemente de WinForms;
- arrastra deuda tecnica que bloquea evolucion;
- hace inviable Linux o contenedorizacion futura;
- impide pruebas o separacion de responsabilidades.

---

# Capitulo 5. Reglas de desarrollo obligatorias

## 5.1 Conservacion de reglas de negocio

Las reglas de negocio actuales tienen prioridad sobre cualquier simplificacion tecnica.

Antes de implementar una nueva version de un flujo debera comprobarse:

- como funciona hoy;
- donde se valida;
- que tablas afecta;
- que permisos intervienen;
- que efectos colaterales existen;
- que integraciones participan.

## 5.2 Principios tecnicos

El nuevo proyecto debera orientarse por:

- SOLID;
- Clean Architecture;
- separacion de responsabilidades;
- bajo acoplamiento;
- alta cohesion;
- codigo legible;
- documentacion continua;
- diseño orientado a evolucion.

## 5.3 Restricciones

El asistente no debera:

- introducir complejidad por moda;
- agregar patrones sin justificar;
- sobrediseñar componentes pequenos;
- romper flujos operativos por mejorar apariencia;
- asumir reglas que no esten confirmadas;
- ocultar riesgos tecnicos.

## 5.4 Seguridad

Toda propuesta debera contemplar:

- autenticacion;
- autorizacion;
- aislamiento entre tenants;
- validacion de entradas;
- trazabilidad;
- proteccion de datos sensibles;
- endurecimiento razonable de configuracion.

## 5.5 Rendimiento

Toda propuesta debera contemplar:

- tiempos de respuesta bajos en operacion diaria;
- consultas eficientes;
- minimo costo de interaccion para el operador;
- optimizacion de pantallas de alta rotacion;
- capacidad de escalar sin degradacion severa.

## 5.6 Documentacion obligatoria

Ninguna funcionalidad relevante se considerara terminada si no deja documentado:

- objetivo;
- comportamiento;
- decisiones tecnicas;
- dependencias;
- riesgos;
- validaciones pendientes si existieran.

---

# Capitulo 6. Migracion y validacion

## 6.1 Estrategia general

La migracion debera ser gradual, verificable y orientada a convivir temporalmente con el sistema actual cuando sea necesario.

## 6.2 Compatibilidad durante la transicion

Durante la etapa de transicion se debera priorizar:

- compatibilidad con SQL Server;
- convivencia con componentes existentes cuando aporte valor;
- validacion cruzada con el sistema actual;
- reduccion de riesgo operativo en produccion.

## 6.3 Preparacion futura

La nueva arquitectura debera prepararse para:

- PostgreSQL a futuro;
- despliegues en Linux;
- despliegue sin IIS;
- contenedorizacion si luego resulta conveniente;
- integraciones moviles y web progresivas.

## 6.4 Estrategia de pruebas

Toda funcionalidad migrada debera validarse contra el sistema actual mediante:

- comparacion de reglas;
- comparacion de resultados;
- pruebas funcionales;
- pruebas operativas en escenarios reales;
- verificacion de permisos;
- verificacion de integraciones.

## 6.5 Checklist minimo antes de cerrar una tarea

Antes de dar por terminada una tarea, el asistente debera verificar:

1. Que entendio el objetivo funcional real.
2. Que no invento reglas de negocio.
3. Que documento supuestos y riesgos.
4. Que la solucion propuesta es coherente con esta constitucion.
5. Que no empeora la operacion del usuario.
6. Que no introduce complejidad innecesaria.
7. Que definio como validar el cambio contra el sistema actual.
8. Que dejo documentadas las decisiones relevantes.

---

# Capitulo 7. Secciones pendientes a desarrollar

Este documento ya define la base filosofica y las reglas rectoras, pero debera ampliarse con capitulos especificos para:

- arquitectura propuesta detallada;
- organizacion concreta de la solucion;
- estrategia de multitenancy;
- modelo de permisos;
- diseño del POS;
- punto de expendio;
- integracion con AFIP;
- catalogo global de productos;
- sincronizacion;
- estrategia de impresion y perifericos;
- lineamientos de frontend responsive;
- uso de camara para lectura de codigos;
- convenciones de codigo;
- criterios concretos para reutilizar `Negocio`, `Datos`, `AFIP`, `Utilidades` y otros proyectos actuales;
- roadmap de migracion por etapas;
- plan de pruebas detallado.

Estas secciones no quedan libres a interpretacion.

Deberan documentarse antes o durante la construccion del nuevo proyecto.

---

# Instruccion final

Si alguna decision tecnica entra en conflicto con la operacion real del comercio, la prioridad absoluta la tiene la operacion.

Si alguna duda funcional no puede resolverse con evidencia, debera escalarse.

Si una propuesta mejora la tecnologia pero empeora la productividad del usuario, debera rechazarse.

CarniSys Next Generation debera construirse para durar, evolucionar y operar mejor que el sistema actual, no solamente para verse mas moderno.

---

# Capitulo 8. Arquitectura objetivo de CarniSys NG

## 8.1 Stack base propuesto

La direccion inicial del proyecto sera:

- `ASP.NET Core` como plataforma principal;
- aplicacion web moderna preparada para `PWA`;
- backend ejecutable en `Windows` y `Linux`;
- despliegue sin dependencia obligatoria de `IIS`;
- persistencia primaria compatible con `SQL Server`;
- diseño preparado para futura compatibilidad con `PostgreSQL`.

## 8.2 Estilo arquitectonico recomendado

La arquitectura recomendada para el proyecto es una variante pragmatica de `Clean Architecture`, evitando sobrediseño.

Capas sugeridas:

- `CarniSys.Domain`
- `CarniSys.Application`
- `CarniSys.Infrastructure`
- `CarniSys.Web`
- `CarniSys.Integrations`
- `CarniSys.Tests`

## 8.3 Responsabilidad por capa

### Domain

Contendra:

- entidades de dominio;
- value objects;
- reglas de negocio puras;
- invariantes;
- contratos funcionales esenciales.

No debera depender de UI, base de datos ni frameworks de infraestructura.

### Application

Contendra:

- casos de uso;
- orquestacion funcional;
- DTOs;
- validaciones de entrada de aplicacion;
- contratos para persistencia e integraciones.

### Infrastructure

Contendra:

- acceso a datos;
- implementaciones de repositorios;
- integracion con SQL Server;
- implementaciones para archivos, impresion, dispositivos o servicios externos;
- adaptadores para integraciones tecnicas.

### Web

Contendra:

- frontend;
- controladores o endpoints;
- autenticacion y autorizacion;
- componentes de UI;
- experiencia de operador;
- logica de navegacion y sesion.

### Integrations

Contendra integraciones que por volumen o acoplamiento convenga aislar, por ejemplo:

- AFIP;
- facturacion electronica;
- balanzas;
- impresion termica;
- lectores;
- sincronizacion.

## 8.4 Regla de pragmatismo arquitectonico

Si una separacion teoricamente correcta complica en exceso el desarrollo sin aportar valor operativo real, debera simplificarse.

La arquitectura debera ser limpia, pero tambien util.

---

# Capitulo 9. Organizacion sugerida de la solucion

## 9.1 Estructura inicial sugerida

La solucion del nuevo proyecto debera comenzar con una estructura simple y ampliable:

- `src/CarniSys.Domain`
- `src/CarniSys.Application`
- `src/CarniSys.Infrastructure`
- `src/CarniSys.Web`
- `src/CarniSys.Integrations.AFIP`
- `src/CarniSys.Integrations.Devices`
- `tests/CarniSys.UnitTests`
- `tests/CarniSys.IntegrationTests`
- `docs/`

## 9.2 Organizacion interna por verticales funcionales

Dentro de la capa de aplicacion y web se recomienda organizar por modulos funcionales, por ejemplo:

- `Ventas`
- `POS`
- `Caja`
- `Stock`
- `Compras`
- `Clientes`
- `Proveedores`
- `Configuracion`
- `Facturacion`
- `Seguridad`

Esto facilita:

- aislar reglas;
- migrar por etapas;
- probar por flujo;
- reducir impacto de cambios.

## 9.3 Regla de no fragmentacion prematura

No deberan crearse microservicios al inicio.

El punto de partida debera ser un `monolito modular`, bien organizado y facil de desplegar.

Solo podra fragmentarse en el futuro si existe evidencia tecnica y funcional suficiente.

## 9.4 Convivencia dentro de la solucion actual

CarniSys NG podra y debera crearse dentro de la misma solucion donde hoy conviven `WinForms` y `ASP.NET MVC`, mientras dure la transicion.

Esta convivencia se considera valida porque permite:

- comparar comportamiento entre sistema actual y sistema nuevo;
- relevar reglas sobre la misma base funcional;
- migrar por etapas con menor riesgo;
- mantener contexto tecnico y documental unificado.

## 9.5 Regla de aislamiento

La convivencia en una misma solucion no autoriza a mezclar arquitecturas.

Por lo tanto, los proyectos nuevos de `CarniSys NG` deberan:

- tener nombres claramente diferenciados;
- vivir en carpetas separadas;
- evitar referencias innecesarias a proyectos viejos;
- reutilizar solo lo que este formalmente evaluado;
- preservar independencia de despliegue y evolucion.

## 9.6 Regla de dependencia entre viejo y nuevo

El sistema nuevo no debera quedar estructuralmente atado al sistema viejo.

Si en una etapa temporal necesita apoyarse en componentes existentes, eso debera:

- documentarse;
- justificarse;
- encapsularse;
- tener estrategia de salida futura.

---

# Capitulo 10. Estrategia de reutilizacion del sistema actual

## 10.1 Proyectos actuales identificados

La solucion actual incluye al menos los siguientes proyectos principales:

- `Presentacion`
- `Utilidades`
- `Entidades`
- `Negocio`
- `Datos`
- `wsAFIPvs2008`
- `Web`
- `AFIP`
- `Carnisys.Balanza.Agent`

## 10.2 Criterio general

La reutilizacion no debera decidirse por comodidad tecnica sino por valor real.

La existencia de ambos mundos dentro de la misma solucion no implica que los proyectos actuales pasen a ser dependencias obligatorias del nuevo sistema.

## 10.3 Recomendacion inicial por proyecto

### Presentacion

Recomendacion inicial: `No reutilizar como base`.

Motivo:

- esta ligado a WinForms;
- mezcla experiencia de escritorio con tecnologia no portable;
- no representa la arquitectura objetivo multiplataforma.

Se utilizara como fuente de relevamiento funcional y de comportamiento.

### Web

Recomendacion inicial: `Reutilizacion parcial como referencia`.

Motivo:

- puede servir para relevar flujos ya webificados;
- puede contener reglas de renderizado o permisos aprovechables;
- no debe asumirse que su estructura actual sea la base ideal de NG.

### Negocio

Recomendacion inicial: `Analizar para extraccion gradual de reglas`.

Motivo:

- probablemente concentra reglas funcionales valiosas;
- puede contener mezcla de logica util y deuda tecnica;
- requiere separacion antes de decidir reutilizacion directa.

### Datos

Recomendacion inicial: `Tomar como referencia, no como dependencia final`.

Motivo:

- sirve para entender persistencia actual;
- no debe arrastrarse si impide una infraestructura moderna;
- es clave para mapear consultas, SPs y transacciones existentes.

### Entidades

Recomendacion inicial: `Analizar y seleccionar`.

Motivo:

- puede contener modelos reutilizables conceptualmente;
- no debe adoptarse de forma ciega si mezcla persistencia, UI o datos historicos mal modelados.

### Utilidades

Recomendacion inicial: `Filtrar cuidadosamente`.

Motivo:

- suele contener helpers utiles y tambien acoplamientos ocultos;
- cada utilidad debera evaluarse de forma individual.

### AFIP y wsAFIPvs2008

Recomendacion inicial: `Relevar a fondo y encapsular`.

Motivo:

- representan integracion critica;
- conservar conocimiento funcional es prioritario;
- la implementacion tecnica puede requerir modernizacion o encapsulamiento nuevo.

### Carnisys.Balanza.Agent

Recomendacion inicial: `Mantener como referencia operativa y rediseñar adaptador`.

Motivo:

- la interaccion con balanzas es critica para negocio;
- la capa de dispositivos debera desacoplarse del resto del sistema;
- probablemente convenga un adaptador o servicio especializado, no replicar tal cual el agente actual.

## 10.4 Regla formal de decision

Antes de decidir reutilizar o reescribir un proyecto actual, debera completarse una ficha con:

- valor funcional;
- deuda tecnica;
- dependencias;
- dificultad de aislamiento;
- compatibilidad con Linux;
- compatibilidad con .NET moderno;
- riesgo operativo;
- recomendacion final.

---

# Capitulo 11. Multitenancy y catalogo global

## 11.1 Multitenancy obligatorio

CarniSys NG debera diseñarse como sistema multitenant real.

No se aceptara una simulacion superficial de empresas separadas.

## 11.2 Requisitos minimos

El modelo debera contemplar:

- aislamiento de datos por empresa;
- usuarios con pertenencia y permisos por tenant;
- configuracion por empresa;
- sucursales o puntos de venta asociados a empresa;
- trazabilidad por tenant;
- posibilidad de catalogos compartidos donde aplique.

## 11.3 Regla de empresa por usuario

El usuario sera unico en todo el sistema.

Un mismo usuario no podra pertenecer a dos o mas empresas.

La empresa activa debera resolverse automaticamente a partir del usuario autenticado.

No debera existir seleccion manual de empresa en el login.

## 11.4 Catalogo global de productos

Debera contemplarse explicitamente el `Catalogo Global de Productos` con `IdEmpresa = 0`.

Esto implica como minimo:

- productos globales reutilizables;
- acceso de cada empresa a registros `IdEmpresa = 0` y a sus propios registros;
- clonacion de productos globales hacia la empresa actual;
- precio definido durante la importacion, respetando el flujo actual MVC;
- prohibicion de modificar productos globales;
- prohibicion de usar productos globales directamente en ventas, stock, inventario u otras operaciones.

## 11.5 Regla de unicidad al clonar

Un producto global solo podra clonarse en una empresa si no existe ya en esa empresa un producto con el mismo codigo.

La validacion de codigo duplicado debera respetar el comportamiento actual.

## 11.6 Sin resincronizacion automatica

El catalogo global cumple una funcion de ayuda para simplificar la carga.

No debera existir resincronizacion automatica entre el producto global y la copia de la empresa.

## 11.7 Punto de stock por sucursal

Cada producto de empresa debera poder tener un `punto stock` por sucursal.

Esta funcionalidad:

- aplica solo a productos de empresa;
- no aplica a productos globales;
- debera inicializarse para todas las sucursales de la empresa al crear o clonar el producto.

## 11.8 Valor por defecto y control

Al crear o clonar un producto:

- se debera crear un registro de punto stock para cada sucursal de la empresa;
- el valor inicial por defecto sera `0`.

Si el `punto stock` es `0`, no se debera controlar faltante para esa sucursal.

Si por algun motivo faltara el registro producto-sucursal, el sistema debera asumir `0`.

## 11.9 Regla de faltante

Cuando el `punto stock` sea mayor a `0`, se considerara faltante cuando:

- `stock actual <= punto stock`

La visualizacion debera mostrar estado interpretable para operador, por ejemplo:

- rojo con diferencia negativa cuando falte;
- verde con diferencia positiva cuando este por encima.

## 11.7 Regla de diseño

Toda consulta, caso de uso o politica de seguridad debera diseñarse considerando multitenancy desde el inicio.

No debera agregarse como parche posterior.

La separacion de datos debera respetar siempre `Row Level Security (RLS)` en SQL Server.

La aplicacion no debera intentar reemplazar esa politica con filtros manuales como mecanismo principal de aislamiento.

---

# Capitulo 12. POS y Punto de Expendio

## 12.1 Prioridad maxima

POS y Punto de Expendio constituyen el corazon operativo del sistema.

Toda decision que impacte estas areas debera evaluarse con prioridad absoluta.

## 12.2 Requisitos obligatorios

El nuevo POS debera privilegiar:

- uso intensivo con teclado;
- foco automatico;
- minima cantidad de clics;
- respuesta inmediata;
- lectura rapida de codigos;
- integracion con balanzas;
- integracion con impresion termica;
- tolerancia a errores humanos;
- continuidad operativa aun con contextos no ideales.

Estos criterios no aplican solo al POS de escritorio historico.

Tambien deberan preservarse en los flujos web actuales que ya fueron diseñados con foco operativo, incluyendo:

- atajos de teclado;
- navegacion rapida;
- reduccion del uso del mouse;
- secuencias de carga optimizadas para operador.

## 12.3 Regla UX para POS

Una mejora visual que agregue lentitud operativa no sera considerada una mejora.

## 12.4 Responsive sin perder velocidad

El POS podra adaptarse a tablets o moviles en escenarios puntuales, pero la prioridad principal seguira siendo la velocidad de operacion real en caja.

## 12.5 Fuente principal de UX operativa para NG

Para `CarniSys NG`, la referencia principal de experiencia de uso operativa debera ser la `Web` actual cuando ya exista alli una solucion funcional bien resuelta.

Esto implica:

- tomar la web actual como guia principal de navegacion y flujo en pantalla;
- preservar atajos y decisiones UX pensadas para operador;
- mejorar esa experiencia sin perder velocidad;
- evitar trasladar innecesariamente patrones propios de WinForms al nuevo sistema web.

## 12.6 Rol complementario de WinForms

`Presentacion` WinForms seguira siendo una fuente funcional critica para:

- validar reglas de negocio;
- revisar flujos historicos sensibles;
- detectar casos borde;
- confirmar comportamientos que todavia no esten reflejados en la web.

Pero no debera imponerse como base principal de UX para `CarniSys NG` cuando el flujo web actual ya haya sido resuelto con criterio operativo correcto.

## 12.7 Reglas de sucursal y caja para POS

El usuario podra cambiar de sucursal activa desde el menu de usuario, como funciona actualmente.

Si el sistema detecta que el usuario tiene la pantalla POS abierta, debera advertir que primero debe cerrar POS antes de cambiar de sucursal.

Al abrir POS, el sistema debera informar claramente en que sucursal esta operando y ofrecer la opcion de cambiar de sucursal antes de continuar.

Para operar en `POS Ventas` o `Expendios` sera obligatoria una sucursal activa.

Para operar en `POS Ventas` sera obligatoria ademas una caja abierta.

Estas validaciones deben respetar como referencia el comportamiento actual del proyecto web MVC.

## 12.8 Atajos, foco y secuencia

El comportamiento actual de POS en la web MVC debera preservarse en:

- atajos de teclado;
- orden de foco;
- mensajes y validaciones visibles;
- secuencia de pasos de venta.

Cualquier mejora sugerida por criterios modernos de punto de venta debera consultarse antes de implementarse.

## 12.9 Lectura de codigos

El sistema debera soportar:

- lector fisico de codigos de barras;
- camara de telefono movil.

En modo responsive o mobile debera verificarse funcionamiento correcto en:

- Android;
- iOS.

Deberan validarse correctamente, como minimo:

- `EAN-8`;
- `EAN-13`.

## 12.10 Impresion y comprobantes

El comportamiento de impresion debera tomar como referencia el POS MVC actual.

El flujo debera contemplar:

- modal de impresion;
- eleccion de impresion termica;
- eleccion de tipo de ticket segun ancho del papel;
- generacion de PDF;
- envio por mail.

Se permite evaluar mejoras como resaltar ciertos campos en termica, por ejemplo el total en negrita, pero cualquier cambio debe validarse antes de implementarse.

---

# Capitulo 13. Infraestructura, portabilidad y despliegue

## 13.1 Portabilidad

El nuevo proyecto debera ejecutarse en Linux y Windows.

## 13.2 Restriccion

No debera depender de:

- `IIS` como requisito excluyente;
- componentes exclusivos de Windows Server;
- APIs que bloqueen despliegue cruzado salvo caso excepcional documentado.

## 13.3 Base de datos

La primera etapa debera ser compatible con `SQL Server`.

La arquitectura debera facilitar futura compatibilidad con `PostgreSQL`.

Esto no implica forzar una falsa neutralidad que complique la etapa actual, pero si evitar acoplamientos innecesarios.

---

# Capitulo 14. Pruebas y validacion contra el sistema actual

## 14.1 Principio general

Todo modulo migrado debera compararse contra el comportamiento real del sistema vigente.

## 14.2 Tipos de validacion requeridos

Deberan contemplarse:

- pruebas unitarias donde tenga sentido;
- pruebas de integracion;
- pruebas funcionales por flujo;
- comparacion contra base actual;
- pruebas manuales operativas;
- validacion del Arquitecto Funcional.

## 14.3 Regla de aceptacion

Una funcionalidad no estara lista solo porque compile o pase pruebas tecnicas.

Tambien debera comportarse correctamente para el negocio.

Un modulo NG solo podra considerarse reemplazable cuando:

- cubra el flujo actual completo;
- respete permisos;
- respete impresion e integraciones;
- valide resultados contra el sistema actual.

## 14.4 Base de datos en primera etapa

La primera etapa utilizara la misma base actual en `SQL Server`.

La nueva aplicacion debera ser compatible con la estructura y el funcionamiento existentes.

La arquitectura debera reducir dependencia directa a detalles de `SQL Server` cuando eso no afecte el funcionamiento actual, para facilitar una futura migracion.

## 14.5 Frontend base recomendado

La base recomendada para la nueva web sera `Razor/MVC server-rendered` con `JavaScript progresivo`.

Este enfoque se considera el mejor punto de partida actual por equilibrio entre:

- multiplataforma;
- velocidad operativa;
- similitud con la web actual;
- menor complejidad inicial.
