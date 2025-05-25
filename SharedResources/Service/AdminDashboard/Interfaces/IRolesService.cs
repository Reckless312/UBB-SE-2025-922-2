using DataAccess.Model.AdminDashboard;

namespace DrinkDb_Auth.Service.AdminDashboard.Interfaces
{
    public interface IRolesService
    {
        Task<List<Role>> GetAllRolesAsync();

        Task<Role?> GetNextRoleInHierarchyAsync(RoleType currentRoleType);
    }
}