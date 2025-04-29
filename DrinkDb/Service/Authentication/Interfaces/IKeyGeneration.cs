namespace DrinkDb_Auth.Service.Authentication.Interfaces
{
    public interface IKeyGeneration
    {
        byte[] GenerateRandomKey(int keyLength);
    }
}
