using System;
using System.Collections.Generic;
using DrinkDb_Auth.Model.AdminDashboard;
using DrinkDb_Auth.Service;

namespace DrinkDb_Auth.Model.Authentication
{
    public class User
    {
        public required Guid UserId { get; set; }

        public required string Username { get; set; }

        public required string PasswordHash { get; set; }

        public required string? TwoFASecret { get; set; }

        public string EmailAddress { get; set; }

        public int NumberOfDeletedReviews { get; set; }

        public bool HasSubmittedAppeal { get; set; }

        public List<Role> AssignedRoles { get; set; }

        public override string ToString()
        {
            return "Id: " + UserId.ToString() + ", email: " + EmailAddress;
        }
    }
}
