using FluentAssertions;
using SysBot.Pokemon;
using SysBot.Pokemon.Discord;
using Xunit;

namespace SysBot.Tests;

public class EggHelperTests
{
    static EggHelperTests() => AutoLegalityWrapper.EnsureInitialized(new LegalitySettings());
    
    [Theory]
    [InlineData("Pikachu", "SUCCESS", true)]
    [InlineData("", "The provided text was invalid. Only provide the species name", false)]
    [InlineData("OneLine\nTwoLines", "The provided text was invalid. Only provide the species name", false)]
    public void MapToTemplate(string content, string expectedMsg, bool isSuccess)
    {
        // Act
        var actual = EggHelper.MapToTemplate(content, out var msg, out var set);

        if (isSuccess)
        {
            actual.Should().NotBeNull();
            set.Should().NotBeNull();
        }
        else
        {
            actual.Should().BeNull();
            set.Should().BeNull();
        }
        msg.Should().Be(expectedMsg);
    }
}