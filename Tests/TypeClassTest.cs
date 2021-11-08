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
        public void TestRecords()
        {
            MyRecordEnum rec = new MyRecordEnum.CaseA();
            Assert.Equal("a", rec.Match(a => "a", b => b.Value.ToString()));
            rec = new MyRecordEnum.CaseB(100);
            Assert.Equal("100", rec.Match(a => "a", b => b.Value.ToString()));
        }

        [Fact]
        public void WorksWithGenerics()
        {
            var rec = new GenericResultExample<int, string>.Ok(100);
            Assert.Equal("100",
                rec.Match(ok => ok.Value.ToString(), error => error.Value.ToString()));
        }
    }

    [GenerateMatch]
    public abstract class MyTypeClass
    {
        public class A: MyTypeClass
        {
            public A(int x)
            {
                X = x;
            }
            public int X { get; }
        }
        public class B: MyTypeClass { };
        public class C: MyTypeClass { };

        public int IgnoreMe { get; set; }
    }

    [GenerateMatch]
    public abstract record MyRecordEnum
    {
        public record CaseA: MyRecordEnum { };
        public record CaseB(int Value): MyRecordEnum;
    }

    [GenerateMatch]
    public abstract record GenericResultExample<T, R>
    {
        public record Ok(T Value): GenericResultExample<T, R>;
        public record Error(R Value): GenericResultExample<T, R>;
    }
}
