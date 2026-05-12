using F1Net.Application;
using F1Net.Application.Anomalies.Commands;
using F1Net.Auth;
using F1Net.Auth.Controllers;
using F1Net.Infrastructure;
using F1Net.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/f1net-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));

builder.Services
    .AddF1NetApplication()
    .AddF1NetInfrastructure(builder.Configuration)
    .AddF1NetAuth(builder.Configuration);

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(AuthorizationController).Assembly);

builder.Services.AddRazorPages();

builder.Services.AddAuthorization();

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

    var sessionsToBackfill = await db.Sessions
        .Where(s => s.Laps.Any())
        .Select(s => s.Id)
        .ToListAsync();

    if (sessionsToBackfill.Count > 0)
    {
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var log = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        log.LogInformation("Anomaly backfill: rebuilding flags for {Count} session(s)", sessionsToBackfill.Count);
        foreach (var sid in sessionsToBackfill)
        {
            try { await mediator.Send(new DetectSessionAnomaliesCommand(sid)); }
            catch (Exception ex) { log.LogWarning(ex, "Backfill detection failed for session {Sid}", sid); }
        }
        log.LogInformation("Anomaly backfill complete");
    }
}

app.Run();
