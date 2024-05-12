namespace Atmoos.Sphere.Text;

public readonly record struct LineMark
{
    public required String Tag { get; init; }
    public required String Name { get; init; }
    public override String ToString() => $"{Tag}{Name}";
}

public static class LineMarks
{
    public static class Markdown
    {
        public static LineMark Header(String name) => new() { Tag = "#", Name = $" {name}" };
        public static LineMark SubHeader(String name) => new() { Tag = "##", Name = $" {name}" };
        public static LineMark SubSubHeader(String name) => new() { Tag = "###", Name = $" {name}" };
        public static LineMark Code(String name) => new() { Tag = "```", Name = name };
    }

    public static class CSharp
    {
        public static LineMark LineComment(String name) => new() { Tag = "//", Name = $" {name}" };
        public static LineMark BlockComment(String name) => new() { Tag = "/*", Name = $" {name}" };
    }
}
