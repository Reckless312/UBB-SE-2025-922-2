// <copyright file="RolesService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.Service.AdminDashboard
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
    using IRepository;

    /// <summary>
    /// Service for managing roles.
    /// </summary>
    public class RolesService : IRolesService
    {
        private readonly IRolesRepository rolesRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RolesService"/> class.
        /// </summary>
        /// <param name="rolesRepository">The roles repository.</param>
        public RolesService(IRolesRepository rolesRepository)
        {
            this.rolesRepository = rolesRepository;
        }

        
        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await this.rolesRepository.GetAllRoles();
        }

        
        public async Task<Role> GetNextRoleInHierarchyAsync(RoleType currentRoleType)
        {
            return await this.rolesRepository.GetNextRoleInHierarchy(currentRoleType);
        }
    }
} 