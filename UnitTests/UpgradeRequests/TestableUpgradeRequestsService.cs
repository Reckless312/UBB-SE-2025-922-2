namespace UnitTests.UpgradeRequests
{
    using DataAccess.Model.AdminDashboard;
    using DrinkDb_Auth.Service;
    using IRepository;
    using Moq;
    using System;
    using System.Collections.Generic;
    using Xunit;

    // Test subclass that exposes the protected method for testing
    public class TestableUpgradeRequestsService : UpgradeRequestsService
    {
        public TestableUpgradeRequestsService(
            IUpgradeRequestsRepository upgradeRequestsRepository,
            IRolesRepository rolesRepository,
            IUserRepository userRepository)
            : base(upgradeRequestsRepository, rolesRepository, userRepository)
        {
            // Skip calling RemoveUpgradeRequestsFromBannedUsers in constructor
            // We'll call it explicitly in our test
        }

        // Expose the protected method for testing
        public void PublicRemoveUpgradeRequestsFromBannedUsers()
        {
            this.RemoveUpgradeRequestsFromBannedUsers();
        }
    }

    public class UpgradeRequestsServiceTests
    {
        private readonly Mock<IUpgradeRequestsRepository> mockUpgradeRequestsRepository;
        private readonly Mock<IRolesRepository> mockRolesRepository;
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly UpgradeRequestsService upgradeRequestsService;

        public UpgradeRequestsServiceTests()
        {
            this.mockUpgradeRequestsRepository = new Mock<IUpgradeRequestsRepository>();
            this.mockRolesRepository = new Mock<IRolesRepository>();
            this.mockUserRepository = new Mock<IUserRepository>();
            List<Role> roles = new List<Role>
            {
                new Role(RoleType.Banned, "Banned"),
                new Role(RoleType.User, "User"),
                new Role(RoleType.Manager, "Manager"),
                new Role(RoleType.Admin, "Admin"),
            };
            this.mockRolesRepository.Setup(r => r.GetAllRoles()).Returns(roles);
            this.mockUpgradeRequestsRepository.Setup(r => r.RetrieveAllUpgradeRequests())
                .Returns(new List<UpgradeRequest>());
            this.upgradeRequestsService = new UpgradeRequestsService(
                this.mockUpgradeRequestsRepository.Object,
                this.mockRolesRepository.Object,
                this.mockUserRepository.Object);
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesService()
        {
            this.mockUpgradeRequestsRepository.Verify(r => r.RetrieveAllUpgradeRequests(), Times.Once);
        }

        [Fact]
        public void RetrieveAllUpgradeRequests_ReturnsListFromRepository()
        {
            List<UpgradeRequest> expectedRequests = new List<UpgradeRequest>
            {
                new UpgradeRequest(1, Guid.NewGuid(), "User 1"),
                new UpgradeRequest(2, Guid.NewGuid(), "User 2"),
            };
            this.mockUpgradeRequestsRepository.Setup(r => r.RetrieveAllUpgradeRequests())
                .Returns(expectedRequests);

            List<UpgradeRequest> result = this.upgradeRequestsService.RetrieveAllUpgradeRequests();
            Assert.Equal(expectedRequests, result);
            this.mockUpgradeRequestsRepository.Verify(r => r.RetrieveAllUpgradeRequests(), Times.AtLeastOnce);
        }

        [Fact]
        public void GetRoleNameBasedOnIdentifier_ReturnsRoleName()
        {
            RoleType roleType = RoleType.User;
            string expectedRoleName = "User";
            string result = this.upgradeRequestsService.GetRoleNameBasedOnIdentifier(roleType);
            Assert.Equal(expectedRoleName, result);
            this.mockRolesRepository.Verify(r => r.GetAllRoles(), Times.AtLeastOnce);
        }

        [Fact]
        public void ProcessUpgradeRequest_WhenAccepted_UpgradesUserRole()
        {
            int upgradeRequestIdentifier = 1;
            Guid requestingUserIdentifier = Guid.NewGuid();
            UpgradeRequest upgradeRequest = new UpgradeRequest(upgradeRequestIdentifier, requestingUserIdentifier, "Test User");
            Role nextRole = new Role(RoleType.Manager, "Manager");
            this.mockUpgradeRequestsRepository.Setup(r => r.RetrieveUpgradeRequestByIdentifier(upgradeRequestIdentifier)).Returns(upgradeRequest);
            this.mockUserRepository.Setup(r => r.GetHighestRoleTypeForUser(requestingUserIdentifier)).Returns(RoleType.User);
            this.mockRolesRepository.Setup(r => r.GetNextRoleInHierarchy(RoleType.User)).Returns(nextRole);
            this.upgradeRequestsService.ProcessUpgradeRequest(true, upgradeRequestIdentifier);
            this.mockUpgradeRequestsRepository.Verify(r => r.RetrieveUpgradeRequestByIdentifier(upgradeRequestIdentifier), Times.Once);
            this.mockUserRepository.Verify(r => r.GetHighestRoleTypeForUser(requestingUserIdentifier), Times.Once);
            this.mockRolesRepository.Verify(r => r.GetNextRoleInHierarchy(RoleType.User), Times.Once);
            this.mockUserRepository.Verify(r => r.AddRoleToUser(requestingUserIdentifier, nextRole), Times.Once);
            this.mockUpgradeRequestsRepository.Verify(r => r.RemoveUpgradeRequestByIdentifier(upgradeRequestIdentifier), Times.Once);
        }

        [Fact]
        public void ProcessUpgradeRequest_WhenDeclined_OnlyRemovesRequest()
        {
            int upgradeRequestIdentifier = 1;
            this.upgradeRequestsService.ProcessUpgradeRequest(false, upgradeRequestIdentifier);
            this.mockUpgradeRequestsRepository.Verify(r => r.RetrieveUpgradeRequestByIdentifier(upgradeRequestIdentifier), Times.Never);
            this.mockUserRepository.Verify(r => r.GetHighestRoleTypeForUser(It.IsAny<Guid>()), Times.Never);
            this.mockRolesRepository.Verify(r => r.GetNextRoleInHierarchy(It.IsAny<RoleType>()), Times.Never);
            this.mockUserRepository.Verify(r => r.AddRoleToUser(It.IsAny<Guid>(), It.IsAny<Role>()), Times.Never);
            this.mockUpgradeRequestsRepository.Verify(r => r.RemoveUpgradeRequestByIdentifier(upgradeRequestIdentifier), Times.Once);
        }

        [Fact]
        public void RemoveUpgradeRequestsFromBannedUsers_RemovesBannedUsersRequests()
        {
            List<UpgradeRequest> upgradeRequests = new List<UpgradeRequest>
            {
                new UpgradeRequest(1, Guid.NewGuid(), "Regular User"),
                new UpgradeRequest(2, Guid.NewGuid(), "Banned User"),
                new UpgradeRequest(3, Guid.NewGuid(), "Another Regular User"),
                new UpgradeRequest(4, Guid.NewGuid(), "Another Banned User"),
            };
            Mock<IUpgradeRequestsRepository> mockRepository = new Mock<IUpgradeRequestsRepository>();
            Mock<IRolesRepository> mockRoles = new Mock<IRolesRepository>();
            Mock<IUserRepository> mockUsers = new Mock<IUserRepository>();
            mockRepository.Setup(r => r.RetrieveAllUpgradeRequests()).Returns(upgradeRequests);
            mockRoles.Setup(r => r.GetAllRoles()).Returns(new List<Role> { new Role(RoleType.Banned, "Banned"), new Role(RoleType.User, "User") });
            TestableUpgradeRequestsService testableService = new TestableUpgradeRequestsService(
                mockRepository.Object,
                mockRoles.Object,
                mockUsers.Object);
            testableService.PublicRemoveUpgradeRequestsFromBannedUsers();
            mockRepository.Verify(r => r.RemoveUpgradeRequestByIdentifier(2), Times.AtLeastOnce);
            mockRepository.Verify(r => r.RemoveUpgradeRequestByIdentifier(4), Times.AtLeastOnce);
        }
    }
}