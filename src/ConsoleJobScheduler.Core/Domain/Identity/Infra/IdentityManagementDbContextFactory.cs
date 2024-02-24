using System.Diagnostics.CodeAnalysis;
using ConsoleJobScheduler.Core.Infra.Migration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConsoleJobScheduler.Core.Domain.Identity.Infra;

[ExcludeFromCodeCoverage]
public sealed class IdentityManagementDbContextFactory : IDesignTimeDbContextFactory<IdentityManagementDbContext>
{
    public IdentityManagementDbContext CreateDbContext(string[] args)
    {
        var arguments = new ArgumentsReader(args);

        var optionsBuilder = new DbContextOptionsBuilder<IdentityManagementDbContext>();
        optionsBuilder.UseNpgsql(arguments.GetValue("connection"));

        return new IdentityManagementDbContext(optionsBuilder.Options);
    }
}