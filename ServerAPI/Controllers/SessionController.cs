using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.Authentication;
using DataAccess.Service.Authentication.Interfaces;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    public class SessionController : ControllerBase
    {
        private ISessionService sessionService;

        public SessionController(ISessionService service)
        {
            this.sessionService = service;
        }

        [HttpPost("add")]
        public async Task<Session?> CreateSession([FromQuery] Guid userId)
        {
            return await this.sessionService.CreateSessionAsync(userId);
        }

        [HttpPatch("end")]
        public async Task<bool> EndSession([FromQuery] Guid sessionId)
        {
            return await this.sessionService.EndSessionAsync(sessionId);
        }

        [HttpGet("{id}")]
        public async Task<Session?> GetSession(Guid id)
        {
            return await this.sessionService.GetSessionAsync(id);
        }

        [HttpGet("byUserId/{id}")]
        public async Task<Session?> GetSessionByUserID(Guid id)
        {
            return await this.sessionService.GetSessionByUserIdAsync(id);
        }
    }
}
