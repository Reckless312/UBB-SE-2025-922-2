// <copyright file="UpgradeRequestsRepositoryTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UnitTests.UpgradeRequests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using DataAccess.Model.AdminDashboard;
    using DrinkDb_Auth.Repository.AdminDashboard;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    public class UpgradeRequestsRepositoryTests : IDisposable
    {
        private readonly string connectionString;
        private readonly UpgradeRequestsRepository repository;
        private readonly IDbConnectionFactory connectionFactory;

        public UpgradeRequestsRepositoryTests()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .Build();

            this.connectionString = configurationRoot.GetConnectionString("TestConnection");
            this.connectionFactory = new SqlConnectionFactory(this.connectionString);
            this.repository = new UpgradeRequestsRepository(this.connectionFactory);
            this.EnsureTableExists();
            this.CleanupTable();
        }

        public void Dispose()
        {
            this.CleanupTable();
        }

        [Fact]
        public void RetrieveAllUpgradeRequests_WhenEmpty_ReturnsEmptyList()
        {
            List<UpgradeRequest> result = this.repository.RetrieveAllUpgradeRequests();
            Assert.Empty(result);
        }

        private int fixedInsertTestRequest(Guid userId, string userName)
        {
            int requestId = 0;
            using var conn = new SqlConnection(this.connectionString);
            conn.Open();

            using (var cmdInsert = new SqlCommand(
                @"INSERT INTO UpgradeRequests (RequestingUserId, RequestingUserName) 
                  VALUES (@userId, @userName);
                  SELECT SCOPE_IDENTITY();", conn))
            {
                cmdInsert.Parameters.AddWithValue("@userId", userId);
                cmdInsert.Parameters.AddWithValue("@userName", userName);

                requestId = Convert.ToInt32(cmdInsert.ExecuteScalar());
            }
            conn.Close();
            return requestId;
        }

        [Fact]
        public void RetrieveAllUpgradeRequests_WithData_ReturnsAllRequests()
        {
            this.fixedInsertTestRequest(new Guid(), "User1");
            this.fixedInsertTestRequest(new Guid(), "User2");
            List<UpgradeRequest> result = this.repository.RetrieveAllUpgradeRequests();
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void RetrieveUpgradeRequestByIdentifier_NonExistentId_ReturnsNull()
        {
            UpgradeRequest result = this.repository.RetrieveUpgradeRequestByIdentifier(999);
            Assert.Null(result);
        }

        [Fact]
        public void RetrieveUpgradeRequestByIdentifier_ExistingId_ReturnsRequest()
        {
            int requestId = this.fixedInsertTestRequest(new Guid(), "TestUser");
            UpgradeRequest result = this.repository.RetrieveUpgradeRequestByIdentifier(requestId);
            Assert.NotNull(result);
            Assert.Equal("TestUser", result.RequestingUserDisplayName);
        }

        [Fact]
        public void RemoveUpgradeRequestByIdentifier_ExistingId_RemovesRequest()
        {
            int requestId = this.fixedInsertTestRequest(new Guid(), "RemoveTestUser");
            this.repository.RemoveUpgradeRequestByIdentifier(requestId);
            UpgradeRequest result = this.repository.RetrieveUpgradeRequestByIdentifier(requestId);
            Assert.Null(result);
        }

        [Fact]
        public void RemoveUpgradeRequestByIdentifier_NonExistentId_DoesNotThrow()
        {
            Exception exception = Record.Exception(() => this.repository.RemoveUpgradeRequestByIdentifier(999));
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_NullConnectionFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new UpgradeRequestsRepository(connectionFactory: null));
        }

        [Fact]
        public void LegacyConstructor_ValidConnectionString_CreatesRepositorySuccessfully()
        {
            Exception exception = Record.Exception(() => new UpgradeRequestsRepository("dummy_connection_string"));
            Assert.Null(exception);
        }

        [Fact]
        public void RetrieveAllUpgradeRequests_DbException_HandlesExceptionAndReturnsEmptyList()
        {
            Mock<IDbConnectionFactory> mockConnectionFactory = new Mock<IDbConnectionFactory>();
            mockConnectionFactory
                .Setup(f => f.CreateConnection());
            UpgradeRequestsRepository repository = new UpgradeRequestsRepository(mockConnectionFactory.Object);
            List<UpgradeRequest> result = repository.RetrieveAllUpgradeRequests();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void RemoveUpgradeRequestByIdentifier_DbException_HandlesException()
        {
            Mock<IDbConnectionFactory> mockConnectionFactory = new Mock<IDbConnectionFactory>();
            mockConnectionFactory
                .Setup(f => f.CreateConnection());
            UpgradeRequestsRepository repository = new UpgradeRequestsRepository(mockConnectionFactory.Object);
            Exception exception = Record.Exception(() => repository.RemoveUpgradeRequestByIdentifier(1));
            Assert.Null(exception);
        }

        [Fact]
        public void RetrieveUpgradeRequestByIdentifier_DbException_HandlesExceptionAndReturnsNull()
        {
            Mock<IDbConnectionFactory> mockConnectionFactory = new Mock<IDbConnectionFactory>();
            mockConnectionFactory.Setup(f => f.CreateConnection());
            UpgradeRequestsRepository repository = new UpgradeRequestsRepository(mockConnectionFactory.Object);
            UpgradeRequest result = repository.RetrieveUpgradeRequestByIdentifier(1);
            Assert.Null(result);
        }

        [Fact]
        public void RetrieveAllUpgradeRequests_WithBadConnectionString_HandlesExceptionAndReturnsEmptyList()
        {
            UpgradeRequestsRepository repository = new UpgradeRequestsRepository("Data Source=nonexistent;Initial Catalog=fake;User Id=wrong;Password=wrong;");
            List<UpgradeRequest> result = repository.RetrieveAllUpgradeRequests();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void RemoveUpgradeRequestByIdentifier_WithBadConnectionString_HandlesException()
        {
            UpgradeRequestsRepository repository = new UpgradeRequestsRepository("Data Source=nonexistent;Initial Catalog=fake;User Id=wrong;Password=wrong;");
            Exception exception = Record.Exception(() => repository.RemoveUpgradeRequestByIdentifier(1));
            Assert.Null(exception);
        }

        [Fact]
        public void RetrieveUpgradeRequestByIdentifier_WithBadConnectionString_HandlesExceptionAndReturnsNull()
        {
            UpgradeRequestsRepository repository = new UpgradeRequestsRepository("Data Source=nonexistent;Initial Catalog=fake;User Id=wrong;Password=wrong;");
            UpgradeRequest result = repository.RetrieveUpgradeRequestByIdentifier(1);
            Assert.Null(result);
        }

        private void CleanupTable()
        {
            using var conn = new SqlConnection(this.connectionString);
            conn.Open();
            using var cmd = new SqlCommand("DELETE FROM UpgradeRequests", conn);
            cmd.ExecuteNonQuery();
        }

        private void EnsureTableExists()
        {
            using var conn = new SqlConnection(this.connectionString);
            conn.Open();
            using var cmd = new SqlCommand(
                @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UpgradeRequests')
                BEGIN
                    CREATE TABLE UpgradeRequests (
                        RequestId INT PRIMARY KEY IDENTITY(1,1),
                        RequestingUserId UNIQUEIDENTIFIER NOT NULL,
                        RequestingUserName NVARCHAR(100) NOT NULL
                    )
                END", conn);
            cmd.ExecuteNonQuery();
        }

        //private int InsertTestRequest(int userId, string userName)
        //{
        //    int requestId = 0;
        //    using var conn = new SqlConnection(this.connectionString);
        //    conn.Open();

        //    using (var cmdInsert = new SqlCommand(
        //        @"INSERT INTO UpgradeRequests (RequestingUserId, RequestingUserName) 
        //          VALUES (@userId, @userName);
        //          SELECT SCOPE_IDENTITY();", conn))
        //    {
        //        cmdInsert.Parameters.AddWithValue("@userId", userId);
        //        cmdInsert.Parameters.AddWithValue("@userName", userName);

        //        requestId = Convert.ToInt32(cmdInsert.ExecuteScalar());
        //    }
        //    conn.Close();
        //    return requestId;
        //}
    }
}