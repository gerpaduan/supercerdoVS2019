# Detalle fino para implementacion

## Objetivo

Concentrar las definiciones funcionales y tecnicas que ya quedaron acordadas para evitar supuestos al comenzar la implementacion de `CarniSys NG`.

## 1. Primeros pilares

La arquitectura debe construirse sobre estos tres pilares:

1. autenticacion
2. empresa
3. permisos

El orden exacto de implementacion puede ajustarse por conveniencia tecnica, pero esos tres pilares son obligatorios desde el inicio.

## 2. Autenticacion

- Mantener login clasico con usuario y contraseña como base inicial.
- La empresa no se elige en el login.
- La empresa activa se resuelve automaticamente desde el usuario autenticado.
- Un usuario no puede pertenecer a mas de una empresa.

## 3. Empresa, sucursal y sesion

- El usuario tiene una empresa unica.
- La sucursal activa puede cambiarse desde el menu de usuario.
- Si el usuario tiene POS abierto, el sistema debe impedir cambio de sucursal hasta cerrar POS.
- Al abrir POS se debe informar la sucursal activa y ofrecer cambio antes de continuar.

## 4. Reglas de POS, sucursal y caja

- `POS Ventas` requiere sucursal activa.
- `POS Expendios` requiere sucursal activa.
- `POS Ventas` requiere caja abierta.
- Estas validaciones deben respetar la implementacion actual del proyecto MVC.

## 5. Permisos

El sistema debe mantener como minimo estas tres dimensiones de permiso:

- lectura por rango de dias
- edicion por rango de dias
- alcance de edicion: propios o todos los registros de la empresa

## 6. Mejora recomendada para permisos

Se recomienda simplificar el modelo interno sin perder funcionalidad.

Direccion sugerida:

- conservar las mismas reglas funcionales actuales;
- representarlas con un modelo mas explicito y mantenible;
- separar permiso base, alcance temporal y alcance sobre autor.

No debe simplificarse eliminando capacidades que hoy el negocio usa.

## 7. Catalogo global

Regla base:

- los registros globales tienen `EmpresaID = 0`
- cada empresa ve los registros globales y los propios
- el catalogo global sirve para ayudar a cargar registros en la empresa

## 8. Uso permitido del catalogo global

Los productos globales no pueden usarse directamente en:

- ventas
- movimientos de stock
- inventario
- otras operaciones del sistema

Solo se usan desde la pantalla de catalogo global para clonarlos a la empresa actual.

## 9. Clonado de productos globales

- La empresa puede clonar un producto global.
- El producto clonado debe respetar el flujo actual MVC, donde el precio se ingresa y valida al importar.
- El producto global nunca puede modificarse.
- Toda modificacion se hace solo sobre la copia de la empresa.
- El clonado solo puede ocurrir si no existe ya en la empresa un producto con el mismo codigo.

## 10. Alta de producto por EAN con recuperacion desde catalogo global

Cuando se crea un producto y el codigo ingresado es un `EAN-8` o `EAN-13` valido:

- si existe coincidencia en el catalogo global, se deben recuperar automaticamente esos datos;
- el sistema debe informar visiblemente al usuario que los datos fueron recuperados desde el catalogo global.

Esta recuperacion automatica aplica en alta.

No debe aplicarse en edicion.

## 11. Relacion entre producto global y copia

- No es obligatorio conservar un vinculo formal con el producto global de origen.
- No debe planearse resincronizacion automatica.
- El catalogo global cumple una funcion de ayuda, no de sincronizacion.

## 12. Web como referencia principal de UX

La referencia principal de UX operativa para NG sera el proyecto `Web` actual.

Debe preservarse:

- atajos
- orden de foco
- secuencia operativa
- mensajes al usuario
- reduccion del uso del mouse

## 13. Regla de mejoras UX

Se pueden proponer mejoras basadas en buenas practicas de POS de supermercado o caja rapida.

Pero toda mejora debe:

- justificarse;
- explicarse;
- pedirse para validacion antes de implementarse.

## 14. Lectura de codigos

Debe soportarse:

- lector fisico
- camara mobile

Debe verificarse en:

- Android
- iOS

Formatos minimos:

- EAN-8
- EAN-13

## 15. Balanza

La integracion actual via agente local separado es la referencia base.

Se puede evaluar una alternativa mas simple, segura y multiplataforma.

Si no existe una opcion claramente mejor o si la alternativa agrega riesgo, se debe continuar con agente local separado.

## 16. AFIP

Tomar como referencia principal la implementacion web MVC actual.

Debe conservarse:

- facturacion electronica
- emision de notas de credito
- flujo actual de operacion

Las mejoras deben consultarse antes de implementarse.

## 17. Impresion

Tomar como referencia el flujo actual del POS MVC.

Debe contemplarse:

- modal de impresion
- impresion termica
- tipo de ticket segun ancho del papel
- PDF
- envio por mail

Se puede evaluar resaltar datos en termica, por ejemplo total en negrita, con validacion previa.

## 18. Base de datos

- La primera etapa usa la misma base `SQL Server` actual.
- La nueva aplicacion debe ser compatible con la estructura actual.
- Debe respetarse siempre `RLS`.
- La app no debe reemplazar el aislamiento de empresa con filtros manuales como estrategia principal.

## 19. Frontend inicial

La base recomendada es:

- `Razor/MVC server-rendered`
- `JavaScript progresivo`

Se elige este enfoque por:

- velocidad operativa
- menor complejidad inicial
- cercania con la web actual
- mejor punto de partida multiplataforma

## 20. Criterio de reemplazo de modulo

Un modulo NG se considera reemplazable cuando:

- cubre 100 por ciento del flujo actual previsto
- respeta permisos
- respeta impresion e integraciones
- valida resultados contra el sistema actual

## 21. Modulo a seguir luego de autenticacion

Despues de autenticacion, empresa y permisos, el siguiente modulo debe definirse explicitamente antes de construirlo.

La recomendacion actual sigue siendo arrancar por un flujo vertical chico y real, por ejemplo:

- catalogo global y productos
- personas
- stock basico

POS debe diseñarse desde el inicio, pero no necesariamente ser el primer modulo implementado.

## 22. Punto stock por sucursal

Se incorpora una funcionalidad nueva para `CarniSys NG`:

- cada producto de empresa tendra punto stock por sucursal;
- los productos globales no participan de este control;
- al crear o clonar un producto se generaran registros para todas las sucursales de la empresa;
- el valor inicial por defecto sera `0` en todas las sucursales.

## 23. Regla de control

- si `punto stock = 0`, no se controla faltante;
- si falta el registro producto-sucursal, se asume `0`;
- existe faltante cuando `stock actual <= punto stock`.

## 24. Edicion en pantalla de producto

En la edicion de producto:

- por defecto el usuario podra ingresar un valor general de `punto stock`;
- ese valor se copiara a todas las sucursales;
- debera existir una opcion para editar por sucursal;
- al activar esa opcion se desplegara una grilla para setear el `punto stock` por cada sucursal.

## 25. Alta de sucursal nueva

Cuando se agregue una sucursal nueva a una empresa, el sistema debera ofrecer al menos estas opciones:

- crear todos los puntos stock en `0`;
- copiar los puntos stock desde una sucursal existente.

## 26. Edicion masiva

Debe contemplarse una edicion masiva o por lote de `punto stock`.

Comportamiento esperado:

- se filtran productos desde el index;
- el usuario puede editar los puntos de stock uno por uno;
- con `Enter` debe poder avanzar rapidamente;
- al completar las sucursales de un producto debe pasar al siguiente producto de la lista.

## 27. Visualizacion

En consultas de stock por sucursal debera mostrarse:

- stock actual;
- punto stock;
- diferencia respecto del punto stock;
- alerta visual.

Ejemplos:

- rojo `-2` si faltan dos unidades respecto del punto;
- verde `+8` si hay ocho por encima del punto.

## 28. Permisos

La edicion de `punto stock` por sucursal utilizara el mismo permiso de edicion de producto.
