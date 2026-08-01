using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;  // ← MUST HAVE THIS
using AspNetCoreStarterKit.Application.Interfaces;
using AspNetCoreStarterKit.Domain.Interfaces;
using AspNetCoreStarterKit.Infrastructure.Identity;
using AspNetCoreStarterKit.Infrastructure.Persistence;
using AspNetCoreStarterKit.Infrastructure.Persistence.Repositories;
using AspNetCoreStarterKit.Infrastructure.Services;

namespace AspNetCoreStarterKit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        //Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IBulkUploadService, ExcelBulkUploadService>();
        services.AddScoped<IExcelTemplateService, ExcelTemplateService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddHttpContextAccessor();  // This now works
        services.AddScoped<IFileStorageService, LocalFileStorageService>();



        // Auth
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddHttpContextAccessor();

        return services;
    }
}