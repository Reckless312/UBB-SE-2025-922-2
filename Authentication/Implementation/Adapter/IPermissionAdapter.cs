using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrinkDb_Auth.Model;

namespace DrinkDb_Auth.Adapter
{
    internal interface IPermissionAdapter
    {
        public void CreatePermission(Permission permission);
        public void UpdatePermission(Permission permission);
        public void DeletePermission(Permission permission);
        public Permission GetPermissionById(Guid id);
        public List<Permission> GetPermissions();
    }
}
