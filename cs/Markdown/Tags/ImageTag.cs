namespace Markdown.Tags;

public class ImageTag : ITag
{
    //[alt](src)
    public string MdTag => "[";

    public string MdClosingTag => ")";

    public string HtmlTag => "img";

    public ITag[] DisallowedChildren => [new CursiveTag(), new HeaderTag(), new ImageTag(), new StrongTag()];

    public bool SelfClosing => true;

    public static string GetRenderAttributes(string content) => throw new NotImplementedException();
}