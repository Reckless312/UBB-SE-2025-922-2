using DrinkDb_Auth.Service;
namespace Tests.CoraMockUps
{
    public interface IMockUpVerifier : IVerify
    {
        public bool Verified { get; set; }
    }
}
