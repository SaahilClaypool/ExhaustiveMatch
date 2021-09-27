using System;

namespace Test
{
    public partial class Result<O, R>
    {
        public class Ok : Result<O, R>
        {
            public O Value { get; }
            public Ok(O value)
            {
                Value = value;
            }
        }
        public class Error : Result<O, R>
        {
            public R Value { get; }
            public Error(R value)
            {
                Value = value;
            }
        }

        // extensions

        public T Match<T>(Func<Ok, T> onOk, Func<Error, T> onError)
        {
            if (this is Ok o)
                onOk(o);
            else if (this is Error e)
                onError(e);
            throw new Exception("Uknown type hierarchy");
        }
    }

    public partial class MyEnum
    {
        public class A: MyEnum
        { }
        public class B: MyEnum
        { }
        public class C: MyEnum
        { }

        // -- generated
        public T Match<T>(Func<A, T> whenA, Func<B, T> whenB)
        {
            return this switch
            {
                A a => whenA(a),
                B b => whenB(b),
                _ => throw new Exception("Impossible")
            };
        }
    }
    
    public partial class R<TOk, TError>
    {
        Type Type => this.GetType();
        TOk OkValue;
        TError ErrorValue;

        public class Ok: R<TOk, TError>
        {
            public Ok(TOk ok)
            {
                this.OkValue = ok;
            }
        }

        public class Error: R<TOk, TError>
        {
            public Error(TError error)
            {
                this.ErrorValue = error;
            }
        }

        public void Match<Result>(Func<TOk, Result> whenOk, Func<TError, Result> whenError)
        {
            if (this is Ok o)
                whenOk(o.OkValue);
            else if (this is Error r)
                whenError(r.ErrorValue);
            else
                throw new Exception("Invalid type");
        }

        public static void Test()
        {
            var ok = new R<int, int>.Ok(1);
            ok.Match(ok => ok * 2, fail => fail * -2);
        }
    }
}