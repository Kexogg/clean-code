using Markdown.Extensions;
using Markdown.Tags;

namespace Markdown.Tokenizer.Rules;

/// <summary>
/// Правила парсинга для тегов Markdown
/// </summary>
public class MarkdownRules
{
    public Dictionary<Type, Rule> Rules = new()
    {
        [typeof(CursiveTag)] = new Rule
        {
            IsValid = (tagToken, content, isClosingTag, orderedTags) =>
            {
                return !IsTagInWordWithDigits(tagToken, content) &&
                       !IsStartOrEndWhitespace(tagToken, content, isClosingTag) &&
                       !IsEmptyContent(tagToken, content, orderedTags) &&
                       !IsTagBorderInWord(tagToken, content, orderedTags);
            }
        },
        [typeof(StrongTag)] = new Rule
        {
            IsValid = (tagToken, content, isClosingTag, orderedTags) =>
            {
                return !IsTagInWordWithDigits(tagToken, content) && 
                       !IsStartOrEndWhitespace(tagToken, content, isClosingTag) &&
                       !IsEmptyContent(tagToken, content, orderedTags) &&
                       !IsTagBorderInWord(tagToken, content, orderedTags);
            }
        },
    };

    private static bool IsEmptyContent(TagToken openingTag, string content, List<TagToken> orderedTags)
    {
        var tagIndex = orderedTags.IndexOf(openingTag);
        if (tagIndex + 1 < orderedTags.Count)
        {
            var closingTag = orderedTags[tagIndex + 1];
            if (openingTag.Tag.Matches(closingTag.Tag))
            {
                var contentLength = closingTag.Position - (openingTag.Position + openingTag.Tag.MdTag.Length);
                return contentLength <= 0;
            }
        }

        return false;
    }

    
    private static bool IsStartOrEndWhitespace(TagToken tagToken, string content, bool isClosingTag)
    {
        var pos = tagToken.Position;
        var tagLen = (isClosingTag ? tagToken.Tag.MdClosingTag : tagToken.Tag.MdTag).Length;
        var tagEnd = pos + tagLen - 1;

        var charBefore = pos > 0 ? content[pos - 1] : '\0';
        var charAfter = tagEnd + 1 < content.Length ? content[tagEnd + 1] : '\0';

        return char.IsWhiteSpace(isClosingTag ? charBefore : charAfter);
    }

    private static bool IsTagBorderInWord(TagToken tag, string content, List<TagToken> orderedTags)
    {
        var isClosing = content.ContainsSubstringOnIndex(tag.Tag.MdClosingTag, tag.Position);
        var tagLength = (isClosing ? tag.Tag.MdClosingTag : tag.Tag.MdTag).Length;
        var tagEnd = tag.Position + tagLength - 1;
        //next symbol
        if (tagEnd + 1 == content.Length || char.IsWhiteSpace(content[tagEnd + 1]))
            return false;
        //previous symbol
        var previousTag = orderedTags.FirstOrDefault(t => t.Position + t.Tag.MdTag.Length == tag.Position);
        //TODO: fix edge case
        if (tag.Position == 0 || char.IsWhiteSpace(content[tag.Position - 1]) ||
            previousTag?.Position + previousTag?.Tag.MdTag.Length == tag.Position)
            return false;


        var i = tagEnd + 1;
        while (content.Length > i && !char.IsWhiteSpace(content[i]))
        {
            if (content.ContainsSubstringOnIndex(tag.Tag.MdClosingTag, i))
            {
                return false;
            }

            i++;
        }

        i = tag.Position - 1;
        while (i > 0 && !char.IsWhiteSpace(content[i]))
        {
            if (content.ContainsSubstringOnIndex(tag.Tag.MdTag, i))
            {
                return false;
            }

            i--;
        }

        return true;
    }
    
    private static bool IsTagInWordWithDigits(TagToken tagToken, string content)
    {
        var isClosing = content.ContainsSubstringOnIndex(tagToken.Tag.MdClosingTag, tagToken.Position);
        var tagLength = (isClosing ? tagToken.Tag.MdClosingTag : tagToken.Tag.MdTag).Length;
        var tagEnd = tagToken.Position + tagLength - 1;
        //next symbol
        if (tagEnd + 1 == content.Length || char.IsWhiteSpace(content[tagEnd + 1]))
            return false;
        //previous symbol
        if (tagToken.Position == 0 || char.IsWhiteSpace(content[tagToken.Position - 1]))
            return false;


        var i = tagEnd + 1;
        while (content.Length > i && !char.IsWhiteSpace(content[i]))
        {
            if (char.IsDigit(content[i]))
            {
                return true;
            }

            i++;
        }

        i = tagToken.Position - 1;
        while (i > 0 && !char.IsWhiteSpace(content[i]))
        {
            if (char.IsDigit(content[i]))
            {
                return true;
            }

            i--;
        }

        return false;
    }
}