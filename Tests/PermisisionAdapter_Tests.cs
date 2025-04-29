using DrinkDb_Auth.Adapter;
using DrinkDb_Auth.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Configuration.Provider;
using Windows.Security.Cryptography.Certificates;

namespace Tests
{
    [TestClass]
    public class PermisisionAdapter_Tests
    {

        [TestMethod]
        public void CreatePermission_ShouldInsertPermission()
        {
            PermissionAdapter _adapter = new PermissionAdapter();
            var permission = new Permission
            {
                PermissionId = Guid.NewGuid(),
                PermissionName = "TestPermission",
                Resource = "TestResource",
                Action = "TestAction"
            }; ;

            _adapter.CreatePermission(permission);

            var permissionId = _adapter.GetPermissions()[0].PermissionId;

            var retrieved = _adapter.GetPermissionById(permissionId);
            Assert.AreEqual(permission.PermissionName, retrieved.PermissionName);
            Assert.AreEqual(permission.Resource, retrieved.Resource);
            Assert.AreEqual(permission.Action, retrieved.Action);
            SqlConnection sqlConnection = new SqlConnection("Data Source=CORA\\MSSQLSERVER01; Initial Catalog = DrinkDB_Dev; Integrated Security = True; Trust Server Certificate = True");
            SqlCommand cmd = new SqlCommand("delete from Permissions", sqlConnection);
            sqlConnection.Open();
            cmd.BeginExecuteNonQuery();
            sqlConnection.Close();
        }
    }
}
