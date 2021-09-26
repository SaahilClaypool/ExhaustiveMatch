using System;
using Xunit;
using ExhaustiveMatch;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var a = MyEnum.A;
            var x = a.Match(() => 1, () => 2, () => 3);
            Assert.Equal(1, x);
        }
    }

    [GenerateExhaustiveMatch]
    public enum MyEnum
    {
        A, B, C
    }
}
