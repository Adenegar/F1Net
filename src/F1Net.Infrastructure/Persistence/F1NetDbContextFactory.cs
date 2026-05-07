using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace F1Net.Infrastructure.Persistence;

public class F1NetDbContextFactory : IDesignTimeDbContextFactory<F1NetDbContext>
{
    public F1NetDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("F1NET_CONNECTION")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=F1Net;Trusted_Connection=True;MultipleActiveResultSets=true";
        var options = new DbContextOptionsBuilder<F1NetDbContext>()
            .UseSqlServer(cs, o => o.MigrationsAssembly(typeof(F1NetDbContext).Assembly.FullName))
            .Options;
        return new F1NetDbContext(options);
    }
}
