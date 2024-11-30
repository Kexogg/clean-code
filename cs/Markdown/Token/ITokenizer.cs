namespace Markdown.Token;

/// <summary>
/// Интерфейс токенайзера - переводчика строки в токены
/// </summary>
public interface ITokenizer
{
    public List<IToken> Tokenize(string content);
    
    public List<IToken> GetTokens();
}