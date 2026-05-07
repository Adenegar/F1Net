using F1Net.Application.Abstractions;
using F1Net.Infrastructure.BackgroundJobs;
using F1Net.Infrastructure.ExternalApis.Ergast;
using F1Net.Infrastructure.ExternalApis.OpenF1;
using F1Net.Infrastructure.Ml;
using F1Net.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

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
        {
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(F1NetDbContext).Assembly.FullName));
            options.UseOpenIddict();
        });
        services.AddScoped<IF1NetDbContext>(sp => sp.GetRequiredService<F1NetDbContext>());

        services.Configure<OpenF1Options>(configuration.GetSection(OpenF1Options.SectionName));
        services.Configure<ErgastOptions>(configuration.GetSection(ErgastOptions.SectionName));
        services.Configure<IngestionOptions>(configuration.GetSection(IngestionOptions.SectionName));

        services.AddHttpClient<IOpenF1Client, OpenF1Client>((sp, c) =>
            {
                var o = configuration.GetSection(OpenF1Options.SectionName).Get<OpenF1Options>() ?? new();
                c.BaseAddress = new Uri(o.BaseUrl);
                c.Timeout = TimeSpan.FromSeconds(30);
                if (!string.IsNullOrWhiteSpace(o.ApiKey))
                    c.DefaultRequestHeaders.Add("Authorization", $"Bearer {o.ApiKey}");
            })
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<IErgastClient, ErgastClient>((sp, c) =>
            {
                var o = configuration.GetSection(ErgastOptions.SectionName).Get<ErgastOptions>() ?? new();
                c.BaseAddress = new Uri(o.BaseUrl);
                c.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetRetryPolicy());

        services.AddSingleton<ILapAnomalyDetector, RandomizedPcaLapDetector>();
        services.AddHostedService<TelemetryIngestionService>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
}
