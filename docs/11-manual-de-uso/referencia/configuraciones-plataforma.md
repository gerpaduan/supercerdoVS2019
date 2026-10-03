---
titulo: Mapa de configuraciones de la plataforma
rol: superadmin
modulo: Referencia
orden: 1
revisada: 2026-10-03
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

## Del servidor (archivo de configuración del servidor, `WebCore.dll.config`)
| Clave | Por defecto | Efecto |
|---|---|---|
| `Security:AccountLockoutMaxAttempts` | 7 | Intentos fallidos por usuario antes del bloqueo. |
| `Security:LoginMaxAttempts` / `LoginWindowMinutes` / `LoginLockoutMinutes` | 7 / 10 / 15 | Freno por IP (en dispositivos no autorizados): intentos, ventana y minutos de bloqueo. |
| `Security:PasswordResetTokenMinutes` | 60 | Vigencia de los enlaces de recuperación de contraseña y de desbloqueo. |
| `Security:TrustedProxies` | vacío | IPs/redes de proxies de confianza **además** de loopback y redes privadas. Hace falta solo si el proxy (Caddy, IIS+ARR, balanceador) está en una IP pública distinta de la app. Si está mal, el freno por IP ve la IP del proxy para todos. **Al cambiar de servidor, revisarlo.** |
| `Smtp*` (host, usuario, remitente…) | — | Envío de mails: recuperación de contraseña, desbloqueo, código de dispositivo y avisos a administradores. Sin esto esas funciones no mandan mails. |
| `Passkeys:*` | apagado | Habilita el ingreso con huella (necesita HTTPS con dominio real). |
| `DataEngine` | Postgres | Motor de base de datos (SQL Server solo en servidores heredados). |

> El detalle operativo (cómo desplegar, qué migraciones aplicar) está en `docs/07-operacion-y-soporte/despliegue-y-publicacion.md`; acá solo se explica qué hace cada valor.
