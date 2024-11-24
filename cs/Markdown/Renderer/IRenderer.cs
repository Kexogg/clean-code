using Markdown.Token;

namespace Markdown.Renderer;

public interface IRenderer
{
    string RenderToHtml(IEnumerable<IToken> tokens);
}