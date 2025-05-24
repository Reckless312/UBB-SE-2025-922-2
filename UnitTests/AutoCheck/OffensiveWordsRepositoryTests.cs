namespace UnitTests.Autocheck
{
    using System;
    using System.IO;
    using DataAccess.Repository.AdminDashboard;
    using DrinkDb_Auth.ProxyRepository.AdminDashboard;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Repository.AdminDashboard;
    using ServerAPI.Data;
    using Xunit;

    /// <summary>
    /// Contains integration tests for the <see cref="OffensiveWordsRepository"/> class.
    /// </summary>
    public class OffensiveWordsRepositoryTests : IDisposable
    {
        private readonly string connectionString;
        private readonly OffensiveWordsRepository repository;
        private readonly IDbConnectionFactory connectionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OffensiveWordsRepositoryTests"/> class.
        /// Sets up configuration, database connection, and clears the test table.
        /// </summary>
        public OffensiveWordsRepositoryTests()
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
            this.repository = new OffensiveWordsRepository(new DatabaseContext(contextOptions));
            this.EnsureTableExists();
            this.CleanupTable();
        }

        /// <summary>
        /// Performs cleanup after each test by clearing the test table.
        /// </summary>
        public void Dispose()
        {
            this.CleanupTable();
        }

        /// <summary>
        /// Verifies that loading words from an empty table returns an empty result set.
        /// </summary>
        [Fact]
        public void LoadOffensiveWords_WhenEmpty_ReturnsEmptySet()
        {
            var result = this.repository.LoadOffensiveWords();
            Assert.Empty(result);
        }

        /// <summary>
        /// Adds a word and verifies that it is returned by LoadOffensiveWords.
        /// </summary>
        [Fact]
        public void AddWord_ThenLoadOffensiveWords_ContainsWord()
        {
            this.repository.AddWord("troll");

            var result = this.repository.LoadOffensiveWords();

            Assert.Contains("troll", result, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Adds and then deletes a word, ensuring it is no longer returned.
        /// </summary>
        [Fact]
        public void DeleteWord_RemovesWord()
        {
            this.repository.AddWord("annoying");
            this.repository.DeleteWord("annoying");

            var result = this.repository.LoadOffensiveWords();

            Assert.DoesNotContain("annoying", result, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that the repository constructor throws an exception if the connection factory is null.
        /// </summary>
        // Not a valid test anymore
        //[Fact]
        //public void Constructor_NullConnectionFactory_ThrowsArgumentNullException()
        //{
        //    Assert.Throws<ArgumentNullException>(() => new OffensiveWordsRepository(null));
        //}

        /// <summary>
        /// Ensures that adding a null word does not throw an exception.
        /// </summary>
        [Fact]
        public async Task AddWord_NullWord_DoesNotThrow()
        {
            var exception = await Record.ExceptionAsync(() => this.repository.AddWord(null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Ensures that adding an empty string does not throw an exception.
        /// </summary>
        [Fact]
        public async Task AddWord_EmptyWord_DoesNotThrow()
        {
            var exception = await Record.ExceptionAsync(() => this.repository.AddWord(string.Empty));
            Assert.Null(exception);
        }

        /// <summary>
        /// Ensures that adding a whitespace-only string does not throw an exception.
        /// </summary>
        [Fact]
        public async Task AddWord_WhitespaceWord_DoesNotThrow()
        {
            var exception = await Record.ExceptionAsync(() => this.repository.AddWord("   "));
            Assert.Null(exception);
        }

        /// <summary>
        /// Ensures that deleting a null word does not throw an exception.
        /// </summary>
        [Fact]
        public async Task DeleteWord_NullWord_DoesNotThrow()
        {
            var exception = await Record.ExceptionAsync(() => this.repository.DeleteWord(null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Ensures that deleting an empty word does not throw an exception.
        /// </summary>
        [Fact]
        public async Task DeleteWord_EmptyWord_DoesNotThrow()
        {
            var exception = await Record.ExceptionAsync(() => this.repository.DeleteWord(string.Empty));
            Assert.Null(exception);
        }

        /// <summary>
        /// Ensures that deleting a whitespace-only word does not throw an exception.
        /// </summary>
        [Fact]
        public async Task DeleteWord_WhitespaceWord_DoesNotThrow()
        {
            var exception = await Record.ExceptionAsync(() => this.repository.DeleteWord("   "));
            Assert.Null(exception);
        }

        /// <summary>
        /// Ensures that deleting a word that does not exist does not throw an exception.
        /// </summary>
        [Fact]
        public async Task DeleteWord_NonExistentWord_DoesNotThrow()
        {
            var exception = await Record.ExceptionAsync(() => this.repository.DeleteWord("nonexistent"));
            Assert.Null(exception);
        }

        /// <summary>
        /// Clears all records from the OffensiveWords table.
        /// </summary>
        private void CleanupTable()
        {
            using SqlConnection conn = new SqlConnection(this.connectionString);
            conn.Open();
            using var cmd = new SqlCommand("DELETE FROM OffensiveWords", conn);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Ensures the OffensiveWords table exists before running tests.
        /// </summary>
        private void EnsureTableExists()
        {
            using var conn = new SqlConnection(this.connectionString);
            conn.Open();
            using var cmd = new SqlCommand(
                @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OffensiveWords')
                BEGIN
                    CREATE TABLE OffensiveWords (
                        Word NVARCHAR(100) PRIMARY KEY
                    )
                END", conn);
            cmd.ExecuteNonQuery();
        }
    }
}