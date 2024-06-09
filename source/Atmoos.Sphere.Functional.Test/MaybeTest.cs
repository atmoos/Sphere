namespace Atmoos.Sphere.Functional.Test;

public sealed class MaybeTest : IFunctorLaws<String>, IEmptyFunctorLaws
{
    [Fact]
    public void TheIdentityFunctionHasNoEffect()
    {
        Maybe<String> value = "Hello, World!";
        Assert.Equal(value, value.Select(Identity));
    }

    [Theory]
    [MemberData(nameof(Values))]
    public void CompositionIsPreserved(String value)
    {
        Maybe<String> something = value;
        Assert.Equal(something.Select(Length).Select(IsEven), something.Select(s => IsEven(Length(s))));
    }

    [Fact]
    public void CompositionIsPreservedOnEmptyFunctor()
    {
        var empty = Just<String>.Nothing;
        Assert.Same(empty.Select(Length).Select(IsEven), empty.Select(s => IsEven(Length(s))));
    }

    public static TheoryData<String> Values() => new() {
         "uneven string",
         "An even string",
    };
}
