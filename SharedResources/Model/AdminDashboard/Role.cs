namespace DataAccess.Model.AdminDashboard
{
    public class Role
    {
        public Role()
        {
            RoleType = RoleType.User;
            RoleName = "User";
        }
        public Role(RoleType roleType, string roleName)
        {
            RoleType = roleType;
            RoleName = roleName;
        }

        public RoleType RoleType { get; set; } = RoleType.User;

        public string RoleName { get; set; }
    }
}