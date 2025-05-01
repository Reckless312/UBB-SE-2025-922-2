namespace UnitTests.ReviewChecker.AuxiliaryTestsClasses
{
    using System;
    using System.IO;

    public class ConsoleOutput : IDisposable
    {
        private readonly StringWriter stringWriter;
        private readonly TextWriter originalOutput;

        public ConsoleOutput()
        {
            this.stringWriter = new StringWriter();
            this.originalOutput = Console.Out;
            Console.SetOut(this.stringWriter);
        }

        public string GetOutput()
        {
            return this.stringWriter.ToString();
        }

        public void Dispose()
        {
            Console.SetOut(this.originalOutput);
            this.stringWriter.Dispose();
        }
    }
}
