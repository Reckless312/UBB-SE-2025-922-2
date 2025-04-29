namespace CombinedProject.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CombinedProject.Model;

    public interface IUserRepository
    {
        // public void UpdateRole(int userID, int permissionID);
        public List<User> GetUsersWhoHaveSubmittedAppeals();

        List<User> GetBannedUsersWhoHaveSubmittedAppeals();

        User GetUserByID(int iD);

        public List<User> GetUsersByRoleType(RoleType roleType);

        public RoleType GetHighestRoleTypeForUser(Guid userId);

        public void AddRoleToUser(Guid userID, Role roleToAdd);

        public List<User> GetAllUsers();
    }
}