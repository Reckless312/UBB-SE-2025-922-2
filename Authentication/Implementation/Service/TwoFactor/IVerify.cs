namespace DrinkDb_Auth.Service
{
    public interface IVerify
    {
        bool Verify2FAForSecret(byte[] twoFactorSecret, string token);
    }
}
