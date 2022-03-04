# GenerateMatch

Generate a `.Match` method to switch over types with a known set of cases.
The idea is to create expression-style switch statements that ensure all conditions are handled.

The supported types are:

- enums: `Match` will take function parameters for each case of the enum
- enumeration classes: The source generator will infer each internal class case
    - [feature is available in newer version of java](https://www.infoq.com/articles/java-sealed-classes/#:~:text=algebraic%20data%20types.\)-,Exhaustiveness,-Sealed%20classes%20like)
    
    ```cs
    [GenerateMatch]
    public abstract record Result<T, R>
    {
        public record Ok(T Value): Result<T, R>;
        public record Error(R Value): Result<T, R>;
    }

    var res = new Result<int, string>.Ok(100);
    Assert.Equal("100",
        res.Match(ok => ok.Value.ToString(), error => error.Value.ToString()));

    // -- generated -- 

    public static class GenericResultExampleMatchExtensions
    {
        public static TReturnType Match<TReturnType, T,  R>
            (this Result<T, R> t,
                Func<Result<T, R>.Ok,TReturnType> whenOk,
                Func<Result<T, R>.Error,TReturnType> whenError)
        {
            if (t is Result<T, R>.Ok t0)
                    return whenOk.Invoke(t0);
            if (t is Result<T, R>.Error t1)
                    return whenError.Invoke(t1);
            throw new Exception("Unreachable");
        }

        public static TReturnType Match<TReturnType, T,  R>
            (this Result<T, R> t,
                TReturnType whenOk,
                TReturnType whenError)
        {
            if (t is Result<T, R>.Ok t0)
                    return whenOk;
            if (t is Result<T, R>.Error t1)
                    return whenError;
            throw new Exception("Unreachable");
        }
    }

    ```
