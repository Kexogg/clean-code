using Markdown.Tags;
using Markdown.Tokenizer.Tokens;

namespace Markdown.Tokenizer;

public static class MarkdownTokenTreeBuilder
{
    /// <summary>
    /// Собирает дерево токенов, исходя из тегов в тексте
    /// </summary>
    /// <param name="availableTokens">Список валидных тегов в тексте</param>
    /// <param name="content">Исходная строка</param>
    /// <returns>Дерево токенов</returns>
    public static List<IToken> BuildTokenTree(List<TagToken> availableTokens, string content)
    {
        var tree = new List<IToken>();
        var tagStack = new Stack<TagToken>();
        var lastTagEnd = 0;

        if (availableTokens.Count == 0)
        {
            tree.Add(new TextToken(content));
            return tree;
        }

        foreach (var tag in availableTokens)
            //If tag is first in the content
            if (tagStack.Count == 0)
            {
                if (tag.Position > lastTagEnd)
                {
                    var textToken = new TextToken(content.Substring(lastTagEnd, tag.Position - lastTagEnd));
                    tree.Add(textToken);
                }

                lastTagEnd = tag.Position + tag.Tag.MdTag.Length;

                if (tag.Tag.SelfClosing)
                {
                    tree.Add(tag);
                }
                else
                {
                    tagStack.Push(tag);
                }
            }
            //If tag is self-closing
            else if (tag.Tag.SelfClosing)
            {
                if (tag.Position > lastTagEnd)
                {
                    var textToken = new TextToken(content.Substring(lastTagEnd, tag.Position - lastTagEnd));
                    tagStack.Peek().Children.Add(textToken);
                }

                tagStack.Peek().Children.Add(tag);
                lastTagEnd = tag.Position + tag.Tag.MdTag.Length;
            }
            //If tag is closing
            else if (tagStack.Peek().Tag.GetType() == tag.Tag.GetType())
            {
                var tagToken = tagStack.Pop();
                //handle image
                if (tagToken.Tag is ImageTag imageTag)
                {
                    var imageToken = new TagToken(imageTag)
                    {
                        Position = tagToken.Position,
                        Attributes =
                            ImageTag.GetHtmlTadAttributes(
                                content.Substring(lastTagEnd - tagToken.Tag.MdTag.Length,
                                tag.Position - lastTagEnd + tagToken.Tag.MdTag.Length + 1))
                    };
                    tree.Add(imageToken);
                    lastTagEnd = tag.Position + tag.Tag.MdClosingTag.Length;
                    continue;
                }

                var textToken = new TextToken(content.Substring(lastTagEnd,
                    tag.Position - lastTagEnd));

                tagToken.Children.Add(textToken);
                if (tagStack.Count == 0)
                    tree.Add(tagToken);
                else
                    tagStack.Peek().Children.Add(tagToken);

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

        if (lastTagEnd < content.Length)
            tree.Add(new TextToken(content[lastTagEnd..]));
        return tree;
    }
}