//using System;
//using System.Threading.Tasks;
//using DrinkDb_Auth.Service.Authentication;
//using DrinkDb_Auth.Service.Authentication.Components;
//using DrinkDb_Auth.Service.Authentication.Interfaces;
//using DrinkDb_Auth.ViewModel.Authentication.Interfaces;
//using DrinkDb_Auth.View.Authentication.Interfaces;
//using DataAccess.Model.Authentication;
//using Tests.CoraMockUps;
//using Xunit;

//namespace TestTest.Service
//{
//    public sealed class TwoFactorAuthenticationServiceTests
//    {
//        private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
//        private readonly IKeyGeneration _keyGeneration;
//        private readonly ITwoFactorAuthenticationView _mockView;
//        private readonly IAuthenticationWindowSetup _mockSetup;
//        private readonly IDialog _mockDialog;

//        public TwoFactorAuthenticationServiceTests()
//        {
//            _keyGeneration = new OtpKeyGeneration();
//            _mockView = new MockAuthenticationWindow();
//            _mockSetup = new MockWindowSetup
//            {
//                FifthDigit = "5",
//                SecondDigit = "2",
//                ThirdDigit = "3",
//                FourthDigit = "4",
//                FirstDigit = "1",
//                SixthDigit = "6"
//            };
//            _mockDialog = new MockUpDialog();
//        }

//        [Fact]
//        public void UserNotFoundInDatabase()
//        {
//            var mockUpUserAdapter = new MockUpUserAdapter { User = null };
//            bool isFirstTimeSetup = true;

//            Assert.Throws<ArgumentException>(() =>
//                TwoFactorAuthenticationService.CreateInjectedInstance(
//                    null,
//                    TestUserId,
//                    isFirstTimeSetup,
//                    mockUpUserAdapter));
//        }

//        [Fact]
//        public void KeyGenerationFailedOnNewEntry()
//        {
//            var mockUpAdapter = new MockUpUserAdapter
//            {
//                User = new User
//                {
//                    UserId = TestUserId,
//                    PasswordHash = "passwordHash",
//                    TwoFASecret = "twoFactorSecret",
//                    Username = "username"
//                }
//            };
//            bool isFirstTimeSetup = true;

//            Assert.Throws<InvalidOperationException>(() =>
//                TwoFactorAuthenticationService.CreateInjectedInstance(
//                    null,
//                    TestUserId,
//                    isFirstTimeSetup,
//                    mockUpAdapter));
//        }

//        [Fact]
//        public async Task NewUserVerified2FactorCorrectly()
//        {
//            var mockUpAdapter = new MockUpUserAdapter
//            {
//                User = new User
//                {
//                    UserId = TestUserId,
//                    PasswordHash = "passwordHash",
//                    TwoFASecret = "twoFactorSecret",
//                    Username = "username"
//                },
//                UpdatedUser = true
//            };
//            var mockUpVerifier = new MockUpVerifier { Verified = true };
//            bool isFirstTimeSetup = true;

//            var injectedService = TwoFactorAuthenticationService.CreateInjectedInstance(
//                null,
//                TestUserId,
//                isFirstTimeSetup,
//                mockUpAdapter,
//                _keyGeneration,
//                mockUpVerifier,
//                _mockSetup,
//                _mockView,
//                _mockDialog,
//                _mockDialog);

//            bool result = await injectedService.SetupOrVerifyTwoFactor();

//            Assert.True(result);
//        }

//        [Fact]
//        public async Task NewUserVerifiedCorrectlyButFailedToUpdateDatabase()
//        {
//            var mockUpAdapter = new MockUpUserAdapter
//            {
//                User = new User
//                {
//                    UserId = TestUserId,
//                    PasswordHash = "passwordHash",
//                    TwoFASecret = "twoFactorSecret",
//                    Username = "username"
//                },
//                UpdatedUser = false
//            };
//            var mockUpVerifier = new MockUpVerifier { Verified = true };
//            bool isFirstTimeSetup = true;

//            var injectedService = TwoFactorAuthenticationService.CreateInjectedInstance(
//                null,
//                TestUserId,
//                isFirstTimeSetup,
//                mockUpAdapter,
//                _keyGeneration,
//                mockUpVerifier,
//                _mockSetup,
//                _mockView,
//                _mockDialog,
//                _mockDialog);

//            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
//                await injectedService.SetupOrVerifyTwoFactor());
//        }

//        [Fact]
//        public async Task AlreadyExistingUserVerified2FactorCorrectly()
//        {
//            var mockUpAdapter = new MockUpUserAdapter
//            {
//                User = new User
//                {
//                    UserId = TestUserId,
//                    PasswordHash = "passwordHash",
//                    TwoFASecret = "f7PS2+oa43HTzlPpEVM0ORbagICVPf5nLbJFGUoPoYeu8iqJABkaPudj",
//                    Username = "username"
//                }
//            };
//            var mockUpVerifier = new MockUpVerifier { Verified = true };
//            bool isFirstTimeSetup = false;

//            var injectedService = TwoFactorAuthenticationService.CreateInjectedInstance(
//                null,
//                TestUserId,
//                isFirstTimeSetup,
//                mockUpAdapter,
//                _keyGeneration,
//                mockUpVerifier,
//                _mockSetup,
//                _mockView,
//                _mockDialog,
//                _mockDialog);

//            bool result = await injectedService.SetupOrVerifyTwoFactor();

//            Assert.True(result);
//        }

//        [Fact]
//        public async Task TwoFactorVerificationFailed()
//        {
//            var mockUpAdapter = new MockUpUserAdapter
//            {
//                User = new User
//                {
//                    UserId = TestUserId,
//                    PasswordHash = "passwordHash",
//                    TwoFASecret = "f7PS2+oa43HTzlPpEVM0ORbagICVPf5nLbJFGUoPoYeu8iqJABkaPudj",
//                    Username = "username"
//                }
//            };
//            var mockUpVerifier = new MockUpVerifier { Verified = false };
//            bool isFirstTimeSetup = false;

//            var injectedService = TwoFactorAuthenticationService.CreateInjectedInstance(
//                null,
//                TestUserId,
//                isFirstTimeSetup,
//                mockUpAdapter,
//                _keyGeneration,
//                mockUpVerifier,
//                _mockSetup,
//                _mockView,
//                _mockDialog,
//                _mockDialog);

//            bool result = await injectedService.SetupOrVerifyTwoFactor();

//            Assert.False(result);
//        }
//    }
//}
