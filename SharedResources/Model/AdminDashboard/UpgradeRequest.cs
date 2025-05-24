namespace DataAccess.Model.AdminDashboard
{
    public class UpgradeRequest
    {

        public UpgradeRequest()
        {
            this.RequestingUserDisplayName = string.Empty;
        }
        public UpgradeRequest(int upgradeRequestId, Guid requestingUserIdentifier, string requestingUserDisplayName)
        {
            this.UpgradeRequestId = upgradeRequestId;
            this.RequestingUserIdentifier = requestingUserIdentifier;
            this.RequestingUserDisplayName = requestingUserDisplayName;
        }

        public int UpgradeRequestId { get; set; }

        public Guid RequestingUserIdentifier { get; set; }

        public string RequestingUserDisplayName { get; set; }

        public override string ToString()
        {
            return RequestingUserDisplayName;
        }
    }
}
