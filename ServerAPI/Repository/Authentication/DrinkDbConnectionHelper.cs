using Microsoft.Data.SqlClient;

namespace Repository.Authentication
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
            string connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=DrinkDB_Dev;User Id=sa;Password=yourStrong(!)Password;Trust Server Certificate=True";

            // Create and open a new SqlConnection
            SqlConnection connection = new (connectionString);
            connection.Open();
            return connection;
        }
    }
}
