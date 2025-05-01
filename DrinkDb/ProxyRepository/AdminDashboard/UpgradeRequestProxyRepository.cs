namespace DrinkDb_Auth.ProxyRepository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using IRepository;

    public class UpgradeRequestProxyRepository : IUpgradeRequestsRepository
    {
        public void RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            throw new NotImplementedException();
        }

        public List<UpgradeRequest> RetrieveAllUpgradeRequests()
        {
            throw new NotImplementedException();
        }

        public UpgradeRequest RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            throw new NotImplementedException();
        }
    }
}
