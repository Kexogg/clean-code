using FluentAssertions;
using Markdown.Token;
using Markdown.Tags;
using NUnit.Framework.Interfaces;

namespace MarkdownTests
{
    [TestFixture]
    public class MarkdownTokenizerTests
    {
        private MarkdownTokenizer _tokenizer;

        [SetUp]
        public void SetUp()
        {
            _tokenizer = new MarkdownTokenizer();
        }

        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                foreach (var token in _tokenizer.GetTokens())
                {
                    DrawTokenTree(token);
                }
            }

            _tokenizer = null;
        }

        [Test]
        public void Tokenize_ShouldReturnTextToken_ForPlainText()
        {
            var content = "This is plain text.";
            var tokens = _tokenizer.Tokenize(content);

            tokens.Should().HaveCount(1);
            tokens[0].Should().BeOfType<TextToken>();
            tokens[0].TextContent.Should().Be("This is plain text.");
        }

        [Test]
        public void Tokenize_ShouldReturnStrongTagToken_ForStrongText()
        {
            var content = "__strong text__";
            var tokens = _tokenizer.Tokenize(content);

            tokens.Should().HaveCount(1);
            tokens[0].Should().BeOfType<TagToken>();
            tokens[0].Children![0].TextContent.Should().Be("strong text");
            ((TagToken)tokens[0]).Tag.Should().BeOfType<StrongTag>();
        }

        [Test]
        public void Tokenize_ShouldReturnMixedTokens_ForMixedContent()
        {
            var content = "This is __strong__ and _cursive_ text.";
            var tokens = _tokenizer.Tokenize(content);

            tokens.Should().HaveCount(5);
            tokens[0].Should().BeOfType<TextToken>().Which.TextContent.Should().Be("This is ");
            tokens[1].Should().BeOfType<TagToken>().Which.Tag.Should().BeOfType<StrongTag>();
            tokens[1].Children![0].TextContent.Should().Be("strong");
            tokens[2].Should().BeOfType<TextToken>().Which.TextContent.Should().Be(" and ");
            tokens[3].Should().BeOfType<TagToken>().Which.Tag.Should().BeOfType<CursiveTag>();
            tokens[3].Children![0].TextContent.Should().Be("cursive");
            tokens[4].Should().BeOfType<TextToken>().Which.TextContent.Should().Be(" text.");
        }

        [Test]
        public void Tokenize_ShouldReturnNestedTokens_ForNestedContent()
        {
            var content = "__strong _and cursive_ text__";
            var tokens = _tokenizer.Tokenize(content);

            tokens.Should().HaveCount(1);
            tokens[0].Should().BeOfType<TagToken>().Which.Tag.Should().BeOfType<StrongTag>();
            tokens[0].Children![0].Should().BeOfType<TextToken>().Which.TextContent.Should().Be("strong ");
            tokens[0].Children![1].Should().BeOfType<TagToken>().Which.Tag.Should().BeOfType<CursiveTag>();
            tokens[0].Children![1].Children![0].Should().BeOfType<TextToken>().Which.TextContent.Should().Be("and cursive");
            tokens[0].Children![2].Should().BeOfType<TextToken>().Which.TextContent.Should().Be(" text");
        }

        /*[Test]
        public void Tokenize_ShouldReturnImageTagToken_ForImage()
        {
            var content = "![src](image.jpg)";
            var tokens = _tokenizer.Tokenize(content);

            tokens.Should().HaveCount(1);
            tokens[0].Should().BeOfType<TagToken>();
            tokens[0].TextContent.Should().Be("image.jpg");
            ((TagToken)tokens[0]).Tag.Should().BeOfType<ImageTag>();
        }*/

        private void DrawTokenTree(IToken token, string indent = " - ")
        {
            switch (token)
            {
                case TextToken textToken:
                    TestContext.Out.WriteLine($"{indent}Text: \"{textToken.TextContent}\"");
                    break;
                case TagToken tagToken:
                    TestContext.Out.WriteLine($"{indent}Tag: {tagToken.Tag.GetType()} {tagToken.TextContent}");
                    break;
            }

            if (token.Children != null)
            {
                foreach (var child in token.Children)
                {
                    DrawTokenTree(child, "  " + indent);
                }
            }
        }
    }
}