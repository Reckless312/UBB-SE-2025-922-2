namespace YourNamespace.Tests
{
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Quartz;
    using Quartz.Spi;
    using System;
    using Xunit;

    public partial class JobFactoryTests
    {
        [Fact]
        public void NewJob_WhenServiceProviderReturnsJob_ReturnsJob()
        {
            Mock<IJob> mockJob = new Mock<IJob>();
            Type mockJobType = typeof(IJob);
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(mockJob.Object);
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            JobFactory jobFactory = new JobFactory(serviceProvider);
            Mock<IJobDetail> mockJobDetail = new Mock<IJobDetail>();
            mockJobDetail.Setup(jd => jd.JobType).Returns(mockJobType);
            Mock<TriggerFiredBundle> mockTriggerFiredBundle = new Mock<TriggerFiredBundle>(null, null, null, false, DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now);
            mockTriggerFiredBundle.Setup(tfb => tfb.JobDetail).Returns(mockJobDetail.Object);
            Mock<IScheduler> mockScheduler = new Mock<IScheduler>();
            IJob result = jobFactory.NewJob(mockTriggerFiredBundle.Object, mockScheduler.Object);
            Assert.Same(mockJob.Object, result);
        }

        [Fact]
        public void NewJob_WhenServiceProviderDoesNotReturnJob_ThrowsException()
        {
            Type mockJobType = typeof(IJob);
            ServiceProvider serviceProvider = new ServiceCollection().BuildServiceProvider();
            JobFactory jobFactory = new JobFactory(serviceProvider);
            Mock<IJobDetail> mockJobDetail = new Mock<IJobDetail>();
            mockJobDetail.Setup(jd => jd.JobType).Returns(mockJobType);
            Mock<TriggerFiredBundle> mockTriggerFiredBundle = new Mock<TriggerFiredBundle>(null, null, null, false, DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now);
            mockTriggerFiredBundle.Setup(tfb => tfb.JobDetail).Returns(mockJobDetail.Object);
            Mock<IScheduler> mockScheduler = new Mock<IScheduler>();
            Assert.Throws<InvalidOperationException>(() => jobFactory.NewJob(mockTriggerFiredBundle.Object, mockScheduler.Object));
        }

        [Fact]
        public void NewJob_WhenServiceProviderReturnsNonIJobType_ThrowsException()
        {
            NonJobClass nonJob = new NonJobClass();
            Type nonJobType = typeof(NonJobClass);
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(nonJob);
            serviceCollection.AddSingleton(nonJobType, nonJob);
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            JobFactory jobFactory = new JobFactory(serviceProvider);
            Mock<IJobDetail> mockJobDetail = new Mock<IJobDetail>();
            mockJobDetail.Setup(jd => jd.JobType).Returns(nonJobType);
            Mock<TriggerFiredBundle> mockTriggerFiredBundle = new Mock<TriggerFiredBundle>(null, null, null, false, DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now);
            mockTriggerFiredBundle.Setup(tfb => tfb.JobDetail).Returns(mockJobDetail.Object);
            Mock<IScheduler> mockScheduler = new Mock<IScheduler>();
            Assert.Throws<Exception>(() => jobFactory.NewJob(mockTriggerFiredBundle.Object, mockScheduler.Object));
        }

        [Fact]
        public void ReturnJob_WhenJobIsNotDisposable_DoesNothing()
        {
            Mock<IJob> mockJob = new Mock<IJob>();
            JobFactory jobFactory = new JobFactory(Mock.Of<IServiceProvider>());
            jobFactory.ReturnJob(mockJob.Object);
        }

        [Fact]
        public void ReturnJob_WhenJobIsDisposable_CallsDispose()
        {
            Mock<IDisposableJob> mockDisposableJob = new Mock<IDisposableJob>();
            JobFactory jobFactory = new JobFactory(Mock.Of<IServiceProvider>());
            jobFactory.ReturnJob(mockDisposableJob.Object);
            mockDisposableJob.Verify(j => j.Dispose(), Times.AtLeastOnce);
        }

        [Fact]
        public void NewJob_WhenServiceProviderReturnsNull_ThrowsException()
        {
            Type mockJobType = typeof(IJob);
            Mock<IServiceProvider> mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(It.Is<Type>(t => t == mockJobType))).Returns(null);
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(mockServiceProvider.Object);
            DelegatingServiceProvider serviceProvider = new DelegatingServiceProvider(mockServiceProvider.Object);
            JobFactory jobFactory = new JobFactory(serviceProvider);
            Mock<IJobDetail> mockJobDetail = new Mock<IJobDetail>();
            mockJobDetail.Setup(jd => jd.JobType).Returns(mockJobType);
            Mock<TriggerFiredBundle> mockTriggerFiredBundle = new Mock<TriggerFiredBundle>(null, null, null, false, DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now);
            mockTriggerFiredBundle.Setup(tfb => tfb.JobDetail).Returns(mockJobDetail.Object);
            Mock<IScheduler> mockScheduler = new Mock<IScheduler>();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => jobFactory.NewJob(mockTriggerFiredBundle.Object, mockScheduler.Object));
            Assert.IsType<string>(exception.Message);
        }

        [Fact]
        public void NewJob_WhenGetRequiredServiceThrowsInvalidOperationException_RethrowsException()
        {
            Type mockJobType = typeof(IJob);
            Mock<IServiceProvider> mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(It.Is<Type>(t => t == mockJobType))).Throws(new InvalidOperationException("Service not registered"));
            DelegatingServiceProvider serviceProvider = new DelegatingServiceProvider(mockServiceProvider.Object);
            JobFactory jobFactory = new JobFactory(serviceProvider);
            Mock<IJobDetail> mockJobDetail = new Mock<IJobDetail>();
            mockJobDetail.Setup(jd => jd.JobType).Returns(mockJobType);
            Mock<TriggerFiredBundle> mockTriggerFiredBundle = new Mock<TriggerFiredBundle>(null, null, null, false, DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now);
            mockTriggerFiredBundle.Setup(tfb => tfb.JobDetail).Returns(mockJobDetail.Object);
            Mock<IScheduler> mockScheduler = new Mock<IScheduler>();
            Assert.Throws<InvalidOperationException>(() => jobFactory.NewJob(mockTriggerFiredBundle.Object, mockScheduler.Object));
        }
    }
}