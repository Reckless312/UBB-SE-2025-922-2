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
        public IEnumerable<User> GetUsers()
        {
            List<User> users = repository.GetAllUsers().Result;
            return users;

        }

        [HttpGet("appealed")]
        public IEnumerable<User> GetUsersWhoHaveSubmittedAppeals()
        {
            return repository.GetUsersWhoHaveSubmittedAppeals().Result;
        }
        
        [HttpGet("banned/appealed")]
        public IEnumerable<User> GetBannedUsersWhoHaveSubmittedAppeals()
        {
            return  repository.GetBannedUsersWhoHaveSubmittedAppeals().Result;
        }

        [HttpGet("byRole/{role}")]
        public IEnumerable<User> GetUsersByRoleType(RoleType roleType)
        {
            return repository.GetUsersByRoleType(roleType).Result;
        }

        [HttpGet("byId/{userId}/role")]
        public ActionResult<RoleType> GetHighestRoleTypeForUser(Guid userId)
        {
            var role = repository.GetHighestRoleTypeForUser(userId);
            return role == null ? NotFound() :  role.Result;
        }

        [HttpPatch("byId/{userID}/addRole")]
        public void AddRoleToUser(Guid userID, [FromBody] Role role)
        {
            // change function to:
            // return repository.AddRoleToUser(userID, role) == null ? NotFound() : Ok() ;
            // when AddRoleToUser is changed to return null
            repository.AddRoleToUser(userID, role);
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
        public bool UpdateUser([FromBody] User user)
        {
            return repository.UpdateUser(user).Result;
        }

        [HttpPost("add")]
        public bool CreateUser([FromBody] User user)
        {
            return repository.CreateUser(user).Result;
        }

        [HttpGet("validateAction")]
        public bool ValidateAction([FromQuery] Guid userID, [FromQuery] string resource, [FromQuery] string action)
        {
            return repository.ValidateAction(userID, resource, action).Result;
        }
    }
}
