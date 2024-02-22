using System.Diagnostics.CodeAnalysis;
using ConsoleJobScheduler.Core.Infra.Migration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConsoleJobScheduler.Core.Domain.Settings.Infra;

[ExcludeFromCodeCoverage]
public sealed class SettingsDbContextFactory : IDesignTimeDbContextFactory<SettingsDbContext>
{
    public SettingsDbContext CreateDbContext(string[] args)
    {
        var arguments = new ArgumentsReader(args);

        var optionsBuilder = new DbContextOptionsBuilder<SettingsDbContext>();
        optionsBuilder.UseNpgsql(arguments.GetValue("connection"));

        return new SettingsDbContext(optionsBuilder.Options);
    }
}