using System;
using Xunit;
using ExhaustiveMatch;

namespace Tests
{
    public class TypeClass
    {
        [Fact]
        public void TestFunctions()
        {
        }

        [Fact]
        public void TestConstants()
        {
        }
    }

    [GenerateTypeClass]
    public abstract class MyTypeClass
    {
        public class A: MyTypeClass { };
        public class B: MyTypeClass { };
        public class C: MyTypeClass { };
    }
}
