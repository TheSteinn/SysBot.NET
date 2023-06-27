using System.IO;
using PKHeX.Core;

namespace SysBot.Pokemon.Structures;

public interface IFileProvider<T>
{
    T? GetFileAsData(string folder, string file);
}

public class PokemonFileProvider : IFileProvider<PKM>
{
    public PKM? GetFileAsData(string folder, string file)
    {
        if (!Directory.Exists(folder))
            return null;

        var path = Path.Combine(folder, file);
        if (!File.Exists(path))
            path += ".pk9";
        if (!File.Exists(path))
            return null;

        var data = File.ReadAllBytes(path);
        var prefer = EntityFileExtension.GetContextFromExtension(path);
        return EntityFormat.GetFromBytes(data, prefer);
    }
}