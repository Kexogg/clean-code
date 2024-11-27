using Markdown.Tags;

namespace Markdown.Token;

/// <summary>
/// Токен тега
/// </summary>
/// <param name="content">Содержание</param>
/// <param name="tag">Название тега</param>
public class TagToken(string content, ITag tag) : IToken
{
    public string TextContent { get; } = content;

    public List<IToken> Children { get; } = new List<IToken>();

    public ITag Tag { get; } = tag;
}