using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using F1Net.Infrastructure.Persistence;
using OpenIddict.Abstractions;

namespace F1Net.Auth;

internal sealed class OpenIddictClientSeeder : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly AuthOptions _options;

    public OpenIddictClientSeeder(IServiceProvider services, IOptions<AuthOptions> options)
    {
        _services = services;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<F1NetDbContext>();
        await db.Database.EnsureCreatedAsync(cancellationToken);

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync(_options.Sync.ClientId, cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = _options.Sync.ClientId,
                ClientSecret = _options.Sync.ClientSecret ?? "change-me-in-user-secrets",
                DisplayName = _options.Sync.DisplayName,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "f1net.sync"
                }
            }, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
