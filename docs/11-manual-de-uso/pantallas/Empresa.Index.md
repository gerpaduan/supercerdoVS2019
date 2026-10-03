---
pantalla: Empresa.Index
titulo: Mi Empresa
rol: admin
modulo: Mi empresa
orden: 1
alias: Empresa.Guardar
permiso: Admin de la empresa (para modificar)
revisada: 2026-10-03
---
## Para qué sirve
Ver y editar los **datos generales de tu empresa**, su **horario laboral** y si se **exige dispositivo seguro**. Está en *Configuración → Mi Empresa*. Cualquier usuario que entre ve los datos en lectura; solo un administrador puede modificarlos.

## Cómo se usa
1. La pantalla abre en **modo lectura**. Tocá **Modificar** (solo administradores) para habilitar los campos.
2. Cambiá lo que necesites y tocá **Guardar** (aparece después de Modificar). Aviso: *"Los datos de la empresa se guardaron correctamente."*

## Qué podés cambiar
- **Datos generales:** nombre de fantasía, teléfono, email, domicilio, ciudad, país y los tres slogans. (No se validan como obligatorios; el email no se chequea.)
- **Razón social (AFIP) y CUIT:** solo se muestran. Los cambia el **super administrador**.
- **Horario laboral:** dos jornadas (diurna y tarde, "desde" y "hasta"). Los **empleados (no administradores) solo pueden iniciar sesión dentro de esas jornadas**; fuera de ellas ven *"Fuera del horario laboral permitido para iniciar sesión"* (también con huella). Por defecto cubren todo el día (00:00 a 23:59), o sea **sin restricción**. Una jornada con "desde" mayor que "hasta" se entiende que cruza la medianoche. Los administradores están exentos. Si un horario no es una hora válida: *"Los horarios ingresados no son válidos."*
- **Dispositivos seguros → "Exigir dispositivo seguro a no-administradores"** (apagado por defecto):
  - **Encendido:** los empleados solo ingresan desde un dispositivo autorizado (PC cargada en [Dispositivos seguros](ayuda:pantalla/DispositivosSeguros.Index), celular o navegador que el empleado autoriza con un código al mail, o uno que vos aprobás cuando te lo piden).
  - **Apagado:** pueden ingresar desde cualquier dispositivo con su contraseña.
  - En los dos casos **los administradores siempre pueden ingresar**, y la lista de usuarios y el PIN del [ingreso por CUIT](ayuda:concepto/login-por-cuit) solo se habilitan en dispositivos autorizados.
  - También se puede exigir **solo a algunos usuarios** desde [Editar usuario](ayuda:pantalla/Usuarios.Editar).
  - El botón **¿Cómo funciona?** explica todo en detalle. **Antes de encenderlo**, verificá que cada empleado tenga su mail cargado o avisales que te pidan la autorización.

## Reglas
- Un usuario que no es administrador y toca Guardar recibe *"solo un administrador puede modificarlos"* y no se cambia nada.
- Solo se guardan los campos de esta pantalla; el resto de los datos de la empresa (CUIT, condición de IVA, entorno AFIP…) los administra el super administrador.

Ver también: [dispositivo seguro](ayuda:concepto/dispositivo-seguro) · [mapa de configuraciones](ayuda:referencia/configuraciones)
