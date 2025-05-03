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
                var nextRole = await _context.Roles
                    .FirstOrDefaultAsync(role => role.RoleType == currentRoleType + 1);

                if (nextRole == null)
                {
                    throw new InvalidOperationException($"No next role exists for {currentRoleType}");
                }

                return nextRole;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to retrieve the next role in hierarchy for {currentRoleType}.", ex);
            }
        }
    }
}