// <copyright file="MainPage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.View
{
    using System;
    using System.ComponentModel;
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;
    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
    using DrinkDb_Auth.ViewModel.AdminDashboard;
    using Microsoft.UI.Text;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Controls.Primitives;
    using Microsoft.Extensions.DependencyInjection;
    using DataAccess.Service.AdminDashboard.Interfaces;
    using DataAccess.AutoChecker;
    using System.Threading.Tasks;

    /// <summary>
    /// a page.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Gets correspinding view model.
        /// </summary>
        public MainPageViewModel ViewModel { get; }

        /// <summary>
        /// main page initialization.
        /// </summary>
        /// <param name="reviewsService">given review service.</param>
        /// <param name="userService">given user service.</param>
        /// <param name="upgradeRequestsService">given update request service.</param>
        /// <param name="checkersService">given checker service.</param>
        /// <param name="autoCheck">given auto check.</param>
        public MainPage(
            IReviewService reviewsService,
            IUserService userService,
            IUpgradeRequestsService upgradeRequestsService,
            ICheckersService checkersService,
            IAutoCheck autoCheck)
        {
            this.InitializeComponent();

            ViewModel = new MainPageViewModel(
                reviewsService,
                userService,
                upgradeRequestsService,
                checkersService,
                autoCheck);

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            DataContext = ViewModel;

            Unloaded += MainPage_Unloaded;
            
            // Load data asynchronously
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await ViewModel.LoadAllData();
            }
            catch (Exception ex)
            {
                // Show error to user
                var dialog = new ContentDialog
                {
                    Title = "Error Loading Data",
                    Content = "There was an error loading the admin dashboard data. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.IsWordListVisible))
            {
                this.WordListPopup.Visibility = ViewModel.IsWordListVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void AppealsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is User selectedUser)
            {
                ViewModel.SelectedAppealUser = selectedUser;
                ShowAppealDetailsUI(sender);
            }
        }

        private void ShowAppealDetailsUI(object anchor)
        {
            Flyout flyout = new Flyout();
            StackPanel panel = new StackPanel { Padding = new Thickness(10) };
            TextBlock userInfo = new TextBlock
            {
                FontSize = 18,
            };
            userInfo.SetBinding(TextBlock.TextProperty, new Microsoft.UI.Xaml.Data.Binding
            {
                Path = new PropertyPath("UserStatusDisplay"),
                Source = ViewModel,
                Mode = Microsoft.UI.Xaml.Data.BindingMode.OneWay,
            });

            TextBlock reviewsHeader = new TextBlock
            {
                Text = "User Reviews:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 5),
            };

            ListView reviewsList = new ListView
            {
                MaxHeight = 200,
            };
            reviewsList.SetBinding(ItemsControl.ItemsSourceProperty, new Microsoft.UI.Xaml.Data.Binding
            {
                Path = new PropertyPath("UserReviewsFormatted"),
                Source = ViewModel,
                Mode = Microsoft.UI.Xaml.Data.BindingMode.OneWay,
            });

            Button banButton = new Button
            {
                Content = "Keep Ban",
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = ViewModel.KeepBanCommand,
            };

            Button appealButton = new Button
            {
                Content = "Accept Appeal",
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = ViewModel.AcceptAppealCommand,
            };

            Button closeButton = new Button
            {
                Content = "Close Appeal Case",
                HorizontalAlignment = HorizontalAlignment.Center,
                Command = ViewModel.CloseAppealCaseCommand,
            };

            closeButton.Click += (s, args) => { flyout.Hide(); };

            panel.Children.Add(userInfo);

            panel.Children.Add(reviewsHeader);
            panel.Children.Add(reviewsList);
            panel.Children.Add(banButton);
            panel.Children.Add(appealButton);
            panel.Children.Add(closeButton);

            flyout.Content = panel;
            flyout.Placement = FlyoutPlacementMode.Left;
            flyout.ShowAt((FrameworkElement)anchor);
        }

        private void RequestList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is UpgradeRequest selectedUpgradeRequest)
            {
                ViewModel.SelectedUpgradeRequest = selectedUpgradeRequest;
                ShowUpgradeRequestDetailsUI(sender);
            }
        }

        private void ShowUpgradeRequestDetailsUI(object anchor)
        {
            Flyout flyout = new Flyout();
            StackPanel panel = new StackPanel { Padding = new Thickness(10) };

            TextBlock userInfo = new TextBlock
            {
                Text = ViewModel.UserUpgradeInfo,
                FontSize = 18,
            };

            TextBlock reviewsHeader = new TextBlock
            {
                Text = "User Reviews:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 5),
            };

            ListView reviewsList = new ListView
            {
                ItemsSource = ViewModel.UserReviewsWithFlags,
                Height = 100,
            };

            panel.Children.Add(userInfo);
            panel.Children.Add(reviewsHeader);
            panel.Children.Add(reviewsList);

            flyout.Content = panel;
            flyout.Placement = FlyoutPlacementMode.Left;
            flyout.ShowAt((FrameworkElement)anchor);
        }

        private void AcceptUpgradeRequestButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int requestId)
            {
                ViewModel.HandleUpgradeRequest(true, requestId);
            }
        }

        private void DeclineUpgradeRequestButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int requestId)
            {
                ViewModel.HandleUpgradeRequest(false, requestId);
            }
        }

        private async void ReviewSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await ViewModel.FilterReviews(this.ReviewSearchTextBox.Text);
        }

        private async void BannedUserSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await ViewModel.FilterAppeals(this.BannedUserSearchTextBox.Text);
        }

        private void MenuFlyoutAllowReview_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Review review)
            {
                ViewModel.ResetReviewFlags(review.ReviewId);
            }
        }

        private void MenuFlyoutHideReview_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Review review)
            {
                ViewModel.HideReview(review.ReviewId);
            }
        }

        private void MenuFlyoutAICheck_Click_2(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Review review)
            {
                ViewModel.RunAICheck(review);
            }
        }

        private async void AddWord_Click(object sender, RoutedEventArgs e)
        {
            TextBox input = new TextBox { PlaceholderText = "Enter new word..." };

            ContentDialog dialog = new ContentDialog
            {
                Title = "Add New Word",
                Content = input,
                PrimaryButtonText = "Add Word",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot,
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                string newWord = input.Text.Trim();
                ViewModel.AddOffensiveWord(newWord);
            }
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(UserPage));
            }
        }
    }
}