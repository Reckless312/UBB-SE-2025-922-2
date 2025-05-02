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

        public async Task RemoveUpgradeRequestsFromBannedUsers()
        {
            List<UpgradeRequest> pendingUpgradeRequests = await this.RetrieveAllUpgradeRequests();

            // Use a reversed loop or a copy of the list to safely remove items
            for (int i = pendingUpgradeRequests.Count - 1; i >= 0; i--)
            {
                Guid requestingUserIdentifier = pendingUpgradeRequests[i].RequestingUserIdentifier;

                if (await this.userRepository.GetHighestRoleTypeForUser(requestingUserIdentifier) == RoleType.Banned)
                {
                    await this.upgradeRequestsRepository.RemoveUpgradeRequestByIdentifier(pendingUpgradeRequests[i].UpgradeRequestId);
                }
            }
        }

        public async Task<string> GetRoleNameBasedOnIdentifier(RoleType roleType)
        {
            List<Role> availableRoles = await this.rolesRepository.GetAllRoles();
            Role matchingRole = availableRoles.First(role => role.RoleType == roleType);
            return matchingRole.RoleName;
        }

        public async Task<List<UpgradeRequest>> RetrieveAllUpgradeRequests()
        {
            return await this.upgradeRequestsRepository.RetrieveAllUpgradeRequests();
        }

        public async Task ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier)
        {
            if (isRequestAccepted)
            {
                UpgradeRequest currentUpgradeRequest = await this.upgradeRequestsRepository.RetrieveUpgradeRequestByIdentifier(upgradeRequestIdentifier);
                Guid requestingUserIdentifier = currentUpgradeRequest.RequestingUserIdentifier;
                RoleType currentHighestRoleType = await this.userRepository.GetHighestRoleTypeForUser(requestingUserIdentifier);
                Role nextRoleLevel = await this.rolesRepository.GetNextRoleInHierarchy(currentHighestRoleType);
                await this.userRepository.AddRoleToUser(requestingUserIdentifier, nextRoleLevel);
            }

            await this.upgradeRequestsRepository.RemoveUpgradeRequestByIdentifier(upgradeRequestIdentifier);
        }
    }
}