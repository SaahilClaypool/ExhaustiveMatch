using System;
using Xunit;
using ExhaustiveMatch;

namespace Tests
{
    public class ResultTests
    {
        [Fact]
        public void TestConstruction()
        {
            Result<int, string> result = Result.Ok(10);
            Assert.Equal("ok: 10", result.Match(ok: val => $"ok: {val}", err: bad => bad));
            result = Result.Error("err");
            Assert.Equal("err", result.Match(ok: val => $"ok: {val}", err: bad => bad));
        }

        
        [Fact]
        public void TestJson()
        {
            Result<int, string> result = Result.Ok(10);
            var st = ToJson(result);
            Console.WriteLine(st);
            Assert.Equal(result, FromJson<Result<int, string>>(st));
            result = Result.Error("err");
            Assert.NotEqual(result, FromJson<Result<int, string>>(st));
        }

        static string ToJson<T>(T thing) => System.Text.Json.JsonSerializer.Serialize(thing);
        static T FromJson<T>(string thing) => System.Text.Json.JsonSerializer.Deserialize<T>(thing);
    }
}
