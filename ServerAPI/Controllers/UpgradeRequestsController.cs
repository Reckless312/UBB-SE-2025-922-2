using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.AdminDashboard;
using DataAccess.Service.AdminDashboard.Interfaces;
using IRepository;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/upgradeRequests")]
    public class UpgradeRequestsController : ControllerBase
    {
        private readonly IUpgradeRequestsService _upgradeRequestsService;

        public UpgradeRequestsController(IUpgradeRequestsService upgradeRequestsService)
        {
            _upgradeRequestsService = upgradeRequestsService ?? throw new ArgumentNullException(nameof(upgradeRequestsService));
        }

        [HttpGet]
        public async Task<IEnumerable<UpgradeRequest>> GetAll()
        {
            return await _upgradeRequestsService.RetrieveAllUpgradeRequests();
        }

        [HttpDelete("{id}/delete")]
        public async Task Delete(int id)
        {
            await _upgradeRequestsService.RemoveUpgradeRequestByIdentifier(id);
        }

        [HttpGet("{id}")]
        public async Task<UpgradeRequest> Get(int id)
        {
            return await _upgradeRequestsService.RetrieveUpgradeRequestByIdentifier(id);
        }

        [HttpPost("{id}/process")]
        public async Task Process(int id, [FromBody] bool isAccepted)
        {
            await _upgradeRequestsService.ProcessUpgradeRequest(isAccepted, id);
        }
    }
}
