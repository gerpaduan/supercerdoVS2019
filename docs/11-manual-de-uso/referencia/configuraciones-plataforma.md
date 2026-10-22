---
titulo: Mapa de configuraciones de la plataforma
rol: superadmin
modulo: Referencia
orden: 1
revisada: 2026-10-12
---
# Mapa de configuraciones de la plataforma

Lo que se configura **a nivel de plataforma o servidor** (no por empresa). Para lo de cada empresa ver el [mapa de la empresa](ayuda:referencia/configuraciones).

## Por empresa, desde Administración del sistema
| Configuración | Dónde | Efecto |
|---|---|---|
| **CUIT** (único entre empresas) | [Editar empresa](ayuda:pantalla/SystemAdministration.EditarEmpresa) | Identifica a la empresa y arma su dirección de ingreso `/Login/<CUIT>`. **No puede repetirse**: el sistema lo valida y la base de datos tiene un índice único. |
| **Empresa activa** | [Editar empresa](ayuda:pantalla/SystemAdministration.EditarEmpresa) | Una empresa inactiva no tiene dirección de ingreso por CUIT. |
| **Entorno AFIP** (PROD / HOMO) | [Editar empresa](ayuda:pantalla/SystemAdministration.EditarEmpresa) | HOMO = modo prueba: las facturas no tienen validez fiscal. Los admin ven un cartel "MODO PRUEBA (AFIP)". |
| **Empresa propia / RRII / Es carnicería** | [Editar empresa](ayuda:pantalla/SystemAdministration.EditarEmpresa) | Marcas de la empresa. **PENDIENTE:** documentar el efecto exacto de cada una en los módulos. |
| **Titular compartido entre empresas** (mismo CUIT como titular en varias empresas) | [Titulares de facturación](ayuda:pantalla/TitularesFiscales.Index) → *Administración del sistema → Empresas → Titulares* | Solo el superadmin lo asigna. Todas las empresas con ese CUIT usan el **mismo certificado** (sin pedir otro); lo administra una sola (donde el CUIT es el principal). Ver [titular compartido entre empresas](ayuda:concepto/titular-compartido-entre-empresas). |
| **Cancelar / reponer la titularidad** de una empresa | [Titulares de facturación](ayuda:pantalla/TitularesFiscales.Index) | Solo superadmin. Si nunca facturó se elimina; si ya facturó queda "cancelada" (de solo lectura). Mientras esté cancelada la empresa no puede emitir facturas ni notas de crédito con ese titular. |
| **No cambiar el CUIT de una empresa mientras otras la usan como titular** | [Editar empresa](ayuda:pantalla/SystemAdministration.EditarEmpresa) | El sistema lo impide hasta cancelar esas titularidades. |

## Del servidor (archivo de configuración del servidor, `WebCore.dll.config`)
| Clave | Por defecto | Efecto |
|---|---|---|
| `Security:AccountLockoutMaxAttempts` | 7 | Intentos fallidos por usuario antes del bloqueo. |
| `Security:LoginMaxAttempts` / `LoginWindowMinutes` / `LoginLockoutMinutes` | 7 / 10 / 15 | Freno por IP (en dispositivos no autorizados): intentos, ventana y minutos de bloqueo. |
| `Security:PasswordResetTokenMinutes` | 60 | Vigencia de los enlaces de recuperación de contraseña y de desbloqueo. |
| `Security:TrustedProxies` | vacío | IPs/redes de proxies de confianza **además** de loopback y redes privadas. Hace falta solo si el proxy (Caddy, IIS+ARR, balanceador) está en una IP pública distinta de la app. Si está mal, el freno por IP ve la IP del proxy para todos. **Al cambiar de servidor, revisarlo.** |
| `Movimientos:ReplicaEntreBases` | `true` con SQL Server, `false` con Postgres | [Movimientos](ayuda:pantalla/Movimientos.Editar): `true` = esta base replica sus movimientos con otra (Servidor SM ↔ San Lorenzo) y el botón **Eliminar** queda deshabilitado, porque la copia en la otra base no se borra sola. Poner `false` cuando se unifiquen las bases. |
| `ExistenciaCarga:MaxConcurrentes` | 2 | [Existencia por sucursales](ayuda:pantalla/Stock.ExistenciaPorSucursales): cálculos de stock simultáneos a partir de los cuales las **actualizaciones automáticas** se postergan ("servidor ocupado"). **0** pospone todas las automáticas (sirve para probar el aviso). El botón **Buscar** nunca se bloquea. Rango 0–50. |
| `ExistenciaCarga:UmbralLatenciaMs` | 5000 | Si el último cálculo de existencia tardó más que esto (milisegundos), el servidor se considera cargado y las automáticas esperan. Rango 500–120000. Normal: ~1000 ms. |
| `ExistenciaCarga:ReintentoSegundos` | 120 | Cada cuántos segundos reintenta una automática postergada, y cuánto dura la espera tras un cálculo lento (pasada esa espera deja entrar **una** prueba cuya duración reemplaza a la anterior). Rango 10–3600. |
| `ExistenciaCarga:JitterInicialMaxSegundos` | 5 | La primera búsqueda automática de cada pantalla espera entre 0 y este valor (segundos) al azar, para que las pantallas abiertas a la vez no lleguen juntas. 0 = sin espera. Rango 0–60. |
| `Pedidos:DiasPrecioViejo` | 90 | [Pedidos](ayuda:pantalla/Pedidos.Detalle): un precio de última compra de hace **más** de estos días (respecto de la fecha del pedido) se marca como viejo (⚠) en el armado, el detalle, el PDF y el Excel (el ticket no lleva importes). Rango 1–3650. No cambia el importe, solo la advertencia. |
| `Pedidos:DiasPrecioMaximo` | 365 | [Pedidos](ayuda:pantalla/Pedidos.Detalle) y *Ingresar pedido*: una compra de hace **más** de estos días (respecto de hoy) **no da precio**: el producto queda «sin precio», no suma al total y el total se rotula «parcial». Evita que precios de hace años (inflación) subestimen el importe. Rango 30–36500. Distinto de `DiasPrecioViejo` (ese avisa y sí suma). |
| `Smtp*` (host, usuario, remitente…) | — | Envío de mails: recuperación de contraseña, desbloqueo, código de dispositivo y avisos a administradores. Sin esto esas funciones no mandan mails. |
| `Passkeys:*` | apagado | Habilita el ingreso con huella (necesita HTTPS con dominio real). |
| `DataEngine` | Postgres | Motor de base de datos (SQL Server solo en servidores heredados). |

> El detalle operativo (cómo desplegar, qué migraciones aplicar) está en `docs/07-operacion-y-soporte/despliegue-y-publicacion.md`; acá solo se explica qué hace cada valor.
