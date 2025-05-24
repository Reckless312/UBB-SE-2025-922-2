namespace Repository.AdminDashboard
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using IRepository;
    using Microsoft.EntityFrameworkCore;
    using ServerAPI.Data;

    public class RolesRepository : IRolesRepository
    {
        private readonly DatabaseContext dataContext;

        public RolesRepository(DatabaseContext context)
        {
            this.dataContext = context;
        }

        public async Task<List<Role>> GetAllRoles()
        {
            this.CheckForExistingRoles();

            return await dataContext.Roles.ToListAsync();
        }

        public async Task<Role?> GetNextRoleInHierarchy(RoleType currentRoleType)
        {
            this.CheckForExistingRoles();

            if (currentRoleType.Equals(RoleType.Admin))
            {
                return await this.dataContext.Roles.FirstOrDefaultAsync(role => role.RoleType == currentRoleType);
            }
            Role? nextRole = await this.dataContext.Roles.FirstOrDefaultAsync(role => role.RoleType == currentRoleType + 1);
            return nextRole;
        }

        private void CheckForExistingRoles()
        {
            if (!this.dataContext.Roles.Any())
            {
                this.AddRole(RoleType.Banned, "Banned");
                this.AddRole(RoleType.User, "User");
                this.AddRole(RoleType.Admin, "Admin");
            }
        }

        private void AddRole(RoleType roleType, string roleName)
        {
            this.dataContext.Roles.Add(new Role(roleType, roleName));
            this.dataContext.SaveChanges();
        }
    }
}