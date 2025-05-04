namespace Tests.CoraMockUps
{
    public class MockUpVerifier : IMockUpVerifier
    {
        public bool Verified { get; set; }

        public bool Verify2FAForSecret(byte[] twoFactorSecret, string token)
        {
            return Verified;
        }
    }
}
