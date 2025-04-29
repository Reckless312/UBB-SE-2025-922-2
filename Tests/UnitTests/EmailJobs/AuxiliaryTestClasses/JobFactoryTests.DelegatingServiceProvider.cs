namespace YourNamespace.Tests
{
    public partial class JobFactoryTests
    {
        // Helper class to intercept GetService calls and delegate to mock
        private class DelegatingServiceProvider : IServiceProvider
        {
            private readonly IServiceProvider innerProvider;

            public DelegatingServiceProvider(IServiceProvider innerProvider)
            {
                this.innerProvider = innerProvider;
            }

            public object? GetService(Type serviceType)
            {
                return this.innerProvider.GetService(serviceType);
            }
        }
    }
}