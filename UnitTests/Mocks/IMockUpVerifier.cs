using DrinkDb_Auth.Service;
using DrinkDb_Auth.Service.Authentication.Interfaces;
namespace Tests.CoraMockUps
{
    public interface IMockUpVerifier : IVerify
    {
        public bool Verified { get; set; }
    }
}
