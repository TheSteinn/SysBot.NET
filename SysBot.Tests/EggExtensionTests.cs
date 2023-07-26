using FluentAssertions;
using PKHeX.Core;
using SysBot.Pokemon;
using SysBot.Pokemon.Helpers;
using Xunit;

namespace SysBot.Tests;

public class EggExtensionTests
{
    static EggExtensionTests() => AutoLegalityWrapper.EnsureInitialized(new LegalitySettings());
    
    [Theory]
    [InlineData("Pikachu", "SUCCESS", true)]
    [InlineData("", "The provided text was invalid. Only provide the species name", false)]
    [InlineData("OneLine\nTwoLines", "The provided text was invalid. Only provide the species name", false)]
    public void CanMapToTemplate(string content, string expectedMsg, bool isSuccess)
    {
        // Act
        var actual = EggExtension<PK9>.MapToTemplate(content, out var msg, out var set);

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

    [Theory]
    [InlineData("Charmander", true)]
    [InlineData("Eevee", true)]
    [InlineData("Gastly", true)]
    [InlineData("Pichu", true)]
    [InlineData("Quaxly", true)]
    [InlineData("Charizard", false)]
    [InlineData("Mew", false)]
    public void EggTestPK9(string content, bool success)
    {
        CanConvertToEgg<PK9>(content, success);
    }
    
    [Theory]
    [InlineData("Charmander", true)]
    [InlineData("Eevee", true)]
    [InlineData("Gastly", true)]
    [InlineData("Pichu", true)]
    [InlineData("Quaxly", true)]
    [InlineData("Charizard", false)]
    [InlineData("Mew", false)]
    public void EggTestPK8(string content, bool success)
    {
        CanConvertToEgg<PK8>(content, success);
    }
    
    private void CanConvertToEgg<T>(string content, bool success) where T : PKM, new()

    {
        var template = EggExtension<T>.MapToTemplate(content, out _, out _);
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK9>();
        var pk = sav.GetLegal(template!, out _);
        EggExtension<T>.ConvertToEgg(pk, template!);

        pk.Should().NotBeNull();

        var la = new LegalityAnalysis(pk);
        pk.IsEgg.Should().BeTrue("it should be an egg");
        if (success) la.Valid.Should().BeTrue("it should be legal");
        else la.Valid.Should().BeFalse();
        if (success) pk.CanBeTraded().Should().BeTrue("it should be tradeable");
    }
}