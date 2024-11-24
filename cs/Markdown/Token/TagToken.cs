using Markdown.Tags;

namespace Markdown.Token;

public class TagToken(string content, ITag tag) : IToken
{
    public string Content { get; } = content;
    public ITag Tag { get; } = tag;
}