namespace Markdown.Token;

public class TextToken(string content) : IToken
{
    public string Content { get; } = content;
}