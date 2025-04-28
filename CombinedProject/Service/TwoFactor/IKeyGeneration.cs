namespace CombinedProject.Service.TwoFactor
{
    public interface IKeyGeneration
    {
        byte[] GenerateRandomKey(int keyLength);
    }
}
