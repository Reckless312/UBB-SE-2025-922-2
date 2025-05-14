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

    public interface IRolesRepository
    {
        public Task<Role> GetNextRoleInHierarchy(RoleType currentRoleType);

        public Task<List<Role>> GetAllRoles();
    }
}