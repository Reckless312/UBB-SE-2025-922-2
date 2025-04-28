namespace CombinedProject.Service
{
    public interface IVerify
    {
        bool Verify2FAForSecret(byte[] twoFactorSecret, string token);
    }
}
