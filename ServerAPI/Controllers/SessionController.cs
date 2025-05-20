using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.Authentication;
using DataAccess.Service.Authentication.Interfaces;


namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    public class SessionController : ControllerBase
    {
        private ISessionService service;

        public SessionController(ISessionService service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpPost("add")]
        public async Task<Session> CreateSession([FromQuery] Guid userId)
        {
            return await service.CreateSessionAsync(userId);
        }

        [HttpPatch("end")]
        public async Task<bool> EndSession([FromQuery] Guid sessionId)
        {
            return await service.EndSessionAsync(sessionId);
        }

        [HttpGet("{id}")]
        public async Task<Session> GetSession(Guid id)
        { 
            return await service.GetSessionAsync(id);
        }
        
        [HttpGet("byUserId/{id}")]
        public async Task<Session> GetSessionByUserID(Guid id)
        { 
            return await service.GetSessionByUserIdAsync(id);
        }
    }
}
