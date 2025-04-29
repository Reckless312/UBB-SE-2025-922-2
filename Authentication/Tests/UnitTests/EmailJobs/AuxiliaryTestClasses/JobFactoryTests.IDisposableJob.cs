namespace YourNamespace.Tests
{
    using Quartz;

    public partial class JobFactoryTests
    {
        public interface IDisposableJob : IJob, IDisposable
        {
        }
    }
}