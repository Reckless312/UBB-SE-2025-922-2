using DrinkDb_Auth.ViewModel.AdminDashboard.Components;
using Microsoft.UI.Xaml.Controls;

namespace DrinkDb_Auth.View.Authentication.Interfaces
{
    public interface IDialog
    {
        ContentDialog? CreateContentDialog();

        public RelayCommand? Command { get; set; }

        public ContentDialog? ContentDialog { get; }

        public void ShowAsync();

        public void Hide();
    }
}
