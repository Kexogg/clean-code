using Markdown.Markdown;

namespace Markdown;

class Program
{
    public static void Main()
    {
        var mdFile = File.ReadAllText("Markdown.md");
        var md = new Md();
        Console.WriteLine(md.Render(mdFile));
    }
}