namespace YourNamespace.Tests
{
    using Quartz;
    using System;

    public partial class JobFactoryTests
    {
        public interface IDisposableJob : IJob, IDisposable
        {
            void Dispose();
        }
    }
}