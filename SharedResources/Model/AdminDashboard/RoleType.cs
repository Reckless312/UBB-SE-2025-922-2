// <copyright file="RoleType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccess.Model.AdminDashboard
{
    /// <summary>
    /// Represents the type of role assigned to a user.
    /// </summary>
    public enum RoleType
    {
        /// <summary>
        /// The user is banned.
        /// </summary>
        Banned = 0,

        /// <summary>
        /// The user has a standard user role.
        /// </summary>
        User = 1,

        /// <summary>
        /// The user has administrative privileges.
        /// </summary>
        Admin = 2,

        /// <summary>
        /// The user has managerial privileges.
        /// </summary>
        Manager = 3,
    }
}