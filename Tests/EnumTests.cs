using System;
using Xunit;
using ExhaustiveMatch;

namespace Tests
{
    public class EnumTests
    {
        [Fact]
        public void TestFunctions()
        {
            var a = MyEnum.A;
            var x = a.Match(
                whenA: () => 1,
                whenB: () => 2,
                whenC: () => 3);
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

    [GenerateMatch]
    public enum MyEnum
    {
        A, B, C
    }
}
