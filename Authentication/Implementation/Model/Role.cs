using System;
namespace DrinkDb_Auth.Model
{
    public class Role
    {
        public required Guid RoleId { get; set; }
        public required string RoleName { get; set; }
    }
}

