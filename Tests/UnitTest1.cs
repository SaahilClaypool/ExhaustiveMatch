using System;
using Xunit;
using ExhaustiveMatch;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void TestFunctions()
        {
            var a = MyEnum.A;
            var x = a.Match(() => 1, () => 2, () => 3);
            Assert.Equal(1, x);
        }

        [Fact]
        public void TestConstants()
        {
            var a = MyEnum.B;
            var x = a.Match(1, 2, 3);
            Assert.Equal(2, x);
        }
    }

    [GenerateExhaustiveMatch]
    public enum MyEnum
    {
        A, B, C
    }
}
