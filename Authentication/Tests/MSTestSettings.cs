using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
[assembly: InternalsVisibleTo("Tests")]