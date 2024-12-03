using Markdown.Extensions;
using Markdown.Tags;
using Markdown.Tokenizer.Rules;

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
        new ImageTag()
    };

    public List<IToken> Tokenize(string content)
    {
        var foundTags = IdentifyTags(content);
        Console.WriteLine(content);
        Console.WriteLine(
            $"Found tags: {string.Join(", ", foundTags.Select(x => $"{x.Tag.GetType()} at {x.Position}"))}");

        RemoveIllegalTags(foundTags, content);

        Console.WriteLine(
            $"Clean tags: {string.Join(", ", foundTags.Select(x => $"{x.Tag.GetType()} at {x.Position}"))}");

        var tree = BuildTree(foundTags, content);

        tokens.AddRange(tree);

        return tree;
    }

    private List<TagToken> IdentifyTags(string content)
    {
        var tagTokens = new List<TagToken>();
        for (var i = 0; i < content.Length; i++)
        {
            foreach (var tag in tags)
            {
                if (content.ContainsSubstringOnIndex(tag.MdTag, i))
                {
                    tagTokens.Add(new TagToken(tag) { Position = i });
                    i += tag.MdTag.Length - 1;
                    break;
                }
                else if (content.ContainsSubstringOnIndex(tag.MdClosingTag, i))
                {
                    tagTokens.Add(new TagToken(tag) { Position = i });
                    i += tag.MdClosingTag.Length - 1;
                    break;
                }
            }
        }

        return tagTokens;
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
            /*if (tag.Tag.SelfClosing)
            {
                var token = new TagToken(tag.Tag) { Position = tag.Position };
                tokens.Add(token);
            }
            else */

            //if tag is escaped
            if (content.IsEscaped(tag.Position))
            {
                //TODO: remove escape character

                continue;
            }

            //If tag is first in the content
            if (tagStack.Count == 0)
            {
                if (tag.Position > 0)
                {
                    var textToken = new TextToken(content.Substring(lastTagEnd, tag.Position - lastTagEnd));
                    tree.Add(textToken);
                }

                lastTagEnd = tag.Position + tag.Tag.MdTag.Length;
                tagStack.Push(tag);
            }
            //If tag is closing
            else if (tagStack.Peek().Tag.GetType() == tag.Tag.GetType())
            {
                var currentTag = tagStack.Pop();

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

                lastTagEnd = tag.Position + tag.Tag.MdClosingTag.Length;
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
            tree.Add(new TextToken(content.Substring(lastTagEnd)));
        }


        return tree;
    }

    private void RemoveIllegalTags(List<TagToken> foundTags, string content)
    {
        var invalidTags = new HashSet<TagToken>();
        var tagStack = new Stack<TagToken>();
        var orderedTags = foundTags.OrderBy(t => t.Position).ToList();
        var tagRules = new MarkdownRules().Rules;

        foreach (var currentTag in orderedTags)
        {
            var tag = currentTag.Tag;

            var isClosingTag = content.ContainsSubstringOnIndex(tag.MdClosingTag, currentTag.Position)
                               && tagStack.Any(tagToken => tagToken.Tag.GetType() == currentTag.Tag.GetType());


            var tagType = currentTag.Tag.GetType();
            if (tagRules.TryGetValue(tagType, out var rule))
            {
                if (!rule.IsValid(currentTag, content, isClosingTag, orderedTags))
                {
                    Console.WriteLine(
                        $"Invalid tag: {currentTag.Tag.GetType()} at {currentTag.Position}. Reason: rule");
                    invalidTags.Add(currentTag);
                    continue;
                }
            }

            if (tagStack.Count == 0 || !currentTag.Tag.Matches(tagStack.Peek().Tag))
            {
                if (tagStack.Count > 0 && tagStack.Peek().Tag.DisallowedChildren
                        .Any(t => t.GetType() == currentTag.Tag.GetType()))
                {
                    Console.WriteLine(
                        $"Invalid tag: {currentTag.Tag.GetType()} at {currentTag.Position}. Reason: disallowed by parent");
                    invalidTags.Add(currentTag);
                    continue;
                }

                tagStack.Push(currentTag);
            }
            else
            {
                var openingTag = tagStack.Pop();
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