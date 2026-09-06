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

// Login/sesion real (2026-09-06, ver docs/DECISIONS.md "Login/Sesion real para WebCore"). Cookie
// Authentication (no Identity, no OIDC -- el modelo de usuario es 100% custom, Entidades.Usuario/
// Negocio.Usuario ya compartido net472;net10.0) reemplaza a Session["Usuario"]+Forms Auth del
// clasico. Mejora real de paso: todo el estado de sesion vive en la cookie firmada, no en memoria
// del proceso -- resuelve de raiz el bug ya documentado en produccion clasica ("la sesion vencia a
// los pocos minutos" por el idle-timeout del App Pool matando la Session in-proc).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<WebCore.Services.IUsuarioSesionService, WebCore.Services.UsuarioSesionService>();

builder.Services
    .AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Login/Logout";
        options.AccessDeniedPath = "/Login";
        // 12hs deslizante, misma duracion que sessionState/forms timeout=720 del Web.config
        // clasico -- ver decision registrada en el plan, facil de ajustar despues.
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    // Toda accion requiere sesion real por default -- LoginController se marca [AllowAnonymous]
    // explicitamente (unico controller que lo necesita). Evita tener que poner [Authorize] a mano
    // en cada uno de los controllers ya portados (fan-out del Batch 3, ver docs/DECISIONS.md).
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Session de ASP.NET Core (in-memory, un solo servidor -- ya descartada la sesion distribuida en
// el plan original de esta migracion) SOLO para el estado transitorio del operador de produccion
// con contraseña (mecanismo B, Batch 5) -- la identidad principal del usuario va en la cookie de
// autenticacion de arriba, nunca en esta Session.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Bug real encontrado el 2026-09-05 (ver docs/DECISIONS.md), detectado recien con Playwright real
// -- curl sin --compressed nunca lo disparaba: MapStaticAssets() (pipeline de assets estaticos de
// .NET 9/10, con manifest de compresion generado en build) devolvia Content-Length: 0 para
// jquery.min.js (y probablemente otros archivos de wwwroot/lib) a CUALQUIER cliente que pida
// gzip -- osea, todo navegador real, siempre. Rompia jQuery en TODA la app silenciosamente (sin
// error de servidor, sin 404, el navegador solo recibia un archivo vacio). Reemplazado por
// UseStaticFiles(), el middleware clasico: sirve los archivos de wwwroot tal cual, sin manifest
// ni compresion de build -- mismo mecanismo que uso toda la migracion hasta ahora sin problemas.
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
