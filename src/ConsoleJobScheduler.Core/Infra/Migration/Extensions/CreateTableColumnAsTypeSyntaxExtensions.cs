using System.Data;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Model;

namespace ConsoleJobScheduler.Core.Infra.Migration.Extensions;

public static class CreateTableColumnAsTypeSyntaxExtensions
{
    public static ICreateTableColumnOptionOrWithColumnSyntax AsBytea(this ICreateTableColumnAsTypeSyntax column)
    {
        return column.AsColumnDataType(new ColumnDataType { Type = DbType.Binary, CustomType = "bytea" });
    }
}