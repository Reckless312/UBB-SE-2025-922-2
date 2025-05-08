//// <copyright file="UserIdToNameConverter.cs" company="PlaceholderCompany">
//// Copyright (c) PlaceholderCompany. All rights reserved.
//// </copyright>

//namespace App1.Converters
//{
//    using System;
//    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
//    using Microsoft.UI.Xaml.Data;

//    /// <summary>
//    /// Converter that transforms a user ID into a display name.
//    /// </summary>
//    public class UserIdToNameConverter : IValueConverter
//    {
//        // Keep the original field name for compatibility with reflection in tests
//        private static IUserService _userService;

//        /// <summary>
//        /// Initializes the converter with a user service.
//        /// </summary>
//        /// <param name="userService">The user service to use for lookups.</param>
//        public static void Initialize(IUserService userService)
//        {
//            _userService = userService;
//        }

//        /// <summary>
//        /// Converts a user ID to a user name.
//        /// </summary>
//        /// <param name="value">The user ID to convert.</param>
//        /// <param name="targetType">The type of the binding target property.</param>
//        /// <param name="parameter">The converter parameter to use.</param>
//        /// <param name="language">The language to use in the converter.</param>
//        /// <returns>A string representation of the user name.</returns>
//        public object Convert(object value, Type targetType, object parameter, string language)
//        {
//            if (value is int userId && _userService != null)
//            {
//                try
//                {
//                    var user = _userService.GetUserById(new Guid());
//                    return string.IsNullOrEmpty(user?.FullName) ? $"User {userId}" : user.FullName;
//                }
//                catch
//                {
//                    return $"User {userId}";
//                }
//            }

//            return "Unknown User";
//        }

//        /// <summary>
//        /// Converts a user name back to a user ID.
//        /// </summary>
//        /// <param name="value">The value to convert back.</param>
//        /// <param name="targetType">The type of the binding target property.</param>
//        /// <param name="parameter">The converter parameter to use.</param>
//        /// <param name="language">The language to use in the converter.</param>
//        /// <returns>The converted value.</returns>
//        /// <exception cref="NotImplementedException">This method is not implemented.</exception>
//        public object ConvertBack(object value, Type targetType, object parameter, string language)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}