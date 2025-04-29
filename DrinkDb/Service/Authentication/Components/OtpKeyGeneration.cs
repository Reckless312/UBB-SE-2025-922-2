using DrinkDb_Auth.Service.Authentication.Interfaces;

namespace DrinkDb_Auth.Service.Authentication.Components
{
    public class OtpKeyGeneration : IKeyGeneration
    {
        public byte[] GenerateRandomKey(int keyLength)
        {
            return OtpNet.KeyGeneration.GenerateRandomKey(keyLength);
        }
    }
}
