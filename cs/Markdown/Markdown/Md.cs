using Markdown.Renderer;
using Markdown.Token;

namespace Markdown.Markdown;

/// <summary>
/// Конвертер markdown в HTML
/// </summary>
public class Md : IMd
{
    private readonly ITokenizer tokenizer = new Tokenizer();
    private readonly IRenderer renderer = new HtmlRenderer();
    public string Render(string md)
    {
        var tokens = tokenizer.Tokenize(md);
        return renderer.Render(tokens);
    }
}