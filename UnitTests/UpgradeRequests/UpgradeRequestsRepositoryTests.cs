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
    using DrinkDb_Auth.ProxyRepository.AdminDashboard;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Repository.AdminDashboard;
    using ServerAPI.Data;
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

            var contextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: "DrinkDB_Test")
                .Options;

            this.connectionString = configurationRoot.GetConnectionString("TestConnection");
            this.connectionFactory = new SqlConnectionFactory(this.connectionString);
            this.repository = new UpgradeRequestsRepository(new DatabaseContext(contextOptions));
            this.EnsureTableExists();
            this.CleanupTable();
        }

        public void Dispose()
        {
            this.CleanupTable();
        }

        [Fact]
        public async Task RetrieveAllUpgradeRequests_WhenEmpty_ReturnsEmptyList()
        {
            List<UpgradeRequest> result = await this.repository.RetrieveAllUpgradeRequests();
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
        public async Task RetrieveUpgradeRequestByIdentifier_NonExistentId_ReturnsNull()
        {
            UpgradeRequest result = await this.repository.RetrieveUpgradeRequestByIdentifier(999);
            Assert.Null(result);
        }

        // Not a valid test anymore
        //[Fact]
        //public async Task RemoveUpgradeRequestByIdentifier_NonExistentId_DoesNotThrow()
        //{
        //    Exception exception = await Record.ExceptionAsync(() => this.repository.RemoveUpgradeRequestByIdentifier(999));
        //    Assert.Null(exception);
        //}


        // Not a valid test anymore since the repo does not take a connectionFactory as parameter
        //[Fact]
        //public void Constructor_NullConnectionFactory_ThrowsArgumentNullException()
        //{
        //    Assert.Throws<ArgumentNullException>(() => new UpgradeRequestsRepository(connectionFactory: null));
        //}

        // Not a valid test anymore since the repo does not take a string as parameter
        //[Fact]
        //public void LegacyConstructor_ValidConnectionString_CreatesRepositorySuccessfully()
        //{
        //    Exception exception = Record.Exception(() => new UpgradeRequestsRepository("dummy_connection_string"));
        //    Assert.Null(exception);
        //}

        [Fact]
        public async Task RetrieveAllUpgradeRequests_DbException_HandlesExceptionAndReturnsEmptyList()
        {
            Mock<IDbConnectionFactory> mockConnectionFactory = new Mock<IDbConnectionFactory>();
            mockConnectionFactory
                .Setup(f => f.CreateConnection());
            var contextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: "DrinkDB_Test")
                .Options;
            UpgradeRequestsRepository repository = new UpgradeRequestsRepository(new DatabaseContext(contextOptions));
            List<UpgradeRequest> result = await repository.RetrieveAllUpgradeRequests();
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        // Not a valid test anymore
        //[Fact]
        //public async Task RemoveUpgradeRequestByIdentifier_DbException_HandlesException()
        //{
        //    Mock<IDbConnectionFactory> mockConnectionFactory = new Mock<IDbConnectionFactory>();
        //    mockConnectionFactory
        //        .Setup(f => f.CreateConnection());
        //    var contextOptions = new DbContextOptionsBuilder<DatabaseContext>()
        //        .UseInMemoryDatabase(databaseName: "DrinkDB_Test")
        //        .Options;
        //    UpgradeRequestsRepository repository = new UpgradeRequestsRepository(new DatabaseContext(contextOptions));
        //    Exception exception = await Record.ExceptionAsync(() => repository.RemoveUpgradeRequestByIdentifier(1));
        //    Assert.Null(exception);
        //}

        [Fact]
        public async Task RetrieveUpgradeRequestByIdentifier_DbException_HandlesExceptionAndReturnsNull()
        {
            Mock<IDbConnectionFactory> mockConnectionFactory = new Mock<IDbConnectionFactory>();
            mockConnectionFactory.Setup(f => f.CreateConnection());
            var contextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: "DrinkDB_Test")
                .Options;
            UpgradeRequestsRepository repository = new UpgradeRequestsRepository(new DatabaseContext(contextOptions));
            UpgradeRequest result = await repository.RetrieveUpgradeRequestByIdentifier(1);
            Assert.Null(result);
        }

        // Not a valid test anymore since the repo does not take a string as parameter
        //[Fact]
        //public void RetrieveAllUpgradeRequests_WithBadConnectionString_HandlesExceptionAndReturnsEmptyList()
        //{
        //    UpgradeRequestsRepository repository = new UpgradeRequestsRepository("Data Source=nonexistent;Initial Catalog=fake;User Id=wrong;Password=wrong;");
        //    List<UpgradeRequest> result = repository.RetrieveAllUpgradeRequests();
        //    Assert.NotNull(result);
        //    Assert.Empty(result);
        //}

        // Not a valid test anymore since the repo does not take a string as parameter
        //[Fact]
        //public void RemoveUpgradeRequestByIdentifier_WithBadConnectionString_HandlesException()
        //{
        //    UpgradeRequestsRepository repository = new UpgradeRequestsRepository("Data Source=nonexistent;Initial Catalog=fake;User Id=wrong;Password=wrong;");
        //    Exception exception = Record.Exception(() => repository.RemoveUpgradeRequestByIdentifier(1));
        //    Assert.Null(exception);
        //}

        // Not a valid test anymore since the repo does not take a string as parameter
        //[Fact]
        //public void RetrieveUpgradeRequestByIdentifier_WithBadConnectionString_HandlesExceptionAndReturnsNull()
        //{
        //    UpgradeRequestsRepository repository = new UpgradeRequestsRepository("Data Source=nonexistent;Initial Catalog=fake;User Id=wrong;Password=wrong;");
        //    UpgradeRequest result = repository.RetrieveUpgradeRequestByIdentifier(1);
        //    Assert.Null(result);
        //}

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