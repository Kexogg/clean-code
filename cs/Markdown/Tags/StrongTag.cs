namespace Markdown.Tags;

public class StrongTag : ITag
{
    public string MdTag => "__";

    public string MdClosingTag => MdTag;

    public string HtmlTag => "strong";

    public ITag[] DisallowedChildren => [];
}