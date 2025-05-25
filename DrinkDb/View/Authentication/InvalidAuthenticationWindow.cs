using System;
using DrinkDb_Auth.View.Authentication.Interfaces;
using DrinkDb_Auth.ViewModel.AdminDashboard.Components;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DrinkDb_Auth.View.Authentication
{
    public class InvalidAuthenticationWindow : IDialog
    {
        private ContentDialog? contentDialog;

        private string title;
        private string content;
        private string closeButtonText;
        private RelayCommand command;
        private Window? window;

        public ContentDialog? ContentDialog
        {
            get { return contentDialog; }
        }

        public RelayCommand? Command { get => command; set => command = value ?? new RelayCommand(new Action(() => { })); }

        public InvalidAuthenticationWindow(string title, string content, string closeButtonText, RelayCommand command, Window? window)
        {
            this.title = title;
            this.content = content;
            this.closeButtonText = closeButtonText;
            this.command = command;
            this.window = window;
        }

        public ContentDialog? CreateContentDialog()
        {
            contentDialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = closeButtonText,
                XamlRoot = window?.Content.XamlRoot,
                CloseButtonCommand = command,
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
