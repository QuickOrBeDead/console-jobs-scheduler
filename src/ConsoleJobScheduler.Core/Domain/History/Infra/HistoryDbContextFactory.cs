using System.Diagnostics.CodeAnalysis;
using ConsoleJobScheduler.Core.Infra.Migration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConsoleJobScheduler.Core.Domain.History.Infra;

[ExcludeFromCodeCoverage]
public sealed class HistoryDbContextFactory : IDesignTimeDbContextFactory<HistoryDbContext>
{
    public HistoryDbContext CreateDbContext(string[] args)
    {
        var arguments = new ArgumentsReader(args);

        var optionsBuilder = new DbContextOptionsBuilder<HistoryDbContext>();
        optionsBuilder.UseNpgsql(arguments.GetValue("connection"));

        return new HistoryDbContext(optionsBuilder.Options);
    }
}