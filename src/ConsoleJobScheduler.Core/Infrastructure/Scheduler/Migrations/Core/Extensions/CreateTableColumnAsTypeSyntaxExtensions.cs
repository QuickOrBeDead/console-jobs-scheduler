using System.Data;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Model;

namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler.Migrations.Core.Extensions;

public static class CreateTableColumnAsTypeSyntaxExtensions
{
    public static ICreateTableColumnOptionOrWithColumnSyntax AsBytea(this ICreateTableColumnAsTypeSyntax column)
    {
        return column.AsColumnDataType(new ColumnDataType { Type = DbType.Binary, CustomType = "bytea" });
    }
}