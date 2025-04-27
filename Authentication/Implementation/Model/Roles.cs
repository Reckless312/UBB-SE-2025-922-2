using System;

namespace DrinkDb_Auth.Model
{
    public class Roles
    {
        public required Guid RoleIdentifier { get; set; }
        public required string RoleName { get; set; }
        public required string RoleDescription { get; set; }
    }
}


