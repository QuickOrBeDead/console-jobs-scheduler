namespace ConsoleJobScheduler.Service.Infrastructure.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class IdentityManagementDbContextFactory : IDesignTimeDbContextFactory<IdentityManagementDbContext>
{
    public IdentityManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityManagementDbContext>();
        optionsBuilder.UseNpgsql("User ID=quartz;Password=quartz;Host=localhost;Port=5432;Database=quartz;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=105;");

        return new IdentityManagementDbContext(optionsBuilder.Options);
    }
}