namespace Markdown.Tags;

public class HeaderTag : ITag
{
    public string MdTag => "#";
    public string HtmlTag => "h1";
    public ITag[] DisallowedChildren => [];
}