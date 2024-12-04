using System.Text;
using Markdown.Extensions;
using Markdown.Tags;
using Markdown.Tokenizer.Rules;
using Markdown.Tokenizer.Tokens;

namespace Markdown.Tokenizer;

/// <summary>
/// Токенайзер - переводит строку Markdown в токены. Учитывает правила
/// </summary>
/// <seealso cref="markdownRules" />
public class MarkdownTokenizer : ITokenizer
{
    private readonly MarkdownRules markdownRules = new();

    private readonly List<ITag> tags = new()
    {
        new StrongTag(),
        new CursiveTag(),
        new HeaderTag(),
        new ImageTag(),
        new NewLineTag()
    };

    private readonly List<IToken> tokens = new();

    public List<IToken> Tokenize(string content)
    {
        var escapePositions = GetEscapePositions(content);
        var tagLines = IdentifyTags(content);

        foreach (var tagLine in tagLines) RemoveInvalidTags(tagLine, content);

        var escapedContent = RemoveEscapeCharacters(content, escapePositions, tagLines);

        var cleanTagTokens = tagLines.SelectMany(t => t).ToList();
        var tokenTree = MarkdownTokenTreeBuilder.BuildTokenTree(cleanTagTokens, escapedContent);

        tokens.AddRange(tokenTree);
        return tokenTree;
    }

    public List<IToken> GetTokens()
    {
        return tokens.ToList();
    }

    private List<List<TagToken>> IdentifyTags(string content)
    {
        var lines = new List<List<TagToken>>();
        var tagTokens = new List<TagToken>();
        for (var i = 0; i < content.Length; i++)
            foreach (var tag in tags)
            {
                var tagToken = new TagToken(tag) { Position = i };

                if (markdownRules.Rules.TryGetValue(tagToken.Tag.GetType(), out var rule))
                    if (rule.IsTag != null && !rule.IsTag(tagToken, content))
                        continue;

                if (content.ContainsSubstringOnIndex(tag.MdTag, i) && !content.IsEscaped(i))
                {
                    if (tag is NewLineTag)
                    {
                        if (tagTokens.FirstOrDefault()?.Tag is not HeaderTag)
                            tagTokens.Add(tagToken);
                        lines.Add(tagTokens);
                        tagTokens = new List<TagToken>();
                        break;
                    }

                    tagTokens.Add(tagToken);
                    i += tag.MdTag.Length - 1;
                    break;
                }

                if (tag.SelfClosing || !content.ContainsSubstringOnIndex(tag.MdClosingTag, i) || content.IsEscaped(i))
                    continue;
                tagTokens.Add(tagToken);
                i += tag.MdClosingTag.Length - 1;
                break;
            }

        lines.Add(tagTokens);

        return lines;
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


    private static string RemoveEscapeCharacters(string content, List<int> escapePositions, List<List<TagToken>> tags)
    {
        var sb = new StringBuilder(content);
        foreach (var pos in escapePositions.OrderByDescending(p => p)) sb.Remove(pos, 1);

        content = sb.ToString();

        foreach (var tagToken in tags.SelectMany(tagTokens => tagTokens))
            tagToken.Position -= escapePositions.Count(pos => pos < tagToken.Position);

        return content;
    }

    private void RemoveInvalidTags(List<TagToken> foundTags, string content)
    {
        var invalidTags = new HashSet<TagToken>();
        var tagStack = new Stack<TagToken>();
        var orderedTags = foundTags.OrderBy(t => t.Position).ToList();

        foreach (var tagToken in orderedTags)
        {
            var tag = tagToken.Tag;
            var isClosingTag = IsClosingTag(tag, tagToken.Position, content, tagStack);

            if (IsTagInvalid(tagToken, content, isClosingTag, orderedTags))
            {
                invalidTags.Add(tagToken);
                continue;
            }

            if (IsOpeningTag(tagStack, tagToken))
            {
                if (IsDisallowedChild(tagStack, tagToken))
                {
                    invalidTags.Add(tagToken);
                    continue;
                }

                if (!tagToken.Tag.SelfClosing)
                    tagStack.Push(tagToken);
            }
            else
            {
                HandleClosingTag(tagStack, tagToken, invalidTags);
            }
        }

        while (tagStack.Count > 0)
            invalidTags.Add(tagStack.Pop());

        foundTags.RemoveAll(t => invalidTags.Contains(t));
    }

    private static bool IsClosingTag(ITag tag, int position, string content, Stack<TagToken> tagStack)
    {
        return !tag.SelfClosing && content.ContainsSubstringOnIndex(tag.MdClosingTag, position)
                                && tagStack.Any(tagToken => tagToken.Tag.GetType() == tag.GetType());
    }

    private bool IsTagInvalid(TagToken tagToken, string content, bool isClosingTag, List<TagToken> orderedTags)
    {
        var tagType = tagToken.Tag.GetType();
        if (!markdownRules.Rules.TryGetValue(tagType, out var rule)) return false;
        return rule.IsValid != null && !rule.IsValid(tagToken, content, isClosingTag, orderedTags);
    }

    private static bool IsOpeningTag(Stack<TagToken> tagStack, TagToken tagToken)
    {
        return tagStack.Count == 0 || (!tagToken.Tag.Matches(tagStack.Peek().Tag) && 
                                       !tagStack.Any(t => t.Tag.Matches(tagToken.Tag)));
    }

    private static bool IsDisallowedChild(Stack<TagToken> tagStack, TagToken tagToken)
    {
        return tagStack.Count > 0 && tagStack.Peek().Tag.DisallowedChildren
            .Any(t => t.GetType() == tagToken.Tag.GetType());
    }

    private static void HandleClosingTag(Stack<TagToken> tagStack, TagToken tagToken, HashSet<TagToken> invalidTags)
    {
        var openingTag = tagStack.Pop();

        if (!openingTag.Tag.Matches(tagToken.Tag))
        {
            invalidTags.Add(openingTag);
            while (tagStack.Count > 0 && !tagStack.Peek().Tag.Matches(tagToken.Tag)) invalidTags.Add(tagStack.Pop());

            invalidTags.Add(tagToken);
        }

        var contentLength = tagToken.Position - (openingTag.Position + openingTag.Tag.MdTag.Length);
        if (contentLength > 0) return;
        invalidTags.Add(openingTag);
        invalidTags.Add(tagToken);
    }
}