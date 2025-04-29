namespace UnitTests.ReviewChecker.AuxiliaryTestsClasses
{
    using System;
    using System.Reflection;

    public class MethodSwapper : IDisposable
    {
        private readonly RuntimeMethodHandle originalMethodHandle;
        private readonly RuntimeMethodHandle replacementMethodHandle;

        public MethodSwapper(Type originalType, string originalMethodName, Type replacementType, string replacementMethodName)
        {
            MethodInfo? originalMethod = originalType.GetMethod(originalMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            MethodInfo? replacementMethod = replacementType.GetMethod(replacementMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            this.originalMethodHandle = originalMethod.MethodHandle;
            this.replacementMethodHandle = replacementMethod.MethodHandle;
        }

        public void Dispose()
        {
        }
    }
}
