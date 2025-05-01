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
        public IEnumerable<Role> GetAll()
        {
            return repository.GetAllRoles();
        }

        [HttpGet("next")]
        public Role GetNextRoleInHierarchy([FromQuery] RoleType currentRoleType)
        {
            return repository.GetNextRoleInHierarchy(currentRoleType);
        }
    }
}
