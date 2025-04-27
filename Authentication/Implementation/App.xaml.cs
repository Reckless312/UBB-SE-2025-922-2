using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace DrinkDb_Auth
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static Window? Window;

        public static Guid CurrentUserId { get; set; } = Guid.Empty;
        public static Guid CurrentSessionId { get; set; } = Guid.Empty;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs e)
        {
            Window = new MainWindow();
            Window.Activate();
        }

        private void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                var protocolArgs = args as ProtocolActivatedEventArgs;
                var uri = protocolArgs?.Uri;

                if (uri?.Scheme == "drink-db-auth")
                {
                    var tokenData = uri.Query;
                    // Handle the token data (e.g., extract the access token and use it)
                    Console.WriteLine($"Received token data: {tokenData}");
                }
            }
        }

        private void OnSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
