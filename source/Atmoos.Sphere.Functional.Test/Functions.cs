namespace Atmoos.Sphere.Functional.Test;

public static class Functions
{
    public static T Identity<T>(T value) => value;
    public static Int32 Length(String value) => value.Length;
    public static Boolean IsEven(Int32 value) => value % 2 == 0;
}

