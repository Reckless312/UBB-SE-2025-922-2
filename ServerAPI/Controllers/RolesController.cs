using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.AdminDashboard;
using Repository.AdminDashboard;
using IRepository;

namespace ServerAPI.Controllers
{

    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private IRolesRepository repository;
        public RolesController(IRolesRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

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
