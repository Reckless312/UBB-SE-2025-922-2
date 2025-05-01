// <copyright file="Role.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccess.Model.AdminDashboard
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