namespace Atmoos.Sphere.Functional;

public static class Result
{
    /// <summary>
    /// Using this method is only needed when T is an interface type. Use the implicit cast operator otherwise.
    /// </summary>
    ///<remarks>
    /// See <see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes#15104-conversion-operators">§15.10.4</see> in the C# language specification for more information.
    ///</remarks>
    public static Result<T> Success<T>(T value) where T : notnull => new Success<T>(value);
    public static Result<T> Failure<T>(String error) where T : notnull => new Failure<T>(error);
    public static Result<T> From<T>(this T? maybe, Func<String> onNull)
        where T : notnull => maybe ?? Failure<T>(onNull());
    public static Result<T> From<T>(Func<T> action)
        where T : notnull => From<T, Exception>(action, exception => exception.Message);
    public static Result<T> From<T, TException>(Func<T> action)
        where T : notnull where TException : Exception => From<T, TException>(action, exception => exception.Message);
    public static Result<T> From<T, TException>(Func<T> action, Func<TException, Boolean> predicate)
        where T : notnull where TException : Exception => From(action, predicate, exception => exception.Message);
    public static Result<T> From<T, TException>(Func<T> action, Func<TException, String> onError)
        where T : notnull where TException : Exception => From(action, _ => true, onError);
    public static Result<T> From<T, TException>(Func<T> action, Func<TException, Boolean> predicate, Func<TException, String> onError)
        where T : notnull where TException : Exception
    {
        try {
            return action();
        }
        catch (TException exception) when (predicate(exception)) {
            return new Failure<T>(onError(exception));
        }
    }
    public static void Deconstruct<T>(this Success<(T, T)> value, out T left, out T right) => (left, right) = ((T, T))value;
    public static void Deconstruct<T>(this Success<(T, T, T)> value, out T a, out T b, out T c) => (a, b, c) = ((T, T, T))value;
    public static void Deconstruct<T>(this Success<(T, T, T, T)> value, out T a, out T b, out T c, out T d) => (a, b, c, d) = ((T, T, T, T))value;
}

/// <summary>
/// A monadic result type. If something goes wrong, it will accumulate errors and return them all at once.
/// </summary>
/// <remarks>
/// This type indicates that not being able to compute a result value must be considered an error and should be appropriately handled.
/// </remarks>
public abstract class Result<T> : IUnwrap<T>, IUnit<Result<T>, T>, IEquatable<Result<T>>
    where T : notnull
{
    private protected Result() { }
    public abstract Result<TResult> Select<TResult>(Func<T, TResult> selector)
        where TResult : notnull;
    public abstract Result<TResult> SelectMany<TResult>(Func<T, Result<TResult>> selector)
        where TResult : notnull;

    /// <inheritdoc/>
    public abstract T Exit<TError>(Func<String, TError> onErrors) where TError : Exception;
    /// <inheritdoc/>
    public abstract T Value(Func<T> fallback);
    public abstract Boolean Equals(Result<T>? other);
    public override Boolean Equals(Object? obj) => obj is Result<T> result && Equals(result);
    public abstract override Int32 GetHashCode();
    protected abstract Result<T> Push(String error);
    // Monadic return
    public static implicit operator Result<T>(T value) => new Success<T>(value);
    // Accumulate errors
    public static Result<T> operator +(Result<T> result, String error) => result.Push(error);
    public static Result<(T, T)> operator &(Result<T> left, Result<T> right) => (left, right) switch {
        (Failure<T> lf, Failure<T> rf) => (lf + rf).Select(v => (v, v)),
        (_, _) => left.SelectMany(l => right.Select(r => (l, r))),
    };
    public static Result<T> operator |(Result<T> left, Result<T> right) => (left, right) switch {
        (Success<T>, _) => left,
        (_, Success<T>) => right,
        (Failure<T> lf, Failure<T> rf) => lf + rf,
        _ => left
    };
}

/// <summary>
/// A successful result. Guaranteed to have a value.
/// </summary>
public sealed class Success<T> : Result<T>, IUnwrap<Success<T>, T>
    where T : notnull
{
    private readonly T value;
    internal Success(T value) => this.value = value;
    public override Result<TResult> Select<TResult>(Func<T, TResult> selector) => selector(this.value);
    public override Result<TResult> SelectMany<TResult>(Func<T, Result<TResult>> selector) => selector(this.value);
    public override T Exit<TError>(Func<String, TError> onErrors) => this.value;

    /// <summary>
    /// Using this method is only needed when T is an interface type. Use the implicit cast operator otherwise.
    /// </summary>
    ///<remarks>
    /// See <see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes#15104-conversion-operators">§15.10.4</see> in the C# language specification for more information.
    ///</remarks>
    public T Value() => this.value;
    public override T Value(Func<T> fallback) => this.value;
    public override Boolean Equals(Result<T>? other) => other is Success<T> success && this.value.Equals(success.value);
    public override Int32 GetHashCode() => HashCode.Combine(this.value, typeof(Success<T>));
    public override String ToString() => $"{nameof(Success<T>)}: {this.value}";
    protected override Result<T> Push(String error) => new Failure<T>(error);

    /// <inheritdoc/>
    public static implicit operator T(Success<T> success) => success.value;
}

/// <summary>
/// A failed result. Check the accumulated errors, or throw an exception with <see cref="Exit"/>.
/// </summary>
public sealed class Failure<T> : Result<T>, ICountable<String>
    where T : notnull
{
    private readonly Stack<String> errors;
    public Int32 Count => this.errors.Count;
    internal Failure(String error) : this(new Stack<String>()) => this.errors.Push(error);
    internal Failure(Stack<String> errors) => this.errors = errors;
    public override Result<TResult> Select<TResult>(Func<T, TResult> selector) => new Failure<TResult>(this.errors);
    public override Result<TResult> SelectMany<TResult>(Func<T, Result<TResult>> selector) => new Failure<TResult>(this.errors);

    /// <inheritdoc/>
    public override T Exit<TError>(Func<String, TError> onFailure) => throw onFailure(ErrorMessage());
    /// <inheritdoc/>
    public override T Value(Func<T> fallback) => fallback();
    public override String ToString() => $"{nameof(Failure<T>)}: {ErrorMessage("- ")}";
    public override Boolean Equals(Result<T>? other) => other is Failure<T> error && ReferenceEquals(this.errors, error.errors);
    public override Int32 GetHashCode() => HashCode.Combine(this.errors, typeof(Failure<T>));
    public IEnumerator<String> GetEnumerator() => this.errors.GetEnumerator();
    public static Result<T> operator +(Failure<T> left, Failure<T> right)
         => new Failure<T>(new Stack<String>(right.Append("-- + --").Concat(left)));
    private String ErrorMessage(String separator = "") => this.errors.Count switch {
        0 => String.Empty,
        1 => this.errors.Peek(),
        _ => String.Join($"{Environment.NewLine}{separator}", this.errors)
    };
    protected override Result<T> Push(String error)
    {
        this.errors.Push(error);
        return this;
    }
}
