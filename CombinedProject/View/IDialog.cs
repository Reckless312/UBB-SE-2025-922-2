using CombinedProject.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace CombinedProject.View
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
