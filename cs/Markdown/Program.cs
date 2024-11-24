namespace Markdown;

class Program
{
    static void Main(string[] args)
    {
        var mdFile = File.ReadAllText("Markdown.md");
        var md = new Markdown.Md();
        Console.WriteLine(md.Render(mdFile));
    }
}