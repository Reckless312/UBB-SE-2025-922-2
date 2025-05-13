using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.AdminDashboard;
using Repository.AdminDashboard;
using IRepository;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/upgradeRequests")]
    public class UpgradeRequestsController : ControllerBase
    {
        IUpgradeRequestsRepository repository;

        public UpgradeRequestsController(IUpgradeRequestsRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpGet]
        public async Task<IEnumerable<UpgradeRequest>> GetAll()
        {
            return repository.RetrieveAllUpgradeRequests().Result;
        }

        [HttpDelete("{id}/delete")]
        public async Task Delete(int id)
        {
            repository.RemoveUpgradeRequestByIdentifier(id);
        }

        [HttpGet("{id}")]
        public async Task<UpgradeRequest> Get(int id)
        {
            return repository.RetrieveUpgradeRequestByIdentifier(id).Result;
        }
    }
}
