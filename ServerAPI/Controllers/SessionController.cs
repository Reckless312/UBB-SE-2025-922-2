using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.Authentication;
using Repository.Authentication;
using IRepository;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    public class SessionController : ControllerBase
    {
        private ISessionRepository repository;

        public SessionController(ISessionRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpPost("add")]
        public async Task<Session> CreateSession([FromQuery] Guid userId)
        {
            return await repository.CreateSession(userId);
        }

        [HttpPatch("end")]
        public async Task<bool> EndSession([FromQuery] Guid sessionId)
        {
            return await repository.EndSession(sessionId);
        }

        [HttpGet("{id}")]
        public async Task<Session> GetSession(Guid id)
        { 
            return await repository.GetSession(id);
        }
        
        [HttpGet("byUserId/{id}")]
        public async Task<Session> GetSessionByUserID(Guid id)
        { 
            return await repository.GetSessionByUserId(id);
        }
    }
}
