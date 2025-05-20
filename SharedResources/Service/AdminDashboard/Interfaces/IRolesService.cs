// <copyright file="IRolesService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.Service.AdminDashboard.Interfaces
{
    using DataAccess.Model.AdminDashboard;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides role-related operations for retrieving and managing roles.
    /// </summary>
    public interface IRolesService
    {
        /// <summary>
        /// Gets all roles in the system.
        /// </summary>
        /// <returns>A list of all roles.</returns>
        Task<List<Role>> GetAllRolesAsync();

        /// <summary>
        /// Gets the next role in the hierarchy after the specified role type.
        /// </summary>
        /// <param name="currentRoleType">The current role type.</param>
        /// <returns>The next role in the hierarchy.</returns>
        Task<Role> GetNextRoleInHierarchyAsync(RoleType currentRoleType);
    }
} 