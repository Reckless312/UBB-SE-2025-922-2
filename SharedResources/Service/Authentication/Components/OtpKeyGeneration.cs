using DataAccess.Service.Authentication.Interfaces;

namespace DataAccess.Service.Authentication.Components
{
    public class OtpKeyGeneration : IKeyGeneration
    {
        public byte[] GenerateRandomKey(int keyLength)
        {
            return OtpNet.KeyGeneration.GenerateRandomKey(keyLength);
        }
    }
}
