using F1Net.Infrastructure.Identity;
using F1Net.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;

namespace F1Net.Auth;

public static class DependencyInjection
{
    public static IServiceCollection AddF1NetAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
        var auth = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();

        services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
            {
                opt.Password.RequiredLength = 10;
                opt.SignIn.RequireConfirmedAccount = false;
                opt.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<F1NetDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
        });

        var authBuilder = services.AddAuthentication(opt =>
        {
            opt.DefaultScheme = IdentityConstants.ApplicationScheme;
            opt.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
        });

        if (!string.IsNullOrWhiteSpace(auth.Google.ClientId) &&
            !string.IsNullOrWhiteSpace(auth.Google.ClientSecret))
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = auth.Google.ClientId!;
                options.ClientSecret = auth.Google.ClientSecret!;
                options.SignInScheme = IdentityConstants.ExternalScheme;
            });
        }

        services.ConfigureApplicationCookie(opt =>
        {
            opt.LoginPath = "/Identity/Account/Login";
            opt.AccessDeniedPath = "/Identity/Account/AccessDenied";
            opt.ExpireTimeSpan = TimeSpan.FromHours(8);
            opt.SlidingExpiration = true;
        });

        services.AddOpenIddict()
            .AddCore(opt =>
            {
                opt.UseEntityFrameworkCore().UseDbContext<F1NetDbContext>();
            })
            .AddServer(opt =>
            {
                opt.SetAuthorizationEndpointUris("connect/authorize")
                   .SetTokenEndpointUris("connect/token")
                   .SetUserinfoEndpointUris("connect/userinfo")
                   .SetLogoutEndpointUris("connect/logout");

                opt.AllowAuthorizationCodeFlow()
                   .RequireProofKeyForCodeExchange()
                   .AllowRefreshTokenFlow()
                   .AllowClientCredentialsFlow();

                opt.RegisterScopes(
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles,
                    "f1net.api",
                    "f1net.sync");

                opt.AddDevelopmentEncryptionCertificate()
                   .AddDevelopmentSigningCertificate();

                opt.UseAspNetCore()
                   .EnableAuthorizationEndpointPassthrough()
                   .EnableTokenEndpointPassthrough()
                   .EnableUserinfoEndpointPassthrough()
                   .EnableLogoutEndpointPassthrough()
                   .DisableTransportSecurityRequirement();
            })
            .AddValidation(opt =>
            {
                opt.UseLocalServer();
                opt.UseAspNetCore();
            });

        services.AddHostedService<OpenIddictClientSeeder>();

        return services;
    }
}
