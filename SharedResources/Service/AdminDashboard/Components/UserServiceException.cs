namespace DataAccess.Service.AdminDashboard.Components
{
    using System;
    using System.Collections.Generic;
    public class UserServiceException : Exception
    {
        public UserServiceException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}