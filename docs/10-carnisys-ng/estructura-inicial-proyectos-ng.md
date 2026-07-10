# Estructura inicial de proyectos NG

## Objetivo

Definir con precision que proyectos deberian crearse primero para `CarniSys NG` dentro de `CarniSys.sln`, que responsabilidad tendria cada uno y que dependencias entre proyectos estaran permitidas.

## Solucion inicial recomendada

### `CarniSys.NG.Domain`

Responsabilidad:

- entidades de dominio nuevas;
- value objects;
- enums;
- reglas puras;
- invariantes;
- conceptos centrales del negocio.

Debe contener:

- ventas
- caja
- stock
- compras
- personas
- usuarios
- permisos
- facturacion como concepto de dominio

No debe contener:

- Entity Framework
- SQL
- HTTP
- UI
- impresoras
- balanzas
- referencias a proyectos legacy

### `CarniSys.NG.Application`

Responsabilidad:

- casos de uso;
- servicios de aplicacion;
- comandos y queries;
- DTOs propios;
- validaciones de aplicacion;
- contratos para persistencia e integraciones.

Debe orquestar:

- flujos de venta
- cierre de caja
- punto de expendio
- catalogo global
- permisos
- integracion AFIP como contrato
- dispositivos como contrato

No debe contener:

- acceso directo a SQL
- controladores MVC
- logica visual
- llamadas directas a proyectos legacy

### `CarniSys.NG.Infrastructure`

Responsabilidad:

- implementacion de persistencia;
- acceso a SQL Server en primera etapa;
- repositorios;
- mapeos;
- configuraciones tecnicas;
- adaptadores a archivos o configuracion.

Debe incluir inicialmente:

- persistencia SQL Server;
- implementaciones de contratos definidos en `Application`;
- trazabilidad y configuracion tecnica.

Regla:

Debe quedar preparada para futura migracion o coexistencia con PostgreSQL, sin fingir neutralidad absoluta desde el primer dia.

### `CarniSys.NG.Web`

Responsabilidad:

- interfaz web principal;
- autenticacion y autorizacion;
- componentes de UI;
- endpoints;
- paginas operativas;
- UX orientada a teclado;
- experiencia responsive y PWA.

Debe tomar como guia principal:

- UX del proyecto `Web` actual;
- reglas operativas validadas en `Presentacion` cuando aplique.

Debe incluir primero:

- login
- shell principal
- navegacion
- permisos
- modulos web priorizados
- base para POS web

### `CarniSys.NG.Integrations.AFIP`

Responsabilidad:

- encapsular integracion con AFIP;
- implementar contratos de `Application`;
- aislar detalles tecnicos de autenticacion, padron y facturacion electronica.

Debe tomar como referencia:

- proyecto `AFIP` como fuente principal;
- `wsAFIPvs2008` como fuente secundaria de comportamiento legacy.

### `CarniSys.NG.Integrations.Devices`

Responsabilidad:

- encapsular balanzas;
- encapsular impresion termica si luego se incorpora aqui;
- abstraer lectura de peso y dialogo con dispositivos.

Debe tomar como referencia:

- `Carnisys.Balanza.Agent`;
- partes utiles de `Utilidades`;
- comportamiento observado en `Presentacion`.

## Proyectos de prueba

### `CarniSys.NG.UnitTests`

Responsabilidad:

- probar reglas puras de dominio y aplicacion;
- validar comportamientos sin infraestructura real.

### `CarniSys.NG.IntegrationTests`

Responsabilidad:

- validar persistencia;
- validar integraciones;
- validar compatibilidad de flujos criticos contra infraestructura real o simulada.

## Referencias permitidas entre proyectos

Matriz recomendada:

- `CarniSys.NG.Domain`: no referencia otros proyectos de NG.
- `CarniSys.NG.Application`: puede referenciar `CarniSys.NG.Domain`.
- `CarniSys.NG.Infrastructure`: puede referenciar `CarniSys.NG.Application` y `CarniSys.NG.Domain`.
- `CarniSys.NG.Web`: puede referenciar `CarniSys.NG.Application` y `CarniSys.NG.Infrastructure` solo para composicion.
- `CarniSys.NG.Integrations.AFIP`: puede referenciar `CarniSys.NG.Application` y `CarniSys.NG.Domain`.
- `CarniSys.NG.Integrations.Devices`: puede referenciar `CarniSys.NG.Application` y `CarniSys.NG.Domain`.
- `CarniSys.NG.UnitTests`: puede referenciar los proyectos necesarios para pruebas unitarias.
- `CarniSys.NG.IntegrationTests`: puede referenciar los proyectos necesarios para pruebas integradas.

## Referencias prohibidas

- `Domain` hacia `Infrastructure`
- `Domain` hacia `Web`
- `Application` hacia `Web`
- cualquier proyecto NG referenciando directamente `Presentacion`, `Web`, `Datos`, `Negocio`, `wsAFIPvs2008` o `Utilidades` como dependencia estructural

## Orden recomendado de creacion

1. `CarniSys.NG.Domain`
2. `CarniSys.NG.Application`
3. `CarniSys.NG.Infrastructure`
4. `CarniSys.NG.Web`
5. `CarniSys.NG.Integrations.AFIP`
6. `CarniSys.NG.Integrations.Devices`
7. `CarniSys.NG.UnitTests`
8. `CarniSys.NG.IntegrationTests`

## Primer alcance sugerido

La primera etapa no debera intentar cubrir todo el sistema.

Se recomienda arrancar con:

- autenticacion
- contexto de empresa
- parametros
- permisos
- estructura base de navegacion
- un flujo vertical chico pero real

Flujos candidatos:

- consulta y gestion de productos
- catalogo global
- personas
- stock basico

POS y punto de expendio deben diseñarse desde el inicio, pero no necesariamente ser el primer modulo implementado si eso retrasa la base arquitectonica.

## Regla final

Crear los proyectos correctos al principio es importante.

Crear demasiados proyectos al principio es un error.

La solucion inicial de `CarniSys NG` debe ser:

- chica;
- clara;
- modular;
- extensible;
- alineada con la migracion gradual.
