using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using Repository.AdminDashboard;
using IRepository;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserRepository repository = new UserRepository();

        [HttpGet]
        public async Task<IEnumerable<User>> GetUsers()
        {
            return await repository.GetAllUsers();
        }

        [HttpGet("appealed")]
        public async Task<IEnumerable<User>> GetUsersWhoHaveSubmittedAppeals()
        {
            return await repository.GetUsersWhoHaveSubmittedAppeals();
        }
        
        [HttpGet("banned/appealed")]
        public async Task<IEnumerable<User>> GetBannedUsersWhoHaveSubmittedAppeals()
        {
            return await repository.GetBannedUsersWhoHaveSubmittedAppeals();
        }

        [HttpGet("byRole/{role}")]
        public async Task<IEnumerable<User>> GetUsersByRoleType(RoleType roleType)
        {
            return await repository.GetUsersByRoleType(roleType);
        }

        [HttpGet("byId/{userId}/role")]
        public async Task<ActionResult<RoleType>> GetHighestRoleTypeForUser(Guid userId)
        {
            var role = repository.GetHighestRoleTypeForUser(userId);
            return role == null ? NotFound() : await role;
        }

        [HttpPatch("byId/{userID}/addRole")]
        public async Task AddRoleToUser(Guid userID, [FromBody] Role role)
        {
            // change function to:
            // return repository.AddRoleToUser(userID, role) == null ? NotFound() : Ok() ;
            // when AddRoleToUser is changed to return null
            await repository.AddRoleToUser(userID, role);
        }

        [HttpGet("byId/{userID}")]
        public ActionResult<User> GetUserById(Guid userId)
        {
            var user = repository.GetUserById(userId);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpGet("byUserName/{userName}")]
        public ActionResult<User> GetUserByName(string userName)
        {
            var user = repository.GetUserByUsername(userName);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPatch("updateUser")]
        public async Task<bool> UpdateUser([FromBody] User user)
        {
            return await repository.UpdateUser(user);
        }

        [HttpPost("add")]
        public async Task<bool> CreateUser([FromBody] User user)
        {
            return await repository.CreateUser(user);
        }

        [HttpGet("validateAction")]
        public async Task<bool> ValidateAction([FromQuery] Guid userID, [FromQuery] string resource, [FromQuery] string action)
        {
            return await repository.ValidateAction(userID, resource, action);
        }
    }
}
