// <copyright file="MainPage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1.Views
{
    using System;
    using System.ComponentModel;
    using App1.AutoChecker;
    using App1.Models;
    using App1.Services;
    using App1.ViewModels;
    using Microsoft.UI.Text;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Controls.Primitives;

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
        public MainPage(IReviewService reviewsService, IUserService userService, IUpgradeRequestsService upgradeRequestsService, ICheckersService checkersService, IAutoCheck autoCheck)
        {
            this.InitializeComponent();

            this.ViewModel = new MainPageViewModel(
                reviewsService,
                userService,
                upgradeRequestsService,
                checkersService,
                autoCheck);

            this.ViewModel.PropertyChanged += this.ViewModel_PropertyChanged;

            this.DataContext = this.ViewModel;

            this.Unloaded += this.MainPage_Unloaded;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.PropertyChanged -= this.ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.ViewModel.IsWordListVisible))
            {
                this.WordListPopup.Visibility = this.ViewModel.IsWordListVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void AppealsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is User selectedUser)
            {
                this.ViewModel.SelectedAppealUser = selectedUser;
                this.ShowAppealDetailsUI(sender);
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
                Source = this.ViewModel,
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
            reviewsList.SetBinding(ListView.ItemsSourceProperty, new Microsoft.UI.Xaml.Data.Binding
            {
                Path = new PropertyPath("UserReviewsFormatted"),
                Source = this.ViewModel,
                Mode = Microsoft.UI.Xaml.Data.BindingMode.OneWay,
            });

            Button banButton = new Button
            {
                Content = "Keep Ban",
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = this.ViewModel.KeepBanCommand,
            };

            Button appealButton = new Button
            {
                Content = "Accept Appeal",
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = this.ViewModel.AcceptAppealCommand,
            };

            Button closeButton = new Button
            {
                Content = "Close Appeal Case",
                HorizontalAlignment = HorizontalAlignment.Center,
                Command = this.ViewModel.CloseAppealCaseCommand,
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
                this.ViewModel.SelectedUpgradeRequest = selectedUpgradeRequest;
                this.ShowUpgradeRequestDetailsUI(sender);
            }
        }

        private void ShowUpgradeRequestDetailsUI(object anchor)
        {
            Flyout flyout = new Flyout();
            StackPanel panel = new StackPanel { Padding = new Thickness(10) };

            TextBlock userInfo = new TextBlock
            {
                Text = this.ViewModel.UserUpgradeInfo,
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
                ItemsSource = this.ViewModel.UserReviewsWithFlags,
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
                this.ViewModel.HandleUpgradeRequest(true, requestId);
            }
        }

        private void DeclineUpgradeRequestButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int requestId)
            {
                this.ViewModel.HandleUpgradeRequest(false, requestId);
            }
        }

        private void ReviewSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ViewModel.FilterReviews(this.ReviewSearchTextBox.Text);
        }

        private void BannedUserSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ViewModel.FilterAppeals(this.BannedUserSearchTextBox.Text);
        }

        private void MenuFlyoutAllowReview_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Review review)
            {
                this.ViewModel.ResetReviewFlags(review.ReviewId);
            }
        }

        private void MenuFlyoutHideReview_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Review review)
            {
                this.ViewModel.HideReview(review.UserId, review.ReviewId);
            }
        }

        private void MenuFlyoutAICheck_Click_2(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Review review)
            {
                this.ViewModel.RunAICheck(review);
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
                XamlRoot = this.XamlRoot,
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                string newWord = input.Text.Trim();
                this.ViewModel.AddOffensiveWord(newWord);
            }
        }
    }
}