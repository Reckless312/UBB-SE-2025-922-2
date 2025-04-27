using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Data.SqlClient;
using DrinkDb_Auth.Model;

namespace DrinkDb_Auth.Adapter
{
    public class PermissionAdapter : IPermissionAdapter
    {
        private readonly string connectionString;

        // TODO delete constructor since it has 0 references
        public PermissionAdapter()
        {
            connectionString = "Data Source=CORA\\MSSQLSERVER01; Initial Catalog = DrinkDB_Dev; Integrated Security = True; Trust Server Certificate = True";
        }

        public void CreatePermission(Permission permission)
        {
            string query = @"INSERT INTO Permissions (permissionName, resource, action)
                             VALUES (@PermissionName, @Role, @Action)";
            using (SqlConnection sqlConnection = new (connectionString))
            using (SqlCommand sqlCommand = new (query, sqlConnection))
            {
                sqlCommand.Parameters.AddWithValue("@PermissionName", permission.PermissionName);
                sqlCommand.Parameters.AddWithValue("@Role", permission.Resource);
                sqlCommand.Parameters.AddWithValue("@Action", permission.Action);
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
            }
        }

        public void UpdatePermission(Permission permission)
        {
            string query = @"UPDATE Permissions 
                             SET permissionName=@PermissionName, resource=@Role, action=@Action 
                             WHERE permissionId=@Id";
            using (SqlConnection sqlConnection = new (connectionString))
            using (SqlCommand sqlCommand = new (query, sqlConnection))
            {
                sqlCommand.Parameters.AddWithValue("@PermissionName", permission.PermissionName);
                sqlCommand.Parameters.AddWithValue("@Role", permission.Resource);
                sqlCommand.Parameters.AddWithValue("@Action", permission.Action);
                sqlCommand.Parameters.AddWithValue("@Id", permission.PermissionId);
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
            }
        }

        public void DeletePermission(Permission permission)
        {
            string query = "DELETE FROM Permissions WHERE permissionId=@Id";
            using (SqlConnection sqlConnection = new (connectionString))
            using (SqlCommand sqlCommand = new (query, sqlConnection))
            {
                sqlCommand.Parameters.AddWithValue("@Id", permission.PermissionId);
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
            }
        }

        public Permission GetPermissionById(Guid id)
        {
            string query = "SELECT permissionId, permissionName, resource, action FROM Permissions WHERE permissionId=@Id";
            using (SqlConnection sqlConnection = new (connectionString))
            using (SqlCommand sqlCommand = new (query, sqlConnection))
            {
                sqlCommand.Parameters.AddWithValue("@Id", id);
                sqlConnection.Open();
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Permission
                        {
                            PermissionId = reader.GetGuid(0),
                            PermissionName = reader.GetString(1),
                            Resource = reader.GetString(2),
                            Action = reader.GetString(3)
                        };
                    }
                }
            }
            throw new Exception("Permission not found");
        }

        public List<Permission> GetPermissions()
        {
            List<Permission> permissions = new ();
            string query = "SELECT permissionId, permissionName, resource, action FROM Permissions";
            using (SqlConnection sqlConnection = new (connectionString))
            using (SqlCommand sqlCommand = new (query, sqlConnection))
            {
                sqlConnection.Open();
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Permission permission = new ()
                        {
                            PermissionId = reader.GetGuid(0),
                            PermissionName = reader.GetString(1),
                            Resource = reader.GetString(2),
                            Action = reader.GetString(3)
                        };
                        permissions.Add(permission);
                    }
                }
            }
            return permissions;
        }
    }
}
