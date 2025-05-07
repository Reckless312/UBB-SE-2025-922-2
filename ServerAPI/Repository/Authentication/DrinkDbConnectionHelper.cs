using Microsoft.Data.SqlClient;

namespace Repository.Authentication
{
    public static class DrinkDbConnectionHelper
    {
        public static SqlConnection connection = null;
        /// <summary>
        /// Reads the DrinkDbConnection string from App.config, opens a SqlConnection, and returns it.
        /// </summary>
        /// <returns>An open SqlConnection object.</returns>
        public static SqlConnection GetConnection()
        {
            if (connection == null)
            {
                string connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=DrinkDB_Dev;Integrated Security=True;Trust Server Certificate=True";
                SqlConnection connection = new(connectionString);
                connection.Open();
            }
            return connection;
        }
    }
}
