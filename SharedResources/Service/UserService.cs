namespace DataAccess.Service
{
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;
    using DataAccess.Service.AdminDashboard.Interfaces;
    using DataAccess.Service.Authentication.Interfaces;
    using IRepository;

    public class UserService : IUserService
    {
        private IUserRepository userRepository;
        public static Guid CurrentSessionId { private get; set; }

        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<User?> GetUserById(Guid userId)
        {
            try
            {
                return await this.userRepository.GetUserById(userId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            try
            {
                return await this.userRepository.GetUserByUsername(username);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<User>> GetAllUsers()
        {
            try
            {
                return await userRepository.GetAllUsers();
            }
            catch
            {
                return new List<User>();
            }
        }

        public async Task<List<User>> GetActiveUsersByRoleType(RoleType roleType)
        {
            try
            {
                if (roleType > 0)
                {
                    return await this.userRepository.GetUsersByRoleType(roleType);
                }
                else
                {
                    return new List<User>();
                }
            }
            catch
            {
                return new List<User>();
            }
        }

        public async Task<List<User>> GetBannedUsers()
        {
            try
            {
                return await this.userRepository.GetUsersByRoleType(RoleType.Banned);
            }
            catch
            {
                return new List<User>();
            }
        }

        public async Task<List<User>> GetUsersByRoleType(RoleType roleType)
        {
            try
            {
                return await this.userRepository.GetUsersByRoleType(roleType);
            }
            catch
            {
                return new List<User>();
            }
        }

        public async Task<List<User>> GetBannedUsersWhoHaveSubmittedAppeals()
        {
            try
            {
                return await this.userRepository.GetBannedUsersWhoHaveSubmittedAppeals();
            }
            catch
            {
                return new List<User>();
            }
        }

        public async Task<RoleType?> GetHighestRoleTypeForUser(Guid userId)
        {
            try
            {
                return await this.userRepository.GetRoleTypeForUser(userId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<User>> GetAdminUsers()
        {
            try
            {
                return await this.userRepository.GetUsersByRoleType(RoleType.Admin);
            }
            catch
            {
                return new List<User>();
            }
        }

        public async Task<List<User>> GetRegularUsers()
        {
            try
            {
                return await this.userRepository.GetUsersByRoleType(RoleType.User);
            }
            catch
            {
                return new List<User>();
            }
        }

        public async Task UpdateUserRole(Guid userId, RoleType roleType)
        {
            try
            {
                User? user = await this.userRepository.GetUserById(userId);

                if (user == null)
                {
                    return;
                }

                user.AssignedRole = roleType;

                await this.userRepository.UpdateUser(user);
            }
            catch
            {
            }
        }

        public async Task UpdateUserAppleaed(User user, bool newValue)
        {
            try
            {
                user.HasSubmittedAppeal = newValue;
                await this.userRepository.UpdateUser(user);
            }
            catch
            {
            }
        }

        public async Task ChangeRoleToUser(Guid userId, Role role)
        {
            try
            {
                await this.userRepository.ChangeRoleToUser(userId, role);
            }
            catch
            {
            }
        }
        public async Task<bool> CreateUser(User user)
        {
            try
            {
                return await this.userRepository.CreateUser(user);
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> UpdateUser(User user)
        {
            if (user == null)
            {
                Console.WriteLine("User is null in UpdateUser method.");
                return false;
            }

            try
            {
                return await this.userRepository.UpdateUser(user);
            }
            catch
            {
                Console.WriteLine("An error occurred while updating the user.");
                return false;
            }
        }
        public async Task<List<User>> GetUsersWhoHaveSubmittedAppeals()
        {
            try
            {
                return await this.userRepository.GetUsersWhoHaveSubmittedAppeals();
            }
            catch
            {
                return new List<User>();
            }
        }
    }
}