namespace DataAccess.Service.AdminDashboard.Components
{
    using System;
    public class UserServiceException : Exception
    {
        public UserServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}