using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Data.SqlClient;
using DrinkDb_Auth.Model;

namespace DrinkDb_Auth.Database
{
    public class RoleAdapter : IRoleAdapter
    {
        private readonly string databaseConnectionString;

        public RoleAdapter()
        {
            databaseConnectionString = ConfigurationManager.ConnectionStrings["DrinkDbConnection"].ConnectionString;
        }

        public void InsertRole(Roles role)
        {
            const string insertQuery = "INSERT INTO Roles (roleId, roleName, roleDescription) VALUES (@RoleId, @RoleName, @RoleDescription)";

            using (SqlConnection databaseConnection = new (databaseConnectionString))
            using (SqlCommand insertCommand = new (insertQuery, databaseConnection))
            {
                insertCommand.Parameters.AddWithValue("@RoleId", role.RoleIdentifier);
                insertCommand.Parameters.AddWithValue("@RoleName", role.RoleName);
                insertCommand.Parameters.AddWithValue("@RoleDescription", role.RoleDescription);
                databaseConnection.Open();
                insertCommand.ExecuteNonQuery();
            }
        }

        public void UpdateRole(Roles role)
        {
            const string updateQuery = "UPDATE Roles SET roleName = @RoleName, roleDescription = @RoleDescription WHERE roleId = @RoleId";

            using (SqlConnection databaseConnection = new (databaseConnectionString))
            using (SqlCommand updateCommand = new (updateQuery, databaseConnection))
            {
                updateCommand.Parameters.AddWithValue("@RoleId", role.RoleIdentifier);
                updateCommand.Parameters.AddWithValue("@RoleName", role.RoleName);
                updateCommand.Parameters.AddWithValue("@RoleDescription", role.RoleDescription);
                databaseConnection.Open();
                updateCommand.ExecuteNonQuery();
            }
        }

        public void DeleteRoleById(Roles role)
        {
            const string deleteQuery = "DELETE FROM Roles WHERE roleId = @RoleId";

            using (SqlConnection databaseConnection = new (databaseConnectionString))
            using (SqlCommand deleteCommand = new (deleteQuery, databaseConnection))
            {
                deleteCommand.Parameters.AddWithValue("@RoleId", role.RoleIdentifier);
                databaseConnection.Open();
                deleteCommand.ExecuteNonQuery();
            }
        }

        public Roles GetRoleById(Guid roleIdentifier)
        {
            const string selectQuery = "SELECT roleId, roleName, roleDescription FROM Roles WHERE roleId = @RoleId";

            using (SqlConnection databaseConnection = new (databaseConnectionString))
            using (SqlCommand selectCommand = new (selectQuery, databaseConnection))
            {
                selectCommand.Parameters.AddWithValue("@RoleId", roleIdentifier);
                databaseConnection.Open();

                using (SqlDataReader dataReader = selectCommand.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        return new Roles
                        {
                            RoleIdentifier = dataReader.GetGuid(0),
                            RoleName = dataReader.GetString(1),
                            RoleDescription = dataReader.GetString(2)
                        };
                    }
                }
            }

            throw new Exception($"Role with ID {roleIdentifier} not found.");
        }

        public List<Roles> GetAllRoles()
        {
            List<Roles> rolesList = new ();

            const string selectAllQuery = "SELECT roleId, roleName, roleDescription FROM Roles";

            using (SqlConnection databaseConnection = new (databaseConnectionString))
            using (SqlCommand selectCommand = new (selectAllQuery, databaseConnection))
            {
                databaseConnection.Open();

                using (SqlDataReader dataReader = selectCommand.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        Roles role = new ()
                        {
                            RoleIdentifier = dataReader.GetGuid(0),
                            RoleName = dataReader.GetString(1),
                            RoleDescription = dataReader.GetString(2)
                        };

                        rolesList.Add(role);
                    }
                }
            }

            return rolesList;
        }
    }
}
