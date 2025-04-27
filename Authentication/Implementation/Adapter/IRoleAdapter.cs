using System;
using System.Collections.Generic;
using DrinkDb_Auth.Model;

namespace DrinkDb_Auth.Database
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






