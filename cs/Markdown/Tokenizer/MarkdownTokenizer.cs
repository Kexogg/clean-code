using System.Text;
using Markdown.Extensions;
using Markdown.Tags;
using Markdown.Tokenizer.Rules;
using Markdown.Tokenizer.Tokens;

namespace Markdown.Tokenizer;

/// <summary>
/// Токенайзер - переводит строку Markdown в токены. Учитывает правила
/// </summary>
/// <seealso cref="markdownRules"/>
public class MarkdownTokenizer : ITokenizer
{
    private readonly List<IToken> tokens = new List<IToken>();

    private readonly MarkdownRules markdownRules = new Rules.MarkdownRules();

    private readonly List<ITag> tags = new List<ITag>()
    {
        new StrongTag(),
        new CursiveTag(),
        new HeaderTag(),
        new ImageTag(),
        new NewLineTag()
    };

    public List<IToken> Tokenize(string content)
    {
        var escapePositions = GetEscapePositions(content);

        var foundTags = IdentifyTags(content);
        

        foreach (var line in foundTags)
        {
            //Console.WriteLine($"Found tags: {string.Join(", ", line.Select(x => $"{x.Tag.GetType()} at {x.Position}"))}");
            RemoveIllegalTags(line, content);
            //Console.WriteLine($"Clean tags: {string.Join(", ", line.Select(x => $"{x.Tag.GetType()} at {x.Position}"))}");
        }

        content = EscapeContent(content, escapePositions, foundTags);

        var cleanTags = foundTags.SelectMany(x => x).ToList();
        var tree = BuildTree(cleanTags, content);

        tokens.AddRange(tree);

        return tree;
    }

    private static string EscapeContent(string content, List<int> escapePositions, List<List<TagToken>> foundTags)
    {
        var sb = new StringBuilder(content);
        foreach (var pos in escapePositions.OrderByDescending(p => p))
        {
            sb.Remove(pos, 1);
        }
        content = sb.ToString();

        foreach (var tagToken in foundTags.SelectMany(tagTokens => tagTokens))
        {
            tagToken.Position -= escapePositions.Count(pos => pos < tagToken.Position);
        }

        return content;
    }

    private List<int> GetEscapePositions(string content)
    {
        var escapePositions = new List<int>();

        for (var i = 0; i < content.Length; i++)
        {
            if (content[i] != '\\') continue;
            if (i + 1 >= content.Length || (content[i + 1] != '\\' && !tags.Any(tag =>
                    content.ContainsSubstringOnIndex(tag.MdTag, i + 1) ||
                    (!tag.SelfClosing && content.ContainsSubstringOnIndex(tag.MdClosingTag, i + 1))))) continue;
            escapePositions.Add(i);
            i++;
        }

        return escapePositions;
    }

    private List<List<TagToken>> IdentifyTags(string content)
    {
        var lines = new List<List<TagToken>>();
        var tagTokens = new List<TagToken>();
        for (var i = 0; i < content.Length; i++)
        {
            foreach (var tag in tags)
            {
                if (content.ContainsSubstringOnIndex(tag.MdTag, i))
                {
                    if (tag is NewLineTag)
                    {
                        if (tagTokens.FirstOrDefault()?.Tag is not HeaderTag)
                            tagTokens.Add(new TagToken(tag) { Position = i });
                        lines.Add(tagTokens);
                        tagTokens = new List<TagToken>();
                        break;
                    }
                    
                    tagTokens.Add(new TagToken(tag) { Position = i });
                    i += tag.MdTag.Length - 1;
                    break;
                }
                else if (!tag.SelfClosing && content.ContainsSubstringOnIndex(tag.MdClosingTag, i))
                {
                    var tagToken = new TagToken(tag) { Position = i };
                    if (markdownRules.Rules.TryGetValue(tagToken.Tag.GetType(), out var rule))
                    {
                        if (rule.IsTag != null && !rule.IsTag(tagToken, content))
                        {
                            continue;
                        }
                    }

                    tagTokens.Add(new TagToken(tag) { Position = i });
                    i += tag.MdClosingTag.Length - 1;
                    break;
                }
            }
        }
        lines.Add(tagTokens);

        return lines;
    }

    private List<IToken> BuildTree(List<TagToken> tagTokens, string content)
    {
        var tree = new List<IToken>();
        var tagStack = new Stack<TagToken>();
        var lastTagEnd = 0;

        if (tagTokens.Count == 0)
        {
            tree.Add(new TextToken(content));
            return tree;
        }

        foreach (var tag in tagTokens)
        {
            //If tag is first in the content
            if (tagStack.Count == 0)
            {
                if (tag.Position > 0)
                {
                    var textToken = new TextToken(content.Substring(lastTagEnd, tag.Position - lastTagEnd));
                    tree.Add(textToken);
                }

                lastTagEnd = tag.Position + tag.Tag.MdTag.Length;
                if (!tag.Tag.SelfClosing) tagStack.Push(tag);
            }
            //If tag is closing
            else if (tagStack.Peek().Tag.GetType() == tag.Tag.GetType())
            {
                var currentTag = tagStack.Pop();
                //handle image
                if (currentTag.Tag is ImageTag imageTag)
                {
                    var imageToken = new TagToken(imageTag)
                    {
                        Position = currentTag.Position,
                        Attributes = ImageTag.GetHtmlRenderAttributes(content.Substring(lastTagEnd - 2, tag.Position - lastTagEnd + 2))
                    };
                    tree.Add(imageToken);
                    lastTagEnd = tag.Position + tag.Tag.MdClosingTag.Length;
                    continue;
                }
                
                var textToken = new TextToken(content.Substring(lastTagEnd,
                    tag.Position - lastTagEnd));


                currentTag.Children.Add(textToken);
                if (tagStack.Count == 0)
                {
                    tree.Add(currentTag);
                }
                else
                {
                    tagStack.Peek().Children.Add(currentTag);
                }

                var offset = tag.Tag.SelfClosing ? tag.Tag.MdTag.Length : tag.Tag.MdClosingTag.Length;
                lastTagEnd = tag.Position + offset;
            }
            else
            {
                var lastTagInStack = tagStack.Peek();

                var length = tag.Position - lastTagInStack.Position - lastTagInStack.Tag.MdTag.Length;
                if (length > 0)
                    lastTagInStack.Children.Add(new TextToken(
                        content.Substring(lastTagInStack.Position + lastTagInStack.Tag.MdTag.Length, length)));
                tagStack.Push(tag);
                lastTagEnd = tag.Position + tag.Tag.MdTag.Length;
            }
        }

        if (lastTagEnd < content.Length)
        {
            tree.Add(new TextToken(content[lastTagEnd..]));
        }

        return tree;
    }

    private void RemoveIllegalTags(List<TagToken> foundTags, string content)
    {
        var invalidTags = new HashSet<TagToken>();
        var tagStack = new Stack<TagToken>();
        var orderedTags = foundTags.OrderBy(t => t.Position).ToList();

        foreach (var currentTag in orderedTags)
        {
            var tag = currentTag.Tag;

            var isClosingTag = !tag.SelfClosing && content.ContainsSubstringOnIndex(tag.MdClosingTag, currentTag.Position)
                               && tagStack.Any(tagToken => tagToken.Tag.GetType() == currentTag.Tag.GetType());


            var tagType = currentTag.Tag.GetType();
            
            //Check if tag is valid
            if (markdownRules.Rules.TryGetValue(tagType, out var rule))
            {
                if (rule.IsValid != null && !rule.IsValid(currentTag, content, isClosingTag, orderedTags))
                {
                    invalidTags.Add(currentTag);
                    continue;
                }
            }

            //If tag is opening
            if (tagStack.Count == 0 || !currentTag.Tag.Matches(tagStack.Peek().Tag) && !tagStack.Any(t => t.Tag.Matches(currentTag.Tag)))
            {
                //Check if tag is disallowed by parent
                if (tagStack.Count > 0 && tagStack.Peek().Tag.DisallowedChildren
                        .Any(t => t.GetType() == currentTag.Tag.GetType()))
                {
                    Console.WriteLine(
                        $"Invalid tag: {currentTag.Tag.GetType()} at {currentTag.Position}. Reason: disallowed by parent ({tagStack.Peek().Tag.GetType()})");
                    invalidTags.Add(currentTag);
                    continue;
                }

                if (!currentTag.Tag.SelfClosing) tagStack.Push(currentTag);
            }
            
            //If tag is closing
            else
            {
                var openingTag = tagStack.Pop();

                //check if we have any children open
                if (!openingTag.Tag.Matches(currentTag.Tag))
                {
                    Console.WriteLine($"Tags are intersecting! {openingTag.Tag.GetType()} at {openingTag.Position} and {currentTag.Tag.GetType()} at {currentTag.Position}");
                    invalidTags.Add(openingTag);
                    while (tagStack.Count > 0 && !tagStack.Peek().Tag.Matches(currentTag.Tag))
                    {
                        invalidTags.Add(tagStack.Pop());
                    }
                    invalidTags.Add(currentTag);
                }
                var contentLength = currentTag.Position - (openingTag.Position + openingTag.Tag.MdTag.Length);
                if (contentLength <= 0)
                {
                    Console.WriteLine(
                        $"Invalid tag: {openingTag.Tag.GetType()} at {openingTag.Position}. Reason: empty content");
                    Console.WriteLine(
                        $"Invalid tag: {currentTag.Tag.GetType()} at {currentTag.Position}. Reason: empty content");
                    invalidTags.Add(openingTag);
                    invalidTags.Add(currentTag);
                }
            }
        }

        while (tagStack.Count > 0)
        {
            Console.WriteLine(
                $"Invalid tag: {tagStack.Peek().Tag.GetType()} at {tagStack.Peek().Position}. Reason: no closing tag");
            invalidTags.Add(tagStack.Pop());
        }

        foundTags.RemoveAll(t => invalidTags.Contains(t));
    }


    public List<IToken> GetTokens()
    {
        return tokens.ToList();
    }
}