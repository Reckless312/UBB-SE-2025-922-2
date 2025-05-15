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
    using Microsoft.EntityFrameworkCore;
    using Repository.Authentication;
    using Data;

    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _context;
        public UserRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetUsersWhoHaveSubmittedAppeals()
        {
            try
            {
                return await _context.Users
                    .Where(user => user.HasSubmittedAppeal)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to retrieve users who have submitted appeals.", ex);
            }
        }

        public async Task<List<User>> GetUsersByRoleType(RoleType roleType)
        {
            try
            {
                return await _context.Users
                    .Where(user => user.AssignedRole == roleType)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to retrieve users with role type '{roleType}'.", ex);
            }
        }

        public async Task<RoleType> GetRoleTypeForUser(Guid userId)
        {
            try
            {
                User user = await GetUserById(userId);
                return user.AssignedRole;
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

        public async Task<List<User>> GetBannedUsersWhoHaveSubmittedAppeals()
        {
             return await _context.Users
                .Where(user => user.HasSubmittedAppeal && user.AssignedRole == RoleType.Banned).ToListAsync();
           
        }

        public async Task ChangeRoleToUser(Guid userId, Role roleToAdd)
        {
            User user = await GetUserById(userId);
            user.AssignedRole = roleToAdd.RoleType;
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public async Task<List<User>> GetAllUsers()
        {
            try
            {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to retrieve all users.", ex);
            }
        }

        public class RepositoryException : Exception
        {
            public RepositoryException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }

        public virtual async Task<User?> GetUserById(Guid userId)
        {
            return _context.Users.Where(user => user.UserId == userId).First();
        }

        public virtual async Task<User?> GetUserByUsername(string username)
        {
                return _context.Users.Where(user => user.Username == username).First();
        }

        public async Task<bool> UpdateUser(User user)
        {
            try
            {
                _context.Users.Update(user);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to update user with ID {user.UserId}.", ex);
            }
        }
        public async Task<bool> DeleteUser(Guid userId)
        {
            try
            {
                User? user = await GetUserById(userId);
                if (user == null)
                {
                    throw new ArgumentException($"No user found with ID {userId}");
                }

                _context.Users.Remove(user);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to delete user with ID {userId}.", ex);
            }
        }

        public async Task<bool> CreateUser(User user)
        {
            _context.Users.Add(user);
            return _context.SaveChangesAsync().Result > 0;
            
        }

        //public virtual async Task<bool> ValidateAction(Guid userId, string resource, string action)
        //{
        //    bool result = false;
        //    string sql = "SELECT dbo.fnValidateAction(@userId, @resource, @action)";

        //    using (SqlConnection connection = DrinkDbConnectionHelper.GetConnection())
        //    using (SqlCommand command = new(sql, connection))
        //    {
        //        command.Parameters.AddWithValue("@userId", userId);
        //        command.Parameters.AddWithValue("@resource", resource);
        //        command.Parameters.AddWithValue("@action", action);

        //        connection.Open();
        //        var scalarResult = command.ExecuteScalar();
        //        if (scalarResult != null && scalarResult != DBNull.Value)
        //        {
        //            result = Convert.ToBoolean(scalarResult);
        //        }
        //    }

        //    return result;
        //}
    }
}