using Npgsql;

namespace ConsoleJobScheduler.Core.Tests.Infrastructure.Scheduler.Migrations;

public static class PostgresDatabase
{
    public sealed class PostgresOptions
    {
        public string Host { get; init; } = null!;
        
        public int Port { get; init; }
        
        public string User { get; init; } = null!;
        
        public string Password { get; init; } = null!;

        public string Db { get; init; } = null!;

        public string GetConnectionString()
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = Host,
                Port = Port,
                Username = User,
                Password = Password,
                Database = Db
            };

            return builder.ConnectionString;
        }
        
        public string GetDbCreateConnectionString()
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = Host,
                Port = Port,
                Username = User,
                Password = Password
            };

            return builder.ConnectionString;
        }
    }
    
    public static void Create(PostgresOptions options)
    {
        Delete(options);
        
        using var conn = new NpgsqlConnection(options.GetDbCreateConnectionString());
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"CREATE DATABASE \"{options.Db}\" WITH OWNER = {options.User} ENCODING = 'UTF8' CONNECTION LIMIT = -1;";
        cmd.ExecuteNonQuery();
    }

    private static void Delete(PostgresOptions options)
    {
        using var conn = new NpgsqlConnection(options.GetDbCreateConnectionString());
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"DROP DATABASE IF EXISTS \"{options.Db}\" WITH (FORCE);";
        cmd.ExecuteNonQuery();
    }
}