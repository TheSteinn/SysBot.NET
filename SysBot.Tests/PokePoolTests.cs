using System.IO;
using FluentAssertions;
using Moq;
using PKHeX.Core;
using SysBot.Pokemon;
using SysBot.Pokemon.Structures;
using Xunit;

namespace SysBot.Tests
{
    public class PokePoolTests
    {
        [Fact]
        public void TestPool() => Test<PK8>();

        private static void Test<T>() where T : PKM, new()
        {
            // Ensure that we can get more than one Pokémon out of the pool.
            var pkmFileProvider = new Mock<PokemonFileProvider>();
            var pool = new PokemonPool<T>(new PokeTradeHubConfig(), pkmFileProvider.Object);
            var a = new T { Species = 5 };
            var b = new T { Species = 12 };
            pool.Add(a);
            pool.Add(b);

            pool.Count.Should().BeGreaterOrEqualTo(2);

            while (true)
            {
                if (ReferenceEquals(pool.GetRandomPoke(), a)) break;
            }

            while (true)
            {
                if (ReferenceEquals(pool.GetRandomPoke(), b)) break;
            }

            while (true)
            {
                if (ReferenceEquals(pool.GetRandomPoke(), a)) break;
            }

            true.Should().BeTrue();
        }

        [Fact]
        public void AttemptFetchFromFolderShouldReturnNullWhenException()
        {
            // Arrange
            var pkmFileProvider = new Mock<IFileProvider<PKM>>();
            pkmFileProvider.Setup(p => 
                p.GetFileAsData(It.IsAny<string>(), It.IsAny<string>())).Throws<IOException>();
            var pool = new PokemonPool<PK9>(new PokeTradeHubConfig(), pkmFileProvider.Object);
            
            // Act and Assert
            Assert.Null(pool.AttemptFetchFromFolder("folder", "file"));
        }
        
        [Fact]
        public void AttemptFetchFromFolderShouldReturnNull()
        {
            // Arrange
            var pkmFileProvider = new Mock<IFileProvider<PKM>>();
            pkmFileProvider.Setup(p =>
                p.GetFileAsData(It.IsAny<string>(), It.IsAny<string>())).Returns((PKM) null!);
            var pool = new PokemonPool<PK9>(new PokeTradeHubConfig(), pkmFileProvider.Object);
            
            // Act and Assert
            Assert.Null(pool.AttemptFetchFromFolder("folder", "file"));
        }
        
        [Fact]
        public void AttemptFetchFromFolderShouldReturnPkm()
        {
            // Arrange
            var expected = new PK9();
            var pkmFileProvider = new Mock<IFileProvider<PKM>>();
            pkmFileProvider.Setup(p =>
                p.GetFileAsData(It.IsAny<string>(), It.IsAny<string>())).Returns(expected);
            var pool = new PokemonPool<PK9>(new PokeTradeHubConfig(), pkmFileProvider.Object);
            
            // Act
            var actual = pool.AttemptFetchFromFolder("folder", "file");
            
            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }
    }
}
