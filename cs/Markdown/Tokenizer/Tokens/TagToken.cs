using Markdown.Tags;

namespace Markdown.Tokenizer.Tokens;

/// <summary>
/// Токен тега
/// </summary>
/// <param name="content">Содержание</param>
/// <param name="tag">Название тега</param>
public class TagToken(ITag tag) : IToken
{
    public string? TextContent { get; }
    public List<IToken> Children { get; init; } = new List<IToken>();

    public Dictionary<string, string> Attributes = new Dictionary<string, string>();

    public int Position { get; set; }

    public ITag Tag { get; } = tag;
    public override string ToString()
    {
        return $"{Tag.GetType()} {TextContent}";
    }
}