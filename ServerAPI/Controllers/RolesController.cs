using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.AdminDashboard;
using Repository.AdminDashboard;
using IRepository;

namespace ServerAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class RolesController : ControllerBase
    {
        private IRolesRepository repository = new RolesRepository();
        [HttpGet]
        public async Task<IEnumerable<Role>> GetAll()
        {
            return await repository.GetAllRoles();
        }

        [HttpGet("next")]
        public async Task<Role> GetNextRoleInHierarchy([FromQuery] RoleType currentRoleType)
        {
            return await repository.GetNextRoleInHierarchy(currentRoleType);
        }
    }
}
