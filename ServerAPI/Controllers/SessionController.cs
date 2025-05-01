using Microsoft.AspNetCore.Mvc;
using Model.Authentication;
using Repository.Authentication;
using Repository.Authentication.Interfaces;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SessionController : ControllerBase
    {
        private ISessionRepository repository = new SessionRepository();
        [HttpPost("add")]
        public Session CreateSession([FromQuery] Guid userId)
        {
            return repository.CreateSession(userId);
        }

        [HttpPatch("end")]
        public bool EndSession([FromQuery] Guid sessionId)
        {
            return repository.EndSession(sessionId);
        }

        [HttpGet("{id}")]
        public Session GetSession(Guid id)
        { 
            return repository.GetSession(id);
        }
        
        [HttpGet("byUserId/{id}")]
        public Session GetSessionByUserID(Guid id)
        { 
            return repository.GetSessionByUserId(id);
        }
    }
}
