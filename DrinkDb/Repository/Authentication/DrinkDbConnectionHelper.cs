using System;
using System.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Protocols;
using Windows.Security.Cryptography.Certificates;

namespace DrinkDb_Auth.Repository.Authentication
{
    public static class DrinkDbConnectionHelper
    {
        /// <summary>
        /// Reads the DrinkDbConnection string from App.config, opens a SqlConnection, and returns it.
        /// </summary>
        /// <returns>An open SqlConnection object.</returns>
        public static SqlConnection GetConnection()
        {
            // Get the connection string from App.config
            string connectionString = "Data Source=CORA\\MSSQLSERVER01; Initial Catalog = DrinkDB_Dev; Integrated Security = True; Trust Server Certificate = True";

            // Create and open a new SqlConnection
            SqlConnection connection = new (connectionString);
            connection.Open();
            return connection;
        }
    }
}
