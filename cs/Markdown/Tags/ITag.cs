namespace Markdown.Tags;

/// <summary>
/// Интерфейс тега
/// </summary>
public interface ITag
{
    /// <summary>
    /// Тег, который используется в Markdown
    /// </summary>
    string MdTag { get; }

    /// <summary>
    /// Закрывающий тег в Markdown 
    /// </summary>
    string MdClosingTag { get; }

    /// <summary>
    /// Тег в HTML, соответсвующий тегу Markdown
    /// </summary>
    /// <see cref="MdTag"/>
    string HtmlTag { get; }

    /// <summary>
    /// Самозакрывание тега в HTML
    /// </summary>
    bool SelfClosing => false;

    /// <summary>
    /// Запрет на вложение дочерних элементов определенного типа
    /// </summary>
    IReadOnlyCollection<ITag> DisallowedChildren { get; }

    /// <summary>
    /// Получить атрибуты для рендера в HTML
    /// </summary>
    /// <param name="content">Содержание тега</param>
    /// <returns>Строка с атрибутами для вставки в тег</returns>
    static string? GetHtmlRenderAttributes(string content) => null;

    //Добавить какой-то метод для валидации?
}