namespace App1.Infrastructure
{
    using System;
    using System.Data;
    using Microsoft.Data.SqlClient;

    public interface IDbConnectionFactory
    {
        SqlConnection CreateConnection();
    }

    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(this.connectionString);
        }
    }
}