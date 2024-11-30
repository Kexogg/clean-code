using Markdown.Tags;
using System.Text;
using Markdown.Extensions;

namespace Markdown.Token;

/// <summary>
/// Токенайзер - переводчик строку Markdown в токены
/// </summary>
public class MarkdownTokenizer : ITokenizer
{
    private List<IToken> tokens = new List<IToken>();
    
    private readonly List<ITag> tags = new List<ITag>()
    {
        new StrongTag(),
        new CursiveTag(),
        new HeaderTag(),
        new ImageTag()
    };

    public List<IToken> Tokenize(string content)
    {
        var tokenStack = new Stack<TagToken>();
        var sb = new StringBuilder();
        var i = 0;
        var foundTag = false;
        while (i < content.Length)
        {
            foreach (var tag in tags)
            {
                //Check if on starting tag
                //TODO: Check for closing tag
                if (content.ContainsSubstringOnIndex(tag.MdTag, i))
                {
                    foundTag = true;
                    //Advance i to the end of tag opening/closing
                    i += tag.MdTag.Length;
                    //We found tag! Let's check if it's closing tag
                    if (tokenStack.Count > 0 && tokenStack.Peek().Tag == tag)
                    {
                        var token = tokenStack.Pop();
                        token.Children.Add(new TextToken(sb.ToString()));
                        sb.Clear();
                        if (tokenStack.Count == 0) tokens.Add(token);
                        else tokenStack.Peek().Children.Add(token);
                        break;
                    }
                    //Not a closing tag, add text token to children of previous tag (if exists) or to tokens
                    if (sb.Length > 0)
                    {
                        if (tokenStack.Count > 0)
                        {
                            //Add text token to children of previous tag
                            tokenStack.Peek().Children.Add(new TextToken(sb.ToString()));
                        }
                        else
                        {
                            //On root level - add text token to tokens
                            tokens.Add(new TextToken(sb.ToString()));
                        }
                        sb.Clear();
                    }
                    //Add new tag to stack
                    tokenStack.Push(new TagToken(tag));
                    break;
                }
            }
            //If we found tag, skip to next iteration
            if (foundTag)
            {
                foundTag = false;
                continue;
            }
            sb.Append(content[i]);
            i++;
        }

        if (sb.Length > 0)
        {
            tokens.Add(new TextToken(sb.ToString()));
        }
        
        return tokens;
    }

    public List<IToken> GetTokens()
    {
        return tokens.ToList();
    }
}