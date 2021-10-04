using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ExhaustiveMatch
{
    [Generator]
    public class ResultGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("result.cs", src);
        }

        public void Initialize(GeneratorInitializationContext context)
        {

#if DEBUG
            if (!Debugger.IsAttached)
            {
                // Debugger.Launch();
            }
#endif 
        }

        
        private const string src = @"
using System;
namespace ExhaustiveMatch
{
    public struct Option<T>
    {
        public T Value { get; set; }
        public bool HasValue { get; set; }

        private Option(T value, bool hasValue)
        {
            Value = value;
            HasValue = hasValue;
        }

        public Option(T value) : this(value, true)
        { }

        public TOut Match<TOut>(Func<T, TOut> some, Func<TOut> none) =>
            HasValue ? some(Value) : none();

        public void Match(Action<T> some, Action none)
        {
            if (HasValue)
                some(Value);
            else
                none();
        }

        public Option<TOut> Select<TOut>(Func<T, TOut> map) =>
            HasValue ? new Option<TOut>(map(Value)) : new Option<TOut>();

        public Option<TOut> Bind<TOut>(Func<T, Option<TOut>> bind) =>
            HasValue ? bind(Value) : new Option<TOut>();

        public T Unwrap() =>
            HasValue ? Value : throw new Exception(""Option was none"");


        public static implicit operator Option<T>(NoneOption _) => new Option<T>();
        public static implicit operator Option<T>(T value) => value != null ? Option.Some(value) : Option.None;
    }

    public readonly struct NoneOption
    { }

    public static class Option
    {
        public static Option<T> Some<T>(T value) => new Option<T>(value);
        public static NoneOption None { get; } = new NoneOption();

        public static bool IsSome<T>(Option<T> option) => option.Match(some => true, () => false);
        public static T Unwrap<T>(Option<T> option) => option.Unwrap();
    }


    public struct Result<TOk, TError>
    {
        public TOk Ok { get; set; }
        public TError Error { get; set; }
        public bool IsError { get; set; }

        private Result(TOk ok, TError error, bool isError)
        {
            Ok = ok;
            Error = error;
            IsError = isError;
        }
        public Result(TOk ok) : this(ok, default, false) { }
        public Result(TError error) : this(default, error, true) { }

        public Result<TOut, TError> Select<TOut>(Func<TOk, TOut> ok) => Select(ok, err => err);
        public Result<TOut, TOutErr> Select<TOut, TOutErr>(Func<TOk, TOut> ok, Func<TError, TOutErr> err)
        {
            if (IsError)
                return Result.Error(err(Error));
            return Result.Ok(ok(Ok));
        }

        public Result<TOut, TError> Bind<TOut>(Func<TOk, Result<TOut, TError>> ok) => Bind(ok, err => Result.Error(err));
        public Result<TOut, TOutError> Bind<TOut, TOutError>(Func<TOk, Result<TOut, TOutError>> ok, Func<TError, Result<TOut, TOutError>> err)
        {
            if (IsError)
                return err(Error);
            return ok(Ok);
        }

        public TOut Match<TOut>(Func<TOk, TOut> ok, Func<TError, TOut> err) =>
            !IsError ? ok(Ok) : err(Error);

        public TOk Unwrap() =>
            !IsError ? Ok : throw new Exception($""Result had error: {Error}"");

        public static implicit operator Result<TOk, TError>(DelayedOk<TOk> ok) =>
            new Result<TOk, TError>(ok.Value);

        public static implicit operator Result<TOk, TError>(DelayedError<TError> error) =>
            new Result<TOk, TError>(error.Value);

        public static implicit operator Result<TOk, TError>(TOk ok) =>
            new Result<TOk, TError>(ok);
    }

    public readonly struct DelayedOk<T>
    {
        public T Value { get; }

        public DelayedOk(T value)
        {
            Value = value;
        }
    }

    public readonly struct DelayedError<T>
    {
        public T Value { get; }

        public DelayedError(T value)
        {
            Value = value;
        }
    }

    public static class Result
    {
        public static DelayedOk<TOk> Ok<TOk>(TOk ok) =>
            new DelayedOk<TOk>(ok);

        public static DelayedError<TError> Error<TError>(TError error) =>
            new DelayedError<TError>(error);

        public static bool IsOk<T, R>(Result<T, R> result) => result.Match(ok => true, err => false);
        public static T Unwrap<T, R>(Result<T, R> result) => result.Unwrap();
    }
}
        ";
    }
}