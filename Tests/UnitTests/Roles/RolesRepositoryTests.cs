namespace UnitTests.Roles
{
    using App1.Models;
    using App1.Repositories;

    public class RolesRepositoryTests
    {
        private readonly RolesRepository repository;

        public RolesRepositoryTests()
        {
            this.repository = new RolesRepository();
        }

        /// <summary>
        /// Tests that GetAllRoles returns the complete set of predefined roles in the system.
        /// This test ensures that the repository correctly initializes and maintains the role hierarchy:
        /// - Banned (0)
        /// - User (1)
        /// - Admin (2)
        /// - Manager (3).
        /// </summary>
        [Fact]
        public void GetAllRoles_WhenCalled_ReturnsAllRoles()
        {
            List<Role> expectedRoles = new List<Role>
            {
                new Role(RoleType.Banned, "Banned"),
                new Role(RoleType.User, "User"),
                new Role(RoleType.Admin, "Admin"),
                new Role(RoleType.Manager, "Manager"),
            };

            List<Role> result = this.repository.GetAllRoles();

            Assert.Equal(expectedRoles.Count, result.Count);
            for (int i = 0; i < expectedRoles.Count; i++)
            {
                Assert.Equal(expectedRoles[i].RoleType, result[i].RoleType);
                Assert.Equal(expectedRoles[i].RoleName, result[i].RoleName);
            }
        }

        /// <summary>
        /// Tests the role promotion functionality by verifying that GetNextRoleInHierarchy returns the correct next role
        /// in the hierarchy for each valid current role. This ensures the role promotion system works correctly:
        /// - Banned -> User
        /// - User -> Admin
        /// - Admin -> Manager.
        /// </summary>
        [Theory]
        [InlineData(RoleType.Banned, RoleType.User)]
        [InlineData(RoleType.User, RoleType.Admin)]
        [InlineData(RoleType.Admin, RoleType.Manager)]
        public void GetNextRoleInHierarchy_WhenValidCurrentRole_ReturnsNextRole(RoleType currentRole, RoleType expectedNextRole)
        {
            Role result = this.repository.GetNextRoleInHierarchy(currentRole);
            Assert.Equal(expectedNextRole, result.RoleType);
        }

        [Fact]
        public void GetNextRoleInHierarchy_WhenManagerRole_ThrowsException()
        {
            RoleType currentRole = RoleType.Manager;
            Assert.Throws<InvalidOperationException>(() => this.repository.GetNextRoleInHierarchy(currentRole));
        }

        [Fact]
        public void GetNextRoleInHierarchy_WhenInvalidRoleType_ThrowsException()
        {
            RoleType invalidRoleType = (RoleType)999;
            Assert.Throws<InvalidOperationException>(() => this.repository.GetNextRoleInHierarchy(invalidRoleType));
        }
    }
}
