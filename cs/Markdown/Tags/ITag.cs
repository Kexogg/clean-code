namespace Markdown.Tags;

public interface ITag
{
    string MdTag { get; }

    string? MdClosingTag => null;

    string HtmlTag { get; }

    bool SelfClosing => false;

    ITag[] DisallowedChildren { get; }

    static string? GetRenderAttributes(string content) => null;

    //Добавить какой-то метод для валидации?
}