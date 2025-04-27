using System;
using System.Collections.Generic;
using DrinkDb_Auth.Model;
using Microsoft.Data.SqlClient;

namespace DrinkDb_Auth.Adapter
{
    public class UserAdapter : IUserAdapter
    {
        public virtual User? GetUserById(Guid userId)
        {
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            string sql = "SELECT * FROM Users WHERE userId = @userId;";
            using SqlCommand command = new (sql, connection);
            command.Parameters.AddWithValue("@userId", userId);
            using SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new User
                {
                    UserId = reader.GetGuid(reader.GetOrdinal("userId")),
                    Username = reader.IsDBNull(reader.GetOrdinal("userName")) ? string.Empty : reader.GetString(reader.GetOrdinal("userName")),
                    PasswordHash = reader.IsDBNull(reader.GetOrdinal("passwordHash")) ? string.Empty : reader.GetString(reader.GetOrdinal("passwordHash")),
                    TwoFASecret = reader.IsDBNull(reader.GetOrdinal("twoFASecret")) ? null : reader.GetString(reader.GetOrdinal("twoFASecret")),
                };
            }
            return null;
        }

        public virtual User? GetUserByUsername(string username)
        {
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            string sql = "SELECT * FROM Users WHERE userName = @username;";
            using SqlCommand command = new (sql, connection);
            command.Parameters.AddWithValue("@username", username);
            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserId = reader.GetGuid(reader.GetOrdinal("userId")),
                    Username = reader.IsDBNull(reader.GetOrdinal("userName")) ? string.Empty : reader.GetString(reader.GetOrdinal("userName")),
                    PasswordHash = reader.IsDBNull(reader.GetOrdinal("passwordHash")) ? string.Empty : reader.GetString(reader.GetOrdinal("passwordHash")),
                    TwoFASecret = reader.IsDBNull(reader.GetOrdinal("twoFASecret")) ? null : reader.GetString(reader.GetOrdinal("twoFASecret")),
                };
            }
            return null;
        }

        public bool UpdateUser(User user)
        {
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            string sql = "UPDATE Users SET userName = @username, passwordHash = @passwordHash, twoFASecret = @twoFASecret WHERE userId = @userId;";
            using (SqlCommand command = new (sql, connection))
            {
                command.Parameters.AddWithValue("@userId", user.UserId);
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@passwordHash", (object?)user.PasswordHash ?? DBNull.Value);
                command.Parameters.AddWithValue("@twoFASecret", (object?)user.TwoFASecret ?? DBNull.Value);
                return command.ExecuteNonQuery() > 0;
            }
        }
        public bool DeleteUser(Guid userId)
        {
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            string sql = "DELETE FROM Users WHERE userId = @userId;";
            using SqlCommand command = new (sql, connection);
            command.Parameters.AddWithValue("@userId", userId);
            return command.ExecuteNonQuery() > 0;
        }

        public bool CreateUser(User user)
        {
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            string sql = "INSERT INTO Users (userId, userName, passwordHash, twoFASecret) VALUES (@userId, @username, @passwordHash, @twoFASecret);";
            using SqlCommand command = new (sql, connection);
            command.Parameters.AddWithValue("@userId", user.UserId);
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@passwordHash", (object?)user.PasswordHash ?? DBNull.Value);
            command.Parameters.AddWithValue("@twoFASecret", (object?)user.TwoFASecret ?? DBNull.Value);
            return command.ExecuteNonQuery() > 0;
        }

        private List<Permission> GetPermissionsForUser(Guid userId)
        {
            List<Permission> permissions = new ();

            // SQL query joining User -> UserRoles -> Roles -> RolePermissions -> Permissions
            string sql = @"
        SELECT p.permissionId, p.permissionName, p.resource, p.action
        FROM Users u
        JOIN UserRoles ur ON u.userId = ur.userId
        JOIN Roles r ON ur.roleId = r.roleId
        JOIN RolePermissions rp ON r.roleId = rp.roleId
        JOIN Permissions p ON rp.permissionId = p.permissionId
        WHERE u.userId = @userId;
    ";

            using (SqlConnection connection = DrinkDbConnectionHelper.GetConnection())
            using (SqlCommand command = new (sql, connection))
            {
                command.Parameters.AddWithValue("@userId", userId);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Permission permission = new ()
                        {
                            PermissionId = reader.GetGuid(reader.GetOrdinal("permissionId")),
                            PermissionName = reader.GetString(reader.GetOrdinal("permissionName")),
                            Resource = reader.GetString(reader.GetOrdinal("resource")),
                            Action = reader.GetString(reader.GetOrdinal("action"))
                        };
                        permissions.Add(permission);
                    }
                }
            }

            return permissions;
        }

        public virtual bool ValidateAction(Guid userId, string resource, string action)
        {
            bool result = false;
            string sql = "SELECT dbo.fnValidateAction(@userId, @resource, @action)";

            using (SqlConnection connection = DrinkDbConnectionHelper.GetConnection())
            using (SqlCommand command = new (sql, connection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@resource", resource);
                command.Parameters.AddWithValue("@action", action);

                connection.Open();
                var scalarResult = command.ExecuteScalar();
                if (scalarResult != null && scalarResult != DBNull.Value)
                {
                    result = Convert.ToBoolean(scalarResult);
                }
            }

            return result;
        }
    }
}
