using TransactionQL.DesktopApp.Services;

namespace TransactionQL.DesktopApp.Tests.Services;

public class FuzzyMatcherTests
{
    private readonly FuzzyMatcher _matcher;

    public FuzzyMatcherTests()
    {
        _matcher = new();
    }

    [Fact]
    public void ItMatches_IdenticalStrings()
        => Assert.True(_matcher.IsMatch("Hello World", "Hello World"));

    [Theory]
    [InlineData("H", "Hello World")]
    [InlineData("He", "Hello World")]
    [InlineData("Hel", "Hello World")]
    public void ItMatches_IdenticalStarts(string search, string target)
        => Assert.True(_matcher.IsMatch(search, target));

    [Theory]
    [InlineData("o", "Hello World")]
    [InlineData("o ", "Hello World")]
    [InlineData("o W", "Hello World")]
    public void ItMatches_IdenticalMiddle(string search, string target)
        => Assert.True(_matcher.IsMatch(search, target));

    [Theory]
    [InlineData("d", "Hello World")]
    [InlineData("ld", "Hello World")]
    [InlineData("rld", "Hello World")]
    public void ItMatches_IdenticalEnds(string search, string target)
        => Assert.True(_matcher.IsMatch(search, target));

    [Theory]
    [InlineData("Hld", "Hello World")]
    [InlineData("l l", "Hello World")]
    [InlineData("eWod", "Hello World")]
    public void ItMatches_IndividualLettersInOrder(string search, string target)
        => Assert.True(_matcher.IsMatch(search, target));

    [Theory]
    [InlineData("hello", "Hello World")]
    [InlineData("world", "Hello World")]
    [InlineData("HeLlO wOrLd", "Hello World")]
    [InlineData("HELLO WORLD", "Hello World")]
    [InlineData("HELLO WORLD", "hello world")]
    [InlineData("hello world", "HELLO WORLD")]
    public void ItMatches_MismatchingCase(string search, string target)
        => Assert.True(_matcher.IsMatch(search, target));

    [Theory]
    [InlineData("See you soon", "Hello World")]
    [InlineData("x", "Hello World")]
    [InlineData(":)", "Hello World")]
    public void ItDoesNotMatch_DifferentWords(string search, string target)
        => Assert.False(_matcher.IsMatch(search, target));

    [Theory]
    [InlineData("", "")]
    [InlineData("", "Hello World")]
    [InlineData(null, "Hello World")]
    [InlineData(null, "")]
    public void ItMatches_EmptyStrings(string? search, string target)
        => Assert.True(_matcher.IsMatch(search, target));
}