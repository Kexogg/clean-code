namespace Markdown.Token;

/// <summary>
/// Текстовый токен
/// </summary>
/// <param name="content">Текст</param>
public class TextToken(string content) : IToken
{
    public string TextContent { get; } = content;

    public List<IToken>? Children => null;
}