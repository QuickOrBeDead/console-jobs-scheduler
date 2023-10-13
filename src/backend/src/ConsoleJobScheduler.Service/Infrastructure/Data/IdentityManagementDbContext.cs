namespace ConsoleJobScheduler.Service.Infrastructure.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class IdentityManagementDbContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
{
    public IdentityManagementDbContext(DbContextOptions<IdentityManagementDbContext> options) : base(options)
    {
    }
}


public class IdentityManagementDbContextFactory : IDesignTimeDbContextFactory<IdentityManagementDbContext>
{
    public IdentityManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityManagementDbContext>();
        optionsBuilder.UseNpgsql("User ID=quartz;Password=quartz;Host=localhost;Port=5432;Database=quartz;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=105;");

        return new IdentityManagementDbContext(optionsBuilder.Options);
    }
}