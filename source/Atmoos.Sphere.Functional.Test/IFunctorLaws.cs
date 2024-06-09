namespace Atmoos.Sphere.Functional.Test;

public interface IEmptyFunctorLaws
{
    void CompositionIsPreservedOnEmptyFunctor();
}

public interface IFunctorLaws<TTestType>
    where TTestType : notnull
{
    void TheIdentityFunctionHasNoEffect();
    void CompositionIsPreserved(TTestType value);
}
