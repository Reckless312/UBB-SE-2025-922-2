namespace DrinkDb_Auth.Service
{
    using DataAccess.Model.AdminDashboard;
    using IRepository;
    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class UpgradeRequestsService : IUpgradeRequestsService
    {
        private readonly IUpgradeRequestsRepository upgradeRequestsRepository;
        private readonly IRolesRepository rolesRepository;
        private readonly IUserRepository userRepository;

        public UpgradeRequestsService(
            IUpgradeRequestsRepository upgradeRequestsRepository,
            IRolesRepository rolesRepository,
            IUserRepository userRepository)
        {
            this.upgradeRequestsRepository = upgradeRequestsRepository;
            this.rolesRepository = rolesRepository;
            this.userRepository = userRepository;
            this.RemoveUpgradeRequestsFromBannedUsers();
        }

        public void RemoveUpgradeRequestsFromBannedUsers()
        {
            List<UpgradeRequest> pendingUpgradeRequests = this.RetrieveAllUpgradeRequests();

            // Use a reversed loop or a copy of the list to safely remove items
            for (int i = pendingUpgradeRequests.Count - 1; i >= 0; i--)
            {
                Guid requestingUserIdentifier = pendingUpgradeRequests[i].RequestingUserIdentifier;

                if (this.userRepository.GetRoleTypeForUser(requestingUserIdentifier).Result == RoleType.Banned)
                {
                    this.upgradeRequestsRepository.RemoveUpgradeRequestByIdentifier(pendingUpgradeRequests[i].UpgradeRequestId);
                }
            }
        }

        public string GetRoleNameBasedOnIdentifier(RoleType roleType)
        {
            List<Role> availableRoles = this.rolesRepository.GetAllRoles().Result;
            Role matchingRole = availableRoles.First(role => role.RoleType == roleType);
            return matchingRole.RoleName;
        }

        public List<UpgradeRequest> RetrieveAllUpgradeRequests()
        {
            return this.upgradeRequestsRepository.RetrieveAllUpgradeRequests().Result;
        }

        public void ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier)
        {
            if (isRequestAccepted)
            {
                UpgradeRequest currentUpgradeRequest = this.upgradeRequestsRepository.RetrieveUpgradeRequestByIdentifier(upgradeRequestIdentifier).Result;
                Guid requestingUserIdentifier = currentUpgradeRequest.RequestingUserIdentifier;
                RoleType currentHighestRoleType = this.userRepository.GetRoleTypeForUser(requestingUserIdentifier).Result;
                Role nextRoleLevel = this.rolesRepository.GetNextRoleInHierarchy(currentHighestRoleType).Result;
                this.userRepository.ChangeRoleToUser(requestingUserIdentifier, nextRoleLevel);
            }

            this.upgradeRequestsRepository.RemoveUpgradeRequestByIdentifier(upgradeRequestIdentifier);
        }
    }
}