using System;
using DrinkDb_Auth.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace DrinkDb_Auth.View
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

        public RelayCommand? Command { get => command; set => this.command = value ?? new RelayCommand(new Action(() => { })); }

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
            this.contentDialog = new ContentDialog
            {
                Title = this.title,
                Content = this.content,
                CloseButtonText = this.closeButtonText,
                XamlRoot = window?.Content.XamlRoot,
                CloseButtonCommand = this.command,
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
