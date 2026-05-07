using F1Net.Application.Abstractions;
using F1Net.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace F1Net.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddF1NetInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("F1Net")
            ?? throw new InvalidOperationException("Missing connection string 'F1Net'.");

        services.AddDbContext<F1NetDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(F1NetDbContext).Assembly.FullName)));

        services.AddScoped<IF1NetDbContext>(sp => sp.GetRequiredService<F1NetDbContext>());

        return services;
    }
}
