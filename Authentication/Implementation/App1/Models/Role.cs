// <copyright file="Role.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1.Models
{
    public class Role
    {
        public Role(RoleType roleType, string roleName)
        {
            this.RoleType = roleType;
            this.RoleName = roleName;
        }

        public RoleType RoleType { get; set; }

        public string RoleName { get; set; }
    }
}