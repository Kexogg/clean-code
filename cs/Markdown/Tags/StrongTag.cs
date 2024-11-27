namespace Markdown.Tags;

/// <summary>
/// Тег для полужирного текста
/// </summary>
public class StrongTag : ITag
{
    public string MdTag { get; } = "__";

    public string MdClosingTag => MdTag;

    public string HtmlTag { get; } = "strong";

    public IReadOnlyCollection<ITag> DisallowedChildren { get; } = new List<ITag>();
}