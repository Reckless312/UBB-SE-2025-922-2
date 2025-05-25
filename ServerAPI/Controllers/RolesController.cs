using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.AdminDashboard;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private IRolesService rolesService;
        public RolesController(IRolesService service)
        {
            this.rolesService = service;
        }

        [HttpGet]
        public async Task<IEnumerable<Role>> GetAll()
        {
            return await this.rolesService.GetAllRolesAsync();
        }

        [HttpGet("next")]
        public async Task<Role?> GetNextRoleInHierarchy([FromQuery] RoleType currentRoleType)
        {
            return await this.rolesService.GetNextRoleInHierarchyAsync(currentRoleType);
        }
    }
}