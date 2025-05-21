using DataAccess.AuthProviders.Twitter;
using DataAccess.OAuthProviders;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrinkDb_Auth.ViewModel.Authentication
{
    internal class TwitterGuiHelper
    {
        // The same Callback/Redirect URI you registered in Twitter Developer Portal.
        // e.g. "http://127.0.0.1:5000/x-callback"
        private const string RedirectUri = "http://127.0.0.1:5000/x-callback";

        // Twitter endpoints:
        private const string AuthorizationEndpoint = "https://twitter.com/i/oauth2/authorize";
        private const string TokenEndpoint = "https://api.twitter.com/2/oauth2/token";
        private const string UserInfoEndpoint = "https://api.twitter.com/2/users/me";

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

                // Initialize the WebView2
                await twitterLoginWebView.EnsureCoreWebView2Async();

                // Listen for navigations
                twitterLoginWebView.CoreWebView2.NavigationStarting += async (sender, navigationArgs) =>
                {
                    string callbackUrl = navigationArgs.Uri;
                    System.Diagnostics.Debug.WriteLine($"NavigationStarting -> {callbackUrl}");

                    // If it's the redirect back to our loopback, we parse out the code
                    if (callbackUrl.StartsWith(RedirectUri, StringComparison.OrdinalIgnoreCase))
                    {
                        navigationArgs.Cancel = true; // don't actually navigate to 127.0.0.1 in the WebView

                        string receivedAuthCode = this.authProvider.ExtractQueryParameter(callbackUrl, "code");
                        System.Diagnostics.Debug.WriteLine($"Found 'code' in callback: {receivedAuthCode}");

                        AuthenticationResponse twitterAuthResponse = await this.authProvider.ExchangeCodeForTokenAsync(receivedAuthCode);

                        // Close the dialog and return
                        this.parentWindow.DispatcherQueue.TryEnqueue(() =>
                        {
                            twitterLoginDialog.Hide();
                            twitterAuthenticationCompletion.SetResult(twitterAuthResponse);
                        });
                    }
                };

                // Start the auth flow
                twitterLoginWebView.CoreWebView2.Navigate(this.authProvider.GetAuthorizationUrl());

                // Display Twitter login dialog
                ContentDialogResult dialogCompletionResult = await twitterLoginDialog.ShowAsync();

                // If user closed the dialog manually before we got a code
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
