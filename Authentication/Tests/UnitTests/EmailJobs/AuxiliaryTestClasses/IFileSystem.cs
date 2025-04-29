namespace UnitTests.EmailJobs.AuxiliaryTestClasses
{
    public interface IFileSystem
    {
        string ReadAllText(string path);
    }
}