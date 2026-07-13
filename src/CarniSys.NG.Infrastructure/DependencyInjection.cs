using CarniSys.NG.Application.Authentication;
using CarniSys.NG.Application.Companies;
using CarniSys.NG.Application.Permissions;
using CarniSys.NG.Application.People;
using CarniSys.NG.Application.Products;
using CarniSys.NG.Application.Movements;
using CarniSys.NG.Application.Purchases;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Application.Stock;
using CarniSys.NG.Application.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CarniSys.NG.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCarniSysNgInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LegacyAuthenticationOptions>(configuration.GetSection(LegacyAuthenticationOptions.SectionName));
        services.AddSingleton<ILegacyConnectionStringResolver, LegacyConnectionStringResolver>();
        services.AddScoped<IAuthenticationService, LegacyAuthenticationService>();
        services.AddScoped<IBranchLookupService, LegacyBranchLookupService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAfipPadronLookupService, LegacyAfipPadronLookupService>();
        services.AddScoped<IBrandCommandService, LegacyBrandCommandService>();
        services.AddScoped<IBrandQueryService, LegacyBrandQueryService>();
        services.AddScoped<IMovementCommandService, LegacyMovementCommandService>();
        services.AddScoped<IMovementProductLookupService, LegacyMovementProductLookupService>();
        services.AddScoped<IMovementQueryService, LegacyMovementQueryService>();
        services.AddScoped<IPurchaseQueryService, LegacyPurchaseQueryService>();
        services.AddScoped<IPurchaseCommandService, LegacyPurchaseCommandService>();
        services.AddScoped<IPersonCommandService, LegacyPersonCommandService>();
        services.AddScoped<IPersonQueryService, LegacyPersonQueryService>();
        services.AddScoped<IStockCommandService, LegacyStockCommandService>();
        services.AddScoped<IStockQueryService, LegacyStockQueryService>();
        services.AddScoped<IUserCommandService, LegacyUserCommandService>();
        services.AddScoped<IUserPermissionCommandService, LegacyUserPermissionCommandService>();
        services.AddScoped<IProductQueryService, LegacyProductQueryService>();
        services.AddScoped<IProductTypeCommandService, LegacyProductTypeCommandService>();
        services.AddScoped<IUserQueryService, LegacyUserQueryService>();
        services.AddScoped<IUserSessionAccessor, HttpContextUserSessionAccessor>();

        return services;
    }
}
