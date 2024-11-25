using FluentAssertions;
using Markdown.Markdown;

namespace MarkdownTests;

public class MarkdownTests
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
        md.Render("Hello, world!").Should().Be("Hello, world!");
    }

    [Test]
    public void Render_ShouldReturnCursiveText()
    {
        md.Render("Hello, _world_!").Should().Be("Hello, <em>world</em>!");
    }

    [TestCase("Hello, __world__!", "Hello, <strong>world</strong>!", TestName = "Simple strong")]
    [TestCase("Hello, world! ____", "Hello, world! ____", TestName = "Empty strong")]
    public void Render_ShouldReturnStrongText(string input, string expected)
    {
        md.Render(input).Should().Be(expected);
    }

    [Test]
    public void Render_ShouldReturnHeadingText()
    {
        md.Render("#Hello, world!").Should().Be("<h1>Hello, world!</h1>");
    }

    [Test]
    public void Render_ShouldReturnImage()
    {
        md.Render("(Hello, image!)[img]").Should().Be("<img alt=\"Hello, image!\" src=\"img\"/>");
    }

    [Test]
    public void Render_ShouldEscapeSpecialCharacters()
    {
        md.Render(@"Hello, \_world\_!").Should().Be("Hello, _world_!");
    }

    [Test]
    [TestCase("#Hello, __world__!", "<h1>Hello, <strong>world!</strong></h1>")]
    [TestCase("__Hello, _world_!__", "<strong>Hello, <em>world</em>!</strong>")]
    public void Render_ShouldReturnNestedFormatting(string input, string expected)
    {
        md.Render(input).Should().Be(expected);
    }
}