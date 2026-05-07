using F1Net.Application;
using F1Net.Auth;
using F1Net.Auth.Controllers;
using F1Net.Infrastructure;
using F1Net.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

builder.Services
    .AddF1NetApplication()
    .AddF1NetInfrastructure(builder.Configuration)
    .AddF1NetAuth(builder.Configuration);

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(AuthorizationController).Assembly);

var fallbackPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

builder.Services.AddRazorPages(opt =>
{
    opt.Conventions.AuthorizeFolder("/");
    opt.Conventions.AllowAnonymousToAreaFolder("Identity", "/Account");
});

builder.Services.AddAuthorization(opt =>
{
    opt.FallbackPolicy = fallbackPolicy;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<F1NetDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
