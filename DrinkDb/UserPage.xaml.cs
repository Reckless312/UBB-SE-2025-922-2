using DataAccess.Model.AdminDashboard;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
using DrinkDb_Auth.Service;
using DrinkDb_Auth.View;
using DrinkDb_Auth;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System;

using System;
using DataAccess.Model.Authentication;
using DrinkDb_Auth.Service;
using DrinkDb_Auth.Service.Authentication;
using DrinkDb_Auth.View;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
using System.Linq;
using System.Collections.Generic;
using DataAccess.Model.AdminDashboard;

namespace DrinkDb_Auth
{
    public sealed partial class UserPage : Page
    {
        private static readonly AuthenticationService AuthenticationService = new();
        private static readonly UserService UserService = new();

        private User currentUser;

        public UserPage()
        {
            this.InitializeComponent();
            LoadUserData();
            LoadUserReviews();
        }

        private void LoadUserData()
        {
            // Retrieve the current user's ID from the static property.
            Guid currentUserId = App.CurrentUserId;

            // Check if the user ID is valid (not empty)
            if (currentUserId != Guid.Empty)
            {
                // Retrieve the user from the database using your UserService.
                var userService = new UserService();
                currentUser = userService.GetUserById(currentUserId).Result;

                // Update UI with the retrieved data.
                if (currentUser != null)
                {
                    NameTextBlock.Text = currentUser.Username;
                    UsernameTextBlock.Text = "@" + currentUser.Username;
                    StatusTextBlock.Text = "Status: Online";
                }
                else
                {
                    NameTextBlock.Text = "User not found";
                    UsernameTextBlock.Text = string.Empty;
                    StatusTextBlock.Text = string.Empty;
                }
            }
            else
            {
                // If no user is stored, show a default message.
                NameTextBlock.Text = "No user logged in";
                UsernameTextBlock.Text = string.Empty;
                StatusTextBlock.Text = string.Empty;
            }

            RoleType userRole = this.currentUser.AssignedRole;

            bool isAdmin = false;

            if (userRole == RoleType.Admin)
            {
                isAdmin = true;
            }
            bool isManager = false;

            if (userRole == RoleType.Manager)
            {
                isManager = true;
            }
            if (!isAdmin&&!isManager)
            {
                this.AdminDashboardButton.Visibility = Visibility.Collapsed;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationService.Logout();
            App.MainWindow.Close();
        }

        private void AdminDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(MainPage));
            }
        }

        private void LoadUserReviews()
        {
            // Get the review service from DI
            var reviewService = (IReviewService)App.Host.Services.GetService(typeof(IReviewService));
            Guid currentUserId = App.CurrentUserId;
            if (currentUserId == Guid.Empty || reviewService == null)
            {
                return;
            }
            var userReviews = reviewService.GetReviewsByUser(currentUserId)
                .Where(r => !r.IsHidden)
                .OrderByDescending(r => r.CreatedDate)
                .ToList();
            ReviewsItemsControl.Items.Clear();
            foreach (var review in userReviews)
            {
                var border = new Border
                {
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Black),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(0, 0, 0, 10),
                    Padding = new Thickness(12)
                };
                var reviewStack = new StackPanel { Spacing = 4 };
                string stars = new string('★', review.Rating) + new string('☆', 5 - review.Rating);
                var starsText = new TextBlock
                {
                    Text = stars,
                    FontSize = 20,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gold)
                };
                reviewStack.Children.Add(starsText);
                var dateText = new TextBlock
                {
                    Text = review.CreatedDate.ToShortDateString(),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray)
                };
                reviewStack.Children.Add(dateText);
                var commentText = new TextBlock
                {
                    Text = review.Content,
                    FontSize = 14
                };
                reviewStack.Children.Add(commentText);
                border.Child = reviewStack;
                ReviewsItemsControl.Items.Add(border);
            }
        }
    }

    public class ReviewModel
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public int Rating { get; set; } = 0;
        public string Comment { get; set; } = string.Empty;
    }
}