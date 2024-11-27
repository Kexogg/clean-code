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

    public bool SelfClosing { get; } = true;

    public static string GetHtmlRenderAttributes(string content) => throw new NotImplementedException();
}