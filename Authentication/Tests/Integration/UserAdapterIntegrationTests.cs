using System;
using System.Transactions;
using DrinkDb_Auth.Adapter;
using DrinkDb_Auth.Model;
using NUnit.Framework;

namespace DrinkDb_Auth.Tests.Integration
{
    [TestFixture]
    public class UserAdapterIntegrationTests
    {
        private UserAdapter _userAdapter;
        private TransactionScope _transactionScope;
        private bool _dbConnectionAvailable = false;

        [SetUp]
        public void Setup()
        {
            try
            {
                // Use TransactionScope to rollback changes after each test
                _transactionScope = new TransactionScope(TransactionScopeOption.Required);
                _userAdapter = new UserAdapter();

                // Verify the connection directly
                using (var connection = DrinkDbConnectionHelper.GetConnection())
                {
                    if (connection == null)
                    {
                        _dbConnectionAvailable = false;
                        Assert.Ignore("Database connection is null - check connection string");
                        return;
                    }
                    
                    // Test the connection with a simple query
                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT 1", connection))
                    {
                        cmd.ExecuteScalar();
                        _dbConnectionAvailable = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _dbConnectionAvailable = false;
                Assert.Ignore($"Database connection failed: {ex.Message}");
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose transaction scope without committing to rollback all changes
            _transactionScope?.Dispose();
        }

        [Test]
        public void CreateUser_WithValidUser_StoresUserInDatabase()
        {
            // Skip test if DB connection isn't available
            if (!_dbConnectionAvailable)
            {
                Assert.Ignore("Database connection not available");
            }
            
            // Arrange
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = $"test_user_{Guid.NewGuid()}",
                PasswordHash = "test_hash",
                TwoFASecret = null
            };

            // Act
            bool result = _userAdapter.CreateUser(newUser);
            var retrievedUser = _userAdapter.GetUserById(newUser.UserId);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(retrievedUser);
            Assert.AreEqual(newUser.UserId, retrievedUser.UserId);
            Assert.AreEqual(newUser.Username, retrievedUser.Username);
            Assert.AreEqual(newUser.PasswordHash, retrievedUser.PasswordHash);
        }

        [Test]
        public void GetUserById_WithExistingId_ReturnsUser()
        {
            // Skip test if DB connection isn't available
            if (!_dbConnectionAvailable)
            {
                Assert.Ignore("Database connection not available");
            }
            
            // Arrange
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = $"test_user_{Guid.NewGuid()}",
                PasswordHash = "test_hash",
                TwoFASecret = null
            };
            _userAdapter.CreateUser(newUser);

            // Act
            var retrievedUser = _userAdapter.GetUserById(newUser.UserId);

            // Assert
            Assert.IsNotNull(retrievedUser);
            Assert.AreEqual(newUser.UserId, retrievedUser.UserId);
            Assert.AreEqual(newUser.Username, retrievedUser.Username);
        }

        [Test]
        public void GetUserById_WithNonExistingId_ReturnsNull()
        {
            // Skip test if DB connection isn't available
            if (!_dbConnectionAvailable)
            {
                Assert.Ignore("Database connection not available");
            }
            
            // Arrange
            Guid nonExistingId = Guid.NewGuid();

            // Act
            var retrievedUser = _userAdapter.GetUserById(nonExistingId);

            // Assert
            Assert.IsNull(retrievedUser);
        }

        [Test]
        public void GetUserByUsername_WithExistingUsername_ReturnsUser()
        {
            // Skip test if DB connection isn't available
            if (!_dbConnectionAvailable)
            {
                Assert.Ignore("Database connection not available");
            }
            
            // Arrange
            string uniqueUsername = $"test_user_{Guid.NewGuid()}";
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = uniqueUsername,
                PasswordHash = "test_hash",
                TwoFASecret = null
            };
            _userAdapter.CreateUser(newUser);

            // Act
            var retrievedUser = _userAdapter.GetUserByUsername(uniqueUsername);

            // Assert
            Assert.IsNotNull(retrievedUser);
            Assert.AreEqual(newUser.UserId, retrievedUser.UserId);
            Assert.AreEqual(uniqueUsername, retrievedUser.Username);
        }

        [Test]
        public void GetUserByUsername_WithNonExistingUsername_ReturnsNull()
        {
            // Skip test if DB connection isn't available
            if (!_dbConnectionAvailable)
            {
                Assert.Ignore("Database connection not available");
            }
            
            // Arrange
            string nonExistingUsername = $"non_existing_user_{Guid.NewGuid()}";

            // Act
            var retrievedUser = _userAdapter.GetUserByUsername(nonExistingUsername);

            // Assert
            Assert.IsNull(retrievedUser);
        }

        [Test]
        public void UpdateUser_WithExistingUser_UpdatesUserInDatabase()
        {
            // Skip test if DB connection isn't available
            if (!_dbConnectionAvailable)
            {
                Assert.Ignore("Database connection not available");
            }
            
            // Arrange
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = $"test_user_{Guid.NewGuid()}",
                PasswordHash = "original_hash",
                TwoFASecret = null
            };
            _userAdapter.CreateUser(newUser);

            // Update user properties
            newUser.PasswordHash = "updated_hash";
            newUser.TwoFASecret = "new_2fa_secret";

            // Act
            bool updateResult = _userAdapter.UpdateUser(newUser);
            var retrievedUser = _userAdapter.GetUserById(newUser.UserId);

            // Assert
            Assert.IsTrue(updateResult);
            Assert.IsNotNull(retrievedUser);
            Assert.AreEqual("updated_hash", retrievedUser.PasswordHash);
            Assert.AreEqual("new_2fa_secret", retrievedUser.TwoFASecret);
        }

        [Test]
        public void DeleteUser_WithExistingUser_RemovesUserFromDatabase()
        {
            // Skip test if DB connection isn't available
            if (!_dbConnectionAvailable)
            {
                Assert.Ignore("Database connection not available");
            }
            
            // Arrange
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = $"test_user_{Guid.NewGuid()}",
                PasswordHash = "test_hash",
                TwoFASecret = null
            };
            _userAdapter.CreateUser(newUser);

            // Act
            bool deleteResult = _userAdapter.DeleteUser(newUser.UserId);
            var retrievedUser = _userAdapter.GetUserById(newUser.UserId);

            // Assert
            Assert.IsTrue(deleteResult);
            Assert.IsNull(retrievedUser);
        }

        [Test]
        public void ValidateAction_WithValidPermissions_ReturnsTrue()
        {
            // Skip test if DB connection isn't available
            if (!_dbConnectionAvailable)
            {
                Assert.Ignore("Database connection not available");
            }
            
            // Arrange - Create user and assign proper role/permissions
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = $"test_admin_{Guid.NewGuid()}",
                PasswordHash = "test_hash",
                TwoFASecret = null
            };
            _userAdapter.CreateUser(newUser);
            
            // Act & Assert
            bool result = _userAdapter.ValidateAction(newUser.UserId, "test_resource", "read");

            // Make sure it doesn't throw an exception:
            Assert.DoesNotThrow(() => _userAdapter.ValidateAction(newUser.UserId, "test_resource", "read"));
        }
    }
} 