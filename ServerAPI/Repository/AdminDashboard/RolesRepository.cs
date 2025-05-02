namespace Repository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using IRepository;

    public class RolesRepository : IRolesRepository
    {
        private readonly List<Role> roles;

        public RolesRepository()
        {
            roles = new List<Role>();

            roles.Add(new Role(RoleType.Banned, "Banned"));
            roles.Add(new Role(RoleType.User, "User"));
            roles.Add(new Role(RoleType.Admin, "Admin"));
            roles.Add(new Role(RoleType.Manager, "Manager"));
        }

        public async Task<List<Role>> GetAllRoles()
        {
            return roles;
        }

        public async Task<Role> GetNextRoleInHierarchy(RoleType currentRoleType)
        {
            try
            {
                Role nextRole = roles.First(role => role.RoleType == currentRoleType + 1);
                return nextRole;
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException($"No next role exists for {currentRoleType}");
            }
        }
    }
}