using DrinkDb_Auth.Service;
using DrinkDb_Auth.Model;
using Tests.CoraMockUps;
using DrinkDb_Auth.View;
using DrinkDb_Auth.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System;
using DrinkDb_Auth.Service.TwoFactor;

namespace Tests
{
    [TestClass]
    public sealed class TestTwoFactorAuthenticationService
    {
        private static readonly Guid TestUserId = new Guid("11111111-1111-1111-1111-111111111111");
        private IKeyGeneration? keyGeneration;
        private ITwoFactorAuthenticationView? mockView;
        private IAuthenticationWindowSetup? mockSetup;
        private IDialog? mockDialog;

        [TestInitialize]
        public void Setup ()
        {
            keyGeneration = new OtpKeyGeneration();
            mockView = new MockAuthenticationWindow();
            mockSetup = new MockWindowSetup { FifthDigit = "5", SecondDigit = "2", ThirdDigit = "3", FourthDigit = "4", FirstDigit = "1", SixthDigit = "6" };
            mockDialog = new MockUpDialog();
        }

        [TestMethod]
        [DoNotParallelize]
        public void UserNotFoundInDatabase()
        {
            IMockUpUserAdapter mockUpUserAdapter = new MockUpUserAdapter();
            mockUpUserAdapter.User = null;

            bool isFirstTimeSetup = true;

            Assert.ThrowsException<ArgumentException>(() =>
            {
                TwoFactorAuthenticationService.CreateInjectedInstance(null, TestUserId, isFirstTimeSetup, new MockUpUserAdapter());
            });
        }

        [TestMethod]
        public void KeyGenerationFailedOnNewEntry()
        {
            IMockUpUserAdapter mockUpAdapter = new MockUpUserAdapter();
            mockUpAdapter.User = new User { UserId = TestUserId, PasswordHash = "passwordHash", TwoFASecret = "twoFactorSecret", Username = "username" };

            bool isFirstTimeSetup = true;

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                TwoFactorAuthenticationService injectedService = TwoFactorAuthenticationService.CreateInjectedInstance(null, TestUserId, isFirstTimeSetup, mockUpAdapter);
            });
        }

        [TestMethod]
        public async Task NewUserVerified2FactorCorrectly()
        {
            IMockUpUserAdapter mockUpAdapter = new MockUpUserAdapter();
            mockUpAdapter.User = new User { UserId = TestUserId, PasswordHash = "passwordHash", TwoFASecret = "twoFactorSecret", Username = "username" };
            mockUpAdapter.UpdatedUser = true;

            IMockUpVerifier mockUpVerifier = new MockUpVerifier();
            mockUpVerifier.Verified = true;

            bool isFirstTimeSetup = true;

            TwoFactorAuthenticationService injectedService = TwoFactorAuthenticationService.CreateInjectedInstance(null, TestUserId, isFirstTimeSetup, mockUpAdapter, keyGeneration, mockUpVerifier, mockSetup, mockView, mockDialog, mockDialog);

            bool result = await injectedService.SetupOrVerifyTwoFactor();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task NewUserVerifiedCorrectlyButFailedToUpdateDatabase()
        {
            IMockUpUserAdapter mockUpAdapter = new MockUpUserAdapter();
            mockUpAdapter.User = new User { UserId = TestUserId, PasswordHash = "passwordHash", TwoFASecret = "twoFactorSecret", Username = "username" };
            mockUpAdapter.UpdatedUser = false;

            IMockUpVerifier mockUpVerifier = new MockUpVerifier();
            mockUpVerifier.Verified = true;

            bool isFirstTimeSetup = true;

            TwoFactorAuthenticationService injectedService = TwoFactorAuthenticationService.CreateInjectedInstance(null, TestUserId, isFirstTimeSetup, mockUpAdapter, keyGeneration, mockUpVerifier, mockSetup, mockView, mockDialog, mockDialog);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                 await injectedService.SetupOrVerifyTwoFactor();
            });
        }

        [TestMethod]
        public async Task AlreadyExistingUserVerified2FactorCorrectly()
        {
            IMockUpUserAdapter mockUpAdapter = new MockUpUserAdapter();
            mockUpAdapter.User = new User { UserId = TestUserId, PasswordHash = "passwordHash", TwoFASecret = "f7PS2+oa43HTzlPpEVM0ORbagICVPf5nLbJFGUoPoYeu8iqJABkaPudj", Username = "username" };

            IMockUpVerifier mockUpVerifier = new MockUpVerifier();
            mockUpVerifier.Verified = true;

            bool isFirstTimeSetup = false;

            TwoFactorAuthenticationService injectedService = TwoFactorAuthenticationService.CreateInjectedInstance(null, TestUserId, isFirstTimeSetup, mockUpAdapter, keyGeneration, mockUpVerifier, mockSetup, mockView, mockDialog, mockDialog);

            bool result = await injectedService.SetupOrVerifyTwoFactor();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TwoFactorVerificationFailed()
        {
            IMockUpUserAdapter mockUpAdapter = new MockUpUserAdapter();
            mockUpAdapter.User = new User { UserId = TestUserId, PasswordHash = "passwordHash", TwoFASecret = "f7PS2+oa43HTzlPpEVM0ORbagICVPf5nLbJFGUoPoYeu8iqJABkaPudj", Username = "username" };

            IMockUpVerifier mockUpVerifier = new MockUpVerifier();
            mockUpVerifier.Verified = false;

            bool isFirstTimeSetup = false;

            TwoFactorAuthenticationService injectedService = TwoFactorAuthenticationService.CreateInjectedInstance(null, TestUserId, isFirstTimeSetup, mockUpAdapter, keyGeneration, mockUpVerifier, mockSetup, mockView, mockDialog, mockDialog);

            bool result = await injectedService.SetupOrVerifyTwoFactor();

            Assert.IsFalse(result);
        }
    }
}
