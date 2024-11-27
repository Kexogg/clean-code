namespace Markdown.Token;

/// <summary>
/// Интерфейс токена
/// </summary>
public interface IToken
{
    string TextContent { get; }
    List<IToken>? Children { get; }
}