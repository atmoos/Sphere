namespace Atmoos.Sphere.Functional;

/// <summary>
/// A simple monadic maybe type, to indicate that a result may or may not occur without indicating an error as such.
/// </summary>
/// <remarks>
/// Note that this type is almost homologous to a nullable type `T?`.
/// </remarks>
public abstract class Maybe<T> : IUnwrap<T>, IUnit<Maybe<T>, T>, IEquatable<Maybe<T>>
    where T : notnull
{
    private protected Maybe() { }
    public abstract Maybe<T> Where(Func<T, Boolean> predicate);
    public abstract Maybe<TResult> Select<TResult>(Func<T, TResult> selector)
        where TResult : notnull;
    public abstract Maybe<TResult> SelectMany<TResult>(Func<T, Maybe<TResult>> selector)
        where TResult : notnull;
    /// <inheritdoc/>
    public abstract T Exit<TError>(Func<String, TError> onErrors) where TError : Exception;
    /// <inheritdoc/>
    public abstract T Value(Func<T> fallback);
    public abstract Boolean Equals(Maybe<T>? other);
    public abstract override Int32 GetHashCode();
    public override Boolean Equals(Object? other) => other is Maybe<T> maybe && Equals(maybe);

    // Monadic return
    public static implicit operator Maybe<T>(T value) => new Just<T>(value);
    public static Maybe<T> operator |(Maybe<T> left, Maybe<T> right) => (left, right) switch {
        (Just<T>, _) => left,
        (_, Just<T>) => right,
        _ => left
    };
    public static Maybe<(T, T)> operator &(Maybe<T> left, Maybe<T> right) => left.SelectMany(l => right.Select(r => (l, r)));
}

/// <inheritdoc/>
public sealed class Just<T> : Maybe<T>, IUnwrap<Just<T>, T>
    where T : notnull
{
    private static readonly Nothing<T> nothing = new();
    private readonly T value;
    public static Maybe<T> Nothing => nothing;
    internal Just(T value) => this.value = value;
    public override Maybe<T> Where(Func<T, Boolean> predicate) => predicate(this.value) ? this : Nothing;
    public override Maybe<TResult> Select<TResult>(Func<T, TResult> selector) => selector(this.value);
    public override Maybe<TResult> SelectMany<TResult>(Func<T, Maybe<TResult>> selector) => selector(this.value);
    public override T Value(Func<T> fallback) => this.value;
    public override T Exit<TError>(Func<String, TError> onFailure) => this.value;
    public override Boolean Equals(Maybe<T>? other) => other is Just<T> just && this.value.Equals(just.value);
    public override Int32 GetHashCode() => HashCode.Combine(this.value, typeof(Just<T>));
    public override String? ToString() => this.value.ToString();

    /// <inheritdoc/>
    public static implicit operator T(Just<T> value) => value.value;
}

public sealed class Nothing<T> : Maybe<T>
    where T : notnull
{
    private static readonly Int32 hash = typeof(Nothing<T>).GetHashCode();
    private static readonly String nothing = $"No {typeof(T).Name}";
    internal Nothing() { }
    public override Maybe<T> Where(Func<T, Boolean> predicate) => this;
    public override Maybe<TResult> Select<TResult>(Func<T, TResult> selector) => Just<TResult>.Nothing;
    public override Maybe<TResult> SelectMany<TResult>(Func<T, Maybe<TResult>> selector) => Just<TResult>.Nothing;
    /// <inheritdoc/>
    public override T Value(Func<T> fallback) => fallback();
    /// <inheritdoc/>
    public override T Exit<TError>(Func<String, TError> onNothing) => throw onNothing(nameof(Nothing<T>));
    public override Boolean Equals(Maybe<T>? other) => other is Nothing<T>;
    public override Int32 GetHashCode() => hash;
    public override String? ToString() => nothing;
}
