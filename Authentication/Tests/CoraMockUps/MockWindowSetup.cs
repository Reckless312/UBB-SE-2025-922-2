using DrinkDb_Auth.ViewModel;


namespace Tests.CoraMockUps
{
    public class MockWindowSetup : IAuthenticationWindowSetup
    {
        public required string FirstDigit { get; set; }
        public required string SecondDigit { get; set; }
        public required string ThirdDigit { get; set; }
        public required string FourthDigit { get; set; }
        public required string FifthDigit { get; set; }
        public required string SixthDigit { get; set; }
    }
}
