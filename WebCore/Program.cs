// QuestPDF (reemplazo de iTextSharp para generacion de PDF, ver docs/10-migracion-aspnet-core/
// README.md): la license Community es gratis para el porte de CarniSys, pero hay que declararla
// una vez al arrancar o cada generacion de PDF tira excepcion.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// SuppressImplicitRequiredAttributeForNonNullableReferenceTypes=true: ASP.NET Core, a diferencia de
// MVC5, marca "Required" implicito a toda propiedad string no-nullable cuando el proyecto tiene
// Nullable habilitado (que es el caso de WebCore). Los ViewModels portados desde Web/Models/*.cs
// solo deben ser obligatorios donde el original tiene [Required] explicito -- sin este flag,
// paridad rota detectada por el juez en el slice de Empresas (docs/DECISIONS.md, 2026-09-01).
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
