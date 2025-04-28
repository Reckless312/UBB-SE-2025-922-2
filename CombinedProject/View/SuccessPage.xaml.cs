using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CombinedProject.View
{
    public sealed partial class SuccessPage : Page
    {
        public SuccessPage()
        {
            this.InitializeComponent();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate from SuccessPage to UserPage
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(UserPage));
            }
        }
    }
}