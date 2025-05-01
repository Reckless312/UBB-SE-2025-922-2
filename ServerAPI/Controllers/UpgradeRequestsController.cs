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
        public IEnumerable<UpgradeRequest> GetAll()
        {
            return repository.RetrieveAllUpgradeRequests();
        }

        [HttpDelete("{id}/delete")]
        public void Delete(int id)
        {
            repository.RemoveUpgradeRequestByIdentifier(id);
        }

        [HttpGet("{id}")]
        public UpgradeRequest Get(int id)
        {
            return repository.RetrieveUpgradeRequestByIdentifier(id);
        }
    }
}
