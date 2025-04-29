using System;
using DrinkDb_Auth.View.Authentication.Interfaces;
using DrinkDb_Auth.ViewModel.AdminDashboard.Components;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace DrinkDb_Auth.View.Authentication
{
    public class AuthenticationCodeWindow : IDialog
    {
        private ContentDialog? contentDialog;
        private string title;
        private string cancelButtonText;
        private string primaryButtonText;
        private RelayCommand command;
        private Window? window;
        private object view;

        public ContentDialog? ContentDialog
        {
            get { return contentDialog; }
        }

        public RelayCommand? Command { get => command; set => command = value ?? new RelayCommand(new Action(() => { })); }

        public AuthenticationCodeWindow(string title, string cancelButtonText, string primaryButtonText, RelayCommand command, Window? window, object view)
        {
            this.title = title;
            this.cancelButtonText = cancelButtonText;
            this.primaryButtonText = primaryButtonText;
            this.command = command;
            this.window = window;
            this.view = view;
        }

        public ContentDialog? CreateContentDialog()
        {
            contentDialog = new ContentDialog
            {
                Title = title,
                CloseButtonText = cancelButtonText,
                PrimaryButtonText = primaryButtonText,
                DefaultButton = ContentDialogButton.Primary,
                PrimaryButtonCommand = command,
                XamlRoot = window?.Content.XamlRoot,
                Content = view
            };

            return contentDialog;
        }

        public async void ShowAsync()
        {
            await contentDialog?.ShowAsync();
        }

        public void Hide()
        {
            contentDialog?.Hide();
        }
    }
}
