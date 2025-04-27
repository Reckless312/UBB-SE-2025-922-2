namespace DrinkDb_Auth.Service.TwoFactor
{
    public interface IKeyGeneration
    {
        byte[] GenerateRandomKey(int keyLength);
    }
}
