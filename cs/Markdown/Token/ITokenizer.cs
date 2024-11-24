namespace Markdown.Token;

public interface ITokenizer
{
    public List<IToken> Tokenize(string content);
}