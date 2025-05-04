namespace Repository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using IRepository;
    using Microsoft.EntityFrameworkCore;
    using ServerAPI.Data;
    using static Repository.AdminDashboard.UserRepository;

    public class RolesRepository : IRolesRepository
    {
        private readonly DatabaseContext _context;

        public RolesRepository(DatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            if(this.GetAllRoles().Result.Count == 0)
                InitializeRoles();
        }

        public async Task<List<Role>> GetAllRoles()
        {
            try
            {
                return await _context.Roles.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to retrieve all roles.", ex);
            }
        }

        public async Task<Role> GetNextRoleInHierarchy(RoleType currentRoleType)
        {
            try
            {
                if (currentRoleType.Equals(RoleType.Manager))
                    return await _context.Roles.FirstOrDefaultAsync(role => role.RoleType == currentRoleType);
                var nextRole = await _context.Roles.FirstOrDefaultAsync(role => role.RoleType == currentRoleType + 1);
                return nextRole;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to retrieve the next role in hierarchy for {currentRoleType}.", ex);
            }
        }

        private void InitializeRoles() {
            this.AddRole(RoleType.Banned, "Banned");
            this.AddRole(RoleType.User, "User");
            this.AddRole(RoleType.Admin, "Admin");
            this.AddRole(RoleType.Manager, "Manager");
        }

        private void AddRole(RoleType roleType, string roleName) {

            _context.Roles.Add(new Role(roleType, roleName));
            _context.SaveChanges();
        }
    }
}