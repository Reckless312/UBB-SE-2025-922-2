namespace CombinedProject.Model
{
    public class Role
    {
        public Role(RoleType roleType, string roleName)
        {
            RoleType = roleType;
            RoleName = roleName;
        }

        public RoleType RoleType { get; set; }

        public string RoleName { get; set; }
    }
}