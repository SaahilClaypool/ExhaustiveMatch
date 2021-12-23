using ExhaustiveMatch;
namespace Test.With.Long.NameSpace;

[GenerateMatch]
public enum MyEnum
{
    A,
    B,
    C
}

[GenerateMatch]
public abstract class MyClass
{
    public class CaseA : MyClass
    {
        public int X { get; set; }
    }

    public class CaseB : MyClass
    {
        public int Y { get; set; }
    }
}

[GenerateMatch]
public record class MyRecord
{
    public record CaseA(int A) : MyRecord;
    public record CaseB(int A) : MyRecord;
}