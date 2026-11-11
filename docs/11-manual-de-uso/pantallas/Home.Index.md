---
pantalla: Home.Index
titulo: Tablero diario (inicio)
rol: usuario
modulo: Inicio
orden: 1
permiso: PENDIENTE (no verificado contra el código en esta revisión; lo que sí se sabe: los datos comerciales los ven solo los administradores, el estado de la balanza lo ve cualquiera)
revisada: 2026-11-10
---
## Para qué sirve
Pantalla de inicio: resumen del negocio en una sola pantalla (ventas, gráficos y últimos movimientos). Cada bloque se carga por separado, así que la pantalla aparece enseguida y los datos van llegando.

## Filtros
- Arriba hay una **franja de una sola línea** con el rótulo *Tablero diario* y los dos filtros: **Periodo** (*Hoy*, *Ultimos 7 dias*, *Este mes*) y **Sucursal**. Antes la franja era más alta y tenía además el título *Resumen del negocio en una sola pantalla* y una frase de ayuda; se quitaron para ganar lugar.
- El tablero abre en **Hoy** y, al cambiar un filtro, refresca cada bloque.
- En el celular los dos filtros ocupan cada uno una línea.

## Totales
Debajo de los filtros hay dos bloques:
- **Ventas**: el **total vendido** (la cifra grande), las **operaciones** (ventas cerradas), los **clientes** atendidos y el **ticket promedio** por venta. Arriba a la derecha dice a qué período y sucursal corresponden (por ejemplo *Hoy · Todas las sucursales*).
- **Cuentas corrientes**: lo que hay **a cobrar** (lo que nos deben, flecha verde) y **a pagar** (lo que debemos, flecha roja). Es el **saldo actual** de todo el sistema: no cambia con el período ni con la sucursal.

Los montos se muestran **sin centavos** para que entren en una línea; al pasar el mouse por encima aparece el importe exacto. Mientras cargan se ve un guion (—).

## Actividades
Lista de las **últimas 10** actividades del período (cambios de precio, ventas anuladas o con descuento manual, egresos, movimientos, compras, fórmulas, autorizaciones y advertencias del POS):
- Cada fila tiene un **icono** según el tipo, el tipo en letra chica, la descripción y la **hora** a la derecha: solo la hora si es de hoy, *ayer* y la hora si es de ayer, y si no el día y la hora (*08/11 11:05*).
- Las **advertencias del POS** (por ejemplo una sesión cerrada con una venta sin terminar) se ven en **ámbar**.
- Si la misma actividad se repite seguida, aparece **una sola vez** con un **×2**, **×3**, etc.
- Cuando la actividad es de una venta, el número de venta (*#48213*) abre su detalle.
- **Ver todas** (arriba a la derecha) abre la pantalla completa de Actividades, con filtros y el detalle de cada advertencia.

## Tablas
Las tablas chicas del tablero (últimas ventas, elaborados, movimientos y finanzas) tienen los **encabezados** al mismo tamaño compacto que el resto del sistema.

## Resto de la pantalla
PENDIENTE: documentar los gráficos y las tablas, y qué ve quien no es administrador (no verificado contra el código en esta revisión).
