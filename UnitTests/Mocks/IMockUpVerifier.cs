using DataAccess.Service;
using DataAccess.Service.Authentication.Interfaces;
namespace Tests.CoraMockUps
{
    public interface IMockUpVerifier : IVerify
    {
        public bool Verified { get; set; }
    }
}
