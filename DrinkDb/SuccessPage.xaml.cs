using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Navigation;

namespace DrinkDb_Auth
{
    public sealed partial class SuccessPage : Page
    {
        private MainWindow mainWindow;

        public SuccessPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is MainWindow window)
            {
                this.mainWindow = window;
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.mainWindow != null)
            {
                this.mainWindow.NavigationFrame.Navigate(typeof(UserPage));
            }
        }
    }
}