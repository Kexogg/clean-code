using FluentAssertions;
using Markdown.Markdown;

namespace MarkdownTests;

public class MdTests
{
    private Md md;

    [SetUp]
    public void Setup()
    {
        md = new Md();
    }

    [Test]
    public void Render_ShouldReturnSimpleText()
    {
        const string text = "Hello, world!";
        md.Render(text).Should().Be(text);
    }

    [Test]
    public void Render_ShouldReturnCursiveText()
    {
        md.Render("Hello, _world_!").Should().Be("Hello, <em>world</em>!");
    }
}