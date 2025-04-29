using DrinkDb_Auth.View;
using DrinkDb_Auth.ViewModel;
using Microsoft.UI.Xaml.Controls;
using System;
namespace Tests.CoraMockUps
{
    public class MockUpDialog : IDialog
    {
        public ContentDialog? ContentDialog => null;

        private RelayCommand command;

        public RelayCommand? Command { get => command; set => this.command = value ?? new RelayCommand(new Action(() => { })); }

        public MockUpDialog()
        {
            command = new RelayCommand(new Action(() => { }));
        }

        public ContentDialog? CreateContentDialog()
        {
            return null;
        }

        public void Hide()
        {
            return;
        }

        public void ShowAsync()
        {
            command?.Execute(this);
        }
    }
}
