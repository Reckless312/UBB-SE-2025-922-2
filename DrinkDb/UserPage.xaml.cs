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
                UserService userService = new UserService();
                currentUser = userService.GetUserById(currentUserId);

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

            List<Role> userRoles = this.currentUser.AssignedRoles;

            bool isAdmin = false;

            foreach (Role role in userRoles)
            {
                if (role.RoleType == RoleType.Admin)
                {
                    isAdmin = true;
                }
            }

            if (!isAdmin)
            {
                this.AdminDashboardButton.Visibility = Visibility.Collapsed;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs eventArguments)
        {
            AuthenticationService.Logout();
            App.MainWindow.Close();
        }

        private void AdminDashboardButton_Click(object sender, RoutedEventArgs eventArguments)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(MainPage));
            }
        }

        private void LoadUserReviews()
        {
            // Constants for layout styling
            const int BORDER_THICKNESS_VALUE = 1;
            const int BORDER_CORNER_RADIUS = 8;
            const int BORDER_BOTTOM_MARGIN = 10;
            const int BORDER_PADDING = 12;
            const int STAR_RATING_MAX = 5;
            const int STAR_FONT_SIZE = 20;
            const int DATE_FONT_SIZE = 12;
            const int COMMENT_FONT_SIZE = 14;
            const int STACK_SPACING = 4;

            // Get the review service from DI
            IReviewService reviewService = (IReviewService)App.Host.Services.GetService(typeof(IReviewService));
            Guid currentUserId = App.CurrentUserId;
            if (currentUserId == Guid.Empty || reviewService == null)
            {
                return;
            }
            List<Review> userReviews = reviewService.GetReviewsByUser(currentUserId)
                .Where(review => !review.IsHidden)
                .OrderByDescending(review => review.CreatedDate)
                .ToList();
            ReviewsItemsControl.Items.Clear();
            foreach (Review review in userReviews)
            {
                Border border = new Border
                {
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Black),
                    BorderThickness = new Thickness(BORDER_THICKNESS_VALUE),
                    CornerRadius = new CornerRadius(BORDER_CORNER_RADIUS),
                    Margin = new Thickness(0, 0, 0, BORDER_BOTTOM_MARGIN),
                    Padding = new Thickness(BORDER_PADDING)
                };
                StackPanel reviewStack = new StackPanel { Spacing = STACK_SPACING };
                string stars = new string('★', review.Rating) + new string('☆', STAR_RATING_MAX - review.Rating);
                TextBlock starsText = new TextBlock
                {
                    Text = stars,
                    FontSize = STAR_FONT_SIZE,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gold)
                };
                reviewStack.Children.Add(starsText);
                var dateText = new TextBlock
                {
                    Text = review.CreatedDate.ToShortDateString(),
                    FontSize = DATE_FONT_SIZE,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray)
                };
                reviewStack.Children.Add(dateText);
                var commentText = new TextBlock
                {
                    Text = review.Content,
                    FontSize = COMMENT_FONT_SIZE
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