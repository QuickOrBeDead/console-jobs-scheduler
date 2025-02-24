using FluentMigrator.Expressions;
using FluentMigrator.Runner.Conventions;

namespace ConsoleJobScheduler.Core.Infra.Migration.Conventions
{
    public sealed class CustomPrimaryKeyNameConvention : IColumnsConvention
    {
        public IColumnsExpression Apply(IColumnsExpression expression)
        {
            foreach (var columnDefinition in expression.Columns)
            {
                if (columnDefinition.IsPrimaryKey && string.IsNullOrEmpty(columnDefinition.PrimaryKeyName))
                {
                    var tableName = string.IsNullOrEmpty(columnDefinition.TableName)
                        ? expression.TableName
                        : columnDefinition.TableName;
                    columnDefinition.PrimaryKeyName = GetPrimaryKeyName(tableName);
                }
            }

            return expression;
        }

        private static string GetPrimaryKeyName(string tableName)
        {
            return $"{tableName}_pkey";
        }
    }
}