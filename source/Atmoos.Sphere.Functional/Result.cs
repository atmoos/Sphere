
namespace Atmoos.Sphere.Functional;

public abstract class Result<T> : IFunctor<T>, IEquatable<Result<T>>
    where T : notnull
{
    private protected Result() { }
    public abstract Result<TResult> Select<TResult>(Func<T, TResult> selector)
        where TResult : notnull;
    public abstract Result<TResult> SelectMany<TResult>(Func<T, Result<TResult>> selector)
        where TResult : notnull;
    public abstract T Exit<TError>(Func<String, TError> onErrors) where TError : Exception;
    public abstract T Value(Func<T> fallback);
    public static Result<T> Failure(String message) => new Failure<T>(message);
    public abstract Boolean Equals(Result<T>? other);
    public override Boolean Equals(Object? obj) => obj is Result<T> result && Equals(result);
    public abstract override Int32 GetHashCode();
    // Monadic return
    public static implicit operator Result<T>(T value) => new Success<T>(value);
}

public sealed class Success<T> : Result<T>
    where T : notnull
{
    private readonly T value;
    internal Success(T value) => this.value = value;
    public override Result<TResult> Select<TResult>(Func<T, TResult> selector) => selector(this.value);
    public override Result<TResult> SelectMany<TResult>(Func<T, Result<TResult>> selector) => selector(this.value);
    public override T Exit<TError>(Func<String, TError> onErrors) => this.value;
    public override T Value(Func<T> fallback) => this.value;
    public override Boolean Equals(Result<T>? other) => other is Success<T> success && this.value.Equals(success.value);
    public override Int32 GetHashCode() => this.value.GetHashCode();

    public static implicit operator T(Success<T> success) => success.value;
}

public sealed class Failure<T> : Result<T>, ICountable<String>
    where T : notnull
{
    private readonly Stack<String> errors;
    public Int32 Count => this.errors.Count;
    internal Failure(String error) : this(new Stack<String>()) => this.errors.Push(error);
    internal Failure(Stack<String> errors) => this.errors = errors;
    public override Result<TResult> Select<TResult>(Func<T, TResult> selector) => new Failure<TResult>(this.errors);
    public override Result<TResult> SelectMany<TResult>(Func<T, Result<TResult>> selector) => new Failure<TResult>(this.errors);
    public override T Exit<TError>(Func<String, TError> onFailure) => throw onFailure(ErrorMessage());
    public override T Value(Func<T> fallback) => fallback();
    public override String? ToString() => $"{nameof(Failure<T>)}: {ErrorMessage("- ")}";
    public override Boolean Equals(Result<T>? other) => other is Failure<T> error && ReferenceEquals(this.errors, error.errors);
    public override Int32 GetHashCode() => this.errors.GetHashCode();
    public IEnumerator<String> GetEnumerator() => this.errors.GetEnumerator();
    private String ErrorMessage(String separator = "") => this.errors.Count switch {
        0 => String.Empty,
        1 => this.errors.Peek(),
        _ => String.Join($"{Environment.NewLine}{separator}", this.errors)
    };
}
