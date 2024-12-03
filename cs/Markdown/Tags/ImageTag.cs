namespace Markdown.Tags;

/// <summary>
/// Тег для картинки
/// </summary>
public class ImageTag : ITag
{
    public string MdTag { get; } = "![";

    public string MdClosingTag { get; } = ")";

    public string HtmlTag { get; } = "img";

    public IReadOnlyCollection<ITag> DisallowedChildren => new List<ITag>() {new CursiveTag(), new HeaderTag(), new ImageTag(), new StrongTag()};

    public bool SelfClosing { get; } = false;

    public static Dictionary<string,string> GetHtmlRenderAttributes(string content) => new()
    {
        {"src", content.Split(']')[1].Split('(')[1]},
        {"alt", content.Split(']')[0][2..]}
    };
}