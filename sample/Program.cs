using Test;
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var x = MyEnum.A;
var y = x.Match(() => 'a', () => 'b', () =>'c');
Console.WriteLine(y);

var z = new MyClass.CaseA { X = 1 };
Console.WriteLine(z.Match(a => $"A.X: {a.X}", b => $"B.Y: {b.Y}"));

var rec = new MyRecord.CaseB(100);
Console.WriteLine(rec.Match(a => $"A.A: {a.A}", b => $"B.A: {b.A}"));