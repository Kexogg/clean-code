using Markdown.Renderer;
using Markdown.Token;

namespace Markdown.Markdown;

public class Md : IMd
{
    public string Render(string md)
    {
        var tokenizer = new Tokenizer();
        var tokens = tokenizer.Tokenize(md);
        var renderer = new HtmlRenderer();
        return renderer.RenderToHtml(tokens);
    }
}