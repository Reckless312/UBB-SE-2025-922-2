using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.AdminDashboard;
using Repository.AdminDashboard;
using IRepository;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UpgradeRequestsController : ControllerBase
    {
        IUpgradeRequestsRepository repository = new UpgradeRequestsRepository(new SqlConnectionFactory("Data Source=CORA\\MSSQLSERVER01; Initial Catalog = DrinkDB_Dev; Integrated Security = True; Trust Server Certificate = True"));
        [HttpGet]
        public async Task<IEnumerable<UpgradeRequest>> GetAll()
        {
            return await repository.RetrieveAllUpgradeRequests();
        }

        [HttpDelete("{id}/delete")]
        public async Task Delete(int id)
        {
            await repository.RemoveUpgradeRequestByIdentifier(id);
        }

        [HttpGet("{id}")]
        public async Task<UpgradeRequest> Get(int id)
        {
            return await repository.RetrieveUpgradeRequestByIdentifier(id);
        }
    }
}
