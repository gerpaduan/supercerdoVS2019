# Datos.BorradoresTests — juez de paridad SQL Server / Postgres de los borradores

Una sola suite (xUnit, `net10.0`) del contrato de `IBorradorGenericoRepository` e `IVentaBorradorRepository`
que corre **idéntica** contra los dos motores: `Datos/BorradorGenerico.cs` + `Datos/VentaBorrador.cs` (SQL Server)
y `DatosPostgres/BorradorGenericoPg.cs` + `VentaBorradorPg.cs` (Postgres). Si una prueba pasa en uno y falla en
el otro, hay una divergencia de comportamiento. Ver `docs/DECISIONS.md` "Borradores en SQL Server".

Cada prueba usa una empresa propia (ids altos aleatorios): las bases scratch **no se limpian** y no hace falta.
Sin la base de un motor, sus pruebas salen **Omitidas** con el motivo (nunca verdes en silencio).

## Preparar las bases scratch (una vez)

**SQL Server** (Express local, autenticación de Windows; base `CarniSys_BorradoresTest`, cadena en `App.config`):

```bash
sqlcmd -S .\SQLEXPRESS -E -Q "CREATE DATABASE CarniSys_BorradoresTest; ALTER DATABASE CarniSys_BorradoresTest SET AUTO_CLOSE OFF;"
sqlcmd -S .\SQLEXPRESS -E -b -d CarniSys_BorradoresTest -i Datos/DB-Procedures/20260923-Create_Borradores.sql
sqlcmd -S .\SQLEXPRESS -E -b -d CarniSys_BorradoresTest -Q "CREATE TABLE dbo.Usuarios (id INT NOT NULL PRIMARY KEY, nombre NVARCHAR(120) NULL);"
```

(`Usuarios` mínima: los repos hacen `LEFT JOIN Usuarios` para traer el nombre del operador.)

**Postgres**: crear una base scratch (dueño el rol admin), correr `DatosPostgres/DB-Migrations/20260922a-Create_borradorgenerico.sql`
y `20260921b-Create_ventaborrador_productosinagregar_notificaciones.sql` con el rol admin, y crear
`usuarios (id integer primary key, nombre text)` con `GRANT SELECT ... TO carnisys_user, cs_admin_pg`. Las credenciales
viven en `~/hosts/postgres-local.env` (nunca en el repo). Exportar la cadena Npgsql **con un rol no superusuario** (para
que aplique RLS) en `CARNISYS_TEST_PG` (ej. la de `ConexionPostgresPiloto` apuntando a la base scratch).

## Correr

```bash
cd tests/Datos.BorradoresTests
dotnet test
```

Para correr la pata SQL Server contra otra base (ej. el `SuperCerdo` local), editar `Database=` en
`bin/Debug/net10.0/Datos.BorradoresTests.dll.config` (no en el repo) y borrar después las filas de prueba
(`idEmpresa >= 100000`) de las 4 tablas de borradores.

## Validar al juez

Romper a propósito un repo (ej. quitar `AND idOperador = @idOperador` del `UPDATE` de `Datos/BorradorGenerico.Guardar`)
y confirmar que aparecen pruebas rojas. Hecho el 2026-09-23: 2 rojas con dos mutaciones.
