using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using Repository.AdminDashboard;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Service.AdminDashboard.Interfaces;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private IUserService service;

        public UsersController(IUserService service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        public async Task<IEnumerable<User>> GetUsers()
        {
            return await service.GetAllUsers();
        }

        [HttpGet("appealed")]
        public async Task<IEnumerable<User>> GetUsersWhoHaveSubmittedAppeals()
        {
            return await service.GetUsersWhoHaveSubmittedAppeals();
        }

        [HttpGet("banned/appealed")]
        public async Task<IEnumerable<User>> GetBannedUsersWhoHaveSubmittedAppeals()
        {
            return await service.GetBannedUsersWhoHaveSubmittedAppeals();
        }

        [HttpGet("byRole/{role}")]
        public async Task<IEnumerable<User>> GetUsersByRoleType(RoleType roleType)
        {
            return await service.GetUsersByRoleType(roleType);
        }

        [HttpGet("byId/{userId}/role")]
        public async Task<ActionResult<RoleType>> GetHighestRoleTypeForUser(Guid userId)
        {
            var role = await service.GetHighestRoleTypeForUser(userId);
            return role == null ? NotFound() : role;
        }

        [HttpPatch("byId/{userId}/addRole")]
        public void AddRoleToUser(Guid userId, Role role)
        {
            service.ChangeRoleToUser(userId, role);
        }

        [HttpGet("byId/{userID}")]
        public async Task<ActionResult<User>> GetUserById(Guid userId)
        {
            var user = await service.GetUserById(userId);
            return user == null ? NotFound() : user;
        }

        [HttpGet("byUserName/{username}")]
        public async Task<ActionResult<User>> GetUserByName(string username)
        {
            try
            {
                var user = await service.GetUserByUsername(username);
                return user == null ? NotFound() : user;
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpPatch("{userId}/updateUser")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] User user)
        {
            var result = await service.UpdateUser(user);
            return result ? Ok(result) : BadRequest("Failed to update user");
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            var result = await service.CreateUser(user);
            return result ? Ok(result) : BadRequest("Failed to create user");
        }

        [HttpGet("validateAction")]
        public async Task<bool> ValidateAction([FromQuery] Guid userID, [FromQuery] string resource, [FromQuery] string action)
        {
            return await service.ValidateAction(userID, resource, action);
        }

        [HttpPatch("byId/{userId}/appealed")]
        public async Task<IActionResult> UpdateUserAppealed(Guid userId, [FromBody] bool newValue)
        {
            try
            {
                var user = await service.GetUserById(userId);
                if (user == null)
                {
                    return NotFound();
                }

                service.UpdateUserAppleaed(user, newValue);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch("byId/{userId}/role")]
        public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] RoleType roleType)
        {
            try
            {
                var user = await service.GetUserById(userId);
                if (user == null)
                {
                    return NotFound();
                }

                await service.UpdateUserRole(userId, roleType);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}