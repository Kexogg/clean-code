using Markdown.Renderer;
using Markdown.Tokenizer;

namespace Markdown.Markdown;

/// <summary>
/// Конвертер markdown в HTML
/// </summary>
public class Md : IMd
{
    private readonly ITokenizer tokenizer = new MarkdownTokenizer();
    private readonly IRenderer renderer = new HtmlRenderer();
    public string Render(string md)
    {
        var tokens = tokenizer.Tokenize(md);
        return renderer.Render(tokens);
    }
}