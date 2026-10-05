using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OmniCart.Application.Common.Interfaces;
using OmniCart.Infrustructure.Persistence;
using OmniCart.Infrustructure.Services;

namespace OmniCart.Infrustructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // ১. PostgreSQL + pgvector ডাটাবেজ ও ApplicationDbContext রেজিস্টার করা
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.UseVector(); // AI pgvector সাপোর্ট এনেবল করা
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            });
        });

        // ২. IApplicationDbContext ইন্টারফেসকে ApplicationDbContext-এর সাথে বাইন্ড করা
        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        // ৩. CurrentTenantService স্কোপড লাইফসাইকেলে যুক্ত করা
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();

        // ৪. পাসওয়ার্ড হ্যাশার ও অথেন্টিকেশন সার্ভিস রেজিস্টার করা
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthService, AuthService>();

        // 5. Redis Cache Engine Setup

        services.AddStackExchangeRedisCache(options =>  
        {
            options.Configuration = configuration.GetConnectionString("Radis") ?? "localhost:6379";
            options.InstanceName = "OmniCart_";
        });

        // 5. Register ICacheService
        services.AddScoped<ICacheService,CacheService>();

        return services;
    }
}
