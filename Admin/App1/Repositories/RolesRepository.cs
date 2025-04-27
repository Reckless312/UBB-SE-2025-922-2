namespace App1.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using App1.Models;

    public class RolesRepository : IRolesRepository
    {
        private readonly List<Role> roles;

        public RolesRepository()
        {
            this.roles = new List<Role>();

            this.roles.Add(new Role(RoleType.Banned, "Banned"));
            this.roles.Add(new Role(RoleType.User, "User"));
            this.roles.Add(new Role(RoleType.Admin, "Admin"));
            this.roles.Add(new Role(RoleType.Manager, "Manager"));
        }

        public List<Role> GetAllRoles()
        {
            return this.roles;
        }

        public Role GetNextRoleInHierarchy(RoleType currentRoleType)
        {
            try
            {
                Role nextRole = this.roles.First(role => role.RoleType == currentRoleType + 1);
                return nextRole;
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException($"No next role exists for {currentRoleType}");
            }
        }
    }
}