using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace DrinkDb_Auth.ViewModel
{
    public static class AuthenticationTextBox
    {
        public static void TextBoxKeyEvent(object sender, KeyRoutedEventArgs @event, Microsoft.UI.Xaml.DependencyObject viewInstance)
        {
            TextBox? authentificationCodeTextBox = sender as TextBox;
            if (authentificationCodeTextBox == null)
            {
                return;
            }

            int numberOfMinimumCharacters = 0, numberOfMaximumCharacters = 1;

            if (@event.Key == Windows.System.VirtualKey.Back)
            {
                if (authentificationCodeTextBox.Text.Length == numberOfMinimumCharacters)
                {
                    MoveFocus(authentificationCodeTextBox, FocusNavigationDirection.Left, viewInstance);
                }
            }
            else
            {
                if (authentificationCodeTextBox.Text.Length == numberOfMaximumCharacters)
                {
                    MoveFocus(authentificationCodeTextBox, FocusNavigationDirection.Right, viewInstance);
                }
            }
        }

        private static void MoveFocus(TextBox authentificationCodeTextBox, FocusNavigationDirection direction, Microsoft.UI.Xaml.DependencyObject viewInstance)
        {
            AutomationPeer? automationPeer = FrameworkElementAutomationPeer.FromElement(authentificationCodeTextBox);
            ITextProvider? textProvider = automationPeer.GetPattern(PatternInterface.Text) as ITextProvider;

            FindNextElementOptions? options = new FindNextElementOptions
            {
                SearchRoot = viewInstance
            };

            FocusManager.TryMoveFocus(direction, options);
        }
    }
}
