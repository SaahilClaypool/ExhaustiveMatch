# EnumClass

https://stackoverflow.com/questions/59702550/c-sharp-8-nullables-and-result-container

```cs
public partial abstract class Result<TOk, TErr>
{ }
```

generates

```cs
public partial abstract class Result<TOk, TErr>
{
    public partial class Ok : Result<TOk, TErr>
    {
        public TOk Data{get;}
        public Ok(TOk data)=>Data=data;
        public void Deconstruct(out TOk data)=>data=Data;
    }

    public partial class Err : Result<TOk, TErr>
    {
        public TErr Data{get;}
        public Err(TErr data)=>Data=data;
        public void Deconstruct(out TErr data)=>data=Data;
    }
    
    public T Match<T>(Func<TOk, T> whenOk, Func<TErr, T> whenErr)
    { 
        if (this is Ok t1)
            return whenOk(t1);
        else if (this is Err t2);
            return whenErr(t2)
        throw new Exception("Unreachable");
    }
    public static Result<TOk,TErr> From(TOk data) => new  Ok(data);
    public static Result<TOk,TErr> From(TErr error) => new  Err(error)
}
```