using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Helper compartido para fijar precios de productos a un valor conocido antes de un test
// (2026-09-07, ver docs/DECISIONS.md). Varios tests de esta suite asumen precios fijos para
// productos reales del seed (CARRE=idCorte 33=$12000, CABEZA=idCorte 34=$2800) -- en vez de
// confiar en "lo que dejo el test anterior" (fragil: un cleanup fallido en un test se propaga a
// todos los siguientes), cada test que depende de un precio especifico lo fija el mismo al
// empezar, usando este helper.
public static class PreciosSeedHelper
{
    public const int IdCorteCarre = 33;
    public const int IdCorteCabeza = 34;
    public const string PrecioCarreCanonico = "12000,00";
    public const string PrecioCabezaCanonico = "2800,00";

    public static async Task<bool> FijarPrecioAsync(IPage page, int idCorte, string precioCsv)
    {
        return await page.EvaluateAsync<bool>(@"
            async ([id, precio]) => {
                const token = document.querySelector('input[name=""__RequestVerificationToken""]')?.value || '';
                const form = new URLSearchParams();
                form.set('__RequestVerificationToken', token);
                form.set('IdCorte', id);
                form.set('PrecioKg', precio);
                const resp = await fetch('/Productos/EditPrecioCorte', { method: 'POST', body: form });
                return resp.ok;
            }", new[] { idCorte.ToString(), precioCsv });
    }

    // Requiere que `page` este autenticada -- navega a /Productos (donde vive el token
    // antiforgery del form) antes de fijar los precios, y no navega de vuelta (el caller sigue
    // desde ahi).
    public static async Task FijarPreciosCanonicosAsync(IPage page)
    {
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var okCarre = await FijarPrecioAsync(page, IdCorteCarre, PrecioCarreCanonico);
        var okCabeza = await FijarPrecioAsync(page, IdCorteCabeza, PrecioCabezaCanonico);
        if (!okCarre || !okCabeza)
        {
            throw new InvalidOperationException($"No se pudieron fijar los precios canonicos (CARRE ok={okCarre}, CABEZA ok={okCabeza})");
        }
    }
}
