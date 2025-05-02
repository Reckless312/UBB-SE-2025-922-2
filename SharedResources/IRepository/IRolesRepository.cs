// <copyright file="IRolesRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IRepository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;

    /// <summary>
    /// interface for roles.
    /// </summary>
    public interface IRolesRepository
    {
        /// <summary>
        /// returns next role.
        /// </summary>
        /// <param name="currentRoleType"current role>the current role.</param>
        /// <returns>next role.</returns>
        public Task<Role> GetNextRoleInHierarchy(RoleType currentRoleType);

        /// <summary>
        /// gets all the roles.
        /// </summary>
        /// <returns>list of all the roles.</returns>
        public Task<List<Role>> GetAllRoles();
    }
}