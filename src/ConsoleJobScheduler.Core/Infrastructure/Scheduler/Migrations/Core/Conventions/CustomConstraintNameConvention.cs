using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Conventions;

namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler.Migrations.Core.Conventions;

public sealed class CustomConstraintNameConvention : IConstraintConvention
{
    public IConstraintExpression Apply(IConstraintExpression expression)
    {
        if (string.IsNullOrEmpty(expression.Constraint.ConstraintName))
        {
            expression.Constraint.ConstraintName = GetConstraintName(expression.Constraint);
        }

        return expression;
    }

    private static string GetConstraintName(ConstraintDefinition expression)
    {
        var sb = new StringBuilder();
        sb.Append(expression.TableName);
        sb.Append(expression.IsPrimaryKeyConstraint ? "_pkey" : "_uc");

        foreach (var column in expression.Columns)
        {
            sb.Append('_').Append(column);
        }
        return sb.ToString();
    }
}