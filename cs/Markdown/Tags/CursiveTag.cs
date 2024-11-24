namespace Markdown.Tags;

public class CursiveTag : ITag
{
    public string MdTag => "_";

    public string MdClosingTag => MdTag;

    public string HtmlTag => "em";

    public ITag[] DisallowedChildren => [new StrongTag()];
}