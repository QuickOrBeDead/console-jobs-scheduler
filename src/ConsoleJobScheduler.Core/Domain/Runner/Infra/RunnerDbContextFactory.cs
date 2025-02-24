using System.Diagnostics.CodeAnalysis;
using ConsoleJobScheduler.Core.Infra.Migration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConsoleJobScheduler.Core.Domain.Runner.Infra
{
    [ExcludeFromCodeCoverage]
    public sealed class RunnerDbContextFactory : IDesignTimeDbContextFactory<RunnerDbContext>
    {
        public RunnerDbContext CreateDbContext(string[] args)
        {
            var arguments = new ArgumentsReader(args);

            var optionsBuilder = new DbContextOptionsBuilder<RunnerDbContext>();
            optionsBuilder.UseNpgsql(arguments.GetValue("connection"));

            return new RunnerDbContext(optionsBuilder.Options);
        }
    }
}