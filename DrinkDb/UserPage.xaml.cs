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
using Microsoft.Extensions.DependencyInjection;

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
using DataAccess.Service.Authentication;
using DataAccess.Service;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Service.Authentication.Interfaces;

namespace DrinkDb_Auth
{
    public sealed partial class UserPage : Page
    {
        private readonly IUserService userService;
        private readonly IAuthenticationService authenticationService;
        private readonly IReviewService reviewService;
        private User currentUser;
        private Exception initializationError;

        public UserPage()
        {
            this.InitializeComponent();
            this.Loaded += UserPage_Loaded;
            
            try
            {
                this.userService = App.Host.Services.GetRequiredService<IUserService>();
                this.authenticationService = App.Host.Services.GetRequiredService<IAuthenticationService>();
                this.reviewService = App.Host.Services.GetRequiredService<IReviewService>();
            }
            catch (Exception ex)
            {
                initializationError = ex;
            }
        }

        private async void UserPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (initializationError != null)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to initialize UserPage: {initializationError.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
            else
            {
                LoadUserData();
                LoadUserReviews();
            }
        }

        private async void LoadUserData()
        {
            // Retrieve the current user's ID from the static property.
            Guid currentUserId = App.CurrentUserId;

            // Check if the user ID is valid (not empty)
            if (currentUserId != Guid.Empty)
            {
                // Retrieve the user from the database using your UserService.
                currentUser = await userService.GetUserById(currentUserId);

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

            RoleType userRole = this.currentUser?.AssignedRole ?? RoleType.User;

            bool isAdmin = userRole == RoleType.Admin;
            bool isManager = userRole == RoleType.Manager;

            if (!isAdmin && !isManager)
            {
                this.AdminDashboardButton.Visibility = Visibility.Collapsed;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            authenticationService.Logout();
            App.MainWindow.Close();
        }

        private void AdminDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                var mainPage = App.Host.Services.GetRequiredService<MainPage>();
                this.Frame.Navigate(typeof(MainPage), mainPage);
            }
        }

        private async void LoadUserReviews()
        {
            Guid currentUserId = App.CurrentUserId;
            if (currentUserId == Guid.Empty)
            {
                return;
            }

            var userReviews = (await reviewService.GetReviewsByUser(currentUserId))
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