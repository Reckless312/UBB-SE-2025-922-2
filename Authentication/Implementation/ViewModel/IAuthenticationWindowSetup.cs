namespace DrinkDb_Auth.ViewModel
{
    public interface IAuthenticationWindowSetup
    {
        public string FirstDigit { get; set; }
        public string SecondDigit { get; set; }
        public string ThirdDigit { get; set; }
        public string FourthDigit { get; set; }
        public string FifthDigit { get; set; }
        public string SixthDigit { get; set; }
    }
}
