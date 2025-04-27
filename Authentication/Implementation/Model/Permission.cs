using System;
namespace DrinkDb_Auth.Model
{
    public class Permission
    {
        public required Guid PermissionId { get; set; }
        public required string PermissionName { get; set; }
        public required string Resource { get; set; }
        public required string Action { get; set; }
    }
}

