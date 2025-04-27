using System;
using DrinkDb_Auth.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace DrinkDb_Auth.View
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

        public RelayCommand? Command { get => command; set => this.command = value ?? new RelayCommand(new Action(() => { })); }

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
            this.contentDialog = new ContentDialog
            {
                Title = this.title,
                CloseButtonText = this.cancelButtonText,
                PrimaryButtonText = this.primaryButtonText,
                DefaultButton = ContentDialogButton.Primary,
                PrimaryButtonCommand = this.command,
                XamlRoot = this.window?.Content.XamlRoot,
                Content = this.view
            };

            return this.contentDialog;
        }

        public async void ShowAsync()
        {
            await this.contentDialog?.ShowAsync();
        }

        public void Hide()
        {
            this.contentDialog?.Hide();
        }
    }
}
