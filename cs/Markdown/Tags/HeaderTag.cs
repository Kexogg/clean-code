namespace Markdown.Tags;

public class HeaderTag : ITag
{
    public string MdTag { get; } = "#";
    public string HtmlTag { get; } = "h1";
    public IReadOnlyCollection<ITag> DisallowedChildren { get; } = new List<ITag>();
}