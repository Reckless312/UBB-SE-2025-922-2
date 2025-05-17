using DataAccess.Service.Authentication.Interfaces;
using OtpNet;

namespace DrinkDb_Auth.Service.Authentication.Components
{
    public class Verify2FactorAuthenticationSecret : IVerify
    {
        public bool Verify2FAForSecret(byte[] twoFactorSecret, string token)
        {
            Totp? oneTimePassword = new Totp(twoFactorSecret);
            int previous = 1, future = 1;
            return oneTimePassword.VerifyTotp(token, out long _, new VerificationWindow(previous, future));
        }
    }
}
