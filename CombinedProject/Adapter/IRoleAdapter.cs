using System;
using System.Collections.Generic;
using CombinedProject.Model;

namespace CombinedProject.Database
{
    public interface IRoleAdapter
    {
        void InsertRole(Roles role);
        void DeleteRoleById(Roles role);
        Roles GetRoleById(System.Guid roleIdentifier);
        List<Roles> GetAllRoles();
        void UpdateRole(Roles role);
    }
}






