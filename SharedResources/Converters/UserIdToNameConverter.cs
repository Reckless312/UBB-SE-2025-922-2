// <copyright file="UserIdToNameConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccess.Converters
{
    using System;
    using DataAccess.Model.Authentication;
    using DataAccess.Service.AdminDashboard.Interfaces;
    using Windows.UI.Xaml.Data;

    public class UserIdToNameConverter : IValueConverter
    {
        // Keep the original field name for compatibility with reflection in tests
        private static IUserService userService;

        public static void Initialize(IUserService userService)
        {
            UserIdToNameConverter.userService = userService;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Guid userId && userService != null)
            {
                try
                {
                    User user = userService.GetUserById(userId).Result;
                    return string.IsNullOrEmpty(user?.Username) ? $"User {userId}" : user.Username;
                }
                catch
                {
                    return $"User {userId}";
                }
            }

            return "Unknown User";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}