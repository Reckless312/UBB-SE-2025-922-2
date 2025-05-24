using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Service.Authentication.Interfaces;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
using DrinkDb_Auth.View;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace DrinkDb_Auth
{
    public sealed partial class UserPage : Page
    {
        private readonly IUserService userService;
        private readonly IAuthenticationService authenticationService;
        private readonly IReviewService reviewService;
        private User? currentUser;

        public UserPage()
        {
            this.InitializeComponent();
            this.Loaded += UserPage_Loaded;
            this.userService = App.Host.Services.GetRequiredService<IUserService>();
            this.authenticationService = App.Host.Services.GetRequiredService<IAuthenticationService>();
            this.reviewService = App.Host.Services.GetRequiredService<IReviewService>();
        }

        private void UserPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserData();
            LoadUserReviews();
        }

        private async void LoadUserData()
        {
            Guid currentUserId = App.CurrentUserId;

            if (currentUserId != Guid.Empty)
            {
                this.currentUser = await userService.GetUserById(currentUserId);

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
                NameTextBlock.Text = "No user logged in";
                UsernameTextBlock.Text = string.Empty;
                StatusTextBlock.Text = string.Empty;
            }

            RoleType userRole = this.currentUser?.AssignedRole ?? RoleType.User;

            bool isAdmin = userRole == RoleType.Admin;
            bool isManager = userRole == RoleType.Admin;

            if (!isAdmin && !isManager)
            {
                this.AdminDashboardButton.Visibility = Visibility.Collapsed;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.authenticationService.Logout();
            App.MainWindow.Close();
        }

        private void AdminDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                MainPage mainPage = App.Host.Services.GetRequiredService<MainPage>();
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

            List<Review> userReviews = (await this.reviewService.GetReviewsByUser(currentUserId))
                .Where(review => !review.IsHidden)
                .OrderByDescending(review => review.CreatedDate)
                .ToList();

            this.ReviewsItemsControl.Items.Clear();
            foreach (Review review in userReviews)
            {
                Border border = new Border
                {
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Black),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(0, 0, 0, 10),
                    Padding = new Thickness(12)
                };
                StackPanel reviewStack = new StackPanel { Spacing = 4 };
                string stars = new string('★', review.Rating) + new string('☆', 5 - review.Rating);
                TextBlock starsText = new TextBlock
                {
                    Text = stars,
                    FontSize = 20,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gold)
                };
                reviewStack.Children.Add(starsText);
                TextBlock dateText = new TextBlock
                {
                    Text = review.CreatedDate.ToShortDateString(),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray)
                };
                reviewStack.Children.Add(dateText);
                TextBlock commentText = new TextBlock
                {
                    Text = review.Content,
                    FontSize = 14
                };
                reviewStack.Children.Add(commentText);
                border.Child = reviewStack;
                this.ReviewsItemsControl.Items.Add(border);
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