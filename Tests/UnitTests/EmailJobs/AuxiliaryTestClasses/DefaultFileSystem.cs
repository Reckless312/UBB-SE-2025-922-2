namespace UnitTests.EmailJobs.AuxiliaryTestClasses
{
    using System.IO;

    public class DefaultFileSystem : IFileSystem
    {
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
    }
}