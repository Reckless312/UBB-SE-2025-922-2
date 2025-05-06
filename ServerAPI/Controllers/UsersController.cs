using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using Repository.AdminDashboard;
using IRepository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private IUserRepository repository;

        public UsersController(IUserRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

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
            var role = await repository.GetRoleTypeForUser(userId);
            return role == null ? NotFound() : role;
        }

        [HttpPatch("byId/{userId}/addRole")]
        public void AddRoleToUser(Guid userId, Role role)
        {
                repository.ChangeRoleToUser(userId, role);
               
        }

        [HttpGet("byId/{userID}")]
        public async Task<ActionResult<User>> GetUserById(Guid userId)
        {
            var user = await repository.GetUserById(userId);
            return user == null ? NotFound() : user;
        }

        [HttpGet("byUserName/{username}")]
        public async Task<ActionResult<User>> GetUserByName(string username)
        {
            var user = await repository.GetUserByUsername(username);
            return user == null ? NotFound() : user;
        }

        [HttpPatch("{userId}/updateUser")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] User user)
        {

                var result = await repository.UpdateUser(user);
                return result ? Ok(result) : BadRequest("Failed to update user");
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
                var result = await repository.CreateUser(user);
                return result ? Ok(result) : BadRequest("Failed to create user");
        }

        [HttpGet("validateAction")]
        public async Task<bool> ValidateAction([FromQuery] Guid userID, [FromQuery] string resource, [FromQuery] string action)
        {
            return await repository.ValidateAction(userID, resource, action);
        }
    }
}