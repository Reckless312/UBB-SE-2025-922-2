using System;
using DrinkDb_Auth.Model;
using DrinkDb_Auth.Service;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace DrinkDb_Auth
{
    public sealed partial class UserPage : Page
    {
        private static readonly AuthenticationService AuthenticationService = new ();
        private static readonly UserService UserService = new ();

        public UserPage()
        {
            this.InitializeComponent();
            LoadUserData();
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
                var user = userService.GetUserById(currentUserId);

                // Update UI with the retrieved data.
                if (user != null)
                {
                    NameTextBlock.Text = user.Username;
                    UsernameTextBlock.Text = "@" + user.Username;
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
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationService.Logout();
            App.Window?.Close();
        }

        public void LoadMockUserData()
        {
            User user = UserService.GetCurrentUser();
            string mockStatus = "Online";
            ReviewModel[] mockReviews =
            [
                new ReviewModel { Date = DateTime.Now.AddDays(-1), Rating = 5, Comment = "Great drink!" },
                new ReviewModel { Date = DateTime.Now.AddDays(-2), Rating = 4, Comment = "Good, but could be better." },
                new ReviewModel { Date = DateTime.Now.AddDays(-3), Rating = 3, Comment = "Average." }
            ];

            string[] mockDrinkList =
            [
                "Mojito",
                "Pina Colada",
                "Margarita",
                "Whiskey Sour"
            ];

            // Show user info
            NameTextBlock.Text = user.Username;
            UsernameTextBlock.Text = "@" + user.Username;
            StatusTextBlock.Text = $"Status: {mockStatus}";

            // Display each review in the ReviewsItemsControl
            foreach (var review in mockReviews)
            {
                // Create a simple border "card"
                var border = new Border
                {
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Black),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(0, 0, 0, 10),
                    Padding = new Thickness(12)
                };

                // A small stack to hold rating, date, and comment
                var reviewStack = new StackPanel { Spacing = 4 };

                // Star rating
                string stars = new string('★', review.Rating) + new string('☆', 5 - review.Rating);
                var starsText = new TextBlock
                {
                    Text = stars,
                    FontSize = 20,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gold)
                };
                reviewStack.Children.Add(starsText);

                // Date
                var dateText = new TextBlock
                {
                    Text = review.Date.ToShortDateString(),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray)
                };
                reviewStack.Children.Add(dateText);

                // Comment
                var commentText = new TextBlock
                {
                    Text = review.Comment,
                    FontSize = 14
                };
                reviewStack.Children.Add(commentText);

                border.Child = reviewStack;
                ReviewsItemsControl.Items.Add(border);
            }

            // Display each drink in the DrinklistItemsControl
            foreach (var drink in mockDrinkList)
            {
                var drinkText = new TextBlock
                {
                    Text = drink,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                DrinklistItemsControl.Items.Add(drinkText);
            }
        }

        private async void EditAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Edit Account",
                Content = "Account editing is not implemented yet.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot,
                Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
            };

            await dialog.ShowAsync();
        }
    }

    public class ReviewModel
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public int Rating { get; set; } = 0;
        public string Comment { get; set; } = string.Empty;
    }
}
