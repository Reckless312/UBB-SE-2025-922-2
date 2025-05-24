using System;
using System.Threading.Tasks;
using DataAccess.AuthProviders.Twitter;
using DataAccess.OAuthProviders;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DrinkDb_Auth.ViewModel.Authentication
{
    internal class TwitterGuiHelper
    {
        private const string RedirectUri = "http://127.0.0.1:5000/x-callback";

        private Window parentWindow;
        private ITwitterOAuth2Provider authProvider;

        public TwitterGuiHelper(Window parent, ITwitterOAuth2Provider authProvider)
        {
            this.parentWindow = parent;
            this.authProvider = authProvider;
        }

        public async Task<AuthenticationResponse> SignInWithTwitterAsync()
        {
            TaskCompletionSource<AuthenticationResponse> twitterAuthenticationCompletion = new TaskCompletionSource<AuthenticationResponse>();

            try
            {
                ContentDialog twitterLoginDialog = new ContentDialog
                {
                    Title = "Sign in with Twitter",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.parentWindow.Content.XamlRoot,
                };

                WebView2 twitterLoginWebView = new WebView2
                {
                    Width = 450,
                    Height = 600,
                };
                twitterLoginDialog.Content = twitterLoginWebView;

                await twitterLoginWebView.EnsureCoreWebView2Async();

                twitterLoginWebView.CoreWebView2.NavigationStarting += async (sender, navigationArgs) =>
                {
                    string callbackUrl = navigationArgs.Uri;
                    System.Diagnostics.Debug.WriteLine($"NavigationStarting -> {callbackUrl}");

                    if (callbackUrl.StartsWith(RedirectUri, StringComparison.OrdinalIgnoreCase))
                    {
                        navigationArgs.Cancel = true;

                        string receivedAuthCode = this.authProvider.ExtractQueryParameter(callbackUrl, "code");
                        System.Diagnostics.Debug.WriteLine($"Found 'code' in callback: {receivedAuthCode}");

                        AuthenticationResponse twitterAuthResponse = await this.authProvider.ExchangeCodeForTokenAsync(receivedAuthCode);

                        this.parentWindow.DispatcherQueue.TryEnqueue(() =>
                        {
                            twitterLoginDialog.Hide();
                            twitterAuthenticationCompletion.SetResult(twitterAuthResponse);
                        });
                    }
                };

                twitterLoginWebView.CoreWebView2.Navigate(this.authProvider.GetAuthorizationUrl());

                ContentDialogResult dialogCompletionResult = await twitterLoginDialog.ShowAsync();

                if (!twitterAuthenticationCompletion.Task.IsCompleted)
                {
                    System.Diagnostics.Debug.WriteLine("Dialog closed; no code was returned.");
                    twitterAuthenticationCompletion.SetResult(new AuthenticationResponse
                    {
                        AuthenticationSuccessful = false,
                        OAuthToken = string.Empty,
                        SessionId = Guid.Empty,
                        NewAccount = false,
                    });
                }
            }
            catch (Exception twitterAuthenticationError)
            {
                System.Diagnostics.Debug.WriteLine($"SignInWithTwitterAsync error: {twitterAuthenticationError.Message}");
                twitterAuthenticationCompletion.TrySetException(twitterAuthenticationError);
            }

            return await twitterAuthenticationCompletion.Task;
        }
    }
}
