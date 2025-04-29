namespace DrinkDb_Auth.Service.AdminDashboard.Components
{
    using System;
    using System.Collections.Generic;
    using static DrinkDb_Auth.Repository.AdminDashboard.UserRepository;

    /// <summary>
    /// Exception class for user service-related errors.
    /// </summary>
    public class UserServiceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserServiceException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public UserServiceException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}