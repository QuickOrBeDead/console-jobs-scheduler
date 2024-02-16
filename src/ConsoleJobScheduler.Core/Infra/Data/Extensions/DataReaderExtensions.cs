using System.Data.Common;

namespace ConsoleJobScheduler.Core.Infra.Data.Extensions;

public static class DataReaderExtensions
{
    /// <summary>
    /// Returns string from given column name, or null if DbNull.
    /// </summary>
    public static string? GetNullableString(this DbDataReader reader, string columnName)
    {
        object columnValue = reader[columnName];
        if (columnValue == DBNull.Value)
        {
            return null;
        }

        return (string)columnValue;
    }
}