using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CombinedProject
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            
            // Default to loading the admin panel on startup
            NavView.SelectedItem = NavView.MenuItems.First();
            NavigateToPage("AdminPanelPage");
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                // Handle settings navigation if needed
                return;
            }

            if (args.InvokedItemContainer is NavigationViewItem item && item.Tag != null)
            {
                NavigateToPage(item.Tag.ToString());
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        private void NavigateToPage(string pageName)
        {
            Type? pageType = null;

            // Map page names to actual page types
            switch (pageName)
            {
                case "AdminPanelPage":
                    // Replace with actual page type from App1
                    // pageType = typeof(AdminPanelPage);
                    break;
                case "AuthPage":
                    // Replace with actual page type from DrinkDb_Auth
                    // pageType = typeof(AuthPage);
                    break;
                // Add more cases for other pages
            }

            if (pageType != null)
            {
                ContentFrame.Navigate(pageType, null, new EntranceNavigationTransitionInfo());
            }
        }
    }
} 