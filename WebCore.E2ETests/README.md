# WebCore.E2ETests

Tests de browser real (Microsoft.Playwright + Chromium) contra una instancia de `WebCore` ya
corriendo. Reemplaza la verificación por `curl` (que no ejecuta JS ni puede pasar el antiforgery
de doble cookie) para flujos que necesitan un click/doble-click/submit real — ver
`docs/10-migracion-aspnet-core/README.md`, sección "Sidebar de navegación global" y Módulo 3
(`GenerarEtiquetasPdf`), y `docs/DECISIONS.md` (2026-09-05).

## Cómo correrlos

1. Levantar `WebCore` aparte, escuchando en `http://localhost:5270` (`dotnet run` desde
   `WebCore/`, o el `.exe` publicado). Estos tests **no** levantan el servidor — apuntan a
   `WebCoreFixture.BaseUrl`.
2. Una sola vez por máquina (o después de actualizar el paquete `Microsoft.Playwright`): instalar
   el binario de Chromium:
   ```powershell
   dotnet build WebCore.E2ETests
   .\WebCore.E2ETests\bin\Debug\net10.0\playwright.ps1 install chromium
   ```
3. Correr los tests:
   ```powershell
   dotnet test WebCore.E2ETests
   ```

## Tests actuales

- `SidebarSmokeTests` — sidebar global renderiza.
- `GenerarEtiquetasPdfTests` — flujo real de doble clic + submit + descarga de PDF (Módulo 3).
- `StaticAssetsTests` — regresión del bug real del 2026-09-05 (`docs/DECISIONS.md`): `jquery.min.js`
  debe servirse completo y `window.jQuery` debe quedar definido en un navegador real. Encontrado
  en la auditoría de paridad Web clásico vs WebCore — `curl` sin `--compressed` nunca lo detectaba.

## Convención para tests nuevos

Cada test usa la fixture compartida `WebCoreFixture` (`[Collection("WebCore browser")]`), que
crea un único `IBrowser` para toda la clase de tests. Un test nuevo solo necesita:
`await _fixture.Browser.NewPageAsync()` y navegar/interactuar como lo haría un usuario real —
nunca reconstruir el token antiforgery a mano (ese era justamente el problema que esto resuelve).
