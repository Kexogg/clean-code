namespace Markdown.Tokenizer;

/// <summary>
/// Текстовый токен. Содержит чистый текст
/// </summary>
/// <param name="content">Текст</param>
public class TextToken(string content) : IToken
{
    public string TextContent { get; } = content;

    public List<IToken>? Children => null;
    
    public override string ToString()
    {
        return TextContent;
    }
}