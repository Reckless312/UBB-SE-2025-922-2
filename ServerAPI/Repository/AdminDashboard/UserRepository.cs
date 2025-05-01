namespace Repository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;
    using IRepository;
    using Microsoft.Data.SqlClient;
    using Repository.Authentication;

    /// <summary>
    /// Repository for managing user data and operations.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        /// <summary>
        /// Static role collections for user initialization.
        /// </summary>
        public static List<Role> BasicUserRoles = new List<Role> { new Role(RoleType.User, "user") };

        public static List<Role> AdminRoles = new List<Role> { new Role(RoleType.User, "user"), new Role(RoleType.Admin, "admin") };

        public static List<Role> ManagerRoles = new List<Role> { new Role(RoleType.User, "user"), new Role(RoleType.Admin, "admin"), new Role(RoleType.Manager, "manager") };

        public static List<Role> BannedUserRoles = new List<Role> { new Role(RoleType.Banned, "banned") };

        /// <summary>
        /// Internal list of users managed by the repository.
        /// </summary>
        private readonly List<User> _usersList;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class with default user data.
        /// </summary>
        public UserRepository()
        {
            _usersList = new List<User>();
        }

        /// <summary>
        /// Retrieves all users who have submitted appeals.
        /// </summary>
        /// <returns>A list of users who have submitted appeals.</returns>
        /// <exception cref="RepositoryException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetUsersWhoHaveSubmittedAppeals()
        {
            try
            {
                if (_usersList == null)
                {
                    throw new NullReferenceException("_usersList is null.");
                }

                return _usersList.Where(user => user.HasSubmittedAppeal).ToList();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to retrieve users who have submitted appeals.", ex);
            }
        }

        /// <summary>
        /// Retrieves all users with a specific role type.
        /// </summary>
        /// <param name="roleType">The role type to filter users by.</param>
        /// <returns>A list of users with the specified role type.</returns>
        /// <exception cref="RepositoryException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetUsersByRoleType(RoleType roleType)
        {
            try
            {
                if (_usersList == null)
                {
                    throw new NullReferenceException("_usersList is null.");
                }

                return _usersList.Where(user => user.AssignedRoles != null && user.AssignedRoles.Any(role => role.RoleType == roleType)).ToList();
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to retrieve users with role type '{roleType}'.", ex);
            }
        }

        /// <summary>
        /// Retrieves the highest role type assigned to a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The highest role type assigned to the user.</returns>
        /// <exception cref="RepositoryException">Thrown when the user has no roles or does not exist.</exception>
        public RoleType GetHighestRoleTypeForUser(Guid userId)
        {
            try
            {
                User user = GetUserById(userId);
                if (user.AssignedRoles == null || !user.AssignedRoles.Any())
                {
                    throw new ArgumentException($"No roles found for user with ID {userId}");
                }

                return user.AssignedRoles.Max(role => role.RoleType);
            }
            catch (ArgumentException ex)
            {
                throw new RepositoryException("User has no roles assigned.", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to get highest role type for user with ID {userId}.", ex);
            }
        }

        /// <summary>
        /// Retrieves all banned users who have submitted appeals.
        /// </summary>
        /// <returns>A list of banned users who have submitted appeals.</returns>
        /// <exception cref="RepositoryException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetBannedUsersWhoHaveSubmittedAppeals()
        {
            try
            {
                if (_usersList == null)
                {
                    throw new NullReferenceException("_usersList is null.");
                }

                return _usersList.Where(user =>
                    user.HasSubmittedAppeal &&
                    user.AssignedRoles != null &&
                    user.AssignedRoles.Any(role => role.RoleType == RoleType.Banned)
                ).ToList();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to retrieve banned users who have submitted appeals.", ex);
            }
        }

        /// <summary>
        /// Adds a role to a user.
        /// </summary>
        /// <param name="userId">The ID of the user to add the role to.</param>
        /// <param name="roleToAdd">The role to add to the user.</param>
        /// <exception cref="RepositoryException">Thrown when the user does not exist or an error occurs.</exception>
        public void AddRoleToUser(Guid userId, Role roleToAdd)
        {
            try
            {
                User? user = _usersList.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    throw new ArgumentException($"No user found with ID {userId}");
                }

                user.AssignedRoles.Add(roleToAdd);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to add role to user with ID {userId}.", ex);
            }
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>A list of all users.</returns>
        /// <exception cref="RepositoryException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetAllUsers()
        {
            try
            {
                if (_usersList == null)
                {
                    throw new NullReferenceException("_usersList is null.");
                }

                return _usersList;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to retrieve all users.", ex);
            }
        }

        /// <summary>
        /// Exception class for repository-related errors.
        /// </summary>
        public class RepositoryException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RepositoryException"/> class.
            /// </summary>
            /// <param name="message">The error message.</param>
            /// <param name="innerException">The inner exception.</param>
            public RepositoryException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }

        public virtual User? GetUserById(Guid userId)
        {
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            string sql = "SELECT * FROM Users WHERE userId = @userId;";
            using SqlCommand command = new(sql, connection);
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
            using SqlCommand command = new(sql, connection);
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
            using (SqlCommand command = new(sql, connection))
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
            using SqlCommand command = new(sql, connection);
            command.Parameters.AddWithValue("@userId", userId);
            return command.ExecuteNonQuery() > 0;
        }

        public bool CreateUser(User user)
        {
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            string sql = "INSERT INTO Users (userId, userName, passwordHash, twoFASecret) VALUES (@userId, @username, @passwordHash, @twoFASecret);";
            using SqlCommand command = new(sql, connection);
            command.Parameters.AddWithValue("@userId", user.UserId);
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@passwordHash", (object?)user.PasswordHash ?? DBNull.Value);
            command.Parameters.AddWithValue("@twoFASecret", (object?)user.TwoFASecret ?? DBNull.Value);
            return command.ExecuteNonQuery() > 0;
        }

        public virtual bool ValidateAction(Guid userId, string resource, string action)
        {
            bool result = false;
            string sql = "SELECT dbo.fnValidateAction(@userId, @resource, @action)";

            using (SqlConnection connection = DrinkDbConnectionHelper.GetConnection())
            using (SqlCommand command = new(sql, connection))
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

        public void GetHighestRoleTypeForUser(int v)
        {
            throw new NotImplementedException();
        }

        public void AddRoleToUser(int v, Role role)
        {
            throw new NotImplementedException();
        }
    }
}