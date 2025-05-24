using System;
using System.Threading.Tasks;
using DataAccess.OAuthProviders;
using DrinkDb_Auth.AuthProviders.Google;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DrinkDb_Auth.ViewModel.Authentication
{
    internal class GoogleGuiHelper
    {
        private Window parentWindow;
        private IGoogleOAuth2Provider googleProvider;

        public GoogleGuiHelper(Window parent, IGoogleOAuth2Provider googleProvider)
        {
            this.parentWindow = parent;
            this.googleProvider = googleProvider;
        }

        public async Task<AuthenticationResponse> SignInWithGoogleAsync()
        {
            TaskCompletionSource<AuthenticationResponse> taskResults = new TaskCompletionSource<AuthenticationResponse>();
            try
            {
                ContentDialog googleSubWindow = new ContentDialog
                {
                    Title = "Sign in with Google",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = parentWindow.Content.XamlRoot
                };

                WebView2 webView = new WebView2();
                webView.Width = 450;
                webView.Height = 600;
                googleSubWindow.Content = webView;

                await webView.EnsureCoreWebView2Async();
                bool authenticationCodeFound = false;
                string approvalPath = "accounts.google.com/o/oauth2/approval";
                string domTitle = "document.title";
                string domBodyText = "document.body.innerText";
                string javaScriptQuery = "document.querySelector('code') ? document.querySelector('code').innerText : ''";
                string pageSuccesContent = "Succes", pageCodeContent = "code", pageSuccesCode = "Success code=";
                string pageCodeContentWithEqualAtTheEnd = pageCodeContent + "=";
                webView.CoreWebView2.DOMContentLoaded += async (sender, args) =>
                {
                    try
                    {
                        string currentUrl = webView.CoreWebView2.Source;
                        string title = await webView.CoreWebView2.ExecuteScriptAsync(domTitle);
                        string javaScriptCode = @"(function() 
                                                {
                                                    const codeElement = document.querySelector('textarea.kHn9Lb');

                                                    if (codeElement) return codeElement.textContent;
                                    
                                                    const possibleCodeElements = document.querySelectorAll('code, pre, textarea, input[readonly]');

                                                    for (const el of possibleCodeElements)
                                                    {
                                                        const content = el.textContent || el.value;
                                                        if (content && content.length > 10) return content;
                                                    }
                                    
                                                    return '';
                                                })()";

                        switch (currentUrl.Contains(approvalPath))
                        {
                            case true:
                                string functionResult = await webView.CoreWebView2.ExecuteScriptAsync(javaScriptCode);
                                string trimmedResult = functionResult.Trim('"');

                                if (!string.IsNullOrEmpty(trimmedResult) && trimmedResult != "null" && !authenticationCodeFound)
                                {
                                    authenticationCodeFound = true;
                                    AuthenticationResponse authenticationResponse = await this.googleProvider.ExchangeCodeForTokenAsync(trimmedResult);
                                    this.parentWindow.DispatcherQueue.TryEnqueue(() =>
                                    {
                                        try
                                        {
                                            googleSubWindow.Hide();
                                        }
                                        catch
                                        {
                                        }
                                        taskResults.SetResult(authenticationResponse);
                                    });

                                    return;
                                }
                                break;
                            case false:
                                break;
                        }

                        string code = string.Empty;
                        if (title.Contains(pageSuccesContent) || title.Contains(pageCodeContent))
                        {
                            switch (title.Contains(pageSuccesCode))
                            {
                                case true:
                                    title = title.Replace("\"", string.Empty);
                                    code = title.Substring(title.IndexOf("Success code=") + "Success code=".Length);
                                    break;
                            }

                            switch (string.IsNullOrEmpty(code))
                            {
                                case true:
                                    string pageContent = await webView.CoreWebView2.ExecuteScriptAsync(domBodyText);
                                    if (pageContent.Contains(pageCodeContentWithEqualAtTheEnd))
                                    {
                                        int skipNumberOfElements = 5;
                                        int startIndex = pageContent.IndexOf(pageCodeContentWithEqualAtTheEnd) + skipNumberOfElements;
                                        int endIndex = pageContent.IndexOf("\"", startIndex);

                                        code = endIndex > startIndex ? code = pageContent.Substring(startIndex, endIndex - startIndex) : code;
                                    }

                                    string codeElement = await webView.CoreWebView2.ExecuteScriptAsync(javaScriptQuery);

                                    code = !string.IsNullOrEmpty(codeElement) && codeElement != "\"\"" ? codeElement.Trim('"') : code;
                                    break;
                            }

                            switch (!string.IsNullOrEmpty(code) && !authenticationCodeFound)
                            {
                                case true:
                                    authenticationCodeFound = true;
                                    AuthenticationResponse response = await this.googleProvider.ExchangeCodeForTokenAsync(code);
                                    parentWindow.DispatcherQueue.TryEnqueue(() =>
                                    {
                                        try
                                        {
                                            googleSubWindow.Hide();
                                        }
                                        catch
                                        {
                                        }
                                        taskResults.SetResult(response);
                                    });
                                    break;
                            }
                        }
                    }
                    catch
                    {
                    }
                };

                webView.NavigationCompleted += async (sender, args) =>
                {
                    try
                    {
                        string uniformResourceIdentifier = webView.Source?.ToString() ?? webView.CoreWebView2.Source;
                        if (uniformResourceIdentifier.Contains(approvalPath) && !authenticationCodeFound)
                        {
                            await Task.Delay(500);

                            string code = string.Empty;

                            string pageContent = await webView.CoreWebView2.ExecuteScriptAsync(domBodyText);

                            switch (pageContent.Contains(pageCodeContentWithEqualAtTheEnd))
                            {
                                case true:
                                    int skipNumberOfElements = 5;
                                    int startIndex = pageContent.IndexOf(pageCodeContentWithEqualAtTheEnd) + skipNumberOfElements;
                                    int endIndex = pageContent.IndexOf(" ", startIndex);

                                    code = endIndex > startIndex ? code = pageContent.Substring(startIndex, endIndex - startIndex).Replace("\"", string.Empty).Trim() : code;
                                    break;
                            }

                            string javaScriptArrayStream = "Array.from(document.querySelectorAll('code, .auth-code, input[readonly]')).map(el => el.innerText || el.value)";
                            string codeElements = await webView.CoreWebView2.ExecuteScriptAsync(javaScriptArrayStream);

                            switch (codeElements != "[]" && !string.IsNullOrEmpty(codeElements))
                            {
                                case true:
                                    string[] elements = codeElements.Trim('[', ']').Split(',');
                                    foreach (string element in elements)
                                    {
                                        string trimmedValue = element.Trim('"', ' ');
                                        if (!string.IsNullOrEmpty(trimmedValue) && trimmedValue.Length > 10)
                                        {
                                            code = trimmedValue;
                                            break;
                                        }
                                    }
                                    break;
                            }

                            switch (!string.IsNullOrEmpty(code) && !authenticationCodeFound)
                            {
                                case true:
                                    authenticationCodeFound = true;
                                    AuthenticationResponse response = await this.googleProvider.ExchangeCodeForTokenAsync(code);

                                    parentWindow.DispatcherQueue.TryEnqueue(() =>
                                    {
                                        try
                                        {
                                            googleSubWindow.Hide();
                                        }
                                        catch
                                        {
                                        }
                                        taskResults.SetResult(response);
                                    });
                                    break;
                            }
                        }
                    }
                    catch
                    {
                    }
                };

                string authorizationURL = this.googleProvider.GetAuthorizationUrl();
                webView.CoreWebView2.Navigate(authorizationURL);
                ContentDialogResult subWindowResults = await googleSubWindow.ShowAsync();

                if (!taskResults.Task.IsCompleted)
                {
                    taskResults.SetResult(new AuthenticationResponse { AuthenticationSuccessful = false, OAuthToken = string.Empty, SessionId = Guid.Empty, NewAccount = false });
                }
            }
            catch (Exception ex)
            {
                taskResults.TrySetException(ex);
            }

            return await taskResults.Task;
        }
    }
}
