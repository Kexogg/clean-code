namespace Markdown.Tokenizer.Rules;

/// <summary>
/// Интерфейс правила парсинга
/// </summary>
public interface IRule
{
    Func<TagToken, string, bool, List<TagToken>, bool> IsValid { get; init; }
}