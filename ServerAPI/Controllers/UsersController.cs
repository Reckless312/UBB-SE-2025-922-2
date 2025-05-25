using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using DataAccess.Service.AdminDashboard.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private IUserService userService;

        public UsersController(IUserService service)
        {
            this.userService = service;
        }

        [HttpGet]
        public async Task<IEnumerable<User>> GetUsers()
        {
            return await this.userService.GetAllUsers();
        }

        [HttpGet("appealed")]
        public async Task<IEnumerable<User>> GetUsersWhoHaveSubmittedAppeals()
        {
            return await this.userService.GetUsersWhoHaveSubmittedAppeals();
        }

        [HttpGet("banned/appealed")]
        public async Task<IEnumerable<User>> GetBannedUsersWhoHaveSubmittedAppeals()
        {
            return await this.userService.GetBannedUsersWhoHaveSubmittedAppeals();
        }

        [HttpGet("byRole/{role}")]
        public async Task<IEnumerable<User>> GetUsersByRoleType(RoleType roleType)
        {
            return await this.userService.GetUsersByRoleType(roleType);
        }

        [HttpGet("byId/{userId}/role")]
        public async Task<ActionResult<RoleType>> GetHighestRoleTypeForUser(Guid userId)
        {
            RoleType? role = await this.userService.GetHighestRoleTypeForUser(userId);
            return role == null ? NotFound() : role;
        }

        [HttpPatch("byId/{userId}/addRole")]
        public async Task AddRoleToUser(Guid userId, Role role)
        {
            await this.userService.ChangeRoleToUser(userId, role);
        }

        [HttpGet("byId/{userID}")]
        public async Task<ActionResult<User>> GetUserById(Guid userId)
        {
            User? user = await this.userService.GetUserById(userId);
            return user == null ? NotFound() : user;
        }

        [HttpGet("byUserName/{username}")]
        public async Task<ActionResult<User>> GetUserByName(string username)
        {
            User? user = await this.userService.GetUserByUsername(username);
            return user == null ? NotFound() : user;
        }

        [HttpPatch("{userId}/updateUser")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] User user)
        {
            bool result = await this.userService.UpdateUser(user);
            return result ? Ok(result) : BadRequest("Failed to update user");
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            bool result = await this.userService.CreateUser(user);
            return result ? Ok(result) : BadRequest("Failed to create user");
        }

        [HttpPatch("byId/{userId}/appealed")]
        public async Task<IActionResult> UpdateUserAppealed(Guid userId, [FromBody] bool newValue)
        {
            User? user = await this.userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound();
            }

            await this.userService.UpdateUserAppleaed(user, newValue);
            return Ok();
        }

        [HttpPatch("byId/{userId}/role")]
        public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] RoleType roleType)
        {
                User? user = await this.userService.GetUserById(userId);
                if (user == null)
                {
                    return NotFound();
                }

                await this.userService.UpdateUserRole(userId, roleType);
                return Ok();
        }
    }
}