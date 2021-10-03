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
            var t = new MyTypeClass.A(100);

            var result = t.Match(
                whenA: a => a.X.ToString(),
                whenB: b => "b",
                whenC: c => "c"
            );

            Assert.Equal("100", result);
        }

        [Fact]
        public void TestConstants()
        {
        }
    }

    [GenerateTypeClass]
    public abstract class MyTypeClass
    {
        public class A : MyTypeClass
        {
            public A(int x)
            {
                X = x;
            }
            public int X { get; }
        }
        public class B : MyTypeClass { };
        public class C : MyTypeClass { };

        public int IgnoreMe { get; set; }
    }
}
